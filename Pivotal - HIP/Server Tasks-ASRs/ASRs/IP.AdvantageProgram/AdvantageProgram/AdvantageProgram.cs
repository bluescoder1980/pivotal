using System;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;

using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;
using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server;


namespace CRM.Pivotal.IP
{
    public class AdvantageProgram : IRFormScript
    {
        /// <summary>
        /// This class represent Advantage Program Code business object
        /// </summary>

        //private const string 
        private IRSystem7 mrsysSystem = null;

        protected IRSystem7 RSysSystem
        {
            get { return mrsysSystem; }
            set { mrsysSystem = value; }
        }

        // Language Resources
        private ILangDict grldtLangDict = null;

        protected ILangDict RldtLangDict
        {
            get { return grldtLangDict; }
            set { grldtLangDict = value; }
        }

        /// <summary>
        /// Add a record.
        /// </summary>
        /// <param name="pForm">The IRForm object reference to the client IRForm object</param>
        /// <param name="Recordsets">
        /// Hold the reference for the current primary recordset and its all secondaries in the specified form
        /// </param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// IRFormScript_AddFormData - Return information to IRSystem</returns>
        /// <history>
        /// Revision#     Date              Author      Description
        /// 5.9.0.0       July 28 2010      CMigles     Initial Version
        /// </history>
        public virtual object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                object vntRecordId = pForm.DoAddFormData(Recordsets, ref ParameterList);


                return vntRecordId;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Delete a record.
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="RecordId">The business object record Id</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date              Author      Description
        /// 5.9.0.0       July 28 2010      CMigles     Initial Version
        /// </history>
        public virtual void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                // check for secondaries.
                //if (CanBeDeleted(RecordId, ref ParameterList))
                //{
                    pForm.DoDeleteFormData(RecordId, ref ParameterList);
                //}
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Execute a specified method
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="MethodName">The method name to be executed</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// ParameterList - Return executed result</returns>
        /// <history>
        /// Revision#     Date              Author      Description
        /// 5.9.0.0       July 28 2010      CMigles     Initial Version
        /// </history>
        public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {
            object vntReturnValue = null;

            try
            {
                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;

                // Dump out the user defined parameters
                object[] parameterArray = objParam.GetUserDefinedParameterArray();

                object vntQuoteContractId = DBNull.Value;

                switch (MethodName)
                {
                    case modAdvantageProgram.strmADVANTAGE_PROGRAM_RESERVE:
                        // RESERVE process
                        vntQuoteContractId = AdvantageProgramReserve(parameterArray[0], parameterArray[1], parameterArray[2]);
                        parameterArray = new object[]{vntQuoteContractId};
                        break;
                    case modAdvantageProgram.strmADVANTAGE_PROGRAM_SALE:
                        // RESERVE process
                        vntQuoteContractId = AdvantageProgramSale(parameterArray[0], parameterArray[1], parameterArray[2]);
                        parameterArray = new object[]{vntQuoteContractId};
                        break;
                    default:
                        vntReturnValue = MethodName + TypeConvert.ToString(RldtLangDict.GetText(modAdvantageProgram.strdINVALID_METHOD));
                        parameterArray = new object [] { vntReturnValue };
                        throw new PivotalApplicationException((string)vntReturnValue, (int)modAdvantageProgram.glngERR_METHOD_NOT_DEFINED);
                }

                // Add the returned values into transit point parameter list
                ParameterList = objParam.SetUserDefinedParameterArray(parameterArray);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Load a record.
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="RecordId">The Generic Code Id</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// IRFormScript_LoadFormData  - The form data</returns>
        /// <history>
        /// Revision#     Date              Author      Description
        /// 5.9.0.0       July 28 2010      CMigles     Initial Version
        /// </history>
        public virtual object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                object vntRecordset = pForm.DoLoadFormData(RecordId, ref ParameterList);
                object[] recordsetArray = (object[])vntRecordset;
                Recordset rstPrimary = (Recordset)recordsetArray[0];
                // checking and seting of the system parameters
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;

                if (!(objParam.HasValidParameters()))
                {
                    objParam.Construct();
                }
      
                vntRecordset = pForm.DoLoadFormData(RecordId, ref ParameterList);

                return vntRecordset;
            }
            catch (Exception exc)
            {
                //throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
                throw new PivotalApplicationException(exc.Message, true);
            }
        }

        /// <summary>
        /// This function load a new GenericCode record
        /// </summary>
        /// <param name="pForm">The IRform object reference to the client IRForm object</param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// IRFormScript_NewFormData   - Returned information</returns>
        /// <history>
        /// Revision#     Date              Author      Description
        /// 5.9.0.0       July 28 2010      CMigles     Initial Version
        /// </history>
        public virtual object NewFormData(IRForm pForm, ref object ParameterList)
        {
            try
            {
                object vntForm = pForm.DoNewFormData(ref ParameterList);
                object[] recordsetArray = (object[])vntForm;
                Recordset rstPrimary = (Recordset)recordsetArray[0];

                // checking and setting of the system parameters
                TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                objParam.ParameterList = ParameterList;

                if (objParam.HasValidParameters() == false)
                {
                    objParam.Construct();
                }
                else
                {
                    objParam.SetDefaultFields(rstPrimary);
                    objParam.WarningMessage = string.Empty;
                }

                ParameterList = objParam.ParameterList;

                return vntForm;
            }
            catch (Exception exc)
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
        /// <returns></returns>
        /// <history>
        /// Revision#     Date              Author      Description
        /// 5.9.0.0       July 28 2010      CMigles     Initial Version
        /// </history>
        public virtual void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset
            recordset)
        {
            try
            {
                pForm.DoNewSecondaryData(SecondaryName, ref ParameterList, recordset);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function updates the GenericCode plan
        /// </summary>
        /// <param name="pForm">The IRForm object reference to the client IRForm object</param>
        /// <param name="Recordsets">
        /// Hold the reference for the current primary recordset and its all secondaries in the specified form
        /// </param>
        /// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date              Author      Description
        /// 5.9.0.0       July 28 2010      CMigles     Initial Version
        /// </history>
        public virtual void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {

                object[] recordsetArray = (object[])Recordsets;
                Recordset rstAdvantageProgram = (Recordset)recordsetArray[0];
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
          
                pForm.DoSaveFormData(Recordsets, ref ParameterList);

                //MANAGE CO-BUYERS CHANGES.
                if (rstAdvantageProgram.RecordCount > 0)
                {
                    ManageCoBuyers(rstAdvantageProgram);
                }

            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Public method to get current IRSystem7 reference
        /// </summary>
        /// <param name="pSystem">Hold the current System Instance Reference</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#     Date              Author      Description
        /// 5.9.0.0       July 28 2010      CMigles     Initial Version
        /// </history>
        public virtual void SetSystem(RSystem pSystem)
        {
            try
            {
                RSysSystem = (IRSystem7)pSystem;
                RldtLangDict = RSysSystem.GetLDGroup(modAdvantageProgram.strgAdvantageProgram);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

  

        /// <summary>
        /// This function checks to see if a secondary exists.
        /// </summary>
        /// <param name="pForm">IRForm object</param>
        /// <param name="recordsetArray">Form collection</param>
        /// <param name="strSection">Section name</param>
        /// <param name="strItem">Section where item was found</param>
        /// <returns>
        /// True if a secondary was found
        /// False if no secondary was found
        /// </returns>
        /// <history>
        /// Revision#     Date              Author      Description
        /// 5.9.0.0       July 28 2010      CMigles     Initial Version
        /// </history>
        protected virtual bool SecondaryExists(IRForm pForm, object recordsetArray, string strSection, ref object strItem)
        {
            try
            {
                bool SecondaryExists = false;
                Recordset rstFormSecondary = pForm.SecondaryFromVariantArray(recordsetArray, strSection);

                if (rstFormSecondary.RecordCount > 0)
                {
                    strItem = strSection;
                    SecondaryExists = true;
                }

                return SecondaryExists;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This funtion will initiate the reservation process from the AP form.
        /// </summary>
        /// <param name="vntAdvantageProgramId">Advantage Program Id</param>
        /// <param name="dtSaleDate">Reservation Date</param>
        /// <param name="vntLotId">Lot Id</param>
        /// <returns>True</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 1.0          07/20/2010   CMigles  Initial Version
        /// </history>
        public virtual object AdvantageProgramReserve(object vntAdvantageProgramId, object dtReservationDate, object vntLotId)
        {
            try
            {
                object vntNewQuoteContractId = DBNull.Value;
                object vntInventoryQuoteId = DBNull.Value;

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Administration administration = (Administration)RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                Opportunity ObjOpportunity = (Opportunity)RSysSystem.ServerScripts[modAdvantageProgram.strsASR_OPPORTUNITY].CreateInstance();

                object vntContactId= (objLib.SqlIndex(modAdvantageProgram.strtTIC_PROJECT_REGISTRATION, modAdvantageProgram.strfTIC_CONTACT_ID, vntAdvantageProgramId));
                object vntNeighborhoodId = (objLib.SqlIndex(modAdvantageProgram.strtTIC_PROJECT_REGISTRATION, modAdvantageProgram.strfTIC_NEIGHBORHOOD_ID, vntAdvantageProgramId));
                object vntCoBuyerId = objLib.SqlIndex(modAdvantageProgram.strtTIC_PROJECT_REGISTRATION, modAdvantageProgram.strfTIC_COBUYER_CONTACT_ID, vntAdvantageProgramId);
                object vntECOE= objLib.SqlIndex(modAdvantageProgram.strtTIC_PROJECT_REGISTRATION, modAdvantageProgram.strfECOE_DATE, vntAdvantageProgramId);
                
                if (vntLotId != null)
                {
                    //Get the Inventory quote for the Lot, make a copy and set the Quote Contract record for the Lot.
                    //AM2010.12.08 - Fixed issue where reservation was not getting the ECOE date from IQ
                    Recordset rstInvQuote = objLib.GetRecordset(modAdvantageProgram.strqHB_INVENTORY_QUOTE_FOR_INVENTORY_HOME, 1, vntLotId, 
                                                    modAdvantageProgram.strfOPPORTUNITY_ID, modAdvantageProgram.strfINACTIVE, modAdvantageProgram.strfECOE_DATE);
                                        

                    if (rstInvQuote.RecordCount > 0)
                    {
                        vntInventoryQuoteId = rstInvQuote.Fields[modAdvantageProgram.strfOPPORTUNITY_ID].Value;
                        if (vntInventoryQuoteId != null)
                        {
                            object dtECOE = rstInvQuote.Fields[modAdvantageProgram.strfECOE_DATE].Value;

                            //Call the CopyQuote function from Opportunity assembly. 
                            vntNewQuoteContractId = ObjOpportunity.CopyQuote(vntInventoryQuoteId, true, false, false);

                            //Get the new quote contract and update it with buyer's and reservation information.
                            if (vntNewQuoteContractId != null)
                            {
                                Recordset rstNewQuoteContract = objLib.GetRecordset(vntNewQuoteContractId, modAdvantageProgram.strtOPPORTUNITY, modAdvantageProgram.strfOPPORTUNITY_ID,
                                                   modAdvantageProgram.strfCONTACT_ID, modAdvantageProgram.strfSTATUS, modAdvantageProgram.strfRESERVATION_DATE, modAdvantageProgram.strfPIPELINE_STAGE,
                                                   modAdvantageProgram.strfTIC_CO_BUYER_ID, modAdvantageProgram.strfECOE_DATE, modAdvantageProgram.strfACCOUNT_MANAGER_ID,
                                                   modAdvantageProgram.strfQUOTE_TOTAL, modAdvantageProgram.strfPLAN_NAME_ID,
                                                   modAdvantageProgram.strfELEVATION_ID);
                                if (rstNewQuoteContract.RecordCount > 0)
                                {
                                    rstNewQuoteContract.Fields[modAdvantageProgram.strfSTATUS].Value = modAdvantageProgram.strsRESERVED;
                                    rstNewQuoteContract.Fields[modAdvantageProgram.strfRESERVATION_DATE].Value = dtReservationDate;
                                    rstNewQuoteContract.Fields[modAdvantageProgram.strfCONTACT_ID].Value = vntContactId;
                                    rstNewQuoteContract.Fields[modAdvantageProgram.strfTIC_CO_BUYER_ID].Value = vntCoBuyerId;

                                    //AM2010.12.08 - set ECOE from IQ instead of AP record
                                    rstNewQuoteContract.Fields[modAdvantageProgram.strfECOE_DATE].Value = TypeConvert.ToDateTime(dtECOE);
                                    rstNewQuoteContract.Fields[modAdvantageProgram.strfACCOUNT_MANAGER_ID].Value = administration.CurrentUserRecordId;

                                    //Save recordset
                                    objLib.PermissionIgnored = true;
                                    objLib.SaveRecordset(modAdvantageProgram.strtOPPORTUNITY, rstNewQuoteContract);

                                    
                                    //Call Send Email Notification
                                    SendReserveNotification(modAdvantageProgram.strsRESERVED, vntNeighborhoodId, rstNewQuoteContract, vntContactId);

                                    //Close all Recordsets 
                                    rstNewQuoteContract.Close();


                                    //Set Co-buyers from AP to Lot and Quote Contract
                                    SetLotQuoteContractCobuyers(vntLotId, vntNewQuoteContractId, vntAdvantageProgramId);
                                    //Set Reservation Status
                                    ObjOpportunity.UpdateReservationStatus(vntContactId, vntNeighborhoodId, DateTime.Today, vntLotId, vntNewQuoteContractId);

                                    //Inactivate the original Inventory Quote
                                    rstInvQuote.Fields[modAdvantageProgram.strfINACTIVE].Value = true;
                                    objLib.SaveRecordset(modAdvantageProgram.strtOPPORTUNITY, rstInvQuote);
                                    rstInvQuote.Close();

                                }
                            }
                        }
                    }
                    else
                    {
                        throw new PivotalApplicationException("AdvantageProgramReserve() - Missing Inventory Quote for the selected lot.");
                    }
                }   



                return vntNewQuoteContractId ;
            }

            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This funtion will initiate the sale process from the AP form.
        /// </summary>
        /// <param name="vntAdvantageProgramId">Advantage Program Id</param>
        /// <param name="dtSaleDate">Sales Date</param>
        /// <param name="vntLotId">Lot Id</param>
        /// <returns>True</returns>
        /// <history>
        /// Revision #   Date         Author   Description
        /// 1.0          07/20/2010   CMigles  Initial Version
        /// </history>
        public virtual object AdvantageProgramSale(object vntAdvantageProgramId, object dtSaleDate, object vntLotId)
        {
            try
            {
                object vntQuoteContractId = DBNull.Value;
                object vntInventoryQuoteId = DBNull.Value;
                

                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                DataAccess objLibs = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Opportunity ObjOpportunity = (Opportunity)RSysSystem.ServerScripts[modAdvantageProgram.strsASR_OPPORTUNITY].CreateInstance();

                if (vntLotId != null)
                {
                    //Find the Lot Status, to find out if a Quote Contract has been created or not. 
                    string strLotStatus = TypeConvert.ToString(objLib.SqlIndex(modAdvantageProgram.strtPRODUCT , modAdvantageProgram.strfLOT_STATUS, vntLotId));

                    if (strLotStatus == modAdvantageProgram.strsAVAILABLE)
                    {
                        //Get the Inventory quote for the Lot, make a copy and set the Quote Contract record for the Lot.
                        Recordset rstInvQuote = objLib.GetRecordset(modAdvantageProgram.strqHB_INVENTORY_QUOTE_FOR_INVENTORY_HOME, 1, vntLotId, 
                                                    modAdvantageProgram.strfOPPORTUNITY_ID, modAdvantageProgram.strfINACTIVE);
                        if (rstInvQuote.RecordCount > 0)
                        {
                            vntInventoryQuoteId = rstInvQuote.Fields[modAdvantageProgram.strfOPPORTUNITY_ID].Value;
                            if (vntInventoryQuoteId != null)
                            {
                                //Call the CopyQuote function from Opportunity assembly. 
                                vntQuoteContractId = ObjOpportunity.CopyQuote(vntInventoryQuoteId, true, false, false);

                                //Inactivate the original Inventory Quote
                                rstInvQuote.Fields[modAdvantageProgram.strfINACTIVE].Value = true;
                                objLibs.SaveRecordset(modAdvantageProgram.strtOPPORTUNITY, rstInvQuote);
                                rstInvQuote.Close();

                                if (vntQuoteContractId != null)
                                {
                                    Recordset  rstQuoteContract = objLib.GetRecordset(vntQuoteContractId, modAdvantageProgram.strtOPPORTUNITY, modAdvantageProgram.strfOPPORTUNITY_ID, 
                                                                                modAdvantageProgram.strfECOE_DATE, modAdvantageProgram.strfCONTACT_ID, modAdvantageProgram.strfTIC_CO_BUYER_ID);
                                    if (rstQuoteContract.RecordCount > 0) 
                                    {
                                        rstQuoteContract.MoveFirst();
                                        //Get the ECOE & Buyer date from AP form and update the quote contract.
                                        object dtECOE = TypeConvert.ToDateTime(objLib.SqlIndex(modAdvantageProgram.strtTIC_PROJECT_REGISTRATION, modAdvantageProgram.strfECOE_DATE, vntAdvantageProgramId));
                                        object vntBuyerId = objLib.SqlIndex(modAdvantageProgram.strtTIC_PROJECT_REGISTRATION, modAdvantageProgram.strfTIC_CONTACT_ID, vntAdvantageProgramId);
                                        object vntCoBuyerId = objLib.SqlIndex(modAdvantageProgram.strtTIC_PROJECT_REGISTRATION, modAdvantageProgram.strfTIC_COBUYER_CONTACT_ID, vntAdvantageProgramId);

                                        rstQuoteContract.Fields[modAdvantageProgram.strfECOE_DATE].Value = dtECOE;
                                        rstQuoteContract.Fields[modAdvantageProgram.strfCONTACT_ID].Value = vntBuyerId;
                                        rstQuoteContract.Fields[modAdvantageProgram.strfTIC_CO_BUYER_ID].Value = vntCoBuyerId;

                                         //Save recordset
                                        objLib.PermissionIgnored = true;
                                        objLib.SaveRecordset(modAdvantageProgram.strtOPPORTUNITY, rstQuoteContract);
                                        rstQuoteContract.Close();

                                        //Set Co-buyers from AP to Lot and Quote Contract
                                        SetLotQuoteContractCobuyers(vntLotId, vntQuoteContractId, vntAdvantageProgramId);

                                        //Get the new quote contract and update it with buyer's and reservation information.
                                        ObjOpportunity.ConvertToSale(vntQuoteContractId, false);

                                        ////Inactivate the original Inventory Quote
                                        //rstInvQuote.Fields[modAdvantageProgram.strfINACTIVE].Value = true;
                                        //objLibs.SaveRecordset(modAdvantageProgram.strtOPPORTUNITY, rstInvQuote);
                                        //rstInvQuote.Close();

                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (strLotStatus == modAdvantageProgram.strsRESERVED)
                        {
                            //Query for the Quote Contract  = In progress
                            Recordset rstQuoteContract = objLib.GetRecordset(modAdvantageProgram.strqTIC_RESERVED_QUOTE_CONTRACT_FOR_LOT, 1, vntLotId, modAdvantageProgram.strfOPPORTUNITY_ID, modAdvantageProgram.strfECOE_DATE);
                            if (rstQuoteContract.RecordCount > 0)
                            {
                                rstQuoteContract.MoveFirst();

                                //Get the ECOE date and update the quote contract.
                                vntQuoteContractId = rstQuoteContract.Fields[modAdvantageProgram.strfOPPORTUNITY_ID].Value;
                                object dtECOE = TypeConvert.ToDateTime(objLib.SqlIndex (modAdvantageProgram.strtTIC_PROJECT_REGISTRATION, modAdvantageProgram.strfECOE_DATE, vntAdvantageProgramId));
                                rstQuoteContract.Fields[modAdvantageProgram.strfECOE_DATE].Value = dtECOE;

                                //Save recordset
                                objLib.PermissionIgnored = true;
                                objLib.SaveRecordset(modAdvantageProgram.strtOPPORTUNITY, rstQuoteContract);
                                rstQuoteContract.Close();

                                //Set Co-buyers from AP to Lot and Quote Contract
                                SetLotQuoteContractCobuyers(vntLotId, vntQuoteContractId, vntAdvantageProgramId);

                                //Call Opportunity Convert to Sale
                                if (vntQuoteContractId != null)
                                {
                                    ObjOpportunity.ConvertToSale(vntQuoteContractId, false);
                                }
                            }
                        }
                    }
                }

                return vntQuoteContractId ;
            }

            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#   Date            Author   Description
        /// 1.0         09/02/2010      CMigles  Initial Version
        /// </history>
        public virtual void SetLotQuoteContractCobuyers(object lotId, object opportunityQuoteId, object advantageProgramId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstContactCobuyer = objLib.GetRecordset(modAdvantageProgram.strqTIC_COBUYERS_FOR_ADVANTAGE_PROGRAM_ID,1, advantageProgramId, 
                                                    modAdvantageProgram.strfTIC_OPPORTUNITY_ID, modAdvantageProgram.strfTIC_PRODUCT_ID);
                if (rstContactCobuyer.RecordCount > 0)
                {
                    rstContactCobuyer.MoveFirst();
                    while (!(rstContactCobuyer.EOF))
                    {
                        rstContactCobuyer.Fields[modAdvantageProgram.strfTIC_OPPORTUNITY_ID].Value = opportunityQuoteId ;
                        rstContactCobuyer.Fields[modAdvantageProgram.strfTIC_PRODUCT_ID].Value = lotId;
                        rstContactCobuyer.MoveNext();
                    }
                    objLib.PermissionIgnored = true;
                    objLib.SaveRecordset(modAdvantageProgram.strtCONTACT_COBUYER, rstContactCobuyer);
                    rstContactCobuyer.Close();
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>None</returns>
        /// <history>
        /// Revision#   Date            Author   Description
        /// 1.0         09/21/2010      CMigles  Initial Version
        /// </history>
        public virtual void ManageCoBuyers(Recordset rstAdvantageProgram)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                object vntAdvantageProgramId = rstAdvantageProgram.Fields[modAdvantageProgram.strfTIC_PROJECT_REGISTRATION_ID].Value;
                object vntBuyerId = rstAdvantageProgram.Fields[modAdvantageProgram.strfTIC_CONTACT_ID].Value;
                object vntCoBuyerId = rstAdvantageProgram.Fields[modAdvantageProgram.strfTIC_COBUYER_CONTACT_ID].Value;
                object vntLotId = rstAdvantageProgram.Fields[modAdvantageProgram.strfTIC_LOT_ID].Value;
                
                if (vntLotId != null)
                {
                    //GET THE RESERVATION CONTRACT ID FOR THE SELECTED LOT
                    object vntReservationContractId = (objLib.SqlIndex(modAdvantageProgram.strtPRODUCT, modAdvantageProgram.strfRESERVATION_CONTRACT_ID, vntLotId));
                    string strLotStatus = (TypeConvert.ToString(objLib.SqlIndex(modAdvantageProgram.strtPRODUCT, modAdvantageProgram.strfLOT_STATUS, vntLotId)));

                    //UPDATE CO-BUYES ONLY IF THE LOT STATUS != CLOSED
                    if (strLotStatus != modAdvantageProgram.strsCLOSED)
                    {
                        
                        //UPDATE MAIN BUYER AND COBUYER FIELDS FOR THE LOT
                        Recordset rstlot = objLib.GetRecordset(vntLotId, modAdvantageProgram.strtPRODUCT, modAdvantageProgram.strfTIC_CO_BUYER_ID, modAdvantageProgram.strfOWNER_ID);
                        if (rstlot.RecordCount > 0)
                        {
                            rstlot.MoveFirst();

                            if (RSysSystem.EqualIds(rstlot.Fields[modAdvantageProgram.strfOWNER_ID].Value, vntBuyerId))
                            {                                
                                //Update Co-Buyer on lot with Co-Buyer from AP record
                                rstlot.Fields[modAdvantageProgram.strfTIC_CO_BUYER_ID].Value = vntCoBuyerId;
                                //Save recordset
                                objLib.PermissionIgnored = true;
                                objLib.SaveRecordset(modAdvantageProgram.strtPRODUCT, rstlot);
                                
                            }

                            rstlot.Close();
                        }

                        //UPDATE MAIN BUYER AND COBUYER FIELDS FOR THE QUOTE/CONTRACT
                        Recordset rstQuoteContract = objLib.GetRecordset(vntReservationContractId, modAdvantageProgram.strtOPPORTUNITY, modAdvantageProgram.strfTIC_CO_BUYER_ID, modAdvantageProgram.strfCONTACT_ID);
                        if (rstQuoteContract.RecordCount > 0)
                        {
                            rstQuoteContract.MoveFirst();

                            if (RSysSystem.EqualIds(rstQuoteContract.Fields[modAdvantageProgram.strfCONTACT_ID].Value, vntBuyerId))
                            {                              
                                rstQuoteContract.Fields[modAdvantageProgram.strfTIC_CO_BUYER_ID].Value = vntCoBuyerId;

                                //Save recordset
                                objLib.PermissionIgnored = true;
                                objLib.SaveRecordset(modAdvantageProgram.strtOPPORTUNITY, rstQuoteContract);

                                //UPDATE CO-BUYERS SEGMENT only if Buyer on AP record and Contact on Contract match
                                SetLotQuoteContractCobuyers(vntLotId, vntReservationContractId, vntAdvantageProgramId);

                            }
                             
                            rstQuoteContract.Close();
                                                        
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }


        /// <summary>
        /// Call Opportunity code to send out Notification for Reservation
        /// </summary>
        /// <param name="notifiactionEvent"></param>
        /// <param name="neighborhoodId"></param>
        /// <param name="rstOpportunity"></param>
        /// <param name="vntContactId"></param>
        public virtual void SendReserveNotification(string notifiactionEvent, object neighborhoodId, Recordset rstOpportunity, object vntContactId)
        {
            Opportunity ObjOpportunity = (Opportunity)RSysSystem.ServerScripts[modAdvantageProgram.strsASR_OPPORTUNITY].CreateInstance();
            ObjOpportunity.SendEmailNotifications(notifiactionEvent, neighborhoodId, rstOpportunity, vntContactId);
        
        }



    }

}