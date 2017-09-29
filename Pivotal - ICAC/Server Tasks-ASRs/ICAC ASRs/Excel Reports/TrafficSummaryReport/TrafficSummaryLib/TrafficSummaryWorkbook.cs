// .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

// Third Parties
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.POIFS.FileSystem;
using NPOI.HSSF.Util;

// Customs
using CRM.Pivotal.IAC.Report.Excel.Data; 

namespace CRM.Pivotal.IAC.Report.Excel {

    /*
     * Create the Summary Traffic Excel report.
     * Author       :  Ha D. Doan
     * Created      : June 2010
     */    
    public class TrafficSummaryWorkbook {

        #region Member variables/constants/enums        

        // The following constants are mapped to Excel column letters.
        private const int A = 0, B = 1, C = 2, D = 3, E = 4, F = 5, G = 6, H = 7, I = 8, J = 9; 
        private const int K = 10, L = 11, M = 12, N = 13, O = 14, P = 15, Q = 16, R = 17, k =  18;                

        private HSSFWorkbook workbook;        
        
        private string sqlConnectionString;
        private string templateFile, targetFile;

        private Dictionary<string, int> trafficSourceRowMappings;
        private Dictionary<string, int> propertyNameRowMappings;
        private string[] ExcelTabs = { "Terri", "Mary", "Brian", "Dane", "Property Support", "Traffic Summary"};                

        // State Flags
        private bool showPropertySupport = false;

        // Data results
        private List<TrafficDailyCountRecord> DailyCountList;
        private List<TrafficByMarketSourceRecord> TrafficSourceCountList;
        private List<HousingSpecialistRecord> SpecialistList;
        private List<AppointmentConversionRecord> ApptConversionCountList;

        #endregion Member variables/constants

        #region Constructor(s)
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        public TrafficSummaryWorkbook(string sqlConnectionString) {
            this.sqlConnectionString = sqlConnectionString;
            ReportData.ConnectionString = sqlConnectionString;
            this.BuildTrafficSourceRowMappings();
            this.BuildPropertyNameRowMappings();
        }
        #endregion Constructor(s)

        #region Methods

        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        public void CreateReport(DateTime weekEndingDate, bool isScheduledReport, out string resultFile) {
            resultFile = "";
            FileStream template;
            HSSFSheet worksheet;
            
            try {
                // Retrieve files
                ReportData.GetFileInfo(isScheduledReport, weekEndingDate, out this.templateFile, out this.targetFile);
                // Create workbook
                using( template = new FileStream(this.templateFile, FileMode.Open, FileAccess.Read)) {
                    workbook = new HSSFWorkbook(template);
                    // Retrieve Housing Specialists
                    SpecialistList = ReportData.GetHousingSpecialists();
                    for (int i = 0; i < SpecialistList.Count; i++) {
                        worksheet = workbook.GetSheet(SpecialistList[i].FirstName);
                        worksheet.GetRow(3).GetCell(B).SetCellValue(String.Format("{0:MMMM d, yyyy}", weekEndingDate));                    
                        PopulateTrafficSource(worksheet, SpecialistList[i].FullName, weekEndingDate);
                        PopulateDailyFlows(worksheet, SpecialistList[i].FullName, weekEndingDate);
                        PopulateApptsConversions(worksheet, SpecialistList[i].FullName, weekEndingDate);
                        worksheet.ForceFormulaRecalculation = true;
                    }

                    // Property Support Sheet
                    worksheet = workbook.GetSheet("Property Support");
                    worksheet.GetRow(3).GetCell(B).SetCellValue(String.Format("{0:MMMM d, yyyy}", weekEndingDate));
                    PopulateTrafficSource(worksheet, "Property Support", weekEndingDate);                                    
                    PopulateDailyFlows(worksheet, "Property Support", weekEndingDate);
                    PopulateApptsConversions(worksheet, "Property Support", weekEndingDate);
                    worksheet.ForceFormulaRecalculation = true; 

                    // Leases Sheet
                    worksheet = workbook.GetSheet("Leases");
                    PopulateLeases(worksheet, weekEndingDate);                    
                    worksheet.ForceFormulaRecalculation = true; 

                    // Summary Sheet
                    worksheet = workbook.GetSheet("Traffic Summary");
                    worksheet.ForceFormulaRecalculation = true;                
                    WriteToFile(workbook, targetFile);
                    template.Close();
                    resultFile = targetFile;
                }
            }            
            catch (IOException e) { throw (e); }            
            catch (Exception e) { throw (e); }            
        }

        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private void PopulateTrafficSource(HSSFSheet sheet, string housingSpecialist, DateTime weekEndingDate) {
            int rowIndex = 0;            
            HSSFRow row;                    
            if (TrafficSourceCountList == null) {
                try { TrafficSourceCountList = ReportData.GetTrafficByMarketSource(weekEndingDate); }
                catch (Exception e) { throw (e);  }
            }

            if (this.TrafficSourceCountList == null)
                return;

            for (int i = 0; i < TrafficSourceCountList.Count; i++) {
                if (housingSpecialist == "Property Support") 
                    if (FindHousingSpecialist(this.TrafficSourceCountList[i].HousingSpecialist) >= 0)
                        continue;
                    else
                        this.showPropertySupport = true;
                else 
                    if (this.TrafficSourceCountList[i].HousingSpecialist != housingSpecialist)
                        continue;

                if (!this.trafficSourceRowMappings.ContainsKey(this.TrafficSourceCountList[i].TrafficSource))
                    continue;

                rowIndex = trafficSourceRowMappings[this.TrafficSourceCountList[i].TrafficSource];
                row = sheet.GetRow(rowIndex);

                // column D: walk-in search
                if (this.TrafficSourceCountList[i].WalkInCount > 0 )                    
                    SetCellCumulatively(row.GetCell(D), this.TrafficSourceCountList[i].WalkInCount);
                
                // column E: Phone Traffic
                if (this.TrafficSourceCountList[i].PhoneTrafficCount > 0) 
                    SetCellCumulatively(row.GetCell(E), this.TrafficSourceCountList[i].PhoneTrafficCount);                

                // column F: Phone Search
                if (this.TrafficSourceCountList[i].PhoneSearchCount> 0) 
                    SetCellCumulatively(row.GetCell(F), this.TrafficSourceCountList[i].PhoneSearchCount);

                // column I: Email Traffic
                if (this.TrafficSourceCountList[i].EmailTrafficCount > 0)                     
                    SetCellCumulatively(row.GetCell(I), this.TrafficSourceCountList[i].EmailTrafficCount);

                // column J: Email Search                
                if (this.TrafficSourceCountList[i].EmailSearchCount > 0) 
                    SetCellCumulatively(row.GetCell(J), this.TrafficSourceCountList[i].EmailSearchCount);
            }

        }

        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private void PopulateDailyFlows(HSSFSheet sheet, string housingSpecialist, DateTime weekEndingDate) { 
            if (this.DailyCountList == null) {
                try { this.DailyCountList = ReportData.GetTrafficDailyCounts(weekEndingDate); }
                catch (Exception e) { throw (e); }
            }            

            if (this.DailyCountList == null)
                return;

            
            int startRow = 0;
            string category = "";
         
            for (int i =0; i < this.DailyCountList.Count; i++) {
                if (housingSpecialist == "Property Support")
                    if (FindHousingSpecialist(this.DailyCountList[i].EmployeeName) >= 0)
                        continue;
                    else
                        this.showPropertySupport = true;
                else 
                    if (DailyCountList[i].EmployeeName != housingSpecialist) 
                        continue;

                if (DailyCountList[i].Category != category) {
                    category = DailyCountList[i].Category;
                    switch (category.ToUpper()) {
                    case ("DAILY TRAFFIC FLOW"):
                        startRow = 31;
                        break;
                    case ("DAILY PHONE CALLS"):
                        startRow = 41;
                        break;
                    case ("DAILY VOICE MAIL"):
                        startRow = 51;
                        break;
                    }                    
                    
                }
                
                int weekDayNumber = DailyCountList[i].WeekDayNumber == 1 ? 7 : DailyCountList[i].WeekDayNumber - 1;
                int rowIndex = startRow + weekDayNumber -1;
                if (housingSpecialist == "Property Support")
                    SetCellCumulatively(sheet.GetRow(rowIndex).GetCell(M), DailyCountList[i].Count);
                else
                    sheet.GetRow(rowIndex).GetCell(M).SetCellValue(DailyCountList[i].Count);                
            }            
        }
        
        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private void PopulateApptsConversions(HSSFSheet sheet, string housingSpecialist, DateTime weekEndingDate) { 
            int rowIndex = 0;

            if (this.ApptConversionCountList == null) {
                try { this.ApptConversionCountList = ReportData.GetAppointmentConversionCounts(weekEndingDate); }
                catch (Exception e) { throw (e); }
            }

            if (this.ApptConversionCountList == null)
                return;

            for (int i=0; i<this.ApptConversionCountList.Count; i++) {
                if (housingSpecialist == "Property Support")
                    if (FindHousingSpecialist(this.ApptConversionCountList[i].ApptConversionSpecialist) >= 0)
                        continue;
                    else
                        this.showPropertySupport = true; 
                else
                    if (this.ApptConversionCountList[i].ApptConversionSpecialist != housingSpecialist)
                        continue;

                if (this.ApptConversionCountList[i].FirstHousingSpecialist != housingSpecialist)
                    rowIndex = GetAppointmentConversionRow(this.ApptConversionCountList[i].FirstHousingSpecialist);
                else 
                    rowIndex = GetAppointmentConversionRow(housingSpecialist);
                
                if (housingSpecialist == "Property Support")
                    SetCellCumulatively(sheet.GetRow(rowIndex).GetCell(C), this.ApptConversionCountList[i].Count);
                else
                    sheet.GetRow(rowIndex).GetCell(C).SetCellValue(this.ApptConversionCountList[i].Count);

            }    
        
        }

        // private void PopulateUnqualifiedProspects(HSSFSheet sheet, long employeeId) { }

        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private void PopulateLeases(HSSFSheet sheet, DateTime weekEndingDate) {                      
            // HSSFRow row;                    
            const int   DANE_COUNT = B, BRIAN_COUNT = C, MARY_COUNT = D, CHRISTINE_COUNT = E,
                        TERRI_COUNT = F;

            List<LeasesByPropertyRecord> list;
            try {

                list = ReportData.GetLeaseCounts(weekEndingDate);
            }
            catch (Exception e) { throw (e); }

            if (list == null)
                return;

            for (int i=0; i<list.Count; i++) {
                if (!this.propertyNameRowMappings.ContainsKey(list[i].Property))
                    continue;

                var rowIndex = this.propertyNameRowMappings[list[i].Property];
                var row = sheet.GetRow(rowIndex);

                if (list[i].DanCount > 0)
                    SetCellCumulatively(row.GetCell(DANE_COUNT), list[i].DanCount);

                if (list[i].BrianCount > 0)
                    SetCellCumulatively(row.GetCell(BRIAN_COUNT), list[i].BrianCount);

                if (list[i].MaryCount > 0)
                    SetCellCumulatively(row.GetCell(MARY_COUNT), list[i].MaryCount);

                if (list[i].ChristineCount > 0)
                    SetCellCumulatively(row.GetCell(CHRISTINE_COUNT), list[i].ChristineCount);

                if (list[i].TerriCount > 0)
                    SetCellCumulatively(row.GetCell(TERRI_COUNT), list[i].TerriCount);

            }


            /*
                            if (!this.trafficSourceRowMappings.ContainsKey(this.TrafficSourceCountList[i].TrafficSource))
                    continue;

                rowIndex = trafficSourceRowMappings[this.TrafficSourceCountList[i].TrafficSource];
                row = sheet.GetRow(rowIndex);
             */
             


        }

        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private void WriteToFile(HSSFWorkbook workbook, string targetFile) {
            try {
                FileStream file = new FileStream(targetFile, FileMode.Create);
                workbook.Write(file);
                file.Close();
            }
            catch (IOException e) {
                throw (e);
            }
        }

        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private void BuildTrafficSourceRowMappings() {
            if (this.trafficSourceRowMappings != null)
                return;
            trafficSourceRowMappings = new Dictionary<string, int>();            
            trafficSourceRowMappings.Add("Apartment Guide", 32);
            trafficSourceRowMappings.Add("Bus Shelters/Bus Ads", 33);
            trafficSourceRowMappings.Add("Chambers/Visitors Bureau", 34);         
            trafficSourceRowMappings.Add("Cinema Ads", 35);
            trafficSourceRowMappings.Add("Craigslist.Org", 36);
            trafficSourceRowMappings.Add("Direct Mail", 37);
            trafficSourceRowMappings.Add("Drive By", 38);
            trafficSourceRowMappings.Add("For Rent Magazine", 39);
            trafficSourceRowMappings.Add("Housing Auth/Afford Housing", 40);
            trafficSourceRowMappings.Add("Internet - Other", 41);
            trafficSourceRowMappings.Add("Magazines - Other", 42);
            trafficSourceRowMappings.Add("Newspapers", 43);
            trafficSourceRowMappings.Add("O.C. Metro", 44);
            trafficSourceRowMappings.Add("Orange County Register", 45);
            trafficSourceRowMappings.Add("Pennysaver", 46);
            trafficSourceRowMappings.Add("Ref-College/University", 47);
            trafficSourceRowMappings.Add("Ref-Current Resident", 48);
            trafficSourceRowMappings.Add("Ref-Employee", 49);
            trafficSourceRowMappings.Add("Ref-Merchant/Locator", 50);
            trafficSourceRowMappings.Add("Ref-Other IAC Community/Corp", 51);
            trafficSourceRowMappings.Add("Ref-Previous Resident/Frien", 52);
            trafficSourceRowMappings.Add("Ref-Realtor/Broker", 53);
            trafficSourceRowMappings.Add("Relocation Services", 54);           
            trafficSourceRowMappings.Add("Rental Living Magazine", 55);
            trafficSourceRowMappings.Add("Rental-Living.com", 56);
            trafficSourceRowMappings.Add("Signs/Bootlegs", 57);
            trafficSourceRowMappings.Add("Television Ads", 58);
            trafficSourceRowMappings.Add("Yellow Pages", 59);
            trafficSourceRowMappings.Add("Other - Not on list", 60);
        }

        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private void BuildPropertyNameRowMappings() {
            if (this.propertyNameRowMappings != null)
                return;
            this.propertyNameRowMappings = new Dictionary<string, int>();
            this.propertyNameRowMappings.Add("Ambrose Apartment Homes", 2);         
            this.propertyNameRowMappings.Add("Anacapa Apartment Homes", 3);
            this.propertyNameRowMappings.Add("Baypointe Apartment Homes", 4);
            this.propertyNameRowMappings.Add("Berkeley/Columbia Court Apartments", 6);
            this.propertyNameRowMappings.Add("Bordeaux Apartment Homes", 7);
            this.propertyNameRowMappings.Add("Brittany at Oak Creek", 8);
            this.propertyNameRowMappings.Add("Cedar Creek Apartments", 9);
            this.propertyNameRowMappings.Add("Cross Creek Apartments", 10);
            this.propertyNameRowMappings.Add("Dartmouth Court Apartments", 11);
            this.propertyNameRowMappings.Add("Deerfield Apartments", 13);
            this.propertyNameRowMappings.Add("Esperanza Apartment Homes", 15);
            this.propertyNameRowMappings.Add("Estancia Apartment Homes", 16);
            this.propertyNameRowMappings.Add("Harvard/Cornell Court Apartments", 17);
            this.propertyNameRowMappings.Add("Las Palmas Apartment Homes", 18);
            this.propertyNameRowMappings.Add("Mariner Square Apartments", 19);
            this.propertyNameRowMappings.Add("Mirasol Apartment Homes", 20);
            this.propertyNameRowMappings.Add("Newport Bluffs Apartment Village", 21);
            this.propertyNameRowMappings.Add("Newport North Apartment Homes", 22);
            this.propertyNameRowMappings.Add("Newport Ridge Apartments", 23);
            this.propertyNameRowMappings.Add("Northwood Park Apartments", 24);
            this.propertyNameRowMappings.Add("Northwood Place Apartments", 25);
            this.propertyNameRowMappings.Add("Oak Glen Apartment Homes", 26);
            this.propertyNameRowMappings.Add("Orchard Hills Apartment Homes", 27);
            this.propertyNameRowMappings.Add("Palmeras Apartment Homes", 28);
            this.propertyNameRowMappings.Add("Parkwest Apartment Homes", 29);
            this.propertyNameRowMappings.Add("Parkwood Apartment Homes", 30);
            this.propertyNameRowMappings.Add("Portola Place Apartment Homes", 31);
            this.propertyNameRowMappings.Add("Promontory Point Villa Apartments", 32);
            this.propertyNameRowMappings.Add("Quail Meadow Apartment Homes", 33);
            this.propertyNameRowMappings.Add("Quail Ridge Apartment Homes", 34);
            this.propertyNameRowMappings.Add("Rancho Alisal Apartment Homes", 35);
            this.propertyNameRowMappings.Add("Rancho Maderas Apartment Homes", 36);
            this.propertyNameRowMappings.Add("Rancho Mariposa Apartments", 37);
            this.propertyNameRowMappings.Add("Rancho Monterey Apartment Homes", 38);
            this.propertyNameRowMappings.Add("Rancho San Joaquin Apartment Homes", 39);
            this.propertyNameRowMappings.Add("Rancho Santa Fe Apartment Homes", 40);
            this.propertyNameRowMappings.Add("Rancho Tierra Apartments", 41);
            this.propertyNameRowMappings.Add("San Carlo Villa Apartment Homes", 42);
            this.propertyNameRowMappings.Add("San Leon Villa Apartment Homes", 43);
            this.propertyNameRowMappings.Add("San Marco Villa Apartment Homes", 44);
            this.propertyNameRowMappings.Add("San Marino Villa Apartment Homes", 45);
            this.propertyNameRowMappings.Add("San Mateo Apartment Homes", 46);
            this.propertyNameRowMappings.Add("San Paulo Apartment Homes", 47);
            this.propertyNameRowMappings.Add("San Remo Villa Apartment Homes", 48);
            this.propertyNameRowMappings.Add("Santa Clara Apartment Homes", 49);
            this.propertyNameRowMappings.Add("Santa Maria Apartment Homes", 50);
            this.propertyNameRowMappings.Add("Santa Rosa Apartment Homes", 51);
            this.propertyNameRowMappings.Add("Serrano Apartment Homes", 52);
            this.propertyNameRowMappings.Add("Shadow Oaks Apartment Homes", 53);
            this.propertyNameRowMappings.Add("Sierra Vista Apartment Homes", 54);
            this.propertyNameRowMappings.Add("Solana Apartment Homes", 55);
            this.propertyNameRowMappings.Add("Somerset Apartment Homes", 56);
            this.propertyNameRowMappings.Add("Sonoma at Oak Creek", 57);
            this.propertyNameRowMappings.Add("Stanford Court Apartments", 58);
            this.propertyNameRowMappings.Add("The Park at Irvine Spectrum Center", 59);
            this.propertyNameRowMappings.Add("Turtle Ridge Apartment Homes", 60);
            this.propertyNameRowMappings.Add("Turtle Rock Canyon Apartments", 61);
            this.propertyNameRowMappings.Add("Turtle Rock Vista Apartments", 62);
            this.propertyNameRowMappings.Add("Villa Coronado Apartment Homes", 63);
            this.propertyNameRowMappings.Add("Villa Siena Apartment Homes", 64);
            this.propertyNameRowMappings.Add("The Village at Irvine Spectrum Center", 65);
            this.propertyNameRowMappings.Add("Windwood Glen Apartment Homes", 66);
            this.propertyNameRowMappings.Add("Windwood Knoll Apartment Homes", 67);
            this.propertyNameRowMappings.Add("Woodbridge Pines Apartment Homes", 68);
            this.propertyNameRowMappings.Add("Woodbridge Villas Apartment Homes", 69);
            this.propertyNameRowMappings.Add("Woodbridge Willows Apartments", 70);
            this.propertyNameRowMappings.Add("Woodbury Court Apartment Homes", 71);
            this.propertyNameRowMappings.Add("Woodbury Lane Apartment Homes", 72);
            this.propertyNameRowMappings.Add("Woodbury Place Apartment Homes",73);
            this.propertyNameRowMappings.Add("Woodbury Square Apartment Homes", 74);
        }

        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private int FindHousingSpecialist(string fullName) {
            if (this.SpecialistList == null || this.SpecialistList.Count == 0)
                return -1;
            
            for (int i = 0; i < SpecialistList.Count; i++) 
                if (this.SpecialistList[i].FullName.ToUpper() == fullName.ToUpper())
                    return i;
            
            return -1;
        }

        /// <author>Ha D. Doan</author>
        /// <created>6/1/2010</created>
        private void SetCellCumulatively(HSSFCell cell, int valu) {                            
            int celValue;
            if (int.TryParse(cell.ToString(), out celValue))
                celValue += valu;
            else
                celValue = valu;

             cell.SetCellValue(celValue);
        }

        private int GetAppointmentConversionRow(string housingSpecialist) {
                       
            switch (housingSpecialist.ToUpper()) {
            case ("TERRI INGRAM"): return 20;                
            case ("PROPERTY SUPPORT"): return 21;                
            case ("MARY TEHRANIAN"): return 22;                
            case ("BRIAN WYSOLMIERSKI"): return 23;                
            case ("DANE WIELER"): return 24;                
            default: return 21;     // Same as Property Support
            }

        }
        #endregion

        #region Deprecated
        //private static HSSFFont StandardFont;
        //private static HSSFFont StandardFontItalic;
        //private static HSSFFont StandardFontBold;
        //private static HSSFFont StandardFontBoldItalic;
        //private static HSSFFont StandardFontHighlighted;

        //private static List<string> marketSources = new List<string>();

        //public void CreateReport(DateTime weekEndingDate)
        //{
        //    FileStream file;
        //    try
        //    {
        //        // Retrieve files
        //        ReportData.GetFileInfo(out templateFile, out targetFile);
        //        // Create workbook
        //        file = new FileStream(@"C:\Temp\Excel_Using_NPOI\ExcelTemplate\TrafficSummaryTemplate.xls.xls", FileMode.Open, FileAccess.Read);
        //        workbook = new HSSFWorkbook(file);
        //    }
        //    catch (IOException e) { throw (e); }
        //    catch (Exception e) { throw (e); }

        //    // Create standard fonts
        //    CreateStandardFonts();

        //    // Retrieve housing specialists
        //    string fullName = "";
        //    List<string> housingSpecialists = new List<string>();
        //    List<HousingSpecialistRecord> SpecialistList = ReportData.GetHousingSpecialists();
        //    HousingSpecialistRecord rec;

        //    // Generate worksheets
        //    HSSFSheet worksheet;
        //    HSSFSheet summaryWs = CloneSheet("Individual_Sheet", "Traffic_Summary");

        //    // Compile list of housing specialists.
        //    for (int i = 0; i < SpecialistList.Count; i++)
        //        housingSpecialists.Add(SpecialistList[i].FirstName + " " + SpecialistList[i].LastName);


        //    // Build individual pages
        //    for (int i = 0; i < SpecialistList.Count; i++)
        //    {
        //        rec = SpecialistList[i];
        //        fullName = rec.FirstName + " " + rec.LastName;
        //        // worksheet = CloneSheet("Individual_Sheet", fullName);
        //        worksheet = CloneSheet("Individual_Sheet", rec.FirstName + "_" + rec.LastName);
        //        FillIndividualSheet(worksheet, housingSpecialists, rec, weekEndingDate);
        //    }

        //    // Build Summary sheet
        //    //worksheet = CloneSheet("Individual_Sheet", "Traffic_Summary");
        //    // FillSummarySheet(worksheet, housingSpecialists, weekEndingDate);
        //    FillSummarySheet(summaryWs, housingSpecialists, weekEndingDate);

        //    // Remove template sheets
        //    workbook.RemoveSheetAt(workbook.GetSheetIndex("Individual_Sheet"));

        //    // Save new workbook
        //    WriteToFile(workbook);

        //    // Clean up
        //    file.Close();
        //    file.Dispose();
        //}



        //private void BuildDailyActivitiesBlock(HSSFSheet worksheet, long employeeId, DateTime weekEndingDate)
        //{
        //    // Daily Activities
        //    int startRow = 0;
        //    HSSFCell cel;
        //    string category = "";
        //    SummaryTrafficByDailyFlowRecord DFRec;
        //    List<SummaryTrafficByDailyFlowRecord> list2 = ReportData.GetSummaryTrafficByDailyFlow(weekEndingDate, employeeId);
        //    for (int i = 0; i < list2.Count; i++)
        //    {
        //        DFRec = list2[i];
        //        if (DFRec.Category != category)
        //        {
        //            category = DFRec.Category;
        //            switch (category.ToUpper())
        //            {
        //                case "DAILY TRAFFIC FLOW":
        //                    startRow = 48;
        //                    break;
        //                case "DAILY PHONE CALLS":
        //                    startRow = 58;
        //                    break;
        //                case "DAILY VOICE MAIL":
        //                    startRow = 68;
        //                    break;
        //                case "IAC WEBSITE":
        //                    startRow = 78;
        //                    break;
        //                default:
        //                    startRow = 0;
        //                    break;
        //            }
        //        }

        //        if (startRow != 0)
        //        {
        //            cel = CreateCell(worksheet, startRow, K);
        //            HighlightCell(cel);
        //            cel.SetCellValue(DFRec.TrafficCount);
        //        }

        //        startRow++;
        //    }


        //    // create formulas            
        //    worksheet.GetRow(35).GetCell(K).SetCellFormula("SUM(K29:K35)"); // Daily Traffic Flow total
        //    worksheet.GetRow(45).GetCell(K).SetCellFormula("SUM(K39:K45)"); // Daily Phone Calls total
        //    worksheet.GetRow(55).GetCell(K).SetCellFormula("SUM(K49:K55)"); // Daily Voice Mail total
        //    worksheet.GetRow(11).GetCell(D).SetCellFormula("SUM(D7-D9-D10-D11)"); // Net traffic searches (walkins)
        //    worksheet.GetRow(11).GetCell(J).SetCellFormula("SUM(J7-J9-J10-J11)"); // Net traffic searches (walkins)
        //}

        //private int BuildTrafficSourceDetailsBlock(HSSFSheet worksheet, List<string> housingSpecialists, HousingSpecialistRecord HSRec, DateTime weekEndingDate)
        //{
        //    int startRow = 48;
        //    HSSFCell cel;
        //    bool cacheMarketSources = marketSources.Count == 0 ? true : false;

        //    // Traffic Source            
        //    SummaryTrafficByMarketSourceRecord MSRec;
        //    List<SummaryTrafficByMarketSourceRecord> list = ReportData.GetSummaryTrafficByMarketSource(weekEndingDate, HSRec.EmployeeId);
        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        MSRec = list[i];
        //        // Cache Market Sources
        //        if (cacheMarketSources)
        //            marketSources.Add(MSRec.MarketingProjectName);

        //        // Trafic source
        //        worksheet.AddMergedRegion(new Region(startRow, A, startRow, C));
        //        cel = CreateCell(worksheet, startRow, A);
        //        cel.SetCellValue(MSRec.MarketingProjectName);

        //        // Walk-in Search
        //        cel = CreateCell(worksheet, startRow, D);
        //        HighlightCell(cel);
        //        cel.SetCellValue(MSRec.WalkInSearchCount);

        //        // Phone Traffic
        //        cel = CreateCell(worksheet, startRow, E);
        //        HighlightCell(cel);
        //        cel.SetCellValue(MSRec.PhoneTrafficCount);

        //        // Phone Search
        //        cel = CreateCell(worksheet, startRow, F);
        //        HighlightCell(cel);
        //        cel.SetCellValue(MSRec.PhoneSearchCount);

        //        // Walk-in Roommate
        //        cel = CreateCell(worksheet, startRow, G);
        //        HighlightCell(cel);
        //        cel.SetCellValue(MSRec.WalkInRoommateCount);

        //        // Phone Roommate
        //        cel = CreateCell(worksheet, startRow, H);
        //        HighlightCell(cel);
        //        cel.SetCellValue(MSRec.PhoneRoommateCount);

        //        // Next row.
        //        startRow++;
        //    }

        //    // Create formulas
        //    cel = CreateCell(worksheet, startRow, C, true);
        //    cel.SetCellValue("TOTALS");
        //    cel = CreateCell(worksheet, startRow, D, true);
        //    cel.SetCellFormula(string.Format("SUM(D{0}:D{1})", (startRow - list.Count), startRow));
        //    cel = CreateCell(worksheet, startRow, E, true);
        //    cel.SetCellFormula(string.Format("SUM(E{0}:E{1})", (startRow - list.Count), startRow));
        //    cel = CreateCell(worksheet, startRow, F, true);
        //    cel.SetCellFormula(string.Format("SUM(F{0}:F{1})", (startRow - list.Count), startRow));
        //    cel = CreateCell(worksheet, startRow, G, true);
        //    cel.SetCellFormula(string.Format("SUM(G{0}:G{1})", (startRow - list.Count), startRow));
        //    cel = CreateCell(worksheet, startRow, H, true);
        //    cel.SetCellFormula(string.Format("SUM(H{0}:H{1})", (startRow - list.Count), startRow));

        //    // Formula Total Traffic Seen, etc...
        //    worksheet.GetRow(6).GetCell(D).SetCellFormula(string.Format("D{0}+G{0}", startRow + 1));        // Total Traffic Seen
        //    worksheet.GetRow(8).GetCell(D).SetCellFormula(string.Format("$G${0}", startRow + 1));           // Less Roommates (Walk-in)
        //    worksheet.GetRow(6).GetCell(J).SetCellFormula(string.Format("E{0}+F{0}+H{0}", startRow + 1));   // Total Phone Calls
        //    worksheet.GetRow(8).GetCell(J).SetCellFormula(string.Format("$H${0}", startRow + 1));           // Less Roommates (Phone)

        //    return startRow; // Return the row which holds the totals
        //}

        //private void BuildUnqualifiedProspectsBlock(HSSFSheet worksheet, long employeeId, DateTime weekEndingDate)
        //{
        //    UnQualifiedProspectsRecord UPRec = ReportData.GetUnqualifiedProspects(weekEndingDate, employeeId);
        //    if (UPRec != null)
        //    {
        //        worksheet.GetRow(20).GetCell(J).SetCellValue(UPRec.WalkinsPriceCount);
        //        worksheet.GetRow(21).GetCell(J).SetCellValue(UPRec.WalkinsLocationCount);
        //        worksheet.GetRow(22).GetCell(J).SetCellValue(UPRec.WalkinsNeedsCount);

        //        worksheet.GetRow(20).GetCell(K).SetCellValue(UPRec.PhonePriceCount);
        //        worksheet.GetRow(21).GetCell(K).SetCellValue(UPRec.PhoneLocationCount);
        //        worksheet.GetRow(22).GetCell(K).SetCellValue(UPRec.PhoneNeedsCount);
        //    }
        //    worksheet.GetRow(23).GetCell(J).SetCellFormula("SUM(J21:J23)");     // Total Unqualified walkin prospects
        //    worksheet.GetRow(23).GetCell(K).SetCellFormula("SUM(K21:K23)");     // Total Unqualified phone prospects                        
        //    worksheet.GetRow(9).GetCell(D).SetCellFormula("$J$24"); // Less unqualified prospects (walkins)
        //    worksheet.GetRow(9).GetCell(J).SetCellFormula("$K$24"); // Less unqualified prospects (phone)

        //}

        //private int BuildHousingSpecialistsBlock(HSSFSheet worksheet, List<string> specialists, string theSpecialist, int detailTotalsRow)
        //{
        //    const int TOT_ROW = 44;     // row 45
        //    const int INIT_ROW = 20;    // row 21
        //    int startRow = 20;          // row 21
        //    int theSpecialistRow = 0;
        //    HSSFCell cel;
        //    for (int i = 0; i < specialists.Count; i++)
        //    {
        //        cel = CreateCell(worksheet, startRow, A);
        //        cel.SetCellValue(specialists[i]);
        //        if (specialists[i] == theSpecialist)
        //            theSpecialistRow = startRow;

        //        startRow++;
        //    }
        //    // Create Formulas
        //    worksheet.GetRow(TOT_ROW).GetCell(C).SetCellFormula(string.Format("SUM(C{0}:C{1})", (INIT_ROW + 1), startRow));
        //    worksheet.GetRow(TOT_ROW).GetCell(D).SetCellFormula(string.Format("SUM(D{0}:D{1})", (INIT_ROW + 1), startRow));
        //    worksheet.GetRow(TOT_ROW).GetCell(E).SetCellFormula(string.Format("SUM(E{0}:E{1})", (INIT_ROW + 1), startRow));
        //    worksheet.GetRow(TOT_ROW).GetCell(F).SetCellFormula(string.Format("SUM(F{0}:F{1})", (INIT_ROW + 1), startRow));
        //    worksheet.GetRow(TOT_ROW).GetCell(G).SetCellFormula(string.Format("SUM(G{0}:G{1})", (INIT_ROW + 1), startRow));
        //    worksheet.GetRow(TOT_ROW).GetCell(H).SetCellFormula(string.Format("SUM(H{0}:H{1})", (INIT_ROW + 1), startRow));

        //    // startRow++;
        //    // Hide unused extra rows
        //    for (; startRow < TOT_ROW; startRow++)
        //        worksheet.GetRow(startRow).Height = 0;

        //    worksheet.GetRow(theSpecialistRow).GetCell(D).SetCellFormula(string.Format("$D${0}", detailTotalsRow + 1));
        //    worksheet.GetRow(theSpecialistRow).GetCell(E).SetCellFormula(string.Format("$E${0}", detailTotalsRow + 1));
        //    worksheet.GetRow(theSpecialistRow).GetCell(F).SetCellFormula(string.Format("$F${0}", detailTotalsRow + 1));
        //    worksheet.GetRow(theSpecialistRow).GetCell(G).SetCellFormula(string.Format("$G${0}", detailTotalsRow + 1));
        //    worksheet.GetRow(theSpecialistRow).GetCell(H).SetCellFormula(string.Format("$H${0}", detailTotalsRow + 1));

        //    worksheet.GetRow(13).GetCell(J).SetCellFormula(string.Format("$C${0}", TOT_ROW + 1));   // Converted Calls to Appoinments
        //    worksheet.GetRow(15).GetCell(J).SetCellFormula("J14/J12");                              // CONVERSION RATIO

        //    return theSpecialistRow; // returns the row index of theSpecialist 
        //}

        //private void FillIndividualSheet(HSSFSheet worksheet, List<string> housingSpecialists, HousingSpecialistRecord HSRec, DateTime weekEndingDate)
        //{

        //    // Unqualified Prospects
        //    BuildUnqualifiedProspectsBlock(worksheet, HSRec.EmployeeId, weekEndingDate);

        //    // Traffic Source Details
        //    int detailTotalsRow = BuildTrafficSourceDetailsBlock(worksheet, housingSpecialists, HSRec, weekEndingDate);

        //    // Daily Activities
        //    BuildDailyActivitiesBlock(worksheet, HSRec.EmployeeId, weekEndingDate);

        //    //Housing Specialists
        //    BuildHousingSpecialistsBlock(worksheet, housingSpecialists, HSRec.FirstName + " " + HSRec.LastName, detailTotalsRow);

        //    // Housing Specialist name
        //    CreateCell(worksheet, 3, I).SetCellValue(HSRec.FirstName + " " + HSRec.LastName);
        //    // CreateCell(worksheet, 20, A).SetCellValue(HSRec.FirstName + " " + HSRec.LastName);
        //}

        //private void FillSummarySheet(HSSFSheet worksheet, List<string> housingSpecialists, DateTime weekEndingDate)
        //{
        //    BuildSummaryTrafficSourceDetailsBlock(worksheet, housingSpecialists, weekEndingDate);
        //}


        //private int BuildSummaryTrafficSourceDetailsBlock(HSSFSheet worksheet, List<string> housingSpecialists, DateTime weekEndingDate)
        //{
        //    const int ASCIIOffset = 64;
        //    int startRow = 48;
        //    HSSFCell cel;
        //    string formula, element;
        //    char columnLetter;
        //    // Traffic Source            
        //    for (int i = 0; i < marketSources.Count; i++)
        //    {
        //        // Trafic source
        //        worksheet.AddMergedRegion(new Region(startRow, A, startRow, C));
        //        cel = CreateCell(worksheet, startRow, A);
        //        cel.SetCellValue(marketSources[i]);

        //        for (int j = 3; j <= 7; j++)
        //        {
        //            formula = "";
        //            columnLetter = (char)(ASCIIOffset + j + 1);
        //            for (int k = 0; k < housingSpecialists.Count; k++)
        //            {
        //                element = string.Format("{0}!{1}{2}", housingSpecialists[k].Replace(' ', '_'), columnLetter, startRow + 1);
        //                if (formula == "")
        //                    formula = element;
        //                else
        //                    formula += ("+" + element);
        //            }
        //            cel = CreateCell(worksheet, startRow, j);
        //            HighlightCell(cel);
        //            cel.SetCellFormula(formula);
        //        }

        //        // Next row
        //        startRow++;
        //    }

        //    return startRow; // Return the row which holds the totals
        //}

        //private void HighlightCell(HSSFCell cel)
        //{
        //    HSSFCellStyle style1 = workbook.CreateCellStyle();
        //    style1.SetFont(StandardFont);

        //    //fill background
        //    style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LIME.index;
        //    style1.FillPattern = HSSFCellStyle.SOLID_FOREGROUND;
        //    style1.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.LIGHT_GREEN.index;

        //    // Style the cell with borders all around.
        //    style1.BorderBottom = HSSFCellStyle.BORDER_THIN;
        //    style1.BottomBorderColor = HSSFColor.BLACK.index;
        //    style1.BorderLeft = HSSFCellStyle.BORDER_THIN;
        //    style1.LeftBorderColor = HSSFColor.BLACK.index;
        //    style1.BorderRight = HSSFCellStyle.BORDER_THIN;
        //    style1.RightBorderColor = HSSFColor.BLACK.index;
        //    //style.BorderTop = HSSFCellStyle.BORDER_MEDIUM_DASHED;
        //    //style.TopBorderColor = HSSFColor.ORANGE.index;   

        //    cel.CellStyle = style1;
        //}

        //private void CreateStandardFonts()
        //{
        //    // Set standard font
        //    StandardFont = workbook.CreateFont();
        //    StandardFont.FontHeightInPoints = 12;
        //    StandardFont.FontName = "Arial";

        //    // Set standard font italic
        //    StandardFontItalic = workbook.CreateFont();
        //    StandardFontItalic.FontHeightInPoints = 12;
        //    StandardFontItalic.FontName = "Arial";
        //    StandardFontItalic.IsItalic = true;

        //    // Set standard font bold
        //    StandardFontBold = workbook.CreateFont();
        //    StandardFontBold.FontHeightInPoints = 12;
        //    StandardFontBold.FontName = "Arial";
        //    StandardFontBold.Boldweight = 1000;


        //    // Set standard font bold italic
        //    StandardFontBoldItalic = workbook.CreateFont();
        //    StandardFontBoldItalic.FontHeightInPoints = 12;
        //    StandardFontBoldItalic.FontName = "Arial";
        //    StandardFontBoldItalic.IsItalic = true;
        //    StandardFontBoldItalic.Boldweight = 30;

        //    // Set standard font highlighted

        //}

        //private HSSFCell CreateCell(HSSFSheet sheet, int startRow, int startCol)
        //{
        //    return CreateCell(sheet, startRow, startCol, false);
        //}

        //private HSSFCell CreateCell(HSSFSheet sheet, int startRow, int startCol, bool useBoldFace)
        //{
        //    HSSFCell cel;
        //    HSSFRow row;

        //    row = sheet.GetRow(startRow);
        //    if (row == null)
        //        row = sheet.CreateRow(startRow);

        //    cel = row.GetCell(startCol);
        //    if (cel == null)
        //    {
        //        cel = sheet.GetRow(startRow).CreateCell(startCol);
        //        cel.CellStyle.SetFont(StandardFont);
        //    }

        //    if (useBoldFace)
        //        cel.CellStyle.SetFont(StandardFontBold);

        //    return cel;
        //}

        //private HSSFSheet CloneSheet(string fromSheet, string toSheet)
        //{
        //    HSSFSheet sheet1 = workbook.CloneSheet(workbook.GetSheetIndex(fromSheet));
        //    workbook.SetSheetName(workbook.GetSheetIndex(sheet1), toSheet);
        //    return sheet1;
        //}
        #endregion Deprecated

    }
}
