//
// $Workfile: OpportunityData.cs$
// $Revision: 14$
// $Author: JHui$
// $Date: Tuesday, May 08, 2007 5:26:40 PM$
//
// Copyright © Pivotal Corporation
//

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    internal static partial class OpportunityData
    {
        internal const string QueryAllApprovedContractsWithContact = "Env: All Approved Contracts w/ Contact ?";
        internal const string QueryAllApprovedContractsWithHomesite = "Env: All Approved Contracts w/ Homesite ?";
        internal const string QueryAllApprovedContractsWithOutSync = "Env: All Approved Contracts w/o sync record";
        internal const string QueryAllApprovedContractsWithSyncPending = "Env: All Approved Contracts w/ sync record pending";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal const string QueryActiveContractsInProgressForLotQueryName = "Env: Active Contracts In Progress for Lot?";
        internal const string QueryActivePSQForContract = "Env: Active PSQ for contract?";

        internal const string QueryAllOutOfSyncOpportunities = "Env: All out of sync Opportunities";
        internal const string QueryAllActiveContractsBeingSyncWithEnvision = "Env: Active Contracts Being Sync with Envision";
        internal const string QueryOpportunitiesWithLinkedNeighborhoodProduct = "Env: Opportunities with Linked NeigborhoodProduct ?";
    }
}