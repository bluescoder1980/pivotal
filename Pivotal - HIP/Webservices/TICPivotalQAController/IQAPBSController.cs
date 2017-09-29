using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TICPivotalQADataObjects;

namespace TICPivotalQAController
{
    public interface IQAPBSController
    {
        string InsertNewInspectionIntoPivotal(string projectId,
            string phaseNumber, string inspTypeId, string scope, string createdById, 
            string inspectorId, string[] scopeItems);
        Inspection UpdateExistingInspection(Inspection inspectionData, ActionForInspection action);
        void DeleteExistingInspection(string inspectionId);

    }

    // Factory to get instance of interface
    public class QAPBSControllerFactory
    {
        public static QAController GetQAPBSController(string pivSysName)
        {
            return new QAController(pivSysName);
        }
    }

}
