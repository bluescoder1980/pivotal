using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Pivotal.Interop.RDALib;
using Pivotal.Interop.ADODBLib;
using Pivotal.Application.Foundation.Utility;
using Pivotal.Application.Foundation.Data.Element;
using System.Diagnostics;

namespace CRM.Pivotal.IAC
{
    public class Contact : IRFormScript
    {
        #region Class-Level Variables
        private IRSystem7 mrsysSystem = null;

        public IRSystem7 RSysSystem
        {
            get { return mrsysSystem; }
            set { mrsysSystem = value; }
        }

        private ILangDict grldtLangDict = null;

        protected ILangDict RldtLangDict
        {
            get { return grldtLangDict; }
            set { grldtLangDict = value; }
        }
        #endregion

        public virtual void SetSystem(RSystem pSystem)
        {
            try
            {
                RSysSystem = (IRSystem7)pSystem;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        public virtual object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                object[] recordsetArray = (object[])Recordsets;
                Recordset rstPrimary = (Recordset)recordsetArray[0];

                object vntRecordId = pForm.DoAddFormData(Recordsets, ref ParameterList);

                AddContactTraffic(vntRecordId, rstPrimary);
                return vntRecordId;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        public virtual void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                pForm.DoDeleteFormData(RecordId, ref ParameterList);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {
            try
            {
                Recordset rstRecordset = null;

                TransitionPointParameter objrInstance = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objrInstance.ParameterList = ParameterList;

                if (objrInstance.HasValidParameters() == false)
                {
                    objrInstance.Construct();
                }

                object[] parameterArray = objrInstance.GetUserDefinedParameterArray();

                switch (MethodName)
                {
                    case modContact.strmCHECK_FOR_DUPE:
                        objrInstance.CheckUserDefinedParameterNumber(5);
                        rstRecordset = this.ContactDuplicateCheck(parameterArray[0], parameterArray[1], parameterArray[2], parameterArray[3], parameterArray[4]);
                        parameterArray = new object[] { rstRecordset, RSysSystem.Tables[modContact.strtCONTACT].TableId };
                        break;
                    case modContact.strmCHECK_FOR_DUPE_FIELDCHANGE:
                        objrInstance.CheckUserDefinedParameterNumber(5);
                        rstRecordset = this.ContactDuplicateCheckField(parameterArray[0], parameterArray[1], parameterArray[2], parameterArray[3], parameterArray[4]);
                        parameterArray = new object[] { rstRecordset, RSysSystem.Tables[modContact.strtCONTACT].TableId };
                        break;
                    case modContact.strmCONTACT_SEARCH:
                        objrInstance.CheckUserDefinedParameterNumber(3);
                        parameterArray = new object[] { this.ContactSearch(parameterArray[0], (Recordset)parameterArray[1], (Boolean)parameterArray[2]) };
                        break;
                    case modContact.strmZIP_LOOKUP:
                        objrInstance.CheckUserDefinedParameterNumber(1);
                        rstRecordset = this.LookupZip((string)parameterArray[0]);
                        parameterArray = new object[] { rstRecordset };
                        break;
                    case modContact.strmSAVE_RECOMMENDATIONS:
                        objrInstance.CheckUserDefinedParameterNumber(1);
                        this.SaveRecommendations(parameterArray[0]);
                        break;
                    default:
                        string message = MethodName + TypeConvert.ToString(RldtLangDict.GetText(modContact.strdINVALID_METHOD));
                        parameterArray = new object[] { message };
                        throw new PivotalApplicationException(message, modContact.glngERR_METHOD_NOT_DEFINED);
                }

                ParameterList = objrInstance.SetUserDefinedParameterArray(parameterArray);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        public virtual object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                object vntRecordset = pForm.DoLoadFormData(RecordId, ref ParameterList);
                object[] recordsetArray = (object[])vntRecordset;

                // checking and seting of the system parameters
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;

                if (!(objParam.HasValidParameters()))
                {
                    objParam.Construct();
                }

                if (pForm.FormName == modContact.strpRLIC_CONTACT)
                {
                    PopulateMissingSearchItems(ref recordsetArray, pForm, RecordId);
                    int paramNumber = 0;
                    for (int paramitem = 1; paramitem <= objParam.UserDefinedParametersNumber; paramitem++)
                    {
                        try
                        {
                            object[] Param = (object[])objParam.GetUserDefinedParameter(paramitem);
                            if (Param.Length == 2)
                            {
                                if ((string)Param[0] == modContact.strmFOUND_CONTACTPROPERTY_MPEB)
                                {
                                    paramNumber = paramitem;
                                    break;
                                }
                            }
                        }
                        catch
                        { //Ignore Exception
                        }
                    }
                    if (paramNumber == 0) paramNumber = objParam.UserDefinedParametersNumber + 1;
                    objParam.SetUserDefinedParameter(paramNumber, new object[] { modContact.strmFOUND_CONTACTPROPERTY_MPEB, FoundContactPropertyMPEB(RecordId) });
                    ParameterList = objParam.Construct();
                }

                return vntRecordset;
            }
            catch (Exception exc)
            {
                //throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
                throw new PivotalApplicationException(exc.Message, true);
            }
        }

        public virtual object NewFormData(IRForm pForm, ref object ParameterList)
        {
            try
            {
                object vntRecordset = pForm.DoNewFormData(ref ParameterList);
                object[] recordsetArray = (object[])vntRecordset;
                Recordset rstRecordset = (Recordset)recordsetArray[0];

                TransitionPointParameter objrParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objrParam.ParameterList = ParameterList;

                if (objrParam.HasValidParameters() == false)
                {
                    objrParam.Construct();
                }
                else
                {
                    objrParam.SetDefaultFields(rstRecordset);
                }

                if (pForm.FormName == modContact.strpRLIC_CONTACT)
                {
                    PopulateMissingSearchItems(ref recordsetArray, pForm, null);
                    GetDefaultRegions(ref recordsetArray, pForm);
                }

                return vntRecordset;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        public virtual void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset Recordset)
        {
            try
            {
                pForm.DoNewSecondaryData(SecondaryName, ref ParameterList, Recordset);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        public virtual void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                TransitionPointParameter objrParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objrParam.ParameterList = ParameterList;

                object[] recordsetArray = (object[])Recordsets;
                Recordset rstPrimary = (Recordset)recordsetArray[0];

                object vntRecordId = (object)rstPrimary.Fields[modContact.strfCONTACT_ID].Value;
                //Save the data
                pForm.DoSaveFormData(Recordsets, ref ParameterList);

                AddContactTraffic(vntRecordId, rstPrimary);

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        protected virtual Recordset ContactDuplicateCheck(object vntFirstName, object vntLastName, object vntEmail, object vntPhone, object vntAddress)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                Recordset rstDuplicate = objLib.GetRecordset(modContact.strqIAC_CONTACT_DUPESEARCH, 14, vntFirstName, vntFirstName, vntFirstName, vntFirstName, vntLastName, vntLastName, vntEmail, vntEmail, vntPhone, vntPhone, vntPhone, vntPhone, vntAddress, vntAddress,
                                                             modContact.strfCONTACT_ID);

                return rstDuplicate;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        protected virtual Recordset ContactDuplicateCheckField(object vntContactId, object vntEmail, object vntPhone, object vntAddress, object vntCell)
        {

            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                Recordset rstDuplicate = objLib.GetRecordset(modContact.strqIAC_CONTACT_DUPESEARCH_FIELDCHANGE, 13, vntContactId, vntEmail, vntEmail, vntPhone, vntPhone, vntCell, vntCell, vntPhone, vntPhone, vntCell, vntCell, vntAddress, vntAddress,
                                                             modContact.strfCONTACT_ID);
                return rstDuplicate;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        protected virtual void PopulateMissingSearchItems(ref object[] recordsetArray, IRForm pForm, object RecordId)
        {
            GetMissingSearchItems(ref recordsetArray, pForm, RecordId, modContact.strsSI_BEDROOMS);
            GetMissingSearchItems(ref recordsetArray, pForm, RecordId, modContact.strsSI_LOCATION);
            GetMissingSearchItems(ref recordsetArray, pForm, RecordId, modContact.strsSI_FLOOR);
            GetMissingSearchItems(ref recordsetArray, pForm, RecordId, modContact.strsSI_TYPE);
            GetMissingSearchItems(ref recordsetArray, pForm, RecordId, modContact.strsSI_PARKING);
            GetMissingSearchItems(ref recordsetArray, pForm, RecordId, modContact.strsSI_VIEW);
            GetMissingSearchItems(ref recordsetArray, pForm, RecordId, modContact.strsSI_OTHER);
            GetMissingSearchItems(ref recordsetArray, pForm, RecordId, modContact.strsSI_PROPERTY);
        }

        protected virtual void GetMissingSearchItems(ref object[] recordsetArray, IRForm pForm, object RecordId, string SegmentName)
        {
            DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            objLib.PermissionIgnored = true;

            Recordset rstSegment = pForm.SecondaryFromVariantArray(recordsetArray, SegmentName);
            Recordset rstMissingItems;
            if (RecordId == null) {
                rstMissingItems = objLib.GetRecordset(modContact.strqIAC_SEARCH_GRID_ITEMS, 1, SegmentName.Replace("SI:", ""),
                                                         modContact.strfIAC_SRCH_CATEGORY_ID,
                                                         modContact.strfIAC_SRCH_GRID_ITEM_ID);
            } else {
                rstMissingItems = objLib.GetRecordset(modContact.strqIAC_MISSING_SEARCH_GRID_ITEMS, 3, SegmentName.Replace("SI:", ""), RecordId, SegmentName.Replace("SI:", ""),
                                                         modContact.strfIAC_SRCH_CATEGORY_ID,
                                                         modContact.strfIAC_SRCH_GRID_ITEM_ID);
            }
            if (!rstMissingItems.BOF && !rstMissingItems.EOF)
            {
                rstMissingItems.MoveFirst();
                while (!rstMissingItems.EOF)
                {
                    rstSegment.AddNew(Type.Missing, Type.Missing);
                    rstSegment.Fields[modContact.strfIAC_SRCH_CATEGORY_ID].Value = rstMissingItems.Fields[modContact.strfIAC_SRCH_CATEGORY_ID].Value;
                    rstSegment.Fields[modContact.strfIAC_SRCH_GRID_ITEM_ID].Value = rstMissingItems.Fields[modContact.strfIAC_SRCH_GRID_ITEM_ID].Value;
                    rstSegment.Fields[modContact.strfIAC_INCLUDE].Value = false;
                    rstMissingItems.MoveNext();
                }
                rstSegment.Sort = modContact.strfIAC_FORMULA_SORTORDER;
            }
        }

        protected virtual void GetDefaultRegions(ref object[] recordsetArray, IRForm pForm)
        {
            DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            objLib.PermissionIgnored = true;

            Recordset rstSegment = pForm.SecondaryFromVariantArray(recordsetArray, modContact.strsSEARCH_REGION);
            Recordset rstRegions = objLib.GetRecordset(modContact.strqIAC_OC_REGIONS, 0,
                                                         modContact.strfREGION_ID,
                                                         modContact.strfREGION_NAME);
            if (!rstRegions.BOF && !rstRegions.EOF)
            {
                rstRegions.Sort = modContact.strfREGION_NAME;
                rstRegions.MoveFirst();
                while (!rstRegions.EOF)
                {
                    rstSegment.AddNew(Type.Missing, Type.Missing);
                    rstSegment.Fields[modContact.strfIAC_REGION_ID].Value = rstRegions.Fields[modContact.strfREGION_ID].Value;
                    rstRegions.MoveNext();
                }
            }

        }

        protected virtual void ClearContactSearch(object vntContactId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;
                objLib.DeleteSecondary(modContact.strqIAC_CONTACT_SEARCH_FP, vntContactId);
                objLib.DeleteSecondary(modContact.strqIAC_CONTACT_SEARCH_U, vntContactId);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        protected virtual Boolean ContactSearch(object vntContactId, Recordset rstPrimary, Boolean blnAddTraffic)
        {
            try
            {
                if (blnAddTraffic) AddContactTraffic(vntContactId, rstPrimary);
                ClearContactSearch(vntContactId);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }

            try
            {
                String External_DB = CRM.Pivotal.IAC.Properties.Settings.Default.External_DB;
                String External_Scope = CRM.Pivotal.IAC.Properties.Settings.Default.External_Scope;
                String FP_SPROC = External_DB + "." + External_Scope + "." + modContact.strxIAC_CONTACT_SEARCH_FP;
                String U_SPROC = External_DB + "." + External_Scope + "." + modContact.strxIAC_CONTACT_SEARCH_U;

                SqlConnection oConn = new SqlConnection(RSysSystem.EnterpriseString.Replace("provider=RDSO.RSQL;", ""));

                #region Floorplan Search
                    SqlCommand oCmd = new SqlCommand(FP_SPROC, oConn);
                    SqlDataAdapter oAdapter = new SqlDataAdapter();
                    DataSet oDataset = new DataSet();
                    oCmd.CommandType = CommandType.StoredProcedure;

                    oCmd.Parameters.Add("@ContactId", SqlDbType.Binary, 8);
                    oCmd.Parameters["@ContactId"].Value = vntContactId;
                    oCmd.Parameters["@ContactId"].Direction = ParameterDirection.Input;

                    oConn.Open();
                    oAdapter.SelectCommand = oCmd;
                    oAdapter.Fill(oDataset);

                    if (oDataset.Tables[0].Rows.Count == 0) return false;

                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    objLib.PermissionIgnored = true;
                    Recordset rstFloorplan = objLib.GetNewRecordset(modContact.strtIAC_CONTACT_SEARCH_FP,
                        modContact.strfIAC_CONTACT_ID,
                        modContact.strfIAC_PROPERTY_ID,
                        modContact.strfIAC_PROPERTY_FLOORPLAN_ID,
                        modContact.strfIAC_RECOMMEND,
                        modContact.strfIAC_PROPERTY_NAME,
                        modContact.strfIAC_FLOORPLAN_NAME,
                        modContact.strfIAC_UNITTYPE,
                        modContact.strfIAC_BEDROOMS,
                        modContact.strfIAC_BATHROOMS,
                        modContact.strfIAC_SQUAREFEET_MIN,
                        modContact.strfIAC_SQUAREFEET_MAX,
                        modContact.strfIAC_MARKETRENT_MIN,
                        modContact.strfIAC_MARKETRENT_MAX,
                        modContact.strfIAC_SECURITY_DEPOSIT_MIN,
                        modContact.strfIAC_AVAILABLE_UNITS,
                        modContact.strfIAC_SPECIALS);

                    foreach (DataRow dr in oDataset.Tables[0].Rows)
                    {
                        rstFloorplan.AddNew(Type.Missing, Type.Missing);
                        rstFloorplan.Fields[modContact.strfIAC_CONTACT_ID].Value = vntContactId;
                        rstFloorplan.Fields[modContact.strfIAC_PROPERTY_ID].Value = dr[modContact.strfIAC_PROPERTY_ID];
                        rstFloorplan.Fields[modContact.strfIAC_PROPERTY_FLOORPLAN_ID].Value = dr[modContact.strfIAC_PROPERTY_FLOORPLAN_ID];
                        rstFloorplan.Fields[modContact.strfIAC_RECOMMEND].Value = false;
                        rstFloorplan.Fields[modContact.strfIAC_PROPERTY_NAME].Value = dr[modContact.strfIAC_PROPERTY_NAME];
                        rstFloorplan.Fields[modContact.strfIAC_FLOORPLAN_NAME].Value = dr[modContact.strfIAC_FLOORPLAN_NAME];
                        rstFloorplan.Fields[modContact.strfIAC_UNITTYPE].Value = dr[modContact.strfIAC_UNITTYPE];
                        rstFloorplan.Fields[modContact.strfIAC_BEDROOMS].Value = dr[modContact.strfIAC_BEDROOMS];
                        rstFloorplan.Fields[modContact.strfIAC_BATHROOMS].Value = dr[modContact.strfIAC_BATHROOMS];
                        rstFloorplan.Fields[modContact.strfIAC_SQUAREFEET_MIN].Value = dr[modContact.strfIAC_SQUAREFEET_MIN];
                        rstFloorplan.Fields[modContact.strfIAC_SQUAREFEET_MAX].Value = dr[modContact.strfIAC_SQUAREFEET_MAX];
                        rstFloorplan.Fields[modContact.strfIAC_MARKETRENT_MIN].Value = dr[modContact.strfIAC_MARKETRENT_MIN];
                        rstFloorplan.Fields[modContact.strfIAC_MARKETRENT_MAX].Value = dr[modContact.strfIAC_MARKETRENT_MAX];
                        rstFloorplan.Fields[modContact.strfIAC_SECURITY_DEPOSIT_MIN].Value = dr[modContact.strfIAC_SECURITY_DEPOSIT_MIN];
                        rstFloorplan.Fields[modContact.strfIAC_AVAILABLE_UNITS].Value = dr[modContact.strfAVAILABLE_UNITS];
                        rstFloorplan.Fields[modContact.strfIAC_SPECIALS].Value = dr[modContact.strfSPECIALS];
                    }
                    objLib.SaveRecordset(modContact.strtIAC_CONTACT_SEARCH_FP, rstFloorplan);
                    oDataset.Dispose();
                    oAdapter.Dispose();
                    oCmd.Dispose();
                #endregion

                #region Unit Search
                    oCmd = new SqlCommand(U_SPROC, oConn);
                    oAdapter = new SqlDataAdapter();
                    oDataset = new DataSet();
                    oCmd.CommandType = CommandType.StoredProcedure;
                    oCmd.Parameters.Add("@ContactId", SqlDbType.Binary, 8);
                    oCmd.Parameters["@ContactId"].Value = vntContactId;
                    oCmd.Parameters["@ContactId"].Direction = ParameterDirection.Input;

                    oAdapter.SelectCommand = oCmd;
                    oAdapter.Fill(oDataset);

                    Recordset rstUnit = objLib.GetNewRecordset(modContact.strtIAC_CONTACT_SEARCH_U,
                        modContact.strfIAC_CONTACT_ID,
                        modContact.strfIAC_PROPERTY_UNIT_ID,
                        modContact.strfIAC_CONTACT_SEARCH_FP_ID,
                        modContact.strfIAC_RECOMMEND,
                        modContact.strfIAC_BLDGID,
                        modContact.strfIAC_UNITID,
                        modContact.strfIAC_CURRUSESQFT,
                        modContact.strfIAC_MARKETRENT,
                        modContact.strfIAC_OCCUSTATUS,
                        modContact.strfIAC_UNITCLASS,
                        modContact.strfIAC_MARKETED_UNITSTATUS,
                        modContact.strfIAC_MARKETED_VACANCYCLASS,
                        modContact.strfIAC_MARKETED_VACATEDATE,
                        modContact.strfIAC_SPECIALS,
                        modContact.strfIAC_CAT,
                        modContact.strfIAC_DOG,
                        modContact.strfIAC_FLOOR);

                    foreach (DataRow dr in oDataset.Tables[0].Rows)
                    {
                        rstUnit.AddNew(Type.Missing, Type.Missing);
                        rstUnit.Fields[modContact.strfIAC_CONTACT_ID].Value = vntContactId;
                        rstUnit.Fields[modContact.strfIAC_PROPERTY_UNIT_ID].Value = dr[modContact.strfIAC_PROPERTY_UNIT_ID];
                        rstUnit.Fields[modContact.strfIAC_CONTACT_SEARCH_FP_ID].Value = dr[modContact.strfIAC_CONTACT_SEARCH_FP_ID];
                        rstUnit.Fields[modContact.strfIAC_RECOMMEND].Value = false;
                        rstUnit.Fields[modContact.strfIAC_BLDGID].Value = dr[modContact.strfIAC_BLDGID];
                        rstUnit.Fields[modContact.strfIAC_UNITID].Value = dr[modContact.strfIAC_UNITID];
                        rstUnit.Fields[modContact.strfIAC_CURRUSESQFT].Value = dr[modContact.strfIAC_CURRUSESQFT];
                        rstUnit.Fields[modContact.strfIAC_MARKETRENT].Value = dr[modContact.strfIAC_MARKETRENT];
                        rstUnit.Fields[modContact.strfIAC_OCCUSTATUS].Value = dr[modContact.strfIAC_OCCUSTATUS];
                        rstUnit.Fields[modContact.strfIAC_UNITCLASS].Value = dr[modContact.strfIAC_UNITCLASS];
                        rstUnit.Fields[modContact.strfIAC_MARKETED_UNITSTATUS].Value = dr[modContact.strfIAC_MARKETED_UNITSTATUS];
                        rstUnit.Fields[modContact.strfIAC_MARKETED_VACANCYCLASS].Value = dr[modContact.strfIAC_MARKETED_VACANCYCLASS];
                        rstUnit.Fields[modContact.strfIAC_MARKETED_VACATEDATE].Value = dr[modContact.strfIAC_MARKETED_VACATEDATE];
                        rstUnit.Fields[modContact.strfIAC_SPECIALS].Value = dr[modContact.strfSPECIALS];
                        rstUnit.Fields[modContact.strfIAC_CAT].Value = dr[modContact.strfIAC_CAT];
                        rstUnit.Fields[modContact.strfIAC_DOG].Value = dr[modContact.strfIAC_DOG];
                        rstUnit.Fields[modContact.strfIAC_FLOOR].Value = dr[modContact.strfIAC_FLOOR];
                    }
                    objLib.SaveRecordset(modContact.strtIAC_CONTACT_SEARCH_U, rstUnit);

                    oDataset.Dispose();
                    oAdapter.Dispose();
                    oCmd.Dispose();
                #endregion

                oConn.Close();

                return true;
            }
            catch (SqlException exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        protected virtual Recordset LookupZip(string strZip)
        {

            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                Recordset rstZip = objLib.GetRecordset(modContact.strqIAC_LOOKUP_ZIP, 1, strZip,
                                                             modContact.strfIAC_CITY,
                                                             modContact.strfIAC_STATE_CODE);
                return rstZip;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        protected virtual void SaveRecommendations(object vntContactId)
        {
            #region Property (Floorplan & Unit are nested)
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                Recordset rstProperties = objLib.GetRecordset(modContact.strqIAC_PROPERTIES_OF_CSF, 1, vntContactId,
                                                             modContact.strfIAC_PROPERTY_ID);

                Object vntActivityId = null;
                Recordset rstActivity = objLib.GetRecordset(modContact.strqIAC_LAST_TRAFFIC_FOR_CONTACT, 1, vntContactId,
                                                            modContact.strfRN_APPOINTMENTS_ID);
                if (!rstActivity.BOF && !rstActivity.EOF)
                {
                    rstActivity.MoveFirst();
                    vntActivityId = rstActivity.Fields[modContact.strfRN_APPOINTMENTS_ID].Value;
                }

                if (!rstProperties.BOF && !rstProperties.EOF)
                {
                    rstProperties.MoveFirst();
                    while (!rstProperties.EOF)
                    {
                        Object vntPropertyId = rstProperties.Fields[modContact.strfIAC_PROPERTY_ID].Value;
                        Recordset rstContactProperty = objLib.GetRecordset(modContact.strqIAC_CONTACTPROPERTY_WITHOUTMPEB, 3, vntPropertyId, vntPropertyId, vntContactId,
                            modContact.strfIAC_CONTACT_ID,
                            modContact.strfIAC_PROPERTY_ID,
                            modContact.strfIAC_ACTIVITY_ID,
                            modContact.strfIAC_MPEB,
                            modContact.strfIAC_REFERRAL_DATE);

                        if (rstContactProperty.BOF && rstContactProperty.EOF) rstContactProperty.AddNew(Type.Missing, Type.Missing);
                        rstContactProperty.Fields[modContact.strfIAC_CONTACT_ID].Value = vntContactId;
                        rstContactProperty.Fields[modContact.strfIAC_PROPERTY_ID].Value = vntPropertyId;
                        rstContactProperty.Fields[modContact.strfIAC_MPEB].Value = false;
                        rstContactProperty.Fields[modContact.strfIAC_REFERRAL_DATE].Value = System.DateTime.Today;
                        rstContactProperty.Fields[modContact.strfIAC_ACTIVITY_ID].Value = vntActivityId;

                        objLib.SaveRecordset(modContact.strtIAC_CONTACT_PROPERTY, rstContactProperty);

                        #region Floorplan (Unit is nested)
                        try
                        {

                            Recordset rstFloorplanRecommendations = objLib.GetRecordset(modContact.strqIAC_RECOMMENDED_FLOORPLANS, 2, vntContactId, rstProperties.Fields[modContact.strfIAC_PROPERTY_ID].Value,
                                                                 modContact.strfIAC_PROPERTY_FLOORPLAN_ID,
                                                                 modContact.strfIAC_CONTACT_SEARCH_FP_ID);

                            if (!rstFloorplanRecommendations.BOF && !rstFloorplanRecommendations.EOF)
                            {
                                rstFloorplanRecommendations.MoveFirst();
                                while (!rstFloorplanRecommendations.EOF)
                                {
                                    Recordset rstFloorplans = objLib.GetRecordset(modContact.strqIAC_PROPERTY_FLOORPLAN, 1, rstFloorplanRecommendations.Fields[modContact.strfIAC_PROPERTY_FLOORPLAN_ID].Value,
                                                                         modContact.strfIAC_PROPERTY_FLOORPLAN_ID,
                                                                         modContact.strfIAC_MARKETRENT,
                                                                         modContact.strfIAC_MARKETRENT_MIN,
                                                                         modContact.strfIAC_MARKETRENT_MAX,
                                                                         modContact.strfIAC_SF,
                                                                         modContact.strfIAC_SQUAREFEET_MIN,
                                                                         modContact.strfIAC_SQUAREFEET_MAX);
                                    if (!rstFloorplans.BOF && !rstFloorplans.EOF)
                                    {
                                        rstFloorplans.MoveFirst();
                                        while (!rstFloorplans.EOF)
                                        {
                                            Object vntContactPropertyId = rstContactProperty.Fields[modContact.strfIAC_CONTACT_PROPERTY_ID].Value;
                                            Object vtnFloorplanId = rstFloorplans.Fields[modContact.strfIAC_PROPERTY_FLOORPLAN_ID].Value;

                                            Recordset rstContactFloorplan = objLib.GetRecordset(modContact.strqIAC_CONTACTFLOORPLAN_WITHOUTMPEB, 3, vntContactPropertyId, vtnFloorplanId, vntContactId,
                                                modContact.strfIAC_CONTACT_PROPERTY_ID,
                                                modContact.strfIAC_CONTACT_ID,
                                                modContact.strfIAC_PROPERTY_FLOORPLAN_ID,
                                                modContact.strfIAC_ACTIVITY_ID,
                                                modContact.strfIAC_MPEB,
                                                modContact.strfIAC_REFERRAL_DATE,
                                                modContact.strfIAC_MARKET_RENT,
                                                modContact.strfIAC_SF);

                                            int RENTmin;
                                            int RENTmax;
                                            string RENTrange;
                                            if (rstFloorplans.Fields[modContact.strfIAC_MARKETRENT_MIN].Value == DBNull.Value || rstFloorplans.Fields[modContact.strfIAC_MARKETRENT_MAX].Value == DBNull.Value)
                                            {
                                                RENTmin = (int)rstFloorplans.Fields[modContact.strfIAC_MARKETRENT].Value;
                                                RENTmax = RENTmin;
                                            }
                                            else
                                            {
                                                RENTmin = (int)rstFloorplans.Fields[modContact.strfIAC_MARKETRENT_MIN].Value;
                                                RENTmax = (int)rstFloorplans.Fields[modContact.strfIAC_MARKETRENT_MAX].Value;
                                            }
                                            if (RENTmin == RENTmax)
                                            {
                                                RENTrange = String.Format("{0:C}", RENTmin);
                                            }
                                            else
                                            {
                                                RENTrange = String.Format("{0:C}", RENTmin) + "-" + String.Format("{0:C}", RENTmax);
                                            }

                                            int SFmin;
                                            int SFmax;
                                            string SFrange;
                                            if (rstFloorplans.Fields[modContact.strfIAC_SQUAREFEET_MIN].Value == DBNull.Value)
                                            {
                                                SFmin = (int)rstFloorplans.Fields[modContact.strfIAC_SF].Value;
                                                SFmax = SFmin;
                                            }
                                            else
                                            {
                                                SFmin = (int)rstFloorplans.Fields[modContact.strfIAC_SQUAREFEET_MIN].Value;
                                                SFmax = (int)rstFloorplans.Fields[modContact.strfIAC_SQUAREFEET_MAX].Value;
                                            }
                                            if (SFmin == SFmax)
                                            {
                                                SFrange = String.Format("{0:n0}", SFmin);
                                            }
                                            else
                                            {
                                                SFrange = String.Format("{0:n0}", SFmin) + "-" + String.Format("{0:n0}", SFmax);
                                            }

                                            if (rstContactFloorplan.BOF && rstContactFloorplan.EOF) rstContactFloorplan.AddNew(Type.Missing, Type.Missing);
                                            rstContactFloorplan.Fields[modContact.strfIAC_CONTACT_PROPERTY_ID].Value = vntContactPropertyId;
                                            rstContactFloorplan.Fields[modContact.strfIAC_CONTACT_ID].Value = vntContactId;
                                            rstContactFloorplan.Fields[modContact.strfIAC_PROPERTY_FLOORPLAN_ID].Value = vtnFloorplanId;
                                            rstContactFloorplan.Fields[modContact.strfIAC_MPEB].Value = false;
                                            rstContactFloorplan.Fields[modContact.strfIAC_MARKET_RENT].Value = RENTrange.Replace(".00", "");
                                            rstContactFloorplan.Fields[modContact.strfIAC_SF].Value = SFrange;
                                            rstContactFloorplan.Fields[modContact.strfIAC_REFERRAL_DATE].Value = System.DateTime.Today;
                                            rstContactFloorplan.Fields[modContact.strfIAC_ACTIVITY_ID].Value = vntActivityId;

                                            objLib.SaveRecordset(modContact.strtIAC_CONTACT_FLOORPLAN, rstContactFloorplan);

                                            #region Unit
                                            try
                                            {
                                                Recordset rstUnits = objLib.GetRecordset(modContact.strqIAC_UNITS_OF_CSF, 1, rstFloorplanRecommendations.Fields[modContact.strfIAC_CONTACT_SEARCH_FP_ID].Value,
                                                                                             modContact.strfIAC_PROPERTY_UNIT_ID,
                                                                                             modContact.strfIAC_CURRUSESQFT,
                                                                                             modContact.strfIAC_MARKETRENT);

                                                Object vntContactFloorplanId = rstContactFloorplan.Fields[modContact.strfIAC_CONTACT_FLOORPLAN_ID].Value;

                                                if (!rstUnits.BOF && !rstUnits.EOF)
                                                {
                                                    rstUnits.MoveFirst();
                                                    while (!rstUnits.EOF)
                                                    {
                                                        Object vntUnitId = rstUnits.Fields[modContact.strfIAC_PROPERTY_UNIT_ID].Value;

                                                        Recordset rstContactUnit = objLib.GetRecordset(modContact.strqIAC_CONTACTUNIT_WITHOUTMPEB, 3, vntContactFloorplanId, vntUnitId, vntContactId,
                                                            modContact.strfIAC_CONTACT_PROPERTY_ID,
                                                            modContact.strfIAC_CONTACT_FLOORPLAN_ID,
                                                            modContact.strfIAC_CONTACT_ID,
                                                            modContact.strfIAC_PROPERTY_UNIT_ID,
                                                            modContact.strfIAC_ACTIVITY_ID,
                                                            modContact.strfIAC_MPEB,
                                                            modContact.strfIAC_SF,
                                                            modContact.strfIAC_MARKETRENT,
                                                            modContact.strfIAC_REFERRAL_DATE);

                                                        if (rstContactUnit.BOF && rstContactUnit.EOF) rstContactUnit.AddNew(Type.Missing, Type.Missing);
                                                        rstContactUnit.Fields[modContact.strfIAC_CONTACT_PROPERTY_ID].Value = vntContactPropertyId;
                                                        rstContactUnit.Fields[modContact.strfIAC_CONTACT_FLOORPLAN_ID].Value = vntContactFloorplanId;
                                                        rstContactUnit.Fields[modContact.strfIAC_CONTACT_ID].Value = vntContactId;
                                                        rstContactUnit.Fields[modContact.strfIAC_PROPERTY_UNIT_ID].Value = vntUnitId;
                                                        rstContactUnit.Fields[modContact.strfIAC_MPEB].Value = false;
                                                        rstContactUnit.Fields[modContact.strfIAC_REFERRAL_DATE].Value = System.DateTime.Today;
                                                        rstContactUnit.Fields[modContact.strfIAC_SF].Value = rstUnits.Fields[modContact.strfIAC_CURRUSESQFT].Value;
                                                        rstContactUnit.Fields[modContact.strfIAC_MARKETRENT].Value = rstUnits.Fields[modContact.strfIAC_MARKETRENT].Value;
                                                        rstContactUnit.Fields[modContact.strfIAC_ACTIVITY_ID].Value = vntActivityId;

                                                        rstUnits.MoveNext();

                                                        objLib.SaveRecordset(modContact.strtIAC_CONTACT_UNIT, rstContactUnit);
                                                    }
                                                }

                                            }
                                            catch (Exception exc)
                                            {
                                                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
                                            }
                                            #endregion


                                            rstFloorplans.MoveNext();
                                        }
                                    }
                                    rstFloorplanRecommendations.MoveNext();
                                }
                            }

                        }
                        catch (Exception exc)
                        {
                            throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
                        }
                        #endregion
                        
                        
                        rstProperties.MoveNext();
                    }
                }

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
            #endregion
        }

        protected virtual Boolean FoundContactPropertyMPEB(object vntContactId)
        {
            if (vntContactId == null) return false;

            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                Recordset rstContactProperty = objLib.GetRecordset(modContact.strqIAC_CONTACT_PROPERTY_WO_MPEB, 1, vntContactId,
                                                             modContact.strfIAC_CONTACT_ID);

                return (rstContactProperty.RecordCount != 0);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        protected virtual void AddContactTraffic(Object vntContactId, Recordset rstPrimary)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                int iTrafficType = (int)(byte)rstPrimary.Fields[modContact.strfIAC_CURRENT_VISIT_TYPE].Value;

                Object vntEmployeeId = RSysSystem.Tables["Employee"].Fields["Employee_Id"].FindValue(RSysSystem.Tables["Employee"].Fields["Rn_Employee_User_id"], RSysSystem.CurrentUserId());
                String description = "";

                switch (iTrafficType)
                {
                    case 0:
                        iTrafficType = 6; //WALK-IN
                        description = "Walk-In";
                        break;
                    case 1:
                        iTrafficType = 7; //PHONE
                        description = "Phone";
                        break;
                    case 2:
                        iTrafficType = 8; //EMAIL
                        description = "Email";
                        break;
                }
                description += " " + rstPrimary.Fields[modContact.strfRN_DESCRIPTOR].Value.ToString();
                Recordset rstContactActivity = objLib.GetRecordset(modContact.strqIAC_DAILY_TRAFFIC_ACTIVITY, 3, vntEmployeeId, iTrafficType, vntContactId,
                                                             modContact.strfCONTACT,
                                                             modContact.strfASSIGNED_BY,
                                                             modContact.strfASSIGNED_TO,
                                                             modContact.strfACCESS_TYPE,
                                                             modContact.strfPRIORITY,
                                                             modContact.strfACTIVITY_TYPE,
                                                             modContact.strfAPPT_DATE,
                                                             modContact.strfSTART_TIME,
                                                             modContact.strfAPPT_DESCRIPTION,
                                                             modContact.strfACTIVITY_COMPLETE,
                                                             modContact.strfACTIVITY_COMPLETED_DATE,
                                                             modContact.strfNOTES);


                if ((rstContactActivity.BOF && rstContactActivity.EOF) || rstPrimary.Fields[modContact.strfCOMMENTS].OriginalValue.ToString() != rstPrimary.Fields[modContact.strfCOMMENTS].Value.ToString())
                {
                    rstContactActivity.AddNew(Type.Missing, Type.Missing);
                    rstContactActivity.Fields[modContact.strfCONTACT].Value = vntContactId;
                    rstContactActivity.Fields[modContact.strfASSIGNED_BY].Value = vntEmployeeId;
                    rstContactActivity.Fields[modContact.strfASSIGNED_TO].Value = vntEmployeeId;
                    rstContactActivity.Fields[modContact.strfACCESS_TYPE].Value = 1;
                    rstContactActivity.Fields[modContact.strfACTIVITY_COMPLETE].Value = true;
                    rstContactActivity.Fields[modContact.strfPRIORITY].Value = "Low";
                    rstContactActivity.Fields[modContact.strfACTIVITY_TYPE].Value = iTrafficType;
                    rstContactActivity.Fields[modContact.strfACTIVITY_COMPLETED_DATE].Value = System.DateTime.Today;
                    rstContactActivity.Fields[modContact.strfAPPT_DATE].Value = System.DateTime.Today;
                    rstContactActivity.Fields[modContact.strfSTART_TIME].Value = System.DateTime.Now;
                    rstContactActivity.Fields[modContact.strfAPPT_DESCRIPTION].Value = description + " Visit";
                    if (rstPrimary.Fields[modContact.strfCOMMENTS].OriginalValue.ToString() != rstPrimary.Fields[modContact.strfCOMMENTS].Value.ToString())
                    {
                        rstContactActivity.Fields[modContact.strfNOTES].Value = rstPrimary.Fields[modContact.strfCOMMENTS].Value.ToString();
                    }

                    objLib.SaveRecordset(modContact.strtACTIVITY, rstContactActivity);
                }

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
    }
}
