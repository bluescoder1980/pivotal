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
    public class InactivateContactProfileNeighborhood : IRAppScript
    {
        /// <summary>
        /// Description : Scheduled script used to update the inactive status in the
        /// </summary>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        private IRSystem7 mrdaSystem = null;

        protected IRSystem7 RdaSystem
        {
            get { return mrdaSystem; }
            set { mrdaSystem = value; }
        }

        /// <summary>
        /// Checks the method name to execute one of the following methods:
        /// Inputs:executed
        /// MethodName    - Method name to be
        /// ParameterList - Transit point parameters passed from the client to AppServer for
        /// business rule processing
        /// </summary>
        /// <params>
        /// ParameterList - Transit point parameters passed from AppServer back to the client side may contain the executed results.
        /// </params>
        /// <returns></returns>
        /// <history>
        /// Revision#   Date        Author  Description
        /// 3.8.0.0     5/5/2006    CLangan Converted to .Net C# code.
        /// </history>
        public virtual void Execute(string methodName, ref object ParameterList)
        {
            try
            {

                TransitionPointParameter objParam = (TransitionPointParameter)RdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;
                object[] parameterArray = objParam.GetUserDefinedParameterArray();

                switch (methodName)
                {
                    case modContactProfileNeighborhood.strmNBHDPROFILEINACTIVATION:
                        UpdateNBHDProfile();
                        break;
                    default:
                        string message = RdaSystem.GetLDGroup(ErrorsLDGroupData.ErrorsLDGroupName).GetTextSub
                            (ErrorsLDGroupData.MethodNotDefinedLDLookupName, new object[] { methodName })
                            .ToString();
                        throw new PivotalApplicationException(message);
                }

                ParameterList = objParam.SetUserDefinedParameterArray(parameterArray);
                return;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RdaSystem);
            }
        }

        /// <summary>
        /// Sets the IRSystem7 for the PHbNBHDProduct.NBHDPInactivation class.
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/5/2006    CLangan Converted to .Net C# code.
        /// </history>
        public virtual void SetSystem(RSystem pSystem)
        {

            try
            {

                RdaSystem = (IRSystem7)pSystem;
                return;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RdaSystem);
            }
        }

        /// <summary>
        /// Updates the active and not reserved quote.
        /// </summary>
        /// <param name="vntContactId">Contact Id</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/5/2006    CLangan Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateQuote(object vntNeighborhoodID, object vntContactId)
        {
            Recordset rstQuote = null;

            try
            {


                DataAccess objDLFunctionLib = (DataAccess)RdaSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objDLFunctionLib.PermissionIgnored = true;

                rstQuote = objDLFunctionLib.GetRecordset("Sys: Active Not Reserved Quote for Neighborhood ? Contact ?", 2, vntNeighborhoodID, vntContactId, modContactProfileNeighborhood.strfINACTIVE, modContactProfileNeighborhood.strfSTATUS);
                if (rstQuote.RecordCount > 0)
                {
                    rstQuote.MoveFirst();
                }
                while (!(rstQuote.EOF))
                {
                    rstQuote.Fields[modContactProfileNeighborhood.strfINACTIVE].Value = true;
                    rstQuote.Fields[modContactProfileNeighborhood.strfSTATUS].Value = modContactProfileNeighborhood.strQUOTE_TYPE_INACTIVE;
                    rstQuote.MoveNext();
                }
                rstQuote = objDLFunctionLib.SaveRecordset(modContactProfileNeighborhood.strtOPPORTUNITY, rstQuote);

                return;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RdaSystem);
            }
        }

        /// <summary>
        /// Updates the Inactive status for inactive NBHD Profile
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#   Date        Author  Description
        /// 3.8.0.0     5/5/2006    CLangan Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateNBHDProfile()
        {
            Recordset rstNBHDProfile = null;
            object vntDivisionId = DBNull.Value;
            object vntNBHDProfileId = DBNull.Value;
            bool blnInactive = false;
            object vntContactId = DBNull.Value;
            object vntNeighborhoodID = DBNull.Value;
            Recordset divisionRecordset = null;
            DateTime divisionExpiryDate;

            try
            {
                UIAccess objPLFunctionLib = (UIAccess)RdaSystem.ServerScripts[AppServerRuleData.UIAccessAppServerRuleName].CreateInstance();
                DataAccess objDLFunctionLib = (DataAccess)RdaSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objDLFunctionLib.PermissionIgnored = true;

                divisionRecordset = objDLFunctionLib.GetRecordset(modContactProfileNeighborhood.strqDIVISIONS_WITH_CONT_EXP_PER_DEFINED, 0, modContactProfileNeighborhood.strfDIVISION_ID, modContactProfileNeighborhood.strfCONTACT_EXPIRATION_PERIOD);
                if (divisionRecordset.RecordCount > 0)
                {
                    divisionRecordset.MoveFirst();
                    while (!divisionRecordset.EOF)
                    {
                        object o = divisionRecordset.Fields[modContactProfileNeighborhood.strfCONTACT_EXPIRATION_PERIOD].Value;
                        int days = (int)o;
                        divisionExpiryDate = DateTime.Today.AddDays(-1 * days); 

                        rstNBHDProfile = objDLFunctionLib.GetRecordset("Sys: Inactivate NBHD Profile", 5, divisionRecordset.Fields[modContactProfileNeighborhood.strfDIVISION_ID].Value, divisionExpiryDate, divisionExpiryDate,
                            divisionExpiryDate, divisionExpiryDate, modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID, modContactProfileNeighborhood.strfINACTIVE, modContactProfileNeighborhood.strfINACTIVE_DATE,
                            modContactProfileNeighborhood.strfINACTIVE_REASON_ID, modContactProfileNeighborhood.strfDIVISION_ID, modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strfINACTIVE_REASON_ID,
                            modContactProfileNeighborhood.strfCONTACT_ID, modContactProfileNeighborhood.strfNEIGHBORHOOD_ID);

                        if (rstNBHDProfile.RecordCount > 0)
                        {
                            rstNBHDProfile.MoveFirst();
                        }

                        while (!(rstNBHDProfile.EOF))
                        {
                            vntNBHDProfileId = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfCONTACT_PROFILE_NBHD_ID].Value;
                            vntNeighborhoodID = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value;
                            vntContactId = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfCONTACT_ID].Value;

                            if (!(RdaSystem.EqualIds(vntDivisionId, (object)rstNBHDProfile.Fields[modContactProfileNeighborhood.strfDIVISION_ID].Value)))
                            {
                                // Division changed, gets the contact expiration period for the division
                                vntDivisionId = rstNBHDProfile.Fields[modContactProfileNeighborhood.strfDIVISION_ID].Value;
                            }

                            // Find activities associated to this NBHD Profile
                            blnInactive = ExpiredNBHDProfile(vntNBHDProfileId, divisionExpiryDate, objDLFunctionLib);
                            if (blnInactive)
                            {
                                rstNBHDProfile.Fields[modContactProfileNeighborhood.strfINACTIVE].Value = true;
                                rstNBHDProfile.Fields[modContactProfileNeighborhood.strfINACTIVE_DATE].Value = DateTime.Today;
                                rstNBHDProfile.Fields[modContactProfileNeighborhood.strfTYPE].Value = objPLFunctionLib.GetComboChoiceText("Inactive", modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD);
                                rstNBHDProfile.Fields[modContactProfileNeighborhood.strfINACTIVE_REASON_ID].Value = RdaSystem.Tables[modContactProfileNeighborhood.strtINACTIVE_REASON].Fields[modContactProfileNeighborhood.strfREASON_CODE].Find("Inactivity");

                                // Cancel incomplete activities
                                CancelActivities(vntNeighborhoodID, vntContactId, "Canceled Due to Inactivity");

                                // Update active not reserved quote
                                if (!(Convert.IsDBNull(vntContactId)))
                                {
                                    UpdateQuote(vntNeighborhoodID, vntContactId);
                                }

                                InactivateTeamMembers(vntNBHDProfileId, true);

                            }
                            rstNBHDProfile.MoveNext();
                        }
                        rstNBHDProfile = objDLFunctionLib.SaveRecordset(modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD, rstNBHDProfile);

                        divisionRecordset.MoveNext();
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RdaSystem);
            }
        }

        /// <summary>
        /// Cancels incompleted activities for given NBHD Profile
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/5/2006    CLangan Converted to .Net C# code.
        /// </history>
        protected virtual void CancelActivities(object vntNBHDId, object vntContactId, object vntNotes)
        {
            try
            {
                DataAccess objDLFunctionLib = (DataAccess)RdaSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objDLFunctionLib.PermissionIgnored = true;
                Recordset rstActivities = objDLFunctionLib.GetRecordset("Sys: Incomplete Activities for NBHD? Contact?", 2, vntNBHDId, vntContactId, modContactProfileNeighborhood.strfACTIVITY_CANCELED, modContactProfileNeighborhood.strfAPPOINTMENT_CANCELED_DATE, modContactProfileNeighborhood.strfNOTES);
                if (rstActivities.RecordCount > 0)
                {
                    rstActivities.MoveFirst();
                }
                while (!(rstActivities.EOF))
                {
                    rstActivities.Fields[modContactProfileNeighborhood.strfACTIVITY_CANCELED].Value = 1;
                    rstActivities.Fields[modContactProfileNeighborhood.strfAPPOINTMENT_CANCELED_DATE].Value = DateTime.Today;
                    rstActivities.Fields[modContactProfileNeighborhood.strfNOTES].Value = vntNotes;
                    rstActivities.MoveNext();
                }

                rstActivities = objDLFunctionLib.SaveRecordset(modContactProfileNeighborhood.strtRN_APPOINTMENT, rstActivities);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RdaSystem);
            }
        }

        /// <summary>
        /// Checks whether the specified NBHD Profile is inactive.
        /// </summary>
        /// <param name="intExpirationPeriod">Expiration Period</param>
        /// <returns>True - Inactive NBHD Profile, False - otherwise</returns>
        /// <history>
        /// Revision#     Date        Author  Description
        /// 3.8.0.0       5/5/2006    CLangan Converted to .Net C# code.
        /// </history>
        protected virtual bool ExpiredNBHDProfile(object vntNBHDProfileId, DateTime expiryDate, DataAccess objDLFunctionLib)
        {
            Recordset rstActivities = null;
            bool blnExpire = false;

            try
            {

                objDLFunctionLib.PermissionIgnored = true;
                blnExpire = true;

                rstActivities = objDLFunctionLib.GetRecordset("Sys: Complete Activities for Active NBHD Profile ?", 1, vntNBHDProfileId, modContactProfileNeighborhood.strfACTIVITY_COMPLETE, modContactProfileNeighborhood.strfACTIVITY_COMPLETED_DATE);
                if (rstActivities.RecordCount > 0)
                {
                    rstActivities.MoveFirst();
                }
                if (rstActivities.RecordCount == 0)
                {
                    blnExpire = true;
                }
                else
                {
                    rstActivities.MoveFirst();
                }

                while (!(rstActivities.EOF))
                {
                    if ((DateTime) rstActivities.Fields[modContactProfileNeighborhood.strfACTIVITY_COMPLETED_DATE].Value >= expiryDate)
                    {
                        blnExpire = false;
                    }
                    rstActivities.MoveNext();
                }


                return blnExpire;

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RdaSystem);
            }
        }

        /// <summary>
        /// Inactivate team members
        /// </summary>
        /// <param name="blnInactive">True for inactivating; False for activating (only handles Inactivating for now)</param>
        /// <returns></returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/5/2006CLangan   Converted to .Net C# code.
        /// </history>
        protected virtual void InactivateTeamMembers(object vntContactProfileNBHDID, bool blnInactive)
        {
            Recordset rstTeamMember = null;

            try
            {
                DataAccess objLib = (DataAccess)RdaSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                if (blnInactive)
                {
                    rstTeamMember = objLib.GetRecordset(modContactProfileNeighborhood.strqACTIVE_SALE_TEAM_FOR_CONTACT_NBHDP, 1,
                        vntContactProfileNBHDID, modContactProfileNeighborhood.strfINACTIVE);
                    if (rstTeamMember.RecordCount > 0)
                    {
                        rstTeamMember.MoveFirst();
                        while (!(rstTeamMember.EOF))
                        {
                            rstTeamMember.Fields[modContactProfileNeighborhood.strfINACTIVE].Value = blnInactive;
                            rstTeamMember.MoveNext();
                        }
                    }
                    rstTeamMember = objLib.SaveRecordset(modContactProfileNeighborhood.strtCONTACT_TEAM_MEMBER, rstTeamMember);
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RdaSystem);

            }
        }

        /// <summary>
        /// Inactivate one team member and cancel all active activities.
        /// </summary>
        /// <param name="vntNBHDProfileTeamId">Id of the team member to be inactivated</param>
        /// <returns></returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// </history>
        public virtual void InactivateATeamMember(object vntNBHDProfileTeamId)
        {
            Recordset rstTeamMember = null;
            Recordset rstActivities = null;
            object vntNeighborhoodID = DBNull.Value;            object vntNBHDProfileId = DBNull.Value;            object vntContactId = DBNull.Value;            object vntEmployeeId = DBNull.Value;
            try
            {
                DataAccess objLib = (DataAccess)RdaSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;
                rstTeamMember = objLib.GetRecordset(vntNBHDProfileTeamId, modContactProfileNeighborhood.strtCONTACT_TEAM_MEMBER,
                    modContactProfileNeighborhood.strfMEMBER_TEAM_MEMBER_ID, modContactProfileNeighborhood.strfINACTIVE, modContactProfileNeighborhood.strfNEIGHBORHOOD_ID,
                    modContactProfileNeighborhood.strfNEIGHBORHOOD_PROFILE_ID, modContactProfileNeighborhood.strfEMPLOYEE_ID);
                if (rstTeamMember.RecordCount > 0)
                {
                    // Inactivate team member
                    rstTeamMember.MoveFirst();
                    rstTeamMember.Fields[modContactProfileNeighborhood.strfINACTIVE].Value = true;
                    objLib.SaveRecordset(modContactProfileNeighborhood.strtCONTACT_TEAM_MEMBER, rstTeamMember);
                    // Cancel active appointments
                    vntEmployeeId = rstTeamMember.Fields[modContactProfileNeighborhood.strfEMPLOYEE_ID].Value;
                    vntNeighborhoodID = rstTeamMember.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value;
                    vntNBHDProfileId = rstTeamMember.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_PROFILE_ID].Value;
                    // TODO (NETCOOLE) ISSUE: Method or data member not found: 'Fields'
                    vntContactId = RdaSystem.Tables[modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD].Fields[modContactProfileNeighborhood.strfCONTACT_ID].Index(vntNBHDProfileId);
                    rstActivities = objLib.GetRecordset("Sys: Incomplete Activities for Emp? NBHD? Cont?", 3, vntEmployeeId, vntNeighborhoodID, vntContactId, modContactProfileNeighborhood.strfACTIVITY_CANCELED, modContactProfileNeighborhood.strfAPPOINTMENT_CANCELED_DATE, modContactProfileNeighborhood.strfNOTES);
                    if (rstActivities.RecordCount > 0)
                    {
                        rstActivities.MoveFirst();
                        while (!(rstActivities.EOF))
                        {
                            rstActivities.Fields[modContactProfileNeighborhood.strfACTIVITY_CANCELED].Value = true;
                            rstActivities.Fields[modContactProfileNeighborhood.strfAPPOINTMENT_CANCELED_DATE].Value = DateTime.Today;
                            rstActivities.MoveNext();
                        }
                        objLib.SaveRecordset(modContactProfileNeighborhood.strtRN_APPOINTMENT, rstActivities);
                    }
                }

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RdaSystem);

            }
        }

        /// <summary>
        /// Cascade Inactivate a neighborhood profile
        /// </summary>
        /// <param name="vntContactProfileNBHDID">the neighborhood profile id</param>
        /// <param name="vntParamArray">array of parameters  (Inactive Reason Code, Activity Notes)</param>
        /// <returns></returns>
        /// <history>
        /// Revision #  Date        Author      Description
        /// 3.8.0.0     5/5/2006    CLangan     Converted to .Net C# code.
        /// 5.9.0       6/16/2010   KA          Converted to IP ASR fr OOB and added active/inactive timestamp
        /// 5.9.1       9/8/2010    Ka          commented out call to UPdateNBHDType, will not change NBHD Profile Type since it's too
        ///                                     difficult to figure out what the last status is if they opted in and out.
        ///                                     also commented out call to UpdatQuote,CancelActivities,& InactivateTeamMembers function
        /// </history>
        public virtual void InactivateNeighborhoodProfile(object vntContactProfileNBHDID, object vntParamArray)
        {
            Recordset rstContactProfileNBHD = null;
            object vntContactId = DBNull.Value;            object vntNeighborhoodID = DBNull.Value;            string strActivityNotes = String.Empty;
            string strInactiveReason = String.Empty;

            try
            {
                strActivityNotes = "";
                strInactiveReason = "";
                if (!((vntParamArray == null)))
                {
                    object[] paramArray = (object[])vntParamArray;
                    if (paramArray.Length > 0)
                    {
                        if (!(Convert.IsDBNull(paramArray[0])) && !((paramArray[0] == null)))
                        {
                            strInactiveReason = TypeConvert.ToString(paramArray[0]);
                        }
                    }
                    //ka 6-16-10 changed from >= to >
                    //if (paramArray.Length >= 1)
                    if (paramArray.Length > 1)
                    {
                        strActivityNotes = TypeConvert.ToString(paramArray[1]);
                    }
                }
                //KA 6/16/10 added tic_opt_Edit_Date
                DataAccess objLib = (DataAccess)RdaSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                rstContactProfileNBHD = objLib.GetRecordset(vntContactProfileNBHDID, modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD,
                    modContactProfileNeighborhood.strfTYPE, modContactProfileNeighborhood.strfCONTACT_ID, modContactProfileNeighborhood.strfINACTIVE,
                    modContactProfileNeighborhood.strfINACTIVE_DATE, modContactProfileNeighborhood.strfNEIGHBORHOOD_ID, 
                    modContactProfileNeighborhood.strfINACTIVE_REASON_ID,"TIC_Opt_Edit_Date");
                if (rstContactProfileNBHD.RecordCount <= 0)
                {
                    return;
                }
                // Set neighborhood profiles inactive, inactive date and inactive reason id fields.
                rstContactProfileNBHD.MoveFirst();
                rstContactProfileNBHD.Fields[modContactProfileNeighborhood.strfINACTIVE].Value = true;
                rstContactProfileNBHD.Fields[modContactProfileNeighborhood.strfINACTIVE_DATE].Value = DateTime.Today;
                //KA 6/16/10 added tic_opt_Edit_Date
                rstContactProfileNBHD.Fields["TIC_Opt_Edit_Date"].Value = DateTime.Now;

                if (strInactiveReason != "")
                {
                    rstContactProfileNBHD.Fields[modContactProfileNeighborhood.strfINACTIVE_REASON_ID].Value = RdaSystem.Tables[modContactProfileNeighborhood.strtINACTIVE_REASON].Fields[modContactProfileNeighborhood.strfREASON_CODE].Find(strInactiveReason);
                }
                objLib.SaveRecordset(modContactProfileNeighborhood.strtCONTACT_PROFILE_NEIGHBORHOOD, rstContactProfileNBHD);

                // Update Neighborhood Profile Type
                ContactProfileNeighborhood objContProfileNBHD = (ContactProfileNeighborhood)RdaSystem.ServerScripts[modContactProfileNeighborhood.strsCONTACT_PROFILE_NBHD].CreateInstance();
                //KA 9/8/10  commented out call to type when inactivating
                //objContProfileNBHD.UpdateNBHDPType(vntContactProfileNBHDID);

                // Cascasde to inactivate neighborhood profile's quotes, activities and profile team members
                vntContactId = rstContactProfileNBHD.Fields[modContactProfileNeighborhood.strfCONTACT_ID].Value;
                vntNeighborhoodID = rstContactProfileNBHD.Fields[modContactProfileNeighborhood.strfNEIGHBORHOOD_ID].Value;
                //KA 9/8/10  commented out call to UpdatQuote,CancelActivities,& InactivateTeamMembers when inactivating - since we are not update profile type
                //UpdateQuote(vntNeighborhoodID, vntContactId);
                //CancelActivities(vntNeighborhoodID, vntContactId, strActivityNotes);
                //InactivateTeamMembers(vntContactProfileNBHDID, true);

                return;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RdaSystem);

            }
        }


    }

}
