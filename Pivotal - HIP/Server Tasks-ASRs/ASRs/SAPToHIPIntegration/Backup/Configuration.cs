//
// $Workfile: Configuration.cs$
// $Revision: 43$
// $Author: tlyne$
// $Date: Tuesday, July 24, 2007 10:03:29 AM$
//
// Copyright © Pivotal Corporation
//


using System;
using System.Collections.Generic;
using System.Text;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server;



namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// Retrieves the settings for Envision Integration
    /// </summary>
    internal class Configuration
    {
        //public const string UpdateTransportTypeMethodName = "UPDATE_TRANSPORT_TYPE";
        public const string UpdateTheLastGeneratedFtpFileName = "UPDATE_LAST_FTP_FILENAME";
        public const string UpdateTheLastFtpFileDestination = "UPDATE_LAST_FTP_FILE_DEST";

        //private const string TransactionalAsr = "PAHB Envision Integration Transactional";


        #region Private Fields

        private string m_envBuilderNameField = string.Empty;
        private string m_envBuyerWsPathField = string.Empty;

        private string m_envFtpDestPathField = string.Empty;
        private string m_envFtpExePathField = string.Empty;
        private string m_envFtpServerField = string.Empty;
        private string m_envFtpPasswordField = string.Empty;
        private string m_envFtpTempDirectoryField = string.Empty;
        private string m_envFtpUserNameField = string.Empty;
        private int m_envFtpTimeout;

        private string m_envHomeWsPathField = string.Empty;
        private string m_envNHTNumberField = string.Empty;
        private string m_envOptionManagerWsPathField = string.Empty;
        private string m_envWsPasswordField = string.Empty;
        private int m_envWsTimeoutField = 600;
        private string m_envWsUserNameField = string.Empty;
        //private bool m_envXmlLog = true;
        private bool m_envValidateXml = true;
        private bool m_envAutoActivateBuyer = true;
        private EnvisionIntegration.TransportType m_envTransportTypeField = EnvisionIntegration.TransportType.Ftp; 
        private string m_envLastFTPSendFilename = string.Empty;
        private EnvisionIntegration.ProductCreationLevel m_envProductCreationLevel;
        private string m_envLatestFtpFile = string.Empty;
        private int m_envLogCleanupDays = 0;

        private EnvisionIntegration m_envisionIntegration;

        #endregion

        #region Static Methods


        /// <summary>
        /// Update last generated ftp file name in system table.
        /// </summary>
        /// <param name="rdaSystem">RSystem object passed in from ASR.</param>
        /// <param name="fileName">new ftp file name</param>
        internal static void UpdateLastGeneratedFtpFileName(IRSystem7 rdaSystem, string fileName)
        {
            DataAccess dataAccess = (DataAccess)rdaSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            Recordset recordset = dataAccess.GetRecordset(SystemData.TableName, SystemData.EnvLastGeneratedFtpFileField);
            try
            {
                recordset.Fields[SystemData.EnvLastGeneratedFtpFileField].Value = fileName;
                dataAccess.SaveRecordset(SystemData.TableName, recordset);
            }
            finally
            {
                recordset.Close();
            }
        }

        /// <summary>
        /// Update last ftp file destination in system table.
        /// </summary>
        /// <param name="rdaSystem">RSystem object passed in from ASR.</param>
        /// <param name="fileDest">new ftp file destination</param>
        internal static void UpdateLastFtpFileDestination(IRSystem7 rdaSystem, string fileDest)
        {
            DataAccess dataAccess = (DataAccess)rdaSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            Recordset recordset = dataAccess.GetRecordset(SystemData.TableName, SystemData.EnvLastFTPSendFileNameField, SystemData.EnvLastFtpFileSendDateTimeField);
            try
            {
                recordset.Fields[SystemData.EnvLastFTPSendFileNameField].Value = fileDest;
                recordset.Fields[SystemData.EnvLastFtpFileSendDateTimeField].Value = DateTime.Now;
                dataAccess.SaveRecordset(SystemData.TableName, recordset);
            }
            finally
            {
                recordset.Close();
            }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envisionIntegration">Envision Integration ASR instance</param>
        internal Configuration(EnvisionIntegration envisionIntegration)
        {
            this.m_envisionIntegration = envisionIntegration;
            LoadFieldsFromDatabase(this.m_envisionIntegration.PivotalSystem);
        }

        /// <summary>
        /// Gets the New Home Technologies client number
        /// </summary>
        internal string EnvisionNHTNumber
        {
            get
            {
                return this.m_envNHTNumberField;
            }
        }

        /// <summary>
        /// Returns the proper name of the Builder
        /// </summary>
        internal string EnvisionBuilderName
        {
            get
            {
                return this.m_envBuilderNameField;
            }
        }


        /// <summary>
        /// Returns or sets the type or mode of Envision communication.
        /// </summary>
        internal EnvisionIntegration.TransportType EnvisionTransportType
        {
            get
            {
                return this.m_envTransportTypeField;
            }
            set
            {
                this.m_envTransportTypeField = value; 
            }
        }

        /// <summary>
        /// Returns the Url for the Envision Buyer Web Service
        /// </summary>
        internal string EnvisionBuyerWebServiceUrl
        {
            get
            {
                return this.m_envBuyerWsPathField;
            }
        }


        /// <summary>
        /// Returns the Url for the Envision Home Web Service
        /// </summary>
        internal string EnvisionHomeWebServiceUrl
        {
            get
            {
                return this.m_envHomeWsPathField;
            }
        }

        /// <summary>
        /// Returns the Url for the Envision Options Manager Web Service
        /// </summary>
        internal string EnvisionOptionsManagerWebServiceUrl
        {
            get
            {
                return this.m_envOptionManagerWsPathField;
            }
        }

        /// <summary>
        /// Returns the username for accessing Envision Web Services
        /// </summary>
        internal string EnvisionWebServiceUserName
        {
            get
            {
                return this.m_envWsUserNameField;
            }
        }

        /// <summary>
        /// Returns the password for accessing Envision Web Services
        /// </summary>
        internal string EnvisionWebServicePassword
        {
            get
            {
                return this.m_envWsPasswordField;
            }
        }


        /// <summary>
        /// Returns the elapse timeout in seconds when accessing Web Services
        /// </summary>
        internal int EnvisionWebServiceTimeout
        {
            get
            {
                return this.m_envWsTimeoutField;
            }
        }

        /// <summary>
        /// Returns the temporary directory intended for staging files for the Ftp transfer.
        /// </summary>
        internal string FtpTempDirectory
        {
            get
            {
                return this.m_envFtpTempDirectoryField;
            }
        }




        /// <summary>
        /// Returns the to the Ftp program.
        /// </summary>
        internal string FtpExePath
        {
            get
            {
                return this.m_envFtpExePathField;
            }
        }


        /// <summary>
        /// Returns the name of the destination Ftp server.
        /// </summary>
        internal string FtpServer
        {
            get
            {
                return this.m_envFtpServerField;
            }
        }

        /// <summary>
        /// Retuns the directory on the Ftp server where to send the Ftp file(s)
        /// </summary>
        internal string FtpDestPath
        {
            get
            {
                return this.m_envFtpDestPathField;
            }
        }

        /// <summary>
        /// Returns the username needed for accessing the Ftp server.
        /// </summary>
        internal string FtpUserName
        {
            get
            {
                return this.m_envFtpUserNameField;
            }
        }

        /// <summary>
        /// Returns the password needed for accessing the Ftp server.
        /// </summary>
        internal string FtpPassword
        {
            get
            {
                return this.m_envFtpPasswordField;

            }
        }

        /// <summary>
        /// The timeout for Ftp transfers
        /// </summary>
        internal int FtpTimeout
        {
            get
            {
                return this.m_envFtpTimeout;
            }
        }


        /// <summary>
        /// return Envision log cleaning up days.
        /// </summary>
        internal int EnvisionLogCleanupDays
        {
            get
            {
                return this.m_envLogCleanupDays;
            }
        }




        /// <summary>
        /// Returns the name of the current file being sent to Envision via Ftp
        /// </summary>
        internal string CurrentFTPSendFilename
        {
            get
            {
                return m_envLastFTPSendFilename;
            }
        }


        /// <summary>
        /// Returns the set product creation level
        /// </summary>
        internal EnvisionIntegration.ProductCreationLevel ProductCreationLevel
        {
            get
            {
                return m_envProductCreationLevel; 
            }
        }


        /// <summary>
        /// Returns whether to validate Envision Xml against the supplied schemas
        /// </summary>
        internal bool ValidateXml
        {
            get
            {
                return m_envValidateXml;
            }
        }

        /// <summary>
        /// Returns whether to validate Envision Xml against the supplied schemas
        /// </summary>
        internal bool AutoActivateBuyer
        {
            get
            {
                return m_envAutoActivateBuyer;
            }
        }


        /// <summary>
        /// Returns latest FTP file
        /// </summary>
        internal string EnvisionLatestFtpFile
        {
            get
            {
                return m_envLatestFtpFile;
            }
        }


        /// <summary>
        /// Loads system table fields
        /// </summary>
        /// <param name="rdaSystem"></param>
        private void LoadFieldsFromDatabase(IRSystem7 rdaSystem)
        {
            DataAccess dataAccess = (DataAccess)rdaSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            Recordset recordset = dataAccess.GetRecordset(SystemData.TableName
                , SystemData.EnvBuilderNameField
                , SystemData.EnvBuyerWsPathField
                , SystemData.EnvFtpDestPathField
                , SystemData.EnvFtpExePathField
                , SystemData.EnvFtpServerField
                , SystemData.EnvFtpPasswordField
                , SystemData.EnvFtpTempDirectoryField
                , SystemData.EnvFtpUserNameField
                , SystemData.EnvFtpTimeoutField
                , SystemData.EnvHomeWsPathField
                , SystemData.EnvNHTNumberField
                , SystemData.EnvOptionManagerWsPathField
                , SystemData.EnvWsPasswordField
                , SystemData.EnvWsTimeoutField
                , SystemData.EnvWsUserNameField
                , SystemData.EnvXMLLogField
                , SystemData.EnvTransportTypeField
                , SystemData.EnvLastFTPSendFileNameField
                , SystemData.ProductCreationLevelField
                , SystemData.EnvValidateXmlField
                , SystemData.EnvAutoActivateBuyerField
                , SystemData.EnvLastGeneratedFtpFileField
                , SystemData.EnvLogCleanupDaysField
                );

            try
            {
                if (recordset.RecordCount == 1)
                {
                    recordset.MoveFirst();
                    m_envBuilderNameField = TypeConvert.ToString(recordset.Fields[SystemData.EnvBuilderNameField].Value);
                    m_envBuyerWsPathField = TypeConvert.ToString(recordset.Fields[SystemData.EnvBuyerWsPathField].Value);
                    m_envFtpDestPathField = TypeConvert.ToString(recordset.Fields[SystemData.EnvFtpDestPathField].Value);
                    m_envFtpExePathField = TypeConvert.ToString(recordset.Fields[SystemData.EnvFtpExePathField].Value);
                    m_envFtpServerField = TypeConvert.ToString(recordset.Fields[SystemData.EnvFtpServerField].Value);
                    m_envFtpPasswordField = TypeConvert.ToString(recordset.Fields[SystemData.EnvFtpPasswordField].Value);
                    m_envFtpTempDirectoryField = TypeConvert.ToString(recordset.Fields[SystemData.EnvFtpTempDirectoryField].Value);
                    m_envFtpUserNameField = TypeConvert.ToString(recordset.Fields[SystemData.EnvFtpUserNameField].Value);
                    m_envFtpTimeout = TypeConvert.ToInt32(recordset.Fields[SystemData.EnvFtpTimeoutField].Value) * 1000; //convert from seconds to milliseconds
                    m_envHomeWsPathField = TypeConvert.ToString(recordset.Fields[SystemData.EnvHomeWsPathField].Value);
                    m_envNHTNumberField = TypeConvert.ToString(recordset.Fields[SystemData.EnvNHTNumberField].Value);
                    m_envOptionManagerWsPathField = TypeConvert.ToString(recordset.Fields[SystemData.EnvOptionManagerWsPathField].Value);
                    m_envWsPasswordField = TypeConvert.ToString(recordset.Fields[SystemData.EnvWsPasswordField].Value);
                    m_envWsTimeoutField = TypeConvert.ToInt32(recordset.Fields[SystemData.EnvWsTimeoutField].Value) * 1000; //convert from seconds to milliseconds
                    m_envWsUserNameField = TypeConvert.ToString(recordset.Fields[SystemData.EnvWsUserNameField].Value);
                    //m_envXmlLog = TypeConvert.ToBoolean(recordset.Fields[SystemData.EnvXMLLogField].Value);
                    m_envTransportTypeField = (EnvisionIntegration.TransportType)(byte)(recordset.Fields[SystemData.EnvTransportTypeField].Value);
                    m_envLastFTPSendFilename = TypeConvert.ToString(recordset.Fields[SystemData.EnvLastFTPSendFileNameField].Value);
                    m_envProductCreationLevel = (EnvisionIntegration.ProductCreationLevel)(byte)recordset.Fields[SystemData.ProductCreationLevelField].Value;
                    m_envValidateXml = TypeConvert.ToBoolean(recordset.Fields[SystemData.EnvValidateXmlField].Value);
                    m_envAutoActivateBuyer = recordset.Fields[SystemData.EnvAutoActivateBuyerField].Value == DBNull.Value ? true : (bool)recordset.Fields[SystemData.EnvAutoActivateBuyerField].Value;
                    m_envLatestFtpFile = recordset.Fields[SystemData.EnvLastGeneratedFtpFileField].Value == DBNull.Value ? string.Empty : (string)recordset.Fields[SystemData.EnvLastGeneratedFtpFileField].Value;
                    m_envLogCleanupDays = recordset.Fields[SystemData.EnvLogCleanupDaysField].Value == DBNull.Value ? 1 : Convert.ToInt32(recordset.Fields[SystemData.EnvLogCleanupDaysField].Value);
                }
                else
                {
                    throw new PivotalApplicationException((string)this.m_envisionIntegration.LangDictionary.GetText("ExceptionUnexpectedNofRecords"));
                }
            }
            finally
            {
                recordset.Close();
            }
        }

    }

}