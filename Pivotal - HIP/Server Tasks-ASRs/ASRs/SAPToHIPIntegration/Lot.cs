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
    public class Lot : IRFormScript
    {
        #region Class-Level Variables
        IRSystem7 mrsysSystem;

        const string LOT_STATUS__AVAILABLE = "Available";
        const string LOT_STATUS__NOT_RELEASED = "Not Released";
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

                // Get Utility Class instance
                IntegrationUtility util = new IntegrationUtility();

                // Define and initialize other local variables
                string strErrMsg = string.Empty;
                object vntRecordId = null;
                object vntNewInventoryQuoteId = null;      // Record Id of new Inventory Quote/Opportunity Record Id

                // Check all required fields have supplied & valid values, and if not, throw the returned error and exit.
                if (this.CheckRequiredFields(rstPrimary, ref strErrMsg) == false)
                {
                    throw new PivotalApplicationException(strErrMsg);
                }
                
                // Do lookups and assign values to other fields
                this.DoLotLookupLogic(rstPrimary, true);
                               

                // Save the new Product record to the database, returning the Product_Id
                vntRecordId = pForm.DoAddFormData(Recordsets, ref ParameterList);

                // Create a new Inventory_Quote for the new Product/Lot record - as long as a Plan has been set.
                // There will be no other IQs, or Contracts or Reservations against this Product 
                // at this time, so we are safe to create a new IQ.
                if (!(Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfPLAN_ID].Value)))
                {
                    // Create a new IQ, getting the Record Id for use further below
                    vntNewInventoryQuoteId = util.UpdateInventoryQuote(mrsysSystem, null, rstPrimary, null);
                    util.UpdateContingencyInformation(mrsysSystem, vntNewInventoryQuoteId, rstPrimary);
                }

                // Create a new Price_Change_History record
                util.InsertPriceChangeHistory(mrsysSystem, rstPrimary, true, "Lot");

                // Create new TIC_INT_SAM_Contract record, passing in the new IQ Record Id, if any
                this.CreateNewTICIntSamContract(vntRecordId,
                                                vntNewInventoryQuoteId, 
                                                DateTime.Now, 
                                                TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfLOT_STATUS].Value));
                                
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

                // Do lookups and assign values to other fields
                this.DoLotLookupLogic(rstPrimary, false);
                
              

                // Save the updated Product record to the database
                pForm.DoSaveFormData(Recordsets, ref ParameterList);



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
                // Check Neighborhood supplied - this is the External Code and NOT the actual Neighborhood_Id Record Id
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfLOT_COMMUNITYID].Value).Trim())))
                {
                    strErrorMessage = "Lot Rejected - Construction Project must be supplied";
                    return false;
                }
                

                //AM2010.11.24 - Because the PM file will be loaded into HIP before any Sales data, we need to remove the constraint on 
                //the Sales Release since the sales relese is in the Sales file.
                //// Check Phase (Release Phase) supplied - this is the External Code and NOT the actual NBHD_Phase_Id Record Id
                //if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfLOT_PHASEID].Value).Trim())))
                //{
                //    strErrorMessage = "Lot Rejected - Release Phase must be supplied";
                //    return false;
                //}

                // Check Lot Premium (Price) supplied
                //if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_PREMIUM].Value).Trim())))
                //{
                //    strErrorMessage = "Lot Rejected - Lot Premium/Price must be supplied";
                //    return false;
                //}

                // Check Release_Date supplied & valid
                //if (Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfRELEASE_DATE].Value))
                //{
                //    strErrorMessage = "Lot Rejected - Release Date must be supplied";
                //    return false;
                //}

                // Check Lot_Number supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfLOT_NUMBER].Value).Trim())))
                {
                    strErrorMessage = "Lot Rejected - Lot Number must be supplied";
                    return false;
                }

                // Check Tract supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfTRACT].Value).Trim())))
                {
                    strErrorMessage = "Lot Rejected - Tract must be supplied";
                    return false;
                }

                // Check Address_1 supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfLOT_ADDRESS_1].Value).Trim())))
                {
                    strErrorMessage = "Lot Rejected - Address 1 must be supplied";
                    return false;
                }

                // Check City supplied 
                //AM2010.10.20 - Not required anymore, need to lookup from the associated Construction Project --> Neighborhood
                //if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCITY].Value).Trim())))
                //{
                //    strErrorMessage = "Lot Rejected - City must be supplied";
                //    return false;
                //}

                // Check State supplied
                //AM2010.10.20 - Not required anymore, need to lookup from the associated Construction Project --> Neighborhood
                //if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.gstrfSTATE].Value).Trim())))
                //{
                //    strErrorMessage = "Lot Rejected - State must be supplied";
                //    return false;
                //}

                // Check Zip supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfZIP].Value).Trim())))
                {
                    strErrorMessage = "Lot Rejected - Zip must be supplied";
                    return false;
                }

                // Check Construction Phase (Phase) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfPHASE].Value).Trim())))
                {
                    strErrorMessage = "Lot Rejected - Construction Phase must be supplied";
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
        /// This method will perform all of the lookup logic.  This method corresponds
        /// to the SetFields method from the VB Codebase.
        /// </summary>
        /// <param name="rstPrimary"></param>
        protected virtual void DoLotLookupLogic(Recordset rstPrimary, bool blnIsNewRecord)
        {
            try
            {
                const string TYPE__HOMESITE = "Homesite";
                const string TYPE__INVENTORY = "Inventory";

                string strErrMsg = String.Empty;
                object vntNewInventoryQuoteId = DBNull.Value;

                // Get an instance of the Utility class
                IntegrationUtility util = new IntegrationUtility();

                // #################################################################################################################
                // ### 1. Lookup Parent Neighborhood - START ###
                // Get supplied External Neighborhood Code / Disconnected_1_2_1 into a local string variable
                //AM2010.10.20 - This is changed now to be the Construction Project code.  The external lookup should be the same
                //since we set the Consruction_Project.External_Source_Community_Id to match the Neighorhood Code
                string strExtSrcConstrProjCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfLOT_COMMUNITYID].Value).Trim();
                // We will try to get the Name of the supplied/looked-up Neighborhood in the variable below.
                string strConstrProjName = String.Empty;

                // Lookup the Construction Project record matching the supplied external system Construction code/identifier
                object vntNeighborhoodId = null;
                object vntConstructionProjectId = util.FindConstructionProject(mrsysSystem, strExtSrcConstrProjCode, 
                    ref strConstrProjName, ref vntNeighborhoodId);

                if (Convert.IsDBNull(vntConstructionProjectId))
                {
                    strErrMsg = "Lot Rejected - No Construction Project record found for the supplied Construction Project lookup value";
                    throw new PivotalApplicationException(strErrMsg);
                }
                else
                {
                    // if Product.TIC_Construction_Project is null, and it should be on all new Product records, then 
                    // update it & the Product.Neighborhood field
                    if (Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfTIC_CONSTRUCTION_PROJECT_ID].Value))
                    {
                        //Set Construction Project
                        rstPrimary.Fields[IntegrationConstants.strfTIC_CONSTRUCTION_PROJECT_ID].Value = vntConstructionProjectId;
                        //Also set Neighborhood Id associated with Construction Project
                        rstPrimary.Fields[IntegrationConstants.strfNEIGHBORHOOD_ID].Value = vntNeighborhoodId;
                        rstPrimary.Fields[IntegrationConstants.strfNEIGHBORHOOD].Value = strConstrProjName;
                    }
                    else
                    {
                        // if Construction Project is not null, and Sequence Sheet has sent a Construction Project which differs from what
                        // is already sent, then throw an error, as this should not be allowed.
                        // TODO: Test what happens if one of these is NULL - does it throw an error?
                        if (!mrsysSystem.EqualIds(rstPrimary.Fields[IntegrationConstants.strfTIC_CONSTRUCTION_PROJECT_ID].Value, vntConstructionProjectId))
                        {
                            strErrMsg = "Lot Rejected - Sequence Sheet is attempting to change the Neighborhood on this Lot record, which is not allowed";
                            throw new PivotalApplicationException(strErrMsg);
                        }
                    }
                }
                // ### 1. Lookup Parent Neighborhood - END ### 
                // #################################################################################################################
                
                // #################################################################################################################
                // ### 2. Lookup NBHD_Phase - START ###
                // Get supplied External Nbhd Phase Code / Disconnected_1_2_2 into a local variable
                string strExtSrcPhaseCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfLOT_PHASEID].Value).Trim();

                // Lookup the NBHD_Phase record which has Construction_Project_Id = Construction_Project_Id determined above AND Phase_Name = strExtSrcPhaseCode
                object vntNbhdPhaseId = util.FindNbhdPhaseByConstructionProject(mrsysSystem, vntConstructionProjectId, strExtSrcPhaseCode);

                if (Convert.IsDBNull(vntNbhdPhaseId))
                {

                    //AM2010.11.24 - Removed constraint on phase lookup since Sales Phase is part of the Sale file
                    //strErrMsg = "Lot Rejected - No NBHD_Phase (Release Phase) record found for the supplied Construction Project & Phase lookup values";
                    //throw new PivotalApplicationException(strErrMsg);
                }
                else
                {
                    // Update Product.NBHD_Phase_Id with the looked-up value (we do allow the Phase to change, unlike Nbhd).
                    rstPrimary.Fields[IntegrationConstants.gstrfNBHD_PHASE_ID].Value = vntNbhdPhaseId;
                }
                // ### 2. Lookup NBHD_Phase - END ###
                // #################################################################################################################

                // #################################################################################################################
                // ### 3. Lookup Plan & Elevation - Only check if supplied field is populated - START ###
                // Get supplied Plan Code / External_Source_Plan_Id into a local variable
                string strExtSrcPlanCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_PLAN_ID].Value).Trim();

                if (!(String.IsNullOrEmpty(strExtSrcPlanCode)))
                {
                    // Plan Lookup...
                    // Lookup the NBHDP_Product.NBHDP_Product_Id of the NBHDP_Product record with matching Construction Project and Code_ field 
                    // values, and of Type = "Plan"                                                           
                    object vntPlanId = util.FindPlanByConstructionProject(mrsysSystem, vntConstructionProjectId, strExtSrcPlanCode);

                    if (Convert.IsDBNull(vntPlanId))
                    {
                        strErrMsg = "Lot Rejected - No Plan record found for the supplied Construction Project & Plan lookup values";
                        throw new PivotalApplicationException(strErrMsg);
                    }
                    else
                    {
                        // Set the Plan field value
                        rstPrimary.Fields[IntegrationConstants.strfPLAN_ID].Value = vntPlanId;
                    
                        // Now that we have a Plan, we can also process the Elevation related to that Plan...
                        // Get supplied Elevation Code / External_Source_Elev_Code into a local variable
                        string strExtSrcElevationCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.gstrfEXT_SOURCE_ELEV_CODE].Value).Trim();

                        if (!(String.IsNullOrEmpty(strExtSrcElevationCode)))
                        {
                            // Elevation Lookup...
                            // Lookup the NBHDP_Product.NBHDP_Product_Id of the NBHDP_Product record with matching 
                            // Plan_Id and Code_ field values, and of Type = "Elevation" - i.e. returns the Elevation
                            // with the supplied Code_ and matching the supplied Plan.
                            object vntElevationId = util.FindElevation(mrsysSystem, vntPlanId, strExtSrcElevationCode);

                            if (Convert.IsDBNull(vntElevationId))
                            {
                                strErrMsg = "Lot Rejected - No Elevation record found for the supplied Plan & Elevation lookup values";
                                throw new PivotalApplicationException(strErrMsg);
                            }
                            else
                            {
                                // Set the Elevation field value
                                rstPrimary.Fields[IntegrationConstants.strfELEVATION_ID].Value = vntElevationId;
                            }
                        }
                        else
                        {
                            // Plan was supplied, but no Elevation was supplied.  This is not allowed.  Raise error.
                            strErrMsg = "Lot Rejected - A Plan Code was found & looked-up successfully, but no Elevation Code was supplied in the External_Source_Elev_Code field - ensure an Elevation is supplied, if also supplying a Plan";
                            throw new PivotalApplicationException(strErrMsg);
                        }                        
                    }
                }
                // ### 3. Lookup Plan & Elevation - END ###
                // #################################################################################################################

                // ### 4. Calculate Lot_Status field value of NEW OR EXISTING Lot/Product record - START ###
                rstPrimary.Fields[IntegrationConstants.strfLOT_STATUS].Value = this.CalculateLotStatus(rstPrimary, blnIsNewRecord);
                // ### 4. Calculate Lot_Status field value of NEW OR EXISTING Lot/Product record - END ###

                // ### 5. Set External_Source_Name to 'SAP' - START ###
                rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_NAME].Value = "Sequence Sheet";
                // ### 5. Set External_Source_Name to 'SAP' - END ###

                if (blnIsNewRecord)
                {
                    // ### NEW RECORD EXTRA LOGIC (INSERT) ###
                    // Calculate Product.Business_Unit_Lot_Number field for NEW record only
                    rstPrimary.Fields[IntegrationConstants.strfBUSINESS_UNIT_LOT_NUM].Value = this.CalculateBusinessUnitLotNumber(rstPrimary);

                    // Default Product.Sales_Manager_Id for NEW record only, setting to related Nbhd's Sales Manager
                    rstPrimary.Fields[IntegrationConstants.gstrfSALES_MANAGER_ID].Value = this.GetSalesManagerId(vntNeighborhoodId);

                    // Default Product.County_Id for NEW record only, setting to related Nbhd Phase's County
                    rstPrimary.Fields[IntegrationConstants.gstrfCOUNTY_ID].Value = this.GetCountyId(vntNbhdPhaseId);

                    //AM2010.10.20 - Default City, State_ and Country from Neighborhood
                    DefaultGeographicInformation(mrsysSystem, vntNeighborhoodId, rstPrimary);

                    // Set Product.Price for NEW record only, setting to supplied Lot Premium value             
                    rstPrimary.Fields[IntegrationConstants.strfPRICE].Value = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_PREMIUM].Value);

                    // Determine whether the newly-supplied Lot has a Plan specified (Product.Plan_Id is not null)
                    if (!(Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfPLAN_ID].Value)))
                    {
                        // Plan is specified - set External_Source_Global_Pln_Flg = 0
                        rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_GLOBAL_PLN_FLG].Value = 0;
                        // As we have a Plan, this means we are also going to create an Inventory Quote, so
                        // we should set Product.Type = "Inventory".  The IQ record itself will be created later (see AddFormData).
                        rstPrimary.Fields[IntegrationConstants.gstrfTYPE].Value = TYPE__INVENTORY;
                    }
                    else
                    {
                        // Plan NOT specified - set External_Source_Global_Pln_Flg = 1
                        rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_GLOBAL_PLN_FLG].Value = 1;
                        // As we do NOT have a Plan, this means we are NOT going to create an Inventory Quote (see AddFormData), so
                        // we should set Product.Type = "Homesite".
                        rstPrimary.Fields[IntegrationConstants.gstrfTYPE].Value = TYPE__HOMESITE;
                    }
                }
                else
                {
                    // ### EXISTING RECORD EXTRA LOGIC (UPDATE) ###

                   
                    this.ManagePriceChangesForExistingProduct(rstPrimary);

                    // Get the Opportunity_Id of any Contract or Reservation against the Lot (which is Active)
                    object vntActiveContractOrReservationId = util.FindContractByLot(mrsysSystem, rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value);
                    // Get the Opportunity_Id of any Inventory Quote against the Lot (which is Active)
                    object vntInventoryQuoteId = util.FindInventoryQuoteByLot(mrsysSystem, rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value);
                                       
                    // Work out if the Plan has changed between prior to save & now...
                    // TODO: Test this if one of these, or both are Null, does it error, or work nicely?
                    // TODO: Test this original value stuff
                    bool blnPlanHasChanged = !mrsysSystem.EqualIds(rstPrimary.Fields[IntegrationConstants.strfPLAN_ID].Value, 
                                                                  rstPrimary.Fields[IntegrationConstants.strfPLAN_ID].OriginalValue);
                    // Reject/exception if Plan has changed, and Lot already has a Contract or Reservation
                    if (blnPlanHasChanged && (!(Convert.IsDBNull(vntActiveContractOrReservationId))))
                    {
                        strErrMsg = "Lot Rejected - Integration is attempting to change the Plan associated with this Lot, and the Lot already has an active Contract or Reservation";
                        throw new PivotalApplicationException(strErrMsg);
                    }

                    // If we have a Plan...
                    if (!(Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfPLAN_ID].Value)))
                    {
                        // If no Contract/Reservation and no IQ, then create a new IQ - 4th param should be false
                        // to indicate no Contract/Reservation
                        if (Convert.IsDBNull(vntActiveContractOrReservationId) && Convert.IsDBNull(vntInventoryQuoteId))
                        {
                            // Create a NEW IQ
                            vntNewInventoryQuoteId = util.UpdateInventoryQuote(mrsysSystem, null, rstPrimary, null);
                            // Given that we have created a new Inventory Quote, set External_Source_Global_Pln_Flg = 0
                            rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_GLOBAL_PLN_FLG].Value = 0;
                            // On insert of a new IQ, also update Product.Type = "Inventory"
                            rstPrimary.Fields[IntegrationConstants.gstrfTYPE].Value = TYPE__INVENTORY;

                        }
                    }

                    // If Inventory Quote ALREADY exists
                    if (!(Convert.IsDBNull(vntInventoryQuoteId)) || !(Convert.IsDBNull(vntActiveContractOrReservationId)))
                    {
                        object vntOppId = util.UpdateInventoryQuote(mrsysSystem, vntInventoryQuoteId, rstPrimary, vntActiveContractOrReservationId);
                                             
                        // For safety, also ensure that set is External_Source_Global_Pln_Flg = 0 when updating an existing IQ.
                        rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_GLOBAL_PLN_FLG].Value = 0;
                        // On insert of a new IQ, also update Product.Type = "Inventory"
                        rstPrimary.Fields[IntegrationConstants.gstrfTYPE].Value = TYPE__INVENTORY;
                    }

                    // Call the function below to determine if the Lot's Status has changed from opening the record
                    // to the save we are about to do.  If so, this function will insert a new TIC_INT_SAM_Contract record.
                    this.ManageLotStatusChangeForExistingProduct(rstPrimary, vntActiveContractOrReservationId, vntInventoryQuoteId, vntNewInventoryQuoteId);



                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// Set Business_Unit_Lot_Number field based on various input values - 
        /// return string value to calling function
        /// </summary>
        /// <param name="rstPrimary"></param>
        protected virtual string CalculateBusinessUnitLotNumber(Recordset rstPrimary)
        {
            try
            {
                // Get Nbhd Code, Lot Number, Unit & Tract into local string-converted variables
                string strNeighborhoodCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfLOT_COMMUNITYID].Value).Trim();
                string strLotNumber = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfLOT_NUMBER].Value).Trim();
                string strUnit = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfUNIT].Value).Trim();
                string strTract = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfTRACT].Value).Trim();

                // Set Business Unit Lot Number to [Neighborhood Code]-[Lot Number]-[Unit]-[Tract]
                // If a given value is an empty string, it won't matter, we'll just end up with "--" instead of "-123-"
                string strBusinessUnitLotNumber = TypeConvert.ToString(strNeighborhoodCode + "-" +
                                                  strLotNumber + "-" +
                                                  strUnit + "-" +
                                                  strTract).Trim();

                return strBusinessUnitLotNumber;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// Return Neighborhood.Sales_Manager_Id field value for Neighborhood record with Neighborhood_Id = vntNeighborhoodId
        /// </summary>
        /// <param name="vntNeighborhoodId"></param>
        /// <returns></returns>
        protected virtual object GetSalesManagerId(object vntNeighborhoodId)
        {
            try
            {                
                object vntResult = DBNull.Value;

                if ((vntNeighborhoodId != null) && (!(Convert.IsDBNull(vntNeighborhoodId))))
                {
                    // Return Neighborhood.Sales_Manager_Id field value for Neighborhood record with Neighborhood_Id = vntNeighborhoodId
                    vntResult = mrsysSystem.Tables[IntegrationConstants.strtNEIGHBORHOOD].Fields[IntegrationConstants.gstrfSALES_MANAGER_ID].FindValue(
                                mrsysSystem.Tables[IntegrationConstants.strtNEIGHBORHOOD].Fields[IntegrationConstants.strfNEIGHBORHOOD_ID], vntNeighborhoodId);

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
        /// This method will default the City, State and Country on the new lot
        /// from the Neighborhood
        /// </summary>
        /// <param name="rSys"></param>
        /// <param name="rst"></param>
        protected virtual void DefaultGeographicInformation(IRSystem7 rSys, object vntNeighborhoodId, Recordset rst)
        {
            // Initialize return variables to return null & empty by default - i.e. assume nothing found
                
            // Get Data Access
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            objLib.PermissionIgnored = true;

            // Execute a Query to return the TIC_Construction_Project record with TIC_Construction_Project.External_Source_Community_Id = strExtSrcConstructionProjCode, 
            // returning TIC_Construction_Project_Id and Name fields
            Recordset rstNeighborhood = objLib.GetRecordset(vntNeighborhoodId, IntegrationConstants.strtNEIGHBORHOOD,
                IntegrationConstants.strfCITY, IntegrationConstants.strfSTATE_, IntegrationConstants.strfCOUNTRY);


            if (rstNeighborhood != null)
            {
                if (rstNeighborhood.RecordCount > 0)
                {
                    rst.Fields[IntegrationConstants.strfCITY].Value = TypeConvert.ToString(rstNeighborhood.Fields[IntegrationConstants.strfCITY].Value);
                    rst.Fields[IntegrationConstants.strfSTATE_].Value = TypeConvert.ToString(rstNeighborhood.Fields[IntegrationConstants.strfSTATE_].Value);
                    rst.Fields[IntegrationConstants.strfCOUNTRY].Value = TypeConvert.ToString(rstNeighborhood.Fields[IntegrationConstants.strfCOUNTRY].Value);
                }
                rstNeighborhood.Close();
            }
        
        }


        /// <summary>
        /// Calculates Product.Lot_Status of new or existing Product table record
        /// </summary>
        /// <param name="rstPrimary"></param>
        /// <param name="blnIsNewRecord"></param>
        /// <returns></returns>
        protected virtual string CalculateLotStatus(Recordset rstPrimary, bool blnIsNewRecord)
        {
            try
            {
                // Get the Release_Date value supplied with the Active Form
                DateTime dtReleaseDate = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfRELEASE_DATE].Value);
                string strCurrentLotStatus = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfLOT_STATUS].Value);

                // if new Product/Lot record OR not a new record AND current Lot_Status = "Available" or "Not Released"...
                // else, return just current lot status value - i.e. don't change it
                if (
                    (blnIsNewRecord) ||
                    ((!blnIsNewRecord) && ((strCurrentLotStatus == LOT_STATUS__AVAILABLE) || (strCurrentLotStatus == LOT_STATUS__NOT_RELEASED)))
                    )
                {
                    if ((DateTime.Compare(dtReleaseDate, TypeConvert.ToDateTime(null)) == 0) ||
                        (DateTime.Compare(dtReleaseDate, DateTime.Now) > 0))
                    {
                        // ...and supplied Release_Date is null OR > Now(), then return "Not Released"
                        return LOT_STATUS__NOT_RELEASED;
                    }
                    else
                    {
                        // ...else return "Available"
                        return LOT_STATUS__AVAILABLE;
                    }
                }
                else
                {
                    // if not a new record, and status is not "Available" or "Not Released", then return current Lot Status value - i.e. no changes.
                    return strCurrentLotStatus;
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// Return Nbhd_Phase.County_Id field value for Nbhd_Phase record with Nbhd_Phase_Id = vntNbhdPhaseId
        /// </summary>
        /// <param name="vntNbhdPhaseId"></param>
        /// <returns></returns>
        protected virtual object GetCountyId(object vntNbhdPhaseId)
        {
            try
            {
                object vntResult = DBNull.Value;

                if ((vntNbhdPhaseId != null) && (!(Convert.IsDBNull(vntNbhdPhaseId))))
                {
                    // Return Nbhd_Phase.County_Id field value for Nbhd_Phase record with Nbhd_Phase_Id = vntNbhdPhaseId
                    vntResult = mrsysSystem.Tables[IntegrationConstants.gstrtNBHD_PHASE].Fields[IntegrationConstants.gstrfCOUNTY_ID].FindValue(
                                mrsysSystem.Tables[IntegrationConstants.gstrtNBHD_PHASE].Fields[IntegrationConstants.gstrfNBHD_PHASE_ID], vntNbhdPhaseId);

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
        /// Manages the update of Product.Next_Price, Product.Price_Change_Date and possible
        /// insertion of Price_Change_History records for EXISTING Product records.  This should
        /// never be called from Product records which are yet to be saved for the first time.
        /// </summary>
        /// <param name="rstPrimary"></param>
        protected virtual void ManagePriceChangesForExistingProduct(Recordset rstPrimary)
        {
            try
            {
                // Throw an exception if Product.Product_Id is null - we must have a Product_Id for this method to work
                if (Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value))
                {
                    throw new PivotalApplicationException("ManagePriceChangesForExistingProduct() can only be called for existing Product/Lot records - no Product_Id Record Id was supplied.");
                }
                else
                {
                    // Get an instance of the Utility class
                    IntegrationUtility util = new IntegrationUtility();

                    bool blnInsertPriceChangeHistoryRecord = false;
                    bool blnUpdateProductNextPriceAndPriceChangeDate = false;

                    // Get the current Product.Price field value (not from the Form, but from the record in the DB)
                    decimal dblCurrentPrice = util.FindCurrentPrice(mrsysSystem, rstPrimary, true);

                    // Get the Lot Premium value supplied by SAP into a decimal variable
                    decimal dblSAPLotPremiumPrice = TypeConvert.ToDecimal(rstPrimary.Fields[IntegrationConstants.strfDISCONNECTED_LOT_PREMIUM].Value);

                    // Get the set of all PCH records related to the Lot, which are yet to be processed
                    // For Irvine, they are not doing future-pricing, so this *should* always return either 0 records, 
                    // or just maybe one record which has not been updated from non-processed to processed by a 
                    // Scheduled Script because that Script failed for some reason.  The PCH records are returned with
                    // the furthest-in-the-future change coming first.
                    Recordset rstPendingPCH = util.FindPriceChangeHistory(mrsysSystem,
                                                                          rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value,
                                                                          true);

                    if (rstPendingPCH != null)
                    {
                        if (rstPendingPCH.RecordCount > 0)
                        {
                            // At lest one non-processed PCH records returned...
                            rstPendingPCH.MoveFirst();

                            // if PCH.Price = Lot Premium Price supplied by SAP, then...
                            if (TypeConvert.ToDecimal(rstPendingPCH.Fields[IntegrationConstants.strfPRICE].Value) ==
                                dblSAPLotPremiumPrice)
                            {
                                // ...Do nothing
                            }
                            else
                            {
                                // if PCH.Price <> SAP Lot Premium Price, then we will want to insert a new PCH record for the
                                // SAP Lot Premium Price change, but NOT immediately update the Product.Next_Price and Product.Price_Change_Date
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
                            // If no pending PCH, and current price in db = supplied Product.Price, then do nothing - else if different, create a new PCH
                            if (dblCurrentPrice == dblSAPLotPremiumPrice)
                            {
                                // ...Do nothing
                            }
                            else
                            {
                                // If current Product.Price <> SAP Lot Premium Price (and there are no pending PCH records), then
                                // we will want to insert a new PCH record for the SAP Lot Premium Price Change, and ALSO update
                                // the Product.Next_Price and Product.Price_Change_Date fields, as we can assume that the change
                                // being made by SAP will be the very next price change, and will be processed by the Scheduled
                                // Scripts when they next run (i.e. hopefully today).                                
                                blnInsertPriceChangeHistoryRecord = true;
                                blnUpdateProductNextPriceAndPriceChangeDate = true;
                            }
                        }
                    }

                    // Update the Product.Next_Price and Product.Price_Change_Date, if the logic above dictates...
                    if (blnUpdateProductNextPriceAndPriceChangeDate)
                    {
                        // Set Next_Price to supplied SAP Lot Premium Price
                        rstPrimary.Fields[IntegrationConstants.strfNEXT_PRICE].Value = dblSAPLotPremiumPrice;
                        // Set Price_Change_Date to today's date/time
                        rstPrimary.Fields[IntegrationConstants.strfPRICE_CHANGE_DATE].Value = DateTime.Now;
                    }

                    // Insert new Price_Change_History record, given that it has been determined that a Price Change will occur
                    // The PCH record should not be marked as processed (i.e. "false" below), as the Scheduled Scripts will process
                    // it eventually.
                    if (blnInsertPriceChangeHistoryRecord)
                    {
                        util.InsertPriceChangeHistory(mrsysSystem, rstPrimary, false, "Lot");
                    }
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }
        
        /// <summary>
        /// Creates a new TIC_INT_SAM_Contract record, returning the Record Id
        /// </summary>
        /// <param name="vntProductId"></param>
        /// <param name="vntOpportunityId"></param>
        /// <param name="dtDateOfBusinessTransaction"></param>
        /// <param name="strLotStatusChangedTo"></param>
        /// <returns></returns>
        protected virtual object CreateNewTICIntSamContract(object vntProductId, object vntOpportunityId, DateTime dtDateOfBusinessTransaction, string strLotStatusChangedTo)
        {
            try
            {
                if (vntProductId != null)
                {
                    object vntNewRecordId = DBNull.Value;

                    //Use this object to get new recordset
                    DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                    //Update fields on Contract
                    object arrFields = new object[] 
                    {
                        IntegrationConstants.TICIntSamContractTable.TableFields.TIC_INT_SAM_CONTRACT_ID, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.BASE_PRICE, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.LOT_PREMIUM, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.PREPLOT_TOTAL, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.OTHER_OPTION_TOTAL, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.INCENTIVE_TOTAL, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.ELEVATION_PREMIUM, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.NEIGHBORHOOD, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.NBHD_PHASE, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.PLAN_, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.ELEVATION, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.UNIT, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.TRACT, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.LOT_NUMBER, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.BUSINESS_UNIT_LOT_NUMBER, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.SALE_DATE, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.PIPELINE_STAGE, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.STATUS, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.PRODUCT_ID, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.STATUS_CHANGE_NUMBER, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.CHANGED_BY, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.CHANGED_ON, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.LOT_STATUS_CHANGED_TO, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.DATE_OF_BUS_TRANSACTION, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.CAUSED_BY_SALE, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.COMMENTS, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.OPPORTUNITY_ID, 
                        IntegrationConstants.TICIntSamContractTable.TableFields.SALES_VALUE
                    };

                    Recordset rstTICIntSamContract = objLib.GetNewRecordset(IntegrationConstants.TICIntSamContractTable.TABLE_NAME, arrFields);

                    rstTICIntSamContract.AddNew(Type.Missing, Type.Missing);

                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.PRODUCT_ID].Value = vntProductId;

                    if (vntOpportunityId != null)
                    {
                        rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.OPPORTUNITY_ID].Value = vntOpportunityId;
                    }

                    // Work out the next Status Change Number value for this Lot/Product
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.STATUS_CHANGE_NUMBER].Value = this.GetNextStatusChangeNumberForLot(vntProductId);
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.DATE_OF_BUS_TRANSACTION].Value = dtDateOfBusinessTransaction;                     
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.CAUSED_BY_SALE].Value = false;
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.CHANGED_ON].Value = DateTime.Now;
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.LOT_STATUS_CHANGED_TO].Value = strLotStatusChangedTo;                    
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.SALES_VALUE].Value = 0;
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.COMMENTS].Value = DBNull.Value;
                    // Get the Employee.Rn_Descriptor, trimming to the length of the target TIC_Changed_By field.
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.CHANGED_BY].Value = this.GetCurrentEmployeeRecordRnDescriptor(rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.CHANGED_BY].DefinedSize);

                    // Other fields in the TIC_INT_SAM_Contract table, which this function doesn't write to.
                    /*
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.BASE_PRICE].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.LOT_PREMIUM].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.PREPLOT_TOTAL].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.OTHER_OPTION_TOTAL].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.INCENTIVE_TOTAL].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.ELEVATION_PREMIUM].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.NEIGHBORHOOD].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.NBHD_PHASE].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.PLAN_].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.ELEVATION].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.UNIT].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.TRACT].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.LOT_NUMBER].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.BUSINESS_UNIT_LOT_NUMBER].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.SALE_DATE].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.PIPELINE_STAGE].Value =
                    rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.STATUS].Value =                
                    */

                    // Save the new record
                    objLib.SaveRecordset(IntegrationConstants.TICIntSamContractTable.TABLE_NAME, rstTICIntSamContract);
                    // Get the new record id
                    vntNewRecordId = rstTICIntSamContract.Fields[IntegrationConstants.TICIntSamContractTable.TableFields.TIC_INT_SAM_CONTRACT_ID].Value;
                    // Clean-up
                    rstTICIntSamContract.Close();
                    // Return the new record id
                    return vntNewRecordId;
                }
                else
                {
                    throw new PivotalApplicationException("CreateNewTICIntSamContract() - Please supply a non-null Product/Lot Record Id");
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vntProductId"></param>
        /// <returns></returns>
        protected virtual int GetNextStatusChangeNumberForLot(object vntProductId)
        {
            try
            {
                if (vntProductId != null)
                {
                    DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    Recordset rst = new Recordset();
                    StringBuilder sqlText = new StringBuilder();
                    int intResult = 1;

                    sqlText.Append("SELECT ((ISNULL(MAX(Status_Change_Number), 0)) + 1) AS NextStatusChangeNumber ");
                    sqlText.Append("FROM TIC_INT_SAM_Contract ");
                    sqlText.Append("WHERE Product_Id = " + mrsysSystem.IdToString(vntProductId));

                    rst = objLib.GetRecordset(sqlText.ToString());

                    if (rst != null)
                    {
                        if (rst.RecordCount > 0)
                        {
                            rst.MoveFirst();
                            intResult = TypeConvert.ToInt32(rst.Fields["NextStatusChangeNumber"].Value);
                        }

                        rst.Close();
                    }

                    return intResult;
                }
                else
                {
                    throw new PivotalApplicationException("GetNextStatusChangeNumberForLot() - Please supply a non-null Product/Lot Record Id");
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// Returns the Employee.Rn_Descriptor for the current User's Employee record.
        /// Returns empty string if no Employee record available.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetCurrentEmployeeRecordRnDescriptor()
        {
            try
            {
                string strResult = String.Empty;

                if (mrsysSystem.UserProfile.EmployeeId != null)
                {
                    strResult = TypeConvert.ToString(mrsysSystem.Tables[IntegrationConstants.strtEMPLOYEE].Fields[IntegrationConstants.gstrfRN_DESCRIPTOR].FindValue(
                                                     mrsysSystem.Tables[IntegrationConstants.strtEMPLOYEE].Fields[IntegrationConstants.strfEMPLOYEE_ID], mrsysSystem.UserProfile.EmployeeId)).Trim();
                    
                }

                return strResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// Returns GetCurrentEmployeeRecordRnDescriptor(), trimming to intLength characters
        /// </summary>
        /// <param name="intLength"></param>
        /// <returns></returns>
        protected virtual string GetCurrentEmployeeRecordRnDescriptor(int intLength)
        {
            try
            {
                string strResult = this.GetCurrentEmployeeRecordRnDescriptor();

                if (!(String.IsNullOrEmpty(strResult)))
                {
                    // If length of Employee.Rn_Descriptor > requested length...
                    if (strResult.Length > intLength)
                    {
                        // ...then trim returned string to requested length
                        strResult = strResult.Substring(0, intLength);
                    }                    
                }

                // return the result
                return strResult;
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
        /// <param name="vntActiveContractOrReservationId"></param>
        /// <param name="vntInventoryQuoteId"></param>
        /// <param name="vntNewInventoryQuoteId"></param>
        protected virtual void ManageLotStatusChangeForExistingProduct(Recordset rstPrimary, object vntActiveContractOrReservationId, object vntInventoryQuoteId, object vntNewInventoryQuoteId)
        {
            try
            {
                // Throw an exception if Product.Product_Id is null - we must have a Product_Id for this method to work
                if (Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value))
                {
                    throw new PivotalApplicationException("ManageLotStatusChangeForExistingProdict() can only be called for existing Product/Lot records - no Product_Id Record Id was supplied.");
                }
                else
                {
                    // Get the current Product.Lot_Status value.  It may have been updated by an earlier call to CalculateLotStatus()
                    string strLotStatus = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfLOT_STATUS].Value);
                    // Get the original value of the Lot_Status prior to opening the record/change.
                    // TODO: Test this OriginalValue stuff works - amcnab had NOT tested this as of Aug.12.2010
                    string strLotStatus_OriginalValue = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfLOT_STATUS].OriginalValue);

                    // Compare the Value and OriginalValue of the Product.Lot_Status field to determine if it has changed 
                    bool blnLotStatusChanged = (strLotStatus != strLotStatus_OriginalValue);

                    if (blnLotStatusChanged)
                    {
                        // Lot Status has changed... we will need to create a new TIC_INT_SAM_Contract record

                        // Send Date of Business Transaction of <Now>, by default, to the new TIC_INT_SAM_Contract record
                        DateTime dtDateOfBusinessTransaction = DateTime.Now;

                        // If Original Lot Status value (at opening of record) was "Not Released", but new Status is "Available", 
                        // then set Date of Business Transaction to the supplied Release Date
                        if ((strLotStatus_OriginalValue == LOT_STATUS__NOT_RELEASED) && (strLotStatus == LOT_STATUS__AVAILABLE))
                        {
                            dtDateOfBusinessTransaction = TypeConvert.ToDateTime(rstPrimary.Fields[IntegrationConstants.strfRELEASE_DATE].Value);
                        }

                        // Determine which of the 3 Opportunity_Ids supplied to this function are NOT null, and send
                        // the first one found to the Opportunity_Id field of the new TIC_INT_SAM_Contract record.
                        object vntOpportunityId = DBNull.Value;

                        if (!(Convert.IsDBNull(vntActiveContractOrReservationId)))
                        {
                            if (Convert.IsDBNull(vntOpportunityId))
                            {
                                vntOpportunityId = vntActiveContractOrReservationId;
                            }
                        }
                        else if (!(Convert.IsDBNull(vntInventoryQuoteId)))
                        {
                            if (Convert.IsDBNull(vntOpportunityId))
                            {
                                vntOpportunityId = vntInventoryQuoteId;
                            }
                        }
                        else if (!(Convert.IsDBNull(vntNewInventoryQuoteId)))
                        {
                            if (Convert.IsDBNull(vntOpportunityId))
                            {
                                vntOpportunityId = vntNewInventoryQuoteId;
                            }
                        }

                        // Create new TIC_INT_SAM_Contract record, passing in the related Opportunity_Id
                        this.CreateNewTICIntSamContract(rstPrimary.Fields[IntegrationConstants.strfPRODUCT_ID].Value, 
                                                        vntOpportunityId, 
                                                        dtDateOfBusinessTransaction, 
                                                        strLotStatus);
                    }
                }
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }


      


        
        
        #endregion
    }
}
