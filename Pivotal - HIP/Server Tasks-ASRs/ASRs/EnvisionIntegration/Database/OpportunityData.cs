//
// $Workfile: OpportunityData.cs$
// $Revision: 2$
// $Author: tlyne$
// $Date: Wednesday, December 19, 2007 11:24:09 AM$
//
// Copyright © Pivotal Corporation
//

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    public static partial class OpportunityData
    {
        public const string QueryAllApprovedContractsWithContact = "Env: All Approved Contracts w/ Contact ?";
        public const string QueryAllApprovedContractsWithHomesite = "Env: All Approved Contracts w/ Homesite ?";
        public const string QueryAllApprovedContractsWithOutSync = "Env: All Approved Contracts w/o sync record";
        public const string QueryAllApprovedContractsWithSyncPending = "Env: All Approved Contracts w/ sync record pending";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public const string QueryActiveContractsInProgressForLotQueryName = "Env: Active Contracts In Progress for Lot?";
        public const string QueryActivePSQForContract = "Env: Active PSQ for contract?";

        public const string QueryAllOutOfSyncOpportunities = "Env: All out of sync Opportunities";
        public const string QueryAllActiveContractsBeingSyncWithEnvision = "Env: Active Contracts Being Sync with Envision";
        public const string QueryOpportunitiesWithLinkedNeighborhoodProduct = "Env: Opportunities with Linked NeigborhoodProduct ?";
    }
}