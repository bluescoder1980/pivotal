//
// $Workfile: EnvisionIntegration.cs$
// $Revision: 88$
// $Author: RYong$
// $Date: Thursday, August 30, 2007 2:44:39 PM$
//
// Copyright © Pivotal Corporation
//

// TODO: Add all language strings
// TODO: Verify the PivotalApplicationException is used instead of ApplicationException, or Exception
// TODO: Verify classes are organized of the following order: Constants, Delegates, Sub Types, Statics, Constructor, Properties, Fields, Methods
// TODO: Solve all Warning Messages
// TODO: Make sure all non-generated files have the header comments
// TODO: Make sure scoping is correct.  Internal is should be used.
// TODO: Make sure logging is implemented appropriatly
// TODO: Remove old comment out code
// TODO: Remove unused elements


using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;

using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Globalization;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// The ASR Class for Envision Integration
    /// </summary>
    public partial class  EnvisionIntegration : IRAppScript
    {

        #region Constants

        /// <summary>
        /// Defined error numbers
        /// </summary>
        internal enum ErrorNumber
        {
            ErrorBase = 50000,
            ErrorWebMethodCall,   
            ErrorFtpXmlNodeCreation,
            ErrorNonCriticalBusinessCall,
            ErrorEnvisionObjectSerialization
        }

        /// <summary>
        /// Location Level constants
        /// </summary>
        internal static class LocationLevel
        {
            internal const string CodeCorporation = "corp";
            internal const string CodeRegion = "reg";
            internal const string CodeDivision = "div";
            internal const string CodeCommunity = "com";
            internal const string CodeRelease = "rel";
            internal const string CodePlan = "pla";
            
        }

        /// <summary>
        /// Opportunity ASR Method Names
        /// </summary>
        internal static class OpportunityAsrMethodName
        {
            internal const string CreatePostSaleQuote = "CreatePostSaleQuote";
            internal const string CreateOpportunityProductOption = "CreateOpportunityProductOption";
            internal const string CalculateTotals = "CalculateTotals";
            internal const string GetReSelectedOptionPriceAndBuiltInfo = "GetReSelectedOptionPriceAndBuiltInfo";
        }

        /// <summary>
        /// External ASR Names
        /// </summary>
        internal static class AppServerRuleName
        {
            //internal const string OpportunityPostSaleQuote = "PAHB Opportunity Post Sale Quote";
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            internal const string Opportunity = "PAHB Opportunity";
            internal const string EnvisionIntegrationTransactional = "PAHB Envision Integration Transactional";
        }

        /// <summary>
        /// Form Names
        /// </summary>
        internal static class FormName
        {
            internal const string HBPostSaleQuote = "HB Post Sale Quote";
            internal const string HBOpportunityOptions = "HB Opportunity Options";
        }
        
        #endregion


        #region Statics
        private static bool sendContractsIsRunning = false;  //flags the current running state of the Send Contracts process
        private static bool sendInventoryIsRunning = false;  //flags the current running state of the Send Inventory process

        /// <summary>
        /// Gets the current time.  Typically used for LCS communication tests
        /// </summary>
        /// <returns>Current Date and Time</returns>
        private static DateTime GetServerTime()
        {
            return DateTime.Now;
        }
        #endregion


        private IRSystem7 m_rdaSystem;
        private DataAccess m_objLib;

        // only use the public implementations
        private ILangDict m_rdaLangDict;
        private Configuration m_config;
        private Logging m_log;
        private SyncProxy m_sync;
        private ValidateXml m_xmlValidation;

        /// <summary>
        /// Returns an instance of the Logging class
        /// </summary>
        /// <remarks>This property is designed to only instantiat if it is used.  The
        /// idea is to increate performance if it is not used</remarks>
        internal Logging Log
        {
            get
            {
                if (this.m_log == null)
                    this.m_log = new Logging(this.m_rdaSystem);

                return this.m_log;
            }
        }


        /// <summary>
        /// Return an instance of the Configuration class
        /// </summary>
        /// <remarks>This property is designed to only instantiate if it is used.  The
        /// idea is to increate performance if it is not used</remarks>
        internal Configuration Config
        {
            get
            {
                if (this.m_config == null)
                    this.m_config = new Configuration(this);

                return this.m_config;
            }
        }

        /// <summary>
        /// Returns an instance of the XmlValidation class
        /// </summary>
        internal ValidateXml XmlValidation
        {
            get
            {
                if (this.m_xmlValidation == null)
                    this.m_xmlValidation = new ValidateXml(this.m_rdaSystem, this.Config, this.Log, this.LangDictionary);

                return this.m_xmlValidation;
            }
        }


        /// <summary>
        /// Return an instance of the Sync Proxy class
        /// </summary>
        /// <remarks>This property is designed to only instantiate if it is used.  The
        /// idea is to increate performance if it is not used</remarks>
        internal SyncProxy Sync
        {
            get
            {
                if (this.m_sync == null)
                    this.m_sync = new SyncProxy(this.m_rdaSystem);

                return this.m_sync;
            }
        }


        /// <summary>
        /// Returns an instance of the Envision Integration Language Dicitonary
        /// </summary>
        internal ILangDict LangDictionary
        {
            get
            {
                if (this.m_rdaLangDict == null)
                    this.m_rdaLangDict = this.m_rdaSystem.GetLDGroup("Envision Integration");

                return this.m_rdaLangDict;
            }
        }

        /// <summary>
        /// Returns an instance of the Pivotal System
        /// </summary>
        internal IRSystem7 PivotalSystem
        {
            get
            {
                return this.m_rdaSystem;
            }
        }


        /// <summary>
        /// Returns a reference to a Pivotal Data Access instance
        /// </summary>
        internal DataAccess PivotalDataAccess
        {
            get
            {
                if (this.m_objLib == null)
                {
                    this.m_objLib = (DataAccess)this.m_rdaSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    this.m_objLib.PermissionIgnored = true;
                }

                return this.m_objLib;
            }
        }


        /// <summary>
        /// Cleans up old Envision log records.
        /// </summary>
        private void CleanupLog()
        {
            DateTime deleteOlderThanMe = DateTime.Now.Subtract(new TimeSpan(Config.EnvisionLogCleanupDays, 0, 0, 0));
            Log.WriteEvent(string.Format("Cleaning Up Envision Log Table - Deleting all records older than '{0} {1}'", deleteOlderThanMe.ToShortDateString(), deleteOlderThanMe.ToShortTimeString()));
            this.PivotalDataAccess.DeleteRecordset("Env: All Log Records older than ?", "Env_Log_Id", deleteOlderThanMe);
        }


        #region IRAppScript Members

        /// <summary>
        /// This subroutine sets the Rich Client System.
        /// </summary>
        public void SetSystem(RSystem pSystem)
        {
            try
            {
                if (pSystem == null)
                    throw new ArgumentNullException("pSystem");

                this.m_rdaSystem = (IRSystem7)pSystem;
                this.syncProxy = new SyncProxy(m_rdaSystem);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, this.m_rdaSystem);
            }
        }


        /// <summary>
        /// Main Execute method of the ASR
        /// </summary>
        /// <param name="MethodName">Method Name</param>
        /// <param name="ParameterList">Input Parameters</param>
        /// <returns>Return Parameters</returns>
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
                    case "GET_SERVER_TIME":
                        tppReturn.SetUserDefinedParameter(1, GetServerTime());
                        break;
                    case "SAVE_BUYER_SELECTIONS":
                        {
                            tppParams.CheckUserDefinedParameterNumber(1, true);
                            string xmlBuyerSelection = (string)tppParams.GetUserDefinedParameter(1);
                            string xmlUpdateOutput = SaveBuyerSelections(xmlBuyerSelection);
                            tppReturn.SetUserDefinedParameter(1, xmlUpdateOutput);
                        }
                        break;
                    case "PROCESS_BUYER_SELECTIONS_QUEUE":
                        {
                            ProcessBuyerSelectionsQueue();
                        }
                        break;
                    case "CLEAR_PENDING_BUYER_SELECTIONS_FROM_QUEUE":
                        {
                            ClearPendingBuyerSelectionsFromQueue();
                        }
                        break;
                    case "APPLY_BUYER_SELECTIONS":
                        {
                            tppParams.CheckUserDefinedParameterNumber(1, true);
                            string xmlBuyerSelection = (string)tppParams.GetUserDefinedParameter(1);
                            string failureReason = ApplyBuyerSelections(xmlBuyerSelection);
                            tppReturn.SetUserDefinedParameter(1, failureReason);
                        }
                        break;

                    
                    case "GENERATE_INVENTORY_INIT_FTP_FILE":
                        {
                            Recordset rstDivisions = PivotalDataAccess.GetRecordset(DivisionData.QueryDivionsForAllIntegrationPending, 0, new string[] { DivisionData.DivisionIdField });

                            object[] arrDivisionIds = new object[rstDivisions.RecordCount];
                            for (int i = 0; i < rstDivisions.RecordCount; i++)
                            {
                                arrDivisionIds[i] = rstDivisions.Fields[DivisionData.DivisionIdField].Value;
                                rstDivisions.MoveNext();
                            }

                            TransportType oldType = Config.EnvisionTransportType;
                            Config.EnvisionTransportType = TransportType.Ftp;
                            SendInventoriesToEnvision(arrDivisionIds, false); //need to pass in array of division Ids
                            Config.EnvisionTransportType = oldType;
                        }
                        break;

                    case "ADD_DIVISIONS_TO_INTEGRATION":
                        {
                            Recordset rstSelectedDivisions = (Recordset)tppParams.GetUserDefinedParameter(1);
                            AddDivisionsToIntegration(rstSelectedDivisions);
                        }
                        break;

                    case "REMOVE_DIVISIONS_FROM_INTEGRATION":
                        {
                            Recordset rstSelectedDivisions = (Recordset)tppParams.GetUserDefinedParameter(1);
                            RemoveDivisionsFromIntegration(rstSelectedDivisions);
                        }
                        break;

                    case "SEND_INVENTORIES_TO_ENVISION":
                        {
                            int numberOfRegions = SendInventoriesToEnvision(null, true);
                            tppReturn.SetUserDefinedParameter(1, numberOfRegions);
                        }
                        break;

                    case "SEND_CONTRACT_CHANGES":
                        SendContractChanges();
                        break;

                    case "SET_PENDING_DIVISIONS_TO_ACTIVATED_WS":
                        {
                            SetPendingDivisionsToActivatedWS();
                        }
                        break;

                    case "SET_PENDING_DIVISIONS_TO_ACTIVATED_FTP":
                        {
                            SetPendingDivisionsToActivatedFTP();
                        }
                        break;

                    case "SET_DIVISION_SETUP_BEING_PROCESSED":
                        {
                            object[] arrDivisionIds = (object[])tppParams.GetUserDefinedParameter(1);
                            SetDivisionSetupBeingProcessed(arrDivisionIds);
                        }
                        break;

                    case "UPDATE_CURRENT_FTP_SUCCESS_STATE":
                        {
                            //Call SyncProxy object to update pending state to success in both sync table and System table.
                            string ftpFilename = Config.CurrentFTPSendFilename;
                            Sync.SetCurrentFTPSuccessState(ftpFilename);
                        }
                        break;

                    case Configuration.UpdateTheLastGeneratedFtpFileName:
                        {
                            string fileName = (string)tppParams.GetUserDefinedParameter(1);
                            Configuration.UpdateLastGeneratedFtpFileName(PivotalSystem, fileName);
                        }
                        break;

                    case Configuration.UpdateTheLastFtpFileDestination:
                        {
                            string fileName = (string)tppParams.GetUserDefinedParameter(1);
                            Configuration.UpdateLastFtpFileDestination(PivotalSystem, fileName);
                        }
                        break;

                    case "GET_GENERATED_FILE_LIST":
                        {
                            int age = 5; //default to 5 days.
                            if (tppParams.GetUserDefinedParameter(1) != null)
                                age = TypeConvert.ToInt32(tppParams.GetUserDefinedParameter(1));
                            object filenameList = GetGeneratedFileList(age); 
                            tppReturn.SetUserDefinedParameter(1, filenameList);
                        }
                        break;

                    //case "RESEND_FILE":
                    //    {
                    //        string filename = TypeConvert.ToString(tppParams.GetUserDefinedParameter(1));
                    //        ResendGeneratedFile(filename);
                    //    }
                    //    break;

                    case "SEND_LATEST_FTP_FILE":
                        {
                            SendLatestFtpFile();
                        }
                        break;

                    //case "SUSPEND_INTEGRATION":
                    //    Recordset rstActiveContracts = null;
                    //    {                            
                    //        object divisionId = tppParams.GetUserDefinedParameter(1);
                    //        bool success = SuspendIntegration(divisionId, out rstActiveContracts);
                    //        tppReturn.SetUserDefinedParameter(1, divisionId);
                    //        tppReturn.SetUserDefinedParameter(2, success);
                    //        tppReturn.SetUserDefinedParameter(3, rstActiveContracts);
                    //    }
                    //    break;

                    case "OPTION_AVAILABLE_TO_CHANGE_ALLOWED":
                        {
                            bool allowed;
                            string warningMessage = string.Empty;
                            object neighborhoodProdId = tppParams.GetUserDefinedParameter(1);
                            string fromOptionAvailableTo = TypeConvert.ToString(tppParams.GetUserDefinedParameter(2));
                            string toOptionAvailableTo = TypeConvert.ToString(tppParams.GetUserDefinedParameter(3));
                            allowed = OptionAvailableToChangeAllowed(neighborhoodProdId, fromOptionAvailableTo, toOptionAvailableTo, out warningMessage);
                            tppReturn.SetUserDefinedParameter(1, allowed);
                            tppReturn.InfoMessage = warningMessage;
                        }
                        break;

                    case "CLEANUP_LOG":
                        {
                            CleanupLog();
                        }
                        break;

                    default:
                        throw new PivotalApplicationException("No such method.");
                }

                ParameterList = tppReturn.ParameterList;
            }
            catch (Exception ex)
            {
                // log the exception with the envision logging
                Log.WriteException(ex);

                // re-throw the exception so that the LCS has the opportunity to handle it
                throw;
            }
        }

        #endregion

    }
}
