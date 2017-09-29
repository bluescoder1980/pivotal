using System;
using System.Diagnostics;

namespace CRM.Pivotal.IAC.Utility {
    /// <summary>Log messages to Windows Events</summary>
    /// <remarks>Originally developed for DebtTraxEBlast</remarks>    
    /// <author>Ha D. Doan</author>
    class Log {        
        /// <summary>
        /// Log event messages to Windows Events log. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="appName"></param>
        /// <param name="logEvent"></param>
        /// <param name="logType"></param>
        public static void LogAppEvent(string source, string appName, string logEvent,
                                        EventLogEntryType logType) {
            if (!EventLog.SourceExists(source))
                EventLog.CreateEventSource(source, appName);
            
            EventLog.WriteEntry(source, logEvent, logType, CreateEventID());            
        }

        /// <summary>
        /// Create an event log ID. This ID is weak since it 
        /// cannot always guarantee that it is unique but it
        /// is good enough for this specific purpose.
        /// </summary>
        /// <returns></returns>
        private static int CreateEventID() {
            return  DateTime.Now.Year +
                    DateTime.Now.Month +
                    DateTime.Now.Day +
                    (DateTime.Now.Hour * 12) +
                    DateTime.Now.Minute;                                 
        }
    }
}
