/*
 * Collection of data classes which are used to hold individual records retrieved from back-end databases.
 * Author       :  Ha D. Doan
 * Created      : June 2010
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRM.Pivotal.IAC.Report.Excel.Data {

    /// <summary>Housing Specialist info</summary>
    /// <author>Ha D. Doan</author>
    /// <created>6/1/2010</created>
    internal class HousingSpecialistRecord {
        public long EmployeeId { get; set; }
        public string FullName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
    }

    /// <summary>Unqualified prospects</summary>
    /// <author>Ha D. Doan</author>
    /// <created>6/1/2010</created>
    internal class UnQualifiedProspectsRecord {
        public short WalkinsPriceCount { get; set; }
        public short WalkinsLocationCount { get; set; }
        public short WalkinsNeedsCount { get; set; }
        public short PhonePriceCount { get; set; }
        public short PhoneLocationCount { get; set; }
        public short PhoneNeedsCount { get; set; }
    }

    /// <summary>Walkin/Phone/Email counts by market source</summary>
    /// <author>Ha D. Doan</author>
    /// <created>6/1/2010</created>
    internal class TrafficByMarketSourceRecord {
        public string HousingSpecialist { get; set; }
        public string TrafficSource { get; set; } // Marketing Project Source
        public int WalkInCount { get; set; }
        public int PhoneTrafficCount { get; set; }
        public int PhoneSearchCount { get; set; }
        public int WalkInRoommateCount { get; set; }
        public int PhoneRoommateCount { get; set; }
        public int EmailTrafficCount { get; set; }
        public int EmailSearchCount { get; set; }
    }

    /// <summary>Daily counts by Walkin, phone, and email</summary>
    /// <author>Ha D. Doan</author>
    /// <created>6/1/2010</created>
    internal class TrafficDailyCountRecord {
        public string Category { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public short WeekDayNumber { get; set; }
        public string WeekDay { get; set; }
        public int Count { get; set; }        
    }

    /// <summary>Appointment conversion by First and and first walkin housing specialist</summary>
    /// <author>Ha D. Doan</author>
    /// <created>6/1/2010</created>
    internal class AppointmentConversionRecord {
        public string ApptConversionSpecialist { get; set; }
        public string FirstHousingSpecialist { get; set; }
        public int Count { get; set; }      
    }


    /// <summary>Lease count by Property by Housing Specialists</summary>
    /// <author>Ha D. Doan</author>
    /// <created>6/1/2010</created>
    internal class LeasesByPropertyRecord {
        public string Property { get; set; }
        public int DanCount { get; set; }
        public int BrianCount { get; set; }      
        public int MaryCount { get; set; }      
        public int ChristineCount { get; set; }
        public int TerriCount { get; set; }      
    }



}
