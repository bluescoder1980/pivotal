//
// $Workfile: EnvBuyerSelectionsData.cs$
// $Revision: 2$
// $Author: tlyne$
// $Date: Wednesday, December 19, 2007 11:24:08 AM$
//
// Copyright © Pivotal Corporation
//

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    public static partial class EnvBuyerSelectionsData
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public const string AllBuyerSelectionXMLQuery = "Env: All Buyer Selection XMLs";
        public const string MIAllBuyerSelectionXMLQuery = "Env: MI All Buyer Selection XMLs Not On Hold";
        public const string BuyerSelectionForContractAndTransactionQuery = "Env: Buyer Selections for Contract and Transaction?";
        public const string PendingBuyerSelectionsQuery = "Env: Pending Buyer Selections";
        public const string SuccessBuyerSelectionsQuery = "Env: Success Buyer Selections";
        public const string QueuedConst = "Queued";
        public const string SuccessConst = "Success";
        public const string FailureConst = "Failure";
    }
}
