using System;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;

using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;

namespace CRM.Pivotal.IP
{        
    /// <summary>
    /// This module implements the business logic in the PHb
    /// </summary>
    // Contact Web Details object.
    // The Contact Web details form contains information about the contact
    // for the purpose of logging into PartnerHub or CustomerHub.
    // It contains the user's login name and password, as well as access information.
    // Only one Contact Web Details record is allowed per Contact record.
    // There should never be duplicate combinations of login name and password.
    // Revision# Date Author Description
    // 3.8.0.0   5/8/2006  svadivu  Converted to .Net C# code.
 
    public class WebDetails : IRFormScript
    {
        private IRSystem7 mrsysSystem = null;

        protected IRSystem7 RSysSystem
        {
            get { return mrsysSystem; }
            set { mrsysSystem = value; }
        }

        /// <summary>
        /// This function checks whether there is any existing Contact Web Details record
        /// with the same Password and Login Id. Assumptions: Queries strqCWD_WITH_LOGIN_AND_PASSWORD and
        /// strqCWD_WITH_LOGIN_AND_PASSWORD_AND_ID are defined.
        /// </summary>
        /// <param name="strfLogin">Login string</param>
        /// <param name="strfPassword">Password string</param>
        /// <param name="vntfWebDetails_Id">Contact Web Details Id</param>
        /// <returns>
        /// Login and Password; False if there is no existing record.</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/8/2006  svadivu Converted to .Net C# code.
        /// </history>
        protected virtual bool HasDuplicates(object strfLogin, object strfPASSWORD, object vntfWebDetails_Id)
        {
            try
            {
                bool bHasDuplicates = false;
                Recordset rstWebDetails = null;
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                if (Convert.IsDBNull(vntfWebDetails_Id))
                {
                    rstWebDetails=objLib.GetRecordset(modContact.strqCWD_WITH_LOGIN_AND_PASSWORD,2,strfLogin,strfPASSWORD);

                }
                else
                {
                    rstWebDetails=objLib.GetRecordset(modContact.strqCWD_WITH_LOGIN_AND_PASSWORD_AND_ID,3,strfLogin,strfPASSWORD,vntfWebDetails_Id);
                }

                if (!(rstWebDetails.EOF))
                {
                    bHasDuplicates = true;
                }
                else
                {
                    bHasDuplicates = false;
                }
                return bHasDuplicates;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks whether the Contact Web Details record about to be saved
        /// </summary>
        /// belongs to a Company and whether it has any duplicates with the same Password and Login Id.
        /// <param name="vntfCompany_Id">Company Id of the Contact record that the Contact Web</param>
        // Details record comes from
        /// <returns>
        /// enmSaveStatus - Signifies whether it is okay to save the record.</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/8/2006  svadivu Converted to .Net C# code.
        /// </history>
        protected virtual int CheckOKToSave(Recordset rstWebDetails, object vntfCompany_Id)
        {
            int intCheckOKToSave;
            try
            {
                if (Convert.IsDBNull(vntfCompany_Id))
                {
                    intCheckOKToSave =(int) enmSaveStatus.intNoCompany;
                }
                else
                {
                    if (this.HasDuplicates(rstWebDetails.Fields[modContact.strfLOGIN_NAME].Value, rstWebDetails.Fields[modContact.strfPASSWORD_ENCRYPT].Value, rstWebDetails.Fields[modContact.strfCONTACT_WEB_DETAILS_ID].Value)
                        == true)
                    {
                        intCheckOKToSave =(int) enmSaveStatus.intDuplicateFound;
                    }
                    else
                    {
                        intCheckOKToSave = (int)enmSaveStatus.intOKToSave;
                    }
                }
                return intCheckOKToSave;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function gets the Company_Id field value of the Contact record
        /// </summary>
        /// with the provided Contact Id.
        /// <param name="vntfContact_Id">Id of the Contact Record for which the Company_Id is being fetched</param>
        /// <returns>
        /// The Company_Id value of the Contact record.</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/8/2006  svadivu Converted to .Net C# code.
        /// </history>
        protected virtual object GetCompanyId(object vntfContact_Id)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstContact = objLib.GetRecordset(modContact.strtCONTACT, modContact.strfCONTACT_ID, modContact.strfCOMPANY_ID);
                return rstContact.Fields[modContact.strfCOMPANY_ID].Value;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function adds a new Contact Web Details record.
        /// </summary>
        /// <returns>
        /// The record Id of the newly added record.
        /// Implements Agent: OnSave(Add)</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/8/2006  svadivu Converted to .Net C# code.
        /// </history>
        public virtual object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                object[] recordsetArray = (object[])Recordsets;
                Recordset rstWebDetails = (Recordset)recordsetArray[0];
                // checking and seting of the system parameters
                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;
                if (ocmsTransitPointParams.HasValidParameters() == false)
                {
                    ocmsTransitPointParams.Construct();
                }
                else
                {
                    ocmsTransitPointParams.SetDefaultFields(rstWebDetails);
                    ocmsTransitPointParams.WarningMessage = string.Empty;
                    ParameterList = ocmsTransitPointParams.ParameterList;

                }

                object vntfCompany_Id = this.GetCompanyId(rstWebDetails.Fields[modContact.strfCONTACT_ID].Value);
                int intCheckOK = this.CheckOKToSave(rstWebDetails, vntfCompany_Id);

                if (intCheckOK ==(int) enmSaveStatus.intNoCompany)
                {
                    throw new PivotalApplicationException(modContact.strldstrCWD_NO_COMPANY,true);//,modContact.lngERR_CWD_NO_COMPANY);
                }
                else if (intCheckOK ==(int) enmSaveStatus.intDuplicateFound)
                {
                    throw new PivotalApplicationException(modContact.strldstrCWD_DUPLICATE_FOUND,true);//modContact.strgCONTACT), modContact.lngERR_CWD_DUPLICATE_FOUND);
                }
                else if (intCheckOK == (int)enmSaveStatus.intOKToSave)
                {
                    // nothing
                }

                return pForm.DoAddFormData(Recordsets, ref ParameterList);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function is not implemented in the Web Details object.
        /// </summary>
        /// Default behavior is specified by the corresponding function in the IntraHub AppServer Services.
        /// <param name="pForm">Contact Web Details IRForm object</param>
        /// <param name="RecordId">Contact Web Details Id</param>
        /// <param name="ParameterList">Transition Point Parameters</param>
        /// <returns>
        /// None
        /// Implements Agent: OnDelete</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/8/2006  svadivu Converted to .Net C# code.
        /// </history>
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

        /// <summary>
        /// This function is not implemented in the Web Details object.
        /// </summary>
        /// Default behavior is specified by the corresponding function in the IntraHub AppServer Services.
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/8/2006  svadivu Converted to .Net C# code.
        /// </history>
        public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {
            try
            { }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function loads an existing Contact Web Details record.
        /// </summary>
        /// <param name="pForm">Contact Web Details IRForm object</param>
        /// <param name="RecordId">Contact Web Details Id of the record to be loaded</param>
        /// <param name="ParameterList">Transition Point Parameters</param>
        /// <returns>
        /// An array of Contact Web Details recordsets.
        /// Implements Agent: OnOpen(Modify)</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/8/2006  svadivu Converted to .Net C# code.
        /// </history>
        public virtual object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                object Recordsets = pForm.DoLoadFormData(RecordId, ref ParameterList);
                object[] recordsetArray = (object[]) Recordsets;
                Recordset rstWebDetails = (Recordset)recordsetArray[0];

                // checking and seting of the system parameters
                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;
                if (ocmsTransitPointParams.HasValidParameters() == false)
                {
                    ocmsTransitPointParams.Construct();
                }
                else
                {
                    ocmsTransitPointParams.SetDefaultFields(rstWebDetails);
                    ocmsTransitPointParams.WarningMessage = string.Empty;
                    ParameterList = ocmsTransitPointParams.ParameterList;

                }
                return Recordsets;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function creates a new Contact Web Details record.
        /// </summary>
        /// <param name="pForm">Contact Web Details IRForm object</param>
        /// <param name="ParameterList">Transition Point Parameters</param>
        /// <returns>
        /// An array of empty Contact Web Details recordsets.
        /// Implements Agent: OnOpen(Add)</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/8/2006  svadivu Converted to .Net C# code.
        /// </history>
        public virtual object NewFormData(IRForm pForm, ref object ParameterList)
        {
            try
            {

                object vntWebDetailsRst = pForm.DoNewFormData(ref ParameterList);
                object[] recordsetArray = (object[]) vntWebDetailsRst;
                Recordset rstWebDetails = (Recordset)recordsetArray[0];
                rstWebDetails.Fields[modContact.strfCONTACT_ID].Value = System.DBNull.Value;

                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;

                if (ocmsTransitPointParams.HasValidParameters() == false)
                {
                    ocmsTransitPointParams.Construct();
                }
                else
                {
                    ocmsTransitPointParams.SetDefaultFields(rstWebDetails);
                    ocmsTransitPointParams.WarningMessage = string.Empty;
                    ParameterList = ocmsTransitPointParams.ParameterList;
                }
                return vntWebDetailsRst;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function is not implemented in the Web Details object.
        /// </summary>
        /// Default behavior is specified by the corresponding function in the IntraHub AppServer Services.
        /// <param name="rfrmForm">Contact Web Details IRForm object</param>
        /// <param name="SecondaryName">Secondary segment name</param>
        /// <param name="ParameterList">Transition Point Parameters</param>
        /// <param name="Recordset">Secondary recordset just created</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/8/2006  svadivu Converted to .Net C# code.
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
        /// This function saves a modified Contact Web Details record.
        /// </summary>
        /// <returns>
        /// None
        /// Implements Agent: OnSave(Modify)</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/8/2006  svadivu Converted to .Net C# code.
        /// </history>
        public virtual void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                object[] recordsetArray=(object[]) Recordsets;
                Recordset rstWebDetails =(Recordset) recordsetArray[0];
                // checking and seting of the system parameters
                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;
                if (ocmsTransitPointParams.HasValidParameters() == false)
                {
                    ocmsTransitPointParams.Construct();
                }
                else
                {
                    ocmsTransitPointParams.SetDefaultFields(rstWebDetails);
                    ocmsTransitPointParams.WarningMessage = string.Empty;
                    ParameterList = ocmsTransitPointParams.ParameterList;

                }

                object vntfCompany_Id = this.GetCompanyId(rstWebDetails.Fields[modContact.strfCONTACT_ID].Value);
                string intCheckOK = TypeConvert.ToString(this.CheckOKToSave(rstWebDetails, vntfCompany_Id));

                if (intCheckOK == TypeConvert.ToString(enmSaveStatus.intNoCompany))
                {
                    throw new PivotalApplicationException(modContact.strldstrCWD_NO_COMPANY,true);// modContact.strgCONTACT),modContact.lngERR_CWD_NO_COMPANY);
                }
                else if (intCheckOK == TypeConvert.ToString(enmSaveStatus.intDuplicateFound))
                {
                    throw new PivotalApplicationException(modContact.strldstrCWD_DUPLICATE_FOUND,true);// modContact.strgCONTACT), modContact.lngERR_CWD_DUPLICATE_FOUND);
                }
                else if (intCheckOK == TypeConvert.ToString(enmSaveStatus.intOKToSave))
                {
                    // nothing
                }

                pForm.DoSaveFormData(Recordsets, ref ParameterList);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This procedure sets the IRSystem7 object and the global variables
        /// </summary>
        /// mrsysSystem, mocmsErrors and mrldtLangDict.
        /// <param name="pSystem">IRSystem object passed by the AppServer Services</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision # Date            Author  Description
        /// 3.8.0.0  5/8/2006  svadivu Converted to .Net C# code.
        /// </history>
        public virtual void SetSystem(RSystem pSystem)
        {
            try
            {
                RSysSystem = (IRSystem7) pSystem;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        
        protected enum enmSaveStatus 
        {
            intOKToSave = 0,
            intNoCompany = 1,
            intDuplicateFound = 2,
        }
    }

}
