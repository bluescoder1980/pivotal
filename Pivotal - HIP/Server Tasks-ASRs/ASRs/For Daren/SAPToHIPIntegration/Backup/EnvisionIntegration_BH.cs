//
// $Workfile: EnvisionIntegration_BH.cs$
// $Revision: 58$
// $Author: tlyne$
// $Date: Tuesday, August 07, 2007 11:44:42 AM$
//
// Copyright © Pivotal Corporation
//

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
using System.Xml;
using System.Web.Services.Protocols;
using System.Globalization;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// The ASR Class for Envision Integration
    /// </summary>
    public partial class EnvisionIntegration : IRAppScript
    {

        /// <summary>
        /// Method definition for methods that can be used in the ProcessRecordset function.
        /// </summary>
        /// <param name="recordset">The recordset with the cursor at the appropriate record.</param>
        /// <param name="parameters">Dynamic of parameters that can be used in the method</param>
        private delegate void ProcessRecord(Recordset recordset, Dictionary<string, object> parameters);

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

        #region Statics
        /// <summary>
        /// A reusable method for iterating through a recordset
        /// </summary>
        /// <param name="recordset">The recordset to iterate though</param>
        /// <param name="parameters">Any parameters to pass to the underlying method</param>
        /// <param name="processRecord">The underlying method to call for each iteration</param>
        /// <remarks>this method will close the recordset when the iteration is finished</remarks>
        private static void ProcessRecordset(Recordset recordset, Dictionary<string, object> parameters, ProcessRecord processRecord)
        {
            try
            {
                if (recordset.RecordCount > 0)
                {
                    recordset.MoveFirst();
                    while (!recordset.EOF)
                    {
                        // call the delegate
                        processRecord(recordset, parameters);

                        recordset.MoveNext();
                    }
                }
            }
            finally
            {
                recordset.Close(); // cleanup recordset
            }
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
        #endregion



        #region Private Members

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
        /// Sends all Contract (Home and Buyer) additions, changes, and cancellations to Envision
        /// </summary>
        /// <remarks>
        /// Changes to records are identified by using a syncronization table that
        /// tracks the last state of the record when it was syncronized by using the special purpose
        /// Rn_Update field.
        /// - Records are identified as needing to be sent to Envision when they don't have a companion 
        ///   record in the syncronization table
        /// - Records are identified as needing to be re-sent to Envision when there is a companion 
        ///   record in the syncronization table but the record's Rn_Update field does not match the 
        ///   companion syncronization record's Rn_Update_Copy field.
        /// - Records are not deleted in Pivotal, an inactive message is sent to Envision when the Pivotal
        ///   record has been updated with an inactive flag.
        /// </remarks>
        private void SendContractChanges()
        {
            DateTime start = DateTime.Now;  //overall elapse start time

            //web service instances are defined here so they can be cleaned up in the finally
            Envision.DesignCenterManager.Buyer.BuyerWebService buyerWebService = null;
            Envision.DesignCenterManager.Home.HomeWebService homeWebService = null;

            try
            {
                //check that process is not in a transation.  Do to the potentially long processing time transactions should no be used at this level.
                if (System.EnterpriseServices.ContextUtil.IsInTransaction)
                    throw new PivotalApplicationException((string)this.LangDictionary.GetText("ExceptionMethodMustNotHaveTransaction"));

                Log.WriteEvent((string)this.LangDictionary.GetText("LogEventSendingContractChanges"));  //log that processing has started

                // throw exception if Send Contracts is already running.  Protect from synchronous running of Send Contracts
                if (sendContractsIsRunning)
                    throw new PivotalApplicationException((string)this.LangDictionary.GetText("ExceptionContractProcessingOverlap"));  //throw error to log process overlap
                sendContractsIsRunning = true;  // set flag to running state


                // setup buyer web service
                buyerWebService = new Envision.DesignCenterManager.Buyer.BuyerWebService(this);
                buyerWebService.AuthHeaderValue = new Envision.DesignCenterManager.Buyer.AuthHeader();
                buyerWebService.AuthHeaderValue.UserName = this.Config.EnvisionWebServiceUserName;
                buyerWebService.AuthHeaderValue.Password = this.Config.EnvisionWebServicePassword;
                buyerWebService.AuthHeaderValue.NHTBillingNumber = this.Config.EnvisionNHTNumber;
                buyerWebService.Url = this.Config.EnvisionBuyerWebServiceUrl;
                buyerWebService.Timeout = this.Config.EnvisionWebServiceTimeout;

                // setup home web service
                homeWebService = new Envision.DesignCenterManager.Home.HomeWebService(this);
                homeWebService.AuthHeaderValue = new Envision.DesignCenterManager.Home.AuthHeader();
                homeWebService.AuthHeaderValue.UserName = this.Config.EnvisionWebServiceUserName;
                homeWebService.AuthHeaderValue.Password = this.Config.EnvisionWebServicePassword;
                homeWebService.AuthHeaderValue.NHTBillingNumber = this.Config.EnvisionNHTNumber;
                homeWebService.Url = this.Config.EnvisionHomeWebServiceUrl;
                homeWebService.Timeout = this.Config.EnvisionWebServiceTimeout;

                // setup the web services as parameters for the common recordset iteration
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("homeWebService", homeWebService);
                parameters.Add("buyerWebService", buyerWebService);

                // **send updates first**
                // process Contract(Opportunity) updates
                DateTime startElapse = DateTime.Now;
                Log.WriteInformation((string)this.LangDictionary.GetTextSub("LogInfoProcessingUpdates", new string[]{"Contract(Opportunity)"}));
                Recordset opportunityRecords = this.PivotalDataAccess.GetRecordset(OpportunityData.QueryAllOutOfSyncOpportunities, 0, new object[] { OpportunityData.OpportunityIdField, OpportunityData.LotIdField, OpportunityData.StatusField, OpportunityData.RnUpdateField });
                Log.WritePerformance((string)this.LangDictionary.GetTextSub("LogPerformanceQueryString", new string[] { opportunityRecords.RecordCount.ToString(CultureInfo.CurrentCulture), "Opportunity", DateTime.Now.Subtract(startElapse).TotalSeconds.ToString(CultureInfo.CurrentCulture) }));
                ProcessRecordset(opportunityRecords, parameters, new ProcessRecord(SendContractUpdate));
                Log.WritePerformance((string)this.LangDictionary.GetTextSub("LogPerformanceEnvisionContractUpdatesElapse", new string[] { "Contract(Opportunity)", DateTime.Now.Subtract(startElapse).TotalSeconds.ToString(CultureInfo.CurrentCulture) }));

                // process Homesite(Product) updates
                startElapse = DateTime.Now;
                Log.WriteInformation((string)this.LangDictionary.GetTextSub("LogInfoProcessingUpdates", new string[] { "Homesite(Product)" }));
                Recordset productRecords = this.PivotalDataAccess.GetRecordset(ProductData.QueryAllOutOfSyncProducts, 0, new object[] { ProductData.ProductIdField, ProductData.RnUpdateField });
                Log.WritePerformance((string)this.LangDictionary.GetTextSub("LogPerformanceQueryString", new string[] { productRecords.RecordCount.ToString(CultureInfo.CurrentCulture), "Product", DateTime.Now.Subtract(startElapse).TotalSeconds.ToString(CultureInfo.CurrentCulture) }));
                ProcessRecordset(productRecords, parameters, new ProcessRecord(SendHomeUpdate));
                Log.WritePerformance((string)this.LangDictionary.GetTextSub("LogPerformanceEnvisionContractUpdatesElapse", new string[] { "Homesite(Product)", DateTime.Now.Subtract(startElapse).TotalSeconds.ToString(CultureInfo.CurrentCulture) }));

                // process Contact updates
                ReSyncChangedContacts(buyerWebService);

                // **send creations after updates**
                // create Homes first
                startElapse = DateTime.Now;
                Log.WriteInformation((string)this.LangDictionary.GetTextSub("LogInfoProcessingCreations", new string[] { "Contract" }));
                DateTime queryElapse = DateTime.Now;
                opportunityRecords = this.PivotalDataAccess.GetRecordset(OpportunityData.QueryAllApprovedContractsWithOutSync, 0, new object[] { OpportunityData.OpportunityIdField, OpportunityData.LotIdField });
                Log.WritePerformance((string)this.LangDictionary.GetTextSub("LogPerformanceQueryString", new string[] { opportunityRecords.RecordCount.ToString(CultureInfo.CurrentCulture), "Opportunity", DateTime.Now.Subtract(queryElapse).TotalSeconds.ToString(CultureInfo.CurrentCulture) }));
                ProcessRecordset(opportunityRecords, parameters, new ProcessRecord(SendNewHome));

                // create Buyers
                queryElapse = DateTime.Now;
                opportunityRecords = this.PivotalDataAccess.GetRecordset(OpportunityData.QueryAllApprovedContractsWithSyncPending, 0, new object[] { OpportunityData.OpportunityIdField, OpportunityData.ContactIdField });
                Log.WritePerformance((string)this.LangDictionary.GetTextSub("LogPerformanceQueryString", new string[] { opportunityRecords.RecordCount.ToString(CultureInfo.CurrentCulture), "Opportunity", DateTime.Now.Subtract(queryElapse).TotalSeconds.ToString(CultureInfo.CurrentCulture) }));
                ProcessRecordset(opportunityRecords, parameters, new ProcessRecord(SendNewBuyer));
                Log.WritePerformance((string)this.LangDictionary.GetTextSub("LogPerformanceEnvisionContractCreationsElapse", new string[] { "Contract", DateTime.Now.Subtract(startElapse).TotalSeconds.ToString(CultureInfo.CurrentCulture) }));
            }
            finally
            {
                // clean up
                sendContractsIsRunning = false; //set flag to not running state
                if (homeWebService != null) homeWebService.Dispose();
                if (buyerWebService != null) buyerWebService.Dispose();
            }

            // log processing details
            TimeSpan elaps = DateTime.Now.Subtract(start);
            Log.WritePerformance((string)this.LangDictionary.GetTextSub("LogPerformanceTotalContractProcessingElapse", new string[] { elaps.TotalSeconds.ToString(CultureInfo.CurrentCulture) }));
            Log.WriteInformation((string)this.LangDictionary.GetText("LogInfoContractSendCompleted"));
        }


        /// <summary>
        /// This method sends change Contract Contacts to Envision
        /// </summary>
        /// <param name="buyerWebService">An instance to the Buyer web service which to send the buyer changes to.</param>
        /// <remarks>
        /// - An Envision Buyer is made up of multiple records from different tables.  If any data of these records change
        ///   the Buyer needs to be re-sent to Envision in order to keep it up to date.
        /// - Envision Buyer entities can contain data from records that are used in other Envision Buyer entities.  As such 
        ///   the syncronization state of the Pivotal records can only be updated to syncronized at the end of processing when it is known
        ///   that all Buyers that the record is involved with have been sent successfully.  The State Tracker type is used to track
        ///   if a failure has occured and thus the syncronized state will not be set.
        /// - Processing is performed on a per Contact bases.
        /// </remarks>
        private void ReSyncChangedContacts(Envision.DesignCenterManager.Buyer.BuyerWebService buyerWebService)
        {
            DateTime startElapse = DateTime.Now;
            Log.WriteInformation((string)this.LangDictionary.GetTextSub("LogInfoProcessingUpdates", new string[] { "Contact" }));

            // create all contact state trackers - this gets all records that are out of sync so that the sync state can be updated to syncronized
            // if successful.
            Recordset contactRecords = this.PivotalDataAccess.GetRecordset(ContactData.QueryOutOfSyncContacts, 0, ContactData.ContactIdField, ContactData.RnUpdateField);
            Dictionary<string, StateTracker> contactStates = CreateStateTrackers(contactRecords, ContactData.ContactIdField, ContactData.RnUpdateField);

            Recordset loanProfileRecords = this.PivotalDataAccess.GetRecordset(LoanProfileData.QueryOutOfSyncLoanProfiles, 0, LoanProfileData.LoanProfileIdField, LoanProfileData.RnUpdateField);
            Dictionary<string, StateTracker> loanProfileStates = CreateStateTrackers(loanProfileRecords, LoanProfileData.LoanProfileIdField, LoanProfileData.RnUpdateField);

            Recordset loanRecords = this.PivotalDataAccess.GetRecordset(LoanData.QueryOutOfSyncLoans, 0, LoanData.LoanIdField, LoanData.RnUpdateField);
            Dictionary<string, StateTracker> loanStates = CreateStateTrackers(loanRecords, LoanData.LoanIdField, LoanData.RnUpdateField);

            // creates the parameters for the common method of recordset processing.
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("contactStates", contactStates);
            parameters.Add("loanProfileStates", loanProfileStates);
            parameters.Add("loanStates", loanStates);
            parameters.Add("buyerWebService", buyerWebService);


            // get all root contacts where the contact or a child record(s) has been changed and send
            // the subsequent Envision Buyer to Envision.
            DateTime queryElapse = DateTime.Now;
            contactRecords = this.PivotalDataAccess.GetRecordset(ContactData.QueryOutOfSyncRootContacts, 0, new object[] { ContactData.ContactIdField });
            Log.WritePerformance((string)this.LangDictionary.GetTextSub("LogPerformanceQueryString", new string[] { contactRecords.RecordCount.ToString(CultureInfo.CurrentCulture), "Contact", DateTime.Now.Subtract(queryElapse).TotalSeconds.ToString(CultureInfo.CurrentCulture) }));
            ProcessRecordset(contactRecords, parameters, new ProcessRecord(UpdateContact));

            //update the state records to syncronized only if successful
            foreach(StateTracker contactState in contactStates.Values)
            {
                //only update the sync state if webservice call(s) have been successful
                if (!contactState.syncFailure)
                    Sync.SetContactState(contactState.Id, contactState.updateId);
            }
            foreach (StateTracker loanProfileState in loanProfileStates.Values)
            {
                if (!loanProfileState.syncFailure)
                    Sync.SetLoanProfileState(loanProfileState.Id, loanProfileState.updateId);
            }

            foreach(StateTracker loanState in loanStates.Values)
            {
                if (!loanState.syncFailure)
                    Sync.SetLoanState(loanState.Id, loanState.updateId);
            }

            //log the time it takes to update the Envision Buyers
            Log.WritePerformance((string)this.LangDictionary.GetTextSub("LogPerformanceEnvisionContractUpdatesElapse", new string[] { "Contact", DateTime.Now.Subtract(startElapse).TotalSeconds.ToString(CultureInfo.CurrentCulture) }));
        }


        /// <summary>
        /// Sends all Envision Buyer entities that the Contact is involved with
        /// </summary>
        /// <param name="contactRecords">A contact recordset with the cursor pointing to the current record needing to be processed</param>
        /// <param name="parameters">
        /// Dynamic list of parameters needed in the method:
        /// contactId - The contact record's unique key
        /// contactStates - A list of state entities that represent Contact record state
        /// loanState - A list of state entities that represent Loan record state
        /// loanProfileStates - A list of state entities that represent Loan Profile record state
        /// buyerWebService - The web service entity to which to send the Buyer Update
        /// </param>
        private void UpdateContact(Recordset contactRecords, Dictionary<string, object> parameters)
        {
            byte[] contactId = (byte[])contactRecords.Fields[ContactData.ContactIdField].Value;

            //creates a new parameter list for processing each Envision Buyer using the common recordset processing method.
            Dictionary<string, object> newParams = new Dictionary<string, object>();
            newParams.Add("contactStates", (Dictionary<string, StateTracker>)parameters["contactStates"]);
            newParams.Add("loanProfileStates", (Dictionary<string, StateTracker>)parameters["loanProfileStates"]);
            newParams.Add("loanStates", (Dictionary<string, StateTracker>)parameters["loanStates"]);
            newParams.Add("contactId", contactId);
            newParams.Add("buyerWebService", (Envision.DesignCenterManager.Buyer.BuyerWebService)parameters["buyerWebService"]);

            //Get all the Contracts the Contact is involved with and process each one.
            Recordset opportunityRecords = this.PivotalDataAccess.GetRecordset(OpportunityData.QueryAllApprovedContractsWithContact, 1, contactId, OpportunityData.OpportunityIdField);
            ProcessRecordset(opportunityRecords, newParams, new ProcessRecord(UpdateEnvisionBuyer));
        }


        /// <summary>
        /// Sends an Envision Buyer update to Envision.
        /// </summary>
        /// <param name="opportunityRecords">An Opportunity recordset with the cursor at the appropriate record</param>
        /// <param name="parameters">
        /// Dynamic list of parameters needed in the method:
        /// contactId - The contact record's unique key
        /// contactStates - A list of state entities that represent Contact record state
        /// loanState - A list of state entities that represent Loan record state
        /// loanProfileStates - A list of state entities that represent Loan Profile record state
        /// buyerWebService - The web service entity to which to send the Buyer Update
        /// </param>
        /// <remarks>
        /// - Provides the standard interface for the common recorset processing reutine.
        /// - SoapExceptions are trapped here and do not bubble so as to not impact furthur processing
        /// </remarks>
        private void UpdateEnvisionBuyer(Recordset opportunityRecords, Dictionary<string, object> parameters)
        {
            // get and type the parameters frm the parameter list
            byte[] opportunityId = (byte[])opportunityRecords.Fields[OpportunityData.OpportunityIdField].Value;
            byte[] contactId = (byte[])parameters["contactId"];
            Dictionary<string, StateTracker> contactStates = (Dictionary<string, StateTracker>)parameters["contactStates"];
            Dictionary<string, StateTracker> loanStates = (Dictionary<string, StateTracker>)parameters["loanStates"];
            Dictionary<string, StateTracker> loanProfileStates = (Dictionary<string, StateTracker>)parameters["loanProfileStates"];

            Envision.DesignCenterManager.Buyer.BuyerWebService buyerWebService = (Envision.DesignCenterManager.Buyer.BuyerWebService)parameters["buyerWebService"];

            try
            {
                // call the specific UpdateEnvisionBuyer method
                UpdateEnvisionBuyer(opportunityId, contactId, contactStates, loanProfileStates, loanStates, buyerWebService);
            }
            catch(SoapException ex)
            {
                //no bubble on Envision Soap Exceptions, continue processing next record.
                Log.WriteException(ex);
            }
            catch (System.Net.WebException ex)
            {
                Log.WriteException(ex);
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
                XmlNode locationXml = new LocationReferenceBuilder(LocationReferenceType.Division, divisionId, this.Config, this.m_rdaSystem).ToXML();

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
        /// Send a Contract update, including both the Envision Home and Envision Buyer, to Envision
        /// </summary>
        /// <param name="opportunityRecords">Contract recordset with the cursor set to the current record in need of processing</param>
        /// <param name="parameters">
        /// Dynamic parameter list for the method:
        /// buyerWebService - The web service to send the Envision Buyer
        /// homeWebService - The web service to send the Envision Home
        /// </param>
        private void SendContractUpdate(Recordset opportunityRecords, Dictionary<string, object> parameters)
        {
            // get and type the web service parameters
            Envision.DesignCenterManager.Buyer.BuyerWebService buyerWebService = (Envision.DesignCenterManager.Buyer.BuyerWebService)parameters["buyerWebService"];
            Envision.DesignCenterManager.Home.HomeWebService homeWebService = (Envision.DesignCenterManager.Home.HomeWebService)parameters["homeWebService"];

            // get the required field values
            byte[] opportunityId = (byte[])opportunityRecords.Fields[OpportunityData.OpportunityIdField].Value;
            byte[] opportunityUpdateId = (byte[])opportunityRecords.Fields[OpportunityData.RnUpdateField].Value;
            byte[] productId = (byte[])opportunityRecords.Fields[OpportunityData.LotIdField].Value;
            string status = (string)opportunityRecords.Fields[OpportunityData.StatusField].Value;

            // call the method
            UpdateEnvisionHomeAndBuyer(status == "Cancelled", opportunityId, opportunityUpdateId, productId, homeWebService, buyerWebService);
        }

        /// <summary>
        /// On a contract change, sends the Home and Buyer updates to Envision.
        /// </summary>
        /// <param name="deactivate">Deactivates the Home and Buyer in Envision</param>
        /// <param name="opportunityId">Opportunity record Id that represents the Contract</param>
        /// <param name="opportunityUpdateId">Rn_Updat id of the Opportunity record.</param>
        /// <param name="productId">Product record Id that represend the Contract's Homesite</param>
        /// <param name="homeWebService">An initialized instance of the home web service</param>
        /// <param name="buyerWebService">An initialized instance of the buyer web service</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        private void UpdateEnvisionHomeAndBuyer(bool deactivate, object opportunityId, byte[] opportunityUpdateId, object productId, Envision.DesignCenterManager.Home.HomeWebService homeWebService, Envision.DesignCenterManager.Buyer.BuyerWebService buyerWebService)
        {
            try
            {
                // update Both the Envision Home and the Envision Buyer
                byte[] contactId = (byte[])this.m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.ContactIdField].Index(opportunityId);
                UpdateEnvisionHome(opportunityId, productId, homeWebService);

                // as this update is only for changes to the Contract, we don't need to track sub-record state
                // so all the state dictionaries are set to empty.
                UpdateEnvisionBuyer(opportunityId, contactId, new Dictionary<string, StateTracker>(), new Dictionary<string, StateTracker>(), new Dictionary<string, StateTracker>(), buyerWebService);



                // if deactivate, send the Home and Buyer status to inactive
                if (deactivate)
                {
                    // incase a deactivated record is updated, don't resend the deactivation to Envision
                    Recordset syncRecords = this.PivotalDataAccess.GetRecordset(EnvSyncData.SyncForContractQuery, 1, new object[] { opportunityId, EnvSyncData.EnvSyncIdField, EnvSyncData.OpportunityInactiveField });
                    if (syncRecords.RecordCount != 1)
                        throw new PivotalApplicationException((string)this.LangDictionary.GetText("ExceptionUnexpectedNofRecords"));
                    syncRecords.MoveFirst();
                    bool contractInactive = (syncRecords.Fields[EnvSyncData.OpportunityInactiveField].Value == DBNull.Value)
                        ? false : (bool)syncRecords.Fields[EnvSyncData.OpportunityInactiveField].Value;
                    syncRecords.Close();
                    if (!contractInactive)
                    {
                        Log.WriteInformation(string.Format(CultureInfo.CurrentCulture, "Deactivating contract {0}", this.m_rdaSystem.IdToString(opportunityId)));

                        //get the Envision entity ids
                        BuilderClasses.EnvisionBuilder builder = new BuilderClasses.EnvisionBuilder(this);
                        string homeNumber = builder.GenerateHomeNumber(opportunityId, productId);
                        string buyerNumber = builder.GenerateBuyerNumber(opportunityId, contactId);


                        try
                        {
                            // *deactivate Envision Buyer first*
                            byte[] neighborhoodId = (byte[])this.m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.NeighborhoodIdField].Index(opportunityId);
                            byte[] divisionId = (byte[])this.m_rdaSystem.Tables[NeighborhoodData.TableName].Fields[NeighborhoodData.DivisionIdField].Index(neighborhoodId);
                            XmlNode buyerLocationXml = new LocationReferenceBuilder(LocationReferenceType.Division, divisionId, this.Config, this.m_rdaSystem).ToXML();

                            // validate location reference
                            XmlValidation.LocationReference(buyerLocationXml);

                            // call web service
                            XmlNode buyerOutputXml = buyerWebService.UpdateBuyerStatus(buyerLocationXml, buyerNumber, EnvisionRecordStatus.Inactive.ToString());
                            EnvisionXsdGenerated.Output buyerOutput = GetOutput(buyerOutputXml);
                            // throw SoapException if Envision returns an error.
                            if (buyerOutput.Status != OutputStatus.Success) throw CreateSoapException(buyerOutput);
                        }
                        catch (SoapException ex)
                        {
                            //wrap exception with better description
                            throw new SoapException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.DeactivateEnvisionBuyer), ex.Code, ex);
                        }
                        catch (System.Net.WebException ex)
                        {
                            //wrap exception with better description
                            throw new System.Net.WebException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.UpdateEnvisionHome), ex);
                        }
                        catch (Exception ex)
                        {
                            //wrap exception with better description
                            throw new PivotalApplicationException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.DeactivateEnvisionBuyer), ex);
                        }


                        try
                        {
                            // *deactivate Envision Home second*
                            byte[] planId = (byte[])this.m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.PlanNameIdField].Index(opportunityId);
                            XmlNode homeLocationXml = new LocationReferenceBuilder(LocationReferenceType.Plan, planId, this.Config, this.m_rdaSystem).ToXML();

                            // validate location reference
                            XmlValidation.LocationReference(homeLocationXml);

                            // call web service
                            XmlNode homeOutputXml = homeWebService.UpdateHomeStatus(homeLocationXml, homeNumber, EnvisionRecordStatus.Inactive.ToString(), true, true);
                            EnvisionXsdGenerated.Output homeOutput = GetOutput(homeOutputXml);
                            // throw SoapException if Envision returns an error.
                            if (homeOutput.Status != OutputStatus.Success) throw CreateSoapException(homeOutput);
                        }
                        catch (SoapException ex)
                        {
                            //wrap exception with better description
                            throw new SoapException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.DeactivateEnvisionHome), ex.Code, ex);
                        }
                        catch (System.Net.WebException ex)
                        {
                            //wrap exception with better description
                            throw new System.Net.WebException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.UpdateEnvisionHome), ex);
                        }
                        catch (Exception ex)
                        {
                            //wrap exception with better description
                            throw new ApplicationException(ContractExceptionMessage.GetContractSendExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, (byte[])opportunityId, ContractExceptionMessage.ContractSendProcessing.DeactivateEnvisionHome), ex);
                        }
                    }
                }

                // if successful update the Contract sync state to syncronized
                Sync.SetContractState(opportunityId, deactivate, opportunityUpdateId, CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.Sync.SyncState.Success);
            }
            catch (SoapException ex) //only catch SoapExceptions.  All others should stop all processing.
            {
                //no bubble on SoapExceptions, log and continue processing next record.
                Log.WriteException(ex);
            }
            catch (System.Net.WebException ex)
            {
                Log.WriteException(ex);
            }
        }



        /// <summary>
        /// Pushes any changes to a Homesite out to Envision
        /// </summary>
        /// <param name="productRecordset">The current product specifying the Homesite that has changed</param>
        /// <param name="parameters">
        /// The set of parameters needed to push the Homesite to Envision:
        /// homeWebService - The web service to send the Envision Home to.
        /// </param>
        /// <remarks>This method catches SoapExceptions so that further processing is not impacted</remarks>
        private void SendHomeUpdate(Recordset productRecordset, Dictionary<string, object> parameters)
        {
            try
            {
                byte[] productId = (byte[])productRecordset.Fields[ProductData.ProductIdField].Value;
                byte[] recordUpdateId = (byte[])productRecordset.Fields[ProductData.RnUpdateField].Value;

                // create a new parameter list for the Opportunity(Contract) iteration.
                Dictionary<string, object> newParameters = new Dictionary<string, object>();
                newParameters.Add("Product_Id", productId);
                newParameters.Add("homeWebService", (Envision.DesignCenterManager.Home.HomeWebService)parameters["homeWebService"]);

                // Send a Envision Home for every Contract with this Homesite
                Recordset opportunityRecords = this.PivotalDataAccess.GetRecordset(OpportunityData.QueryAllApprovedContractsWithHomesite, 1, new object[] { productId, OpportunityData.OpportunityIdField });
                ProcessRecordset(opportunityRecords, newParameters, new ProcessRecord(SendHomeUpdatePerContract));

                // update the sync state to syncronized of successful
                Sync.SetHomeState(productId, recordUpdateId);
            }
            catch (SoapException ex)
            {
                //no bubble on Soap Exceptions, continue processing next record.
                Log.WriteException(ex);
            }
            catch (System.Net.WebException ex)
            {
                Log.WriteException(ex);
            }
        }



        /// <summary>
        /// Sends an Envision Home entity to Envision for each Contract that has the Homesite
        /// </summary>
        /// <param name="opportunityRecords">The current Contract record to process</param>
        /// <param name="parameters">
        /// The set of parameters needed to push the Homesite to Envision:
        /// Product_Id - Id of the homesite.
        /// homeWebService - The web service to send the Envision Home to
        /// </param>
        /// <remarks>This method has a generic interface so that it can be used in a ProcessRecord delegate</remarks>
        private void SendHomeUpdatePerContract(Recordset opportunityRecords, Dictionary<string, object> parameters)
        {
            // get the parameters out of the dynamic parameter list.
            byte[] productId = (byte[])parameters["Product_Id"];
            Envision.DesignCenterManager.Home.HomeWebService homeWebService = (Envision.DesignCenterManager.Home.HomeWebService)parameters["homeWebService"];
    
            byte[] opportunityId = (byte[])opportunityRecords.Fields[OpportunityData.OpportunityIdField].Value;

            // calls the update method
            UpdateEnvisionHome(opportunityId, productId, homeWebService);
        }




        /// <summary>
        /// Sends an Envision Home update to Envision
        /// </summary>
        /// <param name="opportunityId">The Opportunity(Contract) id</param>
        /// <param name="productId">Product record Id from which to gather the Homesite information</param>
        /// <param name="homeWebService">Initialized instance of the Envision Home web serivce</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void UpdateEnvisionHome(object opportunityId, object productId, Envision.DesignCenterManager.Home.HomeWebService homeWebService)
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

                // genrate the location xml
                byte[] planId = (byte[])this.m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.PlanNameIdField].Index(opportunityId);
                object releaseId = m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.NBHDPhaseIdField].Index(opportunityId);
                //XmlNode locationXml = new LocationReferenceBuilder(LocationReferenceType.Plan, planId, this.Config, this.m_rdaSystem).ToXML();
                XmlNode locationXml = new LocationReferenceBuilder(LocationReferenceType.Plan, planId, releaseId, Config, m_rdaSystem).ToXML();

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



        /// <summary>
        /// Pulls the needed parameters out of the parameter argument and calls "CreateEnvisionHomesite" with them.
        /// </summary>
        /// <param name="contractRecords">The current contract reccord.</param>
        /// <param name="parameters">
        /// Parameters that are needed to create and Envision Home:
        /// homeWebService - The web service to send the New Envision Home to.
        /// </param>
        /// <remarks>This method catches SoapExceptions in order not to impact furthur processing.  All other
        /// Exceptions cause process hault.</remarks>
        private void SendNewHome(Recordset contractRecords, Dictionary<string, object> parameters)
        {

            Envision.DesignCenterManager.Home.HomeWebService homeWebService = (Envision.DesignCenterManager.Home.HomeWebService)parameters["homeWebService"];
            byte[] opportunityId = (byte[])contractRecords.Fields[OpportunityData.OpportunityIdField].Value;
            byte[] productId = (byte[])contractRecords.Fields[OpportunityData.LotIdField].Value;

            try
            {
                CreateEnvisionHome(opportunityId, productId, homeWebService);
            }
            catch (SoapException ex)
            {
                //no bubble on SoapExceptions, continue processing next record.
                Log.WriteException(ex);
            }
            catch (System.Net.WebException ex)
            {
                Log.WriteException(ex);
            }
        }

        /// <summary>
        /// Pulls the needed parameters out of the parameter argument and calls "CreateEnvisionBuyer" with them.
        /// </summary>
        /// <param name="contractRecords">The current contract reccord.</param>
        /// <param name="parameters">Parameters that are needed to create and Envision Buyer</param>
        /// <remarks>This method catches SoapExceptions in order not to impact furthur processing.  All other
        /// Exceptions cause process hault.</remarks>
        private void SendNewBuyer(Recordset contractRecords, Dictionary<string, object> parameters)
        {

            Envision.DesignCenterManager.Buyer.BuyerWebService buyerWebService = (Envision.DesignCenterManager.Buyer.BuyerWebService)parameters["buyerWebService"];

            byte[] contractId = (byte[])contractRecords.Fields[OpportunityData.OpportunityIdField].Value;
            byte[] contactId = (byte[])contractRecords.Fields[OpportunityData.ContactIdField].Value;

            try
            {
                CreateEnvisionBuyer(contractId, contactId, buyerWebService);
            }
            catch (SoapException ex)
            {
                //no bubble on Envision Exceptions, continue processing next record.
                Log.WriteException(ex);
            }
            catch (System.Net.WebException ex)
            {
                Log.WriteException(ex);
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
                //XmlNode locationXml = new LocationReferenceBuilder(LocationReferenceType.Plan, planId, this.Config, this.m_rdaSystem).ToXML();
                XmlNode locationXml = new LocationReferenceBuilder(LocationReferenceType.Plan, planId, releaseId, Config, m_rdaSystem).ToXML();

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
                XmlNode buyerLocationXml = new LocationReferenceBuilder(LocationReferenceType.Division, divisionId, this.Config, this.m_rdaSystem).ToXML();

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



        #endregion
    }
}
