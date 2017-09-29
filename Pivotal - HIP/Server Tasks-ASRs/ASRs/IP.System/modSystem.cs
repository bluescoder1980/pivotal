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
	internal class modSystem
	{

		// Project Name : PHbSystem
		/// <summary>
		/// This module holds all the public variables for
		/// </summary>
		// the System class.
		// Revision # Date Author Description
		// 3.8.0.0  5/2/2006dschafferConverted to .Net C# code.
        
		// Server Script
		public const string strs_ERRORS = "Errors";
		public const string strs_FUNCTIONLIB = "Function Lib";
		public const string strsTRANSIT_POINT_PARAMS = "Transit Point Params";
		//public const string strsPRODUCT = "PAHB Product";
        public const string strsPRODUCT = "TIC Product";
		public const string strsNEIGHBORHOOD = "PAHB Neighborhood";
		public const string strsNEIGHBORHOOD_PHASE = "PAHB Neighborhood Phase";
        public const string strsNEIGHBORHOOD_PRODUCT = "PAHB Neighborhood Phase Product";
		public const string strsOPPORTUNITY = "PAHB Opportunity";
		public const string strsMARKETING_PROJECT = "PAHB Marketing Project";
		// Table Name
		public const string strt_ALT_ADDRESS = "Alt_Address";
		public const string strt_ALT_PHONE = "Alt_Phone";
		public const string strt_COMPANY = "Company";
		public const string strt_COMPANY_TEAM_MEMBER = "Company_Team_Member";
		public const string strt_CONTACT = "Contact";
		public const string strt_CONTACT_ACTIVITIES = "Contact_Activities";
		public const string strt_CONTACT_TEAM_MEMBER = "Contact_Team_Member";
		public const string strt_CONTACT_WEB_DETAILS = "Contact_Web_Details";
		public const string strt_INFLUENCER_INFLUENCE = "Influencer_Influence";
		public const string strt_LEAD_ = "Lead_";
		public const string strt_LITERATURE_LISTING = "Literature_Listing";
		public const string strt_MARKETING_PROJECT = "Marketing_Project";
		public const string strt_MEETING_CONTACT_ATTENDEE = "Meeting_Contact_Attendee";
		public const string strt_MEETING_STAFF_ATTENDEE = "Meeting_Staff_Attendee";
		public const string strt_NEIGHBORHOOD = "Neighborhood";
		public const string strt_NEIGHBORHOOD_PHASE = "Neighborhood Phase";
		public const string strt_NEIGHBORHOOD_PRODUCT = "NBHDP_Product";
		public const string strt_OPPORTUNITY = "Opportunity";
		public const string strt_OPPORTUNITY__INFLUENCER = "Opportunity__Influencer";
		public const string strt_OPPORTUNITY__PRODUCT = "Opportunity__Product";
		public const string strt_OPPORTUNITY_TEAM_MEMBER = "Opportunity_Team_Member";
		public const string strt_PRODUCT = "Product";
		public const string strt_PRODUCT_INTEREST = "Product_Interest";
		public const string strt_RESELLER__CUSTOMER = "Reseller__Customer";
		public const string strt_RN_APPOINTMENTS = "Rn_Appointments";
		public const string strt_SYSTEM = "System";
		public const string strt_SYS_TICKLING = "Sys_Tickling";
        public const string strt_CONSTRUCTION_STAGE = "Construction_Stage";
		// Fields used in Marketing_Project
		public const string strf_ALT_ADDRESS_ID = "Alt_Address_Id";
		public const string strf_ALT_PHONE_ID = "Alt_Phone_Id";
		public const string strf_ADMINISTRATOR = "Administrator";
		public const string strf_CC_TO_CURRENT_USER = "cc_to_current_user";
		public const string strf_CO_CITY_MATCH = "Co_City_Match";
		public const string strf_CO_CO_NAME_MATCH = "Co_Co_Name_Match";
		public const string strf_CO_COUNTRY_MATCH = "Co_Country_Match";
		public const string strf_CO_PHONE_MATCH = "Co_Phone_Match";
		public const string strf_CO_STATE_MATCH = "Co_State_Match";
		public const string strf_CO_ZIP_MATCH = "Co_Zip_Match";
		public const string strf_COLLATERAL_SHIP_DAYS = "Collateral_Ship_Days";
		public const string strf_COMPANY_CASCADE_TICKLING = "Company_Cascade_Tickling";
		public const string strf_COMPANY_TEAM_MEMBER_ID = "Company_Team_Member_Id";
		public const string strf_CON_CITY_MATCH = "Con_City_Match";
		public const string strf_CON_CO_NAME_MATCH = "Con_Co_Name_Match";
		public const string strf_CON_COUNTRY_MATCH = "Con_Country_Match";
		public const string strf_CON_NAME_MATCH = "Con_Name_Match";
		public const string strf_CON_PHONE_MATCH = "Con_Phone_Match";
		public const string strf_CON_STATE_MATCH = "Con_State_Match";
		public const string strf_CON_ZIP_MATCH = "Con_Zip_Match";
		public const string strf_CONTACT_ACTVY_CASCADE_TICKLING = "Contact_Actvy_Cascade_Tickling";
		public const string strf_CONTACT_ACTIVITIES_ID = "Contact_Activities_Id";
		public const string strf_CONTACT_CASCADE_TICKLING = "Contact_Cascade_Tickling";
		public const string strf_CONTACT_TEAM_MEMBER_ID = "Member_Team_Member_Id";
		public const string strf_CONTACT_ID = "Contact_Id";
		public const string strf_DEFAULT_ARCHIVE_ER_DAYS = "Default_Archive_ER_Days";
		public const string strf_DEFAULT_CURRENCY = "Default_Currency";
		public const string strf_DEFAULT_FORECAST_ER_DAYS = "Default_Forecast_ER_Days";
		public const string strf_DEFAULT_MILESTONE_TEMPLATE = "Default_Milestone_Template";
		public const string strf_DEFAULT_PRIORITY = "Default_Priority";
		public const string strf_DEFAULT_WEB_MP_ID = "Default_Web_MP_Id";
		public const string strf_DEFAULT_WEB_SALES_TEAM_ID = "Default_Web_Sales_Team_Id";
		public const string strf_DEFAULT_WEB_SUPPORT_TEAM = "Default_Web_Support_Team";
		public const string strf_EMAIL = "Email";
		public const string strf_EURO = "Euro";
		public const string strf_INFLUENCER_INFLUENCE_ID = "Influencer_Influence_Id";
		public const string strf_LEAD_CASCADE_TICKLING = "Lead_Cascade_Tickling";
		public const string strf_LITERATURE_LISTING_ID = "Literature_Listing_Id";
		public const string strf_MEETING_CONTACT_ATTENDEE_ID = "Meeting_Contact_Attendee_Id";
		public const string strf_MEETING_STAFF_ATTENDEES_ID = "Meeting_Staff_Attendees_Id";
		public const string strf_MOBILE_ADMIN_EMAIL = "Mobile_Admin_Email";
		public const string strf_OPPORTUNITY_ID = "Opportunity_Id";
		public const string strf_OPPORTUNITY_CASCADE_TICKLING = "Opportunity_Cascade_Tickling";
		public const string strf_OPPORTUNITY__INFLUENCER_ID = "Opportunity__Influencer_Id";
		public const string strf_OPPORTUNITY__PRODUCT_ID = "Opportunity__Product_Id";
		public const string strf_OPPORTUNITY_TEAM_MEMBER_ID = "Opportunity_Team_Member_Id";
		public const string strf_PRODUCT_INTEREST_ID = "Product_Interest_Id";
		public const string strf_RECORD_ID = "Record_Id";
		public const string strf_RESELLER__CUSTOMER_ID = "Reseller__Customer_Id";
		public const string strf_RN_APPOINTMENTS_ID = "Rn_Appointments_Id";
		public const string strf_RN_APPTS_CASCADE_TICKLING = "Rn_Appts_Cascade_Tickling";
		public const string strf_RN_CREATE_DATE = "Rn_Create_Date";
		public const string strf_RN_CREATE_USER = "Rn_Create_User";
		public const string strf_RN_DESCRIPTOR = "Rn_Descriptor";
		public const string strf_RN_EDIT_DATE = "Rn_Edit_Date";
		public const string strf_RN_EDIT_USER = "Rn_Edit_User";
		public const string strf_SYS_TICKLING_ID = "Sys_Tickling_Id";
		public const string strf_SYSTEM_BOOLEAN = "System_Boolean";
		public const string strf_SYSTEM_ID = "System_Id";
		public const string strf_TABLE_INDICATOR = "Table_Indicator";
		public const string strf_DRIP_CAMPAIGN_ACTIVE = "Drip_Campaign_Active";
		public const string strf_ADMIN_WEB_TAB_URL = "Admin_Web_Tab_URL";
		public const string strf_ADMIN_MOBILE_WEB_TAB_UR = "Admin_Mobile_Web_Tab_UR";
		public const string strf_FINANCIAL_CALC_WEB_TAB_URL = "Financial_Calc_Web_Tab_URL";
		public const string strf_FINANCIAL_CALC_MOBILE_WEB_TAB = "Financial_Calc_Mobile_Web_Tab_";
        //Construction_Stage table
        public const string strf_CONSTRUCTION_STAGE_ID = "Construction_Stage_Id";
        //AM2011.02.28 - Document path for QA
        public const string strfTIC_QA_DOCUMENT_PATH = "TIC_QA_Document_Path";

		// Query Name
		public const string strq_ACTIVITIES_WITH_OPPORTUNITY = "PA: Activities with Opportunity ?";
		public const string strq_ALTERNATE_ADDRESSES_OF_COMPANY = "PA: Alternate Addresses of Company ?";
		public const string strq_ALTERNATE_ADDRESSES_OF_CONTACT = "PA: Alternate Addresses of Contact ?";
		public const string strq_ALTERNATE_PHONE_OF_COMPANY = "PA: Alternate Phone #'s of Company?";
		public const string strq_ALTERNATE_PHONE_OF_CONTACT = "PA: Alternate Phone #'s of Contact?";
		public const string strq_APPOINTMENTS_WITH_COMPANY = "PA: Appointments with Company?";
		public const string strq_APPOINTMENT_WITH_CONTACT = "PA: Appointments with Contact?";
		public const string strq_COMPANY_TEAM_MEMBER_OF_COMPANY = "PA: Company Team Member of Company ?";
		public const string strq_CONTACT_ACTIVITIES_WITH_CONTACT = "PA: Contact Activities with Contact?";
		public const string strq_CONTACT_ACTIVITIES_WITH_OPPORTUNITY = "PA: Contact Activities with Opportunity?";
		public const string strq_CONTACT_TEAM_MEMBER_OF_CONTACT = "PA: Contact Team Member of Contact ?";
		public const string strq_CONTACTS_WITH_COMPANY = "PA: Contacts with Company ?";
		public const string strq_LITERATURE_LISTING = "PA: Literauture Listings for Activity ?";
		public const string strq_MEETING_CONTACT_ATTENDEES = "PA: Meeting Contact Attendees for Meeting ?";
		public const string strq_LITERATURE_LISTING_FOR_CONTACT_ACTIVITIES = "PA: Literature Listings for Contact Activities?";
		public const string strq_MEETING_STAFF_ATTENDEES = "PA: Meeting Staff Attendees for Meeting ?";
		public const string strq_OP_INFLUENCER_WITH_CONTACT = "PA: Op. Influencer with Contact ?";
		public const string strq_OP_INFLUENCERS_WITH_OPPORTUNITY_ID = "PA: Op. Influencers with Opportunity Id ?";
		public const string strq_OP_PRODUCT_WITH_OPPORTUNITY_ID = "PA: Op. Products with Opportunity Id ?";
		public const string strq_OPPORTUNITY_TEAM_MEMBER_OF_OPPORTUNITY = "PA: Opportunity Team Member of Opportunity ?";
		public const string strq_OPPORTUNITIES_WITH_COMPANY = "PA: Opportunities with Company ?";
		public const string strq_PRODUCT_INTEREST_FOR_LEAD_ID = "PA: Product Interests for Lead ?";
		public const string strq_RESELLER_CUSTOMERS_FOR_COMPANY = "Reseller Customers for Company ?";
		public const string strq_SYS_TICKLING_TABLE_RECORDS = "PA: Sys Tickling Table Records";
		public const string strq_CONTACT_WEB_DETAILS_WITH_CONTACT = "Sys: Contact Web Details With Contact ?";
		public const string strq_CONTACT_INFLUENCED_FOR_OPPORTUNITY = "Sys: Influenced Contact With Opportunity Id ?";
		public const string strq_CONTACT_INFLUENCING_FOR_OPPORTUNITY = "Sys: Influencing Contact With Opportunity Id ?";
		public const string strqFIND_SYSTEM_WIDE_PROPERTIES_RECORD = "Find \"System Wide Properties\" record";

		// List_Levels
		public const string strtLIST_LEVELS = "List_Levels";
		// Fields used in List_Levels
		public const string strfAGGREGATIONS = "Aggregations";
		public const string strfDESCRIPTOR_FORMULA = "Descriptor_Formula";
		public const string strfFOREIGN_KEY_ID = "Foreign_Key_Id";
		public const string strfHIDE = "Hide";
		public const string strfLEVEL_NUMBER = "Level_Number";
		public const string strfLIST_ID = "List_Id";
		public const string strfLIST_LEVELS_ID = "List_Levels_Id";
		public const string strfRN_CREATE_DATE = "Rn_Create_Date";
		public const string strfRN_CREATE_USER = "Rn_Create_User";
		public const string strfRN_DESCRIPTOR = "Rn_Descriptor";
		public const string strfRN_EDIT_DATE = "Rn_Edit_Date";
		public const string strfRN_EDIT_USER = "Rn_Edit_User";
		public const string strfRN_UNIQUE_NAME = "Rn_Unique_Name";
		public const string strfSELECT_SCRIPT_ID = "Select_Script_Id";
		public const string strfSORT_FIELD_ID = "Sort_Field_Id";
		public const string strfTABLE_ID = "Table_Id";
		// Queries used in List_Levels
		public const string strqLIST_LEVELS_WITH_LIST_ID__AND_NOT_LIST_LEVELS_ID = "Sys: List Levels with List Id ? and Not List Levels Id ?";
		public const string strqLIST_LEVELS_BY_LIST_LEVEL = "PA: List Levels by List?, Level?";
		public const string strqLIST_LEVELS_BY_LIST = "PA: List Levels by List?";
		public const string strqORPHAN_LIST_LEVELS = "PA: Orphan List Levels";
		// Lists
		public const string strtLISTS = "Lists";
		// Fields used in Lists
		public const string strfDATA_SOURCE = "Data_Source";
		public const string strfLIST_NAME = "List_Name";
		public const string strfLISTS_ID = "Lists_Id";
		public const string strfOPEN_SCRIPT_ID = "Open_Script_Id";
		public const string strfORDINAL = "Ordinal";
		public const string strfSTATUS_TEXT = "Status_Text";
		// Queries used in Lists
		public const string strqLISTS_WITH_TABLE_ID__AND_NOT_LISTS_ID = "Sys: Lists with Table Id ? and Not Lists Id ?";
		public const string strqLIST_BY_TABLE__AND_ORDINAL__AND_RECORDID_NOT_ = "Sys: List by Table ? and Ordinal ? and RecordID not = ?";
		public const string strqLISTS_BY_TABLE = "PA: Lists by Table?";
		public const string strqORPHAN_LISTS = "PA: Orphan Lists";
		// Method
		public const string strm_SET_SYSTEM = "SetSystem";
		public const string strm_EXECUTE = "Execute";
		public const string strm_ADD_FORM_DATA = "AddFormData";
		public const string strm_DELETE_FORM_DATA = "DeleteFormData";
		public const string strm_LOAD_FORM_DATA = "LoadFormData";
		public const string strm_NEW_FORM_DATA = "NewFormData";
		public const string strm_SAVE_FORM_DATA = "SaveFormData";
		public const string strm_NEW_SECONDARY_DATA = "NewSecondaryData";
		public const string strmCALCULATE_CUSTOMER_PROFILES = "CalculateCustomerProfiles";
		public const string strmGET_SYSTEM = "GetSystem";
		public const string strmGET_ALL_WEB_TAB_URLS = "GetAllWebTabURLs";
		public const string strmGET_MOBILE_INFO = "GetMobileInfo";
		public const string strmBATCH_UPDATE_QUOTE_EXPIRY = "BatchUpdateQuoteExpiry";
		public const string strmBATCH_UPDATE_PRICING = "BatchUpdatePricing";
		public const string strmBATCH_UPDATE_DNC = "BatchUpdateDNC";
		public const string strmGET_LCS_DATE_TIME = "GetLCSDateTime";
		public const string strmBATCH_UPDATE_LOT_STATUS = "BatchUpdateLotStatus";
		public const string strmBATCH_UPDATE_RELEASE_STATUS = "BatchUpdateReleaseStatus";
		public const string strmBATCH_UPDATE_NBHD_STATUS = "BatchUpdateNBHDStatus";
		public const string strmBATCH_UPDATE_QUOTE_STATUS = "BatchUpdateQuoteStatus";
		public const string strmBATCH_UPDATE_MARKET_PROJECT_STATUS = "BatchUpdateMarketProjectStatus";
		public const string strmUPDATE_NEIGHBORHOOD_STATUS = "UpdateNeighborhoodStatus";
		public const string strmGET_SYSTEM_FIELDS = "GetSystemFields";
        public const string strmGET_RECORDSET_BY_ID = "GetRecordsetById";
        public const string strmCLEAR_CONSTRUCTION_STAGE = "ClearConstructionStages";

		// Language String
		public const string stre_DELETE_RECORD_FAILED = "Error deleting records.";
		public const string stre_TABLE_TICKLE_FAILED = "Table tickle failed.";
		public const string stre_TABLE_TIEKLE_SEND = "Table tickle sent.";
		public const string stre_ERROR_READING_PROPERTIES = "Error reading system-wide properties.";
		public const string stre_ERROR_RESOLVING_RECORDS = "Error resolving records.";
		public const string stre_SYSTEM_ALREADY_EXIST = "System Already Exist";
        public const string stre_ERROR_GETTING_DEFAULT_FORM = "Error getting default form";


		// Error
		public const string gstrEMPTY_STRING = "";
		public const int glngERR_APPDEV_START_NUMBER = -2147221504 + 10000;
		public const int glngERR_APPDEV_END_NUMBER = -2147221504 + 13399;
		public const int glngERR_APPDEV_EXTEND_START_NUMBER = -2147221504 + 13600;
		public const int glngERR_APPDEV_EXTEND_END_NUMBER = -2147221504 + 29999;
		public const int glngERR_START_NUMBER = -2147221504 + 12899;
		public const int glngERR_END_NUMBER = glngERR_START_NUMBER + 199;
		public const int glngERR_METHOD_NOT_DEFINED = -2147221504 + 13401;
		public const int glngERR_PARAMETERS_ARE_EXPECTED = -2147221504 + 13402;
		public const int glngERR_PARAMETER_IS_VALID = -2147221504 + 13403;
		public const int glngERR_ADDFORMDATA_FAILED = -2147221504 + 13404;
		public const int glngERR_DELETEFORMDATA_FAILED = -2147221504 + 13405;
		public const int glngERR_ON_ADDING_NEW_RECORD = -2147221504 + 13406;
		public const int glngERR_EXECUTE_FAILED = -2147221504 + 13407;
		public const int glngERR_LOADFORMDATA_FAILED = -2147221504 + 13408;
		public const int glngERR_NEWFORMDATA_FAILED = -2147221504 + 13409;
		public const int glngERR_NEWSECONDARYDATA_FAILED = -2147221504 + 13410;
		public const int glngERR_SAVEFORMDATA_FAILED = -2147221504 + 13411;
		public const int glngERR_CAN_NOT_DELETE = glngERR_START_NUMBER + 40;
		public const int glngERR_PARAMETER_EXPECTED = glngERR_END_NUMBER;
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
	}
}
