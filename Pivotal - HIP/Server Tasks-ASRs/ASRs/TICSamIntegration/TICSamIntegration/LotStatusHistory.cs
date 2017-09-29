using System;
using System.Collections.Generic;
using System.Text;

using Pivotal.Interop.RDALib;
using Pivotal.Interop.ADODBLib;
using Pivotal.Application.Foundation.Utility;
using Pivotal.Application.Foundation.Data.Element;
//using Pivotal.Application.TIC.Sale;


namespace Pivotal.Application.TIC.SAMIntegration
{
    /// <summary>
    /// This class will be used to extend the Lot Status History Integration.
    /// Hooks up to TIC Int Lot Status History Active Form called from SSIS
    /// <Author>A.Maldonado</Author>
    /// <CreatedOn>09/16/2010</CreatedOn>
    /// </summary>
    public class LotStatusHistory : IRFormScript
    {
        #region Class Vars

        IRSystem7 rSys = null;

        #endregion

        #region IRFormScript Members

        /// <summary>
        /// This method will be extended to handle all Sale and Lot Status History
        /// Integration requirements
        /// </summary>
        /// <param name="pForm"></param>
        /// <param name="Recordsets"></param>
        /// <param name="ParameterList"></param>
        /// <returns></returns>
        public object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {           
            object[] recordsetArray = (object[])Recordsets;
            Recordset rstLotStatusHistory = (Recordset)recordsetArray[0];
                       

            DataAccess objLib = 
                (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();
            
            //Set current user
            object employeeId = GetEmployeeForUserId();
            if (employeeId != DBNull.Value)
            {
                rstLotStatusHistory.Fields[modIntegration.TIC_Changed_By_Employee_Id].Value = employeeId;
            }

           
            //Check Lot Status Changed to
            switch (TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discLot_Status_Changed_To].Value))
            {                               
                case LotChangeStatus.NotReleased:
                    //Only thing to do for Not Release records is attach to the lot in sam
                    ProcessLotStatusWithNoContract(rstLotStatusHistory, GetSAMLotStatusId(LotChangeStatus.NotReleased));
                    break;
                case LotChangeStatus.Released:
                    ProcessLotStatusWithNoContract(rstLotStatusHistory, GetSAMLotStatusId(LotChangeStatus.Released));
                    ClearOutBuyerInfoForCancellations(rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Id].Value);
                    break;
                case LotChangeStatus.Available:
                    ProcessLotStatusWithNoContract(rstLotStatusHistory, GetSAMLotStatusId(LotChangeStatus.Released));
                    ClearOutBuyerInfoForCancellations(rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Id].Value);
                    break;
                case LotChangeStatus.Reserved:
                    ProcessSaleIntoSAM(rstLotStatusHistory, objLib, GetSAMLotStatusId(LotChangeStatus.Reserved), false);
                    break;
                case LotChangeStatus.Sold:
                    ProcessSaleIntoSAM(rstLotStatusHistory, objLib, GetSAMLotStatusId(LotChangeStatus.Sold), false);
                    break;
                case LotChangeStatus.Closed:
                    ProcessSaleIntoSAM(rstLotStatusHistory, objLib, GetSAMLotStatusId(LotChangeStatus.Closed), true);
                    break;
                case LotChangeStatus.CancelledReserve:
                    ProcessSaleIntoSAM(rstLotStatusHistory, objLib, GetSAMLotStatusId(LotChangeStatus.CancelledReserve), true);
                    ClearOutBuyerInfoForCancellations(rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Id].Value);
                    break;
                case LotChangeStatus.Cancelled:
                    ProcessSaleIntoSAM(rstLotStatusHistory, objLib, GetSAMLotStatusId(LotChangeStatus.Cancelled), true);
                    ClearOutBuyerInfoForCancellations(rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Id].Value);
                    break;
                case LotChangeStatus.TransferSale:
                    ProcessTransferIntoSAM(rstLotStatusHistory, objLib, LotChangeStatus.Cancelled);
                    break;
                case LotChangeStatus.TransferReserve:
                    ProcessTransferIntoSAM(rstLotStatusHistory, objLib, LotChangeStatus.CancelledReserve);
                    break;
                default:
                    throw new PivotalApplicationException("Invalid Lot Status History Value");   
                    
            }

            //Create new Lot Status History Record
            object vntLotStatusHistoryId = pForm.DoAddFormData(Recordsets, ref ParameterList);
            return vntLotStatusHistoryId;

            
        }

        public void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            pForm.DoDeleteFormData(RecordId, ref ParameterList);
        }

        public void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {
            throw new Exception("The method or operation is not implemented.");
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
        /// This method will be implemented to solely handle Rollbacks for Sales and Reservations
        /// </summary>
        /// <param name="pForm"></param>
        /// <param name="Recordsets"></param>
        /// <param name="ParameterList"></param>
        public void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {

            object[] recordsetArray = (object[])Recordsets;
            Recordset rstLotStatusHistory = (Recordset)recordsetArray[0];
            
            DataAccess objLib =
                (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();

            //Set current user
            object employeeId = GetEmployeeForUserId();
            if (employeeId != DBNull.Value)
            {
                rstLotStatusHistory.Fields[modIntegration.TIC_Changed_By_Employee_Id].Value = employeeId;
            }


            //Check Lot Status Changed to
            switch (TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discLot_Status_Changed_To].Value))
            {
                case LotChangeStatus.RollbackReserve:
                    ProcessRollbackIntoSAM(rstLotStatusHistory, false, objLib);
                    ClearOutBuyerInfoForCancellations(rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Id].Value);
                    break;
                case LotChangeStatus.RollbackSale:
                    ProcessRollbackIntoSAM(rstLotStatusHistory, true, objLib);
                    break;
                default:
                    break;
            
            }

            pForm.DoSaveFormData(Recordsets, ref ParameterList);
        }

        public void SetSystem(RSystem pSystem)
        {
            rSys = (IRSystem7)pSystem;
        }

        #endregion


        #region Processing Methods

        /// <summary>
        /// This method will process the Not Release Lot Status History Message from HIP
        /// </summary>
        /// <param name="rstLotStatusHistory"></param>
        public virtual void ProcessLotStatusWithNoContract(Recordset rstLotStatusHistory, object vntLotStatusId)
        {
            rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value = vntLotStatusId;
            rstLotStatusHistory.Fields[modIntegration.TIC_Changed_By_Id].Value
                = TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discChanged_By_Id].Value);
            //Sync Lot record
            SyncLotStatusWithLotHistory(rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Id].Value,
                vntLotStatusId, TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discLot_Status_Changed_To].Value),
                TypeConvert.ToDateTime(rstLotStatusHistory.Fields[modIntegration.TIC_Date_Business_Transaction].Value), 
                rstLotStatusHistory);

        }

        /// <summary>
        /// This method will process a reserved lot status into SAM
        /// </summary>
        /// <param name="rstLotStatusHistory"></param>
        public virtual void ProcessSaleIntoSAM(Recordset rstLotStatusHistory, DataAccess objLib, 
            object vntLotStatusId, bool isClosed)
        { 
            //Set the Lot Status on the Lot Status History
            rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value = vntLotStatusId;

            Recordset rstExistSale = GetExistingSaleRecord(TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discHIPOpportunityId].Value), objLib);
            if (rstExistSale.RecordCount > 0)
            {
                rstExistSale.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value = vntLotStatusId;

                //Update Sale Record
                if (isClosed)
                {
                    MappContractCloseFields(rstExistSale, rstLotStatusHistory);
                }
                else
                {
                    MappContractSaleFields(rstExistSale, rstLotStatusHistory);
                }

                

                //Set the Sale Dates in SAM to match the Lot
                SetSaleDates(rstExistSale, TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discLot_Status_Changed_To].Value),
                    TypeConvert.ToDateTime(rstLotStatusHistory.Fields[modIntegration.TIC_Date_Business_Transaction].Value));

                //Set the Sale Id on the Lot Status History
                rstLotStatusHistory.Fields[modIntegration.TIC_Sale_Id].Value = rstExistSale.Fields[modIntegration.TIC_Sale_Id].Value;

                objLib.SaveRecordset(modIntegration.strtTIC_Sale, rstExistSale);
                rstExistSale.Close();
            }
            else
            { 
                //Insert new Sale
                Recordset rstSale = objLib.GetNewRecordset(modIntegration.strtTIC_Sale, GetSaleFieldList());
                rstSale.AddNew(Type.Missing, Type.Missing);
                //Using data from the Disconnected fields create new Sale record
                if(isClosed)
                {
                    MappContractCloseFields(rstSale, rstLotStatusHistory);
                }
                else
                {
                    MappContractSaleFields(rstSale, rstLotStatusHistory);
                }
                               
                //Set the Sale Dates in SAM to match the Lot
                SetSaleDates(rstSale, TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discLot_Status_Changed_To].Value),
                    TypeConvert.ToDateTime(rstLotStatusHistory.Fields[modIntegration.TIC_Date_Business_Transaction].Value));

                objLib.SaveRecordset(modIntegration.strtTIC_Sale, rstSale);

                //Set the Sale Id on the Lot Status History after the record is saved
                rstLotStatusHistory.Fields[modIntegration.TIC_Sale_Id].Value = rstSale.Fields[modIntegration.TIC_Sale_Id].Value;
                
                rstSale.Close();

            }                  
     
            //Sync Lot record
            SyncLotStatusWithLotHistory(rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Id].Value, vntLotStatusId,
                TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discLot_Status_Changed_To].Value),
                TypeConvert.ToDateTime(rstLotStatusHistory.Fields[modIntegration.TIC_Date_Business_Transaction].Value),
                rstLotStatusHistory);

        
        }


        /// <summary>
        /// This method will ensure that any sales contract or reservation that was transfered gets
        /// Canceled on the SAM Side.
        /// </summary>
        /// <param name="rstLotStatusHistory"></param>
        /// <param name="objLib"></param>
        public virtual void ProcessTransferIntoSAM(Recordset rstLotStatusHistory, DataAccess objLib, string statusLookup)
        { 
            //Need to get the existing contract for the lot that was transfered from and 
            //ensure that the sale record is canceled.
            Recordset rstExistSale 
                = GetExistingSaleRecord(TypeConvert.ToString(rstLotStatusHistory
                .Fields[modIntegration.discHIPOpportunityId].Value), objLib);

            object vntLotStatusId = GetSAMLotStatusId(statusLookup);

            if (rstExistSale.RecordCount > 0)
            {
               
                //Get Lot Sale Status Id for Cancelled
                rstExistSale.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value = vntLotStatusId;  
             
                //Set Cancelled Date or Cancelled Reserved Date for Transfer
                if (statusLookup == LotChangeStatus.CancelledReserve)
                {
                    rstExistSale.Fields[modIntegration.TIC_Date_Reservation_Cancelled].Value = DateTime.Now;
                    //For Transfer Reserves, when cancelling the reserve set the soft delte
                    rstExistSale.Fields[modIntegration.TIC_Deleted].Value = true;
                }
                else if (statusLookup == LotChangeStatus.Cancelled)
                {
                    rstExistSale.Fields[modIntegration.TIC_Date_Sale_Cancelled].Value = DateTime.Now;
                }

                //Always set the TIC_Transfer fields for either a Transfer Reserve or a Transfer Sale
                rstExistSale.Fields[modIntegration.TIC_Transfer].Value
                    = TypeConvert.ToBoolean(rstLotStatusHistory.Fields[modIntegration.discTIC_Transfer].Value);
                rstExistSale.Fields[modIntegration.TIC_Transfer_To_Lot_Id].Value
                    = rstLotStatusHistory.Fields[modIntegration.discTIC_Transfer_To_Lot_Id].Value;
                rstExistSale.Fields[modIntegration.TIC_Transfer_From_Lot_Id].Value 
                    = rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Id].Value;


                objLib.SaveRecordset(modIntegration.strtTIC_Sale, rstExistSale);
                rstExistSale.Close();
            }

            //Sync Lot record
            //AM2010.09.27 - Don't set the Lot Status on the Lot, because the Available status that is created when a 
            //lot is transfered will handle this.
            //SyncLotStatusWithLotHistory(rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Id].Value, vntLotStatusId);

        
        
        }


        /// <summary>
        /// This method will process rollbacks into SAM from HIP
        /// </summary>
        /// <param name="rstLotStatusHistory"></param>
        public virtual void ProcessRollbackIntoSAM(Recordset rstLotStatusHistory, bool isSale, DataAccess objLib)
        { 
            //Check if this rollback is for a sale or a reservation.
            object vntLotSaleStatusId = null;
            Recordset rstExistSale = null;
            //If rollback Sale to Reservation we need to do the following
            if (isSale)
            {
                //Get the Lot Sale Status = "Reserved"
                vntLotSaleStatusId = GetSAMLotStatusId(LotChangeStatus.Reserved);
                //1) update existing Lot Sale History record Rollback = true and Sale Status = "Reserved" and Rollback Date = GetDate()
                rstLotStatusHistory.Fields[modIntegration.TIC_Rollback_Date].Value = DateTime.Now;
                rstLotStatusHistory.Fields[modIntegration.TIC_Rollback_Indic].Value = true;
                //Don't set the lot status of the rolled back record, only set the rolled back flag
                //rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value = vntLotSaleStatusId;
                //2) Update Lot to "Reserved"
                SyncLotStatusWithLotHistory(rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Id].Value, vntLotSaleStatusId,
                    LotChangeStatus.Reserved, TypeConvert.ToDateTime(rstLotStatusHistory.Fields[modIntegration.TIC_Date_Business_Transaction].Value),
                    rstLotStatusHistory);
                //3) Update Sale record to "Reserved"
                rstExistSale = GetExistingSaleRecord(TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discHIPOpportunityId].Value), objLib);
                if (rstExistSale.RecordCount > 0)
                {                   
                    //Update Status to reserved
                    rstExistSale.Fields[modIntegration.TIC_Conversion_Indic].Value = false;
                    rstExistSale.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value = vntLotSaleStatusId;
                    objLib.SaveRecordset(modIntegration.strtTIC_Sale, rstExistSale);
                    
                }
                //Clean up resources
                rstExistSale.Close();
            }
            else
            {

                //If rollback reservation we need to do the following
                //Get Rollback Status to "Released"
                vntLotSaleStatusId = GetSAMLotStatusId(LotChangeStatus.Released);
                //1) Update existing Lot Sale History record Rollback = true and Sale Status = "Released" and Rollback Date = GetDate()
                rstLotStatusHistory.Fields[modIntegration.TIC_Rollback_Date].Value = DateTime.Now;
                rstLotStatusHistory.Fields[modIntegration.TIC_Rollback_Indic].Value = true;
                //rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value = vntLotSaleStatusId;
                //2) Update Lot to "Released"
                SyncLotStatusWithLotHistory(rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Id].Value, vntLotSaleStatusId,
                    LotChangeStatus.Released, TypeConvert.ToDateTime(rstLotStatusHistory.Fields[modIntegration.TIC_Date_Business_Transaction].Value),
                    rstLotStatusHistory);
                //3) Update Sale Record TIC_Deleted = True
                rstExistSale = GetExistingSaleRecord(TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discHIPOpportunityId].Value), objLib);
                if (rstExistSale.RecordCount > 0)
                {
                    //Update Status Canceled
                    rstExistSale.Fields[modIntegration.TIC_Deleted].Value = true;
                    objLib.SaveRecordset(modIntegration.strtTIC_Sale, rstExistSale);

                }
                //Clean up resources
                rstExistSale.Close();
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Updates the associated lot status to keep in sync with the Lot Status History record
        /// </summary>
        /// <param name="vntLotId"></param>
        /// <param name="vntLotStatusId"></param>
        /// <param name="objLib"></param>
        public virtual void SyncLotStatusWithLotHistory(object vntLotId, object vntLotStatusId, string lookUpLotStatus,
            DateTime busTransDate, Recordset rstPrimary)
        {
            //Get Lot Status from SAM
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                       
            //Set the Lot Status Date on the Lot
            //For each Lot Status set the Appropriate Date to today.
            string[] arrFields = new string[] { modIntegration.TIC_Date_Sold, modIntegration.TIC_Date_Sale_Cancelled,
                                                modIntegration.TIC_Date_Reserved, modIntegration.TIC_Date_Released,
                                                modIntegration.TIC_Date_Closed, modIntegration.TIC_Date_Reservation_Cancelled,
                                                modIntegration.TIC_Lot_Sale_Status_Id, modIntegration.TIC_Structural_Option_Preplots,
                                                modIntegration.TIC_Design_Center_Preplots, modIntegration.TIC_ICDC_Room_Preplot,
                                                modIntegration.TIC_Buyer_1_Contact_Id, modIntegration.TIC_Buyer_2_Contact_Id};
            
           
            Recordset rstLot = objLib.GetRecordset(vntLotId, modIntegration.strtTIC_Lot, arrFields);
            
            //Check each status to see what date to set
            switch (lookUpLotStatus)
            { 
                case LotChangeStatus.Sold:
                    rstLot.Fields[modIntegration.TIC_Date_Sold].Value = TypeConvert.ToDateTime(busTransDate);
                    //Set Price Fields
                    SetPriceFields(rstLot, rstPrimary);
                    break;  
                case LotChangeStatus.Cancelled:
                    rstLot.Fields[modIntegration.TIC_Date_Sale_Cancelled].Value = TypeConvert.ToDateTime(busTransDate);
                    SetPriceFields(rstLot, rstPrimary);
                    break;
                case LotChangeStatus.CancelledReserve:
                    rstLot.Fields[modIntegration.TIC_Date_Reservation_Cancelled].Value = TypeConvert.ToDateTime(busTransDate);
                    break;
                case LotChangeStatus.Reserved:
                    //Set Price Fields
                    rstLot.Fields[modIntegration.TIC_Date_Reserved].Value = TypeConvert.ToDateTime(busTransDate);
                    SetPriceFields(rstLot, rstPrimary);
                    break;
                case LotChangeStatus.Available:
                    //Set Price Fields
                    rstLot.Fields[modIntegration.TIC_Date_Released].Value = TypeConvert.ToDateTime(busTransDate);
                    SetPriceFields(rstLot, rstPrimary);
                    break;
                case LotChangeStatus.Released:
                    //Set Price Fields                    
                    rstLot.Fields[modIntegration.TIC_Date_Released].Value = TypeConvert.ToDateTime(busTransDate);
                    SetPriceFields(rstLot, rstPrimary);
                    break;
                case LotChangeStatus.Closed:
                    //Set Price Fields
                    rstLot.Fields[modIntegration.TIC_Date_Closed].Value = TypeConvert.ToDateTime(busTransDate);
                    SetPriceFields(rstLot, rstPrimary);
                    break;
                default:
                    break;
                      
            
            }
            
            rstLot.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value = vntLotStatusId;
            
            //2011.03.09 - Set Buyer and Co-Buyer on lot record
            rstLot.Fields[modIntegration.TIC_Buyer_1_Contact_Id].Value = rstPrimary.Fields[modIntegration.discSAM_Buyer_Id].Value;
            rstLot.Fields[modIntegration.TIC_Buyer_2_Contact_Id].Value = rstPrimary.Fields[modIntegration.discSAM_CoBuyer_Id].Value;
            

            objLib.SaveRecordset(modIntegration.strtTIC_Lot, rstLot);
            rstLot.Close();

        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vntLotId"></param>
        public virtual void ClearOutBuyerInfoForCancellations(object vntLotId)
        {
            //Get Lot Status from SAM
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

            //Set the Lot Status Date on the Lot
            //For each Lot Status set the Appropriate Date to today.
            string[] arrFields = new string[] { modIntegration.TIC_Buyer_1_Contact_Id, modIntegration.TIC_Buyer_2_Contact_Id};


            Recordset rstLot = objLib.GetRecordset(vntLotId, modIntegration.strtTIC_Lot, arrFields);
       
            //2011.03.09 - Set Buyer and Co-Buyer on lot record
            rstLot.Fields[modIntegration.TIC_Buyer_1_Contact_Id].Value = DBNull.Value;
            rstLot.Fields[modIntegration.TIC_Buyer_2_Contact_Id].Value = DBNull.Value;


            objLib.SaveRecordset(modIntegration.strtTIC_Lot, rstLot);
            rstLot.Close();


        }

        /// <summary>
        /// This method will do a lookup on an existing Sale record in HIP
        /// </summary>
        /// <param name="strHipSaleLookup"></param>
        /// <returns></returns>
        public virtual Recordset GetExistingSaleRecord(string strHipSaleLookup, DataAccess objLib)
        {
                      

            //Check for existing Contract for 
            Recordset rstSale = objLib.GetRecordset(modIntegration.strqTIC_SALES_LOOKUP, 1,
                TypeConvert.ToString(strHipSaleLookup), GetSaleFieldList());


            return rstSale;


        }

        /// <summary>
        /// GEt the SAM Lot Status record id for processing into SAM
        /// </summary>
        /// <param name="statusLookUp"></param>
        /// <returns></returns>
        public virtual object GetSAMLotStatusId(string statusLookUp)
        {
            //Get Lot Status from SAM
            DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

            Recordset rstLotSalesStatus = objLib.GetRecordset(modIntegration.strqTIC_SALES_STATUS_LOOKUP,
                1, statusLookUp, modIntegration.TIC_Lot_Sale_Status_Id);

            if (rstLotSalesStatus.RecordCount == 0)
            {
                //Throw an exception since the status could not be found inSAM
                throw new PivotalApplicationException("Invalid Lot Status History Value");
            }

            //Get SAM Status reference on the incoming Lot Status History record
            object vntLotStatusId = rstLotSalesStatus.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value;

            return vntLotStatusId;

        }

        /// <summary>
        /// This method will mapp all fields that are related to the Close of the
        /// Contract
        /// </summary>
        /// <param name="rstSale"></param>
        public virtual void MappContractCloseFields(Recordset rstSale, Recordset rstIncoming)
        {

            //Mapp all Sale Contract fields
            rstSale.Fields[modIntegration.TIC_Project_Id].Value = rstIncoming.Fields[modIntegration.discTIC_Neighborhood_Id].Value;
            rstSale.Fields[modIntegration.TIC_Tract_Id].Value = rstIncoming.Fields[modIntegration.discTIC_Tract_Id].Value;
            rstSale.Fields[modIntegration.TIC_Lot_Id].Value = rstIncoming.Fields[modIntegration.TIC_Lot_Id].Value;
            rstSale.Fields[modIntegration.TIC_Buyer_1_Contact_Id].Value = rstIncoming.Fields[modIntegration.discSAM_Buyer_Id].Value;
            rstSale.Fields[modIntegration.TIC_Buyer_2_Contact_Id].Value = rstIncoming.Fields[modIntegration.discSAM_CoBuyer_Id].Value;
            rstSale.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value = rstIncoming.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value;
            rstSale.Fields[modIntegration.TIC_Estimated_Closing_Date].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discECOE_Date].Value);
            rstSale.Fields[modIntegration.TIC_Sale_Status_Last_Change_Dt].Value = DateTime.Now;

            rstSale.Fields[modIntegration.TIC_Cancellation_Reason].Value = TypeConvert.ToString(rstIncoming.Fields[modIntegration.discCancel_Reason].Value);
            //rstSale.Fields[modIntegration.TIC_Date_Reserved].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discReservation_Date].Value);
            //rstSale.Fields[modIntegration.TIC_Date_Reservation_Cancelled].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discTIC_Reservation_Can_Date].Value);
            //rstSale.Fields[modIntegration.TIC_Date_Sold].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discActual_Revenue_Date].Value);
            //rstSale.Fields[modIntegration.TIC_Date_Sale_Cancelled].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discCancel_Date].Value);
            //rstSale.Fields[modIntegration.TIC_Date_Closed].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discActual_Revenue_Date].Value);
            
            rstSale.Fields[modIntegration.TIC_Closing_Base_Price].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discAdditional_Price].Value);
            rstSale.Fields[modIntegration.TIC_Closing_Elevation_Premium].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discElevation_Premium].Value);
            rstSale.Fields[modIntegration.TIC_Closing_Location_Premium].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discLot_Premium].Value);
            
            //AM2010.11.10 - Fixed 
            rstSale.Fields[modIntegration.TIC_Closing_Premium].Value 
                = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discLot_Premium].Value)
                    + TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discElevation_Premium].Value);

            rstSale.Fields[modIntegration.TIC_Closing_Homebuyer_Extr_Opt].Value = TypeConvert.ToInt32(rstIncoming.Fields[modIntegration.discTIC_Design_Options_Total].Value);
            rstSale.Fields[modIntegration.TIC_Closing_Pre_Plots_Price].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discTIC_Preplot_Options].Value);
            rstSale.Fields[modIntegration.TIC_HIP_External_Source_Id].Value = rstIncoming.Fields[modIntegration.discHIPOpportunityId].Value;

            //AM2010.11.10 - Added closing incentive
            rstSale.Fields[modIntegration.TIC_Closing_Incentive_Price].Value =
                TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discTIC_Selling_Incentive].Value);

             rstSale.Fields[modIntegration.TIC_Closing_Room_Preplot].Value =
                TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discTIC_Preplotted_Structural_Option].Value) 
                    + TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discTIC_Preplot_Options].Value);

            if (rstIncoming.Fields[modIntegration.discBroker_Used_In_Sale].Value != null)
            {
                rstSale.Fields[modIntegration.TIC_Broker_Used_In_Sale_Indic].Value = true;
            }
            if (rstIncoming.Fields[modIntegration.discContingency_Sale].Value != null)
            {
                rstSale.Fields[modIntegration.TIC_Contingency_Sale].Value = true;
            }                   
            
        }

        /// <summary>
        /// This method will mapp all fields that are related to the Sale of the
        /// Contract
        /// </summary>
        /// <param name="rstSale"></param>
        public virtual void MappContractSaleFields(Recordset rstSale, Recordset rstIncoming)
        { 
        
            //Mapp all Sale Contract fields
            rstSale.Fields[modIntegration.TIC_Project_Id].Value = rstIncoming.Fields[modIntegration.discTIC_Neighborhood_Id].Value;
            rstSale.Fields[modIntegration.TIC_Tract_Id].Value = rstIncoming.Fields[modIntegration.discTIC_Tract_Id].Value;
            rstSale.Fields[modIntegration.TIC_Lot_Id].Value = rstIncoming.Fields[modIntegration.TIC_Lot_Id].Value;
            rstSale.Fields[modIntegration.TIC_Buyer_1_Contact_Id].Value = rstIncoming.Fields[modIntegration.discSAM_Buyer_Id].Value;
            rstSale.Fields[modIntegration.TIC_Buyer_2_Contact_Id].Value = rstIncoming.Fields[modIntegration.discSAM_CoBuyer_Id].Value;
            rstSale.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value = rstIncoming.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value;
            rstSale.Fields[modIntegration.TIC_Estimated_Closing_Date].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discECOE_Date].Value);
            rstSale.Fields[modIntegration.TIC_Sale_Status_Last_Change_Dt].Value = DateTime.Now;

            rstSale.Fields[modIntegration.TIC_Cancellation_Reason].Value = TypeConvert.ToString(rstIncoming.Fields[modIntegration.discCancel_Reason].Value);
            //rstSale.Fields[modIntegration.TIC_Date_Reserved].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discReservation_Date].Value);
            //rstSale.Fields[modIntegration.TIC_Date_Reservation_Cancelled].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discTIC_Reservation_Can_Date].Value);
            //rstSale.Fields[modIntegration.TIC_Date_Sold].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discActual_Revenue_Date].Value);
            //rstSale.Fields[modIntegration.TIC_Date_Sale_Cancelled].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discCancel_Date].Value);
            //rstSale.Fields[modIntegration.TIC_Date_Closed].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discActual_Revenue_Date].Value);
            rstSale.Fields[modIntegration.TIC_Base_Price].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discAdditional_Price].Value);
            rstSale.Fields[modIntegration.TIC_Selling_Elevation_Premium].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discElevation_Premium].Value);
            rstSale.Fields[modIntegration.TIC_Selling_Location_Premium].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discLot_Premium].Value);
            
            //AM2010.11.10 - Fixed to map to correct value from HIP (Lot Premium + Elevation Premium)
            rstSale.Fields[modIntegration.TIC_Premium_Price].Value 
                = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discLot_Premium].Value) 
                    + TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discElevation_Premium].Value);

            rstSale.Fields[modIntegration.TIC_Selling_Homebuyer_Extr_Opt].Value = TypeConvert.ToInt32(rstIncoming.Fields[modIntegration.discTIC_Design_Options_Total].Value);
            rstSale.Fields[modIntegration.TIC_Pre_Plots_Price].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discTIC_Preplot_Options].Value);
            rstSale.Fields[modIntegration.TIC_HIP_External_Source_Id].Value = rstIncoming.Fields[modIntegration.discHIPOpportunityId].Value;

            //AM2010.11.10 - added price fields from HIP
            rstSale.Fields[modIntegration.TIC_Incentive_Price].Value 
                = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discTIC_Selling_Incentive].Value);

            rstSale.Fields[modIntegration.TIC_Selling_Room_Preplot].Value =
                TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discTIC_Preplotted_Structural_Option].Value) 
                    + TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discTIC_Preplot_Options].Value);


            if(rstIncoming.Fields[modIntegration.discBroker_Used_In_Sale].Value != null)
            {
                rstSale.Fields[modIntegration.TIC_Broker_Used_In_Sale_Indic].Value = true;
            }
            if(rstIncoming.Fields[modIntegration.discContingency_Sale].Value != null)
            {
                rstSale.Fields[modIntegration.TIC_Contingency_Sale].Value = true;
            }

            //AM2010.09.29 - On sale always recalculate Contact Types (For both buyer and co-buyer)
            if (rstIncoming.Fields[modIntegration.discSAM_Buyer_Id].Value != DBNull.Value)
            {
                RecalculateContactTypes(rstIncoming.Fields[modIntegration.discSAM_Buyer_Id].Value);
            }

            if (rstIncoming.Fields[modIntegration.discSAM_CoBuyer_Id].Value != DBNull.Value)
            {
                RecalculateContactTypes(rstIncoming.Fields[modIntegration.discSAM_CoBuyer_Id].Value);
            }
        
        }

        /// <summary>
        /// This method will set the dates on the Sale record to sync with the lot sale dates
        /// </summary>
        /// <param name="rstSale"></param>
        public virtual void SetSaleDates(Recordset rstSale, string strSalesStatus, DateTime busTransDate)
        {
            //Check each status to see what date to set
            switch (strSalesStatus)
            {
                case LotChangeStatus.Sold:                    
                    //Set the TIC_Conversion = true for all Sold Records
                    rstSale.Fields[modIntegration.TIC_Date_Sold].Value = TypeConvert.ToDateTime(busTransDate);
                    rstSale.Fields[modIntegration.TIC_Conversion_Indic].Value = true;

                    break;
                case LotChangeStatus.Cancelled:
                    rstSale.Fields[modIntegration.TIC_Date_Sale_Cancelled].Value = TypeConvert.ToDateTime(busTransDate);
                    break;
                case LotChangeStatus.CancelledReserve:
                    rstSale.Fields[modIntegration.TIC_Date_Reservation_Cancelled].Value = TypeConvert.ToDateTime(busTransDate);
                    break;
                case LotChangeStatus.Reserved:
                    rstSale.Fields[modIntegration.TIC_Date_Reserved].Value = TypeConvert.ToDateTime(busTransDate);
                    rstSale.Fields[modIntegration.TIC_Conversion_Indic].Value = false;
                    break;               
                case LotChangeStatus.Closed:
                    rstSale.Fields[modIntegration.TIC_Date_Closed].Value = TypeConvert.ToDateTime(busTransDate);
                    break;
                default:
                    break;

            }
        }


        /// <summary>
        /// This method will set the Price fields on the lot record during a Lot Status History Update
        /// </summary>
        /// <param name="rstLot"></param>
        public virtual void SetPriceFields(Recordset rstLot, Recordset rstPrimary)
        { 

            //Set Price fields according to mappings
            rstLot.Fields[modIntegration.TIC_Structural_Option_Preplots].Value 
                = TypeConvert.ToDecimal(rstPrimary.Fields[modIntegration.discTIC_Preplotted_Structural_Option].Value);
            rstLot.Fields[modIntegration.TIC_Design_Center_Preplots].Value
                = TypeConvert.ToDecimal(rstPrimary.Fields[modIntegration.discTIC_Preplot_Options].Value);
            //rstLot.Fields[modIntegration.TIC_Selling_Incentive].Value
            //    = TypeConvert.ToDecimal(rstPrimary.Fields[modIntegration.discTIC_Selling_Incentive].Value);
            //rstLot.Fields[modIntegration.TIC_Premium_Price].Value
            //    = TypeConvert.ToDecimal(rstPrimary.Fields[modIntegration.discLot_Premium].Value)
            //        + TypeConvert.ToDecimal(rstPrimary.Fields[modIntegration.discElevation_Premium].Value);
            rstLot.Fields[modIntegration.TIC_ICDC_Room_Preplot].Value =
                TypeConvert.ToDecimal(rstPrimary.Fields[modIntegration.discTIC_Preplot_Options].Value)
                   + TypeConvert.ToDecimal(rstPrimary.Fields[modIntegration.discTIC_Preplotted_Structural_Option].Value);
        
        }

        /// <summary>
        /// This function will get an array of fields to use to create the new sale
        /// </summary>
        /// <returns></returns>
        public object[] GetSaleFieldList()
        {
            object[] arrFields = new object[]
            {
                modIntegration.TIC_Project_Id,
                modIntegration.TIC_Tract_Id,
                modIntegration.TIC_Lot_Id,
                modIntegration.TIC_Buyer_1_Contact_Id,
                modIntegration.TIC_Buyer_2_Contact_Id,
                modIntegration.TIC_Lot_Sale_Status_Id,
                modIntegration.TIC_Estimated_Closing_Date,
                modIntegration.TIC_Sale_Status_Last_Change_Dt,
                modIntegration.TIC_Broker_Used_In_Sale_Indic,
                modIntegration.TIC_Contingency_Sale,
                modIntegration.TIC_Cancellation_Reason,
                modIntegration.TIC_Date_Reserved,
                modIntegration.TIC_Date_Reservation_Cancelled,
                modIntegration.TIC_Date_Sold,
                modIntegration.TIC_Date_Sale_Cancelled,
                modIntegration.TIC_Date_Closed,
                modIntegration.TIC_Base_Price,
                modIntegration.TIC_Incentive_Price,
                modIntegration.TIC_Selling_Elevation_Premium,
                modIntegration.TIC_Selling_Location_Premium,
                modIntegration.TIC_Premium_Price,
                modIntegration.TIC_Selling_Upgrade_Preplot,
                modIntegration.TIC_Selling_Homebuyer_Extr_Opt,
                modIntegration.TIC_Selling_Models_Upgrade_Rec,
                modIntegration.TIC_Pre_Plots_Price,
                modIntegration.TIC_Closing_Base_Price,
                modIntegration.TIC_Closing_Incentive_Price,
                modIntegration.TIC_Closing_Elevation_Premium,
                modIntegration.TIC_Closing_Location_Premium,
                modIntegration.TIC_Closing_Premium,
                modIntegration.TIC_Closing_Upgrade_Preplot,
                modIntegration.TIC_Closing_Homebuyer_Extr_Opt,
                modIntegration.TIC_Closing_Models_Upgrade_Rec,
                modIntegration.TIC_Closing_Pre_Plots_Price,
                modIntegration.TIC_HIP_External_Source_Id,
                modIntegration.TIC_Deleted,
                modIntegration.TIC_Sale_Id,
                modIntegration.TIC_Conversion_Indic, 
                modIntegration.TIC_Selling_Room_Preplot,
                modIntegration.TIC_Closing_Room_Preplot,
                modIntegration.TIC_Transfer,
                modIntegration.TIC_Transfer_From_Lot_Id,
                modIntegration.TIC_Transfer_To_Lot_Id
              
            };
            return arrFields;
        }

        /// <summary>
        /// Get Employee record for 
        /// </summary>
        /// <returns></returns>
        public object GetEmployeeForUserId()
        {
            object employeeId = rSys.Tables[modIntegration.strtEMPLOYEE].Fields[modIntegration.strfEMPLOYEE_ID].FindValue(
                rSys.Tables[modIntegration.strtEMPLOYEE].Fields[modIntegration.strfEMPLOYEE_USER_ID], rSys.CurrentUserId());
            return employeeId;

        }


        #region Contact Village Re-Calculations (Copied from TIC.Sale)

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vntContactId"></param>
        protected virtual void RecalculateContactTypes(object vntContactId)
        {
            try
            {
                if (!(Convert.IsDBNull(vntContactId)))
                {
                    DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    objLib.PermissionIgnored = true;

                    // In Step 3, we will recalculate Contact.Type.  By default, assume it to be "Prospect";
                    string strContactType = modSale.CONTACT_TYPE_PROSPECT;
                    // strCurrentContactVillageInterestType variable used in Step 2
                    string strCurrentContactVillageInterestType = "";

                    // ## 1 # RECALCULATE TIC_CONTACT_VILLAGE_PROJECT.TIC_CONTACT_TYPE
                    // Get all TIC_Contact_Village_Project records related to the Contact_Id supplied.  
                    // Below, we will recalculate the TIC_Contact_Type for each.
                    Recordset rstContactVillageProject = objLib.GetLinkedRecordset(modSale.strtTIC_CONTACT_VILLAGE_PROJECT, modSale.strfTIC_CONTACT_ID, vntContactId,
                                                                                   modSale.strfTIC_CONTACT_VILLAGE_PROJECT_ID, modSale.strfTIC_CONTACT_ID, modSale.strfTIC_PROJECT_ID, modSale.strfTIC_CONTACT_TYPE);

                    if (rstContactVillageProject.RecordCount > 0)
                    {
                        while (!rstContactVillageProject.EOF)
                        {
                            // Calculate the TIC_Contact_Type for the current TIC_Contact_Village_Project record
                            rstContactVillageProject.Fields[modSale.strfTIC_CONTACT_TYPE].Value = CalculateContactVillageProjectInterestType(vntContactId, rstContactVillageProject.Fields[modSale.strfTIC_PROJECT_ID].Value);
                            // Process the next TIC_Contact_Village_Project record
                            rstContactVillageProject.MoveNext();
                        }
                        //Save the updates back to the TIC_Contact_Village_Project table
                        objLib.SaveRecordset(modSale.strtTIC_CONTACT_VILLAGE_PROJECT, rstContactVillageProject);
                    }

                    // ## 2 # RECALCULATE TIC_CONTACT_VILLAGE.TIC_CONTACT_TYPE
                    Recordset rstContactVillage = objLib.GetLinkedRecordset(modSale.strtTIC_CONTACT_VILLAGE, modSale.strfTIC_CONTACT_ID, vntContactId,
                                                                            modSale.strfTIC_CONTACT_VILLAGE_ID, modSale.strfTIC_CONTACT_ID, modSale.strfTIC_VILLAGE_ID, modSale.strfTIC_CONTACT_TYPE);

                    if (rstContactVillage.RecordCount > 0)
                    {
                        while (!rstContactVillage.EOF)
                        {
                            // Calculate the TIC_Contact_Type for the current TIC_Contact_Village record
                            strCurrentContactVillageInterestType = CalculateContactVillageInterestType(rstContactVillage.Fields[modSale.strfTIC_CONTACT_VILLAGE_ID].Value);
                            // Update the TIC_Contact_Village.TIC_Contact_Type field with strCurrentContactVillageInterestType
                            rstContactVillage.Fields[modSale.strfTIC_CONTACT_TYPE].Value = strCurrentContactVillageInterestType;

                            // We can also calculate what the Contact.Type value should be from the returned strCurrentContactVillageInterestType value.
                            // If it returned "Prospect" and strContactType <> "Customer", then set strContactType = Prospect
                            // If it returned "Buyer" or "Homeowner", then set strContactType = "Customer"
                            // The logic in place means that strContactType can never go back from "Customer" to "Prospect"
                            switch (strCurrentContactVillageInterestType)
                            {
                                case modSale.CONTACT_VILLAGE_TYPE_PROSPECT:
                                    if (strContactType != modSale.CONTACT_TYPE_CUSTOMER)
                                    {
                                        strContactType = modSale.CONTACT_TYPE_PROSPECT;
                                    }
                                    break;

                                case modSale.CONTACT_VILLAGE_TYPE_BUYER:
                                case modSale.CONTACT_VILLAGE_TYPE_HOMEOWNER:
                                    strContactType = modSale.CONTACT_TYPE_CUSTOMER;
                                    break;
                                default:
                                    break;
                            }

                            // Process the next TIC_Contact_Village record
                            rstContactVillage.MoveNext();
                        }
                        // Save the updates back to the TIC_Contact_Village table
                        objLib.SaveRecordset(modSale.strtTIC_CONTACT_VILLAGE, rstContactVillage);
                    }

                    // ## 3 ## UPDATE CONTACT.TYPE WITH strContactType
                    Recordset rstContact = objLib.GetRecordset(vntContactId, modSale.strtCONTACT,
                                                               modSale.strfCONTACT_ID, modSale.strfTYPE);

                    if (rstContact.RecordCount > 0)
                    {
                        // Update Contact.Type with strContactType
                        rstContact.Fields[modSale.strfTYPE].Value = strContactType;
                        // Save the updates back to the Contact table
                        objLib.SaveRecordset(modSale.strtCONTACT, rstContact);
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, rSys);
            }
        }

        /// <summary>
        /// Given the supplied Contact_Id and TIC_Project_Id, calculate what the TIC_Contact_Type for the
        /// related TIC_Contact_Village_Project record should be.  By default, the "lowest power type" of "Prospect" is returned.
        /// </summary>
        /// <param name="vntContactId"></param>
        /// <param name="vntProjectId"></param>
        /// <returns></returns>
        protected virtual string CalculateContactVillageProjectInterestType(object vntContactId, object vntProjectId)
        {
            try
            {
                // Assume we'll return "Prospect" by default.
                string strReturn = modSale.CONTACT_VILLAGE_TYPE_PROSPECT;

                // Only continue if vntContactId and vntProject are BOTH NOT NULL
                if ((!(Convert.IsDBNull(vntContactId))) && (!(Convert.IsDBNull(vntProjectId))))
                {
                    DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    objLib.PermissionIgnored = true;

                    // Get all non-deleted Sales where the Contact is either Buyer 1 or 2 AND the Project = vntProjectId
                    Recordset rstSale = objLib.GetRecordset(modSale.strqTIC_NON_DELETED_SALES_WITH_PROJECT_BUYER_1_OR_2, 3, vntProjectId, vntContactId, vntContactId,
                                                            modSale.strfTIC_SALE_ID, modSale.strfTIC_LOT_SALE_STATUS_ID);

                    // If no non-deleted Sales exist, then "Prospect" will be returned and this code won't run as the recordset is empty
                    if (rstSale.RecordCount > 0)
                    {

                        string strCurrentSaleStatusDescription = "";
                        // If the Contact has >= 1 "Closed" Sales, we can automatically set the Contact Type = "Homeowner",
                        // so use this flag to escape from the while loop below
                        bool blnContactIsHomeownerInProject = false;

                        // Process each Sale record
                        while (!rstSale.EOF)
                        {
                            // Get the Sale's Status Description
                            strCurrentSaleStatusDescription = GetLotSaleStatusDescription(rstSale.Fields[modSale.strfTIC_LOT_SALE_STATUS_ID].Value);

                            switch (strCurrentSaleStatusDescription)
                            {
                                case modSale.LOT_SALE_STATUS_DESCRIPTION_SOLD:
                                case modSale.LOT_SALE_STATUS_DESCRIPTION_RESERVED:
                                    // If the Sale is Sold or Reserved, and the Contact Village Project Type has not 
                                    // ALREADY been determined to be "Homeowner" (which is a "higher power" Type), then
                                    // set the Contact Type = "Buyer" and process the next Sale record
                                    if (strReturn != modSale.CONTACT_VILLAGE_TYPE_HOMEOWNER)
                                    {
                                        strReturn = modSale.CONTACT_VILLAGE_TYPE_BUYER;
                                    }
                                    break;

                                case modSale.LOT_SALE_STATUS_DESCRIPTION_CANCELLED:
                                case modSale.LOT_SALE_STATUS_DESCRIPTION_CANCELLED_RESERVE:
                                    // If the Sale is Cancelled or Cancelled Reserve, and the Contact Village Project Type has not 
                                    // ALREADY been determined to be "Buyer" or "Homeowner" (which are a "higher power" Types), then
                                    // set the Contact Type = "Prospect" and process the next Sale record
                                    if ((strReturn != modSale.CONTACT_VILLAGE_TYPE_BUYER) && (strReturn != modSale.CONTACT_VILLAGE_TYPE_HOMEOWNER))
                                    {
                                        strReturn = modSale.CONTACT_VILLAGE_TYPE_PROSPECT;
                                    }
                                    break;

                                case modSale.LOT_SALE_STATUS_DESCRIPTION_CLOSED:
                                    // If the Sale is Closed, then we don't need to process any further Sale records.
                                    // If the Contact has at least one Closed Sale, then the Contact's Contact Village Project
                                    // Type automatically becomes "Homeowner".
                                    strReturn = modSale.CONTACT_VILLAGE_TYPE_HOMEOWNER;
                                    // Set this flag to true, so that we will exit the while loop (more efficient)
                                    blnContactIsHomeownerInProject = true;
                                    break;

                                default:
                                    break;
                            }

                            // If the Contact is part of a Closed Sale, then exit, as we know the Contact Type = "Homeowner"
                            if (blnContactIsHomeownerInProject == true)
                            {
                                break;
                            }

                            // Process the next Sale record
                            rstSale.MoveNext();
                        }
                    }
                }

                // Return "Prospect" or "Buyer" or "Homeowner"
                return strReturn;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, rSys);
            }
        }

        /// <summary>
        /// Given the supplied TIC_Contact_Village_Id, get all related TIC_Contact_Village_Project records and return
        /// a string which is a calculation of what the TIC_Contact_Village.TIC_Contact_Type value should be, based on
        /// the TIC_Contact_Type values of its TIC_Contact_Village_Project children.
        /// </summary>
        /// <param name="vntContactVillageId"></param>
        /// <returns></returns>
        protected virtual string CalculateContactVillageInterestType(object vntContactVillageId)
        {
            try
            {
                // Assume we'll return "Prospect" by default.
                string strReturn = modSale.CONTACT_VILLAGE_TYPE_PROSPECT;

                // Only continue if vntContactId and vntVillageId are BOTH NOT NULL
                if (!(Convert.IsDBNull(vntContactVillageId)))
                {
                    DataAccess objLib = (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    objLib.PermissionIgnored = true;

                    Recordset rstContactVillageProject = objLib.GetLinkedRecordset(modSale.strtTIC_CONTACT_VILLAGE_PROJECT, modSale.strfTIC_CONTACT_VILLAGE_ID, vntContactVillageId,
                                                                                   modSale.strfTIC_CONTACT_VILLAGE_PROJECT_ID, modSale.strfTIC_CONTACT_ID, modSale.strfTIC_PROJECT_ID, modSale.strfTIC_CONTACT_TYPE);

                    // If no Project Interst records for this Village exist, then "Prospect" will be returned and this code won't run as the recordset is empty
                    if (rstContactVillageProject.RecordCount > 0)
                    {
                        string strCurrentProjectInterestContactType = "";
                        // If the Contact is a "Homeowner" in a Project associated with the supplied Village, then there is no
                        // need for further processing, as we should return "Homeowner", so use this flag to escape from the while loop below
                        bool blnContactIsHomeownerInProject = false;

                        while (!rstContactVillageProject.EOF)
                        {
                            // Get the current TIC_Contact_Village_Project.TIC_Contact_Type value into a string
                            strCurrentProjectInterestContactType = Convert.ToString(rstContactVillageProject.Fields[modSale.strfTIC_CONTACT_TYPE].Value);

                            switch (strCurrentProjectInterestContactType)
                            {
                                case modSale.CONTACT_VILLAGE_TYPE_PROSPECT:
                                    // If the Current TIC_Contact_Village_Project.TIC_Contact_Type = "Prospect"
                                    // and we have not ALREADY processed a TIC_Contact_Village_Project record whose
                                    // TIC_Contact_Type was "Buyer" or "Homeowner", then
                                    // set strReturn = "Prospect" and continue processing TIC_Contact_Village_Project records.
                                    if ((strReturn != modSale.CONTACT_VILLAGE_TYPE_BUYER) && (strReturn != modSale.CONTACT_VILLAGE_TYPE_HOMEOWNER))
                                    {
                                        strReturn = modSale.CONTACT_VILLAGE_TYPE_PROSPECT;
                                    }
                                    break;

                                case modSale.CONTACT_VILLAGE_TYPE_BUYER:
                                    // If the Current TIC_Contact_Village_Project.TIC_Contact_Type = "Buyer"
                                    // and we have not ALREADY processed a TIC_Contact_Village_Project record whose
                                    // TIC_Contact_Type was "Homeowner", then set strReturn = "Buyer" 
                                    // and continue processing TIC_Contact_Village_Project records.
                                    if (strReturn != modSale.CONTACT_VILLAGE_TYPE_HOMEOWNER)
                                    {
                                        strReturn = modSale.CONTACT_VILLAGE_TYPE_BUYER;
                                    }
                                    break;

                                case modSale.CONTACT_VILLAGE_TYPE_HOMEOWNER:
                                    // If the Current TIC_Contact_Village_Project.TIC_Contact_Type = "Homeowner", 
                                    // then we can return "Homeowner", as there is no Type "more powerful" than this.
                                    strReturn = modSale.CONTACT_VILLAGE_TYPE_HOMEOWNER;
                                    // Set this flag to true, so that we will exit the while loop (more efficient)
                                    blnContactIsHomeownerInProject = true;
                                    break;

                                default:
                                    break;
                            }

                            // If the Contact is "Homeowner" in a Project, then quit processing further records, 
                            // as "Homeowner" should be returned.
                            if (blnContactIsHomeownerInProject == true)
                            {
                                break;
                            }

                            // Process the next TIC_Contact_Village_Project record
                            rstContactVillageProject.MoveNext();
                        }
                    }
                }

                // Return "Prospect" or "Buyer" or "Homeowner"
                return strReturn;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, rSys);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strLotSaleStatusId"></param>
        /// <returns></returns>
        protected virtual string GetLotSaleStatusDescription(object strLotSaleStatusId)
        {
            try
            {
                return TypeConvert.ToString(rSys.Tables[modSale.strtTIC_LOT_SALE_STATUS].Fields[modSale.strfTIC_STATUS_DESCRIPTION].Index(strLotSaleStatusId));
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, rSys);
            }
        }

        #endregion

        #endregion
    }
}
