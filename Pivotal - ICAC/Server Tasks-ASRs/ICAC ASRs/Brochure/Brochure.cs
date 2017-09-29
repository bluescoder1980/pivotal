using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using Pivotal.Interop.RDALib;
using Pivotal.Interop.ADODBLib;
using Pivotal.Application.Foundation.Utility;
using Pivotal.Application.Foundation.Data.Element;

namespace CRM.Pivotal.IAC
{
    public class Brochure : IRFormScript
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
                TransitionPointParameter objrInstance = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objrInstance.ParameterList = ParameterList;

                if (objrInstance.HasValidParameters() == false)
                {
                    objrInstance.Construct();
                }
                object[] parameterArray = objrInstance.GetUserDefinedParameterArray();

                object[] recordsetArray = (object[])Recordsets;
                Recordset rstPrimary = (Recordset)recordsetArray[0];

                rstPrimary.Fields[modBrochure.strfIAC_OPPORTUNITY_ID].Value = GetOpportunityId(rstPrimary.Fields[modBrochure.strfIAC_CONTACT_ID].Value);
                object vntRecordId = pForm.DoAddFormData(Recordsets, ref ParameterList);
                
                
                String guid = CreateMPEB(pForm, ref recordsetArray, vntRecordId);

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                Recordset rstBrochure = objLib.GetRecordset(vntRecordId, modBrochure.strtIAC_BROCHURE,
                        modBrochure.strfIAC_BROCHURE_GUID);
                rstBrochure.Fields[modBrochure.strfIAC_BROCHURE_GUID].Value = guid;
                objLib.SaveRecordset(modBrochure.strtIAC_BROCHURE, rstBrochure);
                
                parameterArray = new object[] { guid };
                ParameterList = objrInstance.SetUserDefinedParameterArray(parameterArray);

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
                TransitionPointParameter objrInstance = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objrInstance.ParameterList = ParameterList;

                if (objrInstance.HasValidParameters() == false)
                {
                    objrInstance.Construct();
                }

                object[] parameterArray = objrInstance.GetUserDefinedParameterArray();

                switch (MethodName)
                {
                    case modBrochure.strmGENERATE_GUESTCARDS:
                        objrInstance.CheckUserDefinedParameterNumber(2);
                        Boolean result = this.CreateGuestCard((Recordset)parameterArray[0], (Recordset)parameterArray[1]);
                        parameterArray = new object[] { result };
                        break;
                    default:
                        string message = MethodName + TypeConvert.ToString(RldtLangDict.GetText(modBrochure.strdINVALID_METHOD));
                        parameterArray = new object[] { message };
                        throw new PivotalApplicationException(message, modBrochure.glngERR_METHOD_NOT_DEFINED);
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
                    GetPropertiesMPEB(ref recordsetArray, pForm, rstRecordset.Fields[modBrochure.strfIAC_CONTACT_ID].Value);
                    GetRequirements(ref recordsetArray, pForm, rstRecordset.Fields[modBrochure.strfIAC_CONTACT_ID].Value);
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

                //Save the data
                pForm.DoSaveFormData(Recordsets, ref ParameterList);

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        
        protected virtual void GetPropertiesMPEB(ref object[] recordsetArray, IRForm pForm, object vntContactId)
        {
            try
            {
                String SegmentName = modBrochure.strsCOMMUNITY_INFORMATION;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                Recordset rstSegment = pForm.SecondaryFromVariantArray(recordsetArray, SegmentName);
                Recordset rstProperties = objLib.GetRecordset(modBrochure.strqPROPERTY_CONTACTPROPERTY_MPEB, 1, vntContactId,
                                                             modBrochure.strfIAC_PROPERTY_ID,
                                                             modBrochure.strfRN_DESCRIPTOR);

                if (!rstProperties.BOF && !rstProperties.EOF)
                {
                    rstProperties.Sort = modBrochure.strfRN_DESCRIPTOR;
                    rstProperties.MoveFirst();
                    while (!rstProperties.EOF)
                    {
                        rstSegment.AddNew(Type.Missing, Type.Missing);
                        rstSegment.Fields[modBrochure.strfIAC_PROPERTY_ID].Value = rstProperties.Fields[modBrochure.strfIAC_PROPERTY_ID].Value;
                        rstProperties.MoveNext();
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        protected virtual void GetRequirements(ref object[] recordsetArray, IRForm pForm, object vntContactId)
        {
            try
            {
                String SegmentName = modBrochure.strsREQUIREMENTS;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                Recordset rstSegment = pForm.SecondaryFromVariantArray(recordsetArray, SegmentName);

                /*
                Recordset rstRegions = objLib.GetRecordset(modBrochure.strqIAC_SEARCH_REGIONS, 1, vntContactId,
                                                             modBrochure.strfRN_DESCRIPTOR);

                if (!rstRegions.BOF && !rstRegions.EOF)
                {
                    rstRegions.Sort = modBrochure.strfRN_DESCRIPTOR;
                    rstRegions.MoveFirst();
                    while (!rstRegions.EOF)
                    {
                        rstSegment.AddNew(Type.Missing, Type.Missing);
                        rstSegment.Fields[modBrochure.strfIAC_REQUIREMENT].Value = rstRegions.Fields[modBrochure.strfRN_DESCRIPTOR].Value;
                        rstSegment.Fields[modBrochure.strfIAC_CATEGORY].Value = modBrochure.strCATEGORY_REGION;
                        rstRegions.MoveNext();
                    }
                }
                */

                Recordset rstProperties = objLib.GetRecordset(modBrochure.strqIAC_SEARCH_PROPERTIES, 1, vntContactId,
                                                     modBrochure.strfRN_DESCRIPTOR);

                if (!rstProperties.BOF && !rstProperties.EOF)
                {
                    rstProperties.Sort = modBrochure.strfRN_DESCRIPTOR;
                    rstProperties.MoveFirst();
                    while (!rstProperties.EOF)
                    {
                        rstSegment.AddNew(Type.Missing, Type.Missing);
                        rstSegment.Fields[modBrochure.strfIAC_REQUIREMENT].Value = rstProperties.Fields[modBrochure.strfRN_DESCRIPTOR].Value;
                        rstSegment.Fields[modBrochure.strfIAC_CATEGORY].Value = modBrochure.strCATEGORY_PROPERTY;
                        rstProperties.MoveNext();
                    }
                }

                Recordset rstContact = objLib.GetRecordset(vntContactId, modBrochure.strtCONTACT,
                                    modBrochure.strfIAC_FA_CAT,
                                    modBrochure.strfIAC_FA_DOG,
                                    modBrochure.strfIAC_FA_DOGWEIGHT);
                if ((bool)rstContact.Fields[modBrochure.strfIAC_FA_CAT].Value == true)
                {
                    rstSegment.AddNew(Type.Missing, Type.Missing);
                    rstSegment.Fields[modBrochure.strfIAC_REQUIREMENT].Value = modBrochure.strPET_CAT;
                    rstSegment.Fields[modBrochure.strfIAC_CATEGORY].Value = modBrochure.strCATEGORY_PET;
                }
                if ((bool)rstContact.Fields[modBrochure.strfIAC_FA_DOG].Value == true && rstContact.Fields[modBrochure.strfIAC_FA_DOGWEIGHT].Value != System.DBNull.Value)
                {
                    string DogWeight = RSysSystem.Tables[modBrochure.strtCONTACT].Fields[modBrochure.strfIAC_FA_DOGWEIGHT].Choices[rstContact.Fields[modBrochure.strfIAC_FA_DOGWEIGHT].Value];
                    rstSegment.AddNew(Type.Missing, Type.Missing);
                    rstSegment.Fields[modBrochure.strfIAC_REQUIREMENT].Value = modBrochure.strPET_DOG + " " + DogWeight;
                    rstSegment.Fields[modBrochure.strfIAC_CATEGORY].Value = modBrochure.strCATEGORY_PET;
                }


                Recordset rstSearchItems = objLib.GetRecordset(modBrochure.strqIAC_INCLUDED_SEARCHITEMS, 1, vntContactId,
                             modBrochure.strfIAC_FORMULA_SEARCHITEM,
                             modBrochure.strfIAC_FORMULA_CATEGORY,
                             modBrochure.strfIAC_FORMULA_SORTORDER);

                if (!rstSearchItems.BOF && !rstSearchItems.EOF)
                {
                    rstSearchItems.Sort = modBrochure.strfIAC_FORMULA_CATEGORY + ", " + modBrochure.strfIAC_FORMULA_SORTORDER;
                    rstSearchItems.MoveFirst();
                    while (!rstSearchItems.EOF)
                    {
                        rstSegment.AddNew(Type.Missing, Type.Missing);
                        rstSegment.Fields[modBrochure.strfIAC_REQUIREMENT].Value = rstSearchItems.Fields[modBrochure.strfIAC_FORMULA_SEARCHITEM].Value;
                        rstSegment.Fields[modBrochure.strfIAC_CATEGORY].Value = rstSearchItems.Fields[modBrochure.strfIAC_FORMULA_CATEGORY].Value;
                        rstSearchItems.MoveNext();
                    }
                }

                Recordset rstSchools = objLib.GetRecordset(modBrochure.strqIAC_SEARCH_SCHOOLS, 1, vntContactId,
                                         modBrochure.strfRN_DESCRIPTOR);

                if (!rstSchools.BOF && !rstSchools.EOF)
                {
                    rstSchools.Sort = modBrochure.strfRN_DESCRIPTOR;
                    rstSchools.MoveFirst();
                    while (!rstSchools.EOF)
                    {
                        rstSegment.AddNew(Type.Missing, Type.Missing);
                        rstSegment.Fields[modBrochure.strfIAC_REQUIREMENT].Value = rstSchools.Fields[modBrochure.strfRN_DESCRIPTOR].Value;
                        rstSegment.Fields[modBrochure.strfIAC_CATEGORY].Value = modBrochure.strCATEGORY_SCHOOL;
                        rstSchools.MoveNext();
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        protected virtual object GetOpportunityId(object vntContactId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                Recordset rstOpportunity = objLib.GetRecordset(modBrochure.strqIAC_INPROGRESS_OPPORTUNITY, 1, vntContactId,
                                                             modBrochure.strfOPPORTUNITY_ID);

                if (!rstOpportunity.BOF && !rstOpportunity.EOF)
                {
                    rstOpportunity.MoveFirst();
                    return rstOpportunity.Fields[modBrochure.strfOPPORTUNITY_ID].Value;
                }
                else
                {
                    Recordset rstNewOpportunity = objLib.GetNewRecordset(modBrochure.strtOPPORTUNITY,
                                                    modBrochure.strfCONTACT_ID,
                                                    modBrochure.strfSTATUS,
                                                    modBrochure.strfACCOUNT_MANAGER_ID);

                    rstNewOpportunity.AddNew(Type.Missing, Type.Missing);
                    rstNewOpportunity.Fields[modBrochure.strfCONTACT_ID].Value = vntContactId;
                    rstNewOpportunity.Fields[modBrochure.strfACCOUNT_MANAGER_ID].Value = RSysSystem.Tables["Employee"].Fields["Employee_Id"].FindValue(RSysSystem.Tables["Employee"].Fields["Rn_Employee_User_id"], RSysSystem.CurrentUserId());
                    rstNewOpportunity.Fields[modBrochure.strfSTATUS].Value = "In Progress";
                    objLib.SaveRecordset(modBrochure.strtOPPORTUNITY, rstNewOpportunity);
                    return rstNewOpportunity.Fields[modBrochure.strfOPPORTUNITY_ID].Value;
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        protected virtual Boolean CreateGuestCard(Recordset rstPrimary, Recordset rstProperties)
        {
            try
            {
                Object vntContactId = rstPrimary.Fields[modBrochure.strfIAC_CONTACT_ID].Value;
                String External_DB = CRM.Pivotal.IAC.Properties.Settings.Default.External_DB;
                String External_Scope = CRM.Pivotal.IAC.Properties.Settings.Default.External_Scope;
                String GC_SPROC = External_DB + "." + External_Scope + "." + modBrochure.strxIAC_CONTACT_GUESTCARD_INFO;

                SqlConnection oConn = new SqlConnection(RSysSystem.EnterpriseString.Replace("provider=RDSO.RSQL;", ""));

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;


                if (!rstProperties.BOF && !rstProperties.EOF)
                {
                    rstProperties.Sort = modBrochure.strfIAC_SEQUENCE;
                    rstProperties.MoveFirst();
                    while (!rstProperties.EOF)
                    {
                        object vntPropertyId = rstProperties.Fields[modBrochure.strfIAC_PROPERTY_ID].Value;


                        SqlCommand oCmd = new SqlCommand(GC_SPROC, oConn);
                        SqlDataAdapter oAdapter = new SqlDataAdapter();
                        DataSet oDataset = new DataSet();
                        oCmd.CommandType = CommandType.StoredProcedure;

                        oCmd.Parameters.Add("@ContactID", SqlDbType.Binary, 8);
                        oCmd.Parameters["@ContactID"].Value = vntContactId;
                        oCmd.Parameters["@ContactID"].Direction = ParameterDirection.Input;

                        oCmd.Parameters.Add("@PropertyID", SqlDbType.Binary, 8);
                        oCmd.Parameters["@PropertyID"].Value = vntPropertyId;
                        oCmd.Parameters["@PropertyID"].Direction = ParameterDirection.Input;

                        oConn.Open();
                        oAdapter.SelectCommand = oCmd;
                        oAdapter.Fill(oDataset);

                        if (oDataset.Tables[0].Rows.Count != 0)
                        {
                            foreach (DataRow dr in oDataset.Tables[0].Rows)
                            {
                                String PMS_DBServer;
                                String PMS_DB = CRM.Pivotal.IAC.Properties.Settings.Default.PMS_DB;
                                String PMS_GC_SPROC = PMS_DB + "." + External_Scope + "." + modBrochure.strxPMS_GET_GUESTCARD;

                                if (RSysSystem.SystemName.ToUpper() == "ICAC")
                                {
                                    PMS_DBServer = CRM.Pivotal.IAC.Properties.Settings.Default.PMS_DBServer;
                                }
                                else
                                {
                                    PMS_DBServer = CRM.Pivotal.IAC.Properties.Settings.Default.PMS_DBServer_Test;
                                }
                                String SQL_CONN = RSysSystem.EnterpriseString.Replace("provider=RDSO.RSQL;", "").Replace("IAC_ED", PMS_DB);
                                SQL_CONN = SQL_CONN.Substring(0, SQL_CONN.IndexOf("source=") + 7) + PMS_DBServer + SQL_CONN.Substring(SQL_CONN.IndexOf(";initial catalog"), SQL_CONN.Length - SQL_CONN.IndexOf(";initial catalog"));

                                SqlConnection conn = new SqlConnection(SQL_CONN);
                                conn.Open();

                                SqlCommand cmd = conn.CreateCommand();
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = PMS_GC_SPROC;

                                SqlParameter returnvalue = cmd.Parameters.Add("@GuestCardId", SqlDbType.Int);
                                returnvalue.Direction = ParameterDirection.Output;

                                foreach (DataColumn column in oDataset.Tables[0].Columns)
                                {
                                    if (column.ColumnName != "IAC_Property_Id")
                                    {
                                        cmd.Parameters.AddWithValue("@" + column.ColumnName, dr[column.ColumnName]);
                                        cmd.Parameters["@" + column.ColumnName].Direction = ParameterDirection.Input;
                                    }
                                }

                                SqlDataReader rdr = cmd.ExecuteReader();
                                while (rdr.Read())
                                {
                                    Console.WriteLine("{0} {1}"
                                     , rdr[0].ToString().PadRight(5)
                                     , rdr[1].ToString()
                                    );
                                }
                                rdr.Close();
                                if (cmd.Parameters["@GuestCardId"].Value != System.DBNull.Value)
                                {
                                    Int32 guestcard = (int)cmd.Parameters["@GuestCardId"].Value;
                                    Object vntGuestCardPropertyId = dr["IAC_Property_Id"];
                                    Recordset rstGuestCards = objLib.GetRecordset(modBrochure.strqIAC_MPEB_GUESTCARD, 3, vntGuestCardPropertyId, vntGuestCardPropertyId, vntContactId,
                                        modBrochure.strfIAC_CONTACT_ID,
                                        modBrochure.strfIAC_PROPERTY_ID,
                                        modBrochure.strfIAC_GUESTCARD_NUMBER,
                                        modBrochure.strfIAC_ACTIVE);
                                    Boolean match = false;
                                    if (!rstGuestCards.BOF && !rstGuestCards.EOF)
                                    {
                                        rstGuestCards.MoveFirst();
                                        while (!rstGuestCards.EOF)
                                        {
                                            if ((int)rstGuestCards.Fields[modBrochure.strfIAC_GUESTCARD_NUMBER].Value != guestcard)
                                            {
                                                rstGuestCards.Fields[modBrochure.strfIAC_ACTIVE].Value = false;
                                            }
                                            else
                                            {
                                                rstGuestCards.Fields[modBrochure.strfIAC_ACTIVE].Value = true;
                                                match = true;
                                            }
                                            rstGuestCards.MoveNext();
                                        }
                                    }

                                    Recordset rstGuestCard = objLib.GetRecordset(modBrochure.strqIAC_INACTIVE_CONTACTGUESTCARD_WITH_NUMBER, 2, guestcard, vntGuestCardPropertyId,
                                        modBrochure.strfIAC_CONTACT_ID,
                                        modBrochure.strfIAC_ACTIVE);

                                    if (!rstGuestCard.BOF && !rstGuestCard.EOF)
                                    {
                                        match = true;
                                        rstGuestCard.MoveFirst();
                                        while (!rstGuestCard.EOF)
                                        {
                                            rstGuestCard.Fields[modBrochure.strfIAC_CONTACT_ID].Value = vntContactId;
                                            rstGuestCard.Fields[modBrochure.strfIAC_ACTIVE].Value = true;
                                            rstGuestCard.MoveNext();
                                        }
                                        objLib.SaveRecordset(modBrochure.strtIAC_CONTACT_GUESTCARD, rstGuestCard);
                                    }

                                    if (!match)
                                    {
                                        rstGuestCards.AddNew(Type.Missing, Type.Missing);
                                        rstGuestCards.Fields[modBrochure.strfIAC_CONTACT_ID].Value = vntContactId;
                                        rstGuestCards.Fields[modBrochure.strfIAC_PROPERTY_ID].Value = vntGuestCardPropertyId;
                                        rstGuestCards.Fields[modBrochure.strfIAC_GUESTCARD_NUMBER].Value = guestcard;
                                        rstGuestCards.Fields[modBrochure.strfIAC_ACTIVE].Value = true;
                                    }
                                    objLib.SaveRecordset(modBrochure.strtIAC_CONTACT_GUESTCARD, rstGuestCards);
                                }
                                else
                                {
                                    return false;
                                }
                                conn.Close();
                            }
                        }

                        oConn.Close();
                        rstProperties.MoveNext();
                    }
                    oConn.Dispose();
                }
                return true;
            }
            catch (SqlException exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        protected virtual string CreateMPEB(IRForm pForm, ref object[] recordsetArray, object vntBrochureId)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(dec);
                XmlElement root = doc.CreateElement("MPEB");
                doc.AppendChild(root);

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                Recordset rstPrimary = (Recordset)recordsetArray[0];
                object vntContactId = rstPrimary.Fields[modBrochure.strfIAC_CONTACT_ID].Value;

                // START CONTACT
                #region CONTACT
                Recordset rstContact = objLib.GetRecordset(vntContactId, modBrochure.strtCONTACT,
                                    modBrochure.strfFIRST_NAME,
                                    modBrochure.strfLAST_NAME,
                                    modBrochure.strfEMAIL,
                                    modBrochure.strfIAC_FA_CAT,
                                    modBrochure.strfIAC_FA_DOG,
                                    modBrochure.strfIAC_FA_DOGWEIGHT,
                                    modBrochure.strfCOMMENTS);

                XmlElement contact = doc.CreateElement("Contact");
                contact.SetAttribute("firstname", rstContact.Fields[modBrochure.strfFIRST_NAME].Value.ToString());
                contact.SetAttribute("lastname", rstContact.Fields[modBrochure.strfLAST_NAME].Value.ToString());

                if (rstContact.Fields[modBrochure.strfEMAIL].Value != System.DBNull.Value)
                {
                    XmlElement email = doc.CreateElement("Email");
                    email.InnerText = rstContact.Fields[modBrochure.strfEMAIL].Value.ToString();
                    contact.AppendChild(email);
                }

                // START HOUSING SPECIALIST
                #region HOUSING SPECIALIST
                Recordset rstHousingSpecialist = objLib.GetRecordset(modBrochure.strqSYS_EMPLOYEE_USERID, 1, RSysSystem.CurrentUserId(),
                        modBrochure.strfEMPLOYEE_ID,
                        modBrochure.strfFIRST_NAME,
                        modBrochure.strfLAST_NAME,
                        modBrochure.strfWORK_EMAIL,
                        modBrochure.strfWORK_PHONE);
                if (rstHousingSpecialist.BOF && rstHousingSpecialist.EOF)
                {
                    throw new Exception("No employee record found for " + RSysSystem.CurrentUserName().ToString());
                }
                else
                {
                    XmlElement specialist = doc.CreateElement("HousingSpecialist");
                    specialist.SetAttribute("phone", rstHousingSpecialist.Fields[modBrochure.strfWORK_PHONE].Value.ToString());
                    specialist.SetAttribute("firstname", rstHousingSpecialist.Fields[modBrochure.strfFIRST_NAME].Value.ToString());
                    specialist.SetAttribute("lastname", rstHousingSpecialist.Fields[modBrochure.strfLAST_NAME].Value.ToString());
                    specialist.SetAttribute("email", rstHousingSpecialist.Fields[modBrochure.strfWORK_EMAIL].Value.ToString());
                    contact.AppendChild(specialist);
                }

                #endregion
                // END HOUSING SPECIALIST

                // START REQUESTS & NOTES
                #region REQUESTS
                Recordset rstRequirements = pForm.SecondaryFromVariantArray(recordsetArray, modBrochure.strsREQUIREMENTS);
                XmlElement requests = doc.CreateElement("Requests");
                if (!rstRequirements.BOF && !rstRequirements.EOF)
                {
                    rstRequirements.MoveFirst();
                    while (!rstRequirements.EOF)
                    {
                        String category = rstRequirements.Fields[modBrochure.strfIAC_CATEGORY].Value.ToString();
                        if (category != "Property" && category != "Pet")
                        {
                            XmlElement request = doc.CreateElement("Request");
                            request.InnerText = rstRequirements.Fields[modBrochure.strfIAC_REQUIREMENT].Value.ToString();
                            requests.AppendChild(request);
                        }
                        rstRequirements.MoveNext();
                    }
                }

                // START NOTES
                #region NOTES
                if (rstPrimary.Fields[modBrochure.strfIAC_COMMENTS].Value != System.DBNull.Value)
                {
                    XmlElement notes = doc.CreateElement("Notes");
                    notes.InnerText = rstPrimary.Fields[modBrochure.strfIAC_COMMENTS].Value.ToString();
                    requests.AppendChild(notes);
                }
                #endregion
                // END NOTES
                contact.AppendChild(requests);
                #endregion
                // END REQUESTS & NOTES

                // START PETS
                #region PETS
                if ((bool)rstContact.Fields[modBrochure.strfIAC_FA_CAT].Value == true || (bool)rstContact.Fields[modBrochure.strfIAC_FA_DOG].Value == true)
                {
                    XmlElement pet = doc.CreateElement("Pet");
                    if ((bool)rstContact.Fields[modBrochure.strfIAC_FA_DOG].Value == true)
                    {
                        pet.SetAttribute("type", modBrochure.strPET_DOG);
                        string DogWeight = RSysSystem.Tables[modBrochure.strtCONTACT].Fields[modBrochure.strfIAC_FA_DOGWEIGHT].Choices[rstContact.Fields[modBrochure.strfIAC_FA_DOGWEIGHT].Value];
                        XmlElement weight = doc.CreateElement("Weight");
                        weight.InnerText = DogWeight;
                        pet.AppendChild(weight);
                    }
                    else
                    {
                        pet.SetAttribute("type", modBrochure.strPET_CAT);
                    }
                    contact.AppendChild(pet);
                }
                #endregion
                // END PETS

                // START OTHER OCCUPANTS
                #region OTHER OCCUPANTS
                XmlElement occupants = doc.CreateElement("OtherOccupants");
                Recordset rstOccupants = objLib.GetRecordset(modBrochure.strqIAC_FOCAL_CONTACTS, 1, vntContactId,
                    modBrochure.strfFIRST_NAME,
                    modBrochure.strfLAST_NAME,
                    modBrochure.strfEMAIL);
                if (!rstOccupants.BOF && !rstOccupants.EOF)
                {
                    rstOccupants.MoveFirst();
                    while (!rstOccupants.EOF)
                    {
                        XmlElement occupant = doc.CreateElement("Occupant");
                        occupant.SetAttribute("firstname", rstOccupants.Fields[modBrochure.strfFIRST_NAME].Value.ToString());
                        occupant.SetAttribute("lastname", rstOccupants.Fields[modBrochure.strfLAST_NAME].Value.ToString());
                        if (rstOccupants.Fields[modBrochure.strfEMAIL].Value != System.DBNull.Value)
                        {
                            XmlElement email = doc.CreateElement("Email");
                            email.InnerText = rstOccupants.Fields[modBrochure.strfEMAIL].Value.ToString();
                            occupant.AppendChild(email);
                        }
                        occupants.AppendChild(occupant);
                        rstOccupants.MoveNext();
                    }
                }
                contact.AppendChild(occupants);
                #endregion
                // END OTHER OCCUPANTS

                root.AppendChild(contact);
                #endregion
                // END CONTACT

                // START RECOMMENDATIONS
                #region RECOMMENDATIONS
                XmlElement recommendations = doc.CreateElement("Recommendations");
                recommendations.SetAttribute("startaddress", rstPrimary.Fields[modBrochure.strfIAC_STARTING_ADDRESS].Value.ToString());

                Recordset rstProperties = pForm.SecondaryFromVariantArray(recordsetArray, modBrochure.strsCOMMUNITY_INFORMATION);
                rstProperties.Sort = modBrochure.strfIAC_SEQUENCE;
                rstProperties.MoveFirst();
                while (!rstProperties.EOF)
                {
                    object vntPropertyId = rstProperties.Fields[modBrochure.strfIAC_PROPERTY_ID].Value;

                    XmlElement property = doc.CreateElement("Property");
                    Recordset rstProperty = objLib.GetRecordset(vntPropertyId, modBrochure.strtIAC_PROPERTY,
                        modBrochure.strfIAC_PROPERTY_NAME,
                        modBrochure.strfIAC_OUTGOING_PMS_ID,
                        modBrochure.strfIAC_VAULTWARE_ID);


                    property.SetAttribute("sequence", rstProperties.Fields[modBrochure.strfIAC_SEQUENCE].Value.ToString());
                    if (rstProperty.Fields[modBrochure.strfIAC_OUTGOING_PMS_ID].Value != System.DBNull.Value)
                    {
                        property.SetAttribute("propertyid", rstProperty.Fields[modBrochure.strfIAC_OUTGOING_PMS_ID].Value.ToString());
                        property.SetAttribute("vaultwareid", "0");
                    }
                    property.SetAttribute("name", rstProperty.Fields[modBrochure.strfIAC_PROPERTY_NAME].Value.ToString());
                    if (rstProperty.Fields[modBrochure.strfIAC_OUTGOING_PMS_ID].Value == System.DBNull.Value)
                    {
                        if (rstProperty.Fields[modBrochure.strfIAC_VAULTWARE_ID].Value != System.DBNull.Value)
                        {
                            property.SetAttribute("vaultwareid", rstProperty.Fields[modBrochure.strfIAC_VAULTWARE_ID].Value.ToString());
                        }
                    }

                    // START GUESTCARDS
                    #region GUESTCARDS
                    Recordset rstGuestCards = objLib.GetRecordset(modBrochure.strqIAC_MPEB_GUESTCARD, 3, vntPropertyId, vntPropertyId, vntContactId,
                        modBrochure.strfIAC_FORMULA_PMS_ID,
                        modBrochure.strfIAC_FORMULA_PROPERTY_NAME,
                        modBrochure.strfIAC_GUESTCARD_NUMBER);
                    if (!rstGuestCards.BOF && !rstGuestCards.EOF)
                    {
                        rstGuestCards.MoveFirst();
                        while (!rstGuestCards.EOF)
                        {
                            String PropertyName = rstGuestCards.Fields[modBrochure.strfIAC_FORMULA_PROPERTY_NAME].Value.ToString();
                            XmlElement guestcard = doc.CreateElement("GuestCard");
                            if (rstGuestCards.Fields[modBrochure.strfIAC_FORMULA_PMS_ID].Value != System.DBNull.Value)
                            {
                                guestcard.SetAttribute("propertyid", rstGuestCards.Fields[modBrochure.strfIAC_FORMULA_PMS_ID].Value.ToString());
                            }
                            if (rstProperty.Fields[modBrochure.strfIAC_PROPERTY_NAME].Value.ToString() != PropertyName)
                            {
                                guestcard.SetAttribute("guestcardnumber", PropertyName.Substring(0, 1) + "-" + rstGuestCards.Fields[modBrochure.strfIAC_GUESTCARD_NUMBER].Value.ToString());
                            }
                            else
                            {
                                guestcard.SetAttribute("guestcardnumber", rstGuestCards.Fields[modBrochure.strfIAC_GUESTCARD_NUMBER].Value.ToString());
                            }
                            property.AppendChild(guestcard);
                            rstGuestCards.MoveNext();
                        }
                    }
                    #endregion
                    // END GUESTCARDS

                    XmlElement floorplans = doc.CreateElement("FloorPlans");

                    // START FLOORPLAN & UNITS
                    #region FLOORPLANS
                    objLib.PermissionIgnored = true;
                    Recordset rstFloorplans = objLib.GetRecordset(modBrochure.strqIAC_FLOORPLAN_CONTACTFLOORPLANS_MPEB, 3, vntPropertyId, vntPropertyId, vntContactId,
                        modBrochure.strfIAC_PROPERTY_FLOORPLAN_ID,
                        modBrochure.strfIAC_UNITTYPE,
                        modBrochure.strfIAC_FLOORPLAN_NAME,
                        modBrochure.strfIAC_VAULTWARE_ID,
                        modBrochure.strfIAC_FORMULA_PMS_PROPERTY_ID,
                        modBrochure.strfIAC_FORMULA_PROPERTY_NAME,
                        modBrochure.strfIAC_PROPERTY_FLOORPLAN_ID);

                    if (!rstFloorplans.BOF && !rstFloorplans.EOF)
                    {
                        rstFloorplans.Sort = modBrochure.strfIAC_FORMULA_PROPERTY_NAME + ", " + modBrochure.strfIAC_FLOORPLAN_NAME + ", " + modBrochure.strfIAC_UNITTYPE;
                        rstFloorplans.MoveFirst();
                        while (!rstFloorplans.EOF)
                        {
                            XmlElement floorplan = doc.CreateElement("FloorPlan");
                            floorplan.SetAttribute("unittype", rstFloorplans.Fields[modBrochure.strfIAC_UNITTYPE].Value.ToString());
                            floorplan.SetAttribute("name", rstFloorplans.Fields[modBrochure.strfIAC_FLOORPLAN_NAME].Value.ToString());
                            if (rstFloorplans.Fields[modBrochure.strfIAC_VAULTWARE_ID].Value != System.DBNull.Value)
                            {
                                floorplan.SetAttribute("vaultwareid", rstFloorplans.Fields[modBrochure.strfIAC_VAULTWARE_ID].Value.ToString());
                            }
                            if (rstFloorplans.Fields[modBrochure.strfIAC_FORMULA_PMS_PROPERTY_ID].Value != System.DBNull.Value)
                            {
                                floorplan.SetAttribute("propertyid", rstFloorplans.Fields[modBrochure.strfIAC_FORMULA_PMS_PROPERTY_ID].Value.ToString());
                            }

                            // START UNIT
                            #region UNITS
                            XmlElement units = doc.CreateElement("Units");
                            
                            Recordset rstUnits = objLib.GetRecordset(modBrochure.strqIAC_UNIT_CONTACTUNITS_MPEB, 2, rstFloorplans.Fields[modBrochure.strfIAC_PROPERTY_FLOORPLAN_ID].Value, vntContactId,
                                modBrochure.strfIAC_CURRENTUSE_SF,
                                modBrochure.strfIAC_MARKETRENT,
                                modBrochure.strfIAC_UNITID,
                                modBrochure.strfIAC_BLDGID);

                            if (!rstUnits.BOF && !rstUnits.EOF)
                            {
                                rstUnits.Sort = modBrochure.strfIAC_UNITID;
                                rstUnits.MoveFirst();
                                while (!rstUnits.EOF)
                                {
                                    XmlElement unit = doc.CreateElement("Unit");
                                    unit.SetAttribute("rent", rstUnits.Fields[modBrochure.strfIAC_MARKETRENT].Value.ToString());
                                    unit.SetAttribute("number", rstUnits.Fields[modBrochure.strfIAC_UNITID].Value.ToString());
                                    unit.SetAttribute("sf", rstUnits.Fields[modBrochure.strfIAC_CURRENTUSE_SF].Value.ToString());
                                    units.AppendChild(unit);
                                    rstUnits.MoveNext();
                                }
                            }

                            Recordset rstContactUnits = objLib.GetRecordset(modBrochure.strqIAC_CONTACT_UNITS, 2, rstFloorplans.Fields[modBrochure.strfIAC_PROPERTY_FLOORPLAN_ID].Value, vntContactId,
                                modBrochure.strfIAC_MPEB,
                                modBrochure.strfIAC_BROCHURE_ID);
                            if (!rstContactUnits.BOF && !rstContactUnits.EOF)
                            {
                                rstContactUnits.MoveFirst();
                                while (!rstContactUnits.EOF)
                                {
                                    rstContactUnits.Fields[modBrochure.strfIAC_MPEB].Value = true;
                                    rstContactUnits.Fields[modBrochure.strfIAC_BROCHURE_ID].Value = vntBrochureId;
                                    rstContactUnits.MoveNext();
                                }
                                objLib.SaveRecordset(modBrochure.strtIAC_CONTACT_UNIT, rstContactUnits);
                            }

                            floorplan.AppendChild(units);
                            #endregion
                            // END UNIT

                            Recordset rstContactFloorplans = objLib.GetRecordset(modBrochure.strqIAC_CONTACT_FLOORPLANS, 2, rstFloorplans.Fields[modBrochure.strfIAC_PROPERTY_FLOORPLAN_ID].Value, vntContactId,
                                modBrochure.strfIAC_MPEB,
                                modBrochure.strfIAC_BROCHURE_ID);
                            if (!rstContactFloorplans.BOF && !rstContactFloorplans.EOF)
                            {
                                rstContactFloorplans.MoveFirst();
                                while (!rstContactFloorplans.EOF)
                                {
                                    rstContactFloorplans.Fields[modBrochure.strfIAC_MPEB].Value = true;
                                    rstContactFloorplans.Fields[modBrochure.strfIAC_BROCHURE_ID].Value = vntBrochureId;
                                    rstContactFloorplans.MoveNext();
                                }
                                objLib.SaveRecordset(modBrochure.strtIAC_CONTACT_FLOORPLAN, rstContactFloorplans);
                            }

                            floorplans.AppendChild(floorplan);

                            rstFloorplans.MoveNext();
                        }
                    }

                    property.AppendChild(floorplans);
                    #endregion
                    // END FLOORPLAN & UNITS

                    // START NOTES
                    #region NOTES
                    if (rstProperties.Fields[modBrochure.strfIAC_NOTES].Value != System.DBNull.Value)
                    {
                        XmlElement notes = doc.CreateElement("Notes");
                        notes.InnerText = rstProperties.Fields[modBrochure.strfIAC_NOTES].Value.ToString();
                        property.AppendChild(notes);
                    }
                    #endregion
                    // END NOTES

                    recommendations.AppendChild(property);

                    Recordset rstContactProperties = objLib.GetRecordset(modBrochure.strqIAC_CONTACT_PROPERTIES, 3, rstProperties.Fields[modBrochure.strfIAC_PROPERTY_ID].Value, rstProperties.Fields[modBrochure.strfIAC_PROPERTY_ID].Value, vntContactId,
                        modBrochure.strfIAC_MPEB,
                        modBrochure.strfIAC_BROCHURE_ID);
                    if (!rstContactProperties.BOF && !rstContactProperties.EOF)
                    {
                        rstContactProperties.MoveFirst();
                        while (!rstContactProperties.EOF)
                        {
                            rstContactProperties.Fields[modBrochure.strfIAC_MPEB].Value = true;
                            rstContactProperties.Fields[modBrochure.strfIAC_BROCHURE_ID].Value = vntBrochureId;
                            rstContactProperties.MoveNext();
                        }
                        objLib.SaveRecordset(modBrochure.strtIAC_CONTACT_PROPERTY, rstContactProperties);
                    }

                    rstProperties.MoveNext();
                }

                root.AppendChild(recommendations);
                #endregion
                // END RECOMMENDATIONS

                rstPrimary.Fields[modBrochure.strfIAC_BROCHURE_XML].Value = doc.OuterXml;
                try
                {
                    if (RSysSystem.SystemName.ToUpper() == "ICAC")
                    {
                        CRM.Pivotal.IAC.Sitewire.PDFGenService.PDFGenServiceClient client = new CRM.Pivotal.IAC.Sitewire.PDFGenService.PDFGenServiceClient();
                        string link = client.GetEBrochureId(doc.OuterXml);
                        client.Close(); //http://web02.rental-living.com/eBrochure/default.aspx?id={0}&office=true
                        return link;
                    }
                    else
                    {
                        CRM.Pivotal.IAC.SitewireTest.PDFGenService.PDFGenServiceClient client = new CRM.Pivotal.IAC.SitewireTest.PDFGenService.PDFGenServiceClient();
                        string link = client.GetEBrochureId(doc.OuterXml);
                        client.Close(); //http://tic.rental-living.stage.sitewire.net/eBrochure/default.aspx?id={0}&office=true
                        return link;
                    }
                }
                catch (Exception exc)
                {
                    throw new PivotalApplicationException("PDFGenServiceClient Error:\n" + exc.Message, new Exception(), RSysSystem);
                }

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

    }
}
