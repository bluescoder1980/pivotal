//
// $Workfile: Logging.cs$
// $Revision: 9$
// $Author: tlyne$
// $Date: Wednesday, May 23, 2007 6:59:46 PM$
//
// Copyright © Pivotal Corporation
//

using System;
using System.Text;
using System.Collections.Generic;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Pivotal.Interop.RDALib;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
          
namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// This is a proxy class that uses the the "PAHB Envision Logging" ASR.
    /// </summary>
    public class Logging
    {
        // Envision Logging ASR Name
        private const string LOGGING_ASR_NAME = "PAHB Envision Logging";

        // Pivotal System reference
        private IRSystem7 m_rdaSystem;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pivotalSystem">Pivotal System Reference</param>
        public Logging(IRSystem7 pivotalSystem)
        {
            this.m_rdaSystem = pivotalSystem;
        }

        /// <summary>
        /// Logs Xml
        /// </summary>
        /// <param name="message">Short message describing what is being logged</param>
        /// <param name="xml">Xml to log</param>
        /// <remarks>This method is primarily intended to log Xml that is passed between systems
        /// in an integration.</remarks>
        internal void WriteXml(string message, System.Xml.XmlNode xml)
        {
            // create the parameter instance
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, message);
            transitParams.SetUserDefinedParameter(2, xml.OuterXml);
            object parameterList = transitParams.ParameterList;

            // call the ASR
            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , LOGGING_ASR_NAME, "LOG_XML", ref parameterList);

        }

        /// <summary>
        /// Logs major event messages.
        /// </summary>
        /// <param name="message">Short message string</param>
        /// <remarks>This method is intended for logging major system messages.  As this logging type
        /// is typically always on, it is important to keep these messages to a minimum.</remarks>
        internal void WriteEvent(string message)
        {

            // create the parameter instance
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, message);
            object parameterList = transitParams.ParameterList;

            // call the ASR
            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , LOGGING_ASR_NAME, "LOG_EVENT", ref parameterList);
        }

        /// <summary>
        /// Logs minor processing/debugging messages
        /// </summary>
        /// <param name="message">Short message string</param>
        /// <remarks>This logging type is for logging messages that can be used to diagnose system issues or progress
        /// Typically this logging type is turned off in a working production environment</remarks>
        internal void WriteInformation(string message)
        {
            // create the parameter instance
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, message);
            object parameterList = transitParams.ParameterList;

            // call the ASR
            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , LOGGING_ASR_NAME, "LOG_INFORMATION", ref parameterList);
        }


        private void AddDataMembers(Exception ex)
        {
            if (ex.TargetSite != null)
            {
                if (ex.TargetSite.DeclaringType != null)
                    ex.Data.Add("ClassName", ex.TargetSite.DeclaringType.FullName);

                ex.Data.Add("MemberName", ex.TargetSite.Name);
            }

            if (ex.InnerException != null)
                AddDataMembers(ex.InnerException);
        }


        /// <summary>
        /// Logs application exceptions
        /// </summary>
        /// <param name="ex">The Exception to log</param>
        /// <remarks>The Exception must be serializable.</remarks>
        internal void WriteException(Exception ex)
        {
            AddDataMembers(ex);

            // only primitives and recordsets should be passed through ASRs
            // so the Exception is serialized to a byte array.
            byte[] byteArray = new byte[] { };
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memStream, ex);

                memStream.Flush();
                byteArray = memStream.GetBuffer();
            }

            // create the parameter instance
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, byteArray);
            object parameterList = transitParams.ParameterList;

            // call the ASR
            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , LOGGING_ASR_NAME, "LOG_EXCEPTION", ref parameterList);
        }


        /// <summary>
        /// Logs preformance information.
        /// </summary>
        /// <param name="message">Short performance related message</param>
        internal void WritePerformance(string message)
        {

            // create the parameter instance
            TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
            transitParams.Construct();
            transitParams.SetUserDefinedParameter(1, message);
            object parameterList = transitParams.ParameterList;

            // call the ASR
            RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
            rdaBaseSystem.ExecuteServerScript(m_rdaSystem.SystemName, m_rdaSystem.UserProfile.UserName
                , m_rdaSystem.UserProfile.Password, m_rdaSystem.UserProfile.LoginType, m_rdaSystem.UserProfile.TimeZone
                , LOGGING_ASR_NAME, "LOG_PERFORMANCE", ref parameterList);
        }
    }
}
