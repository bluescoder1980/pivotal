// TODO: Remove any unused methods from this class. Plus any commented out ones...
using System;
using System.Collections.Generic;
using System.Text;

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
    public class Option : IRFormScript
    {
        #region Class-Level Variables
        IRSystem7 mrsysSystem;
        #endregion

        #region IRFormScript Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSystem"></param>
        public virtual void SetSystem(RSystem pSystem)
        {
            mrsysSystem = (IRSystem7)pSystem;
        }

        /// <summary>
        /// Insert of new Lot-related data from SAP into Pivotal CRM
        /// </summary>
        /// <param name="pForm">HBIntLot</param>
        /// <param name="Recordsets"></param>
        /// <param name="ParameterList"></param>
        /// <returns></returns>
        public object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                // Get incoming recordset
                object[] recordsetArray = (object[])Recordsets;
                Recordset rstPrimary = (Recordset)recordsetArray[0];

                // Define and initialize other local variables
                string strErrMsg = string.Empty;
                object vntRecordId = null;

                // Check all required fields have supplied & valid values, and if not, throw the returned error and exit.
                if (this.CheckRequiredFields(rstPrimary, ref strErrMsg) == false)
                {
                    throw new PivotalApplicationException(strErrMsg);
                }

                // Integration Processing
                this.DoOptionLookupLogic(rstPrimary, true);                              

                // Save the new Product record to the database, returning the Product_Id
                vntRecordId = pForm.DoAddFormData(Recordsets, ref ParameterList);

                //Check and log changes to incoming data
                CheckForChangesToOptionConfig(rstPrimary, true);

                // Create a price change history record telling it that this was an insert
                IntegrationUtility util = new IntegrationUtility();
                util.InsertPriceChangeHistory(mrsysSystem, rstPrimary, true, "Option");

                // Return Record Id
                return vntRecordId;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// Update of existing Lot-related data from SAP into Pivotal CRM
        /// </summary>
        /// <param name="pForm">HBIntLot</param>
        /// <param name="Recordsets"></param>
        /// <param name="ParameterList"></param>
        public void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                // Get incoming recordset
                object[] recordsetArray = (object[])Recordsets;
                Recordset rstPrimary = (Recordset)recordsetArray[0];

                // Define and initialize other local variables
                string strErrMsg = string.Empty;

                // Check all required fields have supplied & valid values, and if not, throw the returned error and exit.
                if (this.CheckRequiredFields(rstPrimary, ref strErrMsg) == false)
                {
                    throw new PivotalApplicationException(strErrMsg);
                }
                
                // Integration Processing
                this.DoOptionLookupLogic(rstPrimary, false);                
                
                // Save the updated Product record to the database
                pForm.DoSaveFormData(Recordsets, ref ParameterList);

                //Check and log changes to incoming data
                CheckForChangesToOptionConfig(rstPrimary, false);



            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pForm"></param>
        /// <param name="RecordId"></param>
        /// <param name="ParameterList"></param>
        public void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                pForm.DoDeleteFormData(RecordId, ref ParameterList);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pForm"></param>
        /// <param name="MethodName"></param>
        /// <param name="ParameterList"></param>
        public void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pForm"></param>
        /// <param name="RecordId"></param>
        /// <param name="ParameterList"></param>
        /// <returns></returns>
        public object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                return pForm.DoLoadFormData(RecordId, ref ParameterList);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pForm"></param>
        /// <param name="ParameterList"></param>
        /// <returns></returns>
        public object NewFormData(IRForm pForm, ref object ParameterList)
        {
            try
            {
                return pForm.DoNewFormData(ref ParameterList);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pForm"></param>
        /// <param name="SecondaryName"></param>
        /// <param name="ParameterList"></param>
        /// <param name="Recordset"></param>
        public void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset Recordset)
        {
            try
            {
                pForm.DoNewSecondaryData(SecondaryName, ref ParameterList, Recordset);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
            }
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// Checks all in-bound required fields to see if they have non-null/empty/valid values supplied.
        /// Returns true if all valid/supplied.
        /// Returns false if at least one is invalid - returns false on first failure.
        /// </summary>
        /// <param name="rstPrimary"></param>
        /// <param name="strErrorMessage"></param>
        /// <returns></returns>
        protected virtual bool CheckRequiredFields(Recordset rstPrimary, ref string strErrorMessage)
        {
            try
            {
                // Check Option Number (Form Field: Code_) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCODE_].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Option Number (Form Field: Code_) must be supplied";
                    return false;
                }

                // Check Neighborhood Code (Form Field: External_Source_Community_Id) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_COMMUNITY_ID].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Neighborhood Code (Form Field: External_Source_Community_Id) must be supplied";
                    return false;
                }

                // Check Plan Code (Form Field: Disconnected_1_2_2) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfOPTION_PLANID].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Plan Code (Form Field: Disconnected_1_2_2) must be supplied";
                    return false;
                }

                // Check Margin (Form Field: Disconnected_1_2_8) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfOPTION_MARGIN].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Margin (Form Field: Disconnected_1_2_8) must be supplied";
                    return false;
                }

                // Check Cost (Form Field: Disconnected_1_2_5) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfOPTION_PRICE].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Cost (Form Field: Disconnected_1_2_5) must be supplied";
                    return false;
                }

                // Check Location (Form Field: Option_Available_To) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfOPTION_AVAILABLE_TO].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Location (Form Field: Option_Available_To) must be supplied";
                    return false;
                }

                // If we get this far, then all Required Fields must have valid values supplied, so return true.
                return true;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
            }
        }

        /// <summary>
        /// Return Neighborhood.Region_Id of Neighborhood record with Record Id = vntNeighborhoodId
        /// </summary>
        /// <param name="vntNeighborhoodId"></param>
        /// <returns></returns>
        protected virtual object GetRegionIdFromNeighborhood(object vntNeighborhoodId)
        {
            try
            {
                object vntResult = DBNull.Value;

                if (!(Convert.IsDBNull(vntNeighborhoodId)))
                {
                    vntResult = mrsysSystem.Tables[IntegrationConstants.strtNEIGHBORHOOD].Fields[IntegrationConstants.strfREGION_ID].FindValue(
                        mrsysSystem.Tables[IntegrationConstants.strtNEIGHBORHOOD].Fields[IntegrationConstants.strfNEIGHBORHOOD_ID],
                        vntNeighborhoodId);

                    //If nothing is returned make sure you return a NULL database value
                    if (vntResult == null)
                    {
                        vntResult = DBNull.Value;
                    }
                }

                // Return value
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// Return Neighborhood.Division_Id of Neighborhood record with Record Id = vntNeighborhoodId
        /// </summary>
        /// <param name="vntNeighborhoodId"></param>
        /// <returns></returns>
        protected virtual object GetDivisionIdFromNeighborhood(object vntNeighborhoodId)
        {
            try
            {
                object vntResult = DBNull.Value;

                if (!(Convert.IsDBNull(vntNeighborhoodId)))
                {
                    vntResult = mrsysSystem.Tables[IntegrationConstants.strtNEIGHBORHOOD].Fields[IntegrationConstants.strfDIVISION_ID].FindValue(
                        mrsysSystem.Tables[IntegrationConstants.strtNEIGHBORHOOD].Fields[IntegrationConstants.strfNEIGHBORHOOD_ID],
                        vntNeighborhoodId);

                    //If nothing is returned make sure you return a NULL database value
                    if (vntResult == null)
                    {
                        vntResult = DBNull.Value;
                    }
                }

                // Return value
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// Return Region.External_Source_Id of Region record with Record Id = vntRegionId
        /// </summary>
        /// <param name="vntNeighborhoodId"></param>
        /// <returns></returns>
        protected virtual string GetRegionExternalSourceId(object vntRegionId)
        {
            try
            {
                string strResult = String.Empty;

                if (!(Convert.IsDBNull(vntRegionId)))
                {
                    strResult = TypeConvert.ToString(mrsysSystem.Tables[IntegrationConstants.strtREGION].Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID].FindValue(
                        mrsysSystem.Tables[IntegrationConstants.strtREGION].Fields[IntegrationConstants.strfREGION_ID],
                        vntRegionId)).Trim();
                }

                // Return value
                return strResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// External_Source_Id = Chateau.Region + "-" + Chateau.Option_Number
        /// </summary>
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        protected virtual string CalculateExternalSourceId(Recordset rstPrimary, object vntRegionId)
        {
            try
            {
                // Lookup the Region.External_Source_Id from the Region_Id determined for the current Product record
                string strRegionCode = this.GetRegionExternalSourceId(vntRegionId);

                // Get other fields directly from the Product record
                string strNeighborhoodCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_COMMUNITY_ID].Value).Trim();
                string strPhaseCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfOPTION_PHASEID].Value).Trim();
                string strPlanCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfOPTION_PLANID].Value).Trim();
                string strOptionNumber = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCODE_].Value).Trim();

                //AM2010.08.18 - Logic here was backwards, so simply fixed to 
                //Set "*" when strPhaseCode IS Null or Empty
                // Change strPhaseCode to '*' if it is null/empty
                if (String.IsNullOrEmpty(strPhaseCode))
                {
                    strPhaseCode = "*";
                    //AM2010.08.26 - Also, if phase code is empty then
                    //default Release_Wildcard on NBHDP_Product as well.
                    rstPrimary.Fields[IntegrationConstants.strfRELEASE_WILDCARD].Value = true;
                }

                string strExternalSourceId = TypeConvert.ToString(strRegionCode + "-" +
                                             strNeighborhoodCode + "-" +
                                             strPhaseCode + "-" +
                                             strPlanCode + "-" + 
                                             strOptionNumber).Trim();

                return strExternalSourceId;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rstPrimary"></param>
        /// <param name="blnIsNewRecord"></param>
        public void DoOptionLookupLogic(Recordset rstPrimary, bool blnIsNewRecord)
        {
            try
            {
                object vntRegionId = DBNull.Value;
                string strErrMsg = String.Empty;

                //Utility Class
                IntegrationUtility util = new IntegrationUtility();

                // #################################################################################################################
                // ### 1. Lookup Parent Construction Project - START ###
                // Get supplied External Neighborhood Code / External_Source_Community_Id into a local string variable
                string strExtSrcNeighborhoodCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_COMMUNITY_ID].Value).Trim();
                
                //AM2010.11.03 - Commented out to look up Construction Project instead of Neighborhood
                // Lookup the Neighborhood record matching the supplied external system Neighborhood code/identifier
                //object vntNeighborhoodId = util.FindNeighborhood(mrsysSystem, strExtSrcNeighborhoodCode);

                //if (Convert.IsDBNull(vntNeighborhoodId))
                //{
                //    strErrMsg = "Option Rejected - No Neighborhood record found for the Neighborhood lookup value supplied to the External_Source_Community_Id field";
                //    throw new PivotalApplicationException(strErrMsg);
                //}
                                
                //AM2010.11.03 - Changed lookup logic to use the TIC_Construction_Project

                string strConstrProjName = String.Empty;

                // Lookup the Construction Project record matching the supplied external system Construction code/identifier
                object vntNeighborhoodId = null;
                //Lookup Construction Project by External Community Id to get Construction Project and Neighborhood
                object vntConstrProjId = util.FindConstructionProject(mrsysSystem, strExtSrcNeighborhoodCode, ref strConstrProjName, ref vntNeighborhoodId);

                if(Convert.IsDBNull(vntConstrProjId))
                {
                    strErrMsg = "Option Rejected - No Construction Project record found for the Community Id lookup value supplied to the External_Source_Community_Id field";
                      throw new PivotalApplicationException(strErrMsg);
                }
                else
                {
                    // TIC_Construction_Project_Id was successfully looked-up, so write it to NBHDP_Product.TIC_Construction_Project_Id and
                    //NBHDP_Product.Neighborhood_Id & continue...
                    rstPrimary.Fields[IntegrationConstants.strfNEIGHBORHOOD_ID].Value = vntNeighborhoodId;
                    rstPrimary.Fields[IntegrationConstants.strfTIC_CONSTRUCTION_PROJECT_ID].Value = vntConstrProjId;

                    // #################################################################################################################
                    // ### 2. Lookup Region_Id - START ###
                    // Now that we know the Neighborhood_Id is, we can also look up the Neighborhood's related Region and Division...
                    // Set NBHDP_Product.Region_Id with Neighborhood.Region_Id
                    vntRegionId = this.GetRegionIdFromNeighborhood(vntNeighborhoodId);
                    // Reject Option/throw exception if Region can't be determined
                    if (Convert.IsDBNull(vntRegionId))
                    {
                        strErrMsg = "Option Rejected - Region could not be determined.  Check it is defined on the related Neighborhood record.";
                        throw new PivotalApplicationException(strErrMsg);
                    }
                    else
                    {
                        // Set the NBHDP_Product.Region_Id field value
                        rstPrimary.Fields[IntegrationConstants.strfREGION_ID].Value = vntRegionId;
                    }
                    // ### 2. Lookup Region_Id - END ###
                    // #################################################################################################################

                    // #################################################################################################################
                    // ### 3. Lookup Division_Id - START ###
                    // Set NBHDP_Product.Division_Id with Neighborhood.Division_Id
                    object vntDivisionId = this.GetDivisionIdFromNeighborhood(vntNeighborhoodId);                    
                    // Reject Option/throw exception if Division can't be determined
                    if (Convert.IsDBNull(vntDivisionId))
                    {
                        strErrMsg = "Option Rejected - Division could not be determined.  Check it is defined on the related Neighborhood record.";
                        throw new PivotalApplicationException(strErrMsg);
                    }
                    else
                    {
                        // Set the NBHDP_Product.Division_Id field value
                        rstPrimary.Fields[IntegrationConstants.strfDIVISION_ID].Value = vntDivisionId;
                    }
                    // ### 3. Lookup Division_Id - END ###
                    // #################################################################################################################

                    // #################################################################################################################
                    // ### 4. Lookup NBHD_Phase - START ###
                    // Now try to determine the NBHD_Phase_Id, using the looked-up Neighborhood_Id, and the Phase_Code value supplied
                    // on Disconnected_1_2_1 by Chateau
                    // Get supplied External Nbhd Phase Code / Disconnected_1_2_1 into a local variable
                    // Note: NBHD Phase Code is an optional field, hence the IsNullOrEmpty check below, which means we won't
                    // do the lookup if no Phase Code has been supplied.
                    string strExtSrcPhaseCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfOPTION_PHASEID].Value).Trim();

                    if (!(String.IsNullOrEmpty(strExtSrcPhaseCode)))
                    {

                        //AM2010.11.03 - Changed code to find Phase by Construction Project
                        // If External Nbhd Phase Code is supplied, then try to determined what to set the NBHD_Phase_Id to...
                        // Lookup the NBHD_Phase record which has Neighborhood_Id = Neighborhood determined above AND Phase_Name = strExtSrcPhaseCode
                        //object vntNbhdPhaseId = util.FindNbhdPhase(mrsysSystem, vntNeighborhoodId, strExtSrcPhaseCode);
                        object vntNbhdPhaseId = util.FindNbhdPhaseByConstructionProject(mrsysSystem, vntConstrProjId, strExtSrcPhaseCode);
                        
                        if (Convert.IsDBNull(vntNbhdPhaseId))
                        {
                            strErrMsg = "Option Rejected - No NBHD_Phase (Release Phase) record found for the supplied Construction Project & Phase lookup values";
                            throw new PivotalApplicationException(strErrMsg);
                        }
                        else
                        {
                            // Update Product.NBHD_Phase_Id with the looked-up value (we do allow the Phase to change, unlike Nbhd).
                            rstPrimary.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value = vntNbhdPhaseId;
                        }
                    }
                    // ### 4. Lookup NBHD_Phase - END ###
                    // #################################################################################################################

                    // #################################################################################################################
                    // ### 5. Lookup Plan - START ###
                    // Get supplied Plan Code / Disconnected_1_2_2 into a local variable
                    string strExtSrcPlanCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfOPTION_PLANID].Value).Trim();

                    //AM2010.11.03 - Changed to find plan by Construction Project
                    // Plan Lookup...
                    // Lookup the NBHDP_Product.NBHDP_Product_Id of the NBHDP_Product record with matching Neighborhood_Id and Code_ field 
                    // values, and of Type = "Plan"
                    //object vntPlanId = util.FindPlan(mrsysSystem, vntNeighborhoodId, strExtSrcPlanCode);
                    object vntPlanId = util.FindPlanByConstructionProject(mrsysSystem, vntConstrProjId, strExtSrcPlanCode);

                    if (Convert.IsDBNull(vntPlanId))
                    {
                        strErrMsg = "Option Rejected - No Plan record found for the supplied Constrution Project & Plan lookup values";
                        throw new PivotalApplicationException(strErrMsg);
                    }
                    else
                    {
                        // Set the Plan field value
                        // TODO: Test this is being set properly
                        rstPrimary.Fields[IntegrationConstants.strfPLAN_ID].Value = vntPlanId;
                    }
                    // ### 5. Lookup Plan - END ###
                    // #################################################################################################################

                    // #################################################################################################################
                    // ### 6. Lookup Division_Product - START ###                    
                    // Get supplied Option Number / Code_ into a local variable
                    string strOptionNumber = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCODE_].Value).Trim();
                    // Lookup the Division_Product_Id, looking for a Division_Product record with Region_Id = vntRegionId
                    // and Code_ = strOptionNumber.  If no Id returned, throw an exception.
                    //AM2010.08.19 - Changed the call here to use the new FindDivisionProduct that sets the Type on the NBHDP_Product
                    //from the Division Product Level
                    object vntDivisionProductId = util.FindDivisionProduct(mrsysSystem, vntRegionId, strOptionNumber, rstPrimary);

                    if (Convert.IsDBNull(vntDivisionProductId))
                    {
                        strErrMsg = "Option Rejected - No Option Master (Division_Product) record found for the supplied Region & Option Number lookup values";
                        throw new PivotalApplicationException(strErrMsg);
                    }
                    else
                    {
                        // Set the Division_Product_Id field value
                        // TODO: Test this is being set properly
                        rstPrimary.Fields[IntegrationConstants.strfDIVISION_PRODUCT_ID].Value = vntDivisionProductId;

                        //AM2010.08.19 - Need to get the Type from the Division Product and set it on the NBHDP_Product
                        //in order for the Option Configuration to show up at the Release.


                    }
                    // ### 6. Lookup Division_Product - END ###
                    // #################################################################################################################

                }
                // ### 1. Lookup Parent Neighborhood - END ### 
                // #################################################################################################################

                // TODO: The original MI code does stuff to do with setting the Release_Wildcard & Plan_Wildcard fields.  Do we need to do so?
                // ASM: Need this, but look for blank source Phase instead of +.  If don't set phase, then set Wildcard for Phase.  Set Nbhd and Plan wildcard to False always, noting specific TIC.

                // TODO: The MI code is setting/creating Category(Configuration_Type) and Sub_Category recs at the Option level.  Do we need to do so?  Prob not, as NBHDP_Product.Category_Id is a TLF pulling it down from Division_Product.
                // ASM: No need.
                
                // TODO: The end of the MI DoOptionLookupLogic mentions WC_Level - some big load of code to calc it.  Do we need that?
                // ASM: Do not bring this code in.

                // Insert vs Update of record-specific logic
                if (blnIsNewRecord)
                {
                    // ### NEW RECORD EXTRA LOGIC (INSERT) ###

                    // Set NBDHP_Product.Default_Product = 0/false.
                    rstPrimary.Fields[IntegrationConstants.strfDEFAULT_PRODUCT].Value = false;

                    // Set External_Source_Id (external lookup key) for the new record
                    rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID].Value = this.CalculateExternalSourceId(rstPrimary, vntRegionId);

                    // Set the Option to be Active
                    rstPrimary.Fields[IntegrationConstants.gstrfINACTIVE].Value = false;

                    // Set NBDHP_Product.Margin and Cost_Price for NEW record only, setting 
                    // to supplied Option Margin and Option Price values
                    // TODO: Ask Adam - is me converting NULL to 0 using these TypeConvert.ToDecimal calls a bad idea?  The MI code was doing similar.  Should be OK as these are reqd fields.
                    //AM2010.08.18 - Discussed with with Adam (2010.08.18 - Need to default to 0 for Margin so that Price calculations work correctly).
                    rstPrimary.Fields[IntegrationConstants.strfMARGIN].Value = 0; //TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfOPTION_MARGIN].Value);
                    rstPrimary.Fields[IntegrationConstants.strfCOST].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfOPTION_PRICE].Value);
                    //AM2010.08.18 - Set the new TIC_Cost field to the Cost passed in from Chateau
                    rstPrimary.Fields[IntegrationConstants.strfTIC_COST].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfOPTION_TIC_COST].Value);
                
                }
                else
                {
                    // ### EXISTING RECORD EXTRA LOGIC (UPDATE) ###
                    this.ManagePriceChangesForExistingOption(rstPrimary);
                    rstPrimary.Fields[IntegrationConstants.strfMARGIN].Value = 0; //TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfOPTION_MARGIN].Value);
                    rstPrimary.Fields[IntegrationConstants.strfCOST].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfOPTION_PRICE].Value);
                    //AM2010.08.18 - Set the new TIC_Cost field to the Cost passed in from Chateau
                    rstPrimary.Fields[IntegrationConstants.strfTIC_COST].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfOPTION_TIC_COST].Value);
                
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }            
        }

        /// <summary>
        /// Manages the update of Product.Next_Price, Product.Price_Change_Date and possible
        /// insertion of Price_Change_History records for EXISTING Product records.  This should
        /// never be called from Product records which are yet to be saved for the first time.
        /// </summary>
        /// <param name="rstPrimary"></param>
        protected virtual void ManagePriceChangesForExistingOption(Recordset rstPrimary)
        {
            try
            {
                // Throw an exception if NBHDP_Product.NBHDP_Product_Id is null - we must have a NBHDP_Product_Id for this method to work
                if (Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID].Value))
                {
                    throw new PivotalApplicationException("ManagePriceChangesForExistingOption() can only be called for existing NBHDP_Product/Option records - no NBHDP_Product_Id Record Id was supplied.");
                }
                else
                {
                    // TODO: I may need parts of this, depending on how my discussion with Adam re Option/Pricing/Update logic goes.
                    /*
                    // Get Chateau's Margin and Cost, and Pivotal's current Margin and Cost values into local variables
                    decimal dblExtSrcMargin = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfOPTION_MARGIN].Value);
                    decimal dblExtSrcCost = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfOPTION_PRICE].Value);
                    decimal dblOptionMargin = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfMARGIN].Value);
                    decimal dblOptionCostPrice = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfCOST].Value);

                    // if Chateau.Margin <> Pivotal.Margin OR Chateau.Cost <> Pivotal.Cost...
                    if ((dblExtSrcMargin != dblOptionMargin) || (dblExtSrcCost != dblOptionCostPrice))
                    {
                        // Update the Next_Margin, Next_Cost_Price and Price_Change_Date fields on the Option record
                        rstPrimary.Fields[IntegrationConstants.strfNEXT_MARGIN].Value = rstPrimary.Fields[IntegrationConstants.strfOPTION_MARGIN].Value;
                        rstPrimary.Fields[IntegrationConstants.strfNEXT_COST_PRICE].Value = rstPrimary.Fields[IntegrationConstants.strfOPTION_PRICE].Value;
                        rstPrimary.Fields[IntegrationConstants.strfPRICE_CHANGE_DATE].Value = DateTime.Now;

                        // Create a new unprocessed PCH record for the current date
                        // Create a new PCH record, to record the coming change to the Option's Price
                        util.InsertPriceChangeHistory(mrsysSystem, rstPrimary, false, "Option");
                    }
                    */

                    // Get an instance of the Utility class
                    IntegrationUtility util = new IntegrationUtility();

                    bool blnInsertPriceChangeHistoryRecord = false;
                    bool blnUpdateProductNextPriceAndPriceChangeDate = false;

                    // Get the current NBHDP_Product.Current_Price field value (not from the Form, but from the record in the DB)
                    // TODO: This is getting NBHDP_Product.Current_Price (TLF) rather than Cost_Price - is this OK?
                    decimal dblCurrentPrice = util.FindCurrentPrice(mrsysSystem, rstPrimary, false);

                    // TODO: We never use this variable.  It was present in the MI code, but later code to use it was commented out.  Discuss with Adam.
                    // Get NBHDP_Product.Next_Price field value (not from the Form, but from the record in the DB)
                    // This converts the returned value into a Decimal, so if "null" is returned, then 0 will be the result
                    // This is slightly different from the original MI code, where "null" was replaced with -1, but I suspect we don't need this.
                    // TODO: Test null converts to 0.
                    // TODO: This is getting NBHDP_Product.Next_Price (TLF) rather than Next_Cost_Price - is this OK?
                    //decimal dblNextPrice = TypeConvert.ToDecimal(util.FindNextPrice(mrsysSystem, rstPrimary, false));

                    // Get the Lot Premium value supplied by SAP into a decimal variable
                    // TODO: Test null converts to 0.
                    // TODO: Ask Adam - is me converting NULL to 0 using these TypeConvert.ToDecimal calls a bad idea?  The MI code was doing similar.  Should be OK as these are reqd fields.
                    decimal dblChateauPrice = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfOPTION_PRICE].Value);

                    // Get the set of all PCH records related to the Lot, which are yet to be processed
                    // For Irvine, they are not doing future-pricing, so this *should* always return either 0 records, 
                    // or just maybe one record which has not been updated from non-processed to processed by a 
                    // Scheduled Script because that Script failed for some reason.  The PCH records are returned with
                    // the furthest-in-the-future change coming first.
                    Recordset rstPendingPCH = util.FindPriceChangeHistory(mrsysSystem,
                                                                          rstPrimary.Fields[IntegrationConstants.strfNBHDP_PRODUCT_ID].Value, 
                                                                          false);

                    // TODO: Test this returns a recordset when no records, and not just NULL
                    if (rstPendingPCH != null)
                    {
                        if (rstPendingPCH.RecordCount > 0)
                        {
                            // At lest one non-processed PCH records returned...
                            rstPendingPCH.MoveFirst();

                            // if PCH.Price = supplied Chateau Option Price, then...
                            if (TypeConvert.ToDecimal(rstPendingPCH.Fields[IntegrationConstants.strfPRICE].Value) ==
                                dblChateauPrice)
                            {
                                // ...Do nothing
                            }
                            else
                            {
                                // if PCH.Price <> supplied Chateau Option Price, then we will want to insert a new PCH record for the
                                // Chateau Option Price change, but NOT immediately update the NBDHP_Product.Next_Cost_Price, Next_Margin and Price_Change_Date
                                // fields, as we'll assume that some Scheduled Script is going to write to those, using the data
                                // in the PCH record we're looking at here.
                                blnInsertPriceChangeHistoryRecord = true;
                                blnUpdateProductNextPriceAndPriceChangeDate = false;
                            }
                        }
                        else
                        {
                            // 0 non-processed PCH records returned...
                            // If current Product.Price = Lot Premium Price supplied by SAP...
                            // ASM: If no pending PCH, and current price in db = supplied Product.Price, then do nothing - else if different, create a new PCH
                            // TODO: This is likely a mistake, and needs to change
                            if (dblCurrentPrice == dblChateauPrice)
                            {
                                // ...Do nothing
                            }
                            else
                            {
                                // If current NBHDP_Product.Current_Price <> Chateau Option Price (and there are no pending PCH records), then
                                // we will want to insert a new PCH record for the Chateau Option Price Change, and ALSO update
                                // the NBDHP_Product.Next_Cost_Price, Next_Margin and Price_Change_Date fields, as we can assume that the change
                                // being made by Chateau will be the very next price change, and will be processed by the Scheduled
                                // Scripts when they next run (i.e. hopefully today).                                
                                blnInsertPriceChangeHistoryRecord = true;
                                blnUpdateProductNextPriceAndPriceChangeDate = true;
                            }
                        }
                    }

                    // Update the Product.Next_Price and Product.Price_Change_Date, if the logic above dictates...
                    if (blnUpdateProductNextPriceAndPriceChangeDate)
                    {
                        // Update the Next_Margin, Next_Cost_Price and Price_Change_Date fields on the Option record
                        // using data supplied from Chateau and <Now> as the Price Change Date
                        // TODO: Consider adding TypeConvert to these price values
                        //AM2010.08.18 - Margin should always be 0 when creating Price Change History records.
                        rstPrimary.Fields[IntegrationConstants.strfNEXT_MARGIN].Value = 0; // rstPrimary.Fields[IntegrationConstants.strfOPTION_MARGIN].Value;
                        rstPrimary.Fields[IntegrationConstants.strfNEXT_COST_PRICE].Value = rstPrimary.Fields[IntegrationConstants.strfOPTION_PRICE].Value;
                        rstPrimary.Fields[IntegrationConstants.strfPRICE_CHANGE_DATE].Value = DateTime.Now;
                    }

                    // Insert new Price_Change_History record, given that it has been determined that a Price Change will occur
                    // The PCH record should not be marked as processed (i.e. "false" below), as the Scheduled Scripts will process
                    // it eventually.
                    if (blnInsertPriceChangeHistoryRecord)
                    {
                        // Create a new unprocessed PCH record for the current date
                        // Create a new PCH record, to record the coming change to the Option's Price
                        util.InsertPriceChangeHistory(mrsysSystem, rstPrimary, true, "Option");
                    }
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// This method will evaluate each field being sent over from Chateau and 
        /// write a change log record for each field that has changed.
        /// </summary>
        /// <param name="rstPrimary"></param>
        protected virtual void CheckForChangesToOptionConfig(Recordset rstPrimary, bool isNewRecord)
        {
            IntegrationUtility util = new IntegrationUtility();

            //Plan Code
            string planCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfOPTION_PLANID].Value);

             try
             {
                // TIC Cost
                 if (rstPrimary.Fields[IntegrationConstants.strfOPTION_TIC_COST].Value != rstPrimary.Fields[IntegrationConstants.strfTIC_COST].OriginalValue)
                {
                    //Create Change log for TIC Cost
                    util.WriteChangeLogForOption(mrsysSystem,
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCODE_].Value),
                        IntegrationConstants.strfTIC_COST.ToString(),
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfTIC_COST].OriginalValue),
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfOPTION_TIC_COST].Value),
                        "Option Configuration",
                        planCode,
                        isNewRecord);

                }

                // Cost Price
                 if (rstPrimary.Fields[IntegrationConstants.strfOPTION_PRICE].Value != rstPrimary.Fields[IntegrationConstants.strfCOST].OriginalValue)
                {

                    //Create Change log for TIC Cost
                    util.WriteChangeLogForOption(mrsysSystem,
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCODE_].Value),
                        IntegrationConstants.strfCOST.ToString(),
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCOST].OriginalValue),
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfOPTION_PRICE].Value),
                        "Option Configuration",
                        planCode,
                        isNewRecord);
                }

                // Removal Date Price
                 if (rstPrimary.Fields[IntegrationConstants.strfREMOVAL_DATE].Value != rstPrimary.Fields[IntegrationConstants.strfREMOVAL_DATE].OriginalValue)
                {                   

                    //Create Change log for TIC Cost
                    util.WriteChangeLogForOption(mrsysSystem,
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCODE_].Value),
                        IntegrationConstants.strfREMOVAL_DATE.ToString(),
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfREMOVAL_DATE].OriginalValue),
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfREMOVAL_DATE].Value),
                        "Option Configuration",
                        planCode,
                        isNewRecord);
                }

               
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
            }
        }

        



        #endregion
    }
}
