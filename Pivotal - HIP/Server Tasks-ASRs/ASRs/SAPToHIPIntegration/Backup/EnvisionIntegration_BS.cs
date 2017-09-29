//
// $Workfile: EnvisionIntegration_BS.cs$
// $Revision: 87$
// $Author: jwang$
// $Date: Thursday, August 30, 2007 5:31:02 PM$
//
// Copyright © Pivotal Corporation
//

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
    /// The ASR Class for Envision Integration
    /// </summary>
    public partial class EnvisionIntegration : IRAppScript
    {
        /// <summary>
        /// Save the buyer selections to the database for later processing.
        /// </summary>
        /// <param name="xml">Xml containing the new/modified/deleted buyer selections for the system</param>
        /// <returns>Failure or success message</returns>
        /// <remarks>This method is intended to run within a MTS transaction</remarks>
        private string SaveBuyerSelections(string xml)
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
                OrganizationType region = (OrganizationType)corporation.Item;
                OrganizationType division = (OrganizationType)region.Item;
                InventoryType community = (InventoryType)division.Item;
                InventoryType release = (InventoryType)community.Item;
                InventoryType plan = (InventoryType)release.Item;
                InventoryTypeHome home = (InventoryTypeHome)plan.Item;
                TransactionType[] transactions = (TransactionType[])home.Transaction;
                contractId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(home.HomeNumber.Split(':')[0]));
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
        private string ApplyBuyerSelections(string xml)
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
        private void ClearPendingBuyerSelectionsFromQueue()
        {
            Recordset buyerSelectionsRecordset;


            buyerSelectionsRecordset = this.PivotalDataAccess.GetRecordset(EnvBuyerSelectionsData.PendingBuyerSelectionsQuery, 0
                , EnvBuyerSelectionsData.EnvBuyerSelectionsIdField
                , EnvBuyerSelectionsData.TansactionIdField
                );
            if (buyerSelectionsRecordset.RecordCount == 0)
            {
            }
            else
            {
                buyerSelectionsRecordset.MoveFirst();

                Envision.DesignCenterManager.Home.HomeWebService homeWebService = new Envision.DesignCenterManager.Home.HomeWebService(this);
                homeWebService.AuthHeaderValue = new Envision.DesignCenterManager.Home.AuthHeader();
                homeWebService.AuthHeaderValue.UserName = this.Config.EnvisionWebServiceUserName;
                homeWebService.AuthHeaderValue.Password = this.Config.EnvisionWebServicePassword;
                homeWebService.AuthHeaderValue.NHTBillingNumber = this.Config.EnvisionNHTNumber;
                homeWebService.Url = this.Config.EnvisionHomeWebServiceUrl;
                homeWebService.Timeout = this.Config.EnvisionWebServiceTimeout;

                while (!buyerSelectionsRecordset.EOF)
                {
                    byte[] opportunityId = buyerSelectionsRecordset.Fields[EnvBuyerSelectionsData.TansactionIdField].Value == DBNull.Value ? null : (byte[])buyerSelectionsRecordset.Fields[EnvBuyerSelectionsData.TansactionIdField].Value;
                    int transactionId = buyerSelectionsRecordset.Fields[EnvBuyerSelectionsData.TansactionIdField].Value == DBNull.Value ? 0 : (int)buyerSelectionsRecordset.Fields[EnvBuyerSelectionsData.TansactionIdField].Value;

                    try
                    {
                        XmlNode outputXml = homeWebService.UpdateSelectionStatus(TypeConvert.ToInt32(transactionId), "Received");

                        // validate the returned xml agains the Envision schema
                        XmlValidation.Output(outputXml);

                        // turn the returned xml into an Envision Output entity
                        EnvisionXsdGenerated.Output output = GetOutput(outputXml);

                        // if Envision returns an error, turn the error into an Exception and throw.
                        if (output.Status != EnvisionXsdGenerated.OutputStatus.Success) throw CreateSoapException(output);
                    }
                    catch (SoapException ex)
                    {
                        // wrap exception with better description
                        throw new SoapException(ContractExceptionMessage.GetContractOptionsUpdateExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, opportunityId, transactionId, ContractExceptionMessage.ContractOptionSelectionProcessing.ConfirmInventorySelectionsReciept), ex.Code, ex);
                    }
                    catch (Exception ex)
                    {
                        // wrap exception with better description
                        throw new PivotalApplicationException(ContractExceptionMessage.GetContractOptionsUpdateExceptionMsg(this.PivotalSystem, this.PivotalDataAccess, opportunityId, transactionId, ContractExceptionMessage.ContractOptionSelectionProcessing.ConfirmInventorySelectionsReciept), ex);
                    }

                    //buyerSelectionsRecordset.Delete(AffectEnum.adAffectCurrent);
                    buyerSelectionsRecordset.MoveNext();
                }
                this.PivotalDataAccess.DeleteRecordset(EnvBuyerSelectionsData.PendingBuyerSelectionsQuery,
                                            EnvBuyerSelectionsData.EnvBuyerSelectionsIdField);
                System.EnterpriseServices.ContextUtil.SetComplete();
            }
        }


        /// <summary>
        /// Process all Buyer Selection Xml saved to the database 
        /// </summary>
        /// <remarks>This method is intended to run within a MTS transaction</remarks>
        private void ProcessBuyerSelectionsQueue()
        {

            Log.WriteEvent("Processing Buyer Selection Queue");

            // this method must be in a transaction as it updates the database 
            if (!System.EnterpriseServices.ContextUtil.IsInTransaction)
                throw new PivotalApplicationException((string)this.LangDictionary.GetText("ExceptionMethodRequiresTransaction"));


            // get all xml records
            Recordset buyerSelectionsRecordset = this.PivotalDataAccess.GetRecordset(EnvBuyerSelectionsData.AllBuyerSelectionXMLQuery, 0
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
        private string ProcessBuyerSelections(Builder builder)
        {
            byte[] contractId = null;
            string transactionList = "";

            try
            {
                OrganizationType corporation = builder.Organization;
                OrganizationType region = (OrganizationType)corporation.Item;
                OrganizationType division = (OrganizationType)region.Item;
                InventoryType community = (InventoryType)division.Item;
                InventoryType release = (InventoryType)community.Item;
                InventoryType plan = (InventoryType)release.Item;
                InventoryTypeHome home = (InventoryTypeHome)plan.Item;


                TransactionType[] transactions = (TransactionType[])home.Transaction;
                
                if (home.HomeNumber.IndexOf(":") != -1)
                {
                    contractId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(home.HomeNumber.Split(':')[0]));

                    object postSaleQuoteId = new byte[0];
                    GetPostSaleQuote(contractId, out postSaleQuoteId);

                    // process add, edit or delete
                    for (int i = 0; i < transactions.Length; i++)
                    {
                        TransactionType transaction = transactions[i];
                        byte[] divisionProductId;
                        string productName;  //this is used for custom option search criteria
                        string productNumber;  //this is used for custom option search criteria

                        transactionList += transaction.TransactionID.ToString() + ",";

                        if (transaction.Option.OptionType == SelectedOptionTypeOptionType.Custom) //custom option
                        {
                            divisionProductId = null;
                            productName = transaction.Option.OptionName;
                            productNumber = transaction.Option.OptionNumber;
                        }
                        else
                        {
                            divisionProductId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(transaction.Option.OptionNumber));
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
                                    AddEditOptionToPostSaleQuote(postSaleQuoteId, divisionProductId, productName, productNumber, nbhdp_ProductId, optionAvailableTo, transaction);
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

                    // need to calculate total for the PSQ
                    TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                    transitParams.Construct();
                    transitParams.SetUserDefinedParameter(1, postSaleQuoteId);
                    transitParams.SetUserDefinedParameter(2, false);

                    object parameterList = transitParams.ParameterList;

                    m_rdaSystem.Forms[FormName.HBOpportunityOptions].Execute(OpportunityAsrMethodName.CalculateTotals, ref parameterList);            



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
        private PivotalApplicationException CreateBuyerSelectionContractProcessingException(byte[] contractId, Exception innerException)
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
        private void SendBuyerSelectionUpdateConfirmation(byte[] opportunityId, TransactionType[] transactions)
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
        ///     Get post sale quote for the related contract. If the contract has no PSQ yet then create one for it.
        /// </summary>
        /// <param name="contractId">contractId</param>
        /// <param name="postSaleQuoteId">postSaleQuoteId</param>
        private void GetPostSaleQuote(object contractId, out object postSaleQuoteId)
        {

            Recordset opportunityRecordset;

            //trying to find the in progress PSQ for the contract

            opportunityRecordset = this.PivotalDataAccess.GetRecordset(OpportunityData.QueryActivePSQForContract, 1, contractId, OpportunityData.OpportunityIdField);
            if (opportunityRecordset.RecordCount == 0)
            {
                //the contract currently has no in progress PSQ yet, create one.
                TransitionPointParameter transitionPointParameter = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                object executeParameters = transitionPointParameter.Construct();
                object executeUserParameters = new object[] { contractId };
                executeParameters = transitionPointParameter.SetUserDefinedParameterArray(executeUserParameters);

                m_rdaSystem.Forms[FormName.HBPostSaleQuote].Execute(OpportunityAsrMethodName.CreatePostSaleQuote, ref executeParameters);
                transitionPointParameter.GetUserDefinedParameterArray(executeParameters);
                postSaleQuoteId = (byte[])transitionPointParameter.GetUserDefinedParameter(1);

            }
            else
            {
                opportunityRecordset.MoveFirst();
                postSaleQuoteId = (byte[])opportunityRecordset.Fields[OpportunityData.OpportunityIdField].Value;
            }

        }

        /// <summary>
        ///     Add/Edit option in PSQ.
        /// </summary>
        /// <param name="postSaleQuoteId">postSaleQuoteId</param>
        /// <param name="divisionProductId">divisionProductId, if this is custom option the divisionProductId is null</param>
        /// <param name="productName">productName, used for custom option</param>
        /// <param name="productNumber">productNumber, used for custom option</param>
        /// <param name="nbhdp_ProductId">nbhdp_ProductId, the related product configuration of the option. If it is custom option then null.</param>
        /// <param name="optionAvailableTo">optionAvailableTo, the Option_Available_To value of the involved product configuration</param>
        /// <param name="transaction">transaction, used to pass the manufacturer product data. If it is package option then each component will have its own manufacturer product data. </param>
        private void AddEditOptionToPostSaleQuote(object postSaleQuoteId, object divisionProductId, string productName, string productNumber, object nbhdp_ProductId, string optionAvailableTo, TransactionType transaction)
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
                            , (transaction.CategoryGroup == null ? null : transaction.CategoryGroup.Number), (transaction.Category==null? null:transaction.Category.Number), transaction.RoomNumber, transaction.DateCreated);
                    }
                    else
                    {
                        object oppProductId;
                        object oppProductLocId;
                        AddOptionToPostSaleQuote(postSaleQuoteId, nbhdp_ProductId, transaction.Quantity, transaction.DateCreated
                            , out oppProductId, out oppProductLocId);

                        //set the option selection notes.
                        EditTableRecords(oppProductLocId, OppProductLocationData.TableName
                            , OppProductLocationData.NotesField, GetNotes(transaction)
                            );

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
                        if (DateTime.Compare(TypeConvert.ToDateTime(selectedOptionRecordSet.Fields[OpportunityProductData.EnvOptionSelectedDatetimeField].Value), transaction.DateCreated) < 0)
                        {

                            object oppProdId = selectedOptionRecordSet.Fields[OpportunityProductData.OpportunityProductIdField].Value;
                            selectedOptionRecordSet.Close();
                            //Edit the Opp_Product_Location and opportunity__product records
                            EditOppProdLocAndOppProd(oppProdId, transaction);

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
        ///     Add Envision Custom Option to the post sale quote
        ///     optionNumber and Name is defiend by user
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

        private void AddEnvisonCustomOptionToPostSaleQuote(object postSaleQuoteId, string optionName, string optionNumber
            , string optionDescription, int quantity, decimal extendedPrice
            , string categoryGroupNumber, string categoryNumber, string roomNumber, DateTime transactionDatetime
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
            if (categoryGroupNumber != null)
                opportunityProductRecordset.Fields[OpportunityProductData.CategoryIdField].Value = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(categoryGroupNumber));
            if (categoryNumber != null)
                opportunityProductRecordset.Fields[OpportunityProductData.SubCategoryIdField].Value = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(categoryNumber));
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

        private void AddOptionToPostSaleQuote(object postSaleQuoteId, object nbhdp_ProductId, int quantity, DateTime transactionDatetime
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
        private void DeleteOptionOnPostSaleQuote(object postSaleQuoteId, object divisionProductId, string productName, string productNumber, TransactionType transaction)
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


        private byte[] GetMostSpecificNbhdpProductFromContractAndDivisionProduct(object ContractId, object divisionProductId)
        {
            Recordset nbhdpProductRecordset;
            object planId, releaseId, neighborhoodId, divisionId, regionId;
            string planCode;

            planId = m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.PlanNameIdField].Index(ContractId);
            planCode = TypeConvert.ToString(m_rdaSystem.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.PlanCodeField].Index(planId));
            releaseId = m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.NBHDPhaseIdField].Index(ContractId);
            neighborhoodId = m_rdaSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.NeighborhoodIdField].Index(ContractId);
            divisionId = m_rdaSystem.Tables[NeighborhoodData.TableName].Fields[NeighborhoodData.DivisionIdField].Index(neighborhoodId);
            regionId = m_rdaSystem.Tables[DivisionData.TableName].Fields[DivisionData.RegionIdField].Index(divisionId);

            nbhdpProductRecordset = this.PivotalDataAccess.GetRecordset(NBHDPProductData.OptionsAvailableForPlanAndDivProdQueryName
                        , 7, (object)divisionProductId, planId, planCode, regionId, divisionId, neighborhoodId, releaseId
                        , NBHDPProductData.NBHDPProductIdField, NBHDPProductData.WCLevelField
                                                        );

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
        private string ValidateBuyerSelection(TransactionType transaction, object postSaleQuoteId, object divisionProductId, out object nbhdp_ProductId, out string optionAvailableTo)
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
                if (nbhdp_Product_Inactive)
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

        private int EditTableRecords(string queryName, int parameterNumber, params object[] parameterFieldNameValueArray)
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

        private int EditTableRecords(object recordId, string tableName, params object[] parameterFieldNameValueArray)
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

        private object AddTableRecord(string tableName, params object[] parameterFieldNameValueArray)
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

        private void SetLocationIdOfOpp_Product_Location(object oppProductLocId, string roomNumber)
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
        ///     This will edit Opp_Product_Location and Opportunity__Product table records. 
        /// </summary>
        /// <param name="oppProductId">oppProductId</param>
        /// <param name="transaction">transaction</param>
        private void EditOppProdLocAndOppProd(object oppProductId, TransactionType transaction)
        {
            object locationId=null;
            object oppProdLocId;

            if (transaction.Option.OptionType == SelectedOptionTypeOptionType.Custom)
            {
                //GetGeneratedFileList price from DCM
                //Change the Opportunity_Product Record
                if (transaction.RoomNumber != null)
                    locationId = (byte[])m_rdaSystem.StringToId(BuilderBase.UncompactPivotalId(transaction.RoomNumber));

                EditTableRecords(oppProductId, OpportunityProductData.TableName
                    , OpportunityProductData.SelectedField, 1
                    , OpportunityProductData.QuantityField, transaction.Quantity
                    , OpportunityProductData.PriceField, transaction.Price / (decimal)transaction.Quantity
                    , OpportunityProductData.EnvOptionSelectedDatetimeField, transaction.DateCreated
                    , OpportunityProductData.LocationIdField, locationId
                    , OpportunityProductData.OptionNotesField, transaction.Option.OptionDescription
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

                //Change the Opportunity_Product Record
                EditTableRecords(oppProductId, OpportunityProductData.TableName
                    , OpportunityProductData.SelectedField, 1
                    , OpportunityProductData.QuantityField, transaction.Quantity
                    , OpportunityProductData.PriceField, TypeConvert.ToDecimal(transitParams.GetUserDefinedParameter(1))
                    , OpportunityProductData.UsePCOPriceField, TypeConvert.ToBoolean(transitParams.GetUserDefinedParameter(2))
                    //do not set the option Built, olny use Post Cut-off price when homesite's construction stage passed the involved division_product's construction stage.
                    //                , OpportunityProductData.BuiltOptionField, TypeConvert.ToBoolean(transitParams.GetUserDefinedParameter(3))
                    //                , OpportunityProductData.BuiltOptionField, false
                    , OpportunityProductData.EnvOptionSelectedDatetimeField, transaction.DateCreated
                    );
                if (transaction.RoomNumber == null)
                {//this is option selection for whole house
                    //Change the quantity in dummy Opp_Prod_Loc Record
                    EditTableRecords(OppProductLocationData.QueryOppProdLocationsforOppProdId, 1, oppProductId, OppProductLocationData.LocationQuantityField,transaction.Quantity);

                    Recordset oppProdLocRecordset = this.PivotalDataAccess.GetRecordset(OppProductLocationData.QueryOppProdLocationsforOppProdId, 1, oppProductId
                                                    , OppProductLocationData.OppProductLocationIdField);
                    oppProdLocId = oppProdLocRecordset.Fields[OppProductLocationData.OppProductLocationIdField].Value;
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
                                                    , OppProductLocationData.OppProductLocationIdField);
                    oppProdLocId = oppProdLocRecordset.Fields[OppProductLocationData.OppProductLocationIdField].Value;
                    oppProdLocRecordset.Close();

                    RecalculateQuantityForOppProd(oppProductId);
                }

                //set the option selection notes.
                EditTableRecords(oppProdLocId, OppProductLocationData.TableName
                    , OppProductLocationData.NotesField, GetNotes(transaction)
                    );

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
        private string GetNotes(TransactionType transaction)
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
        ///     oppProductId is a package, this will edit Opp_Product_Location 
        ///     and Opportunity__Product table records for the underlying components. 
        /// </summary>
        /// <param name="oppProductId">oppProductId</param>
        /// <param name="transaction">transaction</param>
        private void EditPackageOppProdLocAndOppProd(object oppProductId, TransactionType transaction)
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
        private void RecalculateQuantityForOppProd(object oppProductId)
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
        private void DeleteOppProdLocAndOppProd(object oppProductId, TransactionType transaction)
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
        private void DeletePackageOppProdLocAndOppProd(object oppProductId, TransactionType transaction)
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
        private void SetManufacturerProductData(object oppProductId, object oppProductLocId, TransactionType transaction)
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
                                        );
                                else //reset the manufacturer product data fields
                                    EditTableRecords(componentOppProdLocId, OppProductLocationData.TableName
                                        , OppProductLocationData.EnvDUNSNumberField, null
                                        , OppProductLocationData.EnvGTINField, null
                                        , OppProductLocationData.EnvNHTManufacturerNumberField, null
                                        , OppProductLocationData.EnvProductBrandField, null
                                        , OppProductLocationData.EnvProductNumberField, null
                                        , OppProductLocationData.EnvUCCCodeField, null
                                        );
                            }
                        }
                    }
                    break;
            }

        }

    }
}
