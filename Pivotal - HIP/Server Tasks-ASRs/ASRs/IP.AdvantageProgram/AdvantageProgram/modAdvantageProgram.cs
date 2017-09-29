using System;
using System.Collections.Generic;
using System.Text;

namespace CRM.Pivotal.IP
{
    internal class modAdvantageProgram
    {
        //*****************************ADVANTAGE PROGRAM******************************

        //TABLE NAME
        public const string strtTIC_PROJECT_REGISTRATION = "TIC_Project_Registration";

        //FIELDS
        public const string strfRN_CREATE_DATE="Rn_Create_Date";
        public const string strfRN_CREATE_USER="Rn_Create_User";
        public const string strfRN_DESCRIPTOR="Rn_Descriptor";
        public const string strfRN_EDIT_DATE="Rn_Edit_Date";
        public const string strfRN_EDIT_USER="Rn_Edit_User";
        public const string strfTIC_1ST_PREF_CONST_PHASE_ID="TIC_1st_Pref_Const_Phase_Id";
        public const string strfTIC_1ST_PREFERRED_LOT_ID="TIC_1st_Preferred_Lot_Id";
        public const string strfTIC_1ST_PREFERRED_PLAN_ID="TIC_1st_Preferred_Plan_Id";
        public const string strfTIC_2ND_PREF_CONST_PHASE_ID="TIC_2nd_Pref_Const_Phase_Id";
        public const string strfTIC_2ND_PREFERRED_LOT_ID="TIC_2nd_Preferred_Lot_Id";
        public const string strfTIC_2ND_PREFERRED_PLAN_ID="TIC_2nd_Preferred_Plan_Id";
        public const string strfTIC_BANK_NAME="TIC_Bank_Name";
        public const string strfTIC_BANK_PREQUAL_OVERRIDE="TIC_Bank_Prequal_Override";
        public const string strfTIC_COBUYER_CONTACT_ID="TIC_CoBuyer_Contact_Id";
        public const string strfTIC_CONSTRUCTION_PHASE_ID="TIC_Construction_Phase_Id";
        public const string strfTIC_CONTACT_ID="TIC_Contact_Id";
        public const string strfTIC_CONTACT_INITIATED="TIC_Contact_Initiated";
        public const string strfTIC_CONTACT_PROFILE_NBHD_ID="TIC_Contact_Profile_NBHD_Id";
        public const string strfTIC_CONTINGENT="TIC_Contingent";
        public const string strfTIC_DATE_COMPLETED="TIC_Date_Completed";
        public const string strfTIC_DATE_CONTACTED="TIC_Date_Contacted";
        public const string strfTIC_DATE_INITIATED="TIC_Date_Initiated";
        public const string strfTIC_DATE_OF_VISIT="TIC_Date_of_Visit";
        public const string strfTIC_DATE_RECEIVED="TIC_Date_Received";
        public const string strfTIC_DATE_SCHEDULED="TIC_Date_Scheduled";
        public const string strfTIC_IF_DENIED_OTHER_WHY="TIC_If_Denied_Other_Why";
        public const string strfTIC_IF_DENIED_WHY="TIC_If_Denied_Why";
        public const string strfTIC_IF_OTHER_WHY_1="TIC_If_Other_Why_1";
        public const string strfTIC_IF_OTHER_WHY_2="TIC_If_Other_Why_2";
        public const string strfTIC_IF_OTHER_WHY_3="TIC_If_Other_Why_3";
        public const string strfTIC_IF_OTHER_WHY_4="TIC_If_Other_Why_4";
        public const string strfTIC_LEAD_ID="TIC_Lead_Id";
        public const string strfTIC_LOT_ID="TIC_Lot_Id";
        public const string strfTIC_NEIGHBORHOOD_ID="TIC_Neighborhood_Id";
        public const string strfTIC_NOTES_1="TIC_Notes_1";
        public const string strfTIC_NOTES_2="TIC_Notes_2";
        public const string strfTIC_NOTES_3="TIC_Notes_3";
        public const string strfTIC_NOTES_4="TIC_Notes_4";
        public const string strfTIC_OVERALL_STATUS="TIC_Overall_Status";
        public const string strfTIC_OWN_OR_RENT="TIC_Own_or_Rent";
        public const string strfTIC_PLAN_ID="TIC_Plan_Id";
        public const string strfTIC_PLAN_SELECTED="TIC_Plan_Selected";
        public const string strfTIC_PREFERRED_LOT="TIC_Preferred_Lot";
        public const string strfTIC_PREFERRED_PLAN_SELECTED="TIC_Preferred_Plan_Selected";
        public const string strfTIC_PREQUAL_COMPLETED="TIC_Prequal_Completed";
        public const string strfTIC_PREQUAL_DOCS_RECEIVED="TIC_Prequal_Docs_Received";
        public const string strfTIC_PREQUAL_INITIATED="TIC_Prequal_Initiated";
        public const string strfTIC_PREVIOUS_REGISTRATION_ID="TIC_Previous_Registration_Id";
        public const string strfTIC_PRIORITY_DATE="TIC_Priority_Date";
        public const string strfTIC_PROJECT_REGISTRATION_ID="TIC_Project_Registration_Id";
        public const string strfTIC_PURCHASED_ELSEWHERE="TIC_Purchased_Elsewhere";
        public const string strfTIC_REGISTRATION_RECEIVED_DATE="TIC_Registration_Received_Date";
        public const string strfTIC_SCHEDULED_VISIT="TIC_Scheduled_Visit";
        public const string strfTIC_SORT_ORDER="TIC_Sort_Order";
        public const string strfTIC_STATUS_1="TIC_Status_1";
        public const string strfTIC_STATUS_2="TIC_Status_2";
        public const string strfTIC_STATUS_3="TIC_Status_3";
        public const string strfTIC_STATUS_4="TIC_Status_4";
        public const string strfTIC_STATUS_DATE_1="TIC_Status_Date_1";
        public const string strfTIC_STATUS_DATE_2="TIC_Status_Date_2";
        public const string strfTIC_STATUS_DATE_3="TIC_Status_Date_3";
        public const string strfTIC_STATUS_DATE_4="TIC_Status_Date_4";
        public const string strfTIC_TICKLE="TIC_Tickle";
        public const string strfTIC_VIP="TIC_VIP";
        public const string strfTIC_VIP_DATE="TIC_VIP_Date";
        public const string strfTIC_VISIT_OCCURRED="TIC_Visit_Occurred";
        public const string strfTIC_WEBSITE_STATUS="TIC_Website_Status";
        public const string strfTIC_WEBSITE_STATUS_NOTES="TIC_Website_Status_Notes";
        public const string strfTIC_WHY_LOST_1="TIC_Why_Lost_1";
        public const string strfTIC_WHY_LOST_2="TIC_Why_Lost_2";
        public const string strfTIC_WHY_LOST_3="TIC_Why_Lost_3";
        public const string strfTIC_WHY_LOST_4 = "TIC_Why_Lost_4";
        public const string strfECOE_DATE = "ECOE_Date";


        //OPPORTUNITY TABLE
        public const string strtOPPORTUNITY = "Opportunity";
        public const string strfOPPORTUNITY_ID = "Opportunity_Id";
        public const string strfCONTACT_ID = "Contact_Id";
        public const string strfTIC_CO_BUYER_ID = "TIC_Co_Buyer_Id";
        public const string strfOWNER_ID = "Owner_Id";
        public const string strfSTATUS = "Status";
        public const string strfTYPE = "Type";
        public const string strfSALE_DATE = "Sale_Date";
        public const string strfCANCEL_REQUEST_DATE = "Cancel_Request_Date";
        public const string strfCLOSE_DATE = "Close_Date";
        public const string strfSALE_DECLINED_DATE = "Sale_Declined_Date";
        public const string strfCANCEL_DECLINED_DATE = "Cancel_Declined_Date";
        public const string strfQUOTE_DATE = "Quote_Date";
        public const string strfFIRST_VISIT_DATE = "First_Visit_Date";
        public const string strfRESERVATIONEXPIRY = "Reservation_Expiration_Date";
        public const string strfSALES_REQUEST_DATE = "Sale_Request_Date";
        public const string strfCANCEL_DATE = "Cancel_Date";
        public const string strfACTUAL_DECISION_DATE = "Actual_Decision_Date";
        public const string strfPIPELINE_STAGE = "Pipeline_Stage";
        public const string strfINACTIVE = "Inactive";
        public const string strfACCOUNT_MANAGER_ID = "Account_Manager_Id";
        public const string strfQUOTE_TOTAL = "Quote_Total";
        public const string strfELEVATION_ID = "Elevation_Id";
        public const string strfPLAN_NAME_ID = "Plan_Name_Id";
        


        // CONTACT PROFILE NEIGHBORHOOD TABLE
        public const string strfRESERVATION_DATE = "Reservation_Date";
        public const string strfCONTACT_PROFILE_NBHD_ID = "Contact_Profile_NBHD_Id";

        //PRODUCT (LOT) TABLE
        public const string strtPRODUCT = "Product";
        public const string strfPRODUCT_ID = "Product_Id";
        public const string strfRESERVATION_CONTRACT_ID = "Reservation_Contract_Id";
        public const string strfLOT_STATUS = "Lot_Status";


        //EMPLOYEE table
        public const string strtEMPLOYEE = "Employee";
        public const string strfEMPLOYEE_ID = "Employee_Id";

        //CONTACT COBUYER
        public const string strtCONTACT_COBUYER = "Contact_CoBuyer";
        public const string strfCONTACT_COBUYER_ID = "Contact_CoBuyer_Id";
        public const string strfTIC_OPPORTUNITY_ID = "TIC_Opportunity_Id";
        public const string strfTIC_PRODUCT_ID = "TIC_Product_Id";

        //FORMS
        public const string strfrmADVANTAGE_PROGRAM = "TIC_Advantage_Program";
        public const string strrHB_CONTACT_PROFILE_NBHD = "HB Contact Profile NBHD";

        //QUERIES
        public const string strqHB_INVENTORY_QUOTE_FOR_INVENTORY_HOME = "HB: Inventory quote for inventory home?";
        public const string strqCONTACT_PROFILE_NBHD_FOR_CONTACT = "HB: ContactProfileNBHD for Contact ? and NBHD ?";
        public const string strqTIC_RESERVED_QUOTE_CONTRACT_FOR_LOT = "TIC: Reserved Quote Contracts for Lot id ?";
        public const string strqTIC_COBUYERS_FOR_ADVANTAGE_PROGRAM_ID = "TIC: CoBuyers for Advantage Program Id?";

        //SERVER SCRIPTS 
        public const string strsASR_OPPORTUNITY = "TIC Opportunity";
        public const string strsASR_ADVANTAGE_PROGRAM = "TIC Advantage Program";

        //Language Strings
        public const string strgAdvantageProgram = "Advantage Program";

        //METHODS
        public const string strmADVANTAGE_PROGRAM_RESERVE = "AdvantageProgramReserve";
        public const string strmADVANTAGE_PROGRAM_SALE = "AdvantageProgramSale";

        //CHOICE STRINGS
        public const string strsRESERVED = "Reserved";
        public const string strsAVAILABLE = "Available";
        public const string strsSOLD = "Sold";
        public const string strsCLOSED = "Closed";
        public const string strsRELEASED = "Released";
        public const string strsNOT_RELEASED = "Not Released";
        public const string strsINACTIVE = "Inactive";
        public const string strsCANCELLED = "Cancelled";
        public const string strsTRANSFERRED = "Transfer";
        public const string strsROLLBACK = "RollBack";
        public const string strsIN_PROGRESS = "In Progress";
        public const string strsCANCEL_REQUEST = "Cancel Request";
        public const string strsQUOTE = "Quote";
        public const string strsCONTRACT = "Contract";

        // Error
        public const string gstrEMPTY_STRING = "";
        public const int glngERR_APPDEV_START_NUMBER = -2147221504 + 10000;
        public const int glngERR_APPDEV_END_NUMBER = -2147221504 + 13399;
        public const int glngERR_APPDEV_EXTEND_START_NUMBER = -2147221504 + 13600;
        public const int glngERR_APPDEV_EXTEND_END_NUMBER = -2147221504 + 29999;
        public const int glngERR_START_NUMBER = -2147221504 + 11200;
        public const int glngERR_END_NUMBER = glngERR_START_NUMBER + 99;
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
        public const int glngERR_CAN_NOT_DELETE = glngERR_START_NUMBER + 40;

        // Group Errors
        public const string strdERROR_ON_ADDING_NEW_RECORD = "Error on Adding New Record";
        public const string strdPARAMETERS_ARE_EXPECTED = "Parameters Are Expected";
        public const string strd_NEWFORMDATA_FAILED = "NewFormDataFailed";
        public const string strd_NEWSECONDARYDATA_FAILED = "NewSecondaryDataFailed";
        public const string strd_DELETEFORMDATA_FAILED = "DeleteFormDataFailed";
        public const string strd_ADDFORMDATA_FAILED = "AddFormDataFailed";
        public const string strd_EXECUTE_FAILED = "ExecuteFailed";
        public const string strdMETHOD_IS_NOT_DEFINED = "Method Is Not Defined";
        public const string strd_LOADFORMDATA_FAILED = "LoadFormDataFailed";
        public const string strd_SAVEFORMDATA_FAILED = "SaveFormDataFailed";
        public const string strdSETSYSTEM_FAILED = "SetSystemFailed";
        public const string strdINVALID_METHOD = "Invalid Method";
    }
}
