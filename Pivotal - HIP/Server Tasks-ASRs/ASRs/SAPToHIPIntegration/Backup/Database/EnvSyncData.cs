//
// $Workfile: EnvSyncData.cs$
// $Revision: 34$
// $Author: RYong$
// $Date: Monday, August 27, 2007 5:07:22 PM$
//
// Copyright © Pivotal Corporation
//

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    internal static partial class EnvSyncData
    {
        internal const int SyncTypeNeighborhood = 0;
        internal const int SyncTypeRelease = 1;
        internal const int SyncTypePlanAssignment = 3;
        internal const int SyncTypeProduct = 4;
        internal const int SyncTypeProductAssignment = 5;
        internal const int SyncTypeRules = 6;
        internal const int SyncTypeContract = 7;
        internal const int SyncTypeBuyer = 8;
        internal const int SyncTypeHome = 9;
        internal const int SyncTypeLocation = 10;
        internal const int SyncTypeLocationAssignment = 11;
        internal const int SyncTypePackageComponent = 12;
        internal const int SyncTypeLoan = 13;
        internal const int SyncTypeLoanProfile = 14;

        internal const string QuerySyncRecordForCoBuyer = "Env: Sync for Co-Buyer ?";
        internal const string QuerySyncRecordForNeighborhood = "Env: Sync for neighborhood ?";
        internal const string QuerySyncRecordForRelease = "Env: Sync for release ?";
        internal const string QuerySyncRecordsForAllRelease = "Env: Sync records of all releases";
        internal const string QuerySyncRecordsForAllNeighborhoods = "Env: Sync records for all neighborhoods";
        internal const string QuerySyncRecordForOption = "Env: Sync for option ?";
        internal const string QuerySyncRecordForPlanAssignment = "Env: Sync for Plan Assignment Release? Plan ?";
        internal const string QuerySyncRecordForProductAssignment = "Env: Sync for product assignment Release ? Plan ? Option ?";
        internal const string QuerySyncRecordForPlanAssignmentRelease = "Env: Sync for Plan Assignment Release?";
        internal const string QuerySyncRecordForHardRule = "Env: Sync for Hard Rule ? Plan ? Release ?";




        internal const string QuerySyncRecordForFtp = "Env: Sync for ftp filename ?";
        internal const string QuerySyncRecordForLocationPlanRelease = "Env: Sync for Location? Plan ? Release ?";
        internal const string QuerySyncRecordForLocationProductAssignment = "Env: Sync for location product assignment Release ? Plan ? Option ? Location ?";
        internal const string QuerySyncRecordForPackageComponent = "Env: Sync for Package Component ?";
        internal const string QuerySyncRecordForOptionRules = "Env: Sync for Option Rules ?";
        internal const string QueryOrphanSyncRecordForPackageComponent = "Env: Orphans Package Component Sync RecordsProductPackageComponentData";
        internal const string QueryOrphanSyncRecordForOptionRules = "Env: Orphans Option Rule Sync Records";
        internal const string QueryPreviousOptionAssignmentsToDelete = "Env: Previous Option Assignments To Deactivate";
        internal const string QueryOrphanSyncRecordForProductAssignment = "Env: Orphans Product Assignment Sync Records";
        internal const string QueryPreviousLocationAssignmentsToDeactivateAllLocations = "Env: Previous Location Assignments To Deactivate - All Locations";
        internal const string QueryPreviousLocationAssignmentsToDeactivateSpecificOrWholeHouse = "Env: Previous Location Assignments To Deactivate - Specific or Whole House";
        
 
        //copied from Sync class for now (will need to rename)
        internal const string SyncForProductAssignmentQuery = "Env: Sync for product assignment Release ? Plan ? Option ?";
        internal const string SyncForContactQuery = "Env: Sync for buyer ?";
        internal const string SyncForHomeQuery = "Env: Sync for home ?";
        internal const string SyncForNeighborhoodQuery = "Env: Sync for neighborhood ?";
        internal const string SyncForOptionQuery = "Env: Sync for option ?";
        internal const string SyncForReleaseQuery = "Env: Sync for release ?";
        internal const string SyncForContractQuery = "Env: Sync for contract ?";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal const string SyncForPlanQuery = "Env: Sync for plan ?";
        internal const string SyncForPlanAssignmentQuery = "Env: Sync for Plan Assignment Release? Plan ?";
        internal const string SyncForLoanProfileQuery = "Env: Sync for Loan Profile ?";
        internal const string SyncForLoanQuery = "Env: Sync for Loan ?";
    }
}