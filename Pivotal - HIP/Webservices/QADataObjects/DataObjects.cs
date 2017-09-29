using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TICPivotalQADataObjects
{
    #region Data Objects

    public class UserObj
    {
        public string contactHexId;
        public string companyType;
        public string First_Name;
        public string Last_Name;
        public string Middle_Initial;
        public string Title;
        public string Suffix;
        public string Company_Name;
        public string email;
        public string passwordHash;
        public string loginName;
        public string role;
        public UserObj[] impersUsers;
    }

    public class ScheduledInspection
    {
        public string scheduledInspectionId;
        public string projectName;
        public string phaseName;
        public string lotRecord;
        public string inspectionType;
        public string scheduledDate;
        public string projectedDate;
        public string baseLineDate;
        public string workflowStatus;
        public string inspectionTypeId;
        public string projectId;
    }

    public class InspectionTemplate
    {
        public string inspectionTemplateId;
        public string inspectionTemplateName;
        public string constructionProjectName;
        public string projectId;
        public string inspectionType;
        public string inspectionTypeId;
        public string inspectionStatus;
        public string phaseName;
        public string phaseId;
        public string templateVersion;
        public string inspectionScope;
        InspectionItemTemplate[] inspectionTemplateItems;
        
    }

    public class InspectionItemTemplate
    {
        public string itemName;
        public string itemDescription;
        public string inspectionTemplateId;
        public string inspectionTemplateItemId;
    
    }

    public class Inspection
    {
        public string inspectionTemplateId;
        public string inspectionTemplateName;
        public string projectId;
        public string projectName;
        public string phaseId;
        public string phaseName;
        public string inspectionType;
        public string inspectionTypeId;
        public string inspectionStatus;
        public string scheduledDate;
        public string dueDate;
        public string inspectedDateTime;
        public string supervisorId;
        public string supervisorName;
        public string createdById;
        public string createdByName;
        public string signOffUserId;
        public string signOffUserName;
        public string reinspectionDueDate;
        public string reinspectionCompleteDate;
        public string reinspectionSignOffUserId;
        public string reinspectionSignOffUserName;
        public string escalationSignOffUserId;
        public string escalationSignOffUserName;
        public string submittedByUserId;
        public string submittedByUserName;
        public string reinspectionSubmittedById;
        public string reinspectionSubittedByUserName;
        //public bool correctiveActionRequired;
        public string inspectionNotes;
        public string templateVersion;
        public string inspectionScope;
        public string inspectionId;
        public string inspectorName;
        public string inspectorId;
        public MiscellaneousCategory miscCategory;
        public InspectionItem[] inspectionItems;
        public InspectedScopeItem[] inspectedScopeItems;
        public QADocument[] qaDocs;
        public QAWeblinks[] qaWeblinks;
        public string websiteAction;
        public string correctiveActionDocLocation;
        public string correctiveActionStatus;
        public string lastSavedById;
        public string lastSavedByUserName;
        public string reinspectedByUserId;
        public string reinspectedByUserName;
    }

    public class MiscellaneousCategory
    {
        public string inspectionId;
        public string categoryId;
        public int ordinal;
        public string description;
    }

    public class InspectionItem
    {
        public string itemDescription;
        public string workingNotes;
        public string inspectionId;
        public string inspectionItemId;
        public string categoryDesc;
        public int categoryOrdinal;
        public int itemOrdinal;
        //public bool? isAcknowledged;
        public string categoryId;
        public string acknowledgeStatus;
    }

    public class ProjectFilter
    {
        public string projectId;
        public string projectName;
        public string inspectionScope;
    }

    public class PhaseFilter
    {
        public string projectId;
        public string PhaseName;        
    }

    public class InspectionTypeFilter
    {
        public string projectId;
        public string phaseName;
        public string inspectionTypeId;
        public string InspectionType;
    }

    public class FilterWrapper
    {
        public ProjectFilter[] projFilter;
        public PhaseFilter[] phaseFilter;
        public InspectionTypeFilter[] inspectionTypeFilter;
        public string[] inspectionStatuses;
    }

    public class ScheduledInspectionWrapper
    {
        public FilterWrapper filterWrapper;
        public ScheduledInspection[] scheduledInspections;
    }

    public class InspectionListWrapper
    {
        public FilterWrapper filterWrapper;
        public InspectionListItem[] inspections;
    }
                
    public class InspectedScopeItem
    {
        public string inspectedScopeItemId;
        public string projectId;
        public string phaseName;
        public string inspectionTypeId;
        public string scopeItemNumber;
        public string inspectionId;
        public string inspectionScope;
        public string[] units;
    }

    public class InspectionListItem
    {
        public string inspectionId;
        public string projectId;
        public string projectName;
        public string phaseNbr;
        public string inspectionScope;
        public string inspectionTypeId;
        public string inspectionTypeName;
        public string dueDate;
        public string status;
        public string[] scopeItemNbrs;
        public string inspectionCompleteDate;
        public string lastSavedById;
        public string lastSavedByUserName;
    }

    public class QADocument
    {
        public string inspectionId;
        public string documentDesc;
        public string documentPath;
        public string qaDocumentId;
        public bool? deleteDocument;
    }

    public class QAWeblinks
    {
        public string inspectionId;
        public string url;
        public string urlDesc;
        public string qaWebLinkId;
        public bool? deleteLink;
    }

    public class InspectionStatus
    { 
        public string Status;
        public string LastSavedById;          // Pivotal Contact hexID
        public string LastSavedByName;        // full contact name
        public string LastSavedByRole;        // contact’s role, i.e. Inspector, Superintendent, Builder
        public string LastSavedByCompanyType; // contact’s company type: Inspector, Builder
        public bool isUserAuthorizedForInspection;
    }


    public enum LoadActionsForInspection
    {
        OpenReadOnly,
        OpenForEdit
    }

    public enum ActionForInspection
    {
        Save,
        SaveAndClose,
        Submit, 
        Cancel
    }

    public enum CompanyType
    {
        Inspector,
        Builder
    }

    #endregion
}
