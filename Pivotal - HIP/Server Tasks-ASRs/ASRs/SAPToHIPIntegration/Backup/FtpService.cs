//
// $Workfile: FtpService.cs$
// $Revision: 15$
// $Author: tlyne$
// $Date: Monday, July 09, 2007 1:42:21 PM$
//
// Copyright � Pivotal Corporation
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Globalization;

using Pivotal.Interop.RDALib;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// Class for sending files to Envision Ftp server.
    /// </summary>
    internal class FtpService
    {
        private string m_outputBuffer = string.Empty;
        private EnvisionIntegration m_envisionIntegration;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envisionIntegration">Envision Integration ASR reference</param>
        internal FtpService(EnvisionIntegration envisionIntegration)
        {
            this.m_envisionIntegration = envisionIntegration;
        }

        /// <summary>
        /// Sends Xml to Envision.  The file name and file are generated by the business code.
        /// </summary>
        /// <param name="filename">Name of generated file in the temporary directory.</param>
        internal void Send(string filename)
        {

            // lock the instance so not other processes can not modify the instance state
            lock (this)
            {
                // execute the ftp process
                UploadXmlViaFtp(filename
                    , this.m_envisionIntegration.Config.FtpExePath
                    , this.m_envisionIntegration.Config.FtpServer
                    , this.m_envisionIntegration.Config.FtpDestPath
                    , this.m_envisionIntegration.Config.FtpUserName
                    , this.m_envisionIntegration.Config.FtpPassword
                    , this.m_envisionIntegration.Config.FtpTimeout);

                string fileDest = string.Format(CultureInfo.CurrentCulture,  @"ftp://{0}/{1}{2}", this.m_envisionIntegration.Config.FtpServer, this.m_envisionIntegration.Config.FtpDestPath, new FileInfo(filename).Name);

                TransitionPointParameter transitParams = (TransitionPointParameter)this.m_envisionIntegration.PivotalSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                transitParams.Construct();
                transitParams.SetUserDefinedParameter(1, fileDest);
                object parameterList = transitParams.ParameterList;

                RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
                rdaBaseSystem.ExecuteServerScript(this.m_envisionIntegration.PivotalSystem.SystemName, this.m_envisionIntegration.PivotalSystem.UserProfile.UserName
                    , this.m_envisionIntegration.PivotalSystem.UserProfile.Password, this.m_envisionIntegration.PivotalSystem.UserProfile.LoginType, this.m_envisionIntegration.PivotalSystem.UserProfile.TimeZone
                    , "PAHB Envision Integration Transactional", Configuration.UpdateTheLastFtpFileDestination, ref parameterList);
            }
        }

        /// <summary>
        /// Uploads generated xml file to Envision
        /// </summary>
        /// <param name="filename">Filename of generated xml file.</param>
        /// <param name="ftpProgPath">FTP program path</param>
        /// <param name="destServer">FTP server name.</param>
        /// <param name="destPath"></param>
        /// <param name="userName">FTP user name.</param>
        /// <param name="password">FTP user password.</param>
        /// <param name="timeOut">FTP timeout in millisecond</param>
        private void UploadXmlViaFtp(string filename, string ftpProgPath, string destServer, string destPath, string userName, string password, int timeOut)
        {
            try
            {
                // enforce cleanup of ftpProcess if an exception is thrown
                using (Process ftpProcess = new Process())
                {
                    ftpProcess.StartInfo.FileName = ftpProgPath;
                    ftpProcess.StartInfo.Arguments = string.Format(CultureInfo.CurrentCulture, "-n {0}", destServer);
                    ftpProcess.StartInfo.UseShellExecute = false;
                    ftpProcess.StartInfo.RedirectStandardInput = true;
                    ftpProcess.StartInfo.RedirectStandardOutput = true;
                    ftpProcess.StartInfo.RedirectStandardError = true;

                    ftpProcess.OutputDataReceived += new DataReceivedEventHandler(ftpProcess_OutputDataReceived);

                    ftpProcess.Start();
                    ftpProcess.BeginErrorReadLine();
                    ftpProcess.BeginOutputReadLine();

                    // enforce cleanup of the stream writer if an exception is thrown
                    using (StreamWriter streamWriter = ftpProcess.StandardInput)
                    {
                        // send all commands to the ftp process
                        streamWriter.WriteLine(string.Format(CultureInfo.CurrentUICulture, "user {0} {1}", userName, password));
                        streamWriter.WriteLine(string.Format(CultureInfo.CurrentUICulture, "cd {0}", destPath));
                        streamWriter.WriteLine(string.Format(CultureInfo.CurrentUICulture, "put {0}", filename));
                        streamWriter.WriteLine("quit");
                    }

                    // wait until timout for successful completion of commands
                    ftpProcess.WaitForExit(timeOut);

                    // if the timeout has elapsed without completion, kill the ftpProcess
                    if (!ftpProcess.HasExited)
                    {
                        ftpProcess.Kill();

                        // cleanup and throw error
                        ftpProcess.Close();

                        throw new PivotalApplicationException((string)this.m_envisionIntegration.LangDictionary.GetTextSub("ExceptionFtpTimeout", new string[] {filename}));
                    }

                    // check for anying processing error written to the output steam (buffer)
                    if (!string.IsNullOrEmpty(this.m_outputBuffer))
                    {
                        ftpProcess.Close();

                        throw new PivotalApplicationException((string)this.m_envisionIntegration.LangDictionary.GetTextSub("ExceptionFtpSend", new string[] { this.m_outputBuffer, filename }));
                    }

                    // cleanup
                    ftpProcess.Close();
                }
            }
            finally
            {
                // clears the buffer so that any other calls don't throw errors from this call.
                this.m_outputBuffer = string.Empty;
            }
        }

        /// <summary>
        /// Writes the process output to the local buffer.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Ftp output event.</param>
        private void ftpProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
                this.m_outputBuffer = this.m_outputBuffer + e.Data + Environment.NewLine;
        }
    }
}
