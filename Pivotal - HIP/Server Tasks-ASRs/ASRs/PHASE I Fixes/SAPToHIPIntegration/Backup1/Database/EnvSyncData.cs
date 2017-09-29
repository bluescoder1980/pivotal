//
// $Workfile: EnvSyncData.cs$
// $Revision: 2$
// $Author: tlyne$
// $Date: Wednesday, December 19, 2007 11:24:08 AM$
//
// Copyright © Pivotal Corporation
//

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    public static partial class EnvSyncData
    {
        public const int SyncTypeNeighborhood = 0;
        public const int SyncTypeRelease = 1;
        public const int SyncTypePlanAssignment = 3;
        public const int SyncTypeProduct = 4;
        public const int SyncTypeProductAssignment = 5;
        public const int SyncTypeRules = 6;
        public const int SyncTypeContract = 7;
        public const int SyncTypeBuyer = 8;
        public const int SyncTypeHome = 9;
        public const int SyncTypeLocation = 10;
        public const int SyncTypeLocationAssignment = 11;
        public const int SyncTypePackageComponent = 12;
        public const int SyncTypeLoan = 13;
        public const int SyncTypeLoanProfile = 14;

        public const string QuerySyncRecordForCoBuyer = "Env: Sync for Co-Buyer ?";
        public const string QuerySyncRecordForNeighborhood = "Env: Sync for neighborhood ?";
        public const string QuerySyncRecordForRelease = "Env: Sync for release ?";
        public const string QuerySyncRecordsForAllRelease = "Env: Sync records of all releases";
        public const string QuerySyncRecordsForAllNeighborhoods = "Env: Sync records for all neighborhoods";
        public const string QuerySyncRecordForOption = "Env: Sync for option ?";
        public const string QuerySyncRecordForPlanAssignment = "Env: Sync for Plan Assignment Release? Plan ?";
        public const string QuerySyncRecordForProductAssignment = "Env: Sync for product assignment Release ? Plan ? Option ?";
        public const string QuerySyncRecordForPlanAssignmentRelease = "Env: Sync for Plan Assignment Release?";
        public const string QuerySyncRecordForHardRule = "Env: Sync for Hard Rule ? Plan ? Release ?";




        public const string QuerySyncRecordForFtp = "Env: Sync for ftp filename ?";
        public const string QuerySyncRecordForLocationPlanRelease = "Env: Sync for Location? Plan ? Release ?";
        public const string QuerySyncRecordForLocationProductAssignment = "Env: Sync for location product assignment Release ? Plan ? Option ? Location ?";
        public const string QuerySyncRecordForPackageComponent = "Env: Sync for Package Component ?";
        public const string QuerySyncRecordForOptionRules = "Env: Sync for Option Rules ?";
        public const string QueryOrphanSyncRecordForPackageComponent = "Env: Orphans Package Component Sync RecordsProductPackageComponentData";
        public const string QueryOrphanSyncRecordForOptionRules = "Env: Orphans Option Rule Sync Records";
        public const string QueryPreviousOptionAssignmentsToDelete = "Env: Previous Option Assignments To Deactivate";
        public const string QueryOrphanSyncRecordForProductAssignment = "Env: Orphans Product Assignment Sync Records";
        public const string QueryPreviousLocationAssignmentsToDeactivateAllLocations = "Env: Previous Location Assignments To Deactivate - All Locations";
        public const string QueryPreviousLocationAssignmentsToDeactivateSpecificOrWholeHouse = "Env: Previous Location Assignments To Deactivate - Specific or Whole House";
        
 
        //copied from Sync class for now (will need to rename)
        public const string SyncForProductAssignmentQuery = "Env: Sync for product assignment Release ? Plan ? Option ?";
        public const string SyncForContactQuery = "Env: Sync for buyer ?";
        public const string SyncForHomeQuery = "Env: Sync for home ?";
        public const string SyncForNeighborhoodQuery = "Env: Sync for neighborhood ?";
        public const string SyncForOptionQuery = "Env: Sync for option ?";
        public const string SyncForReleaseQuery = "Env: Sync for release ?";
        public const string SyncForContractQuery = "Env: Sync for contract ?";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public const string SyncForPlanQuery = "Env: Sync for plan ?";
        public const string SyncForPlanAssignmentQuery = "Env: Sync for Plan Assignment Release? Plan ?";
        public const string SyncForLoanProfileQuery = "Env: Sync for Loan Profile ?";
        public const string SyncForLoanQuery = "Env: Sync for Loan ?";
    }
}