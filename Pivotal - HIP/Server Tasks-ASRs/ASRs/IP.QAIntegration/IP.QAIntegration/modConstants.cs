using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IP.QAIntegration
{
    public static class modConstants
    {
        public const string strfINSPECTION_ID = "Inspection_Id";
        public const string strfTIC_INSPECTION_TYPE_ID = "TIC_Inspection_Type_Id";
        public const string strcTEMPLATE_NOT_FOUND = "Inspection Template not found for : ";
        public const string strfINSPECTION_TEMPLATE_ID = "Inspection_Template_Id";
        public const string strfTIC_SUPERVISOR_ID = "TIC_Supervisor_Id";
        public const string strfSTATUS = "TIC_Inspection_Status";
        public const string strfDATE_COMPLETE = "Date_Complete";
        public const string strfINSPECTION_NAME = "Inspection_Name";
        public const string strfTIC_CORRECTIVE_ACTION_REQUIRED = "TIC_Corrective_Action_Required";
        public const string strfTIC_CORRECTIVE_ACTION_STATUS = "TIC_Corrective_Action_Status";
        public const string strfINSPECTOR_ID = "TIC_Inspector_ID";
        public const string strfCREATED_BY_ID = "TIC_Created_By_Id";
        public const string strfLAST_SAVED_BY_ID = "TIC_Last_Saved_By";


        public const string strtTIC_CONSTRUCTION_PROJECT = "TIC_Construction_Project";
        public const string strtINSPECTION_TEMPLATE = "Inspection_Template";
        public const string strtTIC_INSP_CATEGORY = "TIC_Insp_Category";
        public const string strtINSPECTION_STEP = "Inspection_Step";
        public const string strtTIC_QA_DOCUMENTS = "TIC_QA_Documents";
        public const string strtTIC_QA_WEBLINKS = "TIC_QA_Weblinks";
        public const string strtTIC_INSPECTED_SCOPE_ITEMS = "TIC_Inspected_Scope_Items";
        public const string strtTIC_QA_NOTIFICATION_TEMPLATE = "TIC_QA_Notification_Template";
        public const string strtTIC_QA_NOTIFICATION_TEAM = "TIC_QA_Notification_Team";
        public const string strtINSPECTION = "Inspection";
        public const string strtCONTACT = "Contact";
        public const string strtTIC_INSPECTION_TYPE = "TIC_Construction_Date_Lookup";

        public const string strfBUILDER_ID = "Builder_Id";
        public const string strfINSPECTION_TEMPLATE_NAME = "Inspection_Template_Name";

        public const string strfTIC_CATEGORY_TEMPLATE = "TIC_Insp_Category_Template";
        public const string strfCATEGORY_DESC = "Category_Desc";
        public const string strfORDINAL = "Ordinal";
        public const string strfTIC_INSP_CATEGORY_TEMPLATE_ID = "TIC_Insp_Category_Template_Id";
        public const string strfTIC_INSP_CATEGORY_ID = "TIC_Insp_Category_Id";



        public const string strqTIC_INT_INSP_TEMPLATE_BY_TYPE = "TIC INT : Inspection Temp By Type ?";
        public const string strqTIC_INT_CATEGORY_TEMPLATE_BY_TEMPLATE = "TIC INT : Category Template by Template ?";

        public const string strfTIC_CONSTRUCTION_PROJECT_ID = "TIC_Construction_Project_Id";
        public const string strfTIC_CONSTRUCTION_PROJECT_NAME = "TIC_Construction_Project_Name";
        public const string strfTIC_CONSTRUCTION_PHASE = "TIC_Construction_Phase";

        public const string strfTIC_ORDINAL = "TIC_Ordinal";
        public const string strfDESCRIPTION = "Description";
        public const string strfINSPECTION_STEP_ID = "Inspection_Step_Id";
        public const string strfTIC_SCOPE = "TIC_Scope";

        public const string strfCONSTRUCTION_PROJECT_ID = "Construction_Project_Id";
        public const string strfPHASE_NBR = "Phase_Nbr";
        public const string strfINSPECTION_TYPE_ID = "Inspection_Type_Id";
        public const string strfSCOPE_ITEM_NBR = "Scope_Item_Nbr";

        public const string strqTIC_INT_TRACT_TIME_BY_BUILDING = "TIC INT : Tract Time By Building ?";
        public const string strqTIC_INT_TRACT_TIME_BY_LOT = "TIC INT : Tract Time By Lot ?";
        public const string strqTIC_INT_TRACT_TIME_BY_UNIT = "TIC INT : Tract Time By Unit ?";

        public const string strfINSPECTION_STATUS = "Inspection_Status";

        public const string strtTIC_LOT_CONSTRUCTION_DATE = "TIC_Lot_Construction_Date";
        public const string strfCP_SUPERINTENDENT_ID = "Superintendent_Id";
        public const string strfTIC_SCOPE_ITEMS = "TIC_Scope_Items";

        //Template Tags
        public const string strtagTIC_CONSTRUCTION_PROJECT_NAME = "[TIC_Construction_Project_Name]";
        public const string strtagPHASE = "[Phase]";
        public const string strtagTIC_INSPECTION_TYPE = "[TIC_Inspection_Type]";
        public const string strtagINSPECTED_SCOPE_ITEM = "[Inspected_Scope_Items]";
        public const string strtagQA_ADMIN_LIST = "[QA_Admin_List]";
        public const string strtagDATE_OF_INSPECTION = "[Date_Of_Inspection]";
        public const string strtagINSPECTION_SCOPE = "[Inspection_Scope]";
        public const string strtagINSPECTOR = "[Inspector]";
        public const string strtagSUPER = "[Super]";
        public const string strtagQA_LINK = "[Inspection_Link]";

        public const string strqTIC_QA_TEMPLATE_LOOKUP = "TIC QA : Template Lookup ?";
        public const string strdiscQA_ACTION = "QA_Action";
        public const string strsegPRIMARY_SEGMENT = "New Segment";

        //Inspection Statuses
        public const string strcDATA_ENTRY = "Data Entry";
        public const string strcAWAITING_APPROVAL = "Awaiting Approval";
        public const string strcAPPROVING = "Approving";
        public const string strcAPPROVED = "Approved";
        public const string strcAWAITING_FOLLOW_UP = "Awaiting Follow-Up";
        public const string strcFOLLOW_UP_DATA_ENTRY = "Follow-up Data Entry";
        public const string strcAWAITING_FOLLOW_UP_APPROVAL = "Awaiting Follow-up Approval";
        public const string strcAPPROVING_FOLLOW_UP = "Approving Follow-up";
        public const string strcFOLLOW_UP_APPROVED = "Follow-up Approved";
        public const string strcIN_PROCESS = "In Process";
        public const string strcFOLLOW_UP_IN_PROCESS = "Follow-up In Process";

        //Notification Template fields
        public const string strfSUBJECT = "Subject";
        public const string strfBODY = "Body";
        public const string strfSEND_TO_CONSULTANT = "Send_To_Consultant";
        public const string strfSEND_TO_SUPER = "Send_To_Super";
        public const string strfSEND_TO_ADMIN = "Send_To_Admin";
        public const string strfTIC_QA_NOTIFICATION_TMP_ID = "TIC_QA_Notification_Tmp_Id";
        public const string strfLINK_TEMPLATE = "Inspection_Link_Template";
        
        //Notification template admin fields
        public const string strfCONTACT_ID = "Contact_Id";
        public const string strfINACTIVE = "Inactive";
        public const string strfADMIN_EMAIL = "Admin_Email";

        //Contact fields
        public const string strfEMAIL = "Email";
        public const string strfCOMPANY_NAME = "Company_Name";
        public const string strfPHONE = "Phone";
        public const string strfFIRST_NAME = "First_Name";
        public const string strfLAST_NAME = "Last_Name";
        public const string strfCONTACT_ROLE = "Job_Title";

        public const string strfTIC_INSPECTION_TYPE_DESC = "Date_Description";

        //Exceptions
        public const string strexINSPECTION_ALREADY_CREATED_BUILDING = "An inspection has already been created for the following building : ";
        public const string strexINSPECTION_ALREADY_CREATED_LOT = "An inspection has already been created for the following lot : ";
        public const string strexINSPECTION_ALREADY_CREATED_UNIT = "An inspection has already been created for the following unit: ";

    }
}
