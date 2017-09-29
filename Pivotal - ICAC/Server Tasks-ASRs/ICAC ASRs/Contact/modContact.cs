using System;
using System.Collections.Generic;
using System.Text;

namespace CRM.Pivotal.IAC
{
    internal class modContact
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
        public const string strtCONTACT = "Contact";
        public const string strtIAC_CONTACT_SEARCH_FP = "IAC_Contact_Search_FP";
        public const string strtIAC_CONTACT_SEARCH_U = "IAC_Contact_Search_U";
        public const string strtIAC_CONTACT_PROPERTY = "IAC_Contact_Property";
        public const string strtIAC_CONTACT_FLOORPLAN = "IAC_Contact_Floorplan";
        public const string strtIAC_CONTACT_UNIT = "IAC_Contact_Unit";
        public const string strtACTIVITY = "Rn_Appointments";
        #endregion

        #region Methods
        public const string strmCHECK_FOR_DUPE = "IAC_Check_For_Dup";
        public const string strmCHECK_FOR_DUPE_FIELDCHANGE = "IAC_Check_For_Dup_FieldChange";
        public const string strmCONTACT_SEARCH = "Contact Search";
        public const string strmZIP_LOOKUP = "Zip Lookup";
        public const string strmSAVE_RECOMMENDATIONS = "Save Recommendations";
        public const string strmFOUND_CONTACTPROPERTY_MPEB = "FoundContactPropertyMPEB";
        #endregion

        #region Queries
        public const string strqIAC_CONTACT_DUPESEARCH = "IAC: Contact Duplicate Search";
        public const string strqIAC_CONTACT_DUPESEARCH_FIELDCHANGE = "IAC: Contact Duplicate Search Field Change";
        public const string strqIAC_MISSING_SEARCH_GRID_ITEMS = "IAC: Search Grid Items for Category ? Not in Contact ?";
        public const string strqIAC_SEARCH_GRID_ITEMS = "IAC: Search Grid Items for Category ?";
        public const string strqIAC_CONTACT_SEARCH_FP = "IAC: Contact Search FP for Contact ?";
        public const string strqIAC_CONTACT_SEARCH_U = "IAC: Contact Search U for Contact ?";
        public const string strqIAC_LOOKUP_ZIP = "IAC: Lookup Zip for Zip ?";
        public const string strqIAC_RECOMMENDED_FLOORPLANS = "IAC: Contact Search FP for Contact ? Property ?";
        public const string strqIAC_PROPERTIES_OF_CSF = "IAC: Properties recommended for Contact ?";
        public const string strqIAC_PROPERTY_FLOORPLAN = "IAC: Property Floorplan with Id ?";
        public const string strqIAC_UNITS_OF_CSF = "IAC: Units recommended for CSF ?";
        public const string strqIAC_CONTACT_PROPERTY_WO_MPEB = "IAC: Contact Property w/o MPEB for Contact ?";
        public const string strqIAC_DAILY_TRAFFIC_ACTIVITY = "IAC: Activity by Employee ? of Type ? for Contact ? done Today";
        public const string strqIAC_LAST_ACTIVITY = "IAC: Last Activity for Contact ?";
        public const string strqIAC_OC_REGIONS = "IAC: Orange County Regions";
        public const string strqIAC_LAST_TRAFFIC_FOR_CONTACT = "IAC: Last visit traffic record for Contact ?";
        public const string strqIAC_CONTACTPROPERTY_WITHOUTMPEB = "IAC: Contact Property for Property ?? for Contact ?";
        public const string strqIAC_CONTACTFLOORPLAN_WITHOUTMPEB = "IAC: Contact Floorplan w/o MPEB for Contact Property ? Floorplan ? for Contact ?";
        public const string strqIAC_CONTACTUNIT_WITHOUTMPEB = "IAC: Contact Unit w/o MPEB for Contact Floorplan ? for Unit ? for Contact ?";
        #endregion

        #region Stored Procedures
        public const string strxIAC_CONTACT_SEARCH_FP = "xSP_IAC_Contact_Floorplan_Search";
        public const string strxIAC_CONTACT_SEARCH_U = "xSP_IAC_Contact_Unit_Search";
        #endregion

        #region Segments
        public const string strsSI_BEDROOMS = "SI:Bedrooms";
        public const string strsSI_LOCATION = "SI:Location";
        public const string strsSI_FLOOR = "SI:Floor";
        public const string strsSI_OTHER = "SI:Other";
        public const string strsSI_PARKING = "SI:Parking";
        public const string strsSI_PROPERTY = "SI:Property";
        public const string strsSI_TYPE = "SI:Type";
        public const string strsSI_VIEW = "SI:View";
        public const string strsSEARCH_REGION = "Search Region";
        #endregion

        #region Fields
        public const string strfCONTACT_ID = "Contact_Id";
        public const string strfIAC_SRCH_CATEGORY_ID = "IAC_Search_Category_Id";
        public const string strfIAC_SRCH_GRID_ITEM_ID = "IAC_Search_Grid_Template_Id";
        public const string strfIAC_CONTACT_ID = "IAC_Contact_Id";
        public const string strfIAC_INCLUDE = "IAC_Include";
        public const string strfIAC_PROPERTY_ID = "IAC_Property_Id";
        public const string strfIAC_PROPERTY_FLOORPLAN_ID = "IAC_Property_Floorplan_Id";
        public const string strfIAC_RECOMMEND = "IAC_Recommend";
        public const string strfIAC_AVAILABLE_UNITS = "IAC_Available_Units";
        public const string strfIAC_SPECIALS = "IAC_Specials";
        public const string strfAVAILABLE_UNITS = "Available_Units";
        public const string strfSPECIALS = "Specials";
        public const string strfIAC_SPECIALS_FLOORPLAN_ID = "IAC_Specials_Floorplan_Id";
        public const string strfIAC_PROPERTY_NAME = "IAC_Property_Name";
        public const string strfIAC_FLOORPLAN_NAME = "IAC_Floorplan_Name";
        public const string strfIAC_UNITTYPE = "IAC_UnitType";
        public const string strfIAC_BEDROOMS = "IAC_Bedrooms";
        public const string strfIAC_BATHROOMS = "IAC_Bathrooms";
        public const string strfIAC_SQUAREFEET_MIN = "IAC_SquareFeet_Min";
        public const string strfIAC_SQUAREFEET_MAX = "IAC_SquareFeet_Max";
        public const string strfIAC_MARKETRENT_MIN = "IAC_MarketRent_Min";
        public const string strfIAC_MARKETRENT_MAX = "IAC_MarketRent_Max";
        public const string strfIAC_SECURITY_DEPOSIT_MIN = "IAC_Security_Deposit_Min";
        public const string strfIAC_PROPERTY_UNIT_ID = "IAC_Property_Unit_Id";
        public const string strfIAC_CONTACT_SEARCH_FP_ID = "IAC_Contact_Search_FP_Id";
        public const string strfIAC_BLDGID = "IAC_BldgId";
        public const string strfIAC_UNITID = "IAC_UnitId";
        public const string strfIAC_CURRUSESQFT = "IAC_CurrUseSqFt";
        public const string strfIAC_MARKETRENT = "IAC_MarketRent";
        public const string strfIAC_OCCUSTATUS = "IAC_OccuStatus";
        public const string strfIAC_UNITCLASS = "IAC_UnitClass";
        public const string strfIAC_MARKETED_UNITSTATUS = "IAC_Marketed_UnitStatus";
        public const string strfIAC_MARKETED_VACANCYCLASS = "IAC_Marketed_VacancyClass";
        public const string strfIAC_MARKETED_VACATEDATE = "IAC_Marketed_VacateDate";
        public const string strfIAC_CAT = "IAC_Cat";
        public const string strfIAC_DOG = "IAC_Dog";
        public const string strfIAC_CITY = "IAC_City";
        public const string strfIAC_STATE_CODE = "IAC_State_Code";
        public const string strfIAC_SF = "IAC_SF";
        public const string strfIAC_MPEB = "IAC_MPEB";
        public const string strfIAC_REFERRAL_DATE = "IAC_Referral_Date";
        public const string strfIAC_ACTIVITY_ID = "IAC_Activity_Id";
        public const string strfRN_APPOINTMENTS_ID = "Rn_Appointments_Id";
        public const string strfIAC_MARKET_RENT = "IAC_Market_Rent";
        public const string strfIAC_CONTACT_PROPERTY_ID = "IAC_Contact_Property_Id";
        public const string strfIAC_CONTACT_FLOORPLAN_ID = "IAC_Contact_Floorplan_Id";
        public const string strfIAC_FORMULA_SORTORDER = "IAC_formula_SortOrder";
        public const string strfIAC_FLOOR = "IAC_Floor";
        public const string strfIAC_CURRENT_VISIT_TYPE = "IAC_Current_Visit_Type";
        public const string strfCONTACT = "Contact";
        public const string strfASSIGNED_BY = "Assigned_By";
        public const string strfASSIGNED_TO = "Rn_Employee_Id";
        public const string strfACCESS_TYPE = "Access_Type";
        public const string strfPRIORITY = "Appt_Priority";
        public const string strfACTIVITY_TYPE = "Activity_Type";
        public const string strfAPPT_DATE = "Appt_Date";
        public const string strfSTART_TIME = "Start_Time";
        public const string strfAPPT_DESCRIPTION = "Appt_Description";
        public const string strfACTIVITY_COMPLETE = "Activity_Complete";
        public const string strfACTIVITY_COMPLETED_DATE = "Activity_Completed_Date";
        public const string strfNOTES = "Notes";
        public const string strfCOMMENTS = "Comments";
        public const string strfREGION_ID = "Region_Id";
        public const string strfREGION_NAME = "Region_Name";
        public const string strfIAC_REGION_ID = "IAC_Region_Id";
        public const string strfRN_DESCRIPTOR = "Rn_Descriptor";
        #endregion

        #region pForms
        public const string strpRLIC_CONTACT = "IAC - RLIC Contact";
        #endregion
    }
}
