//
// $Workfile: SyncProxy.cs$
// $Revision: 39$
// $Author: RYong$
// $Date: Tuesday, July 10, 2007 2:27:02 PM$
//
// Copyright © Pivotal Corporation
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// This is a proxy class for accessing the syncronization methods
    /// </summary>
    internal class SyncProxy
    {
        // Syncronization ASR Name
        private const string SyncASR = "PAHB Envision Sync";

        // Pivotal System reference
        private IRSystem7 m_rdaSystem;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rdaSystem">Pivotal System reference</param>
        internal SyncProxy(IRSystem7 rdaSystem)
        {
            this.m_rdaSystem = rdaSystem;
        }
        
        /// <summary>
        /// Updates all the Sync records in a specified ftp file from pending to success.
        /// </summary>
        /// <param name="ftpFilename">File Name string</param>
        internal void SetCurrentFTPSuccessState(string ftpFilename)
        {
            // create the ASR parameters
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, ftpFilename);
            object parameterList = transitParams.ParameterList;

            // call the method
            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_CURRENT_FTP_SUCCESS", ref parameterList);            
        }



        /// <summary>
        /// Updates the sync state of a plan's assigned location.  The Envision location information is spread between Pivotal's
        /// Location and Division_Product_Locations tables.  A sync record for location is uniquely identified by release, plan assignment,
        /// and location.  This sync type is a little different that it tracks the rn_update values for both the Location and 
        /// Division_Product_Locations.
        /// </summary>
        /// <param name="locationId">The location Id.</param>
        /// <param name="planId">The plan definition (division product) record Id of the current plan assignment.</param>
        /// <param name="currentContextReleaseId">References the current release Id.</param>
        /// <param name="rnUpdateLocation">Rn_Update value of the location record.</param>
        /// <param name="rnUpdateDPLocation">Rn_Update value of the Division_Product_Location record.</param>
        /// <param name="ftpFilename">Ftp file name if the transport is Ftp.</param>
        internal void SetLocationState(object locationId, object planId, object currentContextReleaseId, object rnUpdateLocation, object rnUpdateDPLocation, string ftpFilename)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, locationId);
            transitParams.SetUserDefinedParameter(2, planId);
            transitParams.SetUserDefinedParameter(3, currentContextReleaseId);
            transitParams.SetUserDefinedParameter(4, rnUpdateLocation);
            transitParams.SetUserDefinedParameter(5, rnUpdateDPLocation);
            transitParams.SetUserDefinedParameter(6, ftpFilename);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_LOCATION", ref parameterList);
        }




        /// <summary>
        /// Updates the neighborhood's sync record. 
        /// </summary>
        /// <param name="neighborhoodId">Neighborhood Id</param>
        /// <param name="rnUpdateCopy">New Rn_Update value.</param>
        /// <param name="ftpFilename">File name of current daily Ftp update.</param>
        internal void SetNeighborhoodState(object neighborhoodId, byte[] rnUpdateCopy, string ftpFilename)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, neighborhoodId);
            transitParams.SetUserDefinedParameter(2, rnUpdateCopy);
            transitParams.SetUserDefinedParameter(3, ftpFilename);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_NEIGHBORHOOD", ref parameterList);            
        }


        /// <summary>
        /// Updates the sync record for a release.
        /// or not.
        /// </summary>
        /// <param name="releaseId"></param>
        /// <param name="rnUpdateCopy"></param>
        /// <param name="ftpFilename"></param>
        internal void SetReleaseState(object releaseId, byte[] rnUpdateCopy, string ftpFilename)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, releaseId);
            transitParams.SetUserDefinedParameter(2, rnUpdateCopy);
            transitParams.SetUserDefinedParameter(3, ftpFilename);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_RELEASE", ref parameterList);
        }


        /// <summary>
        /// Update the sync record of a plan assignment.  A plan assignment sync record must be specific to a release.  
        /// If a plan assignment is wildcarded to multiple releases, each release must have a separate plan assignment sync record.
        /// </summary>
        /// <param name="planAssignmentId">Plan assignment Id (NBHDP_Product)</param>
        /// <param name="currentContextReleaseId">Release Id to which this plan instance is assigned to.</param>
        /// <param name="rnUpdateCopy">New Rn_Update value.</param>
        /// <param name="ftpFilename">File name of current daily Ftp update.</param>
        internal void SetPlanAssignmentState(object planAssignmentId, object currentContextReleaseId, byte[] rnUpdateCopy, string ftpFilename)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, planAssignmentId);
            transitParams.SetUserDefinedParameter(2, currentContextReleaseId);
            transitParams.SetUserDefinedParameter(3, rnUpdateCopy);
            transitParams.SetUserDefinedParameter(4, ftpFilename);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_PLAN_ASSIGNMENT", ref parameterList);
        }



        /// <summary>
        /// Updates the hard rule's sync record. 
        /// </summary>
        /// <param name="hardRules">Array of hardrules.</param>
        /// <param name="planId">The plan Id (Division_Product) of the plan assignment being processed.</param>
        /// <param name="currentContextReleaseId">The current release being processed.</param>
        /// <param name="ftpFilename">Current Ftp update filename.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "0#")]
        internal void SetHardRuleState(object[,] hardRules, object planId, object currentContextReleaseId, string ftpFilename)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, hardRules);
            transitParams.SetUserDefinedParameter(2, planId);
            transitParams.SetUserDefinedParameter(3, currentContextReleaseId);
            transitParams.SetUserDefinedParameter(4, ftpFilename);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_HARDRULE", ref parameterList);
        }



        /// <summary>
        /// Set the sync status on an option and its secondary records.
        /// Rn_Update values are passed in as a snapshot when records are sent to Envision.  Do not query from
        /// the database since they may have been changed since.
        /// </summary>
        /// <param name="optionId"> The option Id (Division_Product) of the product.</param>
        /// <param name="rnUpdateCopy">Rn_Update value of the option assignment record.</param>
        /// <param name="packageComponents">Array of [Product_Package_Component_Id] and [Rn_Update] pairs.</param>
        /// <param name="optionRules">Array of [Option Rule Id] and [Rn_Update] pairs.</param>
        /// <param name="ftpFilename">Ftp file name.</param>
        internal void SetOptionState(object optionId, byte[] rnUpdateCopy, object[,] packageComponents, object[,] optionRules, string ftpFilename)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, optionId);
            transitParams.SetUserDefinedParameter(2, rnUpdateCopy);
            transitParams.SetUserDefinedParameter(3, packageComponents);
            transitParams.SetUserDefinedParameter(4, optionRules);
            transitParams.SetUserDefinedParameter(5, ftpFilename);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_OPTION", ref parameterList);
        }

        /// <summary>
        /// This deletes orphan synchronization records.
        /// </summary>
        internal void CleanUpEnvSyncTable()
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "DELETE_ORPHAN_SYNC_RECORDS", ref parameterList);
        }





        /// <summary>
        /// Update the sync record of a plan assignment.  A plan assignment sync record must be specific to a release.  
        /// If a plan assignment is wildcarded to multiple releases, each release must have a separate plan assignment sync record.
        /// </summary>
        /// <param name="optionId">Product Id (Division Product)</param>
        /// <param name="currentContextPlanId"> Plan Id (Division Product)</param>
        /// <param name="currentContextReleaseId">Release Id to which this plan instance is assigned to.</param>
        /// <param name="rnUpdateCopy">New Rn_Update value.</param>
        /// <param name="ftpFilename">File name of current daily Ftp update.</param>
        /// <param name="softDeactivate"></param>        
        internal void SetProductAssignmentState(object optionId, object currentContextPlanId, object currentContextReleaseId, byte[] rnUpdateCopy, string ftpFilename, Boolean softDeactivate)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, optionId);
            transitParams.SetUserDefinedParameter(2, currentContextPlanId);
            transitParams.SetUserDefinedParameter(3, currentContextReleaseId);
            transitParams.SetUserDefinedParameter(4, rnUpdateCopy);
            transitParams.SetUserDefinedParameter(5, ftpFilename);
            transitParams.SetUserDefinedParameter(6, softDeactivate);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_PRODUCT_ASSIGNMENT", ref parameterList);
        }



        /// <summary>
        /// Updates the sync record of the location assignment records.  This function is used by Ftp mode one, since
        /// the Ftp schema requires tracking the location assignments seperately from the option assignments.
        /// </summary>
        /// <param name="optionId">The product Id (Division_Product) of the option assignment.</param>
        /// <param name="locationId">Location Id</param>
        /// <param name="planId">The plan Id (Division_Product) of the plan assignment being processed.</param>
        /// <param name="currentContextReleaseId">The current release being processed.</param>
        /// <param name="rnUpdateCopy">Location assignment's Rn_Update value.</param>
        /// <param name="ftpFilename">Current Ftp update filename.</param>
        /// <param name="softDeactivate"></param>
        internal void SetLocationProductAssignmentState(object optionId, object locationId, object planId, object currentContextReleaseId, object rnUpdateCopy, bool softDeactivate, string ftpFilename)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, optionId);
            transitParams.SetUserDefinedParameter(2, locationId);
            transitParams.SetUserDefinedParameter(3, planId);
            transitParams.SetUserDefinedParameter(4, currentContextReleaseId);
            transitParams.SetUserDefinedParameter(5, rnUpdateCopy);
            transitParams.SetUserDefinedParameter(6, softDeactivate);
            transitParams.SetUserDefinedParameter(7, ftpFilename);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_LOCATION_PRODUCT_ASSIGNMENT", ref parameterList);
        }



        
        /// <summary>
        /// Updates or add the sync record for the specified contract, depending on whether the record already exists
        /// or not.  
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="rnUpdateCopy"></param>
        /// <param name="state"></param>
        internal void SetContractState(object contractId, bool contractInactive, byte[] rnUpdateCopy, Sync.SyncState state)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, contractId);
            transitParams.SetUserDefinedParameter(2, contractInactive);
            transitParams.SetUserDefinedParameter(3, rnUpdateCopy);
            transitParams.SetUserDefinedParameter(4, state);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_CONTRACT", ref parameterList);
        }



        /// <summary>
        /// Updates or add the sync record for the specified contract, depending on whether the record already exists
        /// or not.  
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="state"></param>
        internal void SetContractState(object contractId, Sync.SyncState state)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, contractId);
            transitParams.SetUserDefinedParameter(2, state);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_CONTRACT_STATE", ref parameterList);
        }



        /// <summary>
        /// Updates or add the sync record for the specified home, depending on whether the record already exists
        /// or not.  A home always has an attached contract.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="rnUpdateCopy"></param>
        internal void SetHomeState(object productId, byte[] rnUpdateCopy)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, productId);
            transitParams.SetUserDefinedParameter(2, rnUpdateCopy);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_HOME", ref parameterList);        
        }

        /// <summary>
        /// Updates or add the sync record for the specified buyer, depending on whether the record already exists
        /// or not.  A buyer is a contact with an attached contact.
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="rnUpdateCopy"></param>
        internal void SetContactState(object contactId, byte[] rnUpdateCopy)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, contactId);
            transitParams.SetUserDefinedParameter(2, rnUpdateCopy);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_CONTACT", ref parameterList);
        }


        internal void SetLoanProfileState(object loanProfileId, byte[] rnUpdateCopy)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, loanProfileId);
            transitParams.SetUserDefinedParameter(2, rnUpdateCopy);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_LOAN_PROFILE", ref parameterList);
        }


        internal void SetLoanState(object loanId, byte[] rnUpdateCopy)
        {
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, loanId);
            transitParams.SetUserDefinedParameter(2, rnUpdateCopy);
            object parameterList = transitParams.ParameterList;

            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , SyncASR, "SET_LOAN", ref parameterList);
        }
    }
}
