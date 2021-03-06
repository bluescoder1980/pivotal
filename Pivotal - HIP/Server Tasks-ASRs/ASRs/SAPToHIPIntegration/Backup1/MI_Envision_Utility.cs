using System;
using System.Collections.Generic;
using System.Text;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Choice;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Form;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    class MI_Envision_Utility
    {
        //<returns> division name, division number</returns>
        public string[] GetDivisionDetail(object vntDivisionId, IRSystem7 rSys)
        {
            
            string strDivisionName = "";
            string strDivisionNumber = "";

            strDivisionName = rSys.Tables[DivisionData.TableName].Fields[DivisionData.NameField].FindValue(
                        rSys.Tables[DivisionData.TableName].Fields[DivisionData.DivisionIdField],
                        vntDivisionId).ToString();

            strDivisionNumber = rSys.Tables[DivisionData.TableName].Fields[DivisionData.DivisionNumberField].FindValue(
                        rSys.Tables[DivisionData.TableName].Fields[DivisionData.DivisionIdField],
                        vntDivisionId).ToString();

            string [] result = {strDivisionName, strDivisionNumber};

            return result;


        }

        public string[] GetCommunityDetail(object vntCommunityId, IRSystem7 rSys)
        {
            string strCommunityName = "";
            string strCommunityNumber = "";

            strCommunityName = rSys.Tables[NeighborhoodData.TableName].Fields[NeighborhoodData.NameField].FindValue(
                        rSys.Tables[NeighborhoodData.TableName].Fields[NeighborhoodData.NeighborhoodIdField],
                        vntCommunityId).ToString();

            strCommunityNumber = rSys.Tables[NeighborhoodData.TableName].Fields[NeighborhoodData.ExternalSourceCommunityIdField].FindValue(
                        rSys.Tables[NeighborhoodData.TableName].Fields[NeighborhoodData.NeighborhoodIdField],
                        vntCommunityId).ToString();

            string[] result = { strCommunityName, strCommunityNumber };

            return result;
            

        }
        public Object[] GetPhaseDetail(object vntPhaseId, IRSystem7 rSys)
        {
            string strPhaseName = "";
            string strPhaseNumber = "";
            object vntCommunityId = null;
            object vntDivisionId = null;

            strPhaseName = rSys.Tables[NBHDPhaseData.TableName].Fields[NBHDPhaseData.PhaseNameField].FindValue(
                        rSys.Tables[NBHDPhaseData.TableName].Fields[NBHDPhaseData.NBHDPhaseIdField],
                        vntPhaseId).ToString();

            strPhaseNumber = rSys.Tables[NBHDPhaseData.TableName].Fields["External_Source_Phase_Code"].FindValue(
                        rSys.Tables[NBHDPhaseData.TableName].Fields[NBHDPhaseData.NBHDPhaseIdField],
                        vntPhaseId).ToString();

            vntCommunityId = rSys.Tables[NBHDPhaseData.TableName].Fields[NBHDPhaseData.NeighborhoodIdField].FindValue(
                        rSys.Tables[NBHDPhaseData.TableName].Fields[NBHDPhaseData.NBHDPhaseIdField],
                        vntPhaseId);

            vntDivisionId = rSys.Tables[NBHDPhaseData.TableName].Fields[NBHDPhaseData.DivisionIdField].FindValue(
                        rSys.Tables[NBHDPhaseData.TableName].Fields[NBHDPhaseData.NBHDPhaseIdField],
                        vntPhaseId);

            Object[] result = { strPhaseName, strPhaseNumber, vntCommunityId, vntDivisionId };

            return result;


        }

        public string[] GetPlanDetail(object vntPlanId, IRSystem7 rSys)
        {
            string strPlanName = "";
            string strPlanNameFull = "";
            string strPlanNumber = "";
            string strPlanElevation = "";

            strPlanNameFull = rSys.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.ProductNameField].FindValue(
                        rSys.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.NBHDPProductIdField],
                        vntPlanId).ToString();

            strPlanName = strPlanNameFull.Substring(0, (strPlanNameFull.IndexOf("-") - 1)).Trim();

            strPlanNumber = rSys.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.PlanCodeField].FindValue(
                        rSys.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.NBHDPProductIdField],
                        vntPlanId).ToString();

            strPlanElevation = rSys.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.ElevationCodeField].FindValue(
                        rSys.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.NBHDPProductIdField],
                        vntPlanId).ToString();

            string[] result = { strPlanName, strPlanNumber, strPlanElevation };

            return result;


        }
        public object GetDivisionOption(string strArea, string strOptionNumber, IRSystem7 rSys)
        {
            object vntOptionId = null;


            vntOptionId = rSys.Tables[DivisionProductData.TableName].Fields[DivisionProductData.DivisionProductIdField].FindValue(
                        rSys.Tables[DivisionProductData.TableName].Fields[DivisionProductData.ExternalSourceIdField],
                        strArea + "-" + strOptionNumber);

            object result = vntOptionId;

            return result;
        }

        public string [] GetNbhdpProductById(object nbhdpProductId, IRSystem7 rSys)
        {
            string categoryCode = "";
            object categoryId = null;
            string subCategoryCode = "";
            object subCategoryId = null;
            string optionCode = "";

            //Get the category Id. Do this custom to ensure it is retrieved from nbhdp_product and not division product
            categoryId = rSys.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.CategoryIdField].FindValue(
                        rSys.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.NBHDPProductIdField],
                        nbhdpProductId);

            if (DBNull.Value != categoryId)
            {
                categoryCode = rSys.Tables[ConfigurationTypeData.ConfigurationTypeTableName].Fields["Code_"].FindValue(
                            rSys.Tables[ConfigurationTypeData.ConfigurationTypeTableName].Fields[ConfigurationTypeData.ConfigurationTypeIdFieldName],
                            categoryId).ToString();
            }
                     

            //Get the subcategory Id. Do this custom to ensure it is retrieved from nbhdp_product and not division product
            subCategoryId = rSys.Tables[NBHDPProductData.TableName].Fields["MI_Sub_Category_Id"].FindValue(
                        rSys.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.NBHDPProductIdField],
                        nbhdpProductId);

            if (DBNull.Value != subCategoryId)
            {
                subCategoryCode = rSys.Tables["Sub_Category"].Fields["MI_Code"].FindValue(
                                rSys.Tables["Sub_Category"].Fields["Sub_Category_Id"],
                                subCategoryId).ToString();
            }
            optionCode = rSys.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.CodeField].FindValue(
                        rSys.Tables[NBHDPProductData.TableName].Fields[NBHDPProductData.NBHDPProductIdField],
                        nbhdpProductId).ToString();
            string[] result = { optionCode, categoryCode, subCategoryCode };

            return result;
        }
        public object GetInboundContract(object vntContractId, IRSystem7 rSys)
        {
            object vntRetContractId = null;
            String strContractId = rSys.IdToString(vntContractId);

            //look to see if an existing contract exists that references an original inventory quote. If so return the actual
            //contract Id. If not the ID sent is is the actual contract or inventory quote ID.
            //vntRetContractId = rSys.Tables[OpportunityData.TableName].Fields[OpportunityData.OpportunityIdField].FindValue(
                        //rSys.Tables[OpportunityData.TableName].Fields["MI_Originating_Inv_Quote"],
                        //vntContractId);

            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            Recordset rstOpp = new Recordset();
            StringBuilder sqlText = new StringBuilder();


            sqlText.Append("SELECT ");
            sqlText.Append("    o.Opportunity_Id ");
            sqlText.Append("FROM ");
            sqlText.Append("    Opportunity o ");
            sqlText.Append("WHERE ");
            sqlText.Append("    o.MI_Originating_Inv_Quote = " + strContractId );
            sqlText.Append(" AND (( o.Pipeline_Stage = 'Contract' AND o.Status = 'In Progress')");
            sqlText.Append(" OR (o.Status = 'Inventory' AND (o.Inactive = 0 OR o.Inactive is null)))");

            rstOpp = objLib.GetRecordset(sqlText.ToString());

            if (rstOpp.RecordCount > 0)
            {
                rstOpp.MoveFirst();
                //Get oppportunity Id
                vntRetContractId = rstOpp.Fields[0].Value;
                rstOpp.Close();
            }

            if (vntRetContractId == DBNull.Value || vntRetContractId == null)
            {
                //vntRetContractId = vntContractId;
                //Must verify that contract is actually in Pivotal
                DataAccess objLib2 = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOpp2 = new Recordset();
                StringBuilder sqlText2 = new StringBuilder();


                sqlText2.Append("SELECT ");
                sqlText2.Append("    o.Opportunity_Id ");
                sqlText2.Append("FROM ");
                sqlText2.Append("    Opportunity o ");
                sqlText2.Append("WHERE ");
                sqlText2.Append("    o.Opportunity_Id = " + strContractId);
                sqlText2.Append(" AND (( o.Pipeline_Stage = 'Contract' AND o.Status = 'In Progress')");
                sqlText2.Append(" OR (o.Status = 'Inventory' AND (o.Inactive = 0 OR o.Inactive is null)))");

                rstOpp2 = objLib.GetRecordset(sqlText2.ToString());

                if (rstOpp2.RecordCount > 0)
                {
                    rstOpp2.MoveFirst();
                    //Get oppportunity Id
                    vntRetContractId = rstOpp2.Fields[0].Value;
                    rstOpp2.Close();
                }
                else
                {
                    throw new Exception("The contract does not exist in Pivotal");
                }
            }

            return vntRetContractId;


        }
        public Boolean IsActiveSpec(object vntProductId, object vntOppId, object vntOrigId, IRSystem7 rSys)
        {
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

            Recordset rstInvQuote = objLib.GetRecordset("MI: Active Inventory Quotes For Lot_ID ? MI_Orig_Inv_Id ? MI_Orig_Inv_Id ?"
                , 3, vntProductId, vntOppId, vntOrigId
                ,"Opportunity_ID");

                if (rstInvQuote.RecordCount > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
        }
        //public object GetCategory(object vntDivisionId, string strCatagoryCode, IRSystem7 rSys)
        //{
        //    object vntCategoryId = null;
        //    String strDivisionId = rSys.IdToString(vntDivisionId);

        //    DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
        //    Recordset rstCT = new Recordset();
        //    StringBuilder sqlText = new StringBuilder();


        //    //sqlText.Append("SELECT ");
        //    //sqlText.Append("    c.Configuration_Type_Id ");
        //    //sqlText.Append("FROM ");
        //    //sqlText.Append("    Configuration_Type c ");
        //    //sqlText.Append("WHERE ");
        //    //sqlText.Append("    c.Division_Id = " + strDivisionId);
        //    //sqlText.Append(" AND c.Code_ = '" + strCatagoryCode + "'");
            
            
        //    sqlText.Append("SELECT ");
        //    sqlText.Append("    c.Configuration_Type_Id ");
        //    sqlText.Append("FROM ");
        //    sqlText.Append("    Configuration_Type c ");
        //    sqlText.Append("WHERE ");
        //    sqlText.Append("c.Code_ = '" + strCatagoryCode + "'");
            

        //    rstCT = objLib.GetRecordset(sqlText.ToString());

        //    if (rstCT.RecordCount > 0)
        //    {
        //        rstCT.MoveFirst();
        //        //Get oppportunity Id
        //        vntCategoryId = rstCT.Fields[0].Value;
        //        rstCT.Close();
        //    }

        //    return vntCategoryId;


        //}
        //2009 Mar 17 Lookup changed. Assume that a subcategory is unique.
        public object GetCategory(object vntDivisionId, string strCatagoryCode, IRSystem7 rSys)
        {
            object vntSubCategoryId = null;
            //String strCategoryId = rSys.IdToString(vntCategoryId);

            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            Recordset rstSC = new Recordset();
            StringBuilder sqlText = new StringBuilder();


            sqlText.Append("SELECT ");
            sqlText.Append("    sc.Configuration_Type_Id ");
            sqlText.Append("FROM ");
            sqlText.Append("    Sub_Category sc ");
            sqlText.Append("WHERE ");
            sqlText.Append("sc.MI_Code = '" + strCatagoryCode + "'");
            sqlText.Append(" AND (sc.MI_Inactive is null or sc.MI_Inactive = 0)");


            rstSC = objLib.GetRecordset(sqlText.ToString());

            if (rstSC.RecordCount > 0)
            {
                rstSC.MoveFirst();
                //Get oppportunity Id
                vntSubCategoryId = rstSC.Fields[0].Value;
                rstSC.Close();
            }

            return vntSubCategoryId;


        }
        public object GetSubCategory(object vntCategoryId, string strSubCatagoryCode, IRSystem7 rSys)
        {
            object vntSubCategoryId = null;
            //Don't need to lookup by category the sub cat code is unique
            //String strCategoryId = rSys.IdToString(vntCategoryId);

            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            Recordset rstSC = new Recordset();
            StringBuilder sqlText = new StringBuilder();


            sqlText.Append("SELECT ");
            sqlText.Append("    sc.Sub_Category_Id ");
            sqlText.Append("FROM ");
            sqlText.Append("    Sub_Category sc ");
            sqlText.Append("WHERE ");
            //sqlText.Append("    sc.Configuration_Type_Id = " + strCategoryId);
            //sqlText.Append(" AND sc.MI_Code = '" + strSubCatagoryCode + "'");
            sqlText.Append("sc.MI_Code = '" + strSubCatagoryCode + "'");
            sqlText.Append(" AND (sc.MI_Inactive is null or sc.MI_Inactive = 0)");


            rstSC = objLib.GetRecordset(sqlText.ToString());

            if (rstSC.RecordCount > 0)
            {
                rstSC.MoveFirst();
                //Get oppportunity Id
                vntSubCategoryId = rstSC.Fields[0].Value;
                rstSC.Close();
            }

            return vntSubCategoryId;


        }
        
        /// <summary>
        /// This method will retrieve lookup values
        /// </summary>
        /// <history>
        ///         01July2008       AB      Initial version
        /// </history>
        /// <param name="sourceSystem"></param>
        /// <param name="targetSystem"></param>
        /// <param name="sourceValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="lookupType"></param>
        /// <param name="RSysSystem"></param>
        public string GetTranslation(string sourceSystem, string targetSystem, string sourceValue, string lookupType, IRSystem7 RSysSystem)
        {
            try
            {
                Recordset rst = null;
                string retValue = "";

                //Use this object to get new recordset
                DataAccess objLib = (DataAccess)
                   RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                StringBuilder sqlText = new StringBuilder();

                //Get Customer SQL
                sqlText.Append("SELECT ");
                sqlText.Append("    t.Target_Value ");
                sqlText.Append("From ");
                sqlText.Append("    MI_Translation t ");
                sqlText.Append("Where ");
                sqlText.Append("    Source_System = '" + sourceSystem + "'");
                sqlText.Append(" AND Target_System = '" + targetSystem + "'");
                sqlText.Append(" AND Source_Value = '" + sourceValue + "'");
                sqlText.Append(" AND Type = '" + lookupType + "'");
                rst = objLib.GetRecordset(sqlText.ToString());

                if (rst.RecordCount > 0)
                {
                    rst.MoveFirst();
                    //Get oppportunity Id
                    retValue = (string)rst.Fields[0].Value;
                    rst.Close();
                }

                return retValue;
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, RSysSystem);
            }
        }
        public Recordset GetOptionsToDelete(object vntOppId, IRSystem7 rSys)
        {
            String strOppId = rSys.IdToString(vntOppId);

            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            Recordset rstDO = new Recordset();
            StringBuilder sqlText = new StringBuilder();

            //sqlText.Append("Select op.code_, op.opportunity__product_id, op.nbhdp_product_Id ");
            //sqlText.Append("FROM ");
            //sqlText.Append("    opportunity__product op ");
            //sqlText.Append("    left outer join env_sync es1 on op.opportunity__product_Id = es1.opportunity_product_id ");
            //sqlText.Append("    left outer join env_sync es2 on op.opportunity_Id = es2.Opportunity_Id ");
            //sqlText.Append("WHERE ");
            //sqlText.Append("((op.Selected = 0 OR op.Selected is null) OR (op.Option_Selection_Source = 1 AND op.Replaces_Option_Id is not null)) ");
            //sqlText.Append("AND es1.env_sync_Id is null AND es2.env_sync_Id is not null AND op.Replaced_By_Option_Id is null ");
            //sqlText.Append("AND (op.MI_Envision_Deleted is null OR op.MI_Envision_Deleted =0) ");
            //sqlText.Append("AND op.Opportunity_id = " + strOppId);

            sqlText.Append("Select op.code_, op.opportunity__product_id, op.nbhdp_product_Id ");
            sqlText.Append("FROM ");
            sqlText.Append("    opportunity__product op ");
            sqlText.Append("    left outer join env_sync es1 on op.opportunity__product_Id = es1.opportunity_product_id ");
            sqlText.Append("    left outer join env_sync es2 on op.opportunity_Id = es2.Opportunity_Id ");
            sqlText.Append("    left outer join opportunity__product op2 on op.Replaced_By_Option_Id = op2.opportunity__product_id ");
            sqlText.Append("WHERE ");
            sqlText.Append("((op.Selected = 0 OR op.Selected is null) AND (op.Option_Selection_Source = 1)) ");
            sqlText.Append("AND (op2.Option_Selection_Source <> 1 OR op.Replaced_By_Option_Id is null) ");
            sqlText.Append("AND es1.env_sync_Id is null AND es2.env_sync_Id is not null ");
            sqlText.Append("AND (op.MI_Envision_Deleted is null OR op.MI_Envision_Deleted =0) ");
            sqlText.Append("AND op.Opportunity_id = " + strOppId + " ");

            //AAB 05-16-2010 For newly sold specs need to union in 0 change orders These don't need to have sync records
            //Currently it will union in all cases. There may be a need in the future to only do this in some cases
            //if (true)
            //{
                sqlText.Append("UNION ");
                sqlText.Append("Select coo.code_, coo.opportunity_product_id as opportunity__product_id, coo.nbhdp_product_id ");
                sqlText.Append("FROM ");
                sqlText.Append("    change_order_options coo ");
                sqlText.Append("    left outer join change_order co on coo.change_order_id = co.change_order_id ");
                sqlText.Append("    left outer join opportunity o on co.opportunity_id = o.opportunity_id ");
                sqlText.Append("    left outer join opportunity__product op on coo.opportunity_product_id = op.opportunity__product_id ");
                sqlText.Append("    left outer join env_sync es1 on op.opportunity__product_Id = es1.opportunity_product_id ");
                sqlText.Append("    left outer join env_sync es2 on op.opportunity_Id = es2.Opportunity_Id ");
                sqlText.Append("    left outer join env_sync es3 on o.MI_Originating_Inv_Quote = es3.Opportunity_Id ");
                sqlText.Append("    left outer join opportunity__product op2 on op.Replaced_By_Option_Id = op2.opportunity__product_id ");
                sqlText.Append("WHERE ");
                sqlText.Append("((coo.Selected = 0 OR coo.Selected is null)) ");
                sqlText.Append("AND op.Option_Selection_Source = 1 ");
                sqlText.Append("AND (op.Replaced_By_Option_Id is null) ");
                sqlText.Append("AND es1.env_sync_Id is null AND (es2.env_sync_Id is not null or es3.env_sync_Id is not null) ");
                sqlText.Append("AND mi_Pchangeorderstatus = 'DELETE' ");
                sqlText.Append("AND co.Change_Order_number = 0 ");
                sqlText.Append("AND coo.Opportunity_id = " + strOppId + " ");
            //}

            rstDO = objLib.GetRecordset(sqlText.ToString());

            return rstDO;


        }
        public bool HasQueuedChanges(object vntContractId, object vntProductId, IRSystem7 rSys)
        {
            object vntRetContractId = null;
            String strContractId = rSys.IdToString(vntContractId);
            String strProductId = rSys.IdToString(vntProductId);
            bool retValue = false;
            
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            Recordset rstOpp = new Recordset();
            StringBuilder sqlText = new StringBuilder();


            sqlText.Append("SELECT ");
            sqlText.Append("    o.Opportunity_Id ");
            sqlText.Append("FROM ");
            sqlText.Append("    Opportunity o ");
            sqlText.Append("WHERE ");
            sqlText.Append("    o.Lot_Id = " + strProductId);
            sqlText.Append(" AND (o.Pipeline_Stage = 'Post Sale' OR o.Pipeline_Stage = 'Post Build') AND o.Status = 'In Progress'");
            sqlText.Append(" AND (o.Inactive = 0 OR o.Inactive is null)");

            rstOpp = objLib.GetRecordset(sqlText.ToString());

            if (rstOpp.RecordCount > 0)
            {
                retValue = true;
                return retValue;
            }

                        
            //lookup queued inbound options
            DataAccess objLib2 = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            Recordset rstOpp2 = new Recordset();
            StringBuilder sqlText2 = new StringBuilder();


            sqlText2.Append("SELECT ");
            sqlText2.Append("    o.Opportunity_Id ");
            sqlText2.Append("FROM ");
            sqlText2.Append("    env_buyer_selections o ");
            sqlText2.Append("WHERE ");
            sqlText2.Append("    o.Opportunity_Id = " + strContractId);

            rstOpp2 = objLib.GetRecordset(sqlText2.ToString());

            if (rstOpp2.RecordCount > 0)
            {
                retValue = true;
                return retValue;
            }


            return retValue;


        }
        public Recordset GetOptionsToDeleteForSpec(object vntOppId, IRSystem7 rSys)
        {
            String strOppId = rSys.IdToString(vntOppId);

            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            Recordset rstDO = new Recordset();
            StringBuilder sqlText = new StringBuilder();

            //sqlText.Append("Select op.code_, op.opportunity__product_id, op.nbhdp_product_Id ");
            //sqlText.Append("FROM ");
            //sqlText.Append("    opportunity__product op ");
            //sqlText.Append("    left outer join env_sync es1 on op.opportunity__product_Id = es1.opportunity_product_id ");
            //sqlText.Append("    left outer join env_sync es2 on op.opportunity_Id = es2.Opportunity_Id ");
            //sqlText.Append("WHERE ");
            //sqlText.Append("((op.Selected = 0 OR op.Selected is null) OR (op.Option_Selection_Source = 1 AND op.Replaces_Option_Id is not null)) ");
            //sqlText.Append("AND es1.env_sync_Id is null AND es2.env_sync_Id is not null AND op.Replaced_By_Option_Id is null ");
            //sqlText.Append("AND (op.MI_Envision_Deleted is null OR op.MI_Envision_Deleted =0) ");
            //sqlText.Append("AND op.Opportunity_id = " + strOppId);

            sqlText.Append("Select op.code_, op.opportunity__product_id, op.nbhdp_product_Id ");
            sqlText.Append("FROM ");
            sqlText.Append("    opportunity__product op ");
            sqlText.Append("    left outer join env_sync es1 on op.opportunity__product_Id = es1.opportunity_product_id ");
            sqlText.Append("    left outer join opportunity__product op2 on op.Replaced_By_Option_Id = op2.opportunity__product_id ");
            sqlText.Append("WHERE ");
            sqlText.Append("((op.Selected = 0 OR op.Selected is null) AND (op.Option_Selection_Source = 1)) ");
            sqlText.Append("AND (op2.Option_Selection_Source <> 1 OR op.Replaced_By_Option_Id is null) ");
            sqlText.Append("AND es1.env_sync_Id is null ");
            sqlText.Append("AND (op.MI_Envision_Deleted is null OR op.MI_Envision_Deleted =0) ");
            sqlText.Append("AND op.Opportunity_id = " + strOppId);


            rstDO = objLib.GetRecordset(sqlText.ToString());

            return rstDO;


        }
    }
}
