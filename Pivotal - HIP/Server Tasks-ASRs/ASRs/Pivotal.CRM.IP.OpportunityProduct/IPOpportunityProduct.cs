using System;
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
    /// ASR class for the Opportunity Product forms.
    /// </summary>
    public class OpportunityProduct : IRFormScript
    {
        private IRSystem7 mrsysSystem = null;

        /// <summary>
        /// </summary>
        protected IRSystem7 RSysSystem
        {
            get { return mrsysSystem; }
            set { mrsysSystem = value; }
        }

        private ILangDict grldtLangDict;

        /// <summary>
        /// </summary>
        protected ILangDict RldtLangDict
        {
            get { return grldtLangDict; }
            set { grldtLangDict = value; }
        }

        /// <summary>
        /// Add Form Data; sets defaults and calculates totals on the quote linked to this Opp Product.
        /// </summary>
        /// <param name="pForm">The IRForm object reference to the client IRForm object</param>
        /// <param name="Recordsets">Hold the reference for the current primary recordset and its all
        /// secondaries in the specified form</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>New record ID</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/16/2006   JHui    Converted to .Net C# code.
        /// </history>
        public virtual object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {

            try
            {
                Recordset rstOppProduct = null;
                object vntEmployeeId = DBNull.Value;
                object vntNBHDP_ProductId = DBNull.Value;
                object vntChangeOrderId = DBNull.Value;
                object vntParam = null;
                object vntOppId = DBNull.Value;
                object recordId = DBNull.Value;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                object[] arrRecs = (object[])Recordsets;
                rstOppProduct = (Recordset)arrRecs[0];
                // default the Option Added by for Custom Options to CurrentUserId
                vntNBHDP_ProductId = rstOppProduct.Fields[modOpportunityProduct.strfNBHDP_PRODUCT_ID].Value;
                if (!(vntNBHDP_ProductId is Array))
                {
                    Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                    vntEmployeeId = administration.CurrentUserRecordId;
                    rstOppProduct.Fields[modOpportunityProduct.strfOPTION_ADDED_BY].Value = vntEmployeeId;
                }


                recordId = pForm.DoAddFormData(Recordsets, ref ParameterList);

                // If it is custom option save Opportunity__Product_Id to Product_Number field
                if (!(rstOppProduct.Fields[modOpportunityProduct.strfDIVISION_PRODUCT_ID].Value is Array))
                {
                    Recordset rstOppProduct2 = objLib.GetRecordset(recordId, modOpportunityProduct.strtOPPORTUNITY__PRODUCT,
                    modOpportunityProduct.strfPRODUCT_NUMBER, modOpportunityProduct.strfOPTION_SELECTION_SOURCE);
                    string idString = RSysSystem.IdToString(recordId);
                    rstOppProduct2.Fields[modOpportunityProduct.strfPRODUCT_NUMBER].Value = string.Format("{0:X}", Convert.ToInt16(idString, 16));
                    rstOppProduct2.Fields[modOpportunityProduct.strfOPTION_SELECTION_SOURCE].Value = 0; //selected from Pivotal
                    objLib.SaveRecordset(modOpportunityProduct.strtOPPORTUNITY__PRODUCT, rstOppProduct2);
                }

                // added by Carl Langan 01/04/05 for integration
                IRAppScript objIntegration = (IRAppScript)RSysSystem.ServerScripts[modOpportunityProduct.strsINTEGRATION].CreateInstance();
                objIntegration.Execute(modOpportunityProduct.strmIS_INTEGRATION_ON, ref vntParam);
                if (vntParam is Array)
                {
                    object[] arrParameters = (object[])vntParam;
                    if (arrParameters.GetUpperBound(0) >= 6)
                    {
                        if (TypeConvert.ToBoolean(arrParameters[6]))
                        {
                            vntParam = new object[] { rstOppProduct.Fields[modOpportunityProduct.strfOPPORTUNITY_ID].Value };
                            objIntegration.Execute(modOpportunityProduct.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref vntParam);
                        }
                    }
                }

                // update change order information
                vntChangeOrderId = rstOppProduct.Fields[modOpportunityProduct.strfADDED_BY_CHANGE_ORDER_ID].Value;
                if (!Convert.IsDBNull(vntChangeOrderId))
                {
                    UpdateChangeOrder(vntChangeOrderId, recordId, 0);
                }

                // updated by Carl Langan - update the totals on the opportunity
                vntOppId = rstOppProduct.Fields[modOpportunityProduct.strfOPPORTUNITY_ID].Value;
                IRFormScript objOpportunity = (IRFormScript)RSysSystem.ServerScripts[modOpportunityProduct.strsOPPORTUNITY].CreateInstance();
                TransitionPointParameter transitParamLib = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                object executeParameters = transitParamLib.Construct();
                object executeUserParameters = new object[] { vntOppId, false };
                executeParameters = transitParamLib.SetUserDefinedParameterArray(executeUserParameters);
                objOpportunity.Execute(pForm, modOpportunityProduct.strmCALCULATE_TOTALS, ref executeParameters);

                //BC - Update the quantity
                UpdatePackageComponentOppProductQuantity(recordId);
                return recordId;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vntOpportunityProductId">Opportunity Product Location Id</param>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 5.9.0.0       3/07/2007   BC      Updates the Child Opp Product Quantity
        /// 5.9.0.0       07/03/2007  BC      Update Package Component Price
        /// </history>
        protected virtual void UpdatePackageComponentOppProductQuantity(object vntOpportunityProductId)
        {
            try
            {
                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                string strType = TypeConvert.ToString(dataAccess.SqlIndex(modOpportunityProduct.strtOPPORTUNITY__PRODUCT, modOpportunityProduct.strfTYPE,
                    vntOpportunityProductId));
                if (strType == modOpportunityProduct.strsPACKAGE)
                {
                    double dblQuantity = TypeConvert.ToDouble(dataAccess.SqlIndex(modOpportunityProduct.strtOPPORTUNITY__PRODUCT, modOpportunityProduct.strfQUANTITY,
                                    vntOpportunityProductId));
                    Recordset rstOppProductPackageComponent = dataAccess.GetRecordset(modOpportunityProduct.strqOPP_PRODUCT_FOR_PACKAGE, 1,
                                                       vntOpportunityProductId, modOpportunityProduct.strfOPPORTUNITY__PRODUCT_ID,
                                                       modOpportunityProduct.strfQUANTITY);
                    if (rstOppProductPackageComponent.RecordCount > 0)
                    {
                        rstOppProductPackageComponent.MoveFirst();
                        while (!rstOppProductPackageComponent.EOF)
                        {
                            rstOppProductPackageComponent.Fields[modOpportunityProduct.strfQUANTITY].Value = dblQuantity;
                            rstOppProductPackageComponent.MoveNext();
                        }
                        dataAccess.SaveRecordset(modOpportunityProduct.strtOPPORTUNITY__PRODUCT, rstOppProductPackageComponent);
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Delete Form Data ; Cascade deleletes locations.
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="RecordId">The business object record Id</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/16/2006   JHui    Converted to .Net C# code.
        /// 5.9           5/21/2007   JH      This function is deprecated for 5.9.  Deletion is not allowed.
        /// 5.9           5/28/2007   JWang   We still need this function when ConvertToSale for a quote.
        /// </history>
        public virtual void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {

            try
            {
                object vntParam = null;
                object vntOppId = DBNull.Value;
                Recordset rstOppProduct = null;
                Recordset rstLocations = null;
                object vntLocationId = DBNull.Value;
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                rstOppProduct = objLib.GetRecordset(RecordId, modOpportunityProduct.strtOPPORTUNITY__PRODUCT, modOpportunityProduct.strfOPPORTUNITY_ID);
                if (!rstOppProduct.EOF && !rstOppProduct.BOF)
                {
                    vntOppId = rstOppProduct.Fields[modOpportunityProduct.strfOPPORTUNITY_ID].Value;
                }

                rstLocations = objLib.GetRecordset(modOpportunityProduct.strqOPP_LOCATIONS_FOR_OPP, 1, RecordId, modOpportunityProduct.strfLOCATION_ID);
                if (rstLocations.RecordCount > 0)
                {
                    rstLocations.MoveFirst();
                    while (!rstLocations.EOF)
                    {
                        vntLocationId = rstLocations.Fields[modOpportunityProduct.strfLOCATION_ID].Value;
                        object deleteParameters = null;
                        RSysSystem.Forms[modOpportunityProduct.strrLOCATION].DeleteFormData(vntLocationId, ref deleteParameters);
                        rstLocations.MoveNext();
                    }
                }

                pForm.DoDeleteFormData(RecordId, ref ParameterList);

                // added by Carl Langan 01/04/05 for integration
                IRAppScript objIntegration = (IRAppScript)RSysSystem.ServerScripts[modOpportunityProduct.strsINTEGRATION].CreateInstance();
                objIntegration.Execute(modOpportunityProduct.strmIS_INTEGRATION_ON, ref vntParam);
                if ((vntParam is Array) && (vntOppId is Array))
                {
                    object[] arrParameters = (object[])vntParam;
                    if (arrParameters.GetUpperBound(0) >= 6)
                    {
                        if (TypeConvert.ToBoolean(arrParameters[6]))
                        {
                            vntParam = new object[] { vntOppId };
                            objIntegration.Execute(modOpportunityProduct.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref vntParam);
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
        /// Execute a specified method.
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="MethodName">The method name to be executed</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/16/2006   JHui    Converted to .Net C# code.
        /// </history>
        public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {


            try
            {
                Recordset rstRecordset = null;
                string strArgument = String.Empty;

                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;

                // Dump out the user defined parameters
                object[] parameterArray = objParam.GetUserDefinedParameterArray();

                switch (MethodName)
                {
                    case modOpportunityProduct.strmGET_PLAN_LOCATION_FOR_PRODUCT:
                        rstRecordset = GetPlanLocationsForProduct(parameterArray[0]);
                        parameterArray = new object[] { rstRecordset };
                        break;

                    case modOpportunityProduct.strmDELETE_ATTR_PREF:
                        this.DeleteProductAttributePreference(parameterArray[0]);
                        break;

                    default:
                        throw new PivotalApplicationException(MethodName + TypeConvert.ToString(RldtLangDict.GetText(modOpportunityProduct.strdINVALID_METHOD)),
                            modOpportunityProduct.glngERR_METHOD_NOT_DEFINED, RSysSystem);
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
        /// Delete Opportunity Opportunity Location, Product Pref and 
        /// </summary>
        /// <param name="opportunityProductLocationId">Opp Product Location Id</param>
        /// <returns></returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 5.9.0.0        3/6/2007      BC         Initial Version
        /// 5.9.0.0        8/1/2007      BC         Update the Quote Price on delete of Opp Location
        /// </history>
        protected virtual void DeleteProductAttributePreference(object opportunityProductLocationId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOppProductSubComponents = objLib.GetRecordset(modOpportunityProduct.strqOPP_PROD_LOCATION_FOR_PACKAGE, 1,
                                                       opportunityProductLocationId, modOpportunityProduct.strfOPP_PRODUCT_LOCATION_ID);
                object vntOpportunityId = objLib.SqlIndex(modOpportunityProduct.strtOPP_PRODUCT_LOCATION,
                                                          modOpportunityProduct.strfOPPORTUNITY_ID,
                                                          opportunityProductLocationId);

                if (rstOppProductSubComponents.RecordCount > 0)
                {
                    rstOppProductSubComponents.MoveFirst();
                    while (!rstOppProductSubComponents.EOF)
                    {
                        object vntComponentOppProdLocId = rstOppProductSubComponents.Fields[modOpportunityProduct.strfOPP_PRODUCT_LOCATION_ID].Value;
                        DeleteProductAttributePreference(vntComponentOppProdLocId);
                        rstOppProductSubComponents.MoveNext();
                    }
                }
                //Delete from Opportunity Location Product Pref
                Recordset rstProdLocAttrPref = objLib.GetRecordset(modOpportunityProduct.strqOPP_PROD_ATTR_PREF_LOC, 1,
                                                                opportunityProductLocationId,
                                                                modOpportunityProduct.strfOP_LOC_ATTR_PREF_ID);

                if (rstProdLocAttrPref.RecordCount > 0)
                {
                    rstProdLocAttrPref.MoveFirst();
                    while (!rstProdLocAttrPref.EOF)
                    {
                        objLib.DeleteRecordset(modOpportunityProduct.strqOPP_PREF_FOR_ATTRIBUTE,
                                            modOpportunityProduct.strfOP_PREF_ID,
                                            rstProdLocAttrPref.Fields[modOpportunityProduct.strfOP_LOC_ATTR_PREF_ID].Value);
                        rstProdLocAttrPref.MoveNext();
                    }
                }
                //Delete Location Attribute Pref
                objLib.DeleteRecordset(modOpportunityProduct.strqOPP_PROD_ATTR_PREF_LOC,
                                    modOpportunityProduct.strfOP_LOC_ATTR_PREF_ID,
                                    opportunityProductLocationId);
                //Delete The OppLocation Record
                // NS -- (Start 11 May 2007 ) Set the Quantity field in Opportunity__Product correctly <Issue 65536-19115>
                Recordset rstOppProdLoc = objLib.GetRecordset(opportunityProductLocationId, modOpportunityProduct.strtOPP_PRODUCT_LOCATION, modOpportunityProduct.strfLOCATION_QUANTITY, modOpportunityProduct.strfOPP_PRODUCT_ID);
                if (rstOppProdLoc.RecordCount > 0)
                {
                    object vntOppProductId = DBNull.Value;
                    int intOppProdLocationQuantity = 0;
                    vntOppProductId = rstOppProdLoc.Fields[modOpportunityProduct.strfOPP_PRODUCT_ID].Value;
                    intOppProdLocationQuantity = TypeConvert.ToInt32(rstOppProdLoc.Fields[modOpportunityProduct.strfLOCATION_QUANTITY].Value);
                    Recordset rstOppProduct = objLib.GetRecordset(vntOppProductId, modOpportunityProduct.strtOPPORTUNITY__PRODUCT, modOpportunityProduct.strfQUANTITY);
                    if (rstOppProduct.RecordCount > 0)
                    {
                        rstOppProduct.Fields[modOpportunityProduct.strfQUANTITY].Value =
                            TypeConvert.ToInt32(rstOppProduct.Fields[modOpportunityProduct.strfQUANTITY].Value) - intOppProdLocationQuantity;

                        objLib.SaveRecordset(modOpportunityProduct.strtOPPORTUNITY__PRODUCT, rstOppProduct);
                    }
                }
                // NS -- (End 11 May 2007 )
                objLib.DeleteRecord(opportunityProductLocationId, modOpportunityProduct.strtOPP_PRODUCT_LOCATION);

                //Recalculate the Opportunity RecordSet
                if (!Convert.IsDBNull(vntOpportunityId))
                {
                    Opportunity objOpportunity = (Opportunity)RSysSystem.ServerScripts[modOpportunityProduct.strsOPPORTUNITY].CreateInstance();
                    
                    objOpportunity.CalculateTotals(vntOpportunityId);
                }

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Load Form Data
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object.</param>
        /// <param name="RecordId">Record Id</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule.</param>
        /// <returns>The form data</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/16/2006   JHui    Converted to .Net C# code.
        /// </history>
        public virtual object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {


            try
            {
                object Recordsets = null;
                Recordset rstRecordset = null;

                Recordsets = pForm.DoLoadFormData(RecordId, ref ParameterList);
                object[] recordsetArray = (object[])Recordsets;
                rstRecordset = (Recordset)recordsetArray[0];

                if (!Convert.IsDBNull(ParameterList))
                {
                    object[] arrParameters = (object[])ParameterList;
                    if (arrParameters.GetUpperBound(0) > 6)
                    {
                        if (TypeConvert.ToString(arrParameters[6]) == modOpportunityProduct.strsCHANGE_ORDER && (arrParameters[7]
                            is Array))
                        {
                            rstRecordset.Fields[modOpportunityProduct.strfADDED_BY_CHANGE_ORDER_ID].Value = arrParameters[7];
                        }
                    }
                }

                return Recordsets;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function creates a new Opportunity Product record
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>Array of empty recordsets</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/16/2006   JHui    Converted to .Net C# code.
        /// </history>
        public virtual object NewFormData(IRForm pForm, ref object ParameterList)
        {
            try
            {
                object vntOppProduct = null;
                Recordset rstOppProduct = null;

                vntOppProduct = pForm.DoNewFormData(ref ParameterList);
                object[] recordsetArray = (object[])vntOppProduct;
                rstOppProduct = (Recordset)recordsetArray[0];

                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;

                if (!ocmsTransitPointParams.HasValidParameters())
                {
                    ocmsTransitPointParams.Construct();
                }
                else
                {
                    ocmsTransitPointParams.SetDefaultFields(rstOppProduct);
                }

                return vntOppProduct;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function creates a new secondary record for the specified secondary.
        /// </summary>
        /// <param name="pForm">The IRForm object reference to the client IRForm object</param>
        /// <param name="SecondaryName">The secondary name (the Segment name to hold a secondary)</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <param name="recordset">Hold the reference for the secondary</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/16/2006   JHui    Converted to .Net C# code.
        /// </history>
        public virtual void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset
            recordset)
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
        /// This function saves the Opportunity product record.
        /// </summary>
        /// <param name="pForm">The IRForm object reference to the client IRForm object</param>
        /// <param name="Recordsets">Hold the reference for the current primary recordset and its all
        /// secondaries in the specified form</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/16/2006   JHui    Converted to .Net C# code.
        /// 5.9.0.0       07/03/2007  BC      Update Package Component Price
        /// </history>
        public virtual void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                Recordset rstOppProduct = null;
                object vntOppId = DBNull.Value;
                object vntOppProductId = DBNull.Value;
                object vntChangeOrderId = DBNull.Value;
                object vntParam = null;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Opportunity objOpp = (Opportunity)RSysSystem.ServerScripts[modOpportunityProduct.strsOPPORTUNITY].CreateInstance();

                object[] arrRecs = (object[])Recordsets;
                rstOppProduct = (Recordset)arrRecs[0];

                vntOppId = rstOppProduct.Fields[modOpportunityProduct.strfOPPORTUNITY_ID].Value;
                vntOppProductId = rstOppProduct.Fields[modOpportunityProduct.strfOPPORTUNITY__PRODUCT_ID].Value;

                // quantity change check
                if (TypeConvert.ToInt32(rstOppProduct.Fields[modOpportunityProduct.strfQUANTITY].Value) !=
                    TypeConvert.ToInt32(objLib.SqlIndex(modOpportunityProduct.strtOPPORTUNITY__PRODUCT, modOpportunityProduct.strfQUANTITY, vntOppProductId)))
                {
                    // must be change - if inventory quote then inactivate all customer records
                    objOpp.InactivateCustomerQuotes(vntOppProductId, null);
                }

                pForm.DoSaveFormData(Recordsets, ref ParameterList);

                // updated by Carl Langan for delegation purposes.
                objOpp.CalculateTotals(vntOppId);

                // update change order information
                vntChangeOrderId = rstOppProduct.Fields[modOpportunityProduct.strfADDED_BY_CHANGE_ORDER_ID].Value;
                if (!Convert.IsDBNull(vntChangeOrderId))
                {
                    UpdateChangeOrder(vntChangeOrderId, vntOppProductId, 2);
                }

                // added by Carl Langan 01/04/05 for integration
                IRAppScript objIntegration = (IRAppScript)RSysSystem.ServerScripts[modOpportunityProduct.strsINTEGRATION].CreateInstance();
                objIntegration.Execute(modOpportunityProduct.strmIS_INTEGRATION_ON, ref vntParam);
                if (vntParam is Array)
                {
                    object[] arrParameters = (object[])vntParam;
                    if (arrParameters.GetUpperBound(0) >= 6)
                    {
                        if (TypeConvert.ToBoolean(arrParameters[6]))
                        {
                            vntParam = new object[] { rstOppProduct.Fields[modOpportunityProduct.strfOPPORTUNITY_ID].Value };
                            objIntegration.Execute(modOpportunityProduct.strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE, ref vntParam);
                        }
                    }
                }
                //BC - Update the quantity
                UpdatePackageComponentOppProductQuantity(vntOppProductId);

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Public method to get current IRSystem7 reference
        /// </summary>
        /// <param name="pSystem">Hold the current System Instance Reference</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/16/2006   JHui    Converted to .Net C# code.
        /// </history>
        public virtual void SetSystem(RSystem pSystem)
        {
            try
            {
                RSysSystem = (IRSystem7)pSystem;
                RldtLangDict = RSysSystem.GetLDGroup(modOpportunityProduct.strgOPPORTUNITY_PRODUCT);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);

            }
        }

        /// <summary>
        /// This function updates the change order.
        /// </summary>
        /// <param name="intSelected"></param>
        /// <param name="vntChangeOrderId"></param>
        /// <param name="vntOppProductId"></param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/16/2006   JHui    Converted to .Net C# code.
        /// </history>
        protected virtual bool UpdateChangeOrder(object vntChangeOrderId, object vntOppProductId, int intSelected)
        {


            try
            {

                bool updateChangeOrder = false;
                Recordset rstChangeOrder = null;
                Recordset rstOppProduct = null;

                updateChangeOrder = false;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                rstOppProduct = objLib.GetRecordset(vntOppProductId, modOpportunityProduct.strtOPPORTUNITY__PRODUCT,
                    modOpportunityProduct.strfBUILT_OPTION, modOpportunityProduct.strfCODE_, modOpportunityProduct.strfCONSTRUCTION_STAGE_ID,
                    modOpportunityProduct.strfCONSTRUCTION_STAGE_ORDINAL, modOpportunityProduct.strfCUSTOMERINSTRUCTIONS,
                    modOpportunityProduct.strfDELTA_BUILT_OPTION, modOpportunityProduct.strfDEPOSIT, modOpportunityProduct.strfDIVISION_PRODUCT_ID,
                    modOpportunityProduct.strfEXTENDED_PRICE, modOpportunityProduct.strfFILTER_VISIBILITY, modOpportunityProduct.strfNBHDP_PRODUCT_ID,
                    modOpportunityProduct.strfNET_CONFIG, modOpportunityProduct.strfOPP_CURRENCY, modOpportunityProduct.strfOPPORTUNITY_ID,
                    modOpportunityProduct.strfOPPORTUNITY__PRODUCT_ID, modOpportunityProduct.strfOPPORTUNITY_PRODUCT_PREF_ID,
                    modOpportunityProduct.strfOPTIONNOTES, modOpportunityProduct.strfPREFERENCE, modOpportunityProduct.strfPREFERENCES_LIST,
                    modOpportunityProduct.strfPRICE, modOpportunityProduct.strfPRICE_CHANGED, modOpportunityProduct.strfPRODUCT_AVAILABLE,
                    modOpportunityProduct.strfPRODUCT_NAME, modOpportunityProduct.strfQUANTITY, modOpportunityProduct.strfQUOTED_PRICE,
                    modOpportunityProduct.strfSELECTED, modOpportunityProduct.strfTICKLE_COUNTER, modOpportunityProduct.strfTYPE,
                    modOpportunityProduct.strfADDED_BY_CHANGE_ORDER_ID);

                rstChangeOrder = objLib.GetNewRecordset(modOpportunityProduct.strtCHANGE_ORDER_OPTIONS, modOpportunityProduct.strfBUILT_OPTION,
                    modOpportunityProduct.strfCHANGE_ORDER_ID, modOpportunityProduct.strfCHANGE_ORDER_OPTIONS_ID, modOpportunityProduct.strfCHANGE_ORDER_STATUS,
                    modOpportunityProduct.strfCODE_, modOpportunityProduct.strfCONSTRUCTION_STAGE_ID, modOpportunityProduct.strfCONSTRUCTION_STAGE_ORDINAL,
                    modOpportunityProduct.strfCUSTOMERINSTRUCTIONS, modOpportunityProduct.strfDELTA_BUILT_OPTION, modOpportunityProduct.strfDEPOSIT,
                    modOpportunityProduct.strfDIVISION_PRODUCT_ID, modOpportunityProduct.strfEXTENDED_PRICE, modOpportunityProduct.strfFILTER_VISIBILITY,
                    modOpportunityProduct.strfNBHDP_PRODUCT_ID, modOpportunityProduct.strfNET_CONFIG, modOpportunityProduct.strfOPP_CURRENCY,
                    modOpportunityProduct.strfOPPORTUNITY_ID, modOpportunityProduct.strfOPPORTUNITY_PRODUCT_ID, modOpportunityProduct.strfOPPORTUNITY_PRODUCT_PREF_ID,
                    modOpportunityProduct.strfOPTIONNOTES, modOpportunityProduct.strfPREFERENCE, modOpportunityProduct.strfPREFERENCES_LIST,
                    modOpportunityProduct.strfPRICE, modOpportunityProduct.strfPRICE_CHANGED, modOpportunityProduct.strfPRODUCT_AVAILABLE,
                    modOpportunityProduct.strfPRODUCT_NAME, modOpportunityProduct.strfQUANTITY, modOpportunityProduct.strfQUOTED_PRICE,
                    modOpportunityProduct.strfSELECTED, modOpportunityProduct.strfTICKLE_COUNTER, modOpportunityProduct.strfTYPE);

                rstChangeOrder.AddNew(modOpportunityProduct.strfCHANGE_ORDER_ID, DBNull.Value);
                foreach (Field objField in rstOppProduct.Fields)
                {
                    if (objField.Name == modOpportunityProduct.strfOPPORTUNITY__PRODUCT_ID)
                    {
                        rstChangeOrder.Fields[modOpportunityProduct.strfOPPORTUNITY_PRODUCT_ID].Value = objField.Value;
                    }
                    else if (objField.Name == modOpportunityProduct.strfADDED_BY_CHANGE_ORDER_ID)
                    {
                        // do nothing
                    }
                    else
                    {
                        rstChangeOrder.Fields[objField.Name].Value = rstOppProduct.Fields[objField.Name].Value;
                    }
                }
                rstChangeOrder.Fields[modOpportunityProduct.strfCHANGE_ORDER_ID].Value = vntChangeOrderId;
                rstChangeOrder.Fields[modOpportunityProduct.strfCHANGE_ORDER_STATUS].Value = intSelected;

                objLib.SaveRecordset(modOpportunityProduct.strtCHANGE_ORDER_OPTIONS, rstChangeOrder);

                // now update the opp product change order id
                rstOppProduct.Fields[modOpportunityProduct.strfADDED_BY_CHANGE_ORDER_ID].Value = DBNull.Value;
                objLib.SaveRecordset(modOpportunityProduct.strtOPPORTUNITY__PRODUCT, rstOppProduct);

                updateChangeOrder = true;
                return updateChangeOrder;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Filter for Location query on the Option secondary.
        /// </summary>
        /// <param name="vntNeighborhoodProductId">NHBD Product ID</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#      Date          Author     Description
        /// 3.8.0.0        5/16/2006     JHui       Converted to .Net C# code.
        /// </history>
        protected virtual Recordset GetPlanLocationsForProduct(object vntNeighborhoodProductId)
        {
            try
            {
                Recordset rstPlanLocations = null;
                object vntNBHDPlanId = DBNull.Value;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                if (vntNeighborhoodProductId is Array)
                {
                    vntNBHDPlanId = RSysSystem.Tables[modOpportunityProduct.strtNBHDP_PRODUCT].Fields[modOpportunityProduct.strfPLAN_ID].Index(vntNeighborhoodProductId);
                    if (vntNBHDPlanId is Array)
                    {
                        rstPlanLocations = objLib.GetRecordset(modOpportunityProduct.strqSET_LOCATION_DIV_PRODUCT, 1,
                            vntNBHDPlanId, modOpportunityProduct.strfLOC_LOCATION_ID);
                    }
                    return rstPlanLocations;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

    }

}
