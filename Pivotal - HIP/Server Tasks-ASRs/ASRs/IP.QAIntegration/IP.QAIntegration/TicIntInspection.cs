using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;
using Pivotal.Interop.COMAdminLib;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Config;
using System.ComponentModel;
using System.Reflection;

namespace IP.QAIntegration
{
    public class TicIntInspection : IRFormScript
    {
        #region Class Vars

        IRSystem7 rSys;

        #endregion

        public enum NotificationTypes
        {
           
            [Description("Inspection Submitted")]
            InspectionSubmitted,
            [Description("Inspection Process Complete")]
            InspectionProcessComplete,
            [Description("Follow-up Inspection Required")]
            FollowupInspectionRequired,
            [Description("Follow-up Inspection Submitted")]
            FollowupInspectionSubmitted,
            [Description("Follow-up Inspection Process Complete")]
            FollowUpInspectionProcessComplete,
            [Description("Initial Inspection Requested")]
            InitialInspectionRequested
           
        
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
            Submit
        }


        #region IRFormScript Members

        /// <summary>
        /// Implemented for the QA Website Integration to handle new Inspection records
        /// created via the Website
        /// </summary>
        /// <param name="pForm"></param>
        /// <param name="Recordsets"></param>
        /// <param name="ParameterList"></param>
        /// <returns></returns>
        public object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            //New Inspection record override logic
            //Get incoming recordset
            object[] recordsetArray = (object[])Recordsets;
            Recordset rstInspection = (Recordset)recordsetArray[0];
            Recordset rstScopeItems = (Recordset)recordsetArray[1];
            object inspectionId = null;
            object templateId = null;
            
            //Get scope of this inspection
            string strScope = TypeConvert.ToString(rstInspection.Fields[modConstants.strfTIC_SCOPE].Value);

            //Check to make sure template has been configured for inspection type
            if(!DoesTemplateExistForInspectionType(rstInspection.Fields[modConstants.strfTIC_INSPECTION_TYPE_ID].Value, out templateId))
            {
                throw new PivotalApplicationException(modConstants.strcTEMPLATE_NOT_FOUND + rSys.IdToString(rstInspection
                    .Fields[modConstants.strfTIC_INSPECTION_TYPE_ID].Value));
            }            

            //TO-DO:
            //   - Default as much information on the inspection record
            rstInspection.Fields[modConstants.strfINSPECTION_TEMPLATE_ID].Value = templateId;
            SetInspectionDefaults(rstInspection);

            //Set teh Scope Items on the Inspection so that it can be displayed on SRL
            SetScopeItemsOnInspection(rstInspection, rstScopeItems);


            //   - Save inspection and get id
            inspectionId = pForm.DoAddFormData(Recordsets, ref ParameterList);
            rstInspection.Fields[modConstants.strfINSPECTION_ID].Value = inspectionId;

            //   - Create Inspection Items and Categories from Template upon initial record creation
            CreateTemplateItems(rstInspection);
                       
            //   - Update Tract Time records to In Progress
            UpdateTractTimeRecordStatus(strScope, rstScopeItems, inspectionId);
            
            //Create default miscellaneous category
            CreateDefaultMiscCategory(inspectionId);            

            //   - return Inspection record id  
            return inspectionId;
        }

        /// <summary>
        /// This method will be used to delete an existing inspection record from
        /// the QA Website.
        /// </summary>
        /// <param name="pForm"></param>
        /// <param name="RecordId"></param>
        /// <param name="ParameterList"></param>
        public void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            //TO-DO: Perform cascading delete and rollback of Tract Time records
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            
            //1) Rollback Tract-Time records to "Pending" or Null
            ReleaseTractTimeRecords(objLib, RecordId);
            //2) Cascade Delete all related records for Inspection
            PerformCascadeDeleteForInspection(objLib, RecordId);
            //3) let platform delete inspection record
            pForm.DoDeleteFormData(RecordId, ref ParameterList);
        }

        public void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {
            throw new NotImplementedException();
        }

        public object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            return pForm.DoLoadFormData(RecordId, ref ParameterList);
        }

        public object NewFormData(IRForm pForm, ref object ParameterList)
        {
            return pForm.DoNewFormData(ref ParameterList);
        }

        public void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset Recordset)
        {
            pForm.DoNewSecondaryData(SecondaryName, ref ParameterList, Recordset);
        }

        /// <summary>
        /// This method will be overrided and used to handle saves to an existing Inspection
        /// record from the QA Website.
        /// </summary>
        /// <param name="pForm"></param>
        /// <param name="Recordsets"></param>
        /// <param name="ParameterList"></param>
        public void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {           
            //Process Notification 
            object[] recordsetArray = (object[])Recordsets;
            Recordset rstInspection = (Recordset)recordsetArray[0];
            
            //Get QA Action 
            string strAction = GetDisconnected(rSys, pForm, rstInspection, modConstants.strdiscQA_ACTION,
                modConstants.strsegPRIMARY_SEGMENT);

            //Process workflow
            ProcessWorkflowStatus(rstInspection, strAction);

            //After all workflow is processed done, save the record
            pForm.DoSaveFormData(Recordsets, ref ParameterList);

        }

        /// <summary>
        /// Set System context for this AppServerRule
        /// </summary>
        /// <param name="pSystem"></param>
        public void SetSystem(RSystem pSystem)
        {
            rSys = (IRSystem7)pSystem;
        }

        #endregion

        #region Private ASR Methods

        /// <summary>
        /// This method will default data from the template, construction project
        /// for the new inspection record.
        /// </summary>
        /// <param name="rstInspection"></param>
        protected virtual void SetInspectionDefaults(Recordset rstInspection)
        {
            DataAccess objLib 
                = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

            //Inspector will be set by QA Website
            //Created By will be set by QA Website

            //TO-DO: Need to figure out how to get the super-intendent from the builder defined on the
            //construction project
            object vntBuilder = objLib.SqlIndex(modConstants.strtTIC_CONSTRUCTION_PROJECT,
                modConstants.strfCP_SUPERINTENDENT_ID, rstInspection.Fields[modConstants.strfTIC_CONSTRUCTION_PROJECT_ID].Value);
            rstInspection.Fields[modConstants.strfTIC_SUPERVISOR_ID].Value = vntBuilder;
           
            //Inspection name
            object inspectionName = objLib.SqlIndex(modConstants.strtINSPECTION_TEMPLATE,
                modConstants.strfINSPECTION_TEMPLATE_NAME, rstInspection.Fields[modConstants.strfINSPECTION_TEMPLATE_ID].Value);
            rstInspection.Fields[modConstants.strfINSPECTION_NAME].Value = TypeConvert.ToString(inspectionName);

            //set status to in process
            rstInspection.Fields[modConstants.strfSTATUS].Value = "Awaiting Submittal";

            //set date of inspeciton to today()
            //rstInspection.Fields[modConstants.strfDATE_COMPLETE].Value = DateTime.Now;

            //Set the last saved by user initially to the created by inspector
            rstInspection.Fields[modConstants.strfLAST_SAVED_BY_ID].Value = rstInspection.Fields[modConstants.strfCREATED_BY_ID].Value;

            //Default inspector
            SetDefaultInspector(rstInspection);

        }
        

        /// <summary>
        /// This method will create the Inspection Template check list as well
        /// as the associated categories as well as a default category
        /// </summary>
        /// <param name="rstInspection"></param>
        protected virtual void CreateTemplateItems(Recordset rstInspection)
        {
            //Do lookup on template to get categories and inspection items
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();
                     

            //Get all categories and loop through to create associated template items
            Recordset rstInspTmpCat = objLib.GetRecordset(modConstants.strqTIC_INT_CATEGORY_TEMPLATE_BY_TEMPLATE,
                1, rstInspection.Fields[modConstants.strfINSPECTION_TEMPLATE_ID].Value, GetInspectionCategoryTemplateFields());

            if (rstInspTmpCat.RecordCount > 0)
            {
                rstInspTmpCat.MoveFirst();
                while (!rstInspTmpCat.EOF)
                {
                    //Get inspection template items for each category
                    StringBuilder sb = new StringBuilder();
                    sb.Append("select tic_ordinal, tic_inspection_item from inspection_step_template ");
                    sb.Append("where tic_insp_category_template_id = " 
                        + rSys.IdToString(rstInspTmpCat.Fields[modConstants.strfTIC_INSP_CATEGORY_TEMPLATE_ID].Value));
                    
                    //load up the template items by category
                    Recordset rstTempItems = objLib.GetRecordset(sb.ToString());

                    if(rstTempItems.RecordCount > 0)
                    {
                        //TO-DO : Create category since there are template items under this category template
                        object vntCategoryId 
                            = CreateInspectionCategory(objLib, rstInspection.Fields[modConstants.strfINSPECTION_ID].Value,
                                                        TypeConvert.ToInt32(rstInspTmpCat.Fields[modConstants.strfORDINAL].Value), 
                                                        TypeConvert.ToString(rstInspTmpCat.Fields[modConstants.strfCATEGORY_DESC].Value));

                        //Get new Inspection step recordset to copy into
                        Recordset rstInspSteps = objLib.GetNewRecordset(modConstants.strtINSPECTION_STEP, GetInspectionStepFields());

                        //Create steps from template
                        CopyStepTemplate(objLib, vntCategoryId, rstInspection.Fields[modConstants.strfINSPECTION_ID].Value,
                            rstTempItems, rstInspSteps);

                        //close recordsets
                        rstTempItems.Close();
                        rstInspSteps.Close();

                    }

                    //Next Category Template
                    rstInspTmpCat.MoveNext();
                }
            }

        }

        /// <summary>
        /// This method will update the tract time line item so that it will
        /// fall off the list of scheduled inspections.
        /// </summary>
        /// <param name="rstInspection"></param>
        protected virtual void UpdateTractTimeRecordStatus(string scope, Recordset rstScopeItems, object inspectionId)
        {
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();

            //TO-DO : Get all TIC_Inspected_Scope Itmes associated with this inspection
            // and updte the exploded units as inprocess
            if (rstScopeItems.RecordCount > 0)
            {
                rstScopeItems.MoveFirst();
                while (!rstScopeItems.EOF)
                {
                    //With each scope item query the tract time data
                    switch (scope)
                    { 
                        case "Building":
                            UpdateTractTimeByBuilding(objLib, inspectionId, rstScopeItems.Fields[modConstants.strfCONSTRUCTION_PROJECT_ID].Value,
                                rstScopeItems.Fields[modConstants.strfPHASE_NBR].Value, rstScopeItems.Fields[modConstants.strfINSPECTION_TYPE_ID].Value,
                                TypeConvert.ToString(rstScopeItems.Fields[modConstants.strfSCOPE_ITEM_NBR].Value));
                            break;
                        case "Lot":
                            UpdateTractTimeByLot(objLib, inspectionId, rstScopeItems.Fields[modConstants.strfCONSTRUCTION_PROJECT_ID].Value,
                                rstScopeItems.Fields[modConstants.strfPHASE_NBR].Value, rstScopeItems.Fields[modConstants.strfINSPECTION_TYPE_ID].Value,
                                TypeConvert.ToString(rstScopeItems.Fields[modConstants.strfSCOPE_ITEM_NBR].Value));
                            break;
                        case "Unit":
                            UpdateTractTimeByUnit(objLib, inspectionId, rstScopeItems.Fields[modConstants.strfCONSTRUCTION_PROJECT_ID].Value,
                                rstScopeItems.Fields[modConstants.strfPHASE_NBR].Value, rstScopeItems.Fields[modConstants.strfINSPECTION_TYPE_ID].Value,
                                TypeConvert.ToString(rstScopeItems.Fields[modConstants.strfSCOPE_ITEM_NBR].Value));
                            break;
                        default:
                            break;
                    }

                    rstScopeItems.MoveNext();
                }
            }
        
        }


        /// <summary>
        /// Retrieves the template for the inspection type selected.
        /// </summary>
        /// <param name="inspTypeId"></param>
        /// <returns></returns>
        protected virtual bool DoesTemplateExistForInspectionType(object inspTypeId, out object templateId)
        {
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();

            templateId = rSys.Tables[modConstants.strtINSPECTION_TEMPLATE].Fields[modConstants.strfINSPECTION_TEMPLATE_ID].FindValue(
                rSys.Tables[modConstants.strtINSPECTION_TEMPLATE].Fields[modConstants.strfTIC_INSPECTION_TYPE_ID], inspTypeId);

            if (!Convert.IsDBNull(templateId))
            {
                return true;
            }
            else
            {
                return false;            
            }
           
        }

        /// <summary>
        /// This method will create the new Inspection category for the inspection
        /// from the template
        /// </summary>
        /// <param name="inspectionId"></param>
        /// <param name="ordinal"></param>
        /// <param name="categoryDesc"></param>
        /// <returns></returns>
        protected virtual object CreateInspectionCategory(DataAccess objLib, object inspectionId, int ordinal, string categoryDesc)
        {
            Recordset rstNewInspCat = objLib.GetNewRecordset(modConstants.strtTIC_INSP_CATEGORY, GetInspectionCategoryFields());
            rstNewInspCat.AddNew(Type.Missing, Type.Missing);
            rstNewInspCat.Fields[modConstants.strfORDINAL].Value = ordinal;
            rstNewInspCat.Fields[modConstants.strfCATEGORY_DESC].Value = categoryDesc;
            rstNewInspCat.Fields[modConstants.strfINSPECTION_ID].Value = inspectionId;
            objLib.SaveRecordset(modConstants.strtTIC_INSP_CATEGORY, rstNewInspCat);
            object inspCatId = rstNewInspCat.Fields[modConstants.strfTIC_INSP_CATEGORY_ID].Value;
            return inspCatId;         

        }

        /// <summary>
        /// Create a default place holder to store miscellaneous inspection items added from QA Site
        /// </summary>
        /// <param name="objLib"></param>
        /// <param name="inspectionId"></param>
        protected virtual void CreateDefaultMiscCategory(object inspectionId)
        {
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();

            Recordset rstNewInspCat = objLib.GetNewRecordset(modConstants.strtTIC_INSP_CATEGORY, GetInspectionCategoryFields());
            rstNewInspCat.AddNew(Type.Missing, Type.Missing);
            rstNewInspCat.Fields[modConstants.strfORDINAL].Value = 99;
            rstNewInspCat.Fields[modConstants.strfCATEGORY_DESC].Value = "Miscellaneous";
            rstNewInspCat.Fields[modConstants.strfINSPECTION_ID].Value = inspectionId;
            objLib.SaveRecordset(modConstants.strtTIC_INSP_CATEGORY, rstNewInspCat);              
        }

        /// <summary>
        /// This method will copy over the template steps and associated the categories
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="inspectionId"></param>
        /// <param name="rstTempSteps"></param>
        /// <param name="rstInspSteps"></param>
        private void CopyStepTemplate(DataAccess objLib, object categoryId, object inspectionId, Recordset rstTempSteps, Recordset rstInspSteps)
        {
            //Loop through and create new inspection steps
            rstTempSteps.MoveFirst();
            while (!rstTempSteps.EOF)
            {
                rstInspSteps.AddNew(Type.Missing, Type.Missing);
                rstInspSteps.Fields[modConstants.strfTIC_INSP_CATEGORY_ID].Value = categoryId;
                rstInspSteps.Fields[modConstants.strfINSPECTION_ID].Value = inspectionId;
                rstInspSteps.Fields[modConstants.strfTIC_ORDINAL].Value = TypeConvert.ToInt32(rstTempSteps.Fields["tic_ordinal"].Value);
                rstInspSteps.Fields[modConstants.strfDESCRIPTION].Value = rstTempSteps.Fields["tic_inspection_item"].Value;
                rstTempSteps.MoveNext();
            }

            objLib.SaveRecordset(modConstants.strtINSPECTION_STEP, rstInspSteps);


        }

        /// <summary>
        /// This method will update all tract tiem records associated with this 
        /// particular inspection in order to release the tract time records back to 
        /// the scheduled inspection
        /// </summary>
        /// <param name="objLib"></param>
        /// <param name="inspectionId"></param>
        private void ReleaseTractTimeRecords(DataAccess objLib, object inspectionId)
        {
            Recordset rstTractTime = objLib.GetLinkedRecordset(modConstants.strtTIC_LOT_CONSTRUCTION_DATE, 
                modConstants.strfINSPECTION_ID, inspectionId, modConstants.strfINSPECTION_STATUS, modConstants.strfINSPECTION_ID);
            if (rstTractTime.RecordCount > 0)
            {
                rstTractTime.MoveFirst();
                while (!rstTractTime.EOF)
                {
                    rstTractTime.Fields[modConstants.strfINSPECTION_STATUS].Value = DBNull.Value;
                    rstTractTime.Fields[modConstants.strfINSPECTION_ID].Value = DBNull.Value;
                    rstTractTime.MoveNext();
                }
                objLib.SaveRecordset(modConstants.strtTIC_LOT_CONSTRUCTION_DATE, rstTractTime);
            }              
        
        }

        /// <summary>
        /// This method will delete all associated linked records for the inspection
        /// </summary>
        /// <param name="objLib"></param>
        /// <param name="inspectionId"></param>
        private void PerformCascadeDeleteForInspection(DataAccess objLib, object inspectionId)
        {
            //Delete Inspection Items, Categories, QA Docs, QA Links, Scope Items
            objLib.DeleteLinkedRecordset(modConstants.strtINSPECTION_STEP, modConstants.strfINSPECTION_ID, inspectionId);
            objLib.DeleteLinkedRecordset(modConstants.strtTIC_INSP_CATEGORY, modConstants.strfINSPECTION_ID, inspectionId);
            objLib.DeleteLinkedRecordset(modConstants.strtTIC_INSPECTED_SCOPE_ITEMS, modConstants.strfINSPECTION_ID, inspectionId);
            objLib.DeleteLinkedRecordset(modConstants.strtTIC_QA_DOCUMENTS, modConstants.strfINSPECTION_ID, inspectionId);
            objLib.DeleteLinkedRecordset(modConstants.strtTIC_QA_WEBLINKS, modConstants.strfINSPECTION_ID, inspectionId);

        }

        private void SetScopeItemsOnInspection(Recordset rstInspection, Recordset rstScopeItems)
        {
            string strScopeItems = string.Empty;
            if (rstScopeItems.RecordCount > 0)
            {
                rstScopeItems.MoveFirst();
                while (!rstScopeItems.EOF)
                {
                    strScopeItems = strScopeItems + TypeConvert.ToString(rstScopeItems.Fields[modConstants.strfSCOPE_ITEM_NBR].Value) + ",";
                    rstScopeItems.MoveNext();
                }

            }

            if (strScopeItems.Length > 0)
            {
                string trimmedScopeItems = string.Empty;
                trimmedScopeItems = strScopeItems.TrimEnd(',');
                trimmedScopeItems = trimmedScopeItems.Trim();
                rstInspection.Fields[modConstants.strfTIC_SCOPE_ITEMS].Value = TypeConvert.ToString(trimmedScopeItems);
            }

        }


        #region Field List getters

        private object[] GetInspectionStepFields()
        {
            string[] arrInspSteps = new string[]
            {
                modConstants.strfTIC_ORDINAL, modConstants.strfDESCRIPTION,
                modConstants.strfINSPECTION_ID, modConstants.strfINSPECTION_STEP_ID,
                modConstants.strfTIC_INSP_CATEGORY_ID
            };
            return arrInspSteps; 
        }

        private object[] GetInspectionCategoryFields()
        {
            string[] arrCatFields = new string[] 
            {
                modConstants.strfCATEGORY_DESC, modConstants.strfORDINAL, modConstants.strfINSPECTION_ID,
                modConstants.strfTIC_INSP_CATEGORY_ID
            };
            return arrCatFields;
        }

        private object[] GetInspectionCategoryTemplateFields()
        {
            string[] arrCatFields = new string[] 
            {
                modConstants.strfCATEGORY_DESC, modConstants.strfORDINAL
            };
            return arrCatFields;
        }

        private object[] GetNotificationTemplateFieldList()
        {
            string[] arrTemplateFlds = new string[] 
            { 
                modConstants.strfTIC_QA_NOTIFICATION_TMP_ID,
                modConstants.strfSUBJECT, 
                modConstants.strfBODY, 
                modConstants.strfSEND_TO_ADMIN,
                modConstants.strfSEND_TO_CONSULTANT,
                modConstants.strfSEND_TO_SUPER,
                modConstants.strfLINK_TEMPLATE
            
            };
            return arrTemplateFlds;
        }

        private object[] GetNotificationTeamMembersFieldList()
        {
            string[] arrTemplateFlds = new string[] 
            { 
                modConstants.strfCONTACT_ID,
                modConstants.strfTIC_QA_NOTIFICATION_TMP_ID, 
                modConstants.strfINACTIVE,
                modConstants.strfADMIN_EMAIL
            
            };
            return arrTemplateFlds;
        }

        #endregion

        #region Update methods

        /// <summary>
        /// This method will perform an update on all tract time records when an inspectio is created for the 
        /// building associated with these records.
        /// </summary>
        /// <param name="objLib"></param>
        /// <param name="projectId"></param>
        /// <param name="phaseName"></param>
        /// <param name="inspTypeId"></param>
        /// <param name="buildingNbr"></param>
        private void UpdateTractTimeByBuilding(DataAccess objLib, object inspectionId, object projectId, object phaseName, object inspTypeId, string buildingNbr)
        { 
            //Query all tract time records in order to get all for the building passed in
            Recordset rstTractTime = objLib.GetRecordset(modConstants.strqTIC_INT_TRACT_TIME_BY_BUILDING, 4,
                projectId, phaseName, inspTypeId, buildingNbr, modConstants.strfINSPECTION_STATUS, modConstants.strfINSPECTION_ID);

            if (rstTractTime.RecordCount > 0)
            {
                rstTractTime.MoveFirst();
                while (!rstTractTime.EOF)
                {
                    //AM2011.02.14 - Added check to ensure that Tract Time record has not
                    //be claimed by another inspection.  If so, throw exception to Web

                    if (rstTractTime.Fields[modConstants.strfINSPECTION_ID].Value == DBNull.Value)
                    {

                        rstTractTime.Fields[modConstants.strfINSPECTION_STATUS].Value = "In Process";
                        rstTractTime.Fields[modConstants.strfINSPECTION_ID].Value = inspectionId;
                        rstTractTime.MoveNext();
                    }
                    else
                    { 
                        //Throw Exception to Web
                        throw new PivotalApplicationException(
                            modConstants.strexINSPECTION_ALREADY_CREATED_BUILDING 
                            + TypeConvert.ToString(buildingNbr));
                    }
                }

                //Save to HIP
                objLib.SaveRecordset(modConstants.strtTIC_LOT_CONSTRUCTION_DATE, rstTractTime);

            }
        
        }

        private void UpdateTractTimeByLot(DataAccess objLib, object inspectionId, object projectId, object phaseName, object inspTypeId, string lotNbr)
        {
            //Query all tract time records in order to get all for the building passed in
            Recordset rstTractTime = objLib.GetRecordset(modConstants.strqTIC_INT_TRACT_TIME_BY_LOT, 4,
                projectId, phaseName, inspTypeId, lotNbr, modConstants.strfINSPECTION_STATUS, modConstants.strfINSPECTION_ID);

            if (rstTractTime.RecordCount > 0)
            {
                rstTractTime.MoveFirst();
                while (!rstTractTime.EOF)
                {

                    if (rstTractTime.Fields[modConstants.strfINSPECTION_ID].Value == DBNull.Value)
                    {
                        rstTractTime.Fields[modConstants.strfINSPECTION_STATUS].Value = "In Process";
                        rstTractTime.Fields[modConstants.strfINSPECTION_ID].Value = inspectionId;
                        rstTractTime.MoveNext();
                    }
                    else
                    {
                        //Throw Exception to Web
                        throw new PivotalApplicationException(
                            modConstants.strexINSPECTION_ALREADY_CREATED_LOT
                            + TypeConvert.ToString(lotNbr));
                    }
                }

                //Save to HIP
                objLib.SaveRecordset(modConstants.strtTIC_LOT_CONSTRUCTION_DATE, rstTractTime);

            }
        }

        private void UpdateTractTimeByUnit(DataAccess objLib, object inspectionId, object projectId, object phaseName, object inspTypeId, string unitNbr)
        {
            //Query all tract time records in order to get all for the building passed in
            Recordset rstTractTime = objLib.GetRecordset(modConstants.strqTIC_INT_TRACT_TIME_BY_UNIT, 4,
                projectId, phaseName, inspTypeId, unitNbr, modConstants.strfINSPECTION_STATUS, modConstants.strfINSPECTION_ID);

            if (rstTractTime.RecordCount > 0)
            {
                rstTractTime.MoveFirst();
                while (!rstTractTime.EOF)
                {

                    if (rstTractTime.Fields[modConstants.strfINSPECTION_ID].Value == DBNull.Value)
                    {
                        rstTractTime.Fields[modConstants.strfINSPECTION_STATUS].Value = "In Process";
                        rstTractTime.Fields[modConstants.strfINSPECTION_ID].Value = inspectionId;
                        rstTractTime.MoveNext();
                    }
                    else
                    {
                        //Throw Exception to Web
                        throw new PivotalApplicationException(
                            modConstants.strexINSPECTION_ALREADY_CREATED_UNIT
                            + TypeConvert.ToString(unitNbr));
                    }
                }

                //Save to HIP
                objLib.SaveRecordset(modConstants.strtTIC_LOT_CONSTRUCTION_DATE, rstTractTime);

            }
        }
                      

        #endregion

        #region Notification Methods

        /// <summary>
        /// This method will check teh status table in HIP to determine
        /// the follow on status and send an email notifiation if one is
        /// configured for the status transition
        /// </summary>
        /// <param name="rstInspection"></param>
        protected virtual void ProcessWorkflowStatus(Recordset rstInspection, string action)
        {
            //Get status passed in on saved record
            string strCurrStatus = TypeConvert.ToString(rstInspection.Fields[modConstants.strfSTATUS].Value);
            //bool? blnFollowUpReq = TypeConvert.ToBoolean(rstInspection.Fields[modConstants.strfTIC_CORRECTIVE_ACTION_REQUIRED].Value);
            string correctiveActionStatus = TypeConvert.ToString(rstInspection.Fields[modConstants.strfTIC_CORRECTIVE_ACTION_STATUS].Value);
            int intFollowUpReq = correctiveActionStatus == "Follow-Up" ? 1 : 0;
            //Place holder for when we add escalation in Phase II
            int intEscalReq = 0;

            //Always set follow up req = 0 if user is not Submitting.  This will 
            //prevent us from having to create a follow up required state transition for every state transition record
            if (action != "Submit")
            { intFollowUpReq = 0; }


            //Get all state transition records for the current inspection status and action
            DataAccess objLib = (DataAccess) rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();

            StringBuilder sb = new StringBuilder();
            sb.Append("select ");
            sb.Append("qa_action, ");
            sb.Append("current_status, ");
            sb.Append("follow_on_status, ");
            sb.Append("send_notification, ");
            sb.Append("email_template, ");
            sb.Append("follow_up_required, ");
            sb.Append("escalation_required ");
            sb.Append("from tic_qa_status_transitions(nolock) ");
            sb.Append("where qa_action = '" + action + "'");
            sb.Append("and current_status = '" + strCurrStatus + "'");
            sb.Append("and isnull(follow_up_required, 0) = " + intFollowUpReq);
            sb.Append(" and isnull(escalation_required, 0) = " + intEscalReq);
            //Query the database for state trans record
            Recordset rstStateTrans = objLib.GetRecordset(sb.ToString());

            if (rstStateTrans.RecordCount > 0)
            {
                rstStateTrans.MoveFirst();
                
                //Set status to follow on status
                rstInspection.Fields[modConstants.strfSTATUS].Value 
                    = rstStateTrans.Fields["follow_on_status"].Value;

                //Send e-mail if configured to do so
                if (TypeConvert.ToBoolean(rstStateTrans.Fields["send_notification"].Value))
                {
                    //Handles all e-mail notification processing
                    ProcessQANotification(objLib, rstInspection, rstStateTrans.Fields["email_template"].Value);
                }

            }

        }

        /// <summary>
        /// This method will used as the main wrapper method which will be called from the main ASR
        /// to process an e-mail notification based on teh notification type
        /// </summary>
        /// <param name="rstInspection"></param>
        /// <param name="notificationtype"></param>
        protected virtual void ProcessQANotification(DataAccess objLib, Recordset rstInspection, object notificationTemplateId)
        {
            string strToList = string.Empty;
            string strSubject = string.Empty;
            string strBody = string.Empty;
            string strLink = string.Empty;
            
            //If notification template is defined send email
            if (notificationTemplateId != null)
            {
                object[] arrFields = GetNotificationTemplateFieldList();

                //Get template configuration
                Recordset rstEmailTemp 
                    = objLib.GetRecordset(notificationTemplateId, 
                    modConstants.strtTIC_QA_NOTIFICATION_TEMPLATE, 
                    arrFields);

                if (rstEmailTemp.RecordCount > 0)
                {
                    rstEmailTemp.MoveFirst();

                    //Build Email 
                    strToList = BuildNotificationRecipientList(rstEmailTemp, rstInspection);
                    strSubject = BuildNotificationSubject(rstEmailTemp, rstInspection);
                    strBody = BuildNotificationBody(rstEmailTemp, rstInspection);
                    //Send Email out
                    SendNotification(strToList, strSubject, strBody, strLink);
                
                }


            
            }


        }


        /// <summary>
        /// Helper method used to build notification recipient list 
        /// dynamically based on template configurations
        /// </summary>
        /// <returns></returns>
        protected virtual string BuildNotificationRecipientList(Recordset rstTemplate, Recordset rstInspection)
        {             
            //Build recipient list
            List<string> recipientList = new List<string>();
            
            //interact with DB
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();

            //Get send indicators
            bool blnToInspector = TypeConvert.ToBoolean(rstTemplate.Fields[modConstants.strfSEND_TO_CONSULTANT].Value);
            bool blnToSuper = TypeConvert.ToBoolean(rstTemplate.Fields[modConstants.strfSEND_TO_SUPER].Value);
            bool blnToAdmin = TypeConvert.ToBoolean(rstTemplate.Fields[modConstants.strfSEND_TO_ADMIN].Value);
            object inspectorEmail = null;
            if (blnToInspector)
            {
                inspectorEmail = objLib.SqlIndex(modConstants.strtCONTACT,
                modConstants.strfEMAIL, rstInspection.Fields[modConstants.strfINSPECTOR_ID].Value);
                if (!String.IsNullOrEmpty(TypeConvert.ToString(inspectorEmail)))
                {
                    recipientList.Add(inspectorEmail.ToString());
                }

            }
            if (blnToSuper)
            {
                object superEmail = objLib.SqlIndex(modConstants.strtCONTACT,
                    modConstants.strfEMAIL, rstInspection.Fields[modConstants.strfTIC_SUPERVISOR_ID].Value);
                if (!String.IsNullOrEmpty(TypeConvert.ToString(superEmail)))
                {
                    recipientList.Add(inspectorEmail.ToString());
                }
            }
            if (blnToAdmin)
            {
                object AdminEmail = objLib.SqlIndex(modConstants.strtCONTACT,
                    modConstants.strfEMAIL, rstInspection.Fields[modConstants.strfCREATED_BY_ID].Value);
                if (!String.IsNullOrEmpty(TypeConvert.ToString(AdminEmail)))
                {
                    //Only set if create user is different than inspector
                    if (TypeConvert.ToString(AdminEmail) != TypeConvert.ToString(inspectorEmail)) 
                    {
                        recipientList.Add(inspectorEmail.ToString());
                    }
                }
            }                      

            Recordset rstTempAdmins = objLib.GetLinkedRecordset(modConstants.strtTIC_QA_NOTIFICATION_TEAM,
                modConstants.strfTIC_QA_NOTIFICATION_TMP_ID, rstTemplate.Fields[modConstants.strfTIC_QA_NOTIFICATION_TMP_ID].Value,
                GetNotificationTeamMembersFieldList());

            if (rstTempAdmins.RecordCount > 0)
            {
                rstTempAdmins.MoveFirst();
                while (!rstTempAdmins.EOF)
                {
                    if (!TypeConvert.ToBoolean(rstTempAdmins.Fields[modConstants.strfINACTIVE].Value))
                    {
                        string strTeamMbrEmail = TypeConvert.ToString(rstTempAdmins.Fields[modConstants.strfADMIN_EMAIL].Value);
                        if (!String.IsNullOrEmpty(strTeamMbrEmail))
                        {
                            //Add to recipient list
                            recipientList.Add(strTeamMbrEmail);
                        }
                    }


                    rstTempAdmins.MoveNext();
                }
            }

            //clean up
            rstTempAdmins.Close();
            
            //Now interate through list and build ; delimited string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < recipientList.Count; i++)
            { sb.Append(recipientList[i].ToString()+ ";");}

            //Remove trailing ";"
            return sb.ToString().TrimEnd(';');
     
        }

        /// <summary>
        /// Helper method used to build notification subject  
        /// dynamically based on template configurations
        /// </summary>
        /// <returns></returns>
        protected virtual string BuildNotificationSubject(Recordset rstTemplate, Recordset rstInspection)
        {
            //DataAccess
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();

            //Pull out subject from the template
            string strSubjectTemplate = TypeConvert.ToString(rstTemplate.Fields[modConstants.strfSUBJECT].Value);

            //Get values from inspection we want to use in template
            string strProjectName = TypeConvert.ToString(objLib.SqlIndex(modConstants.strtTIC_CONSTRUCTION_PROJECT,
                modConstants.strfTIC_CONSTRUCTION_PROJECT_NAME, 
                rstInspection.Fields[modConstants.strfTIC_CONSTRUCTION_PROJECT_ID].Value));
                        
            //Construction Phase
            string strPhase = TypeConvert.ToString(rstInspection.Fields[modConstants.strfTIC_CONSTRUCTION_PHASE].Value);

            //Inspection Type
            string strInspectionType = TypeConvert.ToString(objLib.SqlIndex(modConstants.strtTIC_INSPECTION_TYPE,
               modConstants.strfTIC_INSPECTION_TYPE_DESC,
               rstInspection.Fields[modConstants.strfTIC_INSPECTION_TYPE_ID].Value));

            //Replace template tags with data
            strSubjectTemplate = strSubjectTemplate.Replace(modConstants.strtagTIC_CONSTRUCTION_PROJECT_NAME,
                strProjectName);
            strSubjectTemplate = strSubjectTemplate.Replace(modConstants.strtagPHASE, strPhase);
            strSubjectTemplate = strSubjectTemplate.Replace(modConstants.strtagTIC_INSPECTION_TYPE, strInspectionType);
            return strSubjectTemplate;
        }

        /// <summary>
        /// Helper method used to build notification body 
        /// dynamically based on template configurations
        /// </summary>
        /// <returns></returns>
        protected virtual string BuildNotificationBody(Recordset rstTemplate, Recordset rstInspection)
        {
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();

            string strBody = TypeConvert.ToString(rstTemplate.Fields[modConstants.strfBODY].Value);

            //Get all data needed for e-mail body
            //Get values from inspection we want to use in template
            string strProjectName = TypeConvert.ToString(objLib.SqlIndex(modConstants.strtTIC_CONSTRUCTION_PROJECT,
                modConstants.strfTIC_CONSTRUCTION_PROJECT_NAME,
                rstInspection.Fields[modConstants.strfTIC_CONSTRUCTION_PROJECT_ID].Value));

            //Construction Phase
            string strPhase = TypeConvert.ToString(rstInspection.Fields[modConstants.strfTIC_CONSTRUCTION_PHASE].Value);

            //Inspection Type
            string strInspectionType = TypeConvert.ToString(objLib.SqlIndex(modConstants.strtTIC_INSPECTION_TYPE,
               modConstants.strfTIC_INSPECTION_TYPE_DESC,
               rstInspection.Fields[modConstants.strfTIC_INSPECTION_TYPE_ID].Value));
            
            string strDateOfInsp = TypeConvert.ToString(rstInspection.Fields[modConstants.strfDATE_COMPLETE].Value);
            string strInspScope = TypeConvert.ToString(rstInspection.Fields[modConstants.strfTIC_SCOPE].Value);

            //Get Inspector fields
            string inspectorPhone = string.Empty;
            string inspectorCompany = string.Empty;
            string inspectorFirstName = string.Empty;
            string inspectorLastName = string.Empty;
            Recordset rstInspector = objLib.GetRecordset(rstInspection.Fields[modConstants.strfINSPECTOR_ID].Value,
                modConstants.strtCONTACT, modConstants.strfCOMPANY_NAME, modConstants.strfPHONE, modConstants.strfFIRST_NAME,
                modConstants.strfLAST_NAME);
            if (rstInspector.RecordCount > 0)
            {
                rstInspector.MoveFirst();
                inspectorCompany = TypeConvert.ToString(rstInspector.Fields[modConstants.strfCOMPANY_NAME].Value);
                inspectorPhone = TypeConvert.ToString(rstInspector.Fields[modConstants.strfPHONE].Value); 
                inspectorFirstName = TypeConvert.ToString(rstInspector.Fields[modConstants.strfFIRST_NAME].Value);
                inspectorLastName = TypeConvert.ToString(rstInspector.Fields[modConstants.strfLAST_NAME].Value);
            }
            //String to use for replace
            string inspectorString = inspectorFirstName
                + " " + inspectorLastName
                + " / " + inspectorCompany
                + " / " + inspectorPhone;


            //Get Super-Intendent fields
            string builderPhone = string.Empty;
            string builderCompany = string.Empty;
            string builderFirstName = string.Empty;
            string builderLastName = string.Empty;
            Recordset rstBuilder = objLib.GetRecordset(rstInspection.Fields[modConstants.strfTIC_SUPERVISOR_ID].Value,
                modConstants.strtCONTACT, modConstants.strfCOMPANY_NAME, modConstants.strfPHONE, modConstants.strfFIRST_NAME,
                modConstants.strfLAST_NAME);
            if (rstBuilder.RecordCount > 0)
            {
                rstBuilder.MoveFirst();
                builderCompany = TypeConvert.ToString(rstBuilder.Fields[modConstants.strfCOMPANY_NAME].Value);
                builderPhone = TypeConvert.ToString(rstBuilder.Fields[modConstants.strfPHONE].Value);
                builderFirstName = TypeConvert.ToString(rstBuilder.Fields[modConstants.strfFIRST_NAME].Value);
                builderLastName = TypeConvert.ToString(rstBuilder.Fields[modConstants.strfLAST_NAME].Value);
            }
            //String to use for replace
            string builderString = builderFirstName
                + " " + builderLastName
                + " / " + builderCompany
                + " / " + builderPhone;

            //Get list of scope items and build list
            Recordset rstScopeItems = objLib.GetLinkedRecordset(modConstants.strtTIC_INSPECTED_SCOPE_ITEMS,
                modConstants.strfINSPECTION_ID, rstInspection.Fields[modConstants.strfINSPECTION_ID].Value,
                modConstants.strfSCOPE_ITEM_NBR);

            string scopeItemList = string.Empty;
            if (rstScopeItems.RecordCount > 0)
            {
                while (!rstScopeItems.EOF)
                {
                    scopeItemList = scopeItemList + TypeConvert.ToString(rstScopeItems.Fields[modConstants.strfSCOPE_ITEM_NBR].Value) + ",";
                    rstScopeItems.MoveNext();
                }
            }
            //Remove trailing commas
            scopeItemList = scopeItemList.TrimEnd(',');

            //Now replace
            strBody = strBody.Replace(modConstants.strtagTIC_INSPECTION_TYPE, strInspectionType);
            strBody = strBody.Replace(modConstants.strtagDATE_OF_INSPECTION, strDateOfInsp);
            strBody = strBody.Replace(modConstants.strtagTIC_CONSTRUCTION_PROJECT_NAME, strProjectName);
            strBody = strBody.Replace(modConstants.strtagPHASE, strPhase);
            strBody = strBody.Replace(modConstants.strtagINSPECTION_SCOPE, strInspScope);
            strBody = strBody.Replace(modConstants.strtagINSPECTED_SCOPE_ITEM, scopeItemList);
            strBody = strBody.Replace(modConstants.strtagINSPECTOR, inspectorString);
            strBody = strBody.Replace(modConstants.strtagSUPER, builderString);
            strBody = strBody.Replace(modConstants.strtagQA_LINK, BuildAttachmentURL(rstTemplate, rstInspection));

            //return replaced string
            return strBody;

        }

        /// <summary>
        /// Helper method used to build notification attachment list 
        /// dynamically based data stored in the QA Document table
        /// </summary>
        /// <returns></returns>
        protected virtual string BuildAttachmentURL(Recordset rstTemplate, Recordset rstInspection)
        {
            string inspectionId = rSys.IdToString(rstInspection.Fields[modConstants.strfINSPECTION_ID].Value);
            int intInspectionId = Convert.ToInt32(inspectionId, 16);

            string linkTemplate = TypeConvert.ToString(rstTemplate.Fields[modConstants.strfLINK_TEMPLATE].Value);
            string urlLink = linkTemplate + intInspectionId.ToString();
            return urlLink;
        }

        /// <summary>
        /// Sends Email using the IRSend.  Will be used to send out 
        /// e-mail notifications for Quality Assurance
        /// </summary>
        /// <param name="toList">To recipients list</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body</param>
        /// <history>
        /// Revision#    Date       Author       Description
        /// 1.0.0.0      01/31/11   A.Maldonado  E-mail sending method
        /// </history>
        protected virtual void SendNotification(string toList, string subject, 
            string body, string link)
        {
            IRSend objEmail = rSys.CreateEmail();
            objEmail.NewMessage();
            objEmail.To = toList;
            objEmail.Subject = TypeConvert.ToString(subject);
            objEmail.Body = TypeConvert.ToString(body);
            //Add link to the email
            //objEmail.AddAttachment(link);
            objEmail.Send();
        }


        /// <summary>
        /// Default the inspector based on business logic provided by Web Team
        /// </summary>
        /// <param name="rstInspection"></param>
        protected virtual void SetDefaultInspector(Recordset rstInspection)
        { 
            //Check the type of the CreatedBy user :
            //1) If created by user is an inspector sset as the default inspector
            //2) If created by user is an Admin, then find the first inspector and default
            //3) If created by user is an admin without a configured inspector then default
            //   to first inspector in the inspection company
            DataAccess objLib = (DataAccess) rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();

            Recordset rstContact = objLib.GetRecordset(rstInspection.Fields[modConstants.strfCREATED_BY_ID].Value,
                modConstants.strtCONTACT, modConstants.strfCONTACT_ROLE, modConstants.strfCONTACT_ID);

            if (rstContact.RecordCount > 0)
            {
                rstContact.MoveFirst();
                //If Inspector then default on inspection
                if (TypeConvert.ToString(rstContact.Fields[modConstants.strfCONTACT_ROLE].Value) == "Inspector")
                {
                    rstInspection.Fields[modConstants.strfINSPECTOR_ID].Value = rstInspection.Fields[modConstants.strfCREATED_BY_ID].Value;
                }
                else
                { 
                    //Get the first configured inspector for the Assistant
                    StringBuilder sb = new StringBuilder();
                    sb.Append("select top 1 ");
                    sb.Append("c.contact_id ");
                    sb.Append("from contact c ");
                    sb.Append("inner join tic_contact_admin ca on c.contact_id = ca.inspector_contact_id ");
                    sb.Append("where ca.Contact_Admin_Id = " + rSys.IdToString(rstContact.Fields[modConstants.strfCONTACT_ID].Value));
                    sb.Append(" and c.job_title = 'Inspector'");

                    Recordset rstInspectorFirst = objLib.GetRecordset(sb.ToString());
                    if (rstInspectorFirst.RecordCount > 0)
                    {
                        rstInspection.Fields[modConstants.strfINSPECTOR_ID].Value = rstInspectorFirst.Fields[0].Value;
                    }
                    else
                    { 
                        //Could not find configured inspector so now resort to getting first inspector 
                        //that belongs to the Inspector company
                        StringBuilder sb1 = new StringBuilder();
                        sb1.Append("select top 1 ");
                        sb1.Append("c.contact_id ");
                        sb1.Append("from contact c ");
                        sb1.Append("inner join contact c1 on c.company_id = c1.company_id ");
                        sb1.Append("where c1.contact_id = " + rSys.IdToString(rstContact.Fields[modConstants.strfCONTACT_ID].Value));
                        sb1.Append(" and c.job_title = 'Inspector'");

                        Recordset rstInspectorFirstInCompany = objLib.GetRecordset(sb1.ToString());
                        if (rstInspectorFirstInCompany.RecordCount > 0)
                        {
                            rstInspection.Fields[modConstants.strfINSPECTOR_ID].Value = rstInspectorFirstInCompany.Fields[0].Value;
                        }
                        else
                        {
                            throw new Exception("Could not find a configured Inspector to default for this inspection.  Please contact Pivotal administrator.");
                        }
                    }

                
                }

            }
            else
            {
                throw new Exception("Created By User Record could not be found.  Please contact Pivotal Administrator");
            }            

        }


        #endregion

        #region Helper Methods

        /// <summary>
        /// This method is used to get to the description if the
        /// enumeration has a space in it.  If it doesn't then just
        /// return the ToString() value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public string GetDisconnected(IRSystem7 rSys, IRForm pform, Recordset rstPrimary, string fieldName, string segName)
        {

            UIAccess objPLFunctionLib = (UIAccess)rSys.ServerScripts[AppServerRuleData.UIAccessAppServerRuleName].CreateInstance();
            string strDisconnectedFldName = string.Empty;
            object vntDisconnectedFldVal = null;

            strDisconnectedFldName = objPLFunctionLib.GetDisconnectedFieldName(pform.FormName, fieldName, segName);

            if (DBNull.Value == rstPrimary.Fields[strDisconnectedFldName].Value)
            {
                vntDisconnectedFldVal = "";
            }
            else
            {
                vntDisconnectedFldVal = rstPrimary.Fields[strDisconnectedFldName].Value;
            }

            return vntDisconnectedFldVal.ToString();
        }

        #endregion

        #endregion

    }
}
