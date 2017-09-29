// .NET Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

// Pivotal Name Spaces
using Pivotal.Application.Foundation.Utility;
using Pivotal.Application.Foundation.Data.Element;
using Pivotal.Interop.RDALib;
using Pivotal.Interop.ADODBLib;

// Custom Name Spaces
using CRM.Pivotal.IAC.Report.Excel;
using CRM.Pivotal.IAC.Utility;

namespace CRM.Pivotal.IAC.Report {
    /// <summary>
    /// A Pivotal ASR component that facilitates external reports.
    /// </summary>
    /// <author>Ha D. Doan</author>
    /// <created>6/1/2010</created>
    public class IACReports : IRAppScript {

        #region Region: Member variables

        private const string SOURCE = "Pivotal CRM";
        private const string APP_NAME = "IAC Reports: Traffic Summary Excel Report";

        private IRSystem7 _rdaSystem;

        #endregion Member variables

        #region Region: Class Methods
            
        /// <remarks>
        /// Overloaded method. See others for detailed description.
        /// This overloaded method was created to support server-side call;
        /// i.e, Pivotal Scheduled Script.
        /// </remarks>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private void ScheduledSummaryTrafficReport() {
            string resultFile;
            DateTime lastSunday = CalculateLastSundaysDate();

            try {
                Log.LogAppEvent(SOURCE, APP_NAME, "'Scheduled Traffic Excel Report' starts", EventLogEntryType.Information);
                GenerateSummaryTrafficExcelReport(lastSunday, out resultFile, true);    
                Log.LogAppEvent(SOURCE, APP_NAME, "'Scheduled Traffic Excel Report' ends", EventLogEntryType.Information);
            }
            catch (Exception e) {
                // Extract error message.
                Exception exx = e;
                string logMsg = "Error:\n";
                while (exx != null) {
                    logMsg += exx.ToString();
                    exx = exx.InnerException;
                }                
                Log.LogAppEvent(SOURCE, APP_NAME, logMsg, EventLogEntryType.Error);
            }                        
        }        

        /// <remarks>Overloaded method. See others for detailed description</remarks>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private void GenerateSummaryTrafficExcelReport(object weekEndingDate, out string resultFile) {                               
            GenerateSummaryTrafficExcelReport(weekEndingDate, out resultFile, false);
        }

        /// <summary>Invoke the TrafficSummaryWorkbook's CreateReport method.</summary>
        /// <param name="weekEndingDate">Upper bound of the report date range which is a 7-days span.</param>
        /// <param name="resultFile">The returned Excel workbook.</param>
        /// <param name="isScheduled">Flag whether it was called by a scheduled job.</param>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private void GenerateSummaryTrafficExcelReport(object weekEndingDate, out string resultFile, bool isScheduled) {
            resultFile = "";
            DateTime endingDate;
            try {
                endingDate = (DateTime)weekEndingDate;                
                string cnnString = GetConnectionString(); 

                var workbook = new TrafficSummaryWorkbook(cnnString); 
                workbook.CreateReport(endingDate, isScheduled, out resultFile);            
            }
            catch (Exception e) {
                throw new PivotalApplicationException(e.Message, (e.InnerException != null) ? e.InnerException : e, _rdaSystem);
            }
            
        }

        /// <summary>Calculate and return last Sunday's date.</summary>
        /// <returns>Last Sunday's date</returns>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private DateTime CalculateLastSundaysDate() {
            return DateTime.Now.AddDays(-(DateTime.Now.DayOfWeek - DayOfWeek.Sunday));             
        }

        /// <summary>Return the SQL connection string.</summary>
        /// <returns>SQL connection string</returns>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private string GetConnectionString() {
            // Sample EnterpriseString:
            // "provider=RDSO.RSQL;data source=.;initial catalog=ProdMasterED;integrated security=SSPI;APP=Lifecycle Engine AppServer (Build 5.9.2.24)"
            // string cnnString ="Data Source=nbcdtpvtl01;Initial Catalog=TIC_OBJECTS_IAC;Integrated Security=True;Connect Timeout=600";                                
            // string cnnString =@"Data Source=nbcdtpvtl01\instance_a;Initial Catalog=TIC_OBJECTS_IAC;Integrated Security=True;Connect Timeout=600";  
            string cnnString = _rdaSystem.EnterpriseString;
            cnnString = cnnString.Replace("provider=RDSO.RSQL;", "");
                        
            return cnnString;
        }
        #endregion Class Methods

        #region Region: IRAppScript Interface Members
        /// <remarks>Implemented by Ha D. Doan.</remarks>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        public void Execute(string methodName, ref object parameterList) {
            
            try {
                object[] parms = null;
                var tpp = (TransitionPointParameter)_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();                
                tpp.ParameterList = parameterList;
                // if (!tpp.HasValidParameters())                             
                    parms = tpp.GetUserDefinedParameterArray();

                switch(methodName.ToUpper()) {
                case "SCHEDULEDSUMMARYTRAFFICREPORT":
                    this.ScheduledSummaryTrafficReport();
                    break;
                case "GENERATESUMMARYTRAFFICEXCELREPORT":
                    string resultFile;
                    this.GenerateSummaryTrafficExcelReport(parms[0], out resultFile);                     
                    // Build outbound parameter list to return the resultFile
                    tpp.Construct();
                    tpp.SetUserDefinedParameter(1, resultFile);
                    parameterList = tpp.ParameterList;

                    break;
                default: 
                    break;
                }
                
            }
            catch (Exception e) {
                throw new PivotalApplicationException(e.Message, (e.InnerException != null) ? e.InnerException : e, _rdaSystem);
            }

        }

        /// <summary>Sets the IRSystem object and system-relevant vars.</summary>
        /// <remarks>Implemented by Ha D. Doan.</remarks>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        public void SetSystem(RSystem pSystem) {
            _rdaSystem = (IRSystem7)pSystem;
        }

        #endregion IRAppScript Interface Members
       
    }

}
