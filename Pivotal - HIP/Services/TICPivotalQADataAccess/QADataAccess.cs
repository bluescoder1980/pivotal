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
    public class QADataAccess
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
                = new PivotalEDTableAdapters.UserTableAdapter();
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


        #endregion


        



    }
}
