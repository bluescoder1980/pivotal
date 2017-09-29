using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TICPivotalQADataObjects;

namespace TICPivotalQAController
{
    public interface IQAController
    {
        UserObj GetUserForLogin(string userLogin);
        ScheduledInspectionWrapper GetScheduledInspectionsForUser(string userLogin, CompanyType type);
        Inspection LoadExistingInspection(string inspectionId, LoadActionsForInspection action, string lastSavedByUserId);
        InspectionListWrapper GetInspectionList(string userLogin, string status, CompanyType type);
        InspectionStatus GetInspectionStatus(string inspectionId, string userLogin);

    }

    // Factory to get instance of interface
    public class QAControllerFactory
    {
        public static IQAController GetQAController()
        {
            return new QAController();
        }

        public static IQAController GetQAController(string pivSysName)
        {
            return new QAController(pivSysName);
        }
    }
}
