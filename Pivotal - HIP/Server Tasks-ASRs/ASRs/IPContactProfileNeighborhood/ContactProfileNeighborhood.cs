using System;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;

using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;

namespace CRM.Pivotal.IP
{
    public class ContactProfileNeighborhood : IRFormScript
    {

        /// <summary>
        /// This module implements all the business rules for ContactProfileNeighborhood
        /// </summary>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        private IRSystem7 mrsysSystem = null;

        protected IRSystem7 RSysSystem
        {
            get { return mrsysSystem; }
            set { mrsysSystem = value; }
        }

        private object mvntContactId = DBNull.Value;
        
        private ILangDict grldtLangDict = null;

        public ILangDict RldtLangDict
        {
            get { return grldtLangDict; }
            set { grldtLangDict = value; }
        }


        /// <summary>
        /// This function loads the Contact Profile NBHD
        /// </summary>
        /// <param name="RecordId">Holds Card Profile Id</param>
        /// <param name="ParameterList">Argument Lists</param>
        /// <returns> array containing form data </returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// 5.9.0.0     9/6/2006    DYin        Fix Issue: 1. Id can not be null, 2. ParameterList has to be passed to 
        ///                                     TransitionPointParameter instance. 3. Initialize Id by null.
        /// </history>
        public virtual object LoadFormData(IRForm pform, object RecordId, ref object ParameterList)
        {
            try
            {
                bool NBHDProfileExists = (!(RSysSystem.Tables[modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD].Fields[modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID].Index(RecordId) == DBNull.Value));
                TransitionPointParameter ocmsparams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();

                ocmsparams.ParameterList = ParameterList;
                object matchedNBHDProfID = DBNull.Value;
                if (ocmsparams.UserDefinedParametersNumber > 0)
                {
                    matchedNBHDProfID = ocmsparams.GetUserDefinedParameter(1);
                }

                if ((!NBHDProfileExists) && (matchedNBHDProfID != DBNull.Value))
                    return pform.DoLoadFormData(matchedNBHDProfID, ref ParameterList);
                else
                    return pform.DoLoadFormData(RecordId, ref ParameterList);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine adds a new record to a given secondary.
        /// Assumptions:
        /// </summary>
        /// <param name="SecondaryName">Secondary Name for Opportunity form</param>
        /// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
        /// <param name="Outputs:ParameterList">Transit Point Parameters passed from the AppServer to client</param>
        /// <param name="Recordset">Variant array of recordsets of Lead form data</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        public virtual void NewSecondaryData(IRForm pform, object SecondaryName, ref object ParameterList, ref Recordset
            Recordset)
        {
            try
            {

                pform.DoNewSecondaryData(SecondaryName, ref ParameterList, Recordset);

                return;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sets a reference to the Pivotal System
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        public virtual void SetSystem(RSystem rsysSystem)
        {
            try
            {
                RSysSystem = (IRSystem7)rsysSystem;

                RldtLangDict = RSysSystem.GetLDGroup(modContactProfileNeighborhood.strgCONTACT_PROFILE_NBHD);
                mvntContactId = System.DBNull.Value;
                return;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine deletes the Contact Profile NBHD
        /// </summary>
        /// <param name="RecordId">Record Id for Card Profile</param>
        /// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        public virtual void DeleteFormData(IRForm pform, object RecordId, ref object ParameterList)
        {
            object vntContactId = DBNull.Value;            
            
            try
            {
                DataAccess objDLFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                vntContactId = objDLFunctionLib.GetRecordset(RecordId, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD, modContactProfileNeighborhood.strfCONTACT_ID);

                string infoMessage;

                if (CanBeDeleted(pform, RecordId, out infoMessage))
                {
                    CascadeDelete(RecordId);
                    // Delete the Contact Profile NBHD
                    object returnValue = null;
                    pform.DoDeleteFormData(RecordId, ref returnValue);
                    // udpate the contact dates if any neighborhood profiles are deleted
                    if ((vntContactId is Array))
                    {
                        UpdateContactDates(vntContactId);
                    }
                }
                else
                {
                    TransitionPointParameter objTransitPointParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();

                    objTransitPointParams.ParameterList = ParameterList;

                    if (!(objTransitPointParams.HasValidParameters()))
                    {
                        objTransitPointParams.Construct();
                    }

                    if (infoMessage.Length > 0)
                        objTransitPointParams.InfoMessage = infoMessage;

                    ParameterList = objTransitPointParams.ParameterList;
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function Saves the new Contact Profile NBHD form data to the database.
        /// The current user is added as a team member, visit log is added, and neighborhood profile type
        /// is updated.
        /// </summary>
        /// <returns>Record Id</returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// 5.9.0       9/2/2010    KA          added call to ip_manage_interest
        /// </history>
        public virtual object AddFormData(IRForm pform, object Recordsets, ref object ParameterList)
        {
            Recordset rstNBHDProfile = null;
            Recordset rstVisitLog = null;
            Recordset rstCTM = null;
            object[] parameterArray = null;
            object ContactProfileNeighborhoodID = DBNull.Value;
            try
            {
                parameterArray = (object[])Recordsets;
                rstNBHDProfile = (Recordset)parameterArray[0];
                rstVisitLog = pform.SecondaryFromVariantArray(Recordsets, modContactProfileNeighborhood.strsVISIT_LOG);
                rstCTM = pform.SecondaryFromVariantArray(Recordsets, modContactProfileNeighborhood.strsSALES_TEAM);

                if (!(Convert.IsDBNull(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfFIRST_VISIT_DATE].Value)) || (rstNBHDProfile.Fields[modContactProfileNeighborhood.strfFIRST_VISIT_DATE].Value
                    == null))
                {
                    pform.DoNewSecondaryData(modContactProfileNeighborhood.strsVISIT_LOG, ref ParameterList, rstVisitLog);
                    rstVisitLog.Fields[modContactProfileNeighborhood.strfAPPT_DATE].Value = DateTime.Today;
                    rstVisitLog.Fields[modContactProfileNeighborhood.strfSTART_TIME].Value = DateTime.Now;
                    rstVisitLog.Fields[modContactProfileNeighborhood.strfRN_EMPLOYEE_ID].Value = RSysSystem.Tables[modContactProfileNeighborhood.strtEMPLOYEE].Fields[modContactProfileNeighborhood.strfRN_EMPLOYEE_USER_ID].Find(RSysSystem.CurrentUserId());
                    rstVisitLog.Fields[modContactProfileNeighborhood.strfACTIVITY_COMPLETE].Value = true;
                    rstVisitLog.Fields[modContactProfileNeighborhood.strfACTIVITY_TYPE].Value = 7;
                    rstVisitLog.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value;
                    rstVisitLog.Fields[modContactProfileNeighborhood.strfCONTACT].Value = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfCONTACT_ID].Value;
                    rstVisitLog.Fields[modContactProfileNeighborhood.strfLEAD_ID].Value = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfLEAD_ID].Value;
                    rstVisitLog.Fields[modContactProfileNeighborhood.strfASSIGNED_BY].Value = RSysSystem.Tables[modContactProfileNeighborhood.strtEMPLOYEE].Fields[modContactProfileNeighborhood.strfRN_EMPLOYEE_USER_ID].Find(RSysSystem.CurrentUserId());
                    rstVisitLog.Fields[modContactProfileNeighborhood.strfAPPT_DESCRIPTION].Value = "Visit Log: " + RSysSystem.Tables[modContactProfileNeighborhood.strtCONTACT].Fields[modContactProfileNeighborhood.strfFULL_NAME].Index(rstVisitLog.Fields[modContactProfileNeighborhood.strfCONTACT].Value) + " at " + RSysSystem.Tables[modContactProfileNeighborhood.strtNEIGHBORHOOD].Fields[modContactProfileNeighborhood.strfNAME].Index(rstVisitLog.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value);

                }

                ContactProfileNeighborhoodID = pform.DoAddFormData(Recordsets, ref ParameterList);
                UpdateNBHDProfieTeam(ContactProfileNeighborhoodID, RSysSystem.UserProfile.EmployeeId);
                UpdateNBHDPType(ContactProfileNeighborhoodID);

                TransitionPointParameter objTransitPointParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objTransitPointParams.ParameterList = ParameterList;
                if (!(objTransitPointParams.HasValidParameters()))
                {
                    objTransitPointParams.Construct();
                }
                IP_Manage_Interest(rstNBHDProfile);
                ParameterList = objTransitPointParams.SetUserDefinedParameter(0, mvntContactId);

                return ContactProfileNeighborhoodID;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Saving the Contact Profile NBHD.
        /// </summary>
        /// <param name="Recordsets">Variant array of recordsets of Card Profile form data</param>
        /// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        public virtual void SaveFormData(IRForm pform, object Recordsets, ref object ParameterList)
        {
            object vntContactProfileNeighborhoodID = DBNull.Value;            Recordset rstNBHDProfile = null;
            Recordset rstVisitLog = null;
            object vntFirstVisitDate = null;
            IRSystem5 rdaSystem = null;
            Recordset rstChgNBHDPTeam = null;
            object[] parameterArray = null;

            try
            {
                rdaSystem = RSysSystem;
                parameterArray = (object[])Recordsets;
                rstNBHDProfile = (Recordset)parameterArray[0];
                rstChgNBHDPTeam = pform.SecondaryFromVariantArray(Recordsets, modContactProfileNeighborhood.strsSALES_TEAM);
                rstVisitLog = pform.SecondaryFromVariantArray(Recordsets, modContactProfileNeighborhood.strsVISIT_LOG);
                vntContactProfileNeighborhoodID = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID].Value;
                if (!(Convert.IsDBNull(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfFIRST_VISIT_DATE].Value)) || (rstNBHDProfile.Fields[modContactProfileNeighborhood.strfFIRST_VISIT_DATE].Value
                    == null))
                {
                    vntFirstVisitDate = RSysSystem.Tables[modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD].Fields[modContactProfileNeighborhood.strfFIRST_VISIT_DATE].Index(vntContactProfileNeighborhoodID);
                    if (Convert.IsDBNull(vntFirstVisitDate) || (vntFirstVisitDate == null))
                    {
                        // The first visit date is not existed in the database
                        pform.DoNewSecondaryData(modContactProfileNeighborhood.strsVISIT_LOG, ref ParameterList, rstVisitLog);
                        rstVisitLog.Fields[modContactProfileNeighborhood.strfAPPT_DATE].Value = DateTime.Today;
                        rstVisitLog.Fields[modContactProfileNeighborhood.strfSTART_TIME].Value = DateTime.Now;
                        rstVisitLog.Fields[modContactProfileNeighborhood.strfRN_EMPLOYEE_ID].Value = RSysSystem.Tables[modContactProfileNeighborhood.strtEMPLOYEE].Fields[modContactProfileNeighborhood.strfRN_EMPLOYEE_USER_ID].Find(RSysSystem.CurrentUserId());
                        rstVisitLog.Fields[modContactProfileNeighborhood.strfACTIVITY_COMPLETE].Value = true;
                        rstVisitLog.Fields[modContactProfileNeighborhood.strfACTIVITY_TYPE].Value = 7;
                        rstVisitLog.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value;
                        rstVisitLog.Fields[modContactProfileNeighborhood.strfLEAD_ID].Value = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfLEAD_ID].Value;
                        rstVisitLog.Fields[modContactProfileNeighborhood.strfCONTACT].Value = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfCONTACT_ID].Value;
                        rstVisitLog.Fields[modContactProfileNeighborhood.strfASSIGNED_BY].Value = RSysSystem.Tables[modContactProfileNeighborhood.strtEMPLOYEE].Fields[modContactProfileNeighborhood.strfRN_EMPLOYEE_USER_ID].Find(RSysSystem.CurrentUserId());
                        rstVisitLog.Fields[modContactProfileNeighborhood.strfAPPT_DESCRIPTION].Value = "Visit Log: " + RSysSystem.Tables[modContactProfileNeighborhood.strtCONTACT].Fields[modContactProfileNeighborhood.strfFULL_NAME].Index(rstVisitLog.Fields[modContactProfileNeighborhood.strfCONTACT].Value) + " at " + RSysSystem.Tables[modContactProfileNeighborhood.strtNEIGHBORHOOD].Fields[modContactProfileNeighborhood.strfNAME].Index(rstVisitLog.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value);

                    }
                }
                object returnValue = null;
                pform.DoSaveFormData(Recordsets, ref returnValue);

                // Inactivate team members
                if (rstChgNBHDPTeam.RecordCount > 0)
                {
                    InactivateContactProfileNeighborhood objInactivateContactNeighborhoodProfile = (InactivateContactProfileNeighborhood)RSysSystem.ServerScripts[modContactProfileNeighborhood.strsINACTIVATE_NBHD_PROFILE].CreateInstance();
                    rstChgNBHDPTeam.MoveFirst();
                    while (!(rstChgNBHDPTeam.EOF))
                    {
                        if (Convert.ToBoolean(rstChgNBHDPTeam.Fields[modContactProfileNeighborhood.strfINACTIVE].Value))
                        {
                            objInactivateContactNeighborhoodProfile.InactivateATeamMember(rstChgNBHDPTeam.Fields[modContactProfileNeighborhood.strfMEMBER_TEAM_MEMBER_ID].Value);
                        }
                        rstChgNBHDPTeam.MoveNext();
                    }
                }

                UpdateNBHDPType(vntContactProfileNeighborhoodID);

                TransitionPointParameter objTransitPointParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objTransitPointParams.ParameterList = ParameterList;
                if (!(objTransitPointParams.HasValidParameters()))
                {
                    objTransitPointParams.Construct();
                }
                ParameterList = objTransitPointParams.SetUserDefinedParameter(0, mvntContactId);

                if (Convert.ToBoolean( rstNBHDProfile.Fields["Inactive"].UnderlyingValue) == true 
                    && Convert.ToBoolean(rstNBHDProfile.Fields["Inactive"].Value) == false)
                {
                    IP_Manage_Interest(rstNBHDProfile);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function gets new Contact Profile NBHD.
        /// </summary>
        /// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
        /// <returns>Variant array of recordsets of Card Profile</returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        public virtual object NewFormData(IRForm pform, ref object ParameterList)
        {
            Recordset rstCPN = null;
            object vntCPN = null;
            Recordset rstSalesTeam = null;


            try
            {

                vntCPN = pform.DoNewFormData(ref ParameterList);
                object[] recordsetArray = (object[])vntCPN;
                rstCPN = (Recordset)recordsetArray[0];
                rstSalesTeam = pform.SecondaryFromVariantArray(vntCPN, modContactProfileNeighborhood.strsSALES_TEAM);

                if (!((ParameterList == null)) && !(Convert.IsDBNull(ParameterList)))
                {
                    TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                    objParam.ParameterList = ParameterList;
                    if (objParam.HasValidParameters())
                    {
                        objParam.SetDefaultFields(rstCPN);
                    }
                }

                rstCPN.Fields[modContactProfileNeighborhood.strfLEAD_DATE].Value = DateTime.Today;
                object returnValue = null;
                pform.NewSecondaryData(modContactProfileNeighborhood.strsSALES_TEAM, ref returnValue, rstSalesTeam);
                rstSalesTeam.Fields[modContactProfileNeighborhood.strfEMPLOYEE_ID].Value = RSysSystem.Tables[modContactProfileNeighborhood.strtEMPLOYEE].Fields[modContactProfileNeighborhood.strfRN_EMPLOYEE_USER_ID].Find(RSysSystem.CurrentUserId());
                rstSalesTeam.Fields[modContactProfileNeighborhood.strfINACTIVE].Value = false;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
            return vntCPN;
        }

        /// <summary>
        /// Allows client scripts to check duplicates, check if a NP can be inactivated, and determine
        /// </summary>
        /// first interenet date.
        /// <param name="pForm">IRform object references to the client IRForm object</param>
        /// <param name="MethodName">Method name to be executed</param>
        /// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {
            string strErrMsg = String.Empty;
            bool blnDuplicate = false;
            try
            {

                TransitionPointParameter objInstance = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objInstance.ParameterList = ParameterList;
                object[] parameterArray = objInstance.GetUserDefinedParameterArray();

                switch (MethodName)
                {
                    case modContactProfileNeighborhood.strmPOSSIBLE_DUPLICATES:
                        blnDuplicate = DuplicateNBHDProfile(parameterArray[0], parameterArray[1], parameterArray[2], parameterArray[3]);
                        parameterArray = new object[] { blnDuplicate };
                        break;
                    case modContactProfileNeighborhood.strmCAN_BE_INACTIVE:
                        if (parameterArray.Length == 0)
                        {
                            parameterArray = new object[] { CanBeInactive(parameterArray[0], true) };
                        }
                        else if (parameterArray.Length >= 1)
                        {
                            parameterArray = new object[] { CanBeInactive(parameterArray[0], parameterArray[1]) };
                        }
                        break;
                    case modContactProfileNeighborhood.strmGET_EARLIEST_INTERNET_DATE_OF_NBHD_PROFILE_FOR_CONTACT:
                        parameterArray = new object[] { GetEarliestInternetDateOfNBHDProfileForContact(parameterArray[0]) };
                        break;
                    case modContactProfileNeighborhood.strmFIND_CONTNBHDPROFILE:
                        parameterArray = new object[] { FindContNBHDProfile(parameterArray[0], parameterArray[1]) };
                        break;
                    default:
                        string message = RSysSystem.GetLDGroup(ErrorsLDGroupData.ErrorsLDGroupName).GetTextSub
                            (ErrorsLDGroupData.MethodNotDefinedLDLookupName, new object[] { MethodName, pForm.FormTitle })
                            .ToString();
                        throw new PivotalApplicationException(message);
                }
                ParameterList = objInstance.SetUserDefinedParameterArray(parameterArray);

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function is used to delete secondary records linked to the ContProfileNBHD.
        /// </summary>
        /// <param name="vntBARId">Contact Profile NBHD Id</param>
        /// <returns></returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        protected virtual void CascadeDelete(object vntContProfileNBHD_Id)
        {
            Recordset rstCTM = null;

            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                // Deletes traffic source
                objLib.DeleteLinkedRecordset(modContactProfileNeighborhood.strtTRAFFIC_SOURCE, modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID,
                    vntContProfileNBHD_Id);

                // Deletes action plan history
                objLib.DeleteLinkedRecordset(modContactProfileNeighborhood.strtNBHDP_ACTION_PLAN_HISTORY, modContactProfileNeighborhood.strfNEIGHBORHOOD_PROFILE_ID,
                    vntContProfileNBHD_Id);

                // Deletes contact team member
                // Couldn't call the core library DL function because it doesn't work for tables with primary key field
                // name different from the table name.
                rstCTM = objLib.GetLinkedRecordset(modContactProfileNeighborhood.strtCONTACT_TEAM_MEMBER, modContactProfileNeighborhood.strfNEIGHBORHOOD_PROFILE_ID,
                    vntContProfileNBHD_Id, modContactProfileNeighborhood.strfMEMBER_TEAM_MEMBER_ID);
                if (rstCTM.RecordCount > 0)
                {
                    rstCTM.MoveFirst();
                    while (!(rstCTM.EOF))
                    {
                        objLib.DeleteRecord(rstCTM.Fields[modContactProfileNeighborhood.strfMEMBER_TEAM_MEMBER_ID].Value, modContactProfileNeighborhood.strtCONTACT_TEAM_MEMBER);
                        rstCTM.MoveNext();
                    }
                }

                // Deletes visit logs
                objLib.DeleteRecordset(modContactProfileNeighborhood.strqVISIT_LOGS_FOR_CONT_PROF_NBHD, modContactProfileNeighborhood.strfRN_APPOINTMENTS_ID,
                    vntContProfileNBHD_Id);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks to see if the form has the indicated secondaries.
        /// </summary>
        /// <param name="RecordId">Record Id</param>
        /// <param name="ParameterList">Transition Point Parameters</param>
        /// <returns>
        /// True if the form has no children
        /// False if the form has children
        /// </returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        protected virtual bool CanBeDeleted(IRForm pForm, object vntRecordId, out string infoMessage)
        {
            try
            {
                object strItem = null;
                int i = 0;
                bool canBeDeleted = false;
                object vntParameterList = System.DBNull.Value;

                // Get the Form
                object vntForm = pForm.DoLoadFormData(vntRecordId, ref vntParameterList);
                object[] recordsetArray = (object[])vntForm;

                // Set up the Segments array
                string[] arrSegments = new string[] { modContactProfileNeighborhood.strsVISIT_LOGS };

                while (Convert.ToString(strItem).Length == 0 && i < arrSegments.Length)
                {
                    canBeDeleted = !SecondaryExists(pForm, vntForm, arrSegments[i], ref strItem);
                    i++;
                }

                if (!canBeDeleted)
                {
                    infoMessage = TypeConvert.ToString(RldtLangDict.GetTextSub(modContactProfileNeighborhood.strdDELETION_CANCELED, new object[] { strItem }));
                }
                else
                {
                    infoMessage = string.Empty;
                }

                return canBeDeleted;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks to see if a secondary exists.
        /// </summary>
        /// <param name="vntForm">Form collection</param>
        /// <param name="strSection">Section name</param>
        /// <param name="str">Section where item was found</param>
        /// <returns>
        /// True if a secondary was found
        /// False if no secondary was found
        /// </returns> 
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        protected virtual bool SecondaryExists(IRForm pform, object vntForm, string strSection, ref object strItem)
        {
            bool secondaryExists = false;
            Recordset rstForm_Secondary = null;


            secondaryExists = false;

            rstForm_Secondary = pform.SecondaryFromVariantArray(vntForm, strSection);
            if (rstForm_Secondary.RecordCount > 0)
            {
                strItem = strSection;
                secondaryExists = true;
            }

            return secondaryExists;
        }

        /// <summary>
        /// Calculates the Neighborhood Profile Type field.
        /// then set the Neighborhood Profile Type="Lost Opportunity".
        /// Any other Inactive Reasons sets the Type="Inactive".
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        ///             7/20/2006   JH          Merged in 3.7 sp1 code.
        /// </history>
        public virtual void UpdateNBHDPType(object vntContactProfileNeighborhoodID)
        {
            Recordset rstContactProfileNeighborhood = null;
            Recordset rstCTM = null;
            Recordset rstVisitLog = null;
            Recordset rstMktLvlNBHR = null;
            object vntMktLvlNBHRId = DBNull.Value;            object vntNeighborhoodID = DBNull.Value;            string strType = String.Empty;
            string strTypeChangeTo = String.Empty;
            bool blnInactive = false;
            object vntInactiveReasonID = DBNull.Value;            string strInactiveReason = String.Empty;
            Recordset rstQuote = null;
            object vntContactId = DBNull.Value;            bool blnFound = false;

            try
            {
                mvntContactId = System.DBNull.Value;

                DataAccess objDLFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                UIAccess objPLFunctionLib = (UIAccess)RSysSystem.ServerScripts[AppServerRuleData.UIAccessAppServerRuleName].CreateInstance();
                rstContactProfileNeighborhood = objDLFunctionLib.GetRecordset(vntContactProfileNeighborhoodID, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD,
                    modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strfLEAD_ID, modContactProfileNeighborhood.strfNEIGHBORHOOD_ID,
                    modContactProfileNeighborhood.strfINACTIVE, modContactProfileNeighborhood.strfINACTIVE_REASON_ID, modContactProfileNeighborhood.strfINTERNET_DATE,
                    modContactProfileNeighborhood.strfCONTACT_ID, modContactProfileNeighborhood.strfDIVISION_ID, modContactProfileNeighborhood.strfFIRST_VISIT_DATE);
                rstCTM = objDLFunctionLib.GetLinkedRecordset(modContactProfileNeighborhood.strtCONTACT_TEAM_MEMBER, modContactProfileNeighborhood.strfNEIGHBORHOOD_PROFILE_ID,
                    vntContactProfileNeighborhoodID, modContactProfileNeighborhood.strfMEMBER_TEAM_MEMBER_ID);
                rstVisitLog = objDLFunctionLib.GetRecordset(modContactProfileNeighborhood.strqVISIT_LOGS_FOR_NBHD_PROFILE, 1, vntContactProfileNeighborhoodID,
                    modContactProfileNeighborhood.strfRN_APPOINTMENTS_ID);

                if (rstContactProfileNeighborhood.RecordCount == 0)
                {
                    return;
                }

                vntNeighborhoodID = rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value;
                strType = TypeConvert.ToString(rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfTYPE].Value);
                vntContactId = rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfCONTACT_ID].Value;
                blnInactive = TypeConvert.ToBoolean(rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfINACTIVE].Value);
                vntInactiveReasonID = rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfINACTIVE_REASON_ID].Value;

                if (blnInactive)
                {
                    strInactiveReason = TypeConvert.ToString(RSysSystem.Tables[modContactProfileNeighborhood.strtINACTIVE_REASON].Fields[modContactProfileNeighborhood.strfREASON_CODE].Index(vntInactiveReasonID));
                    if (strInactiveReason == modContactProfileNeighborhood.strINACTIVE_REASON_PUR_ELSE)
                    {
                        strType = modContactProfileNeighborhood.strNBHDP_TYPE_LOST_OPP;  // Set Type="Lost Opportunity"
                    }
                    else
                    {
                        strType = modContactProfileNeighborhood.strNBHDP_TYPE_INACTIVE; // Set Type="Inactive"
                    }
                    rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfTYPE].Value = strType;
                    // Update the value in the type field
                    objDLFunctionLib.SaveRecordset(modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD, rstContactProfileNeighborhood);
                    return;
                }
                else
                {
                    rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfINACTIVE_REASON_ID].Value = System.DBNull.Value;
                }

                strTypeChangeTo = strType;

                if (!(Convert.IsDBNull(vntContactId)))
                {
                    // If this profile is based on a contact record....
                    // Buyer
                    rstQuote = objDLFunctionLib.GetRecordset("HB: Active Contract for Contact? Neighborhood?", 2, vntContactId, vntNeighborhoodID, modContactProfileNeighborhood.strfOPPORTUNITY_ID);
                    if (rstQuote.RecordCount > 0)
                    {
                        strTypeChangeTo = objPLFunctionLib.GetComboChoiceText("Buyer", modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD);
                        blnFound = true;
                    }

                    // Prospect
                    if (!blnFound)
                    {
                        rstQuote = objDLFunctionLib.GetRecordset("HB: Active Quotes For Contact? Neighborhood?", 2, vntContactId, vntNeighborhoodID, modContactProfileNeighborhood.strfOPPORTUNITY_ID);
                        if (rstQuote.RecordCount > 0)
                        {
                            strTypeChangeTo = objPLFunctionLib.GetComboChoiceText("Prospect", modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD);
                            blnFound = true;
                        }
                    }

                    // Cancelled
                    if (!blnFound)
                    {
                        rstQuote = objDLFunctionLib.GetRecordset("HB: Closed Quote for Contact? Neighborhood?", 2, vntContactId, vntNeighborhoodID, modContactProfileNeighborhood.strfOPPORTUNITY_ID);
                        if (rstQuote.RecordCount > 0)
                        {
                            strTypeChangeTo = objPLFunctionLib.GetComboChoiceText("Closed", modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD);
                            blnFound = true;
                        }
                    }

                    // Closed
                    if (!blnFound)
                    {
                        rstQuote = objDLFunctionLib.GetRecordset("HB: Canceled Quote for Contact? Neighborhood?", 2, vntContactId, vntNeighborhoodID, modContactProfileNeighborhood.strfOPPORTUNITY_ID);
                        if (rstQuote.RecordCount > 0)
                        {
                            strTypeChangeTo = objPLFunctionLib.GetComboChoiceText("Cancelled", modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD);
                            blnFound = true;
                        }
                    }
                }

                if (!blnFound)
                {
                    vntMktLvlNBHRId = System.DBNull.Value;
                    rstMktLvlNBHR = objDLFunctionLib.GetRecordset(modContactProfileNeighborhood.streMKT_LVL_NBHD_OF_DIVISION, 1,
                        rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfDIVISION_ID].Value, modContactProfileNeighborhood.strfNEIGHBORHOOD_ID);
                    if (rstMktLvlNBHR.RecordCount > 0)
                    {
                        rstMktLvlNBHR.MoveFirst();
                        vntMktLvlNBHRId = rstMktLvlNBHR.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value;
                    }

                    // Prospect
                    // 09/21/2005 JWang When converting Lead to Contact, the First_Visit_Date is not null then set type
                    // as Prospect
                    if (rstVisitLog.RecordCount > 0 || !(Convert.IsDBNull(rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfFIRST_VISIT_DATE].Value)))
                    {
                        strTypeChangeTo = objPLFunctionLib.GetComboChoiceText("Prospect", modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD);

                        // RY: set type "Lead"
                    }
                    else if (Convert.IsDBNull(rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfINTERNET_DATE].Value))
                    {
                        strTypeChangeTo = objPLFunctionLib.GetComboChoiceText("Lead", modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD);
                    }
                    else if (RSysSystem.EqualIds(vntNeighborhoodID, vntMktLvlNBHRId))
                    {
                        // Neighborhood is not specified
                        if (rstCTM.RecordCount == 0)
                        {
                            // Account Manager is not specified
                            strTypeChangeTo = objPLFunctionLib.GetComboChoiceText("UA Mkt Lead", modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD);
                        }
                        else
                        {
                            // Account Manager is specified
                            strTypeChangeTo = objPLFunctionLib.GetComboChoiceText("Mkt Lead", modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD);
                        }

                    }
                    else
                    {
                        // Neighborhood is specified
                        if (rstCTM.RecordCount == 0)
                        {
                            // Account Manager is not specified
                            strTypeChangeTo = objPLFunctionLib.GetComboChoiceText("UA Nbhd Lead", modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD);
                        }
                        else
                        {
                            // Account Manager is specified
                            strTypeChangeTo = objPLFunctionLib.GetComboChoiceText("Nbhd Lead", modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD);
                        }

                    }
                }
                if (strType != strTypeChangeTo)
                {
                    // update the value in the type field
                    rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfTYPE].Value = strTypeChangeTo;
                    rstContactProfileNeighborhood = objDLFunctionLib.SaveRecordset(modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD, rstContactProfileNeighborhood);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Checks whether duplicated NBHD Profiles exist for a particular Contact or Lead, and a neighborhood.
        /// </summary>
        /// Only need to specify one of Contact Id and Lead Id.
        /// <param name="vntContactId">Record Id for the Contact</param>
        /// <param name="vntLeadId">Record Id for the Lead</param>
        /// <param name="vntNeighborhoodId">Record Id for the neighborhood</param>
        /// <param name="vntContactNBHDProfileId">Record Id for the Contact NBHD Profile</param>
        /// <returns>
        /// True - possible duplicated NBHD Profile
        /// False - not a duplicated NBHD Profile</returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        protected virtual bool DuplicateNBHDProfile(object vntContactId, object vntLeadId, object vntNeighborhoodID, object vntContactNBHDProfileId)
        {
            bool duplicateNBHDProfile = false;
            Recordset rstDuplicated = null;

            try
            {
                duplicateNBHDProfile = false;

                DataAccess objFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                if (!(Convert.IsDBNull(vntContactId)))
                {
                    rstDuplicated = objFunctionLib.GetRecordset(modContactProfileNeighborhood.strqNBHD_PROFILE_FOR_CONTACT__NBHD,
                        2, vntContactId, vntNeighborhoodID, modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID);
                }
                else
                {
                    rstDuplicated = objFunctionLib.GetRecordset(modContactProfileNeighborhood.strqNBHD_PROFILE_FOR_LEAD__NBHD,
                        2, vntLeadId, vntNeighborhoodID, modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID);
                }

                if (rstDuplicated.RecordCount > 1)
                {
                    duplicateNBHDProfile = true;
                }
                else if (rstDuplicated.RecordCount == 1)
                {
                    rstDuplicated.MoveFirst();
                    if (!(RSysSystem.EqualIds(vntContactNBHDProfileId, rstDuplicated.Fields[modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID].Value)))
                    {
                        duplicateNBHDProfile = true;
                    }
                }

                return duplicateNBHDProfile;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Returns a recordset of contact neighborhood profiles for a particular contact and neighborhood.
        /// </summary>
        /// <param name="contactId">Record Id for the Contact</param>
        /// <param name="neighborhoodId">Record Id for the neighborhood</param>
        /// <returns>Recordset - neighborhood profiles</returns>
        protected virtual Recordset FindContNBHDProfile(object contactId, object neighborhoodId)
        {
            try
            {
                DataAccess objFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstDuplicated = objFunctionLib.GetRecordset(modContactProfileNeighborhood.strqNBHD_PROFILE_FOR_CONTACT__NBHD,
                    2, contactId, neighborhoodId, modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID);

                return rstDuplicated;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// When "Buyer Is Global Stage" flag is set to true for a Quote's division then on Convert to Sale,
        /// Neighborhood Profile where the Neighborhood is in the same Division
        /// as the sale's Division. For each of these NBHD Profiles,
        /// if the type is not "Buyer" or "Closed" or "Canceled" or
        /// No Reservation Date or The Reservation Expiration date less than = current date,
        /// then
        /// </summary>
        /// updates the neighborhood file.
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// 5.9.0.0     9/6/2006    DYin        Fix Issue: invalid cast to make Convert to Sale falied in Quote
        /// </history>
        public virtual void GlobalBuyerSale(object vntOpportunityID)
        {
            Recordset rstNBHDProfile = null;
            Recordset rstNBHDP_Team = null;
            Recordset rstOpportunity = null;
            object vntNeighborhoodID = DBNull.Value;            object vntOppContactID = DBNull.Value;            object vntNBHDPhaseID = DBNull.Value;            object vntLotID = DBNull.Value;            object vntPlanNameID = DBNull.Value;            object vntElevationID = DBNull.Value;            decimal curQuoteTotal = 0;
            string strOppFirstName = String.Empty;
            string strOppLastName = String.Empty;
            string strOppPhone = String.Empty;
            Recordset rstContact = null;
            Recordset rstLot = null;
            string strLotRnDescriptor = String.Empty;
            string strLotNumber = String.Empty;
            string strLotPhase = String.Empty;
            string strLotBlock = String.Empty;
            string strLotTract = String.Empty;
            string strLotBuilding = String.Empty;
            string strLotUnit = String.Empty;
            string strLotJobNumber = String.Empty;
            string strLotStatus = String.Empty;
            Recordset rstRelease = null;
            string strReleaseRnDescriptor = String.Empty;
            string strReleaseName = String.Empty;
            Recordset rstNBHDPProduct = null;
            string strPlanCode = String.Empty;
            string strElevationCode = String.Empty;
            Recordset rstNeighborhood = null;
            string strNeighborhoodName = String.Empty;
            string strSaleFirstName = String.Empty;
            string strSaleLastName = String.Empty;
            string strSaleWorkEmail = String.Empty;
            string strSaleWorkPhone = String.Empty;
            Recordset rstEmployee = null;
            string strOtherNbhdName = String.Empty;
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                InactivateContactProfileNeighborhood objInactivateContactNeighborhoodProfile = (InactivateContactProfileNeighborhood)RSysSystem.ServerScripts[modContactProfileNeighborhood.strsINACTIVATE_NBHD_PROFILE].CreateInstance();

                // first get all source information for the quote Quote being sold
                // quote
                rstOpportunity = objLib.GetRecordset(vntOpportunityID, modContactProfileNeighborhood.strtOPPORTUNITY, modContactProfileNeighborhood.strfCONTACT_ID,
                    modContactProfileNeighborhood.strfLOT_ID, modContactProfileNeighborhood.strfPLAN_NAME_ID, modContactProfileNeighborhood.strfELEVATION_ID,
                    modContactProfileNeighborhood.strfNBHD_PHASE_ID, modContactProfileNeighborhood.strfNEIGHBORHOOD_ID, modContactProfileNeighborhood.strfQUOTE_TOTAL);
                vntOppContactID = rstOpportunity.Fields[modContactProfileNeighborhood.strfCONTACT_ID].Value;
                vntLotID = rstOpportunity.Fields[modContactProfileNeighborhood.strfLOT_ID].Value;
                vntPlanNameID = rstOpportunity.Fields[modContactProfileNeighborhood.strfPLAN_NAME_ID].Value;
                vntElevationID = rstOpportunity.Fields[modContactProfileNeighborhood.strfELEVATION_ID].Value;
                vntNBHDPhaseID = rstOpportunity.Fields[modContactProfileNeighborhood.strfNBHD_PHASE_ID].Value;
                vntNeighborhoodID = rstOpportunity.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value;
                if (Convert.IsDBNull(rstOpportunity.Fields[modContactProfileNeighborhood.strfQUOTE_TOTAL].Value))
                    curQuoteTotal = 0M;
                else
                    curQuoteTotal = TypeConvert.ToDecimal(rstOpportunity.Fields[modContactProfileNeighborhood.strfQUOTE_TOTAL].Value);
                rstOpportunity.Close();

                // purchaser info from Contact table
                rstContact = objLib.GetRecordset(vntOppContactID, modContactProfileNeighborhood.strtCONTACT, modContactProfileNeighborhood.strfFIRST_NAME,
                    modContactProfileNeighborhood.strfLAST_NAME, modContactProfileNeighborhood.strfPHONE);
                strOppFirstName = rstContact.Fields[modContactProfileNeighborhood.strfFIRST_NAME].Value + "";
                strOppLastName = rstContact.Fields[modContactProfileNeighborhood.strfLAST_NAME].Value + "";
                strOppPhone = rstContact.Fields[modContactProfileNeighborhood.strfPHONE].Value + "";
                rstContact.Close();

                // lot info
                rstLot = objLib.GetRecordset(vntLotID, modContactProfileNeighborhood.strtPRODUCT, modContactProfileNeighborhood.strfRN_DESCRIPTOR,
                    modContactProfileNeighborhood.strfLOT_NUMBER, modContactProfileNeighborhood.strfDEVELOPMENT_PHASE, modContactProfileNeighborhood.strfBLOCK,
                    modContactProfileNeighborhood.strfTRACT, modContactProfileNeighborhood.strfBUILDING, modContactProfileNeighborhood.strfUNIT, modContactProfileNeighborhood.strfJOB_NUMBER,
                    modContactProfileNeighborhood.strfLOT_STATUS);
                strLotRnDescriptor = rstLot.Fields[modContactProfileNeighborhood.strfRN_DESCRIPTOR].Value + "";
                strLotNumber = rstLot.Fields[modContactProfileNeighborhood.strfLOT_NUMBER].Value + "";
                strLotPhase = rstLot.Fields[modContactProfileNeighborhood.strfDEVELOPMENT_PHASE].Value + "";
                strLotBlock = rstLot.Fields[modContactProfileNeighborhood.strfBLOCK].Value + "";
                strLotTract = rstLot.Fields[modContactProfileNeighborhood.strfTRACT].Value + "";
                strLotBuilding = rstLot.Fields[modContactProfileNeighborhood.strfBUILDING].Value + "";
                strLotUnit = rstLot.Fields[modContactProfileNeighborhood.strfUNIT].Value + "";
                strLotJobNumber = rstLot.Fields[modContactProfileNeighborhood.strfJOB_NUMBER].Value + "";
                strLotStatus = rstLot.Fields[modContactProfileNeighborhood.strfLOT_STATUS].Value + "";
                rstLot.Close();

                // release info
                rstRelease = objLib.GetRecordset(vntNBHDPhaseID, modContactProfileNeighborhood.strtNBHD_PHASE, modContactProfileNeighborhood.strfRN_DESCRIPTOR,
                    modContactProfileNeighborhood.strfPHASE_NAME);
                strReleaseRnDescriptor = rstRelease.Fields[modContactProfileNeighborhood.strfRN_DESCRIPTOR].Value + "";
                strReleaseName = rstRelease.Fields[modContactProfileNeighborhood.strfPHASE_NAME].Value + "";
                rstRelease.Close();

                // neighborhood info
                rstNeighborhood = objLib.GetRecordset(vntNeighborhoodID, modContactProfileNeighborhood.strtNEIGHBORHOOD, modContactProfileNeighborhood.strfNEIGHBORHOOD_NAME);
                strNeighborhoodName = rstNeighborhood.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_NAME].Value + "";
                rstNeighborhood.Close();

                // Plan/Elevation info
                rstNBHDPProduct = objLib.GetRecordset(vntPlanNameID, modContactProfileNeighborhood.strtNBHDP_PRODUCT, modContactProfileNeighborhood.strfPLAN_CODE,
                    modContactProfileNeighborhood.strfELEVATION_CODE);
                strPlanCode = rstNBHDPProduct.Fields[modContactProfileNeighborhood.strfPLAN_CODE].Value + "";
                strElevationCode = rstNBHDPProduct.Fields[modContactProfileNeighborhood.strfELEVATION_CODE].Value + "";
                rstNBHDPProduct.Close();

                // get all contact neighborhood profiles for related to this quote
                rstNBHDProfile = objLib.GetRecordset(modContactProfileNeighborhood.strqNBHD_PROFILE_FOR_OPPORTUNITY, 2, vntOpportunityID,
                    vntOpportunityID, modContactProfileNeighborhood.strfINACTIVE, modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strfNEIGHBORHOOD_ID,
                    modContactProfileNeighborhood.strfRESERVATION_DATE, modContactProfileNeighborhood.strfRESERVATION_EXPIRATION_DATE);
                if (!((rstNBHDProfile.BOF || rstNBHDProfile.EOF)))
                {
                    rstNBHDProfile.MoveFirst();
                    while (!(rstNBHDProfile.EOF))
                    {
                        if (RSysSystem.EqualIds(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value, vntNeighborhoodID))
                        {
                            // this is for the quote being sold - just update type
                            // UpdateNBHDPType .Fields(strfCONTACT_PROFILE_NBHD_ID).Value
                            // handled in Opportunity - COnvertToSale
                        }
                        else if (TypeConvert.ToString(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfTYPE].Value) == modContactProfileNeighborhood.strBUYER
                            || TypeConvert.ToString(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfTYPE].Value) == modContactProfileNeighborhood.strCLOSED
                            || TypeConvert.ToString(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfTYPE].Value) == modContactProfileNeighborhood.strCANCELLED)
                        {
                            // do not process
                        }
                        else if ((rstNBHDProfile.Fields[modContactProfileNeighborhood.strfRESERVATION_DATE].Value.GetType() ==
                            typeof(DateTime)) && !(Convert.IsDBNull(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfRESERVATION_EXPIRATION_DATE].Value))
                            && Convert.ToDouble(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfRESERVATION_EXPIRATION_DATE].Value)
                            <= DateTime.Today.ToOADate())
                        {
                            // do not process
                        }
                        else
                        {
                            // send email to all sales team members for this profile - before we inactivate them
                            rstNBHDP_Team = objLib.GetRecordset(modContactProfileNeighborhood.strqACTIVE_SALE_TEAM_FOR_CONTACT_NBHDP,
                                1, rstNBHDProfile.Fields[modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID].Value, modContactProfileNeighborhood.strfEMPLOYEE_ID);
                            if (!((rstNBHDP_Team.BOF || rstNBHDP_Team.EOF)))
                            {
                                rstNBHDP_Team.MoveFirst();
                                while (!(rstNBHDP_Team.EOF))
                                {
                                    rstEmployee = objLib.GetRecordset(rstNBHDP_Team.Fields[modContactProfileNeighborhood.strfEMPLOYEE_ID].Value,
                                        modContactProfileNeighborhood.strtEMPLOYEE, modContactProfileNeighborhood.strfFIRST_NAME, modContactProfileNeighborhood.strfLAST_NAME,
                                        modContactProfileNeighborhood.strfWORK_PHONE, modContactProfileNeighborhood.strfWORK_EMAIL);
                                    if (!((rstEmployee.BOF || rstEmployee.EOF)))
                                    {
                                        strSaleFirstName = rstEmployee.Fields[modContactProfileNeighborhood.strfFIRST_NAME].Value + "";
                                        strSaleLastName = rstEmployee.Fields[modContactProfileNeighborhood.strfLAST_NAME].Value + "";
                                        strSaleWorkPhone = rstEmployee.Fields[modContactProfileNeighborhood.strfWORK_PHONE].Value + "";
                                        strSaleWorkEmail = rstEmployee.Fields[modContactProfileNeighborhood.strfWORK_EMAIL].Value + "";
                                        strOtherNbhdName = TypeConvert.ToString(RSysSystem.Tables[modContactProfileNeighborhood.strtNEIGHBORHOOD].Fields[modContactProfileNeighborhood.strfNAME].Index(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value));
                                        // Send out email
                                        SendEmailForNBHDInact(strOppFirstName + " " + strOppLastName, strOtherNbhdName, strNeighborhoodName, strSaleWorkEmail);

                                    }
                                    rstEmployee.Close();
                                    rstNBHDP_Team.MoveNext();
                                }
                            }
                            // inactivate record, related quotes and related activities
                            objInactivateContactNeighborhoodProfile.InactivateNeighborhoodProfile(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID].Value, new object[] { RldtLangDict.GetText(modContactProfileNeighborhood.strdCONVERTED_TO_BUYER), "" });
                        }
                        rstNBHDProfile.MoveNext();
                    }
                }
                rstNBHDProfile.Close();
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks several conditions to see whether this Contact Neighborhood Profile
        /// can be inactive. If it can become inactive, then update certain fields.
        /// vntContactProfileNeighborhoodID: the primary key of the table Contact_Profile_Neighborhood
        /// blnPerformSave : Go ahead with Inactivation? Default is true.
        /// ahead with inactivation.
        /// </summary>
        /// <returns>
        /// True if this release can become inactive
        /// False if this release can not become inactive</returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// 5.9.0       9/1/10      KA          Added a call to IP_Manage_Interest
        /// 5.9.1       9/8/2010    Ka          commented out return value and set it to True at all times since update of 
        ///                                     contact nbhd profile type is disabled.
        /// </history>
        protected virtual bool CanBeInactive(object vntContactProfileNeighborhoodID, object blnPerformSave)
        {
            bool CanBeInactive = false;
            Recordset rstContactProfileNeighborhood = null;
            Recordset rstOpportunity = null;
            object vntContactId = DBNull.Value;
            object vntNeighborhoodID = DBNull.Value;
            string strType = String.Empty;

            try
            {

                CanBeInactive = true;
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                rstContactProfileNeighborhood = objLib.GetRecordset(vntContactProfileNeighborhoodID, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD,
                    modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strfCONTACT_ID, modContactProfileNeighborhood.strfINACTIVE,
                    modContactProfileNeighborhood.strfINACTIVE_DATE, modContactProfileNeighborhood.strfNEIGHBORHOOD_ID);
                vntContactId = rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfCONTACT_ID].Value;
                vntNeighborhoodID = rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value;
                strType = rstContactProfileNeighborhood.Fields[modContactProfileNeighborhood.strfTYPE].Value + "";

                if (strType == modContactProfileNeighborhood.strNBHDP_TYPE_BUYER || strType == modContactProfileNeighborhood.strNBHDP_TYPE_CANCELLED
                    || strType == modContactProfileNeighborhood.strNBHDP_TYPE_CLOSED)
                {
                    CanBeInactive = false;
                }
                else
                {
                    rstOpportunity = objLib.GetRecordset(modContactProfileNeighborhood.strqRESERVED_OR_SALES_REQUEST_QUOTES, 2,
                        vntNeighborhoodID, vntContactId, modContactProfileNeighborhood.strfSTATUS);
                    if (rstOpportunity.RecordCount > 0)
                    {
                        CanBeInactive = false;
                    }
                }

                // If all good
                //if (CanBeInactive && Convert.ToBoolean(blnPerformSave))
                if (Convert.ToBoolean(blnPerformSave))
                {

                    InactivateContactProfileNeighborhood objInactivateContactNeighborhoodProfile = (InactivateContactProfileNeighborhood)RSysSystem.ServerScripts[modContactProfileNeighborhood.strsINACTIVATE_NBHD_PROFILE].CreateInstance();
                    objInactivateContactNeighborhoodProfile.InactivateNeighborhoodProfile(vntContactProfileNeighborhoodID, null);
                    IP_Manage_Interest_By_ContactProfileNBHDId(vntContactProfileNeighborhoodID);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
            //KA 9/8/10 commented out OOB return and set it to True at all time since Type is not being updated anyway.
            //return CanBeInactive;
            return true;
        }

        /// <summary>
        /// Updates the fields in the contact record from lead record
        /// </summary>
        /// <param name="rstContact">Recordset of Contact</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateContactFromLead(Recordset rstLead, ref Recordset rstContact)
        {

            try
            {
                rstContact.Fields[modContactProfileNeighborhood.strfFIRST_NAME].Value = rstLead.Fields[modContactProfileNeighborhood.strfFIRST_NAME].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfLAST_NAME].Value = rstLead.Fields[modContactProfileNeighborhood.strfLAST_NAME].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfADDRESS_1].Value = rstLead.Fields[modContactProfileNeighborhood.strfADDRESS_1].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfADDRESS_2].Value = rstLead.Fields[modContactProfileNeighborhood.strfADDRESS_2].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfADDRESS_3].Value = rstLead.Fields[modContactProfileNeighborhood.strfADDRESS_3].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfCITY].Value = rstLead.Fields[modContactProfileNeighborhood.strfCITY].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfSTATE_].Value = rstLead.Fields[modContactProfileNeighborhood.strfSTATE_].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfZIP].Value = rstLead.Fields[modContactProfileNeighborhood.strfZIP].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfAREA_CODE].Value = rstLead.Fields[modContactProfileNeighborhood.strfAREA_CODE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfCOUNTY_ID].Value = rstLead.Fields[modContactProfileNeighborhood.strfCOUNTY_ID].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfWORK_PHONE].Value = rstLead.Fields[modContactProfileNeighborhood.strfWORK_PHONE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfEXTENSION].Value = rstLead.Fields[modContactProfileNeighborhood.strfEXTENSION].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfCELL].Value = rstLead.Fields[modContactProfileNeighborhood.strfCELL].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfPHONE].Value = rstLead.Fields[modContactProfileNeighborhood.strfPHONE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfEDUCATION].Value = rstLead.Fields[modContactProfileNeighborhood.strfEDUCATION].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfTITLE].Value = rstLead.Fields[modContactProfileNeighborhood.strfTITLE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfSUFFIX].Value = rstLead.Fields[modContactProfileNeighborhood.strfSUFFIX].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfFAX].Value = rstLead.Fields[modContactProfileNeighborhood.strfFAX].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfEMAIL].Value = rstLead.Fields[modContactProfileNeighborhood.strfEMAIL].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfCOUNTRY].Value = rstLead.Fields[modContactProfileNeighborhood.strfCOUNTRY].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfPREFERRED_CONTACT].Value = rstLead.Fields[modContactProfileNeighborhood.strfPREFERRED_CONTACT].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfTYPE].Value = modContactProfileNeighborhood.strCUSTOMER;
                rstContact.Fields[modContactProfileNeighborhood.strfSSN].Value = rstLead.Fields[modContactProfileNeighborhood.strfSSN].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfGENDER].Value = rstLead.Fields[modContactProfileNeighborhood.strfGENDER].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfMARITAL_STATUS].Value = rstLead.Fields[modContactProfileNeighborhood.strfMARITAL_STATUS].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfREALTOR_COMPANY_ID].Value = rstLead.Fields[modContactProfileNeighborhood.strfREALTOR_COMPANY_ID].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfREALTOR_ID].Value = rstLead.Fields[modContactProfileNeighborhood.strfREALTOR_AGENT_ID].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfREFERRED_BY_CONTACT_ID].Value = rstLead.Fields[modContactProfileNeighborhood.strfREFERRED_BY_CONTACT_ID].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfACCOUNT_MANAGER_ID].Value = rstLead.Fields[modContactProfileNeighborhood.strfACCOUNT_MANAGER_ID].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfLEAD_DATE].Value = rstLead.Fields[modContactProfileNeighborhood.strfRN_CREATE_DATE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfCOMMENTS].Value = rstLead.Fields[modContactProfileNeighborhood.strfCOMMENTS].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value = rstLead.Fields[modContactProfileNeighborhood.strfNP1_NEIGHBORHOOD_ID].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfLEAD_SOURCE_ID].Value = rstLead.Fields[modContactProfileNeighborhood.strfLEAD_SOURCE_ID].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfLEAD_SOURCE_TYPE].Value = rstLead.Fields[modContactProfileNeighborhood.strfLEAD_SOURCE_TYPE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfHOUSEHOLD_SIZE].Value = rstLead.Fields[modContactProfileNeighborhood.strfHOUSEHOLD_SIZE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfSINGLE_OR_DUAL_INCOME].Value = rstLead.Fields[modContactProfileNeighborhood.strfSINGLE_OR_DUAL_INCOME].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfCOMBINED_INCOME_RANGE].Value = rstLead.Fields[modContactProfileNeighborhood.strfCOMBINED_INCOME_RANGE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfAGE_RANGE_OF_BUYERS].Value = rstLead.Fields[modContactProfileNeighborhood.strfAGE_RANGE_OF_BUYERS].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfAGE_RANGE_OF_CHILDREN].Value = rstLead.Fields[modContactProfileNeighborhood.strfAGE_RANGE_OF_CHILDREN].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfNUMBER_OF_CHILDREN].Value = rstLead.Fields[modContactProfileNeighborhood.strfNUMBER_OF_CHILDREN].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfTIME_SEARCHING].Value = rstLead.Fields[modContactProfileNeighborhood.strfTIME_SEARCHING].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfRESALE].Value = rstLead.Fields[modContactProfileNeighborhood.strfRESALE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfOTHER_NEIGHBORHOODS].Value = rstLead.Fields[modContactProfileNeighborhood.strfOTHER_NEIGHBORHOODS].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfOTHER_BUILDERS].Value = rstLead.Fields[modContactProfileNeighborhood.strfOTHER_BUILDERS].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfHOME_TYPE].Value = rstLead.Fields[modContactProfileNeighborhood.strfHOME_TYPE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfMINIMUM_BEDROOMS].Value = rstLead.Fields[modContactProfileNeighborhood.strfMINIMUM_BEDROOMS].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfMINIMUM_BATHROOMS].Value = rstLead.Fields[modContactProfileNeighborhood.strfMINIMUM_BATHROOMS].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfMINIMUM_GARAGE].Value = rstLead.Fields[modContactProfileNeighborhood.strfMINIMUM_GARAGE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfNUMBER_LIVING_AREAS].Value = rstLead.Fields[modContactProfileNeighborhood.strfNUMBER_LIVING_AREAS].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfDESIRED_SQUARE_FOOTAGE].Value = rstLead.Fields[modContactProfileNeighborhood.strfDESIRED_SQUARE_FOOTAGE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfDESIRED_MONTHLY_PAYMENT].Value = rstLead.Fields[modContactProfileNeighborhood.strfDESIRED_MONTHLY_PAYMENT].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfDESIRED_PRICE_RANGE].Value = rstLead.Fields[modContactProfileNeighborhood.strfDESIRED_PRICE_RANGE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfDESIRED_MOVE_IN_DATE].Value = rstLead.Fields[modContactProfileNeighborhood.strfDESIRED_MOVE_IN_DATE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfOWNERSHIP].Value = rstLead.Fields[modContactProfileNeighborhood.strfOWNERSHIP].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfFOR_SALE].Value = rstLead.Fields[modContactProfileNeighborhood.strfFOR_SALE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfCOMBINED_INCOME_RANGE].Value = rstLead.Fields[modContactProfileNeighborhood.strfCOMBINED_INCOME_RANGE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfTRANSFERRING_TO_AREA].Value = rstLead.Fields[modContactProfileNeighborhood.strfTRANSFERRING_TO_AREA].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfCURRENT_MONTHLY_PAYMENT].Value = rstLead.Fields[modContactProfileNeighborhood.strfCURRENT_MONTHLY_PAYMENT].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfCURRENT_SQUARE_FOOTAGE].Value = rstLead.Fields[modContactProfileNeighborhood.strfCURRENT_SQUARE_FOOTAGE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfREASONS_FOR_MOVING].Value = rstLead.Fields[modContactProfileNeighborhood.strfREASONS_FOR_MOVING].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfHOMES_OWNED].Value = rstLead.Fields[modContactProfileNeighborhood.strfHOMES_OWNED].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfCOMMUTE].Value = rstLead.Fields[modContactProfileNeighborhood.strfCOMMUTE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfWALK_IN_DATE].Value = rstLead.Fields[modContactProfileNeighborhood.strfNP1_FIRST_VISIT_DATE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfNEXT_FOLLOW_UP_DATE].Value = rstLead.Fields[modContactProfileNeighborhood.strfVL1_NEXT_DATE].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfCELL_CDNC].Value = rstLead.Fields[modContactProfileNeighborhood.strfCELL_CDNC].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfCELL_NDNC].Value = rstLead.Fields[modContactProfileNeighborhood.strfCELL_NDNC].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfPHONE_CDNC].Value = rstLead.Fields[modContactProfileNeighborhood.strfPHONE_CDNC].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfPHONE_NDNC].Value = rstLead.Fields[modContactProfileNeighborhood.strfPHONE_NDNC].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfWORK_PHONE_CDNC].Value = rstLead.Fields[modContactProfileNeighborhood.strfWORK_PHONE_CDNC].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfWORK_PHONE_NDNC].Value = rstLead.Fields[modContactProfileNeighborhood.strfWORK_PHONE_NDNC].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfFAX_CDNC].Value = rstLead.Fields[modContactProfileNeighborhood.strfFAX_CDNC].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfFAX_NDNC].Value = rstLead.Fields[modContactProfileNeighborhood.strfFAX_NDNC].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfEMAIL_CDNC].Value = rstLead.Fields[modContactProfileNeighborhood.strfEMAIL_CDNC].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfDNC_STATUS].Value = rstLead.Fields[modContactProfileNeighborhood.strfDNC_STATUS].Value;
                rstContact.Fields[modContactProfileNeighborhood.strfWEB_EDITED].Value = 0;

                return;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Finds the neighborhood profile with the earliest Internet Date, and returns the date.
        /// </summary>
        /// <returns>The earliest Internet Date</returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        public virtual object GetEarliestInternetDateOfNBHDProfileForContact(object vntContactId)
        {
            object getEarliestInternetDateOfNBHDProfileForContact = null;
            Recordset rstNBHDProfile = null;

            try
            {
                getEarliestInternetDateOfNBHDProfileForContact = System.DBNull.Value;

                if (Convert.IsDBNull(vntContactId))
                {
                    return getEarliestInternetDateOfNBHDProfileForContact;
                }

                DataAccess objFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                rstNBHDProfile = objFunctionLib.GetRecordset(modContactProfileNeighborhood.strqNBHD_PROFILE_WITH_DEFINED_INTERNETDATE_FOR_CONTACT,
                    1, vntContactId, modContactProfileNeighborhood.strfINTERNET_DATE);

                if (rstNBHDProfile.BOF && rstNBHDProfile.EOF)
                {
                    return getEarliestInternetDateOfNBHDProfileForContact;
                }

                rstNBHDProfile.MoveFirst();
                rstNBHDProfile.Sort = modContactProfileNeighborhood.strfINTERNET_DATE;
                getEarliestInternetDateOfNBHDProfileForContact = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfINTERNET_DATE].Value;

                return getEarliestInternetDateOfNBHDProfileForContact;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Ensure that there's a neighborhood profile record linked to vntNeighborhoodId
        /// and one of vntContactId and vntLeadId.  If the neighborhood profile does not
        /// exist, create one.  If it exists but inactive, the activate it.
        /// Also accept an array of parameters.  Currently only has 1 element which is the
        /// employee which to add to the neighborhood profile team.
        /// </summary>
        /// <params>
        /// vntNeighborhoodId
        /// vntContactId        can only be one of vntContactId and vntLeadId defined
        /// vntLeadId
        /// vntParamArray(0)    assign the neighborhood profile to this employee
        /// vntParamArray(1)    priority code id
        /// vntParamArray(2)    marketing project id
        /// vntParamArray(3)    dtLeadDate
        /// vntParamArray(4)    First Visit Date
        /// </params>
        /// <returns>
        /// Id of the new Neighborhood Profile
        /// </returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// 5.9.0       6/16/2010   KA          Converted to IP ASR fr OOB and added active/inactive timestamp
        /// 5.9.1       9/2/2010    KA          added call to manage interest function
        /// </history>
        public virtual object NewNeighborhoodProfile(object vntNeighborhoodID, object vntContactId, object vntLeadId,
            object vntParamArray)
        {
            object newNeighborhoodProfile = null;
            Recordset rstNBHDProfile = null;
            Recordset rstContactNP = null;
            object vntNBHDProfileId = DBNull.Value;
            object vntAssignedToId = DBNull.Value;            object vntPriorityCodeId = DBNull.Value;            object vntMarketingProjectId = DBNull.Value;            object dtLeadDate = null;
            object dtVisitDate = null;
            object[] paramArray = null;


            try
            {

                paramArray = (object[])vntParamArray;
                DataAccess objLib = null;

                //check to see if the array 0 is from manage interest function, if so then set skip to true to skip over the
                //call back to manage interest function, which should be only called if this method was called outside of manage interest.
                bool blnSkip = false;
                if (paramArray.GetUpperBound(0) == 0 && paramArray[0].ToString() == "Skip")
                {
                    blnSkip = true;
                    //reset param to null so that it won't error out on the bottom section of this code
                    paramArray = new object[] { null };

                }

                // check to see if we need to create a new neighborhood profile
                if ((vntContactId is Array) && (vntNeighborhoodID is Array))
                {//KA 6/16/10 added tic_opt_Edit_Date
                    objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    objLib.PermissionIgnored = true;
                    rstNBHDProfile = objLib.GetRecordset(modContactProfileNeighborhood.strqNBHD_PROFILE_FOR_CONTACT__NBHD, 2, vntContactId,
                        vntNeighborhoodID, modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID, modContactProfileNeighborhood.strfINACTIVE, modContactProfileNeighborhood.strfINACTIVE_DATE,
                        modContactProfileNeighborhood.strfFIRST_VISIT_DATE, modContactProfileNeighborhood.strfMARKETING_PROJECT_ID, modContactProfileNeighborhood.strfLEAD_DATE, modContactProfileNeighborhood.strfINACTIVE_REASON_ID,
                        modContactProfileNeighborhood.strfPRIORITY_CODE_ID, "TIC_Opt_Edit_Date","Neighborhood_Id");
                }
                else if ((vntLeadId is Array) && (vntNeighborhoodID is Array))
                {//KA 6/16/10 added tic_opt_Edit_Date
                    objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    objLib.PermissionIgnored = true;
                    rstNBHDProfile = objLib.GetRecordset(modContactProfileNeighborhood.strqNBHD_PROFILE_FOR_LEAD__NBHD, 2, vntLeadId,
                        vntNeighborhoodID, modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID, modContactProfileNeighborhood.strfINACTIVE, modContactProfileNeighborhood.strfINACTIVE_DATE,
                        modContactProfileNeighborhood.strfFIRST_VISIT_DATE, modContactProfileNeighborhood.strfMARKETING_PROJECT_ID, modContactProfileNeighborhood.strfLEAD_DATE, modContactProfileNeighborhood.strfINACTIVE_REASON_ID,
                        modContactProfileNeighborhood.strfPRIORITY_CODE_ID, "TIC_Opt_Edit_Date", "Neighborhood_Id");
                }
                else
                {
                    // One of vntContactId and vntLeadId has to be defined
                    newNeighborhoodProfile = System.DBNull.Value;
                    return newNeighborhoodProfile;
                }

                if (rstNBHDProfile.RecordCount == 0)
                {
                    // none found for this contact and neighborhood, therefore create a new profile
                    // create Contact Neighborhood Profile only if there is a neighborhood to link to
                    if ((vntNeighborhoodID is Array))
                    {//KA 6/16/10 added tic_opt_Edit_Date
                        rstContactNP = objLib.GetNewRecordset(modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD, modContactProfileNeighborhood.strfCONTACT_ID,
                            modContactProfileNeighborhood.strfLEAD_ID, modContactProfileNeighborhood.strfNEIGHBORHOOD_ID, modContactProfileNeighborhood.strfINTERNET_DATE,
                            modContactProfileNeighborhood.strfLEAD_DATE, modContactProfileNeighborhood.strfDIVISION_ID, modContactProfileNeighborhood.strfPRIORITY_CODE_ID,
                            modContactProfileNeighborhood.strfMARKETING_PROJECT_ID, modContactProfileNeighborhood.strfFIRST_VISIT_DATE,
                            "TIC_Opt_Edit_Date","Inactive", "Inactive_Reason_Id");
                        rstContactNP.AddNew(modContactProfileNeighborhood.strfCONTACT_ID,DBNull.Value);
                        rstContactNP.Fields[modContactProfileNeighborhood.strfCONTACT_ID].Value = vntContactId;
                        rstContactNP.Fields[modContactProfileNeighborhood.strfLEAD_ID].Value = vntLeadId;
                        rstContactNP.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value = vntNeighborhoodID;
                        rstContactNP.Fields[modContactProfileNeighborhood.strfINTERNET_DATE].Value = GetEarliestInternetDateOfNBHDProfileForContact(vntContactId);
                        rstContactNP.Fields[modContactProfileNeighborhood.strfLEAD_DATE].Value = DateTime.Today;
                        rstContactNP.Fields["TIC_Opt_Edit_Date"].Value = DateTime.Now;
                        rstContactNP.Fields["Inactive"].Value = false;

                        // Nov 16, By RY. Modified to use mrsysSystem
                        // May 27, By JWang. Populate division_id from Neighborhood table
                        // Set rstNeighborhood = objLib.GetRecordset
                        rstContactNP.Fields[modContactProfileNeighborhood.strfDIVISION_ID].Value = RSysSystem.Tables[modContactProfileNeighborhood.strtNEIGHBORHOOD].Fields[modContactProfileNeighborhood.strfDIVISION_ID].Index(vntNeighborhoodID);

                        // Jun 9th, by RY. Priority Code Id
                        if (paramArray.GetUpperBound(0) >= 1)
                        {
                            vntPriorityCodeId = paramArray[1];
                            if ((vntPriorityCodeId is Array))
                            {
                                rstContactNP.Fields[modContactProfileNeighborhood.strfPRIORITY_CODE_ID].Value = vntPriorityCodeId;
                            }
                        }

                        // Jun 16th, by RY. Marketing Project Id
                        if (paramArray.GetUpperBound(0) >= 2)
                        {
                            vntMarketingProjectId = paramArray[2];
                            if ((vntMarketingProjectId is Array))
                            {
                                rstContactNP.Fields[modContactProfileNeighborhood.strfMARKETING_PROJECT_ID].Value = vntMarketingProjectId;
                            }
                        }

                        // Jun 24th, by RY. Lead Date
                        if (paramArray.GetUpperBound(0) >= 3)
                        {
                            dtLeadDate = paramArray[3];
                            rstContactNP.Fields[modContactProfileNeighborhood.strfLEAD_DATE].Value = dtLeadDate;
                        }

                        // Jun 28th, by RY. Visit Date
                        if (paramArray.GetUpperBound(0) >= 4)
                        {
                            dtVisitDate = paramArray[4];
                            rstContactNP.Fields[modContactProfileNeighborhood.strfFIRST_VISIT_DATE].Value = dtVisitDate;
                        }

                        objLib.SaveRecordset(modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD, rstContactNP);
                        vntNBHDProfileId = rstContactNP.Fields[modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID].Value;
                    }
                }
                else
                {
                    // link the one found to
                    if (rstNBHDProfile.RecordCount == 1)
                    {
                        // set the NBHD profile active
                        rstNBHDProfile.Fields[modContactProfileNeighborhood.strfINACTIVE].Value = false;
                        rstNBHDProfile.Fields[modContactProfileNeighborhood.strfINACTIVE_DATE].Value = DBNull.Value;
                        rstNBHDProfile.Fields[modContactProfileNeighborhood.strfINACTIVE_REASON_ID].Value = DBNull.Value;
                        rstNBHDProfile.Fields["TIC_Opt_Edit_Date"].Value = DateTime.Now;

                        // Jun 9th, by RY. Priority Code Id
                        if (paramArray.GetUpperBound(0) >= 1)
                        {
                            vntPriorityCodeId = paramArray[1];
                            if ((vntPriorityCodeId is Array) && (Convert.IsDBNull(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfPRIORITY_CODE_ID].Value)))
                            {
                                rstNBHDProfile.Fields[modContactProfileNeighborhood.strfPRIORITY_CODE_ID].Value = vntPriorityCodeId;
                            }
                        }

                        // Jun 16th, by RY. Marketing Project Id
                        if (paramArray.GetUpperBound(0) >= 2)
                        {
                            vntMarketingProjectId = paramArray[2];
                            if ((vntMarketingProjectId is Array) && Convert.IsDBNull(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfMARKETING_PROJECT_ID].Value))  
                            {
                                rstNBHDProfile.Fields[modContactProfileNeighborhood.strfMARKETING_PROJECT_ID].Value = vntMarketingProjectId;
                            }
                        }

                        // Jun 24th, by RY. Lead Date
                        if (paramArray.GetUpperBound(0) >= 3)
                        {
                            dtLeadDate = paramArray[3];
                            if (Convert.IsDBNull(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfLEAD_DATE].Value))
                            {
                                rstNBHDProfile.Fields[modContactProfileNeighborhood.strfLEAD_DATE].Value = dtLeadDate;  
                            }
                        }

                        // Jun 28th, by RY. Visit Date
                        if (paramArray.GetUpperBound(0) >= 4)
                        {
                            dtVisitDate = paramArray[4];
                            if (Convert.IsDBNull(rstNBHDProfile.Fields[modContactProfileNeighborhood.strfFIRST_VISIT_DATE].Value))
                            {
                                rstNBHDProfile.Fields[modContactProfileNeighborhood.strfFIRST_VISIT_DATE].Value = dtVisitDate;
                            }
                        }

                        objLib.SaveRecordset(modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD, rstNBHDProfile);
                        vntNBHDProfileId = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID].Value;
                    }
                }

                // Add Team Member
                if (paramArray.GetUpperBound(0) >= 0)
                {
                    vntAssignedToId = paramArray[0];
                    if ((vntAssignedToId is Array))
                    {
                        UpdateNBHDProfieTeam(vntNBHDProfileId, vntAssignedToId);
                    }
                }
                // Update NBHD Profile Type
                UpdateNBHDPType(vntNBHDProfileId);

                //ka call IP custom code if NBHD record existed, then use that otherwise, use the new rstContactNP object
                if (blnSkip == false)
                {
                    if (rstNBHDProfile.RecordCount > 0)
                    {
                        IP_Manage_Interest(rstNBHDProfile);
                    }
                    else
                    {
                        IP_Manage_Interest(rstContactNP);
                    }

                }
                newNeighborhoodProfile = vntNBHDProfileId;

                return newNeighborhoodProfile;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// updates Neighborhood Profile team. If current user is not existed in the NBHDP team,
        /// creates one. If current user is exised in the NBHDP team, and it's inactive flag is set to true,
        /// re-set the inactive flag to false.
        /// Inputs : vntNBHDProfileId - NBHD Profile Id
        /// vntEmployeeId    - Employee Id
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        public virtual void UpdateNBHDProfieTeam(object vntNBHDProfileId, object vntEmployeeId)
        {
            bool blnNBHDPTeamExist = false;
            bool blnInactive = false;
            Recordset rstCTM = null;

            try
            {


                blnNBHDPTeamExist = false;
                if (Convert.IsDBNull(vntEmployeeId) || Convert.IsDBNull(vntNBHDProfileId))
                {
                    return;
                }
                DataAccess objFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objFunctionLib.PermissionIgnored = true;
                if (!(Convert.IsDBNull(vntNBHDProfileId)))
                {
                    rstCTM = objFunctionLib.GetRecordset(modContactProfileNeighborhood.strqCTM_WITH_NBHDPROFILE_EMPLOYEE, 2, vntNBHDProfileId,
                        vntEmployeeId, modContactProfileNeighborhood.strfNEIGHBORHOOD_PROFILE_ID, modContactProfileNeighborhood.strfEMPLOYEE_ID,
                        modContactProfileNeighborhood.strfINACTIVE, modContactProfileNeighborhood.strfROLE_ID);

                    if (rstCTM.RecordCount > 0)
                    {
                        rstCTM.MoveFirst();
                        blnNBHDPTeamExist = true;
                        if (Convert.ToBoolean(rstCTM.Fields[modContactProfileNeighborhood.strfINACTIVE].Value))
                        {
                            // Updates the inactive flag to False
                            rstCTM.Fields[modContactProfileNeighborhood.strfINACTIVE].Value = false;
                            blnInactive = true;
                        }
                    }

                    if (!blnNBHDPTeamExist)
                    {
                        rstCTM = objFunctionLib.GetNewRecordset(modContactProfileNeighborhood.strtCONTACT_TEAM_MEMBER,
                            modContactProfileNeighborhood.strfNEIGHBORHOOD_PROFILE_ID, modContactProfileNeighborhood.strfEMPLOYEE_ID,
                            modContactProfileNeighborhood.strfINACTIVE, modContactProfileNeighborhood.strfROLE_ID);
                        rstCTM.AddNew(modContactProfileNeighborhood.strfNEIGHBORHOOD_PROFILE_ID, System.DBNull.Value);
                        rstCTM.Fields[modContactProfileNeighborhood.strfEMPLOYEE_ID].Value = vntEmployeeId;
                        rstCTM.Fields[modContactProfileNeighborhood.strfROLE_ID].Value = RSysSystem.Tables[modContactProfileNeighborhood.strtEMPLOYEE].Fields[modContactProfileNeighborhood.strfROLE_ID].Index(vntEmployeeId);
                        rstCTM.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_PROFILE_ID].Value = vntNBHDProfileId;
                        rstCTM.Fields[modContactProfileNeighborhood.strfINACTIVE].Value = false;
                    }

                    if (!blnNBHDPTeamExist || blnInactive)
                    {
                        objFunctionLib.SaveRecordset(modContactProfileNeighborhood.strtCONTACT_TEAM_MEMBER, rstCTM);
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Send Email to Sales Team notifying them that the NBHD_Profile has
        /// been inactivated due to Buyer as Global Stage
        /// strLotRnDescriptor, strLotNumber, strLotPhase, strLotBlock, strLotTract,
        /// strLotBuilding, strLotUnit, strLotJobNumber, strLotStatus, curQuoteTotal,
        /// strReleaseRnDescriptor, strReleaseName, strNeighborhoodName, strOppFirstName, strOppLastName,
        /// strOppPhone , strPlanCode, strElevationCode
        /// </summary>
        /// <params>
        /// strContactName - contact name
        /// strNbhdName - neighborhood name
        /// strBuyingNbhdName - buying neighborhood name
        /// strSaleWorkEmail - sale work email
        /// </params>
        /// <returns>nothing</returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        protected virtual void SendEmailForNBHDInact(string strContactName, string strNbhdName, string strBuyingNbhdName, string
            strSaleWorkEmail)
        {
            IRSend objrEmail = null;
            string strRecipient = String.Empty;
            string strSubject = String.Empty;
            string strBody = String.Empty;


            try
            {

                strRecipient = strSaleWorkEmail;
                strSubject = TypeConvert.ToString(RldtLangDict.GetTextSub(modContactProfileNeighborhood.strdBUYER_NBHD_PROFILE_INACTIVATED,
                    new object[] { strContactName, strNbhdName }));
                strBody = TypeConvert.ToString(RldtLangDict.GetTextSub(modContactProfileNeighborhood.strdALL_OPEN_ACTIVITIES_CANCELLED,
                    new object[] { strContactName, strNbhdName, strBuyingNbhdName }));

                objrEmail = RSysSystem.CreateEmail();
                objrEmail.NewMessage();
                objrEmail.To = strRecipient;
                objrEmail.Subject = strSubject;
                objrEmail.Body = strBody;
                objrEmail.Send();

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// update the contact dates Next_Follow_Up_Date and Walk_In_Date for visit log type of activity
        /// Inputs :
        /// vntContact Id : Contact Id associated with the activity
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        public virtual void UpdateContactDates(object vntContactId)
        {
            Recordset rstContact = null;
            Recordset rstActivities = null;
            Recordset rstNextActivity = null;


            try
            {

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // get the contact associated with this!
                if ((vntContactId is Array))
                {
                    // check the date, update only if null
                    rstContact = objLib.GetRecordset(vntContactId, modContactProfileNeighborhood.strtCONTACT, modContactProfileNeighborhood.strfWALK_IN_DATE,
                        modContactProfileNeighborhood.strfNEXT_FOLLOW_UP_DATE, modContactProfileNeighborhood.strfFIRST_CONTACT_DATE);
                    if (!(rstContact.EOF) && !(rstContact.BOF))
                    {
                        rstContact.MoveFirst();
                        objLib.SortAscending = true;
                        objLib.SortFieldName = modContactProfileNeighborhood.strfAPPT_DATE;
                        rstActivities = objLib.GetRecordset(modContactProfileNeighborhood.strqVISIT_LOGS_FOR_CONTACT, 1, vntContactId, modContactProfileNeighborhood.strfAPPT_DATE);
                        if (rstActivities.RecordCount <= 0)
                        {
                            // none found
                            rstContact.Fields[modContactProfileNeighborhood.strfWALK_IN_DATE].Value = System.DBNull.Value;

                        }
                        else
                        {
                            rstContact.MoveFirst();
                            if (Convert.IsDBNull(rstContact.Fields[modContactProfileNeighborhood.strfWALK_IN_DATE].Value))
                            {
                                rstContact.Fields[modContactProfileNeighborhood.strfWALK_IN_DATE].Value = rstActivities.Fields[modContactProfileNeighborhood.strfAPPT_DATE].Value;
                            }
                            if (Convert.IsDBNull(rstContact.Fields[modContactProfileNeighborhood.strfFIRST_CONTACT_DATE].Value))
                            {
                                rstContact.Fields[modContactProfileNeighborhood.strfFIRST_CONTACT_DATE].Value = rstActivities.Fields[modContactProfileNeighborhood.strfAPPT_DATE].Value;
                            }
                        }

                        // set the next follow up date
                        objLib.SortAscending = true;
                        objLib.SortFieldName = modContactProfileNeighborhood.strfAPPT_DATE;
                        rstNextActivity = objLib.GetRecordset(modContactProfileNeighborhood.strqIN_COMPLETE_VISIT_LOGS_FOR_CONTACT, 1, vntContactId, 
                            modContactProfileNeighborhood.strfAPPT_DATE);
                        if (rstNextActivity.RecordCount <= 0)
                        {
                            rstContact.Fields[modContactProfileNeighborhood.strfNEXT_FOLLOW_UP_DATE].Value = System.DBNull.Value;
                        }
                        else
                        {
                            rstContact.MoveFirst();
                            rstContact.Fields[modContactProfileNeighborhood.strfNEXT_FOLLOW_UP_DATE].Value = rstNextActivity.Fields[modContactProfileNeighborhood.strfAPPT_DATE].Value;
                        }
                        objLib.SaveRecordset(modContactProfileNeighborhood.strtCONTACT, rstContact);
                    }
                }

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        //Name: IP_Manage_Interest_By_ContactProfileNBHDId
        //Desc: since "inactive" flag = true is set after the CanBeInactive function runs, that function will call this
        //      to process the inactive neighborhood
        //Revision  Date        Author  Description
        //5.9.0     09/01/2010  KA      Initial Version     
        public void IP_Manage_Interest_By_ContactProfileNBHDId(object vntContactProfileNBHDId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                InactivateContactProfileNeighborhood objInactivateContactNeighborhoodProfile = (InactivateContactProfileNeighborhood)RSysSystem.ServerScripts[modContactProfileNeighborhood.strsINACTIVATE_NBHD_PROFILE].CreateInstance();

                Recordset rstRecordset = objLib.GetRecordset(vntContactProfileNBHDId, "Contact_Profile_Neighborhood","Division_Id",
                    "Lead_Id", "Contact_Id", "Contact_Profile_NBHD_Id", "Neighborhood_Id", "Inactive", "Inactive_Reason_Id");

                IP_Manage_Interest(rstRecordset);
            }
         
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        //Name: IP_Manage_Interest
        //Desc: Rebuilds NBHD Opt Ins and Out fields - if village is opted out, then opted out of all NBHD for that village
        //      that the person is currently in.  if they opted into a NBHD, then opted them into the Village too
        //Revision  Date        Author  Description
        //5.9.0     09/01/2010  KA      Initial Version
        //5.9.1     9/8/2010    Ka      commented out call to UPdateNBHDType, will not change NBHD Profile Type since it's too
        //                              difficult to figure out what the last status is if they opted in and out.
        public void IP_Manage_Interest( Recordset rstRecordset)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                InactivateContactProfileNeighborhood objInactivateContactNeighborhoodProfile = (InactivateContactProfileNeighborhood)RSysSystem.ServerScripts[modContactProfileNeighborhood.strsINACTIVATE_NBHD_PROFILE].CreateInstance();

                string strFkIdField = "";
                string strUpdateTable = "";
                if (rstRecordset.Fields["Lead_Id"].Value != System.DBNull.Value)
                {
                    strFkIdField = "Lead_Id";
                    strUpdateTable = "Lead_";
                }
                else
                {
                    strFkIdField = "Contact_Id";
                    strUpdateTable = "Contact";
                }

                //get all NBHD Interest record for the person
                object vntPersonId = rstRecordset.Fields[strFkIdField].Value;
                Recordset rstInterest = objLib.GetLinkedRecordset("Contact_Profile_Neighborhood", strFkIdField, 
                    vntPersonId,"Contact_Profile_NBHD_Id", "Inactive", "TIC_Opt_Edit_Date","Division_Id",
                    "Neighborhood_Id", "Inactive_Reason_Id");

                while (rstRecordset.EOF == false)
                {
                    //get neighborhood info from recordset that's passed in
                    object vntContactNBHDId = rstRecordset.Fields["Contact_Profile_NBHD_Id"].Value;
                    object vntNBHDId = rstRecordset.Fields["Neighborhood_Id"].Value;
                    bool blnInactive = Convert.ToBoolean(rstRecordset.Fields["Inactive"].Value);
                    object vntReasonId = rstRecordset.Fields["Inactive_Reason_Id"].Value;
                    object vntDivisionId = rstRecordset.Fields["Division_Id"].Value;

                    //get neighborhood record
                    Recordset rstNBHD = objLib.GetRecordset(vntNBHDId, "Neighborhood", "Division_Id",
                        "Market_Level_Neighborhood", "Neighborhood_Id");

                    if (rstNBHD.RecordCount > 0)
                    {
                        //check to see if the neighborhood is a division level
                        bool blnMktLvlNbhd = Convert.ToBoolean(rstNBHD.Fields["Market_Level_Neighborhood"].Value);

                        //if OPTED OUTs of division, then opted them out of NBHD too
                        if (blnMktLvlNbhd == true && blnInactive == true)
                        {
                            while (rstInterest.EOF == false)
                            {
                                object vntInterestId = rstInterest.Fields["Contact_Profile_NBHD_Id"].Value;
                                object vntInterestDivId = rstInterest.Fields["Division_Id"].Value;
                                bool blnInterestInactive = Convert.ToBoolean(rstInterest.Fields["Inactive"].Value);

                                //make sure it's not the current record that's being evaluated, 
                                //check to make sure the NBHD is in the Village (divison) that is being opted out 
                                //only update if the NBHD is not inactive
                                if (mrsysSystem.EqualIds(vntContactNBHDId, vntInterestId) == false
                                    && mrsysSystem.EqualIds(vntDivisionId, vntInterestDivId) == true
                                    && blnInterestInactive == false)
                                {
                                    string strReason = mrsysSystem.Tables["Inactive_Reason"].Fields["Reason_Code"].Index(vntReasonId).ToString();
                                    objInactivateContactNeighborhoodProfile.InactivateNeighborhoodProfile(vntInterestId, new object[] {strReason});
                                    //UpdateNBHDPType(vntInterestId);
                                }
                                rstInterest.MoveNext();
                            }
                        }

                        
                        //move the interest recordset back 
                        if (rstInterest.RecordCount > 0) { rstInterest.MoveFirst(); }

                        //process OPTED INs - if NBHD is added and it's not Village/division, 
                        //then check to make sure they are in village/division first, if not opted in, then opted in
                        if (blnMktLvlNbhd == false && blnInactive == false)
                        {
                            bool blnDivisionFound = false;

                            while (rstInterest.EOF == false)
                            {
                                object vntInterestDivId = rstInterest.Fields["Division_Id"].Value;
                                bool blnInterestInactive = Convert.ToBoolean(rstInterest.Fields["Inactive"].Value);
                                object vntInterestNBHDId = rstInterest.Fields["Neighborhood_Id"].Value;

                                Recordset rstLookupInterestNBHD = objLib.GetRecordset(vntInterestNBHDId, "Neighborhood",
                                        "Division_Id", "Market_Level_Neighborhood",  "Neighborhood_Id");

                                if (rstLookupInterestNBHD.RecordCount > 0)
                                {
                                    bool blnInterestMktLvlNbhd = Convert.ToBoolean(rstLookupInterestNBHD.Fields["Market_Level_Neighborhood"].Value);

                                    //check to see if the village is already included in the interest recordset
                                    //if yes, then set it to active and missing, then add it
                                    if (mrsysSystem.EqualIds(vntInterestDivId, vntDivisionId) == true
                                        && blnInterestMktLvlNbhd == true)
                                    {
                                        blnDivisionFound = true;
                                        if (blnInterestInactive == true)
                                        {
                                            
                                            if (strFkIdField == "Lead_Id")
                                            {
                                                NewNeighborhoodProfile(vntInterestNBHDId, DBNull.Value, vntPersonId, new object[] { "Skip" });
                                                //UpdateNBHDPType(vntInterestNBHDId);
                                            }
                                            else
                                            {
                                                NewNeighborhoodProfile(vntInterestNBHDId, vntPersonId, DBNull.Value, new object[] { "Skip" });
                                                //UpdateNBHDPType(vntInterestNBHDId);
                                            }
                                        }
                                    }
                                }
                                rstInterest.MoveNext();
                            }
                            //if division is not included as an interest, then find the division record for nbhd and add it
                            if (blnDivisionFound == false)
                            {
                                Recordset rstDivisionNBHD = objLib.GetRecordset("TIC: Mkt Lvl Neighborhood for Division Id?", 1,
                                    vntDivisionId, "Neighborhood_Id");
                                if (rstDivisionNBHD.RecordCount > 0)
                                {
                                    object vntDivNBHDId = rstDivisionNBHD.Fields["Neighborhood_Id"].Value;
                                    if (strFkIdField == "Lead_Id")
                                    {
                                        NewNeighborhoodProfile(vntDivNBHDId, DBNull.Value, vntPersonId, new object[] { "Skip" });
                                        //UpdateNBHDPType(vntDivNBHDId);
                                    }
                                    else
                                    {
                                        NewNeighborhoodProfile(vntDivNBHDId, vntPersonId, DBNull.Value, new object[] { "Skip" });
                                        //UpdateNBHDPType(vntDivNBHDId);
                                    }
                                }
                            }
                            
                        }
                    }

                    rstRecordset.MoveNext();
                }
                
                //build m1 memo fields
                IP_Build_M1_Opted_In_Out_Field(vntPersonId, strUpdateTable);

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        //Name: IP_Build_M1_Opted_In_Out_Field
        //Desc: builds the memo fields that is need pass back to M1
        //Revision  Date        Author  Description
        //5.9.0     09/02/2010  KA      Initial Version     
        public void IP_Build_M1_Opted_In_Out_Field(object vntLeadOrContactId, string strUpdateTable)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                string strFkIdField = "";

                if (strUpdateTable == "Lead_")
                {
                    strFkIdField = "Lead_Id";
                }
                else
                {
                    strFkIdField = "Contact_Id";
                }

                string strNBHDOptedIns = "";
                string strNBHDOptedOuts = "";

                Recordset rstInterest = objLib.GetLinkedRecordset("Contact_Profile_Neighborhood", strFkIdField,
                    vntLeadOrContactId, "Contact_Profile_NBHD_Id", "Inactive", "Neighborhood_Id");

                while (rstInterest.EOF == false)
                {
                    object vntNBHDId = rstInterest.Fields["Neighborhood_Id"].Value;

                    Recordset rstNBHD = objLib.GetRecordset(vntNBHDId, "Neighborhood", "TIC_Neighborhood_Code");

                    if (rstNBHD.RecordCount > 0 && rstNBHD.Fields["TIC_Neighborhood_Code"].Value != System.DBNull.Value)
                    {
                        //if it's opted out interest then build opted out field else it's opted ins
                        if (Convert.ToBoolean(rstInterest.Fields["Inactive"].Value) == true)
                        {
                            strNBHDOptedOuts = strNBHDOptedOuts + "|" + rstNBHD.Fields["TIC_Neighborhood_Code"].Value.ToString();
                        }
                        else
                        {
                            strNBHDOptedIns = strNBHDOptedIns + "|" + rstNBHD.Fields["TIC_Neighborhood_Code"].Value.ToString();
                        }
                    }

                    rstInterest.MoveNext();
                }

                Recordset rstLeadOrContact = objLib.GetRecordset(vntLeadOrContactId, strUpdateTable,
                    "TIC_M1_Proj_List_Opt_Out", "TIC_M1_Proj_List_Opt_In");
                if (rstLeadOrContact.RecordCount == 1)
                {
                    rstLeadOrContact.Fields["TIC_M1_Proj_List_Opt_Out"].Value = strNBHDOptedOuts;
                    rstLeadOrContact.Fields["TIC_M1_Proj_List_Opt_In"].Value = strNBHDOptedIns;

                    objLib.SaveRecordset(strUpdateTable, rstLeadOrContact);

                }
                
            }

            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        public object Contact
        {
            get
            {
                return mvntContactId;
            }
        }
    }
}
