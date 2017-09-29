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
    /// <summary>
    /// This class will be where all the constants values (table names, field names, etc. will be stored)
    /// </summary>
    internal static class IntegrationConstants
    {
        public const string gstrfRN_DESCRIPTOR = "Rn_Descriptor";
        public const string gstrfCOMPANY_COUNTY = "Disconnected_1_2_1";
        public const string strfCONTACT_COUNTY = "Disconnected_1_2_1";
        public const string gstrfCOMPANY_DIVISION = "Disconnected_1_2_2";
        public const string gstrfSTATE = "State_";
        public const string gstrfCOUNTY_ID = "County_Id";
        public const string gstrtCOUNTY = "County";
        public const string gstrqCOUNTY_BY_NAME_STATE = "HB: County where County Name = ?";
        public const string gstrsCORE_PL_FUNCTIONLIB = "Core PL Function Lib";
        public const string gstrfEXTERNAL_SOURCE_SYNC_STATUS = "External_Source_Sync_Status";
        public const string strfCONTACT_ID = "Contact_Id";
        public const string gstrfCONTACT_COMMUNITY = "Disconnected_1_2_1";
        public const string gstrtNEIGHBORHOOD = "Neighborhood";
        public const string gstrqCOMMUNITY_BY_EXTERNAL_SOURCE_ID = "HBInt: Neighborhood for Extern Src Id = ?";
        public const string gstrfNEIGHBORHOOD_ID = "Neighborhood_Id";
        public const string gstrfDIVISION_ID = "Division_Id";
        public const string gstrfMI_DIVISION_ID = "MI_Division_Id";
        public const string gstrfTYPE = "Type";
        public const string gstrfSYS_TRUE = "Sys_True";
        public const string gstrtCONTACT_PROFILE_NEIGHB = "Contact_Profile_Neighborhood";
        public const string gstrqCONTACT_PROFILE_NEIGHB_BY_CONTACT = "HBInt: Contact Profile Neighborhood By Contact ?";
        public const string gstrqCONTACT_PROFILE_NEIGHB_BY_LEAD = "HBInt: Contact Profile Neighborhood By Lead ?";
        public const string gstrBUYER = "Buyer";
        public const string gstrPROSPECT = "Prospect";
        public const string gstrfEMPLOYEE_ID = "Employee_Id";
        public const string gstrfLOGIN_NAME = "Login_Name";
        public const string strcEMPLOYEE_E1_ROLE_NAME = "Disconnected_1_2_1";
        public const string strtEMPLOYEE = "Employee";
        public const string strfEMPLOYEE_ID = "Employee_Id";
        public const string strfRN_EMPLOYEE_USER_ID = "Rn_Employee_User_Id";
        public const string strfLOGIN_NAME = "Login_Name";
        public const string strfEXTERNAL_SOURCE_NAME = "External_Source_Name";
        public const string strfROLE_ID = "Role_Id";
        public const string strfCOUNTY_ID = "County_Id";
        public const string strfACTIVE = "Active";
        public const string strcEMPLOYEE_E1_COUNTY = "Disconnected_1_2_2";
        public const string strcEMPLOYEE_E1_TIME_ZONE = "Disconnected_1_2_3";
        public const string strtTIME_ZONE = "Time_Zone";
        public const string strfTIME_ZONE_ID = "Time_Zone_Id";
        public const string strfTIME_ZONE_NAME = "Time_Zone_Name";
        public const string strfNEIGHBORHOOD_ID = "Neighborhood_Id";
        public const string strfTIC_CONSTRUCTION_PROJECT_ID = "TIC_Construction_Project_Id";
        public const string strfTIC_CONSTRUCTION_PROJECT_NAME = "TIC_Construction_Project_Name";
        public const string strfTIC_NEIGHBORHOOD_ID = "TIC_Neighborhood_Id";
        public const string strtNEIGHBORHOOD = "Neighborhood";
        public const string strfEXTERNAL_SOURCE_COMMUNITY_ID = "External_Source_Community_Id";
        public const string strfEXTERNAL_SOURCE_SALES_STATUS = "External_Source_Sales_Status";
        public const string strfLEAD_ID = "Lead_Id";

        // Division
        public const string strtDIVISION = "Division";
        public const string strfDIVISION_ID = "Division_Id";
        public const string strfDIVISION_NUMBER = "Division_Number";
        public const string strfNAME = "Name";
        public const string strfADDRESS_1 = "Address_Line_1";
        public const string strfADDRESS_2 = "Address_Line_2";
        public const string strfADDRESS_3 = "Address_Line_3";
        public const string strfZIP = "Zip";
        public const string strfCITY = "City";
        public const string strfSTATE_ = "State_";
        public const string strfCOUNTRY = "Country";
        public const string strfBUILT_OPT_PRICING = "Built_Option_Pricing";
        public const string strfDIVISION_MIGRATED = "MI_MIGRATED";

        // Users
        public const string strtUSERS = "Users";
        public const string strfUSERS_ID = "Users_Id";

        // Team_Member_Role
        public const string strtTEAM_MEMBER_ROLE = "Team_Member_Role";
        public const string strfTEAM_MEMBER_ROLE_ID = "Team_Member_Role_Id";
        public const string strfROLE_NAME = "Role_Name";
        public const string strtCOUNTY = "County";
        public const string strfCOUNTY_NAME = "County_Name";

        // Opp_Product_Location
        public const string strtOPP_PRODUCT_LOCATION = "Opp_Product_Location";
        public const string strfLOCATION_QUANTITY = "Location_Quantity";
        public const string strfOPPORTUNITY_PRODUCT_ID = "Opportunity_Product_Id";

        // Opportunity__Product
        public const string strtOPPORTUNITY__PRODUCT = "Opportunity__Product";
        public const string strcCONTRACTOPTION_E1_COMMUNITYID = "Disconnected_1_2_1";
        public const string strcQUOTEOPTION_E1_COMMUNITYID = "Disconnected_1_2_1";
        public const string strcQUOTE_E1_COMMUNITYID = "Disconnected_1_2_1";
        public const string strcQUOTE_E1_PHASEID = "Disconnected_1_2_2";
        public const string strcQUOTE_E1_PLANID = "Disconnected_1_2_3";
        public const string strcQUOTE_ELEVATION_ID = "Disconnected_1_2_4";
        public const string strcQUOTE_SALES_ASSOCIATE = "Disconnected_1_2_5";
        public const string strcQUOTE_PREMIUM = "Disconnected_1_2_6";
        public const string strcQUOTE_DISCOUNT = "Disconnected_1_2_7";
        public const string strcQUOTE_JOB_NUMBER = "Disconnected_1_2_8";
        public const string strcQUOTE_REALTOR = "Disconnected_1_2_9";
        public const string strcQUOTE_REALTOR_AGENCY = "Disconnected_1_2_10";

        public const string strcCONTRACTOPTION_E1_PHASEID = "Disconnected_1_2_2";
        public const string strcQUOTEOPTION_E1_PHASEID = "Disconnected_1_2_2";
        public const string strcCONTRACTOPTION_E1_LOTID = "Disconnected_1_2_3";
        public const string strcCONTRACTOPTION_NAME = "Disconnected_1_2_4";
        public const string strcQUOTEOPTION_NAME = "Disconnected_1_2_4";
        public const string strcCONTRACTOPTION_DESCRIPTION = "Disconnected_1_2_5";
        public const string strcQUOTEOPTION_DESCRIPTION = "Disconnected_1_2_5";
        public const string strcCONTRACTOPTION_E1_OPTIONID = "Disconnected_1_2_6";
        public const string strcQUOTEOPTION_E1_OPTIONID = "Disconnected_1_2_6";
        public const string strcQUOTEOPTION_BUYER_ID = "Disconnected_1_2_7";
        public const string strfOPPORTUNITY__PRODUCT_ID = "Opportunity__Product_Id";
        public const string strfDIVISION_PRODUCT_ID = "Division_Product_Id";
        public const string strfINITIAL_ADD_DATE = "Initial_Add_Date";
        public const string strfJDE_COMMITTED = "JDE_Committed";
        public const string strcCOMMITTED = "Committed";
        public const string strcBUILT = "Built";
        public const string strfBUILT_OPTION = "Built_Option";
        public const string strfPRODUCT_NAME = "Disconnected_1_2_6";
        public const string strfDIV_PRODUCT_NAME = "Product_Name";
        public const string strfCODE_ = "Code_";
        public const string strfQUANTITY = "Quantity";
        public const string strfSKETCHNUMBER = "SketchNumber";
        public const string strfNET_CONFIG = "Net_Config";
        public const string strfPRODUCT_AVAILABLE = "Product_Available";
        public const string strfFILTER_VISIBILITY = "Filter_Visibility";
        public const string strfOPTION_SELECTED_DATE = "Option_Selected_Date";
        public const string strfOPTIONNOTES = "OptionNotes";
        public const string strfPRODUCT_NUMBER = "Product_Number";

        // Opportunity
        public const string strtOPPORTUNITY = "Opportunity";
        public const string strfOPPORTUNITY_ID = "Opportunity_Id";
        public const string strfACCOUNT_MANAGER_ID = "Account_Manager_Id";
        public const string strfECOE_DATE = "ECOE_Date";
        public const string strfACTUAL_REVENUE_DATE = "Actual_Revenue_Date";
        public const string strfSCHED_BUYER_WALKTHROUGH_DATE = "Scheduled_Walk_Through_Date";
        public const string strfACTUAL_BUYER_WALKTHROUGH_DATE = "Actual_Walk_Through_Date";
        public const string strfSTATUS = "Status";
        public const string strfEXTERNAL_SOURCE_SYNC_STATUS = "External_Source_Sync_Status";
        public const string strfPLAN_NAME_ID = "Plan_Name_Id";
        public const string strfLOT_PREMIUM = "Lot_Premium";
        public const string strfFINANCED_OPTIONS = "Financed_Options";
        public const string strfQUOTE_OPTION_TOTAL = "Quoted_Options_Total";
        public const string strfPIPELINE_STAGE = "Pipeline_Stage";
        public const string strfQUOTE_CREATE_DATE = "Quote_Create_Date";
        public const string strfCONFIGURATION_COMPLETE = "Configuration_Complete";
        public const string strfELEVATION_PREMIUM = "Elevation_Premium";
        public const string strfPLAN_BUILT = "Plan_Built";
        public const string strfACTUAL_DECISION_DATE = "Actual_Decision_Date";
        public const string strfCONTRACT_APPROVED_SUBMITTED = "Contract_Approved_Submitted";
        public const string strfCANCEL_DATE = "Cancel_Date";
        public const string strfCANCEL_REQUEST_DATE = "Cancel_Request_Date";
        public const string strfCANCEL_REASON_ID = "Cancel_Reason_Id";
        public const string strfCANCEL_NOTES = "Cancel_Notes";
        public const string strfLOAN_APPROVAL_DATE = "Loan_Approval_Date";
        public const string strfCONCESSIONS = "Concessions";
        public const string strfJDE_CONTRACTAPPROVALDATE = "Disconnected_1_2_15";
        public const string strfJDE_SALES_TEAM_AB = "Disconnected_1_2_16";
        public const string strfMI_CONTRACTAPPROVALDATE = "MI_ContractApprovalDate";
        public const string strfCONTRACT_APPROVED_DATE = "Contract_Approved_Date";
        public const string strfCONTRACT_APPROVED_DATETIME = "Contract_Approved_Datetime";
        public const string strfENV_EDC_PASSWORD = "ENV_EDC_Password";
        public const string strfTIC_FUTURE_CHANGE_PRICE = "TIC_Future_Change_Price";
        public const string strfADDITIONAL_PRICE = "Additional_Price";
        public const string strfTIC_OPTIONS_SQ_FT = "TIC_Options_Sq_Ft";
        public const string strfTIC_TOTAL_SQ_FT = "TIC_Total_Sq_Ft";
        public const string strfTIC_FUTURE_ELEVATION_PREMIUM = "TIC_Future_Elevation_Premium";
        public const string strfTIC_FUTURE_LOT_PREMIUM = "TIC_Future_Lot_Premium";
        

        // Neighborhood
        public const string strfEXTERNAL_SOURCE_AREA = "External_Source_Area";

        // Product
        public const string strtPRODUCT = "Product";
        public const string strfPRODUCT_ID = "Product_Id";
        public const string strfEXTERNAL_SOURCE_GLOBAL_PLN_FLG = "External_Source_Global_Pln_Flg";
        public const string gstrfEXT_SOURCE_ELEV_CODE = "External_Source_Elev_Code";
        public const string strfBUSINESS_UNIT_LOT_NUM = "Business_Unit_Lot_Number";
        public const string strfLOT_COMMUNITYID = "Disconnected_1_2_1";
        public const string strfLOT_PHASEID = "Disconnected_1_2_2";
        public const string strfLOT_BASEINCENTIVE = "Disconnected_1_2_3";
        public const string strfLOT_LOTINCENTIVE = "Disconnected_1_2_4";
        public const string strfLOT_MORTGAGEINCENTIVE = "Disconnected_1_2_5";
        public const string strfLOT_MARKETINGINCENTIVE = "Disconnected_1_2_6";
        public const string strfLOT_OPTIONINCENTIVE = "Disconnected_1_2_7";
        public const string strfLOT_OPTIONTOTAL = "Disconnected_1_2_8";
        public const string strfLOT_PRIMARYSALESREP = "Disconnected_1_2_9";
        public const string strfLOT_SECONDARYSALESREP = "Disconnected_1_2_10";
        public const string strfSALES_REQUEST_DATE = "Disconnected_1_2_11";
        public const string strfEXTERNAL_SOURCE_PLAN_ID = "External_Source_Plan_Id";
        public const string strfSALE_DATE = "Sales_Date";
        public const string strfRESERVATION_DATE = "Reserved_Date";
        public const string strfLOT_STATUS = "Lot_Status";
        public const string strfEST_CONTRACT_CLOSED_DATE = "Est_Contract_Closed_Date";
        public const string strfCONTRACT_CLOSE_DATE = "Contract_Close_Date";
        public const string strfEXTERNAL_SOURCE_SCHED_BUYER_WT = "External_Source_Sched_Buyer_Wt";
        public const string strfEXTERNAL_SOURCE_ACTUAL_WALKTHR = "External_Source_Actual_Walkthr";
        public const string strfCLOSED_DATE = "Contract_Close_Date";
        public const string strfOWNER_ID = "Owner_Id";
        public const string strfEXTERNAL_SOURCE_CANCEL_DATE = "External_Source_Cancel_Date";
        public const string strfEXTERNAL_SOURCE_CANCEL_REASON = "External_Source_Cancel_Reason";
        public const string strfEXTERNAL_SOURCE_CANCEL_NOTE = "External_Source_Cancel_Note";
        public const string strfEXTERNAL_SOURCE_CREDIT_APRV_DT = "External_Source_Credit_Aprv_Dt";
        public const string strfDISCONNECTED_LOT_BASE_PRICE = "Disconnected_1_2_12";
        public const string strfDISCONNECTED_LOT_PREMIUM = "Disconnected_1_2_14";
        public const string strfLOT_ADDRESS_1 = "Address_1";
        public const string strfLOT_EXTERNAL_SOURCE_REALTOR_ID = "External_Source_Realtor_Id";
        public const string strfLOT_AGENCY = "Disconnected_1_2_16";
        public const string strfRELEASE_DATE = "Release_Date";
        public const string strfOPEN_DATE = "Open_Date";
        public const string strfCOMM_CLOSE_DATE = "Close_Date";
        public const string strfEXTERNAL_SOURCE_BUYER_ID = "External_Source_Buyer_Id";
        public const string strfEXTERNAL_SOURCE_CONSTR_STAGE = "External_Source_Constr_Stage";
        public const string strfLOT_NUMBER = "Lot_Number";
        public const string strfUNIT = "Unit";
        public const string strfTRACT = "Tract";
        public const string strfBLOCK_ = "Block_";
        public const string strfPHASE = "Phase";
        public const string strfTIC_INCENTIVE_LIMIT = "TIC_Incentive_Limit";
        public const string strfTIC_HOUSE_SQ_FT = "TIC_House_Sq_Ft";
        public const string strfTIC_GARAGE_SPACE = "TIC_Garage_Space";
        public const string strfNEIGHBORHOOD = "Neighborhood";
        public const string strfELEVATION_ID = "Elevation_Id";
        public const string strfRESERVATION_CONTRACT_ID = "Reservation_Contract_Id";
        public const string strfDISCONNECTED_TIC_OPTIONS_SQ_FT = "Disconnected_1_2_17";
        public const string strfDISCONNECTED_TIC_TOTAL_SQ_FT = "Disconnected_1_2_18";
        public const string strfDISCONNECTED_TIC_PRICE_OVERRIDE = "Disconnected_1_2_19";
        public const string strfDISCONNECTED_ELEVATION_PREMIUM = "Disconnected_1_2_20";

        // NBHDP_Product
        public const string strfNBHDP_PRODUCT_ID = "NBHDP_Product_Id";
        public const string strfCONSTRUCTION_STAGE_ID = "Construction_Stage_Id";
        public const string strfCATEGORY_ID = "Category_Id";
        public const string strfPRICE = "Price";
        public const string strfSELECTED = "Selected";
        public const string strtINT_LIVE_COMMUNITY = "Int_Live_Community";
        public const string strfINTEGRATED = "Integrated";
        public const string strfCOMMUNITY = "Community";
        public const string strfLOT_ID = "Lot_Id";
        public const string strfOPTION_AVAILABLE_TO = "Option_Available_To";
        public const string strfNEIGHBORHOOD_WILDCARD = "NBHD_WildCard";
        public const string strfOPTION_SUBCATEGORY = "Disconnected_1_2_12";
        public const string strfOPTION_SUB_DESCRIPTION = "Disconnected_1_2_13";
        public const string strfWC_LEVEL = "WC_Level";
        public const string strfDEFAULT_PRODUCT = "Default_Product";
        public const string strfNEXT_COST_PRICE = "Next_Cost_Price";
        public const string strfNEXT_MARGIN = "Next_Margin";
        public const string strfTIC_COST = "TIC_Cost";

        // Lot__Company
        public const string strtLOT__COMPANY = "Lot__Company";
        public const string strfLOTCOMPANY_COMPANYID = "Disconnected_1_2_1";
        public const string strfLOTCOMPANY_LOTID = "Disconnected_1_2_2";
        public const string strfLOT__COMPANY_ID = "Lot__Company_Id";        
        public const string strfLIST_OF_TRADE_CODES = "List_Of_Trade_Codes";
        public const string strfLOT_COMPANY_TRADE_CODE = "Disconnected_1_2_3";
        public const string strfMI_SUB_CATEGORY_ID = "MI_Sub_Category_Id";
        public const string strfLOT_COMPANY_TRADE_DESCRIPTION = "Disconnected_1_2_4";

        // Division_Product
        public const string strfREGION_CODE = "Disconnected_1_2_1";
        public const string strfCATEGORY_DESC = "Disconnected_1_2_2";
        public const string strfCATEGORY_CODE = "Disconnected_1_2_8";
        public const string strfSTAGE_AFTER = "Disconnected_1_2_3";
        public const string strfSTAGE_DESCRIPTION = "Disconnected_1_2_4";
        public const string strfSUB_CATEGORY_NAME = "Disconnected_1_2_5";
        public const string strfSUB_CATEGORY_IMPORT_MATCH_KEY = "Disconnected_1_2_7";
        public const string strfTYPE = "Type";
        public const string strfTIC_CODE = "TIC_Code";
        public const string strfSTYLE_NUMBER = "Style_Number";
        public const string strfMANUFACTURER = "Manufacturer";
        public const string strfUNITS_OF_MEASURE = "Units_Of_Measure";
        public const string strfTIC_MODEL = "TIC_Model";

        // Sub_Category
        public const string strtSUB_CATEGORY = "Sub_Category";
        public const string strfIMPORT_MATCH_KEY = "Import_Match_Key";
        public const string strfSUBCATEGORY_NAME = "Name";
        public const string strfCONFIGURATION_TYPE_ID = "Configuration_Type_Id";
        public const string strfSUB_CATEGORY_ID = "Sub_Category_Id";
        //public const string strfCODE = "MI_Code";

        // Company
        public const string strtCOMPANY = "Company";
        public const string strfCOMPANY_ID = "Company_Id";

        // Contact
        public const string strtCONTACT = "Contact";
        public const string strfCOBUYER_FLAG = "Disconnected_1_2_3";
        public const string strfCONTACT_LOT_ID = "Disconnected_1_2_4";
        public const string strfMI_INVESTOR = "MI_Investor";
        public const string strfCONTACT_SALES_AB = "Disconnected_1_2_5";
        public const string strfCONTACT_COMMUNITY = "Disconnected_1_2_1";
        public const string strfMI_CFT_ID = "MI_CFT_Id";
        public const string strfLEAD_SOURCE_ID = "Lead_Source_Id";

        // Lead_
        public const string strfLEAD_COMMUNITY = "Disconnected_1_2_1";
        public const string strfLEAD_COUNTY = "Disconnected_1_2_2";
        public const string strfLEAD_SALES_AB = "Disconnected_1_2_5";
        public const string strfLEAD_MARKETING_PROJECT_NAME = "Disconnected_1_2_6";
        public const string strfLEAD_AREA = "Disconnected_1_2_7";
        public const string strtLEAD = "Lead_";
        public const string strfMI_M1_DIVISION_ID = "MI_M1_Division_Id";
        public const string strfMI_DIVISION_NAME = "MI_Division_Name";

        // Contact_CoBuyer
        public const string strtCOBUYER_CONTACT = "Contact_CoBuyer";
        public const string strfCO_BUYER_CONTACT_ID = "Co_Buyer_Contact_Id";

        // Trade
        public const string strtTRADE = "Trade";
        public const string strfTRADE_ID = "Trade_Id";
        public const string strfTRADE_CODE = "Trade_Code";
        public const string strfTRADE_NAME = "Trade_Name";

        // Lot__Company__Trade
        public const string strtLOT__COMPANY__TRADE = "Lot__Company__Trade";
        public const string strfLOT_COMPANY_ID = "Lot_Company_Id";

        // Lot__Contact
        public const string strtLOT_CONTACT = "Lot__Contact";
        public const string strfLOT_CONTACT_TYPE = "Type";

        // NBHDP__Company
        public const string strtNBHDP__COMPANY = "NBHDP__Company";
        public const string strfNBHDP__COMPANY_ID = "NBHDP__Company_Id";
        public const string strfNBHDPCOMPANY_COMPANYID = "Disconnected_1_2_1";
        public const string strfNBHDPCOMPANY_NEIGHBORHOODID = "Disconnected_1_2_2";

        // NBHD__Company__Trade
        public const string strtNBHD__COMPANY__TRADE = "NBHD__Company__Trade";
        public const string strfNBHD__COMPANY__TRADE_ID = "NBHD__Company__Trade_Id";
        public const string strfNBHD__COMPANY_ID = "NBHD__Company_Id";

        // Company_Trade
        public const string strtCOMPANY_TRADE = "Company_Trade";

        // System
        public const string strtSYSTEM = "System";
        public const string strfSYSTEM_ID = "System_Id";
        public const string strfINT_DEFAULT_CANCEL_REASON_ID = "Int_Default_Cancel_Reason_Id";
        public const string strfINT_DEFAULT_CONSTRUCTION_STAGE = "Int_Default_Construction_Stage";

        // Cancel_Reason
        public const string strtCANCEL_REASON = "Cancel_Reason";
        public const string strfCANCEL_REASON = "Cancel_Reason";

        // Opportunity_Adjustment
        public const string strtOPPORTUNITY_ADJUSTMENT = "Opportunity_Adjustment";
        public const string strfOPPORTUNITY_ADJUSTMENT_ID = "Opportunity_Adjustment_Id";
        public const string strfSUM_FIELD = "Sum_Field";

        // Adjustment Types
        public const string strcADJUSTMENT_BASE_HOUSE = "Base House";
        public const string strcADJUSTMENT_LOT_INCENTIVE = "Lot Incentive";
        public const string strcADJUSTMENT_MORTGATE_INCENTIVES = "Mortgage Incentives";
        public const string strcADJUSTMENT_MARKETING_INCENTIVES = "Marketing Incentives";
        public const string strcADJUSTMENT_OPTION = "Option";

        // Lot__Contact
        public const string strtLOT__CONTACT = "Lot__Contact";
        public const string strfPRIMARY_CONTACT = "Primary_Contact";
        public const string strfEXTERNAL_SOURCE_ID = "External_Source_Id";
        public const string gstrfCOMMUNITY_COUNTY = "Disconnected_1_2_1";

        // Construction_Stage
        public const string strtCONSTRUCTION_STAGE = "Construction_Stage";
        public const string strfCONSTRUCTION_STAGE_ORDINAL = "Construction_Stage_Ordinal";
        public const string strfCONSTRUCTION_STAGE_NAME = "Construction_Stage_Name";
        public const string strfCORPORATE = "Corporate";

        // NBHD_Phase
        public const string gstrtNBHD_PHASE = "NBHD_Phase";
        public const string gstrfNBHD_PHASE_ID = "NBHD_Phase_Id";
        public const string gstrfNBHD_PHASE_ADDR_1 = "Address_1";
        public const string gstrfNBHD_PHASE_ADDR_2 = "Address_2";
        public const string gstrfNBHD_PHASE_ADDR_3 = "Address_3";
        public const string gstrfAREA_CODE = "Area_Code";
        public const string gstrfPHONE = "Phone";
        //Disconnected fields
        public const string gstrfPHASE_COMMUNITY = "Disconnected_1_2_1";
        //Queries
        public const string gstrqPHASE_BY_EXT_SOURCE_ID = "HBInt: Phase for Extern Src Id ?";
        public const string gstrfSALES_MANAGER_ID = "Sales_Manager_Id";
        public const string gstrfCONSTRUCTION_MANAGER_ID = "Construction_Manager_Id";

        public const string gstrtDIVISION_ADJUSTMENT = "Division_Adjustment";
        //Fields
        public const string gstrfDIVISION_ADJUSTMENT_ID = "Division_Adjustment_Id";
        public const string gstrfEXTERENAL_SOURCE_ID = "External_Source_Id";
        //External_Source_Id values
        public const string gstrPRICE_ADJUST = "PriceAdj";

        public const string gstrfADJUSTMENT_REASON = "Adjustment_Reason";
        //Adjustment_Reason values
        public const string gstrCONTRACT_PRICE_ADJUST = "Contract Price Adjustment";

        public const string gstrfADJUSTMENT_TYPE = "Adjustment_Type";
        //Queries
        public const string gstrqDIVISION_ADJUST_BY_DIVISION_ID = "HBInt: Division Adjustments By Division Id ?";

        public const string gstrfINACTIVE = "Inactive";
        public const string gstrfRELEASE_ID = "Release_Id";
        public const string gstrtRELEASE_ADJUSTMENT = "Release_Adjustment";

        public const string gstrfPRODUCT_EXT_SOURCE_GLOBAL_PLAN_FLAG = "External_Source_Global_Pln_Flg";
        public const string strfPLAN_ID = "Plan_Id";

        public const string strfNEXT_PRICE = "Next_Price";
        public const string strfPRICE_CHANGE_DATE = "Price_Change_Date";
        public const string strfPRODUCT_PRICE_UPDATE_DATE = "Product_Price_Update_Date";
        public const string strfPRODUCT_PRICE_UPDATE_DATETIME = "Product_Price_Update_Datetime";


        public const string strtNBHDP_PRODUCT = "NBHDP_Product";
        public const string strfCURRENT_PRICE = "Current_Price";
        public const string strtOPPORTUNITY_TEAM_MEMBER = "Opportunity_Team_Member";

        public const string strfRELEASE_ADJUSTMENT_ID = "Release_Adjustment_Id";
        public const string strfOPP_ADJUST_AMOUNT = "Adjustment_Amount";

        public const string strtPRICE_CHANGE_HISTORY = "Price_Change_History";
        public const string strfCHANGE_DATE = "Change_Date";
        public const string strfCHANGE_TIME = "Change_Time";
        public const string strfCHANGE_TIMESTAMP = "Change_TimeStamp";
        public const string strfPROCESSED = "Processed";

        public const string strfEXTERNAL_SOURCE_PLAN_CODE = "External_Source_Plan_Code";
        public const string strfEXTERNAL_SOURCE_ELEV_CODE = "External_Source_Elev_Code";
        public const string strfEXTERNAL_SOURCE_PHASE_CODE = "External_Source_Phase_Code";

        public const string strfOPTION_PHASEID = "Disconnected_1_2_1";
        public const string strfOPTION_PLANID = "Disconnected_1_2_2";
        public const string strfOPTION_CATEGORY1 = "Disconnected_1_2_3";
        public const string strfOPTION_CATEGORY1DESC = "Disconnected_1_2_4";
        public const string strfOPTION_CURRENTSALEPRICE = "Disconnected_1_2_5";
        public const string strfOPTION_LOTID = "Disconnected_1_2_6";
        public const string strfOPTION_STAGENOTAFTER = "Disconnected_1_2_7";
        public const string strfOPTION_STAGENOTAFTER_DESC = "Disconnected_1_2_10";
        public const string strfOPTION_DESCRIPTION = "Disconnected_1_2_8";
        public const string strfELEVATION_CODE = "Elevation_Code";
        public const string strfPLAN_PRICE = "Disconnected_1_2_3";
        public const string strfPLAN_MARGIN = "Disconnected_1_2_4";
        public const string strfPLAN_SQUAREFEET = "Disconnected_1_2_2";
        public const string strfPLAN_PLANTYPE = "Disconnected_1_2_1";
        public const string strfPLAN_DESCRIPTION = "Disconnected_1_2_7";
        public const string strfEFFECTIVE_DATE_PLAN = "Disconnected_1_2_5";
        public const string strfEFFECTIVE_DATE_OPTION = "Disconnected_1_2_9";
        public const string strfREQUIRED_DEPOSIT_AMT = "Disconnected_1_2_10";
        public const string strfCONTIGENCY_CODE = "Disconnected_1_2_13";
        public const string strfESTIMATED_SQ_FEET = "Estimated_Square_Feet";
        public const string strfOPTION_STAGENOTAFTERDESC = "Disconnected_1_2_10";
        public const string strfOPTION_AREAID = "Disconnected_1_2_11";
        //AM2010.08.19 - Changed Option Price to get value from Disconnected_1_2_8 instead of 1_2_5
        public const string strfOPTION_PRICE = "Disconnected_1_2_8";
        public const string strfOPTION_MARGIN = "Disconnected_1_2_8";
        public const string strfOPTION_TIC_COST = "Disconnected_1_2_5";

        public const string strfREMOVAL_DATE = "Removal_Date";
        public const string strfPLAN_TYPE = "Plan_Type";
        public const string strfPLAN_CODE = "Plan_Code";
        public const string strfPRODUCT_CODE = "Code_";
        public const string strtDIVISION_PRODUCT = "Division_Product";
        public const string strqDIVISION_PRODUCT_BY_EXT_SOURCE_ID_TYPE = "HBInt: Division Product for ExternSrcId ? Type ?";
        public const string strfMI_ELEVATION_CODE = "MI_Elevation_Code";

        public const string strtCONFIG_TYPE = "Configuration_Type";
        public const string strfCONFIG_TYPE_ID = "Configuration_Type_Id";
        public const string strfCONFIG_TYPE_NAME = "Configuration_Type_Name";
        public const string gstrfCOMPONENT = "Component";
        public const string strqCONFIG_TYPE_BY_CONFIG_TYPE_NAME_COMPONENT = "HBInt: Config Type with Config. Type Name ? Component ?";
        public const string strPLAN = "Plan";


        public const string strfAVAILABLE_DATE = "Available_Date";
        public const string strfRELEASE_WILDCARD = "Release_Wildcard";
        public const string strfPLAN_WILDCARD = "Plan_Wildcard";
        public const string strfDESCRIPTION = "Description";

        // Price_Change_History
        public const string strfMARGIN = "Margin";
        public const string strfCOST = "Cost_Price";
        //public const string strfREGION_ID = "Region_Id";
        public const string strfPLAN_HAS_STD_OPT = "Plan_Has_StndOptions";

        // Rn_Appointments
        public const string strtRN_APPOINTMENTS = "Rn_Appointments";
        public const string strfRN_APPOINTMENTS_ID = "Rn_Appointments_Id";
        public const string strfNOTES = "Notes";
        public const string strfAPPT_DATE = "Appt_Date";
        public const string strfACTIVITY_COMPLETE_DATE = "Activity_Complete_Date";
        public const string strfACTIBITY_COMPLETE = "Activity_Complete";
        public const string strfACT_TYPE = "Disconnected_1_2_1";
        public const string strfACT_SALES_REP = "Disconnected_1_2_2";
        public const string strfACT_CONTACT = "Disconnected_1_2_3";
        public const string strfACT_COMMUNITY = "Disconnected_1_2_4";
        public const string strfACT_DIVISION = "Disconnected_1_2_5";
        public const string strfACT_LEAD = "Disconnected_1_2_6";
        public const string strfACTIVITY_TYPE = "Activity_Type";
        public const string strfCONTACT = "Contact";
        public const string strfASSIGNED_BY = "Assigned_By";
        public const string strfRN_EMPLOYEE_ID = "Rn_Employee_Id";
        public const string strfAPPT_DESCRIPTION = "Appt_Description";
        public const string strfACCESS_TYPE = "Access_Type";
        public const string strfCONTACT_PROFILE_NBHD_ID = "Contact_Profile_Nbhd_Id";

        // Marketing_Project
        public const string strtMARKETING_PROJECT = "Marketing_Project";
        public const string strfMARKETING_PROJECT_ID = "Marketing_Project_Id";
        public const string strfMARKETING_PROJECT_NAME = "Marketing_Project_Name";
        public const string strfCON_MARKETING_PROJECT_NAME = "Disconnected_1_2_6";

        // Support_Incident
        public const string strtSUPPORT_INCIDENT = "Support_Incident";
        public const string strfSI_NEIGHBORHOOD_ID = "Disconnected_1_2_1";
        public const string strfSI_CONTACT_ID = "Disconnected_1_2_2";
        public const string strfSI_LOT_ID = "Disconnected_1_2_3";
        public const string strfSI_OWNER_ID = "Disconnected_1_2_4";
        public const string strfSI_RESPOND_TO_EMPLOYEE_ID = "Disconnected_1_2_5";
        public const string strfSI_RESPOND_TO_CONTACT_ID = "Disconnected_1_2_6";
        public const string strfSI_CONTACT_NAME = "Disconnected_1_2_7";
        public const string strfMI_COMMUNITY_LOT_DESC = "MI_Community_Lot_Desc";
        public const string strfMI_OVERRIDE_HOMESITE = "MI_Override_Homesite";
        public const string strfLONG_DESCRIPTION = "Long_Description";
        public const string strfRESPOND_TO_EMPLOYEE = "Respond_To_Employee";
        public const string strfRESPOND_TO_CONTACT = "Respond_To_Contact";
        public const string strfSI_DIVISION_ID = "Disconnected_1_2_8";
        public const string strfSUPPORT_INCIDENT_ID = "Support_Incident_Id";

        // Support_Incident_Player
        public const string strtSUPPORT_INCIDENT_PLAYER = "Support_Incident_Player";
        public const string strfSI_TEAM_SI_ID = "Disconnected_1_2_1";
        public const string strfSI_TEAM_PLAYER_ID = "Disconnected_1_2_2";
        public const string strfPLAYER_ID = "Player_Id";
        public const string strfMI_EXTERNAL_SOURCE_ID = "MI_External_Source_Id";

        // Support_Step
        public const string strtSUPPORT_STEP = "Support_Step";
        public const string strfSI_STEP_SI_ID = "Disconnected_1_2_1";
        public const string strfSI_STEP_ASSIGNED_TO = "Disconnected_1_2_2";
        public const string strfSI_STEP_CATEGORY = "Disconnected_1_2_3";
        public const string strfSI_STEP_SUBJECT = "Disconnected_1_2_4";
        public const string strfSI_STEP_TOPIC = "Disconnected_1_2_5";
        public const string strfASSIGNED_TO_ID = "Assigned_To_Id";
        public const string strfMI_SUPPORT_CATEGORY_ID = "MI_Support_Category_Id";
        public const string strfMI_SUPPORT_SUBJECT_ID = "MI_Support_Subject_Id";
        public const string strfMI_SUPPORT_TOPIC_ID = "MI_Support_Topic_Id";
        public const string strfSI_STEP_DIVISION_ID = "Disconnected_1_2_6";
        public const string strfSUPPORT_STEP_ID = "Support_Step_Id";

        // Support_Category
        public const string strtSUPPORT_CATEGORY = "Support_Category";
        public const string strfSUPPORT_CATEGORY_ID = "Support_Category_Id";
        public const string strfSUPPORT_CATEGORY_NAME = "Support_Category_Name";

        // Support_Subject
        public const string strtSUPPORT_SUBJECT = "Support_Subject";
        public const string strfSUPPORT_SUBJECT_ID = "Support_Subject_Id";
        public const string strfSUPPORT_SUBJECT_NAME = "Support_Subject_Name";

        // Support_Topic
        public const string strtSUPPORT_TOPIC = "Support_Topic";
        public const string strfSUPPORT_TOPIC_ID = "Support_Topic_Id";
        public const string strfSUPPORT_TOPIC_NAME = "Support_Topic_Name";
        public const string strfSUBJECT_ID = "Subject_Id";

        // Work_Order
        public const string strtWORK_ORDER = "Work_Order";
        public const string strfWO_SUPPORT_STEP_ID = "Disconnected_1_2_1";
        public const string strfWO_ASSIGNED_TO = "Disconnected_1_2_2";
        public const string strfWO_ASSIGNED_BY = "Disconnected_1_2_3";
        public const string strfWO_TRADE = "Disconnected_1_2_4";
        public const string strfWO_CONTRACTOR = "Disconnected_1_2_5";
        public const string strfWO_CCONTRACTOR = "Disconnected_1_2_6";
        public const string strfWO_DIVISION = "Disconnected_1_2_7";
        public const string strfWO_CONTRACTOR_NAME = "Disconnected_1_2_8";
        public const string strfASSIGNED_TO_EMPLOYEE = "Assigned_To_Employee";
        //public const string strfASSIGNED_BY = "Assigned_By";
        public const string strfASSIGNED_TO_CONTRACTOR = "Assigned_To_Contractor";
        public const string strfMI_ASSIGN_TO_CONTRACTOR_CONTACT_ID = "MI_AssignContractor_Contact_Id";

        // Contact_Web_Details
        public const string strtCONTACT_WEB_DETAILS = "Contact_Web_Details";
        public const string strfCONTACT_WEB_DETAILS_Id = "Contact_Web_Details_Id";
        //public const string strfLOGIN_NAME = "Login_Name";
        public const string strfPASSWORD_ENCRYPT = "Password_Encrypt";
        public const string strfTIME_ZONE = "Time_Zone";
        public const string strfCONTACT_EMAIL_ADDRESS = "Contact_Email_Address";
        public const string strfSEND_NOTIFICATION = "Send_Notification";

        // Region
        public const string strtREGION = "Region";
        public const string strfREGION_ID = "Region_Id";

        //Envision Constants (Reusing some of this in Option Selecitons Code)
        public const string LOGGING_ASR_NAME = "PAHB Envision Logging";
        public const string ENVISION_INTEGRATION_ASR_NAME_TRANS = "PAHB Envision Integration Transactional";
        public const string ENVISION_INTEGRATION_ASR_NAME = "PAHB Envision Integration";


        //Escrow Table
        public const string strfTIC_LOT_ID = "TIC_Lot_Id";
        public const string strfTIC_EST_LOAN_APP_DATE = "TIC_Est_Loan_App_Date";
        public const string strfTIC_ACT_LOAN_APP_DATE = "TIC_Act_Loan_App_Date";
        public const string strfTIC_EST_APPROVAL_DATE = "TIC_Est_Approval_Date";
        public const string strfTIC_EST_DOCS_TO_ESCROW_DATE = "TIC_Est_Docs_To_Escrow_Date";
        public const string strfTIC_ACT_DOCS_TO_ESCROW_DATE = "TIC_Act_Docs_To_Escrow_Date";
        public const string strfTIC_EST_DOCS_SIGN_DATE = "TIC_Est_Docs_Sign_Date";
        public const string strfTIC_ACT_DOCS_SIGN_DATE = "TIC_Act_Docs_Sign_Date";
        public const string strfTIC_ACT_APPROVAL_DATE = "TIC_Act_Approval_Date";
        public const string strfTIC_APPRAISAL_ORDER = "TIC_Appraisal_Order";
        public const string strfTIC_APPRAISAL_RECEIVED = "TIC_Appraisal_Received";
        public const string strfTIC_LOAN_STATUS_COMMENTS = "TIC_Loan_Status_Comments";
        public const string strfTIC_FLOORING_RELEASE = "TIC_Flooring_Release";
        public const string strfTIC_HOMEOWNER_WALK_SCHEDULED = "TIC_Homeowner_Walk_Scheduled";
        public const string strfTIC_HOMEOWNER_WALK_ACTUAL = "TIC_Homeowner_Walk_Actual";
        public const string strfTIC_FINAL_PRICE_SENT_DATE = "TIC_Final_Price_Sent_Date";
        public const string strfTIC_BUILDER_PACK_SENT_DATE = "TIC_Builder_Pack_Sent_Date";
        public const string strfTIC_JOB_CARD_REC_DATE = "TIC_Job_Card_Rec_Date";
        public const string strfTIC_NOTICE_OF_COMP_SUB_DATE = "TIC_Notice_Of_Comp_Sub_Date";
        public const string strfTIC_DEL_ASSMT_AT_CLOSING_DATE = "TIC_Del_Assmt_At_Closing_Date";
        public const string strfTIC_YELLOW_REPORT_RECEIVED = "TIC_Yellow_Report_Received";
        public const string strfTIC_WHITE_REPORT_RECEIVED = "TIC_White_Report_Received";
        public const string strfTIC_GAS_METER_INST_DATE = "TIC_Gas_Meter_Inst_Date";
        public const string strfTIC_ESCROW_DOC_COMMENTS = "TIC_Escrow_Doc_Comments";
        public const string strfTIC_FUNDED = "TIC_Funded";
        public const string strfTIC_ESTIMATED_COE = "TIC_Estimated_COE";
        public const string strtTIC_ESCROW = "TIC_Escrow";
        public const string strfTIC_GRANT_DEED_TO_ESCROW = "TIC_Grant_Deed_To_Escrow_Comp";

        public const string strfCONTINGENCY_EXP_DATE = "Expiration_Date";
        public const string strfDISC_CONTINGENCY_EXP_DATE = "Disconnected_1_5_12";
        public const string strfCONTINGENCY_TIC_REASON_CODE = "TIC_Reason_Code";
        public const string strfDISC_CONTINGENCY_TIC_REASON_CODE = "Disconnected_1_5_11";
        public const string strtCONTINGENCY = "Contingency";
    
        public const string strfDISC_TIC_EST_LOAN_APP_DATE = "Disconnected_1_5_1";
        public const string strfDISC_TIC_ACT_LOAN_APP_DATE = "Disconnected_1_5_2";
        public const string strfDISC_TIC_EST_APPROVAL_DATE = "Disconnected_1_5_3";
        public const string strfDISC_TIC_EST_DOCS_TO_ESCROW_DATE = "Disconnected_1_5_4";
        public const string strfDISC_TIC_ACT_DOCS_TO_ESCROW_DATE = "Disconnected_1_5_5";
        public const string strfDISC_TIC_EST_DOCS_SIGN_DATE = "Disconnected_1_5_6";
        public const string strfDISC_TIC_ACT_DOCS_SIGN_DATE = "Disconnected_1_5_7";                    
        public const string strfDISC_TIC_ACT_APPROVAL_DATE = "Disconnected_1_5_8";
        public const string strfDISC_TIC_APPRAISAL_ORDER = "Disconnected_1_5_9";
        public const string strfDISC_TIC_APPRAISAL_RECEIVED = "Disconnected_1_5_10";
        public const string strfDISC_TIC_LOAN_STATUS_COMMENTS = "Disconnected_1_5_13";
        public const string strfDISC_TIC_FLOORING_RELEASE = "Disconnected_1_5_16";
        public const string strfDISC_TIC_HOMEOWNER_WALK_SCHEDULED = "Disconnected_1_5_19";
        public const string strfDISC_TIC_HOMEOWNER_WALK_ACTUAL = "Disconnected_1_5_20";
        public const string strfDISC_TIC_FINAL_PRICE_SENT_DATE = "Disconnected_1_5_21";
        public const string strfDISC_TIC_BUILDER_PACK_SENT_DATE = "Disconnected_1_5_22";
        public const string strfDISC_TIC_JOB_CARD_REC_DATE = "Disconnected_1_5_23";
        public const string strfDISC_TIC_NOTICE_OF_COMP_SUB_DATE = "Disconnected_1_5_24";
        public const string strfDISC_TIC_DEL_ASSMT_AT_CLOSING_DATE = "Disconnected_1_5_25";
        public const string strfDISC_TIC_YELLOW_REPORT_RECEIVED = "Disconnected_1_5_26";
        public const string strfDISC_TIC_WHITE_REPORT_RECEIVED = "Disconnected_1_5_27";
        public const string strfDISC_TIC_GAS_METER_INST_DATE = "Disconnected_1_5_28";
        public const string strfDISC_TIC_ESCROW_DOC_COMMENTS = "Disconnected_1_5_29";
        public const string strfDISC_ECOE_DATE = "Disconnected_1_5_32";
        public const string strfDISC_TIC_FUNDED = "Disconnected_1_5_33";
        public const string strfTIC_CONTRACT_ID = "TIC_Contract_Id";
        public const string strfDISC_TIC_GRANT_TO_DEED = "Disconnected_1_5_34";

        //TIC_Option_Change_Log
        public const string strtTIC_OPTION_CHANGE_LOG = "TIC_Option_Change_Log";
        public const string strfFIELD_NAME = "Field_Name";
        public const string strfORIGINAL_VALUE = "Original_Value";
        public const string strfNEW_VALUE = "New_Value";
        public const string strfTIME_PROCESSED_INTO_PIVOTAL = "Time_Processed_Into_Pivotal";
        public const string strfCHATEAU_FILE_FROM = "Chateau_File_From";
        public const string strfOPTION_NUMBER = "Option_Number";
        public const string strfIS_NEW = "Is_New_Record";
       



        // amcnab 2010-00-09: Added constant structs for Opportunity_Adjustment Table - cleaner than above.
        public struct OpportunityAdjustmentTable
        {
            public const string TABLE_NAME = "Opportunity_Adjustment";

            // Disconnected Fields on the "HBIntAdjustment" Active Form
            public struct DisconnectedFields
            {
                public const string NEIGHBORHOOD_CODE = "Disconnected_1_2_1";
                public const string PHASE_CODE = "Disconnected_1_2_2";
                public const string LOT_NUMBER = "Disconnected_1_2_3";
                public const string UNIT = "Disconnected_1_2_4";
                public const string TRACT = "Disconnected_1_2_5";
                public const string ADJUSTMENT_TYPE = "Disconnected_1_2_6";
            }

            // These Table Fields appear on the "HBIntAdjustment" Active Form
            public struct TableFields
            {
                public const string OPPORTUNITY_ADJUSTMENT_ID = "Opportunity_Adjustment_Id";
                public const string ADJUSTMENT_AMOUNT = "Adjustment_Amount";
                public const string ADJUSTMENT_TYPE = "Adjustment_Type";
                public const string APPLY_TO = "Apply_To";
                public const string NOTES = "Notes";
                public const string OPPORTUNITY_ID = "Opportunity_Id";
                public const string SELECTED = "Selected"; 
                public const string SUM_FIELD = "Sum_Field";
                public const string RELEASE_ADJUSTMENT_ID = "Release_Adjustment_Id";
                public const string TIC_INT_EXTERNAL_SOURCE_ID = "TIC_INT_External_Source_Id";                
            }
        }

        // amcnab 2010-08-10: Added constant structs for TIC_INT_SAM_Contract Table
        public struct TICIntSamContractTable
        {
            public const string TABLE_NAME = "TIC_INT_SAM_Contract";

            public struct TableFields
            {
                public const string TIC_INT_SAM_CONTRACT_ID = "TIC_INT_SAM_Contract_Id";
                public const string RN_DESCRIPTOR = "Rn_Descriptor";
                public const string RN_CREATE_DATE = "Rn_Create_Date";
                public const string RN_CREATE_USER = "Rn_Create_User";
                public const string RN_EDIT_DATE = "Rn_Edit_Date";
                public const string RN_EDIT_USER = "Rn_Edit_User";
                public const string BASE_PRICE = "Base_Price";
                public const string LOT_PREMIUM = "Lot_Premium";
                public const string PREPLOT_TOTAL = "Preplot_Total";
                public const string OTHER_OPTION_TOTAL = "Other_Option_Total";
                public const string INCENTIVE_TOTAL = "Incentive_Total";
                public const string ELEVATION_PREMIUM = "Elevation_Premium";
                public const string NEIGHBORHOOD = "Neighborhood";
                public const string NBHD_PHASE = "Nbhd_Phase";
                public const string PLAN_ = "Plan_";
                public const string ELEVATION = "Elevation";
                public const string UNIT = "Unit";
                public const string TRACT = "Tract";
                public const string LOT_NUMBER = "Lot_Number";
                public const string BUSINESS_UNIT_LOT_NUMBER = "Business_Unit_Lot_Number";
                public const string SALE_DATE = "Sale_Date";
                public const string PIPELINE_STAGE = "Pipeline_Stage";
                public const string STATUS = "Status";
                public const string PRODUCT_ID = "Product_Id";
                public const string STATUS_CHANGE_NUMBER = "Status_Change_Number";
                public const string CHANGED_BY = "Changed_By";
                public const string CHANGED_ON = "Changed_On";
                public const string LOT_STATUS_CHANGED_TO = "Lot_Status_Changed_To";
                public const string DATE_OF_BUS_TRANSACTION = "Date_Of_Bus_Transaction";
                public const string COMMENTS = "Comments";
                public const string OPPORTUNITY_ID = "Opportunity_Id";
                public const string SALES_VALUE = "Sales_Value";
                public const string CAUSED_BY_SALE = "Caused_By_Sale";
            }
        }

        // A.Maldonado 2010.08.23 - Added constant structures for TIC_INT_OPTION_SELECTIONS integration
        public struct TICIntOptionSelectionsTable
        {
            public const string TABLE_NAME = "TIC_INT_OPTION_SELECTIONS";

            public struct TableFields
            {
                public const string TIC_INT_OPTION_SELECTIONS_ID = "TIC_INT_Option_Selections_Id";
                public const string OPTION_NUMBER = "Option_Number";
                public const string TYPE = "Type";
                public const string TOTAL_QUANTITY = "Total_Quantity";
                public const string UNIT_PRICE = "Unit_Price";
                public const string TOTAL_PRICE = "Total_Price";
                public const string LOT_NUMBER = "Lot_Number";
                public const string UNIT = "Unit";
                public const string TRACT = "Tract";
                public const string NEIGHBORHOOD = "Neighborhood";
                public const string PLAN_CODE = "Plan_Code";
                public const string PHASE = "Phase";
                public const string UNIT_COST = "Unit_Cost";
                public const string PRODUCT_NAME = "Product_Name";
                public const string NOTES = "Notes";
                public const string COLOR_NOTES = "Color_Notes";
                public const string STYLE_NOTES = "Style_Notes";
                public const string LOCATION_NOTE = "Location_Notes";
                public const string CATEGORY_CODE = "Category_Code";
                public const string CATEGORY_DESC = "Category_Description";
                public const string DEPOSIT_TOTAL = "Deposit_Total";
                public const string COMPLETE_NOTES = "Complete_Notes";
                public const string STATUS = "Status";
                public const string PROCESS_FAILURE_REASON = "Process_Failure_Reason";
                public const string TRANSACTION_ID = "Transaction_Id";
                public const string TRANSACTION_DATE = "Rn_Create_Date";


            }

            public struct Queries
            { 
                public const string OPTION_SELECTIONS_READY_FOR_SYNC = "TIC : Option Selections For Sync";
                public const string OPTION_SELECTIONS_SUCCESS = "TIC : Success Option Selections";
            }

            public struct Statuses
            {
                public const string SUCCESS = "Success";
                public const string FAILED = "Failed";
                public const string QUEUED = "Queued";
            }

        }



    }
}
