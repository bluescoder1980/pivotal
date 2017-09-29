using System;
using System.Collections.Generic;
using System.Text;

namespace CRM.Pivotal.IAC
{
    internal class modOpportunity
    {
        #region Error
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
        #endregion

        #region Group Errors
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
        #endregion

        #region Tables
        public const string strtOPPORTUNITY = "Opportunity";
        #endregion

        #region Fields
        public const string strfOPPORTUNITY_ID = "Opportunity_Id";
        public const string strfCONTACT_ID = "Contact_Id";
        public const string strfSTATUS = "Status";
        public const string strfACCOUNT_MANAGER_ID = "Account_Manager_Id";
        #endregion
    }
}
