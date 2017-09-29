using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;

using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using System.Web.Services.Protocols;
using System.Globalization;


namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    public class MI_EnvisionIntegration:CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionIntegration
    {

        #region Constants
 
        /// <summary>
        /// External ASR Names
        /// </summary>
        internal new static class AppServerRuleName
        {
            //internal const string OpportunityPostSaleQuote = "PAHB Opportunity Post Sale Quote";
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            internal const string Opportunity = "PAHB Opportunity";
            internal const string EnvisionIntegrationTransactional = "PAHB Envision Integration Transactional";
        }

        #endregion


        #region Statics
        private static bool sendContractsIsRunning = false;  //flags the current running state of the Send Contracts process
        private static bool sendInventoryIsRunning = false;  //flags the current running state of the Send Inventory process

        /// <summary>
        /// Gets the current time.  Typically used for LCS communication tests
        /// </summary>
        /// <returns>Current Date and Time</returns>
        private static DateTime GetServerTime()
        {
            return DateTime.Now;
        }
        #endregion


        private IRSystem7 m_rdaSystem;
        private DataAccess m_objLib;

        // only use the public implementations
        private ILangDict m_rdaLangDict;
        private Configuration m_config;
        private Logging m_log;
        private SyncProxy m_sync;
        private ValidateXml m_xmlValidation;

        private SyncProxy syncProxy; //Sync class proxy to create and edit Env_Sync records.


        #region Types
        /// <summary>
        /// Envision record status constants
        /// </summary>
        private enum EnvisionRecordStatus
        {
            New,
            Active,
            Inactive
        }

        /// <summary>
        /// As both the Home and Buyer need to be deactivated together
        /// this tracks if a failure occurs so the deactivation and be retried.
        /// </summary>
        [Flags]
        private enum EnvisionDeactivationState
        {
            Active = 0,             // Buyer and Home have not been deactivated in Envision yet.
            HomeDeactivated = 1,    // The Home has been successfully deactivated but the Buyer has not.
            BuyerDeactivated = 2,   // The Buyer has been successfully deactivated but the Home has not.
            Deactivated = 3         // Both the Home and Buyer have been deactivated.
        }

        /// <summary>
        /// Entity used to track Pivotal record status and Envision syncronization failures
        /// </summary>
        private struct StateTracker
        {
            /// <summary>
            /// Pivotal record Id (database unique key)
            /// </summary>
            internal byte[] Id;

            /// <summary>
            /// The Rn_Update state of the record when it was queried from the database
            /// </summary>
            internal byte[] updateId;

            /// <summary>
            /// Flag that specifies if an Envision operation that this record was involved in failed.
            /// </summary>
            internal bool syncFailure;
        }
        #endregion

        /// <summary>
        /// Creates a Dictionary of state tracker entities from a recordset.
        /// </summary>
        /// <param name="recordset">The recordset from which to create the entities</param>
        /// <param name="primaryKeyFieldName">The record id or primary key field name</param>
        /// <param name="rnUpdateFieldName">The Rn_Update id field name</param>
        /// <returns>A typed Dictionary instance filled with StateTracker entities</returns>
        private Dictionary<string, StateTracker> CreateStateTrackers(Recordset recordset, string primaryKeyFieldName, string rnUpdateFieldName)
        {
            Dictionary<string, StateTracker> stateTrackers = new Dictionary<string, StateTracker>();
            try
            {
                if (recordset.RecordCount > 0)
                {
                    recordset.MoveFirst();
                    while (!recordset.EOF)
                    {
                        // create a new StateTracker and set the properties appropriatly
                        StateTracker stateTracker = new StateTracker();
                        stateTracker.Id = (byte[])recordset.Fields[primaryKeyFieldName].Value;
                        stateTracker.updateId = (byte[])recordset.Fields[rnUpdateFieldName].Value;

                        // syncFailure must be initialized to false to indicate there are no failures yet.
                        stateTracker.syncFailure = false;

                        stateTrackers.Add(this.m_rdaSystem.IdToString(stateTracker.Id), stateTracker);
                        recordset.MoveNext();
                    }
                }
            }
            finally
            {
                recordset.Close(); // cleanup recordset
            }
            return stateTrackers;
        }

        /// <summary>
        /// Creates a SoapException from and Envision Output entity
        /// </summary>
        /// <param name="output">An Envision Output entity that must have its status set to Failed</param>
        /// <returns>A new SoapException</returns>
        private SoapException CreateSoapException(EnvisionXsdGenerated.Output output)
        {
            // throw an exception if the status flag is not set to failure
            if (output.Status == OutputStatus.Success)
                throw new PivotalApplicationException((string)this.LangDictionary.GetText("ExceptionUnexpectedOutputState"));

            // set default message
            string msg = (string)this.LangDictionary.GetText("ExceptionWebServiceFail");

            // find the failure message.
            for (int i = 0; i < output.Messages.Length; i++)
            {
                if (output.Messages[i].Type == OutputMessageType.Error)
                {
                    // format the message
                    msg = msg.Replace("%1", output.Messages[i].Value);
                    break;
                }
            }

            return new SoapException(msg, SoapException.ClientFaultCode);
        }

        /// <summary>
        /// Create an Envision Output instance from an XmlNode
        /// </summary>
        /// <param name="xml">Root node of an Envision Output xml structure</param>
        /// <returns>An Envision Output instance</returns>
        private static EnvisionXsdGenerated.Output GetOutput(System.Xml.XmlNode xml)
        {
            object instance = null;

            // create a reader for reading the xml
            using (System.IO.StringReader reader = new System.IO.StringReader(xml.OuterXml))
            {
                // create a serializer and deserialize
                System.Xml.Serialization.XmlSerializer s = new System.Xml.Serialization.XmlSerializer(typeof(EnvisionXsdGenerated.Output));
                instance = s.Deserialize(reader);
            }

            return (EnvisionXsdGenerated.Output)instance;
        }

        /// <summary>
        /// This subroutine sets the Rich Client System.
        /// </summary>
        public new void SetSystem(RSystem pSystem)
        {
            try
            {
                if (pSystem == null)
                    throw new ArgumentNullException("pSystem");

                this.m_rdaSystem = (IRSystem7)pSystem;
                this.syncProxy = new SyncProxy(m_rdaSystem);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, this.m_rdaSystem);
            }
        }

        /// <summary>
        /// Send a new Envision Buyer to Envision
        /// </summary>
        /// <param name="opportunityId">Opportunity record from which to get the Contract information</param>
        /// <param name="contactId">Contact record id from which to get the Buyer Information</param>
        /// <param name="buyerWebService">An initialized Envision Buyer web service</param>
        /// <remarks>Once the Buyer is successfully sent, the Contract sync record is set to Successful</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void CreateEnvisionBuyer(object opportunityId, object contactId, Envision.DesignCenterManager.Buyer.BuyerWebService buyerWebService)
        {
            try
            {
                
                // get the state id for the contact record
                byte[] contactUpdateId = (byte[])this.m_rdaSystem.Tables[ContactData.TableName].Fields[ContactData.RnUpdateField].Index(contactId);

                // get the product id
                byte[] productId = (byte[])this.m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.LotIdField].Index(opportunityId);

                // create all state trackers.
                Recordset coBuyerContactRecords = this.PivotalDataAccess.GetRecordset(ContactData.QueryCoBuyerContactsForContact, 1, contactId, ContactData.ContactIdField, ContactData.RnUpdateField);
                Dictionary<string, StateTracker> coBuyerContactStates = CreateStateTrackers(coBuyerContactRecords, ContactData.ContactIdField, ContactData.RnUpdateField);

                Recordset loanProfileRecords = this.PivotalDataAccess.GetRecordset(LoanProfileData.QueryLoanProfilesForQuote, 1, opportunityId, LoanProfileData.LoanProfileIdField, LoanProfileData.RnUpdateField);
                Dictionary<string, StateTracker> loanProfileStates = CreateStateTrackers(loanProfileRecords, LoanProfileData.LoanProfileIdField, LoanProfileData.RnUpdateField);

                Recordset loanRecords = this.PivotalDataAccess.GetRecordset(LoanData.QueryLoansForQuote, 1, opportunityId, LoanData.LoanIdField, LoanData.RnUpdateField);
                Dictionary<string, StateTracker> loanStates = CreateStateTrackers(loanRecords, LoanData.LoanIdField, LoanProfileData.RnUpdateField);

                // create the homeNumber
                BuilderClasses.EnvisionBuilder builder = new BuilderClasses.EnvisionBuilder(this);
                string homeNumber = builder.GenerateHomeNumber(opportunityId, productId);

                // variables for returning the ids from the records used to generate and Envision Buyer
                byte[][] coBuyerContactIds = new byte[][] { };
                byte[][] loanProfileIds = new byte[][] { };
                byte[] loanId = new byte[] { };

                // Create the Envision Buyer entity
                Buyer buyer = builder.GetBuyer(opportunityId, contactId, out coBuyerContactIds, out loanProfileIds, out loanId);

                // turn the Envision buyer into and xml document
                XmlDocument buyerDoc = new XmlDocument();
                buyerDoc.LoadXml(builder.SerializeToXmlString(buyer));

                // validate the xml document agains the schema
                XmlValidation.Buyer(buyerDoc);

                // create the location id
                byte[] neighborhoodId = (byte[])this.m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.NeighborhoodIdField].Index(opportunityId);
                byte[] divisionId = (byte[])this.m_rdaSystem.Tables[NeighborhoodData.TableName].Fields[NeighborhoodData.DivisionIdField].Index(neighborhoodId);

                // this is always the lowest organization level 'Division'
                //2007-12-17 AB Commented out to call custom LocationReferenceClass
                //XmlNode buyerLocationXml = new LocationReferenceBuilder(LocationReferenceType.Division, divisionId, this.Config, this.m_rdaSystem).ToXML();
                XmlNode buyerLocationXml = new MI_LocationReferenceBuilder(MI_LocationReferenceType.Division, divisionId, this.Config, this.m_rdaSystem).ToXML();

                // validate location reference
                XmlValidation.LocationReference(buyerLocationXml);


                // execute the create web service
                XmlNode xmlReturn = buyerWebService.CreateBuyer(buyerLocationXml, buyerDoc, homeNumber, Config.AutoActivateBuyer);

                // validate the output
                XmlValidation.Output(xmlReturn);

                // if Envision returned a failure, throw a SoapException
                EnvisionXsdGenerated.Output output = GetOutput(xmlReturn);
                if (output.Status != OutputStatus.Success) throw CreateSoapException(output);


                // Note - as some records can be used in other Envision operations it is important not
                // to reset the sync state as it could falsely identify the record a synced.  Instead
                // only set the sync state if no sync state record already exists.

                // set Contact sync record if it does not yet exist
                if (this.m_rdaSystem.Tables[EnvSyncData.TableName].Fields[EnvSyncData.ContactIdField].Find(contactId) == DBNull.Value)
                    Sync.SetContactState(contactId, contactUpdateId);


                foreach (StateTracker coBuyerContactState in coBuyerContactStates.Values)
                {
                    // set coBuyer sync record if it does not yet exist
                    if (this.m_rdaSystem.Tables[EnvSyncData.TableName].Fields[EnvSyncData.ContactIdField].Find(coBuyerContactState.Id) == DBNull.Value)
                        Sync.SetContactState(coBuyerContactState.Id, coBuyerContactState.updateId);
                }


                foreach (StateTracker loanProfileState in loanProfileStates.Values)
                {
                    //set Loan Profile sync record if it does not yet exist
                    if (this.m_rdaSystem.Tables[EnvSyncData.TableName].Fields[EnvSyncData.ContactIdField].Find(loanProfileState.Id) == DBNull.Value)
                        Sync.SetLoanProfileState(loanProfileState.Id, loanProfileState.updateId);
                }


                foreach (StateTracker loanState in loanStates.Values)
                {
                    //set Loan State sync record if it does not yet exist
                    if (this.m_rdaSystem.Tables[EnvSyncData.TableName].Fields[EnvSyncData.LoanIdField].Find(loanState.Id) == DBNull.Value)
                        Sync.SetLoanState(loanState.Id, loanState.updateId);
                }

                // set Contract state to successful
                Sync.SetContractState(opportunityId, CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.Sync.SyncState.Success);  //DO NOT update Rn_Update_Copy field

            }
            catch (SoapException ex)
            {
                // wrap with better description
                throw new SoapException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.CreateEnvisionBuyer), ex.Code, ex);
            }
            catch (System.Net.WebException ex)
            {
                //wrap exception with better description
                throw new System.Net.WebException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.UpdateEnvisionHome), ex);
            }
            catch (Exception ex)
            {
                // wrap with better description
                throw new PivotalApplicationException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.CreateEnvisionBuyer), ex);
            }
        }
        /// <summary>
        /// Sends an Envision Buyer Update
        /// </summary>
        /// <param name="opportunityId">The id of the Contract involved</param>
        /// <param name="contactId">The id of the Contact involved</param>
        /// <param name="contactStates">The record state management entities for all Contact records</param>
        /// <param name="loanProfileStates">The record state management entities for all Loan Profile records</param>
        /// <param name="loanStates">The record state management entities for all Loan States</param>
        /// <param name="buyerWebService">The web service instance on which to send the Buyer Update</param>
        private void UpdateEnvisionBuyer(object opportunityId, object contactId, Dictionary<string, StateTracker> contactStates, Dictionary<string, StateTracker> loanProfileStates, Dictionary<string, StateTracker> loanStates, Envision.DesignCenterManager.Buyer.BuyerWebService buyerWebService)
        {
            // holds the ids of the records that have been used to send the Buyer Update
            byte[][] coBuyerContactIds = new byte[][] { };
            byte[][] loanProfileIds = new byte[][] { };
            byte[] loanId = new byte[] { };

            try
            {

                // Creates the Envision Buyer entity returning the record ids used.
                BuilderClasses.EnvisionBuilder builder = new BuilderClasses.EnvisionBuilder(this);
                Buyer buyer = builder.GetBuyer(opportunityId, contactId, out coBuyerContactIds, out loanProfileIds, out loanId);

                // turn the Envision Buyer into xml
                XmlDocument buyerDoc = new XmlDocument();
                buyerDoc.LoadXml(builder.SerializeToXmlString(buyer));

                // validate the Buyer agains the Envision schema
                XmlValidation.Buyer(buyerDoc);

                // generate the location xml
                byte[] neighborhoodId = (byte[])this.m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.NeighborhoodIdField].Index(opportunityId);
                byte[] divisionId = (byte[])this.m_rdaSystem.Tables[NeighborhoodData.TableName].Fields[NeighborhoodData.DivisionIdField].Index(neighborhoodId);

                // this is always the lowest organization level 'Division'
                //2007-12-17 AB commented out to call custom MI location reference builder
                //XmlNode locationXml = new LocationReferenceBuilder(LocationReferenceType.Division, divisionId, this.Config, this.m_rdaSystem).ToXML();
                XmlNode locationXml = new MI_LocationReferenceBuilder(MI_LocationReferenceType.Division, divisionId, this.Config, this.m_rdaSystem).ToXML();


                // validate location reference
                XmlValidation.LocationReference(locationXml);

                // execute the web service call
                XmlNode returnXml = buyerWebService.UpdateBuyer(locationXml, buyerDoc);

                // validate the returned xml agains the Envision schema
                XmlValidation.Output(returnXml);

                // turn the returned xml into an Envision Output entity
                EnvisionXsdGenerated.Output output = GetOutput(returnXml);

                // if Envision returns an error, turn the error into an Exception and throw.
                if (output.Status != OutputStatus.Success) throw CreateSoapException(output);
            }
            catch (SoapException se)
            {
                // Due to the processing model, an 'active' approach is need to track failures so that
                // record sync state is set correctly at the end.  The following accomplishes this.
                // Note - Only SoapExceptions are treated as errors that don't stop the processing of
                //        the next record.  All other Exceptions stop all processing.
                //      - If there is a SoapException, process all subsequet Buyer Updates in order to
                //        keep Envision up to date as possible eventhough all the records can potentially
                //        be processed again next iteration to recover from the failure.

                //fail Contact
                string pivotalId = this.m_rdaSystem.IdToString((byte[])contactId);


                // Note - Dictionary entities seem to return copies, not references, to the
                //        item instance.  Therefore any changes to the item instance must be
                //        added back to the Dicitonary.

                //if we are tracking this Contact's state.  Set it to failed
                if (contactStates.ContainsKey(pivotalId))
                {
                    StateTracker contactState = contactStates[pivotalId];
                    contactState.syncFailure = true;
                    contactStates[pivotalId] = contactState;
                }

                //fail Co-Buyer Contacts
                for (int i = 0; i < coBuyerContactIds.Length; i++)
                {
                    //if we are tracking this Co-Buyer's state.  Set it to failed.
                    pivotalId = this.m_rdaSystem.IdToString(coBuyerContactIds[i]);
                    if (contactStates.ContainsKey(pivotalId))
                    {
                        StateTracker contactState = contactStates[pivotalId];
                        contactState.syncFailure = true;
                        contactStates[pivotalId] = contactState;
                    }
                }

                //fail LoanProfiles
                for (int i = 0; i < loanProfileIds.Length; i++)
                {
                    //if we are tracking this LoanProfile's state.  Set it to failed.
                    pivotalId = this.m_rdaSystem.IdToString(loanProfileIds[i]);
                    if (loanProfileStates.ContainsKey(pivotalId))
                    {
                        StateTracker loanProfileState = loanProfileStates[pivotalId];
                        loanProfileState.syncFailure = true;
                        loanProfileStates[pivotalId] = loanProfileState;
                    }
                }

                //fail Loan
                if (loanId.Length > 0)
                {
                    //if we are tacking this Loan's state.  Set it to failed.
                    pivotalId = this.m_rdaSystem.IdToString(loanId);
                    if (loanStates.ContainsKey(pivotalId))
                    {
                        StateTracker loanState = loanStates[pivotalId];
                        loanState.syncFailure = true;
                        loanStates[pivotalId] = loanState;
                    }
                }

                // once all the failures have been set, re-throw the Exception to be handled
                // further up in processing.
                throw new SoapException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.UpdateEnvisionBuyer), se.Code, se);
            }
            catch (System.Net.WebException ex)
            {
                //wrap exception with better description
                throw new System.Net.WebException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.UpdateEnvisionHome), ex);
            }
            catch (Exception ex)
            {
                // wrap with better description
                throw new PivotalApplicationException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.UpdateEnvisionBuyer), ex);
            }
        }
        /// <summary>
        /// Create an Envision Home in Envision
        /// </summary>
        /// <param name="opportunityId">Opportunity record from which to get the Contract data</param>
        /// <param name="productId">Product record from which to get the Homesite data</param>
        /// <param name="homeWebService">An initialized web service</param>
        /// <remarks>Once the Home is successfully created, a sync record is added to Pivotal with a Pending status as
        /// the Buyer still has to be sent</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void CreateEnvisionHome(object opportunityId, object productId, Envision.DesignCenterManager.Home.HomeWebService homeWebService)
        {

            try
            {
                byte[] productUpdateId = (byte[])this.m_rdaSystem.Tables[ProductData.TableName].Fields[ProductData.RnUpdateField].Index(productId);
                byte[] opportunityUpdateId = (byte[])this.m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.RnUpdateField].Index(opportunityId);

                // Create the Envision Home entity
                BuilderClasses.EnvisionBuilder builder = new BuilderClasses.EnvisionBuilder(this);
                EnvisionXsdGenerated.Home home = builder.GetHome(opportunityId, productId, Config.EnvisionNHTNumber);

                // turn the Envision Home into xml
                XmlDocument homeDoc = new XmlDocument();
                homeDoc.LoadXml(builder.SerializeToXmlString(home));

                // validate the Envision Home xml agains the schema
                XmlValidation.Home(homeDoc);

                // create the location xml
                byte[] planId = (byte[])this.m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.PlanNameIdField].Index(opportunityId);
                object releaseId = m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.NBHDPhaseIdField].Index(opportunityId);

                // this is always the lowest inventory level 'Plan'
                //2007-12-17 AB commented out to call custom MI location reference builder
                //lowest level for MI is the elevation
                //XmlNode locationXml = new LocationReferenceBuilder(LocationReferenceType.Plan, planId, releaseId, Config, m_rdaSystem).ToXML();
                XmlNode locationXml = new MI_LocationReferenceBuilder(MI_LocationReferenceType.Elevation, planId, releaseId, Config, m_rdaSystem).ToXML();

                // validate location reference
                XmlValidation.LocationReference(locationXml);

                // execute the web service.
                System.Xml.XmlNode returnXml = homeWebService.CreateHome(locationXml, homeDoc);

                // validate the output xml agains the schema
                XmlValidation.Output(returnXml);

                // turn any failure into a SoapException and throw
                EnvisionXsdGenerated.Output output = GetOutput(returnXml);
                if (output.Status != OutputStatus.Success) throw CreateSoapException(output);


                //Add opportunity to sync table with status pending
                Sync.SetContractState(opportunityId, false, opportunityUpdateId, CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.Sync.SyncState.Pending);

                // set the Product state only if the sync record does not already exist.  If it does exist, do nothing as we don't want
                // to interfier with any other syncing that may use this record.
                if (this.m_rdaSystem.Tables[EnvSyncData.TableName].Fields[EnvSyncData.ProductIdField].Find(productId) == DBNull.Value)
                    Sync.SetHomeState(productId, productUpdateId);

            }
            catch (SoapException ex)
            {
                //wrap the exception with a better description
                throw new SoapException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.CreateEnvisionHome), ex.Code, ex);
            }
            catch (System.Net.WebException ex)
            {
                //wrap exception with better description
                throw new System.Net.WebException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.UpdateEnvisionHome), ex);
            }
            catch (Exception ex)
            {
                //wrap the exxception with a better description
                throw new PivotalApplicationException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.CreateEnvisionHome), ex);
            }
        }
        private void UpdateEnvisionHome(object opportunityId, object productId, object releaseId, object planId, Envision.DesignCenterManager.Home.HomeWebService homeWebService)
        {
            try
            {

                // create the Envision Home Entity
                BuilderClasses.EnvisionBuilder builder = new BuilderClasses.EnvisionBuilder(this);
                Home home = builder.GetHome(opportunityId, productId, Config.EnvisionNHTNumber);

                // turn the Envision Home into xml.
                XmlDocument homeDoc = new XmlDocument();
                homeDoc.LoadXml(builder.SerializeToXmlString(home));

                // validate the Envision Home against the schema
                XmlValidation.Home(homeDoc);

                // this is always the lowest inventory level 'Plan'
                //2007-12-17 AB commented out to call custom MI location reference builder
                //lowest level for MI is the elevation
                //XmlNode locationXml = new LocationReferenceBuilder(LocationReferenceType.Plan, planId, releaseId, Config, m_rdaSystem).ToXML();
                XmlNode locationXml = new MI_LocationReferenceBuilder(MI_LocationReferenceType.Elevation, planId, releaseId, Config, m_rdaSystem).ToXML();

                // validate location reference
                XmlValidation.LocationReference(locationXml);

                // execute the web service call
                XmlNode returnXml = homeWebService.UpdateHome(locationXml, homeDoc);

                // validate the return XML against the schema
                XmlValidation.Output(returnXml);

                // create and throw a SoapException if Envision reports a failure
                EnvisionXsdGenerated.Output output = GetOutput(returnXml);
                if (output.Status != OutputStatus.Success) throw CreateSoapException(output);
            }
            catch (SoapException ex)
            {
                //wrap exception with better description
                throw new SoapException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.UpdateEnvisionHome), ex.Code, ex);
            }
            catch (System.Net.WebException ex)
            {
                //wrap exception with better description
                throw new System.Net.WebException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.UpdateEnvisionHome), ex);
            }
            catch (Exception ex)
            {
                // wrap exception with better description
                throw new PivotalApplicationException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.UpdateEnvisionHome), ex);
            }
        }
        
    }

}
