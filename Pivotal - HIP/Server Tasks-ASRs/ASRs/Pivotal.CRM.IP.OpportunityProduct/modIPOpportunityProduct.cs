using System;

using Pivotal.Interop.RDALib;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    internal class modOpportunityProduct
    {
        /// <summary>
        /// This module contains all public constants for the Opportunity Product Object.
        /// </summary>
        // tables
        public const string strtOPPORTUNITY = "Opportunity";
        public const string strtLOCATION = "Opp_Product_Location";
        // Opportunity__Product
        public const string strtOPPORTUNITY__PRODUCT = "Opportunity__Product";
        // Fields used in Opportunity__Product
        public const string strfADDED_BY_CHANGE_ORDER_ID = "Added_By_Change_Order_Id";
        public const string strfBUILT_OPTION = "Built_Option";
        public const string strfCODE_ = "Code_";
        public const string strfCONSTRUCTION_STAGE_ID = "Construction_Stage_Id";
        public const string strfCONSTRUCTION_STAGE_ORDINAL = "Construction_Stage_Ordinal";
        public const string strfCUSTOMERINSTRUCTIONS = "CustomerInstructions";
        public const string strfDELTA_BUILT_OPTION = "Delta_Built_Option";
        public const string strfDEPOSIT = "Deposit";
        public const string strfDIVISION_PRODUCT_ID = "Division_Product_Id";
        public const string strfEXTENDED_PRICE = "Extended_Price";
        public const string strfFILTER_VISIBILITY = "Filter_Visibility";
        public const string strfNBHDP_PRODUCT_ID = "NBHDP_Product_Id";
        public const string strfNET_CONFIG = "Net_Config";
        public const string strfOPP_CURRENCY = "Opp_Currency";
        public const string strfOPPORTUNITY__PRODUCT_ID = "Opportunity__Product_Id";
        public const string strfOPPORTUNITY_ID = "Opportunity_Id";
        public const string strfOPPORTUNITY_PRODUCT_PREF_ID = "Opportunity_Product_Pref_Id";
        public const string strfOPTIONNOTES = "OptionNotes";
        public const string strfPREFERENCE = "Preference";
        public const string strfPREFERENCES_LIST = "Preferences_List";
        public const string strfPRICE = "Price";
        public const string strfPRICE_CHANGED = "Price_Changed";
        public const string strfPRODUCT_AVAILABLE = "Product_Available";
        public const string strfPRODUCT_NAME = "Product_Name";
        public const string strfPRODUCT_NUMBER = "Product_Number";
        public const string strfOPTION_SELECTION_SOURCE = "Option_Selection_Source";
        public const string strfQUANTITY = "Quantity";
        public const string strfQUOTED_PRICE = "Quoted_Price";
        public const string strfREMOVED_BY_CHANGE_ORDER_ID = "Removed_by_Change_Order_Id";
        public const string strfRN_CREATE_DATE = "Rn_Create_Date";
        public const string strfRN_CREATE_USER = "Rn_Create_User";
        public const string strfRN_DESCRIPTOR = "Rn_Descriptor";
        public const string strfRN_EDIT_DATE = "Rn_Edit_Date";
        public const string strfRN_EDIT_USER = "Rn_Edit_User";
        public const string strfSELECTED = "Selected";
        public const string strfTICKLE_COUNTER = "Tickle_Counter";
        public const string strfTYPE = "Type";
        public const string strfOPTION_ADDED_BY = "Option_Added_By";
        public const string strfPLAN_BUILT = "Plan_Built";
        // Fields used in Opportunity
        public const string strfLOT_ID = "LOT_ID";
        public const string strfPIPELINE_STAGE = "PIPELINE_STAGE";
        public const string strfSTATUS = "Status";
        // Queries used in Opportunity__Product
        public const string strqOP_PRODUCTS_WITH_OPPORTUNITY_ID = "PA: Op. Products with Opportunity Id ?";
        public const string strqNO_PRICE_FOR_OPP_PRODUCT_OF_OPPORTUNITY = "PA: No Price for Opp_Product of Opportunity ?";
        public const string strqOPP_PRODUCTS_WITH_PRODUCT_ID = "PA: Opp. Products with Product Id ?";
        public const string strqELEVATION_FOR_OPPORTUNITY = "Sys: Elevation for Opportunity ?";
        public const string strqOPP_PROD_FOR_OPPORTUNITY__AND_CATEGORY = "Sys: Opp Prod for Opportunity ? and Category ?";
        public const string strqVALID_OPTIONS_FOR_OPPORTUNITY = "Sys: Valid Options for Opportunity ?";
        public const string strqOPPORTUNITY_PRODUCT_NET_CONFIG__TRUE = "Opportunity Product Net Config = TRUE";
        public const string strqOPPORTUNITY_PRODUCTS_WHERE_OPPORTUNITYPIPELINE_STAGE__QUOTE = "Sys: Opportunity Products where Opportunity.Pipeline Stage = Quote";
        public const string strqOPPORTUNITY_PRODUCTS_WHERE_PLAN_NAME_ID___AND_PIPELINE_STAGE__QUOTE = "Sys: Opportunity Products where Plan Name Id = ? And Pipeline Stage = Quote";
        public const string strqALL_AVAILABLE_OPTIONS = "PA: All Available Options";
        public const string strqALL_SELECTED_OPTIONS = "PA: All Selected Options";
        public const string strqSELECTED_PRODUCTS_FOR_OPP__NBHDPROD = "PA: Selected Products for Opp ? NBHDProd ?";
        public const string strqAVAILABLE_PRODUCTS_FOR_OPP__NBHDPROD = "PA: Available Products for Opp ? NBHDProd ?";
        public const string strqSELECTED_OPTIONS_FOR_OPP = "PA: Selected Options for Opp ?";
        public const string strqOPP_PRODUCT_WITH_NBHD_PRODUCT = "PA: Opp. Product with NBHD Product ?";
        public const string strqOPP_PRODUCT_FOR_OPP__AND_NBHD_PRODUCT = "PA: Opp Product for Opp ? and NBHD Product ?";
        public const string strqSELECTED_OPTIONS_FOR_QUOTE = "Sys: Selected Options for Quote ?";
        public const string strqOPP_PRODUCTS_FOR_OPP__AND_NEIGHBORHOOD_PRODUCT__NOT_PLAN_NOT_ELEVATION = "Sys: Opp Products for Opp ? and Neighborhood Product ? Not Plan Not Elevation";
        public const string strqBUILD_AND_SELECTED_OPTIONS_FOR_QUOTE = "Sys: Build and Selected Options for Quote ?";
        public const string strqALL_SELECTED_AND_BUILD_OPTIONS_FOR_PRODUCT__AND_NOT_SOLD_QUOTE = "Sys: All Selected and Build Options for Product ? and Not Sold Quote";
        public const string strqALL_AVAIL_OPTIONS_FILTERED = "PA: All Avail Options Filtered";
        public const string strqAVAILABLE_OPTIONS_FOR_QUOTE = "SYS: Available Options for Quote ?";

        public const string strfOP_LOC_ATTR_PREF_ID = "OppProd_Loc_Attribute_Pref_Id";
        public const string strfOP_PREF_ID = "Opportunity_Product_Pref_Id";

        // Opp_Product_Location
        public const string strtOPP_PRODUCT_LOCATION = "Opp_Product_Location";
        public const string strfOPP_PRODUCT_LOCATION_ID = "Opp_Product_Location_Id";
        public const string strfPREFERENCE_LIST = "Preference_List";
        public const string strfLOCATION_QUANTITY = "Location_Quantity";
        public const string strfOPP_PRODUCT_ID = "Opportunity_Product_Id";
        public const string strfPLAN_ID = "Plan_Id";
        public const string strfPARENT_PACKAGE_OPPPROD_ID = "Parent_Package_OppProd_Id";
        public const string strqDIV_PRODUCT_FOR_NBHDPRODUCT = "HB: Div Product for NBHD Product ?";
        public const string strqOPP_PROD_LOC_FOR_OPPPRODUCT = "HB: OP Locations for Opprtunity Product?";

        public const string strsPACKAGE = "Package";
        public const string strqOPP_PROD_LOCATION_FOR_PACKAGE = "HB: Opportunity Product Location for Parent Package ?";
        public const string strqOPP_PROD_ATTR_PREF_LOC = "HB: Opp Product Attr Pref for OP Loc?";
        public const string strqOPP_PREF_FOR_ATTRIBUTE = "HB: OP Pref for Attribute?";
        public const string strqOPP_PRODUCT_FOR_PACKAGE = "HB: Opportunity Products for Parent Pacakge ?";

        // Change_Order_Options
        public const string strtCHANGE_ORDER_OPTIONS = "Change_Order_Options";
        // Fields used in Change_Order_Options
        public const string strfCHANGE_ORDER_ID = "Change_Order_Id";
        public const string strfCHANGE_ORDER_OPTIONS_ID = "Change_Order_Options_Id";
        public const string strfCHANGE_ORDER_STATUS = "Change_Order_Status";
        public const string strfOPPORTUNITY_PRODUCT_ID = "Opportunity_Product_Id";
        // Queries used in Change_Order_Options
        public const string strqHB_CHANGE_ORDER_OPTIONS_FOR_SELECTED_CHANGE_ORDER_STATUS = "HB: Change Order Options for Selected Change Order Status";
        public const string strqHB_CHANGE_ORDER_OPTIONS_FOR_UNSELECTED_CHANGE_ORDER_STATUS = "HB: Change Order Options for Unselected Change Order Status";
        public const string strqHB_CHANGE_ORDER_OPTIONS_FOR_CHANGED_CHANGE_ORDER_STATUS = "HB: Change Order Options for Changed Change Order Status";
        // Opportunity_Adjustment
        public const string strtOPPORTUNITY_ADJUSTMENT = "Opportunity_Adjustment";
        // Fields used in Opportunity_Adjustments
        public const string strfOPPORTUNITY_ADJUSTMENT_ID = "Opportunity_Adjustment_Id";
        public const string strfADJUSTMENT_AMOUNT = "Adjustment_Amount";
        public const string strfADJUSTMENT_PERCENTAGE = "Adjustment_Percentage";
        public const string strfADJUSTMENT_TYPE = "Adjustment_Type";
        public const string strfSUM_FIELD = "Sum_Field";
        // Fields used in Opp_Product_Location
        public const string strfLOCATION_ID = "Opp_Product_Location_Id";
        // Queries used in Opp_Product_Location
        public const string strqOPP_LOCATIONS_FOR_OPP = "HB: OP Locations for Opprtunity Product?";
        // Queries used in Change_Order_Options
        public const string strqOPP_ADJUSTS_FOR_QUOTE = "Sys: Opp Adjustments for Quote ?";
        // NBHDP_Product table
        public const string strtNBHDP_PRODUCT = "NBHDP_Product";
        // Location table
        public const string strtLOC_LOCATION = "Location";
        public const string strfLOC_LOCATION_ID = "Location_Id";
        public const string strqSET_LOCATION_DIV_PRODUCT = "HB: Set Location - Div Prod";
        // Script names
        // General Script names
        // These script names will exist in all modules
        public const string strsINTEGRATION = "PAHB Integration";
        // Module specific Script names
        // These script names will exist in all modules
        // If you need to invoke an other client script, declare it as it follows:
        public const string strsOPP_ADJUSTMENT = "PAHB Opportunity Adjustment";
        public const string strsCHANGE_ORDER = "Change_Order";
        public const string strsOPPORTUNITY = "TIC Opportunity";
        

        // Method names
        // Public Method Name and Private procedure name
        // These procedures will exist in all modules
        public const string strmADD_FORM_DATA = "AddFormData";
        public const string strmDELETE_FORM_DATA = "DeleteFormData";
        public const string strmEXECUTE = "Execute";
        public const string strmLOAD_FORM_DATA = "LoadFormData";
        public const string strmNEW_FORM_DATA = "NewFormData";
        public const string strmSAVE_FORM_DATA = "SaveFormData";
        public const string strmSET_SYSTEM = "SetSystem";
        public const string strmNEW_SECONDARY_DATA = "NewSecondaryData";
        public const string strmCALCULATE_TOTALS = "CalculateTotals";
        public const string strmIS_INTEGRATION_ON = "IsIntegrationOn";
        public const string strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE = "NotifyIntegrationOfContractChange";
        public const string strmUPDATE_OPTION_BUILT_FOR_ACTIVE_CUSTOMER_QUOTE = "UpdateOptionBuiltForActiveCustomerQuote";
        public const string strmGET_PLAN_LOCATION_FOR_PRODUCT = "GetPlanLocationsForProduct";
        public const string strmDELETE_ATTR_PREF = "DeleteAttrPreference";
        // Module specific procedure names
        // These procedures will exist only in you module
        public const string strmUPDATE_CHANGE_ORDER = "UpdateChangeOrder";
        // Active form name
        public const string strrGENERIC_CODE = "GenericCode";
        public const string strrLOCATION = "Opportunity Product Location";
        public const string gstrEMPTY_STRING = "";
        // Error Numbers
        // General error numbers
        // These error numbers will exist in all modules
        public const int glngERR_APPDEV_START_NUMBER = -2147221504 + 10000;
        public const int glngERR_START_NUMBER = -2147221504 + 10000;
        public const int glngERR_END_NUMBER = glngERR_START_NUMBER + 99;
        public const int glngERR_NEWFORMDATA_FAILED = glngERR_START_NUMBER + 1;
        public const int glngERR_LOADFORMDATA_FAILED = glngERR_START_NUMBER + 2;
        public const int glngERR_ADDFORMDATA_FAILED = glngERR_START_NUMBER + 3;
        public const int glngERR_SAVEFORMDATA_FAILED = glngERR_START_NUMBER + 4;
        public const int glngERR_DELETEFORMDATA_FAILED = glngERR_START_NUMBER + 5;
        public const int glngERR_EXECUTE_FAILED = glngERR_START_NUMBER + 6;
        public const int glngERR_NEWSECONDARYDATA_FAILED = glngERR_START_NUMBER + 7;
        // Module specific error numbers
        // These error numbers will exist only in your module
        public const int glngERR_GENERIC_CODE = glngERR_START_NUMBER + 20;
        // Shared error numbers
        // These error numbers will exist in all modules
        public const int glngERR_SHARED_START_NUMBER = -2147221504 + 13400;
        public const int glngERR_METHOD_NOT_DEFINED = glngERR_SHARED_START_NUMBER + 1;
        public const int glngERR_PARAMETER_EXPECTED = glngERR_SHARED_START_NUMBER + 2;
        // Language Resources

        // Group Name in LD_Groups table
        public const string strgOPPORTUNITY_PRODUCT = "Opportunity Product";
        // String Names for CODE_GROUP group in LD_Strings table
        public const string strdGENERIC_CODE = "Generic Code";
        // Strings for Errors group in LD_Strings table
        // These LD_Strings will exist in all modules
        public const string strdERROR_ON_ADDING_NEW_RECORD = "Error on Adding New Record";
        public const string strdPARAMETERS_ARE_EXPECTED = "Parameters Are Expected";
        public const string strdNEWFORMDATA_FAILED = "NewFormDataFailed";
        public const string strdNEWSECONDARYDATA_FAILED = "NewSecondaryDataFailed";
        public const string strdDELETEFORMDATA_FAILED = "DeleteFormDataFailed";
        public const string strdADDFORMDATA_FAILED = "AddFormDataFailed";
        public const string strdEXECUTE_FAILED = "ExecuteFailed";
        public const string strdLOADFORMDATA_FAILED = "LoadFormDataFailed";
        public const string strdSAVEFORMDATA_FAILED = "SaveFormDataFailed";
        public const string strdSETSYSTEM_FAILED = "SetSystemFailed";
        // Miscelaneous string contants
        public const string strdINVALID_METHOD = "Invalid Method";
        public const string strPIPELINE_STAGE_QUOTE = "Quote";
    }
}
