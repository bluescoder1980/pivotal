#region Namespaces Used
using System;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;

using CdcSoftware.Pivotal.Applications.Foundation.Server.Data;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;
using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server;
#endregion


namespace CRM.Pivotal.IP
{

    #region Enum Declarations

    /// Enum declarations
    public enum ProcessLeadDestination
    {
        /// <summary>
        /// enum for Opportunity
        /// </summary>
        SaveAsOpportunity = 0,

        /// <summary>
        /// enum for Note
        /// </summary>
        SaveAsNote = 1,

        /// <summary>
        /// enum for Discard
        /// </summary>
        Discard = 2,
    }


    //public enum CommunicationMethod
    //{
    //    NotifyByEmail = 1,
    //    NotifyByFax = 2,
    //    NotifyByMail = 3,
    //    NotifyByCourier = 4,
    //    NotifyByTaxi = 5,
    //}
    #endregion

    /// <summary>
    /// This module powers the basic operations of the Lead form. It also provides
    /// COM functions for Lead Processing, duplicate checking and calculating total product interest amounts.
    /// </summary>
    /// <history>
    /// Revision# Date        Author  Description
    /// 3.8.0.0   5/10/2006   PPhilip Converted to .Net C# code.
    /// </history>   
    public class Lead : IRFormScript, IRAppScript
    {
        #region Lead Class Variables
        
        private IRSystem7 mrsysSystem = null;

        protected IRSystem7 RSysSystem
        {
            get { return mrsysSystem; }
            set { mrsysSystem = value; }
        }

        private object mvntLeadId = DBNull.Value;

        private IRForm mrfrmLead = null;

        protected IRForm LeadForm
        {
            get { return mrfrmLead; }
            set { mrfrmLead = value; }
        }

        private object mvntLeadRst = null;

        protected object LeadRecordset
        {
            get { return mvntLeadRst; }
            set { mvntLeadRst = value; }
        }

        private ILangDict mrldtLangDict = null;

        protected ILangDict RldtLangDict
        {
            get { return mrldtLangDict; }
            set { mrldtLangDict = value; }
        }

        private Connection mcnED = null;

        protected Connection EDConnection
        {
            get { return mcnED; }
            set { mcnED = value; }
        }
        #endregion

        #region AppScript
        // Revision#    Date        Author  Description
        // 5.9.0        6/16/2010   KA      Initial Version
        void IRAppScript.Execute(string methodName, ref object ParameterList)
        {
            try
            {
                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;
                object[] parameterArray = objParam.GetUserDefinedParameterArray();

                switch (methodName)
                {
                    case "DoProcessLeadSimple":
                        //Lead objLead = (Lead)
                        //RSysSystem.ServerScripts["TIC Lead"].CreateInstance();
                        //objLead.DoProcessLeadSimple((parameterArray[0]), ref parameterArray[1], ref parameterArray[2], parameterArray[3]);
                        DoProcessLeadSimple((parameterArray[0]), ref parameterArray[1], ref parameterArray[2], parameterArray[3]);
                        break;
                    default:
                        string message = RSysSystem.GetLDGroup(ErrorsLDGroupData.ErrorsLDGroupName).GetTextSub
                            (ErrorsLDGroupData.MethodNotDefinedLDLookupName, new object[] { methodName })
                            .ToString();
                        throw new PivotalApplicationException(message);
                }

                ParameterList = objParam.SetUserDefinedParameterArray(parameterArray);
                return;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        #endregion

        #region FormScript
        /// <summary>
        /// Loads the current Lead's valid alerts.
        /// </summary>
        /// <param name="pForm">form object reference that holds the layout and format of the data</param>
        /// <param name="recordId">record id to load</param>
        /// <param name="parameterlist">variant array containing extra information from the client</param>
        /// <returns>
        /// Variant array of recordsets containing the data loaded</returns>
        /// <history>
        /// Revision# Date        Author  Description
        /// 3.8.0.0   5/10/2006   PPhilip Converted to .Net C# code.
        /// </history>.
        public virtual object LoadFormData(IRForm pForm, object recordId, ref object parameterlist)
        {
            try
            {
                TransitionPointParameter ocmsParams = (TransitionPointParameter)RSysSystem.
                    ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsParams.ParameterList = parameterlist;

                if (!(ocmsParams.HasValidParameters()))
                    ocmsParams.Construct();

                // Find Alert and return a list of Alert Id's to client thru ParameterList
                Alert ocmsAlert = (Alert)RSysSystem.ServerScripts[modLead.strALERT_APPRULE].CreateInstance();
                Recordset rstAlert = ocmsAlert.FindValidAlerts(recordId, "Lead");

                if (rstAlert != null)
                {
                    if (rstAlert.RecordCount > 0)
                    {
                        ocmsParams.SetUserDefinedParameter(1, rstAlert);
                        object[] userdefinedArray = new object[] { ocmsParams.GetUserDefinedParameter(1) };
                        parameterlist = ocmsParams.ParameterList;
                    }
                    else
                    {
                        ocmsParams.SetUserDefinedParameter(1, null);
                        object[] userdefinedArray = new object[] { ocmsParams.GetUserDefinedParameter(1) };
                        parameterlist = ocmsParams.ParameterList;
                    }
                }
                return pForm.DoLoadFormData(recordId, ref parameterlist);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Deletes a Lead and its secondary records, e.g. alerts, lead groups and product interests.
        /// </summary>
        /// <param name="rfrmCurrent">form object reference that holds the layout and format of the data</param>
        /// <param name="vntLeadId">record id to delete</param>
        /// <param name="vntParameterList">variant array containing extra information from the client</param>
        /// <returns></returns>
        /// <history>
        /// Revision# Date        Author  Description
        /// 3.8.0.0   5/10/2006   PPhilip Converted to .Net C# code.
        /// </history>
        public virtual void DeleteFormData(IRForm rfrmCurrent, object vntLeadId, ref object vntParameterList)
        {
            try
            {
                // If the alerts or activities are linked to other objects like Company, Contact, skip delete for the time
                // being.
                // Delete the alert links
                Alert ocmsAlert = (Alert)RSysSystem.ServerScripts[modLead.strALERT_FORM].CreateInstance();
                Recordset rstAlert = ocmsAlert.FindAllAlerts(vntLeadId, "Lead");
                if (rstAlert.RecordCount > 0)
                {
                    rstAlert.MoveFirst();
                    while (!(rstAlert.EOF))
                    {
                        try
                        {
                            ocmsAlert.DeleteAlerts(rstAlert.Fields[modLead.strfALERT_ID].Value, "Lead");
                        }
                        catch
                        {
                            //ignore exception if alert can't be deleted.
                        }
                        rstAlert.MoveNext();
                    }
                }

                // Delete related Contact NBHD Profiles
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                ContactProfileNeighborhood oHBCPN = (ContactProfileNeighborhood)RSysSystem.ServerScripts[modLead.strsCONTACT_PROFILE_NBHD].CreateInstance();
                IRForm3 rfrmCPN = (IRForm3)RSysSystem.Forms[modLead.strCONTACT_PROF_NBHD];
                Recordset rstCPNs = objLib.GetRecordset(modLead.strqNEIGHBORHOOD_PROFILES_OF_LEADS, 1, vntLeadId, modLead.strfCONTACT_PROFILE_NBHD_ID);
                if (rstCPNs.RecordCount > 0)
                {
                    rstCPNs.MoveFirst();
                    while (!rstCPNs.EOF)
                    {
                        object vntCPN_Id = rstCPNs.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value;
                        object ParameterList = DBNull.Value;
                        oHBCPN.DeleteFormData(rfrmCPN, vntCPN_Id, ref ParameterList);
                        rstCPNs.MoveNext();
                    }
                }

                objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.
                           DataAccessAppServerRuleName].CreateInstance();

                Recordset rstActivities = objLib.GetRecordset(modLead.strqAPPOINTMENTS_WITH_LEAD, 1,
                        vntLeadId, modLead.strfRN_APPOINTMENTS_ID);


                TransitionPointParameter ocmsParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsParams.ParameterList = vntParameterList;
                ocmsParams.Construct();
                ocmsParams.SetUserDefinedParameter(1, "Lead");
                object[] userDefineArray = new object[] { ocmsParams.GetUserDefinedParameter(1) };
                vntParameterList = ocmsParams.SetUserDefinedParameterArray(userDefineArray);

                if (rstActivities.RecordCount > 0)
                {
                    rstActivities.MoveFirst();
                    IRForm rfrmActivity = RSysSystem.Forms[modLead.strGENERAL_ACTIVITY_FORM];
                    while (!rstActivities.EOF)
                    {
                        try
                        {
                            rfrmActivity.DeleteFormData(rstActivities.Fields[modLead.strfRN_APPOINTMENTS_ID].Value, ref vntParameterList);
                        }
                        catch
                        {
                            //ignore exception if activity can't be deleted.
                        }

                        rstActivities.MoveNext();
                    }
                }
                rfrmCurrent.DoDeleteFormData(vntLeadId, ref vntParameterList);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Creates an empty secondary record for data entry for the given secondary
        /// </summary>
        /// <param name="pForm">form object reference that holds the layout and format of the data</param>
        /// <param name="secondaryName">name/id of the secondary segment to add a new record</param>
        /// <param name="parameterlist">variant array containing extra information from the client</param>
        /// <param name="recordSet">secondary recordset to create a new record</param>
        /// <returns></returns>
        /// <history>
        /// Revision# Date        Author  Description
        /// 3.8.0.0   5/10/2006   PPhilip Converted to .Net C# code.
        /// </history>
        public virtual void NewSecondaryData(IRForm pForm, object secondaryName, ref object parameterlist, ref Recordset
            recordSet)
        {
            try
            {
                pForm.DoNewSecondaryData(secondaryName, ref parameterlist, recordSet);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// If user inactivated neighborhood profiles on the secondary, then call cascade inactivate for those
        /// neighborhood profiles.
        /// </summary>
        /// <param name="rfrmCurrent">form object reference that holds the layout and format of the data</param>
        /// <param name="vntLeadRst">variant array of recordsets that contain the data based on the layout and format</param>
        /// <param name="vntParameterList">variant array containing extra information from the client</param>
        /// <returns></returns>
        /// <history>
        /// Revision# Date        Author  Description
        /// 3.8.0.0   5/10/2006   PPhilip Converted to .Net C# code.
        /// 5.9.0     9/2/2010    KA      Added code to call manage interest function
        /// 5.9.1     9/8/2010    Ka      Commented out call to UPdateNBHDType, will not change NBHD Profile Type since it's too
        ///                               Difficult to figure out what the last status is if they opted in and out.
        /// </history>
        public virtual void SaveFormData(IRForm rfrmCurrent, object vntLeadRst, ref object vntParameterList)
        {
            try
            {
                object[] recordArray = (object[])vntLeadRst;
                Recordset rstLead = (Recordset)recordArray[0];
                vntParameterList = null;
                rfrmCurrent.DoSaveFormData(vntLeadRst, ref vntParameterList);

                // Notify partner for the Lead Distribution
                // Cascade inactivate Neighborhood Profile if profile is inactivated.
                if (rfrmCurrent.FormName == modLead.strLEAD_FORM)
                {
                    Recordset rstNP = rfrmCurrent.SecondaryFromVariantArray(vntLeadRst, modLead.strsNEIGHBORHOOD_PROFILE);
                    if (rstNP.RecordCount > 0)
                    {   //KA 9-2-10 commented out oob inactivation code and replaced with custom IP call to function that will handle active/inactive stuff
                        ContactProfileNeighborhood objContactProfNBHD = (ContactProfileNeighborhood)
                        RSysSystem.ServerScripts[modLead.strsCONTACT_PROFILE_NBHD].CreateInstance();
                        objContactProfNBHD.IP_Manage_Interest(rstNP);

                        rstNP.MoveFirst();

                        while (!(rstNP.EOF))
                        {
                            
                            object vntContNBHDProfileId = rstNP.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value;

                            if (Convert.ToBoolean(rstNP.Fields[modLead.strfINACTIVE].Value) == true)
                            {
                                InactivateContactProfileNeighborhood objInactivateNBHDP = (InactivateContactProfileNeighborhood)
                                    RSysSystem.ServerScripts[modLead.strsINACTIVATE_NBHD_PROFILE].CreateInstance();
                                objInactivateNBHDP.InactivateNeighborhoodProfile(vntContNBHDProfileId, null);
                            }

                            else
                            {
                                object vntLeadID = rstNP.Fields["Lead_Id"].Value;
                                object vntNBHDId = rstNP.Fields["Neighborhood_Id"].Value;
                                objContactProfNBHD.NewNeighborhoodProfile(vntNBHDId, System.DBNull.Value, vntLeadID, new object[] { "Skip" });
                            }
                            //KA 9/8/10 commented out call to update type
                            //objContactProfNBHD.UpdateNBHDPType(vntContNBHDProfileId);

                            rstNP.MoveNext();
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
        /// Set default values for new form data.
        /// </summary>
        /// <param name="pForm">form object reference that holds the layout and format of the data</param>
        /// <param name="vntParameterList">variant array containing extra information from the client</param>
        /// <returns>
        /// Variant array of recordsets containing empty data for entry</returns>
        /// <history>
        /// Revision# Date        Author  Description
        /// 3.8.0.0   5/10/2006   PPhilip Converted to .Net C# code.
        /// </history>
        public virtual object NewFormData(IRForm pForm, ref object vntParameterList)
        {
            try
            {
                TransitionPointParameter ocmsParams = (TransitionPointParameter)RSysSystem.
                    ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsParams.ParameterList = vntParameterList;

                if (ocmsParams.HasValidParameters())
                {
                    object vrstLead = pForm.DoNewFormData(ref vntParameterList);
                    object[] recordsetArray = (object[])vrstLead;
                    Recordset rstLead = (Recordset)recordsetArray[0];
                    object[] userDefineArray = ocmsParams.GetUserDefinedParameterArray();
                    userDefineArray = new object[] { ocmsParams.SetDefaultFields(rstLead) };
                    vntParameterList = ocmsParams.SetUserDefinedParameterArray(userDefineArray);
                    return vrstLead;
                }
                else
                    return pForm.DoNewFormData(ref vntParameterList);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Adds the lead record. Create Neighborhood Profile if neighborhood is specified.
        /// </summary>
        /// <param name="pForm">form object reference that holds the layout and format of the data</param>
        /// <param name="Recordsets">variant array of recordsets that contain the data based on the layout and</param>
        /// <param name="parameterlist">variant array containing extra information from the client</param>
        /// <returns>
        /// Record Id of the new primary record added</returns>
        /// <history>
        /// Revision# Date        Author  Description
        /// 3.8.0.0   5/10/2006   PPhilip Converted to .Net C# code.
        /// </history>
        public virtual object AddFormData(IRForm pForm, object Recordsets, ref object parameterlist)
        {
            try
            {
                object[] recordArray = (object[])Recordsets;
                Recordset rstLead = (Recordset)recordArray[0];

                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = parameterlist;
                bool blnCreateNBHDProfile = false;
                
                switch (pForm.FormName)
                {
                    
                    default:
                        {
                            if (objParam.HasValidParameters())
                                if (objParam.UserDefinedParametersNumber > 0)
                                    if (!(Convert.IsDBNull(objParam.GetUserDefinedParameter(1))))
                                        blnCreateNBHDProfile = Convert.ToBoolean(objParam.GetUserDefinedParameter(1));
                            break;
                        }
                }

                
                if (RSysSystem.EqualIds(rstLead.Fields[modLead.strfACCOUNT_MANAGER_ID].Value, DBNull.Value))
                    rstLead.Fields[modLead.strfACCOUNT_MANAGER_ID].Value = DBNull.Value;

                parameterlist = null;
                object vntRecord_Id = pForm.DoAddFormData(Recordsets, ref parameterlist);

                if (blnCreateNBHDProfile && !(Convert.IsDBNull(rstLead.Fields[modLead.strfNP1_NEIGHBORHOOD_ID].Value)))
                {
                    ContactProfileNeighborhood objContactProfileNBHD = (ContactProfileNeighborhood)
                        RSysSystem.ServerScripts[modLead.strsCONTACT_PROFILE_NBHD].CreateInstance();
                    DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                    objContactProfileNBHD.NewNeighborhoodProfile(rstLead.Fields[modLead.strfNP1_NEIGHBORHOOD_ID].Value,
                        System.DBNull.Value, vntRecord_Id, new object[] {administration.CurrentUserRecordId, rstLead.Fields[modLead.strfPRIORITY_CODE_ID].Value,
                        rstLead.Fields[modLead.strfLEAD_SOURCE_ID].Value});
                    
                }
                
                return vntRecord_Id;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Execute methods for the client scripts and external applications.
        /// </summary>
        /// <returns>None.</returns>
        /// <history>
        /// Revision# Date        Author  Description
        /// 3.8.0.0   5/10/2006   PPhilip Converted to .Net C# code.
        /// </history>
        void IRFormScript.Execute(IRForm pForm, string strProcedureName, ref object vntArgument)
        {
            try
            {
                object vntContactId = DBNull.Value;                object vntLeadId = DBNull.Value;                object vntCobuyerId = DBNull.Value;                Recordset rstLead = null;
                object[] userDefineArray = null;

                TransitionPointParameter ocmsParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsParams.ParameterList = vntArgument;
                if (!(ocmsParams.HasValidParameters()))
                    throw new PivotalApplicationException(TypeConvert.ToString(RSysSystem.GetLDGroup("Lead").GetText("Unknown Method")), modLead.intERRNO_UNKNOWN_METHOD_NAME);
                switch (strProcedureName)
                {
                    case "ArchiveLead":
                        // Used by Partnerhub and M1 Integration.
                        vntLeadId = ocmsParams.GetUserDefinedParameter(1);
                        string strStatus = TypeConvert.ToString(ocmsParams.GetUserDefinedParameter(2));
                        ArchiveLead(vntLeadId, strStatus);
                        break;
                    case "UpdateContact":
                        if (ocmsParams.UserDefinedParametersNumber != 2)
                            throw new PivotalApplicationException(TypeConvert.ToString(RSysSystem.
                                GetLDGroup("Lead").GetText("Missing Parameter")), modLead.intERRNO_UNKNOWN_METHOD_NAME);
                        vntContactId = ocmsParams.GetUserDefinedParameter(1);
                        vntLeadId = ocmsParams.GetUserDefinedParameter(2);
                        UpdateContact(vntContactId, vntLeadId);
                        break;
                    case "DoProcessLeadSimple":
                        // Converts the current Lead into a contact record, and possibly a co-buyer too.
                        object vntProcessLeadId = ocmsParams.GetUserDefinedParameter(1);
                        object vntCreateContactId = ocmsParams.GetUserDefinedParameter(2);
                        string strContactType = TypeConvert.ToString(ocmsParams.GetUserDefinedParameter(5));
                        vntCobuyerId = ocmsParams.GetUserDefinedParameter(4);
                        bool blnContactOnly = Convert.ToBoolean(ocmsParams.GetUserDefinedParameter(6));
                        object vntOriginalLeadId = ocmsParams.GetUserDefinedParameter(7);

                        object vntQuoteId = DBNull.Value;                        DoProcessLeadSimple(vntProcessLeadId, ref vntCreateContactId, ref vntQuoteId, vntCobuyerId,
                            strContactType, blnContactOnly);

                        if (!(Convert.IsDBNull(vntOriginalLeadId)))
                            DeleteLead(vntOriginalLeadId);

                        userDefineArray = ocmsParams.GetUserDefinedParameterArray();
                        userDefineArray = new object[3];
                        ocmsParams.SetUserDefinedParameter(1, vntCreateContactId);
                        ocmsParams.SetUserDefinedParameter(2, DateTime.Now);
                        ocmsParams.SetUserDefinedParameter(3, vntCreateContactId);

                        userDefineArray[0] = ocmsParams.GetUserDefinedParameter(1);
                        userDefineArray[1] = ocmsParams.GetUserDefinedParameter(2);
                        userDefineArray[2] = ocmsParams.GetUserDefinedParameter(3);
                        vntArgument = ocmsParams.SetUserDefinedParameterArray(userDefineArray);
                        break;
                    case "CheckDuplicateForNewLead":
                        object vntMatchCode = ocmsParams.GetUserDefinedParameter(1);
                        userDefineArray = ocmsParams.GetUserDefinedParameterArray();
                        CheckDuplicateForNewLead(vntMatchCode, ref userDefineArray);
                        vntArgument = ocmsParams.SetUserDefinedParameterArray(userDefineArray);
                        break;
                    case "UpdateDuplicateContact":
                        vntContactId = ocmsParams.GetUserDefinedParameter(1);
                        rstLead = (Recordset)ocmsParams.GetUserDefinedParameter(2);
                        object vntNeighborhoodId = ocmsParams.GetUserDefinedParameter(3);
                        vntCobuyerId = ocmsParams.GetUserDefinedParameter(4);
                        userDefineArray = ocmsParams.GetUserDefinedParameterArray();
                        UpdateDuplicateContact(vntContactId, rstLead, vntNeighborhoodId, vntCobuyerId, ref userDefineArray);
                        vntArgument = ocmsParams.SetUserDefinedParameterArray(userDefineArray);
                        break;
                    case "MergeContactFromLead":
                        vntLeadId = ocmsParams.GetUserDefinedParameter(1);
                        vntContactId = ocmsParams.GetUserDefinedParameter(2);
                        vntCobuyerId = ocmsParams.GetUserDefinedParameter(3);
                        MergeContactFromLead(vntLeadId, ref vntContactId, ref vntCobuyerId);
                        break;
                    case "LeadDuplicate":
                        id = ocmsParams.GetUserDefinedParameter(4);
                        rstLead = LeadDuplicate(ocmsParams.GetUserDefinedParameter(1), ocmsParams.GetUserDefinedParameter(2),
                            ocmsParams.GetUserDefinedParameter(3));
                        ocmsParams.SetUserDefinedParameter(1, rstLead);
                        userDefineArray = new object[] { ocmsParams.GetUserDefinedParameter(1) };
                        vntArgument = ocmsParams.SetUserDefinedParameterArray(userDefineArray);
                        break;
                    case "LeadContactDuplicate":
                        object vntFirstName = ocmsParams.GetUserDefinedParameter(1);
                        object vntLastName = ocmsParams.GetUserDefinedParameter(2);
                        object vntZipCode = ocmsParams.GetUserDefinedParameter(3);
                        object vntContactType = ocmsParams.GetUserDefinedParameter(4);
                        if (ocmsParams.UserDefinedParametersNumber >= 5)
                        {
                            id = ocmsParams.GetUserDefinedParameter(5);
                            rstLead = LeadDuplicate(vntFirstName, vntLastName, vntZipCode);
                        }
                        RSysSystem.Forms[modLead.strCONTACT_FORM].Execute(modLead.strmCONTACT_DUPLICATE, ref vntArgument);
                        ocmsParams.ParameterList = vntArgument;
                        vntArgument = ocmsParams.SetUserDefinedParameterArray(new object[] { ocmsParams.GetUserDefinedParameter(1), rstLead });
                        break;
                    case "MergeLeadFromLead":
                        MergeLeadFromLead(ocmsParams.GetUserDefinedParameter(1), ocmsParams.GetUserDefinedParameter(2));
                        break;
                    default:
                        throw new PivotalApplicationException(TypeConvert.ToString(RSysSystem.GetLDGroup("Lead").
                            GetText("Unknown Method")), modLead.intERRNO_UNKNOWN_METHOD_NAME);
                }
                this.ClearCache();
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Initializes some global variables.
        /// </summary>
        /// <param name="rsysSystem">Contains the current System Instance Reference</param>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       4/28/2006   PPhilip   Converted to .Net C# code.
        /// </history>
        public virtual void SetSystem(RSystem rsysSystem)
        {
            try
            {
                if (!(RSysSystem == null))
                    return;

                RSysSystem = (IRSystem7)rsysSystem;
                RldtLangDict = RSysSystem.GetLDGroup(modLead.strLEAD_LDGROUP);

                EDConnection = new Connection();
                EDConnection.CursorLocation = (CursorLocationEnum)CursorLocationEnum.adUseClient;
                EDConnection.Open(RSysSystem.EnterpriseString, "", "", -1);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Called by M1 and Partnerhub to find the territory for the current Lead record.
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        /// </history>
        protected virtual Recordset FindTerritory(object vntRecordId, object vntType, object vntAreaCodePhone, object
            vntZipCode, object vntState, object vntCountry)
        {
            try
            {
                TerritoryManagementRule objInstance = (TerritoryManagementRule)RSysSystem.ServerScripts["Territory Mgmt"].CreateInstance();
                Recordset rstTerritory = objInstance.FindTerritory(BusinessEntityIndicator.Lead, DBNull.Value, "Lead",
                        Convert.ToString(vntType), Convert.ToString(vntAreaCodePhone), Convert.ToString(vntZipCode), Convert.ToString(vntState), Convert.ToString(vntCountry));
                return rstTerritory;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine loads the current object into the cache member variable.
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        /// </history>
        protected virtual void LoadCache()
        {
            if (LeadForm == null)
            {
                LeadForm = RSysSystem.Forms[modLead.strLEAD_FORM];
                object ParameterList = null;
                LeadRecordset = LeadForm.LoadFormData(id, ref ParameterList);
            }
        }

        /// <summary>
        /// This subroutine unloads the cache member variable.
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        /// </history>
        protected virtual void ClearCache()
        {
            LeadForm = null;
        }

        /// <summary>
        /// This function merge data from Lead to Contact including neighborhood profiles.
        /// </summary>
        /// Used by duplicate contact check on QuickPath
        /// <param name="vntLeadId">QuickPath Lead record id</param>
        /// <param name="vntContactId">Contact to copy Lead to.</param>
        /// <param name="vntCobuyerId">Co-buyer contact to copy Lead to</param>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        ///               7/20/2006   JH        Merged 3.7 sp1 in.
        /// </history>
        protected virtual void MergeContactFromLead(object vntLeadId, ref object vntContactId, ref object vntCobuyerId)
        {
            try
            {
                if (Convert.IsDBNull(vntLeadId) || (vntLeadId == null))
                    return;

                IRForm rfrmContact = RSysSystem.Forms[modLead.strCONTACT_FORM];
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                DataAccess objDLFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                UIAccess objPLFunctionLib = (UIAccess)RSysSystem.ServerScripts[AppServerRuleData.UIAccessAppServerRuleName].CreateInstance();
                Currency objrCurrency = (Currency)RSysSystem.ServerScripts[AppServerRuleData.CurrencyAppServerRuleName].CreateInstance();

                // Loads lead recordset
                this.id = vntLeadId;
                this.LoadCache();
                object[] recordArray = (object[])LeadRecordset;
                Recordset rstLead = (Recordset)recordArray[0];

                if (!Convert.IsDBNull(vntContactId) && !(vntContactId == null))
                {
                    // Loads contact recordset
                    //object ParameterList = null;
                    //object vntContactRst = rfrmContact.LoadFormData(vntContactId, ref ParameterList);
                    //recordArray = (object[])vntContactRst;
                    Recordset rstContact = objLib.GetRecordset(vntContactId, modLead.strtCONTACT);

                    // Copy primary values from Lead to Contact.
                    AddUpdateContactFields(rstContact, rstLead, "");
                    //ParameterList = null;
                    //rfrmContact.SaveFormData(vntContactRst, ref ParameterList);
                    objLib.SaveRecordset(modLead.strtCONTACT, rstContact);

                    // Move/update neighborhood profile secondary.
                    object vntNeighborhoodId = rstLead.Fields[modLead.strfNP1_NEIGHBORHOOD_ID].Value;
                    //Recordset rstFindDupNP = objDLFunctionLib.GetRecordset(modLead.strqNBHD_PROFILE_FOR_CONTACT_AND_NEIGHBORHOOD,
                    //    2, vntContactId, vntNeighborhoodId, modLead.strfCONTACT_PROFILE_NBHD_ID);

                    Recordset rstQuickPathNP = objDLFunctionLib.GetLinkedRecordset(modLead.strtCONTACT_PROFILE_NEIGHBORHOOD,
                        modLead.strfLEAD_ID, rstLead.Fields[modLead.strfLEAD__ID].Value, modLead.strfLEAD_ID,
                        modLead.strfCONTACT_ID, modLead.strfNEIGHBORHOOD_ID, modLead.strfMARKETING_PROJECT_ID,
                        modLead.strfFIRST_VISIT_DATE, "rn_descriptor");
                    objDLFunctionLib.PermissionIgnored = true;

                    if (rstQuickPathNP.RecordCount > 0)
                    {
                        rstQuickPathNP.MoveFirst();

                        while (!rstQuickPathNP.EOF)
                        {
                            Recordset rstFindDupNP = objDLFunctionLib.GetRecordset(modLead.strqNBHD_PROFILE_FOR_CONTACT_AND_NEIGHBORHOOD,
                                2, vntContactId, rstQuickPathNP.Fields[modLead.strfNEIGHBORHOOD_ID].Value, modLead.strfCONTACT_PROFILE_NBHD_ID);

                            if (rstFindDupNP.RecordCount > 0)
                            {
                                // Update existing profile with info from QuickPath form.
                                ContactProfileNeighborhood objContProfNBHD = (ContactProfileNeighborhood)RSysSystem.ServerScripts[modLead.strsCONTACT_PROFILE_NBHD].CreateInstance();
                                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                                objContProfNBHD.NewNeighborhoodProfile(rstQuickPathNP.Fields[modLead.strfNEIGHBORHOOD_ID].Value, rstContact.Fields[modLead.strfCONTACT_ID].Value,
                                    DBNull.Value, new object[] {administration.CurrentUserRecordId, rstLead.Fields[modLead.strfPRIORITY_CODE_ID].Value,
                                    rstLead.Fields[modLead.strfLEAD_SOURCE_ID].Value, DateTime.Today, rstLead.Fields[modLead.strfNP1_FIRST_VISIT_DATE].Value});
                                objDLFunctionLib.DeleteRecord(rstQuickPathNP.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value, modLead.strtCONTACT_PROFILE_NEIGHBORHOOD);
                            }
                            else
                            {
                                // Not duplicated. Simply move the QuickPath profile over to the Contact record.
                                rstQuickPathNP.Fields[modLead.strfLEAD_ID].Value = DBNull.Value;
                                rstQuickPathNP.Fields[modLead.strfCONTACT_ID].Value = vntContactId;
                            }

                            rstQuickPathNP.MoveNext();
                        }

                        objDLFunctionLib.SaveRecordset(modLead.strtCONTACT_PROFILE_NEIGHBORHOOD, rstQuickPathNP);
                    }

                    // Delete From lead.
                    if (!Convert.IsDBNull(vntCobuyerId) && !(vntCobuyerId == null))
                    {
                        Recordset rstCoBuyerContact = objLib.GetRecordset(vntCobuyerId, modLead.strtCONTACT, modLead.strfADDRESS_1,
                            modLead.strfADDRESS_2, modLead.strfADDRESS_3, modLead.strfCITY, modLead.strfSTATE_,
                            modLead.strfFIRST_NAME, modLead.strfLAST_NAME, modLead.strfTITLE, modLead.strfWORK_PHONE,
                            modLead.strfZIP, modLead.strfEXTENSION, modLead.strfPHONE, modLead.strfCELL,
                            modLead.strfHAS_SAME_ADDRESS_ID, modLead.strfEDUCATION, modLead.strfMARITAL_STATUS);

                        if (!rstCoBuyerContact.EOF && !rstCoBuyerContact.BOF)
                        {
                            rstCoBuyerContact.Fields[modLead.strfADDRESS_1].Value = rstLead.Fields[modLead.strfCO_BUYER_ADDRESS_1].Value;
                            rstCoBuyerContact.Fields[modLead.strfADDRESS_2].Value = rstLead.Fields[modLead.strfCO_BUYER_ADDRESS_2].Value;
                            rstCoBuyerContact.Fields[modLead.strfADDRESS_3].Value = rstLead.Fields[modLead.strfCO_BUYER_ADDRESS_3].Value;
                            rstCoBuyerContact.Fields[modLead.strfCELL].Value = rstLead.Fields[modLead.strfCO_BUYER_CELL].Value;
                            rstCoBuyerContact.Fields[modLead.strfCITY].Value = rstLead.Fields[modLead.strfCO_BUYER_CITY].Value;
                            rstCoBuyerContact.Fields[modLead.strfFIRST_NAME].Value = rstLead.Fields[modLead.strfCO_BUYER_FIRST_NAME].Value;
                            rstCoBuyerContact.Fields[modLead.strfLAST_NAME].Value = rstLead.Fields[modLead.strfCO_BUYER_LAST_NAME].Value;
                            rstCoBuyerContact.Fields[modLead.strfPHONE].Value = rstLead.Fields[modLead.strfCO_BUYER_PHONE].Value;
                            rstCoBuyerContact.Fields[modLead.strfSTATE_].Value = rstLead.Fields[modLead.strfCO_BUYER_STATE].Value;
                            rstCoBuyerContact.Fields[modLead.strfTITLE].Value = rstLead.Fields[modLead.strfCO_BUYER_TITLE].Value;
                            rstCoBuyerContact.Fields[modLead.strfEXTENSION].Value = rstLead.Fields[modLead.strfCO_BUYER_WORK_EXTENSION].Value;
                            rstCoBuyerContact.Fields[modLead.strfWORK_PHONE].Value = rstLead.Fields[modLead.strfCO_BUYER_WORK_PHONE].Value;
                            rstCoBuyerContact.Fields[modLead.strfZIP].Value = rstLead.Fields[modLead.strfCO_BUYER_ZIP].Value;
                            rstCoBuyerContact.Fields[modLead.strfEDUCATION].Value = DBNull.Value;
                            if (TypeConvert.ToBoolean(rstLead.Fields[modLead.strfSAME_AS_BUYER_ADDRESS].Value))
                                rstCoBuyerContact.Fields[modLead.strfHAS_SAME_ADDRESS_ID].Value = vntContactId;

                            if (TypeConvert.ToBoolean(rstLead.Fields[modLead.strfCOBUYER_MARRIED_TO_BUYER].Value))
                                rstCoBuyerContact.Fields[modLead.strfMARITAL_STATUS].Value = "Married";

                            objLib.SaveRecordset(modLead.strtCONTACT, rstCoBuyerContact);
                        }
                    }
                }

                ReAssignActivities(vntLeadId, vntContactId);
                ReAssignAlerts(vntLeadId, vntContactId);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function converts a lead into a contact, copies the activities over, moves neighborhood profiles
        /// over to the new contact, refreshes the contact team members, creates the co-buyer if any, and creates
        /// a visit log if first visit date exists.
        /// </summary>
        /// <param name="vntLeadId">Lead Id</param>
        /// <param name="vntContactId">contact id</param>
        /// <param name="vntQuoteId">quote id</param>
        /// <param name="vntCobuyerId">co buyer id</param>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        /// </history>
        public virtual object DoProcessLeadSimple(object vntLeadId, ref object vntContactId, ref object vntQuoteId,
            object vntCobuyerId)
        {
            try
            {
                return DoProcessLeadSimple(vntLeadId, ref vntContactId, ref vntQuoteId, vntCobuyerId, null);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function converts a lead into a contact, copies the activities over, moves neighborhood profiles
        /// over to the new contact, refreshes the contact team members, creates the co-buyer if any, and creates
        /// a visit log if first visit date exists.
        /// </summary>
        /// <param name="vntLeadId">Lead Id</param>
        /// <param name="vntContactId">contact id</param>
        /// <param name="vntQuoteId">quote id</param>
        /// <param name="vntCobuyerId">co buyer id</param>
        /// <param name="strButtonPressed">button pressed string</param>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        /// </history>
        public virtual object DoProcessLeadSimple(object vntLeadId, ref object vntContactId, ref object vntQuoteId,
            object vntCobuyerId, string strButtonPressed)
        {
            try
            {
                return DoProcessLeadSimple(vntLeadId, ref vntContactId, ref vntQuoteId, vntCobuyerId, null, true);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function converts a lead into a contact, copies the activities over, moves neighborhood profiles
        /// over to the new contact, refreshes the contact team members, creates the co-buyer if any, and creates
        /// a visit log if first visit date exists.
        /// </summary>
        /// <param name="vntLeadId">Lead Id</param>
        /// <param name="vntContactId">contact id</param>
        /// <param name="vntQuoteId">quote id</param>
        /// <param name="vntCobuyerId">co buyer id</param>
        /// <param name="strButtonPressed">button pressed string</param>
        /// <param name="blnContactOnly">boolean for contact only</param>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        /// HB 5.9        2007-09-03  TL        Fixed Issue 65536-17290
        /// 5.9.1         6/4/10      KA        Added call to update Project Registration after contact is created from lead & mapped m1 contact id
        /// </history>
        public virtual object DoProcessLeadSimple(object vntLeadId, ref object vntContactId, ref object vntQuoteId,
            object vntCobuyerId, string strButtonPressed, bool blnContactOnly)
        {
            try
            {
                object vntParams = null;
                bool blnUpdatedContact = false;
                bool blnCreateProfile = true;
                object vntContactNPId = DBNull.Value;                ContactProfileNeighborhood objContactProfileNBHD = null;

                // Set this boolean to make sure that a new contact and all the child records are not created again.
                if (!(Convert.IsDBNull(vntContactId)) && !((vntContactId == null)))
                    blnUpdatedContact = true;
                else
                    blnUpdatedContact = false;

                IRForm rfrmContact = RSysSystem.Forms[modLead.strCONTACT_FORM];
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                UIAccess objPLFunctionLib = (UIAccess)RSysSystem.ServerScripts[AppServerRuleData.UIAccessAppServerRuleName].CreateInstance();
                Currency objrCurrency = (Currency)RSysSystem.ServerScripts[AppServerRuleData.CurrencyAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                if ((strButtonPressed == null))
                    strButtonPressed = null;

                this.id = vntLeadId;
                this.LoadCache();
                object[] recordArray = (object[])LeadRecordset;
                Recordset rstLead = (Recordset)recordArray[0];

                TransitionPointParameter ocmsParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsParams.ParameterList = vntParams;
                ocmsParams.Construct();
                object[] userDefinedArray = ocmsParams.GetUserDefinedParameterArray();

                //KA 6-8-10
                //string[,] arrDefField = new string[1 + 1, 13 + 1];
                string[,] arrDefField = new string[2, 36];
                arrDefField[0, 0] = modLead.strfZIP;
                arrDefField[1, 0] = TypeConvert.ToString(rstLead.Fields[modLead.strfZIP].Value);
                arrDefField[0, 1] = modLead.strfPHONE;
                arrDefField[1, 1] = TypeConvert.ToString(rstLead.Fields[modLead.strfPHONE].Value);
                arrDefField[0, 2] = modLead.strfADDRESS_1;
                arrDefField[1, 2] = TypeConvert.ToString(rstLead.Fields[modLead.strfADDRESS_1].Value);
                arrDefField[0, 3] = modLead.strfADDRESS_2;
                arrDefField[1, 3] = TypeConvert.ToString(rstLead.Fields[modLead.strfADDRESS_2].Value);
                arrDefField[0, 4] = modLead.strfADDRESS_3;
                arrDefField[1, 4] = TypeConvert.ToString(rstLead.Fields[modLead.strfADDRESS_3].Value);
                arrDefField[0, 5] = modLead.strfCITY;
                arrDefField[1, 5] = TypeConvert.ToString(rstLead.Fields[modLead.strfCITY].Value);
                arrDefField[0, 6] = modLead.strfSTATE_;
                arrDefField[1, 6] = TypeConvert.ToString(rstLead.Fields[modLead.strfSTATE_].Value);
                arrDefField[0, 7] = modLead.strfCOUNTRY;
                arrDefField[1, 7] = TypeConvert.ToString(rstLead.Fields[modLead.strfCOUNTRY].Value);
                arrDefField[0, 10] = modLead.strfCOMPANY_ID;
                arrDefField[1, 10] = null;
                arrDefField[0, 11] = modLead.strfWALK_IN_DATE;
                arrDefField[1, 11] = TypeConvert.ToString(rstLead.Fields[modLead.strfNP1_FIRST_VISIT_DATE].Value);
                arrDefField[0, 12] = modLead.strfNEXT_FOLLOW_UP_DATE;
                arrDefField[1, 12] = TypeConvert.ToString(rstLead.Fields[modLead.strfVL1_NEXT_DATE].Value);
                arrDefField[0, 13] = modLead.strfM1_CONTACT_ID;
                arrDefField[1, 13] = TypeConvert.ToString(rstLead.Fields[modLead.strfM1_CONTACT_ID].Value);
                arrDefField[0, 14] = "TIC_Household_Config";
                arrDefField[1, 14] = TypeConvert.ToString(rstLead.Fields["TIC_Household_Config"].Value);
                arrDefField[0, 15] = "TIC_M1_Proj_List_Opt_In";
                arrDefField[1, 15] = TypeConvert.ToString(rstLead.Fields["TIC_M1_Proj_List_Opt_In"].Value);
                arrDefField[0, 16] = "TIC_M1_Proj_List_Opt_Out";
                arrDefField[1, 16] = TypeConvert.ToString(rstLead.Fields["TIC_M1_Proj_List_Opt_Out"].Value);
                arrDefField[0, 17] = "TIC_Move_Timing";
                arrDefField[1, 17] = TypeConvert.ToString(rstLead.Fields["TIC_Move_Timing"].Value);
                arrDefField[0, 18] = "TIC_Preferred_Price_Range_From";
                arrDefField[1, 18] = TypeConvert.ToString(rstLead.Fields["TIC_Preferred_Price_Range_From"].Value);
                arrDefField[0, 19] = "TIC_Preferred_Price_Range_To";
                arrDefField[1, 19] = TypeConvert.ToString(rstLead.Fields["TIC_Preferred_Price_Range_To"].Value);
                arrDefField[0, 20] = "TIC_Password";
                arrDefField[1, 20] = TypeConvert.ToString(rstLead.Fields["TIC_Password"].Value);
                arrDefField[0, 21] = "TIC_Square_Footage_From";
                arrDefField[1, 22] = TypeConvert.ToString(rstLead.Fields["TIC_Square_Footage_From"].Value);
                arrDefField[0, 23] = "TIC_Square_Footage_To";
                arrDefField[1, 23] = TypeConvert.ToString(rstLead.Fields["TIC_Square_Footage_To"].Value);
                arrDefField[0, 24] = "TIC_VIP";
                arrDefField[1, 24] = TypeConvert.ToString(rstLead.Fields["TIC_VIP"].Value);
                arrDefField[0, 25] = "TIC_VIP_Date";
                arrDefField[1, 25] = TypeConvert.ToString(rstLead.Fields["TIC_VIP_Date"].Value);
                arrDefField[0, 26] = "TIC_Work_Zip";
                arrDefField[1, 26] = TypeConvert.ToString(rstLead.Fields["TIC_Work_Zip"].Value);
                arrDefField[0, 27] = "TIC_First_Home";
                arrDefField[1, 27] = TypeConvert.ToString(rstLead.Fields["TIC_First_Home"].Value);
                arrDefField[0, 28] = "TIC_Important_Factor_1";
                arrDefField[1, 28] = TypeConvert.ToString(rstLead.Fields["TIC_Important_Factor_1"].Value);
                arrDefField[0, 29] = "TIC_Important_Factor_2";
                arrDefField[1, 29] = TypeConvert.ToString(rstLead.Fields["TIC_Important_Factor_2"].Value);
                arrDefField[0, 30] = "TIC_Important_Factor_3";
                arrDefField[1, 30] = TypeConvert.ToString(rstLead.Fields["TIC_Important_Factor_3"].Value);
                arrDefField[0, 31] = "TIC_If_Other_Factor_1";
                arrDefField[1, 31] = TypeConvert.ToString(rstLead.Fields["TIC_If_Other_Factor_1"].Value);
                arrDefField[0, 32] = "TIC_If_Other_Factor_2";
                arrDefField[1, 32] = TypeConvert.ToString(rstLead.Fields["TIC_If_Other_Factor_2"].Value);
                arrDefField[0, 33] = "TIC_If_Other_Factor_3";
                arrDefField[1, 33] = TypeConvert.ToString(rstLead.Fields["TIC_If_Other_Factor_3"].Value);
                arrDefField[0, 34] = "Company_Name";
                arrDefField[1, 34] = TypeConvert.ToString(rstLead.Fields["Company_Name"].Value);
                arrDefField[0, 35] = "TIC_HIP_Integration_Id";
                arrDefField[1, 35] = TypeConvert.ToString(rstLead.Fields["TIC_HIP_Integration_Id"].Value);

                userDefinedArray = new object[3];
                userDefinedArray[2] = arrDefField;
                vntParams = ocmsParams.SetUserDefinedParameterArray(userDefinedArray);
                if (!blnUpdatedContact)
                {
                    object vntContactRst = rfrmContact.NewFormData(ref vntParams);
                    recordArray = (object[])vntContactRst;
                    Recordset rstContact = (Recordset)recordArray[0];
                    ocmsParams = null;

                    // Copy values from Lead to Contact.
                    AddUpdateContactFields(rstContact, rstLead, "");
                    object parameterList = null;
                    vntContactId = rfrmContact.AddFormData(vntContactRst, ref parameterList);

                    //ka 6/4/10 update PR records from lead id to contact id
                    IP_Update_Project_Registration(vntLeadId, vntContactId);
                }

                // copy over the activities as well
                Recordset rstAppointments = objLib.GetRecordset(modLead.strqACTIVITIES_FOR_LEAD, 1, vntLeadId, modLead.strfLEAD_ID, modLead.strfCONTACT);
                if (!(rstAppointments.EOF) && !(rstAppointments.BOF))
                {
                    rstAppointments.MoveFirst();
                    while (!(rstAppointments.EOF))
                    {
                        rstAppointments.Fields[modLead.strfLEAD_ID].Value = DBNull.Value;  // added to solve issue 65536-17290
                        rstAppointments.Fields[modLead.strfCONTACT].Value = vntContactId;
                        rstAppointments.MoveNext();
                    }
                    objLib.SaveRecordset(modLead.strRN_APPOINTMENTS_TABLE, rstAppointments);
                }


                // copy over the alerts as well
                Recordset rstAlerts = objLib.GetRecordset(modLead.strqALERTS_WITH_LEAD, 1, vntLeadId, modLead.strfCONTACT_ID,
                    modLead.strfLEAD_ID);
                if (!(rstAlerts.EOF) && !(rstAlerts.BOF))
                {
                    rstAlerts.MoveFirst();
                    while (!(rstAlerts.EOF))
                    {
                        rstAlerts.Fields[modLead.strfCONTACT_ID].Value = vntContactId;
                        rstAlerts.Fields[modLead.strfLEAD_ID].Value = DBNull.Value;
                        rstAlerts.MoveNext();
                    }
                    objLib.SaveRecordset(modLead.strtALERT, rstAlerts);
                }

                if (!((vntCobuyerId is Array)))
                {
                    // If the Lead co-buyer First and Last Name fields are entered then create a co-buyer
                    // record and copy over these fields:
                    if (TypeConvert.ToString(rstLead.Fields[modLead.strfCO_BUYER_FIRST_NAME].Value) != "" && TypeConvert.ToString(rstLead.Fields[modLead.strfCO_BUYER_LAST_NAME].Value) != "")
                    {
                        object vntCoBuyerRst = rfrmContact.NewFormData(ref vntParams);
                        recordArray = (object[])vntCoBuyerRst;
                        Recordset rstCoBuyerContact = (Recordset)recordArray[0];
                        object parameterList = null;

                        // Copy values from Lead to Contact.
                        AddUpdateContactFields(rstCoBuyerContact, rstLead, "Cobuyer");
                        object vntCoBuyerContactId = rfrmContact.AddFormData(vntCoBuyerRst, ref parameterList);

                        rstCoBuyerContact = objLib.GetRecordset(vntCoBuyerContactId, modLead.strtCONTACT, modLead.strfADDRESS_1,
                            modLead.strfADDRESS_2, modLead.strfADDRESS_3, modLead.strfCITY, modLead.strfSTATE_,
                            modLead.strfFIRST_NAME, modLead.strfLAST_NAME, modLead.strfTITLE, modLead.strfWORK_PHONE,
                            modLead.strfZIP, modLead.strfEXTENSION, modLead.strfPHONE, modLead.strfCELL,
                            modLead.strfHAS_SAME_ADDRESS_ID, modLead.strfEDUCATION);
                        if (!(rstCoBuyerContact.EOF) && !(rstCoBuyerContact.BOF))
                        {
                            rstCoBuyerContact.Fields[modLead.strfADDRESS_1].Value = rstLead.Fields[modLead.strfCO_BUYER_ADDRESS_1].Value;
                            rstCoBuyerContact.Fields[modLead.strfADDRESS_2].Value = rstLead.Fields[modLead.strfCO_BUYER_ADDRESS_2].Value;
                            rstCoBuyerContact.Fields[modLead.strfADDRESS_3].Value = rstLead.Fields[modLead.strfCO_BUYER_ADDRESS_3].Value;
                            rstCoBuyerContact.Fields[modLead.strfCELL].Value = rstLead.Fields[modLead.strfCO_BUYER_CELL].Value;
                            rstCoBuyerContact.Fields[modLead.strfZIP].Value = rstLead.Fields[modLead.strfCO_BUYER_ZIP].Value;
                            rstCoBuyerContact.Fields[modLead.strfSTATE_].Value = rstLead.Fields[modLead.strfCO_BUYER_STATE].Value;
                            rstCoBuyerContact.Fields[modLead.strfCITY].Value = rstLead.Fields[modLead.strfCO_BUYER_CITY].Value;
                            rstCoBuyerContact.Fields[modLead.strfFIRST_NAME].Value = rstLead.Fields[modLead.strfCO_BUYER_FIRST_NAME].Value;
                            rstCoBuyerContact.Fields[modLead.strfLAST_NAME].Value = rstLead.Fields[modLead.strfCO_BUYER_LAST_NAME].Value;
                            rstCoBuyerContact.Fields[modLead.strfPHONE].Value = rstLead.Fields[modLead.strfCO_BUYER_PHONE].Value;
                            rstCoBuyerContact.Fields[modLead.strfTITLE].Value = rstLead.Fields[modLead.strfCO_BUYER_TITLE].Value;
                            rstCoBuyerContact.Fields[modLead.strfEXTENSION].Value = rstLead.Fields[modLead.strfCO_BUYER_WORK_EXTENSION].Value;
                            rstCoBuyerContact.Fields[modLead.strfWORK_PHONE].Value = rstLead.Fields[modLead.strfCO_BUYER_WORK_PHONE].Value;
                            if (Convert.ToBoolean(rstLead.Fields[modLead.strfSAME_AS_BUYER_ADDRESS].Value))
                                rstCoBuyerContact.Fields[modLead.strfHAS_SAME_ADDRESS_ID].Value = vntContactId;

                            objLib.SaveRecordset(modLead.strtCONTACT, rstCoBuyerContact);
                        }
                        vntCoBuyerContactId = rstCoBuyerContact.Fields[modLead.strfCONTACT_ID].Value;

                        Recordset rstCoBuyer = objLib.GetNewRecordset(modLead.strtCONTACT_COBUYER, modLead.strfCONTACT_ID,
                            modLead.strfCO_BUYER_CONTACT_ID, modLead.strfCOBUYER_MARRIED_TO_BUYER);
                        rstCoBuyer.AddNew(modLead.strfCO_BUYER_CONTACT_ID, DBNull.Value);
                        rstCoBuyer.Fields[modLead.strfCONTACT_ID].Value = vntContactId;
                        rstCoBuyer.Fields[modLead.strfCO_BUYER_CONTACT_ID].Value = vntCoBuyerContactId;
                        rstCoBuyer.Fields[modLead.strfCOBUYER_MARRIED_TO_BUYER].Value = rstLead.Fields[modLead.strfCOBUYER_MARRIED_TO_BUYER].Value;
                        objLib.SaveRecordset(modLead.strtCONTACT_COBUYER, rstCoBuyer);
                    }
                }

                if (!blnUpdatedContact)
                {
                    // update neighborhood profile
                    DataAccess objDLFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    Recordset rstContactNBHDP = objDLFunctionLib.GetLinkedRecordset(modLead.strtCONTACT_PROFILE_NEIGHBORHOOD,
                        modLead.strfLEAD_ID, vntLeadId, modLead.strfLEAD_ID, modLead.strfCONTACT_ID,
                        modLead.strfNEIGHBORHOOD_ID);

                    if (rstContactNBHDP.RecordCount > 0)
                        rstContactNBHDP.MoveFirst();

                    while (!(rstContactNBHDP.EOF))
                    {
                        // modify neighborhood profile
                        rstContactNBHDP.Fields[modLead.strfCONTACT_ID].Value = vntContactId;
                        rstContactNBHDP.Fields[modLead.strfLEAD_ID].Value = DBNull.Value;
                        rstContactNBHDP.MoveNext();
                    }
                    objDLFunctionLib.SaveRecordset(modLead.strtCONTACT_PROFILE_NEIGHBORHOOD, rstContactNBHDP);

                    Recordset rstNBHDPTeam = objDLFunctionLib.GetLinkedRecordset(modLead.strtCONTACT_TEAM_MEMBER, modLead.strfLEAD_ID,
                        vntLeadId, modLead.strfTICKLE_COUNTER, modLead.strfCONTACT_TEAM_MEMBER_ID);
                    if (rstNBHDPTeam.RecordCount > 0)
                    {
                        object vntCounter = null;
                        rstNBHDPTeam.MoveFirst();
                        while (!(rstNBHDPTeam.EOF))
                        {
                            // modify Contact Team Member to refresh table-level formulas.
                            if (Convert.IsDBNull(rstNBHDPTeam.Fields[modLead.strfTICKLE_COUNTER].Value))
                                vntCounter = null;
                            else
                                vntCounter = rstNBHDPTeam.Fields[modLead.strfTICKLE_COUNTER].Value;
                            rstNBHDPTeam.Fields[modLead.strfTICKLE_COUNTER].Value = Convert.ToDouble(vntCounter)
                                + 1.0;
                            rstNBHDPTeam.MoveNext();
                        }
                        objDLFunctionLib.SaveRecordset(modLead.strtCONTACT_TEAM_MEMBER, rstNBHDPTeam);
                    }

                    object vntLeadNbhdId = rstLead.Fields[modLead.strfNP1_NEIGHBORHOOD_ID].Value;

                    // create Contact Neighborhood Profile only if there is a neighborhood to link to
                    // and the neighborhood is not existed in the contact profile neighborhood
                    if (!(Convert.IsDBNull(vntLeadNbhdId)))
                    {
                        objContactProfileNBHD = (ContactProfileNeighborhood)
                            RSysSystem.ServerScripts[modLead.strsCONTACT_PROFILE_NBHD].CreateInstance();

                        if (rstContactNBHDP.RecordCount > 0)
                        {
                            rstContactNBHDP.MoveFirst();
                        }
                        while (!(rstContactNBHDP.EOF))
                        {
                            if (RSysSystem.EqualIds(rstContactNBHDP.Fields[modLead.strfNEIGHBORHOOD_ID].Value, vntLeadNbhdId))
                            {
                                blnCreateProfile = false;
                                vntContactNPId = rstContactNBHDP.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value;
                                break;
                            }
                            rstContactNBHDP.MoveNext();
                        }

                        if (blnCreateProfile)
                        {
                            // Creating the Contact Neighbourhood Profile and the TrafficSource
                            Recordset rstContactNP = objLib.GetNewRecordset(modLead.strtCONTACT_PROFILE_NEIGHBORHOOD, modLead.strfCONTACT_ID,
                                modLead.strfNEIGHBORHOOD_ID, modLead.strfPROSPECT_RATING, modLead.strfLEAD_DATE,
                                modLead.strfTRAFFIC_SOURCE, modLead.strfFIRST_VISIT_DATE, modLead.strfDIVISION_ID,
                                modLead.strfPRIORITY_CODE_ID, modLead.strfQUOTE_DATE, modLead.strfMARKETING_PROJECT_ID);

                            rstContactNP.AddNew(modLead.strfCONTACT_PROFILE_NBHD_ID, DBNull.Value);
                            rstContactNP.Fields[modLead.strfCONTACT_ID].Value = vntContactId;
                            rstContactNP.Fields[modLead.strfDIVISION_ID].Value = RSysSystem.Tables[modLead.strtNEIGHBORHOOD].Fields[modLead.strfDIVISION_ID].Index(rstLead.Fields[modLead.strfNP1_NEIGHBORHOOD_ID].Value);
                            rstContactNP.Fields[modLead.strfLEAD_DATE].Value = DateTime.Today;
                            rstContactNP.Fields[modLead.strfNEIGHBORHOOD_ID].Value = rstLead.Fields[modLead.strfNP1_NEIGHBORHOOD_ID].Value;
                            rstContactNP.Fields[modLead.strfPROSPECT_RATING].Value = rstLead.Fields[modLead.strfNP1_PROSPECT_RATING].Value;
                            rstContactNP.Fields[modLead.strfFIRST_VISIT_DATE].Value = rstLead.Fields[modLead.strfNP1_FIRST_VISIT_DATE].Value;
                            rstContactNP.Fields[modLead.strfTRAFFIC_SOURCE].Value = RSysSystem.Tables[modLead.strtMARKETING_PROJECT].Fields[modLead.strfMARKETING_PROJECT_NAME].Index(rstLead.Fields[modLead.strfLEAD_SOURCE_ID].Value);
                            rstContactNP.Fields[modLead.strfLEAD_DATE].Value = DateTime.Today;
                            rstContactNP.Fields[modLead.strfPRIORITY_CODE_ID].Value = rstLead.Fields[modLead.strfPRIORITY_CODE_ID].Value;
                            rstContactNP.Fields[modLead.strfMARKETING_PROJECT_ID].Value = rstLead.Fields[modLead.strfLEAD_SOURCE_ID].Value;
                            if (!blnContactOnly)
                                rstContactNP.Fields[modLead.strfQUOTE_DATE].Value = DateTime.Today;

                            objLib.SaveRecordset(modLead.strtCONTACT_PROFILE_NEIGHBORHOOD, rstContactNP);
                            objContactProfileNBHD.UpdateNBHDPType(rstContactNP.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value);

                            // Add current employee to NBHD Profile sales team - fpoulsen 06/13/2005
                            DataAccess objServerShareLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                            Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                            objContactProfileNBHD.UpdateNBHDProfieTeam(rstContactNP.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value,
                                administration.CurrentUserRecordId);

                            vntContactNPId = rstContactNP.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value;
                            Recordset rstTrafficSource = objLib.GetNewRecordset(modLead.strtTRAFFIC_SOURCE, modLead.strfMARKETING_PROJECT_ID,
                                modLead.strfCONTACT_PROFILE_NBHD_ID);
                            rstTrafficSource.AddNew(modLead.strfMARKETING_PROJECT_ID, DBNull.Value);
                            rstTrafficSource.Fields[modLead.strfMARKETING_PROJECT_ID].Value = rstLead.Fields[modLead.strfLEAD_SOURCE_ID].Value;
                            rstTrafficSource.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value = vntContactNPId;
                            objLib.SaveRecordset(modLead.strtTRAFFIC_SOURCE, rstTrafficSource);

                        }
                    }
                    if ((vntContactNPId is Array) && !(Convert.IsDBNull(rstLead.Fields[modLead.strfNP1_FIRST_VISIT_DATE].Value))
                        && (rstLead.Fields[modLead.strfNP1_FIRST_VISIT_DATE].Value != null))
                    {
                        // create the visit log only if the first visit date is not null

                        Recordset rstVisitLog = objLib.GetNewRecordset(modLead.strRN_APPOINTMENTS_TABLE, modLead.strfCONTACT,
                            modLead.strfNEIGHBORHOOD_ID, modLead.strfCONTACT_PROFILE_NBHD_ID, modLead.strfACTIVITY_TYPE,
                            modLead.strfNEXT_FOLLOW_UP_DATE, modLead.strfVISIT_NUMBER, modLead.strfRN_EMPLOYEE_ID,
                            modLead.strfACTIVITY_COMPLETE, modLead.strfACTIVITY_COMPLETED_DATE, modLead.strfNOTES,
                            modLead.strfAPPT_DATE, modLead.strfRN_EMPLOYEE_ID, modLead.strfASSIGNED_BY,
                            modLead.strfCONTACT, modLead.strfSTART_TIME, modLead.strfAPPT_DESCRIPTION);
                        rstVisitLog.AddNew(modLead.strfRN_APPOINTMENTS_ID, DBNull.Value);
                        rstVisitLog.Fields[modLead.strfACTIVITY_TYPE].Value = modLead.lngACTIVITY_TYPE_VISITLOG;
                        rstVisitLog.Fields[modLead.strfNEIGHBORHOOD_ID].Value = rstLead.Fields[modLead.strfNP1_NEIGHBORHOOD_ID].Value;
                        rstVisitLog.Fields[modLead.strfNEXT_FOLLOW_UP_DATE].Value = rstLead.Fields[modLead.strfVL1_NEXT_DATE].Value;
                        rstVisitLog.Fields[modLead.strfRN_EMPLOYEE_ID].Value = rstLead.Fields[modLead.strfVL1_EMPLOYEE_ID].Value;
                        rstVisitLog.Fields[modLead.strfASSIGNED_BY].Value = rstLead.Fields[modLead.strfVL1_EMPLOYEE_ID].Value;
                        rstVisitLog.Fields[modLead.strfACTIVITY_COMPLETE].Value = Convert.IsDBNull(rstVisitLog.Fields[modLead.strfNEXT_FOLLOW_UP_DATE].Value);
                        if (Convert.ToBoolean(rstVisitLog.Fields[modLead.strfACTIVITY_COMPLETE].Value))
                            rstVisitLog.Fields[modLead.strfACTIVITY_COMPLETED_DATE].Value = DateTime.Now;

                        rstVisitLog.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value = vntContactNPId;
                        rstVisitLog.Fields[modLead.strfNOTES].Value = rstLead.Fields[modLead.strfVL1_VISIT_COMMENTS].Value;
                        if ((rstLead.Fields[modLead.strfVL1_VISIT_DATE].Value == null) || Convert.IsDBNull(rstLead.Fields[modLead.strfVL1_VISIT_DATE].Value))
                            rstVisitLog.Fields[modLead.strfAPPT_DATE].Value = DateTime.Today;
                        else
                            rstVisitLog.Fields[modLead.strfAPPT_DATE].Value = rstLead.Fields[modLead.strfVL1_VISIT_DATE].Value;

                        rstVisitLog.Fields[modLead.strfSTART_TIME].Value = DateTime.Now;
                        rstVisitLog.Fields[modLead.strfCONTACT].Value = vntContactId;
                        string strContactRnDescriptor = TypeConvert.ToString(RSysSystem.Tables[modLead.strtCONTACT].Fields[modLead.strfFULL_NAME].Index(vntContactId));
                        string strNeighborhoodName = TypeConvert.ToString(RSysSystem.Tables[modLead.strtNEIGHBORHOOD].Fields[modLead.strfNAME].Index(rstVisitLog.Fields[modLead.strfNEIGHBORHOOD_ID].Value));
                        rstVisitLog.Fields[modLead.strfAPPT_DESCRIPTION].Value = "Visit Log: " + strContactRnDescriptor + " at " +
                            strNeighborhoodName;
                        objLib.SaveRecordset(modLead.strRN_APPOINTMENTS_TABLE, rstVisitLog);

                    }
                }

                if (!blnContactOnly)
                {
                    if (!(Convert.IsDBNull(vntContactNPId)) && !((vntContactNPId == null)))
                    {
                        // update NBHD Profile type
                        objContactProfileNBHD.UpdateNBHDPType(vntContactNPId);
                    }
                }

                ArchiveLead(vntLeadId, objPLFunctionLib.GetComboChoiceText(modLead.strcINTERNAL, modLead.strfSTATUS_, modLead.strtARCHIVE_LEAD));
                return null;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Archives lead
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        /// 5.9.1         6/4/10      KA        Fixed arching errors
        public virtual bool ArchiveLead(object vntLeadId, string strStatus)
        {
            try
            {
                // Building a new recordset for the Arch_Lead table and Lead table
                IRFields objFields = RSysSystem.Tables[modLead.strtLEAD_].Fields;
                object[] arrFields = new object[objFields.Count - 8 + 1];
                int i = 0;
                string fields = string.Empty;
                IRField5 objField = null;
                foreach (IRField5 __each1 in objFields)
                {
                    objField = __each1;
                    // ignore id and Rn fields
                    if (objField.FieldName != modLead.strfLEAD__ID && ((objField.FieldName).Substring(0, 3)).ToUpper() != "RN_")
                    {
                        arrFields[i] = objField.FieldName;
                        i = i + 1;
                    }
                }

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.
                       DataAccessAppServerRuleName].CreateInstance();

                Recordset rstLead = objLib.GetRecordset(vntLeadId, modLead.strtLEAD_, arrFields);

                objFields = RSysSystem.Tables[modLead.strtARCHIVE_LEAD].Fields;
                arrFields = new object[objFields.Count - 7 + 1];
                i = 0;
                fields = string.Empty;
                foreach (IRField5 __each2 in objFields)
                {
                    objField = __each2;
                    // ignore id and Rn fields
                    if (objField.FieldName != modLead.strfARCH_LEAD_ID && ((objField.FieldName).Substring(0, 3)).ToUpper() != "RN_")
                    {
                        arrFields[i] = objField.FieldName;
                        i = (i + 1);
                    }
                }

                objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.
                      DataAccessAppServerRuleName].CreateInstance();
                //Recordset rstArchLead = objLib.GetNewRecordset(modLead.strtARCHIVE_LEAD, fields);
                Recordset rstArchLead = objLib.GetRecordset(modLead.strtARCHIVE_LEAD, arrFields);
                rstArchLead.AddNew(modLead.strfARCHIEVE_LEAD_ID, DBNull.Value);

                rstArchLead.Fields[modLead.strfSTATUS_].Value = strStatus;
                i = arrFields.GetLowerBound(0);
                int j = arrFields.GetUpperBound(0);
                //KA 6/4/10 redoing for statement since i never changes, the fields vals are not being populated in arch lead table
                //for (int k = i; k < j - 1; k++)
                for ( i=0; i <= j ; i++)
                {
                    // There is no Status_, Soundex fields in Lead
                    //KA 6-4-10 changed i to j since is is the low bound array i!=0 is always false since i will always be zero
                    if (j != 0 && (TypeConvert.ToString(arrFields[i]) != modLead.strfSTATUS_ && TypeConvert.ToString(arrFields[i])
                        != modLead.strfARCH_LEAD_SOUNDEX))
                    {
                        if (TypeConvert.ToString(arrFields[i]) == modLead.strfAGE)
                        {
                            // calculate the age
                            rstArchLead.Fields[modLead.strfAGE].Value = DateTime.Today.ToOADate() - TypeConvert.ToDateTime(rstLead.Fields[modLead.strfDATE_ENTERED].Value).ToOADate();
                        }
                        else
                        {
                            rstArchLead.Fields[arrFields[i]].Value = rstLead.Fields[arrFields[i]].Value;
                        }
                        System.Diagnostics.Debug.WriteLine(rstLead.Fields[i].Name);
                    }
                }
                objLib.SaveRecordset(modLead.strtARCHIVE_LEAD, rstArchLead);

                // Delete the current lead object
                if (LeadForm == null)
                    LeadForm = RSysSystem.Forms[modLead.strLEAD_FORM];

                object ParameterList = null;
                LeadForm.DeleteFormData(vntLeadId, ref ParameterList);

                return true;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function will check for duplicate contact records.
        /// </summary>
        /// <param name="vntLeadId">Lead Id</param>
        /// <param name="vntfParameter">Parameter passed from IRFormScript_Execute</param>
        /// <returns>
        /// vntfParameter - array containing the duplicate contact records</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/10/2006   PPhilip     Converted to .Net C# code.
        /// </history>
        public virtual void CheckDuplicateForProcessLead(object vntLeadId, ref object vntfParameter)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset recLead = objLib.GetRecordset(vntLeadId, modLead.strtLEAD_, modLead.strfLEAD_NAME_SOUNDEX,
                    modLead.strfFIRST_NAME, modLead.strfLAST_NAME, modLead.strfCO_BUYER_FIRST_NAME, modLead.strfCO_BUYER_LAST_NAME);
                object vntLeadSoundex = recLead.Fields[modLead.strfLEAD_NAME_SOUNDEX].Value;
                string strFirstName = TypeConvert.ToString(recLead.Fields[modLead.strfFIRST_NAME].Value);
                string strLastName = TypeConvert.ToString(recLead.Fields[modLead.strfLAST_NAME].Value);
                object vntCoBuyerFirstName = recLead.Fields[modLead.strfCO_BUYER_FIRST_NAME].Value;
                object vntCoBuyerLastName = recLead.Fields[modLead.strfCO_BUYER_LAST_NAME].Value;

                Recordset rstContact = null;
                Recordset rstCoBuyer = null;
                if (!(Convert.IsDBNull(vntLeadSoundex)))
                    // Set rstContact = objLib.GetRecordset("PA: Contacts with Soundex ?", strtCONTACT, 1, vntLeadSoundex, strfCONTACT_ID)
                    rstContact = objLib.GetRecordset(modLead.strqCHECK_DUPLICATE_CONTACTS, 2, strFirstName, strLastName,
                        modLead.strfCONTACT_ID);
                rstCoBuyer = objLib.GetRecordset(modLead.strqCHECK_DUPLICATE_CONTACTS, 2, vntCoBuyerFirstName,
                    vntCoBuyerLastName, modLead.strfCONTACT_ID);

                vntfParameter = new object[] { rstContact.Fields, rstCoBuyer.Fields };
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Check duplicate records for lead.
        /// </summary>
        /// <param name="vntMatchCode">Match code</param>
        /// <param name="vntfParameter">Parameter passed from IRFormScript_Execute</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#  Date        Author    Note
        /// 3.8.0.0    5/10/2006   PPhilip   Converted to .Net C# code.
        /// </history>
        public virtual void CheckDuplicateForNewLead(object vntMatchCode, ref object[] vntfParameter)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstLead = objLib.GetRecordset("HB: Leads with Match Code?", 1, vntMatchCode, modLead.strfLEAD__ID);
                vntfParameter = new object[] { rstLead };
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Create quote from a Lead
        /// </summary>
        /// <returns>
        /// a variant containing the newly created quote id</returns>
        /// <history>
        /// Revision#    Date        Author   Description
        /// 3.8.0.0      5/10/2006   PPhilip  Converted to .Net C# code.
        /// </history>
        public virtual object CreateQuoteForLead(object vntContactId, Recordset rstLead, string strType)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // create a new quote
                Recordset rstQuoteNew = objLib.GetNewRecordset(modLead.strtOPPORTUNITY, modLead.strfQUOTE_CREATE_DATE,
                    modLead.strfACTUAL_DECISION_DATE, modLead.strfCONTACT_ID, modLead.strfSTATUS, modLead.strfPIPELINE_STAGE,
                    modLead.strfNEIGHBORHOOD_ID, modLead.strfACCOUNT_MANAGER_ID);
                rstQuoteNew.AddNew(modLead.strfOPPORTUNITY_NAME, DBNull.Value);
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                rstQuoteNew.Fields[modLead.strfACCOUNT_MANAGER_ID].Value = administration.CurrentUserRecordId;
                rstQuoteNew.Fields[modLead.strfQUOTE_CREATE_DATE].Value = DateTime.Now;
                rstQuoteNew.Fields[modLead.strfACTUAL_DECISION_DATE].Value = DBNull.Value;
                rstQuoteNew.Fields[modLead.strfCONTACT_ID].Value = vntContactId;

                const string strQUOTE = "QUOTE";
                const string strCONTRACT = "CONTRACT";

                if (strType.ToUpper() == strQUOTE)
                {
                    rstQuoteNew.Fields[modLead.strfSTATUS].Value = modLead.strsIN_PROGRESS;
                    rstQuoteNew.Fields[modLead.strfPIPELINE_STAGE].Value = modLead.strsQUOTE;
                }
                else if (strType.ToUpper() == strCONTRACT)
                {
                    rstQuoteNew.Fields[modLead.strfSTATUS].Value = modLead.strsON_HOLD;
                    rstQuoteNew.Fields[modLead.strfPIPELINE_STAGE].Value = modLead.strsCONTRACT;
                    rstQuoteNew.Fields[modLead.strfACTUAL_DECISION_DATE].Value = DateTime.Now;
                }

                rstQuoteNew.Fields[modLead.strfNEIGHBORHOOD_ID].Value = rstLead.Fields[modLead.strfNP1_NEIGHBORHOOD_ID].Value;

                objLib.SaveRecordset(modLead.strtOPPORTUNITY, rstQuoteNew);
                return rstQuoteNew.Fields[modLead.strfOPPORTUNITY_ID].Value;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function creates an opportunity from a lead. It also converts the multiple
        /// product interests into quotes.
        /// Assumptions:
        /// Effects:
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        /// </history>
        protected virtual object CreateOpportunity(object vntLeadId, Recordset rstCompany, Recordset rstContact, object
            vntResellerId)
        {
            try
            {
                object vntParams = null;
                object vntOppParams = null;

                this.id = vntLeadId;
                this.LoadCache();
                object[] recordsArray = (object[])LeadRecordset;
                Recordset rstLead = (Recordset)recordsArray[0];

                Currency objrCurrency = (Currency)RSysSystem.ServerScripts[AppServerRuleData.CurrencyAppServerRuleName].CreateInstance();

                // Create the new opportunity
                IRForm rfrmOpp = RSysSystem.Forms[modLead.strOPP_FORM];

                TransitionPointParameter objTransitParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objTransitParams.ParameterList = vntParams;
                objTransitParams.Construct();
                object[] userDefinedArray = objTransitParams.GetUserDefinedParameterArray();

                object[,] arrDefaultFields = new object[1 + 1, 6 + 1];
                arrDefaultFields[0, 0] = modLead.strfCONTACT_ID;
                arrDefaultFields[1, 0] = rstContact.Fields[modLead.strfCONTACT_ID].Value;
                arrDefaultFields[0, 1] = modLead.strfTERRITORY_ID;
                arrDefaultFields[1, 1] = DBNull.Value;
                arrDefaultFields[0, 2] = modLead.strfOPPORTUNITY_NAME;
                arrDefaultFields[1, 2] = "Please fill in";
                arrDefaultFields[0, 3] = modLead.strfACCOUNT_MANAGER_OVERRIDE;
                arrDefaultFields[1, 3] = false;
                arrDefaultFields[0, 4] = modLead.strfPRODUCT_TYPE_INTEREST;
                arrDefaultFields[1, 4] = rstLead.Fields[modLead.strfPRODUCT_INTEREST_TYPE].Value;
                arrDefaultFields[0, 5] = modLead.strfACCOUNT_MANAGER_ID;
                arrDefaultFields[1, 5] = DBNull.Value;
                arrDefaultFields[0, 6] = modLead.strfOVERRIDE_CALC_PROBABILITY;
                arrDefaultFields[1, 6] = true;
                userDefinedArray = new object[3];
                userDefinedArray[2] = arrDefaultFields;
                vntParams = objTransitParams.SetUserDefinedParameterArray(userDefinedArray);
                objTransitParams = null;

                object vntOppRst = rfrmOpp.NewFormData(ref vntParams);
                object[] recordArray = (object[])vntOppRst;
                Recordset rstOpp = (Recordset)recordArray[0];

                // Copy values from Contact
                rstOpp.Fields[modLead.strfCONTACT_ID].Value = rstContact.Fields[modLead.strfCONTACT_ID].Value;
                if (!(Convert.IsDBNull(rstContact.Fields[modLead.strfCOMPANY_ID].Value)))
                    rstOpp.Fields[modLead.strfCOMPANY_ID].Value = rstContact.Fields[modLead.strfCOMPANY_ID].Value;
                else
                    rstOpp.Fields[modLead.strfCOMPANY_ID].Value = rstCompany.Fields[modLead.strfCOMPANY_ID].Value;

                rstOpp.Fields[modLead.strfCURRENCY_ID].Value = rstContact.Fields[modLead.strfCURRENCY_ID].Value;
                rstOpp.Fields[modLead.strfDELTA_CURRENCY_ID].Value = rstContact.Fields[modLead.strfCURRENCY_ID].Value;

                string strPipelineStage = TypeConvert.ToString(RSysSystem.GetLDGroup(modLead.strLEAD_LDGROUP).GetText("Pipeline Needs Analysis"));
                // "2 - Needs Analysis"
                rstOpp.Fields[modLead.strfPIPELINE_STAGE].Value = strPipelineStage;

                // Copy values from Lead to Opportunity
                if (TypeConvert.ToString(rstLead.Fields[modLead.strfQUALITY].Value) == "Hot")
                {
                    rstOpp.Fields[modLead.strfQUALITY].Value = 0;
                }
                else if (TypeConvert.ToString(rstLead.Fields[modLead.strfQUALITY].Value) == "Medium")
                {
                    rstOpp.Fields[modLead.strfQUALITY].Value = 1;
                }
                else if (TypeConvert.ToString(rstLead.Fields[modLead.strfQUALITY].Value) == "Cold")
                {
                    rstOpp.Fields[modLead.strfQUALITY].Value = 2;
                }

                rstOpp.Fields[modLead.strfLEAD_DATE].Value = rstLead.Fields[modLead.strfRN_CREATE_DATE].Value;
                rstOpp.Fields[modLead.strfEXPECTED_DECISION_DATE].Value = rstLead.Fields[modLead.strfDECISION_DATE].Value;
                rstOpp.Fields[modLead.strfINTEREST_LEVEL].Value = rstLead.Fields[modLead.strfINTEREST_LEVEL].Value;
                rstOpp.Fields[modLead.strfBUDGET_DOLLARS].Value = rstLead.Fields[modLead.strfBUDGET_DOLLARS].Value;
                if (!((vntResellerId == null)))
                {
                    rstOpp.Fields[modLead.strfRESELLER_ID].Value = vntResellerId;
                }
                rstOpp.Fields[modLead.strfLEAD_SOURCE_ID].Value = rstLead.Fields[modLead.strfLEAD_SOURCE_ID].Value;
                rstOpp.Fields[modLead.strfLEAD_SOURCE_TYPE].Value = rstLead.Fields[modLead.strfLEAD_SOURCE_TYPE].Value;
                rstOpp.Fields[modLead.strfREFERRED_BY_ID].Value = rstLead.Fields[modLead.strfREFERRED_BY_CONTACT_ID].Value;
                rstOpp.Fields[modLead.strfREFERRED_BY_EMPLOYEE_ID].Value = rstLead.Fields[modLead.strfREFERRED_BY_EMPLOYEE_ID].Value;
                rstOpp.Fields[modLead.strfDESCRIPTION].Value = rstLead.Fields[modLead.strfCOMMENTS].Value;

                if (Convert.IsDBNull(rstLead.Fields[modLead.strfPRODUCT_INTEREST_TYPE].Value))
                {
                    rstOpp.Fields[modLead.strfPRODUCT_TYPE_INTEREST].Value = "Please Fill In";
                }
                else
                {
                    rstOpp.Fields[modLead.strfPRODUCT_TYPE_INTEREST].Value = rstLead.Fields[modLead.strfPRODUCT_INTEREST_TYPE].Value;
                }

                if (Convert.IsDBNull(rstOpp.Fields[modLead.strfCURRENCY_ID].Value))
                {
                    if (Convert.IsDBNull(rstLead.Fields[modLead.strfCURRENCY_ID].Value))
                    {
                        rstOpp.Fields[modLead.strfCURRENCY_ID].Value = objrCurrency.SystemDefaultCurrency;
                    }
                    else
                    {
                        rstOpp.Fields[modLead.strfCURRENCY_ID].Value = rstLead.Fields[modLead.strfCURRENCY_ID].Value;
                    }
                }
                rstOpp.Fields[modLead.strfDELTA_CURRENCY_ID].Value = rstOpp.Fields[modLead.strfCURRENCY_ID].Value;
                rstOpp.Fields[modLead.strfPARTNER_CONTACT_ID].Value = rstLead.Fields[modLead.strfASSIGNED_TO_PARTNER_CONTACT].Value;

                // Arbitrary values (to be modified by user later on.)
                // (maybe give user a info message.)
                rstOpp.Fields[modLead.strfOPPORTUNITY_NAME].Value = "Please fill in";
                // to be filled out by user.
                rstOpp.Fields[modLead.strfEXPECTED_REVENUE_DATE].Value = rstLead.Fields[modLead.strfDECISION_DATE].Value;
                rstOpp.Fields[modLead.strfSTATUS].Value = "In Progress";
                rstOpp.Fields[modLead.strfPROBABILITY_TO_CLOSE].Value = 0;
                rstOpp.Fields[modLead.strfESTIMATED_TOTAL].Value = 0;

                if (Convert.IsDBNull(rstOpp.Fields[modLead.strfACCOUNT_MANAGER_ID].Value))
                {
                    rstOpp.Fields[modLead.strfOVERRIDE_CALC_PROBABILITY].Value = true;
                    if (Convert.IsDBNull(rstLead.Fields[modLead.strfACCOUNT_MANAGER_ID].Value))
                    {
                        DataAccess objrFncLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                        Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                        rstOpp.Fields[modLead.strfACCOUNT_MANAGER_ID].Value = administration.CurrentUserRecordId;
                        objrFncLib = null;
                    }
                    else
                    {
                        rstOpp.Fields[modLead.strfACCOUNT_MANAGER_ID].Value = rstLead.Fields[modLead.strfACCOUNT_MANAGER_ID].Value;
                    }
                }

                TransitionPointParameter objrParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objrParams.ParameterList = vntOppParams;
                objrParams.Construct();
                userDefinedArray = objrParams.GetUserDefinedParameterArray();
                userDefinedArray = new object[4];
                objrParams.SetUserDefinedParameter(1, false);
                objrParams.SetUserDefinedParameter(2, false);
                objrParams.SetUserDefinedParameter(3, false);

                userDefinedArray[1] = objrParams.GetUserDefinedParameter(1);
                userDefinedArray[2] = objrParams.GetUserDefinedParameter(2);
                userDefinedArray[3] = objrParams.GetUserDefinedParameter(3);
                vntOppParams = objrParams.SetUserDefinedParameterArray(userDefinedArray);

                // Convert the product interests into Opportunity products (quotes)
                Recordset rstProdInterest = LeadForm.SecondaryFromVariantArray(LeadRecordset, modLead.strPRODUCT_INTEREST_SEGMENT);

                DataAccess ocmsFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstOppProduct = rfrmOpp.SecondaryFromVariantArray(vntOppRst, modLead.strPRODUCT_QUOTE_SEGMENT);
                if (rstProdInterest.RecordCount > 0)
                {
                    rstProdInterest.MoveFirst();
                    while (!(rstProdInterest.EOF))
                    {
                        object ParameterList = DBNull.Value;
                        rfrmOpp.NewSecondaryData(modLead.strPRODUCT_QUOTE_SEGMENT, ref ParameterList, rstOppProduct);
                        rstOppProduct.MoveLast();
                        rstOppProduct.Fields[modLead.strfPRODUCT_ID].Value = rstProdInterest.Fields[modLead.strfPRODUCT_ID].Value;
                        rstOppProduct.Fields[modLead.strfQUANTITY].Value = rstProdInterest.Fields[modLead.strfQUANTITY].Value;
                        Recordset rstProduct = ocmsFunctionLib.GetRecordset(rstProdInterest.Fields[modLead.strfPRODUCT_ID].Value,
                            modLead.strtPRODUCT, modLead.strfPRICE);
                        rstOppProduct.Fields[modLead.strfPRICE].Value = rstProduct.Fields[modLead.strfPRICE].Value;
                        rstProdInterest.MoveNext();
                    }
                }
                return rfrmOpp.AddFormData(vntOppRst, ref vntOppParams);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function creates a Note object from a lead.
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        /// </history>
        protected virtual object CreateNote(Recordset rstCompany, Recordset rstContact)
        {
            try
            {
                object[] recordArray = (object[])LeadRecordset;
                Recordset rstLead = (Recordset)recordArray[0];

                // Create the new Note form
                IRForm rfrmNote = RSysSystem.Forms[modLead.strNOTE_FORM];

                object ParameterList = DBNull.Value;
                object vntNoteRst = rfrmNote.NewFormData(ref ParameterList);

                recordArray = (object[])vntNoteRst;
                Recordset rstNote = (Recordset)recordArray[0];
                rstNote.Fields[modLead.strfCONTACT].Value = rstContact.Fields[modLead.strfCONTACT_ID].Value;
                rstNote.Fields[modLead.strfCOMPANY].Value = rstContact.Fields[modLead.strfCOMPANY_ID].Value;

                string strDesc = TypeConvert.ToString(RSysSystem.GetLDGroup(modLead.strLEAD_LDGROUP).GetText("Call Desc"));
                // "Call: Follow up referred lead"
                if (!(Convert.IsDBNull(rstContact.Fields[modLead.strfRN_DESCRIPTOR].Value)))
                {
                    strDesc = strDesc + " " + TypeConvert.ToString(rstContact.Fields[modLead.strfRN_DESCRIPTOR].Value);
                }
                rstNote.Fields[modLead.strfAPPT_DESCRIPTION].Value = strDesc;

                object vntArrParam = new object[] {rstLead.Fields[modLead.strfQUALITY].Value,
                    rstLead.Fields[modLead.strfBUDGET_DOLLARS].Value,
                    rstLead.Fields[modLead.strfDATE_ENTERED].Value,
                    rstLead.Fields[modLead.strfDECISION_DATE].Value,
                    rstLead.Fields[modLead.strfINTEREST_LEVEL].Value};
                string strNotes = TypeConvert.ToString(RSysSystem.GetLDGroup(modLead.strLEAD_LDGROUP).GetTextSub("Call Notes", vntArrParam));

                rstNote.Fields[modLead.strfNOTES].Value = strNotes;

                DataAccess objrFncLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                rstNote.Fields[modLead.strfASSIGNED_BY].Value = administration.CurrentUserRecordId;
                rstNote.Fields[modLead.strfRN_EMPLOYEE_ID].Value = rstContact.Fields[modLead.strfACCOUNT_MANAGER_ID].Value;
                rstNote.Fields[modLead.strfMARKETING_PROJECT].Value = rstLead.Fields[modLead.strfLEAD_SOURCE_ID].Value;
                // Reproduce logic in default formula (next version will probably run them in the Middle Tier)
                rstNote.Fields[modLead.strfAPPT_DATE].Value = DateTime.Now;
                rstNote.Fields[modLead.strfACTIVITY_COMPLETED_DATE].Value = DateTime.Now;
                rstNote.Fields[modLead.strfACTIVITY_COMPLETE].Value = true;
                rstNote.Fields[modLead.strfACTIVITY_TYPE].Value = 5;
                rstNote.Fields[modLead.strfAPPT_PRIORITY].Value = "Medium";
                rstNote.Fields[modLead.strfACCESS_TYPE].Value = 1;

                // 'Addition for M1 Integration by ASikri November 4, 2002
                // .Fields(strfM1_CONTACT_ID).Value = rstContact.Fields(strfM1_CONTACT_ID).Value
                ParameterList = DBNull.Value;
                return rfrmNote.AddFormData(vntNoteRst, ref ParameterList);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// M1 Integration.
        /// </summary>
        /// <param name="vntContactId">Contact Id</param>
        /// <param name="vntLeadId">Lead Id</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author       Description
        /// 3.8.0.0       5/10/2006   PPhilip      Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateContact(object vntContactId, object vntLeadId)
        {
            try
            {
                const string strfM1_CONTACT_ID = "M1_Contact_Id";
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.
                       DataAccessAppServerRuleName].CreateInstance();

                Recordset rstRecordset = objLib.GetRecordset(modLead.strqLEADS_OF_LEAD, 1, vntLeadId,
                    strfM1_CONTACT_ID, modLead.strfM1_UNSUBSCRIBE);

                int intm1_contact_id = 0;
                bool intm1_unsubscribe = false;
                if (rstRecordset.RecordCount > 0)
                {
                    intm1_contact_id = Convert.ToInt32(rstRecordset.Fields[strfM1_CONTACT_ID].Value);
                    intm1_unsubscribe = Convert.ToBoolean(rstRecordset.Fields[modLead.strfM1_UNSUBSCRIBE].Value);
                }

                if (Convert.ToBoolean(intm1_contact_id))
                {

                    objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.
                       DataAccessAppServerRuleName].CreateInstance();

                    rstRecordset = objLib.GetRecordset(modLead.strqCONTACT_WITH_CONTACTID, 1, vntContactId,
                    strfM1_CONTACT_ID, modLead.strfM1_UNSUBSCRIBE);

                    if (rstRecordset.RecordCount > 0)
                    {
                        rstRecordset.Fields[strfM1_CONTACT_ID].Value = intm1_contact_id;
                        rstRecordset.Fields[modLead.strfM1_UNSUBSCRIBE].Value = intm1_unsubscribe;
                    }
                    objLib.SaveRecordset(modLead.strtCONTACT, rstRecordset);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }

        }

        /// <summary>
        /// Returns true if Expression is neighter Empty or Null, else returns false.
        /// </summary>
        /// <param name="Expression">the expression to evaluate</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author       Description
        /// 3.8.0.0       5/10/2006   PPhilip      Converted to .Net C# code.
        /// </history>
        protected virtual bool IsNull2(ref object Expression)
        {
            bool IsNull2 = false;
            if ((Expression == null))
                Expression = DBNull.Value;
            if (Convert.IsDBNull(Expression))
                IsNull2 = true;
            else
                IsNull2 = false;
            return IsNull2;
        }

        /// <summary>
        /// This function is called from Quick Path functionality. If there is a duplicate
        /// contact, this function will update the contact, neighborhood profile and visit log
        /// for the contact.
        /// </summary>
        /// <param name="vntContactId">record to update</param>
        /// <param name="rstLead">Record set for lead</param>
        /// <param name="vntCobuyerId">Buyer id</param>
        /// <param name="vntNeighborhoodId">Neighborhood Id</param>
        /// <param name="vntfParameter">Parameter passed from IRFormScript_Execute</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author       Description
        /// 3.8.0.0       5/10/2006   PPhilip      Converted to .Net C# code.
        /// because that piece has been moved to the
        /// global client script HB_Global_Shared_Function_2
        /// </history>
        public virtual void UpdateDuplicateContact(object vntContactId, Recordset rstLead, object vntNeighborhoodId,
            object vntCobuyerId, ref object[] vntfParameter)
        {
            try
            {
                Recordset rstContact = null;
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                ContactProfileNeighborhood objContactProfileNBHD = (ContactProfileNeighborhood)RSysSystem.ServerScripts[modLead.strsCONTACT_PROFILE_NBHD].CreateInstance();

                object vntSpokedWithId = DBNull.Value;                object vntVisitDate = null;
                object vntNBHD_ProfileId = DBNull.Value;                Recordset rstVisitLog;
                if ((vntContactId is Array))
                {
                    if (rstLead.RecordCount > 0)
                    {
                        vntSpokedWithId = rstLead.Fields[modLead.strfVL1_EMPLOYEE_ID].Value;
                        vntVisitDate = rstLead.Fields[modLead.strfNP1_FIRST_VISIT_DATE].Value;
                    }

                    if ((vntNeighborhoodId is Array))
                    {
                        // Get the Contact Neighborhood Profile for the Contact & Neighborhood
                        // and update based on Lead QuickPath info.
                        Recordset rstContactNBHDProfile = objLib.GetRecordset(modLead.strqNBHD_PROFILE_FOR_CONTACT_AND_NEIGHBORHOOD,
                            2, vntContactId, vntNeighborhoodId, modLead.strfCONTACT_PROFILE_NBHD_ID, modLead.strfPRIORITY_CODE_ID,
                            modLead.strfFIRST_VISIT_DATE, modLead.strfNEXT_FOLLOW_UP);

                        if (rstContactNBHDProfile.RecordCount > 0)
                        {
                            rstContactNBHDProfile.MoveFirst();
                            vntNBHD_ProfileId = rstContactNBHDProfile.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value;
                            rstContactNBHDProfile.Fields[modLead.strfPRIORITY_CODE_ID].Value = rstLead.Fields[modLead.strfPRIORITY_CODE_ID].Value;
                            if (Convert.IsDBNull(rstContactNBHDProfile.Fields[modLead.strfFIRST_VISIT_DATE].Value))
                            {
                                rstContactNBHDProfile.Fields[modLead.strfFIRST_VISIT_DATE].Value = rstLead.Fields[modLead.strfNP1_FIRST_VISIT_DATE].Value;
                            }
                            rstContactNBHDProfile.Fields[modLead.strfNEXT_FOLLOW_UP].Value = rstLead.Fields[modLead.strfVL1_NEXT_DATE].Value;
                            objLib.SaveRecordset(modLead.strtCONTACT_PROFILE_NEIGHBORHOOD, rstContactNBHDProfile);
                            // save Contact Neighborhood Profile
                            rstContactNBHDProfile.Close();

                            if (!(Convert.IsDBNull(vntVisitDate)))
                            {
                                rstVisitLog = objLib.GetRecordset(modLead.strqVALID_VISIT_LOG_TO_UPDATE_QUICK_PATH,
                                    4, vntNBHD_ProfileId, vntNeighborhoodId, vntSpokedWithId, vntVisitDate, modLead.strfRN_APPOINTMENTS_ID,
                                    modLead.strfNEXT_FOLLOW_UP_DATE, modLead.strfNOTES);
                                if (rstVisitLog.RecordCount > 0)
                                {
                                    rstVisitLog.MoveFirst();
                                    rstVisitLog.Fields[modLead.strfNEXT_FOLLOW_UP_DATE].Value = rstLead.Fields[modLead.strfVL1_NEXT_DATE].Value;
                                    rstVisitLog.Fields[modLead.strfNOTES].Value = Convert.ToString(rstVisitLog.Fields[modLead.strfNOTES].Value)
                                        + Convert.ToString(rstLead.Fields[modLead.strfVL1_VISIT_COMMENTS].Value);
                                    objLib.SaveRecordset(modLead.strRN_APPOINTMENTS_TABLE, rstVisitLog);
                                    rstVisitLog.Close();
                                }
                                else
                                {
                                    rstVisitLog = objLib.GetNewRecordset(modLead.strRN_APPOINTMENTS_TABLE, modLead.strfRN_APPOINTMENTS_ID,
                                        modLead.strfNOTES, modLead.strfNEXT_FOLLOW_UP_DATE, modLead.strfNEIGHBORHOOD_ID,
                                        modLead.strfAPPT_DATE, modLead.strfCONTACT, modLead.strfRN_EMPLOYEE_ID,
                                        modLead.strfCONTACT_PROFILE_NBHD_ID, modLead.strfACTIVITY_TYPE, modLead.strfASSIGNED_BY,
                                        modLead.strfACTIVITY_COMPLETE, modLead.strfACTIVITY_COMPLETED_DATE);
                                    rstVisitLog.AddNew(modLead.strfRN_APPOINTMENTS_ID, DBNull.Value);
                                    rstVisitLog.Fields[modLead.strfCONTACT].Value = vntContactId;
                                    rstVisitLog.Fields[modLead.strfNEIGHBORHOOD_ID].Value = vntNeighborhoodId;
                                    rstVisitLog.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value = vntNBHD_ProfileId;
                                    rstVisitLog.Fields[modLead.strfAPPT_DATE].Value = vntVisitDate;
                                    rstVisitLog.Fields[modLead.strfRN_EMPLOYEE_ID].Value = vntSpokedWithId;
                                    rstVisitLog.Fields[modLead.strfASSIGNED_BY].Value = vntSpokedWithId;
                                    rstVisitLog.Fields[modLead.strfNEXT_FOLLOW_UP_DATE].Value = rstLead.Fields[modLead.strfVL1_NEXT_DATE].Value;
                                    rstVisitLog.Fields[modLead.strfNOTES].Value = rstLead.Fields[modLead.strfVL1_VISIT_COMMENTS].Value;
                                    rstVisitLog.Fields[modLead.strfACTIVITY_TYPE].Value = modLead.lngACTIVITY_TYPE_VISITLOG;
                                    rstVisitLog.Fields[modLead.strfACTIVITY_COMPLETE].Value = true;
                                    rstVisitLog.Fields[modLead.strfACTIVITY_COMPLETED_DATE].Value = DateTime.Today;
                                    objLib.SaveRecordset(modLead.strRN_APPOINTMENTS_TABLE, rstVisitLog);
                                    rstVisitLog.Close();
                                    objContactProfileNBHD.UpdateNBHDPType(vntNBHD_ProfileId);
                                }
                            }

                        }
                        else
                        {
                            // Add new neighborhood profile
                            Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                            vntNBHD_ProfileId = objContactProfileNBHD.NewNeighborhoodProfile(vntNeighborhoodId, vntContactId,
                                DBNull.Value, new object[] {administration.CurrentUserRecordId, rstLead.Fields[modLead.strfPRIORITY_CODE_ID].Value,
                                rstLead.Fields[modLead.strfLEAD_SOURCE_ID].Value, DateTime.Today, vntVisitDate});

                            if ((vntNBHD_ProfileId is Array) && !(Convert.IsDBNull(rstLead.Fields[modLead.strfNP1_FIRST_VISIT_DATE].Value)))
                            {
                                // add new Visit Log
                                rstVisitLog = objLib.GetNewRecordset(modLead.strRN_APPOINTMENTS_TABLE, modLead.strfRN_APPOINTMENTS_ID,
                                    modLead.strfNOTES, modLead.strfNEXT_FOLLOW_UP_DATE, modLead.strfNEIGHBORHOOD_ID,
                                    modLead.strfAPPT_DATE, modLead.strfCONTACT, modLead.strfRN_EMPLOYEE_ID,
                                    modLead.strfCONTACT_PROFILE_NBHD_ID, modLead.strfACTIVITY_TYPE, modLead.strfASSIGNED_BY,
                                    modLead.strfACTIVITY_COMPLETE, modLead.strfACTIVITY_COMPLETED_DATE);
                                rstVisitLog.AddNew(modLead.strfRN_APPOINTMENTS_ID, DBNull.Value);
                                rstVisitLog.Fields[modLead.strfCONTACT].Value = vntContactId;
                                rstVisitLog.Fields[modLead.strfNEIGHBORHOOD_ID].Value = vntNeighborhoodId;
                                rstVisitLog.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value = vntNBHD_ProfileId;
                                rstVisitLog.Fields[modLead.strfAPPT_DATE].Value = vntVisitDate;
                                rstVisitLog.Fields[modLead.strfRN_EMPLOYEE_ID].Value = vntSpokedWithId;
                                rstVisitLog.Fields[modLead.strfASSIGNED_BY].Value = vntSpokedWithId;
                                rstVisitLog.Fields[modLead.strfNEXT_FOLLOW_UP_DATE].Value = rstLead.Fields[modLead.strfVL1_NEXT_DATE].Value;
                                rstVisitLog.Fields[modLead.strfNOTES].Value = rstLead.Fields[modLead.strfVL1_VISIT_COMMENTS].Value;
                                rstVisitLog.Fields[modLead.strfACTIVITY_TYPE].Value = modLead.lngACTIVITY_TYPE_VISITLOG;
                                rstVisitLog.Fields[modLead.strfACTIVITY_COMPLETE].Value = true;
                                rstVisitLog.Fields[modLead.strfACTIVITY_COMPLETED_DATE].Value = DateTime.Today;

                                objLib.SaveRecordset(modLead.strRN_APPOINTMENTS_TABLE, rstVisitLog);
                                rstVisitLog.Close();
                                objContactProfileNBHD.UpdateNBHDPType(vntNBHD_ProfileId);
                            }
                        }
                    }
                    vntfParameter = new object[] { rstContact };
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function is called on add or update Contact
        /// </summary>
        /// <param name="rstContact">Recordset for Contact</param>
        /// <param name="rstLead"> Recordset for Lead</param>
        /// <param name="strContactType">old param.  Not used anymore.</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date        Author       Description
        /// 3.8.0.0       5/10/2006   PPhilip      Converted to .Net C# code.
        /// 5.9.0         6/8/2010    KA           Added TIC Custom fields
        /// </history>
        public virtual void AddUpdateContactFields(Recordset rstContact, Recordset rstLead, string strContactType)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                rstContact.Fields[modLead.strfACCOUNT_MANAGER_ID].Value = rstLead.Fields[modLead.strfACCOUNT_MANAGER_ID].Value;
                
                //KA 6/8/10 TIC custom field mapping loop
                for (int a = 0; a <= rstLead.Fields.Count - 1; a++)
                {
                    if (rstLead.Fields[a].Name.ToString().StartsWith( "TIC_") == true)
                    {
                        if (rstLead.Fields[a].Name.ToString() != "TIC_Password" && rstLead.Fields[a].Name.ToString() != "TIC_Numeric_Lead_Id")
                        {
                            rstContact.Fields[rstLead.Fields[a].Name.ToString()].Value = rstLead.Fields[a].Value;
                        }
                        else if (rstLead.Fields[a].Name.ToString() != "TIC_Numeric_Lead_Id")
                        {
                            //if it's password then if the type is not a cobuyer, then update password
                            if (strContactType != "Cobuyer")
                            {
                                rstContact.Fields[rstLead.Fields[a].Name.ToString()].Value = rstLead.Fields[a].Value;
                            }
                        }
                    }
                }

                if (Convert.IsDBNull(rstContact.Fields[modLead.strfACCOUNT_MANAGER_ID].Value))
                {
                    Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                    rstContact.Fields[modLead.strfACCOUNT_MANAGER_ID].Value = administration.CurrentUserRecordId;
                }

                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfADDRESS_1].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfADDRESS_1].Value))
                {
                    rstContact.Fields[modLead.strfADDRESS_1].Value = rstLead.Fields[modLead.strfADDRESS_1].Value;
                    rstContact.Fields[modLead.strfADDRESS_2].Value = rstLead.Fields[modLead.strfADDRESS_2].Value;
                    rstContact.Fields[modLead.strfADDRESS_3].Value = rstLead.Fields[modLead.strfADDRESS_3].Value;
                    rstContact.Fields[modLead.strfCITY].Value = rstLead.Fields[modLead.strfCITY].Value;
                    rstContact.Fields[modLead.strfSTATE_].Value = rstLead.Fields[modLead.strfSTATE_].Value;
                    rstContact.Fields[modLead.strfCOUNTRY].Value = rstLead.Fields[modLead.strfCOUNTRY].Value;
                    rstContact.Fields[modLead.strfCOUNTY_ID].Value = rstLead.Fields[modLead.strfCOUNTY_ID].Value;
                    rstContact.Fields[modLead.strfZIP].Value = rstLead.Fields[modLead.strfZIP].Value;
                    
                }
                //KA 6/9/10 don't copy primary lead's fields to cobuyer cause they are specific to primary lead
                if (strContactType != "Cobuyer")
                {
                    if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfEMAIL].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfEMAIL].Value))
                    {
                        rstContact.Fields[modLead.strfEMAIL].Value = rstLead.Fields[modLead.strfEMAIL].Value;
                    }
                    rstContact.Fields[modLead.strfM1_CONTACT_ID].Value = rstLead.Fields[modLead.strfM1_CONTACT_ID].Value;
                    rstContact.Fields[modLead.strfDNC_STATUS].Value = rstLead.Fields[modLead.strfDNC_STATUS].Value;
                }
                if (Convert.ToBoolean(rstLead.Fields[modLead.strfCOBUYER_MARRIED_TO_BUYER].Value))
                {
                    rstContact.Fields[modLead.strfMARITAL_STATUS].Value = "Married";
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfAGE_RANGE_OF_BUYERS].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfAGE_RANGE_OF_BUYERS].Value))
                {
                    rstContact.Fields[modLead.strfAGE_RANGE_OF_BUYERS].Value = rstLead.Fields[modLead.strfAGE_RANGE_OF_BUYERS].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfAGE_RANGE_OF_BUYERS].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfAGE_RANGE_OF_CHILDREN].Value))
                {
                    rstContact.Fields[modLead.strfAGE_RANGE_OF_CHILDREN].Value = rstLead.Fields[modLead.strfAGE_RANGE_OF_CHILDREN].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfAREA_CODE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfAREA_CODE].Value))
                {
                    rstContact.Fields[modLead.strfAREA_CODE].Value = rstLead.Fields[modLead.strfAREA_CODE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfCELL].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfCELL].Value))
                {
                    rstContact.Fields[modLead.strfCELL].Value = rstLead.Fields[modLead.strfCELL].Value;
                }

                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfCOMBINED_INCOME_RANGE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfCOMBINED_INCOME_RANGE].Value))
                {
                    rstContact.Fields[modLead.strfCOMBINED_INCOME_RANGE].Value = rstLead.Fields[modLead.strfCOMBINED_INCOME_RANGE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfCOMMENTS].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfCOMMENTS].Value))
                {
                    rstContact.Fields[modLead.strfCOMMENTS].Value = rstLead.Fields[modLead.strfCOMMENTS].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfCOMMUTE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfCOMMUTE].Value))
                {
                    rstContact.Fields[modLead.strfCOMMUTE].Value = rstLead.Fields[modLead.strfCOMMUTE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfCURRENT_MONTHLY_PAYMENT].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfCURRENT_MONTHLY_PAYMENT].Value))
                {
                    rstContact.Fields[modLead.strfCURRENT_MONTHLY_PAYMENT].Value = rstLead.Fields[modLead.strfCURRENT_MONTHLY_PAYMENT].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfCURRENT_SQUARE_FOOTAGE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfCURRENT_SQUARE_FOOTAGE].Value))
                {
                    rstContact.Fields[modLead.strfCURRENT_SQUARE_FOOTAGE].Value = rstLead.Fields[modLead.strfCURRENT_SQUARE_FOOTAGE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfDESIRED_MONTHY_PAYMENT].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfDESIRED_MONTHY_PAYMENT].Value))
                {
                    rstContact.Fields[modLead.strfDESIRED_MONTHY_PAYMENT].Value = rstLead.Fields[modLead.strfDESIRED_MONTHY_PAYMENT].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfDESIRED_MOVE_IN_DATE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfDESIRED_MOVE_IN_DATE].Value))
                {
                    rstContact.Fields[modLead.strfDESIRED_MOVE_IN_DATE].Value = rstLead.Fields[modLead.strfDESIRED_MOVE_IN_DATE].Value;
                }

                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfCOMPANY_NAME].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfCOMPANY_NAME].Value))
                {
                    rstContact.Fields[modLead.strfCOMPANY_NAME].Value = rstLead.Fields[modLead.strfCOMPANY_NAME].Value;
                }
                
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfEDUCATION].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfEDUCATION].Value))
                {
                    rstContact.Fields[modLead.strfEDUCATION].Value = rstLead.Fields[modLead.strfEDUCATION].Value;
                }
                
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfEXTENSION].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfEXTENSION].Value))
                {
                    rstContact.Fields[modLead.strfEXTENSION].Value = rstLead.Fields[modLead.strfEXTENSION].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfFAX].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfFAX].Value))
                {
                    rstContact.Fields[modLead.strfFAX].Value = rstLead.Fields[modLead.strfFAX].Value;
                }

                rstContact.Fields[modLead.strfFIRST_CONTACT_DATE].Value = DateTime.Today;

                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfFIRST_NAME].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfFIRST_NAME].Value))
                {
                    rstContact.Fields[modLead.strfFIRST_NAME].Value = rstLead.Fields[modLead.strfFIRST_NAME].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfFOR_SALE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfFOR_SALE].Value))
                {
                    rstContact.Fields[modLead.strfFOR_SALE].Value = rstLead.Fields[modLead.strfFOR_SALE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfHOME_TYPE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfHOME_TYPE].Value))
                {
                    rstContact.Fields[modLead.strfHOME_TYPE].Value = rstLead.Fields[modLead.strfHOME_TYPE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfHOMES_OWNED].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfHOMES_OWNED].Value))
                {
                    rstContact.Fields[modLead.strfHOMES_OWNED].Value = rstLead.Fields[modLead.strfHOMES_OWNED].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfHOUSEHOLD_SIZE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfHOUSEHOLD_SIZE].Value))
                {
                    rstContact.Fields[modLead.strfHOUSEHOLD_SIZE].Value = rstLead.Fields[modLead.strfHOUSEHOLD_SIZE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfLAST_NAME].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfLAST_NAME].Value))
                {
                    rstContact.Fields[modLead.strfLAST_NAME].Value = rstLead.Fields[modLead.strfLAST_NAME].Value;
                }
                
                if ((rstLead.Fields[modLead.strfLEAD_SOURCE_ID].Value is Array) && Convert.IsDBNull(rstContact.Fields[modLead.strfLEAD_SOURCE_ID].Value))
                {
                    rstContact.Fields[modLead.strfLEAD_SOURCE_ID].Value = rstLead.Fields[modLead.strfLEAD_SOURCE_ID].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfLEAD_SOURCE_TYPE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfLEAD_SOURCE_TYPE].Value))
                {
                    rstContact.Fields[modLead.strfLEAD_SOURCE_TYPE].Value = rstLead.Fields[modLead.strfLEAD_SOURCE_TYPE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfMINIMUM_BATHROOMS].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfMINIMUM_BATHROOMS].Value))
                {
                    rstContact.Fields[modLead.strfMINIMUM_BATHROOMS].Value = rstLead.Fields[modLead.strfMINIMUM_BATHROOMS].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfMINIMUM_BEDROOMS].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfMINIMUM_BEDROOMS].Value))
                {
                    rstContact.Fields[modLead.strfMINIMUM_BEDROOMS].Value = rstLead.Fields[modLead.strfMINIMUM_BEDROOMS].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfMINIMUM_GARAGE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfMINIMUM_GARAGE].Value))
                {
                    rstContact.Fields[modLead.strfMINIMUM_GARAGE].Value = rstLead.Fields[modLead.strfMINIMUM_GARAGE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfVL1_NEXT_DATE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfNEXT_FOLLOW_UP_DATE].Value))
                {
                    rstContact.Fields[modLead.strfNEXT_FOLLOW_UP_DATE].Value = rstLead.Fields[modLead.strfVL1_NEXT_DATE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfNUMBER_LIVING_AREAS].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfNUMBER_LIVING_AREAS].Value))
                {
                    rstContact.Fields[modLead.strfNUMBER_LIVING_AREAS].Value = rstLead.Fields[modLead.strfNUMBER_LIVING_AREAS].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfNUMBER_OF_CHILDREN].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfNUMBER_OF_CHILDREN].Value))
                {
                    rstContact.Fields[modLead.strfNUMBER_OF_CHILDREN].Value = rstLead.Fields[modLead.strfNUMBER_OF_CHILDREN].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfOTHER_BUILDERS].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfOTHER_BUILDERS].Value))
                {
                    rstContact.Fields[modLead.strfOTHER_BUILDERS].Value = rstLead.Fields[modLead.strfOTHER_BUILDERS].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfOTHER_NEIGHBORHOODS].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfOTHER_NEIGHBORHOODS].Value))
                {
                    rstContact.Fields[modLead.strfOTHER_NEIGHBORHOODS].Value = rstLead.Fields[modLead.strfOTHER_NEIGHBORHOODS].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfOWNERSHIP].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfOWNERSHIP].Value))
                {
                    rstContact.Fields[modLead.strfOWNERSHIP].Value = rstLead.Fields[modLead.strfOWNERSHIP].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfPHONE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfPHONE].Value))
                {
                    rstContact.Fields[modLead.strfPHONE].Value = rstLead.Fields[modLead.strfPHONE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfPREFERRED_AREA].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfPREFERRED_AREA].Value))
                {
                    rstContact.Fields[modLead.strfPREFERRED_AREA].Value = rstLead.Fields[modLead.strfPREFERRED_AREA].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfPREFERRED_CONTACT].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfPREFERRED_CONTACT].Value))
                {
                    rstContact.Fields[modLead.strfPREFERRED_CONTACT].Value = rstLead.Fields[modLead.strfPREFERRED_CONTACT].Value;
                }
                
                if ((rstLead.Fields[modLead.strfREALTOR_AGENT_ID].Value is Array) && Convert.IsDBNull(rstContact.Fields[modLead.strfREALTOR_ID].Value))
                {
                    rstContact.Fields[modLead.strfREALTOR_ID].Value = rstLead.Fields[modLead.strfREALTOR_AGENT_ID].Value;
                }
                
                if ((rstLead.Fields[modLead.strfREALTOR_COMPANY_ID].Value is Array) && Convert.IsDBNull(rstContact.Fields[modLead.strfREALTOR_COMPANY_ID].Value))
                {
                    rstContact.Fields[modLead.strfREALTOR_COMPANY_ID].Value = rstLead.Fields[modLead.strfREALTOR_COMPANY_ID].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfREASONS_FOR_MOVING].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfREASONS_FOR_MOVING].Value))
                {
                    rstContact.Fields[modLead.strfREASONS_FOR_MOVING].Value = rstLead.Fields[modLead.strfREASONS_FOR_MOVING].Value;
                }
                
                if ((rstLead.Fields[modLead.strfREFERRED_BY_CONTACT_ID].Value is Array) && Convert.IsDBNull(rstContact.Fields[modLead.strfREFERRED_BY_CONTACT_ID].Value))
                {
                    rstContact.Fields[modLead.strfREFERRED_BY_CONTACT_ID].Value = rstLead.Fields[modLead.strfREFERRED_BY_CONTACT_ID].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfRESALE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfRESALE].Value))
                {
                    rstContact.Fields[modLead.strfRESALE].Value = rstLead.Fields[modLead.strfRESALE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfSINGLE_OR_DUAL_INCOME].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfSINGLE_OR_DUAL_INCOME].Value))
                {
                    rstContact.Fields[modLead.strfSINGLE_OR_DUAL_INCOME].Value = rstLead.Fields[modLead.strfSINGLE_OR_DUAL_INCOME].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfSUFFIX].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfSUFFIX].Value))
                {
                    rstContact.Fields[modLead.strfSUFFIX].Value = rstLead.Fields[modLead.strfSUFFIX].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfTITLE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfTITLE].Value))
                {
                    rstContact.Fields[modLead.strfTITLE].Value = rstLead.Fields[modLead.strfTITLE].Value;
                }
                
                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfTRANSFERRING_TO_AREA].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfTRANSFERRING_TO_AREA].Value))
                {
                    rstContact.Fields[modLead.strfTRANSFERRING_TO_AREA].Value = rstLead.Fields[modLead.strfTRANSFERRING_TO_AREA].Value;
                }

                //rstContact.Fields[modLead.strfTYPE].Value = modLead.strsCUSTOMER;
                rstContact.Fields[modLead.strfTYPE].Value = modLead.strsPROSPECT;

                if (!(Convert.IsDBNull(rstLead.Fields[modLead.strfWORK_PHONE].Value)) && Convert.IsDBNull(rstContact.Fields[modLead.strfWORK_PHONE].Value))
                {
                    rstContact.Fields[modLead.strfWORK_PHONE].Value = rstLead.Fields[modLead.strfWORK_PHONE].Value;
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Finds duplicated lead for given first name, last name and zip code.
        /// </summary>
        /// <param name="vntFirstName">First Name</param>
        /// <param name="vntLastName">Last Name</param>
        /// <param name="vntZip">Zip code</param>
        /// <returns>Duplicated Lead records</returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        /// </history>
        public virtual Recordset LeadDuplicate(object vntFirstName, object vntLastName, object vntZip)
        {
            Recordset rstLead = null;
            try
            {
                DataAccess objDLFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                if (Convert.IsDBNull(vntZip) || TypeConvert.ToString(vntZip) == "")
                {
                    // zip is not filled in
                    rstLead = objDLFunctionLib.GetRecordset("Sys: Check Duplicate Leads", 3, vntFirstName, vntLastName, id, modLead.strfLEAD__ID, modLead.strfFIRST_NAME, modLead.strfLAST_NAME, modLead.strfPHONE, modLead.strfCITY, modLead.strfEMAIL);
                }
                else
                {
                    // zip is filled in
                    rstLead = objDLFunctionLib.GetRecordset("Sys: Check Duplicate Leads with Zip", 4, vntLastName, vntFirstName, vntZip, id, modLead.strfLEAD__ID, modLead.strfFIRST_NAME, modLead.strfLAST_NAME, modLead.strfPHONE, modLead.strfCITY, modLead.strfEMAIL);
                }
                return rstLead;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Copies data from Source Lead to Target Lead.
        /// </summary>
        /// <param name="vntFromLeadId">Source Lead Id</param>
        /// <param name="vntToLeadId">Target Lead Id</param>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        ///               7/20/2006   JH        Merged 3.7 sp1 in.
        /// </history>
        protected virtual void MergeLeadFromLead(object vntFromLeadId, object vntToLeadId)
        {
            try
            {
                if (Convert.IsDBNull(vntFromLeadId) || (vntToLeadId == null))
                    return;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                DataAccess objDLFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                UIAccess objPLFunctionLib = (UIAccess)RSysSystem.ServerScripts[AppServerRuleData.UIAccessAppServerRuleName].CreateInstance();
                objDLFunctionLib.PermissionIgnored = true;

                // Loads lead recordset
                Recordset rstFromLead = objDLFunctionLib.GetRecordset(vntFromLeadId, modLead.strtLEAD_, modLead.strfFIRST_NAME,
                    modLead.strfADDRESS_1, modLead.strfADDRESS_2, modLead.strfADDRESS_3, modLead.strfZIP,
                    modLead.strfCITY, modLead.strfSTATE_, modLead.strfCOUNTRY, modLead.strfTYPE,
                    modLead.strfNP1_FIRST_VISIT_DATE, modLead.strfVL1_NEXT_DATE, modLead.strfPHONE, modLead.strfCELL,
                    modLead.strfWORK_PHONE, modLead.strfFAX, modLead.strfEMAIL, modLead.strfACCOUNT_MANAGER_ID,
                    modLead.strfAGE_RANGE_OF_BUYERS, modLead.strfAGE_RANGE_OF_CHILDREN, modLead.strfAREA_CODE,
                    modLead.strfCOMBINED_INCOME_RANGE, modLead.strfCOMMENTS, modLead.strfCOMMUTE, modLead.strfCURRENT_MONTHLY_PAYMENT,
                    modLead.strfCURRENT_SQUARE_FOOTAGE, modLead.strfDESIRED_MOVE_IN_DATE, modLead.strfDESIRED_PRICE_RANGE,
                    modLead.strfDESIRED_SQUARE_FOOTAGE, modLead.strfEDUCATION, modLead.strfDESIRED_MONTHLY_PAYMENT,
                    modLead.strfEXTENSION, modLead.strfFOR_SALE, modLead.strfHOME_TYPE, modLead.strfHOMES_OWNED,
                    modLead.strfHOUSEHOLD_SIZE, modLead.strfLEAD_SOURCE_ID, modLead.strfLEAD_SOURCE_TYPE,
                    modLead.strfMINIMUM_BATHROOMS, modLead.strfMINIMUM_BEDROOMS, modLead.strfMINIMUM_GARAGE,
                    modLead.strfNUMBER_LIVING_AREAS, modLead.strfNUMBER_OF_CHILDREN, modLead.strfOTHER_BUILDERS,
                    modLead.strfOTHER_NEIGHBORHOODS, modLead.strfOWNERSHIP, modLead.strfPREFERRED_AREA,
                    modLead.strfPREFERRED_CONTACT, modLead.strfREALTOR_AGENT_ID, modLead.strfREALTOR_COMPANY_ID,
                    modLead.strfREASONS_FOR_MOVING, modLead.strfREFERRED_BY_CONTACT_ID, modLead.strfRESALE,
                    modLead.strfSINGLE_OR_DUAL_INCOME, modLead.strfSUFFIX, modLead.strfTITLE, modLead.strfTRANSFERRING_TO_AREA,
                    modLead.strfTYPE, modLead.strfSSN, modLead.strfGENDER, modLead.strfCOUNTY_ID,
                    modLead.strfTIME_SEARCHING, modLead.strfSAME_AS_BUYER_ADDRESS, modLead.strfCOBUYER_MARRIED_TO_BUYER,
                    modLead.strfCO_BUYER_FIRST_NAME, modLead.strfCO_BUYER_LAST_NAME, modLead.strfCO_BUYER_WORK_PHONE,
                    modLead.strfCO_BUYER_WORK_EXTENSION, modLead.strfCO_BUYER_CELL, modLead.strfCO_BUYER_TITLE,
                    modLead.strfCO_BUYER_ADDRESS_1, modLead.strfCO_BUYER_ADDRESS_2, modLead.strfCO_BUYER_ADDRESS_3,
                    modLead.strfCO_BUYER_CITY, modLead.strfCO_BUYER_ZIP, modLead.strfCO_BUYER_COUNTY_ID,
                    modLead.strfCO_BUYER_STATE, modLead.strfNP1_NEIGHBORHOOD_ID, modLead.strfPRIORITY_CODE_ID);

                Recordset rstToLead = objDLFunctionLib.GetRecordset(vntToLeadId, modLead.strtLEAD_, modLead.strfFIRST_NAME,
                    modLead.strfADDRESS_1, modLead.strfADDRESS_2, modLead.strfADDRESS_3, modLead.strfZIP,
                    modLead.strfCITY, modLead.strfSTATE_, modLead.strfCOUNTRY, modLead.strfTYPE,
                    modLead.strfNP1_FIRST_VISIT_DATE, modLead.strfVL1_NEXT_DATE, modLead.strfPHONE, modLead.strfCELL,
                    modLead.strfWORK_PHONE, modLead.strfFAX, modLead.strfEMAIL, modLead.strfACCOUNT_MANAGER_ID,
                    modLead.strfAGE_RANGE_OF_BUYERS, modLead.strfAGE_RANGE_OF_CHILDREN, modLead.strfAREA_CODE,
                    modLead.strfCOMBINED_INCOME_RANGE, modLead.strfCOMMENTS, modLead.strfCOMMUTE, modLead.strfCURRENT_MONTHLY_PAYMENT,
                    modLead.strfCURRENT_SQUARE_FOOTAGE, modLead.strfDESIRED_MOVE_IN_DATE, modLead.strfDESIRED_PRICE_RANGE,
                    modLead.strfDESIRED_SQUARE_FOOTAGE, modLead.strfEDUCATION, modLead.strfDESIRED_MONTHLY_PAYMENT,
                    modLead.strfEXTENSION, modLead.strfFOR_SALE, modLead.strfHOME_TYPE, modLead.strfHOMES_OWNED,
                    modLead.strfHOUSEHOLD_SIZE, modLead.strfLEAD_SOURCE_ID, modLead.strfLEAD_SOURCE_TYPE,
                    modLead.strfMINIMUM_BATHROOMS, modLead.strfMINIMUM_BEDROOMS, modLead.strfMINIMUM_GARAGE,
                    modLead.strfNUMBER_LIVING_AREAS, modLead.strfNUMBER_OF_CHILDREN, modLead.strfOTHER_BUILDERS,
                    modLead.strfOTHER_NEIGHBORHOODS, modLead.strfOWNERSHIP, modLead.strfPREFERRED_AREA,
                    modLead.strfPREFERRED_CONTACT, modLead.strfREALTOR_AGENT_ID, modLead.strfREALTOR_COMPANY_ID,
                    modLead.strfREASONS_FOR_MOVING, modLead.strfREFERRED_BY_CONTACT_ID, modLead.strfRESALE,
                    modLead.strfSINGLE_OR_DUAL_INCOME, modLead.strfSUFFIX, modLead.strfTITLE, modLead.strfTRANSFERRING_TO_AREA,
                    modLead.strfTYPE, modLead.strfSSN, modLead.strfGENDER, modLead.strfCOUNTY_ID,
                    modLead.strfTIME_SEARCHING, modLead.strfSAME_AS_BUYER_ADDRESS, modLead.strfCOBUYER_MARRIED_TO_BUYER,
                    modLead.strfCO_BUYER_FIRST_NAME, modLead.strfCO_BUYER_LAST_NAME, modLead.strfCO_BUYER_WORK_PHONE,
                    modLead.strfCO_BUYER_WORK_EXTENSION, modLead.strfCO_BUYER_CELL, modLead.strfCO_BUYER_TITLE,
                    modLead.strfCO_BUYER_ADDRESS_1, modLead.strfCO_BUYER_ADDRESS_2, modLead.strfCO_BUYER_ADDRESS_3,
                    modLead.strfCO_BUYER_CITY, modLead.strfCO_BUYER_ZIP, modLead.strfCO_BUYER_COUNTY_ID,
                    modLead.strfCO_BUYER_STATE);

                // copies changed fields
                short i = 0;
                for (i = 0; i <= rstToLead.Fields.Count - 1;
                    i = Convert.ToInt16(i + 1))
                {
                    string strFieldName = rstToLead.Fields[i].Name;
                    if (strFieldName != modLead.strfLEAD__ID && (strFieldName.Substring(0, 3)).ToUpper() != "RN_")
                    {
                        if (!(Convert.IsDBNull(rstFromLead.Fields[strFieldName].Value) && Convert.IsDBNull(rstToLead.Fields[strFieldName].Value)))
                        {
                            rstToLead.Fields[strFieldName].Value = rstFromLead.Fields[strFieldName].Value;
                        }
                    }
                }
                objLib.SaveRecordset(modLead.strtLEAD_, rstToLead);
                object vntNeighborhoodId = rstFromLead.Fields[modLead.strfNP1_NEIGHBORHOOD_ID].Value;

                // If the neighborhood profile is duplicated in the existing lead, then delete
                // current quick path neighborhood profile and update the existing neighborhood profile.
                Recordset rstFindDupNP = objDLFunctionLib.GetRecordset(modLead.strqNEIGHBORHOOD_PROFILE_FOR_LEAD_NEIGHBORHOOD,
                    2, vntToLeadId, vntNeighborhoodId, modLead.strfCONTACT_PROFILE_NBHD_ID);
                Recordset rstQuickPathNP = objDLFunctionLib.GetLinkedRecordset(modLead.strtCONTACT_PROFILE_NEIGHBORHOOD,
                    modLead.strfLEAD_ID, vntFromLeadId, modLead.strfLEAD_ID);
                if (rstFindDupNP.RecordCount > 0)
                {
                    // Duplicate found. Delete the QuickPath profile and update the existing profile.
                    if (rstQuickPathNP.RecordCount > 0)
                    {
                        rstQuickPathNP.MoveFirst();
                        objLib.PermissionIgnored = true;
                        objLib.DeleteRecord(rstQuickPathNP.Fields[modLead.strfCONTACT_PROFILE_NBHD_ID].Value, modLead.strtCONTACT_PROFILE_NEIGHBORHOOD);
                    }
                    ContactProfileNeighborhood objContProfNBHD = (ContactProfileNeighborhood)RSysSystem.ServerScripts[modLead.strsCONTACT_PROFILE_NBHD].CreateInstance();
                    Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                    objContProfNBHD.NewNeighborhoodProfile(vntNeighborhoodId, DBNull.Value, vntToLeadId, new
                        object[] {administration.CurrentUserRecordId, rstFromLead.Fields[modLead.strfPRIORITY_CODE_ID].Value,
                        rstFromLead.Fields[modLead.strfLEAD_SOURCE_ID].Value, DateTime.Today, rstFromLead.Fields[modLead.strfNP1_FIRST_VISIT_DATE].Value});
                }
                else
                {
                    // No duplicate found. Simply copy QuickPath profile over to the To Lead.
                    if (rstQuickPathNP.RecordCount > 0)
                    {
                        rstQuickPathNP.MoveFirst();
                        rstQuickPathNP.Fields[modLead.strfLEAD_ID].Value = vntToLeadId;
                        objLib.SaveRecordset(modLead.strtCONTACT_PROFILE_NEIGHBORHOOD, rstQuickPathNP);
                    }
                }                
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Deletes Lead from the database
        /// </summary>
        /// <param name="vntLeadId">Lead Id</param>
        /// Return: None
        /// Revision# Date Author Description
        /// 3.8.0.0   5/10/2006  PPhilip  Converted to .Net C# code.
        protected virtual void DeleteLead(object vntLeadId)
        {
            try
            {
                DataAccess objDLFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstLead = objDLFunctionLib.GetRecordset(vntLeadId, modLead.strtLEAD_, modLead.strfLEAD__ID);
                if (rstLead.RecordCount > 0)
                    objDLFunctionLib.DeleteRecord(vntLeadId, modLead.strtLEAD_);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This property method sets\returns the Id of the current cached Lead record.
        /// Descriptions:
        /// </summary>
        /// <returns>Id of current cached Lead record.</returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        /// </history>
        protected object id
        {
            get
            {
                return mvntLeadId;
            }

            set
            {
                mvntLeadId = value;
            }
        }

        /// <summary>
        /// Not inplemented ..dummy.
        /// </summary>
        /// <param name="rstRecord">Recordset</param>
        /// <returns></returns>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       5/10/2006   PPhilip   Converted to .Net C# code.
        /// </history>
        public virtual void DupDistribute(Recordset rstRecord)
        {
            return;
        }

        /// <summary>
        /// Reassigns activities from lead to contact.
        /// </summary>
        /// <param name="leadId">Lead ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       7/20/06     JH        Merged 3.7 SP1 in.
        /// </history>
        protected virtual void ReAssignActivities(object leadId, object contactId)
        {
            try
            {
                Recordset rstActivities;
                DataAccess objDataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                rstActivities = objDataAccess.GetRecordset(modLead.strqACTIVITIES_FOR_LEAD, 1, leadId,
                    modLead.strfCONTACT, modLead.strfLEAD_ID);
                if (rstActivities.RecordCount > 0)
                {
                    rstActivities.MoveFirst();
                }
                while (!rstActivities.EOF)
                {
                    rstActivities.Fields[modLead.strfCONTACT].Value = contactId;
                    rstActivities.Fields[modLead.strfLEAD_ID].Value = DBNull.Value;
                    rstActivities.MoveNext();
                }

                objDataAccess.SaveRecordset(modLead.strRN_APPOINTMENTS_TABLE, rstActivities);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Reassigns alerts from Lead to Contact.
        /// </summary>
        /// <param name="leadId">Lead ID</param>
        /// <param name="contactId">Contact ID</param>
        /// <history>
        /// Revision#     Date        Author    Note
        /// 3.8.0.0       7/20/06     JH        Merged 3.7 SP1 in.
        /// </history>
        protected virtual void ReAssignAlerts(object leadId, object contactId)
        {
            try
            {
                Recordset rstAlerts;
                DataAccess objDataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                rstAlerts = objDataAccess.GetRecordset(modLead.strqALERTS_WITH_LEAD, 1, leadId,
                    modLead.strfCONTACT_ID, modLead.strfLEAD_ID);
                if (rstAlerts.RecordCount > 0)
                {
                    rstAlerts.MoveFirst();
                }
                while (!rstAlerts.EOF)
                {
                    rstAlerts.Fields[modLead.strfCONTACT_ID].Value = contactId;
                    rstAlerts.Fields[modLead.strfLEAD_ID].Value = DBNull.Value;
                    rstAlerts.MoveNext();
                }

                objDataAccess.SaveRecordset(modLead.strtALERT, rstAlerts);

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        // Name:    IP_Update_Project_Registration
        // Desc:    Updates project registration records from lead Id to contact id when lead is converted to contact
        //          since lead record is deleted after contact is created from doleadsimple process, i'm nulling out lead id in PR
        // Ver#     Date        Author  Desc:
        // 5.9.0    6/4/10      KA      Initial Version
        protected virtual void IP_Update_Project_Registration(object vntLeadId, object vntContactId)
        {
            try
            {
                DataAccess objDataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstPR = objDataAccess.GetLinkedRecordset("TIC_Project_Registration", "TIC_Lead_Id", vntLeadId,
                                    "TIC_Contact_Id", "TIC_Lead_Id");
                if (rstPR.RecordCount > 0)
                {
                    while (rstPR.EOF == false)
                    {
                        rstPR.Fields["TIC_Contact_Id"].Value = vntContactId;
                        rstPR.Fields["TIC_Lead_Id"].Value = System.DBNull.Value;
                        rstPR.MoveNext();
                    }
                    objDataAccess.SaveRecordset("TIC_Project_Registration", rstPR);
                }

            }

            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        
        #endregion
    }
}
