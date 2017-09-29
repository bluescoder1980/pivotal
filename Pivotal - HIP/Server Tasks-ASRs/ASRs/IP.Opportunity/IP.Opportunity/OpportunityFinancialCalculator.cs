using System;
using System.Globalization;
using System.Text;
using System.Xml;

using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;

using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{

    /// <summary>
    /// This module provides all the business rules for running and saving from the
    /// financial calculator.
    /// </summary>
    /// <history>
    /// Revision #  Date        Author  Description
    /// 3.8.0.0     5/12/2006   DYin    Converted to .Net C# code.
    /// </history>
    public class OpportunityFinancialCalculator : IRFormScript
    {
        // User Defined Types
        /// <summary>
        /// Loan Profile Information
        /// </summary>
        public struct LoanProfileInfo
        {
            /// <summary>
            /// Loan Profile Id
            /// </summary>
            public object LoanProfileId;
            /// <summary>
            /// Loan Program Id
            /// </summary>
            public object LoanProgramId;
            /// <summary>
            /// Name
            /// </summary>
            public string Name;
            /// <summary>
            /// Loan 1 Id
            /// </summary>
            public object Loan1Id;
            /// <summary>
            /// Loan 1 Amount
            /// </summary>
            public decimal Loan1Amount;
            /// <summary>
            /// Loan 1 Interest Rate
            /// </summary>
            public double Loan1InterestRate;
            /// <summary>
            /// Loan 2 Id
            /// </summary>
            public object Loan2Id;
            /// <summary>
            /// Loan 2 Amount
            /// </summary>
            public decimal Loan2Amount;
            /// <summary>
            /// Loan 2 Interest Rate
            /// </summary>
            public double Loan2InterestRate;
            /// <summary>
            /// Monthly Income
            /// </summary>
            public decimal MonthlyIncome;
            /// <summary>
            /// Monthly Debt
            /// </summary>
            public decimal MonthlyDebt;
            /// <summary>
            /// Total Price
            /// </summary>
            public decimal TotalPrice;
            /// <summary>
            /// Down Payment
            /// </summary>
            public decimal DownPayment;
            /// <summary>
            /// Down Payment Percent
            /// </summary>
            public double DownPaymentPercent;
            /// <summary>
            /// Post Contract Adjustment
            /// </summary>
            public decimal PostContractAdjustment;
            /// <summary>
            /// Payment
            /// </summary>
            public decimal Payment;
            /// <summary>
            /// Region Id
            /// </summary>
            public object RegionId;
            /// <summary>
            /// Division Id
            /// </summary>
            public object DivisionId;
            /// <summary>
            /// Neighborhood Id
            /// </summary>
            public object NeighborhoodId;
            /// <summary>
            /// Xml
            /// </summary>
            public string Xml;
            /// <summary>
            /// Participation Fede
            /// </summary>
            public bool ParticipationFee;

            /// <summary>
            /// Default constructor for Loan Profile Info
            /// </summary>
            /// <param name="obj">Object</param>
            public LoanProfileInfo(object obj)
            {
                LoanProfileId = DBNull.Value; 
                LoanProgramId = DBNull.Value; 
                Name = String.Empty;
                Loan1Id = DBNull.Value;
                Loan1Amount = 0;
                Loan1InterestRate = 0;
                Loan2Id = DBNull.Value; 
                Loan2Amount = 0;
                Loan2InterestRate = 0;
                MonthlyIncome = 0;
                MonthlyDebt = 0;
                TotalPrice = 0;
                DownPayment = 0;
                DownPaymentPercent = 0;
                PostContractAdjustment = 0;
                Payment = 0;
                RegionId = DBNull.Value;
                DivisionId = 0;
                NeighborhoodId = DBNull.Value; ;
                Xml = String.Empty;
                ParticipationFee = false;
            }
        }

        private ILangDict grldtLangDict;

        /// <summary>
        /// Language Dictionary
        /// </summary>
        protected ILangDict LangDict
        {
            get { return grldtLangDict; }
            set { grldtLangDict = value; }
        }

        private IRSystem7 mrsysSystem;

        /// <summary>
        /// System
        /// </summary>
        protected IRSystem7 RSysSystem
        {
            get { return mrsysSystem; }
            set { mrsysSystem = value; }
        }

        // Constants
        private const decimal dblDEFAULT_LOAN_AMT = 100000;

        /// <summary>
        /// Default Loan Amount
        /// </summary>
        protected decimal DEFAULT_LOAN_AMT
        {
            get { return dblDEFAULT_LOAN_AMT; }
        }

        private OpportunityFinancialCalculator.LoanProfileInfo m_LoanProfile = new OpportunityFinancialCalculator.LoanProfileInfo(null);

        /// <summary>
        /// Loan Profile
        /// </summary>
        protected OpportunityFinancialCalculator.LoanProfileInfo LoanProfile
        {
            get { return m_LoanProfile; }
            set { m_LoanProfile = value; }
        }

        /// <summary>
        /// Add form data
        /// </summary>
        /// <param name="pForm">The IRForm object reference to the client IRForm object</param>
        /// <param name="Recordsets">Hold the reference for the current primary recordset and its all
        /// secondaries in the specified form.</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// IRFormScript_AddFormData - Return information to IRSystem</returns>
        /// <history>
        /// Revision#     Date            Author          Description
        /// 3.8.0.0       5/12/2006       DYin            Converted to .Net C# code.
        /// </history>
        public virtual object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                return pForm.DoAddFormData(Recordsets, ref ParameterList);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Delete Form Data
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="RecordId">The business object record Id</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        public virtual void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                pForm.DoDeleteFormData(RecordId, ref ParameterList);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        // Name : Execute
        /// <summary>
        /// Execute a specified method
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="MethodName">The method name to be executed</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// ParameterList - Return executed result</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {
            try
            {
                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;

                object[] parameterArray = ocmsTransitPointParams.GetUserDefinedParameterArray();

                switch (MethodName)
                {
                    case modOpportunity.strmGET_XML_FOR_FC:
                        // Parameters: (0) - LoanProfileId, (1) OpportunityId, (2) NBHDId
                        parameterArray[0] = LoadFinancialCalculatorXml(this.ObjectToId(parameterArray[0]), this.ObjectToId(parameterArray[1]), 
                            this.ObjectToId(parameterArray[2]));
                        break;
                    case modOpportunity.strmSAVE_LOAN_PROFILE:
                        // Parameters: (0) - LoanProfileId, (1) OpportunityId, (2) LoanProfileXML
                        parameterArray[0] = SaveLoanProfile(this.ObjectToId(parameterArray[0]), this.ObjectToId(parameterArray[1]), 
                            TypeConvert.ToString(parameterArray[2]));

                        break;
                    default:
                        string message = MethodName + TypeConvert.ToString(LangDict.GetText(modOpportunity.strdINVALID_METHOD));
                        parameterArray = new object[] { message };
                        throw new PivotalApplicationException(message, modOpportunity.glngERR_METHOD_NOT_DEFINED);
                }
                // Add the returned values into transit point parameter list
                ParameterList = ocmsTransitPointParams.SetUserDefinedParameterArray(parameterArray);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="RecordId">The Opportunity Id</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns> The form data</returns>
        /// <history>
        /// Revision#     Date        Author       Description
        /// 3.8.0.0       5/12/2006   DYin         Converted to .Net C# code.
        /// </history>
        public virtual object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                return pForm.DoLoadFormData(RecordId, ref ParameterList);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function load a new Opportunity Loan Profile record
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// IRFormScript_NewFormData   - Returned information</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        public virtual object NewFormData(IRForm pForm, ref object ParameterList)
        {
            try
            {
                object vntRecordsets = pForm.DoNewFormData(ref ParameterList);
                object[] recordsetArray = (object[]) vntRecordsets;

                Recordset rstOppFinCalc = (Recordset) recordsetArray[0];

                // Set Default Fields value
                TransitionPointParameter objParam = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;
                if (objParam.HasValidParameters() == false)
                {
                    objParam.Construct();
                }
                else
                {
                    objParam.SetDefaultFields(rstOppFinCalc);
                    objParam.WarningMessage = string.Empty;
                    ParameterList = objParam.ParameterList;
                }
                return vntRecordsets;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function create a new secondary record for the specified secondary
        /// </summary>
        /// <param name="pForm">The IRForm object reference to the client IRForm object</param>
        /// <param name="SecondaryName">The secondary name (the Segment name to hold a secondary)</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <param name="Recordset">Hold the reference for the secondary</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        public virtual void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset
            Recordset)
        {
            try
            {
                pForm.DoNewSecondaryData(SecondaryName, ref ParameterList, Recordset);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function updates the Opportunity Loan Profile plan
        /// </summary>
        /// <param name="pForm">The IRForm object reference to the client IRForm object</param>
        /// <param name="Recordsets">Hold the reference for the current primary recordset and its all
        /// secondaries in the specified form.</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date        Author          Description
        /// 3.8.0.0       5/12/2006   DYin            Converted to .Net C# code.
        /// </history>
        public virtual void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                pForm.DoSaveFormData(Recordsets, ref ParameterList);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This subroutine sets the Active Client System.
        /// </summary>
        /// <param name="pSystem">Active Client System Name</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        public virtual void SetSystem(RSystem pSystem)
        {
            try
            {
                RSysSystem = (IRSystem7) pSystem;
                LangDict = RSysSystem.GetLDGroup(modOpportunity.strt_OPPORTUNITY);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Add an XML attribute to an XML node
        /// </summary>
        /// <param name="pairName">The name of the attribute</param>
        /// <param name="pairValue">The value of the attribute</param>
        /// <returns>Attribute string in the format "Attribute='Name' "</returns>
        /// <history>
        /// Revision#     Date        Author       Description
        /// 3.8.0.0       5/12/2006   DYin         Converted to .Net C# code.
        /// </history>
        protected virtual string SetValuePairsString(string pairName, object pairValue)
        {
            string SetValuePairsString = String.Empty;
            try
            {
                string valueText = TypeConvert.ToString(pairValue);
                if (valueText.Length > 0)
                    SetValuePairsString = pairName.Trim() + "=" + (char)(34) + (Opportunity.EncodeXML(valueText)) + (char)(34) + new String(' ', 1);
                else
                    SetValuePairsString = pairName.Trim() + "=" + (char)(34) + (char)(34) + new String(' ',1);
                return SetValuePairsString;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Add an XML attribute to an XML node evaluated as a boolean
        /// </summary>
        /// <param name="pairName">The name of the attribute</param>
        /// <param name="pairValue">The expression to be evaluated</param>
        /// <returns>Attribute string in the format "Attribute='Yes' " OR "Attribute='No' "</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetValuePairsBool(string pairName, object pairValue)
        {
            try
            {
                if (TypeConvert.ToBoolean(pairValue))
                {
                    return pairName.Trim() + "=" + (char)(34) + TypeConvert.ToString(LangDict.GetText(modOpportunity.strdYES)) + (char)(34) + new String(' ', 1);
                }
                else
                {
                    return pairName.Trim() + "=" + (char)(34) + TypeConvert.ToString(LangDict.GetText(modOpportunity.strdNO)) + (char)(34) + new String(' ', 1);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This service is called when the financer component requires a new list of loan programs to be downloaded
        /// </summary>
        /// <param name="loanProfileId">LoanProfile Id</param>
        /// <param name="opportunityId">Opportunity</param>
        /// <param name="neighborhoodId">Neighborhood Id</param>
        /// <history>
        /// Revision#     Date        Author          Description
        /// 3.8.0.0       5/12/2006   DYin            Converted to .Net C# code.
        /// </history>
        public virtual string GetLoanProgramsXml(object loanProfileId, object opportunityId, object neighborhoodId)
        {
            try
            {
                // Check for Loan_Profile_Id and Opportunity_Id
                string strTextXML = string.Empty;
                if (loanProfileId != DBNull.Value)
                {
                    strTextXML = LoadLoanProfile(loanProfileId);
                }
                else
                {
                    if (neighborhoodId != DBNull.Value)
                    {
                        strTextXML = SetLoanProgramsForNeighborhood(neighborhoodId, opportunityId);
                    }
                    else
                    {
                        // Get Employee Id
                        object employeeId = RSysSystem.UserProfile.EmployeeId;
                        // Run for all neighborhoods
                        strTextXML = SetLoanProgramsForEmployee(employeeId);
                    }
                }
                return strTextXML;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Set Loan Program for Empleoyees
        /// </summary>
        /// <param name="employeeId">Employee Idc</param>
        /// <returns>Xml text with Program for the employee</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetLoanProgramsForEmployee(object employeeId)
        {
            try
            {
                StringBuilder xmlBuilder = new StringBuilder();
                xmlBuilder.Append("<LoanPrograms>");
                xmlBuilder.Append(SetRegions(employeeId));
                // strXML = strXML & SetOpp ' Include if the Save button is to be displayed
                xmlBuilder.Append("</LoanPrograms>");
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sets the Regions XML information for the financial calculator
        /// </summary>
        /// <param name="employeeId">Employee Id</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// 5.9.0.0       3/20/2007   RY          Took out Neighborhood Phone.  Issue 63614.
        /// </history>
        protected virtual string SetRegions(object employeeId)
        {
            try
            {
                // region, division, and neighborhood selection by employee
                StringBuilder sqlBuilder = new StringBuilder();
                if (RSysSystem.UserInGroup(RSysSystem.CurrentUserId(), modOpportunity.gstrstyHB_ADMIN))
                {
                    sqlBuilder.Append("SELECT DISTINCT ");
                    sqlBuilder.Append("ISNULL(Region.Region_Name,'') as Region_Name, ");
                    //sqlBuilder.Append("ISNULL(Neighborhood.Phone,'') as Phone, ");
                    sqlBuilder.Append("Neighborhood.Region_Id AS RID ");
                    //sqlBuilder.Append("CAST(Neighborhood.Region_Id AS int) AS RID ");
                    sqlBuilder.Append("FROM Neighborhood INNER JOIN ");
                    sqlBuilder.Append("Division ON Neighborhood.Division_Id = Division.Division_Id INNER JOIN ");
                    sqlBuilder.Append("Employee ON Employee.Division_Id = Division.Division_Id FULL OUTER JOIN ");
                    sqlBuilder.Append("Region ON Neighborhood.Region_Id = Region.Region_Id ");
                    sqlBuilder.Append("WHERE (Employee.Employee_Id = ");
                    sqlBuilder.Append(RSysSystem.IdToString(employeeId));
                    sqlBuilder.Append(") ");
                    sqlBuilder.Append("AND (Neighborhood.Inactive is null or Neighborhood.Inactive = 0)");
                    sqlBuilder.Append("ORDER BY Region_Name ");

                }
                else
                {
                    sqlBuilder.Append("SELECT DISTINCT ");
                    sqlBuilder.Append("ISNULL(Region.Region_Name,'') as Region_Name, ");
                    sqlBuilder.Append("ISNULL(Neighborhood.Phone,'') as Phone, ");
                    sqlBuilder.Append("Neighborhood.Region_Id AS RID ");
                    //sqlBuilder.Append("CAST(Neighborhood.Region_Id AS int) AS RID ");
                    sqlBuilder.Append("FROM Neighborhood INNER JOIN ");
                    sqlBuilder.Append("NBHD_Phase ON Neighborhood.Neighborhood_Id = NBHD_Phase.Neighborhood_Id INNER JOIN ");
                    sqlBuilder.Append("Employee_NBHD ON NBHD_Phase.NBHD_Phase_Id = Employee_NBHD.NBHD_Phase_Id INNER JOIN ");
                    sqlBuilder.Append("Employee ON Employee_NBHD.Employee_Id = Employee.Employee_Id FULL OUTER JOIN ");
                    sqlBuilder.Append("Division ON Neighborhood.Division_Id = Division.Division_Id FULL OUTER JOIN ");
                    sqlBuilder.Append("Region ON Neighborhood.Region_Id = Region.Region_Id ");
                    sqlBuilder.Append("WHERE (Employee.Employee_Id = ");
                    sqlBuilder.Append(RSysSystem.IdToString(employeeId));
                    sqlBuilder.Append(") ");
                    sqlBuilder.Append("AND (Neighborhood.Inactive is null or Neighborhood.Inactive = 0)");
                    sqlBuilder.Append("ORDER BY Region_Name ");
                }
                // return results from database
                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rsSQLRecordset = dataAccess.GetRecordset(sqlBuilder.ToString());

                // Build XML
                StringBuilder xmlBuilder = new StringBuilder();
                if (rsSQLRecordset.RecordCount > 0)
                {
                    // records found, create a dataset on the rn_appointments table
                    rsSQLRecordset.MoveFirst();
                    while(!(rsSQLRecordset.EOF))
                    {
                        xmlBuilder.Append("<Region ID=");
                        xmlBuilder.Append((char)(34));
                        xmlBuilder.Append(this.IdToInteger(rsSQLRecordset.Fields["RID"].Value));
                        xmlBuilder.Append((char)(34));
                        xmlBuilder.Append(new String(' ', 1));
                        xmlBuilder.Append(SetValuePairsString("Name", TypeConvert.ToString(rsSQLRecordset.Fields[modOpportunity.strfREGION_NAME].Value)));
                        xmlBuilder.Append(new String(' ', 1));
                        //xmlBuilder.Append(SetValuePairsString("Phone", TypeConvert.ToString(rsSQLRecordset.Fields[modOpportunity.strfHOME_PHONE].Value)));
                        xmlBuilder.Append(">\r\n");
                        // do divisions
                        xmlBuilder.Append(SetDivisions(employeeId, rsSQLRecordset.Fields["RID"].Value));
                        // end this region
                        xmlBuilder.Append("</Region>");
                        rsSQLRecordset.MoveNext();
                    }
                }
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sets the Divisions XML information for the financial calculator by calling <see cref="SetNeighborhoods"/>
        /// </summary>
        /// <param name="employeeId">Employee Id</param>
        /// <param name="regionId">Region Id</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetDivisions(object employeeId, object regionId)
        {
            try
            {
                // region, division, and neighborhood selection by employee
                StringBuilder sqlBuilder = new StringBuilder();
                if (RSysSystem.UserInGroup(RSysSystem.CurrentUserId(), modOpportunity.gstrstyHB_ADMIN))
                {
                    sqlBuilder.Append("SELECT DISTINCT ");
                    sqlBuilder.Append("ISNULL(Division.Name,'') AS Division_Name, ");
                    sqlBuilder.Append("Neighborhood.Division_Id AS DID ");
                    //sqlBuilder.Append("CAST(Neighborhood.Division_Id AS int) AS DID ");
                    sqlBuilder.Append("FROM Neighborhood INNER JOIN ");
                    sqlBuilder.Append("Division ON Neighborhood.Division_Id = Division.Division_Id INNER JOIN ");
                    sqlBuilder.Append("Employee ON Employee.Division_Id = Division.Division_Id FULL OUTER JOIN ");
                    sqlBuilder.Append("Region ON Neighborhood.Region_Id = Region.Region_Id ");
                    sqlBuilder.Append("WHERE (Employee.Employee_Id = ");
                    sqlBuilder.Append(RSysSystem.IdToString(employeeId));
                    sqlBuilder.Append(") ");
                    sqlBuilder.Append("AND (Neighborhood.Region_Id = ");
                    sqlBuilder.Append(RSysSystem.IdToString(regionId));
                    sqlBuilder.Append(") ");
                    sqlBuilder.Append("AND (Neighborhood.Inactive is null or Neighborhood.Inactive = 0)");
                    sqlBuilder.Append("ORDER BY Division_Name ");
                }
                else
                {
                    sqlBuilder.Append("SELECT DISTINCT ");
                    // strSQL = strSQL &"ISNULL as Region_Name, "
                    // strSQL = strSQL &"Neighborhood.Region_Id AS RID "
                    sqlBuilder.Append("ISNULL(Division.Name,'') AS Division_Name, ");
                    sqlBuilder.Append("Neighborhood.Division_Id AS DID ");
                    //sqlBuilder.Append("CAST(Neighborhood.Division_Id AS int) AS DID ");
                    // strSQL = strSQL &"ISNULL AS Neighborhood_name, "
                    // strSQL = strSQL &"CAST(Neighborhood.Neighborhood_Id AS bigint) AS NID "
                    sqlBuilder.Append("FROM Neighborhood INNER JOIN ");
                    sqlBuilder.Append("NBHD_Phase ON Neighborhood.Neighborhood_Id = NBHD_Phase.Neighborhood_Id INNER JOIN ");
                    sqlBuilder.Append("Employee_NBHD ON NBHD_Phase.NBHD_Phase_Id = Employee_NBHD.NBHD_Phase_Id INNER JOIN ");
                    sqlBuilder.Append("Employee ON Employee_NBHD.Employee_Id = Employee.Employee_Id FULL OUTER JOIN ");
                    sqlBuilder.Append("Division ON Neighborhood.Division_Id = Division.Division_Id FULL OUTER JOIN ");
                    sqlBuilder.Append("Region ON Neighborhood.Region_Id = Region.Region_Id ");
                    sqlBuilder.Append("WHERE (Employee.Employee_Id = ");
                    sqlBuilder.Append(RSysSystem.IdToString(employeeId));
                    sqlBuilder.Append(") ");
                    sqlBuilder.Append("AND (Neighborhood.Region_Id = ");
                    sqlBuilder.Append(RSysSystem.IdToString(regionId));
                    sqlBuilder.Append(") ");
                    sqlBuilder.Append("ORDER BY Division_Name ");
                }
                // return results from database
                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rsSQLRecordset = dataAccess.GetRecordset(sqlBuilder.ToString());

                // Build XML
                StringBuilder xmlBuilder = new StringBuilder();
                if (rsSQLRecordset.RecordCount > 0)
                {
                    // records found, create a dataset on the rn_appointments table
                    rsSQLRecordset.MoveFirst();
                    while(!(rsSQLRecordset.EOF))
                    {
                        xmlBuilder.Append("<Division ID=");
                        xmlBuilder.Append((char)(34));
                        xmlBuilder.Append(this.IdToInteger(rsSQLRecordset.Fields[modOpportunity.strfDID].Value));
                        xmlBuilder.Append((char)(34));
                        xmlBuilder.Append(new String(' ', 1));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfNAME, TypeConvert.ToString(rsSQLRecordset.Fields[modOpportunity.strfDIVISION_NAME].Value)));
                        xmlBuilder.Append(">\r\n");
                        // do Neighborhoods
                        xmlBuilder.Append(SetNeighborhoods(employeeId, regionId, rsSQLRecordset.Fields[modOpportunity.strfDID].Value));
                        // end this division
                        xmlBuilder.Append("</Division>");
                        rsSQLRecordset.MoveNext();
                    }
                }
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sets the Neighborhoods XML information for the financial calculator by calling 
        /// <see cref="SetNeighborhoodFees"/> and <see cref="SetLoanPrograms"/>.
        /// </summary>
        /// <param name="employeeId">Employee Id</param>
        /// <param name="regionId">Region Id</param>
        /// <param name="divisionId">Division Id</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetNeighborhoods(object employeeId, object regionId, object divisionId)
        {
            try
            {
                // region, division, and neighborhood selection by employee
                StringBuilder sqlBuilder = new StringBuilder();
                if (RSysSystem.UserInGroup(RSysSystem.CurrentUserId(), modOpportunity.gstrstyHB_ADMIN))
                {
                    sqlBuilder.Append("SELECT DISTINCT ");
                    sqlBuilder.Append("ISNULL(Neighborhood.Name,'') AS Neighborhood_name, ");
                    sqlBuilder.Append("ISNULL(Neighborhood.Phone,'') AS Phone, ");
                    sqlBuilder.Append("Neighborhood.Neighborhood_Id AS NID ");
                    //sqlBuilder.Append("CAST(Neighborhood.Neighborhood_Id AS int) AS NID ");
                    sqlBuilder.Append("FROM Neighborhood INNER JOIN ");
                    sqlBuilder.Append("Division ON Neighborhood.Division_Id = Division.Division_Id INNER JOIN ");
                    sqlBuilder.Append("Employee ON Employee.Division_Id = Division.Division_Id LEFT OUTER JOIN ");
                    sqlBuilder.Append("Region ON Neighborhood.Region_Id = Region.Region_Id ");
                    sqlBuilder.Append("WHERE (Employee.Employee_Id = ");
                    sqlBuilder.Append(RSysSystem.IdToString(employeeId));
                    sqlBuilder.Append(") ");
                    sqlBuilder.Append("AND (Neighborhood.Region_Id = ");
                    sqlBuilder.Append(RSysSystem.IdToString(regionId));
                    sqlBuilder.Append(") ");
                    sqlBuilder.Append("AND (Neighborhood.Division_Id = ");
                    sqlBuilder.Append(RSysSystem.IdToString(divisionId));
                    sqlBuilder.Append(") ");
                    sqlBuilder.Append("AND (Neighborhood.Inactive is null or Neighborhood.Inactive = 0)");
                    sqlBuilder.Append("ORDER BY Neighborhood_name ");
                }
                else
                {
                    sqlBuilder.Append("SELECT DISTINCT ");
                    sqlBuilder.Append("ISNULL(Neighborhood.Name,'') AS Neighborhood_name, ");
                    sqlBuilder.Append("ISNULL(Neighborhood.Phone,'') AS Phone, ");
                    sqlBuilder.Append("Neighborhood.Neighborhood_Id AS NID ");
                    //sqlBuilder.Append("CAST(Neighborhood.Neighborhood_Id AS int) AS NID ");
                    sqlBuilder.Append("FROM Neighborhood INNER JOIN ");
                    sqlBuilder.Append("NBHD_Phase ON Neighborhood.Neighborhood_Id = NBHD_Phase.Neighborhood_Id INNER JOIN ");
                    sqlBuilder.Append("Employee_NBHD ON NBHD_Phase.NBHD_Phase_Id = Employee_NBHD.NBHD_Phase_Id INNER JOIN ");
                    sqlBuilder.Append("Employee ON Employee_NBHD.Employee_Id = Employee.Employee_Id LEFT OUTER JOIN ");
                    sqlBuilder.Append("Division ON Neighborhood.Division_Id = Division.Division_Id LEFT OUTER JOIN ");
                    sqlBuilder.Append("Region ON Neighborhood.Region_Id = Region.Region_Id ");
                    sqlBuilder.Append("WHERE (Employee.Employee_Id = ");
                    sqlBuilder.Append(RSysSystem.IdToString(employeeId));
                    sqlBuilder.Append(") ");
                    sqlBuilder.Append("AND (Neighborhood.Region_Id = ");
                    sqlBuilder.Append(RSysSystem.IdToString(regionId));
                    sqlBuilder.Append(") ");
                    sqlBuilder.Append("AND (Neighborhood.Division_Id = ");
                    sqlBuilder.Append(RSysSystem.IdToString(divisionId));
                    sqlBuilder.Append(") ");
                    sqlBuilder.Append("AND (Neighborhood.Inactive is null or Neighborhood.Inactive = 0)");
                    sqlBuilder.Append("ORDER BY Neighborhood_name ");
                }

                // return results from database
                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rsSQLRecordset = dataAccess.GetRecordset(sqlBuilder.ToString());

                // Build XML
                StringBuilder xmlBuilder = new StringBuilder();
                if (rsSQLRecordset.RecordCount >0)
                {
                    // records found, create a dataset on the rn_appointments table
                    rsSQLRecordset.MoveFirst();
                    while(!(rsSQLRecordset.EOF))
                    {
                        // BRS - Check to see if this is the default Market Level Neighborhood. If so, ignore it.
                        if ((TypeConvert.ToString(rsSQLRecordset.Fields[modOpportunity.strfNEIGHBORHOOD_NAME].Value)).ToUpper()
                            != (TypeConvert.ToString(RSysSystem.GetLDGroup(modOpportunity.strgDIVISION).GetText(modOpportunity.strlMARKET_LEVEL_NEIGHBORHOOD))).ToUpper())
                        {
                            xmlBuilder.Append("<Neighborhood ID= ");
                            xmlBuilder.Append((char)(34));
                            xmlBuilder.Append(this.IdToInteger(rsSQLRecordset.Fields["NID"].Value));
                            xmlBuilder.Append((char)(34));
                            xmlBuilder.Append(new String(' ', 1));
                            xmlBuilder.Append(SetValuePairsString("Name", TypeConvert.ToString(rsSQLRecordset.Fields[modOpportunity.strfNEIGHBORHOOD_NAME].Value)));
                            xmlBuilder.Append(new String(' ', 1));
                            xmlBuilder.Append(SetValuePairsString("Phone", TypeConvert.ToString(rsSQLRecordset.Fields[modOpportunity.strfHOME_PHONE].Value)));
                            xmlBuilder.Append(">\r\n");
                            // do neighborhood fees
                            xmlBuilder.Append(SetNeighborhoodFees(regionId, divisionId, rsSQLRecordset.Fields["NID"].Value));
                            // do neighborhood loan programs
                            xmlBuilder.Append(SetLoanPrograms(regionId, divisionId, rsSQLRecordset.Fields["NID"].Value));
                            // end this neighborhood
                            xmlBuilder.Append("</Neighborhood>");
                        }
                        rsSQLRecordset.MoveNext();
                    }
                }
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sets the Neighborhood fees XML information for the financial calculator
        /// </summary>
        /// <param name="regionId">Region Id</param>
        /// <param name="divisionId">Division Id</param>
        /// <param name="neighborhoodId">NeighborhoodId</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision# Date Author Description
        /// 3.8.0.0   5/12/2006  DYin  Converted to .Net C# code.
        /// </history>
        protected virtual string SetNeighborhoodFees(object regionId, object divisionId, object neighborhoodId)
        {
            try
            {
                
                StringBuilder sqlBuilder = new StringBuilder();

                // neighborhood fee selection by neighborhood
                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append("ISNULL(Name,'') AS Name, ");
                sqlBuilder.Append("ISNULL(Amount,0.0) AS Amount, ");
                sqlBuilder.Append("ISNULL(PctLoanAmount,0) AS PctLoanAmount, ");
                sqlBuilder.Append("ISNULL(PctSalePrice,0) AS PctSalePrice, ");
                sqlBuilder.Append("ISNULL(PWM,0) AS PWM, ");
                sqlBuilder.Append("ISNULL(PAC,0) AS PAC, ");
                sqlBuilder.Append("ISNULL(Impound,0) AS Impound, ");
                sqlBuilder.Append("ISNULL(Months,0) AS Months, ");
                sqlBuilder.Append("ISNULL(InsRelated,0) AS InsRelated, ");
                sqlBuilder.Append("ISNULL(TaxRelated,0) AS TaxRelated ");
                sqlBuilder.Append("FROM NBHD_Fees ");
                sqlBuilder.Append("WHERE (Neighborhood_Id = " + RSysSystem.IdToString(neighborhoodId) + ") ");
                sqlBuilder.Append("  AND (Inactive is null OR (Inactive = 0))");
                // strSQL = strSQL &"AND (Division_Id = " & CStr(DID) & ") "
                // strSQL = strSQL &"AND (Neighborhood_Id = " & CStr(NID) & ") "
                sqlBuilder.Append("ORDER BY Name ");

                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // return results from database
                Recordset rsSQLRecordset = dataAccess.GetRecordset(sqlBuilder.ToString());

                // Build XML
                StringBuilder xmlBuilder = new StringBuilder();
                if (rsSQLRecordset.RecordCount >0)
                {
                    // records found, create a dataset on the rn_appointments table
                    xmlBuilder.Append("<Fees>");
                    rsSQLRecordset.MoveFirst();
                    while(!(rsSQLRecordset.EOF))
                    {
                        // create one entry for each fee
                        xmlBuilder.Append("<Fee ");
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfNAME, rsSQLRecordset.Fields[modOpportunity.strfNAME].Value));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfAMOUNT, rsSQLRecordset.Fields[modOpportunity.strfAMOUNT].Value));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfPCT_LOAN_AMOUNT, TypeConvert.ToDouble(rsSQLRecordset.Fields[modOpportunity.strfPCT_LOAN_AMOUNT].Value)
                            / 100D));
                        // strXML = strXML & SetValuePairsString(strfPCTSALEPRICE, CStr(rsSQLRecordset.Fields(strfPCTSALEPRICE).Value
                        // / 100#))
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfPCTSALEAMOUNT, TypeConvert.ToDouble(rsSQLRecordset.Fields[modOpportunity.strfPCTSALEPRICE].Value)
                            / 100D));
                        xmlBuilder.Append(SetValuePairsBool(modOpportunity.strfPWM, rsSQLRecordset.Fields[modOpportunity.strfPWM].Value));
                        xmlBuilder.Append(SetValuePairsBool(modOpportunity.strfPAC, rsSQLRecordset.Fields[modOpportunity.strfPAC].Value));
                        xmlBuilder.Append(SetValuePairsBool(modOpportunity.strfIMPOUND, rsSQLRecordset.Fields[modOpportunity.strfIMPOUND].Value));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfMONTHS, rsSQLRecordset.Fields[modOpportunity.strfMONTHS].Value));
                        xmlBuilder.Append(SetValuePairsBool(modOpportunity.strfTAX_RELATED, rsSQLRecordset.Fields[modOpportunity.strfTAX_RELATED].Value));
                        xmlBuilder.Append(SetValuePairsBool(modOpportunity.strfINS_RELATED, rsSQLRecordset.Fields[modOpportunity.strfINS_RELATED].Value));
                        xmlBuilder.Append("/>\r\n");
                        rsSQLRecordset.MoveNext();
                    }
                    xmlBuilder.Append("</Fees>");
                }
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sets the Loan Program XML information for the financial calculator
        /// </summary>
        /// <param name="regionId">Region Id</param>
        /// <param name="divisionId">Division Id</param>
        /// <param name="neighborhoodId">NeighborhoodId</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetLoanPrograms(object regionId, object divisionId, object neighborhoodId)
        {
            try
            {
                StringBuilder sqlBuilder = new StringBuilder();
                // loan program selection by neighborhood
                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append("Loan_Program_Id, ");
                //sqlBuilder.Append("CAST(Loan_Program_Id AS int) AS Loan_Program_Id, ");
                sqlBuilder.Append("ISNULL(Name,'') AS Name, ");
                sqlBuilder.Append("ISNULL(Available,0) AS Available, ");
                sqlBuilder.Append("ISNULL(First_Date,'01/01/1900') AS First_Date, ");
                sqlBuilder.Append("ISNULL(Last_Date,'01/01/1900') AS Last_Date, ");
                sqlBuilder.Append("ISNULL(CCFixed,0) AS CCFixed, ");
                sqlBuilder.Append("ISNULL(CCPctApp,0) AS CCPctApp, ");
                sqlBuilder.Append("ISNULL(MinDwnPct,0) AS MinDwnPct, ");
                sqlBuilder.Append("First_Loan_Id, ");
                sqlBuilder.Append("Second_Loan_Id ");
                //sqlBuilder.Append("CAST(First_Loan_Id AS int) AS First_Loan_ID, ");
                //sqlBuilder.Append("CAST(Second_Loan_Id AS int) AS Second_Loan_Id ");
                sqlBuilder.Append("FROM Loan_Program ");
                sqlBuilder.Append("WHERE ");
                sqlBuilder.Append("(Neighborhood_Id = ");
                sqlBuilder.Append(RSysSystem.IdToString(neighborhoodId));
                sqlBuilder.Append(") ");
                sqlBuilder.Append("AND (Inactive is null or Inactive = 0)");

                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                // return results from database
                Recordset rsSQLRecordset = dataAccess.GetRecordset(sqlBuilder.ToString());

                // Build XML
                StringBuilder xmlBuilder = new StringBuilder();
                if (rsSQLRecordset.RecordCount >0)
                {
                    // records found, create a dataset on the rn_appointments table
                    rsSQLRecordset.MoveFirst();
                    while(!(rsSQLRecordset.EOF))
                    {
                        xmlBuilder.Append("<LoanProgram ");
                        xmlBuilder.Append(SetValuePairsString("ID", this.IdToInteger(rsSQLRecordset.Fields[modOpportunity.strfLOAN_PROGRAM_ID].Value)));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfNAME, rsSQLRecordset.Fields[modOpportunity.strfNAME].Value));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfEND_DATE, rsSQLRecordset.Fields[modOpportunity.strfLAST_DATE].Value));
                        xmlBuilder.Append(SetValuePairsBool(modOpportunity.strlAVAILABLE, rsSQLRecordset.Fields[modOpportunity.strfAVAILABLE].Value));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strlCLOSING_COST_FIXED, rsSQLRecordset.Fields[modOpportunity.strfCC_FIXED].Value));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strlCLOSING_COST_PCT_APPRAISED, rsSQLRecordset.Fields[modOpportunity.strfCC_PCT_APP].Value));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strlMINIMUM_DOWN_PERCENT, rsSQLRecordset.Fields[modOpportunity.strfMIN_DWN_PCT].Value));
                        xmlBuilder.Append(">\r\n");
                        // process the first loan
                        if (!(Convert.IsDBNull(rsSQLRecordset.Fields[modOpportunity.strfFIRST_LOAN_ID].Value)))
                        {
                            xmlBuilder.Append(SetLoans(rsSQLRecordset.Fields[modOpportunity.strfFIRST_LOAN_ID].Value, neighborhoodId));
                        }
                        // process the second loan
                        if (!(Convert.IsDBNull(rsSQLRecordset.Fields[modOpportunity.strfSECOND_LOAN_ID].Value)))
                        {
                            xmlBuilder.Append(SetLoans(rsSQLRecordset.Fields[modOpportunity.strfSECOND_LOAN_ID].Value, neighborhoodId));
                        }
                        // end this loan program
                        xmlBuilder.Append("</LoanProgram>");
                        rsSQLRecordset.MoveNext();
                    }
                }
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sets the Loan XML information for the financial calculator by calling <see cref="SetLoanFees"/>
        /// </summary>
        /// <param name="loanId">Loan Id</param>
        /// <param name="neighborhoodId">Neighborhood Id</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision# Date Author Description
        /// 3.8.0.0   5/12/2006  DYin  Converted to .Net C# code.
        /// </history>
        protected virtual string SetLoans(object loanId, object neighborhoodId)
        {
            try
            {
                StringBuilder sqlBuilder = new StringBuilder();

                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append("Loan_Id, ");
                //sqlBuilder.Append("CAST(Loan_Id as int) AS Loan_Id, ");
                sqlBuilder.Append("ISNULL(Loan_Name,'') AS Loan_Name, ");
                sqlBuilder.Append("ISNULL(Type,'') AS Type, ");
                sqlBuilder.Append("ISNULL(Interest_Rate,0) AS Interest_Rate, ");
                sqlBuilder.Append("ISNULL(Term,0) AS Term, ");
                sqlBuilder.Append("ISNULL(Periods,0) AS Periods, ");
                sqlBuilder.Append("ISNULL(Call_After,0) AS Call_After, ");
                sqlBuilder.Append("ISNULL(Max_Loan_Amount,0) AS Max_Loan_Amount, ");
                sqlBuilder.Append("ISNULL(CCPctLoan,0) AS CCPctLoan, ");
                sqlBuilder.Append("ISNULL(Index_Name,'') AS Index_Name, ");
                sqlBuilder.Append("ISNULL(Index_Rate,0) AS Index_Rate, ");
                sqlBuilder.Append("ISNULL(Margin_Rate,0) AS Margin_Rate, ");
                sqlBuilder.Append("ISNULL(PctSalePrice,0) AS PctSalePrice, ");
                sqlBuilder.Append("ISNUll(First_Adj_Periods,0) AS First_Adj_Periods, ");
                sqlBuilder.Append("ISNULL(Life_Rate_Cap,0) AS Life_Rate_Cap, ");
                sqlBuilder.Append("ISNULL(Life_Rate_Floor,0) AS Life_Rate_Floor, ");
                sqlBuilder.Append("ISNULL(Adj_Periods,0) AS Adj_Periods, ");
                sqlBuilder.Append("ISNULL(Adj_Rate_Cap,0) AS Adj_Rate_Cap, ");
                // BRS 6/01/2005 - start
                sqlBuilder.Append("ISNULL(Adj_Rate_Floor,0) AS Adj_Rate_Floor, ");
                sqlBuilder.Append("ISNULL(TopRatio, 100) AS TopRatio, ");
                sqlBuilder.Append("ISNULL(BottomRatio, 100) AS BottomRatio, ");
                sqlBuilder.Append("ISNULL(BalloonUsed, 0) AS BalloonUsed, ");
                sqlBuilder.Append("ISNULL(BalloonTerm, 0) AS BalloonTerm, ");
                sqlBuilder.Append("ISNULL(BuydownUsed, 0) AS BuydownUsed, ");
                sqlBuilder.Append("ISNULL(BuyDownRate1, 0) AS BuyDownRate1, ");
                sqlBuilder.Append("ISNULL(BuyDownTerm1, 0) AS BuyDownTerm1, ");
                sqlBuilder.Append("ISNULL(BuyDownRate2, 0) AS BuyDownRate2, ");
                sqlBuilder.Append("ISNULL(BuyDownTerm2, 0) AS BuyDownTerm2, ");
                sqlBuilder.Append("ISNULL(BuyDownRate3, 0) AS BuyDownRate3, ");
                sqlBuilder.Append("ISNULL(BuyDownTerm3, 0) AS BuyDownTerm3, ");
                sqlBuilder.Append("ISNULL(PctSalePrice, 0) AS PctSalePrice, ");
                sqlBuilder.Append("ISNULL(InterestOnly, 0) AS InterestOnly, ");
                // BRS 6/01/2005 - end
                // RY 13/6/2005 - start
                sqlBuilder.Append("ISNULL(IntOnlyTermsInYear, 0) As IntOnlyTermsInYear, ");
                sqlBuilder.Append("ISNULL(Prepaid_Int_Num_of_Days, 0) As Prepaid_Int_Num_of_Days, ");
                sqlBuilder.Append("ISNULL(AdjStartingRate, 0) As AdjStartingRate, ");
                sqlBuilder.Append("ISNULL(Round_To_Nearest_50, 0) As Round_To_Nearest_50, ");
                sqlBuilder.Append("ISNULL(PMI, 0) As PMI, ");
                sqlBuilder.Append("ISNULL(MIP, 0) As MIP, ");
                sqlBuilder.Append("ISNULL(VA_Funding, 0) As VA_Funding, ");
                // RY 13/6/2005 - end
                sqlBuilder.Append("ISNULL(Adj_Rate_Floor,0) AS Adj_Rate_Floor ");
                sqlBuilder.Append("FROM dbo.Loan ");
                sqlBuilder.Append("WHERE");
                sqlBuilder.Append("(Loan_Id = " + RSysSystem.IdToString(loanId) + ") ");

                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // return results from database
                Recordset rsSQLRecordset = dataAccess.GetRecordset(sqlBuilder.ToString());

                // Build XML
                StringBuilder xmlBuilder = new StringBuilder();
                if (rsSQLRecordset.RecordCount >0)
                {
                    // records found, create a dataset on the rn_appointments table
                    rsSQLRecordset.MoveFirst();
                    while(!(rsSQLRecordset.EOF))
                    {
                        xmlBuilder.Append("<Loan ");
                        xmlBuilder.Append(SetValuePairsString("ID", this.IdToInteger(rsSQLRecordset.Fields[modOpportunity.strfLOAN_ID].Value)));
                        xmlBuilder.Append(SetValuePairsString("Name", rsSQLRecordset.Fields[modOpportunity.strfLOAN_NAME].Value));
                        xmlBuilder.Append(SetValuePairsString("Type", rsSQLRecordset.Fields[modOpportunity.strfTYPE].Value));
                        xmlBuilder.Append(SetValuePairsString("Rate", rsSQLRecordset.Fields[modOpportunity.strfINTEREST_RATE].Value));
                        xmlBuilder.Append(SetValuePairsString("Term", rsSQLRecordset.Fields[modOpportunity.strfTERM].Value));
                        xmlBuilder.Append(SetValuePairsString("PeriodsPerYear", rsSQLRecordset.Fields[modOpportunity.strfPERIODS].Value));
                        xmlBuilder.Append(SetValuePairsString("RepricePeriods", ""));
                        xmlBuilder.Append(SetValuePairsString("PayoffPeriods", ""));
                        xmlBuilder.Append(SetValuePairsString("MaximumLoanAmount", rsSQLRecordset.Fields[modOpportunity.strfMAX_LOAN_AMOUNT].Value));
                        xmlBuilder.Append(SetValuePairsString("ClosingCostPctLoanAmt", rsSQLRecordset.Fields[modOpportunity.strfCCPCTLOAN].Value));
                        xmlBuilder.Append(SetValuePairsString("Index", rsSQLRecordset.Fields[modOpportunity.strfINDEX_NAME].Value));
                        xmlBuilder.Append(SetValuePairsString("IndexRate", rsSQLRecordset.Fields[modOpportunity.strfINDEX_RATE].Value));
                        xmlBuilder.Append(SetValuePairsString("MarginRate", rsSQLRecordset.Fields[modOpportunity.strfMARGIN_RATE].Value));
                        if (TypeConvert.ToString(rsSQLRecordset.Fields[modOpportunity.strfTYPE].Value) == "A")
                        {
                            xmlBuilder.Append(SetValuePairsString("TeaserRate", TypeConvert.ToInt32(rsSQLRecordset.Fields[modOpportunity.strfINDEX_RATE].Value)
                                + TypeConvert.ToInt32(rsSQLRecordset.Fields[modOpportunity.strfMARGIN_RATE].Value)));
                        }
                        else
                        {
                            xmlBuilder.Append(SetValuePairsString("TeaserRate", ""));
                        }
                        xmlBuilder.Append(SetValuePairsString("TeaserTerm", rsSQLRecordset.Fields[modOpportunity.strfFIRST_ADJ_PERIODS].Value));
                        xmlBuilder.Append(SetValuePairsString("RateCap", rsSQLRecordset.Fields[modOpportunity.strfLIFE_RATE_CAP].Value));
                        xmlBuilder.Append(SetValuePairsString("RateFloor", rsSQLRecordset.Fields[modOpportunity.strfLIFE_RATE_FLOOR].Value));
                        xmlBuilder.Append(SetValuePairsString("AdjustPeriods", rsSQLRecordset.Fields[modOpportunity.strfADJ_PERIODS].Value));
                        xmlBuilder.Append(SetValuePairsString("AdjustRateCap", rsSQLRecordset.Fields[modOpportunity.strfADJ_RATE_CAP].Value));
                        // BRS 6/01/2005 - start
                        xmlBuilder.Append(SetValuePairsString("AdjustRateFloor", rsSQLRecordset.Fields[modOpportunity.strfADJ_RATE_FLOOR].Value));
                        xmlBuilder.Append(SetValuePairsString("Top", rsSQLRecordset.Fields[modOpportunity.strfTOPRATIO].Value));
                        xmlBuilder.Append(SetValuePairsString("Bottom", rsSQLRecordset.Fields[modOpportunity.strfBOTTOMRATIO].Value));
                        xmlBuilder.Append(SetValuePairsString("BLU", rsSQLRecordset.Fields[modOpportunity.strfBALLOONUSED].Value));
                        xmlBuilder.Append(SetValuePairsString("BLT", rsSQLRecordset.Fields[modOpportunity.strfBALLOONTERM].Value));
                        xmlBuilder.Append(SetValuePairsString("BDU", rsSQLRecordset.Fields[modOpportunity.strfBUYDOWNUSED].Value));
                        xmlBuilder.Append(SetValuePairsString("BDR1", rsSQLRecordset.Fields[modOpportunity.strfBUYDOWNRATE1].Value));
                        xmlBuilder.Append(SetValuePairsString("BDT1", rsSQLRecordset.Fields[modOpportunity.strfBUYDOWNTERM1].Value));
                        xmlBuilder.Append(SetValuePairsString("BDR2", rsSQLRecordset.Fields[modOpportunity.strfBUYDOWNRATE2].Value));
                        xmlBuilder.Append(SetValuePairsString("BDT2", rsSQLRecordset.Fields[modOpportunity.strfBUYDOWNTERM2].Value));
                        xmlBuilder.Append(SetValuePairsString("BDR3", rsSQLRecordset.Fields[modOpportunity.strfBUYDOWNRATE3].Value));
                        xmlBuilder.Append(SetValuePairsString("BDT3", rsSQLRecordset.Fields[modOpportunity.strfBUYDOWNTERM3].Value));
                        xmlBuilder.Append(SetValuePairsString("PctSalePrice", rsSQLRecordset.Fields[modOpportunity.strfPCTSALEPRICE].Value));
                        xmlBuilder.Append(SetValuePairsString("InterestOnly", rsSQLRecordset.Fields[modOpportunity.strfINTERESTONLY].Value));
                        // BRS 6/01/2005 - end
                        // RY 13/6/2005 - start
                        xmlBuilder.Append(SetValuePairsString("InterestOnlyTerm ", rsSQLRecordset.Fields[modOpportunity.strfINT_ONLY_TERMS_IN_YEAR].Value));
                        xmlBuilder.Append(SetValuePairsString("PPID", rsSQLRecordset.Fields[modOpportunity.strfPREPAID_INT_NUM_OF_DAYS].Value));
                        xmlBuilder.Append(SetValuePairsString("Start", rsSQLRecordset.Fields[modOpportunity.strfADJSTARTINGRATE].Value));
                        xmlBuilder.Append(SetValuePairsBool("Rnd50", rsSQLRecordset.Fields[modOpportunity.strfROUND_TO_NEAREST_50].Value));
                        xmlBuilder.Append(SetValuePairsBool("AgPMI", rsSQLRecordset.Fields[modOpportunity.strfPMI].Value));
                        xmlBuilder.Append(SetValuePairsBool("AgMIP", rsSQLRecordset.Fields[modOpportunity.strfMIP].Value));
                        xmlBuilder.Append(SetValuePairsBool("AgVA", rsSQLRecordset.Fields[modOpportunity.strVA_FUNDING].Value));
                        // RY 13/6/2005 - end
                        xmlBuilder.Append(">");

                        // check for fees on this loan
                        xmlBuilder.Append(SetLoanFees(neighborhoodId, loanId));
                        xmlBuilder.Append("</Loan>" + "\r\n");
                        rsSQLRecordset.MoveNext();
                    }
                }
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sets the Loan Fee XML information for the financial calculator
        /// </summary>
        /// <param name="neighborhoodId">NeighborhoodId</param>
        /// <param name="loanId">Loan Id</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision# Date Author Description
        /// 3.8.0.0   5/12/2006  DYin  Converted to .Net C# code.
        /// </history>
        protected virtual string SetLoanFees(object neighborhoodId, object loanId)
        {
            try
            {
                object DID = RSysSystem.Tables[modOpportunity.strtLOAN].Fields[modOpportunity.strfDIVISION_ID]
                    .Index(loanId);
                // RY
                // loan fee selection by loan
                StringBuilder sqlBuilder = new StringBuilder();
                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append("Loan_Id, ");
                //sqlBuilder.Append("CAST(Loan_Id as int) AS Loan_Id, ");
                sqlBuilder.Append("ISNULL(Name,'') AS Name, ");
                sqlBuilder.Append("ISNULL(Amount,0.0) AS Amount, ");
                sqlBuilder.Append("ISNULL(PctLoanAmount,0) AS PctLoanAmount, ");
                sqlBuilder.Append("ISNULL(PctSalesAmount,0) AS PctSaleAmount, ");
                sqlBuilder.Append("ISNULL(PWM,0) AS PWM, ");
                sqlBuilder.Append("ISNULL(PAC,0) AS PAC, ");
                sqlBuilder.Append("ISNULL(Impound,0) AS Impound, ");
                sqlBuilder.Append("ISNULL(Months,0) AS Months ");
                sqlBuilder.Append("FROM Loan_Fees ");
                sqlBuilder.Append("WHERE ((Loan_Id = ");
                sqlBuilder.Append(RSysSystem.IdToString(loanId));
                sqlBuilder.Append(") ");
                // strSQL = strSQL & " AND (Neighborhood_Id = " & CStr(NID) & ") )"
                sqlBuilder.Append("  OR (Fee_Level = 'Divisional' AND Division_Id = ");
                sqlBuilder.Append(RSysSystem.IdToString(DID));
                sqlBuilder.Append("))");
                // RY
                sqlBuilder.Append("AND (Inactive is null or Inactive = 0)");
                sqlBuilder.Append("ORDER BY Name ");

                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // return results from database
                Recordset rsSQLRecordset = dataAccess.GetRecordset(sqlBuilder.ToString());

                // Build XML
                StringBuilder xmlBuilder = new StringBuilder();
                if (rsSQLRecordset.RecordCount >0)
                {
                    // records found, create a dataset on the rn_appointments table
                    rsSQLRecordset.MoveFirst();
                    xmlBuilder.Append("<Fees>");
                    while(!(rsSQLRecordset.EOF))
                    {
                        xmlBuilder.Append("<Fee ");
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfNAME, rsSQLRecordset.Fields[modOpportunity.strfNAME].Value));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfAMOUNT, rsSQLRecordset.Fields[modOpportunity.strfAMOUNT].Value));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfPCT_LOAN_AMOUNT, TypeConvert.ToDouble(rsSQLRecordset.Fields[modOpportunity.strfPCT_LOAN_AMOUNT].Value)
                            / 100D));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfPCT_SALE_AMOUNT, TypeConvert.ToDouble(rsSQLRecordset.Fields[modOpportunity.strfPCT_SALE_AMOUNT].Value)
                            / 100D));
                        xmlBuilder.Append(SetValuePairsBool(modOpportunity.strfPWM, rsSQLRecordset.Fields[modOpportunity.strfPWM].Value));
                        xmlBuilder.Append(SetValuePairsBool(modOpportunity.strfPAC, rsSQLRecordset.Fields[modOpportunity.strfPAC].Value));
                        xmlBuilder.Append(SetValuePairsBool(modOpportunity.strfIMPOUND, rsSQLRecordset.Fields[modOpportunity.strfIMPOUND].Value));
                        xmlBuilder.Append(SetValuePairsString(modOpportunity.strfMONTHS, rsSQLRecordset.Fields[modOpportunity.strfMONTHS].Value));
                        xmlBuilder.Append("/>\r\n");
                        rsSQLRecordset.MoveNext();
                    }
                    xmlBuilder.Append("</Fees>");
                }
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Return the value of the loan to be geneated in XML format
        /// This XML needs to be included if the Save button is to be diplayed 
        /// </summary>
        /// <returns>
        /// XML string for the Opp XML node containing the TPC attribute with the loan amount</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetOpportunityTotalXml()
        {
            try
            {
                StringBuilder xmlBuilder = new StringBuilder();
                xmlBuilder.Append("<Opp ");
                xmlBuilder.Append(SetValuePairsString("TPC", DEFAULT_LOAN_AMT));
                xmlBuilder.Append("/>\r\n");
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Save or update a Loan Profile
        /// </summary>
        /// <param name="loanProfileId">Loan Profile Id</param>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="loanProfileXml" >Loan Profile XML</param>
        /// <returns>true/false</returns>
        /// <history>
        /// Revision# Date Author Description
        /// 3.8.0.0   5/12/2006  DYin  Converted to .Net C# code.
        /// 5.9.0.0   3/20/2007  RY    Issue 65536-17297.  Fixed bug on resetting other loan profile Selected flags.
        /// </history>
        protected virtual bool SaveLoanProfile(object loanProfileId, object opportunityId, string loanProfileXml)
        {
            try
            {
                // Create share function library object
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // Get parameter values
                if (((loanProfileId == DBNull.Value) && (opportunityId == DBNull.Value)) || (loanProfileXml == null))
                    return false;
                else
                {
                    Recordset rstLoanProfile = null;
                    if (loanProfileId == DBNull.Value)
                    {
                        // Add new LP
                        rstLoanProfile = objLib.GetNewRecordset(modOpportunity.strtLOAN_PROFILE, modOpportunity.strfLOAN_PROFILE_NAME,
                            modOpportunity.strfLOAN1_ID, modOpportunity.strfLOAN1_AMT, modOpportunity.strfLOAN1_INT, modOpportunity.strfLOAN2_ID,
                            modOpportunity.strfLOAN2_AMT, modOpportunity.strfLOAN2_INT, modOpportunity.strfMTHLY_INCOME,
                            modOpportunity.strfMTHLY_DEBT, modOpportunity.strfTOTAL_PRICE, modOpportunity.strfDOWN_PMT,
                            modOpportunity.strfPOST_CONTRACT_ADJ, modOpportunity.strfEST_MTH_PMT, modOpportunity.strfREGION_ID,
                            modOpportunity.strfDIVISION_ID, modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfNEIGHBORHOOD_ID,
                            modOpportunity.strfLOAN_PROGRAM_ID, modOpportunity.strlPARTICIPATION_FEE, modOpportunity.strfSELECTED);

                        rstLoanProfile.AddNew(Type.Missing, Type.Missing);
                    }
                    else
                    {
                        // update currect LP
                        rstLoanProfile = objLib.GetRecordset(loanProfileId, modOpportunity.strtLOAN_PROFILE, modOpportunity.strfLOAN_PROFILE_NAME,
                            modOpportunity.strfLOAN1_ID, modOpportunity.strfLOAN1_AMT, modOpportunity.strfLOAN1_INT, modOpportunity.strfLOAN2_ID,
                            modOpportunity.strfLOAN2_AMT, modOpportunity.strfLOAN2_INT, modOpportunity.strfMTHLY_INCOME,
                            modOpportunity.strfMTHLY_DEBT, modOpportunity.strfTOTAL_PRICE, modOpportunity.strfDOWN_PMT,
                            modOpportunity.strfPOST_CONTRACT_ADJ, modOpportunity.strfEST_MTH_PMT, modOpportunity.strfREGION_ID,
                            modOpportunity.strfDIVISION_ID, modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfNEIGHBORHOOD_ID,
                            modOpportunity.strfLOAN_PROGRAM_ID, modOpportunity.strlPARTICIPATION_FEE, modOpportunity.strfSELECTED);
                    }

                    // Load the XML

                    XmlDocument objXMLDoc = new XmlDocument();
                    objXMLDoc.LoadXml(loanProfileXml);
                    // Parse the XML
                    GetLoanProfileInfo(objXMLDoc);

                    // Need to convert Int to Hex to Ids
                    rstLoanProfile.Fields[modOpportunity.strfLOAN_PROFILE_NAME].Value = LoanProfile.Name;
                    rstLoanProfile.Fields[modOpportunity.strfLOAN1_ID].Value = LoanProfile.Loan1Id;
                    rstLoanProfile.Fields[modOpportunity.strfLOAN1_AMT].Value = LoanProfile.Loan1Amount;
                    rstLoanProfile.Fields[modOpportunity.strfLOAN1_INT].Value = LoanProfile.Loan1InterestRate * 100.0;
                    // Convert decimal to %
                    rstLoanProfile.Fields[modOpportunity.strfLOAN2_ID].Value = LoanProfile.Loan2Id;
                    rstLoanProfile.Fields[modOpportunity.strfLOAN2_AMT].Value = LoanProfile.Loan2Amount;
                    rstLoanProfile.Fields[modOpportunity.strfLOAN2_INT].Value = LoanProfile.Loan2InterestRate * 100.0;
                    // Convert decimal to %
                    rstLoanProfile.Fields[modOpportunity.strfMTHLY_INCOME].Value = LoanProfile.MonthlyIncome;
                    rstLoanProfile.Fields[modOpportunity.strfMTHLY_DEBT].Value = LoanProfile.MonthlyDebt;
                    rstLoanProfile.Fields[modOpportunity.strfTOTAL_PRICE].Value = LoanProfile.TotalPrice;
                    rstLoanProfile.Fields[modOpportunity.strfDOWN_PMT].Value = LoanProfile.DownPayment;
                    rstLoanProfile.Fields[modOpportunity.strfPOST_CONTRACT_ADJ].Value = LoanProfile.PostContractAdjustment;
                    rstLoanProfile.Fields[modOpportunity.strfEST_MTH_PMT].Value = LoanProfile.Payment;
                    rstLoanProfile.Fields[modOpportunity.strfREGION_ID].Value = LoanProfile.RegionId;
                    rstLoanProfile.Fields[modOpportunity.strfDIVISION_ID].Value = LoanProfile.DivisionId;
                    rstLoanProfile.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value = LoanProfile.NeighborhoodId;
                    rstLoanProfile.Fields[modOpportunity.strfLOAN_PROGRAM_ID].Value = LoanProfile.LoanProgramId;
                    rstLoanProfile.Fields[modOpportunity.strfOPPORTUNITY_ID].Value = opportunityId;
                    // RY: save participation fee into Pivotal. Also mark current loan profile as selected.
                    rstLoanProfile.Fields[modOpportunity.strlPARTICIPATION_FEE].Value = LoanProfile.ParticipationFee;
                    rstLoanProfile.Fields[modOpportunity.strfSELECTED].Value = true;

                    // Save the recordset
                    objLib.SaveRecordset(modOpportunity.strtLOAN_PROFILE, rstLoanProfile);

                    // RY: Mark other loan profiles of the current quote as not selected.
                    if ((loanProfileId == DBNull.Value))
                    {
                        loanProfileId = rstLoanProfile.Fields[modOpportunity.strfLOAN_PROFILE_ID].Value;
                    }
                    opportunityId = RSysSystem.Tables[modOpportunity.strtLOAN_PROFILE].Fields[modOpportunity.strfOPPORTUNITY_ID].Index(loanProfileId);
                    rstLoanProfile = objLib.GetRecordset(modOpportunity.strqLOAN_PROFILES_FOR_QUOTE, 1, opportunityId,
                        modOpportunity.strfSELECTED);
                    if (rstLoanProfile.RecordCount > 0)
                    {
                        rstLoanProfile.MoveFirst();
                        while (!(rstLoanProfile.EOF))
                        {
                            if ( ! RSysSystem.EqualIds( rstLoanProfile.Fields[modOpportunity.strfLOAN_PROFILE_ID].Value, loanProfileId))
                            {
                                rstLoanProfile.Fields[modOpportunity.strfSELECTED].Value = false;
                            }
                            rstLoanProfile.MoveNext();
                        }
                        objLib.SaveRecordset(modOpportunity.strtLOAN_PROFILE, rstLoanProfile);
                    }
                    return true;
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Set up the m_LoanProfile type containing the values for the selected Loan Profile from the
        /// XML that is returned from the Financial Calculator
        /// </summary>
        /// <param name="xmlDocument">XML passed in from the Financial Calculator application</param>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual void GetLoanProfileInfo(XmlDocument xmlDocument)
        {
            try
            {
                LoanProfileInfo loanProfile = LoanProfile;
                XmlElement currElement = null;
                string sXpath = "LoanProfile";
                XmlNodeList objDOMNodeList = xmlDocument.SelectNodes(sXpath);
                if (objDOMNodeList.Count > 0)
                {
                    currElement = (XmlElement) objDOMNodeList[0];
                    if (currElement.Attributes.Count != 0)
                    {
                        for (int i = 0; i < currElement.Attributes.Count; ++i)
                        {
                            string sTextValue = Opportunity.DecodeXml((TypeConvert.ToString(currElement.Attributes[i].Value)).Trim());
                            switch (currElement.Attributes[i].Name)
                            {
                                case "ID":
                                    loanProfile.LoanProfileId = this.ObjectToId(sTextValue);
                                    break;
                                case "PvtlID":
                                    loanProfile.LoanProgramId = this.ObjectToId(sTextValue);
                                    break;
                                case modOpportunity.strfNAME:
                                    loanProfile.Name = sTextValue;
                                    break;
                            }
                        }
                    }
                }

                sXpath = "LoanProfile/Loan1";
                objDOMNodeList = xmlDocument.SelectNodes(sXpath);
                if (objDOMNodeList.Count > 0)
                {
                    currElement = (XmlElement) objDOMNodeList[0];
                    if (currElement.Attributes.Count > 0)
                    {
                        for (int i = 0; i < currElement.Attributes.Count; ++i)
                        {
                            string sTextValue = Opportunity.DecodeXml((TypeConvert.ToString(currElement.Attributes[i].Value)).Trim());
                            switch (currElement.Attributes[i].Name)
                            {
                                case "ID":
                                    loanProfile.Loan1Id = this.ObjectToId(sTextValue);
                                    break;
                                case "Amt":
                                    loanProfile.Loan1Amount = TypeConvert.ToDecimal(sTextValue);
                                    break;
                                case "Int":
                                    loanProfile.Loan1InterestRate = TypeConvert.ToDouble(sTextValue);
                                    break;
                            }
                        }
                    }
                }

                sXpath = "LoanProfile/Loan2";
                objDOMNodeList = xmlDocument.SelectNodes(sXpath);
                if (objDOMNodeList.Count > 0)
                {
                    currElement = (XmlElement) objDOMNodeList[0];
                    if (currElement.Attributes.Count != 0)
                    {
                        for (int i = 0; i < currElement.Attributes.Count; ++i)
                        {
                            string sTextValue = Opportunity.DecodeXml((TypeConvert.ToString(currElement.Attributes[i].Value)).Trim());
                            switch (currElement.Attributes[i].Name)
                            {
                                case "ID":
                                    loanProfile.Loan2Id = this.ObjectToId(sTextValue);
                                    break;
                                case "Amt":
                                    loanProfile.Loan2Amount = TypeConvert.ToDecimal(sTextValue);
                                    break;
                                case "Int":
                                    loanProfile.Loan2InterestRate = TypeConvert.ToDouble(sTextValue);
                                    break;
                            }
                        }
                    }
                }

                XmlNode objDOMNode = xmlDocument.SelectSingleNode("LoanProfile/MthlyIncome");
                loanProfile.MonthlyIncome = TypeConvert.ToDecimal(objDOMNode.InnerText);
                objDOMNode = xmlDocument.SelectSingleNode("LoanProfile/MthlyDebt");
                loanProfile.MonthlyDebt = TypeConvert.ToDecimal(objDOMNode.InnerText);
                objDOMNode = xmlDocument.SelectSingleNode("LoanProfile/TotalPrice");
                loanProfile.TotalPrice = TypeConvert.ToDecimal(objDOMNode.InnerText);
                objDOMNode = xmlDocument.SelectSingleNode("LoanProfile/DownPayment");
                loanProfile.DownPayment = TypeConvert.ToDecimal(objDOMNode.InnerText);
                objDOMNode = xmlDocument.SelectSingleNode("LoanProfile/PCA");
                loanProfile.PostContractAdjustment = TypeConvert.ToDecimal(objDOMNode.InnerText);
                objDOMNode = xmlDocument.SelectSingleNode("LoanProfile/Payment");
                loanProfile.Payment = TypeConvert.ToDecimal(objDOMNode.InnerText);
                // Get the Territory Information
                objDOMNode = xmlDocument.SelectSingleNode("LoanProfile/RegionID");
                loanProfile.RegionId = this.ObjectToId(objDOMNode.InnerText);
                objDOMNode = xmlDocument.SelectSingleNode("LoanProfile/DivisionID");
                loanProfile.DivisionId = this.ObjectToId(objDOMNode.InnerText);
                objDOMNode = xmlDocument.SelectSingleNode("LoanProfile/NbhdID");
                loanProfile.NeighborhoodId = this.ObjectToId(objDOMNode.InnerText);
                XmlNodeList objTempDomNodeList = xmlDocument.SelectNodes("LoanProfile/ParticipationFee");
                if (objTempDomNodeList.Count > 0)
                {
                    objDOMNode = xmlDocument.SelectSingleNode("LoanProfile/ParticipationFee");
                    loanProfile.ParticipationFee = TypeConvert.ToBoolean(objDOMNode.InnerText);
                }
                this.LoanProfile = loanProfile;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Loads the m_LoanProfile type from the database
        /// </summary>
        /// <param name="loanProfileId">Loan Profile Id</param>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// 5.9.0.0       3/20/2007   RY          Issue 63834.  Fixed down payment percent to include decimal digits.
        /// 5.9.0.0       8/25/2007   RY          Issue 65536-20245. Set the LoanProfile member properly.
        /// </history>
        protected virtual void LoadLoanProfileInfo(object loanProfileId)
        {
            try
            {
                // Create share function library object
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                LoanProfileInfo loanProfile = new LoanProfileInfo();
                Recordset rstLoanProfile = objLib.GetRecordset(loanProfileId, modOpportunity.strtLOAN_PROFILE, modOpportunity.strfTOTAL_PRICE,
                    modOpportunity.strfDOWN_PMT, modOpportunity.strfPOST_CONTRACT_ADJ, modOpportunity.strfMTHLY_INCOME,
                    modOpportunity.strfMTHLY_DEBT, modOpportunity.strfLOAN1_AMT, modOpportunity.strfLOAN2_AMT, modOpportunity.strfLOAN1_INT,
                    modOpportunity.strfLOAN2_INT, modOpportunity.strfEST_MTH_PMT, modOpportunity.strfLOAN1_ID, modOpportunity.strfLOAN2_ID,
                    modOpportunity.strfLOAN_PROFILE_NAME, modOpportunity.strfOPPORTUNITY_ID, modOpportunity.strfREGION_ID,
                    modOpportunity.strlPARTICIPATION_FEE, modOpportunity.strfDIVISION_ID, modOpportunity.strfNEIGHBORHOOD_ID,
                    modOpportunity.strfLOAN_PROGRAM_ID, modOpportunity.strfDOWN_PMT_PCT, modOpportunity.strfXML);

                if (rstLoanProfile.RecordCount > 0)
                {
                    loanProfile.LoanProfileId = Convert.IsDBNull(RSysSystem.IdToString(loanProfileId)) ? TypeConvert.ToString(-1) 
                        : RSysSystem.IdToString(loanProfileId);
                    loanProfile.LoanProgramId = rstLoanProfile.Fields[modOpportunity.strfLOAN_PROGRAM_ID].Value;
                    loanProfile.Name = TypeConvert.ToString(rstLoanProfile.Fields[modOpportunity.strfLOAN_PROFILE_NAME].Value);
                    loanProfile.TotalPrice = TypeConvert.ToDecimal(rstLoanProfile.Fields[modOpportunity.strfTOTAL_PRICE].Value);
                    loanProfile.DownPayment = TypeConvert.ToDecimal(rstLoanProfile.Fields[modOpportunity.strfDOWN_PMT].Value);
                    loanProfile.DownPaymentPercent = TypeConvert.ToDouble(rstLoanProfile.Fields[modOpportunity.strfDOWN_PMT_PCT].Value);
                    loanProfile.PostContractAdjustment = TypeConvert.ToDecimal(rstLoanProfile.Fields[modOpportunity.strfPOST_CONTRACT_ADJ].Value);
                    loanProfile.MonthlyIncome = TypeConvert.ToDecimal(rstLoanProfile.Fields[modOpportunity.strfMTHLY_INCOME].Value);
                    loanProfile.MonthlyDebt = TypeConvert.ToDecimal(rstLoanProfile.Fields[modOpportunity.strfMTHLY_DEBT].Value);
                    loanProfile.Loan1Id = rstLoanProfile.Fields[modOpportunity.strfLOAN1_ID].Value;
                    loanProfile.Loan1Amount = TypeConvert.ToDecimal(rstLoanProfile.Fields[modOpportunity.strfLOAN1_AMT].Value);
                    loanProfile.Loan1InterestRate = TypeConvert.ToDouble(rstLoanProfile.Fields[modOpportunity.strfLOAN1_INT].Value);
                    loanProfile.Loan2Id = rstLoanProfile.Fields[modOpportunity.strfLOAN2_ID].Value;
                    loanProfile.Loan2Amount = TypeConvert.ToDecimal(rstLoanProfile.Fields[modOpportunity.strfLOAN2_AMT].Value);
                    loanProfile.Loan2InterestRate = TypeConvert.ToDouble(rstLoanProfile.Fields[modOpportunity.strfLOAN2_INT].Value);
                    loanProfile.Payment = TypeConvert.ToDecimal(rstLoanProfile.Fields[modOpportunity.strfEST_MTH_PMT].Value);
                    loanProfile.RegionId = rstLoanProfile.Fields[modOpportunity.strfREGION_ID].Value;
                    loanProfile.DivisionId = rstLoanProfile.Fields[modOpportunity.strfDIVISION_ID].Value;
                    loanProfile.NeighborhoodId = rstLoanProfile.Fields[modOpportunity.strfNEIGHBORHOOD_ID].Value;
                    loanProfile.ParticipationFee = Convert.IsDBNull(rstLoanProfile.Fields[modOpportunity.strlPARTICIPATION_FEE].Value)
                        ? false : TypeConvert.ToBoolean(rstLoanProfile.Fields[modOpportunity.strlPARTICIPATION_FEE].Value);

                    LoanProfile = loanProfile;
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Return all Loan Programs for a specified Neighborhood in XML format
        /// </summary>
        /// <param name="neighborhoodId">Neighborhood Id</param>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetLoanProgramsForNeighborhood(object neighborhoodId, object opportunityId)
        {
            try
            {
                // Create share function library object
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstNbhd = objLib.GetRecordset(neighborhoodId, modOpportunity.strtNEIGHBORHOOD, modOpportunity.strfREGION_ID,
                    modOpportunity.strfDIVISION_ID, modOpportunity.strfNAME, modOpportunity.strfHOME_PHONE);

                int xmlRegionId = 0;
                int xmlDivisionId = 0;
                object regionId = DBNull.Value;
                object divisionId = DBNull.Value;
                string strNbhdName = string.Empty;
                string strPhone = string.Empty;
                if (rstNbhd.RecordCount > 0)
                {
                    xmlRegionId = this.IdToInteger(rstNbhd.Fields[modOpportunity.strfREGION_ID].Value);
                    regionId = rstNbhd.Fields[modOpportunity.strfREGION_ID].Value;
                    xmlDivisionId = this.IdToInteger(rstNbhd.Fields[modOpportunity.strfDIVISION_ID].Value);
                    divisionId = rstNbhd.Fields[modOpportunity.strfDIVISION_ID].Value;
                    strNbhdName = TypeConvert.ToString(rstNbhd.Fields[modOpportunity.strfNAME].Value);
                    strPhone = TypeConvert.ToString(rstNbhd.Fields[modOpportunity.strfHOME_PHONE].Value);
                }

                Recordset rstOpp = objLib.GetRecordset(opportunityId, modOpportunity.strt_OPPORTUNITY, 
                    modOpportunity.strfQUOTE_TOTAL);
                decimal dblPcaTotal = DEFAULT_LOAN_AMT;
                decimal dblQuoteTotal = 0;
                if (rstOpp.RecordCount > 0)
                {
                    dblQuoteTotal = TypeConvert.ToDecimal(rstOpp.Fields[modOpportunity.strfQUOTE_TOTAL].Value);

                    // adds up all Post Contract Adjustments for the Quote
                    Recordset rstPca = objLib.GetRecordset(modOpportunity.strqPCA, 1, opportunityId, modOpportunity.strfOPPORTUNITY_ADJUSTMENT_ID,
                        modOpportunity.strfSUM_FIELD);
                    if (rstPca.RecordCount > 0)
                    {
                        rstPca.MoveFirst();
                        while (!rstPca.EOF)
                        {
                            dblPcaTotal = dblPcaTotal + TypeConvert.ToDecimal(rstPca.Fields[modOpportunity.strfSUM_FIELD].Value);
                            rstPca.MoveNext();
                        }
                    }
                }

                StringBuilder xmlBuilder = new StringBuilder();
                xmlBuilder.Append("<LoanPrograms>");
                xmlBuilder.Append("<Region ID=" + (char)(34) + xmlRegionId + (char)(34) + " Name=" + (char)(34) + "Loan Profile Region" + (char)(34) + ">");
                xmlBuilder.Append("<Division ID=" + (char)(34) + xmlDivisionId + (char)(34) + " Name=" + (char)(34) + "Loan Profile Division" + (char)(34) + ">");
                xmlBuilder.Append("<Neighborhood ID=" + (char)(34) + this.IdToInteger(neighborhoodId) + (char)(34) + " Name=" + (char)(34) + strNbhdName + (char)(34) + " Phone=" + (char)(34) + strPhone + (char)(34) + ">");
                // Add neighborhood fees
                xmlBuilder.Append(SetNeighborhoodFees(regionId, divisionId, neighborhoodId));
                // Add loan programs
                xmlBuilder.Append(SetLoanPrograms(regionId, divisionId, neighborhoodId));
                xmlBuilder.Append("</Neighborhood>");
                xmlBuilder.Append("</Division>");
                xmlBuilder.Append("</Region>");
                xmlBuilder.Append(SetNeighborhoodOpportunityXml(dblPcaTotal, dblQuoteTotal));
                xmlBuilder.Append("</LoanPrograms>");
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Creates the Opp XML node to set the Quote amount
        /// </summary>
        /// <param name="postContractAdjustment">Post contract adjustment</param>
        /// <param name="totalAmount">Total Invoice amount for the Quote</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetNeighborhoodOpportunityXml(decimal postContractAdjustment, decimal totalAmount)
        {
            try
            {
                decimal dblTPC_local = 0;
                if ((totalAmount == 0))
                    dblTPC_local = DEFAULT_LOAN_AMT;
                else
                    dblTPC_local = totalAmount;

                StringBuilder xmlBuilder = new StringBuilder();
                xmlBuilder.Append("<Opp ");
                xmlBuilder.Append(SetValuePairsString("TPC", dblTPC_local));
                xmlBuilder.Append(SetValuePairsString("PCA", postContractAdjustment));
                xmlBuilder.Append("/>");
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Load a Loan Profile from the database
        /// </summary>
        /// <param name="loanProfileId">Loan Profile Id</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string LoadLoanProfile(object loanProfileId)
        {
            try
            {
                LoadLoanProfileInfo(loanProfileId);
                return SetLoanProfile(loanProfileId);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sets up the header XML to contain Loan Profiles by calling <see cref="SetLoanProfileXml"/> and
        /// <see cref="SetLoanPrograms"/>.
        /// </summary>
        /// <param name="loanProfileId">Loan Profile Id</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetLoanProfile(object loanProfileId)
        {
            try
            {
                string strNbhdName = TypeConvert.ToString(RSysSystem.Tables[modOpportunity.strtNEIGHBORHOOD].Fields[modOpportunity.strfNAME].Index(LoanProfile.NeighborhoodId));

                StringBuilder xmlBuilder = new StringBuilder();
                xmlBuilder.Append("<LoanPrograms>");
                xmlBuilder.Append("<Region ID=" + (char)(34) + this.IdToInteger(LoanProfile.RegionId) + (char)(34) + " Name=" + (char)(34) + "Loan Profile Region" + (char)(34) + ">");
                xmlBuilder.Append("<Division ID=" + (char)(34) + this.IdToInteger(LoanProfile.DivisionId) + (char)(34) + " Name=" + (char)(34) + "Loan Profile Division" + (char)(34) + ">");
                xmlBuilder.Append("<Neighborhood ID=" + (char)(34) + this.IdToInteger(LoanProfile.NeighborhoodId) + (char)(34) + " Name=" + (char)(34) + strNbhdName + (char)(34) + ">");
                // Add neighborhood fees
                xmlBuilder.Append(SetNeighborhoodFees(LoanProfile.RegionId, LoanProfile.DivisionId, LoanProfile.NeighborhoodId));
                // Load the custom Loan Profile
                xmlBuilder.Append(SetLoanProfileXml(loanProfileId));
                // Load up all other loan programs for this Nbhd
                xmlBuilder.Append(SetLoanPrograms(LoanProfile.RegionId, LoanProfile.DivisionId, LoanProfile.NeighborhoodId));
                xmlBuilder.Append("</Neighborhood>");
                xmlBuilder.Append("</Division>");
                xmlBuilder.Append("</Region>");
                xmlBuilder.Append(SetOpportunityXml());
                xmlBuilder.Append("</LoanPrograms>");
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Sets up the header XML to contain Loan Profiles by calling <see cref="SetLoanProfileXml"/> and 
        /// <see cref="SetLoanPrograms"/>
        /// </summary>
        /// <param name="loanProfileId">Loan Profile Id</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetLoanProfileXml(object loanProfileId)
        {
            try
            {
                StringBuilder xmlBuilder = new StringBuilder();
                xmlBuilder.Append("<LoanProgram ID=" + (char)(34) + this.IdToInteger(LoanProfile.LoanProgramId) + (char)(34) + new String(' ', 1));
                xmlBuilder.Append(SetValuePairsString(modOpportunity.strfNAME, LoanProfile.Name));
                xmlBuilder.Append(SetValuePairsString(modOpportunity.strfEND_DATE, ""));
                xmlBuilder.Append(SetValuePairsString(modOpportunity.strfAVAILABLE, "yes"));
                xmlBuilder.Append(SetValuePairsString(modOpportunity.strlCLOSING_COST_FIXED, "0"));
                xmlBuilder.Append(SetValuePairsString(modOpportunity.strlCLOSING_COST_PCT_APPRAISED, "0"));
                xmlBuilder.Append(SetValuePairsBool(modOpportunity.strlPARTICIPATIONFEE, LoanProfile.ParticipationFee));
                xmlBuilder.Append(SetValuePairsString(modOpportunity.strlMINIMUM_DOWN_PERCENT, 100 - (LoanProfile.DownPaymentPercent)));
                xmlBuilder.Append(">");
                if (!(Convert.IsDBNull(LoanProfile.Loan1Id)))
                {
                    xmlBuilder.Append(SetLoanProfileLoanXml(LoanProfile.Loan1Id, LoanProfile.NeighborhoodId, LoanProfile.Loan1InterestRate));
                }
                if (!(Convert.IsDBNull(LoanProfile.Loan2Id))) //&& m_LoanProfile.Loan2Id > 0)
                {
                    xmlBuilder.Append(SetLoanProfileLoanXml(LoanProfile.Loan2Id, LoanProfile.NeighborhoodId, LoanProfile.Loan2InterestRate));
                }
                xmlBuilder.Append("</LoanProgram>");
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Retrieves the loan information from the database for a specified loan
        /// </summary>
        /// <param name="loanId">Loan Id in integer format</param>
        /// <param name="neighborhoodId">Neighborhood Id in integer format</param>
        /// <param name="loanRate">Loan Rate</param>
        /// <returns>XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetLoanProfileLoanXml(object loanId, object neighborhoodId, double loanRate)
        {
            try
            {
                // Create share function library object
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstLoan = objLib.GetRecordset(loanId, modOpportunity.strtLOAN, modOpportunity.strfLOAN_NAME,
                    modOpportunity.strfTYPE, modOpportunity.strfTERM, modOpportunity.strfPERIODS, modOpportunity.strfINDEX_NAME,
                    modOpportunity.strfINDEX_RATE, modOpportunity.strfMARGIN_RATE, modOpportunity.strfADJ_RATE_CAP,
                    modOpportunity.strfADJ_RATE_FLOOR, modOpportunity.strfADJ_PERIODS, modOpportunity.strfTOPRATIO,
                    modOpportunity.strfBOTTOMRATIO, modOpportunity.strfMAX_LOAN_AMOUNT, modOpportunity.strfINT_ONLY_TERMS_IN_YEAR,
                    modOpportunity.strfPREPAID_INT_NUM_OF_DAYS, modOpportunity.strfADJSTARTINGRATE, modOpportunity.strfROUND_TO_NEAREST_50,
                    modOpportunity.strfPMI, modOpportunity.strfMIP, modOpportunity.strVA_FUNDING);

                StringBuilder xmlBuilder = new StringBuilder();
                if (rstLoan.RecordCount > 0)
                {
                    xmlBuilder.Append("<Loan ID=" + (char)(34) + this.IdToInteger(loanId) + (char)(34) + new String(' ', 1));
                    xmlBuilder.Append(SetValuePairsString(modOpportunity.strfNAME, rstLoan.Fields[modOpportunity.strfLOAN_NAME].Value));
                    xmlBuilder.Append(SetValuePairsString(modOpportunity.strfTYPE, rstLoan.Fields[modOpportunity.strfTYPE].Value));
                    xmlBuilder.Append(SetValuePairsString("Rate", loanRate));
                    xmlBuilder.Append(SetValuePairsString(modOpportunity.strfTERM, rstLoan.Fields[modOpportunity.strfTERM].Value));
                    xmlBuilder.Append(SetValuePairsString("PeriodsPerYear", rstLoan.Fields[modOpportunity.strfPERIODS].Value));
                    xmlBuilder.Append(SetValuePairsString("RepricePeriods", ""));
                    xmlBuilder.Append(SetValuePairsString("PayoffPeriods", ""));
                    xmlBuilder.Append(SetValuePairsString("MaximumLoanAmount", rstLoan.Fields[modOpportunity.strfMAX_LOAN_AMOUNT].Value));
                    xmlBuilder.Append(SetValuePairsString("ClosingCostPctLoanAmt", ""));
                    xmlBuilder.Append(SetValuePairsString("Index", rstLoan.Fields[modOpportunity.strfINDEX_NAME].Value));
                    xmlBuilder.Append(SetValuePairsString("IndexRate", rstLoan.Fields[modOpportunity.strfINDEX_RATE].Value));
                    xmlBuilder.Append(SetValuePairsString("MarginRate", rstLoan.Fields[modOpportunity.strfMARGIN_RATE].Value));
                    xmlBuilder.Append(SetValuePairsString("TeaserRate", loanRate));
                    xmlBuilder.Append(SetValuePairsString("TeaserTerm", "36"));
                    xmlBuilder.Append(SetValuePairsString("RateCap", rstLoan.Fields[modOpportunity.strfTOPRATIO].Value));
                    xmlBuilder.Append(SetValuePairsString("RateFloor", rstLoan.Fields[modOpportunity.strfBOTTOMRATIO].Value));
                    xmlBuilder.Append(SetValuePairsString("AdjustPeriods", rstLoan.Fields[modOpportunity.strfADJ_PERIODS].Value));
                    xmlBuilder.Append(SetValuePairsString("AdjustRateCap", rstLoan.Fields[modOpportunity.strfADJ_RATE_CAP].Value));
                    xmlBuilder.Append(SetValuePairsString("AdjustRateFloor", rstLoan.Fields[modOpportunity.strfADJ_RATE_FLOOR].Value));
                    // RY 13/6/2005 - start
                    xmlBuilder.Append(SetValuePairsString("InterestOnlyTerm ", rstLoan.Fields[modOpportunity.strfINT_ONLY_TERMS_IN_YEAR].Value));
                    xmlBuilder.Append(SetValuePairsString("PPID", rstLoan.Fields[modOpportunity.strfPREPAID_INT_NUM_OF_DAYS].Value));
                    xmlBuilder.Append(SetValuePairsString("Start", rstLoan.Fields[modOpportunity.strfADJSTARTINGRATE].Value));
                    xmlBuilder.Append(SetValuePairsBool("Rnd50", rstLoan.Fields[modOpportunity.strfROUND_TO_NEAREST_50].Value));
                    xmlBuilder.Append(SetValuePairsBool("AgPMI", rstLoan.Fields[modOpportunity.strfPMI].Value));
                    xmlBuilder.Append(SetValuePairsBool("AgMIP", rstLoan.Fields[modOpportunity.strfMIP].Value));
                    xmlBuilder.Append(SetValuePairsBool("AgVA", rstLoan.Fields[modOpportunity.strVA_FUNDING].Value));
                    // strXML = strXML & SetValuePairsBool("ParticipationFee", m_LoanProfile.ParticipationFee)
                    // RY 13/6/2005 - end
                    xmlBuilder.Append(">");
                    xmlBuilder.Append(SetLoanFees(neighborhoodId, loanId));
                    xmlBuilder.Append("</Loan>");
                    return xmlBuilder.ToString();
                }
                else
                    return string.Empty;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Return the value of the loan to be geneated in XML format
        /// </summary>
        /// <returns>
        /// XML string to be passed to the financial calculator</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetOpportunityXml()
        {
            try
            {
                StringBuilder xmlBuilder = new StringBuilder();
                xmlBuilder.Append("<Opp ");
                xmlBuilder.Append(SetValuePairsString("TPC", LoanProfile.TotalPrice));
                xmlBuilder.Append(SetValuePairsString("LPID", this.IdToInteger(LoanProfile.LoanProgramId)));
                xmlBuilder.Append(SetValuePairsString("Income", LoanProfile.MonthlyIncome));
                xmlBuilder.Append(SetValuePairsString("Debt", LoanProfile.MonthlyDebt));
                xmlBuilder.Append(SetValuePairsString("PCA", LoanProfile.PostContractAdjustment));
                xmlBuilder.Append("/>");
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function is called when the web service needs to determine what
        /// tax rate data is available by year
        /// </summary>
        /// <param name="taxYear">the tax year for tax rate data</param>
        /// <param name="taxScheduleId">the tax schedule id</param>
        /// <returns>XML string representing the tax deduction data defined for the
        /// year requested and the immediate prior year, if any</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetTaxDeductionData(int taxYear, object taxScheduleId)
        {
            try
            {
                StringBuilder sqlBuilder = new StringBuilder();
                // loan fee selection by loan
                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append("StdDeduction, StdDedFor1Checked, StdDedFor2Checked, StdDedFor3Checked, StdDedFor4Checked ");
                sqlBuilder.Append("FROM Tax_Deductions ");
                sqlBuilder.Append("WHERE TaxYear = '" + taxYear + "' ");
                sqlBuilder.Append("AND Tax_Schedule_Id = " + RSysSystem.IdToString(taxScheduleId));

                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // return results from database
                Recordset rsSQLRecordset = dataAccess.GetRecordset(sqlBuilder.ToString());

                // Build XML
                StringBuilder xmlBuilder = new StringBuilder();
                if (rsSQLRecordset.RecordCount >0)
                {
                    // records found
                    rsSQLRecordset.MoveFirst();
                    while(!(rsSQLRecordset.EOF))
                    {
                        xmlBuilder.Append("<Deduction");
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("TaxYear", taxYear));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("Count", 0));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("Amount", TypeConvert.ToDecimal(rsSQLRecordset.Fields[modOpportunity.strfSTDDEDUCTION].Value)));
                        xmlBuilder.Append("/>" + "\r\n");
                        xmlBuilder.Append("<Deduction");
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("TaxYear", taxYear));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("Count", 0));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("Amount", TypeConvert.ToDecimal(rsSQLRecordset.Fields[modOpportunity.strfSTDDEDFOR1CHECKED].Value)));
                        xmlBuilder.Append("/>" + "\r\n");
                        xmlBuilder.Append("<Deduction");
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("TaxYear", taxYear));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("Count", 0));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("Amount", TypeConvert.ToDecimal(rsSQLRecordset.Fields[modOpportunity.strfSTDDEDFOR2CHECKED].Value)));
                        xmlBuilder.Append("/>" + "\r\n");
                        xmlBuilder.Append("<Deduction");
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("TaxYear", taxYear));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("Count", 0));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("Amount", TypeConvert.ToDecimal(rsSQLRecordset.Fields[modOpportunity.strfSTDDEDFOR3CHECKED].Value)));
                        xmlBuilder.Append("/>" + "\r\n");
                        xmlBuilder.Append("<Deduction");
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("TaxYear",  TypeConvert.ToString(taxYear).Replace(",", "")));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("Count", 0));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("Amount", TypeConvert.ToDecimal(rsSQLRecordset.Fields[modOpportunity.strfSTDDEDFOR4CHECKED].Value)));
                        xmlBuilder.Append("/>" + "\r\n");
                        rsSQLRecordset.MoveNext();
                    }
                }
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function is called when the web service needs to determine what
        /// tax table data is available by year and schedule
        /// intYear, the tax year for tax rate data
        /// intTSID, the tax schedule id value
        /// XML string representing the tax table data defined for the
        /// year requested and the immediate prior year, if any
        /// </summary>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetTaxTableData(int taxYear, object taxScheduleId)
        {

            try
            {
                StringBuilder sqlBuilder = new StringBuilder();
                // loan fee selection by loan
                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append("TaxYear, Income_Low, Income_High, Tax_Base_Amount, ");
                sqlBuilder.Append("Tax_Bracket_Percentage, Tax_Income_Base_Amount ");
                sqlBuilder.Append("FROM TaxTables ");
                sqlBuilder.Append("WHERE TaxYear = '" + taxYear + "' AND Tax_Schedule_Id = " + RSysSystem.IdToString(taxScheduleId) + " ");
                sqlBuilder.Append("ORDER BY Income_Low");

                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // return results from database
                Recordset rsSQLRecordset = dataAccess.GetRecordset(sqlBuilder.ToString());

                // Build XML
                StringBuilder xmlBuilder = new StringBuilder();
                if (rsSQLRecordset.RecordCount > 0)
                {
                    // records found
                    rsSQLRecordset.MoveFirst();
                    while(!(rsSQLRecordset.EOF))
                    {
                        xmlBuilder.Append("<Table");
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("TaxYear", taxYear));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("IncomeLow", TypeConvert.ToDecimal(rsSQLRecordset.Fields[modOpportunity.strfINCOME_LOW].Value)));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("IncomeHi", TypeConvert.ToDecimal(rsSQLRecordset.Fields[modOpportunity.strfINCOME_HIGH].Value)));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("TaxBaseAmt", TypeConvert.ToDecimal(rsSQLRecordset.Fields[modOpportunity.strfTAX_BASE_AMOUNT].Value)));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("TaxMTR", TypeConvert.ToDecimal(rsSQLRecordset.Fields[modOpportunity.strfTAX_BRACKET_PERCENTAGE].Value)));
                        xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("TaxIncomeBase", TypeConvert.ToDecimal(rsSQLRecordset.Fields[modOpportunity.strfTAX_INCOME_BASE_AMOUNT].Value)));
                        xmlBuilder.Append("/>\r\n");

                        rsSQLRecordset.MoveNext();
                    }
                }
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function is called when the web service needs to determine what
        /// </summary>
        /// tax rate data is available by year
        /// intYear, the tax year for tax rate data
        /// XML string representing the tax schedule data defined for the
        /// year requested and the immediate prior year, if any
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetTaxScheduleData(int taxYear)
        {
            try
            {
                StringBuilder sqlBuilder = new StringBuilder();

                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append("TaxSchedules.Tax_Schedule_Id AS TSID, ");
                //sqlBuilder.Append("CAST(TaxSchedules.Tax_Schedule_Id AS INT) AS TSID, ");
                sqlBuilder.Append("TaxSchedules.Tax_Schedule_Name AS TSN, ");
                sqlBuilder.Append("TaxSchedules.Tax_Schedule_Code AS TSC, ");
                sqlBuilder.Append("TaxTables.TaxYear ");
                sqlBuilder.Append("FROM TaxSchedules INNER JOIN ");
                sqlBuilder.Append("TaxTables ON TaxSchedules.Tax_Schedule_Id = TaxTables.Tax_Schedule_Id ");
                sqlBuilder.Append("GROUP BY TaxSchedules.Tax_Schedule_Name, ");
                sqlBuilder.Append("TaxSchedules.Tax_Schedule_Code, TaxTables.TaxYear, ");
                sqlBuilder.Append("TaxSchedules.Tax_Schedule_Id ");
                sqlBuilder.Append("HAVING(TaxTables.TaxYear = " + taxYear + ")");

                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // return results from database
                Recordset rsSQLRecordset = dataAccess.GetRecordset(sqlBuilder.ToString());

                // Build XML
                StringBuilder xmlBuilder = new StringBuilder();
                if (rsSQLRecordset.RecordCount > 0)
                {
                    // records found
                    rsSQLRecordset.MoveFirst();
                    while(!(rsSQLRecordset.EOF))
                    {
                        // check for additional standard deduction values here
                        string strDeductData = SetTaxDeductionData(taxYear, rsSQLRecordset.Fields["TSID"].Value);
                        if (strDeductData.Length > 0)
                        {
                            // check for tax table data here
                            string strTableData = SetTaxTableData(taxYear, rsSQLRecordset.Fields["TSID"].Value);
                            if (strTableData.Length > 0)
                            {
                                xmlBuilder.Append("<Schedule");
                                xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("Name", rsSQLRecordset.Fields["TSN"].Value));
                                xmlBuilder.Append(new String(' ',1) + SetValuePairsString("Code", rsSQLRecordset.Fields["TSC"].Value));
                                xmlBuilder.Append(">" + "\r\n");
                                xmlBuilder.Append(strTableData);
                                xmlBuilder.Append(strDeductData);
                                xmlBuilder.Append("</Schedule>");
                            }
                        }
                        rsSQLRecordset.MoveNext();
                    }
                }
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function is called when the web service needs to determine what
        /// </summary>
        /// <param name="taxYear">the tax year for tax rate data</param>
        /// <returns>true, the specified years' data is available
        /// false, the specified years' data is not available</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual bool CheckTaxDataAvailable(int taxYear)
        {
            try
            {
                StringBuilder sqlBuilder = new StringBuilder();

                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append("TaxSchedules.Tax_Schedule_Name, ");
                sqlBuilder.Append("TaxSchedules.Tax_Schedule_Code,");
                sqlBuilder.Append("TaxTables.TaxYear ");
                sqlBuilder.Append("FROM TaxSchedules INNER JOIN ");
                sqlBuilder.Append("TaxTables ON TaxSchedules.Tax_Schedule_Id = TaxTables.Tax_Schedule_Id ");
                sqlBuilder.Append("GROUP BY TaxSchedules.Tax_Schedule_Name, ");
                sqlBuilder.Append("TaxSchedules.Tax_Schedule_Code, ");
                sqlBuilder.Append("TaxTables.TaxYear ");
                sqlBuilder.Append("HAVING(TaxTables.TaxYear = " + taxYear + ")");

                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                // return results from database
                Recordset rsSQLRecordset = dataAccess.GetRecordset(sqlBuilder.ToString());

                bool bTaxDataAvailable = false;

                // Build XML
                string strXML = string.Empty;
                if (rsSQLRecordset.RecordCount >0)
                {
                    bTaxDataAvailable = true;
                }
                return bTaxDataAvailable;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function is called when the web service needs to determine what
        /// tax rate data should be delivered to the XML string
        /// </summary>
        /// <returns>XML string specifying the tax rate data in the database</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetTaxRateData()
        {
            try
            {
                // if next years' data is available
                int intTopYear = 0;
                StringBuilder xmlBuilder = new StringBuilder();
                if (CheckTaxDataAvailable((DateTime.Today.Year + 1)))
                {
                    intTopYear = (DateTime.Today.Year + 1);
                }
                else
                {
                    // if this years' data is available
                    if (CheckTaxDataAvailable((DateTime.Today.Year)))
                    {
                        intTopYear = (DateTime.Today.Year);
                    }
                    else
                    {
                        // if last years' data is available
                        if (CheckTaxDataAvailable((DateTime.Today.Year - 1)))
                        {
                            intTopYear = (DateTime.Today.Year - 1);
                        }
                        else
                        {
                            xmlBuilder.Append("<TaxRates ");
                            xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("DataAvailable", 0));
                            xmlBuilder.Append("/>\r\n");
                        }
                    }
                }

                if (xmlBuilder.Length == 0)
                {
                    xmlBuilder.Append("<TaxRates");
                    xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("DataAvailable", 1));
                    xmlBuilder.Append(">\r\n");
                    xmlBuilder.Append(SetTaxScheduleData(intTopYear));
                    xmlBuilder.Append("</TaxRates>\r\n");
                }
                return xmlBuilder.ToString();

            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function is called when the financial calculator is loaded
        /// </summary>
        /// <param name="loanProfileId">Loan Profile Id</param>
        /// <param name="opportunityId">Opportunity Id</param>
        /// <param name="neighborhoodId">Neighborhood Id)</param>
        /// <returns>Financial Calculator Xml</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string LoadFinancialCalculatorXml(object loanProfileId, object opportunityId, object neighborhoodId)
        {
            try
            {
                StringBuilder xmlBuilder = new StringBuilder();
                xmlBuilder.Append("<Alldata>");
                // Set Tax Data
                xmlBuilder.Append(SetTaxRateData());
                // Set Loan Special Fees
                xmlBuilder.Append(SetLoanSpecialFees());
                // Set Loan Profile Data
                xmlBuilder.Append(GetLoanProgramsXml(loanProfileId, opportunityId, neighborhoodId));
                xmlBuilder.Append("</Alldata>");
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Returns the XML for the most recent Loan Special Fees data.
        /// </summary>
        /// <returns>XML string specifying the loan special fee data in the database</returns>
        /// <history>
        /// Revision#     Date        Author      Description
        /// 3.8.0.0       5/12/2006   DYin        Converted to .Net C# code.
        /// </history>
        protected virtual string SetLoanSpecialFees()
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstLoanSpecialFees = objLib.GetRecordset(modOpportunity.strqLATEST_LOAN_SPECIAL_FEE, 0, modOpportunity.strfEFFECTIVE_DATE,
                    modOpportunity.strfFHA_PARTICIPATION_RATE, modOpportunity.strfFHA_MAX_LTV, modOpportunity.strfVA_MAX_LTV);

                StringBuilder xmlBuilder = new StringBuilder();
                xmlBuilder.Append("<SpecialFees");

                if (rstLoanSpecialFees.RecordCount > 0)
                {
                    rstLoanSpecialFees.MoveFirst();
                    xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("FPR", rstLoanSpecialFees.Fields[modOpportunity.strfFHA_PARTICIPATION_RATE].Value));
                    xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("FML", rstLoanSpecialFees.Fields[modOpportunity.strfFHA_MAX_LTV].Value));
                    xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("VML", rstLoanSpecialFees.Fields[modOpportunity.strfVA_MAX_LTV].Value));
                    xmlBuilder.Append(">\r\n");

                    Recordset rstLoanSpecialFeesDet = objLib.GetLinkedRecordset(modOpportunity.strt_LOAN_SPECIAL_FEE_DETAIL, modOpportunity.strfLOAN_SPECIAL_FEE_ID,
                        rstLoanSpecialFees.Fields[modOpportunity.strfLOAN_SPECIAL_FEE_ID].Value, modOpportunity.strfTYPE,
                        modOpportunity.strfPREMIUM_RATE_15, modOpportunity.STRFPREMIUM_RATE, modOpportunity.strfVETERANS_STATUS,
                        modOpportunity.strfUSES, modOpportunity.strfLTV_RATIO, modOpportunity.strfFUNDING_RATE);

                    if (rstLoanSpecialFeesDet.RecordCount > 0)
                    {
                        rstLoanSpecialFeesDet.Sort = modOpportunity.strfTYPE;
                        rstLoanSpecialFeesDet.MoveFirst();
                        while(!(rstLoanSpecialFeesDet.EOF))
                        {
                            if (!(Convert.IsDBNull(rstLoanSpecialFeesDet.Fields[modOpportunity.strfTYPE].Value)))
                            {
                                string strType = TypeConvert.ToString(rstLoanSpecialFeesDet.Fields[modOpportunity.strfTYPE].Value);
                                xmlBuilder.Append("<" + strType);
                                if (strType.ToUpper() == "PMI" || strType.ToUpper() == "MIP")
                                {
                                    xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("LTVR", TypeConvert.ToString(rstLoanSpecialFeesDet.Fields[modOpportunity.strfLTV_RATIO].Value)));
                                    xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("PR15", TypeConvert.ToString(rstLoanSpecialFeesDet.Fields[modOpportunity.strfPREMIUM_RATE_15].Value)));
                                    xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("PR", TypeConvert.ToString(rstLoanSpecialFeesDet.Fields[modOpportunity.STRFPREMIUM_RATE].Value)));
                                }
                                else if (strType.ToUpper() == "VA")
                                {
                                    xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("LTVR", TypeConvert.ToString(rstLoanSpecialFeesDet.Fields[modOpportunity.strfLTV_RATIO].Value)));
                                    xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("VS", rstLoanSpecialFeesDet.Fields[modOpportunity.strfVETERANS_STATUS].Value));
                                    xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("USES", rstLoanSpecialFeesDet.Fields[modOpportunity.strfUSES].Value));
                                    xmlBuilder.Append(new String(' ', 1) + SetValuePairsString("FR", TypeConvert.ToString(rstLoanSpecialFeesDet.Fields[modOpportunity.strfFUNDING_RATE].Value)));
                                }
                                xmlBuilder.Append("/>\r\n");
                            }
                            rstLoanSpecialFeesDet.MoveNext();
                        }
                    }
                }
                else
                {
                    xmlBuilder.Append(">\r\n");
                }
                xmlBuilder.Append("</SpecialFees>");
                return xmlBuilder.ToString();
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Converts an Object to an Id
        /// </summary>
        /// <param name="recordId">Record Id</param>
        /// <returns>Object representation of the record id</returns>
        protected virtual object ObjectToId(object recordId)
        {
            if (recordId == null)
            {
                return DBNull.Value;
            }
            if ((recordId.GetType() == typeof(string)) || (recordId.GetType() == typeof(int)))
            {
                try
                {
                    return TypeConvert.ToString(recordId).Length == 0 ? recordId = DBNull.Value : RSysSystem.StringToId(TypeConvert.ToString(recordId));
                }
                catch
                {
                    if (Share.IsNumeric(recordId))
                    {
                        string binaryId = "0x0000000000000000";
                        string hexValue = TypeConvert.ToInt32(recordId).ToString("X");
                        string firstPart = binaryId.Substring(0, binaryId.Length - hexValue.Length);
                        recordId = firstPart + hexValue;
                        return RSysSystem.StringToId(TypeConvert.ToString(recordId));
                    }
                    else
                        return recordId;
                }
            }
            else
                return recordId;
        }

        /// <summary>
        /// Converts an Id to an Integer
        /// </summary>
        /// <param name="recordId">Record Id</param>
        /// <returns>Integer representation of the record id</returns>
        protected virtual int IdToInteger(object recordId)
        {
            try
            {
                string textRecordId = RSysSystem.IdToString(recordId).Substring(2);
                return Int32.Parse(textRecordId, NumberStyles.AllowHexSpecifier);
            }
            catch
            {
                return 0;
            }
        }
    }
}
