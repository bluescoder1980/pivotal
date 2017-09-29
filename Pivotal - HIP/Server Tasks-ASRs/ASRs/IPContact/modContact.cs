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
    internal class modContact
    {

        // Fields used in table Contact
        public const string strfFIRST_NAME = "First_Name";
        public const string strfLAST_NAME = "Last_Name";
        public const string strfCOMPANY_ID = "Company_Id";
        public const string strfCONTACT_ID = "Contact_Id";
        public const string strfZIP = "Zip";
        public const string strfACCOUNT_MANAGER_ID = "Account_Manager_Id";
        public const string strfACCOUNT_MANAGER_OVERRIDE = "Account_Manager_Override";
        public const string strfPHONE = "Phone";
        public const string strfCITY = "City";
        public const string strfSTATE = "State_";
        public const string strfCONTACT_PROFILE_NBHD_TYPE = "CS_Neighborhood_Profile_Type";
        public const string strfCS_SECONDARY_LEAD_SOURCE_ID = "CS_Secondary_Lead_Source_Id";
        public const string strfCOUNTRY = "Country";
        public const string strfCOUNTY_ID = "County_Id";
        public const string strfFAX = "Fax";
        public const string strfADDRESS_1 = "Address_1";
        public const string strfADDRESS_2 = "Address_2";
        public const string strfADDRESS_3 = "Address_3";
        public const string strfTERRITORY_ID = "Territory_Id";
        public const string strfTIME_ZONE_ID = "Time_Zone_Id";
        public const string strfAREA_CODE = "Area_Code";
        public const string strfDELTA_COMPANY_ID = "Delta_Company_Id";
        public const string strfDELTA_ZIP = "Delta_Zip";
        public const string strfDELTA_ACCOUNT_MANAGER_ID = "Delta_Account_Manager";
        public const string strfDELTA_ACCOUNT_MANAGER_OVERRIDE = "Delta_Account_Manager_Override";
        public const string strfDELTA_PHONE = "Delta_Phone";
        public const string strfDELTA_STATE = "Delta_State";
        public const string strfDELTA_TYPE = "Delta_Type";
        public const string strfDELTA_COUNTRY = "Delta_Country";
        public const string strfDELTA_AREA_CODE = "Delta_Area_Code";
        public const string strfHAS_SAME_ADDR_ID = "Has_Same_Address_Id";
        public const string strfMARITAL_STATUS = "Marital_Status";
        public const string strfSAME_AS_BUYER_ADDR = "Same_as_Buyer_Address";
        public const string strfCLOSE_DATE = "Close_Date";
        public const string strfWORK_OUT_OF_OFFICE = "Works_Out_of_Office";
        public const string strfROLE_ID = "Role_Id";
        public const string strfEMPLOYEE_ID = "Employee_Id";
        public const string strfMATCHCODE = "Match_Code";
        public const string strfACCOUNT_MANAGER_CHANGED = "Account_Mgr_Changed";
        public const string strfUSER_ID = "User_Id";
        public const string strfRN_DESCRIPTOR = "Rn_Descriptor";
        public const string strfTYPE = "Type";
        public const string strfLEAD_TYPE = "Lead_Type";
        public const string strfDNC_STATUS = "DNC_Status";
        public const string strfCS_PRIORITY_CODE_ID = "CS_Priority_Code_Id";
        // Fields used in table Contact_Profile_Neighborhood
        public const string strfCONTACT_PROFILE_NBHD_ID = "Contact_Profile_NBHD_Id";
        public const string strfCONTACT_PROFILE_NBHD_LEAD_ID = "Lead_Id";
        // constants
        public const string strCONTACT_TYPE_PROSPECT = "Prospect";
        public const string strNP_TYPE_BUYER = "Buyer";
        public const string strNP_TYPE_CANCELLED = "Cancelled";
        public const string strNP_TYPE_CLOSED = "Closed";
        public const string strNP_TYPE_UA_MKT_LEAD = "UA Mkt Lead";
        public const string strNP_TYPE_MKT_LEAD = "Mkt Lead";
        public const string strNP_TYPE_UA_NBHD_LEAD = "UA NBHD Lead";
        public const string strNP_TYPE_NBHD_LEAD = "NBHD Lead";
        // Home Builders
        public const string strtCONTACT_MARKETING_PROJECT = "Contact_Marketing_Project";
        public const string strfREALTOR_ID = "Realtor_Id";
        public const string strfWALK_IN_DATE = "Walk_In_Date";
        public const string strfRELEASE_ID = "Release_Id";
        public const string strfINACTIVE = "Inactive";
        public const string strfMEDIA_SOURCE_ID = "Media_Source_Id";
        public const string strfDIVISION_ID = "Division_Id";
        public const string strfLEAD_DATE = "Lead_Date";
        public const string strfRN_APPOINTMENTS_ID = "Rn_Appointments_Id";
        public const string strfACCESS_TYPE = "Access_Type";
        public const string strfACTIVITY_COMPLETE = "Activity_Complete";
        public const string strfACTIVITY_COMPLETED_DATE = "Activity_Completed_Date";
        public const string strfACTIVITY_TYPE = "Activity_Type";
        public const string strfAPPT_DATE = "Appt_Date";
        public const string strfAPPT_DESCRIPTION = "Appt_Description";
        public const string strfAPPT_PRIORITY = "Appt_Priority";
        public const string strfASSIGNED_BY = "Assigned_By";
        public const string strfASSIGNED_BY_CONTACT_CO_ID = "Assigned_By_Contact_Co_Id";
        public const string strfASSIGNED_BY_CONTACT_ID = "Assigned_By_Contact_Id";
        public const string strfSTART_TIME = "Start_Time";
        public const string strfNOTES = "Notes";
        public const string strfRN_EMPLOYEE_ID = "RN_Employee_Id";
        //Osm
        public const string strfCONTACT_SYNC_RECORD = "Contact Sync Record";
        // Contact Co-Buyer
        public const string strtCONTACT_COBUYER = "Contact_CoBuyer";
        public const string strfCONTACT_COBUYER_ID = "Contact_CoBuyer_Id";
        public const string strfCOBUYER_CONTACT_ID = "Co_Buyer_Contact_Id";
        public const string strfCO_BUYER_HOME_PHONE = "Co_Buyer_Home_Phone";
        public const string strfCO_BUYER_WORK_PHONE = "Co_Buyer_Work_Phone";
        public const string strfCO_BUYER_CELL_PHONE = "Co_Buyer_Cell_Phone";
        public const string strfCO_BUYER_CELL = "Co_Buyer_Cell";
        public const string strfCO_BUYER_PHONE = "Co_Buyer_Phone";
        public const string strfCO_BUYER_FIRST_NAME = "Co_Buyer_First_Name";
        public const string strfCO_BUYER_LAST_NAME = "Co_Buyer_Last_Name";
        public const string strfCO_BUYER_EMAIL = "Co_Buyer_Email";
        public const string strfLEAD_DATE_FROM = "Lead_Date_Greater_Than_Equal_To";
        public const string strfLEAD_DATE_TO = "Lead_Date_Less_Than_Equal_To";
        public const string strfVISIT_DATE_FROM = "Visit_Date_Greater_Than_Equal_To";
        public const string strfVISIT_DATE_TO = "Visit_Date_Less_Than_Equal_To";
        public const string strfINACTIVE_DATE_FROM = "Inactive_Date_Greater_Than_Equal_To";
        public const string strfINACTIVE_DATE_TO = "Inactive_Date_Less_Than_Equal_To";
        public const string strfQUOTE_DATE_FROM = "Quote_Date_Greater_Than_Equal_To";
        public const string strfQUOTE_DATE_TO = "Quote_Date_Less_Than_Equal_To";
        public const string strfSALE_DATE_FROM = "Sale_Date_Greater_Than_Equal_To";
        public const string strfSALE_DATE_TO = "Sale_Date_Less_Than_Equal_To";
        public const string strfCLOSE_DATE_FROM = "Close_Date_Greater_Than_Equal_To";
        public const string strfCLOSE_DATE_TO = "Close_Date_Less_Than_Equal_To";
        public const string strfCANCEL_DATE_FROM = "Cancel_Date_Greater_Than_Equal_To";
        public const string strfCANCEL_DATE_TO = "Cancel_Date_Less_Than_Equal_To";
        public const string strqCOBUYER_FOR_CONT_COBUYER = "Sys: Co Buyer Contact for Contact ? and CoBuyer ?";
        public const string strqCO_BUYERS_FOR_CONTACT = "Sys: Contact Co-Buyers of Contact ?";
        public const string strqNBHD_PROFILES_OF_CONTACTS = "HB: Neighborhood Profiles of Contact?";
        // Employee Fields
        public const string strfWORK_EMAIL = "Work_Email";
        public const string strfCONTACT_NOTIFICATION = "Contact_Notification";
        // Contact Team Member Fields
        public const string strfMEMBER_TEAM_MEMBER_ID = "Member_Team_Member_Id";
        public const string strfCOMPANY_CONTACT_ID = "Company_Contact_Id";
        // Order Fields
        public const string strfORDER_BILL_TO_CONTACT_ID = "Bill_To_Contact_Id";
        // Registration Fields
        public const string strfREGISTRATION_ADMINISTRATIVE_CONTACT_ID = "Administrative_Contact_Id";
        public const string strfREGISTRATION_MIS_CONTACT_ID = "MIS_Contact_Id";
        public const string strfREGISTRATION_USER_CONTACT_ID = "User_Contact_Id";
        // Support Contract Fields
        public const string strfSUPPORT_CONTRACT_ADMINISTRATIVE_CONTACT_ID = "Administrative_Contact_Id";
        // Company Fields
        public const string strfREFERRED_BY_EMPLOYEE_ID = "Referred_By_Employee_Id";
        public const string strfLEAD_SOURCE_TYPE = "Lead_Source_Type";
        public const string strfLEAD_SOURCE_ID = "Lead_Source_Id";
        public const string strfCOMPANY_NAME = "Company_Name";
        // Choices
        public const string strcOKAY_TO_CONTACT = "Okay To Contact";
        public const string strcDO_NOT_CONTACT = "Do Not Contact";
        public const string strcLIMITED_CONTACT = "Limited Contact";
        public const string strcREALTOR = "Realtor";
        // Table Names
        public const string strtCONTACT = "Contact";
        public const string strtCOMPANY = "Company";
        public const string strtCOMPANY_CONTACT = "Company_Contact";
        public const string strtCONTACT_TEAM_MEMBER = "Contact_Team_Member";
        public const string strtCONTACT_PROFILE_NBHD = "Contact_Profile_Neighborhood";
        public const string strtTTM = "Territory_Team_Member";
        public const string strtALERT = "Alert";
        public const string strtSUPPORT_INCIDENT = "Support_Incident";
        public const string strtOPPORTUNITY = "Opportunity";
        public const string strtORDER = "Order_";
        public const string strtREGISTRATION = "Registration";
        public const string strtSUPPORT_CONTRACT = "Support_Contract";
        public const string strtTERRITORY = "Territory";
        public const string strtSUB_TERRITORY = "Sub_Territory";
        public const string strtACTION_PLAN_CONTACT_STEP = "Action_Plan_Contact_Step";
        public const string strtACTION_PLAN_STEP = "Action_Plan_Step";
        public const string strtALT_ADDRESS = "Alt_Address";
        public const string strtALT_PHONE = "Alt_Phone";
        public const string strtARCH_ACTIVITY = "Arch_Activity";
        public const string strtARCH_LEAD = "Arch_Lead";
        public const string strtARCH_MEETING_CONT_ATTENDEE = "Arch_Meeting_Cont_Attendee";
        public const string strtCOMPETITIVE_INFORMATION = "Competitive_Information";
        public const string strtTIME_ZONE = "Time_Zone";
        public const string strtCONTACT_WEB_DETAILS = "Contact_Web_Details";
        public const string strtCONTACT_ACTIVITIES = "Contact_Activities";
        public const string strtCONTRACT_NAMED_CONTACT = "Contract_Named_Contact";
        public const string strtCONTRACT_PROHIBITED_CONTACT = "Contract_Prohibited_Contact";
        public const string strtISSUE = "Issue";
        public const string strtLEAD = "Lead_";
        public const string strtMARKETING_PROJECT = "Marketing_Project";
        public const string strtMEETING_CONTACT_ATTENDEE = "Meeting_Contact_Attendee";
        public const string strtOPPORTUNITY_INFLUENCER = "Opportunity__Influencer";
        public const string strtOREDER = "Order";
        public const string strtREGISTRATION_NAMED_CONTACT = "Registration_Named_Contact";
        public const string strtRN_APPOINTMENTS = "Rn_Appointments";
        public const string strtRN_CONTACT_SYNC = "Rn_Contact_Sync";
        public const string strtSUPPORT_REQUEST = "Support_Request";
        public const string strtSUPPORT_STEP = "Support_Step";
        public const string strtTMP_CONNECTION = "Tmp_Connection";
        public const string strtCURRENCY = "Currency_";
        public const string strtEMPLOYEE = "Employee";
        public const string strtSAVED_LISTS = "Saved_Lists";
        public const string strtSAVED_LIST_ITEMS = "Saved_List_Items";
        

        // Queries
        public const string strqPOSSIBLE_DUPLICATE = "Sys: Possible Contact Duplicates Match Code Only?";
        public const string strqMEMBER_OF_TERRITORY = "PA: Members of Territory ?";
        public const string strqSUPPORT_INCIDENT_WITH_CONTACT = "Sys: Support Incident with Contact?";
        public const string strqORDER_WITH_BILL_TO_CONTACT = "Sys: Order with Bill To Contact?";
        public const string strqREGISTRATION_WITH_ADMINISTRATIVE_CONTACT = "Sys: Registration with Administrative Contact?";
        public const string strqREGISTRATION_WITH_MIS_CONTACT = "Sys: Registration with MIS Contact?";
        public const string strqREGISTRATION_WITH_USER_CONTACT = "Sys: Registration with User Contact?";
        public const string strqSUPPORT_CONTRACT_WITH_ADMINISTRATIVE_CONTACT = "Sys: Support Contract with Administrative Contact?";
        public const string strqOPPORTUNITIES_WITH_CONTACT = "PA: Opportunities with Contact ?";
        public const string strqACTION_PLAN_CONTACT_STEP_WITH_ASSIGNED_TO_ID = "Sys: Action Plan Contact Step with Assigned To Id?";
        public const string strqALERT_WITH_CONTACT = "PA: Alerts with Contact Id ?";
        public const string strqALERT_WITH_COMPANY = "PA: Alerts with Company Id ?";
        public const string strqALERT_WITH_LEAD = "PA: Alerts with Lead Id ?";
        public const string strqALERT_WITH_MARKETING_PROJECT = "PA: Alerts with Marketing Project Id ?";
        public const string strqALERT_WITH_OPPORTUNITY = "PA: Alerts with Opportunity Id ?";
        public const string strqALT_ADDRESSES_OF_CONTACT = "PA: Alternate Addresses of Contact ?";
        public const string strqALT_PHONE_WITH_CONTACT_ID = "PA: Alternate Phone #'s of Contact?";
        public const string strqARCH_ACTIVITY_WITH_ASSIGNED_BY_CONTACT_ID = "Sys: Arch Activity With Assigned By Contact Id?";
        public const string strqARCH_ACTIVITY_WITH_CONTACT_ID = "Sys: Arch Activity With Contact Id?";
        public const string strqARCH_LEAD_WITH_ASSIGNED_TO_PARTNER_CONTACT = "Sys: Arch Lead With Assigned To Partner Contact Id";
        public const string strqARCH_LEAD_WITH_REFERRED_BY_CONTACT_ID = "Sys: Arch Lead With Referred By Contact Id?";
        public const string strqARCH_MEETING_CONT_ATTENDEE_WITH_CONTACT_ID = "Sys: Arch Meeting Cont Attendee With Contact Id?";
        public const string strqCOMPANY_CONTACT_WITH_CONTACT_ID = "Sys: Company Contacts for Contact ?";
        public const string strqCOMPANY_WITH_PARTNER_CONTACT_ID = "Sys: Company With Partner Contact Id?";
        public const string strqCOMPANY_WITH_REFERRED_BY_ID = "Sys: Company With Referred By Id?";
        public const string strqCOMPANY_WITH_RESELLER_KEY_CONTACT_ID = "Sys: Company With Reseller Key Contact Id?";
        public const string strqCOMPANY_WITH_SUPPLIER_ACCOUNT_MANAGER_ID = "Sys: Company With Supplier Account Manager Id?";
        public const string strqCOMPETITIVE_INFORMATION_WITH_CONTACT_ID = "Sys: Competitive Information With Contact Id?";
        public const string strqCOMPANY_IN_TERRITORY_PARTNER_WITH_TERRITORY_ID = "PA: Companies in Territory Partner w/ Territory ?";
        public const string strqCWD_WITH_LOGIN_AND_PASSWORD_AND_ID = "PA: CWD with Login ? and Password ? Rec Id Not ?";
        public const string strqCWD_WITH_LOGIN_AND_PASSWORD = "Sys: CWD with Login ? and Password ?";
        public const string strqCONTACT_WITH_PARTNER_CONTACT_ID = "Sys: Contact With Partner Contact Id ?";
        public const string strqCONTACT_WITH_REPORTS_TO_CONTACT_ID = "Sys: Contact With Reports To Id?";
        public const string strqCONTACT_ACTIVITIES_WITH_ASSIGNED_BY_CONTACT_ID = "Sys: Cont Act With Assigned By Contact Id?";
        public const string strqCONTACT_ACTIVITIES_WITH_ASSIGNED_TO_CONTACT_ID = "Sys: Cont Act With Assigned To Contact Id?";
        public const string strqCONTACT_ACTIVITIES_WITH_CONTACT_ID = "PA: Contact Activities with Contact?";
        public const string strqCONTACT_ACTIVITIES_WITH_DELTA_ASSIGNED_BY_CONTACT_ID = "Sys: Cont Act With Delta Assigned By Contact Id?";
        public const string strqCONTACT_ACTIVITIES_WITH_DELTA_ASSIGNED_TO_CONTACT_ID = "Sys: Cont Act With Delta Assigned To Contact Id?";
        public const string strqCONTACT_ACTIVITIES_WITH_DELTA_CONTACT_ID = "Sys: Cont Act With Delta Contact Id?";
        public const string strqCONTACT_TEAM_MEMBER_WITH_CONTACT_ID = "PA: Contact Team Member of Contact ?";
        public const string strqCONTACT_WEB_DETAILS_WITH_CONTACT_ID = "Sys: CWD With Contact Id?";
        public const string strqCONTRACT_NAMED_CONTACT_WITH_CONTACT_ID = "Sys: Contract Named Contact With Contact Id?";
        public const string strqCONTRACT_PROHIBITED_CONTACT_WITH_CONTACT_ID = "Sys: Contract Prohibited Contact With Contact Id?";
        public const string strqISSUE_WITH_REPORTED_BY_CONTACT_ID = "Sys: Issue With Reported By Contact Id?";
        public const string strqLEAD_WITH_ASSIGNED_TO_PARTNER_CONTACT = "Sys: Lead With Assigned To Partner Contact?";
        public const string strqLEAD_WITH_REFFERRED_BY_CONTACT_ID = "Sys: Lead With Referred By Contact Id?";
        public const string strqMARKETING_PROJECT_WITH_CONTACT_ID = "Sys: Marketing Projects of Contact ?";
        public const string strqMEETING_CONTACT_ATTENDEE_WITH_CONTACT_ID = "PA: Meeting Contact Attendee with Contact?";
        public const string strqOPPORTUNITY_WITH_DELTA_CONTACT_ID = "Sys: Opportunity With Delta Contact Id?";
        public const string strqOPPORTUNITY_WITH_PARTNER_CONTACT_ID = "Sys: Opportunity With Partner Contact Id?";
        public const string strqOPPORTUNITY_INFLUENCER_WITH_INFLUENCER_ID = "PA: Op. Influencer with Contact ?";
        public const string strqORDER_WITH_PARTNER_CONTACT_ID = "Sys: Order With Partner Contact Id?";
        public const string strqORDER_WITH_SHIP_TO_CONTACT_ID = "Sys: Order With Ship To Contact Id?";
        public const string strqDUPLICATE_CONTACTS_CHECK_WITH_ZIP = "HB: Check Duplicate Contacts with Zip ?";
        public const string strqREGISTRATION_NAMED_CONTACT_WITH_CONTACT_ID = "Sys: Registration Named Contact With Contact Id?";
        public const string strqRN_APPOINTMENTS_WITH_ASSIGNED_BY_CONTACT_ID = "Sys: Schedule With Assigned By Contact Id?";
        public const string strqRN_APPOINTMENTS_WITH_CONTACT = "PA: Appointments with Contact?";
        public const string strqRN_APPOINTMENTS_WITH_DELTA_CONTACT = "Sys: Schedule With Delta Contact?";
        public const string strqRN_CONTACT_SYNC_WITH_CONTACT_ID = "Sys: Rn Contact Sync With Contact Id?";
        public const string strqSUPPORT_INCIDENT_WITH_PARTNER_CONTACT_ID = "Sys: Support Incident With Partner Contact?";
        public const string strqSUPPORT_INCIDENT_WITH_RECORDED_BY_CONTACT_ID = "Sys: Support Incident With Recorded By Contact Id?";
        public const string strqSUPPORT_REQUEST_WITH_CONTACT_ID = "Sys: Support Request With Contact Id?";
        public const string strqSUPPORT_STEP_WITH_CONTACT_ID = "Sys: Support Step With Contact Id?";
        public const string strqSUPPORT_STEP_WITH_RECORDED_BY_CONTACT_ID = "Sys: Support Step With Recorded By Contact Id?";
        public const string strqACTION_PLAN_STEPS_WITH_PLAN = "PA: Action Plan Steps with Plan ?";
        public const string strqEMPLOYEES_ARE_ACCOUNT_MANAGERS = "Sys: Employees are Account Managers";
        public const string strqACTIVE_EMPLOYEES = "Sys: Active Employees";
        public const string strqCONTACT_SAVED_LISTS = "PA: Contact Static Lists";
        public const string strqCONTACT_ITEM_IN_LIST = "PA: Contact Item in List ?";
        public const string strqCONTACT_WEB_DETAILS_WITH_CONTACT = "Sys: Contact Web Details With Contact ?";
        public const string strqCONTACT_SEARCH = "Sys: Contact Search";
        public const string strqCONTACT_SEARCH1 = "Sys: Contact Search1";
        public const string strqALL_CONTACTS_OF_TYPE_CUST = "Sys: Contacts Prospects Buyers Not Contact?";
        public const string strqCHECK_DUPLICATE_CONTACTS = "HB: Check Duplicate Contacts";
        public const string strqREALTOR_SEARCH_ONLY = "HB: Realtor Search Only";
        public const string strqREALTOR_SEARCH_QUOTE = "HB: Realtor Search (Quote)";
        public const string strqREALTOR_SEARCH_COMPANY = "HB: Realtor Search (Company)";
        public const string strqREALTOR_SEARCH_QUOTE_AND_COMPANY = "HB: Realtor Search (Quote and Company)";
        public const string strqCOMPANY_CONTACT_REALTOR_TYPE = "HB: Company? Contact? Realtor Type";
        public const string strqCONTACTS_FOR_GIVENID = "Sys: Contacts with Contact Id ?";
        public const string strqNBHD_PROFILES_FOR_CONTACT = "HB: Neighborhood Profiles of Contact?";

        // Fields used in Alert
        public const string strfALERT_ID = "Alert_Id";
        public const string strfALERT_TEXT = "Alert_Text";
        public const string strfMARKETING_PROJECT_ID = "Marketing_Project_Id";
        public const string strfLEAD_ID = "Lead__Id";
        public const string strfOPPORTUNITY_ID = "Opportunity_Id";
        public const string strfVALID_UNTIL = "Valid_Until";
        public const string strfDELETE_AFTER = "Delete_After";
        // Fields used in Opportunity
        public const string strfESTIMATED_TOTAL = "Estimated_Total";
        public const string strfCURRENCY_ID = "Currency_Id";
        // Fields used in Territory
        public const string strfTERRITORY_NAME = "Territory_Name";
        // Fields used in Action_Plan_Contact_Step
        public const string strfACTION_PLAN_CONTACT_STEP_ID = "Action_Plan_Contact_Step_Id";
        public const string strfASSIGNED_TO_ID = "Assigned_To_Id";
        // Fields used in Saved_Lists
        public const string strfSAVED_LIST_ID = "Saved_Lists_Id";
        // Fields used in Saved_List_
        public const string strfRECORD_ID = "Record_Id";
        public const string strfALT_ADDRESS_ID = "Alt_Address_Id";
        public const string strfALT_PHONE_ID = "Alt_Phone_Id";
        public const string strfARCH_ACTIVITY_ID = "Arch_Activity_Id";
        public const string strfASSIGNED_TO_CONTACT_ID = "Assigned_To_Contact_Id";
        public const string strfARCH_LEAD_ID = "Arch_Lead_Id";
        public const string strfASSIGNED_TO_PARTNER_CONTACT = "Assigned_To_Partner_Contact";
        public const string strfREFERRED_BY_CONTACT_ID = "Referred_By_Contact_Id";
        public const string strfARCH_MEETING_CONT_ATTENDEE_ID = "Arch_Meeting_Cont_Attendee_Id";
        public const string strfPARTNER_CONTACT_ID = "Partner_Contact_Id";
        public const string strfREFERRED_BY_ID = "Referred_By_Id";
        public const string strfRESELLER_KEY_CONTACT_ID = "Reseller_Key_Contact_Id";
        public const string strfSUPPLIER_ACCOUNT_MANAGER_ID = "Supplier_Account_Manager_Id";
        public const string strfCOMPETITIVE_INFORMATION_ID = "Competitive_Information_Id";
        public const string strfCONNECTED_CONTACT_ID = "Connected_Contact_Id";
        public const string strfFOCAL_CONTACT_ID = "Focal_Contact_Id";
        public const string strfREPORTS_TO_ID = "Reports_To_Id";
        public const string strfDELTA_ASSIGNED_BY_CONTACT_ID = "Delta_Assigned_By_Contact_Id";
        public const string strfDELTA_ASSIGNED_TO_CONTACT_ID = "Delta_Assigned_To_Contact_Id";
        public const string strfREPORTED_BY_CONTACT_ID = "Reported_By_Contact_Id";
        public const string strfINFLUENCER_ID = "Influencer_Id";
        public const string strfSHIP_TO_CONTACT_ID = "Ship_To_Contact_Id";
        public const string strfCONTACT = "Contact";
        public const string strfDELTA_CONTACT = "Delta_Contact";
        public const string strfDELTA_CONTACT_ID = "Delta_Contact_Id";
        public const string strfRECORDED_BY_CONTACT_ID = "Recorded_By_Contact_Id";
        public const string strfCONTACT_ACTIVITIES_ID = "Contact_Activities_Id";
        public const string strfCONTRACT_NAMED_CONTACT_ID = "Contract_Named_Contact_Id";
        public const string strfCONTRACT_PROHIBITED_CONTACT_ID = "Contract_Prohibited_Contact_Id";
        public const string strfISSUE_ID = "Issue_Id";
        public const string strfOPPORTUNITY_INFLUENCER_ID = "Opportunity__Influencer_Id";
        public const string strfORDER_ID = "Order__Id";
        public const string strfREGISTRATION_NAMED_CONTACT_ID = "Registration_Named_Contact_Id";
        public const string strfRN_CONTACT_SYNC_ID = "Rn_Contact_Sync_Id";
        public const string strfSUPPORT_INCIDENT_ID = "Support_Incident_Id";
        public const string strfSUPPORT_REQUEST_ID = "Support_Request_Id";
        public const string strfSUPPORT_STEP_ID = "Support_Step_Id";
        public const string strfTMP_CONNECTION_ID = "Tmp_Connection_Id";
        public const string strfMEETING_CONTACT_ATTENDEE_ID = "Meeting_Contact_Attendee_Id";
        public const string strfACTION_PLAN_ID = "Action_Plan_Id";
        public const string strfCURRENCY_NAME = "Currency_Name";
        // Fields used in Time_Zone
        public const string strfTIME_ZONE_OFFSET = "Time_Zone_Offset";
        public const string strfTZI = "TZI";
        // Fields in Web Details
        public const string strfCONTACT_WEB_DETAILS_ID = "Contact_Web_Details_Id";
        public const string strfPASSWORD = "Password_";
        public const string strfPASSWORD_ENCRYPT = "Password_Encrypt";
        public const string strfLOGIN_NAME = "Login_Name";
        public const string strfCONTACT_EMAIL_ADDRESS = "Contact_Email_Address";
        // Fields in Conatct Search
        public const string strfLOT_NUMBER = "Lot_Number";
        public const string strfBUL_NUMBER = "BUL_Number";
        public const string strfSERVICE_REQUEST = "Service_Request";
        public const string strfSERVICE_ITEM = "Service_Item";
        public const string strfWORK_ORDER = "Work_Order";
        public const string strfNEIGHBORHOOD_ID = "Neighborhood_Id";
        public const string strfEMAIL = "Email";
        public const string strfHOME_PHONE = "Home_Phone";
        public const string strfWORK_PHONE = "Work_Phone";
        public const string strfCELL_PHONE = "Cell_Phone";
        public const string strfCS_INACTIVE_REASON = "CS_Inactive_Reason";
        public const string strfCS_CANCEL_REASON_ID = "CS_Cancel_Reason_Id";
        // Segments in Contact Search
        public const string strsCONTACT_SEARCH = "Contact Search";
        public const string strsSERVICES_AND_WARRENTY = "Services and Warrenty";
        public const string strsDATES = "Dates";
        public const string strsBUYERS = "Buyers";
        public const string strsCLOSED = "Closed";
        public const string strsCANCELLED = "Cancelled";
        public const string strsINACTIVE = "Inactive";
        // Segment in Realtor Search
        public const string strsREALTOR_SEARCH = "Realtor Search";
        // Segments and tabs in HB Quick Contact
        public const string strsNEIGHBORHOOD_PROFILE = "Neighborhood Profile";
        public const string strtPROFILE = "Profile";
        // Scripts
        public const string strsTERRITORY_MGMT = "Territory Mgmt";
        public const string strsFUNCTION_LIBRARY = "Function Lib";
        public const string strsALERT = "PAHB Alert";
        public const string strsCONTACT_PROFILE_NBHD = "PAHB Contact Profile Neighborhood";
        public const string strsCURRENCY = "Currency";
        public const string strsSYSSYSTEM = "System";
        public const string strsACTION_PLAN = "Action Plan";
        public const string strsTRANSIT_POINT_PARAMS = "Transit Point Params";
        public const string strsERRORS = "Errors";
        public const string strsMESSAGE_MASTER = "Message Master";
        public const string strsINTEGRATION = "PAHB Integration";
        public const string strsCORE_DL_FUNCTIONLIB = "Core DL Function Lib";
        public const string strsCORE_PL_FUNCTIONLIB = "Core PL Function Lib";
        public const string strsINACTIVATE_NBHD_PROFILE = "PAHB Inactivate Contact Profile Neighborhood";
        
        // error Groups
        public const string strgCONTACT = "Contact";
        public const string strgERRORS = "Errors";
        // LD Strings
        // Form errors strings
        public const string strldstrLOADFORMDATA_FAILED = "LoadFormDataFailed";
        public const string strldstrSAVEFORMDATA_FAILED = "SaveFormDataFailed";
        public const string strldstrNEWFORMDATA_FAILED = "NewFormDataFailed";
        public const string strldstrADDFORMDATA_FAILED = "AddFormDataFailed";
        public const string strldstrDELETEFORMDATA_FAILED = "DeleteFormDataFailed";
        public const string strldstrEXECUTE_FAILED = "ExecuteFailed";
        public const string strldstrSETSYSTEM_FAILED = "SetSystemFailed";
        // Contact Business violation errors strings
        public const string strldstrHAS_SUPPORT_INCIDENT = "Has Support Incident";
        public const string strldstrHAS_OPPORTUNITY = "Has Opportunity";
        public const string strldstrHAS_ORDER = "Has Order";
        public const string strldstrHAS_REGISTRATION = "Has Registration";
        public const string strldstrHAS_SUPPORT_CONTRACT = "Has Support Contract";
        // Contact methods called by Execute errors strings
        public const string strldstrHASDUPLICATES_FAILED = "HasDuplicates Failed";
        public const string strldstrGETDEFAULTTEAMMEMBERS_FAILED = "GetDefaultTeamMembers Failed";
        public const string strldstrESTIMATEDTOTALREVENUE_FAILED = "EstimatedTotalRevenue Failed";
        public const string strldstrLOCALTIME_FAILED = "LocalTime Failed";
        public const string strldstrEXITTERRITORY_FAILED = "ExitTerritory Failed";
        public const string strldstrGETCOMPANY_FAILED = "GetCompany Failed";
        public const string strldstrGETCOMPANYFOROPP_FAILED = "GetCompanyForOpp Failed";
        public const string strldstrFILLINCURRENCY_FAILED = "FillInCurrency Failed";
        public const string strldstrLISTPARTNER_FAILED = "ListPartner Failed";
        public const string strldstrGETWEBDETAILS_FAILED = "GetWebDetails Failed";
        public const string strldstrGETTERRITORYINFO_FAILED = "GetTerritoryInfo Failed";
        public const string strldstrEXPORTCONTACTSTOOUTLOOK_FAILED = "ExportContactsToOutlook Failed";
        public const string strldstrGETEMPLOYEES_FAILED = "GetEmployees Failed";
        public const string strldstrGETCONTACTSAVEDLISTS_FAILED = "GetContactSavedLists Failed";
        // CWD Business violation errors strings
        public const string strldstrCWD_NO_COMPANY = "CWD No Company";
        public const string strldstrCWD_DUPLICATE_FOUND = "CWD Duplicate Found";
        // other strings
        public const string strldstrBLANK = "Blank";
        // PHub strings
        public const string strldstrPHUB_UNSUCCESSFUL_SAVE = "PHub Unsuccessful Save";
        public const string strldstrPHUB_SUCCESSFUL_SAVE = "PHub Successful Save";
        public const string strldstrPHUB_SUCCESSFUL_SAVE_NO_ACCT_MGR = "PHub Successful Save No Acct Mgr";
        public const string strldstrAM_NOTIFICATION_EMAIL_SUBJECT = "AM Notification Email Subject";
        public const string strldstrAM_NOTIFICATION_EMAIL_BODY = "AM Notification Email Body";
        public const string strldstrAM_CHANGED = "AM Changed";
        // Homebuilders strings
        public const string strldstrERROR_DELETECHILDRENFIRST = "Error_DeleteChildrenFirst";
        // Contact methods
        // Form methods
        public const string strmSAVEFORMDATA = "SaveFormData";
        public const string strmLOADFORMDATA = "LoadFormData";
        public const string strmADDFORMDATA = "AddFormData";
        public const string strmNEWFORMDATA = "NewFormData";
        public const string strmDELETEFORMDATA = "DeleteFormData";
        public const string strmEXECUTE = "Execute";
        public const string strmSETSYSTEM = "SetSystem";
        // Methods used by Execute
        public const string strmCONTACT_SEARCH = "Contact Search";
        public const string strmREALTOR_SEARCH = "Realtor Search";
        public const string strmESTIMATED_TOTAL_REVENUE = "EstimatedTotalRevenue";
        public const string strmGET_COMPANY = "GetCompany";
        public const string strmLIST_PARTNER = "ListPartner";
        public const string strmEXIT_TERRITORY = "ExitTerritory";
        public const string strmGET_DEFAULT_TEAM_MEMBERS = "GetDefaultTeamMembers";
        public const string strmHAS_DUPLICATES = "HasDuplicates";
        public const string strmGET_COMPANY_FOR_OPP = "GetCompanyForOpp";
        public const string strmEXPORT_CONTACTS_TO_OUTLOOK = "ExportContactsToOutlook";
        public const string strmGET_EMPLOYEES = "GetEmployees";
        public const string strmGET_CONTACT_SAVED_LISTS = "GetContactSavedLists";
        public const string strmLINK_CONTACT_COBUYER = "LinkContactCobuyer";
        public const string strmGET_CONTACTS = "GetContacts";
        public const string strmDELETE_COBUYER_LINK = "DeleteCoBuyer";
        public const string strmHAS_EMAIL_RECIPIENTS = "HasEmailRecipients";
        public const string strmIS_BUYER_COBUYER_ADDR_SAME = "IsBuyerCoBuyerAddressSame";
        public const string strmCOPY_BUYER_ADDR_TO_COBUYER = "CopyBuyerAddressToCoBuyer";
        public const string strmHB_MERGE_CONTACT = "HB_MergeContact";
        public const string strmGET_WEB_DETAILS = "GetWebDetails";
        public const string strmGET_OSM_STATUS = "GetOSMStatus";
        public const string strmADD_TO_OSM = "AddtoOSM";
        public const string strmADD_MULTIPLE_TO_OSM = "AddMultipletoOSM";
        // private methods
        public const string strmFOUND_CONTACT_TEAM_MEMBERS = "FoundContactTeamMembers";
        public const string strmWITH_ASSOCIATED_LINKS = "WithAssociatedLinks";
        public const string strmHAS_SUPPORT_INCIDENT = "HasSupportIncident";
        public const string strmHAS_OPPORTUNITY = "HasOpportunity";
        public const string strmHAS_ORDER = "HasOrder";
        public const string strmHAS_REGISTRATION = "HasRegistration";
        public const string strmHAS_SUPPORT_CONTRACT = "HasSupportContract";
        public const string strmDELETE_OR_SET_NULL = "DeleteOrSetNull";
        public const string strmCASCADE_DELETE = "CascadeDelete";
        public const string strmGET_ACCOUNT_MANAGER = "GetAccountManager";
        public const string strmCREATE_RECORDSET = "CreateRecordset";
        public const string strmUPDATE_DELTA_FIELDS = "UpdateDeltaFields";
        public const string strmSET_TO_COMPANY_INFO = "SetToCompanyInfo";
        public const string strmIS_ADDRESS_CHANGED = "IsBuyerCoBuyerAddressSame";
        public const string strmCOPY_ADDRESS = "CopyBuyerAddressToCoBuyer";
        public const string strmIS_TERRITORY_FIELDS_CHANGED = "IsTerritoryFieldsChanged";
        public const string strmCAN_BE_DELETED = "CanBeDeleted";
        public const string strmCONTACT_DUPLICATE = "Contact Duplicate";
        public const string strmMERGE_CONTACT = "Merge Contact";
        public const string strmIS_INTEGRATION_ON = "IsIntegrationOn";
        public const string strmNOTIFY_INTEGRATION_OF_BUYER_CHANGE = "NotifyIntegrationOfBuyerChange";
        // CWD method
        // CWD private methods
        public const string strmGET_COMPANY_ID = "GetCompanyId";
        public const string strmCHECK_OK_TO_SAVE = "CheckOKToSave";
        // Error Ranges
        public const int lngERR_LB_CONTACT_ERR_NO = 10700 + -2147221504;
        public const int lngERR_UB_CONTACT_ERR_NO = 10899 + -2147221504;
        public const int lngERR_LB_AD_ERR_NO = 10000 + -2147221504;
        public const int lngERR_UB_AD_BV_ERR_NO = 13399 + -2147221504;
        public const int lngERR_LB_FUTURE_EXPANSION_NO = 13600 + -2147221504;
        public const int lngERR_UB_AD_ERR_NO = 29999 + -2147221504;
        // Form errors
        public const int glngERR_PARAMETER_EXPECTED = 13402 + -2147221504;
        public const int lngERR_DELETEFORMDATA_FAILED = 13405 + -2147221504;
        public const int lngERR_LOADFORMDATA_FAILED = 13408 + -2147221504;
        public const int lngERR_NEWFORMDATA_FAILED = 13409 + -2147221504;
        public const int lngERR_SAVEFORMDATA_FAILED = 13411 + -2147221504;
        public const int lngERR_ADDFORMDATA_FAILED = 13404 + -2147221504;
        public const int lngERR_EXECUTE_FAILED = 13407 + -2147221504;
        public const int lngERR_SETSYSTEM_FAILED = 13412 + -2147221504;
        // Contact Business violation errors
        public const int lngERR_HAS_SUPPORT_INCIDENT = lngERR_LB_CONTACT_ERR_NO + 10;
        public const int lngERR_HAS_OPPORTUNITY = lngERR_LB_CONTACT_ERR_NO + 11;
        public const int lngERR_HAS_ORDER = lngERR_LB_CONTACT_ERR_NO + 12;
        public const int lngERR_HAS_REGISTRATION = lngERR_LB_CONTACT_ERR_NO + 13;
        public const int lngERR_HAS_SUPPORT_CONTRACT = lngERR_LB_CONTACT_ERR_NO + 14;
        // Contact methods called by execute errors
        public const int lngERR_HASDUPLICATES_FAILED = lngERR_LB_CONTACT_ERR_NO + 60;
        public const int lngERR_GETDEFAULTTEAMMEMBERS_FAILLED = lngERR_LB_CONTACT_ERR_NO + 61;
        public const int lngERR_ESTIMATEDTOTALREVENUE_FAILED = lngERR_LB_CONTACT_ERR_NO + 62;
        public const int lngERR_LOCAL_TIME_FAILED = lngERR_LB_CONTACT_ERR_NO + 63;
        public const int lngERR_EXITTERRITORY_FAILED = lngERR_LB_CONTACT_ERR_NO + 64;
        public const int lngERR_GETCOMPANY_FAILED = lngERR_LB_CONTACT_ERR_NO + 65;
        public const int lngERR_GETCOMPANYFOROPP_FAILED = lngERR_LB_CONTACT_ERR_NO + 66;
        public const int lngERR_FILLINCURRENCY_FAILED = lngERR_LB_CONTACT_ERR_NO + 67;
        public const int lngERR_LISTPARTNER_FAILED = lngERR_LB_CONTACT_ERR_NO + 68;
        public const int lngERR_GETWEBDETAILS_FAILED = lngERR_LB_CONTACT_ERR_NO + 69;
        public const int lngERR_GETTERRITORYINFO_FAILED = lngERR_LB_CONTACT_ERR_NO + 70;
        public const int lngERR_EXPORTCONTACTSTOOUTLOOK_FAILED = lngERR_LB_CONTACT_ERR_NO + 71;
        public const int lngERR_GETEMPLOYEES_FAILED = lngERR_LB_CONTACT_ERR_NO + 72;
        public const int lngERR_GETCONTACTSAVEDLISTS_FAILED = lngERR_LB_CONTACT_ERR_NO + 73;
        // Contact private function errors
        public const int lngERR_DELETEORSETNULL_FAILED = lngERR_LB_CONTACT_ERR_NO + 74;
        // CWD Business violation errors
        public const int lngERR_CWD_NO_COMPANY = lngERR_LB_CONTACT_ERR_NO + 150;
        public const int lngERR_CWD_DUPLICATE_FOUND = lngERR_LB_CONTACT_ERR_NO + 151;
        // Source Names
        public const string strsrcCONTACT = "PHbContact.Contact";
        public const string strsrcCONTACTWEBDETAILS = "PHbContact.WebDetails";
        public const string strsrcPHUBCONTACT = "PHbContact.PHubContact";
        // Form Names
        public const string strfrmOPPORTUNITY_INFLUENCER = "Opportunity Influencer";
        public const string strfrmACTION_PLAN_CONTACT_STEP = "Action Plan Contact Step";
        public const string strfrmCOMPANY = "Company";
        public const string strfrmCONTACT_PROF_NBHD = "HB Contact Profile NBHD";
        public const string strfrmCOMPETITIVE_INFORMATION = "Competitive Information";
        public const string strfrmCONTACT = "Contact";
        public const string strfrmWEB_DETAILS = "Contact Web Details";
        public const string strfrmLEAD = "Lead";
        public const string strfrmMARKETING_PROJECT = "Marketing Project";
        public const string strfrmOPPORTUNITY = "Opportunity";
        public const string strfrmORDER = "Order";
        public const string strfrmGENERAL_CONTACT_ACTIVITY = "CA - General Activity";
        public const string strfrmRN_APPOINTMENT = "General Activity";
        // Object Types
        public const string stroCOMPANY = "Company";
        public const string stroCONTACT = "Contact";
        public const string stroLEAD = "Lead";
        public const string stroMARKETING_PROJECT = "Marketing Project";
        public const string stroOPPORTUNITY = "Opportunity";
        // Segment Names
        public const string strsegTEAM = "Team";
        public const string strsegQUOTES = "Quotes";
        public const string strsegACTIVITIES = "Activities";
        public const string strsegSERVICE_REQUESTS = "Service Requests";
        // String Enum
        public const string strenumMYSELF = "Myself";
        public const string strenumEMPLOYEE = "Employee";
        public const string strenumTEAM = "Team";
        // Miscelaneous string constants
        public const string strCONTACT = "Contact";
        // Form Names
        public const string strfrmHB_REALTOR = "HB Realtor";
        public const string strfrmHB_REALTOR_SEARCH = "HB Realtor Search";
        public const string strfrmHB_LOAN_OFFICER = "HB Loan Officer";
        public const string strmHB_ESCROW_OFFICER = "HB Escrow Officer";
        public const string strfrmHB_QUICK_CONTACT = "HB Quick Contact";
        public const string strfrmHB_TITLE_OFFICER = "HB Title Officer";
        public const string strfADDITIONAL_COMMISSION = "Additional_Commission";
        public const string strfADDITIONAL_COMMISSION_PERCENT = "Additional_Commission_Percent";
        public const string strfAGE = "Age";
        public const string strfAGE_RANGE_OF_BUYERS = "Age_Range_Of_Buyers";
        public const string strfAGE_RANGE_OF_CHILDREN = "Age_Range_Of_Children";
        public const string strfANNUAL_REVENUE = "Annual_Revenue";
        public const string strfASSIGNED_TO_RESELLER_ID = "Assigned_To_Reseller_Id";
        public const string strfASSISTANT_PHONE = "Assistant_Phone";
        public const string strfASSISTANTS_EXTENSION = "Assistants_Extension";
        public const string strfASSISTANTS_NAME = "Assistants_Name";
        public const string strfBUDGET_APPROVED = "Budget_Approved";
        public const string strfBUDGET_DOLLARS = "Budget_Dollars";
        public const string strfCELL = "cell";
        public const string strfCOMBINED_INCOME_RANGE = "Combined_Income_Range";
        public const string strfCOMMENTS = "Comments";
        public const string strfCOMMUTE = "Commute";
        public const string strfCONT_MATCH_CODE = "Cont_Match_Code";
        public const string strfCREATED_BY_EMPLOYEE_ID = "Created_By_Employee_Id";
        public const string strfCURRENT_MONTHLY_PAYMENT = "Current_Monthly_Payment";
        public const string strfCURRENT_SQUARE_FOOTAGE = "Current_Square_Footage";
        public const string strfDATE_ENTERED = "Date_Entered";
        public const string strfDECISION_DATE = "Decision_Date";
        public const string strfDEPARTMENT = "Department";
        public const string strfDESIRED_MONTHY_PAYMENT = "Desired_Monthly_Payment";
        public const string strfDESIRED_MOVE_IN_DATE = "Desired_Move_In_Date";
        public const string strfDESIRED_PRICE_RANGE = "Desired_Price_Range";
        public const string strfDESIRED_SQUARE_FOOTAGE = "Desired_Square_Footage";
        public const string strfDIST_TERRITORY = "Dist_Territory";
        public const string strfDIVISION = "Division";
        public const string strfEDUCATION = "Education";
        public const string strfEXTENSION = "Extension";
        public const string strfEXTERNAL_LAST_UPDATE = "External_Last_Update";
        public const string strfEXTERNAL_SOURCE_ID = "External_Source_Id";
        public const string strfEXTERNAL_SOURCE_NAME = "External_Source_Name";
        public const string strfFAMILIAR_NAME = "Familiar_Name";
        public const string strfFIRST_CONTACT_DATE = "First_Contact_Date";
        public const string strfFIRST_VISIT_DATE = "First_Visit_Date";
        public const string strfFOR_SALE = "For_Sale";
        public const string strfGENDER = "Gender";
        public const string strfHOME_TYPE = "Home_Type";
        public const string strfHOMES_OWNED = "Homes_Owned";
        public const string strfHOUSEHOLD_SIZE = "Household_Size";
        public const string strfIMPORT_LOG_ID = "Import_Log_Id";
        public const string strfIMPORT_SOURCE_PRIORITY = "Import_Source_Priority";
        public const string strfINDUSTRY_TYPE = "Industry_Type";
        public const string strfINTEREST_LEVEL = "Interest_Level";
        public const string strfLEAD_NAME_SOUNDEX = "Lead_Name_Soundex";
        public const string strfLEAD_OWNERSHIP = "Lead_Ownership";
        public const string strfMARKETING_PROJECT_NAME = "Marketing_Project_Name";
        public const string strfMATCH_CODE = "Match_Code";
        public const string strfMIDDLE_INITIAL = "Middle_Initial";
        public const string strfMINIMUM_BATHROOMS = "Minimum_Bathrooms";
        public const string strfMINIMUM_BEDROOMS = "Minimum_Bedrooms";
        public const string strfMINIMUM_GARAGE = "Minimum_Garage";
        public const string strfNEXT_FOLLOW_UP_DATE = "Next_Follow_UP_Date";
        public const string strfNEW_REPEAT = "New_Repeat";
        public const string strfNP1_FIRST_VISIT_DATE = "NP1_First_Visit_Date";
        public const string strfNP1_NEIGHBORHOOD_ID = "NP1_Neighborhood_Id";
        public const string strfNP1_PROSPECT_RATING = "NP1_Prospect_Rating";
        public const string strfNUMBER_LIVING_AREAS = "Number_Living_Areas";
        public const string strfNUMBER_OF_CHILDREN = "Number_Of_Children";
        public const string strfOTHER_BUILDERS = "Other_Builders";
        public const string strfOTHER_NEIGHBORHOODS = "Other_Neighborhoods";
        public const string strfOWNERSHIP = "Ownership";
        public const string strfPARTNER_DETAILS_ID = "Partner_Details_Id";
        public const string strfPIN = "PIN";
        public const string strfPOSSIBLE_DUPLICATE = "Possible_Duplicate";
        public const string strfPREFERRED_AREA = "Preferred_Area";
        public const string strfPREFERRED_CONTACT = "Preferred_Contact";
        public const string strfPRICE = "Price";
        public const string strfQUALITY = "Quality";
        public const string strfREALTOR_AGENT_ID = "Realtor_Agent_Id";
        public const string strfREALTOR_COMPANY_ID = "Realtor_Company_Id";
        public const string strfREASONS_FOR_MOVING = "Reasons_For_Moving";
        public const string strfRECEIVE_LEADS = "Receive_Leads";
        public const string strfRESALE = "Resale";
        public const string strfSAME_AS_BUYER_ADDRESS = "Same_as_Buyer_Address";
        public const string strfSINGLE_OR_DUAL_INCOME = "Single_Or_Dual_Income";
        public const string strfSPOUSES_NAME = "Spouses_Name";
        public const string strfSSN = "SSN";
        public const string strfSTATE_ = "State_";
        public const string strfSUFFIX = "Suffix";
        public const string strfTIME_SEARCHING = "Time_Searching";
        public const string strfTITLE = "Title";
        public const string strfTRANSFERRING_TO_AREA = "Transferring_To_Area";
        public const string strfWEB_EDITED = "Web_Edited";
        public const string strfWEB_MEETING_DATE = "Web_Meeting_Date";
        public const string strfWEB_MEETING_TIME = "Web_Meeting_Time";
        public const string strfWEB_PROFILED = "Web_Profiled";
        public const string strfWEB_REGISTERED = "Web_Registered";
        public const string strfWWW = "WWW";

    }

}
