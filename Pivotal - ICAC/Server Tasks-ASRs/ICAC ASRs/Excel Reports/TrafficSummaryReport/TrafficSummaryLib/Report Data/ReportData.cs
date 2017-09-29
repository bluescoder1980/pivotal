using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient; 
using System.IO;

namespace CRM.Pivotal.IAC.Report.Excel.Data {    
    /*
     * A collection of methods which interface witht the data backend for the purpose of data retrieval.
     * Author       :  Ha D. Doan
     * Created      : June 2010
     */
    internal static class ReportData {

        #region Members

        private static string sqlConnectionString;


        #endregion Members

        #region Enumerator
        
        public enum FactoryDataSets {HousingSpecialists, TrafficByMarketSource, TrafficDailyCounts, AppointmentConversionCounts, LeaseByPropertyCounts, };
        
        #endregion Enumerator

        #region Properties
        
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>  
        internal static string ConnectionString {
            set { sqlConnectionString = value; }
            private get { return sqlConnectionString; }
        }

        #endregion Properties

        #region Methods

        /// <summary>Retrieve unqualified prospect</summary>
        /// <param name="weekEndingDate">Upper bound of the report date range which is a 7-days span.</param>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Unqualified prospects</returns>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        internal static UnQualifiedProspectsRecord GetUnqualifiedProspects(DateTime weekEndingDate, long employeeId) {
            
            UnQualifiedProspectsRecord rec = null;

            try {
                using (SqlConnection cnn = GetConnection()) {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "TIC_Objects_IAC.dbo.xsp_rpt_UnqualifiedProspects";
                    cmd.Connection = cnn;
                    
                    var parm = CreateParm("@weekEndingDate", ParameterDirection.Input, SqlDbType.DateTime, weekEndingDate);
                    cmd.Parameters.Add(parm);
                    
                    parm = CreateParm("@employeeId", ParameterDirection.Input, SqlDbType.BigInt, employeeId);                    
                    cmd.Parameters.Add(parm);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows) {
                        reader.Read();
                        rec = new UnQualifiedProspectsRecord();
                        rec.WalkinsPriceCount = DBNull.Value.Equals(reader["Walkins_Price_Count"]) ? (short)0 : (short)reader["Walkins_Price_Count"];
                        rec.WalkinsLocationCount = DBNull.Value.Equals(reader["Walkins_Location_Count"]) ? (short)0 : (short)reader["Walkins_Location_Count"];
                        rec.WalkinsNeedsCount = DBNull.Value.Equals(reader["Walkins_Needs_Count"]) ? (short)0 : (short)reader["Walkins_Needs_Count"];
                        rec.PhonePriceCount = DBNull.Value.Equals(reader["Phone_Price_Count"]) ? (short)0 : (short)reader["Phone_Price_Count"];
                        rec.PhoneLocationCount = DBNull.Value.Equals(reader["Phone_Location_Count"]) ? (short)0 : (short)reader["Phone_Location_Count"];
                        rec.PhoneNeedsCount = DBNull.Value.Equals(reader["Phone_Needs_Count"]) ? (short)0 : (short)reader["Phone_Needs_Count"];
                    }
                }
            }
            catch (SqlException ex) { throw (ex); }
            catch (Exception ex) { throw (ex); }

            return rec;
        }

        /// <summary>Retrieve housing specialists</summary>
        /// <returns>Housing specialists</returns>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        internal static List<HousingSpecialistRecord> GetHousingSpecialists() {
            List<HousingSpecialistRecord> list = new List<HousingSpecialistRecord>();
            try {
                using (SqlConnection cnn = GetConnection())  {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "TIC_Objects_IAC.dbo.xsp_rpt_Get_HousingSpecialists";
                    cmd.Connection = cnn;
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows) {
                        HousingSpecialistRecord rec;
                        while (reader.Read()) {
                            rec = new HousingSpecialistRecord();                            
                            rec.EmployeeId = (long)reader["Employee_Id"];
                            rec.FullName  = (string)reader["Full_Name"];
                            rec.FirstName = (string)reader["First_Name"];
                            rec.LastName = (string)reader["Last_Name"];

                            list.Add(rec);
                        }
                    }
                }
            }
            catch (SqlException e) { throw (e); }
            catch (Exception e)    { throw (e); }
            return list;
        }

        /// <remarks>Overloaded methods. See other for detailed description.</remarks>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        internal static List<TrafficByMarketSourceRecord> GetTrafficByMarketSource(DateTime weekEndingDate) {
            try {
                return GetTrafficByMarketSource(weekEndingDate, 0);
            }
            catch (SqlException e) { throw (e); }
            catch (Exception e) { throw (e); }
        }

        /// <summary>Retrieve traffic counts by market source</summary>
        /// <param name="weekEndingDate">Upper bound of the report date range which is a 7-days span.</param>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Traffic counts by market source</returns>
        /// <remarks>If employeeId is set to 0 or less, it will retreive all counts for all users.</remarks>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        internal static List<TrafficByMarketSourceRecord> GetTrafficByMarketSource(DateTime weekEndingDate, long employeeId) {
            List<TrafficByMarketSourceRecord> list = new List<TrafficByMarketSourceRecord>();
            try {
                using (SqlConnection cnn = GetConnection()) {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;                    
                    cmd.CommandText = "TIC_Objects_IAC.dbo.xsp_rpt_TrafficByMarketSource";
                    cmd.Connection = cnn;

                    var parm = CreateParm("@weekEndingDate", ParameterDirection.Input, SqlDbType.DateTime, weekEndingDate);
                    cmd.Parameters.Add(parm);

                    if (employeeId > 0) {
                    parm = CreateParm("@employeeId", ParameterDirection.Input, SqlDbType.BigInt, employeeId);
                    cmd.Parameters.Add(parm);
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows) {
                        TrafficByMarketSourceRecord rec;
                        while (reader.Read()) {
                            rec = new TrafficByMarketSourceRecord();
                            rec.HousingSpecialist = (string)reader["Housing_Specialist"];
                            rec.TrafficSource = (string)reader["Traffic_Source"];
                            rec.WalkInCount = DBNull.Value.Equals(reader["Walkin"]) ? 0 : (int)reader["Walkin"];
                            rec.PhoneTrafficCount = DBNull.Value.Equals(reader["Phone"]) ? 0 : (int)reader["Phone"];
                            rec.PhoneSearchCount = DBNull.Value.Equals(reader["Phone_Search"]) ? 0 : (int)reader["Phone_Search"];
                            rec.EmailTrafficCount = DBNull.Value.Equals(reader["Email"]) ? 0 : (int)reader["Email"];
                            rec.EmailSearchCount = DBNull.Value.Equals(reader["Email_Search"]) ? 0 : (int)reader["Email_Search"];

                            list.Add(rec);
                        }
                    }
                }
            }
            catch (SqlException e) { throw (e); }
            catch (Exception e) { throw (e); }

            return list;
        }

        /// <remarks>Overloaded method. See others for detailed description.</remarks>                
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>        
        internal static List<TrafficDailyCountRecord> GetTrafficDailyCounts(DateTime weekEnding) {
            try {
                return GetTrafficDailyCounts(weekEnding, 0);            
            } 
            catch (Exception e) { throw (e); }
        }

        /// <summary>An overloaded method which returns the daily counts for walkins, phones, emails.</summary>
        /// <remarks>If employeeId is set to 0 or less, it returns all counts for all employees.</remarks>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>        
        internal static List<TrafficDailyCountRecord> GetTrafficDailyCounts(DateTime weekEnding, long employeeId) {
            var list = new List<TrafficDailyCountRecord>();
            try {
                using (SqlConnection cnn = GetConnection()) {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "TIC_Objects_IAC.dbo.xSP_rpt_TrafficByDailyFlow";
                    cmd.Connection = cnn;
                    
                    var parm = CreateParm("@weekEndingDate", ParameterDirection.Input, SqlDbType.VarChar, weekEnding);
                    cmd.Parameters.Add(parm);
                    
                    if (employeeId > 0) {
                    parm = CreateParm("@employeeId", ParameterDirection.Input, SqlDbType.BigInt, employeeId);
                    cmd.Parameters.Add(parm);
                    }                    
                    
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows) {
                        TrafficDailyCountRecord rec;
                        while (reader.Read()) {
                            rec = new TrafficDailyCountRecord();
                            rec.Category = DBNull.Value.Equals(reader["Category"]) ? "" : (string)reader["Category"] ;
                            rec.EmployeeId = DBNull.Value.Equals(reader["Employee_Id"]) ? 0L : (long)reader["Employee_Id"];
                            rec.EmployeeName = DBNull.Value.Equals((string)reader["Employee_Name"]) ?  "" : (string)reader["Employee_Name"];
                            rec.WeekDayNumber = DBNull.Value.Equals(reader["Week_Day_Number"]) ? (short)0 : (short)reader["Week_Day_Number"];
                            rec.WeekDay = DBNull.Value.Equals((string)reader["Week_Day"]) ? "" : (string)reader["Week_Day"];
                            rec.Count = DBNull.Value.Equals((int)reader["Daily_Count"]) ? 0 : (int)reader["Daily_Count"];
                            
                            list.Add(rec);
                        }
                    }

                }
            }
            catch (SqlException e) { throw (e); }
            catch (Exception e) { throw(e);}
            return list;
        }

        /// <summary>Returns the appointment conversion counts.</summary>
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>        
        internal static List<AppointmentConversionRecord> GetAppointmentConversionCounts(DateTime weekEnding) {
            var list = new List<AppointmentConversionRecord>();
            try {
                using (SqlConnection cnn = GetConnection()) {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "TIC_Objects_IAC.dbo.xsp_rpt_AppointmentConversions";
                    cmd.Connection = cnn; 
                    var parm = CreateParm("@weekEndingdate", ParameterDirection.Input, SqlDbType.VarChar, weekEnding);                
                    cmd.Parameters.Add(parm);
                                          
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows) {                        
                        while (reader.Read()) {
                            var rec = new AppointmentConversionRecord();
                            rec.ApptConversionSpecialist = DBNull.Value.Equals(reader["Appt_Conversion_Specialist"]) ? "" : (string)reader["Appt_Conversion_Specialist"];
                            rec.FirstHousingSpecialist = DBNull.Value.Equals(reader["First_Housing_Specialist"]) ? "" : (string)reader["First_Housing_Specialist"];
                            rec.Count = DBNull.Value.Equals(reader["Conversion_Count"]) ? 0 : (int)reader["Conversion_Count"];
                            list.Add(rec);
                        }
                    }
                }
                
            }
            catch (SqlException e) { throw (e); }
            catch (Exception e) { throw (e); }

            return list;
        }

        internal static List<LeasesByPropertyRecord> GetLeaseCounts(DateTime weekEnding) {
            var list = new List<LeasesByPropertyRecord>();
            try {
                using (SqlConnection cnn = GetConnection()) {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "TIC_Objects_IAC.dbo.xSP_Rpt_LeasesByProperty";
                    cmd.Connection = cnn; 
                    var parm = CreateParm("@weekEndingdate", ParameterDirection.Input, SqlDbType.VarChar, weekEnding);                
                    cmd.Parameters.Add(parm);
                                          
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows) {                        
                        while (reader.Read()) {
                            var rec = new LeasesByPropertyRecord();
                            rec.Property = (string)reader["Property"];
                            rec.DanCount = DBNull.Value.Equals(reader["Dan_Count"]) ? 0 : (int)reader["Dan_Count"];
                            rec.BrianCount = DBNull.Value.Equals(reader["Brian_Count"]) ? 0 : (int)reader["Brian_Count"];
                            rec.MaryCount = DBNull.Value.Equals(reader["Mary_Count"]) ? 0 : (int)reader["Mary_Count"];
                            rec.ChristineCount = DBNull.Value.Equals(reader["Christine_Count"]) ? 0 : (int)reader["Christine_Count"];                            
                            rec.TerriCount = DBNull.Value.Equals(reader["Terri_Count"]) ? 0 : (int)reader["Terri_Count"];
                            list.Add(rec);
                        }
                    }
                }
                
            }
            catch (SqlException e) { throw (e); }
            catch (Exception e) { throw (e); }

            return list;        
        }
        
        /// <summary>Return relevant file and file folders.</summary>                
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>        
        internal static void GetFileInfo(bool isScheduledReport, DateTime weekEndingDate, out string templateFile, out string targetFile) {            
            string targetFolder = "", paramName = "", paramValu = "";
            templateFile = "";
            targetFile = "";

            try {
                using (SqlConnection cnn = GetConnection()) {
                                        
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "TIC_Objects_IAC.dbo.xsp_rpt_SummaryTrafficReportFileInfo";
                    cmd.Connection = cnn;

                    SqlDataReader reader = cmd.ExecuteReader();
                                        
                    if (reader.HasRows) {                        
                        while (reader.Read()) {
                            if (DBNull.Value.Equals(reader["Param_Name"]) || 
                                DBNull.Value.Equals(reader["Param_Value"]) )                                
                                continue;

                            paramName = (string)reader["Param_Name"];
                            paramValu = (string)reader["Param_Value"];
                            if (paramName == "Traffic Summary Excel Report Template")
                                templateFile = paramValu;
                            else
                                if (!isScheduledReport && paramName == "Traffic Summary Excel Report Output Folder")
                                    targetFolder = paramValu;
                                else
                                    if (isScheduledReport && paramName == "Traffic Summary Scheduled Report Output Folder")
                                        targetFolder = paramValu;
                            
                        }
                    }

                    if (!File.Exists(templateFile)) 
                        throw new IOException("Missing Excel template file!");

                    if (!Directory.Exists(targetFolder))
                        throw new IOException("Missing or inaccessible folder '" + targetFolder + "'");

                    // Add code to check for existing folder.
                    if (isScheduledReport) 
                        targetFile = targetFolder + string.Format("SummaryTraffic_{0:yyyyMMdd}.xls", weekEndingDate);                      
                    else
                        targetFile = targetFolder + DateTime.Now.Ticks.ToString() + ".xls";
                        
                }
            }
            catch (IOException e) { throw (e); }
            catch (SqlException e) { throw (e); }
            catch (Exception e) { throw (e); }
            
        }
            
        #endregion Methods

        #region Helper Methods

        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>  
        private static SqlParameter CreateParm(string name, ParameterDirection input, SqlDbType dataType, object value) {
            var parm = new SqlParameter();
            parm.ParameterName = name;
            parm.Direction = input;
            parm.SqlDbType = dataType;
            parm.Value = value;
                        
            return parm;
        }

        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>  
        private static SqlConnection GetConnection() {            
            try {
                SqlConnection cnn = new SqlConnection(sqlConnectionString);
                cnn.Open();
                return cnn;
            }
            catch (SqlException ex) {
                throw (ex);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        #endregion Helper Methods


    }
}
