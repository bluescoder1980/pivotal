using System;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;

using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;

namespace CRM.Pivotal.IP
{
    internal class modContactProfileNeighborhood
    {

        // Fields
        public const string strfACCOUNT_MANAGER_ID = "Account_Manager_Id";
        public const string strfACCOUNT_MANAGER_OVERRIDE = "Account_Manager_Override";
        public const string strfACCOUNT_MGR_CHANGED = "Account_Mgr_Changed";
        public const string strfACTIVITY_COMPLETE = "Activity_Complete";
        public const string strfACTIVITY_COMPLETED_DATE = "Activity_Completed_Date";
        public const string strfADDITIONAL_COMMISSION = "Additional_Commission";
        public const string strfADDITIONAL_COMMISSION_PERCENT = "Additional_Commission_Percent";
        public const string strfADDRESS_1 = "Address_1";
        public const string strfADDRESS_2 = "Address_2";
        public const string strfADDRESS_3 = "Address_3";
        public const string strfAGE = "Age";
        public const string strfAGE_RANGE_OF_BUYERS = "Age_Range_Of_Buyers";
        public const string strfAGE_RANGE_OF_CHILDREN = "Age_Range_Of_Children";
        public const string strfANNUAL_REVENUE = "Annual_Revenue";
        public const string strfAPPT_DESCRIPTION = "Appt_Description";
        public const string strfAPPOINTMENT_CANCELED_DATE = "Appointment_Canceled_Date";
        public const string strfARCH_LEAD_ID = "Arch_Lead_Id";
        public const string strfAREA_CODE = "Area_Code";
        public const string strfASSIGNED_TO_PARTNER_CONTACT = "Assigned_To_Partner_Contact";
        public const string strfASSIGNED_TO_RESELLER_ID = "Assigned_To_Reseller_Id";
        public const string strfASSIGNED_BY = "Assigned_By";
        public const string strfASSISTANT_PHONE = "Assistant_Phone";
        public const string strfASSISTANTS_EXTENSION = "Assistants_Extension";
        public const string strfASSISTANTS_NAME = "Assistants_Name";
        public const string strfBUDGET_APPROVED = "Budget_Approved";
        public const string strfBUDGET_DOLLARS = "Budget_Dollars";
        public const string strfCELL = "Cell";
        public const string strfCELL_CDNC = "Cell_CDNC";
        public const string strfCELL_NDNC = "Cell_NDNC";
        public const string strfCITY = "City";
        public const string strfCO_BUYER_ADDRESS_1 = "Co_Buyer_Address_1";
        public const string strfCO_BUYER_ADDRESS_2 = "Co_Buyer_Address_2";
        public const string strfCO_BUYER_ADDRESS_3 = "Co_Buyer_Address_3";
        public const string strfCO_BUYER_AREA_CODE = "Co_Buyer_Area_Code";
        public const string strfCO_BUYER_CELL = "Co_Buyer_Cell";
        public const string strfCO_BUYER_CITY = "Co_Buyer_City";
        public const string strfCO_BUYER_CONTACT_ID = "Co_Buyer_Contact_Id";
        public const string strfCO_BUYER_COUNTY_ID = "Co_Buyer_County_Id";
        public const string strfCO_BUYER_FIRST_NAME = "Co_Buyer_First_Name";
        public const string strfCO_BUYER_LAST_NAME = "Co_Buyer_Last_Name";
        public const string strfCO_BUYER_PHONE = "Co_Buyer_Phone";
        public const string strfCO_BUYER_STATE = "Co_Buyer_State";
        public const string strfCO_BUYER_TITLE = "Co_Buyer_Title";
        public const string strfCO_BUYER_WORK_EXTENSION = "Co_Buyer_Work_Extension";
        public const string strfCO_BUYER_WORK_PHONE = "Co_Buyer_Work_Phone";
        public const string strfCO_BUYER_ZIP = "Co_Buyer_Zip";
        public const string strfCOBUYER_MARRIED_TO_BUYER = "Cobuyer_Married_To_Buyer";
        public const string strfCOMBINED_INCOME_RANGE = "Combined_Income_Range";
        public const string strfCOMMENTS = "Comments";
        public const string strfCOMMUTE = "Commute";
        public const string strfCOMP_MATCH_CODE = "Comp_Match_Code";
        public const string strfCOMPANY_NAME = "Company_Name";
        public const string strfCOMPANY_NAME_SOUNDEX = "Company_Name_Soundex";
        public const string strfCONT_MATCH_CODE = "Cont_Match_Code";
        public const string strfCONTACT = "Contact";
        public const string strfCONTACT_TEAM_MEMBER_ID = "Contact_Team_Member_Id";
        public const string strfCONTACT_EXPIRATION_PERIOD = "Contact_Expiration_Period";
        public const string strfCOUNTRY = "Country";
        public const string strfCOUNTY_ID = "County_Id";
        public const string strfCREATED_BY_EMPLOYEE_ID = "Created_By_Employee_Id";
        public const string strfCURRENCY_ID = "Currency_Id";
        public const string strfCURRENT_MONTHLY_PAYMENT = "Current_Monthly_Payment";
        public const string strfCURRENT_SQUARE_FOOTAGE = "Current_Square_Footage";
        public const string strfDATE_ENTERED = "Date_Entered";
        public const string strfDECISION_DATE = "Decision_Date";
        public const string strfDELTA_ACCOUNT_MANAGER = "Delta_Account_Manager";
        public const string strfDELTA_ACCOUNT_MANAGER_OVERRIDE = "Delta_Account_Manager_Override";
        public const string strfDELTA_AREA_CODE = "Delta_Area_Code";
        public const string strfDELTA_ASSIGNED_TO_PARTNER_CONT = "Delta_Assigned_To_Partner_Cont";
        public const string strfDELTA_COUNTRY = "Delta_Country";
        public const string strfDELTA_PHONE = "Delta_Phone";
        public const string strfDELTA_STATE = "Delta_State";
        public const string strfDELTA_TERRITORY_ID = "Delta_Territory_Id";
        public const string strfDELTA_TYPE = "Delta_Type";
        public const string strfDELTA_ZIP = "Delta_Zip";
        public const string strfDEPARTMENT = "Department";
        public const string strfDESIRED_MONTHLY_PAYMENT = "Desired_Monthly_Payment";
        public const string strfDESIRED_MOVE_IN_DATE = "Desired_Move_In_Date";
        public const string strfDESIRED_PRICE_RANGE = "Desired_Price_Range";
        public const string strfDESIRED_SQUARE_FOOTAGE = "Desired_Square_Footage";
        public const string strfDIST_TERRITORY = "Dist_Territory";
        public const string strfDIVISION = "Division";
        public const string strfDIVISION_ID = "Division_Id";
        public const string strfDNC_STATUS = "DNC_Status";
        public const string strfEDUCATION = "Education";
        public const string strfEMAIL = "Email";
        public const string strfEMAIL_CDNC = "Email_CDNC";
        public const string strfEXTENSION = "Extension";
        public const string strfEXTERNAL_LAST_UPDATE = "External_Last_Update";
        public const string strfEXTERNAL_SOURCE_ID = "External_Source_Id";
        public const string strfEXTERNAL_SOURCE_NAME = "External_Source_Name";
        public const string strfFAMILIAR_NAME = "Familiar_Name";
        public const string strfFAX = "Fax";
        public const string strfFAX_CDNC = "Fax_CDNC";
        public const string strfFAX_NDNC = "Fax_NDNC";
        public const string strfFIRST_NAME = "First_Name";
        public const string strfFIRST_CONTACT_DATE = "First_Contact_Date";
        public const string strfFOR_SALE = "For_Sale";
        public const string strfFULL_NAME = "Full_Name";
        public const string strfGENDER = "Gender";
        public const string strfHAS_SAME_ADDRESS_ID = "Has_Same_Address_Id";
        public const string strfHOME_TYPE = "Home_Type";
        public const string strfHOMES_OWNED = "Homes_Owned";
        public const string strfHOUSEHOLD_SIZE = "Household_Size";
        public const string strfIMPORT_LOG_ID = "Import_Log_Id";
        public const string strfIMPORT_SOURCE_PRIORITY = "Import_Source_Priority";
        public const string strfINDUSTRY_TYPE = "Industry_Type";
        public const string strfINTEREST_LEVEL = "Interest_Level";
        public const string strfINTERNET_DATE = "Internet_Date";
        public const string strfJOB_TITLE = "Job_Title";
        public const string strfLAST_NAME = "Last_Name";
        public const string strfLEAD_DATE = "Lead_Date";
        public const string strfLEAD_NAME_SOUNDEX = "Lead_Name_Soundex";
        public const string strfLEAD_OWNERSHIP = "Lead_Ownership";
        public const string strfLEAD_SOURCE_ID = "Lead_Source_Id";
        public const string strfLEAD_SOURCE_TYPE = "Lead_Source_Type";
        public const string strfMARITAL_STATUS = "Marital_Status";
        public const string strfMARKETING_PROJECT_NAME = "Marketing_Project_Name";
        public const string strfMARKETING_PROJECT_ID = "Marketing_Project_Id";
        public const string strfMATCH_CODE = "Match_Code";
        public const string strfMIDDLE_INITIAL = "Middle_Initial";
        public const string strfMINIMUM_BATHROOMS = "Minimum_Bathrooms";
        public const string strfMINIMUM_BEDROOMS = "Minimum_Bedrooms";
        public const string strfMINIMUM_GARAGE = "Minimum_Garage";
        public const string strfNEXT_FOLLOW_UP_DATE = "Next_Follow_UP_Date";
        public const string strfNEW_REPEAT = "New_Repeat";
        public const string strfNOTES = "Notes";
        public const string strfNP1_FIRST_VISIT_DATE = "NP1_First_Visit_Date";
        public const string strfNP1_NEIGHBORHOOD_ID = "NP1_Neighborhood_Id";
        public const string strfNP1_PROSPECT_RATING = "NP1_Prospect_Rating";
        public const string strfNUMBER_LIVING_AREAS = "Number_Living_Areas";
        public const string strfNUMBER_OF_CHILDREN = "Number_Of_Children";
        public const string strfOPPORTUNITY_ID = "Opportunity_Id";
        public const string strfOTHER_BUILDERS = "Other_Builders";
        public const string strfOTHER_NEIGHBORHOODS = "Other_Neighborhoods";
        public const string strfOWNERSHIP = "Ownership";
        public const string strfPAGER = "Pager";
        public const string strfPARTNER_DETAILS_ID = "Partner_Details_Id";
        public const string strfPHONE = "Phone";
        public const string strfPHONE_CDNC = "Phone_CDNC";
        public const string strfPHONE_NDNC = "Phone_NDNC";
        public const string strfPIN = "PIN";
        public const string strfPLAN_NAME = "Plan_Name";
        public const string strfPOSSIBLE_DUPLICATE = "Possible_Duplicate";
        public const string strfPREFERRED_AREA = "Preferred_Area";
        public const string strfPREFERRED_CONTACT = "Preferred_Contact";
        public const string strfPRICE = "Price";
        public const string strfPRIORITY_CODE_ID = "Priority_Code_Id";
        public const string strfPRODUCT_INTEREST_ID = "Product_Interest_Id";
        public const string strfPRODUCT_INTEREST_TYPE = "Product_Interest_Type";
        public const string strfQUALITY = "Quality";
        public const string strfREALTOR_AGENT_ID = "Realtor_Agent_Id";
        public const string strfREALTOR_ID = "Realtor_Id";
        public const string strfREALTOR_COMPANY_ID = "Realtor_Company_Id";
        public const string strfREASONS_FOR_MOVING = "Reasons_For_Moving";
        public const string strfRECEIVE_LEADS = "Receive_Leads";
        public const string strfREFERRED_BY_CONTACT_ID = "Referred_By_Contact_Id";
        public const string strfREFERRED_BY_EMPLOYEE_ID = "Referred_By_Employee_Id";
        public const string strfRESALE = "Resale";
        public const string strfRN_CREATE_DATE = "Rn_Create_Date";
        public const string strfRN_CREATE_USER = "Rn_Create_User";
        public const string strfRN_DESCRIPTOR = "Rn_Descriptor";
        public const string strfRN_EDIT_DATE = "Rn_Edit_Date";
        public const string strfRN_EDIT_USER = "Rn_Edit_User";
        public const string strfRN_EMPLOYEE_USER_ID = "Rn_Employee_User_Id";
        public const string strfSAME_AS_BUYER_ADDRESS = "Same_as_Buyer_Address";
        public const string strfSINGLE_OR_DUAL_INCOME = "Single_Or_Dual_Income";
        public const string strfSPOUSES_NAME = "Spouses_Name";
        public const string strfSSN = "SSN";
        public const string strfSTART_TIME = "Start_Time";

        public const string strfSTATE_ = "State_";
        public const string strfSTOCK_SYMBOL = "Stock_Symbol";
        public const string strfSUFFIX = "Suffix";
        public const string strfTERRITORY_ID = "Territory_Id";
        public const string strfTICKLE_COUNTER = "Tickle_Counter";
        public const string strfTIME_SEARCHING = "Time_Searching";
        public const string strfTIME_ZONE_ID = "Time_Zone_ID";
        public const string strfTITLE = "Title";
        public const string strfTRANSFERRING_TO_AREA = "Transferring_To_Area";
        public const string strfTYPE = "Type";
        public const string strfVL1_COMPLETE = "VL1_Complete";
        public const string strfVL1_CONTACT_ID = "VL1_Contact_Id";
        public const string strfVL1_EMPLOYEE_ID = "VL1_Employee_Id";
        public const string strfVL1_NEXT_DATE = "VL1_Next_Date";
        public const string strfVL1_NP_NEIGHBORHOOD_ID = "VL1_NP_Neighborhood_Id";
        public const string strfVL1_VISIT_COMMENTS = "VL1_Visit_Comments";
        public const string strfVL1_VISIT_DATE = "VL1_Visit_Date";
        public const string strfVL1_VISIT_NUMBER = "VL1_Visit_Number";
        public const string strfWEB_EDITED = "Web_Edited";
        public const string strfWEB_MEETING_DATE = "Web_Meeting_Date";
        public const string strfWEB_MEETING_TIME = "Web_Meeting_Time";
        public const string strfWEB_PROFILED = "Web_Profiled";
        public const string strfWEB_REGISTERED = "Web_Registered";
        public const string strfWORK_PHONE = "Work_Phone";
        public const string strfWORK_EMAIL = "Work_Email";
        public const string strfWORK_PHONE_CDNC = "Work_Phone_CDNC";
        public const string strfWORK_PHONE_NDNC = "Work_Phone_NDNC";
        public const string strfWALK_IN_DATE = "Walk_In_Date";
        public const string strfWWW = "WWW";
        public const string strfZIP = "Zip";
        public const string strfRESERVATION_DATE = "Reservation_Date";
        public const string strfRESERVATION_EXPIRATION_DATE = "Reservation_Expiration_Date";
        public const string strfCONTACT_ID = "Contact_Id";
        public const string strfNEIGHBORHOOD_ID = "Neighborhood_Id";
        public const string strfCONTACT_PROFILE_NBHD_ID = "Contact_Profile_NBHD_Id";
        public const string strfRN_APPOINTMENTS_ID = "Rn_Appointments_Id";
        public const string strfTRAFFIC_SOURCE_ID = "Traffic_Source_Id";
        public const string strfLEAD_ID = "Lead_Id";
        public const string strfNAME = "Name";
        public const string strfMEMBER_TEAM_MEMBER_ID = "Member_Team_Member_Id";
        public const string strfNEIGHBORHOOD_PROFILE_ID = "Neighborhood_Profile_Id";
        public const string strfFIRST_VISIT_DATE = "First_Visit_Date";
        public const string strfAPPT_DATE = "Appt_Date";
        public const string strfRN_EMPLOYEE_ID = "Rn_Employee_Id";
        public const string strfEMPLOYEE_ID = "Employee_Id";
        public const string strfROLE_ID = "Role_Id";
        public const string strfINACTIVE = "Inactive";
        public const string strfACTIVITY_TYPE = "Activity_Type";
        public const string strfINACTIVE_DATE = "Inactive_Date";
        public const string strfINACTIVE_REASON_ID = "Inactive_Reason_Id";
        // Fields from Opportunity table
        public const string strfQUOTE_TOTAL = "Quote_Total";
        public const string strfLOT_ID = "Lot_Id";
        public const string strfPLAN_NAME_ID = "Plan_Name_Id";
        public const string strfELEVATION_ID = "Elevation_Id";
        public const string strfNBHD_PHASE_ID = "NBHD_Phase_Id";
        public const string strfSTATUS = "Status";
        // Fields from Product table
        public const string strfLOT_NUMBER = "Lot_Number";
        public const string strfTRACT = "Tract";
        public const string strfCONSTRUCTION_STAGE = "Construction_Stage";
        public const string strfDEVELOPMENT_PHASE = "Phase";
        public const string strfBLOCK = "Block_";
        public const string strfBUILDING = "Building";
        public const string strfUNIT = "Unit";
        public const string strfJOB_NUMBER = "Job_Number";
        public const string strfLOT_STATUS = "Lot_Status";
        // Fields from NBHD_Phase table
        public const string strfPHASE_NAME = "Phase_Name";
        // Fields from Neighborhood table
        public const string strfNEIGHBORHOOD_NAME = "Name";
        // Fields from NBHDP_Product table
        public const string strfPLAN_CODE = "Plan_Code";
        public const string strfELEVATION_CODE = "Elevation_Code";
        // Fields from Reason table
        public const string strfREASON_CODE = "Reason_Code";
        // Fields from Rn_Appointment table
        public const string strfACTIVITY_CANCELED = "Activity_Canceled";
        public const string strfAPPOINTMENT_CANCEL_DATE = "Appointment_Canceled_Date";
        // Fields from System Table
        public const string strfSCHEDULED_SCRIPT_DAYS = "Schdld_Scrpt_Hstry_Days_Qury";
        // public const
        // Tables
        public const string strtARCH_LEAD = "Arch_Lead";
        public const string strtALERT = "Alert";
        public const string strtCONTACT_COBUYER = "Contact_CoBuyer";
        public const string strtCONTACT_PROFILE_NEIGHBORHOOD = "Contact_Profile_Neighborhood";
        public const string strtRN_APPOINTMENTS = "Rn_Appointments";
        public const string strtTRAFFIC_SOURCE = "Traffic_Source";
        public const string strtLEAD_ = "Lead_";
        public const string strtNEIGHBORHOOD = "Neighborhood";
        public const string strtCONTACT_TEAM_MEMBER = "Contact_Team_Member";
        public const string strtEMPLOYEE = "Employee";
        public const string strtOPPORTUNITY = "Opportunity";
        public const string strtCONTACT = "Contact";
        public const string strtDIVISION = "Division";
        public const string strtPRODUCT = "Product";
        public const string strtNBHD_PHASE = "NBHD_Phase";
        public const string strtNBHDP_PRODUCT = "NBHDP_Product";
        public const string strtNBHDP_ACTION_PLAN_HISTORY = "NBHDP_Action_Plan_History";
        public const string strtINACTIVE_REASON = "Inactive_Reason";
        public const string strtRN_APPOINTMENT = "Rn_Appointments";
        public const string strtSYSTEM = "System";
        // NBHD Profile Type string
        public const string strNBHDP_TYPE_LEAD = "Lead";
        public const string strNBHDP_TYPE_PROSPECT = "Prospect";
        public const string strNBHDP_TYPE_BUYER = "Buyer";
        public const string strNBHDP_TYPE_CANCELLED = "Cancelled";
        public const string strNBHDP_TYPE_CLOSED = "Closed";
        public const string strNBHDP_TYPE_LOST_OPP = "Lost Opportunity";
        public const string strNBHDP_TYPE_INACTIVE = "Inactive";
        public const string strNBHDP_TYPE_UA_MKT_LEAD = "UA Mkt Lead";
        public const string strNBHDP_TYPE_MKT_LEAD = "Mkt Lead";
        public const string strNBHDP_TYPE_UA_NBHD_LEAD = "UA NBHD Lead";
        public const string strNBHDP_TYPE_NBHD_LEAD = "NBHD Lead";
        // Contact Type string
        public const string strCUSTOMER = "Customer";
        // Quote Type string
        public const string strQUOTE_TYPE_INACTIVE = "Inactive";
        // Inactive Reason
        public const string strINACTIVE_REASON_PUR_ELSE = "Purchased Elsewhere";
        // Queries
        public const string strqNBHD_PROFILE_FOR_CONTACT__NBHD = "HB: NBHD Profile for Contact Id? Neighborhood Id?";
        public const string strqNBHD_PROFILE_FOR_LEAD__NBHD = "HB: NBHD Profile for Lead Id? Neighborhood Id?";
        public const string strqTRAFFIC_SOURCE_OF_CONT_PROF_NBHD = "Sys: Traffic Sources of Cont Prof NBHD ?";
        public const string strqVISIT_LOGS_OF_CONT_PROF_NBHD = "Sys: Visit Logs of Cont Prof NBHD?";
        public const string strqNBHD_PROFILE_FOR_OPPORTUNITY = "HB: NBHD Profile for Opportunity?";
        public const string strqNBHD_PROFILE_SALES_TEAM_FOR_OPP = "HB: NBHD Profile Sales Team Member For Opportunity?";
        public const string strqOPPORTUNITY_FOR_CONTACT = "HB: Opps with Reserved or Sales Requested For Contact?";
        public const string strqACTIVE_SALE_TEAM_FOR_CONTACT_NBHDP = "HB: Active NBHDP Team Members for Contact NBHD Profile?";
        public const string strqCONTACT_ACTIVITY_FOR_NBHD_PROFILE = "HB: Contact Activities for Contact NBHD Profile?";
        public const string strqQUOTE_FOR_CONTACT_AND_NEIGHBORHOOD = "HB: Active Quotes For Contact? Neighborhood?";
        public const string strqACTIVITIES_FOR_LEAD = "HB: Activities for Lead?";
        public const string strqNBHD_PROFILE_WITH_DEFINED_INTERNETDATE_FOR_CONTACT = "HB: NBHD Profile With Defined InternetDate For Contact?";
        public const string strqRESERVED_OR_SALES_REQUEST_QUOTES = "HB: Active Reserved or Sales Request Quotes for NBHD? Contact?";
        public const string strqVISIT_LOGS_FOR_NBHD_PROFILE = "HB: Visit Logs for Contact Profile NBHD?";
        public const string streMKT_LVL_NBHD_OF_DIVISION = "HB: Mkt Lvl NBHD of Division?";
        public const string strqALERTS_WITH_LEAD = "PA: Alerts with Lead Id ?";
        public const string strqVISIT_LOGS_FOR_CONT_PROF_NBHD = "HB: Visit Logs for Contact Profile NBHD Id?";
        public const string strqCTM_WITH_NBHDPROFILE_EMPLOYEE = "Sys: NBHDP Team with NBHDProfile Id? and Employee Id?";
        public const string strqNBHDP_TEAM_WITH_NEIGH_PROFILE = "HB: All NBHDP Team of Neighborhood Profile?";
        public const string strqVISIT_LOGS_FOR_CONTACT = "HB: Visit Logs for Contact ?";
        public const string strqIN_COMPLETE_VISIT_LOGS_FOR_CONTACT = "HB: Not Complete Visit Logs for Contact ?";
        public const string strqDIVISIONS_WITH_CONT_EXP_PER_DEFINED = "HB: Divisions with Cont Exp Period Defined";
        // segments
        public const string strsVISIT_LOGS = "Visit Log";
        // string constants
        public const string strBUYER = "Buyer";
        public const string strCLOSED = "Closed";
        public const string strCANCELLED = "Cancelled";
        // Script names
        // General Script names
        // These script names will exist in all modules
        public const string strsFUNCTION_LIBRARY = "Function Lib";
        public const string strsERRORS = "Errors";
        public const string strsTRANSIT_POINT_PARAMS = "Transit Point Params";
        public const string strsCORE_ERRORS = "Core Error";
        public const string gstrsCORE_TRANSIT_POINT_PARAM = "Core Transit Point Param";
        public const string strsCORE_DL_FUNCTIONLIB = "Core DL Function Lib";
        public const string strsCORE_PL_FUNCTIONLIB = "Core PL Function Lib";
        //kA 6-16-10 converted to IP ASR
        //public const string strsINACTIVATE_NBHD_PROFILE = "PAHB Inactivate Contact Profile Neighborhood";
        //public const string strsCONTACT_PROFILE_NBHD = "PAHB Contact Profile Neighborhood";
        public const string strsCONTACT_PROFILE_NBHD = "TIC Contact Profile Neighborhood";
        public const string strsINACTIVATE_NBHD_PROFILE = "TIC Inactivate Contact Profile Neighborhood";
        // Module specific Script names
        // These script names will exist in all modules
        // If you need to invoke an other client script, declare it as it follows:
        public const string strsGENERIC_CODE_SCRIPT = "Other Generic Code Script";
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
        public const string strmGET_EARLIEST_INTERNET_DATE_OF_NBHD_PROFILE_FOR_CONTACT = "GetEarliestInternetDateOfNBHDProfileForContact";
        public const string strmNBHDPROFILEINACTIVATION = "NBHDProfileInactivation";
        // Module specific procedure names
        // These procedures will exist only in you module
        public const string strmWIRELESS_UPDATE_STATUS = "WirelessUpdateStatus";
        public const string strmCASCADE_DELETE = "CascadeDelete";
        public const string strmCAN_BE_DELETED = "CanBeDeleted";
        public const string strmPOSSIBLE_DUPLICATES = "PossibleDuplicates";
        public const string strmCAN_BE_INACTIVE = "CanBeInactive";
        public const string strmFIND_CONTNBHDPROFILE = "FindContNBHDProfile";
        // Active form name
        public const string strrNBHD_PRODUCT = "HB Neighborhood Plan";
        public const string strrDIVISION_PRODUCT_OTHER = "HB Division Other Option";
        public const string strrDIVISION_PRODUCT_PLAN = "HB Division Plan";
        public const string strrVISIT_LOG = "Visit Log";
        // Active form segments
        public const string strsOCCURRENCES = "Occurrences";
        public const string strsRELEASE_OCCURRENCES = "Release Occurrences";
        public const string strsPREFERENCES = "Preferences";
        public const string strsREQUIRED_CATEGORIES = "Required Categories";
        public const string strsSALES_TEAM = "Sales Team";
        // Active Search Names
        public const string strsrchWIRELESS_ADD_LOCATION = "Wireless Add Location";
        public const string strsrchWIRELESS_ADD_CATEGORY = "Wireless Add Category";
        public const string strsrchWIRELESS_ADD_SUB_CATEGORY = "Wireless Add Sub-Category";
        public const string strsrchWIRELESS_ADD_PROBLEM_CODE = "Wireless Add Problem Code";
        // Segment Name
        public const string strsVISIT_LOG = "Visit Log";
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
        public const int glngERR_CAN_NOT_DELETE = glngERR_SHARED_START_NUMBER + 3;
        // Language Resources
        
        // Group Name in LD_Groups table
        public const string strgCONTACT_PROFILE_NBHD = "Contact Profile NBHD";
        // String Names for CODE_GROUP group in LD_Strings table
        public const string strdDELETION_CANCELED = "Deletion Canceled";
        public const string strdDUPLICATED_SAVE = "Duplicated - Save";
        public const string strdDUPLICATED_ADD = "Duplicated - Add";
        public const string strdCONVERTED_TO_BUYER = "Converted to Buyer";
        public const string strdBUYER_NBHD_PROFILE_INACTIVATED = "Buyer NBHD Profile Inactivated";
        public const string strdALL_OPEN_ACTIVITIES_CANCELLED = "All Open Activities Cancelled";
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

    }

}
