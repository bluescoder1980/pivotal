//
// $Workfile: Sync.cs$
// $Revision: 63$
// $Author: RYong$
// $Date: Thursday, August 09, 2007 1:57:44 PM$
//
// Copyright © Pivotal Corporation
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

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
    /// Class for managing the synchronization state and error recovery for records including:
    /// - Inventory objects, e.g. release, plan, option...etc.
    /// - Home and Buyer.
    /// - Contract.
    /// - Location.
    /// - Rules.
    /// </summary>
    public class Sync : IRAppScript
    {
        private IRSystem7 m_rdaSystem = null;
        private Logging m_log = null;
        private DataAccess m_dataLibrary = null;

        /// <summary>
        /// The Sync State numbers as defined in the Sync_State field in the Env_Sync table.
        /// </summary>
        internal enum SyncState
        {
            Pending = 0,
            Success = 1,
        }

        /// <summary>
        /// The SyncType numbers as defined in the Sync_Type field in the Env_Sync table.
        /// </summary>
        internal enum SyncType
        {
            Neighborhood = 0,
            Release = 1,
            Plan = 2,
            PlanAssignment = 3,
            Option = 4,
            ProductAssignment = 5,
            NormalRule = 6,
            Contract = 7,
            Contact = 8,
            Home = 9,
            Location = 10,
            LocationOptionAssignment = 11,
            PackageComponent = 12,
            Loan = 13,
            LoanProfile = 14,
            HardRule = 15
        }

        #region Statics
        /// <summary>
        /// Set the Ftp sync status to Pending/Success.
        /// </summary>
        /// <param name="ftpFilename">Ftp file name.</param>
        /// <param name="sync">Recordset of the sync record.</param>
        private static void SetFtpRelatedFields(string ftpFilename, ref Recordset sync)
        {
            if (!string.IsNullOrEmpty(ftpFilename))
            {
                sync.Fields[EnvSyncData.SyncStateField].Value = (int)SyncState.Pending;
                sync.Fields[EnvSyncData.FtpFilenameField].Value = ftpFilename;
            }
            else
            {
                sync.Fields[EnvSyncData.SyncStateField].Value = (int)SyncState.Success;
                sync.Fields[EnvSyncData.FtpFilenameField].Value = System.DBNull.Value;
            }
        }
        #endregion

        #region IRAppScript Members

        /// <summary>
        /// This function un-pack the parameters for the execute functions.
        /// </summary>
        /// <param name="MethodName">Method name.</param>
        /// <param name="ParameterList">Array of parameters.</param>
        public void Execute(string MethodName, ref object ParameterList)
        {
            try
            {
                TransitionPointParameter tppParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                tppParams.ParameterList = ParameterList;

                TransitionPointParameter tppReturn = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                tppReturn.Construct();

                MethodName = MethodName.ToUpper(CultureInfo.CurrentCulture);

                switch (MethodName)
                {
                    case "DELETE_ORPHAN_SYNC_RECORDS":
                        {
                            DeleteOrphanSyncRecords();
                            break;
                        }
                    case "SET_CURRENT_FTP_SUCCESS":
                        {
                            string ftpFilename = TypeConvert.ToString(tppParams.GetUserDefinedParameter(1));
                            SetCurrentFTPSuccessState(ftpFilename);
                            break;
                        }
                    case "SET_RELEASE":
                        {
                            object releaseId = tppParams.GetUserDefinedParameter(1);
                            byte[] rnUpdateCopy = (byte[])tppParams.GetUserDefinedParameter(2);
                            string ftpFilename = TypeConvert.ToString(tppParams.GetUserDefinedParameter(3));

                            SetReleaseState(releaseId, rnUpdateCopy, ftpFilename);
                            break;
                        }

                    case "SET_NEIGHBORHOOD":
                        {
                            object neighborhoodId = tppParams.GetUserDefinedParameter(1);
                            byte[] rnUpdateCopy = (byte[])tppParams.GetUserDefinedParameter(2);
                            string ftpFilename = TypeConvert.ToString(tppParams.GetUserDefinedParameter(3));

                            SetNeighborhoodState(neighborhoodId, rnUpdateCopy, ftpFilename);
                            break;
                        }

                    case "SET_PLAN_ASSIGNMENT":
                        {
                            object planInstanceId = tppParams.GetUserDefinedParameter(1);
                            object currentContextReleaseId = tppParams.GetUserDefinedParameter(2);
                            byte[] rnUpdateCopy = (byte[])tppParams.GetUserDefinedParameter(3);
                            string ftpFilename = TypeConvert.ToString(tppParams.GetUserDefinedParameter(4));

                            SetPlanAssignmentState(planInstanceId, currentContextReleaseId, rnUpdateCopy, ftpFilename);
                            break;
                        }

                    case "SET_HARDRULE":
                        {
                            object[,] hardRules = (object[,]) tppParams.GetUserDefinedParameter(1);
                            object planId = tppParams.GetUserDefinedParameter(2);
                            object releaseId = tppParams.GetUserDefinedParameter(3);
                            string ftpFilename = TypeConvert.ToString(tppParams.GetUserDefinedParameter(4));

                            SetHardRuleState(hardRules, planId, releaseId, ftpFilename);
                            break;
                        }

                    case "SET_PRODUCT_ASSIGNMENT":
                        {
                            object neighborhoodProductId = tppParams.GetUserDefinedParameter(1);
                            object currentContextPlanId = tppParams.GetUserDefinedParameter(2);
                            object currentContextReleaseId = tppParams.GetUserDefinedParameter(3);
                            byte[] rnUpdateCopy = (byte[])tppParams.GetUserDefinedParameter(4);
                            string ftpFilename = TypeConvert.ToString(tppParams.GetUserDefinedParameter(5));                           
                            Boolean softDeactivate = TypeConvert.ToBoolean(tppParams.GetUserDefinedParameter(6));

                            SetProductAssignmentState(neighborhoodProductId, currentContextPlanId, currentContextReleaseId, rnUpdateCopy, ftpFilename, softDeactivate);
                            break;
                        }

                    case "SET_OPTION":
                        {
                            object divisionProductOptionId = tppParams.GetUserDefinedParameter(1);
                            byte[] rnUpdateCopy = (byte[])tppParams.GetUserDefinedParameter(2);
                            object[,] packageComponent = (object[,])tppParams.GetUserDefinedParameter(3);
                            object[,] optionRules = (object[,])tppParams.GetUserDefinedParameter(4);
                            string ftpFilename = TypeConvert.ToString(tppParams.GetUserDefinedParameter(5));

                            SetOptionState(divisionProductOptionId, rnUpdateCopy, packageComponent, optionRules, ftpFilename);
                            break;
                        }

                    case "SET_HOME":
                        {
                            object productId = tppParams.GetUserDefinedParameter(1);
                            byte[] rnUpdateCopy = (byte[])tppParams.GetUserDefinedParameter(2);
                            SetHomeState(productId, rnUpdateCopy);
                            break;
                        }

                    case "SET_LOAN_PROFILE":
                        {
                            object loanProfileId = tppParams.GetUserDefinedParameter(1);
                            byte[] rnUpdateCopy = (byte[])tppParams.GetUserDefinedParameter(2);
                            SetLoanProfile(loanProfileId, rnUpdateCopy);
                            break;
                        }

                    case "SET_LOAN":
                        {
                            object loanId = tppParams.GetUserDefinedParameter(1);
                            byte[] rnUpdateCopy = (byte[])tppParams.GetUserDefinedParameter(2);
                            SetLoan(loanId, rnUpdateCopy);
                            break;
                        }


                    case "SET_CONTACT":
                        {
                            object contactId = tppParams.GetUserDefinedParameter(1);
                            byte[] rnUpdateCopy = (byte[])tppParams.GetUserDefinedParameter(2);
                            SetContactState(contactId, rnUpdateCopy);
                            break;
                        }

                    case "SET_CONTRACT":
                        {
                            object contractId = tppParams.GetUserDefinedParameter(1);
                            bool contractInactive = (bool)tppParams.GetUserDefinedParameter(2);
                            byte[] rnUpdateCopy = (byte[])tppParams.GetUserDefinedParameter(3);
                            int syncState = TypeConvert.ToInt32(tppParams.GetUserDefinedParameter(4));
                            SetContractState(contractId, rnUpdateCopy, contractInactive, (SyncState)syncState);
                            break;
                        }

                    case "SET_CONTRACT_STATE":
                        {
                            object contractId = tppParams.GetUserDefinedParameter(1);
                            int syncState = TypeConvert.ToInt32(tppParams.GetUserDefinedParameter(2));

                            SetContractState(contractId, (SyncState)syncState);
                            break;
                        }

                    case "SET_LOCATION":
                        {
                            object locationId = tppParams.GetUserDefinedParameter(1);
                            object planId = tppParams.GetUserDefinedParameter(2);
                            object releaseId = tppParams.GetUserDefinedParameter(3);
                            byte[] rnUpdateLocation = (byte[])tppParams.GetUserDefinedParameter(4);
                            byte[] rnUpdateDPLocation = (byte[])tppParams.GetUserDefinedParameter(5);
                            string ftpFilename = TypeConvert.ToString(tppParams.GetUserDefinedParameter(6));

                            SetLocationState(locationId, planId, releaseId, rnUpdateLocation, rnUpdateDPLocation, ftpFilename);
                            break;
                        }

                    case "SET_LOCATION_PRODUCT_ASSIGNMENT":
                        {
                            object optionId = tppParams.GetUserDefinedParameter(1);
                            object locationId = tppParams.GetUserDefinedParameter(2);
                            object planId = tppParams.GetUserDefinedParameter(3);
                            object releaseId = tppParams.GetUserDefinedParameter(4);
                            byte[] rnUpdateCopy = (byte[])tppParams.GetUserDefinedParameter(5);
                            bool softDeactivate = (bool)tppParams.GetUserDefinedParameter(6);
                            string ftpFilename = TypeConvert.ToString(tppParams.GetUserDefinedParameter(7));

                            SetLocationProductAssignmentState(optionId, locationId, planId, releaseId, rnUpdateCopy, softDeactivate, ftpFilename);
                            break;
                        }

                    default:
                        throw new PivotalApplicationException("Unknown method.");
                }

                ParameterList = tppReturn.ParameterList;
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                throw;
            }
        }

        /// <summary>
        /// Entry point for the ASR.
        /// </summary>
        /// <param name="pSystem"></param>
        public void SetSystem(RSystem pSystem)
        {
            try
            {
                if (pSystem == null)
                    throw new ArgumentNullException("pSystem");

                m_rdaSystem = (IRSystem7)pSystem;
                m_dataLibrary = (DataAccess)m_rdaSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                throw;
            }
        }

        #endregion


        /// <summary>
        /// Updates all the Sync records in a specified ftp file from pending to success.
        /// </summary>
        /// <param name="ftpFilename">Filename of current Ftp daily update.</param>
        internal void SetCurrentFTPSuccessState(string ftpFilename)
        {
            //Update Sync records state according to ftp filename.
            Recordset sync = m_dataLibrary.GetRecordset(EnvSyncData.QuerySyncRecordForFtp, 1,
                ftpFilename, EnvSyncData.SyncStateField);
            if (sync.RecordCount > 0)
            {
                sync.MoveFirst();
                while (!sync.EOF)
                {
                    sync.Fields[EnvSyncData.SyncStateField].Value = SyncState.Success;
                    sync.MoveNext();
                }

                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
            }

        }


        /// <summary>
        /// Updates the neighborhood's sync record. 
        /// </summary>
        /// <param name="neighborhoodId">Neighborhood Id</param>
        /// <param name="rnUpdateCopy">New Rn_Update value.</param>
        /// <param name="ftpFilename">File name of current daily Ftp update.</param>
        internal void SetNeighborhoodState(object neighborhoodId, byte[] rnUpdateCopy, string ftpFilename)
        {
            //If sync record exists, update it.  If not, create one.
            Recordset sync = m_dataLibrary.GetRecordset(EnvSyncData.SyncForNeighborhoodQuery, 1, neighborhoodId,
                EnvSyncData.RnUpdateCopyField,
                EnvSyncData.NeighborhoodIdField,
                EnvSyncData.FtpFilenameField,
                EnvSyncData.SyncStateField,
                EnvSyncData.SyncTypeField);

            if (sync.RecordCount > 0)
            {
                sync.MoveFirst();
                sync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                SetFtpRelatedFields(ftpFilename, ref sync);
                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
            }
            else
            {
                Recordset newSync = m_dataLibrary.GetNewRecordset(EnvSyncData.TableName,
                    EnvSyncData.RnUpdateCopyField,
                    EnvSyncData.NeighborhoodIdField,
                    EnvSyncData.FtpFilenameField,
                    EnvSyncData.SyncStateField,
                    EnvSyncData.SyncTypeField);

                newSync.AddNew(Type.Missing, Type.Missing);
                newSync.MoveFirst();
                newSync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                newSync.Fields[EnvSyncData.NeighborhoodIdField].Value = neighborhoodId;
                newSync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.Neighborhood;
                SetFtpRelatedFields(ftpFilename, ref newSync);
                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, newSync);
            }
        }



        /// <summary>
        /// Updates the sync record for a release.
        /// </summary>
        /// <param name="releaseId">Release Id</param>
        /// <param name="ftpFilename">File name of current daily Ftp update.</param>
        /// <param name="rnUpdateCopy">New Rn_Update value.</param>
        internal void SetReleaseState(object releaseId, byte[] rnUpdateCopy, string ftpFilename)
        {
            //Does the sync record already exist?
            Recordset sync = m_dataLibrary.GetRecordset(EnvSyncData.SyncForReleaseQuery, 1, releaseId,
                EnvSyncData.RnUpdateCopyField,
                EnvSyncData.ReleaseIdField,
                EnvSyncData.FtpFilenameField,
                EnvSyncData.SyncStateField,
                EnvSyncData.SyncTypeField);

            if (sync.RecordCount > 0)
            {
                sync.MoveFirst();
                sync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                sync.Fields[EnvSyncData.ReleaseIdField].Value = releaseId;
                sync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.Release;
                SetFtpRelatedFields(ftpFilename, ref sync);
                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
            }
            else
            {
                Recordset newSync = m_dataLibrary.GetNewRecordset(EnvSyncData.TableName,
                    EnvSyncData.RnUpdateCopyField,
                    EnvSyncData.ReleaseIdField,
                    EnvSyncData.FtpFilenameField,
                    EnvSyncData.SyncStateField,
                    EnvSyncData.SyncTypeField);

                // adds a new sync record
                newSync.AddNew(Type.Missing, Type.Missing);
                newSync.MoveFirst();
                newSync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                newSync.Fields[EnvSyncData.ReleaseIdField].Value = releaseId;
                newSync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.Release;
                SetFtpRelatedFields(ftpFilename, ref newSync);
                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, newSync);
            }
        }



        /// <summary>
        /// Update the sync record of a plan assignment.  A plan assignment sync record must be specific to a release.  
        /// If a plan assignment is wildcarded to multiple releases, each release must have a separate plan assignment sync record.
        /// </summary>
        /// <param name="planAssignmentId">Plan assignment Id (NBHDP_Product) </param>
        /// <param name="currentContextReleaseId">Release Id to which this plan instance is assigned to.</param>
        /// <param name="rnUpdateCopy">New Rn_Update value.</param>
        /// <param name="ftpFilename">File name of current daily Ftp update.</param>
        internal void SetPlanAssignmentState(object planAssignmentId, object currentContextReleaseId, byte[] rnUpdateCopy, string ftpFilename)
        {
            // Plan assignment recordset.
            Recordset planAssignRst = m_dataLibrary.GetRecordset(planAssignmentId, NBHDPProductData.TableName,
                NBHDPProductData.NBHDPhaseIdField, NBHDPProductData.DivisionProductIdField);
            planAssignRst.MoveFirst();

            //Does the sync record already exist?
            Recordset sync = m_dataLibrary.GetRecordset(EnvSyncData.QuerySyncRecordForPlanAssignment, 2,
                currentContextReleaseId,
                planAssignRst.Fields[NBHDPProductData.DivisionProductIdField].Value,
                EnvSyncData.RnUpdateCopyField,
                EnvSyncData.FtpFilenameField,
                EnvSyncData.SyncStateField,
                EnvSyncData.SyncTypeField);

            if (sync.RecordCount > 0)
            {
                sync.MoveFirst();
                sync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                sync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.PlanAssignment;
                SetFtpRelatedFields(ftpFilename, ref sync);
                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
            }
            else
            {
                Recordset newSync = m_dataLibrary.GetNewRecordset(EnvSyncData.TableName,
                    EnvSyncData.RnUpdateCopyField,
                    EnvSyncData.FtpFilenameField,
                    EnvSyncData.SyncStateField,
                    EnvSyncData.SyncTypeField,
                    EnvSyncData.DivisionProductPlanIdField,
                    EnvSyncData.ReleaseIdField);

                // adds a new sync record
                newSync.AddNew(Type.Missing, Type.Missing);
                newSync.MoveFirst();
                newSync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.PlanAssignment;
                newSync.Fields[EnvSyncData.DivisionProductPlanIdField].Value = planAssignRst.Fields[NBHDPProductData.DivisionProductIdField].Value;
                newSync.Fields[EnvSyncData.ReleaseIdField].Value = currentContextReleaseId;
                newSync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                SetFtpRelatedFields(ftpFilename, ref newSync);

                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, newSync);
            }
        }




        /// <summary>
        /// Updates the sync record of the hard rule record.  
        /// </summary>
        /// <param name="hardRules">Array of ruleIds, Rn_Update values, and soft deactivates.</param>
        /// <param name="planId">The plan Id (Division_Product) of the plan assignment being processed.</param>
        /// <param name="currentContextReleaseId">The current release being processed.</param>
        /// <param name="ftpFilename">Current Ftp update filename.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "0#")]
        internal void SetHardRuleState(object[,] hardRules, object planId, object currentContextReleaseId, string ftpFilename)
        {
            const int RULE_ID = 0;
            const int RN_UPDATE = 1;
            const int SOFT_DEACTIVATE = 2;

            if (hardRules != null && hardRules.Length > 0)
            {
                for (int i = 0; i < hardRules.GetLength(0); i++)
                {
                    //If sync record exists, update it.  If not, create one.
                    Recordset sync = m_dataLibrary.GetRecordset(EnvSyncData.QuerySyncRecordForHardRule, 3,
                        hardRules[i, RULE_ID],
                        planId,
                        currentContextReleaseId,
                        EnvSyncData.RnUpdateCopyField,
                        EnvSyncData.FtpFilenameField,
                        EnvSyncData.SyncStateField,
                        EnvSyncData.SyncTypeField,
                        EnvSyncData.SoftDeactivateField);

                    if (sync.RecordCount > 0)
                    {
                        sync.MoveFirst();
                        sync.Fields[EnvSyncData.RnUpdateCopyField].Value = hardRules[i, RN_UPDATE];
                        sync.Fields[EnvSyncData.SoftDeactivateField].Value = (bool)hardRules[i, SOFT_DEACTIVATE];
                        SetFtpRelatedFields(ftpFilename, ref sync);
                        m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
                    }
                    else
                    {
                        Recordset newSync = m_dataLibrary.GetNewRecordset(EnvSyncData.TableName,
                            EnvSyncData.RnUpdateCopyField,
                            EnvSyncData.ReleaseIdField,
                            EnvSyncData.DivisionProductPlanIdField,
                            EnvSyncData.ProductOptionRuleIdField,
                            EnvSyncData.LocationIdField,
                            EnvSyncData.FtpFilenameField,
                            EnvSyncData.SyncStateField,
                            EnvSyncData.SyncTypeField,
                            EnvSyncData.SoftDeactivateField);

                        // adds a new sync record
                        newSync.AddNew(Type.Missing, Type.Missing);
                        newSync.MoveFirst();
                        newSync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.HardRule;
                        newSync.Fields[EnvSyncData.ReleaseIdField].Value = currentContextReleaseId;
                        newSync.Fields[EnvSyncData.DivisionProductPlanIdField].Value = planId;
                        newSync.Fields[EnvSyncData.ProductOptionRuleIdField].Value = hardRules[i, RULE_ID];
                        newSync.Fields[EnvSyncData.RnUpdateCopyField].Value = hardRules[i, RN_UPDATE];
                        newSync.Fields[EnvSyncData.SoftDeactivateField].Value = (bool)hardRules[i, SOFT_DEACTIVATE];
                        SetFtpRelatedFields(ftpFilename, ref newSync);
                        m_dataLibrary.SaveRecordset(EnvSyncData.TableName, newSync);
                    }
                }
            }
        }

        /// <summary>
        /// This function deletes orphan synchronization records.
        /// </summary>
        internal void DeleteOrphanSyncRecords()
        {
            // If some package components are deleted, delete the sync records as well.
            m_dataLibrary.PermissionIgnored = true;
            m_dataLibrary.DeleteRecordset(EnvSyncData.QueryOrphanSyncRecordForOptionRules, EnvSyncData.EnvSyncIdField);
            m_dataLibrary.DeleteRecordset(EnvSyncData.QueryOrphanSyncRecordForPackageComponent, EnvSyncData.EnvSyncIdField);
            m_dataLibrary.DeleteRecordset(EnvSyncData.QueryOrphanSyncRecordForProductAssignment, EnvSyncData.EnvSyncIdField);
            m_dataLibrary.PermissionIgnored = false;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "3#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "2#")]
        internal void SetOptionState(object optionId, byte[] rnUpdateCopy, object[,] packageComponents, object[,] optionRules, string ftpFilename)
        {
            const int PackageComponentId = 0;
            const int OptionRuleId = 0;
            const int RnUpdate = 1;

            //Does the sync record already exist?
            Recordset sync = m_dataLibrary.GetRecordset(EnvSyncData.SyncForOptionQuery, 1, optionId,
                EnvSyncData.RnUpdateCopyField,
                EnvSyncData.DivisionProductOptionIdField,
                EnvSyncData.FtpFilenameField,
                EnvSyncData.SyncStateField,
                EnvSyncData.SyncTypeField);

            if (sync.RecordCount > 0)
            {
                sync.MoveFirst();
                sync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                sync.Fields[EnvSyncData.DivisionProductOptionIdField].Value = optionId;
                sync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.Option;
                SetFtpRelatedFields(ftpFilename, ref sync);
                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
            }
            else
            {
                Recordset newSync = m_dataLibrary.GetNewRecordset(EnvSyncData.TableName,
                    EnvSyncData.RnUpdateCopyField,
                    EnvSyncData.DivisionProductOptionIdField,
                    EnvSyncData.FtpFilenameField,
                    EnvSyncData.SyncStateField,
                    EnvSyncData.SyncTypeField);

                // adds a new sync record
                newSync.AddNew(Type.Missing, Type.Missing);
                newSync.MoveFirst();
                newSync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                newSync.Fields[EnvSyncData.DivisionProductOptionIdField].Value = optionId;
                newSync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.Option;
                SetFtpRelatedFields(ftpFilename, ref newSync);

                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, newSync);
            }

            // Process package component array.
            if (packageComponents != null && packageComponents.Length > 0)
            {
                for (int i = 0; i < packageComponents.GetLength(0); i++)
                {
                    sync = m_dataLibrary.GetRecordset(EnvSyncData.QuerySyncRecordForPackageComponent, 1, packageComponents[i, PackageComponentId],
                        EnvSyncData.RnUpdateCopyField, EnvSyncData.FtpFilenameField, EnvSyncData.SyncStateField);
                    if (sync.RecordCount > 0)
                    {
                        // If sync record exists for this package component and Rn_Update_Copy is outdateded, then refresh it.
                        if (!m_rdaSystem.EqualIds(sync.Fields[EnvSyncData.RnUpdateCopyField].Value, packageComponents[i, RnUpdate])
                            || TypeConvert.ToInt16(sync.Fields[EnvSyncData.SyncStateField].Value) != (int)SyncState.Success)
                        {
                            sync.Fields[EnvSyncData.RnUpdateCopyField].Value = packageComponents[i, RnUpdate];
                            SetFtpRelatedFields(ftpFilename, ref sync);
                            m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
                        }
                        else if (!string.IsNullOrEmpty(ftpFilename)) //keep above optimization while taking care of FTP.
                        {
                            sync.Fields[EnvSyncData.RnUpdateCopyField].Value = packageComponents[i, RnUpdate];
                            SetFtpRelatedFields(ftpFilename, ref sync);
                            m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
                        }
                    }
                    else
                    {
                        // If no sync record for this package component, create one.
                        sync = m_dataLibrary.GetNewRecordset(EnvSyncData.TableName, EnvSyncData.ProductPackageComponentIdField
                            , EnvSyncData.RnUpdateCopyField, EnvSyncData.SyncTypeField, EnvSyncData.SyncStateField,
                            EnvSyncData.FtpFilenameField);
                        sync.AddNew(Type.Missing, Type.Missing);
                        sync.MoveFirst();
                        sync.Fields[EnvSyncData.ProductPackageComponentIdField].Value = packageComponents[i, PackageComponentId];
                        sync.Fields[EnvSyncData.RnUpdateCopyField].Value = packageComponents[i, RnUpdate];
                        sync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.PackageComponent;
                        SetFtpRelatedFields(ftpFilename, ref sync);
                        m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
                    }
                }
            }

            // Process option rule array.
            if (optionRules != null && optionRules.Length > 0)
            {
                for (int i = 0; i < optionRules.GetLength(0); i++)
                {
                    sync = m_dataLibrary.GetRecordset(EnvSyncData.QuerySyncRecordForOptionRules, 1, optionRules[i, OptionRuleId],
                        EnvSyncData.RnUpdateCopyField, EnvSyncData.SyncStateField, EnvSyncData.FtpFilenameField);
                    if (sync.RecordCount > 0)
                    {
                        // If sync record exists for this option rule and Rn_Update_Copy is outdateded, then refresh it.
                        if (!m_rdaSystem.EqualIds(sync.Fields[EnvSyncData.RnUpdateCopyField].Value, optionRules[i, RnUpdate])
                            || TypeConvert.ToInt16(sync.Fields[EnvSyncData.SyncStateField].Value) != (int)SyncState.Success)
                        {
                            sync.Fields[EnvSyncData.RnUpdateCopyField].Value = optionRules[i, RnUpdate];
                            SetFtpRelatedFields(ftpFilename, ref sync);
                            m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
                        }
                        else if (!string.IsNullOrEmpty(ftpFilename)) //keep above optimization while taking care of FTP.
                        {
                            sync.Fields[EnvSyncData.RnUpdateCopyField].Value = optionRules[i, RnUpdate];
                            SetFtpRelatedFields(ftpFilename, ref sync);
                            m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
                        }
                    }
                    else
                    {
                        // If no sync record for this package component, create one.
                        sync = m_dataLibrary.GetNewRecordset(EnvSyncData.TableName, EnvSyncData.ProductOptionRuleIdField
                            , EnvSyncData.RnUpdateCopyField, EnvSyncData.SyncTypeField, EnvSyncData.SyncStateField,
                            EnvSyncData.FtpFilenameField);
                        sync.AddNew(Type.Missing, Type.Missing);
                        sync.MoveFirst();
                        sync.Fields[EnvSyncData.ProductOptionRuleIdField].Value = optionRules[i, OptionRuleId];
                        sync.Fields[EnvSyncData.RnUpdateCopyField].Value = optionRules[i, RnUpdate];
                        sync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.NormalRule;
                        SetFtpRelatedFields(ftpFilename, ref sync);
                        m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
                    }
                }
            }

        }



        /// <summary>
        /// Update the sync record of a plan assignment.  A plan assignment sync record must be specific to a release.  
        /// If a plan assignment is wildcarded to multiple releases, each release must have a separate plan assignment sync record.
        /// </summary>
        /// <param name="optionId">Option Id (Division Product)</param>
        /// <param name="currentContextPlanId">Id of plan to which the assigment is made.</param>
        /// <param name="currentContextReleaseId">Id of the release to which the plan belongs.</param>
        /// <param name="rnUpdateCopy">New Rn_Update value.</param>
        /// <param name="ftpFilename">Current Ftp update filename.</param>
        /// <param name="softDeactivate">Sets to true if the product assignment is outdated due to a new plan assignment being assigned.</param>
        internal void SetProductAssignmentState(object optionId, object currentContextPlanId, object currentContextReleaseId, byte[] rnUpdateCopy, string ftpFilename, Boolean softDeactivate)
        {

            //Does the sync record already exist?
            Recordset sync = m_dataLibrary.GetRecordset(EnvSyncData.QuerySyncRecordForProductAssignment, 3,
                currentContextReleaseId,
                currentContextPlanId,
                optionId, //productAssignRst.Fields[NBHDPProductData.DivisionProductIdField].Value,
                EnvSyncData.RnUpdateCopyField,
                EnvSyncData.FtpFilenameField,
                EnvSyncData.SyncStateField,
                EnvSyncData.SyncTypeField,
                EnvSyncData.SoftDeactivateField);

            if (sync.RecordCount > 0)
            {
                sync.MoveFirst();                

                //Set RnUpdateCopy equals RnUpdate of the product assignment record, so the assignment record is viewed as current. 
                sync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;                
                sync.Fields[EnvSyncData.SoftDeactivateField].Value = softDeactivate;
                sync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.ProductAssignment;
                SetFtpRelatedFields(ftpFilename, ref sync);
                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
            }
            else
            {
                Recordset newSync = m_dataLibrary.GetNewRecordset(EnvSyncData.TableName,
                    EnvSyncData.RnUpdateCopyField,
                    EnvSyncData.DivisionProductOptionIdField,
                    EnvSyncData.DivisionProductPlanIdField,
                    EnvSyncData.ReleaseIdField,
                    EnvSyncData.FtpFilenameField,
                    EnvSyncData.SyncStateField,
                    EnvSyncData.SyncTypeField);

                // adds a new sync record
                newSync.AddNew(Type.Missing, Type.Missing);
                newSync.MoveFirst();
                newSync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.ProductAssignment;
                newSync.Fields[EnvSyncData.DivisionProductOptionIdField].Value = optionId; // productAssignRst.Fields[NBHDPProductData.DivisionProductIdField].Value;
                newSync.Fields[EnvSyncData.DivisionProductPlanIdField].Value = currentContextPlanId;
                newSync.Fields[EnvSyncData.ReleaseIdField].Value = currentContextReleaseId;
                newSync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                SetFtpRelatedFields(ftpFilename, ref newSync);
                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, newSync);
            }

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
        /// <param name="softDeactivate">To deactivate this record since it's no longer linked to an option assignment.</param>
        /// <param name="ftpFilename">Current Ftp update filename.</param>
        internal void SetLocationProductAssignmentState(object optionId, object locationId, object planId, object currentContextReleaseId, byte[] rnUpdateCopy, bool softDeactivate, string ftpFilename)
        {
            //If sync record exists, update it.  If not, create one.
            Recordset sync = m_dataLibrary.GetRecordset(EnvSyncData.QuerySyncRecordForLocationProductAssignment, 4,
                currentContextReleaseId,
                planId,
                optionId,
                locationId,
                EnvSyncData.RnUpdateCopyField,
                EnvSyncData.FtpFilenameField,
                EnvSyncData.SyncStateField,
                EnvSyncData.SyncTypeField,
                EnvSyncData.SoftDeactivateField);

            if (sync.RecordCount > 0)
            {
                sync.MoveFirst();
                sync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                sync.Fields[EnvSyncData.SoftDeactivateField].Value = softDeactivate;
                SetFtpRelatedFields(ftpFilename, ref sync);
                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
            }
            else
            {
                Recordset newSync = m_dataLibrary.GetNewRecordset(EnvSyncData.TableName,
                    EnvSyncData.RnUpdateCopyField,
                    EnvSyncData.ReleaseIdField,
                    EnvSyncData.DivisionProductPlanIdField,
                    EnvSyncData.DivisionProductOptionIdField,
                    EnvSyncData.LocationIdField,
                    EnvSyncData.FtpFilenameField,
                    EnvSyncData.SyncStateField,
                    EnvSyncData.SyncTypeField,
                    EnvSyncData.SoftDeactivateField);

                // adds a new sync record
                newSync.AddNew(Type.Missing, Type.Missing);
                newSync.MoveFirst();
                newSync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.LocationOptionAssignment;
                newSync.Fields[EnvSyncData.ReleaseIdField].Value = currentContextReleaseId;
                newSync.Fields[EnvSyncData.DivisionProductPlanIdField].Value = planId;
                newSync.Fields[EnvSyncData.DivisionProductOptionIdField].Value = optionId;
                newSync.Fields[EnvSyncData.LocationIdField].Value = locationId;
                newSync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                newSync.Fields[EnvSyncData.SoftDeactivateField].Value = softDeactivate;
                SetFtpRelatedFields(ftpFilename, ref newSync);
                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, newSync);
            }
        }



        /// <summary>
        /// This function update the Rn_Update value for the Home's sync record.
        /// </summary>
        /// <param name="productId">Homesite Id.</param>
        /// <param name="rnUpdateCopy">New Rn_Update value.</param>
        internal void SetHomeState(object productId, byte[] rnUpdateCopy)
        {
            Recordset sync = m_dataLibrary.GetRecordset(EnvSyncData.SyncForHomeQuery, 1, productId,
                EnvSyncData.RnUpdateCopyField,
                EnvSyncData.ProductIdField,
                EnvSyncData.SyncTypeField);

            if (sync.RecordCount > 0)
            {
                sync.MoveFirst();
                sync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                sync.Fields[EnvSyncData.ProductIdField].Value = productId;
                sync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.Home;

                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
            }
            else
            {
                Recordset newSync = m_dataLibrary.GetNewRecordset(EnvSyncData.TableName,
                    EnvSyncData.RnUpdateCopyField,
                    EnvSyncData.ProductIdField,
                    EnvSyncData.SyncTypeField);

                // adds a new sync record
                newSync.AddNew(Type.Missing, Type.Missing);
                newSync.MoveFirst();
                newSync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                newSync.Fields[EnvSyncData.ProductIdField].Value = productId;
                newSync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.Home;

                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, newSync);
            }
        }

        /// <summary>
        /// This function updates the Rn_Update value for the contact's sync record.
        /// </summary>
        /// <param name="contactId">Contact Id</param>
        /// <param name="rnUpdateCopy">New Rn_Update value.</param>
        internal void SetContactState(object contactId, byte[] rnUpdateCopy)
        {
            SetSyncState(EnvSyncData.SyncForContactQuery
                , new string[] {EnvSyncData.ContactIdField}
                , new byte[][] {(byte[])contactId}
                , SyncType.Contact
                , true
                , rnUpdateCopy
                , false
                , SyncState.Success);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="keyFields"></param>
        /// <param name="keyData"></param>
        /// <param name="syncType"></param>
        /// <param name="updateUpdateCopy"></param>
        /// <param name="rnUpdateCopy"></param>
        /// <param name="updateSyncState"></param>
        /// <param name="syncState"></param>
        private void SetSyncState(string query, string[] keyFields, byte[][] keyData, SyncType syncType, bool updateUpdateCopy, byte[] rnUpdateCopy, bool updateSyncState,  SyncState syncState)
        {
            List<object> parameters = new List<object>(keyData);
            parameters.AddRange(keyFields);
            parameters.Add(EnvSyncData.RnUpdateCopyField);
            parameters.Add(EnvSyncData.SyncTypeField);
            parameters.Add(EnvSyncData.SyncStateField);

            Recordset sync = this.m_dataLibrary.GetRecordset(query, keyFields.Length, parameters.ToArray());

            try
            {
                if (sync.RecordCount == 0)
                {
                    sync.AddNew(Type.Missing, Type.Missing);
                    for (int i = 0; i < keyFields.Length; i++)
                        sync.Fields[keyFields[i]].Value = keyData[i];
                    sync.Fields[EnvSyncData.SyncTypeField].Value = (int)syncType;
                }
                else if (sync.RecordCount == 1)
                    sync.MoveFirst();
                else
                    throw new PivotalApplicationException("Duplicate Sync records exist in the database");

                if (updateUpdateCopy) sync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                if (updateSyncState) sync.Fields[EnvSyncData.SyncStateField].Value = syncState;

                this.m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
            }
            finally
            {
                sync.Close();
            }
        }



        /// <summary>
        /// This function updates the Rn_Update value and sync state for the contract's sync record.
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <param name="rnUpdateCopy">New Run_Update value.</param>
        /// <param name="state">Sync State, either Pending or Success.</param>
        /// <param name="contractInactive">Sets if the Contact has been set to inactive.</param>
        internal void SetContractState(object contractId, byte[] rnUpdateCopy, bool contractInactive, SyncState state)
        {
            Recordset sync = this.m_dataLibrary.GetRecordset(EnvSyncData.SyncForContractQuery, 1, new object[] 
                { contractId
                , EnvSyncData.OpportunityIdField
                , EnvSyncData.OpportunityInactiveField
                , EnvSyncData.RnUpdateCopyField
                , EnvSyncData.SyncTypeField
                , EnvSyncData.SyncStateField});

            try
            {
                if (sync.RecordCount == 0)
                {
                    sync.AddNew(Type.Missing, Type.Missing);
                    sync.Fields[EnvSyncData.OpportunityIdField].Value = contractId;
                    sync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.Contract;
                }
                else if (sync.RecordCount == 1)
                    sync.MoveFirst();
                else
                    throw new PivotalApplicationException("Duplicate Sync records exist in the database");

                sync.Fields[EnvSyncData.OpportunityInactiveField].Value = contractInactive;
                sync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateCopy;
                sync.Fields[EnvSyncData.SyncStateField].Value = (int)state;

                this.m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
            }
            finally
            {
                sync.Close();
            }
        }

        /// <summary>
        /// This function updates the sync state for the contract's sync record.
        /// </summary>
        /// <param name="contractId">Contract record Id</param>
        /// <param name="state">Sync state.</param>
        internal void SetContractState(object contractId, SyncState state)
        {
            SetSyncState(EnvSyncData.SyncForContractQuery
                , new string[] { EnvSyncData.OpportunityIdField }
                , new byte[][] { (byte[])contractId }
                , SyncType.Contract
                , false
                , new byte[] { }
                , true
                , state);
        }

        /// <summary>
        /// This function updates the sync status for a Contract's Loan Profile sync record.
        /// </summary>
        /// <param name="loanProfileId">Loan Profile record Id</param>
        /// <param name="rnUpdateCopy">New sync status</param>
        internal void SetLoanProfile(object loanProfileId, byte[] rnUpdateCopy)
        {
            SetSyncState(EnvSyncData.SyncForLoanProfileQuery
                , new string[] { EnvSyncData.LoanProfileIdField }
                , new byte[][] { (byte[])loanProfileId }
                , SyncType.LoanProfile
                , true
                , rnUpdateCopy
                , true
                , SyncState.Success);
        }

        /// <summary>
        /// This function updates the sync status for a Contract's Loan
        /// </summary>
        /// <param name="loanId">Loan record Id</param>
        /// <param name="rnUpdateCopy">New sync status</param>
        internal void SetLoan(object loanId, byte[] rnUpdateCopy)
        {
            SetSyncState(EnvSyncData.SyncForLoanQuery
                , new string[] { EnvSyncData.LoanIdField }
                , new byte[][] { (byte[])loanId }
                , SyncType.Loan
                , false
                , rnUpdateCopy
                , true
                , SyncState.Success);
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
        internal void SetLocationState(object locationId, object planId, object currentContextReleaseId, byte[] rnUpdateLocation, byte[] rnUpdateDPLocation, string ftpFilename)
        {

            //Does the sync record already exist?
            Recordset sync = m_dataLibrary.GetRecordset(EnvSyncData.QuerySyncRecordForLocationPlanRelease, 3,
                locationId,
                planId,
                currentContextReleaseId,
                EnvSyncData.RnUpdateCopyField,
                EnvSyncData.RnUpdateCopy2Field,
                EnvSyncData.FtpFilenameField,
                EnvSyncData.SyncStateField);

            // If it does, refresh the rn_update values.
            if (sync.RecordCount > 0)
            {
                sync.MoveFirst();
                sync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateLocation;
                sync.Fields[EnvSyncData.RnUpdateCopy2Field].Value = rnUpdateDPLocation;
                SetFtpRelatedFields(ftpFilename, ref sync);
                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, sync);
            }
            // If not, add a new sync record
            else
            {
                Recordset newSync = m_dataLibrary.GetNewRecordset(EnvSyncData.TableName,
                    EnvSyncData.RnUpdateCopyField,
                    EnvSyncData.RnUpdateCopy2Field,
                    EnvSyncData.LocationIdField,
                    EnvSyncData.DivisionProductPlanIdField,
                    EnvSyncData.ReleaseIdField,
                    EnvSyncData.FtpFilenameField,
                    EnvSyncData.SyncStateField,
                    EnvSyncData.SyncTypeField
                    );
            
                newSync.AddNew(Type.Missing, Type.Missing);
                newSync.MoveFirst();
                newSync.Fields[EnvSyncData.SyncTypeField].Value = (int)SyncType.Location;
                newSync.Fields[EnvSyncData.LocationIdField].Value = locationId;
                newSync.Fields[EnvSyncData.DivisionProductPlanIdField].Value = planId;
                newSync.Fields[EnvSyncData.ReleaseIdField].Value = currentContextReleaseId;
                newSync.Fields[EnvSyncData.RnUpdateCopyField].Value = rnUpdateLocation;
                newSync.Fields[EnvSyncData.RnUpdateCopy2Field].Value = rnUpdateDPLocation;
                SetFtpRelatedFields(ftpFilename, ref newSync);

                m_dataLibrary.SaveRecordset(EnvSyncData.TableName, newSync);
            }
        }



        /// <summary>
        /// Property for the Log object.
        /// </summary>
        internal Logging Log
        {
            get
            {
                if (this.m_log == null)
                    this.m_log = new Logging(this.m_rdaSystem);

                return this.m_log;
            }
        }

    }
}
