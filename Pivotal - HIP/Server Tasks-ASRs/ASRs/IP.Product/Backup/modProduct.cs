using System;

using Pivotal.Interop.RDALib;


namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    internal class modProduct
    {
        /// <summary>
        /// </summary>
        /// This module contains all public constants for the Product and Price_Book Objects.
        /// <history>
        /// Revision#    Date        Author  Description
        /// 3.8.0.0      4/28/2006   JHui    Converted to .Net C# code.
        /// NOTE: 1. This module orgalize the table name, field name, query name as following format for
        /// every table order by alphabet except the primary record id:
        /// Table name
        /// Declare table name constant
        /// Declare form name constant
        /// Declare field name constants
        /// Declare query name constants
        /// AppServer Rule Name, Language String ID Name, Active Form Name, Method Name,
        /// Error Number
        /// Product
        /// </history>
        public const string strtPRODUCT = "Product";
        public const string strtOPPORTUNITY = "Opportunity";
        public const string strtLOT_CONFIGURATION = "Lot_Configuration";
        public const string strfPRODUCT_ID = "Product_Id";
        public const string strfACTIVE = "Active";
        public const string strfCONSTRUCTION_STAGE_ID = "Construction_Stage_Id";
        public const string strfDELTA_PRICE = "Delta_Price";
        public const string strfNEXT_PRICE = "Next_Price";
        public const string strfPRICE = "Price";

        public const string strfCOST_PRICE = "Cost_Price";
        public const string strfMARGIN = "Margin";

        public const string strfPRICE_CHANGE_DATE = "Price_Change_Date";
        public const string strfSYSTEM_DEFAULT_CURRENCY = "System_Default_Currency";
        public const string strfNBHD_PHASE_ID = "NBHD_Phase_Id";
        public const string strfTYPE = "Type";
        public const string strfLOT_NUMBER = "Lot_Number";
        public const string strfSALES_DATE = "Sales_Date";
        public const string strfRELEASE_DATE = "Release_Date";
        public const string strfCONTRACT_CLOSE_DATE = "Contract_Close_Date";
        public const string strfRESERVED_DATE = "Reserved_Date";
        public const string strfINACTIVE = "Inactive";
        public const string strfOPPORTUNITY_ID = "Opportunity_Id";
        public const string strfLOT_PREMIUM_CHANGE = "Lot_Premium_Change";
        public const string strfLOT_PREMIUM_PRICE_UPDATE_DATE = "Lot_Premium_Price_Update_Date";
        public const string strfSCHEDULED_SCRIPT_DAYS = "Schdld_Scrpt_Hstry_Days_Qury";
        public const string strmCAN_BE_INACTIVATED = "CanBeInactivated";
        public const string strqACTIVE_PRODUCT_IN_SYSTEM = "Sys: Active Product In System ?";
        public const string strqQUOTES_WITH_LOTS = "Sys: Quotes with Lot Id ?";
        public const string strqINVENTORY_QUOTE_FOR_LOT = "Sys: Inventory Quote with Lot Id ?";
        public const string strqEXPIRED_RESERVED_LOTS = "HB: Expired Reserved Lots";
        public const string strqLOT_NUMBER_AND_RELEASE = "HB: Lot Number ? and Release ?";
        public const string strqALL_ACTIVE_LOTS = "HB: All Active Lots";
        public const string strqALL_ACTIVE_LOTS_NEWER = "HB: All Active Lots for Edit Date > ?";
        public const string strqPRICE_UPDATE_LOTS = "HB: Price Update - Lots";
        public const string strqHOMESITE_REFERENCED_FOR_LOT = "HB: Homesite Referenced For Lot ?";
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
        public const string strrLOT_ADMIN = "HB Lot Admin";
        // Alert
        public const string strsALERT = "PAHB Alert";
        // Lot Status value list
        public const string strLOT_STATUS_RESERVED = "Reserved";
        public const string strLOT_STATUS_SOLD = "Sold";
        public const string strLOT_STATUS_CLOSED = "Closed";
        public const string strLOT_STATUS_AVAILABLE = "Available";
        public const string strLOT_STATUS_UNAVAILABLE = "Not Released";
        public const string strLOT_STATUS_INACTIVE = "Inactive";
        // Activity
        public const string strtRN_APPOINTMENTS = "Rn_Appointments";
        public const string strfRN_APPOINTMENTS_ID = "Rn_Appointments_Id";
        public const string strqRN_APPTS_FOR_LOT = "HB: Activities for Lot Id ?";
        // Alert
        public const string strqALERT_FOR_LOT = "PA: Alerts with Lot Id ?";
        // Arch_Lead
        public const string strtARCH_LEAD = "Arch_Lead";
        public const string strrARCH_LEAD = "Arch_Lead";
        public const string strfPRODUCT_INTEREST_ID = "Product_Interest_Id";
        public const string strqARCHIVED_LEADS_WITH_PRODUCT = "Sys: Archived Leads with Product ?";
        // Arch_Product_Interest
        public const string strtARCH_PRODUCT_INTEREST = "Arch_Product_Interest";
        public const string strqARCH_PRODUCT_INTERESTS_WITH_PRODUCT = "Sys: Arch Product Interests with Product ?";
        // CrossSell_Product
        public const string strtCROSSSELL_PRODUCT = "CrossSell_Product";
        public const string strfCROSSSELL_PRODUCT_ID = "CrossSell_Product_Id";
        public const string strfMAIN_PRODUCT_ID = "Main_Product_Id";
        public const string strqCROSSSELL_PRODUCTS_FOR_PRODUCT = "Sys: CrossSell Products for Product ?";
        public const string strqCROSSSELL_PRODUCTS_WITH_CROSSSELL_PRODUCT = "Sys: CrossSell Products with CrossSell Product ?";
        // Currency -------------------------------------------------------------------------------------------
        public const string strtCURRENCY = "Currency_";
        public const string strfCURRENCY_NAME = "Currency_Name";
        public const string strqCURRENCIES_MISSED_PRICE_BOOK_WITH_PRODUCT = "Sys: Currencies Missed Price Book with Product ?";
        public const string strqCURRENCIES_WITH_EXTRA_PRICE_FOR_PRODUCT = "Sys: Currencies with Extra Price Book for Product?";
        public const string strqSYSTEM_CURRENCY = "PA: System Currency";
        // Currency_Exchange_Rate
        public const string strtCURRENCY_EXCHANGE_RATE = "Currency_Exchange_Rate";
        public const string strfCURRENT_EXCHANGE_RATE = "Current_Exchange_Rate";
        public const string strqEXCH_RATE_FROM_SYS_TO_CURRENCY = "PA: Exch Rate from Sys Currency to Currency ?";
        // Construction Stage
        public const string strtCONSTRUCTION_STAGE = "Construction_Stage";
        public const string strfCONSTRUCTION_STAGE_ORD = "Construction_Stage_Ordinal";
        public const string strfCONSTRUCTION_STAGE_NAME = "Construction_Stage_Name";
        public const string strqCONSTRUCTION_STAGES_FOR_RELEASE_STAGE_NUMBER = "HB: Construction Stages for Release? Stage Number?";
        // Division
        public const string strtDIVISION = "Division";
        public const string strfDIVISION_ID = "Division_Id";
        public const string strfINCLUDE_HOMESITE_PREMIUM = "Include_Homesite_Premium";
        public const string strfSTANDARD_OPTION_PRICING = "Standard_Option_Pricing";
        public const string strfBUILD_OPTION_PRICING = "Built_Option_Pricing";
        // constants for standard options
        public const int intSTANDARD_OPTION_FIXED = 0;
        public const int intSTANDARD_OPTION_FLOATING = 1;
        public const int intBUILD_OPTION_FIXED = 0;
        public const int intBUILD_OPTION_FLOATING = 1;
        // Division Product
        public const string strtDIVISION_PRODUCT = "Division_Product";
        // ImageAttachment
        public const string strqIMAGE_ATTACHMENT_FOR_LOT = "HB: Image Attachment for Lot?";
        // Inspection
        public const string strtINSPECTION = "Inspection";
        public const string strfINSPECTION_ID = "Inpsection_Id";
        public const string strqINSPECTIONS_FOR_LOT = "HB: Inpsections for Lot Id ?";
        // Issue
        public const string strtISSUE = "Issue";
        public const string strfISSUE_ID = "Issue_Id";
        public const string strqISSUES_WITH_PRODUCT = "Sys: Issues with Product ?";
        // Knowledge_Base_
        public const string strtKNOWLEDGE_BASE_ITEM = "Knowledge_Base_Item";
        public const string strfKNOWLEDGE_BASE_ITEM_ID = "Knowledge_Base_Item_Id";
        public const string strq_KB_ITEMS_WITH_PRODUCT = "Sys: KB Items with Product ?";
        // Lead -------------------------------------------------------------------------------------------
        public const string strtLEAD_ = "Lead_";
        public const string strrLEAD = "Lead";
        public const string strfLEAD__ID = "Lead__Id";
        public const string strqLEADS_WITH_PRODUCT = "Sys: Leads with Product ?";
        // Literature
        public const string strtLITERATURE = "Literature";
        public const string strrLITERATURE = "Literature";
        public const string strfLITERATURE_ID = "Literature_Id";
        public const string strqLITERATURE_ITEMS_WITH_PRODUCT = "Sys: Literature Items with Product ?";
        // Lot_Plan
        public const string strtLOT_PLAN = "Lot_Plan";
        public const string strfLOT_PLAN_ID = "Lot_Plan_Id";
        public const string strqLOT_PLANS_FOR_LOT = "HB: Plans for Lot Id ?";
        // Lot__Company
        public const string strqCONTRACTORS_FOR_LOT = "HB: Contractors for Homesite ?";
        // Lot__Contact
        public const string strqCONTACTS_FOR_LOT = "HB: Associated Contacts of Lot?";
        // Marketing_Campaign
        public const string strtMARKETING_CAMPAIGN = "Marketing_Campaign";
        public const string strrMARKETING_CAMPAIGN = "Marketing Campaign";
        public const string strfMARKETING_CAMPAIGN_ID = "Marketing_Campaign_Id";
        public const string strqMARKETING_CAMPAIGNS_WITH_PRODUCT = "Sys: Marketing Campaigns with Product ?";
        // Marketing_Project
        public const string strtMARKETING_PROJECT = "Marketing_Project";
        public const string strrMARKETING_PROJECT = "Marketing Project";
        public const string strfMARKETING_PROJECT_ID = "Marketing_Project_Id";
        public const string strqMARKETING_PROJECTS_WITH_PRODUCT = "Sys: Marketing Projects with Product ?";
        // NBHD_Phase
        public const string strtNBHD_PHASE = "NBHD_Phase";
        public const string strfRN_DESCRIPTOR = "Rn_Descriptor";
        // Neighborhood Product
        public const string strtNBHD_PRODUCT = "NBHDP_Product";
        public const string strfNBHD_PRODUCT_ID = "NBHDP_Product_Id";
        public const string strfCURRENT_PRICE = "Current_Price";
        public const string strfDIVISION_PRODUCT_ID = "Division_Product_Id";
        // Opportunity
        public const string strtQUOTE = "Opportunity";
        public const string strfQUOTE_ID = "Opportunity_Id";
        public const string strfCONTRACT_APPROVED_SUBMITTED_DATETIME = "Contract_Approved_Datetime";
        public const string strfCONTRACT_APPROVED_SUBMITTED = "Contract_Approved_Submitted";
        public const string strfDELTA_LOT_STATUS = "Delta_Lot_Status";
        public const string strfLOT_PREMIUM = "Lot_Premium";
        public const string strfLOT_STATUS = "Lot_Status";
        public const string strfPLAN_ID = "Plan_Name_Id";
        public const string strfELEVATION_ID = "Elevation_Id";
        public const string strfPLAN_BUILT = "Plan_Built";
        public const string strfBUILT_OPTIONS = "Built_Options";
        public const string strfPRICE_UPDATE = "Price_Update";
        public const string strfPIPELINE_STAGE = "Pipeline_Stage";
        public const string strfQUOTE_CREATE_DATETIME = "Quote_Create_Datetime";
        public const string strfQUOTE_CREATE_DATE = "Quote_Create_Date";
        public const string strfSTATUS = "Status";
        public const string strqQUOTE_FOR_LOT = "HB: Quotes for Lot Id ?";
        public const string strqRES_QUOTE_FOR_LOT = "HB: Reserved Quotes for Lot Id ?";
        // Opportunity__Product
        public const string strtOPPORTUNITY__PRODUCT = "Opportunity__Product";
        public const string strfOPPORTUNITY__PRODUCT_ID = "Opportunity__Product_Id";
        public const string strfBUILD_OPTION = "Built_Option";
        public const string strfPRODUCT_AVAILABLE = "Product_Available";
        public const string strfSELECTED = "Selected";
        // Public Const strfNBHD_PRODUCT_ID As String = "NBHDP_Product_Id"
        public const string strqOPP_PRODUT_WITH_PRODUCT_ID = "PA: Opp. Products with Product Id ?";
        public const string strqSELECTED_OPTIONS_FOR_QUTOE = "Sys: Selected Options for Quote ?";
        public const string strqOPP_PRODUCTS_WITH_OPP_ID = "PA: Op. Products with Opportunity Id ?";
        // Order_Detail
        public const string strtORDER_DETAIL = "Order_Detail";
        public const string strfORDER_DETAIL_ID = "Order_Detail_Id";
        public const string strqORDER_DETAILS_WITH_PRODUCT_ID = "PA: Order Details with Product Id ?";
        // Price_Book
        public const string strtPRICE_BOOK = "Price_Book";
        // Product_Id
        public const string strfPRICE_BOOK_ID = "Price_Book_Id";
        public const string strfCURRENCY_ID = "Currency_Id";
        public const string strqEXTRA_PRICE_BOOK_FOR_PRODUCT = "PA: Extra Price Book for Product ?";
        public const string strqPRICE_BOOK_DUPLICATES = "Sys: Price Book Possible Duplicates with Pro. Cu ?";
        public const string strqPRICE_BOOK_FOR_PRODUCT = "PA: Price Book for Product ?";
        public const string strqPRICE_BOOK_FOR_PRODUCT_AND_CURRENCY = "PA: Price Book for Product ? and Currency ?";
        public const string strqPRICE_BOOK_FOR_APPLY_CURRENCIES_AND_PRODUCT = "Sys: Price Book for Apply Currencies and Product ?";
        // Price_Change_History
        public const string strtPRICE_CHANGE_HISTORY = "Price_Change_History";
        public const string strfCHANGE_DATE = "Change_Date";
        public const string strfCHANGE_DATETIME = "Change_Timestamp";
        public const string strfLOT_ID = "Lot_Id";
        public const string strfPROCESSED = "Processed";
        public const string strfSTANDARD = "Standard";
        public const string strfPRICE_CHANGE_HISTORY_ID = "Price_Change_History_Id";
        public const string strfPCH_COST_PRICE = "Cost_Price";
        public const string strfPCH_MARGIN = "Margin";
        public const string strfPCH_POST_CUT_OFF_PRICE = "Post_CutOff_Price";
        public const string strqPRICE_CHNG_HISTORY_FOR_LOT = "HB: Prices for Lot Id ?";
        public const string strqPRICE_FOR_SALES_RQ_LOT_CHANGE_DATE_TIME = "HB: Price for Sales Request Homesite ? and Change Date ? Time?";
        public const string strqVALID_PRICES_FOR_LOT = "HB: Valid Prices for Lot?";
        // Product_Interest
        public const string strtPRODUCT_INTEREST = "Product_Interest";
        // Product_Id
        public const string strqPRODUCT_INTERESTS_WITH_PRODUCT = "Sys: Product Interests with Product ?";
        // Quotes
        public const string strqACTIVE_CUSTOMER_INVENTORY = "HB: Active Customer and Inventory Quote For Lot ?";
        public const string strqACTIVE_CUSTOMER_INVENTORY_NEW = "HB: Active Customer and Inventory Quote For Lot New?";
        public const string strqACTIVE_SALES_ITEMS = "HB: Active Sales Item For Lot ?";
        public const string strqLOT_CONFIGURATIONS = "HB: Lot Config for Lot?";
        // Registration
        public const string strtREGISTRATION = "Registration";
        public const string strfREGISTRATION_ID = "Registration_Id";
        public const string strqREGISTRATIONS_FOR_PRODUCT = "Registrations For Product ?";
        // Support_Category
        public const string strtSUPPORT_CATEGORY = "Support_Category";
        public const string strfSUPPORT_CATEGORY_ID = "Support_Category_Id";
        public const string strqSUPPORT_CATEGORIES_WITH_PRODUCT = "Sys: Support Categories with Product ?";
        // Support_Contract
        public const string strtSUPPORT_CONTRACT = "Support_Contract";
        public const string strfSUPPORT_CONTRACT_ID = "Support_Contract_Id";
        public const string strfCONTRACT_PRODUCT_ID = "Contract_Product_Id";
        public const string strqSUPPORT_CONTRACTS_WITH_PRODUCT = "Sys: Support Contracts with Product ?";
        // Support_Incident
        public const string strtSUPPORT_INCIDENT = "Support_Incident";
        public const string strfSUPPORT_INCIDENT_ID = "Support_Incident_Id";
        public const string strfCONTRACT_TYPE = "Contract_Type";
        public const string strqSUPPORT_INCIDENTS_WITH_PRODUCT = "Sys: Support Incidents with Product ?";
        public const string strqSUPPORT_INCIDENTS_WITH_CONTRACT_TYPE = "PA: Support Incidents with Contract Type ?";
        public const string strqSERVICE_REQUESTS_FOR_LOT = "HB: Support Incident for Lot Id ?";
        // Support_Request
        public const string strtSUPPORT_REQUEST = "Support_Request";
        public const string strfSUPPORT_REQUEST_ID = "Support_Request_Id";
        public const string strqSUPPORT_REQUESTS_FOR_PRODUCT = "Support Requests for Product ?";
        // Support_Subject
        public const string strtSUPPORT_SUBJECT = "Support_Subject";
        public const string strfSUPPORT_SUBJECT_ID = "Support_Subject_Id";
        public const string strqSUPPORT_SUBJECTS_FOR_PRODUCT = "Sys: Support Subjects for Product ?";
        // Support_Team
        public const string strtSUPPORT_TEAM = "Support_Team";
        public const string strfSUPPORT_TEAM_ID = "Support_Team_Id";
        public const string strqSUPPORT_TEAMS_FOR_PRODUCT = "Sys: Support Teams for Product ?";
        // System
        public const string strtSYSTEM = "System";
        public const string strfPRODUCT_COUNT = "Product_Count";
        public const string strfSUPPORT_COST_FACTOR = "Support_Cost_Factor";
        // Tmp_Support
        public const string strtTMP_SUPPORT = "Tmp_Support";
        public const string strfTMP_SUPPORT_ID = "Tmp_Support_Id";
        public const string strfKB_PRODUCT_ID = "KB_Product_Id";
        public const string strqTEMPORARY_SUPPORTS_FOR_PRODUCT = "Sys: Temporary Supports for Product ?";
        // Upsell_Product
        public const string strtUPSELL_PRODUCT = "Upsell_Product";
        public const string strfUPSELL_PRODUCT_ID = "Upsell_Product_Id";
        public const string strqUPSELL_PRODUCTS_FOR_PRODUCT = "Sys: UpSell Products for Product ?";
        public const string strqUPSELL_PRODUCTS_WITH_UPSELL_PRODUCT = "Sys: UpSell Products with UpSell Product ?";
        // Web_Sales_Team
        public const string strtWEB_SALES_TEAM = "Web_Sales_Team";
        public const string strfWEB_SALES_TEAM_ID = "Web_Sales_Team_Id";
        public const string strqWEB_SALES_FOR_PRODUCT = "Sys: Web Sales Team for Product ?";
        // Work_Order
        public const string strtWORK_ORDER = "Work_Order";
        public const string strfWORK_ORDER_ID = "Work_Order_Id";
        public const string strqWORK_ORDER_FOR_LOT = "HB: Work Order for Lot Id ?";
        // Forms
        public const string strrINSPECTION = "Inspection";
        public const string strrWORK_ORDER = "HB Work Order";
        // segment name
        public const string strsegQUOTES = "Quotes";
        public const string strsegPREMIUM_HISTORY = "Premium History";
        public const string strsegLOT_PLANS = "Lot Plans";
        public const string strsegINSPECTIONS = "Inspection";
        public const string strsegSERVICE_REQUESTS = "Service Requests";
        public const string strsegWORK_ORDERS = "Work Orders";
        public const string strsegAGREEMENT = "Agreement";
        // choices
        public const string strcAVAILABLE = "Available";
        // Script Name
        public const string strsINSPECTION = "PAHB Inspection";
        public const string strsOPPORTUNITY = "PAHB Opportunity";
        public const string strsINTEGRATION = "PAHB Integration";
        public const string strsPRICE_CHANGE_HISTORY = "PAHB Price Change History";
        // form segments
        public const string strsPREMIUM_HISTORY = "Premium History";
        // Public Method Name and Private procedure name
        public const string strmADD_FORM_DATA = "AddFormData";
        public const string strmDELETE_FORM_DATA = "DeleteFormData";
        public const string strmEXECUTE = "Execute";
        public const string strmLOAD_FORM_DATA = "LoadFormData";
        public const string strmNEW_FORM_DATA = "NewFormData";
        public const string strmSAVE_FORM_DATA = "SaveFormData";
        public const string strmSET_SYSTEM = "SetSystem";
        public const string strmNEW_SECONDARY_DATA = "NewSecondaryData";
        public const string strmCASCADE_DELETE = "CascadeDelete";
        public const string strmHAS_DUPLICATES = "HasDuplicates";
        public const string strmIS_USER_FINANCIAL_ADMIN = "IsUserFinancialAdmin";
        public const string strmGET_EXTRA_AND_MISSED_PRICE_BOOK = "GetExtraAndMissedPriceBook";
        public const string strmCAN_BE_DELETED = "CanBeDeleted";
        public const string strmUPDATE_QUOTE_OPTIONS = "UpdateQuoteOptions";
        public const string strmGET_CONSTRUCTION_STAGE = "GetConstructionStage";
        public const string strmGET_NUMBER_OF_QUOTES = "GetNumberOfQuotes";
        public const string strmUPDATE_DELTA_FIELDS = "UpdateDeltaFields";
        public const string strmGET_LOT_TYPE = "GetLotType";
        public const string strmGET_INV_QUOTE = "GetInventoryQuote";
        public const string strmUPDATE_LOT_STATUS = "UpdateLotStatus";
        public const string strmRESERVE_RELEASE_LOTS = "ReserveReleaseLots";
        public const string strmUPDATE_LOT_PRICING = "UpdateLotPricing";
        public const string strmCAN_LOT_BE_CREATED = "CanLotBeCreated";
        public const string strmIS_INTEGRATION_ON = "IsIntegrationOn";
        public const string strmNOTIFY_INTEGRATION_OF_LOT_CHANGE = "NotifyIntegrationOfLotChange";
        public const string strmVERIFY_RULES = "VerifyRules";
        public const string strmBATCH_UPDATE_LOT_STATUS = "BatchUpdateLotStatus";
        public const string strmGET_LOT_STATUS_BY_RULES = "GetLotStatusByRules";
        public const string strmCHECK_CUSTOMER_INVENTORY_QUOTE = "CheckCustomerInventoryQuote";
        public const string strmCHECK_SALES_TIME = "CheckSalesTime";
        public const string strmCHECK_LOT_CONFIGURATION = "CheckLotConfiguration";
        // String Names for Product group in LD_Strings table
        public const string strdDELETION_CANCELED = "Deletion Canceled";
        public const string strdLOT_EXISTS = "LotExists";
        public const string strdHOMESITE_REFERENCED_ALERT = "Homesite Referenced Alert";
        public const string strdHOMESITE_HAS_BUILT_OPTIONS = "Homesite Has Built Options";
        // Group Name
        public const string strgFINANCIAL_ADMINISTRATOR = "Financial Administrator";
        // Resources
        public static DateTime gdtLastBatchUpdateLotStatusRun;
        // maintains the last query run date between BatchUpdateLotStatus scheduled script runs
        
        public const string strgPRODUCT = "Product";
        public const string strdPRODUCT = "Product";
        public const string strdDUPLICATE_PRICE_BOOK = "Duplicate Price Book";
        public const string strdMISSING_REQUIRED_RECORDS = "Missing Required Records";
        public const string strdUNABLE_TO_OPEN_PRICE_BOOK = "Unable to Open Price Book";
        public const string strdUNABLE_TO_LOAD_PRODUCT_PRICE = "Unable to Load Product Price";
        public const string strdUNABLE_TO_LOAD_PRODUCT_CURRENCY = "Unable to Load Product Currency";
        public const string strdDELETION_IS_CANCELED = "Deletion Is Canceled";
        public const string strdORDER_WITH_BILL_TO_COMPANY = "Order (with Bill to Company)";
        public const string strdORDER_WITH_SHIP_TO_COMPANY = "Order (with Ship to Company)";
        public const string strdREGISTRATION = "Registration";
        public const string strdREGISTRATION_COMPANY = "Registration Company";
        public const string strdSUPPORT_CONTRACT = "Support Contract";
        public const string strdSUPPORT_INCIDENT = "Support Incident";
        public const string strdONLY_CAN_UPDATE_PRICE_BOOK = "Only Can Update Price Book";
        public const string strdINVALID_METHOD = "Invalid Method";
        public const string strdPARAMETERS_EXPECTRED = "Parameters Expected";
        public const string strdPARAMETERS_PASSED = "Parameters Passed";
        public const string strdPARAMETERES = "Parameters";
        public const string strdACTIVE_SALES_ITEMS = "Active Sales Item";
        public const string strdEXISTING_BUILT_OPTIONS = "Existing Built Options";
        public const string strdACTIVE_CUSTOMER_INVENTORY = "Active Inventory and Customer";
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
        // Error Number
        public const string gstrEMPTY_STRING = "";
        public const int glngERR_APPDEV_START_NUMBER = -2147221504 + 10000;
        public const int glngERR_APPDEV_END_NUMBER = -2147221504 + 13399;
        public const int glngERR_APPDEV_START_NUMBER_A = -2147221504 + 13600;
        public const int glngERR_APPDEV_END_NUMBER_A = -2147221504 + 29999;
        public const int glngERR_START_NUMBER = -2147221504 + 12300;
        public const int glngERR_END_NUMBER = glngERR_START_NUMBER + 99;
        public const int glngERR_CAN_NOT_DELETE = glngERR_START_NUMBER + 10;
        public const int glngERR_CAN_NOT_UPDATE_PRICE_BOOK = glngERR_START_NUMBER + 15;
        public const int glngERR_MISSING_REQUIRED_RECORDS = glngERR_START_NUMBER + 16;
        public const int glngERR_DELETION_IS_CANCELED = glngERR_START_NUMBER + 17;
        public const int glngERR_NO_RIGHT_OPEN_PRICE_BOOK = glngERR_START_NUMBER + 18;
        public const int glngERR_LOT_EXISTS = glngERR_START_NUMBER + 19;
        public const int glngERR_SHARED_START_NUMBER = -2147221504 + 13400;
        public const int glngERR_METHOD_NOT_DEFINED = glngERR_SHARED_START_NUMBER + 1;
        public const int glngERR_PARAMETER_EXPECTED = glngERR_SHARED_START_NUMBER + 2;
        public const int glngERR_PARAMETER_INVALID = glngERR_SHARED_START_NUMBER + 3;
        public const int glngERR_ADDFORMDATA_FAILED = glngERR_SHARED_START_NUMBER + 4;
        public const int glngERR_DELETEFORMDATA_FAILED = glngERR_SHARED_START_NUMBER + 5;
        public const int glngERR_ADD_RECORD_FAILED = glngERR_SHARED_START_NUMBER + 6;
        public const int glngERR_EXECUTE_FAILED = glngERR_SHARED_START_NUMBER + 7;
        public const int glngERR_LOADFORMDATA_FAILED = glngERR_SHARED_START_NUMBER + 8;
        public const int glngERR_NEWFORMDATA_FAILED = glngERR_SHARED_START_NUMBER + 9;
        public const int glngERR_NEWSECONDARYDATA_FAILED = glngERR_SHARED_START_NUMBER + 10;
        public const int glngERR_SAVEFORMDATA_FAILED = glngERR_SHARED_START_NUMBER + 11;
        public const int glngERR_SETSYSTEM_FAILED = glngERR_SHARED_START_NUMBER + 12;

    }
}
