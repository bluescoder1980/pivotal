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
    public class DivisionOption : IRFormScript
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

                this.CheckForChangesToOptionMaster(rstPrimary, true);

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

                this.CheckForChangesToOptionMaster(rstPrimary, false);
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
                // Check Option Name (Form Field: Product_Name) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDIV_PRODUCT_NAME].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Option Name (Form Field: Product_Name) must be supplied";
                    return false;
                }

                // Check Category Code (Form Field: Disconnected_1_2_8) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCATEGORY_CODE].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Category Code (Form Field: Disconnected_1_2_8) must be supplied";
                    return false;
                }

                // Check Category Description (Form Field: Disconnected_1_2_2) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCATEGORY_DESC].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Category Description (Form Field: Disconnected_1_2_2) must be supplied";
                    return false;
                }

                // Check Option Number (Form Field: Code_) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCODE_].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Option Number (Form Field: Code_) must be supplied";
                    return false;
                }

                // Check Option Code (Form Field: TIC_Code) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfTIC_CODE].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Option Code (Form Field: TIC_Code) must be supplied";
                    return false;
                }

                // Check Type (Form Field: Type) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfTYPE].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Type (Form Field: Type) must be supplied";
                    return false;
                }

                // Check Region (Form Field: Disconnected_1_2_1) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfREGION_CODE].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Region (Form Field: Disconnected_1_2_1) must be supplied";
                    return false;
                }

                //AM2010.08.26 - Per Adam, Style is not required for Chateau
                // Check Style (Form Field: Style_Number) supplied
                //if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfSTYLE_NUMBER].Value).Trim())))
                //{
                //    strErrorMessage = "Division Option Rejected - Style (Form Field: Style_Number) must be supplied";
                //    return false;
                //}

                //AM2010.08.26 - Not making Manufacturer required for Chateau
                // Check Manufacturer (Form Field: Manufacturer) supplied
                //if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfMANUFACTURER].Value).Trim())))
                //{
                //    strErrorMessage = "Division Option Rejected - Manufacturer (Form Field: Manufacturer) must be supplied";
                //    return false;
                //}

                // Check Units of Measure (Form Field: Units_Of_Measure) supplied
                if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfUNITS_OF_MEASURE].Value).Trim())))
                {
                    strErrorMessage = "Division Option Rejected - Units of Measure (Form Field: Units_Of_Measure) must be supplied";
                    return false;
                }

                //AM2010.08.26 - TIC_Model not required for Chateau
                // Check Model (Form Field: TIC_Model) supplied
                //if (String.IsNullOrEmpty((TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfTIC_MODEL].Value).Trim())))
                //{
                //    strErrorMessage = "Division Option Rejected - Model (Form Field: TIC_Model) must be supplied";
                //    return false;
                //}

                // If we get this far, then all Required Fields must have valid values supplied, so return true.
                return true;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
            }
        }

        /// <summary>
        /// This method does all of the logic for set fields and calling utility methods for incoming 
        /// option integration
        /// </summary>
        /// <param name="rstPrimary"></param>
        /// <param name="blnIsNewRecord"></param>
        public void DoOptionLookupLogic(Recordset rstPrimary, bool blnIsNewRecord)
        {
            try
            {
                string strErrMsg = String.Empty;

                if (blnIsNewRecord)
                {
                    // ### NEW RECORD EXTRA LOGIC (INSERT) ###
                    
                    // For new Division_Product records, set Available_Date = Now()
                    rstPrimary.Fields[IntegrationConstants.strfAVAILABLE_DATE].Value = DateTime.Now;

                    // Set External_Source_Id (external lookup key) for the new record
                    rstPrimary.Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID].Value = this.CalculateExternalSourceId(rstPrimary);

                    // #################################################################################################################
                    // ### 1. Lookup Region_Id - START ###
                    // Set the Region_Id, doing a lookup on the Region table, based on the supplied Chateau Region Code
                    object vntRegionId = this.GetRegionId(TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfREGION_CODE].Value).Trim());
                    // Reject Option/throw exception if Region can't be determined
                    if (Convert.IsDBNull(vntRegionId))
                    {
                        strErrMsg = "Division Option Rejected - Region could not be determined.";
                        throw new PivotalApplicationException(strErrMsg);
                    }
                    else
                    {
                        // Set the Region_Id, doing a lookup on the Region table, based on the supplied Chateau Region Code
                        rstPrimary.Fields[IntegrationConstants.strfREGION_ID].Value = vntRegionId;
                    }
                    // ### 1. Lookup Region_Id - END ###
                    // #################################################################################################################
                }
                else
                {
                    // ### EXISTING RECORD EXTRA LOGIC (UPDATE) ###
                    // amcnab 2010-07-29: No such update-specific logic at this stage
                }

                // Set Division_Product.Category_Id by doing a lookup against the Configuration_Type table.
                // A new Configuration_Type record will be cretaed if the lookup returns 0 records.
                rstPrimary.Fields[IntegrationConstants.strfCATEGORY_ID].Value = this.GetCategoryId(rstPrimary);

                // Set Division_Product.Sub_Category_Id by doing a lookup against the Sub_Category table.
                // A new Sub_Category record will be cretaed if the lookup returns 0 records.
                // If no Category was determined above, this will automatically return DbNull
                rstPrimary.Fields[IntegrationConstants.strfSUB_CATEGORY_ID].Value = this.GetSubCategoryId(rstPrimary);
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }
        }

        /// <summary>
        /// Return Region_Id of Region record with Region.External_Source_Id = strRegionCode
        /// </summary>
        /// <param name="strRegionCode"></param>
        /// <returns></returns>
        protected virtual object GetRegionId(string strRegionCode)
        {
            try
            {
                object vntResult = DBNull.Value;

                if (!(String.IsNullOrEmpty(strRegionCode)))
                {
                    vntResult = mrsysSystem.Tables[IntegrationConstants.strtREGION].Fields[IntegrationConstants.strfREGION_ID].FindValue(
                        mrsysSystem.Tables[IntegrationConstants.strtREGION].Fields[IntegrationConstants.strfEXTERNAL_SOURCE_ID],
                        strRegionCode);

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
        /// External_Source_Id = Chateau.Region + "-" + Chateau.Option_Number
        /// </summary>
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        protected virtual string CalculateExternalSourceId(Recordset rstPrimary)
        {
            try
            {
                string strRegionCode = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfREGION_CODE].Value).Trim();
                string strOptionNumber = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCODE_].Value).Trim();

                string strExternalSourceId = TypeConvert.ToString(strRegionCode + "-" +
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
        /// <returns></returns>
        protected virtual object GetCategoryId(Recordset rstPrimary)
        {
            try
            {
                // Can only proceed with a non-empty Category Code/Disconnected_1_2_8 field value
                if (!(String.IsNullOrEmpty(TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCATEGORY_CODE].Value).Trim())))
                {
                    //Utility Class
                    IntegrationUtility util = new IntegrationUtility();

                    // Return the Record Id of the Configuration_Type record with its Code_ = supplied strCategoryCode.
                    // If no such record is found, then a new Configuration_Type will be created, and the new Record Id returned.
                    return util.FindCategory(mrsysSystem, 
                                             TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCATEGORY_CODE].Value).Trim(),
                                             TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCATEGORY_DESC].Value).Trim(),
                                             TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfTYPE].Value).Trim(), 
                                             true);
                }
                else
                {
                    // If empty strCategoryCode supplied, return null
                    return DBNull.Value;
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
        /// <param name="rstPrimary"></param>
        /// <returns></returns>
        protected virtual object GetSubCategoryId(Recordset rstPrimary)
        {
            try
            {
                // Can only proceed with a non-empty Sub-Category Code/Disconnected_1_2_7 field value and non-null Category_Id
                if ((!(String.IsNullOrEmpty(TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfSUB_CATEGORY_IMPORT_MATCH_KEY].Value).Trim()))) && 
                    (!(Convert.IsDBNull(rstPrimary.Fields[IntegrationConstants.strfCATEGORY_ID].Value))))
                {
                    //Utility Class
                    IntegrationUtility util = new IntegrationUtility();


                    //AM2010.08.18 - Need to pass the correct lookup value for Sub_Category.ImportMatchKey
                    //This is a table-level formula : Configuration_Type_Id --> Import_Match_Key ([Configuration_Type_Name] + [Component]) + Sub_Category.Name
                    //Category_Description + Type + Subcategory_Description
                    string strSubCategoryMatchKey
                        = TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCATEGORY_DESC].Value).Trim() +
                            TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfTYPE].Value).Trim() +
                            TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfSUB_CATEGORY_NAME].Value).Trim();

                    // Return the Record Id of the Configuration_Type record with its Code_ = supplied strCategoryCode.
                    // If no such record is found, then a new Configuration_Type will be created, and the new Record Id returned.
                    return util.FindSubCategory(mrsysSystem,
                                                rstPrimary.Fields[IntegrationConstants.strfCATEGORY_ID].Value,
                                                TypeConvert.ToString(strSubCategoryMatchKey).Trim(), 
                                                TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfSUB_CATEGORY_NAME].Value).Trim(), 
                                                true);
                }
                else
                {
                    // If empty values supplied, return null
                    return DBNull.Value;
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
        protected virtual void CheckForChangesToOptionMaster(Recordset rstPrimary, bool isNewRecord)
        {
            IntegrationUtility util = new IntegrationUtility();
            try
            {
                // Product Name
                if (TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDIV_PRODUCT_NAME].Value)
                    != TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDIV_PRODUCT_NAME].OriginalValue))
                {
                    //Create Change log for Product Name
                    util.WriteChangeLogForOption(mrsysSystem,
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCODE_].Value),
                        IntegrationConstants.strfDIV_PRODUCT_NAME.ToString(),
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDIV_PRODUCT_NAME].OriginalValue),
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDIV_PRODUCT_NAME].Value),
                        "Option Master",
                        null,
                        isNewRecord);

                }

                // Description
                if (TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDESCRIPTION].Value) 
                    != TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDESCRIPTION].OriginalValue))
                {

                    //Create Change log for Description
                    util.WriteChangeLogForOption(mrsysSystem,
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCODE_].Value),
                        IntegrationConstants.strfDESCRIPTION.ToString(),
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDESCRIPTION].OriginalValue),
                        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfDESCRIPTION].Value),
                        "Option Master",
                        null,
                        isNewRecord);
                }

                //// Category
                //if (rstPrimary.Fields[IntegrationConstants.strfREMOVAL_DATE].Value != rstPrimary.Fields[IntegrationConstants.strfREMOVAL_DATE].OriginalValue)
                //{

                //    //Create Change log for TIC Cost
                //    util.WriteChangeLogForOption(mrsysSystem,
                //        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCODE_].Value),
                //        IntegrationConstants.strfREMOVAL_DATE.ToString(),
                //        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfREMOVAL_DATE].OriginalValue),
                //        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfREMOVAL_DATE].Value),
                //        "Option Master",
                //        isNewRecord);
                //}


                //// Sub Category
                //if (rstPrimary.Fields[IntegrationConstants.strfREMOVAL_DATE].Value != rstPrimary.Fields[IntegrationConstants.strfREMOVAL_DATE].OriginalValue)
                //{

                //    //Create Change log for TIC Cost
                //    util.WriteChangeLogForOption(mrsysSystem,
                //        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfCODE_].Value),
                //        IntegrationConstants.strfREMOVAL_DATE.ToString(),
                //        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfREMOVAL_DATE].OriginalValue),
                //        TypeConvert.ToString(rstPrimary.Fields[IntegrationConstants.strfREMOVAL_DATE].Value),
                //        "Option Master",
                //        isNewRecord);
                //}


            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
            }
        }



        #endregion
    }
}
