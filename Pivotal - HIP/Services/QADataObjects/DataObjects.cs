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
        public string Type;
        public string First_Name;
        public string Last_Name;
        public string Middle_Initial;
        public string Title;
        public string Suffix;
        public string Company_Name;
        public string email;
        public string passwordHash;
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
    }

    public class InspectionTemplate
    {
        public string inspectionTemplateId;
        public string inspectionTemplateName;
        public string constructionProjectName;
        public string inspectionType;
        public string inspectionStatus;
        public string phaseName;
        public string templateVersion;
        public string inspectionScope;
        InspectionItemTemplate inspectionTemplateItems;
        InspectedEntity[] inspectedEntities;

    }

    public class InspectionItemTemplate
    {
        public string itemName;
        public string itemDescription;
        public string inspectionTemplateId;
    
    }

    public class Inspection
    {
        public string inspectionStatus;
        public string inspectionType;
        public string scheduledDate;
        public string dueDate;
        public string inspectionScope;
        public string inspectionId;
        public InspectionItem[] inspectionItems;
        InspectedEntity[] inspectedEntities;        
    
    }

    public class InspectionItem
    { }

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
        public string InspectionType;
    }

    public class FilterWrapper
    {
        public ProjectFilter[] projFilter;
        public PhaseFilter[] phaseFilter;
        public InspectionTypeFilter[] inspectionTypeFilter;
    }

    public class ScheduledInspectionWrapper
    {
        public FilterWrapper filterWrapper;
        public ScheduledInspection[] scheduledInspections;
    }

    public class InspectionWrapper
    {
        public FilterWrapper filterWrapper;
        public Inspection[] inspections;
    }

    public class InspectedEntity
    {
        public string inspectionScopeType;
        public string productId;
        public string inspectionId;
        public string inspectedBuildingId;
        public string scheduledInspDate;
        public string actualInspDate;
        public string inspReportEntryDate;
        public bool noCorrectiveActionRequired;
        public bool correctionMadeDuringInsp;
        public bool followOnInspectionRequired;
        public bool inspectorAckReq;
        public bool fieldManagerAckReq;
        public string dateOfReinspection;
        public bool inspectorSignOff;
        public bool fieldManagerSignOff;
        public string comments;
    }

    #endregion
}
