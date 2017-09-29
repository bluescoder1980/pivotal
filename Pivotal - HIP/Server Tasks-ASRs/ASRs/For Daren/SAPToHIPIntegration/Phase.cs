using System;
using System.Collections.Generic;
using System.Text;
//Add Pivotal namespaces
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;
using Pivotal.Interop.COMAdminLib;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Choice;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Form;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;


namespace CRM.Pivotal.IP.SAPToHIPIntegration
{

    /// <summary>
    /// Phases will be integrated to Pivotal from Fusion by invoking
    /// a webservice that calls the AddFormData and SaveFormData on this 
    /// inbound AppServer rule.
    /// </summary>
    /// 
    /// <Author>A.Maldonado</Author>
    /// <CreateDate>10/15/2007</CreateDate>
    /// <Version>1.0</Version>
    class Phase : IRFormScript
    {
        #region Class Variables

        IRSystem7 mrsysSystem;

        #endregion

        #region IRFormScript Members


        /// <summary>
        /// For inserts need to make sure integration logic is performed on record 
        /// before being written to Pivotal DB.
        /// </summary>
        /// <param name="pForm">HBIntPhase</param>
        /// <param name="Recordsets">Recordset from Fusion</param>
        /// <param name="ParameterList"></param>
        /// <returns></returns>
        public object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {

            try
            {
                //Get incoming recordset
                object[] recordsetArray = (object[])Recordsets;
                Recordset rstPhase = (Recordset)recordsetArray[0];

                //Set Default Fields and perform lookup logic
                this.DoPhaseLookupLogic(pForm, rstPhase);

                //2007.11.19 - AAM MI Specific logic
                //Set Open/Closed Date based on Status
                if (rstPhase.Fields[IntegrationConstants.strfSTATUS].Value.ToString() == "Open")
                {
                    //Set Open_Date = Today
                    rstPhase.Fields[IntegrationConstants.strfOPEN_DATE].Value = DateTime.Now;
                    //Clear out Closed_Date
                    rstPhase.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value = DBNull.Value;
                }
                else if (rstPhase.Fields[IntegrationConstants.strfSTATUS].Value.ToString() == "Closed")
                {
                    //Set Closed Date
                    rstPhase.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value = DateTime.Now;
                }
                else
                {
                    //If neither open nor closed null out open/closed dates
                    rstPhase.Fields[IntegrationConstants.strfOPEN_DATE].Value = DBNull.Value;
                    rstPhase.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value = DBNull.Value;
                }


                object vntPhase = pForm.DoAddFormData(Recordsets, ref ParameterList);

                //Use utility class to copy division adjustments to release
                IntegrationUtility util = new IntegrationUtility();
                util.CopyDivisionAdjustments(mrsysSystem, rstPhase);

                return vntPhase;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }

        }

        public void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            pForm.DoDeleteFormData(RecordId, ref ParameterList);
        }

        public void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        { }

        public object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            return pForm.DoLoadFormData(RecordId, ref ParameterList);
        }

        public object NewFormData(IRForm pForm, ref object ParameterList)
        {
            return pForm.DoNewFormData(ref ParameterList);
        }

        public void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset Recordset)
        {
            pForm.DoNewSecondaryData(SecondaryName, ref ParameterList, Recordset);
        }

        /// <summary>
        /// For update need to make sure integration logic is performed on record before
        /// being written to Pivotal DB.
        /// </summary>
        /// <param name="pForm">HBIntCompany</param>
        /// <param name="Recordsets">Incoming recordset from Pervasive work</param>
        /// <param name="ParameterList"></param>
        public void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                //Get incoming recordset
                object[] recordsetArray = (object[])Recordsets;
                Recordset rstPhase = (Recordset)recordsetArray[0];

                //Set Default Fields and perform lookup logic
                this.DoPhaseLookupLogic(pForm, rstPhase);

                //2007.11.19 - AAM MI Specific logic
                //Set Open/Closed Date based on Status
                if (rstPhase.Fields[IntegrationConstants.strfSTATUS].Value.ToString() == "Open")
                {
                    //Only update Release Date if not already set.
                    if (DBNull.Value == rstPhase.Fields[IntegrationConstants.strfOPEN_DATE].Value)
                    {
                        //Set Open_Date = Today
                        rstPhase.Fields[IntegrationConstants.strfOPEN_DATE].Value = DateTime.Now;
                    }
                    //Clear out Closed_Date
                    rstPhase.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value = DBNull.Value;
                }
                else if (rstPhase.Fields[IntegrationConstants.strfSTATUS].Value.ToString() == "Closed")
                {
                    //Only update Closed Date if not already set
                    if (DBNull.Value == rstPhase.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value)
                    {
                        rstPhase.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value = DateTime.Now;
                    }
                }
                else
                {
                    //If neither open nor closed null out open/closed dates
                    rstPhase.Fields[IntegrationConstants.strfOPEN_DATE].Value = DBNull.Value;
                    rstPhase.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value = DBNull.Value;
                }
                //Call DoSaveForm Data after performing lookup logic
                pForm.DoSaveFormData(Recordsets, ref ParameterList);
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }


        }

        public void SetSystem(RSystem pSystem)
        {

            mrsysSystem = (IRSystem7)pSystem;
        }

        #endregion


        /// <summary>
        /// This method will perform all of the integration logic for the 
        /// inbound phase prior to be written to the Pivotal database.  This 
        /// method replaces the SetFields method from the VB6 project
        /// </summary>
        /// <param name="rstPrimary"></param>
        public void DoPhaseLookupLogic(IRForm pform, Recordset rstPrimary)
        {

            try
            {
                string strCaller = string.Empty;
                string strErrMsg = string.Empty;

                //Utility class
                IntegrationUtility util = new IntegrationUtility();

                //Get Incoming Phase Id
                object vntPhaseId = rstPrimary.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value;

                //Build field values array to be copied from Neighborhood
                object[] vntAllSourceFldNames = new object[] { IntegrationConstants.strfNEIGHBORHOOD_ID, 
                                                               IntegrationConstants.strfADDRESS_1,
                                                               IntegrationConstants.strfADDRESS_2,
                                                               IntegrationConstants.strfADDRESS_3,
                                                               IntegrationConstants.gstrfAREA_CODE,
                                                               IntegrationConstants.strfCITY,
                                                               IntegrationConstants.strfCOUNTY_ID,
                                                               IntegrationConstants.gstrfSTATE,
                                                               IntegrationConstants.strfCOUNTRY,
                                                               IntegrationConstants.strfZIP,
                                                               IntegrationConstants.gstrfPHONE,
                                                               IntegrationConstants.gstrfSALES_MANAGER_ID,
                                                               IntegrationConstants.gstrfCONSTRUCTION_MANAGER_ID};
                
                //Set field to be used for lookup value
                object[] arrLookUpFieldValues = new object[]
                {
                    new object[] {true, pform.Segments[1].SegmentName, 
                    IntegrationConstants.gstrfPHASE_COMMUNITY}
                };
                
                //Get value to set
                object[] vntReturn =  util.SetValuesByLookUp(mrsysSystem, pform, rstPrimary, null, arrLookUpFieldValues, null,
                    IntegrationConstants.strtNEIGHBORHOOD, IntegrationConstants.gstrqCOMMUNITY_BY_EXTERNAL_SOURCE_ID, vntAllSourceFldNames);

                //Get values returned
                object vntCommunityId = vntReturn[0];
                Recordset rstCommunity = (Recordset)vntReturn[1];

                //If Phase Id null, then it's an insert
                if (null == vntPhaseId)
                {
                    strCaller = "Insert";
                }
                else
                {
                    strCaller = "Update";
                }

                //If Community is null, then reject
                if (null == vntCommunityId || DBNull.Value == vntCommunityId)
                {
                    if (strErrMsg == string.Empty)
                    {
                        strErrMsg = "HBIntNeighborhood - Phase must have an existing " +
                            "community in Pivotal.";
                    }
                    else
                    {
                        strErrMsg = strErrMsg + ", " + "HBIntNeighborhood";
                    }
                    //Reject record
                    throw new PivotalApplicationException(strErrMsg);
                }
                else 
                {
                    rstCommunity.MoveFirst();
                    //Set the Neighborhood Id on current recordset
                    rstPrimary.Fields[IntegrationConstants.gstrfNEIGHBORHOOD_ID].Value = vntCommunityId;

                    //Update Release fields from community fields
                    object[] vntUpdateTargetFldNames = new object[] { 
                        IntegrationConstants.gstrfNBHD_PHASE_ADDR_1, IntegrationConstants.gstrfNBHD_PHASE_ADDR_2,
                        IntegrationConstants.gstrfNBHD_PHASE_ADDR_3, IntegrationConstants.gstrfAREA_CODE,
                        IntegrationConstants.strfCITY, IntegrationConstants.strfCOUNTY_ID, IntegrationConstants.gstrfSTATE,
                        IntegrationConstants.strfCOUNTRY, IntegrationConstants.strfZIP, IntegrationConstants.gstrfPHONE};
                    
                    //Update source fields
                    object[] vntUpdateSourceFldNames = new object[] {
                        IntegrationConstants.strfADDRESS_1, IntegrationConstants.strfADDRESS_2,
                        IntegrationConstants.strfADDRESS_3, IntegrationConstants.gstrfAREA_CODE, 
                        IntegrationConstants.strfCITY, IntegrationConstants.gstrfCOUNTY_ID,
                        IntegrationConstants.gstrfSTATE, IntegrationConstants.strfCOUNTRY, 
                        IntegrationConstants.strfZIP, IntegrationConstants.gstrfPHONE};

                    //Dynamically mapp source field values to target field values
                    for (int i = 0; i < vntUpdateSourceFldNames.Length; i++)
                    {
                        rstPrimary.Fields[vntUpdateTargetFldNames[i]].Value =
                            rstCommunity.Fields[vntUpdateSourceFldNames[i]].Value;
                    }

                    //Set Sales and Construction Managers 
                    util.SetNeighborhoodManagersForPhase(mrsysSystem, rstPrimary, rstCommunity);

                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
         }



    }
}
