using System;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;
using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server;
using System.Globalization;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// 
    /// This class represents the Homesite object used to record and maintain information about
    /// the homesites the user's company offers. This information is used throughout the system.
    ///
    /// </summary>
    /// <history>
    /// Revision#     Date        Author  Description
    /// 3.8.0.0       4/28/2006   JHui    Converted to .Net C# code.
    /// </history>
    public class Product : IRFormScript
    {
        private IRSystem7 mrsysSystem = null;

        protected IRSystem7 RSysSystem
        {
            get { return mrsysSystem; }
            set { mrsysSystem = value; }
        }

        private ILangDict grldtLangDict;

        protected ILangDict RldtLangDict
        {
            get { return grldtLangDict; }
            set { grldtLangDict = value; }
        }

        /// <summary>
        /// This function retrieves the first contruction stage for the division, for the neighborhood, for the release.
        /// </summary>
        /// 
        /// <param name="vntRelease_Id">Specified release record Id</param>
        /// <returns>The first construction stage</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       4/28/2006   JHui    Converted to .Net C# code.
        /// </history>
        protected virtual object[] GetConstructionStage(object vntRelease_Id)
        {
            try
            {
                object vntConstructionStage_Id = DBNull.Value;
                Recordset rstConstructionStage = null;
                string vntConstStageName = String.Empty;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                rstConstructionStage = objLib.GetRecordset(modProduct.strqCONSTRUCTION_STAGES_FOR_RELEASE_STAGE_NUMBER,
                    2, vntRelease_Id, 1, modProduct.strfCONSTRUCTION_STAGE_ID, modProduct.strfCONSTRUCTION_STAGE_NAME);

                if (!rstConstructionStage.EOF && !rstConstructionStage.BOF)
                {
                    rstConstructionStage.MoveFirst();
                    // get construction stage for the release
                    vntConstructionStage_Id = rstConstructionStage.Fields[modProduct.strfCONSTRUCTION_STAGE_ID].Value;
                    vntConstStageName = TypeConvert.ToString(rstConstructionStage.Fields[modProduct.strfCONSTRUCTION_STAGE_NAME].Value);
                }

                return new object[] { vntConstructionStage_Id, vntConstStageName };

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function retrieves the Lot Type/
        /// </summary>
        /// <param name="vntLotID">Specified Lot Id</param>
        /// <returns>Lot type (Inventory or Model) of specified lot ID</returns>
        /// Revision# Date       Author  Description
        /// 3.8.0.0   4/28/2006  JHui    Converted to .Net C# code.
        protected virtual string GetLotType(object vntLotID)
        {
            try
            {
                string strType;

                if (vntLotID is Array)
                {
                    strType = TypeConvert.ToString(RSysSystem.Tables[modProduct.strtPRODUCT].Fields[modProduct.strfTYPE].Index(vntLotID));
                }
                else
                {
                    strType = string.Empty;
                }

                return strType;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks to see if there is an inventory quote associated with a particular lot.
        /// </summary>
        /// <param name="vntLotID">Specified Lot Id</param>
        /// <returns>Array of the inventory quote values.  If there is no inventory quote, then all the values are null.</returns>
        /// <history>
        /// Revision# Date       Author  Description
        /// 3.8.0.0   4/28/2006  JHui    Converted to .Net C# code.
        /// </history>
        protected virtual object[] GetInventoryQuote(object vntLotID)
        {
            try
            {
                object vntInvQuote_Id = DBNull.Value;
                Recordset rstInventoryQuote = null;
                object vntPlan_Id = DBNull.Value;
                object vntElevation_Id = DBNull.Value;
                object vntPlanBuild = null;
                object vntBuiltOptions = null;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                rstInventoryQuote = objLib.GetRecordset(modProduct.strqINVENTORY_QUOTE_FOR_LOT, 1, vntLotID, modProduct.strfQUOTE_ID,
                    modProduct.strfPLAN_ID, modProduct.strfELEVATION_ID, modProduct.strfPLAN_BUILT, modProduct.strfBUILT_OPTIONS);

                if (!rstInventoryQuote.EOF && !rstInventoryQuote.BOF)
                {
                    rstInventoryQuote.MoveFirst();
                    vntInvQuote_Id = rstInventoryQuote.Fields[modProduct.strfQUOTE_ID].Value;
                    vntPlan_Id = rstInventoryQuote.Fields[modProduct.strfPLAN_ID].Value;
                    vntElevation_Id = rstInventoryQuote.Fields[modProduct.strfELEVATION_ID].Value;
                    vntPlanBuild = rstInventoryQuote.Fields[modProduct.strfPLAN_BUILT].Value;
                    vntBuiltOptions = rstInventoryQuote.Fields[modProduct.strfBUILT_OPTIONS].Value;
                }
                else
                {
                    vntPlan_Id = System.DBNull.Value;
                    vntElevation_Id = System.DBNull.Value;
                }

                return new object[] { vntPlan_Id, vntElevation_Id, vntInvQuote_Id, vntPlanBuild, vntBuiltOptions };
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }


        /// <summary>
        /// This function is called by the when a product is deleted. It deletes all of the associated product records and price book records.
        /// </summary>
        /// <param name="vntfProduct_Id">Product record Id to be deleted</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       4/28/2006   JHui    Converted to .Net C# code.
        /// </history>
        protected virtual void CascadeDelete(object vntfProduct_Id)
        {
            try
            {
                Recordset rstInspections = null;
                IRForm rfrmInspection = null;
                Recordset rstWorkOrders = null;
                IRForm rfrmWORK_ORDER = null;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // Delete Lot Plans
                objLib.DeleteRecordset(modProduct.strqLOT_PLANS_FOR_LOT, modProduct.strfLOT_PLAN_ID, vntfProduct_Id);

                // Delete Quotes
                objLib.DeleteRecordset(modProduct.strqQUOTE_FOR_LOT, modProduct.strfQUOTE_ID, vntfProduct_Id);

                // Delete Price Change Histories
                objLib.DeleteRecordset(modProduct.strqPRICE_CHNG_HISTORY_FOR_LOT, modProduct.strfPRICE_CHANGE_HISTORY_ID, vntfProduct_Id);

                // Delete Inspections
                rstInspections = objLib.GetRecordset(modProduct.strqINSPECTIONS_FOR_LOT, 1, vntfProduct_Id, modProduct.strfINSPECTION_ID);
                if (!rstInspections.EOF && !rstInspections.BOF)
                {
                    rstInspections.MoveFirst();
                    rfrmInspection = RSysSystem.Forms[modProduct.strrINSPECTION];
                    while (!rstInspections.EOF)
                    {
                        object parameterList = null;
                        rfrmInspection.DeleteFormData(rstInspections.Fields[modProduct.strfINSPECTION_ID].Value, ref parameterList);
                        rstInspections.MoveNext();
                    }
                }

                // Delete Service Requests
                objLib.DeleteRecordset(modProduct.strqSERVICE_REQUESTS_FOR_LOT, modProduct.strfSUPPORT_INCIDENT_ID, vntfProduct_Id);

                // Delete Work Orders
                rstWorkOrders = objLib.GetRecordset(modProduct.strqWORK_ORDER_FOR_LOT, 1, vntfProduct_Id, modProduct.strfWORK_ORDER_ID);
                if (!rstWorkOrders.EOF && !rstWorkOrders.BOF)
                {
                    rstWorkOrders.MoveFirst();
                    rfrmWORK_ORDER = RSysSystem.Forms[modProduct.strrWORK_ORDER];
                    while (!rstWorkOrders.EOF)
                    {
                        object parameterList = null;
                        rfrmWORK_ORDER.DeleteFormData(rstWorkOrders.Fields[modProduct.strfWORK_ORDER_ID].Value, ref parameterList);
                        rstWorkOrders.MoveNext();
                    }
                }

                // Delete Activities
                objLib.DeleteRecordset(modProduct.strqRN_APPTS_FOR_LOT, modProduct.strfRN_APPOINTMENTS_ID, vntfProduct_Id);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }


        /// <summary>
        /// Checks if there are quotes or orders attached to the specified product.
        /// </summary>
        /// <param name="vntfProduct_id">Product record Id</param>
        /// <returns>True if there are attached entities, False otherwise.</returns>
        /// <history>
        /// Revision# Date       Author  Description
        /// 3.8.0.0   4/28/2006  JHui    Converted to .Net C# code.
        /// </history>
        protected virtual bool HasAttacheds(object vntfProduct_id)
        {
            bool bHasAttacheds = false;
            try
            {

                Recordset rstRecordset = null;

                // Use function to check if there are quotes or orders attached to the product
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                rstRecordset = objLib.GetRecordset(modProduct.strqOPP_PRODUT_WITH_PRODUCT_ID, 1, vntfProduct_id, modProduct.strfOPPORTUNITY__PRODUCT_ID);
                if (rstRecordset.RecordCount > 0)
                {
                    bHasAttacheds = true;
                }
            }
            catch
            {
                //Ignore exception.
            }
            return bHasAttacheds;
        }


        /// <summary>
        /// This transit point method gets triggered by the AppServer before a new record
        /// is added. It provides a custom entry point to save new form data consisting of
        /// an array of primary and secondary recordsets to the database for the Product object.
        /// </summary>
        /// <param name="pForm">IRForm object reference to the client IRForm object where the AddFormData occurs.</param>
        /// <param name="Recordsets">Variant to hold an array including the primary recordset and its
        /// secondaries in the specified form for the Product object to be added to the database.</param>
        /// <param name="ParameterList">Transition point parameters passed from the client to AppServer for business rule processing.</param>
        /// <returns>New record Id for Product after the form data is added to the database successfully.</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       4/28/2006   JHui        Converted to .Net C# code.
        /// 5.9.0.0       2/08/2007   ML          changes for creating PriceChangeHistory
        /// </history>
        public virtual object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {

            try
            {
                object vntfProduct_id = DBNull.Value;
                Recordset rstRecordset = null;
                object vntParams = null;
                bool bTemplate = false;
                object vntInspectionTemplateId = DBNull.Value;
                object vntSoldDate = null;
                object vntReleasedDate = null;
                object vntClosedDate = null;
                object vntReservedDate = null;
                string strOldStatus = String.Empty;
                string strNewStatus = String.Empty;
                object vntInactive = null;

                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;
                object[] arrayRecs = (object[])Recordsets;
                rstRecordset = (Recordset)arrayRecs[0];

                // Apply the Status Rules based on several Dates
                vntSoldDate = rstRecordset.Fields[modProduct.strfSALES_DATE].Value;
                vntReleasedDate = rstRecordset.Fields[modProduct.strfRELEASE_DATE].Value;
                vntClosedDate = rstRecordset.Fields[modProduct.strfCONTRACT_CLOSE_DATE].Value;
                vntReservedDate = rstRecordset.Fields[modProduct.strfRESERVED_DATE].Value;
                vntInactive = rstRecordset.Fields[modProduct.strfINACTIVE].Value;
                strOldStatus = "" + rstRecordset.Fields[modProduct.strfLOT_STATUS].Value;
                strNewStatus = GetLotStatusByRules(vntSoldDate, vntReleasedDate, vntClosedDate, vntReservedDate, TypeConvert.ToBoolean(vntInactive));
                if (strOldStatus.ToUpper() != strNewStatus.ToUpper())
                {
                    rstRecordset.Fields[modProduct.strfLOT_STATUS].Value = strNewStatus;
                }

                // update delta fields
                UpdateDeltaFields(rstRecordset);

                // Add form data into database and get the product_Id
                vntfProduct_id = pForm.DoAddFormData(Recordsets, ref ParameterList);

                bTemplate = TypeConvert.ToBoolean(objParam.GetUserDefinedParameter(1));
                vntInspectionTemplateId = objParam.GetUserDefinedParameter(2);

                // create an inspection record for this lot
                if (bTemplate)
                {
                    Inspection objInspection = (Inspection)RSysSystem.ServerScripts[modProduct.strsINSPECTION].CreateInstance();
                    vntParams = objInspection.CreateNewInspection(vntfProduct_id, vntInspectionTemplateId, "");
                }
                // Issue #65536-13009 - HB r3.7
                // Removed this condition as the Price (secondary grid) doesn't implement any logic of not accepting
                // Values <= 0. Further, the Customer can get discounts in the form of -ve pricing, and it does not
                // harm the business flow.
                // Create the price change history record for the current price only if the price is > 0
                // if (TypeConvert.ToDouble(rstRecordset.Fields[modProduct.strfPRICE].Value) > 0.0)
                // {
                PriceChangeHistory objPCH = (PriceChangeHistory)RSysSystem.ServerScripts[modProduct.strsPRICE_CHANGE_HISTORY].CreateInstance();
                //ML - feb 2007 - as here Price = PriceChangeHistory.Cost_Price and hence Margin = 0    
                decimal dblMargin = 0;
                objPCH.CreatePriceChangeHistory(DateTime.Today, TypeConvert.ToDecimal(rstRecordset.Fields[modProduct.strfPRICE].Value), dblMargin,
                        PriceParentType.Lot, vntfProduct_id, true, false);
                // }

                return vntfProduct_id;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This transition point method gets triggered by the AppServer before a record is
        /// deleted. It provides a custom entry point to delete form data, consisting of an
        /// array of the primary and the secondary recordsets, from the database.
        /// This function calls the CascadeDelete to delete all of the associated product records and 
        /// price book records when the primary Product record is deleted.
        /// </summary>
        /// <param name="pForm">IRForm object reference to the client IRForm object where the DeleteFormData occurs</param>
        /// <param name="RecordId">Record Id for Product object</param>
        /// <param name="ParameterList">Transition point parameters passed from the client to AppServer for business rule processing</param>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       4/28/2006   JHui        Converted to .Net C# code.
        /// </history>
        public virtual void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {

            try
            {
                string strErrMsg = String.Empty;

                // If this product has quotes attached to it. Cancel the deletion
                if (HasAttacheds(RecordId) || !CanBeDeleted(pForm, RecordId, ref strErrMsg))
                {
                    if (strErrMsg.Length == 0)
                    {
                        strErrMsg = TypeConvert.ToString(RSysSystem.GetLDGroup(modProduct.strgPRODUCT).GetText(modProduct.strdDELETION_IS_CANCELED));
                    }
                }
                else
                {
                    pForm.DoDeleteFormData(RecordId, ref ParameterList);
                }

                ParameterList = new object[] { strErrMsg };
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This transition point method gets triggered by the AppServer when Execute is called
        /// in client scripting. It provides a general entry point to AppServer business
        /// rule methods to execute a specified method when Execute is called in client scripting.
        /// </summary>
        /// <param name="pForm">IRForm object reference to the client IRForm object that calls Execute.</param>
        /// <param name="MethodName">Method name to be executed</param>
        /// <param name="ParameterList">Transition point parameters passed from the client to AppServer for business rule processing.
        /// ParameterList - Transit point parameters passed from AppServer back to the client
        /// side may contain the executed results.
        /// </param>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       4/28/2006   JHui        Converted to .Net C# code.
        /// </history>
        public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {

            try
            {
                string strArgument = String.Empty;
                string errMessage = String.Empty;
                object executeParameters = null;

                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;
                object[] parameterArray = objParam.GetUserDefinedParameterArray();

                switch (MethodName)
                {
                    case modProduct.strmGET_CONSTRUCTION_STAGE:
                        parameterArray = GetConstructionStage(parameterArray[0]);
                        break;
                    case modProduct.strmGET_NUMBER_OF_QUOTES:
                        parameterArray = new object[] { GetNumberOfQuotes(parameterArray[0]) };
                        break;
                    case modProduct.strmGET_LOT_TYPE:
                        parameterArray = new object[] { GetLotType(parameterArray[0]) };
                        break;
                    case modProduct.strmGET_INV_QUOTE:
                        parameterArray = GetInventoryQuote(parameterArray[0]);
                        break;
                    case modProduct.strmUPDATE_LOT_STATUS:
                        UpdateLotStatus(parameterArray[0], TypeConvert.ToString(parameterArray[1]));
                        break;
                    case modProduct.strmUPDATE_LOT_PRICING:
                        parameterArray = new object[] { UpdateLotPricing(parameterArray[0], ref executeParameters) };
                        break;
                    case modProduct.strmBATCH_UPDATE_LOT_STATUS:
                        BatchUpdateLotStatus();
                        break;
                    case modProduct.strmCAN_BE_INACTIVATED:
                        CanBeInactivated(ref parameterArray);
                        break;
                    case modProduct.strmCAN_LOT_BE_CREATED:
                        parameterArray = new object[] { CanLotBeCreated(parameterArray) };
                        break;
                    case modProduct.strmVERIFY_RULES:
                        VerifyRules(parameterArray[0], ref parameterArray[1]);
                        break;
                    default:
                        errMessage = MethodName + TypeConvert.ToString(RldtLangDict.GetText(modProduct.strdINVALID_METHOD));
                        throw new PivotalApplicationException(errMessage, modProduct.glngERR_START_NUMBER + 1);
                }

                // Add the returned values into transit point parameter list
                ParameterList = objParam.SetUserDefinedParameterArray(parameterArray);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This transition point method gets triggered by the AppServer before an existing
        /// record is loaded. It provides a custom entry point to load form data, consisting
        /// of an array of primary and secondary recordsets, from the database for Product
        /// object.
        /// </summary>
        /// <param name="pForm">IRForm object reference to the client IRForm object where the LoadFormData occurs</param>
        /// <param name="RecordId">Record Id for Product object</param>
        /// <param name="ParameterList">Transit point parameters passed from the client to AppServer for business rule processing.
        /// ParameterList - Transit point parameters passed from AppServer back to the client
        /// side.
        /// </param>
        /// <returns>
        /// Variant to hold an array including primary recordset and its secondaries loaded from the database for the Product object.</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       4/28/2006   JHui        Converted to .Net C# code.
        /// </history>
        public virtual object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                IRServerScript rscriptAlert = null;
                Recordset rstPrimary = null;
                object Recordsets = null;
                Recordset rstAlert = null;

                Recordsets = pForm.DoLoadFormData(RecordId, ref ParameterList);
                object[] recordsetArray = (object[])Recordsets;
                rstPrimary = (Recordset)recordsetArray[0];

                // checking and seting of the system parameters
                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;
                if (!ocmsTransitPointParams.HasValidParameters())
                {
                    ocmsTransitPointParams.Construct();
                }
                else
                {
                    ocmsTransitPointParams.SetDefaultFields(rstPrimary);
                }


                try
                {
                    // Find Alert and return a recordset of Alert Id's to client thru ParameterList
                    rscriptAlert = RSysSystem.ServerScripts[modProduct.strsALERT];
                    Alert objAlert = (Alert)rscriptAlert.CreateInstance();
                    rstAlert = objAlert.FindValidAlerts(RecordId, "Lot");
                }
                catch
                {
                    //No exception handling for the above code block.
                }

                ocmsTransitPointParams.SetUserDefinedParameter(1, rstAlert);
                ParameterList = ocmsTransitPointParams.ParameterList;
                return Recordsets;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This transition point method gets triggered by the AppServer before a new record is
        /// created. It provides a custom entry point to create new form data, consisting of
        /// an array of primary and secondary recordsets, from the database for the Product object.
        /// </summary>
        /// <param name="pForm">IRForm object reference to the client IRForm object where the NewFormData occurs</param>
        /// <param name="ParameterList">Transit point parameters passed from the client to AppServer for business rule processing.
        /// ParameterList - Transit point parameters passed from AppServer back to the client
        /// side.
        /// </param>
        /// <returns>
        /// Variant to hold an array of recordsets including primary and secondaries created for the Product object.</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       4/28/2006   JHui        Converted to .Net C# code.
        /// </history>
        public virtual object NewFormData(IRForm pForm, ref object ParameterList)
        {
            try
            {
                object vntRecordsets = null;
                TransitionPointParameter objParam = null;
                Recordset rstPrimary = null;

                vntRecordsets = pForm.DoNewFormData(ref ParameterList);
                object[] recordsetArray = (object[])vntRecordsets;
                rstPrimary = (Recordset)recordsetArray[0];

                objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;
                if (!objParam.HasValidParameters())
                {
                    objParam.Construct();
                }
                else
                {
                    objParam.SetDefaultFields(rstPrimary);
                }

                return vntRecordsets;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This transition point method gets triggered by the AppServer before a new secondary
        /// record is created. It provides a custom entry point to create a new secondary
        /// record from the database for a specified secondary of Product object.
        /// </summary>
        /// <param name="pForm">IRForm object reference to the client IRForm object where the NewSecondaryData occurs</param>
        /// <param name="SecondaryName">Segment name to hold the secondary data</param>
        /// <param name="ParameterList">Transit point parameters passed from the client to AppServer for business rule processing</param>
        /// <param name="recordset">Secondary recordset containing structure information</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       4/28/2006   JHui        Converted to .Net C# code.
        /// </history>
        public virtual void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset recordset)
        {
            try
            {
                pForm.DoNewSecondaryData(SecondaryName, ref ParameterList, recordset);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This transition point method gets triggered by the AppServer before a record is
        /// saved. It provides a custom entry point to update form data, consisting of an
        /// array of primary and secondary recordsets, to the database for the Product
        /// object. If the product price has been changed, this function updates the price
        /// book of this product.
        /// </summary>
        /// <param name="pForm">IRForm object reference to the client IRForm object where the SaveFormData occurs</param>
        /// <param name="Recordsets">Variant to hold an array, including the primary recordset and its
        /// secondaries, in the specified form for the Product object to
        /// be saved to the database.</param> 
        /// <param name="ParameterList">Transit point parameters passed from the client to AppServer for business rule processing.
        /// ParameterList - Transit point parameters passed from AppServer back to the client
        /// side.</param>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       4/28/2006   JHui        Converted to .Net C# code.
        /// 5.9.0.0       March/01/2007 ML          Replaced the check for Construction Stage
        /// </history>
        public virtual void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                Recordset rstRecordset = null;
                object vntConstructionStageId = DBNull.Value;
                object vntLotID = DBNull.Value;
                object vntParam = null;
                object vntSoldDate = null;
                object vntReleasedDate = null;
                object vntClosedDate = null;
                object vntReservedDate = null;
                string strOldStatus = String.Empty;
                string strNewStatus = String.Empty;
                object vntInactive = null;
                object vntPriceHistoryEval = null;
                object vntSavConstStageId = DBNull.Value;

                object[] arrayRecs = (object[])Recordsets;
                rstRecordset = (Recordset)arrayRecs[0];

                IRFormScript objOpportunity = (IRFormScript)RSysSystem.ServerScripts[modProduct.strsOPPORTUNITY].CreateInstance();

                // BHan - Mar. 22, 2005
                // Apply the Status Rules based on several Dates
                vntSoldDate = rstRecordset.Fields[modProduct.strfSALES_DATE].Value;
                vntReleasedDate = rstRecordset.Fields[modProduct.strfRELEASE_DATE].Value;
                vntClosedDate = rstRecordset.Fields[modProduct.strfCONTRACT_CLOSE_DATE].Value;
                vntReservedDate = rstRecordset.Fields[modProduct.strfRESERVED_DATE].Value;
                strOldStatus = "" + rstRecordset.Fields[modProduct.strfLOT_STATUS].Value;
                vntInactive = rstRecordset.Fields[modProduct.strfINACTIVE].Value;
                strNewStatus = GetLotStatusByRules(vntSoldDate, vntReleasedDate, vntClosedDate, vntReservedDate, TypeConvert.ToBoolean(vntInactive));
                if (strOldStatus.ToUpper() != strNewStatus.ToUpper())
                {
                    rstRecordset.Fields[modProduct.strfLOT_STATUS].Value = strNewStatus;
                }
                vntConstructionStageId = rstRecordset.Fields[modProduct.strfCONSTRUCTION_STAGE_ID].Value;
                vntSavConstStageId = RSysSystem.Tables[modProduct.strtPRODUCT].Fields[modProduct.strfCONSTRUCTION_STAGE_ID].Index(rstRecordset.Fields[modProduct.strfPRODUCT_ID].Value);

                // update delta fields
                UpdateDeltaFields(rstRecordset);

                pForm.DoSaveFormData(Recordsets, ref ParameterList);

                // update the options built flag
                TransitionPointParameter transitPointLib = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                object executeParameters = transitPointLib.Construct();
                vntLotID = rstRecordset.Fields[modProduct.strfPRODUCT_ID].Value;
                //ML- commented - as this would always be true
                //hence replaced the check with old Construction Stage in DB 
                //if (!mrsysSystem.EqualIds(vntConstructionStageId, rstRecordset.Fields[modProduct.strfCONSTRUCTION_STAGE_ID].Value))
                if (!RSysSystem.EqualIds(vntConstructionStageId, vntSavConstStageId))
                {
                    //ML- Commented - as from now Opportunity__Product records which were added after Cut-Off stage
                    //should be updated during the Save/Apply action since the user gets a choice of doing so
                    //while changing the Construction Stage on Homesite
                    //object executeUserParameters = new object[] { vntLotID, vntConstructionStageId };
                    object executeUserParameters = new object[] { vntLotID, vntConstructionStageId, false };
                    executeParameters = transitPointLib.SetUserDefinedParameterArray(executeUserParameters);
                    objOpportunity.Execute(pForm, modProduct.strmUPDATE_QUOTE_OPTIONS, ref executeParameters);
                }
                // added by Carl Langan 01/04/05 for integration
                IRAppScript objIntegration = (IRAppScript)RSysSystem.ServerScripts[modProduct.strsINTEGRATION].CreateInstance();
                objIntegration.Execute(modProduct.strmIS_INTEGRATION_ON, ref vntParam);
                if (vntParam is Array)
                {
                    object[] arrayParameters = (object[])vntParam;
                    if (arrayParameters.GetUpperBound(0) >= 6)
                    {
                        if (TypeConvert.ToBoolean(arrayParameters[6]))
                        {
                            vntParam = new object[] { rstRecordset.Fields[modProduct.strfPRODUCT_ID].Value };
                            objIntegration.Execute(modProduct.strmNOTIFY_INTEGRATION_OF_LOT_CHANGE, ref vntParam);
                        }
                    }
                }

                // check if any pricing updates need to be run
                if (pForm.FormName == modProduct.strrLOT_ADMIN)
                {
                    UpdateLotPricing(rstRecordset.Fields[modProduct.strfPRODUCT_ID].Value, ref vntPriceHistoryEval);
                    if (!(vntPriceHistoryEval == null))
                    {
                        PriceChangeHistory objPCH = (PriceChangeHistory)RSysSystem.ServerScripts[modProduct.strsPRICE_CHANGE_HISTORY].CreateInstance();
                        objPCH.MarkPriceHistoryProcessed((object[])vntPriceHistoryEval);
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Entry point for the ASR.
        /// </summary>
        /// <param name="pSystem">Contains the current System Instance Reference</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       4/28/2006   JHui        Converted to .Net C# code.
        /// </history>
        public virtual void SetSystem(RSystem pSystem)
        {
            try
            {
                if (RSysSystem == null)
                {
                    modProduct.gdtLastBatchUpdateLotStatusRun = new DateTime(0);
                }
                RSysSystem = (IRSystem7)pSystem;
                RldtLangDict = RSysSystem.GetLDGroup(modProduct.strgPRODUCT);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks to see if the form has the indicated secondaries.  If any of the secondaries is not empty, then
        /// the Product is not allowed to be deleted.
        /// </summary>
        /// <param name="pForm">IRForm object</param>
        /// <param name="vntRecordId">Record Id</param>
        /// <param name="strErrMsg">Error message</param>
        /// <returns>True if the product can be deleted, False otherwise.</returns>
        /// <history>
        /// Revision # Date     Author  Description
        /// 3.8.0.0  4/28/2006  JHui    Converted to .Net C# code.
        /// </history>
        protected virtual bool CanBeDeleted(IRForm pForm, object vntRecordId, ref string strErrMsg)
        {
            try
            {
                bool bCanBeDeleted = false;
                object vntForm = null;
                object item = null;
                string strItem = String.Empty;
                int i = 0;
                object parameterList = null;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstRecordset = objLib.GetRecordset(modProduct.strqPRICE_CHNG_HISTORY_FOR_LOT, 1, vntRecordId, modProduct.strfPRICE_CHANGE_HISTORY_ID, modProduct.strfCHANGE_DATE);
                if (rstRecordset.RecordCount > 1)
                {
                    strItem = modProduct.strsegPREMIUM_HISTORY;
                }
                // Init
                i = 0;

                // Get the Form
                vntForm = pForm.DoLoadFormData(vntRecordId, ref parameterList);
                object[] recordsetArray = (object[])vntForm;

                // Set up the Segments array
                object[] arrSegments = new object[] {modProduct.strsegQUOTES, modProduct.strsegINSPECTIONS,
                    modProduct.strsegSERVICE_REQUESTS, modProduct.strsegAGREEMENT};

                while (strItem.Length == 0 && i <= arrSegments.GetUpperBound(0))
                {
                    bCanBeDeleted = !SecondaryExists(pForm, vntForm, TypeConvert.ToString(arrSegments[i]), ref item);
                    strItem = TypeConvert.ToString(item);
                    i++;
                }

                if (!bCanBeDeleted)
                {
                    strErrMsg = TypeConvert.ToString(RldtLangDict.GetTextSub(modProduct.strdDELETION_CANCELED,
                        new object[] { strItem }));
                }
                else
                {
                    strErrMsg = string.Empty;
                    // continue with the deletion, delete these child records
                    // 1  - Alerts
                    objLib.DeleteSecondary(modProduct.strqALERT_FOR_LOT, vntRecordId);
                    // 2  - Image_Attachment
                    objLib.DeleteSecondary(modProduct.strqIMAGE_ATTACHMENT_FOR_LOT, vntRecordId);
                    // 3 - Lot__Company
                    objLib.DeleteSecondary(modProduct.strqCONTRACTORS_FOR_LOT, vntRecordId);
                    // 4 - Lot__Contact
                    objLib.DeleteSecondary(modProduct.strqCONTACTS_FOR_LOT, vntRecordId);
                    // 5 - Lot_Plan
                    objLib.DeleteSecondary(modProduct.strqLOT_PLANS_FOR_LOT, vntRecordId);
                    // 6 - Price_Change_History - The only 1 record
                    objLib.DeleteSecondary(modProduct.strqPRICE_CHNG_HISTORY_FOR_LOT, vntRecordId);

                    // clear out these links
                    // Rn_Appontments
                    objLib.SetForeignKeyFieldNullByQuery(modProduct.strqRN_APPTS_FOR_LOT, modProduct.strfLOT_ID, vntRecordId);

                    // Literature
                    objLib.SetForeignKeyFieldNullByQuery(modProduct.strqLITERATURE_ITEMS_WITH_PRODUCT, modProduct.strfPRODUCT_ID, vntRecordId);
                }

                return bCanBeDeleted;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks to see if a secondary exists.
        /// </summary>
        /// <param name="pForm">IRForm reference.</param>
        /// <param name="vntForm">Array of primary recordset and secondaries.</param>
        /// <param name="strSection">Section name</param>
        /// <param name="strItem">Section where item is found</param>
        /// <returns>True if the specified secondary is found, False otherwise.</returns>
        /// <history>
        /// Revision # Date     Author  Description
        /// 3.8.0.0  4/28/2006  JHui    Converted to .Net C# code.
        /// </history>
        protected virtual bool SecondaryExists(IRForm pForm, object vntForm, string strSection, ref object strItem)
        {
            bool bSecondaryExists = false;
            try
            {

                Recordset rstForm_Secondary = null;

                bSecondaryExists = false;

                rstForm_Secondary = pForm.SecondaryFromVariantArray(vntForm, strSection);

                if (rstForm_Secondary.RecordCount > 0)
                {
                    strItem = strSection;
                    bSecondaryExists = true;
                }


            }
            catch
            {
                //Ignore exceptions (original design in VB function)
            }
            return bSecondaryExists;
        }

        /// <summary>
        /// This function gets the total number of quotes for a Lot.
        /// </summary>
        /// <param name="vntLotID">Lot ID</param>
        /// <returns>Number of quotes for the Lot</returns>
        /// <history>
        /// Revision # Date     Author  Description
        /// 3.8.0.0  4/28/2006  JHui    Converted to .Net C# code.
        /// </history>
        protected virtual int GetNumberOfQuotes(object vntLotID)
        {
            try
            {
                Recordset rstRecordset = null;
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                rstRecordset = objLib.GetRecordset(modProduct.strqQUOTE_FOR_LOT, 1, vntLotID, modProduct.strfQUOTE_ID);
                return rstRecordset.RecordCount;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// To update the delta fields.
        /// </summary>
        /// <param name="rstProduct">Product recordset</param>
        /// <returns></returns>
        /// <history>
        /// Revision # Date     Author  Description
        /// 3.8.0.0  4/28/2006  JHui    Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateDeltaFields(Recordset rstProduct)
        {
            try
            {
                rstProduct.Fields[modProduct.strfDELTA_LOT_STATUS].Value = rstProduct.Fields[modProduct.strfLOT_STATUS].Value;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// To update Lot Status.
        /// </summary>
        /// <param name="vntLotID">Lot ID</param>
        /// <param name="vntStatus">New status</param>
        /// <returns>True if status updated, False otherwise.</returns>
        /// <history>
        /// Revision # Date     Author  Description
        /// 3.8.0.0  4/28/2006  JHui    Converted to .Net C# code.
        /// </history>
        protected virtual bool UpdateLotStatus(object vntLotID, string vntStatus)
        {
            try
            {
                DataAccess objLib = null;
                Recordset rstLot = null;

                objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                rstLot = objLib.GetRecordset(vntLotID, modProduct.strtPRODUCT, modProduct.strfLOT_STATUS, modProduct.strfDELTA_LOT_STATUS);

                if (!rstLot.EOF && !rstLot.BOF)
                {
                    rstLot.Fields[modProduct.strfLOT_STATUS].Value = vntStatus;
                    rstLot.Fields[modProduct.strfDELTA_LOT_STATUS].Value = vntStatus;
                    objLib.SaveRecordset(modProduct.strtPRODUCT, rstLot);
                }

                return true;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lotId"></param>
        /// <returns></returns>
        public virtual bool UpdateLotPricing(object lotId)
        {
            object priceHistoryEvaluates = DBNull.Value;
            return this.UpdateLotPricing(lotId, ref priceHistoryEvaluates);
        }

        /// <summary>
        /// Given the product, searches for all quotes that have that lot and updates it.
        /// Update the current price field and next price field with the price secondary.
        /// a. Initial Population of Options use "Build Option Pricing"
        /// Fixed    --> use prices on Opp Product
        /// Floating --> use Product current Price
        /// b. Additional Option Selection --> use Product current Price
        /// Fixed    --> use price as of the Sales Request date and in the case of the Post Sale use the sale date of
        /// the contract
        /// Floating --> use Product current price
        /// </summary>
        /// <param name="vntLotID">Lot ID</param>
        /// <param name="vntPriceHistoryEval">Array of price history values</param>
        /// <returns>True if updated, False otherwise.</returns>
        /// <history>
        /// Revision# Date      Author  Description
        /// 3.8.0.0   4/28/2006 JHui    Converted to .Net C# code.
        /// 5.9.0.0   2/08/2007 ML      Changes related to Cost_Price,Margin and Post_CutOff_Price
        ///                             fields in Price_Change_History table
        /// </history>
        public virtual bool UpdateLotPricing(object vntLotID, ref object vntPriceHistoryEval)
        {
            try
            {
                bool UpdateLotPricing = false;
                Recordset rstPriceHistory = null;
                Recordset rstLot = null;
                Recordset rstQuotes = null;
                bool blnDataChanged = false;
                DateTime vntNextDate;
                decimal vntNextPrice;
                decimal dblNewPrice = 0;
                DateTime vntNewDate;
                bool blnStandard = false;
                DateTime dteNextStandardUpdate;
                bool blnInclHomsitePremium = false;
                object vntDivisionId = DBNull.Value;
                object vntLot_Type = null;
                object vntQuote_Status = null;
                object vntPipeline_Stage = null;
                bool blnUpdatePrice = false;
                object vntBuildOption = null;
                object vntCurrentPrice = null;
                object vntQuoteId = DBNull.Value;
                object vntStndOption = null;
                object vntPlanPrice = null;
                decimal dblNewCostPrice = 0;
                decimal dblNewMargin = 0;
                decimal dblNewPostCutOffPrice = 0;
                decimal dblNextCostPrice = 0;
                decimal dblNextMargin = 0;
                decimal dblNextPostCutOffPrice = 0;

                UpdateLotPricing = false;

                // verify passed parameters
                if (vntLotID is Array)
                {
                    // define objects
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    PriceChangeHistory objPCH = (PriceChangeHistory)RSysSystem.ServerScripts[modProduct.strsPRICE_CHANGE_HISTORY].CreateInstance();

                    // set to no changes
                    blnDataChanged = false;

                    // get list of all price change history records for the given Lot
                    // Add field "Standard" to the recordset rstPriceHistory           
                    objLib.SortAscending = true;
                    objLib.SortFieldName = modProduct.strfCHANGE_DATETIME;

                    rstPriceHistory = objLib.GetRecordset(modProduct.strqPRICE_CHNG_HISTORY_FOR_LOT, 1, vntLotID, modProduct.strfPRICE,
                        modProduct.strfCHANGE_DATE, modProduct.strfPROCESSED, modProduct.strfSTANDARD, modProduct.strfCHANGE_DATETIME,
                        modProduct.strfPCH_COST_PRICE, modProduct.strfPCH_MARGIN, modProduct.strfPCH_POST_CUT_OFF_PRICE);

                    // read through price history, returns list of records evaluated
                    vntPriceHistoryEval = objPCH.ProcessPriceHistory(rstPriceHistory, out vntNextDate, out vntNextPrice, out dblNewPrice, out blnStandard,
                        out dteNextStandardUpdate, out vntNewDate, out dblNextCostPrice, out dblNextMargin, out dblNextPostCutOffPrice, out dblNewCostPrice,
                        out dblNewMargin, out dblNewPostCutOffPrice);

                    // open the Lot record
                    rstLot = objLib.GetRecordset(vntLotID, modProduct.strtPRODUCT, modProduct.strfPRICE, modProduct.strfNEXT_PRICE,
                        modProduct.strfPRICE_CHANGE_DATE, modProduct.strfLOT_PREMIUM_CHANGE, modProduct.strfLOT_PREMIUM_PRICE_UPDATE_DATE,
                        modProduct.strfNBHD_PHASE_ID);
                    if (!rstLot.BOF || rstLot.EOF)
                    {
                        vntDivisionId = RSysSystem.Tables[modProduct.strtNBHD_PHASE].Fields[modProduct.strfDIVISION_ID].Index(rstLot.Fields[modProduct.strfNBHD_PHASE_ID].Value);
                        // see if the data has changed
                        if ((TypeConvert.ToDecimal(rstLot.Fields[modProduct.strfPRICE].Value) == dblNewPrice) ||
                            (TypeConvert.ToDecimal(rstLot.Fields[modProduct.strfNEXT_PRICE].Value) != vntNextPrice) ||
                            (TypeConvert.ToDateTime(rstLot.Fields[modProduct.strfPRICE_CHANGE_DATE].Value).ToShortDateString() != vntNextDate.ToShortDateString()) ||
                            Convert.IsDBNull(rstLot.Fields[modProduct.strfNEXT_PRICE].Value) || Convert.IsDBNull(rstLot.Fields[modProduct.strfPRICE_CHANGE_DATE].Value))
                        {
                            // if so make the changes, this stops possible unnecessary changes being made
                            rstLot.Fields[modProduct.strfLOT_PREMIUM_CHANGE].Value = dblNewPrice - TypeConvert.ToDecimal(rstLot.Fields[modProduct.strfPRICE].Value);
                            rstLot.Fields[modProduct.strfPRICE].Value = dblNewPrice;
                            //if (TypeConvert.IsDBNull(rstLot.Fields[modProduct.strfPRICE_CHANGE_DATE].Value))
                            if (vntNextDate == TypeConvert.ToDateTime(DBNull.Value))
                            {
                                rstLot.Fields[modProduct.strfNEXT_PRICE].Value = DBNull.Value;
                                rstLot.Fields[modProduct.strfPRICE_CHANGE_DATE].Value = DBNull.Value;
                            }
                            else
                            {
                                rstLot.Fields[modProduct.strfNEXT_PRICE].Value = vntNextPrice;
                                rstLot.Fields[modProduct.strfPRICE_CHANGE_DATE].Value = vntNextDate;
                            }
                            rstLot.Fields[modProduct.strfLOT_PREMIUM_PRICE_UPDATE_DATE].Value = vntNewDate;
                            blnDataChanged = true;
                            objLib.SaveRecordset(modProduct.strtPRODUCT, rstLot);
                        }
                    }
                    if (blnDataChanged)
                    {
                        // something changed so process all required quotes - see query and design spec for details
                        rstQuotes = objLib.GetRecordset(modProduct.strqPRICE_UPDATE_LOTS, 3, vntLotID, vntLotID, vntLotID,
                            modProduct.strfPRICE, modProduct.strfQUOTE_ID, modProduct.strfPRICE_UPDATE, modProduct.strfLOT_PREMIUM,
                            modProduct.strfSTATUS, modProduct.strfPIPELINE_STAGE, modProduct.strfOPPORTUNITY_ID, modProduct.strfPLAN_BUILT,
                            modProduct.strfQUOTE_CREATE_DATE, modProduct.strfCONTRACT_APPROVED_SUBMITTED, modProduct.strfPLAN_ID);
                        // update linked quotes

                        vntBuildOption = RSysSystem.Tables[modProduct.strtDIVISION].Fields[modProduct.strfBUILD_OPTION_PRICING].Index(vntDivisionId);
                        blnInclHomsitePremium = Convert.ToBoolean(RSysSystem.Tables[modProduct.strtDIVISION].Fields[modProduct.strfINCLUDE_HOMESITE_PREMIUM].Index(vntDivisionId));
                        vntStndOption = RSysSystem.Tables[modProduct.strtDIVISION].Fields[modProduct.strfSTANDARD_OPTION_PRICING].Index(vntDivisionId);

                        blnUpdatePrice = false;
                        if (!rstQuotes.EOF && !rstQuotes.BOF)
                        {
                            rstQuotes.MoveFirst();
                            while (!rstQuotes.EOF)
                            {
                                vntPlanPrice = RSysSystem.Tables[modProduct.strtNBHD_PRODUCT].Fields[modProduct.strfCURRENT_PRICE].Index(rstQuotes.Fields[modProduct.strfPLAN_ID].Value);
                                vntQuoteId = rstQuotes.Fields[modProduct.strfOPPORTUNITY_ID].Value;
                                vntPipeline_Stage = rstQuotes.Fields[modProduct.strfPIPELINE_STAGE].Value;
                                vntQuote_Status = rstQuotes.Fields[modProduct.strfSTATUS].Value;

                                if (TypeConvert.ToString(vntPipeline_Stage) == modProduct.strPIPELINE_QUOTE)
                                {
                                    vntLot_Type = RSysSystem.Tables[modProduct.strtPRODUCT].Fields[modProduct.strfTYPE].Index(vntLotID);
                                    // two situations
                                    if (TypeConvert.ToString(vntQuote_Status) != modProduct.strQUOTE_STATUS_INVENTORY
                                        && (Convert.IsDBNull(vntLotID) || TypeConvert.ToString(vntLot_Type) != modProduct.strLOT_TYPE_INVENTORY))
                                    {
                                        // is always the current price of the Option based on the quote
                                        blnUpdatePrice = true;
                                    }
                                    else if (TypeConvert.ToString(vntLot_Type) == modProduct.strLOT_TYPE_INVENTORY &&
                                        (TypeConvert.ToString(vntQuote_Status) == modProduct.strQUOTE_STATUS_INVENTORY
                                        || TypeConvert.ToString(vntQuote_Status) == modProduct.strQUOTE_STATUS_IN_PROGRESS
                                        || TypeConvert.ToString(vntQuote_Status) == modProduct.strQUOTE_STATUS_RESERVED))
                                    {
                                        // depends on division settings

                                        if (TypeConvert.ToInt32(vntBuildOption) == modProduct.intBUILD_OPTION_FIXED)
                                        {
                                            // fixed, only update if not built
                                            if (!TypeConvert.ToBoolean(rstQuotes.Fields[modProduct.strfPLAN_BUILT].Value))
                                            {
                                                blnUpdatePrice = true;
                                            }
                                            else
                                            {
                                                blnUpdatePrice = false;
                                            }
                                        }
                                        else if (TypeConvert.ToInt32(vntBuildOption) == modProduct.intBUILD_OPTION_FLOATING)
                                        {
                                            // floating, always update
                                            blnUpdatePrice = true;
                                        }
                                    }
                                }
                                else if (TypeConvert.ToString(vntPipeline_Stage) == modProduct.strPIPELINE_POST_BUILD_QUOTE)
                                {
                                    if (TypeConvert.ToString(vntQuote_Status) == modProduct.strQUOTE_STATUS_IN_PROGRESS)
                                    {
                                        // depends on division settings
                                        if (TypeConvert.ToInt32(vntBuildOption) == modProduct.intBUILD_OPTION_FIXED)
                                        {
                                            // fixed, only update if not built
                                            if (!TypeConvert.ToBoolean(rstQuotes.Fields[modProduct.strfPLAN_BUILT].Value))
                                            {
                                                blnUpdatePrice = true;
                                            }
                                            else
                                            {
                                                blnUpdatePrice = false;
                                            }
                                        }
                                        else if (TypeConvert.ToInt32(vntBuildOption) == modProduct.intBUILD_OPTION_FLOATING)
                                        {
                                            // floating, always update
                                            blnUpdatePrice = true;
                                        }
                                    }

                                }
                                else if (TypeConvert.ToString(vntPipeline_Stage) == modProduct.strPIPELINE_SALES_REQUEST || TypeConvert.ToString(vntPipeline_Stage)
                                    == modProduct.strPIPELINE_POST_SALE)
                                {
                                    // no updates
                                }
                                else if (TypeConvert.ToString(vntPipeline_Stage) == modProduct.strPIPELINE_CANCELED || TypeConvert.ToString(vntPipeline_Stage) ==
                                    modProduct.strPIPELINE_CLOSED)
                                {
                                    // Never gets updated
                                }

                                if (blnUpdatePrice)
                                {
                                    // price is what the current price of the Lot is
                                    vntCurrentPrice = dblNewPrice;
                                    // update price only if the include homsite preimium flag is not true otherwise
                                    // include this new price with the plan
                                    if (blnInclHomsitePremium)
                                    {
                                        // Plan premium price includes the Homesite Premium
                                        rstQuotes.Fields[modProduct.strfPRICE].Value = Convert.ToDouble(vntPlanPrice)
                                            + TypeConvert.ToDouble(vntCurrentPrice);
                                    }
                                    else
                                    {
                                        rstQuotes.Fields[modProduct.strfLOT_PREMIUM].Value = vntCurrentPrice;
                                    }

                                    rstQuotes.Fields[modProduct.strfPRICE_UPDATE].Value = 1;
                                }

                                rstQuotes.MoveNext();
                            }
                            objLib.SaveRecordset(modProduct.strtQUOTE, rstQuotes);
                        }
                    }
                }

                // mark successful finish
                UpdateLotPricing = true;
                return UpdateLotPricing;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Check to see if the lot can be created - a lot cannot have the same lot number in the same release.
        /// </summary>
        /// <param name="ParameterList">Array of parameters.  First one is Lot Number, second is release ID and third one is Lot ID.</param>
        /// <returns>A string containing an error message, if a duplicate lot number is found.</returns>
        /// <history>
        /// Revision# Date      Author  Description
        /// 3.8.0.0   4/28/2006 JHui    Converted to .Net C# code.
        /// </history>
        protected virtual string CanLotBeCreated(object[] ParameterList)
        {

            try
            {
                string strErrMsg = String.Empty;
                Recordset rstRecordset = null;
                string strLotNumber = String.Empty;
                object vntReleaseId = DBNull.Value;
                object vntLotID = DBNull.Value;

                // retrive the values
                strLotNumber = TypeConvert.ToString(ParameterList[0]);
                vntReleaseId = ParameterList[1];
                vntLotID = ParameterList[2];

                strErrMsg = string.Empty;
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                rstRecordset = objLib.GetRecordset(modProduct.strqLOT_NUMBER_AND_RELEASE, 3, strLotNumber, vntReleaseId, vntLotID, modProduct.strfPRODUCT_ID);
                if (rstRecordset.RecordCount > 0)
                {
                    strErrMsg = TypeConvert.ToString(RldtLangDict.GetTextSub(modProduct.strdLOT_EXISTS,
                        new object[] { strLotNumber, RSysSystem.Tables[modProduct.strtNBHD_PHASE].Fields[modProduct.strfRN_DESCRIPTOR].Index(vntReleaseId) }));
                }

                return strErrMsg;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function updates active releases based on the rules below.
        /// Rule 1: If Closed Date .le. Current Date then set Status to "Closed"
        /// Rule 2: If Sale Date .le. Current Date and no Closed Date then set the Status to "Sold"
        /// Rule 3: If Reserved Date .le. Current Date and No Sold or Closed date on Lot then set Status to "Reserved"
        /// Rule 4: If Released date .le. Current date and No Reserved date or Sold or Closed date on Lot then set Status to "Available"
        /// Rule 5: If Released date is Null or  .gt.  Current date and No Reserved date or Sold or Closed date on Lot then set Status to "Not Released
        /// Notes:     - Closed Date is Release_Date in table
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision # Date     Author    Description
        /// 3.8.0.0  4/28/2006  JHui     Converted to .Net C# code.
        /// </history>
        public virtual void BatchUpdateLotStatus()
        {
            try
            {
                Recordset rstLot = null;
                int lngRecordCount = 0;
                int lngLoop = 0;
                object vntSoldDate = null;
                object vntReleasedDate = null;
                object vntClosedDate = null;
                object vntReservedDate = null;
                string strOldStatus = String.Empty;
                string strNewStatus = String.Empty;
                object vntInactive = null;
                int intFirstRunPeriod = 0;
                Recordset rstSystem = null;
                DateTime dtQueryDate;
                DateTime dtTemp;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                if (modProduct.gdtLastBatchUpdateLotStatusRun.Ticks == 0)
                {
                    rstSystem = objLib.GetRecordset(modProduct.strtSYSTEM, modProduct.strfSCHEDULED_SCRIPT_DAYS);
                    if (rstSystem.RecordCount == 1)
                    {
                        rstSystem.MoveFirst();

                        int tempScheduledScriptDays;
                        if (int.TryParse(TypeConvert.ToString(rstSystem.Fields[modProduct.strfSCHEDULED_SCRIPT_DAYS].Value), out tempScheduledScriptDays))
                        {
                            intFirstRunPeriod = (TypeConvert.ToInt32(rstSystem.Fields[modProduct.strfSCHEDULED_SCRIPT_DAYS].Value));
                            modProduct.gdtLastBatchUpdateLotStatusRun = DateTime.Now.AddDays(intFirstRunPeriod * -1);
                        }
                    }
                }

                dtQueryDate = modProduct.gdtLastBatchUpdateLotStatusRun;
                dtTemp = DateTime.Now.AddMinutes(-1);
                rstLot = objLib.GetRecordset(modProduct.strqALL_ACTIVE_LOTS_NEWER, 1, dtQueryDate, modProduct.strfSALES_DATE, modProduct.strfRELEASE_DATE, modProduct.strfCONTRACT_CLOSE_DATE, modProduct.strfRESERVED_DATE, modProduct.strfLOT_STATUS, modProduct.strfINACTIVE, "Rn_Edit_Date");

                lngRecordCount = rstLot.RecordCount;

                if (lngRecordCount > 0)
                {
                    rstLot.MoveFirst();
                }
                for (lngLoop = 0; lngLoop <= lngRecordCount - 1; lngLoop += 1)
                {
                    vntSoldDate = rstLot.Fields[modProduct.strfSALES_DATE].Value;
                    vntReleasedDate = rstLot.Fields[modProduct.strfRELEASE_DATE].Value;
                    vntClosedDate = rstLot.Fields[modProduct.strfCONTRACT_CLOSE_DATE].Value;
                    vntReservedDate = rstLot.Fields[modProduct.strfRESERVED_DATE].Value;
                    strOldStatus = "" + rstLot.Fields[modProduct.strfLOT_STATUS].Value;
                    vntInactive = rstLot.Fields[modProduct.strfINACTIVE].Value;
                    strNewStatus = GetLotStatusByRules(vntSoldDate, vntReleasedDate, vntClosedDate, vntReservedDate, TypeConvert.ToBoolean(vntInactive));

                    if (strOldStatus.ToUpper() != strNewStatus.ToUpper())
                    {
                        rstLot.Fields[modProduct.strfLOT_STATUS].Value = strNewStatus;
                    }

                    rstLot.MoveNext();
                }

                objLib.SaveRecordset(modProduct.strtPRODUCT, rstLot);

                modProduct.gdtLastBatchUpdateLotStatusRun = dtTemp;
                // only update date if successful
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function returns a new lot status based on the rules below.
        /// Rule 0: If Inactive is True, then set Status to Inactive
        /// Rule 1: If Closed Date .le. Current Date then set Status to "Closed"
        /// Rule 2: If Sold Date .le. Current Date and Closed Date is null then set Status to "Sold"
        /// Rule 3: If Reserved Date .le. Current Date and (Sales Date is null or Closed Date is null)
        /// then set Status to "Reserved"
        /// Rule 4: If Released Date .le. Current Date and (Reserved Date is null or Sold Date is null or Closed Date
        /// is null)
        /// then set Status to "Available"
        /// Rule 5: If (Released Date is null or Released Date .gt. Current Date) and (Reserved Date is null or Sold Date
        /// is null or Closed Date is null)
        /// then set Status to "Not Released"
        /// </summary>
        /// <param name="vntClosedDate">Contract closed date</param>
        /// <param name="vntInactive">Product Inactive flag.</param>
        /// <param name="vntReleasedDate">Release date</param>
        /// <param name="vntReservedDate">Reserved date</param>
        /// <param name="vntSoldDate">Sales date</param>
        /// <returns>The new status</returns>
        /// <history>
        /// Revision # Date     Author  Description
        /// 3.8.0.0  4/28/2006  JHui    Converted to .Net C# code.
        /// </history>
        protected virtual string GetLotStatusByRules(object vntSoldDate, object vntReleasedDate, object vntClosedDate, object vntReservedDate, bool vntInactive)
        {

            try
            {
                string GetLotStatusByRules = String.Empty;
                DateTime dtmCurrentDate;
                dtmCurrentDate = DateTime.Today;

                //Convert all DateTime parameters to Date only (remove time component)               
                DateTimeFormatInfo fmtInfo = new DateTimeFormatInfo();
                dtmCurrentDate = DateTime.ParseExact(dtmCurrentDate.ToString("d", fmtInfo), "d", fmtInfo);
                DateTime dtSoldDate = DateTime.ParseExact(TypeConvert.ToDateTime(vntSoldDate).ToString("d", fmtInfo), "d", fmtInfo);
                DateTime dtReleasedDate = DateTime.ParseExact(TypeConvert.ToDateTime(vntReleasedDate).ToString("d", fmtInfo), "d", fmtInfo);
                DateTime dtClosedDate = DateTime.ParseExact(TypeConvert.ToDateTime(vntClosedDate).ToString("d", fmtInfo), "d", fmtInfo);
                DateTime dtReservedDate = DateTime.ParseExact(TypeConvert.ToDateTime(vntReservedDate).ToString("d", fmtInfo), "d", fmtInfo);

                GetLotStatusByRules = string.Empty;

                // Apr. 7, 2005 - BH
                // Add Rule 0: If Inactive is True, then set Status to Inactive
                if (vntInactive == true)
                {
                    GetLotStatusByRules = modProduct.strLOT_STATUS_INACTIVE;
                    // Rule 0
                }
                else if (!Convert.IsDBNull(vntClosedDate) && dtClosedDate <= dtmCurrentDate)
                {
                    GetLotStatusByRules = modProduct.strLOT_STATUS_CLOSED;
                    // Rule 1
                }
                else if (!Convert.IsDBNull(vntSoldDate) && dtSoldDate <= dtmCurrentDate && Convert.IsDBNull(vntClosedDate))
                {
                    GetLotStatusByRules = modProduct.strLOT_STATUS_SOLD;
                    // Rule 2
                }
                else if (!Convert.IsDBNull(vntReservedDate) && dtReservedDate <= dtmCurrentDate && (Convert.IsDBNull(vntClosedDate) || Convert.IsDBNull(vntSoldDate)))
                {
                    GetLotStatusByRules = modProduct.strLOT_STATUS_RESERVED;
                    // Rule 3: Waiting for table redesign
                }
                else if (!Convert.IsDBNull(vntReleasedDate) && dtReleasedDate <= dtmCurrentDate && (Convert.IsDBNull(vntReservedDate) || Convert.IsDBNull(vntSoldDate) || Convert.IsDBNull(vntClosedDate)))
                {
                    GetLotStatusByRules = modProduct.strLOT_STATUS_AVAILABLE;
                    // Rule 4: Waiting for table redesign
                }
                else if (!Convert.IsDBNull(vntReleasedDate) && dtReleasedDate > dtmCurrentDate || Convert.IsDBNull(vntReleasedDate))
                {
                    GetLotStatusByRules = modProduct.strLOT_STATUS_UNAVAILABLE;
                    // Rule 5
                }

                return GetLotStatusByRules;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Checks the presence of any associated active secondary records like:
        /// Active Customer and Inventory
        /// Active Sales 
        /// Built Options ( Lot Configuration)
        /// </summary>
        /// <param name="vntParameterList">It returns the error message pertainng to the found records</param>
        /// <returns>True : If an active secondary is not found.</returns>
        /// <history>
        /// Revision # Date     Author  Description
        /// 3.8.0.0  4/28/2006  JHui    Converted to .Net C# code.
        /// </history>
        protected virtual bool CanBeInactivated(ref object[] vntParameterList)
        {

            try
            {
                bool CanBeInactivated = false;

                if (CheckCustomerInventoryQuote(ref vntParameterList))
                {
                    CanBeInactivated = false;
                }
                else if (CheckSalesTime(ref vntParameterList))
                {
                    CanBeInactivated = false;
                }
                else if (CheckLotConfiguration(ref vntParameterList))
                {
                    CanBeInactivated = false;
                }
                else
                {
                    vntParameterList[0] = true;
                    CanBeInactivated = true;
                }

                return CanBeInactivated;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Checks if there is any Quote record with Pipeline status equal to any of "In Progress",        
        /// "Reserved", "Closed" and "Inventory".
        /// </summary>
        /// <returns>True if there is active customer inventory, False otherwise.</returns>
        /// <history>
        /// Revision #   Date        Author  Description
        /// 3.8.0.0      4/28/2006   JHui    Converted to .Net C# code.
        /// </history>
        protected virtual bool CheckCustomerInventoryQuote(ref object[] vntParameterList)
        {

            try
            {
                bool CheckCustomerInventoryQuote = false;
                Recordset rstSecondary = null;
                object LotID = DBNull.Value;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                LotID = vntParameterList[0];
                // May 9, 2005 - BH
                // Use a new query
                rstSecondary = objLib.GetRecordset(modProduct.strqACTIVE_CUSTOMER_INVENTORY_NEW, 1, LotID, modProduct.strfLOT_ID);

                if (rstSecondary.RecordCount > 0)
                {
                    vntParameterList[0] = RldtLangDict.GetText(modProduct.strdACTIVE_CUSTOMER_INVENTORY);
                    CheckCustomerInventoryQuote = true;
                }
                else
                {
                    CheckCustomerInventoryQuote = false;
                }

                return CheckCustomerInventoryQuote;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Checks for Sales 
        /// </summary>
        /// <param name="vntParameterList"></param>
        /// <returns>True if there is active sales item, False otherwise.</returns>
        /// <history>
        /// Revision #  Date        Author  Description
        /// 3.8.0.0     4/28/2006   JHui    Converted to .Net C# code.
        /// </history>
        protected virtual bool CheckSalesTime(ref object[] vntParameterList)
        {
            try
            {
                bool CheckSalesTime = false;
                Recordset rstSecondary = null;
                object LotID = DBNull.Value;

                LotID = vntParameterList[0];
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                rstSecondary = objLib.GetRecordset(modProduct.strqACTIVE_SALES_ITEMS, 1, LotID, modProduct.strfLOT_ID);

                if (rstSecondary.RecordCount > 0)
                {
                    vntParameterList[0] = RldtLangDict.GetText(modProduct.strdACTIVE_SALES_ITEMS);
                    CheckSalesTime = true;
                }
                else
                {
                    CheckSalesTime = false;
                }
                return CheckSalesTime;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Checks Built options.
        /// </summary>
        /// <param name="vntParameterList"></param>
        /// <returns></returns>
        /// <history>
        /// Revision #  Date        Author  Description
        /// 3.8.0.0     4/28/2006   JHui    Converted to .Net C# code.
        /// </history>
        protected virtual bool CheckLotConfiguration(ref object[] vntParameterList)
        {

            try
            {
                bool CheckLotConfiguration = false;
                Recordset rstSecondary = null;
                object LotID = DBNull.Value;

                LotID = vntParameterList[0];
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                rstSecondary = objLib.GetRecordset(modProduct.strqLOT_CONFIGURATIONS, 1, LotID, modProduct.strfPRODUCT_ID);

                if (rstSecondary.RecordCount > 0)
                {
                    vntParameterList[0] = RldtLangDict.GetText(modProduct.strdEXISTING_BUILT_OPTIONS);
                    CheckLotConfiguration = true;
                }
                else
                {
                    CheckLotConfiguration = false;
                }
                return CheckLotConfiguration;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Checks homesite references
        /// a.   No active Customer or Inventory Quotes exist for the Home Site
        /// b.   No Sales (Sales Request, Contract or Closed Contract) exist for the Homesite
        /// c.   No built options (Lot Configuration records) exist for the Homesite
        /// </summary>
        /// <param name="strMessage">reason if verfication fails</param>
        /// <param name="vntLot_Id">Lot ID</param>
        /// <returns></returns>
        /// <history>
        /// Revision #  Date        Author  Description
        /// 3.8.0.0     4/28/2006   JHui    Converted to .Net C# code.
        /// </history>
        protected virtual void VerifyRules(object vntLot_Id, ref object strMessage)
        {


            try
            {
                Recordset rstRecordset = null;
                strMessage = string.Empty;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                rstRecordset = objLib.GetRecordset(modProduct.strqHOMESITE_REFERENCED_FOR_LOT, 1, vntLot_Id, modProduct.strfOPPORTUNITY_ID);

                if (rstRecordset.RecordCount > 0)
                {
                    strMessage = RldtLangDict.GetText(modProduct.strdHOMESITE_REFERENCED_ALERT);
                    return;
                }

                rstRecordset = objLib.GetRecordset(modProduct.strqLOT_CONFIGURATIONS, 1, vntLot_Id, modProduct.strfPRODUCT_ID);

                if (rstRecordset.RecordCount > 0)
                {
                    strMessage = RldtLangDict.GetText(modProduct.strdHOMESITE_HAS_BUILT_OPTIONS);
                    return;
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
    }
}
