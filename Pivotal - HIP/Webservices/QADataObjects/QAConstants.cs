using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TICPivotalQADataObjects
{
    public static class QAConstants
    {
        //Constants
        public const string strcBUILDING_SCOPE = "Building";
        public const string strcLOT_SCOPE = "Lot";
        public const string strcUNIT_SCOPE = "Unit";

        //Error Messages
        public const string strmsgINSPECTION_SCOPE_NOT_DEFINED = "Construction Project does not have an Inspection Scope defined.";

        //PBS Service constants
        public const string INSPECTION_ACTIVE_FORM_NAME = "TIC Int QA Inspection";
        public const string INSPECTION_TABLE_NAME = "Inspection";

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




    }
}
