using System;
using System.Collections.Generic;
using System.Text;
using Pivotal.Interop.RDALib;
using Pivotal.Interop.ADODBLib;
using Pivotal.Application.Foundation.Utility;
using Pivotal.Application.Foundation.Data.Element;

namespace CRM.Pivotal.IAC
{
    public class Opportunity : IRFormScript
    {
        #region Class-Level Variables
        private IRSystem7 mrsysSystem = null;

        public IRSystem7 RSysSystem
        {
            get { return mrsysSystem; }
            set { mrsysSystem = value; }
        }

        private ILangDict grldtLangDict = null;

        protected ILangDict RldtLangDict
        {
            get { return grldtLangDict; }
            set { grldtLangDict = value; }
        }
        #endregion

        public virtual void SetSystem(RSystem pSystem)
        {
            try
            {
                RSysSystem = (IRSystem7)pSystem;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        public virtual object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                object[] recordsetArray = (object[])Recordsets;
                Recordset rstPrimary = (Recordset)recordsetArray[0];

                object vntRecordId = pForm.DoAddFormData(Recordsets, ref ParameterList);

                return vntRecordId;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        public virtual void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                pForm.DoDeleteFormData(RecordId, ref ParameterList);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {
            try
            {
                TransitionPointParameter objrInstance = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objrInstance.ParameterList = ParameterList;

                if (objrInstance.HasValidParameters() == false)
                {
                    objrInstance.Construct();
                }

                object[] parameterArray = objrInstance.GetUserDefinedParameterArray();

                switch (MethodName)
                {
                    case "":
                        break;
                    default:
                        string message = MethodName + TypeConvert.ToString(RldtLangDict.GetText(modOpportunity.strdINVALID_METHOD));
                        parameterArray = new object[] { message };
                        throw new PivotalApplicationException(message, modOpportunity.glngERR_METHOD_NOT_DEFINED);
                }

                ParameterList = objrInstance.SetUserDefinedParameterArray(parameterArray);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        //Change History:
        //Ver:  By: Date:       Desc:
        //---------------------------------------------------
        //
        public virtual object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                object vntRecordset = pForm.DoLoadFormData(RecordId, ref ParameterList);
                object[] recordsetArray = (object[])vntRecordset;

                // checking and seting of the system parameters
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;

                if (!(objParam.HasValidParameters()))
                {
                    objParam.Construct();
                }

                return vntRecordset;
            }
            catch (Exception exc)
            {
                //throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
                throw new PivotalApplicationException(exc.Message, true);
            }
        }

        public virtual object NewFormData(IRForm pForm, ref object ParameterList)
        {
            try
            {
                object vntRecordset = pForm.DoNewFormData(ref ParameterList);
                object[] recordsetArray = (object[])vntRecordset;
                Recordset rstRecordset = (Recordset)recordsetArray[0];

                TransitionPointParameter objrParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objrParam.ParameterList = ParameterList;

                if (objrParam.HasValidParameters() == false)
                {
                    objrParam.Construct();
                }
                else
                {
                    objrParam.SetDefaultFields(rstRecordset);
                }

                return vntRecordset;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        public virtual void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset Recordset)
        {
            try
            {
                pForm.DoNewSecondaryData(SecondaryName, ref ParameterList, Recordset);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        //Change History:
        //Ver:  By: Date:       Desc:
        //---------------------------------------------------

        public virtual void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                TransitionPointParameter objrParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objrParam.ParameterList = ParameterList;

                object[] recordsetArray = (object[])Recordsets;
                Recordset rstPrimary = (Recordset)recordsetArray[0];

                //Save the data
                pForm.DoSaveFormData(Recordsets, ref ParameterList);

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

    }


}
