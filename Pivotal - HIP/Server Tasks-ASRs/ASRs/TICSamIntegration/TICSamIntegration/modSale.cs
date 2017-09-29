using System;
using System.Collections.Generic;
using System.Text;

using Pivotal.Interop.RDALib;
using Pivotal.Interop.ADODBLib;
using Pivotal.Application.Foundation.Utility;
using Pivotal.Application.Foundation.Data.Element;

namespace Pivotal.Application.TIC.SAMIntegration
{
    internal class modSale
    {
        // Tables        
        public const string strtTIC_SALE = "TIC_Sale";
        public const string strtTIC_LOT = "TIC_Lot";
        public const string strtTIC_CUSTOM_LOT_COMPLIANCE = "TIC_Custom_Lot_Compliance";
        public const string strtTIC_LOT_SALE_STATUS = "TIC_Lot_Sale_Status";
        public const string strtTIC_LOT_STATUS_HISTORY = "TIC_Lot_Status_History";
        public const string strtTIC_LOT_PRICE_HISTORY = "TIC_Lot_Price_History";
        public const string strtTIC_LOT_OWNER_HISTORY = "TIC_Lot_Owner_History";
        public const string strtCONTACT = "Contact";
        public const string strtTIC_CONTACT_VILLAGE = "TIC_Contact_Village";
        public const string strtTIC_CONTACT_VILLAGE_PROJECT = "TIC_Contact_Village_Project";
        public const string strtTIC_PROJECT = "TIC_Project";
        public const string strtTIC_TRACT = "TIC_Tract";
        public const string strtTIC_VILLAGE = "TIC_Village";
        public const string strtTIC_HOMEBUYER_SURVEY = "TIC_Homebuyer_Survey";
        public const string strtTIC_PROJECT_PHASE = "TIC_Project_Phase";
        public const string strfTIC_DATE_UNRELEASED = "TIC_Date_Unreleased";

        // Fields - TIC_Sale table
        public const string strfTIC_SALE_ID = "TIC_Sale_Id";
        public const string strfTIC_LOT_SALE_STATUS_ID = "TIC_Lot_Sale_Status_Id";
        public const string strfTIC_BUYER_1_CONTACT_ID = "TIC_Buyer_1_Contact_Id";
        public const string strfTIC_BUYER_2_CONTACT_ID = "TIC_Buyer_2_Contact_Id";
        public const string strfTIC_SALE_STATUS_LAST_CHANGE_DT = "TIC_Sale_Status_Last_Change_Dt";
        public const string strfTIC_TRACT_ID = "TIC_Tract_Id";
        public const string strfTIC_DATE_RESERVED = "TIC_Date_Reserved";
        public const string strfTIC_DATE_RESERVATION_CANCELLED = "TIC_Date_Reservation_Cancelled";
        public const string strfTIC_DATE_SOLD = "TIC_Date_Sold";
        public const string strfTIC_DATE_SALE_CANCELLED = "TIC_Date_Sale_Cancelled";
        public const string strfTIC_DATE_CLOSED = "TIC_Date_Closed";
        public const string strfTIC_BUYER_1_ORIG_ADDRESS_1 = "TIC_Buyer_1_Orig_Address_1";
        public const string strfTIC_BUYER_1_ORIG_CITY = "TIC_Buyer_1_Orig_City";
        public const string strfTIC_BUYER_1_ORIG_STATE = "TIC_Buyer_1_Orig_State";
        public const string strfTIC_BUYER_1_ORIG_ZIP = "TIC_Buyer_1_Orig_Zip";
        public const string strfTIC_BUYER_1_ORIG_COUNTRY = "TIC_Buyer_1_Orig_Country";
        public const string strfTIC_BUYER_2_ORIG_ADDRESS_1 = "TIC_Buyer_2_Orig_Address_1";
        public const string strfTIC_BUYER_2_ORIG_CITY = "TIC_Buyer_2_Orig_City";
        public const string strfTIC_BUYER_2_ORIG_STATE = "TIC_Buyer_2_Orig_State";
        public const string strfTIC_BUYER_2_ORIG_ZIP = "TIC_Buyer_2_Orig_Zip";
        public const string strfTIC_BUYER_2_ORIG_COUNTRY = "TIC_Buyer_2_Orig_Country";
        public const string strfTIC_DELETED = "TIC_Deleted";
        public const string strfTIC_CONVERSION_INDIC = "TIC_Conversion_Indic";

        // Fields - TIC_Lot table
        public const string strfTIC_LOT_ID = "TIC_Lot_Id";
        public const string strfTIC_PARENT_COMBINED_LOT_ID = "TIC_Parent_Combined_Lot_Id";
        public const string strfTIC_CUSTOM_LOT_COMPLIANCE_ID = "TIC_Custom_Lot_Compliance_Id";
        public const string strfTIC_LOT_TYPE = "TIC_Lot_Type";
        public const string strfTIC_STREET_ADDRESS = "TIC_Street_Address";
        public const string strfTIC_CITY = "TIC_City";
        public const string strfTIC_STATE = "TIC_State";
        public const string strfTIC_ZIP = "TIC_Zip";
        //public const string strfTIC_DATE_RESERVED = "TIC_Date_Reserved";
        //public const string strfTIC_DATE_RESERVATION_CANCELLED = "TIC_Date_Reservation_Cancelled";
        //public const string strfTIC_DATE_SOLD = "TIC_Date_Sold";
        //public const string strfTIC_DATE_SALE_CANCELLED = "TIC_Date_Sale_Cancelled";
        //public const string strfTIC_DATE_CLOSED = "TIC_Date_Closed";
        public const string strfTIC_PROJECT_PHASE_ID = "TIC_Project_Phase_Id";
        public const string strfTIC_DATE_RELEASED = "TIC_Date_Released";
        public const string strfTIC_PROJECT_PLAN_ID = "TIC_Project_Plan_Id";

        // Fields - TIC_Lot_Sale_Status table
        //public const string strfTIC_LOT_SALE_STATUS_ID = "TIC_Lot_Sale_Status_Id";
        public const string strfTIC_STATUS_DESCRIPTION = "TIC_Status_Description";

        // Fields - TIC_Lot_Status_History table
        public const string strfTIC_LOT_STATUS_HISTORY_ID = "TIC_Lot_Status_History_Id";
        //public const string strfTIC_LOT_ID  = "TIC_Lot_Id";
        public const string strfTIC_CHANGE_NUMBER_ORDINAL = "TIC_Change_Number_Ordinal";
        public const string strfTIC_CHANGED_BY_EMPLOYEE_ID = "TIC_Changed_By_Employee_Id";
        //public const string strfTIC_LOT_SALE_STATUS_ID = "TIC_Lot_Sale_Status_Id";
        public const string strfTIC_COMMENTS = "TIC_Comments";
        public const string strfTIC_DATE_BUSINESS_TRANSACTION = "TIC_Date_Business_Transaction";
        //public const string strfTIC_SALE_ID = "TIC_Sale_Id";
        public const string strfTIC_ROLLBACK_INDIC = "TIC_Rollback_Indic";
        public const string strfTIC_ROLLBACK_DATE = "TIC_Rollback_Date";
        public const string strfTIC_ROLLBACK_EMPLOYEE_ID = "TIC_Rollback_Employee_Id";
        public const string strfTIC_CHANGED_BY_CONTACT_ID = "TIC_Changed_By_Contact_Id";

        // Fields - TIC_Contact_Village
        public const string strfTIC_CONTACT_VILLAGE_ID = "TIC_Contact_Village_Id";
        public const string strfTIC_CONTACT_ID = "TIC_Contact_Id";
        public const string strfTIC_VILLAGE_ID = "TIC_Village_Id";
        public const string strfTIC_CONTACT_TYPE = "TIC_Contact_Type";
        public const string strfTIC_OPTED_OUT = "TIC_Opted_Out";

        // Fields - TIC_Contact_Village_Project
        public const string strfTIC_CONTACT_VILLAGE_PROJECT_ID = "TIC_Contact_Village_Project_Id";
        //public const string strfTIC_CONTACT_ID = "TIC_Contact_Id";
        public const string strfTIC_PROJECT_ID = "TIC_Project_Id";
        //public const string strfTIC_CONTACT_VILLAGE_ID = "TIC_Contact_Village_Id";
        //public const string strfTIC_CONTACT_TYPE = "TIC_Contact_Type";

        // Fields - Contact
        public const string strfCONTACT_ID = "Contact_Id";
        public const string strfTYPE = "Type";
        public const string strfTIC_RANK = "TIC_Rank";
        public const string strfTIC_M1_CON_VILLAGE_LIST_OUT = "TIC_M1_Con_Village_List_Out";
        public const string strfTIC_M1_CON_PROJECT_LIST_OUT = "TIC_M1_Con_Project_List_Out";
        public const string strfADDRESS_1 = "Address_1";
        public const string strfADDRESS_2 = "Address_2";
        public const string strfCITY = "City";
        public const string strfSTATE = "State_";
        public const string strfZIP = "Zip";
        public const string strfTIC_CANCELLED_PROJECT_NAME = "TIC_Cancelled_Project_Name";
        public const string strfTIC_CANCELLED_PURCHASE_DATE = "TIC_Cancelled_Purchase_Date";
        public const string strfTIC_CANCELLED_CANCEL_DATE = "TIC_Cancelled_Cancel_Date";
        public const string strfCOUNTRY = "Country";

        // Fields - TIC_Village
        //public const string strfTIC_VILLAGE_ID = "TIC_Village_Id";
        public const string strfTIC_M1_UNIQUE_IDENTIFIER = "TIC_M1_Unique_Identifier";

        // Fields - TIC_Project
        //public const string strfTIC_PROJECT_ID = "TIC_Project_Id";
        //public const string strfTIC_VILLAGE_ID = "TIC_Village_Id";
        //public const string strfTIC_M1_UNIQUE_IDENTIFIER = "TIC_M1_Unique_Identifier";

        // Fields - TIC_Homebuyer_Survey
        public const string strfTIC_HOMEBUYER_SURVEY_ID = "TIC_Homebuyer_Survey_Id";
        public const string strfTIC_SURVEY_STATUS = "TIC_Survey_Status";

        //Active Forms
        public const string strrTIC_LOT = "TIC_Lot";

        //Queries
        public const string strqTIC_LOTS_WITH_PARENT_COMBINED_LOT = "TIC: Lots with Parent Combined Lot ?";
        public const string strqTIC_LOT_STATUS_HISTORY_WITH_LOT = "TIC: Lot Status History with Lot ?";
        public const string strqTIC_CONTACT_VILLAGE_CONTACT_VILLAGE = "TIC: Contact Village with Contact ? Village ?";
        public const string strqTIC_CVP_CONTACT_VILLAGE_PROJECT = "TIC: CVP with Contact ? Village ? Project ?";
        public const string strqTIC_NON_CANCELLED_SALES_FOR_NOT_SALE_CONTACT = "TIC: Non-Cancelled Sales for Sale <> ? Contact ? ?";
        public const string strqTIC_NON_CANCELLED_SALES_FOR_NOT_SALE_CONTACT_PROJECT = "TIC: Non-Cancelled Sales for Sale <> ? Contact ? ? Project ?";
        public const string strqTIC_NON_CANCELLED_SALES_FOR_NOT_SALE_CONTACT_VILLAGE = "TIC: Non-Cancelled Sales for Sale <> ? Contact ? ? Village ?";
        public const string strqTIC_OPTED_IN_CONTACT_VILLAGES_FOR_CONTACT = "TIC: Opted-In Contact Villages for Contact ?";
        public const string strqTIC_CONTACT_VILLAGE_PROJECTS_FOR_CONTACT = "TIC: Contact Village Projects for Contact ?";
        public const string strqTIC_OPTED_IN_CONTACT_VILLAGE_PROJECTS_FOR_CONTACT = "TIC: Opted-In Contact Village Projects for Contact ?";
        public const string strqTIC_LSH_WITH_LOT_ORDINAL_NOT_ROLLED_BACK = "TIC: LSH with Lot ? Ordinal > ? Rolled-Back = No";
        public const string strqTIC_LSH_WITH_SALE_ORDINAL_LESS_THAN_NOT_ROLLED_BACK = "TIC: LSH with Sale ? Ordinal < ? Rolled-Back = No";
        public const string strqTIC_LSH_WITH_SALE_ORDINAL_GREATER_THAN_NOT_ROLLED_BACK = "TIC: LSH with Sale ? Ordinal > ? Rolled-Back = No";
        public const string strqTIC_LSH_WITH_SALE_NOT_ROLLED_BACK = "TIC: LSH with Sale ? Rolled-Back = No";
        public const string strqTIC_LSH_WITH_LOT_ORDINAL_LESS_THAN_NOT_ROLLED_BACK = "TIC: LSH with Lot ? Ordinal < ? Rolled-Back = No";
        public const string strqTIC_NON_DELETED_SALES_WITH_PROJECT_BUYER_1_OR_2 = "TIC: Non-Deleted Sales with Project ? Buyer 1 ? Or Buyer 2 ?";
        public const string strqTIC_PROJECT_WITH_CONTACT_VILLAGE = "TIC: Project with Contact Village ?";
        public const string strqTIC_RESERVED_SOLD_CLOSED_SALES_WITH_LOT = "TIC: Reserved, Sold or Closed Sales with Lot ?";

        // ASR Methods - TIC
        public const string strmCALCULATE_PARENT_LOT_STATUS = "CalculateParentLotStatus";
        public const string strmSTATUS_HISTORY_CHANGE_ORDINAL_IS_MOST_RECENT = "StatusHistoryChangeOrdinalIsMostRecent";
        public const string strmSTATUS_HISTORY_EXISTS_FOR_SALE_WITH_ORDINAL_LESS_THAN = "StatusHistoryExistsForSaleWithOrdinalLessThan";
        public const string strmROLLBACK_STATUS = "RollbackStatus";
        public const string strmDELETE_SALE = "DeleteSale";
        public const string strmCREATE_LOT_STATUS_HISTORY_RECORD = "CreateLotStatusHistoryRecord";
        public const string strmMANAGE_EPARTNER_PRE_SALE_RECORD_SAVE = "ManageEpartnerPreSaleRecordSave";
        public const string strmMANAGE_EPARTNER_POST_SALE_RECORD_SAVE = "ManageEpartnerPostSaleRecordSave";

        // Lot/Sale Status Descriptions - corresponds to TIC_Lot_Sale_Status.TIC_Status_Description
        public const string LOT_SALE_STATUS_DESCRIPTION_NOT_RELEASED = "Not Released";
        public const string LOT_SALE_STATUS_DESCRIPTION_RELEASED = "Released";
        public const string LOT_SALE_STATUS_DESCRIPTION_CANCELLED = "Cancelled";
        public const string LOT_SALE_STATUS_DESCRIPTION_RESERVED = "Reserved";
        public const string LOT_SALE_STATUS_DESCRIPTION_SOLD = "Sold";
        public const string LOT_SALE_STATUS_DESCRIPTION_CLOSED = "Closed";
        public const string LOT_SALE_STATUS_DESCRIPTION_CANCELLED_RESERVE = "Cancelled Reserve";
        public const string LOT_SALE_STATUS_DESCRIPTION_CONVERSION = "Conversion";

        // Lot Types - TIC_Lot.TIC_Lot_Type field
        public const string LOT_TYPE_BUILDER_LOT = "Builder Lot";
        public const string LOT_TYPE_CUSTOM_LOT = "Custom Lot";
        public const string LOT_TYPE_CUSTOM_COMBINED_LOT = "Custom Combined Lot";

        // Contact Types
        public const string CONTACT_TYPE_PROSPECT = "Prospect";
        public const string CONTACT_TYPE_CUSTOMER = "Customer";

        // TIC_Contact_Village and TIC_Contact_Village_Project types
        public const string CONTACT_VILLAGE_TYPE_PROSPECT = "Prospect";
        public const string CONTACT_VILLAGE_TYPE_BUYER = "Buyer";
        public const string CONTACT_VILLAGE_TYPE_HOMEOWNER = "Homeowner";

        // TIC_Ranks User Choice List
        public const string CONTACT_RANK_A = "A";
        public const string CONTACT_RANK_B = "B";
        public const string CONTACT_RANK_C = "C";
        public const string CONTACT_RANK_D = "D";
        public const string CONTACT_RANK_X = "X";
        public const string CONTACT_RANK_U = "U";

        // UCL: TIC_Survey Statuses
        public const string SURVEY_STATUS_NOT_SUBMITTED = "Not Submitted";
        public const string SURVEY_STATUS_SUBMITTED = "Submitted";
        public const string SURVEY_STATUS_REJECTED = "Rejected";
        public const string SURVEY_STATUS_APPROVED = "Approved";
        public const string SURVEY_STATUS_CANCELLED_SALE = "Cancelled Sale";

        // Script Name
        public const string strs_SYSTEM = "System";
        public const string strs_ERRORS = "Errors";
        public const string strs_FUNCTIONLIB = "Function Lib";
        public const string strs_TRANSIT_POINT_PARAMS = "Transit Point Params";
        public const string strsCORE_TRANSIT_POINT_PARAM = "Core Transit Point Param";
        public const string strsCORE_DL_FUNCTIONLIB = "Core DL Function Lib";

        // ASR Methods
        public const string strm_EXECUTE = "Execute";
        public const string strmSET_SYSTEM = "SetSystem";
        public const string strm_ADD_FORM_DATA = "AddFormData";
        public const string strm_DELETE_FORM_DATA = "DeleteFormData";
        public const string strm_LOAD_FORM_DATA = "LoadFormData";
        public const string strm_NEW_FORM_DATA = "NewFormData";
        public const string strm_SAVE_FORM_DATA = "SaveFormData";
        public const string strm_NEW_SECONDARY_DATA = "NewSecondaryData";

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
