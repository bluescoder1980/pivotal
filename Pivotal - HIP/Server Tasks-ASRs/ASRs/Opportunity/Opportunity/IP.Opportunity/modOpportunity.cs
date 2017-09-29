using System;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;

using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    internal static class modOpportunity
    {
        // Choice strings
        public const string strsINVENTORY = "Inventory";
        public const string strsQUOTE = "Quote";
        public const string strsCANCELLED = "Cancelled";
        public const string strsCONTRACT = "Contract";
        public const string strsCLOSED = "Closed";
        public const string strsNOT_PURSUED = "Not Pursued";
        public const string strsAVAILABLE = "Available";
        public const string strsSTATUS_WON = "Won";
        public const string strsCOMPANY_TYPE_PROSPECT = "Prospect";
        public const string strsCOMPANY_TYPE_CUSTOMER = "Customer";
        public const string strsCUSTOMER = "Customer";
        public const string strsRESERVED = "Reserved";
        public const string strsTRANSFER = "Transfer";
        public const string strsROLLBACK = "Rollback";
        public const string strsSOLD = "Sold";
        public const string strsBUYER = "Buyer";
        public const string strsIN_PROGRESS = "In Progress";
        public const string strsLOST = "Lost";
        public const string strsON_HOLD = "On Hold";
        public const string strsINACTIVE = "Inactive";
        public const string strsCANCEL_REQUEST = "Cancel Request";
        public const int intCO_BUYER_TYPE = 4;
        public const string strsRESERVATION_EXPIRED = "Reservation Expired";
        public const string strsPOST_SALE = "Post Sale";
        public const string strsPOST_SALE_ACCEPTED = "Post Sale Accepted";
        public const string strsACCEPTED = "Accepted";
        public const string strsPOST_BUILD_QUOTE = "Post Build";
        public const string strsPOST_BUILD_ACCEPTED = "Post Build Accepted";
        public const string strsELEVATION = "Elevation";
        public const string strsGREATER_THAN = "Greater Than";
        public const string strsGREATER_THAN_OR_EQUAL_TO = "Greater Than or Equal To";
        public const string strlPOST_BUILD_SALE = "Post Build Sale";
        public const string strsNBHD_PHASE_STATUS_COMING_SOON = "Coming Soon";
        public const string strsLOT_STATUS_NOT_RELEASED = "Not Released";
        public const string strsLOT_TYPE_HOMESITE = "Homesite";
        public const string sGREATER_THAN = "Greater Than";
        public const string sGREATER_THAN_OR_EQUAL_TO = "Greater Than or Equal To";
        public const string strsSALES_REQUEST = "Sales Request";
        public const string strsPACKAGE = "Package";
        //public const string strsRESERVED = "Reserved";
        // form names
        public const string strrHB_OPPORTUNITY_OPTIONS = "HB Opportunity Options";
        public const string strrCHANGE_ORDER_OPTIONS = "HB Change Order Options";
        public const string strrHB_QUICK_QUOTE = "HB Quick Quote";
        public const string strrHB_QUOTE = "HB Quote";
        public const string strrHB_SALE = "HB Sale";
        public const string strrINVENTORY_QUOTE = "HB Inventory Quote";
        public const string strrINVENTORY_QUOTE_SEARCH = "HB Inventory Quote Search";
        public const string strrHB_OPPORTUNITY_PRODUCT = "HB Opportunity Product";
        public const string strrHB_OPPORTUNITY_ADJUSTMENTS = "HB Opportunity Adjustments";
        public const string strrOPPORTUNITY_PRODUCT_LOCATION = "Opportunity Product Location";
        public const string strrLOT_CONFIGURATION = "Lot Configuration";
        public const string strrHB_CONTACT_PROFILE_NBHD = "HB Contact Profile NBHD";
        public const string strrHB_POST_SALE_QUOTE = "HB Post Sale Quote";
        // segment names
        public const string strsegOPTIONS = "Options";
        public const string strsegDEPOSITS = "Deposits";
        public const string strsegLOAN_PROFILES = "Loan Profiles";
        public const string strsegREALTOR = "Realtor";
        public const string strsINVENTORY_QUOTE_SEARCH = "Inventory Quote Search";
        public const string strsAVAILABLE_FILTER = "AvailableFilter";
        public const string strsHIDDEN = "Hidden";
        // Server Script
        public const string strs_ACTIONPLAN = "PAHB Action Plan";
        public const string strs_PRODUCT = "PAHB Price Book";
        public const string strs_SYSTEM = "PAHB System";
        public const string strs_ALERT = "PAHB Alert";
        public const string strs_CONTACT_ACTIVITIES = "PAHB Contact Activity";
        public const string strsOPP_ADJUSTMENT = "PAHB Opportunity Adjustment";
        public const string strs_INTEGRATION = "PAHB Integration";
        public const string strsCONTACT_PROFILE_NBHD = "PAHB Contact Profile Neighborhood";
        public const string strsPRICE_CHANGE_HISTORY = "PAHB Price Change History";
        public const string strsOP_ATTR_PREF = "PAHB Opportunity Product Attribute Preference";
        public const string strsMILESTONE_ITEMS = "PAHB Milestone Item";
        public const string strsOPPORTUNITY = "PAHB Opportunity";
        public const string strsLOT_CONFIG = "PAHB Lot Configuration";
        // Table Name
        public const string strt_ACTION_PLAN = "Action_Plan";
        public const string strt_ACTION_PLAN_STEP = "Action_Plan_Step";
        public const string strt_ACTION_PLAN_CONTACT_STEP = "Action_Plan_Contact_Step";
        public const string strt_ALERT = "Alert";
        public const string strt_COMPANY = "Company";
        public const string strt_CONTACT = "Contact";
        public const string strt_CONTACT_ACTIVITIES = "Contact_Activities";
        public const string strt_CONTACT_WEB_DETAILS = "Contact_Web_Details";
        public const string strt_EMPLOYEE = "Employee";
        public const string strt_INFLUENCER_INFLUENCE = "Influencer_Influence";
        public const string strt_LOAN_FEE = "Loan_Fee";
        public const string strt_LOAN_SPECIAL_FEE = "Loan_Special_Fee";
        public const string strt_LOAN_SPECIAL_FEE_DETAIL = "Loan_Special_Fee_Detail";
        public const string strt_MILESTONES = "Milestones";
        public const string strt_MILESTONE_ITEMS = "Milestone_Items";
        public const string strtSYSTEM = "System";
        public const string strt_OPPORTUNITY = "Opportunity";
        public const string strt_OPPORTUNITY__PRODUCT = "Opportunity__Product";
        public const string strt_OPPORTUNITY_TEAM_MEMBER = "Opportunity_Team_Member";
        public const string strt_PRICEBOOK = "Price_book";
        public const string strt_QUOTA_ = "Quota_";
        public const string strt_RN_APPOINTMENT = "Rn_Appointments";
        public const string strt_SYSTEM = "System";
        public const string strt_TERRITORY = "Territory";
        public const string strt_TERRITORY_TEAM_MEMBER = "Territory_Team_Member";
        public const string strt_NBHDP_PRODUCT = "NBHDP_Product";
        public const string strtDIVISION = "Division";
        public const string strt_DEPOSIT_SCHEDULE_TEMPL_ITEM = "Deposit_Schedule_Templ_Item";
        // Home Builder changes
        public const string strt_NBHD_PRODUCT = "NBHDP_Product";
        public const string strt_CHANGE_ORDER = "Change_Order";
        public const string strt_NBHD_PHASE = "NBHD_Phase";
        public const string strt_NEIGHBORHOOD = "Neighborhood";
        public const string strt_NEIGHBORHOOD_AGREEMENT = "Neighborhood_Agreement";
        public const string strt_PRODUCT = "Product";
        public const string strt_CONTACT_TEAM_MEMBER = "Contact_Team_Member";
        public const string strt_CONTACT_RELEASE = "Contact_Release";
        public const string strt_NBHD_OPTION_RULES = "NBHDP_Option_Rule";
        public const string strt_OPPORTUNITY_PRODUCT_PREF = "Opportunity_Product_Pref";
        public const string strt_DIVISION_PRODUCT_PREF = "Division_Product_Pref";
        public const string strt_DIVISION = "Division";
        public const string strt_OPPORTUNITY_AGREEMENT = "Opportunity_Agreement";
        public const string strt_DIVISION_PRODUCT = "Division_Product";
        public const string strtCONSTRUCTION_STAGE = "Construction_Stage";
        public const string strtCONTACT_PROFILE_NEIGHBORHOOD = "Contact_Profile_Neighborhood";
        public const string strf_NBHD_PHASE_ID = "NBHD_Phase_Id";
        public const string strf_NBHDP_PRODUCT_ID = "NBHDP_Product_Id";
        public const string strf_PLAN_NAME_ID = "Plan_Name_Id";
        public const string strfWC_LEVEL_WITH_PLAN = "WC_Level_With_Plan";
        // Opp tbl
        public const string strf_PARENT_PRODUCT_ID = "Parent_Product_Id";
        public const string strf_DEFAULT_PRODUCT = "Default_Product";
        public const string strf_DIVISION_ID = "Division_Id";
        public const string strf_DIVISION_PRODUCT_ID = "Division_Product_Id";
        public const string strf_DIVISION_PRODUCT_PREF_ID = "Division_Product_Pref_Id";
        public const string strf_OPPORTUNITY_PRODUCT_ID = "Opportunity_Product_Id";
        public const string strf_OPPORTUNITY__PRODUCT_ID = "Opportunity__Product_Id";
        public const string strf_OPPORTUNITY_PRODUCT_PREF_ID = "Opportunity_Product_Pref_Id";
        public const string strfPPI_MANAGEMENT = "PPI_management";
        public const string strf_CHILD_PARENT_ID = "Child_Product_Id";
        public const string strf_SELECTED = "Selected";
        public const string strfNEIGHBORHOOD_ID = "Neighborhood_Id";
        public const string strfLOT_REQD = "Lot_Required";
        public const string strfSALES_MANAGER_ID = "Sales_Manager_Id";
        public const string strfLOT_ID = "Lot_Id";
        public const string strfLOT_STATUS_TEXT = "Lot_Status_Text";
        public const string strfLOT_STATUS = "Lot_Status";
        public const string strfRELEASE_DATE = "Release_Date";
        public const string strfBLOCK = "Block_";
        public const string strfTRACT = "Tract";
        public const string strfLOT_NUMBER = "Lot_Number";
        public const string strfPLAN_NAME_ID = "Plan_Name_Id";
        public const string strfPHASE = "Phase";
        public const string strfNEIGHBORHOOD = "Neighborhood";
        public const string strfCONFIGURATION_COMPLETE = "Configuration_Complete";
        public const string strfACTUAL_DECISION_DATE = "Actual_Decision_Date";
        public const string strfCURRENT_PRICE = "Current_Price";
        public const string strfPRICE = "Price";
        public const string strfPOST_CUTTOFF_PRICE = "Post_CuttOff_Price";
        public const string strfREALTOR = "Realtor_Id";
        public const string strfLOT_PREMIUM = "Lot_Premium";
        public const string strfBUILT_OPTION = "Built_Option";
        public const string strfFINANCED_OPTIONS = "Financed_Options";
        public const string strfINCLUDE = "Include_";
        public const string strfEXCLUDE = "Exclude";
        public const string strfPARENT_OPTION_ID = "NBHDP_Product_Parent_Option";
        public const string strfCHILD_OPTION_ID = "NBHDP_Product_Child_Option";
        public const string strfCHILD_PRODUCT_ID = "Child_Product_Id";
        public const string strfCHANGE_ORDER_ID = "Change_Order_Id";
        public const string strfADDED_BY_ID = "Added_By_Id";
        public const string strfCHANGE_ORDER_NUMBER = "Change_Order_Number";
        public const string strfCHANGE_ORDER_DATE = "Change_Order_Date";
        public const string strfNOTES = "Notes";
        public const string strfCODE = "Code_";
        public const string strfCOMPONENT_TYPE = "Component_Type";
        public const string strfENV_EDC_PASSWORD = "Env_EDC_Password";
        public const string strfENV_EDC_USERNAME = "Env_EDC_Username";
        public const string strfFIRST_NAME = "First_Name";
        public const string strfLAST_NAME = "Last_Name";
        public const string strfIS_ENVISION_CONTRACT = "Is_Envision_Contract";
        public const string strfENV_ENVISION_ACTIVATED = "Env_Envision_Activated";

        public const string strfCOMPONENT_PRODUCT_ID = "Component_Product_Id";
        public const string strfPARENT_PACK_OPPPROD_ID = "Parent_Package_OppProd_Id";

        public const string strfSTYLE_NUMBER = "Style_Number";
        public const string strfOWNER_ID = "Owner_Id";
        public const string strfOWNER_NAME = "Owner_Name";
        public const string strfELEVATION_ID = "Elevation_Id";
        public const string strfELEVATION_PREMIUM = "Elevation_Premium";
        public const string strfPLAN_ID = "Plan_Id";
        public const string strfDEPOSIT = "Deposit";
        public const string strfCUSTOME_INSTRUCTIONS = "CustomerInstructions";
        public const string strfOPTION_NOTES = "OptionNotes";
        public const string strfPREFERENCE = "Preference";
        public const string strfNET_CONFIG = "Net_Config";
        public const string strfADDED_BY_CHNG_ORDER_ID = "Added_By_Change_Order_Id";
        public const string strfPRODUCT_NAME = "Product_Name";
        public const string strfPREFERENCE_NAME = "Preference_Name";
        public const string strfPREFERENCE_LIST = "Preference_List";
        public const string strfOPTION_ADDED_BY = "Option_Added_By";
        public const string strfDEFAULT_PREFERENCE = "Default_Preference";
        public const string strfDEPOSIT_AMOUNT_TAKEN = "Deposit_Amount_Taken";
        public const string strfREQUIRED_DEPOSIT_AMOUNT = "Required_Deposit_Amount";
        public const string strfMONTHS = "Months";
        public const string strfDID = "DID";
        public const string strfTAX_RELATED = "TaxRelated";
        public const string strfINS_RELATED = "InsRelated";
        public const string strfEND_DATE = "EndDate";
        public const string strfLAST_DATE = "Last_Date";
        public const string strfAVAILABLE = "Available";
        public const string strfCC_FIXED = "CCFixed";
        public const string strfLOT_PRODUCT_ID = "Product_ID";
        public const string strfRESERVATIONEXPIRY = "Reservation_Expiration_Date";
        public const string strfCONSTRUCTION_STAGE_COMPARISON = "Construction_Stage_Comparison";
        public const string strfCONTRACT_APPROVED_SUBMITTED = "Contract_Approved_Submitted";
        public const string strfCONTRACT_APPROVED_SUBMITTED_DATETIME = "Contract_Approved_Datetime";
        public const string strfPIPELINE_STAGE = "Pipeline_Stage";
        public const string strfCANCEL_DECLINED_DATE = "Cancel_Declined_Date";
        public const string strfCANCEL_DECLINED_By = "Cancel_Declined_By";
        public const string strfCANCEL_APPROVED_BY = "Cancel_Approved_By";
        public const string strfPRICE_UPDATE = "Price_Update";
        public const string strfPLAN_BUILT = "Plan_Built";
        public const string strfBUILT_OPTIONS = "BUILT_OPTIONS";
        public const string strfELEVATION_BUILT = "Elevation_Built";
        public const string strfADDITIONAL_PRICE = "Additional_Price";
        public const string strfRESERVED_DATE = "Reserved_Date";
        public const string strfRESERVATION_CONTRACT_ID = "Reservation_Contract_Id";
        public const string strfEST_CONTRACT_CLOSED_DATE = "Est_Contract_Closed_Date";
        public const string strfCONTRACT_APPROVAL_SUBMITTED = "Contract_Approved_Submitted";
        public const string strfCONTRACT_APPROVAL_DATETIME = "Contract_Approved_Datetime";
        public const string strfRESERVATION_AMOUNT = "Reservation_Amount";
        public const string strfUSE_POST_CUTOFF_PRICE = "Use_PCO_Price";
        public const string strfOPTION_SELECTION_SOURCE = "Option_Selection_Source";
        public const string EnvOptionSelectedDatetimeField = "Env_Option_Selected_Datetime";

        public const string strfWALK_IN_DATE = "Walk_In_Date";
        public const string strfWALK_IN_SALE_DATE = "Walk_In_Sale_Date";
        public const string strfTYPE = "Type";
        public const string strfSALE_DATE = "Sale_Date";
        public const string strfCLOSE_DATE = "Close_Date";
        public const string strfCLOSED_DATE = "Closed_Date";
        public const string strfCONTACT_ID = "Contact_Id";
        public const string strfCANCEL_REQUEST_DATE = "Cancel_Request_Date";
        public const string strfCANCEL_DATE = "Cancel_Date";
        public const string strfCANCEL_NOTES = "Cancel_Notes";
        public const string strfACTUAL_REVENUE_DATE = "Actual_Revenue_Date";
        public const string strfDELTA_CANCEL_DATE = "Delta_Cancel_Date";
        public const string strfDELTA_ACT_REV_DATE = "Delta_Actual_Revenue_Date";
        public const string strfSERVICE_DATE = "Service_Date";
        public const string strfWARRANTY_DATE = "Warranty_Date";
        public const string strfQUOTE_CREATE_DATE = "Quote_Create_Date";
        public const string strfQUOTE_CREATE_DATETIME = "Quote_Create_Datetime";
        public const string strfQUOTE_TOTAL = "Quote_Total";
        public const string strfQUOTE_EXPIRY_PERIOD_DAYS = "Quote_Expiry_Period_Days";
        public const string strfLOCATION_ID = "Location_Id";
        public const string strfMANUFACTURER = "Manufacturer";
        public const string strfSTATUS = "Status";
        public const string strfHOME_PHONE = "Phone";
        public const string strfWORK_PHONE = "Work_Phone";
        public const string strfSALES_REQUEST_DATE = "Sale_Request_Date";
        public const string strfFILTER_CONSTRUCTION_STAGE_ONLY = "Filter_Construction_Stage_Only";
        public const string strfFILTER_LOCATION_ID = "Filter_Location_Id";
        public const string strfFILTER_CATEGORY_ID = "Filter_Category_Id";
        public const string strfFILTER_SUB_CATEGORY_ID = "Filter_Sub_Category_Id";
        public const string strfFILTER_MANUFACTURER = "Filter_Manufacturer";
        public const string strfFILTER_CONSTRUCTION_STAGE_ID = "Filter_Constr_Stage_Id";
        public const string strfFILTER_CODE_ = "Filter_Code_";
        public const string strfOPTION_CODE_FILTER = "Option_Code_Filter";
        public const string strfQUOTE_DATE = "Quote_Date";
        public const string strfINVENTORY_MANAGEMENT_ALLOWED = "Inventory_Management_Allowed";
        public const string strfADDRESS_1 = "Address_1";
        public const string strfBUILDING = "Building";
        public const string strfUNIT = "Unit";
        public const string strfJOB_NUMBER = "Job_Number";
        public const string strfPOST_SALE_ID = "Post_Sale_Id";
        public const string strfECOE_DATE = "Ecoe_Date";
        public const string strfCONTRACT_APPROVED_BY_ID = "Contract_Approved_By_Id";
        public const string strfDESCRIPTION = "Description";
        public const string strfSALE_DECLINED_BY = "Sale_Declined_By";
        public const string strfSALE_DECLINED_DATE = "Sale_Declined_Date";
        public const string strfFIRST_VISIT_DATE = "First_Visit_Date";
        public const string strfUNSELECTED_OPTIONS_FOR_LOT = "HB: Unselected Options for Lot ?";
        public const string strfADJUSTMENT_TOTAL = "Concessions";
        public const string strfPPI_ADJUSTMENT_TOTAL = "Total_PPI_Adjustments";
        public const string strfPAGINATION = "Pagination";
        public const string PaginationFieldTitle = "(pages sorted by Option Name)";
        public const string PageCountFieldTitle = "Page Count";
        public const string strfPAGE_COUNT = "Page_Count";
        public const string strfCURRENT_PAGE = "Filter_Current_Page";
        public const string strfSELECT_OPTION_RECORDS_PER_PAGE = "Select_Option_Records_Per_Page";
        public const string strfUNITS_OF_MEASURE = "Units_Of_Measure";
        // opportunity__product
        public const string strfREPLACES_OPTION_ID = "Replaces_Option_Id";
        public const string strfREPLACED_BY_OPTION_ID = "Replaced_By_Option_Id";
        public const string EnvDUNSNumberField = "Env_DUNS_Number";
        public const string EnvGTINField = "Env_GTIN";
        public const string EnvNHTManufacturerNumberField = "Env_NHT_Manufacturer_Number";
        public const string EnvProductBrandField = "Env_Product_Brand";
        public const string EnvProductNumberField = "Env_Product_Number";
        public const string EnvUCCCodeField = "Env_UCCCode";
        public const string EnvManufacturerProductField = "Env_Manufacturer_Product";
        public const string strfPARENT_PACKAGE_OPPPROD_ID = "Parent_Package_OppProdLoc_Id";
        public const string strfOPTION_SELECTED_DATE = "Option_Selected_Date";
        public const string strfOPTION_AVAILABLE_TO = "Option_Available_To";


        public const string strqOPP_PRODUCT_FOR_PACKAGE = "HB: Opportunity Products for Parent Pacakge ?";
        public const string strqOPP_PRODUCT_LOC_FOR_PACKAGE = "HB: Opportunity Product Location for Parent Package ?";

        // Opportunity_Team_Member

        public const string strfOPPORTUNITY_TEAM_MEMBER_ID = "Opportunity_Team_Member_Id";
        // change_order_options
        public const string strfPREVIOUS_EXT_PRICE = "Previous_Ext_Price";
        public const string strfPREVIOUS_PRICE = "Previous_Price";
        public const string strfPREVIOUS_QUANTITY = "Previous_Quantity";
        public const string strfPREVIOUS_OPTION_ID = "Previous_Option_Id";
        public const string strqOPP_PRODUCTS_FOR_OPP_ORIG_ID = "HB: Opp Products for Opp ? and Orig Opp Product Not Defined";
        public const string strqOP_PRODS_FOR_OPP_ORG_NOT_SELECTED = "HB: Opp Products for Opp ? and Orig Opp Product Is Defined and Selected = False";
        public const string strqOP_PRODS_FOR_OPP_ORG_SELECTED = "HB: Opp Products for Opp ? and Orig Opp Product Is Defined and Selected = True";
        public const string strqOP_PRODS_FOR_OPP_DELETED_OR_ADDED = "HB: Deleted or Added Opp Products for Opp ?";

        // Loan
        public const string strfINT_ONLY_TERMS_IN_YEAR = "IntOnlyTermsInYear";
        public const string strfPREPAID_INT_NUM_OF_DAYS = "Prepaid_Int_Num_of_Days";
        public const string strfADJSTARTINGRATE = "AdjStartingRate";
        public const string strfROUND_TO_NEAREST_50 = "Round_To_Nearest_50";
        public const string strfPMI = "PMI";
        public const string strfMIP = "MIP";
        public const string strVA_FUNDING = "VA_Funding";
        // Loan Special Fee
        public const string strfLOAN_SPECIAL_FEE_ID = "Loan_Special_Fee_Id";
        public const string strfEFFECTIVE_DATE = "Effective_Date";
        public const string strfFHA_PARTICIPATION_RATE = "FHA_Participation_Rate";
        public const string strfFHA_MAX_LTV = "FHA_Max_LTV";
        public const string strfVA_MAX_LTV = "VA_Max_LTV";
        // Loan Special Fee
        public const string strfPREMIUM_RATE_15 = "Premium_Rate_15_Yr";
        public const string STRFPREMIUM_RATE = "Premium_Rate";
        public const string strfVETERANS_STATUS = "Veterans_Status";
        public const string strfUSES = "Uses";
        public const string strfLTV_RATIO = "LTV_Ratio";
        public const string strfFUNDING_RATE = "Funding_Rate";


        //TIC_INT_SAM_CONTRACT table
        public const string strtTIC_INT_SAM_CONTRACT = "TIC_INT_SAM_Contract";
        //Fields
        public const string strfTIC_INT_SAM_CONTRACT_ID = "TIC_INT_SAM_Contract_Id";
        public const string strfSTATUS_CHANGE_NUMBER = "Status_Change_Number";
        public const string strfCHANGED_BY = "Changed_By";
        public const string strfCHANGED_ON = "Changed_On";
        public const string strfLOT_STATUS_CHANGED_TO = "Lot_Status_Changed_To";
        public const string strfDATE_OF_BUS_TRANSACTION = "Date_Of_Bus_Transaction";
        public const string strfCAUSED_BY_SALE = "Caused_By_Sale";
        public const string strfCOMMENTS = "Comments";
        public const string strfTRANSFERED_FROM_LOT_ID = "Transfer_From_Lot_Id";
        public const string strfTRANSFERED_TO_LOT_ID = "Transfer_To_Lot_Id";
        public const string strfSALES_VALUE = "Sales_Value";
        //Queries
        public const string strqTIC_CONTRACT_HISTORY_RECORDS_FOR_LOT_ID = "TIC: Contract History Records for Lot Id?";

        //EMPLOYEE table
        public const string strtEMPLOYEE = "Employee";
        public const string strfEMPLOYEE_ID = "Employee_Id";
        public const string strfRN_DESCRIPTOR = "Rn_Descriptor";

        // Disconnected field name of Inventory Quote Search
        public const string strfDIS_STREET = "Street";
        public const string strfDIS_TRACT = "Tract";
        public const string strfDIS_CONSTRUCTION_STAGE = "Construction_Stage";
        public const string strfDIS_DEVELOPMENT_PHASE = "Phase";
        public const string strfDIS_BLOCK = "Block";
        public const string strfDIS_LOT_NUMBER = "Lot_Number";
        public const string strfDIS_BUILDING = "Building";
        public const string strfDIS_UNIT = "Unit";
        public const string strfDIS_JOB_NUMBER = "Job_Number";
        // Queries
        public const string strqCOMPONENT_PRODUCT_FOR_PARENT = "HB: Component Products with Parent Product ?";
        public const string strqDEPOSITS_FOR_QUOTE = "HB: Deposits for Opportunity ?";
        public const string strqACTIVITIES_FOR_QUOTE = "HB: Activities for Opportunity Id?";
        public const string strqTEAM_MEMBERS_FOR_OPP = "Sys: Opportunity Team Member with Opportunity ?";
        public const string strqTEAM_MEMBER_EXISTS_FOR_OPPORTUNITY_EMPLOYEE = "Sys: Team Member Exists For Opportunity? Employee?";
        public const string strqSALES_TEAMS_FOR_CONTACT_NEIGHBORHOOD = "Sys: Sales Teams for Contact? Neighborhood?";
        public const string strqTEAM_MEMBERS_WITH_NOTIFY = "SYS: Opportunity Team Member with cancel request notify for Opportunity ?";
        public const string strqEMPLOYEES_WITH_NOTIFY = "SYS: Opportunity Employees with cancel request notify for Opportunity ?";
        public const string strqCONTRACTS_WHERE_CONTACT = "HB: Contracts where Contact ?";
        public const string strqINVENTORY_QUOTE_SEARCH = "Sys: Inventory Quote Search";
        public const string strqINVENTORY_QUOTE_SEARCH_W_STAGE = "Sys: Inventory Quote w/ Stage Name";
        public const string strqCONTACT_PROFILE_NBHD_FOR_CONTACT = "HB: ContactProfileNBHD for Contact ? and NBHD ?";
        public const string strqINACTIVE_CONTACT_PROFILE_NBHD = "HB: Inactive ContactProfileNBHD for Contact? NBHD?";
        public const string strqLOT_FOR_LOT_ID = "HB: Lot for Lot_ID ?";
        public const string strqACTIVE_CUSTOMER_QUOTES_FOR_LOT = "HB: Active Customer Quotes For Lot_ID ?";
        public const string strqACTIVE_POST_BUILD_QUOTES_FOR_LOT = "HB: Active Post Build Quotes For Lot ?";
        public const string strqOPTIONS_IN_ACTIVE_CUSTOMER_QUOTES_FOR_LOT_NBHDP_PRODUCT = "HB: Options in  Active Customer Qutes For Lot? NBHDP_Product?";
        public const string strqSELECTED_OPTIONS_ELV_ON_QUOTE = "HB: Selected Option of Type Elevation for Quote?";
        public const string strqACTIVE_POST_SALE_QUOTES_FOR_OPP = "HB: Active Post Sale Quotes for Opp ?";
        public const string strqQUOTES_FOR_LOT_NOT_QUOTE = "HB: Quotes for Lot ? Not Quote ?";
        public const string strqEMPLOYEES_WITH_SALES_NOTIFY_CHECKED = "HB: Employees with Sales Notify Checked for Division ?";
        public const string strqACTIVE_QUOTES_FOR_LOT = "HB: Active Quotes for Lot Id ?";
        public const string strqACTIVE_SALES_FOR_HOMESITE = "HB: Active Sales Quote for Homesite?";
        public const string strqLOT_CONFIG_FOR_LOT = "HB: Lot Config for Lot?";
        public const string strqACTIVE_QUOTE_FOR_LOT_NOT_QUOTE = "HB: Active Quotes for Lot ? Not Quote ?";
        public const string strqLATEST_LOAN_SPECIAL_FEE = "HB: Latest Loan Special Fee - Custom SQL";
        public const string strqLOAN_PROFILES_FOR_QUOTE = "HB: Loan Profiles for Quote?";
        public const string strqRESERVED_OR_SALES_REQUEST_QUOTES = "HB: Active Reserved or Sales Request Quotes for NBHD? Contact?";
        public const string strqSEL_OPTIONS_FOR_OPP_NBHDPRODUCT = "HB: Selected Option for Opp? and NBHD Product?";
        public const string strqNBHD_PROFILE_FOR_CONTACT_NBHD = "HB: NBHD Profile for Contact Id? Neighborhood Id?";
        public const string strqPCA = "HB: Post Contract Adjustments";
        public const string strqSELECTED_OPTIONS_FOR_OPP_AND_NBHDP_WITHOUT_PCO = "HB: Selected Option for Opp? and NBHD Product? Without PCO Price";
        public const string strqOPP_PRODUCTS_REFERENCING_ALL_LOCATIONS_WITHOUT_LOCATION_DEFINED_FOR_OPP = "HB: Opp Products Referencing All Locations Without Location Defined For Opp?";
        public const string strqOPTIONS_WITH_DUPLICATE_LOCATIONS_FOR_OPP = "HB: Options with duplicate locations for Opp?";
        public const string strqSINGLE_OPTION_REFERENCING_ALL_LOCATIONS_WITHOUT_LOCATION_DEFINED_FOR_OPTION = "HB: Single Opp Product Referencing All Locations Without Location Defined For OppProd?";
        public const string strqSINGLE_OPTION_WITH_DUPLICATE_LOCATIONS_FOR_OPTION = "HB: Single Opp Product with duplicate locations for OppProd?";

        // Contact
        public const string strtCONTACT = "Contact";
        // Contact Profile Neighborhood
        public const string strfRESERVATION_DATE = "Reservation_Date";
        public const string strfCONTACT_PROFILE_NBHD_ID = "Contact_Profile_NBHD_Id";
        // Contingency
        public const string strtCONTINGENCY = "Contingency";
        public const string strqCONTINGENCY_FOR_QUOTE = "HB: Contingency for Opp ?";
        // Construction Stage
        public const string strfCONSTRUCTION_STAGE_ORD = "Construction_Stage_Ordinal";
        public const string strfCONSTRUCTION_STAGE_NAME = "Construction_Stage_Name";
        public const string strqCONSTRUCTION_STAGES_FOR_RELEASE_STAGE_NUMBER = "HB: Construction Stages for Release? Stage Number?";
        public const string strqQUOTES_WITH_LOTS = "Sys: Quotes with Lot Id ?";
        public const string strtDEPOSIT = "Deposit";
        public const string strfDEPOSIT_ID = "Deposit_Id";
        // fields for the deposit table
        public const string strfDEPOSIT_TYPE = "Type";
        public const string strfDEPOSIT_AMOUNT = "Amount";
        public const string strfDEPOSIT_METHOD_OF_PAYMENT = "Method_Of_Payment";
        public const string strfDEPOSIT_NOTES = "Notes";
        public const string strfDEPOSIT_REFUNDABLE = "Refundable";
        public const string strfDEPOSIT_OPPORTUNITY_ID = "Opportunity_Id";
        public const string strfSCHEDULED_DATE = "Scheduled_Date";
        public const string strfDEPOSIT_SCHEDULE_TEMPLATE_ID = "Deposit_Schedule_Template_Id";
        public const string strfDEP_TEMPL_ITM_DEPOSIT_TYPE = "Type_Of_Deposit";
        public const string strfDEP_TEMPL_ITM_DEPOSIT_AMOUNT = "Deposit_Amount";
        public const string strfDEP_TEMPL_ITM_METHOD_OF_PAYMENT = "Method_Of_Payment";
        public const string strfDEP_TEMPL_ITM_NOTES = "Notes";
        public const string strfDEP_TEMPL_ITM_REFUNDABLE = "Refundable";
        public const string strfOFFSET_APPLY_DATE = "Offset_Apply_Date";
        // Fields used in Neighborhood_agreement
        public const string strfAGREEMENT_NAME = "Agreement_Name";
        public const string strfDIVISION_AGREEMENT_ID = "Agreement_Id";
        public const string strfDIVISION_ID = "Division_Id";
        public const string strfNEIGHBORHOOD_AGREEMENT_ID = "Neighborhood_Agreement_Id";
        public const string strfORDINAL = "Ordinal";
        public const string strfNAME = "Name";
        public const string strfAMOUNT = "Amount";
        public const string strfPCT_LOAN_AMOUNT = "PctLoanAmount";
        public const string strfPCT_SALE_AMOUNT = "PctSaleAmount";
        public const string strfPWM = "PWM";
        public const string strfPAC = "PAC";
        public const string strfIMPOUND = "Impound";
        // Fields used in Opportunity_agreement
        public const string strfMERGED_AGREEMENT = "Merged_Agreement";
        // Fields used in Division
        public const string strfBUYER_IS_GLOBAL_STAGE = "Buyer_Is_Global_Stage";
        // Queries
        public const string strqQUOTES_FOR_LOT_ID = "HB: Quotes for Lot Id ?";
        // Change_Order
        public const string strtCHANGE_ORDER = "Change_Order";
        // Fields used in Change_ORder
        public const string strfADMINISTRATION_FEE = "Administration_Fee";
        public const string strfCHANGE_TYPE = "Change_Type";
        public const string strfNEW_SALES_PRICE = "New_Sales_Price";
        public const string strfTOTAL_ADJUSTMENT = "Total_Sales_Adjustment";
        public const string strfTOTAL_PROJECT_COST = "Total_Project_Cost";
        public const string strfTOTAL_QUOTE_ADJUSTED = "Total_Quote_Adjusted";
        // Queries used in Change_ORder
        public const string strqHB_CHANGE_ORDERS_FOR_OPPORTUNITY = "HB: Change Orders for Opportunity ?";
        // Change_Order_Options
        public const string strtCHANGE_ORDER_OPTIONS = "Change_Order_Options";
        // Fields used in Change_ORder_Options
        public const string strfCHANGE_ORDER_OPTIONS_ID = "Change_Order_Options_Id";
        public const string strfCHANGE_ORDER_STATUS = "Change_Order_Status";
        // Queries used in Change_ORder_Options
        public const string strqHB_CHANGE_ORDER_OPTIONS_FOR_SELECTED_CHANGE_ORDER_STATUS = "HB: Change Order Options for Selected Change Order Status";
        public const string strqHB_CHANGE_ORDER_OPTIONS_FOR_UNSELECTED_CHANGE_ORDER_STATUS = "HB: Change Order Options for Unselected Change Order Status";
        public const string strqHB_CHANGE_ORDER_OPTIONS_FOR_CHANGED_CHANGE_ORDER_STATUS = "HB: Change Order Options for Changed Change Order Status";
        // Change_Order_Adjustment
        public const string strtCHANGE_ORDER_ADJUSTMENT = "Change_Order_Adjustment";
        public const string strfCHANGE_ORDER_ADJUSTMENT_ID = "Change_Order_Adjustment_Id";
        public const string strfPREVIOUS_ADJUSTMENT_PERCENT = "Previous_Adjustment_Percent";
        public const string strfPREVIOUS_ADJUSTMENT_AMOUNT = "Previous_Adjustment_Amount";
        public const string strfPREVIOUS_APPLY_TO = "Previous_Apply_To";
        public const string strfPREVIOUS_ADJUSTMENT_TOTAL = "Previous_Adjustment_Total";
        public const string strfNEW_ADJUSTMENT = "New_Adjustment";
        public const string strfPREVIOUS_ADJUSTMENT_ID = "Previous_Adjustment_Id";
        // Contact
        public const string strqCONTRACTS_FOR_CONTACT = "HB: Contracts for Contact ?";
        // Fields used in Lot__Contact
        public const string strtLOT__CONTACT = "Lot__Contact";
        public const string strfLOT__CONTACT_ID = "Lot__Contact_Id";
        // Queries
        public const string strqASS_CONTACTS_FOR_CONTACT_LOT = "HB: Associated Contacts for Lot and Contact";
        public const string strqLOTS_CONTACTS_FOR_LOT_CONTACT = "HB: Lot Contacts for Lot Id ? and Contact Id?";
        // Company_Opportunity
        public const string strqLENDERS_OF_CONTRACT = "HB: Lenders of Contract?";
        public const string strqTITLE_COMPANIES_OF_CONTRACT = "HB: Title Companies of Contract?";
        public const string strqESCROW_COMPANIES_OF_CONTRACT = "HB: Escrow Companies of Contract?";
        // Division
        // constants for standard options
        public const int intSTANDARD_OPTION_FIXED = 0;
        public const int intSTANDARD_OPTION_FLOATING = 1;
        public const int intBUILD_OPTION_FIXED = 0;
        public const int intBUILD_OPTION_FLOATING = 1;
        public const string strfSTANDARD_OPTION_PRICING = "Standard_Option_Pricing";
        public const string strfBUILD_OPTION_PRICING = "Built_Option_Pricing";
        public const string strfINCLUDE_HOMESITE_PREMIUM = "Include_Homesite_Premium";
        // Division Product
        public const string strtDIVISION_PRODUCT = "Division_Product";
        public const string strfDIVISION_PRODUCT_ID = "Division_Product_Id";
        public const string strfREQUIRED_DEPOSIT_AMT = "Required_Deposit_Amount";
        // Employee NBHD
        public const string strtEMPLOYEE_NBHD = "Employee_NBHD";
        public const string strfDISABLE_STRUCTURAL = "Disable_Structural";
        public const string strfDISABLE_DECORATOR = "Disable_Decorator";
        public const string strfROLE_ID = "Role_Id";
        public const string strqSALES_TEAM_FOR_RELEASE = "Sys: Sales Team for Release ?";
        public const string strqEMP_NBHD_FOR_REL_FOR_EMP = "HB: Employee NBHD for Release ? Employee ?";
        // Product
        public const string strtPRODUCT = "Product";
        // fields used in Product
        public const string strfPRODUCT_ID = "Product_Id";
        public const string strfSALES_DATE = "Sales_Date";
        public const string strfCONTRACT_CLOSE_DATE = "Contract_Close_Date";
        // queries used in product
        public const string strqPRODUCT_WITH_QUOTE_ID = "HB: Product with Quote Id ?";
        // Contact_CoBuyer
        public const string strtCONTACT_COBUYER = "Contact_CoBuyer";
        public const string strfCONTACT_COBUYER_ID = "Contact_CoBuyer_Id";
        public const string strfCO_BUYER_CONTACT_ID = "Co_Buyer_Contact_Id";
        // queries used in ContactCobuyer
        public const string strqCONTACT_COBUYERS_FOR_CONTACT = "HB: Contact CoBuyers for Contact?";
        // Employee
        // Tables

        // Fields
        public const string strfRN_EMPLOYEE_USER_ID = "Rn_Employee_User_Id";
        public const string strqHB_CAN_BE_SALES_REP = "HB: Can be Sales Rep";
        // Loan
        // Tables
        public const string strtLOAN = "Loan";
        // Fields used in Loan
        public const string strfADJ_PERIODS = "Adj_Periods";
        public const string strfADJ_RATE_CAP = "Adj_Rate_Cap";
        public const string strfADJ_RATE_FLOOR = "Adj_Rate_Floor";
        public const string strfBALLOONTERM = "BalloonTerm";
        public const string strfBALLOONUSED = "BalloonUsed";
        public const string strfBOTTOMRATIO = "BottomRatio";
        public const string strfBUYDOWNRATE1 = "BuydownRate1";
        public const string strfBUYDOWNRATE2 = "BuydownRate2";
        public const string strfBUYDOWNRATE3 = "BuydownRate3";
        public const string strfBUYDOWNTERM1 = "BuydownTerm1";
        public const string strfBUYDOWNTERM2 = "BuydownTerm2";
        public const string strfBUYDOWNTERM3 = "BuydownTerm3";
        public const string strfBUYDOWNUSED = "BuydownUsed";
        public const string strfCALL_AFTER = "Call_After";
        public const string strfCCPCTLOAN = "CCPctLoan";
        public const string strfFIRST_ADJ_PERIODS = "First_Adj_Periods";
        public const string strfINACTIVE = "Inactive";
        public const string strfINDEX_NAME = "Index_Name";
        public const string strfINDEX_RATE = "Index_Rate";
        public const string strfINTEREST_RATE = "Interest_Rate";
        public const string strfINTERESTONLY = "InterestOnly";
        public const string strfLENDER_ID = "Lender_Id";
        public const string strfLIFE_RATE_CAP = "Life_Rate_Cap";
        public const string strfLIFE_RATE_FLOOR = "Life_Rate_Floor";
        public const string strfLOAN_ID = "Loan_Id";
        public const string strfLOAN_NAME = "Loan_Name";
        public const string strfLOAN_PRODUCT_ID = "Loan_Product_Id";
        public const string strfMARGIN_RATE = "Margin_Rate";
        public const string strfMAX_LOAN_AMOUNT = "Max_Loan_Amount";
        public const string strfPCTSALEPRICE = "PctSalePrice";
        public const string strfPERIODS = "Periods";
        public const string strfPRIMARYLOAN = "PrimaryLoan";
        public const string strfREGION_ID = "Region_Id";
        public const string strfRN_CREATE_DATE = "Rn_Create_Date";
        public const string strfRN_CREATE_USER = "Rn_Create_User";
        public const string strfRN_EDIT_DATE = "Rn_Edit_Date";
        public const string strfRN_EDIT_USER = "Rn_Edit_User";
        public const string strfTERM = "Term";
        public const string strfTERRITORY_ID = "Territory_Id";
        public const string strfTOPRATIO = "TopRatio";
        public const string strfPCTSALEAMOUNT = "PctSaleAmount";
        // Queries used in Loan
        public const string strqALL_LOANS = "All Loans";
        public const string strqACTIVE_LOANS = "Active Loans";
        public const string strqINACTIVE_LOANS = "Inactive Loans";
        public const string strqHB_LOANS_FOR_DIVISION = "HB: Loans for Division ?";
        // Loan_Profile
        // Tables
        public const string strtLOAN_PROFILE = "Loan_Profile";
        // Fields used in Loan_Profile
        public const string strfBUYER_RATIO = "Buyer_Ratio";
        public const string strfDOWN_PMT = "Down_Pmt";
        public const string strfDOWN_PMT_PCT = "Down_Pmt_Pct";
        public const string strfEST_MTH_PMT = "Est_Mth_Pmt";
        public const string strfLENDER_RATIO = "Lender_Ratio";
        public const string strfLOAN_PROFILE_ID = "Loan_Profile_Id";
        public const string strfLOAN_PROFILE_NAME = "Loan_Profile_Name";
        public const string strfLOAN_PROGRAM_ID = "Loan_Program_Id";
        public const string strfLOAN1_AMT = "Loan1_Amt";
        public const string strfLOAN1_ID = "Loan1_Id";
        public const string strfLOAN1_INT = "Loan1_Int";
        public const string strfLOAN2_AMT = "Loan2_Amt";
        public const string strfLOAN2_ID = "Loan2_Id";
        public const string strfLOAN2_INT = "Loan2_Int";
        public const string strfMTHLY_DEBT = "Mthly_Debt";
        public const string strfMTHLY_INCOME = "Mthly_Income";
        public const string strfPOST_CONTRACT_ADJ = "Post_Contract_Adj";
        public const string strfTOTAL_PRICE = "Total_Price";
        public const string strfXML = "XML";
        // Lot Plan
        public const string strtLOT_PLAN = "Lot_Plan";
        public const string strfLOT_PLAN_ID = "Lot_Plan_Id";
        // Neighborhood
        // Tables
        public const string strtNEIGHBORHOOD = "Neighborhood";
        // Neighborhood_Agreement
        // queries used in Neighborhood_Agreement
        public const string strqNEIGHBORHOOD_AGREEMENTS_FOR_NEIGHBORHOOD = "Neighborhood Agreements for Neighborhood ?";
        // queries used in Neighborhood Product
        public const string strqSTANDARD_OPTIONS_FOR_RELEASE = "Sys: Standard Global Option for Release ?";
        public const string strqSTANDARD_OPTIONS_FOR_RELEASE_NBHD = "Sys: Standard Global Option for Release ? Neighborhood ?";
        public const string strqOPTIONS_PRODUCT_GEOGRAPHY_PLAN_AND_PLANCODE = "HB: Active Occurrence for Product? Region?Division?NBD?Release?Plan?PlanCode?";
        public const string strqOPTIONS_PRODUCT_GEOGRAPHY_PLAN_PLANCODE_ORD_ORD_GREATER = "HB: Active Occ for Product?Region?Division?NBD?Release?Plan?PlanCode?Ord?Ord?Greater";
        public const string strqOPTIONS_PRODUCT_GEOGRAPHY_PLAN_PLANCODE_ORD_ORD_GREATER_EQUAL = "HB: Active Occ for Product?Region?Division?NBD?Release?Plan?PlanCode?Ord?Ord?GreaterThanEqual";
        public const string strtOPPORTUNITY = "Opportunity";

        public const string strtDIVISION_PRODUCT_LOCATIONS = "Division_Product_Locations";
        public const string strfTOTAL_AREA = "Total_Area";

        public const string strqHB_ACTIVE_DIVPROD_LOCATION = "HB: Active DivProd? Location?";

        // Opportunity_Adjustment
        public const string strtOPPORTUNITY_ADJUSTMENT = "Opportunity_Adjustment";
        // Fields used in Opportunity_Adjustments
        public const string strfOPPORTUNITY_ADJUSTMENT_ID = "Opportunity_Adjustment_Id";
        public const string strfADJUSTMENT_AMOUNT = "Adjustment_Amount";
        public const string strfADJUSTMENT_PERCENTAGE = "Adjustment_Percentage";
        public const string strfADJUSTMENT_TYPE = "Adjustment_Type";
        public const string strfAPPLY_TO = "Apply_To";
        public const string strfSUM_FIELD = "Sum_Field";
        public const string strfRELEASE_ADJUSTMENT_ID = "Release_Adjustment_Id";
        public const string strfCOPY_OF_ADJUSTMENT_ID = "Copy_Of_Adjustment_Id";
        public const string strfREPLACED_BY_ADJUSTMENT_ID = "Replaced_By_Adjustment_Id";
        public const string strfREPLACES_ADJUSTMENT_ID = "Replaces_Adjustment_Id";
        // Queries used in Opportunity_Adjustment
        public const string strqOPP_ADJUSTS_FOR_QUOTE = "Sys: Opp Adjustments for Quote ?";
        public const string strqSELECTED_ADJUSTMENTS_FOR_OPPORTUNITY = "HB: Selected Adjustments for Opportunity ?";
        public const string strqSELECTED_ADJUSTS_COPY_OF_ADJUST_UNDEFINED_OPP = "HB: Selected Adjusts Copy of Adjust = undefined Opp ?";
        public const string strqUNSELECTED_ADJUSTS_COPY_OF_ADJUST_DEFINDED_OPP = "HB:UnSelected Adjustments Copy of Adjust is defined Opp ?";
        public const string strqSELECTED_ADJUSTS_COPY_OF_ADJUST_DEFINED_OPP = "HB: Selected Adjusts Copy of Adjust = defined Opp ?";
        public const string strqREMOVED_ADJUSTMENTS_FROM_PSQ_ADJUSTMENT_ORIG_OPP_PSQ_OPP = "HB: Removed Adjustments from PSQ Adjustment Orig Opp ? PSQ Opp ?";
        public const string strqSELECTED_PPI_ADJUSTMENTS_FOR_OPP = "HB: Selected PPI Adjustments for Opp ?";
        // Opportunity_Agreement
        public const string strtOPPORTUNITY_AGREEMENT = "Opportunity_Agreement";
        public const string strqOPPORTUNITY_AGREEMENTS_FOR_OPP = "HB: Agreements for Opportunity ?";
        // Opportunity__Product
        public const string strqCOMPONENT_OPTION_FOR_A_PARENT_PACKAGE_OPTION_AND_PRODUCT = "Env: Component Option for a Parent Package Option? and Product?";
        public const string strtOPPORTUNITY__PRODUCT = "Opportunity__Product";
        // Fields used in Opportunity__Product
        public const string strfADDED_BY_CHANGE_ORDER_ID = "Added_By_Change_Order_Id";
        public const string strfBUILD_OPTION = "Built_Option";
        public const string strfCATEGORY_ID = "Category_Id";
        public const string strfSUB_CATEGORY_ID = "Sub_Category_Id";

        public const string strfCODE_ = "Code_";
        public const string strfCONSTRUCTION_STAGE_ID = "Construction_Stage_Id";
        public const string strfCONSTRUCTION_STAGE_ORDINAL = "Construction_Stage_Ordinal";
        public const string strfCUSTOMERINSTRUCTIONS = "CustomerInstructions";
        public const string strfDELTA_BUILT_OPTION = "Delta_Built_Option";
        public const string strfCONFIGURATION_CHANGED = "Configuration_Changed";
        public const string strfEXTENDED_PRICE = "Extended_Price";
        public const string strfFILTER_VISIBILITY = "Filter_Visibility";
        public const string strfNBHDP_PRODUCT_ID = "NBHDP_Product_Id";
        public const string strfOPP_CURRENCY = "Opp_Currency";
        public const string strfOPPORTUNITY__PRODUCT_ID = "Opportunity__Product_Id";
        public const string strfOPPORTUNITY_ID = "Opportunity_Id";
        public const string strfOPPORTUNITY_PRODUCT_PREF_ID = "Opportunity_Product_Pref_Id";
        public const string strfOPTIONNOTES = "OptionNotes";
        public const string strfPREFERENCES_LIST = "Preferences_List";
        public const string strfPRODUCT_AVAILABLE = "Product_Available";
        public const string strfQUANTITY = "Quantity";
        public const string strfQUOTED_PRICE = "Quoted_Price";
        public const string strfREMOVED_BY_CHANGE_ORDER_ID = "Removed_by_Change_Order_Id";
        public const string strfSELECTED = "Selected";
        public const string strfTICKLE_COUNTER = "Tickle_Counter";
        public const string strfORIG_OPP_PROD_ID = "Orig_Opp_Prod_Id";
        public const string strfMODIFIED_BY_CHANGE_ORDER_ID = "Modified_by_Change_Order_Id";
        public const string strfTOTAL_PPI_ADJUSTMENTS = "Total_PPI_Adjustments";
        public const string strfPLAN_HAS_STND_OPTIONS = "Plan_Has_StndOptions";
        // Queries NBHD Phase
        public const string strfNBHD_PHASE_ID = "NBHD_Phase_Id";
        public const string strqOPEN_NBHD_PHASES_FOR_NBHD = "Sys: Open Status with Neighborhood ?";
        public const string strqGLOBAL_OPTIONS_FOR_RELEASE = "Sys: Global Options for Release ?";
        // Queries Opportunity_Product
        public const string strq_OPP_PRODUCTS_FOR_OPP = "PA: Op. Products with Opportunity Id ?";
        public const string strq_PLAN_OPTIONS_FOR_NBHD_PRODUCTS = "PA: Plan Option Products for Neighborhood Product ?";
        public const string strqOPTIONS_FOR_PLAN_AND_PRODUCT = "PA: Options for Plan ? and Product ?";
        public const string strqOPP_PRODUCT_WITH_NBHD_PRODUCT = "PA: Opp. Product with NBHD Product ?";
        public const string strqSELECTED_PRODUCTS_FOR_OPP_AND_NBDHPROD = "PA: Selected Products for Opp ? NBHDProd ?";
        public const string strqSELECTED_OPTIONS_FOR_QUOTE = "Sys: Selected Options for Quote ?";
        public const string strqINCLUDED_OPTION_RULES_WITH_PLAN_AND_PRODUCT = "PA: Included Option Rules with Plan ? and Product ?";
        public const string strqINCLUDED_OPTION_RULES_WITH_PLAN_AND_CHILD_PRODUCT = "PA: Included Option Rules with Plan ? and Child Product ?";
        public const string strqINCLUDED_OPTION_RULES_WITH_PLAN_AND_PRODUCT_AND_NOT_PRODUCT = "PA: Included Option Rules with Plan ? and Product ? and Product != ?";
        public const string strqINCLUDED_OPTION_RULES_WITH_PLAN_AND_CHILD_PRODUCT_AND_NOT_PARENT_PRODUCT = "PA: Included Option Rules with Plan ? and Child Product ? and Parent Product != ?";
        public const string strqOPP_PRODUCT_FOR_OPP_AND_NBHD_PRODUCT = "PA: Opp Product for Opp ? and NBHD Product ?";
        public const string strqOPTIONAL_STANDARD_PRODCTS_FOR_PLAN = "Sys: Optional Standard Products for Plan ?";
        public const string strqOPP_PRODUCTS_FOR_OPP = "PA: Op. Products with Opportunity Id ?";
        public const string strqOPP_PRODUCTS_WITH_OPP_ID = "PA: Op. Products with Opportunity Id ?";
        public const string strqSELECTEDOPP_PRODUCTS_WITH_OPP_ID = "PA: Selected Op. Products with Opportunity Id ?";
        public const string strqOP_LOCS_FOR_OP = "HB: OP Locations for Opprtunity Product?";
        public const string strqOPP_ADJUSTMENT_FOR_OPP = "HB: All Seleted Adjustments for Opportunity?";
        public const string strqSELECTED_OPTIONS_OPP_DIV_PROD = "HB: Selected Options for Opp? with Division Product?";
        public const string strqOPP_PROD_WITH_OPP_WITHOUT_PCO_PRICE_OPTIONS = "PA: Op. Products with Opportunity Id ? without PCO Price";
        public const string strqSELECTED_OPP_PROD_WITH_OPP_WITHOUT_PCO_PRICE_OPTIONS = "PA: Selected Op. Products with Opportunity Id ? without PCO Price";
        public const string strqACTIVE_CONTRACT_PROGRESS__OR_SALES_REQ_FOR_LOT = "HB: Active  Contracts In Progress Or Sales Request for Lot?";
        public const string strqACTIVE_OPTIONS_GEO_PRODUCT = "HB: Active Options Corp Reg?Div?NBD?Rel?Plan?Plan?Plan DivProd?Division?";
        public const string strqACTIVE_CHILD_EXCLUDE_RULE_WITH_PARENT = "HB: Active Child having Exclude Rule with Parent?Plan?Parent?Plan?Parent?Plan";
        public const string strqACTIVE_OPTIONS_GEO_PRODUCT_FOR_PRODUCT = "HB: Active Options Corp Reg?Div?NBD?Rel?DivProd?Plan?Plan?Plan DivProd?Division?";
        //Searches on NBHDP_Product table
        public const string strsearchACTIVE_OPTIONS_GEO_PRODUCT = "Active Options Geo Product";
        public const string strsearchACTIVE_OPTIONS_GEO_PRODUCT_FOR_PRODUCT = "Active Options Geo Product For Product";
        //Search Result List on NBHDP_Product table
        public const string strsrlOPTIONS_BY_PRODUCT_BY_PRIOTIY = "Options by Product by Priority";
        //Table Product_Option_Rule
        public const string strtPRODUCT_OPTION_RULE = "Product_Option_Rule";

        //fields Product_Option_Rule
        public const string strfPLAN_PRODUCT_ID = "Plan_Product_Id";
        public const string strfPARENT_PRODUCT_ID = "Parent_Product_Id";
        //public const string strfCHILD_PRODUCT_ID = "Child_Product_Id";
        public const string strfINCLUDE_OPTIONAL = "Include_Optional";

        //Queries for Product_Option_Rule
        public const string strqACTIVE_EXC_PRIORITY_PARENT_PLAN_PARENT_PARENT_PLAN = "HB: Active Exc Priority Rule with Parent? Plan? Parent? Parent? Plan?";
        public const string strqACTIVE_HARD_INC_PRIORITY_PARENT_PLAN_PARENT_PARENT_PLAN = "HB: Active Hard Inc Priority Rule with Parent? Plan? Parent? Parent? Plan?";
        public const string strqACTIVE_SOFT_INC_PRIORITY_PARENT_PLAN_PARENT_PARENT_PLAN = "HB: Active Soft Inc Priority Rule with Parent? Plan? Parent? Parent? Plan?";
        public const string strqACTIVE_HARD_INC_PRIORITY_CHILD_PLAN_CHILD_CHILD_PLAN = "HB: Active Hard Inc Priority Rule with Child? Plan? Child? Child? Plan?";
        public const string strqACTIVE_HARD_INC_PRIORITY_CHILD_PLAN_CHILD_CHILD_PLAN_NOT_PARENT = "HB: Active Hard Inc Priority Rule with Child? Plan? Child? Child? Plan? Parent !=?";

        // OppProd_Loc_Attribute_Pref
        public const string strtOPPPROD_ATTR_PREF = "OppProd_Loc_Attribute_Pref";
        // Fields used in OppProd_Attribute_Pref
        public const string strfATTRIBUTE = "Attribute";
        public const string strfOP_LOC_ATTR_PREF_ID = "OppProd_Loc_Attribute_Pref_Id";

        public const string strfOP_PREF_ID = "Opportunity_Product_Pref_Id";
        public const string strqOP_LOC_ATTR_PREF_FOR_OPLOC = "HB: Opp Product Attr Pref for OP Loc?";
        public const string strqOP_PREF_FOR_ATTRIBUTE = "HB: OP Pref for Attribute?";
        // Opp_Product_Location
        public const string strtOPP_PRODUCT_LOCATION = "Opp_Product_Location";
        public const string strfOPP_PRODUCT_LOCATION_ID = "Opp_Product_Location_Id";
        // public const string strfPREFERENCE_LIST  = "Preference_List"
        public const string strfLOCATION_QUANTITY = "Location_Quantity";
        public const string strfOPP_PRODUCT_ID = "Opportunity_Product_Id";
        public const string strqDIV_PRODUCT_FOR_NBHDPRODUCT = "HB: Div Product for NBHD Product ?";
        public const string strqOPP_PROD_LOC_FOR_OPPPRODUCT = "HB: OP Locations for Opprtunity Product?";
        public const string strqOPP_PROD_PREF_FOR_OPP_PROD_AND_DIV_PROD = "HB: Opp Prod Pref for Opp Prod ? Div Prod Pref ?";
        public const string strqOPP_PROD_LOC_FOR_OPPPRODUCT_AND_LOC = "HB: OP Locations for Opprtunity Product? Location?";
        // Opportunity_Product_Pref
        public const string strtOPPORTUNITY_PRODUCT_PREF = "Opportunity_Product_Pref";
        public const string strfOPP_PRODUCT_PREF_ID = "Opp_Product_Pref_Id";
        public const string strfOPPORTUNITY_PRODUCT_ID = "Opportunity_Product_Id";
        // Queries Employee
        public const string strqDIVISION_OF_CURRENT_USER = "Division of Current User";
        public const string strq_CHANGE_ORDER_FOR_OPP = "HB: Change Orders for Opportunity ?";
        public const string strqMISSING_CATEGORIES_PLAN_OPP = "Sys: Missing Categories for Plan ? and Opportunit ?";
        public const string strqMissing_Built_Options = "Sys: Built Options Not In Opportunity";
        public const string strqBUILD_OPTIONS_IN_OPP = "Sys: Built Options Not In Opportunity";
        public const string strqNBHD_PRODUCTS_FOR_OPP = "Sys: NBHP_Products where Opportunity Product.OpportunityId = ?";
        public const string strqCONTACT_TEAM_MEMBERS = "PA: Contact Team Member of Contact ?";
        public const string strqContactReleaseforNBHDPhase = "Sys: Contact Release for Contact ? and Release ?";
        public const string strqNBHD_PLAN_OPTS_FOR_PRODUCT = "Plan Options for NBHDP Product ?";
        public const string strqNBHD_PLAN_OPTS_EXCL = "PA: Plan Option Products for Neighborhood Product ? Excl ?";
        public const string strqSELECTED_OPTIONS_FOR_OPP = "PA: Selected Options for Opp ?";
        public const string strqSELECTED_OPTION_EXCL_EXCL = "HB: Selected Options on Opp and Not Eelvation";
        public const string strqSELECTED_OPTIONS_EXCEPT_ELEVATION = "HB: Selected Options on Opp and Not Eelvation";
        public const string strqHOME_SITES_AVAILABLE_FOR_RELEASE = "PA: Sites for Phase ? and not Closed";
        public const string strqDIV_PRODUCT_PREF_FOR_DIV_PROD = "PA: Division Product Pref for Div Product ?";
        public const string strqAVAILABLE_PLANS_FOR_LOT = "HB: NHBDP Plans w/ Phase = ? for Quote Exclu Plans";
        public const string strqAVAILABLE_PLANS_FOR_LOT_EXCL = "HB: NHBDP Plans w/ Phase = ? Exclude Current Plan for Quote";
        public const string strqDIVISION_FOR_OPPORTUNITY = "HB: Division For Opportunity?";
        public const string strqDEPOSIT_SCHED_TEMPL_ITEMS = "HB: Deposit Schedule Template Items for Template ID = ?";
        public const string strqMOST_SPECIFIC_PLANS_EXCLUDING_CURRENT_DIV_PROD = "HB: Active Available Priority Plans for Quotes and Contract with Release NBD Lot and Div Prod";
        // System
        public const string strfWARRANTY_START_AFTER = "Warranty_Start_After";
        // Tax Deductions
        // Tables
        public const string strtTAX_DEDUCTIONS = "tax_deductions";
        // Fields used in tax_deductions
        public const string strfSTDDEDFOR1CHECKED = "StdDedFor1Checked";
        public const string strfSTDDEDFOR2CHECKED = "StdDedFor2Checked";
        public const string strfSTDDEDFOR3CHECKED = "StdDedFor3Checked";
        public const string strfSTDDEDFOR4CHECKED = "StdDedFor4Checked";
        public const string strfSTDDEDUCTION = "StdDeduction";
        public const string strfTAX_DEDUCTIONS_ID = "Tax_Deductions_Id";
        public const string strfTAX_SCHEDULE_ID = "Tax_Schedule_Id";
        public const string strfTAXYEAR = "TaxYear";
        // TaxTables
        // Tables
        public const string strtTAXTABLES = "taxtables";
        // Fields used in taxtables
        public const string strfINCOME_HIGH = "Income_High";
        public const string strfINCOME_LOW = "Income_Low";
        public const string strfTAX_BASE_AMOUNT = "Tax_Base_Amount";
        public const string strfTAX_BRACKET_PERCENTAGE = "Tax_Bracket_Percentage";
        public const string strfTAX_INCOME_BASE_AMOUNT = "Tax_Income_Base_Amount";
        public const string strfTAXTABLES_ID = "TaxTables_Id";
        // CONSTANTS
        // Price Change History
        public const string strtPRICE_CHANGE_HISTORY = "Price_Change_History";
        public const string strfCHANGE_DATE = "Change_Date";
        public const string strfCHANGE_DATETIME = "Change_Timestamp";
        public const string strfPROCESSED = "Processed";
        public const string strfSTANDARD = "Standard";
        public const string strfPCH_COST_PRICE = "Cost_Price";
        public const string strfPCH_MARGIN = "Margin";
        public const string strfPCH_POST_CUT_OFF_PRICE = "Post_CutOff_Price";
        public const string strqPRICES_FOR_NBHDP_PRODUCTS = "Sys: Prices for Neighborhood Product ?";
        public const string strqVALID_PRICES_FOR_NBHDP_PRODUCTS = "HB: Valid Prices for Neighborhood Product ?";
        public const string strqPRICE_FOR_SQ_CHANGE_DATE = "HB: Price for Sales Request Option ? and Change Date?";
        public const string strqPRICE_FOR_SQ_CHANGE_DATE_TIME = "HB: Price for Sales Request Option ? and Change Date? Time?";
        // Configuration Type
        public const string strt_CONFIGURATION_TYPE = "Configuration_Type";
        public const string strf_CONFIGURATION_TYPE_ID = "Configuration_Type_Id";
        public const string strf_COMPONENT = "Component";
        public const string strf_CONFIGURATION_TYPE_NAME = "Configuration_Type_Name";
        // LD Strings
        public const string strdLOT_UNAVAILABLE = "Lot_Unavailable";
        public const string strdMISSING_REQUIRED_CATEGORIES = "This quote is not complete";
        public const string strdMISSING_BUILT_OPTIONS = "Quote Incomplete";
        public const string strdCONTRACT_PENDING = "ContractPending";
        // Fields used in Marketing_Campaign
        public const string strf_ACCOUNT_MANAGER_ID = "Account_Manager_Id";
        public const string strf_ACCOUNT_MANAGER_OVERRIDE = "Account_Manager_Override";
        public const string strf_ACTION_PLAN_ID = "Action_Plan_Id";
        public const string strf_ACTION_PLAN_NAME = "Action_Plan_Name";
        public const string strf_ACTION_PLAN_STEP_ID = "Action_Plan_Step_Id";
        public const string strf_ACTION_PLAN_CONTACT_STEP_ID = "Action_Plan_Contact_Step_Id";
        public const string strf_ACTION_PLAN_STEP_NAME = "Action_Plan_Step_Name";
        public const string strf_ACTIVITY_COMPLETE = "Activity_Complete";
        public const string strf_ACTIVITY_COMPLETED_DATE = "Activity_Completed_Date";
        public const string strf_ACTUAL_DECISION_DATE = "Actual_Decision_Date";
        public const string strf_ALERT_ID = "Alert_Id";
        public const string strf_AREA_CODE = "Area_Code";
        public const string strf_CALC_PROBABILITY_TO_CLOSE = "Calc_Probability_To_Close";
        public const string strf_COMPANY_ID = "Company_Id";
        public const string strf_COMPANY_NAME = "Company_Name";
        public const string strf_CONTACT_ACTIVITIES_ID = "Contact_Activities_Id";
        public const string strf_CONTACT_ID = "Contact_Id";
        public const string strf_COUNTRY = "Country";
        public const string strf_CURRENCY_ID = "Currency_Id";
        public const string strf_DATE_BECAME_CUSTOMER = "Date_Became_Customer";
        public const string strf_DEFAULT_MILESTONE_TEMPLATE = "Default_Milestone_Template";
        public const string strf_DELETE_AFTER = "delete_after";
        public const string strf_DELTA_ACCOUNT_MANAGER = "Delta_Account_Manager";
        public const string strf_DELTA_ACCOUNT_MANAGER_OVERRIDE = "Delta_Account_Manager_Override";
        public const string strf_DELTA_CONTACT_ID = "Delta_Contact_Id";
        public const string strf_DELTA_CURRENCY_ID = "Delta_Currency_Id";
        public const string strf_DELTA_PRODUCT_TYPE_INTEREST = "Delta_Product_Type_Interest";
        public const string strf_DISCOUNT = "Discount";
        public const string strf_EMPLOYEE_ID = "Employee_Id";
        public const string strf_EXPECTED_DURATION_DAYS = "Expected_Duration_Days";
        public const string strf_EXPECTED_REVENUE_DATE = "Expected_Revenue_Date";
        public const string strf_EXTENDED_PRICE = "Extended_Price";
        public const string strf_EXPECTED_DECISION_DATE = "Expected_Decision_Date";
        public const string strf_FIRST_NAME = "First_Name";
        public const string strf_INFLUENCED_ID = "Influenced_Id";
        public const string strf_INFLUENCER_ID = "Influencer_Id";
        public const string strf_INFLUENCER_INFLUENCE_ID = "Influencer_Influence_Id";
        public const string strf_INFLUENCER_ORIENTATION = "Influencer_Orientation";
        public const string strf_INFLUENCER_ROLE = "Influencer_Role";
        public const string strf_INFLUENCE_STATUS = "Influence_Status";
        public const string strf_INFLUENCING_ID = "Influencing_Id";
        public const string strf_INNER_CIRCLE = "Inner_Circle";
        public const string strf_LAST_NAME = "Last_Name";
        public const string strf_LOGIN_NAME = "Login_Name";
        public const string strf_MILESTONE_ID = "Milestone_Id";
        public const string strf_MILESTONES_ID = "Milestones_Id";
        public const string strf_MILESTONE_NAME = "Milestone_Name";
        public const string strf_MILESTONE_ORDINAL = "Milestone_Ordinal";
        public const string strf_MILESTONE_STATUS = "Milestone_Status";
        public const string strf_MILESTONE_TYPE = "Milestone_Type";
        public const string strf_MILESTONE_ITEM_NAME = "Milestone_Item_Name";
        public const string strf_MILESTONE_ITEM_ORDINAL = "Milestone_Item_Ordinal";
        public const string strf_MILESTONE_ITEMS_ID = "Milestone_Items_Id";
        public const string strf_OPPORTUNITY__INFLUENCER_ID = "Opportunity__Influencer_Id";
        public const string strf_OPPORTUNITY_TEAM_MEMBER_ID = "Opportunity_Team_Member_Id";
        public const string strf_OPPORTUNITY_ID = "Opportunity_Id";
        public const string strf_OPPORTUNITY_NAME = "Opportunity_Name";
        public const string strf_OVERRIDE_CALC_PROBABILITY = "Override_Calc_Probability";
        public const string strf_PERCENT_COMPLETE = "Percent_Complete";
        public const string strf_PIPELINE_STAGE = "Pipeline_Stage";
        public const string strf_PIPELINE_EXP_DURATION_DAYS = "Pipeline_Exp_Duration_Days";
        public const string strf_PIPELINE_LAST_UPDATED_DATE = "Pipeline_Last_Updated_Date";
        public const string strf_PRICE = "Price";
        public const string strf_POLITICAL_STRUCTURE = "Political_Structure";
        public const string strf_PROBABILITY_TO_CLOSE = "Probability_To_Close";
        public const string strf_PRODUCT_ID = "Product_Id";
        public const string strf_PRODUCT_TYPE_INTEREST = "Product_Type_Interest";
        public const string strf_QUANTITY = "Quantity";
        public const string strf_QUANTITY_ORDERED = "quantity_ordered";
        public const string strf_QUOTA__ID = "Quota__Id";
        public const string strf_QUOTA_PERIOD = "Quota_Period";
        public const string strf_QUOTE_CREATE_DATE = "Quote_Create_Date";
        public const string strf_PIPELINE_DURATION_DATE = "Pipeline_Duration_Date";
        public const string strf_RN_DESCRIPTOR = "Rn_Descriptor";
        public const string strf_RESULT_REASON_1 = "Result_Reason_1";
        public const string strf_REVENUE_DATE = "Revenue_Date";
        public const string strf_REVENUE_DATE_USE = "Revenue_Date_Use";
        public const string strf_ROLE_ID = "Role_Id";
        public const string strf_SALES_NOTIFICATION = "Sales_Notification";
        public const string strf_SHADOW_PROBABILITY_TO_CLOSE = "Shadow_Probability_To_Close";
        public const string strf_STATE = "State_";
        public const string strf_STATUS = "Status";
        public const string strf_STATUS_DATE = "Status_Date";
        public const string strf_STATUS_EDITED_DATE = "Status_Edited_Date";
        public const string strf_TEMPLATE_PROBABILITY_MAPPING = "Template_Probability_Mapping";
        public const string strf_TERRITORY_ID = "Territory_Id";
        public const string strf_TERRITORY_NAME = "Territory_Name";
        public const string strf_TERRITORY_TEAM_MEMBER_ID = "Territory_Team_Member_Id";
        public const string strf_TIME_SPENT = "Time_Spent";
        public const string strf_TYPE = "Type";
        public const string strf_UNIT_PRICE = "Unit_Price";
        public const string strf_VALID_UNTIL = "valid_until";
        public const string strf_WORK_EMAIL = "Work_Email";
        public const string strf_ZIP_CODE = "Zip";
        public const string strfREGION_NAME = "Region_Name";
        public const string strfDIVISION_NAME = "Division_Name";
        public const string strfNEIGHBORHOOD_NAME = "Neighborhood_Name";
        public const string strfCC_PCT_APP = "CCPctApp";
        public const string strfMIN_DWN_PCT = "MinDwnPct";
        public const string strfFIRST_LOAN_ID = "First_Loan_Id";
        public const string strfSECOND_LOAN_ID = "Second_Loan_Id";
        // Query Name
        public const string strq_ACTION_PARTNER_STEP_WITH_PLAN = "PA: Action Partner Steps with Plan ?";
        public const string strq_PARTNER_STEPS_ASSIGN_TO_PLAN = "PA: Partner Steps w/o Assign To w/ Plan ?";
        public const string strq_ACTION_PLAN_STEPS_WITH_PLAN = "PA: Action Plan Steps with Plan ?";
        public const string strq_ACTIVE_PLAN_STEPS_WITH_PLAN = "PA: Active Action Plan Steps with Plan ?";
        public const string strq_ALERT_WITH_OPPORTUNITY_ID = "PA: Alerts with Opportunity Id ?";
        public const string strq_COMPANY_IN_TERRITORY = "PA: Companies in Territory Partner w/ Territory ?";
        public const string strq_COMPANY_WITH_ID = "Sys: Company with Id ?";
        public const string strq_CONTACT_ACTIVITIES_WITH_OPPORTUNITY_ID = "PA: Contact Activities with Opportunity?";
        public const string strq_CONTACTS_WITH_COMPANY = "PA: Contacts with Company ?";
        public const string strq_CURRENCY_EXCHANGE_RATE = "PA: Exch Rate from Euro to Currency ?";
        public const string strq_FIND_OPPORTUNITYA_TEAM_MEMBER = "Sys: Opportunity Team Member with Opportunity?";
        public const string strq_INFLUENCER_RECORDS_FOR_INFLUENCER = "PA: Influence Records for Influencer ?";
        public const string strq_INFLUENCER_WITH_CONTACT_FOR_OPPORTUNITY = "PA: Opportunity Influencer with Contact ? for Op ?";
        public const string strq_LEAD_ACTION_PLANS = "PA: Lead Action Plans";
        public const string strq_MARKETING_ACTION_PLANS = "PA: Marketing Action Plans";
        public const string strq_MEMBERS_OF_TERRITORY = "PA: Members of Territory ?";
        public const string strq_MILESTONE = "PA: Milestones, Milestone Template Id?";
        public const string strq_MILESTONES_WITN_OPPORTUINTIY = "PA: Milestones, Opportunity ?";
        public const string strq_MILESTONE_ITEMS = "PA: Milestone Items, Milestone ID?";
        public const string strq_MILESTONE_ITEMS_WITH_OPPORTUNITY_ID = "PA: Milestone Items In 'PA: Milestones, Opport?";
        public const string strqMILESTONES_FOR_REL = "HB: All Active Milestones For Release?";
        public const string strq_OPP_INFLUENCERS_WITH_OPP_ID = "PA: Op. Influencers with Opportunity Id ?";
        public const string strq_OPPORTUNITY_PRODUCT_WITH_OPPORTUNITY_ID = "PA: Op. Products with Opportunity Id ?";
        public const string strq_OPPORTUNITY_TEAM_MEMBER_OF_OPPORTUNITY_ID = "PA: Opportunity Team Member of Opportunity ?";
        public const string strq_OPPORTUNITY_WITY_COMPANY = "PA: Opportunities with Company ?";
        public const string strq_OPPORTUNITY_WITH_ID = "Sys: Opportunity with Id ?";
        public const string strq_QUOTA_PERIOD = "Sys: Quota Period Start >= ? and End <= ?";
        public const string strq_RELATIONSHIP_ACTION_PLANS = "PA: Relationship Action Plans";
        public const string strq_SALES_ACTION_PLANS = "PA: Sales Action Plans";
        public const string strq_MILESTONE_EXPECTED_DURATIONS = "Sys: Milestone with Opp Id ? Pipeline Stage ?";
        public const string strq_PRICEBOOK = "PA: Price Book for Product ? and Currency ?";
        public const string strq_LENDER_FOR_LOAN_OFFICER = "HB: Lender for Loan Officer Id?";
        public const string strq_AVAILABLE_LOTS_FOR_RELEASE_ID = "HB: Available Lots for Release Id ?";
        public const string strq_AVAILABLE_LOTS_FOR_RELEASE_NO_CONSTR_STAGE = "HB: Available Trf Lots for Release Id ? No Construction Stage";
        public const string strq_AVAILABLE_TRF_LOTS_FOR_RELEASE_ID = "HB: Available Trf Lots for Release Id ?";
        public const string strq_EXCLUDED_PLANS_FOR_LOT = "HB: Excluded Plans for Lot Id?";
        public const string strq_QUOTES_WITH_NO_EXP_DEC_DATE = "SYS: Quotes with no Exp Decision Date";
        public const string strq_QUOTES_TO_BE_EXPIRED = "Sys: Quotes to be Expired";
        public const string strqINVENTORY_QUOTE_FOR_LOT = "Sys: Inventory Quote with Lot Id ?";
        public const string strqOPP_PRODUCT_PREFS_FOR_OPP_PRODUCT = "PA: Opp Product Prefs for Opp Product ?";
        public const string strqAVAILABLE_PRODUCTS_FOR_OPP_NBHDP_PROD = "PA: Available Products for Opp ? NBHDProd ?";
        public const string strqAVAILABLE_OPTIONS_FOR_QUOTE = "SYS: Available Options for Quote ?";
        public const string strqOPTIONS_FOR_MAIL_PRODUCT_CHILD_PARENT = "PA: Options for Main Product ? Child Parent ?";
        public const string strq_QUOTES_RESERVED_TO_BE_EXPIRED = "Sys: Quotes Reserved to be Expired";
        public const string strqACTIVE_INVENTORY_QUOTES_FOR_LOT = "HB: Active Inventory Quotes For Lot_ID ?";
        public const string strfCONTACT_HOMESITE_ASSOCIATED_CONTACTS = "HB: Associated Homesites Contacts of Type Cobuyer for Contact? and Lot?";
        public const string strtNBHD_NOTIFICATION_TEAM = "NBHD_Notification_Team";
        public const string strfNBHD_NOTIFICATION_TEAM_ID = "NBHD_Notification_Team_Id";
        public const string strqNOTIFICATION_TEAM_FOR_NBHD_SALES_APPROVED = "HB: Notification Team for NBHD ? Sales Approved";
        public const string strqNOTIFICATION_ON_SALES_RQST = "HB: Notification on Sale Request";
        public const string strqNOTIFICATION_FOR_SALES_RQST_DECLINED = "HB: Notification of Sales Requet Declined";
        public const string strqNOTIFICATION_OF_CANCEL_RQST_DECLINED = "HB: Notification of Cancel Request Declined";
        public const string strqNOTIFICATION_ON_CANCEL_APPROVAL = "HB: Notification on Cancel Approval";
        public const string strqNOTIFICATION_ON_CANCEL_REQUEST = "HB: Notification on Cancel Request";
        public const string strqNOTFICATION_OF_CHANGE_ORDER_CREATION = "HB: Notification of Change Order Creation";
        public const string strqINVENTORY_QUOTE_FOR_INVENTORY_HOME = "HB: Inventory quote for inventory home?";
        public const string strqTIC_NON_INVENTORY_QUOTE_FOR_LOT_ID = "TIC: Non Inventory Quote for Lot Id?";
        // Report
        public const string strr_OPPORTUNITY_PROPOSAL = "Opportunity Proposal";
        // Pipeline_Stage constants
        public const string strPIPELINE_QUOTE = "Quote";
        public const string strPIPELINE_SALES_REQUEST = "Sales Request";
        public const string strPIPELINE_POST_SALE = "Post Sale";
        public const string strPIPELINE_POST_BUILD_QUOTE = "Post Build";
        public const string strPIPELINE_CANCELED = "Canceled";
        public const string strPIPELINE_CLOSED = "Closed";
        public const string strPIPELINE_CONTRACT = "Contract";
        // Quote Status
        public const string strQUOTE_STATUS_INVENTORY = "Inventory";
        public const string strQUOTE_STATUS_IN_PROGRESS = "In Progress";
        public const string strQUOTE_STATUS_RESERVED = "Reserved";
        // Lot Types
        public const string strLOT_TYPE_INVENTORY = "Inventory";
        public const string strLOT_TYPE_HOMESITE = "Homesite";
        // Method
        public const string strmADD_INFLUENCERS = "AddInfluencers";
        public const string strmGET_ACTION_PLAN_CONTACT_STEP = "GetActionPlanContactStep";
        public const string strmCOPY_INFLUENCERS = "CopyInfluencers";
        public const string strmCOPY_INFLUENCER_INFLUENCE = "CopyInfluencerInfluence";
        public const string strmDELETE_INFLUENCERS = "DeleteInfluencers";
        public const string strmEXIT_CURRENCY = "ExitCurrency";
        public const string strmEXIT_STATUS = "ExitStatus";
        public const string strmEXIT_TERRITORY = "ExitTerritory";
        public const string strmGET_ACTIONPLAN = "GetActionplan";
        public const string strmGET_ACTIONPLAN_STEP = "GetActionplanStep";
        public const string strmGET_COMPANIES = "GetCompanies";
        public const string strmGET_EXPECTED_DURATION = "GetExpectedduration";
        public const string strmAPPLY_MILESTONE = "ApplyMilestone";
        public const string strmFINDQUOTAPERIOD = "FindQuotaPeriod";
        public const string strmGET_OPPORTUNITY_PRODUCT = "GetOpportunityProduct";
        public const string strmGET_TERRITORY_NAME = "GetTerritoryName";
        public const string strmOPPORTUNITY_WITH_COMPANY = "OpportunityWithCompany";
        public const string strmRESET_TEAM_MEMBER = "ResetTeamMember";
        public const string strmPROPOSAL = "Proposal";
        public const string strmUPDATE_PRICEBOOK = "UpdatePriceBook";
        public const string strmUSE_ACTION_PLAN = "UseActionPlan";
        public const string strm_DEFAULT_DESCRIPTION = "DefaultDescription";
        public const string strm_SET_SYSTEM = "SetSystem";
        public const string strm_EXECUTE = "Execute";
        public const string strm_ADD_FORM_DATA = "AddFormData";
        public const string strm_DELETE_FORM_DATA = "DeleteFormData";
        public const string strm_LOAD_FORM_DATA = "LoadFormData";
        public const string strm_NEW_FORM_DATA = "NewFormData";
        public const string strm_SAVE_FORM_DATA = "SaveFormData";
        public const string strm_NEW_SECONDARY_DATA = "NewSecondaryData";
        public const string strm_GET_PIPELINE_STAGE = "GetPipelineStage";
        public const string strm_CHANGE_PIPELINE = "ChangePipeline";
        public const string strmFIND_ACCOUNT_MANAGER = "FindAccountManager";
        public const string strmTRANSFER_CONTRACT = "TransferContract";
        public const string strmGET_AVAILABLE_PLANS = "GetAvailablePlans";
        public const string strmUPDATE_QUOTE_OPTIONS = "UpdateQuoteOptions";
        public const string strmUPDATE_QUOTE_OPTIONS_SINGLE_OPTION = "UpdateQuoteOptionsSingleOption";
        public const string strmSELECT_UNSELECT_OPTIONS = "SelectUnselectOptions";
        public const string strmSELECT_MULTPLE_OPTIONS = "SelectMultipleOptions";
        public const string strmSAVE_OPTIONS = "UpdateOptions";
        public const string strmCONVERT_TO_SALE = "ConvertToSale";
        public const string strmCAN_BE_DELETED = "CanBeDeleted";
        public const string strmCOPY_QUOTE = "CopyQuote";
        public const string strmCREATE_INVENTORY_QUOTE_FROM_CONTRACT = "CreateInventoryQuoteFromContract";
        public const string strmCOPY_QUOTE_SECONDARIES = "CopyQuoteSecondaries";
        public const string strmCANCEL_CONTRACT = "CancelContract";
        public const string strmGET_NUMBER_OF_NON_QUOTES = "GetNumberOfNonQuotes";
        public const string strmUPDATE_OPTION_FILTER = "UpdateOptionFilter";
        public const string strmCHECK_OPTIONS = "CheckOptions";
        public const string strmGET_CHILD_OPTIONS = "GetChildOptions";
        public const string strmGET_PARENT_OPTIONS = "GetParentOptions";
        public const string strmGET_SELECTED_EXCLUDED_OPTIONS = "GetSelectedExcludedOptions";
        public const string strmGET_LENDER_ID = "GetLenderID";
        public const string strmGET_LOTS_LIST = "GetLotsList";
        public const string strmGET_LOTS_LIST_TRF = "GetLotsListTrf";
        public const string strmCHECK_PLAN = "CheckPlan";
        public const string strmBATCH_UPDATE_QUOTE_EXPIRY = "BatchUpdateQuoteExpiry";
        public const string strmUPDATE_QUOTE_ON_PLAN_CHNG = "UpdateQuoteOnPlanChange";
        public const string strmOPTION_AM_I_BUILT = "OptionAmIBuilt";
        public const string strmDELETE_TEAM = "DeleteTeam";
        public const string strmGET_MILESTONE = "GetMilestone";
        public const string strmCREATE_NEW_RECORDSET = "CreateNewRecordset";
        public const string strmDATASET_DELETE = "DatasetDelete";
        public const string strmGET_RECORDSET = "GetRecordset";
        public const string strmUPDATE_MILESTONE_ITEM = "UpdateMilestoneItem";
        public const string strmUPDATE_PREFERENCE_LIST = "UpdatePreferencesList";
        public const string strmUPDATE_OPTIONS = "UpdateOptions";
        public const string strmGET_NEXT_CHANGE_ORDER_NUMBER = "GetNextChangeOrderNumber";
        public const string strmCHECK_COMPLETENESS = "CheckCompleteness";
        public const string strmVERIFY_BUILT_OPTIONS = "VerifyBuiltOptions";
        public const string strmCALCULATE_TOTALS = "CalculateTotals";
        public const string strmCLOSE_CANCEL_OPPORTUNITY = "CloseCancelOpportunity";
        public const string strmUPDATE_LOT_STATUS_CLOSED = "UpdateLotStatusClosed";
        public const string strmCHECK_LAST_LOT_CLOSED = "CheckLastLotClosed";
        public const string strmSET_SALES_TEAM = "SetSalesTeam";
        public const string strmCOPY_NBHDP_AGREEMENT_TO_OPP_AGREEMENT = "CopyNBHDAgreementToOppAgreement";
        public const string strmADD_INV_QUOTE_OPTIONS = "AddInvQuoteOptions";
        public const string strmADD_CHANGE_ORDERS = "AddChangeOrders";
        public const string strmSET_OPTION_PRICING = "SetOptionPricing";
        public const string strmGET_OPTION_NEXT_PRICE = "GetOptionNextPrice";
        public const string strmGET_OPTION_FIXED_PRICE = "GetOptionFixedPrice";
        public const string strmGET_XML_FOR_FC = "GetXMLforFC";
        public const string strmSAVE_LOAN_PROFILE = "SaveLoanProfile";
        public const string strmSET_VALUE_PAIRS_STRING = "SetValuePairsString";
        public const string strmSET_VALUE_PAIRS_BOOL = "SetValuePairsBool";
        public const string strmINT_TO_ID_HEX_STR = "InttoIdHexStr";
        public const string strmID_TO_INT_STR = "IdtoIntStr";
        public const string strmGET_LOAN_PROGRAMS_XML = "GetLoanProgramsXML";
        public const string strmSET_LOAN_PROGRAMS_FOR_EMP = "SetLoanProgramsForEmp";
        public const string strmSET_REGIONS = "SetRegions";
        public const string strmSET_DIVISIONS = "SetDivisions";
        public const string strmSET_NEIGHBORHOOD = "SetNeighborhoods";
        public const string strmSET_NEIGHBORHOOD_FEES = "SetNeighborhoodFees";
        public const string strmSET_LOAN_PROGRAMS = "SetLoanPrograms";
        public const string strmSET_LOANS = "SetLoans";
        public const string strmSET_LOANS_FEES = "SetLoanFees";
        public const string strmSET_OPP = "SetOpp";
        public const string strmGET_LOAN_PROFILE_INFO = "GetLoanProfileInfo";
        public const string strmLOAD_LOAN_PROFILE_INFO = "LoadLoanProfileInfo";
        public const string strmSET_LOAN_PROGRAMS_FOR_NBHD = "SetLoanProgramsForNbhd";
        public const string strmSET_NBHD_OPP_XML = "SetNbhdOppXML";
        public const string strmLOAD_LOAN_PROFILE = "LoadLoanProfile";
        public const string strmSET_LOAN_PROFILE = "SetLoanProfile";
        public const string strmSET_LOAN_PROFILE_XML = "SetLoanProfileXML";
        public const string strmSET_LOAN_PROFILE_LOAN_XML = "SetLoanProfileLoanXML";
        public const string strmSET_OPP_XML = "SetOppXML";
        public const string strmSET_TAX_DEDUCTION_DATA = "SetTaxDeductionData";
        public const string strmSET_TAX_TABLE_DATA = "SetTaxTableData";
        public const string strmSET_TAX_SCHEDULE_DATA = "SetTaxScheduleData";
        public const string strmCHECK_TAX_DATA_AVAILABLE = "checkTaxDataAvailable";
        public const string strmSET_TAX_RATE_DATA = "SetTaxRateData";
        public const string strmLOAD_FIN_CALC_XML = "LoadFinCalcXML";
        public const string strmRESET_QUOTE = "ResetQuote";
        public const string strmCLEAR_OPTIONS = "ClearOptions";
        public const string strmINVENTORY_QUOTE_SEARCH = "InventoryQuoteSearch";
        public const string strmINVENTORY_MANAGEMENT_ALLOWED_FOR_CURRENT_USER = "InventoryManagementAllowedForCurrentUser";
        public const string strmUPDATE_RESERVATION_STATUS = "UpdateReservationStatus";
        public const string strmNOTIFY_INTEGRATION_OF_CONTRACT_CHANGE = "NotifyIntegrationOfContractChange";
        public const string strmIS_INTEGRATION_ON = "IsIntegrationOn";
        public const string strmBATCH_UPDATE_QUOTE_STATUS = "BatchUpdateQuoteStatus";
        public const string strmUPDATE_QUOTE_STATUS = "UpdateQuoteStatus";
        public const string strmCHECK_LOT_AVAILABILITY = "CheckLotAvailability";

        public const string strmUPDATE_PROFILE = "UpdateProfile";
        public const string strmCHECK_BUYER_IS_GLOBAL_STAGE = "CheckBuyerIsGlobalStage";
        public const string strmCAN_COPY_QUOTE = "CanCopyQuote";
        public const string strmGET_INVENTORY_QUOTE_BY_CUSTOM_QUERY = "GetInventoryQuotesByCustomQuery";
        public const string strmGET_CONSTRUCTION_STAGE_COMPARISON = "GetConstructionStageComparison";
        public const string strmUPDATE_HOMESITE_PLAN = "UpdateHomesitePlan";
        public const string strmUPDATE_HOMESITE_BUILT_ELEVATION = "UpdateHomesiteBuiltElevation";
        public const string strmUPDATE_PLAN_BUILT_FOR_ACTIVE_CUSTOMER_QUOTE = "UpdatePlanBuiltForActiveCustomerQuote";
        public const string strmUPDATE_OPTION_BUILT_FOR_ACTIVE_CUSTOMER_QUOTE = "UpdateOptionBuiltForActiveCustomerQuote";
        public const string strmGET_RESELECTED_OPTION_PRICE_AND_BUILT_INFO = "GetReSelectedOptionPriceAndBuiltInfo";
        public const string strmUPDATE_OPTION_BUILTS = "UpdateOptionBuilts";
        public const string strmCANCEL_REQUEST_CONTRACT = "CancelRequestOrContract";
        public const string strmCREATE_POST_SALE_QUOTE = "CreatePostSaleQuote";
        public const string strmGET_EMAIL_TEXT = "GetEmailText";
        public const string strmGET_EMAIL_RECIPIENTS = "GetEmailRecipients";
        public const string strmAPPLY_DEPOSIT_SCHEDULE_TEMPLATES = "ApplyDepositScheduleTemplates";
        public const string strmCHECK_HOMESITE = "CheckHomesite";
        public const string strmAPPLY_POST_SALE_QUOTE = "ApplyPostSaleQuote";
        public const string strmSALES_REQUEST_DECLINED = "SalesRequestDeclined";
        public const string strmCANCEL_REQUEST_DECLINED = "CancelRequestDeclined";
        public const string strmSALES_REQUEST = "SalesRequest";
        public const string strmGET_QUOTE_PLAN_PRICE = "GetQuotePlanPrice";
        public const string strmGET_INVENTORY_CHANGE_NOTE = "GetInventoryChangeNote";
        public const string strmINACTIVATE_CUSTOMER_QUOTES = "InactivateCustomerQuotes";
        public const string strmUPDATE_CUSTOMER_QUOTE_LOCATIONS = "UpdateCustomerQuoteLocations";
        public const string strmCREATE_HOMESITE_CONFIGURATION = "Create Homesite Configuration";
        public const string strmINACTIVATE_UNBUILT_LOT_CONFIGURATIONS = "Inactive Unbuilt Lot Configurations";
        public const string strmUPDATE_INVENTORY_HOMESITE_TYPE = "UpdateInventoryHomesiteType";
        public const string strmSET_LOAN_SPECIAL_FEE = "SetLoanSpecialFee";
        public const string strmINACTIVATE_OTHER_PSQ = "InactivateOtherPSQ";
        public const string strmINACTIVATE_ALL_PSQ = "InactivateAllPSQ";
        public const string strmGET_OPTION_PRICE = "GetOptionPrice";
        public const string strmLOAD_EXCLUDED_OPTIONS = "LoadExcludedOptions";
        public const string strmLOAD_NBHD_PRODUCTS = "LoadNBHDProducts";
        public const string strmLOAD_EXCL_PRODUCTS = "LoadExcludedProducts";
        public const string strmCREATE_OPPRODUCT_OPTION = "CreateOpportunityProductOption";
        public const string strmCREATE_OPP_PRODUCT_STD = "CreateOpportunityProductStandard";
        public const string strmAPPLY_REL_MILESTONES = "ApplyReleaseMilestones";
        public const string strmUPDATE_CUST_QUOTE_ADD_PRICE = "UpdateCustomerQuoteAdditionalPrice";
        public const string strmDELETE_OPTIONS = "DeleteOptions";
        public const string strmUPDATE_QUOTE_CHOSEN_ELEV = "UpdateQuoteChosenElevation";
        public const string strmGET_QUOTE_OPTION_PRICE = "GetQuoteOptionPrice";
        public const string strmSET_OPTION_PRICE = "SetOptionPrice";
        public const string strmUPDATE_OPP_TEAM_FROM_CNBHDPROFILE = "UpdateOpportunitySalesTeamFromContactNBHDProfile";
        public const string strmUPDATE_COBUYER_STATUS = "UpdateCoBuyerStatus";
        public const string strmUPDATE_CONTACT_PROFILE_NBHD = "UpdateContactProfileNBHD";
        public const string strmUPDATE_CONTACT_TYPE = "UpdateContactType";
        public const string strmUPDATE_LOT_STATUS = "UpdateLotStatus";
        public const string strmUPDATE_LOT_STATUS_EX = "UpdateLotStatusEx";
        public const string strmUPDATE_OPP_SALES_TEAM = "UpdateOpportunitySalesTeam";
        public const string strmSET_BASE_CONFIGURATION = "SetBaseConfiguration";
        public const string strmSET_QUOTE_PRICE_UPDATE = "SetQuotePriceUpdate";
        public const string strmRESET_CONTACT_COBUYERTYPE = "ResetContactCoBuyerType";
        public const string strmADD_CHANGE_CUSTOM_ORDERS = "AddChangeCustomOrders";
        public const string strmCAN_INACTIVATE_INVENTORY_QUOTE = "CanInactivateInventoryQuote";
        public const string strmCOPYOPTION_SECONDARIES = "CopyOptionSecondaries";
        public const string strmCOPY_OPT_SECONDARY_BY_OPTION = "CopyOptionSecondariesByOption";
        public const string strmHOMESITE_CONSTRUCTION_ORD_PAST = "HomesiteConstructionOrdinalPastPlanOne";
        public const string strmPROCESS_ADJ_RECORDS = "ProcessAdjustmentRecords";
        public const string strmPROCESS_OPTION_RECORDS = "ProcessOptionRecords";
        public const string strmGET_NEXT_SEQ_NUMBER = "GetNextSequenceNumber";
        public const string strmIS_OPTION_MODIFIED = "IsOptionModified";
        public const string strmIS_OPT_SEC_MODIFIED = "IsOptionSecondaryModified";
        public const string strmMODIFY_CONTRACT_OPTION = "ModifyContractOption";
        public const string strmADD_CHNG_ORDER_ADJUSTMENTS = "AddChangeOrderAdjustments";
        public const string strmADD_PSQ_ADJUST_TO_ORG_OPP = "AddPSQAdjustmentToOriginalOpp";
        public const string strmIS_ADJUSTMENT_MODIFIED = "IsAdjustmentModified";
        public const string strmADD_CUST_CHANGE_ORDER_OPT = "AddCustomChangeOrderOption";
        public const string strmCREATE_NEW_ATTR_PREF = "CreateNewAttrPreference";
        public const string strmIS_THERE_BUILT_OPTION_CHANGE = "IsThereBuiltOptionChange";
        public const string strmIS_THERE_PSQ = "IsTherePSQ";
        public const string strmSINGLE_OPTION_VALIDATION = "SingleOptionValidation";
        public const string strmINVENTORY_HOME_OPTION_VALIDATION = "InventoryHomeOptionValidation";
        public const string strmOPTION_NEEDS_LOCATION = "OptionNeedsLocation";
        public const string strmOPTIONS_WITH_DUPLICATE_LOCATIONS = "OptionsWithDuplicateLocations";


        // LD Groups
        public const string strgOPPORTUNITY = "Opportunity";
        public const string strgNBHD_NOTIFICATION_TEAM = "NBHD Notification Team";
        public const string strgDIVISION = "Division";
        // Langusge string
        public const string strlACCOUNT_MANAGER_NOTIFICATION = "Account Manager Notification";
        public const string strlCOMPANY_UPDATED = "Company Has Been Changed To A Customer";
        public const string strlCOMPANY_NOT_UPDATED = "Company Was Not Successfully Updated";
        public const string strlFIELD_CHANGED = "Field Changed";
        public const string strlFIND_ALERT_FAILD = "FindAlert Method Faild";
        public const string strlINFLUENCER_RECORD_ADD = "Influencer Record Add";
        public const string strlNO_ASSIGN_TO = "No Assign To";
        public const string strlNO_STEPS = "No Steps";
        public const string strlNO_PARTNER = "No Partner";
        public const string strlNOTIFY_APPLY_ACTION_PLAN = "Notify Apply Action Plan";
        public const string strlOPPORTUNITY_RECORD_EDIT = "Opportunity Record Edit";
        public const string strlPARTNER_ACTIVITY_RECORD_EDIT = "Partner Activity Record Edit";
        public const string strlQUOTA_RECORD_ADD = "Quota Record Add";
        public const string strlQUOTA_RECORD_DELETE = "Quota Record Delete";
        public const string strlSALES_TEAM_ACTIVITY_RECORD_ADD = "Sales Team Activity Record Add";
        public const string strlWITH_SUPPORT_INCIDENT = "Opportunity with a supporting incident";
        public const string strlPHUB_SAVED_SECCESSFULLY = "PHub Saved Successfully";
        public const string strlPHUB_DELETED_SECCESSFULLY = "PHub Deleted Successfully";
        public const string strlPHUB_ADDED_SECCESSFULLY = "PHub Added Successfully";
        public const string strlRECORD_DELETED = "Record Deleted";
        public const string strlRECORD_EDITED = "Record Edited";
        public const string strlOPPORTUNITY_EDITED = "Opportunity Edited";
        public const string strlDETAILS = "Details";
        public const string strlOPPORTUNITY_EMAIL_SUBJECT = "Opportunity Email Subject";
        public const string strlOPPORTUNITY_RELATED_EMAIL_SUBJECT = "Opportunity Related Email Subject";
        public const string strlSECONDARY_ADD_RECORD = "Secondary Add Record";
        public const string strlSECONDARY_MODIFY_RECORD = "Secondary Modify Record";
        public const string strlSECONDARY_DELETE_RECORD = "Secondary Delete Record";
        public const string strlDELETION_CANCELED = "Deletion Canceled";
        public const string strlSALE_CANCELED_BY = "Sale cancelled by ";
        public const string strlON = " on ";
        public const string strlAVAILABLE = "Available";
        public const string strlCLOSING_COST_FIXED = "ClosingCostFixed";
        public const string strlCLOSING_COST_PCT_APPRAISED = "ClosingCostPctAppraised";
        public const string strlMINIMUM_DOWN_PERCENT = "MinimumDownPercent";
        public const string strlPOST_SALE = "Post Sale";
        public const string strlPOST_SALE_ACCEPTED = "Post Sale Accepted";
        public const string strlPARTICIPATION_FEE = "Participation_Fee";
        public const string strlPARTICIPATIONFEE = "ParticipationFee";
        public const string strlTOP_RATIO = "TopRatio";
        public const string strlBOTTOM_RATIO = "BottomRatio";
        public const string strlMARKET_LEVEL_NEIGHBORHOOD = "Market Level Neighborhood";
        public const string strlAPPROVED_CANCEL = "Approved Cancel";
        public const string strdCHANGE_ORDER_CREATION_SUBJECT = "Change Order Creation Subject";
        public const string strdCHANGE_ORDER_CREATION_MESSAGE1 = "Change Order Creation Message1";
        public const string strdCHANGE_ORDER_CREATION_MESSAGE2 = "Change Order Creation Message2";
        public const string strdCHANGE_ORDER_CREATION_MESSAGE3 = "Change Order Creation Message3";
        public const string strdBUILT_OPTION_CHANGE_ALERT = "Built Option Change Alert";
        public const string strdOPTION_NEEDS_LOCATION = "OptionNeedsLocation";
        public const string strdOPTIONS_WITH_DUPLICATE_LOCATIONS = "OptionsWithDuplicateLocations";
        public const string strdCANNOT_FIND_INVENTORY_QUOTE = "Cannot Find Inventory Quote";

        // Security Group
        public const string gstrstyHB_ADMIN = "Home Builders - Admin";
        public const string strsHB_ADMIN = "GRN: S-0-79"; //language string for Home Builders - Admin security group.
        // Error
        public const string gstrEMPTY_STRING = "";
        public const int glngERR_APPDEV_START_NUMBER = -2147221504 + 10000;
        public const int glngERR_APPDEV_END_NUMBER = -2147221504 + 13399;
        public const int glngERR_APPDEV_EXTEND_START_NUMBER = -2147221504 + 13600;
        public const int glngERR_APPDEV_EXTEND_END_NUMBER = -2147221504 + 29999;
        public const int glngERR_START_NUMBER = -2147221504 + 11800;
        public const int glngERR_END_NUMBER = glngERR_START_NUMBER + 199;
        public const int glngERR_METHOD_NOT_DEFINED = -2147221504 + 13401;
        public const int glngERR_PARAMETER_EXPECTED = -2147221504 + 13402;
        public const int glngERR_PARAMETER_IS_VALID = -2147221504 + 13403;
        public const int glngERR_ADDFORMDATA_FAILED = -2147221504 + 13404;
        public const int glngERR_DELETEFORMDATA_FAILED = -2147221504 + 13405;
        public const int glngERR_ON_ADDING_NEW_RECORD = -2147221504 + 13406;
        public const int glngERR_EXECUTE_FAILED = -2147221504 + 13407;
        public const int glngERR_LOADFORMDATA_FAILED = -2147221504 + 13408;
        public const int glngERR_NEWFORMDATA_FAILED = -2147221504 + 13409;
        public const int glngERR_NEWSECONDARYDATA_FAILED = -2147221504 + 13410;
        public const int glngERR_SAVEFORMDATA_FAILED = -2147221504 + 13411;
        public const int glngERR_EXECUTE_IS_NOT_AVAILABLE = -2147221504 + 13412;
        public const int glngERR_NEWSECONDARYDATA_IS_NOT_AVAILABLE = -2147221504 + 13413;
        //public const int glngERR_ADDFORMDATA_IS_NOT_AVAILABLE = VariantType.Object + 13414;
        //public const int glngERR_NEWFORMDATA_IS_NOT_AVAILABLE = VariantType.Object + 13415;
        //public const int glngERR_DELETEFORMDATA_IS_NOT_AVAILABLE = VariantType.Object + 13416;
        public const int glngERR_CAN_NOT_DELETE = glngERR_START_NUMBER + 40;
        public const int glngLOT_UNAVAILABLE = glngERR_APPDEV_EXTEND_START_NUMBER + 5;
        public const int glngMISSING_REQUIRED_CATEGORIES = glngERR_APPDEV_EXTEND_START_NUMBER + 4;
        public const int glngMISSING_BUILT_OPTIONS = glngERR_APPDEV_EXTEND_START_NUMBER + 9;
        public const int glngPARENTS_ARE_BUILT = glngERR_APPDEV_EXTEND_START_NUMBER + 12;

        // Group Errors
        public const string strdERROR_ON_ADDING_NEW_RECORD = "Error on Adding New Record";
        public const string strdPARAMETERS_ARE_EXPECTED = "Parameters Are Expected";
        public const string strdNEWFORMDATA_FAILED = "NewFormDataFailed";
        public const string strdNEWSECONDARYDATA_FAILED = "NewSecondaryDataFailed";
        public const string strdDELETEFORMDATA_FAILED = "DeleteFormDataFailed";
        public const string strdADDFORMDATA_FAILED = "AddFormDataFailed";
        public const string strdEXECUTE_FAILED = "ExecuteFailed";
        public const string strdMETHOD_IS_NOT_DEFINED = "Method Is Not Defined";
        public const string strdLOADFORMDATA_FAILED = "LoadFormDataFailed";
        public const string strdSAVEFORMDATA_FAILED = "SaveFormDataFailed";
        public const string strdSETSYSTEM_FAILED = "SetSystemFailed";
        public const string strdPHB_SAVED_UNSUCCESSFULLY = "PHub Unsuccessful Save";
        public const string strdNEWSCEONDARYDATA_IS_NOT_AVAILABLE = "NewSecondaryData is not available";
        public const string strdINVALID_METHOD = "Invalid Method";
        public const string strdCANNOT_SELECT_OPTION = "Cannot Select Option";
        public const string strdSELECT_MULTIPLE_ERROR = "Select Multiple Error";
        public const string strdALREADY_SELECTED = "Already Selected";
        public const string strdCANNOT_BE_ADDED = "Cannot Be Added";
        public const string strdREQURES_INCLUDING_ANOTHER_OPTION = "Requires Including Another Option";
        public const string strdHOMESITE_REQUIRED = "HomeSite Required";
        public const string strdLOT = "Lot";
        public const string strdTRACT = "Tract";
        public const string strdBLOCK = "Block";
        public const string strdNEIGHBORHOOD = "Neighborhood";
        public const string strdPHASE = "Phase";
        public const string strdNO_TEAM_MEMBERS_DEFINED = "No Team Members Defined";
        public const string strdLOG_START = "Log Start";
        public const string strdERR_NO_SYSTEM_RECORD = "Error No System Record";
        public const string strdERR_NO_EXPIRY_PERIOD = "Error No Expiry Period";
        public const string strdLOG_END = "Log End";
        public const string strdLOG_DETAIL = "Log Details";
        public const string strdERROR = "Error";
        public const string strdYES = "Yes";
        public const string strdNO = "No";
        public const string strdPARAMETERS_EXPECTED = "Parameters Expected";
        public const string strdPARAMETERS_PASSED = "Parameters Passed";
        public const string strdPARAMETERS = "Parameters";
        public const string strdEOCE_DATE_REQUIRED = "ECOE  Date Required";
        public const string strdCONVERT_TO_SALE_NOTE = "Convert To Sale Note";
        public const string strdSALES_APPROVED_SUBJECT = "Sales Approved Subject";
        public const string strdSALES_APPROVED_MESSAGE1 = "Sales Approved Message1";
        public const string strdSALES_APPROVED_MESSAGE2 = "Sales Approved Message2";
        public const string strdSALES_APPROVED_MESSAGE3 = "Sales Approved Message3";
        public const string strdMULTIPLE_ELEVATIONS = "Multiple Elevations";
        public const string strdSALES_DECLINED_SUBJECT = "Sales Declined Subject";
        public const string strdSALES_DECLINED_MESSAGE1 = "Sale Declined Message1";
        public const string strdSALES_DECLINED_MESSAGE2 = "Sale Declined Message2";
        public const string strdSALES_DECLINED_MESSAGE3 = "Sales Declined Message3";
        public const string strdCANCEL_DECLINE_SUBJECT = "Cancel Declined Subject";
        public const string strdCANCEL_DECLINE_MESSAGE1 = "Cancel Declined Message1";
        public const string strdCANCEL_DECLINE_MESSAGE2 = "Cancel Declined Message2";
        public const string strdCANCEL_DECLINE_MESSAGE3 = "Cancel Declined Message3";
        public const string strdSALES_REQUEST_SUBJECT = "Sales Request Subject";
        public const string strdSALES_REQUEST_MESSAGE1 = "Sales Request Message1";
        public const string strdSALES_REQUEST_MESSAGE2 = "Sales Request Message2";
        public const string strdSALES_REQUEST_MESSAGE3 = "Sales Request Message3";
        public const string strdCANCEL_REQUEST_SUBJECT = "Cancel Request Subject";
        public const string strdCANCEL_REQUEST_MESSAGE1 = "Cancel Request Message1";
        public const string strdCANCEL_REQUEST_MESSAGE2 = "Cancel Request Message2";
        public const string strdCANCEL_REQUEST_MESSAGE3 = "Cancel Request Message3";
        public const string strdCANCEL_APPROVED_SUBJECT = "Cancel Approved Subject";
        public const string strdCANCEL_APPROVED_MESSAGE1 = "Cancel Approved Message1";
        public const string strdCANCEL_APPROVED_MESSAGE2 = "Cancel Approved Message2";
        public const string strdCANCEL_APPROVED_MESSAGE3 = "Cancel Approved Message3";
        public const string strdINVENTORY_INACTIVATE_START = "Inventory Inactivate Start";
        public const string strdINVENTORY_INACTIVATE_ADD_INVENT = "Inventory Inactivate Add Inventory";
        public const string strdINVENTORY_MODIFIED_START = "Inventory Modified Start";
        public const string strdINVENTORY_ADDITIONAL_PRICE_CHANGE = "Inventory Additional Price Change";
        public const string strdINVENTORY_INACTIVATE_INVENTORY_ONLY = "Inventory Inactivate Inventory Only";
        public const string strdINVENTORY_INACTIVATE_CUSTOMER_ONLY = "Inventory Inactivate Customer Only";
        public const string strdINVENTORY_MODIFIED_OPTION = "Inventory Modified Option";
        public const string strdINVENTORY_INACTIVATE_CHANGED = "Inventory Inactivate Option Change";
        public const string strdINVENTORY_INACTIVATE_PLAN_CHANGE = "Inventory Inactivate Plan Change";
        public const string strdHOMESITE_CONSTRUCTION_STAGE_CHANGE = "Homesite Construction Stage Change";
        public const string strdINVENTORY_QUOTE_CHANGED = "Inventory Quote Changed";
        public const string strdLOT_RESERVED = "Lot_Reserved";
        public const string strdCANT_UNSELECT_CHILD_W_BUILT_PARENTS = "Cannot Unselect Child Option - Built Parent";

    }

}
