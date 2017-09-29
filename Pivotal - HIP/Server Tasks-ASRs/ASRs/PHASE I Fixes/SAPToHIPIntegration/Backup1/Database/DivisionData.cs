//
// $Workfile: DivisionData.cs$
// $Revision: 2$
// $Author: tlyne$
// $Date: Wednesday, December 19, 2007 11:24:08 AM$
//
// Copyright © Pivotal Corporation
//

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    public static partial class DivisionData
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public const string QueryDivisionForId = "Division for Id=?";
        public const string QueryDivisionsToSynchronizeForRegion = "Env: Divisions to Synchronize for Region ?";
        public const string QueryDivionsForAllIntegrationPending = "Env: Divisions with setup being processed";
    }
}