//
// $Workfile: DivisionData.cs$
// $Revision: 5$
// $Author: tlyne$
// $Date: Sunday, June 10, 2007 1:36:23 PM$
//
// Copyright © Pivotal Corporation
//

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    internal static partial class DivisionData
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal const string QueryDivisionForId = "Division for Id=?";
        internal const string QueryDivisionsToSynchronizeForRegion = "Env: Divisions to Synchronize for Region ?";
        internal const string QueryDivionsForAllIntegrationPending = "Env: Divisions with setup being processed";
    }
}