using System;
using System.Collections.Generic;
using System.Text;

namespace CRM.Pivotal.IAC
{
    internal class modBrochure
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

        #region Queries
        public const string strqPROPERTY_CONTACTPROPERTY_MPEB = "IAC: Properties in Contact Property w/o MPEB for Contact ?";
        public const string strqIAC_INPROGRESS_OPPORTUNITY = "IAC: In Progress Opportunity for Contact ?";
        public const string strqIAC_INCLUDED_SEARCHITEMS = "IAC: Included Contact Search Items for Contact ?";
        public const string strqIAC_SEARCH_PROPERTIES = "IAC: Search Properties for Contact ?";
        public const string strqIAC_SEARCH_SCHOOLS = "IAC: Search Schools for Contact Id ?";
        public const string strqIAC_SEARCH_REGIONS = "IAC: Search Regions for Contact ?";
        public const string strqSYS_EMPLOYEE_USERID = "Sys: Employee with User ?";
        public const string strqIAC_PROPERTY_OR_PARENT = "IAC: Property Id = ? or Parent Property Id = ?";
        public const string strqIAC_MPEB_GUESTCARD = "IAC: MPEB Guest Card for Property ? ? and Contact ?";
        public const string strqIAC_FOCAL_CONTACTS = "IAC: Focal Contacts of Contact ?";
        public const string strqIAC_CONTACT_UNITS = "IAC: Contact Unit w/o MPEB for Floorplan ? for Contact ?";
        public const string strqIAC_CONTACT_PROPERTIES = "IAC: Contact Property for Property ?? for Contact ?";
        public const string strqIAC_FLOORPLAN_CONTACTFLOORPLANS_MPEB = "IAC: Property Floorplans of Contact Floorplans w/o MPEB Property ? Parent ? Contact ?";
        public const string strqIAC_UNIT_CONTACTUNITS_MPEB = "IAC: Property Units of Contact Units w/o MPEB for Floorplan ? for Contact ?";
        public const string strqIAC_CONTACT_FLOORPLANS = "IAC: Contact Floorplan w/o MPEB for Floorplan ? for Contact ?";
        public const string strqIAC_INACTIVE_CONTACTGUESTCARD_WITH_NUMBER = "IAC: Inactive GuestCard with GuestCard Number ? for Property ?";
        #endregion

        #region Methods
        public const string strmGENERATE_GUESTCARDS = "Generate GuestCards";
        #endregion

        #region Tables
        public const string strtIAC_BROCHURE = "IAC_Brochure";
        public const string strtIAC_BROCHURE_PROPERTY = "IAC_Brochure_Property";
        public const string strtIAC_PROPERTY = "IAC_Property";
        public const string strtOPPORTUNITY = "Opportunity";
        public const string strtCONTACT = "Contact";
        public const string strtIAC_CONTACT_PROPERTY = "IAC_Contact_Property";
        public const string strtIAC_CONTACT_FLOORPLAN = "IAC_Contact_Floorplan";
        public const string strtIAC_CONTACT_UNIT = "IAC_Contact_Unit";
        public const string strtIAC_CONTACT_GUESTCARD = "IAC_Contact_GuestCard";
        #endregion

        #region Segments
        public const string strsCOMMUNITY_INFORMATION = "Community Information";
        public const string strsREQUIREMENTS = "Requirements";
        #endregion

        #region Fields
        public const string strfIAC_STARTING_ADDRESS = "IAC_Starting_Address";
        public const string strfIAC_PROPERTY_ID = "IAC_Property_Id";
        public const string strfIAC_SEQUENCE = "IAC_Sequence";
        public const string strfIAC_CONTACT_ID = "IAC_Contact_Id";
        public const string strfRN_DESCRIPTOR = "Rn_Descriptor";
        public const string strfIAC_REQUIREMENT = "IAC_Requirement";
        public const string strfIAC_CATEGORY = "IAC_Category";
        public const string strfIAC_FORMULA_CATEGORY = "IAC_formula_Category_Name";
        public const string strfIAC_FORMULA_SORTORDER = "IAC_formula_SortOrder";
        public const string strfIAC_FORMULA_SEARCHITEM = "IAC_formula_Search_Item_Desc";
        public const string strfOPPORTUNITY_ID = "Opportunity_Id";
        public const string strfIAC_OPPORTUNITY_ID = "IAC_Opportunity_Id";
        public const string strfCONTACT_ID = "Contact_Id";
        public const string strfSTATUS = "Status";
        public const string strfACCOUNT_MANAGER_ID = "Account_Manager_Id";
        public const string strfEMPLOYEE_ID = "Employee_Id";
        public const string strfFIRST_NAME = "First_Name";
        public const string strfLAST_NAME = "Last_Name";
        public const string strfWORK_EMAIL = "Work_Email";
        public const string strfWORK_PHONE = "Work_Phone";
        public const string strfEMAIL = "Email";
        public const string strfIAC_FA_CAT = "IAC_FA_Cat";
        public const string strfIAC_FA_DOG = "IAC_FA_Dog";
        public const string strfIAC_FA_DOGWEIGHT = "IAC_FA_Dog_Weight";
        public const string strfCOMMENTS = "Comments";
        public const string strfIAC_NOTES = "IAC_Notes";
        public const string strfIAC_VAULTWARE_ID = "IAC_VaultWare_ID";
        public const string strfIAC_OUTGOING_PMS_ID = "IAC_Outgoing_PMS_ID";
        public const string strfIAC_PROPERTY_NAME = "IAC_Property_Name";
        public const string strfIAC_FORMULA_PROPERTY_NAME = "IAC_formula_Property_Name";
        public const string strfIAC_FORMULA_PMS_ID = "IAC_formula_PMS_ID";
        public const string strfIAC_GUESTCARD_NUMBER = "IAC_GuestCard_Number";
        public const string strfIAC_UNITTYPE = "IAC_UnitType";
        public const string strfIAC_FLOORPLAN_NAME = "IAC_Floorplan_Name";
        public const string strfIAC_FORMULA_PMS_PROPERTY_ID = "IAC_formula_PMS_PropertyID";
        public const string strfIAC_PROPERTY_FLOORPLAN_ID = "IAC_Property_Floorplan_Id";
        public const string strfIAC_CURRENTUSE_SF = "IAC_CurrUseSqFt";
        public const string strfIAC_MARKETRENT = "IAC_MarketRent";
        public const string strfIAC_UNITID = "IAC_UnitId";
        public const string strfIAC_BLDGID = "IAC_BldgId";
        public const string strfIAC_MPEB = "IAC_MPEB";
        public const string strfIAC_BROCHURE_ID = "IAC_Brochure_Id";
        public const string strfIAC_ACTIVE = "IAC_Active";
        public const string strfIAC_BROCHURE_GUID = "IAC_Brochure_GUID";
        public const string strfIAC_COMMENTS = "IAC_Comments";
        public const string strfIAC_BROCHURE_XML = "IAC_Brochure_XML";
        #endregion

        #region Stored Procedures
        public const string strxIAC_CONTACT_GUESTCARD_INFO = "xSP_IAC_Contact_GuestCard_Information";
        public const string strxPMS_GET_GUESTCARD = "xSP_CreateINetGuestCard";
        #endregion


        #region Other
        public const string strCATEGORY_REGION = "Market";
        public const string strCATEGORY_PROPERTY = "Property";
        public const string strCATEGORY_SCHOOL = "School";
        public const string strCATEGORY_PET = "Pet";
        public const string strPET_CAT = "Cat";
        public const string strPET_DOG = "Dog";
        #endregion
    }
}
