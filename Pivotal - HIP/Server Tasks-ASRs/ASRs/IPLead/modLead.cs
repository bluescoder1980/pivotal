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
    internal class modLead
    {

        /// <summary>
        /// This module contains the constants used throughout the Lead project
        /// </summary>
        /// <history>
        /// Revision# Date        Author  Description
        /// 3.8.0.0   5/10/2006   PPhilip Converted to .Net C# code.
        /// choice strings
        /// Choice strings
        /// </history>
        public const string strsQUOTE = "Quote";
        public const string strsIN_PROGRESS = "In Progress";
        public const string strsCONTRACT = "Contract";
        public const string strsWON = "Won";
        public const string strsPROSPECT = "Prospect";
        public const string strsBUYER = "Buyer";
        public const string strsON_HOLD = "On Hold";
        public const string strsCUSTOMER = "Customer";
        // Segments
        public const string strsNEIGHBORHOOD_PROFILE = "Neighborhood Profile";
        // Lead
        public const string strtLEAD_ = "Lead_";
        public const string strfLEAD__ID = "Lead__Id";
        public const string strfACCOUNT_MANAGER_ID = "Account_Manager_Id";
        public const string strfACCOUNT_MANAGER_OVERRIDE = "Account_Manager_Override";
        public const string strfACCOUNT_MGR_CHANGED = "Account_Mgr_Changed";
        public const string strfADDITIONAL_COMMISSION = "Additional_Commission";
        public const string strfADDITIONAL_COMMISSION_PERCENT = "Additional_Commission_Percent";
        public const string strfADDRESS_1 = "Address_1";
        public const string strfADDRESS_2 = "Address_2";
        public const string strfADDRESS_3 = "Address_3";
        public const string strfAGE = "Age";
        public const string strfAGE_RANGE_OF_BUYERS = "Age_Range_Of_Buyers";
        public const string strfAGE_RANGE_OF_CHILDREN = "Age_Range_Of_Children";
        public const string strfANNUAL_REVENUE = "Annual_Revenue";
        public const string strfAREA_CODE = "Area_Code";
        public const string strfASSIGNED_TO_PARTNER_CONTACT = "Assigned_To_Partner_Contact";
        public const string strfASSIGNED_TO_RESELLER_ID = "Assigned_To_Reseller_Id";
        public const string strfASSISTANT_PHONE = "Assistant_Phone";
        public const string strfASSISTANTS_EXTENSION = "Assistants_Extension";
        public const string strfASSISTANTS_NAME = "Assistants_Name";
        public const string strfBUDGET_APPROVED = "Budget_Approved";
        public const string strfBUDGET_DOLLARS = "Budget_Dollars";
        public const string strfCELL = "Cell";
        public const string strfCITY = "City";
        public const string strfCO_BUYER_ADDRESS_1 = "Co_Buyer_Address_1";
        public const string strfCO_BUYER_ADDRESS_2 = "Co_Buyer_Address_2";
        public const string strfCO_BUYER_ADDRESS_3 = "Co_Buyer_Address_3";
        public const string strfCO_BUYER_CELL = "Co_Buyer_Cell";
        public const string strfCO_BUYER_CITY = "Co_Buyer_City";
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
        public const string strfDESIRED_MONTHY_PAYMENT = "Desired_Monthly_Payment";
        public const string strfDESIRED_MOVE_IN_DATE = "Desired_Move_In_Date";
        public const string strfDESIRED_PRICE_RANGE = "Desired_Price_Range";
        public const string strfDESIRED_SQUARE_FOOTAGE = "Desired_Square_Footage";
        public const string strfDIST_TERRITORY = "Dist_Territory";
        public const string strfDIVISION = "Division";
        public const string strfDIVISION_ID = "Division_Id";
        public const string strfDNC_STATUS = "DNC_Status";
        public const string strfEDUCATION = "Education";
        public const string strfEMAIL = "Email";
        public const string strfEXTENSION = "Extension";
        public const string strfEXTERNAL_LAST_UPDATE = "External_Last_Update";
        public const string strfEXTERNAL_SOURCE_ID = "External_Source_Id";
        public const string strfEXTERNAL_SOURCE_NAME = "External_Source_Name";
        public const string strfFAMILIAR_NAME = "Familiar_Name";
        public const string strfFAX = "Fax";
        public const string strfFIRST_NAME = "First_Name";
        public const string strfFOR_SALE = "For_Sale";
        public const string strfGENDER = "Gender";
        public const string strfHOME_TYPE = "Home_Type";
        public const string strfHOMES_OWNED = "Homes_Owned";
        public const string strfHOUSEHOLD_SIZE = "Household_Size";
        public const string strfIMPORT_LOG_ID = "Import_Log_Id";
        public const string strfIMPORT_SOURCE_PRIORITY = "Import_Source_Priority";
        public const string strfINDUSTRY_TYPE = "Industry_Type";
        public const string strfINTEREST_LEVEL = "Interest_Level";
        public const string strfJOB_TITLE = "Job_Title";
        public const string strfLAST_NAME = "Last_Name";
        public const string strfLEAD_NAME_SOUNDEX = "Lead_Name_Soundex";
        public const string strfLEAD_OWNERSHIP = "Lead_Ownership";
        public const string strfLEAD_SOURCE_ID = "Lead_Source_Id";
        public const string strfLEAD_SOURCE_TYPE = "Lead_Source_Type";
        public const string strfMARITAL_STATUS = "Marital_Status";
        public const string strfMARKETING_PROJECT_NAME = "Marketing_Project_Name";
        public const string strfMATCH_CODE = "Match_Code";
        public const string strfMIDDLE_INITIAL = "Middle_Initial";
        public const string strfMINIMUM_BATHROOMS = "Minimum_Bathrooms";
        public const string strfMINIMUM_BEDROOMS = "Minimum_Bedrooms";
        public const string strfMINIMUM_GARAGE = "Minimum_Garage";
        public const string strfNEXT_FOLLOW_UP_DATE = "Next_Follow_Up_Date";
        public const string strfNEW_REPEAT = "New_Repeat";
        public const string strfNP1_FIRST_VISIT_DATE = "NP1_First_Visit_Date";
        public const string strfNP1_NEIGHBORHOOD_ID = "NP1_Neighborhood_Id";
        public const string strfNP1_PROSPECT_RATING = "NP1_Prospect_Rating";
        public const string strfNUMBER_LIVING_AREAS = "Number_Living_Areas";
        public const string strfNUMBER_OF_CHILDREN = "Number_Of_Children";
        public const string strfNEIGHBORHOOD_PROFILE_ID = "Neighborhood_Profile_Id";
        public const string strfOTHER_BUILDERS = "Other_Builders";
        public const string strfOTHER_NEIGHBORHOODS = "Other_Neighborhoods";
        public const string strfOWNERSHIP = "Ownership";
        public const string strfPAGER = "Pager";
        public const string strfPARTNER_DETAILS_ID = "Partner_Details_Id";
        public const string strfPHONE = "Phone";
        public const string strfPIN = "PIN";
        public const string strfPOSSIBLE_DUPLICATE = "Possible_Duplicate";
        public const string strfPREFERRED_AREA = "Preferred_Area";
        public const string strfPREFERRED_CONTACT = "Preferred_Contact";
        public const string strfPRICE = "Price";
        public const string strfPRIORITY_CODE_ID = "Priority_Code_Id";
        public const string strfPRODUCT_INTEREST_ID = "Product_Interest_Id";
        public const string strfPRODUCT_INTEREST_TYPE = "Product_Interest_Type";
        public const string strfQUALITY = "Quality";
        public const string strfREALTOR_AGENT_ID = "Realtor_Agent_Id";
        public const string strfREALTOR_COMPANY_ID = "Realtor_Company_Id";
        public const string strfREASONS_FOR_MOVING = "Reasons_For_Moving";
        public const string strfREFERRED_BY_CONTACT_ID = "Referred_By_Contact_Id";
        public const string strfREFERRED_BY_EMPLOYEE_ID = "Referred_By_Employee_Id";
        public const string strfRESALE = "Resale";
        public const string strfRN_CREATE_DATE = "Rn_Create_Date";
        public const string strfRN_CREATE_USER = "Rn_Create_User";
        public const string strfRN_DESCRIPTOR = "Rn_Descriptor";
        public const string strfRN_EDIT_DATE = "Rn_Edit_Date";
        public const string strfRN_EDIT_USER = "Rn_Edit_User";
        public const string strfSAME_AS_BUYER_ADDRESS = "Same_as_Buyer_Address";
        public const string strfSINGLE_OR_DUAL_INCOME = "Single_Or_Dual_Income";
        public const string strfSPOUSES_NAME = "Spouses_Name";
        public const string strfSSN = "SSN";
        public const string strfSTATE_ = "State_";
        public const string strfSTOCK_SYMBOL = "Stock_Symbol";
        public const string strfSUFFIX = "Suffix";
        public const string strfTERRITORY_ID = "Territory_Id";
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
        public const string strfWALK_IN_DATE = "Walk_In_Date";
        public const string strfWWW = "WWW";
        public const string strfZIP = "Zip";
        public const string strfDESIRED_MONTHLY_PAYMENT = "Desired_Monthly_Payment";
        public const string strfCO_BUYER_COUNTY_ID = "Co_Buyer_County_Id";
        public const string strfCO_BUYER_AREA_CODE = "Co_Buyer_Area";
        // Queries
        public const string strqLEADS_WITH_MATCH_CODE = "HB: Leads with Match Code?";
        public const string strqACTIVITIES_FOR_LEAD = "HB: Activities for Lead?";
        public const string strqNEIGHBORHOOD_PROFILES_OF_LEADS = "HB: Neighborhood Profiles of Lead?";
        public const string strqALERTS_WITH_LEAD = "PA: Alerts with Lead Id ?";
        // Archieve_Lead
        public const string strtARCHIVE_LEAD = "Arch_Lead";
        public const string strfARCHIEVE_LEAD_ID = "Arch_Lead_Id";
        public const string strfARCH_LEAD_SOUNDEX = "Arch_Lead_Name_Soundex";
        // contact
        public const string strfHAS_SAME_ADDRESS_ID = "Has_Same_Address_Id";
        public const string strfWORKS_OUT_OF_OFFICE = "Works_Out_Of_Office";
        public const string strfFULL_NAME = "Full_Name";
        public const string strmCONTACT_DUPLICATE = "Contact Duplicate";
        // Contact Team Member
        public const string strtCONTACT_TEAM_MEMBER = "Contact_Team_Member";
        public const string strfCONTACT_TEAM_MEMBER_ID = "Member_Team_Member_Id";
        // Contact_CoBuyer
        public const string strtCONTACT_COBUYER = "Contact_CoBuyer";
        public const string strfCO_BUYER_CONTACT_ID = "Co_Buyer_Contact_Id";
        public const string strfCONTACT_COBUYER_ID = "Contact_CoBuyer_Id";
        public const string strfCONTACT_ID = "Contact_Id";
        // Neighborhood
        public const string strtNEIGHBORHOOD = "Neighborhood";
        public const string strfNAME = "Name";
        // Contact_Profile_Neighborhood
        public const string strtCONTACT_PROFILE_NEIGHBORHOOD = "Contact_Profile_Neighborhood";
        public const string strfCONTACT_PROFILE_NBHD_ID = "Contact_Profile_NBHD_Id";
        public const string strfFIRST_CONTACT_DATE = "First_Contact_Date";
        public const string strfFIRST_VISIT_DATE = "First_Visit_Date";
        public const string strfNEIGHBORHOOD_ID = "Neighborhood_Id";
        public const string strfNEXT_FOLLOW_UP = "Next_Follow_Up";
        public const string strfPROSPECT_RATING = "Prospect_Rating";
        public const string strfTRAFFIC_COMMENTS = "Traffic_Comments";
        public const string strfTRAFFIC_SOURCE = "Traffic_Source";
        public const string strfQUOTE_DATE = "Quote_Date";
        // TRAFFIC SOURCE
        public const string strtTRAFFIC_SOURCE = "Traffic_Source";
        public const string strfMARKETING_PROJECT_ID = "Marketing_Project_Id";
        // Employee
        public const string strtEMPLOYEE = "Employee";
        public const string strfEMPLOYEE_ID = "Employee_Id";
        public const string strfRN_EMPLOYEE_USER_ID = "Rn_Employee_User_Id";
        public const string strqEMPLOYEE_ID_OF_CURRENT_USER = "Employee Id of Current User";
        public const string strqEMPLOYEE_WITH_USER = "Sys: Employee with User ?";
        public const string strfCOMPANY = "Company_Id";
        public const string strfCOMPANY_MATCH_CODE = "Comp_Match_Code";
        public const string strfCONTACT_MATCH_CODE = "Cont_Match_Code";
        public const string strfCOMPANY_ID = "Company_Id";
        // db Field of Company, Company_Group, Contact, Opportunity
        public const string strfPARENT_COMPANY_ID = "Parent_Company_Id";
        // db Field of Company
        public const string strfCURRENCY_TYPE = "Currency_Type";
        // db Field of Company
        public const string strfCREDIT_LIMIT = "Credit_Limit";
        // db Field of Company
        public const string strfREFERRED_BY_ID = "Referred_By_Id";
        // db Field of Company, Opportunity
        public const string strfACCOUNT_CODE = "Account_Code";
        // db Field of Company
        public const string strfBUSINESS_UNIT = "Business_Unit";
        // db Field of Company
        public const string strfUSE_AS_REFERENCE = "Use_As_Reference";
        // db Field of Company
        public const string strfDATE_BECAME_CUSTOMER = "Date_Became_Customer";
        // db Field of Company
        public const string strfFISCAL_YEAR_END = "Fiscal_Year_End";
        // db Field of Company
        public const string strfSIC_CODE = "SIC_Code";
        // db Field of Company
        public const string strfDUNS = "DUNS";
        // db Field of Company
        public const string strfCREDIT_RATING = "Credit_Rating";
        // db Field of Company
        public const string strfSUPPLIER_CREDIT_LIMIT = "Supplier_Credit_Limit";
        // db Field of Company
        public const string strfSUPPLIER_ACCOUNT_CODE = "Supplier_Account_Code";
        // db Field of Company
        public const string strfSUPPLIER_ACCOUNT_MANAGER_ID = "Supplier_Account_Manager_Id";
        // db Field of Company
        public const string strfRESELLER_KEY_CONTACT_ID = "Reseller_Key_Contact_Id";
        // db Field of Company
        public const string strfNUMBER_OF_EMPLOYEES = "Number_Of_Employees";
        // db Field of Company
        public const string strfDONT_FAX = "Dont_Fax";
        // db Field of Company, Contact
        public const string strfACTIVITY_COMPLETE = "Activity_Complete";
        // db Field of Company, Contact, Opportunity, Rn_Appointments
        public const string strfINACTIVE = "Inactive";
        // db Field of Company
        public const string strfDOES_BUSINESS_AS = "Does_Business_As";
        // db Field of Company
        public const string strfPRODUCT_TYPE_INTEREST = "Product_Type_Interest";
        // db Field of Company, Contact, Opportunity
        public const string strfCREDIT_STATUS = "Credit_Status";
        // db Field of Company
        public const string strfDELTA_CREDIT_STATUS = "Delta_Credit_Status";
        // db Field of Company
        public const string strfDELTA_PAYMENT_TERMS = "Delta_Payment_Terms";
        // db Field of Company
        public const string strfPAYMENT_TERMS = "Payment_Terms";
        // db Field of Company
        public const string strfACCOUNT_MANAGER_USER_ID = "Account_Manager_User_Id";
        // db Field of Company, Contact, Opportunity
        public const string strfACCOUNT_MGR_REPORTS_TO_USER_ID = "Account_Mgr_Reports_To_User_Id";
        // db Field of Company, Contact, Opportunity
        public const string strfPARTNER_COMPANY_ID = "Partner_Company_Id";
        // db Field of Company, Contact
        public const string strfPARTNER_CONTACT_ID = "Partner_Contact_Id";
        // db Field of Company, Contact, Opportunity
        public const string strfGROWTH_POTENTIAL_ID = "Growth_Potential_Id";
        // db Field of Company
        public const string strfPRESTIGE_INDICATOR_ID = "Prestige_Indicator_Id";
        // db Field of Company
        public const string strfLOYALTY_INDICATOR_ID = "Loyalty_Indicator_Id";
        // db Field of Company
        public const string strfCONTACT_WEB_DETAILS_ID = "Contact_Web_Details_Id";
        // db Field of Contact, Rn_Appointments
        public const string strfLEAD_ID = "Lead_Id";
        // db Field of Lead_, Lead_Group
        public const string strfDELTA_CURRENCY_ID = "Delta_Currency_Id";
        // db Field of Opportunity
        public const string strfOPPORTUNITY_NAME = "Opportunity_Name";
        // db Field of Opportunity
        public const string strfLAST_ASSIGNED_LEAD_ID = "Last_Assigned_Lead_Id";
        public const string strfDELTA_RN_EMPLOYEE_ID = "Delta_Rn_Employee_Id";
        // db Field of Rn_Appointments
        public const string strfDELTA_COMPANY = "Delta_Company";
        // db Field of Rn_Appointments
        public const string strfDELTA_CONTACT = "Delta_Contact";
        // db Field of Rn_Appointments
        public const string strfDELTA_OPPORTUNITY = "Delta_Opportunity";
        // db Field of Rn_Appointments
        // Opportunity'
        public const string strtOPPORTUNITY = "Opportunity";
        public const string strfACTUAL_DECISION_DATE = "Actual_Decision_Date";
        public const string strfACTUAL_REVENUE_DATE = "Actual_Revenue_Date";
        public const string strfBUILT_OPTIONS = "Built_Options";
        public const string strfCALC_PROBABILITY_TO_CLOSE = "Calc_Probability_To_Close";
        public const string strfCANCEL_DATE = "Cancel_Date";
        public const string strfCANCEL_NOTES = "Cancel_Notes";
        public const string strfCANCEL_REASON_CODE = "Cancel_Reason_Code";
        public const string strfCANCEL_REQUEST_DATE = "Cancel_Request_Date";
        public const string strfCASH_OPTIONS = "Cash_Options";
        public const string strfCHANGE_ORDER_ID = "Change_Order_Id";
        public const string strfCONCESSIONS = "Concessions";
        public const string strfCONFIGURATION_COMPLETE = "Configuration_Complete";
        public const string strfCONSTRUCTION_STAGE_ID = "Construction_Stage_Id";
        public const string strfCONSTRUCTION_STAGE_ORDINAL = "Construction_Stage_Ordinal";
        public const string strfCONTINGENCY = "Contingency";
        public const string strfCONTINGENCY_NOTES = "Contingency_Notes";
        public const string strfDEBTS = "Debts";
        public const string strfDECORATOR_ESTIMATE = "Decorator_Estimate";
        public const string strfDELTA_ACTUAL_REVENUE_DATE = "Delta_Actual_Revenue_Date";
        public const string strfDELTA_CANCEL_DATE = "Delta_Cancel_Date";
        public const string strfDELTA_CONTACT_ID = "Delta_Contact_Id";
        public const string strfDELTA_ELEVATION_BUILT = "Delta_Elevation_Built";
        public const string strfDELTA_PLAN_BUILT = "Delta_Plan_Built";
        public const string strfDELTA_PRODUCT_TYPE_INTEREST = "Delta_Product_Type_Interest";
        public const string strfDEPOSIT_AMOUNT_TAKEN = "Deposit_Amount_Taken";
        public const string strfDESCRIPTION = "Description";
        public const string strfDISPLAY_CURRENCY = "Display_Currency";
        public const string strfECOE_DATE = "ECOE_Date";
        public const string strfELEVATION_BUILT = "Elevation_Built";
        public const string strfELEVATION_ID = "Elevation_Id";
        public const string strfESTIMATED_TOTAL = "Estimated_Total";
        public const string strfEXPECTED_DECISION_DATE = "Expected_Decision_Date";
        public const string strfEXPECTED_DECISION_DATE_TEXT = "Expected_Decision_Date_Text";
        public const string strfEXPECTED_REVENUE_DATE = "Expected_Revenue_Date";
        public const string strfFILTER_CATEGORY = "Filter_Category";
        public const string strfFILTER_CATEGORY_ID = "Filter_Category_Id";
        public const string strfFILTER_CONSTR_STAGE_ID = "Filter_Constr_Stage_Id";
        public const string strfFILTER_CONSTRUCTION_STAGE = "Filter_Construction_Stage";
        public const string strfFINANCED_OPTIONS = "Financed_Options";
        public const string strfINCLUDE_IN_PROJECTION = "Include_In_Projection";
        public const string strfINCOME = "Income";
        public const string strfLEAD_DATE = "Lead_Date";
        public const string strfLENDER_AGENT = "Lender_Agent";
        public const string strfLENDER_ID = "Lender_Id";
        public const string strfLENDER_NOTES = "Lender_Notes";
        public const string strfLOAN_ID = "Loan_Id";
        public const string strfLOAN_OFFICER_ID = "Loan_Officer_Id";
        public const string strfLOT_ID = "Lot_Id";
        public const string strfLOT_PREMIUM = "Lot_Premium";
        public const string strfNBHD_PHASE_ID = "NBHD_Phase_Id";
        public const string strfOPPORTUNITY_ID = "Opportunity_Id";
        public const string strfOPPORTUNITY_MOVEMENT = "Opportunity_Movement";
        public const string strfOVERRIDE_CALC_PROBABILITY = "Override_Calc_Probability";
        public const string strfPIPELINE_EXP_DURATION_DAYS = "Pipeline_Exp_Duration_Days";
        public const string strfPIPELINE_LAST_UPDATED_DATE = "Pipeline_Last_Updated_Date";
        public const string strfPIPELINE_STAGE = "Pipeline_Stage";
        public const string strfPLAN_BUILT = "Plan_Built";
        public const string strfPLAN_NAME_ID = "Plan_Name_Id";
        public const string strfPLAN_TEXT = "Plan_Text";
        public const string strfPRICE_CHANGE_DATE = "Price_Change_Date";
        public const string strfPRICE_UPDATE = "Price_Update";
        public const string strfPROBABILITY_TO_CLOSE = "Probability_To_Close";
        public const string strfQUOTA_PERIOD = "Quota_Period";
        public const string strfQUOTE_CREATE_DATE = "Quote_Create_Date";
        public const string strfQUOTE_TOTAL = "Quote_Total";
        public const string strfQUOTED_LOT_PREMIUM = "Quoted_Lot_Premium";
        public const string strfQUOTED_OPTIONS_TOTAL = "Quoted_Options_Total";
        public const string strfQUOTED_PRICE = "Quoted_Price";
        public const string strfREALTOR_ID = "Realtor_Id";
        public const string strfREQUIRED_DEPOSIT_AMOUNT = "Required_Deposit_Amount";
        public const string strfRESELLER_ID = "Reseller_Id";
        public const string strfRESERVATION_AMOUNT = "Reservation_Amount";
        public const string strfRESERVATION_DATE = "Reservation_Date";
        public const string strfRESERVATION_EXPIRATION = "Reservation_Expiration";
        public const string strfRESULT_DESCRIPTION_1 = "Result_Description_1";
        public const string strfRESULT_DESCRIPTION_2 = "Result_Description_2";
        public const string strfRESULT_REASON_1 = "Result_Reason_1";
        public const string strfRESULT_REASON_2 = "Result_Reason_2";
        public const string strfREVENUE_DATE_USE = "Revenue_Date_Use";
        public const string strfSERVICE_DATE = "Service_Date";
        public const string strfSHADOW_PROBABILITY_TO_CLOSE = "Shadow_Probability_To_Close";
        public const string strfSTATUS = "Status";
        public const string strfSTATUS_EDITED_DATE = "Status_Edited_Date";
        public const string strfSTRUCTURAL_ESTIMATE = "Structural_Estimate";
        public const string strfTICKLE_COUNTER = "Tickle_Counter";
        public const string strfWARRANTY_DATE = "Warranty_Date";
        public const string strfWEIGHTED_TOTAL = "Weighted_Total";
        public const string strfWIRELESS_PROBABILITY_TO_CLOSE = "Wireless_Probability_To_Close";
        // Opportunity__Product
        public const string strfADDED_BY_CHANGE_ORDER_ID = "Added_By_Change_Order_Id";
        public const string strfBUILT_OPTION = "Built_Option";
        public const string strfCATEGORY_ID = "Category_Id";
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
        public const string strfOPPORTUNITY_PRODUCT_PREF_ID = "Opportunity_Product_Pref_Id";
        public const string strfOPTION_ADDED_BY = "Option_Added_By";
        public const string strfOPTIONNOTES = "OptionNotes";
        public const string strfPREFERENCE = "Preference";
        public const string strfPREFERENCES_LIST = "Preferences_List";
        public const string strfPRODUCT_AVAILABLE = "Product_Available";
        public const string strfPRODUCT_ID = "Product_Id";
        public const string strfPRODUCT_NAME = "Product_Name";
        public const string strfQUANTITY = "Quantity";
        public const string strfREMOVED_BY_CHANGE_ORDER_ID = "Removed_by_Change_Order_Id";
        public const string strfSELECTED = "Selected";
        public const string strfSYSTEM_DEFAULT_CURRENCY = "System_Default_Currency";
        // db Field of Product.
        public const string strfALERT_ID = "Alert_Id";
        // db Field of Alert
        public const string strfARCH_LEAD_ID = "Arch_Lead_Id";
        // db Field of Arch_Lead
        public const string strfARCH_PRODUCT_INTEREST_ID = "Arch_Product_Interest_Id";
        // db Field of Arch_Lead
        public const string strfSTATUS_ = "Status_";
        // db Field of Arch_Lead
        // In Lead_Distribution
        public const string strLEAD_DISTRIBUTION_ID = "Lead_Distribution_Id";
        public const string strfDATE_ = "Date_";
        public const string strfREASON_FOR_REJECTION = "Reason_for_Rejection";
        public const string strfLEAD_DISTRIBUTION_ID = "Lead_Distribution_Id";
        public const string strfSYSTEM_NOTIFICATION = "System_Notification";
        public const string strfTERRITORY_NAME = "Territory_Name";
        // In Contact_Web_Details
        public const string strfLOGIN_NAME = "Login_Name";
        // In Employee table
        public const string strfWORK_EMAIL = "Work_Email";
        // In Contact table
        public const string strtCONTACT = "Contact";
        public const string strtPRODUCT = "Product";
        // Rn Appointments
        public const int lngACTIVITY_TYPE_VISITLOG = 7;
        public const string strRN_APPOINTMENTS_TABLE = "Rn_Appointments";
        public const string strfRN_APPOINTMENTS_ID = "Rn_Appointments_Id";
        public const string strfACCESS_TYPE = "Access_Type";
        public const string strfACTIVITY_COMPLETED_DATE = "Activity_Completed_Date";
        public const string strfACTIVITY_TYPE = "Activity_Type";
        public const string strfAPPT_DATE = "Appt_Date";
        public const string strfAPPT_DESCRIPTION = "Appt_Description";
        public const string strfAPPT_PRIORITY = "Appt_Priority";
        public const string strfASSIGNED_BY = "Assigned_By";
        public const string strfCONTACT = "Contact";
        public const string strfMARKETING_PROJECT = "Marketing_Project";
        public const string strfNOTES = "Notes";
        public const string strfVISIT_NUMBER = "Visit_Number";
        public const string strfRN_EMPLOYEE_ID = "Rn_Employee_Id";
        public const string strfSTART_TIME = "Start_Time";
        // Other Constants
        // Form name constants
        public const string strCONTACT_PROF_NBHD = "HB Contact Profile NBHD";
        public const string strCOMPANY_FORM = "Company";
        public const string strCONTACT_FORM = "HB Quick Contact";
        public const string strPHUB_NEW_LEAD_FORM = "PHub New Lead";
        public const string strLEAD_FORM = "HB Lead";
        public const string strNOTE_FORM = "Note";
        public const string strALERT_FORM = "PAHB Alert";
        public const string strOPP_FORM = "PAHB Opportunity";
        public const string strGENERAL_ACTIVITY_FORM = "General Activity";
        public const string strARCH_LEAD_FORM = "Arch_Lead";
        public const string strPHUB_LEAD_DISTRIBUTION_FORM = "PHub Lead Distribution";
        public const string strCURRENCY = "Currency";
        // Tables
        public const string strCOMPANY_TABLE = "Company";
        public const string strCONTACT_TABLE = "Contact";
        public const string strtLEAD_TABLE = "Lead_";
        public const string strtALERT = "Alert";
        public const string strPRODUCT_INTEREST_TABLE = "Product_Interest";
        public const string strARCH_LEAD_TABLE = "Arch_Lead";
        public const string strARCH_PRODUCT_INTEREST_TABLE = "Arch_Product_Interest";
        public const string strOPP_TABLE = "Opportunity";
        public const string strtTERRITORY = "Territory";
        public const string strLEAD_DISTRIBUTION_TABLE = "Lead_Distribution";
        public const string strPRODUCT_TABLE = "Product";
        public const string strEMPLOYEE_TABLE = "Employee";
        public const string strCONTACT_WEB_DETAILS_TABLE = "Contact_Web_Details";
        public const string strtNBHDP_TEAM = "NBHDP_Team";
        public const string strtCONTACT_PROFILE_NBHD = "Contact_Profile_Neighborhood";
        public const string strLEAD_LDGROUP = "PAHB Lead";
        public const string strTRANSIT_PARAMS_APPRULE = "Transit Point Params";
        public const string strCURRENCY_APPRULE = "Currency";
        public const string strSYSTEM_APPRULE = "System";
        public const string strERRORS_APPRULE = "Errors";
        public const string strNCERRORS_APPRULE = "NCErrors";
        public const string strALERT_APPRULE = "PAHB Alert";
        public const string strsTERRITORY_MGMT = "Territory Mgmt";
        public const string strsFUNCTION_LIBRARY = "Function Lib";
        public const string strsSHARE_FUNCTION_LIBRARY = "Share Function Library";
        // Server Script
        //kA 6-16-10 converted to IP ASR
        //public const string strsCONTACT_PROFILE_NBHD = "PAHB Contact Profile Neighborhood";
        //public const string strsINACTIVATE_NBHD_PROFILE = "PAHB Inactivate Contact Profile Neighborhood";
        public const string strsCONTACT_PROFILE_NBHD = "TIC Contact Profile Neighborhood";
        public const string strsINACTIVATE_NBHD_PROFILE = "TIC Inactivate Contact Profile Neighborhood";
        public const string strsCORE_TRANSIT_POINT_PARAM = "PAHB Core Transit Point Param";
        public const string strsCONTACT = "PAHB Contact";
        public const string strPRODUCT_QUOTE_SEGMENT = "PAHB Product Quote";
        public const string strARCH_PRODUCT_INTEREST_SEGMENT = "PAHB Product Interest";
        public const string strPRODUCT_INTEREST_SEGMENT = "PAHB Product Interest";
        public const string strLEAD_DISTRIBUTION_SEGMENT = "PAHB Partner Lead Distribution";
        // Query
        public const string strqARCHLEAD_WITH_IMPORTLOGID = "Sys: Arch Lead with Import Log Id ?";
        public const string strqCONTACTS_WITH_LOGIN_NAME = "Sys: Contacts with Login Name ? ";
        public const string strgFIND_MATCH_CONTACTS = "Sys: Find Matches w / Contacts ? ";
        public const string strgFIND_MATCH_COMPANIES = "Sys: Find Matches w / Companies ? ";
        public const string strqDISTRIBUTE_LEAD_INTERNALLY = "Sys: Distribute Lead Internally ? ";
        public const string strgLEAD_DISTRIBUTION_WITH_LEAD = "Sys: Lead Distribution with lead ? ";
        public const string strqLEAD_POSSIBLE_DUPLICATES = "Sys: Lead Possible Duplicate ?";
        public const string strqAll_LEAD_DISTRIBUTION_REJECTED = "Sys: All Lead Distribution Rejected ?";
        public const string strqCONTACT_WITH_PARTNER_CONTACT = "Sys: Contact With Partner Contact Id ?";
        public const string strgNEW_LEAD_DISTRIBUTION_FOR_LEAD = "Sys: New Lead Distributions for Lead?";
        public const string strgACCEPT_PARTNER_CONTACT_WITH_LEAD = "Sys: Accept Partner Contact With Lead ?";
        public const string strqNBHD_PROFILE_FOR_CONTACT_AND_NEIGHBORHOOD = "HB: NBHD Profile for Contact Id? Neighborhood Id?";
        public const string strqVALID_VISIT_LOG_TO_UPDATE_QUICK_PATH = "HB: Valid Visit Logs to Update on Quick Path";
        public const string strqCHECK_DUPLICATE_CONTACTS = "HB: Check Duplicate Contacts";
        public const string strqNEIGHBORHOOD_PROFILE_FOR_LEAD_NEIGHBORHOOD = "HB: NBHD Profile for Lead Id? Neighborhood Id?";
        public const string strqNBHDP_TEAM_OF_NEIGHBORHOOD_PROFILE = "HB: All NBHDP Team of Neighborhood Profile?";
        public const string strqAPPOINTMENTS_WITH_LEAD = "PA: Appointments with Lead ?";
        public const string strqLEADS_OF_LEAD = "Sys: Leads of Lead ?";
        public const string strqCONTACT_WITH_CONTACTID = "M1 Contact with Contact_Id ?";
        // Marketing Project
        public const string strtMARKETING_PROJECT = "Marketing_Project";
        // LD Strings
        public const string strldMISSING_RQD_FIELDS_IN_CONTACT = "Missing Required Fields in Contact";
        public const string strldMISSING_RQD_FIELDS_IN_LEAD = "Missing Required Fields in Lead";
        public const string strldPHUB_DUPLICATE_NOTIFY = "Phub New Lead Duplicate Notify";
        public const string strldgPHUB_PROMPT_NOTIFY = "Phub New Lead Prompt user Notify";
        public const string strldgPHUB_LEAD_NOTE_TO_USER = "Phub Lead Note To User";
        public const string strdPartner_CHANGED = "Partner_Changed";
        // Addition for PartnerHub by JDai May 26, 2000
        public const string strmSET_SYSTEM = "SetSystem";
        public const string gstrEMPTY_STRING = " ";
        public const string strdBLANK = " ";
        // Addition for M1 Integration by ASikri November 4, 2002
        public const string strfM1_CONTACT_ID = "M1_Contact_Id";
        public const string strfM1_UNSUBSCRIBE = "M1_Unsubscribe";
        public const string strfqCONTACTS_USING_M1_CONTACT_ID = "M1 Contacts with M1_Contact_Id ?";
        public const string strdDUPLICATE_CONTACTS_M1_CONTACT_ID = "Failed to create new contact during lead processing.  Contact with the following MarketFirst Contact Id already exists: ";
        public const string strCREATE_CONTACT = "CreateContact";

        public const int intSTART_BUSINESS_ERROR = 10000 + -2147221504;
        public const int intEND_BUSINESS_ERROR = 13399 + -2147221504;
        public const int intMIN_LEAD_ERROR = 11400 + -2147221504;
        public const int intMAX_LEAD_ERROR = 11499 + -2147221504;
        public const int lngERR_LB_FUTURE_EXPANSION_NO = 13600 + -2147221504;
        public const int lngERR_UB_AD_ERR_NO = 29999 + -2147221504;
        // Methods Error
        public const int intERRNO_UNKNOWN_METHOD_NAME = intMIN_LEAD_ERROR;
        public const int intERRNO_PROCESSLEAD_FAILED = intMIN_LEAD_ERROR + 1;
        public const int intERRNO_REFERTORESELLER_FAILED = intMIN_LEAD_ERROR + 2;
        public const int intERRNO_DUPLICATECHECKING_FAILED = intMIN_LEAD_ERROR + 3;
        public const int intERRNO_CALCULATETOTAL_FAILED = intMIN_LEAD_ERROR + 4;
        public const int intERRNO_ARCHIVELEAD_FAILED = intMIN_LEAD_ERROR + 5;
        public const int intERRNO_MISSING_RQD_FIELDS_IN_CONTACT = intMIN_LEAD_ERROR + 6;
        public const int intERRNO_MISSING_RQD_FIELDS_IN_LEAD = intMIN_LEAD_ERROR + 7;
        // Implements Error
        public const int intERRNO_SETSYSTEM_FAILED = 13412;
        // shared numbers
        public const int intERRNO_LOADFORMDATA_FAILED = 13408;
        // shared numbers
        public const int intERRNO_SAVEFORMDATA_FAILED = 13411;
        // shared numbers
        public const int intERRNO_DELETEFORMDATA_FAILED = 13405;
        // shared numbers
        public const int intERRNO_NEWFORMDATA_FAILED = 13409;
        // shared numbers
        public const int intERRNO_ADDFORMDATA_FAILED = 13404;
        // shared numbers
        public const int intERRNO_NEWSECONDARYDATA_FAILED = 13410;
        // shared numbers
        public const int intERRNO_MISSING_PARAMETER = 13402;
        // shared numbers
        public const int intERRNO_ACCOUNTMANAGER_EMAIL_FAILD = 13412;

        public const string strcINTERNAL = "Internal";

    }

}
