using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;

using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;

using CRM.Pivotal.IP;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    #region Public enumerators
    /// <summary>
    /// Options for Change Order Status.
    /// </summary>
    public enum ChangeOrderStatus
    {
        /// <summary></summary>
        Selected = 0,
        /// <summary></summary>
        Unselected = 1,
        /// <summary></summary>
        Changed = 2
    }

    /// <summary>
    /// Unit of Measure
    /// </summary>
    public enum UnitOfMeasure
    {
        /// <summary></summary>
        Each = 0,
        /// <summary></summary>
        Square_Feet = 1,
        /// <summary></summary>
        Linear_Feet = 2,
        /// <summary></summary>
        Square_Yards = 3
    }


    /// <summary>
    /// Option Selection Source
    /// </summary>
    public enum OptionSelectionSource
    {
        /// <summary>
        /// Pivotal is the source of option seletion
        /// </summary>
        Pivotal=0,

        /// <summary>
        /// Envision is the source of option selection
        /// </summary>
        Envision=1
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ChangeOrderLogic
    {
        /// <summary></summary>
        Add = 0,
        /// <summary></summary>
        Remove = 1,
        /// <summary></summary>
        Change = 2
    }

    /// <summary>
    /// 
    /// </summary>
    public enum InactiveQuoteReason
    {
        /// <summary></summary>
        NoReason = -1,
        /// <summary></summary>
        PlanChange = 0,
        /// <summary></summary>
        OptionChange = 1,
        /// <summary></summary>
        ConvertToSale = 2,
        /// <summary></summary>
        NewInventoryQuote = 3,
        /// <summary></summary>
        PostBuildAccept = 4
    }

    /// <summary>
    /// 
    /// </summary>
    public enum StandardOptionPricing
    {
        /// <summary></summary>
        Fixed = 0,
        /// <summary></summary>
        Floating = 1
    }
    #endregion

    /// <summary>
    /// This module provides all the business rules for the Opportunity object.
    /// This object is used to record the pipeline stage and status of the opportunity,
    /// create a new activity for this opportunity, create an alert to notify other staff of
    /// important information about this opportunity, launch a sales plan, etc.
    /// </summary>
    /// <history>
    /// Revision #   Date          Author   Description
    /// 3.8.0.0  5/12/2006  DYin   Converted to .Net C# code.
    /// </history>
    public class Opportunity : IRFormScript
    {
        #region Private fields
        private ILangDict m_rdaLangDict;

        /// <summary>
        /// Language Dictionary
        /// </summary>
        protected ILangDict LangDict
        {
            get { return m_rdaLangDict; }
            set { m_rdaLangDict = value; }
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
        #endregion

        #region Private Structures
        /// <summary>
        /// Structure used for Standard Options in GetStandardWithRules method
        /// </summary>
        private struct structOption
            {
                public int intSelected;
                public string strDivProductId;
                public string strProductConfigId;
                public string strDependency;
                public int intPriority;
            }
        #endregion


        #region IRFormScript interface methods
        /// <summary>
        /// This function Saves new opportunity form data to the database.
        /// </summary>
        /// <param name="pForm">IRForm reference to the current form</param>
        /// <param name="Recordsets">Variant array of recordsets of Opportunity form data</param>
        /// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer
        /// User Def 1 - State of reset team member
        /// User Def 2 - State of apply milestone
        /// User Def 3 - State of quota period
        /// </param>
        /// <returns>Opportunity Id</returns>
        /// <history>
        /// Revision #   Date           Author      Description
        /// 3.8.0.0      5/12/2006      DYin        Converted to .Net C# code.
        /// 5.9.0.0     July/04/2007    YK          Bypassing the Plan_Has_Standard flag.
        /// </history>
        public virtual object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                object[] recordsetArray = (object[])Recordsets;
                Recordset rstAddOpportunity = (Recordset)recordsetArray[0];

                // Set Plan Price, Lot Premium and Realtor Id when record is saved
                object vntLot_Id = rstAddOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                object vntPlan_Name_Id = rstAddOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value;
                // Check to see if Plan_Id of form is not NULL
                if (vntPlan_Name_Id != DBNull.Value)
                {
                    decimal homesitePremium = 0;
                    rstAddOpportunity.Fields[modOpportunity.strfPRICE].Value = GetQuotePlanPrice(rstAddOpportunity, out homesitePremium);
                    if ((vntLot_Id != DBNull.Value))
                    {
                        rstAddOpportunity.Fields[modOpportunity.strfLOT_PREMIUM].Value = homesitePremium;
                    }
                }

                object vntOpportunityId = pForm.DoAddFormData(Recordsets, ref ParameterList);
                //If contract On Hold set homesite as Sold.
                if (TypeConvert.ToString(rstAddOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value) == modOpportunity.strsCONTRACT
                    && TypeConvert.ToString(rstAddOpportunity.Fields[modOpportunity.strfSTATUS].Value) == modOpportunity.strsON_HOLD)
                {
                    UpdateLotStatusEx(vntLot_Id, modOpportunity.strsSOLD);
                }

                // Copy all Agreements from NBHD
                object vntReleaseId = rstAddOpportunity.Fields[modOpportunity.strf_NBHD_PHASE_ID].Value;
                vntLot_Id = rstAddOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                // copy routine commented out till Division.Agreement_Id problem is resolved
                // CopyNBHDAgreementToOppAgreement vntOpportunityId, vntReleaseId
                // if the pipeline stage is closed, then set all other quotes for this lot to not pursued
                // TODO (DiYin) Use Language String to replace the hard-typing string.
                if (TypeConvert.ToString(rstAddOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value) == modOpportunity.strsCLOSED)
                {
                    vntLot_Id = rstAddOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                    SetQuotesToNotPursued(vntLot_Id, vntOpportunityId);
                }

                // If the lot selected is an inventory lot then set blnInvLot = true so that inv quote options will
                // get copied instead.
                bool blnInvLot = (TypeConvert.ToString(RSysSystem.Tables[modOpportunity.strt_PRODUCT].Fields[modOpportunity.strfTYPE].Index(vntLot_Id))
                    == modOpportunity.strsINVENTORY);

                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // processed related to addition of Inventory Quote
                if (TypeConvert.ToString(rstAddOpportunity.Fields[modOpportunity.strf_STATUS].Value) == modOpportunity.strsINVENTORY)
                {
                    // lot related changes
                    vntLot_Id = rstAddOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                    Recordset rstLot = objLib.GetRecordset(vntLot_Id, modOpportunity.strt_PRODUCT, modOpportunity.strfPLAN_ID,
                        modOpportunity.strfELEVATION_ID, modOpportunity.strfTYPE);
                    if (rstLot.RecordCount > 0)
                    {
                        // set lot plan_built if applicable
                        if (TypeConvert.ToDouble(rstAddOpportunity.Fields[modOpportunity.strfPLAN_BUILT].Value) == -1)
                        {
                            rstLot.Fields[modOpportunity.strfPLAN_ID].Value = rstAddOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value;
                        }
                        // Update the Type to "Inventory"
                        rstLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsINVENTORY;
                        objLib.PermissionIgnored = true;
                        objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);
                    }
                    // inactivate existing quotes
                    InactivateCustomerQuotes("" /* EMPTY */,  vntOpportunityId, InactiveQuoteReason.NewInventoryQuote);
                }

                // add standard options
                if (blnInvLot)
                {
                    AddInventoryQuoteOptions(vntOpportunityId, vntLot_Id);
                }
                else
                {
                    //YK - Bypassing the flag, rather checking for Stnadard everytime a Quote is created.
                    /* 
                    // check to see if plan has standard options;skip if it does not
                    bool blnPlanHasOptions = TypeConvert.ToBoolean(objLib.SqlIndex( modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfPLAN_HAS_STND_OPTIONS,
                        vntPlan_Name_Id));
                    if (blnPlanHasOptions)
                    {*/
                        CreateOpportunityProductStandard(vntReleaseId, rstAddOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value,
                            vntOpportunityId, vntPlan_Name_Id);
                    /* } */
                }

                // check to see if a convert to sale is needed:
                TransitionPointParameter objInstance = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objInstance.ParameterList = ParameterList;
                object[] parameterArray = objInstance.GetUserDefinedParameterArray();
                bool blnCreateContract = false;
                if (parameterArray.Length > 0)
                {
                     blnCreateContract = TypeConvert.ToBoolean(parameterArray[0]);
                }

                if (blnCreateContract)
                {
                    if (parameterArray.Length > 1)
                        parameterArray = ConvertToSale(vntOpportunityId, TypeConvert.ToBoolean(parameterArray[1]));
                    else
                        parameterArray = ConvertToSale(vntOpportunityId);
                    if (parameterArray[0].ToString().Length > 0)
                    {
                        objInstance.InfoMessage = parameterArray[0].ToString();
                        ParameterList = objInstance.ParameterList;
                        return new object[0];
                        //throw new PivotalApplicationException(parameterArray[0].ToString(), modOpportunity.glngERR_SAVEFORMDATA_FAILED);
                    }
                }


                // added by Carl Langan 01/05/05 for integration
                IRAppScript objIntegration = (IRAppScript) RSysSystem.ServerScripts[modOpportunity.strs_INTEGRATION].CreateInstance();
                object vntParam = objInstance.Construct();
                objIntegration.Execute(modOpportunity.strmIS_INTEGRATION_ON, ref vntParam);
                objInstance.ParameterList = vntParam;
                if (objInstance.UserDefinedParametersNumber > 0)
                {
                    if (TypeConvert.ToBoolean(objInstance.GetUserDefinedParameter(1)))
                    {
                        vntParam = objInstance.Construct();
                        objInstance.SetUserDefinedParameter(1,  rstAddOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value);
                        vntParam = objInstance.ParameterList;
                        objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref vntParam);
                    }
                }

                // Update quote status based on reservation expiration date
                UpdateQuoteStatus(vntOpportunityId);
                CalculateTotals(vntOpportunityId, false);

                // 2005/09/29 By JWang. When creating new contract from Quick Path, also need creating a new NBHD profile
                // if necesary.
                // May 26 By JWang. Create a new NBHD profile if necesary. only for customer quotes.
                if (TypeConvert.ToString(rstAddOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value) == modOpportunity.strPIPELINE_QUOTE
                    && (TypeConvert.ToString(rstAddOpportunity.Fields[modOpportunity.strf_STATUS].Value) == modOpportunity.strQUOTE_STATUS_IN_PROGRESS
                    || TypeConvert.ToString(rstAddOpportunity.Fields[modOpportunity.strfSTATUS].Value) == modOpportunity.strsRESERVED)
                    || TypeConvert.ToString(rstAddOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value) == modOpportunity.strPIPELINE_CONTRACT)
                {

                    ContactProfileNeighborhood objContactProfileNBHD = (ContactProfileNeighborhood)RSysSystem.ServerScripts[modOpportunity.strsCONTACT_PROFILE_NBHD].CreateInstance();
                    object vntSalesRep = rstAddOpportunity.Fields[modOpportunity.strf_ACCOUNT_MANAGER_ID].Value;
                    // Changed to call with vntSalesRep instead of currrent user - fpoulsen 06/21/2005
                    objContactProfileNBHD.NewNeighborhoodProfile(rstAddOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value,
                        rstAddOpportunity.Fields[modOpportunity.strf_CONTACT_ID].Value, DBNull.Value, new object[] { vntSalesRep });

                    // Add Sales Rep to Quote Sales Team
                    UpdateOpportunitySalesTeam(vntSalesRep, RSysSystem.Tables[modOpportunity.strtEMPLOYEE].Fields[modOpportunity.strf_ROLE_ID].Index(vntSalesRep),
                        vntOpportunityId, true);

                    // update the quote create date
                    UpdateContactProfileNeighborhood(rstAddOpportunity.Fields[modOpportunity.strfCONTACT_ID].Value, 
                        rstAddOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value, null,  null, 
                        null, null, null, null, null, null, null, null,
                        TypeConvert.ToDateTime(rstAddOpportunity.Fields[modOpportunity.strfQUOTE_CREATE_DATE].Value));

                    // Add current user, if not the same as Sales Rep, and not an Admin
                    Administration administration = (Administration) RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                    object vntCurrentEmployee = administration.CurrentUserRecordId;
                    if (!(RSysSystem.EqualIds(vntSalesRep, vntCurrentEmployee)) && !(RSysSystem.UserInGroup(RSysSystem.CurrentUserId(),
                        RSysSystem.GetLDGroup(null).GetText(modOpportunity.strsHB_ADMIN))))
                    {
                        object vntContact_Id = rstAddOpportunity.Fields[modOpportunity.strf_CONTACT_ID].Value;
                        object vntNBHDId = rstAddOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                        Recordset rstNBHDP = objLib.GetRecordset(modOpportunity.strqCONTACT_PROFILE_NBHD_FOR_CONTACT, 2, vntContact_Id,
                            vntNBHDId, modOpportunity.strfCONTACT_PROFILE_NBHD_ID);
                        if (rstNBHDP.RecordCount == 1)
                        {
                            rstNBHDP.MoveFirst();
                            object vntNBHDPId = rstNBHDP.Fields[modOpportunity.strfCONTACT_PROFILE_NBHD_ID].Value;
                            objContactProfileNBHD.UpdateNBHDProfieTeam(vntNBHDPId, vntCurrentEmployee);
                        }

                        // Add current Employee to the Quote Sales Team
                        UpdateOpportunitySalesTeam(vntCurrentEmployee, RSysSystem.Tables[modOpportunity.strtEMPLOYEE].Fields[modOpportunity.strf_ROLE_ID].Index(vntCurrentEmployee),
                            vntOpportunityId, true);
                    }
                }
                return vntOpportunityId;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine takes the Id of an opportunity, deletes the records from its secondary tables and deletes 
        /// the opportunity.
        /// </summary>
        /// <param name="pForm">IRForm reference to the current form</param>
        /// <param name="RecordId">Record Id for Opportunity</param>
        /// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
        /// <history>
        /// Revision       Date           Author   Description
        /// 3.8.0.0        5/12/2006      DYin     Converted to .Net C# code.
        /// 5.9            5/21/2007      JH       This function is deprecated in 5.9.
        /// 5.9            5/28/2007      JWang    As per Amita, the code is resumed.
        /// </history>
        public virtual void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                TransitionPointParameter transitionPointParameter = (TransitionPointParameter)RSysSystem
                  .ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                transitionPointParameter.ParameterList = ParameterList;
                string infoMessage = string.Empty;
                if (CanBeDeleted(pForm.FormName, RecordId, out infoMessage))
                {
                    // May 28, Added by JWang
                    // Update homesite type back to "Homesite" for Inventory Quote only
                    if (pForm.FormName == modOpportunity.strrINVENTORY_QUOTE)
                    {
                        UpdateInventoryHomesiteType(RecordId, modOpportunity.strLOT_TYPE_HOMESITE);
                    }

                    this.OpportunityCascadeDelete(RecordId);
                    pForm.DoDeleteFormData(RecordId, ref ParameterList);
                }
                transitionPointParameter.InfoMessage = infoMessage;
                ParameterList = transitionPointParameter.ParameterList;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }

        }

        /// <summary>
        /// This subroutine executes a specified method.
        /// </summary>
        /// <param name="pForm">IRForm object references to the client IRForm object</param>
        /// <param name="MethodName">Method name to be executed</param>
        /// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
        /// <history>
        /// Reversion#    Date        Author  Description
        /// HB 3.6        10/14/2005  TL       Added GetOptionPrice Call
        /// </history>
        public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {
            try
            {
                TransitionPointParameter objInstance = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objInstance.ParameterList = ParameterList;
                object[] parameterArray = objInstance.GetUserDefinedParameterArray();

                switch (MethodName)
                {
                    case modOpportunity.strmINVENTORY_MANAGEMENT_ALLOWED_FOR_CURRENT_USER:
                        // Get InventoryManagementAllowedForCurrentUser.
                        parameterArray = new object[] { InventoryManagementAllowedForCurrentUser() };
                        break;
                    case modOpportunity.strmGET_CONSTRUCTION_STAGE_COMPARISON:
                        // Get system definition of Construction Stage Comparison.
                        parameterArray = new object[] { GetConstructionStageComparison() };
                        break;
                    case modOpportunity.strmUPDATE_HOMESITE_PLAN:
                        // Update Homesite's Built Plan field with Plan of quote
                        UpdateHomesitePlan(parameterArray[0], parameterArray[1]);
                        break;
                    case modOpportunity.strmUPDATE_HOMESITE_BUILT_ELEVATION:
                        // Update Homesite's Built Elevation field with NBHDP_Product_Id of Opportunity Product
                        UpdateHomesiteBuiltElevation(parameterArray[0], parameterArray[1]);
                        break;
                    case modOpportunity.strmUPDATE_OPTION_BUILT_FOR_ACTIVE_CUSTOMER_QUOTE:
                        // Update option built for active customer quote
                        UpdateOptionBuiltForActiveCustomerQuote(parameterArray[0], parameterArray[1], TypeConvert.ToBoolean(parameterArray[2]));
                        break;
                    case modOpportunity.strmSALES_REQUEST_DECLINED:
                        SalesRequestDeclined(parameterArray[0]);
                        break;
                    case modOpportunity.strmCANCEL_REQUEST_DECLINED:
                        CancelRequestDeclined(parameterArray[0]);
                        break;
                    case modOpportunity.strmSALES_REQUEST:
                        SalesRequest(parameterArray[0]);
                        break;
                    case modOpportunity.strmGET_RESELECTED_OPTION_PRICE_AND_BUILT_INFO:
                        // Get ReSelected Option Price And Built Info
                        objInstance.CheckUserDefinedParameterNumber(1, true);
                        decimal optionPrice;
                        bool usePostCutoffPrice;
                        bool built;
                        GetReSelectedOptionPriceAndBuiltInfo(parameterArray[0], out optionPrice, out usePostCutoffPrice, out built);
                        parameterArray = new object[] { optionPrice, usePostCutoffPrice, built };
                        break;
                    case "CheckAdjustmentLimits":
                        decimal incentiveLimit;
                        bool blnLimitExceeded;
                        decimal adjustmentTotal;
                        CheckAdjustmentLimits(parameterArray[0], parameterArray[2], TypeConvert.ToDecimal(parameterArray[1]),
                            out blnLimitExceeded, out incentiveLimit, TypeConvert.ToBoolean(parameterArray[3]), out adjustmentTotal);
                        parameterArray = new object[] { blnLimitExceeded, TypeConvert.ToDecimal(incentiveLimit) };
                        break;

                    case "CheckForExistingAdjustment":
                        bool blnAdjustExists;
                        string dupAdjustmentName;
                        CheckForExistingAdjustment(parameterArray[0], parameterArray[1], TypeConvert.ToBoolean(parameterArray[2]), out blnAdjustExists, out dupAdjustmentName);
                        parameterArray = new object[] { blnAdjustExists, dupAdjustmentName };
                        break;
                    default:
                        // determine if integration has been turned on in system table
                        object integrationParameterList = null;
                        IRAppScript objIntegration = (IRAppScript) RSysSystem.ServerScripts[modOpportunity.strs_INTEGRATION].CreateInstance();
                        objIntegration.Execute("IsIntegrationOn", ref integrationParameterList);
                        TransitionPointParameter transitionPointParameter = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                        transitionPointParameter.ParameterList = integrationParameterList;
                        bool bolUseIntegration = false;
                        if (transitionPointParameter.UserDefinedParametersNumber > 0)
                        {
                            bolUseIntegration = TypeConvert.ToBoolean(transitionPointParameter.GetUserDefinedParameter(1));
                        }

                        // Added for integration by Carl Langan 01/05/05
                        // Rather than copy all the cases where the parameterArray is valid or not,
                        // we'll just copy the 1st and 2nd and have the decision login when/if to use it later

                        object vntSourceOpty = DBNull.Value;
                        if (parameterArray.Length > 0)
                            vntSourceOpty = parameterArray[0];

                        object vntTargetOpty = DBNull.Value;
                        if (parameterArray.Length > 1)
                            vntTargetOpty = parameterArray[1];

                        integrationParameterList = transitionPointParameter.Construct();
                        transitionPointParameter.SetUserDefinedParameter(1, vntSourceOpty);
                        integrationParameterList = transitionPointParameter.ParameterList;
                        switch (MethodName)
                        {
                            case modOpportunity.strmGET_LOTS_LIST_TRF:
                                objInstance.CheckUserDefinedParameterNumber(1);
                                // Get Lots
                                Recordset lotRecordset = GetTransferableLotList(parameterArray[0]);
                                parameterArray = new object[] {parameterArray[0], lotRecordset, RSysSystem.Tables[modOpportunity.strt_PRODUCT].TableId };
                                break;
                            case modOpportunity.strmGET_AVAILABLE_PLANS:
                                Recordset planRecordset = GetAvailablePlans(parameterArray[0], parameterArray[1], 
                                    parameterArray[2], parameterArray[3]);
                                parameterArray = new object[] { parameterArray[0], parameterArray[1],  parameterArray[2], planRecordset, RSysSystem.Tables[modOpportunity.strt_NBHD_PRODUCT].TableId };
                                break;
                            case modOpportunity.strmSELECT_UNSELECT_OPTIONS:
                                object vntReturn = DBNull.Value;

                                integrationParameterList = transitionPointParameter.Construct();
                                transitionPointParameter.SetUserDefinedParameter(1, parameterArray[1]);
                                integrationParameterList = transitionPointParameter.ParameterList;

                                if (parameterArray.Length > 4)
                                {
                                    vntReturn = SelectUnselectOptions(parameterArray[0], parameterArray[1], parameterArray[2], TypeConvert.ToBoolean(parameterArray[3]),
                                        parameterArray[4], 1, String.Empty, String.Empty);
                                }
                                else
                                {
                                    vntReturn = SelectUnselectOptions(parameterArray[0], parameterArray[1], parameterArray[2], TypeConvert.ToBoolean(parameterArray[3]),
                                        DBNull.Value, 1, String.Empty, String.Empty);
                                }
                                parameterArray = new object[] { vntReturn};
                                // integration
                                if (bolUseIntegration)
                                {
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                }
                                break;
                            case modOpportunity.strmSELECT_MULTPLE_OPTIONS:
                                string multpleOptionMessage = String.Empty;

                                integrationParameterList = transitionPointParameter.Construct();
                                transitionPointParameter.SetUserDefinedParameter(1, parameterArray[1]);
                                integrationParameterList = transitionPointParameter.ParameterList;

                                if (parameterArray.Length > 2)
                                {
                                    multpleOptionMessage = SelectMultipleOptions((Recordset)parameterArray[0], parameterArray[1], parameterArray[2]);
                                }
                                else
                                {
                                    multpleOptionMessage = SelectMultipleOptions((Recordset)parameterArray[0], parameterArray[1]);
                                }
                                parameterArray = new object[] { multpleOptionMessage };
                                // integration
                                if (bolUseIntegration)
                                {
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                }
                                break;
                            case modOpportunity.strmSAVE_OPTIONS:
                                UpdateOptions(parameterArray[0]);
                                // integration
                                if (bolUseIntegration)
                                {
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                }
                                break;
                            case modOpportunity.strmCONVERT_TO_SALE:
                                if (parameterArray.Length > 1)
                                {
                                    parameterArray = ConvertToSale(parameterArray[0], TypeConvert.ToBoolean(parameterArray[1])) ;
                                }
                                else
                                {
                                    parameterArray = ConvertToSale(parameterArray[0]);
                                }
                                // integration
                                if (bolUseIntegration)
                                {
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                }
                                break;
                            case modOpportunity.strmCOPY_QUOTE:
                                object quoteId = DBNull.Value;
                                if (parameterArray.Length > 2)
                                {
                                    quoteId = CopyQuote(parameterArray[0], TypeConvert.ToBoolean(parameterArray[1]),
                                        TypeConvert.ToBoolean(parameterArray[2]), false);
                                }
                                else
                                {
                                    quoteId = CopyQuote(parameterArray[0], TypeConvert.ToBoolean(parameterArray[1]),
                                        true, false);
                                }
                                parameterArray = new object[] { quoteId };

                                // integration
                                if (bolUseIntegration)
                                {
                                    // vntTempParameterList = objInstance.GetUserDefinedParameterArray();
                                    integrationParameterList = objInstance.ParameterList;
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                }
                                break;
                            case modOpportunity.strmCANCEL_CONTRACT:
                                bool contractCanceled = CancelContract(parameterArray[0], TypeConvert.ToBoolean(parameterArray[1]));
                                parameterArray = new object[] { contractCanceled };
                                // integration
                                if (bolUseIntegration)
                                {
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                }
                                break;
                            case modOpportunity.strmUPDATE_OPTION_FILTER:
                                UpdateOptionFilter(parameterArray[0], parameterArray[1], parameterArray[2], parameterArray[3],
                                    TypeConvert.ToString(parameterArray[4]), TypeConvert.ToBoolean(parameterArray[5]), TypeConvert.ToString(parameterArray[6]),
                                    TypeConvert.ToInt32(parameterArray[7]), parameterArray[8]);
                                parameterArray = new object[] { string.Empty};
                                break;
                            case modOpportunity.strmCHECK_PLAN:
                                parameterArray = new object[] {CheckPlan(parameterArray[0], parameterArray[1])};
                                break;
                            case modOpportunity.strmBATCH_UPDATE_QUOTE_EXPIRY:
                                BatchUpdateQuoteExpiry();
                                break;
                            case modOpportunity.strmUPDATE_QUOTE_ON_PLAN_CHNG:
                                if (parameterArray.GetUpperBound(0) >= 5)
                                {
                                    UpdateQuoteOnPlanChange(parameterArray[0], parameterArray[1], TypeConvert.ToBoolean(parameterArray[2]),
                                        parameterArray[3], parameterArray[4], parameterArray[5]);
                                }
                                else
                                {
                                    UpdateQuoteOnPlanChange(parameterArray[0], parameterArray[1], TypeConvert.ToBoolean(parameterArray[2]),
                                        parameterArray[3], parameterArray[4], null);
                                }

                                // integration
                                if (bolUseIntegration)
                                {
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                }
                                break;
                            case modOpportunity.strmTRANSFER_CONTRACT:
                                parameterArray = new object[] {TransferContract(parameterArray[0], parameterArray[1], parameterArray[2],
                                    TypeConvert.ToBoolean(parameterArray[3]), TypeConvert.ToBoolean(parameterArray[4]))};

                                // integration
                                if (bolUseIntegration)
                                {
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                    integrationParameterList = objInstance.ParameterList;
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                }
                                break;
                            case modOpportunity.strmOPTION_AM_I_BUILT:
                                parameterArray = new object[] { OptionAmIBuilt(parameterArray[0], TypeConvert.ToInt32(parameterArray[1])) };
                                break;
                            case modOpportunity.strmADD_INV_QUOTE_OPTIONS:
                                AddInventoryQuoteOptions(parameterArray[0], parameterArray[1]);
                                // integration
                                if (bolUseIntegration)
                                {
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                }
                                break;
                            case modOpportunity.strmDELETE_TEAM:
                                DeleteTeam(parameterArray[0]);

                                // integration
                                if (bolUseIntegration)
                                {
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                }
                                break;
                            case modOpportunity.strmUPDATE_QUOTE_OPTIONS:
                                //ML- march/02/2007 as requirements changed for change of construction stage on homesite
                                if (parameterArray.GetUpperBound(0) > 1)
                                parameterArray = new object[] {UpdateQuoteOptions(parameterArray[0], parameterArray[1],TypeConvert.ToBoolean(parameterArray[2]))};
                                else
                                parameterArray = new object[] { UpdateQuoteOptions(parameterArray[0], parameterArray[1]) };
                                break;
                            case modOpportunity.strmUPDATE_QUOTE_OPTIONS_SINGLE_OPTION:
                                parameterArray = new object[] {UpdateQuoteOptionsSingleOption(parameterArray[0], parameterArray[1], TypeConvert.ToBoolean(parameterArray[2]))};
                                break;
                            case modOpportunity.strmCALCULATE_TOTALS:
                                CalculateTotals(parameterArray[0], TypeConvert.ToBoolean(parameterArray[1]));
                                break;
                            case modOpportunity.strmRESET_QUOTE:
                                ResetQuote(parameterArray[0], TypeConvert.ToString(parameterArray[1]));

                                // integration
                                if (bolUseIntegration)
                                {
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                }

                                // integration
                                if (bolUseIntegration)
                                {
                                    objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref integrationParameterList);
                                }
                                break;
                            case modOpportunity.strmINVENTORY_QUOTE_SEARCH:
                                // Apr. 18, 2005 - BH
                                Recordset inventoryQuoteRecordset = InventoryQuoteSearch(pForm.FormName, (Recordset)parameterArray[0]);
                                parameterArray = new object[] { RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].TableId, inventoryQuoteRecordset };
                                break;
                            case modOpportunity.strmUPDATE_RESERVATION_STATUS:
                                UpdateReservationStatus(parameterArray[0], parameterArray[1], TypeConvert.ToDateTime(parameterArray[2]),
                                    parameterArray[3], parameterArray[4]);
                                break;
                            case modOpportunity.strmBATCH_UPDATE_QUOTE_STATUS:
                                BatchUpdateQuoteStatus();
                                break;
                            case modOpportunity.strmUPDATE_QUOTE_STATUS:
                                UpdateQuoteStatus(parameterArray[0]);
                                break;
                            case modOpportunity.strmCAN_COPY_QUOTE:
                                // Apr. 26, 2005 - BH
                                parameterArray = new object[] { CanCopyQuote(TypeConvert.ToString(parameterArray[0]), 
                                    TypeConvert.ToString(parameterArray[1]), parameterArray[2], parameterArray[3]) };
                                break;
                            case modOpportunity.strmCHECK_LOT_AVAILABILITY:
                                parameterArray = new object[] { CheckLotAvailability(parameterArray[0])};
                                break;

                            case modOpportunity.strmAPPLY_DEPOSIT_SCHEDULE_TEMPLATES:
                                ApplyDepositScheduleTemplates(parameterArray[0], (Recordset) parameterArray[1]);
                                break;
                            case modOpportunity.strmCANCEL_REQUEST_CONTRACT:
                                CancelRequestOrContract(parameterArray[0], TypeConvert.ToBoolean(parameterArray[1]));
                                break;
                            case modOpportunity.strmGET_EMAIL_RECIPIENTS:
                                parameterArray = new object[] {GetEmailRecipients(parameterArray[0])};
                                break;
                            case modOpportunity.strmGET_EMAIL_TEXT:
                                parameterArray = GetEmailTextSubject(parameterArray[0], TypeConvert.ToString(parameterArray[1]));
                                break;
                            case modOpportunity.strmCHECK_HOMESITE:
                                parameterArray = new object[] {CheckHomesite(parameterArray[0])};
                                break;
                            case modOpportunity.strmGET_QUOTE_PLAN_PRICE:
                                // TODO (Di Yin) there is no client script call this method.
                                parameterArray = new object[] {GetQuotePlanPrice(parameterArray[0], parameterArray[1], parameterArray[2])};
                                break;
                            case modOpportunity.strmGET_INVENTORY_CHANGE_NOTE:
                                parameterArray = new object[] {GetInventoryChangeNote(TypeConvert.ToString(parameterArray[0]), (string[]) parameterArray[1],
                                    (string[]) parameterArray[2])};
                                break;
                            case modOpportunity.strmINACTIVATE_CUSTOMER_QUOTES:
                                InactivateCustomerQuotes(parameterArray[0], parameterArray[1], InactiveQuoteReason.NoReason);
                                break;
                            case modOpportunity.strmUPDATE_CUSTOMER_QUOTE_LOCATIONS:
                                UpdateCustomerQuoteLocations(parameterArray[0]);
                                break;
                            case modOpportunity.strmUPDATE_INVENTORY_HOMESITE_TYPE:
                                UpdateInventoryHomesiteType(parameterArray[0], TypeConvert.ToString(parameterArray[1]));
                                break;
                            case modOpportunity.strmGET_OPTION_PRICE:
                                if (parameterArray.GetUpperBound(0) >= 4)
                                {
                                    vntReturn = GetQuoteOptionPrice(parameterArray[0], parameterArray[1], parameterArray[2], parameterArray[3], Convert.ToBoolean(parameterArray[4]));
                                }
                                else
                                {
                                    vntReturn = GetQuoteOptionPrice(parameterArray[0], parameterArray[1], parameterArray[2], parameterArray[3]);
                                }
                                parameterArray = new object[] { vntReturn };
                                break;
                            case modOpportunity.strmLOAD_EXCLUDED_OPTIONS:
                                vntReturn = LoadExcludedProducts(parameterArray[0], parameterArray[1], parameterArray[2], parameterArray[3],
                                    (Recordset) parameterArray[4]);
                                parameterArray = new object[] { vntReturn };
                                break;
                            case modOpportunity.strmCREATE_OPPRODUCT_OPTION:
                                switch (parameterArray.Length)
                                {
                                    case 3:
                                        vntReturn = CreateOpportunityProductOption(parameterArray[0], parameterArray[1], (Recordset)parameterArray[2]);
                                        break;
                                    case 4:
                                        vntReturn = CreateOpportunityProductOption(parameterArray[0], parameterArray[1], (Recordset)parameterArray[2], TypeConvert.ToInt32(parameterArray[3]));
                                        break;
                                    case 6:
                                        object oppProdId;
                                        object oppProdLocId;
                                        vntReturn = CreateOpportunityProductOption(parameterArray[0], parameterArray[1], (Recordset)parameterArray[2], TypeConvert.ToInt32(parameterArray[3]), (OptionSelectionSource)parameterArray[4], TypeConvert.ToDateTime(parameterArray[5]), out oppProdId, out oppProdLocId);
                                        parameterArray = new object[] {oppProdId, oppProdLocId };
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case modOpportunity.strmIS_THERE_PSQ:
                                parameterArray = new object[] { IsTherePSQ(parameterArray[0]) };
                                break;
                            case modOpportunity.strmSINGLE_OPTION_VALIDATION:
                                parameterArray = new object[] { SingleOptionValidation(parameterArray[0]) };
                                break;
                            case modOpportunity.strmINVENTORY_HOME_OPTION_VALIDATION:
                                parameterArray = new object[] { InventoryHomeOptionValidation(parameterArray[0]) };
                                break;
                            case modOpportunity.strmOPTION_NEEDS_LOCATION:
                                parameterArray = new object[] { OptionNeedsLocation(parameterArray[0]) };
                                break;
                            case modOpportunity.strmOPTIONS_WITH_DUPLICATE_LOCATIONS:
                                parameterArray = new object[] { OptionsWithDuplicateLocations(parameterArray[0]) };
                                break;
                            case modOpportunity.strmGET_ESCROW:
                                object vntEscrowId = GetEscrow(parameterArray[0]);
                                parameterArray = new object[] { vntEscrowId };
                                break;
                            case modOpportunity.strmROLLBACKS:
                                bool blnRollbackOK = Rollbacks( parameterArray[0]);
                                parameterArray = new object[] { blnRollbackOK };
                                break;
                            case modOpportunity.strmRESERVATION_CANCELLATION:
                                bool blnReservationCancellationOK = ReservationCancellation(parameterArray[0]);
                                parameterArray = new object[] { blnReservationCancellationOK };
                                break;
                            case modOpportunity.strmTRANSFERS:
                                 bool blnTransferOK = Transfers(parameterArray[0]);
                                parameterArray = new object[] { blnTransferOK };
                                break;
                            case modOpportunity.strmESCROW_CONTRACT_CLOSE:
                                EscrowCloseContract(parameterArray[0], parameterArray[1]);
                                break;
                            default:
                                string message = MethodName + TypeConvert.ToString(LangDict.GetText(modOpportunity.strdINVALID_METHOD));
                                parameterArray = new object[] { message };
                                throw new PivotalApplicationException(message, modOpportunity.glngERR_METHOD_NOT_DEFINED);
                        }
                        break;
                }
                ParameterList = objInstance.SetUserDefinedParameterArray(parameterArray);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function loads opportunity form data from the database.
        /// </summary>
        /// <param name="pForm">IRForm reference to the current active form</param>
        /// <param name="RecordId">Record Id of opportunity</param>
        /// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer
        /// User Def 1 - Recordset for Alert</param>
        /// <returns>Variant array of recordsets of opportunity form data</returns>
        /// <history>
        /// Revision       Date           Author   Description
        /// 3.8.0.0        5/12/2006      DYin     Converted to .Net C# code.
        /// HB 3.6         09/09/2005     TL       Added required categories check and setting of the completeness flag
        /// </history>
        public virtual object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                object vntRecordset = pForm.DoLoadFormData(RecordId, ref ParameterList);
                object[] recordsetArray = (object[])vntRecordset;

                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;
                if (objParam.HasValidParameters() == false)
                {
                    objParam.Construct();
                }

                // set Config Complete flag
                Recordset rstOpportunity = (Recordset)recordsetArray[0];

                rstOpportunity.Fields[modOpportunity.strfCONFIGURATION_COMPLETE].Value = CheckCompleteness(RecordId);

                // TODO (Di Yin) the rstAlert never assigned
                // objParam.SetUserDefinedParameter(1, rstAlert);

                // check to see if inventory quotes can be inactivated
                bool InactivateQuote = this.CanInactivateInventoryQuote(RecordId);
                objParam.SetUserDefinedParameter(2, InactivateQuote);
                if (pForm.FormName == modOpportunity.strrHB_OPPORTUNITY_OPTIONS || pForm.FormName == modOpportunity.strrCHANGE_ORDER_OPTIONS)
                {

                    UIAccess objPLFunctionLib = (UIAccess)RSysSystem.ServerScripts[AppServerRuleData.UIAccessAppServerRuleName].CreateInstance();
                    // VB code use field name to search disconnected fields, that is wrong, we should use field title 
                    string strPageCount = objPLFunctionLib.GetDisconnectedFieldName(pForm.FormName, modOpportunity.PageCountFieldTitle,
                        modOpportunity.strsHIDDEN);

                    string strPagination = objPLFunctionLib.GetDisconnectedFieldName(pForm.FormName, modOpportunity.PaginationFieldTitle,
                        modOpportunity.strsAVAILABLE_FILTER);

                    int intPage = 1;
                    int intPageCount = 0;
                    if (Share.IsNumeric(rstOpportunity.Fields[modOpportunity.strfCURRENT_PAGE].Value))
                    {
                        intPage = (TypeConvert.ToInt32(rstOpportunity.Fields[modOpportunity.strfCURRENT_PAGE].Value));
                    }

                    LoadNeighborhoodProducts(recordsetArray, TypeConvert.ToString(objParam.GetUserDefinedParameter(3)), ref intPage, ref intPageCount);

                    // return the current page as it could have changed
                    rstOpportunity.Fields[modOpportunity.strfCURRENT_PAGE].Value = intPage;
                    rstOpportunity.Fields[strPageCount].Value = intPageCount;

                    // clear out the load filter for first time loading.
                    if (String.Compare(TypeConvert.ToString(objParam.GetUserDefinedParameter(3)), "<filter load>", true) == 0)
                    {
                        objParam.SetUserDefinedParameter(3, string.Empty);
                    }

                    // figure out user's settings for configuring options
                    bool blnDisableDecorator = false;
                    bool blnDisableStructure = false;
                    bool blnIsEmpNBHDEmpty = true;
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                    object employeeId = administration.CurrentUserRecordId;
                    object vntReleaseId = ((Recordset)recordsetArray[0]).Fields[modOpportunity.strf_NBHD_PHASE_ID].Value;
                    Recordset rstEmployeeNBHD = objLib.GetRecordset(modOpportunity.strqEMP_NBHD_FOR_REL_FOR_EMP, 2, vntReleaseId,
                        employeeId, modOpportunity.strfDISABLE_DECORATOR, modOpportunity.strfDISABLE_STRUCTURAL);
                    if (rstEmployeeNBHD.RecordCount > 0)
                    {
                        rstEmployeeNBHD.MoveFirst();
                        blnDisableDecorator = TypeConvert.ToBoolean(rstEmployeeNBHD.Fields[modOpportunity.strfDISABLE_DECORATOR].Value);
                        blnDisableStructure = TypeConvert.ToBoolean(rstEmployeeNBHD.Fields[modOpportunity.strfDISABLE_STRUCTURAL].Value);
                        blnIsEmpNBHDEmpty = false;
                    }
                    objParam.SetUserDefinedParameter(4, blnDisableDecorator);
                    objParam.SetUserDefinedParameter(5, blnDisableStructure);
                    objParam.SetUserDefinedParameter(6, blnIsEmpNBHDEmpty);
                }
                ParameterList = objParam.ParameterList;
                return vntRecordset;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function gets new opportunity form data from the database.
        /// </summary>
        /// <param name="pForm">IRForm reference to the current form</param>
        /// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
        /// <returns>Variant array of recordsets of Opportunity</returns>
        /// <history>
        /// Revision       Date           Author   Description
        /// 3.8.0.0        5/12/2006      DYin     Converted to .Net C# code.
        /// HB
        /// </history>
        public virtual object NewFormData(IRForm pForm, ref object ParameterList)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // Get New Form Data
                object vntOpportunity = pForm.DoNewFormData(ref ParameterList);
                object[] recordsetArray = (object[])vntOpportunity;
                Recordset rstOpportunity = (Recordset)recordsetArray[0];

                // Set Default Fields value
                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;
                if (objParam.HasValidParameters() == false)
                {
                    objParam.Construct();
                }
                else
                {
                    objParam.SetDefaultFields(rstOpportunity);
                    objParam.WarningMessage = string.Empty;
                    ParameterList = objParam.ParameterList;
                }

                // Get the Default Code Filter. If this has not been filled out, Don't crash.
                SystemSetting systemSetting = (SystemSetting)RSysSystem.ServerScripts[AppServerRuleData.SystemSettingAppServerRuleName].CreateInstance();
                rstOpportunity.Fields[modOpportunity.strfFILTER_CODE_].Value = systemSetting.GetSystemSetting(modOpportunity.strfOPTION_CODE_FILTER);

                object vntContact_Id = rstOpportunity.Fields[modOpportunity.strf_CONTACT_ID].Value;
                object vntLot_Id = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;

                // if there is only one release id then populate it with that
                if (rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value != DBNull.Value)
                {
                    Recordset rstNPhase = objLib.GetRecordset(modOpportunity.strqOPEN_NBHD_PHASES_FOR_NBHD, 1, rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value,
                        modOpportunity.strf_NBHD_PHASE_ID);
                    if (rstNPhase.RecordCount == 1)
                    {
                        rstOpportunity.Fields[modOpportunity.strf_NBHD_PHASE_ID].Value = rstNPhase.Fields[modOpportunity.strf_NBHD_PHASE_ID].Value;
                    }
                }

                // set plan price if applicable
                if (pForm.FormName != modOpportunity.strrINVENTORY_QUOTE_SEARCH)
                {
                    Decimal homesitePremium = 0;
                    rstOpportunity.Fields[modOpportunity.strfPRICE].Value = GetQuotePlanPrice(rstOpportunity, out homesitePremium);
                    rstOpportunity.Fields[modOpportunity.strfLOT_PREMIUM].Value = homesitePremium;

                } else if (pForm.FormName == modOpportunity.strrHB_QUOTE || pForm.FormName == modOpportunity.strrHB_SALE || pForm.FormName
                    == modOpportunity.strrINVENTORY_QUOTE || pForm.FormName == modOpportunity.strrHB_QUICK_QUOTE)
                {
                    // default sales rep to current user
                    Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                    Recordset rstTest = objLib.GetRecordset(modOpportunity.strqHB_CAN_BE_SALES_REP, 1, administration.CurrentUserRecordId,
                        modOpportunity.strf_EMPLOYEE_ID);
                    if (rstTest.RecordCount > 0)
                    {
                        rstOpportunity.Fields[modOpportunity.strf_ACCOUNT_MANAGER_ID].Value = administration.CurrentUserRecordId;
                    }
                }
                return vntOpportunity;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine adds a new record to a given secondary.
        /// </summary>
        /// <param name="pForm">IRForm reference to the current active form.</param>
        /// <param name="SecondaryName">Secondary Name for Opportunity form</param>
        /// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer and from the 
        /// AppServer to client</param>
        /// <param name="Recordset">Variant array of recordsets of Opportunity form data</param>
        /// <history>
        /// Revision       Date           Author   Description
        /// 3.8.0.0        5/12/2006      DYin     Converted to .Net C# code.
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
        /// This subroutine saves opportunity form data to the database.
        /// </summary>
        /// <param name="pForm">IRForm reference to the current active form</param>
        /// <param name="Recordsets">Variant array of recordsets of Opportunity form data</param>
        /// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer
        /// User Def 1 - State of Quota Period</param>
        /// <history>
        /// Revision       Date           Author      Description
        /// 3.8.0.0        5/12/2006      DYin        Converted to .Net C# code.
        /// 5.9            04/05/2007     JH          Fixed 65536-16245.
        /// </history>
        public virtual void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                object[] recordsetArray = (object[])Recordsets;
                Recordset rstOpportunity = (Recordset) recordsetArray[0];
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // TL - added to update Homesite Premium during apply
                // Set Plan Price, Lot Premium and Realtor Id when record is saved
                object vntLot_Id = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                object vntPlan_Name_Id = rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value;
                object vntOpportunity_Id = rstOpportunity.Fields[modOpportunity.strf_OPPORTUNITY_ID].Value;
                string vntStatus = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfPIPELINE_STAGE].Value);

                string vntOppStatus = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfSTATUS].Value);

                //  Check to see if Plan_Id of form is not NULL
                //AM2010.10.04 - Added logic to include Reservations for this logic for setting the Lot PRemium
                if (!(vntStatus == modOpportunity.strPIPELINE_CANCELED || vntStatus == modOpportunity.strPIPELINE_CLOSED
                     || vntStatus == modOpportunity.strPIPELINE_CONTRACT || vntStatus == modOpportunity.strsSALES_REQUEST 
                     || vntOppStatus == modOpportunity.strsRESERVED ))
                {
                    // Check to see if Plan_Id of form is not NULL
                    if (!((Convert.IsDBNull(vntPlan_Name_Id))))
                    {
                        decimal homesitePremium = 0;
                        //ML - commented - 23 july 07 so that we deal with clients changed recordset
                        //rstOpportunity.Fields[modOpportunity.strfPRICE].Value = GetQuotePlanPrice(vntOpportunity_Id,
                        //    vntLot_Id, vntPlan_Name_Id, out homesitePremium);
                        rstOpportunity.Fields[modOpportunity.strfPRICE].Value = GetQuotePlanPrice(rstOpportunity, out homesitePremium);
                        if (vntLot_Id != DBNull.Value)
                        {
                            rstOpportunity.Fields[modOpportunity.strfLOT_PREMIUM].Value = homesitePremium;
                        }
                    }
                }
                bool blnCloseContract = false;

                if (pForm.FormName == modOpportunity.strrHB_OPPORTUNITY_OPTIONS || pForm.FormName == modOpportunity.strrCHANGE_ORDER_OPTIONS
                    || pForm.FormName == modOpportunity.strrHB_OPPORTUNITY_ADJUSTMENTS)
                {
                    // we are saving the Options select form etc no additional checks are necessary, just save.
                    pForm.DoSaveFormData(Recordsets, ref ParameterList);
                    //If contract On Hold set homesite as Sold.
                    if (TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value) == modOpportunity.strsCONTRACT
                        && TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfSTATUS].Value) == modOpportunity.strsON_HOLD)
                    {
                        UpdateLotStatusEx(vntLot_Id, modOpportunity.strsSOLD);
                    }
                    return ;
                }

                // prepare objContactProfileNBHD and rstNBHDP for later use.
                ContactProfileNeighborhood objContactProfileNBHD = (ContactProfileNeighborhood)RSysSystem.ServerScripts[modOpportunity.strsCONTACT_PROFILE_NBHD].CreateInstance();
                object vntNBHDId = rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                object vntContact_Id = rstOpportunity.Fields[modOpportunity.strf_CONTACT_ID].Value;

                Recordset rstNBHDP = objLib.GetRecordset(modOpportunity.strqCONTACT_PROFILE_NBHD_FOR_CONTACT, 2, vntContact_Id,
                    vntNBHDId, modOpportunity.strfCONTACT_PROFILE_NBHD_ID);

                vntOpportunity_Id = rstOpportunity.Fields[modOpportunity.strf_OPPORTUNITY_ID].Value;
                bool vntPlan_Built = TypeConvert.ToBoolean(rstOpportunity.Fields[modOpportunity.strfPLAN_BUILT].Value);
                vntLot_Id = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                vntPlan_Name_Id = rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value;
                object vntContactId = rstOpportunity.Fields[modOpportunity.strfCONTACT_ID].Value;

                // if the pipeline stage is closed, then set all other quotes for this lot to not pursued
                if (TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value) == modOpportunity.strsCONTRACT)
                {
                    SetQuotesToNotPursued(vntLot_Id, vntOpportunity_Id);

                    // Jul 12, 2005. By JWang
                    // redefine the Warranty Date in term of Months. and only define it once.
                    if (Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfWARRANTY_DATE].Value))
                    {
                        // set the warranty field on this
                        SystemSetting systemSetting = (SystemSetting)RSysSystem.ServerScripts[AppServerRuleData.SystemSettingAppServerRuleName].CreateInstance();
                        int intWarrantyMonths = TypeConvert.ToInt32(systemSetting.GetSystemSetting(modOpportunity.strfWARRANTY_START_AFTER));
                        rstOpportunity.Fields[modOpportunity.strfWARRANTY_DATE].Value = DateTime.Today.AddMonths(intWarrantyMonths);
                    }

                    // update the contact NBHD profile dates
                    // May 26 By JWang. provide close date from HB Sale form instead of using the current date.
                    UpdateContactProfileNeighborhood(vntContactId, rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value,
                        null, null, null, null, null, null, null, null, 
                        TypeConvert.ToDateTime(rstOpportunity.Fields[modOpportunity.strfACTUAL_REVENUE_DATE].Value),
                        null, null);

                    // update the Lot and contact close and sale dates
                    if ((vntContactId != DBNull.Value))
                    {
                        if (TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfSTATUS].Value) == modOpportunity.strsCLOSED)
                        {
                            Recordset rstContact = objLib.GetRecordset(vntContactId, modOpportunity.strtCONTACT, modOpportunity.strfCLOSE_DATE,
                                modOpportunity.strfTYPE);
                            if (rstContact.RecordCount > 0)
                            {
                                if (Convert.IsDBNull(rstContact.Fields[modOpportunity.strfCLOSE_DATE].Value) || 
                                    (TypeConvert.ToDateTime(rstContact.Fields[modOpportunity.strfCLOSE_DATE].Value)
                                    < TypeConvert.ToDateTime(rstOpportunity.Fields[modOpportunity.strfACTUAL_REVENUE_DATE].Value)))
                                {
                                    rstContact.Fields[modOpportunity.strfCLOSE_DATE].Value = rstOpportunity.Fields[modOpportunity.strfACTUAL_REVENUE_DATE].Value;
                                }
                                blnCloseContract = true;
                            }
                            objLib.SaveRecordset(modOpportunity.strtCONTACT, rstContact);
                        }
                    }

                    if (vntLot_Id != DBNull.Value)
                    {
                        Recordset rstLot = objLib.GetRecordset(vntLot_Id, modOpportunity.strtPRODUCT, modOpportunity.strfSALES_DATE,
                            modOpportunity.strfCONTRACT_CLOSE_DATE);
                        if (rstLot.RecordCount > 0)
                        {
                            if ((Convert.IsDBNull(rstLot.Fields[modOpportunity.strfSALES_DATE].Value) || 
                                (TypeConvert.ToDateTime(rstLot.Fields[modOpportunity.strfSALES_DATE].Value) 
                                < TypeConvert.ToDateTime(rstOpportunity.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value))))
                            {
                                //KA 10/14/10 not setting this cause if it's cancelled, we don't want lot to get stamped
                                //rstLot.Fields[modOpportunity.strfSALES_DATE].Value = rstOpportunity.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value;
                            }
                            if (TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfSTATUS].Value) == modOpportunity.strsCLOSED)
                            {
                                if ((Convert.IsDBNull(rstLot.Fields[modOpportunity.strfCONTRACT_CLOSE_DATE].Value) ||
                                    (TypeConvert.ToDateTime(rstLot.Fields[modOpportunity.strfCONTRACT_CLOSE_DATE].Value)
                                    < TypeConvert.ToDateTime(rstOpportunity.Fields[modOpportunity.strfACTUAL_REVENUE_DATE].Value))))
                                {
                                    //KA 10/14/10 not setting this cause it's being set by Escrow "Closed" method
                                    //rstLot.Fields[modOpportunity.strfCONTRACT_CLOSE_DATE].Value = rstOpportunity.Fields[modOpportunity.strfACTUAL_REVENUE_DATE].Value;
                                }
                            }
                        }
                        objLib.PermissionIgnored = true;
                        objLib.SaveRecordset(modOpportunity.strtPRODUCT, rstLot);
                    }
                }

                UpdateCoBuyerStatus(vntOpportunity_Id, false, false);

                // TODO (Di Yin) objInstance, vntAccount_Manager and vntDelta_Account_Manager are never assigned
                //if (!(mrsysSystem.EqualIds(vntAccount_Manager, vntDelta_Account_Manager)))
                //{
                //    objInstance.TickleOpportunity(rstOpportunity.Fields[modOpportunity.strf_OPPORTUNITY_ID].Value, false);
                //}

                // check to see if a convert to sale is needed:
                TransitionPointParameter objTransit = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objTransit.ParameterList = ParameterList;
                object[] parameterArray = objTransit.GetUserDefinedParameterArray();
                bool blnCreateContract = false;
                if (parameterArray.Length > 0) blnCreateContract = TypeConvert.ToBoolean(parameterArray[0]);

                string vntLot_Status = TypeConvert.ToString(RSysSystem.Tables[modOpportunity.strt_PRODUCT].Fields[modOpportunity.strfLOT_STATUS].Index(vntLot_Id));
                string strPSStage = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value);
                string strStatus = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfSTATUS].Value);

                // added by JWang May 28, 2005
                // If Plan built flag is unset, set homesite built plan field as null otherwise set as plan_name_id
                // from quote
                if ((pForm.FormName == modOpportunity.strrINVENTORY_QUOTE) || (pForm.FormName == 
                    modOpportunity.strrHB_SALE))
                {
                    if (TypeConvert.ToBoolean(rstOpportunity.Fields[modOpportunity.strfPLAN_BUILT].Value))
                    {
                        UpdateHomesitePlan(rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value, rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value);
                    }
                    else
                    {
                        UpdateHomesitePlan(rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value, DBNull.Value);
                    }
                }

                // added by JWang Apr. 29, 2005
                if (pForm.FormName == modOpportunity.strrINVENTORY_QUOTE)
                {
                    // Update corresponding Active Customer Quote's Plan Built Flag
                    UpdatePlanBuiltForActiveCustomerQuote(vntLot_Id, vntPlan_Built);
                    // Loop through secondary recordset Options to update options in active customer quotes for the
                    // lot
                    Recordset optionRecordset = pForm.SecondaryFromVariantArray(Recordsets, modOpportunity.strsegOPTIONS);
                    UpdateOptionBuilts(optionRecordset, vntLot_Id);

                    // Update additional price on customer quotes - fpoulsen
                    UpdateCustomerQuoteAdditionalPrice(rstOpportunity);
                }

                // added by JWang May 29, 2005
                // if the sale is closed set Lot status as Closed and Contract Pipeline_Stage as Closed
                // Added check for close contract - fpoulsen May 31, 2005
                if (blnCloseContract)
                {
                    if (TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value) == modOpportunity.strsCONTRACT)
                    {
                        UpdateLotStatusEx(vntLot_Id, modOpportunity.strsCLOSED);
                        rstOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value = modOpportunity.strsCLOSED;
                    }
                }

                // Add sales rep to NBHD Profile and Quote Sales team, if different from original Sales Rep
                // fpoulsen 06/29/2005
                if (!(RSysSystem.EqualIds(rstOpportunity.Fields[modOpportunity.strf_ACCOUNT_MANAGER_ID].Value, 
                    rstOpportunity.Fields[modOpportunity.strf_ACCOUNT_MANAGER_ID].OriginalValue)))
                {
                    object vntSalesRep = rstOpportunity.Fields[modOpportunity.strf_ACCOUNT_MANAGER_ID].Value;
                    if (rstNBHDP.RecordCount == 1)
                    {
                        rstNBHDP.MoveFirst();
                        object vntNBHDPId = rstNBHDP.Fields[modOpportunity.strfCONTACT_PROFILE_NBHD_ID].Value;
                        objContactProfileNBHD.UpdateNBHDProfieTeam(vntNBHDPId, vntSalesRep);
                    }

                    // Add Sales Rep to Quote Sales Team
                    UpdateOpportunitySalesTeam(vntSalesRep, RSysSystem.Tables[modOpportunity.strtEMPLOYEE].Fields[modOpportunity.strf_ROLE_ID].Index(vntSalesRep),
                        vntOpportunity_Id, true);

                }

                pForm.DoSaveFormData(Recordsets, ref ParameterList);

                //If contract On Hold set homesite as Sold.
                if (TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value) == modOpportunity.strsCONTRACT
                    && TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfSTATUS].Value) == modOpportunity.strsON_HOLD)
                {
                    UpdateLotStatusEx(vntLot_Id, modOpportunity.strsSOLD);
                }

                // added by Carl Langan 01/05/05 for integration
                IRAppScript objIntegration = (IRAppScript)RSysSystem.ServerScripts[modOpportunity.strs_INTEGRATION].CreateInstance();
                object vntParam = DBNull.Value;
                objIntegration.Execute(modOpportunity.strmIS_INTEGRATION_ON, ref vntParam);
                TransitionPointParameter integrationTransitionPointParameter = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                integrationTransitionPointParameter.ParameterList = vntParam;

                if ((integrationTransitionPointParameter.UserDefinedParametersNumber > 0))
                {
                    if (TypeConvert.ToBoolean(integrationTransitionPointParameter.GetUserDefinedParameter(1)))
                    {
                        integrationTransitionPointParameter.SetUserDefinedParameter(1, rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value);
                        vntParam = integrationTransitionPointParameter.ParameterList;
                        objIntegration.Execute(modOpportunity.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref vntParam);
                    }
                }

                if (blnCreateContract)
                {
                    if (parameterArray.Length > 1)
                    {
                        parameterArray = ConvertToSale(vntOpportunity_Id, TypeConvert.ToBoolean(parameterArray[1]));
                    }
                    else
                    {
                        parameterArray = ConvertToSale(vntOpportunity_Id);
                    }
                    if (parameterArray[0].ToString().Length > 0)
                    {
                        integrationTransitionPointParameter.InfoMessage = parameterArray[0].ToString();
                        ParameterList = integrationTransitionPointParameter.ParameterList;
                        return;
                        //throw new PivotalApplicationException(parameterArray[0].ToString(), modOpportunity.glngERR_SAVEFORMDATA_FAILED);
                    }

                }
                else if (vntLot_Status != modOpportunity.strsSOLD && strPSStage == modOpportunity.strsCONTRACT
                    && strStatus == modOpportunity.strsON_HOLD)
                {
                    // added by Carl Langan for Quick Contract
                    parameterArray = ConvertToSale(vntOpportunity_Id);
                    if (parameterArray[0].ToString().Length > 0)
                    {
                        integrationTransitionPointParameter.InfoMessage = parameterArray[0].ToString();
                        ParameterList = integrationTransitionPointParameter.ParameterList;
                        return;
                        //throw new PivotalApplicationException(parameterArray[0].ToString(), modOpportunity.glngERR_SAVEFORMDATA_FAILED);
                    }
                }

                CalculateTotals(vntOpportunity_Id, false);

                UpdateQuoteStatus(vntOpportunity_Id);

                // update contact nbhd profile type
                if (rstNBHDP.RecordCount >  0)
                {
                    // update contact profile neighborhood type
                    rstNBHDP.MoveFirst();
                    object vntNBHDPId = rstNBHDP.Fields[modOpportunity.strfCONTACT_PROFILE_NBHD_ID].Value;
                    objContactProfileNBHD.UpdateNBHDPType(vntNBHDPId);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine sets the Active Client System.
        /// </summary>
        /// <returns>None
        /// Implements Agent: None</returns>
        /// <history>
        /// Revision       Date           Author   Description
        /// 3.8.0.0        5/12/2006      DYin     Converted to .Net C# code.
        /// </history>
        public virtual void SetSystem(RSystem pSystem)
        {
            try
            {
                RSysSystem = (IRSystem7) pSystem;
                LangDict = RSysSystem.GetLDGroup(modOpportunity.strgOPPORTUNITY);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// This function will add Change Orders records
        /// </summary>
        /// <param name="neighborhoodPhaseProductIdArray">the Neighborhood Id record</param>
        /// <param name="changeOrderId">the change order Id</param>
        /// <param name="opportunityId">the opportunity Id</param>
        /// <param name="changeOrderStatus">enumerator ChangeOrderStatus</param>
        /// <returns>
        /// True or False</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        public virtual object AddChangeOrders(object[] neighborhoodPhaseProductIdArray, object changeOrderId, object opportunityId,
            ChangeOrderStatus changeOrderStatus)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                object changOrderOptionId = DBNull.Value;
                for (int i = 0; i < neighborhoodPhaseProductIdArray.Length; ++i)
                {
                    // add the nbhdproduct to the change order
                    if (TypeConvert.ToString(neighborhoodPhaseProductIdArray[i]).Trim().Length > 0)
                    {
                        object vntNBHDProductId = neighborhoodPhaseProductIdArray[i];

                        Recordset rstOppProduct = objLib.GetRecordset(modOpportunity.strqOPP_PRODUCT_FOR_OPP_AND_NBHD_PRODUCT,
                            2, opportunityId, vntNBHDProductId, modOpportunity.strfBUILT_OPTION, modOpportunity.strfCODE_,
                            modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL,
                            modOpportunity.strfCUSTOMERINSTRUCTIONS, modOpportunity.strfDELTA_BUILT_OPTION, modOpportunity.strfDEPOSIT,
                            modOpportunity.strfDIVISION_PRODUCT_ID, modOpportunity.strfEXTENDED_PRICE, modOpportunity.strfFILTER_VISIBILITY,
                            modOpportunity.strf_NBHDP_PRODUCT_ID, modOpportunity.strfNET_CONFIG, modOpportunity.strfOPP_CURRENCY,
                            modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strf_OPPORTUNITY__PRODUCT_ID, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID,
                            modOpportunity.strfOPTIONNOTES, modOpportunity.strfPREFERENCE, modOpportunity.strfPREFERENCES_LIST,
                            modOpportunity.strfPRICE, modOpportunity.strfPRODUCT_AVAILABLE, modOpportunity.strfPRODUCT_ID,
                            modOpportunity.strfPRODUCT_NAME, modOpportunity.strfQUANTITY, modOpportunity.strfQUOTED_PRICE,
                            modOpportunity.strfSELECTED, modOpportunity.strfTICKLE_COUNTER, modOpportunity.strfTYPE
                            );

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
                                modOpportunity.strfSELECTED, modOpportunity.strfTICKLE_COUNTER, modOpportunity.strfTYPE
                                //modOpportunity.EnvGTINField, modOpportunity.EnvNHTManufacturerNumberField,
                                //modOpportunity.EnvProductBrandField, modOpportunity.EnvProductNumberField,
                                //modOpportunity.EnvDUNSNumberField, modOpportunity.EnvUCCCodeField,
                                //modOpportunity.EnvManufacturerProductField
                                );
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
                            //rstChangeOrder.Fields[modOpportunity.EnvManufacturerProductField].Value=EnvManufacturerProduct(
                            //    TypeConvert.ToString(rstChangeOrder.Fields[modOpportunity.EnvGTINField].Value) 
                            //    , TypeConvert.ToString(rstChangeOrder.Fields[modOpportunity.EnvNHTManufacturerNumberField].Value)
                            //    , TypeConvert.ToString(rstChangeOrder.Fields[modOpportunity.EnvProductNumberField].Value)
                            //    , TypeConvert.ToString(rstChangeOrder.Fields[modOpportunity.EnvDUNSNumberField].Value)
                            //    , TypeConvert.ToString(rstChangeOrder.Fields[modOpportunity.EnvUCCCodeField].Value));
                            objLib.SaveRecordset(modOpportunity.strtCHANGE_ORDER_OPTIONS, rstChangeOrder);
                            changOrderOptionId = rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_OPTIONS_ID].Value;
                        }
                    }
                }
                return changOrderOptionId;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        

        ///// <summary>
        ///// Because the manufacturer product data is moved from Opportunity__Product table to Opp_Product_Location table
        ////  Just comment out this function for now.

        ///// This function defined EnvManufacturerProductField value based on 
        /////     modOpportunity.EnvGTINField, modOpportunity.EnvNHTManufacturerNumberField,
        /////     modOpportunity.EnvProductNumberField,
        /////     modOpportunity.EnvDUNSNumberField, modOpportunity.EnvUCCCodeField,
        ///// </summary>
        ///// <param name="gtin">gtin</param>
        ///// <param name="nhtManufacturerNumber">nhtManufacturerNumber</param>
        ///// <param name="productNumber">productNumber</param>
        ///// <param name="dunsNumber">dunsNumber</param>
        ///// <param name="uccCode">uccCode</param>
        ///// <history>
        ///// Revision#     Date        Author      Description
        ///// 5.9.0.0       4/4/2007    JWang       init version
        ///// </history>
        //private string EnvManufacturerProduct(string gtin, string nhtManufacturerNumber,
        //    string productNumber,string dunsNumber,string uccCode)
        //{
        //    string manufacturerProduct="";
        //    if (gtin != "")
        //        manufacturerProduct ="GTIN: " + gtin;
        //    else
        //        if (productNumber != "")
        //            if (nhtManufacturerNumber != "")
        //                manufacturerProduct = "MF#: " + nhtManufacturerNumber + "  Prod#: " + productNumber;
        //            else
        //                if (dunsNumber != "")
        //                    manufacturerProduct = "DUNS#: " + dunsNumber + "  Prod#: " + productNumber;
        //                else
        //                    if (uccCode != "")
        //                        manufacturerProduct = "UCC#: " + uccCode + "  Prod#: " + productNumber;
        //    return manufacturerProduct;
        //}

        /// <summary>
        /// This function gets the Lender for the Loan Officer
        /// </summary>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        public virtual void BatchUpdateQuoteExpiry()
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // check expiry value in system table
                Recordset rstSystem = objLib.GetRecordset("Find \"System Wide Properties\" record", 0, "System_Id", modOpportunity.strfQUOTE_EXPIRY_PERIOD_DAYS);
                if (rstSystem.RecordCount > 0)
                {
                    rstSystem.MoveFirst();
                    int intQuote_Expiry_Period_Days = TypeConvert.ToInt32(rstSystem.Fields[modOpportunity.strfQUOTE_EXPIRY_PERIOD_DAYS].Value);
                    if (intQuote_Expiry_Period_Days > 0)
                    {
                        // clean up

                        // check for empty expiry dates and set them accordingly
                        Recordset rstQuote = objLib.GetRecordset(modOpportunity.strq_QUOTES_WITH_NO_EXP_DEC_DATE, 0, modOpportunity.strf_EXPECTED_DECISION_DATE,
                            modOpportunity.strf_QUOTE_CREATE_DATE);
                        if (rstQuote.RecordCount > 0)
                        {
                            rstQuote.MoveFirst();
                            while (!(rstQuote.EOF))
                            {
                                if ((rstQuote.Fields[modOpportunity.strf_QUOTE_CREATE_DATE].Value.GetType() == typeof(DateTime)))
                                {
                                    rstQuote.Fields[modOpportunity.strf_EXPECTED_DECISION_DATE].Value =
                                        TypeConvert.ToDateTime(rstQuote.Fields[modOpportunity.strf_QUOTE_CREATE_DATE].Value).AddDays
                                    (TypeConvert.ToInt32(intQuote_Expiry_Period_Days));
                                }
                                rstQuote.MoveNext();
                            }
                        }
                        // save changes
                        objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstQuote);
                        rstQuote.Close();

                        // get all in progress or on hold quotes with an expiry date of today or earlier
                        rstQuote = objLib.GetRecordset(modOpportunity.strq_QUOTES_TO_BE_EXPIRED, 0, modOpportunity.strf_STATUS,
                            modOpportunity.strf_RN_DESCRIPTOR);
                        if (rstQuote.RecordCount > 0)
                        {
                            rstQuote.MoveFirst();
                            while (!(rstQuote.EOF))
                            {
                                rstQuote.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsINACTIVE;
                                // log detail
                                // mrsysSystem.LogEvent grldtLangDict.GetText(strdLOG_DETAIL) & .Fields(strf_RN_DESCRIPTOR).Value
                                rstQuote.MoveNext();
                            }
                        }
                        // save changes
                        objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstQuote);
                        rstQuote.Close();
                    }
                    else
                        RSysSystem.LogEvent(TypeConvert.ToString(LangDict.GetText(modOpportunity.strdERR_NO_EXPIRY_PERIOD))
                            + TypeConvert.ToString(DateTime.Now));
                }
                else
                    RSysSystem.LogEvent(TypeConvert.ToString(LangDict.GetText(modOpportunity.strdERR_NO_SYSTEM_RECORD))
                        + TypeConvert.ToString(DateTime.Now));
            }
            catch (Exception exc)
            {
                RSysSystem.LogEvent(TypeConvert.ToString(LangDict.GetText(modOpportunity.strdERROR) + exc.Message + " - " + DateTime.Now));
            }
        }

        /// <summary>
        /// Overloaded method will calculate totals for the gived Opportunity and at the same time recalculate the adjustment.
        /// Also, for each adjustment on the quote redo the sum field. 
        /// </summary>
        /// <param name="opportunityId">opportunity Id</param>
        /// <history>
        /// Revision#    Date       Author    Description
        /// 3.8.0.0      5/12/2006  DYin      Converted to .Net C# code.
        /// </history>
        public virtual void CalculateTotals(object opportunityId)
        {
            CalculateTotals(opportunityId, false);
        }

        /// <summary>
        /// This method will calculate totals for the gived Opportunity and at the same time recalculate the adjustment.
        /// Also, for each adjustment on the quote redo the sum field.
        /// </summary>
        /// <param name="opportunityId">opportunity Id</param>
        /// <param name="resetQuote">Flag to indicate whether reset quote or not.</param>
        /// <history>
        /// Revision#    Date       Author    Description
        /// 3.8.0.0      5/12/2006  DYin      Converted to .Net C# code.
        /// </history>
        public virtual void CalculateTotals(object opportunityId, bool resetQuote)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOpp = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strfFINANCED_OPTIONS,
                    modOpportunity.strfPRICE, modOpportunity.strfLOT_PREMIUM, modOpportunity.strfQUOTE_TOTAL, modOpportunity.strfELEVATION_PREMIUM,
                    modOpportunity.strfADJUSTMENT_TOTAL, modOpportunity.strfPPI_ADJUSTMENT_TOTAL, modOpportunity.strfELEVATION_ID,
                    modOpportunity.strfREQUIRED_DEPOSIT_AMOUNT, modOpportunity.strfPLAN_NAME_ID,
                    modOpportunity.TIC_Base_Price_Adj_Total, modOpportunity.TIC_Closing_Cost_Adj_Total, modOpportunity.TIC_Decorator_Adj_Total,
                    modOpportunity.TIC_Design_Options_Total, modOpportunity.TIC_DG_Option_Deposit_Total, modOpportunity.TIC_Init_Option_Deposit_Total,
                    modOpportunity.TIC_Other_Deposit_Total, modOpportunity.TIC_Preplot_Options, modOpportunity.TIC_Structural_Adj_Total,
                    modOpportunity.TIC_Structural_Options_Total, modOpportunity.TIC_Total_Options_Deposits, modOpportunity.TIC_Struct_Opt_Deposit_Total,
                    modOpportunity.TIC_Pre_Plotted_Structural_Opt, modOpportunity.TIC_Closing_Costs_Adjustments, modOpportunity.TIC_Decorator_Opt_Cost_Total,
                    modOpportunity.strfLOT_ID, modOpportunity.TIC_Actual_Broker_Commission,
                    modOpportunity.TIC_Merch_Bond_Adj_Total);

                if (resetQuote)
                {
                    // clear the whole quote and recalculate the totals
                    DeleteOptions(opportunityId);
                    DeleteTeam(opportunityId);
                    rstOpp.Fields[modOpportunity.strfELEVATION_ID].Value = DBNull.Value;
                    rstOpp.Fields[modOpportunity.strfQUOTE_TOTAL].Value = 0;
                    rstOpp.Fields[modOpportunity.strfPRICE].Value = 0;
                    rstOpp.Fields[modOpportunity.strfREQUIRED_DEPOSIT_AMOUNT].Value = 0;
                }

                // set current totals to 0
                decimal dblOptionTotal = 0;
                decimal dblRqdDepAmt = 0;

                //rstOpp.Fields[modOpportunity.strfELEVATION_PREMIUM].Value = 0;

                //AM2010.09.08 - Calculate all breakout totals for IP
                decimal dblStructOptionDeposit = 0;
                decimal dblDecOptionDeposit = 0;
                decimal dblInitOptionDeposit = 0;
                decimal dblOtherOptionDeposit = 0;

                decimal dblBasePriceAdj = 0;
                decimal dblDecoratorAdj = 0;
                decimal dblStructAdj = 0;
                decimal dblCloseCostAdj = 0;
                decimal dblMerchBondAdj = 0;

                decimal dblStructOptions = 0;
                decimal dblDecoratorOptions = 0;
                decimal dblPreplotOptions = 0;
                decimal dblPreplotStructOptions = 0;

                decimal dblDecoratorOptCostTotal = 0;

                decimal dblOptionSqFtTotal = 0;
                decimal dblActualBrokComm = 0;
                
                //Calculate Option Deposit Breakouts
                CalculateOptionDepositBuckets(opportunityId, out dblStructOptionDeposit, out dblDecOptionDeposit,
                    out dblInitOptionDeposit, out dblOtherOptionDeposit);
                rstOpp.Fields[modOpportunity.TIC_Struct_Opt_Deposit_Total].Value = dblStructOptionDeposit;
                rstOpp.Fields[modOpportunity.TIC_DG_Option_Deposit_Total].Value = dblDecOptionDeposit;
                rstOpp.Fields[modOpportunity.TIC_Init_Option_Deposit_Total].Value = dblInitOptionDeposit;
                rstOpp.Fields[modOpportunity.TIC_Other_Deposit_Total].Value = dblOtherOptionDeposit;

                //Calculate Adjustment Breakouts
                CalculateAdjustmentBuckets(opportunityId, out dblBasePriceAdj, out dblDecoratorAdj,
                    out dblStructAdj, out dblCloseCostAdj, out dblMerchBondAdj);
                rstOpp.Fields[modOpportunity.TIC_Base_Price_Adj_Total].Value = dblBasePriceAdj;
                rstOpp.Fields[modOpportunity.TIC_Decorator_Adj_Total].Value = dblDecoratorAdj;
                rstOpp.Fields[modOpportunity.TIC_Structural_Adj_Total].Value = dblStructAdj;
                rstOpp.Fields[modOpportunity.TIC_Closing_Cost_Adj_Total].Value = dblCloseCostAdj;
                rstOpp.Fields[modOpportunity.TIC_Merch_Bond_Adj_Total].Value = dblMerchBondAdj;

                //Calculate Option Breakouts
                CalculateOptionBuckets(opportunityId, out dblStructOptions, out dblDecoratorOptions);
                rstOpp.Fields[modOpportunity.TIC_Structural_Options_Total].Value = dblStructOptions;
                //AM2011.02.26 - New DesignOptions Total should remove the design center incentives (except Merchandise bonds)
                rstOpp.Fields[modOpportunity.TIC_Design_Options_Total].Value = (dblDecoratorOptions - dblDecoratorAdj);

                //Calculate Preplot Options
                CalculatePrePlotOptionBuckets(opportunityId, out dblPreplotOptions, out dblPreplotStructOptions);
                rstOpp.Fields[modOpportunity.TIC_Preplot_Options].Value = dblPreplotOptions;
                rstOpp.Fields[modOpportunity.TIC_Pre_Plotted_Structural_Opt].Value = dblPreplotStructOptions;

                //Calculate Design Option Costs
                CalculateDesignOptionCosts(opportunityId, out dblDecoratorOptCostTotal);
                rstOpp.Fields[modOpportunity.TIC_Decorator_Opt_Cost_Total].Value = dblDecoratorOptCostTotal;

                //Calculate Option Totals (All Options excluding Preplotted options)
                //CalculateOptionTotalsForContract(opportunityId, out dblOptionTotal);
                //AM2011.02.26 - Use Structural Options + Design Center options less design center discounts and allowancse
                rstOpp.Fields[modOpportunity.strfFINANCED_OPTIONS].Value 
                    = dblStructOptions + (dblDecoratorOptions - dblDecoratorAdj); //dblOptionTotal;

                //Calculate Option SqFt totals for Lot
                CalculateOptionSquareFootageForContract(opportunityId, out dblOptionSqFtTotal);
                Recordset rstLot = objLib.GetRecordset(rstOpp.Fields[modOpportunity.strfLOT_ID].Value, modOpportunity.strtPRODUCT,
                    modOpportunity.strfTIC_OPTIONED_SQFT);
                if (rstLot.RecordCount > 0)
                {
                    rstLot.MoveFirst();
                    rstLot.Fields[modOpportunity.strfTIC_OPTIONED_SQFT].Value = TypeConvert.ToDecimal(dblOptionSqFtTotal);
                    objLib.SaveRecordset(modOpportunity.strtPRODUCT, rstLot);                    
                }
                rstLot.Close();

                //Calculate Actual Broker Commissions
                CalculateActualBrokerCommissions(opportunityId, out dblActualBrokComm);
                rstOpp.Fields[modOpportunity.TIC_Actual_Broker_Commission].Value = TypeConvert.ToDecimal(dblActualBrokComm);


                /*
                 * AM2010.09.08 - Calculate totals buckets for each custom bucket for IP
                 * 
                 //get all the selected options for the quote
                Recordset rstOpportunityProduct = objLib.GetRecordset(modOpportunity.strqSELECTED_OPTIONS_FOR_QUOTE, 1, 
                    opportunityId, modOpportunity.strfEXTENDED_PRICE, modOpportunity.strfTYPE, 
                    modOpportunity.strfNBHDP_PRODUCT_ID, modOpportunity.strfDEPOSIT, modOpportunity.strfDIVISION_PRODUCT_ID);

                if (rstOpportunityProduct.RecordCount > 0)
                {
                    rstOpportunityProduct.MoveFirst();
                    while (!(rstOpportunityProduct.EOF))
                    {
                        if (TypeConvert.ToString(rstOpportunityProduct.Fields[modOpportunity.strfTYPE].Value) == modOpportunity.strsELEVATION)
                        {   // set elevation premium field directly
                            rstOpp.Fields[modOpportunity.strfELEVATION_PREMIUM].Value = rstOpportunityProduct.Fields[modOpportunity.strfEXTENDED_PRICE].Value;
                        }
                        else
                        {
                            dblOptionTotal = dblOptionTotal + TypeConvert.ToDecimal(rstOpportunityProduct.Fields[modOpportunity.strfEXTENDED_PRICE].Value);
                        }
                        object vntDivProductId = rstOpportunityProduct.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;
                        if (vntDivProductId != DBNull.Value)
                        {
                            dblRqdDepAmt = dblRqdDepAmt + TypeConvert.ToDecimal(objLib.SqlIndex(modOpportunity.strtDIVISION_PRODUCT,
                                modOpportunity.strfREQUIRED_DEPOSIT_AMOUNT, vntDivProductId));
                        }
                        else
                        {    // custom option
                            dblRqdDepAmt = dblRqdDepAmt + TypeConvert.ToDecimal(rstOpportunityProduct.Fields[modOpportunity.strfDEPOSIT].Value);
                        }
                        rstOpportunityProduct.MoveNext();
                    }
                }
                rstOpportunityProduct.Close();
                */


                // Save the Opportunity Record
                //AM2010.09.08 - Exclude Pre-Plotted options totals from the Option Totals

                //rstOpp.Fields[modOpportunity.strfFINANCED_OPTIONS].Value = dblOptionTotal;

                object vntDivPanId = objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfDIVISION_PRODUCT_ID,
                    rstOpp.Fields[modOpportunity.strfPLAN_NAME_ID].Value);
                rstOpp.Fields[modOpportunity.strfREQUIRED_DEPOSIT_AMOUNT].Value = dblRqdDepAmt + TypeConvert.ToDecimal
                    (objLib.SqlIndex(modOpportunity.strtDIVISION_PRODUCT, modOpportunity.strfREQUIRED_DEPOSIT_AMOUNT, vntDivPanId));

                //Decimal dblQuoteAdjustmentTotal = 0;
                Decimal dblTotalPPIAdjustments = 0;

                // at the same time lets recalculate the adjustment
                OpportunityAdjustment objOppAdjustment = (OpportunityAdjustment)RSysSystem.ServerScripts[modOpportunity.strsOPP_ADJUSTMENT].CreateInstance();
                // for each adjustment on the quote redo the sum field
                Recordset rstOppAdj = objLib.GetRecordset(modOpportunity.strqSELECTED_ADJUSTMENTS_FOR_OPPORTUNITY, 1, opportunityId,
                    modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID, modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfADJUSTMENT_AMOUNT,
                    modOpportunity.strfSUM_FIELD, modOpportunity.strfADJUSTMENT_PERCENTAGE, modOpportunity.strfAPPLY_TO,
                    modOpportunity.strfADJUSTMENT_TYPE);
                if (rstOppAdj.RecordCount > 0)
                {
                    rstOppAdj.MoveFirst();
                    while (!(rstOppAdj.EOF))
                    {
                        Decimal dblRetVal = TypeConvert.ToDecimal(objOppAdjustment.CalculateSumField(rstOppAdj));
                        if (dblRetVal != 0)
                        {
                            rstOppAdj.Fields[modOpportunity.strfSUM_FIELD].Value = dblRetVal;
                            string strAdjustmentType = TypeConvert.ToString(rstOppAdj.Fields[modOpportunity.strfADJUSTMENT_TYPE].Value);
                            switch (strAdjustmentType)
                            {
                                case "PPI":
                                    dblTotalPPIAdjustments = dblTotalPPIAdjustments + dblRetVal;
                                    break;
                                case "Post Contract":
                                    break;

                                default:
                                    //dblQuoteAdjustmentTotal = dblQuoteAdjustmentTotal + dblRetVal;
                                    break;
                            }
                        }
                        rstOppAdj.MoveNext();
                    }
                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY_ADJUSTMENT, rstOppAdj);
                }
                rstOpp.Fields[modOpportunity.strfPPI_ADJUSTMENT_TOTAL].Value = dblTotalPPIAdjustments;

                //AM2010.09.22 - Calculate Concessions and exclude Closing Cost Adjustment, and design center adjustments
                //only add Merchandise Bonds here.  Set Closing Costs Adjustments to seperate field
                rstOpp.Fields[modOpportunity.strfADJUSTMENT_TOTAL].Value = (dblStructAdj + dblBasePriceAdj + dblMerchBondAdj) * -1; //dblQuoteAdjustmentTotal;
                rstOpp.Fields[modOpportunity.TIC_Closing_Costs_Adjustments].Value = dblCloseCostAdj;

                objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstOpp);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }



        /// <summary>
        /// Create a copy of the given Quote record
        /// </summary>
        /// <param name="quoteOpportunityId">the quote id</param>
        /// <param name="copyPlan">Flag to determine whether to assign the plan/options or not</param>
        /// <param name="copyContract">boolean to determine if this is a Contract</param>
        /// <param name="copyPostSaleQuote">boolean to determine if this is a Post Sale Quote</param>
        /// <returns>A variant containing the newly created quote</returns>
        /// <history>
        /// Revision#    Date        Author   Description
        /// 3.8.0.0      5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        public virtual object CopyQuote(object quoteOpportunityId, bool copyPlan, bool copyContract, bool copyPostSaleQuote)
        {
            return CopyQuote(quoteOpportunityId, copyPlan, copyContract, copyPostSaleQuote, false);
        }
   
        /// <summary>
        /// Create a copy of the given Quote record
        /// </summary>
        /// <param name="quoteOpportunityId">the quote id</param>
        /// <param name="copyPlan">Flag to determine whether to assign the plan/options or not</param>
        /// <param name="copyContract">boolean to determine if this is a Contract</param>
        /// <param name="copyPostSaleQuote">boolean to determine if this is a Post Sale Quote</param>
        /// <param name="transferContract">Boolean flag to indicate if transfer contract or not</param>
        /// <returns>A variant containing the newly created quote</returns>
        /// <history>
        /// Revision#    Date        Author   Description
        /// 3.8.0.0      5/12/2006   DYin     Converted to .Net C# code.
        /// 5.9.0        10/14/2010  KA       Don't copy Transfer or Rollback boolean     
        /// 5.9.1        11/02/2010  AM       Don't copy Reservation_Expiry Date  
        /// </history>
        public virtual object CopyQuote(object quoteOpportunityId, bool copyPlan, bool copyContract, bool copyPostSaleQuote, 
            bool transferContract)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstQuote = objLib.GetRecordset(quoteOpportunityId, modOpportunity.strt_OPPORTUNITY);
                Recordset rstQuoteNew = objLib.GetNewRecordset(modOpportunity.strt_OPPORTUNITY);

                rstQuoteNew.AddNew(Type.Missing, Type.Missing);
                // Apr. 27, 2005 - BH
                // Allow copying to the same Contact
                // Comment out "arrFields(i) <> strf_CONTACT_ID And _"
                foreach (Field field in rstQuoteNew.Fields)
                {
                    string fieldName = field.Name;
                    TypeEnum fieldType = ((IRField5)RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[fieldName]).Type;
                    bool notEqualWorkflow = (fieldName.Length < 9);
                    if (fieldName.Length >= 9)
                    {
                        notEqualWorkflow = (fieldName.Substring(0, 9).ToUpper() != "WORKFLOW_");
                    }
                    //KA 10/14/10
                    //AM2010.11.01 - omit Reservation_Expiry_Date from copy
                    if (fieldName != modOpportunity.strfQUOTE_CREATE_DATE &&
                        fieldName != modOpportunity.strfCLOSE_DATE &&
                        fieldName != modOpportunity.strf_PLAN_NAME_ID &&
                        fieldName != modOpportunity.strf_OPPORTUNITY_ID &&
                        fieldName != modOpportunity.strfPOST_SALE_ID &&
                        fieldName != modOpportunity.strfDEPOSIT_AMOUNT_TAKEN &&
                        fieldName != modOpportunity.strfTIC_TRANSFER &&
                        fieldName != modOpportunity.strfTIC_ROLLBACK &&
                        fieldName != modOpportunity.strfRESERVATIONEXPIRY &&
                        ((fieldName).Substring(0, 3)).ToUpper() != "RN_" &&
                        fieldType != TypeEnum.metaDate && notEqualWorkflow)
                    {
                        // BH - Do not copy over any dates
                        if (fieldType == TypeEnum.metaText)
                        {
                            // Handle when the text length is greater then defined length for some calculated fields
                            // Debuig code 
                            rstQuoteNew.Fields[fieldName].Value = objLib.GetValidAssignValue(field, rstQuote.Fields[fieldName].Value);
                        }
                        else
                        {
                            rstQuoteNew.Fields[fieldName].Value = rstQuote.Fields[fieldName].Value;
                        }
                    }
                    else if (copyContract && fieldName == modOpportunity.strf_CONTACT_ID)
                    {
                        rstQuoteNew.Fields[fieldName].Value = rstQuote.Fields[fieldName].Value;
                    }
                    // Debug.Print arrFields(i)
                }

                rstQuoteNew.Fields[modOpportunity.strfQUOTE_CREATE_DATE].Value = DateTime.Now;
                rstQuoteNew.Fields[modOpportunity.strfQUOTE_CREATE_DATETIME].Value = DateTime.Now;
                rstQuoteNew.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value = DBNull.Value;
                rstQuoteNew.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsIN_PROGRESS;
                rstQuoteNew.Fields[modOpportunity.strfINACTIVE].Value = false;

                if (transferContract)
                {
                    // Fix Issue #65536-15076 - Not copying the "Plan_Build" field.
                    rstQuoteNew.Fields[modOpportunity.strfPLAN_BUILT].Value = false;
                }

                // Fix Issue #65536-15143
                rstQuoteNew.Fields[modOpportunity.strfRESERVATION_AMOUNT].Value = DBNull.Value;

                if (copyPlan)
                {
                    rstQuoteNew.Fields[modOpportunity.strf_PLAN_NAME_ID].Value = rstQuote.Fields[modOpportunity.strf_PLAN_NAME_ID].Value;
                }

                if (copyPostSaleQuote)
                {
                    // copy over the sales request and sales date
                    rstQuoteNew.Fields[modOpportunity.strfCONTRACT_APPROVED_SUBMITTED].Value = rstQuote.Fields[modOpportunity.strfCONTRACT_APPROVED_SUBMITTED].Value;
                    rstQuoteNew.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value = rstQuote.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value;

                    // set the Pipeline stage here
                    string vntStatus = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strfSTATUS,
                        quoteOpportunityId));
                    if (vntStatus == modOpportunity.strsINVENTORY)
                    {
                        // post build quote
                        rstQuoteNew.Fields[modOpportunity.strf_PIPELINE_STAGE].Value = RSysSystem.GetLDGroup(modOpportunity.strgOPPORTUNITY).GetText(modOpportunity.strlPOST_BUILD_SALE);
                    }
                    else
                    {
                        rstQuoteNew.Fields[modOpportunity.strf_PIPELINE_STAGE].Value = RSysSystem.GetLDGroup(modOpportunity.strgOPPORTUNITY).GetText(modOpportunity.strlPOST_SALE);
                    }

                }

                objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstQuoteNew);
                object vntNewQuoteId = rstQuoteNew.Fields[modOpportunity.strf_OPPORTUNITY_ID].Value;

                // add new opp team
                CopyQuoteSecondary(quoteOpportunityId, vntNewQuoteId, modOpportunity.strt_OPPORTUNITY_TEAM_MEMBER, modOpportunity.strf_OPPORTUNITY_ID,
                    modOpportunity.strf_OPPORTUNITY_TEAM_MEMBER_ID, false);

                if (copyPlan)
                {
                    // add options
                    CopyQuoteSecondary(quoteOpportunityId, vntNewQuoteId, modOpportunity.strt_OPPORTUNITY__PRODUCT, modOpportunity.strf_OPPORTUNITY_ID,
                        modOpportunity.strf_OPPORTUNITY__PRODUCT_ID, copyPostSaleQuote);
                }

                // Add adjustments
                CopyQuoteSecondary(quoteOpportunityId, vntNewQuoteId, modOpportunity.strtOPPORTUNITY_ADJUSTMENT, modOpportunity.strfOPPORTUNITY_ID,
                    modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID, copyPostSaleQuote);

                // May 4, 2005 - BH
                // If the Contact NBHD Profile is inactive on the Quote, then make it Active
                object vntContactId = rstQuote.Fields[modOpportunity.strfCONTACT_ID].Value;
                object vntNeighborhoodId = rstQuote.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                Recordset rstContactNBHDProfile = objLib.GetRecordset(modOpportunity.strqINACTIVE_CONTACT_PROFILE_NBHD, 2, vntContactId,
                    vntNeighborhoodId, modOpportunity.strfINACTIVE);
                if (rstContactNBHDProfile.RecordCount > 0)
                {
                    rstContactNBHDProfile.MoveFirst();
                    while (!rstContactNBHDProfile.EOF)
                    {
                        rstContactNBHDProfile.Fields[modOpportunity.strfINACTIVE].Value = false;
                        rstContactNBHDProfile.MoveNext();
                    }
                    objLib.SaveRecordset(modOpportunity.strtCONTACT_PROFILE_NEIGHBORHOOD, rstContactNBHDProfile);
                }
                return vntNewQuoteId;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#  Date        Author   Description
        /// 3.8.0.0    5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        public virtual void UpdateLotStatus(object lotId, object opportunityQuoteId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstLot = objLib.GetRecordset(lotId, modOpportunity.strt_PRODUCT, modOpportunity.strfLOT_PRODUCT_ID, modOpportunity.strfLOT_STATUS,
                    modOpportunity.strfRESERVED_DATE, modOpportunity.strfRESERVATION_CONTRACT_ID,modOpportunity.strfTIC_CO_BUYER_ID, modOpportunity.strfOWNER_ID);
                if (rstLot.RecordCount > 0)
                {
                    rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsRESERVED;
                    rstLot.Fields[modOpportunity.strfRESERVATION_CONTRACT_ID].Value = opportunityQuoteId;
                    rstLot.Fields[modOpportunity.strfRESERVED_DATE].Value = DateTime.Today;
                    //Set the buyer and co-buyer on Lot
                    object vntBuyerId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strfCONTACT_ID, opportunityQuoteId);
                    object vntCoBuyerId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strfTIC_CO_BUYER_ID, opportunityQuoteId);
                    rstLot.Fields[modOpportunity.strfTIC_CO_BUYER_ID].Value = vntCoBuyerId;
                    rstLot.Fields[modOpportunity.strfOWNER_ID].Value = vntBuyerId;

                    objLib.PermissionIgnored = true;
                    objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will update the Quote's chosen elevation when Elevation option changed.
        /// </summary>
        /// <param name="quoteOpportunityId">Quote Id</param>
        /// <param name="newElevationNeighborhoodPhaseProductId">New elevation Neighborhood Phase Product Id</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        public virtual void UpdateQuoteChosenElevation(object quoteOpportunityId, object newElevationNeighborhoodPhaseProductId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOpportunity = objLib.GetRecordset(quoteOpportunityId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfELEVATION_ID);
                if (rstOpportunity.RecordCount > 0)
                {
                    rstOpportunity.MoveFirst();
                    rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value = newElevationNeighborhoodPhaseProductId;
                    objLib.PermissionIgnored = true;
                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstOpportunity);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Called by the Inventory Quote Search business object. To search any combination of the
        /// fields based on the HB Inventory Quote Search form.
        /// </summary>
        /// <param name="formName">Contact search form name</param>
        /// <param name="quoteRecordset">Recordset holds the contact search information</param>
        /// <returns>Inventory quote recordset.</returns>
        /// <history>
        /// Revision #  Date        Author  Description
        /// 3.8.0.0     5/12/2006   DYin    Converted to .Net C# code.
        /// </history>
        protected virtual Recordset InventoryQuoteSearch(string formName, Recordset quoteRecordset)
        {
            try
            {
                DataAccess objDLFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                UIAccess objPLFunctionLib = (UIAccess)RSysSystem.ServerScripts[AppServerRuleData.UIAccessAppServerRuleName].CreateInstance();
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                object vntNeighborhoodId = quoteRecordset.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                object vntReleaseId = quoteRecordset.Fields[modOpportunity.strf_NBHD_PHASE_ID].Value;
                object planNameId = quoteRecordset.Fields[modOpportunity.strf_PLAN_NAME_ID].Value;
                DateTime datCreateDate = TypeConvert.ToDateTime(quoteRecordset.Fields[modOpportunity.strfRN_CREATE_DATE].Value);
                object strStreet = quoteRecordset.Fields[objPLFunctionLib.GetDisconnectedFieldName(modOpportunity.strrINVENTORY_QUOTE_SEARCH,
                    modOpportunity.strfDIS_STREET, modOpportunity.strsINVENTORY_QUOTE_SEARCH, null)].Value;
                object strTract = quoteRecordset.Fields[objPLFunctionLib.GetDisconnectedFieldName(modOpportunity.strrINVENTORY_QUOTE_SEARCH,
                    modOpportunity.strfDIS_TRACT, modOpportunity.strsINVENTORY_QUOTE_SEARCH, null)].Value;
                object strConstructionStage = quoteRecordset.Fields[objPLFunctionLib.GetDisconnectedFieldName(modOpportunity.strrINVENTORY_QUOTE_SEARCH,
                    modOpportunity.strfDIS_CONSTRUCTION_STAGE, modOpportunity.strsINVENTORY_QUOTE_SEARCH, null)].Value;
                object strDevelopmentPhase = quoteRecordset.Fields[objPLFunctionLib.GetDisconnectedFieldName(modOpportunity.strrINVENTORY_QUOTE_SEARCH,
                    modOpportunity.strfDIS_DEVELOPMENT_PHASE, modOpportunity.strsINVENTORY_QUOTE_SEARCH, null)].Value;
                object strBlock = quoteRecordset.Fields[objPLFunctionLib.GetDisconnectedFieldName(modOpportunity.strrINVENTORY_QUOTE_SEARCH,
                    modOpportunity.strfDIS_BLOCK, modOpportunity.strsINVENTORY_QUOTE_SEARCH, null)].Value;
                object strLotNumber = quoteRecordset.Fields[objPLFunctionLib.GetDisconnectedFieldName(modOpportunity.strrINVENTORY_QUOTE_SEARCH,
                    modOpportunity.strfDIS_LOT_NUMBER, modOpportunity.strsINVENTORY_QUOTE_SEARCH, null)].Value;
                object strBuilding = quoteRecordset.Fields[objPLFunctionLib.GetDisconnectedFieldName(modOpportunity.strrINVENTORY_QUOTE_SEARCH,
                    modOpportunity.strfDIS_BUILDING, modOpportunity.strsINVENTORY_QUOTE_SEARCH, null)].Value;
                object strUnit = quoteRecordset.Fields[objPLFunctionLib.GetDisconnectedFieldName(modOpportunity.strrINVENTORY_QUOTE_SEARCH,
                    modOpportunity.strfDIS_UNIT, modOpportunity.strsINVENTORY_QUOTE_SEARCH, null)].Value;
                object strJobNumber = quoteRecordset.Fields[objPLFunctionLib.GetDisconnectedFieldName(modOpportunity.strrINVENTORY_QUOTE_SEARCH,
                    modOpportunity.strfDIS_JOB_NUMBER, modOpportunity.strsINVENTORY_QUOTE_SEARCH, null)].Value;

                // Set to the past date when you don't need to search this field
                // Otherwise set it to the future date
                DateTime datPAST_DATE = TypeConvert.ToDateTime(DBNull.Value);
                DateTime datFUTURE_DATE = DateTime.MaxValue;
                DateTime datForNeighborhoodID = Convert.IsDBNull(vntNeighborhoodId) ? datPAST_DATE : datFUTURE_DATE;
                DateTime datForReleaseID = Convert.IsDBNull(vntReleaseId) ? datPAST_DATE : datFUTURE_DATE;
                DateTime datForPlanNameID = Convert.IsDBNull(planNameId) ? datPAST_DATE : datFUTURE_DATE;
                DateTime datForStreet = strStreet == DBNull.Value ? datPAST_DATE : datFUTURE_DATE;
                DateTime datForTract = strTract == DBNull.Value ? datPAST_DATE : datFUTURE_DATE;
                DateTime datForConstructionStage = strConstructionStage == DBNull.Value ? datPAST_DATE : datFUTURE_DATE;
                DateTime datForDevelopmentPhase = strDevelopmentPhase == DBNull.Value ? datPAST_DATE : datFUTURE_DATE;
                DateTime datForBlock = strBlock == DBNull.Value ? datPAST_DATE : datFUTURE_DATE;
                DateTime datForLotNumber = strLotNumber == DBNull.Value ? datPAST_DATE : datFUTURE_DATE;
                DateTime datForBuilding = strBuilding == DBNull.Value ? datPAST_DATE : datFUTURE_DATE;
                DateTime datForUnit = strUnit == DBNull.Value ? datPAST_DATE : datFUTURE_DATE;
                DateTime datForJobNumber = strJobNumber == DBNull.Value ? datPAST_DATE : datFUTURE_DATE;

                if (strConstructionStage == DBNull.Value)
                {
                    return objLib.GetRecordset(modOpportunity.strqINVENTORY_QUOTE_SEARCH, 11, vntNeighborhoodId,
                        vntReleaseId, planNameId, strStreet, strTract, strDevelopmentPhase, strBlock, strLotNumber,
                        strBuilding, strUnit, strJobNumber, modOpportunity.strfOPPORTUNITY_ID);
                }
                else
                {
                    return objLib.GetRecordset(modOpportunity.strqINVENTORY_QUOTE_SEARCH_W_STAGE, 13,
                        vntNeighborhoodId, vntReleaseId, planNameId, strStreet, strTract, strDevelopmentPhase, strBlock,
                        strLotNumber, strBuilding, strUnit, strJobNumber, strConstructionStage, strConstructionStage,
                        modOpportunity.strfOPPORTUNITY_ID);
                    // strfRN_DESCRIPTOR)
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine takes the Id of an Opportunity, and offers the
        /// </summary>
        /// user the choice of deleting the associated table records.
        /// <returns>None
        /// Implements Agent: Sys\Form\Opportunity\OnDelete</returns>
        /// <history>
        /// Revision#       Date           Author   Description
        /// 3.8.0.0         5/12/2006      DYin     Converted to .Net C# code.
        /// HB  3.7        01/31/2006      AV    Removed unused code, this is not complete
        /// 5.9             5/21/2007      JH       This function is deprecated in 5.9.
        /// </history>
        protected virtual void OpportunityCascadeDelete(object opportunityId)
        {
            throw new PivotalApplicationException("Deletion not allowed.");
        }

        /// <summary>
        /// Review the Passed in Record Set passed, set quantity to 1 for any selected and check elevation rule.
        /// </summary>
        /// <param name="optionRecordset">Option recordset.</param>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        protected virtual string SelectMultipleOptions(Recordset optionRecordset, object opportunityId)
        {
            return this.SelectMultipleOptions(optionRecordset, opportunityId, DBNull.Value);
        }

        /// <summary>
        /// Review the Passed in Record Set passed, set quantity to 1 for any selected and check elevation rule.
        /// </summary>
        /// <param name="optionRecordset">Option recordset.</param>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="changeOrderId">Change Order Id</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        protected virtual string SelectMultipleOptions(Recordset optionRecordset, object opportunityId, object changeOrderId)
        {
            try
            {
                if (optionRecordset == null)
                {
                    return string.Empty;
                }
                else
                {
                    StringBuilder newOptionBuilder = new StringBuilder();
                    StringBuilder newOptionParentChildBuilder = new StringBuilder();
                    if (optionRecordset.RecordCount > 0)
                    {
                        // first get list of new options for select option checking
                        optionRecordset.MoveFirst();
                        bool blnElevation = false;
                        while (!(optionRecordset.EOF))
                        {
                            // record id for rule checking
                            if (TypeConvert.ToBoolean(optionRecordset.Fields[modOpportunity.strfSELECTED].Value))
                            {
                                if (!(Convert.IsDBNull(optionRecordset.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value) && Convert.IsDBNull(optionRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value)))
                                {

                                    newOptionBuilder.Append(RSysSystem.IdToString(optionRecordset.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value) + ";");
                                    newOptionParentChildBuilder.Append("Parent:" + RSysSystem.IdToString(optionRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value) + ",Child:" + RSysSystem.IdToString(optionRecordset.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value) + ";");
                                }                                
                                // record if multiple elevation
                                if (TypeConvert.ToString(optionRecordset.Fields[modOpportunity.strfTYPE].Value) == modOpportunity.strsELEVATION)
                                {
                                    if (blnElevation)
                                    {
                                        // this must be at least the second elevation
                                        newOptionBuilder.Append(modOpportunity.strsELEVATION);
                                    }
                                    blnElevation = true;
                                }
                                if (Convert.IsDBNull(optionRecordset.Fields[modOpportunity.strfQUANTITY].Value))
                                {
                                    if (!(Convert.IsDBNull(optionRecordset.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value) && Convert.IsDBNull(optionRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value)))
                                        optionRecordset.Fields[modOpportunity.strfQUANTITY].Value = GetQuantity(opportunityId, optionRecordset.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value);
                                    else
                                    optionRecordset.Fields[modOpportunity.strfQUANTITY].Value = 1;
                                }
                            }
                            optionRecordset.MoveNext();
                        }
                        // continue with selection
                        optionRecordset.MoveFirst();
                        StringBuilder messageBuilder = new StringBuilder();
                        while (!(optionRecordset.EOF))
                        {
                            if (!(Convert.IsDBNull(optionRecordset.Fields[modOpportunity.strfQUANTITY].Value)) && TypeConvert.ToDouble(optionRecordset.Fields[modOpportunity.strfQUANTITY].Value)
                                > 0.0)
                            {
                                string strTemp = TypeConvert.ToString(SelectUnselectOptions(opportunityId, optionRecordset.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                                    optionRecordset.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value, true, changeOrderId,
                                    TypeConvert.ToInt32(optionRecordset.Fields[modOpportunity.strfQUANTITY].Value), newOptionBuilder.ToString(), newOptionParentChildBuilder.ToString()));
                                if (!(strTemp.Trim().Length == 0))
                                {
                                    if (!messageBuilder.ToString().Contains(strTemp))
                                    {
                                        messageBuilder.Append("\r\n" + strTemp);
                                    }
                                }
                            }
                            optionRecordset.MoveNext();
                        }
                        if (messageBuilder.Length > 0)
                        {
                            return TypeConvert.ToString(LangDict.GetText(modOpportunity.strdSELECT_MULTIPLE_ERROR)) + messageBuilder.ToString();
                        }
                    }
                    return string.Empty;
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine toggles the products between select and unselect, sets availability flag on unselected 
        /// options, adds Change Orders and recalculates the adjustment.
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="opportunityProductId">Opportunity Product Id</param>
        /// <param name="neighborhoodPhaseProductId">Neighborhood Product Id</param>
        /// <param name="selected">Flag value true or false</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        protected virtual object SelectUnselectOptions(object opportunityId, object opportunityProductId, 
            object neighborhoodPhaseProductId, bool selected)
        {
            return SelectUnselectOptions(opportunityId, opportunityProductId, neighborhoodPhaseProductId, selected,
                DBNull.Value, 1, string.Empty, string.Empty);
        }

        /// <summary>
        /// This subroutine toggles the products between select and unselect, sets availability flag on unselected 
        /// options, adds Change Orders and recalculates the adjustment.
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="opportunityProductId">Opportunity Product Id</param>
        /// <param name="neighborhoodPhaseProductId">Neighborhood Product Id</param>
        /// <param name="selected">Flag value true or false</param>
        /// <param name="changeOrderId">Change Order Id</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        protected virtual object SelectUnselectOptions(object opportunityId, object opportunityProductId, 
            object neighborhoodPhaseProductId, bool selected, object changeOrderId)
        {
            return SelectUnselectOptions(opportunityId, opportunityProductId, neighborhoodPhaseProductId, selected,
                changeOrderId, 1, string.Empty, string.Empty);
        }

        /// <summary>
        /// This subroutine toggles the products between select and unselect, sets availability flag on unselected 
        /// options, adds Change Orders and recalculates the adjustment.
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="opportunityProductId">Opportunity Product Id</param>
        /// <param name="neighborhoodPhaseProductId">Neighborhood Product Id</param>
        /// <param name="selected">Flag value true or false</param>
        /// <param name="changeOrderId">Change Order Id</param>
        /// <param name="quantity">quantity</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        protected virtual object SelectUnselectOptions(object opportunityId, object opportunityProductId, 
            object neighborhoodPhaseProductId, bool selected, object changeOrderId, int quantity)
        {
            return SelectUnselectOptions(opportunityId, opportunityProductId, neighborhoodPhaseProductId, selected, 
                changeOrderId, quantity, string.Empty, string.Empty);
        }

        /// <summary>
        /// This subroutine toggles the products between select and unselect,
        /// sets availability flag on unselected options, adds Change Orders and
        /// recalculates the adjustment
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="opportunityProductId">Opportunity Product Id</param>
        /// <param name="neighborhoodPhaseProductId">Neighborhood Product Id</param>
        /// <param name="selected">Flag value true or false</param>
        /// <param name="changeOrderId">Change Order Id</param>
        /// <param name="quantity">quantity</param>
        /// <param name="newOption">mulitple use string that is used in elevation and rules checks</param>
        /// <param name="newOptionWithParents">Multiple use string used for rule checks with the following implementation:
        /// "Parent:Division_Product_Id,Child:NBHDP_Product_Id;"</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// 5.9.0.0        2/20/2007     YK         Changed as per HB r5.9 release guidelines.
        /// 5.9.0.0        1/3/2007      BC         Changed the code to handle the Package Type
        /// </history>
        protected virtual string SelectUnselectOptions(object opportunityId, object opportunityProductId, 
            object neighborhoodPhaseProductId, bool selected, object changeOrderId, int quantity, string newOption, string newOptionWithParents)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // check for multiple elevations - only need to check against new options
                if (selected)
                {
                    if (newOption.Contains(modOpportunity.strsELEVATION))
                    {
                        return TypeConvert.ToString(LangDict.GetText(modOpportunity.strdMULTIPLE_ELEVATIONS));
                    }
                }
                                
                string strMsg = string.Empty;
                object planNameId = DBNull.Value;
                object planProductId = DBNull.Value;
                // check exclusion rules but omit custom options
                if ((neighborhoodPhaseProductId != DBNull.Value))
                {
                    planNameId = RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[modOpportunity.strf_PLAN_NAME_ID].Index(opportunityId);
                    

                    if (selected)
                    {
                        //YK - Commenting this call as we do not want ot check for more than 1 level.
                        //     from the UI interface, we have already check for 1 level.
                        //YK - strMsg = GetSelectedExcludedOptions(opportunityId, neighborhoodPhaseProductId, planNameId, newOption, newOptionWithParents);
                        if (strMsg.Length > 0)
                        {
                            return strMsg;
                        }
                    }
                }

                // get employee data
                object vntEmployeeId = DBNull.Value;
                if (selected)
                {
                    Administration administration = (Administration) RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                    vntEmployeeId = administration.CurrentUserRecordId;
                }
                else
                {
                    vntEmployeeId = DBNull.Value;
                }

                string[] vntRetVal = new string[0];
                if ((neighborhoodPhaseProductId != DBNull.Value))
                {

                    //YK - Commenting this call as we do not want ot check for more than 1 level.
                    //     from the UI interface, we have already check for 1 level.
                    //YK - //YK - Call for All Hard Rules
                    //YK - strMsg = GetChildOptions(neighborhoodPhaseProductId, planNameId, opportunityId , false);
                    
                    //YK - The Soft Rule Auto Inclusion has been hauled for this release. Not Removing this piece 
                    //     of code, so as to just uncomment it to make it work in the near future as and when
                    //     required.
                    // //YK - Call for All Soft Rules
                    // strMsg = strMsg + GetChildOptions(neighborhoodPhaseProductId, planNameId, opportunityId, true);
                    
                    // msg is an string containing the nbhdproduct ids with a ";" delimiter
                    strMsg = "Self:" + RSysSystem.IdToString(neighborhoodPhaseProductId) + ";" + strMsg;

                    Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfELEVATION_ID,
                        modOpportunity.strfELEVATION_PREMIUM, modOpportunity.strfPIPELINE_STAGE, modOpportunity.strfCONTRACT_APPROVED_SUBMITTED,
                        modOpportunity.strfQUOTE_CREATE_DATE, modOpportunity.strfCONTRACT_APPROVED_SUBMITTED_DATETIME,
                        modOpportunity.strfQUOTE_CREATE_DATETIME, modOpportunity.strfPOST_SALE_ID);

                    if (rstOpportunity.RecordCount > 0)
                    {
                        rstOpportunity.MoveFirst();
                        object vntPipeline_Stage = rstOpportunity.Fields[modOpportunity.strfPIPELINE_STAGE].Value;
                        object vntSalesRequestDate = Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfCONTRACT_APPROVED_SUBMITTED].Value)
                            ? DBNull.Value : rstOpportunity.Fields[modOpportunity.strfCONTRACT_APPROVED_SUBMITTED].Value;
                        object vntSalesRequestDateTime = Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfCONTRACT_APPROVED_SUBMITTED_DATETIME].Value)
                            ? vntSalesRequestDate : rstOpportunity.Fields[modOpportunity.strfCONTRACT_APPROVED_SUBMITTED_DATETIME].Value;
                        object vntQuoteCreateDate = Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfQUOTE_CREATE_DATE].Value)
                            ? DBNull.Value : rstOpportunity.Fields[modOpportunity.strfQUOTE_CREATE_DATE].Value;
                        object vntQuoteCreateDateTime = Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfQUOTE_CREATE_DATETIME].Value)
                            ? vntQuoteCreateDate : rstOpportunity.Fields[modOpportunity.strfQUOTE_CREATE_DATETIME].Value;

                        vntRetVal = strMsg.Split(new char[] { Convert.ToChar(";") });
                        for (int index = 0; index <= vntRetVal.GetUpperBound(0) - 1; ++index)
                        {
                            String[] strRetVal = new string[0];
                            object vntLocalNBHDPId = DBNull.Value;
                            // get the record and set the selected to the boolean
                            if (TypeConvert.ToString(vntRetVal[index]).Length > 0)
                            {
                                //YK - Splitting the <Type>:<RecordID> pair
                                strRetVal = vntRetVal[index].Split(new char[] { Convert.ToChar(":") });
                                
                                vntLocalNBHDPId = RSysSystem.StringToId(TypeConvert.ToString(strRetVal[1]));
                                Recordset rstRecordset = objLib.GetRecordset(modOpportunity.strqOPP_PRODUCT_FOR_OPP_AND_NBHD_PRODUCT,
                                    2, opportunityId, vntLocalNBHDPId, modOpportunity.strf_SELECTED, modOpportunity.strfNET_CONFIG,
                                    modOpportunity.strfOPTION_ADDED_BY, modOpportunity.strfPRICE, modOpportunity.strfOPPORTUNITY__PRODUCT_ID,
                                    modOpportunity.strfQUANTITY, modOpportunity.strfTYPE, modOpportunity.strfNBHDP_PRODUCT_ID,
                                    modOpportunity.strfUSE_POST_CUTOFF_PRICE, modOpportunity.strfDIVISION_PRODUCT_ID, modOpportunity.strfOPTION_SELECTED_DATE);
                                if (rstRecordset.RecordCount > 0)
                                {
                                    rstRecordset.MoveFirst();
                                    // RY: Issue 19305 2005/08/18 Don't unselect child option when parent is unselected
                                    if (selected || (!selected && RSysSystem.EqualIds(rstRecordset.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value,
                                        neighborhoodPhaseProductId)))
                                    {
                                        rstRecordset.Fields[modOpportunity.strf_SELECTED].Value = selected;
                                        rstRecordset.Fields[modOpportunity.strfOPTION_SELECTED_DATE].Value = DateTime.Now;

                                        //YK - Call this function only if of type Package.
                                        if (Convert.ToString(rstRecordset.Fields[modOpportunity.strf_TYPE].Value) == modOpportunity.strsPACKAGE)
                                        {
                                            UpdateChildOpportunityProducts(rstRecordset.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value, selected);
                                        }
                                    }

                                    rstRecordset.Fields[modOpportunity.strfOPTION_ADDED_BY].Value = vntEmployeeId;

                                    // need to update the price of the selected option now, when its selected
                                    if (selected)
                                    {
                                        object vntDivProductId = rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;
                                        bool blnUsePCO = false;
                                        if (!Convert.IsDBNull(vntDivProductId))
                                        {
                                            //BC Use PCO
                                            string strConstructionStageComparison = GetConstructionStageComparison();
                                            object vntHomesiteID = RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfLOT_ID].Index(opportunityId);
                                            

                                            if ((!Convert.IsDBNull(RSysSystem.Tables[modOpportunity.strtPRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntHomesiteID)))
                                                && (!Convert.IsDBNull(RSysSystem.Tables[modOpportunity.strtDIVISION_PRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntDivProductId))))                                            {
                                                object vntHomesiteConstructionStageId = RSysSystem.Tables[modOpportunity.strtPRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntHomesiteID);
                                                int intHomesiteConstructionStageOrdinal = (int)RSysSystem.Tables[modOpportunity.strtCONSTRUCTION_STAGE].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Index(vntHomesiteConstructionStageId);
                                                object vntOptionConstructionStageId = RSysSystem.Tables[modOpportunity.strtDIVISION_PRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntDivProductId);
                                                int intOptionConstructionStageOrdinal = (int)RSysSystem.Tables[modOpportunity.strtCONSTRUCTION_STAGE].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Index(vntOptionConstructionStageId);
                                                if ((strConstructionStageComparison == modOpportunity.strsGREATER_THAN && intHomesiteConstructionStageOrdinal > intOptionConstructionStageOrdinal) ||
                                                    (strConstructionStageComparison == modOpportunity.strsGREATER_THAN_OR_EQUAL_TO && intHomesiteConstructionStageOrdinal >= intOptionConstructionStageOrdinal))
                                                {
                                                    rstRecordset.Fields[modOpportunity.strfUSE_POST_CUTOFF_PRICE].Value = true;
                                                    blnUsePCO = true;
                                                }
                                                else
                                                {
                                                    rstRecordset.Fields[modOpportunity.strfUSE_POST_CUTOFF_PRICE].Value = false;
                                                    blnUsePCO = false;
                                                }
                                            }
                                            else
                                            {
                                                rstRecordset.Fields[modOpportunity.strfUSE_POST_CUTOFF_PRICE].Value = false;
                                                blnUsePCO = false;
                                            }
                                        }
                                        // TL - consolidated price calculation algorithms
                                        rstRecordset.Fields[modOpportunity.strfPRICE].Value = GetQuoteOptionPrice(rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value,
                                            vntLocalNBHDPId, rstRecordset.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                                            rstOpportunity.Fields[modOpportunity.strfPOST_SALE_ID].Value, blnUsePCO);
                                    }

                                    // update the elevtion and elevation premium fields on the opp.
                                    if (selected)
                                    {
                                        if (TypeConvert.ToString(rstRecordset.Fields[modOpportunity.strfTYPE].Value) ==
                                            modOpportunity.strsELEVATION)
                                        {
                                            if (!(rstOpportunity.EOF))
                                            {
                                                rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value = rstRecordset.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value;
                                                rstOpportunity.Fields[modOpportunity.strfELEVATION_PREMIUM].Value = rstRecordset.Fields[modOpportunity.strfPRICE].Value;
                                                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstOpportunity);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (TypeConvert.ToString(rstRecordset.Fields[modOpportunity.strfTYPE].Value) ==
                                            modOpportunity.strsELEVATION)
                                        {
                                            rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strtOPPORTUNITY,
                                                modOpportunity.strfELEVATION_ID, modOpportunity.strfELEVATION_PREMIUM);
                                            if (rstOpportunity.RecordCount > 0)
                                            {
                                                rstOpportunity.MoveFirst();
                                                rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value = DBNull.Value;
                                                rstOpportunity.Fields[modOpportunity.strfELEVATION_PREMIUM].Value = 0;
                                                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstOpportunity);
                                            }
                                        }
                                    }
                                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY__PRODUCT, rstRecordset);

                                }
                                else if (selected)
                                {
                                    // DPK 11/2004 The NBHD Product Option Needs to be Added to the Opportunity Product.
                                    // Get returned record id - fpoulsen 06/07/2005
                                    //YK - Making sure that no 2 options are created forthe same products, in case of 2 options with the same 
                                    //     priority code (WC_Level_With_Plan)
                                    object divProdId = RSysSystem.Tables[modOpportunity.strt_NBHD_PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(opportunityId);
                                    if (!DataAccess.FindMatchInRecordset(objLib.GetRecordset(modOpportunity.strqSELECTED_OPTIONS_FOR_QUOTE,
                            1, opportunityId, modOpportunity.strfDIVISION_PRODUCT_ID), modOpportunity.strfDIVISION_PRODUCT_ID, vntLocalNBHDPId))
                                    {
                                        opportunityProductId = CreateOpportunityProductOption(opportunityId, vntLocalNBHDPId, null, quantity);
                                    }
                                }
                            }
                        }
                    
                        // RY: Issue 19305: When unselecting an option, also unselect all the included parent options
                        if (!selected)
                        {
                            strMsg = GetParentOptions(neighborhoodPhaseProductId, planNameId, DBNull.Value);
                            vntRetVal = strMsg.Split(new char[] { Convert.ToChar(";") });
                            for (int index = 0; index <= vntRetVal.GetUpperBound(0) - 1; ++index)
                            {
                                object vntLocalDivProdId = RSysSystem.StringToId(TypeConvert.ToString(vntRetVal[index]));
                                // get the record and set the selected to the boolean
                                if (TypeConvert.ToString(vntRetVal[index]).Length > 0)
                                {
                                    Recordset rstRecordset = objLib.GetRecordset(modOpportunity.strqSELECTED_OPTIONS_OPP_DIV_PROD,
                                        2, opportunityId, vntLocalDivProdId, modOpportunity.strf_SELECTED, modOpportunity.strfNET_CONFIG,
                                        modOpportunity.strfBUILD_OPTION, modOpportunity.strfOPTION_ADDED_BY, modOpportunity.strfPRICE, 
                                        modOpportunity.strfOPPORTUNITY__PRODUCT_ID, modOpportunity.strfQUANTITY, modOpportunity.strfTYPE, 
                                        modOpportunity.strfNBHDP_PRODUCT_ID, modOpportunity.strfPRICE, modOpportunity.strfUSE_POST_CUTOFF_PRICE, modOpportunity.strfOPTION_SELECTED_DATE);
                                    if (rstRecordset.RecordCount > 0)
                                    {
                                        rstRecordset.MoveFirst();
                                        if (TypeConvert.ToBoolean(rstRecordset.Fields[modOpportunity.strfBUILD_OPTION].Value))
                                        {
                                            // Cannot unselect a child with a built parent.  Cancel the entire unselect operation
                                            throw new PivotalApplicationException(TypeConvert.ToString(LangDict
                                                .GetText(modOpportunity.strdCANT_UNSELECT_CHILD_W_BUILT_PARENTS)),
                                                modOpportunity.glngPARENTS_ARE_BUILT, false);
                                        }

                                        rstRecordset.Fields[modOpportunity.strf_SELECTED].Value = false;
                                        rstRecordset.Fields[modOpportunity.strfOPTION_SELECTED_DATE].Value = DateTime.Now;
                                        UpdateChildOpportunityProducts(rstRecordset.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value, selected);
                                        //YK - Making sure that the "Post cut Off Price is set to False. If it needs 
                                        //     to set back to True, will be taken care of when "Selecting" it again.
                                        rstRecordset.Fields[modOpportunity.strfUSE_POST_CUTOFF_PRICE].Value = false;

                                        if (TypeConvert.ToString(rstRecordset.Fields[modOpportunity.strfTYPE].Value) ==
                                            modOpportunity.strsELEVATION)
                                        {
                                            rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strtOPPORTUNITY,
                                                modOpportunity.strfELEVATION_ID, modOpportunity.strfELEVATION_PREMIUM);
                                            if (rstOpportunity.RecordCount > 0)
                                            {
                                                rstOpportunity.MoveFirst();
                                                rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value = DBNull.Value;
                                                rstOpportunity.Fields[modOpportunity.strfELEVATION_PREMIUM].Value = 0;
                                                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstOpportunity);
                                            }
                                        }

                                        objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY__PRODUCT, rstRecordset);
                                    }
                                }
                            }
                        }

                    }
                }
                else
                {
                        // custom option
                        if ((opportunityProductId != DBNull.Value))
                        {
                            Recordset rstCustomOption = objLib.GetRecordset(opportunityProductId, modOpportunity.strtOPPORTUNITY__PRODUCT,
                                modOpportunity.strf_SELECTED, modOpportunity.strfNET_CONFIG, modOpportunity.strfOPTION_ADDED_BY, modOpportunity.strfOPTION_SELECTED_DATE);
                            if (rstCustomOption.RecordCount > 0)
                            {
                                rstCustomOption.MoveFirst();
                                rstCustomOption.Fields[modOpportunity.strf_SELECTED].Value = selected;
                                rstCustomOption.Fields[modOpportunity.strfOPTION_SELECTED_DATE].Value = DateTime.Now;
                                rstCustomOption.Fields[modOpportunity.strfOPTION_ADDED_BY].Value = vntEmployeeId;
                            }
                            objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstCustomOption);
                        }
                }  

                // Add Change Orders
                if (!((changeOrderId == null)))
                {
                    if (!(Convert.IsDBNull(changeOrderId)))
                    {
                        if ((neighborhoodPhaseProductId != DBNull.Value))
                        {
                           AddChangeOrders(vntRetVal, changeOrderId, opportunityId, (selected
                               ? ChangeOrderStatus.Selected : ChangeOrderStatus.Unselected));
                           // regular option
                        }
                        else
                        {
                            // must be custom option
                            // change the availablility flag on custom option order
                            Recordset rstOppProduct = objLib.GetRecordset(opportunityProductId, modOpportunity.strtOPPORTUNITY__PRODUCT,
                               modOpportunity.strfSELECTED, modOpportunity.strfNET_CONFIG, modOpportunity.strfFILTER_VISIBILITY, modOpportunity.strfOPTION_SELECTED_DATE);
                            if (rstOppProduct.RecordCount > 0)
                            {
                                rstOppProduct.MoveFirst();
                                rstOppProduct.Fields[modOpportunity.strfSELECTED].Value = selected;
                                rstOppProduct.Fields[modOpportunity.strfOPTION_SELECTED_DATE].Value = DateTime.Now;
                            }
                            objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstOppProduct);
                            AddChangeCustomOrders(opportunityProductId, changeOrderId, opportunityId, selected);
                        }
                    }
                }
                CalculateTotals(opportunityId, false);

                // check inventory quote rules
                InactivateCustomerQuotes(opportunityProductId, DBNull.Value, InactiveQuoteReason.NoReason);
                
                return string.Empty;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function updates the child opportunity products with selected flag
        /// </summary>
        /// <param name="vntOpportunityProductId">Opportunity Product Id</param>
        /// <param name="blnSelected">Flag value true or false</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 5.9.0.0        1/3/2007      BC         Updates the Child Opp Produts
        /// </history>
        protected virtual void UpdateChildOpportunityProducts(object vntOpportunityProductId,
            bool blnSelected)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOppPackageComponents = objLib.GetRecordset(modOpportunity.strqOPP_PRODUCT_FOR_PACKAGE,
                                    1, vntOpportunityProductId, modOpportunity.strfOPPORTUNITY__PRODUCT_ID,
                                    modOpportunity.strf_SELECTED, modOpportunity.strfOPTION_ADDED_BY, modOpportunity.strfOPTION_SELECTED_DATE);
                if (rstOppPackageComponents.RecordCount > 0)
                {
                    while (!rstOppPackageComponents.EOF)
                    {
                        rstOppPackageComponents.Fields[modOpportunity.strf_SELECTED].Value = blnSelected;
                        rstOppPackageComponents.Fields[modOpportunity.strfOPTION_SELECTED_DATE].Value = DateTime.Now;
                        rstOppPackageComponents.MoveNext();
                    }
                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY__PRODUCT, rstOppPackageComponents);
                }
                rstOppPackageComponents = null;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }

        }
        /// <summary>
        /// Load Additional NBHDP_Product Value
        /// </summary>
        /// <param name="recordsetArray"></param>
        /// <param name="code"></param>
        /// <param name="page"></param>
        /// <param name="pageCount"></param>
        /// <history>
        /// Revision       Date           Author   Description
        /// 3.8.0.0        5/12/2006      DYin     Converted to .Net C# code.
        /// 5.9.0.0        3/26/2007      YK       Made changes as per HB r5.9 release guidelines.
        /// </history>
        protected virtual void LoadNeighborhoodProducts(object[] recordsetArray, string code, ref int page, ref int pageCount)
        {
            try
            {
                const string SORT = "SELECT * FROM ({0}) AS t ORDER BY {1} ASC";

                SystemSetting systemSetting = (SystemSetting)RSysSystem.ServerScripts[AppServerRuleData.SystemSettingAppServerRuleName].CreateInstance();
                int intPaginateAt = TypeConvert.ToInt32(systemSetting.GetSystemSetting(modOpportunity.strfSELECT_OPTION_RECORDS_PER_PAGE));
                if (intPaginateAt== 0) intPaginateAt = 50;

                Recordset rstOpportunity = (Recordset) recordsetArray[0];
                Recordset rstProducts =  (Recordset) recordsetArray[2];
                Recordset rstNBHDP = (Recordset)recordsetArray[4];
                // sorts the pages by Option Name
                string strSQL = GetWildcardSql(rstOpportunity.Fields[modOpportunity.strf_NBHD_PHASE_ID].Value, rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value,
                    rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value, rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value,
                    false, code, false);
                string strSort = SORT.Replace("{1}", modOpportunity.strfDIVISION_PRODUCT_ID + ", " + modOpportunity.strfWC_LEVEL_WITH_PLAN);
                strSQL = strSort.Replace("{0}", strSQL);

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstNBHDPFull = new Recordset();
                rstNBHDPFull = objLib.GetRecordset(strSQL);
                
                if (rstNBHDPFull.RecordCount > 0)
                {
                    rstNBHDPFull.MoveFirst();
                    object vntRecordId = DBNull.Value;
                    
                    //YK - Making sure that all the Custome options are getting copied to the final recordset.
                    while (Convert.IsDBNull(vntRecordId) && !rstNBHDPFull.EOF)
                    {
                        vntRecordId = rstNBHDPFull.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;
                        rstNBHDP.AddNew(Type.Missing, Type.Missing);

                        foreach (Field oField in rstNBHDP.Fields)
                        {
                            if (oField.Name == modOpportunity.strfOPPORTUNITY_ID)
                            {
                                oField.Value = rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                            }
                            else if (oField.Name.Contains("Quantity"))
                            {
                                oField.Value = DBNull.Value;
                            }
                            else if (oField.Name.Contains("Icon_"))
                            {
                                oField.Value = true;
                            }
                            else if (oField.Name.Contains("Price"))
                            {
                                oField.Value = DBNull.Value;
                            }
                            else if ((oField.Name != "__Ordinal") && (!oField.Name.Contains("@Rn_Descriptor"))
                                && (!oField.Name.Contains("Rn_Edit_Date")) && (!oField.Name.Contains("Rn_Create_Date"))
                                && (!oField.Name.Contains("Rn_Edit_User")) && (!oField.Name.Contains("Rn_Create_User"))
                                && (!oField.Name.Contains("@Special")) && (!oField.Name.Contains("Icon_")))
                            {
                                oField.Value = rstNBHDPFull.Fields[oField.Name].Value;
                            }
                        }                       
                        rstNBHDPFull.MoveNext();
                    }

                    ////YK - Copy the first one always OR immediately after all the custom options.
                    //if (!rstNBHDPFull.EOF)
                    //{
                    //    //Save the Division_Product_Id
                    //    vntRecordId = rstNBHDPFull.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;

                    //    rstNBHDP.AddNew(Type.Missing, Type.Missing);
                    //    foreach (Field oField in rstNBHDP.Fields)
                    //    {
                    //        if (oField.Name == modOpportunity.strfOPPORTUNITY_ID)
                    //        {
                    //            oField.Value = rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                    //        }
                    //        else if (oField.Name.Contains("Quantity"))
                    //        {
                    //            oField.Value = DBNull.Value;
                    //        }
                    //        else if (oField.Name.Contains("Icon_"))
                    //        {
                    //            oField.Value = true;
                    //        }
                    //        else if (oField.Name.Contains("Price"))
                    //        {
                    //            oField.Value = DBNull.Value;
                    //        }
                    //        else if ((oField.Name != "__Ordinal") && (!oField.Name.Contains("@Rn_Descriptor"))
                    //            && (!oField.Name.Contains("Rn_Edit_Date")) && (!oField.Name.Contains("Rn_Create_Date"))
                    //            && (!oField.Name.Contains("Rn_Edit_User")) && (!oField.Name.Contains("Rn_Create_User"))
                    //            && (!oField.Name.Contains("@Special")) && (!oField.Name.Contains("Icon_")))
                    //        {
                    //            oField.Value = rstNBHDPFull.Fields[oField.Name].Value;
                    //        }
                    //    }
                    //    rstNBHDPFull.MoveNext();
                    //}
                    while (!rstNBHDPFull.EOF)
                    {
                        if (RSysSystem.IdToString(rstNBHDPFull.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value) != RSysSystem.IdToString(vntRecordId))
                        {
                            rstNBHDP.AddNew(Type.Missing, Type.Missing);
                            foreach (Field oField in rstNBHDP.Fields)
                            {
                                if (oField.Name == modOpportunity.strfOPPORTUNITY_ID)
                                {
                                    oField.Value = rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                                }
                                else if (oField.Name.Contains("Quantity"))
                                {
                                    oField.Value = DBNull.Value;
                                }
                                else if (oField.Name.Contains("Icon_"))
                                {
                                    oField.Value = true;
                                }
                                else if (oField.Name.Contains("Price"))
                                {
                                    oField.Value = DBNull.Value;
                                }
                                else if ((oField.Name != "__Ordinal") && (!oField.Name.Contains("@Rn_Descriptor"))
                                    && (!oField.Name.Contains("Rn_Edit_Date")) && (!oField.Name.Contains("Rn_Create_Date"))
                                    && (!oField.Name.Contains("Rn_Edit_User")) && (!oField.Name.Contains("Rn_Create_User"))
                                    && (!oField.Name.Contains("@Special")) && (!oField.Name.Contains("Icon_")))
                                {
                                    oField.Value = rstNBHDPFull.Fields[oField.Name].Value;
                                }
                            }
                            vntRecordId = rstNBHDPFull.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;
                        }
                        rstNBHDPFull.MoveNext();
                    }
                }
                rstNBHDP.Sort = "Type ASC, Product_Name ASC"; //Category_Id, Type, 
                // if records exists...
                if (rstNBHDP.RecordCount > 0)
                {
                    // sets up copying collection
                    rstNBHDP.PageSize = intPaginateAt;
                    pageCount = (rstNBHDP.PageCount);

                    // DY Fix Issue: We can not assign a integer to rstNBHDP.AbsolutePage as it's type is enumerator.
                    //               The solution is use Move to move the current location to the start location of the 
                    //               page. The following code claculates the start location and the end location for the
                    //               specified page. The code has the same functionality as that in VB.

                    //rstNBHDP.AbsolutePage = (PositionEnum) page;

                   // if (rstNBHDP.AbsolutePage == PositionEnum.adPosEOF)
                    //{
                    //    rstNBHDP.AbsolutePage = (PositionEnum) 1;
                    //}

                    //YK - Changed the beginning of the record index from "1" to "0"
                    int absoluteStartLocation = (page - 1) * intPaginateAt ; 
                    if (absoluteStartLocation < 0)
                    {
                        absoluteStartLocation = 0;
                    }
                    int absoluteEndLocation = absoluteStartLocation + intPaginateAt - 1;
                    rstNBHDP.Move(absoluteStartLocation, BookmarkEnum.adBookmarkFirst);
                    page = TypeConvert.ToInt32(rstNBHDP.AbsolutePage);

                    // copy all records
                    while (!((TypeConvert.ToInt32(rstNBHDP.AbsolutePage) != page) || rstNBHDP.EOF))
                    {
                        rstProducts.AddNew(Type.Missing, Type.Missing);

                        foreach (Field oField in rstProducts.Fields)
                        {
                            if (oField.Name == modOpportunity.strfOPPORTUNITY_ID)
                            {
                                oField.Value = rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                            }
                            else if (oField.Name.Contains("Quantity"))
                            {
                                oField.Value = DBNull.Value;
                            }
                            else if (oField.Name.Contains("Icon_"))
                            {
                                oField.Value = true;
                            }
                            else if (oField.Name.Contains("Price"))
                            {
                                oField.Value = DBNull.Value;
                            }
                            else if (oField.Name.Contains("Disconnected_"))
                            {
                                oField.Value = DBNull.Value;
                            }
                            else if ((oField.Name != "__Ordinal") && (!oField.Name.Contains("@Rn_Descriptor"))
                                && (!oField.Name.Contains("Rn_Edit_Date")) && (!oField.Name.Contains("Rn_Create_Date"))
                                && (!oField.Name.Contains("Rn_Edit_User")) && (!oField.Name.Contains("Rn_Create_User"))
                                && (!oField.Name.Contains("@Special")) && (!oField.Name.Contains("Icon_")))
                            {
                                oField.Value = rstNBHDP.Fields[oField.Name].Value;
                            }
                        }
                        rstNBHDP.MoveNext();
                    }
                    //rstNBHDP.Close();
                    rstProducts.Sort = "Type ASC, Product_Name ASC"; //Category_Id, Type, 
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Loads the excluded recordset on the selected options page.
        /// </summary>
        /// <history>
        /// Revision       Date           Author   Description
        /// 3.8.0.0        5/12/2006      DYin     Converted to .Net C# code.
        /// 5.9.0.0        Mar/06/2007     YK      Added the commented code for the exclusion enhancement to 
        ///                                        to view all the records which will get excluded due to the
        ///                                        prioirty (WC_Level_With_Plan) code at each NBHDP_Product level
        /// </history>
        protected virtual Recordset LoadExcludedProducts(object neighborhoodPhaseId, object neighborhoodId, object opportunityId, object
            planNameId, Recordset excludedRecordset)
        {
            try
            {                
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                string strSQL = GetWildcardSql(neighborhoodPhaseId, neighborhoodId, opportunityId, planNameId, false, string.Empty, true);

                Recordset rstNBHDP = objLib.GetRecordset(strSQL);

                // if records exists...
                if (rstNBHDP.RecordCount > 0)
                {
                    // copy all records
                    rstNBHDP.MoveFirst();
                    while (!(rstNBHDP.EOF))
                    {
                        excludedRecordset.AddNew(Type.Missing, Type.Missing);

                        foreach (Field oField in excludedRecordset.Fields)
                        {
                            if (oField.Name == modOpportunity.strfOPPORTUNITY_ID)
                            {
                                oField.Value = opportunityId;
                            }
                            else if (oField.Name.Contains("Icon_"))
                            {
                                oField.Value = true;
                            }
                            else if (oField.Name.Contains("Quantity"))
                            {
                                oField.Value = DBNull.Value;
                            }
                            else if ((oField.Name != "__Ordinal") && (!oField.Name.Contains("@Rn_Descriptor")) &&
                               (!oField.Name.Contains("Rn_Edit_Date")) && (!oField.Name.Contains("Rn_Float_Edit_Date")) &&
                               (!oField.Name.Contains("Rn_Create_Date")) && (!oField.Name.Contains("Rn_Edit_User")) &&
                               (!oField.Name.Contains("Rn_Create_User")) && (!oField.Name.Contains("@Special")))
                            {
                                oField.Value = rstNBHDP.Fields[oField.Name].Value;
                            }
                        }
                        rstNBHDP.MoveNext();
                    }
                    rstNBHDP.Close();
                }


                //YK - March 06, 2007 - Commenting this piece of code to append the extra records, for future 
                //     reference, in case this is required once again; post HB r5.9 project
                //YK - //YK - Adding all those records to excluded options, which were not available for selection
                //YK - //     due to the Geographical level priority.
                //YK - //YK - The Logic is simple, re-run the Query that is the one for Available Options, then remove 
                //YK - //     all the custom option froms this recordset and also, remove the first element in each group
                //YK - //     which is set as per Division_Product_Id, as this first entry would be the one which will show
                //YK - //     up for the Available Options, and the rest brother records will be discarded. This Discarded
                //YK - //     set needs to be assigned to the excludedRecordset as this would never be visisible to the 
                //YK - //     end customer.

                //YK - const string SORT = "SELECT * FROM ({0}) AS t ORDER BY {1} ASC";
                //YK - strSQL = GetWildcardSql(neighborhoodPhaseId, neighborhoodId, opportunityId, planNameId, false, string.Empty, false);

                //YK - string strSort = SORT.Replace("{1}", modOpportunity.strfDIVISION_PRODUCT_ID + ", " + modOpportunity.strfWC_LEVEL_WITH_PLAN);
                //YK - strSQL = strSort.Replace("{0}", strSQL);
                //YK - Recordset rstNBHDPIncluded = objLib.GetRecordset(strSQL);

                //YK - if (rstNBHDPIncluded.RecordCount > 0)
                //YK - {
                //YK -     rstNBHDPIncluded.MoveFirst();
                //YK -     //YK - Making sure that all the Custom options are taken off.
                //YK -     while (Convert.IsDBNull(rstNBHDPIncluded.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value) && !rstNBHDPIncluded.EOF)
                //YK -         rstNBHDPIncluded.MoveNext();
                    
                //YK -     object vntRecordId = rstNBHDPIncluded.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;

                //YK -     //YK - Moving to the next one, as the first one would always be available for selection. 
                //YK -     rstNBHDPIncluded.MoveNext();

                //YK -     while (!rstNBHDPIncluded.EOF)
                //YK -     {
                //YK -         if (mrsysSystem.IdToString(rstNBHDPIncluded.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value) == mrsysSystem.IdToString(vntRecordId))
                //YK -         {
                //YK -             excludedRecordset.AddNew(Type.Missing, Type.Missing);
                //YK -             foreach (Field oField in excludedRecordset.Fields)
                //YK -             {
                //YK -                 if (oField.Name == modOpportunity.strfOPPORTUNITY_ID)
                //YK -                 {
                //YK -                     oField.Value = opportunityId;
                //YK -                 }
                //YK -                 else if (oField.Name.Contains("Icon_"))
                //YK -                 {
                //YK -                     oField.Value = true;
                //YK -                }
                //YK -                else if (oField.Name.Contains("Quantity"))
                //YK -                {
                //YK -                    oField.Value = DBNull.Value;
                //YK -                }
                //YK -                else if ((oField.Name != "__Ordinal") && (!oField.Name.Contains("@Rn_Descriptor")) &&
                //YK -                   (!oField.Name.Contains("Rn_Edit_Date")) && (!oField.Name.Contains("Rn_Float_Edit_Date")) &&
                //YK -                   (!oField.Name.Contains("Rn_Create_Date")) && (!oField.Name.Contains("Rn_Edit_User")) &&
                //YK -                   (!oField.Name.Contains("Rn_Create_User")) && (!oField.Name.Contains("@Special")))
                //YK -                {
                //YK -                    oField.Value = rstNBHDPIncluded.Fields[oField.Name].Value;
                //YK -                }
                //YK -            }
                //YK -        }
                //YK -        else
                //YK -        {
                //YK -            vntRecordId = rstNBHDPIncluded.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;
                //YK -        }
                //YK -        rstNBHDPIncluded.MoveNext();
                //YK -    }
                //YK -    rstNBHDPIncluded.Close();
                //YK -}

                return excludedRecordset;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Get the Available Options Based on the Parameters PAssed in
        /// </summary>
        /// <param name="releaseId">Release Id </param>
        /// <param name="neighborhoodId">NEighborhood for WildCard</param>
        /// <param name="opportunityId">Opportunity to Exclude Options from.</param>
        /// <param name="planId">Plan Id</param>
        /// <param name="standard">Get ONLY standard Options</param>
        /// <param name="code">Code </param>
        /// <param name="excluded">Flag to indicate</param>
        /// <history>
        /// Revision       Date           Author   Description
        /// 3.8.0.0        5/12/2006      DYin     Converted to .Net C# code.
        /// 5.9.0.0        3/26/2007      YK       Changed a lot depending upon the HB r5.9 requirements.
        /// </history>
        protected virtual string GetWildcardSql(object releaseId, object neighborhoodId, object opportunityId, object
            planId, bool standard, string code, bool excluded)
        {
            try
            {
                // get oracle info
                string strSchema = string.Empty;
                if (RSysSystem.ServerBrand == CRServerBrand.SQL_BRAND_ORACLE) 
                {
                    strSchema = RSysSystem.UserSchema + ".";
                }

                // Get the Filter Values
                //AM2010.11.09 - Added code to get Construction Project from Opportunity so that we can lookup the construction project
                // related options
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strfFILTER_CATEGORY_ID,
                    modOpportunity.strfFILTER_SUB_CATEGORY_ID, modOpportunity.strfFILTER_CONSTRUCTION_STAGE_ONLY, modOpportunity.strfFILTER_CONSTRUCTION_STAGE_ID,
                    modOpportunity.strfFILTER_LOCATION_ID, modOpportunity.strfFILTER_MANUFACTURER, modOpportunity.strfFILTER_CODE_,
                    modOpportunity.strfELEVATION_ID, modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfTIC_CONSTRUCTION_PROJECT_ID);

                //YK - Read the Division and Region Information, rather than changing the paramter list.
                Recordset rstNeighborhood = objLib.GetRecordset(neighborhoodId, modOpportunity.strt_NEIGHBORHOOD, modOpportunity.strfDIVISION_ID,
                    modOpportunity.strfREGION_ID);
                object vntDivisionId = rstNeighborhood.Fields[modOpportunity.strfDIVISION_ID].Value;
                object vntRegionId = rstNeighborhood.Fields[modOpportunity.strfREGION_ID].Value;
                rstNeighborhood.Close();

                object vntCategory_Id = rstOpportunity.Fields[modOpportunity.strfFILTER_CATEGORY_ID].Value;
                bool vntFilterCSOnly = TypeConvert.ToBoolean(rstOpportunity.Fields[modOpportunity.strfFILTER_CONSTRUCTION_STAGE_ONLY].Value);
                object vntConstruction_Stage_Id = rstOpportunity.Fields[modOpportunity.strfFILTER_CONSTRUCTION_STAGE_ID].Value;
                object vntLocation_Id = rstOpportunity.Fields[modOpportunity.strfFILTER_LOCATION_ID].Value;
                string vntManufacturer = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfFILTER_MANUFACTURER].Value).Trim();
                string vntCode = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfFILTER_CODE_].Value).Trim();
                object vntElevation_Id = rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value;
                object vntOpportunity_Construction_Stage_id = rstOpportunity.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value;
                //AM2010.11.09 - Get construction project
                object vntConstructionProjectId = rstOpportunity.Fields[modOpportunity.strfTIC_CONSTRUCTION_PROJECT_ID].Value;


                //YK - Sub Category Filter.
                object vntSubCategoryId = rstOpportunity.Fields[modOpportunity.strfFILTER_SUB_CATEGORY_ID].Value;

                rstOpportunity.Close();
                int vntConstructionStageOrdinal = TypeConvert.ToInt32(RSysSystem.Tables[modOpportunity.strtCONSTRUCTION_STAGE]
                    .Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Index(vntOpportunity_Construction_Stage_id));

                // Issue 20343, faster loading for option form.
                // Cause no options to load until the user selects the apply filter.
                if (code.ToUpper() == "<filter load>".ToUpper())
                {
                    vntCode = "filter load";
                    vntCategory_Id = RSysSystem.StringToId("0x0000000000000000");
                }

                object vntDivPlanId = RSysSystem.Tables[modOpportunity.strt_NBHDP_PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(planId);

                // Get the system setting for construction stage comparison
                SystemSetting systemSetting = (SystemSetting)RSysSystem.ServerScripts[AppServerRuleData.SystemSettingAppServerRuleName].CreateInstance();
                string strConstructionComparision = TypeConvert.ToString(systemSetting.GetSystemSetting(modOpportunity.strfCONSTRUCTION_STAGE_COMPARISON));
                // default "Greater Than"
                if (strConstructionComparision.Length == 0) strConstructionComparision = modOpportunity.sGREATER_THAN;

                // Get the filter Construction Stage Ordinal
                object vntFilterConstructionOrdinal = RSysSystem.Tables[modOpportunity.strtCONSTRUCTION_STAGE].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Index(vntConstruction_Stage_Id);

                // start creating custom sql statement
                StringBuilder sqlText = new StringBuilder();
                sqlText.Append("SELECT");
                sqlText.Append(" 1 Product_Available,");
                sqlText.Append(" NBHDP_Product.Type,");
                sqlText.Append(" NBHDP_Product.Product_Name,");
                sqlText.Append(" Division_Product.Category_Id,");
                sqlText.Append(" Division_Product.Sub_Category_Id,");
                sqlText.Append(" NBHDP_Product.Code_,");
                sqlText.Append(" NBHDP_Product.Current_Price Price,");
                sqlText.Append(" NBHDP_Product.Location_Id,");
                sqlText.Append(" NBHDP_Product.Manufacturer,");
                sqlText.Append(" NULL Opportunity__Product_Id,");
                sqlText.Append(" NBHDP_Product.NBHDP_Product_Id,");
                sqlText.Append(" NBHDP_Product.Division_Product_Id,");
                sqlText.Append(" NBHDP_Product.Construction_Stage_Ordinal,");
                sqlText.Append(" NBHDP_Product.WC_Level_With_Plan,");
                sqlText.Append(" NBHDP_Product.Option_Available_To,");
                sqlText.Append(" NULL Quantity,");
                sqlText.Append(" Division_Product.Construction_Stage_Id,");
                sqlText.Append(" Division_Product.Required_Deposit_Amount,");
                sqlText.Append(" 0 Selected,");
                sqlText.Append(" 0 Use_PCO_Price,");
                sqlText.Append(" NBHDP_Product.Post_CuttOff_Price,");
                sqlText.Append(" NBHDP_Product.Inactive");
                sqlText.Append(" FROM " + strSchema + "NBHDP_Product");
                sqlText.Append(" LEFT JOIN Division_Product Division_Product ON NBHDP_Product.Division_Product_Id = Division_Product.Division_Product_Id");
                sqlText.Append(" WHERE (NBHDP_Product.Inactive = 0 OR NBHDP_Product.Inactive IS NULL)");
                //AM2010.09.07 - Added so that Decorator options integrated
                //from Chateau are not available for selection in the Option Selection form
                sqlText.Append(" AND (NBHDP_PRODUCT.TYPE <> 'Decorator')");

                 // YK The entire Wildcarding related checks done in here with the help of WC_* fields.
                sqlText.Append(" AND ( ");
                //All Corporate Specific
                sqlText.Append(" NBHDP_Product.WC_Corporate = 1 ");
                //All Region Specific but Division Wildcarded
                sqlText.Append(" OR NBHDP_Product.WC_Region_Id = " + RSysSystem.IdToString(vntRegionId));
                //All Division Specific but Neighborhood Wildcarded
                sqlText.Append(" OR NBHDP_Product.WC_Division_Id = " + RSysSystem.IdToString(vntDivisionId));
                
                //AM2010.11.09 - For Irvine, all options are now at the Construction Project level, so need
                //to use this in our filter and comment out neighborhood filter
                //All Neighborhood Specific but Release Wildcarded
                //sqlText.Append(" OR NBHDP_Product.WC_Neighborhood_Id = " + RSysSystem.IdToString(neighborhoodId));
                sqlText.Append(" OR NBHDP_Product.TIC_WC_Construction_Project_Id = " + RSysSystem.IdToString(vntConstructionProjectId));

                //All Release Specific
                sqlText.Append(" OR NBHDP_Product.NBHD_Phase_Id = " + RSysSystem.IdToString(releaseId));
                sqlText.Append(" ) ");

                // Determine Global Or Plan defined or Plan Code wildcarded
                sqlText.Append(" AND (NBHDP_Product.Type = 'Global'");
                sqlText.Append(" OR (NBHDP_Product.Plan_Code = (SELECT Plan_Code FROM NBHDP_Product Plan_Table WHERE Plan_Table.NBHDP_Product_Id = ");
                sqlText.Append(RSysSystem.IdToString(planId));
                sqlText.Append(") AND NBHDP_Product.Plan_Id IS NULL)");
                sqlText.Append(" OR (NBHDP_Product.Plan_Code IS NULL AND NBHDP_Product.Plan_Id = " + RSysSystem.IdToString(planId) + ")");
                sqlText.Append(" OR (NBHDP_Product.Plan_Code IS NULL AND NBHDP_Product.Plan_Id IS NULL)");
                sqlText.Append(" ) ");

                // Ensure that the Location is either null or Location is defined for that Plan
                // Added for Division Information as well.
                sqlText.Append(" AND (NBHDP_Product.Location_Id IS NULL");
                sqlText.Append(" OR (NBHDP_Product.Location_Id is not null");
                sqlText.Append(" AND NBHDP_Product.Location_Id in");
                sqlText.Append(" (SELECT Division_Product_Locations.Location_Id FROM Division_Product_Locations");
                sqlText.Append(" WHERE Division_Product_Locations.Division_Product_Id = " + RSysSystem.IdToString(vntDivPlanId));
                sqlText.Append(" AND ( ");
                sqlText.Append(" Division_Product_Locations.Division_Id IS NULL OR ");
                sqlText.Append(" Division_Product_Locations.Division_Id = " + RSysSystem.IdToString(vntDivisionId));
                sqlText.Append(" ) ");
                sqlText.Append(" AND (Division_Product_Locations.Inactive = 0 OR Division_Product_Locations.Inactive IS NULL)");
                sqlText.Append(" ) ) )");

                sqlText.Append(" AND NOT (NBHDP_Product.Type = 'Plan')");

                // Make sure the option is currently Available
                // strSQL = strSQL & " and  AND (ISNULL(NBHDP_Product.Removal_Date, GETDATE()) >= GETDATE()) "
                sqlText.Append(" AND (NBHDP_Product.Available_Date <= GETDATE() OR NBHDP_Product.Available_Date IS NULL)");
                sqlText.Append(" AND (NBHDP_Product.Removal_Date >= GETDATE() OR NBHDP_Product.Removal_Date IS NULL)");

                // Determine that the NBHDP_Product is already been added to the Opportunity_Product
                // YK - Please note that due to addition of Packages and it's Component's Opportunity Records, 
                // Will have the Division_Product_Id populated but the NBHDP_Product_Id as NULL. In case of 
                // Custom Options both these fields will be NULL. 
                // YK - In this test, we do not want any of the Custome options or the [Package] component's
                // Opportunity_Product record to show up in here, hence just comparing the NBHDP_Product_Id to NULL.
                sqlText.Append(" AND NBHDP_Product.NBHDP_Product_Id NOT IN (SELECT Opportunity__Product.NBHDP_Product_Id FROM Opportunity__Product WHERE Opportunity__Product.Opportunity_Id = ");
                sqlText.Append(RSysSystem.IdToString(opportunityId));
                sqlText.Append(" AND NOT Opportunity__Product.NBHDP_Product_Id IS NULL )");

                
                if (standard)
                {
                    sqlText.Append(" AND NBHDP_Product.Default_Product = 1 AND (NBHDP_Product.Inactive IS NULL OR NBHDP_Product.Inactive = 0)");
                }

                // Filter The Options based on the selected Values
                if (!(Convert.IsDBNull(vntCategory_Id)))
                {
                    sqlText.Append(" AND Division_Product.Category_Id = " + RSysSystem.IdToString(vntCategory_Id));
                }

                //YK - New Filter Criteria, The Sub Category field.
                if (!(Convert.IsDBNull(vntSubCategoryId)))
                {
                    sqlText.Append(" AND Division_Product.Sub_Category_Id = " + RSysSystem.IdToString(vntSubCategoryId));
                }

                if (!(Convert.IsDBNull(vntConstruction_Stage_Id)))
                {
                    if (vntFilterCSOnly)
                    {
                        sqlText.Append(" AND " + "Division_Product.Construction_Stage_Id = " + RSysSystem.IdToString(vntConstruction_Stage_Id));
                    }
                }
                                                
                if (!(Convert.IsDBNull(vntFilterConstructionOrdinal)))
                {
                    // Else ' get all with greater than or equal to the ordinal
                    sqlText.Append(" AND " + "NBHDP_Product.Construction_Stage_Ordinal >= " + TypeConvert.ToString(vntFilterConstructionOrdinal));
                }

                if (!(Convert.IsDBNull(vntLocation_Id)))
                {
                    sqlText.Append(" AND NBHDP_Product.Location_Id = " + RSysSystem.IdToString(vntLocation_Id));
                }

                if (vntManufacturer.Length > 0)
                {
                    sqlText.Append(" AND NBHDP_Product.Manufacturer = '" + TypeConvert.ToString(vntManufacturer).Trim() + "'");
                }

                if (vntCode.Length > 0)
                {
                    sqlText.Append(" AND NBHDP_Product.Code_ LIKE '" + TypeConvert.ToString(vntCode).Trim() + "'");
                }

                // Need To Handle the Excluded
                // RY
                sqlText.Append(" AND (");
                if (excluded)
                {
                    // check to see if elevation is currently selected
                    if (!(Convert.IsDBNull(vntElevation_Id)))
                    {
                        vntElevation_Id = RSysSystem.IdToString(vntElevation_Id);
                        sqlText.Append(" (NBHDP_Product.Type = 'Elevation' And NBHDP_Product.NBHDP_Product_Id <> " + vntElevation_Id + ")");
                        sqlText.Append(" OR");
                    }
                    sqlText.Append(" NOT (");
                }
                else
                {
                    if (!(Convert.IsDBNull(vntElevation_Id)))
                    {
                        sqlText.Append("(NBHDP_Product.Type <> 'Elevation' AND (");
                    }
                    else
                    {
                        sqlText.Append("(NBHDP_Product.Type = 'Elevation' AND (");

                        sqlText.Append("NBHDP_Product.Division_Product_Id");
                        sqlText.Append(" NOT");
                        sqlText.Append(" IN (SELECT Product_Option_Rule.Child_Product_Id");
                        sqlText.Append(" FROM Product_Option_Rule INNER JOIN Opportunity__Product OP ON OP.Division_Product_Id = Product_Option_Rule.Parent_Product_Id");
                        sqlText.Append(" WHERE Opportunity_Id = " + RSysSystem.IdToString(opportunityId));
                        sqlText.Append(" AND Product_Option_Rule.Exclude = 1");
                        sqlText.Append(" AND (Product_Option_Rule.Inactive = 0 OR Product_Option_Rule.Inactive IS NULL)");
                        sqlText.Append(" AND OP.Selected = 1");
                        sqlText.Append(" AND OP.Opportunity_Id = " + RSysSystem.IdToString(opportunityId));
                        sqlText.Append(" AND NOT OP.NBHDP_Product_Id IS NULL ");
                        sqlText.Append(" AND (Product_Option_Rule.Plan_Product_Id = " + RSysSystem.IdToString(vntDivPlanId));
                        //YK - March 22, 2007, The most appropriate rule, if plan specific, it takes the precedence, else, the generic one.
                        sqlText.Append(" OR ( ");
                        sqlText.Append(" Product_Option_Rule.Plan_Product_Id IS NULL AND Product_Option_Rule.Child_Product_Id NOT ");
                        sqlText.Append(" IN (SELECT Product_Option_Rule.Child_Product_Id");
                        sqlText.Append(" FROM Product_Option_Rule INNER JOIN Opportunity__Product OP ON OP.Division_Product_Id = Product_Option_Rule.Parent_Product_Id");
                        sqlText.Append(" WHERE Opportunity_Id = " + RSysSystem.IdToString(opportunityId));
                        sqlText.Append(" AND (Product_Option_Rule.Inactive = 0 OR Product_Option_Rule.Inactive IS NULL)");
                        sqlText.Append(" AND OP.Selected = 1");
                        sqlText.Append(" AND Product_Option_Rule.Plan_Product_Id = " + RSysSystem.IdToString(vntDivPlanId));
                        sqlText.Append(" ) )");
                        sqlText.Append(" ) ");

                        //sqlText.Append(" OR Product_Option_Rule.Plan_Product_Id IS NULL)");
                        sqlText.Append(" ) )");
                        sqlText.Append(" AND (");
                    }


                    // Get the Opportunities Construction Stage Ordinal
                    if (Convert.IsDBNull(vntOpportunity_Construction_Stage_id) || vntConstructionStageOrdinal == 0)
                    {
                        sqlText.Append(" ( -1");
                    }
                    else
                    {
                        sqlText.Append(" ( ");
                        sqlText.Append(vntConstructionStageOrdinal.ToString());
                    }

                    if (!excluded)
                    {
                        if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                        {
                            sqlText.Append(" <=");
                        }
                        else
                        {
                            sqlText.Append(" <");
                        }
                    }
                    else
                    {
                        if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                        {
                            sqlText.Append(" >");
                        }
                        else
                        {
                            sqlText.Append(" >=");
                        }
                    }

                    // NBHD_Product_Option.Construction > The Construction Stage of the Lot.
                    sqlText.Append(" ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000) ");

                    //YK - Getting all the records whose Construction stage has surpassed 
                    //     but they have a Post Cut Off Price attached to them 

                    sqlText.Append(" OR  (");

                    if (Convert.IsDBNull(vntOpportunity_Construction_Stage_id) || vntConstructionStageOrdinal == 0)
                    {
                        sqlText.Append("-1");
                    }
                    else
                    {
                        sqlText.Append(vntConstructionStageOrdinal.ToString());
                    }

                    if (excluded)
                    {
                        if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                        {
                            sqlText.Append(" <=");
                        }
                        else
                        {
                            sqlText.Append(" <");
                        }
                        sqlText.Append(" ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000) ");
                        sqlText.Append(" AND NBHDP_Product.Post_CuttOff_Price IS NULL ) )");
                    }
                    else
                    {
                        if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                        {
                            sqlText.Append(" >");
                        }
                        else
                        {
                            sqlText.Append(" >=");
                        }
                        sqlText.Append(" ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000) ");
                        sqlText.Append(" AND NBHDP_Product.Post_CuttOff_Price IS NOT NULL ) )");
                    }

                    sqlText.Append(" )");
                    sqlText.Append(" )");
                    if (!(Convert.IsDBNull(vntElevation_Id)))
                    {
                        sqlText.Append("AND (");
                    }
                    else
                    {
                        sqlText.Append("OR (");
                    }
                }  
                
                //YK - Option Rules have been moved one level up to table named Product_Option_Rule
                sqlText.Append("(NBHDP_Product.Division_Product_Id");
                sqlText.Append(" NOT");
                sqlText.Append(" IN (SELECT Product_Option_Rule.Child_Product_Id");
                sqlText.Append(" FROM Product_Option_Rule INNER JOIN Opportunity__Product OP ON OP.Division_Product_Id = Product_Option_Rule.Parent_Product_Id");
                sqlText.Append(" WHERE Opportunity_Id = " + RSysSystem.IdToString(opportunityId));
                sqlText.Append(" AND Product_Option_Rule.Exclude = 1");
                sqlText.Append(" AND (Product_Option_Rule.Inactive = 0 OR Product_Option_Rule.Inactive IS NULL)");
                sqlText.Append(" AND OP.Selected = 1");
                sqlText.Append(" AND OP.Opportunity_Id = " + RSysSystem.IdToString(opportunityId));
                sqlText.Append(" AND NOT OP.NBHDP_Product_Id IS NULL ");
                sqlText.Append(" AND (Product_Option_Rule.Plan_Product_Id = " + RSysSystem.IdToString(vntDivPlanId));

                //YK - March 22, 2007, The most appropriate rule, if plan specific, it takes the precedence, else, the generic one.
                sqlText.Append(" OR ( ");
                sqlText.Append(" Product_Option_Rule.Plan_Product_Id IS NULL AND Product_Option_Rule.Child_Product_Id NOT ");
                sqlText.Append(" IN (SELECT Product_Option_Rule.Child_Product_Id");
                sqlText.Append(" FROM Product_Option_Rule INNER JOIN Opportunity__Product OP ON OP.Division_Product_Id = Product_Option_Rule.Parent_Product_Id");
                sqlText.Append(" WHERE Opportunity_Id = " + RSysSystem.IdToString(opportunityId));
                sqlText.Append(" AND (Product_Option_Rule.Inactive = 0 OR Product_Option_Rule.Inactive IS NULL)");
                sqlText.Append(" AND OP.Selected = 1");
                sqlText.Append(" AND Product_Option_Rule.Plan_Product_Id = " + RSysSystem.IdToString(vntDivPlanId));
                sqlText.Append(" ) )");
 
                //sqlText.Append(" OR Product_Option_Rule.Plan_Product_Id IS NULL)");

                sqlText.Append(" ))");

                // RY
                sqlText.Append(" AND (");
                if (!(Convert.IsDBNull(vntElevation_Id)))
                {
                    sqlText.Append(" NBHDP_Product.Type <> 'Elevation' AND ");
                }
                sqlText.Append(" NBHDP_Product.Division_Product_Id NOT IN ( SELECT Opportunity__Product.Division_Product_Id FROM Opportunity__Product WHERE Opportunity__Product.Opportunity_Id = ");
                sqlText.Append(RSysSystem.IdToString(opportunityId));
                sqlText.Append(" AND Opportunity__Product.selected = 1 AND NOT Opportunity__Product.NBHDP_Product_Id IS NULL)))");

                // Need To Handle the Exclusion of Options where the Construction Stage of the Opportunity(Lot) is Less
                // than the Option's Construction Stage Ordinal
                //YK-EOO - Fixing up the Excluded Options, Operation (EOO).
                //YK-EOO - if (!excluded)
                //YK-EOO - {
                    sqlText.Append(" AND ");
                //YK-EOO - }
                //YK-EOO - else
                //YK-EOO - {
                //YK-EOO -     sqlText.Append(" OR ");
                //YK-EOO - }

                // Get the Opportunities Construction Stage Ordinal
                if (Convert.IsDBNull(vntOpportunity_Construction_Stage_id) || vntConstructionStageOrdinal == 0)
                {
                    sqlText.Append(" ( -1");
                }
                else
                {
                    sqlText.Append(" ( " + vntConstructionStageOrdinal.ToString());
                }

                //YK-EOO - if (!excluded)
                //YK-EOO - {
                    if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                    {
                        sqlText.Append(" <=");
                    }
                    else
                    {
                        sqlText.Append(" <");
                    }
                //YK-EOO - }
                //YK-EOO - else
                //YK-EOO - {
                //YK-EOO -     if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                //YK-EOO -     {
                //YK-EOO -         sqlText.Append(" >");
                //YK-EOO -     }
                //YK-EOO -     else
                //YK-EOO -     {
                //YK-EOO -         sqlText.Append(" >=");
                //YK-EOO -     }
                //YK-EOO - }

                // NBHD_Product_Option.Construction > The Construction Stage of the Lot.
                sqlText.Append(" ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000)  ");

                //YK - Getting all the records whose Construction stage has surpassed 
                //     but they have a Post Cut Off Price attached to them 

                sqlText.Append(" OR ( ");

                if (Convert.IsDBNull(vntOpportunity_Construction_Stage_id) || vntConstructionStageOrdinal == 0)
                {
                    sqlText.Append("-1");
                }
                else
                {
                    sqlText.Append(vntConstructionStageOrdinal.ToString());
                }

                //YK-EOO - if (excluded)
                //YK-EOO - {
                //YK-EOO -     if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                //YK-EOO -     {
                //YK-EOO -         sqlText.Append(" <=");
                //YK-EOO -     }
                //YK-EOO -     else
                //YK-EOO -     {
                //YK-EOO -         sqlText.Append(" <");
                //YK-EOO -     }
                //YK-EOO -     sqlText.Append(" ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000) ");
                //YK-EOO -     sqlText.Append(" AND NBHDP_Product.Post_CuttOff_Price IS NULL ) )");
                //YK-EOO - }
                //YK-EOO - else
                //YK-EOO - {
                    if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                    {
                        sqlText.Append(" >");
                    }
                    else
                    {
                        sqlText.Append(" >=");
                    }
                    sqlText.Append(" ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000) ");
                    sqlText.Append(" AND NBHDP_Product.Post_CuttOff_Price IS NOT NULL ) )");
                //YK-EOO - }

                sqlText.Append(" ) )");

                // We do not want to Add the Standard Options Twice
                if (!standard)
                {
                    // Union in all the Opportunity Product Records that are Currently NOT Selected.
                    sqlText.Append(" UNION");
                    sqlText.Append(" SELECT");
                    sqlText.Append(" 1 Product_Available,");
                    sqlText.Append(" OP0.Type,");
                    sqlText.Append(" OP0.Product_Name,");
                    sqlText.Append(" Division_Product.Category_Id,");
                    sqlText.Append(" Division_Product.Sub_Category_Id,");
                    sqlText.Append(" OP0.Code_,");
                    sqlText.Append(" OP0.Price,");
                    sqlText.Append(" OP0.Location_Id,");
                    sqlText.Append(" OP0.Manufacturer,");
                    sqlText.Append(" OP0.Opportunity__Product_Id,");
                    sqlText.Append(" OP0.NBHDP_Product_Id,");
                    sqlText.Append(" OP0.Division_Product_Id,");
                    sqlText.Append(" OP0.Construction_Stage_Ordinal,");
                    sqlText.Append(" NBHDP_Product.WC_Level_With_Plan,");
                    sqlText.Append(" NBHDP_Product.Option_Available_To,");
                    sqlText.Append(" NULL Quantity,");
                    sqlText.Append(" Division_Product.Construction_Stage_Id,");
                    sqlText.Append(" Division_Product.Required_Deposit_Amount,");
                    sqlText.Append(" 0 Selected,");
                    sqlText.Append(" 0 Use_PCO_Price,");
                    sqlText.Append(" NBHDP_Product.Post_CuttOff_Price,");
                    sqlText.Append(" NBHDP_Product.Inactive");
                    sqlText.Append(" FROM " + strSchema + "Opportunity__Product OP0");
                    sqlText.Append(" LEFT OUTER JOIN Division_Product Division_Product ON OP0.Division_Product_Id = Division_Product.Division_Product_Id");
                    sqlText.Append(" LEFT OUTER JOIN Construction_Stage ON OP0.Construction_Stage_Id = Construction_Stage.Construction_Stage_Id");
                    sqlText.Append(" LEFT OUTER JOIN NBHDP_Product NBHDP_Product ON OP0.NBHDP_Product_Id = NBHDP_Product.NBHDP_Product_Id");
                    sqlText.Append(" WHERE Opportunity_Id = " + RSysSystem.IdToString(opportunityId));
                    sqlText.Append(" AND OP0.Selected = 0");
                    sqlText.Append(" AND (NBHDP_Product.Inactive = 0 OR NBHDP_Product.Inactive IS NULL)");

                    //YK - Choose only the Custom Options and no Package Component specific Option records should be seleted
                    if (!excluded)
                    {
                        sqlText.Append(" AND ( (OP0.Division_Product_Id IS NOT NULL AND OP0.NBHDP_Product_Id IS NOT NULL) ");
                        sqlText.Append(" OR (OP0.Division_Product_Id IS NULL AND OP0.NBHDP_Product_Id IS NULL) )");
                    }

                    // RY:
                    sqlText.Append(" AND (");

                    if (excluded)
                    {

                        // check to see if elevation is currently selected
                        if (!(Convert.IsDBNull(vntElevation_Id)))
                        {
                            sqlText.Append(" (OP0.Type = 'Elevation' And OP0.NBHDP_Product_Id <> ");
                            sqlText.Append(RSysSystem.IdToString(vntElevation_Id));
                            sqlText.Append(") OR ");
                        }
                        //else
                        //{
                        //    sqlText.Append(" (OP0.Type <> 'Elevation') AND");
                        //}
                        //YK - Taking care of the Package Component records. No Custom Options, as in Exclude, the 
                        //     Custom option will never be participating.
                        sqlText.Append("( OP0.NBHDP_Product_Id is not NULL AND OP0.Division_Product_Id is not NULL) AND NOT ");
                        sqlText.Append(" ( ");
                    }
                    else
                    {
                        //YK - Taking care of the Package Component records along with the Custom Options
                        sqlText.Append(" ( OP0.NBHDP_Product_Id is NULL AND OP0.Division_Product_Id is NULL)  OR");
                        sqlText.Append(" ( ");
                        if (!(Convert.IsDBNull(vntElevation_Id)))
                        {
                            sqlText.Append(" OP0.Type <> 'Elevation' AND ");
                        }
                        //else
                        //{
                        //    sqlText.Append(" (OP0.Type = 'Elevation') OR");
                        //}
                    }

                    //YK - Option Rules have been moved one level up to table named Product_Option_Rule
                    sqlText.Append(" ((OP0.Division_Product_Id");
                    sqlText.Append(" NOT");
                    sqlText.Append(" IN (SELECT Product_Option_Rule.Child_Product_Id");
                    sqlText.Append(" FROM Product_Option_Rule INNER JOIN Opportunity__Product OP ON OP.Division_Product_Id = Product_Option_Rule.Parent_Product_Id");
                    sqlText.Append(" WHERE Opportunity_Id = " + RSysSystem.IdToString(opportunityId));
                    sqlText.Append(" AND Product_Option_Rule.Exclude = 1");
                    sqlText.Append(" AND (Product_Option_Rule.Inactive = 0 OR Product_Option_Rule.Inactive IS NULL)");
                    sqlText.Append(" AND OP.Selected = 1");
                    sqlText.Append(" AND OP.Opportunity_Id = " + RSysSystem.IdToString(opportunityId));
                    sqlText.Append(" AND NOT OP.NBHDP_Product_Id IS NULL ");
                    sqlText.Append(" AND (Product_Option_Rule.Plan_Product_Id = " + RSysSystem.IdToString(vntDivPlanId));

                    //YK - March 22, 2007, The most appropriate rule, if plan specific, it takes the precedence, else, the generic one.
                    sqlText.Append(" OR ( ");
                    sqlText.Append(" Product_Option_Rule.Plan_Product_Id IS NULL AND Product_Option_Rule.Child_Product_Id NOT ");
                    sqlText.Append(" IN (SELECT Product_Option_Rule.Child_Product_Id");
                    sqlText.Append(" FROM Product_Option_Rule INNER JOIN Opportunity__Product OP ON OP.Division_Product_Id = Product_Option_Rule.Parent_Product_Id");
                    sqlText.Append(" WHERE Opportunity_Id = " + RSysSystem.IdToString(opportunityId));
                    sqlText.Append(" AND (Product_Option_Rule.Inactive = 0 OR Product_Option_Rule.Inactive IS NULL)");
                    sqlText.Append(" AND OP.Selected = 1");
                    sqlText.Append(" AND Product_Option_Rule.Plan_Product_Id = " + RSysSystem.IdToString(vntDivPlanId));
                    sqlText.Append(" ) )");
                    sqlText.Append(" ) ");
                    //sqlText.Append(" OR Product_Option_Rule.Plan_Product_Id IS NULL)");
                    sqlText.Append(" )");


                    // RY
                    sqlText.Append(" AND (OP0.Division_Product_Id NOT IN (SELECT OP2.Division_Product_Id FROM Opportunity__Product OP2 WHERE OP2.Opportunity_Id = ");
                    sqlText.Append(RSysSystem.IdToString(opportunityId));
                    sqlText.Append(" AND OP2.selected = 1 AND NOT OP2.NBHDP_Product_Id IS NULL ) )");
                    sqlText.Append(" )");
                    sqlText.Append(" ) ");

                    // BA Issue #19163 Commented out and moved to above because it was not excluding the options correctly
                    // Need To Handle the Exclusion of Options where the Construction Stage of the Opportunity(Lot) is Less
                    // than the Option's Construction Stage Ordinal
                    //YK-EOO - if (!excluded)
                    //YK-EOO - {
                        sqlText.Append(" AND (");
                    //YK-EOO - }
                    //YK-EOO - else
                    //YK-EOO - {
                    //YK-EOO -     sqlText.Append(" OR (");
                    //YK-EOO - }

                    if (Convert.IsDBNull(vntOpportunity_Construction_Stage_id) || vntConstructionStageOrdinal == 0)
                    {
                        sqlText.Append(" ( -1");
                    }
                    else
                    {
                        sqlText.Append(" ( " + vntConstructionStageOrdinal);
                    }

                    //YK-EOO - if (!excluded)
                    //YK-EOO - {
                        if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                        {
                            sqlText.Append(" <=");
                        }
                        else
                        {
                            sqlText.Append(" <");
                        }
                    //YK-EOO - }
                    //YK-EOO - else
                    //YK-EOO - {
                    //YK-EOO -     if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                    //YK-EOO -     {
                    //YK-EOO -         sqlText.Append(" >");
                    //YK-EOO -     }
                    //YK-EOO -     else
                    //YK-EOO -     {
                    //YK-EOO -         sqlText.Append(" >=");
                    //YK-EOO -     }
                    //YK-EOO - }

                    // NBHD_Product_Option.Construction > The Construction Stage of the Lot.
                    sqlText.Append(" ISNULL(Construction_Stage.Construction_Stage_Ordinal, 1000000000)");

                    //YK - Getting all the records whose Construction stage has surpassed 
                    //     but they have a Post Cut Off Price attached to them 

                    sqlText.Append(" OR ( ");

                    if (Convert.IsDBNull(vntOpportunity_Construction_Stage_id) || vntConstructionStageOrdinal == 0)
                    {
                        sqlText.Append("-1");
                    }
                    else
                    {
                        sqlText.Append(vntConstructionStageOrdinal.ToString());
                    }

                    //YK-EOO -if (excluded)
                    //YK-EOO -{
                    //YK-EOO -    if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                    //YK-EOO -    {
                    //YK-EOO -        sqlText.Append(" <=");
                    //YK-EOO -    }
                    //YK-EOO -    else
                    //YK-EOO -    {
                    //YK-EOO -        sqlText.Append(" <");
                    //YK-EOO -    }
                    //YK-EOO -    sqlText.Append(" ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000) ");
                    //YK-EOO -    sqlText.Append(" AND NBHDP_Product.Post_CuttOff_Price IS NULL ) )");
                    //YK-EOO -}
                    //YK-EOO -else
                    //YK-EOO -{
                        if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                        {
                            sqlText.Append(" >");
                        }
                        else
                        {
                            sqlText.Append(" >=");
                        }
                        sqlText.Append(" ISNULL(NBHDP_Product.Construction_Stage_Ordinal, 1000000000) ");
                        sqlText.Append(" AND NBHDP_Product.Post_CuttOff_Price IS NOT NULL ) )");
                    //YK-EOO -}
                    sqlText.Append(" )) )");

                    // Filter The Options based on the selected Values
                    if (!(Convert.IsDBNull(vntCategory_Id)))
                    {
                        sqlText.Append(" AND Division_Product.Category_Id = " + RSysSystem.IdToString(vntCategory_Id));
                    }

                    //YK - New Filter Criteria, The Sub Category field.
                    if (!(Convert.IsDBNull(vntSubCategoryId)))
                    {
                        sqlText.Append(" AND Division_Product.Sub_Category_Id = " + RSysSystem.IdToString(vntSubCategoryId));
                    }

                    if (!(Convert.IsDBNull(vntConstruction_Stage_Id)))
                    {
                        if (vntFilterCSOnly)
                        {
                            sqlText.Append(" AND Division_Product.Construction_Stage_Id = " + RSysSystem.IdToString(vntConstruction_Stage_Id));
                        }
                    }

                    if (!(Convert.IsDBNull(vntFilterConstructionOrdinal)))
                    {
                        sqlText.Append(" AND OP0.Construction_Stage_Ordinal >= " + TypeConvert.ToString(vntFilterConstructionOrdinal));
                    }

                    if (!(Convert.IsDBNull(vntLocation_Id)))
                    {
                        sqlText.Append(" AND OP0.Location_Id = " + RSysSystem.IdToString(vntLocation_Id));
                    }

                    if (vntManufacturer.Length > 0)
                    {
                        sqlText.Append(" AND OP0.Manufacturer = '"+ TypeConvert.ToString(vntManufacturer).Trim() + "'");
                    }

                    if (vntCode.Length > 0)
                    {
                        sqlText.Append(" AND OP0.Code_ LIKE '" + TypeConvert.ToString(vntCode).Trim()+ "'");
                    }
                }
                return sqlText.ToString();
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Add the Standard Options to the Opportunity Product
        /// </summary>
        /// <param name="releaseId">Release Id</param>
        /// <param name="neighborhoodId">Neighborhood Id</param>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="planId">Plan Id</param>
        /// <history>
        /// Revision       Date           Author   Description
        /// 3.8.0.0        5/12/2006      DYin     Converted to .Net C# code.
        /// 5.9.0.0        3/26/2007      YK       Changed the code to accomodate the selection of the most appropriate
        ///                                        Product Configuration record depending upon the priority. Further
        ///                                        there was a bug in the system, that if the same product has multiple
        ///                                        "Standard" product Configuration records, all of them were getting
        ///                                        added. Have fixed this as well due to the above mentioned implementation.
        /// 5.9.0.0        7/04/2007      YK       Changed the Implementation. 
        ///                                        1) Previously the query returned all Stnadard options from which
        ///                                           the most appropriate (Priority) options was chosen. 
        ///                                           Now, first the appropriate rules are identified from the set
        ///                                           of available optisn, and then which ever is "Standard" out
        ///                                           of them are appended to the Quote.
        ///                                        2) Previously we were using a Custom SQL to get the resultset. 
        ///                                           Now we are getting it through a Pivotal Query.
        /// </history>
        protected virtual void CreateOpportunityProductStandard(object releaseId, object neighborhoodId, object opportunityId,
            object planId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                //YK - Removed this code and written a Pivotal Query which is a strip down version of the Custom Query
                /* 
                const string SORT = "SELECT * FROM ({0}) AS t ORDER BY {1} ASC"; 
                string strSQL = GetWildcardSql(releaseId, neighborhoodId, opportunityId, planId, true, string.Empty,
                    false);
                string strSort = SORT.Replace("{1}", modOpportunity.strfDIVISION_PRODUCT_ID + ", " + modOpportunity.strfWC_LEVEL_WITH_PLAN);
                strSQL = strSort.Replace("{0}", strSQL);
                Recordset rstNBHDPFull = objLib.GetRecordset(strSQL);
                 */

                //YK - Read the Division and Region Information, rather than changing the paramter list.
                Recordset rstNeighborhood = objLib.GetRecordset(neighborhoodId, modOpportunity.strt_NEIGHBORHOOD, modOpportunity.strfDIVISION_ID,
                    modOpportunity.strfREGION_ID);
                object vntDivisionId = rstNeighborhood.Fields[modOpportunity.strfDIVISION_ID].Value;
                object vntRegionId = rstNeighborhood.Fields[modOpportunity.strfREGION_ID].Value;
                rstNeighborhood.Close();

                //YK - Reading the Plan's Division Product Id
                object vntDivPlanId = RSysSystem.Tables[modOpportunity.strt_NBHDP_PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(planId);

                //ML start **need to do the sorting, thereby attaching a ASRL** 
                //set search
                IRSearch3 search = (IRSearch3)RSysSystem.Searches[modOpportunity.strsearchACTIVE_OPTIONS_GEO_PRODUCT];
                //attach SearchResultList
                IRSearchResultsList3 srlList = (IRSearchResultsList3)((IRUserProfile5)RSysSystem.UserProfile).GetSearchResultsList(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strsrlOPTIONS_BY_PRODUCT_BY_PRIOTIY);
                IRSearchCriteria5 scCriteria = (IRSearchCriteria5)search.CreateSearchCriteria();
                scCriteria.ListView.BaseSRL = srlList;
                //set parameters
                scCriteria.Parameter = vntRegionId;
                scCriteria.Parameter = vntDivisionId;
                scCriteria.Parameter = neighborhoodId;
                scCriteria.Parameter = releaseId;
                scCriteria.Parameter = planId;
                scCriteria.Parameter = planId;
                scCriteria.Parameter = vntDivPlanId;
                scCriteria.Parameter = vntDivisionId;
                //load search
                Recordset rstNBHDPFull = scCriteria.LoadSearchResults();
                //ML end **need to do the sorting, thereby attaching a ASRL**
                //Recordset rstNBHDPFull = objLib.GetRecordset(modOpportunity.strqACTIVE_OPTIONS_GEO_PRODUCT, 8, vntRegionId, vntDivisionId, neighborhoodId, releaseId,
                  //  planId, planId, vntDivPlanId, vntDivisionId);

                Recordset rstNBHDP = new Recordset();
                // Append all the fields
                foreach (Field oField in rstNBHDPFull.Fields)
                    rstNBHDP.Fields.Append(oField.Name, oField.Type, oField.DefinedSize, (FieldAttributeEnum)(oField.Attributes));
                //Open the Reocrd set
                rstNBHDP.Open(Type.Missing, Type.Missing, CursorTypeEnum.adOpenKeyset, LockTypeEnum.adLockOptimistic, -1);
                
                if (rstNBHDPFull.RecordCount > 0)
                {
                    rstNBHDPFull.MoveFirst();

                    object vntRecordId = rstNBHDPFull.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;
                    if(TypeConvert.ToInt16(rstNBHDPFull.Fields[modOpportunity.strf_DEFAULT_PRODUCT].Value) == 1)
                    {
                        rstNBHDP.AddNew(Type.Missing, Type.Missing);
                        foreach (Field oField in rstNBHDP.Fields)
                        {
                            if (Convert.IsDBNull(rstNBHDPFull.Fields[oField.Name].Value))
                            { 
                                //Do Nothing
                            }
                            else if (oField.Name == modOpportunity.strfOPPORTUNITY_ID)
                            {
                                oField.Value = opportunityId;
                            }
                            else if (oField.Name.Contains("Quantity"))
                            {
                                //Do Nothing
                                //oField.Value = DBNull.Value;
                            }
                            else if (oField.Name.Contains("Icon_"))
                            {
                                oField.Value = true;
                            }
                            else if (oField.Name.Contains("Price"))
                            {
                                //Do Nothing
                                //oField.Value = DBNull.Value;
                            }
                            else if ((oField.Name != "__Ordinal") && (!oField.Name.Contains("@Rn_Descriptor"))
                                && (!oField.Name.Contains("Rn_Edit_Date")) && (!oField.Name.Contains("Rn_Create_Date"))
                                && (!oField.Name.Contains("Rn_Edit_User")) && (!oField.Name.Contains("Rn_Create_User"))
                                && (!oField.Name.Contains("@Special")) && (!oField.Name.Contains("Icon_")))
                            {
                                oField.Value = rstNBHDPFull.Fields[oField.Name].Value;
                            }
                        }
                    }

                    rstNBHDPFull.MoveNext();

                    while (!rstNBHDPFull.EOF)
                    {
                        if (RSysSystem.IdToString(rstNBHDPFull.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value) != RSysSystem.IdToString(vntRecordId))
                        {
                            vntRecordId = rstNBHDPFull.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;
                            if(TypeConvert.ToInt16(rstNBHDPFull.Fields[modOpportunity.strf_DEFAULT_PRODUCT].Value) == 1)
                            {
                                rstNBHDP.AddNew(Type.Missing, Type.Missing);
                                foreach (Field oField in rstNBHDP.Fields)
                                {
                                    if (Convert.IsDBNull(rstNBHDPFull.Fields[oField.Name].Value))
                                    {
                                        //Do Nothing
                                    }
                                    else if (oField.Name == modOpportunity.strfOPPORTUNITY_ID)
                                    {
                                        oField.Value = opportunityId;
                                    }
                                    else if (oField.Name.Contains("Quantity"))
                                    {
                                        //Do Nothing
                                        //oField.Value = DBNull.Value;
                                    }
                                    else if (oField.Name.Contains("Icon_"))
                                    {
                                        oField.Value = true;
                                    }
                                    else if (oField.Name.Contains("Price"))
                                    {
                                        //Do Nothing
                                        //oField.Value = DBNull.Value;
                                    }
                                    else if ((oField.Name != "__Ordinal") && (!oField.Name.Contains("@Rn_Descriptor"))
                                        && (!oField.Name.Contains("Rn_Edit_Date")) && (!oField.Name.Contains("Rn_Create_Date"))
                                        && (!oField.Name.Contains("Rn_Edit_User")) && (!oField.Name.Contains("Rn_Create_User"))
                                        && (!oField.Name.Contains("@Special")) && (!oField.Name.Contains("Icon_")))
                                    {
                                        oField.Value = rstNBHDPFull.Fields[oField.Name].Value;
                                    }
                                }                                
                            }
                        }
                        rstNBHDPFull.MoveNext();
                    }
                }

                Recordset rstNewNBHDP_Product = new Recordset();
                if (rstNBHDP.RecordCount > 0)
                {
                    rstNewNBHDP_Product = this.GetStandardWithRules(rstNBHDP, vntRegionId, vntDivisionId, neighborhoodId,
                    releaseId, vntDivPlanId, planId);
                    if (rstNewNBHDP_Product.RecordCount > 0)
                    {
                        rstNewNBHDP_Product.MoveFirst();
                        while (!(rstNewNBHDP_Product.EOF))
                        {
                            CreateOpportunityProductOption(opportunityId, rstNewNBHDP_Product.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value,
                                null, 1);
                            rstNewNBHDP_Product.MoveNext();
                        }
                    }
                }
                
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        /// <summary>
        /// This subroutine Applys the rules to the set of Standard Options.
        /// </summary>
        /// <param name="rstRecordset">Recordset with the Initial set of Standard Options</param>
        /// <param name="vntRegionId">Region Id of the Opportunity</param>
        /// <param name="vntDivisionId">Division Id</param>
        /// <param name="vntNeighborhoodId">Neighborhood Id</param>
        /// <param name="vntReleaseId">Release Id</param>
        /// <param name="vntPlanDivProdId">Plan's Division Product Id</param>
        /// <param name="vntPlanId">Plan's Product Configuration Id</param>        
        /// <returns>Final recordset with all the elements which needs to be added as Standard options to a Quote</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 5.9.0.0        Jul/05/2007     YK       Initial Version
        /// </history>
        protected virtual Recordset GetStandardWithRules(Recordset rstRecordset, object vntRegionId, object vntDivisionId,
            object vntNeighborhoodId, object vntReleaseId, object vntPlanDivProdId, object vntPlanId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                
                //Step 1: Remove all the Standard options which are having exclusion rule within themselves.
                StringBuilder strDivProdIds = new StringBuilder();
                rstRecordset.MoveFirst();
                strDivProdIds.Append(RSysSystem.IdToString(rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value));
                rstRecordset.MoveNext();
                if (rstRecordset.RecordCount > 1)
                {
                    //Build the Stringset with the rest of the Div Prod Ids.
                    while (!rstRecordset.EOF)
                    {
                        strDivProdIds.Append(",");
                        strDivProdIds.Append(RSysSystem.IdToString(rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value));
                        rstRecordset.MoveNext();
                    }

                    //Check for Exclusion Rules within themselves
                    if (rstRecordset.RecordCount > 0)
                    {
                        rstRecordset.MoveFirst();
                        long intRecordCount = 1;
                        while (!rstRecordset.EOF)
                        {
                            Recordset rstExclusionRules = objLib.GetRecordset(modOpportunity.strqACTIVE_CHILD_EXCLUDE_RULE_WITH_PARENT, 6,
                                rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value, vntPlanDivProdId,
                                rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value, vntPlanDivProdId,
                                rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value, vntPlanDivProdId);                            
                            if (rstExclusionRules.RecordCount > 0)
                            {
                                rstExclusionRules.MoveFirst();
                                while (!rstExclusionRules.EOF)
                                {
                                    string strDivProdId = mrsysSystem.IdToString(rstExclusionRules.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value) + ",";
                                    if (strDivProdIds.ToString().Contains(strDivProdId))
                                    {
                                        //Remove both the Division Products from the Recordset as well as the Stringset.
                                        strDivProdId = strDivProdId.Replace(",", "");
                                        rstRecordset.Fields[modOpportunity.strfINACTIVE].Value = 1;
                                        
                                        Recordset rstTempRecordset = rstRecordset;
                                        if (rstTempRecordset.RecordCount > 0)
                                        {
                                            rstTempRecordset.MoveFirst();
                                            bool blnDeleted = false;
                                            while (!rstTempRecordset.EOF && !blnDeleted)
                                            {
                                                string strDivProdToCompare = mrsysSystem.IdToString(rstTempRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value);
                                                if (strDivProdId == strDivProdToCompare)
                                                {                                                    
                                                    rstTempRecordset.Fields[modOpportunity.strfINACTIVE].Value = 1;
                                                    blnDeleted = true;
                                                }
                                                rstTempRecordset.MoveNext();
                                            }
                                            //Go back to the Original position of the Recordset.
                                            rstRecordset.MoveFirst();
                                            for (long i = 1; i < intRecordCount; i++)
                                                rstRecordset.MoveNext();
                                        }

                                    }
                                    rstExclusionRules.MoveNext();
                                }
                            }
                            intRecordCount++;
                            rstRecordset.MoveNext();
                        }
                    }
                }

                //Step 1A: Remove all those elemets from this list which have Inclusion rules with the ones to be deleted due to Exclusion.
                if(rstRecordset.RecordCount > 0)
                {
                    StringBuilder strToBeDeletedStrings = new StringBuilder();                    
                    rstRecordset.MoveFirst();
                    while (!rstRecordset.EOF)
                    {
                        if (TypeConvert.ToInt32(rstRecordset.Fields[modOpportunity.strfINACTIVE].Value) == 1)
                        {
                            if (strToBeDeletedStrings.Length > 0)
                            {
                                strToBeDeletedStrings.Append("," + mrsysSystem.IdToString(rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value));
                            }
                            else
                            {
                                strToBeDeletedStrings.Append(mrsysSystem.IdToString(rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value));
                            }                     
                        }
                        rstRecordset.MoveNext();
                    }
                    if (strToBeDeletedStrings.Length > 0)
                    {
                        //Create the Qurey
                        StringBuilder strSQLInternal = new StringBuilder();

                        strSQLInternal.Append("SELECT");
                        strSQLInternal.Append(" Product_Option_Rule.Parent_Product_Id,");
                        strSQLInternal.Append(" Product_Option_Rule.Child_Product_Id,");
                        strSQLInternal.Append(" Product_Option_Rule.Product_Option_Rule_Id");
                        strSQLInternal.Append(" FROM Product_Option_Rule ");
                        strSQLInternal.Append(" LEFT JOIN Division_Product ON Product_Option_Rule.Parent_Product_Id = Division_Product.Division_Product_Id");
                        strSQLInternal.Append(" WHERE ");
                        strSQLInternal.Append(" (Division_Product.Inactive = 0 OR Division_Product.Inactive IS NULL)");
                        strSQLInternal.Append(" AND");
                        strSQLInternal.Append(" (Product_Option_Rule.Inactive = 0 OR Product_Option_Rule.Inactive IS NULL)");
                        strSQLInternal.Append(" AND");
                        strSQLInternal.Append(" Product_Option_Rule.Child_Product_Id IN");
                        strSQLInternal.Append(" (");
                        strSQLInternal.Append(strToBeDeletedStrings.ToString());
                        strSQLInternal.Append(" )");
                        strSQLInternal.Append(" AND");
                        strSQLInternal.Append(" (");
                        strSQLInternal.Append(" Product_Option_Rule.Plan_Product_Id = " + mrsysSystem.IdToString(vntPlanDivProdId));
                        strSQLInternal.Append(" OR");
                        strSQLInternal.Append(" (");
                        strSQLInternal.Append(" Product_Option_Rule.Plan_Product_Id IS NULL AND");
                        strSQLInternal.Append(" Product_Option_Rule.Parent_Product_Id NOT IN");
                        strSQLInternal.Append(" (");
                        strSQLInternal.Append(" SELECT POR.Parent_Product_Id FROM Product_Option_Rule POR");
                        strSQLInternal.Append(" LEFT JOIN Division_Product DP ON POR.Parent_Product_Id = DP.Division_Product_Id");
                        strSQLInternal.Append(" WHERE");
                        strSQLInternal.Append(" (DP.Inactive = 0 OR DP.Inactive IS NULL)");
                        strSQLInternal.Append(" AND");
                        strSQLInternal.Append(" (POR.Inactive = 0 OR POR.Inactive IS NULL)");
                        strSQLInternal.Append(" AND");
                        strSQLInternal.Append(" POR.Child_Product_Id IN");
                        strSQLInternal.Append(" (");
                        strSQLInternal.Append(strToBeDeletedStrings.ToString());
                        strSQLInternal.Append(" )");
                        strSQLInternal.Append(" AND");
                        strSQLInternal.Append(" POR.Plan_Product_Id = " + mrsysSystem.IdToString(vntPlanDivProdId));
                        strSQLInternal.Append(" )");
                        strSQLInternal.Append(" )");
                        strSQLInternal.Append(" )");
                        strSQLInternal.Append(" AND");
                        strSQLInternal.Append(" Product_Option_Rule.Include_ = 1");

                        Recordset rstIncudeRules = objLib.GetRecordset(strSQLInternal.ToString());
                        if (rstIncudeRules.RecordCount > 0)
                        {
                            rstIncudeRules.MoveFirst();
                            while (!rstIncudeRules.EOF)
                            {
                                object ParentId = rstIncudeRules.Fields[modOpportunity.strfPARENT_PRODUCT_ID].Value;
                                rstRecordset.MoveFirst();
                                bool blnDeleted = false;
                                while (!rstRecordset.EOF && !blnDeleted)
                                {
                                    if (mrsysSystem.EqualIds(rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value, ParentId))
                                    {
                                        rstRecordset.Fields[modOpportunity.strfINACTIVE].Value = 1;
                                        blnDeleted = true;
                                    }
                                    rstRecordset.MoveNext();
                                }

                                rstIncudeRules.MoveNext();
                            }
                        
                        }
                    }
                
                
                
                
                }

                //Actually deleting the records now. Chose to Set the field "Inactive" to true to denote the rows to
                //be deleted. The recordset alwasy contains only rhe Active ones, hence this flag could be used for
                // this purpose.
                if (rstRecordset.RecordCount > 0)
                {
                    rstRecordset.MoveFirst();
                    strDivProdIds.Append(",");
                    while (!rstRecordset.EOF)
                    {
                        if (TypeConvert.ToInt32(rstRecordset.Fields[modOpportunity.strfINACTIVE].Value) == 1)
                        {
                            strDivProdIds = strDivProdIds.Replace(mrsysSystem.IdToString(rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value) + ",", "");
                            rstRecordset.Delete(AffectEnum.adAffectCurrent);
                        }
                        rstRecordset.MoveNext();
                    }
                }
                //Check if last "," is present
                if (strDivProdIds.ToString().EndsWith(","))
                    strDivProdIds.Remove(strDivProdIds.Length - 1, 1);

                //Re-checking if there are still any records left after removing the ones having Exclusion within themselves.
                if (rstRecordset.RecordCount > 0)
                {

                    //Step 2: Array Creation and Implementation of Inclusion Rules.

                    //Create the Qurey
                    StringBuilder strSQL = new StringBuilder();

                    strSQL.Append("SELECT");
                    strSQL.Append(" Product_Option_Rule.Parent_Product_Id,");
                    strSQL.Append(" Product_Option_Rule.Child_Product_Id,");
                    strSQL.Append(" Product_Option_Rule.Product_Option_Rule_Id");
                    strSQL.Append(" FROM Product_Option_Rule ");
                    strSQL.Append(" LEFT JOIN Division_Product ON Product_Option_Rule.Child_Product_Id = Division_Product.Division_Product_Id");
                    strSQL.Append(" WHERE ");
                    strSQL.Append(" (Division_Product.Inactive = 0 OR Division_Product.Inactive IS NULL)");
                    strSQL.Append(" AND");
                    strSQL.Append(" (Product_Option_Rule.Inactive = 0 OR Product_Option_Rule.Inactive IS NULL)");
                    strSQL.Append(" AND");
                    strSQL.Append(" Product_Option_Rule.Parent_Product_Id IN");
                    strSQL.Append(" (");
                    strSQL.Append(strDivProdIds.ToString());
                    strSQL.Append(" )");
                    strSQL.Append(" AND");
                    strSQL.Append(" (");
                    strSQL.Append(" Product_Option_Rule.Plan_Product_Id = " + mrsysSystem.IdToString(vntPlanDivProdId));
                    strSQL.Append(" OR");
                    strSQL.Append(" (");
                    strSQL.Append(" Product_Option_Rule.Plan_Product_Id IS NULL AND");
                    strSQL.Append(" Product_Option_Rule.Child_Product_Id NOT IN");
                    strSQL.Append(" (");
                    strSQL.Append(" SELECT POR.Child_Product_Id FROM Product_Option_Rule POR");
                    strSQL.Append(" LEFT JOIN Division_Product DP ON POR.Child_Product_Id = DP.Division_Product_Id");
                    strSQL.Append(" WHERE");
                    strSQL.Append(" (DP.Inactive = 0 OR DP.Inactive IS NULL)");
                    strSQL.Append(" AND");
                    strSQL.Append(" (POR.Inactive = 0 OR POR.Inactive IS NULL)");
                    strSQL.Append(" AND");
                    strSQL.Append(" POR.Parent_Product_Id IN");
                    strSQL.Append(" (");
                    strSQL.Append(strDivProdIds.ToString());
                    strSQL.Append(" )");
                    strSQL.Append(" AND POR.Plan_Product_Id = " + mrsysSystem.IdToString(vntPlanDivProdId));
                    strSQL.Append(" )");
                    strSQL.Append(" )");
                    strSQL.Append(" )");

                    StringBuilder strSQLInclude = new StringBuilder();
                    strSQLInclude.Append(strSQL.ToString());
                    strSQLInclude.Append(" AND");
                    strSQLInclude.Append(" Product_Option_Rule.Include_ = 1");

                    Recordset rstIncudeRules = objLib.GetRecordset(strSQLInclude.ToString());
                    StringBuilder strPrimaryList = new StringBuilder();
                    StringBuilder strSecondaryList = new StringBuilder();
                    long intTotalPossibleProducts = rstIncudeRules.RecordCount + rstRecordset.RecordCount;
                    structOption[] structOptions = new structOption[intTotalPossibleProducts];
                    long intCount = 0;
                    if (rstRecordset.RecordCount > 0)
                    {
                        rstRecordset.MoveFirst();
                        while (!rstRecordset.EOF)
                        {
                            structOptions[intCount].intPriority = 1;
                            structOptions[intCount].strDivProductId = mrsysSystem.IdToString(rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value);
                            structOptions[intCount].strProductConfigId = mrsysSystem.IdToString(rstRecordset.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value);
                            structOptions[intCount].intSelected = 1;
                            structOptions[intCount].strDependency = "";
                            strPrimaryList.Append(structOptions[intCount].strDivProductId + "=" + TypeConvert.ToString(intCount) + ",");
                            intCount++;
                            rstRecordset.MoveNext();
                        }
                    }
                    if (rstIncudeRules.RecordCount > 0)
                    {
                        rstIncudeRules.MoveFirst();
                        while (!rstIncudeRules.EOF)
                        {
                            string strParent = mrsysSystem.IdToString(rstIncudeRules.Fields[modOpportunity.strfPARENT_PRODUCT_ID].Value);
                            string strChild = mrsysSystem.IdToString(rstIncudeRules.Fields[modOpportunity.strfCHILD_PRODUCT_ID].Value);
                            long intParent = -1;
                            long intChild = -1;
                            {
                                bool blnFlag = true;
                                for (long i = 0; i < rstRecordset.RecordCount && blnFlag; i++)
                                {
                                    if (strParent == structOptions[i].strDivProductId)
                                    {
                                        intParent = i;
                                        blnFlag = false;
                                    }
                                }
                            }

                            if (strPrimaryList.ToString().Contains(strChild))
                            {
                                bool blnFlag = true;
                                for (long i = 0; i < rstRecordset.RecordCount && blnFlag; i++)
                                {
                                    if (strChild == structOptions[i].strDivProductId)
                                    {
                                        intChild = i;
                                        blnFlag = false;
                                    }
                                }

                            }
                            else
                            {
                                if (strSecondaryList.ToString().Contains(strChild))
                                {
                                    bool blnFlag = true;
                                    for (long i = rstRecordset.RecordCount; i < intCount && blnFlag; i++)
                                    {
                                        if (strChild == structOptions[i].strDivProductId)
                                        {
                                            intChild = i;
                                            blnFlag = false;
                                        }

                                    }
                                }
                                else
                                {
                                    intChild = intCount;
                                    structOptions[intCount].intPriority = 2;
                                    structOptions[intCount].intSelected = 1;
                                    structOptions[intCount].strDivProductId = strChild;
                                    strSecondaryList.Append(strChild + "=" + TypeConvert.ToString(intCount) + ",");
                                    intCount++;
                                }
                            }
                            if (intChild != -1 && intParent != -1)
                            {
                                structOptions[intChild].strDependency += (TypeConvert.ToString(intParent) + ",");
                            }
                            rstIncudeRules.MoveNext();
                        }
                    }

                    //Step 3: Remove the Options due to Exclusion.
                    StringBuilder strTotalList = new StringBuilder();
                    strTotalList.Append(" ");
                    strTotalList.Append(strPrimaryList.ToString() + strSecondaryList.ToString());
                    StringBuilder strSQLExclude = new StringBuilder();

                    strSQLExclude.Append(strSQL.ToString());
                    strSQLExclude.Append(" AND");
                    strSQLExclude.Append(" Product_Option_Rule.Exclude = 1");

                    Recordset rstExcludeRules = objLib.GetRecordset(strSQLExclude.ToString());
                    if (rstExcludeRules.RecordCount > 0)
                    {
                        rstExcludeRules.MoveFirst();
                        while (!rstExcludeRules.EOF)
                        {
                            string strParent = mrsysSystem.IdToString(rstExcludeRules.Fields[modOpportunity.strfPARENT_PRODUCT_ID].Value);
                            string strChild = mrsysSystem.IdToString(rstExcludeRules.Fields[modOpportunity.strfCHILD_PRODUCT_ID].Value);
                            if (strTotalList.ToString().Contains(strChild))
                            {
                                long intChild = -1;
                                string[] strArrChild = strTotalList.ToString().Split(',');
                                for (long i = 0; i < strArrChild.Length; i++)
                                {
                                    if (strArrChild[i].Contains(strChild))
                                    {
                                        string[] strArrChildIndex = strArrChild[i].Split('=');
                                        intChild = TypeConvert.ToInt64(strArrChildIndex[1]);
                                        break;
                                    }
                                }

                                //Unselect the affiliated Parent options which had Inclusion with the same.
                                if (structOptions[intChild].strDependency.Length != 0)
                                {
                                    string[] strChildDependency = structOptions[intChild].strDependency.Split(',');
                                    for (long i = 0; i < strChildDependency.Length; i++)
                                    {
                                        if (strChildDependency[i].Length > 0)
                                        {
                                            long intParent = TypeConvert.ToInt64(strChildDependency[i]);
                                            structOptions[TypeConvert.ToInt64(strChildDependency[i])].intSelected = 0;
                                            if (structOptions[TypeConvert.ToInt64(strChildDependency[i])].intPriority == 1)
                                            {
                                                //Deleting from the main record.
                                                if (rstRecordset.RecordCount > 0)
                                                {
                                                    rstRecordset.MoveFirst();
                                                    while (!rstRecordset.EOF)
                                                    {
                                                        if (mrsysSystem.IdToString(rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value) == structOptions[TypeConvert.ToInt64(strChildDependency[i])].strDivProductId)
                                                        {
                                                            rstRecordset.Delete(AffectEnum.adAffectCurrent);
                                                            break;
                                                        }
                                                        rstRecordset.MoveNext();
                                                    }
                                                }
                                                //Delete the Added records due to this Parent, if they do not have any other dependency.
                                                for (long j = 0; j < intCount ; j++)
                                                {
                                                    if (structOptions[j].strDependency.Contains(strChildDependency[i] + ","))
                                                    {
                                                        structOptions[j].strDependency = structOptions[j].strDependency.Replace(strChildDependency[i] + ",", "");
                                                        if (structOptions[j].strDependency.Length == 0)
                                                        {
                                                            structOptions[j].intSelected = 0;
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                                structOptions[intChild].intSelected = 0;
                            }
                            rstExcludeRules.MoveNext();
                        }
                    }
                    //Step 4: Add the Product Configuration for the Priority 2 Products in the list.
                    for (long i = 0; i < intCount ; i++)
                    {
                        if (structOptions[i].intSelected == 1)
                        {
                            if (structOptions[i].intPriority == 2)
                            {
                                object vntProduct_Id = mrsysSystem.StringToId(structOptions[i].strDivProductId);

                                //ML start **need to do the sorting, thereby attaching a ASRL** 
                                //set search
                                IRSearch3 search = (IRSearch3)RSysSystem.Searches[modOpportunity.strsearchACTIVE_OPTIONS_GEO_PRODUCT_FOR_PRODUCT];
                                //attach SearchResultList
                                IRSearchResultsList3 srlList = (IRSearchResultsList3)((IRUserProfile5)RSysSystem.UserProfile).GetSearchResultsList(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strsrlOPTIONS_BY_PRODUCT_BY_PRIOTIY);
                                IRSearchCriteria5 scCriteria = (IRSearchCriteria5)search.CreateSearchCriteria();
                                scCriteria.ListView.BaseSRL = srlList;
                                //set parameters
                                scCriteria.Parameter = vntRegionId;
                                scCriteria.Parameter = vntDivisionId;
                                scCriteria.Parameter = vntNeighborhoodId;
                                scCriteria.Parameter = vntReleaseId;
                                scCriteria.Parameter = vntProduct_Id;
                                scCriteria.Parameter = vntPlanId;
                                scCriteria.Parameter = vntPlanId;
                                scCriteria.Parameter = vntPlanDivProdId;
                                scCriteria.Parameter = vntDivisionId;
                                //load search
                                Recordset rstNBHDP_Product = scCriteria.LoadSearchResults();
                                //ML end **need to do the sorting, thereby attaching a ASRL**

                                //Recordset rstNBHDP_Product = objLib.GetRecordset(modOpportunity.strqACTIVE_OPTIONS_GEO_PRODUCT_FOR_PRODUCT, 9,
                                    //new object[] { 
                                    //           vntRegionId, 
                                    //           vntDivisionId, 
                                    //           vntNeighborhoodId, 
                                    //           vntReleaseId,
                                    //           vntProduct_Id,
                                    //           vntPlanId, 
                                    //           vntPlanId, 
                                    //           vntPlanDivProdId, 
                                    //           vntDivisionId                                               
                                    //           });
                                if (rstNBHDP_Product.RecordCount > 0)
                                {
                                    rstRecordset.AddNew(Type.Missing, Type.Missing);
                                    rstRecordset.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value = rstNBHDP_Product.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;
                                    rstRecordset.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value = rstNBHDP_Product.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value;
                                }

                            }

                        }
                    }
                }
                return rstRecordset;
            }
            catch (Exception Exc)
            {
                throw new PivotalApplicationException(Exc.Message, Exc, RSysSystem);
            }
        }
        /// <summary>
        /// This subroutine gets the quantity based on the location area and unit of measurement
        /// </summary>
        /// <param name="vntOpportunityId">Opportunity Id</param> 
        /// <param name="vntNBHDDivisionProductId">Neighborhood Product Id</param> 
        /// <returns>New Opportunity product option Id</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 5.9.0.0        3/30/2007     BC         For Quantity using the Unit of Measurement
        /// </history>
        protected virtual int GetQuantity(object vntOpportunityId, object vntNBHDDivisionProductId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                int intQuantity = 1;
                if (!Convert.IsDBNull(vntNBHDDivisionProductId))
                {
                    object vntDivisionProductLocationId = objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfLOCATION_ID, vntNBHDDivisionProductId);
                    object vntDivisionProductId = objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfDIVISION_PRODUCT_ID, vntNBHDDivisionProductId);

                    if (!Convert.IsDBNull(objLib.SqlIndex(modOpportunity.strtDIVISION_PRODUCT, modOpportunity.strfUNITS_OF_MEASURE, vntDivisionProductId)))
                    {
                        object objUOM;
                        objUOM = objLib.SqlIndex(modOpportunity.strtDIVISION_PRODUCT, modOpportunity.strfUNITS_OF_MEASURE, vntDivisionProductId);
                        UnitOfMeasure UOMDivisionProduct = (UnitOfMeasure) TypeConvert.ToInt32(objUOM);
                        if ((UOMDivisionProduct != UnitOfMeasure.Each) && (UOMDivisionProduct != UnitOfMeasure.Linear_Feet) && !(Convert.IsDBNull(vntDivisionProductLocationId)))
                        {
                            object vntPlanId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strfPLAN_NAME_ID, vntOpportunityId);
                            object vntPlanDivProductId = objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfDIVISION_PRODUCT_ID, vntPlanId);
                            Recordset rstTotalArea = objLib.GetRecordset(modOpportunity.strqHB_ACTIVE_DIVPROD_LOCATION, 2, vntPlanDivProductId, vntDivisionProductLocationId, modOpportunity.strfTOTAL_AREA);
                            if (rstTotalArea.RecordCount > 0)
                            {
                                rstTotalArea.MoveFirst();
                                if (!Convert.IsDBNull(rstTotalArea.Fields[modOpportunity.strfTOTAL_AREA].Value))
                                {
                                    if (UOMDivisionProduct == UnitOfMeasure.Square_Feet)
                                        intQuantity = TypeConvert.ToInt32(rstTotalArea.Fields[modOpportunity.strfTOTAL_AREA].Value);
                                    else if (UOMDivisionProduct == UnitOfMeasure.Square_Yards)
                                        intQuantity = TypeConvert.ToInt32(Decimal.Ceiling((TypeConvert.ToDecimal(TypeConvert.ToDecimal(rstTotalArea.Fields[modOpportunity.strfTOTAL_AREA].Value) / 9))));
                                    else
                                        intQuantity = 1;
                                }
                            }
                        }
                    }

                }
                return intQuantity;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine creates a new Option in the Opportunity Product Table Based on the Passed in NBHD Phase
        /// Product
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param> 
        /// <param name="neighborhoodPhaseProductId">Neighborhood Product Id</param> 
        /// <param name="neighborhoodPhaseProductRecordset">Neighborhood Product recordset</param> 
        /// <returns>New Opportunity product option Id</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// 5.9.0.0        3/30/2007     BC         For Quantity using the Unit of Measurement
        /// </history>
        protected virtual object CreateOpportunityProductOption(object opportunityId, object neighborhoodPhaseProductId,
            Recordset neighborhoodPhaseProductRecordset)
        {
            try
            {
                int intQuantity = 1;
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                object newOppProdId;
                object newOppProdLocId;

                if (!Convert.IsDBNull(neighborhoodPhaseProductId))
                {
                    intQuantity = GetQuantity(opportunityId, neighborhoodPhaseProductId);
                }
                return this.CreateOpportunityProductOption(opportunityId, neighborhoodPhaseProductId, neighborhoodPhaseProductRecordset, intQuantity, OptionSelectionSource.Pivotal, TypeConvert.ToDateTime(DBNull.Value), out newOppProdId, out newOppProdLocId);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        
        /// <summary>
        /// This subroutine creates a new Option in the Opportunity Product Table Based on the Passed in NBHD Phase
        /// Product
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param> 
        /// <param name="neighborhoodPhaseProductId">Neighborhood Product Id</param> 
        /// <param name="neighborhoodPhaseProductRecordset">Neighborhood Product recordset</param> 
        /// <param name="quantity">Quantity</param> 
        /// <returns>New Opportunity product option Id</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        protected virtual object CreateOpportunityProductOption(object opportunityId, object neighborhoodPhaseProductId, 
            Recordset neighborhoodPhaseProductRecordset, int quantity)
        {
            object newOppProdId;
            object newOppProdLocId;
            return this.CreateOpportunityProductOption(opportunityId, neighborhoodPhaseProductId, neighborhoodPhaseProductRecordset, quantity, OptionSelectionSource.Pivotal
                , TypeConvert.ToDateTime(DBNull.Value), out newOppProdId, out newOppProdLocId);
        }


        /// <summary>
        /// This subroutine creates a new Option in the Opportunity Product Table Based on the Passed in NBHD Phase
        /// Product
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param> 
        /// <param name="neighborhoodPhaseProductId">Neighborhood Product Id</param> 
        /// <param name="neighborhoodPhaseProductRecordset">Neighborhood Product recordset</param> 
        /// <param name="quantity">Quantity</param> 
        /// <param name="optionSelectionSource">option selection source</param> 
        /// <param name="transactionDatetime">transactionDatetime</param> 
        /// <param name="newOppProductId">newOppProductId</param> 
        /// <param name="newOppProdLocId">newOppProdLocId</param> 
        /// <returns>New Opportunity product option Id</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// 5.9.0.0        2/24/2007     BC         Changed the code to copy the Package Component to Opp Product Table
        /// 5.9.0.0        5/21/2007     ML         Issue#65536-19187
        /// </history>
        protected virtual object CreateOpportunityProductOption(object opportunityId, object neighborhoodPhaseProductId,
            Recordset neighborhoodPhaseProductRecordset, int quantity, OptionSelectionSource optionSelectionSource
            ,DateTime transactionDatetime
            ,out object newOppProductId, out object newOppProdLocId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // need to update the elevation and elevation premium
                Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfELEVATION_ID,
                    modOpportunity.strfELEVATION_PREMIUM, modOpportunity.strfPIPELINE_STAGE, modOpportunity.strfCONTRACT_APPROVED_SUBMITTED,
                    modOpportunity.strfQUOTE_CREATE_DATE, modOpportunity.strfCONTRACT_APPROVED_SUBMITTED_DATETIME, modOpportunity.strfQUOTE_CREATE_DATETIME);

                // create a new Option
                Recordset rstNewOppProduct = objLib.GetNewRecordset(modOpportunity.strt_OPPORTUNITY__PRODUCT, modOpportunity.strf_SELECTED,
                    modOpportunity.strf_OPPORTUNITY_ID, modOpportunity.strf_NBHDP_PRODUCT_ID, modOpportunity.strfPRICE,
                    modOpportunity.strf_QUANTITY, modOpportunity.strf_OPPORTUNITY__PRODUCT_ID, modOpportunity.strfCODE,
                    modOpportunity.strfPRODUCT_AVAILABLE, modOpportunity.strfPRODUCT_NAME, modOpportunity.strfFILTER_VISIBILITY,
                    modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL, modOpportunity.strfNET_CONFIG,
                    modOpportunity.strfCATEGORY_ID, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID, modOpportunity.strfOPTION_ADDED_BY,
                    modOpportunity.strfDEPOSIT, modOpportunity.strfLOCATION_ID, modOpportunity.strfMANUFACTURER, modOpportunity.strfOPTION_SELECTION_SOURCE,
                    modOpportunity.EnvOptionSelectedDatetimeField,
                    modOpportunity.strfSTYLE_NUMBER, modOpportunity.strfSUB_CATEGORY_ID, modOpportunity.strfUSE_POST_CUTOFF_PRICE,
                    modOpportunity.strfDIVISION_PRODUCT_ID,
                    modOpportunity.strfBUILT_OPTION, modOpportunity.strfOPTION_SELECTED_DATE, modOpportunity.strfOPTION_AVAILABLE_TO
                    );

                rstNewOppProduct.AddNew(Type.Missing, Type.Missing);

                // Get Existing NBHDP_Product
                object vntDivProductId = DBNull.Value;
                if (neighborhoodPhaseProductRecordset == null)
                {
                    neighborhoodPhaseProductRecordset = objLib.GetRecordset(neighborhoodPhaseProductId, modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strf_NBHDP_PRODUCT_ID,
                        modOpportunity.strfPRODUCT_NAME, modOpportunity.strfCODE_, modOpportunity.strfLOCATION_ID, modOpportunity.strfTYPE,
                        modOpportunity.strfMANUFACTURER, modOpportunity.strfCATEGORY_ID, modOpportunity.strf_DIVISION_PRODUCT_ID,
                        modOpportunity.strfDIVISION_PRODUCT_ID, modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL, modOpportunity.strfSTYLE_NUMBER,
                        modOpportunity.strfOPTION_AVAILABLE_TO
                        );
                    // Construction Stage, Required deposit and Preferences defaulted from Division Product
                    vntDivProductId = neighborhoodPhaseProductRecordset.Fields[modOpportunity.strf_DIVISION_PRODUCT_ID].Value;
                    
                    if ((vntDivProductId != DBNull.Value))
                    {
                        rstNewOppProduct.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value = RSysSystem.Tables[modOpportunity.strtDIVISION_PRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntDivProductId);
                        rstNewOppProduct.Fields[modOpportunity.strfDEPOSIT].Value = RSysSystem.Tables[modOpportunity.strtDIVISION_PRODUCT].Fields[modOpportunity.strfREQUIRED_DEPOSIT_AMT].Index(vntDivProductId);
                        rstNewOppProduct.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value = vntDivProductId;
                    }
                }
                else
                {
                    // Construction Stage is in the Query for performance
                    vntDivProductId = neighborhoodPhaseProductRecordset.Fields[modOpportunity.strf_DIVISION_PRODUCT_ID].Value;
                    rstNewOppProduct.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value = neighborhoodPhaseProductRecordset.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value;
                    rstNewOppProduct.Fields[modOpportunity.strfDEPOSIT].Value = neighborhoodPhaseProductRecordset.Fields[modOpportunity.strfREQUIRED_DEPOSIT_AMT].Value;
                    rstNewOppProduct.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value = vntDivProductId;
                }

                rstNewOppProduct.Fields[modOpportunity.strf_OPPORTUNITY_ID].Value = opportunityId;
                //BC Use PCO
                string strConstructionStageComparison = GetConstructionStageComparison();
                object vntHomesiteID = RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfLOT_ID].Index(opportunityId);
                bool blnUsePCOPrice = false;
                if ((!Convert.IsDBNull(RSysSystem.Tables[modOpportunity.strtPRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntHomesiteID)))
                    && (!Convert.IsDBNull(RSysSystem.Tables[modOpportunity.strtDIVISION_PRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntDivProductId))))
                {
                    object vntHomesiteConstructionStageId = RSysSystem.Tables[modOpportunity.strtPRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntHomesiteID);
                    int intHomesiteConstructionStageOrdinal = (int)RSysSystem.Tables[modOpportunity.strtCONSTRUCTION_STAGE].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Index(vntHomesiteConstructionStageId);
                    object vntOptionConstructionStageId = RSysSystem.Tables[modOpportunity.strtDIVISION_PRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntDivProductId);
                    int intOptionConstructionStageOrdinal = (int)RSysSystem.Tables[modOpportunity.strtCONSTRUCTION_STAGE].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Index(vntOptionConstructionStageId);
                    if ((strConstructionStageComparison == modOpportunity.strsGREATER_THAN && intHomesiteConstructionStageOrdinal > intOptionConstructionStageOrdinal) ||
                        (strConstructionStageComparison == modOpportunity.strsGREATER_THAN_OR_EQUAL_TO && intHomesiteConstructionStageOrdinal >= intOptionConstructionStageOrdinal))
                    {
                        rstNewOppProduct.Fields[modOpportunity.strfUSE_POST_CUTOFF_PRICE].Value = true;
                        blnUsePCOPrice = true;
                        //ML - May 21 2007 - Issue#65536-19187, setting Built_Option to false
                        rstNewOppProduct.Fields[modOpportunity.strfBUILT_OPTION].Value = false;
                    }
                    else
                    {
                        rstNewOppProduct.Fields[modOpportunity.strfUSE_POST_CUTOFF_PRICE].Value = false;
                        blnUsePCOPrice = false;
                        rstNewOppProduct.Fields[modOpportunity.strfBUILT_OPTION].Value = false;
                    }
                }
                else
                {
                    rstNewOppProduct.Fields[modOpportunity.strfUSE_POST_CUTOFF_PRICE].Value = false;
                    blnUsePCOPrice = false;
                }
                
                rstNewOppProduct.Fields[modOpportunity.strf_NBHDP_PRODUCT_ID].Value = neighborhoodPhaseProductRecordset.Fields[modOpportunity.strf_NBHDP_PRODUCT_ID].Value;
                // the option thats being copied
                // set values previously handled by table level formulas
                rstNewOppProduct.Fields[modOpportunity.strfPRODUCT_NAME].Value = neighborhoodPhaseProductRecordset.Fields[modOpportunity.strfPRODUCT_NAME].Value;

                rstNewOppProduct.Fields[modOpportunity.strfCODE_].Value = neighborhoodPhaseProductRecordset.Fields[modOpportunity.strfCODE_].Value;
                rstNewOppProduct.Fields[modOpportunity.strfPRODUCT_AVAILABLE].Value = true;
                rstNewOppProduct.Fields[modOpportunity.strfFILTER_VISIBILITY].Value = true;
                rstNewOppProduct.Fields[modOpportunity.strfLOCATION_ID].Value = neighborhoodPhaseProductRecordset.Fields[modOpportunity.strfLOCATION_ID].Value;
                rstNewOppProduct.Fields[modOpportunity.strfMANUFACTURER].Value = neighborhoodPhaseProductRecordset.Fields[modOpportunity.strfMANUFACTURER].Value;
                rstNewOppProduct.Fields[modOpportunity.strfCATEGORY_ID].Value = neighborhoodPhaseProductRecordset.Fields[modOpportunity.strfCATEGORY_ID].Value;
                rstNewOppProduct.Fields[modOpportunity.strfOPTION_AVAILABLE_TO].Value = neighborhoodPhaseProductRecordset.Fields[modOpportunity.strfOPTION_AVAILABLE_TO].Value;

                rstNewOppProduct.Fields[modOpportunity.strf_SELECTED].Value = true;
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                rstNewOppProduct.Fields[modOpportunity.strfOPTION_ADDED_BY].Value = administration.CurrentUserRecordId;
                rstNewOppProduct.Fields[modOpportunity.strfOPTION_SELECTED_DATE].Value = DateTime.Now;

                // get the price for this option
                string vntPipeline_Stage = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfPIPELINE_STAGE].Value);
                DateTime vntSalesRequestDate = TypeConvert.ToDateTime(rstOpportunity.Fields[modOpportunity.strfCONTRACT_APPROVED_SUBMITTED].Value);
                DateTime vntSalesRequestDateTime = TypeConvert.ToDateTime(rstOpportunity.Fields[modOpportunity.strfCONTRACT_APPROVED_SUBMITTED_DATETIME].Value);
                DateTime vntQuoteCreateDate = TypeConvert.ToDateTime(rstOpportunity.Fields[modOpportunity.strfQUOTE_CREATE_DATE].Value);
                DateTime vntQuoteCreateDateTime = TypeConvert.ToDateTime(rstOpportunity.Fields[modOpportunity.strfQUOTE_CREATE_DATETIME].Value);

                bool blnContinue = false;
                if ((vntPipeline_Stage == modOpportunity.strPIPELINE_SALES_REQUEST) ||
                    (vntPipeline_Stage == modOpportunity.strPIPELINE_POST_SALE))
                {
                    object vntDivisionId = objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfDIVISION_ID,
                        neighborhoodPhaseProductId);
                    StandardOptionPricing vntStndOption = (StandardOptionPricing)TypeConvert.ToInt32(objLib.SqlIndex(modOpportunity.strt_DIVISION, modOpportunity.strfSTANDARD_OPTION_PRICING,
                        vntDivisionId));
                    DateTime vntOptionPriceDate = TypeConvert.ToDateTime(DBNull.Value);
                    if (vntPipeline_Stage == modOpportunity.strPIPELINE_SALES_REQUEST)
                    {
                        vntOptionPriceDate = vntSalesRequestDateTime;
                    }
                    else
                    {
                        vntOptionPriceDate = vntQuoteCreateDateTime;
                    }

                    if (vntStndOption == StandardOptionPricing.Fixed)
                    {
                        // fixed, use the sales request date to figure out the price
                        rstNewOppProduct.Fields[modOpportunity.strfPRICE].Value = GetOptionFixedPrice(neighborhoodPhaseProductId,
                            vntOptionPriceDate, blnUsePCOPrice);

                    }
                    else if (vntStndOption == StandardOptionPricing.Floating)
                    {
                        // floating, get curr price
                        blnContinue = true;
                    }
                }
                else
                {
                    blnContinue = true;
                    // carry on as usual
                }
                if (blnContinue)
                {
                    rstNewOppProduct.Fields[modOpportunity.strfPRICE].Value = GetOptionNextPrice(neighborhoodPhaseProductId, 
                                            blnUsePCOPrice);
                }
                // Default Quantity
                rstNewOppProduct.Fields[modOpportunity.strf_QUANTITY].Value = quantity;
                rstNewOppProduct.Fields[modOpportunity.strfOPTION_SELECTION_SOURCE].Value = optionSelectionSource;
                rstNewOppProduct.Fields[modOpportunity.EnvOptionSelectedDatetimeField].Value = transactionDatetime;

                //BC - For Packages
                //***************************************************************************
                rstNewOppProduct.Fields[modOpportunity.strfSTYLE_NUMBER].Value = objLib.SqlIndex(modOpportunity.strtDIVISION_PRODUCT,
                                                                                modOpportunity.strfSTYLE_NUMBER, vntDivProductId);
                //***************************************************************************

                objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY__PRODUCT, rstNewOppProduct);
                newOppProductId = rstNewOppProduct.Fields[modOpportunity.strf_OPPORTUNITY__PRODUCT_ID].Value;


                if (TypeConvert.ToString(neighborhoodPhaseProductRecordset.Fields[modOpportunity.strfTYPE].Value) == modOpportunity.strsELEVATION)
                {
                    // Set rstOpportunity = objLib.GetRecordset
                    if (rstOpportunity.RecordCount > 0)
                    {
                        rstOpportunity.MoveFirst();
                        rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value = neighborhoodPhaseProductRecordset.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value;
                        rstOpportunity.Fields[modOpportunity.strfELEVATION_PREMIUM].Value = rstNewOppProduct.Fields[modOpportunity.strfPRICE].Value;
                        objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstOpportunity);
                    }
                }
                //BC - Create Package Child Opportunity REcords
                CreateOpportunityProductPackage(vntDivProductId, newOppProductId);

                // set default preferences:
                //OpportunityProductAttributePreference objOpAttrPref = (OpportunityProductAttributePreference)mrsysSystem.ServerScripts[modOpportunity.strsOP_ATTR_PREF].CreateInstance();
                newOppProdLocId = DBNull.Value;
                object parameters = new object[] { newOppProdLocId, newOppProductId };
                RSysSystem.Forms[modOpportunity.strrOPPORTUNITY_PRODUCT_LOCATION].Execute(modOpportunity.strmCREATE_NEW_ATTR_PREF, ref parameters);
                newOppProdLocId=((object[])parameters)[6];
                return newOppProductId;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine creates the Opportunity Product Record for the given package
        /// </summary>
        /// <param name="vntParentDivisionProductId">Parent Division Product Id</param>
        /// <param name="vntParentOppProductId"> Parent Opportunity Divison Product Id</param>
        /// <returns>Returns string containing an error message.</returns>
        /// <history>
        /// Revision#      Date          Author    Description
        /// 5.9.0.0        24/02/2007     BC      Create Opp Product for Package.
        /// </history>
        protected virtual void CreateOpportunityProductPackage(object vntParentDivisionProductId, object vntParentOppProductId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                //BC for packages for parent Division Product
                Recordset rstPackageComponents = objLib.GetRecordset(modOpportunity.strqCOMPONENT_PRODUCT_FOR_PARENT, 1,
                                                vntParentDivisionProductId, modOpportunity.strfCOMPONENT_PRODUCT_ID);

                Recordset rstParentOppProduct = objLib.GetRecordset(vntParentOppProductId, modOpportunity.strtOPPORTUNITY__PRODUCT, 
                                                modOpportunity.strfSELECTED, modOpportunity.strf_OPPORTUNITY_ID, 
                                                modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL, 
                                                modOpportunity.strfCATEGORY_ID, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID, modOpportunity.strfOPTION_ADDED_BY,
                                                modOpportunity.strfLOCATION_ID, modOpportunity.strfMANUFACTURER, modOpportunity.strfOPTION_SELECTION_SOURCE,
                                                modOpportunity.strfSTYLE_NUMBER, modOpportunity.strfQUANTITY, modOpportunity.strfOPTION_SELECTED_DATE,
                                                modOpportunity.strfOPTION_AVAILABLE_TO, modOpportunity.EnvOptionSelectedDatetimeField
                                                );

                Recordset rstNewOppProduct = null;

                // check for the components in the Package
                if (rstPackageComponents.RecordCount > 0)
                {
                    rstPackageComponents.MoveFirst();
                    while (!(rstPackageComponents.EOF))
                    {
                        object vntChildDivisionProdId = rstPackageComponents.Fields[modOpportunity.strfCOMPONENT_PRODUCT_ID].Value;
                        // create a new Option
                        rstNewOppProduct = objLib.GetNewRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, modOpportunity.strf_SELECTED,
                            modOpportunity.strf_OPPORTUNITY_ID, modOpportunity.strfDIVISION_PRODUCT_ID,
                            modOpportunity.strf_OPPORTUNITY__PRODUCT_ID, modOpportunity.strfPARENT_PACK_OPPPROD_ID, modOpportunity.strfCODE,
                            modOpportunity.strfPRODUCT_AVAILABLE,
                            modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL,
                            modOpportunity.strfCATEGORY_ID, modOpportunity.strfOPTION_ADDED_BY,
                            modOpportunity.strfLOCATION_ID, modOpportunity.strfMANUFACTURER, modOpportunity.strfOPTION_SELECTION_SOURCE,
                            modOpportunity.strfSTYLE_NUMBER, modOpportunity.strfTYPE, modOpportunity.strfQUANTITY, modOpportunity.strfOPTION_SELECTED_DATE,
                            modOpportunity.strfOPTION_AVAILABLE_TO, modOpportunity.EnvOptionSelectedDatetimeField
                            );

                        rstNewOppProduct.AddNew(Type.Missing, Type.Missing);
                        rstNewOppProduct.Fields[modOpportunity.strfPARENT_PACK_OPPPROD_ID].Value = vntParentOppProductId;
                        rstNewOppProduct.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value = rstPackageComponents.Fields[modOpportunity.strfCOMPONENT_PRODUCT_ID].Value;

                        rstNewOppProduct.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = rstParentOppProduct.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                        rstNewOppProduct.Fields[modOpportunity.strf_SELECTED].Value = rstParentOppProduct.Fields[modOpportunity.strf_SELECTED].Value;
                        rstNewOppProduct.Fields[modOpportunity.strfOPTION_SELECTED_DATE].Value = rstParentOppProduct.Fields[modOpportunity.strfOPTION_SELECTED_DATE].Value;

                        rstNewOppProduct.Fields[modOpportunity.strfLOCATION_ID].Value = rstParentOppProduct.Fields[modOpportunity.strfLOCATION_ID].Value;
                        rstNewOppProduct.Fields[modOpportunity.strfOPTION_AVAILABLE_TO].Value = rstParentOppProduct.Fields[modOpportunity.strfOPTION_AVAILABLE_TO].Value;
                        rstNewOppProduct.Fields[modOpportunity.EnvOptionSelectedDatetimeField].Value = rstParentOppProduct.Fields[modOpportunity.EnvOptionSelectedDatetimeField].Value;

                        rstNewOppProduct.Fields[modOpportunity.strfMANUFACTURER].Value = rstParentOppProduct.Fields[modOpportunity.strfMANUFACTURER].Value;
                        rstNewOppProduct.Fields[modOpportunity.strfOPTION_SELECTION_SOURCE].Value = rstParentOppProduct.Fields[modOpportunity.strfOPTION_SELECTION_SOURCE].Value;
                        rstNewOppProduct.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value = rstParentOppProduct.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value;
                        rstNewOppProduct.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Value = rstParentOppProduct.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Value;
                        rstNewOppProduct.Fields[modOpportunity.strfOPTION_ADDED_BY].Value = rstParentOppProduct.Fields[modOpportunity.strfOPTION_ADDED_BY].Value;
                        rstNewOppProduct.Fields[modOpportunity.strfQUANTITY].Value = rstParentOppProduct.Fields[modOpportunity.strfQUANTITY].Value;

                        rstNewOppProduct.Fields[modOpportunity.strfSTYLE_NUMBER].Value = objLib.SqlIndex(modOpportunity.strtDIVISION_PRODUCT,
                                                                                        modOpportunity.strfSTYLE_NUMBER, vntChildDivisionProdId);
                        rstNewOppProduct.Fields[modOpportunity.strfCATEGORY_ID].Value = objLib.SqlIndex(modOpportunity.strtDIVISION_PRODUCT,
                                                                                        modOpportunity.strfCATEGORY_ID, vntChildDivisionProdId);
                        rstNewOppProduct.Fields[modOpportunity.strfCODE].Value = objLib.SqlIndex(modOpportunity.strtDIVISION_PRODUCT,
                                                                                        modOpportunity.strfCODE, vntChildDivisionProdId);
                        rstNewOppProduct.Fields[modOpportunity.strfTYPE].Value = objLib.SqlIndex(modOpportunity.strtDIVISION_PRODUCT,
                                                                                        modOpportunity.strfTYPE, vntChildDivisionProdId);
                        rstNewOppProduct.Fields[modOpportunity.strfOPTION_SELECTED_DATE].Value = rstParentOppProduct.Fields[modOpportunity.strfOPTION_SELECTED_DATE].Value;

                        objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstNewOppProduct);

                        rstNewOppProduct.Close();
                        rstNewOppProduct = null;
                        rstPackageComponents.MoveNext();
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine Copies the Opportunity Product Record
        /// </summary>
        /// <param name="vntSourceOppProductPackId"> Source Opp package Id</param>
        /// <param name="vntSourceOppProductPackLocId"> Source Opp Pack Loc Id</param>
        /// <param name="vntTargetOppProductPackId"> Target package Id</param>
        /// <param name="vntTargetOppProductPackLocId"> Target Opp Pack Loc Id</param>
        /// <returns>Returns string containing an error message.</returns>
        /// <history>
        /// Revision#      Date          Author    Description
        /// 5.9.0.0        9/4/2007      BC        Copies the Pacakge Components
        /// </history>
        protected virtual void CopyOpportunityProductPackageComponents(object vntSourceOppProductPackId, object vntSourceOppProductPackLocId,
                                                                       object vntTargetOppProductPackId, object vntTargetOppProductPackLocId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                //BC for packages for parent Division Product
                Recordset rstPackageComponents = objLib.GetRecordset(modOpportunity.strqOPP_PRODUCT_FOR_PACKAGE, 1,
                                                vntSourceOppProductPackId, modOpportunity.strf_SELECTED,
                                                modOpportunity.strf_OPPORTUNITY_ID, modOpportunity.strfDIVISION_PRODUCT_ID,
                                                modOpportunity.strf_OPPORTUNITY__PRODUCT_ID, modOpportunity.strfPARENT_PACK_OPPPROD_ID, modOpportunity.strfCODE,
                                                modOpportunity.strfPRODUCT_AVAILABLE,
                                                modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL,
                                                modOpportunity.strfCATEGORY_ID, modOpportunity.strfOPTION_ADDED_BY,
                                                modOpportunity.strfLOCATION_ID, modOpportunity.strfMANUFACTURER, modOpportunity.strfOPTION_SELECTION_SOURCE,
                                                modOpportunity.strfSTYLE_NUMBER, modOpportunity.strfTYPE, modOpportunity.strfQUANTITY,
                                                modOpportunity.strfOPTION_SELECTED_DATE);

                Recordset rstParentOppProduct = objLib.GetRecordset(vntTargetOppProductPackId, modOpportunity.strtOPPORTUNITY__PRODUCT,
                                modOpportunity.strfSELECTED, modOpportunity.strf_OPPORTUNITY_ID,
                                modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL,
                                modOpportunity.strfCATEGORY_ID, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID, modOpportunity.strfOPTION_ADDED_BY,
                                modOpportunity.strfLOCATION_ID, modOpportunity.strfMANUFACTURER, modOpportunity.strfOPTION_SELECTION_SOURCE,
                                modOpportunity.strfSTYLE_NUMBER, modOpportunity.strfQUANTITY);


                Recordset rstNewOppProduct = null;

                // check for the components in the Package
                if (rstPackageComponents.RecordCount > 0)
                {
                    rstPackageComponents.MoveFirst();
                    while (!(rstPackageComponents.EOF))
                    {
                        //skip creating pakage components if they are already created.
                        //because this funciton is called within a Opp_Product_Location recordset loop, if a package is assigned to
                        //mutiple locations, then to avoid creating the duplicate components we should skip below code.
                        Recordset rstComponentsForParentPakageOptionAndProduct = objLib.GetRecordset(modOpportunity.strqCOMPONENT_OPTION_FOR_A_PARENT_PACKAGE_OPTION_AND_PRODUCT
                            , 2, vntTargetOppProductPackId, rstPackageComponents.Fields[modOpportunity.strf_DIVISION_PRODUCT_ID].Value
                            , modOpportunity.strf_OPPORTUNITY__PRODUCT_ID);
                        object vntTargetProductPackComponentId;
                        if (rstComponentsForParentPakageOptionAndProduct.RecordCount == 0)
                        {
                            rstComponentsForParentPakageOptionAndProduct.Close();
                            rstComponentsForParentPakageOptionAndProduct = null;

                            // create a new Option
                            rstNewOppProduct = objLib.GetNewRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, modOpportunity.strf_SELECTED,
                                modOpportunity.strf_OPPORTUNITY_ID, modOpportunity.strfDIVISION_PRODUCT_ID,
                                modOpportunity.strf_OPPORTUNITY__PRODUCT_ID, modOpportunity.strfPARENT_PACK_OPPPROD_ID, modOpportunity.strfCODE,
                                modOpportunity.strfPRODUCT_AVAILABLE,
                                modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL,
                                modOpportunity.strfCATEGORY_ID, modOpportunity.strfOPTION_ADDED_BY,
                                modOpportunity.strfLOCATION_ID, modOpportunity.strfMANUFACTURER, modOpportunity.strfOPTION_SELECTION_SOURCE,
                                modOpportunity.strfSTYLE_NUMBER, modOpportunity.strfTYPE, modOpportunity.strfQUANTITY, modOpportunity.strfOPTION_SELECTED_DATE);

                            rstNewOppProduct.AddNew(Type.Missing, Type.Missing);
                            rstNewOppProduct.Fields[modOpportunity.strfPARENT_PACK_OPPPROD_ID].Value = vntTargetOppProductPackId;
                            rstNewOppProduct.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value = rstPackageComponents.Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Value;

                            rstNewOppProduct.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = rstParentOppProduct.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                            rstNewOppProduct.Fields[modOpportunity.strf_SELECTED].Value = rstPackageComponents.Fields[modOpportunity.strf_SELECTED].Value;
                            rstNewOppProduct.Fields[modOpportunity.strfOPTION_SELECTED_DATE].Value = rstPackageComponents.Fields[modOpportunity.strfOPTION_SELECTED_DATE].Value;

                            rstNewOppProduct.Fields[modOpportunity.strfLOCATION_ID].Value = rstParentOppProduct.Fields[modOpportunity.strfLOCATION_ID].Value;
                            rstNewOppProduct.Fields[modOpportunity.strfMANUFACTURER].Value = rstParentOppProduct.Fields[modOpportunity.strfMANUFACTURER].Value;
                            rstNewOppProduct.Fields[modOpportunity.strfOPTION_SELECTION_SOURCE].Value = rstParentOppProduct.Fields[modOpportunity.strfOPTION_SELECTION_SOURCE].Value;
                            rstNewOppProduct.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value = rstParentOppProduct.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value;
                            rstNewOppProduct.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Value = rstParentOppProduct.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Value;
                            rstNewOppProduct.Fields[modOpportunity.strfOPTION_ADDED_BY].Value = rstParentOppProduct.Fields[modOpportunity.strfOPTION_ADDED_BY].Value;
                            rstNewOppProduct.Fields[modOpportunity.strfQUANTITY].Value = rstParentOppProduct.Fields[modOpportunity.strfQUANTITY].Value;

                            rstNewOppProduct.Fields[modOpportunity.strfSTYLE_NUMBER].Value = rstPackageComponents.Fields[modOpportunity.strfSTYLE_NUMBER].Value;
                            rstNewOppProduct.Fields[modOpportunity.strfCATEGORY_ID].Value = rstPackageComponents.Fields[modOpportunity.strfCATEGORY_ID].Value;
                            rstNewOppProduct.Fields[modOpportunity.strfCODE].Value = rstPackageComponents.Fields[modOpportunity.strfCODE].Value;
                            rstNewOppProduct.Fields[modOpportunity.strfTYPE].Value = rstPackageComponents.Fields[modOpportunity.strfTYPE].Value;
                            objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstNewOppProduct);
                            vntTargetProductPackComponentId = rstNewOppProduct.Fields[modOpportunity.strf_OPPORTUNITY__PRODUCT_ID].Value;
                        }
                        else
                        {
                            vntTargetProductPackComponentId = rstComponentsForParentPakageOptionAndProduct.Fields[modOpportunity.strf_OPPORTUNITY__PRODUCT_ID].Value;
                        }
                        object vntSourceProductPackComponentId = rstPackageComponents.Fields[modOpportunity.strf_OPPORTUNITY__PRODUCT_ID].Value;

                        Recordset rstSourceLocationProdComponents;

                        object locationId = RSysSystem.Tables[modOpportunity.strtOPP_PRODUCT_LOCATION].Fields[modOpportunity.strfLOCATION_ID].Index(vntSourceOppProductPackLocId);
                        if (Convert.IsDBNull(locationId))
                        {
                            rstSourceLocationProdComponents = objLib.GetRecordset(modOpportunity.strqOPP_PROD_LOC_FOR_OPPPRODUCT, 1,
                                                        vntSourceProductPackComponentId, modOpportunity.strfLOCATION_QUANTITY,
                                                        modOpportunity.strfPREFERENCE_LIST, modOpportunity.strfOPP_PRODUCT_LOCATION_ID,
                                                        modOpportunity.strfLOCATION_ID, modOpportunity.strfPARENT_PACKAGE_OPPPROD_ID,
                                                        modOpportunity.EnvDUNSNumberField, modOpportunity.EnvGTINField,
                                                        modOpportunity.EnvNHTManufacturerNumberField, modOpportunity.EnvProductBrandField,
                                                        modOpportunity.EnvProductNumberField, modOpportunity.EnvUCCCodeField
                                                        );
                        }
                        else
                        {
                            rstSourceLocationProdComponents = objLib.GetRecordset(modOpportunity.strqOPP_PROD_LOC_FOR_OPPPRODUCT_AND_LOC, 2,
                                                        vntSourceProductPackComponentId, locationId,
                                                        modOpportunity.strfLOCATION_QUANTITY,
                                                        modOpportunity.strfPREFERENCE_LIST, modOpportunity.strfOPP_PRODUCT_LOCATION_ID,
                                                        modOpportunity.strfLOCATION_ID, modOpportunity.strfPARENT_PACKAGE_OPPPROD_ID,
                                                        modOpportunity.EnvDUNSNumberField,modOpportunity.EnvGTINField,
                                                        modOpportunity.EnvNHTManufacturerNumberField,modOpportunity.EnvProductBrandField,
                                                        modOpportunity.EnvProductNumberField,modOpportunity.EnvUCCCodeField
                                                        );
                        }

                        if (rstSourceLocationProdComponents.RecordCount > 0)
                        {
                            rstSourceLocationProdComponents.MoveFirst();
                            while (!(rstSourceLocationProdComponents.EOF))
                            {
                                // create a new one
                                Recordset rstNewOPLoc = objLib.GetNewRecordset(modOpportunity.strtOPP_PRODUCT_LOCATION, modOpportunity.strfLOCATION_ID,
                                    modOpportunity.strfLOCATION_QUANTITY, modOpportunity.strfPREFERENCE_LIST, modOpportunity.strfOPPORTUNITY_ID,
                                    modOpportunity.strfOPP_PRODUCT_ID, modOpportunity.strfPARENT_PACKAGE_OPPPROD_ID,
                                    modOpportunity.EnvDUNSNumberField, modOpportunity.EnvGTINField,
                                    modOpportunity.EnvNHTManufacturerNumberField, modOpportunity.EnvProductBrandField,
                                    modOpportunity.EnvProductNumberField, modOpportunity.EnvUCCCodeField
                                    );
                                rstNewOPLoc.AddNew(Type.Missing, Type.Missing);
                                rstNewOPLoc.Fields[modOpportunity.strfLOCATION_ID].Value = rstSourceLocationProdComponents.Fields[modOpportunity.strfLOCATION_ID].Value;
                                rstNewOPLoc.Fields[modOpportunity.strfLOCATION_QUANTITY].Value = rstSourceLocationProdComponents.Fields[modOpportunity.strfLOCATION_QUANTITY].Value;
                                rstNewOPLoc.Fields[modOpportunity.strfPREFERENCE_LIST].Value = rstSourceLocationProdComponents.Fields[modOpportunity.strfPREFERENCE_LIST].Value;
                                rstNewOPLoc.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY__PRODUCT,
                                    modOpportunity.strfOPPORTUNITY_ID, vntSourceOppProductPackId);
                                rstNewOPLoc.Fields[modOpportunity.strfOPP_PRODUCT_ID].Value = vntTargetProductPackComponentId;
                                rstNewOPLoc.Fields[modOpportunity.strfPARENT_PACKAGE_OPPPROD_ID].Value = vntTargetOppProductPackLocId;
                                rstNewOPLoc.Fields[modOpportunity.EnvDUNSNumberField].Value = rstSourceLocationProdComponents.Fields[modOpportunity.EnvDUNSNumberField].Value;;
                                rstNewOPLoc.Fields[modOpportunity.EnvGTINField].Value = rstSourceLocationProdComponents.Fields[modOpportunity.EnvGTINField].Value;;
                                rstNewOPLoc.Fields[modOpportunity.EnvNHTManufacturerNumberField].Value = rstSourceLocationProdComponents.Fields[modOpportunity.EnvNHTManufacturerNumberField].Value;;
                                rstNewOPLoc.Fields[modOpportunity.EnvProductBrandField].Value = rstSourceLocationProdComponents.Fields[modOpportunity.EnvProductBrandField].Value;;
                                rstNewOPLoc.Fields[modOpportunity.EnvProductNumberField].Value = rstSourceLocationProdComponents.Fields[modOpportunity.EnvProductNumberField].Value;;
                                rstNewOPLoc.Fields[modOpportunity.EnvUCCCodeField].Value = rstSourceLocationProdComponents.Fields[modOpportunity.EnvUCCCodeField].Value;;

                                objLib.SaveRecordset(modOpportunity.strtOPP_PRODUCT_LOCATION, rstNewOPLoc);
                                object vntOPLocId = rstNewOPLoc.Fields[modOpportunity.strfOPP_PRODUCT_LOCATION_ID].Value;

                                // attributes?
                                Recordset rstOPAttrPref = objLib.GetRecordset(modOpportunity.strqOP_LOC_ATTR_PREF_FOR_OPLOC, 1, rstSourceLocationProdComponents.Fields[modOpportunity.strfOPP_PRODUCT_LOCATION_ID].Value,
                                    modOpportunity.strfOPP_PRODUCT_LOCATION_ID, modOpportunity.strfATTRIBUTE, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID, 
                                    modOpportunity.strfOP_LOC_ATTR_PREF_ID);
                                if (rstOPAttrPref.RecordCount > 0)
                                {
                                    rstOPAttrPref.MoveFirst();
                                    while (!(rstOPAttrPref.EOF))
                                    {
                                        // creat new atrr/pref for Op location
                                        Recordset rstNewAttrPref = objLib.GetNewRecordset(modOpportunity.strtOPPPROD_ATTR_PREF, modOpportunity.strfATTRIBUTE,
                                            modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID, modOpportunity.strfOPP_PRODUCT_LOCATION_ID,
                                            modOpportunity.strfOP_LOC_ATTR_PREF_ID);
                                        rstNewAttrPref.AddNew(Type.Missing, Type.Missing);
                                        rstNewAttrPref.Fields[modOpportunity.strfOPP_PRODUCT_LOCATION_ID].Value = vntOPLocId;
                                        rstNewAttrPref.Fields[modOpportunity.strfATTRIBUTE].Value = rstOPAttrPref.Fields[modOpportunity.strfATTRIBUTE].Value;

                                        objLib.SaveRecordset(modOpportunity.strtOPPPROD_ATTR_PREF, rstNewAttrPref);

                                        //Copy data from Opp Product Pref table for the the Opp Product Id
                                        Recordset rstOppProductPref = objLib.GetRecordset(modOpportunity.strqOP_PREF_FOR_ATTRIBUTE, 1, rstOPAttrPref.Fields[modOpportunity.strfOP_LOC_ATTR_PREF_ID].Value,
                                                                    modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID, modOpportunity.strfPREFERENCE_NAME, modOpportunity.strfCODE_,
                                                                    modOpportunity.strfOPPORTUNITY_PRODUCT_ID, modOpportunity.strf_DIVISION_PRODUCT_PREF_ID,
                                                                    modOpportunity.strfOP_LOC_ATTR_PREF_ID);
                                        if (rstOppProductPref.RecordCount > 0)
                                        {
                                            rstOppProductPref.MoveFirst();
                                            while (!rstOppProductPref.EOF)
                                            {
                                                // create a new one
                                                Recordset rstNewOppProductPref = objLib.GetNewRecordset(modOpportunity.strtOPPORTUNITY_PRODUCT_PREF,
                                                            modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID, modOpportunity.strfPREFERENCE_NAME,
                                                            modOpportunity.strfCODE_, modOpportunity.strfOPPORTUNITY_PRODUCT_ID,
                                                            modOpportunity.strf_DIVISION_PRODUCT_PREF_ID, modOpportunity.strfOP_LOC_ATTR_PREF_ID);
                                                rstNewOppProductPref.AddNew(Type.Missing, Type.Missing);
                                                rstNewOppProductPref.Fields[modOpportunity.strfPREFERENCE_NAME].Value
                                                        = rstOppProductPref.Fields[modOpportunity.strfPREFERENCE_NAME].Value;

                                                rstNewOppProductPref.Fields[modOpportunity.strfCODE_].Value
                                                        = rstOppProductPref.Fields[modOpportunity.strfCODE_].Value;

                                                rstNewOppProductPref.Fields[modOpportunity.strfOPPORTUNITY_PRODUCT_ID].Value
                                                        = vntTargetProductPackComponentId;
                                                rstNewOppProductPref.Fields[modOpportunity.strf_DIVISION_PRODUCT_PREF_ID].Value
                                                        = rstOppProductPref.Fields[modOpportunity.strf_DIVISION_PRODUCT_PREF_ID].Value;

                                                rstNewOppProductPref.Fields[modOpportunity.strfOP_LOC_ATTR_PREF_ID].Value
                                                        = rstNewAttrPref.Fields[modOpportunity.strfOP_LOC_ATTR_PREF_ID].Value;
                                                objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY_PRODUCT_PREF, rstNewOppProductPref);

                                                rstOppProductPref.MoveNext();
                                            }
                                            //Set the selected Attribute
                                            if (!Convert.IsDBNull(rstOPAttrPref.Fields[modOpportunity.strf_OPPORTUNITY_PRODUCT_PREF_ID].Value))
                                            {
                                                rstNewAttrPref = objLib.GetRecordset(rstNewAttrPref.Fields[modOpportunity.strfOP_LOC_ATTR_PREF_ID].Value,
                                                    modOpportunity.strtOPPPROD_ATTR_PREF, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID);

                                                object vntDivisionProductPrefId = objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY_PRODUCT_PREF,
                                                    modOpportunity.strf_DIVISION_PRODUCT_PREF_ID,
                                                    rstOPAttrPref.Fields[modOpportunity.strf_OPPORTUNITY_PRODUCT_PREF_ID].Value);
                                                Recordset rstOppProdPrefTarget = objLib.GetRecordset(modOpportunity.strqOPP_PROD_PREF_FOR_OPP_PROD_AND_DIV_PROD,
                                                    2, vntTargetProductPackComponentId, vntDivisionProductPrefId,
                                                    modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID);
                                                if (rstOppProdPrefTarget.RecordCount > 0)
                                                {
                                                    rstNewAttrPref.Fields[modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID].Value =
                                                        rstOppProdPrefTarget.Fields[modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID].Value;
                                                    objLib.SaveRecordset(modOpportunity.strtOPPPROD_ATTR_PREF, rstNewAttrPref);
                                                }
                                            }
                                        }
                                        rstOPAttrPref.MoveNext();
                                    }
                                }
                                rstSourceLocationProdComponents.MoveNext();
                            }
                        }

                        if (rstNewOppProduct != null)
                        {
                            rstNewOppProduct.Close();
                            rstNewOppProduct = null;
                        }

                        rstPackageComponents.MoveNext();
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }


        /// <summary>
        /// This subroutine gets the Selected Excluded options. If any are found, an error
        /// message is returned
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="neighborhoodPhaseProductId"></param>
        /// <param name="planNameId"></param>
        /// <param name="newOption">Contains string id's of current additions in a multi select</param>
        /// <param name="newOptionWithParent">Multiple use string used for rule checks with the following implementation:
        /// "Parent:Division_Product_Id,Child:NBHDP_Product_Id;"</param>
        /// <returns>Returns string containing an error message.</returns>
        /// <history>
        /// Revision#      Date          Author    Description
        /// 3.8.0.0        5/12/2006     DYin      Converted to .Net C# code.
        /// 5.9.0.0        Feb/09/2007    YK       Inclorporated the Product_Option_Rule related changes
        /// </history>
        protected virtual string GetSelectedExcludedOptions(object opportunityId, object neighborhoodPhaseProductId, 
            object planNameId, string newOption, string newOptionWithParent)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
 
                object planProductId = DBNull.Value;
                object divisionProductId = DBNull.Value;

                planProductId = RSysSystem.Tables[modOpportunity.strt_NBHDP_PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(planNameId);
                divisionProductId = RSysSystem.Tables[modOpportunity.strt_NBHDP_PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(neighborhoodPhaseProductId);

                // get a list of mutually exclusive options
                Recordset rstOptions = objLib.GetRecordset(modOpportunity.strqACTIVE_EXC_PRIORITY_PARENT_PLAN_PARENT_PARENT_PLAN, 5, divisionProductId, planProductId, divisionProductId, divisionProductId, planProductId,
                    modOpportunity.strfEXCLUDE, modOpportunity.strfCHILD_PRODUCT_ID);
                // check against previosuly added options
                if (rstOptions.RecordCount > 0)
                {
                    rstOptions.MoveFirst();
                    while (!(rstOptions.EOF))
                    {
                        object vntChildProdId = rstOptions.Fields[modOpportunity.strfCHILD_PRODUCT_ID].Value;
                        //object vntChildNBHDProductId = objLib.GetRecordset(modOpportunity.strqSELECTED_OPTIONS_OPP_DIV_PROD, 2,
                           // opportunityId, vntChildProdId);
                        bool blnError = false;
                        // check and see if this product has already been selected
                        if (DataAccess.FindMatchInRecordset(objLib.GetRecordset(modOpportunity.strqSELECTED_OPTIONS_FOR_QUOTE,
                            1, opportunityId, modOpportunity.strfDIVISION_PRODUCT_ID), modOpportunity.strfDIVISION_PRODUCT_ID, vntChildProdId))
                        {
                            blnError = true;
                        }
                        // check against current additions
                        if (newOptionWithParent.Contains("Parent:" + RSysSystem.IdToString(vntChildProdId)))
                        {
                            blnError = true;
                        }
                        if (blnError)
                        {
                            
                            string[] vntRetVal = new string[0];
                            string[] strRetVal = new string[0];
                            vntRetVal = newOptionWithParent.Split(new char[] { Convert.ToChar(";") });
                            int intIndex = 0;
                            while (! vntRetVal[intIndex].Contains("Parent:" + RSysSystem.IdToString(vntChildProdId)) && intIndex < vntRetVal.Length)
                                  intIndex++;

                            if (intIndex < vntRetVal.Length)
                            {
                                strRetVal = vntRetVal[intIndex].Split(new string[] { Convert.ToString(",Child:") }, StringSplitOptions.RemoveEmptyEntries);
                            }
                               
                            return TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdCANNOT_SELECT_OPTION,
                                new object[] {TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfPRODUCT_NAME,
                            neighborhoodPhaseProductId))})) + TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT,
                                modOpportunity.strfPRODUCT_NAME, RSysSystem.StringToId(strRetVal[1]))) +
                                TypeConvert.ToString(LangDict.GetText(modOpportunity.strdALREADY_SELECTED));
                            
                        }
                        // check next mutally exclusive option
                        rstOptions.MoveNext();
                    }
                }
                return string.Empty;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine will Simply Save the Opportunity Filter Fields
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="categoryId">Category Id</param>
        /// <param name="constructionStageId">Construction Stage Id</param>
        /// <param name="locationId">Location Id</param>
        /// <param name="manufacturer">Manufacturer Id</param>
        /// <param name="constructionStageOnly">Flag to indicate the filter is only for constrction stage.</param>
        /// <param name="code">Code</param>
        /// <param name="currentPage"></param>
        /// <param name="subCategoryId">Sub Category Id</param>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// 5.9.0.0        Jan/24/2007   YK         Added Filter_Sub_Category
        /// comparing Ids. This is a specific request for DR Horton only.
        /// </history>
        protected virtual void UpdateOptionFilter(object opportunityId, object categoryId, object constructionStageId,
            object locationId, string manufacturer, bool constructionStageOnly, string code, int currentPage, object subCategoryId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // get list of all available options for the opportunity
                Recordset rstOpp = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strfFILTER_CONSTRUCTION_STAGE_ONLY,
                    modOpportunity.strfFILTER_LOCATION_ID, modOpportunity.strfFILTER_CATEGORY_ID, modOpportunity.strfFILTER_MANUFACTURER,
                    modOpportunity.strfFILTER_CONSTRUCTION_STAGE_ID, modOpportunity.strfFILTER_CODE_, modOpportunity.strfCURRENT_PAGE, modOpportunity.strfFILTER_SUB_CATEGORY_ID);

                rstOpp.Fields[modOpportunity.strfFILTER_CONSTRUCTION_STAGE_ONLY].Value = constructionStageOnly;
                rstOpp.Fields[modOpportunity.strfFILTER_LOCATION_ID].Value = locationId;
                rstOpp.Fields[modOpportunity.strfFILTER_CATEGORY_ID].Value = categoryId;
                rstOpp.Fields[modOpportunity.strfFILTER_MANUFACTURER].Value = manufacturer;
                rstOpp.Fields[modOpportunity.strfFILTER_CONSTRUCTION_STAGE_ID].Value = constructionStageId;
                rstOpp.Fields[modOpportunity.strfFILTER_CODE_].Value = code;
                rstOpp.Fields[modOpportunity.strfCURRENT_PAGE].Value = currentPage;
                rstOpp.Fields[modOpportunity.strfFILTER_SUB_CATEGORY_ID].Value = subCategoryId;

                // save changes
                objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstOpp);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine gets the parent options for the product used when unselecting an option, also unselect all 
        /// the included parent options
        /// </summary>
        /// <param name="neighborhoodPhaseProductId">Neighborhood Pahse Product Id</param>
        /// <param name="planNameId">Plan Name Id</param>
        /// <param name="originalProductId">Original Product Id</param>
        /// <returns>strMsg</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// 5.9.0.0        3/22/2007     YK         Hard Rule + Plan specific Option rules Vs generic Option Rules
        /// </history>
        protected virtual string GetParentOptions(object neighborhoodPhaseProductId, object planNameId, object originalProductId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOptions = null;
                // get a list of included options
                //YK - The Rules have been moved to Product_Option_Rule
                object planProductId = RSysSystem.Tables[modOpportunity.strt_NBHDP_PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(planNameId);
                object divisionProductId = RSysSystem.Tables[modOpportunity.strt_NBHDP_PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(neighborhoodPhaseProductId);
                object originalDivisionProductId = DBNull.Value;

                if (Convert.IsDBNull(originalProductId))
                {
                    rstOptions = objLib.GetRecordset(modOpportunity.strqACTIVE_HARD_INC_PRIORITY_CHILD_PLAN_CHILD_CHILD_PLAN,
                        5, divisionProductId, planProductId, divisionProductId, divisionProductId, planProductId, modOpportunity.strfINCLUDE, modOpportunity.strfEXCLUDE,
                        modOpportunity.strfPARENT_PRODUCT_ID, modOpportunity.strfCHILD_PRODUCT_ID);
                }
                else
                {
                    originalDivisionProductId = RSysSystem.Tables[modOpportunity.strt_NBHDP_PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(originalProductId);
                    rstOptions = objLib.GetRecordset(modOpportunity.strqACTIVE_HARD_INC_PRIORITY_CHILD_PLAN_CHILD_CHILD_PLAN_NOT_PARENT,
                        6, divisionProductId, planProductId, divisionProductId, divisionProductId, planProductId, originalDivisionProductId, modOpportunity.strfINCLUDE, modOpportunity.strfEXCLUDE,
                        modOpportunity.strfPARENT_PRODUCT_ID, modOpportunity.strfCHILD_PRODUCT_ID);
                }
                string strMsg = string.Empty;
                if (rstOptions.RecordCount > 0)
                {
                    rstOptions.MoveFirst();
                    StringBuilder messageBuilder = new StringBuilder();
                    while (!(rstOptions.EOF))
                    {
                        // get the Division product id
                        string strParentId = RSysSystem.IdToString(rstOptions.Fields[modOpportunity.strfPARENT_PRODUCT_ID].Value);
                        messageBuilder.Append(strParentId + ";");
                        messageBuilder.Append(GetParentOptions(rstOptions.Fields[modOpportunity.strfPARENT_PRODUCT_ID].Value, planNameId, neighborhoodPhaseProductId));

                        rstOptions.MoveNext();
                    }
                    strMsg = messageBuilder.ToString();
                }
                return strMsg;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine gets the children options for the product.
        /// </summary>
        /// <param name="neighborhoodPhaseProductId">Neighborhood Pahse Product Id</param>
        /// <param name="planNameId">Plan Name Id</param>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="isSoftRule">True = Soft Inclusion else Hard Inclusion.</param>
        /// <returns>strMsg</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// 5.9.0.0        Feb/09/2007    YK        Implementing changes for Soft/Hard Inclusion and also
        ///                                         due to moving the Rules one level up to Product Defn level.
        /// 5.9.00         Mar/05/2007    YK        Changing due to the Construction Stages + Post Cut Off Price
        /// </history>
        protected virtual string GetChildOptions(object neighborhoodPhaseProductId, object planNameId, object opportunityId, Boolean isSoftRule)
        {
            try
            {
                // Get a list of included options
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                
                //YK - Executing the Query for Soft or Hard as required
                String strQueryName = modOpportunity.strqACTIVE_SOFT_INC_PRIORITY_PARENT_PLAN_PARENT_PARENT_PLAN;
                String strPrefix = "Soft";
                if (!isSoftRule)
                {
                    strQueryName = modOpportunity.strqACTIVE_HARD_INC_PRIORITY_PARENT_PLAN_PARENT_PARENT_PLAN;
                    strPrefix = "Hard";
                }
                //YK - The Rules have been moved to Product_Option_Rule
                object planProductId = RSysSystem.Tables[modOpportunity.strt_NBHDP_PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(planNameId);
                object divisionProductId = RSysSystem.Tables[modOpportunity.strt_NBHDP_PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(neighborhoodPhaseProductId);
                object planCode = RSysSystem.Tables[modOpportunity.strt_NBHDP_PRODUCT].Fields[modOpportunity.strfCODE].Index(planNameId);

                //YK - Reading the Geographical Information
                object releaseId = RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfNBHD_PHASE_ID].Index(opportunityId);
                object neighborhoodId = RSysSystem.Tables[modOpportunity.strt_NBHD_PHASE].Fields[modOpportunity.strfNEIGHBORHOOD_ID].Index(releaseId);
                object divisionId = RSysSystem.Tables[modOpportunity.strt_NEIGHBORHOOD].Fields[modOpportunity.strfDIVISION_ID].Index(neighborhoodId);
                object regionId = RSysSystem.Tables[modOpportunity.strt_DIVISION].Fields[modOpportunity.strfREGION_ID].Index(divisionId);
                
                //YK - Construction stage related details
                SystemSetting systemSetting = (SystemSetting)RSysSystem.ServerScripts[AppServerRuleData.SystemSettingAppServerRuleName].CreateInstance();
                string strConstructionComparision = TypeConvert.ToString(systemSetting.GetSystemSetting(modOpportunity.strfCONSTRUCTION_STAGE_COMPARISON));
                // default "Greater Than"
                if (strConstructionComparision.Length == 0) strConstructionComparision = modOpportunity.sGREATER_THAN;
                object homesiteId = RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[modOpportunity.strfLOT_ID].Index(opportunityId);
                object constructionStageId = RSysSystem.Tables[modOpportunity.strt_PRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(homesiteId);
                object intconstructionOrdinalLot = DBNull.Value;
                if (!Convert.IsDBNull(constructionStageId))
                {
                    intconstructionOrdinalLot = RSysSystem.Tables[modOpportunity.strtCONSTRUCTION_STAGE].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Index(constructionStageId);
                }

                Recordset rstOptions = objLib.GetRecordset(strQueryName, 5, divisionProductId, planProductId, divisionProductId, divisionProductId, planProductId, modOpportunity.strfINCLUDE, modOpportunity.strfEXCLUDE,
                     modOpportunity.strfINCLUDE_OPTIONAL, modOpportunity.strfPARENT_PRODUCT_ID, modOpportunity.strfCHILD_PRODUCT_ID);
                string strMsg = String.Empty;
                if (rstOptions.RecordCount > 0)
                {
                    rstOptions.MoveFirst();
                    StringBuilder messageList = new StringBuilder();
                    while (!(rstOptions.EOF))
                    {
                        string strQuery = modOpportunity.strqOPTIONS_PRODUCT_GEOGRAPHY_PLAN_AND_PLANCODE;
                        object constructionStageOptionId = DBNull.Value;
                        object intconstructionOrdinalOption = DBNull.Value;
                        bool blnFlag = false;
                        //YK - Reading the construction stage ordinal values for conmarision.
                        if (!Convert.IsDBNull(intconstructionOrdinalLot))
                        {
                            constructionStageOptionId = RSysSystem.Tables[modOpportunity.strt_DIVISION_PRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(rstOptions.Fields[modOpportunity.strfCHILD_PRODUCT_ID].Value);
                            if (!Convert.IsDBNull(constructionStageOptionId))
                            {
                                intconstructionOrdinalOption = RSysSystem.Tables[modOpportunity.strtCONSTRUCTION_STAGE].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Index(constructionStageOptionId);
                            }
                            if (!Convert.IsDBNull(intconstructionOrdinalLot) && !Convert.IsDBNull(intconstructionOrdinalOption))
                            {
                                blnFlag = true;
                                strQuery = modOpportunity.strqOPTIONS_PRODUCT_GEOGRAPHY_PLAN_PLANCODE_ORD_ORD_GREATER_EQUAL;
                                if (strConstructionComparision == modOpportunity.sGREATER_THAN)
                                    strQuery = modOpportunity.strqOPTIONS_PRODUCT_GEOGRAPHY_PLAN_PLANCODE_ORD_ORD_GREATER;
                            }
                        }

                        //YK - Geting the required NBHDP_Product recordset
                        Recordset rstRandomOptions = new Recordset();
                        if (blnFlag)
                        {
                            rstRandomOptions = objLib.GetRecordset(strQuery, 9, rstOptions.Fields[modOpportunity.strfCHILD_PRODUCT_ID].Value,
                                regionId, divisionId, neighborhoodId, releaseId, planNameId, planCode, intconstructionOrdinalLot, intconstructionOrdinalLot, modOpportunity.strfNBHDP_PRODUCT_ID, modOpportunity.strfWC_LEVEL_WITH_PLAN);
                        }
                        else
                        {
                            rstRandomOptions = objLib.GetRecordset(strQuery, 7, rstOptions.Fields[modOpportunity.strfCHILD_PRODUCT_ID].Value,
                                regionId, divisionId, neighborhoodId, releaseId, planNameId, planCode, modOpportunity.strfNBHDP_PRODUCT_ID, modOpportunity.strfWC_LEVEL_WITH_PLAN);
                        }

                        // Get the NBHD product child id
                        // YK - Picking up top most Release Occurrence which has been having the least WC_Level_With_Plan value. 
                        //      This sorting is done with the help of the Lists defined in the system. The above
                        //      executes and sorts the recordset according to the required field.
                        if (rstRandomOptions.RecordCount > 0)
                        {
                            rstRandomOptions.Sort = modOpportunity.strfWC_LEVEL_WITH_PLAN;
                            rstRandomOptions.MoveFirst();
                            string strChildId = RSysSystem.IdToString(rstRandomOptions.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value);
                            messageList.Append(strPrefix + ":" + strChildId + ";");
                            messageList.Append(GetChildOptions(rstRandomOptions.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value, planNameId, opportunityId, isSoftRule));
                        }
                        rstRandomOptions.Close();
                        rstOptions.MoveNext();
                    }
                    strMsg = messageList.ToString();
                }
                rstOptions.Close();
                return strMsg;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method is called when a user changes the Neighborhood, Release,
        /// Plan or Homesite on the Quote. And depending on what type of quote it is, the code
        /// handles each scenrio and finally recalulates the total on the quote.
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateOptions(object opportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strf_PIPELINE_STAGE,
                    modOpportunity.strf_STATUS);

                if (rstOpportunity.RecordCount > 0)
                {

                    // Check for records, and retrieve the Pipeline_Stage
                    string strPipelineStage = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value);
                    string strStatus = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strf_STATUS].Value);

                    // Handle the data import differently based on the pipeline stage
                    if (strPipelineStage == modOpportunity.strsCONTRACT)
                    {
                        Recordset rstChangeOrder = objLib.GetNewRecordset(modOpportunity.strt_CHANGE_ORDER, modOpportunity.strfCHANGE_ORDER_DATE,
                            modOpportunity.strfCHANGE_ORDER_NUMBER, modOpportunity.strfNOTES, modOpportunity.strfADDED_BY_ID,
                            modOpportunity.strfCHANGE_ORDER_ID, modOpportunity.strf_OPPORTUNITY_ID);

                        object vntEmployeeId = RSysSystem.CurrentUserId();
                        int lngChangeOrderNumber = GetNextChangeOrderNumber(opportunityId);
                        rstChangeOrder.AddNew(Type.Missing, Type.Missing);
                        rstChangeOrder.Fields[modOpportunity.strfADDED_BY_ID].Value = vntEmployeeId;
                        rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_NUMBER].Value = lngChangeOrderNumber;
                        rstChangeOrder.Fields[modOpportunity.strf_OPPORTUNITY_ID].Value = opportunityId;
                        rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_DATE].Value = DateTime.Now;
                        objLib.SaveRecordset(modOpportunity.strt_CHANGE_ORDER, rstChangeOrder);

                        // retrieve the record id of the new change order
                       object vntChangeOrderId = rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_ID].Value;

                        // Add the option, set it's Change Order Id to be that
                        // of the change order just created, and check Net Config
                        // insert a new record into opportunity__products
                        // Process based on the action
                        Recordset rstOppProd = objLib.GetRecordset(modOpportunity.strq_OPP_PRODUCTS_FOR_OPP, 1, opportunityId,
                            modOpportunity.strf_OPPORTUNITY_ID, modOpportunity.strf_PRICE, modOpportunity.strf_QUANTITY,
                            modOpportunity.strf_EXTENDED_PRICE, modOpportunity.strfDEPOSIT, modOpportunity.strfOPTION_NOTES,
                            modOpportunity.strfCUSTOME_INSTRUCTIONS, modOpportunity.strf_SELECTED, modOpportunity.strfNET_CONFIG,
                            modOpportunity.strfADDED_BY_CHNG_ORDER_ID, modOpportunity.strfPRODUCT_NAME, modOpportunity.strf_OPPORTUNITY__PRODUCT_ID);
                        if (rstOppProd.RecordCount > 0)
                        {
                            rstOppProd.MoveFirst();
                            while (!(rstOppProd.EOF))
                            {
                                object vntOppProdId = rstOppProd.Fields[modOpportunity.strf_OPPORTUNITY__PRODUCT_ID].Value;
                                string strAction = TypeConvert.ToString(rstOppProd.Fields[modOpportunity.strf_SELECTED].Value);

                                if (strAction == "true")
                                {
                                    // selected is set, therefore set the values
                                    // rstOppProd.Fields(strfNET_CONFIG) = True
                                    rstOppProd.Fields[modOpportunity.strf_QUANTITY].Value = 1;

                                }
                                else if (strAction == "false")
                                {
                                    // unselecting options
                                    // Initialize Change order if not already set
                                    if (Convert.IsDBNull(vntChangeOrderId))
                                    {
                                        // Add a new row
                                        rstChangeOrder.AddNew(Type.Missing, Type.Missing);
                                        rstChangeOrder.Fields[modOpportunity.strfADDED_BY_ID].Value = vntEmployeeId;
                                        rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_NUMBER].Value = lngChangeOrderNumber;
                                        rstChangeOrder.Fields[modOpportunity.strf_OPPORTUNITY_ID].Value = opportunityId;
                                        rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_DATE].Value = DateTime.Now;
                                        // save to the relationship database
                                        objLib.SaveRecordset(modOpportunity.strt_CHANGE_ORDER, rstChangeOrder);
                                        // retrieve the record id of the new change order
                                        vntChangeOrderId = rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_ID].Value;
                                    }

                                    // Locate the existing opportunity__product record based on product
                                    // update removed by change order id
                                    rstOppProd.Fields[modOpportunity.strfDEPOSIT].Value = 0;
                                    // set deposit to 0
                                    rstOppProd.Fields[modOpportunity.strf_QUANTITY].Value = 0;

                                }
                                rstOppProd.MoveNext();
                            }

                            // save the changes
                            objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstOppProd);

                        }

                    }
                    else if (strPipelineStage == modOpportunity.strsQUOTE || strPipelineStage == modOpportunity.strsINVENTORY)
                    {

                        Recordset rstOppProd = objLib.GetRecordset(modOpportunity.strq_OPP_PRODUCTS_FOR_OPP, 1, opportunityId,
                            modOpportunity.strf_OPPORTUNITY_ID, modOpportunity.strf_PRICE, modOpportunity.strf_QUANTITY,
                            modOpportunity.strf_EXTENDED_PRICE, modOpportunity.strfDEPOSIT, modOpportunity.strfOPTION_NOTES,
                            modOpportunity.strfCUSTOME_INSTRUCTIONS, modOpportunity.strf_SELECTED, modOpportunity.strf_NBHDP_PRODUCT_ID,
                            modOpportunity.strfNET_CONFIG, modOpportunity.strfADDED_BY_CHNG_ORDER_ID, modOpportunity.strfPRODUCT_NAME,
                            modOpportunity.strf_OPPORTUNITY__PRODUCT_ID);
                        if (rstOppProd.RecordCount > 0)
                        {
                            rstOppProd.MoveFirst();
                            while (!(rstOppProd.EOF))
                            {
                                // get the product
                                object vntOppProdId = rstOppProd.Fields[modOpportunity.strf_OPPORTUNITY__PRODUCT_ID].Value;
                                string strAction = TypeConvert.ToString(rstOppProd.Fields[modOpportunity.strf_SELECTED].Value);

                                if (strAction.ToLower() == "true")
                                {
                                    // selected is set, therefore set the values
                                    if (Convert.IsDBNull(rstOppProd.Fields[modOpportunity.strf_QUANTITY].Value))
                                    {
                                        rstOppProd.Fields[modOpportunity.strf_QUANTITY].Value = 1;
                                    }
                                }
                                else if (strAction.ToLower() == "false")
                                {
                                    // do nothing
                                }
                                rstOppProd.MoveNext();
                            }
                            objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstOppProd);
                        }
                    }
                    // Update the Totals for the Opportunity
                    CalculateTotals(opportunityId, false);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method retrieves the next sequential change order number
        /// for the specified opportunity
        /// </summary>
        /// <param name="opportunityId">the OpportunityId</param>
        /// <returns>The next sequential change order number</returns>
        /// <history>
        /// Revision#      Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual int GetNextChangeOrderNumber(object opportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstChangeOrder = objLib.GetRecordset(modOpportunity.strq_CHANGE_ORDER_FOR_OPP, 1, opportunityId,
                    modOpportunity.strfCHANGE_ORDER_NUMBER);
                int lngMaxNmber = 0;
                if (rstChangeOrder.RecordCount > 0)
                {
                    rstChangeOrder.MoveFirst();
                    while(!(rstChangeOrder.EOF))
                    {
                        int changeOrderNumber = TypeConvert.ToInt32(rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_NUMBER].Value);
                        if (changeOrderNumber > lngMaxNmber) lngMaxNmber = changeOrderNumber;
                        rstChangeOrder.MoveNext();
                    }
                }
                return lngMaxNmber + 1;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Overloaded method to convert a quote to a contract.  A Quote's division then on Convert to Sale then 
        /// Create a function Appserver Rule in the Contact NBHD Profile Dll to update the profiles
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual object[] ConvertToSale(object opportunityId)
        {
            return this.ConvertToSale(opportunityId, false);
        }

        /// <summary>
        /// This routine will convert a quote to a contract.  A Quote's division then on Convert to Sale then Create a
        /// function Appserver Rule in the Contact NBHD Profile Dll to update the profiles
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="finishConvert">Flag to indicate whether </param>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// 5.9.0          10/7/2010      Kevin Auh commented out setting actual decision date to today, changed the approval note
        ///                                         to use today rather than sales date, update write history to pass in the sale date
        ///                                         instead of today.
        /// </history>
        public virtual object[] ConvertToSale(object opportunityId, bool finishConvert)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                ContactProfileNeighborhood objContactProfileNBHD = (ContactProfileNeighborhood)RSysSystem.ServerScripts[modOpportunity.strsCONTACT_PROFILE_NBHD].CreateInstance();

                // get opportunity data
                Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strf_NBHD_PHASE_ID,
                    modOpportunity.strfLOT_ID, modOpportunity.strf_CONTACT_ID, modOpportunity.strf_ACCOUNT_MANAGER_ID,
                    modOpportunity.strfCONFIGURATION_COMPLETE, modOpportunity.strf_PIPELINE_STAGE, modOpportunity.strfACTUAL_DECISION_DATE,
                    modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfELEVATION_ID, modOpportunity.strfDEPOSIT_AMOUNT_TAKEN,
                    modOpportunity.strfREQUIRED_DEPOSIT_AMOUNT, modOpportunity.strf_STATUS, modOpportunity.strfACTUAL_REVENUE_DATE,
                    modOpportunity.strfECOE_DATE, modOpportunity.strfCONTRACT_APPROVED_BY_ID, modOpportunity.strfDESCRIPTION,
                    modOpportunity.strfWALK_IN_SALE_DATE, modOpportunity.strfSALE_DECLINED_DATE, modOpportunity.strfRESERVATION_DATE,
                    modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfELEVATION_ID, modOpportunity.strfQUOTE_TOTAL,
                    modOpportunity.strfCONTRACT_APPROVED_SUBMITTED, modOpportunity.strfCONTRACT_APPROVED_SUBMITTED_DATETIME, modOpportunity.strfTIC_CO_BUYER_ID);
                // check quote before connversion
                // check ECOE date

                rstOpportunity.MoveFirst();

                if (Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfECOE_DATE].Value))
                {
                    return new object[] {TypeConvert.ToString(LangDict.GetText(modOpportunity.strdEOCE_DATE_REQUIRED)),
                        DBNull.Value};
                }
                // check lot availability
                object vntLotId = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                object vntNBHDPhaseID = rstOpportunity.Fields[modOpportunity.strf_NBHD_PHASE_ID].Value;
                object vntNeighborhood_Id = objLib.SqlIndex(modOpportunity.strt_NBHD_PHASE, modOpportunity.strfNEIGHBORHOOD_ID,
                    vntNBHDPhaseID);
                string vntPipeline_Stage = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value);
                string vntStatus = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strf_STATUS].Value);
                string vntLotStatus = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_PRODUCT, modOpportunity.strfLOT_STATUS,
                    vntLotId));

                DateTime dtSaleDate = TypeConvert.ToDateTime (rstOpportunity.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value);

                if ((vntLotStatus == modOpportunity.strsAVAILABLE) || (vntLotStatus
                    == modOpportunity.strsRESERVED && !(Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfRESERVATION_DATE].Value)))
                    || (vntLotStatus == modOpportunity.strsSOLD && vntPipeline_Stage
                    == modOpportunity.strsCONTRACT && vntStatus == modOpportunity.strsON_HOLD
                    && finishConvert))
                {
                    // lot is available
                }
                else
                {
                    return new object[] {TypeConvert.ToString(LangDict.GetText(modOpportunity.strdLOT_RESERVED)),
                        string.Empty};  // Fix Issue 58097
                }
                // get the number of quotes which don't have a pipeline stage of quote - if there are more than one,
                // we can't convert to sale
                // The query needs to be changed so that the current record is excluded. Thus if records returned
                // is 1 then we assume that this is the current record.
                if (GetNumberOfNonQuotes(vntLotId) > 1)
                {
                    return new object[] {TypeConvert.ToString(LangDict.GetText(modOpportunity.strdCONTRACT_PENDING)),
                        DBNull.Value};
                }
                // See that quote has everything required for the plan
                string strInfoList = string.Empty;
                if (!((CheckCompleteness(opportunityId, out strInfoList))))
                {
                    if (strInfoList.Trim().Length > 0)
                    {
                        return  new object[] {strInfoList, DBNull.Value};
                    }
                }

                if ((strInfoList = OptionNeedsLocation(opportunityId)) != string.Empty)
                {
                    return new object[] { strInfoList, DBNull.Value };
                }

                if ((strInfoList = OptionsWithDuplicateLocations(opportunityId)) != string.Empty)
                {
                    return new object[] { strInfoList, DBNull.Value };
                }

                // pre conversion checks complete. convert to sale can proceed
                // update Quote
                rstOpportunity.Fields[modOpportunity.strfCONFIGURATION_COMPLETE].Value = true;
                rstOpportunity.Fields[modOpportunity.strf_PIPELINE_STAGE].Value = modOpportunity.strsCONTRACT;
                if ((TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strf_STATUS].Value) == modOpportunity.strsON_HOLD
                    && finishConvert) || TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strf_STATUS].Value)
                    != modOpportunity.strsON_HOLD)
                {
                    rstOpportunity.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsIN_PROGRESS;
                }
                //KA 10/7/10 commented out line below to leave the date alone
                //rstOpportunity.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value = DateTime.Today;

                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                rstOpportunity.Fields[modOpportunity.strfCONTRACT_APPROVED_BY_ID].Value = administration.CurrentUserRecordId;
                // set note
                object vntCurrentEmployeeId = administration.CurrentUserRecordId;
                string vntCurrentEmployeeFirstName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_FIRST_NAME,
                    vntCurrentEmployeeId));
                string vntCurrentEmployeeLastName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_LAST_NAME,
                    vntCurrentEmployeeId));
                //rstOpportunity.Fields[modOpportunity.strfDESCRIPTION].Value = TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdCONVERT_TO_SALE_NOTE, 
                //    new object[] {rstOpportunity.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value, 
                //        vntCurrentEmployeeFirstName + " " + vntCurrentEmployeeLastName})) + "\r\n" 
                //        + TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfDESCRIPTION].Value);
                //KA 10/14/10
                rstOpportunity.Fields[modOpportunity.strfDESCRIPTION].Value = TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdCONVERT_TO_SALE_NOTE,
                    new object[] {DateTime.Today,vntCurrentEmployeeFirstName + " " + vntCurrentEmployeeLastName})) + "\r\n"
                        + TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfDESCRIPTION].Value);
                if (Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfWALK_IN_SALE_DATE].Value))
                {
                    rstOpportunity.Fields[modOpportunity.strfWALK_IN_SALE_DATE].Value = DateTime.Today;
                }
                rstOpportunity.Fields[modOpportunity.strfSALE_DECLINED_DATE].Value = DBNull.Value;
                // set the sales request date to current date if it is null
                if (Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfCONTRACT_APPROVED_SUBMITTED].Value))
                {
                    rstOpportunity.Fields[modOpportunity.strfCONTRACT_APPROVED_SUBMITTED].Value = DateTime.Today;
                    rstOpportunity.Fields[modOpportunity.strfCONTRACT_APPROVED_SUBMITTED_DATETIME].Value = DateTime.Now;
                }

                // save changes
                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstOpportunity);

                object vntContactId = rstOpportunity.Fields[modOpportunity.strf_CONTACT_ID].Value;
                object vntCoBuyerId = rstOpportunity.Fields[modOpportunity.strfTIC_CO_BUYER_ID].Value;
                // update cobuyer status and contact buyer
                UpdateCoBuyerStatus(opportunityId, true, false);

                // update contact profile neighborhood
                UpdateContactProfileNeighborhood(vntContactId, vntNeighborhood_Id, null, DateTime.Today, 
                    DateTime.Today, DBNull.Value, null, null, null, DateTime.Today, null,
                    null, Type.Missing);
                
                object vntOpportunityId = rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                WriteContractHistoryRecords(vntLotId, vntOpportunityId, "Sold", dtSaleDate, true, null, false, false);


                Recordset rstCntNBHDProfile = objLib.GetRecordset(modOpportunity.strqCONTACT_PROFILE_NBHD_FOR_CONTACT, 2, vntContactId,
                    vntNeighborhood_Id, modOpportunity.strfCONTACT_PROFILE_NBHD_ID);
                if (rstCntNBHDProfile.RecordCount > 0)
                {
                    // update contact profile neighborhood type
                    rstCntNBHDProfile.MoveFirst();
                    object vntCntNBHDProfileID = rstCntNBHDProfile.Fields[modOpportunity.strfCONTACT_PROFILE_NBHD_ID].Value;
                    objContactProfileNBHD.UpdateNBHDPType(vntCntNBHDProfileID);
                }

                // BH - Apr. 5, 2005
                // When "Buyer As a Global Stage" flag is set to true for a Quote's division then
                if (CheckBuyerIsGlobalStage(opportunityId))
                {
                    objContactProfileNBHD.GlobalBuyerSale(opportunityId);
                }

                // update lot
                bool blnInventoryHome = false;
                Recordset rstLot = objLib.GetRecordset(vntLotId, modOpportunity.strt_PRODUCT, modOpportunity.strf_RN_DESCRIPTOR,
                    modOpportunity.strfLOT_NUMBER, modOpportunity.strfBLOCK, modOpportunity.strfTRACT, modOpportunity.strfNEIGHBORHOOD,
                    modOpportunity.strfPHASE, modOpportunity.strfLOT_STATUS, modOpportunity.strfOWNER_ID, modOpportunity.strfTYPE,
                    modOpportunity.strfPLAN_ID, modOpportunity.strfELEVATION_ID, modOpportunity.strfCONTRACT_CLOSE_DATE,
                    modOpportunity.strfSALES_DATE, modOpportunity.strfJOB_NUMBER, modOpportunity.strfRESERVATION_CONTRACT_ID,
                    modOpportunity.strfEST_CONTRACT_CLOSED_DATE, modOpportunity.strfTIC_CO_BUYER_ID);
                objLib.PermissionIgnored = true;
                if (rstLot.RecordCount > 0)
                {
                    rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsSOLD;
                    rstLot.Fields[modOpportunity.strfOWNER_ID].Value = vntContactId;
                    rstLot.Fields[modOpportunity.strfTIC_CO_BUYER_ID].Value = vntCoBuyerId;
                    rstLot.Fields[modOpportunity.strfSALES_DATE].Value = dtSaleDate;
                    rstLot.Fields[modOpportunity.strfRESERVATION_CONTRACT_ID].Value = opportunityId;
                    // change the inventory homsite to 'homesite' once lot is sold
                    if (TypeConvert.ToString(rstLot.Fields[modOpportunity.strfTYPE].Value) == modOpportunity.strsINVENTORY)
                    {
                        rstLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsLOT_TYPE_HOMESITE;
                        blnInventoryHome = true;
                    }

                    // Jul 20, 2005. Added by JWang. Update Lot's Estimated CLosed date from Quote's ECOE_Date
                    rstLot.Fields[modOpportunity.strfEST_CONTRACT_CLOSED_DATE].Value = rstOpportunity.Fields[modOpportunity.strfECOE_DATE].Value;

                    objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);
                }

                // Reading out strf_RN_DESCRIPTOR and strfJOB_NUMBER
                rstLot = objLib.GetRecordset(vntLotId, modOpportunity.strt_PRODUCT, modOpportunity.strf_RN_DESCRIPTOR,
                    modOpportunity.strfJOB_NUMBER);
                string strLotDescriptor = string.Empty;
                int vntJob_Number = 0;
                if (rstLot.RecordCount > 0)
                {
                    strLotDescriptor = TypeConvert.ToString(rstLot.Fields[modOpportunity.strf_RN_DESCRIPTOR].Value);
                    vntJob_Number = TypeConvert.ToInt32(rstLot.Fields[modOpportunity.strfJOB_NUMBER].Value);
                }

                // set the net config flag to true for al the options on the quote
                SetBaseConfiguration(opportunityId);

                // remove unselected options from all quotes for the lot
                Recordset rstUpdateOtherQuotes = objLib.GetRecordset(modOpportunity.strfUNSELECTED_OPTIONS_FOR_LOT, 1, vntLotId,
                    modOpportunity.strf_OPPORTUNITY__PRODUCT_ID);
                object parameterList = DBNull.Value;
                if (rstUpdateOtherQuotes.RecordCount > 0)
                {
                    rstUpdateOtherQuotes.MoveFirst();
                    IRForm rfrmForm = RSysSystem.Forms[modOpportunity.strrHB_OPPORTUNITY_PRODUCT];
                    while (!(rstUpdateOtherQuotes.EOF))
                    {
                        rfrmForm.DeleteFormData(rstUpdateOtherQuotes.Fields[modOpportunity.strf_OPPORTUNITY__PRODUCT_ID].Value,
                            ref parameterList);
                        rstUpdateOtherQuotes.MoveNext();
                    }
                }
                rstUpdateOtherQuotes.Close();
                rstUpdateOtherQuotes = null;


                //CMigles - Sept 15, 2010 - SEND EMAIL NOTIFICATIONS
                SendEmailNotifications(modOpportunity.strsCONVERT_TO_SALE, vntNeighborhood_Id, rstOpportunity, vntContactId);

                //// send email
                //// get recepient list from neighborhood notifcation team where notify on Sales Approved is true
                //Recordset rstEmailTo = objLib.GetRecordset(modOpportunity.strqNOTIFICATION_TEAM_FOR_NBHD_SALES_APPROVED, 1, vntNeighborhood_Id,
                //    modOpportunity.strf_EMPLOYEE_ID);
                //string strEmailTo = string.Empty;
                //if (rstEmailTo.RecordCount > 0)
                //{
                //    rstEmailTo.MoveFirst();
                //    StringBuilder emailToBuilder = new StringBuilder(); 
                //    while(!(rstEmailTo.EOF))
                //    {
                //        string strWorkEmail = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtEMPLOYEE, 
                //            modOpportunity.strf_WORK_EMAIL, rstEmailTo.Fields[modOpportunity.strf_EMPLOYEE_ID].Value));
                //        // add if not already there
                //        if (!emailToBuilder.ToString().Contains(strWorkEmail))
                //        {
                //            emailToBuilder.Append(strWorkEmail + ";");
                //        }
                //        rstEmailTo.MoveNext();
                //    }
                //    // strip out last ;
                //    strEmailTo = emailToBuilder.ToString();
                //    strEmailTo = strEmailTo.Substring(0, strEmailTo.Length - 1);
                //}

                //// all language strings are in nbhd_notification_team
                //ILangDict lngNBHD_Notification_Team = RSysSystem.GetLDGroup(modOpportunity.strgNBHD_NOTIFICATION_TEAM);
                //// set subject
                //object vntSalesRepId = rstOpportunity.Fields[modOpportunity.strf_ACCOUNT_MANAGER_ID].Value;
                //string vntSalesRepFirstName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, 
                //    modOpportunity.strf_FIRST_NAME, vntSalesRepId));
                //string vntSalesRepLastName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, 
                //    modOpportunity.strf_LAST_NAME, vntSalesRepId));
                //string strSubject = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdSALES_APPROVED_SUBJECT, 
                //    new object[] {vntSalesRepFirstName, vntSalesRepLastName, strLotDescriptor, 
                //    String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)) }));
                //// set message
                //string strMessage = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdSALES_APPROVED_MESSAGE1, 
                //    new object[] { DateTime.Today, vntSalesRepFirstName, vntSalesRepLastName, 
                //        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_FIRST_NAME, vntContactId)), 
                //        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_LAST_NAME, vntContactId)), 
                //        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strfHOME_PHONE, vntContactId)), 
                //        String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)), 
                //        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                //        rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value)), 
                //        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                //        rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value)) }));
                //strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdSALES_APPROVED_MESSAGE2,
                //    new object[] { vntJob_Number, TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfECOE_DATE].Value) }));
                //strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdSALES_APPROVED_MESSAGE3, 
                //    new object[] {vntSalesRepFirstName, vntSalesRepLastName, 
                //        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_WORK_EMAIL, vntSalesRepId)), 
                //        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strfWORK_PHONE, vntSalesRepId))}));
                //if (strEmailTo.Length > 0 && strSubject.Length > 0 && strMessage.Length > 0)
                //{
                //    SendSimpleMail(strEmailTo, strSubject, strMessage);
                //}



                // Apply Release Milestones to the Quote (copy over the Active Milestone  from the Quote's Release)
                ApplyReleaseMilestones(opportunityId);

                // Inactive all Quotes that are related to the Homesite sold.
                InactivateCustomerQuotes(DBNull.Value, opportunityId, InactiveQuoteReason.ConvertToSale);
                // Inactivate the Inventory Quote if an Inventory homesite
                if (blnInventoryHome)
                {
                    Recordset rstInv_Quote = objLib.GetRecordset(modOpportunity.strqINVENTORY_QUOTE_FOR_LOT, 1, vntLotId, modOpportunity.strfOPPORTUNITY_ID,
                        modOpportunity.strfSTATUS, modOpportunity.strfINACTIVE);
                    if (rstInv_Quote.RecordCount > 0)
                    {
                        rstInv_Quote.MoveFirst();
                        rstInv_Quote.Fields[modOpportunity.strfINACTIVE].Value = true;
                    }
                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstInv_Quote);
                }


                // Add homesite configuration
                TransitionPointParameter transitionPointParameter = (TransitionPointParameter) RSysSystem.ServerScripts
                    [AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                transitionPointParameter.SetUserDefinedParameter(1, opportunityId);
                parameterList = transitionPointParameter.ParameterList;
                RSysSystem.Forms[modOpportunity.strrLOT_CONFIGURATION].Execute(modOpportunity.strmCREATE_HOMESITE_CONFIGURATION,
                    ref parameterList);
                return new object[] {string.Empty, vntLotId};
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method will check if Buyer As a Global Stage flag is set for this quote's (opportunity) division
        /// </summary>
        /// <param name="opportunityId">OpportunityId</param>
        /// <returns>true or false</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual bool CheckBuyerIsGlobalStage(object opportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstDivision = objLib.GetRecordset(modOpportunity.strqDIVISION_FOR_OPPORTUNITY, 1, opportunityId,
                    modOpportunity.strfBUYER_IS_GLOBAL_STAGE);
                return TypeConvert.ToBoolean(rstDivision.Fields[modOpportunity.strfBUYER_IS_GLOBAL_STAGE].Value);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method will check if there are any missing categories for Plan and Opportunity
        /// </summary>
        /// <param name="opportunityId">OpportunityId</param>
        /// <returns>Flag to indicate if pass the completeness checking.</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual bool CheckCompleteness(object opportunityId)
        {
            string infoMessage = string.Empty;
            return CheckCompleteness(opportunityId, out infoMessage);
        }

        /// <summary>
        /// This method will check if there are any missing categories for Plan and Opportunity
        /// </summary>
        /// <param name="opportunityId">OpportunityId</param>
        /// <param name="outMessage">Out message</param>
        /// <returns>Flag to indicate if pass the completeness checking.</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual bool CheckCompleteness(object opportunityId, out string outMessage)
        {
            try
            {
                outMessage = string.Empty;
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                object vntPlanId = RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[modOpportunity.strfPLAN_NAME_ID].Index(opportunityId);
                object vntDivisionProductId = RSysSystem.Tables[modOpportunity.strt_NBHD_PRODUCT].Fields[modOpportunity.strf_DIVISION_PRODUCT_ID].Index(vntPlanId);

                Recordset rstMissingCategories = objLib.GetRecordset(modOpportunity.strqMISSING_CATEGORIES_PLAN_OPP, 2, vntDivisionProductId,
                    opportunityId, modOpportunity.strf_RN_DESCRIPTOR);
                if (rstMissingCategories.RecordCount > 0)
                {
                    StringBuilder statusList = new StringBuilder();
                    statusList.Append(TypeConvert.ToString(LangDict.GetText(modOpportunity.strdMISSING_REQUIRED_CATEGORIES))+ "\r\n");
                    rstMissingCategories.MoveFirst();
                    while(!(rstMissingCategories.EOF))
                    {
                        statusList.Append(rstMissingCategories.Fields[modOpportunity.strf_RN_DESCRIPTOR].Value + "\r\n");
                        rstMissingCategories.MoveNext();
                    }
                    outMessage = statusList.ToString();
                    return false;
                }
                else
                    return true;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method will check for missing Built Options for Opportunity
        /// </summary>
        /// <param name="opportunityId">the OpportunityId</param>
        /// <param name="lotId">Homesite Id</param>
        /// <param name="outMessage">output message</param>
        /// <returns>true or false</returns> 
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual bool VerifyBuiltOptions(object opportunityId, object lotId, out string outMessage)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstMissingBuiltOptions = objLib.GetRecordset(modOpportunity.strqMissing_Built_Options, 2, lotId,
                    opportunityId, modOpportunity.strf_RN_DESCRIPTOR);
                if (rstMissingBuiltOptions.RecordCount > 0)
                {
                    StringBuilder statusBuilder = new StringBuilder();
                    statusBuilder.Append(modOpportunity.strdMISSING_BUILT_OPTIONS + "</br><hr>");
                    rstMissingBuiltOptions.MoveFirst();
                    while (!(rstMissingBuiltOptions.EOF))
                    {
                        statusBuilder.Append(rstMissingBuiltOptions.Fields[modOpportunity.strf_RN_DESCRIPTOR].Value + "\r\n");
                        rstMissingBuiltOptions.MoveNext();
                    }
                    outMessage = statusBuilder.ToString();
                    return false;
                }
                else
                {
                    outMessage = string.Empty;
                    return true;
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Get the Contact_Release record, set the Type=Buyer and set the Sale_Date = Opportunity.Actual_Dec_Date
        /// </summary>
        /// <param name="contactId">Updated Contact Record</param>
        /// <param name="saleDate">The Actual Decision Date from the Opportunity</param>
        /// <history>
        /// Revision#    Date        Author  Description
        /// 3.8.0.0      5/12/2006   DYin    Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateContactBuyer(object contactId, DateTime saleDate)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstContact = objLib.GetRecordset(contactId, modOpportunity.strt_CONTACT, modOpportunity.strfTYPE, modOpportunity.strfWALK_IN_DATE,
                    modOpportunity.strfSALE_DATE);
                if (rstContact.RecordCount > 0)
                {
                    rstContact.MoveFirst();
                    if (TypeConvert.ToString(rstContact.Fields[modOpportunity.strfTYPE].Value) != modOpportunity.strsCLOSED)
                    {
                        rstContact.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsBUYER;
                    }
                    if (Convert.IsDBNull(rstContact.Fields[modOpportunity.strfWALK_IN_DATE].Value))
                    {
                        rstContact.Fields[modOpportunity.strfWALK_IN_DATE].Value = saleDate;
                    }
                    rstContact.Fields[modOpportunity.strfSALE_DATE].Value = saleDate;
                    objLib.SaveRecordset(modOpportunity.strt_CONTACT, rstContact);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Updates the type of the contact.
        /// Used when cancelling a contract
        /// </summary>
        /// <param name="contactId">Updated Contact Record</param>
        /// <param name="type">The new status of the contact</param>
        /// <history>
        /// Revision#    Date        Author  Description
        /// 3.8.0.0      5/12/2006   DYin    Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateContactType(object contactId, string type)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstContact = objLib.GetRecordset(contactId, modOpportunity.strt_CONTACT, modOpportunity.strfTYPE);
                if (rstContact.RecordCount > 0)
                {
                    rstContact.MoveFirst();
                    rstContact.Fields[modOpportunity.strfTYPE].Value = type;
                    objLib.SaveRecordset(modOpportunity.strt_CONTACT, rstContact);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Update all Oppoortunity Product records for Opportunity w Base Config = TRUE
        /// </summary>
        /// <param name="opportunityId">Target Opportunity Id</param>
        /// <history>
        /// Revision#    Date       Author    Description
        /// 3.8.0.0      5/12/2006  DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void SetBaseConfiguration(object opportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOpportunityProduct = objLib.GetRecordset(modOpportunity.strqSELECTED_OPTIONS_FOR_QUOTE, 1, opportunityId,
                    modOpportunity.strfNET_CONFIG);
                if (rstOpportunityProduct.RecordCount > 0)
                {
                    rstOpportunityProduct.MoveFirst();
                    while (!rstOpportunityProduct.EOF)
                    {
                        rstOpportunityProduct.Fields[modOpportunity.strfNET_CONFIG].Value = true;
                        rstOpportunityProduct.MoveNext();
                    }
                }
                // Save the Opportunity Product Record
                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstOpportunityProduct);

                Recordset rstOppAdjustments = objLib.GetRecordset(modOpportunity.strqOPP_ADJUSTMENT_FOR_OPP, 1, opportunityId,
                    modOpportunity.strfNET_CONFIG);
                if (rstOppAdjustments.RecordCount > 0)
                {
                    rstOppAdjustments.MoveFirst();
                    while (!rstOppAdjustments.EOF)
                    {
                        rstOppAdjustments.Fields[modOpportunity.strfNET_CONFIG].Value = true;
                        rstOppAdjustments.MoveNext();
                    }
                }
                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY_ADJUSTMENT, rstOppAdjustments);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sends Simple Mail
        /// </summary>
        /// <param name="toList">To recipients list</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body</param>
        /// <history>
        /// Revision#    Date       Author    Description
        /// 3.8.0.0      5/12/2006  DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void SendSimpleMail(string toList, string subject, string body)
        {
            IRSend objEmail = RSysSystem.CreateEmail();
            objEmail.NewMessage();
            objEmail.To = toList;
            objEmail.Subject =TypeConvert.ToString(subject);
            objEmail.Body = TypeConvert.ToString(body);
            objEmail.Send();
        }

        /// <summary>
        /// Update all Opportunities when the cancelled date is set
        /// </summary>
        /// <param name="opportunityId">Target Opportunity Id</param>
        /// <history>
        /// Revision#    Date       Author    Description
        /// 3.8.0.0      5/12/2006  DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void CloseCancelOpportunity(object opportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstQuote = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strfCANCEL_DATE,
                    modOpportunity.strfLOT_ID, modOpportunity.strf_STATUS, modOpportunity.strf_PIPELINE_STAGE, modOpportunity.strfACTUAL_REVENUE_DATE,
                    modOpportunity.strfDELTA_CANCEL_DATE, modOpportunity.strfDELTA_ACT_REV_DATE, modOpportunity.strf_CONTACT_ID,
                    modOpportunity.strf_NBHD_PHASE_ID, modOpportunity.strfWARRANTY_DATE, modOpportunity.strfSERVICE_DATE,
                    modOpportunity.strfCANCEL_NOTES);

                // update quote to closed
                object vntActualDate = rstQuote.Fields[modOpportunity.strfACTUAL_REVENUE_DATE].Value;
                bool boolClosedLot = false;
                if (!(Convert.IsDBNull(vntActualDate)))
                {
                    if (vntActualDate.Equals(rstQuote.Fields[modOpportunity.strfDELTA_ACT_REV_DATE].Value) == false)
                    {
                        boolClosedLot = true;
                        rstQuote.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsCLOSED;
                        rstQuote.Fields[modOpportunity.strfSERVICE_DATE].Value = vntActualDate;
                        rstQuote.Fields[modOpportunity.strfWARRANTY_DATE].Value = TypeConvert.ToDouble(vntActualDate) +
                            365.0;
                        object vntContactId = rstQuote.Fields[modOpportunity.strf_CONTACT_ID].Value;
                        object vntReleaseId = rstQuote.Fields[modOpportunity.strf_NBHD_PHASE_ID].Value;
                        rstQuote.Fields[modOpportunity.strfDELTA_ACT_REV_DATE].Value = rstQuote.Fields[modOpportunity.strfACTUAL_REVENUE_DATE].Value;
                        // Get Lot Id and call function to update it's status to "Closed"
                        object vntLotId = rstQuote.Fields[modOpportunity.strfLOT_ID].Value;
                        if (!(Convert.IsDBNull(vntLotId)))
                        {
                            UpdateLotStatusClosed(vntLotId);
                        }
                    }
                }

                UpdateCoBuyerStatus(opportunityId, false, true);
                // [Label: opportunity_save start]
                objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstQuote);
                // Check to see if all lots are closed - if so then close the Phase (Release)
                if (boolClosedLot)
                {
                    object vntReleaseId = rstQuote.Fields[modOpportunity.strf_NBHD_PHASE_ID].Value;
                    CheckLastLotClosed(vntReleaseId);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This routine will reset the Lot to Available
        /// </summary>
        /// <param name="lotId">the Lot to be reset</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#    Date       Author    Description
        /// 3.8.0.0      5/12/2006  DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void MoveLotToInventory(object lotId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // check the construction stage of the lot, if started then check to see if any of the options,
                // have been built then mark this lot as an inventory lot
                Recordset rstProduct = objLib.GetRecordset(lotId, modOpportunity.strt_PRODUCT, modOpportunity.strfLOT_STATUS, modOpportunity.strfTYPE);
                if (rstProduct.RecordCount > 0)
                {
                    rstProduct.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsAVAILABLE;
                    rstProduct.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsINVENTORY;
                    objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstProduct);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Update the Lot Site Status to Closed
        /// </summary>
        /// <param name="lotId">The Lot Id to be updated.</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#    Date        Author   Description
        /// 3.8.0.0      5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateLotStatusClosed(object lotId)
        {
            try
            {
                // Retrieve the Lot record for this Opportunity from Lot_Id; update to "Closed"
                if (!Convert.IsDBNull(lotId))
                {
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                    Recordset rstLot = objLib.GetRecordset(lotId, modOpportunity.strt_PRODUCT, modOpportunity.strfLOT_STATUS);
                    if (rstLot.RecordCount > 0)
                    {
                        rstLot.MoveFirst();
                        rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsSOLD;
                        // now status is sold when closing a quote
                    }

                    // Save the Updated Lot Record
                    objLib.PermissionIgnored = true;
                    objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Updates the Phase (Release) if all lots are 'Closed'
        /// </summary>
        /// <param name="releaseId">the release Id</param>
        /// <returns></returns>
        // None
        // Revision# Date Author Description
        // 3.8.0.0   5/12/2006  DYin  Converted to .Net C# code.
        protected virtual void CheckLastLotClosed(object releaseId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                // Query for Home Sites that are not Closed
                Recordset rstProduct = objLib.GetRecordset(modOpportunity.strqHOME_SITES_AVAILABLE_FOR_RELEASE, 1, releaseId,
                    modOpportunity.strf_PRODUCT_ID);

                // See if any Home Sites are not Closed
                // If none our found then 'Close' the Phase (Release)
                if (rstProduct.RecordCount == 0)
                {
                    Recordset rstPhase = objLib.GetRecordset(releaseId, modOpportunity.strt_NBHD_PHASE, modOpportunity.strfCLOSE_DATE,
                        modOpportunity.strf_STATUS);
                    if (rstPhase.RecordCount > 0)
                    {
                        rstPhase.MoveFirst();
                        rstPhase.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsCLOSED;
                        if (Convert.IsDBNull(rstPhase.Fields[modOpportunity.strfCLOSE_DATE].Value))
                        {
                            rstPhase.Fields[modOpportunity.strfCLOSE_DATE].Value = DateTime.Today;
                            // Save the Updated Lot Record
                            objLib.SaveRecordset(modOpportunity.strt_NBHD_PHASE, rstPhase);
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks to see if the form has the indicated secondaries.
        /// </summary>
        /// <param name="formName">Form name</param>
        /// <param name="recordId">Record Id</param>
        /// <param name="infoMessage">Out infomation message</param>
        /// <returns>True if the form has no children. False if the form has children</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/12/2006         DYin    Converted to .Net C# code
        /// 5.9      5/21/2007         JH      This function is deprecated in 5.9.     
        /// </history>
        protected virtual bool CanBeDeleted(string formName, object recordId, out string infoMessage)
        {
            throw new PivotalApplicationException("Deletion not allowed.");
        }

        /// <summary>
        /// Create an Inventory quote from a Contract
        /// </summary>
        /// <param name="contractOpportunityId">The contract id</param>
        /// <returns>a variant containing the newly created quote</returns>
        /// <history>
        /// Revision#    Date        Author   Description
        /// 3.8.0.0      5/12/2006   DYin     Converted to .Net C# code.
        /// 5.9.0        10/14/2010  KA       Don't copy transfer & rollback boolean to new inventory quote
        /// 5.9.1        11/02/2010  AM       Don't copy Reservation_Expiry_Date to new IQ
        /// </history>
        protected virtual object CreateInventoryQuoteFromContract(object contractOpportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstQuote = objLib.GetRecordset(contractOpportunityId, modOpportunity.strt_OPPORTUNITY);
                Recordset rstQuoteNew = objLib.GetNewRecordset(modOpportunity.strt_OPPORTUNITY);

                rstQuoteNew.AddNew(Type.Missing, Type.Missing);
                for(int i = 0; i < rstQuote.Fields.Count - 12; ++i)
                {
                    string fieldName = rstQuote.Fields[i].Name;
                    //KA 10/14/10
                    //AM 11/2/10
                    //AM 11/11/10 - added transferred from and transferred to as fields to NOT copy over to IQ
                    if (fieldName != modOpportunity.strfQUOTE_CREATE_DATE && fieldName != modOpportunity.strfCLOSE_DATE 
                        && fieldName != modOpportunity.strf_OPPORTUNITY_ID && fieldName != modOpportunity.strf_CONTACT_ID
                        && fieldName != modOpportunity.strfTIC_TRANSFER && fieldName != modOpportunity.strfTIC_ROLLBACK
                        && fieldName != modOpportunity.strfRESERVATIONEXPIRY && fieldName != modOpportunity.strfTIC_TRANSFER_FROM_LOT_ID
                        && fieldName != modOpportunity.strfTIC_TRANSFER_TO_LOT_ID
                        && fieldName != modOpportunity.strf_STATUS && fieldName.Substring(0, 3).ToUpper() != "RN_")
                        // && fieldName != modOpportunity.strfELEVATION_ID) Issue #65536-19950
                    {

                        if (rstQuote.Fields[fieldName].Type == DataTypeEnum.adBSTR || 
                            rstQuote.Fields[fieldName].Type == DataTypeEnum.adChar || 
                            rstQuote.Fields[fieldName].Type == DataTypeEnum.adVarChar ||
                            rstQuote.Fields[fieldName].Type == DataTypeEnum.adWChar || 
                            rstQuote.Fields[fieldName].Type ==DataTypeEnum.adVarWChar || 
                            rstQuote.Fields[fieldName].Type == DataTypeEnum.adLongVarChar || 
                            rstQuote.Fields[fieldName].Type == DataTypeEnum.adLongVarWChar)
                        {
                            if (!(Convert.IsDBNull(rstQuoteNew.Fields[fieldName].Value)))
                            {
                                rstQuoteNew.Fields[fieldName].Value = (TypeConvert.ToString(rstQuote.Fields[fieldName].Value)).Substring(0,
                                    rstQuote.Fields[fieldName].DefinedSize);
                            }
                        }
                        else
                            rstQuoteNew.Fields[fieldName].Value = rstQuote.Fields[fieldName].Value;
                    }
                }

                rstQuoteNew.Fields[modOpportunity.strfQUOTE_CREATE_DATE].Value = DateTime.Now;
                rstQuoteNew.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value = DBNull.Value;
                rstQuoteNew.Fields[modOpportunity.strf_CONTACT_ID].Value = DBNull.Value;
                rstQuoteNew.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsINVENTORY;
                rstQuoteNew.Fields[modOpportunity.strf_PIPELINE_STAGE].Value = modOpportunity.strsQUOTE;
                rstQuoteNew.Fields[modOpportunity.strfCANCEL_REASON_ID].Value = DBNull.Value;
                rstQuoteNew.Fields[modOpportunity.strfRESERVATION_DATE].Value = DBNull.Value;

                
                //CMigles - Sept 28, 2010 - Set future pricing, if available.
                if (!(Convert.IsDBNull(rstQuote.Fields[modOpportunity.strfTIC_FUTURE_CHANGE_PRICE].Value)))
                {
                    rstQuoteNew.Fields[modOpportunity.strfADDITIONAL_PRICE].Value = rstQuote.Fields[modOpportunity.strfTIC_FUTURE_CHANGE_PRICE].Value;
                    rstQuoteNew.Fields[modOpportunity.strfTIC_FUTURE_CHANGE_PRICE].Value = rstQuote.Fields[modOpportunity.strfTIC_FUTURE_CHANGE_PRICE].Value;

                }
                if (!(Convert.IsDBNull(rstQuote.Fields[modOpportunity.strfTIC_FUTURE_ELEVATION_PREMIUM].Value)))
                {
                    //AM2010.11.03 - the code is setting the wrong field!  Fixed to set new field
                    //rstQuoteNew.Fields[modOpportunity.strfTIC_ADDITIONAL_PREMIUM_PRICE].Value = rstQuote.Fields[modOpportunity.strfTIC_FUTURE_ELEVATION_PREMIUM].Value;
                    rstQuoteNew.Fields[modOpportunity.strfELEVATION_PREMIUM].Value = rstQuote.Fields[modOpportunity.strfTIC_FUTURE_ELEVATION_PREMIUM].Value;
                    rstQuoteNew.Fields[modOpportunity.strfTIC_FUTURE_ELEVATION_PREMIUM].Value = rstQuote.Fields[modOpportunity.strfTIC_FUTURE_ELEVATION_PREMIUM].Value;

                }

                //AM2010.11.08 - Added logic for setting the Future Lot Premium if exists
                if (!(Convert.IsDBNull(rstQuote.Fields[modOpportunity.strfTIC_FUTURE_LOT_PREMIUM].Value)))
                {
                    rstQuoteNew.Fields[modOpportunity.strfLOT_PREMIUM].Value
                        = TypeConvert.ToDecimal(rstQuote.Fields[modOpportunity.strfTIC_FUTURE_LOT_PREMIUM].Value);
                    rstQuoteNew.Fields[modOpportunity.strfTIC_FUTURE_LOT_PREMIUM].Value
                        = TypeConvert.ToDecimal(rstQuote.Fields[modOpportunity.strfTIC_FUTURE_LOT_PREMIUM].Value);
                }

                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstQuoteNew);
                object vntNewQuoteId = rstQuoteNew.Fields[modOpportunity.strf_OPPORTUNITY_ID].Value;

                // add new opp team
                CopyQuoteSecondary(contractOpportunityId, vntNewQuoteId, modOpportunity.strt_OPPORTUNITY_TEAM_MEMBER, modOpportunity.strf_OPPORTUNITY_ID,
                    modOpportunity.strf_OPPORTUNITY_TEAM_MEMBER_ID, false);

                // add options
                CopyQuoteSecondary(contractOpportunityId, vntNewQuoteId, modOpportunity.strt_OPPORTUNITY__PRODUCT, modOpportunity.strf_OPPORTUNITY_ID,
                    modOpportunity.strf_OPPORTUNITY__PRODUCT_ID, false);

                // add Location/preferences
                //CopyOptionSecondary(contractOpportunityId, vntNewQuoteId); was causing duplicate locations

                return vntNewQuoteId;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Copy A quote secondary
        /// </summary>
        /// <param name="sourceQuoteOpportunityId">The original quote id</param>
        /// <param name="targetQuoteOpportunityId">The new quote id</param>
        /// <param name="linkFieldName">The field we are linking to - this should always be oppId, but is here for
        /// further enhancibility</param>
        /// <param name="tableName">the name of the new secondary table</param>
        /// <param name="mainId">the main id of the new secondary (e.g. if the table is opp_team, then the main
        /// id would be opp_team_id). This allows us to copy all of the fields and ignore
        /// the rn_ fields and the main id field (the 6 fields that Relationship creates)</param>
        /// <returns>A boolean - true if the function created the secondaries properly, false otherwise</returns>
        /// <history>
        /// Revision# Date Author Description
        /// 3.8.0.0   5/12/2006  DYin  Converted to .Net C# code.
        /// </history>
        protected virtual bool CopyQuoteSecondary(object sourceQuoteOpportunityId, object targetQuoteOpportunityId, 
            string tableName, string linkFieldName, object mainId)
        {
            return this.CopyQuoteSecondary(sourceQuoteOpportunityId, targetQuoteOpportunityId, tableName, linkFieldName, mainId, false);
        }

        /// <summary>
        /// Copy A quote secondary
        /// </summary>
        /// <param name="sourceQuoteOpportunityId">The original quote id</param>
        /// <param name="targetQuoteOpportunityId">The new quote id</param>
        /// <param name="linkFieldName">The field we are linking to - this should always be oppId, but is here for
        /// further enhancibility</param>
        /// <param name="tableName">the name of the new secondary table</param>
        /// <param name="mainId">the main id of the new secondary (e.g. if the table is opp_team, then the main
        /// id would be opp_team_id). This allows us to copy all of the fields and ignore
        /// the rn_ fields and the main id field (the 6 fields that Relationship creates)</param>
        /// <param name="postSaleQuote">flag to indicate a Post Sale Quote</param>
        /// <returns>A boolean - true if the function created the secondaries properly, false otherwise</returns>
        /// <history>
        /// Revision# Date Author Description
        /// 3.8.0.0   5/12/2006  DYin  Converted to .Net C# code.
        /// 5.9.0.0   9/4/2007   BC    COpy of Packages
        /// 5.9.0.0   july/31/07  ML       Issue#65536-20113 and #65536-20115
        /// </history>
        protected virtual bool CopyQuoteSecondary(object sourceQuoteOpportunityId, object targetQuoteOpportunityId, 
            string tableName, string linkFieldName, object mainId, bool postSaleQuote)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOriginal = objLib.GetLinkedRecordset(tableName, linkFieldName,  sourceQuoteOpportunityId);
                
                
                

                
                
                Recordset rstNew = objLib.GetNewRecordset(tableName);

                // figure out what price to use, when copying quote use current price on the n.option
                string vntPipelineStage = TypeConvert.ToString(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY]
                    .Fields[modOpportunity.strfPIPELINE_STAGE].Index(targetQuoteOpportunityId));
                string vntOldQuotePipelineStage = TypeConvert.ToString(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfPIPELINE_STAGE].Index(sourceQuoteOpportunityId));
                string vntStatus = TypeConvert.ToString(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfSTATUS].Index(targetQuoteOpportunityId));

                if (rstOriginal.RecordCount > 0)
                {
                    rstOriginal.MoveFirst();
                    while(!(rstOriginal.EOF))
                    {
                        // if this is the opp product, because we are saving the recordset within the loop, we need
                        // to create a new
                        // record at the beginning of each iteration
                        bool blnUpdate = false;
                        decimal vntCurrentPrice = 0;
                        if (tableName == modOpportunity.strt_OPPORTUNITY__PRODUCT)
                        {
                            rstNew = objLib.GetNewRecordset(tableName);
                            object neighborhoodPhaseProductId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY__PRODUCT,
                                modOpportunity.strfNBHDP_PRODUCT_ID, rstOriginal.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value);
                            object vntDivisionProductId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY__PRODUCT,
                                modOpportunity.strfDIVISION_PRODUCT_ID, rstOriginal.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value);

                            if (Convert.IsDBNull(neighborhoodPhaseProductId) && Convert.IsDBNull(vntDivisionProductId))
                            {
                                blnUpdate = false;
                                // custom option
                            }
                            else if (Convert.IsDBNull(neighborhoodPhaseProductId) && !Convert.IsDBNull(vntDivisionProductId))
                            {
                                blnUpdate = false;
                                //Pacakge Options
                            }
                            else
                            {
                                if (vntPipelineStage == modOpportunity.strPIPELINE_QUOTE)
                                {
                                    object vntLot_Id = objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfLOT_ID,
                                        targetQuoteOpportunityId);
                                    string vntLot_Type = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_PRODUCT, modOpportunity.strfTYPE,
                                        vntLot_Id));
                                    // two situations
                                    if (vntStatus != modOpportunity.strQUOTE_STATUS_INVENTORY
                                        && (Convert.IsDBNull(vntLot_Id) || vntLot_Type != modOpportunity.strLOT_TYPE_INVENTORY))
                                    {
                                        // always update
                                        blnUpdate = true;
                                    }
                                    else if (vntLot_Type == modOpportunity.strLOT_TYPE_INVENTORY
                                        && (vntStatus == modOpportunity.strQUOTE_STATUS_INVENTORY
                                        || vntStatus == modOpportunity.strQUOTE_STATUS_IN_PROGRESS
                                        || vntStatus == modOpportunity.strQUOTE_STATUS_RESERVED))
                                    {
                                        // depends on division settings
                                        object vntDivisionId = objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT,
                                            modOpportunity.strfDIVISION_ID, neighborhoodPhaseProductId);
                                        object vntBuildOption = objLib.SqlIndex(modOpportunity.strt_DIVISION,
                                            modOpportunity.strfBUILD_OPTION_PRICING, vntDivisionId);
                                        if (vntBuildOption.Equals(modOpportunity.intBUILD_OPTION_FIXED))
                                        {   // fixed, only update if not built
                                            blnUpdate = !TypeConvert.ToBoolean(rstOriginal.Fields[modOpportunity.strfBUILT_OPTION].Value);
                                        }
                                        else if (vntBuildOption.Equals(modOpportunity.intBUILD_OPTION_FLOATING))
                                        {   // floating, always update
                                            blnUpdate = true;
                                        }
                                    }
                                }
                                else if (vntPipelineStage == modOpportunity.strPIPELINE_SALES_REQUEST || vntPipelineStage
                                    == modOpportunity.strsPOST_SALE)
                                {
                                    // get additional criteria
                                    DateTime vntContractApprovedSubmitted = TypeConvert.ToDateTime(objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY,
                                        modOpportunity.strfCONTRACT_APPROVED_SUBMITTED, sourceQuoteOpportunityId));
                                    DateTime vntCreateDate = TypeConvert.ToDateTime(objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfQUOTE_CREATE_DATE,
                                        targetQuoteOpportunityId));
                                    if (TypeConvert.ToDouble(rstOriginal.Fields[modOpportunity.strfSELECTED].Value) == -1
                                        || vntCreateDate <= vntContractApprovedSubmitted)
                                    {
                                        // do not update
                                    }
                                    else if (vntCreateDate > vntContractApprovedSubmitted)
                                    {
                                        // depends on division settings
                                        object vntDivisionId = objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT,
                                            modOpportunity.strfDIVISION_ID, neighborhoodPhaseProductId);
                                        object vntStndOption = objLib.SqlIndex(modOpportunity.strt_DIVISION, modOpportunity.strfSTANDARD_OPTION_PRICING,
                                            vntDivisionId);
                                        if (modOpportunity.intSTANDARD_OPTION_FIXED.Equals(vntStndOption))
                                        {
                                            // fixed, do not update
                                        }
                                        else if (modOpportunity.intBUILD_OPTION_FLOATING.Equals(vntStndOption))
                                        {
                                            // floating, always update
                                            blnUpdate = true;
                                        }
                                    }
                                }
                                else if (vntPipelineStage == modOpportunity.strPIPELINE_CANCELED || vntPipelineStage
                                    == modOpportunity.strPIPELINE_CLOSED)
                                {
                                    // do not update
                                }
                            }
                            if (blnUpdate)
                            {// figure out what the next price is (if there is any) and proceed with the update
                                vntCurrentPrice = GetOptionNextPrice(neighborhoodPhaseProductId); //((0));
                            }
                            else
                            {   // price is what the option price is
                                // ISSUE: Method or data member not found: 'Fields'
                                vntCurrentPrice = TypeConvert.ToDecimal(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY__PRODUCT].Fields[modOpportunity.strfPRICE].Index(rstOriginal.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value));
                            }
                        }
                        bool bolAddNew = true;

                        // don't copy over non-built options for canceled contracts
                        if (!postSaleQuote && tableName == modOpportunity.strt_OPPORTUNITY__PRODUCT && TypeConvert.ToString(vntOldQuotePipelineStage)
                            == modOpportunity.strsCANCELLED)
                        {
                            if (TypeConvert.ToDouble(rstOriginal.Fields[modOpportunity.strfBUILD_OPTION].Value) == 0)
                            {
                                //CMigles - COPY ALL OPTIONS FROM THE CANCELLED CONTRACT INTO THE NEW INVENTORY QUOTE
                                //bolAddNew = false;
                                //AM2010.12.07 - need to set to true, so that non-built options get carried over to the IQ
                                bolAddNew = true;
                            }
                        }
                        // added by CLangan - 05/30/2005 - don't copy over unselected options for PSQ - don't copy over
                        // the adjustments unless they are selected
                        if (postSaleQuote && (tableName == modOpportunity.strt_OPPORTUNITY__PRODUCT || tableName == modOpportunity.strtOPPORTUNITY_ADJUSTMENT))
                        {
                            if (TypeConvert.ToBoolean(rstOriginal.Fields[modOpportunity.strfSELECTED].Value) == false)
                            {
                                bolAddNew = false;
                            }
                        }
                        // Fix Issue #58099 - Copying Adjustments which have been selected from a IQ to a CQ
                        if (tableName == modOpportunity.strtOPPORTUNITY_ADJUSTMENT)
                        {
                            if (TypeConvert.ToDouble(rstOriginal.Fields[modOpportunity.strfSELECTED].Value) == 0)
                            {
                                bolAddNew = false;
                            }
                        }
                        
                        //For Package Components
                        if (tableName == modOpportunity.strt_OPPORTUNITY__PRODUCT && !Convert.IsDBNull(rstOriginal.Fields[modOpportunity.strfPARENT_PACK_OPPPROD_ID].Value))
                            bolAddNew = false;

                        if (bolAddNew)
                        {
                            rstNew.AddNew(Type.Missing, Type.Missing);

                            foreach (Field field in rstNew.Fields)
                            {
                                string fieldName = field.Name;
                                // May 4, 2005 - BH
                                // Use current price when ...
                                // TODO (Di Yin) strLotType is never assigned, I temporaryl  assign it to empty.
                                string strLotType = string.Empty;

                                if (fieldName == modOpportunity.strf_OPPORTUNITY_ID)
                                {
                                    rstNew.Fields[fieldName].Value = targetQuoteOpportunityId;
                                }
                                //CMigles - NEED TO SET THE PREPLOT FLAG TO TRUE.
                                //if (fieldName == modOpportunity.strfTIC_PREPLOT_OPTION)
                                //{
                                //    rstNew.Fields[fieldName].Value = true;
                               // }
                                else
                                {
                                    // Fix Issue #65536-15076 - Not copying the Built_Option flag.
                                    switch (tableName)
                                    {
                                        case modOpportunity.strt_OPPORTUNITY__PRODUCT:
                                            if ((fieldName == modOpportunity.strfBUILT_OPTION)
                                                && (vntOldQuotePipelineStage == modOpportunity.strsCONTRACT) && !postSaleQuote)
                                            {
                                                rstNew.Fields[fieldName].Value = false;
                                            }
                                            else if (strLotType != modOpportunity.strLOT_TYPE_INVENTORY
                                                && fieldName == modOpportunity.strfPRICE)
                                            {
                                                rstNew.Fields[fieldName].Value = vntCurrentPrice;
                                            }
                                            else if (postSaleQuote && fieldName == modOpportunity.strfORIG_OPP_PROD_ID)
                                            {   // Post Sale Quote Logic
                                                // set the origional opp prod id field to the source opp prod id
                                                rstNew.Fields[fieldName].Value = rstOriginal.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value;
                                            }
                                            else
                                            {
                                                rstNew.Fields[fieldName].Value = objLib.GetValidAssignValue(rstNew.Fields[fieldName], rstOriginal.Fields[fieldName].Value);
                                            }
                                            break;
                                        case modOpportunity.strtOPPORTUNITY_ADJUSTMENT:
                                            //ML july 31 07 Issue#65536-20113 and #65536-20115
                                            //if (postSaleQuote)
                                            //{
                                            //    rstNew.Fields[modOpportunity.strfCOPY_OF_ADJUSTMENT_ID].Value = rstOriginal.Fields[modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID].Value;
                                            //}
                                            //else
                                            {
                                                rstNew.Fields[fieldName].Value = objLib.GetValidAssignValue(rstNew.Fields[fieldName], rstOriginal.Fields[fieldName].Value);
                                            }
                                            break;
                                        default:
                                            rstNew.Fields[fieldName].Value = objLib.GetValidAssignValue(rstNew.Fields[fieldName], rstOriginal.Fields[fieldName].Value);
                                            break;
                                    }
                                }
                            }
                            //ML july 31 07 
                            if (postSaleQuote && (tableName == modOpportunity.strtOPPORTUNITY_ADJUSTMENT))
                            {
                                rstNew.Fields[modOpportunity.strfCOPY_OF_ADJUSTMENT_ID].Value = rstOriginal.Fields[modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID].Value;
                            }
                        
                            // if this is the opp product table, save the record and copy the secondaries on the opp
                            // product, otherwise
                            // save the recordset at the end of the loop to save time
                            if (tableName == modOpportunity.strt_OPPORTUNITY__PRODUCT)
                            {
                                objLib.SaveRecordset(tableName, rstNew);
                                object vntOppProductId = rstNew.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value;
                                this.CopyOptionSecondaryByOption(rstOriginal.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                                    vntOppProductId);
                            }
                        }
                        rstOriginal.MoveNext();
                    }
                    if (tableName != modOpportunity.strt_OPPORTUNITY__PRODUCT)
                    {
                        objLib.SaveRecordset(tableName, rstNew);
                    }

                }
                return true;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will cancel an existing contract
        /// </summary>
        /// <param name="opportunityId">the opportunity Id</param>
        /// <param name="sameLot">true or false</param>
        /// <returns>Flag to indicate whether cancel the contract.</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual bool CancelContract(object opportunityId, bool sameLot )
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // update the quote
                Recordset rstQuote = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strfCANCEL_DATE,
                    modOpportunity.strfLOT_ID, modOpportunity.strf_STATUS, modOpportunity.strf_PIPELINE_STAGE, modOpportunity.strfACTUAL_REVENUE_DATE,
                    modOpportunity.strfDELTA_CANCEL_DATE, modOpportunity.strfDELTA_ACT_REV_DATE, modOpportunity.strf_CONTACT_ID,
                    modOpportunity.strf_NBHD_PHASE_ID, modOpportunity.strfPLAN_BUILT, modOpportunity.strfWARRANTY_DATE,
                    modOpportunity.strfSERVICE_DATE, modOpportunity.strfCANCEL_NOTES, modOpportunity.strfCANCEL_REQUEST_DATE,
                    modOpportunity.strfNEIGHBORHOOD_ID, modOpportunity.strfCANCEL_APPROVED_BY, modOpportunity.strfCANCEL_DECLINED_DATE,
                    modOpportunity.strfCANCEL_DECLINED_By, modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfELEVATION_BUILT);
                object vntCurrentUserId = RSysSystem.CurrentUserId();

                bool blnPlanBuilt = false;
                bool blnElevationBuilt = false;
                object vntLotId = DBNull.Value;
                object vntNeighborhoodId = DBNull.Value;
                object vntContactId = DBNull.Value;
                if (rstQuote.RecordCount > 0)
                {
                    vntLotId = rstQuote.Fields[modOpportunity.strfLOT_ID].Value;
                    vntNeighborhoodId = rstQuote.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                    vntContactId = rstQuote.Fields[modOpportunity.strfCONTACT_ID].Value;

                    rstQuote.Fields[modOpportunity.strf_PIPELINE_STAGE].Value = modOpportunity.strsCANCELLED;
                    rstQuote.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsCANCELLED;
                    rstQuote.Fields[modOpportunity.strfCANCEL_NOTES].Value = rstQuote.Fields[modOpportunity.strfCANCEL_NOTES].Value + "\r\n" + modOpportunity.strlSALE_CANCELED_BY + RSysSystem.CurrentUserName() + modOpportunity.strlON + DateTime.Today.ToShortDateString();
                    
                    
                    rstQuote.Fields[modOpportunity.strfDELTA_CANCEL_DATE].Value = DateTime.Today;
                    rstQuote.Fields[modOpportunity.strfCANCEL_DATE].Value = DateTime.Today;
                    rstQuote.Fields[modOpportunity.strfCANCEL_REQUEST_DATE].Value = DateTime.Today;
                    rstQuote.Fields[modOpportunity.strfCANCEL_APPROVED_BY].Value = vntCurrentUserId;
                    rstQuote.Fields[modOpportunity.strfCANCEL_DECLINED_DATE].Value = DBNull.Value;
                    rstQuote.Fields[modOpportunity.strfCANCEL_DECLINED_By].Value = DBNull.Value;

                    blnPlanBuilt = TypeConvert.ToBoolean(rstQuote.Fields[modOpportunity.strfPLAN_BUILT].Value);
                    blnElevationBuilt = TypeConvert.ToBoolean(rstQuote.Fields[modOpportunity.strfELEVATION_BUILT].Value);

                    // update NBHD Profile
                    UpdateContactProfileNeighborhood(vntContactId, vntNeighborhoodId, null, null, null, null, null, DateTime.Today,
                        DateTime.Today, null, null, null, null);
                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstQuote);
                }

                // RY 11/28/2005: Inactivate the Post Sales Quotes if the Contract is cancelled.
                Recordset rstPostSaleQuotes = objLib.GetRecordset(modOpportunity.strqACTIVE_POST_SALE_QUOTES_FOR_OPP, 2, opportunityId,
                    modOpportunity.strsPOST_SALE, modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfSTATUS, modOpportunity.strfINACTIVE);
                if (rstPostSaleQuotes.RecordCount > 0)
                {
                    rstPostSaleQuotes.MoveFirst();
                    while(!(rstPostSaleQuotes.EOF))
                    {
                        // inactivate the quote
                        rstPostSaleQuotes.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsINACTIVE;
                        rstPostSaleQuotes.Fields[modOpportunity.strfINACTIVE].Value = true;
                        rstPostSaleQuotes.MoveNext();
                    }
                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstPostSaleQuotes);
                }

                // update the contact
                UpdateCoBuyerStatus(opportunityId, false, false);

                // get the lot recordset
                Recordset rstLot = objLib.GetRecordset(vntLotId, modOpportunity.strt_PRODUCT, modOpportunity.strfTYPE, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                    modOpportunity.strfLOT_STATUS, modOpportunity.strfOWNER_ID, modOpportunity.strfOWNER_NAME, modOpportunity.strfCONTRACT_CLOSE_DATE,
                    modOpportunity.strfPLAN_ID, modOpportunity.strfELEVATION_ID, modOpportunity.strfSALES_DATE, modOpportunity.strfRESERVATION_CONTRACT_ID,
                    modOpportunity.strfRESERVED_DATE);
                object vntPrevOwner = DBNull.Value;

                // if the lot is under construction, make sure the type is set to inventory and there is an
                // inventory quote created
                if (rstLot.RecordCount > 0)
                {
                    if (Convert.IsDBNull(rstLot.Fields[modOpportunity.strfTYPE].Value))
                    {
                        rstLot.Fields[modOpportunity.strfTYPE].Value = string.Empty;
                    }
                    rstLot.Fields[modOpportunity.strfCONTRACT_CLOSE_DATE].Value = DBNull.Value;
                    rstLot.Fields[modOpportunity.strfSALES_DATE].Value = DBNull.Value;
                    if (sameLot != true)
                    {
                        rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsAVAILABLE;
                        vntPrevOwner = rstLot.Fields[modOpportunity.strfOWNER_ID].Value;
                        rstLot.Fields[modOpportunity.strfOWNER_ID].Value = DBNull.Value;
                        rstLot.Fields[modOpportunity.strfOWNER_NAME].Value = DBNull.Value;
                        rstLot.Fields[modOpportunity.strfPLAN_ID].Value = DBNull.Value;

                        //Keep Elevation_Id as is
                        //rstLot.Fields[modOpportunity.strfELEVATION_ID].Value = DBNull.Value;

                        rstLot.Fields[modOpportunity.strfRESERVATION_CONTRACT_ID].Value = DBNull.Value;
                        rstLot.Fields[modOpportunity.strfRESERVED_DATE].Value = DBNull.Value;
                    }
                    // if the plan built flag is set then set it on this quote
                    if (blnPlanBuilt)
                    {
                        rstLot.Fields[modOpportunity.strfPLAN_ID].Value = rstQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value;
                    }

                    if ((!(Convert.IsDBNull(rstLot.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value)) || blnPlanBuilt)
                        && (TypeConvert.ToString(rstLot.Fields[modOpportunity.strfTYPE].Value)) != modOpportunity.strsINVENTORY)
                    {
                        // this lot is under contstruction
                        rstLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsINVENTORY;
                    }

                    // make sure an inventory quote doesn't already exist
                    Recordset rstInvQuote = objLib.GetRecordset(modOpportunity.strqINVENTORY_QUOTE_FOR_LOT, 1, vntLotId, modOpportunity.strfOPPORTUNITY_ID);

                    // based on the previous statements, the check for not null construction stage and type = Inventory,
                    // it's better to make the entire check in case something changes later.
                    if (rstInvQuote.RecordCount == 0 && (TypeConvert.ToString(rstLot.Fields[modOpportunity.strfTYPE].Value))
                        == modOpportunity.strsINVENTORY)
                    {
                        // create an inventory quote from the contract
                        object newOpportunityId = CreateInventoryQuoteFromContract(opportunityId);
                       
                        //CMigles - Set all options preplot = true
                        SetOptionsPreplotFlag(newOpportunityId);
                        //AM2011.03.03 - Combine pre-plots and non-preplots on cancels
                        CombinePrePlotAndNonPrePlotsForSameOption(newOpportunityId);
                        CalculateTotals(newOpportunityId, false);
                    }


                    objLib.PermissionIgnored = true;
                    objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);

                    if (!sameLot)
                    {
                        // add lot contact
                        Recordset rstNewLotContact = objLib.GetNewRecordset(modOpportunity.strtLOT__CONTACT, modOpportunity.strfPRODUCT_ID,
                            modOpportunity.strf_CONTACT_ID, modOpportunity.strfTYPE);
                        rstNewLotContact.AddNew(Type.Missing, Type.Missing);
                        rstNewLotContact.Fields[modOpportunity.strf_CONTACT_ID].Value = vntPrevOwner;
                        rstNewLotContact.Fields[modOpportunity.strfPRODUCT_ID].Value = vntLotId;
                        rstNewLotContact.Fields[modOpportunity.strfTYPE].Value = 0;

                        objLib.SaveRecordset(modOpportunity.strtLOT__CONTACT, rstNewLotContact);

                    }
                }

                // Inactive Unbuilt Lot Configurations
                TransitionPointParameter transitionPointParameter = (TransitionPointParameter)RSysSystem.ServerScripts
                    [AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                transitionPointParameter.SetUserDefinedParameter(1, vntLotId);
                object parameterList = transitionPointParameter.ParameterList;

                RSysSystem.Forms[modOpportunity.strrLOT_CONFIGURATION].Execute(modOpportunity.strmINACTIVATE_UNBUILT_LOT_CONFIGURATIONS,
                    ref parameterList);


                //Ensure to inactive escrow records associated to the cancelled contract
                InactivateCancelledEscrow(opportunityId);

                return true;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Copy Secondaries for a opportunity product
        /// </summary>
        /// <param name="sourceOpportunityId">The original quote id</param>
        /// <param name="targetOpportunityId">The new oportunity Id</param>
        /// <returns>a boolean - true if the function created the secondaries properly, false otherwise</returns>
        // Revision# Date Author Description
        // 3.8.0.0   5/12/2006  DYin  Converted to .Net C# code.
        // HB
        protected virtual bool CopyOptionSecondary(object sourceOpportunityId, object targetOpportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                if ((sourceOpportunityId != DBNull.Value))
                {
                    // get the options on the quote
                    Recordset rstOptions = objLib.GetRecordset(modOpportunity.strqOPP_PRODUCTS_FOR_OPP, 1, sourceOpportunityId, modOpportunity.strfLOCATION_ID,
                        modOpportunity.strfOPPORTUNITY__PRODUCT_ID);
                    if (rstOptions.RecordCount > 0)
                    {
                        rstOptions.MoveFirst();
                        while (!(rstOptions.EOF))
                        {
                            Recordset rstOPLocation = objLib.GetRecordset(modOpportunity.strqOP_LOCS_FOR_OP, 1, rstOptions.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                                modOpportunity.strfLOCATION_QUANTITY, modOpportunity.strfPREFERENCE_LIST, modOpportunity.strfOPP_PRODUCT_LOCATION_ID,
                                modOpportunity.strfLOCATION_ID);
                            if (rstOPLocation.RecordCount > 0)
                            {
                                rstOPLocation.MoveFirst();
                                while (!(rstOPLocation.EOF))
                                {
                                    // attributes?
                                    Recordset rstOPAttrPref = objLib.GetRecordset(modOpportunity.strqOP_LOC_ATTR_PREF_FOR_OPLOC, 1,
                                        rstOPLocation.Fields[modOpportunity.strfOPP_PRODUCT_LOCATION_ID].Value, modOpportunity.strfOPP_PRODUCT_LOCATION_ID,
                                        modOpportunity.strfATTRIBUTE, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID);
                                    // create a new one
                                    Recordset rstNewOPLoc = objLib.GetNewRecordset(modOpportunity.strtOPP_PRODUCT_LOCATION, modOpportunity.strfLOCATION_ID,
                                        modOpportunity.strfLOCATION_QUANTITY, modOpportunity.strfPREFERENCE_LIST, modOpportunity.strfOPPORTUNITY_ID,
                                        modOpportunity.strfOPP_PRODUCT_ID);
                                    rstNewOPLoc.AddNew(Type.Missing, Type.Missing);
                                    rstNewOPLoc.Fields[modOpportunity.strfLOCATION_ID].Value = rstOPLocation.Fields[modOpportunity.strfLOCATION_ID].Value;
                                    rstNewOPLoc.Fields[modOpportunity.strfLOCATION_QUANTITY].Value = rstOPLocation.Fields[modOpportunity.strfLOCATION_QUANTITY].Value;
                                    rstNewOPLoc.Fields[modOpportunity.strfPREFERENCE_LIST].Value = rstOPLocation.Fields[modOpportunity.strfPREFERENCE_LIST].Value;
                                    rstNewOPLoc.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = targetOpportunityId;
                                    rstNewOPLoc.Fields[modOpportunity.strfOPP_PRODUCT_ID].Value = rstOptions.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value;
                                    objLib.SaveRecordset(modOpportunity.strtOPP_PRODUCT_LOCATION, rstNewOPLoc);
                                    object vntOPLocId = rstNewOPLoc.Fields[modOpportunity.strfOPP_PRODUCT_LOCATION_ID].Value;
                                    if (rstOPAttrPref.RecordCount > 0)
                                    {
                                        rstOPAttrPref.MoveFirst();
                                        while (!(rstOPAttrPref.EOF))
                                        {
                                            // creat new atrr/pref for Op location
                                            Recordset rstNewAttrPref = objLib.GetNewRecordset(modOpportunity.strtOPPPROD_ATTR_PREF,
                                                modOpportunity.strfATTRIBUTE, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID,
                                                modOpportunity.strfOPP_PRODUCT_LOCATION_ID);
                                            rstNewAttrPref.AddNew(Type.Missing, Type.Missing);
                                            rstNewAttrPref.Fields[modOpportunity.strfOPP_PRODUCT_LOCATION_ID].Value = vntOPLocId;
                                            rstNewAttrPref.Fields[modOpportunity.strfATTRIBUTE].Value = rstOPAttrPref.Fields[modOpportunity.strfATTRIBUTE].Value;
                                            rstNewAttrPref.Fields[modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID].Value
                                                = rstOPAttrPref.Fields[modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID].Value;
                                            objLib.SaveRecordset(modOpportunity.strtOPPPROD_ATTR_PREF, rstNewAttrPref);

                                            rstOPAttrPref.MoveNext();
                                        }
                                    }
                                    rstOPLocation.MoveNext();
                                }
                            }
                            rstOptions.MoveNext();
                        }
                    }
                }
                return true;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        /// <summary>
        /// Get the number of quotes where the pipeline stage is not equal to quote
        /// </summary>
        /// <param name="lotId">the Lot id</param>
        /// <returns>
        /// GetNumberOfNonQuotes - integer value</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// when the Status or Pipeline_Stage field is null.
        /// </history>
        protected virtual int GetNumberOfNonQuotes(object lotId)
        {
            try
            {
                int intQuotes = 0;

                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstRecordset = objLib.GetRecordset(modOpportunity.strqQUOTES_FOR_LOT_ID, 1, lotId, modOpportunity.strf_PIPELINE_STAGE,
                    modOpportunity.strf_STATUS);

                if (rstRecordset.RecordCount == 0)
                    return intQuotes;
                else
                {
                    rstRecordset.MoveFirst();
                    while (!(rstRecordset.EOF))
                    {
                        string vntStatus = TypeConvert.ToString(rstRecordset.Fields[modOpportunity.strf_STATUS].Value);
                        string vntPipelineStage = TypeConvert.ToString(rstRecordset.Fields[modOpportunity.strf_PIPELINE_STAGE].Value);
                        if (vntPipelineStage == modOpportunity.strsQUOTE || vntStatus
                            == modOpportunity.strsINVENTORY || vntStatus == modOpportunity.strsINACTIVE
                            || vntPipelineStage == modOpportunity.strsPOST_SALE
                            || vntPipelineStage == modOpportunity.strsPOST_BUILD_ACCEPTED
                            || vntPipelineStage == modOpportunity.strsPOST_BUILD_QUOTE
                            || (vntPipelineStage == modOpportunity.strsCONTRACT
                            && (vntStatus == modOpportunity.strsCANCELLED || vntStatus
                            == modOpportunity.strsON_HOLD) || vntPipelineStage
                            == modOpportunity.strsCANCELLED && vntStatus == modOpportunity.strsCANCELLED))
                        {
                            // do nothing - it's easier to test for existence
                        }
                        else
                        {
                            intQuotes = (intQuotes + 1);
                        }
                        rstRecordset.MoveNext();
                    }
                    return intQuotes;
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Set all of the quotes for this lot to a status of Not Pursued whenvever a quote becomes contract
        /// </summary>
        /// <param name="lotId">the Lot id</param>
        /// <param name="opportunityId">this opportunity - so that we don't over-write it's status</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual bool SetQuotesToNotPursued(object lotId, object opportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstRecordset = objLib.GetLinkedRecordset(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfLOT_ID,
                    lotId, modOpportunity.strf_OPPORTUNITY_ID, modOpportunity.strf_STATUS, modOpportunity.strf_PIPELINE_STAGE);

                if (rstRecordset.RecordCount > 0)
                {
                    rstRecordset.MoveFirst();
                    while (!(rstRecordset.EOF))
                    {
                        if (!((RSysSystem.EqualIds(opportunityId, rstRecordset.Fields[modOpportunity.strf_OPPORTUNITY_ID].Value)))
                            && (TypeConvert.ToString(rstRecordset.Fields[modOpportunity.strf_PIPELINE_STAGE].Value) == modOpportunity.strsQUOTE
                            && TypeConvert.ToString(rstRecordset.Fields[modOpportunity.strf_STATUS].Value) == modOpportunity.strsIN_PROGRESS))
                        {
                            rstRecordset.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsNOT_PURSUED;
                        }
                        rstRecordset.MoveNext();
                    }
                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstRecordset);
                }
                return true;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method gets the next price for an option, if there is one else the current price is used.
        /// </summary>
        /// <param name="releaseId">ReleaseId</param>
        /// <param name="opportunityId">the OpportunityId</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void SetSalesTeam(object releaseId, object opportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                if ((releaseId != DBNull.Value))
                {
                    // Added by CLangan 01/14/05 - make sure the sales team hasn't already been added - exit if it has
                    Recordset rstSalesTeam = objLib.GetRecordset(modOpportunity.strqTEAM_MEMBERS_FOR_OPP, 1, opportunityId, modOpportunity.strf_OPPORTUNITY_TEAM_MEMBER_ID);
                    if (rstSalesTeam.RecordCount == 0)
                    {
                        Recordset rstRelTeam = objLib.GetRecordset(modOpportunity.strqSALES_TEAM_FOR_RELEASE, 1, releaseId, modOpportunity.strf_EMPLOYEE_ID,
                            modOpportunity.strfROLE_ID);
                        if (rstRelTeam.RecordCount > 0)
                        {
                            rstRelTeam.MoveFirst();

                            Recordset rstTeam = objLib.GetNewRecordset(modOpportunity.strt_OPPORTUNITY_TEAM_MEMBER, modOpportunity.strf_EMPLOYEE_ID,
                                modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfROLE_ID);
                            while (!(rstRelTeam.EOF))
                            {
                                rstTeam.AddNew(Type.Missing, Type.Missing);
                                rstTeam.Fields[modOpportunity.strf_EMPLOYEE_ID].Value = rstRelTeam.Fields[modOpportunity.strf_EMPLOYEE_ID].Value;
                                rstTeam.Fields[modOpportunity.strfROLE_ID].Value = rstRelTeam.Fields[modOpportunity.strfROLE_ID].Value;
                                rstTeam.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = opportunityId;
                                rstRelTeam.MoveNext();
                            }
                            objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY_TEAM_MEMBER, rstTeam);
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Adds a new employee to the opportunity sales team or activates the employee, if already on the sales team
        /// </summary>
        /// <remarks>       
        /// 1) if not (the new member already exists in the sales team) then
        /// add the new member to the sales team
        /// end if
        /// 2) define the role_id for the new member
        /// </remarks>
        /// <param name="employeeId">The employee to be added to the sales team</param>
        /// <param name="roleId">Role Id</param>
        /// <param name="opportunityId">the Opportunity, the employee should be added to</param>
        /// <param name="checkExistence">do a check to see if member already present</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateOpportunitySalesTeam(object employeeId, object roleId, object opportunityId, 
            bool checkExistence)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // Added by JWang 08/22/2005
                Recordset rstTeam = null;
                if (checkExistence)
                {
                    // When populate members from Contact Neighborhood Profile for the new quote, we do not need check
                    // the existence of the sales team member to be added.
                    // make sure the new employeeId hasn't already been added to the Quote's sales team - exit if it
                    // has
                    rstTeam = objLib.GetRecordset(modOpportunity.strqTEAM_MEMBER_EXISTS_FOR_OPPORTUNITY_EMPLOYEE, 2,
                        opportunityId, employeeId, modOpportunity.strf_OPPORTUNITY_TEAM_MEMBER_ID, modOpportunity.strfINACTIVE);
                    if (rstTeam.RecordCount > 0)
                    {
                        rstTeam.MoveFirst();
                        // If the member is Inactive, Reactivate it.
                        if (TypeConvert.ToDouble(rstTeam.Fields[modOpportunity.strfINACTIVE].Value) == -1)
                        {
                            rstTeam.Fields[modOpportunity.strfINACTIVE].Value = false;
                            objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY_TEAM_MEMBER, rstTeam);
                        }
                    }
                }

                rstTeam = objLib.GetNewRecordset(modOpportunity.strt_OPPORTUNITY_TEAM_MEMBER, modOpportunity.strf_EMPLOYEE_ID,
                    modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfROLE_ID);
                rstTeam.AddNew(Type.Missing, Type.Missing);
                rstTeam.Fields[modOpportunity.strf_EMPLOYEE_ID].Value = employeeId;
                rstTeam.Fields[modOpportunity.strfROLE_ID].Value = roleId;
                rstTeam.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = opportunityId;
                objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY_TEAM_MEMBER, rstTeam);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method copies the nbhd agreement's info to the opp agreements
        /// </summary>
        /// <param name="opportunityId">the OpportunityId</param>
        /// <param name="releaseId">the Neighbourhood id</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void CopyNeighborhoodAgreementToOpportunityAgreement(object opportunityId, object releaseId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                object vntNBHDId = RSysSystem.Tables[modOpportunity.strt_NBHD_PHASE].Fields[modOpportunity.strfNEIGHBORHOOD_ID].Index(releaseId);

                Recordset rstNBHDAgreement = objLib.GetRecordset(modOpportunity.strqNEIGHBORHOOD_AGREEMENTS_FOR_NEIGHBORHOOD,
                    1, vntNBHDId, modOpportunity.strfAGREEMENT_NAME, modOpportunity.strfDIVISION_AGREEMENT_ID, modOpportunity.strfDIVISION_ID,
                    modOpportunity.strfNEIGHBORHOOD_AGREEMENT_ID, modOpportunity.strfORDINAL);

                Recordset rstOppAgreement = objLib.GetNewRecordset(modOpportunity.strt_OPPORTUNITY_AGREEMENT, modOpportunity.strfAGREEMENT_NAME,
                    modOpportunity.strfDIVISION_AGREEMENT_ID, modOpportunity.strfNEIGHBORHOOD_AGREEMENT_ID, modOpportunity.strfORDINAL,
                    modOpportunity.strfMERGED_AGREEMENT, modOpportunity.strfOPPORTUNITY_ID);

                if (rstNBHDAgreement.RecordCount > 0)
                {
                    rstNBHDAgreement.MoveFirst();
                    while (!(rstNBHDAgreement.EOF))
                    {
                        rstOppAgreement.AddNew(Type.Missing, Type.Missing);
                        rstOppAgreement.Fields[modOpportunity.strfAGREEMENT_NAME].Value = (TypeConvert.ToString(rstNBHDAgreement.Fields[modOpportunity.strfAGREEMENT_NAME].Value)).Trim();
                        rstOppAgreement.Fields[modOpportunity.strfDIVISION_AGREEMENT_ID].Value = rstNBHDAgreement.Fields[modOpportunity.strfDIVISION_AGREEMENT_ID].Value;
                        rstOppAgreement.Fields[modOpportunity.strfAGREEMENT_NAME].Value = rstNBHDAgreement.Fields[modOpportunity.strfAGREEMENT_NAME].Value;
                        rstOppAgreement.Fields[modOpportunity.strfNEIGHBORHOOD_AGREEMENT_ID].Value = rstNBHDAgreement.Fields[modOpportunity.strfNEIGHBORHOOD_AGREEMENT_ID].Value;
                        rstOppAgreement.Fields[modOpportunity.strfORDINAL].Value = rstNBHDAgreement.Fields[modOpportunity.strfORDINAL].Value;
                        rstOppAgreement.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = opportunityId;
                        rstNBHDAgreement.MoveNext();
                    }
                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY_AGREEMENT, rstOppAgreement);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Called from SaveData. It determines the inventory quote record associated with the inventory lot
        /// and then calls the shared function that will copy the secondary product data
        /// </summary>
        /// <param name="quoteOpportunityId">the OpportunityId</param>
        /// <param name="lotId">Lot id</param>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void AddInventoryQuoteOptions(object quoteOpportunityId, object lotId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // Check to see if there are any records currently linked to the opp. If no records then
                // copy data
                if (quoteOpportunityId != DBNull.Value)
                {
                    // always delete all the Opportunity Products on this quote
                    DeleteOptions(quoteOpportunityId);

                    object vntPlan_Name_Id = RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[modOpportunity.strfPLAN_NAME_ID].Index(quoteOpportunityId);
                    Recordset rstOppOptions = objLib.GetRecordset(modOpportunity.strq_OPPORTUNITY_PRODUCT_WITH_OPPORTUNITY_ID, 1,
                        quoteOpportunityId, modOpportunity.strfOPPORTUNITY__PRODUCT_ID);
                    if (rstOppOptions.RecordCount == 0)
                    {
                        // Get the inventory quote associated with the lot and once we have that, get the options associated
                        // with it
                        // get all the options that are available for the plan on the Lot being quoted.
                        Recordset rstInv_Quote = objLib.GetRecordset(modOpportunity.strqINVENTORY_QUOTE_FOR_LOT, 2, lotId, quoteOpportunityId,
                            modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfELEVATION_ID,
                            modOpportunity.strfPLAN_BUILT, modOpportunity.strfBUILT_OPTIONS);
                        if (rstInv_Quote.RecordCount > 0)
                        {
                            if (RSysSystem.EqualIds(quoteOpportunityId, rstInv_Quote.Fields[modOpportunity.strfOPPORTUNITY_ID].Value))
                            {
                                // got to be the first one therefore add standard options
                                if ((vntPlan_Name_Id != DBNull.Value))
                                {
                                    CreateOpportunityProductStandard(RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[modOpportunity.strf_NBHD_PHASE_ID].Index(quoteOpportunityId),
                                        RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[modOpportunity.strfNEIGHBORHOOD_ID].Index(quoteOpportunityId),
                                        quoteOpportunityId, vntPlan_Name_Id);
                                }
                            }
                            else
                            {
                                // copy inventory quote data to customer quote
                                rstInv_Quote.MoveFirst();
                                object vntInvQuoteId = rstInv_Quote.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                                
                                // copy the opportunity product details from the inventory quote
                                CopyQuoteSecondary(vntInvQuoteId, quoteOpportunityId, modOpportunity.strt_OPPORTUNITY__PRODUCT,
                                    modOpportunity.strf_OPPORTUNITY_ID, modOpportunity.strf_OPPORTUNITY__PRODUCT_ID, false);
                                
                                // Fix Issue #58099
                                // Copy the opportunity adjustments details from the inventory quote
                                CopyQuoteSecondary(vntInvQuoteId, quoteOpportunityId, modOpportunity.strtOPPORTUNITY_ADJUSTMENT,
                                    modOpportunity.strf_OPPORTUNITY_ID, modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID);

                                // set primary fields
                                Recordset rstCust_Quote = objLib.GetRecordset(quoteOpportunityId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfPLAN_NAME_ID,
                                    modOpportunity.strfELEVATION_ID, modOpportunity.strfPLAN_BUILT, modOpportunity.strfBUILT_OPTIONS,
                                    modOpportunity.strfPRICE);
                                if (rstCust_Quote.RecordCount > 0)
                                {
                                    rstCust_Quote.MoveFirst();
                                    rstCust_Quote.Fields[modOpportunity.strfPLAN_NAME_ID].Value = rstInv_Quote.Fields[modOpportunity.strfPLAN_NAME_ID].Value;
                                    rstCust_Quote.Fields[modOpportunity.strfELEVATION_ID].Value = rstInv_Quote.Fields[modOpportunity.strfELEVATION_ID].Value;
                                    rstCust_Quote.Fields[modOpportunity.strfPLAN_BUILT].Value = rstInv_Quote.Fields[modOpportunity.strfPLAN_BUILT].Value;
                                    rstCust_Quote.Fields[modOpportunity.strfBUILT_OPTIONS].Value = rstInv_Quote.Fields[modOpportunity.strfBUILT_OPTIONS].Value;
                                    // set the plan price
                                    rstCust_Quote.Fields[modOpportunity.strfPRICE].Value = GetQuotePlanPrice(quoteOpportunityId, lotId, rstCust_Quote.Fields[modOpportunity.strfPLAN_NAME_ID].Value);
                                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstCust_Quote);
                                }
                            }
                        }
                        else
                        {
                            if ((vntPlan_Name_Id != DBNull.Value))
                            {
                                // got to be the first one therefore add standard options
                                CreateOpportunityProductStandard(RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[modOpportunity.strf_NBHD_PHASE_ID].Index(quoteOpportunityId),
                                    RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[modOpportunity.strfNEIGHBORHOOD_ID].Index(quoteOpportunityId),
                                    quoteOpportunityId, vntPlan_Name_Id);
                            }
                        }
                    }
                    CalculateTotals(quoteOpportunityId, false);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function gets Lots for a given Release that a Contract can be transfered to
        /// Inputs :
        /// releaseId - Release Id
        /// </summary>
        /// <returns>
        /// Recordset of Lots</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        protected virtual Recordset GetTransferableLotList(object releaseId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                return objLib.GetRecordset(modOpportunity.strq_AVAILABLE_LOTS_FOR_RELEASE_NO_CONSTR_STAGE, 2, releaseId,
                    releaseId, modOpportunity.strfPRODUCT_ID);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Check if the Plan is excluded on the current Lot
        /// </summary>
        /// <param name="lotId">The Lot Id</param>
        /// <param name="planNameId">The plan Id</param>
        /// <returns>
        /// True or False</returns>
        /// <history>
        /// Revision#    Date        Author   Description
        /// 3.8.0.0      5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        protected virtual bool CheckPlan(object lotId, object planNameId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // check if the Plan is excluded on the current Lot
                Recordset rstLotPlan = objLib.GetRecordset(modOpportunity.strq_EXCLUDED_PLANS_FOR_LOT, 2, lotId, planNameId,
                    modOpportunity.strfLOT_PLAN_ID);

                return (rstLotPlan.RecordCount > 0);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will update the Quote record when the Plan selection is changed.
        /// </summary>
        /// <param name="quoteOpportunityId">Quote opportunity Id </param>
        /// <param name="planNameId">Plan Name Id</param>
        /// <param name="releaseChange">indicates additional fields need to be cleared</param>
        /// <param name="neighborhoodPhaseId">Neighborhood Phase Id</param>
        /// <param name="lotId">Lot Id</param>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateQuoteOnPlanChange(object quoteOpportunityId, object planNameId, bool releaseChange, object
            neighborhoodPhaseId, object lotId)
        {
            this.UpdateQuoteOnPlanChange(quoteOpportunityId, planNameId, releaseChange, neighborhoodPhaseId, lotId, DBNull.Value);
        }

        /// <summary>
        /// This function will update the Quote record when the Plan selection is changed.
        /// </summary>
        /// <param name="quoteOpportunityId">Quote opportunity Id </param>
        /// <param name="planNameId">Plan Name Id</param>
        /// <param name="releaseChange">indicates additional fields need to be cleared</param>
        /// <param name="neighborhoodPhaseId">Neighborhood Phase Id</param>
        /// <param name="lotId">Lot Id</param>
        /// <param name="neighborhoodId">neighborhoodId  (Optional)</param>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateQuoteOnPlanChange(object quoteOpportunityId, object planNameId, bool releaseChange, object
            neighborhoodPhaseId, object lotId, object neighborhoodId)
        {
            Recordset rstOpp = null;
            try
            {

                if ((neighborhoodId == null) || (neighborhoodId == DBNull.Value))
                {
                    neighborhoodId = RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfNEIGHBORHOOD_ID].Index(quoteOpportunityId);
                }

                if (quoteOpportunityId != DBNull.Value)
                {
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    rstOpp = objLib.GetRecordset(quoteOpportunityId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfELEVATION_ID,
                        modOpportunity.strfELEVATION_PREMIUM, modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfPRICE,
                        modOpportunity.strfLOT_ID, modOpportunity.strfFINANCED_OPTIONS, modOpportunity.strfQUOTE_TOTAL,
                        modOpportunity.strfADJUSTMENT_TOTAL, modOpportunity.strf_NBHD_PHASE_ID, modOpportunity.strfCONFIGURATION_COMPLETE,
                        modOpportunity.strfPLAN_BUILT, modOpportunity.strfBUILT_OPTIONS, modOpportunity.strfNEIGHBORHOOD_ID);
                    if (rstOpp.RecordCount > 0)
                    {
                        rstOpp.MoveFirst();
                        // 2005/09/09 by JWang
                        // always set lot_id from parameter
                        rstOpp.Fields[modOpportunity.strfLOT_ID].Value = lotId;
                        // always clear elevation related fields
                        rstOpp.Fields[modOpportunity.strfELEVATION_ID].Value = DBNull.Value;
                        rstOpp.Fields[modOpportunity.strfELEVATION_PREMIUM].Value = 0;
                        // always delete all the Opportunity Products on this quote
                        DeleteOptions(quoteOpportunityId);

                        // 2005/09/09 by JWang
                        // clear out Configuration_Complete, Plan Built and Built_Options
                        rstOpp.Fields[modOpportunity.strfCONFIGURATION_COMPLETE].Value = DBNull.Value;
                        rstOpp.Fields[modOpportunity.strfPLAN_BUILT].Value = DBNull.Value;
                        rstOpp.Fields[modOpportunity.strfBUILT_OPTIONS].Value = DBNull.Value;
                        rstOpp.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value = neighborhoodId;

                        // if update is due to a relase change then also clear additional fields
                        if (releaseChange)
                        {
                            rstOpp.Fields[modOpportunity.strfLOT_ID].Value = DBNull.Value;
                            rstOpp.Fields[modOpportunity.strfFINANCED_OPTIONS].Value = 0;
                            rstOpp.Fields[modOpportunity.strfQUOTE_TOTAL].Value = 0;
                            rstOpp.Fields[modOpportunity.strfPRICE].Value = 0;
                            rstOpp.Fields[modOpportunity.strfADJUSTMENT_TOTAL].Value = 0;
                            planNameId = DBNull.Value;
                            rstOpp.Fields[modOpportunity.strf_NBHD_PHASE_ID].Value = neighborhoodPhaseId;
                        }
                        // if plan id supplied then add the standard options for this plan
                        if ((planNameId != DBNull.Value))
                        {
                            rstOpp.Fields[modOpportunity.strfPLAN_NAME_ID].Value = planNameId;
                            // set the plan price
                            rstOpp.Fields[modOpportunity.strfPRICE].Value = GetQuotePlanPrice(quoteOpportunityId, rstOpp.Fields[modOpportunity.strfLOT_ID].Value, planNameId);
                            // add the standard options for this plan
                            CreateOpportunityProductStandard(objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY,
                                modOpportunity.strf_NBHD_PHASE_ID, quoteOpportunityId), objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY,
                                modOpportunity.strfNEIGHBORHOOD_ID, quoteOpportunityId), quoteOpportunityId, planNameId);
                        }
                        else
                        {
                            // remove plan id
                            rstOpp.Fields[modOpportunity.strfPLAN_NAME_ID].Value = DBNull.Value;
                        }
                        // save changes
                        objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstOpp);
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will start the contract transfer process
        /// </summary>
        /// <param name="quoteOpportunityId">the Quote Id</param>
        /// <param name="lotId">the Lot Id</param>
        /// <param name="planNameId">the Plan Id</param>
        /// <param name="samePlan">boolean to identify Plan change</param>
        /// <param name="sameLot">boolean to identify Lot change</param>
        /// <returns>
        /// a variant containing the newly created quote</returns>
        /// <history>
        /// Revision#    Date        Author   Description
        /// 3.8.0.0      5/12/2006   DYin     Converted to .Net C# code.
        /// 5.9.0.0      Apr/12/2007 ML       Issue #65536-18916
        ///                                   The new Lot was not getting updated     
        /// 5.9.1        10/8/2010   KA       commented out ecoe and actual decision date          
        /// 5.9.2        11/02/2010  AM       Fixed issue where transfer Contract was not getting price from IQ
        /// 5.9.3        11/24/10    KA       calling CancelTransferContract instead of CancelContract so that correct note will be used
        /// </history>
        protected virtual object TransferContract(object quoteOpportunityId, object lotId, object planNameId, bool samePlan,
            bool sameLot)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                object vntNewQuoteId = DBNull.Value;
                object vntInvQuoteId = DBNull.Value;

                //Get Transferred From Contract so that we can get buyer information from it.
                Recordset rstTransContract = objLib.GetRecordset(quoteOpportunityId, modOpportunity.strtOPPORTUNITY,
                    modOpportunity.strfCONTACT_ID, modOpportunity.strfTIC_CO_BUYER_ID, modOpportunity.strfTIC_TRANSFER_FROM_LOT_ID);


                if (lotId != null)
                {
                    //Get the Inventory quote for the Lot, make a copy and set the Quote Contract record for the Lot.
                    Recordset rstInvQuote = objLib.GetRecordset(modOpportunity.strqHB_INVENTORY_QUOTE_FOR_INVENTORY_HOME, 1, lotId,
                                                    modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfINACTIVE, modOpportunity.strfPLAN_NAME_ID);
                    if (rstInvQuote.RecordCount > 0)
                    {
                        vntInvQuoteId = rstInvQuote.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;

                        if (vntInvQuoteId != DBNull.Value)
                        {
                            // Copy Quote
                            //AM2010.11.02 - set copyPlan to false so that it doesn't copy options over to new contract
                            vntNewQuoteId = CopyQuote(vntInvQuoteId, false, true, false, true);
                            if (vntNewQuoteId != DBNull.Value)
                            {
                                //Set buyer information on the Reservation
                                Recordset rstNewQuote = objLib.GetRecordset(vntNewQuoteId, modOpportunity.strt_OPPORTUNITY,
                                    modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfLOT_ID,
                                    modOpportunity.strf_PIPELINE_STAGE, modOpportunity.strf_STATUS, modOpportunity.strfCONTRACT_APPROVED_SUBMITTED,
                                    modOpportunity.strf_ACTUAL_DECISION_DATE, modOpportunity.strfECOE_DATE, modOpportunity.strfPRICE,
                                    modOpportunity.strfLOT_PREMIUM, modOpportunity.strfNBHD_PHASE_ID, modOpportunity.strf_CONTACT_ID,
                                    modOpportunity.strfENV_EDC_USERNAME, modOpportunity.strfTIC_CO_BUYER_ID, modOpportunity.strfTIC_TRANSFER_TO_LOT_ID, modOpportunity.strfRESERVATION_DATE,
                                    modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfTIC_TRANSFER_FROM_LOT_ID, modOpportunity.strfTIC_TRANSFER);


                                bool blnIncHomesitePremium = false;
                                decimal dblHomesitePremium = 0;
                                // Set the new Lot and New Plan on the new created quote
                                if (rstNewQuote.RecordCount > 0)
                                {
                                    //Add new unique EDC User Name
                                    //string oppId = RSysSystem.IdToString(rstNewQuote.Fields[modOpportunity.strfOPPORTUNITY_ID].Value);
                                    //oppId = string.Format(CultureInfo.CurrentCulture, "{0:X}", Convert.ToInt64(oppId, 16));
                                    //byte[] contactId = (byte[])rstNewQuote.Fields[modOpportunity.strfCONTACT_ID].Value;
                                    //string contactFirstName = (string)RSysSystem.Tables[modOpportunity.strt_CONTACT].Fields[modOpportunity.strfFIRST_NAME].Index(contactId);
                                    //string contactLastName = (string)RSysSystem.Tables[modOpportunity.strt_CONTACT].Fields[modOpportunity.strfLAST_NAME].Index(contactId);
                                    //string userId = string.Format("{0}{1}:{2}"
                                    //    , contactFirstName.Substring(0, 1)
                                    //    , contactLastName
                                    //    , oppId);
                                    //rstNewQuote.Fields[modOpportunity.strfENV_EDC_USERNAME].Value = userId.ToLower(CultureInfo.CurrentCulture);

                                    object vntDivision_Id = objLib.SqlIndex(modOpportunity.strt_NBHD_PHASE, modOpportunity.strfDIVISION_ID,
                                        rstNewQuote.Fields[modOpportunity.strfNBHD_PHASE_ID].Value);
                                    blnIncHomesitePremium = TypeConvert.ToBoolean(objLib.SqlIndex(modOpportunity.strtDIVISION,
                                        modOpportunity.strfINCLUDE_HOMESITE_PREMIUM, vntDivision_Id));
                                    if (sameLot != true)
                                    {
                                        rstNewQuote.Fields[modOpportunity.strfLOT_ID].Value = lotId;
                                        rstNewQuote.Fields[modOpportunity.strfLOT_PREMIUM].Value = objLib.SqlIndex(modOpportunity.strtPRODUCT,
                                            modOpportunity.strfPRICE, lotId);
                                    }
                                    if (blnIncHomesitePremium)
                                    {
                                        // Plan premium price includes the Homesite Premium
                                        rstNewQuote.Fields[modOpportunity.strfLOT_PREMIUM].Value = 0;
                                    }
                                    if (samePlan != true)
                                    {
                                        rstNewQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value = planNameId;
                                    }
                                    else
                                    {
                                        rstNewQuote.Fields[modOpportunity.strfPRICE].Value = GetQuotePlanPrice(vntNewQuoteId, lotId,
                                            rstNewQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value, out dblHomesitePremium);
                                    }
                                    //CMigles - 09/24/2010 - The tranfers should always go to Reservation as per Bruce's request.
                                    //rstNewQuote.Fields[modOpportunity.strf_PIPELINE_STAGE].Value = modOpportunity.strsCONTRACT;
                                    //rstNewQuote.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsIN_PROGRESS;

                                    rstNewQuote.Fields[modOpportunity.strf_PIPELINE_STAGE].Value = modOpportunity.strsQUOTE;
                                    rstNewQuote.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsRESERVED;
                                    rstNewQuote.Fields[modOpportunity.strfTIC_TRANSFER_TO_LOT_ID].Value = DBNull.Value;
                                    rstNewQuote.Fields[modOpportunity.strfRESERVATION_DATE].Value = DateTime.Today;
                                    rstNewQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value = rstInvQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value;

                                    //AM2010.11.02 - Set buyer infromation from transferred contract
                                    rstNewQuote.Fields[modOpportunity.strfCONTACT_ID].Value = rstTransContract.Fields[modOpportunity.strfCONTACT_ID].Value;
                                    rstNewQuote.Fields[modOpportunity.strfTIC_CO_BUYER_ID].Value = rstTransContract.Fields[modOpportunity.strfTIC_CO_BUYER_ID].Value;

                                    //AM2010.11.11 - Set the Transferred From Field and the TIC_Transferred flag to the new Reservation
                                    rstNewQuote.Fields[modOpportunity.strfTIC_TRANSFER_FROM_LOT_ID].Value
                                        = rstTransContract.Fields[modOpportunity.strfTIC_TRANSFER_FROM_LOT_ID].Value;
                                    rstNewQuote.Fields[modOpportunity.strfTIC_TRANSFER].Value = true;

                                    // Jul 29, 2005. By JWang. define Sale Request Date, Sale Date, and ECOE Date as current
                                    // date.
                                    rstNewQuote.Fields[modOpportunity.strfCONTRACT_APPROVED_SUBMITTED].Value = DateTime.Today;
                                    //KA 10-8-10 commented out, on transfer have them refill in the dates
                                    //rstNewQuote.Fields[modOpportunity.strf_ACTUAL_DECISION_DATE].Value = DateTime.Today;
                                    //rstNewQuote.Fields[modOpportunity.strfECOE_DATE].Value = DateTime.Today;

                                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstNewQuote);

                                    if (samePlan != true)
                                    {
                                        // Fix Issue #65536-15073 - Set the New Lot Status to Sold
                                        objLib.PermissionIgnored = true;
                                        Recordset rstNewLot = objLib.GetRecordset(lotId, modOpportunity.strt_PRODUCT, modOpportunity.strfLOT_PRODUCT_ID,
                                            modOpportunity.strfLOT_STATUS);
                                        rstNewLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsSOLD;
                                        objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstNewLot);

                                        // add standard options from the current plan on the current
                                        UpdateQuoteOnPlanChange(vntNewQuoteId, planNameId, false, DBNull.Value,
                                            rstNewQuote.Fields[modOpportunity.strfLOT_ID].Value, DBNull.Value);
                                        CalculateTotals(vntNewQuoteId, false);
                                    }
                                    //ML - Apr 12 2007 - the new lot should get updated
                                    //issue #65536-18916
                                    if (!sameLot)
                                    {
                                        Recordset rstNewLot = objLib.GetRecordset(lotId, modOpportunity.strt_PRODUCT, modOpportunity.strfLOT_STATUS,
                                           modOpportunity.strfOWNER_ID, modOpportunity.strfSALES_DATE, modOpportunity.strfRESERVATION_CONTRACT_ID,
                                            modOpportunity.strfEST_CONTRACT_CLOSED_DATE, modOpportunity.strfTIC_CO_BUYER_ID, modOpportunity.strfRESERVED_DATE);
                                        objLib.PermissionIgnored = true;
                                        if (rstNewLot.RecordCount > 0)
                                        {
                                            //CMigles - 09/24/2010 - The tranfers should always go to Reservation as per Bruce's request.
                                            //rstNewLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsSOLD;
                                            rstNewLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsRESERVED;
                                            rstNewLot.Fields[modOpportunity.strfOWNER_ID].Value = rstNewQuote.Fields[modOpportunity.strf_CONTACT_ID].Value;

                                            //AM2010.10.13 - When a contract is transfered the new lot sales date should be
                                            //null, since this would cause the scheduled script to update the lot status = sold
                                            //rstNewLot.Fields[modOpportunity.strfSALES_DATE].Value = DateTime.Today;
                                            rstNewLot.Fields[modOpportunity.strfRESERVED_DATE].Value = DateTime.Today;

                                            //CMigles - 09/24/2010 - Set Cobuyer Id
                                            rstNewLot.Fields[modOpportunity.strfTIC_CO_BUYER_ID].Value = rstNewQuote.Fields[modOpportunity.strfTIC_CO_BUYER_ID].Value;
                                            rstNewLot.Fields[modOpportunity.strfRESERVATION_CONTRACT_ID].Value = vntNewQuoteId;
                                            // since its same as that on the new opportunity just created
                                            rstNewLot.Fields[modOpportunity.strfEST_CONTRACT_CLOSED_DATE].Value = DateTime.Today;

                                            objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstNewLot);
                                        }
                                    }
                                }
                            }

                            // Cancel current contract
                            //ka 11-24-10 used to call cancel contract method, changed so that cancel note will reflect transfer
                            CancelTransferContract(quoteOpportunityId, sameLot);
                        }

                    }
                }


                return vntNewQuoteId;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function gets all available Plans for a Lot
        /// Inputs :
        /// releaseId - Release Id
        /// quoteOpportunityId - the Quote Id
        /// neighborhoodId - neighborhood Id
        /// vntLotId - the current lot id
        /// </summary>
        /// <returns>Recordset containing available plans</returns>
        /// <history>
        /// Recordset of Plans
        /// Revision#  Date         Author  Description
        /// 3.8.0.0    5/12/2006    DYin    Converted to .Net C# code.
        /// 5.9.0.0    Apr/10/2007  ML      Issue #65536-18672 changed the query 
        ///                                 to give most specific plans.
        ///                                 added one more parameter for Lot Id
        /// </history>
        protected virtual Recordset GetAvailablePlans(object releaseId, object quoteOpportunityId, object neighborhoodId, object vntLotId)
        {
            try
            {
                DataAccess objLibr = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOpportunity = objLibr.GetRecordset(quoteOpportunityId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfPLAN_NAME_ID,modOpportunity.strfLOT_ID);
                object vntDivisionProductId = RSysSystem.Tables[modOpportunity.strt_NBHDP_PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value);
                return objLibr.GetRecordset(modOpportunity.strqMOST_SPECIFIC_PLANS_EXCLUDING_CURRENT_DIV_PROD, 16, releaseId, neighborhoodId,
                    neighborhoodId, releaseId, neighborhoodId, vntLotId, neighborhoodId, neighborhoodId, vntLotId, neighborhoodId, neighborhoodId, vntLotId, neighborhoodId, vntLotId, vntLotId, vntDivisionProductId,modOpportunity.strf_NBHDP_PRODUCT_ID);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Updates price related fields for the passed id, dependent on quereied criteria
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#  Date        Author     Description
        /// 3.8.0.0    5/12/2006   DYin       Converted to .Net C# code.
        /// 5.9.0.0    13/02/2007  ML         Changes for post cut-off price
        /// </history>
        public virtual bool SetOptionPricing(object opportunityProductId)
        {
            try
            {
                // default to failure
                bool blnUpdate = false;

                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                if (opportunityProductId == DBNull.Value)
                {
                    return false;
                }
                // get quote option
                Recordset rstQuoteOption = objLib.GetRecordset(opportunityProductId, modOpportunity.strtOPPORTUNITY__PRODUCT,
                    modOpportunity.strfBUILT_OPTION, modOpportunity.strfPRICE, modOpportunity.strfNBHDP_PRODUCT_ID,
                    modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfNET_CONFIG, modOpportunity.strfSELECTED,modOpportunity.strfUSE_POST_CUTOFF_PRICE);
                if (rstQuoteOption.RecordCount > 0)
                {
                    rstQuoteOption.MoveFirst();
                    // get criteria
                    object vntQuoteId = rstQuoteOption.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                    string vntPipeline_Stage = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strf_PIPELINE_STAGE,
                        vntQuoteId));
                    object neighborhoodPhaseProductId = rstQuoteOption.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value;
                    object vntQuote_Status = objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfSTATUS,
                        vntQuoteId);
                    //ML - 19 july 07 - we should consider the Division to which that quote belongs
                    //object vntDivisionId = objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfDIVISION_ID,
                    //    neighborhoodPhaseProductId);
                    object vntNBHD_Phase_Id = objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfNBHD_PHASE_ID,
                        vntQuoteId);
                    object vntDivisionId = objLib.SqlIndex(modOpportunity.strt_NBHD_PHASE, modOpportunity.strfDIVISION_ID,
                        vntNBHD_Phase_Id);
                    object vntBuildOption = TypeConvert.ToInt32(objLib.SqlIndex(modOpportunity.strt_DIVISION, modOpportunity.strfBUILD_OPTION_PRICING,
                        vntDivisionId));
                    //ML feb 2007 - to decide whether to update with Current Price or the Post Cut-Off Price
                    bool blnUsePostCutOff = TypeConvert.ToBoolean(rstQuoteOption.Fields[modOpportunity.strfUSE_POST_CUTOFF_PRICE].Value);
                    // first update criteria is the pipeline statge
                    DateTime vntContractApprovedSubmitted = TypeConvert.ToDateTime(DBNull.Value);

                    if (vntPipeline_Stage == modOpportunity.strPIPELINE_QUOTE)
                    {
                        // get additional criteria
                        object vntLot_Id = objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfLOT_ID,
                            vntQuoteId);
                        object vntLot_Type = objLib.SqlIndex(modOpportunity.strt_PRODUCT, modOpportunity.strfTYPE,
                            vntLot_Id);
                        // two situations
                        if (TypeConvert.ToString(vntQuote_Status) != modOpportunity.strQUOTE_STATUS_INVENTORY && (Convert.IsDBNull(vntLot_Id)
                            || TypeConvert.ToString(vntLot_Type) != modOpportunity.strLOT_TYPE_INVENTORY))
                        {
                            // always update
                            blnUpdate = true;
                        }
                        else if( TypeConvert.ToString(vntLot_Type) == modOpportunity.strLOT_TYPE_INVENTORY && (TypeConvert.ToString(vntQuote_Status)
                            == modOpportunity.strQUOTE_STATUS_INVENTORY || TypeConvert.ToString(vntQuote_Status) ==
                            modOpportunity.strQUOTE_STATUS_IN_PROGRESS || TypeConvert.ToString(vntQuote_Status) == modOpportunity.strQUOTE_STATUS_RESERVED))
                        {
                            // depends on division settings
                            if (vntBuildOption.Equals(modOpportunity.intBUILD_OPTION_FIXED))
                            {
                                // fixed, only update if not built
                                if (!TypeConvert.ToBoolean(rstQuoteOption.Fields[modOpportunity.strfBUILT_OPTION].Value))
                                {
                                    blnUpdate = true;
                                }
                            }
                            else if( vntBuildOption.Equals(modOpportunity.intBUILD_OPTION_FLOATING))
                            {
                                // floating, always update
                                blnUpdate = true;
                            }
                        }

                    }
                    else if (vntPipeline_Stage == modOpportunity.strPIPELINE_SALES_REQUEST || vntPipeline_Stage == modOpportunity.strPIPELINE_POST_SALE)
                    {
                        // get additional criteria
                        vntContractApprovedSubmitted = TypeConvert.ToDateTime(objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY,
                            modOpportunity.strfCONTRACT_APPROVED_SUBMITTED, vntQuoteId));
                        // two situations
                        if ((TypeConvert.ToBoolean(rstQuoteOption.Fields[modOpportunity.strfSELECTED].Value) || (TypeConvert.ToDateTime(rstQuoteOption.Fields[modOpportunity.strfRN_CREATE_DATE].Value)
                            <= vntContractApprovedSubmitted)))
                        {
                            // do not update
                        }
                        else if(TypeConvert.ToDateTime(rstQuoteOption.Fields[modOpportunity.strfRN_CREATE_DATE].Value) > vntContractApprovedSubmitted)
                        {
                            // depends on division settings
                            //ML - 19 july 07 - we should consider the Division to which that quote belongs
                            //vntDivisionId = objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfDIVISION_ID,
                            //    neighborhoodPhaseProductId);
                            object vntStndOption = TypeConvert.ToInt32(objLib.SqlIndex(modOpportunity.strt_DIVISION, modOpportunity.strfSTANDARD_OPTION_PRICING,
                                vntDivisionId));
                            if (modOpportunity.intSTANDARD_OPTION_FIXED.Equals(vntStndOption))
                            {
                                // fixed, do not update
                            }
                            else if( modOpportunity.intSTANDARD_OPTION_FLOATING.Equals(vntStndOption))
                            {
                                // floating, always update
                                blnUpdate = true;
                            }
                        }

                    }
                    else if (vntPipeline_Stage == modOpportunity.strPIPELINE_POST_BUILD_QUOTE)
                    {
                        if (TypeConvert.ToString(vntQuote_Status) == modOpportunity.strsIN_PROGRESS)
                        {
                            // depends on division settings
                            if (vntBuildOption.Equals(modOpportunity.intBUILD_OPTION_FIXED))
                            {
                                // fixed, only update if not built
                                blnUpdate =(! TypeConvert.ToBoolean(rstQuoteOption.Fields[modOpportunity.strfBUILT_OPTION].Value));
                            }
                            else if( vntBuildOption.Equals(modOpportunity.intBUILD_OPTION_FLOATING))
                            {
                                // floating, always update
                                blnUpdate = true;
                            }
                        }

                    }
                    else if (vntPipeline_Stage == modOpportunity.strPIPELINE_CANCELED || vntPipeline_Stage == modOpportunity.strPIPELINE_CLOSED)
                    {
                        // do not update
                    }

                    if (blnUpdate)
                    {
                        // figure out what the next price is (if there is any) and proceed with the update
                        //ML - feb 2007 - check if post cut-off has to be considered
                        rstQuoteOption.Fields[modOpportunity.strfPRICE].Value = GetOptionNextPrice(neighborhoodPhaseProductId, blnUsePostCutOff);                        
                        // mark Quote as Updated
                        SetQuotePriceUpdate(vntQuoteId, true);
                        // save changes
                        objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstQuoteOption);
                        // re-calculate the Quote Totals.
                        CalculateTotals(vntQuoteId, false);
                    }

                    rstQuoteOption.Close();
                    // mark successful finish
                    return true;
                }
                return false;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method gets the next post cut-off price for an option
        /// </summary>
        /// <returns>dblNewPostCutOffPrice - the next post cut-off price</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 5.9.0.0        2/09/2007      ML        Initial version
        /// 5.9.0.0        03/15/2007     BC        Sorting order for date is set to ascending
        /// </history>
        protected virtual decimal GetOptionNextPrice(object neighborhoodPhaseProductId, bool blnUsePCO)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                PriceChangeHistory objPCH = (PriceChangeHistory)RSysSystem.ServerScripts[modOpportunity.strsPRICE_CHANGE_HISTORY].CreateInstance();
                // get list of all price change history records for the given NBHDP_Product_Id
                objLib.SortFieldName = modOpportunity.strfCHANGE_DATETIME;
                objLib.SortAscending = true;
                Recordset rstPriceHistory = objLib.GetRecordset(modOpportunity.strqVALID_PRICES_FOR_NBHDP_PRODUCTS, 1, neighborhoodPhaseProductId,
                    modOpportunity.strfPRICE, modOpportunity.strfCHANGE_DATE, modOpportunity.strfPROCESSED, modOpportunity.strfSTANDARD,
                    modOpportunity.strfCHANGE_DATETIME, modOpportunity.strfPCH_COST_PRICE, modOpportunity.strfPCH_MARGIN, modOpportunity.strfPCH_POST_CUT_OFF_PRICE);
                DateTime vntNextDate = TypeConvert.ToDateTime(DBNull.Value);
                // get list of all price change history records for the given NBHDP_Product_Id
                decimal dblNextPrice = 0;
                decimal dblNewPrice = 0;
                DateTime vntNewDate = TypeConvert.ToDateTime(DBNull.Value);
                bool blnStandard = false;
                decimal dblNextCostPrice = 0;
                decimal dblNewCostPrice = 0;
                decimal dblNextMargin = 0;
                decimal dblNewMargin = 0;
                decimal dblNextPostCutOffPrice = 0;
                decimal dblNewPostCutOffPrice = 0;
                DateTime dteNextStandardUpdate = TypeConvert.ToDateTime(DBNull.Value);
                objPCH.ProcessPriceHistory(rstPriceHistory, out vntNextDate, out dblNextPrice, out dblNewPrice, out blnStandard,
                    out dteNextStandardUpdate, out vntNewDate, out dblNextCostPrice, out dblNextMargin, out dblNextPostCutOffPrice, out dblNewCostPrice, out dblNewMargin, out dblNewPostCutOffPrice);
                if (blnUsePCO == false)
                {
                    return dblNewPrice; //, vntNewDate};
                }
                else
                {
                    return dblNewPostCutOffPrice; //, vntNewDate};
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method gets the next price for an option
        /// </summary>
        /// <returns>dblPrice - the next price</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual decimal GetOptionNextPrice(object neighborhoodPhaseProductId)
        {
            try
            {
                return GetOptionNextPrice(neighborhoodPhaseProductId, false);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method gets the fixed price for an option
        /// </summary>
        /// <param name="neighborhoodPhaseProductId">Neighborhood Product Id</param>
        /// <param name="optionPriceDate">the fixed price date</param>
        /// <returns>The fixed proce for the option.</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual decimal GetOptionFixedPrice(object neighborhoodPhaseProductId, DateTime optionPriceDate)
        {
            try
            {
                return GetOptionFixedPrice(neighborhoodPhaseProductId, optionPriceDate, false);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method gets the fixed price for an option
        /// </summary>
        /// <param name="neighborhoodPhaseProductId">Neighborhood Product Id</param>
        /// <param name="optionPriceDate">the fixed price date</param>
        /// <param name="blnUsePCO">boolean field to check the PCO</param>
        /// <returns>the Fixed Price including the PCO</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 5.9.0.0        2/26/2007      BC        PCO Price Validation
        /// </history>
        protected virtual decimal GetOptionFixedPrice(object neighborhoodPhaseProductId, DateTime optionPriceDate, bool blnUsePCO)
        {
            try
            {
                object[] vntPriceReturn = new object[0];
                if (Convert.IsDBNull(optionPriceDate) || (optionPriceDate == null))
                {
                    return GetOptionNextPrice(neighborhoodPhaseProductId, blnUsePCO);
                }

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.SortAscending = true;
                objLib.SortFieldName = modOpportunity.strfCHANGE_DATETIME;
                Recordset rstPriceHistory = objLib.GetRecordset(modOpportunity.strqPRICE_FOR_SQ_CHANGE_DATE_TIME, 4, neighborhoodPhaseProductId,
                    optionPriceDate, optionPriceDate, optionPriceDate, modOpportunity.strfPRICE, modOpportunity.strfPCH_POST_CUT_OFF_PRICE);
                if (rstPriceHistory.RecordCount > 0)
                {
                    rstPriceHistory.MoveLast();
                    // the latest date
                    if (blnUsePCO == false)
                        return TypeConvert.ToDecimal(rstPriceHistory.Fields[modOpportunity.strfPRICE].Value);
                    else
                        return TypeConvert.ToDecimal(rstPriceHistory.Fields[modOpportunity.strfPCH_POST_CUT_OFF_PRICE].Value);
                }
                else
                {
                    // did not find any that matches the sales request date, who knows what the price is?
                    return GetOptionNextPrice(neighborhoodPhaseProductId, blnUsePCO);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will check if the option is already buit
        /// </summary>
        /// <param name="opportunityProductId">Opportunity Product Id</param>
        /// <param name="lotStageOrdinal">Lot Stage Ordinal</param>
        /// <returns>True or False</returns>
        /// <history>
        /// Revision#  Date        Author     Description
        /// 3.8.0.0    5/12/2006   DYin       Converted to .Net C# code.
        /// 5.9.0.0    mar/12/2007 ML         changes to consider package components
        /// </history>
        protected virtual bool OptionAmIBuilt(object opportunityProductId, int lotStageOrdinal)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                bool optionAmIBuilt = true;
                // get the option
                Recordset rstOptions = objLib.GetRecordset(opportunityProductId, modOpportunity.strt_OPPORTUNITY__PRODUCT,
                    modOpportunity.strfBUILT_OPTION, modOpportunity.strfNBHDP_PRODUCT_ID, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                    modOpportunity.strfPRODUCT_AVAILABLE, modOpportunity.strfSELECTED,modOpportunity.strfPARENT_PACK_OPPPROD_ID);
                if (rstOptions.RecordCount > 0)
                {
                    rstOptions.MoveFirst();
                    // compare the lot construction stage ordinal to the options (parent) construction ordinal
                    object vntNBHDProduct_Id = rstOptions.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value;
                    //ML- mar-12-2007 - for package components, we need to consider parent package's Construction Stage
                    if (rstOptions.Fields[modOpportunity.strfPARENT_PACK_OPPPROD_ID].Value != DBNull.Value)
                    {
                        object vntPackageOppProdID = rstOptions.Fields[modOpportunity.strfPARENT_PACK_OPPPROD_ID].Value;
                        vntNBHDProduct_Id = RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY__PRODUCT].Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Index(vntPackageOppProdID);
                    }
                    int intDivStageOrdinal = 0;
                    if ((vntNBHDProduct_Id != DBNull.Value))
                    {
                        object vntDivisionProductId = RSysSystem.Tables[modOpportunity.strt_NBHD_PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(vntNBHDProduct_Id);
                        if ((vntDivisionProductId != DBNull.Value))
                        {
                            object vntDivConsStageId = RSysSystem.Tables[modOpportunity.strtDIVISION_PRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntDivisionProductId);
                            if ((vntDivConsStageId == DBNull.Value))
                                return false;
                            intDivStageOrdinal = TypeConvert.ToInt32(RSysSystem.Tables[modOpportunity.strtCONSTRUCTION_STAGE].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Index(vntDivConsStageId));
                        }
                    }

                    string vntConstructionStageComparison = GetConstructionStageComparison();

                    optionAmIBuilt = (vntConstructionStageComparison == modOpportunity.strsGREATER_THAN) &&
                        lotStageOrdinal > TypeConvert.ToInt32(intDivStageOrdinal) || (vntConstructionStageComparison 
                        == modOpportunity.strsGREATER_THAN_OR_EQUAL_TO) && (lotStageOrdinal >= intDivStageOrdinal);
                }
                return optionAmIBuilt;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Encapsulates logis for setting and unsetting an options built flag.  If an Elevation Option got built, 
        /// then set the Lot's Built Elevation.
        /// </summary>
        /// <param name="optionRecordset">Option recordset</param>
        /// <param name="built">new value for built flag</param>
        /// <param name="lotId">Lot Id</param>
        /// <returns>The blnBuilt flag back unchanged</returns>
        /// <history>
        /// Revision#  Date        Author     Description
        /// 3.8.0.0    5/12/2006   DYin       Converted to .Net C# code.
        /// </history>
        protected virtual bool OptionBuildMe(Recordset optionRecordset, bool built, object lotId)
        {
            if (built)
            {
                // set the build option to true
                optionRecordset.Fields[modOpportunity.strfBUILD_OPTION].Value = true;
                // and option will always be no longer be available
                optionRecordset.Fields[modOpportunity.strfPRODUCT_AVAILABLE].Value = false;

                // Jun 23, 2005. By JWang
                // If this is Elevation Option got built, then set the Lot's Built Elevation.
                if (TypeConvert.ToString(optionRecordset.Fields[modOpportunity.strfTYPE].Value) == modOpportunity.strsELEVATION)
                {
                    UpdateHomesiteBuiltElevation(lotId, optionRecordset.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value);
                }
            }
            else
            {
                // set the build option to false
                optionRecordset.Fields[modOpportunity.strfBUILD_OPTION].Value = false;
                // and option will now be available
                optionRecordset.Fields[modOpportunity.strfPRODUCT_AVAILABLE].Value = true;

                // Jun 23, 2005. By JWang
                // If this is Elevation Option, and option built was cleared , then clear the Lot's Built Elevation.
                if (TypeConvert.ToString(optionRecordset.Fields[modOpportunity.strfTYPE].Value) == modOpportunity.strsELEVATION)
                {
                    UpdateHomesiteBuiltElevation(lotId, DBNull.Value);
                }
            }
            return built;
        }

        // HomesiteConstructionOrdinalPastPlanOne
        /// <summary>
        /// Check if Homesite Construction Ordinal past Plan Construction Ordinal.
        /// </summary>
        /// <param name="lotStageOrdinal">Lot stage ordinal</param>
        /// <param name="planId">PlanId of a quote or contract</param>
        /// <returns>True -- Homesite Construction Ordinal past Plan Construction Ordinal
        /// False -- Not past</returns>
        /// <history>
        /// Revision#      Date            Author          Description
        /// 3.8.0.0        5/12/2006       DYin            Converted to .Net C# code.
        /// 5.9.0.0        Apr/6/2007      ML              Issue#65536-18817
        /// </history>
        protected virtual bool HomesiteConstructionOrdinalPastPlanOne(int lotStageOrdinal, object planId)
        {
            try
            {
                if (planId == DBNull.Value)
                {
                    return false;
                }
                else
                {
                    string vntConstructionStageComparison = GetConstructionStageComparison();

                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    object vntDivisionProductId = objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfDIVISION_PRODUCT_ID, planId);
                    object vntDivConsStageId = objLib.SqlIndex(modOpportunity.strtDIVISION_PRODUCT, modOpportunity.strfCONSTRUCTION_STAGE_ID, vntDivisionProductId);
                    int intDivStageOrdinal = TypeConvert.ToInt32(objLib.SqlIndex(modOpportunity.strtCONSTRUCTION_STAGE, modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL, vntDivConsStageId));
                    int intPlanConstructionOrdinal = intDivStageOrdinal == 0 ? -1000 : intDivStageOrdinal;
                    //ML - Apr/6/2007 return false if plans constructionstage is not set
                    //Issue#65536-18817
                    if (intPlanConstructionOrdinal == -1000)
                        return false;
                    else
                    return (((vntConstructionStageComparison == modOpportunity.strsGREATER_THAN) && (lotStageOrdinal
                        > intPlanConstructionOrdinal)) || ((vntConstructionStageComparison == modOpportunity.strsGREATER_THAN_OR_EQUAL_TO)
                        && (lotStageOrdinal >= intPlanConstructionOrdinal)));
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// When the Construction Stage on the lot ia changed, update the Construction Stage on all
        /// quotes and update the built flag on all options on all quotes belonging to the lot.
        /// This function also will check if the Lot's construction Stage Ordinal past Plan's one, so
        /// the name is not proper anymore.
        /// </summary>
        /// <param name="lotId">Lot Id</param>
        /// <param name="constructionStageId">Construction Stage Id</param>
        /// <returns>True if its successful in updating everything False otherwise</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 5.9.0.0     28/Feb/2007    ML      Initial Version.
        /// </history>
        public virtual bool UpdateQuoteOptions(object lotId, object constructionStageId)
        {
            bool Return = this.UpdateQuoteOptions(lotId, constructionStageId, true);
            return Return;
        }
        /// <summary>
        /// When the Construction Stage on the lot ia changed, update the Construction Stage on all
        /// quotes and update the built flag on all options on all quotes belonging to the lot.
        /// This function also will check if the Lot's construction Stage Ordinal past Plan's one, so
        /// the name is not proper anymore.
        /// </summary>
        /// <param name="lotId">Lot Id</param>
        /// <param name="constructionStageId">Construction Stage Id</param>
        /// <param name="blnUpdateAll">If = TRUE, Update all Options, else update which are not using the Post CutOff Price</param>
        /// <returns>True if its successful in updating everything False otherwise</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 3.8.0.0  5/12/2006  DYin  Converted to .Net C# code.
        /// 5.9.0.0     28/Feb/2007   ML       Updated for the Post Cut Off Requirement.
        /// 5.9.0.0      10/Apr/2007  ML       Issue#65536-18822
        /// </history>
        public virtual bool UpdateQuoteOptions(object lotId, object constructionStageId, bool blnUpdateAll)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                LotConfiguration objLotConfig = (LotConfiguration)RSysSystem.ServerScripts[modOpportunity.strsLOT_CONFIG].CreateInstance();
                string strQueryName = string.Empty;
                

                if (lotId == DBNull.Value) return false;

                // set ordinal of current construction stage on the lot
                int intLotStageOrdinal = TypeConvert.ToInt32((objLib.SqlIndex(modOpportunity.strtCONSTRUCTION_STAGE, modOpportunity.strfCONSTRUCTION_STAGE_ORD, constructionStageId)));

                string strInvQuoteOptionsBuilt = string.Empty;
                if (TypeConvert.ToString(objLib.SqlIndex( modOpportunity.strtPRODUCT, modOpportunity.strfTYPE,
                    lotId)) == modOpportunity.strsINVENTORY)
                {
                    
                    // get the inventory quote for lot, there should only be one
                    Recordset rstInvQuote = objLib.GetRecordset(modOpportunity.strqACTIVE_INVENTORY_QUOTES_FOR_LOT, 1, lotId,
                        modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfPLAN_BUILT, modOpportunity.strfBUILT_OPTIONS,
                        modOpportunity.strfPLAN_NAME_ID);
                    if (rstInvQuote.RecordCount > 0)
                    {
                        StringBuilder inventoryQuoteOptionsBuilder = new StringBuilder();
                        rstInvQuote.MoveFirst();
                        // Jun 26, 2005. By JWangIf the homesite construction stage passes Plan's construction stage
                        // then
                        // set quotes's Plan Built flag, and populate homesite's Built Plan
                        if (HomesiteConstructionOrdinalPastPlanOne(intLotStageOrdinal, rstInvQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value))
                        {
                            rstInvQuote.Fields[modOpportunity.strfPLAN_BUILT].Value = true;
                            UpdateHomesitePlan(lotId, rstInvQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value);
                        }

                        bool blnBuildOptionSet = false;
                        object vntQuoteId = rstInvQuote.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                        
                        //ML - Setting up the Query name depending upon the option chosen by the end user.
                        //ML - 27/06/07 changed the query to consider only selected ones 
                        //  Issue#65536-19627
                        strQueryName = modOpportunity.strqSELECTED_OPP_PROD_WITH_OPP_WITHOUT_PCO_PRICE_OPTIONS;
                        if (blnUpdateAll)
                            strQueryName = modOpportunity.strqSELECTEDOPP_PRODUCTS_WITH_OPP_ID;
                            
                        
                        // update inventory quote options (retain list of options marked as built)
                        Recordset rstOptions = objLib.GetRecordset(strQueryName, 1, vntQuoteId,
                            modOpportunity.strfBUILD_OPTION, modOpportunity.strfNBHDP_PRODUCT_ID, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                            modOpportunity.strfPRODUCT_AVAILABLE, modOpportunity.strfTYPE);
                        if (rstOptions.RecordCount > 0)
                        {
                            rstOptions.MoveFirst();
                            while(!(rstOptions.EOF))
                            {
                                // check to see if option is now built
                                object vntOppProductId = rstOptions.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value;
                                if (OptionBuildMe(rstOptions, OptionAmIBuilt(rstOptions.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                                    intLotStageOrdinal), lotId))
                                {
                                    blnBuildOptionSet = true;

                                    // Create Homesite Configuration - fpoulsen 06/27/2005
                                    // object vntDivisionProductId = rstOptions.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value;
                                    objLotConfig.CreateLotConfiguration(vntOppProductId, lotId);

                                    // remember option via RO
                                    inventoryQuoteOptionsBuilder.Append(RSysSystem.IdToString
                                        (rstOptions.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value) + ";");
                                }
                                else
                                    // If option was unbuild ensure the lot config is also updated
                                    objLotConfig.CreateLotConfiguration(vntOppProductId, lotId, true);

                                rstOptions.MoveNext();
                            }
                            objLib.PermissionIgnored = true;
                            objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstOptions);
                            strInvQuoteOptionsBuilt = inventoryQuoteOptionsBuilder.ToString();

                        }

                        // update inventory quote
                        rstInvQuote.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value = constructionStageId;
                        if (blnBuildOptionSet)
                        {
                            // Option(s) were built, need to set Plan Built and OPTIONS
                            rstInvQuote.Fields[modOpportunity.strfPLAN_BUILT].Value = true;
                            rstInvQuote.Fields[modOpportunity.strfBUILT_OPTIONS].Value = true;
                            // also update lot
                            UpdateHomesitePlan(lotId, rstInvQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value);
                        }

                        objLib.PermissionIgnored = true;
                        objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstInvQuote);
                    }
                    // get customer quotes for lot
                    Recordset rstQuote = objLib.GetRecordset(modOpportunity.strqACTIVE_CUSTOMER_QUOTES_FOR_LOT, 1, lotId, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                        modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfPLAN_BUILT, modOpportunity.strfBUILT_OPTIONS,
                        modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfCONFIGURATION_CHANGED, modOpportunity.strfDESCRIPTION,
                        modOpportunity.strfSTATUS, modOpportunity.strfINACTIVE);
                    if (rstQuote.RecordCount > 0)
                    {
                        rstQuote.MoveFirst();
                        // Jun 26, 2005. By JWang If the homesite construction stage passes Plan's construction stage
                        // then
                        // set quotes's Plan Built flag, and populate homesite's Built Plan
                        if (HomesiteConstructionOrdinalPastPlanOne(intLotStageOrdinal, rstQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value))
                        {
                            rstQuote.Fields[modOpportunity.strfPLAN_BUILT].Value = true;
                            UpdateHomesitePlan(lotId, rstQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value);
                        }

                        while(!(rstQuote.EOF))
                        {
                            bool blnBuildOptionSet = false;
                            string strInvQuoteOptionsBuilt1 = strInvQuoteOptionsBuilt;
                            string strInvalidQuoteOptions = "";
                            object vntQuoteId = rstQuote.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;

                            //ML - Setting up the Query according to the blnUpdateAll flag used for options using
                            //     Post CutOff Prices.
                            strQueryName = modOpportunity.strqSELECTEDOPP_PRODUCTS_WITH_OPP_ID;
                            if (!blnUpdateAll)
                                strQueryName = modOpportunity.strqSELECTED_OPP_PROD_WITH_OPP_WITHOUT_PCO_PRICE_OPTIONS;
                            
                            // get all the Options for this Quote
                            Recordset rstOptions = objLib.GetRecordset(strQueryName, 1,
                                vntQuoteId, modOpportunity.strfBUILD_OPTION, modOpportunity.strfNBHDP_PRODUCT_ID, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                                modOpportunity.strfPRODUCT_AVAILABLE, modOpportunity.strfTYPE);
                            if (rstOptions.RecordCount > 0)
                            {
                                rstOptions.MoveFirst();
                                while(!(rstOptions.EOF))
                                {
                                    // check to see if option is now built
                                    if (OptionBuildMe(rstOptions, OptionAmIBuilt(rstOptions.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                                        intLotStageOrdinal), lotId))
                                    {
                                        blnBuildOptionSet = true;
                                        // check to see if option was also built on inventory via RO
                                        int intStart = strInvQuoteOptionsBuilt1.IndexOf(RSysSystem.IdToString(rstOptions.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value), 0);
                                        if (intStart > -1)
                                        {
                                            // things are good, we built this on the inventory quote as well, remove
                                            // from tracking list
                                            //ML- mar-12-2007 commented - as was not giving correct results
                                            //strInvQuoteOptionsBuilt1 = strInvQuoteOptionsBuilt1.Substring(0, intStart
                                            //   - 1 ) +
                                            //    strInvQuoteOptionsBuilt1.Substring(strInvQuoteOptionsBuilt1.Length - strInvQuoteOptionsBuilt1.Length - strInvQuoteOptionsBuilt1.IndexOf(";", intStart));
                                            //strInvQuoteOptionsBuilt1 = strInvQuoteOptionsBuilt1.Substring(intStart);
                                            strInvQuoteOptionsBuilt1 = strInvQuoteOptionsBuilt1.Replace((RSysSystem.IdToString(rstOptions.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value)+ ";"), string.Empty);
                                        }
                                        else
                                        {
                                            // we will inactivate quote, remembering why
                                            strInvalidQuoteOptions = RSysSystem.IdToString(rstOptions.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value) + ";";
                                        }
                                    }
                                    rstOptions.MoveNext();
                                }

                                if (blnBuildOptionSet)
                                {
                                    // Option(s) were built, need to set Plan Built and OPTIONS
                                    rstQuote.Fields[modOpportunity.strfPLAN_BUILT].Value = true;
                                    rstQuote.Fields[modOpportunity.strfBUILT_OPTIONS].Value = true;
                                    // also update lot
                                    UpdateHomesitePlan(lotId, rstQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value);
                                }

                                // save changes
                                objLib.PermissionIgnored = true;
                                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstOptions);
                            }
                            // update the quote
                            rstQuote.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value = constructionStageId;
                            rstQuote.Fields[modOpportunity.strfCONFIGURATION_CHANGED].Value = 1;
                            // check for invalid options
                            if (strInvQuoteOptionsBuilt1.Length > 0 || strInvalidQuoteOptions.Length > 0)
                            {
                                rstQuote.Fields[modOpportunity.strfINACTIVE].Value = true;
                                rstQuote.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsINACTIVE;
                                rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value = GetInventoryChangeNote(
                                    TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_INACTIVATE_START, new object[] {DateTime.Today})), 
                                    new string[] {TypeConvert.ToString(LangDict.GetText(modOpportunity.strdINVENTORY_INACTIVATE_INVENTORY_ONLY)), 
                                        TypeConvert.ToString(LangDict.GetText(modOpportunity.strdINVENTORY_INACTIVATE_CUSTOMER_ONLY))}, 
                                    new string[] {strInvQuoteOptionsBuilt1, strInvalidQuoteOptions}) +
                                    "\r\n" + TypeConvert.ToString(rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value);
                            }
                            else
                            {
                                if (strInvQuoteOptionsBuilt.Length == 0)
                                {
                                    rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value = GetInventoryChangeNote(
                                        TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_MODIFIED_START, new object[] {DateTime.Today})), 
                                        TypeConvert.ToString(LangDict.GetText(modOpportunity.strdHOMESITE_CONSTRUCTION_STAGE_CHANGE)),
                                        strInvQuoteOptionsBuilt) +
                                        "\r\n" + TypeConvert.ToString(rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value);
                                }
                                else
                                {
                                    rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value = GetInventoryChangeNote(
                                        TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_MODIFIED_START, new object[] {DateTime.Today})),
                                        TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_MODIFIED_OPTION, new object[] {"True"})),
                                        strInvQuoteOptionsBuilt) +
                                        "\r\n" + TypeConvert.ToString(rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value);
                                }
                            }
                            rstQuote.MoveNext();
                        }
                    }
                    objLib.PermissionIgnored = true;
                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstQuote);
                    rstQuote.Close();

                    // get post-build quotes for lot
                    Recordset rstPostBuildQuote = objLib.GetRecordset(modOpportunity.strqACTIVE_POST_BUILD_QUOTES_FOR_LOT, 1,
                        lotId, modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfPLAN_BUILT,
                        modOpportunity.strfBUILT_OPTIONS, modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfCONFIGURATION_CHANGED,
                        modOpportunity.strfDESCRIPTION, modOpportunity.strfSTATUS, modOpportunity.strfINACTIVE);
                    if (rstPostBuildQuote.RecordCount > 0)
                    {
                        rstPostBuildQuote.MoveFirst();
                        // Jun 26, 2005. By JWang If the homesite construction stage passes Plan's construction stage
                        // then
                        // set quotes's Plan Built flag, and populate homesite's Built Plan
                        if (HomesiteConstructionOrdinalPastPlanOne(intLotStageOrdinal, rstPostBuildQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value))
                        {
                            rstPostBuildQuote.Fields[modOpportunity.strfPLAN_BUILT].Value = true;
                            UpdateHomesitePlan(lotId, rstPostBuildQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value);
                        }
                        while(!(rstPostBuildQuote.EOF))
                        {
                            bool blnBuildOptionSet = false;
                            string strInvQuoteOptionsBuilt1 = strInvQuoteOptionsBuilt;
                            string strInvalidQuoteOptions = "";
                            object vntQuoteId = rstPostBuildQuote.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;

                            //ML - Setting up the Query according to the blnUpdateAll flag used for options using
                            //     Post CutOff Prices.
                            strQueryName = modOpportunity.strqSELECTEDOPP_PRODUCTS_WITH_OPP_ID;
                            if (!blnUpdateAll)
                                strQueryName = modOpportunity.strqSELECTED_OPP_PROD_WITH_OPP_WITHOUT_PCO_PRICE_OPTIONS;

                            // get all the Options for this Quote
                            Recordset rstOptions = objLib.GetRecordset(strQueryName, 1,
                                vntQuoteId, modOpportunity.strfBUILD_OPTION, modOpportunity.strfNBHDP_PRODUCT_ID, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                                modOpportunity.strfPRODUCT_AVAILABLE, modOpportunity.strfTYPE);
                            if (rstOptions.RecordCount > 0)
                            {
                                rstOptions.MoveFirst();
                                while(!(rstOptions.EOF))
                                {
                                    // check to see if option is now built
                                    if (OptionBuildMe(rstOptions, OptionAmIBuilt(rstOptions.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                                        intLotStageOrdinal), lotId))
                                    {
                                        blnBuildOptionSet = true;
                                        // check to see if option was also built on inventory via RO
                                        int intStart = strInvQuoteOptionsBuilt1.IndexOf(RSysSystem.IdToString(rstOptions.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value), 0);
                                        if (intStart > -1)
                                        {
                                            // things are good, we built this on the inventory quote as well, remove
                                            // from tracking list
                                            //ML- mar-12-2007 commented - as was not giving correct results
                                            //strInvQuoteOptionsBuilt1 = strInvQuoteOptionsBuilt1.Substring(0, intStart
                                            //    - 1) +
                                            //    // namespace, please convert them by using .Net Framework.
                                            //    strInvQuoteOptionsBuilt1.Substring(strInvQuoteOptionsBuilt1.Length - strInvQuoteOptionsBuilt1.Length - strInvQuoteOptionsBuilt1.IndexOf(";", intStart));
                                            strInvQuoteOptionsBuilt1 = strInvQuoteOptionsBuilt1.Replace((RSysSystem.IdToString(rstOptions.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value) + ";"), string.Empty);
                                        }
                                        else
                                        {
                                            // we will inactivate quote, remembering why
                                            strInvalidQuoteOptions = RSysSystem.IdToString(rstOptions.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value) + ";";
                                        }
                                    }
                                    rstOptions.MoveNext();
                                }

                                if (blnBuildOptionSet)
                                {
                                    // Option(s) were built, need to set Plan Built and OPTIONS
                                    rstPostBuildQuote.Fields[modOpportunity.strfPLAN_BUILT].Value = true;
                                    rstPostBuildQuote.Fields[modOpportunity.strfBUILT_OPTIONS].Value = true;
                                    // also update lot
                                    UpdateHomesitePlan(lotId, rstPostBuildQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value);
                                }

                                // save changes
                                objLib.PermissionIgnored = true;
                                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstOptions);
                                rstOptions.Close();
                            }
                            // update the quote
                            rstPostBuildQuote.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value = constructionStageId;
                            rstPostBuildQuote.Fields[modOpportunity.strfCONFIGURATION_CHANGED].Value = 1;
                            // check for invalid options
                            if (strInvQuoteOptionsBuilt1.Length > 0 || strInvalidQuoteOptions.Length > 0)
                            {
                                rstPostBuildQuote.Fields[modOpportunity.strfINACTIVE].Value = true;
                                rstPostBuildQuote.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsINACTIVE;
                                rstPostBuildQuote.Fields[modOpportunity.strfDESCRIPTION].Value = GetInventoryChangeNote(
                                    TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_INACTIVATE_START, new object[] {DateTime.Today})),
                                    new string[] {TypeConvert.ToString(LangDict.GetText(modOpportunity.strdINVENTORY_INACTIVATE_INVENTORY_ONLY)), 
                                        TypeConvert.ToString(LangDict.GetText(modOpportunity.strdINVENTORY_INACTIVATE_CUSTOMER_ONLY))},
                                    new string[] {strInvQuoteOptionsBuilt1, strInvalidQuoteOptions}) +
                                    "\r\n" + TypeConvert.ToString(rstPostBuildQuote.Fields[modOpportunity.strfDESCRIPTION].Value);
                            }
                            else
                            {
                                rstPostBuildQuote.Fields[modOpportunity.strfDESCRIPTION].Value = GetInventoryChangeNote(
                                    TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_MODIFIED_START, new object[] {DateTime.Today})),
                                    TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_MODIFIED_OPTION, new object[] {"True"})), 
                                    strInvQuoteOptionsBuilt) +
                                    "\r\n" + TypeConvert.ToString(rstPostBuildQuote.Fields[modOpportunity.strfDESCRIPTION].Value);
                            }
                            rstPostBuildQuote.MoveNext();
                        }
                    }
                    objLib.PermissionIgnored = true;
                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstPostBuildQuote);
                    rstPostBuildQuote.Close();
                }
                else
                {
                    // get all the Quotes that are connected to this Lot
                    // should always be either <= 1
                    Recordset rstQuote = objLib.GetRecordset(modOpportunity.strqACTIVE_CONTRACT_PROGRESS__OR_SALES_REQ_FOR_LOT, 1, lotId, modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfPLAN_BUILT, modOpportunity.strfBUILT_OPTIONS, modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfPIPELINE_STAGE);
                    if (rstQuote.RecordCount > 0)
                    {
                        rstQuote.MoveFirst();
                        // Jun 26, 2005. By JWang
                        // If the homesite construction stage passes Plan's construction stage then
                        // set quotes's Plan Built flag, and populate homesite's Built Plan
                        if (HomesiteConstructionOrdinalPastPlanOne(intLotStageOrdinal, rstQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value))
                        {
                            rstQuote.Fields[modOpportunity.strfPLAN_BUILT].Value = true;
                            UpdateHomesitePlan(lotId, rstQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value);
                        }
                        while(!(rstQuote.EOF))
                        {
                            bool blnBuildOptionSet = false;
                            object vntQuoteId = rstQuote.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                            string strInvQuoteOptionsBuilt1 = strInvQuoteOptionsBuilt;
                            string strInvalidQuoteOptions = string.Empty;
                            object vntQuotePipeline = rstQuote.Fields[modOpportunity.strfPIPELINE_STAGE].Value;

                            //ML - Setting up the Query name depending upon the option chosen by the end user.
                            //ML - 27/06/07 changed the query to consider only selected ones 
                            //  Issue#65536-19627
                            strQueryName = modOpportunity.strqSELECTED_OPP_PROD_WITH_OPP_WITHOUT_PCO_PRICE_OPTIONS;
                            if (blnUpdateAll)
                                strQueryName = modOpportunity.strqSELECTEDOPP_PRODUCTS_WITH_OPP_ID;

                            // get all the Options for this Quote
                            Recordset rstOptions = objLib.GetRecordset(strQueryName, 1, vntQuoteId,
                                modOpportunity.strfBUILD_OPTION, modOpportunity.strfNBHDP_PRODUCT_ID, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                                modOpportunity.strfPRODUCT_AVAILABLE, modOpportunity.strfTYPE);
                            if (rstOptions.RecordCount > 0)
                            {
                                rstOptions.MoveFirst();
                                while(!(rstOptions.EOF))
                                {
                                    // check to see if option is now built
                                    object vntOppProductId = rstOptions.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value;
                                    //ML -27/06/07- commented - we shoud set it once we cross OptionBuidMe!!
                                    //blnBuildOptionSet = true;

                                    // with contract pipeline see if there is any post-sale quotes
                                    if (TypeConvert.ToString(vntQuotePipeline) == modOpportunity.strsCONTRACT)
                                    {
                                        // check to see if there are post-sale quotes, need to make sure each of
                                        // these quotes have the option that is being built
                                        Recordset rstPostSaleQuotes = objLib.GetRecordset(modOpportunity.strqACTIVE_POST_SALE_QUOTES_FOR_OPP,
                                            2, vntQuoteId, modOpportunity.strsPOST_SALE, modOpportunity.strfOPPORTUNITY_ID,
                                            modOpportunity.strfSTATUS, modOpportunity.strfINACTIVE);
                                        if (rstPostSaleQuotes.RecordCount > 0)
                                        {
                                            rstPostSaleQuotes.MoveFirst();
                                            while(!(rstPostSaleQuotes.EOF))
                                            {
                                                //ML - Setting up the Query name depending upon the option chosen by the end user.
                                                strQueryName = modOpportunity.strqSELECTED_OPTIONS_FOR_OPP_AND_NBHDP_WITHOUT_PCO;
                                                if (blnUpdateAll)
                                                    strQueryName = modOpportunity.strqSEL_OPTIONS_FOR_OPP_NBHDPRODUCT;
                                                Recordset rstPSOptions = objLib.GetRecordset(strQueryName,
                                                    2, rstPostSaleQuotes.Fields[modOpportunity.strfOPPORTUNITY_ID].Value,
                                                    rstOptions.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value,
                                                    modOpportunity.strfOPPORTUNITY__PRODUCT_ID, modOpportunity.strfBUILD_OPTION);
                                                if (OptionBuildMe(rstOptions, OptionAmIBuilt(rstOptions.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value, 
                                                        intLotStageOrdinal), lotId))
                                                    {
                                                        blnBuildOptionSet = true;

                                                        if (rstPSOptions.RecordCount > 0)
                                                        {
                                                            // build option found on the post-sale,
                                                            rstPSOptions.MoveFirst();
                                                            rstPSOptions.Fields[modOpportunity.strfBUILD_OPTION].Value = true;
                                                        }
                                                        else
                                                        {
                                                            // inactivate the quote
                                                            rstPostSaleQuotes.Fields[modOpportunity.strfSTATUS].Value =
                                                                modOpportunity.strsINACTIVE;
                                                            rstPostSaleQuotes.Fields[modOpportunity.strfINACTIVE].Value
                                                                = true;
                                                        }
                                                        //   Create Homesite Configuration - fpoulsen 07/18/2005
                                                        objLotConfig.CreateLotConfiguration(vntOppProductId, lotId);
                                                    }
                                                    else
                                                    {
                                                        // update the lot config, remove any recs which were un-build
                                                        objLotConfig.CreateLotConfiguration(vntOppProductId, lotId, true);
                                                        //ML june 7 2007 Issue#65536-19348 
                                                        if (rstPSOptions.RecordCount > 0)
                                                        {
                                                        // update the build flag
                                                            rstPSOptions.MoveFirst();
                                                        rstPSOptions.Fields[modOpportunity.strfBUILD_OPTION].Value = false;
                                                    }
                                                    }
                                                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstPSOptions);
                                                rstPostSaleQuotes.MoveNext();
                                            }
                                            objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstPostSaleQuotes);
                                        }
                                        //ML Apr 10 2007 - need to update the options for the contract
                                        //Issue#65536-18822
                                        if (OptionBuildMe(rstOptions, OptionAmIBuilt(rstOptions.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                                                        intLotStageOrdinal), lotId))
                                        {
                                            blnBuildOptionSet = true;
                                        }
                                    }
                                    rstOptions.MoveNext();
                                }

                                if (blnBuildOptionSet)
                                {
                                    // Option(s) were built, need to set Plan Built and OPTIONS
                                    rstQuote.Fields[modOpportunity.strfPLAN_BUILT].Value = true;
                                    rstQuote.Fields[modOpportunity.strfBUILT_OPTIONS].Value = true;
                                    // also update lot
                                    UpdateHomesitePlan(lotId, rstQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value);

                                }

                                // save changes
                                objLib.PermissionIgnored = true;
                                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstOptions);
                            }
                            // update the quote
                            rstQuote.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value = constructionStageId;
                            rstQuote.MoveNext();
                        }
                    }
                    objLib.PermissionIgnored = true;
                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstQuote);
                    rstQuote.Close();
                }
                return true;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// When the Construction Stage on the lot ia changed, update the Construction Stage on all
        /// quotes and update the built flag on all options on all quotes belonging to the lot
        /// </summary>
        /// <param name="lotId">Lot Id</param>
        /// <param name="opportunityProductId">Opportunit Product Id</param>
        /// <param name="builtValue">Flag to indicate whether build value or not</param>
        /// <returns>True if its successful in updating everything False otherwise</returns> 
        /// <history>
        /// Revision #   Date         Author   Description
        /// 3.8.0.0  5/12/2006  DYin  Converted to .Net C# code.
        /// </history>
        public virtual bool UpdateQuoteOptionsSingleOption(object lotId, object opportunityProductId, bool builtValue)
        {
            try
            {
                if (lotId == DBNull.Value) return false;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                LotConfiguration objLotConfig = (LotConfiguration)RSysSystem.ServerScripts[modOpportunity.strsLOT_CONFIG].CreateInstance();

                // update homesite if setting to built
                if (builtValue)
                {
                    // Jun 23, 2005. By JWang
                    // if it is Elevation option then populate Lot's Built Elevation with the NBHDP_Product_Id of the
                    // option
                    if (TypeConvert.ToString(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY__PRODUCT].Fields[modOpportunity.strfTYPE].Index(opportunityProductId))
                        == modOpportunity.strsELEVATION)
                    {
                        UpdateHomesiteBuiltElevation(lotId, RSysSystem.Tables[modOpportunity.strtOPPORTUNITY__PRODUCT].Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Index(opportunityProductId));
                    }

                    Recordset rstCurOpp = objLib.GetRecordset(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY__PRODUCT].Fields[modOpportunity.strfOPPORTUNITY_ID].Index(opportunityProductId),
                        modOpportunity.strt_OPPORTUNITY, modOpportunity.strfPLAN_BUILT, modOpportunity.strfPLAN_NAME_ID,
                        modOpportunity.strfBUILT_OPTIONS);
                    if (rstCurOpp.RecordCount > 0)
                    {
                        rstCurOpp.MoveFirst();
                        // need to set Plan Built and OPTIONS
                        rstCurOpp.Fields[modOpportunity.strfPLAN_BUILT].Value = true;
                        rstCurOpp.Fields[modOpportunity.strfBUILT_OPTIONS].Value = true;
                        // also update lot
                        UpdateHomesitePlan(lotId, rstCurOpp.Fields[modOpportunity.strfPLAN_NAME_ID].Value);
                        objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstCurOpp);
                        rstCurOpp.Close();
                    }

                    Recordset rstInvQuote = objLib.GetRecordset(modOpportunity.strqACTIVE_INVENTORY_QUOTES_FOR_LOT, 1, lotId,
                        modOpportunity.strfPLAN_BUILT, modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfBUILT_OPTIONS);
                    if (rstInvQuote.RecordCount > 0)
                    {
                        // need to set Plan Built and OPTIONS
                        rstInvQuote.MoveFirst();
                        rstInvQuote.Fields[modOpportunity.strfPLAN_BUILT].Value = true;
                        rstInvQuote.Fields[modOpportunity.strfBUILT_OPTIONS].Value = true;
                        objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstInvQuote);
                        rstInvQuote.Close();
                    }

                    // Create Homesite Configuration - fpoulsen 06/27/2005
                    objLotConfig.CreateLotConfiguration(opportunityProductId, lotId);
                }
                else
                {
                    // Jun 23, 2005. By JWang
                    // if it is Elevation option then clear out Built Elevation for lot
                    if (TypeConvert.ToString(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY__PRODUCT].Fields[modOpportunity.strfTYPE].Index(opportunityProductId))
                        == modOpportunity.strsELEVATION)
                    {
                        UpdateHomesiteBuiltElevation(lotId, DBNull.Value);
                    }
                    // remove any that were un-built
                    objLotConfig.CreateLotConfiguration(opportunityProductId, lotId, true);
                }
                // get customer quotes for lot
                Recordset rstQuote = objLib.GetRecordset(modOpportunity.strqACTIVE_CUSTOMER_QUOTES_FOR_LOT, 1, lotId, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                    modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfPLAN_BUILT, modOpportunity.strfPLAN_NAME_ID,
                    modOpportunity.strfCONFIGURATION_CHANGED, modOpportunity.strfDESCRIPTION, modOpportunity.strfSTATUS,
                    modOpportunity.strfINACTIVE, modOpportunity.strfBUILT_OPTIONS);
                if (rstQuote.RecordCount > 0)
                {
                    rstQuote.MoveFirst();
                    while(!(rstQuote.EOF))
                    {
                        bool blnBuildOptionSet = false;
                        object vntQuoteId = rstQuote.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                        // get all the Options for this Quote
                        Recordset rstOptions = objLib.GetRecordset(modOpportunity.strqSELECTEDOPP_PRODUCTS_WITH_OPP_ID, 1, vntQuoteId,
                            modOpportunity.strfBUILD_OPTION, modOpportunity.strfNBHDP_PRODUCT_ID, modOpportunity.strfPRODUCT_AVAILABLE,
                            modOpportunity.strfPRODUCT_NAME, modOpportunity.strfTYPE);
                        if (rstOptions.RecordCount > 0)
                        {
                            rstOptions.MoveFirst();
                            while(!(rstOptions.EOF))
                            {
                                // find the single option that was built via RO comparison
                                if (RSysSystem.EqualIds(rstOptions.Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Value, objLib.SqlIndex(
                                    modOpportunity.strtOPPORTUNITY__PRODUCT, modOpportunity.strfNBHDP_PRODUCT_ID, opportunityProductId)))
                                {
                                    if (OptionBuildMe(rstOptions, builtValue, lotId))
                                    {
                                        blnBuildOptionSet = true;
                                        // need to set Plan Built and Built Options
                                        rstQuote.Fields[modOpportunity.strfPLAN_BUILT].Value = true;
                                        rstQuote.Fields[modOpportunity.strfBUILT_OPTIONS].Value = true;
                                    }
                                }
                                rstOptions.MoveNext();
                            }
                            // save changes
                            objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstOptions);
                            rstOptions.Close();
                        }
                        // update the quote
                        rstQuote.Fields[modOpportunity.strfCONFIGURATION_CHANGED].Value = 1;
                        // check for invalid options
                        object neighborhoodPhaseProductId = RSysSystem.IdToString(objLib.SqlIndex( modOpportunity.strt_OPPORTUNITY__PRODUCT, modOpportunity.strfNBHDP_PRODUCT_ID, opportunityProductId)) + ";";
                        if (builtValue && !blnBuildOptionSet)
                        {
                            rstQuote.Fields[modOpportunity.strfINACTIVE].Value = true;
                            rstQuote.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsINACTIVE;
                            rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value = GetInventoryChangeNote(
                                TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_INACTIVATE_START, new object[] {DateTime.Today})), 
                                TypeConvert.ToString(LangDict.GetText(modOpportunity.strdINVENTORY_INACTIVATE_INVENTORY_ONLY)),
                                RSysSystem.IdToString(neighborhoodPhaseProductId)) +
                                "\r\n" + rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value;
                        }
                        else
                        {
                            string strBuiltValue = (builtValue ? "true" : "false");
                            string strTitle = TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_MODIFIED_START, new object[] { DateTime.Today }));
                            string strSubject = TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_MODIFIED_OPTION, new object[] { strBuiltValue }));
                            string strData = TypeConvert.ToString(neighborhoodPhaseProductId) + "\r\n" + TypeConvert.ToString(rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value);
                            
                            rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value = GetInventoryChangeNote(strTitle, strSubject, strData);                            
                        }
                        rstQuote.MoveNext();
                    }
                }
                objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstQuote);
                rstQuote.Close();
                return true;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Generates the note to be placed on the quote when inactivating because of an inventory change
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="subject">Subjects</param>
        /// <param name="data">Data for the note.</param>
        /// <returns>note as string</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 3.8.0.0  5/12/2006  DYin  Converted to .Net C# code.
        /// </history>
        protected virtual string GetInventoryChangeNote(string title, string subject, string data)
        {
            return this.GetInventoryChangeNote(title, new string[] {subject}, new string[] {data});
        }

        /// <summary>
        /// Generates the note to be placed on the quote when inactivating because of an inventory change
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="subjectArray">Array to contain subjects</param>
        /// <param name="dataArray">Array to contains data for the note</param>
        /// <returns>note as string</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 3.8.0.0  5/12/2006  DYin  Converted to .Net C# code.
        /// </history>
        protected virtual string GetInventoryChangeNote(string title, string[] subjectArray, string[] dataArray)
        {
            object objId = null;

            try
            {
                StringBuilder noteBuilder = new StringBuilder();
                if (title.Length > 0)
                {
                    noteBuilder.Append(title);
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    for (int i = 0; i < subjectArray.Length; ++i)
                    {
                        if (subjectArray[i].Length > 0)
                        {
                            noteBuilder.Append(TypeConvert.ToString(subjectArray[i]));
                            if (TypeConvert.ToString(dataArray[i]).Length > 0)
                            {
                                string[] arrData = dataArray[i].Split(new char[] {Convert.ToChar(";")});
                                for(int j = 0; j < arrData.Length; ++ j)
                                {
                                    if (arrData[j].Length > 0)
                                    {
                                        try
                                        {
                                            objId = RSysSystem.StringToId(arrData[j]);
                                        }
                                        catch
                                        {
                                            noteBuilder.Append(TypeConvert.ToString(arrData[j]));
                                        }
                                        finally
                                        {
                                            noteBuilder.Append(TypeConvert.ToString(objLib.SqlIndex(
                                                modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                                                objId)));
                                        }
                                        if (j != arrData.GetUpperBound(0))
                                        {
                                            noteBuilder.Append(",");
                                        }
                                    }
                                }
                            }
                            noteBuilder.Append("\r\n");
                        }
                    }
                }
                return noteBuilder.ToString();

            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        ///  Inactive customer's quotes with no reason.
        /// </summary>
        /// <param name="opportunityProductId">Opportunity Product id.</param>
        /// <param name="opportunityId">Oppotunity Id</param>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 3.8.0.0  5/12/2006  DYin  Converted to .Net C# code.
        /// </history>
        public virtual void InactivateCustomerQuotes(object opportunityProductId, object opportunityId)
        {
            InactivateCustomerQuotes(opportunityProductId, opportunityId, InactiveQuoteReason.NoReason);
        }

        /// <summary>
        ///  Inactive customer's quotes.
        /// </summary>
        /// <param name="opportunityProductId">Opportunity Product id.</param>
        /// <param name="opportunityId">Oppotunity Id</param>
        /// <param name="inactivateReason">Enumerator to indicate the inactive reason</param>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 3.8.0.0  5/12/2006  DYin  Converted to .Net C# code.
        /// </history>
        public virtual void InactivateCustomerQuotes(object opportunityProductId, object opportunityId,
            InactiveQuoteReason inactivateReason)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                object neighborhoodPhaseProductId = DBNull.Value;
                if (inactivateReason == InactiveQuoteReason.NoReason)
                {
                    if ((opportunityId == null) || (opportunityId == DBNull.Value))
                    {
                        inactivateReason = InactiveQuoteReason.OptionChange;
                        opportunityId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY__PRODUCT, modOpportunity.strfOPPORTUNITY_ID,
                            opportunityProductId);
                        neighborhoodPhaseProductId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY__PRODUCT,
                            modOpportunity.strfNBHDP_PRODUCT_ID, opportunityProductId);
                    }
                    else
                    {
                        inactivateReason = InactiveQuoteReason.PlanChange;
                    }
                }

                if (TypeConvert.ToString(objLib.SqlIndex( modOpportunity.strt_OPPORTUNITY, modOpportunity.strfSTATUS,
                    opportunityId)) == modOpportunity.strsINVENTORY || inactivateReason == InactiveQuoteReason.ConvertToSale
                    || inactivateReason == InactiveQuoteReason.PostBuildAccept)
                {
                    // get customer quotes for lot
                    Recordset rstQuote = objLib.GetRecordset(modOpportunity.strqACTIVE_CUSTOMER_QUOTES_FOR_LOT, 1, 
                        objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfLOT_ID, opportunityId), 
                        modOpportunity.strfDESCRIPTION, modOpportunity.strfSTATUS, modOpportunity.strfINACTIVE);
                    if (rstQuote.RecordCount > 0)
                    {
                        rstQuote.MoveFirst();
                        while(!(rstQuote.EOF))
                        {
                            rstQuote.Fields[modOpportunity.strfINACTIVE].Value = true;
                            rstQuote.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsINACTIVE;
                            if (inactivateReason == InactiveQuoteReason.PlanChange)
                            {
                                rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value = GetInventoryChangeNote(
                                    TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_INACTIVATE_PLAN_CHANGE, new object[] {DateTime.Today})),
                                    string.Empty, string.Empty) +
                                    "\r\n" + rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value;
                            }
                            else if (inactivateReason == InactiveQuoteReason.OptionChange)
                            {
                                rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value = GetInventoryChangeNote(
                                    TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_INACTIVATE_START, new object[] {DateTime.Today})),
                                    TypeConvert.ToString(LangDict.GetText(modOpportunity.strdINVENTORY_INACTIVATE_CHANGED)),
                                    RSysSystem.IdToString(neighborhoodPhaseProductId) + ";") +
                                    "\r\n" + TypeConvert.ToString(rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value);
                            }
                            else if (inactivateReason == InactiveQuoteReason.NewInventoryQuote)
                            {
                                rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value = TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_INACTIVATE_START,new
                                    object[] {DateTime.Today})) + TypeConvert.ToString(LangDict.GetText(modOpportunity.strdINVENTORY_INACTIVATE_ADD_INVENT))
                                    +
                                    "\r\n" + TypeConvert.ToString(rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value);
                            }
                            else if (inactivateReason == InactiveQuoteReason.ConvertToSale)
                            {
                                // do nothing
                            }
                            else if (inactivateReason == InactiveQuoteReason.PostBuildAccept)
                            {
                                rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value = TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_INACTIVATE_START,new
                                    object[] {DateTime.Today})) + TypeConvert.ToString(LangDict.GetText(modOpportunity.strdINVENTORY_QUOTE_CHANGED))
                                    +
                                    "\r\n" + rstQuote.Fields[modOpportunity.strfDESCRIPTION].Value;
                            }
                            else
                            {
                                // do nothing
                            }
                            rstQuote.MoveNext();
                        }
                    }
                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstQuote);
                    rstQuote.Close();
                    object vntLot_Id = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strfLOT_ID,
                        opportunityId);
                    string vntLot_Status = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtPRODUCT, modOpportunity.strfLOT_STATUS,
                        vntLot_Id));
                    DateTime vntRelease_Date = TypeConvert.ToDateTime(objLib.SqlIndex(modOpportunity.strtPRODUCT, modOpportunity.strfRELEASE_DATE,
                        vntLot_Id));
                    if (vntLot_Status == modOpportunity.strsRESERVED)
                    {
                        if ((vntRelease_Date.GetType() == typeof(DateTime)) && vntRelease_Date <= DateTime.Today)
                        {
                            UpdateLotStatusEx(vntLot_Id, modOpportunity.strsAVAILABLE);
                        }
                        else
                        {
                            UpdateLotStatusEx(vntLot_Id, modOpportunity.strsLOT_STATUS_NOT_RELEASED);
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// When an Opportunity is saved, or when a Quote goes through a sale or cancellation, this is
        /// called to update the cobuyer status.
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="sale"> True if sale</param>
        /// <param name="canceled"> True if contract is canceled</param>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 3.8.0.0  5/12/2006  DYin  Converted to .Net C# code.
        /// (Buyer type is not used for contact)
        /// </history>
        protected virtual void UpdateCoBuyerStatus(object opportunityId, bool sale, bool canceled)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                if (opportunityId != DBNull.Value)
                {
                    // see if the Opp has any co-buyers!
                    Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strf_CONTACT_ID,
                        modOpportunity.strf_STATUS, modOpportunity.strfLOT_ID, modOpportunity.strfACTUAL_REVENUE_DATE);
                    if (rstOpportunity.RecordCount > 0)
                    {
                        rstOpportunity.MoveFirst();
                        object vntContactId = rstOpportunity.Fields[modOpportunity.strf_CONTACT_ID].Value;
                        object vntLotId = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                        if ((vntContactId != DBNull.Value))
                        {
                            string strMainContactType = TypeConvert.ToString(!(Convert.IsDBNull(RSysSystem.Tables[modOpportunity.strt_CONTACT].Fields[modOpportunity.strfTYPE].Index(vntContactId))) ? RSysSystem.Tables[modOpportunity.strt_CONTACT].Fields[modOpportunity.strfTYPE].Index(vntContactId) : "Prospect");
                            Recordset rstCoBuyers = objLib.GetRecordset(modOpportunity.strqCONTACT_COBUYERS_FOR_CONTACT, 1, vntContactId,
                                modOpportunity.strfCO_BUYER_CONTACT_ID);
                            if (rstCoBuyers.RecordCount > 0)
                            {
                                rstCoBuyers.MoveFirst();
                                while(!(rstCoBuyers.EOF))
                                {
                                    // go through each contact and change the status only if necessary
                                    object vntCoBuyerId = rstCoBuyers.Fields[modOpportunity.strfCO_BUYER_CONTACT_ID].Value;
                                    if ((vntCoBuyerId != DBNull.Value))
                                    {
                                        Recordset rstContactCoBuyer = objLib.GetRecordset(vntCoBuyerId, modOpportunity.strt_CONTACT,
                                            modOpportunity.strfTYPE, modOpportunity.strfCLOSE_DATE);
                                        if (rstContactCoBuyer.RecordCount > 0)
                                        {
                                            rstContactCoBuyer.MoveFirst();
                                            // update contract close dates on these cobuyer contacts
                                            if (Convert.IsDBNull(rstContactCoBuyer.Fields[modOpportunity.strfCLOSE_DATE].Value)
                                                || (TypeConvert.ToDateTime(rstContactCoBuyer.Fields[modOpportunity.strfCLOSE_DATE].Value) < 
                                                TypeConvert.ToDateTime(rstOpportunity.Fields[modOpportunity.strfACTUAL_REVENUE_DATE].Value)))
                                            {
                                                rstContactCoBuyer.Fields[modOpportunity.strfCLOSE_DATE].Value = rstOpportunity.Fields[modOpportunity.strfACTUAL_REVENUE_DATE].Value;
                                            }

                                            objLib.SaveRecordset(modOpportunity.strt_CONTACT, rstContactCoBuyer);

                                            if (sale)
                                            {
                                                // Changed from strsBUYER to strsCUSTOMER - fpoulsen 06/21/2005
                                                if (strMainContactType == modOpportunity.strsCUSTOMER)
                                                {
                                                    // link the cobuyers to the lot's associated contact list if not
                                                    // already listed
                                                    Recordset rstAssociatedContacts = objLib.GetRecordset(modOpportunity.strqLOTS_CONTACTS_FOR_LOT_CONTACT,
                                                        2, vntLotId, vntCoBuyerId, modOpportunity.strfTYPE);
                                                    if (rstAssociatedContacts.RecordCount <= 0)
                                                    {
                                                        // not found, create a new record to add it to the lot's assoc
                                                        // contacts
                                                        Recordset rstLotContact = objLib.GetNewRecordset(modOpportunity.strtLOT__CONTACT,
                                                            modOpportunity.strfTYPE, modOpportunity.strf_CONTACT_ID,
                                                            modOpportunity.strfPRODUCT_ID);
                                                        rstLotContact.AddNew(Type.Missing, Type.Missing);
                                                        rstLotContact.Fields[modOpportunity.strf_CONTACT_ID].Value =
                                                            vntCoBuyerId;
                                                        rstLotContact.Fields[modOpportunity.strfPRODUCT_ID].Value =
                                                            rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                                                        rstLotContact.Fields[modOpportunity.strfTYPE].Value = modOpportunity.intCO_BUYER_TYPE;

                                                        objLib.SaveRecordset(modOpportunity.strtLOT__CONTACT, rstLotContact);
                                                    }
                                                }
                                            }
                                            if (canceled)
                                            {
                                                // added by Carl Langan - 01/10/05
                                                // remove the co-buyer from the Lot__Contact table for this lot
                                                // link the cobuyers to the lot's associated contact list if not already
                                                // listed
                                                objLib.DeleteRecordset(modOpportunity.strqLOTS_CONTACTS_FOR_LOT_CONTACT, modOpportunity.strfLOT__CONTACT_ID, 
                                                    vntLotId, vntCoBuyerId);
                                            }
                                        }
                                    }
                                    rstCoBuyers.MoveNext();
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will add Change Orders records
        /// </summary>
        /// <param name="opportunityProductId">the Opp Product Id record</param>
        /// <param name="changeOrderId">the change order Id</param>
        /// <param name="opportunityId">the opportunity Id</param>
        /// <param name="selected">boolean value</param>
        /// <returns>
        /// True or False</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual bool AddChangeCustomOrders(object opportunityProductId, object changeOrderId, object opportunityId, bool selected)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                if ((opportunityProductId != DBNull.Value) && (opportunityId != DBNull.Value))
                {
                    Recordset rstOppProduct = objLib.GetRecordset(opportunityProductId, modOpportunity.strt_OPPORTUNITY__PRODUCT, modOpportunity.strfBUILT_OPTION,
                        modOpportunity.strfCODE_, modOpportunity.strfCONSTRUCTION_STAGE_ID, modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL,
                        modOpportunity.strfCUSTOMERINSTRUCTIONS, modOpportunity.strfDELTA_BUILT_OPTION, modOpportunity.strfDEPOSIT,
                        modOpportunity.strfDIVISION_PRODUCT_ID, modOpportunity.strfEXTENDED_PRICE, modOpportunity.strfFILTER_VISIBILITY,
                        modOpportunity.strf_NBHDP_PRODUCT_ID, modOpportunity.strfNET_CONFIG, modOpportunity.strfOPP_CURRENCY,
                        modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strf_OPPORTUNITY__PRODUCT_ID, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID,
                        modOpportunity.strfOPTIONNOTES, modOpportunity.strfPREFERENCE, modOpportunity.strfPREFERENCES_LIST,
                        modOpportunity.strfPRICE, modOpportunity.strfPRODUCT_AVAILABLE, modOpportunity.strfPRODUCT_ID,
                        modOpportunity.strfPRODUCT_NAME, modOpportunity.strfQUANTITY, modOpportunity.strfQUOTED_PRICE,
                        modOpportunity.strfSELECTED, modOpportunity.strfTICKLE_COUNTER, modOpportunity.strfTYPE
                        //modOpportunity.EnvGTINField, modOpportunity.EnvNHTManufacturerNumberField,
                        //modOpportunity.EnvProductBrandField, modOpportunity.EnvProductNumberField,
                        //modOpportunity.EnvDUNSNumberField, modOpportunity.EnvUCCCodeField,
                        //modOpportunity.EnvManufacturerProductField
                        );

                    if (rstOppProduct.RecordCount > 0)
                    {
                        rstOppProduct.MoveFirst();
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
                            modOpportunity.strfSELECTED, modOpportunity.strfTICKLE_COUNTER, modOpportunity.strfTYPE
                            //modOpportunity.EnvGTINField, modOpportunity.EnvNHTManufacturerNumberField,
                            //modOpportunity.EnvProductBrandField, modOpportunity.EnvProductNumberField,
                            //modOpportunity.EnvDUNSNumberField, modOpportunity.EnvUCCCodeField
                            );
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
                        if (selected)
                            rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_STATUS].Value = 0;
                        else
                            rstChangeOrder.Fields[modOpportunity.strfCHANGE_ORDER_STATUS].Value = 1;

                        //rstChangeOrder.Fields[modOpportunity.EnvManufacturerProductField].Value = EnvManufacturerProduct(
                        //    TypeConvert.ToString(rstChangeOrder.Fields[modOpportunity.EnvGTINField].Value)
                        //    , TypeConvert.ToString(rstChangeOrder.Fields[modOpportunity.EnvNHTManufacturerNumberField].Value)
                        //    , TypeConvert.ToString(rstChangeOrder.Fields[modOpportunity.EnvProductNumberField].Value)
                        //    , TypeConvert.ToString(rstChangeOrder.Fields[modOpportunity.EnvDUNSNumberField].Value)
                        //    , TypeConvert.ToString(rstChangeOrder.Fields[modOpportunity.EnvUCCCodeField].Value));

                        objLib.SaveRecordset(modOpportunity.strtCHANGE_ORDER_OPTIONS, rstChangeOrder);
                    }
                }
                return true;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method will re-calculate totals for the gived Opportunity, options and adjsustments totals
        /// </summary>
        /// <param name="quoteOpportunityId">Target Opportunity Id</param>
        /// <param name="quoteStage">Quote stage</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#    Date       Author    Description
        /// 3.8.0.0      5/12/2006  DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void ResetQuote(object quoteOpportunityId, string quoteStage)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // reset the options
                if ((quoteOpportunityId != DBNull.Value))
                {
                    Recordset rstOpp = objLib.GetRecordset(quoteOpportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strf_PIPELINE_STAGE,
                        modOpportunity.strfCONTACT_ID, modOpportunity.strf_STATUS);
                    if (rstOpp.RecordCount > 0)
                    {
                        rstOpp.MoveFirst();
                        rstOpp.Fields[modOpportunity.strf_PIPELINE_STAGE].Value = quoteStage;
                        rstOpp.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsIN_PROGRESS;
                    }
                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstOpp);

                    // reset the contact/co-buyer statuses

                    object vntContactId = rstOpp.Fields[modOpportunity.strfCONTACT_ID].Value;

                    if ((vntContactId != DBNull.Value))
                    {
                        Recordset rstQuotes = objLib.GetRecordset(modOpportunity.strqCONTRACTS_WHERE_CONTACT, 1, vntContactId, modOpportunity.strfOPPORTUNITY_ID);
                        if (rstQuotes.RecordCount > 0)
                        {
                            // one contract there leave the type as buyer
                        }
                        else
                        {
                            Recordset rstContact = objLib.GetRecordset(vntContactId, modOpportunity.strtCONTACT, modOpportunity.strfTYPE);
                            if (rstContact.RecordCount > 0)
                            {
                                rstContact.MoveFirst();
                                // change the status back to prospect provided there isn't any contracts for this contact
                                rstContact.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsCOMPANY_TYPE_PROSPECT;
                            }
                            objLib.SaveRecordset(modOpportunity.strtCONTACT, rstContact);
                            UpdateCoBuyerStatus(quoteOpportunityId, false, false);
                        }
                        ResetContactCobuyerType(vntContactId, quoteOpportunityId);
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method will re-calculate totals for the gived Opportunity, options and adjsustments totals
        /// </summary>
        /// <param name="contactId">Contact Id</param>
        /// <param name="opportunityId">Target Opportunity Id</param>
        /// <history>
        /// Revision#    Date       Author    Description
        /// 3.8.0.0      5/12/2006  DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void ResetContactCobuyerType(object contactId, object opportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // reset the contact/co-buyer statuses
                if ((contactId != DBNull.Value) && (opportunityId  != DBNull.Value))
                {
                    Recordset rstQuotes = objLib.GetRecordset(modOpportunity.strqCONTRACTS_WHERE_CONTACT, 1, contactId, modOpportunity.strfOPPORTUNITY_ID);
                    if (rstQuotes.RecordCount == 0)
                    {
                        Recordset rstContact = objLib.GetRecordset(contactId, modOpportunity.strtCONTACT, modOpportunity.strfTYPE);
                        if (rstContact.RecordCount > 0)
                        {
                            // change the status back to prospect provided there isn't any contracts for this contact
                            rstContact.MoveFirst();
                            rstContact.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsCOMPANY_TYPE_PROSPECT;
                        }
                        objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstContact);
                        UpdateCoBuyerStatus(opportunityId, false, false);
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Checks whether a Construction stage can be inactivated i.e. it is not associated with any active division
        /// products
        /// </summary>
        /// <returns>Inventory Management Allowed</returns>
        /// <history>
        /// Revision#  Date        Author   Description
        /// 3.8.0.0    5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        protected virtual string InventoryManagementAllowedForCurrentUser()
        {
            const string undefinedInventoryManagementAllowed = "Undefined";
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // Get MANAGEMENT_ALLOWED from the current user's setting if it exists
                object vntCurrentUserId = RSysSystem.CurrentUserId();
                if (vntCurrentUserId != DBNull.Value)
                {
                    Recordset rstCurrEmployee = objLib.GetRecordset(modOpportunity.strqDIVISION_OF_CURRENT_USER, 0, modOpportunity.strfINVENTORY_MANAGEMENT_ALLOWED);
                    if (rstCurrEmployee.RecordCount > 0)
                    {
                        rstCurrEmployee.MoveFirst();
                        return TypeConvert.ToString(rstCurrEmployee.Fields[modOpportunity.strfINVENTORY_MANAGEMENT_ALLOWED].Value);
                    }
                    else
                        return undefinedInventoryManagementAllowed;
                }
                return undefinedInventoryManagementAllowed;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Calls other functions which take care of following:
        /// Updation of "reservation date" in Contact Profile NBHD
        /// Update Lot Status
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#  Date        Author   Description
        /// 3.8.0.0    5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        public virtual void UpdateReservationStatus(object contactId, object  neighborhoodId, DateTime reservationDate, 
            object lotId, object opportunityQuoteId)
        {
            try
            {
                UpdateContactProfileNeighborhood(contactId, neighborhoodId, reservationDate, null, null, null,
                    null, null, null, null, null, null, null);
                UpdateLotStatus(lotId, opportunityQuoteId);
                WriteContractHistoryRecords(lotId, opportunityQuoteId, modOpportunity.strsRESERVED, DateTime.Today, true, null, false, false);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        /// <summary>
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#  Date        Author   Description
        /// 3.8.0.0    5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateContactProfileNeighborhood(object contactId, object neighborhoodId, object parameterList)
        {
            try
            {

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // Look up NBHDP Id
                Recordset rstCPNBHDLookup = objLib.GetRecordset(modOpportunity.strqCONTACT_PROFILE_NBHD_FOR_CONTACT, 2, contactId,
                    neighborhoodId, modOpportunity.strfCONTACT_PROFILE_NBHD_ID);

                object vntParams = DBNull.Value;
                if (rstCPNBHDLookup.RecordCount > 0)
                {
                    object vntNBHDPId = rstCPNBHDLookup.Fields[modOpportunity.strfCONTACT_PROFILE_NBHD_ID].Value;

                    // Load recordset using form
                    IRForm pFormContactNBHDProfile = RSysSystem.Forms[modOpportunity.strrHB_CONTACT_PROFILE_NBHD];
                    object vntRecordset = pFormContactNBHDProfile.DoLoadFormData(vntNBHDPId, ref vntParams);
                    object[] recordsetArray = (object[]) vntRecordset;
                    Recordset rstCPNBHD = (Recordset) recordsetArray[0];

                    TransitionPointParameter transitionPointParameter = (TransitionPointParameter)RSysSystem.ServerScripts
                        [AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                    transitionPointParameter.ParameterList = parameterList;
                    object firstVisitDate = rstCPNBHD.Fields[modOpportunity.strfFIRST_VISIT_DATE].Value;
                    object dteCloseDate =
                    transitionPointParameter.SetDefaultFields(rstCPNBHD);

                    if (firstVisitDate != DBNull.Value) rstCPNBHD.Fields[modOpportunity.strfFIRST_VISIT_DATE].Value = firstVisitDate;
                    if (rstCPNBHD.Fields[modOpportunity.strfCANCEL_DATE].Value != DBNull.Value)
                    {
                        // May 31 2005 By FPoulsen. update NBHD Profile type as Cancelled
                        rstCPNBHD.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsCANCELLED;
                    }

                    if (rstCPNBHD.Fields[modOpportunity.strfCLOSE_DATE].Value != DBNull.Value)
                    {
                        // May 26 By JWang. update NBHD Profile type as Closed
                        rstCPNBHD.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsCLOSED;
                    }

                    // Save recordset using form so that the save initiates other functionality
                    pFormContactNBHDProfile.SaveFormData(vntRecordset, ref vntParams);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#  Date        Author   Description
        /// 3.8.0.0    5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        public virtual void UpdateContactProfileNeighborhood(object contactId, object neighborhoodId, object reservationDate,
            object saleDate, object firstVisitDate, object saleDeclinedDate, object cancelDeclinedDate, object cancelRequestDate,
            object cancelDate, object salesRequestDate, object closeDate, object reservationExpirationDate, object quoteCreateDate)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // May 26 By JWang. added strfTYPE in the field list
                // Look up NBHDP Id
                Recordset rstCPNBHDLookup = objLib.GetRecordset(modOpportunity.strqCONTACT_PROFILE_NBHD_FOR_CONTACT, 2, contactId,
                    neighborhoodId, modOpportunity.strfRESERVATION_DATE, modOpportunity.strfSALE_DATE, modOpportunity.strfFIRST_VISIT_DATE,
                    modOpportunity.strfCONTACT_PROFILE_NBHD_ID, modOpportunity.strfSALE_DECLINED_DATE, modOpportunity.strfCANCEL_DECLINED_DATE,
                    modOpportunity.strfCANCEL_REQUEST_DATE, modOpportunity.strfCANCEL_DATE, modOpportunity.strfSALES_REQUEST_DATE,
                    modOpportunity.strfCLOSE_DATE, modOpportunity.strfTYPE, modOpportunity.strfRESERVATIONEXPIRY, modOpportunity.strfQUOTE_DATE);

                if (rstCPNBHDLookup.RecordCount > 0)
                {
                    object vntNBHDPId = rstCPNBHDLookup.Fields[modOpportunity.strfCONTACT_PROFILE_NBHD_ID].Value;

                    // Load recordset using form
                    IRForm pFormContactNBHDProfile = RSysSystem.Forms[modOpportunity.strrHB_CONTACT_PROFILE_NBHD];
                    object vntParams = null;
                    object vntRecordset = pFormContactNBHDProfile.DoLoadFormData(vntNBHDPId, ref vntParams);
                    object[] recordsetArray = (object[])vntRecordset;
                    Recordset rstCPNBHD = (Recordset)recordsetArray[0];

                    if (rstCPNBHD.RecordCount > 0)
                    {
                        rstCPNBHD.MoveFirst();
                        if (reservationDate != null) rstCPNBHD.Fields[modOpportunity.strfRESERVATION_DATE].Value = reservationDate;
                        if (saleDate != null) rstCPNBHD.Fields[modOpportunity.strfSALE_DATE].Value = saleDate;
                        if ((firstVisitDate != null) && (rstCPNBHD.Fields[modOpportunity.strfFIRST_VISIT_DATE].Value) == DBNull.Value)
                        {
                            // create a visit log
                            rstCPNBHD.Fields[modOpportunity.strfFIRST_VISIT_DATE].Value = firstVisitDate;
                        }
                        if (saleDeclinedDate != null) rstCPNBHD.Fields[modOpportunity.strfSALE_DECLINED_DATE].Value = saleDeclinedDate;
                        if (cancelDeclinedDate != null) rstCPNBHD.Fields[modOpportunity.strfCANCEL_DECLINED_DATE].Value = cancelDeclinedDate;
                        if (cancelRequestDate != null) rstCPNBHD.Fields[modOpportunity.strfCANCEL_REQUEST_DATE].Value = cancelRequestDate;
                        if (cancelDate != null)
                        {
                            rstCPNBHD.Fields[modOpportunity.strfCANCEL_DATE].Value = cancelDate;
                        }
                        if (salesRequestDate != null) rstCPNBHD.Fields[modOpportunity.strfSALES_REQUEST_DATE].Value = salesRequestDate;
                        if (closeDate != null)
                        {
                            rstCPNBHD.Fields[modOpportunity.strfCLOSE_DATE].Value = closeDate;
                        }
                        if (reservationExpirationDate != null) rstCPNBHD.Fields[modOpportunity.strfRESERVATIONEXPIRY].Value = reservationExpirationDate;
                        if (quoteCreateDate == null) rstCPNBHD.Fields[modOpportunity.strfQUOTE_DATE].Value = quoteCreateDate;

                        // Save recordset using form so that the save initiates other functionality
                        pFormContactNBHDProfile.SaveFormData(vntRecordset, ref vntParams);
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#  Date        Author   Description
        /// 3.8.0.0    5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        public virtual void UpdateLotStatusEx(object LotId, string newStatus)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstLot = objLib.GetRecordset(LotId, modOpportunity.strt_PRODUCT, modOpportunity.strfLOT_STATUS);
                if (rstLot.RecordCount > 0)
                {
                    rstLot.MoveFirst();
                    rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = newStatus;
                    objLib.PermissionIgnored = true;
                    objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Updates status of quotes with current date
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#  Date        Author   Description
        /// 3.8.0.0    5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        public virtual void BatchUpdateQuoteStatus()
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // get all in reserved quotes with an expiry date of today or earlier
                Recordset rstQuote = objLib.GetRecordset(modOpportunity.strq_QUOTES_RESERVED_TO_BE_EXPIRED, 0, modOpportunity.strf_ACCOUNT_MANAGER_ID,
                    modOpportunity.strf_STATUS, modOpportunity.strfINACTIVE, modOpportunity.strfLOT_ID, modOpportunity.strf_RN_DESCRIPTOR,
                    modOpportunity.strfOPPORTUNITY_ID);
                if (rstQuote.RecordCount > 0)
                {
                    rstQuote.MoveFirst();
                    while(!(rstQuote.EOF))
                    {
                        object vntQuoteId = rstQuote.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                        UpdateQuoteStatus(vntQuoteId);
                        rstQuote.MoveNext();
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Updates status of quotes based on the reservation expiry date, also updates the Homesite
        /// </summary>
        /// associated with the quote and the Contact NBHD profile as well.
        /// <param name="quoteOpportunityId">Quote current record id</param>
        /// <history>
        /// Revision#  Date        Author   Description
        /// 3.8.0.0    5/12/2006   DYin     Converted to .Net C# code.
        /// 5.9        3/13/2007   JH       Fixed issue 65536-17278.
        /// </history>
        protected virtual void UpdateQuoteStatus(object quoteOpportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstQuote = objLib.GetRecordset(quoteOpportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strf_STATUS,
                    modOpportunity.strfINACTIVE, modOpportunity.strfRESERVATIONEXPIRY, modOpportunity.strfLOT_ID, modOpportunity.strf_RN_DESCRIPTOR,
                    modOpportunity.strfCONTACT_ID, modOpportunity.strfNEIGHBORHOOD_ID);
                if (rstQuote.RecordCount > 0)
                {
                    if ((rstQuote.Fields[modOpportunity.strfRESERVATIONEXPIRY].Value.GetType() == typeof(DateTime)))
                    {
                        object vntLotId = rstQuote.Fields[modOpportunity.strfLOT_ID].Value;

                        object vntContactId = rstQuote.Fields[modOpportunity.strfCONTACT_ID].Value;
                        object neighborhoodId = rstQuote.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;

                        // update the reservation expiration date on the profile
                        UpdateContactProfileNeighborhood(vntContactId, neighborhoodId, null, null, null, null, null, null, null,
                            null, null, rstQuote.Fields[modOpportunity.strfRESERVATIONEXPIRY].Value, null);

                        if ((TypeConvert.ToDateTime(rstQuote.Fields[modOpportunity.strfRESERVATIONEXPIRY].Value) - DateTime.Today).Days <= 0)
                        {
                            // Quote is now inactive, set both the flag and status
                            rstQuote.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsINACTIVE;
                            rstQuote.Fields[modOpportunity.strfINACTIVE].Value = true;
                            objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstQuote);
                                
                                // set homesite to available since the reservation expired
                            if ((vntLotId != DBNull.Value))
                            {
                                Recordset rstLot = objLib.GetRecordset(vntLotId, modOpportunity.strt_PRODUCT, modOpportunity.strfPLAN_ID,
                                    modOpportunity.strfELEVATION_ID, modOpportunity.strfTYPE, modOpportunity.strfLOT_STATUS);
                                if (rstLot.RecordCount > 0)
                                {
                                    rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsAVAILABLE;
                                    // Jun 20 Added by JWang
                                    // dataset ignores permissions when updating the records. Because sales users
                                    // don't have modify permission to the Product(Lot/Homesite) table.
                                    objLib.PermissionIgnored = true;
                                    objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);
                                }
                            }

                        }
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Checks to see if the lot is Available when a user tries to reserve it.
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#  Date        Author   Description
        /// 3.8.0.0    5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        protected virtual bool CheckLotAvailability(object lotId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstLot = objLib.GetRecordset(lotId, modOpportunity.strt_PRODUCT, modOpportunity.strfLOT_PRODUCT_ID,
                    modOpportunity.strfLOT_STATUS);
                if (rstLot.RecordCount > 0)
                    return (TypeConvert.ToString(rstLot.Fields[modOpportunity.strfLOT_STATUS].Value) == modOpportunity.strsAVAILABLE);
                else
                    return false;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Check whether this quote can be copied or not
        /// Rules:  Copy Quote button is available of shows up for:
        /// Quotes (both Active and Inactive) with Pipeline Stage="Quote" AND
        /// Status="In Progress" or "Reservation Expired" AND the Quote's Homesite's
        /// status is "Available" or Quote's Lot is null AND the Neighborhood's
        /// "Lot Required" flag is False (or Null)
        /// </summary>
        /// <returns>True if this quote can be copied
        /// False if this quote cannot be copied</returns>
        /// <history>
        /// Revision#  Date        Author   Description
        /// 3.8.0.0    5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        protected virtual bool CanCopyQuote(string pipelineStage, string quoteStatus, object lotId, 
            object neighborhoodId)
        {
            try
            {
                pipelineStage = pipelineStage.Trim();
                quoteStatus = quoteStatus.Trim();
                if ((pipelineStage == modOpportunity.strsQUOTE) && (quoteStatus == modOpportunity.strsIN_PROGRESS
                    || quoteStatus == modOpportunity.strsRESERVATION_EXPIRED))
                {
                    DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    Recordset rstLot = objLib.GetRecordset(lotId, modOpportunity.strtPRODUCT, modOpportunity.strfLOT_STATUS);
                    if (rstLot.RecordCount > 0)
                    {
                        string strLotStatus = TypeConvert.ToString(rstLot.Fields[modOpportunity.strfLOT_STATUS].Value);

                        if ((strLotStatus.Length == 0) || (strLotStatus == modOpportunity.strsAVAILABLE))
                        {
                            Recordset rstNeighborhood = objLib.GetRecordset(neighborhoodId, modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfLOT_REQD);
                            if (rstNeighborhood.RecordCount > 0)
                            {
                                rstNeighborhood.MoveFirst();
                                return !TypeConvert.ToBoolean(rstNeighborhood.Fields[modOpportunity.strfLOT_REQD].Value);
                            }
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
        /// Checks the system table for the setting wheather greater than or greater than and eqal to
        /// </summary>
        /// <returns>String for construction stage comparison</returns>
        /// <history>
        /// Revision#  Date        Author   Description
        /// 3.8.0.0    5/12/2006   DYin     Converted to .Net C# code.
        /// </history>
        protected virtual string GetConstructionStageComparison()
        {
            try
            {
                SystemSetting systemSetting = (SystemSetting) RSysSystem.ServerScripts[AppServerRuleData.SystemSettingAppServerRuleName].CreateInstance();

                string constructionStageComparison = TypeConvert.ToString(systemSetting.GetSystemSetting(modOpportunity.strfCONSTRUCTION_STAGE_COMPARISON));

                if (constructionStageComparison.Length == 0) constructionStageComparison = "Greater Than";
                return constructionStageComparison;
            }
            catch
            {
                return "Greater Than";
            }
        }

        /// <summary>
        /// This routine will update Homesite's plan based on the plan of quote.
        /// </summary>
        /// <param name="lotId">Lot Id</param>
        /// <param name="newPlanId">New Plan_Id value</param>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/12/2006  DYin   Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateHomesitePlan(object lotId, object newPlanId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstLot = objLib.GetRecordset(lotId, modOpportunity.strt_PRODUCT, modOpportunity.strfPLAN_ID);
                if (rstLot.RecordCount > 0)
                {
                    // May 19 Added by JWang
                    // dataset ignores permissions when updating the records. Because sales users
                    // don't have modify permission to the Product(Lot/Homesite) table.
                    objLib.PermissionIgnored = true;
                    rstLot.Fields[modOpportunity.strfPLAN_ID].Value = newPlanId;
                    objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This routine will update Homesite's Built Elevation with the NBHDP_Product_Id of Opportunity Product
        /// </summary>
        /// <param name="lotId">Lot id</param>
        /// <param name="newNeighborhoodProductId">New NBHDP_Product_Id value</param>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/12/2006  DYin   Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateHomesiteBuiltElevation(object lotId, object newNeighborhoodProductId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstLot = objLib.GetRecordset(lotId, modOpportunity.strt_PRODUCT, modOpportunity.strfELEVATION_ID);
                if (rstLot.RecordCount > 0)
                {
                    objLib.PermissionIgnored = true;
                    rstLot.Fields[modOpportunity.strfELEVATION_ID].Value = newNeighborhoodProductId;
                    objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This routine will update the plan built flag on any other active quote(s) with this Homesite
        /// </summary>
        /// <param name="lotId">Lot Id</param>
        /// <param name="planBuilt">Plan Built flag</param>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/12/2006  DYin   Converted to .Net C# code.
        /// </history>
        protected virtual void UpdatePlanBuiltForActiveCustomerQuote(object lotId, bool planBuilt)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstQuote = objLib.GetRecordset(modOpportunity.strqACTIVE_CUSTOMER_QUOTES_FOR_LOT, 1, lotId, modOpportunity.strfPLAN_BUILT);

                if (rstQuote.RecordCount > 0)
                {
                    rstQuote.MoveFirst();
                    while (!(rstQuote.EOF))
                    {
                        rstQuote.Fields[modOpportunity.strfPLAN_BUILT].Value = planBuilt;
                        rstQuote.MoveNext();
                    }
                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstQuote);

                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// For each option on this quote, find any other options that match this criteria and
        /// call UpdateOptionBuiltForActiveCustomerQuote function to update their build flag as well
        /// </summary>
        /// <param name="optionRecordset">Option secondary recordset in opportunity form</param>
        /// <param name="lotId">Lot Id</param>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/12/2006  DYin   Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateOptionBuilts(Recordset optionRecordset, object lotId)
        {
            try
            {
                if (optionRecordset.RecordCount > 0)
                {
                    optionRecordset.MoveFirst();
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    while (!(optionRecordset.EOF))
                    {
                        Recordset rstOptions = objLib.GetRecordset(optionRecordset.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                            modOpportunity.strt_OPPORTUNITY__PRODUCT, modOpportunity.strf_NBHDP_PRODUCT_ID);
                        UpdateOptionBuiltForActiveCustomerQuote(lotId, rstOptions.Fields[modOpportunity.strf_NBHDP_PRODUCT_ID].Value,
                            TypeConvert.ToBoolean(optionRecordset.Fields[modOpportunity.strfBUILT_OPTION].Value));
                        optionRecordset.MoveNext();
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This routine will finds all the active options (match on NBHD Product) where the opp's homesite is same
        /// as curr op's and update Option Built based on the Option Built of Inventory Quote.
        /// </summary>
        /// <param name="lotId">Lot Id</param>
        /// <param name="neighborhoodPhaseProductId">NPHDP_Product_Id of the option</param>
        /// <param name="optionBuilt">Option Built flag</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/12/2006  DYin   Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateOptionBuiltForActiveCustomerQuote(object lotId, object neighborhoodPhaseProductId, 
            bool optionBuilt)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstQuote = objLib.GetRecordset(modOpportunity.strqOPTIONS_IN_ACTIVE_CUSTOMER_QUOTES_FOR_LOT_NBHDP_PRODUCT,
                    2, lotId, neighborhoodPhaseProductId, modOpportunity.strfBUILT_OPTION);

                if (rstQuote.RecordCount > 0)
                {
                    rstQuote.MoveFirst();
                    while (!(rstQuote.EOF))
                    {
                        rstQuote.Fields[modOpportunity.strfBUILT_OPTION].Value = optionBuilt;
                        rstQuote.MoveNext();
                    }

                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY__PRODUCT, rstQuote);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Creates new deposit records for each selected template
        /// </summary>
        /// <param name="quoteOpportunityId">opportunity ID</param>
        /// <param name="selectedDepositTemplateRecordset">recordset with selected templates</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        public virtual void ApplyDepositScheduleTemplates(object quoteOpportunityId, Recordset selectedDepositTemplateRecordset)
        {
            try
            {
                if (selectedDepositTemplateRecordset != null)
                {
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                    if (selectedDepositTemplateRecordset.RecordCount > 0)
                    {

                        selectedDepositTemplateRecordset.MoveFirst();
                        while(!(selectedDepositTemplateRecordset.EOF))
                        {
                            object vntDepositItemID = selectedDepositTemplateRecordset.Fields[modOpportunity.strfDEPOSIT_SCHEDULE_TEMPLATE_ID].Value;

                            Recordset rstDepositTemplateItem = objLib.GetRecordset(modOpportunity.strqDEPOSIT_SCHED_TEMPL_ITEMS,
                                1, vntDepositItemID, modOpportunity.strfDEP_TEMPL_ITM_DEPOSIT_TYPE, modOpportunity.strfDEP_TEMPL_ITM_DEPOSIT_AMOUNT,
                                modOpportunity.strfDEP_TEMPL_ITM_METHOD_OF_PAYMENT, modOpportunity.strfDEP_TEMPL_ITM_NOTES,
                                modOpportunity.strfDEP_TEMPL_ITM_REFUNDABLE, modOpportunity.strfOFFSET_APPLY_DATE);

                            if (rstDepositTemplateItem.RecordCount > 0)
                            {
                                rstDepositTemplateItem.MoveFirst();

                                while(!(rstDepositTemplateItem.EOF))
                                {

                                    Recordset rstDeposit = objLib.GetNewRecordset(modOpportunity.strtDEPOSIT, modOpportunity.strfDEPOSIT_OPPORTUNITY_ID,
                                        modOpportunity.strfDEPOSIT_ID, modOpportunity.strfDEPOSIT_TYPE, modOpportunity.strfDEPOSIT_AMOUNT,
                                        modOpportunity.strfDEPOSIT_METHOD_OF_PAYMENT, modOpportunity.strfDEPOSIT_NOTES,
                                        modOpportunity.strfDEPOSIT_REFUNDABLE, modOpportunity.strfSCHEDULED_DATE);

                                    rstDeposit.AddNew(Type.Missing, Type.Missing);

                                    rstDeposit.Fields[modOpportunity.strfDEPOSIT_OPPORTUNITY_ID].Value = quoteOpportunityId;
                                    rstDeposit.Fields[modOpportunity.strfDEPOSIT_TYPE].Value = rstDepositTemplateItem.Fields[modOpportunity.strfDEP_TEMPL_ITM_DEPOSIT_TYPE].Value;
                                    rstDeposit.Fields[modOpportunity.strfDEPOSIT_AMOUNT].Value = rstDepositTemplateItem.Fields[modOpportunity.strfDEP_TEMPL_ITM_DEPOSIT_AMOUNT].Value;
                                    rstDeposit.Fields[modOpportunity.strfDEPOSIT_METHOD_OF_PAYMENT].Value = rstDepositTemplateItem.Fields[modOpportunity.strfDEP_TEMPL_ITM_METHOD_OF_PAYMENT].Value;
                                    rstDeposit.Fields[modOpportunity.strfDEPOSIT_NOTES].Value = rstDepositTemplateItem.Fields[modOpportunity.strfDEP_TEMPL_ITM_NOTES].Value;
                                    rstDeposit.Fields[modOpportunity.strfDEPOSIT_REFUNDABLE].Value = rstDepositTemplateItem.Fields[modOpportunity.strfDEP_TEMPL_ITM_REFUNDABLE].Value;
                                    if (TypeConvert.ToDouble(rstDepositTemplateItem.Fields[modOpportunity.strfOFFSET_APPLY_DATE].Value)
                                        >= 0.0)
                                    {
                                        rstDeposit.Fields[modOpportunity.strfSCHEDULED_DATE].Value = DateTime.Today
                                            .AddDays(TypeConvert.ToInt32(rstDepositTemplateItem.Fields[modOpportunity.strfOFFSET_APPLY_DATE].Value));
                                    }
                                    objLib.SaveRecordset(modOpportunity.strtDEPOSIT, rstDeposit);
                                    rstDepositTemplateItem.MoveNext();
                                }
                            }
                            selectedDepositTemplateRecordset.MoveNext();
                        }

                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Cancels Request/Contract and sends notification email
        /// </summary>
        /// <param name="opportunityId">opportunity ID</param>
        /// <param name="cancelContractApproval">Flag to indicate if cancel contract is approval</param>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        ///                                         that crashed the system , if a construction stage was set
        /// 5.9.0           11/23/10    KA          Update email subject and msg with more custom info                                             
        
        public virtual void CancelRequestOrContract(object opportunityId, bool cancelContractApproval)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                ILangDict ldOpportunity = RSysSystem.GetLDGroup(modOpportunity.strgOPPORTUNITY);

                // update status
                Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strf_STATUS,
                    modOpportunity.strfCANCEL_REQUEST_DATE, modOpportunity.strfQUOTE_TOTAL, modOpportunity.strfACTUAL_DECISION_DATE,
                    modOpportunity.strfCONTACT_ID, modOpportunity.strf_ACCOUNT_MANAGER_ID, modOpportunity.strfNEIGHBORHOOD_ID,
                    modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfELEVATION_ID, modOpportunity.strfLOT_ID, modOpportunity.strfECOE_DATE,
                    modOpportunity.strf_RN_DESCRIPTOR, modOpportunity.strfPIPELINE_STAGE, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                    modOpportunity.strfCANCEL_DATE, modOpportunity.strfCANCEL_DECLINED_DATE, modOpportunity.strfCANCEL_DECLINED_By,
                    modOpportunity.strfCANCEL_APPROVED_BY, modOpportunity.strfCANCEL_NOTES, modOpportunity.strfPLAN_BUILT);

                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                object parameterList = objParam.Construct();
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                object vntLotId = DBNull.Value;
                object vntNeighborhoodId = DBNull.Value;
                object vntContactId = DBNull.Value;
                object vntCurrentEmployeeId = DBNull.Value;
                string vntCurrentEmployeeFirstName = string.Empty;
                string vntCurrentEmployeeLastName = string.Empty;
                string strCurrentEmployeeName = string.Empty;
                if (rstOpportunity.RecordCount > 0)
                {
                    rstOpportunity.MoveFirst();
                    vntLotId = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                    vntNeighborhoodId = rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                    vntContactId = rstOpportunity.Fields[modOpportunity.strfCONTACT_ID].Value;

                    // Get Current Employee full name
                    vntCurrentEmployeeId = administration.CurrentUserRecordId;
                    vntCurrentEmployeeFirstName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_FIRST_NAME,
                        vntCurrentEmployeeId));
                    vntCurrentEmployeeLastName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_LAST_NAME,
                        vntCurrentEmployeeId));
                    strCurrentEmployeeName = vntCurrentEmployeeFirstName + " " + vntCurrentEmployeeLastName;

                    if (cancelContractApproval)
                    {
                        // Cancel Approved
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_REQUEST_DATE].Value = DateTime.Today;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_DATE].Value = DateTime.Today;
                        rstOpportunity.Fields[modOpportunity.strfPIPELINE_STAGE].Value = modOpportunity.strsCANCELLED;
                        rstOpportunity.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsCANCELLED;

                        object vntCurrentUserId = RSysSystem.CurrentUserId();
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_APPROVED_BY].Value = administration.CurrentUserRecordId;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_DECLINED_DATE].Value = DBNull.Value;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_DECLINED_By].Value = DBNull.Value;

                        string str = TypeConvert.ToString(ldOpportunity.GetTextSub(modOpportunity.strlAPPROVED_CANCEL, new
                            object[] {DateTime.Today, strCurrentEmployeeName}));
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value = TypeConvert.ToString(Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value) ? "" : TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value) + "\r\n") + str;

                        // update NBHD Profile
                        UpdateContactProfileNeighborhood(vntContactId, vntNeighborhoodId, null, null, null, null, null,
                            null, DateTime.Today, DateTime.Today, null, null, null);

                        //WRITE HISTORY RECORDS ON CANCELLATION
                        WriteContractHistoryRecords(vntLotId, opportunityId, modOpportunity.strsCANCELLED, DateTime.Today, false, null, false, false);

                        // Jul 27, 2005. by JWang. If Cancel a contract, go inactivate all in progress Post Sale Quotes.
                        parameterList = objParam.SetUserDefinedParameter(1, opportunityId);
                        // RY: Modified method call to inactivate all PSQ instead of only in progress ones.
                        RSysSystem.Forms[modOpportunity.strrHB_POST_SALE_QUOTE].Execute(modOpportunity.strmINACTIVATE_ALL_PSQ,
                            ref parameterList);

                        //Inactive all esccrow records for cancelled contract
                        InactivateCancelledEscrow(opportunityId);


                    }
                    else
                    {
                        // Cancel Request
                        rstOpportunity.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsCANCEL_REQUEST;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_REQUEST_DATE].Value = DateTime.Today;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value = TypeConvert.ToString(Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value) ? "" : TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value) + "\r\n") + "Cancellation Requested on " + DateTime.Today.ToShortDateString() + " by " + strCurrentEmployeeName;// update NBHD Profile Cancel Request DateUpdateContactProfileNBHD(vntContactId, vntNeighborhoodId, null, null, null, null, null, DateTime.Today, null, null, null, null, null);

                        // update NBHD Profile - Update the Cancel Request Date
                        UpdateContactProfileNeighborhood(vntContactId, vntNeighborhoodId, null, null, null, null, null,
                            DateTime.Today, null, null, null, null, null);

                    }

                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstOpportunity);

                    UpdateCoBuyerStatus(opportunityId, false, false);
                    // AV Contact profile NBHD gets updated, if cancelling and there is a sales request or reserved
                    // quote then
                    // update the type to prospect
                    if (cancelContractApproval)
                    {
                        // UpdateContactType vntContactId, strsCANCELLED
                        Recordset rstCntNBHDProfile = objLib.GetRecordset(modOpportunity.strqCONTACT_PROFILE_NBHD_FOR_CONTACT,
                            2, vntContactId, vntNeighborhoodId, modOpportunity.strfCONTACT_PROFILE_NBHD_ID);
                        if (!(rstCntNBHDProfile.EOF))
                        {
                            // update contact profile neighborhood type
                            rstCntNBHDProfile.MoveFirst();
                            object vntCntNBHDProfileID = rstCntNBHDProfile.Fields[modOpportunity.strfCONTACT_PROFILE_NBHD_ID].Value;
                            ContactProfileNeighborhood objContactProfileNBHD = (ContactProfileNeighborhood)RSysSystem.ServerScripts[modOpportunity.strsCONTACT_PROFILE_NBHD].CreateInstance();

                            objContactProfileNBHD.UpdateNBHDPType(vntCntNBHDProfileID);

                            // if the type was set to 'prospect' and there are no sales request/ or reserved qutoes
                            // then cancelation takes priority therefore type of the profile is 'cancelled'
                            //rstCntNBHDProfile = objLib.GetRecordset(vntCntNBHDProfileID, modOpportunity.strtCONTACT_PROFILE_NEIGHBORHOOD,
                            //    modOpportunity.strfTYPE);
                            //if (!(rstCntNBHDProfile.EOF))
                            //{
                            //    rstCntNBHDProfile.MoveFirst();
                            //    if (TypeConvert.ToString(rstCntNBHDProfile.Fields[modOpportunity.strfTYPE].Value) == "Prospect")
                            //    {
                            //        // any in progress sales requests or reserved quotes?
                            //        Recordset rstSalesReqResQ = objLib.GetRecordset(modOpportunity.strqRESERVED_OR_SALES_REQUEST_QUOTES,
                            //            2, vntNeighborhoodId, vntContactId, modOpportunity.strfSTATUS);
                            //        if (rstSalesReqResQ.EOF)
                            //        {
                            //            // no sales request or reserved quote exist for this contact in the NBHD
                            //            rstCntNBHDProfile.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsCANCELLED;
                            //            objLib.SaveRecordset(modOpportunity.strtCONTACT_PROFILE_NEIGHBORHOOD, rstCntNBHDProfile);
                            //        }

                            //    }
                            //}
                        }
                    }

                }
                else
                {
                    // we haven't found oportunity, it must be some error
                    return ;
                }

                // for cancel approvals or cancel contracts do the rest
                if (cancelContractApproval)
                {
                    UpdateCoBuyerStatus(opportunityId, false, true); // remove the co-buyers from the lot
                     // Update homesite status
                    if ((vntLotId != DBNull.Value))
                    {
                        // Set rstOpportunity = objLib.GetRecordset
                        Recordset rstLot = objLib.GetRecordset(vntLotId, modOpportunity.strt_PRODUCT, modOpportunity.strfPLAN_ID,
                            modOpportunity.strfELEVATION_ID, modOpportunity.strfTYPE, modOpportunity.strfLOT_STATUS,
                            modOpportunity.strfSALES_DATE, modOpportunity.strfOWNER_ID, modOpportunity.strfPRODUCT_ID,
                            modOpportunity.strfOWNER_NAME, modOpportunity.strfTYPE, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                            modOpportunity.strfPLAN_ID, modOpportunity.strfELEVATION_ID, modOpportunity.strfRESERVATION_CONTRACT_ID,
                            modOpportunity.strfRESERVED_DATE, modOpportunity.strfTIC_CO_BUYER_ID);

                        object vntPrevOwner = DBNull.Value;
                        object vntProductID = DBNull.Value;
                        if (rstLot.RecordCount > 0)
                        {
                            rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsAVAILABLE;
                            rstLot.Fields[modOpportunity.strfSALES_DATE].Value = DBNull.Value;
                            rstLot.Fields[modOpportunity.strfRESERVED_DATE].Value = DBNull.Value;
                            vntProductID = rstLot.Fields[modOpportunity.strfPRODUCT_ID].Value;
                            vntPrevOwner = rstLot.Fields[modOpportunity.strfOWNER_ID].Value;

                            rstLot.Fields[modOpportunity.strfOWNER_ID].Value = DBNull.Value;
                            //Cmigles - Sept 24/2010 - Clear co-buyes on lot as well.
                            rstLot.Fields[modOpportunity.strfTIC_CO_BUYER_ID].Value = DBNull.Value;
                            rstLot.Fields[modOpportunity.strfOWNER_NAME].Value = DBNull.Value;
                            rstLot.Fields[modOpportunity.strfRESERVATION_CONTRACT_ID].Value = DBNull.Value;
                            if ((!(Convert.IsDBNull(rstLot.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value))
                                || (!(Convert.IsDBNull(rstLot.Fields[modOpportunity.strfPLAN_ID].Value)))) 
                                && (TypeConvert.ToString(rstLot.Fields[modOpportunity.strfTYPE].Value))
                                != modOpportunity.strsINVENTORY)
                            {
                                // this lot is under contstruction or the plan is build
                                rstLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsINVENTORY;
                            }
                            // if the plan built flag is set then set it on this quote
                            if (TypeConvert.ToBoolean(rstOpportunity.Fields[modOpportunity.strfPLAN_BUILT].Value))
                            {
                                rstLot.Fields[modOpportunity.strfPLAN_ID].Value = rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value;
                            }
                            // if the elevation is set then set it on the quote
                            // make sure an inventory quote doesn't already exist
                            Recordset rstInvQuote = objLib.GetRecordset(modOpportunity.strqINVENTORY_QUOTE_FOR_LOT, 1, vntLotId,
                                modOpportunity.strfOPPORTUNITY_ID);

                            // based on the previous statements, the check for not null construction stage and type
                            // = Inventory,
                            // it's better to make the entire check in case something changes later.
                            //if (rstInvQuote.RecordCount == 0 && (!(Convert.IsDBNull(rstLot.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value))
                            //    || TypeConvert.ToBoolean(rstOpportunity.Fields[modOpportunity.strfPLAN_BUILT].Value)) &&
                            //    TypeConvert.ToString(rstLot.Fields[modOpportunity.strfTYPE].Value) == modOpportunity.strsINVENTORY)
                            if (rstInvQuote.RecordCount == 0)
                            {
                                // create an inventory quote from the contract
                                object newOpportunityId = CreateInventoryQuoteFromContract(opportunityId);
                                                               
                                //AM2011.01.05 - Ensure that preploted options are carried over
                                //to the inventory quote
                                SetOptionsPreplotFlag(newOpportunityId);
                                //AM2011.03.03 - Combine pre-plots and non-preplots on cancels
                                CombinePrePlotAndNonPrePlotsForSameOption(newOpportunityId);
                                CalculateTotals(newOpportunityId, false);
                            }

                            objLib.PermissionIgnored = true;
                            objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);
                        }

                        // add lot contact
                        Recordset rstNewLotContact = objLib.GetNewRecordset(modOpportunity.strtLOT__CONTACT, modOpportunity.strfPRODUCT_ID,
                            modOpportunity.strf_CONTACT_ID, modOpportunity.strfTYPE);
                        rstNewLotContact.AddNew(Type.Missing, Type.Missing);
                        rstNewLotContact.Fields[modOpportunity.strf_CONTACT_ID].Value = vntPrevOwner;
                        rstNewLotContact.Fields[modOpportunity.strfPRODUCT_ID].Value = vntProductID;
                        rstNewLotContact.Fields[modOpportunity.strfTYPE].Value = 0;

                        objLib.SaveRecordset(modOpportunity.strtLOT__CONTACT, rstNewLotContact);
                    }

                    // May 30, 2005. Added By JWang
                    // Inactive Unbuilt Lot Configurations
                    parameterList = objParam.SetUserDefinedParameter(1, vntLotId);
                    RSysSystem.Forms[modOpportunity.strrLOT_CONFIGURATION].Execute(modOpportunity.strmINACTIVATE_UNBUILT_LOT_CONFIGURATIONS,
                        ref parameterList);
                }

                // send email
                string strNotify = string.Empty;
                string strSubject = string.Empty;
                string strMsg1 = string.Empty;
                string strMsg2 = string.Empty;
                string strMsg3 = string.Empty;
                if (cancelContractApproval)
                {
                    strNotify = modOpportunity.strqNOTIFICATION_ON_CANCEL_APPROVAL;
                    strSubject = modOpportunity.strdCANCEL_APPROVED_SUBJECT;
                    strMsg1 = modOpportunity.strdCANCEL_APPROVED_MESSAGE1;
                    strMsg2 = modOpportunity.strdCANCEL_APPROVED_MESSAGE2;
                    strMsg3 = modOpportunity.strdCANCEL_APPROVED_MESSAGE3;
                }
                else
                {
                    strNotify = modOpportunity.strqNOTIFICATION_ON_CANCEL_REQUEST;
                    strSubject = modOpportunity.strdCANCEL_REQUEST_SUBJECT;
                    strMsg1 = modOpportunity.strdCANCEL_REQUEST_MESSAGE1;
                    strMsg2 = modOpportunity.strdCANCEL_REQUEST_MESSAGE2;
                    strMsg3 = modOpportunity.strdCANCEL_REQUEST_MESSAGE3;
                }
                // get recepient list from neighborhood notifcation team where notify on Sales Approved is true
                Recordset rstEmailTo = objLib.GetRecordset(strNotify, 1, vntNeighborhoodId, modOpportunity.strf_EMPLOYEE_ID);
                string strEmailTo = string.Empty;
                if (rstEmailTo.RecordCount > 0)
                {
                    rstEmailTo.MoveFirst();
                    StringBuilder emailToBuilder = new StringBuilder();
                    while(!(rstEmailTo.EOF))
                    {
                        string strWorkEmail = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtEMPLOYEE, 
                            modOpportunity.strf_WORK_EMAIL, rstEmailTo.Fields[modOpportunity.strf_EMPLOYEE_ID].Value));
                        // add if not already there
                        if (!emailToBuilder.ToString().Contains(strWorkEmail))
                        {
                            emailToBuilder.Append(strWorkEmail + ";");
                        }
                        rstEmailTo.MoveNext();
                    }
                    // strip out last ;
                    strEmailTo = emailToBuilder.ToString();
                    strEmailTo = strEmailTo.Substring(0, strEmailTo.Length - 1);
                }
                rstEmailTo.Close();

                if (strEmailTo.Trim().Length == 0)
                {
                    return ;
                }
                // all language strings are in nbhd_notification_team
                ILangDict lngNBHD_Notification_Team = RSysSystem.GetLDGroup(modOpportunity.strgNBHD_NOTIFICATION_TEAM);
                // set subject
                vntCurrentEmployeeId = administration.CurrentUserRecordId;
                vntCurrentEmployeeFirstName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_FIRST_NAME,
                    vntCurrentEmployeeId));
                vntCurrentEmployeeLastName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_LAST_NAME,
                    vntCurrentEmployeeId));
                // TODO (DI Yin) strLotDescriptor is never assigned. Temporary code here 
               
                //AM2010.11.17 - Email notification changes for cancelled contract
                // TODO (DI Yin) strLotDescriptor is never assigned. Temporary code here 
                string strLotDescriptor = string.Empty;
                string strMessage = string.Empty;

                //AM2010.10.14 - Get Neighborhood, Division and Lot for the strLot Descriptor
                string strNeighborhood = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfNAME, vntNeighborhoodId));
                object vntDivisionId = objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfDIVISION_ID, vntNeighborhoodId);
                string strDivision = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtDIVISION, modOpportunity.strfNAME, vntDivisionId));
                //object vntLotId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strfLOT_ID, rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value);
                Recordset rstLotRef = objLib.GetRecordset(vntLotId, modOpportunity.strtPRODUCT, modOpportunity.strfUNIT, modOpportunity.strfTRACT, modOpportunity.strfLOT_NUMBER);
                string strLot = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfLOT_NUMBER].Value);
                string strUnit = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfUNIT].Value);
                string strTract = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfTRACT].Value);
                strLotDescriptor = strDivision + ", " + strNeighborhood + ", T/" + strTract + " L/" + strLot + " U/" + strUnit;

                //commented out building of subject & msg from LD String and replaced it with new subject/msg 
                //strSubject = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strSubject, 
                //    new object[] {vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, strLotDescriptor,
                //    String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)) }));
                // set message
                strMessage = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strMsg1, 
                    new object[] {DateTime.Today, vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_FIRST_NAME, vntContactId)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_LAST_NAME, vntContactId)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strfHOME_PHONE, vntContactId)), 
                        String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)),
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                        rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                        rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value)) }));

                // TODO (Di Yin) vntJob_Number is never assigned, temporary code here
                int vntJob_Number = 0;
                strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strMsg2, 
                    new object[] {vntJob_Number, TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfECOE_DATE].Value)}));
                strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strMsg3, 
                    new object[] {vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_WORK_EMAIL, vntCurrentEmployeeId)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strfWORK_PHONE, vntCurrentEmployeeId))}));
                //KA 11/24/10 adding village/nbdh/tract/lot/unit info infront of the message var
                strMessage = strLotDescriptor + "\n\n" + strMessage;

                strSubject = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strSubject,
                    new object[] {vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, strLotDescriptor,
                    String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)) }));
                SendSimpleMail(strEmailTo, strSubject, strMessage);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }



        /// <summary>
        /// Cancels Reservation, needed to ensure that added any new parameters wouldn't break any
        /// out of the box code and sends notification email
        /// </summary>
        /// <param name="opportunityId">opportunity ID</param>
        /// <param name="cancelContractApproval">Flag to indicate if cancel contract is approval</param>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// 5.9.0          11/24/2010    Kevin Auh  Added code to make cancelled reservation note and added village/project/tract/lot/unit to email 
        /// that crashed the system , if a construction stage was set
        /// </history>
        public virtual void CancelReservation(object opportunityId, bool cancelContractApproval)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                ILangDict ldOpportunity = RSysSystem.GetLDGroup(modOpportunity.strgOPPORTUNITY);

                // update status
                Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strf_STATUS,
                    modOpportunity.strfCANCEL_REQUEST_DATE, modOpportunity.strfQUOTE_TOTAL, modOpportunity.strfACTUAL_DECISION_DATE,
                    modOpportunity.strfCONTACT_ID, modOpportunity.strf_ACCOUNT_MANAGER_ID, modOpportunity.strfNEIGHBORHOOD_ID,
                    modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfELEVATION_ID, modOpportunity.strfLOT_ID, modOpportunity.strfECOE_DATE,
                    modOpportunity.strf_RN_DESCRIPTOR, modOpportunity.strfPIPELINE_STAGE, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                    modOpportunity.strfCANCEL_DATE, modOpportunity.strfCANCEL_DECLINED_DATE, modOpportunity.strfCANCEL_DECLINED_By,
                    modOpportunity.strfCANCEL_APPROVED_BY, modOpportunity.strfCANCEL_NOTES, modOpportunity.strfPLAN_BUILT);

                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                object parameterList = objParam.Construct();
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                object vntLotId = DBNull.Value;
                object vntNeighborhoodId = DBNull.Value;
                object vntContactId = DBNull.Value;
                object vntCurrentEmployeeId = DBNull.Value;
                string vntCurrentEmployeeFirstName = string.Empty;
                string vntCurrentEmployeeLastName = string.Empty;
                string strCurrentEmployeeName = string.Empty;
                if (rstOpportunity.RecordCount > 0)
                {
                    rstOpportunity.MoveFirst();
                    vntLotId = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                    vntNeighborhoodId = rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                    vntContactId = rstOpportunity.Fields[modOpportunity.strfCONTACT_ID].Value;

                    // Get Current Employee full name
                    vntCurrentEmployeeId = administration.CurrentUserRecordId;
                    vntCurrentEmployeeFirstName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_FIRST_NAME,
                        vntCurrentEmployeeId));
                    vntCurrentEmployeeLastName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_LAST_NAME,
                        vntCurrentEmployeeId));
                    strCurrentEmployeeName = vntCurrentEmployeeFirstName + " " + vntCurrentEmployeeLastName;

                    if (cancelContractApproval)
                    {
                        // Cancel Approved
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_REQUEST_DATE].Value = DateTime.Today;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_DATE].Value = DateTime.Today;
                        rstOpportunity.Fields[modOpportunity.strfPIPELINE_STAGE].Value = modOpportunity.strsCANCELLED;
                        rstOpportunity.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsCANCELLED;

                        object vntCurrentUserId = RSysSystem.CurrentUserId();
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_APPROVED_BY].Value = administration.CurrentUserRecordId;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_DECLINED_DATE].Value = DBNull.Value;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_DECLINED_By].Value = DBNull.Value;
                        //KA 11/24/10 update so that "cancelled reservation shows up rather than just cancelled
                        //string str = TypeConvert.ToString(ldOpportunity.GetTextSub(modOpportunity.strlAPPROVED_CANCEL, new
                        //    object[] { DateTime.Today, strCurrentEmployeeName }));
                        string str = "Cancelled Reservation Approved on " + DateTime.Today.ToShortDateString() + " by " + strCurrentEmployeeName ;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value = TypeConvert.ToString(Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value) ? "" : TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value) + "\r\n") + str;

                        // update NBHD Profile
                        UpdateContactProfileNeighborhood(vntContactId, vntNeighborhoodId, null, null, null, null, null,
                            null, DateTime.Today, DateTime.Today, null, null, null);

                        //WRITE HISTORY RECORDS ON CANCELLATION OF RESERVE
                        WriteContractHistoryRecords(vntLotId, opportunityId, modOpportunity.strsCANCELLED_RESERVED, DateTime.Today, false, null, false, false);

                        // Jul 27, 2005. by JWang. If Cancel a contract, go inactivate all in progress Post Sale Quotes.
                        parameterList = objParam.SetUserDefinedParameter(1, opportunityId);
                        // RY: Modified method call to inactivate all PSQ instead of only in progress ones.
                        RSysSystem.Forms[modOpportunity.strrHB_POST_SALE_QUOTE].Execute(modOpportunity.strmINACTIVATE_ALL_PSQ,
                            ref parameterList);
                    }
                    else
                    {
                        // Cancel Request
                        rstOpportunity.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsCANCEL_REQUEST;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_REQUEST_DATE].Value = DateTime.Today;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value = TypeConvert.ToString(Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value) ? "" : TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value) + "\r\n") + "Cancelled Reservation Requested on " + DateTime.Today.ToShortDateString() + " by " + strCurrentEmployeeName;// update NBHD Profile Cancel Request DateUpdateContactProfileNBHD(vntContactId, vntNeighborhoodId, null, null, null, null, null, DateTime.Today, null, null, null, null, null);

                        // update NBHD Profile - Update the Cancel Request Date
                        UpdateContactProfileNeighborhood(vntContactId, vntNeighborhoodId, null, null, null, null, null,
                            DateTime.Today, null, null, null, null, null);

                    }

                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstOpportunity);

                    UpdateCoBuyerStatus(opportunityId, false, false);
                    // AV Contact profile NBHD gets updated, if cancelling and there is a sales request or reserved
                    // quote then
                    // update the type to prospect
                    if (cancelContractApproval)
                    {
                        // UpdateContactType vntContactId, strsCANCELLED
                        Recordset rstCntNBHDProfile = objLib.GetRecordset(modOpportunity.strqCONTACT_PROFILE_NBHD_FOR_CONTACT,
                            2, vntContactId, vntNeighborhoodId, modOpportunity.strfCONTACT_PROFILE_NBHD_ID);
                        if (!(rstCntNBHDProfile.EOF))
                        {
                            // update contact profile neighborhood type
                            rstCntNBHDProfile.MoveFirst();
                            object vntCntNBHDProfileID = rstCntNBHDProfile.Fields[modOpportunity.strfCONTACT_PROFILE_NBHD_ID].Value;
                            ContactProfileNeighborhood objContactProfileNBHD = (ContactProfileNeighborhood)RSysSystem.ServerScripts[modOpportunity.strsCONTACT_PROFILE_NBHD].CreateInstance();

                            objContactProfileNBHD.UpdateNBHDPType(vntCntNBHDProfileID);

                                                        // if the type was set to 'prospect' and there are no sales request/ or reserved qutoes
                            // then cancelation takes priority therefore type of the profile is 'cancelled'
                            //rstCntNBHDProfile = objLib.GetRecordset(vntCntNBHDProfileID, modOpportunity.strtCONTACT_PROFILE_NEIGHBORHOOD,
                            //    modOpportunity.strfTYPE);
                            //if (!(rstCntNBHDProfile.EOF))
                            //{
                            //    rstCntNBHDProfile.MoveFirst();
                            //    if (TypeConvert.ToString(rstCntNBHDProfile.Fields[modOpportunity.strfTYPE].Value) == "Prospect")
                            //    {
                            //        // any in progress sales requests or reserved quotes?
                            //        Recordset rstSalesReqResQ = objLib.GetRecordset(modOpportunity.strqRESERVED_OR_SALES_REQUEST_QUOTES,
                            //            2, vntNeighborhoodId, vntContactId, modOpportunity.strfSTATUS);
                            //        if (rstSalesReqResQ.EOF)
                            //        {
                            //            // no sales request or reserved quote exist for this contact in the NBHD
                            //            rstCntNBHDProfile.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsCANCELLED;
                            //            objLib.SaveRecordset(modOpportunity.strtCONTACT_PROFILE_NEIGHBORHOOD, rstCntNBHDProfile);
                            //        }

                            //    }
                            //}
                        }
                    }

                }
                else
                {
                    // we haven't found oportunity, it must be some error
                    return;
                }

                // for cancel approvals or cancel contracts do the rest
                if (cancelContractApproval)
                {
                    UpdateCoBuyerStatus(opportunityId, false, true); // remove the co-buyers from the lot
                    // Update homesite status
                    if ((vntLotId != DBNull.Value))
                    {
                        // Set rstOpportunity = objLib.GetRecordset
                        Recordset rstLot = objLib.GetRecordset(vntLotId, modOpportunity.strt_PRODUCT, modOpportunity.strfPLAN_ID,
                            modOpportunity.strfELEVATION_ID, modOpportunity.strfTYPE, modOpportunity.strfLOT_STATUS,
                            modOpportunity.strfSALES_DATE, modOpportunity.strfOWNER_ID, modOpportunity.strfPRODUCT_ID,
                            modOpportunity.strfOWNER_NAME, modOpportunity.strfTYPE, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                            modOpportunity.strfPLAN_ID, modOpportunity.strfELEVATION_ID, modOpportunity.strfRESERVATION_CONTRACT_ID,
                            modOpportunity.strfRESERVED_DATE, modOpportunity.strfTIC_CO_BUYER_ID, modOpportunity.strfNEIGHBORHOOD_ID);

                        object vntPrevOwner = DBNull.Value;
                        object vntProductID = DBNull.Value;
                        if (rstLot.RecordCount > 0)
                        {
                            rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsAVAILABLE;
                            rstLot.Fields[modOpportunity.strfSALES_DATE].Value = DBNull.Value;
                            rstLot.Fields[modOpportunity.strfRESERVED_DATE].Value = DBNull.Value;
                            vntProductID = rstLot.Fields[modOpportunity.strfPRODUCT_ID].Value;
                            vntPrevOwner = rstLot.Fields[modOpportunity.strfOWNER_ID].Value;

                            rstLot.Fields[modOpportunity.strfOWNER_ID].Value = DBNull.Value;
                            //Cmigles - Sept 24/2010 - Clear co-buyes on lot as well.
                            rstLot.Fields[modOpportunity.strfTIC_CO_BUYER_ID].Value = DBNull.Value;
                            rstLot.Fields[modOpportunity.strfOWNER_NAME].Value = DBNull.Value;
                            rstLot.Fields[modOpportunity.strfRESERVATION_CONTRACT_ID].Value = DBNull.Value;
                            if ((!(Convert.IsDBNull(rstLot.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value))
                                || (!(Convert.IsDBNull(rstLot.Fields[modOpportunity.strfPLAN_ID].Value))))
                                && (TypeConvert.ToString(rstLot.Fields[modOpportunity.strfTYPE].Value))
                                != modOpportunity.strsINVENTORY)
                            {
                                // this lot is under contstruction or the plan is build
                                rstLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsINVENTORY;
                            }
                            // if the plan built flag is set then set it on this quote
                            if (TypeConvert.ToBoolean(rstOpportunity.Fields[modOpportunity.strfPLAN_BUILT].Value))
                            {
                                rstLot.Fields[modOpportunity.strfPLAN_ID].Value = rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value;
                            }
                            // if the elevation is set then set it on the quote
                            // make sure an inventory quote doesn't already exist
                            Recordset rstInvQuote = objLib.GetRecordset(modOpportunity.strqINVENTORY_QUOTE_FOR_LOT, 1, vntLotId,
                                modOpportunity.strfOPPORTUNITY_ID);

                            // based on the previous statements, the check for not null construction stage and type
                            // = Inventory,
                            // it's better to make the entire check in case something changes later.
                            //if (rstInvQuote.RecordCount == 0 && (!(Convert.IsDBNull(rstLot.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value))
                            //    || TypeConvert.ToBoolean(rstOpportunity.Fields[modOpportunity.strfPLAN_BUILT].Value)) &&
                            //    TypeConvert.ToString(rstLot.Fields[modOpportunity.strfTYPE].Value) == modOpportunity.strsINVENTORY)
                            if (rstInvQuote.RecordCount == 0)
                            {
                                // create an inventory quote from the contract
                                object newOpportunityId = CreateInventoryQuoteFromContract(opportunityId);

                                //AM2011.01.05 - Ensure that preploted options are carried over
                                //to the inventory quote
                                SetOptionsPreplotFlag(newOpportunityId);
                                //AM2011.03.03 - Combine pre-plots and non-preplots on cancels
                                CombinePrePlotAndNonPrePlotsForSameOption(newOpportunityId);

                                CalculateTotals(newOpportunityId, false);
                            }

                            objLib.PermissionIgnored = true;
                            objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);
                        }

                        // add lot contact
                        Recordset rstNewLotContact = objLib.GetNewRecordset(modOpportunity.strtLOT__CONTACT, modOpportunity.strfPRODUCT_ID,
                            modOpportunity.strf_CONTACT_ID, modOpportunity.strfTYPE);
                        rstNewLotContact.AddNew(Type.Missing, Type.Missing);
                        rstNewLotContact.Fields[modOpportunity.strf_CONTACT_ID].Value = vntPrevOwner;
                        rstNewLotContact.Fields[modOpportunity.strfPRODUCT_ID].Value = vntProductID;
                        rstNewLotContact.Fields[modOpportunity.strfTYPE].Value = 0;

                        objLib.SaveRecordset(modOpportunity.strtLOT__CONTACT, rstNewLotContact);
                    }

                    // May 30, 2005. Added By JWang
                    // Inactive Unbuilt Lot Configurations
                    parameterList = objParam.SetUserDefinedParameter(1, vntLotId);
                    RSysSystem.Forms[modOpportunity.strrLOT_CONFIGURATION].Execute(modOpportunity.strmINACTIVATE_UNBUILT_LOT_CONFIGURATIONS,
                        ref parameterList);

                    //AM2010.11.20 - inactive esrow records for cancelled contract
                    InactivateCancelledEscrow(opportunityId);

                }

                // send email
                string strNotify = string.Empty;
                string strSubject = string.Empty;
                string strMsg1 = string.Empty;
                string strMsg2 = string.Empty;
                string strMsg3 = string.Empty;
                if (cancelContractApproval)
                {
                    strNotify = modOpportunity.strqNOTIFICATION_ON_CANCEL_APPROVAL;
                    //KA 11/24/10 redone so it doesn't use the ld string
                    //strSubject = modOpportunity.strdCANCEL_APPROVED_SUBJECT;
                    strSubject = "Cancel Reservation Approved ";
                    strMsg1 = modOpportunity.strdCANCEL_APPROVED_MESSAGE1;
                    strMsg2 = modOpportunity.strdCANCEL_APPROVED_MESSAGE2;
                    strMsg3 = modOpportunity.strdCANCEL_APPROVED_MESSAGE3;
                }
                else
                {
                    strNotify = modOpportunity.strqNOTIFICATION_ON_CANCEL_REQUEST;
                    //KA 11/24/10 redone so it doesn't use the ld string
                    //strSubject = modOpportunity.strdCANCEL_REQUEST_SUBJECT;
                    strSubject = "Cancel Reservation Request ";
                    strMsg1 = modOpportunity.strdCANCEL_REQUEST_MESSAGE1;
                    strMsg2 = modOpportunity.strdCANCEL_REQUEST_MESSAGE2;
                    strMsg3 = modOpportunity.strdCANCEL_REQUEST_MESSAGE3;
                }
                // get recepient list from neighborhood notifcation team where notify on Sales Approved is true
                Recordset rstEmailTo = objLib.GetRecordset(strNotify, 1, vntNeighborhoodId, modOpportunity.strf_EMPLOYEE_ID);
                string strEmailTo = string.Empty;
                if (rstEmailTo.RecordCount > 0)
                {
                    rstEmailTo.MoveFirst();
                    StringBuilder emailToBuilder = new StringBuilder();
                    while (!(rstEmailTo.EOF))
                    {
                        string strWorkEmail = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtEMPLOYEE,
                            modOpportunity.strf_WORK_EMAIL, rstEmailTo.Fields[modOpportunity.strf_EMPLOYEE_ID].Value));
                        // add if not already there
                        if (!emailToBuilder.ToString().Contains(strWorkEmail))
                        {
                            emailToBuilder.Append(strWorkEmail + ";");
                        }
                        rstEmailTo.MoveNext();
                    }
                    // strip out last ;
                    strEmailTo = emailToBuilder.ToString();
                    strEmailTo = strEmailTo.Substring(0, strEmailTo.Length - 1);
                }
                rstEmailTo.Close();

                if (strEmailTo.Trim().Length == 0)
                {
                    return;
                }
                // all language strings are in nbhd_notification_team
                ILangDict lngNBHD_Notification_Team = RSysSystem.GetLDGroup(modOpportunity.strgNBHD_NOTIFICATION_TEAM);
                // set subject
                vntCurrentEmployeeId = administration.CurrentUserRecordId;
                vntCurrentEmployeeFirstName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_FIRST_NAME,
                    vntCurrentEmployeeId));
                vntCurrentEmployeeLastName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_LAST_NAME,
                    vntCurrentEmployeeId));
                // TODO (DI Yin) strLotDescriptor is never assigned. Temporary code here 
                string strLotDescriptor = string.Empty;
                
                //AM2010.11.17 - Email notification changes for cancelled contract
                // TODO (DI Yin) strLotDescriptor is never assigned. Temporary code here 
                
                //AM2010.10.14 - Get Neighborhood, Division and Lot for the strLot Descriptor
                string strNeighborhood = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfNAME, vntNeighborhoodId));
                object vntDivisionId = objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfDIVISION_ID, vntNeighborhoodId);
                string strDivision = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtDIVISION, modOpportunity.strfNAME, vntDivisionId));
                //object vntLotId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strfLOT_ID, rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value);
                Recordset rstLotRef = objLib.GetRecordset(vntLotId, modOpportunity.strtPRODUCT, modOpportunity.strfUNIT, modOpportunity.strfTRACT, modOpportunity.strfLOT_NUMBER);
                string strLot = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfLOT_NUMBER].Value);
                string strUnit = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfUNIT].Value);
                string strTract = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfTRACT].Value);
                strLotDescriptor = strDivision + ", " + strNeighborhood + ", T/" + strTract + " L/" + strLot + " U/" + strUnit;

                //KA 11/24/10 redone so it doesn't use the ld string
                //strSubject = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strSubject,
                //    new object[] {vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, strLotDescriptor,
                //    String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)) }));
                strSubject = strSubject + vntCurrentEmployeeFirstName + " " + vntCurrentEmployeeLastName + ", " + strLotDescriptor
                    + " - " + String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)) ;

                // set message
                string strMessage = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strMsg1,
                    new object[] {DateTime.Today, vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_FIRST_NAME, vntContactId)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_LAST_NAME, vntContactId)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strfHOME_PHONE, vntContactId)), 
                        String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)),
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                        rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                        rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value)) }));
                // TODO (Di Yin) vntJob_Number is never assigned, temporary code here
                int vntJob_Number = 0;
                strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strMsg2,
                    new object[] { vntJob_Number, TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfECOE_DATE].Value) }));
                strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strMsg3,
                    new object[] {vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_WORK_EMAIL, vntCurrentEmployeeId)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strfWORK_PHONE, vntCurrentEmployeeId))}));
                //KA 11/24/10 adding village/nbdh/tract/lot/unit info infront of the message var
                strMessage = strLotDescriptor + "\n\n" + strMessage;
                SendSimpleMail(strEmailTo, strSubject, strMessage);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }



        /// <summary>
        /// Gets the text for the Cancel notification email
        /// </summary>
        /// <param name="opportunityId">opportunity ID</param>
        /// <param name="subject">email subject text</param>
        /// <returns>Email message text</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        protected virtual string[] GetEmailTextSubject(object opportunityId, string subject)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strf_STATUS,
                    modOpportunity.strfCANCEL_REQUEST_DATE, modOpportunity.strfQUOTE_TOTAL, modOpportunity.strf_ACTUAL_DECISION_DATE,
                    modOpportunity.strfCONTACT_ID, modOpportunity.strf_ACCOUNT_MANAGER_ID, modOpportunity.strfNEIGHBORHOOD_ID,
                    modOpportunity.strfLOT_ID, modOpportunity.strfECOE_DATE, modOpportunity.strf_RN_DESCRIPTOR, modOpportunity.strf_PLAN_NAME_ID,
                    modOpportunity.strfELEVATION_ID);

                if (rstOpportunity.RecordCount > 0)
                {

                    object vntAccountMgrId = rstOpportunity.Fields[modOpportunity.strf_ACCOUNT_MANAGER_ID].Value;
                    object vntNeighborhoodId = rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                    object vntContactId = rstOpportunity.Fields[modOpportunity.strfCONTACT_ID].Value;
                    object vntPlanId = rstOpportunity.Fields[modOpportunity.strf_PLAN_NAME_ID].Value;
                    string strPlan = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT,
                            modOpportunity.strfPRODUCT_NAME, vntPlanId));
                    object vntElevationID = rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value;
                    string strElevation = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT,
                        modOpportunity.strfPRODUCT_NAME, vntElevationID));
                    string strSalesPrice = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value);
                    string strEcoeDate = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfECOE_DATE].Value);
                    object vntLotId = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;

                    // get the sales rep data
                    Recordset rstAccountMgr = objLib.GetRecordset(vntAccountMgrId, modOpportunity.strt_EMPLOYEE, modOpportunity.strf_FIRST_NAME,
                        modOpportunity.strf_LAST_NAME, modOpportunity.strfWORK_PHONE, modOpportunity.strf_WORK_EMAIL);
                    string strSalesRepName = String.Empty;
                    string strSalesRepPhone = String.Empty;
                    string strSalesRepEmail = String.Empty;
                    if (!(rstAccountMgr.RecordCount > 0))
                    {
                        strSalesRepName = rstAccountMgr.Fields[modOpportunity.strf_FIRST_NAME].Value + " " + rstAccountMgr.Fields[modOpportunity.strf_LAST_NAME].Value;
                        strSalesRepPhone = rstAccountMgr.Fields[modOpportunity.strfWORK_PHONE].Value + "";
                        strSalesRepEmail = rstAccountMgr.Fields[modOpportunity.strf_WORK_EMAIL].Value + "";
                    }

                    // get the purchaser data
                    Recordset rstContact = objLib.GetRecordset(vntContactId, modOpportunity.strtCONTACT, modOpportunity.strf_FIRST_NAME,
                        modOpportunity.strf_LAST_NAME, modOpportunity.strfHOME_PHONE);
                    string strPurchaserName = String.Empty;
                    string strPurchaserPhone = String.Empty;
                    if (!(rstContact.RecordCount > 0))
                    {
                        strPurchaserName = rstContact.Fields[modOpportunity.strf_FIRST_NAME].Value + " " + rstContact.Fields[modOpportunity.strf_LAST_NAME].Value;
                        strPurchaserPhone = rstContact.Fields[modOpportunity.strfHOME_PHONE].Value + "";
                    }

                    // get the lot job and descriptor
                    string strRnDescriptor = string.Empty;
                    string strJobNumber = string.Empty;
                    if ((vntLotId != DBNull.Value))
                    {
                        Recordset rstLot = objLib.GetRecordset(vntLotId, modOpportunity.strt_PRODUCT, modOpportunity.strfRN_DESCRIPTOR,
                            modOpportunity.strfELEVATION_ID, modOpportunity.strfTYPE, modOpportunity.strfJOB_NUMBER);

                        if (rstLot.RecordCount > 0)
                        {
                            strRnDescriptor = TypeConvert.ToString(rstLot.Fields[modOpportunity.strfRN_DESCRIPTOR].Value + "");
                            strJobNumber = TypeConvert.ToString(rstLot.Fields[modOpportunity.strfJOB_NUMBER].Value + "");
                        }
                    }

                    subject = subject + ": " + strSalesRepName + ", " + strRnDescriptor + " - " + strSalesPrice;

                    string strMessage = "\r\n" + " Sale Date: " + "\r\n" + " Cancel Request By: " + strSalesRepName + "\r\n" 
                        + " Purchaser Name: " + strPurchaserName + "\r\n" + " Purchaser Home Phone: " 
                        + strPurchaserPhone;
                    strMessage = strMessage + "\r\n" + " Total Sales Price: " + strSalesPrice + "\r\n" 
                        + " Plan Name/Elevation: " + strPlan + "/" + strElevation + "\r\n" + " Job ID: " 
                        + strJobNumber + "\r\n" + " ECOE Date: " + strEcoeDate;
                    strMessage = strMessage + "\r\n" + "If there are questions regarding this " + subject + ", " 
                        + strSalesRepName + " can be reached at following: " + "\r\n" + "Email: " + strSalesRepEmail 
                        + "\r\n" + "Work Phone: " + strSalesRepPhone;
                    return new string[] { strMessage, subject };
                }
                else
                {
                    // we haven't found oportunity, it must be some error
                    return new string[] { string.Empty, subject };
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Gets the emails of them sales team members on the opportunity team member who have the notify
        /// set to true
        /// </summary>
        /// <param name="opportunityId">opportunity ID</param>
        /// <returns>List of email recipients</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        protected virtual string GetEmailRecipients(object opportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // get the neighborhood id from opportunity
                object vntNeighborhoodId = objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfNEIGHBORHOOD_ID,
                    opportunityId);

                // get division id
                object vntDivisionId = objLib.SqlIndex(modOpportunity.strt_NEIGHBORHOOD, modOpportunity.strfDIVISION_ID,
                    vntNeighborhoodId);

                // get the recipients list
                // get sales team
                Recordset rstSalesTeam = objLib.GetRecordset(modOpportunity.strqTEAM_MEMBERS_WITH_NOTIFY, 1, opportunityId,
                    modOpportunity.strf_EMPLOYEE_ID);
                StringBuilder recipientBuilder = new StringBuilder();
                if (rstSalesTeam.RecordCount > 0)
                {
                    rstSalesTeam.MoveFirst();
                    while (!(rstSalesTeam.EOF))
                    {
                        object employeeId = rstSalesTeam.Fields[modOpportunity.strf_EMPLOYEE_ID].Value;

                        Recordset rstEmployee = objLib.GetRecordset(employeeId, modOpportunity.strt_EMPLOYEE, modOpportunity.strf_WORK_EMAIL);
                        if (rstEmployee.RecordCount > 0)
                        {
                            recipientBuilder.Append(rstEmployee.Fields[modOpportunity.strf_WORK_EMAIL].Value + ";");
                        }
                    }
                }

                // get other recipients which are not in the sales team
                // May 30, 2005 By Jwang. Changed the table name to Employee
                Recordset rstNotifedEmpl = objLib.GetRecordset(modOpportunity.strqEMPLOYEES_WITH_NOTIFY, 2, vntDivisionId, opportunityId,
                    modOpportunity.strf_EMPLOYEE_ID);
                if (rstNotifedEmpl.RecordCount > 0)
                {
                    rstNotifedEmpl.MoveFirst();
                    while (!(rstNotifedEmpl.EOF))
                    {
                        object employeeId = rstNotifedEmpl.Fields[modOpportunity.strf_EMPLOYEE_ID].Value;

                        Recordset rstEmployee = objLib.GetRecordset(employeeId, modOpportunity.strt_EMPLOYEE, modOpportunity.strf_WORK_EMAIL);
                        if (rstEmployee.RecordCount > 0)
                        {
                            recipientBuilder.Append(rstEmployee.Fields[modOpportunity.strf_WORK_EMAIL].Value);
                            recipientBuilder.Append(";");
                        }
                        rstNotifedEmpl.MoveNext();
                    }
                }
                return recipientBuilder.ToString();
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will be called when a user is trying to save a Quote record and entered Neighborhood
        /// but didn't enter <see crtef="Homesite"/>. If <see crtef="Homesite Required Quote"/> field of neighborhood is 
        /// True, then <see crtef="Homesite"/> field on the Quote is mandatory
        /// </summary>
        /// <param name="neighborhoodId">Neighborhood Id</param>
        /// <returns>True if validation OK (then <see crtef="Homesite Required Quote"/>is False)
        /// False if validation is NOT OK</returns>
        /// <history>
        /// Revision#      Date            Author          Description
        /// 3.8.0.0        5/12/2006       DYin            Converted to .Net C# code.
        /// </history>
        protected virtual bool CheckHomesite(object neighborhoodId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstNeighborhood = objLib.GetRecordset(neighborhoodId, modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfLOT_REQD);
                if (rstNeighborhood.RecordCount > 0)
                {
                    return !TypeConvert.ToBoolean(rstNeighborhood.Fields[modOpportunity.strfLOT_REQD].Value);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// sets the Price Update field to the supplied value
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="priceUpdated">Flag to indicate if the quote price is updated.</param>
        /// <history>
        /// Revision#    Date       Author    Description
        /// 3.8.0.0      5/12/2006  DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void SetQuotePriceUpdate(object opportunityId, bool priceUpdated)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOpp = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strfPRICE_UPDATE);

                if (rstOpp.RecordCount > 0)
                {
                    rstOpp.MoveFirst();
                    rstOpp.Fields[modOpportunity.strfPRICE_UPDATE].Value = priceUpdated;
                }
                objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstOpp);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Executes a Sales Request Declined and sends notification email
        /// </summary>
        /// <param name="opportunityId">opportunity ID</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        protected virtual void SalesRequestDeclined(object opportunityId)
        {
            try
            {
                if (opportunityId != DBNull.Value)
                {

                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                    // update status
                    Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strfCONTACT_ID,
                        modOpportunity.strfNEIGHBORHOOD_ID, modOpportunity.strf_ACCOUNT_MANAGER_ID, modOpportunity.strfPLAN_NAME_ID,
                        modOpportunity.strfELEVATION_ID, modOpportunity.strfQUOTE_TOTAL, modOpportunity.strfECOE_DATE, modOpportunity.strfLOT_ID);
                    object vntNeighborhood_Id = DBNull.Value;
                    object vntContactId = DBNull.Value;
                    object vntLotId = DBNull.Value;
                    if (rstOpportunity.RecordCount > 0)
                    {
                        // update NBHD Profile Cancel Request Date
                        vntContactId = rstOpportunity.Fields[modOpportunity.strfCONTACT_ID].Value;
                        vntNeighborhood_Id = rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                        vntLotId = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                        if ((vntContactId != DBNull.Value) && (vntNeighborhood_Id != DBNull.Value))
                        {
                            UpdateContactProfileNeighborhood(vntContactId, vntNeighborhood_Id, null, null, null, DateTime.Today,
                                null, null, null, null, null, null, null);
                        }
                    }

                    // send email
                    // get recepient list from neighborhood notifcation team where notify on Sales Approved is true
                    Recordset rstEmailTo = objLib.GetRecordset(modOpportunity.strqNOTIFICATION_FOR_SALES_RQST_DECLINED, 1, vntNeighborhood_Id,
                        modOpportunity.strf_EMPLOYEE_ID);
                    string strEmailTo = string.Empty;
                    if (rstEmailTo.RecordCount > 0)
                    {
                        rstEmailTo.MoveFirst();
                        StringBuilder emailToBuilder = new StringBuilder();
                        while (!(rstEmailTo.EOF))
                        {
                            string strWorkEmail = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtEMPLOYEE,
                                modOpportunity.strf_WORK_EMAIL, rstEmailTo.Fields[modOpportunity.strf_EMPLOYEE_ID].Value));
                            // add if not already there
                            if (!emailToBuilder.ToString().Contains(strWorkEmail))
                            {
                                emailToBuilder.Append(strWorkEmail + ";");
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
                    // TODO (Di Yin) strLotDescriptor is never assigned, temporary code here
                    string strLotDescriptor = string.Empty;

                    //AM2010.11.17 - Email notification changes for cancelled contract
                    // TODO (DI Yin) strLotDescriptor is never assigned. Temporary code here 

                    //AM2010.10.14 - Get Neighborhood, Division and Lot for the strLot Descriptor
                    string strNeighborhood = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfNAME, vntNeighborhood_Id));
                    object vntDivisionId = objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfDIVISION_ID, vntNeighborhood_Id);
                    string strDivision = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtDIVISION, modOpportunity.strfNAME, vntDivisionId));
                    //object vntLotId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strfLOT_ID, rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value);
                    Recordset rstLotRef = objLib.GetRecordset(vntLotId, modOpportunity.strtPRODUCT, modOpportunity.strfUNIT, modOpportunity.strfTRACT, modOpportunity.strfLOT_NUMBER);
                    string strLot = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfLOT_NUMBER].Value);
                    string strUnit = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfUNIT].Value);
                    string strTract = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfTRACT].Value);
                    strLotDescriptor = strDivision + ", " + strNeighborhood + ", T/" + strTract + " L/" + strLot + " U/" + strUnit;


                    string strSubject = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdSALES_DECLINED_SUBJECT, 
                        new object[] {vntSalesRepFirstName, vntSalesRepLastName, strLotDescriptor,
                        String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)) }));
                    // set message
                    string strMessage = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdSALES_DECLINED_MESSAGE1, 
                        new object[] { DateTime.Today, vntSalesRepFirstName, vntSalesRepLastName, 
                            TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_FIRST_NAME, vntContactId)), 
                            TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_LAST_NAME, vntContactId)), 
                            TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strfHOME_PHONE, vntContactId)), 
                            String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)), 
                            TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                            rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value)),
                            TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                            rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value))}));
                    // TODO (Di Yin) vntJob_Number is never assigned, temporary code here
                    int vntJob_Number = 0;
                    strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdSALES_DECLINED_MESSAGE2, 
                        new object[] { TypeConvert.ToString(vntJob_Number), TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfECOE_DATE].Value) }));
                    strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdSALES_DECLINED_MESSAGE3, 
                        new object[] { vntSalesRepFirstName, vntSalesRepLastName, 
                            TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_WORK_EMAIL, vntSalesRepId)), 
                            TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strfWORK_PHONE, vntSalesRepId)) }));

                    //KA 11/24/10 adding village/nbdh/tract/lot/unit info infront of the message var
                    strMessage = strLotDescriptor + "\n\n" + strMessage;
                    if (strEmailTo.Length > 0 && strSubject.Length > 0 && strMessage.Length > 0)
                    {
                        SendSimpleMail(strEmailTo, strSubject, strMessage);
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Cancel Request Declined and sends notification email
        /// </summary>
        /// <param name="opportunityId">opportunity ID</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        protected virtual void CancelRequestDeclined(object opportunityId)
        {
            try
            {
                if (opportunityId != DBNull.Value)
                {
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    // update status
                    Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strfCONTACT_ID,
                        modOpportunity.strfNEIGHBORHOOD_ID, modOpportunity.strf_ACCOUNT_MANAGER_ID, modOpportunity.strfPLAN_NAME_ID,
                        modOpportunity.strfELEVATION_ID, modOpportunity.strfQUOTE_TOTAL, modOpportunity.strfECOE_DATE, modOpportunity.strfLOT_ID);
                    object vntContactId = DBNull.Value;
                    object vntNeighborhood_Id = DBNull.Value;
                    object vntLotId = DBNull.Value;
                    if (rstOpportunity.RecordCount > 0)
                    {
                        // update NBHD Profile Cancel Request Declined Date
                        vntContactId = rstOpportunity.Fields[modOpportunity.strfCONTACT_ID].Value;
                        vntNeighborhood_Id = rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                        vntLotId = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                        if ((vntContactId != DBNull.Value) && (vntNeighborhood_Id != DBNull.Value))
                        {
                            UpdateContactProfileNeighborhood(vntContactId, vntNeighborhood_Id, null, null, null, null, DateTime.Today,
                                null, null, null, null, null, null);
                        }
                    }

                    // send email
                    // get recepient list from neighborhood notifcation team where notify on Sales Approved is true
                    Recordset rstEmailTo = objLib.GetRecordset(modOpportunity.strqNOTIFICATION_OF_CANCEL_RQST_DECLINED, 1, vntNeighborhood_Id,
                        modOpportunity.strf_EMPLOYEE_ID);
                    string strEmailTo = string.Empty;
                    if (rstEmailTo.RecordCount > 0)
                    {
                        rstEmailTo.MoveFirst();
                        StringBuilder emailToBuilder = new StringBuilder();
                        while (!(rstEmailTo.EOF))
                        {
                            string strWorkEmail = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtEMPLOYEE,
                                modOpportunity.strf_WORK_EMAIL, rstEmailTo.Fields[modOpportunity.strf_EMPLOYEE_ID].Value));
                            // add if not already there
                            if (!emailToBuilder.ToString().Contains(strWorkEmail))
                            {
                                emailToBuilder.Append(strWorkEmail + ";");
                            }
                            rstEmailTo.MoveNext();
                        }
                        // strip out last ;
                        strEmailTo = emailToBuilder.ToString();
                        strEmailTo = strEmailTo.Substring(0, strEmailTo.Length - 1);

                        // all language strings are in nbhd_notification_team
                        ILangDict lngNBHD_Notification_Team = RSysSystem.GetLDGroup(modOpportunity.strgNBHD_NOTIFICATION_TEAM);
                        // set subject
                        object vntSalesRepId = rstOpportunity.Fields[modOpportunity.strf_ACCOUNT_MANAGER_ID].Value;
                        string vntSalesRepFirstName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_FIRST_NAME,
                            vntSalesRepId));
                        string vntSalesRepLastName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_LAST_NAME,
                            vntSalesRepId));
                        // TODO (DI Yin) strLotDescriptor is never assigned. Temporary code here 
                        string strLotDescriptor = string.Empty;


                        string strNeighborhood = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfNAME, vntNeighborhood_Id));
                        object vntDivisionId = objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfDIVISION_ID, vntNeighborhood_Id);
                        string strDivision = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtDIVISION, modOpportunity.strfNAME, vntDivisionId));
                        //object vntLotId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strfLOT_ID, rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value);
                        Recordset rstLotRef = objLib.GetRecordset(vntLotId, modOpportunity.strtPRODUCT, modOpportunity.strfUNIT, modOpportunity.strfTRACT, modOpportunity.strfLOT_NUMBER);
                        string strLot = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfLOT_NUMBER].Value);
                        string strUnit = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfUNIT].Value);
                        string strTract = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfTRACT].Value);
                        strLotDescriptor = strDivision + ", " + strNeighborhood + ", T/" + strTract + " L/" + strLot + " U/" + strUnit;


                        string strSubject = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdCANCEL_DECLINE_SUBJECT, 
                            new object[] { vntSalesRepFirstName, vntSalesRepLastName, strLotDescriptor, 
                            String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)) }));
                        // set message
                        string strMessage = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdCANCEL_DECLINE_MESSAGE1, 
                            new object[] { DateTime.Today, vntSalesRepFirstName, vntSalesRepLastName, 
                                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_FIRST_NAME, vntContactId)), 
                                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_LAST_NAME, vntContactId)), 
                                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strfHOME_PHONE, vntContactId)), 
                                String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)), 
                                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                                rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value)), 
                                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                                rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value)) }));
                        // TODO (DI Yin) vntJob_Number is never assigned. Temporary code here 
                        int vntJob_Number = 0;
                        strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdCANCEL_DECLINE_MESSAGE2, 
                            new object[] { vntJob_Number, TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfECOE_DATE].Value) }));
                        strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdCANCEL_DECLINE_MESSAGE3, 
                            new object[] { vntSalesRepFirstName, vntSalesRepLastName, 
                                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_WORK_EMAIL, vntSalesRepId)), 
                                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strfWORK_PHONE, vntSalesRepId)) }));
                        //KA 11/24/10 adding village/nbdh/tract/lot/unit info infront of the message var
                        strMessage = strLotDescriptor + "\n\n" + strMessage;
                        if (strEmailTo.Length > 0 && strSubject.Length > 0 && strMessage.Length > 0)
                        {
                            SendSimpleMail(strEmailTo, strSubject, strMessage);
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sales Request and sends notification email
        /// </summary>
        /// <param name="opportunityId">opportunity ID</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        public virtual void SalesRequest(object opportunityId)
        {
            try
            {

                if (opportunityId != DBNull.Value)
                {
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                    // update status
                    Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strfCONTACT_ID,
                        modOpportunity.strfNEIGHBORHOOD_ID, modOpportunity.strf_ACCOUNT_MANAGER_ID, modOpportunity.strfPLAN_NAME_ID,
                        modOpportunity.strfELEVATION_ID, modOpportunity.strfQUOTE_TOTAL, modOpportunity.strfECOE_DATE, modOpportunity.strfLOT_ID,
                        modOpportunity.strfCONTRACT_APPROVED_SUBMITTED, modOpportunity.strfCONTRACT_APPROVED_SUBMITTED_DATETIME,
                        modOpportunity.strfRESERVATION_DATE, modOpportunity.strfRESERVATIONEXPIRY, modOpportunity.strfPIPELINE_STAGE,
                        modOpportunity.strfSTATUS, modOpportunity.strfSALE_DECLINED_DATE, modOpportunity.strfSALE_DECLINED_BY);

                    if (rstOpportunity.RecordCount > 0)
                    {
                        // continue with the rest of the sale request process
                        rstOpportunity.Fields[modOpportunity.strfCONTRACT_APPROVAL_SUBMITTED].Value = DateTime.Now;
                        rstOpportunity.Fields[modOpportunity.strfCONTRACT_APPROVAL_DATETIME].Value = DateTime.Now;
                        if (Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfRESERVATION_DATE].Value))
                        {
                            rstOpportunity.Fields[modOpportunity.strfRESERVATION_DATE].Value = DateTime.Now;
                        }
                        rstOpportunity.Fields[modOpportunity.strfRESERVATIONEXPIRY].Value = DBNull.Value;
                        rstOpportunity.Fields[modOpportunity.strfPIPELINE_STAGE].Value = modOpportunity.strsSALES_REQUEST;
                        rstOpportunity.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsIN_PROGRESS;
                        rstOpportunity.Fields[modOpportunity.strfSALE_DECLINED_DATE].Value = DBNull.Value;
                        rstOpportunity.Fields[modOpportunity.strfSALE_DECLINED_BY].Value = DBNull.Value;

                        objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstOpportunity);

                        // update NBHD Profile Cancel Request Date
                        object vntContactId = rstOpportunity.Fields[modOpportunity.strfCONTACT_ID].Value;
                        object vntNeighborhood_Id = rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                        object vntLotId = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                        if ((vntContactId != DBNull.Value) && (vntNeighborhood_Id != DBNull.Value) && (vntLotId != DBNull.Value))
                        {
                            UpdateContactProfileNeighborhood(vntContactId, vntNeighborhood_Id, DateTime.Today, null, null, null,
                                null, null, null, DateTime.Today, null, null, null);
                            UpdateLotStatus(vntLotId, opportunityId);
                            
                            //WriteContractHistoryRecords(vntLotId, opportunityId, modOpportunity.strsSOLD, DateTime.Today, true, null);
                        }

                        // Set the net config flag to true for al the options on the quote
                        SetBaseConfiguration(opportunityId);
    
                        // Remove unselected options from all quotes for the lot

                        Recordset rstOptions = objLib.GetRecordset(modOpportunity.strfUNSELECTED_OPTIONS_FOR_LOT, 
                            1, vntLotId, modOpportunity.strf_OPPORTUNITY__PRODUCT_ID);
                            
                        if (rstOptions.RecordCount > 0)
                        {
                            rstOptions.MoveFirst();
                            IRForm rfrmForm = RSysSystem.Forms[modOpportunity.strrHB_OPPORTUNITY_PRODUCT];
                            object parameterList = DBNull.Value;
                            while (!rstOptions.EOF)
                            {
                                rfrmForm.DeleteFormData(rstOptions.Fields[modOpportunity.strf_OPPORTUNITY__PRODUCT_ID].Value, ref parameterList);
                                rstOptions.MoveNext();
                            }
                        }
                        rstOptions.Close();
                        rstOptions = null;

                        // send email
                        // get recepient list from neighborhood notifcation team where notify on Sales Approved is true
                        Recordset rstEmailTo = objLib.GetRecordset(modOpportunity.strqNOTIFICATION_ON_SALES_RQST, 1, vntNeighborhood_Id,
                            modOpportunity.strf_EMPLOYEE_ID);
                        if (rstEmailTo.RecordCount > 0)
                        {
                            rstEmailTo.MoveFirst();
                            StringBuilder emailToBuilder = new StringBuilder();
                            while (!(rstEmailTo.EOF))
                            {
                                string strWorkEmail = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtEMPLOYEE,
                                    modOpportunity.strf_WORK_EMAIL, rstEmailTo.Fields[modOpportunity.strf_EMPLOYEE_ID].Value));
                                // add if not already there
                                if (!emailToBuilder.ToString().Contains(strWorkEmail))
                                {
                                    emailToBuilder.Append(strWorkEmail + ";");
                                }
                                rstEmailTo.MoveNext();
                            }
                            // strip out last ;
                            string strEmailTo = emailToBuilder.ToString();
                            strEmailTo = strEmailTo.Substring(0, strEmailTo.Length - 1);

                            // all language strings are in nbhd_notification_team
                            ILangDict lngNBHD_Notification_Team = RSysSystem.GetLDGroup(modOpportunity.strgNBHD_NOTIFICATION_TEAM);
                            // set subject
                            object vntSalesRepId = rstOpportunity.Fields[modOpportunity.strf_ACCOUNT_MANAGER_ID].Value;
                            string vntSalesRepFirstName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_FIRST_NAME,
                                vntSalesRepId));
                            string vntSalesRepLastName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_LAST_NAME,
                                vntSalesRepId));
                            string strLotDescriptor = string.Empty;//TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_PRODUCT, modOpportunity.strfRN_DESCRIPTOR,
                                //vntLotId));
                            int vntJob_Number = TypeConvert.ToInt32(objLib.SqlIndex(modOpportunity.strt_PRODUCT, modOpportunity.strfJOB_NUMBER,
                                vntLotId));

                            string strNeighborhood = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfNAME, vntNeighborhood_Id));
                            object vntDivisionId = objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfDIVISION_ID, vntNeighborhood_Id);
                            string strDivision = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtDIVISION, modOpportunity.strfNAME, vntDivisionId));
                            //object vntLotId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strfLOT_ID, rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value);
                            Recordset rstLotRef = objLib.GetRecordset(vntLotId, modOpportunity.strtPRODUCT, modOpportunity.strfUNIT, modOpportunity.strfTRACT, modOpportunity.strfLOT_NUMBER);
                            string strLot = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfLOT_NUMBER].Value);
                            string strUnit = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfUNIT].Value);
                            string strTract = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfTRACT].Value);
                            strLotDescriptor = strDivision + ", " + strNeighborhood + ", T/" + strTract + " L/" + strLot + " U/" + strUnit;


                            string strSubject = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdSALES_REQUEST_SUBJECT, 
                                new object[] {vntSalesRepFirstName, vntSalesRepLastName, strLotDescriptor, 
                                    String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)) }));
                            // set message
                            string strMessage = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdSALES_REQUEST_MESSAGE1, 
                                new object[] {DateTime.Today, vntSalesRepFirstName, vntSalesRepLastName, 
                                    TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_FIRST_NAME, vntContactId)), 
                                    TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_LAST_NAME, vntContactId)), 
                                    TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strfHOME_PHONE, vntContactId)), 
                                    String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)),
                                    TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                                    rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value)), 
                                    TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                                    rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value)) }));

                            strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdSALES_REQUEST_MESSAGE2, 
                                new object[] { vntJob_Number, TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfECOE_DATE].Value) }));
                            strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(modOpportunity.strdSALES_REQUEST_MESSAGE3, 
                                new object[] { vntSalesRepFirstName, vntSalesRepLastName, 
                                    TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_WORK_EMAIL, vntSalesRepId)), 
                                    TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strfWORK_PHONE, vntSalesRepId)) }));

                            strMessage = strLotDescriptor + "/n" + strMessage;
                            if (strEmailTo.Length > 0 && strSubject.Length > 0 && strMessage.Length > 0)
                            {
                                SendSimpleMail(strEmailTo, strSubject, strMessage);
                            }
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// returns the final plan price for a quote
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <returns>Quote plan price</returns>
        /// <history>
        /// Revision#      Date           Author      Description
        /// 3.8.0.0        5/12/2006      DYin        Converted to .Net C# code.
        /// </history>
        protected virtual decimal GetQuotePlanPrice(object opportunityId)
        {
            decimal homesitePremium = 0;
            return this.GetQuotePlanPrice(opportunityId, out homesitePremium);
        }

        /// <summary>
        /// returns the final plan price for a quote
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="homesitePremium">the base price</param>
        /// <returns>Quote plan price</returns>
        /// <history>
        /// Revision#      Date           Author      Description
        /// 3.8.0.0        5/12/2006      DYin        Converted to .Net C# code.
        /// </history>
        protected virtual decimal GetQuotePlanPrice(object opportunityId, out decimal homesitePremium)
        {
            DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();

            Recordset opportunityRecordset = dataAccess.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY,
                modOpportunity.strfSTATUS, modOpportunity.strfPLAN_BUILT, modOpportunity.strfPRICE,
                modOpportunity.strfLOT_PREMIUM, modOpportunity.strfLOT_ID, modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfNEIGHBORHOOD_ID);
            return this.GetQuotePlanPrice(opportunityRecordset, out homesitePremium);
        }
        /// <summary>
        /// returns the final plan price for a quote
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="lotId">Lot Id</param>
        /// <param name="planId">can be supplied if price is not, to obtain price from plan</param>
        /// <returns>Quote plan price</returns>
        /// <history>
        /// Revision#      Date           Author      Description
        /// 3.8.0.0        5/12/2006      DYin        Converted to .Net C# code.
        /// </history>
        protected virtual decimal GetQuotePlanPrice(object opportunityId, object lotId, object planId)
        {
            decimal homesitePremium = 0;
            return this.GetQuotePlanPrice(opportunityId, lotId, planId, out homesitePremium);
        }

        /// <summary>
        /// Returns the final plan price for a quote, If the lotId is null, the lot Id for opportunity will be used, 
        /// if the planId is null, the Plan_Name_Id for the opportunity will be used.
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="lotId">Lot Id</param>
        /// <param name="planId">can be supplied if price is not, to obtain price from plan</param>
        /// <param name="homesitePremium">the base price</param>
        /// <returns>Quote plan price</returns>
        /// <history>
        /// Revision#      Date           Author      Description
        /// 3.8.0.0        5/12/2006      DYin        Converted to .Net C# code.
        /// </history>
        protected virtual decimal GetQuotePlanPrice(object opportunityId, object lotId, object planId, 
            out decimal homesitePremium)
        {
            try
            {
                decimal quotePlanPrice = 0;
                homesitePremium = 0;
                if (opportunityId != DBNull.Value)
                {
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                        .CreateInstance();

                    Recordset opportunityRecordset = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY,
                        modOpportunity.strfSTATUS, modOpportunity.strfPLAN_BUILT, modOpportunity.strfPRICE,
                        modOpportunity.strfLOT_PREMIUM, modOpportunity.strfLOT_ID, modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfNEIGHBORHOOD_ID);

                    if (opportunityRecordset.RecordCount > 0)
                    {
                        opportunityRecordset.MoveFirst();
                        if (lotId != DBNull.Value)
                        {
                            opportunityRecordset.Fields[modOpportunity.strfLOT_ID].Value = lotId;
                        }

                        if (planId != DBNull.Value)
                        {
                            opportunityRecordset.Fields[modOpportunity.strfPLAN_NAME_ID].Value = planId;
                        }
                        return this.GetQuotePlanPrice(opportunityRecordset, out homesitePremium);
                    }
                }
                return quotePlanPrice;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// returns the final plan price for a quote
        /// </summary>
        /// <param name="opportunityRecordset">Opportunity recordset</param>
        /// <param name="homesitePremium">the base price</param>
        /// <returns>Quote plan price</returns>
        /// <history>
        /// Revision#      Date           Author      Description
        /// 3.8.0.0        5/12/2006      DYin        Converted to .Net C# code.
        /// 5.9.0.0        4/9/2007       BC          Issue Fix: 65536-18697
        /// 5.9.0.0        7/24/2007      ML          Issue Fix: 65536-19941
        /// </history>
        protected virtual decimal GetQuotePlanPrice(Recordset opportunityRecordset, out decimal homesitePremium)
        {
            try
            {
                object lotId = opportunityRecordset.Fields[modOpportunity.strfLOT_ID].Value;
                object planId = opportunityRecordset.Fields[modOpportunity.strfPLAN_NAME_ID].Value;
                object vntNeighborhoodId = opportunityRecordset.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                    .CreateInstance();
                //Get the Division Id from Neighborhood
                object vntDivisionId = objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfDIVISION_ID,
                    vntNeighborhoodId);

                StandardOptionPricing builtOptionPricing = (StandardOptionPricing)TypeConvert.ToInt32(objLib.SqlIndex
                    (modOpportunity.strt_DIVISION, modOpportunity.strfBUILD_OPTION_PRICING, vntDivisionId));

                // start with with base price
                decimal quotePlanPrice = 0;
                homesitePremium = 0;
                if ((TypeConvert.ToString(opportunityRecordset.Fields[modOpportunity.strfSTATUS].Value) == modOpportunity.strQUOTE_STATUS_INVENTORY) &&
                    (TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtPRODUCT, modOpportunity.strfTYPE, lotId)) == modOpportunity.strsINVENTORY) &&
                    TypeConvert.ToBoolean(opportunityRecordset.Fields[modOpportunity.strfPLAN_BUILT].Value) &&
                    (builtOptionPricing == StandardOptionPricing.Fixed))
                {
                    // there is no date & time when the plan built was set therefore, price remains the same
                    quotePlanPrice = TypeConvert.ToDecimal(opportunityRecordset.Fields[modOpportunity.strfPRICE].Value);
                    homesitePremium = TypeConvert.ToDecimal(opportunityRecordset.Fields[modOpportunity.strfLOT_PREMIUM].Value);
                }
                else
                {
                    if (TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtPRODUCT, modOpportunity.strfTYPE, lotId)) == modOpportunity.strsINVENTORY)
                    {
                        // Homesite is of type = Inventory
                        //ML - 23 july 07 - in case of inventory quote for which the Additional_Price field has just been updated
                        //Issue#65536-19941
                        Recordset rstInvQuote = opportunityRecordset;
                        if (!(TypeConvert.ToString(opportunityRecordset.Fields[modOpportunity.strfSTATUS].Value) == modOpportunity.strsINVENTORY))
                        {
                            rstInvQuote = objLib.GetRecordset(modOpportunity.strqACTIVE_INVENTORY_QUOTES_FOR_LOT, 1, lotId,
                            modOpportunity.strfADDITIONAL_PRICE, modOpportunity.strfPRICE, modOpportunity.strfLOT_PREMIUM);
                          if (rstInvQuote.RecordCount > 0)
                          {
                            rstInvQuote.MoveFirst();
                            quotePlanPrice = TypeConvert.ToDecimal(rstInvQuote.Fields[modOpportunity.strfPRICE].Value)
                                + TypeConvert.ToDecimal(rstInvQuote.Fields[modOpportunity.strfADDITIONAL_PRICE].Value);
                            homesitePremium = TypeConvert.ToDecimal(rstInvQuote.Fields[modOpportunity.strfLOT_PREMIUM].Value);
                          }
                        }
                        else
                        {
                            rstInvQuote.MoveFirst();
                            quotePlanPrice = TypeConvert.ToDecimal(rstInvQuote.Fields[modOpportunity.strfPRICE].Value);
                            homesitePremium = TypeConvert.ToDecimal(rstInvQuote.Fields[modOpportunity.strfLOT_PREMIUM].Value);
                        }
                        
                    }
                    else
                    {
                        quotePlanPrice = TypeConvert.ToDecimal(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfCURRENT_PRICE, planId));
                        homesitePremium = TypeConvert.ToDecimal(objLib.SqlIndex(modOpportunity.strt_PRODUCT, modOpportunity.strfPRICE, lotId));
                    }

                    bool blnIncHomesitePremium = TypeConvert.ToBoolean(objLib.SqlIndex(modOpportunity.strtDIVISION,
                        modOpportunity.strfINCLUDE_HOMESITE_PREMIUM, vntDivisionId));
                    if (blnIncHomesitePremium)
                    {
                        // Plan premium price includes the Homesite Premium
                        quotePlanPrice = quotePlanPrice + homesitePremium;
                        homesitePremium = 0;
                    }
                }
                return quotePlanPrice;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Generates the note to be placed on the quote when inactivating because of an inventory change
        /// </summary>
        /// <returns>note as string</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 3.8.0.0  5/12/2006  DYin  Converted to .Net C# code.
        /// </history>
        public virtual void UpdateCustomerQuoteLocations(object opportunityProductId)
        {
            OpportunityProductAttributePreference objOpAttrPref = (OpportunityProductAttributePreference) RSysSystem.ServerScripts[modOpportunity.strsOP_ATTR_PREF].CreateInstance();
            DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

            object vntOpportunity_Id = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY__PRODUCT, modOpportunity.strfOPPORTUNITY_ID,
                opportunityProductId);

            if (TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfSTATUS,
                vntOpportunity_Id)) == modOpportunity.strsINVENTORY)
            {
                // get customer quotes for lot
                Recordset rstQuote = objLib.GetRecordset(modOpportunity.strqACTIVE_CUSTOMER_QUOTES_FOR_LOT, 1, objLib.SqlIndex
                    (modOpportunity.strt_OPPORTUNITY, modOpportunity.strfLOT_ID, vntOpportunity_Id), 
                    modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfCONFIGURATION_CHANGED, modOpportunity.strfDESCRIPTION);
                if (rstQuote.RecordCount > 0)
                {
                    rstQuote.MoveFirst();
                    while(!(rstQuote.EOF))
                    {
                        // get matching opp product for the quote
                        Recordset rstOppProd = objLib.GetRecordset(modOpportunity.strqOPP_PRODUCT_FOR_OPP_AND_NBHD_PRODUCT, 2,
                            rstQuote.Fields[modOpportunity.strfOPPORTUNITY_ID].Value, objLib.SqlIndex(modOpportunity.strtOPPORTUNITY__PRODUCT,
                            modOpportunity.strfNBHDP_PRODUCT_ID, opportunityProductId), modOpportunity.strfOPPORTUNITY__PRODUCT_ID);
                        if (rstOppProd.RecordCount > 0)
                        {
                            // delete exisiting product locations
                            rstOppProd.MoveFirst();
                            Recordset rstOppProdLoc = objLib.GetRecordset(modOpportunity.strqOPP_PROD_LOC_FOR_OPPPRODUCT, 1, rstOppProd.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value,
                                modOpportunity.strfOPP_PRODUCT_LOCATION_ID);
                            if (rstOppProdLoc.RecordCount > 0)
                            {
                                rstQuote.MoveFirst();
                                IRForm rfrmForm = RSysSystem.Forms[modOpportunity.strrOPPORTUNITY_PRODUCT_LOCATION];
                                object parameterList = DBNull.Value;
                                rstOppProdLoc.MoveFirst();
                                while(!(rstOppProdLoc.EOF))
                                {
                                    rfrmForm.DeleteFormData(rstOppProdLoc.Fields[modOpportunity.strfOPP_PRODUCT_LOCATION_ID].Value,
                                        ref parameterList);
                                    rstOppProdLoc.MoveNext();
                                }
                            }
                            rstOppProdLoc = null;
                            // add from inventory
                            CopyOptionSecondaryByOption(opportunityProductId, rstOppProd.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value);

                            objOpAttrPref.UpdateOptionTotal(DBNull.Value, rstOppProd.Fields[modOpportunity.strfOPPORTUNITY__PRODUCT_ID].Value);
                        }

                        // set the configuration changed flag on the quote
                        rstQuote.Fields[modOpportunity.strfCONFIGURATION_CHANGED].Value = true;
                        rstOppProd.Close();
                        rstOppProd = null;
                        rstQuote.MoveNext();
                    }
                }

                objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstQuote);
                rstQuote.Close();
            }
        }

        /// <summary>
        /// Copy Secondaries for a opportunity product
        /// </summary>
        /// <param name="sourceOptionId">Source Option Id</param>
        /// <param name="targetOptionId">Target Option id</param>
        /// <returns>a boolean - true if the function created the secondaries properly, false otherwise</returns>
        // Revision# Date Author Description
        // 3.8.0.0   5/12/2006  DYin  Converted to .Net C# code.
        // 5.9.0.0   4/9/2007   BC    Packages Options Copy
        public virtual bool CopyOptionSecondaryByOption(object sourceOptionId, object targetOptionId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                if ((sourceOptionId != DBNull.Value))
                {
                    string strType = "";
                    strType = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY__PRODUCT, modOpportunity.strfTYPE, sourceOptionId));
                    Recordset rstOPLocation = objLib.GetRecordset(modOpportunity.strqOP_LOCS_FOR_OP, 1, sourceOptionId, modOpportunity.strfLOCATION_QUANTITY,
                        modOpportunity.strfPREFERENCE_LIST, modOpportunity.strfOPP_PRODUCT_LOCATION_ID, modOpportunity.strfLOCATION_ID,
                        modOpportunity.EnvDUNSNumberField, modOpportunity.EnvGTINField, modOpportunity.EnvNHTManufacturerNumberField,
                        modOpportunity.EnvProductBrandField, modOpportunity.EnvProductNumberField, modOpportunity.EnvUCCCodeField
                        );
                    if (rstOPLocation.RecordCount > 0)
                    {
                        rstOPLocation.MoveFirst();
                        while (!(rstOPLocation.EOF))
                        {
                            // attributes?
                            Recordset rstOPAttrPref = objLib.GetRecordset(modOpportunity.strqOP_LOC_ATTR_PREF_FOR_OPLOC, 1, rstOPLocation.Fields[modOpportunity.strfOPP_PRODUCT_LOCATION_ID].Value,
                                modOpportunity.strfOPP_PRODUCT_LOCATION_ID, modOpportunity.strfATTRIBUTE, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID);
                            // create a new one
                            Recordset rstNewOPLoc = objLib.GetNewRecordset(modOpportunity.strtOPP_PRODUCT_LOCATION, modOpportunity.strfLOCATION_ID,
                                modOpportunity.strfLOCATION_QUANTITY, modOpportunity.strfPREFERENCE_LIST, modOpportunity.strfOPPORTUNITY_ID,
                                modOpportunity.strfOPP_PRODUCT_ID, modOpportunity.strfPARENT_PACKAGE_OPPPROD_ID,
                                modOpportunity.EnvDUNSNumberField, modOpportunity.EnvGTINField, modOpportunity.EnvNHTManufacturerNumberField,
                                modOpportunity.EnvProductBrandField, modOpportunity.EnvProductNumberField, modOpportunity.EnvUCCCodeField
                                );
                            rstNewOPLoc.AddNew(Type.Missing, Type.Missing);
                            rstNewOPLoc.Fields[modOpportunity.strfLOCATION_ID].Value = rstOPLocation.Fields[modOpportunity.strfLOCATION_ID].Value;
                            rstNewOPLoc.Fields[modOpportunity.strfLOCATION_QUANTITY].Value = rstOPLocation.Fields[modOpportunity.strfLOCATION_QUANTITY].Value;
                            rstNewOPLoc.Fields[modOpportunity.strfPREFERENCE_LIST].Value = rstOPLocation.Fields[modOpportunity.strfPREFERENCE_LIST].Value;
                            rstNewOPLoc.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY__PRODUCT,
                                modOpportunity.strfOPPORTUNITY_ID, targetOptionId);
                            rstNewOPLoc.Fields[modOpportunity.strfOPP_PRODUCT_ID].Value = targetOptionId;
                            rstNewOPLoc.Fields[modOpportunity.EnvDUNSNumberField].Value = rstOPLocation.Fields[modOpportunity.EnvDUNSNumberField].Value; 
                            rstNewOPLoc.Fields[modOpportunity.EnvGTINField].Value = rstOPLocation.Fields[modOpportunity.EnvGTINField].Value; 
                            rstNewOPLoc.Fields[modOpportunity.EnvNHTManufacturerNumberField].Value = rstOPLocation.Fields[modOpportunity.EnvNHTManufacturerNumberField].Value; 
                            rstNewOPLoc.Fields[modOpportunity.EnvProductBrandField].Value = rstOPLocation.Fields[modOpportunity.EnvProductBrandField].Value; 
                            rstNewOPLoc.Fields[modOpportunity.EnvProductNumberField].Value = rstOPLocation.Fields[modOpportunity.EnvProductNumberField].Value; 
                            rstNewOPLoc.Fields[modOpportunity.EnvUCCCodeField].Value = rstOPLocation.Fields[modOpportunity.EnvUCCCodeField].Value; 

                            objLib.SaveRecordset(modOpportunity.strtOPP_PRODUCT_LOCATION, rstNewOPLoc);
                            object vntOPLocId = rstNewOPLoc.Fields[modOpportunity.strfOPP_PRODUCT_LOCATION_ID].Value;
                            if (rstOPAttrPref.RecordCount > 0)
                            {
                                rstOPAttrPref.MoveFirst();
                                while (!(rstOPAttrPref.EOF))
                                {
                                    // creat new atrr/pref for Op location
                                    Recordset rstNewAttrPref = objLib.GetNewRecordset(modOpportunity.strtOPPPROD_ATTR_PREF, modOpportunity.strfATTRIBUTE,
                                        modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID, modOpportunity.strfOPP_PRODUCT_LOCATION_ID);
                                    rstNewAttrPref.AddNew(Type.Missing, Type.Missing);
                                    rstNewAttrPref.Fields[modOpportunity.strfOPP_PRODUCT_LOCATION_ID].Value = vntOPLocId;
                                    rstNewAttrPref.Fields[modOpportunity.strfATTRIBUTE].Value = rstOPAttrPref.Fields[modOpportunity.strfATTRIBUTE].Value;
                                    rstNewAttrPref.Fields[modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID].Value = rstOPAttrPref.Fields[modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID].Value;
                                    objLib.SaveRecordset(modOpportunity.strtOPPPROD_ATTR_PREF, rstNewAttrPref);
                                    
                                    //Copy data from Opp Product Pref table for the the Opp Product Id
                                    Recordset rstOppProductPref = objLib.GetRecordset(modOpportunity.strqOP_PREF_FOR_ATTRIBUTE, 1, rstOPAttrPref.Fields[modOpportunity.strfOP_LOC_ATTR_PREF_ID].Value,
                                                                modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID, modOpportunity.strfPREFERENCE_NAME, modOpportunity.strfCODE_,
                                                                modOpportunity.strfOPPORTUNITY_PRODUCT_ID, modOpportunity.strf_DIVISION_PRODUCT_PREF_ID,
                                                                modOpportunity.strfOP_LOC_ATTR_PREF_ID);
                                    if (rstOppProductPref.RecordCount > 0)
                                    {
                                        rstOppProductPref.MoveFirst();
                                        while (!rstOppProductPref.EOF)
                                        {
                                            // create a new one
                                            Recordset rstNewOppProductPref = objLib.GetNewRecordset(modOpportunity.strtOPPORTUNITY_PRODUCT_PREF,
                                                        modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID, modOpportunity.strfPREFERENCE_NAME,
                                                        modOpportunity.strfCODE_, modOpportunity.strfOPPORTUNITY_PRODUCT_ID,
                                                        modOpportunity.strf_DIVISION_PRODUCT_PREF_ID, modOpportunity.strfOP_LOC_ATTR_PREF_ID);
                                            rstNewOppProductPref.AddNew(Type.Missing, Type.Missing);
                                            rstNewOppProductPref.Fields[modOpportunity.strfPREFERENCE_NAME].Value
                                                    = rstOppProductPref.Fields[modOpportunity.strfPREFERENCE_NAME].Value;

                                            rstNewOppProductPref.Fields[modOpportunity.strfCODE_].Value
                                                    = rstOppProductPref.Fields[modOpportunity.strfCODE_].Value;

                                            rstNewOppProductPref.Fields[modOpportunity.strfOPPORTUNITY_PRODUCT_ID].Value
                                                    = targetOptionId;
                                            rstNewOppProductPref.Fields[modOpportunity.strf_DIVISION_PRODUCT_PREF_ID].Value
                                                    = rstOppProductPref.Fields[modOpportunity.strf_DIVISION_PRODUCT_PREF_ID].Value;

                                            rstNewOppProductPref.Fields[modOpportunity.strfOP_LOC_ATTR_PREF_ID].Value
                                                    = rstNewAttrPref.Fields[modOpportunity.strfOP_LOC_ATTR_PREF_ID].Value;
                                            objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY_PRODUCT_PREF, rstNewOppProductPref);

                                            rstOppProductPref.MoveNext();
                                        }
                                        //Set the selected Attribute
                                        if (!Convert.IsDBNull(rstOPAttrPref.Fields[modOpportunity.strf_OPPORTUNITY_PRODUCT_PREF_ID].Value))
                                        {
                                            rstNewAttrPref = objLib.GetRecordset(rstNewAttrPref.Fields[modOpportunity.strfOP_LOC_ATTR_PREF_ID].Value,
                                                modOpportunity.strtOPPPROD_ATTR_PREF, modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID);

                                            object vntDivisionProductPrefId = objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY_PRODUCT_PREF,
                                                modOpportunity.strf_DIVISION_PRODUCT_PREF_ID,
                                                rstOPAttrPref.Fields[modOpportunity.strf_OPPORTUNITY_PRODUCT_PREF_ID].Value);
                                            Recordset rstOppProdPrefTarget = objLib.GetRecordset(modOpportunity.strqOPP_PROD_PREF_FOR_OPP_PROD_AND_DIV_PROD,
                                                2, targetOptionId, vntDivisionProductPrefId,
                                                modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID);
                                            if (rstOppProdPrefTarget.RecordCount > 0)
                                            {
                                                rstNewAttrPref.Fields[modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID].Value =
                                                    rstOppProdPrefTarget.Fields[modOpportunity.strfOPPORTUNITY_PRODUCT_PREF_ID].Value;
                                                objLib.SaveRecordset(modOpportunity.strtOPPPROD_ATTR_PREF, rstNewAttrPref);
                                            }
                                        }
                                    }
                                    rstOPAttrPref.MoveNext();
                                }
                            }
                            
                            //BC - Changes for the package Components
                            if (strType == modOpportunity.strsPACKAGE)
                                this.CopyOpportunityProductPackageComponents(sourceOptionId, rstOPLocation.Fields[modOpportunity.strfOPP_PRODUCT_LOCATION_ID].Value, targetOptionId, vntOPLocId);

                            rstOPLocation.MoveNext();
                        }
                    }
                }
                return true;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Can only inactivate an inventory quote when 1. no active customer quotes exists
        /// </summary>
        /// <returns>note as string</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 3.8.0.0  5/12/2006  DYin  Converted to .Net C# code.
        /// </history>
        protected virtual bool CanInactivateInventoryQuote(object opportunityQuoteId)
        {
            try
            {
                if (opportunityQuoteId == DBNull.Value) return false;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                if (TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfSTATUS,
                    opportunityQuoteId)) == modOpportunity.strsINVENTORY)
                {
                    object vntLotId = objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfLOT_ID,
                        opportunityQuoteId);
                    // get customer quotes for lot
                    Recordset rstQuote = objLib.GetRecordset(modOpportunity.strqACTIVE_CUSTOMER_QUOTES_FOR_LOT, 1, vntLotId, modOpportunity.strfOPPORTUNITY_ID);
                    if (rstQuote.RecordCount > 0)
                    {
                        return false;
                    }

                    // Sales (Sales Request, Contract or Closed Contract)
                    rstQuote = objLib.GetRecordset(modOpportunity.strqACTIVE_SALES_FOR_HOMESITE, 1, vntLotId, modOpportunity.strfOPPORTUNITY_ID);
                    if (rstQuote.RecordCount > 0)
                    {
                        return false;
                    }

                    // Lot Configuration exists
                    Recordset rstLot_Config = objLib.GetRecordset(modOpportunity.strqLOT_CONFIG_FOR_LOT, 1, vntLotId, modOpportunity.strfPRODUCT_ID);
                    if (rstLot_Config.RecordCount > 0)
                    {
                        return false;
                    }

                    // Built Plan or Built Elevation
                    if (!(Convert.IsDBNull(objLib.SqlIndex(modOpportunity.strtPRODUCT, modOpportunity.strfPLAN_ID,
                        vntLotId))) || !(Convert.IsDBNull(objLib.SqlIndex(modOpportunity.strtPRODUCT, modOpportunity.strfELEVATION_ID,
                        vntLotId))))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Called in DeleteFormData. Change the quote's homesite type from "Inventory" back to "Homesite"
        /// whenever an inventory quote is deleted.
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="newHomesiteType">New homesite type</param>
        /// <history>
        /// Revision#    Date        Author  Description
        /// 3.8.0.0      5/12/2006   DYin    Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateInventoryHomesiteType(object opportunityId, string newHomesiteType)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                object vntLotId = RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY].Fields[modOpportunity.strfLOT_ID].Index(opportunityId);

                Recordset rstLot = objLib.GetRecordset(vntLotId, modOpportunity.strt_PRODUCT, modOpportunity.strfTYPE);

                if (rstLot.RecordCount > 0)
                {
                    rstLot.Fields[modOpportunity.strfTYPE].Value = newHomesiteType;
                    objLib.PermissionIgnored = true;
                    objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// On Approve of sale the Milestones defined on the release are applied to the contract
        /// </summary>
        /// <param name="quoteOpportunityId">Quote Id</param>
        /// <history>
        /// Revision#    Date        Author  Description
        /// 3.8.0.0      5/12/2006   DYin    Converted to .Net C# code.
        /// </history>
        protected virtual void ApplyReleaseMilestones(object quoteOpportunityId)
        {
            try
            {
                MilestoneItem objMilestone = (MilestoneItem)RSysSystem.ServerScripts[modOpportunity.strsMILESTONE_ITEMS].CreateInstance();
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                if ((quoteOpportunityId != DBNull.Value))
                {
                    object vntReleaseId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strf_NBHD_PHASE_ID,
                        quoteOpportunityId);
                    // get the milestones
                    if ((vntReleaseId != DBNull.Value))
                    {
                        Recordset rstRelMilestones = objLib.GetRecordset(modOpportunity.strqMILESTONES_FOR_REL, 1, vntReleaseId);
                        if (!(rstRelMilestones.EOF))
                        {
                            rstRelMilestones.MoveFirst();
                            while (!(rstRelMilestones.EOF))
                            {
                                objMilestone.CreateContractMilestone(rstRelMilestones.Fields[modOpportunity.strf_MILESTONE_ITEMS_ID].Value,
                                    quoteOpportunityId);
                                rstRelMilestones.MoveNext();
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Updates customer quotes price when additional price for inventory quote changes
        /// </summary>
        /// <param name="opportunityRecordset">Opportunity recordset</param>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 3.8.0.0  5/12/2006  DYin  Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateCustomerQuoteAdditionalPrice(Recordset opportunityRecordset)
        {
            try
            {
                if (!((TypeConvert.ToDecimal(opportunityRecordset.Fields[modOpportunity.strfADDITIONAL_PRICE].Value) == 
                    TypeConvert.ToDecimal(opportunityRecordset.Fields[modOpportunity.strfADDITIONAL_PRICE].OriginalValue))))
                {
                    decimal dblAdditionalPrice = TypeConvert.ToDecimal(opportunityRecordset.Fields[modOpportunity.strfADDITIONAL_PRICE].Value);
                    decimal dblPlanPrice = TypeConvert.ToDecimal(opportunityRecordset.Fields[modOpportunity.strfPRICE].Value);
                    decimal dblNewPrice = dblAdditionalPrice + dblPlanPrice;
                    DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    Recordset rstCustomerQuotes = objLib.GetRecordset(modOpportunity.strqACTIVE_CUSTOMER_QUOTES_FOR_LOT, 1, opportunityRecordset.Fields[modOpportunity.strfLOT_ID].Value,
                        modOpportunity.strfPRICE, modOpportunity.strfCONFIGURATION_CHANGED, modOpportunity.strfDESCRIPTION);
                    if (rstCustomerQuotes.RecordCount > 0)
                    {
                        rstCustomerQuotes.MoveFirst();
                        while(!(rstCustomerQuotes.EOF))
                        {
                            rstCustomerQuotes.Fields[modOpportunity.strfPRICE].Value = dblNewPrice;
                            rstCustomerQuotes.Fields[modOpportunity.strfCONFIGURATION_CHANGED].Value = true;
                            rstCustomerQuotes.Fields[modOpportunity.strfDESCRIPTION].Value = TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdINVENTORY_MODIFIED_START,
                                new object[] {DateTime.Today})) + TypeConvert.ToString(LangDict.GetText(modOpportunity.strdINVENTORY_ADDITIONAL_PRICE_CHANGE))
                                +
                                "\r\n" + "\r\n" + rstCustomerQuotes.Fields[modOpportunity.strfDESCRIPTION].Value;
                            rstCustomerQuotes.MoveNext();
                        }
                        objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstCustomerQuotes);
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will delete Quote sales team
        /// </summary>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void DeleteTeam(object opportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.DeleteRecordset(modOpportunity.strq_OPPORTUNITY_TEAM_MEMBER_OF_OPPORTUNITY_ID, modOpportunity.strfOPPORTUNITY_TEAM_MEMBER_ID, opportunityId);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will delete options on a Quote
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual void DeleteOptions(object opportunityQuoteId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOptions = objLib.GetRecordset(modOpportunity.strqOPP_PRODUCTS_FOR_OPP, 1, opportunityQuoteId, modOpportunity.strf_OPPORTUNITY__PRODUCT_ID);
                if (rstOptions.RecordCount > 0)
                {
                    rstOptions.MoveFirst();
                    IRForm rfrmForm = RSysSystem.Forms[modOpportunity.strrHB_OPPORTUNITY_PRODUCT];
                    object parameterList = DBNull.Value;
                    while (!(rstOptions.EOF))
                    {
                        rfrmForm.DeleteFormData(rstOptions.Fields[modOpportunity.strf_OPPORTUNITY__PRODUCT_ID].Value,
                            ref parameterList);
                        rstOptions.MoveNext();
                    }
                }
                rstOptions.Close();
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function is in high level, and it will call GetQuoteOptionPrice to get the price for the RE-SELECTED option.
        /// If a brand new option was selected then CreateOpportunityProductOption function will determine the price.
        /// This function will also be called in Envision integration method UpdateBuyerSelections
        /// </summary>
        /// <param name="opportunityProductId">Opportunity Product Id</param>
        /// <param name="price">price</param>
        /// <param name="usePostCutoffPrice">usePostCutoffPrice</param>
        /// <param name="built">built</param>
        /// <returns>true: it is regular option, i.e. division_product_ID is defined 
        ///          false: it is custom option, i.e. division_product_ID is not defined 
        /// </returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 5.9.0.0        May/01/2007    JWang     Get the re-selected option price.
        /// </history>
        protected virtual bool GetReSelectedOptionPriceAndBuiltInfo(object opportunityProductId, out decimal price, out bool usePostCutoffPrice, out bool built)
        {
            try
            {
                object vntDivProductId = RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY__PRODUCT].Fields[modOpportunity.strfDIVISION_PRODUCT_ID].Index(opportunityProductId);
                object nbhdpProductId = RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY__PRODUCT].Fields[modOpportunity.strfNBHDP_PRODUCT_ID].Index(opportunityProductId);
                if (!Convert.IsDBNull(vntDivProductId))
                {
                    string strConstructionStageComparison = GetConstructionStageComparison();
                    object psqId = RSysSystem.Tables[modOpportunity.strt_OPPORTUNITY__PRODUCT].Fields[modOpportunity.strfOPPORTUNITY_ID].Index(opportunityProductId);
                    object contractId = RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfPOST_SALE_ID].Index(psqId);

                    object vntHomesiteID = RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfLOT_ID].Index(contractId);


                    if ((!Convert.IsDBNull(RSysSystem.Tables[modOpportunity.strtPRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntHomesiteID)))
                        && (!Convert.IsDBNull(RSysSystem.Tables[modOpportunity.strtDIVISION_PRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntDivProductId))))
                    {
                        object vntHomesiteConstructionStageId = RSysSystem.Tables[modOpportunity.strtPRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntHomesiteID);
                        int intHomesiteConstructionStageOrdinal = (int)RSysSystem.Tables[modOpportunity.strtCONSTRUCTION_STAGE].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Index(vntHomesiteConstructionStageId);
                        object vntOptionConstructionStageId = RSysSystem.Tables[modOpportunity.strtDIVISION_PRODUCT].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Index(vntDivProductId);
                        int intOptionConstructionStageOrdinal = (int)RSysSystem.Tables[modOpportunity.strtCONSTRUCTION_STAGE].Fields[modOpportunity.strfCONSTRUCTION_STAGE_ORDINAL].Index(vntOptionConstructionStageId);
                        if ((strConstructionStageComparison == modOpportunity.strsGREATER_THAN && intHomesiteConstructionStageOrdinal > intOptionConstructionStageOrdinal) ||
                            (strConstructionStageComparison == modOpportunity.strsGREATER_THAN_OR_EQUAL_TO && intHomesiteConstructionStageOrdinal >= intOptionConstructionStageOrdinal))
                        {
                            usePostCutoffPrice = true;
                            built = true;
                        }
                        else
                        {
                            usePostCutoffPrice = false;
                            built = false;
                        }
                    }
                    else
                    {
                        usePostCutoffPrice = false;
                        built = false;
                    }
                    price = GetQuoteOptionPrice(psqId, nbhdpProductId, opportunityProductId, contractId, usePostCutoffPrice);
                    return true;
                }
                else
                {
                    //C# does not allow leaving out parameters undefind, and all premitive type cannot be set null.
                    usePostCutoffPrice = false;
                    built = false;
                    price = 0;
                    return false;
                }

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function figures out the price of options based on certain conditions of the quote.
        /// a. Initial Population of Options use "Build Option Pricing"
        /// Fixed    --> use prices on Opp Product
        /// Floating --> use NBHDP Product current Price
        /// b. Additional Option Selection --> use NBHD Product current Price
        /// Fixed    --> use price as of the Sales Request date and in the case of the Post Sale use the sale date of
        /// the contract
        /// Floating --> use NBHD Product current price
        /// </summary>
        /// <param name="quoteOpportunityId"></param>
        /// <param name="neighborhoodPhaseProductId">Neighborhood Phase Product Id</param>
        /// <param name="opportunityProductId">Opportunity Product Id</param>
        /// <param name="contractId">Contract Id is used with Post-Sale Quote</param>
        /// <returns>Quote option price </returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 5.9.0.0       Mar/06/2007      YK       Initial Version.
        /// </history>
        protected virtual decimal GetQuoteOptionPrice(object quoteOpportunityId, object neighborhoodPhaseProductId,
            object opportunityProductId, object contractId)
        {
            try
            {
                return GetQuoteOptionPrice(quoteOpportunityId, neighborhoodPhaseProductId, opportunityProductId, contractId, false);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        
        /// <summary>
        /// This function figures out the price of options based on certain conditions of the quote.
        /// a. Initial Population of Options use "Build Option Pricing"
        /// Fixed    --> use prices on Opp Product
        /// Floating --> use NBHDP Product current Price
        /// b. Additional Option Selection --> use NBHD Product current Price
        /// Fixed    --> use price as of the Sales Request date and in the case of the Post Sale use the sale date of
        /// the contract
        /// Floating --> use NBHD Product current price
        /// </summary>
        /// <param name="quoteOpportunityId"></param>
        /// <param name="neighborhoodPhaseProductId">Neighborhood Phase Product Id</param>
        /// <param name="opportunityProductId">Opportunity Product Id</param>
        /// <param name="contractId">Contract Id is used with Post-Sale Quote</param>
        /// <param name="blnUsePCOPrice">True-If the Post Cut Off Price needs to be returned, False for the regular price</param>
        /// <returns>Quote option price </returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// 5.9.0.0        2/26/2007      BC        Changed the code to handle the Post Cutt Off Price
        /// 5.9.0.0        mar/06/2007    YK        Adding another paramter to get appropriate results.
        /// </history>
        protected virtual decimal GetQuoteOptionPrice(object quoteOpportunityId, object neighborhoodPhaseProductId, 
            object opportunityProductId, object contractId, bool blnUsePCOPrice)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                string vntPipelineStage = string.Empty;
                bool blnBuildOption = false;
                DateTime vntCreateDate = TypeConvert.ToDateTime(DBNull.Value);
                DateTime vntContractApprovedSubmitted = TypeConvert.ToDateTime(DBNull.Value);
                bool blnSelectedOption = false;
                object vntLot_Id = DBNull.Value;
                string vntStatus = string.Empty;
                int vntBuildOption = 0;
                bool netConfig = false;
                bool blnPCOUsed = false;
                StandardOptionPricing vntStndOption = StandardOptionPricing.Fixed;
                if ((quoteOpportunityId != DBNull.Value))
                {
                    Recordset rstOpportunity = objLib.GetRecordset(quoteOpportunityId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfPIPELINE_STAGE,
                        modOpportunity.strfLOT_ID, modOpportunity.strfSTATUS, modOpportunity.strfCONTRACT_APPROVED_SUBMITTED);
                    if (!(rstOpportunity.EOF))
                    {
                        rstOpportunity.MoveFirst();
                        vntPipelineStage = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfPIPELINE_STAGE].Value);
                        vntLot_Id = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                        vntStatus = TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfSTATUS].Value);
                        object vntDivisionId = objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfDIVISION_ID,
                            neighborhoodPhaseProductId);
                        vntBuildOption = TypeConvert.ToInt32(objLib.SqlIndex(modOpportunity.strt_DIVISION, modOpportunity.strfBUILD_OPTION_PRICING,
                            vntDivisionId));
                        vntCreateDate = TypeConvert.ToDateTime(objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfQUOTE_CREATE_DATE,
                            quoteOpportunityId));
                        vntStndOption = (StandardOptionPricing) TypeConvert.ToInt32(objLib.SqlIndex(modOpportunity.strt_DIVISION, modOpportunity.strfSTANDARD_OPTION_PRICING,
                            vntDivisionId));
                        vntContractApprovedSubmitted = TypeConvert.ToDateTime(objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfCONTRACT_APPROVED_SUBMITTED,
                            quoteOpportunityId));

                    }

                    // if called when loading the option configuration form either the NBHD Product id or the OPP product
                    // Id will be null
                    if (RSysSystem.IdToString(opportunityProductId) == "0x0000000000000000")
                    {
                        // this is a nbhd product
                    }
                    else
                    {
                        Recordset rstOption = objLib.GetRecordset(opportunityProductId, modOpportunity.strtOPPORTUNITY__PRODUCT,
                            modOpportunity.strfSELECTED, modOpportunity.strfBUILD_OPTION, modOpportunity.strfNET_CONFIG, 
                            modOpportunity.strfUSE_POST_CUTOFF_PRICE);
                        //blnSelectedOption = rstOption.Fields(strfSELECTED).value
                        //blnBuildOption = rstOption.Fields(strfBUILD_OPTION).value
                        blnSelectedOption = TypeConvert.ToBoolean(rstOption.Fields[modOpportunity.strfSELECTED].Value);
                        blnBuildOption = TypeConvert.ToBoolean(rstOption.Fields[modOpportunity.strfBUILD_OPTION].Value);
                        netConfig = TypeConvert.ToBoolean(rstOption.Fields[modOpportunity.strfNET_CONFIG].Value);
                        blnSelectedOption = blnBuildOption ? blnSelectedOption : false;
                        blnPCOUsed = TypeConvert.ToBoolean(rstOption.Fields[modOpportunity.strfUSE_POST_CUTOFF_PRICE].Value);
                    }
                }

                // added by TL 10/24/2005
                if (!((neighborhoodPhaseProductId != DBNull.Value)))
                {
                    // is custom option
                    // Set rstCustomOption = objLib.GetDynRecordsetById(vntOpportunityProductId, strtOPPORTUNITY__PRODUCT, rdstCustomOption, strf_PRICE, strfNET_CONFIG)
                    Recordset rstCustomOption = objLib.GetRecordset(opportunityProductId, modOpportunity.strtOPPORTUNITY__PRODUCT,
                        modOpportunity.strf_PRICE);
                    if (rstCustomOption.RecordCount > 0)
                    {
                        rstCustomOption.MoveFirst();
                        return TypeConvert.ToDecimal(rstCustomOption.Fields[modOpportunity.strf_PRICE].Value);
                    }
                }

                bool blnEvalPrice = false;
                DateTime vntOptionPriceDate = TypeConvert.ToDateTime(DBNull.Value);
                switch (vntPipelineStage)
                {
                    case modOpportunity.strPIPELINE_QUOTE:
                        string vntLot_Type = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_PRODUCT, modOpportunity.strfTYPE,
                            vntLot_Id));
                        // two situations
                        if ((vntStatus != modOpportunity.strQUOTE_STATUS_INVENTORY) && 
                            (vntLot_Type != modOpportunity.strLOT_TYPE_INVENTORY))
                        {
                            // is always the current price of the Option based on the quote
                            blnEvalPrice = false;
                        }
                        else if (vntLot_Type == modOpportunity.strLOT_TYPE_INVENTORY && (vntStatus
                            == modOpportunity.strQUOTE_STATUS_INVENTORY || vntStatus == modOpportunity.strQUOTE_STATUS_IN_PROGRESS
                            || vntStatus == modOpportunity.strQUOTE_STATUS_RESERVED))
                        {
                            // depends on division settings
                            if (vntBuildOption.Equals(modOpportunity.intBUILD_OPTION_FIXED))
                            {
                                // fixed, only update if not built
                                if (!blnBuildOption)
                                {
                                    blnEvalPrice = true;
                                }
                            }
                            else if (vntBuildOption.Equals(modOpportunity.intBUILD_OPTION_FLOATING))
                            {
                                // floating, always update
                                blnEvalPrice = true;
                            }
                        }
                        break;
                    case modOpportunity.strPIPELINE_SALES_REQUEST:
                        // Get additional criteria, option price is frozen at the sales request date
                        vntOptionPriceDate = TypeConvert.ToDateTime(objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfCONTRACT_APPROVED_SUBMITTED_DATETIME,
                            quoteOpportunityId));
                        //if (blnSelectedOption)
                        {
                            // depends if the option was previously selected or is new
                            if ((opportunityProductId != DBNull.Value))
                            {
                                if (netConfig)
                                {
                                    // Already existing option
                                    blnEvalPrice = true;
                                    // always figure out the sales request date price
                                }
                            }
                            if (!blnEvalPrice) 
                            {
                                // new option
                                // depends on division settings
                                if (modOpportunity.intSTANDARD_OPTION_FIXED.Equals(vntStndOption))
                                {
                                    // fixed, get price dependent on date
                                    blnEvalPrice = true;
                                }
                                else if (modOpportunity.intSTANDARD_OPTION_FLOATING.Equals(vntStndOption))
                                {
                                    // floating, use current price
                                }
                            }
                        }
                        break;
                    case modOpportunity.strsPOST_SALE:
                        // Get additional criteria, option price is frozen at the sales request date
                        vntOptionPriceDate = TypeConvert.ToDateTime(objLib.SqlIndex(modOpportunity.strt_OPPORTUNITY, modOpportunity.strfCONTRACT_APPROVED_SUBMITTED_DATETIME,
                            quoteOpportunityId));
                        //if (blnSelectedOption)
                        {
                            // depends if the option was previously selected or is new
                            if ((opportunityProductId != DBNull.Value))
                            {
                                if (netConfig)
                                {
                                    // Already existing option
                                    blnEvalPrice = true;
                                    // always figure out the sales request date price
                                }
                            }
                            if (!blnEvalPrice)
                            {
                                // new option
                                // depends on division settings
                                if (modOpportunity.intSTANDARD_OPTION_FIXED.Equals(vntStndOption))
                                {
                                    // fixed, get price dependent on date
                                    blnEvalPrice = true;
                                }
                                else if (modOpportunity.intSTANDARD_OPTION_FLOATING.Equals(vntStndOption))
                                {
                                    // floating, use current price
                                }
                            }
                        }
                        break;
                    case modOpportunity.strPIPELINE_CANCELED:
                        // Never gets updated
                        break;
                    case modOpportunity.strPIPELINE_CLOSED:
                        // Never gets updated
                        break;
                }

                if (blnEvalPrice)
                    // have to get the price based on a certain date
                    return GetOptionPrice(neighborhoodPhaseProductId, false, vntOptionPriceDate, blnUsePCOPrice);
                else
                    // price is what the option price is
                    return GetOptionPrice(neighborhoodPhaseProductId, true, vntOptionPriceDate, blnUsePCOPrice);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function figures out the price of options based on certain dates
        /// </summary>
        /// <param name="neighborhoodPhaseProductId"></param>
        /// <param name="current">Flag to indicate whether get the current option price or price based on a date</param>
        /// <param name="optionPriceDate">Date for the price</param>
        /// <returns>Option price</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual decimal GetOptionPrice(object neighborhoodPhaseProductId, bool current, DateTime optionPriceDate)
        {
            try
            {
                return GetOptionPrice(neighborhoodPhaseProductId, current, optionPriceDate, false);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function figures out the price of options based on certain dates
        /// </summary>
        /// <param name="neighborhoodPhaseProductId"></param>
        /// <param name="current">Flag to indicate whether get the current option price or price based on a date</param>
        /// <param name="optionPriceDate">Date for the price</param>
        /// <param name="blnPCOUsed">boolean to check if the PCO is used</param>
        /// <returns>Option price</returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// 3.8.0.0        5/12/2006      DYin      Converted to .Net C# code.
        /// </history>
        protected virtual decimal GetOptionPrice(object neighborhoodPhaseProductId, bool current, DateTime optionPriceDate, bool blnPCOUsed)
        {
            try
            {
                if (current)
                {
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    if (blnPCOUsed == false)
                        return TypeConvert.ToDecimal(objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfCURRENT_PRICE,
                            neighborhoodPhaseProductId));
                    else
                        return TypeConvert.ToDecimal(objLib.SqlIndex(modOpportunity.strt_NBHD_PRODUCT, modOpportunity.strfPOST_CUTTOFF_PRICE,
                            neighborhoodPhaseProductId));
                }
                else
                {
                    return GetOptionFixedPrice(neighborhoodPhaseProductId, optionPriceDate, blnPCOUsed);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Encode an XML string
        /// </summary>
        /// <param name="xmlText">string to be encoded</param>
        /// <returns>encoded XML string</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        internal static string EncodeXML(string xmlText) 
        {
            try 
            {
                string strMyXML = xmlText;
                if (strMyXML.Contains("&"))
                {
                    // test to see if someone has already included these escapes in the string
                    strMyXML = strMyXML.Replace("&lt;", "<");
                    strMyXML = strMyXML.Replace("&gt;", ">");
                    strMyXML = strMyXML.Replace("&quot;", @"""");
                    strMyXML = strMyXML.Replace("&apos;", "'");
                    // if not, these are real apostrophes that need to be converted
                    strMyXML = strMyXML.Replace("&", "&amp;");
                }
                // just replace the other occurrances if they exist
                strMyXML = strMyXML.Replace("<", "&lt;");
                strMyXML = strMyXML.Replace(">", "&gt;");
                strMyXML = strMyXML.Replace(@"""", "&quot;");
                strMyXML = strMyXML.Replace("'", "&apos;");
                // WARNING: On Error Goto 0 is not supported
                return strMyXML;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc);
            }
        }

        /// <summary>
        /// Decode any XML that has been encoded
        /// </summary>
        /// <param name="xmlText">the XML to be decoded</param>
        /// <returns>decoded XML string</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/12/2006     DYin       Converted to .Net C# code.
        /// </history>
        internal static string DecodeXml(string xmlText) 
        {
            try 
            {
                string strMyXML = xmlText;
                strMyXML = strMyXML.Replace("&lt;", "<");
                strMyXML = strMyXML.Replace("&gt;", ">");
                strMyXML = strMyXML.Replace("&amp;", "&");
                strMyXML = strMyXML.Replace("&quot;", @"""");
                strMyXML = strMyXML.Replace("&apos;", "'");
                return strMyXML;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc);
            }
        }

        /// <summary>
        /// This function checks to see if a secondary exists.
        /// </summary>
        /// <param name="formName">form Name</param>
        /// <param name="recordsets">Form collection</param>
        /// <param name="segmentName">Section name</param>
        /// <returns>True if a secondary was found, False if no secondary was found</returns>
        protected virtual bool SecondaryExists(string formName, object recordsets, string segmentName)
        {
            Recordset rstForm_Secondary = RSysSystem.Forms[formName].SecondaryFromVariantArray(recordsets, segmentName);
            return (rstForm_Secondary.RecordCount > 0);
        }

        /// <summary>
        /// Check if there is a post sale quote for the specified contract
        /// </summary>
        /// <param name="contractId">contractId</param>
        /// <returns>bool - True if find PSQ
        ///               - False otherwise
        /// </returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// HB5.9          6/08/2007      JWang     Initial version
        /// </history>
        protected virtual bool IsTherePSQ(object contractId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset postSaleQuoteRecordset = objLib.GetRecordset(modOpportunity.strqACTIVE_POST_SALE_QUOTES_FOR_OPP
                    , 2, contractId, modOpportunity.strsPOST_SALE, modOpportunity.strf_OPPORTUNITY_ID);

                if (postSaleQuoteRecordset.RecordCount > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }


        /// <summary>
        /// Do validation for the single option.
        /// Check if this option has no location info provided but referencing "All Locations" product configurations.
        /// Check if this option has dulpcate locations.
        /// This function should be called before making a single option built.
        /// </summary>
        /// <param name="oppProdId">oppProdId</param>
        /// <returns>string - Massage showing validation result; empty if validation succeeds.
        /// </returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// HB5.9          08/14/2007     JWang     Initial version
        /// </history>
        protected virtual string SingleOptionValidation(object oppProdId)
        {
            try
            {
                //check if location is missing
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset oppProdRecordset = objLib.GetRecordset(modOpportunity.strqSINGLE_OPTION_REFERENCING_ALL_LOCATIONS_WITHOUT_LOCATION_DEFINED_FOR_OPTION
                    , 1, oppProdId, modOpportunity.strfPRODUCT_NAME);

                if (oppProdRecordset.RecordCount > 0)
                {
                    oppProdRecordset.MoveFirst();
                    return TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdOPTION_NEEDS_LOCATION, new object[] { TypeConvert.ToString(oppProdRecordset.Fields[modOpportunity.strfPRODUCT_NAME].Value) }));
                }

                //check if duplicate locations
                oppProdRecordset = objLib.GetRecordset(modOpportunity.strqSINGLE_OPTION_WITH_DUPLICATE_LOCATIONS_FOR_OPTION
                    , 1, oppProdId, modOpportunity.strfPRODUCT_NAME);

                if (oppProdRecordset.RecordCount > 0)
                {
                    oppProdRecordset.MoveFirst();
                    return TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdOPTIONS_WITH_DUPLICATE_LOCATIONS, new object[] { TypeConvert.ToString(oppProdRecordset.Fields[modOpportunity.strfPRODUCT_NAME].Value) }));
                }

                return string.Empty;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }



        /// <summary>
        /// For the inventory homesite find the corresponding inventory quote and do validation for the options
        /// Check if there are options with no location info provided but referencing "All Locations" product configurations.
        /// This function should be called before changing the construction stage of an inventory homesite.
        /// </summary>
        /// <param name="homesiteId">HomesiteId</param>
        /// <returns>string - Massage showing validation result; empty if validation succeeds.
        /// </returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// HB5.9          08/14/2007      JWang     Initial version
        /// </history>
        protected virtual string InventoryHomeOptionValidation(object homesiteId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset oppRecordset = objLib.GetRecordset(modOpportunity.strqINVENTORY_QUOTE_FOR_INVENTORY_HOME
                    , 1, homesiteId, modOpportunity.strf_OPPORTUNITY_ID);

                if (oppRecordset.RecordCount > 0)
                {
                    oppRecordset.MoveFirst();
                    object oppId = oppRecordset.Fields[modOpportunity.strf_OPPORTUNITY_ID].Value;
                    string validationResult;
                    if ((validationResult = OptionNeedsLocation(oppId)) != string.Empty)
                        return validationResult;
                    if ((validationResult = OptionsWithDuplicateLocations(oppId)) != string.Empty)
                        return validationResult;
                    return string.Empty;
                }
                else
                {
                    return TypeConvert.ToString(LangDict.GetText(modOpportunity.strdCANNOT_FIND_INVENTORY_QUOTE));
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

    
    
        /// <summary>
        /// Check if there are options with no location info provided but referencing "All Locations" product configurations.
        /// This function should be called before converting the quote to contract if the related division is integrated with Envision.
        /// </summary>
        /// <param name="oppId">oppId</param>
        /// <returns>string - Massage showing list of options which need location info; empty if no options missing locations.
        /// </returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// HB5.9          6/08/2007      JWang     Initial version
        /// </history>
        protected virtual string OptionNeedsLocation(object oppId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset oppProdRecordset = objLib.GetRecordset(modOpportunity.strqOPP_PRODUCTS_REFERENCING_ALL_LOCATIONS_WITHOUT_LOCATION_DEFINED_FOR_OPP
                    , 1, oppId, modOpportunity.strfPRODUCT_NAME);

                if (oppProdRecordset.RecordCount > 0)
                {
                    oppProdRecordset.MoveFirst();
                    string optionList=string.Empty;
                    while (!(oppProdRecordset.EOF))
                    {
                        optionList += TypeConvert.ToString(oppProdRecordset.Fields[modOpportunity.strfPRODUCT_NAME].Value)+", ";
                        oppProdRecordset.MoveNext();
                    }
                    return TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdOPTION_NEEDS_LOCATION, new object[] { optionList.Substring(0, optionList.Length - 2) })); //get rid of the last delimiter ", "
                }
                else
                    return string.Empty;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Check if there are options with duplicate locations info provided.
        /// One location can only select the same option once.
        /// This function should be called before converting the quote to contract if the related division is integrated with Envision.
        /// </summary>
        /// <param name="oppId">oppId</param>
        /// <returns>string - Message showing list of options with duplicate locations info; empty string if no duplicates found.
        /// </returns>
        /// <history>
        /// Revision       Date           Author    Description
        /// HB5.9          8/01/2007      JWang     Initial version
        /// </history>
        protected virtual string OptionsWithDuplicateLocations(object oppId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset oppProdRecordset = objLib.GetRecordset(modOpportunity.strqOPTIONS_WITH_DUPLICATE_LOCATIONS_FOR_OPP
                    , 1, oppId, modOpportunity.strfPRODUCT_NAME);

                if (oppProdRecordset.RecordCount > 0)
                {
                    oppProdRecordset.MoveFirst();
                    string optionList = string.Empty;
                    while (!(oppProdRecordset.EOF))
                    {
                        optionList += TypeConvert.ToString(oppProdRecordset.Fields[modOpportunity.strfPRODUCT_NAME].Value) + ", ";
                        oppProdRecordset.MoveNext();
                    }
                    return TypeConvert.ToString(LangDict.GetTextSub(modOpportunity.strdOPTIONS_WITH_DUPLICATE_LOCATIONS, new object[] { optionList.Substring(0, optionList.Length - 2) })); //get rid of the last delimiter ", "
                }
                else
                    return string.Empty;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }


        /// <summary>
        /// This funtion will write all contract changes into the TIC_INT_SAM_Contract table.
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 1.0          08/05/2010   CMigles  Initial Version
        /// </history>
        public virtual object WriteContractHistoryRecords(object vntLotId, object vntOpportunityId, string strLotStatus,
                            DateTime dtDateOfBusinessTransaction, Boolean blnCausedBySale, object vntTransferToLotId, 
                            Boolean blnNewLot, Boolean blnLotBatchUpdate)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();

                if (vntOpportunityId != null)
                {
                    //get the Quote Contract's Status 
                    string vntQuoteContractStatus = TypeConvert.ToString(RSysSystem.Tables[modOpportunity.strtOPPORTUNITY].Fields[modOpportunity.strfSTATUS].Index(vntOpportunityId));
                }

                //Set contract's fields array
                object arrTableFields = new object[] 
                {
                    modOpportunity.strfTIC_INT_SAM_CONTRACT_ID,
                    modOpportunity.strfSTATUS_CHANGE_NUMBER,
                    modOpportunity.strfDATE_OF_BUS_TRANSACTION, 
                    modOpportunity.strfPRODUCT_ID, 
                    modOpportunity.strfCAUSED_BY_SALE, 
                    modOpportunity.strfOPPORTUNITY_ID,
                    modOpportunity.strfCOMMENTS, 
                    modOpportunity.strfCHANGED_BY, 
                    modOpportunity.strfCHANGED_ON, 
                    modOpportunity.strfLOT_STATUS_CHANGED_TO,
                    modOpportunity.strfTRANSFER_FROM_LOT_ID,
                    modOpportunity.strfTRANSFER_TO_LOT_ID,
                    modOpportunity.strfSALES_VALUE
                };

                //WRITE new record to the TIC_INT_SAM_Contract
                Recordset rstNewContractHistory = objLib.GetNewRecordset(modOpportunity.strtTIC_INT_SAM_CONTRACT, arrTableFields);

                rstNewContractHistory.AddNew( Type.Missing, Type.Missing);
                rstNewContractHistory.Fields[modOpportunity.strfSTATUS_CHANGE_NUMBER].Value = this.GetNextStatusChangeNumberForLot (vntLotId);
                rstNewContractHistory.Fields[modOpportunity.strfPRODUCT_ID].Value = vntLotId;
                if (vntOpportunityId != null)
                {
                    rstNewContractHistory.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = vntOpportunityId;
                }
                rstNewContractHistory.Fields[modOpportunity.strfCHANGED_ON].Value = DateTime.Now;
                rstNewContractHistory.Fields[modOpportunity.strfLOT_STATUS_CHANGED_TO].Value = strLotStatus;
                rstNewContractHistory.Fields[modOpportunity.strfDATE_OF_BUS_TRANSACTION].Value = dtDateOfBusinessTransaction;
                rstNewContractHistory.Fields[modOpportunity.strfCAUSED_BY_SALE].Value = blnCausedBySale;
                object vntCurrentEmployee = administration.CurrentUserRecordId;
                if (vntCurrentEmployee != null)
                {
                    string strEmployeeDesc = TypeConvert.ToString ( objLib.SqlIndex(modOpportunity.strtEMPLOYEE, modOpportunity.strfRN_DESCRIPTOR, vntCurrentEmployee));
                    rstNewContractHistory.Fields[modOpportunity.strfCHANGED_BY].Value = strEmployeeDesc ;
                }
                if (strLotStatus == modOpportunity.strsTRANSFER_SALE || strLotStatus == modOpportunity.strsTRANSFER_RESERVE)
                {
                    if (blnNewLot == false)
                    {
                        rstNewContractHistory.Fields[modOpportunity.strfTRANSFER_FROM_LOT_ID].Value = vntLotId;
                        rstNewContractHistory.Fields[modOpportunity.strfTRANSFER_TO_LOT_ID].Value = vntTransferToLotId;
                    }
                    else
                    {
                        if (blnNewLot == true) 
                        {
                            rstNewContractHistory.Fields[modOpportunity.strfTRANSFER_FROM_LOT_ID].Value = vntTransferToLotId;
                            rstNewContractHistory.Fields[modOpportunity.strfTRANSFER_TO_LOT_ID].Value = vntLotId;
                        }
                    }

                }

                switch (strLotStatus)
                {
                    case modOpportunity.strsRESERVED:
                        rstNewContractHistory.Fields[modOpportunity.strfSALES_VALUE].Value = 1;
                        break;

                    case modOpportunity.strsSOLD:
                        rstNewContractHistory.Fields[modOpportunity.strfSALES_VALUE].Value = 1;
                        break;
                    case modOpportunity.strsCLOSED:
                        rstNewContractHistory.Fields[modOpportunity.strfSALES_VALUE].Value = 0;
                        break;
                    case modOpportunity.strsCANCELLED_RESERVED:
                        rstNewContractHistory.Fields[modOpportunity.strfSALES_VALUE].Value = -1;
                        break;
                    case modOpportunity.strsCANCELLED:
                        rstNewContractHistory.Fields[modOpportunity.strfSALES_VALUE].Value = -1;
                        break;
                    case modOpportunity.strsAVAILABLE:
                        rstNewContractHistory.Fields[modOpportunity.strfSALES_VALUE].Value = 0;
                        break;
                    case modOpportunity.strsLOT_STATUS_NOT_RELEASED:
                        rstNewContractHistory.Fields[modOpportunity.strfSALES_VALUE].Value = 0;
                        break;
                    case modOpportunity.strsTRANSFER:
                        rstNewContractHistory.Fields[modOpportunity.strfSALES_VALUE].Value = 0;
                        break;
                    case modOpportunity.strsTRANSFER_SALE:
                        rstNewContractHistory.Fields[modOpportunity.strfSALES_VALUE].Value = 0;
                        break;
                    case modOpportunity.strsTRANSFER_RESERVE:
                        rstNewContractHistory.Fields[modOpportunity.strfSALES_VALUE].Value = 0;
                        break;
                    case modOpportunity.strsROLLBACK:
                        rstNewContractHistory.Fields[modOpportunity.strfSALES_VALUE].Value = 0;
                        break;
                    default:
                        //Do nothing
                        break;
                }

                //SAVE the new recordset.
                objLib.PermissionIgnored = true;
                objLib.SaveRecordset(modOpportunity.strtTIC_INT_SAM_CONTRACT, rstNewContractHistory);

                object vntNewContractHistoryId = rstNewContractHistory.Fields[modOpportunity.strfTIC_INT_SAM_CONTRACT_ID].Value;
                rstNewContractHistory.Close();

                return vntNewContractHistoryId;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }


        /// <summary>
        /// This funtion will send emails on Convert to Sale and Transfer events.
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 1.0          09/15/2010   CMigles  Initial Version
        /// 1.1          11/24/2010   KA       Added additional info to email msg & subject
        /// </history>
        public virtual void SendEmailNotifications(string strNotificationEvent, object vntNeighborhoodId, Recordset rstOpportunity, object vntContactId)
                            
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();

                string strNotify = string.Empty;
                string strSubject = string.Empty;
                string strMsg1 = string.Empty;
                string strMsg2 = string.Empty;
                string strMsg3 = string.Empty;
                object vntCurrentEmployeeId = DBNull.Value;
                string vntCurrentEmployeeFirstName = string.Empty;
                string vntCurrentEmployeeLastName = string.Empty;
                string strCurrentEmployeeName = string.Empty;

                if (strNotificationEvent == modOpportunity.strsCONVERT_TO_SALE)
                {
                    strNotify = modOpportunity.strqNOTIFICATION_ON_CONVERT_TO_SALE;
                    //KA 11/24/10 reseting subject 
                    //strSubject = modOpportunity.strdCONVERT_TO_SALE_SUBJECT;
                    strSubject = "Convert to Sale ";
                    strMsg1 = modOpportunity.strdCONVERT_TO_SALE_MESSAGE1;
                    strMsg2 = modOpportunity.strdCONVERT_TO_SALE_MESSAGE2;
                    strMsg3 = modOpportunity.strdCONVERT_TO_SALE_MESSAGE3;
                }
                else if (strNotificationEvent == modOpportunity.strsRESERVED)
                {
                    strNotify = modOpportunity.strqNOTIFICATION_ON_RESERVATION;
                    //KA 11/24/10 reseting subject 
                    //strSubject = modOpportunity.strdRESERVATION_SUBJECT;
                    strSubject = "Reservation ";
                    strMsg1 = modOpportunity.strdRESERVATION_MESSAGE1;
                    strMsg2 = modOpportunity.strdRESERVATION_MESSAGE2;
                    strMsg3 = modOpportunity.strdRESERVATION_MESSAGE3;
                  
                }

                else 
                {
                    //KA 11/24/10 check notification and use correct subject
                    if (strNotificationEvent == "Transfer Contract")
                    {
                        strSubject = "Transfer Contract ";
                    }
                    else
                    {
                        strSubject = "Transfer Reservation ";
                    }
                    strNotify = modOpportunity.strqNOTIFICATION_ON_TRANSFER;
                    strMsg1 = modOpportunity.strdTRANSFER_MESSAGE1;
                    strMsg2 = modOpportunity.strdTRANSFER_MESSAGE2;
                    strMsg3 = modOpportunity.strdTRANSFER_MESSAGE3;
                }
                
                // get recepient list from neighborhood notifcation team where notify on Sales Approved is true
                Recordset rstEmailTo = objLib.GetRecordset(strNotify, 1, vntNeighborhoodId, modOpportunity.strf_EMPLOYEE_ID);
                
                string strEmailTo = string.Empty;
                if (rstEmailTo.RecordCount > 0)
                {
                    rstEmailTo.MoveFirst();
                    StringBuilder emailToBuilder = new StringBuilder();
                    while (!(rstEmailTo.EOF))
                    {
                        string strWorkEmail = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtEMPLOYEE,
                            modOpportunity.strf_WORK_EMAIL, rstEmailTo.Fields[modOpportunity.strf_EMPLOYEE_ID].Value));
                        // add if not already there
                        if (!emailToBuilder.ToString().Contains(strWorkEmail))
                        {
                            emailToBuilder.Append(strWorkEmail + ";");
                        }
                        rstEmailTo.MoveNext();
                    }
                    // strip out last ;
                    strEmailTo = emailToBuilder.ToString();
                    strEmailTo = strEmailTo.Substring(0, strEmailTo.Length - 1);
                }
                rstEmailTo.Close();

                if (strEmailTo.Trim().Length == 0)
                {
                    return;
                }

                // all language strings are in nbhd_notification_team
                ILangDict lngNBHD_Notification_Team = RSysSystem.GetLDGroup(modOpportunity.strgNBHD_NOTIFICATION_TEAM);

                // find current user id
                vntCurrentEmployeeId = administration.CurrentUserRecordId;
                vntCurrentEmployeeFirstName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_FIRST_NAME,
                    vntCurrentEmployeeId));
                vntCurrentEmployeeLastName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_LAST_NAME,
                    vntCurrentEmployeeId));

                // TODO (DI Yin) strLotDescriptor is never assigned. Temporary code here 
                string strLotDescriptor = string.Empty;
                string strMessage = string.Empty;
                              
                //AM2010.10.14 - Get Neighborhood, Division and Lot for the strLot Descriptor
                string strNeighborhood = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfNAME, vntNeighborhoodId));
                object vntDivisionId = objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfDIVISION_ID, vntNeighborhoodId);
                string strDivision = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtDIVISION, modOpportunity.strfNAME, vntDivisionId));
                object vntLotId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strfLOT_ID, rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value);
                Recordset rstLot = objLib.GetRecordset(vntLotId, modOpportunity.strtPRODUCT, modOpportunity.strfUNIT, modOpportunity.strfTRACT, modOpportunity.strfLOT_NUMBER);
                string strLot = TypeConvert.ToString(rstLot.Fields[modOpportunity.strfLOT_NUMBER].Value);
                string strUnit = TypeConvert.ToString(rstLot.Fields[modOpportunity.strfUNIT].Value);
                string strTract = TypeConvert.ToString(rstLot.Fields[modOpportunity.strfTRACT].Value);
                strLotDescriptor = strDivision + ", " + strNeighborhood + ", T/" + strTract + " L/" + strLot + " U/" + strUnit;

                //KA 11/24/10 redo subject 
                //strSubject = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strSubject,
                //    new object[] {vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, strLotDescriptor,
                //String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)) }));
                strSubject = strSubject + vntCurrentEmployeeFirstName + " " + vntCurrentEmployeeLastName + ", " +
                            strLotDescriptor + " - " + String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value));
                

            // set message
                strMessage = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strMsg1,
                new object[] {DateTime.Today, vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, 
                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_FIRST_NAME, vntContactId)), 
                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_LAST_NAME, vntContactId)), 
                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strfHOME_PHONE, vntContactId)), 
                String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)),
                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value)), 
                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value)) }));

                // TODO (Di Yin) vntJob_Number is never assigned, temporary code here
                int vntJob_Number = 0;
                strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strMsg2,
                    new object[] { vntJob_Number, TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfECOE_DATE].Value) }));

                strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strMsg3,
                    new object[] {vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, 
                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_WORK_EMAIL, vntCurrentEmployeeId)), 
                TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strfWORK_PHONE, vntCurrentEmployeeId))}));

                //KA 11/24/10 adding village/nbdh/tract/lot/unit info infront of the message var
                strMessage = strLotDescriptor + "\n\n" + strMessage;               
                //SEND EMAIL
                SendSimpleMail(strEmailTo, strSubject, strMessage);

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vntProductId"></param>
        /// <returns></returns>
        protected virtual int GetNextStatusChangeNumberForLot(object vntProductId)
        {
            try
            {
                if (vntProductId != null)
                {
                    DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    Recordset rst = new Recordset();
                    StringBuilder sqlText = new StringBuilder();
                    int intResult = 1;

                    sqlText.Append("SELECT ((ISNULL(MAX(Status_Change_Number), 0)) + 1) AS NextStatusChangeNumber ");
                    sqlText.Append("FROM TIC_INT_SAM_Contract ");
                    sqlText.Append("WHERE Product_Id = " + mrsysSystem.IdToString(vntProductId));

                    rst = objLib.GetRecordset(sqlText.ToString());

                    if (rst != null)
                    {
                        if (rst.RecordCount > 0)
                        {
                            rst.MoveFirst();
                            intResult = TypeConvert.ToInt16(rst.Fields["NextStatusChangeNumber"].Value);
                        }
                        rst.Close();
                    }
                    return intResult;
                }
                else
                {
                    throw new PivotalApplicationException("GetNextStatusChangeNumberForLot() - Please supply a non-null Product/Lot Record Id");
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }



        /// <summary>
        /// Returns the Employee.Rn_Descriptor for the current User's Employee record.
        /// Returns empty string if no Employee record available.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetCurrentEmployeeRecordRnDescriptor()
        {
            try
            {
                string strResult = String.Empty;

                if (mrsysSystem.UserProfile.EmployeeId != null)
                {
                    strResult = TypeConvert.ToString(mrsysSystem.Tables[modOpportunity.strtEMPLOYEE].Fields[modOpportunity.strfRN_DESCRIPTOR].FindValue(
                                                     mrsysSystem.Tables[modOpportunity.strtEMPLOYEE].Fields[modOpportunity.strfEMPLOYEE_ID], mrsysSystem.UserProfile.EmployeeId)).Trim();

                }
                return strResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// Returns GetCurrentEmployeeRecordRnDescriptor(), trimming to intLength characters
        /// </summary>
        /// <param name="intLength"></param>
        /// <returns></returns>
        protected virtual string GetCurrentEmployeeRecordRnDescriptor(int intLength)
        {
            try
            {
                string strResult = this.GetCurrentEmployeeRecordRnDescriptor();

                if (!(String.IsNullOrEmpty(strResult)))
                {
                    // If length of Employee.Rn_Descriptor > requested length...
                    if (strResult.Length > intLength)
                    {
                        // ...then trim returned string to requested length
                        strResult = strResult.Substring(0, intLength);
                    }
                }

                // return the result
                return strResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// This function gets  the Escrow record for the Quote/Contract
        /// Inputs :
        /// Opportunity_Id
        /// </summary>
        /// <returns>Escrow Id</returns>
        /// <history>
        /// Recordset of Plans
        /// Revision#  Date         Author      Description
        /// 1.0.0.0    9/01/2010    CMigles     Initial Version

        /// </history>
        protected virtual object GetEscrow(object vntOpportunityId)
        {
            try
            {
                DataAccess objLibr = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstEscrow = objLibr.GetRecordset(modOpportunity.strqTIC_ESCROW_FOR_OPPORTUNITY_ID,1, vntOpportunityId, modOpportunity.strfTIC_ESCROW_ID );

                if (rstEscrow.RecordCount > 0)
                {
                    object vntEscrowId = rstEscrow.Fields[modOpportunity.strfTIC_ESCROW_ID].Value;
                    if (vntEscrowId != null)
                    {
                        return vntEscrowId;
                    }
                }
                return null;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will perform the Rollback process for a sale or a reservation
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 1.0          08/19/2010   CMigles  Initial Version
        /// 1.1          10/14/2010   KA       setting the dtSaleDate Actual Decision date since that's what is being used as sale date in lot/product
        /// 1.2          11/24/2010   KA       changed from calling cancelreservation to CancelTransferRollbackReservation
        /// </history>
        public virtual bool  Rollbacks(object vntOppId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();

                Recordset rstQuoteContract = objLib.GetRecordset(vntOppId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfOPPORTUNITY_ID,
                                            modOpportunity.strfTIC_ROLLBACK, modOpportunity.strfTIC_ROLLBACK_DATE, modOpportunity.strfTIC_TRANSFER,
                                            modOpportunity.strfTIC_TRANSFER_DATE, modOpportunity.strfTIC_ORIGINAL_RESERVATION_DATE, modOpportunity.strfTIC_ORIGINAL_SALE_DATE,
                                            modOpportunity.strfPIPELINE_STAGE, modOpportunity.strfSTATUS, modOpportunity.strfLOT_ID, modOpportunity.strfQUOTE_CREATE_DATE,
                                            modOpportunity.strfRESERVATION_DATE, modOpportunity.strfACTUAL_DECISION_DATE);
                
                if (rstQuoteContract.RecordCount > 0)
                {
                    //GET the Quote Contract's Status 
                    string strQuoteContractStatus = TypeConvert.ToString(rstQuoteContract.Fields[modOpportunity.strfSTATUS].Value);
                    object vntLotId = rstQuoteContract.Fields[modOpportunity.strfLOT_ID].Value;
                    //DateTime dtSaleDate =  TypeConvert.ToDateTime( rstQuoteContract.Fields[modOpportunity.strfQUOTE_CREATE_DATE].Value);
                    DateTime dtSaleDate = TypeConvert.ToDateTime(rstQuoteContract.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value);
                    DateTime dtReservationDate = TypeConvert.ToDateTime(rstQuoteContract.Fields[modOpportunity.strfRESERVATION_DATE].Value);
            
                    if (strQuoteContractStatus == modOpportunity.strsRESERVED)
                    {
                        //UPDATE Opportunity and lot with the Rollback details
                        UpdateRollbackTransferInfo(rstQuoteContract, modOpportunity.strsROLLBACK, vntLotId, strQuoteContractStatus);
                        
                        //PROCESS Cancellation
                        //CancelRequestOrContract(vntOppId,true);
                        //CancelReservation(vntOppId, true);
                        CancelTransferRollbackReservation(vntOppId, true, "Rollback");

                        //WRITE History records.
                        //KA 10/14/10 use current date as bus tran date instead of og res date
                        WriteContractHistoryRecords(vntLotId, vntOppId, modOpportunity.strsROLLBACK_RESERVE, DateTime.Today, false, null, false, false);
                    }
                    else
                    {
                        if (strQuoteContractStatus == modOpportunity.strsIN_PROGRESS)
                        {
                            //UPDATE Opportunity and lot with the Rollback details
                            UpdateRollbackTransferInfo(rstQuoteContract, modOpportunity.strsROLLBACK, vntLotId, strQuoteContractStatus);
                            
                            //Write History records.
                            //KA 10/14/10 use current date as bus tran date instead of og sale date
                            WriteContractHistoryRecords(vntLotId, vntOppId, modOpportunity.strsROLLBACK_SALE, DateTime.Today, false, null, false, false);
                        }
                    }
                }
                return true;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function wilL cancel a reservation
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 1.0          09/23/2010   CMigles  Initial Version
        /// </history>
        public virtual bool ReservationCancellation(object vntOppId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();

                Recordset rstQuoteContract = objLib.GetRecordset(vntOppId, modOpportunity.strtOPPORTUNITY, 
                                        modOpportunity.strfTIC_CANCELLATION_RESERVATION_DATE, modOpportunity.strfLOT_ID);

                if (rstQuoteContract.RecordCount > 0)
                {
                    object vntLotId = rstQuoteContract.Fields[modOpportunity.strfLOT_ID].Value;

                    //SET RESERVATION CANCELLATION DATE
                    rstQuoteContract.Fields[modOpportunity.strfTIC_CANCELLATION_RESERVATION_DATE].Value = DateTime.Today;
                    objLib.PermissionIgnored = true;
                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstQuoteContract);
                    rstQuoteContract.Close();

                    //PROCESS Cancellation
                    //CancelRequestOrContract(vntOppId, true);
                    CancelReservation(vntOppId, true);

                    //WRITE HISTORY RECORDS, CANCELLATION RESERVATION
                    WriteContractHistoryRecords(vntLotId, vntOppId, modOpportunity.strsAVAILABLE, DateTime.Today, false, null, false, false);

                }
                return true;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will update opportunity and lot with the rollback or transfer info
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 1.0          08/19/2010   CMigles  Initial Version
        /// 1.1         10/14/2010    KA       changed the original sale date to buyer offer date rather than quote create date
        ///                                     since that's the sale date that gets passed into the Lot/Product record when convert to sale occurs
        /// </history>
        public virtual void UpdateRollbackTransferInfo(Recordset rstQuoteContract, string strcase, object vntLotId, string strQuoteContractStatus)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();

                //TRANFER FROM LOT ID
                Recordset rstLot = objLib.GetRecordset(vntLotId, modOpportunity.strtPRODUCT, modOpportunity.strfTIC_ROLLBACK, modOpportunity.strfTIC_ROLLBACK_DATE, modOpportunity.strfTIC_TRANSFER,
                                            modOpportunity.strfTIC_TRANSFER_DATE, modOpportunity.strfTIC_ORIGINAL_RESERVATION_DATE, modOpportunity.strfTIC_ORIGINAL_SALE_DATE, 
                                            modOpportunity.strfLOT_STATUS, modOpportunity.strfTYPE, modOpportunity.strfTIC_TRANSFER_FROM_LOT_ID, modOpportunity.strfTIC_TRANSFER_TO_LOT_ID, modOpportunity.strfRESERVATION_CONTRACT_ID,
                                            modOpportunity.strfSALES_DATE, modOpportunity.strfRESERVED_DATE);
                
                if (rstQuoteContract.RecordCount > 0)
                {
                    //ROLLBACK
                    if (strcase  == modOpportunity.strsROLLBACK)
                    {
                        //UPDATE the Opportunity record with the Rollback details.
                        rstQuoteContract.Fields[modOpportunity.strfTIC_ROLLBACK].Value = true;
                        rstQuoteContract.Fields[modOpportunity.strfTIC_ROLLBACK_DATE].Value = DateTime.Today;
                        //rstQuoteContract.Fields[modOpportunity.strfTIC_ORIGINAL_SALE_DATE].Value = rstQuoteContract.Fields[modOpportunity.strfQUOTE_CREATE_DATE].Value;
                        rstQuoteContract.Fields[modOpportunity.strfTIC_ORIGINAL_SALE_DATE].Value = rstQuoteContract.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value;
                        rstQuoteContract.Fields[modOpportunity.strfTIC_ORIGINAL_RESERVATION_DATE].Value = rstQuoteContract.Fields[modOpportunity.strfRESERVATION_DATE].Value;


                        //Rollback from a Sale to a Reservation
                        if (strQuoteContractStatus == modOpportunity.strsIN_PROGRESS)
                        {
                            //set the status one step back, which is Reserved.
                            rstQuoteContract.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsRESERVED;
                            rstQuoteContract.Fields[modOpportunity.strfPIPELINE_STAGE].Value = modOpportunity.strsQUOTE;

                        }
                        else
                        {
                            if (strQuoteContractStatus == modOpportunity.strsRESERVED)
                            {
                                rstQuoteContract.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsCANCELLED;
                                rstQuoteContract.Fields[modOpportunity.strfPIPELINE_STAGE].Value = modOpportunity.strsCANCELLED;
                            }
                        }
                        objLib.PermissionIgnored = true;
                        objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstQuoteContract);

                        //UPDATE the Lot record with the Rollback details.
                        if (rstLot.RecordCount > 0)
                        {
                            rstLot.Fields[modOpportunity.strfTIC_ROLLBACK].Value = true;
                            rstLot.Fields[modOpportunity.strfTIC_ROLLBACK_DATE].Value = DateTime.Today;
                            //rstLot.Fields[modOpportunity.strfTIC_ORIGINAL_SALE_DATE].Value = rstQuoteContract.Fields[modOpportunity.strfQUOTE_CREATE_DATE].Value;
                            rstLot.Fields[modOpportunity.strfTIC_ORIGINAL_SALE_DATE].Value = rstQuoteContract.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value;
                            rstLot.Fields[modOpportunity.strfTIC_ORIGINAL_RESERVATION_DATE].Value = rstQuoteContract.Fields[modOpportunity.strfRESERVATION_DATE].Value;

                            //Rollback from a Sale to a Reservation
                            if (strQuoteContractStatus == modOpportunity.strsIN_PROGRESS)
                            {
                                //set the Lot status one step back, from Sold to Reserved.
                                rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsRESERVED;
                                rstLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsINVENTORY;
                                //AM2010.10.13 - Set the Sales_Date to DBNull for a lot that has been rolleded back to 
                                //a reservation
                                rstLot.Fields[modOpportunity.strfSALES_DATE].Value = DBNull.Value;

                            }
                            else
                            {
                                if (strQuoteContractStatus == modOpportunity.strsRESERVED)
                                {
                                    //set the Lot status one step back, from Reserved to Available.
                                    rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsAVAILABLE;
                                    rstLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsINVENTORY;
                                    //rstLot.Fields[modOpportunity.strfRESERVATION_CONTRACT_ID].Value =DBNull.Value;
                                    //AM2010.10.13 - Clear out the sales and reservation dates on the lot
                                    //being rolled back to available
                                    rstLot.Fields[modOpportunity.strfSALES_DATE].Value = DBNull.Value;
                                    rstLot.Fields[modOpportunity.strfRESERVED_DATE].Value = DBNull.Value;
                                }
                            }

                            objLib.PermissionIgnored = true;
                            objLib.SaveRecordset(modOpportunity.strtPRODUCT, rstLot);
                            rstLot.Close();
                        }
                    }
                    else
                    {
                        //TRANSFER  -----------------------------------------------------------------------------------------
                        if (strcase == modOpportunity.strsTRANSFER)
                        {
                            //GET THE TRANSFER TO lOT ID
                            object vntTransferToLotId = rstQuoteContract.Fields[modOpportunity.strfTIC_TRANSFER_TO_LOT_ID].Value;

                            if (vntTransferToLotId != null)
                            {
                            
                                //UPDATE the Opportunity record with the Transfer details.
                                rstQuoteContract.Fields[modOpportunity.strfTIC_TRANSFER].Value = true;
                                rstQuoteContract.Fields[modOpportunity.strfTIC_TRANSFER_DATE].Value = DateTime.Today;
                                //KA 10/14/10
                                //rstQuoteContract.Fields[modOpportunity.strfTIC_ORIGINAL_SALE_DATE].Value = rstQuoteContract.Fields[modOpportunity.strfQUOTE_CREATE_DATE].Value;
                                rstQuoteContract.Fields[modOpportunity.strfTIC_ORIGINAL_SALE_DATE].Value = rstQuoteContract.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value;
                                rstQuoteContract.Fields[modOpportunity.strfTIC_ORIGINAL_RESERVATION_DATE].Value = rstQuoteContract.Fields[modOpportunity.strfRESERVATION_DATE].Value;
                                rstQuoteContract.Fields[modOpportunity.strfTIC_TRANSFER_FROM_LOT_ID].Value = vntLotId;
                                rstQuoteContract.Fields[modOpportunity.strfTIC_TRANSFER_TO_LOT_ID].Value = vntTransferToLotId;
                            }

                            //UPDATE the TRANSFER FROM Lot record with the Transfer details.
                            if (rstLot.RecordCount > 0)
                            {
                                rstLot.Fields[modOpportunity.strfTIC_TRANSFER].Value = true;
                                rstLot.Fields[modOpportunity.strfTIC_TRANSFER_DATE].Value = DateTime.Today;
                                //KA 10/14/10
                                //rstLot.Fields[modOpportunity.strfTIC_ORIGINAL_SALE_DATE].Value = rstQuoteContract.Fields[modOpportunity.strfQUOTE_CREATE_DATE].Value;
                                rstLot.Fields[modOpportunity.strfTIC_ORIGINAL_SALE_DATE].Value = rstQuoteContract.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value;
                                rstLot.Fields[modOpportunity.strfTIC_ORIGINAL_RESERVATION_DATE].Value = rstQuoteContract.Fields[modOpportunity.strfRESERVATION_DATE].Value;
                                rstLot.Fields[modOpportunity.strfTIC_TRANSFER_TO_LOT_ID].Value = vntTransferToLotId;
                                rstLot.Fields[modOpportunity.strfRESERVATION_CONTRACT_ID].Value = DBNull.Value;
                                rstLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsINVENTORY;
                                rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsAVAILABLE;
                                //AM2010.10.13 - Clear out the sales and reservation dates on the lot
                                //being rolled back to available
                                rstLot.Fields[modOpportunity.strfSALES_DATE].Value = DBNull.Value;
                                rstLot.Fields[modOpportunity.strfRESERVED_DATE].Value = DBNull.Value;

                                objLib.PermissionIgnored = true;
                                objLib.SaveRecordset(modOpportunity.strtPRODUCT, rstLot);
                                rstLot.Close();
                            }

                            //GET the TRANSFER TO LOT Recordset
                            Recordset rstTransferToLot = objLib.GetRecordset(vntTransferToLotId, modOpportunity.strtPRODUCT, modOpportunity.strfTIC_ROLLBACK, modOpportunity.strfTIC_ROLLBACK_DATE, modOpportunity.strfTIC_TRANSFER,
                                                        modOpportunity.strfTIC_TRANSFER_DATE, modOpportunity.strfTIC_ORIGINAL_RESERVATION_DATE, modOpportunity.strfTIC_ORIGINAL_SALE_DATE,
                                                        modOpportunity.strfLOT_STATUS, modOpportunity.strfTYPE, modOpportunity.strfTIC_TRANSFER_FROM_LOT_ID, modOpportunity.strfTIC_TRANSFER_TO_LOT_ID);

                            //UPDATE the TRANSFER T0 Lot record with the Transfer details.
                            if (rstTransferToLot.RecordCount > 0)
                            {
                                rstTransferToLot.Fields[modOpportunity.strfTIC_TRANSFER].Value = true;
                                rstTransferToLot.Fields[modOpportunity.strfTIC_TRANSFER_DATE].Value = DateTime.Today;
                                rstTransferToLot.Fields[modOpportunity.strfTIC_TRANSFER_FROM_LOT_ID].Value = vntLotId;
                                rstTransferToLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsINVENTORY;
                                rstTransferToLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsRESERVED;
                                                              


                                //CMigles - transfers should always go to Reserved as per Bruce's request.
                                //if (strQuoteContractStatus == modOpportunity.strsIN_PROGRESS)
                                //{
                                //    rstTransferToLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsHOMESITE;
                                //}
                                //else
                                //    if (strQuoteContractStatus == modOpportunity.strsRESERVED)
                                //    {
                                //        rstTransferToLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsINVENTORY;
                                //    }


                                //SAVE
                                objLib.PermissionIgnored = true;
                                objLib.SaveRecordset(modOpportunity.strtPRODUCT, rstTransferToLot);
                                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstQuoteContract);
                                rstTransferToLot.Close();
                            } 
                        }
                    }
               }
                return;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }


        /// <summary>
        /// This function will perform the Transfer process for a sale or a reservation
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 1.0          08/19/2010   CMigles  Initial Version
        /// 1.1          10/7/2010    KA       added actual deicision date field and update the transferReserve call to include cancelled quote
        /// 1.2          10/14/2010   KA       changed dtsaledate from quote create date to actual decision date
        /// </history>
        public virtual bool Transfers(object vntOppId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();

                //QUOTE CONTRACT TO CANCELL
                Recordset rstQuoteContract = objLib.GetRecordset(vntOppId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfOPPORTUNITY_ID,
                                            modOpportunity.strfTIC_ROLLBACK, modOpportunity.strfTIC_ROLLBACK_DATE, modOpportunity.strfTIC_TRANSFER,
                                            modOpportunity.strfTIC_TRANSFER_DATE, modOpportunity.strfTIC_ORIGINAL_RESERVATION_DATE, modOpportunity.strfTIC_ORIGINAL_SALE_DATE,
                                            modOpportunity.strfPIPELINE_STAGE, modOpportunity.strfSTATUS, modOpportunity.strfLOT_ID, modOpportunity.strfQUOTE_CREATE_DATE,
                                            modOpportunity.strfRESERVATION_DATE, modOpportunity.strfTIC_TRANSFER_FROM_LOT_ID, modOpportunity.strfTIC_TRANSFER_TO_LOT_ID, 
                                            modOpportunity.strfNEIGHBORHOOD_ID, modOpportunity.strfCONTACT_ID, modOpportunity.strfTIC_CO_BUYER_ID, modOpportunity.strfQUOTE_TOTAL, 
                                            modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfELEVATION_ID,modOpportunity.strfECOE_DATE,
                                            modOpportunity.strfACTUAL_DECISION_DATE);

                if (rstQuoteContract.RecordCount > 0)
                {
                    string strQuoteContractStatus = TypeConvert.ToString(rstQuoteContract.Fields[modOpportunity.strfSTATUS].Value);
                    object vntLotId = rstQuoteContract.Fields[modOpportunity.strfLOT_ID].Value;      //TRANSFER FROM LOT ID
                    object vntNeighborhoodId = rstQuoteContract.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value; 
                    object vntContactId = rstQuoteContract.Fields[modOpportunity.strfCONTACT_ID].Value; 
                    object vntCoBuyerId = rstQuoteContract.Fields[modOpportunity.strfTIC_CO_BUYER_ID].Value;
                    //DateTime dtSaleDate = TypeConvert.ToDateTime(rstQuoteContract.Fields[modOpportunity.strfQUOTE_CREATE_DATE].Value);
                    DateTime dtSaleDate = TypeConvert.ToDateTime(rstQuoteContract.Fields[modOpportunity.strfACTUAL_DECISION_DATE].Value);
                    DateTime dtReservationDate = TypeConvert.ToDateTime(rstQuoteContract.Fields[modOpportunity.strfRESERVATION_DATE].Value);
                    object vntTransferToLotID = rstQuoteContract.Fields[modOpportunity.strfTIC_TRANSFER_TO_LOT_ID].Value;    //TRANSFER TO LOT ID
                    
                    if (strQuoteContractStatus == modOpportunity.strsRESERVED)
                    {
                        //UPDATE THE CANCELLED QUOTE CONTRACT WITH THE TRANSFER DETAILS
                        UpdateRollbackTransferInfo(rstQuoteContract, modOpportunity.strsTRANSFER, vntLotId, strQuoteContractStatus);
                        
                       
                        //PERFORM RESERVATION CANCELLATION FOR THE TRANSFER FROM LOT
                        //CancelRequestOrContract(vntOppId, true);
                        //AM2010.10.09 - If a cancel need to do a cancel reserve, this will force a Cancelled Reserve Lot Status History Record
                        CancelTransferRollbackReservation(vntOppId, true, "Transfer");
                       
                        
                        //WRITE History record on the old lot
                        //KA 10/14/10 changed bus tran date to today rather than reservation date
                        WriteContractHistoryRecords(vntLotId, vntOppId, modOpportunity.strsTRANSFER_RESERVE, DateTime.Today, false, vntTransferToLotID, false, false);
                        
                        //WRITE History records.Old Lot becomes Available
                        //KA 10/14/10 changed bus tran date to today rather than reservation date
                        WriteContractHistoryRecords(vntLotId, vntOppId, modOpportunity.strsAVAILABLE, DateTime.Today, false, null, false, false);

                        //CREATE RESERVATION FOR THE TRANSFER TO LOT
                        //KA 10/7/10 passing cancelled quote recordset
                        object vntNewOppId = TransferReserve(vntTransferToLotID, vntNeighborhoodId, vntContactId, vntCoBuyerId, vntLotId);

                        //WRITE History records on the new Lot
                        //KA 10/14/10 changed bus tran date to today rather than reservation date
                        WriteContractHistoryRecords(vntTransferToLotID, vntNewOppId, modOpportunity.strsTRANSFER_RESERVE, DateTime.Today, false, vntLotId, true, false);

                        //WRITE History records New Lot becomes Reserved.
                        //KA 10/14/10 changed bus tran date to today rather than reservation date
                        WriteContractHistoryRecords(vntTransferToLotID, vntNewOppId, modOpportunity.strsRESERVED, DateTime.Today, false, null, true, false);
                        
                        //SEND EMAIL NOTIFICATIONS
                        SendEmailNotifications("Transfer Reservation", vntNeighborhoodId, rstQuoteContract, vntContactId);
                    }
                    else
                    {
                        if (strQuoteContractStatus == modOpportunity.strsIN_PROGRESS)
                        {                            
                            //UPDATE Opportunity and lot with the Transfer details
                            UpdateRollbackTransferInfo(rstQuoteContract, modOpportunity.strsTRANSFER, vntLotId, strQuoteContractStatus);

                            //AM2010.10.09 - Because the UpdateRollbackTransferInfo call above doesn't create a SAM Constract record
                            //we need to create a Cancelled record for this lot/sale (This will ensure that a Cancelled status gets created
                            //SAM).
                            //WRITE CANCELLED HISTORY ON TRANSFER SALE
                            WriteContractHistoryRecords(vntLotId, vntOppId, modOpportunity.strsCANCELLED, DateTime.Today, false, null, false, false);
                                        

                            //PROCESS TRANSFER CONTRACT
                            //KA 10/7/10 passing cancelled quote recordset
                            object vntNewOppID = TransferContract(vntOppId, vntTransferToLotID, null, true, false);

                            //INACTIVATE THE INVENTORY QUOTE FOR THE TRANSFER_TO LOT.
                            Recordset rstInvQuote = objLib.GetRecordset(modOpportunity.strqHB_INVENTORY_QUOTE_FOR_INVENTORY_HOME, 1, vntTransferToLotID,
                                                          modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfINACTIVE);
                            if (rstInvQuote.RecordCount > 0)
                            {
                                rstInvQuote.MoveFirst();
                                rstInvQuote.Fields[modOpportunity.strfINACTIVE].Value = true;

                                objLib.PermissionIgnored = true;
                                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstInvQuote);
                                rstInvQuote.Close();
                            }

                            //WRITE HISTORY RECORDS FOR THE TRANSFERED FROM SALE - Transfer sale
                            //KA 10/14/10 changed bus tran date to today rather than og sale date
                            WriteContractHistoryRecords(vntLotId, vntOppId, modOpportunity.strsTRANSFER_SALE, DateTime.Today, false, vntTransferToLotID, false, false);
                            
                            //WRITE HISTORY RECORDS FOR THE TRANSFERED FROM SALE - Lot Status becomes Available
                            //KA 10/14/10 changed bus tran date to today rather than og sale date
                            WriteContractHistoryRecords(vntLotId, vntOppId, modOpportunity.strsAVAILABLE, DateTime.Today, false, null, false, false);
                            
                            //WRITE HISTORY RECORDS FOR THE TRANSFER TO LOT  - TRANSFER SALE
                            //KA 10/14/10 changed bus tran date to today rather than og sale date
                            WriteContractHistoryRecords(vntTransferToLotID, vntNewOppID, modOpportunity.strsTRANSFER_SALE, DateTime.Today, false, vntLotId, true, false);
                            
                            //WRITE HISTORY RECORDS FOR THE TRANSFER TO LOT  - SALE
                            WriteContractHistoryRecords(vntTransferToLotID, vntNewOppID, modOpportunity.strsRESERVED, DateTime.Today, false, null, true, false);

                            //SEND EMAIL NOTIFICATIONS
                            SendEmailNotifications("Transfer Contract", vntNeighborhoodId, rstQuoteContract, vntContactId);
                        }
                    }
                }
                return true;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will perform the Transfer process for a reservation
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 1.0          09/03/2010   CMigles  Initial Version
        /// 1.1          10/7/10      KA       commented out ecoe date
        /// 1.2          11/11/10     AM       Added transferred from lot Id to signature
        /// </history>
        public virtual object TransferReserve(object vntLotId, object vntNeighborhoodId, object vntContactId, object vntCoBuyerId,
            object vntTransferredFromLotId)
        {
            try
            {
                object vntNewQuoteContractId = DBNull.Value;
                object vntInventoryQuoteId = DBNull.Value;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                if (vntLotId != null)
                {
                    //Get the Inventory quote for the Lot, make a copy and set the Quote Contract record for the Lot.
                    Recordset rstInvQuote = objLib.GetRecordset(modOpportunity.strqHB_INVENTORY_QUOTE_FOR_INVENTORY_HOME, 1, vntLotId,
                                                    modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfINACTIVE, modOpportunity.strfPLAN_NAME_ID);
                    
                    if (rstInvQuote.RecordCount > 0)
                    {
                        vntInventoryQuoteId = rstInvQuote.Fields[modOpportunity.strfOPPORTUNITY_ID].Value;
                        
                        
                        if (vntInventoryQuoteId != null)
                        {
                            //Call the CopyQuote function from Opportunity assembly. 
                            //AM2010.11.02 - set copyPlan to false so that options are not copied to the new
                            //transferred contract
                            vntNewQuoteContractId = CopyQuote(vntInventoryQuoteId, false, false, false);

                            //Get the new quote contract and update it with buyer's and reservation information.
                            if (vntNewQuoteContractId != null)
                            {
                                Recordset rstNewQuoteContract = objLib.GetRecordset(vntNewQuoteContractId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfOPPORTUNITY_ID,
                                                   modOpportunity.strfCONTACT_ID, modOpportunity.strfSTATUS, modOpportunity.strfRESERVATION_DATE, modOpportunity.strfPIPELINE_STAGE,
                                                   modOpportunity.strfTIC_CO_BUYER_ID, modOpportunity.strfECOE_DATE, modOpportunity.strfTIC_TRANSFER_TO_LOT_ID,
                                                   modOpportunity.strfACTUAL_DECISION_DATE, modOpportunity.strfPLAN_NAME_ID,
                                                   modOpportunity.strfTIC_TRANSFER, modOpportunity.strfTIC_TRANSFER_FROM_LOT_ID);


                                if (rstNewQuoteContract.RecordCount > 0)
                                {
                                    rstNewQuoteContract.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsRESERVED;
                                    rstNewQuoteContract.Fields[modOpportunity.strfRESERVATION_DATE].Value = DateTime.Today;
                                    rstNewQuoteContract.Fields[modOpportunity.strfCONTACT_ID].Value = vntContactId;
                                    rstNewQuoteContract.Fields[modOpportunity.strfTIC_CO_BUYER_ID].Value = vntCoBuyerId;
                                    //ka 10-08-10 commented out ecoe 
                                    //rstNewQuoteContract.Fields[modOpportunity.strfECOE_DATE].Value = DateTime.Today;

                                    //AM2010.11.11 - settign transferred from and transfer flag
                                    rstNewQuoteContract.Fields[modOpportunity.strfTIC_TRANSFER_TO_LOT_ID].Value = DBNull.Value;
                                    rstNewQuoteContract.Fields[modOpportunity.strfTIC_TRANSFER_FROM_LOT_ID].Value = vntTransferredFromLotId;
                                    rstNewQuoteContract.Fields[modOpportunity.strfTIC_TRANSFER].Value = true;
                                    //AM2010.11.02 - Set the plan on the new quote
                                    rstNewQuoteContract.Fields[modOpportunity.strfPLAN_NAME_ID].Value
                                        = rstInvQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value;

                                    //Save recordset
                                    objLib.PermissionIgnored = true;
                                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstNewQuoteContract);

                                    //Close all Recordsets 
                                    rstNewQuoteContract.Close();


                                    //Set Co-buyers from AP to Lot and Quote Contract
                                    //SetLotQuoteContractCobuyers(vntLotId, vntNewQuoteContractId, vntAdvantageProgramId);
                                    
                                    //UPDATE CONTACT PROFILE NEIGHBORHOOD
                                    UpdateContactProfileNeighborhood(vntContactId, vntNeighborhoodId, DateTime.Today, null, null, null,
                                                            null, null, null, null, null, null, null);
                                    //UPDATE LOT STATUS
                                    UpdateLotStatus(vntLotId, vntNewQuoteContractId);

                                    //Inactivate the original Inventory Quote
                                    rstInvQuote.Fields[modOpportunity.strfINACTIVE].Value = true;
                                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstInvQuote);
                                    rstInvQuote.Close();

                                }
                            }
                        }
                    }
                }

                return vntNewQuoteContractId;
            }

            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }



        /// <summary>
        /// This function will set the Opportunity status to Closed as well as set the OOTB Actual Revenue Date.
        /// This will be needed just to make sure none of the OOTB code will break.
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 1.0          09/13/2010   CMigles  Initial Version
        /// </history>
        public virtual void EscrowCloseContract(object vntContractId, object dtEscrowCloseDate)
        {
            try
            {

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                OpportunityPostSaleQuote objOpportunityPostSaleQuote = (OpportunityPostSaleQuote)RSysSystem.ServerScripts[modOpportunity.strsTIC_OPPORTUNITY_POST_SALE_QUOTE].CreateInstance();

                if (vntContractId != null)
                {

                    Recordset rstContract = objLib.GetRecordset(vntContractId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfOPPORTUNITY_ID,
                                                   modOpportunity.strfSTATUS, modOpportunity.strfACTUAL_REVENUE_DATE, modOpportunity.strfLOT_ID,
                                                   modOpportunity.strfEXTERNAL_SOURCE_SYNC_STATUS);
                    if (rstContract.RecordCount > 0)
                    {
                        object vntLotId = rstContract.Fields[modOpportunity.strfLOT_ID].Value;
                        //CLOSE THE CONTRACT
                        rstContract.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsCLOSED;
                        rstContract.Fields[modOpportunity.strfACTUAL_REVENUE_DATE].Value = dtEscrowCloseDate;
                        //AM2010.10.20 - Added for SAP Integration when contract closes
                        rstContract.Fields[modOpportunity.strfEXTERNAL_SOURCE_SYNC_STATUS].Value = "Pending Send";
                        objLib.PermissionIgnored = true;
                        objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstContract);
                        rstContract.Close();

                        if (vntLotId != null)
                        {
                            //CLOSE THE LOT
                            Recordset rstlot = objLib.GetRecordset(vntLotId, modOpportunity.strtPRODUCT, modOpportunity.strfPRODUCT_ID,
                                       modOpportunity.strfLOT_STATUS, modOpportunity.strfCONTRACT_CLOSE_DATE);
                            if (rstlot.RecordCount > 0)
                            {
                                //CLOSE THE CONTRACT
                                rstlot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsCLOSED;
                                rstlot.Fields[modOpportunity.strfCONTRACT_CLOSE_DATE].Value = dtEscrowCloseDate;

                                objLib.PermissionIgnored = true;
                                objLib.SaveRecordset(modOpportunity.strtPRODUCT, rstlot);
                                rstlot.Close();
                            }
                        }

                        
                    //WRITE HISTORY RECORD  
                        WriteContractHistoryRecords(vntLotId, vntContractId, modOpportunity.strsCLOSED, TypeConvert.ToDateTime(dtEscrowCloseDate), false, null, false, false);
                    }

                    //AM2010.11.11 - Set built option flag for all options on contract at close of escrow
                    UpdateOptionsToBuiltForClosedContract(vntContractId, true);


                    //AM2010.11.11 - Create Lot Configurations for all Built Options on the close of escrow
                    // Add homesite configuration
                    object parameterList = DBNull.Value;
                    TransitionPointParameter transitionPointParameter = (TransitionPointParameter)RSysSystem.ServerScripts
                        [AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                    transitionPointParameter.SetUserDefinedParameter(1, vntContractId);
                    parameterList = transitionPointParameter.ParameterList;
                    RSysSystem.Forms[modOpportunity.strrLOT_CONFIGURATION].Execute(modOpportunity.strmCREATE_HOMESITE_CONFIGURATION,
                        ref parameterList);


                    //INACTIVATE ANY POST SALES QUOTES LINKED TO THE CONTRACT
                    objOpportunityPostSaleQuote.InactivateOtherPostSaleQuotes(vntContractId, false);

                }

                return;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method will be used to calculate the Option Deposit Breakouts
        /// </summary>
        /// <param name="vntOppId"></param>
        /// <param name="structOptionDeposit"></param>
        /// <param name="dgOptionDeposit"></param>
        /// <param name="initOptionDeposit"></param>
        /// <param name="otherOptionDeposit"></param>
        public void CalculateOptionDepositBuckets(object vntOppId, out decimal structOptionDeposit, out decimal dgOptionDeposit,
            out decimal initOptionDeposit, out decimal otherOptionDeposit)
        {

            DataAccess objLib = (DataAccess)
                RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                    .CreateInstance();

            StringBuilder sb = new StringBuilder();

            structOptionDeposit = 0;
            dgOptionDeposit = 0;
            initOptionDeposit = 0;
            otherOptionDeposit = 0;

            sb.Append("SELECT COALESCE(SUM(AMOUNT), 0) AS OPT_DEPOSIT_TOTAL, TYPE ");
            sb.Append("FROM DEPOSIT ");
            sb.Append("WHERE OPPORTUNITY_ID = " + RSysSystem.IdToString(vntOppId));
            sb.Append(" GROUP BY TYPE");

            Recordset rstOppProd = objLib.GetRecordset(sb.ToString());

            if (rstOppProd.RecordCount > 0)
            {
                rstOppProd.MoveFirst();

                while (!rstOppProd.EOF)
                {
                    if (TypeConvert.ToString(rstOppProd.Fields[1].Value) == "Structural Option")
                    {
                        structOptionDeposit = TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                    }
                    else if (TypeConvert.ToString(rstOppProd.Fields[1].Value) == "Decorator Option")
                    {
                        dgOptionDeposit = TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                    }
                    else if (TypeConvert.ToString(rstOppProd.Fields[1].Value) == "Initial Deposit")
                    {
                        initOptionDeposit = TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                    }
                    else if (TypeConvert.ToString(rstOppProd.Fields[1].Value) == "Other")
                    {
                        otherOptionDeposit = TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                    }



                    rstOppProd.MoveNext();
                }



            }
            else
            {
                structOptionDeposit = 0;
                dgOptionDeposit = 0;
                initOptionDeposit = 0;
                otherOptionDeposit = 0;
            }
            rstOppProd.Close();

        }


        /// <summary>
        /// This method will calculate totals for Adjustments and break them out into the
        /// correct buckets
        /// </summary>
        /// <param name="vntOppId"></param>
        /// <param name="basePriceAdjTotal"></param>
        /// <param name="decoratorAdjTotal"></param>
        /// <param name="structAdjTotal"></param>
        /// <param name="closingCostsAdjTotal"></param>
        public void CalculateAdjustmentBuckets(object vntOppId, out decimal basePriceAdjTotal, out decimal decoratorAdjTotal,
            out decimal structAdjTotal, out decimal closingCostsAdjTotal, out decimal merchBondsAdjTotal)
        {
            DataAccess objLib = (DataAccess)
                RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                    .CreateInstance();

            StringBuilder sb = new StringBuilder();

            basePriceAdjTotal = 0;
            decoratorAdjTotal = 0;
            structAdjTotal = 0;
            closingCostsAdjTotal = 0;
            merchBondsAdjTotal = 0;

            sb.Append("SELECT COALESCE(SUM(OA.ADJUSTMENT_AMOUNT), 0) AS ADJUSTMENT_AMOUNT, DA.ADJUSTMENT_TYPE AS ADJUSTMENT_TYPE, ");
            sb.Append("OA.TIC_ADJUSTMENT_SUB_TYPE ");
            sb.Append("FROM OPPORTUNITY_ADJUSTMENT OA ");
            sb.Append("INNER JOIN RELEASE_ADJUSTMENT RA ON OA.RELEASE_ADJUSTMENT_ID = RA.RELEASE_ADJUSTMENT_ID ");
            sb.Append("INNER JOIN DIVISION_ADJUSTMENT DA ON RA.DIVISION_ADJUSTMENT_ID = DA.DIVISION_ADJUSTMENT_ID ");
            sb.Append("WHERE OA.OPPORTUNITY_ID = " + RSysSystem.IdToString(vntOppId));
            sb.Append(" AND OA.SELECTED = 1 ");
            sb.Append(" GROUP BY DA.ADJUSTMENT_TYPE, OA.TIC_ADJUSTMENT_SUB_TYPE");

            Recordset rstOppProd = objLib.GetRecordset(sb.ToString());

            if (rstOppProd.RecordCount > 0)
            {
                rstOppProd.MoveFirst();

                while (!rstOppProd.EOF)
                {
                    if (TypeConvert.ToString(rstOppProd.Fields[1].Value) == "Base House")
                    {
                        basePriceAdjTotal = basePriceAdjTotal + TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                    }
                    else if (TypeConvert.ToString(rstOppProd.Fields[1].Value) == "Decorator")
                    {
                        if (TypeConvert.ToString(rstOppProd.Fields[2].Value) == "Merchandise Bonds")
                        {
                            merchBondsAdjTotal = merchBondsAdjTotal + TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                        }
                        else
                        {
                            decoratorAdjTotal = decoratorAdjTotal + TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                        }
                    }
                    else if (TypeConvert.ToString(rstOppProd.Fields[1].Value) == "Structural")
                    {
                        structAdjTotal = structAdjTotal + TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                    }
                    else if (TypeConvert.ToString(rstOppProd.Fields[1].Value) == "Closing Costs")
                    {
                        closingCostsAdjTotal = closingCostsAdjTotal + TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                    }

                    rstOppProd.MoveNext();
                }



            }
            else
            {
                basePriceAdjTotal = 0;
                decoratorAdjTotal = 0;
                structAdjTotal = 0;
                closingCostsAdjTotal = 0;
            }


            rstOppProd.Close();


        }


        /// <summary>
        /// This method will calculate structural and decorator options
        /// </summary>
        /// <param name="vntOppId"></param>
        /// <param name="structOptionTotal"></param>
        /// <param name="dgOptionTotal"></param>
        public void CalculateOptionBuckets(object vntOppId, out decimal structOptionTotal, out decimal dgOptionTotal)
        {

            DataAccess objLib = (DataAccess)
                RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                    .CreateInstance();

            StringBuilder sb = new StringBuilder();

            structOptionTotal = 0;
            dgOptionTotal = 0;

            sb.Append("SELECT COALESCE(SUM(OP.EXTENDED_PRICE), 0) AS OPTION_TOTAL, OP.TYPE ");
            sb.Append("FROM OPPORTUNITY__PRODUCT OP ");
            sb.Append("WHERE OP.OPPORTUNITY_ID = " + RSysSystem.IdToString(vntOppId));
            sb.Append(" AND OP.SELECTED = 1");
            sb.Append(" GROUP BY OP.TYPE ");

            Recordset rstOppProd = objLib.GetRecordset(sb.ToString());

            if (rstOppProd.RecordCount > 0)
            {
                rstOppProd.MoveFirst();
                while (!rstOppProd.EOF)
                {
                    if (TypeConvert.ToString(rstOppProd.Fields[1].Value) == "Structural")
                    {
                        structOptionTotal = TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                    }
                    else //if (TypeConvert.ToString(rstOppProd.Fields[1].Value) == "Decorator") - AM2011.01.25 - include null types (Custom) for calculation in design center totals
                    {
                        dgOptionTotal = dgOptionTotal + TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                    }

                    rstOppProd.MoveNext();
                }
            }
            else
            {
                structOptionTotal = 0;
                dgOptionTotal = 0;

            }
            rstOppProd.Close();

        }
        /// <summary>
        /// This method will calculate the Pre-Plotted options for the Contract
        /// </summary>
        /// <param name="vntOppId"></param>
        /// <param name="prePlotOptionTotal"></param>
        public void CalculatePrePlotOptionBuckets(object vntOppId, out decimal prePlotOptionTotal, out decimal prePlotStructOptionTotal)
        {
            DataAccess objLib = (DataAccess)
               RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                   .CreateInstance();

            StringBuilder sb = new StringBuilder();

        	prePlotOptionTotal = 0;
            prePlotStructOptionTotal = 0;

            sb.Append("SELECT COALESCE(SUM(EXTENDED_PRICE), 0) AS PREPLOT_OPTION_TOTAL, OP.TYPE ");
            sb.Append("FROM OPPORTUNITY__PRODUCT OP ");
            sb.Append("WHERE OP.TIC_PREPLOT_OPTION = 1 ");
            sb.Append(" AND OP.SELECTED = 1");
            sb.Append(" AND OP.OPPORTUNITY_ID = " + RSysSystem.IdToString(vntOppId));
            sb.Append(" GROUP BY OP.TYPE ");

            Recordset rstOppProd = objLib.GetRecordset(sb.ToString());


            if (rstOppProd.RecordCount > 0)
            {
                rstOppProd.MoveFirst();
                while (!rstOppProd.EOF)
                {
                    if (TypeConvert.ToString(rstOppProd.Fields[1].Value) == "Structural")
                    {
                        prePlotStructOptionTotal = TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                    }
                    else
                    {
                        prePlotOptionTotal = prePlotOptionTotal + TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
                    }

                    rstOppProd.MoveNext();
                }
            }
            else
            {
                prePlotStructOptionTotal = 0;
                prePlotOptionTotal = 0;

            }
            rstOppProd.Close();

        }


        /// <summary>
        /// This query will return all options on the contract excluding the Pre-Plotted options.
        /// </summary>
        /// <param name="vntOppId"></param>
        /// <param name="optionTotalForContract"></param>
        public void CalculateOptionTotalsForContract(object vntOppId, out decimal optionTotalForContract)
        {

            DataAccess objLib = (DataAccess)
                  RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                      .CreateInstance();

            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT COALESCE(SUM(EXTENDED_PRICE), 0) ");
            sb.Append("FROM OPPORTUNITY__PRODUCT OP ");
            //sb.Append("WHERE (OP.TIC_PREPLOT_OPTION IS NULL OR OP.TIC_PREPLOT_OPTION = 0)");
            sb.Append(" WHERE OP.SELECTED = 1");
            sb.Append(" AND OP.OPPORTUNITY_ID = " + RSysSystem.IdToString(vntOppId));

            Recordset rstOppProd = objLib.GetRecordset(sb.ToString());

            if (rstOppProd.RecordCount == 1)
            {
                optionTotalForContract = TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
            }
            else
            {
                optionTotalForContract = 0;
            }
            rstOppProd.Close();

        }

        /// <summary>
        /// Calculate the selected adjustments
        /// </summary>
        /// <param name="vntOpportunityId"></param>
        /// <param name="dblAdjTotals"></param>
        /// <param name="vntCurrentAdjustmentId"></param>
        /// <param name="isPSQ"></param>
        public virtual void CalculateAdjustmentTotalsForLimitChecking(object vntOpportunityId, object vntCurrentAdjustmentId,
            out decimal dblAdjTotals, bool isPSQ)
        {
            DataAccess objLib = (DataAccess)
                  RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                      .CreateInstance();

            StringBuilder sb = new StringBuilder();

            dblAdjTotals = 0;

            if (isPSQ)
            {
                sb.Append("SELECT COALESCE(SUM(ADJUSTMENT_AMOUNT), 0) ");
                sb.Append(" FROM OPPORTUNITY_ADJUSTMENT");
                sb.Append(" WHERE OPPORTUNITY_ID = " + RSysSystem.IdToString(vntOpportunityId));
                sb.Append(" AND SELECTED = 1 ");
                sb.Append(" AND (ADJUSTMENT_TYPE = 'Structural' OR ADJUSTMENT_TYPE = 'Decorator' ");
                sb.Append(" OR ADJUSTMENT_TYPE = 'Base Price')");
                sb.Append("  AND (TIC_INT_External_Source_Id IS NULL) ");
            }
            else
            {

                sb.Append("SELECT COALESCE(SUM(ADJUSTMENT_AMOUNT), 0) ");
                sb.Append(" FROM OPPORTUNITY_ADJUSTMENT");
                sb.Append(" WHERE OPPORTUNITY_ID = " + RSysSystem.IdToString(vntOpportunityId));
                sb.Append(" AND OPPORTUNITY_ADJUSTMENT_ID <> " + RSysSystem.IdToString(vntCurrentAdjustmentId));
                sb.Append(" AND SELECTED = 1 ");
                sb.Append(" AND (ADJUSTMENT_TYPE = 'Structural' OR ADJUSTMENT_TYPE = 'Decorator' ");
                sb.Append(" OR ADJUSTMENT_TYPE = 'Base Price')");
                sb.Append("  AND (TIC_INT_External_Source_Id IS NULL) ");
            }
            Recordset rstOppProd = objLib.GetRecordset(sb.ToString());

            if (rstOppProd.RecordCount == 1)
            {
                dblAdjTotals = Math.Abs(TypeConvert.ToDecimal(rstOppProd.Fields[0].Value));
            }
            else
            {
                dblAdjTotals = 0;
            }
            rstOppProd.Close();

        }


        /// <summary>
        /// This method will perform a check to make sure adjustment limits have not yet been 
        /// exceeded.
        /// </summary>
        /// <param name="opportunityId"></param>
        /// <param name="adjustmentAmount"></param>
        /// <param name="limitExceeded"></param>
        /// <param name="lotIncentiveLimit"></param>
        /// <param name="currAdjustmentId"></param>
        /// <param name="isPSQ"></param>
        /// <param name="adjustTotal"></param>
        public virtual void CheckAdjustmentLimits(object opportunityId, object currAdjustmentId,
            decimal adjustmentAmount, out bool limitExceeded, out decimal lotIncentiveLimit, bool isPSQ, out decimal adjustTotal)
        {

            limitExceeded = false;
            lotIncentiveLimit = 0;
            adjustTotal = 0;

            decimal dblTotalAdjAmt = 0;

            DataAccess objLib
                = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

            //'Get Lot Incentive Limit
            Recordset rstQuote = objLib.GetRecordset(opportunityId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfLOT_ID,
                modOpportunity.strfPIPELINE_STAGE, modOpportunity.strfSTATUS);

            //For PSQ or PBQ Check Limits against adjustments on quote
            if (isPSQ)
            {
                object vntLotId = rstQuote.Fields[modOpportunity.strfLOT_ID].Value;
                lotIncentiveLimit = (TypeConvert.ToDecimal(RSysSystem.Tables[modOpportunity.strt_PRODUCT].Fields["TIC_Incentive_Limit"].Index(vntLotId)));

                //'Find all selected opportunity adjustments for this quote
                CalculateAdjustmentTotalsForLimitChecking(opportunityId, null, out dblTotalAdjAmt, true);

                adjustTotal = dblTotalAdjAmt;

                //'Add current adjustment to total
                if (lotIncentiveLimit < (dblTotalAdjAmt))
                {
                    limitExceeded = true;
                }
                else
                {
                    limitExceeded = false;
                }
            }
            else
            {


                //Only check limits for Reserved Quotes.  All other checks will happen at PSQ Accept Changes
                if (TypeConvert.ToString(rstQuote.Fields[modOpportunity.strfSTATUS].Value) == modOpportunity.strsRESERVED
                    && TypeConvert.ToString(rstQuote.Fields[modOpportunity.strfPIPELINE_STAGE].Value) == modOpportunity.strsQUOTE)
                {
                    object vntLotId = rstQuote.Fields[modOpportunity.strfLOT_ID].Value;
                    lotIncentiveLimit = (TypeConvert.ToDecimal(RSysSystem.Tables[modOpportunity.strt_PRODUCT].Fields["TIC_Incentive_Limit"].Index(vntLotId)));

                    //'Find all selected opportunity adjustments for this quote
                    CalculateAdjustmentTotalsForLimitChecking(opportunityId, currAdjustmentId, out dblTotalAdjAmt, false);


                    //'Add current adjustment to total
                    if (lotIncentiveLimit < (dblTotalAdjAmt + adjustmentAmount))
                    {
                        limitExceeded = true;
                    }
                    else
                    {
                        limitExceeded = false;
                    }
                }
                else
                {
                    limitExceeded = false;

                }

            }

        }

        /// <summary>
        /// Set all options preplot flag = true for a given opportunity
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#  Date        Author       Description
        /// 3.8.0.0    9/30/2010   CMigles      Initial Version
        /// </history>
        protected virtual void SetOptionsPreplotFlag(object OpportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                
                Recordset rstSelectedOptions = objLib.GetRecordset(modOpportunity.strqTIC_ALL_SELECTED_OPTIONS_FOR_OPP,1, OpportunityId, 
                                     modOpportunity.strfTIC_PREPLOT_OPTION );

                if (rstSelectedOptions.RecordCount > 0)
                {
                    rstSelectedOptions.MoveFirst();

                    while (!rstSelectedOptions.EOF)
                    {
                        rstSelectedOptions.Fields[modOpportunity.strfTIC_PREPLOT_OPTION].Value = true;

                        rstSelectedOptions.MoveNext();
                    }
                }

                //SAVE recordset.
                objLib.PermissionIgnored = true;
                objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstSelectedOptions);
                rstSelectedOptions.Close();
                   
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }


        /// <summary>
        /// This method will retreive all selected opportunity records that are dups and combine the quantities
        /// in order to only create 1 preplotted line item on the Inventory Quote.
        /// </summary>
        /// <param name="OpportunityId"></param>
        protected virtual void CombinePrePlotAndNonPrePlotsForSameOption(object OpportunityId)
        { 
            //Assumptions: prices will be the same for Pre-Plotted Option and Non-Preplotted Option
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                //With the selected recordset for this opportunity record, find any option code_ that is in the 
                //recordset twice (means it's a pre-plot and non-preplot option scenario).
                StringBuilder sb = new StringBuilder();
                sb.Append("select max(opportunity__product_id) as opp_prod_id, code_, sum(quantity) as quantity, ");
                sb.Append("max(price) as price from opportunity__product op (nolock) ");
                sb.Append("where opportunity_id = " + RSysSystem.IdToString(OpportunityId));
                sb.Append(" and selected = 1 ");
                sb.Append("group by code_ ");
                sb.Append("having count(*) > 1");
                Recordset rstDuplicates = objLib.GetRecordset(sb.ToString());

                //Check to see if any dups exist
                if (rstDuplicates.RecordCount > 0)
                {
                    rstDuplicates.MoveFirst();
                    while (!rstDuplicates.EOF)
                    {
                        //Set price and quantity variables
                        string strCode_ = TypeConvert.ToString(rstDuplicates.Fields["code_"].Value);
                        int intQuantity = TypeConvert.ToInt32(rstDuplicates.Fields["quantity"].Value);
                        decimal decPrice = TypeConvert.ToDecimal(rstDuplicates.Fields["price"].Value);
                        object vntOppProdId = rstDuplicates.Fields["opp_prod_id"].Value;

                        //Delete the opprod id prior to getting the recordset.
                        objLib.DeleteRecord(vntOppProdId, modOpportunity.strtOPPORTUNITY__PRODUCT);
                        

                        //Now get all selected options for Opportunity Id (excludes record just deleted).
                        Recordset rstSelectedOptions = objLib.GetRecordset(modOpportunity.TIC_OPTIONS_BY_CODE_AND_OPPORTUNITY, 2, OpportunityId,
                            strCode_, modOpportunity.strfCODE_, modOpportunity.strfPRICE, modOpportunity.strfQUANTITY);

                        if (rstSelectedOptions.RecordCount > 0)
                        {
                            rstSelectedOptions.MoveFirst();                    
                            rstSelectedOptions.Fields[modOpportunity.strfPRICE].Value = decPrice;
                            rstSelectedOptions.Fields[modOpportunity.strfQUANTITY].Value = intQuantity;
                            //SAVE recordset.
                            objLib.PermissionIgnored = true;
                            objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstSelectedOptions);
                       
                        }

                        rstDuplicates.MoveNext();
                        rstSelectedOptions.Close();
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        
        }

        /// <summary>
        /// This method will calculate the total costs for all Design Options
        /// </summary>
        /// <param name="opportunityId"></param>
        /// <param name="decoratorOptionCosts"></param>
        protected virtual void CalculateDesignOptionCosts(object opportunityId, out decimal decoratorOptionCosts)
        {
            DataAccess objLib = (DataAccess)
                 RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                     .CreateInstance();

            StringBuilder sb = new StringBuilder();

            decoratorOptionCosts = 0;
            
            sb.Append("SELECT COALESCE(SUM(OPT.TIC_COST), 0) ");
            sb.Append("FROM OPPORTUNITY__PRODUCT OP ");
            sb.Append("INNER JOIN NBHDP_PRODUCT OPT ON OP.NBHDP_PRODUCT_ID = OPT.NBHDP_PRODUCT_ID ");
            sb.Append("WHERE OP.SELECTED = 1 ");
            sb.Append("AND OPT.TYPE = 'Decorator' ");
            sb.Append("AND OP.OPPORTUNITY_ID = " + RSysSystem.IdToString(opportunityId));

            Recordset rstOppProd = objLib.GetRecordset(sb.ToString());

            if (rstOppProd.RecordCount == 1)
            {
                decoratorOptionCosts = TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
            }
            else
            {
                decoratorOptionCosts = 0;
            }
            rstOppProd.Close();
        
        }

        /// <summary>
        /// This method will be called from both the reservation and when a user accepts a post sale quote
        /// </summary>
        /// <param name="opportunityId"></param>
        /// <param name="currentAdjustmentId"></param>
        /// <param name="isPSQ"></param>
        /// <param name="adjustmentExists"></param>
        /// <param name="dupAdjustmentName"></param>
        public virtual void CheckForExistingAdjustment(object opportunityId, object currentAdjustmentId, bool isPSQ,
            out bool adjustmentExists, out string dupAdjustmentName)
        {

            dupAdjustmentName = string.Empty;

            DataAccess objLib
                = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

            //'Get Lot Incentive Limit
            Recordset rstQuote = objLib.GetRecordset(opportunityId, modOpportunity.strtOPPORTUNITY, modOpportunity.strfLOT_ID,
                modOpportunity.strfPIPELINE_STAGE, modOpportunity.strfSTATUS);

            //For PSQ or PBQ Check Limits against adjustments on quote
            if (isPSQ)
            {
               //Loop through all selected opportunity adjustments and see if there are duplicates
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT RA.ADJUSTMENT_TYPE, COUNT(RA.RELEASE_ADJUSTMENT_ID) AS ADJUSTMENT_COUNT FROM OPPORTUNITY_ADJUSTMENT OA ");
                sb.Append("INNER JOIN RELEASE_ADJUSTMENT RA ");
                sb.Append("ON OA.RELEASE_ADJUSTMENT_ID = RA.RELEASE_ADJUSTMENT_ID ");
                sb.Append("WHERE OA.SELECTED = 1 ");
                sb.Append("AND OA.OPPORTUNITY_ID = " + RSysSystem.IdToString(opportunityId));
                sb.Append("GROUP BY RA.ADJUSTMENT_TYPE, RA.RELEASE_ADJUSTMENT_ID");

                Recordset rstOppAdj = objLib.GetRecordset(sb.ToString());

                //No dup adjustments
                adjustmentExists = false;

                if (rstOppAdj.RecordCount > 0)
                {
                    rstOppAdj.MoveFirst();

                    while (!(rstOppAdj.EOF))
                    {
                        if (TypeConvert.ToInt32(rstOppAdj.Fields[1].Value) > 1)
                        {
                            dupAdjustmentName = TypeConvert.ToString(rstOppAdj.Fields[0].Value);
                            adjustmentExists = true;
                            break;
                        }
                        
                        rstOppAdj.MoveNext();
                    }
                                       
                }
                else
                {
                    adjustmentExists = false;
                }

                rstOppAdj.Close();
               
               
            }
            else
            {
                //Using the current Release adjustment Id check to see if an existing selected opportunity adjustment
                //already exists.  If so then don't allow
                //Only check adjustments for Reserved Quotes.  All other checks will happen at PSQ Accept Changes
                if (TypeConvert.ToString(rstQuote.Fields[modOpportunity.strfSTATUS].Value) == modOpportunity.strsRESERVED
                    && TypeConvert.ToString(rstQuote.Fields[modOpportunity.strfPIPELINE_STAGE].Value) == modOpportunity.strsQUOTE)
                {
                                     

                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT RA.ADJUSTMENT_TYPE, RA.RELEASE_ADJUSTMENT_ID, OA.OPPORTUNITY_ID, OA.SELECTED FROM OPPORTUNITY_ADJUSTMENT OA ");
                    sb.Append("INNER JOIN RELEASE_ADJUSTMENT RA ");
                    sb.Append("ON OA.RELEASE_ADJUSTMENT_ID = RA.RELEASE_ADJUSTMENT_ID ");
                    sb.Append("WHERE OA.SELECTED = 1 ");
                    sb.Append("AND OA.OPPORTUNITY_ID = " + RSysSystem.IdToString(opportunityId));
                    sb.Append(" AND RA.RELEASE_ADJUSTMENT_ID = " + RSysSystem.IdToString(currentAdjustmentId));
            
                    Recordset rstOppAdj = objLib.GetRecordset(sb.ToString());

                    if (rstOppAdj.RecordCount > 0)
                    {
                        dupAdjustmentName = TypeConvert.ToString(rstOppAdj.Fields[0].Value);
                        adjustmentExists = true;
                    }
                    else
                    {
                        adjustmentExists = false;
                    }

                    rstOppAdj.Close();


                }
                else
                {
                    adjustmentExists = false;

                }


            }
        
        }

        
        /// <summary>
        /// This query will return all options on the contract excluding the Pre-Plotted options.
        /// </summary>
        /// <param name="vntOppId"></param>
        /// <param name="optionTotalForContract"></param>
        public void CalculateOptionSquareFootageForContract(object vntOppId, out decimal optionSqFtForContract)
        {

            DataAccess objLib = (DataAccess)
                  RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                      .CreateInstance();

            StringBuilder sb = new StringBuilder();
                     

            sb.Append(" SELECT COALESCE(SUM(TIC_Option_Sq_Ft), 0) ");
            sb.Append("FROM OPPORTUNITY__PRODUCT OP ");
            //sb.Append("WHERE (OP.TIC_PREPLOT_OPTION IS NULL OR OP.TIC_PREPLOT_OPTION = 0)");
            sb.Append(" WHERE OP.SELECTED = 1 ");
            sb.Append("AND OP.TYPE = 'Structural' ");
            sb.Append(" AND OP.OPPORTUNITY_ID = " + RSysSystem.IdToString(vntOppId));

            Recordset rstOppProd = objLib.GetRecordset(sb.ToString());

            if (rstOppProd.RecordCount == 1)
            {
                optionSqFtForContract = TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
            }
            else
            {
                optionSqFtForContract = 0;
            }
            rstOppProd.Close();

        }


        /// <summary>
        /// Calculates the actual borker commissions for a give opportunity.  The table-level formula was not 
        /// working correctly, so decided to put in the code
        /// </summary>
        /// <param name="vntOppId"></param>
        /// <param name="actualBrokerCommissions"></param>
        public void CalculateActualBrokerCommissions(object vntOppId, out decimal actualBrokerCommissions)
        {
            DataAccess objLib = (DataAccess)
                      RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                          .CreateInstance();

            StringBuilder sb = new StringBuilder();

            actualBrokerCommissions = 0;
            
            sb.Append("select sum(tic_broker_commission) from company__opportunity ");
            sb.Append("where opportunity_id = " + RSysSystem.IdToString(vntOppId));
           
            Recordset rstOppProd = objLib.GetRecordset(sb.ToString());

            if (rstOppProd.RecordCount == 1)
            {
                actualBrokerCommissions = TypeConvert.ToDecimal(rstOppProd.Fields[0].Value);
            }
            else
            {
                actualBrokerCommissions = 0;
            }
            rstOppProd.Close();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lotId"></param>
        /// <param name="planBuilt"></param>
        protected virtual void UpdateOptionsToBuiltForClosedContract(object vntContractId, bool builtFlag)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOppProd = objLib.GetRecordset(modOpportunity.strqSELECTED_OPTIONS_FOR_QUOTE, 1, vntContractId, modOpportunity.strfBUILD_OPTION);

                if (rstOppProd.RecordCount > 0)
                {
                    rstOppProd.MoveFirst();
                    while (!(rstOppProd.EOF))
                    {
                        rstOppProd.Fields[modOpportunity.strfBUILD_OPTION].Value = builtFlag;
                        rstOppProd.MoveNext();
                    }
                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY__PRODUCT, rstOppProd);

                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        //Name: CancelTransferContract
        //By:   Kevin Auh
        //Date: 11/24/10
        //Desc: Same as Cancel Contract but will stamp a "transfer" note rather than "cancelled" 
        protected virtual bool CancelTransferContract(object opportunityId, bool sameLot)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // update the quote
                Recordset rstQuote = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strfCANCEL_DATE,
                    modOpportunity.strfLOT_ID, modOpportunity.strf_STATUS, modOpportunity.strf_PIPELINE_STAGE, modOpportunity.strfACTUAL_REVENUE_DATE,
                    modOpportunity.strfDELTA_CANCEL_DATE, modOpportunity.strfDELTA_ACT_REV_DATE, modOpportunity.strf_CONTACT_ID,
                    modOpportunity.strf_NBHD_PHASE_ID, modOpportunity.strfPLAN_BUILT, modOpportunity.strfWARRANTY_DATE,
                    modOpportunity.strfSERVICE_DATE, modOpportunity.strfCANCEL_NOTES, modOpportunity.strfCANCEL_REQUEST_DATE,
                    modOpportunity.strfNEIGHBORHOOD_ID, modOpportunity.strfCANCEL_APPROVED_BY, modOpportunity.strfCANCEL_DECLINED_DATE,
                    modOpportunity.strfCANCEL_DECLINED_By, modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfELEVATION_BUILT);
                object vntCurrentUserId = RSysSystem.CurrentUserId();

                bool blnPlanBuilt = false;
                bool blnElevationBuilt = false;
                object vntLotId = DBNull.Value;
                object vntNeighborhoodId = DBNull.Value;
                object vntContactId = DBNull.Value;
                if (rstQuote.RecordCount > 0)
                {
                    vntLotId = rstQuote.Fields[modOpportunity.strfLOT_ID].Value;
                    vntNeighborhoodId = rstQuote.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                    vntContactId = rstQuote.Fields[modOpportunity.strfCONTACT_ID].Value;

                    rstQuote.Fields[modOpportunity.strf_PIPELINE_STAGE].Value = modOpportunity.strsCANCELLED;
                    rstQuote.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsCANCELLED;
                    rstQuote.Fields[modOpportunity.strfCANCEL_NOTES].Value = rstQuote.Fields[modOpportunity.strfCANCEL_NOTES].Value + "\r\n" + "Transferred Sale by " + RSysSystem.CurrentUserName() + modOpportunity.strlON + DateTime.Today.ToShortDateString();


                    rstQuote.Fields[modOpportunity.strfDELTA_CANCEL_DATE].Value = DateTime.Today;
                    rstQuote.Fields[modOpportunity.strfCANCEL_DATE].Value = DateTime.Today;
                    rstQuote.Fields[modOpportunity.strfCANCEL_REQUEST_DATE].Value = DateTime.Today;
                    rstQuote.Fields[modOpportunity.strfCANCEL_APPROVED_BY].Value = vntCurrentUserId;
                    rstQuote.Fields[modOpportunity.strfCANCEL_DECLINED_DATE].Value = DBNull.Value;
                    rstQuote.Fields[modOpportunity.strfCANCEL_DECLINED_By].Value = DBNull.Value;

                    blnPlanBuilt = TypeConvert.ToBoolean(rstQuote.Fields[modOpportunity.strfPLAN_BUILT].Value);
                    blnElevationBuilt = TypeConvert.ToBoolean(rstQuote.Fields[modOpportunity.strfELEVATION_BUILT].Value);

                    // update NBHD Profile
                    UpdateContactProfileNeighborhood(vntContactId, vntNeighborhoodId, null, null, null, null, null, DateTime.Today,
                        DateTime.Today, null, null, null, null);
                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstQuote);
                }

                // RY 11/28/2005: Inactivate the Post Sales Quotes if the Contract is cancelled.
                Recordset rstPostSaleQuotes = objLib.GetRecordset(modOpportunity.strqACTIVE_POST_SALE_QUOTES_FOR_OPP, 2, opportunityId,
                    modOpportunity.strsPOST_SALE, modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfSTATUS, modOpportunity.strfINACTIVE);
                if (rstPostSaleQuotes.RecordCount > 0)
                {
                    rstPostSaleQuotes.MoveFirst();
                    while (!(rstPostSaleQuotes.EOF))
                    {
                        // inactivate the quote
                        rstPostSaleQuotes.Fields[modOpportunity.strfSTATUS].Value = modOpportunity.strsINACTIVE;
                        rstPostSaleQuotes.Fields[modOpportunity.strfINACTIVE].Value = true;
                        rstPostSaleQuotes.MoveNext();
                    }
                    objLib.SaveRecordset(modOpportunity.strtOPPORTUNITY, rstPostSaleQuotes);
                }

                // update the contact
                UpdateCoBuyerStatus(opportunityId, false, false);

                // get the lot recordset
                Recordset rstLot = objLib.GetRecordset(vntLotId, modOpportunity.strt_PRODUCT, modOpportunity.strfTYPE, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                    modOpportunity.strfLOT_STATUS, modOpportunity.strfOWNER_ID, modOpportunity.strfOWNER_NAME, modOpportunity.strfCONTRACT_CLOSE_DATE,
                    modOpportunity.strfPLAN_ID, modOpportunity.strfELEVATION_ID, modOpportunity.strfSALES_DATE, modOpportunity.strfRESERVATION_CONTRACT_ID,
                    modOpportunity.strfRESERVED_DATE);
                object vntPrevOwner = DBNull.Value;

                // if the lot is under construction, make sure the type is set to inventory and there is an
                // inventory quote created
                if (rstLot.RecordCount > 0)
                {
                    if (Convert.IsDBNull(rstLot.Fields[modOpportunity.strfTYPE].Value))
                    {
                        rstLot.Fields[modOpportunity.strfTYPE].Value = string.Empty;
                    }
                    rstLot.Fields[modOpportunity.strfCONTRACT_CLOSE_DATE].Value = DBNull.Value;
                    rstLot.Fields[modOpportunity.strfSALES_DATE].Value = DBNull.Value;
                    if (sameLot != true)
                    {
                        rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsAVAILABLE;
                        vntPrevOwner = rstLot.Fields[modOpportunity.strfOWNER_ID].Value;
                        rstLot.Fields[modOpportunity.strfOWNER_ID].Value = DBNull.Value;
                        rstLot.Fields[modOpportunity.strfOWNER_NAME].Value = DBNull.Value;
                        rstLot.Fields[modOpportunity.strfPLAN_ID].Value = DBNull.Value;

                        //Keep Elevation_Id as is
                        //rstLot.Fields[modOpportunity.strfELEVATION_ID].Value = DBNull.Value;

                        rstLot.Fields[modOpportunity.strfRESERVATION_CONTRACT_ID].Value = DBNull.Value;
                        rstLot.Fields[modOpportunity.strfRESERVED_DATE].Value = DBNull.Value;
                    }
                    // if the plan built flag is set then set it on this quote
                    if (blnPlanBuilt)
                    {
                        rstLot.Fields[modOpportunity.strfPLAN_ID].Value = rstQuote.Fields[modOpportunity.strfPLAN_NAME_ID].Value;
                    }

                    if ((!(Convert.IsDBNull(rstLot.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value)) || blnPlanBuilt)
                        && (TypeConvert.ToString(rstLot.Fields[modOpportunity.strfTYPE].Value)) != modOpportunity.strsINVENTORY)
                    {
                        // this lot is under contstruction
                        rstLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsINVENTORY;
                    }

                    // make sure an inventory quote doesn't already exist
                    Recordset rstInvQuote = objLib.GetRecordset(modOpportunity.strqINVENTORY_QUOTE_FOR_LOT, 1, vntLotId, modOpportunity.strfOPPORTUNITY_ID);

                    // based on the previous statements, the check for not null construction stage and type = Inventory,
                    // it's better to make the entire check in case something changes later.
                    if (rstInvQuote.RecordCount == 0 && (TypeConvert.ToString(rstLot.Fields[modOpportunity.strfTYPE].Value))
                        == modOpportunity.strsINVENTORY)
                    {
                        // create an inventory quote from the contract
                        object newOpportunityId = CreateInventoryQuoteFromContract(opportunityId);
                        
                        //CMigles - Set all options preplot = true
                        SetOptionsPreplotFlag(newOpportunityId);

                        //AM2011.03.03 - Combine Pre-Plots and Non-Preplots on transfers
                        CombinePrePlotAndNonPrePlotsForSameOption(newOpportunityId);
                        CalculateTotals(newOpportunityId, false);
                    }


                    objLib.PermissionIgnored = true;
                    objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);

                    if (!sameLot)
                    {
                        // add lot contact
                        Recordset rstNewLotContact = objLib.GetNewRecordset(modOpportunity.strtLOT__CONTACT, modOpportunity.strfPRODUCT_ID,
                            modOpportunity.strf_CONTACT_ID, modOpportunity.strfTYPE);
                        rstNewLotContact.AddNew(Type.Missing, Type.Missing);
                        rstNewLotContact.Fields[modOpportunity.strf_CONTACT_ID].Value = vntPrevOwner;
                        rstNewLotContact.Fields[modOpportunity.strfPRODUCT_ID].Value = vntLotId;
                        rstNewLotContact.Fields[modOpportunity.strfTYPE].Value = 0;

                        objLib.SaveRecordset(modOpportunity.strtLOT__CONTACT, rstNewLotContact);

                    }
                }

                // Inactive Unbuilt Lot Configurations
                TransitionPointParameter transitionPointParameter = (TransitionPointParameter)RSysSystem.ServerScripts
                    [AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                transitionPointParameter.SetUserDefinedParameter(1, vntLotId);
                object parameterList = transitionPointParameter.ParameterList;

                //AM2010.11.30 - Inactivate escrow records for cancelled/transferred contract
                InactivateCancelledEscrow(opportunityId);


                RSysSystem.Forms[modOpportunity.strrLOT_CONFIGURATION].Execute(modOpportunity.strmINACTIVATE_UNBUILT_LOT_CONFIGURATIONS,
                    ref parameterList);
                return true;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        //Name: CancelTransferRollbackReservation
        //By:   Kevin Auh
        //Date: 11/24/10
        //Desc: Same as Cancel Reservation but will stamp a "transfer" or "rollback" note rather than "cancelled" 
        public virtual void CancelTransferRollbackReservation(object opportunityId, bool cancelContractApproval, string strTransferOrRollback)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                ILangDict ldOpportunity = RSysSystem.GetLDGroup(modOpportunity.strgOPPORTUNITY);

                // update status
                Recordset rstOpportunity = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, modOpportunity.strf_STATUS,
                    modOpportunity.strfCANCEL_REQUEST_DATE, modOpportunity.strfQUOTE_TOTAL, modOpportunity.strfACTUAL_DECISION_DATE,
                    modOpportunity.strfCONTACT_ID, modOpportunity.strf_ACCOUNT_MANAGER_ID, modOpportunity.strfNEIGHBORHOOD_ID,
                    modOpportunity.strfPLAN_NAME_ID, modOpportunity.strfELEVATION_ID, modOpportunity.strfLOT_ID, modOpportunity.strfECOE_DATE,
                    modOpportunity.strf_RN_DESCRIPTOR, modOpportunity.strfPIPELINE_STAGE, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                    modOpportunity.strfCANCEL_DATE, modOpportunity.strfCANCEL_DECLINED_DATE, modOpportunity.strfCANCEL_DECLINED_By,
                    modOpportunity.strfCANCEL_APPROVED_BY, modOpportunity.strfCANCEL_NOTES, modOpportunity.strfPLAN_BUILT);

                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                object parameterList = objParam.Construct();
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                object vntLotId = DBNull.Value;
                object vntNeighborhoodId = DBNull.Value;
                object vntContactId = DBNull.Value;
                object vntCurrentEmployeeId = DBNull.Value;
                string vntCurrentEmployeeFirstName = string.Empty;
                string vntCurrentEmployeeLastName = string.Empty;
                string strCurrentEmployeeName = string.Empty;
                if (rstOpportunity.RecordCount > 0)
                {
                    rstOpportunity.MoveFirst();
                    vntLotId = rstOpportunity.Fields[modOpportunity.strfLOT_ID].Value;
                    vntNeighborhoodId = rstOpportunity.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                    vntContactId = rstOpportunity.Fields[modOpportunity.strfCONTACT_ID].Value;

                    // Get Current Employee full name
                    vntCurrentEmployeeId = administration.CurrentUserRecordId;
                    vntCurrentEmployeeFirstName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_FIRST_NAME,
                        vntCurrentEmployeeId));
                    vntCurrentEmployeeLastName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_LAST_NAME,
                        vntCurrentEmployeeId));
                    strCurrentEmployeeName = vntCurrentEmployeeFirstName + " " + vntCurrentEmployeeLastName;

                    if (cancelContractApproval)
                    {
                        // Cancel Approved
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_REQUEST_DATE].Value = DateTime.Today;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_DATE].Value = DateTime.Today;
                        rstOpportunity.Fields[modOpportunity.strfPIPELINE_STAGE].Value = modOpportunity.strsCANCELLED;
                        rstOpportunity.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsCANCELLED;

                        object vntCurrentUserId = RSysSystem.CurrentUserId();
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_APPROVED_BY].Value = administration.CurrentUserRecordId;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_DECLINED_DATE].Value = DBNull.Value;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_DECLINED_By].Value = DBNull.Value;
                        //KA 11/24/10 update so that "cancelled reservation shows up rather than just cancelled
                        //string str = TypeConvert.ToString(ldOpportunity.GetTextSub(modOpportunity.strlAPPROVED_CANCEL, new
                        //    object[] { DateTime.Today, strCurrentEmployeeName }));
                        string str = strTransferOrRollback + " Reservation Approved on " + DateTime.Today.ToShortDateString() + " by " + strCurrentEmployeeName;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value = TypeConvert.ToString(Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value) ? "" : TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value) + "\r\n") + str;

                        // update NBHD Profile
                        UpdateContactProfileNeighborhood(vntContactId, vntNeighborhoodId, null, null, null, null, null,
                            null, DateTime.Today, DateTime.Today, null, null, null);

                        //WRITE HISTORY RECORDS ON CANCELLATION OF RESERVE
                        WriteContractHistoryRecords(vntLotId, opportunityId, modOpportunity.strsCANCELLED_RESERVED, DateTime.Today, false, null, false, false);

                        // Jul 27, 2005. by JWang. If Cancel a contract, go inactivate all in progress Post Sale Quotes.
                        parameterList = objParam.SetUserDefinedParameter(1, opportunityId);
                        // RY: Modified method call to inactivate all PSQ instead of only in progress ones.
                        RSysSystem.Forms[modOpportunity.strrHB_POST_SALE_QUOTE].Execute(modOpportunity.strmINACTIVATE_ALL_PSQ,
                            ref parameterList);
                    }
                    else
                    {
                        // Cancel Request
                        rstOpportunity.Fields[modOpportunity.strf_STATUS].Value = modOpportunity.strsCANCEL_REQUEST;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_REQUEST_DATE].Value = DateTime.Today;
                        rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value = TypeConvert.ToString(Convert.IsDBNull(rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value) ? "" : TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfCANCEL_NOTES].Value) + "\r\n") + strTransferOrRollback + " Reservation Requested on " + DateTime.Today.ToShortDateString() + " by " + strCurrentEmployeeName;

                        // update NBHD Profile - Update the Cancel Request Date
                        UpdateContactProfileNeighborhood(vntContactId, vntNeighborhoodId, null, null, null, null, null,
                            DateTime.Today, null, null, null, null, null);

                    }

                    objLib.SaveRecordset(modOpportunity.strt_OPPORTUNITY, rstOpportunity);

                    UpdateCoBuyerStatus(opportunityId, false, false);
                    // AV Contact profile NBHD gets updated, if cancelling and there is a sales request or reserved
                    // quote then
                    // update the type to prospect
                    if (cancelContractApproval)
                    {
                        // UpdateContactType vntContactId, strsCANCELLED
                        Recordset rstCntNBHDProfile = objLib.GetRecordset(modOpportunity.strqCONTACT_PROFILE_NBHD_FOR_CONTACT,
                            2, vntContactId, vntNeighborhoodId, modOpportunity.strfCONTACT_PROFILE_NBHD_ID);
                        if (!(rstCntNBHDProfile.EOF))
                        {
                            // update contact profile neighborhood type
                            rstCntNBHDProfile.MoveFirst();
                            object vntCntNBHDProfileID = rstCntNBHDProfile.Fields[modOpportunity.strfCONTACT_PROFILE_NBHD_ID].Value;
                            ContactProfileNeighborhood objContactProfileNBHD = (ContactProfileNeighborhood)RSysSystem.ServerScripts[modOpportunity.strsCONTACT_PROFILE_NBHD].CreateInstance();

                            objContactProfileNBHD.UpdateNBHDPType(vntCntNBHDProfileID);

                        }
                    }

                }
                else
                {
                    // we haven't found oportunity, it must be some error
                    return;
                }

                // for cancel approvals or cancel contracts do the rest
                if (cancelContractApproval)
                {
                    UpdateCoBuyerStatus(opportunityId, false, true); // remove the co-buyers from the lot
                    // Update homesite status
                    if ((vntLotId != DBNull.Value))
                    {
                        // Set rstOpportunity = objLib.GetRecordset
                        Recordset rstLot = objLib.GetRecordset(vntLotId, modOpportunity.strt_PRODUCT, modOpportunity.strfPLAN_ID,
                            modOpportunity.strfELEVATION_ID, modOpportunity.strfTYPE, modOpportunity.strfLOT_STATUS,
                            modOpportunity.strfSALES_DATE, modOpportunity.strfOWNER_ID, modOpportunity.strfPRODUCT_ID,
                            modOpportunity.strfOWNER_NAME, modOpportunity.strfTYPE, modOpportunity.strfCONSTRUCTION_STAGE_ID,
                            modOpportunity.strfPLAN_ID, modOpportunity.strfELEVATION_ID, modOpportunity.strfRESERVATION_CONTRACT_ID,
                            modOpportunity.strfRESERVED_DATE, modOpportunity.strfTIC_CO_BUYER_ID, modOpportunity.strfNEIGHBORHOOD_ID);

                        object vntPrevOwner = DBNull.Value;
                        object vntProductID = DBNull.Value;
                        if (rstLot.RecordCount > 0)
                        {
                            rstLot.Fields[modOpportunity.strfLOT_STATUS].Value = modOpportunity.strsAVAILABLE;
                            rstLot.Fields[modOpportunity.strfSALES_DATE].Value = DBNull.Value;
                            rstLot.Fields[modOpportunity.strfRESERVED_DATE].Value = DBNull.Value;
                            vntProductID = rstLot.Fields[modOpportunity.strfPRODUCT_ID].Value;
                            vntPrevOwner = rstLot.Fields[modOpportunity.strfOWNER_ID].Value;

                            rstLot.Fields[modOpportunity.strfOWNER_ID].Value = DBNull.Value;
                            //Cmigles - Sept 24/2010 - Clear co-buyes on lot as well.
                            rstLot.Fields[modOpportunity.strfTIC_CO_BUYER_ID].Value = DBNull.Value;
                            rstLot.Fields[modOpportunity.strfOWNER_NAME].Value = DBNull.Value;
                            rstLot.Fields[modOpportunity.strfRESERVATION_CONTRACT_ID].Value = DBNull.Value;
                            if ((!(Convert.IsDBNull(rstLot.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value))
                                || (!(Convert.IsDBNull(rstLot.Fields[modOpportunity.strfPLAN_ID].Value))))
                                && (TypeConvert.ToString(rstLot.Fields[modOpportunity.strfTYPE].Value))
                                != modOpportunity.strsINVENTORY)
                            {
                                // this lot is under contstruction or the plan is build
                                rstLot.Fields[modOpportunity.strfTYPE].Value = modOpportunity.strsINVENTORY;
                            }
                            // if the plan built flag is set then set it on this quote
                            if (TypeConvert.ToBoolean(rstOpportunity.Fields[modOpportunity.strfPLAN_BUILT].Value))
                            {
                                rstLot.Fields[modOpportunity.strfPLAN_ID].Value = rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value;
                            }
                            // if the elevation is set then set it on the quote
                            // make sure an inventory quote doesn't already exist
                            Recordset rstInvQuote = objLib.GetRecordset(modOpportunity.strqINVENTORY_QUOTE_FOR_LOT, 1, vntLotId,
                                modOpportunity.strfOPPORTUNITY_ID);

                            // based on the previous statements, the check for not null construction stage and type
                            // = Inventory,
                            // it's better to make the entire check in case something changes later.
                            //if (rstInvQuote.RecordCount == 0 && (!(Convert.IsDBNull(rstLot.Fields[modOpportunity.strfCONSTRUCTION_STAGE_ID].Value))
                            //    || TypeConvert.ToBoolean(rstOpportunity.Fields[modOpportunity.strfPLAN_BUILT].Value)) &&
                            //    TypeConvert.ToString(rstLot.Fields[modOpportunity.strfTYPE].Value) == modOpportunity.strsINVENTORY)
                            if (rstInvQuote.RecordCount == 0)
                            {
                                // create an inventory quote from the contract
                                object newOpportunityId = CreateInventoryQuoteFromContract(opportunityId);
                                //CMigles - Set all options preplot = true
                                SetOptionsPreplotFlag(newOpportunityId);

                                //AM2011.03.03 - Combine Pre-Plots and Non-Preplots on transfers
                                CombinePrePlotAndNonPrePlotsForSameOption(newOpportunityId);
                                CalculateTotals(newOpportunityId, false);
                            }

                            objLib.PermissionIgnored = true;
                            objLib.SaveRecordset(modOpportunity.strt_PRODUCT, rstLot);
                        }

                        // add lot contact
                        Recordset rstNewLotContact = objLib.GetNewRecordset(modOpportunity.strtLOT__CONTACT, modOpportunity.strfPRODUCT_ID,
                            modOpportunity.strf_CONTACT_ID, modOpportunity.strfTYPE);
                        rstNewLotContact.AddNew(Type.Missing, Type.Missing);
                        rstNewLotContact.Fields[modOpportunity.strf_CONTACT_ID].Value = vntPrevOwner;
                        rstNewLotContact.Fields[modOpportunity.strfPRODUCT_ID].Value = vntProductID;
                        rstNewLotContact.Fields[modOpportunity.strfTYPE].Value = 0;

                        objLib.SaveRecordset(modOpportunity.strtLOT__CONTACT, rstNewLotContact);
                    }

                    // May 30, 2005. Added By JWang
                    // Inactive Unbuilt Lot Configurations
                    parameterList = objParam.SetUserDefinedParameter(1, vntLotId);
                    RSysSystem.Forms[modOpportunity.strrLOT_CONFIGURATION].Execute(modOpportunity.strmINACTIVATE_UNBUILT_LOT_CONFIGURATIONS,
                        ref parameterList);
                
                    //AM2010.11.30 - inactive escrow records for cancelled contract
                    InactivateCancelledEscrow(opportunityId);
                }
                //only send email from this if it's a rollback since transfer has the send email embedded in it's own function
                if (strTransferOrRollback == "Rollback")
                {
                    // send email
                    string strNotify = string.Empty;
                    string strSubject = string.Empty;
                    string strMsg1 = string.Empty;
                    string strMsg2 = string.Empty;
                    string strMsg3 = string.Empty;
                    if (cancelContractApproval)
                    {
                        strNotify = modOpportunity.strqNOTIFICATION_ON_CANCEL_APPROVAL;
                        //KA 11/24/10 redone so it doesn't use the ld string
                        //strSubject = modOpportunity.strdCANCEL_APPROVED_SUBJECT;
                        strSubject = strTransferOrRollback + " Reservation Approved ";
                        strMsg1 = modOpportunity.strdCANCEL_APPROVED_MESSAGE1;
                        strMsg2 = modOpportunity.strdCANCEL_APPROVED_MESSAGE2;
                        strMsg3 = modOpportunity.strdCANCEL_APPROVED_MESSAGE3;
                    }
                    else
                    {
                        strNotify = modOpportunity.strqNOTIFICATION_ON_CANCEL_REQUEST;
                        //KA 11/24/10 redone so it doesn't use the ld string
                        //strSubject = modOpportunity.strdCANCEL_REQUEST_SUBJECT;
                        strSubject = strTransferOrRollback + " Reservation Request ";
                        strMsg1 = modOpportunity.strdCANCEL_REQUEST_MESSAGE1;
                        strMsg2 = modOpportunity.strdCANCEL_REQUEST_MESSAGE2;
                        strMsg3 = modOpportunity.strdCANCEL_REQUEST_MESSAGE3;
                    }
                    // get recepient list from neighborhood notifcation team where notify on Sales Approved is true
                    Recordset rstEmailTo = objLib.GetRecordset(strNotify, 1, vntNeighborhoodId, modOpportunity.strf_EMPLOYEE_ID);
                    string strEmailTo = string.Empty;
                    if (rstEmailTo.RecordCount > 0)
                    {
                        rstEmailTo.MoveFirst();
                        StringBuilder emailToBuilder = new StringBuilder();
                        while (!(rstEmailTo.EOF))
                        {
                            string strWorkEmail = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtEMPLOYEE,
                                modOpportunity.strf_WORK_EMAIL, rstEmailTo.Fields[modOpportunity.strf_EMPLOYEE_ID].Value));
                            // add if not already there
                            if (!emailToBuilder.ToString().Contains(strWorkEmail))
                            {
                                emailToBuilder.Append(strWorkEmail + ";");
                            }
                            rstEmailTo.MoveNext();
                        }
                        // strip out last ;
                        strEmailTo = emailToBuilder.ToString();
                        strEmailTo = strEmailTo.Substring(0, strEmailTo.Length - 1);
                    }
                    rstEmailTo.Close();

                    if (strEmailTo.Trim().Length == 0)
                    {
                        return;
                    }
                    // all language strings are in nbhd_notification_team
                    ILangDict lngNBHD_Notification_Team = RSysSystem.GetLDGroup(modOpportunity.strgNBHD_NOTIFICATION_TEAM);
                    // set subject
                    vntCurrentEmployeeId = administration.CurrentUserRecordId;
                    vntCurrentEmployeeFirstName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_FIRST_NAME,
                        vntCurrentEmployeeId));
                    vntCurrentEmployeeLastName = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_LAST_NAME,
                        vntCurrentEmployeeId));
                    // TODO (DI Yin) strLotDescriptor is never assigned. Temporary code here 
                    string strLotDescriptor = string.Empty;

                    //AM2010.11.17 - Email notification changes for cancelled contract
                    // TODO (DI Yin) strLotDescriptor is never assigned. Temporary code here 

                    //AM2010.10.14 - Get Neighborhood, Division and Lot for the strLot Descriptor
                    string strNeighborhood = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfNAME, vntNeighborhoodId));
                    object vntDivisionId = objLib.SqlIndex(modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfDIVISION_ID, vntNeighborhoodId);
                    string strDivision = TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strtDIVISION, modOpportunity.strfNAME, vntDivisionId));
                    //object vntLotId = objLib.SqlIndex(modOpportunity.strtOPPORTUNITY, modOpportunity.strfLOT_ID, rstOpportunity.Fields[modOpportunity.strfOPPORTUNITY_ID].Value);
                    Recordset rstLotRef = objLib.GetRecordset(vntLotId, modOpportunity.strtPRODUCT, modOpportunity.strfUNIT, modOpportunity.strfTRACT, modOpportunity.strfLOT_NUMBER);
                    string strLot = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfLOT_NUMBER].Value);
                    string strUnit = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfUNIT].Value);
                    string strTract = TypeConvert.ToString(rstLotRef.Fields[modOpportunity.strfTRACT].Value);
                    strLotDescriptor = strDivision + ", " + strNeighborhood + ", T/" + strTract + " L/" + strLot + " U/" + strUnit;

                    //KA 11/24/10 redone so it doesn't use the ld string
                    //strSubject = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strSubject,
                    //    new object[] {vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, strLotDescriptor,
                    //    String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)) }));
                    strSubject = strSubject + vntCurrentEmployeeFirstName + " " + vntCurrentEmployeeLastName + ", " + strLotDescriptor
                        + " - " + String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value));

                    // set message
                    string strMessage = TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strMsg1,
                        new object[] {DateTime.Today, vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_FIRST_NAME, vntContactId)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strf_LAST_NAME, vntContactId)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_CONTACT, modOpportunity.strfHOME_PHONE, vntContactId)), 
                        String.Format("{0:C}", TypeConvert.ToDecimal(rstOpportunity.Fields[modOpportunity.strfQUOTE_TOTAL].Value)),
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                        rstOpportunity.Fields[modOpportunity.strfPLAN_NAME_ID].Value)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_NBHDP_PRODUCT, modOpportunity.strfPRODUCT_NAME, 
                        rstOpportunity.Fields[modOpportunity.strfELEVATION_ID].Value)) }));
                    // TODO (Di Yin) vntJob_Number is never assigned, temporary code here
                    int vntJob_Number = 0;
                    strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strMsg2,
                        new object[] { vntJob_Number, TypeConvert.ToString(rstOpportunity.Fields[modOpportunity.strfECOE_DATE].Value) }));
                    strMessage = strMessage + TypeConvert.ToString(lngNBHD_Notification_Team.GetTextSub(strMsg3,
                        new object[] {vntCurrentEmployeeFirstName, vntCurrentEmployeeLastName, 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strf_WORK_EMAIL, vntCurrentEmployeeId)), 
                        TypeConvert.ToString(objLib.SqlIndex(modOpportunity.strt_EMPLOYEE, modOpportunity.strfWORK_PHONE, vntCurrentEmployeeId))}));
                    //KA 11/24/10 replace LD String built message
                    strMessage = strMessage.Replace("Cancel", strTransferOrRollback);
                    strMessage = strMessage.Replace("cancel", strTransferOrRollback.ToLower());
                    //KA 11/24/10 adding village/nbdh/tract/lot/unit info infront of the message var
                    strMessage = strLotDescriptor + "\n\n" + strMessage;
                    SendSimpleMail(strEmailTo, strSubject, strMessage);
                }
                
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }


        /// <summary>
        /// This method will inactive all escrow records for a specific contract
        /// </summary>
        /// <param name="vntOpportunityId"></param>
        public virtual void InactivateCancelledEscrow(object vntOpportunityId)
        { 
            DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            objLib.PermissionIgnored = true;
            Recordset rstEscrowRecs = objLib.GetRecordset("TIC: Escrow for Opportunity Id?", 1, vntOpportunityId, "TIC_Inactive");

            if (rstEscrowRecs.RecordCount > 0)
            {
                rstEscrowRecs.MoveFirst();

                while (!rstEscrowRecs.EOF)
                { 
                    //Set inactive = true
                    rstEscrowRecs.Fields["TIC_Inactive"].Value = true;
                    rstEscrowRecs.MoveNext();
                }

                objLib.SaveRecordset(modOpportunity.strtTIC_ESCROW, rstEscrowRecs);
                
            }

            rstEscrowRecs.Close();



        }

        #endregion

    }

}
