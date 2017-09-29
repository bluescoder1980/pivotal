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
    public class Adjustment : IRFormScript
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
                this.DoAdjustmentLookupLogic(rstPrimary, true);

                // Save the new Product record to the database, returning the Product_Id
                vntRecordId = pForm.DoAddFormData(Recordsets, ref ParameterList);

                //AM2010.08.26 - Need to recalculate the Concessions at the Opportunity level
                


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
                this.DoAdjustmentLookupLogic(rstPrimary, false);

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
                // Check Neighborhood Code (Form Field: Disconnected_1_2_1) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.NEIGHBORHOOD_CODE].Value).Trim())))
                {
                    strErrorMessage = "Adjustment Rejected - Neighborhood Code (Form Field: Disconnected_1_2_1) must be supplied";
                    return false;
                }

                // Check Phase Code (Form Field: Disconnected_1_2_2) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.PHASE_CODE].Value).Trim())))
                {
                    strErrorMessage = "Adjustment Rejected - Phase Code (Form Field: Disconnected_1_2_2) must be supplied";
                    return false;
                }

                // Check Lot Number (Form Field: Disconnected_1_2_3) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.LOT_NUMBER].Value).Trim())))
                {
                    strErrorMessage = "Adjustment Rejected - Lot Number (Form Field: Disconnected_1_2_3) must be supplied";
                    return false;
                }

                // Check Unit (Form Field: Disconnected_1_2_4) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.UNIT].Value).Trim())))
                {
                    strErrorMessage = "Adjustment Rejected - Unit (Form Field: Disconnected_1_2_4) must be supplied";
                    return false;
                }

                // Check Tract (Form Field: Disconnected_1_2_5) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.TRACT].Value).Trim())))
                {
                    strErrorMessage = "Adjustment Rejected - Tract (Form Field: Disconnected_1_2_5) must be supplied";
                    return false;
                }

                // Check Adjustment Type (Form Field: Disconnected_1_2_6) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.ADJUSTMENT_TYPE].Value).Trim())))
                {
                    strErrorMessage = "Adjustment Rejected - Adjustment Type (Form Field: Disconnected_1_2_6) must be supplied";
                    return false;
                }

                // Check Adjustment Amount (Form Field: Adjustment_Amount) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.TableFields.ADJUSTMENT_AMOUNT].Value).Trim())))
                {
                    strErrorMessage = "Adjustment Rejected - Adjustment Amount (Form Field: Adjustment_Amount) must be supplied";
                    return false;
                }

                // Check Sum Field (Form Field: Sum_Field) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.TableFields.SUM_FIELD].Value).Trim())))
                {
                    strErrorMessage = "Adjustment Rejected - Sum Field (Form Field: Sum_Field) must be supplied";
                    return false;
                }

                // Check Apply To (Form Field: Apply_To) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.TableFields.APPLY_TO].Value).Trim())))
                {
                    strErrorMessage = "Adjustment Rejected - Apply To (Form Field: Apply_To) must be supplied";
                    return false;
                }

                // Check Notes (Form Field: Notes) supplied
                //if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.TableFields.NOTES].Value).Trim())))
                //{
                //    strErrorMessage = "Adjustment Rejected - Notes (Form Field: Notes) must be supplied";
                //    return false;
                //}

                // Check Adjustment Code (Form Field: TIC_INT_External_Source_Id) supplied"
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.TableFields.TIC_INT_EXTERNAL_SOURCE_ID].Value).Trim())))
                {
                    strErrorMessage = "Adjustment Rejected - Adjustment Code (Form Field: TIC_INT_External_Source_Id) must be supplied";
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
        /// AM2010.08.26 - Added 
        /// This method does all of the logic for set fields and calling utility methods for incoming 
        /// Adjustment integration
        /// </summary>
        /// <param name="rstPrimary"></param>
        /// <param name="blnIsNewRecord"></param>
        public object DoAdjustmentLookupLogic(Recordset rstPrimary, bool blnIsNewRecord)
        {
            try
            {
                string strErrMsg = String.Empty;
                object vntOpportunityId = null;
                // #################################################################################################################
                // ### 1. Lookup Release_Adjustment_Id & set Adjustment_Type consequently via TLF - START ###
                // TODO: Test this very heavily
                object vntReleaseAdjustmentId = this.GetReleaseAdjustmentId(rstPrimary);

                if (Convert.IsDBNull(vntReleaseAdjustmentId))
                {
                    strErrMsg = "Adjustment Rejected - Unable to determine Release_Adjustment_Id to lookup from the supplied data.  Possible causes - Release_Adjustment Table data has not been defined.";
                    throw new PivotalApplicationException(strErrMsg);
                }
                else
                {
                    // Set Release_Adjustment_Id with the looked-up value; this will also cause 
                    // Opportunity_Adjustment.Adjustment_Type to be set via a TLF which is "[Release_Adjustment_Id]->[Adjustment_Type]".
                    rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.TableFields.RELEASE_ADJUSTMENT_ID].Value = vntReleaseAdjustmentId;
                }
                // ### 1. Lookup Release_Adjustment_Id & set Adjustment_Type consequently via TLF - END ###
                // #################################################################################################################

                if (blnIsNewRecord)
                {
                    // ### NEW RECORD EXTRA LOGIC (INSERT) ###

                    // For new Opportunity_Adjustment records, set Selected = 1
                    rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.TableFields.SELECTED].Value = true;

                    // #################################################################################################################
                    // ### 2. Set Opportunity_Id - START ###
                    vntOpportunityId = this.GetOpportunityId(rstPrimary);

                    if (Convert.IsDBNull(vntOpportunityId))
                    {
                        strErrMsg = "Adjustment Rejected - Unable to determine Opportunity_Id to lookup from supplied data.  Possible causes - no Lot (Product) record matching the supplied data exists, or no active Contract or Reservation or Inventory Quote exists for the Lot.";
                        throw new PivotalApplicationException(strErrMsg);
                    }
                    else
                    {
                        rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.TableFields.OPPORTUNITY_ID].Value = vntOpportunityId;
                    }
                    // ### 2. Set Opportunity_Id - END ###
                    // #################################################################################################################
                }
                else
                {
                    // ### EXISTING RECORD EXTRA LOGIC (UPDATE) ###
                    // amcnab 2010-08-09: No such update-specific logic at this stage
                }

                //Return so that we can recalculate Concessions
                return vntOpportunityId;

            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// See comments within function.
        /// </summary>
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        protected virtual object GetReleaseAdjustmentId(Recordset rstPrimary)
        {
            try
            {
                // TODO: Test VERY heavily.
                //string strAdjustmentType = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.ADJUSTMENT_TYPE].Value).Trim();
                //AM2010.09.13 - Need to ensure that the code is defaulting the adjustment to "Decorator".  This is because
                //of the totals rollup values on the Opportunity record we want to be able to break out the adjustment type buckets
                string strAdjustmentType = "Decorator";
                                
                string strPhaseCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.PHASE_CODE].Value).Trim();
                string strNeighborhoodCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.NEIGHBORHOOD_CODE].Value).Trim();

                // TODO: Test this function, as I added it
                // Initialize return variable to return DBNull by default - i.e. assume nothing found
                object vntResult = DBNull.Value;

                // Get Data Access
                DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                objLib.PermissionIgnored = true;

                // Execute a Query to return the Release_Adjustment record which is a) Active, b) has Adjustment_Type = <string supplied by Chateau>, 
                // c) has related NBHD_Phase whose Phase_Name = <Chateau-supplied string> (should be non-"" by this stage), and d) has Phase related
                // to Neighborhood whose External_Source_Community_Id = <Chateau-supplied string>
                Recordset rstReleaseAdjustment = objLib.GetRecordset("INT - Active Release_Adjustments with Type ? Phase Name ? Ext Nbhd Id ?", 
                                                                     3,
                                                                     strAdjustmentType, strPhaseCode, strNeighborhoodCode,
                                                                     IntegrationConstants.strfRELEASE_ADJUSTMENT_ID);

                if (rstReleaseAdjustment != null)
                {
                    if (rstReleaseAdjustment.RecordCount > 0)
                    {
                        // Return the Release_Adjustment_Id
                        vntResult = rstReleaseAdjustment.Fields[IntegrationConstants.strfRELEASE_ADJUSTMENT_ID].Value;
                    }

                    rstReleaseAdjustment.Close();
                }

                // Return result
                return vntResult;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// Return the Opportunity_Id of any Active Contract or Reservation or Inventory Quote (i.e. Opportunity record)
        /// which is related to the Product/Lot record, which is itself looked up by the Product record whose 
        /// Business_Unit_Lot_Number matches the supplied Nbhd, Phase, Lot, Unit and Tract data.
        /// </summary>
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        protected virtual object GetOpportunityId(Recordset rstPrimary)
        {
            try
            {
                // TODO: TEST ME HEAVILY
                // By default, this method will end-up returning DBNull.  This should be an Opportunity_Id.
                object vntResult = DBNull.Value;

                // Get Nbhd Code, Lot Number, Unit & Tract into local string-converted variables
                string strNeighborhoodCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.NEIGHBORHOOD_CODE].Value).Trim();
                string strLotNumber = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.LOT_NUMBER].Value).Trim();
                string strUnit = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.UNIT].Value).Trim();
                string strTract = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.OpportunityAdjustmentTable.DisconnectedFields.TRACT].Value).Trim();

                // Set Business Unit Lot Number to [Neighborhood Code]-[Lot Number]-[Unit]-[Tract]
                // If a given value is an empty string, it won't matter, we'll just end up with "--" instead of "-123-"
                string strBusinessUnitLotNumber = TypeConvert.ToString(strNeighborhoodCode + "-" +
                                                  strLotNumber + "-" +
                                                  strUnit + "-" +
                                                  strTract).Trim();
              
                // Get utility class instance
                IntegrationUtility util = new IntegrationUtility();

                // Now that we have built the Business Unit Lot Number from the supplied data, attempt to find
                // the Product record with Product.Business_Unit_Lot_Number = strBusinessUnitLotNumber, returning its
                // Product.Product_Id (i.e. record id), if found.  If no Product record found, we should throw an exception.
                object vntProductId = util.FindLot(mrsysSystem, strBusinessUnitLotNumber);

                // If a DBNull vntProductId is returned, then this function will return DBNull
                if (!(Convert.IsDBNull(vntProductId)))
                {
                    // We got a Product.Product_Id back - i.e. Product record with matching Business_Unit_Lot_Number was found.

                    // Get the Opportunity_Id of any Contract or Reservation against the Lot (which is Active)
                    object vntContractOrReservationId = util.FindContractByLot(mrsysSystem, vntProductId);

                    // If a Contract/Reservation found, then write its Opportunity_Id to vntResult, and it will be returned
                    if (!(Convert.IsDBNull(vntContractOrReservationId)))
                    {
                        vntResult = vntContractOrReservationId;
                    }
                    else
                    {
                        // If no Contract/Reservation found, then now get the Opportunity_Id of any 
                        // Inventory Quote against the Lot (which is Active)
                        object vntInventoryQuoteId = util.FindInventoryQuoteByLot(mrsysSystem, vntProductId);

                        if (!(Convert.IsDBNull(vntInventoryQuoteId)))
                        {
                            // If an Inventory Quote is found, then write its Opportunity_Id to vntResult, and it will be returned
                            // If no IQ is found, then DBNull will be returned, and an exception thrown by the caller
                            vntResult = vntInventoryQuoteId;
                        }
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
        /// This method will recalculate the Concessions on the Contract.  Adjustments 
        /// could be added from both the UI and the integration so everytime an 
        /// adjustment is created we need to recalculate all
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns></returns>
        protected virtual void ReCalculateContractConcessions(object contractId)
        {
            Recordset rstOpp = new Recordset();
            StringBuilder sqlText = new StringBuilder();
            string strContractId = mrsysSystem.IdToString(contractId);
            decimal decAdjmentTotals = 0;

            DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

            sqlText.Append("SELECT SUM(OA.ADJUSTMENT_AMOUNT) AS ADJUSTMENT_TOTAL FROM OPPORTUNITY_ADJUSTMENT OA ");
            sqlText.Append("WHERE OA.OPPORTUNITY_ID = " + strContractId);
           
            rstOpp = objLib.GetRecordset(sqlText.ToString());

            if (rstOpp.RecordCount > 0)
            {
                rstOpp.MoveFirst();
                //Get oppportunity Id
                decAdjmentTotals = TypeConvert.ToDecimal(rstOpp.Fields[0].Value);
                rstOpp.Close();

                //Now update Contract
                Recordset rstContract = objLib.GetRecordset(contractId, OpportunityData.OpportunityTableName, IntegrationConstants.strfCONCESSIONS);
                rstContract.Fields[IntegrationConstants.strfCONCESSIONS].Value = decAdjmentTotals;
                objLib.SaveRecordset(OpportunityData.OpportunityTableName, rstContract);

            }

           

        }

        #endregion
    }
}
