using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CRM.Pivotal.IAC.Report.Excel;

namespace TrafficSummaryReport {
    class Program {
        // Tester
        static void Main(string[] args) {
            // Test connection string
            string connectionString = "Data Source=nbcdtpvtl01;Initial Catalog=TIC_Objects_IAC;Integrated Security=True;Connect Timeout=30";

            // Generate workbook
            CRM.Pivotal.IAC.Report.Excel.TrafficSummaryWorkbook tester = new TrafficSummaryWorkbook(connectionString);
            DateTime testDate = DateTime.Parse("5/26/2010");
            
            try {
                tester.CreateReport(testDate);
            } catch(Exception e) {
                Console.WriteLine(e.Message);
                Console.Read();
            }            
               
        }
    }
}
