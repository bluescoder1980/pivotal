using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace TICPivotalQADataAccess
{

    /// <summary>
    /// This class will provide a Data Access interface for the QA Webservice
    /// to interact with in order to provide Pivotal data to the QA Website via Web Service calls
    /// </summary>
    public class QADataAccess : IQADataAccess
    {
        #region Data Access Queries
        
        /// <summary>
        /// This method will get the User Profile for the user name passed
        /// into the DAL.
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public PivotalED.UserDataTable GetUserProfileByLogin(string userLogin)
        {
            PivotalEDTableAdapters.UserTableAdapter user
                = new  PivotalEDTableAdapters.UserTableAdapter();            
            return user.GetUserByLogin(userLogin);        

        }

        /// <summary>
        /// This method will return all Inspectors in which the Admin is configured
        /// to record inspections on behalf of.
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public PivotalED.UserDataTable GetInspectorsByAdmin(string userLogin)
        {
            PivotalEDTableAdapters.UserTableAdapter user
                = new PivotalEDTableAdapters.UserTableAdapter();
            return user.GetContactAdminsByUserLogin(userLogin);
        }

        /// <summary>
        /// This method will get all Construction projects where this user is defined on.
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public PivotalED.ConstructionProjectFilterDataTable GetConstructionFilterForUser(string userLogin)
        {
            PivotalEDTableAdapters.ConstructionProjectFilterTableAdapter cpFilter
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.ConstructionProjectFilterTableAdapter();
            return cpFilter.GetProjectFilterByUserLogin(userLogin);
        }

        /// <summary>
        /// This method will get all the Phases for the construction projects this user belongs to
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public PivotalED.PhaseFilterDataTable GetPhaseFilterForUser(string userLogin)
        {
            PivotalEDTableAdapters.PhaseFilterTableAdapter phaseFilter
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.PhaseFilterTableAdapter();
            PivotalED.PhaseFilterDataTable phaseDt = new PivotalED.PhaseFilterDataTable();           
            return phaseFilter.GetPhaseFilterByUserLogin(userLogin);
        }

        /// <summary>
        /// This method will get all the Inspection Templates for the construction project this 
        /// user belongs to (per his company)
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public PivotalED.InspTemplateFilterDataTable GetInspTemplateFilterForUser(string userLogin)
        {
            PivotalEDTableAdapters.InspTemplateFilterTableAdapter inspTempFilter
               = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspTemplateFilterTableAdapter();
            return inspTempFilter.GetInspTemplatesByUserLogin(userLogin);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public PivotalED.ScheduledInspectionsDataTable GetLotScheduledInspectionsForUserLogin(string userLogin, string projectName)
        {
            PivotalEDTableAdapters.ScheduledInspectionsTableAdapter lotSIs
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.ScheduledInspectionsTableAdapter();
            return lotSIs.GetLotScheduledInspectionsByUserLogin(userLogin, projectName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public PivotalED.ScheduledInspectionsDataTable GetBuildingScheduledInspectionsForUserLogin(string userLogin, string projectName)
        {
            PivotalEDTableAdapters.ScheduledInspectionsTableAdapter buildingSIs
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.ScheduledInspectionsTableAdapter();
            return buildingSIs.GetBuildingScheduledInspectionsByUserLogin(userLogin, projectName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public PivotalED.ScheduledInspectionsDataTable GetUnitScheduledInspectionsForUserLogin(string userLogin, string projectName)
        {
            PivotalEDTableAdapters.ScheduledInspectionsTableAdapter unitSIs
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.ScheduledInspectionsTableAdapter();
            return unitSIs.GetUnitScheduledInspectionsByUserLogin(userLogin, projectName);
        }

        /// <summary>
        /// This method will get the Inspection Template for the selected Inspection type when 
        /// the user clicks on create inspection
        /// </summary>
        /// <param name="inspTypeId"></param>
        /// <returns></returns>
        public PivotalED.InspectionTemplateDataTable GetInspectionTemplateByInspectionType(int inspTypeId)
        {
            PivotalEDTableAdapters.InspectionTemplateTableAdapter templates
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspectionTemplateTableAdapter();
            return templates.GetInspectionTemplate(inspTypeId);
        }

        /// <summary>
        /// This query will return the inspection check list items for a specific inspection template
        /// </summary>
        /// <param name="inspTypeId"></param>
        /// <returns></returns>
        public PivotalED.InspectionStepTemplateDataTable GetInspectionTemplateCheckList(int inspTypeId)
        {
            PivotalEDTableAdapters.InspectionStepTemplateTableAdapter templateList
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspectionStepTemplateTableAdapter();
            return templateList.GetInspectionCheckListItems(inspTypeId);
        }

        /// <summary>
        /// This query will be used to get all associated Units for the selected Scheduled inspection
        /// by Building
        /// </summary>
        /// <param name="cpId"></param>
        /// <param name="inspTypeId"></param>
        /// <param name="phaseName"></param>
        /// <param name="buildingNumber"></param>
        /// <returns></returns>
        public PivotalED.ExplodedUnitsDataTable GetExplodedUnitsByBuilding(int cpId, int inspTypeId,
            string phaseName, string buildingNumber)
        {
            PivotalEDTableAdapters.ExplodedUnitsTableAdapter bUnits
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.ExplodedUnitsTableAdapter();
            return bUnits.GetExplodedUnitsByBuilding(cpId, phaseName, buildingNumber, inspTypeId);
        }

        /// <summary>
        /// This query will be used to get all associated units for the selected scheduled inspections
        /// by lot number
        /// </summary>
        /// <param name="cpId"></param>
        /// <param name="inspTypeId"></param>
        /// <param name="phaseName"></param>
        /// <param name="lotNumber"></param>
        /// <returns></returns>
        public PivotalED.ExplodedUnitsDataTable GetExplodedUnitsByLot(int cpId, int inspTypeId,
            string phaseName, string lotNumber)
        {
            PivotalEDTableAdapters.ExplodedUnitsTableAdapter lUnits
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.ExplodedUnitsTableAdapter();
            return lUnits.GetExplodedUnitsByLotNumber(cpId, phaseName, lotNumber, inspTypeId);
        }

        /// <summary>
        /// Executes query to retreive inspection record by id
        /// </summary>
        /// <param name="inspectionId"></param>
        /// <returns></returns>
        public PivotalED.InspectionDataTable GetInspectionByInspectionId(int inspectionId)
        {
            PivotalEDTableAdapters.InspectionTableAdapter insp
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspectionTableAdapter();
            return insp.GetInspectionById(inspectionId);
        }

        /// <summary>
        /// This method will return inspection steps for the assocated id
        /// </summary>
        /// <param name="inspectionId"></param>
        /// <returns></returns>
        public PivotalED.InspectionStepDataTable GetInspectionStepsByInspectionId(int inspectionId)
        {
            PivotalEDTableAdapters.InspectionStepTableAdapter inspSteps
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspectionStepTableAdapter();
            return inspSteps.GetInspectionStepByInspectionId(inspectionId);
        }

        /// <summary>
        /// This method will get Inspected scope items by inspection id
        /// </summary>
        /// <param name="inspectionId"></param>
        /// <returns></returns>
        public PivotalED.ScopeItemsDataTable GetScopeItemsByInspectionId(int inspectionId)
        {
            PivotalEDTableAdapters.ScopeItemsTableAdapter scope
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.ScopeItemsTableAdapter();
            return scope.GetSCopeItemsByInspectionId(inspectionId);
        }

        /// <summary>
        /// This method will get all inspections by login and a provided status
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public PivotalED.InspectionListDataTable GetInspectionListByLoginAndStatus(string userLogin,
            string status)
        {
            PivotalEDTableAdapters.InspectionListTableAdapter inspList
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspectionListTableAdapter();
            return inspList.GetInspectionListByLoginAndStatus(userLogin, status);

        }

        /// <summary>
        /// This method will get all inspections by login 
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public PivotalED.InspectionListDataTable GetInspectionListByLogin(string userLogin)
        {
            PivotalEDTableAdapters.InspectionListTableAdapter insplist
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspectionListTableAdapter();
            return insplist.GetInspectionListByLogin(userLogin);
        }

        /// <summary>
        /// stand alone method used to get miscellaneous category created when the inspection is
        /// created.
        /// </summary>
        /// <param name="inspectionId"></param>
        /// <returns></returns>
        public PivotalED.MiscellaneousCategoryDataTable GetMiscellaneousCategory(int inspectionId)
        {
            PivotalEDTableAdapters.MiscellaneousCategoryTableAdapter misc
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.MiscellaneousCategoryTableAdapter();
            return misc.GetMiscellaneousCategoryByInspectionId(inspectionId);
        }

        public PivotalED.QADocumentsDataTable GetQADocumentsByInspectionId(int inspectionId)
        {
            PivotalEDTableAdapters.QADocumentsTableAdapter docs
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.QADocumentsTableAdapter();
            return docs.GetQADocumentsByInspectionId(inspectionId);
        }

        public PivotalED.QAWeblinksDataTable GetQAWeblinksByInspectionId(int inspectionId)
        {
            PivotalEDTableAdapters.QAWeblinksTableAdapter links
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.QAWeblinksTableAdapter();
            return links.GetQAWeblinksByInspectionId(inspectionId);
        }

        public PivotalED.InspectionStatusDataTable GetInspectionStatus(int inspectionId)
        {
            PivotalEDTableAdapters.InspectionStatusTableAdapter insp
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspectionStatusTableAdapter();
            return insp.GetInspectionStatusByInspectionId(inspectionId);
        }

        public PivotalED.InspectionStatusesDataTable GetInspectionStatuses()
        {
            PivotalEDTableAdapters.InspectionStatusesTableAdapter inspStatuses
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspectionStatusesTableAdapter();
            return inspStatuses.GetInspectionStatuses();
                
        }

        public PivotalED.InspTemplateFilterDataTable GetInspectionTypesForInspectorLogin(string login)
        {
            PivotalEDTableAdapters.InspTemplateFilterTableAdapter inspTypes
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspTemplateFilterTableAdapter();
            return inspTypes.GetInspectionTypeFilterForInspector(login);
        }

        public PivotalED.ContactDataTable GetContactByLogin(string login)
        {
            PivotalEDTableAdapters.ContactTableAdapter cnt
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.ContactTableAdapter();
            return cnt.GetContactByLogin(login);
        }

        public PivotalED.InspectionListDataTable GetInspectionListByCompanyId(int companyIdInt)
        {
            PivotalEDTableAdapters.InspectionListTableAdapter inspList
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspectionListTableAdapter();
            return inspList.GetInspectionListByCompany(companyIdInt);
        }

        public PivotalED.InspectionListDataTable GetInspectionListByCompanyAndStatus(int companyIdint, string status)
        {
            PivotalEDTableAdapters.InspectionListTableAdapter inspList
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspectionListTableAdapter();
            return inspList.GetInspectionListByCompanyAndStatus(companyIdint, status);
        }

        public PivotalED.ScheduledInspectionsDataTable GetBuildingInspectionsByInspectorLogin(string login, string projectName)
        {
            PivotalEDTableAdapters.ScheduledInspectionsTableAdapter schInsp
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.ScheduledInspectionsTableAdapter();
            return schInsp.GetBuildingScheduledInspectionsByInspectorLogin(login, projectName);
        }

        public PivotalED.ScheduledInspectionsDataTable GetLotInspectionsByInspectorLogin(string login, string projectName)
        {
            PivotalEDTableAdapters.ScheduledInspectionsTableAdapter schInsp
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.ScheduledInspectionsTableAdapter();
            return schInsp.GetLotScheduledInspectionsByInspectorLogin(login, projectName);
        }

        public PivotalED.ScheduledInspectionsDataTable GetUnitInspectionsByInspectorLogin(string login, string projectName)
        {
            PivotalEDTableAdapters.ScheduledInspectionsTableAdapter schInsp
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.ScheduledInspectionsTableAdapter();
            return schInsp.GetUnitScheduledInspectionsByInspectorLogin(login, projectName);
        }

        public PivotalED.InspectionListDataTable GetInspectionByCompanyAndInspectionId(int companyId, int inspectionId)
        {
            PivotalEDTableAdapters.InspectionListTableAdapter insp
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspectionListTableAdapter();
            return insp.GetInspectionForCompanyAndInspectionId(companyId, inspectionId);
        }

        public PivotalED.InspectionListDataTable GetInspectionByLoginAndInspectionId(string login, int inspectionId)
        {
            PivotalEDTableAdapters.InspectionListTableAdapter insp
                = new TICPivotalQADataAccess.PivotalEDTableAdapters.InspectionListTableAdapter();
            return insp.GetInspectionByLoginAndInspectionId(login, inspectionId);
        }
        #endregion


        



    }
}
