using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TICPivotalQADataAccess.PivotalEDTableAdapters;
using TICPivotalQADataAccess;
using TICPivotalQADataObjects;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility; //Using the TypeConvert from this

namespace TICPivotalQAController
{
    public class QAController
    {

        #region Class Level Vars
        QADataAccess dal;
        #endregion

        /// <summary>
        /// This method will retrieve the user profile for the
        /// user name provided and build a userObject including the
        /// users which could be acted on behalf of.
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public UserObj GetUserForLogin(string userLogin)
        {
            //Set up Service objects
            UserObj userObj = new UserObj();

            //Log the user in
            QADataAccess dal = new QADataAccess();
            PivotalED.UserDataTable userDataTable = dal.GetUserProfileByLogin(userLogin);
            PivotalED.UserDataTable impUsersDataTable = dal.GetInspectorsByAdmin(userLogin);
                        
            //TO-DO : Need to make sure GetUserProfileByLogin only returns 1 row
            foreach (PivotalED.UserRow user in userDataTable)
            {
                //Map user to user object
                userObj.contactHexId = user.contact_id_int.ToString("X16");
                userObj.Company_Name = TypeConvert.ToString(user.company_name);
                userObj.email = TypeConvert.ToString(user.email);
                userObj.First_Name = TypeConvert.ToString(user.first_name);
                userObj.Last_Name = TypeConvert.ToString(user.last_name);
                userObj.Middle_Initial = TypeConvert.ToString(user.middle_initial);
                userObj.Suffix = TypeConvert.ToString(user.suffix);
                userObj.Title = TypeConvert.ToString(user.title);
                userObj.Type = TypeConvert.ToString(user.type);
                userObj.passwordHash = TypeConvert.ToString(user.password_encrypt);
            }

            List<UserObj> impUsers = new List<UserObj>();
            //Get impersonating users if exist and add to user object
            foreach (PivotalED.UserRow impUser in impUsersDataTable)
            {
                UserObj uObj = new UserObj();
                uObj.contactHexId = impUser.contact_id_int.ToString("X16");
                uObj.Company_Name = impUser.company_name;
                uObj.email = impUser.email;
                uObj.First_Name = impUser.first_name;
                uObj.Last_Name = impUser.last_name;
                uObj.Middle_Initial = impUser.middle_initial;
                uObj.Suffix = impUser.suffix;
                uObj.Title = impUser.title;
                uObj.Type = impUser.type;
                uObj.passwordHash = impUser.password_encrypt;
                impUsers.Add(uObj);
            }

            //Set impersonating users on User object to return
            // to QA App
            userObj.impersUsers = impUsers.ToArray();           
            return userObj;
    
        }


        /// <summary>
        /// This method will need to get all the scheduled inspections for the user based
        /// on the configured Construction Projects for the inspector's company
        /// regardless of the scope (Building, Lot, Unit).  Also it will get all the filters
        /// necessary for the web to display the correct scheduled inspections
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        public ScheduledInspectionWrapper GetScheduledInspectionsForUser(string userLogin)
        { 
            //Set up objects
            ProjectFilter[] projFilterObjArr;
            PhaseFilter[] phaseFilterObjArr;
            InspectionTypeFilter[] inspTypeFilterObjArr;
            ScheduledInspection[] scheduledInspectionsObjArr;
            ScheduledInspectionWrapper wrapper = new ScheduledInspectionWrapper();

            //Call to Data Access Layer
            dal = new QADataAccess();
            PivotalED.ConstructionProjectFilterDataTable projFilterDataTable= dal.GetConstructionFilterForUser(userLogin);
            PivotalED.PhaseFilterDataTable phaseFilterDataTable = dal.GetPhaseFilterForUser(userLogin);
            PivotalED.InspTemplateFilterDataTable inspTypeFilterDataTable = dal.GetInspTemplateFilterForUser(userLogin);

            //List to store scheduled inspections (pass by ref)
            List<ScheduledInspection> schInspList = new List<ScheduledInspection>();
            List<ProjectFilter> projFilterList = new List<ProjectFilter>();
            List<PhaseFilter> phaseFilterList = new List<PhaseFilter>();
            List<InspectionTypeFilter> inspTypeFilterList = new List<InspectionTypeFilter>();

            //Now with the list of Construction Projects lets determine
            //which inspection scopes we need to get and call the appropriate DAL method
            foreach (PivotalED.ConstructionProjectFilterRow projFilterRow in projFilterDataTable)
            { 
                //Build Construction Project filter
                ProjectFilter projFilter = new ProjectFilter();
                projFilter.projectId = projFilterRow.tic_construction_project_id_int.ToString("X16");
                projFilter.projectName = projFilterRow.tic_construction_project_name;
                projFilter.inspectionScope = projFilterRow.TIC_Inspection_Scope;

                //Check the Scope configured for each project and populate the necessary
                //scheduled inspections based on this project and user login
                if (projFilterRow.TIC_Inspection_Scope == QAConstants.strcBUILDING_SCOPE)
                {
                    GetBuildingScheduledInspectionForUser(userLogin, 
                        projFilter.projectName, ref schInspList);                
                }
                else if (projFilterRow.TIC_Inspection_Scope == QAConstants.strcLOT_SCOPE)
                {
                    GetLotScheduledInspectionsForUser(userLogin, 
                        projFilter.projectName, ref schInspList);                    
                }
                else if (projFilterRow.TIC_Inspection_Scope == QAConstants.strcUNIT_SCOPE)
                {
                    GetUnitScheduledInspectionForUser(userLogin, 
                        projFilter.projectName, ref schInspList);                    
                }
                else
                {
                    throw new Exception(QAConstants.strmsgINSPECTION_SCOPE_NOT_DEFINED 
                        + " : " + projFilterRow.tic_construction_project_name);
                }

                projFilterList.Add(projFilter);

            }
            
            //Now let's populate the Phase Filter
            foreach (PivotalED.PhaseFilterRow phaseFilterRow in phaseFilterDataTable)
            {
                PhaseFilter phaseFilter = new PhaseFilter();
                phaseFilter.projectId = phaseFilterRow.tic_construction_project_id_int.ToString("X16");
                phaseFilter.PhaseName = phaseFilterRow.phase_name;
                phaseFilterList.Add(phaseFilter);
            }

            //Now populate template/Inspection Type list defined at the project level
            foreach (PivotalED.InspTemplateFilterRow inspTypeFilterRow in inspTypeFilterDataTable)
            {
                InspectionTypeFilter inspTypeFilter = new InspectionTypeFilter();
                inspTypeFilter.projectId = inspTypeFilterRow.tic_construction_project_id_int.ToString("X16");
                inspTypeFilter.InspectionType = inspTypeFilterRow.date_description;
                inspTypeFilterList.Add(inspTypeFilter);
            }
            
            //Set Scheduled inpsections object array
            scheduledInspectionsObjArr = schInspList.ToArray();
            //Filter to object arrays
            projFilterObjArr = projFilterList.ToArray();
            phaseFilterObjArr = phaseFilterList.ToArray();
            inspTypeFilterObjArr = inspTypeFilterList.ToArray();

            //set filter wrapper
            FilterWrapper filtWrapper = new FilterWrapper();
            filtWrapper.inspectionTypeFilter = inspTypeFilterObjArr;
            filtWrapper.phaseFilter = phaseFilterObjArr;
            filtWrapper.projFilter = projFilterObjArr;

            //Set payload and pass to service layer
            wrapper.filterWrapper = filtWrapper;
            wrapper.scheduledInspections = scheduledInspectionsObjArr;
            
            return wrapper;



        }

        #region Private Controller Methods

        /// <summary>
        /// class method which will get the inspections for the user at the lot aggregate
        /// based on the Scope of the Project, userlogin and project name
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        private void GetLotScheduledInspectionsForUser(string userLogin, 
            string projectName, ref List<ScheduledInspection> siList)
        {
            //Get Scheduled Inspections for Construction Project at the lot level
            PivotalED.ScheduledInspectionsDataTable lotSIDataTable
                = dal.GetLotScheduledInspectionsForUserLogin(userLogin, projectName);

            foreach (PivotalED.ScheduledInspectionsRow siLotRow in lotSIDataTable)
            {
                ScheduledInspection si = new ScheduledInspection();
                si.projectName = siLotRow.tic_construction_project_name;
                si.phaseName = siLotRow.phase_name;
                si.lotRecord = siLotRow.lot_number;
                si.inspectionType = siLotRow.date_description;
                si.scheduledDate = TypeConvert.ToString(siLotRow.Scheduled_Date);
                siList.Add(si);
            }
            
        }

        /// <summary>
        /// class method which will get the inspections for the user at the Unit level
        /// based on the Scope of the Project, userlogin and project name
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="projectName"></param>
        /// <param name="siList"></param>
        private void GetUnitScheduledInspectionForUser(string userLogin, 
            string projectName, ref List<ScheduledInspection> siList)
        {
            //Get Scheduled Inspections for Construction Project at the lot level
            PivotalED.ScheduledInspectionsDataTable lotSIDataTable
                = dal.GetUnitScheduledInspectionsForUserLogin(userLogin, projectName);

            foreach (PivotalED.ScheduledInspectionsRow siLotRow in lotSIDataTable)
            {
                ScheduledInspection si = new ScheduledInspection();
                si.projectName = siLotRow.tic_construction_project_name;
                si.phaseName = siLotRow.phase_name;
                si.lotRecord = siLotRow.Unit;
                si.inspectionType = siLotRow.date_description;
                si.scheduledDate = TypeConvert.ToString(siLotRow.Scheduled_Date);
                siList.Add(si);
            }
        }

        /// <summary>
        /// class method which will get the inspections for the user at the Building aggregate
        /// based on the Scope of the Project, userlogin and project name
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="projectName"></param>
        /// <param name="siList"></param>
        private void GetBuildingScheduledInspectionForUser(string userLogin, 
            string projectName, ref List<ScheduledInspection> siList)
        {
            //Get Scheduled Inspections for Construction Project at the lot level
            PivotalED.ScheduledInspectionsDataTable lotSIDataTable
                = dal.GetBuildingScheduledInspectionsForUserLogin(userLogin, projectName);

            foreach (PivotalED.ScheduledInspectionsRow siLotRow in lotSIDataTable)
            {
                ScheduledInspection si = new ScheduledInspection();
                si.projectName = siLotRow.tic_construction_project_name;
                si.phaseName = siLotRow.phase_name;
                si.lotRecord = siLotRow.building;
                si.inspectionType = siLotRow.date_description;
                si.scheduledDate = TypeConvert.ToString(siLotRow.Scheduled_Date);
                siList.Add(si);
            }
        }

        #endregion


    }
}
