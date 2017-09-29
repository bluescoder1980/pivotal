//
// $Workfile: EnvisionIntegration_BS.cs$
// $Revision: 2$
// $Author: tlyne$
// $Date: Wednesday, December 19, 2007 11:12:40 AM$
//
// Copyright © Pivotal Corporation
//

//AM2010.08.26 - Modified Envision code and logic to work with The Irvine Company
//process for integration option selections from Chateau Design Center

using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated.BuyerSelections;
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using System.Web.Services.Protocols;


namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// Option Selection Source
    /// </summary>
    public enum OptionSelectionSource
    {
        /// <summary>
        /// Pivotal is the source of option seletion
        /// </summary>
        Pivotal = 0,

        /// <summary>
        /// Envision is the source of option selection
        /// </summary>
        Envision = 1
    }


    /// <summary>
    /// Option Selection Source
    /// </summary>
    public enum OpportunityType
    {
        /// <summary>
        /// Pivotal is the source of option seletion
        /// </summary>
        InventoryQuote,

        /// <summary>
        /// Envision is the source of option selection
        /// </summary>
        Contract
    }
    /// <summary>
    /// The ASR Class for Envision Integration
    /// </summary>
    public partial class EnvisionIntegration : IRAppScript
    {

        #region Class-Level variables

        //AM2010.08.26 - Class level list to track new Post Sale Quotes and
        //Post Build Quotes to "Accept" once batch processing is complete
        List<object> pstIdList = new List<object>();
        
        #endregion

        /// <summary>
        /// Save the buyer selections to the database for later processing.
        /// </summary>
        /// <param name="xml">Xml containing the new/modified/deleted buyer selections for the system</param>
        /// <returns>Failure or success message</returns>
        /// <remarks>This method is intended to run within a MTS transaction</remarks>
        public virtual string SaveBuyerSelections(string xml)
        {
            byte[] contractId = new byte[0];

            //default response to failure
            EnvisionXsdGenerated.Output output = new EnvisionXsdGenerated.Output();
            output.Status = EnvisionXsdGenerated.OutputStatus.Failure;

            try
            {
                // as this method updates the database it must be in a transcation
                if (!System.EnterpriseServices.ContextUtil.IsInTransaction)
                    throw new PivotalApplicationException((string)this.LangDictionary.GetText("ExceptionMethodRequiresTransaction"));

                //log incomming xml
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                Log.WriteXml((string)this.LangDictionary.GetText("ReceivedBuyerSelections"), doc);
                XmlValidation.BuyerSelections(doc);

                // create builder instance from xml
                XmlSerializer ser = new XmlSerializer(typeof(Builder));

                // deserialize the xml into an object hierarchy
                Builder builder = null;
                using (StringReader reader = new StringReader(xml))
                    builder = (Builder)ser.Deserialize(reader);

                // get the contract id and transaction list
                OrganizationType corporation = builder.Organization;
                //2007-12-18 regions are not supported by MI in Envision. Elevation was added prior to home record.
                //OrganizationType region = (OrganizationType)corporation.Item;
                //OrganizationType division = (OrganizationType)region.Item;
                OrganizationType division = (OrganizationType)corporation.Item;
                InventoryType community = (InventoryType)division.Item;
                InventoryType release = (InventoryType)community.Item;
                InventoryType plan = (InventoryType)release.Item;
                //2007-12-18 AB added new elevation level
                InventoryType elevation = (InventoryType)plan.Item;
                InventoryTypeHome home = (InventoryTypeHome)elevation.Item;
                TransactionType[] transactions = (TransactionType[])home.Transaction;
                //2008-04-13 Home number format has changed. Opportuinity ID is second after "-"
                //2008-06-03 AB Format changed again to no include the job number
                //contractId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(home.HomeNumber.Split(':')[0]));
                //contractId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(home.HomeNumber.Split('-')[1].Trim()));
                contractId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(home.HomeNumber.Trim()));

                
                for (int i = 0; i < transactions.Length; i++)
                {
                    // get transaction id and date&time
                    TransactionType transaction = transactions[i];
                    string transactionId = transaction.TransactionID.ToString();
                    DateTime transactionDatetime = transaction.DateCreated;

                    // open recordset
                    Recordset buyerSelectionRecordset = this.PivotalDataAccess.GetRecordset(EnvBuyerSelectionsData.BuyerSelectionForContractAndTransactionQuery, 2
                        , contractId, transactionId
                        , EnvBuyerSelectionsData.EnvBuyerSelectionsIdField
                        , EnvBuyerSelectionsData.OpportunityidField
                        , EnvBuyerSelectionsData.TansactionIdField
                        , EnvBuyerSelectionsData.TransactionDatetimeField
                        , EnvBuyerSelectionsData.XMLAttachmentField
                        , EnvBuyerSelectionsData.StatusField
                        , EnvBuyerSelectionsData.ProcessFailureReasonField);

                    // add new record if does not exists, update if does.
                    if (buyerSelectionRecordset.RecordCount == 0)
                        buyerSelectionRecordset.AddNew(Type.Missing, Type.Missing);
                    else
                        buyerSelectionRecordset.MoveFirst();

                    // populate the simple fields
                    buyerSelectionRecordset.Fields[EnvBuyerSelectionsData.OpportunityidField].Value = contractId;
                    buyerSelectionRecordset.Fields[EnvBuyerSelectionsData.TansactionIdField].Value = transactionId;
                    buyerSelectionRecordset.Fields[EnvBuyerSelectionsData.TransactionDatetimeField].Value = transactionDatetime;
                    buyerSelectionRecordset.Fields[EnvBuyerSelectionsData.StatusField].Value = EnvBuyerSelectionsData.QueuedConst;
                    buyerSelectionRecordset.Fields[EnvBuyerSelectionsData.ProcessFailureReasonField].Value = DBNull.Value;

                    if (xml != null)
                    {
                        // populates the XML Attachment field
                        UTF8Encoding encoding = new UTF8Encoding();
                        //byte[] dataXML = encoding.GetBytes(xml.OuterXml);
                        byte[] dataXML = encoding.GetBytes(xml);
                        IRField5 field = (IRField5)m_rdaSystem.Tables[EnvBuyerSelectionsData.TableName].Fields[EnvBuyerSelectionsData.XMLAttachmentField];
                        IRBLOBHelper objBlobHelper = field.GetBLOBHelper(buyerSelectionRecordset);
                        objBlobHelper.SetBLOBData("file.xml", dataXML, DateTime.Today, DateTime.Now);
                    }

                    // saves the changes
                    this.PivotalDataAccess.SaveRecordset(EnvBuyerSelectionsData.TableName, buyerSelectionRecordset);
                    Log.WriteEvent(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("BuyerSelectionsSaved", new string[] { transactionId })));
                }

                // set success status if there were no problems.
                System.EnterpriseServices.ContextUtil.SetComplete();
                output.Status = EnvisionXsdGenerated.OutputStatus.Success;
            }
            catch (Exception ex)
            {
                // roll back the transaction on failure
                System.EnterpriseServices.ContextUtil.SetAbort();
                Log.WriteException(CreateBuyerSelectionContractProcessingException(contractId, ex));
            }

            // create return xml
            XmlSerializer outputSerializer = new XmlSerializer(output.GetType());
            StringWriter outputStringWriter = new StringWriter(CultureInfo.CurrentCulture);
            outputSerializer.Serialize(outputStringWriter, output);
            outputStringWriter.Close();
            string returnXml = outputStringWriter.ToString();

            // log return xml
            XmlDocument returnDoc = new XmlDocument();
            returnDoc.LoadXml(returnXml);
            Log.WriteXml((string)this.LangDictionary.GetText("SendResponseOfReceivingBS"), returnDoc);

            return returnXml;
        }



        /// <summary>
        /// Main method that apply the buyer selections
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>Process Failure Reason
        ///         empty "": process buyer selection xml successfully
        /// </returns>
        /// <remarks>This method is intended to run within a MTS transaction</remarks>
        public virtual string ApplyBuyerSelections(string xml)
        {

            try
            {
                // performance counter for elaps time
                DateTime procStart = DateTime.Now;
                Log.WriteEvent((string)this.LangDictionary.GetText("GetBSFromQueue"));

                //log incomming xml
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                Log.WriteXml((string)this.LangDictionary.GetText("ProcessingBS"), doc);
                XmlValidation.BuyerSelections(doc);

                // create builder instance from xml
                XmlSerializer ser = new XmlSerializer(typeof(Builder));

                Builder builder;
                string transactionIdList;
                using (StringReader reader = new StringReader(xml))
                {
                    builder = (Builder)ser.Deserialize(reader);

                    // process the buyer selections
                    transactionIdList = ProcessBuyerSelections(builder);
                }


                //process buyer selection ends.
                Log.WriteEvent(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("ProcessingBSEnds", new string[] { transactionIdList })));

                // log elaps performance
                TimeSpan elaps = DateTime.Now.Subtract(procStart);
                Log.WritePerformance(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("UpdateBSTimeUsed", new string[] { transactionIdList, elaps.TotalSeconds.ToString() })));

                return "";
            }
            catch (PivotalApplicationException ex)
            {
                // roll back the transaction on failure
                System.EnterpriseServices.ContextUtil.SetAbort();
                //the inner exception has more detailed info.
                Log.WriteException(ex.InnerException);
                return ex.InnerException.Message;
            }

            catch (Exception ex)
            {
                Log.WriteException(ex);
                // roll back the transaction on failure
                System.EnterpriseServices.ContextUtil.SetAbort();
                return ex.Message;
            }
        }

        /// <summary>
        /// Read buyer selections 
        /// </summary>
        /// <returns></returns>
        /// <remarks>This method is intended to run within a MTS transaction</remarks>
        public virtual void ClearPendingBuyerSelectionsFromQueue()
        {           
            //AM2010.08.26 - This will delete all "Failed" messages in the queue for clean up purposes.
            this.PivotalDataAccess.DeleteRecordset(TICEnvisionConstants.TICIntOptionSelectionsTable.Queries.OPTION_SELECTIONS_PENDING,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TIC_INT_OPTION_SELECTIONS_ID);
            System.EnterpriseServices.ContextUtil.SetComplete();
            
        }


        /// <summary>
        /// Process all Buyer Selection Xml saved to the database 
        /// </summary>
        /// <remarks>This method is intended to run within a MTS transaction</remarks>
        public virtual void ProcessBuyerSelectionsQueue()
        {

            Log.WriteEvent("Processing Buyer Selection Queue");

            // this method must be in a transaction as it updates the database 
            if (!System.EnterpriseServices.ContextUtil.IsInTransaction)
                throw new PivotalApplicationException((string)this.LangDictionary.GetText("ExceptionMethodRequiresTransaction"));


            // get all xml records
            Recordset buyerSelectionsRecordset = this.PivotalDataAccess.GetRecordset(EnvBuyerSelectionsData.MIAllBuyerSelectionXMLQuery, 0
                , EnvBuyerSelectionsData.EnvBuyerSelectionsIdField
                , EnvBuyerSelectionsData.XMLAttachmentField
                , EnvBuyerSelectionsData.TransactionDatetimeField
                , EnvBuyerSelectionsData.ProcessFailureReasonField
                , EnvBuyerSelectionsData.StatusField
                );

            if (buyerSelectionsRecordset.RecordCount > 0)
            {
                buyerSelectionsRecordset.Sort = EnvBuyerSelectionsData.TransactionDatetimeField;
                buyerSelectionsRecordset.MoveFirst();
                while (!buyerSelectionsRecordset.EOF)
                {
                    string buyerSelectionsXml = System.Text.UTF8Encoding.UTF8.GetString((byte[])buyerSelectionsRecordset.Fields[EnvBuyerSelectionsData.XMLAttachmentField].Value);
                    string buyerSelectionsProcessResult = ApplyBuyerSelections(buyerSelectionsXml);
                    //the DBNull.Value does not work here, have to use null instead.
                    if (buyerSelectionsProcessResult == string.Empty)
                    {
                        buyerSelectionsRecordset.Fields[EnvBuyerSelectionsData.StatusField].Value = EnvBuyerSelectionsData.SuccessConst;
                    }
                    else
                    {
                        buyerSelectionsRecordset.Fields[EnvBuyerSelectionsData.ProcessFailureReasonField].Value = buyerSelectionsProcessResult;
                        buyerSelectionsRecordset.Fields[EnvBuyerSelectionsData.StatusField].Value = EnvBuyerSelectionsData.FailureConst;
                    }
                    buyerSelectionsRecordset.MoveNext();
                }
                this.PivotalDataAccess.SaveRecordset(EnvBuyerSelectionsData.TableName, buyerSelectionsRecordset);

                this.PivotalDataAccess.DeleteRecordset(EnvBuyerSelectionsData.SuccessBuyerSelectionsQuery,
                    EnvBuyerSelectionsData.EnvBuyerSelectionsIdField);
            }
        }



        /// <summary>
        /// Process Buyer Selections transaction
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>transactionID list. Actually for now only one transaction in one Buyer Selections XML. </returns>
        protected virtual string ProcessBuyerSelections(Builder builder)
        {
            byte[] contractId = null;
            string transactionList = "";

            try
            {
                OrganizationType corporation = builder.Organization;
                //2007-12-18 regions are not supported by MI in Envision. Elevation was added prior to home record.
                //OrganizationType region = (OrganizationType)corporation.Item;
                //OrganizationType division = (OrganizationType)region.Item;
                OrganizationType division = (OrganizationType)corporation.Item;
                InventoryType community = (InventoryType)division.Item;
                InventoryType release = (InventoryType)community.Item;
                InventoryType plan = (InventoryType)release.Item;
                //2007-12-18 AB added new elevation level
                InventoryType elevation = (InventoryType)plan.Item;
                InventoryTypeHome home = (InventoryTypeHome)elevation.Item;

                //2007-12-18 AB added to support utility functions need for custom MI coding
                TIC_Envision_Utility util = new TIC_Envision_Utility();

                TransactionType[] transactions = (TransactionType[])home.Transaction;
                //2007-12-30 AB This check is not needed because no home numbers will have a ":" in it. A simple check has been added to always pass to minimize code changes.
                //if (home.HomeNumber.IndexOf(":") != -1)
                if (0==0)
                {
                    //contractId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(home.HomeNumber.Split(':')[0]));
                    //String strContractId = home.HomeNumber.Split(new Char[] { '-' }, 2)[1];
                    String strContractId = home.HomeNumber;
                    strContractId = strContractId.Trim();
                    bool pivContract = true;

                    try
                    {
                        contractId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(strContractId));
                        //2007-12-30 AB. Get the actual contract ID. If this is not a contract created from an IQ then the ID from Envision
                        //is the actual ID. If not we need to find the actual contract ID
                        contractId = (byte[])util.GetInboundContract(contractId, m_rdaSystem);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteInformation("A non-Pivotal contract with contract ID " + strContractId + " has been removed.");
                        pivContract = false;
                    }

                    
                        object postSaleQuoteId = new byte[0];

                        if (pivContract)
                        {
                            GetPostSaleQuote(contractId, out postSaleQuoteId, OpportunityType.Contract);
                        }
                        // process add, edit or delete
                        for (int i = 0; i < transactions.Length; i++)
                        {
                            TransactionType transaction = transactions[i];
                            byte[] divisionProductId;
                            string productName;  //this is used for custom option search criteria
                            string productNumber;  //this is used for custom option search criteria

                            transactionList += transaction.TransactionID.ToString() + ",";
                            if (pivContract)
                            {
                                if (transaction.Option.OptionType == SelectedOptionTypeOptionType.Custom) //custom option
                                {
                                    divisionProductId = null;
                                    productName = transaction.Option.OptionName;
                                    productNumber = transaction.Option.OptionNumber;
                                }
                                else
                                {
                                    //divisionProductId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(transaction.Option.OptionNumber));
                                    //Need to find the division product Id
                                    string divisionProductNumber = transaction.Option.OptionNumber;
                                    string divisionNumber = division.LocationNumber;

                                    divisionProductId = (byte[])util.GetDivisionOption(divisionNumber, divisionProductNumber, m_rdaSystem);
                                    productName = null;
                                    productNumber = null;
                                }

                                string validateBuyerSelectionReturn;
                                object nbhdp_ProductId;
                                string optionAvailableTo;
                                validateBuyerSelectionReturn = ValidateBuyerSelection(transaction, postSaleQuoteId, divisionProductId, out nbhdp_ProductId, out optionAvailableTo);
                                if (validateBuyerSelectionReturn.Length == 0)
                                {
                                    switch (transaction.TransactionType1)
                                    {
                                        case TransactionTypeTransactionType.Add:
                                        case TransactionTypeTransactionType.Edit:
                                            AddEditOptionToPostSaleQuote(postSaleQuoteId, divisionProductId, productName, productNumber, nbhdp_ProductId, optionAvailableTo, transaction, false);
                                            break;
                                        case TransactionTypeTransactionType.Delete:
                                            DeleteOptionOnPostSaleQuote(postSaleQuoteId, divisionProductId, productName, productNumber, transaction);
                                            break;
                                        default:
                                            throw new PivotalApplicationException((string)this.LangDictionary.GetText("TransactionTypeNotSupported"));
                                    }
                                }
                                else
                                {
                                    throw new PivotalApplicationException(string.Format(CultureInfo.CurrentCulture, validateBuyerSelectionReturn));
                                }
                            }
                        }

                        if (pivContract)
                        {
                            // need to calculate total for the PSQ
                            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                            transitParams.Construct();
                            transitParams.SetUserDefinedParameter(1, postSaleQuoteId);
                            transitParams.SetUserDefinedParameter(2, false);

                            object parameterList = transitParams.ParameterList;

                            m_rdaSystem.Forms[FormName.HBOpportunityOptions].Execute(OpportunityAsrMethodName.CalculateTotals, ref parameterList);
                        }
                    
                    //AAB 2010-01-25 Update the post sale quote to indicate it has been updated by the integration
                    Recordset psq = this.m_objLib.GetRecordset(postSaleQuoteId, OpportunityData.TableName, "MI_Envision_Edited");
                    psq.Fields["MI_Envision_Edited"].Value = true;
                    this.m_objLib.SaveRecordset(OpportunityData.TableName, psq);
                    psq.Close();

                    // send confirmation to envision
                    SendBuyerSelectionUpdateConfirmation(contractId, transactions);
                }

        
            }
            catch (Exception ex)
            {
                // roll back the transaction on failure
                // System.EnterpriseServices.ContextUtil.SetAbort();
                throw CreateBuyerSelectionContractProcessingException(contractId, ex);
            }

            return transactionList.Substring(0, transactionList.Length - 1);
        }

        /// <summary>
        /// Instantiates a new exception with the Contract details in the description.
        /// </summary>
        /// <param name="contractId">Contact Id</param>
        /// <param name="innerException">Originating exception</param>
        /// <returns>New exception with the Contract destails in the description</returns>
        protected virtual PivotalApplicationException CreateBuyerSelectionContractProcessingException(byte[] contractId, Exception innerException)
        {
            string msg = (string)this.LangDictionary.GetText("ExceptionBuyerSelectionNoContract");

            if ((contractId != null) && (contractId.Length > 0))
            {
                Recordset oppRecords = this.PivotalDataAccess.GetRecordset(contractId, OpportunityData.TableName, new string[] { OpportunityData.RnDescriptorField });
                try
                {
                    if (oppRecords.RecordCount == 1)
                    {
                        string descriptor = (string)oppRecords.Fields[OpportunityData.RnDescriptorField].Value;
                        string id = PivotalSystem.IdToString(contractId);
                        msg = (string)this.LangDictionary.GetTextSub("ExceptionBuyerSelectionContract", new string[] { id, descriptor });
                    }
                }
                finally
                {
                    oppRecords.Close();
                }
            }

            return new PivotalApplicationException(msg, innerException);
        }


        /// <summary>
        /// Send Envision DCM back the "Received" message for the buyer selections transaction
        /// </summary>
        /// <param name="opportunityId">opportunityId</param>
        /// <param name="transactions">transactions</param>
        /// <returns></returns>
        protected virtual void SendBuyerSelectionUpdateConfirmation(byte[] opportunityId, TransactionType[] transactions)
        {
            // setup web service
            Envision.DesignCenterManager.Home.HomeWebService homeWebService = new Envision.DesignCenterManager.Home.HomeWebService(this);
            homeWebService.AuthHeaderValue = new Envision.DesignCenterManager.Home.AuthHeader();
            homeWebService.AuthHeaderValue.UserName = this.Config.EnvisionWebServiceUserName;
            homeWebService.AuthHeaderValue.Password = this.Config.EnvisionWebServicePassword;
            homeWebService.AuthHeaderValue.NHTBillingNumber = this.Config.EnvisionNHTNumber;
            homeWebService.Url = this.Config.EnvisionHomeWebServiceUrl;
            homeWebService.Timeout = this.Config.EnvisionWebServiceTimeout;

            // process confirmation webservice calls
            for (int i = 0; i < transactions.Length; i++)
            {
                try
                {
                    TransactionType transaction = transactions[i];

                    Log.WriteInformation(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("ConfirmedBS", new string[] { transaction.TransactionID.ToString()}) ));

                    XmlNode outputXml = homeWebService.UpdateSelectionStatus(transactions[i].TransactionID, "Received");

                    // validate the returned xml agains the Envision schema
                    XmlValidation.Output(outputXml);

                    // turn the returned xml into an Envision Output entity
                    EnvisionXsdGenerated.Output output = GetOutput(outputXml);

                    // if Envision returns an error, turn the error into an Exception and throw.
                    if (output.Status != EnvisionXsdGenerated.OutputStatus.Success) throw CreateSoapException(output);
                }
                catch (SoapException ex)
                {
                    throw new SoapException(ContractExceptionMessage.GetContractOptionsUpdateExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, opportunityId, transactions[i].TransactionID, ContractExceptionMessage.ContractOptionSelectionProcessing.ConfirmInventorySelectionsReciept), ex.Code, ex);
                }
                catch (Exception ex)
                {
                    throw new PivotalApplicationException(ContractExceptionMessage.GetContractOptionsUpdateExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, opportunityId, transactions[i].TransactionID, ContractExceptionMessage.ContractOptionSelectionProcessing.ConfirmInventorySelectionsReciept), ex);
                }
            }
        }

        /// <summary>
        ///     AM2010.08.26 - Modified from original Envision code to handle Irvine Company specific requirements
        ///     Get post sale quote for the related contract. If the contract has no PSQ yet then create one for it. 
        ///     Also, added a flag to indicate if a PSQ or PBQ needs to be created.
        /// </summary>
        /// <param name="contractId">contractId</param>
        /// <param name="postSaleQuoteId">postSaleQuoteId</param>
        /// <param name="oppType">oppType</param>
        protected virtual void GetPostSaleQuote(object contractId, out object postSaleQuoteId, OpportunityType oppType)
        {

            Recordset opportunityRecordset;

            //Which query to call will be based on the OpportunityType passed in.
            if (oppType == OpportunityType.InventoryQuote)
            { 
                 //Get existing PBQ for IQ
                opportunityRecordset 
                    = this.PivotalDataAccess.GetRecordset(TICEnvisionConstants.TICIntOptionSelectionsTable.Queries.TIC_ACTIVE_CHATEAU_PBQs_FOR_IQ
                    , 1, contractId, OpportunityData.OpportunityIdField);
                
            }
            else
            {
                //Get existing PSQ for Contract
                opportunityRecordset = this.PivotalDataAccess.GetRecordset(TICEnvisionConstants.TICIntOptionSelectionsTable.Queries.TIC_ACTIVE_CHATEAU_PSQs_FOR_CONTRACT, 1, contractId, OpportunityData.OpportunityIdField);
                //opportunityRecordset = this.PivotalDataAccess.GetRecordset("Env: MI Active PSQ for contract?", 1, contractId, OpportunityData.OpportunityIdField);
            }

            //Need to create Quote if doesn't already exist
            if (opportunityRecordset.RecordCount == 0)
            {                
                TransitionPointParameter transitionPointParameter = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                object executeParameters = transitionPointParameter.Construct();
                object executeUserParameters = new object[] { contractId };
                executeParameters = transitionPointParameter.SetUserDefinedParameterArray(executeUserParameters);

                m_rdaSystem.Forms[FormName.HBPostSaleQuote].Execute(OpportunityAsrMethodName.CreatePostSaleQuote, ref executeParameters);
                transitionPointParameter.GetUserDefinedParameterArray(executeParameters);
                postSaleQuoteId = (byte[])transitionPointParameter.GetUserDefinedParameter(1);
                
                //AM2010.08.26 - After creation of new Post Sale Quote by Integration we need to flag the External Source Name = Chateau
                //Didn't want to change OOTB "CreatePostSaleQuote" so updating after it is created.
                Recordset rstPSQ = this.PivotalDataAccess.GetRecordset(postSaleQuoteId, OpportunityData.TableName, "External_Source_Name");
                if (rstPSQ.RecordCount > 0)
                {
                    rstPSQ.MoveFirst();
                    rstPSQ.Fields[TICEnvisionConstants.strfEXTERNAL_SOURCE_NAME].Value = TICEnvisionConstants.TICIntOptionSelectionsTable.Constants.CHATEAU;
                    this.PivotalDataAccess.SaveRecordset(OpportunityData.TableName, rstPSQ);
                }

                //AM2010.08.26 - For Inserts add new PSQ to list to Accept changes for.  Don't need to add update items to list
                //since if an update is found, it is assumed that id was already added.  This is because if all PSQ's created during this 
                //integration should be accepted as a part of the process.  If an existing one is in the system not accepted, an error will
                //be thrown since this will more than likely be a user created Post Sale Quote.
                pstIdList.Add(postSaleQuoteId);

            }
            else
            {
                opportunityRecordset.MoveFirst();
                postSaleQuoteId = (byte[])opportunityRecordset.Fields[OpportunityData.OpportunityIdField].Value;
            }

        }

        /// <summary>
        ///     AM2010.08.26 - This method was modified from the original Envision source to accomodate
        ///     Irivine Company specific requirements
        ///     Add/Edit option in PSQ or PBQ.
        /// </summary>
        /// <param name="postSaleQuoteId">postSaleQuoteId</param>
        /// <param name="divisionProductId">divisionProductId, if this is custom option the divisionProductId is null</param>
        /// <param name="productName">productName, used for custom option</param>
        /// <param name="productNumber">productNumber, used for custom option</param>
        /// <param name="nbhdp_ProductId">nbhdp_ProductId, the related product configuration of the option. If it is custom option then null.</param>
        /// <param name="optionAvailableTo">optionAvailableTo, the Option_Available_To value of the involved product configuration</param>
        /// <param name="transaction">transaction, used to pass the manufacturer product data. If it is package option then each component will have its own manufacturer product data. </param>
        protected virtual void AddEditOptionToPostSaleQuote(object postSaleQuoteId, object divisionProductId, string productName, string productNumber, object nbhdp_ProductId, string optionAvailableTo, TransactionType transaction,
            bool isPreplot)
        {

            Recordset selectedOptionRecordSet = this.PivotalDataAccess.GetRecordset(OpportunityProductData.OpportunityProductForOpportunityDivProdAndProdNameQueryName
                    , 4, postSaleQuoteId, divisionProductId, productName, productNumber
                    , OpportunityProductData.OpportunityProductIdField, OpportunityProductData.PriceField, OpportunityProductData.UsePCOPriceField, OpportunityProductData.BuiltOptionField
                    , OpportunityProductData.SelectedField, OpportunityProductData.QuantityField, OpportunityProductData.EnvOptionSelectedDatetimeField
                    , OpportunityProductData.OptionSelectedDateField
                    );

                //SelectedOptionTypeProduct[] products;
                if (selectedOptionRecordSet.RecordCount == 0)
                {
                    selectedOptionRecordSet.Close();

                    if (divisionProductId == null)
                    {
                        AddEnvisonCustomOptionToPostSaleQuote(postSaleQuoteId, transaction.Option.OptionName, transaction.Option.OptionNumber
                            , transaction.Option.OptionDescription, transaction.Quantity, transaction.Price
                            , (transaction.CategoryGroup == null ? null : transaction.CategoryGroup.Number), (transaction.Category==null? null:transaction.Category.Number), 
                            transaction.RoomNumber, transaction.DateCreated, isPreplot);
                    }
                    else
                    {
                        object oppProductId;
                        object oppProductLocId;
                        //AM2010.08.26 - Pass price to this method to check whether or not the Price is different than 
                        //the NBHDP_Product record.  If so, flag Opportunity_Product as Price_Mismatch = true
                        AddOptionToPostSaleQuote(postSaleQuoteId, nbhdp_ProductId, transaction.Quantity, transaction.DateCreated, 
                            transaction.Price, out oppProductId, out oppProductLocId, isPreplot);

                        //AM2010.08.26 - Added to handle new option notes mappings
                        SetNotes(transaction, oppProductId);
                        
                        //set the location_id in Opp_Product_Location table. 
                        if (transaction.RoomNumber !=null && optionAvailableTo == NBHDPProductData.OptionAvailableToAllLocations)
                            //if the optionAvailableTo is "Specific Location", we do not need to set Opp_Product_Location.Location_Id, 
                            //it was already set.
                            SetLocationIdOfOpp_Product_Location(oppProductLocId, transaction.RoomNumber);

                        //set manufacturer product data for group options including the component options of type group within a pacake option
                        SetManufacturerProductData(oppProductId, oppProductLocId, transaction);

                    }
                }
                else
                {
                    if (selectedOptionRecordSet.RecordCount == 1)
                    {
                        if (DateTime.Compare(TypeConvert.ToDateTime(selectedOptionRecordSet.Fields[OpportunityProductData.EnvOptionSelectedDatetimeField].Value), transaction.DateCreated) <= 0)
                        {

                            object oppProdId = selectedOptionRecordSet.Fields[OpportunityProductData.OpportunityProductIdField].Value;
                            selectedOptionRecordSet.Close();
                            //Edit the Opp_Product_Location and opportunity__product records
                            EditOppProdLocAndOppProd(oppProdId, transaction);
                            SetNotes(transaction, oppProdId);

                        }
                        else
                        {
                            //else this is an out-of-date transaction
                            Log.WriteException(new PivotalApplicationException(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("OutOfDateBS", new string[] { TypeConvert.ToString(transaction.DateCreated), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(divisionProductId)), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(postSaleQuoteId))}))));
                        }
                    }
                    else
                    {
                        throw new PivotalApplicationException(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("MutipleProductConfigsForOption", new string[] { BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(divisionProductId)), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(postSaleQuoteId))} )));
                    }
                }
        }


        /// <summary>
        ///     AM2010.08.26 - This method was modified from the original Envision Code to accomodate
        ///     Irvine Company specific requirements.
        ///     Add Envision Custom Option to the post sale quote or Post Build Quote
        ///     optionNumber and Name is defiend by user
        ///     Category is looked up but code was removed to lookup SubCategory
        /// </summary>
        /// <param name="postSaleQuoteId">postSaleQuoteId</param>
        /// <param name="optionName">optionName</param>
        /// <param name="optionNumber">optionNumber</param>
        /// <param name="optionDescription">optionDescription</param>
        /// <param name="quantity">quantity</param>
        /// <param name="extendedPrice">extendedPrice</param>
        /// <param name="categoryGroupNumber">categoryGroupNumber. Please be aware that Envision.CategoryGroupNumber is mapped to Pivotal.CategoryNumber.</param>
        /// <param name="categoryNumber">categoryNumber. Please be aware that Envision.CategoryNumber is mapped to Pivotal.SubCategoryNumber.</param>
        /// <param name="roomNumber">roomNumber</param>
        /// <param name="transactionDatetime">transactionDatetime</param>

        protected virtual void AddEnvisonCustomOptionToPostSaleQuote(object postSaleQuoteId, string optionName, string optionNumber
            , string optionDescription, int quantity, decimal extendedPrice
            , string categoryGroupNumber, string categoryNumber, string roomNumber, DateTime transactionDatetime, bool isPreplot
            )
        {
            Recordset opportunityProductRecordset;

            opportunityProductRecordset = this.PivotalDataAccess.GetNewRecordset(OpportunityProductData.TableName);


            opportunityProductRecordset.AddNew(Type.Missing, Type.Missing);
            opportunityProductRecordset.Fields[OpportunityData.OpportunityIdField].Value = postSaleQuoteId;

            //division_id will be populated by table formula

            opportunityProductRecordset.Fields[OpportunityProductData.QuantityField].Value = quantity;
            
            //ExtendedPrice is set by table formula
            //opportunityProductRecordset.Fields[OpportunityProductData.ExtendedPriceField].Value = extendedPrice;

            opportunityProductRecordset.Fields[OpportunityProductData.PriceField].Value = extendedPrice /(decimal)quantity;
            opportunityProductRecordset.Fields[OpportunityProductData.ProductNameField].Value = optionName;
            opportunityProductRecordset.Fields[OpportunityProductData.ProductNumberField].Value = optionNumber;
            opportunityProductRecordset.Fields[OpportunityProductData.TypeField].Value = "Custom";
            opportunityProductRecordset.Fields[OpportunityProductData.SelectedField].Value = 1;
            opportunityProductRecordset.Fields[OpportunityProductData.OptionSelectedDateField].Value = DateTime.Now;
            opportunityProductRecordset.Fields[OpportunityProductData.ProductAvailableField].Value = 1;
            opportunityProductRecordset.Fields[OpportunityProductData.FilterVisibilityField].Value = 1;

            //AM2010.09.08 - Default to Built (All options from Chateau)
            opportunityProductRecordset.Fields[OpportunityProductData.BuiltOptionField].Value = 1;

            //AM2010.09.23 - Added to support Chateau sending us Pre-PLot information
            opportunityProductRecordset.Fields[TICEnvisionConstants.strfTIC_PREPLOT_OPTION].Value = TypeConvert.ToBoolean(isPreplot);


            //2008-06-17 AB added to support require code fiel
            opportunityProductRecordset.Fields[OpportunityProductData.CodeField].Value = optionNumber;
                        
            //support utility functions need for custom MI coding
            TIC_Envision_Utility util = new TIC_Envision_Utility();

            //get the division
            object neighborhoodId = this.m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.NeighborhoodIdField].FindValue(
                            this.m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.OpportunityIdField],
                            postSaleQuoteId);

            object divId = this.m_rdaSystem.Tables[NeighborhoodData.TableName].Fields[NeighborhoodData.DivisionIdField].FindValue(
                            this.m_rdaSystem.Tables[NeighborhoodData.TableName].Fields[NeighborhoodData.DivisionIdField],
                            neighborhoodId);
            
            object catId = null;
            object subCatId = null;

            //AM2010.08.26 - Category number is passed from Chateau, so use this to find existing Category based on Configuration_Type.Code_ fiedl
            //if (categoryGroupNumber != null)
            //{
            //    //opportunityProductRecordset.Fields[OpportunityProductData.CategoryIdField].Value = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(categoryGroupNumber));
            //    catId = util.GetCategory(divId, categoryGroupNumber, this.m_rdaSystem);
            //}
           
            //if (categoryNumber != null) // && catId != null)
            //{
            //    //opportunityProductRecordset.Fields[OpportunityProductData.SubCategoryIdField].Value = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(categoryNumber));
            //    subCatId = util.GetSubCategory(catId, categoryNumber, this.m_rdaSystem);
            //}
            if (categoryNumber != null) // && catId != null)
            {
                //opportunityProductRecordset.Fields[OpportunityProductData.CategoryIdField].Value = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(categoryGroupNumber));
                catId = util.GetCategory(divId, categoryNumber, this.m_rdaSystem);
            }

            //TO-DO: AM2010.08.26 - Do we need to default a Category if one can't be found? 
            if (catId == null || catId == DBNull.Value) // || subCatId == null || subCatId == DBNull.Value)
            {
                catId = util.GetCategory(divId, "MIS", this.m_rdaSystem);
                //AM2010.08.24 - Don't set Sub Category
                //subCatId = util.GetSubCategory(catId, "MIS", this.m_rdaSystem);
                
                if (catId != null && catId != DBNull.Value) // || subCatId == null || subCatId == DBNull.Value)
                {
                    opportunityProductRecordset.Fields[OpportunityProductData.CategoryIdField].Value = catId;
                    //opportunityProductRecordset.Fields[OpportunityProductData.SubCategoryIdField].Value = subCatId;
                }
            }

            
            
            opportunityProductRecordset.Fields[OpportunityProductData.OptionNotesField].Value = optionDescription;
            if (roomNumber != null)
                opportunityProductRecordset.Fields[OpportunityProductData.LocationIdField].Value = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(roomNumber));
            opportunityProductRecordset.Fields[OpportunityProductData.EnvOptionSelectedDatetimeField].Value = transactionDatetime;
            Administration administration = (Administration)m_rdaSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
            byte[] employeeId = (byte[])administration.CurrentUserRecordId;
            opportunityProductRecordset.Fields[OpportunityProductData.OptionAddedByField].Value = employeeId;
            opportunityProductRecordset.Fields[OpportunityProductData.OptionSelectionSourceField].Value = OptionSelectionSource.Envision;

            this.PivotalDataAccess.SaveRecordset(OpportunityProductData.TableName, opportunityProductRecordset);
        }

        
        
        
        /// <summary>
        ///     Add non-custom option to the post sale quote
        ///     optionNumber is Division_Product_Id
        /// </summary>
        /// <param name="postSaleQuoteId">postSaleQuoteId</param>
        /// <param name="nbhdp_ProductId">nbhdp_ProductId</param>
        /// <param name="quantity">quantity</param>
        /// <param name="transactionDatetime">transactionDatetime</param>
        /// <param name="OppProductId">OppProductId, returned new Opportunity__Product_Id</param>
        /// <param name="OppProductLocId">OppProductLocId, returned new Opp_Product_Location_Id</param>

        protected virtual void AddOptionToPostSaleQuote(object postSaleQuoteId, object nbhdp_ProductId, int quantity, DateTime transactionDatetime
            , out object OppProductId, out object OppProductLocId)
        {

            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, postSaleQuoteId);
            transitParams.SetUserDefinedParameter(2, nbhdp_ProductId);
            transitParams.SetUserDefinedParameter(3, null);
            transitParams.SetUserDefinedParameter(4, quantity);
            transitParams.SetUserDefinedParameter(5, OptionSelectionSource.Envision);
            transitParams.SetUserDefinedParameter(6, transactionDatetime);
            
            object parameterList = transitParams.ParameterList;

            m_rdaSystem.Forms[FormName.HBOpportunityOptions].Execute(OpportunityAsrMethodName.CreateOpportunityProductOption, ref parameterList);

            transitParams.GetUserDefinedParameterArray(parameterList);
            OppProductId = transitParams.GetUserDefinedParameter(1);
            OppProductLocId=transitParams.GetUserDefinedParameter(2);  


        }

        /// <summary>
        ///     AM2010.08.26 - This method was modified from the original Envision code to accomodate 
        ///     Irvine Company specific requirements. 
        ///     Add non-custom option to the post sale quote
        ///     optionNumber is Division_Product_Id
        ///     This was created because Pivotal must accept the price for options that are added in Chateau
        ///     The customer is quoted the Chateau Price and the option will need to be flagged if they two don't match.
        /// </summary>
        /// <param name="postSaleQuoteId">postSaleQuoteId</param>
        /// <param name="nbhdp_ProductId">nbhdp_ProductId</param>
        /// <param name="quantity">quantity</param>
        /// <param name="transactionDatetime">transactionDatetime</param>
        /// <param name="price">price</param>
        /// <param name="OppProductId">OppProductId, returned new Opportunity__Product_Id</param>
        /// <param name="OppProductLocId">OppProductLocId, returned new Opp_Product_Location_Id</param>

        protected virtual void AddOptionToPostSaleQuote(object postSaleQuoteId, object nbhdp_ProductId, int quantity, DateTime transactionDatetime
            ,decimal price, out object OppProductId, out object OppProductLocId, bool isPreplot)
        {

            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, postSaleQuoteId);
            transitParams.SetUserDefinedParameter(2, nbhdp_ProductId);
            transitParams.SetUserDefinedParameter(3, null);
            transitParams.SetUserDefinedParameter(4, quantity);
            transitParams.SetUserDefinedParameter(5, OptionSelectionSource.Envision);
            transitParams.SetUserDefinedParameter(6, transactionDatetime);
            
            object parameterList = transitParams.ParameterList;

            m_rdaSystem.Forms[FormName.HBOpportunityOptions].Execute(OpportunityAsrMethodName.CreateOpportunityProductOption, ref parameterList);

            transitParams.GetUserDefinedParameterArray(parameterList);
            OppProductId = transitParams.GetUserDefinedParameter(1);
            OppProductLocId = transitParams.GetUserDefinedParameter(2);

            
            //AM2010.08.25 - Now with new Option record, need to compare price with total_price sent in
            //from Chateau and set the Price Mismatch field is they don't match.
            Recordset rstOppProd = this.PivotalDataAccess.GetRecordset(OppProductId, OpportunityProductData.TableName,
                TICEnvisionConstants.strfPRICE, TICEnvisionConstants.OPPORTUNITY__PRODUCT_PRICE_MISMATCH, OpportunityProductData.BuiltOptionField,
                TICEnvisionConstants.strfTIC_PREPLOT_OPTION);
            //AM2010.09.08 - Set all Chateau options to built
            rstOppProd.Fields[OpportunityProductData.BuiltOptionField].Value = 1;
            //AM2010.09.23 - Added to support Chateau sending us pre-plot information about an option
            rstOppProd.Fields[TICEnvisionConstants.strfTIC_PREPLOT_OPTION].Value = TypeConvert.ToBoolean(isPreplot);

            if (TypeConvert.ToDecimal(rstOppProd.Fields[TICEnvisionConstants.strfPRICE].Value) != price)
            {
                rstOppProd.Fields[TICEnvisionConstants.strfPRICE].Value = price;
                rstOppProd.Fields[TICEnvisionConstants.OPPORTUNITY__PRODUCT_PRICE_MISMATCH].Value = true;
                
            }

            //Save always so that built flag gets set
            this.PivotalDataAccess.SaveRecordset(OpportunityProductData.TableName, rstOppProd);


        }
        
        /// <summary>
        ///     Delete option for the post sale quote. To remove an option from PSQ we do not delete it but set the Selected flag as 0
        ///     For the option with room information, we will delete the corresponding Opp_Product_Location record, 
        ///     and set Opportunity_Product.Selected 0 if there is no Opp_Product_Location record left.
        ///     Once option got removed, we need re-calculate total ammount for the PSQ.
        /// </summary>
        /// <remarks>This method assumes that there are no hierarchical option configurations</remarks>
        /// <param name="postSaleQuoteId">postSaleQuoteId</param>
        /// <param name="divisionProductId">divisionProductId, it is optionNumber gotten from transaction for non-custom option, otherwise null</param>
        /// <param name="productName">productName, used for custom option</param>
        /// <param name="productNumber">productNumber, used for custom option</param>
        /// <param name="transaction">transaction</param>
        protected virtual void DeleteOptionOnPostSaleQuote(object postSaleQuoteId, object divisionProductId, string productName, string productNumber, TransactionType transaction)
        {
            //if (the option(divisionProductId) already exists in the Opportunity__Product table for the PSQ)
            //  if (the receiving transactionDatetime is later then the one in the Opportunity__Product record)
            //    then delete the existing option in PSQ (actually set the selected as 0)
            //  else
            //    ignore the out-of-date transaction, and return Success status to Envision
            //else
            //  ignore the DeleteOptionOnPostSaleQuote, log the exception, and return Failure status to Envision

            Recordset selectedOptionRecordSet = this.PivotalDataAccess.GetRecordset(OpportunityProductData.OpportunityProductForOpportunityDivProdAndProdNameQueryName
                                , 4, postSaleQuoteId, divisionProductId, productName, productNumber
                                , OpportunityProductData.EnvOptionSelectedDatetimeField
                                , OpportunityProductData.OpportunityProductIdField
                                );


                if (selectedOptionRecordSet.RecordCount == 0)
                {
                    //the option to be deleted cannot be found
                    if (divisionProductId==null)
                        throw new PivotalApplicationException(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("CannotFindCustomOptionToBeDeleted", new string[] { productName, productNumber, BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(postSaleQuoteId)) })));
                    else
                        throw new PivotalApplicationException(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("CannotFindOptionToBeDeleted", new string[] { BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(divisionProductId)), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(postSaleQuoteId)) } )));
                }
                else
                {
                    if (selectedOptionRecordSet.RecordCount == 1)
                    {
                        if (DateTime.Compare(TypeConvert.ToDateTime(selectedOptionRecordSet.Fields[OpportunityProductData.EnvOptionSelectedDatetimeField].Value), transaction.DateCreated) < 0)
                        {
                            selectedOptionRecordSet.MoveFirst();
                            DeleteOppProdLocAndOppProd(selectedOptionRecordSet.Fields[OpportunityProductData.OpportunityProductIdField].Value, transaction);
                            
                        }
                        else
                        {
                            //else this is an out-of-date transaction
                            if (divisionProductId == null)
                                Log.WriteException(new PivotalApplicationException(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("OutOfDateCustomOptDeletion", new string[] { TypeConvert.ToString(transaction.DateCreated), productName, productNumber, BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(postSaleQuoteId))}) )));
                            else
                                Log.WriteException(new PivotalApplicationException(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("OutOfDateOptionDeletion", new string[] { TypeConvert.ToString(transaction.DateCreated), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(divisionProductId)), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(postSaleQuoteId))}) )));
                        }
                    }
                    else
                    {
                        if (divisionProductId == null)
                            throw new PivotalApplicationException(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("DuplicatesOfCustomOption", new string[] { productName, productNumber, BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(postSaleQuoteId))}) ));
                        else
                            throw new PivotalApplicationException(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("DuplicatesOfOption", new string[] { BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(divisionProductId)), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(postSaleQuoteId))})));
                    }
                }


        }

        /// <summary>
        ///     Get most specified NBHDP_Product_Id from the contract and the Divisiion_Product_Id
        /// </summary>
        /// <param name="ContractId">ContractId</param>
        /// <param name="divisionProductId">divisionProductId</param>
        /// <returns>Neighborhood_Product_Id of the Divisiion_Product_Id for the contract</returns>        
        protected virtual byte[] GetMostSpecificNbhdpProductFromContractAndDivisionProduct(object ContractId, object divisionProductId)
        {
            Recordset nbhdpProductRecordset;
            object planId, releaseId, neighborhoodId, divisionId, regionId;
            string planCode;
            string elevationCode;

            planId = m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.PlanNameIdField].Index(ContractId);
            planCode = TypeConvert.ToString(m_rdaSystem.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.PlanCodeField].Index(planId));
            elevationCode = TypeConvert.ToString(m_rdaSystem.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.ElevationCodeField].Index(planId));
            releaseId = m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.NBHDPhaseIdField].Index(ContractId);
            neighborhoodId = m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.NeighborhoodIdField].Index(ContractId);
            divisionId = m_rdaSystem.Tables[NeighborhoodData.TableName].Fields[NeighborhoodData.DivisionIdField].Index(neighborhoodId);
            regionId = m_rdaSystem.Tables[DivisionData.TableName].Fields[DivisionData.RegionIdField].Index(divisionId);
            
            nbhdpProductRecordset = this.PivotalDataAccess.GetRecordset(NBHDPProductData.OptionsAvailableForPlanAndDivProdQueryName
                        , 7, (object)divisionProductId, planId, planCode, regionId, divisionId, neighborhoodId, releaseId
                        , NBHDPProductData.NBHDPProductIdField, NBHDPProductData.WCLevelField);

            if (nbhdpProductRecordset.RecordCount == 0)
            {
                string errorMessage = (string)this.LangDictionary.GetTextSub("CannotFindNBHDP_ProdFromPlanAndDivProd", new string[] { m_rdaSystem.IdToString(planId), m_rdaSystem.IdToString(divisionProductId) });
                throw new PivotalApplicationException(errorMessage);
            }
            nbhdpProductRecordset.Sort = NBHDPProductData.WCLevelField + " desc";
            nbhdpProductRecordset.MoveFirst();
            return (byte[])nbhdpProductRecordset.Fields[NBHDPProductData.NBHDPProductIdField].Value;
        }

        /// <summary>
        ///     Check if the BuyerSelection transaction is valid,
        ///       rule1:
        ///             the referencing NBHDP_Product record should be active
        ///       rule2:
        ///             if room number is provided, then it must be a valid room 
        ///             the referencing NBHDP_Product is assigned to 
        /// </summary>
        /// <param name="transaction">transaction</param>
        /// <param name="postSaleQuoteId">postSaleQuoteId</param>
        /// <param name="divisionProductId">divisionProductId</param>
        /// <param name="nbhdp_ProductId">nbhdp_ProductId</param>
        /// <param name="optionAvailableTo">optionAvailableTo</param>
        /// <returns>empty string "": valid, for non-custom option, nbhdp_ProductId and optionAvailableTo are defined; 
        ///                                  for custom option return null nbhdp_ProductId and empty optionAvailableTo.
        ///          invalid reason: invalid, nbhdp_ProductId return null and optionAvailableTo return ""
        /// </returns>        
        protected virtual string ValidateBuyerSelection(TransactionType transaction, object postSaleQuoteId, object divisionProductId, out object nbhdp_ProductId, out string optionAvailableTo)
        {
            if (divisionProductId == null) //custom option
            {
                nbhdp_ProductId = null;
                optionAvailableTo = "";
                if (transaction.RoomNumber==null)
                    return ("");
                else 
                {
                    //check if the room is in the room list assigned to the plan of the postSaleQuote
                    object planAssignmentId = m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.PlanNameIdField].Index(postSaleQuoteId);
                    object planId = m_rdaSystem.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.DivisionProductIdField].Index(planAssignmentId);
                    object locationId = m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(transaction.RoomNumber));
                    Recordset locationRecordset = this.PivotalDataAccess.GetRecordset(DivisionProductLocationsData.DivProdLocationsForPlanAndLocationQuery
                                , 2, planId, locationId
                                , LocationData.LocationIdField);

                    if (locationRecordset.RecordCount == 0)
                    {
                        return (string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("InvalidLocationInSelectedOption", new string[] { transaction.TransactionID.ToString(), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(locationId))
                            , BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(planId))})));
                    }
                    else
                    {
                        return ("");
                    }
                }
            }
            else
            { //regular option
                nbhdp_ProductId = GetMostSpecificNbhdpProductFromContractAndDivisionProduct(postSaleQuoteId, divisionProductId);
                optionAvailableTo = TypeConvert.ToString(m_rdaSystem.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.OptionAvailableToField].Index(nbhdp_ProductId));

                bool nbhdp_Product_Inactive = TypeConvert.ToBoolean(m_rdaSystem.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.InactiveField].Index(nbhdp_ProductId));
                //AB 2008-09-08 this must be allowed in all cases because of design center timing and the delete\add needed to add product information
                if (nbhdp_Product_Inactive && 0==1)
                {
                    return (string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("InvalidSelectedOptionReferencingInactiveProductConfig", new string[] { transaction.TransactionID.ToString(), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(nbhdp_ProductId)) })));
                }
                else
                {
                    if (transaction.RoomNumber==null)
                        return ("");
                    else
                    {
                        switch (optionAvailableTo)
                        {
                            case NBHDPProductData.OptionAvailableToWholeHouse:
                                return (string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("InvalidSelectedOptionWithRoomInfoConflictWithOptAvailableTo", new string[] { transaction.TransactionID.ToString(), transaction.RoomNumber, BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(nbhdp_ProductId)), NBHDPProductData.OptionAvailableToWholeHouse})
                                    
                                    ));
                                //break;
                            case NBHDPProductData.OptionAvailableToSpecificLocation:
                                string strNBHDPProductLocationId=BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(m_rdaSystem.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.LocationIdField].Index(nbhdp_ProductId)));
                                if (transaction.RoomNumber==strNBHDPProductLocationId)
                                    return ("");
                                else
                                    return (string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("InvalidSelectedOptionWithIncorrectRoomInfo", new string[] {transaction.TransactionID.ToString(), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(transaction.RoomNumber)), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(nbhdp_ProductId)), strNBHDPProductLocationId })));
                                //break;
                            case NBHDPProductData.OptionAvailableToAllLocations:
                                //check if the room is in the room list assigned to the plan of the postSaleQuote
                                object planAssignmentId = m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.PlanNameIdField].Index(postSaleQuoteId);
                                object planId = m_rdaSystem.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.DivisionProductIdField].Index(planAssignmentId);
                                object locationId = m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(transaction.RoomNumber));
                                Recordset locationRecordset = this.PivotalDataAccess.GetRecordset(DivisionProductLocationsData.DivProdLocationsForPlanAndLocationQuery
                                            , 2, planId, locationId
                                            , LocationData.LocationIdField);

                                if (locationRecordset.RecordCount == 0)
                                {
                                    return (string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("InvalidSelectedOptionReferenceingNonexistentRoom", new string[] { transaction.TransactionID.ToString(), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(locationId)), BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(planId))})));
                                }
                                else
                                {
                                    return ("");
                                }
                                
                                //break;
                            default:
                                throw new PivotalApplicationException(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("InvalidOptionAvailableTo", new string[] { optionAvailableTo })));
                        }

                    }
                }


            }
        }

        /// <summary>
        ///     This is a generic function, will get a recordset based on query, and set speicifed fields with the values provided.
        /// </summary>
        /// <param name="queryName">queryName</param>
        /// <param name="parameterNumber">parameterNumber</param>
        /// <param name="parameterFieldNameValueArray">parameterFieldNameValueArray
        /// it uses the below format,
        /// query_para1, query_para2,...,field1_name, field1_value, field2_name,field2_value,...
        ///</param>
        ///<return>number of records affected</return>

        protected virtual int EditTableRecords(string queryName, int parameterNumber, params object[] parameterFieldNameValueArray)
        {
            ArrayList parameterFieldNameList = new ArrayList();
            //int countOfFields=(parameterFieldNameValueArray.Length-parameterNumber)/2;
            int i;
            for (i = 0; i < parameterNumber; i++) //get query parameter values
                parameterFieldNameList.Add(parameterFieldNameValueArray[i]);
            for (i = parameterNumber; i < parameterFieldNameValueArray.Length; i+=2) //get field name list
                parameterFieldNameList.Add(parameterFieldNameValueArray[i]);
            object[] parameterFieldNameArray = parameterFieldNameList.ToArray();

            int numebrOfRecAffected = 0;
            Recordset recordSet = this.PivotalDataAccess.GetRecordset(queryName, parameterNumber, parameterFieldNameArray);
            if (recordSet.RecordCount > 0)
            {
                recordSet.MoveFirst();
                while (!recordSet.EOF)
                {
                    for (i = parameterNumber; i < parameterFieldNameValueArray.Length; i += 2)
                    {
                        recordSet.Fields[parameterFieldNameValueArray[i].ToString()].Value = parameterFieldNameValueArray[i + 1] == null ? DBNull.Value : parameterFieldNameValueArray[i + 1];
                    }
                    ++numebrOfRecAffected;
                    recordSet.MoveNext();
                }
                IRQuery objQuery = m_rdaSystem.Queries[queryName];
                this.PivotalDataAccess.SaveRecordset(objQuery.Table.TableName, recordSet);
            }
            return numebrOfRecAffected;
        }


        /// <summary>
        ///     This is a generic function, will get a recordset based on table primary key value, and set speicifed fields with the values provided.
        /// </summary>
        /// <param name="recordId">recordId, primary key value of a table record</param>
        /// <param name="tableName">tableName</param>
        /// <param name="parameterFieldNameValueArray">parameterFieldNameValueArray
        /// it uses the below format,
        /// field1_name, field1_value, field2_name,field2_value,...
        ///</param>
        ///<return>number of records affected</return>

        protected virtual int EditTableRecords(object recordId, string tableName, params object[] parameterFieldNameValueArray)
        {
            ArrayList parameterFieldNameList = new ArrayList();
            int i;
            for (i = 0; i < parameterFieldNameValueArray.Length; i += 2) //get field name list
                parameterFieldNameList.Add(parameterFieldNameValueArray[i]);
            object[] parameterFieldNameArray = parameterFieldNameList.ToArray();

            int numebrOfRecAffected = 0;
            Recordset recordSet = this.PivotalDataAccess.GetRecordset(recordId, tableName, parameterFieldNameArray);
            if (recordSet.RecordCount > 0)
            {
                recordSet.MoveFirst();
                for (i = 0; i < parameterFieldNameValueArray.Length; i += 2)
                {
                    recordSet.Fields[parameterFieldNameValueArray[i].ToString()].Value = parameterFieldNameValueArray[i + 1] == null ? DBNull.Value : parameterFieldNameValueArray[i + 1];
                }
                this.PivotalDataAccess.SaveRecordset(tableName, recordSet);
                ++numebrOfRecAffected;
            }
            return numebrOfRecAffected;
        }


        /// <summary>
        ///     This is a generic function, will add a new record to a table with speicifed fields with the values provided.
        /// </summary>
        /// <param name="tableName">tableName</param>
        /// <param name="parameterFieldNameValueArray">parameterFieldNameValueArray
        /// it uses the below format,
        /// field1_name, field1_value, field2_name,field2_value,...
        ///</param>
        ///<returns>the new record primary key</returns>

        protected virtual object AddTableRecord(string tableName, params object[] parameterFieldNameValueArray)
        {
            ArrayList parameterFieldNameList = new ArrayList();
            int i;
            for (i = 0; i < parameterFieldNameValueArray.Length; i += 2) //get field name list
                parameterFieldNameList.Add(parameterFieldNameValueArray[i]);
            object[] parameterFieldNameArray = parameterFieldNameList.ToArray();

            Recordset recordSet = this.PivotalDataAccess.GetNewRecordset(tableName, parameterFieldNameArray);
            recordSet.AddNew(Type.Missing, Type.Missing);

            for (i = 0; i < parameterFieldNameValueArray.Length; i += 2)
            {
                recordSet.Fields[parameterFieldNameValueArray[i].ToString()].Value = parameterFieldNameValueArray[i + 1] == null ? DBNull.Value : parameterFieldNameValueArray[i + 1];
            }
            this.PivotalDataAccess.SaveRecordset(tableName, recordSet);
            
            return recordSet.Fields[DataAccess.GetDefaultRecordIdFieldName(tableName)].Value;
        }




        /// <summary>
        ///     This will set the locaiton_Id for all related opp_Product_Loc records. 
        ///     If it is package product, then the invovled component opp_product_locs records are set as well.
        /// </summary>
        /// <param name="oppProductLocId">oppProductLocId</param>
        /// <param name="roomNumber">roomNumber</param>

        protected virtual void SetLocationIdOfOpp_Product_Location(object oppProductLocId, string roomNumber)
        {
            Recordset oppProdLocRecordSet = this.PivotalDataAccess.GetRecordset(OppProductLocationData.QueryOppProdLocationsforOppProdLocId
                , 2, oppProductLocId, oppProductLocId
                , OppProductLocationData.LocationIdField
                );
            if (oppProdLocRecordSet.RecordCount > 0)
            {
                oppProdLocRecordSet.MoveFirst();
                while (!oppProdLocRecordSet.EOF)
                {

                    oppProdLocRecordSet.Fields[OppProductLocationData.LocationIdField].Value = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(roomNumber));
                    oppProdLocRecordSet.MoveNext();
                }
                this.PivotalDataAccess.SaveRecordset(OppProductLocationData.TableName, oppProdLocRecordSet);
            }
        }

        /// <summary>
        ///     AM2010.08.26 - This method was modified from the original Envision code to 
        ///     accomodate Irvine Company specific requirements
        ///     This will edit Opp_Product_Location and Opportunity__Product table records. 
        ///     Only category is being assigned and not Sub Category
        /// </summary>
        /// <param name="oppProductId">oppProductId</param>
        /// <param name="transaction">transaction</param>
        protected virtual void EditOppProdLocAndOppProd(object oppProductId, TransactionType transaction)
        {
            object locationId=null;
            object oppProdLocId;
            string pivotalNotes=null;

            if (transaction.Option.OptionType == SelectedOptionTypeOptionType.Custom)
            {
                //GetGeneratedFileList price from DCM
                //Change the Opportunity_Product Record
                if (transaction.RoomNumber != null)
                    locationId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(transaction.RoomNumber));

                //AB 020708 CHANGED TO SUPPORT OPTION DELETE AND UPDATE FROM PIVOTAL

                //AB Get the selected status of the option. If it is selected use the current price. Else use the price provided by Envision
                bool cusSelected = (bool)this.m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.SelectedField]
                .FindValue(this.m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.OpportunityProductIdField],
                oppProductId);

                decimal cusPrice = TypeConvert.ToDecimal(this.m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.PriceField]
                .FindValue(this.m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.OpportunityProductIdField],
                oppProductId));

                object cusSelSource = this.m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.OptionSelectionSourceField]
                .FindValue(this.m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.OpportunityProductIdField],
                oppProductId);

                string categoryNumber = transaction.Category == null ? null : transaction.Category.Number;
                
               
                //support utility functions need for custom MI coding
                TIC_Envision_Utility util = new TIC_Envision_Utility();

                object catId = null;
                object subCatId = null;


                //AM2010.08.25 - Chateau only sends Category so we will only lookup Configuration_Type on Code_,  no subcategory
                //if (categoryGroupNumber != null)
                //{
                //    //opportunityProductRecordset.Fields[OpportunityProductData.CategoryIdField].Value = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(categoryGroupNumber));
                //    catId = util.GetCategory(divId, categoryGroupNumber, this.m_rdaSystem);
                //}                
                //if (categoryNumber != null) // && catId != null)
                //{
                //    //opportunityProductRecordset.Fields[OpportunityProductData.SubCategoryIdField].Value = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(categoryNumber));
                //    subCatId = util.GetSubCategory(catId, categoryNumber, this.m_rdaSystem);
                //}
                if (categoryNumber != null) // && catId != null)
                {
                    //opportunityProductRecordset.Fields[OpportunityProductData.CategoryIdField].Value = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(categoryGroupNumber));
                    catId = util.GetCategory(null, categoryNumber, this.m_rdaSystem);
                }
               
                //TO-DO : AM2010.08.25 - Should we default a category if one is not passed?
                if (catId == null || catId == DBNull.Value) // || subCatId == null || subCatId == DBNull.Value)
                {
                    catId = util.GetCategory(null, "MIS", this.m_rdaSystem);
                    //subCatId = util.GetSubCategory(catId, "Mis", this.m_rdaSystem);
                }


                if (!cusSelected)
                {
                    //price = transaction.Price;
                    cusPrice = transaction.Price / (decimal)transaction.Quantity;
                    cusSelSource = 1;
                }
                //EditTableRecords(oppProductId, OpportunityProductData.TableName
                //    , OpportunityProductData.SelectedField, 1
                //    , OpportunityProductData.QuantityField, transaction.Quantity
                //    , OpportunityProductData.PriceField, transaction.Price / (decimal)transaction.Quantity
                //    , OpportunityProductData.EnvOptionSelectedDatetimeField, transaction.DateCreated
                //    , OpportunityProductData.LocationIdField, locationId
                //    , OpportunityProductData.OptionNotesField, transaction.Option.OptionDescription
                // );
                EditTableRecords(oppProductId, OpportunityProductData.TableName
                    , OpportunityProductData.SelectedField, 1
                    , OpportunityProductData.QuantityField, transaction.Quantity
                    , OpportunityProductData.PriceField, cusPrice
                    , OpportunityProductData.EnvOptionSelectedDatetimeField, transaction.DateCreated
                    , OpportunityProductData.LocationIdField, locationId
                    , OpportunityProductData.OptionNotesField, transaction.Option.OptionDescription
                    , OpportunityProductData.OptionSelectionSourceField, cusSelSource
                    , OpportunityProductData.CategoryIdField, catId
                    //, OpportunityProductData.SubCategoryIdField, subCatId
                 );
            }
            else //regular option
            {
                // need to get the option price and built info.
                TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                transitParams.Construct();
                transitParams.SetUserDefinedParameter(1, oppProductId);
                object parameterList = transitParams.ParameterList;
                m_rdaSystem.Forms[FormName.HBOpportunityOptions].Execute(OpportunityAsrMethodName.GetReSelectedOptionPriceAndBuiltInfo, ref parameterList);
                transitParams.GetUserDefinedParameterArray(parameterList);

                //AB 020708 CHANGED TO SUPPORT OPTION DELETE AND UPDATE FROM PIVOTAL
                //Change the Opportunity_Product Record
                //EditTableRecords(oppProductId, OpportunityProductData.TableName
                //    , OpportunityProductData.SelectedField, 1
                //    , OpportunityProductData.QuantityField, transaction.Quantity
                //    , OpportunityProductData.PriceField, TypeConvert.ToDecimal(transitParams.GetUserDefinedParameter(1))
                //    , OpportunityProductData.UsePCOPriceField, TypeConvert.ToBoolean(transitParams.GetUserDefinedParameter(2))
                //    //do not set the option Built, olny use Post Cut-off price when homesite's construction stage passed the involved division_product's construction stage.
                //    //                , OpportunityProductData.BuiltOptionField, TypeConvert.ToBoolean(transitParams.GetUserDefinedParameter(3))
                //    //                , OpportunityProductData.BuiltOptionField, false
                //    , OpportunityProductData.EnvOptionSelectedDatetimeField, transaction.DateCreated
                //    );

                //AB Get the selected status of the option. If it is selected use the current price. Else use the price provided by Envision
                bool selected = (bool)this.m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.SelectedField]
                .FindValue(this.m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.OpportunityProductIdField],
                oppProductId);

                decimal price = TypeConvert.ToDecimal(this.m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.PriceField]
                .FindValue(this.m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.OpportunityProductIdField],
                oppProductId));

                object selSource = this.m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.OptionSelectionSourceField]
                .FindValue(this.m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.OpportunityProductIdField],
                oppProductId);

                              

                if (!selected)
                {
                    //price = transaction.Price;
                    price = transaction.Price / (decimal)transaction.Quantity;
                    selSource = 1;
                }

                EditTableRecords(oppProductId, OpportunityProductData.TableName
                    , OpportunityProductData.SelectedField, 1
                    , OpportunityProductData.QuantityField, transaction.Quantity
                    , OpportunityProductData.PriceField, price
                    , OpportunityProductData.UsePCOPriceField, TypeConvert.ToBoolean(transitParams.GetUserDefinedParameter(2))
                    //do not set the option Built, olny use Post Cut-off price when homesite's construction stage passed the involved division_product's construction stage.
                    //                , OpportunityProductData.BuiltOptionField, TypeConvert.ToBoolean(transitParams.GetUserDefinedParameter(3))
                    //                , OpportunityProductData.BuiltOptionField, false
                    , OpportunityProductData.EnvOptionSelectedDatetimeField, transaction.DateCreated
                    , OpportunityProductData.OptionSelectionSourceField, selSource
                    );
                if (transaction.RoomNumber == null)
                {//this is option selection for whole house
                    //Change the quantity in dummy Opp_Prod_Loc Record
                    EditTableRecords(OppProductLocationData.QueryOppProdLocationsforOppProdId, 1, oppProductId, OppProductLocationData.LocationQuantityField,transaction.Quantity);

                    Recordset oppProdLocRecordset = this.PivotalDataAccess.GetRecordset(OppProductLocationData.QueryOppProdLocationsforOppProdId, 1, oppProductId
                                                    , OppProductLocationData.OppProductLocationIdField, OppProductLocationData.NotesField);
                    oppProdLocId = oppProdLocRecordset.Fields[OppProductLocationData.OppProductLocationIdField].Value;
                    pivotalNotes = TypeConvert.ToString(oppProdLocRecordset.Fields[OppProductLocationData.NotesField].Value);
                    oppProdLocRecordset.Close();
                }
                else
                {//this is option selection with room info
                    locationId=(byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(transaction.RoomNumber));
                    if (EditTableRecords(OppProductLocationData.QueryOppProdLocationsforOppProdAndLocation, 2, oppProductId,locationId
                        , OppProductLocationData.LocationQuantityField,transaction.Quantity
                        ) == 0)
                    {//this is new location for this opportunity product, we need to add a new Opp_Prod_Loc record
                        AddTableRecord(OppProductLocationData.TableName
                            , OppProductLocationData.LocationIdField,locationId
                            , OppProductLocationData.OpportunityProductIdField,oppProductId
                            , OppProductLocationData.LocationQuantityField,transaction.Quantity
                            , OppProductLocationData.OpportunityIdField,m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.OpportunityIdField].Index(oppProductId)
                            , OppProductLocationData.OptionSelectedDateField, DateTime.Now
                            );
                    }

                    Recordset oppProdLocRecordset = this.PivotalDataAccess.GetRecordset(OppProductLocationData.QueryOppProdLocationsforOppProdAndLocation, 2, oppProductId, locationId
                                                    , OppProductLocationData.OppProductLocationIdField, OppProductLocationData.NotesField);
                    oppProdLocId = oppProdLocRecordset.Fields[OppProductLocationData.OppProductLocationIdField].Value;
                    pivotalNotes = TypeConvert.ToString(oppProdLocRecordset.Fields[OppProductLocationData.NotesField].Value);
                    oppProdLocRecordset.Close();

                    RecalculateQuantityForOppProd(oppProductId);
                }
                //AAB 2010-05-17 No need to set notes at this level. Customized for MI
                //set the option selection notes.
                /*string newNotes = GetNotes(transaction);
                if ("Other Notes: " + pivotalNotes != newNotes)
                {
                    EditTableRecords(oppProdLocId, OppProductLocationData.TableName
                        , OppProductLocationData.NotesField, GetNotes(transaction)
                        );
                }
                */
                if (transaction.Option.OptionType == SelectedOptionTypeOptionType.Package)
                    EditPackageOppProdLocAndOppProd(oppProductId, transaction);

                //set manufacturer product data for group options including the component options of type group within a pacake option
                SetManufacturerProductData(oppProductId, oppProdLocId, transaction);
            }

        }

        /// <summary>
        ///     Get 4 type notes from transaction and concatenate into one.
        /// </summary>
        /// <param name="transaction">transaction</param>
        /// <returns>Notes from transaction</returns>
        protected virtual string GetNotes(TransactionType transaction)
        {
            string notes=string.Empty;
            string otherNotes = string.Empty; //leave Other Notes at the last.
            string notesLabel;

            if (transaction.Notes != null)
            {
                notesLabel=(string)this.LangDictionary.GetText("Notes");
                for (int i = 0; i < transaction.Notes.Length; ++i)
                {
                    if (transaction.Notes[i].Text != null)
                    {
                        if (transaction.Notes[i].Type == NoteTypeType.Other)
                            otherNotes = transaction.Notes[i].Type + notesLabel + transaction.Notes[i].Text + "\r\n";
                        else
                            notes += transaction.Notes[i].Type + notesLabel + transaction.Notes[i].Text + "\r\n";
                    }
                }
                notes += otherNotes;
            }    
            return (notes==string.Empty? null: notes);
        }

        /// <summary>
        /// AM2010.08.26 - Updated this method to accomodate custom TIC Notes fields on 
        /// the Opportunity__Product
        /// Sets the option notes in custom TIC fields
        /// </summary>
        /// <param name="transaction">transaction</param>
        /// <returns>Notes from transaction</returns>
        protected virtual void SetNotes(TransactionType transaction, object oppProdId)
        {
            string styleNotes = string.Empty;
            string locationNotes = string.Empty;
            string colorNotes = string.Empty;
            string otherNotes = string.Empty;


            if (transaction.Notes != null)
            {
                for (int i = 0; i < transaction.Notes.Length; ++i)
                {
                    if (transaction.Notes[i].Text != null)
                    {
                        if (transaction.Notes[i].Type == NoteTypeType.Other)
                        {
                            otherNotes = transaction.Notes[i].Text;
                        }
                        else if (transaction.Notes[i].Type == NoteTypeType.Color)
                        {
                            colorNotes = transaction.Notes[i].Text;
                        }
                        else if (transaction.Notes[i].Type == NoteTypeType.Location)
                        {
                            locationNotes = transaction.Notes[i].Text;
                        }
                        else if (transaction.Notes[i].Type == NoteTypeType.Style)
                        {
                            styleNotes = transaction.Notes[i].Text;
                        }
                    }
                }

                //AM2010.08.24 - Changed note fields to map to TIC specific notes fields                
                EditTableRecords(oppProdId, OpportunityProductData.TableName, 
                    TICEnvisionConstants.strfTIC_COLOR_NOTES, colorNotes, TICEnvisionConstants.strfTIC_STYLE_NOTES, styleNotes,
                    TICEnvisionConstants.strfTIC_LOCATION_NOTES, locationNotes, TICEnvisionConstants.strfTIC_OPTION_NOTES, otherNotes);

            }
            
        }

        /// <summary>
        ///     oppProductId is a package, this will edit Opp_Product_Location 
        ///     and Opportunity__Product table records for the underlying components. 
        /// </summary>
        /// <param name="oppProductId">oppProductId</param>
        /// <param name="transaction">transaction</param>
        protected virtual void EditPackageOppProdLocAndOppProd(object oppProductId, TransactionType transaction)
        {
            Recordset componentOppProdRecordset = this.PivotalDataAccess.GetRecordset(OpportunityProductData.OpportunityProductsForParentPacakgeQueryName
                , 1, oppProductId
                , OpportunityProductData.OpportunityProductIdField
                , OpportunityProductData.SelectedField
                , OpportunityProductData.QuantityField
                , OpportunityProductData.PriceField
                , OpportunityProductData.UsePCOPriceField
                , OpportunityProductData.BuiltOptionField
                , OpportunityProductData.EnvOptionSelectedDatetimeField
                );

            if (componentOppProdRecordset.RecordCount > 0)
            {
                componentOppProdRecordset.MoveFirst();
                
                //get the parent Opp_Prod_Loc_Id
                object parentOppProdLocId=null;
                object locationId = null;
                if (transaction.RoomNumber != null)
                {
                    locationId=(byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(transaction.RoomNumber));
                    Recordset parentOppProdLocRecordset = this.PivotalDataAccess.GetRecordset(OppProductLocationData.QueryOppProdLocationsforOppProdAndLocation
                        , 2, oppProductId, locationId
                        , OppProductLocationData.OppProductLocationIdField
                        );
                    parentOppProdLocId = parentOppProdLocRecordset.Fields[OppProductLocationData.OppProductLocationIdField].Value;
                }

                while (!componentOppProdRecordset.EOF)
                {
                    object componentOppProdId = componentOppProdRecordset.Fields[OpportunityProductData.OpportunityProductIdField].Value;
                    // need to get the option price and built info.
                    TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                    transitParams.Construct();
                    transitParams.SetUserDefinedParameter(1, componentOppProdId);
                    object parameterList = transitParams.ParameterList;
                    m_rdaSystem.Forms[FormName.HBOpportunityOptions].Execute(OpportunityAsrMethodName.GetReSelectedOptionPriceAndBuiltInfo, ref parameterList);
                    transitParams.GetUserDefinedParameterArray(parameterList);

                    //Change the Opportunity_Product Record
                    componentOppProdRecordset.Fields[OpportunityProductData.SelectedField].Value = 1;
                    componentOppProdRecordset.Fields[OpportunityProductData.QuantityField].Value = transaction.Quantity;
                    componentOppProdRecordset.Fields[OpportunityProductData.PriceField].Value = TypeConvert.ToDecimal(transitParams.GetUserDefinedParameter(1));
                    componentOppProdRecordset.Fields[OpportunityProductData.UsePCOPriceField].Value = TypeConvert.ToBoolean(transitParams.GetUserDefinedParameter(2));
                    //do not set the option Built, olny use Post Cut-off price when homesite's construction stage passed the involved division_product's construction stage.
                    //componentOppProdRecordset.Fields[OpportunityProductData.BuiltOptionField].Value = TypeConvert.ToBoolean(transitParams.GetUserDefinedParameter(3));
                    //componentOppProdRecordset.Fields[OpportunityProductData.BuiltOptionField].Value = false;
                    componentOppProdRecordset.Fields[OpportunityProductData.EnvOptionSelectedDatetimeField].Value = transaction.DateCreated;

                    if (transaction.RoomNumber == null)
                    {//this is option selection for whole house
                        //Change the quantity in dummy Opp_Prod_Loc Record
                        EditTableRecords(OppProductLocationData.QueryOppProdLocationsforOppProdId, 1, componentOppProdId, OppProductLocationData.LocationQuantityField, transaction.Quantity);
                    }
                    else
                    {//this is option selection with room info
                        if (EditTableRecords(OppProductLocationData.QueryOppProdLocationsforOppProdAndLocation, 2, componentOppProdId, locationId
                            , OppProductLocationData.LocationQuantityField, transaction.Quantity
                            ) == 0)
                        {//this is new location for this opportunity product, we need to add a new Opp_Prod_Loc record
                            AddTableRecord(OppProductLocationData.TableName
                                , OppProductLocationData.LocationIdField, locationId
                                , OppProductLocationData.OpportunityProductIdField, componentOppProdId
                                , OppProductLocationData.LocationQuantityField, transaction.Quantity
                                , OppProductLocationData.OpportunityIdField, m_rdaSystem.Tables[OpportunityProductData.TableName].Fields[OpportunityProductData.OpportunityIdField].Index(oppProductId)
                                , OppProductLocationData.OptionSelectedDateField, DateTime.Now
                                , OppProductLocationData.ParentPackageOppProdLocIdField, parentOppProdLocId
                                );
                        }

                        //RecalculateQuantityForOppProd(componentOppProdId);
                        Recordset oppProdLocRecordSet = this.PivotalDataAccess.GetRecordset(OppProductLocationData.QueryOppProdLocationsforOppProdId
                                , 1, componentOppProdId, OppProductLocationData.LocationQuantityField);
                        int optQuantity = 0;
                        if (oppProdLocRecordSet.RecordCount > 0)
                        {
                            oppProdLocRecordSet.MoveFirst();
                            while (!oppProdLocRecordSet.EOF)
                            {
                                optQuantity = optQuantity + TypeConvert.ToInt32(oppProdLocRecordSet.Fields[OppProductLocationData.LocationQuantityField].Value);
                                oppProdLocRecordSet.MoveNext();
                            }
                        }
                        componentOppProdRecordset.Fields[OpportunityProductData.QuantityField].Value = optQuantity;
                    }
                    componentOppProdRecordset.MoveNext();
                }
                this.PivotalDataAccess.SaveRecordset(OpportunityProductData.TableName, componentOppProdRecordset);
            }

        }

    
    
        /// <summary>
        ///     This will recalculate the quantities from Opp_Product_Location for the provided oppProductId
        ///     and set the Opportunity__Product.Quantity. 
        /// </summary>
        /// <param name="oppProductId">oppProductId</param>
        protected virtual void RecalculateQuantityForOppProd(object oppProductId)
        {
            Recordset oppProdLocRecordSet = this.PivotalDataAccess.GetRecordset(OppProductLocationData.QueryOppProdLocationsforOppProdId
                    , 1, oppProductId, OppProductLocationData.LocationQuantityField);
            if (oppProdLocRecordSet.RecordCount > 0)
            {
                int optQuantity=0;
                oppProdLocRecordSet.MoveFirst();
                while (!oppProdLocRecordSet.EOF)
                {
                    optQuantity = optQuantity+TypeConvert.ToInt32(oppProdLocRecordSet.Fields[OppProductLocationData.LocationQuantityField].Value);
                    oppProdLocRecordSet.MoveNext();
                }
                EditTableRecords(oppProductId, OpportunityProductData.TableName, OpportunityProductData.QuantityField, optQuantity);
            }

        }


        /// <summary>
        ///     For Whole House option, this will set selected as 0;
        ///     For option with room info, this will delete Opp_Product_Location record and if no Opp_Product_Location records left 
        ///         for the option then set selected as 0 for Opportunity__Product table record. 
        /// </summary>
        /// <param name="oppProductId">oppProductId</param>
        /// <param name="transaction">transaction</param>
        protected virtual void DeleteOppProdLocAndOppProd(object oppProductId, TransactionType transaction)
        {
            if (transaction.RoomNumber==null)
            {//this is option selection removal for whole house
             //set selected=0 in Opportunity_Product Record
                EditTableRecords(oppProductId, OpportunityProductData.TableName 
                    ,OpportunityProductData.SelectedField, 0
                    , OpportunityProductData.EnvOptionSelectedDatetimeField, transaction.DateCreated
                    );
            }
            else
            {//this is option selection removal with room info
                if (transaction.Option.OptionType == SelectedOptionTypeOptionType.Package)
                    DeletePackageOppProdLocAndOppProd(oppProductId, transaction);

                object locationId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(transaction.RoomNumber));
                this.PivotalDataAccess.DeleteRecordset(OppProductLocationData.QueryOppProdLocationsforOppProdAndLocation,
                                            OppProductLocationData.OppProductLocationIdField,
                                            oppProductId,
                                            locationId
                                   );

                Recordset oppProdLocRecordSet = this.PivotalDataAccess.GetRecordset(OppProductLocationData.QueryOppProdLocationsforOppProdId
                        , 1, oppProductId, OppProductLocationData.OppProductLocationIdField);
                if (oppProdLocRecordSet.RecordCount == 0)
                {
                    //no record left in Opp_Prod_Loc for the oppProd, set selected=0 in oppProd Record
                    EditTableRecords(oppProductId, OpportunityProductData.TableName
                        , OpportunityProductData.SelectedField, 0
                        , OpportunityProductData.EnvOptionSelectedDatetimeField, transaction.DateCreated
                        );
                }
                else
                {
                    RecalculateQuantityForOppProd(oppProductId);
                }
            }
        }

        /// <summary>
        ///     oppProductId is a package option
        ///     For option with room info, this will delete Opp_Product_Location record and if no Opp_Product_Location records left 
        ///         for the option then set selected as 0 for Opportunity__Product table record. 
        /// </summary>
        /// <param name="oppProductId">oppProductId</param>
        /// <param name="transaction">transaction</param>
        protected virtual void DeletePackageOppProdLocAndOppProd(object oppProductId, TransactionType transaction)
        {
            Recordset componentOppProdRecordset = this.PivotalDataAccess.GetRecordset(OpportunityProductData.OpportunityProductsForParentPacakgeQueryName
                , 1, oppProductId
                , OpportunityProductData.OpportunityProductIdField
                , OpportunityProductData.QuantityField
                , OpportunityProductData.SelectedField
                , OpportunityProductData.EnvOptionSelectedDatetimeField
                );

            if (componentOppProdRecordset.RecordCount > 0)
            {
                componentOppProdRecordset.MoveFirst();
                while (!componentOppProdRecordset.EOF)
                {
                    object componentOppProdId = componentOppProdRecordset.Fields[OpportunityProductData.OpportunityProductIdField].Value;
                    if (transaction.RoomNumber == null)
                    {//this is option selection removal for whole house
                        //set selected=0 in Opportunity_Product Record
                        componentOppProdRecordset.Fields[OpportunityProductData.SelectedField].Value = 0;
                        componentOppProdRecordset.Fields[OpportunityProductData.EnvOptionSelectedDatetimeField].Value = transaction.DateCreated;
                    }
                    else
                    {//this is option selection removal with room info
                        this.PivotalDataAccess.DeleteRecordset(OppProductLocationData.QueryOppProdLocationsforOppProdAndLocation,
                                                                        OppProductLocationData.OppProductLocationIdField,
                                                                        componentOppProdId,
                                                                        (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(transaction.RoomNumber))
                                                               );
                        Recordset oppProdLocRecordSet = this.PivotalDataAccess.GetRecordset(OppProductLocationData.QueryOppProdLocationsforOppProdId
                                , 1, componentOppProdId, OppProductLocationData.OppProductLocationIdField);
                        if (oppProdLocRecordSet.RecordCount == 0)
                        {
                            //no record left in Opp_Prod_Loc for the componentOppProdId, set selected=0 in componentOppProdId Record
                            componentOppProdRecordset.Fields[OpportunityProductData.SelectedField].Value = 0;
                            componentOppProdRecordset.Fields[OpportunityProductData.EnvOptionSelectedDatetimeField].Value = transaction.DateCreated;
                        }
                        else
                        {
                            oppProdLocRecordSet.Close();
                            //RecalculateQuantityForOppProd(componentOppProdId);
                            oppProdLocRecordSet = this.PivotalDataAccess.GetRecordset(OppProductLocationData.QueryOppProdLocationsforOppProdId
                                    , 1, componentOppProdId, OppProductLocationData.LocationQuantityField);
                            int optQuantity = 0;
                            if (oppProdLocRecordSet.RecordCount > 0)
                            {
                                oppProdLocRecordSet.MoveFirst();
                                while (!oppProdLocRecordSet.EOF)
                                {
                                    optQuantity = optQuantity + TypeConvert.ToInt32(oppProdLocRecordSet.Fields[OppProductLocationData.LocationQuantityField].Value);
                                    oppProdLocRecordSet.MoveNext();
                                }
                            }
                            componentOppProdRecordset.Fields[OpportunityProductData.QuantityField].Value = optQuantity;
                            componentOppProdRecordset.Fields[OpportunityProductData.EnvOptionSelectedDatetimeField].Value = transaction.DateCreated;
                        }
                    }
                    componentOppProdRecordset.MoveNext();
                }
                this.PivotalDataAccess.SaveRecordset(OpportunityProductData.TableName, componentOppProdRecordset);
            }
        }


        
        /// <summary>
        ///     set manufacturer product data for group options including the component options of type group within a pacake option
        /// </summary>
        /// <param name="oppProductId">oppProductId</param>
        /// <param name="oppProductLocId">oppProductLocId</param>
        /// <param name="transaction">transaction</param>
        protected virtual void SetManufacturerProductData(object oppProductId, object oppProductLocId, TransactionType transaction)
        {

            switch (transaction.Option.OptionType)
            {
                case SelectedOptionTypeOptionType.Group:
                    SelectedOptionTypeProduct[] products = transaction.Option.Products;
                    if (products != null)
                        EditTableRecords(oppProductLocId, OppProductLocationData.TableName
                            , OppProductLocationData.EnvDUNSNumberField, products[0].DUNSNumber
                            , OppProductLocationData.EnvGTINField, products[0].GTIN
                            , OppProductLocationData.EnvNHTManufacturerNumberField, products[0].NHTManufacturerNumber
                            , OppProductLocationData.EnvProductBrandField, products[0].ProductBrand
                            , OppProductLocationData.EnvProductNumberField, products[0].ProductNumber
                            , OppProductLocationData.EnvUCCCodeField, products[0].UCCCode
                            , MI_OppProductLocationData.EnvModelNumberField, products[0].ModelNumber
                            , MI_OppProductLocationData.EnvNameField, products[0].Name
                            , MI_OppProductLocationData.EnvSKUField, products[0].SKU
                            , MI_OppProductLocationData.EnvStyleKeyField, products[0].StyleKey
                            , MI_OppProductLocationData.EnvStyleNameField, products[0].StyleName
                            );
                    break;
                case SelectedOptionTypeOptionType.Package:
                    if (transaction.Option.Option != null)
                    {
                        for (int i = 0; i < transaction.Option.Option.Length; ++i)
                        {
                            if (transaction.Option.Option[i].OptionType == SelectedOptionTypeOptionType.Group)
                            {
                                object componentProductID = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(transaction.Option.Option[i].OptionNumber));
                                object locationId;
                                if (transaction.RoomNumber == null)
                                    locationId = null;
                                else
                                    locationId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(transaction.RoomNumber));
                                Recordset oppProdLocRecordSet = this.PivotalDataAccess.GetRecordset(OppProductLocationData.QueryOppProdLocationOfComponentForLocAndParentOppProdAndProd
                                                    , 3, locationId , oppProductId, componentProductID
                                                    , OppProductLocationData.OppProductLocationIdField
                                                    );

                                object componentOppProdLocId = oppProdLocRecordSet.Fields[OppProductLocationData.OppProductLocationIdField].Value;
                                oppProdLocRecordSet.Close();
                                SelectedOptionTypeProduct[] componentProducts = transaction.Option.Option[i].Products;
                                if (componentProducts != null)
                                    EditTableRecords(componentOppProdLocId, OppProductLocationData.TableName
                                        , OppProductLocationData.EnvDUNSNumberField, componentProducts[0].DUNSNumber
                                        , OppProductLocationData.EnvGTINField, componentProducts[0].GTIN
                                        , OppProductLocationData.EnvNHTManufacturerNumberField, componentProducts[0].NHTManufacturerNumber
                                        , OppProductLocationData.EnvProductBrandField, componentProducts[0].ProductBrand
                                        , OppProductLocationData.EnvProductNumberField, componentProducts[0].ProductNumber
                                        , OppProductLocationData.EnvUCCCodeField, componentProducts[0].UCCCode
                                        , MI_OppProductLocationData.EnvModelNumberField, componentProducts[0].ModelNumber
                                        , MI_OppProductLocationData.EnvNameField, componentProducts[0].Name
                                        , MI_OppProductLocationData.EnvSKUField, componentProducts[0].SKU
                                        , MI_OppProductLocationData.EnvStyleKeyField, componentProducts[0].StyleKey
                                        , MI_OppProductLocationData.EnvStyleNameField, componentProducts[0].StyleName
                                        
                                        );
                                else //reset the manufacturer product data fields
                                    EditTableRecords(componentOppProdLocId, OppProductLocationData.TableName
                                        , OppProductLocationData.EnvDUNSNumberField, null
                                        , OppProductLocationData.EnvGTINField, null
                                        , OppProductLocationData.EnvNHTManufacturerNumberField, null
                                        , OppProductLocationData.EnvProductBrandField, null
                                        , OppProductLocationData.EnvProductNumberField, null
                                        , OppProductLocationData.EnvUCCCodeField, null
                                        , MI_OppProductLocationData.EnvModelNumberField, null
                                        , MI_OppProductLocationData.EnvNameField, null
                                        , MI_OppProductLocationData.EnvSKUField, null
                                        , MI_OppProductLocationData.EnvStyleKeyField, null
                                        , MI_OppProductLocationData.EnvStyleNameField, null
                                        );
                            }
                        }
                    }
                    break;
            }

        }


        #region Custom Irvine Company - Option Selection Methods

        #region Processing Methods

        /// <summary>
        /// AM2010.08.26 - This method replaces the ProcessBuyerSelectionsQueue in the 
        /// Execute Case : PROCESS_BUYER_SELECTIONS_QUEUE
        /// This method will extract the options loaded from Chateau to Contracts
        /// and Inventory Quotes in Pivotal. This method will take the place of the
        /// ProcessBuyerSelectionsQueue in the OOTB Envision Code
        /// </summary>
        private void ProcessOptionSelectionsQueue()
        {
            //Write start of process to event viewer
            Log.WriteEvent("Processing Option Selection Queue");

            // this method must be in a transaction as it updates the database             
            if (!System.EnterpriseServices.ContextUtil.IsInTransaction)
                throw new PivotalApplicationException((string)this.LangDictionary.GetText("ExceptionMethodRequiresTransaction"));
            
            //Get list of TIC_INT_OPTION_SELECTIONS fields to work with
            object[] arrFieldNames = GetTICIntOptionSelectionFields();

            // get all records records from queue
            Recordset optionSelectionsRecordset = this.PivotalDataAccess.GetRecordset(TICEnvisionConstants.TICIntOptionSelectionsTable.Queries.OPTION_SELECTIONS_READY_FOR_SYNC,
                0, arrFieldNames);


            if (optionSelectionsRecordset.RecordCount > 0)
            {
                optionSelectionsRecordset.Sort = TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TRANSACTION_DATE;
                optionSelectionsRecordset.MoveFirst();
                while (!optionSelectionsRecordset.EOF)
                {
                    //Apply Buyer Selections the same way Envision does
                    string optionSelectionsProcessResult = ApplyOptionSelections(optionSelectionsRecordset);
                    //the DBNull.Value does not work here, have to use null instead.
                    if (optionSelectionsProcessResult == string.Empty)
                    {
                        optionSelectionsRecordset
                            .Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.STATUS].Value
                                = TICEnvisionConstants.TICIntOptionSelectionsTable.Statuses.SUCCESS;
                    }
                    else
                    {
                        //Set failure reason and set status of record to failed.
                        optionSelectionsRecordset.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.PROCESS_FAILURE_REASON].Value
                            = optionSelectionsProcessResult;
                        optionSelectionsRecordset.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.STATUS].Value
                            = TICEnvisionConstants.TICIntOptionSelectionsTable.Statuses.FAILED;
                    }
                    optionSelectionsRecordset.MoveNext();
                }

                //Now that all records have been processed we need to accept changes to post sale/post build quotes
                AcceptChangesToPostSaleAndPostBuildQuotes();

                //Save recordset once processed.
                this.PivotalDataAccess.SaveRecordset(TICEnvisionConstants.TICIntOptionSelectionsTable.TABLE_NAME, optionSelectionsRecordset);

                //Delete all records that were sucessfull
                this.PivotalDataAccess.DeleteRecordset(TICEnvisionConstants.TICIntOptionSelectionsTable.Queries.OPTION_SELECTIONS_SUCCESS,
                    TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TIC_INT_OPTION_SELECTIONS_ID);

            }


        }

        /// <summary>
        /// AM2010.08.26 - This method replaces the ApplyBuyerSelections method 
        /// from the OOTB Envision code
        /// This method will apply buyer selections one by one
        /// </summary>
        /// <param name="optionSelection"></param>
        /// <returns></returns>
        private string ApplyOptionSelections(Recordset optionSelection)
        {
            try
            {
                // performance counter for elaps time
                DateTime procStart = DateTime.Now;
                Log.WriteEvent((string)this.LangDictionary.GetText("GetBSFromQueue"));

                // process the buyer selections
                string transactionIdList = ProcessOptionSelection(optionSelection);

                //process buyer selection ends.
                Log.WriteEvent(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("ProcessingBSEnds", new string[] { transactionIdList })));

                // log elaps performance
                TimeSpan elaps = DateTime.Now.Subtract(procStart);
                Log.WritePerformance(string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("UpdateBSTimeUsed", new string[] { transactionIdList, elaps.TotalSeconds.ToString() })));

                return "";
            }
            catch (PivotalApplicationException ex)
            {
                // roll back the transaction on failure
                System.EnterpriseServices.ContextUtil.SetAbort();
                //the inner exception has more detailed info.
                Log.WriteException(ex.InnerException);
                return ex.InnerException.Message;
            }

            catch (Exception ex)
            {
                Log.WriteException(ex);
                // roll back the transaction on failure
                System.EnterpriseServices.ContextUtil.SetAbort();
                return ex.Message;
            }
        }


        /// <summary>
        /// AM2010.08.26 - This code replaces the ProcessBuyerSelection method 
        /// from teh original Envision Code.  This method contains all the 
        /// Irvine Specific integration logic.
        /// This method will process the Option selection into Pivotal
        /// to.  Where applicable the OOTB Envision code will be called, else
        /// this code will be customized to meet the requirements for the Irvine Company.
        /// </summary>
        /// <param name="rstOptionSelection"></param>
        /// <returns></returns>
        private string ProcessOptionSelection(Recordset rstOptionSelection)
        {
            object contractId = null;
            string strErrorMsg = string.Empty;

            try
            {
                //Integration Utility to find Lot
                TIC_Envision_Utility util = new TIC_Envision_Utility();

                //Create a new Transaction Type record and mapp it to incoming
                //Chateau options so that we can re-use envision code.
                TransactionType transType = new TransactionType();
                SelectedOptionType opt = new SelectedOptionType();
                opt.OptionName = TypeConvert.ToString(rstOptionSelection.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.PRODUCT_NAME].Value);
                opt.OptionNumber = TypeConvert.ToString(rstOptionSelection.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.OPTION_NUMBER].Value);
                opt.OptionDescription = TypeConvert.ToString(rstOptionSelection.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.PRODUCT_NAME].Value);
                transType.Option = opt;
                transType.Quantity = TypeConvert.ToInt32(rstOptionSelection.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TOTAL_QUANTITY].Value);
                transType.Price = TypeConvert.ToDecimal(rstOptionSelection.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.UNIT_PRICE].Value);                
                TransactionTypeCategory category = new TransactionTypeCategory();
                category.Name = TypeConvert.ToString(rstOptionSelection.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.CATEGORY_DESC].Value);
                category.Number = TypeConvert.ToString(rstOptionSelection.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.CATEGORY_CODE].Value);
                transType.Category = category;
                transType.DateCreated = TypeConvert.ToDateTime(rstOptionSelection.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TRANSACTION_DATE].Value);
                bool isPrePlot = TypeConvert.ToBoolean(rstOptionSelection.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.PREPLOT_OPTION].Value);
                
                //Map notes to TransType object
                MapNotesToTransactionType(ref transType, rstOptionSelection);

                //Set up Post-Sale QuoteSet, Division Product, Product Name and Product Code
                object vntPostSaleQuoteId = null;
                object divisionProductId = null;
                object nbhdProductId = null;
                string productName = null;
                string productNumber = null;

                //Check to see if option type = "Custom" or "Desiger"
                string optionType = TypeConvert.ToString(rstOptionSelection
                    .Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TYPE].Value);

                //If Customer Option, No Division_Product
                if (optionType.ToUpper().Trim() == TICEnvisionConstants.TICIntOptionSelectionsTable.Constants.CUSTOM_OPTION_TYPE)
                {
                    //Set TransactionType object for Custom Options
                    transType.Option.OptionType = SelectedOptionTypeOptionType.Custom;                    
                    //Set Product Number to incoming Product Number from Chateau and Division_Product = null
                    divisionProductId = null;
                    productNumber = TypeConvert.ToString(rstOptionSelection
                        .Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.OPTION_NUMBER].Value);
                    productName = TypeConvert.ToString(rstOptionSelection
                        .Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.PRODUCT_NAME].Value);
                }
                else //If not Customer, then assume this option is designer (Note: Chateau should never send Structural options)
                {                    
                    object neighborhoodId = util.FindNeighborhood(m_rdaSystem, TypeConvert.ToString(rstOptionSelection.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.NEIGHBORHOOD].Value));
                    object regionId = GetRegionIdFromNeighborhood(neighborhoodId);
                    string optionNumber = CalculateNBHDProductExternalSourceId(rstOptionSelection, regionId, false);

                    //Get NBHDP_Product for incoming option
                    nbhdProductId = this.GetNeighborhoodProduct(TypeConvert.ToString(rstOptionSelection
                        .Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields
                        .NEIGHBORHOOD].Value), optionNumber, out divisionProductId);

                    if (Convert.IsDBNull(nbhdProductId))
                    {

                        //AM2010.09.07 - Need to force a lookup again on wildcarded option to ensure that we find the 
                        //associated wildcarded option configuration
                        string wcOptionNumber = CalculateNBHDProductExternalSourceId(rstOptionSelection, regionId, true);
                        nbhdProductId = this.GetNeighborhoodProduct(TypeConvert.ToString(rstOptionSelection
                            .Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields
                            .NEIGHBORHOOD].Value), wcOptionNumber, out divisionProductId);

                        //If still not found throw exception
                        if (Convert.IsDBNull(nbhdProductId))
                        {
                            //Throw exception that we couldn't find the Option we in the Sysetm
                            //by this unique Id
                            strErrorMsg = "Option Rejected - No Option record found for the supplied Option Number : " + optionNumber.ToString();
                            throw new PivotalApplicationException(strErrorMsg);
                        }
                    }

                }

                //Get Unique Lot Identifier
                string uniqueLotNumber = this.CalculateBusinessUnitLotNumber(rstOptionSelection);

                //Find the Contract or Active IQ for the Lot in question
                object vntLotId = util.FindLot(m_rdaSystem, uniqueLotNumber);

                if (Convert.IsDBNull(vntLotId))
                {
                    //Throw exception that we couldn't find the lot we in the Sysetm
                    //by this unique Id
                    strErrorMsg = "Option Rejected - No Lot record found for the supplied Neighborhood & Lot & Unit & Tract lookup values";
                    throw new PivotalApplicationException(strErrorMsg);
                }
                else
                {
                    //Now check to see if Lot is associated to an Active IQ or a Contract
                    contractId = util.FindInventoryQuoteByLot(m_rdaSystem, vntLotId);
                    //If IQ is found need to write code to handle Adding/Editing options for
                    //a Post Build Quote (The same way as the Envision Code is doing it.
                    if (!Convert.IsDBNull(contractId))
                    {
                        //Check for existing non-Chateau Post Build Quotes
                        if (CheckForExistingNonChateauPostSaleOrPostBuildQuote(OpportunityType.InventoryQuote, contractId))
                        {
                            strErrorMsg = "Option Rejected - An existing In Progress Post Build Quote which was not created by Chateau"
                                + " exists for this Inventory Quote.  Cannot proceed";
                            throw new PivotalApplicationException(strErrorMsg);
                        }

                        //Perform same logic that Envision Code is doing for PSQ, but create PBQ instead
                        //This method will find existing Post Sale Quote, or
                        //create one if one doesn't exist (This code calls OOTB Envision Code)
                        GetPostSaleQuote(contractId, out vntPostSaleQuoteId, OpportunityType.InventoryQuote);

                        string validateBuyerSelectionReturn;
                        string optionAvailableTo = string.Empty;
                        //validateBuyerSelectionReturn = ValidateBuyerSelection(transaction, postSaleQuoteId, divisionProductId, out nbhdp_ProductId, out optionAvailableTo);
                        validateBuyerSelectionReturn = ValidateOptionSelection(vntPostSaleQuoteId, divisionProductId, out optionAvailableTo, out nbhdProductId);

                        if (validateBuyerSelectionReturn.Length == 0)
                        {

                            //AM2010.08.24 - All options send to Pivotal will be either a Add/Remove
                            //Need to check for an option with a Zero Quantity (will indicate a remove).
                            int intQuantity = TypeConvert.ToInt32(rstOptionSelection
                                    .Fields[TICEnvisionConstants
                                    .TICIntOptionSelectionsTable.TableFields.TOTAL_QUANTITY].Value);

                            if (intQuantity > 0)
                            {
                                AddEditOptionToPostSaleQuote(vntPostSaleQuoteId, divisionProductId, productName, productNumber, nbhdProductId, optionAvailableTo, transType, isPrePlot);
                            }
                            else
                            {
                                DeleteOptionOnPostSaleQuote(vntPostSaleQuoteId, divisionProductId, productName, productNumber, transType);
                            }
                        }
                        else
                        {
                            throw new PivotalApplicationException(string.Format(CultureInfo.CurrentCulture, validateBuyerSelectionReturn));
                        }
                                                               
                    }
                    else
                    {
                        //No IQ was found so check for existing Contract
                        contractId = util.FindContractByLot(m_rdaSystem, vntLotId);
                        //If Contract is found, call Envision code that 
                        //checks for the Post Sale Quote
                        if (!Convert.IsDBNull(contractId))
                        {
                            //Check for existing non-Chateau Post Sale Quotes
                            if (CheckForExistingNonChateauPostSaleOrPostBuildQuote(OpportunityType.Contract, contractId))
                            {
                                strErrorMsg = "Option Rejected - An existing In Progress Post Sale Quote which was not created by Chateau"
                                    + " exists for this Contract.  Cannot proceed";
                                throw new PivotalApplicationException(strErrorMsg);

                            }

                            //This method will find existing Post Sale Quote, or
                            //create one if one doesn't exist (This code calls OOTB Envision Code)
                            GetPostSaleQuote(contractId, out vntPostSaleQuoteId, OpportunityType.Contract);

                            string validateBuyerSelectionReturn;
                            string optionAvailableTo = string.Empty;
                            //validateBuyerSelectionReturn = ValidateBuyerSelection(transaction, postSaleQuoteId, divisionProductId, out nbhdp_ProductId, out optionAvailableTo);
                            validateBuyerSelectionReturn = ValidateOptionSelection(vntPostSaleQuoteId, divisionProductId, out optionAvailableTo, out nbhdProductId);
                            
                            if (validateBuyerSelectionReturn.Length == 0)
                            {

                                //AM2010.08.24 - All options send to Pivotal will be either a Add/Remove
                                //Need to check for an option with a Zero Quantity (will indicate a remove).
                                int intQuantity = TypeConvert.ToInt32(rstOptionSelection
                                        .Fields[TICEnvisionConstants
                                        .TICIntOptionSelectionsTable.TableFields.TOTAL_QUANTITY].Value);
                                                               
                                if(intQuantity > 0)
                                {
                                    AddEditOptionToPostSaleQuote(vntPostSaleQuoteId, divisionProductId, productName, productNumber, nbhdProductId, optionAvailableTo, transType, isPrePlot);
                                }
                                else
                                {
                                    DeleteOptionOnPostSaleQuote(vntPostSaleQuoteId, divisionProductId, productName, productNumber, transType);
                                }

                                //For Contracts Calculate Totals now
                                // need to calculate total for the PSQ
                                TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                                transitParams.Construct();
                                transitParams.SetUserDefinedParameter(1, vntPostSaleQuoteId);
                                transitParams.SetUserDefinedParameter(2, false);

                                object parameterList = transitParams.ParameterList;

                                m_rdaSystem.Forms[FormName.HBOpportunityOptions].Execute(OpportunityAsrMethodName.CalculateTotals, ref parameterList);
                                
                            }
                            else
                            {
                                throw new PivotalApplicationException(string.Format(CultureInfo.CurrentCulture, validateBuyerSelectionReturn));
                            }
                                          
                        }
                        else
                        {
                            //Throw exception that we couldn't find the lot we in the Sysetm
                            //by this unique Id
                            strErrorMsg = "Option Rejected - No Contract or Active IQ record found for the Lot : (Neighborhood & Lot & Unit & Tract lookup values)";
                            throw new PivotalApplicationException(strErrorMsg);
                        }
                    }
                   
                    //TO-DO : AM2010.08.26 - Envision notification is not needed, however may need to send Chateau some type of confirmation
                    //SendBuyerSelectionUpdateConfirmation(contractId, transactions);
                          
                    
                }


            }
            catch (Exception ex)
            {
                // roll back the transaction on failure
                System.EnterpriseServices.ContextUtil.SetAbort();
                throw CreateBuyerSelectionContractProcessingException((byte[])contractId, ex);
            }

            return string.Empty; //transactionList.Substring(0, transactionList.Length - 1);



        }


        /// <summary>
        ///  Check if the BuyerSelection transaction is valid,
        ///       rule1:
        ///             the referencing NBHDP_Product record should be active
        ///       
        ///  NOTE: This method was copied from the EnvisionIntegration code and modified
        ///  to work for TIC.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="postSaleQuoteId"></param>
        /// <param name="divisionProductId"></param>
        /// <param name="nbhdp_ProductId"></param>
        /// <param name="optionAvailableTo"></param>
        /// <returns></returns>
        private string ValidateOptionSelection(object postSaleQuoteId, object divisionProductId, out string optionAvailableTo, out object nbhdp_ProductId)
        {
            //AM2010.08.24 - Only validation that needs to occur is for Decorator options to 
            //ensure that NBHDP_Product is Active.
            //Envision code disabled for Room validation since all decorator options are "Whole House"

            if (divisionProductId == null)
            {
                nbhdp_ProductId = null;
                optionAvailableTo = "";
                return "";
            }
            else
            {
                nbhdp_ProductId = GetMostSpecificNbhdpProductFromContractAndDivisionProduct(postSaleQuoteId, divisionProductId);
                optionAvailableTo = TypeConvert.ToString(m_rdaSystem.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.OptionAvailableToField].Index(nbhdp_ProductId));

                bool nbhdp_Product_Inactive = TypeConvert.ToBoolean(m_rdaSystem.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.InactiveField].Index(nbhdp_ProductId));
                if (nbhdp_Product_Inactive)
                {
                    return (string.Format(CultureInfo.CurrentCulture, (string)this.LangDictionary.GetTextSub("InvalidSelectedOptionReferencingInactiveProductConfig", new string[] { "", BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(nbhdp_ProductId)) })));
                }
                else
                {
                    return "";
                }
            }

        }


        #endregion

        #region Utility Methods

        /// <summary>
        /// AM2010.08.26 - Utility Method added
        /// Set Business_Unit_Lot_Number field based on various input values - 
        /// return string value to calling function
        /// </summary>
        /// <param name="rstPrimary"></param>
        private string CalculateBusinessUnitLotNumber(Recordset rstPrimary)
        {
            try
            {
                // Get Nbhd Code, Lot Number, Unit & Tract into local string-converted variables
                string strNeighborhoodCode = TypeConvert.ToString(rstPrimary.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.NEIGHBORHOOD].Value).Trim();
                string strLotNumber = TypeConvert.ToString(rstPrimary.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.LOT_NUMBER].Value).Trim();
                string strUnit = TypeConvert.ToString(rstPrimary.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.UNIT].Value).Trim();
                string strTract = TypeConvert.ToString(rstPrimary.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TRACT].Value).Trim();

                // Set Business Unit Lot Number to [Neighborhood Code]-[Lot Number]-[Unit]-[Tract]
                // If a given value is an empty string, it won't matter, we'll just end up with "--" instead of "-123-"
                string strBusinessUnitLotNumber = TypeConvert.ToString(strNeighborhoodCode + "-" +
                                                  strLotNumber + "-" +
                                                  strUnit + "-" +
                                                  strTract).Trim();

                return strBusinessUnitLotNumber;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, m_rdaSystem);
            }
        }


        /// <summary>
        /// AM2010.08.26 - Utility Method added
        /// This method will return the Division Product record for the option/neighborhood
        /// combination passed in.
        /// </summary>
        /// <param name="neighborhoodCode"></param>
        /// <param name="optionNumber"></param>
        /// <returns></returns>
        private object GetNeighborhoodProduct(string neighborhoodCode, string optionNumber, out object divisionProductId)
        {
            string strErrMsg = string.Empty;
            Recordset rst = null;
            object nbhdProductId = null;

            //Use this object to get new recordset
            DataAccess objLib = (DataAccess)
               m_rdaSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

            StringBuilder sqlText = new StringBuilder();

            //Get Customer SQL
            sqlText.Append("SELECT NBHDP_PRODUCT_ID, DIVISION_PRODUCT_ID FROM ");
            sqlText.Append("NBHDP_PRODUCT NP INNER JOIN NEIGHBORHOOD N ");
            sqlText.Append("ON NP.NEIGHBORHOOD_ID = N.NEIGHBORHOOD_ID ");
            sqlText.Append("WHERE N.EXTERNAL_SOURCE_COMMUNITY_ID = '" + neighborhoodCode + "'");
            sqlText.Append("AND NP.EXTERNAL_SOURCE_ID = '" + optionNumber + "'");

            rst = objLib.GetRecordset(sqlText.ToString());

            if (rst.RecordCount > 0)
            {
                rst.MoveFirst();
                nbhdProductId = rst.Fields[0].Value;
                divisionProductId = rst.Fields[1].Value;
            }
            else
            {
                nbhdProductId = DBNull.Value;
                divisionProductId = DBNull.Value;
            }

            //Clean Up
            rst.Close();

            return nbhdProductId;
        }


        /// <summary>
        /// AM2010.08.26 - Utility Method added
        /// External_Source_Id = Chateau.Region + "-" + Chateau.Option_Number
        /// </summary>
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        protected virtual string CalculateNBHDProductExternalSourceId(Recordset rstPrimary, object vntRegionId, bool blnForceWC)
        {
            try
            {
                // Lookup the Region.External_Source_Id from the Region_Id determined for the current Product record
                string strRegionCode = this.GetRegionExternalSourceId(vntRegionId);

                // Get other fields directly from the Product record
                string strNeighborhoodCode = TypeConvert.ToString(rstPrimary.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.NEIGHBORHOOD].Value).Trim();
                string strPhaseCode = TypeConvert.ToString(rstPrimary.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.PHASE].Value).Trim();
                string strPlanCode = TypeConvert.ToString(rstPrimary.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.PLAN_CODE].Value).Trim();
                string strOptionNumber = TypeConvert.ToString(rstPrimary.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.OPTION_NUMBER].Value).Trim();

                //AM2010.08.18 - Logic here was backwards, so simply fixed to 
                //Set "*" when strPhaseCode IS Null or Empty
                // Change strPhaseCode to '*' if it is null/empty
                if (String.IsNullOrEmpty(strPhaseCode) || blnForceWC)
                {
                    strPhaseCode = "*";
                }

                string strExternalSourceId = TypeConvert.ToString(strRegionCode + "-" +
                                             strNeighborhoodCode + "-" +
                                             strPhaseCode + "-" +
                                             strPlanCode + "-" +
                                             strOptionNumber).Trim();

                return strExternalSourceId;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, m_rdaSystem);
            }
        }

        /// <summary>
        /// AM2010.08.26 - Utility Method added
        /// Return Neighborhood.Region_Id of Neighborhood record with Record Id = vntNeighborhoodId
        /// </summary>
        /// <param name="vntNeighborhoodId"></param>
        /// <returns></returns>
        protected virtual object GetRegionIdFromNeighborhood(object vntNeighborhoodId)
        {
            try
            {
                object vntResult = DBNull.Value;

                if (!(Convert.IsDBNull(vntNeighborhoodId)))
                {
                    vntResult = m_rdaSystem.Tables[TICEnvisionConstants.strtNEIGHBORHOOD].Fields[TICEnvisionConstants.strfREGION_ID].FindValue(
                        m_rdaSystem.Tables[TICEnvisionConstants.strtNEIGHBORHOOD].Fields[TICEnvisionConstants.strfNEIGHBORHOOD_ID],
                        vntNeighborhoodId);

                    //If nothing is returned make sure you return a NULL database value
                    if (vntResult == null)
                    {
                        vntResult = DBNull.Value;
                    }
                }

                // Return value
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, m_rdaSystem);
            }
        }


        /// <summary>
        /// AM2010.08.26 - Utility Method added
        /// Return Region.External_Source_Id of Region record with Record Id = vntRegionId
        /// </summary>
        /// <param name="vntNeighborhoodId"></param>
        /// <returns></returns>
        protected virtual string GetRegionExternalSourceId(object vntRegionId)
        {
            try
            {
                string strResult = String.Empty;

                if (!(Convert.IsDBNull(vntRegionId)))
                {
                    strResult = TypeConvert.ToString(m_rdaSystem.Tables[TICEnvisionConstants.strtREGION].Fields[TICEnvisionConstants.strfEXTERNAL_SOURCE_ID].FindValue(
                        m_rdaSystem.Tables[TICEnvisionConstants.strtREGION].Fields[TICEnvisionConstants.strfREGION_ID],
                        vntRegionId)).Trim();
                }

                // Return value
                return strResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, m_rdaSystem);
            }
        }
        
        /// <summary>
        /// AM2010.08.26 - Utility Method added
        /// This method will use the notes in the incoming recordset to map to a TransactionType
        /// for processing
        /// </summary>
        /// <param name="transType"></param>
        private void MapNotesToTransactionType(ref TransactionType transType, Recordset rstPrimary)
        {
            NoteType[] notes = new NoteType[4];
            string locationNotes = TypeConvert.ToString(rstPrimary.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.LOCATION_NOTE].Value);
            string styleNotes = TypeConvert.ToString(rstPrimary.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.STYLE_NOTES].Value);
            string colorNotes = TypeConvert.ToString(rstPrimary.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.COLOR_NOTES].Value);
            string otherNotes = TypeConvert.ToString(rstPrimary.Fields[TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.NOTES].Value);
            NoteType locationNote = new NoteType();
            NoteType styleNote = new NoteType();
            NoteType colorNote = new NoteType();
            NoteType otherNote = new NoteType();

            if (!String.IsNullOrEmpty(locationNotes))
            {                
                locationNote.Text = locationNotes;
                locationNote.Type = NoteTypeType.Location;                
            }
            if (!String.IsNullOrEmpty(styleNotes))
            {                
                styleNote.Text = styleNotes;
                styleNote.Type = NoteTypeType.Style;                
            }
            if (!String.IsNullOrEmpty(colorNotes))
            {                
                colorNote.Text = colorNotes;
                colorNote.Type = NoteTypeType.Color;                
            }
            if (!String.IsNullOrEmpty(otherNotes))
            {                
                otherNote.Text = otherNotes;
                otherNote.Type = NoteTypeType.Other;
                
            }
            //Always assign note records
            notes[0] = locationNote;
            notes[1] = styleNote;
            notes[2] = colorNote;
            notes[3] = otherNote;
            transType.Notes = notes;
                   
        }

        /// <summary>
        /// This method will check the existing contract to determine if an existign Post Sale
        /// Quote or an existing Post Build Quote exists for the IQ/Contract
        /// </summary>
        /// <param name="oppType"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        private bool CheckForExistingNonChateauPostSaleOrPostBuildQuote(OpportunityType oppType, object contractId)
        {
            Recordset opportunityRecordset; 

            if(OpportunityType.Contract == oppType)
            {            
                //trying to find the in progress PBQ for the contract
                opportunityRecordset = this.PivotalDataAccess.GetRecordset(TICEnvisionConstants.TICIntOptionSelectionsTable.Queries.TIC_ACTIVE_NON_CHATEAU_PSQs_FOR_CONTRACT
                    , 1, contractId, OpportunityData.OpportunityIdField);               
            }
            else
            {            
                //trying to find the in progress PBQ for the contract
                opportunityRecordset = this.PivotalDataAccess.GetRecordset(TICEnvisionConstants.TICIntOptionSelectionsTable.Queries.TIC_ACTIVE_NON_CHATEAU_PBQs_FOR_IQ
                    , 1, contractId, OpportunityData.OpportunityIdField);
                //opportunityRecordset = this.PivotalDataAccess.GetRecordset("Env: MI Active PSQ for contract?", 1, contractId, OpportunityData.OpportunityIdField);
            
            }

            //If one or more records exists we need to throw an error
            if(opportunityRecordset.RecordCount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
                    
        }

        /// <summary>
        /// AM2010.08.26 - This method will accept PSQ/PBQ for each 
        /// Post Sale in the List defined at the class level
        /// This method will accept changes for all new Post Sale and Post Build Quotes
        /// after the batch is complete.
        /// </summary>
        private void AcceptChangesToPostSaleAndPostBuildQuotes()
        {

            foreach (object psqId in pstIdList)
            {                
                //the contract currently has no in progress PSQ yet, create one.
                TransitionPointParameter transitionPointParameter = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                object executeParameters = transitionPointParameter.Construct();
                object executeUserParameters = new object[] { (object)psqId };
                executeParameters = transitionPointParameter.SetUserDefinedParameterArray(executeUserParameters);

                m_rdaSystem.Forms[FormName.HBPostSaleQuote].Execute(OpportunityAsrMethodName.ApplyPostSaleQuote, ref executeParameters);
                //transitionPointParameter.GetUserDefinedParameterArray(executeParameters);
                //postSaleQuoteId = (byte[])transitionPointParameter.GetUserDefinedParameter(1);
            }
        
        }

        #endregion

        #region Build Fields Arrays

        /// <summary>
        /// AM2010.08.26 - Build an array list of the fields from the
        /// TIC_Int_Option_Selections table.
        /// </summary>
        /// <returns></returns>
        public object[] GetTICIntOptionSelectionFields()
        {
            object[] arrFields = new object[] 
            {
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.CATEGORY_CODE,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.CATEGORY_DESC,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.COLOR_NOTES,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.COMPLETE_NOTES,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.DEPOSIT_TOTAL,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.LOCATION_NOTE,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.LOT_NUMBER,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.NEIGHBORHOOD,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.NOTES,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.OPTION_NUMBER,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.PHASE,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.PLAN_CODE,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.PROCESS_FAILURE_REASON,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.PRODUCT_NAME,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.STATUS,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.STYLE_NOTES,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TOTAL_PRICE,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TOTAL_QUANTITY,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TRACT,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TRANSACTION_ID,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TYPE,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.UNIT,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.UNIT_COST,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.UNIT_PRICE,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.TRANSACTION_DATE,
                TICEnvisionConstants.TICIntOptionSelectionsTable.TableFields.PREPLOT_OPTION
                            
            };

            return arrFields;

        }

        #endregion

        #endregion




    }
}
