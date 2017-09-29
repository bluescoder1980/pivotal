using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TICPivotalQADataAccess.PivotalEDTableAdapters;
using TICPivotalQADataAccess;
using TICPivotalQADataObjects;
using TICQAPBSComms;
using TICQAPBSComms.Enumerations;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility; //Using the TypeConvert from this

namespace TICPivotalQAController 
{
    /// <summary>
    /// This class will provide the business logic layer for the QA webservice.  All
    /// calls to the DAL will reside here and any additional business logic will reside in
    /// this class so that the QA Webservice will not need to implement any business specific 
    /// code.
    /// </summary>
    /// <Author>A.Maldonado</Author>
    public class QAController : IQAController, IQAPBSController
    {
        #region Class Level Vars
        //Use interface to interact with DAL
        IQADataAccess dal;
        private GenericData genData;
        string pivotalSystemName;
        #endregion

               
        /// <summary>
        /// Class constructor to allow web service to pass
        /// Pivotalsystem name to connect to.
        /// </summary>
        /// <param name="pivotalSysName"></param>
        public QAController(string pivotalSysName)
        {
            pivotalSystemName = pivotalSysName;
        }

        /// <summary>
        /// Generic Class constructor
        /// </summary>
        /// <param name="pivotalSysName"></param>
        public QAController()
        {}

        #region Read-Only Methods (IQAController Interface)

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
            //Use Factory to get access to DAL
            dal = QADataAccessFactory.GetQADataAccess();
            
            PivotalED.UserDataTable userDataTable = dal.GetUserProfileByLogin(userLogin);
            PivotalED.UserDataTable impUsersDataTable = dal.GetInspectorsByAdmin(userLogin);
                        
            //TO-DO : Need to make sure GetUserProfileByLogin only returns 1 row
            foreach (PivotalED.UserRow user in userDataTable)
            {
                //Map user to user object
                userObj.contactHexId = user.contact_id_int.ToString("X16");
                userObj.Company_Name = TypeConvert.ToString(user.company_name);
                userObj.email = TypeConvert.ToString(user.email);
                userObj.loginName = TypeConvert.ToString(user.login_name);
                userObj.First_Name = TypeConvert.ToString(user.first_name);
                userObj.Last_Name = TypeConvert.ToString(user.last_name);
                userObj.Middle_Initial = TypeConvert.ToString(user.middle_initial);
                userObj.Suffix = TypeConvert.ToString(user.suffix);
                userObj.Title = TypeConvert.ToString(user.title);
                userObj.companyType = TypeConvert.ToString(user.company_type);
                userObj.role = TypeConvert.ToString(user.role);
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
                uObj.loginName = impUser.login_name;
                uObj.First_Name = impUser.first_name;
                uObj.Last_Name = impUser.last_name;
                uObj.Middle_Initial = impUser.middle_initial;
                uObj.Suffix = impUser.suffix;
                uObj.Title = impUser.title;
                uObj.companyType = impUser.company_type;
                uObj.role = impUser.role;
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
        public ScheduledInspectionWrapper GetScheduledInspectionsForUser(string userLogin, CompanyType type)
        { 
            //Set up objects
            ProjectFilter[] projFilterObjArr;
            PhaseFilter[] phaseFilterObjArr;
            InspectionTypeFilter[] inspTypeFilterObjArr;
            ScheduledInspection[] scheduledInspectionsObjArr;
            ScheduledInspectionWrapper wrapper = new ScheduledInspectionWrapper();

            //Use Factory to get access to DAL
            dal = QADataAccessFactory.GetQADataAccess();

            //Call to Data Access Layer            
            PivotalED.ConstructionProjectFilterDataTable projFilterDataTable= dal.GetConstructionFilterForUser(userLogin);
            PivotalED.PhaseFilterDataTable phaseFilterDataTable = dal.GetPhaseFilterForUser(userLogin);

            //AM2011.02.09 - Changed during IT Testing
            PivotalED.InspTemplateFilterDataTable inspTypeFilterDataTable;
            if(type == CompanyType.Inspector)
            {
                inspTypeFilterDataTable = dal.GetInspectionTypesForInspectorLogin(userLogin);
            }
            else
            {
                inspTypeFilterDataTable = dal.GetInspTemplateFilterForUser(userLogin);
            }

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
                            projFilter.projectName, ref schInspList, type);
                    
                }
                else if (projFilterRow.TIC_Inspection_Scope == QAConstants.strcLOT_SCOPE)
                {
                    GetLotScheduledInspectionsForUser(userLogin, 
                        projFilter.projectName, ref schInspList, type);                    
                }
                else if (projFilterRow.TIC_Inspection_Scope == QAConstants.strcUNIT_SCOPE)
                {
                    GetUnitScheduledInspectionForUser(userLogin, 
                        projFilter.projectName, ref schInspList, type);                    
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
                phaseFilter.PhaseName = phaseFilterRow.construction_phase_number;
                phaseFilterList.Add(phaseFilter);
            }

            //Now populate template/Inspection Type list defined at the project level
            foreach (PivotalED.InspTemplateFilterRow inspTypeFilterRow in inspTypeFilterDataTable)
            {
                InspectionTypeFilter inspTypeFilter = new InspectionTypeFilter();
                inspTypeFilter.projectId = inspTypeFilterRow.tic_construction_project_id_int.ToString("X16");
                inspTypeFilter.InspectionType = inspTypeFilterRow.date_description;
                inspTypeFilter.phaseName = null;
                inspTypeFilter.inspectionTypeId = inspTypeFilterRow.tic_inspection_type_id_int.ToString("X16");
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

        /// <summary>
        /// This method will be used to retreive a new Inspection Template based on the
        /// Construction Project and Inspection Type Id passed to the
        /// </summary>
        /// <returns></returns>       
        public Inspection LoadExistingInspection(string inspectionId, LoadActionsForInspection action, string lastSavedByUserId)
        {
            //TO-DO: 
            //The first thing we need to do is get the current status,
            //if read-only then just load the inspection as is in the DB
            //if editable then update status on inspection then load
            if (action == LoadActionsForInspection.OpenForEdit)
            {
                //Get existing status from DB
                string strStatus = GetCurrentInspectionStatus(inspectionId);
                //Save record and let ASR set the appropriate state transition if necessary            
                UpdateInspectionStatus(pivotalSystemName, inspectionId, strStatus, action.ToString(), null, lastSavedByUserId);
            }



            //Use DAL to get inspection template
            dal = QADataAccessFactory.GetQADataAccess();
            int inspectionIdInt = Convert.ToInt32(inspectionId, 16);

            //1) Get the Inspection           
            PivotalED.InspectionDataTable inspTemp
                = dal.GetInspectionByInspectionId(inspectionIdInt);
                        
            Inspection inspection = new Inspection();
            List<InspectionItem> inspectionItemList = new List<InspectionItem>();

            //should always return one inspection record
            foreach (PivotalED.InspectionRow temp in inspTemp)
            {
                inspection.inspectionScope = temp.tic_scope;
                inspection.inspectionId = inspectionId;              
                inspection.projectId = ValidatePivotalIdValue(temp.construction_project_id_int);
                inspection.projectName = temp.tic_construction_project_name;
                inspection.phaseName = temp.tic_Construction_Phase;
                inspection.inspectionType = temp.tic_inspection_type;
                inspection.inspectionTypeId = ValidatePivotalIdValue(temp.tic_inspection_type_id_int);
                inspection.inspectionStatus = temp.status;
                //inspection.correctiveActionRequired = Convert.ToBoolean(temp.TIC_Corrective_Action_Required);
                inspection.correctiveActionStatus = temp.TIC_Corrective_Action_Status;
                inspection.inspectedDateTime = !temp.IsDate_CompleteNull() ? TypeConvert.ToString(temp.Date_Complete) : null;
                inspection.inspectorId = !temp.Istic_inspector_id_intNull() ? temp.tic_inspector_id_int.ToString("X16") : string.Empty;
                inspection.inspectorName = temp.tic_inspector_name;
                inspection.createdById = !temp.Istic_created_by_id_intNull() ? temp.tic_created_by_id_int.ToString("X16") : string.Empty;
                inspection.createdByName = temp.tic_created_by_name;
                inspection.supervisorId = temp.Istic_supervisor_id_intNull() != true ? temp.tic_supervisor_id_int.ToString("X16") : string.Empty;
                inspection.supervisorName = temp.tic_supervisor_name;
                inspection.signOffUserId = !temp.Istic_sign_off_user_id_intNull() ? temp.tic_sign_off_user_id_int.ToString("X16") : string.Empty;
                inspection.signOffUserName = temp.tic_sign_off_user_name;
                inspection.reinspectionSignOffUserId = !temp.Istic_reinsp_sign_off_user_idNull() ? temp.tic_reinsp_sign_off_user_id.ToString("X16") : string.Empty;
                inspection.reinspectionSignOffUserName = temp.tic_reinsp_sign_off_user_name;
                inspection.escalationSignOffUserId = !temp.Istic_esc_user_sign_off_Id_intNull() ? temp.tic_esc_user_sign_off_Id_int.ToString("X16") : string.Empty;
                inspection.escalationSignOffUserName = temp.tic_esc_user_sign_off_name;
                inspection.submittedByUserName = temp.tic_submitted_by_user_name;
                inspection.reinspectionSubittedByUserName = temp.TIC_Reinsp_Submitted_By_name;

                inspection.lastSavedById = !temp.IsTIC_Last_Saved_By_IdNull() ? temp.TIC_Last_Saved_By_Id.ToString("X16") : string.Empty;
                inspection.lastSavedByUserName = temp.TIC_Last_Saved_By_Name;

                inspection.submittedByUserId = !temp.Istic_submitted_by_idNull() ? temp.tic_submitted_by_id.ToString("X16") : string.Empty;
                inspection.reinspectionSubmittedById = !temp.IsTIC_Reinsp_Submitted_By_IdNull() ? temp.TIC_Reinsp_Submitted_By_Id.ToString("X16") : string.Empty;

                inspection.reinspectedByUserId = !temp.IsTIC_Reinspected_By_IdNull() ? temp.TIC_Reinspected_By_Id.ToString("X16") : string.Empty;
                inspection.reinspectedByUserName = temp.TIC_Reinspected_By_Name;

                inspection.inspectionNotes = temp.Notes;
                inspection.correctiveActionDocLocation = temp.TIC_Corr_Action_Doc_Location;
                inspection.inspectionTemplateName = temp.inspection_name;

                //inspection.dueDate = !temp.IsDate_CompleteNull() ? Convert.ToString(temp.Date_Complete) : string.Empty;
                inspection.scheduledDate = !temp.Isscheduled_dateNull() ? Convert.ToString(temp.scheduled_date) : string.Empty;
                inspection.reinspectionCompleteDate = !temp.Istic_reinspection_complete_dateNull() ? Convert.ToString(temp.tic_reinspection_complete_date) : string.Empty;
                inspection.reinspectionDueDate = !temp.Istic_reinspection_due_dateNull() ? Convert.ToString(temp.tic_reinspection_due_date) : string.Empty;


                
                
                PivotalED.InspectionStepDataTable inspItems
                    = dal.GetInspectionStepsByInspectionId(inspectionIdInt);

                foreach (PivotalED.InspectionStepRow item in inspItems)
                {
                    InspectionItem inspItem = new InspectionItem();
                    inspItem.categoryDesc = item.category_desc;
                    //Do check for null since we want to allow null to be a valid value to be returned to the website
                    inspItem.acknowledgeStatus = item.tic_acknowledgement_status;
                    
                    inspItem.itemDescription = item.description;
                    if (item.Isstep_ordinalNull())
                    { inspItem.itemOrdinal = 99; }
                    else
                    { inspItem.itemOrdinal = item.step_ordinal; }
                                        
                    inspItem.categoryOrdinal = item.category_ordinal;
                    inspItem.inspectionId = inspectionId;
                    inspItem.inspectionItemId = item.inspection_step_id_int.ToString("X16");
                    inspItem.categoryId = item.category_id_int.ToString("X16");
                    inspectionItemList.Add(inspItem);
                    
                    //TO-DO: May need to sort.  Test this.
                    //inspectionItemList.Sort(people.Sort(delegate(InspectionItem i1, InspectionItem i2) { return i1.; });
                }

                //Get Misc category and set on inspectionobject
                MiscellaneousCategory miscCat = new MiscellaneousCategory();
                PivotalED.MiscellaneousCategoryDataTable miscDataTable
                    = dal.GetMiscellaneousCategory(inspectionIdInt);

                foreach (PivotalED.MiscellaneousCategoryRow miscRow in miscDataTable)
                {
                    miscCat.categoryId = miscRow.category_id_int.ToString("X16");
                    miscCat.description = miscRow.category_desc;
                    miscCat.ordinal = miscRow.ordinal;
                    miscCat.inspectionId = miscRow.inspection_id_int.ToString("X16");
                    inspection.miscCategory = miscCat;
                }



            }

            List<InspectedScopeItem> scopeItemList = new List<InspectedScopeItem>();
            //2) Get Inspected Scope item and exploded Units for each selected item
            PivotalED.ScopeItemsDataTable scopeItems
                = dal.GetScopeItemsByInspectionId(inspectionIdInt);

            foreach (PivotalED.ScopeItemsRow si in scopeItems)
            {
                //Build InspectedScopeItem object
                InspectedScopeItem sitem = new InspectedScopeItem();

                sitem.inspectionScope = si.Inspection_Scope;
                sitem.phaseName = si.phase_nbr;
                sitem.inspectionTypeId = si.inspection_type_id_int.ToString("X16");
                sitem.projectId = si.construction_project_id_int.ToString("X16");
                sitem.scopeItemNumber = TypeConvert.ToString(si.scope_item_nbr);
                sitem.inspectionId = inspectionId;
                sitem.inspectedScopeItemId = si.scope_item_id.ToString("X16");

                List<string> unitList = new List<string>();
                //For each scope item (E.g. Building) get the units associated with building
                PivotalED.ExplodedUnitsDataTable units
                    = dal.GetExplodedUnitsByBuilding(si.construction_project_id_int, si.inspection_type_id_int,
                        si.phase_nbr, si.scope_item_nbr);

                //Add units for each Inspection Scope item record
                foreach (PivotalED.ExplodedUnitsRow unitRow in units)
                {
                    unitList.Add(unitRow.unit);
                }

                //Add Exploded units to scope item array
                sitem.units = unitList.ToArray();
                //Add scope item list to Inspection Scope item object
                scopeItemList.Add(sitem);

            }

            //Get QA documents
            List<QADocument> docList = new List<QADocument>();
            //Get documents for inspection
            PivotalED.QADocumentsDataTable docs
                = dal.GetQADocumentsByInspectionId(inspectionIdInt);

            foreach (PivotalED.QADocumentsRow docRow in docs)
            {
                QADocument doc = new QADocument();
                doc.inspectionId = inspectionId;
                doc.documentDesc = docRow.document_description;
                doc.documentPath = docRow.qa_document_path;
                doc.qaDocumentId = docRow.qa_documents_id_int.ToString("X16");
                docList.Add(doc);
            }

            //Get QA Links
            List<QAWeblinks> linkList = new List<QAWeblinks>();
            PivotalED.QAWeblinksDataTable links
                = dal.GetQAWeblinksByInspectionId(inspectionIdInt);

            foreach (PivotalED.QAWeblinksRow linkRow in links)
            {
                QAWeblinks link = new QAWeblinks();
                link.inspectionId = inspectionId;
                link.url = linkRow.url;
                link.urlDesc = linkRow.url_desc;
                link.qaWebLinkId = linkRow.tic_qa_weblinks_id_int.ToString("X16");
                linkList.Add(link);
            
            }
            
            //3) Build Object and send to caller (Webservice)
            inspection.qaDocs = docList.ToArray();
            inspection.qaWeblinks = linkList.ToArray();
            inspection.inspectionItems = inspectionItemList.ToArray();
            inspection.inspectedScopeItems = scopeItemList.ToArray();

            //return inspection payload to service
            return inspection;
        }

        /// <summary>
        /// This method will 
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public InspectionListWrapper GetInspectionList(string userLogin, string status, CompanyType type)
        {
            //TO-DO: 
            //1) build filter wrapper
            //Set up objects
            ProjectFilter[] projFilterObjArr;
            PhaseFilter[] phaseFilterObjArr;
            InspectionTypeFilter[] inspTypeFilterObjArr;
            InspectionListItem[] inspectionListObjArr;
            InspectionListWrapper wrapper = new InspectionListWrapper();

            //Use Factory to get access to DAL
            dal = QADataAccessFactory.GetQADataAccess();

            //Call to Data Access Layer            
            PivotalED.ConstructionProjectFilterDataTable projFilterDataTable = dal.GetConstructionFilterForUser(userLogin);
            PivotalED.PhaseFilterDataTable phaseFilterDataTable = dal.GetPhaseFilterForUser(userLogin);
            PivotalED.InspTemplateFilterDataTable inspTypeFilterDataTable;

            //AM2011.02.08 - Changed during IT Testing
            if (type == CompanyType.Inspector)
            {
                inspTypeFilterDataTable = dal.GetInspectionTypesForInspectorLogin(userLogin);
            }
            else
            {
                inspTypeFilterDataTable = dal.GetInspTemplateFilterForUser(userLogin);
            }
            
            PivotalED.InspectionStatusesDataTable inspStatusesDataTable = dal.GetInspectionStatuses();
            PivotalED.InspectionListDataTable inspDataTable;

            //Following code was added to check to see whether an inspector or builder is logged in
            //different query will need to e executed for different login type
            int companyIdInt = 0;
            PivotalED.ContactDataTable cntDataTable = dal.GetContactByLogin(userLogin);
            foreach (PivotalED.ContactRow cntRow in cntDataTable)
            {
                companyIdInt = cntRow.company_id_int;
            }

            if (type == CompanyType.Inspector && companyIdInt != 0)
            {
                if (!String.IsNullOrEmpty(status))
                {
                    inspDataTable = dal.GetInspectionListByCompanyAndStatus(companyIdInt, status);
                }
                else
                {
                    inspDataTable = dal.GetInspectionListByCompanyId(companyIdInt);
                }
            }
            else
            {
                //check to see if we load by status or wildcard
                if (!String.IsNullOrEmpty(status))
                {
                    inspDataTable = dal.GetInspectionListByLoginAndStatus(userLogin, status);
                }
                else
                {
                    inspDataTable = dal.GetInspectionListByLogin(userLogin);
                }
            }
            //List to store scheduled inspections (pass by ref)
            List<InspectionListItem> InspListList = new List<InspectionListItem>();
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
                projFilterList.Add(projFilter);
            }

            //Now let's populate the Phase Filter
            foreach (PivotalED.PhaseFilterRow phaseFilterRow in phaseFilterDataTable)
            {
                PhaseFilter phaseFilter = new PhaseFilter();
                phaseFilter.projectId = phaseFilterRow.tic_construction_project_id_int.ToString("X16");
                phaseFilter.PhaseName = phaseFilterRow.construction_phase_number;
                phaseFilterList.Add(phaseFilter);
            }

            //Now populate template/Inspection Type list defined at the project level
            foreach (PivotalED.InspTemplateFilterRow inspTypeFilterRow in inspTypeFilterDataTable)
            {
                InspectionTypeFilter inspTypeFilter = new InspectionTypeFilter();
                inspTypeFilter.projectId = inspTypeFilterRow.tic_construction_project_id_int.ToString("X16");
                inspTypeFilter.InspectionType = inspTypeFilterRow.date_description;
                inspTypeFilter.phaseName = null;
                inspTypeFilter.inspectionTypeId = inspTypeFilterRow.tic_inspection_type_id_int.ToString("X16");
                inspTypeFilterList.Add(inspTypeFilter);
            }
            
            //Populate Inspection Statuses
            List<string> statusList = new List<string>();
            foreach (PivotalED.InspectionStatusesRow statusRow in inspStatusesDataTable)
            {
                statusList.Add(statusRow.rn_descriptor);
            }
            

            //build inspection list
            foreach (PivotalED.InspectionListRow inspListRow in inspDataTable)
            {
                InspectionListItem listItem = new InspectionListItem();
                listItem.inspectionId = inspListRow.inspection_id_int.ToString("X16");
                listItem.projectId = inspListRow.tic_construction_project_id_int.ToString("X16");
                listItem.projectName = inspListRow.tic_construction_project_name;
                listItem.phaseNbr = inspListRow.TIC_Construction_Phase;
                listItem.inspectionScope = inspListRow.tic_scope;
                listItem.status = inspListRow.status;
                listItem.inspectionTypeId = !inspListRow.Istic_inspection_type_id_intNull() ? inspListRow.tic_inspection_type_id_int.ToString("X16") : null;
                listItem.inspectionTypeName = inspListRow.inspection_type_name;
                listItem.dueDate = !inspListRow.Isdue_dateNull() ? TypeConvert.ToString(inspListRow.due_date) : null;
                listItem.inspectionCompleteDate = !inspListRow.Isdate_completeNull() ? TypeConvert.ToString(inspListRow.date_complete) : null;
                listItem.lastSavedById = !inspListRow.Islast_saved_by_idNull() ? inspListRow.last_saved_by_id.ToString("X16") : null;
                listItem.lastSavedByUserName = inspListRow.last_saved_by_user_name;
                //Foreach inspection list item 
                PivotalED.ScopeItemsDataTable scopeItemsDataTable = dal.GetScopeItemsByInspectionId(inspListRow.inspection_id_int);
                List<string> scopeItemsList = new List<string>();
                foreach (PivotalED.ScopeItemsRow scopeRow in scopeItemsDataTable)
                {
                    scopeItemsList.Add(scopeRow.scope_item_nbr);
                }
                //Add inspection scope items
                listItem.scopeItemNbrs = scopeItemsList.ToArray();
                //Set list with inspectionlist item
                InspListList.Add(listItem);
                
            }
            //Set Scheduled inpsections object array
            inspectionListObjArr = InspListList.ToArray();
            //Filter to object arrays
            projFilterObjArr = projFilterList.ToArray();
            phaseFilterObjArr = phaseFilterList.ToArray();
            inspTypeFilterObjArr = inspTypeFilterList.ToArray();

            //set filter wrapper
            FilterWrapper filtWrapper = new FilterWrapper();
            filtWrapper.inspectionTypeFilter = inspTypeFilterObjArr;
            filtWrapper.phaseFilter = phaseFilterObjArr;
            filtWrapper.projFilter = projFilterObjArr;
            filtWrapper.inspectionStatuses = statusList.ToArray();

            //Set payload and pass to service layer
            wrapper.filterWrapper = filtWrapper;
            wrapper.inspections = inspectionListObjArr;

            return wrapper;


        }

        /// <summary>
        /// This public method will provide the web with the status information for the inspection prior
        /// to performing any action on the inspection.
        /// </summary>
        /// <param name="inspectionId"></param>
        /// <returns></returns>
        public InspectionStatus GetInspectionStatus(string inspectionId, string userLogin)
        {
            IQADataAccess dal = QADataAccessFactory.GetQADataAccess();
            PivotalED.InspectionStatusDataTable inspStat
                = dal.GetInspectionStatus(Convert.ToInt32(inspectionId, 16));

            InspectionStatus status = new InspectionStatus();
            
            foreach (PivotalED.InspectionStatusRow inspRow in inspStat)
            {
                
                status.Status = inspRow.status;
                status.LastSavedByCompanyType = inspRow.company_Type;
                status.LastSavedByRole = inspRow.role_;
                status.LastSavedByName = inspRow.last_saved_by_user_name;
                status.LastSavedById = !inspRow.Islast_saved_by_idNull() ? inspRow.last_saved_by_id.ToString("X16") : string.Empty;
                
            }

            //AM2011.02.25 - added to address security hole when opening inspection fromURL
            //Check whether or not user is authorized to view inspection and set flag
            //Use Factory to get access to DAL
            dal = QADataAccessFactory.GetQADataAccess();
            PivotalED.InspectionListDataTable inspDataTable;
            //Following code was added to check to see whether an inspector or builder is logged in
            //different query will need to e executed for different login type
            int companyIdInt = 0;
            string companyType = string.Empty;
            PivotalED.ContactDataTable cntDataTable = dal.GetContactByLogin(userLogin);
            foreach (PivotalED.ContactRow cntRow in cntDataTable)
            {
                companyIdInt = cntRow.company_id_int;
                companyType = cntRow.company_type;
                
            }

            if (companyType == CompanyType.Inspector.ToString() && companyIdInt != 0)
            {
                inspDataTable 
                    = dal.GetInspectionByCompanyAndInspectionId(companyIdInt, Convert.ToInt32(inspectionId, 16));
            }
            else
            {
                inspDataTable 
                    = dal.GetInspectionByLoginAndInspectionId(userLogin, Convert.ToInt32(inspectionId, 16));

            }

            bool isUserAuthorized = false;

            //Check if inspector list count is greater than 1, if so then return true
            foreach (PivotalED.InspectionListRow listRow in inspDataTable)
            {
                isUserAuthorized = true;
            }

            //Set Authorization flag
            status.isUserAuthorizedForInspection = isUserAuthorized;

            return status;

        }

       


        #endregion

        #region PBS Communication (IQAPBSController)

        /// <summary>
        /// This method will create the initial inspection record when the user
        /// clicks teh "Create Inspection Report" button from the QA App
        /// </summary>
        /// <param name="inspObj"></param>
        /// <returns></returns>
        public string InsertNewInspectionIntoPivotal(string projectId, string phaseNumber, 
            string inspTypeId, string scope, string createdById, string inspectorId, 
            string[] scopeItems)
        {
            //TO-DO
            //1) With as minimal information as possible create a new Inspection record
            //and let the ASR handle the update of units per building
            Inspection insp = new Inspection();
            insp.projectId = projectId;
            insp.phaseName = phaseNumber;
            insp.inspectionTypeId = inspTypeId;
            insp.inspectionScope = scope;
            insp.createdById = createdById;
            insp.inspectorId = inspectorId;
            //2) Create Inspection Scope secondaries (Mapp data selected to secondary object (PBS XML)
            List<InspectedScopeItem> inspScopeItems = new List<InspectedScopeItem>();

            foreach (string s in scopeItems)
            {
                InspectedScopeItem inspScopeItem = new InspectedScopeItem();
                inspScopeItem.projectId = projectId;
                inspScopeItem.inspectionTypeId = inspTypeId;
                inspScopeItem.phaseName = phaseNumber;
                inspScopeItem.scopeItemNumber = s.ToString();
                inspScopeItems.Add(inspScopeItem);
            }
            //Set scope items on inspection object
            insp.inspectedScopeItems = inspScopeItems.ToArray();

            //3) Send to PBS to do the following :
            //   - Create Inspection Items and Categories from Template upon initial record creation
            //   - Also, create default Miscellaneous category
            //   - Update Tract Time records to In Progress
            //   - return Inspection record id  
            string newInspectionId = InsertInspection(pivotalSystemName, insp, null);
            //Pass back new pivotal id
            return newInspectionId;
        }

        /// <summary>
        /// Controller method that will be called from service
        /// </summary>
        /// <param name="inspectionData"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Inspection UpdateExistingInspection(Inspection inspectionData, ActionForInspection action)
        {
            //Update Inspection will update pivotal record, ASR will handle all state transition logic  
            inspectionData.websiteAction = action.ToString();
            UpdateInspection(pivotalSystemName, inspectionData.inspectionId, inspectionData, null);
            
            Inspection inspObj = new Inspection();
            //If anything but Save and close reload the record and send back to web
            if (action != ActionForInspection.SaveAndClose)
            {
                inspObj = LoadExistingInspection(inspectionData.inspectionId, LoadActionsForInspection.OpenReadOnly, 
                    inspectionData.lastSavedById);
            }
            return inspObj;
        }

        /// <summary>
        /// Controller method that will delete the inspection via ASR in order to 
        /// perform cascade delete and rollback of track time records
        /// </summary>
        /// <param name="inspectionId"></param>
        public void DeleteExistingInspection(string inspectionId)
        {
            this.Delete(pivotalSystemName, inspectionId, null);
        }

        #endregion
        
        #region Private Controller Methods

        /// <summary>
        /// class method which will get the inspections for the user at the lot aggregate
        /// based on the Scope of the Project, userlogin and project name
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        private void GetLotScheduledInspectionsForUser(string userLogin, 
            string projectName, ref List<ScheduledInspection> siList, CompanyType type)
        {
            //Get Scheduled Inspections for Construction Project at the lot level
            PivotalED.ScheduledInspectionsDataTable lotSIDataTable;

            if(type == CompanyType.Inspector)
            {              

                lotSIDataTable = dal.GetLotInspectionsByInspectorLogin(userLogin, projectName);
            }
            else
            {
                lotSIDataTable = dal.GetLotScheduledInspectionsForUserLogin(userLogin, projectName);
            }
            foreach (PivotalED.ScheduledInspectionsRow siLotRow in lotSIDataTable)
            {
                ScheduledInspection si = new ScheduledInspection();
                si.projectId = siLotRow.tic_construction_project_int.ToString("X16");
                si.projectName = siLotRow.tic_construction_project_name;
                si.phaseName = siLotRow.phase_name;
                si.lotRecord = siLotRow.lot_number;
                si.inspectionType = siLotRow.date_description;
                si.inspectionTypeId = siLotRow.tic_construction_dt_lookup_id_int.ToString("X16");
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
            string projectName, ref List<ScheduledInspection> siList, CompanyType type)
        {
            //Get Scheduled Inspections for Construction Project at the lot level
            PivotalED.ScheduledInspectionsDataTable lotSIDataTable;

            if(type == CompanyType.Inspector)
            {
                lotSIDataTable = dal.GetUnitInspectionsByInspectorLogin(userLogin, projectName);
            }
            else
            {
                lotSIDataTable = dal.GetUnitScheduledInspectionsForUserLogin(userLogin, projectName);
            }
            foreach (PivotalED.ScheduledInspectionsRow siLotRow in lotSIDataTable)
            {
                ScheduledInspection si = new ScheduledInspection();
                si.projectId = siLotRow.tic_construction_project_int.ToString("X16");
                si.projectName = siLotRow.tic_construction_project_name;
                si.phaseName = siLotRow.phase_name;
                si.lotRecord = siLotRow.Unit;
                si.inspectionType = siLotRow.date_description;
                si.inspectionTypeId = siLotRow.tic_construction_dt_lookup_id_int.ToString("X16");
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
            string projectName, ref List<ScheduledInspection> siList, CompanyType type)
        {
            //Get Scheduled Inspections for Construction Project at the building level
            PivotalED.ScheduledInspectionsDataTable lotSIDataTable;

            if (type == CompanyType.Inspector)
            {
                lotSIDataTable = dal.GetBuildingInspectionsByInspectorLogin(userLogin, projectName);
            }
            else
            {
                //Get Scheduled Inspections for Construction Project at the lot level
                lotSIDataTable = dal.GetBuildingScheduledInspectionsForUserLogin(userLogin, projectName);
            }
            foreach (PivotalED.ScheduledInspectionsRow siLotRow in lotSIDataTable)
            {
                ScheduledInspection si = new ScheduledInspection();
                si.projectId = siLotRow.tic_construction_project_int.ToString("X16");
                si.projectName = siLotRow.tic_construction_project_name;
                si.phaseName = siLotRow.phase_name;
                si.lotRecord = siLotRow.building;
                si.inspectionType = siLotRow.date_description;
                si.inspectionTypeId = siLotRow.tic_construction_dt_lookup_id_int.ToString("X16");
                si.scheduledDate = TypeConvert.ToString(siLotRow.Scheduled_Date);
                siList.Add(si);
            }
        }

        private string GetCurrentInspectionStatus(string inspectionId)
        {
            IQADataAccess dal = QADataAccessFactory.GetQADataAccess();
            PivotalED.InspectionStatusDataTable inspStat
                = dal.GetInspectionStatus(Convert.ToInt32(inspectionId, 16));

            string strStatus = string.Empty;
            foreach (PivotalED.InspectionStatusRow inspRow in inspStat)
            {
                strStatus = inspRow.status;
            }
            return strStatus;

        }

        /// <summary>
        /// This method will provide the business logic to determine the
        /// correct status of the inspection based on the action of the
        /// QA website.
        /// </summary>
        /// <param name="currentStatus"></param>
        /// <returns></returns>
        private string ManageInspectionStatus(string currentStatus, string action, 
            bool correctActionReq)
        {
            //TO-DO: Set send email boolean for state transitions which trigger an 
            //email (Submitted)
            
            string strStatus = currentStatus;
            //Based on defined status transitions set the status on the inspection based
            //on the action taken from the web
            if (action == LoadActionsForInspection.OpenForEdit.ToString())
            {
                if (currentStatus == QAConstants.strcIN_PROCESS)
                {
                    strStatus = QAConstants.strcDATA_ENTRY;
                }
                else if (currentStatus == QAConstants.strcAWAITING_APPROVAL)
                {
                    strStatus = QAConstants.strcAPPROVING;
                }
                else if (currentStatus == QAConstants.strcAWAITING_FOLLOW_UP)
                {
                    strStatus = QAConstants.strcFOLLOW_UP_DATA_ENTRY;
                }
                else if (currentStatus == QAConstants.strcFOLLOW_UP_IN_PROCESS)
                {
                    strStatus = QAConstants.strcFOLLOW_UP_DATA_ENTRY;
                }
                else if (currentStatus == QAConstants.strcAWAITING_FOLLOW_UP_APPROVAL)
                {
                    strStatus = QAConstants.strcAWAITING_FOLLOW_UP;
                }
            }
            else if (action == LoadActionsForInspection.OpenReadOnly.ToString())
            {
                strStatus = currentStatus;
            }
            else if (action == ActionForInspection.Save.ToString())
            {
                //Save doesn't change the status so just pass back the current status
                strStatus = currentStatus;
            }
            else if (action == ActionForInspection.SaveAndClose.ToString())
            {
                //Save and Close logic will vary based on the current status
                if (currentStatus == QAConstants.strcDATA_ENTRY)
                {
                    strStatus = QAConstants.strcIN_PROCESS;
                }
                else if (currentStatus == QAConstants.strcFOLLOW_UP_DATA_ENTRY)
                {
                    strStatus = QAConstants.strcFOLLOW_UP_IN_PROCESS;
                }
            }
            else if (action == ActionForInspection.Submit.ToString())
            {
                //Submit logic will vary based on current status
                if (currentStatus == QAConstants.strcDATA_ENTRY)
                {
                    strStatus = QAConstants.strcAWAITING_APPROVAL;                    
                }
                else if (currentStatus == QAConstants.strcAPPROVING)
                {
                    //Check to see if corrective action flag is set
                    if (correctActionReq)
                    {
                        strStatus = QAConstants.strcAWAITING_FOLLOW_UP;                      
                    }
                    else
                    {
                        strStatus = QAConstants.strcAPPROVED;                        
                    }
                }
                else if (currentStatus == QAConstants.strcFOLLOW_UP_DATA_ENTRY)
                {
                    strStatus = QAConstants.strcAWAITING_FOLLOW_UP_APPROVAL;
                }
                else if (currentStatus == QAConstants.strcAPPROVING_FOLLOW_UP)
                {
                    strStatus = QAConstants.strcFOLLOW_UP_APPROVED;
                }

            }

            return strStatus;
        }


        #endregion

        #region Private PBS Controller Methods

        #region Inspection (C, U, D)

        /// <summary>
        /// This method will map the Inspection object passed in from the website and call the PBS
        /// with the insert XML command
        /// </summary>
        /// <param name="System"></param>
        /// <param name="inspectionData"></param>
        /// <param name="CommandParameters"></param>
        /// <returns></returns>
        private string InsertInspection(string System, Inspection inspectionData, string[] CommandParameters)
        {
            ExceptionHandler exHandler = null;
            string errMsg = string.Empty;
            if (System.Equals(""))
            {                               
                exHandler = new ExceptionHandler();
                errMsg = genData.GetString("MSG_EMPTY_SYSTEM_NAME");
                throw exHandler.RaiseException("", errMsg, "", FaultCode.Client);
            }
            if (inspectionData == null)
            {
                exHandler = new ExceptionHandler();
                errMsg = "TIC_Int_QA_InspectionData" + genData.GetString("MSG_NULL");
                throw exHandler.RaiseException("", errMsg, "", FaultCode.Client);
            }
            // Default return
            string tempInsert = "";
            PBSComms pbsComms = new PBSComms();

            try
            {
                // Construct PBS XML
                pbsComms.AddRequestHeader(System);
                pbsComms.AddRequestCommandStart(PBSComms.CommandType.Insert, ref CommandParameters, QAConstants.INSPECTION_ACTIVE_FORM_NAME);
                CreateInspectionActiveFormFieldXML(ref pbsComms, ref inspectionData, false);
                pbsComms.AddRequestCommandEnd(PBSComms.CommandType.Insert);

                // Execute PBS command and branch for success
                if (pbsComms.DoPBSRequest())
                {
                    // Success - return record id
                    tempInsert = pbsComms.GetResponseNewRecordId();
                }
                else
                    RaisePBSError(pbsComms); // Request failed - Raise the PBS error 

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return tempInsert;
        }

        /// <summary>
        /// This method will update inspection record and associated inspection items by executing a 
        /// PBS XML Command.  Additional ASR logic will be attached to this method
        /// </summary>
        /// <param name="System"></param>
        /// <param name="RecordId"></param>
        /// <param name="inspectionData"></param>
        /// <param name="CommandParameters"></param>
        private void UpdateInspection(string System, string RecordId, Inspection inspectionData, string[] CommandParameters)
        {
            ExceptionHandler exHandler = null;
            string errMsg = string.Empty;
            if (System.Equals(""))
            {
                exHandler = new ExceptionHandler();
                errMsg = genData.GetString("MSG_EMPTY_SYSTEM_NAME");
                throw exHandler.RaiseException("", errMsg, "", FaultCode.Client);
            }
            if (RecordId.Equals(""))
            {
                exHandler = new ExceptionHandler();
                errMsg = genData.GetString("MSG_EMPTY_RECORDID");
                throw exHandler.RaiseException("", errMsg, "", FaultCode.Client);
            }
            if (inspectionData == null)
            {
                exHandler = new ExceptionHandler();
                errMsg = "TIC_Int_QA_InspectionData" + genData.GetString("MSG_NULL");
                throw exHandler.RaiseException("", errMsg, "", FaultCode.Client);
            }
            PBSComms pbsComms = new PBSComms();

            try
            {
                // Construct PBS XML
                pbsComms.AddRequestHeader(System);
                pbsComms.AddRequestCommandStart(PBSComms.CommandType.Update, ref CommandParameters, QAConstants.INSPECTION_ACTIVE_FORM_NAME);
                pbsComms.AddRequestRecordSource(RecordId);
                CreateInspectionActiveFormFieldXML(ref pbsComms, ref inspectionData, true);
                pbsComms.AddRequestCommandEnd(PBSComms.CommandType.Update);

                // Execute PBS command and branch for success
                if (!(pbsComms.DoPBSRequest()))
                {
                    // Request failed - Raise the PBS error 
                    RaisePBSError(pbsComms);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdateInspectionStatus(string System, string RecordId, string currentStatus, 
            string qaAction, string[] CommandParameters, string lastSavedById)
        {
            ExceptionHandler exHandler = null;
            string errMsg = string.Empty;
            if (System.Equals(""))
            {
                exHandler = new ExceptionHandler();
                errMsg = genData.GetString("MSG_EMPTY_SYSTEM_NAME");
                throw exHandler.RaiseException("", errMsg, "", FaultCode.Client);
            }
            if (RecordId.Equals(""))
            {
                exHandler = new ExceptionHandler();
                errMsg = genData.GetString("MSG_EMPTY_RECORDID");
                throw exHandler.RaiseException("", errMsg, "", FaultCode.Client);
            }
            
            PBSComms pbsComms = new PBSComms();

            try
            {
                // Construct PBS XML
                pbsComms.AddRequestHeader(System);
                pbsComms.AddRequestCommandStart(PBSComms.CommandType.Update, ref CommandParameters, QAConstants.INSPECTION_ACTIVE_FORM_NAME);
                pbsComms.AddRequestRecordSource(RecordId);
                CreateInspectionStatusActiveFormFieldXML(ref pbsComms, currentStatus, qaAction, lastSavedById);
                pbsComms.AddRequestCommandEnd(PBSComms.CommandType.Update);

                // Execute PBS command and branch for success
                if (!(pbsComms.DoPBSRequest()))
                {
                    // Request failed - Raise the PBS error 
                    RaisePBSError(pbsComms);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        /// <summary>
        /// This method will perform a delete on the inspection record from the web.  In order to 
        /// release the tract time records additional ASR logic will be used to unset these tract time reocrds
        /// back to pending.
        /// </summary>
        /// <param name="System"></param>
        /// <param name="RecordId"></param>
        /// <param name="CommandParameters"></param>
        private void Delete(string System, string RecordId, string[] CommandParameters)
        {
            ExceptionHandler exHandler = null;
            string errMsg = string.Empty;
            if (System.Equals(""))
            {
                exHandler = new ExceptionHandler();
                errMsg = genData.GetString("MSG_EMPTY_SYSTEM_NAME");
                throw exHandler.RaiseException("", errMsg, "", FaultCode.Client);
            }
            if (RecordId.Equals(""))
            {
                exHandler = new ExceptionHandler();
                errMsg = genData.GetString("MSG_EMPTY_RECORDID");
                throw exHandler.RaiseException("", errMsg, "", FaultCode.Client);
            }
            PBSComms pbsComms = new PBSComms();
            try
            {
                // Construct PBS XML
                pbsComms.AddRequestHeader(System);
                pbsComms.AddRequestCommandStart(PBSComms.CommandType.Delete, ref CommandParameters, QAConstants.INSPECTION_ACTIVE_FORM_NAME);
                pbsComms.AddRequestRecordId(RecordId);
                pbsComms.AddRequestCommandEnd(PBSComms.CommandType.Delete);

                // Execute PBS command and branch for success
                if (!(pbsComms.DoPBSRequest()))
                {
                    // Request failed - Raise the PBS error 
                    RaisePBSError(pbsComms);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
	
        /// <summary>
        /// This method will build the PBS XML from the data object passed in from the 
        /// QA Website application
        /// </summary>
        /// <param name="PBSComms"></param>
        /// <param name="Data"></param>
        private void CreateInspectionActiveFormFieldXML(ref PBSComms PBSComms, ref Inspection Data, bool isUpdate)
        {
            try
            {
                PBSComms.AddSegmentUpdateStart("New Segment");
                PBSComms.AddFieldUpdate("TIC_Construction_Project_Id", Data.projectId);
                PBSComms.AddFieldUpdate("TIC_Construction_Phase", Data.phaseName);
                PBSComms.AddFieldUpdate("TIC_Inspection_Type_Id", Data.inspectionTypeId);
                PBSComms.AddFieldUpdate("Inspection_Template_Id", Data.inspectionTemplateId);
                PBSComms.AddFieldUpdate("TIC_Corrective_Action_Status", Data.correctiveActionStatus);
                PBSComms.AddFieldUpdate("TIC_Inspector_ID", Data.inspectorId);
                PBSComms.AddFieldUpdate("TIC_Created_By_Id", Data.createdById);
                PBSComms.AddFieldUpdate("TIC_Supervisor_Id", Data.supervisorId);
                PBSComms.AddFieldUpdate("TIC_Sign_Off_User_Id", Data.signOffUserId);
                PBSComms.AddFieldUpdate("TIC_Esc_User_Sign_Off_Id", Data.escalationSignOffUserId);
                PBSComms.AddFieldUpdate("TIC_Reinspection_Due_Date", FormatPivotalDate(Data.reinspectionDueDate));
                PBSComms.AddFieldUpdate("TIC_Reinspection_Complete_Date", FormatPivotalDate(Data.reinspectionCompleteDate));
                PBSComms.AddFieldUpdate("Notes", Data.inspectionNotes);
                PBSComms.AddFieldUpdate("Scheduled_Date", FormatPivotalDate(Data.scheduledDate));
                PBSComms.AddFieldUpdate("Date_Assigned", FormatPivotalDate(Data.dueDate));
                PBSComms.AddFieldUpdate("Date_Complete", FormatPivotalDate(Data.inspectedDateTime));
                PBSComms.AddFieldUpdate("Inspection_Name", Data.inspectionTemplateName);
                PBSComms.AddFieldUpdate("TIC_Inspection_Status", Data.inspectionStatus);
                PBSComms.AddFieldUpdate("TIC_Scope", Data.inspectionScope);
                PBSComms.AddFieldUpdate("Disconnected_1_1_22", Data.websiteAction);
                PBSComms.AddFieldUpdate("TIC_Corr_Action_Doc_Location", Data.correctiveActionDocLocation);
                PBSComms.AddFieldUpdate("TIC_Submitted_By_Id", Data.submittedByUserId);
                PBSComms.AddFieldUpdate("TIC_Reinsp_Submitted_By_Id", Data.reinspectionSubmittedById);
                PBSComms.AddFieldUpdate("TIC_Last_Saved_By", Data.lastSavedById);
                PBSComms.AddFieldUpdate("TIC_Reinspected_By_Id", Data.reinspectedByUserId);
                PBSComms.AddSegmentUpdateEnd();

                //Scope Items are created when the new inspection is created.  There is currently
                //no requirement to update scope records at this time
                if (!isUpdate)
                {
                    //New Inspection record so need to create the following:
                    //- Inspection Scope Items
                    //- QA Documents
                    //- QA Weblinks
                    if (Data.inspectedScopeItems != null)
                    {
                        foreach (InspectedScopeItem si in Data.inspectedScopeItems)
                        {
                            PBSComms.TICAddSecondarySegmentUpdateStart("Inspected Scope Items");
                            PBSComms.AddFieldUpdate("Construction_Project_Id", TypeConvert.ToString(si.projectId));
                            PBSComms.AddFieldUpdate("Phase_Nbr", TypeConvert.ToString(si.phaseName));
                            PBSComms.AddFieldUpdate("Inspection_Type_Id", TypeConvert.ToString(si.inspectionTypeId));
                            PBSComms.AddFieldUpdate("Scope_Item_Nbr", TypeConvert.ToString(si.scopeItemNumber));
                            PBSComms.AddFieldUpdate("Inspection_Scope", TypeConvert.ToString(Data.inspectionScope));
                            PBSComms.TICAddSecondarySegmentUpdateEnd();
                        }
                    }
                    
                }
                else
                {

                    if (Data.qaDocs != null)
                    {
                        foreach (QADocument doc in Data.qaDocs)
                        {
                            if (doc.qaDocumentId != null)
                            {
                                //Update secondary record
                                PBSComms.TICAddSecondarySegmentUpdateStart("QA Documents");
                                PBSComms.AddRequestRecordSource(doc.qaDocumentId);
                                PBSComms.AddFieldUpdate("Inspection_Id", Data.inspectionId);
                                PBSComms.AddFieldUpdate("QA_Document_Path", doc.documentPath);
                                PBSComms.AddFieldUpdate("Document_Description", doc.documentDesc);
                                if (doc.deleteDocument != null)
                                {
                                    if (doc.deleteDocument == true)
                                    { PBSComms.AddFieldUpdate("Ready_For_Delete", "1"); }
                                }

                                PBSComms.TICAddSecondarySegmentUpdateEnd();
                            }
                            else
                            {
                                //Insert secondary record
                                PBSComms.TICAddSecondarySegmentUpdateStart("QA Documents");
                                PBSComms.AddFieldUpdate("Inspection_Id", Data.inspectionId);
                                PBSComms.AddFieldUpdate("QA_Document_Path", doc.documentPath);
                                PBSComms.AddFieldUpdate("Document_Description", doc.documentDesc);
                                PBSComms.TICAddSecondarySegmentUpdateEnd();
                            }
                        }
                    }

                    if (Data.qaWeblinks != null)
                    {
                        foreach (QAWeblinks link in Data.qaWeblinks)
                        {
                            if (link.qaWebLinkId != null)
                            {
                                //Update document
                                PBSComms.TICAddSecondarySegmentUpdateStart("QA Weblinks");
                                PBSComms.AddRequestRecordSource(link.qaWebLinkId);
                                PBSComms.AddFieldUpdate("Inspection_Id", Data.inspectionId);
                                PBSComms.AddFieldUpdate("URL", link.url);
                                PBSComms.AddFieldUpdate("URL_Desc", link.urlDesc);
                                if (link.deleteLink != null)
                                {
                                    if (link.deleteLink == true)
                                    {PBSComms.AddFieldUpdate("Ready_For_Delete", "1");}
                                }
                                
                                PBSComms.TICAddSecondarySegmentUpdateEnd();
                            }
                            else
                            {
                                //Delete document
                                PBSComms.TICAddSecondarySegmentUpdateStart("QA Weblinks");
                                PBSComms.AddFieldUpdate("Inspection_Id", Data.inspectionId);
                                PBSComms.AddFieldUpdate("URL", link.url);
                                PBSComms.AddFieldUpdate("URL_Desc", link.urlDesc);
                                PBSComms.TICAddSecondarySegmentUpdateEnd();
                            }
                        }
                    }
                    


                    //Update logic will update inspection items
                    if (Data.inspectionItems != null)
                    {
                        foreach (InspectionItem ii in Data.inspectionItems)
                        {
                            //If inspection item id is null, means a custom item was added and 
                            //needs to be created
                            if (ii.inspectionItemId != null)
                            {
                                //only need to update if acknowledgement is not null
                                if(ii.acknowledgeStatus != null)
                                {
                                    //build update secondary record XML
                                    PBSComms.TICAddSecondarySegmentUpdateStart("Inspection Checklist");
                                    PBSComms.AddRequestRecordSource(ii.inspectionItemId);
                                    PBSComms.AddFieldUpdate("TIC_Acknowledgement_Status", ii.acknowledgeStatus);
                                    PBSComms.TICAddSecondarySegmentUpdateEnd();
                                }
                            }
                            else
                            { 
                                
                                PBSComms.TICAddSecondarySegmentUpdateStart("Inspection Checklist");
                                if (ii.acknowledgeStatus != null)
                                {
                                   PBSComms.AddFieldUpdate("TIC_Acknowledgement_Status", ii.acknowledgeStatus);                                   
                                }
                                PBSComms.AddFieldUpdate("Description", TypeConvert.ToString(ii.itemDescription));
                                PBSComms.AddFieldUpdate("Working_Notes", TypeConvert.ToString(ii.workingNotes));
                                PBSComms.AddFieldUpdate("Inspection_Id", ii.inspectionId);
                                PBSComms.AddFieldUpdate("TIC_Insp_Category_Id", ii.categoryId);
                                PBSComms.AddFieldUpdate("TIC_Ordinal", TypeConvert.ToString(ii.itemOrdinal));
                                PBSComms.TICAddSecondarySegmentUpdateEnd();

                            }



                        }



                    }
                }
                
            }
            catch (Exception ex)
            {
                ExceptionHandler exHandler = new ExceptionHandler();
                throw exHandler.RaiseException("", ex.Message, "", FaultCode.Client);
            }
        }
        
        private void CreateInspectionStatusActiveFormFieldXML(ref PBSComms PBSComms, string status, string action, string lastSavedById)
        {
            //Now build update XML
            PBSComms.AddSegmentUpdateStart("New Segment");
            PBSComms.AddFieldUpdate("TIC_Inspection_Status", status);
            PBSComms.AddFieldUpdate("Disconnected_1_1_22", action);
            PBSComms.AddFieldUpdate("TIC_Last_Saved_By", lastSavedById);
            PBSComms.AddSegmentUpdateEnd();
        }

        #endregion

        private void RaisePBSError(PBSComms PBSComms)
        {
            try
            {
                int errorNumber = PBSComms.GetResponseErrorNumber();
                string errorText = PBSComms.GetResponseErrorText();
                string errorDetails = PBSComms.GetResponseErrorDetails();

                ExceptionHandler exHandler = new ExceptionHandler();
                throw exHandler.RaiseException("", errorText, "", FaultCode.Server);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Helper Methods

        public string FormatPivotalDate(string strDate)
        {
            if (!String.IsNullOrEmpty(strDate))
            {
                DateTime dt = TypeConvert.ToDateTime(strDate);
                strDate = dt.ToString("o");
            }
            else
            {
                strDate = null;
            }

            return strDate;
        }

        public string ValidatePivotalIdValue(int pivotalId)
        {
            if (pivotalId == 0)
            {
                return string.Empty;
            }
            else
            {
                return pivotalId.ToString("X16");
            }
        }

        #endregion

    }
}
