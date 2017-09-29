using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TICPivotalQADataAccess
{
    public interface IQADataAccess
    {
        PivotalED.UserDataTable GetUserProfileByLogin(string userLogin);
        PivotalED.UserDataTable GetInspectorsByAdmin(string userLogin);
        PivotalED.ConstructionProjectFilterDataTable GetConstructionFilterForUser(string userLogin);
        PivotalED.PhaseFilterDataTable GetPhaseFilterForUser(string userLogin);
        PivotalED.InspTemplateFilterDataTable GetInspTemplateFilterForUser(string userLogin);
        PivotalED.ScheduledInspectionsDataTable GetLotScheduledInspectionsForUserLogin(string userLogin, string projectName);
        PivotalED.ScheduledInspectionsDataTable GetBuildingScheduledInspectionsForUserLogin(string userLogin, string projectName);
        PivotalED.ScheduledInspectionsDataTable GetUnitScheduledInspectionsForUserLogin(string userLogin, string projectName);
        PivotalED.InspectionTemplateDataTable GetInspectionTemplateByInspectionType(int inspTypeId);
        PivotalED.InspectionStepTemplateDataTable GetInspectionTemplateCheckList(int inspTypeId);
        PivotalED.ExplodedUnitsDataTable GetExplodedUnitsByBuilding(int cpId, int inspTypeId, string phaseName, string buildingNumber);
        PivotalED.ExplodedUnitsDataTable GetExplodedUnitsByLot(int cpId, int inspTypeId, string phaseName, string lotNumber);
        PivotalED.InspectionDataTable GetInspectionByInspectionId(int inspectionId);
        PivotalED.InspectionStepDataTable GetInspectionStepsByInspectionId(int inspectionId);
        PivotalED.ScopeItemsDataTable GetScopeItemsByInspectionId(int inspectionId);
        PivotalED.InspectionListDataTable GetInspectionListByLoginAndStatus(string userLogin, string status);
        PivotalED.InspectionListDataTable GetInspectionListByLogin(string userLogin);
        PivotalED.MiscellaneousCategoryDataTable GetMiscellaneousCategory(int inspectionId);
        PivotalED.QADocumentsDataTable GetQADocumentsByInspectionId(int inspectionId);
        PivotalED.QAWeblinksDataTable GetQAWeblinksByInspectionId(int inspectionId);
        PivotalED.InspectionStatusDataTable GetInspectionStatus(int inspectionId);
        PivotalED.InspectionStatusesDataTable GetInspectionStatuses();
        PivotalED.InspTemplateFilterDataTable GetInspectionTypesForInspectorLogin(string login);
        PivotalED.ContactDataTable GetContactByLogin(string login);
        PivotalED.InspectionListDataTable GetInspectionListByCompanyId(int companyIdInt);
        PivotalED.InspectionListDataTable GetInspectionListByCompanyAndStatus(int companyIdint, string status);
        PivotalED.ScheduledInspectionsDataTable GetBuildingInspectionsByInspectorLogin(string login, string projectName);
        PivotalED.ScheduledInspectionsDataTable GetLotInspectionsByInspectorLogin(string login, string projectName);
        PivotalED.ScheduledInspectionsDataTable GetUnitInspectionsByInspectorLogin(string login, string projectName);
        PivotalED.InspectionListDataTable GetInspectionByCompanyAndInspectionId(int companyId, int inspectionId);
        PivotalED.InspectionListDataTable GetInspectionByLoginAndInspectionId(string login, int inspectionId);
    }

    // Factory to get instance of interface
    public class QADataAccessFactory
    {
        public static IQADataAccess GetQADataAccess()
        {
            return new QADataAccess();
        }
    }
}
