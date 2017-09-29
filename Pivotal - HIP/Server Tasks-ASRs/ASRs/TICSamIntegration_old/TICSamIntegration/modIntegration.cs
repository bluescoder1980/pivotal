using System;
using System.Collections.Generic;
using System.Text;

namespace Pivotal.Application.TIC.SAMIntegration
{
    internal class modIntegration
    {
        public const string TIC_Lot_Id = "TIC_Lot_Id";
        public const string TIC_Change_Number_Ordinal = "TIC_Change_Number_Ordinal";
        public const string TIC_Lot_Sale_Status_Id = "TIC_Lot_Sale_Status_Id";
        public const string TIC_Comments = "TIC_Comments";
        public const string TIC_Date_Business_Transaction = "TIC_Date_Business_Transaction";
        public const string TIC_Changed_By_Id = "TIC_Changed_By_Id";
        public const string TIC_Rollback_Date = "TIC_Rollback_Date";
        public const string TIC_Rollback_Indic = "TIC_Rollback_Indic";
        public const string TIC_Sale_Id = "TIC_Sale_Id";

        //Disconnected fields for Sale record
        public const string discTIC_Neighborhood_Id         = "Disconnected_1_2_1";
        public const string discTIC_Tract_Id                = "Disconnected_1_2_2";
        public const string discECOE_Date                   = "Disconnected_1_2_3";
        public const string discReservation_Date            = "Disconnected_1_2_4";
        public const string discTIC_Reservation_Can_Date    = "Disconnected_1_2_5";
        public const string discActual_Revenue_Date         = "Disconnected_1_2_6";
        public const string discCancel_Date                 = "Disconnected_1_2_7";
        public const string discAdditional_Price            = "Disconnected_1_2_8";
        public const string discCancel_Reason               = "Disconnected_1_2_9";
        public const string discElevation_Premium           = "Disconnected_1_2_10";
        public const string discLot_Premium                 = "Disconnected_1_2_11";
        public const string discPrice                       = "Disconnected_1_2_12";
        public const string discTIC_Design_Options_Total    = "Disconnected_1_2_13";
        public const string discTIC_Preplot_Options         = "Disconnected_1_2_14";
        public const string discBroker_Used_In_Sale         = "Disconnected_1_2_15";
        public const string discContingency_Sale            = "Disconnected_1_2_16";
        public const string discSAM_Buyer_Id                = "Disconnected_1_2_17";
        public const string discSAM_CoBuyer_Id              = "Disconnected_1_2_18";
        public const string discHIPOpportunityId            = "Disconnected_1_2_19";
        public const string discChanged_By_Id               = "Disconnected_1_2_20";
        public const string discLot_Status_Changed_To       = "Disconnected_1_2_21";

        public const string strqTIC_SALES_STATUS_LOOKUP     = "TIC : Int Sale Status Lookup";
        public const string strqTIC_SALES_LOOKUP            = "TIC : Sale Lookup By HIP Ext Src Id";

        public const string strtTIC_Sale                    = "TIC_Sale";



        public const string TIC_Project_Id = "TIC_Project_Id";
        public const string TIC_Tract_Id = "TIC_Tract_Id";
        public const string TIC_Buyer_1_Contact_Id = "TIC_Buyer_1_Contact_Id";
        public const string TIC_Buyer_2_Contact_Id = "TIC_Buyer_2_Contact_Id";       
        public const string TIC_Estimated_Closing_Date = "TIC_Estimated_Closing_Date";
        public const string TIC_Sale_Status_Last_Change_Dt = "TIC_Sale_Status_Last_Change_Dt";
        public const string TIC_Broker_Used_In_Sale_Indic = "TIC_Broker_Used_In_Sale_Indic";
        public const string TIC_Contingency_Sale = "TIC_Contingency_Sale";
        public const string TIC_Cancellation_Reason = "TIC_Cancellation_Reason";
        public const string TIC_Date_Reserved = "TIC_Date_Reserved";
        public const string TIC_Date_Reservation_Cancelled = "TIC_Date_Reservation_Cancelled";
        public const string TIC_Date_Sold = "TIC_Date_Sold";
        public const string TIC_Date_Sale_Cancelled = "TIC_Date_Sale_Cancelled";
        public const string TIC_Date_Closed = "TIC_Date_Closed";
        public const string TIC_Base_Price = "TIC_Base_Price";
        public const string TIC_Incentive_Price = "TIC_Incentive_Price";
        public const string TIC_Selling_Elevation_Premium = "TIC_Selling_Elevation_Premium";
        public const string TIC_Selling_Location_Premium = "TIC_Selling_Location_Premium";
        public const string TIC_Premium_Price = "TIC_Premium_Price";
        public const string TIC_Selling_Upgrade_Preplot = "TIC_Selling_Upgrade_Preplot";
        public const string TIC_Selling_Homebuyer_Extr_Opt = "TIC_Selling_Homebuyer_Extr_Opt";
        public const string TIC_Selling_Models_Upgrade_Rec = "TIC_Selling_Models_Upgrade_Rec";
        public const string TIC_Pre_Plots_Price = "TIC_Pre_Plots_Price";
        public const string TIC_Closing_Base_Price = "TIC_Closing_Base_Price";
        public const string TIC_Closing_Incentive_Price = "TIC_Closing_Incentive_Price";
        public const string TIC_Closing_Elevation_Premium = "TIC_Closing_Elevation_Premium";
        public const string TIC_Closing_Location_Premium = "TIC_Closing_Location_Premium";
        public const string TIC_Closing_Premium = "TIC_Closing_Premium";
        public const string TIC_Closing_Upgrade_Preplot = "TIC_Closing_Upgrade_Preplot";
        public const string TIC_Closing_Homebuyer_Extr_Opt = "TIC_Closing_Homebuyer_Extr_Opt";
        public const string TIC_Closing_Models_Upgrade_Rec = "TIC_Closing_Models_Upgrade_Rec";
        public const string TIC_Closing_Pre_Plots_Price = "TIC_Closing_Pre_Plots_Price";
        public const string TIC_HIP_External_Source_Id = "TIC_HIP_External_Source_Id";

    }

    internal class LotChangeStatus
    { 
        public const string NotReleased = "Not Released";
        public const string Released = "Released";
        public const string Reserved = "Reserved";
        public const string Sold = "Sold";
        public const string Closed = "Closed";
        public const string RollbackSale = "Rollback Sale";
        public const string RollbackReserve = "Rollback Reserve";
        public const string CancelledReserve = "Cancelled Reserve";
        public const string Cancelled = "Cancelled";
        public const string TransferSale = "Transfer Sale";
        public const string TransferReserve = "Transfer Reserve";
    
    }
}
