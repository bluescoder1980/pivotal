using System;
using System.Text;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;

using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// This module provides all the business rules for the Opportunity object.
    /// </summary>
    // This object is used to record the pipeline stage and status of the opportunity,
    // create a new activity for this opportunity, create an alert to notify other staff of
    // important information about this opportunity, launch a sales plan, etc.
    // Revision # Date Author Description
    // 3.8.0.0  5/12/2006  DYin  Converted to .Net C# code.
    public class OpportunityPostSaleQuote : IRFormScript
    {
        private ILangDict grldtLangDict;

        /// <summary>
        /// Language Dictionary
        /// </summary>
        protected ILangDict RldtLangDict
        {
            get { return grldtLangDict; }
            set { grldtLangDict = value; }
        }
        
        private IRSystem7 mrsysSystem;

        /// <summary>
        /// System
        /// </summary>
        protected IRSystem7 RSysSystem
        {
            get { return mrsysSystem; }
            set { mrsysSystem = value; }
        }
        
        /// <summary>
        /// Add form data
        /// </summary>
        /// <param name="pForm">The IRForm object reference to the client IRForm object</param>
        /// <param name="Recordsets">Hold the reference for the current primary recordset and its all</param>
        /// secondaries in the specified form
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// IRFormScript_AddFormData - Return information to IRSystem</returns>
        /// <history>
        /// Revision#     Date            Author          Description
        /// 3.8.0.0       5/12/2006       DYin            Converted to .Net C# code.
        /// </history>
        public virtual object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                return pForm.DoAddFormData(Recordsets, ref ParameterList);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Delete Form Data
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="RecordId">The business object record Id</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        public virtual void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                pForm.DoDeleteFormData(RecordId, ref ParameterList);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        // Name : Execute
        /// <summary>
        /// Execute a specified method
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="MethodName">The method name to be executed</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// ParameterList - Return executed result</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {
            try
            {
                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;

                // Dump out the user defined parameters
                object[] parameterArray = ocmsTransitPointParams.GetUserDefinedParameterArray();

                switch (MethodName)
                {
                    case modOpportunity.strmCREATE_POST_SALE_QUOTE:
                        parameterArray = new object[] { CreatePostSaleQuote(parameterArray[0]) };
                        break;
                    case modOpportunity.strmIS_THERE_BUILT_OPTION_CHANGE:
                        parameterArray = new object[] { IsThereBuiltOptionChange(parameterArray[0]) };
                        break;
                    case modOpportunity.strmAPPLY_POST_SALE_QUOTE:
                        ApplyPostSaleQuote(parameterArray[0]);
                        break;
                    case modOpportunity.strmINACTIVATE_OTHER_PSQ:
                        InactivateOtherPostSaleQuotes(parameterArray[0], false);
                        break;
                    case modOpportunity.strmINACTIVATE_ALL_PSQ:
                        InactivateOtherPostSaleQuotes(parameterArray[0], true);
                        break;
                    default:
                        string mmessage = MethodName + TypeConvert.ToString(RldtLangDict.GetText(modOpportunity.strdINVALID_METHOD));
                        parameterArray = new object[] { mmessage };
                        throw new PivotalApplicationException(mmessage, modOpportunity.glngERR_METHOD_NOT_DEFINED);
                }
                // Add the returned values into transit point parameter list
                ParameterList = ocmsTransitPointParams.SetUserDefinedParameterArray(parameterArray);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="RecordId">The Opportunity Id</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>The form data</returns>
        /// <history>
        /// Revision#     Date        Author       Description
        /// 3.8.0.0       5/12/2006   DYin         Converted to .Net C# code.
        /// </history>
        public virtual object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                return pForm.DoLoadFormData(RecordId, ref ParameterList);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function load a new Opportunity Loan Profile record
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// IRFormScript_NewFormData   - Returned information</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        public virtual object NewFormData(IRForm pForm, ref object ParameterList)
        {
            try
            {
                object vntRecordsets = pForm.DoNewFormData(ref ParameterList);
                object[] recordsetArray = (object[]) vntRecordsets;

                Recordset rstOpp = (Recordset) recordsetArray[0];

                // Set Default Fields value
                TransitionPointParameter objParam = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;
                if (objParam.HasValidParameters() == false)
                {
                    objParam.Construct();
                }
                else
                {
                    objParam.SetDefaultFields(rstOpp);
                    objParam.WarningMessage = string.Empty;
                    ParameterList = objParam.ParameterList;
                }
                return vntRecordsets;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function create a new secondary record for the specified secondary
        /// </summary>
        /// <param name="pForm">The IRForm object reference to the client IRForm object</param>
        /// <param name="SecondaryName">The secondary name (the Segment name to hold a secondary)</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <param name="Recordset">Hold the reference for the secondary</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        public virtual void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset
            Recordset)
        {
            try
            {
                pForm.DoNewSecondaryData(SecondaryName, ref ParameterList, Recordset);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function updates the Opportunity Loan Profile plan
        /// </summary>
        /// <param name="pForm">The IRForm object reference to the client IRForm object</param>
        /// <param name="Recordsets">Hold the reference for the current primary recordset and its all</param>
        /// secondaries in the specified form
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date        Author          Description
        /// 3.8.0.0       5/12/2006   DYin            Converted to .Net C# code.
        /// </history>
        public virtual void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                object[] recordsetArray = (object[])Recordsets;
                Recordset rstPrimaryRecordset = (Recordset) recordsetArray[0];
                object vntOpportunity_Id = rstPrimaryRecordset.Fields[modOpportunity.strf_OPPORTUNITY_ID].Value;
                Opportunity objOpportunity = (Opportunity) RSysSystem.ServerScripts[modOpportunity.strsOPPORTUNITY].CreateInstance();

                pForm.DoSaveFormData(Recordsets, ref ParameterList);

                objOpportunity.CalculateTotals(vntOpportunity_Id, false);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine sets the Active Client System.
        /// </summary>
        /// <param name="pSystem">Active Client System Name</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        public virtual void SetSystem(RSystem pSystem)
        {
            try
            {
                RSysSystem = (IRSystem7) pSystem;
                RldtLangDict = RSysSystem.GetLDGroup(modOpportunity.strt_OPPORTUNITY);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Create a Post Sale Quote on a contract
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual object CreatePostSaleQuote(object quoteOpportunityId)
        {
            try
            {
                Opportunity objOpportunity = (Opportunity) RSysSystem.ServerScripts[modOpportunity.strsOPPORTUNITY].CreateInstance();

                // Copy over quote details and options
                object postSaleQuoteOpportunityId = objOpportunity.CopyQuote(quoteOpportunityId, true, true, true);

                // Populate Post_Sale Quote Id
                if (postSaleQuoteOpportunityId != DBNull.Value)
                {
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    Recordset rstOpp = objLib.GetRecordset(postSaleQuoteOpportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strf_PIPELINE_STAGE,
                        modOpportunity.strfPOST_SALE_ID, modOpportunity.strf_STATUS);
                    if (rstOpp.RecordCount > 0)
                    {
                        rstOpp.MoveFirst();
                        rstOpp.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsIN_PROGRESS;
                        rstOpp.Fields[modOpportunity.strfPOST_SALE_ID].Value = quoteOpportunityId;
                    }
                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstOpp);
                }
                return postSaleQuoteOpportunityId;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        // Apply the changes from a post sale quote to it's contract
        /// <summary>
        /// Functionality for when a user accepts a post sale quote
        /// </summary>
        /// <returns>n/a</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void ApplyPostSaleQuote(object postSaleQuoteOpportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Opportunity objOpportunity = (Opportunity) RSysSystem.ServerScripts[modOpportunity.strsOPPORTUNITY].CreateInstance();

                Recordset rstContract = objLib.GetRecordset(postSaleQuoteOpportunityId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfPOST_SALE_ID,
                    modOpportunity.strf_NBHD_PHASE_ID);
                object vntContractId = rstContract.Fields[modOpportunity.strfPOST_SALE_ID].Value;

                // Jul 20, 2005. By JWang. Get Division PPI Managment flag value.
                object vntReleaseId = rstContract.Fields[modOpportunity.strf_NBHD_PHASE_ID].Value;
                object vntNeighborhood = RSysSystem.Tables[modOpportunity.strt_NBHD_PHASE].Fields[modOpportunity.strfNEIGHBORHOOD_ID].Index(vntReleaseId);
                object vntDivision = RSysSystem.Tables[modOpportunity.strtNEIGHBORHOOD].Fields[modOpportunity.strf_DIVISION_ID].Index(vntNeighborhood);
                bool bolPPI = TypeConvert.ToBoolean(RSysSystem.Tables[modOpportunity.strtDIVISION].Fields[modOpportunity.strfPPI_MANAGEMENT].Index(vntDivision));

                // update the post sale quote pipeline stage and active flag
                // Jul 14, 2005. By JWang. Update Post Sale Quote status with "Accepted"
                Recordset rstPSQuote = objLib.GetRecordset(postSaleQuoteOpportunityId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfPIPELINE_STAGE,
                    modOpportunity.strfSTATUS, modOpportunity.strfINACTIVE);
                if (rstPSQuote.RecordCount > 0)
                {
                    // set the Pipeline stage here
                    if (TypeConvert.ToString(rstPSQuote.Fields[modOpportunity.strfPIPELINE_STAGE].Value) == modOpportunity.strsPOST_BUILD_QUOTE)
                    {
                        // post build quote
                        rstPSQuote.Fields[modOpportunity.strfPIPELINE_STAGE].Value = modOpportunity.strsPOST_BUILD_ACCEPTED;
                    }
                    else
                    {
                        rstPSQuote.Fields[modOpportunity.strfPIPELINE_STAGE].Value = modOpportunity.strsPOST_SALE_ACCEPTED;
                    }

                    rstPSQuote.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsACCEPTED;
                    rstPSQuote.Fields[modOpportunity.strfINACTIVE].Value = false;
                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstPSQuote);
                }

                // added by CLangan - create the change order here
                // Set up new Change Order Record
                Recordset rstChangeOrder = objLib.GetNewRecordset(modOpportunity.strtCHANGE_ORDER);

                rstChangeOrder.AddNew(Type.Missing, Type.Missing);
                rstChangeOrder.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = vntContractId;
                rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_DATE].Value = DateTime.Today;
                rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_NUMBER].Value = GetNextSequenceNumber(vntContractId);
                rstChangeOrder.Fields[modOpportunity.strfADDED_BY_ID].Value = RSysSystem.Tables[modOpportunity.strtEMPLOYEE].Fields[modOpportunity.strfRN_EMPLOYEE_USER_ID].Find(RSysSystem.CurrentUserId());
                // TODO (Di Yin) use LD String
                rstChangeOrder.Fields[modOpportunity.strfCHANGE_TYPE].Value = "Change Order";
                rstChangeOrder.Fields[modOpportunity.strfNEW_SALES_PRICE].Value = RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfQUOTE_TOTAL].Index(postSaleQuoteOpportunityId);

                // Jul 20, 2005. By JWang. Define TOTAL_PROJECT_COST and TOTAL_QUOTE_ADJUSTED fields
                rstChangeOrder.Fields[modOpportunity.strfTOTAL_PROJECT_COST].Value = RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfTOTAL_PROJECT_COST].Index(postSaleQuoteOpportunityId);

                if (bolPPI)
                {
                    // Division PPI Management is set, TOTAL_QUOTE_ADJUSTED = (Post Sale).Total_Project_cost - (Contract).Total_Project_cost
                    rstChangeOrder.Fields[modOpportunity.strfTOTAL_QUOTE_ADJUSTED].Value = TypeConvert.ToDouble(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfTOTAL_PROJECT_COST].Index(postSaleQuoteOpportunityId))
                        - TypeConvert.ToDouble(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfTOTAL_PROJECT_COST].Index(vntContractId));
                }
                else
                {   // Division PPI Management is not set, TOTAL_QUOTE_ADJUSTED = (Post Sale).Quote_Total - (Contract).Quote_Total
                    rstChangeOrder.Fields[modOpportunity.strfTOTAL_QUOTE_ADJUSTED].Value = TypeConvert.ToDouble(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfQUOTE_TOTAL].Index(postSaleQuoteOpportunityId))
                        - TypeConvert.ToDouble(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfQUOTE_TOTAL].Index(vntContractId));
                }

                // Total Adjustment = Contract Quote total - Accepted Post Sale Quote Total -> TLF
                Recordset rstPPIAdjustment = objLib.GetRecordset(modOpportunity.strqSELECTED_PPI_ADJUSTMENTS_FOR_OPP, 1, postSaleQuoteOpportunityId,
                    modOpportunity.strfSUM_FIELD);
                if (rstPPIAdjustment.RecordCount > 0)
                {
                    rstPPIAdjustment.MoveFirst();
                    while (!(rstPPIAdjustment.EOF))
                    {
                        rstChangeOrder.Fields[modOpportunity.strfTOTAL_PPI_ADJUSTMENTS].Value = TypeConvert.ToDouble(rstChangeOrder.Fields[modOpportunity.strfTOTAL_PPI_ADJUSTMENTS].Value)
                            + TypeConvert.ToDouble(rstPPIAdjustment.Fields[modOpportunity.strfSUM_FIELD].Value);
                        rstPPIAdjustment.MoveNext();
                    }
                }
                objLib.SaveRecordset(modOpportunity.strtCHANGE_ORDER, rstChangeOrder);
                object vntChangeOrderId = rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_ID].Value;

                // Get the Differences between the Post Sale Qutoe and the Contract and apply the Changes to the contract
                ProcessOptionRecords(postSaleQuoteOpportunityId, vntChangeOrderId);

                ProcessAdjustmentRecords(postSaleQuoteOpportunityId, vntChangeOrderId);

                // update the Contract's/IQ's total
                objOpportunity.CalculateTotals(vntContractId, false);

                // send out notification email to all the employees with notification change order set to true
                SendEmailNotification(postSaleQuoteOpportunityId);

                // Inactivate all other Post Sale Quotes for this Contract
                InactivateOtherPostSaleQuotes(vntContractId, false);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Process the Source and Target adjustment recordsets
        /// </summary>
        /// <param name="postSaleQuoteContractId">Post Sale Quote Contract Id</param>
        /// <param name="changeOrderId">change order id</param>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void ProcessAdjustmentRecords(object postSaleQuoteContractId, object changeOrderId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                object vntOriginalOppId = RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfPOST_SALE_ID].Index(postSaleQuoteContractId);

                // 1) get the removed adjustments
                Recordset rstPSQAdjustments = objLib.GetRecordset(modOpportunity.strqREMOVED_ADJUSTMENTS_FROM_PSQ_ADJUSTMENT_ORIG_OPP_PSQ_OPP,
                    2, vntOriginalOppId, postSaleQuoteContractId);

                // add the change order adjustments
                object opportunityAdjustmentId = DBNull.Value;
                Recordset rstChangeOrderAdjust = null;
                if (rstPSQAdjustments.RecordCount > 0)
                {
                    rstPSQAdjustments.MoveFirst();

                    while (!(rstPSQAdjustments.EOF))
                    {
                        // add a change order adjustment
                        object vntChangeOrderAdjustId = AddChangeOrderAdjustments(rstPSQAdjustments.Fields[modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID].Value,
                            changeOrderId, ChangeOrderStatus.Unselected);
                        opportunityAdjustmentId = rstPSQAdjustments.Fields[modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID].Value;

                        // update the new change order adjustment with the new opp adjustment
                        rstChangeOrderAdjust = objLib.GetRecordset(vntChangeOrderAdjustId, modOpportunity.strtCHANGE_ORDER_ADJUSTMENT,
                            modOpportunity.strfNEW_ADJUSTMENT, modOpportunity.strfPREVIOUS_ADJUSTMENT_AMOUNT, modOpportunity.strfPREVIOUS_ADJUSTMENT_PERCENT,
                            modOpportunity.strfPREVIOUS_APPLY_TO, modOpportunity.strfPREVIOUS_ADJUSTMENT_TOTAL, modOpportunity.strfPREVIOUS_ADJUSTMENT_ID,
                            modOpportunity.strfADJUSTMENT_AMOUNT, modOpportunity.strfADJUSTMENT_PERCENTAGE, modOpportunity.strfSUM_FIELD,
                            modOpportunity.strfAPPLY_TO);
                        Recordset rstAdjustment = objLib.GetRecordset(opportunityAdjustmentId, modOpportunity.strtOPPORTUNITY_ADJUSTMENT,
                            modOpportunity.strfADJUSTMENT_AMOUNT, modOpportunity.strfADJUSTMENT_PERCENTAGE, modOpportunity.strfSUM_FIELD,
                            modOpportunity.strfAPPLY_TO);

                        if (rstChangeOrderAdjust.RecordCount > 0)
                        {
                            rstChangeOrderAdjust.MoveFirst();
                            rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_ADJUSTMENT_ID].Value = opportunityAdjustmentId;
                            rstChangeOrderAdjust.Fields[modOpportunity.strfADJUSTMENT_AMOUNT].Value = 0;
                            rstChangeOrderAdjust.Fields[modOpportunity.strfADJUSTMENT_PERCENTAGE].Value = 0;
                            rstChangeOrderAdjust.Fields[modOpportunity.strfAPPLY_TO].Value = DBNull.Value;
                            rstChangeOrderAdjust.Fields[modOpportunity.strfSUM_FIELD].Value = 0;
                            if (rstAdjustment.RecordCount > 0)
                            {
                                rstAdjustment.MoveFirst();
                                rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_ADJUSTMENT_AMOUNT].Value = rstAdjustment.Fields[modOpportunity.strfADJUSTMENT_AMOUNT].Value;
                                rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_ADJUSTMENT_PERCENT].Value = rstAdjustment.Fields[modOpportunity.strfADJUSTMENT_PERCENTAGE].Value;
                                rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_APPLY_TO].Value = rstAdjustment.Fields[modOpportunity.strfAPPLY_TO].Value;
                                rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_ADJUSTMENT_TOTAL].Value = rstAdjustment.Fields[modOpportunity.strfSUM_FIELD].Value;
                            }
                            objLib.SaveRecordset(modOpportunity.strtCHANGE_ORDER_ADJUSTMENT, rstChangeOrderAdjust);
                        }

                        // remove the adjustment from the orginal contract
                        Recordset rstConAdjust = objLib.GetRecordset(opportunityAdjustmentId, modOpportunity.strtOPPORTUNITY_ADJUSTMENT,
                            modOpportunity.strfSELECTED);
                        if (rstConAdjust.RecordCount > 0)
                        {
                            rstConAdjust.MoveFirst();
                            if (!(rstConAdjust.EOF))
                            {
                                rstConAdjust.Fields[modOpportunity.strfSELECTED].Value = false;
                            }
                            objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY_ADJUSTMENT, rstConAdjust);
                        }
                        rstPSQAdjustments.MoveNext();
                    }
                }
                // 2) get the added adjustments
                rstPSQAdjustments = objLib.GetRecordset(modOpportunity.strqSELECTED_ADJUSTS_COPY_OF_ADJUST_UNDEFINED_OPP,
                    1, postSaleQuoteContractId);
                // add the change order adjustments
                if (rstPSQAdjustments.RecordCount > 0)
                {
                    rstPSQAdjustments.MoveFirst();

                    while (!(rstPSQAdjustments.EOF))
                    {
                        // add a change order adjustment
                        object vntChangeOrderAdjustId = AddChangeOrderAdjustments(rstPSQAdjustments.Fields[modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID].Value,
                            changeOrderId, ChangeOrderStatus.Selected);
                        // add the adjustment to the original opp
                        opportunityAdjustmentId = AddPostSaleQuoteAdjustmentToOriginalOpportunity(vntOriginalOppId, rstPSQAdjustments.Fields[modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID].Value);

                        // update the new change order adjustment with the new opp adjustment
                        rstChangeOrderAdjust = objLib.GetRecordset(vntChangeOrderAdjustId, modOpportunity.strtCHANGE_ORDER_ADJUSTMENT,
                            modOpportunity.strfNEW_ADJUSTMENT, modOpportunity.strfPREVIOUS_ADJUSTMENT_AMOUNT, modOpportunity.strfPREVIOUS_ADJUSTMENT_PERCENT,
                            modOpportunity.strfPREVIOUS_APPLY_TO, modOpportunity.strfPREVIOUS_ADJUSTMENT_TOTAL, modOpportunity.strfADJUSTMENT_AMOUNT,
                            modOpportunity.strfADJUSTMENT_PERCENTAGE, modOpportunity.strfSUM_FIELD, modOpportunity.strfAPPLY_TO);
                        Recordset rstAdjustment = objLib.GetRecordset(opportunityAdjustmentId, modOpportunity.strtOPPORTUNITY_ADJUSTMENT,
                            modOpportunity.strfADJUSTMENT_AMOUNT, modOpportunity.strfADJUSTMENT_PERCENTAGE, modOpportunity.strfSUM_FIELD,
                            modOpportunity.strfAPPLY_TO);

                        if (rstChangeOrderAdjust.RecordCount > 0)
                        {
                            rstChangeOrderAdjust.MoveFirst();
                            rstChangeOrderAdjust.Fields[modOpportunity.strfNEW_ADJUSTMENT].Value = opportunityAdjustmentId;
                            rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_ADJUSTMENT_AMOUNT].Value = 0;
                            rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_ADJUSTMENT_PERCENT].Value = 0;
                            rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_APPLY_TO].Value = DBNull.Value;
                            rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_ADJUSTMENT_TOTAL].Value = 0;
                            if (rstAdjustment.RecordCount > 0)
                            {
                                rstAdjustment.MoveFirst();
                                rstChangeOrderAdjust.Fields[modOpportunity.strfADJUSTMENT_AMOUNT].Value = rstAdjustment.Fields[modOpportunity.strfADJUSTMENT_AMOUNT].Value;
                                rstChangeOrderAdjust.Fields[modOpportunity.strfADJUSTMENT_PERCENTAGE].Value = rstAdjustment.Fields[modOpportunity.strfADJUSTMENT_PERCENTAGE].Value;
                                rstChangeOrderAdjust.Fields[modOpportunity.strfSUM_FIELD].Value = rstAdjustment.Fields[modOpportunity.strfSUM_FIELD].Value;
                                rstChangeOrderAdjust.Fields[modOpportunity.strfAPPLY_TO].Value = rstAdjustment.Fields[modOpportunity.strfAPPLY_TO].Value;
                            }

                            objLib.SaveRecordset(modOpportunity.strtCHANGE_ORDER_ADJUSTMENT, rstChangeOrderAdjust);
                        }

                        rstPSQAdjustments.MoveNext();
                    }
                }
                // 3) get the remaining adjustments and see if they have been modified
                rstPSQAdjustments = objLib.GetRecordset(modOpportunity.strqSELECTED_ADJUSTS_COPY_OF_ADJUST_DEFINED_OPP,
                    1, postSaleQuoteContractId);

                if (rstPSQAdjustments.RecordCount > 0)
                {
                    rstPSQAdjustments.MoveFirst();

                    while (!(rstPSQAdjustments.EOF))
                    {
                        object vntOldOppAdjustmentId = rstPSQAdjustments.Fields[modOpportunity.strfCOPY_OF_ADJUSTMENT_ID].Value;
                        if (IsAdjustmentModified(vntOldOppAdjustmentId, rstPSQAdjustments.Fields[modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID].Value))
                        {
                            // add a change order adjustment
                            object vntChangeOrderAdjustId = AddChangeOrderAdjustments(rstPSQAdjustments.Fields[modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID].Value,
                                changeOrderId, ChangeOrderStatus.Changed);

                            // update the replaced by in the original adjustment and unselect it
                            Recordset rstAdjustment = objLib.GetRecordset(vntOldOppAdjustmentId, modOpportunity.strtOPPORTUNITY_ADJUSTMENT);
                            rstAdjustment.Fields[modOpportunity.strfREPLACED_BY_ADJUSTMENT_ID].Value = rstPSQAdjustments.Fields[modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID].Value;
                            rstAdjustment.Fields[modOpportunity.strfSELECTED].Value = false;
                            objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY_ADJUSTMENT, rstAdjustment);

                            // update the previous adjustment link
                            rstChangeOrderAdjust = objLib.GetRecordset(vntChangeOrderAdjustId, modOpportunity.strtCHANGE_ORDER_ADJUSTMENT);

                            if (rstChangeOrderAdjust.RecordCount > 0)
                            {
                                rstChangeOrderAdjust.MoveFirst();
                                rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_ADJUSTMENT_ID].Value = vntOldOppAdjustmentId;
                                if (rstAdjustment.RecordCount > 0)
                                {
                                    rstAdjustment.MoveFirst();
                                    rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_ADJUSTMENT_AMOUNT].Value = rstAdjustment.Fields[modOpportunity.strfADJUSTMENT_AMOUNT].Value;
                                    rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_ADJUSTMENT_PERCENT].Value = rstAdjustment.Fields[modOpportunity.strfADJUSTMENT_PERCENTAGE].Value;
                                    rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_APPLY_TO].Value = rstAdjustment.Fields[modOpportunity.strfAPPLY_TO].Value;
                                    rstChangeOrderAdjust.Fields[modOpportunity.strfPREVIOUS_ADJUSTMENT_TOTAL].Value = rstAdjustment.Fields[modOpportunity.strfSUM_FIELD].Value;
                                }
                            }
                            // add the adjustment to the original opp
                            opportunityAdjustmentId = AddPostSaleQuoteAdjustmentToOriginalOpportunity(vntOriginalOppId, rstPSQAdjustments.Fields[modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID].Value);
                            rstAdjustment = objLib.GetRecordset(opportunityAdjustmentId, modOpportunity.strtOPPORTUNITY_ADJUSTMENT,
                                modOpportunity.strfREPLACES_ADJUSTMENT_ID);

                            if (rstAdjustment.RecordCount > 0)
                            {
                                rstAdjustment.MoveFirst();
                                rstAdjustment.Fields[modOpportunity.strfREPLACES_ADJUSTMENT_ID].Value = vntOldOppAdjustmentId;

                                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY_ADJUSTMENT, rstAdjustment);
                            }

                            rstChangeOrderAdjust.Fields[modOpportunity.strfNEW_ADJUSTMENT].Value = opportunityAdjustmentId;
                            rstChangeOrderAdjust.Fields[modOpportunity.strfADJUSTMENT_AMOUNT].Value = rstPSQAdjustments.Fields[modOpportunity.strfADJUSTMENT_AMOUNT].Value;
                            rstChangeOrderAdjust.Fields[modOpportunity.strfADJUSTMENT_PERCENTAGE].Value = rstPSQAdjustments.Fields[modOpportunity.strfADJUSTMENT_PERCENTAGE].Value;
                            rstChangeOrderAdjust.Fields[modOpportunity.strfAPPLY_TO].Value = rstPSQAdjustments.Fields[modOpportunity.strfAPPLY_TO].Value;
                            rstChangeOrderAdjust.Fields[modOpportunity.strfSUM_FIELD].Value = rstPSQAdjustments.Fields[modOpportunity.strfSUM_FIELD].Value;

                            objLib.SaveRecordset(modOpportunity.strtCHANGE_ORDER_ADJUSTMENT, rstChangeOrderAdjust);
                        }
                        rstPSQAdjustments.MoveNext();
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Get Difference between 2 Contract Option recordsets
        /// </summary>
        /// <param name="postSaleQuoteOpportunityId">Post Sale Quote Contract Id</param>
        /// <param name="changeOrderId">change order id</param>
        /// <returns>n/a</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// 3.8.1.0        9/21/2006      NDcu      Added "this" keyword to the ModifyContractOption 
        /// 5.9.1.0        4/10/2007      BC        To handle the Package Components
        /// </history>
        protected virtual void ProcessOptionRecords(object postSaleQuoteOpportunityId, object changeOrderId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Opportunity objOpportunity = (Opportunity) RSysSystem.ServerScripts[modOpportunity.strsOPPORTUNITY].CreateInstance();

                object vntContractOppId = RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfPOST_SALE_ID].Index(postSaleQuoteOpportunityId);
                bool blnInventoryQuote = TypeConvert.ToString(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfSTATUS].Index(vntContractOppId))
                    == modOpportunity.strsINVENTORY ? true : false;

                // Jul 25, 2005. By JWang
                // Changed the order of processing Added options and Removed options, so that process removed option
                // first,
                // then process added options. The reason for this is that we can use select options in post sale quote
                // to remove an existing
                // elevation, and add a new elevation. When we remove an elevation, system will set the source contract's
                // elevation field as null.
                // so the order of processing does matter.
                // 2) get the removed options
                Recordset postSaleQuoteOptionRecordset = objLib.GetRecordset(modOpportunity.strqOP_PRODS_FOR_OPP_ORG_NOT_SELECTED, 1, postSaleQuoteOpportunityId,
                    modOpportunity.strfOPPORTUNITY__PRODUCT_ID, modOpportunity.strfORIG_OPP_PROD_ID, modOpportunity.strf_NBHDP_PRODUCT_ID, 
                    modOpportunity.strf_DIVISION_PRODUCT_ID);
                if (postSaleQuoteOptionRecordset.RecordCount > 0)
                {
                    postSaleQuoteOptionRecordset.MoveFirst();
                    while (!(postSaleQuoteOptionRecordset.EOF))
                    {
                        //Consider only Normal & Custom Options and not the Package Components
                        if(Convert.IsDBNull(postSaleQuoteOptionRecordset.Fields[modOpportunity.strf_NBHDP_PRODUCT_ID].Value) && 
                            !Convert.IsDBNull(postSaleQuoteOptionRecordset.Fields[modOpportunity.strf_DIVISION_PRODUCT_ID].Value))
                        {}
                        else
                            this.ModifyContractOption(vntContractOppId, postSaleQuoteOptionRecordset.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                                ChangeOrderLogic.Remove, ChangeOrderStatus.Unselected, changeOrderId, blnInventoryQuote, 
                                postSaleQuoteOptionRecordset.Fields[modOpportunity.strfORIG_OPP_PROD_ID].Value);
                        postSaleQuoteOptionRecordset.MoveNext();
                    }
                }

                // 1) get the added options
                postSaleQuoteOptionRecordset = objLib.GetRecordset(modOpportunity.strqOPP_PRODUCTS_FOR_OPP_ORIG_ID, 1, postSaleQuoteOpportunityId,
                    modOpportunity.strfOPPORTUNITY__PRODUCT_ID, modOpportunity.strf_NBHDP_PRODUCT_ID, 
                    modOpportunity.strf_DIVISION_PRODUCT_ID);
                if (postSaleQuoteOptionRecordset.RecordCount > 0)
                {
                    postSaleQuoteOptionRecordset.MoveFirst();
                    while (!(postSaleQuoteOptionRecordset.EOF))
                    {
                        if(Convert.IsDBNull(postSaleQuoteOptionRecordset.Fields[modOpportunity.strf_NBHDP_PRODUCT_ID].Value) && 
                            !Convert.IsDBNull(postSaleQuoteOptionRecordset.Fields[modOpportunity.strf_DIVISION_PRODUCT_ID].Value))
                        {}
                        else
                            this.ModifyContractOption(vntContractOppId, postSaleQuoteOptionRecordset.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                                ChangeOrderLogic.Add, ChangeOrderStatus.Selected, changeOrderId, blnInventoryQuote);
                        postSaleQuoteOptionRecordset.MoveNext();
                    }
                }

                // 3) get the modified options
                postSaleQuoteOptionRecordset = objLib.GetRecordset(modOpportunity.strqOP_PRODS_FOR_OPP_ORG_SELECTED, 1, postSaleQuoteOpportunityId);

                if (postSaleQuoteOptionRecordset.RecordCount > 0)
                {
                    postSaleQuoteOptionRecordset.MoveFirst();
                    while (!(postSaleQuoteOptionRecordset.EOF))
                    {
                        Recordset rstContractOptions = objLib.GetRecordset(postSaleQuoteOptionRecordset.Fields[modOpportunity.strfORIG_OPP_PROD_ID].Value,
                            modOpportunity.strtOPPORTUNITY__PRODUCT);
                        bool blnQuantityModified = false;
                        bool bOptionModified = IsOptionModified(rstContractOptions, postSaleQuoteOptionRecordset, out blnQuantityModified);
                        if (bOptionModified)
                        {
                            ModifyContractOption(vntContractOppId, postSaleQuoteOptionRecordset.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                                ChangeOrderLogic.Change, ChangeOrderStatus.Changed, changeOrderId, blnInventoryQuote,
                                rstContractOptions.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value);
                            if (blnInventoryQuote)
                            {
                                if (blnQuantityModified)
                                {
                                    // inactivate in this case
                                    objOpportunity.InactivateCustomerQuotes(System.DBNull.Value, postSaleQuoteOpportunityId, InactiveQuoteReason.PostBuildAccept);
                                }
                                else
                                {
                                    // perform an update
                                    objOpportunity.UpdateCustomerQuoteLocations(vntContractOppId);
                                }
                            }
                        }
                        postSaleQuoteOptionRecordset.MoveNext();
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Get the next sequence number for the change order
        /// </summary>
        /// <param name="opportunityId">the opportunity id</param>
        /// <returns>
        /// long - the next sequence number</returns>
        /// <history>
        /// Revision#  Date        Author    Description
        /// 3.8.0.0    5/12/2006   DYin      Converted to .Net C# code.
        /// </history>
        protected virtual int GetNextSequenceNumber(object opportunityId)
        {
            try
            {
                int lngSequenceNumber = 1;

                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstChangeOrder = objLib.GetRecordset(modOpportunity.strqHB_CHANGE_ORDERS_FOR_OPPORTUNITY, 1, opportunityId,
                    modOpportunity.strfCHANGE_ORDER_NUMBER);
                lngSequenceNumber = rstChangeOrder.RecordCount + 1;
                return lngSequenceNumber;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Inactivate the remaining Post Sale Quotes after one has been Applied.
        /// </summary>
        /// Sets the inactive flags on all other Post Sale Quotes for a Contract.
        /// <returns>nothing</returns>
        /// <history>
        /// nothing
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void InactivateOtherPostSaleQuotes(object contractId, bool forAllPostSaleQuote)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                string strStatus = TypeConvert.ToString(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfSTATUS].Index(contractId)).Trim();
                if (strStatus.Length > 0)
                {
                    // TODO (Di Yin) strPipelineStage is never used affter assignd value.
                    string strPipelineStage = string.Empty;
                    if (strStatus == modOpportunity.strsINVENTORY)
                    {
                        strPipelineStage = TypeConvert.ToString(RldtLangDict.GetText(modOpportunity.strlPOST_BUILD_SALE));
                    }
                    else
                    {
                        strPipelineStage = TypeConvert.ToString(RldtLangDict.GetText(modOpportunity.strlPOST_SALE));
                    }

                    //2006-12-01 JWang. In C# we should pass null for optional parameter of a query if no specific value to pass.
                    //Recordset rstOpp = objLib.GetRecordset(modOpportunity.strqACTIVE_POST_SALE_QUOTES_FOR_OPP, 2, contractId, "" /* EMPTY */, modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfPOST_SALE_ID, modOpportunity.strfINACTIVE, modOpportunity.strfPIPELINE_STAGE, modOpportunity.strfSTATUS);
                    Recordset rstOpp = objLib.GetRecordset(modOpportunity.strqACTIVE_POST_SALE_QUOTES_FOR_OPP, 2, contractId, null, modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfPOST_SALE_ID, modOpportunity.strfINACTIVE, modOpportunity.strfPIPELINE_STAGE, modOpportunity.strfSTATUS);
                    if (rstOpp.RecordCount > 0)
                    {
                        rstOpp.MoveFirst();
                        while (!(rstOpp.EOF))
                        {
                            // RY: Added if blnAllPSQ is true, inactivate regardless of pipeline stage.
                            if (TypeConvert.ToString(rstOpp.Fields[modOpportunity.strfPIPELINE_STAGE].Value) == TypeConvert.ToString(RldtLangDict.GetText(modOpportunity.strlPOST_SALE))
                                || TypeConvert.ToString(rstOpp.Fields[modOpportunity.strfPIPELINE_STAGE].Value) == TypeConvert.ToString(RldtLangDict.GetText(modOpportunity.strlPOST_BUILD_SALE))
                                || forAllPostSaleQuote)
                            {
                                rstOpp.Fields[modOpportunity.strfINACTIVE].Value = true;
                                // Jul 20, 2005. By JWang.
                                // As per Sean, the Pipeline_Stage will not be changed after the quote, post sale quote
                                // or post build quote becomes Inactive
                                rstOpp.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsINACTIVE;
                            }
                            rstOpp.MoveNext();
                        }
                    }
                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstOpp);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Determine if an option on a Post Sale Quote has been modified compared to the origional contract
        /// </summary>
        /// <param name="contractOptionRecordset">Recordset containing Contract Option</param>
        /// <param name="postSaleQuoteOptionRecordset">recordset containing Post Sale Quote option</param>
        /// <param name="quantityModified">set when quantity has been modifed either on the option or the secondary</param>- 
        /// <returns>Boolean - True if modified; false otherwise</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual bool IsOptionModified(Recordset contractOptionRecordset, Recordset postSaleQuoteOptionRecordset, 
            out bool quantityModified)
        {
            try
            {
                quantityModified = false;
                // 1) compare the option records
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                string[] arrFields = objLib.GetAllFieldNames(modOpportunity.strtOPPORTUNITY__PRODUCT, modOpportunity.strfOPPORTUNITY__PRODUCT_ID);

                for(int i = 0; i < arrFields.Length; ++i)
                {
                    // Filter out fields we do not want to check on...
                    if ((arrFields[i].Substring(0, 3).ToUpper() != "RN_") && (arrFields[i] != modOpportunity.strfOPPORTUNITY__PRODUCT_ID)
                        && (arrFields[i] != modOpportunity.strfORIG_OPP_PROD_ID) && (arrFields[i] != modOpportunity.strfOPPORTUNITY_ID)
                        //&& (arrFields[i] != modOpportunity.strfPRODUCT_AVAILABLE)
                        )
                    {
                        if (contractOptionRecordset.Fields[arrFields[i]].Type == DataTypeEnum.adBinary)
                        {
                            if (!Share.EqualValues(contractOptionRecordset.Fields[arrFields[i]].Value, postSaleQuoteOptionRecordset.Fields[arrFields[i]].Value))
                            {
                                if (arrFields[i] == modOpportunity.strfQUANTITY)
                                {
                                    quantityModified = true;
                                }
                                return true;
                            }
                        }
                        else
                        {
                            if (!Share.Equals(contractOptionRecordset.Fields[arrFields[i]].Value, postSaleQuoteOptionRecordset.Fields[arrFields[i]].Value))
                            {
                                if (arrFields[i] == modOpportunity.strfQUANTITY)
                                {
                                    quantityModified = true;
                                }
                                return true;
                            }
                        }
                    }
                }
                // 2) compare the option secondaries
                // 2a) Opp_Product_Location
                return IsOptionSecondaryModified(contractOptionRecordset.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                    postSaleQuoteOptionRecordset.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value, modOpportunity.strtOPP_PRODUCT_LOCATION,
                    modOpportunity.strfOPPORTUNITY_PRODUCT_ID, out quantityModified, modOpportunity.strfRN_DESCRIPTOR);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Determine if an option's secondary has been modified on a Post Sale Quote
        /// </summary>
        /// <param name="sourceOptionId">Source Option Id</param>
        /// <param name="targetOptionId">Target option id</param>
        /// <param name="tableName">secondary table name</param>
        /// <param name="linkFieldName">the link field</param>
        /// <param name="quantityModified">Flag to indicate if quantity is modified</param>
        /// <param name="sortFieldName">Field name for sort</param>
        /// <returns>Boolean - True if modified; false otherwise</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual bool IsOptionSecondaryModified(object sourceOptionId, object targetOptionId, string tableName,
            string linkFieldName, out bool quantityModified, string sortFieldName)
        {
            try
            {
                quantityModified = false;

                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                string strPrimaryFieldName = TypeConvert.ToString(RSysSystem.Tables[tableName].PrimaryKeyField.FieldName);
                string[] arrFields = objLib.GetAllFieldNames(tableName, strPrimaryFieldName);

                objLib.SortFieldName = sortFieldName;
                Recordset rstTarget = objLib.GetLinkedRecordset(tableName, linkFieldName, targetOptionId, arrFields);

                Recordset rstSource = objLib.GetLinkedRecordset(tableName, linkFieldName, sourceOptionId, arrFields);

                if (rstSource.RecordCount > 0 && rstTarget.RecordCount > 0)
                {
                    while(!(rstSource.EOF) && !(rstTarget.EOF))
                    {
                        for (int i = 0; i < arrFields.Length; ++i)
                        {
                            if ((arrFields[i] != modOpportunity.strfOPP_PRODUCT_ID) && 
                                (arrFields[i] != modOpportunity.strfOPPORTUNITY_ID) && 
                                (arrFields[i] != modOpportunity.strfOPP_PRODUCT_LOCATION_ID))
                            {
                                if (rstSource.Fields[arrFields[i]].Type == DataTypeEnum.adBinary)
                                {
                                    if (!Share.EqualValues(rstSource.Fields[arrFields[i]].Value, rstTarget.Fields[arrFields[i]].Value))
                                    {
                                        if (arrFields[i] == modOpportunity.strfQUANTITY)
                                        {
                                            quantityModified = true;
                                        }
                                        return true;
                                    }
                                }
                                else
                                {
                                    if (!Share.Equals(rstSource.Fields[arrFields[i]].Value, rstTarget.Fields[arrFields[i]].Value))
                                    {
                                        if (arrFields[i] == modOpportunity.strfQUANTITY)
                                        {
                                            quantityModified = true;
                                        }
                                        return true;
                                    }
                                }
                            }
                        }

                        // if this is the location table, we need to check the OppProd_Loc_Attribute_Pref
                        if (tableName == modOpportunity.strtOPP_PRODUCT_LOCATION)
                        {
                            return IsOptionSecondaryModified(rstSource.Fields[strPrimaryFieldName].Value, rstTarget.Fields[strPrimaryFieldName].Value,
                                modOpportunity.strtOPPPROD_ATTR_PREF, strPrimaryFieldName, out quantityModified, modOpportunity.strfRN_DESCRIPTOR);
                        }
                        rstSource.MoveNext();
                        rstTarget.MoveNext();
                    }
                    if (!(rstSource.EOF) || !(rstTarget.EOF))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Make Option changes to the Contract
        /// If ADD
        /// If REMOVE
        /// If CHANGE
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <param name="postSaleQuoteOpportunityProductId">PSQ Opp Product</param>
        /// <param name="changeOrderLogic">Change Order as enumerator value(Add/Remove/Change)</param>
        /// <param name="changeOrderStatus">Change Order Status as enumerator value</param>
        /// <param name="changeOrderId">Change Order id</param>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void ModifyContractOption(object contractId, object postSaleQuoteOpportunityProductId, 
            ChangeOrderLogic changeOrderLogic, ChangeOrderStatus changeOrderStatus, object changeOrderId)
        {
            this.ModifyContractOption(contractId, postSaleQuoteOpportunityProductId, changeOrderLogic, changeOrderStatus,
                changeOrderId, false, DBNull.Value);
        }

        /// <summary>
        /// Make Option changes to the Contract
        /// If ADD
        /// If REMOVE
        /// If CHANGE
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <param name="postSaleQuoteOpportunityProductId">PSQ Opp Product</param>
        /// <param name="changeOrderLogic">Change Order as enumerator value(Add/Remove/Change)</param>
        /// <param name="changeOrderStatus">Change Order Status as enumerator value</param>
        /// <param name="changeOrderId">Change Order id</param>
        /// <param name="inventoryQuote">handle updates/incativates for inventory quote</param>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// 3.8.1.0        9/21/2006      NDcu      Added  DBNull.Value as the parameter to the ModifyContractOption method 
        /// </history>
        protected virtual void ModifyContractOption(object contractId, object postSaleQuoteOpportunityProductId, 
            ChangeOrderLogic changeOrderLogic, ChangeOrderStatus changeOrderStatus, object changeOrderId,
            bool inventoryQuote)
        {
            this.ModifyContractOption(contractId, postSaleQuoteOpportunityProductId, changeOrderLogic, changeOrderStatus,
                changeOrderId, inventoryQuote, DBNull.Value);
        }

        /// <summary>
        /// Make Option changes to the Contract
        /// If ADD
        /// If REMOVE
        /// If CHANGE
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <param name="postSaleQuoteOpportunityProductId">PSQ Opp Product</param>
        /// <param name="changeOrderLogic">Change Order as enumerator value(Add/Remove/Change)</param>
        /// <param name="changeOrderStatus">Change Order Status as enumerator value</param>
        /// <param name="changeOrderId">Change Order id</param>
        /// <param name="inventoryQuote">handle updates/incativates for inventory quote</param>
        /// <param name="contractOpportunityProductId">Contract Option Product</param>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void ModifyContractOption(object contractId, object postSaleQuoteOpportunityProductId, 
            ChangeOrderLogic changeOrderLogic, ChangeOrderStatus changeOrderStatus, object changeOrderId, 
            bool inventoryQuote, object contractOpportunityProductId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                // Get the Post Sale Quote Record
                Recordset rstPSQOppProd = objLib.GetRecordset(postSaleQuoteOpportunityProductId, modOpportunity.strtOPPORTUNITY__PRODUCT);

                Opportunity objOpportunity = (Opportunity) RSysSystem.ServerScripts[modOpportunity.strsOPPORTUNITY].CreateInstance();

                // update Change Order - create a change order option
                object vntNHHDP_ProductId = RSysSystem.Tables[modOpportunity.strtOPPORTUNITY__PRODUCT].Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Index(postSaleQuoteOpportunityProductId);
                string strOptionType = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtOPPORTUNITY__PRODUCT, modOpportunity.strfTYPE,
                    postSaleQuoteOpportunityProductId));

                object vntChngOrderOptionId = DBNull.Value;
                if (vntNHHDP_ProductId == DBNull.Value)
                {
                    // custom option
                    vntChngOrderOptionId = AddCustomChangeOrderOption(postSaleQuoteOpportunityProductId, changeOrderId, contractId,
                        changeOrderStatus);
                }
                else
                {
                    if (changeOrderLogic == ChangeOrderLogic.Add || changeOrderLogic == ChangeOrderLogic.Change)
                    {
                        // add
                        vntChngOrderOptionId = objOpportunity.AddChangeOrders(new object[] {RSysSystem.IdToString(vntNHHDP_ProductId)},
                            changeOrderId, rstPSQOppProd.Fields[modOpportunity.strfOPPORTUNITY_ID].Value, changeOrderStatus);
                    }
                    else
                    {
                        vntChngOrderOptionId = objOpportunity.AddChangeOrders(new object[] {RSysSystem.IdToString(vntNHHDP_ProductId)},
                            changeOrderId, contractId, changeOrderStatus);
                    }
                }
                // Set up field array
                string[] arrFields = objLib.GetAllFieldNames(modOpportunity.strtOPPORTUNITY__PRODUCT,  modOpportunity.strfOPPORTUNITY__PRODUCT_ID);

                Recordset rstContractOppProd = null;
                Recordset rstChangeOrder = null;

                switch (changeOrderLogic)
                {
                    case ChangeOrderLogic.Add:
                        // Get the New Contract Options Record
                        rstContractOppProd = objLib.GetNewRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT);
                        rstContractOppProd.AddNew(Type.Missing, Type.Missing);

                        // Copy the PSQ option to the Contract
                        for (int i = 0; i < arrFields.Length; ++i)
                        {
                            // Filter out fields we do not want to check on...
                            if ((arrFields[i].Substring(0, 3).ToUpper() != "RN_") &&
                                (arrFields[i] != modOpportunity.strfOPPORTUNITY__PRODUCT_ID) &&
                                (arrFields[i] != modOpportunity.strfORIG_OPP_PROD_ID) &&
                                (arrFields[i] != modOpportunity.strfOPPORTUNITY_ID))
                            {
                                rstContractOppProd.Fields[arrFields[i]].Value = rstPSQOppProd.Fields[arrFields[i]].Value;
                            }
                        }
                        rstContractOppProd.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = contractId;
                        rstContractOppProd.Fields[modOpportunity.strfSELECTED].Value = true;
                        objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstContractOppProd);

                        // copy over the option secondaries
                        objOpportunity.CopyOptionSecondaryByOption(postSaleQuoteOpportunityProductId, rstContractOppProd.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value);

                        // set the Option id in the change order option record created
                        rstChangeOrder = objLib.GetRecordset(vntChngOrderOptionId, modOpportunity.strtCHANGE_ORDER_OPTIONS,
                            modOpportunity.strfOPP_PRODUCT_ID, modOpportunity.strfPREVIOUS_PRICE, modOpportunity.strfPREVIOUS_QUANTITY,
                            modOpportunity.strfQUANTITY, modOpportunity.strfPRICE);
                        if (!(rstChangeOrder.EOF))
                        {
                            rstChangeOrder.MoveFirst();
                            rstChangeOrder.Fields[modOpportunity.strfOPP_PRODUCT_ID].Value = rstContractOppProd.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value;
                            rstChangeOrder.Fields[modOpportunity.strfPREVIOUS_PRICE].Value = 0;
                            rstChangeOrder.Fields[modOpportunity.strfPREVIOUS_QUANTITY].Value = 0;
                            // previous extended price calculated with TLF
                            objLib.SaveRecordset(modOpportunity.strtCHANGE_ORDER_OPTIONS, rstChangeOrder);
                        }

                        // Inventory Customer Quotes need to be inactivated
                        if (inventoryQuote)
                        {
                            objOpportunity.InactivateCustomerQuotes(postSaleQuoteOpportunityProductId, contractId, InactiveQuoteReason.NoReason);
                        }

                        // Jul 08, 2005. By JWang
                        // A new elevation is added, set the Contract's chosen elevation.
                        if (strOptionType == modOpportunity.strsELEVATION)
                        {
                            objOpportunity.UpdateQuoteChosenElevation(contractId, vntNHHDP_ProductId);
                        }
                        break;

                    case ChangeOrderLogic.Remove:
                        // Get the Contract Options Record
                        rstContractOppProd = objLib.GetRecordset(contractOpportunityProductId, modOpportunity.strtOPPORTUNITY__PRODUCT,
                            modOpportunity.strfPRICE, modOpportunity.strfQUANTITY, modOpportunity.strfSELECTED);
                        rstContractOppProd.Fields[modOpportunity.strfSELECTED].Value = false;
                        objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstContractOppProd);

                        // set the Option id in the change order option record created
                        rstChangeOrder = objLib.GetRecordset(vntChngOrderOptionId, modOpportunity.strtCHANGE_ORDER_OPTIONS,
                            modOpportunity.strfOPP_PRODUCT_ID, modOpportunity.strfPREVIOUS_PRICE, modOpportunity.strfPREVIOUS_QUANTITY,
                            modOpportunity.strfQUANTITY, modOpportunity.strfPRICE, modOpportunity.strfPREVIOUS_OPTION_ID);
                        if (!(rstChangeOrder.EOF))
                        {
                            rstChangeOrder.MoveFirst();
                            rstChangeOrder.Fields[modOpportunity.strfPREVIOUS_OPTION_ID].Value = rstContractOppProd.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value;
                            rstChangeOrder.Fields[modOpportunity.strfPREVIOUS_PRICE].Value = rstContractOppProd.Fields[modOpportunity.strfPRICE].Value;
                            rstChangeOrder.Fields[modOpportunity.strfPREVIOUS_QUANTITY].Value = rstContractOppProd.Fields[modOpportunity.strfQUANTITY].Value;
                            // previous extended price calculated with TLF
                            rstChangeOrder.Fields[modOpportunity.strfPRICE].Value = 0;
                            rstChangeOrder.Fields[modOpportunity.strfQUANTITY].Value = 0;
                            objLib.SaveRecordset(modOpportunity.strtCHANGE_ORDER_OPTIONS, rstChangeOrder);
                        }

                        // Inventory Customer Quotes need to be inactivated
                        if (inventoryQuote)
                        {
                            objOpportunity.InactivateCustomerQuotes(postSaleQuoteOpportunityProductId, contractId, InactiveQuoteReason.NoReason);
                        }

                        // Jul 08, 2005. By JWang
                        // Elevation is removed, clear out the Contract's chosen elevation
                        if (strOptionType == modOpportunity.strsELEVATION)
                        {
                            objOpportunity.UpdateQuoteChosenElevation(contractId, System.DBNull.Value);
                        }
                        break;
                    case ChangeOrderLogic.Change:
                        // First, hide the origional option on the contract
                        rstContractOppProd = objLib.GetRecordset(contractOpportunityProductId, modOpportunity.strtOPPORTUNITY__PRODUCT);
                        rstContractOppProd.Fields[modOpportunity.strfSELECTED].Value = false;

                        // Now, create a New Contract Options Record
                        Recordset rstNewContractOppProd = objLib.GetNewRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT);
                        rstNewContractOppProd.AddNew(Type.Missing, Type.Missing);
                        for (int i = 0; i < arrFields.Length; ++i)
                        {
                            // Filter out fields we do not want to check on...
                            if ((arrFields[i].Substring(0, 3).ToUpper() != "RN_") &&
                                (arrFields[i] != modOpportunity.strfOPPORTUNITY__PRODUCT_ID) &&
                                (arrFields[i] != modOpportunity.strfORIG_OPP_PROD_ID) &&
                                (arrFields[i] != modOpportunity.strfOPPORTUNITY_ID))
                            {
                                rstNewContractOppProd.Fields[arrFields[i]].Value = rstPSQOppProd.Fields[arrFields[i]].Value;
                            }
                        }
                        rstNewContractOppProd.Fields[modOpportunity.strfREPLACES_OPTION_ID].Value = contractOpportunityProductId;
                        rstNewContractOppProd.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = contractId;
                        rstNewContractOppProd.Fields[modOpportunity.strfSELECTED].Value = true;
                        objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstNewContractOppProd);

                        // copy over the option secondaries
                        objOpportunity.CopyOptionSecondaryByOption(postSaleQuoteOpportunityProductId, rstNewContractOppProd.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value);

                        // set the replaced_by_option_id
                        rstContractOppProd.Fields[modOpportunity.strfREPLACED_BY_OPTION_ID].Value = rstNewContractOppProd.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value;
                        objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstContractOppProd);

                        // set the Option id in the change order option record created
                        rstChangeOrder = objLib.GetRecordset(vntChngOrderOptionId, modOpportunity.strtCHANGE_ORDER_OPTIONS,
                            modOpportunity.strfOPP_PRODUCT_ID, modOpportunity.strfPREVIOUS_PRICE, modOpportunity.strfPREVIOUS_QUANTITY,
                            modOpportunity.strfQUANTITY, modOpportunity.strfPRICE, modOpportunity.strfPREVIOUS_OPTION_ID);
                        if (!(rstChangeOrder.EOF))
                        {
                            rstChangeOrder.MoveFirst();
                            rstChangeOrder.Fields[modOpportunity.strfOPP_PRODUCT_ID].Value = rstNewContractOppProd.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value;
                            rstChangeOrder.Fields[modOpportunity.strfPREVIOUS_OPTION_ID].Value = rstContractOppProd.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value;
                            rstChangeOrder.Fields[modOpportunity.strfPREVIOUS_PRICE].Value = rstContractOppProd.Fields[modOpportunity.strfPRICE].Value;
                            rstChangeOrder.Fields[modOpportunity.strfPREVIOUS_QUANTITY].Value = rstContractOppProd.Fields[modOpportunity.strfQUANTITY].Value;
                            // previous extended price calculated with TLF
                            objLib.SaveRecordset(modOpportunity.strtCHANGE_ORDER_OPTIONS, rstChangeOrder);
                        }

                        // Jul 18, 2005. By JWang
                        // A elevation is changed, set the Contract's chosen elevation.
                        if (strOptionType == modOpportunity.strsELEVATION)
                        {
                            objOpportunity.UpdateQuoteChosenElevation(contractId, vntNHHDP_ProductId);
                        }
                        break;
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will add Change Orders Adjustment records
        /// </summary>
        /// <param name="opportunityAdjustmentId">Opportunity Adjustment Id</param>
        /// <param name="changeOrderId">the change order Id</param>
        /// <param name="changeOrderStatus">the opportunity Id</param>
        /// <returns>Change Order Adjustment Id</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual object AddChangeOrderAdjustments(object opportunityAdjustmentId, object changeOrderId, 
            ChangeOrderStatus changeOrderStatus)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                // source adjustment
                Recordset rstOppAdjustment = objLib.GetRecordset(opportunityAdjustmentId, modOpportunity.strtOPPORTUNITY_ADJUSTMENT,
                    modOpportunity.strfADJUSTMENT_AMOUNT, modOpportunity.strfADJUSTMENT_TYPE, modOpportunity.strfADJUSTMENT_TYPE,
                    modOpportunity.strfNOTES, modOpportunity.strfRELEASE_ADJUSTMENT_ID, modOpportunity.strfAPPLY_TO);

                if (rstOppAdjustment.RecordCount > 0)
                {
                    rstOppAdjustment.MoveFirst();
                    Recordset rstCOAdjustment = objLib.GetNewRecordset(modOpportunity.strtCHANGE_ORDER_ADJUSTMENT);
                    rstCOAdjustment.AddNew(Type.Missing, Type.Missing);
                    foreach (Field objField in rstOppAdjustment.Fields)
                    {
                        if ((objField.Name.Substring(0, 3).ToUpper() != "RN_") && (objField.Name != modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID))
                        {
                            rstCOAdjustment.Fields[objField.Name].Value = rstOppAdjustment.Fields[objField.Name].Value;
                        }
                    }
                    rstCOAdjustment.Fields[modOpportunity.strfCHANGE_ORDER_ID].Value = changeOrderId;
                    rstCOAdjustment.Fields[modOpportunity.strfCHANGE_ORDER_STATUS].Value = changeOrderStatus;

                    objLib.SaveRecordset(modOpportunity.strtCHANGE_ORDER_ADJUSTMENT, rstCOAdjustment);
                    return rstCOAdjustment.Fields[modOpportunity.strfCHANGE_ORDER_ADJUSTMENT_ID].Value;
                }
                else
                    return DBNull.Value;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// this function will add a psq adjustment to the original opp
        /// </summary>
        /// <param name="opportunityId">the Opporuntiy Id to add the adjustment to</param>
        /// <param name="opportunityAdjustmentId">the psq adjustment id</param>
        /// <returns>
        /// the new adjustment id</returns>
        /// <history>
        /// Revision       Date           Author      Description
        /// 3.8.0.0        5/12/2006      DYin        Converted to .Net C# code.
        /// </history>
        protected virtual object AddPostSaleQuoteAdjustmentToOriginalOpportunity(object opportunityId, object opportunityAdjustmentId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                string[] arrFields = objLib.GetAllFieldNames(modOpportunity.strtOPPORTUNITY_ADJUSTMENT, modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID);

                Recordset rstOriginal = objLib.GetRecordset(opportunityAdjustmentId, modOpportunity.strtOPPORTUNITY_ADJUSTMENT, arrFields);
                Recordset rstNew = objLib.GetNewRecordset(modOpportunity.strtOPPORTUNITY_ADJUSTMENT, arrFields);

                if (rstOriginal.RecordCount > 0)
                {
                    rstNew.AddNew(Type.Missing, Type.Missing);

                    for(int i = 0; i < arrFields.Length; ++i)
                    {
                        switch (arrFields[i])
                        {
                            case modOpportunity.strfOPPORTUNITY_ID:
                                rstNew.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = opportunityId;
                                break;
                            case modOpportunity.strfSELECTED:
                                rstNew.Fields[modOpportunity.strfSELECTED].Value = true;
                                break;
                            case modOpportunity.strfNET_CONFIG:
                                rstNew.Fields[modOpportunity.strfNET_CONFIG].Value = false;
                                break;
                            case modOpportunity.strfCOPY_OF_ADJUSTMENT_ID:
                                rstNew.Fields[modOpportunity.strfCOPY_OF_ADJUSTMENT_ID].Value = System.DBNull.Value;
                                break;
                            default:
                                rstNew.Fields[arrFields[i]].Value = rstOriginal.Fields[arrFields[i]].Value;
                                break;
                        }
                    }
                }
                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY_ADJUSTMENT, rstNew);
                return rstNew.Fields[modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID].Value;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Determine if an adjustment on a Post Sale Quote has been modified compared to the origional contract
        /// </summary>
        /// <param name="contractAdjustmentId">Contract adjustment Id</param>
        /// <param name="postSaleQuoteAdjustmentId">Post Sale Quote adjustment Id</param>
        /// <returns>Boolean - True if modified; false otherwise</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual bool IsAdjustmentModified(object contractAdjustmentId, object postSaleQuoteAdjustmentId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstContractAdjustment = objLib.GetRecordset(contractAdjustmentId, modOpportunity.strtOPPORTUNITY_ADJUSTMENT);
                Recordset rstPSQAdjustment = objLib.GetRecordset(postSaleQuoteAdjustmentId, modOpportunity.strtOPPORTUNITY_ADJUSTMENT);

                // 1) compare the option records
                string[] arrFields = objLib.GetAllFieldNames(modOpportunity.strtOPPORTUNITY_ADJUSTMENT, modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID);

                for(int i = 0; i < arrFields.Length; ++i)
                {
                    // Filter out fields we do not want to check on...
                    if ((arrFields[i].Substring(0, 3).ToUpper() != "RN_") && 
                        (arrFields[i] != modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID) && 
                        (arrFields[i] != modOpportunity.strfCOPY_OF_ADJUSTMENT_ID) && 
                        (arrFields[i] != modOpportunity.strfOPPORTUNITY_ID))
                    {
                        if (!Share.EqualValues(rstContractAdjustment.Fields[arrFields[i]].Value, rstPSQAdjustment.Fields[arrFields[i]].Value))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will add Change Orders records
        /// </summary>
        /// <param name="opportunityProductId">Opportunity ProductId</param>
        /// <param name="changeOrderId">Change order Id</param>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="changeOrderStatus">boolean value</param>
        /// <returns>
        /// True or False</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        public virtual object AddCustomChangeOrderOption(object opportunityProductId, object changeOrderId, object
            opportunityId, ChangeOrderStatus changeOrderStatus)
        {
            try
            {
                if (opportunityProductId != DBNull.Value)
                {
                    // add the nbhdproduct to the change order
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                    Recordset rstOppProduct = objLib.GetRecordset(opportunityProductId, modOpportunity.strt_OPPORTUNITY__PRODUCT,
                            modOpportunity.strfBUILT_OPTION, modOpportunity.strfCODE_, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                            modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL, modOpportunity.strfCUSTOMERINSTRUCTIONS,
                            modOpportunity.strfDELTA_BUILT_OPTION, modOpportunity.strfDEPOSIT, modOpportunity.strfDIVISION_PRODUCT_ID,
                            modOpportunity.strfEXTENDED_PRICE, modOpportunity.strfFILTER_VISIBILITY, modOpportunity.strf_NBHDP_PRODUCT_ID,
                            modOpportunity.strfNET_CONFIG, modOpportunity.strfOPP_CURRENCY, modOpportunity.strfOPPORTUNITY_ID,
                            modOpportunity.strf_OPPORTUNITY__PRODUCT_ID, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID,
                            modOpportunity.strfOPTIONNOTES, modOpportunity.strfPREFERENCE, modOpportunity.strfPREFERENCES_LIST,
                            modOpportunity.strfPRICE, modOpportunity.strfPRODUCT_AVAILABLE, modOpportunity.strfPRODUCT_ID,
                            modOpportunity.strfPRODUCT_NAME, modOpportunity.strfQUANTITY, modOpportunity.strfQUOTED_PRICE,
                            modOpportunity.strfSELECTED, modOpportunity.strfTICKLE_COUNTER, modOpportunity.strfTYPE);

                    if (rstOppProduct.RecordCount > 0)
                    {
                        Recordset rstChangeOrder = objLib.GetNewRecordset(modOpportunity.strtCHANGE_ORDER_OPTIONS, modOpportunity.strfBUILT_OPTION,
                            modOpportunity.strfCHANGE_ORDER_ID, modOpportunity.strfCHANGE_ORDER_OPTIONS_ID, modOpportunity.strfCHANGE_ORDER_STATUS,
                            modOpportunity.strfCODE_, modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL,
                            modOpportunity.strfCUSTOMERINSTRUCTIONS, modOpportunity.strfDELTA_BUILT_OPTION, modOpportunity.strfDEPOSIT,
                            modOpportunity.strfDIVISION_PRODUCT_ID, modOpportunity.strfEXTENDED_PRICE, modOpportunity.strfFILTER_VISIBILITY,
                            modOpportunity.strf_NBHDP_PRODUCT_ID, modOpportunity.strfNET_CONFIG, modOpportunity.strfOPP_CURRENCY,
                            modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strf_OPPORTUNITY_PRODUCT_ID, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID,
                            modOpportunity.strfOPTIONNOTES, modOpportunity.strfPREFERENCE, modOpportunity.strfPREFERENCES_LIST,
                            modOpportunity.strfPRICE, modOpportunity.strfPRODUCT_AVAILABLE, modOpportunity.strfPRODUCT_ID,
                            modOpportunity.strfPRODUCT_NAME, modOpportunity.strfQUANTITY, modOpportunity.strfQUOTED_PRICE,
                            modOpportunity.strfSELECTED, modOpportunity.strfTICKLE_COUNTER, modOpportunity.strfTYPE);
                        rstChangeOrder.AddNew(Type.Missing, Type.Missing);
                        foreach (Field objField in rstOppProduct.Fields)
                        {
                            if (objField.Name == modOpportunity.strf_OPPORTUNITY__PRODUCT_ID)
                            {
                                rstChangeOrder.Fields[modOpportunity.strf_OPPORTUNITY_PRODUCT_ID].Value = objField.Value;
                            }
                            else
                            {
                                rstChangeOrder.Fields[objField.Name].Value = rstOppProduct.Fields[objField.Name].Value;
                            }
                        }
                        rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_ID].Value = changeOrderId;
                        rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_STATUS].Value = changeOrderStatus;
                        objLib.SaveRecordset(modOpportunity.strtCHANGE_ORDER_OPTIONS, rstChangeOrder);
                        return rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_OPTIONS_ID].Value;
                    }
                }
                return DBNull.Value;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sends out email notification to the employees where the change order notification is set to true
        /// </summary>
        /// <returns>Boolean - True if email was sent out successfully; false otherwise</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual bool SendEmailNotification(object opportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Opportunity objOpportunity = (Opportunity) RSysSystem.ServerScripts[modOpportunity.strsOPPORTUNITY].CreateInstance();

                 Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strfCONTACT_ID,
                    modOpportunity.strfNEIGHBORHOOD_ID, modOpportunity.strf_ACCOUNT_MANAGER_ID, modOpportunity.strfPLAN_NAME_ID,
                    modOpportunity.strfELEVATION_ID, modOpportunity.strfQUOTE_TOTAL, modOpportunity.strfECOE_DATE, modOpportunity.strfLOT_ID);
                object vntNeighborhoodId = rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                object vntLotId = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                object vntContactId = rstOpportunity.Fields[modOpportunity.strfCONTACT_ID].Value;
                // send email
                // get recepient list from neighborhood notifcation team where notify on Sales Approved is true
                Recordset rstEmailTo = objLib.GetRecordset("HB: Notification of Change Order Creation", 1, vntNeighborhoodId, modOpportunity.strf_EMPLOYEE_ID);
                string strEmailTo = string.Empty;
                if (rstEmailTo.RecordCount > 0)
                {
                    rstEmailTo.MoveFirst();
                    StringBuilder emailToBuilder = new StringBuilder();
                    while(!(rstEmailTo.EOF))
                    {
                        string strWorkEmail = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtEMPLOYEE, modOpportunity.strf_WORK_EMAIL, rstEmailTo.Fields[modOpportunity.strf_EMPLOYEE_ID].Value));
                        // add if not already there
                        if (!emailToBuilder.ToString().Contains(strWorkEmail))
                        {
                            emailToBuilder.Append(strWorkEmail);
                            emailToBuilder.Append(";");
                        }
                        rstEmailTo.MoveNext();
                    }
                    // strip out last ;
                    strEmailTo = emailToBuilder.ToString();
                    strEmailTo = strEmailTo.Substring(0, strEmailTo.Length - 1);
                }
                rstEmailTo.Close();

                // all language strings are in nbhd_notification_team
                ILangDict lngNBHD_Notification_Team = RSysSystem.GetLDGroup(modOpportunity.strgNBHD_NOTIFICATION_TEAM);
                // set subject
                object vntSalesRepId = rstOpportunity.Fields[modOpportunity.strf_ACCOUNT_MANAGER_ID].Value;
                string vntSalesRepFirstName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_FIRST_NAME,
                    vntSalesRepId));
                string vntSalesRepLastName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_LAST_NAME,
                    vntSalesRepId));
                string strLotDescriptor = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_PRODUCT, modOpportunity.strfRN_DESCRIPTOR,
                    vntLotId));
                int vntJob_Number = TypeConvert.ToInt32(objLib.SqlIndex(modOpportunity.strt_PRODUCT, modOpportunity.strfJOB_NUMBER,
                    vntLotId));

                string strSubject = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdCHANGE_ORDER_CREATION_SUBJECT,
                    new object[] { vntSalesRepFirstName, vntSalesRepLastName, strLotDescriptor, 
                        String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)) }));
                // set message
                string strMessage = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdCHANGE_ORDER_CREATION_MESSAGE1, 
                    new object[] {DateTime.Today, vntSalesRepFirstName, vntSalesRepLastName, 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_FIRST_NAME, vntContactId)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_LAST_NAME, vntContactId)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strfHOME_PHONE, vntContactId)), 
                        String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)),
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value)) }));

                // Jul 27, 2005. By JWang. Get ECOE_Date from the original quote.
                object vntOrgOpp_Id = RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[modOpportunity.strfPOST_SALE_ID].Index(opportunityId);
                strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdCHANGE_ORDER_CREATION_MESSAGE2, 
                    new object[] { vntJob_Number, TypeConvert.ToString(RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[modOpportunity.strfECOE_DATE].Index(vntOrgOpp_Id))}));
                // "" & rstOpportunity.Fields(strfECOE_DATE).Value))
                strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdCHANGE_ORDER_CREATION_MESSAGE3, 
                    new object[] {vntSalesRepFirstName, vntSalesRepLastName, 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_WORK_EMAIL, vntSalesRepId)),
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strfWORK_PHONE, vntSalesRepId))}));
                if (strEmailTo.Length > 0 && strSubject.Length > 0 && strMessage.Length > 0)
                {
                    IRSend objEmail = RSysSystem.CreateEmail();
                    objEmail.NewMessage();
                    objEmail.To = strEmailTo;
                    objEmail.Subject = TypeConvert.ToString(strSubject);
                    objEmail.Body = TypeConvert.ToString(strMessage);
                    objEmail.Send();
                }
                return false;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Check if there is a built option change in the Post Sale Quote
        /// </summary>
        /// <param name="postSaleQuoteId">Post Sale Quote Id</param>
        /// <returns>string - "" if no built option changed (added, removed, edited)
        ///                 - specifiy which built option changed
        /// </returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// HB5.9          4/25/2007      JWang     Initial version
        /// 5.9.0.0        5/22/2007      ML        options using Post Cut-Off Price should not be considered as built
        ///                                         unless it is explicitly set to built
        /// HB5.9          7/16/2007      JWang     Check the counterpart option in original contract to see if it is Built.
        /// </history>
        protected virtual string IsThereBuiltOptionChange(object postSaleQuoteId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                object lotID = RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[modOpportunity.strfLOT_ID].Index(postSaleQuoteId);
                object lotConstructionStageID = RSysSystem.Tables[modOpportunity.strt_PRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(lotID);
                int lotStageOrdinal = TypeConvert.ToInt32(RSysSystem.Tables[modOpportunity.strtCONSTRUCTION_STAGE].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORD].Index(lotConstructionStageID));
                string infoMessage = TypeConvert.ToString(RldtLangDict.GetText(modOpportunity.strdBUILT_OPTION_CHANGE_ALERT));
                Recordset postSaleQuoteOptionRecordset;
                bool optionBuilt;

                // 1) for added/removed OptionSelectionSource, directly check if it is built Or not.
                postSaleQuoteOptionRecordset = objLib.GetRecordset(modOpportunity.strqOP_PRODS_FOR_OPP_DELETED_OR_ADDED, 1, postSaleQuoteId
                    , modOpportunity.strfOPPORTUNITY__PRODUCT_ID, modOpportunity.strfBUILT_OPTION, modOpportunity.strfUSE_POST_CUTOFF_PRICE
                    , modOpportunity.strfPARENT_PACK_OPPPROD_ID, modOpportunity.strfDIVISION_PRODUCT_ID);

                if (postSaleQuoteOptionRecordset.RecordCount > 0)
                {
                    postSaleQuoteOptionRecordset.MoveFirst();
                    while (!(postSaleQuoteOptionRecordset.EOF))
                    {
                        if (TypeConvert.ToBoolean(postSaleQuoteOptionRecordset.Fields[modOpportunity.strfBUILT_OPTION].Value))
                            return infoMessage;

                        //TransitionPointParameter transitParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                        //transitParams.Construct();
                        //transitParams.SetUserDefinedParameter(1, postSaleQuoteOptionRecordset.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value);
                        //transitParams.SetUserDefinedParameter(2, lotStageOrdinal);

                        //object parameterList = transitParams.ParameterList;

                        //RSysSystem.Forms[modOpportunity.strrHB_SALE].Execute(modOpportunity.strmOPTION_AM_I_BUILT, ref parameterList);            

                        //transitParams.GetUserDefinedParameterArray(parameterList);
                        //bool optionBuilt = TypeConvert.ToBoolean(transitParams.GetUserDefinedParameter(1));
                        ////ML 05-22-2007 as options using Post Cut-Off Price should not be considered as built
                        ////06-28-07 also package components (options which have value for Parent_Package_OppProd_Id)
                        //if (TypeConvert.ToBoolean(postSaleQuoteOptionRecordset.Fields[modOpportunity.strfUSE_POST_CUTOFF_PRICE].Value)||(postSaleQuoteOptionRecordset.Fields[modOpportunity.strfPARENT_PACK_OPPPROD_ID].Value != DBNull.Value))
                        //    optionBuilt = false;

                        //Check if there is a counterpart of this option in the original contract, 
                        //and the counterpart option in contract is set to Built.
                        object contractId=RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfPOST_SALE_ID].Index(postSaleQuoteId);
                        object divProductId=postSaleQuoteOptionRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;
                        Recordset couterpartOptionRecordset = objLib.GetRecordset(modOpportunity.strqSELECTED_OPTIONS_OPP_DIV_PROD
                            , 2, contractId, divProductId
                            , modOpportunity.strfBUILT_OPTION);

                        optionBuilt = couterpartOptionRecordset.RecordCount > 0 ? TypeConvert.ToBoolean(couterpartOptionRecordset.Fields[modOpportunity.strfBUILT_OPTION].Value) : false;

                        if (optionBuilt)
                            return infoMessage;
                        postSaleQuoteOptionRecordset.MoveNext();
                    }
                }
                postSaleQuoteOptionRecordset.Close();


                // 2) for other options, need To call IsOptionModified, and if it is modified, then check if it is built or not.
                postSaleQuoteOptionRecordset = objLib.GetRecordset(modOpportunity.strqOP_PRODS_FOR_OPP_ORG_SELECTED, 1, postSaleQuoteId);

                if (postSaleQuoteOptionRecordset.RecordCount > 0)
                {
                    postSaleQuoteOptionRecordset.MoveFirst();
                    while (!(postSaleQuoteOptionRecordset.EOF))
                    {
                        Recordset rstContractOptions = objLib.GetRecordset(postSaleQuoteOptionRecordset.Fields[modOpportunity.strfORIG_OPP_PROD_ID].Value,
                            modOpportunity.strtOPPORTUNITY__PRODUCT);
                        bool blnQuantityModified = false;
                        bool bOptionModified = IsOptionModified(rstContractOptions, postSaleQuoteOptionRecordset, out blnQuantityModified);
                        if (bOptionModified)
                        {
                            if (TypeConvert.ToBoolean(postSaleQuoteOptionRecordset.Fields[modOpportunity.strfBUILT_OPTION].Value))
                                return infoMessage;

                            //TransitionPointParameter transitParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                            //transitParams.Construct();
                            //transitParams.SetUserDefinedParameter(1, postSaleQuoteOptionRecordset.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value);
                            //transitParams.SetUserDefinedParameter(2, lotStageOrdinal);

                            //object parameterList = transitParams.ParameterList;

                            //RSysSystem.Forms[modOpportunity.strrHB_SALE].Execute(modOpportunity.strmOPTION_AM_I_BUILT, ref parameterList);

                            //transitParams.GetUserDefinedParameterArray(parameterList);
                            //bool optionBuilt = TypeConvert.ToBoolean(transitParams.GetUserDefinedParameter(1));

                            //Check if there is a counterpart of this option in the original contract, 
                            //and the counterpart option in contract is set to Built.
                            object contractId = RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfPOST_SALE_ID].Index(postSaleQuoteId);
                            object divProductId = postSaleQuoteOptionRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;
                            Recordset couterpartOptionRecordset = objLib.GetRecordset(modOpportunity.strqSELECTED_OPTIONS_OPP_DIV_PROD
                                , 2, contractId, divProductId
                                , modOpportunity.strfBUILT_OPTION);

                            optionBuilt = couterpartOptionRecordset.RecordCount > 0 ? TypeConvert.ToBoolean(couterpartOptionRecordset.Fields[modOpportunity.strfBUILT_OPTION].Value) : false;

                            if (optionBuilt)
                                return infoMessage;
                        }
                        postSaleQuoteOptionRecordset.MoveNext();
                    }
                }
                postSaleQuoteOptionRecordset.Close();

                return "";
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
    }
}
