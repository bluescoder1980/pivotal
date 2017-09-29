// ###################################################################################################################################
// IMPORTANT: amcnab Aug 2010 - The following BM/Target Queries are used by this class.  They should be RTR'd into your BM before use.
//
// INT - Active Release_Adjustments with Type ? Phase Name ? Ext Nbhd Id ?
// INT - Division Product for Region ? Code ?
// INT - Elevation for Plan ? Code ?
// INT - NBHD_Phase with Nbhd Id ? Phase Name ?
// INT - Neighborhood with External Source Community Id ?
// INT - Plan for Neighborhood ? Code ?
// ###################################################################################################################################

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

// Pivotal-specific namespaces
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Choice;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Form;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;
using Pivotal.Interop.COMAdminLib;

namespace CRM.Pivotal.IP.SAPToHIPIntegration
{
    class IntegrationUtility
    {

        /// <summary>
        /// This method is used to dump debug info if the .NET debugger doesn't work
        /// </summary>
        /// <param name="dbg"></param>
        public void DebugWriter(string dbg)
        {
            FileInfo t = new FileInfo("C:/temp/debug.txt");
            StreamWriter tex = t.AppendText();
            DateTime now = DateTime.Now;
            string date = now.ToShortDateString();
            string time = now.ToLongTimeString();
            tex.WriteLine("**********************************************");
            tex.WriteLine("TimeStamp: " + date + "@ " + time);
            tex.WriteLine(dbg);
            tex.WriteLine("**********************************************");
            tex.Close();
        }

        /// <summary>
        /// This function will handle null values by setting the string to "" if the 
        /// value comes in from the DB as a null
        /// </summary>
        /// <param name="strVal"></param>
        /// <returns></returns>
        public string HandleNullValues(string strVal)
        {
            if (DBNull.Value.Equals(strVal))
            {
                strVal = "";
            }
            return strVal;
        }

        /// <summary>
        /// This method is used for lookups so that all the classes in the PivotalE1Integration59 classes
        /// can use
        /// </summary>
        /// <param name="rsys">IRSystem Object passed from calling AppServer Rule</param>
        /// <param name="pForm">IRForm Object that is tied to the calling AppServer Rule</param>
        /// <param name="recordset">The Primary recordset</param>
        /// <param name="arrLookupValues"></param>
        /// <param name="arrLookupFieldValues">Object Array containting the field values to look up</param>
        /// <param name="arrSetFieldValues">Object Array containing the field values to set</param>
        /// <param name="tablename">table name of the field being set</param>
        /// <param name="queryname">query name used to do lookup</param>
        /// <param name="tableValues">table field</param>
        public object[] SetValuesByLookUp(IRSystem7 rsys, IRForm pForm, Recordset recordset,
                                          object[] arrLookupValues, object[] arrLookupFieldValues, 
                                          object[] arrSetFieldValues, string tablename,
                                          string queryname, object[] tableValues)
        {
            try
            {
                IRDataset4 rdstDataset = null;
                Recordset rstRecordset = null;
                object vntRecordId = null;
                int i = 0;
                int intLookupRange = 0;

                //Don//t do any thing if this is not null
                //means that override lookup values are being used
                //instead of incoming recordset
                if (arrLookupValues != null)
                {
                    //Place Holder
                }
                //Do look up on field values from form
                else if (arrLookupFieldValues != null)
                {
                    //Get Number of rows in array (assumption is that for this object
                    //array the inner array//s will be of the same dimensions)
                    intLookupRange = arrLookupFieldValues.Length;
                    //Use this array to set new look up fields
                    arrLookupValues = new object[intLookupRange];

                    //loop through the object array and configure lookup
                    for (i = 0; i < arrLookupFieldValues.Length; i++)
                    {
                        //Set tmp array with value of object array
                        object[] tmpObjArr = null;
                        tmpObjArr = (object[])arrLookupFieldValues.GetValue(i);



                        //Check to see if this is a disconnected field
                        if (Convert.ToBoolean(tmpObjArr.GetValue(0)) == true)
                        {

                            //Get Disconnected field value and set it as a lookup value
                            //1) RSystem Object
                            //2) Incoming recorset
                            //3) field to do lookup on
                            //4) Segment name
                            arrLookupValues[i] = this.GetDisconnected(rsys, pForm, recordset,
                                tmpObjArr.GetValue(2).ToString(), tmpObjArr.GetValue(1).ToString());
                        }
                        else
                        {
                            //Not a disconnected field so just set the lookup value 
                            arrLookupValues[i] = new object[] { tmpObjArr.GetValue(2).ToString(), recordset.Fields[tmpObjArr.GetValue(2)].Value };
                        }
                    }
                }

                //Now that we set the look up values, we need to create a Dataset in order to query for the value we want

                //Create Pivotal DataSet Object
                rdstDataset = (IRDataset4)rsys.CreateDataset();

                //Run the Return Lookup procedure to get recordset
                //1) RSystem Object
                //2) DataSet Object we just created
                //3) table name to query on
                //4) query to run
                //5) Ignore Permissions flag
                //6) Lookup values created above

                rstRecordset = ReturnLookUp(rsys, rdstDataset, tablename, tableValues, queryname, true, arrLookupValues);

                //Check the Recordset for value to set
                if (null != rstRecordset)
                {
                    //If a value was returned then get it
                    if (rstRecordset.RecordCount > 0)
                    {
                        rstRecordset.MoveFirst();

                        //check the table values to see if any data is there
                        for (int j = 0; j < tableValues.Length; j++)
                        {
                            //If fields have been set
                            if (null != arrSetFieldValues)
                            {
                                //Set the primary recordset with values returned from teh query
                                recordset.Fields[arrSetFieldValues[j]].Value = rstRecordset.Fields[tableValues[j]].Value;
                            }

                            //set the Id with the returned value
                            vntRecordId = rstRecordset.Fields[tableValues[0]].Value;
                        }
                    }
                    else
                    {
                        vntRecordId = DBNull.Value;
                    }
                }
                else
                {
                    vntRecordId = DBNull.Value;
                }

                if (null != arrSetFieldValues)
                {
                    recordset.Fields[arrSetFieldValues[0]].Value = vntRecordId;
                }

                //return build object with returned       
                object[] arrObj = new object[] { vntRecordId, rstRecordset };
                return arrObj;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rsys);
            }
        }

        /// <summary>
        /// Used by the SetValuesByLookUp method to get get the values used by lookup
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rDS"></param>
        /// <param name="tablename"></param>
        /// <param name="arrFields"></param>
        /// <param name="queryName"></param>
        /// <param name="blnIgnorePerm"></param>
        /// <param name="arrParams"></param>
        /// <returns></returns>
        public Recordset ReturnLookUp(IRSystem7 rSys, IRDataset4 rDS, string tablename,
                                      object[] arrFields, string queryName, bool blnIgnorePerm, 
                                      object[] arrParams)
        {
            try
            {
                rDS.TableName = tablename;

                for (int i = 0; i < arrFields.Length; i++)
                {
                    rDS.Fields.Append(arrFields[i]);
                }

                // JB 09Jun2008: changed from OR(||) test to AND(&&)
                if (queryName != null && queryName != "")
                {
                    rDS.Query = rSys.Queries[queryName];
                    rDS.IgnorePermissions = blnIgnorePerm;

                    if (null != arrParams)
                    {
                        for (int j = 0; j < arrParams.Length; j++)
                        {
                            //Set tmp array with value of object array
                            object[] tmpObjArr = null;

                            //If arrParams is object array do the following
                            if (arrParams.GetValue(j).GetType() == typeof(System.Object[]))
                            {
                                tmpObjArr = (object[])arrParams.GetValue(j);

                                rDS.SetParameter(j + 1, tmpObjArr[1]);
                            }
                            //If string then get value from object array
                            else if (arrParams.GetValue(j).GetType() == typeof(System.String))
                            {
                                rDS.SetParameter(j + 1, arrParams[j].ToString());
                            }
                            //If object then get value and set)
                            else if (arrParams.GetValue(j).GetType() == typeof(System.Byte[]))
                            {
                                rDS.SetParameter(j + 1, arrParams[j]);
                            }
                        }
                    }
                }

                return rDS.BuildRecordset(null);
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Get Disconnected field by using the UIAccess Pivotal Object
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="pform"></param>
        /// <param name="rstPrimary"></param>
        /// <param name="fieldName"></param>
        /// <param name="segName"></param>
        /// <returns></returns>
        public object[] GetDisconnected(IRSystem7 rSys, IRForm pform, Recordset rstPrimary, string fieldName, string segName)
        {
            UIAccess objPLFunctionLib = (UIAccess)rSys.ServerScripts[AppServerRuleData.UIAccessAppServerRuleName].CreateInstance();
            string strDisconnectedFldName = string.Empty;
            object vntDisconnectedFldVal = null;

            strDisconnectedFldName = objPLFunctionLib.GetDisconnectedFieldName(pform.FormName, fieldName, segName);

            if (DBNull.Value == rstPrimary.Fields[strDisconnectedFldName].Value)
            {
                vntDisconnectedFldVal = "";
            }
            else
            {
                vntDisconnectedFldVal = rstPrimary.Fields[strDisconnectedFldName].Value;
            }

            return new object[] { strDisconnectedFldName, vntDisconnectedFldVal.ToString() };
        }

        /// <summary>
        /// This method will be used by the Contact Integration touchpoint to create a new 
        /// Community profile for a new or updated contact if one doesn//t exist
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="pform"></param>
        /// <param name="rs"></param>
        /// <param name="vntContactId"></param>
        public void SetCommProfile(IRSystem7 rSys, IRForm pform, Recordset rs, object vntContactId)
        {
            try
            {
                Recordset rstCommProfile = null;
                object[] vntReturn = null;
                object vntCommunityId = null;
                object vntDivisionId = null;

                //Initialize profile flag
                bool blnCreateProfile = false;

                //Set Disconnected field to do lookup on form
                object[] arrLookUpFieldVals = new object[] 
                { 
                    new object[] {true, pform.Segments[1].SegmentName, IntegrationConstants.gstrfCONTACT_COMMUNITY}
                };

                //Set the fields to be set with the lookup return value
                //- This object array can have multiple fields to set but for this touchpoint we 
                //  only need to set the Neighborhood_Id
                object[] arrSetFieldVals = new object[] 
                { 
                    IntegrationConstants.gstrfNEIGHBORHOOD_ID
                };

                IntegrationUtility util = new IntegrationUtility();

                //This will set the community Id on the recordset if one exists
                vntReturn = util.SetValuesByLookUp(rSys, pform, rs, null, arrLookUpFieldVals,
                    null, IntegrationConstants.gstrtNEIGHBORHOOD,
                    IntegrationConstants.gstrqCOMMUNITY_BY_EXTERNAL_SOURCE_ID, arrSetFieldVals);

                vntCommunityId = vntReturn[0];

                //Get the DivisionId from the Neighborhood Id returned
                vntDivisionId = rSys.Tables[IntegrationConstants.gstrtNEIGHBORHOOD]
                    .Fields[IntegrationConstants.gstrfDIVISION_ID].Index(vntCommunityId);

                //Create a profile if one doesn//t exist
                object[] vntAllFieldNames = new object[] {IntegrationConstants.gstrfDIVISION_ID, 
                    IntegrationConstants.gstrfNEIGHBORHOOD_ID, IntegrationConstants.strfCONTACT_ID, 
                    IntegrationConstants.gstrfTYPE, IntegrationConstants.gstrfSYS_TRUE};

                //Create dataset to work with
                IRDataset4 rdstDataset = (IRDataset4)rSys.CreateDataset();

                //Check if Comm Profile already exists for contact and community
                rstCommProfile = this.ReturnLookUp(rSys, rdstDataset, IntegrationConstants.gstrtCONTACT_PROFILE_NEIGHB,
                    vntAllFieldNames, IntegrationConstants.gstrqCONTACT_PROFILE_NEIGHB_BY_CONTACT, true,
                    new object[] { vntContactId, vntCommunityId });

                //Check to see if recordset has any records returned from lookup
                if (null != rstCommProfile)
                {
                    if (rstCommProfile.RecordCount > 0)
                    {
                        //Still false
                    }
                    else
                    {
                        //This means that Comm Profile needs to be created
                        blnCreateProfile = true;
                    }
                }

                if (blnCreateProfile)
                {
                    //Set values to be used for creating new Comm Profile
                    object[] vntAllFieldValues = null;
                    if (rs.Fields[IntegrationConstants.strfTYPE].Value.ToString() != "Prospect")
                    {
                        vntAllFieldValues = new object[] { vntDivisionId, vntCommunityId, vntContactId, 
                            IntegrationConstants.gstrBUYER, true};
                    }
                    else
                    {
                        vntAllFieldValues = new object[] { vntDivisionId, vntCommunityId, vntContactId, 
                            IntegrationConstants.gstrPROSPECT, true};
                    }
                    //Creawte new recrd
                    rstCommProfile.AddNew(vntAllFieldNames, vntAllFieldValues);

                    rdstDataset.SaveRecordset(rstCommProfile);
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will be used by the Contact Integration touchpoint to create a new 
        /// Community profile for a new or updated contact if one doesn//t exist
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="pform"></param>
        /// <param name="rs"></param>
        /// <param name="vntContactId"></param>
        public void SetLeadCommProfile(IRSystem7 rSys, IRForm pform, Recordset rs, object vntContactId)
        {
            try
            {
                Recordset rstCommProfile = null;
                object[] vntReturn = null;
                object vntCommunityId = null;
                object vntDivisionId = null;

                //Initialize profile flag
                bool blnCreateProfile = false;

                //Set Disconnected field to do lookup on form
                object[] arrLookUpFieldVals = new object[] 
                { 
                    new object[] {true, pform.Segments[1].SegmentName, IntegrationConstants.gstrfCONTACT_COMMUNITY}
                };

                //Set the fields to be set with the lookup return value
                //- This object array can have multiple fields to set but for this touchpoint we 
                //  only need to set the Neighborhood_Id
                object[] arrSetFieldVals = new object[] 
                { 
                    IntegrationConstants.gstrfNEIGHBORHOOD_ID
                };

                IntegrationUtility util = new IntegrationUtility();

                //This will set the community Id on the recordset if one exists
                vntReturn = util.SetValuesByLookUp(rSys, pform, rs, null, arrLookUpFieldVals,
                    null, IntegrationConstants.gstrtNEIGHBORHOOD,
                    IntegrationConstants.gstrqCOMMUNITY_BY_EXTERNAL_SOURCE_ID, arrSetFieldVals);

                vntCommunityId = vntReturn[0];

                //Get the DivisionId from the Neighborhood Id returned
                vntDivisionId = rSys.Tables[IntegrationConstants.gstrtNEIGHBORHOOD]
                    .Fields[IntegrationConstants.gstrfDIVISION_ID].Index(vntCommunityId);

                //Create a profile if one doesn//t exist
                object[] vntAllFieldNames = new object[] {IntegrationConstants.gstrfDIVISION_ID, 
                    IntegrationConstants.gstrfNEIGHBORHOOD_ID, IntegrationConstants.strfLEAD_ID, 
                    IntegrationConstants.gstrfTYPE, IntegrationConstants.gstrfSYS_TRUE};

                //Create dataset to work with
                IRDataset4 rdstDataset = (IRDataset4)rSys.CreateDataset();

                //Check if Comm Profile already exists for contact and community
                rstCommProfile = this.ReturnLookUp(rSys, rdstDataset, IntegrationConstants.gstrtCONTACT_PROFILE_NEIGHB,
                    vntAllFieldNames, IntegrationConstants.gstrqCONTACT_PROFILE_NEIGHB_BY_LEAD, true,
                    new object[] { vntContactId, vntCommunityId });

                //Check to see if recordset has any records returned from lookup
                if (null != rstCommProfile)
                {
                    if (rstCommProfile.RecordCount > 0)
                    {
                        //Still false
                    }
                    else
                    {
                        //This means that Comm Profile needs to be created
                        blnCreateProfile = true;
                    }
                }

                if (blnCreateProfile)
                {
                    //Set values to be used for creating new Comm Profile
                    object[] vntAllFieldValues = null;
                    if (rs.Fields[IntegrationConstants.strfTYPE].Value.ToString() != "Prospect")
                    {
                        vntAllFieldValues = new object[] { vntDivisionId, vntCommunityId, vntContactId, 
                            IntegrationConstants.gstrBUYER, true};
                    }
                    else
                    {
                        vntAllFieldValues = new object[] { vntDivisionId, vntCommunityId, vntContactId, 
                            IntegrationConstants.gstrPROSPECT, true};
                    }
                    // Create new recrd
                    rstCommProfile.AddNew(vntAllFieldNames, vntAllFieldValues);
                    rdstDataset.SaveRecordset(rstCommProfile);
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Find the Pivotal Rn_Employee_User_Id by login name
        /// </summary>
        /// <param name="vntUserId"></param>
        /// <returns></returns>
        public object FindUser(IRSystem7 rSys, string strUserLogin)
        {
            try
            {
                //Use this object to get new recordset
                DataAccess objLib = (DataAccess)
                   rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                //Get UserId by login name
                object vntUserId = rSys.Tables[IntegrationConstants.strtUSERS].Fields[IntegrationConstants.strfUSERS_ID].FindValue(
                    rSys.Tables[IntegrationConstants.strtUSERS].Fields[IntegrationConstants.strfLOGIN_NAME],
                    strUserLogin);

                //Did we find it? If not insert
                if (DBNull.Value.Equals(vntUserId) || null == vntUserId)
                {
                    //Insert 
                    Recordset rstUsers = null;

                    //Specify which fields to set in new recordset
                    object[] arrFieldList = new object[] { IntegrationConstants.strfLOGIN_NAME };
                    object[] arrFieldValues = new object[] { strUserLogin };
                    //Get new recordset object
                    rstUsers = objLib.GetNewRecordset(IntegrationConstants.strtUSERS, arrFieldList);
                    rstUsers.AddNew(arrFieldList, arrFieldValues);
                    //Save recordset to DB
                    objLib.SaveRecordset(IntegrationConstants.strtUSERS, rstUsers);
                    rstUsers.Close();

                    //Now get the User_Id of the recently added record from above
                    vntUserId = rSys.Tables[IntegrationConstants.strtUSERS].Fields[IntegrationConstants.strfUSERS_ID].FindValue(
                        rSys.Tables[IntegrationConstants.strtUSERS].Fields[IntegrationConstants.strfLOGIN_NAME],
                        strUserLogin);
                }

                if (DBNull.Value.Equals(vntUserId) || null == vntUserId)
                {
                    throw new PivotalApplicationException("Cannot find nor create User Id, Employee must have a User Id.");
                }

                //Return user Id
                return vntUserId;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Find the Role by Pivotal E1 Role Name
        /// </summary>
        /// <param name="strRoleName"></param>
        /// <returns></returns>
        public object FindRole(IRSystem7 rSys, string strRoleName)
        {
            try
            {
                //Do lookup on role passed in
                object vntResult = rSys.Tables[IntegrationConstants.strtTEAM_MEMBER_ROLE].Fields[IntegrationConstants.strfTEAM_MEMBER_ROLE_ID].FindValue(
                 rSys.Tables[IntegrationConstants.strtTEAM_MEMBER_ROLE].Fields[IntegrationConstants.strfROLE_NAME],
                    strRoleName);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }

                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Find County by county name from E1
        /// </summary>
        /// <param name="countyname"></param>
        /// <returns></returns>
        public object FindCounty(IRSystem7 rSys, string countyname)
        {
            try
            {
                object vntResult = rSys.Tables[IntegrationConstants.strtCOUNTY].Fields[IntegrationConstants.strfCOUNTY_ID].FindValue(
                    rSys.Tables[IntegrationConstants.strtCOUNTY].Fields[IntegrationConstants.strfCOUNTY_NAME],
                    countyname);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }

                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Find Time Zone by TimeZone passed in from Edwards
        /// </summary>
        /// <param name="timezone"></param>
        /// <returns></returns>
        public object FindTimeZone(IRSystem7 rSys, string timezone)
        {
            try
            {
                object vntResult = rSys.Tables[IntegrationConstants.strtTIME_ZONE].Fields[IntegrationConstants.strfTIME_ZONE_ID].FindValue(
                   rSys.Tables[IntegrationConstants.strtTIME_ZONE].Fields[IntegrationConstants.strfTIME_ZONE_NAME],
                   timezone);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }
                //Return result
                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will check the community to make sure it is
        /// flagged for integration and return a true or false.
        /// </summary>
        /// <param name="strSync"></param>
        /// <returns></returns>
        public bool FindCommunitySyncFlag(IRSystem7 rSys, string strSync)
        {
            try
            {
                bool blnReturn = false;

                object vntResult = rSys.Tables[IntegrationConstants.strtINT_LIVE_COMMUNITY].Fields[IntegrationConstants.strfINTEGRATED].FindValue(
                       rSys.Tables[IntegrationConstants.strtINT_LIVE_COMMUNITY].Fields[IntegrationConstants.strfCOMMUNITY],
                       strSync);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    blnReturn = false;
                }
                else
                {
                    blnReturn = Convert.ToBoolean(vntResult);
                }
                //Return result
                return blnReturn;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will check the community to make sure it is
        /// flagged for integration and return a true or false.
        /// </summary>
        /// <param name="strSync"></param>
        /// <returns></returns>
        public bool FindDivisionSyncFlag(IRSystem7 rSys, string strSync)
        {
            try
            {
                bool blnReturn = false;

                object vntResult = rSys.Tables[IntegrationConstants.strtDIVISION].Fields[IntegrationConstants.strfDIVISION_MIGRATED].FindValue(
                       rSys.Tables[IntegrationConstants.strtDIVISION].Fields[IntegrationConstants.strfDIVISION_NUMBER],
                       strSync);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    blnReturn = false;
                }
                else
                {
                    blnReturn = Convert.ToBoolean(vntResult);
                }

                //Return result
                return blnReturn;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }

        }

        /// <summary>
        /// This method will use the parameters passed in to find the appropriate opportunity record
        /// to use for inbound integration
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="strCommunityID"></param>
        /// <param name="strPhaseID"></param>
        /// <param name="strPlanID"></param>
        /// <param name="strELevationID"></param>
        /// <param name="strLotId"></param>
        /// <returns></returns>
        public object FindOpportunity(IRSystem7 rSys, string strCommunityID, string strPhaseID,
            string strPlanID, string strELevationID, string strLotId)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOpp = new Recordset();
                object vntReturn = null;
                StringBuilder sqlText = new StringBuilder();

                sqlText.Append("SELECT ");
                sqlText.Append("    o.Opportunity_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("    Opportunity o ");
                sqlText.Append("    INNER JOIN Neighborhood n ON ( n.Neighborhood_Id = o.Neighborhood_Id ) ");
                sqlText.Append("    INNER JOIN NBHD_Phase np ON ( np.NBHD_Phase_Id = o.NBHD_Phase_Id ) ");
                //sqlText.Append("    INNER JOIN NBHDP_Product p ON ( p.NBHDP_Product_Id = o.Plan_Name_Id ) ");
                sqlText.Append("    INNER JOIN Product lot ON ( lot.Product_Id = o.Lot_Id ) ");
                sqlText.Append("WHERE ");
                sqlText.Append("    n.External_Source_Community_Id = '" + strCommunityID + "'");
                sqlText.Append(" AND ( np.External_Source_Id = '" + strCommunityID + "-" + strPhaseID + "')");
                //sqlText.Append(" AND ( p.Type= 'Plan' AND p.External_Source_Id = '" + 
                //strCommunityID + "-" + strPhaseID + "-" + strPlanID + "-" + strELevationID  + "')");
                sqlText.Append(" AND ( lot.Business_Unit_Lot_Number = '" + strLotId + "')");
                sqlText.Append(" AND ( ( o.Pipeline_Stage = 'Quote' AND o.Status = 'Inventory' AND (o.Inactive = 0 OR o.Inactive = null)) OR ( o.Pipeline_Stage = 'Contract' AND o.Status = 'In Progress') OR ( o.Pipeline_Stage = 'Contract' AND o.Status = 'Closed') ) ");

                rstOpp = objLib.GetRecordset(sqlText.ToString());

                if (rstOpp.RecordCount > 0)
                {
                    rstOpp.MoveFirst();
                    //Get oppportunity Id
                    vntReturn = rstOpp.Fields[0].Value;
                    rstOpp.Close();
                }
                else
                {
                    string strErrMsg = "Cannot find Opportunity record for this record.";
                    //Cannot find opportunity raise error
                    throw new PivotalApplicationException(strErrMsg);
                }

                return vntReturn;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }

        }

        /// <summary>
        /// This method will use the parameters passed in to find the appropriate opportunity record
        /// to use for inbound integration
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="strProspectID"></param>
        /// <returns></returns>
        public object FindQuote(IRSystem7 rSys, string strProspectID)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOpp = new Recordset();
                object vntReturn = null;
                StringBuilder sqlText = new StringBuilder();

                sqlText.Append("SELECT ");
                sqlText.Append("    o.Opportunity_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("    Opportunity o ");
                sqlText.Append("WHERE ");
                sqlText.Append(" o.External_Source_Id = '" + strProspectID + "'");
                sqlText.Append(" AND ( o.Pipeline_Stage = 'Quote' AND o.Status = 'In Progress' AND (o.Inactive = 0 OR o.Inactive = null)) ");

                rstOpp = objLib.GetRecordset(sqlText.ToString());

                if (rstOpp.RecordCount > 0)
                {
                    rstOpp.MoveFirst();
                    //Get oppportunity Id
                    vntReturn = rstOpp.Fields[0].Value;
                    rstOpp.Close();
                }
                else
                {
                    string strErrMsg = "Cannot find Opportunity record for this record.";
                    //Cannot find opportunity raise error
                    throw new PivotalApplicationException(strErrMsg);
                }

                return vntReturn;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }


        /// <summary>
        /// This method will use the parameters passed in to find the appropriate plan and elevation ID
        /// to use for inbound integration
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntOpportunityID"></param>
        /// <returns></returns>
        public string FindOpportunityPlanElev(IRSystem7 rSys, object vntOpportunityId)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOpp = new Recordset();
                string strReturn = "";
                StringBuilder sqlText = new StringBuilder();

                sqlText.Append("SELECT ");
                sqlText.Append("    np.Code_ ");
                sqlText.Append("FROM ");
                sqlText.Append("    Opportunity o ");
                sqlText.Append("    INNER JOIN nbhdp_product np ON ( o.Plan_Name_Id = np.nbhdp_product_id ) ");
                sqlText.Append("WHERE ");
                sqlText.Append("    o.opportunity_Id = " + rSys.IdToString(vntOpportunityId));

                rstOpp = objLib.GetRecordset(sqlText.ToString());

                if (rstOpp.RecordCount > 0)
                {
                    rstOpp.MoveFirst();
                    //Get oppportunity Id
                    strReturn = (string)rstOpp.Fields[0].Value;
                    rstOpp.Close();
                }
                else
                {
                    string strErrMsg = "Cannot find opportunity record for this record.";
                    //Cannot find opportunity raise error
                    throw new PivotalApplicationException(strErrMsg);
                }

                return strReturn;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }


        /// <summary>
        /// This method will return the neighborhood product assocaited with this contract option
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="strCommunityID"></param>
        /// <param name="strPhaseID"></param>
        /// <param name="strPlanID"></param>
        /// <param name="strElevationID"></param>
        /// <param name="strOptionId"></param>
        /// <param name="strProductName"></param>
        /// <param name="strProductCode"></param>
        /// <param name="vntDivisionProductId"></param>
        /// <param name="vntConstrStgId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public object GetNBHDP_Product(IRSystem7 rSys, string strExternalID, ref string strProductName, ref string strProductCode,
            ref object vntDivisionProductId, ref object vntConstrStgId, ref object vntCategoryId, ref string strOptionAvailTo,
            ref object vntSubCategoryId)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstNBHDP = new Recordset();
                StringBuilder sqlText = new StringBuilder();
                object vntReturn = null;

                sqlText.Append("SELECT ");
                sqlText.Append("    npp.NBHDP_Product_Id, ");
                sqlText.Append("    npp.Product_Name, ");
                sqlText.Append("    npp.Code_, ");
                sqlText.Append("    dp.Division_Product_Id, ");
                sqlText.Append("    npp.NBHDP_Product_Id, ");
                sqlText.Append("    npp.Product_Name, ");
                sqlText.Append("    dp.Construction_Stage_Id, ");
                sqlText.Append("    npp.Category_Id, ");
                sqlText.Append("    npp.Option_Available_To, ");
                sqlText.Append("    npp.MI_Sub_Category_Id ");
                sqlText.Append(" FROM ");
                sqlText.Append("    NBHDP_Product npp ");
                sqlText.Append("    INNER JOIN Division_Product dp ON ( dp.Division_Product_Id = npp.Division_Product_Id ) ");
                sqlText.Append("WHERE ");
                sqlText.Append("    npp.External_Source_Id = '" + strExternalID + "'");

                rstNBHDP = objLib.GetRecordset(sqlText.ToString());

                if (rstNBHDP.RecordCount > 0)
                {
                    rstNBHDP.MoveFirst();
                    //Set values by reference
                    strProductName = rstNBHDP.Fields[IntegrationConstants.strfDIV_PRODUCT_NAME].Value.ToString();
                    strProductCode = rstNBHDP.Fields[IntegrationConstants.strfCODE_].Value.ToString();
                    vntDivisionProductId = rstNBHDP.Fields[IntegrationConstants.strfDIVISION_PRODUCT_ID].Value;
                    vntConstrStgId = rstNBHDP.Fields[IntegrationConstants.strfCONSTRUCTION_STAGE_ID].Value;
                    vntCategoryId = rstNBHDP.Fields[IntegrationConstants.strfCATEGORY_ID].Value;
                    strOptionAvailTo = rstNBHDP.Fields[IntegrationConstants.strfOPTION_AVAILABLE_TO].Value.ToString();
                    vntSubCategoryId = rstNBHDP.Fields[IntegrationConstants.strfMI_SUB_CATEGORY_ID].Value;
                    vntReturn = rstNBHDP.Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID].Value;

                    //Clean Up
                    rstNBHDP.Close();

                    //Return neighorhood product id
                    return vntReturn;
                }
                else
                {
                    strProductName = string.Empty;
                    strProductCode = string.Empty;
                    vntDivisionProductId = null;
                    vntConstrStgId = null;
                    vntCategoryId = null;
                    rstNBHDP.Close();
                    //Return neighorhood product id
                    return null;
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will return the neighborhood product assocaited with this contract option
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="strCommunityID"></param>
        /// <param name="strPhaseID"></param>
        /// <param name="strPlanID"></param>
        /// <param name="strElevationID"></param>
        /// <param name="strOptionId"></param>
        /// <param name="strProductName"></param>
        /// <param name="strProductCode"></param>
        /// <param name="vntDivisionProductId"></param>
        /// <param name="vntConstrStgId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public object FindNBHDP_Product(IRSystem7 rSys, string strDivNum, string strCommunityID, string strPhaseID,
            string strPlanID, string strElevationID, string strOptionId, ref string strProductName, ref string strProductCode,
            ref object vntDivisionProductId, ref object vntConstrStgId, ref object vntCategoryId, ref string strOptionAvailTo,
            ref object vntSubCatId)
        {
            object vntReturn = null;

            try
            {
                vntReturn = GetNBHDP_Product(rSys, strDivNum + "-" + strCommunityID + "-" + strPhaseID + "-" + strPlanID + "-" + strElevationID + "-" + strOptionId,
                    ref strProductName, ref strProductCode, ref vntDivisionProductId, ref vntConstrStgId, ref vntCategoryId, ref strOptionAvailTo, ref vntSubCatId);

                if (vntReturn == null)
                {
                    vntReturn = GetNBHDP_Product(rSys, strDivNum + "-" + strCommunityID + "-" + strPhaseID + "-" + strPlanID + "-" + "+" + "-" + strOptionId,
                        ref strProductName, ref strProductCode, ref vntDivisionProductId, ref vntConstrStgId, ref vntCategoryId, ref strOptionAvailTo, ref vntSubCatId);

                    if (vntReturn == null)
                    {
                        vntReturn = GetNBHDP_Product(rSys, strDivNum + "-" + strCommunityID + "-+-" + strPlanID + "-" + strElevationID + "-" + strOptionId,
                            ref strProductName, ref strProductCode, ref vntDivisionProductId, ref vntConstrStgId, ref vntCategoryId, ref strOptionAvailTo, ref vntSubCatId);

                        if (vntReturn == null)
                        {
                            vntReturn = GetNBHDP_Product(rSys, strDivNum + "-" + strCommunityID + "-+-" + strPlanID + "-" + "+" + "-" + strOptionId,
                                ref strProductName, ref strProductCode, ref vntDivisionProductId, ref vntConstrStgId, ref vntCategoryId, ref strOptionAvailTo, ref vntSubCatId);

                            if (vntReturn == null)
                            {
                                vntReturn = GetNBHDP_Product(rSys, strDivNum + "-" + "+-+-" + strPlanID + "-" + strElevationID,
                                    ref strProductName, ref strProductCode, ref vntDivisionProductId, ref vntConstrStgId, ref vntCategoryId, ref strOptionAvailTo, ref vntSubCatId);

                                if (vntReturn == null)
                                {
                                    vntReturn = GetNBHDP_Product(rSys, strDivNum + "-" + "+-+-" + strPlanID + "-+",
                                        ref strProductName, ref strProductCode, ref vntDivisionProductId, ref vntConstrStgId, ref vntCategoryId, ref strOptionAvailTo, ref vntSubCatId);

                                    if (vntReturn == null)
                                    {
                                        vntReturn = GetNBHDP_Product(rSys, strDivNum + "-" + strCommunityID + "-" + strPhaseID + "-+-+-" + strOptionId,
                                            ref strProductName, ref strProductCode, ref vntDivisionProductId, ref vntConstrStgId, ref vntCategoryId, ref strOptionAvailTo, ref vntSubCatId);

                                        if (vntReturn == null)
                                        {
                                            vntReturn = GetNBHDP_Product(rSys, strDivNum + "-" + strCommunityID + "-+-+-+-" + strOptionId,
                                                ref strProductName, ref strProductCode, ref vntDivisionProductId, ref vntConstrStgId, ref vntCategoryId, ref strOptionAvailTo, ref vntSubCatId);

                                            if (vntReturn == null)
                                            {
                                                vntReturn = GetNBHDP_Product(rSys, strDivNum + "-" + "+-+-+-+-" + strOptionId,
                                                ref strProductName, ref strProductCode, ref vntDivisionProductId, ref vntConstrStgId, ref vntCategoryId, ref strOptionAvailTo, ref vntSubCatId);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return vntReturn;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        /// <summary>
        /// This method will get the Opportunity Product (Contract Option by the Opportunity Id and the
        /// Neighborhood Id
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="oppid"></param>
        /// <param name="nbhdp_product_id"></param>
        /// <returns></returns>
        public object FindOpportunityProduct(IRSystem7 rSys, object oppid, object nbhdp_product_id)
        {
            try
            {

                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOppProd = new Recordset();
                StringBuilder sqlText = new StringBuilder();
                object vntReturn = null;

                sqlText.Append("SELECT ");
                sqlText.Append("    op.Opportunity__Product_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("    Opportunity__Product op ");
                sqlText.Append("WHERE ");
                sqlText.Append("    op.Opportunity_Id = " + rSys.IdToString(oppid));
                sqlText.Append(" AND op.NBHDP_Product_Id = " + rSys.IdToString(nbhdp_product_id));
                sqlText.Append(" AND op.Selected = 1 ");

                rstOppProd = objLib.GetRecordset(sqlText.ToString());

                if (rstOppProd.RecordCount > 0)
                {
                    rstOppProd.MoveFirst();
                    //Get oppportunity Id
                    vntReturn = rstOppProd.Fields[0].Value;
                    rstOppProd.Close();
                }

                //Return the OpportunityProduct id
                return vntReturn;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will get the customOpportunity Product (Contract Option by the Opportunity Id and the
        /// Neighborhood Id
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="oppid"></param>
        /// <returns></returns>
        public object FindCustomOpportunityProduct(IRSystem7 rSys, object oppid, string code)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOppProd = new Recordset();
                StringBuilder sqlText = new StringBuilder();
                object vntReturn = null;

                sqlText.Append("SELECT ");
                sqlText.Append("    op.Opportunity__Product_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("    Opportunity__Product op ");
                sqlText.Append("WHERE ");
                sqlText.Append("    op.Opportunity_Id = " + rSys.IdToString(oppid));
                sqlText.Append(" AND op.NBHDP_Product_Id is null ");
                sqlText.Append(" AND op.Selected = 1 ");
                sqlText.Append(" AND op.Code_ = '" + code + "'");

                rstOppProd = objLib.GetRecordset(sqlText.ToString());

                if (rstOppProd.RecordCount > 0)
                {
                    rstOppProd.MoveFirst();
                    //Get oppportunity Id
                    vntReturn = rstOppProd.Fields[0].Value;
                    rstOppProd.Close();
                }

                //Return the OpportunityProduct id
                return vntReturn;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }


        /// <summary>
        /// This method will locate other open quotes and for the same
        /// lot with the same option and updte other
        /// built or committed options
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntOppid"></param>
        /// <param name="vntConstructStgId"></param>
        /// <returns></returns>
        public bool UpdateBuiltOptionFlag(IRSystem7 rSys, object vntOppid, object vntConstructStgId)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOppProd = new Recordset();
                StringBuilder sqlText = new StringBuilder();
                bool blnUpdateOption = false;

                sqlText.Append("SELECT ");
                sqlText.Append("    1 ");
                sqlText.Append("FROM ");
                sqlText.Append("    Construction_Stage prodcs ");
                sqlText.Append("    INNER JOIN Construction_Stage oppprodcs ON ( oppprodcs.division_id = prodcs.division_id ) ");
                sqlText.Append("WHERE ");
                sqlText.Append("    prodcs.Construction_Stage_Ordinal >= oppprodcs.Construction_Stage_Ordinal ");
                sqlText.Append("AND prodcs.Construction_Stage_Id = ");
                sqlText.Append("    ( ");
                sqlText.Append("        SELECT lot.Construction_Stage_Id ");
                sqlText.Append("        FROM ");
                sqlText.Append("            Product Lot ");
                sqlText.Append("            INNER JOIN Opportunity o ON ( o.Lot_ID = lot.Product_Id ) ");
                sqlText.Append("        WHERE ");
                sqlText.Append("            o.Opportunity_Id =  " + rSys.IdToString(vntOppid));
                sqlText.Append("    ) ");
                sqlText.Append("AND oppprodcs.Construction_Stage_Id =  " + rSys.IdToString(vntConstructStgId));

                rstOppProd = objLib.GetRecordset(sqlText.ToString());

                if (rstOppProd.RecordCount > 0)
                {
                    rstOppProd.Close();
                    blnUpdateOption = true;
                }

                //Return the OpportunityProduct id
                return blnUpdateOption;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }


        /// <summary>
        /// This method will Update Option Details in a cascading fashion
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntOppProdId"></param>
        public void CascadeUpdateOptionDetails(IRSystem7 rSys, object vntOppProdId)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOppProd = new Recordset();
                StringBuilder sqlText = new StringBuilder();

                sqlText.Append("SELECT ");
                sqlText.Append("    op2.Opportunity__Product_Id, op2.SP_Committed, op2.Built_Option, ");
                sqlText.Append("    IsNull(op.SP_Committed, 0) AS [" + IntegrationConstants.strcCOMMITTED + "], IsNull(op.Built_Option, 0) AS [" + IntegrationConstants.strcBUILT + "] ");
                sqlText.Append("FROM ");
                sqlText.Append("    Opportunity__Product op ");
                sqlText.Append("    INNER JOIN Opportunity o ON o.Opportunity_Id = op.Opportunity_Id");
                sqlText.Append("    INNER JOIN Opportunity o2 ON ( o2.Lot_Id = o.Lot_Id ) ");
                sqlText.Append("    INNER JOIN Opportunity__Product op2 ON ( op2.Opportunity_Id = o2.Opportunity_Id  ) ");
                sqlText.Append("WHERE ");
                sqlText.Append("    o2.Opportunity_Id != o.Opportunity_Id ");
                sqlText.Append("AND op2.NBHDP_Product_Id = op.NBHDP_Product_Id ");
                sqlText.Append("AND o2.Pipeline_Stage = //Quote// ");
                sqlText.Append("AND o2.Status = //In Progress// ");
                sqlText.Append("AND op.Opportunity__Product_Id =  " + rSys.IdToString(vntOppProdId));

                rstOppProd = objLib.GetRecordset(sqlText.ToString());

                Recordset rstOppUpdate = null;

                if (rstOppProd.RecordCount > 0)
                {
                    rstOppProd.MoveFirst();

                    //For every record found
                    while (!(rstOppProd.EOF))
                    {

                        //Get record to update
                        rstOppUpdate = objLib.GetRecordset(rstOppProd.Fields[IntegrationConstants.strfOPPORTUNITY__PRODUCT_ID].Value,
                            IntegrationConstants.strtOPPORTUNITY__PRODUCT,
                            new object[] { IntegrationConstants.strfJDE_COMMITTED, IntegrationConstants.strfBUILT_OPTION });

                        //If committed flag is different and null then update 
                        if (DBNull.Value == rstOppUpdate.Fields[IntegrationConstants.strfJDE_COMMITTED].Value)
                        {
                            rstOppUpdate.Fields[IntegrationConstants.strfJDE_COMMITTED].Value =
                                  rstOppProd.Fields[IntegrationConstants.strfJDE_COMMITTED].Value;
                        }
                        else if (rstOppUpdate.Fields[IntegrationConstants.strfJDE_COMMITTED].Value !=
                            rstOppProd.Fields[IntegrationConstants.strfJDE_COMMITTED].Value)
                        {
                            rstOppUpdate.Fields[IntegrationConstants.strfJDE_COMMITTED].Value =
                                rstOppProd.Fields[IntegrationConstants.strfJDE_COMMITTED].Value;
                        }

                        //If Built flag is different and null then update 
                        if (DBNull.Value == rstOppUpdate.Fields[IntegrationConstants.strfBUILT_OPTION].Value)
                        {
                            rstOppUpdate.Fields[IntegrationConstants.strfBUILT_OPTION].Value =
                                  rstOppProd.Fields[IntegrationConstants.strfBUILT_OPTION].Value;
                        }
                        else if (rstOppUpdate.Fields[IntegrationConstants.strfBUILT_OPTION].Value !=
                            rstOppProd.Fields[IntegrationConstants.strfBUILT_OPTION].Value)
                        {
                            rstOppUpdate.Fields[IntegrationConstants.strfBUILT_OPTION].Value =
                                rstOppProd.Fields[IntegrationConstants.strfBUILT_OPTION].Value;
                        }

                        rstOppProd.MoveNext();
                    }

                    //Save updated recordset
                    objLib.SaveRecordset(IntegrationConstants.strtOPPORTUNITY__PRODUCT, rstOppUpdate);
                }

                rstOppUpdate.Close();
                rstOppProd.Close();
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Used by LotCompany integration to assign trade codes form JDE Edwards
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rstPrimary"></param>
        public object FindTrade(IRSystem7 rSys, string trade, Boolean createTrade, string tradeDescription)
        {
            try
            {
                object vntTradeId = rSys.Tables[IntegrationConstants.strtTRADE].Fields[IntegrationConstants.strfTRADE_ID].FindValue(
                    rSys.Tables[IntegrationConstants.strtTRADE].Fields[IntegrationConstants.strfTRADE_CODE],
                    trade);

                if ((null == vntTradeId || vntTradeId == DBNull.Value) && createTrade)
                {
                    //throw new PivotalApplicationException("Unable to locate the specified Trade_Code");
                    //Per Michal on 3/17/2010 we need to create this if it doesn't exist
                    vntTradeId = CreateTrade(rSys, trade, tradeDescription);
                }

                return vntTradeId;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Used by LotCompany integration to assign trade codes form JDE Edwards
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rstPrimary"></param>
        public void AssignTradeCodes(IRSystem7 rSys, Recordset rstPrimary)
        {
            try
            {
                string currentTrade = rstPrimary.Fields[IntegrationConstants.strfLOT_COMPANY_TRADE_CODE].Value.ToString();
                string tradeDescription = rstPrimary.Fields[IntegrationConstants.strfLOT_COMPANY_TRADE_DESCRIPTION].Value.ToString();

                object vntTradeId = rSys.Tables[IntegrationConstants.strtTRADE].Fields[IntegrationConstants.strfTRADE_ID].FindValue(
                    rSys.Tables[IntegrationConstants.strtTRADE].Fields[IntegrationConstants.strfTRADE_CODE],
                    currentTrade);

                if (null == vntTradeId || vntTradeId == DBNull.Value)
                {
                    //throw new PivotalApplicationException("Unable to locate the specified Trade_Code");
                    //Per Michal on 3/17/2010 we need to create this if it doesn't exist
                    vntTradeId = CreateTrade(rSys, currentTrade, tradeDescription);
                }

                //See if this trade needs to be added
                this.FindLotCompanyTrade(rSys, rstPrimary.Fields[IntegrationConstants.strfLOT__COMPANY_ID].Value,
                                                                        vntTradeId);

                //See if any other companyes are curently assiged to this trade. If so delete them.
                this.FindOtherLotCompanyTrade(rSys, rstPrimary.Fields["Lot_Id"].Value, vntTradeId, rstPrimary.Fields[IntegrationConstants.strfCOMPANY_ID].Value, currentTrade);
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will create a new location record
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="oppid"></param>
        /// <param name="nbhdp_product_id"></param>
        /// <returns></returns>
        public void InsertLocation(IRSystem7 rSys, object vntOppProdId, object quantity)
        {
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            Recordset rstLocation = new Recordset();
            try
            {
                //Field List 
                object[] arrFieldList = new object[] {IntegrationConstants.strfOPPORTUNITY_PRODUCT_ID, 
                                                          IntegrationConstants.strfLOCATION_QUANTITY};

                //Record doesn not exist so need to create it
                rstLocation = objLib.GetNewRecordset(IntegrationConstants.strtOPP_PRODUCT_LOCATION, arrFieldList);
                rstLocation.AddNew(Type.Missing, Type.Missing);
                rstLocation.Fields[IntegrationConstants.strfOPPORTUNITY_PRODUCT_ID].Value = vntOppProdId;
                rstLocation.Fields[IntegrationConstants.strfLOCATION_QUANTITY].Value = quantity;
                objLib.SaveRecordset(IntegrationConstants.strtOPP_PRODUCT_LOCATION, rstLocation);
                
                //Release recordset
                rstLocation.Close();
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will be used to assign company trades
        /// </summary>
        /// <param name="vntCompanyId"></param>
        /// <param name="tradecodes"></param>
        public void AssignCompanyTrades(IRSystem7 rSys, object vntCompanyId, string tradecodes)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                Recordset rstCoTrade = new Recordset();
                StringBuilder sqlText = new StringBuilder();

                //Lookup Trade Id using trade code
                //reject if not found
                object vntTradeId = rSys.Tables[IntegrationConstants.strtTRADE].Fields[IntegrationConstants.strfTRADE_ID].FindValue(
                    rSys.Tables[IntegrationConstants.strtTRADE].Fields[IntegrationConstants.strfTRADE_CODE],
                    tradecodes);

                if (null == vntTradeId)
                {
                    throw new PivotalApplicationException("Unable to locate the specified Trade_Code");
                }

                //Build custom sql to find lot__company record
                sqlText.Append("SELECT ");
                sqlText.Append("    Company_Trade_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("    Company_Trade ");
                sqlText.Append("WHERE ");
                sqlText.Append("    Company_Id =  " + rSys.IdToString(vntCompanyId));
                sqlText.Append("AND Trade_Id =  " + rSys.IdToString(vntTradeId));

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    //If record was found do nothing
                }
                else
                {
                    //Field List 
                    object[] arrFieldList = new object[] {IntegrationConstants.strfCOMPANY_ID, 
                                                          IntegrationConstants.strfTRADE_ID};
                    //Record doesn not exist so need to create it
                    rstCoTrade = objLib.GetNewRecordset(IntegrationConstants.strtCOMPANY_TRADE, arrFieldList);
                    rstCoTrade.AddNew(Type.Missing, Type.Missing);
                    rstCoTrade.Fields[IntegrationConstants.strfCOMPANY_ID].Value = vntCompanyId;
                    rstCoTrade.Fields[IntegrationConstants.strfTRADE_ID].Value = vntTradeId;
                    objLib.SaveRecordset(IntegrationConstants.strtCOMPANY_TRADE, rstCoTrade);
                    //Release recordset
                    rstCoTrade.Close();
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }


        /// <summary>
        /// This method will be used to assign Trade Codes for Neighorhhod Company records
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rstPrimary"></param>
        public void AssignTradeCodesForNBHDCompany(IRSystem7 rSys, Recordset rstPrimary)
        {
            try
            {
                string[] arrTradeCodes = null;

                //Split list of trade codes and store in an array
                arrTradeCodes = rstPrimary.Fields[IntegrationConstants.strfLIST_OF_TRADE_CODES]
                    .Value.ToString().Split(",".ToCharArray());

                for (int x = 0; x < arrTradeCodes.Length; x++)
                {

                    //Call method to assign company trades
                    this.AssignCompanyTrades(rSys, rstPrimary.Fields[IntegrationConstants.strfCOMPANY_ID].Value,
                        arrTradeCodes[x].ToString().Trim());
                    //Check to see if it exists
                    //or if it doesn not
                    this.FindNBHDCompanyTrade(rSys, rstPrimary.Fields[IntegrationConstants.strfNBHDP__COMPANY_ID].Value,
                                                                        arrTradeCodes[x].ToString().Trim());
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This will use custom SQL to find teh Lot Compan trde
        /// </summary>
        /// <param name="lotcompanyId"></param>
        /// <param name="tradecode"></param>
        public void FindLotCompanyTrade(IRSystem7 rSys, object lotcompanyId, object vntTradeId)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                Recordset rstLotCoTrade = new Recordset();
                StringBuilder sqlText = new StringBuilder();

                //Build custom sql to find lot__comopany record
                sqlText.Append("SELECT ");
                sqlText.Append("    Lot__Company__Trade_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("    Lot__Company__Trade ");
                sqlText.Append("WHERE ");
                sqlText.Append("    Lot_Company_Id =  " + rSys.IdToString(lotcompanyId));
                sqlText.Append(" AND Trade_Id =  " + rSys.IdToString(vntTradeId));

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    //If record was found do nothing
                }
                else
                {
                    //Field List 
                    object[] arrFieldList = new object[] {IntegrationConstants.strfLOT_COMPANY_ID, 
                                                          IntegrationConstants.strfTRADE_ID};
                    //Record does not exist so need to create it
                    rstLotCoTrade = objLib.GetNewRecordset(IntegrationConstants.strtLOT__COMPANY__TRADE, arrFieldList);
                    rstLotCoTrade.AddNew(Type.Missing, Type.Missing);
                    rstLotCoTrade.Fields[IntegrationConstants.strfLOT_COMPANY_ID].Value = lotcompanyId;
                    rstLotCoTrade.Fields[IntegrationConstants.strfTRADE_ID].Value = vntTradeId;
                    objLib.SaveRecordset(IntegrationConstants.strtLOT__COMPANY__TRADE, rstLotCoTrade);
                    //Release recordset
                    rstLotCoTrade.Close();
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This will use custom SQL to find teh Lot Compan trde
        /// </summary>
        /// <param name="lotcompanyId"></param>
        /// <param name="tradecode"></param>
        public void FindOtherLotCompanyTrade(IRSystem7 rSys, object lotId, object vntTradeId, object vntCompanyId, string tradeCode)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                string[] arrTradeCodes = null;

                //update the list of trade codes on the lot company record
                rst = objLib.GetRecordset("MI: Lot Company Trade for Trade ? but not Company ?", 3, vntTradeId, lotId, vntCompanyId, "Lot_Company_Id");
                if (rst.RecordCount > 0)
                {
                    //If records were found delete them
                    rst.MoveFirst();
                    while (rst.EOF == false && rst.BOF == false)
                    {
                        string finalTradeString = "";
                        Recordset rstLotCompany = objLib.GetRecordset(rst.Fields["Lot_Company_Id"].Value, IntegrationConstants.strtLOT__COMPANY, IntegrationConstants.strfLIST_OF_TRADE_CODES);
                        //Split list of trade codes and store in an array
                        arrTradeCodes = rstLotCompany.Fields[IntegrationConstants.strfLIST_OF_TRADE_CODES]
                            .Value.ToString().Split(",".ToCharArray());

                        for (int x = 0; x < arrTradeCodes.Length; x++)
                        {
                            if (arrTradeCodes[x] != tradeCode)
                            {
                                if (finalTradeString == "")
                                {
                                    finalTradeString = arrTradeCodes[x];
                                }
                                else
                                {
                                    finalTradeString = finalTradeString + "," + arrTradeCodes[x];
                                }
                            }
                        }
                        rstLotCompany.Fields[IntegrationConstants.strfLIST_OF_TRADE_CODES].Value = finalTradeString;
                        objLib.SaveRecordset(IntegrationConstants.strtLOT__COMPANY, rstLotCompany);
                        rstLotCompany.Close();
                        rst.MoveNext();
                    }
                    objLib.SaveRecordset(IntegrationConstants.strtLOT__COMPANY__TRADE, rst);
                }

                objLib.DeleteRecordset("MI: Lot Company Trade for Trade ? but not Company ?", "Lot__Company__Trade_Id", vntTradeId, lotId, vntCompanyId);
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method is used to lookup and create the Neighborhood Company trade associated with the
        /// Neighborhood Compoany record
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="nbhdcompanyId"></param>
        /// <param name="tradecode"></param>
        public void FindNBHDCompanyTrade(IRSystem7 rSys, object nbhdcompanyId, string tradecode)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                Recordset rstNBHDCoTrade = new Recordset();
                StringBuilder sqlText = new StringBuilder();

                //Lookup Trade Id using trade code
                //reject if not found
                object vntTradeId = rSys.Tables[IntegrationConstants.strtTRADE].Fields[IntegrationConstants.strfTRADE_ID].FindValue(
                    rSys.Tables[IntegrationConstants.strtTRADE].Fields[IntegrationConstants.strfTRADE_CODE],
                    tradecode);

                if (null == vntTradeId)
                {
                    throw new PivotalApplicationException("Unable to locate the specified Trade_Code");
                }

                //Build custom sql to find lot__comopany record
                sqlText.Append("SELECT ");
                sqlText.Append("    NBHD__Company__Trade_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("    NBHD__Company__Trade ");
                sqlText.Append("WHERE ");
                sqlText.Append("    NBHD__Company_Id = " + rSys.IdToString(nbhdcompanyId));
                sqlText.Append("AND Trade_Id =  " + rSys.IdToString(vntTradeId));

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    //If record was found do nothing
                }
                else
                {
                    //Field List 
                    object[] arrFieldList = new object[] {IntegrationConstants.strfNBHD__COMPANY_ID, 
                                                          IntegrationConstants.strfTRADE_ID};
                    //Record doesn not exist so need to create it
                    rstNBHDCoTrade = objLib.GetNewRecordset(IntegrationConstants.strtNBHD__COMPANY__TRADE, arrFieldList);
                    rstNBHDCoTrade.AddNew(Type.Missing, Type.Missing);
                    rstNBHDCoTrade.Fields[IntegrationConstants.strfLOT_COMPANY_ID].Value = nbhdcompanyId;
                    rstNBHDCoTrade.Fields[IntegrationConstants.strfTRADE_ID].Value = vntTradeId;
                    objLib.SaveRecordset(IntegrationConstants.strtNBHD__COMPANY__TRADE, rstNBHDCoTrade);
                    //Release recordset
                    rstNBHDCoTrade.Close();
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Used to lookup company by external source id
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public object FindCompany(IRSystem7 rSys, string companyId)
        {
            try
            {
                object vntResult = rSys.Tables[IntegrationConstants.strtCOMPANY].Fields[IntegrationConstants.strfCOMPANY_ID].FindValue(
                   rSys.Tables[IntegrationConstants.strtCOMPANY].Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID],
                   companyId);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }

                //Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Used to lookup lot by external source id
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="lotId"></param>
        /// <returns></returns>
        public object FindLot(IRSystem7 rSys, string lotId)
        {
            try
            {
                object vntResult = rSys.Tables[IntegrationConstants.strtPRODUCT].Fields[IntegrationConstants.strfPRODUCT_ID].FindValue(
                   rSys.Tables[IntegrationConstants.strtPRODUCT].Fields[IntegrationConstants.strfBUSINESS_UNIT_LOT_NUM],
                   lotId);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }

                //Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Used to do a lookup on Neighborhood, returning Neighborhood.Neighborhood_Id for the 
        /// record whose Neighborhood.External_Source_Community_Id = supplied External System value
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="lotId"></param>
        /// <returns></returns>
        public object FindNeighborhood(IRSystem7 rSys, string strExtSrcNeighborhoodCode)
        {
            try
            {
                object vntResult = rSys.Tables[IntegrationConstants.strtNEIGHBORHOOD].Fields[IntegrationConstants.strfNEIGHBORHOOD_ID].FindValue(
                   rSys.Tables[IntegrationConstants.strtNEIGHBORHOOD].Fields[IntegrationConstants.strfEXTERNAL_SOURCE_COMMUNITY_ID],
                   strExtSrcNeighborhoodCode);

                //If nothing is returned make sure System.DBNull.Value is returned
                if (vntResult == null)
                {
                    vntResult = DBNull.Value;
                }

                //Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Used to do a lookup on Neighborhood, returning Neighborhood.Neighborhood_Id and Neighborhood.Name for the 
        /// record whose Neighborhood.External_Source_Community_Id = supplied External System value
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="strExternalSourceNeighborhoodCode"></param>
        /// <param name="strNeighborhoodName"></param>
        /// <returns></returns>
        public object FindNeighborhood(IRSystem7 rSys, string strExtSrcNeighborhoodCode, ref string strNeighborhoodName)
        {
            try
            {
                // Initialize return variables to return null & empty by default - i.e. assume nothing found
                object vntResult = DBNull.Value;
                strNeighborhoodName = String.Empty;

                // Get Data Access
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                // Execute a Query to return the Neighborhood record with Neighborhood.External_Source_Community_Id = strExternalSourceNeighborhoodCode, 
                // returning Neighborhood_Id and Name fields
                Recordset rstNeighborhood = objLib.GetRecordset("INT - Neighborhood with External Source Community Id ?", 
                                                                1, 
                                                                strExtSrcNeighborhoodCode, 
                                                                IntegrationConstants.strfNEIGHBORHOOD_ID, IntegrationConstants.strfNAME);

                if (rstNeighborhood != null)
                {
                    if (rstNeighborhood.RecordCount > 0)
                    {
                        vntResult = rstNeighborhood.Fields[IntegrationConstants.strfNEIGHBORHOOD_ID].Value;
                        strNeighborhoodName = TypeConvert.ToString(rstNeighborhood.Fields[IntegrationConstants.strfNAME].Value);
                    }
                }

                // Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will get the associated Construction Project based on the incoming ExtSrcConstruction Project code
        /// from the sequence sheet staging table.
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="strExtSrcConstructionProjCode"></param>
        /// <param name="strConstrProjName"></param>
        /// <returns></returns>
        public object FindConstructionProject(IRSystem7 rSys, string strExtSrcConstructionProjCode, ref string strConstrProjName, 
            ref object vntNeighborhoodId)
        {
            try
            {
                // Initialize return variables to return null & empty by default - i.e. assume nothing found
                object vntResult = DBNull.Value;
                vntNeighborhoodId = DBNull.Value;
                strConstrProjName = String.Empty;

                // Get Data Access
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                // Execute a Query to return the TIC_Construction_Project record with TIC_Construction_Project.External_Source_Community_Id = strExtSrcConstructionProjCode, 
                // returning TIC_Construction_Project_Id and Name fields
                Recordset rstConstrProj = objLib.GetRecordset("INT - Constrution Project with External Source Community Id ?",
                                                                1,
                                                                strExtSrcConstructionProjCode,
                                                                IntegrationConstants.strfTIC_CONSTRUCTION_PROJECT_ID, 
                                                                IntegrationConstants.strfTIC_CONSTRUCTION_PROJECT_NAME,
                                                                IntegrationConstants.strfTIC_NEIGHBORHOOD_ID);

                if (rstConstrProj != null)
                {
                    if (rstConstrProj.RecordCount > 0)
                    {
                        vntResult = rstConstrProj.Fields[IntegrationConstants.strfTIC_CONSTRUCTION_PROJECT_ID].Value;
                        strConstrProjName = TypeConvert.ToString(rstConstrProj.Fields[IntegrationConstants.strfTIC_CONSTRUCTION_PROJECT_NAME].Value);
                        vntNeighborhoodId = rstConstrProj.Fields[IntegrationConstants.strfTIC_NEIGHBORHOOD_ID].Value;
                    }
                }

                // Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }


        /// <summary>
        /// Returns the NBHD_Phase.NBHD_Phase_Id of the NBHD_Phase record with matching 
        /// Neighborhood_Id and Phase_Name field values
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntNeighborhoodId"></param>
        /// <param name="strPhaseName"></param>
        /// <returns></returns>
        public object FindNbhdPhase(IRSystem7 rSys, object vntNeighborhoodId, string strPhaseName)
        {
            try
            {
                // Initialize return variables to return null by default - i.e. assume nothing found
                object vntResult = DBNull.Value;

                // Get Data Access
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                // Execute a Query to return the NBHD_Phase record with Neighborhood_Id = vntNeighborhoodId & 
                // Phase_Name = strPhaseName, returning NBHD_Phase_Id of first such record (there should only be one)                
                Recordset rstNbhdPhase = objLib.GetRecordset("INT - NBHD_Phase with Nbhd Id ? Phase Name ?",
                                                             2,
                                                             vntNeighborhoodId, strPhaseName,
                                                             IntegrationConstants.gstrfNBHD_PHASE_ID);

                if (rstNbhdPhase != null)
                {
                    if (rstNbhdPhase.RecordCount > 0)
                    {
                        vntResult = rstNbhdPhase.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value;                        
                    }
                }

                // Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Returns the NBHD_Phase.NBHD_Phase_Id of the NBHD_Phase record with matching 
        /// Neighborhood_Id and Phase_Name field values
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntNeighborhoodId"></param>
        /// <param name="strPhaseName"></param>
        /// <returns></returns>
        public object FindNbhdPhaseByConstructionProject(IRSystem7 rSys, object vntConstructionProjectId, string strPhaseName)
        {
            try
            {
                // Initialize return variables to return null by default - i.e. assume nothing found
                object vntResult = DBNull.Value;

                // Get Data Access
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                // Execute a Query to return the NBHD_Phase record with Neighborhood_Id = vntNeighborhoodId & 
                // Phase_Name = strPhaseName, returning NBHD_Phase_Id of first such record (there should only be one)                
                Recordset rstNbhdPhase = objLib.GetRecordset("INT - NBHD_Phase with CP Id ? Phase Name ?",
                                                             2,
                                                             vntConstructionProjectId, strPhaseName,
                                                             IntegrationConstants.gstrfNBHD_PHASE_ID);

                if (rstNbhdPhase != null)
                {
                    if (rstNbhdPhase.RecordCount > 0)
                    {
                        vntResult = rstNbhdPhase.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value;
                    }
                }

                // Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Returns the NBHDP_Product.NBHDP_Product_Id of the NBHDP_Product record with matching 
        /// Neighborhood_Id and Code_ field values, and of Type = "Plan"
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntNeighborhoodId"></param>
        /// <param name="strPlanCode"></param>
        /// <returns></returns>
        public object FindPlan(IRSystem7 rSys, object vntNeighborhoodId, string strPlanCode)
        {
            try
            {
                // Initialize return variables to return null by default - i.e. assume nothing found
                object vntResult = DBNull.Value;

                // Get Data Access
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                // Get the NBHDP_Product record with Type = "Plan" and Nbhd = ? and Code_ = ? - i.e. return Plan_Id for specified data
                Recordset rstPlan = objLib.GetRecordset("INT - Plan for Neighborhood ? Code ?",
                                                        2,
                                                        vntNeighborhoodId, strPlanCode,
                                                        IntegrationConstants.strfNBHDP_PRODUCT_ID);

                if (rstPlan != null)
                {
                    if (rstPlan.RecordCount > 0)
                    {
                        vntResult = rstPlan.Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID].Value;
                    }
                }

                // Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Returns the NBHDP_Product.NBHDP_Product_Id of the NBHDP_Product record with matching 
        /// Construction Project Id and Code_ field values, and of Type = "Plan"
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntConstructionProjectId"></param>
        /// <param name="strPlanCode"></param>
        /// <returns></returns>
        public object FindPlanByConstructionProject(IRSystem7 rSys, object vntConstructionProjectId, string strPlanCode)
        {         
            try
            {
                // Initialize return variables to return null by default - i.e. assume nothing found
                object vntResult = DBNull.Value;

                // Get Data Access
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                // Get the NBHDP_Product record with Type = "Plan" and Nbhd = ? and Code_ = ? - i.e. return Plan_Id for specified data
                Recordset rstPlan = objLib.GetRecordset("INT - Plan for CP ? Code ?",
                                                        2,
                                                        vntConstructionProjectId, strPlanCode,
                                                        IntegrationConstants.strfNBHDP_PRODUCT_ID);

                if (rstPlan != null)
                {
                    if (rstPlan.RecordCount > 0)
                    {
                        vntResult = rstPlan.Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID].Value;
                    }
                }

                // Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        
        }


        /// <summary>
        /// Returns the NBHDP_Product.NBHDP_Product_Id of the NBHDP_Product record with matching 
        /// Plan_Id and Code_ field values, and of Type = "Elevation" - i.e. returns the Elevation
        /// with the supplied Code_ and matching the supplied Plan.
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntNeighborhoodId"></param>
        /// <param name="strElevationCode"></param>
        /// <returns></returns>
        public object FindElevation(IRSystem7 rSys, object vntPlanId, string strElevationCode)
        {
            try
            {
                // Initialize return variables to return null by default - i.e. assume nothing found
                object vntResult = DBNull.Value;

                // Get Data Access
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                // Get the NBHDP_Product record with Type = "Elevation" and Plan = ? and Code_ = ? - i.e. return Elevation_Id for specified data
                Recordset rstElevation = objLib.GetRecordset("INT - Elevation for Plan ? Code ?",
                                                             2,
                                                             vntPlanId, strElevationCode,
                                                             IntegrationConstants.strfNBHDP_PRODUCT_ID);

                if (rstElevation != null)
                {
                    if (rstElevation.RecordCount > 0)
                    {
                        vntResult = rstElevation.Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID].Value;
                    }
                }

                // Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Returns the NBHD_Phase.NBHD_Phase_Id of the NBHD_Phase record with matching 
        /// Neighborhood_Id and Phase_Name field values
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntNeighborhoodId"></param>
        /// <param name="strPhaseName"></param>
        /// <returns></returns>
        public object FindDivisionProduct(IRSystem7 rSys, object vntRegionId, string strCode)
        {
            try
            {
                // Initialize return variables to return null by default - i.e. assume nothing found
                object vntResult = DBNull.Value;
                
                // Get Data Access
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                // Execute a Query to return the Division_Product record with Region_Id = vntRegionId & 
                // Code_ = strCode, returning Division_Product_Id of first such record (there should only be one)                
                Recordset rstDivisionProduct = objLib.GetRecordset("INT - Division Product for Region ? Code ?",
                                                                   2,
                                                                   vntRegionId, strCode,
                                                                   IntegrationConstants.strfDIVISION_PRODUCT_ID);

                if (rstDivisionProduct != null)
                {
                    if (rstDivisionProduct.RecordCount > 0)
                    {
                        vntResult = rstDivisionProduct.Fields[IntegrationConstants.strfDIVISION_PRODUCT_ID].Value;
                    }
                }

                // Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }


        /// <summary>
        /// AM2010.08.18
        /// Added another FindDivisionProduct method to set the Type of the NBHDP_Product when
        /// getting the Division Product
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntNeighborhoodId"></param>
        /// <param name="strPhaseName"></param>
        /// <returns></returns>
        public object FindDivisionProduct(IRSystem7 rSys, object vntRegionId, string strCode, Recordset rstPrimary)
        {
            try
            {
                // Initialize return variables to return null by default - i.e. assume nothing found
                object vntResult = DBNull.Value;

                // Get Data Access
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                // Execute a Query to return the Division_Product record with Region_Id = vntRegionId & 
                // Code_ = strCode, returning Division_Product_Id of first such record (there should only be one)                
                Recordset rstDivisionProduct = objLib.GetRecordset("INT - Division Product for Region ? Code ?",
                                                                   2,
                                                                   vntRegionId, strCode,
                                                                   IntegrationConstants.strfDIVISION_PRODUCT_ID,
                                                                   IntegrationConstants.strfTYPE);

                if (rstDivisionProduct != null)
                {
                    if (rstDivisionProduct.RecordCount > 0)
                    {
                        vntResult = rstDivisionProduct.Fields[IntegrationConstants.strfDIVISION_PRODUCT_ID].Value;
                        rstPrimary.Fields[IntegrationConstants.strfTYPE].Value = TypeConvert.ToString(rstDivisionProduct.Fields[IntegrationConstants.strfTYPE].Value);
                    }
                }

                // Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will be used to find the Region Id for reference
        /// for the integration.  This value will be lookedup on the community
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="communityId"></param>
        /// <returns></returns>
        public object FindRegion(IRSystem7 rSys, object communityId)
        {
            try
            {
                object vntResult = rSys.Tables[IntegrationConstants.strtNEIGHBORHOOD].Fields[IntegrationConstants.strfREGION_ID].FindValue(
                  rSys.Tables[IntegrationConstants.strtNEIGHBORHOOD].Fields[IntegrationConstants.strfNEIGHBORHOOD_ID],
                  communityId);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }

                //Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will be used to find the Region Id for reference
        /// for the integration.  This value will be lookedup on the community
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="communityId"></param>
        /// <returns></returns>
        public object FindRegionByDivision(IRSystem7 rSys, object divisionId)
        {
            try
            {
                object vntResult = rSys.Tables[IntegrationConstants.strtDIVISION].Fields[IntegrationConstants.strfREGION_ID].FindValue(
                  rSys.Tables[IntegrationConstants.strtDIVISION].Fields[IntegrationConstants.strfDIVISION_ID],
                  divisionId);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }

                //Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Thiss method is used to lookup the Division on External_Sourc_Id and set the Division on the
        /// Neighborhood record with the Pivotal Division Id
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="pForm"></param>
        /// <param name="rs"></param>
        public void SetDivision(IRSystem7 rSys, IRForm pForm, Recordset rs)
        {
            try
            {
                string strErrMsg = string.Empty;
                object vntDivisionId = null;
                //Make sure External Source Area Id is not null
                if (DBNull.Value == rs.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_AREA].Value)
                {
                    //throw an exception
                    strErrMsg = "External Source Area not supplied";
                    throw new PivotalApplicationException(strErrMsg);
                }

                //Lookup Division using External_source_area
                vntDivisionId = rSys.Tables[IntegrationConstants.strtDIVISION].Fields[IntegrationConstants.strfDIVISION_ID].FindValue(
                    rSys.Tables[IntegrationConstants.strtDIVISION].Fields[IntegrationConstants.strfDIVISION_NUMBER],
                    rs.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_AREA].Value);

                //If null refject
                if (null == vntDivisionId || vntDivisionId == DBNull.Value)
                {
                    strErrMsg = "The specified Division does not exist in the Pivotal System.";
                    throw new PivotalApplicationException(strErrMsg);
                }
                else
                {
                    //Set the Division Id
                    rs.Fields[IntegrationConstants.strfDIVISION_ID].Value = vntDivisionId;
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }


        /// <summary>
        /// This method is used to set the Construction Manager and the 
        /// Sales Manager on the Phase Record.  If the SM and CM are passed in
        /// then we need to set, if not copy from Neighborhood
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rs"></param>
        public void SetNeighborhoodManagersForPhase(IRSystem7 rSys, Recordset rsPhase, Recordset rsComm)
        {
            try
            {
                const string strcNEIGHBORHOOD_SALES_MANAGER_ID = "Disconnected_1_2_2";
                const string strcNEIGHBORHOOD_CONSTRUCTION_MANAGER_ID = "Disconnected_1_2_3";
                bool blnNotFound = false;
                string strCaller = string.Empty;
                string strErrMsg = string.Empty;

                //Lookup Sales Manager
                object vntEmployeeId = null;
                if (DBNull.Value != rsPhase.Fields[strcNEIGHBORHOOD_SALES_MANAGER_ID].Value)
                {
                    vntEmployeeId = rSys.Tables["Employee"].Fields["Employee_Id"].FindValue(
                        rSys.Tables["Employee"].Fields["External_Source_Id"],
                        rsPhase.Fields[strcNEIGHBORHOOD_SALES_MANAGER_ID].Value);

                    if (null == vntEmployeeId || DBNull.Value == vntEmployeeId)
                    {
                        blnNotFound = true;
                    }
                    //Check to see if Employee was found
                    if (blnNotFound)
                    {
                        //Set the Sales Manager from the Neighborhood record
                        rsPhase.Fields[IntegrationConstants.gstrfSALES_MANAGER_ID].Value
                            = rsComm.Fields[IntegrationConstants.gstrfSALES_MANAGER_ID].Value;
                    }
                    else
                    {
                        rsPhase.Fields["Sales_Manager_Id"].Value = vntEmployeeId;
                    }

                }
                else
                {
                    //Set the Sales Manager from the Neighborhood record
                    rsPhase.Fields[IntegrationConstants.gstrfSALES_MANAGER_ID].Value
                        = rsComm.Fields[IntegrationConstants.gstrfSALES_MANAGER_ID].Value;

                }

                //Lookup Construction Manager
                vntEmployeeId = null;
                if (DBNull.Value != rsPhase.Fields[strcNEIGHBORHOOD_CONSTRUCTION_MANAGER_ID].Value)
                {
                    vntEmployeeId = rSys.Tables["Employee"].Fields["Employee_Id"].FindValue(
                        rSys.Tables["Employee"].Fields["External_Source_Id"],
                        rsPhase.Fields[strcNEIGHBORHOOD_CONSTRUCTION_MANAGER_ID].Value);

                    if (null == vntEmployeeId || DBNull.Value == vntEmployeeId)
                    {
                        blnNotFound = true;
                    }
                    //Check to see if Employee was found
                    if (blnNotFound)
                    {
                        //Set the Construction Manager from the Neighborhood record
                        rsPhase.Fields[IntegrationConstants.gstrfCONSTRUCTION_MANAGER_ID].Value
                            = rsComm.Fields[IntegrationConstants.gstrfCONSTRUCTION_MANAGER_ID].Value;

                    }
                    else
                    {
                        rsPhase.Fields["Construction_Manager_Id"].Value = vntEmployeeId;
                    }
                }
                else
                {
                    //Set the Construction Manager from the Neighborhood record
                    rsPhase.Fields[IntegrationConstants.gstrfCONSTRUCTION_MANAGER_ID].Value
                        = rsComm.Fields[IntegrationConstants.gstrfCONSTRUCTION_MANAGER_ID].Value;

                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method is used to set the Construction Manager and the 
        /// Sales Manager on the Neighborhood Record
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rs"></param>
        public void SetNeighborhoodManagers(IRSystem7 rSys, Recordset rs)
        {
            try
            {
                const string strcNEIGHBORHOOD_SALES_MANAGER_ID = "Disconnected_1_2_2";
                const string strcNEIGHBORHOOD_CONSTRUCTION_MANAGER_ID = "Disconnected_1_2_3";
                bool blnNotFound = false;
                string strCaller = string.Empty;
                string strErrMsg = string.Empty;

                object vntNeighborhoodId = rs.Fields[IntegrationConstants.strfNEIGHBORHOOD_ID].Value;

                if (null == vntNeighborhoodId || DBNull.Value == vntNeighborhoodId)
                {
                    strCaller = "Insert";
                }
                else
                {
                    strCaller = "Update";
                }

                //Lookup Sales Manager
                object vntEmployeeId = null;
                if (DBNull.Value != rs.Fields[strcNEIGHBORHOOD_SALES_MANAGER_ID].Value)
                {
                    vntEmployeeId = rSys.Tables["Employee"].Fields["Employee_Id"].FindValue(
                        rSys.Tables["Employee"].Fields["External_Source_Id"],
                        rs.Fields[strcNEIGHBORHOOD_SALES_MANAGER_ID].Value);

                    if (null == vntEmployeeId || DBNull.Value == vntEmployeeId)
                    {
                        blnNotFound = true;
                    }
                    //Check to see if Employee was found
                    if (blnNotFound)
                    {
                        strErrMsg = "Unable to locate Sales manager Record using : " +
                            rs.Fields[strcNEIGHBORHOOD_SALES_MANAGER_ID].Value;
                        throw new PivotalApplicationException(strErrMsg);
                    }
                    else
                    {
                        rs.Fields["Sales_Manager_Id"].Value = vntEmployeeId;
                    }
                }
                else
                {
                    throw new PivotalApplicationException("No Sales Manager Id was passed in from JDE.");
                }

                //Lookup Construction Manager
                vntEmployeeId = null;
                if (DBNull.Value != rs.Fields[strcNEIGHBORHOOD_CONSTRUCTION_MANAGER_ID].Value)
                {
                    vntEmployeeId = rSys.Tables["Employee"].Fields["Employee_Id"].FindValue(
                        rSys.Tables["Employee"].Fields["External_Source_Id"],
                        rs.Fields[strcNEIGHBORHOOD_CONSTRUCTION_MANAGER_ID].Value);

                    if (null == vntEmployeeId || vntEmployeeId == DBNull.Value)
                    {
                        blnNotFound = true;
                    }
                    //Check to see if Employee was found
                    if (blnNotFound)
                    {
                        strErrMsg = "Unable to locate Construction Manager Record using : " +
                            rs.Fields[strcNEIGHBORHOOD_CONSTRUCTION_MANAGER_ID].Value;
                        throw new PivotalApplicationException(strErrMsg);
                    }
                    else
                    {
                        rs.Fields["Construction_Manager_Id"].Value = vntEmployeeId;
                    }
                }
                else
                {
                    rs.Fields["Construction_Manager_Id"].Value = DBNull.Value;
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        

        /// <summary>
        /// This method is used in the Phase integration to copy over division
        /// adjustments.
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rs"></param>
        public void CopyDivisionAdjustments(IRSystem7 rSys, Recordset rs)
        {
            try
            {
                object vntDivisionId = null;
                object vntCommunityId = rs.Fields[IntegrationConstants.gstrfNEIGHBORHOOD_ID].Value;
                object vntPhaseId = rs.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value;

                //If Community if not null get Division id
                if (DBNull.Value != vntCommunityId)
                {
                    //Lookup DivisionId
                    vntDivisionId = rSys.Tables[IntegrationConstants.gstrtNEIGHBORHOOD].Fields[IntegrationConstants.gstrfDIVISION_ID]
                    .Index(vntCommunityId);
                }

                //Check to see if phase is null
                if (DBNull.Value != vntPhaseId)
                {
                    //Check DivisionId
                    if (DBNull.Value != vntDivisionId)
                    {
                        object[] vntAllTargetFieldnames = new object[] 
                        {
                            IntegrationConstants.gstrfDIVISION_ADJUSTMENT_ID, 
                            IntegrationConstants.gstrfDIVISION_ID,
                            IntegrationConstants.gstrfADJUSTMENT_TYPE,
                            IntegrationConstants.gstrfADJUSTMENT_REASON,
                            IntegrationConstants.gstrfINACTIVE,
                            IntegrationConstants.gstrfRELEASE_ID

                        };

                        object[] vntCopySourceFieldNames = new object[] 
                        {
                            IntegrationConstants.gstrfDIVISION_ADJUSTMENT_ID, 
                            IntegrationConstants.gstrfDIVISION_ID,
                            IntegrationConstants.gstrfADJUSTMENT_TYPE,
                            IntegrationConstants.gstrfADJUSTMENT_REASON,
                            IntegrationConstants.gstrfINACTIVE
                        };

                        object[] vntCopyTargetFieldNames = new object[] 
                        {
                            IntegrationConstants.gstrfDIVISION_ADJUSTMENT_ID, 
                            IntegrationConstants.gstrfDIVISION_ID,
                            IntegrationConstants.gstrfADJUSTMENT_TYPE,
                            IntegrationConstants.gstrfADJUSTMENT_REASON,
                            IntegrationConstants.gstrfINACTIVE
                        };

                        //Use DataSet object
                        IRDataset4 rdstDataset = (IRDataset4)rSys.CreateDataset();

                        //Call Return lookup method
                        Recordset rstDivAdj = this.ReturnLookUp(rSys, rdstDataset, IntegrationConstants.gstrtDIVISION_ADJUSTMENT,
                            vntCopySourceFieldNames, IntegrationConstants.gstrqDIVISION_ADJUST_BY_DIVISION_ID,
                            true, new object[] { vntDivisionId });

                        //Make sure recordset is not null
                        if (null != rstDivAdj)
                        {
                            //If records were returned
                            if (rstDivAdj.RecordCount > 0)
                            {
                                rstDivAdj.MoveFirst();

                                //Do lookup for rlease adjustment
                                Recordset rstReleaseAdjust = this.ReturnLookUp(rSys, rdstDataset,
                                    IntegrationConstants.gstrtRELEASE_ADJUSTMENT,
                                    vntAllTargetFieldnames, "", true, null);

                                while (!(rstDivAdj.EOF))
                                {

                                    rstReleaseAdjust.AddNew(Type.Missing, Type.Missing);

                                    for (int k = 0; k < vntCopySourceFieldNames.Length; k++)
                                    {
                                        rstReleaseAdjust.Fields[vntCopyTargetFieldNames[k]].Value =
                                            rstDivAdj.Fields[vntCopySourceFieldNames[k]].Value;
                                    }
                                    //Set Phase Id
                                    rstReleaseAdjust.Fields[IntegrationConstants.gstrfRELEASE_ID].Value = vntPhaseId;
                                    //Get Next Adjst ment to copy
                                    rstDivAdj.MoveNext();
                                }

                                //Committ the record to the database.
                                rdstDataset.SaveRecordset(rstReleaseAdjust);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will be used to update the Lot Contract from teh Lot Integration touchpoint
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntContractId"></param>
        /// <param name="rstPrimary"></param>
        public void UpdateLotContract(IRSystem7 rSys, object vntContractId, Recordset rstPrimary)
        {
            try
            {
                string strErrMsg = string.Empty;
                Recordset rstContract = null;
                object[] arrFields = null;
                string strPlanCode = string.Empty;
                string strNeighborhood_Id = string.Empty;
                string strNBHD_Phase_Id = string.Empty;
                string strNBHD_Phase_Code = string.Empty;
                string strPlanId = string.Empty;
                string strElevId = string.Empty;

                //Get Utility Class to do lookup
                IntegrationUtility util = new IntegrationUtility();

                //Use this object to get new recordset
                DataAccess objLib = (DataAccess)
                   rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                //Update fields on Contract
                arrFields = new object[] 
                {
                    IntegrationConstants.strfECOE_DATE, IntegrationConstants.strfACTUAL_REVENUE_DATE, IntegrationConstants.strfSCHED_BUYER_WALKTHROUGH_DATE,
                    IntegrationConstants.strfACTUAL_BUYER_WALKTHROUGH_DATE, IntegrationConstants.strfSTATUS, IntegrationConstants.strfEXTERNAL_SOURCE_SYNC_STATUS,
                    IntegrationConstants.strfNEIGHBORHOOD_ID, IntegrationConstants.gstrfNBHD_PHASE_ID, IntegrationConstants.strfLOT_ID,
                    IntegrationConstants.strfPLAN_NAME_ID, IntegrationConstants.strfLOT_PREMIUM, IntegrationConstants.strfFINANCED_OPTIONS,
                    IntegrationConstants.strfQUOTE_OPTION_TOTAL, IntegrationConstants.strfEXTERNAL_SOURCE_NAME, 
                    IntegrationConstants.gstrfINACTIVE, IntegrationConstants.strfPIPELINE_STAGE, IntegrationConstants.strfSTATUS,
                    IntegrationConstants.strfQUOTE_CREATE_DATE, IntegrationConstants.strfCONFIGURATION_COMPLETE, IntegrationConstants.strfELEVATION_PREMIUM,
                    IntegrationConstants.strfPLAN_BUILT, IntegrationConstants.strfACTUAL_DECISION_DATE, IntegrationConstants.strfCONTRACT_APPROVED_SUBMITTED,
                    IntegrationConstants.strfCANCEL_DATE, IntegrationConstants.strfCANCEL_REQUEST_DATE, IntegrationConstants.strfCANCEL_REASON_ID,
                    IntegrationConstants.strfCANCEL_NOTES, IntegrationConstants.strfLOAN_APPROVAL_DATE, IntegrationConstants.strfCONCESSIONS,
                    IntegrationConstants.strfEXTERNAL_SOURCE_NAME, IntegrationConstants.strfQUOTE_CREATE_DATE, 
                    IntegrationConstants.strfCONFIGURATION_COMPLETE, IntegrationConstants.strfCONTACT_ID, IntegrationConstants.strfEXTERNAL_SOURCE_SYNC_STATUS,
                    IntegrationConstants.strfPRICE, IntegrationConstants.strfCONTRACT_APPROVED_DATETIME, 
                    IntegrationConstants.strfMI_CONTRACTAPPROVALDATE

                };

                //Insert or Update
                if (DBNull.Value.Equals(vntContractId))
                {
                    //insert 
                    rstContract = objLib.GetNewRecordset(IntegrationConstants.strtOPPORTUNITY, arrFields);

                    //Force framework to create new record
                    rstContract.AddNew(Type.Missing, Type.Missing);

                    //Fields to be set only on insert
                    rstContract.Fields[IntegrationConstants.strfNEIGHBORHOOD_ID].Value = rstPrimary.Fields[IntegrationConstants.strfNEIGHBORHOOD_ID].Value;
                    rstContract.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value = rstPrimary.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value;
                    rstContract.Fields[IntegrationConstants.strfLOT_ID].Value = rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value;
                    rstContract.Fields[IntegrationConstants.strfLOT_PREMIUM].Value = rstPrimary.Fields[IntegrationConstants.strfPRICE].Value;
                    rstContract.Fields[IntegrationConstants.strfCONTACT_ID].Value = rstPrimary.Fields[IntegrationConstants.strfOWNER_ID].Value;

                    //must requery plan because it might not be on the lot
                    //Build Plan External Source Id
                    //1. Set Neighborhood String
                    strPlanId = util.BuildPlanCode(rstPrimary.Fields[IntegrationConstants.strfLOT_COMMUNITYID].Value, rstPrimary.Fields[IntegrationConstants.strfLOT_PHASEID].Value,
                                        rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_PLAN_ID].Value, rstPrimary.Fields[IntegrationConstants.gstrfEXT_SOURCE_ELEV_CODE].Value);
                    
                    // ASM 2010-07-28: Commented-out, as my implementation of FindPlan had changed.  May need to fix this later if we need this call to FindPlan.
                    object vntPlanId = null;
                    //object vntPlanId = util.FindPlan(rSys, strPlanId, true);

                    rstContract.Fields[IntegrationConstants.strfPLAN_NAME_ID].Value = vntPlanId;
                    rstContract.Fields[IntegrationConstants.strfACTUAL_DECISION_DATE].Value = rstPrimary.Fields[IntegrationConstants.strfSALE_DATE].Value;
                    rstContract.Fields[IntegrationConstants.strfCONTRACT_APPROVED_SUBMITTED].Value = rstPrimary.Fields[IntegrationConstants.strfSALES_REQUEST_DATE].Value;
                    rstContract.Fields[IntegrationConstants.strfCONTRACT_APPROVED_DATETIME].Value = rstPrimary.Fields[IntegrationConstants.strfSALES_REQUEST_DATE].Value;
                    rstContract.Fields[IntegrationConstants.strfCANCEL_DATE].Value = rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_CANCEL_DATE].Value;

                    //assign the sales team
                    //Object vntSalesTeam = FindSalesTeam(rSys, rstPrimary.Fields[IntegrationConstants.strfLOT_PRIMARYSALESREP].Value.ToString(), rstPrimary.Fields[IntegrationConstants.strfCONTACT_COMMUNITY].Value.ToString(), "");
                    //rstContract.Fields[IntegrationConstants.strfMI_SALES_TEAM_ID].Value = vntSalesTeam;

                    //2007-12-12 AB Throw error is lot is sold or closed but no sales request date exists
                    //
                    if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfSALE_DATE].Value &&
                        DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfSALES_REQUEST_DATE].Value)
                    {
                        throw new PivotalApplicationException("Cannot insert a contract with a sale date if the sale request date is not set.");
                    }

                    //2007-12-13 AB Cancel logic is not valid for inserts. Canceled contracts will not be created through the integration for MI
                    //Cancel Request Date
                    /*if (DBNull.Value != rstContract.Fields[IntegrationConstants.strfCANCEL_DATE].Value)
                    {
                        //rstContract.Fields[IntegrationConstants.strfCANCEL_REQUEST_DATE].Value = rstPrimary.Fields[IntegrationConstants.strfCANCEL_DATE].Value;
                    }
                    else if (DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfSALE_DATE].Value &&
                        DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfRESERVATION_DATE].Value &&
                        DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfCLOSED_DATE].Value)
                    {
                        rstContract.Fields[IntegrationConstants.strfCANCEL_REQUEST_DATE].Value = DateTime.Now;
                    }
                    else
                    {
                        rstContract.Fields[IntegrationConstants.strfCANCEL_REQUEST_DATE].Value = DBNull.Value;
                    }

                    //Cancel Reason Id
                    if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_CANCEL_REASON].Value)
                    {
                        //Do lookup on Cancel Reason
                        rstContract.Fields[IntegrationConstants.strfCANCEL_REASON_ID].Value
                            = this.FindCancelReason(rSys, rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_CANCEL_REASON].Value.ToString());

                    }
                    else if (DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfSALE_DATE].Value &&
                        DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfRESERVATION_DATE].Value &&
                        DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfCLOSED_DATE].Value)
                    {
                        //Use the "" switch so that this method will lookup the default Cancel_ReasonId
                        rstContract.Fields[IntegrationConstants.strfCANCEL_REASON_ID].Value
                            = this.FindCancelReason(rSys, "");
                    }
                    else
                    {
                        rstContract.Fields[IntegrationConstants.strfCANCEL_REASON_ID].Value = DBNull.Value;
                    }


                    //Cancel Note
                    if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_CANCEL_NOTE].Value)
                    {
                        //Do lookup on Cancel Reason
                        rstContract.Fields[IntegrationConstants.strfCANCEL_NOTES].Value
                            = rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_CANCEL_NOTE].Value;
                    }
                    else if (DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfSALE_DATE].Value &&
                       DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfRESERVATION_DATE].Value &&
                       DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfCLOSED_DATE].Value)
                    {
                        //Use the "" switch so that this method will lookup the default Cancel_ReasonId
                        rstContract.Fields[IntegrationConstants.strfCANCEL_NOTES].Value
                            = "Cancelled by E1 Through Integration";
                    }
                    else
                    {
                        rstContract.Fields[IntegrationConstants.strfCANCEL_NOTES].Value = DBNull.Value;
                    }
                    */
                    //Loan Approval Date
                    rstContract.Fields[IntegrationConstants.strfLOAN_APPROVAL_DATE].Value
                        = rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_CREDIT_APRV_DT].Value;

                    //Concessions
                    double lotbase = 0;
                    double lotmortgage = 0;
                    double lotoption = 0;

                    //Check for nulls and set values
                    if (DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfLOT_BASEINCENTIVE].Value)
                    { lotbase = 0.0; }
                    else { lotbase = Convert.ToDouble(rstPrimary.Fields[IntegrationConstants.strfLOT_BASEINCENTIVE].Value); }
                    if (DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfLOT_MORTGAGEINCENTIVE].Value)
                    { lotmortgage = 0.0; }
                    else { lotmortgage = Convert.ToDouble(rstPrimary.Fields[IntegrationConstants.strfLOT_MORTGAGEINCENTIVE].Value); }
                    if (DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfLOT_OPTIONINCENTIVE].Value)
                    { lotoption = 0.0; }
                    else { lotoption = Convert.ToDouble(rstPrimary.Fields[IntegrationConstants.strfLOT_OPTIONINCENTIVE].Value); }

                    //Calculate concessions
                    rstContract.Fields[IntegrationConstants.strfCONCESSIONS].Value = lotbase + lotmortgage + lotoption;

                    //Financed Options
                    rstContract.Fields[IntegrationConstants.strfFINANCED_OPTIONS].Value
                        = rstPrimary.Fields[IntegrationConstants.strfLOT_OPTIONTOTAL].Value;

                    //Option Total
                    rstContract.Fields[IntegrationConstants.strfQUOTE_OPTION_TOTAL].Value
                        = rstPrimary.Fields[IntegrationConstants.strfLOT_OPTIONTOTAL].Value;

                    //External_Source Name
                    rstContract.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_NAME].Value
                        = rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_NAME].Value;

                    //Inactive
                    rstContract.Fields[IntegrationConstants.gstrfINACTIVE].Value = 0;

                    //Set Lot Premium to Contract Lot Premium
                    if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfPRICE].Value)
                    {
                        rstContract.Fields[IntegrationConstants.strfLOT_PREMIUM].Value
                            = rstPrimary.Fields[IntegrationConstants.strfPRICE].Value;
                    }
                    else
                    {
                        rstContract.Fields[IntegrationConstants.strfLOT_PREMIUM].Value = 0;
                    }

                    //Pipeline stage
                    //Removed this logic because canceled contracts are not create in Pivotal based on E1 data
                    //if (DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfSALE_DATE].Value &&
                    //   DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfRESERVATION_DATE].Value &&
                    //   DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfCLOSED_DATE].Value)
                    //{
                    //    rstContract.Fields[IntegrationConstants.strfPIPELINE_STAGE].Value
                    //        = "Cancelled";
                    //}
                    //else if (DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfSALE_DATE].Value &&
                    //DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfCLOSED_DATE].Value)
                    //if (DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfSALE_DATE].Value &&
                    //    DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfCLOSED_DATE].Value)
                    //{
                    //    rstContract.Fields[IntegrationConstants.strfPIPELINE_STAGE].Value
                    //        = "Quote";
                    //}
                    //else
                    //{
                    rstContract.Fields[IntegrationConstants.strfPIPELINE_STAGE].Value
                        = "Contract";
                    //}

                    //Status
                    //AB 09-29-08 logic changed to use status and not dates
                    //if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfCLOSED_DATE].Value)
                    //{
                    //    rstContract.Fields[IntegrationConstants.strfSTATUS].Value
                    //        = "Closed";
                    //}
                    //else if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfSALE_DATE].Value)
                    //{
                    //    rstContract.Fields[IntegrationConstants.strfSTATUS].Value
                    //        = "In Progress";
                    //}
                    //else if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfRESERVATION_DATE].Value)
                    //{
                    //    rstContract.Fields[IntegrationConstants.strfSTATUS].Value
                    //        = "Reserved";
                    //}
                    //else
                    //{
                    //    rstContract.Fields[IntegrationConstants.strfSTATUS].Value
                    //        = "Cancelled";
                    //}

                    if (rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_SALES_STATUS].Value.ToString() == "CLS")
                    {
                        rstContract.Fields[IntegrationConstants.strfSTATUS].Value = "Closed";
                    }
                    else if (rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_SALES_STATUS].Value.ToString() == "SAL")
                    {
                        rstContract.Fields[IntegrationConstants.strfSTATUS].Value = "In Progress";
                    }
                    else
                    {
                        rstContract.Fields[IntegrationConstants.strfSTATUS].Value = "Reserved";
                    }

                    //Quote Create Date
                    rstContract.Fields[IntegrationConstants.strfQUOTE_CREATE_DATE].Value = DateTime.Now;

                    //Configuration Complete
                    rstContract.Fields[IntegrationConstants.strfCONFIGURATION_COMPLETE].Value = true;

                    //Elevation Premium
                    rstContract.Fields[IntegrationConstants.strfELEVATION_PREMIUM].Value = 0;

                    //Sales Request date
                    if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfSALES_REQUEST_DATE].Value)
                    {
                        rstContract.Fields[IntegrationConstants.strfCONTRACT_APPROVED_SUBMITTED].Value
                            = rstPrimary.Fields[IntegrationConstants.strfSALES_REQUEST_DATE].Value;

                    }

                    //2007-12-11 Set base price based on JDE data on insert
                    rstContract.Fields[IntegrationConstants.strfPRICE].Value = rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_BASE_PRICE].Value;
                }
                else
                {
                    //Update Contract
                    //Check for cancellation updates - not allowed
                    if (DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfSALE_DATE].Value &&
                           DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfRESERVATION_DATE].Value &&
                           DBNull.Value == rstPrimary.Fields[IntegrationConstants.strfCLOSED_DATE].Value)
                    {
                        strErrMsg = "Reject Record - Cancellations are not permitted in E1.";
                        throw new PivotalApplicationException(strErrMsg);
                    }

                    //Get record and set point to allow fields to be updated
                    rstContract = objLib.GetRecordset(vntContractId, IntegrationConstants.strtOPPORTUNITY, arrFields);

                    //Set record cursor
                    rstContract.MoveFirst();
                }

                //Common Fields
                //AM 2007.07 - will not update ECOE date via integration

                //rstContract.Fields[IntegrationConstants.strfECOE_DATE].Value
                //    = rstPrimary.Fields[IntegrationConstants.strfEST_CONTRACT_CLOSED_DATE].Value;
                //rstContract.Fields[IntegrationConstants.strfLOT_PREMIUM].Value
                //    = rstPrimary.Fields[IntegrationConstants.strfPRICE].Value;
                //rstContract.Fields[IntegrationConstants.strfCONTACT_ID].Value
                //    = rstPrimary.Fields[IntegrationConstants.strfOWNER_ID].Value;
                rstContract.Fields[IntegrationConstants.strfECOE_DATE].Value
                    = rstPrimary.Fields[IntegrationConstants.strfEST_CONTRACT_CLOSED_DATE].Value;
                rstContract.Fields[IntegrationConstants.strfACTUAL_REVENUE_DATE].Value
                    = rstPrimary.Fields[IntegrationConstants.strfCLOSED_DATE].Value;
                rstContract.Fields[IntegrationConstants.strfMI_CONTRACTAPPROVALDATE].Value
                    = rstPrimary.Fields[IntegrationConstants.strfJDE_CONTRACTAPPROVALDATE].Value;
                rstContract.Fields[IntegrationConstants.strfSCHED_BUYER_WALKTHROUGH_DATE].Value
                    = rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_SCHED_BUYER_WT].Value;
                rstContract.Fields[IntegrationConstants.strfACTUAL_BUYER_WALKTHROUGH_DATE].Value
                    = rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ACTUAL_WALKTHR].Value;

                //Plan Price 
                object vntPlanPrice = null;
                decimal planPrice = 0;

                //Get Plan Price and store as object first
                vntPlanPrice = rSys.Tables[IntegrationConstants.strtNBHDP_PRODUCT].Fields[IntegrationConstants.strfCURRENT_PRICE]
                    .FindValue(rSys.Tables[IntegrationConstants.strtNBHDP_PRODUCT].Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID],
                    rstPrimary.Fields[IntegrationConstants.strfPLAN_ID].Value);

                //Need to evaluate for nulls before converting to decimal
                if (DBNull.Value.Equals(vntPlanPrice))
                {
                    planPrice = 0;
                }
                else
                {
                    planPrice = Convert.ToDecimal(vntPlanPrice);
                }

                //Plan Price is only set on insert and must be the price set in JDE
                //rstContract.Fields[IntegrationConstants.strfPRICE].Value = planPrice;

                //Buyer
                //rstContract.Fields[IntegrationConstants.strfCONTACT_ID].Value
                //    = rstPrimary.Fields[IntegrationConstants.strfOWNER_ID].Value;

                //Check Closed
                if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfCLOSED_DATE].Value)
                {
                    rstContract.Fields[IntegrationConstants.strfSTATUS].Value = "Closed";
                    rstContract.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_SYNC_STATUS].Value = "Closed Sync'd";

                    //Ensure buyers and cobuyers are in Lot__Contact table on Close
                    this.AssignCoBuyers(rSys, rstContract.Fields[IntegrationConstants.strfCONTACT_ID].Value, rstContract.Fields[IntegrationConstants.strfLOT_ID].Value);

                }
                //2007-12-12 AB Removed because contracts will not be canceled through the integration
                //else
                //{
                //rstContract.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_SYNC_STATUS].Value = "Cancel Sync'd";
                //}

                objLib.SaveRecordset(IntegrationConstants.strtOPPORTUNITY, rstContract);

                //Find and Deactivate any Inventory Quote
                if (rstContract.Fields[IntegrationConstants.strfPIPELINE_STAGE].Value.ToString() == "Contract" &&
                    rstContract.Fields[IntegrationConstants.strfSTATUS].Value.ToString() == "In Progress")
                {
                    this.InactiveInventoryQuotes(rSys, rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value);
                }
                //Clean up
                rstContract.Close();
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }


        /// <summary>
        /// This method is used in teh Lot Touchpoint to assign cobuyesr on the contract
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="ownerId"></param>
        /// <param name="lotId"></param>
        public void AssignCoBuyers(IRSystem7 rSys, object ownerId, object lotId)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                Recordset rstCoBuyers = new Recordset();
                StringBuilder sqlText = new StringBuilder();

                //Build query to find only those CoBuyer contacts that are not currently in the
                //Lot__Contact table for this lot
                sqlText.Append("SELECT ");
                sqlText.Append("    c.Contact_Id, 4 as [Type], 1 as [Primary_Contact] ");
                sqlText.Append("FROM ");
                sqlText.Append("    Contact c ");
                sqlText.Append("    LEFT JOIN Lot__Contact lc ON ( lc.Contact_Id = c.Contact_Id AND lc.Product_Id = " + rSys.IdToString(lotId) + ")");
                sqlText.Append(" WHERE ");
                sqlText.Append("    c.Contact_Id = " + rSys.IdToString(ownerId));
                sqlText.Append(" AND lc.Contact_Id IS NULL ");
                sqlText.Append("UNION ALL ");
                sqlText.Append("SELECT ");
                sqlText.Append("    c.Contact_Id, 4 as [Type], 0 as [Primary_Contact] ");
                sqlText.Append("FROM ");
                sqlText.Append("    Contact c ");
                sqlText.Append("    LEFT JOIN Contact_CoBuyer ccb ON ccb.Co_buyer_contact_id = c.Contact_ID ");
                sqlText.Append("    LEFT JOIN Lot__Contact lc ON ( lc.Contact_Id = c.Contact_Id AND lc.Product_Id = " + rSys.IdToString(lotId) + ")");
                sqlText.Append("WHERE ");
                sqlText.Append("    ccb.Contact_Id = " + rSys.IdToString(ownerId));
                sqlText.Append(" AND lc.Contact_Id IS NULL ");

                rst = objLib.GetRecordset(sqlText.ToString());

                object[] arrFields = new object[] 
                        {
                            IntegrationConstants.strfCONTACT_ID,
                            IntegrationConstants.gstrfTYPE, 
                            IntegrationConstants.strfPRIMARY_CONTACT,
                            IntegrationConstants.strfPRODUCT_ID
                        };

                if (rst.RecordCount > 0)
                {
                    rst.MoveFirst();
                    //Create new Lot contact
                    rstCoBuyers = objLib.GetNewRecordset(IntegrationConstants.strtLOT__CONTACT,
                        arrFields);

                    while (!(rst.EOF))
                    {

                        rstCoBuyers.AddNew(Type.Missing, Type.Missing);

                        rstCoBuyers.Fields[IntegrationConstants.strfCONTACT_ID].Value
                           = rst.Fields[IntegrationConstants.strfCONTACT_ID].Value;
                        rstCoBuyers.Fields[IntegrationConstants.gstrfTYPE].Value
                          = rst.Fields[IntegrationConstants.gstrfTYPE].Value;
                        rstCoBuyers.Fields[IntegrationConstants.strfPRIMARY_CONTACT].Value
                           = rst.Fields[IntegrationConstants.strfPRIMARY_CONTACT].Value;
                        rstCoBuyers.Fields[IntegrationConstants.strfPRODUCT_ID].Value = lotId;
                        rst.MoveNext();
                    }

                    //Save recordset after building
                    objLib.SaveRecordset(IntegrationConstants.strtLOT__CONTACT, rstCoBuyers);

                    //CLean up
                    rst.Close();
                    rstCoBuyers.Close();
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will be used to assign sales reps to lots
        /// </summary>
        /// <param name="vntLotId"></param>
        /// <param name="salesrep1"></param>
        /// <param name="salesrep2"></param>
        public void AssignSalesReps(IRSystem7 rSys, object vntLotId, string salesrep1, string salesrep2)
        {
            try
            {
                string strErrMsg = string.Empty;
                Recordset rstContract = null;
                Recordset rstContractTeamMember = null;
                object vntEmployeeId = null;
                bool blnAddTeamMember = true;
                object vntContractId = null;

                //Use Data Access 
                DataAccess objLib =
                    (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                    .CreateInstance();

                vntContractId = this.FindContractByLot(rSys, vntLotId);

                //If no contract is found, but SalesRep1 as supplied, then error
                if (null == vntContractId && salesrep1.Length > 0)
                {
                    //Reject if not found - throw an exceptions
                    strErrMsg = "Reject Record - Primary Sales Agent supplied" +
                        " associated Contract Record was not found.";
                    throw new PivotalApplicationException(strErrMsg);

                }

                //Just exit if Contract not found
                if (DBNull.Value.Equals(vntContractId))
                {
                    return;
                }

                if (salesrep1.Length > 0)
                {
                    //Lookup employeid and store as Account Manaer Id
                    vntEmployeeId = rSys.Tables[IntegrationConstants.strtEMPLOYEE].Fields[IntegrationConstants.strfEMPLOYEE_ID]
                    .FindValue(rSys.Tables[IntegrationConstants.strtEMPLOYEE].Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID],
                    salesrep1);

                    if (DBNull.Value.Equals(vntEmployeeId))
                    {
                        //Removed code because sales rep might not be available for old contracts
                        //strErrMsg = "Reject Record - " +
                        //" Associated Primary Sales Agent not found in Pivotal.";

                        //throw new PivotalApplicationException(strErrMsg);
                    }
                    else
                    {
                        //Load the Contract record based on the contract Id
                        rstContract = objLib.GetRecordset(vntContractId, IntegrationConstants.strtOPPORTUNITY,
                            new object[] { IntegrationConstants.strfACCOUNT_MANAGER_ID });
                        if (rstContract.RecordCount > 0)
                        {
                            //Only update if different
                            if (!rSys.EqualIds(rstContract.Fields[IntegrationConstants.strfACCOUNT_MANAGER_ID].Value,
                                vntEmployeeId))
                            {
                                //Update the field
                                rstContract.Fields[IntegrationConstants.strfACCOUNT_MANAGER_ID].Value = vntEmployeeId;
                            }

                            //Save to DB
                            objLib.SaveRecordset(IntegrationConstants.strtOPPORTUNITY, rstContract);
                        }
                        else
                        {
                            //Contract not found
                            //SHould not happen we just looked it up
                        }
                    }
                }

                //Lookup up the Secondar sales rep
                if (salesrep2.Length > 0)
                {
                    //Lookup employee Id and store as Opportunity Team Member
                    //Lookup employeid and store as Account Manaer Id
                    vntEmployeeId = rSys.Tables[IntegrationConstants.strtEMPLOYEE].Fields[IntegrationConstants.strfEMPLOYEE_ID]
                    .FindValue(rSys.Tables[IntegrationConstants.strtEMPLOYEE].Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID],
                    salesrep2);

                    if (!DBNull.Value.Equals(vntEmployeeId))
                    {
                        //Add the Team Member REcord
                        rstContractTeamMember = objLib.GetLinkedRecordset(IntegrationConstants.strtOPPORTUNITY_TEAM_MEMBER,
                            IntegrationConstants.strfOPPORTUNITY_ID, vntContractId,
                            new object[] 
                            {
                                IntegrationConstants.strfEMPLOYEE_ID,
                                IntegrationConstants.strfOPPORTUNITY_ID,
                                IntegrationConstants.strfROLE_ID,
                                IntegrationConstants.gstrfINACTIVE
                            });

                        if (rstContractTeamMember.RecordCount > 0)
                        {
                            //See if team member record is already here
                            while (!rstContractTeamMember.EOF)
                            {
                                if (rSys.EqualIds(rstContractTeamMember.Fields[IntegrationConstants.strfEMPLOYEE_ID].Value,
                                    vntEmployeeId))
                                {
                                    //Already there...
                                    blnAddTeamMember = false;
                                    break;
                                }

                                rstContractTeamMember.MoveNext();
                            }
                        }
                        else
                        {
                            //Not found, add it - flag is already set to true
                        }

                        //Reset recordset position
                        //rstCOntractTeamMember.MoveFirst();
                        //Do we need to add the Team Member Record

                        if (blnAddTeamMember)
                        {
                            //Add it
                            rstContractTeamMember.AddNew(Type.Missing, Type.Missing);

                            rstContractTeamMember.Fields[IntegrationConstants.strfOPPORTUNITY_ID].Value = vntContractId;
                            rstContractTeamMember.Fields[IntegrationConstants.strfEMPLOYEE_ID].Value = vntEmployeeId;

                            //Set Sales Reps role
                            object vntRoleId = null;

                            vntRoleId = rSys.Tables[IntegrationConstants.strtTEAM_MEMBER_ROLE].Fields[IntegrationConstants.strfTEAM_MEMBER_ROLE_ID]
                            .FindValue(rSys.Tables[IntegrationConstants.strtTEAM_MEMBER_ROLE].Fields[IntegrationConstants.strfROLE_NAME],
                            "Sales Representative");

                            if (DBNull.Value.Equals(vntRoleId)) { vntRoleId = null; }

                            rstContractTeamMember.Fields[IntegrationConstants.strfROLE_ID].Value = vntRoleId;
                            rstContractTeamMember.Fields[IntegrationConstants.gstrfINACTIVE].Value = false;

                            objLib.SaveRecordset(IntegrationConstants.strtOPPORTUNITY_TEAM_MEMBER, rstContractTeamMember);
                            rstContractTeamMember.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method is used to update/create an E1 Originated Inventory Quote
        /// Returns Record Id of new IQ/Opportunity record if one has been created
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntQuoteId"></param>
        /// <param name="rstPrimary"></param>
        /// <history>
        /// 28May2008   JB      Added logic to check for construction stage to determine if plan built should be checked
        /// </history>
        public object UpdateInventoryQuote(IRSystem7 rSys, object vntQuoteId, Recordset rstPrimary, object vntActiveContractOrReservationId)
        {
            try
            {
                Recordset rstQuote = null;

                //Use this object to get new recordset
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                //Update fields on Contract
                object[] arrFields = arrFields = new object[] 
                {
                    IntegrationConstants.strfOPPORTUNITY_ID, 
                    IntegrationConstants.gstrfINACTIVE, 
                    IntegrationConstants.gstrfNBHD_PHASE_ID,
                    IntegrationConstants.strfADDITIONAL_PRICE, 
                    IntegrationConstants.strfCONFIGURATION_COMPLETE, 
                    IntegrationConstants.strfECOE_DATE, 
                    IntegrationConstants.strfELEVATION_ID, 
                    IntegrationConstants.strfELEVATION_PREMIUM,
                    IntegrationConstants.strfEXTERNAL_SOURCE_NAME, 
                    IntegrationConstants.strfFINANCED_OPTIONS,
                    IntegrationConstants.strfLOT_ID, 
                    IntegrationConstants.strfLOT_PREMIUM,
                    IntegrationConstants.strfNEIGHBORHOOD_ID, 
                    IntegrationConstants.strfPIPELINE_STAGE, 
                    IntegrationConstants.strfPLAN_BUILT, 
                    IntegrationConstants.strfPLAN_NAME_ID, 
                    IntegrationConstants.strfQUOTE_CREATE_DATE, 
                    IntegrationConstants.strfQUOTE_OPTION_TOTAL, 
                    IntegrationConstants.strfSTATUS,
                    IntegrationConstants.strfTIC_FUTURE_CHANGE_PRICE, 
                    IntegrationConstants.strfTIC_OPTIONS_SQ_FT, 
                    IntegrationConstants.strfTIC_TOTAL_SQ_FT,
                    IntegrationConstants.strfTIC_FUTURE_ELEVATION_PREMIUM,
                    IntegrationConstants.strfTIC_FUTURE_LOT_PREMIUM
                };

                //AM2010.11.03 - If a reservation or contract is found 
                //we need to get the opportunity record for it.
                if ((vntActiveContractOrReservationId!=null) && !(Convert.IsDBNull(vntActiveContractOrReservationId)))
                {
                    vntQuoteId = vntActiveContractOrReservationId;

                    //If the lot has an active quote or contract or closed contract then manage the
                    //escrow and contingency information
                    //Sync Contingency information
                    this.UpdateContingencyInformation(rSys, vntQuoteId, rstPrimary);
                    //Sync Escrow data                        
                    this.ManageEscrowFieldsForLot(rSys, rstPrimary, vntQuoteId);
                }

                //Insert or Update
                if (vntQuoteId == null)
                {
                    // INSERT OF NEW INVENTORY QUOTE                    
                    // Get new recordset
                    rstQuote = objLib.GetNewRecordset(IntegrationConstants.strtOPPORTUNITY, arrFields);
                    // Add record
                    rstQuote.AddNew(Type.Missing, Type.Missing);                    
                    
                    // Populate fields with values...
                    rstQuote.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_NAME].Value = "SAP";
                    rstQuote.Fields[IntegrationConstants.strfPIPELINE_STAGE].Value = "Quote";
                    rstQuote.Fields[IntegrationConstants.strfSTATUS].Value = "Inventory";
                    rstQuote.Fields[IntegrationConstants.gstrfINACTIVE].Value = false;
                    rstQuote.Fields[IntegrationConstants.strfQUOTE_CREATE_DATE].Value = DateTime.Now;
                    rstQuote.Fields[IntegrationConstants.strfCONFIGURATION_COMPLETE].Value = 0;
                    rstQuote.Fields[IntegrationConstants.strfPLAN_BUILT].Value = 1;

                    // Set FK Field values
                    rstQuote.Fields[IntegrationConstants.strfNEIGHBORHOOD_ID].Value = rstPrimary.Fields[IntegrationConstants.strfNEIGHBORHOOD_ID].Value;
                    rstQuote.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value = rstPrimary.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value;
                    rstQuote.Fields[IntegrationConstants.strfLOT_ID].Value = rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value;                    
                    rstQuote.Fields[IntegrationConstants.strfPLAN_NAME_ID].Value = rstPrimary.Fields[IntegrationConstants.strfPLAN_ID].Value;
                    rstQuote.Fields[IntegrationConstants.strfELEVATION_ID].Value = rstPrimary.Fields[IntegrationConstants.strfELEVATION_ID].Value;
                    // Write SAP-supplied Lot Premium value to IQ's Lot_Premium
                    rstQuote.Fields[IntegrationConstants.strfLOT_PREMIUM].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_PREMIUM].Value);

                    // Custom for IP.  If Elevation supplied, set Elevation_Premium = Lot record's Elevation (NBHDP_Product).Current_Price
                    //if (!(Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfELEVATION_ID].Value)))
                    //{
                    //    rstQuote.Fields[IntegrationConstants.strfELEVATION_PREMIUM].Value = this.FindElevationCurrentPrice(rSys, rstPrimary.Fields[IntegrationConstants.strfELEVATION_ID].Value);
                    //}

                    //Set Elevation Premium for new IQ to whatever Sequence sheet is passing
                    rstQuote.Fields[IntegrationConstants.strfELEVATION_PREMIUM].Value
                        = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_ELEVATION_PREMIUM].Value);
                    // ASM: Review this block with Adam.  Commented-out.  Doubt we need this as fields not mentioned in design.
                    /*
                    rstQuote.Fields[IntegrationConstants.strfFINANCED_OPTIONS].Value = rstPrimary.Fields[IntegrationConstants.strfLOT_OPTIONTOTAL].Value;
                    rstQuote.Fields[IntegrationConstants.strfQUOTE_OPTION_TOTAL].Value = rstPrimary.Fields[IntegrationConstants.strfLOT_OPTIONTOTAL].Value;
                    */




                }
                else
                {
                    // Get the Inventory Quote record with the supplied Record Id
                    rstQuote = objLib.GetRecordset(vntQuoteId, IntegrationConstants.strtOPPORTUNITY, arrFields);
                    rstQuote.MoveFirst();            
                }

                // The following logic occurs on both newly-created and existing/being-updated IQs...
                
                // Logic for update of Additional_Price OR TIC_Future_Change_Price on IQ record...
                if ((vntActiveContractOrReservationId != null) && (!(Convert.IsDBNull(vntActiveContractOrReservationId))))                    
                {
                    // If an *active* Contract or Reservation Id (Opportunity_Id) is supplied, then
                    // look to see if the IQ.Additional_Price <> supplied Base Price.
                    if (TypeConvert.ToDecimal(rstQuote.Fields[IntegrationConstants.strfADDITIONAL_PRICE].Value) !=
                        TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_BASE_PRICE].Value))
                    {
                        rstQuote.Fields[IntegrationConstants.strfTIC_FUTURE_CHANGE_PRICE].Value 
                            = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_BASE_PRICE].Value);   
                    }

                    // If an *active* Contract or Reservation Id (Opportunity_Id) is supplied, then
                    // look to see if the IQ.Elevation_Premium <> supplied Elevation Premium.
                    if (TypeConvert.ToDecimal(rstQuote.Fields[IntegrationConstants.strfELEVATION_PREMIUM].Value) !=
                        TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_ELEVATION_PREMIUM].Value))
                    {
                        //this.UpdateActiveContractOrReservationElevationFutureChangePrice(rSys,
                        //    vntActiveContractOrReservationId,
                        //    TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_ELEVATION_PREMIUM].Value));
                        rstQuote.Fields[IntegrationConstants.strfTIC_FUTURE_ELEVATION_PREMIUM].Value
                            = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_ELEVATION_PREMIUM].Value);
                    }

                    //If an *active* Contract or Reservation Id (Opportunity Id) is supplied, then
                    //look to see if the IQ.Lot_Premium <> supplied Lot Premium
                    if (TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_PREMIUM].Value) !=
                        TypeConvert.ToDecimal(rstQuote.Fields[IntegrationConstants.strfLOT_PREMIUM].Value))
                    {
                        rstQuote.Fields[IntegrationConstants.strfTIC_FUTURE_LOT_PREMIUM].Value
                            = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_PREMIUM].Value);
                    }


                }
                else
                {
                    // If no Contract or Reservation related to the IQ's Product/Lot, do a straight update of
                    // the IQ's Additional_Price with the supplied Base Price (Disconnected_1_2_12)
                    // This line will also be executed on a new IQ, as long as a null/DBNull vntActiveContractOrReservationId is supplied

                    //AM2010.09.28 - In order to retro fit Lots loaded originally without
                    //Premium Prices
                    if (TypeConvert.ToBoolean(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_TIC_PRICE_OVERRIDE].Value))
                    {
                        //If true set the Lot Premium with the incoming Lot Premium Price
                        rstPrimary.Fields[IntegrationConstants.strfPRICE].Value
                            = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_PREMIUM].Value);

                    }

                    rstQuote.Fields[IntegrationConstants.strfADDITIONAL_PRICE].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_BASE_PRICE].Value);

                    //AM2010.11.02 - If a lot does not have any contracts on it then we need to update ensure that we 
                    //update everythying on the IQ since we can theoretically change plan, elevation, phase, etc, etc
                    rstQuote.Fields[IntegrationConstants.strfNEIGHBORHOOD_ID].Value = rstPrimary.Fields[IntegrationConstants.strfNEIGHBORHOOD_ID].Value;
                    rstQuote.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value = rstPrimary.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value;
                    rstQuote.Fields[IntegrationConstants.strfLOT_ID].Value = rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value;
                    rstQuote.Fields[IntegrationConstants.strfPLAN_NAME_ID].Value = rstPrimary.Fields[IntegrationConstants.strfPLAN_ID].Value;
                    rstQuote.Fields[IntegrationConstants.strfELEVATION_ID].Value = rstPrimary.Fields[IntegrationConstants.strfELEVATION_ID].Value;

                    //AM2010.11.03 - No contract or reservation update IQ to whatever the sequence sheet is passing
                    rstQuote.Fields[IntegrationConstants.strfLOT_PREMIUM].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_PREMIUM].Value);
                    rstPrimary.Fields[IntegrationConstants.strfPRICE].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_PREMIUM].Value);

                    //AM2010.09.28 - Do same logic for Elevation Premium as we do for Base Price.  If No Contract or Reserve exist
                    //then set the Elevation Premium to what the Seq Sheet is passing
                    rstQuote.Fields[IntegrationConstants.strfELEVATION_PREMIUM].Value
                        = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_ELEVATION_PREMIUM].Value);
                }
                
                // Set ECOE Date, Option and Total Sq Ft fields
                rstQuote.Fields[IntegrationConstants.strfECOE_DATE].Value = rstPrimary.Fields[IntegrationConstants.strfEST_CONTRACT_CLOSED_DATE].Value;
                rstQuote.Fields[IntegrationConstants.strfTIC_OPTIONS_SQ_FT].Value = TypeConvert.ToInt32(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_TIC_OPTIONS_SQ_FT].Value);
                rstQuote.Fields[IntegrationConstants.strfTIC_TOTAL_SQ_FT].Value = TypeConvert.ToInt32(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_TIC_TOTAL_SQ_FT].Value);
               
                // Save the Opportunity/IQ
                objLib.SaveRecordset(IntegrationConstants.strtOPPORTUNITY, rstQuote);
                // Get the Opportunity_Id - this has already been supplied in vntQuoteId for existing records, but will now
                // be returned for new records
                object vntNewRecordId = rstQuote.Fields[IntegrationConstants.strfOPPORTUNITY_ID].Value;
                // Clean-up
                rstQuote.Close();
                // Return the new record id
                return vntNewRecordId;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method is called from teh SetAdjustments method to retrieve the adjustments by 
        /// custom SQL inorder to create the Adjustent
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntContractId"></param>
        /// <param name="curAdjustment"></param>
        /// <param name="strAdjustmentType"></param>
        public void SetAdjustment(IRSystem7 rSys, object vntContractId, double curAdjustment, string strAdjustmentType)
        {
            try
            {
                string strErrMsg = string.Empty;
                object vntReleaseAdjustmentId = null;
                Recordset rst = null;
                Recordset rstOppAdjust = null;
                object[] arrFields = null;

                //Exit if Zero, or contract not found
                if (curAdjustment == 0)
                {
                    //Exit method
                    return;
                }

                //Use this object to get new recordset
                DataAccess objLib = (DataAccess)
                   rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                arrFields = new object[] 
    {
        IntegrationConstants.strfOPPORTUNITY_ADJUSTMENT_ID,
        IntegrationConstants.strfRELEASE_ADJUSTMENT_ID,
        IntegrationConstants.strfOPP_ADJUST_AMOUNT,
        IntegrationConstants.strfOPPORTUNITY_ID,
        IntegrationConstants.strfSELECTED,
        IntegrationConstants.strfSUM_FIELD
    };

                StringBuilder sqlText = new StringBuilder();

                //Get Customer SQL
                sqlText.Append("SELECT ");
                sqlText.Append("    ra.Release_Adjustment_Id ");
                sqlText.Append("From ");
                sqlText.Append("    Release_Adjustment ra ");
                sqlText.Append("    INNER JOIN Opportunity c ON c.NBHD_Phase_Id = ra.Release_Id ");
                sqlText.Append("Where ");
                sqlText.Append("    ra.Adjustment_Type =  '" + strAdjustmentType + "'");
                sqlText.Append("AND ( ra.Inactive = 0 OR ra.Inactive IS NULL ) ");
                sqlText.Append("AND c.Opportunity_Id = " + rSys.IdToString(vntContractId));

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    rst.MoveFirst();
                    vntReleaseAdjustmentId = rst.Fields[0].Value;
                    rst.Close();
                }
                else
                {
                    //Release Adjustment not found
                    strErrMsg = "Reject Record - Release Adjustment record not found for " +
                        strAdjustmentType + ".";
                    throw new PivotalApplicationException(strErrMsg);
                }

                //Create new opportunity adjustment record
                rstOppAdjust = objLib.GetNewRecordset(IntegrationConstants.strtOPPORTUNITY_ADJUSTMENT,
                    arrFields);

                //Set field values
                rstOppAdjust.AddNew(Type.Missing, Type.Missing);
                rstOppAdjust.Fields[IntegrationConstants.strfRELEASE_ADJUSTMENT_ID].Value = vntReleaseAdjustmentId;
                rstOppAdjust.Fields[IntegrationConstants.strfOPP_ADJUST_AMOUNT].Value = curAdjustment;
                rstOppAdjust.Fields[IntegrationConstants.strfOPPORTUNITY_ID].Value = vntContractId;
                rstOppAdjust.Fields[IntegrationConstants.strfSELECTED].Value = true;
                rstOppAdjust.Fields[IntegrationConstants.strfSUM_FIELD].Value = curAdjustment;

                objLib.SaveRecordset(IntegrationConstants.strtOPPORTUNITY_ADJUSTMENT, rstOppAdjust);
                rstOppAdjust.Close();
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

/// <summary>
/// This method will allow adjustments to be set for this Lot touchpoint
/// </summary>
/// <param name="rSys"></param>
/// <param name="rstPrimary"></param>
public void SetAdjustments(IRSystem7 rSys, Recordset rstPrimary)
{
try
{
    object vntContractId = null;
    string strErrMsg = string.Empty;

    vntContractId = this.FindContractByLot(rSys, rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value);

    if (DBNull.Value == vntContractId)
    {
        //See if there is an active IQ
        vntContractId = this.FindInventoryQuoteByLot(rSys, rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value);
        if (DBNull.Value == vntContractId)
        {
            //Exit
            return;
        }
    }

    //•	Change the adjustment logic to create an adjustment 
    //based on the 2 new disconnected fields. One field will be 
    //receiving a negative value and the other will receive a 
    //positive value. Any non-zero value will need to create an adjustment 
    //of type “Base House.”
    //Assumption is that the values will be passed in the Disconnected fields
    //for:
    //Disconnected_1_2_3 strfLOT_BASEINCENTIVE
    //Disconnected_1_2_5 strfLOT_MORTGAGEINCENTIVE

    //Contract Premiums
    if (Convert.ToDouble(rstPrimary.Fields[IntegrationConstants.strfLOT_BASEINCENTIVE].Value) == 0
        || DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfLOT_BASEINCENTIVE].Value)
    {
        this.SetAdjustment(rSys, vntContractId,
            Convert.ToDouble(rstPrimary.Fields[IntegrationConstants.strfLOT_BASEINCENTIVE].Value),
            IntegrationConstants.strcADJUSTMENT_BASE_HOUSE);
    }

    //Contract Discounts
    if (Convert.ToDouble(rstPrimary.Fields[IntegrationConstants.strfLOT_MORTGAGEINCENTIVE].Value) == 0
    || DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfLOT_MORTGAGEINCENTIVE].Value)
    {
        this.SetAdjustment(rSys, vntContractId,
            Convert.ToDouble(rstPrimary.Fields[IntegrationConstants.strfLOT_MORTGAGEINCENTIVE].Value),
            IntegrationConstants.strcADJUSTMENT_BASE_HOUSE);
    }

    //The logic below will not be used by MI
    /*
    //Base House
    if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfLOT_BASEINCENTIVE].Value)
    {
                    
        this.SetAdjustment(rSys, vntContractId,
            Convert.ToDouble(rstPrimary.Fields[IntegrationConstants.strfLOT_BASEINCENTIVE].Value),
            IntegrationConstants.strcADJUSTMENT_BASE_HOUSE);
    }

    //Lot Incentive House
    if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfLOT_LOTINCENTIVE].Value)
    {
        this.SetAdjustment(rSys, vntContractId,
            Convert.ToDouble(rstPrimary.Fields[IntegrationConstants.strfLOT_LOTINCENTIVE].Value),
            IntegrationConstants.strcADJUSTMENT_LOT_INCENTIVE);
    }

     //Mortage Incentive House
    if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfLOT_MORTGAGEINCENTIVE].Value)
    {
                    
        this.SetAdjustment(rSys, vntContractId,
            Convert.ToDouble(rstPrimary.Fields[IntegrationConstants.strfLOT_MORTGAGEINCENTIVE].Value),
            IntegrationConstants.strcADJUSTMENT_MORTGATE_INCENTIVES);
    }

    //AM 2007.07 - integration will not support Marketing Incentives
    //Marketing Incentive 
    //if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfLOT_MARKETINGINCENTIVE].Value)
    //{
    //    this.SetAdjustment(vntContractId,
    //       rstPrimary.Fields[IntegrationConstants.strfLOT_MARKETINGINCENTIVE].Value,
    //        IntegrationConstants.strcADJUSTMENT_MARKETING_INCENTIVES);
    //}

    //OPTION Incentive
    if (DBNull.Value != rstPrimary.Fields[IntegrationConstants.strfLOT_OPTIONINCENTIVE].Value)
    {
                    
        this.SetAdjustment(rSys, vntContractId,
            Convert.ToDouble(rstPrimary.Fields[IntegrationConstants.strfLOT_OPTIONINCENTIVE].Value),
            IntegrationConstants.strcADJUSTMENT_OPTION);
    }
         */
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method is used by the Lot Touchpoint to inactivate any IQ for 
        /// a specific lot
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="lotId"></param>
        public void InactiveInventoryQuotes(IRSystem7 rSys, object lotId)
        {
            try
            {
                string strErrMsg = string.Empty;
                Recordset rst = null;
                Recordset rstInvQuote = null;

                //Use this object to get new recordset
                DataAccess objLib = (DataAccess)
                   rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                StringBuilder sqlText = new StringBuilder();

                //Get Customer SQL
                sqlText.Append("SELECT ");
                sqlText.Append("    Opportunity_Id ");
                sqlText.Append("From ");
                sqlText.Append("    Opportunity ");
                sqlText.Append("Where ");
                sqlText.Append("    Lot_Id = " + rSys.IdToString(lotId));
                sqlText.Append(" AND Status = 'Inventory' ");

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    rst.MoveFirst();

                    rstInvQuote = objLib.GetRecordset(rst.Fields[0].Value,
                        IntegrationConstants.strtOPPORTUNITY, new object[] { IntegrationConstants.gstrfINACTIVE });

                    if (rstInvQuote.RecordCount > 0)
                    {
                        rstInvQuote.MoveFirst();

                        while (!rstInvQuote.EOF)
                        {
                            //Inactivate quotes
                            rstInvQuote.Fields[IntegrationConstants.gstrfINACTIVE].Value = true;
                            rstInvQuote.MoveNext();
                        }

                        //Save recordset
                        objLib.SaveRecordset(IntegrationConstants.strtOPPORTUNITY, rstInvQuote);
                    }
                    rstInvQuote.Close();
                }

                //Clean Up
                rst.Close();
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Use this function to do a lookup to get the default Cancel Reason from Pivotal
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="cancelreason"></param>
        /// <returns></returns>
        public object FindCancelReason(IRSystem7 rSys, string cancelreason)
        {
            try
            {
                object vntResult = null;

                if (cancelreason == "")
                {

                    vntResult = rSys.Tables[IntegrationConstants.strtSYSTEM].Fields[IntegrationConstants.strfINT_DEFAULT_CANCEL_REASON_ID].FindValue(
                      rSys.Tables[IntegrationConstants.strtSYSTEM].Fields[IntegrationConstants.strfSYSTEM_ID],
                      rSys.StringToId("0x0000000000000001"));
                }
                else
                {
                    vntResult = rSys.Tables[IntegrationConstants.strfCANCEL_REASON].Fields[IntegrationConstants.strfCANCEL_REASON_ID].FindValue(
                    rSys.Tables[IntegrationConstants.strfCANCEL_REASON].Fields[IntegrationConstants.strfCANCEL_REASON],
                    cancelreason);
                }

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }

                //Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        // ASM: Commented this out, as it's name clashes with my new FindPlan method, but we may need this again later.
        /*
        /// <summary>
        /// This mehtod is used by the integration to look up plan for the lot integration touchpoint
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="planId"></param>
        /// <returns></returns>
        public object FindPlan(IRSystem7 rSys, string planId, Boolean isLot)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();                
                StringBuilder sqlText = new StringBuilder();
                object vntResult = null;

                //make sure default contrustion stage is populated
                //reject if not found
                object vntConstructionId = rSys.Tables[IntegrationConstants.strtSYSTEM].Fields[IntegrationConstants.strfINT_DEFAULT_CONSTRUCTION_STAGE].FindValue(
                    rSys.Tables[IntegrationConstants.strtSYSTEM].Fields[IntegrationConstants.strfSYSTEM_ID],
                    rSys.StringToId("0x0000000000000001"));

                if (null == vntConstructionId)
                {
                    throw new PivotalApplicationException("Reject Record - The default construction stage is not set in teh System Table");
                }

                //Build custom sql to find plan record
                sqlText.Append("SELECT ");
                sqlText.Append("    npp.NBHDP_Product_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("   NBHDP_Product npp ");
                sqlText.Append("WHERE ");
                sqlText.Append("    npp.Type = 'Plan' ");
                sqlText.Append("AND npp.External_Source_Id = '" + planId + "'");

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0 && isLot == true)
                {
                    //Found it, now check the consturction stage
                    rst.Close();

                    //BKP 05/30/2007 - only set plans if the construction stage ordinal of the plan
                    //is greater than or equal to the default construction stage's
                    //ordinal

                    StringBuilder sqlText2 = new StringBuilder();

                    sqlText2.Append("SELECT ");
                    sqlText2.Append("    npp.NBHDP_Product_Id ");
                    sqlText2.Append("FROM ");
                    sqlText2.Append("   NBHDP_Product npp ");
                    sqlText2.Append("WHERE ");
                    sqlText2.Append("    npp.Type = 'Plan' ");
                    sqlText2.Append("AND npp.External_Source_Id = '" + planId + "'");
                    sqlText2.Append(" AND npp.Construction_Stage_Ordinal >= ");
                    sqlText2.Append("    ( ");
                    sqlText2.Append("        SELECT ");
                    sqlText2.Append("            cs.Construction_Stage_Ordinal ");
                    sqlText2.Append("        FROM ");
                    sqlText2.Append("            Construction_Stage cs ");
                    sqlText2.Append("            INNER JOIN System s ON s.Int_Default_Construction_Stage = ");
                    sqlText2.Append("               cs.Construction_Stage_Id ");
                    sqlText2.Append("    ) ");

                    rst = objLib.GetRecordset(sqlText2.ToString());

                    if (rst.RecordCount > 0)
                    {
                    rst.MoveFirst();

                    vntResult = rst.Fields[0].Value;
                    rst.Close();

                    //}
                    //else
                    //{
                    //Not found - means that the construction stage is before the default
                    //no need to pass something that is not null
                    //vntResult = rSys.StringToId("0xFFFFFFFFFFFFFFFF");
                    //}
                }
                else if (rst.RecordCount > 0)
                {
                    rst.MoveFirst();

                    vntResult = rst.Fields[0].Value;
                    rst.Close();
                }
                else
                {
                    //Not found set to null
                    vntResult = DBNull.Value;
                }

                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        */

        /// <summary>
        /// Used by the Lot touchpoint to get the contract by a particular lot Id
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntLotId"></param>
        /// <returns></returns>
        public object FindContractByLot(IRSystem7 rSys, object vntLotId)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                StringBuilder sqlText = new StringBuilder();
                object vntResult = null;

                sqlText.Append("SELECT ");
                sqlText.Append("o.Opportunity_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("Opportunity o ");
                sqlText.Append("INNER JOIN Product lot ON lot.Product_Id = o.Lot_Id ");
                sqlText.Append("WHERE ");
                sqlText.Append("lot.Product_Id = " + rSys.IdToString(vntLotId));
                sqlText.Append(" AND ");
                sqlText.Append("( ");
                sqlText.Append("(o.Pipeline_Stage = 'Contract' AND o.Status = 'In Progress')");
                    sqlText.Append(" OR ");
                    sqlText.Append("(o.Pipeline_Stage = 'Quote' AND o.Status = 'Reserved')");
                    sqlText.Append(" OR ");
                    sqlText.Append("(o.Pipeline_Stage = 'Contract' AND o.Status = 'Closed')");
                    sqlText.Append(" OR ");
                    sqlText.Append("(o.Pipeline_stage = 'Closed' AND o.Status = 'Closed')");
                sqlText.Append(") ");
                sqlText.Append("AND (o.Inactive = 0 OR o.Inactive IS NULL) ");

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    rst.MoveFirst();
                    //Grab the Opportunity_Id and return it
                    vntResult = rst.Fields[0].Value;
                    rst.Close();
                }
                else
                {
                    // No results - return Null
                    vntResult = DBNull.Value;
                }

                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /*
        /// <summary>
        /// Used by the Lot touchpoint to get the contract by a particular lot Id
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntLotId"></param>
        /// <returns></returns>
        public object FindContractByLot(IRSystem7 rSys, object vntLotId)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                StringBuilder sqlText = new StringBuilder();
                object vntResult = null;

                sqlText.Append("SELECT ");
                sqlText.Append("   o.Opportunity_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("   Opportunity o ");
                sqlText.Append("   INNER JOIN Product lot ON lot.Product_Id = o.Lot_Id ");
                sqlText.Append("WHERE ");
                sqlText.Append("    lot.Product_Id = " + rSys.IdToString(vntLotId));
                sqlText.Append(" AND ");
                sqlText.Append("( ");
                sqlText.Append("    o.Pipeline_Stage = 'Contract' ");
                //BKP 05/11/2007 - we don't need to look for reservations
                //    sqlText.Append("    OR "
                //    sqlText.Append("    ( "
                //    sqlText.Append("        o.Pipeline_Stage = 'Quote' "
                //    sqlText.Append("        AND "
                //    sqlText.Append("        o.Status = 'Reserved' "
                //    sqlText.Append("    ) "
                sqlText.Append(") ");
                sqlText.Append("AND ( o.Inactive = 0 OR o.Inactive IS NULL ) ");

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    rst.MoveFirst();
                    //Grab the Id and the Pipeline stage
                    vntResult = rst.Fields[0].Value;
                    rst.Close();
                }
                else
                {
                    vntResult = DBNull.Value;
                }

                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        */

        /// <summary>
        /// This method can be used to find the Inventory Quote by the Lot Id passed in.
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntIQId"></param>
        /// <returns></returns>
        public object FindInventoryQuoteByLot(IRSystem7 rSys, object vntLotId)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                StringBuilder sqlText = new StringBuilder();
                object vntResult = null;

                sqlText.Append("SELECT ");
                sqlText.Append("   o.Opportunity_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("   Opportunity o ");
                sqlText.Append("   INNER JOIN Product lot ON lot.Product_Id = o.Lot_Id ");
                sqlText.Append("WHERE ");
                sqlText.Append("    lot.Product_Id = " + rSys.IdToString(vntLotId));
                sqlText.Append(" AND ");
                sqlText.Append("(o.Pipeline_Stage = 'Quote') AND (o.Status = 'Inventory') ");
                sqlText.Append("AND (o.Inactive = 0 OR o.Inactive IS NULL) ");

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    rst.MoveFirst();
                    //Grab the Id
                    vntResult = rst.Fields[0].Value;
                }
                else
                {
                    //Cannot find the Opportunity - return Null
                    vntResult = DBNull.Value;
                }

                rst.Close();
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        public decimal FindElevationCurrentPrice(IRSystem7 rSys, object vntElevationId)
        {
            try
            {
                return TypeConvert.ToDecimal(rSys.Tables[IntegrationConstants.strtNBHDP_PRODUCT].Fields[IntegrationConstants.strfCURRENT_PRICE].FindValue(
                                             rSys.Tables[IntegrationConstants.strtNBHDP_PRODUCT].Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID], 
                                             vntElevationId));
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will be used to lookup the current price for a lot
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        public decimal FindCurrentPrice(IRSystem7 rSys, Recordset rstPrimary, bool blnLot)
        {
            try
            {
                decimal decResult = 0;
                object vntResult = null;

                if (blnLot)
                {
                    vntResult = rSys.Tables[IntegrationConstants.strtPRODUCT].Fields[IntegrationConstants.strfPRICE].FindValue(
                        rSys.Tables[IntegrationConstants.strtPRODUCT].Fields[IntegrationConstants.strfPRODUCT_ID],
                        rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value);
                }
                else
                {
                    vntResult = rSys.Tables[IntegrationConstants.strtNBHDP_PRODUCT].Fields[IntegrationConstants.strfCURRENT_PRICE].FindValue(
                       rSys.Tables[IntegrationConstants.strtNBHDP_PRODUCT].Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID],
                       rstPrimary.Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID].Value);
                }

                //If nothing is returned make sure you null out result
                if (DBNull.Value.Equals(vntResult))
                {
                    decResult = 0;
                }
                else
                {
                    decResult = Convert.ToDecimal(vntResult);
                }

                //Return result
                return decResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will be used to lookup the current price for a lot
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        public object FindNextPrice(IRSystem7 rSys, Recordset rstPrimary, bool blnLot)
        {
            try
            {                
                object decResult = null;
                object vntResult = null;

                if (blnLot)
                {
                    vntResult = rSys.Tables[IntegrationConstants.strtPRODUCT].Fields[IntegrationConstants.strfNEXT_PRICE].FindValue(
                        rSys.Tables[IntegrationConstants.strtPRODUCT].Fields[IntegrationConstants.strfPRODUCT_ID],
                        rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value);
                }
                else
                {
                    vntResult = rSys.Tables[IntegrationConstants.strtNBHDP_PRODUCT].Fields[IntegrationConstants.strfNEXT_PRICE].FindValue(
                       rSys.Tables[IntegrationConstants.strtNBHDP_PRODUCT].Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID],
                       rstPrimary.Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID].Value);

                }

                //If nothing is returned, return null, as we DON'T want NULL to = 0
                if (DBNull.Value.Equals(vntResult))
                {
                    // Do nothing, return null
                }
                else
                {
                    decResult = Convert.ToDecimal(vntResult);
                }

                //Return result
                return decResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This is the method used for Inserting a new Price Change History record.
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rstPrimary"></param>
        /// <param name="blnAction"></param>
        /// <param name="strLotOrProduct"></param>
        public void InsertPriceChangeHistory(IRSystem7 rSys, Recordset rstPrimary, bool blnAction, string strLotOrProduct)
        {
            try
            {
                string strErrMsg = string.Empty;
                Recordset rstPriceChange = null;
                object[] arrFields = null;

                // Check to see if this is for Lot or NBHDP_Product-related Price_Change_History record, defining the set
                // of fields we will want to write to accordingly.
                if (strLotOrProduct == "Lot")
                {
                    // Lot-related Price_Change_History record
                    arrFields = new object[] 
                    {                        
                        IntegrationConstants.strfLOT_ID,
                        IntegrationConstants.strfCHANGE_DATE,
                        IntegrationConstants.strfCHANGE_TIME,
                        IntegrationConstants.strfPROCESSED,
                        IntegrationConstants.strfMARGIN,
                        IntegrationConstants.strfCOST                        
                    };
                }
                else
                {
                    // NBHDP_Product-related Price_Change_History record
                    arrFields = new object[] 
                    {                        
                        IntegrationConstants.strfNBHDP_PRODUCT_ID,
                        IntegrationConstants.strfCHANGE_DATE,
                        IntegrationConstants.strfCHANGE_TIME,
                        IntegrationConstants.strfPROCESSED,
                        IntegrationConstants.strfMARGIN,
                        IntegrationConstants.strfCOST
                    };
                }
                
                // Get Data Access object
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                // Get a new Price_Change_History record
                rstPriceChange = objLib.GetNewRecordset(IntegrationConstants.strtPRICE_CHANGE_HISTORY, arrFields);
                //Force new record to be created
                rstPriceChange.AddNew(Type.Missing, Type.Missing);

                // Check to see if this is for Lot or NBHDP_Product-related Price_Change_History record
                if (strLotOrProduct == "Lot")
                {
                    // Lot-related Price_Change_History record
                    // Relate the new PCH record to the Lot
                    rstPriceChange.Fields[IntegrationConstants.strfLOT_ID].Value = rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value;
                    // Price logic for Lot - Set Margin to 0
                    rstPriceChange.Fields[IntegrationConstants.strfMARGIN].Value = 0;
                    // Set Cost_Price to supplied Lot Premium
                    rstPriceChange.Fields[IntegrationConstants.strfCOST].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_PREMIUM].Value);
                }
                else if (strLotOrProduct == "Plan")
                {
                    // NBHDP_Product-related Price_Change_History record
                    // Relate the new PCH record to the NBHDP_Product
                    rstPriceChange.Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID].Value = rstPrimary.Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID].Value;
                    // Price Logic for Option and Plan - set to 0 if NULL supplied, set to value if value supplied
                    rstPriceChange.Fields[IntegrationConstants.strfCOST].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfPLAN_PRICE].Value);
                    // Set Margin - set to 0 if NULL supplied, set to value if value supplied
                    rstPriceChange.Fields[IntegrationConstants.strfMARGIN].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfPLAN_MARGIN].Value);
                }
                else
                {
                    // This branch is most likely processed when the strLotOrProduct parameter is filled with "Option" from Option.cs calls
                    // Some other supplied value to relate the new Price_Change_History record to...
                    rstPriceChange.Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID].Value = rstPrimary.Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID].Value;                    
                    //Price Logic for Option and Plan     
                    //AM2010.08.19 - Ensured Cost_Price is sourced from Chateau Price
                    rstPriceChange.Fields[IntegrationConstants.strfCOST].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfOPTION_PRICE].Value);
                    //Set Margin
                    //AM2010.08.18 - Discussed with with Adam (2010.08.18 - Need to default to 0 for Margin so that Price calculations work correctly).
                    rstPriceChange.Fields[IntegrationConstants.strfMARGIN].Value = 0;  //TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfOPTION_MARGIN].Value);
                    
                }

                // Set Price_Change_History.Processed field
                if (blnAction)
                {
                    rstPriceChange.Fields[IntegrationConstants.strfPROCESSED].Value = true;
                }
                else
                {
                    rstPriceChange.Fields[IntegrationConstants.strfPROCESSED].Value = false;
                }

                // Set Price_Change_History.Change_Date field
                rstPriceChange.Fields[IntegrationConstants.strfCHANGE_DATE].Value = DateTime.Now;

                // Set Price_Change_History.Change_Time field
                rstPriceChange.Fields[IntegrationConstants.strfCHANGE_TIME].Value = DateTime.Now;

                // Write new Price_Change_History record to the database
                objLib.SaveRecordset(IntegrationConstants.strtPRICE_CHANGE_HISTORY, rstPriceChange);

                // Clean-up
                rstPriceChange.Close();
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        public void SetDivisionProduct(IRSystem7 rSys, IRForm pForm, Recordset rstPrimary)
        {
            try
            {

                Recordset rstDivProd = null;
                Recordset rstConfigType = null;
                Recordset rstPhase = null;
                string strPlanTypeLookupFldName = string.Empty;
                string strEstSqFeetLookupFldName = string.Empty;
                string strProductNameLookupFldName = string.Empty;
                string strCostLookupFldName = string.Empty;
                string strMarginLookupFldName = string.Empty;
                string strPlanDescriptionLookupFldName = string.Empty;
                string strExtSourceId = string.Empty;
                bool blnAddDivProduct = true;
                bool blnAddType = true;
                int x = 0;

                object[] vntAllTargetFieldNames = null;
                object[] vntDefaultTargetFldNames = null;
                object[] vntUpdateTargetFldNames = null;
                object[] vntDefaultSourceFldNames = null;
                object[] vntUpdateSourceFldNames = null;
                object vntCommunity = null;
                object vntDivisionId = null;
                object vntPhaseId = null;
                object vntConfigTypeId = null;
                object vntDivProductId = null;
                string strInTrans = string.Empty;
                string strInTrans2 = string.Empty;

                //Debug code - check this ASR is in a transaction - output goes to Immediate window in VB IDE

                //Get Community
                vntCommunity = rstPrimary.Fields[IntegrationConstants.strfNEIGHBORHOOD_ID].Value;

                //Update NBHDP_Product, set Division_Id
                if (!DBNull.Value.Equals(vntCommunity))
                {
                    vntDivisionId = rSys.Tables[IntegrationConstants.strtNEIGHBORHOOD].Fields[IntegrationConstants.strfDIVISION_ID]
                    .Index(vntCommunity);
                    rstPrimary.Fields[IntegrationConstants.strfDIVISION_ID].Value = vntDivisionId;
                }

                //Update NBHDP_Product, set NBHD_Phase_Id
                //BUild phase external_Source_id
                //lookup phase_Id using external_source_id
                //Prep parameters
                strExtSourceId = this.HandleNullValues(rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_COMMUNITY_ID].Value.ToString()) + "-" +
                    this.HandleNullValues(rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_PHASE_CODE].Value.ToString());
                IRDataset4 rdstDataSet = (IRDataset4)rSys.CreateDataset();
                rstPhase = this.ReturnLookUp(rSys, rdstDataSet, IntegrationConstants.gstrtNBHD_PHASE, new object[] { IntegrationConstants.gstrfNBHD_PHASE_ID },
                    IntegrationConstants.gstrqPHASE_BY_EXT_SOURCE_ID, true, new object[] { strExtSourceId });

                if (rstPhase.RecordCount > 0)
                {
                    rstPhase.MoveFirst();
                    vntPhaseId = rstPhase.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value;
                }
                else
                {
                    vntPhaseId = DBNull.Value;
                }

                //Set Phase Id
                rstPrimary.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value = vntPhaseId;

                //Add/Update Division Product based on NBHDP_Product + Update NBHDP_Product with Division ProductLink

                //BKP 05/10/2007 - updated field labels for new Fields
                //Get Plan Type and est.sq feet disconnected field name to update division product
                object[] arrStr1 = this.GetDisconnected(rSys, pForm, rstPrimary, IntegrationConstants.strfPLAN_PLANTYPE, pForm.Segments[1].SegmentName);
                object[] arrStr2 = this.GetDisconnected(rSys, pForm, rstPrimary, IntegrationConstants.strfPLAN_SQUAREFEET, pForm.Segments[1].SegmentName);
                object[] arrStr3 = this.GetDisconnected(rSys, pForm, rstPrimary, IntegrationConstants.strfPRODUCT_NAME, pForm.Segments[1].SegmentName);
                object[] arrStr4 = this.GetDisconnected(rSys, pForm, rstPrimary, IntegrationConstants.strfPLAN_PRICE, pForm.Segments[1].SegmentName);
                object[] arrStr5 = this.GetDisconnected(rSys, pForm, rstPrimary, IntegrationConstants.strfPLAN_MARGIN, pForm.Segments[1].SegmentName);
                object[] arrStr6 = this.GetDisconnected(rSys, pForm, rstPrimary, IntegrationConstants.strfPLAN_DESCRIPTION, pForm.Segments[1].SegmentName);

                strPlanTypeLookupFldName = arrStr2[0].ToString();
                strEstSqFeetLookupFldName = arrStr1[0].ToString();
                strProductNameLookupFldName = arrStr3[0].ToString();
                strCostLookupFldName = arrStr4[0].ToString();
                strMarginLookupFldName = arrStr5[0].ToString();
                strPlanDescriptionLookupFldName = arrStr6[0].ToString();


                //BKP 05/30/2007 - Include default construction stage
                vntAllTargetFieldNames = new object[] 
                {
                  IntegrationConstants.strfEXTERNAL_SOURCE_NAME,
                    IntegrationConstants.strfEXTERNAL_SOURCE_ID, 
                    IntegrationConstants.gstrfTYPE,
                    IntegrationConstants.gstrfINACTIVE,
                    IntegrationConstants.strfEXTERNAL_SOURCE_COMMUNITY_ID,
                    IntegrationConstants.strfEXTERNAL_SOURCE_PHASE_CODE,
                    IntegrationConstants.strfEXTERNAL_SOURCE_PLAN_CODE,
                    IntegrationConstants.strfEXTERNAL_SOURCE_ELEV_CODE,
                    IntegrationConstants.strfESTIMATED_SQ_FEET, 
                    IntegrationConstants.strfDIV_PRODUCT_NAME,
                    IntegrationConstants.strfPRICE,
                    IntegrationConstants.strfREMOVAL_DATE,
                    IntegrationConstants.strfPLAN_TYPE, 
                    IntegrationConstants.strfDIVISION_ID, 
                    IntegrationConstants.strfPRODUCT_CODE,
                    IntegrationConstants.strfCATEGORY_ID, 
                    IntegrationConstants.strfDIVISION_PRODUCT_ID,
                    IntegrationConstants.strfCONSTRUCTION_STAGE_ID,
                    IntegrationConstants.strfREGION_ID,
                    IntegrationConstants.strfAVAILABLE_DATE,
                    IntegrationConstants.strfCOST,
                    IntegrationConstants.strfMARGIN,
                    IntegrationConstants.strfDIV_PRODUCT_NAME,
                    IntegrationConstants.strfDESCRIPTION
                    
                };

                string strExternalSourceId = this.HandleNullValues(rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_COMMUNITY_ID].Value.ToString()) + "-" +
                    this.HandleNullValues(rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_PHASE_CODE].Value.ToString()) + "-" +
                    this.HandleNullValues(rstPrimary.Fields[IntegrationConstants.strfPLAN_CODE].Value.ToString()) + "-" +
                    this.HandleNullValues(rstPrimary.Fields[IntegrationConstants.strfELEVATION_CODE].Value.ToString());

                rstDivProd = this.ReturnLookUp(rSys, rdstDataSet, IntegrationConstants.strtDIVISION_PRODUCT, vntAllTargetFieldNames,
                    IntegrationConstants.strqDIVISION_PRODUCT_BY_EXT_SOURCE_ID_TYPE, true, new object[] { strExternalSourceId, "Plan" });

                //default target fields set on insert
                vntDefaultTargetFldNames = new object[] 
                {
                    IntegrationConstants.strfEXTERNAL_SOURCE_NAME, IntegrationConstants.strfEXTERNAL_SOURCE_ID, 
                    IntegrationConstants.gstrfTYPE, IntegrationConstants.gstrfINACTIVE
                };

                //Update target fields set on insert/update
                vntUpdateTargetFldNames = new object[] 
                {
                    IntegrationConstants.strfEXTERNAL_SOURCE_COMMUNITY_ID,
                    IntegrationConstants.strfEXTERNAL_SOURCE_PHASE_CODE,
                    IntegrationConstants.strfEXTERNAL_SOURCE_PLAN_CODE,
                    IntegrationConstants.strfEXTERNAL_SOURCE_ELEV_CODE,
                    IntegrationConstants.strfESTIMATED_SQ_FEET, 
                    //IntegrationConstants.strfPRODUCT_NAME,
                    IntegrationConstants.strfPRICE,
                    IntegrationConstants.strfREMOVAL_DATE,
                    IntegrationConstants.strfPLAN_TYPE, 
                    IntegrationConstants.strfDIVISION_ID, 
                    IntegrationConstants.strfPRODUCT_CODE,
                    IntegrationConstants.strfREGION_ID,
                    IntegrationConstants.strfAVAILABLE_DATE,
                    IntegrationConstants.strfCOST,
                    IntegrationConstants.strfMARGIN,
                    IntegrationConstants.strfDIV_PRODUCT_NAME,
                    IntegrationConstants.strfDESCRIPTION
                   
                };

                //Default source fields set on insert
                vntDefaultSourceFldNames = new object[] 
                {
                    IntegrationConstants.strfEXTERNAL_SOURCE_NAME, IntegrationConstants.strfEXTERNAL_SOURCE_ID, 
                    IntegrationConstants.gstrfTYPE, IntegrationConstants.gstrfINACTIVE
                };

                //Update target fields set on insert/update
                vntUpdateSourceFldNames = new object[]
                {
                    IntegrationConstants.strfEXTERNAL_SOURCE_COMMUNITY_ID,
                    IntegrationConstants.strfEXTERNAL_SOURCE_PHASE_CODE,
                    IntegrationConstants.strfPLAN_CODE,
                    IntegrationConstants.strfELEVATION_CODE,
                    strEstSqFeetLookupFldName, 
                    //IntegrationConstants.strfPRODUCT_NAME,
                    IntegrationConstants.strfCURRENT_PRICE,
                    IntegrationConstants.strfREMOVAL_DATE,
                    strPlanTypeLookupFldName, 
                    IntegrationConstants.strfDIVISION_ID, 
                    IntegrationConstants.strfPRODUCT_CODE,
                    IntegrationConstants.strfREGION_ID,
                    IntegrationConstants.strfAVAILABLE_DATE,
                    strCostLookupFldName,
                    strMarginLookupFldName,
                    strProductNameLookupFldName,
                    strPlanDescriptionLookupFldName

                };

                //Check if insert/update division_product
                blnAddDivProduct = false;

                if (rstDivProd.RecordCount > 0)
                {
                    rstDivProd.MoveFirst();

                }
                else
                {
                    blnAddDivProduct = true;
                }

                //Insert DivisionProduct
                if (blnAddDivProduct)
                {
                    //Check if config. type "Plan" exists

                    IRDataset4 rdstDataSet1 = (IRDataset4)rSys.CreateDataset();
                    rstConfigType = this.ReturnLookUp(rSys, rdstDataSet1, IntegrationConstants.strtCONFIG_TYPE, new object[]{IntegrationConstants.strfCONFIG_TYPE_ID, 
                        IntegrationConstants.strfCONFIG_TYPE_NAME, IntegrationConstants.gstrfINACTIVE, IntegrationConstants.gstrfCOMPONENT}, IntegrationConstants.strqCONFIG_TYPE_BY_CONFIG_TYPE_NAME_COMPONENT, true, new object[] { IntegrationConstants.strPLAN, IntegrationConstants.strPLAN });

                    blnAddType = false;
                    vntConfigTypeId = null;

                    if (rstConfigType.RecordCount > 0)
                    {
                        rstConfigType.MoveFirst();
                        //Get Config type id for Plan - to set DIvision Product
                        vntConfigTypeId = rstConfigType.Fields[IntegrationConstants.strfCONFIG_TYPE_ID].Value;
                    }
                    else
                    {
                        blnAddType = true;
                    }

                    //if config type does not exist, add new
                    if (blnAddType)
                    {
                        //IRDataset4 rdstDataSet2 = (IRDataset4)rSys.CreateDataset();
                        //rstConfigType.AddNew(Type.Missing, Type.Missing);
                        //rstConfigType.Fields[IntegrationConstants.strfCONFIG_TYPE_NAME].Value = "Plan";
                        //rstConfigType.Fields[IntegrationConstants.gstrfINACTIVE].Value = 0;
                        //rstConfigType.Fields[IntegrationConstants.gstrfCOMPONENT].Value = "Plan";

                        //rdstDataSet2.SaveRecordset(rstConfigType);

                        //Get Config type id for "Plan" - to set for Division Product
                        //vntConfigTypeId = rstConfigType.Fields[IntegrationConstants.strfCONFIG_TYPE_ID].Value;

                        String strErrMsg = "Rejected Record - " +
                        "Plan configuration type does not exist";
                        throw new PivotalApplicationException(strErrMsg);

                    }

                    //Add Division Product
                    rstDivProd.AddNew(Type.Missing, Type.Missing);

                    //For every Field name in Target assig values
                    for (x = 0; x < vntDefaultTargetFldNames.Length; x++)
                    {
                        rstDivProd.Fields[vntDefaultTargetFldNames[x]].Value =
                            rstPrimary.Fields[vntDefaultSourceFldNames[x]].Value;

                    }

                    //Set Division Product Config Type
                    rstDivProd.Fields[IntegrationConstants.strfCATEGORY_ID].Value = vntConfigTypeId;

                }


                //Update Division Product fields
                for (int y = 0; y < vntUpdateTargetFldNames.Length; y++)
                {

                    rstDivProd.Fields[vntUpdateTargetFldNames[y]].Value
                        = rstPrimary.Fields[vntUpdateSourceFldNames[y]].Value;
                }

                //BKP 05/10/2007 - ensure External_Source_Id gets populated
                rstDivProd.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID].Value = strExternalSourceId;
                rstDivProd.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_NAME].Value = "E1";

                //BKP 05/03/2007 - default the construction stage
                if (DBNull.Value == rstDivProd.Fields[IntegrationConstants.strfCONSTRUCTION_STAGE_ID].Value)
                {
                    rstDivProd.Fields[IntegrationConstants.strfCONSTRUCTION_STAGE_ID].Value = this.GetDefaultConstructionStageId(rSys);

                }

                //Save Division Product
                rdstDataSet.SaveRecordset(rstDivProd);

                //Get Division Product Id
                vntDivProductId = rstDivProd.Fields[IntegrationConstants.strfDIVISION_PRODUCT_ID].Value;

                //Update NBHDP_Product, set Division Product Id
                rstPrimary.Fields[IntegrationConstants.strfDIVISION_PRODUCT_ID].Value = vntDivProductId;

                //Clean Up
                rstDivProd.Close();
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }


        /// <summary>
        /// This method is used to find the max price change history record by using custom SQL
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public DateTime FindMaxPriceChangeHistory(IRSystem7 rSys, object id)
        {
            try
            {

                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                string vntResult;
                StringBuilder sqlText = new StringBuilder();


                //Build custom sql to find lot__comopany record
                sqlText.Append("SELECT ");
                sqlText.Append("    MAX(Change_Timestamp) ");
                sqlText.Append("FROM ");
                sqlText.Append("    Price_Change_History ");
                sqlText.Append("WHERE ");
                sqlText.Append("    Processed = 1 ");
                sqlText.Append("AND NBHDP_Product_Id = " + rSys.IdToString(id));
                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    //Found it
                    rst.MoveFirst();
                    vntResult = rst.Fields[0].Value.ToString();
                }
                else
                {
                    vntResult = string.Empty;
                }

                if (null == vntResult)
                {
                    vntResult = "01/01/1900 12:00:00";
                }

                return Convert.ToDateTime(vntResult);

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }



        /// <summary>
        /// This method will get the default Construction Stage Id defined in teh system table
        /// </summary>
        /// <param name="rSys"></param>
        /// <returns></returns>
        public object GetDefaultConstructionStageId(IRSystem7 rSys)
        {
            try
            {

                object vntResult = null;
                string strErrMsg = string.Empty;

                vntResult = rSys.Tables[IntegrationConstants.strtSYSTEM].Fields[IntegrationConstants.strfINT_DEFAULT_CONSTRUCTION_STAGE]
                .FindValue(rSys.Tables[IntegrationConstants.strtSYSTEM].Fields[IntegrationConstants.strfSYSTEM_ID],
                rSys.StringToId("0x0000000000000001"));

                if (DBNull.Value.Equals(vntResult))
                {
                    vntResult = DBNull.Value;
                }
                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        /// <summary>
        /// This method will get the assigned or default sales team
        /// </summary>
        /// <param name="rSys"></param>
        /// <returns></returns>
        public object FindSalesTeam(IRSystem7 rSys, string abNumber, string communityID, string areaID)
        {
            object salesTeamID = null;
            object employeeID = null;
            object divisionID = null;

            try
            {

                if (abNumber != "")
                {
                    employeeID = FindEmployeeByAB(rSys, abNumber);
                }
                if (DBNull.Value != employeeID && employeeID != null)
                {
                    salesTeamID = FindSalesTeamByEmployee(rSys, employeeID);
                    if (DBNull.Value == salesTeamID)
                    {
                        if (areaID == "")
                        {
                            divisionID = FindDivisionByCommunity(rSys, communityID);
                        }
                        else
                        {
                            divisionID = FindDivisionByArea(rSys, areaID);
                        }
                        employeeID = FindDefaultEmployee(rSys, divisionID);
                        salesTeamID = FindSalesTeamByEmployee(rSys, employeeID);
                    }
                }
                else
                {
                    if (areaID == "")
                    {
                        divisionID = FindDivisionByCommunity(rSys, communityID);
                    }
                    else
                    {
                        divisionID = FindDivisionByArea(rSys, areaID);
                    }
                    employeeID = FindDefaultEmployee(rSys, divisionID);
                    salesTeamID = FindSalesTeamByEmployee(rSys, employeeID);
                }

                return salesTeamID;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will get the assigned or default sales team
        /// </summary>
        /// <param name="rSys"></param>
        /// <returns></returns>
        public object FindEmployeeByAB(IRSystem7 rSys, string abNumber)
        {
            try
            {

                object vntResult = null;
                string strErrMsg = string.Empty;

                vntResult = rSys.Tables[IntegrationConstants.strtEMPLOYEE].Fields[IntegrationConstants.strfEMPLOYEE_ID]
                .FindValue(rSys.Tables[IntegrationConstants.strtEMPLOYEE].Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID],
                abNumber);

                if (DBNull.Value.Equals(vntResult))
                {
                    vntResult = DBNull.Value;
                }
                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will get the assigned or default sales team
        /// </summary>
        /// <param name="rSys"></param>
        /// <returns></returns>
        public object FindSalesTeamByEmployee(IRSystem7 rSys, object employeeID)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                object vntResult;
                StringBuilder sqlText = new StringBuilder();


                //Build custom sql to find sales team ID record
                sqlText.Append("SELECT ");
                sqlText.Append("    MI_Sales_Team_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("    MI_Sales_Team_Member ");
                sqlText.Append("WHERE ");
                sqlText.Append("    MI_Active = 1 ");
                sqlText.Append("AND MI_Team_Member_Id = " + rSys.IdToString(employeeID));
                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    rst.MoveFirst();
                    vntResult = rst.Fields[0].Value;

                }
                else
                {
                    vntResult = DBNull.Value;
                }
                rst.Close();

                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will get the assigned or default sales team
        /// </summary>
        /// <param name="rSys"></param>
        /// <returns></returns>
        public object FindDefaultEmployee(IRSystem7 rSys, object division)
        {
            try
            {

                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                StringBuilder sqlText = new StringBuilder();
                object vntResult = null;


                sqlText.Append("SELECT ");
                sqlText.Append("   e.Employee_Id ");
                sqlText.Append("FROM ");
                sqlText.Append("   Employee e ");
                sqlText.Append("WHERE ");
                sqlText.Append("   e.Last_Name like '%Conversion%' ");
                sqlText.Append(" AND ");
                sqlText.Append("Division_Id = " + rSys.IdToString(division));

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    rst.MoveFirst();
                    //Grab the Id and the Pipeline stage
                    vntResult = rst.Fields[0].Value;

                }
                else
                {
                    //Cannot find the Opportunity...
                    //Raise an error in calling function
                    vntResult = DBNull.Value;
                }
                rst.Close();

                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will be used to retrieve the Pivotal Division Id from the 
        /// associated Neighorhood record.
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        public object FindDivision(IRSystem7 rSys, Recordset rstPrimary)
        {

            try
            {

                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                object vntResult;
                StringBuilder sqlText = new StringBuilder();


                //Build custom sql to find lot__comopany record
                sqlText.Append("SELECT ");
                sqlText.Append("    n.Division_Id ");
                sqlText.Append("From ");
                sqlText.Append("    Neighborhood n ");
                sqlText.Append("Where ");
                sqlText.Append("    n.External_Source_Community_Id = '" + rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_COMMUNITY_ID].Value + "'");

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    //Found it
                    rst.MoveFirst();
                    vntResult = rst.Fields[0].Value;
                }
                else
                {
                    vntResult = DBNull.Value;
                }

                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        /// <summary>
        /// This method will be used to retrieve the Pivotal Division Id from the 
        /// associated Neighorhood record.
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        public object FindDivisionByCommunity(IRSystem7 rSys, string communityId)
        {

            try
            {

                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                object vntResult;
                StringBuilder sqlText = new StringBuilder();


                //Build custom sql to find lot__comopany record
                sqlText.Append("SELECT ");
                sqlText.Append("    n.Division_Id ");
                sqlText.Append("From ");
                sqlText.Append("    Neighborhood n ");
                sqlText.Append("Where ");
                sqlText.Append("    n.External_Source_Community_Id = '" + communityId + "'");

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    //Found it
                    rst.MoveFirst();
                    vntResult = rst.Fields[0].Value;
                }
                else
                {
                    vntResult = DBNull.Value;
                }

                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        /// <summary>
        /// This method will be used to retrieve the Pivotal Division Id from the 
        /// associated Neighorhood record.
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        public object FindDivisionByArea(IRSystem7 rSys, string area)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                object vntResult;
                StringBuilder sqlText = new StringBuilder();

                //Build custom sql to find lot__comopany record
                sqlText.Append("SELECT ");
                sqlText.Append("    d.Division_Id ");
                sqlText.Append("From ");
                sqlText.Append("    Division d ");
                sqlText.Append("Where ");
                sqlText.Append("    d.Division_Number = '" + area + "'");

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    //Found it
                    rst.MoveFirst();
                    vntResult = rst.Fields[0].Value;
                }
                else
                {
                    vntResult = DBNull.Value;
                }

                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        // ASM: Commented this out, as it's name clashes with my new FindDivisionProduct method, but we may need this again later.
        // ASM: There were already-commented-out portions of code in here, which I had to delete.  If I reinstate the method, consider these.
        /*
        /// <summary>
        /// This method is used by the Option touchpoint to retrieve the DIvisio nProduct Id,
        /// if the method cannot find the DIvisionProduct Id it will create one and return it.
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        public object FindDivisionProduct(IRSystem7 rSys, Recordset rstPrimary, bool release)
        {
            try
            {
                object vntDivProductId = null;
                string strDivProdExtId = string.Empty;
                string[] arrFieldNames = null;
                string strErrMsg = string.Empty;

                arrFieldNames = new string[] 
                {
                    IntegrationConstants.strfEXTERNAL_SOURCE_ID, IntegrationConstants.strfEXTERNAL_SOURCE_COMMUNITY_ID,
                    IntegrationConstants.strfDIVISION_ID, IntegrationConstants.strfCATEGORY_ID, IntegrationConstants.strfCONSTRUCTION_STAGE_ID,
                    IntegrationConstants.strfDESCRIPTION, IntegrationConstants.strfEXTERNAL_SOURCE_NAME, IntegrationConstants.gstrfINACTIVE,
                    IntegrationConstants.strfREQUIRED_DEPOSIT_AMT, IntegrationConstants.gstrfTYPE, IntegrationConstants.strfPRODUCT_NAME,
                    IntegrationConstants.strfCODE_, IntegrationConstants.strfEXTERNAL_SOURCE_ELEV_CODE, IntegrationConstants.strfEXTERNAL_SOURCE_PHASE_CODE,
                    IntegrationConstants.strfEXTERNAL_SOURCE_PLAN_CODE, IntegrationConstants.strfREGION_ID
                };

                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                //Lookup Division Product by external_Source_Id
                //12-10-2007 AB Division product lookup is MI specific. There is not a 1 to 1 relationship between division options and product configurations


                strDivProdExtId = rstPrimary.Fields[IntegrationConstants.strfOPTION_AREAID].Value + "-" +
                    rstPrimary.Fields[IntegrationConstants.strfCODE_].Value;
                vntDivProductId = rSys.Tables[IntegrationConstants.strtDIVISION_PRODUCT].Fields[IntegrationConstants.strfDIVISION_PRODUCT_ID]
                .FindValue(rSys.Tables[IntegrationConstants.strtDIVISION_PRODUCT].Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID],
                strDivProdExtId);

                return vntDivProductId;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        */

        // ASM: Commented this out, as it's name clashes with my new FindNbhdPhase method, but we may need this again later.
        /*
        /// <summary>
        /// This method is used to lookup 
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="externalId"></param>
        /// <returns></returns>
        public object FindNBHDPhase(IRSystem7 rSys, string externalId)
        {
            try
            {
                object vntResult = null;

                vntResult = rSys.Tables[IntegrationConstants.gstrtNBHD_PHASE].Fields[IntegrationConstants.gstrfNBHD_PHASE_ID]
                .FindValue(rSys.Tables[IntegrationConstants.gstrtNBHD_PHASE].Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID],
                externalId);

                if (DBNull.Value.Equals(vntResult))
                {
                    vntResult = DBNull.Value;
                }
                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        */

        /// <summary>
        /// This method is used to lookup support categories
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public object FindSupportCategory(IRSystem7 rSys, string category)
        {
            try
            {
                object vntResult = null;

                vntResult = rSys.Tables[IntegrationConstants.strtSUPPORT_CATEGORY].Fields[IntegrationConstants.strfSUPPORT_CATEGORY_ID]
                .FindValue(rSys.Tables[IntegrationConstants.strtSUPPORT_CATEGORY].Fields[IntegrationConstants.strfSUPPORT_CATEGORY_NAME],
                category);

                if (DBNull.Value.Equals(vntResult))
                {
                    vntResult = DBNull.Value;
                }
                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method is used to default the env password 
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public string CreateEnvisionPassword(IRSystem7 rSys, object contactId)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                string strResult;
                StringBuilder sqlText = new StringBuilder();
                string strLast = "";
                string strID = "";

                //Build custom sql to find lot__comopany record
                sqlText.Append("SELECT ");
                sqlText.Append("    c.First_Name, c.Last_Name, c.MI_CFT_ID ");
                sqlText.Append("From ");
                sqlText.Append("    Contact c ");
                sqlText.Append("Where ");
                sqlText.Append("    c.Contact_Id = " + rSys.IdToString(contactId));

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    //Found it
                    rst.MoveFirst();
                    if (rst.Fields[1].Value.ToString().Length > 5)
                    {
                        strLast = rst.Fields[1].Value.ToString().Substring(0, 5);
                    }
                    else
                    {
                        strLast = rst.Fields[1].Value.ToString();
                    }

                    if (rst.Fields[2].Value.ToString().Length > 4)
                    {
                        strID = rst.Fields[2].Value.ToString().Substring(0, 4);
                    }
                    else
                    {
                        strID = rst.Fields[2].Value.ToString();
                    }

                    strResult = rst.Fields[0].Value.ToString().Substring(0, 1) + strLast + strID;
                }
                else
                {
                    strResult = "";
                }

                return strResult;


            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will build the description for a new activity
        /// </summary>
        /// <param name="rSys"></param>
        /// <returns></returns>
        public string BuildActivityDescription(IRSystem7 rSys, string type, object contact, object neighborhood)
        {
            try
            {
                string strResult = "";
                string contactDesc = "";
                string strNeighborhood = "";

                if (!(neighborhood == DBNull.Value))
                {
                    strNeighborhood = (string)rSys.Tables[IntegrationConstants.gstrtNEIGHBORHOOD].Fields[IntegrationConstants.strfNAME]
                    .FindValue(rSys.Tables[IntegrationConstants.gstrtNEIGHBORHOOD].Fields[IntegrationConstants.strfNEIGHBORHOOD_ID],
                    neighborhood);
                }

                contactDesc = (string)rSys.Tables[IntegrationConstants.strtCONTACT].Fields["Rn_Descriptor"]
                .FindValue(rSys.Tables[IntegrationConstants.strtCONTACT].Fields[IntegrationConstants.strfCONTACT_ID],
                contact);

                if (type == "Visit Log")
                {
                    strResult = "Visit Log: " + contactDesc + " at " + strNeighborhood;
                }
                else if (type == "Meeting")
                {
                    strResult = "Meeting " + contactDesc;
                }
                else
                {
                    strResult = "Call " + contactDesc;
                }

                return strResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will build the description for a new activity
        /// </summary>
        /// <param name="rSys"></param>
        /// <returns></returns>
        public string BuildLeadActivityDescription(IRSystem7 rSys, string type, object lead, object neighborhood)
        {
            try
            {
                string strResult = "";
                string leadDesc = "";
                string strNeighborhood = "";

                if (!(neighborhood == DBNull.Value))
                {
                    strNeighborhood = (string)rSys.Tables[IntegrationConstants.gstrtNEIGHBORHOOD].Fields[IntegrationConstants.strfNAME]
                    .FindValue(rSys.Tables[IntegrationConstants.gstrtNEIGHBORHOOD].Fields[IntegrationConstants.strfNEIGHBORHOOD_ID],
                    neighborhood);
                }

                leadDesc = (string)rSys.Tables[IntegrationConstants.strtLEAD].Fields["Rn_Descriptor"]
                .FindValue(rSys.Tables[IntegrationConstants.strtLEAD].Fields["Lead__Id"],
                lead);

                if (type == "Visit Log")
                {
                    strResult = "Visit Log: " + leadDesc + " at " + strNeighborhood;
                }
                else if (type == "Meeting")
                {
                    strResult = "Meeting " + leadDesc;
                }
                else
                {
                    strResult = "Call " + leadDesc;
                }

                return strResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will return marketing project
        /// </summary>
        /// <param name="rSys"></param>
        /// <returns></returns>
        public object FindMarketingProject(IRSystem7 rSys, string project)
        {
            try
            {
                object vntResult;
                string strErrMsg = string.Empty;

                vntResult = rSys.Tables[IntegrationConstants.strtMARKETING_PROJECT].Fields[IntegrationConstants.strfMARKETING_PROJECT_ID]
                .FindValue(rSys.Tables[IntegrationConstants.strtMARKETING_PROJECT].Fields[IntegrationConstants.strfMARKETING_PROJECT_NAME],
                project);

                if (DBNull.Value.Equals(vntResult))
                {
                    vntResult = DBNull.Value;
                }
                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will return the contact profile for a specific prospect
        /// </summary>
        /// <param name="rSys"></param>
        /// <returns></returns>
        public object FindContactProfile(IRSystem7 rSys, object contact)
        {
            try
            {
                object vntResult;
                string strErrMsg = string.Empty;

                vntResult = rSys.Tables[IntegrationConstants.gstrtCONTACT_PROFILE_NEIGHB].Fields[IntegrationConstants.strfCONTACT_PROFILE_NBHD_ID]
                .FindValue(rSys.Tables[IntegrationConstants.gstrtCONTACT_PROFILE_NEIGHB].Fields[IntegrationConstants.strfCONTACT_ID],
                contact);

                if (DBNull.Value.Equals(vntResult))
                {
                    vntResult = DBNull.Value;
                }

                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        // TODO: Test this method as I made some serious improvements/changes
        /// <summary>
        /// This method will find a Category based on description. If updateDesc is set to false the description 
        /// will not be updated and the new cat will not be inserted if not found.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="categoryDesc"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object FindCategory(IRSystem7 rSys, string strCategoryCode, string strCategoryDescription, 
                                   object strType, bool blnUpdateDescription)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstCategory = null;
                Recordset rstExistingCategory = null;

                string strErrMsg = String.Empty;                
                object vntConfigurationTypeId = null;
                
                // Look it up
                vntConfigurationTypeId = rSys.Tables[IntegrationConstants.strtCONFIG_TYPE].Fields[IntegrationConstants.strfCONFIG_TYPE_ID]
                    .FindValue(rSys.Tables[IntegrationConstants.strtCONFIG_TYPE].Fields[IntegrationConstants.strfCODE_],
                    strCategoryCode);

                if (blnUpdateDescription)
                {
                    // TODO: Test this, as I am not sure that the FindValue call below will return DBNull.Value, but maybe Null instead?
                    if ((Convert.IsDBNull(vntConfigurationTypeId)) || (vntConfigurationTypeId == null))
                    {
                        // Configuration_Type record with supplied Code_ does not exist - create one
                        rstCategory = objLib.GetNewRecordset(IntegrationConstants.strtCONFIG_TYPE, 
                                                             new object[] { IntegrationConstants.strfCONFIG_TYPE_ID, 
                                                                            IntegrationConstants.strfCODE_, 
                                                                            IntegrationConstants.strfCONFIG_TYPE_NAME, 
                                                                            IntegrationConstants.gstrfINACTIVE, 
                                                                            IntegrationConstants.gstrfCOMPONENT });

                        rstCategory.AddNew(Type.Missing, Type.Missing);
                        rstCategory.Fields[IntegrationConstants.gstrfINACTIVE].Value = false;
                        rstCategory.Fields[IntegrationConstants.strfCODE_].Value = strCategoryCode;
                        rstCategory.Fields[IntegrationConstants.strfCONFIG_TYPE_NAME].Value = strCategoryDescription;
                        rstCategory.Fields[IntegrationConstants.gstrfCOMPONENT].Value = strType;
                        // Save the record
                        objLib.SaveRecordset(IntegrationConstants.strtCONFIG_TYPE, rstCategory);
                        // TODO: Test this works, as this wasn't in the initial MI code, and I added this to be better code
                        // Get the Record Id of the new Configuration_Type record
                        vntConfigurationTypeId = rstCategory.Fields[IntegrationConstants.strfCONFIG_TYPE_ID].Value;
                        // Close the recordset
                        rstCategory.Close();

                        // TODO: Delete this commented code if the above line returns the new Record Id
                        /*
                        //Look it up again
                        vntConfigurationTypeId = rSys.Tables[IntegrationConstants.strtCONFIG_TYPE].Fields[IntegrationConstants.strfCONFIG_TYPE_ID]
                            .FindValue(rSys.Tables[IntegrationConstants.strtCONFIG_TYPE].Fields[IntegrationConstants.strfCODE_],
                            strCategoryCode);
                        */
                    }
                    else
                    {
                        // Configuration_Type record with supplied Code_ DOES exist - update the Description if instructed to do so

                        // TODO: Test this TypeConvert works if the current description in the database is null - I changed this code from the MI version.
                        string strCurrentDescription = TypeConvert.ToString(
                            rSys.Tables[IntegrationConstants.strtCONFIG_TYPE].Fields[IntegrationConstants.strfCONFIG_TYPE_NAME]
                            .FindValue(rSys.Tables[IntegrationConstants.strtCONFIG_TYPE].Fields[IntegrationConstants.strfCONFIG_TYPE_ID],
                            vntConfigurationTypeId));

                        if (strCurrentDescription != strCategoryDescription)
                        {
                            //Get the recordset and update it
                            rstExistingCategory = objLib.GetRecordset(vntConfigurationTypeId, 
                                                                      IntegrationConstants.strtCONFIG_TYPE,
                                                                      new object[] { IntegrationConstants.strfCONFIG_TYPE_NAME });

                            rstExistingCategory.Fields[IntegrationConstants.strfCONFIG_TYPE_NAME].Value = strCategoryDescription;
                            // Save the updated record
                            objLib.SaveRecordset(IntegrationConstants.strtCONFIG_TYPE, rstExistingCategory);
                            // Cleanup
                            rstExistingCategory.Close();
                        }
                    }
                }

                return vntConfigurationTypeId;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        // amcnab 2010-07-28: Commented out.  Code was not being called by Lot integration.  Compiler was generating warnings, so commented-out.
        /*
        /// <summary>
        /// This method will find a Category based on description
        /// </summary>
        /// <param name="category"></param>
        /// <param name="categoryDesc"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object FindCategoryByDivision(IRSystem7 rSys, string category, string categoryDesc, object type, object vntDivId)
        {
            try
            {

                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                object vntResult;
                StringBuilder sqlText = new StringBuilder();


                //Build custom sql to find lot__comopany record
                sqlText.Append("SELECT ");
                sqlText.Append("    ct.Configuration_Type_Id ");
                sqlText.Append("From ");
                sqlText.Append("    Configuration_Type ct ");
                sqlText.Append("Where ");
                sqlText.Append("   ct.Code_ = '" + category + "'");
                sqlText.Append(" AND ");
                sqlText.Append("ct.Division_Id = " + rSys.IdToString(vntDivId));

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    //Found it
                    rst.MoveFirst();
                    vntResult = rst.Fields[0].Value;
                }
                else
                {
                    vntResult = DBNull.Value;
                }


                if (DBNull.Value.Equals(vntResult))
                {
                    //Create it
                    Recordset rstCategory = null;

                    rstCategory = objLib.GetNewRecordset(IntegrationConstants.strtCONFIG_TYPE,
                        new object[] { IntegrationConstants.strfCODE_, IntegrationConstants.strfCONFIG_TYPE_NAME,
                                       IntegrationConstants.gstrfINACTIVE, IntegrationConstants.gstrfCOMPONENT,
                                       IntegrationConstants.strfDIVISION_ID});

                    rstCategory.AddNew(Type.Missing, Type.Missing);
                    rstCategory.Fields[IntegrationConstants.gstrfINACTIVE].Value = false;
                    rstCategory.Fields[IntegrationConstants.strfCODE_].Value = category;
                    rstCategory.Fields[IntegrationConstants.strfCONFIG_TYPE_NAME].Value = categoryDesc;
                    rstCategory.Fields[IntegrationConstants.gstrfCOMPONENT].Value = type;
                    rstCategory.Fields[IntegrationConstants.strfDIVISION_ID].Value = vntDivId;
                    objLib.SaveRecordset(IntegrationConstants.strtCONFIG_TYPE, rstCategory);
                    rstCategory.Close();

                    //Lookit up again
                    rst = objLib.GetRecordset(sqlText.ToString());
                    vntResult = rst.Fields[0].Value;

                }
                //Else update the description if it has changed
                else
                {
                    string strCurrentDesc = "";

                    object currentDesc = rSys.Tables[IntegrationConstants.strtCONFIG_TYPE].Fields[IntegrationConstants.strfCONFIG_TYPE_NAME]
                                .FindValue(rSys.Tables[IntegrationConstants.strtCONFIG_TYPE].Fields[IntegrationConstants.strfCONFIG_TYPE_ID],
                                 vntResult);

                    if (currentDesc != DBNull.Value && currentDesc != null)
                    {
                        strCurrentDesc = (string)currentDesc;
                    }


                    if (currentDesc != categoryDesc)
                    {
                        //Get the recordset and update it
                        Recordset rstExistingCategory = null;
                        rstExistingCategory = objLib.GetRecordset(vntResult, IntegrationConstants.strtCONFIG_TYPE,
                                               new object[] { IntegrationConstants.strfCODE_, IntegrationConstants.strfCONFIG_TYPE_NAME });

                        rstExistingCategory.Fields[IntegrationConstants.strfCONFIG_TYPE_NAME].Value = categoryDesc;
                        objLib.SaveRecordset(IntegrationConstants.strtCONFIG_TYPE, rstExistingCategory);
                        rstExistingCategory.Close();
                    }
                }
                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntConfigurationTypeId"></param>
        /// <param name="strSubCategoryImportMatchKey"></param>
        /// <param name="strSubCategoryName"></param>
        /// <param name="blnUpdateDescription"></param>
        /// <returns></returns>
        public object FindSubCategory(IRSystem7 rSys, object vntConfigurationTypeId,
                                      string strSubCategoryImportMatchKey, string strSubCategoryName,
                                      bool blnUpdateDescription)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                object vntSubCategoryId;
                StringBuilder sqlText = new StringBuilder();

                rst = objLib.GetRecordset("TIC_INT : Sub Category : Match Key ? and Config Type ?", 2, strSubCategoryImportMatchKey, vntConfigurationTypeId,
                    "Sub_Category_Id");

                ////Build custom sql to find lot__comopany record
                //sqlText.Append("SELECT ");
                //sqlText.Append("    sc.Sub_Category_Id ");
                //sqlText.Append("FROM ");
                //sqlText.Append("    Sub_Category sc ");
                //sqlText.Append("WHERE ");
                //sqlText.Append("    sc.Import_Match_Key = '" + strSubCategoryImportMatchKey + "'");
                //sqlText.Append(" AND sc.Configuration_Type_Id = " + rSys.IdToString(vntConfigurationTypeId));

                //rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    //Found it
                    rst.MoveFirst();
                    vntSubCategoryId = rst.Fields["Sub_Category_Id"].Value;
                }
                else
                {
                    vntSubCategoryId = DBNull.Value;
                }

                if (blnUpdateDescription)
                {
                    // TODO: Test this, as I am not sure that the FindValue call below will return DBNull.Value, but maybe Null instead?
                    if ((Convert.IsDBNull(vntSubCategoryId)) || (vntSubCategoryId == null))
                    {
                        // Sub_Category record with supplied Category_Id and Name does not exist - create one
                        Recordset rstSubCategory = new Recordset();
                        rstSubCategory = objLib.GetNewRecordset(IntegrationConstants.strtSUB_CATEGORY,
                                                                new object[] { 
                                                                    IntegrationConstants.strfSUB_CATEGORY_ID, 
                                                                    IntegrationConstants.strfCONFIGURATION_TYPE_ID,
                                                                    IntegrationConstants.strfSUBCATEGORY_NAME,                                                                     
                                                                    IntegrationConstants.strfIMPORT_MATCH_KEY });

                        rstSubCategory.AddNew(Type.Missing, Type.Missing);
                        rstSubCategory.Fields[IntegrationConstants.strfCONFIGURATION_TYPE_ID].Value = vntConfigurationTypeId;
                        rstSubCategory.Fields[IntegrationConstants.strfIMPORT_MATCH_KEY].Value = strSubCategoryImportMatchKey;
                        rstSubCategory.Fields[IntegrationConstants.strfSUBCATEGORY_NAME].Value = strSubCategoryName;
                        // Save the record
                        objLib.SaveRecordset(IntegrationConstants.strtSUB_CATEGORY, rstSubCategory);
                        // TODO: Test this works, as this wasn't in the initial MI code, and I added this to be better code
                        // Get the Record Id of the new Sub_Category record
                        vntSubCategoryId = rstSubCategory.Fields[IntegrationConstants.strfSUB_CATEGORY_ID].Value;
                        // Close the recordset
                        rstSubCategory.Close();
                    }
                    else
                    {
                        // Sub_Category record with supplied Category_Id and Name DOES exist - update its description

                        // TODO: Test this TypeConvert works if the current name in the database is null - I changed this code from the MI version.
                        string strCurrentName = TypeConvert.ToString(
                            rSys.Tables[IntegrationConstants.strtSUB_CATEGORY].Fields[IntegrationConstants.strfSUBCATEGORY_NAME]
                            .FindValue(rSys.Tables[IntegrationConstants.strtSUB_CATEGORY].Fields[IntegrationConstants.strfSUB_CATEGORY_ID],
                            vntSubCategoryId));

                        if (strCurrentName != strSubCategoryName)
                        {
                            //Get the recordset and update it
                            Recordset rstExistingCategory = null;
                            rstExistingCategory = objLib.GetRecordset(vntSubCategoryId,
                                                                      IntegrationConstants.strtSUB_CATEGORY,
                                                                      new object[] { IntegrationConstants.strfSUBCATEGORY_NAME });

                            rstExistingCategory.Fields[IntegrationConstants.strfSUBCATEGORY_NAME].Value = strSubCategoryName;
                            // Save the updated record
                            objLib.SaveRecordset(IntegrationConstants.strtSUB_CATEGORY, rstExistingCategory);
                            // Cleanup
                            rstExistingCategory.Close();
                        }
                    }
                }

                return vntSubCategoryId;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        public object FindSupportSubject(IRSystem7 rSys, object category, string subcategory)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                object vntResult;
                StringBuilder sqlText = new StringBuilder();


                //Build custom sql to find subcategory (aka subject) record
                sqlText.Append("SELECT ");
                sqlText.Append("    sc.Support_Subject_Id ");
                sqlText.Append("From ");
                sqlText.Append("    Support_Subject sc ");
                sqlText.Append("Where ");
                sqlText.Append("   sc.Support_Subject_Name = '" + subcategory + "'");
                sqlText.Append(" AND ");
                sqlText.Append("sc.Category_Id = " + rSys.IdToString(category));

                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    //Found it
                    rst.MoveFirst();
                    vntResult = rst.Fields[0].Value;
                }
                else
                {
                    vntResult = DBNull.Value;
                }

                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Return all PCH records where Processed <> 1 and are related to the supplied Lot or NBHDP_Product, 
        /// ordered by Change_Timestamp descending
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntOptionId"></param>
        /// <param name="isLot"></param>
        /// <returns></returns>
        public Recordset FindPriceChangeHistory(IRSystem7 rSys, object vntRecordId, Boolean isLot)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                StringBuilder sqlText = new StringBuilder();

                if (isLot)
                {
                    // Sorting will on work properly on recordset to build query manually                    
                    // Return all PCH records where Processed <> 1 and are related to the supplied Lot, ordered by Change_Timestamp DESC
                    sqlText.Append("SELECT ");
                    sqlText.Append("    Price, Change_Date, Change_TimeStamp ");
                    sqlText.Append("FROM ");
                    sqlText.Append("    Price_Change_History pc ");
                    sqlText.Append("WHERE ");
                    sqlText.Append("   (Processed = 0 OR Processed IS NULL) ");
                    sqlText.Append(" AND ");
                    sqlText.Append("pc.Lot_Id = " + rSys.IdToString(vntRecordId));
                    sqlText.Append(" ORDER BY Change_Timestamp DESC ");
                    // Get the recordset
                    rst = objLib.GetRecordset(sqlText.ToString());
                }
                else
                {
                    //rst = objLib.GetRecordset("MI: Price History for Option not Processed", 1, vntOptionId, "Change_Date", "Price");
                    sqlText.Append("SELECT ");
                    sqlText.Append("    Price, Change_Date, Change_TimeStamp ");
                    sqlText.Append("FROM ");
                    sqlText.Append("    Price_Change_History pc ");
                    sqlText.Append("WHERE ");
                    sqlText.Append("   (Processed = 0 OR Processed IS NULL) ");
                    sqlText.Append(" AND ");
                    sqlText.Append("pc.NBHDP_Product_Id = " + rSys.IdToString(vntRecordId));
                    sqlText.Append(" ORDER BY Change_Timestamp DESC ");
                    // Get the recordset
                    rst = objLib.GetRecordset(sqlText.ToString());
                }

                // Return the Recordset of Unprocessed PCH records for the Lot
                return rst;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        public object FindSupportTopic(IRSystem7 rSys, object subject, string topic)
        {
            try
            {
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rst = new Recordset();
                object vntResult;
                StringBuilder sqlText = new StringBuilder();


                //Build custom sql to find subcategory (aka subject) record
                sqlText.Append("SELECT ");
                sqlText.Append("    sc.Support_Topic_Id ");
                sqlText.Append("From ");
                sqlText.Append("    Support_Topic sc ");
                sqlText.Append("Where ");
                sqlText.Append("   sc.Support_Topic_Name = '" + topic + "'");
                sqlText.Append(" AND ");
                sqlText.Append("sc.Subject_Id = " + rSys.IdToString(subject));


                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    //Found it
                    rst.MoveFirst();
                    vntResult = rst.Fields[0].Value;
                }
                else
                {
                    vntResult = DBNull.Value;
                }

                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method is used to Find Construction Stage by doing a lookup on the 
        /// stageNot After value
        /// </summary>
        /// <param name="stageNotAfter"></param>
        /// <returns></returns>
        public object FindConstructionStage(IRSystem7 rSys, string stageNotAfter, string description)
        {
            try
            {

                string strErrMsg = string.Empty;
                Recordset rstStage = null;
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                object vntResult = null;

                vntResult = rSys.Tables[IntegrationConstants.strtCONSTRUCTION_STAGE].Fields[IntegrationConstants.strfCONSTRUCTION_STAGE_ID]
                .FindValue(rSys.Tables[IntegrationConstants.strtCONSTRUCTION_STAGE].Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID],
                stageNotAfter);

                if (DBNull.Value.Equals(vntResult))
                {
                    //Create it
                    rstStage = objLib.GetNewRecordset(IntegrationConstants.strtCONSTRUCTION_STAGE,
                        new object[] { IntegrationConstants.strfCONSTRUCTION_STAGE_ORDINAL, IntegrationConstants.strfCONSTRUCTION_STAGE_NAME,
                                       IntegrationConstants.gstrfINACTIVE, IntegrationConstants.strfEXTERNAL_SOURCE_ID, IntegrationConstants.strfCORPORATE});

                    rstStage.AddNew(Type.Missing, Type.Missing);
                    rstStage.Fields[IntegrationConstants.gstrfINACTIVE].Value = false;
                    rstStage.Fields[IntegrationConstants.strfCONSTRUCTION_STAGE_ORDINAL].Value = stageNotAfter;
                    rstStage.Fields[IntegrationConstants.strfCORPORATE].Value = true;
                    rstStage.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID].Value = stageNotAfter;
                    rstStage.Fields[IntegrationConstants.strfCONSTRUCTION_STAGE_NAME].Value = description;
                    objLib.SaveRecordset(IntegrationConstants.strtCONSTRUCTION_STAGE, rstStage);
                    rstStage.Close();

                    //Lookit up again
                    vntResult = rSys.Tables[IntegrationConstants.strtCONSTRUCTION_STAGE].Fields[IntegrationConstants.strfCONSTRUCTION_STAGE_ID]
                                    .FindValue(rSys.Tables[IntegrationConstants.strtCONSTRUCTION_STAGE].Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID],
                                        stageNotAfter);

                }

                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method is used to Fbuild the plan lookup value
        /// 
        /// </summary>
        /// <param name="stageNotAfter"></param>
        /// <returns></returns>
        public string BuildPlanCode(object nbhd, object phase, object plan, object elev)
        {
            string strPlanCode = string.Empty;
            string strNeighborhood_Id = string.Empty;
            string strNBHD_Phase_Id = string.Empty;
            string strNBHD_Phase_Code = string.Empty;
            string strPlanId = string.Empty;
            string strElevId = string.Empty;

            try
            {

                if (DBNull.Value == nbhd)
                {
                    strNeighborhood_Id = "";
                }
                else
                {
                    strNeighborhood_Id = nbhd.ToString();
                }

                //2. Set NBHD Phase String
                if (DBNull.Value == phase)
                {
                    strNBHD_Phase_Code = "";
                }
                else
                {
                    strNBHD_Phase_Code = phase.ToString();
                }
                //Check for Plan Id
                if (DBNull.Value == plan)
                {
                    strPlanCode = "";
                }
                else
                {
                    strPlanCode = plan.ToString();
                }
                if (DBNull.Value == elev)
                {
                    strElevId = "";
                }
                else
                {
                    strElevId = elev.ToString();
                }

                strPlanId = strNeighborhood_Id + "-" + strNBHD_Phase_Code + "-" + strPlanCode + "-" + strElevId;

                return strPlanId;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e);
            }
        }


        /// <summary>
        /// This method will create a new Contingency record for a new Lot.
        /// This logic is specific to MI Homes
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="contCode"></param>
        /// <param name="vntOppId"></param>
        public void CreateContigency(IRSystem7 rSys, string contCode, object vntLotId)
        {
            try
            {
                if (contCode == null || contCode == "")
                {
                    //at this point do nothing
                }
                else
                {
                    DataAccess objLib =
                        (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                        .CreateInstance();

                    //Get record and set point to allow fields to be updated
                    object vntContractId = this.FindContractByLot(rSys, vntLotId);

                    if (null == vntContractId)
                    {
                        throw new PivotalApplicationException("Contract must exist in order to create Contingency record.");
                    }
                    object[] arrFields = new object[] { IntegrationConstants.strfOPPORTUNITY_ID, "Reason" };

                    Recordset rstContigency = objLib.GetNewRecordset("Contingency", arrFields);


                    //Force new record to be created
                    rstContigency.AddNew(Type.Missing, Type.Missing);
                    rstContigency.Fields[IntegrationConstants.strfOPPORTUNITY_ID].Value = vntContractId;
                    //AB 2008-06-20 MUST HARD CODE FOR MI
                    //rstContigency.Fields["Reason"].Value = contCode.ToString();
                    switch (contCode.ToString())
                    {
                        case "ATTY":
                            rstContigency.Fields["Reason"].Value = "Cont Upon Attorney Approval:ATTY";
                            break;
                        case "CCT":
                            rstContigency.Fields["Reason"].Value = "Contingent Contract:CCT";
                            break;
                        case "CLEARED":
                            rstContigency.Fields["Reason"].Value = "All Contingencies Cleared:CLEARED";
                            break;
                        case "GBO":
                            rstContigency.Fields["Reason"].Value = "Guarantee Buy Out Date:GBO";
                            break;
                        case "HTC":
                            rstContigency.Fields["Reason"].Value = "House to Close:HTC";
                            break;
                        case "HTS":
                            rstContigency.Fields["Reason"].Value = "House to Sell:HTS";
                            break;
                        case "JOB":
                            rstContigency.Fields["Reason"].Value = "Purchaser Must Acquire Job:JOB";
                            break;
                        case "MISC":
                            rstContigency.Fields["Reason"].Value = "Miscellaneous:MISC";
                            break;
                        case "RELEASED":
                            rstContigency.Fields["Reason"].Value = "Contingency Release:RELEASED";
                            break;
                        case "SBI":
                            rstContigency.Fields["Reason"].Value = "Sell By Insulation:SBI";
                            break;
                        case "WAIVER":
                            rstContigency.Fields["Reason"].Value = "Contingency Waiver:WAIVER";
                            break;
                        default:
                            rstContigency.Fields["Reason"].Value = "House to Sell:HTS";
                            break;


                    }

                    objLib.SaveRecordset("Contingency", rstContigency);
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        public void SetLotContact(IRSystem7 rSys, IRForm pForm, Recordset rstPrimary, object vntContactId)
        {
            try
            {
                DataAccess objLib =
                    (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                    .CreateInstance();

                //Get record and set point to allow fields to be updated
                object vntLotId = this.FindLot(rSys, rstPrimary.Fields[IntegrationConstants.strfCONTACT_LOT_ID].Value.ToString());
                object vntContractId = this.FindContractByLot(rSys, vntLotId);


                if (null == vntContractId)
                {
                    throw new PivotalApplicationException("Contract must exist in order to create the co-buyer record.");
                }

                if (null == vntLotId)
                {
                    throw new PivotalApplicationException("Lot must exist in order to create the co-buyer record.");
                }

                object[] arrFields = new object[] { IntegrationConstants.strfPRODUCT_ID, IntegrationConstants.strfCONTACT_ID, IntegrationConstants.strfLOT_CONTACT_TYPE };

                Recordset rstLotContact = objLib.GetNewRecordset(IntegrationConstants.strtLOT__CONTACT, arrFields);


                //Force new record to be created
                rstLotContact.AddNew(Type.Missing, Type.Missing);
                rstLotContact.Fields[IntegrationConstants.strfPRODUCT_ID].Value = vntLotId;
                rstLotContact.Fields[IntegrationConstants.strfCONTACT_ID].Value = vntContactId;
                rstLotContact.Fields[IntegrationConstants.strfLOT_CONTACT_TYPE].Value = 4;
                objLib.SaveRecordset(IntegrationConstants.strtLOT__CONTACT, rstLotContact);

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        public void SetContactCobuyer(IRSystem7 rSys, IRForm pForm, Recordset rstPrimary, object vntContactId, string strfCFTId)
        {
            try
            {
                DataAccess objLib =
                    (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                    .CreateInstance();

                object vntResult = null;

                if (strfCFTId == null || strfCFTId == "")
                {
                    //Get record and set point to allow fields to be updated
                    object vntLotId = this.FindLot(rSys, rstPrimary.Fields[IntegrationConstants.strfCONTACT_LOT_ID].Value.ToString());
                    object vntContractId = this.FindContractByLot(rSys, vntLotId);

                    vntResult = rSys.Tables[IntegrationConstants.strtOPPORTUNITY].Fields[IntegrationConstants.strfCONTACT_ID].FindValue(
                    rSys.Tables[IntegrationConstants.strtOPPORTUNITY].Fields[IntegrationConstants.strfOPPORTUNITY_ID],
                    vntContractId);

                    if (null == vntContractId)
                    {
                        throw new PivotalApplicationException("Contract must exist in order to create the co-buyer record.");
                    }
                }
                else
                {
                    vntResult = this.FindContactByCFT(rSys, strfCFTId);
                }

                object[] arrFields = new object[] { IntegrationConstants.strfCO_BUYER_CONTACT_ID, IntegrationConstants.strfCONTACT_ID };

                Recordset rstCobuyerContact = objLib.GetNewRecordset(IntegrationConstants.strtCOBUYER_CONTACT, arrFields);


                //Force new record to be created
                rstCobuyerContact.AddNew(Type.Missing, Type.Missing);
                rstCobuyerContact.Fields[IntegrationConstants.strfCO_BUYER_CONTACT_ID].Value = vntContactId;
                rstCobuyerContact.Fields[IntegrationConstants.strfCONTACT_ID].Value = vntResult;
                objLib.SaveRecordset(IntegrationConstants.strtCOBUYER_CONTACT, rstCobuyerContact);

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        /// <summary>
        /// This method will create a new realtor record for a new sale
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntAgent"></param>
        /// <param name="vntAgency"></param>
        /// <param name="vntLotId"></param>
        public void CreateContractRealtor(IRSystem7 rSys, object vntAgent, object vntAgency, object vntLotId)
        {
            try
            {
                DataAccess objLib =
                    (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                    .CreateInstance();

                //Get record and set point to allow fields to be updated
                object vntContractId = this.FindContractByLot(rSys, vntLotId);

                if (null == vntContractId)
                {
                    throw new PivotalApplicationException("Contract must exist in order to create realtor record.");
                }
                object[] arrFields = new object[] { IntegrationConstants.strfOPPORTUNITY_ID, "Type", "Agent_Id", "Company_Id" };

                Recordset rstCompOpp = objLib.GetNewRecordset("Company__Opportunity", arrFields);


                //Force new record to be created
                rstCompOpp.AddNew(Type.Missing, Type.Missing);
                rstCompOpp.Fields[IntegrationConstants.strfOPPORTUNITY_ID].Value = vntContractId;
                rstCompOpp.Fields["Type"].Value = "Realtor";
                if (vntAgent != null && vntAgent != DBNull.Value)
                {
                    rstCompOpp.Fields["Agent_Id"].Value = vntAgent;
                }
                if (vntAgency != null && vntAgency != DBNull.Value)
                {
                    rstCompOpp.Fields["Company_Id"].Value = vntAgency;
                }
                objLib.SaveRecordset("Company__Opportunity", rstCompOpp);

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        /// <summary>
        /// Used to lookup company by external source id
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public object FindContact(IRSystem7 rSys, string contactId)
        {
            try
            {
                object vntResult = rSys.Tables[IntegrationConstants.strtCONTACT].Fields[IntegrationConstants.strfCONTACT_ID].FindValue(
                   rSys.Tables[IntegrationConstants.strtCONTACT].Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID],
                   contactId);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }
                //Return result
                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Used to lookup SI Records by external source id
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="SIId"></param>
        /// <returns></returns>
        public object FindSI(IRSystem7 rSys, string SIId)
        {
            try
            {
                object vntResult = rSys.Tables[IntegrationConstants.strtSUPPORT_INCIDENT].Fields[IntegrationConstants.strfSUPPORT_INCIDENT_ID].FindValue(
                   rSys.Tables[IntegrationConstants.strtSUPPORT_INCIDENT].Fields[IntegrationConstants.strfMI_EXTERNAL_SOURCE_ID],
                   SIId);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }
                //Return result
                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Used to lookup SI Records by external source id
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="SIId"></param>
        /// <returns></returns>
        public object FindSupportStep(IRSystem7 rSys, string SSId)
        {
            try
            {
                object vntResult = rSys.Tables[IntegrationConstants.strtSUPPORT_STEP].Fields[IntegrationConstants.strfSUPPORT_STEP_ID].FindValue(
                   rSys.Tables[IntegrationConstants.strtSUPPORT_STEP].Fields[IntegrationConstants.strfMI_EXTERNAL_SOURCE_ID],
                   SSId);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }
                //Return result
                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        /// <summary>
        /// Used to lookup company by external source id
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public object FindContactByCFT(IRSystem7 rSys, string contactId)
        {
            try
            {
                object vntResult = rSys.Tables[IntegrationConstants.strtCONTACT].Fields[IntegrationConstants.strfCONTACT_ID].FindValue(
                   rSys.Tables[IntegrationConstants.strtCONTACT].Fields["MI_CFT_ID"],
                   contactId);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }
                //Return result
                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        public object FindLeadByMigrationID(IRSystem7 rSys, string leadId)
        {
            try
            {
                object vntResult = rSys.Tables[IntegrationConstants.strtLEAD].Fields["Lead__Id"].FindValue(
                   rSys.Tables[IntegrationConstants.strtLEAD].Fields["MI_Migration_Id"],
                   leadId);

                //If nothing is returned make sure you null out result
                if (null == vntResult)
                {
                    vntResult = DBNull.Value;
                }
                //Return result
                return vntResult;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will be used by the Company Integration touchpoint to create a new 
        /// NBHDP__Company record for a new company if one doesn't exist
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="pform"></param>
        /// <param name="rs"></param>
        /// <param name="vntDivisionId"></param>
        /// <param name="vntCompanyId"></param>
        public void SetNBHDPCompany(IRSystem7 rSys, IRForm pform, Recordset rs, object vntDivisionId, object vntCompanyId)
        {
            //Create dataset to work with
            IRDataset4 rdstDataset = (IRDataset4)rSys.CreateDataset();

            try
            {
                DataAccess objLib =
                    (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                    .CreateInstance();

                IntegrationUtility util = new IntegrationUtility();

                //Create a profile if one doesn't exist
                object[] vntAllFieldNames = new object[] {IntegrationConstants.gstrfMI_DIVISION_ID, 
                    IntegrationConstants.strfCOMPANY_ID};


                Recordset rstNCompany = objLib.GetNewRecordset("NBHDP__Company", vntAllFieldNames);

                //Force new record to be created
                rstNCompany.AddNew(Type.Missing, Type.Missing);
                rstNCompany.Fields[IntegrationConstants.gstrfDIVISION_ID].Value = vntDivisionId;
                rstNCompany.Fields[IntegrationConstants.strfCOMPANY_ID].Value = vntCompanyId;
                objLib.SaveRecordset("NBHDP__Company", rstNCompany);

                //Marshal.ReleaseComObject(rdstDataset);

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }

            finally
            {
                if (rdstDataset != null)
                {
                    Marshal.ReleaseComObject(rdstDataset);
                }
            }

        }

        public void InsertNBHDPCompany(IRSystem7 rSys, object companyId, object divisionId)
        {
            try
            {
                DataAccess objLib =
                    (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                    .CreateInstance();
                Recordset rstNBHDPCompany = new Recordset();
                rstNBHDPCompany = objLib.GetNewRecordset(IntegrationConstants.strtNBHDP__COMPANY,
                    new object[] { IntegrationConstants.strfCOMPANY_ID, "MI_Division_Id" });

                rstNBHDPCompany.AddNew(Type.Missing, Type.Missing);
                rstNBHDPCompany.Fields[IntegrationConstants.strfCOMPANY_ID].Value = companyId;
                rstNBHDPCompany.Fields["MI_Division_Id"].Value = divisionId;
                objLib.SaveRecordset(IntegrationConstants.strtNBHDP__COMPANY, rstNBHDPCompany);
                rstNBHDPCompany.Close();

            }

            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }
        /// <summary>
        /// This will create a trade code
        /// </summary>
        /// <param name="tradecode"></param>
        /// <param name="tradedescription"></param>
        public object CreateTrade(IRSystem7 rSys, string tradeCode, string tradeDescription)
        {

            try
            {
                Recordset rstTrade = new Recordset();
                object vntTradeId = null;
                DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                //Field List 
                object[] arrFieldList = new object[] {IntegrationConstants.strfTRADE_CODE, 
                                                      IntegrationConstants.strfTRADE_NAME};
                //Record does not exist so need to create it
                rstTrade = objLib.GetNewRecordset(IntegrationConstants.strtTRADE, arrFieldList);
                rstTrade.AddNew(Type.Missing, Type.Missing);
                rstTrade.Fields[IntegrationConstants.strfTRADE_CODE].Value = tradeCode;
                rstTrade.Fields[IntegrationConstants.strfTRADE_NAME].Value = tradeDescription;
                objLib.SaveRecordset(IntegrationConstants.strtTRADE, rstTrade);
                vntTradeId = rstTrade.Fields[IntegrationConstants.strfTRADE_ID].Value;
                //Release recordset
                rstTrade.Close();
                return vntTradeId;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }

        }
        /// <summary>
        /// This method will create a new contact web detail record if one does not exist.
        /// This logic is specific to MI Homes
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntContactId"></param>
        /// 
        public void CreateContactWebDetails(IRSystem7 rSys, object vntContactId, object vntOpportunityId)
        {
            try
            {

                DataAccess objLib =
                    (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                    .CreateInstance();


                Recordset contactWebDetailsTest = objLib.GetLinkedRecordset(IntegrationConstants.strtCONTACT_WEB_DETAILS, IntegrationConstants.strfCONTACT_ID, vntContactId, "Contact_Id");


                if (contactWebDetailsTest.RecordCount > 0)
                {
                    //Do nothing yet. May need to add logic later
                }
                else
                {
                    //create the new record
                    //Field List 
                    object[] arrFieldList = new object[] {IntegrationConstants.strfCONTACT_ID, IntegrationConstants.strfCONTACT_WEB_DETAILS_Id, 
                                                      IntegrationConstants.strfLOGIN_NAME, IntegrationConstants.strfPASSWORD_ENCRYPT, IntegrationConstants.strfTIME_ZONE,
                                                      IntegrationConstants.strfCONTACT_EMAIL_ADDRESS};
                    Recordset rstContactWebDetails = objLib.GetNewRecordset(IntegrationConstants.strtCONTACT_WEB_DETAILS, arrFieldList);

                    Recordset rstContact = objLib.GetRecordset(vntContactId, IntegrationConstants.strtCONTACT, "Email");
                    Recordset rstOpporunity = objLib.GetRecordset(vntOpportunityId, IntegrationConstants.strtOPPORTUNITY,
                                                                    IntegrationConstants.strfENV_EDC_PASSWORD, "Env_Edc_Username");


                    //Force new record to be created
                    rstContactWebDetails.AddNew(Type.Missing, Type.Missing);
                    rstContactWebDetails.Fields[IntegrationConstants.strfCONTACT_ID].Value = vntContactId;
                    rstContactWebDetails.Fields[IntegrationConstants.strfLOGIN_NAME].Value = rstOpporunity.Fields["Env_Edc_Username"].Value;
                    rstContactWebDetails.Fields[IntegrationConstants.strfPASSWORD_ENCRYPT].Value = rstOpporunity.Fields[IntegrationConstants.strfENV_EDC_PASSWORD].Value;
                    rstContactWebDetails.Fields[IntegrationConstants.strfCONTACT_EMAIL_ADDRESS].Value = rstContact.Fields["Email"].Value;
                    objLib.SaveRecordset(IntegrationConstants.strtCONTACT_WEB_DETAILS, rstContactWebDetails);

                    object[] arrFieldList2 = new object[] { "User_Id", "Group_Id" };
                    Recordset rstEUsers = objLib.GetNewRecordset("Extranet_Group_Members", arrFieldList2);

                    rstEUsers.AddNew(Type.Missing, Type.Missing);
                    rstEUsers.Fields["User_Id"].Value = rstContactWebDetails.Fields["Contact_Web_Details_Id"].Value;
                    rstEUsers.Fields["Group_Id"].Value = rSys.StringToId("0x8000000000000061");
                    objLib.SaveRecordset("Extranet_Group_Members", rstEUsers);

                    rstEUsers.Close();
                    rstContactWebDetails.Close();
                }
            }

            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// This method will create a new contact web detail record if one does not exist.
        /// This logic is specific to MI Homes
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntContactId"></param>
        /// 
        public void MoveLotProducts(IRSystem7 rSys, object vntOpportunityId)
        {
            try
            {

                DataAccess objLib =
                    (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                    .CreateInstance();

                //This is only called on close. Update all selected options to built
                Recordset unbuiltOptions = objLib.GetRecordset("MI: Not Built and Selected Options for Quote ?", 1, vntOpportunityId, "Built_Option");

                if (unbuiltOptions.RecordCount > 0)
                {
                    unbuiltOptions.MoveFirst();
                    while (!unbuiltOptions.EOF)
                    {
                        unbuiltOptions.Fields["Built_Option"].Value = 1;
                        unbuiltOptions.MoveNext();
                    }
                    objLib.SaveRecordset("Opportunity__Product", unbuiltOptions);
                }
                object parameterList = DBNull.Value;

                // Add homesite configuration
                TransitionPointParameter transitionPointParameter = (TransitionPointParameter)rSys.ServerScripts
                    [AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                transitionPointParameter.SetUserDefinedParameter(1, vntOpportunityId);
                parameterList = transitionPointParameter.ParameterList;
                rSys.Forms["Lot Configuration"].Execute("Create Homesite Configuration",
                    ref parameterList);


            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }

        /// <summary>
        /// Updates the TIC_Future_Change_Price field on the supplied Active Contract or Reservation's Opportunity record
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntActiveContractOrReservationId"></param>
        /// <param name="decFutureChangePrice"></param>
        protected virtual void UpdateActiveContractOrReservationFutureChangePrice(IRSystem7 rSys, object vntActiveContractOrReservationId, decimal decFutureChangePrice)
        {
            try
            {
                // Only proceed if vntActiveContractOrReservationId is not null
                if (vntActiveContractOrReservationId != null)
                {
                    // Use this object to get new recordset
                    DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                    // Define fields to retrieve from Opportunity - including TIC_Future_Change_Price
                    object[] arrFields = arrFields = new object[] 
                    {
                        IntegrationConstants.strfOPPORTUNITY_ID, 
                        IntegrationConstants.strfTIC_FUTURE_CHANGE_PRICE 
                    };

                    // Get the Opportunity recordset for the Active Contract or Reservation
                    Recordset rstOpportunity = objLib.GetRecordset(vntActiveContractOrReservationId,
                                                                   IntegrationConstants.strtOPPORTUNITY,
                                                                   arrFields);

                    // If we got an Opportunity record back then update it...
                    if (rstOpportunity != null)
                    {
                        if (rstOpportunity.RecordCount > 0)
                        {
                            rstOpportunity.MoveFirst();
                            // Update the TIC_Future_Change_Price field with the new price value
                            rstOpportunity.Fields[IntegrationConstants.strfTIC_FUTURE_CHANGE_PRICE].Value = decFutureChangePrice;
                            // Save the updates
                            objLib.SaveRecordset(IntegrationConstants.strtOPPORTUNITY, rstOpportunity);
                        }

                        // Clean-up
                        rstOpportunity.Close();
                    }
                }
                else
                {
                    // vntActiveContractOrReservationId is Nullable - throw exception
                    throw new PivotalApplicationException("UpdateActiveContractOrReservationFutureChangePrice() - method cannot be executed without suppling a non-null vntActiveContractOrReservationId");
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }



        /// <summary>
        /// Updates the TIC_Future_Change_Price field on the supplied Active Contract or Reservation's Opportunity record
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="vntActiveContractOrReservationId"></param>
        /// <param name="decFutureChangePrice"></param>
        protected virtual void UpdateActiveContractOrReservationElevationFutureChangePrice(IRSystem7 rSys, object vntActiveContractOrReservationId, decimal decFutureChangePrice)
        {
            try
            {
                // Only proceed if vntActiveContractOrReservationId is not null
                if (vntActiveContractOrReservationId != null)
                {
                    // Use this object to get new recordset
                    DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                    // Define fields to retrieve from Opportunity - including TIC_Future_Change_Price
                    object[] arrFields = arrFields = new object[] 
                    {
                        IntegrationConstants.strfOPPORTUNITY_ID, 
                        IntegrationConstants.strfTIC_FUTURE_ELEVATION_PREMIUM 
                    };

                    // Get the Opportunity recordset for the Active Contract or Reservation
                    Recordset rstOpportunity = objLib.GetRecordset(vntActiveContractOrReservationId,
                                                                   IntegrationConstants.strtOPPORTUNITY,
                                                                   arrFields);

                    // If we got an Opportunity record back then update it...
                    if (rstOpportunity != null)
                    {
                        if (rstOpportunity.RecordCount > 0)
                        {
                            rstOpportunity.MoveFirst();
                            // Update the TIC_Future_Change_Price field with the new price value
                            rstOpportunity.Fields[IntegrationConstants.strfTIC_FUTURE_ELEVATION_PREMIUM].Value = decFutureChangePrice;
                            // Save the updates
                            objLib.SaveRecordset(IntegrationConstants.strtOPPORTUNITY, rstOpportunity);
                        }

                        // Clean-up
                        rstOpportunity.Close();
                    }
                }
                else
                {
                    // vntActiveContractOrReservationId is Nullable - throw exception
                    throw new PivotalApplicationException("UpdateActiveContractOrReservationFutureChangePrice() - method cannot be executed without suppling a non-null vntActiveContractOrReservationId");
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, rSys);
            }
        }


        /// <summary>
        /// This method will find the associated contingency record in Pivotal and update
        /// the reason with theinformatino from the sequence sheet.
        /// </summary>
        /// <param name="vntOpportunityId"></param>
        /// <param name="rstPrimary"></param>
        public virtual void UpdateContingencyInformation(IRSystem7 mrsysSystem, object vntOpportunityId, Recordset rstPrimary)
        {

            // Get Data Access
            DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            objLib.PermissionIgnored = true;

            // Execute a Query to return the TIC_Construction_Project record with TIC_Construction_Project.External_Source_Community_Id = strExtSrcConstructionProjCode, 
            // returning TIC_Construction_Project_Id and Name fields

            string[] arrFields = new string[] { IntegrationConstants.strfCONTINGENCY_EXP_DATE, IntegrationConstants.strfCONTINGENCY_TIC_REASON_CODE,
                IntegrationConstants.strfOPPORTUNITY_ID, "Rn_Create_Date"};

            Recordset rstCont = objLib.GetRecordset("HB: Contingency for Opp ?", 1, vntOpportunityId, arrFields);


            if (rstCont != null)
            {
                if (rstCont.RecordCount > 0)
                {
                    rstCont.Sort = "Rn_Create_Date DESC";
                    rstCont.MoveFirst();
                    rstCont.Fields[IntegrationConstants.strfCONTINGENCY_TIC_REASON_CODE].Value = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDISC_CONTINGENCY_TIC_REASON_CODE].Value);

                    if(!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_CONTINGENCY_EXP_DATE].Value))
                    {
                        rstCont.Fields[IntegrationConstants.strfCONTINGENCY_EXP_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_CONTINGENCY_EXP_DATE].Value);
                    }
                }
                else
                {
                    rstCont = objLib.GetNewRecordset(IntegrationConstants.strtCONTINGENCY, arrFields);
                    rstCont.AddNew(Type.Missing, Type.Missing);
                    rstCont.Fields[IntegrationConstants.strfCONTINGENCY_TIC_REASON_CODE].Value = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDISC_CONTINGENCY_TIC_REASON_CODE].Value);
                    rstCont.Fields[IntegrationConstants.strfOPPORTUNITY_ID].Value = vntOpportunityId;
                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_CONTINGENCY_EXP_DATE].Value))
                    {
                        rstCont.Fields[IntegrationConstants.strfCONTINGENCY_EXP_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_CONTINGENCY_EXP_DATE].Value);
                    }

                }

                objLib.SaveRecordset(IntegrationConstants.strtCONTINGENCY, rstCont);
                rstCont.Close();
            }


        }

        /// <summary>
        /// This method will check to see if an existing escrow record exists for the 
        /// Closed Contract for the Lot.  If an esrow record is found, then the integration will
        /// update the esrow values from the sequence sheet.
        /// </summary>
        /// <param name="rstPrimary"></param>
        protected virtual void ManageEscrowFieldsForLot(IRSystem7 mrsysSystem, Recordset rstPrimary, object vntOppId)
        {
            //Find an escrow record for the lot being updated.  If one exists, then 
            //set fields on Escrow record from incoming Sequence sheet data.
            //"INT - Escrow By Lot Id ?";
            string[] arrFields = new string[] 
            {
                 IntegrationConstants.strfTIC_LOT_ID, 
                IntegrationConstants.strfTIC_EST_LOAN_APP_DATE,
                IntegrationConstants.strfTIC_ACT_LOAN_APP_DATE,
                IntegrationConstants.strfTIC_EST_APPROVAL_DATE,
                IntegrationConstants.strfTIC_EST_DOCS_TO_ESCROW_DATE,
                IntegrationConstants.strfTIC_ACT_DOCS_TO_ESCROW_DATE,
                IntegrationConstants.strfTIC_EST_DOCS_SIGN_DATE,
                IntegrationConstants.strfTIC_ACT_DOCS_SIGN_DATE,
                IntegrationConstants.strfTIC_ACT_APPROVAL_DATE,
                IntegrationConstants.strfTIC_APPRAISAL_ORDER,
                IntegrationConstants.strfTIC_APPRAISAL_RECEIVED,
                IntegrationConstants.strfTIC_LOAN_STATUS_COMMENTS,
                IntegrationConstants. strfTIC_FLOORING_RELEASE,
                IntegrationConstants. strfTIC_HOMEOWNER_WALK_SCHEDULED,
                IntegrationConstants.strfTIC_HOMEOWNER_WALK_ACTUAL,
                IntegrationConstants.strfTIC_FINAL_PRICE_SENT_DATE,
                IntegrationConstants.strfTIC_BUILDER_PACK_SENT_DATE,
                IntegrationConstants.strfTIC_JOB_CARD_REC_DATE,
                IntegrationConstants.strfTIC_NOTICE_OF_COMP_SUB_DATE,
                IntegrationConstants.strfTIC_DEL_ASSMT_AT_CLOSING_DATE,
                IntegrationConstants. strfTIC_YELLOW_REPORT_RECEIVED,
                IntegrationConstants. strfTIC_WHITE_REPORT_RECEIVED,
                IntegrationConstants. strfTIC_GAS_METER_INST_DATE,
                IntegrationConstants. strfTIC_ESCROW_DOC_COMMENTS,
                IntegrationConstants.strfTIC_FUNDED,
                IntegrationConstants.strfTIC_CONTRACT_ID,
                IntegrationConstants.strfTIC_ESTIMATED_COE,
                IntegrationConstants.strfTIC_GRANT_DEED_TO_ESCROW
            };


            // Get Data Access
            DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            objLib.PermissionIgnored = true;

            // Execute a Query to return the TIC_Construction_Project record with TIC_Construction_Project.External_Source_Community_Id = strExtSrcConstructionProjCode, 
            // returning TIC_Construction_Project_Id and Name fields
            Recordset rstEscrow = objLib.GetRecordset("INT - Escrow By Lot Id ?", 2, rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value,
                vntOppId, arrFields);

            if (rstEscrow != null)
            {
                if (rstEscrow.RecordCount > 0)
                {
                    //Esrow record found so update it
                    rstEscrow.MoveFirst();
                    rstEscrow.Fields[IntegrationConstants.strfTIC_LOT_ID].Value = rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value;
                    rstEscrow.Fields[IntegrationConstants.strfTIC_CONTRACT_ID].Value = vntOppId;
                    rstEscrow.Fields[IntegrationConstants.strfTIC_LOAN_STATUS_COMMENTS].Value = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_LOAN_STATUS_COMMENTS].Value);
                    rstEscrow.Fields[IntegrationConstants.strfTIC_ESCROW_DOC_COMMENTS].Value = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ESCROW_DOC_COMMENTS].Value);

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_LOAN_APP_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_EST_LOAN_APP_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_LOAN_APP_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_LOAN_APP_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_ACT_LOAN_APP_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_LOAN_APP_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_APPROVAL_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_EST_APPROVAL_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_APPROVAL_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_DOCS_TO_ESCROW_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_EST_DOCS_TO_ESCROW_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_DOCS_TO_ESCROW_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_DOCS_TO_ESCROW_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_ACT_DOCS_TO_ESCROW_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_DOCS_TO_ESCROW_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_DOCS_SIGN_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_EST_DOCS_SIGN_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_DOCS_SIGN_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_DOCS_SIGN_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_ACT_DOCS_SIGN_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_DOCS_SIGN_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_APPROVAL_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_ACT_APPROVAL_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_APPROVAL_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_APPRAISAL_ORDER].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_APPRAISAL_ORDER].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_APPRAISAL_ORDER].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_APPRAISAL_RECEIVED].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_APPRAISAL_RECEIVED].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_APPRAISAL_RECEIVED].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_FLOORING_RELEASE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_FLOORING_RELEASE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_FLOORING_RELEASE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_HOMEOWNER_WALK_SCHEDULED].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_HOMEOWNER_WALK_SCHEDULED].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_HOMEOWNER_WALK_SCHEDULED].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_HOMEOWNER_WALK_ACTUAL].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_HOMEOWNER_WALK_ACTUAL].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_HOMEOWNER_WALK_ACTUAL].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_FINAL_PRICE_SENT_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_FINAL_PRICE_SENT_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_FINAL_PRICE_SENT_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_BUILDER_PACK_SENT_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_BUILDER_PACK_SENT_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_BUILDER_PACK_SENT_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_JOB_CARD_REC_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_JOB_CARD_REC_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_JOB_CARD_REC_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_NOTICE_OF_COMP_SUB_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_NOTICE_OF_COMP_SUB_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_NOTICE_OF_COMP_SUB_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_DEL_ASSMT_AT_CLOSING_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_DEL_ASSMT_AT_CLOSING_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_DEL_ASSMT_AT_CLOSING_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_YELLOW_REPORT_RECEIVED].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_YELLOW_REPORT_RECEIVED].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_YELLOW_REPORT_RECEIVED].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_WHITE_REPORT_RECEIVED].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_WHITE_REPORT_RECEIVED].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_WHITE_REPORT_RECEIVED].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_GAS_METER_INST_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_GAS_METER_INST_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_GAS_METER_INST_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_FUNDED].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_FUNDED].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_FUNDED].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_ECOE_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_ESTIMATED_COE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_ECOE_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_GRANT_TO_DEED].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_GRANT_DEED_TO_ESCROW].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_GRANT_TO_DEED].Value); }

                }
                else
                {
                    rstEscrow = objLib.GetNewRecordset(IntegrationConstants.strtTIC_ESCROW, arrFields);
                    rstEscrow.AddNew(Type.Missing, Type.Missing);
                    rstEscrow.Fields[IntegrationConstants.strfTIC_LOT_ID].Value = rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value;
                    rstEscrow.Fields[IntegrationConstants.strfTIC_CONTRACT_ID].Value = vntOppId;
                    rstEscrow.Fields[IntegrationConstants.strfTIC_LOAN_STATUS_COMMENTS].Value = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_LOAN_STATUS_COMMENTS].Value);
                    rstEscrow.Fields[IntegrationConstants.strfTIC_ESCROW_DOC_COMMENTS].Value = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ESCROW_DOC_COMMENTS].Value);

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_LOAN_APP_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_EST_LOAN_APP_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_LOAN_APP_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_LOAN_APP_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_ACT_LOAN_APP_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_LOAN_APP_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_APPROVAL_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_EST_APPROVAL_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_APPROVAL_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_DOCS_TO_ESCROW_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_EST_DOCS_TO_ESCROW_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_DOCS_TO_ESCROW_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_DOCS_TO_ESCROW_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_ACT_DOCS_TO_ESCROW_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_DOCS_TO_ESCROW_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_DOCS_SIGN_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_EST_DOCS_SIGN_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_EST_DOCS_SIGN_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_DOCS_SIGN_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_ACT_DOCS_SIGN_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_DOCS_SIGN_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_APPROVAL_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_ACT_APPROVAL_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_ACT_APPROVAL_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_APPRAISAL_ORDER].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_APPRAISAL_ORDER].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_APPRAISAL_ORDER].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_APPRAISAL_RECEIVED].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_APPRAISAL_RECEIVED].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_APPRAISAL_RECEIVED].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_FLOORING_RELEASE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_FLOORING_RELEASE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_FLOORING_RELEASE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_HOMEOWNER_WALK_SCHEDULED].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_HOMEOWNER_WALK_SCHEDULED].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_HOMEOWNER_WALK_SCHEDULED].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_HOMEOWNER_WALK_ACTUAL].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_HOMEOWNER_WALK_ACTUAL].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_HOMEOWNER_WALK_ACTUAL].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_FINAL_PRICE_SENT_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_FINAL_PRICE_SENT_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_FINAL_PRICE_SENT_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_BUILDER_PACK_SENT_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_BUILDER_PACK_SENT_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_BUILDER_PACK_SENT_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_JOB_CARD_REC_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_JOB_CARD_REC_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_JOB_CARD_REC_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_NOTICE_OF_COMP_SUB_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_NOTICE_OF_COMP_SUB_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_NOTICE_OF_COMP_SUB_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_DEL_ASSMT_AT_CLOSING_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_DEL_ASSMT_AT_CLOSING_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_DEL_ASSMT_AT_CLOSING_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_YELLOW_REPORT_RECEIVED].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_YELLOW_REPORT_RECEIVED].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_YELLOW_REPORT_RECEIVED].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_WHITE_REPORT_RECEIVED].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_WHITE_REPORT_RECEIVED].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_WHITE_REPORT_RECEIVED].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_GAS_METER_INST_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_GAS_METER_INST_DATE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_GAS_METER_INST_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_FUNDED].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_FUNDED].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_FUNDED].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_ECOE_DATE].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_ESTIMATED_COE].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_ECOE_DATE].Value); }

                    if (!Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_GRANT_TO_DEED].Value))
                    { rstEscrow.Fields[IntegrationConstants.strfTIC_GRANT_DEED_TO_ESCROW].Value = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfDISC_TIC_GRANT_TO_DEED].Value); }


                }

                objLib.SaveRecordset(IntegrationConstants.strtTIC_ESCROW, rstEscrow);
                rstEscrow.Close();

            }


        }

        /// <summary>
        /// This method will write a change log record to Pivotal
        /// if any field defined to be monitored is changed since the last 
        /// time a record was added.
        /// </summary>
        public virtual void WriteChangeLogForOption(IRSystem7 rSys, string optionNumber, string fieldName, string originalValue,
            string newValue, string fileName, string planCode, bool isNew)
        {
            //Find an escrow record for the lot being updated.  If one exists, then 
            //set fields on Escrow record from incoming Sequence sheet data.
            //"INT - Escrow By Lot Id ?";
            string[] arrFields = new string[] 
            {
                IntegrationConstants.strfFIELD_NAME,
                IntegrationConstants.strfORIGINAL_VALUE,
                IntegrationConstants.strfNEW_VALUE,
                IntegrationConstants.strfTIME_PROCESSED_INTO_PIVOTAL,
                IntegrationConstants.strfCHATEAU_FILE_FROM,
                IntegrationConstants.strfOPTION_NUMBER,
                IntegrationConstants.strfIS_NEW,
                IntegrationConstants.strfPROCESSED,
                IntegrationConstants.strfPLAN_CODE
                
            };


            // Get Data Access
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            objLib.PermissionIgnored = true;

            // Execute a Query to return the TIC_Construction_Project record with TIC_Construction_Project.External_Source_Community_Id = strExtSrcConstructionProjCode, 
            // returning TIC_Construction_Project_Id and Name fields
            Recordset rstChangeLog = objLib.GetNewRecordset(IntegrationConstants.strtTIC_OPTION_CHANGE_LOG, arrFields);
            rstChangeLog = objLib.GetNewRecordset(IntegrationConstants.strtTIC_OPTION_CHANGE_LOG, arrFields);
            rstChangeLog.AddNew(Type.Missing, Type.Missing);
            rstChangeLog.Fields[IntegrationConstants.strfFIELD_NAME].Value = TypeConvert.ToString(fieldName);
            rstChangeLog.Fields[IntegrationConstants.strfORIGINAL_VALUE].Value = TypeConvert.ToString(originalValue);
            rstChangeLog.Fields[IntegrationConstants.strfNEW_VALUE].Value = TypeConvert.ToString(newValue);
            rstChangeLog.Fields[IntegrationConstants.strfTIME_PROCESSED_INTO_PIVOTAL].Value = DateTime.Now;
            rstChangeLog.Fields[IntegrationConstants.strfCHATEAU_FILE_FROM].Value = TypeConvert.ToString(fileName);
            rstChangeLog.Fields[IntegrationConstants.strfOPTION_NUMBER].Value = TypeConvert.ToString(optionNumber);
            rstChangeLog.Fields[IntegrationConstants.strfIS_NEW].Value = isNew;
            rstChangeLog.Fields[IntegrationConstants.strfPROCESSED].Value = false;

            if (!String.IsNullOrEmpty(planCode))
            {
                rstChangeLog.Fields[IntegrationConstants.strfPLAN_CODE].Value = planCode;
            }

            objLib.SaveRecordset(IntegrationConstants.strtTIC_OPTION_CHANGE_LOG, rstChangeLog);
            rstChangeLog.Close();
        }



    }
}
