using System;
using System.Collections.Generic;
using System.Text;

using Pivotal.Interop.RDALib;
using Pivotal.Interop.ADODBLib;
using Pivotal.Application.Foundation.Utility;
using Pivotal.Application.Foundation.Data.Element;


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

            //Get Lot Status from SAM
            Recordset rstLotSalesStatus = objLib.GetRecordset(modIntegration.strqTIC_SALES_STATUS_LOOKUP, 
                1, TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discLot_Status_Changed_To].Value), 
                    modIntegration.TIC_Lot_Sale_Status_Id);

            if (rstLotSalesStatus.RecordCount == 0)
            { 
                //Throw an exception since the status could not be found inSAM
                throw new PivotalApplicationException("Invalid Lot Status History Value");
            }

            //Get SAM Status reference on the incoming Lot Status History record
            object vntLotStatusId = rstLotSalesStatus.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value;


            //Check Lot Status Changed to
            switch (TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discLot_Status_Changed_To].Value))
            {                               
                case LotChangeStatus.NotReleased:
                    //Only thing to do for Not Release records is attach to the lot in sam
                    ProcessLotStatusWithNoContract(rstLotStatusHistory, vntLotStatusId);
                    break;
                case LotChangeStatus.Released:
                    ProcessLotStatusWithNoContract(rstLotStatusHistory, vntLotStatusId);
                    break;
                case LotChangeStatus.Reserved:
                    ProcessReservationIntoSAM(rstLotStatusHistory, objLib, vntLotStatusId);
                    break;
                case LotChangeStatus.Sold:
                    break;
                case LotChangeStatus.Closed:
                    break;
                case LotChangeStatus.CancelledReserve:
                    break;
                case LotChangeStatus.Cancelled:
                    break;
                case LotChangeStatus.TransferSale:
                    break;
                case LotChangeStatus.TransferReserve:
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

        public void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {

            object[] recordsetArray = (object[])Recordsets;
            Recordset rstLotStatusHistory = (Recordset)recordsetArray[0];


            DataAccess objLib =
                (DataAccess)rSys.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName]
                .CreateInstance();

            //Get Lot Status from SAM
            Recordset rstLotSalesStatus = objLib.GetRecordset(modIntegration.strqTIC_SALES_STATUS_LOOKUP,
                1, TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discLot_Status_Changed_To].Value),
                    modIntegration.TIC_Lot_Sale_Status_Id);

            if (rstLotSalesStatus.RecordCount == 0)
            {
                //Throw an exception since the status could not be found inSAM
                throw new PivotalApplicationException("Invalid Lot Status History Value");
            }

            //Get SAM Status reference on the incoming Lot Status History record
            object vntLotStatusId = rstLotSalesStatus.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value;


            //Check Lot Status Changed to
            switch (TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discLot_Status_Changed_To].Value))
            {
                case LotChangeStatus.RollbackReserve:
                    //Only thing to do for Not Release records is attach to the lot in sam
                    
                    break;
                case LotChangeStatus.RollbackSale:
                   
                    break;
                
                default:
                    throw new PivotalApplicationException("Invalid Lot Status History Value");

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
        
        }

        /// <summary>
        /// This method will process a reserved lot status into SAM
        /// </summary>
        /// <param name="rstLotStatusHistory"></param>
        public virtual void ProcessReservationIntoSAM(Recordset rstLotStatusHistory, DataAccess objLib, object vntLotStatusId)
        { 
            //Set the Lot Status on the Lot Status History
            rstLotStatusHistory.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value = vntLotStatusId;

            Recordset rstExistSale = GetExistingSaleRecord(TypeConvert.ToString(rstLotStatusHistory.Fields[modIntegration.discHIPOpportunityId].Value), objLib);
            if (rstExistSale.RecordCount > 0)
            {
                //Update Sale Record
                rstExistSale.Fields[modIntegration.TIC_Lot_Sale_Status_Id].Value = vntLotStatusId;
                objLib.SaveRecordset(modIntegration.strtTIC_Sale, rstExistSale);
                rstExistSale.Close();
            }
            else
            { 
                //Insert new Sale
                Recordset rstSale = objLib.GetNewRecordset(modIntegration.strtTIC_Sale, GetSaleFieldList());
                rstSale.AddNew(Type.Missing, Type.Missing);
                //Using data from the Disconnected fields create new Sale record
                MappContractSaleFields(rstSale, rstLotStatusHistory);
                objLib.SaveRecordset(modIntegration.strtTIC_Sale, rstSale);
                rstSale.Close();

            }                       

        
        }


        #endregion

        #region Utility Methods

        /// <summary>
        /// This method will do a lookup on an existing Sale record in HIP
        /// </summary>
        /// <param name="strHipSaleLookup"></param>
        /// <returns></returns>
        public virtual Recordset GetExistingSaleRecord(string strHipSaleLookup, DataAccess objLib)
        {
                       
            //Check for existing Contract for 
            Recordset rstSale = objLib.GetRecordset(modIntegration.strqTIC_SALES_LOOKUP, 1,
                TypeConvert.ToString(strHipSaleLookup), modIntegration.TIC_Sale_Id);


            return rstSale;


        }

        /// <summary>
        /// This method will mapp all fields that are related to the Close of the
        /// Contract
        /// </summary>
        /// <param name="rstSale"></param>
        public virtual void MappContractCloseFields(Recordset rstSale, Recordset rstIncoming)
        { }

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
            rstSale.Fields[modIntegration.TIC_Date_Reserved].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discReservation_Date].Value);
            rstSale.Fields[modIntegration.TIC_Date_Reservation_Cancelled].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discTIC_Reservation_Can_Date].Value);
            rstSale.Fields[modIntegration.TIC_Date_Sold].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discActual_Revenue_Date].Value);
            rstSale.Fields[modIntegration.TIC_Date_Sale_Cancelled].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discCancel_Date].Value);
            rstSale.Fields[modIntegration.TIC_Date_Closed].Value = TypeConvert.ToDateTime(rstIncoming.Fields[modIntegration.discActual_Revenue_Date].Value);
            rstSale.Fields[modIntegration.TIC_Base_Price].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discAdditional_Price].Value);
            rstSale.Fields[modIntegration.TIC_Selling_Elevation_Premium].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discElevation_Premium].Value);
            rstSale.Fields[modIntegration.TIC_Selling_Location_Premium].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discLot_Premium].Value);
            rstSale.Fields[modIntegration.TIC_Premium_Price].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discPrice].Value);
            rstSale.Fields[modIntegration.TIC_Selling_Homebuyer_Extr_Opt].Value = TypeConvert.ToInt32(rstIncoming.Fields[modIntegration.discTIC_Design_Options_Total].Value);
            rstSale.Fields[modIntegration.TIC_Pre_Plots_Price].Value = TypeConvert.ToDecimal(rstIncoming.Fields[modIntegration.discTIC_Preplot_Options].Value);
            rstSale.Fields[modIntegration.TIC_HIP_External_Source_Id].Value = rstIncoming.Fields[modIntegration.discHIPOpportunityId].Value;

            if(rstIncoming.Fields[modIntegration.discBroker_Used_In_Sale].Value != null)
            {
                rstSale.Fields[modIntegration.TIC_Broker_Used_In_Sale_Indic].Value = true;
            }
            if(rstIncoming.Fields[modIntegration.discContingency_Sale].Value != null)
            {
                rstSale.Fields[modIntegration.TIC_Contingency_Sale].Value = true;
            }
   
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
                modIntegration.TIC_HIP_External_Source_Id
            };
            return arrFields;
        }

        #endregion
    }
}
