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
    /// <summary>
    /// This module implements the business logic in the PHb Contact object.
    /// </summary>
    /// The Contact object is used to keep and manage information about
    /// the people with whom your company has established a relationship.
    /// Contact records can represent a whole spectrum of individuals,
    /// whether they are customers, connections to sale opportunities,
    /// or people associated with other companies you deal with. Contact
    /// information is available throughout the system.
    /// <historty>
    /// Revision#   Date        Author      Description
    /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
    /// </historty>

    public class Contact : IRFormScript
    {
        private IRSystem7 mrsysSystem = null;

        protected IRSystem7 RSysSystem
        {
            get { return mrsysSystem; }
            set { mrsysSystem = value; }
        }

        private ILangDict mrldtLangDict = null;

        protected ILangDict RldtLangDict
        {
            get { return mrldtLangDict; }
            set { mrldtLangDict = value; }
        }
       
        /// <summary>
        /// Merge fields from a contact recordset to vntTargetContactId's record.
        /// </summary>
        /// <param name="rstSourceContact">recordset from contact form</param>
        /// <param name="vntTargetContactId">contact id to be modified</param>
        /// <param name="rstRealtorCompanies">Companies</param>
        /// <returns></returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual void HB_MergeContact(ref Recordset rstSourceContact, object vntTargetContactId, ref Recordset rstRealtorCompanies)
        {
            try
            {
                if (rstSourceContact == null)
                {
                    return;
                }

                if (rstSourceContact.RecordCount <= 0)
                {
                    return;
                }

                if (Convert.IsDBNull(vntTargetContactId))
                {
                    return;
                }

                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstTargetContact = objLib.GetRecordset(vntTargetContactId, modContact.strtCONTACT, modContact.strfADDRESS_1,
                    modContact.strfADDRESS_2, modContact.strfADDRESS_3, modContact.strfCITY, modContact.strfSTATE_,
                    modContact.strfCOMMUTE, modContact.strfCOMPANY_ID, modContact.strfCOUNTRY, modContact.strfCOUNTY_ID, modContact.strfZIP, modContact.strfAGE_RANGE_OF_BUYERS,
                    modContact.strfAGE_RANGE_OF_CHILDREN, modContact.strfAREA_CODE, modContact.strfCLOSE_DATE, modContact.strfCELL,
                    modContact.strfCOMBINED_INCOME_RANGE, modContact.strfCOMMENTS, modContact.strfCOMMENTS, modContact.strfCURRENT_MONTHLY_PAYMENT,
                    modContact.strfCURRENT_SQUARE_FOOTAGE, modContact.strfDESIRED_MONTHY_PAYMENT, modContact.strfDESIRED_MOVE_IN_DATE,
                    modContact.strfDESIRED_PRICE_RANGE, modContact.strfDESIRED_SQUARE_FOOTAGE, modContact.strfDIVISION_ID,
                    modContact.strfEDUCATION, modContact.strfEMAIL, modContact.strfEXTENSION, modContact.strfFAX, modContact.strfFIRST_NAME,
                    modContact.strfFIRST_CONTACT_DATE, modContact.strfFOR_SALE, modContact.strfHOME_TYPE, modContact.strfGENDER,
                    modContact.strfHOMES_OWNED, modContact.strfHOUSEHOLD_SIZE, modContact.strfLAST_NAME, modContact.strfLEAD_DATE,
                    modContact.strfLEAD_SOURCE_ID, modContact.strfLEAD_SOURCE_TYPE, modContact.strfMARITAL_STATUS, modContact.strfMINIMUM_BATHROOMS,
                    modContact.strfMINIMUM_BEDROOMS, modContact.strfMINIMUM_GARAGE, modContact.strfNEXT_FOLLOW_UP_DATE,
                    modContact.strfNUMBER_LIVING_AREAS, modContact.strfNUMBER_OF_CHILDREN, modContact.strfOTHER_BUILDERS,
                    modContact.strfOTHER_NEIGHBORHOODS, modContact.strfOWNERSHIP, modContact.strfPHONE, modContact.strfPREFERRED_AREA,
                    modContact.strfPREFERRED_CONTACT, modContact.strfREALTOR_ID, modContact.strfREALTOR_COMPANY_ID,
                    modContact.strfREASONS_FOR_MOVING, modContact.strfREFERRED_BY_CONTACT_ID, modContact.strfRESALE,
                    modContact.strfSINGLE_OR_DUAL_INCOME, modContact.strfSSN, modContact.strfSUFFIX, modContact.strfTIME_SEARCHING,
                    modContact.strfTITLE, modContact.strfTRANSFERRING_TO_AREA, modContact.strfCONTACT_PROFILE_NBHD_TYPE,
                    modContact.strfWORK_PHONE, modContact.strfWALK_IN_DATE, modContact.strfWORK_OUT_OF_OFFICE, modContact.strfACCOUNT_MANAGER_ID);

                rstTargetContact.MoveFirst();
                rstTargetContact.Fields[modContact.strfACCOUNT_MANAGER_ID].Value = rstSourceContact.Fields[modContact.strfACCOUNT_MANAGER_ID].Value;

                if (!(Convert.IsDBNull(rstSourceContact.Fields[modContact.strfADDRESS_1].Value)))
                {
                    rstTargetContact.Fields[modContact.strfADDRESS_1].Value = rstSourceContact.Fields[modContact.strfADDRESS_1].Value;
                    rstTargetContact.Fields[modContact.strfADDRESS_2].Value = rstSourceContact.Fields[modContact.strfADDRESS_2].Value;
                    rstTargetContact.Fields[modContact.strfADDRESS_3].Value = rstSourceContact.Fields[modContact.strfADDRESS_3].Value;
                    rstTargetContact.Fields[modContact.strfCITY].Value = rstSourceContact.Fields[modContact.strfCITY].Value;
                    rstTargetContact.Fields[modContact.strfSTATE_].Value = rstSourceContact.Fields[modContact.strfSTATE_].Value;
                    rstTargetContact.Fields[modContact.strfCOUNTRY].Value = rstSourceContact.Fields[modContact.strfCOUNTRY].Value;
                    rstTargetContact.Fields[modContact.strfCOUNTY_ID].Value = rstSourceContact.Fields[modContact.strfCOUNTY_ID].Value;
                    rstTargetContact.Fields[modContact.strfZIP].Value = rstSourceContact.Fields[modContact.strfZIP].Value;
                }

                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfACCOUNT_MANAGER_ID);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfAGE_RANGE_OF_BUYERS);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfAGE_RANGE_OF_CHILDREN);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfAREA_CODE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfCELL);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfCLOSE_DATE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfCOMBINED_INCOME_RANGE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfCOMMENTS);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfCOMMUTE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfCOMPANY_ID);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfCURRENT_MONTHLY_PAYMENT);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfCURRENT_SQUARE_FOOTAGE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfDESIRED_MONTHY_PAYMENT);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfDESIRED_MOVE_IN_DATE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfDESIRED_PRICE_RANGE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfDESIRED_SQUARE_FOOTAGE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfEDUCATION);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfEMAIL);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfEXTENSION);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfFAX);
                rstTargetContact.Fields[modContact.strfFIRST_CONTACT_DATE].Value = DateTime.Today;
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfFIRST_NAME);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfFIRST_CONTACT_DATE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfFOR_SALE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfGENDER);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfHOME_TYPE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfHOMES_OWNED);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfHOUSEHOLD_SIZE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfLAST_NAME);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfLEAD_DATE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfLEAD_SOURCE_ID);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfLEAD_SOURCE_TYPE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfMARITAL_STATUS);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfMINIMUM_BATHROOMS);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfMINIMUM_BEDROOMS);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfMINIMUM_GARAGE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfNEXT_FOLLOW_UP_DATE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfNUMBER_LIVING_AREAS);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfNUMBER_OF_CHILDREN);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfOTHER_BUILDERS);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfOTHER_NEIGHBORHOODS);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfOWNERSHIP);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfPHONE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfPREFERRED_AREA);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfPREFERRED_CONTACT);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfREALTOR_COMPANY_ID);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfREALTOR_ID);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfREASONS_FOR_MOVING);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfREFERRED_BY_CONTACT_ID);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfRESALE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfSINGLE_OR_DUAL_INCOME);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfSSN);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfSUFFIX);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfTITLE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfTIME_SEARCHING);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfTRANSFERRING_TO_AREA);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfWALK_IN_DATE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfWORK_OUT_OF_OFFICE);
                CopyFieldValue(rstSourceContact, rstTargetContact, modContact.strfWORK_PHONE);

                objLib.SaveRecordset(modContact.strtCONTACT, rstTargetContact);

                if (rstRealtorCompanies != null)
                {
                    if (rstRealtorCompanies.RecordCount > 0)
                    {
                        rstRealtorCompanies.MoveFirst();

                        while (!(rstRealtorCompanies.EOF))
                        {
                            Recordset rstExistsCompCont = objLib.GetRecordset(modContact.strqCOMPANY_CONTACT_REALTOR_TYPE, 2, rstRealtorCompanies.Fields[modContact.strfCOMPANY_ID].Value, vntTargetContactId, modContact.strfCOMPANY_CONTACT_ID);
                            
                            if (rstExistsCompCont.RecordCount == 0)
                            {
                                Recordset rstNewCC = objLib.GetNewRecordset(modContact.strtCOMPANY_CONTACT, modContact.strfCOMPANY_ID, modContact.strfCONTACT_ID, modContact.strfTYPE);
                                rstNewCC.AddNew(Type.Missing, Type.Missing);
                                rstNewCC.Fields[modContact.strfCOMPANY_ID].Value = rstRealtorCompanies.Fields[modContact.strfCOMPANY_ID].Value;
                                rstNewCC.Fields[modContact.strfCONTACT_ID].Value = vntTargetContactId;
                                rstNewCC.Fields[modContact.strfTYPE].Value = modContact.strcREALTOR;

                                objLib.SaveRecordset(modContact.strtCOMPANY_CONTACT, rstNewCC);
                            }

                            rstRealtorCompanies.MoveNext();
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function accepts first and last name to check if there exists contacts
        /// with the same first and last name.  If they exist, return the recordset.
        /// </summary>
        /// <param name="vntFirstName">first name</param>
        /// <param name="vntLastName">last name</param>
        /// <param name="vntZip">Zip code</param>
        /// <param name="vntContactType">Contact's Type</param>
        /// <returns>
        /// A recordset of duplicate Contacts with the first and last names.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual Recordset HB_ContactDuplicate(object vntFirstName, object vntLastName, object vntZip, object
            vntContactType)
        {
            try
            {
                Recordset rstContact = null;
                DataAccess objDLFunctionLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                
                if (Convert.IsDBNull(vntZip) || TypeConvert.ToString(vntZip) == "")
                {
                    // zip is not filled in
                    rstContact = objDLFunctionLib.GetRecordset(modContact.strqCHECK_DUPLICATE_CONTACTS, 3, vntFirstName,
                        vntLastName, vntContactType, modContact.strfCONTACT_ID, modContact.strfFIRST_NAME, modContact.strfLAST_NAME,
                        modContact.strfPHONE, modContact.strfCITY, modContact.strfEMAIL);
                }
                else
                {
                    // zip is filled in
                    rstContact = objDLFunctionLib.GetRecordset(modContact.strqDUPLICATE_CONTACTS_CHECK_WITH_ZIP, 4,
                        vntLastName, vntFirstName, vntZip, vntContactType, modContact.strfCONTACT_ID, modContact.strfFIRST_NAME,
                        modContact.strfLAST_NAME, modContact.strfPHONE, modContact.strfCITY, modContact.strfEMAIL);
                }

                return rstContact;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function finds duplicate Contact records according to the Match Code.
        /// This function is used by IRFormScript_Execute. Assumptions: MatchCode is the Contact's "First_Name" + "Last_Name" + "Phone".
        /// Query strqPOSSIBLE_DUPLICATE is defined with one parameter for Matchcode.
        /// </summary>
        /// <param name="strfMATCH_CODE">Match Code of a Contact = "First_Name" + "Last_Name" + "Phone"</param>
        /// <returns>
        /// A recordset of duplicate Contacts with the same Match Code.
        /// Implements Agent: Dup Checking</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual Recordset HasDuplicates(object strfMATCH_CODE)
        {
            //Recordset rstContactDup = null;

            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstContactDup=objLib.GetRecordset(modContact.strqPOSSIBLE_DUPLICATE,1,strfMATCH_CODE,
                    modContact.strfCONTACT_ID,modContact.strfFIRST_NAME,modContact.strfLAST_NAME,modContact.strfPHONE);
                
                return rstContactDup;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function finds all the Contact Team Members for a specified Contact Id.
        /// </summary>
        /// <param name="vntfContact_Id">Contact Id</param>
        /// <returns>
        /// A recordset of Contact Team Members for the given Contact Id.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual Recordset FoundContactTeamMembers(object vntfContact_Id)
        {
            try
            {
                // find contact team member using contact id
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstCTM=objLib.GetRecordset(modContact.strqCONTACT_TEAM_MEMBER_WITH_CONTACT_ID,1,vntfContact_Id,
                    modContact.strfCONTACT_ID,modContact.strfEMPLOYEE_ID);

                return rstCTM;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function uses Territory Id to find Territory Team Members.
        /// </summary>
        /// This function is used by IRFormScript_Execute. Assumptions: Query strqMEMBER_OF_TERRITORY is defined.
        /// <param name="vntfTerritory_Id">Territory Id of the Contact</param>
        /// <returns>
        /// A recordset of Territory Team Members for the given Territory Id.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual Recordset GetDefaultTeamMembers(object vntfTerritory_Id)
        {
            try
            {
                // find territory member under this contact.territory_id
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstTTM=objLib.GetRecordset(modContact.strqMEMBER_OF_TERRITORY,1,vntfTerritory_Id,
                    modContact.strfTERRITORY_ID,modContact.strfROLE_ID,modContact.strfEMPLOYEE_ID);

                return rstTTM;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks whether the given Contact has any Support Incident records.
        /// Assumptions: Query strqSUPPORT_INCIDENT_WITH_CONTACT is defined.
        /// </summary>
        /// <param name="vntfContact_Id">Contact Id</param>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual bool HasSupportIncident(object vntfContact_Id)
        {
            bool bHasSupportIncident = false;
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstSupportIncident=objLib.GetRecordset(modContact.strqSUPPORT_INCIDENT_WITH_CONTACT,1,vntfContact_Id,
                    modContact.strfCONTACT_ID);

                if (!(rstSupportIncident.EOF))
                {
                    bHasSupportIncident = true;
                }
                else
                {
                    bHasSupportIncident = false;
                }

                return bHasSupportIncident;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks whether the given Contact has any Opportunity records.
        /// Assumptions: Query strqOPPORTUNITIES_WITH_CONTACT is defined.
        /// </summary>
        /// <param name="vntfContact_Id">Contact Id</param>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual bool HasOpportunity(object vntfContact_Id)
        {
            try
            {
                bool bHasOpportunity = false;

                if (!(Convert.IsDBNull(vntfContact_Id)))
                {
                    DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                    Recordset rstOpportunity=objLib.GetRecordset(modContact.strqOPPORTUNITIES_WITH_CONTACT,1,vntfContact_Id,
                        modContact.strfCONTACT_ID);

                    if (!(rstOpportunity.EOF))
                    {
                        bHasOpportunity = true;
                    }
                    else
                    {
                        bHasOpportunity = false;
                    }
                }

                return bHasOpportunity;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks whether the given Contact has any Order records
        /// under the linked field Bill_To_Contact_Id.
        /// Assumptions: Query strqORDER_WITH_BILL_TO_CONTACT is defined.
        /// </summary>
        /// <param name="vntfContact_Id">Contact Id</param>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual bool HasOrder(object vntfContact_Id)
        {
            try
            {
                bool bHasOrder = false;
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOrder=objLib.GetRecordset(modContact.strqORDER_WITH_BILL_TO_CONTACT,1,vntfContact_Id,
                    modContact.strfORDER_BILL_TO_CONTACT_ID);

                if (!(rstOrder.EOF))
                {
                    bHasOrder = true;
                }
                else
                {
                    bHasOrder = false;
                }

                return bHasOrder;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks whether the given Contact has any Registration records
        /// under the linked fields Administrative_Contact_Id or MIS_Contact_Id.
        /// Assumptions: Query strqREGISTRATION_WITH_ADMINISTRATIVE_CONTACT,
        /// strqREGISTRATION_WITH_MIS_CONTACT and strqREGISTRATION_WITH_USER_CONTACT are defined.
        /// </summary>
        /// <param name="vntfContact_Id">Contact Id</param>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual bool HasRegistration(object vntfContact_Id)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstRegistration=objLib.GetRecordset(modContact.strqREGISTRATION_WITH_ADMINISTRATIVE_CONTACT,1,vntfContact_Id,
                    modContact.strfREGISTRATION_ADMINISTRATIVE_CONTACT_ID,modContact.strfREGISTRATION_MIS_CONTACT_ID,modContact.strfREGISTRATION_USER_CONTACT_ID);
                
                if (!(rstRegistration.EOF))
                {
                   return true;
                }

                rstRegistration=objLib.GetRecordset(modContact.strqREGISTRATION_WITH_MIS_CONTACT,1,vntfContact_Id);

                if (!(rstRegistration.EOF))
                {
                    return false;
                }
                rstRegistration=objLib.GetRecordset(modContact.strqREGISTRATION_WITH_USER_CONTACT,1,vntfContact_Id);

                if (!(rstRegistration.EOF))
                {
                    return true;
                }

                return false;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks whether the given Contact has any
        /// Support Contact records under the linked field Administrative_Contact_Id
        /// Assumptions: Query strqSUPPORT_CONTRACT_WITH_ADMINISTRATIVE_CONTACT is defined.
        /// </summary>
        /// <param name="vntfContact_Id">Contact Id</param>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual bool HasSupportContract(object vntfContact_Id)
        {
            try
            {
                bool bHasSupportContract = false;
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstSupportContract=objLib.GetRecordset(modContact.strqSUPPORT_CONTRACT_WITH_ADMINISTRATIVE_CONTACT,1,
                    vntfContact_Id,modContact.strfSUPPORT_CONTRACT_ADMINISTRATIVE_CONTACT_ID);

                if (!(rstSupportContract.EOF))
                {
                     bHasSupportContract = true;
                }
                else
                {
                     bHasSupportContract = false;
                }

                return bHasSupportContract;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks whether the given Contact has any associated link with
        /// the tables: Support_Incident, Opportunity, Order, Registration, and Support_Contract.
        /// </summary>
        /// <param name="vntfContact_Id">Contact Id</param>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual bool WithAssociatedLinks(object vntfContact_Id)
        {
            bool ProtContact_WithAssociatedLinks = false;
            // obsolete
            return ProtContact_WithAssociatedLinks;
        }

        /// <summary>
        /// This function deletes the record associated with the given Contact or
        /// </summary>
        /// sets the link field to Null. Assumptions: The strQuery given is defined.
        /// <param name="strFormName">Form name of the record to be deleted or set to Null</param>
        /// <param name="strTableName">Table name of the record to be deleted or set to Null</param>
        /// <param name="strPrimaryKey">Primary key field name of the table given</param>
        /// <param name="strLinkField">Link field name corresponding to the Contact Id</param>
        /// <param name="strQuery">Query used to find all the associated</param>
        /// <param name="blnRealDelete">True to delete the record False to set the link field to Null</param>
        /// <param name="vntfContact_Id">Contact Id</param>
        /// <returns>None</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual void DeleteOrSetNull(object strFormName, string strTableName, string strPrimaryKey, object
            vntLinkField, string strQuery, bool blnRealDelete, object vntfContact_Id)
        {
            try
            {
                IRForm pForm = null;
                bool blnChanged = false;
                object vntParameterList = null;

                if (TypeConvert.ToString(strFormName) != "")
                {
                    pForm = RSysSystem.Forms[strFormName];
                }

                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstChild=objLib.GetRecordset(strQuery,1,vntfContact_Id,vntLinkField);

                while(!(rstChild.EOF))
                {
                    if (blnRealDelete == true)
                    {
                        if (pForm == null)
                        {
                           objLib.DeleteRecord(rstChild.Fields[strPrimaryKey].Value,strTableName);
                        }
                        else
                        {
                            if (TypeConvert.ToString(strFormName) == modContact.strfrmRN_APPOINTMENT)
                            {
                                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                                ocmsTransitPointParams.Construct();
                                ocmsTransitPointParams.ParameterList = ocmsTransitPointParams.SetUserDefinedParameter(1, vntParameterList);
                                ocmsTransitPointParams.ParameterList = ocmsTransitPointParams.SetUserDefinedParameter(2, vntParameterList);
                            }
                            pForm.DeleteFormData(rstChild.Fields[strPrimaryKey].Value, ref vntParameterList);
                        }
                    }
                    else
                    {
                        if (pForm == null)
                        {
                            rstChild.Fields[vntLinkField].Value = System.DBNull.Value;
                            blnChanged = true;
                        }
                        else
                        {
                            object rstRecordsets = pForm.LoadFormData(rstChild.Fields[strPrimaryKey].Value, ref vntParameterList);
                            object[] recordsetArray = (object[])rstRecordsets;
                            Recordset rstForm = (Recordset) recordsetArray[0];
                            rstForm.Fields[vntLinkField].Value = System.DBNull.Value;
                            pForm.SaveFormData(rstRecordsets, ref vntParameterList);
                        }
                    }

                    rstChild.MoveNext();
                }

                if (blnChanged == true)
                {
                    objLib.SaveRecordset(strTableName,rstChild);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function goes through all the related tables of Contact and either deletes the record or sets the link
        /// field of the record to Null.
        /// </summary>
        /// <param name="vntfContact_Id">Contact Id</param>
        /// <returns>None</returns>
        /// Implements Agent: Cascade Delete
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// 5.9         5/3/2007    JH          This function is deprecated.
        /// 5.9         8/7/2007    BC          This function is modified to handle the delete of contact.
        /// </history>
        protected virtual void CascadeDelete(object vntfContact_Id)
        {
            try
            {
                // Delete or set null Children Tables
                this.DeleteOrSetNull(modContact.strfrmACTION_PLAN_CONTACT_STEP, modContact.strtACTION_PLAN_CONTACT_STEP,
                    modContact.strfACTION_PLAN_CONTACT_STEP_ID, modContact.strfASSIGNED_TO_ID, modContact.strqACTION_PLAN_CONTACT_STEP_WITH_ASSIGNED_TO_ID,
                    false, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtALT_ADDRESS, modContact.strfALT_ADDRESS_ID, modContact.strfCONTACT_ID, modContact.strqALT_ADDRESSES_OF_CONTACT, true, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtALT_PHONE, modContact.strfALT_PHONE_ID, modContact.strfCONTACT_ID, modContact.strqALT_PHONE_WITH_CONTACT_ID, true, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtARCH_ACTIVITY, modContact.strfARCH_ACTIVITY_ID, modContact.strfASSIGNED_BY_CONTACT_ID, modContact.strqARCH_ACTIVITY_WITH_ASSIGNED_BY_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtARCH_ACTIVITY, modContact.strfARCH_ACTIVITY_ID, modContact.strfCONTACT, modContact.strqARCH_ACTIVITY_WITH_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtARCH_LEAD, modContact.strfARCH_LEAD_ID, modContact.strfASSIGNED_TO_PARTNER_CONTACT, modContact.strqARCH_LEAD_WITH_ASSIGNED_TO_PARTNER_CONTACT, false, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtARCH_LEAD, modContact.strfARCH_LEAD_ID, modContact.strfREFERRED_BY_CONTACT_ID, modContact.strqARCH_LEAD_WITH_REFERRED_BY_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtARCH_MEETING_CONT_ATTENDEE, modContact.strfARCH_MEETING_CONT_ATTENDEE_ID, modContact.strfCONTACT_ID, modContact.strqARCH_MEETING_CONT_ATTENDEE_WITH_CONTACT_ID, true, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmCOMPANY, modContact.strtCOMPANY, modContact.strfCOMPANY_ID, modContact.strfPARTNER_CONTACT_ID,
                    modContact.strqCOMPANY_WITH_PARTNER_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmCOMPANY, modContact.strtCOMPANY, modContact.strfCOMPANY_ID, modContact.strfREFERRED_BY_ID,
                    modContact.strqCOMPANY_WITH_REFERRED_BY_ID, false, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmCOMPANY, modContact.strtCOMPANY, modContact.strfCOMPANY_ID, modContact.strfRESELLER_KEY_CONTACT_ID,
                    modContact.strqCOMPANY_WITH_RESELLER_KEY_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmCOMPANY, modContact.strtCOMPANY, modContact.strfCOMPANY_ID, modContact.strfSUPPLIER_ACCOUNT_MANAGER_ID,
                    modContact.strqCOMPANY_WITH_SUPPLIER_ACCOUNT_MANAGER_ID, false, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtCOMPANY_CONTACT, modContact.strfCOMPANY_CONTACT_ID, modContact.strfCONTACT_ID, modContact.strqCOMPANY_CONTACT_WITH_CONTACT_ID, true, vntfContact_Id);

                //this.DeleteOrSetNull(modContact.strfrmCONTACT, modContact.strtCONTACT, modContact.strfCONTACT_ID, modContact.strfPARTNER_CONTACT_ID,
                //    modContact.strqCONTACT_WITH_PARTNER_CONTACT_ID, false, vntfContact_Id);

                //this.DeleteOrSetNull(modContact.strfrmCONTACT, modContact.strtCONTACT, modContact.strfCONTACT_ID, modContact.strfREPORTS_TO_ID,
                //    modContact.strqCONTACT_WITH_REPORTS_TO_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmGENERAL_CONTACT_ACTIVITY, modContact.strtCONTACT_ACTIVITIES, modContact.strfCONTACT_ACTIVITIES_ID,
                    modContact.strfASSIGNED_BY_CONTACT_ID, modContact.strqCONTACT_ACTIVITIES_WITH_ASSIGNED_BY_CONTACT_ID,
                    false, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmGENERAL_CONTACT_ACTIVITY, modContact.strtCONTACT_ACTIVITIES, modContact.strfCONTACT_ACTIVITIES_ID,
                    modContact.strfASSIGNED_TO_CONTACT_ID, modContact.strqCONTACT_ACTIVITIES_WITH_ASSIGNED_TO_CONTACT_ID,
                    true, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmGENERAL_CONTACT_ACTIVITY, modContact.strtCONTACT_ACTIVITIES, modContact.strfCONTACT_ACTIVITIES_ID,
                    modContact.strfCONTACT_ID, modContact.strqCONTACT_ACTIVITIES_WITH_CONTACT_ID, true, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtCONTACT_TEAM_MEMBER, modContact.strfMEMBER_TEAM_MEMBER_ID, modContact.strfCONTACT_ID, modContact.strqCONTACT_TEAM_MEMBER_WITH_CONTACT_ID, true, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtCONTRACT_NAMED_CONTACT, modContact.strfCONTRACT_NAMED_CONTACT_ID, modContact.strfCONTACT_ID, modContact.strqCONTRACT_NAMED_CONTACT_WITH_CONTACT_ID, true, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtCONTRACT_PROHIBITED_CONTACT, modContact.strfCONTRACT_PROHIBITED_CONTACT_ID, modContact.strfCONTACT_ID, modContact.strqCONTRACT_PROHIBITED_CONTACT_WITH_CONTACT_ID, true, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtISSUE, modContact.strfISSUE_ID, modContact.strfREPORTED_BY_CONTACT_ID, modContact.strqISSUE_WITH_REPORTED_BY_CONTACT_ID, false, vntfContact_Id);

                //this.DeleteOrSetNull(modContact.strfrmLEAD, modContact.strtLEAD, modContact.strfLEAD_ID, modContact.strfASSIGNED_TO_PARTNER_CONTACT,
                //    modContact.strqLEAD_WITH_ASSIGNED_TO_PARTNER_CONTACT, false, vntfContact_Id);

                //this.DeleteOrSetNull(modContact.strfrmLEAD, modContact.strtLEAD, modContact.strfLEAD_ID, modContact.strfREFERRED_BY_CONTACT_ID,
                //    modContact.strqLEAD_WITH_REFFERRED_BY_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmMARKETING_PROJECT, modContact.strtMARKETING_PROJECT, modContact.strfMARKETING_PROJECT_ID,
                    modContact.strfCONTACT_ID, modContact.strqMARKETING_PROJECT_WITH_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtMEETING_CONTACT_ATTENDEE, modContact.strfMEETING_CONTACT_ATTENDEE_ID, modContact.strfCONTACT_ID, modContact.strqMEETING_CONTACT_ATTENDEE_WITH_CONTACT_ID, true, vntfContact_Id);

                //this.DeleteOrSetNull(modContact.strfrmOPPORTUNITY, modContact.strtOPPORTUNITY, modContact.strfOPPORTUNITY_ID,
                //    modContact.strfPARTNER_CONTACT_ID, modContact.strqOPPORTUNITY_WITH_PARTNER_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmOPPORTUNITY_INFLUENCER, modContact.strtOPPORTUNITY_INFLUENCER,
                    modContact.strfOPPORTUNITY_INFLUENCER_ID, modContact.strfINFLUENCER_ID, modContact.strqOPPORTUNITY_INFLUENCER_WITH_INFLUENCER_ID,
                    true, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmORDER, modContact.strtORDER, modContact.strfORDER_ID, modContact.strfPARTNER_CONTACT_ID,
                    modContact.strqORDER_WITH_PARTNER_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmORDER, modContact.strtORDER, modContact.strfORDER_ID, modContact.strfSHIP_TO_CONTACT_ID,
                    modContact.strqORDER_WITH_SHIP_TO_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtREGISTRATION_NAMED_CONTACT, modContact.strfREGISTRATION_NAMED_CONTACT_ID, modContact.strfCONTACT_ID, modContact.strqREGISTRATION_NAMED_CONTACT_WITH_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmRN_APPOINTMENT, modContact.strtRN_APPOINTMENTS, modContact.strfRN_APPOINTMENTS_ID,
                    modContact.strfASSIGNED_BY_CONTACT_ID, modContact.strqRN_APPOINTMENTS_WITH_ASSIGNED_BY_CONTACT_ID,
                    true, vntfContact_Id);

                this.DeleteOrSetNull(modContact.strfrmRN_APPOINTMENT, modContact.strtRN_APPOINTMENTS, modContact.strfRN_APPOINTMENTS_ID,
                    modContact.strfCONTACT, modContact.strqRN_APPOINTMENTS_WITH_CONTACT, true, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtRN_CONTACT_SYNC, modContact.strfRN_CONTACT_SYNC_ID, modContact.strfCONTACT_ID, modContact.strqRN_CONTACT_SYNC_WITH_CONTACT_ID, true, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtSUPPORT_INCIDENT, modContact.strfSUPPORT_INCIDENT_ID, modContact.strfPARTNER_CONTACT_ID, modContact.strqSUPPORT_INCIDENT_WITH_PARTNER_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtSUPPORT_INCIDENT, modContact.strfSUPPORT_INCIDENT_ID, modContact.strfRECORDED_BY_CONTACT_ID, modContact.strqSUPPORT_INCIDENT_WITH_RECORDED_BY_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtSUPPORT_REQUEST, modContact.strfSUPPORT_REQUEST_ID, modContact.strfCONTACT_ID, modContact.strqSUPPORT_REQUEST_WITH_CONTACT_ID, true, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtSUPPORT_STEP, modContact.strfSUPPORT_STEP_ID, modContact.strfCONTACT_ID, modContact.strqSUPPORT_STEP_WITH_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtSUPPORT_STEP, modContact.strfSUPPORT_STEP_ID, modContact.strfRECORDED_BY_CONTACT_ID, modContact.strqSUPPORT_STEP_WITH_RECORDED_BY_CONTACT_ID, false, vntfContact_Id);

                this.DeleteOrSetNull("", modContact.strtCONTACT_PROFILE_NBHD, modContact.strfCONTACT_PROFILE_NBHD_ID, modContact.strfCONTACT_ID, modContact.strqNBHD_PROFILES_FOR_CONTACT, true, vntfContact_Id);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function sums the Estimated Total of all opportunities for this contact
        /// using the contact's currency. This function used by IRFormScript_Execute.
        /// </summary>
        /// <param name="vntfContact_Id">Contact Id</param>
        /// <param name="vntfCurrency_Id">Currency Id used by the current contact</param>
        /// <returns>The calculated total revenue.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual string EstimatedTotalRevenue(object vntfContact_Id, object vntfCurrency_Id, ref string strCurrencyName)
        {
            try
            {
                // Initialize the total revenue
                decimal curTotalRevenue = 0;
                
                // Get recordset contains the currency_id and Estimated_Total of the reseller's oppotunities
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstOpportunity=objLib.GetRecordset(modContact.strqOPPORTUNITIES_WITH_CONTACT,1,vntfContact_Id,
                    modContact.strfESTIMATED_TOTAL,modContact.strfCURRENCY_ID);

                // Create Currency object instance for handle currency exchange calculation
                Currency ocmsCurrency = (Currency) RSysSystem.ServerScripts[AppServerRuleData.CurrencyAppServerRuleName].CreateInstance();
                
                while(!(rstOpportunity.EOF))
                {
                    curTotalRevenue = curTotalRevenue + ocmsCurrency.CalculateExchange(rstOpportunity.Fields[modContact.strfCURRENCY_ID].Value,
                        vntfCurrency_Id,TypeConvert.ToDecimal(rstOpportunity.Fields[modContact.strfESTIMATED_TOTAL].Value));
                    rstOpportunity.MoveNext();
                }

                rstOpportunity.Close();

                strCurrencyName = TypeConvert.ToString(RSysSystem.Tables[modContact.strtCURRENCY].Fields[modContact.strfCURRENCY_NAME].Index(vntfCurrency_Id));
                return ocmsCurrency.FormatCurrency(curTotalRevenue, vntfCurrency_Id);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function returns the local time of the Contact. This function used
        /// by IRFormScript_Execute.
        /// </summary>
        /// <param name="vntfTime_Zone_Id">Time Zone Id</param>
        /// <returns>The local time zone offset of the Contact.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual float LocalTime(object vntfTime_Zone_Id)
        {
            float ProtContact_LocalTime = 0;
            // obsolete
            return ProtContact_LocalTime;
        }

        /// <summary>
        /// This function returns the account manager of the territory.
        /// </summary>
        /// <param name="vntfTerritory_Id">Territory Id</param>
        /// <returns>The account manager id of the territory.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual object GetAccountManager(object vntfTerritory_Id)
        {
            object ProtContact_GetAccountManager = null;

            try
            {
                if (Convert.IsDBNull(vntfTerritory_Id))
                {
                    ProtContact_GetAccountManager = System.DBNull.Value;
                    return ProtContact_GetAccountManager;
                }

                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstTerritory=objLib.GetRecordset( modContact.strtTERRITORY, modContact.strfACCOUNT_MANAGER_ID);

                if (!(rstTerritory.EOF))
                {
                    ProtContact_GetAccountManager = rstTerritory.Fields[modContact.strfACCOUNT_MANAGER_ID].Value;
                }
                else
                {
                    ProtContact_GetAccountManager = System.DBNull.Value;
                }

                return ProtContact_GetAccountManager;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function updates the territory of the contact and its
        /// opportunities if the zip code, area code, state, country or account manager
        /// override is changed. It also updates the account manager unless account
        /// manager override is true. This function is used by IRFormScript_Execute.
        /// </summary>
        /// <param name="vntStatus">
        /// Array of status indicators:
        /// (0)blnIsTerritoryChanged
        /// (1)blnIsAccountManagerOverrideChanged
        /// (2)blnIsAccountManagerChanged
        /// (3)blnIsCompanyChanged
        /// </param>
        /// <param name="vntFields">Array of Contact fields to be used in this function</param>
        /// <returns>
        /// Array of variants
        /// (0)Recordset of Territory/Sub-Territory
        /// (1)Account Manager Id
        /// (2)Delta Account Manager Id
        /// (3)Account Manager Changed
        /// (4)blnHasOpportunities - True if the Contact has opportunities or False if the Contact has no opportunities.
        /// (5)Company fields if company is changed
        /// (6)SubTerritory table Id
        /// </returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual object ExitTerritory(object vntStatus, object vntFields)
        {
            try
            {
                object rstTerritory = null;
                object vntCompFields = null;

                // Get Status parameters
                bool blnIsTerritoryChanged = Convert.ToBoolean(((Array[])vntStatus)[0]);
                bool blnIsAccountManagerOverrideChanged = Convert.ToBoolean(((Array[])vntStatus)[1]);
                bool blnIsAccountManagerChanged = Convert.ToBoolean(((Array[])vntStatus)[2]);
                bool blnIsCompanyChanged = Convert.ToBoolean(((Array[])vntStatus)[3]);

                // Get Fields
                Recordset rstRecordset = this.CreateRecordset(vntFields);

                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                if (blnIsTerritoryChanged == true)
                {
                    // Yes, goto FindTerritory
                }
                else if( blnIsAccountManagerOverrideChanged == true)
                {
                    if (Convert.ToDouble(rstRecordset.Fields[modContact.strfACCOUNT_MANAGER_OVERRIDE].Value) == -1)
                    {
                        // Yes, goto FindTerritory
                    }
                    else
                    {
                        rstRecordset.Fields[modContact.strfACCOUNT_MANAGER_ID].Value = RSysSystem.Tables[modContact.strtTERRITORY].Fields[modContact.strfACCOUNT_MANAGER_ID].Index(rstRecordset.Fields[modContact.strfTERRITORY_ID].Value);
                        rstRecordset.Fields[modContact.strfACCOUNT_MANAGER_CHANGED].Value = true;
                        // Yes, goto FindTerritory
                    }
                }
                else if( blnIsCompanyChanged == true)
                {
                    this.SetToCompanyInfo(rstRecordset, false);

                    if (Convert.ToDouble(rstRecordset.Fields[modContact.strfWORK_OUT_OF_OFFICE].Value) == 0)
                    {
                        vntCompFields = new object[] {new object[] {modContact.strfZIP,rstRecordset.Fields[modContact.strfZIP].Value},
                            new object[] {modContact.strfSTATE,rstRecordset.Fields[modContact.strfSTATE].Value},
                            new object[] {modContact.strfCOUNTRY,rstRecordset.Fields[modContact.strfCOUNTRY].Value},
                            new object[] {modContact.strfADDRESS_1,rstRecordset.Fields[modContact.strfADDRESS_1].Value},
                            new object[] {modContact.strfADDRESS_2,rstRecordset.Fields[modContact.strfADDRESS_2].Value},
                            new object[] {modContact.strfADDRESS_3,rstRecordset.Fields[modContact.strfADDRESS_3].Value},
                            new object[] {modContact.strfCITY,rstRecordset.Fields[modContact.strfCITY].Value},
                            new object[] {modContact.strfFAX,rstRecordset.Fields[modContact.strfFAX].Value},
                            new object[] {modContact.strfTIME_ZONE_ID,rstRecordset.Fields[modContact.strfTIME_ZONE_ID].Value},
                            new object[] {modContact.strfPHONE,rstRecordset.Fields[modContact.strfPHONE].Value}};
                    }
                    // Yes, goto FindTerritory
                }
                else if( blnIsAccountManagerChanged == true)
                {
                    rstRecordset.Fields[modContact.strfDELTA_ACCOUNT_MANAGER_ID].Value = rstRecordset.Fields[modContact.strfACCOUNT_MANAGER_ID].Value;
                    rstRecordset.Fields[modContact.strfACCOUNT_MANAGER_CHANGED].Value = true;
                    return new object[] {rstTerritory,rstRecordset.Fields[modContact.strfACCOUNT_MANAGER_ID].Value,rstRecordset.Fields[modContact.strfDELTA_ACCOUNT_MANAGER_ID].Value,rstRecordset.Fields[modContact.strfACCOUNT_MANAGER_CHANGED].Value,this.HasOpportunity(rstRecordset.Fields[modContact.strfCONTACT_ID].Value),vntCompFields,RSysSystem.Tables[modContact.strtSUB_TERRITORY].TableId};
                }
                else
                {
                    return new object[] {rstTerritory,rstRecordset.Fields[modContact.strfACCOUNT_MANAGER_ID].Value,rstRecordset.Fields[modContact.strfDELTA_ACCOUNT_MANAGER_ID].Value,rstRecordset.Fields[modContact.strfACCOUNT_MANAGER_CHANGED].Value,this.HasOpportunity(rstRecordset.Fields[modContact.strfCONTACT_ID].Value),vntCompFields,RSysSystem.Tables[modContact.strtSUB_TERRITORY].TableId};
                }

                // Find territory
                if (Convert.IsDBNull(rstRecordset.Fields[modContact.strfFIRST_NAME].Value) == true)
                {
                    rstRecordset.Fields[modContact.strfFIRST_NAME].Value = "";
                }

                if (Convert.IsDBNull(rstRecordset.Fields[modContact.strfLAST_NAME].Value) == true)
                {
                    rstRecordset.Fields[modContact.strfLAST_NAME].Value = "";
                }

                string strContactObjectName = TypeConvert.ToString(Convert.ToDouble(rstRecordset.Fields[modContact.strfFIRST_NAME].Value)
                    + Convert.ToDouble(rstRecordset.Fields[modContact.strfLAST_NAME].Value));

                TerritoryManagementRule objTerritoryMgmt = (TerritoryManagementRule) RSysSystem.ServerScripts[AppServerRuleData.TerritoryManagementRuleAppServerRuleName].CreateInstance();
                rstTerritory = objTerritoryMgmt.FindTerritory(BusinessEntityIndicator.Contact, TypeConvert.ToString(rstRecordset.Fields[modContact.strfCOMPANY_ID].Value),
                    TypeConvert.ToString(strContactObjectName), TypeConvert.ToString(rstRecordset.Fields[modContact.strfCONTACT_PROFILE_NBHD_TYPE].Value),
                    TypeConvert.ToString(rstRecordset.Fields[modContact.strfPHONE].Value), TypeConvert.ToString(rstRecordset.Fields[modContact.strfZIP].Value),
                    TypeConvert.ToString(rstRecordset.Fields[modContact.strfSTATE].Value), TypeConvert.ToString(rstRecordset.Fields[modContact.strfCOUNTRY].Value));
                
                return new object[] { rstTerritory, rstRecordset.Fields[modContact.strfACCOUNT_MANAGER_ID].Value, rstRecordset.Fields[modContact.strfDELTA_ACCOUNT_MANAGER_ID].Value, rstRecordset.Fields[modContact.strfACCOUNT_MANAGER_CHANGED].Value, this.HasOpportunity(rstRecordset.Fields[modContact.strfCONTACT_ID].Value), vntCompFields, RSysSystem.Tables[modContact.strtSUB_TERRITORY].TableId };
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function creates a recordset using the provided field names and field values.
        /// </summary>
        /// <param name="vntFields">Array of variants of field names and field values</param>
        /// <returns>
        /// A recordset.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual Recordset CreateRecordset(object vntFields)
        {
            try
            {
                string strFields = string.Empty;
                int intCounter = 0;

                if (vntFields ==null)
                {
                    return null;
                }

                // Create Dataset object
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                
                // Assign the table to be searched
                // Append fields
                for(intCounter = 0;intCounter <= ((Array[])vntFields).GetUpperBound(0); intCounter++)
                {
                    strFields = strFields+TypeConvert.ToString(((Array[][])vntFields)[intCounter][0])+",";
                }
                
                // Peel off the last delimiter,.
                strFields = strFields.Substring(0, strFields.Length - 1);
                
                // build the recordset
                Recordset rstRecordset=objLib.GetRecordset(modContact.strtCONTACT,strFields);

                // .Open
                rstRecordset.AddNew(modContact.strtCONTACT,  DBNull.Value);

                for(intCounter = 0;intCounter <= ((Array[])vntFields).GetUpperBound(0); intCounter++)
                {
                    rstRecordset.Fields[((Array[][])vntFields)[intCounter][0]].Value = ((Array[][])vntFields)[intCounter][1];
                }

                rstRecordset.Update(modContact.strtCONTACT,  DBNull.Value);

                return rstRecordset;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function gets the Company recordset using the Company Id.
        /// This function is used by IRFormScript_Execute.
        /// </summary>
        /// <param name="vntfCompany_Id">Company Id of the record</param>
        /// <returns>
        /// A recordset of the Company record based on the Company Id specified.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual Recordset GetCompany(object vntfCompany_Id)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                return objLib.GetRecordset(vntfCompany_Id, modContact.strtCOMPANY,
                    modContact.strfCURRENCY_ID,modContact.strfADDRESS_1,modContact.strfADDRESS_2,modContact.strfADDRESS_3,
                    modContact.strfTERRITORY_ID,modContact.strfCITY,modContact.strfSTATE,modContact.strfCOUNTRY,
                    modContact.strfFAX,modContact.strfPHONE,modContact.strfZIP,modContact.strfTIME_ZONE_ID,
                    modContact.strfACCOUNT_MANAGER_OVERRIDE,modContact.strfAREA_CODE,modContact.strfCOUNTY_ID);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function gets the Company recordset with Opportunity related fields using the Company Id.
        /// </summary>
        /// <param name="vntfCompany_Id">Company Id</param>
        /// <returns>
        /// A recordset of the Company record based on the Company Id.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual Recordset GetCompanyForOpp(object vntfCompany_Id)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                return objLib.GetRecordset(vntfCompany_Id, modContact.strtCOMPANY,modContact.strfREFERRED_BY_ID,
                    modContact.strfREFERRED_BY_EMPLOYEE_ID,modContact.strfLEAD_SOURCE_TYPE,modContact.strfLEAD_SOURCE_ID);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function returns the Currency_Id for a given country
        /// This function is used by IRFormScript_Execute.
        /// </summary>
        /// <param name="strCountry">Country string</param>
        /// <returns>
        /// The Currency Id.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual object FillInCurrency(string strCountry)
        {
            object ProtContact_FillInCurrency = null;
            // obsolete
            return ProtContact_FillInCurrency;
        }

        /// <summary>
        /// This function updates the delta fields of the Contact record.
        /// </summary>
        /// <param name="rstContact">Recordset of the Contact</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual void UpdateDeltaFields(Recordset rstContact)
        {
            try
            {
                rstContact.Fields[modContact.strfDELTA_ACCOUNT_MANAGER_ID].Value = rstContact.Fields[modContact.strfACCOUNT_MANAGER_ID].Value;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function sets the default fields from the Company record.
        /// </summary>
        /// <param name="rstContact">Recordset of the Contact</param>
        /// <param name="blnNewContact">True if Contact is new and Account Manager and Territory get set
        /// from Company information or False if Contact exists and only the Company Id
        /// is changed. Only company information is copied.
        /// </param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual void SetToCompanyInfo(Recordset rstContact, bool blnNewContact)
        {
            try
            {
                if (Convert.IsDBNull(rstContact.Fields[modContact.strfCOMPANY_ID].Value))
                {
                    rstContact.Fields[modContact.strfWORK_OUT_OF_OFFICE].Value = true;
                }
                else if( Convert.ToDouble(rstContact.Fields[modContact.strfWORK_OUT_OF_OFFICE].Value) == 0)
                {
                    // get company record
                    Recordset rstCompany = this.GetCompany(rstContact.Fields[modContact.strfCOMPANY_ID].Value);
                    // set address info from Company
                    rstContact.Fields[modContact.strfPHONE].Value = rstCompany.Fields[modContact.strfPHONE].Value;
                    rstContact.Fields[modContact.strfADDRESS_1].Value = rstCompany.Fields[modContact.strfADDRESS_1].Value;
                    rstContact.Fields[modContact.strfADDRESS_2].Value = rstCompany.Fields[modContact.strfADDRESS_2].Value;
                    rstContact.Fields[modContact.strfADDRESS_3].Value = rstCompany.Fields[modContact.strfADDRESS_3].Value;
                    rstContact.Fields[modContact.strfZIP].Value = rstCompany.Fields[modContact.strfZIP].Value;
                    rstContact.Fields[modContact.strfSTATE].Value = rstCompany.Fields[modContact.strfSTATE].Value;
                    rstContact.Fields[modContact.strfCITY].Value = rstCompany.Fields[modContact.strfCITY].Value;
                    rstContact.Fields[modContact.strfCOUNTRY].Value = rstCompany.Fields[modContact.strfCOUNTRY].Value;
                    rstContact.Fields[modContact.strfFAX].Value = rstCompany.Fields[modContact.strfFAX].Value;
                    rstContact.Fields[modContact.strfTIME_ZONE_ID].Value = rstCompany.Fields[modContact.strfTIME_ZONE_ID].Value;

                    if (blnNewContact == true)
                    {
                        // set territory and account manager info from Company
                        rstContact.Fields[modContact.strfTERRITORY_ID].Value = rstCompany.Fields[modContact.strfTERRITORY_ID].Value;

                        if (Convert.IsDBNull(rstCompany.Fields[modContact.strfACCOUNT_MANAGER_OVERRIDE].Value))
                        {
                            rstContact.Fields[modContact.strfACCOUNT_MANAGER_ID].Value = rstCompany.Fields[modContact.strfACCOUNT_MANAGER_ID].Value;
                        }
                        else
                        {
                            rstContact.Fields[modContact.strfACCOUNT_MANAGER_ID].Value = this.GetAccountManager(rstContact.Fields[modContact.strfTERRITORY_ID].Value);
                            rstContact.Fields[modContact.strfACCOUNT_MANAGER_CHANGED].Value = true;
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function returns a recordset of partner companies of the Contact.
        /// This function is used by IRFormScript_Execute.
        /// </summary>
        /// <param name="vntfTerritory_Id">Territory Id of the territory in which the partner companies reside.</param>
        /// <returns>
        /// A recordset of partner companies.</returns>
        /// Implements Agent: List Partner
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual Recordset ListPartner(object vntfTerritory_Id)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                return objLib.GetRecordset(modContact.strqCOMPANY_IN_TERRITORY_PARTNER_WITH_TERRITORY_ID,1,vntfTerritory_Id);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function gets the Web Details Record Id given the Contact Id
        /// This function is used by IRFormScript_Execute.
        /// </summary>
        /// <param name="vntfContact_Id">Contact Id of the Contact Web Details record</param>
        /// <returns>
        /// The Contact Web Details record Id.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual object GetWebDetails(object vntfContact_Id)
        {
            try
            {
                object ProtContact_GetWebDetails = RSysSystem.Tables[modContact.strtCONTACT_WEB_DETAILS].Fields[modContact.strfCONTACT_WEB_DETAILS_ID].FindValue(RSysSystem.Tables[modContact.strtCONTACT_WEB_DETAILS].Fields[modContact.strfCONTACT_ID],
                    vntfContact_Id);

                if ((ProtContact_GetWebDetails == null) == true)
                {
                    ProtContact_GetWebDetails = System.DBNull.Value;
                }

                return ProtContact_GetWebDetails;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function creates Rn_Contact_Sync records from Contact records for
        /// exporting to Outlook.
        /// </summary>
        /// <param name="vntSavedListId"></param>
        /// <param name="intOption">Choice of Current User, Another Employee, or Each Contact's Team Member</param>
        /// <param name="vntUserIds">List of user Ids that the Contacts will be sent to</param>
        /// <returns>
        /// None</returns>
        /// Implements Agent: Export Contacts To Outlook
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual void ExportContactsToOutlook(object vntSavedListId, string strOption, object vntUserId)
        {
            try
            {
                //Recordset rstSavedListItems = null;

                // get the Contact record id's using the SavedListId
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstSavedListItems = objLib.GetRecordset(modContact.strqCONTACT_ITEM_IN_LIST,1,vntSavedListId,modContact.strfRECORD_ID);
                // create an empty Contact Sync Recordset
                Recordset rstContactSync = objLib.GetRecordset(modContact.strtRN_CONTACT_SYNC,modContact.strfCONTACT_ID,modContact.strfUSER_ID);

                // add records to the Contact Sync Recordset according to the option
                while(!(rstSavedListItems.EOF))
                {
                    int intCount = 0;

                    switch (strOption)
                    {
                        case modContact.strenumMYSELF:
                            rstContactSync.AddNew(modContact.strfCONTACT_ID, DBNull.Value);
                            rstContactSync.Fields[modContact.strfCONTACT_ID].Value = rstSavedListItems.Fields[modContact.strfRECORD_ID].Value;
                            rstContactSync.Fields[modContact.strfUSER_ID].Value = RSysSystem.CurrentUserId();
                            rstContactSync.Update(null, DBNull.Value);
                            break;
                        case modContact.strenumEMPLOYEE:
                            if (vntUserId != null)
                            {
                                while (!(intCount > ((Array[])vntUserId).GetUpperBound(0)))
                                {
                                    rstContactSync.AddNew(modContact.strfCONTACT_ID, DBNull.Value);
                                    rstContactSync.Fields[modContact.strfCONTACT_ID].Value = rstSavedListItems.Fields[modContact.strfRECORD_ID].Value;
                                    rstContactSync.Fields[modContact.strfUSER_ID].Value = ((Array[])vntUserId)[intCount];
                                    rstContactSync.Update(null, DBNull.Value);
                                    intCount = (intCount + 1);
                                }
                            }
                            break;
                        case modContact.strenumTEAM:
                            Recordset rstCTM = this.FoundContactTeamMembers(rstSavedListItems.Fields[modContact.strfRECORD_ID].Value);

                            while (!(rstCTM.EOF))
                            {
                                rstContactSync.AddNew(modContact.strfCONTACT_ID, DBNull.Value);
                                rstContactSync.Fields[modContact.strfCONTACT_ID].Value = rstSavedListItems.Fields[modContact.strfRECORD_ID].Value;
                                rstContactSync.Fields[modContact.strfUSER_ID].Value = rstCTM.Fields[modContact.strfMEMBER_TEAM_MEMBER_ID].Value;
                                rstContactSync.Update(null, DBNull.Value);
                                rstCTM.MoveNext();                                
                            }
                            break;
                    }

                    rstSavedListItems.MoveNext();
                }

                objLib.SaveRecordset(modContact.strtSAVED_LIST_ITEMS,rstContactSync);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function gets all employees for a Contact.
        /// This function is used by IRFormScript_Execute.
        /// </summary>
        /// <returns>
        /// A recordset of the employees.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual Recordset GetEmployees()
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                return objLib.GetRecordset(modContact.strqACTIVE_EMPLOYEES,modContact.strfEMPLOYEE_ID);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function gets all contact static lists.
        /// This function is used by IRFormScript_Execute.
        /// </summary>
        /// <returns>
        /// A recordset of the static list.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual Recordset GetContactSavedLists()
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                return objLib.GetRecordset(modContact.strqCONTACT_SAVED_LISTS,modContact.strfSAVED_LIST_ID);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method gets information about a new territory.
        /// This function is used by IRFormScript_Execute.
        /// </summary>
        /// <param name="vntfTerritory_Id">New territory Id</param>
        /// <param name="vntfOldTerritoryId">Old territory Id</param>
        /// <returns>
        /// An array that contains territory name, account manager Id,
        /// old territory name, flag to indicate whether the
        /// territory is changed, recordset of all account managers if
        /// no account manager is defined for the territory, and
        /// the employee table Id.</returns>
        /// Implements Agent: part of Sys\Form|Contact\Exit Territory
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual object GetTerritoryInfo(object vntfTerritory_Id, object vntfOldTerritoryId)
        {
            object ProtContact_GetTerritoryInfo = null;
            // obsolete
            return ProtContact_GetTerritoryInfo;
        }

        /// <summary>
        /// This function adds a new Contact record.
        /// </summary>
        /// <param name="pForm">Contact IRForm object</param>
        /// <param name="Recordsets">Recordset of the Contact to be saved</param>
        /// <param name="ParameterList">Transition Point Parameters</param>
        /// <returns>Record Id of the newly added record.</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        public virtual object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            Recordset rstContact = null;
            try
            {
                // checking and seting of the system parameters
                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;

                if (ocmsTransitPointParams.HasValidParameters() == false)
                {
                    ocmsTransitPointParams.Construct();
                }
                else
                {
                    ocmsTransitPointParams.SetDefaultFields(rstContact);
                    ocmsTransitPointParams.WarningMessage = string.Empty;
                    ParameterList = ocmsTransitPointParams.ParameterList;
                }

                object[] recordsetArry = (object[])Recordsets;
                rstContact =(Recordset) recordsetArry[0];
                object vntContactId = pForm.DoAddFormData(Recordsets, ref ParameterList);
                // check for duplicates

                // Create record set for Contact Team Member table for the prospect only:
                if (TypeConvert.ToString(rstContact.Fields[modContact.strfTYPE].Value) == modContact.strCONTACT_TYPE_PROSPECT)
                {
                    DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();


                    Administration administration = (Administration) RSysSystem.ServerScripts[AppServerRuleData.AdministrationAppServerRuleName].CreateInstance();
                    object vntEmployeeId = administration.CurrentUserRecordId;
                    Recordset rstContact_Team = objLib.GetNewRecordset(modContact.strtCONTACT_TEAM_MEMBER, modContact.strfEMPLOYEE_ID,
                        modContact.strfCONTACT_ID);
                    rstContact_Team.AddNew(modContact.strfCONTACT_ID, DBNull.Value);
                    rstContact_Team.Fields[modContact.strfCONTACT_ID].Value = rstContact.Fields[modContact.strfCONTACT_ID].Value;
                    rstContact_Team.Fields[modContact.strfEMPLOYEE_ID].Value = vntEmployeeId;

                    objLib.SaveRecordset(modContact.strtCONTACT_TEAM_MEMBER, rstContact_Team);
                }

                return vntContactId;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function deletes the Contact record and either deletes or sets Null
        /// of all its secondary or linked records.
        /// </summary>
        /// <param name="RecordId">Contact Id</param>
        /// <param name="ParameterList">Transition Point Parameters</param>
        /// <returns>
        /// None</returns>
        /// Implements Agent: OnDelete
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// 5.9         5/3/2007    JH          This function is deprecated.
        /// </history>
        public virtual void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                // checking and seting of the system parameters
                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;

                if (ocmsTransitPointParams.HasValidParameters() == false)
                {
                    ocmsTransitPointParams.Construct();
                }
                else
                {
                    ocmsTransitPointParams.WarningMessage = string.Empty;
                    ParameterList = ocmsTransitPointParams.ParameterList;

                }

                // Check if this Contact has any children records
                if (this.CanBeDeleted(pForm, RecordId, ref ParameterList))
                {
                    // set the link field to null
                    this.CascadeDelete(RecordId);
                    pForm.DoDeleteFormData(RecordId, ref ParameterList);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function checks to see if the contact record has secondaries.
        /// </summary>
        /// <param name="pForm">Contact IRForm object</param>
        /// <param name="RecordId">Contact Id</param>
        /// <param name="ParameterList">Transition Point Parameters</param>
        /// <returns>
        /// True if the contact has no children
        /// False if the contact has children</returns>
        /// Implements Agent: OnDelete
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// 5.9         5/3/2007    JH          This function is deprecated.
        /// </history>
        protected virtual bool CanBeDeleted(IRForm pForm, object vntRecordId, ref object vntParameterList)
        {
            try
            {
                Recordset rstContact_Secondary = null;
                string strItem = String.Empty;
                string[] returnArray = null;

                // Get the Contact Form
                object vntContactForm = pForm.DoLoadFormData(vntRecordId, ref vntParameterList);
                object[] recordsetArray = (object[]) vntContactForm;

                if (pForm.FormName != modContact.strfrmHB_REALTOR)
                {
                    rstContact_Secondary = pForm.SecondaryFromVariantArray(vntContactForm, modContact.strsegQUOTES);

                    if (rstContact_Secondary.RecordCount > 0)
                    {
                        strItem = modContact.strsegQUOTES;
                        returnArray = new string[] { (string)RldtLangDict.GetTextSub(modContact.strldstrERROR_DELETECHILDRENFIRST, new object[] { strItem }) };
                        vntParameterList = (object)returnArray;

                        return false;
                    }
                }

                rstContact_Secondary = pForm.SecondaryFromVariantArray(vntContactForm, modContact.strsegACTIVITIES);

                if (rstContact_Secondary.RecordCount > 0)
                {
                    strItem = modContact.strsegACTIVITIES;
                    returnArray = new string[] { (string)RldtLangDict.GetTextSub(modContact.strldstrERROR_DELETECHILDRENFIRST, new object[] { strItem }) };
                    vntParameterList = (object)returnArray;
                    return false;
                }
                else
                {
                    if (pForm.FormName != modContact.strfrmHB_REALTOR && pForm.FormName != modContact.strfrmHB_LOAN_OFFICER
                        &&pForm.FormName != modContact.strmHB_ESCROW_OFFICER &&pForm.FormName != modContact.strfrmHB_TITLE_OFFICER)
                    {
                        rstContact_Secondary = pForm.SecondaryFromVariantArray(vntContactForm, modContact.strsegSERVICE_REQUESTS);

                        if (rstContact_Secondary.RecordCount > 0)
                        {
                            strItem = modContact.strsegSERVICE_REQUESTS;
                            returnArray = new string[] { (string)RldtLangDict.GetTextSub(modContact.strldstrERROR_DELETECHILDRENFIRST, new object[] { strItem }) };
                            vntParameterList = (object)returnArray;
                            return false;
                        }
                    }
                }

                // if with associated links, delete not allowed
                if (this.HasSupportIncident(vntRecordId))
                {
                    vntParameterList = new object[] { modContact.strldstrHAS_SUPPORT_INCIDENT, modContact.strgCONTACT };
                    return false;
                }

                if (this.HasOpportunity(vntRecordId))
                {
                    vntParameterList = new object[] {modContact.strldstrHAS_OPPORTUNITY, modContact.strgCONTACT};
                    return false;
                }

                if (this.HasOrder(vntRecordId))
                {
                    vntParameterList = new object[] {modContact.strldstrHAS_ORDER, modContact.strgCONTACT};
                    return false;
                }

                if (this.HasRegistration(vntRecordId))
                {
                    vntParameterList = new object[] {modContact.strldstrHAS_REGISTRATION, modContact.strgCONTACT};
                    return false;
                }

                if (this.HasSupportContract(vntRecordId))
                {
                    vntParameterList = new object[] {modContact.strldstrHAS_SUPPORT_CONTRACT, modContact.strgCONTACT};
                    return false;
                }

                vntParameterList = System.DBNull.Value;
                return true;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This method executes a specified method.
        /// </summary>
        /// <param name="pForm">IRform object reference to the client IRForm object</param>
        /// <param name="vntMethodName">Method name to be executed</param>
        /// <param name="ParameterList">
        /// Transition Point Parameters
        /// (User Def's) Input parameters of the method being executed
        /// </param>        
        /// <returns>
        /// ParameterList - Transition Point Parameters
        /// (User Def's) Return value/Output parameters of the method being executed
        /// /Not used
        /// </returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        {
            try
            {
                Recordset rstRecordset = null;
                object vntReturn = null;
                string strArgument = String.Empty;
                //TerritoryManagementRule ocmsTerritoryMgmt = null;
                //TickleRule ocmsSystem = null;
                Recordset rstresult = null;

                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;

                if (ocmsTransitPointParams.HasValidParameters() == false)
                {
                    ocmsTransitPointParams.Construct();
                }

                object[] parameterArray = ocmsTransitPointParams.GetUserDefinedParameterArray();
                
                switch(MethodName)
                {
                    case modContact.strmESTIMATED_TOTAL_REVENUE:
                        ocmsTransitPointParams.CheckUserDefinedParameterNumber(2);
                        vntReturn = this.EstimatedTotalRevenue(parameterArray[0], parameterArray[1], ref strArgument);
                        parameterArray = new object[] {vntReturn,strArgument};
                        break;
                    case modContact.strmGET_ACCOUNT_MANAGER:
                        ocmsTransitPointParams.CheckUserDefinedParameterNumber(1);
                        vntReturn = this.GetAccountManager(parameterArray[0]);
                        parameterArray = new object[] {vntReturn};
                        break;
                    case modContact.strmGET_COMPANY:
                        ocmsTransitPointParams.CheckUserDefinedParameterNumber(1);
                        rstRecordset = this.GetCompany(parameterArray[0]);
                        parameterArray = new object[] {rstRecordset};
                        break;
                    case modContact.strmGET_COMPANY_FOR_OPP:
                        ocmsTransitPointParams.CheckUserDefinedParameterNumber(1);
                        rstRecordset = this.GetCompanyForOpp(parameterArray[0]);
                        parameterArray = new object[] {rstRecordset};
                        break;
                    case modContact.strmLIST_PARTNER:
                        ocmsTransitPointParams.CheckUserDefinedParameterNumber(1);
                        rstRecordset = this.ListPartner(parameterArray[0]);
                        parameterArray = new object[] {RSysSystem.Tables[modContact.strtCOMPANY].TableId,rstRecordset};
                        break;
                    case modContact.strmEXIT_TERRITORY:
                        ocmsTransitPointParams.CheckUserDefinedParameterNumber(2);
                        parameterArray = (object[])this.ExitTerritory(parameterArray[0],parameterArray[1]);
                        break;
                    case modContact.strmGET_DEFAULT_TEAM_MEMBERS:
                        ocmsTransitPointParams.CheckUserDefinedParameterNumber(1);
                        rstRecordset = this.GetDefaultTeamMembers(parameterArray[0]);
                        parameterArray = new object[] {rstRecordset};
                        break;
                    case modContact.strmGET_WEB_DETAILS:
                        ocmsTransitPointParams.CheckUserDefinedParameterNumber(1);
                        vntReturn = this.GetWebDetails(parameterArray[0]);
                        parameterArray = new object[] {vntReturn};
                        parameterArray = new object[] {RSysSystem.Tables[modContact.strtCONTACT].TableId,rstRecordset};
                        break;
                    case modContact.strmHAS_DUPLICATES:
                        ocmsTransitPointParams.CheckUserDefinedParameterNumber(1);
                        rstRecordset = this.HasDuplicates(parameterArray[0]);
                        parameterArray = new object[] {RSysSystem.Tables[modContact.strtCONTACT].TableId,rstRecordset};
                        break;
                    case modContact.strmEXPORT_CONTACTS_TO_OUTLOOK:
                        if (parameterArray.GetUpperBound(0) > 1)
                        {
                            ocmsTransitPointParams.CheckUserDefinedParameterNumber(3);
                            this.ExportContactsToOutlook(parameterArray[0], TypeConvert.ToString(parameterArray[1]), parameterArray[2]);
                        }
                        else
                        {
                            ocmsTransitPointParams.CheckUserDefinedParameterNumber(2);
                            this.ExportContactsToOutlook(parameterArray[0], TypeConvert.ToString(parameterArray[1]), null);
                        }
                        break;
                    case modContact.strmGET_EMPLOYEES:
                        rstRecordset = this.GetEmployees();
                        parameterArray = new object[] {RSysSystem.Tables[modContact.strtEMPLOYEE].TableId,rstRecordset};
                        break;
                    case modContact.strmGET_CONTACT_SAVED_LISTS:
                        rstRecordset = this.GetContactSavedLists();
                        parameterArray = new object[] {RSysSystem.Tables[modContact.strtSAVED_LISTS].TableId,rstRecordset};
                        break;
                    case modContact.strmCONTACT_SEARCH:
                        // RY: 01/28/2004
                        // Set rstResult = ContactSearch(parameterArray)
                        rstresult = ContactSearch(pForm.FormName, (Recordset)parameterArray[0]);
                        parameterArray = new object[] {rstresult};
                        break;
                    case modContact.strmREALTOR_SEARCH:
                        // JWang: 05/17/2005
                        rstresult = RealtorSearch(pForm.FormName,(Recordset) parameterArray[0]);
                        parameterArray = new object[] {RSysSystem.Tables[modContact.strtCONTACT].TableId,rstresult};
                        break;
                    case modContact.strmLINK_CONTACT_COBUYER:
                        LinkContactCoBuyer(parameterArray[0], parameterArray[1], String.Empty);
                        break;
                    case modContact.strmDELETE_COBUYER_LINK:
                        DeleteCoBuyer(parameterArray[0], parameterArray[1]);
                        break;
                    case modContact.strmGET_CONTACTS:
                        parameterArray = new object[] {GetContacts(parameterArray[0])};
                        break;
                    case modContact.strmHAS_EMAIL_RECIPIENTS:
                        parameterArray = new object[] {HasEmailRecipients(parameterArray[0])};
                        break;
                    case modContact.strmIS_ADDRESS_CHANGED:
                        bool blnSameAddr = IsBuyerCoBuyerAddressSame(parameterArray[0], parameterArray[1], ref rstRecordset,
                            (bool)parameterArray[2]);
                        parameterArray = new object[] {blnSameAddr,rstRecordset};
                        break;
                    case modContact.strmCOPY_ADDRESS:
                        CopyBuyerAddressToCoBuyer(parameterArray[0], parameterArray[1]);
                        break;
                    case modContact.strmCONTACT_DUPLICATE:
                        // Jun 7, 2005, JWang. Added Contact Type parameter in HB_ContactDuplicate
                        rstresult = HB_ContactDuplicate(parameterArray[0], parameterArray[1], parameterArray[2], parameterArray[3]);
                        parameterArray = new object[] {rstresult};
                        break;
                    case modContact.strmMERGE_CONTACT:
                        // RY: parameterArray(0) is source contact recordset.
                        // parameterArray(1) is target contact id.
                        Recordset rstRealtorCompanies = null;
                        Recordset rstSourceContact = (Recordset)parameterArray[0];

                        if (parameterArray.GetUpperBound(0) == 2)
                        {
                            rstRealtorCompanies = (Recordset) parameterArray[2];
                            HB_MergeContact(ref rstSourceContact, parameterArray[1], ref rstRealtorCompanies);
                        }
                        else
                        {
                            HB_MergeContact(ref rstSourceContact, parameterArray[1], ref rstRealtorCompanies);
                        }
                        break;
                    case modContact.strmGET_OSM_STATUS:
                        ocmsTransitPointParams.CheckUserDefinedParameterNumber(1);
                        vntReturn = GetOSMStatus(parameterArray[0]);
                        parameterArray = new object[] { TypeConvert.ToBoolean(vntReturn) };
                        break;
                    case modContact.strmADD_TO_OSM :
                        ocmsTransitPointParams.CheckUserDefinedParameterNumber(1);
                        AddtoOSM(parameterArray[0]);
                        break;
                    case modContact.strmADD_MULTIPLE_TO_OSM:
                        ocmsTransitPointParams.CheckUserDefinedParameterNumber(1);
                        vntReturn = AddMultipletoOSM(TypeConvert.ToString(parameterArray[0]));
                        parameterArray = (object[])vntReturn;
                        break;
                    default:
                        parameterArray = new object[] { System.DBNull.Value };
                        break;
                }

                ParameterList = ocmsTransitPointParams.SetUserDefinedParameterArray(parameterArray);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function loads an existing company record.
        /// </summary>
        /// <param name="pForm">Contact IRForm object</param>
        /// <param name="vntfContact_Id">Contact Id of the record to be loaded</param>
        /// <param name="ParameterList">
        /// Outputs
        /// Transition Point Parameters
        /// (User Def 1) array of Alert Id's under this Contact</param>
        /// <returns>
        /// An array of recordsets, starting with Contact and followed by the  secondaries.
        /// Implements Agent: OnOpen(Modify)</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        public virtual object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            try
            {
                object Recordsets = pForm.DoLoadFormData(RecordId, ref ParameterList);
                object[] recordsetArray = (object[]) Recordsets;
                Recordset rstContact = (Recordset) recordsetArray[0];

                // checking and seting of the system parameters
                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;

                if (ocmsTransitPointParams.HasValidParameters() == false)
                {
                    ocmsTransitPointParams.Construct();
                }
                else
                {
                    ocmsTransitPointParams.SetDefaultFields(rstContact);
                    ocmsTransitPointParams.WarningMessage = string.Empty;
                    ParameterList = ocmsTransitPointParams.ParameterList;
                }

                // Find Alert and return a recordset of Alert Id's to client thru ParameterList
                Alert objAlert = (Alert)RSysSystem.ServerScripts[modContact.strsALERT].CreateInstance();
                Recordset rstAlert = objAlert.FindValidAlerts(rstContact.Fields[modContact.strfCONTACT_ID].Value, modContact.stroCONTACT);
                ParameterList = ocmsTransitPointParams.SetUserDefinedParameter(1, rstAlert);

                return Recordsets;
            }
            catch(Exception exc)
            {
              throw new PivotalApplicationException(exc.Message,exc,RSysSystem);
            }
        }

        /// <summary>
        /// This function creates a new Contact record and sets the default values.
        /// </summary>
        /// <param name="pForm">Contact IRForm object</param>
        /// <param name="ParameterList">Transition Point Parameters</param>
        /// <returns>
        /// An array of empty recordsets, starting with Contact and followed by the secondaries.</returns>
        /// Implements Agent: OnOpen(Add)
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        public virtual object NewFormData(IRForm pForm, ref object ParameterList)
        {
            try
            {
                // get an array of empty recordsets
                object vntContactRst = pForm.DoNewFormData(ref ParameterList);
                object[] recordsetArray = (object[]) vntContactRst;

                if (pForm.FormName == modContact.strfrmHB_REALTOR_SEARCH)
                {
                    return vntContactRst;
                }

                Recordset rstContact = (Recordset) recordsetArray[0];

                // checking and seting of the system parameters
                TransitionPointParameter ocmsTransitPointParams = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                ocmsTransitPointParams.ParameterList = ParameterList;

                if (ocmsTransitPointParams.HasValidParameters() == false)
                {
                    ocmsTransitPointParams.Construct();
                }
                else
                {
                    ocmsTransitPointParams.SetDefaultFields(rstContact);
                    ocmsTransitPointParams.WarningMessage = string.Empty;
                    ParameterList = ocmsTransitPointParams.ParameterList;

                }

                // Core Code
                rstContact.Fields[modContact.strfWORK_OUT_OF_OFFICE].Value = false;

                if (Convert.IsDBNull(rstContact.Fields[modContact.strfCOMPANY_ID].Value))
                {
                    rstContact.Fields[modContact.strfWORK_OUT_OF_OFFICE].Value = true;
                }

                return vntContactRst;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This function creates a new secondary record of the Contact record.
        /// Default behavior is specified by the corresponding function in the IntraHub AppServer Services.
        /// </summary>
        /// <param name="pForm">Contact IRForm object</param>
        /// <param name="SecondaryName">Name of the secondary segment</param>
        /// <param name="ParameterList">Transition Point Parameters</param>
        /// <param name="Recordset">Secondary recordset just created</param>
        /// <returns>
        /// None</returns>
        /// Implements Agent: OnOpen(Add)
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        public virtual void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset
            Recordset)
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

        /// <summary>
        /// This function saves a modified contact record and its secondary records.
        /// </summary>
        /// <returns>
        /// None</returns>
        /// Implements Agent: OnSave(Modify)
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// 5.9.0       9/2/2010    KA          Added code to call manage interest function
        /// 5.9.1       9/8/2010    Ka          commented out call to UPdateNBHDType, will not change NBHD Profile Type since it's too
        ///                                     difficult to figure out what the last status is if they opted in and out.
        /// </history>
        public virtual void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                string strEmail = String.Empty;
                object vntParam = null;

                object[] recorsetArray = (object[])Recordsets;
                Recordset rstContact = (Recordset)recorsetArray[0];

                if (!(rstContact.EOF))
                {
                    this.UpdateDeltaFields(rstContact);
                }

                // Save primary and secondary recordsets
                pForm.DoSaveFormData(Recordsets, ref ParameterList);

                // Cascade inactivate Neighborhood Profile if profile is inactivated.
                if (pForm.FormName == modContact.strfrmHB_QUICK_CONTACT)
                {
                    Recordset rstNP = pForm.SecondaryFromVariantArray(Recordsets, modContact.strsNEIGHBORHOOD_PROFILE);
                    if (rstNP.RecordCount > 0)
                    {
                        ContactProfileNeighborhood objContactProfNBHD = (ContactProfileNeighborhood)
                        RSysSystem.ServerScripts["TIC Contact Profile Neighborhood"].CreateInstance();
                        objContactProfNBHD.IP_Manage_Interest(rstNP);

                        rstNP.MoveFirst();

                        while (!(rstNP.EOF))
                        {
                            object vntContNBHDProfileId = rstNP.Fields[modContact.strfCONTACT_PROFILE_NBHD_ID].Value;

                            if (Convert.ToBoolean(rstNP.Fields[modContact.strfINACTIVE].Value) == true)
                            {
                                InactivateContactProfileNeighborhood objInactivateNBHDP = (InactivateContactProfileNeighborhood)RSysSystem.ServerScripts["TIC Inactivate Contact Profile Neighborhood"].CreateInstance();
                                objInactivateNBHDP.InactivateNeighborhoodProfile(vntContNBHDProfileId, null);
                            }
                            else
                            {
                                object vntContactID = rstNP.Fields["Contact_Id"].Value;
                                object vntNBHDId = rstNP.Fields["Neighborhood_Id"].Value;
                                objContactProfNBHD.NewNeighborhoodProfile(vntNBHDId, vntContactID, System.DBNull.Value, new object[] { "Skip" });
                            }
                            //KA 9/8/10 commented out call to update type
                            //objContactProfNBHD.UpdateNBHDPType(vntContNBHDProfileId);

                            rstNP.MoveNext();
                        }
                                                
                    }
                }

                // added by Carl Langan 01/04/05 for integration
                Integration objIntegration = (Integration)RSysSystem.ServerScripts[modContact.strsINTEGRATION].CreateInstance();
                objIntegration.Execute(modContact.strmIS_INTEGRATION_ON, ref vntParam);

                if ((vntParam is Array))
                {
                    if (((object[])vntParam).GetUpperBound(0) >= 6)
                    {
                        if (Convert.ToBoolean(((object[])vntParam)[6]))
                        {
                            vntParam = new object[] { rstContact.Fields[modContact.strfCONTACT_ID].Value };
                            objIntegration.Execute(modContact.strmNOTIFY_INTEGRATION_OF_BUYER_CHANGE, ref vntParam);
                        }
                    }
                }

                // Update Web Details Email
                if (Convert.IsDBNull(rstContact.Fields[modContact.strfEMAIL].Value))
                    strEmail = TypeConvert.ToString("");
                else
                    strEmail = TypeConvert.ToString(rstContact.Fields[modContact.strfEMAIL].Value);

                this.UPdateWebDetailsEmail(rstContact.Fields[modContact.strfCONTACT_ID].Value, strEmail);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// This procedure sets the IRSystem7 object and the global
        /// variables mrsysSystem, mocmsErrors, and mrldtLangDict.
        /// </summary>
        /// <param name="pSystem">IRSystem object passed by the AppServer Services</param>
        /// <returns>
        // None</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        public virtual void SetSystem(RSystem pSystem)
        {
            try
            {
                RSysSystem = (IRSystem7) pSystem;
                RldtLangDict = RSysSystem.GetLDGroup(modContact.strgCONTACT);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }
        

        /// <summary>
        /// This function is used to update Contact Web Details Email address to fix the
        /// following Issue:
        /// Currently the only place where a contacts email address can be updated is from
        /// there inital contact form. However, a (hidden) field exists on the web_Details
        /// form that is populated from this contact feild on the creation of a new
        /// Web_Details record. If someone modifies the contact email for a contact where the
        /// web_details record already exists, the web_details record is not updated and as
        /// such still contains the old email value.
        /// </summary>
        /// <returns>
        /// None</returns>
        /// Implements Agent: Sys\Form\Contact\Update Web Details email
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        public virtual void UPdateWebDetailsEmail(object vntContactId, string strEmail)
        {
            // YK - Not handling the Error as the Contact should get saved irrespective of the success of updation of
            // CWD

            DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
            Recordset rstWebDetails = objLib.GetRecordset(modContact.strqCONTACT_WEB_DETAILS_WITH_CONTACT, 1, vntContactId, modContact.strfCONTACT_EMAIL_ADDRESS);
           
            if (rstWebDetails.RecordCount > 0)
            {
                rstWebDetails.Fields[modContact.strfCONTACT_EMAIL_ADDRESS].Value = strEmail;
                objLib.SaveRecordset(modContact.strtCONTACT_WEB_DETAILS, rstWebDetails);
            }
        }

        /// <summary>
        /// Called by the Contact Search business object. To search any combination of the
        /// fields based on the HB Contact Center Search form.
        /// </summary>
        /// <param name="strFormName">Contact search form name</param>
        /// <param name="rstContact">Recordset holds the contact search information</param>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual Recordset RealtorSearch(string strFormName, Recordset rstContact)
        {
            try
            {
                bool blnNoCompanyFilter = false;
                bool blnNoSaleCloseDateFilter = false;
                Recordset rstresult = null;

                DataAccess objDLFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                UIAccess objPLFunctionLib = (UIAccess)RSysSystem.ServerScripts[AppServerRuleData.UIAccessAppServerRuleName].CreateInstance();

                // Initializing parameter variables.
                // At the time of testing, passing ParameterList(x) to query didn't work, but
                // passing local variables worked.
                string vntFirstName = TypeConvert.ToString(rstContact.Fields[modContact.strfFIRST_NAME].Value);
                string vntLastName = TypeConvert.ToString(rstContact.Fields[modContact.strfLAST_NAME].Value);
                string vntEmail = TypeConvert.ToString(rstContact.Fields[modContact.strfEMAIL].Value);
                string vntWorkPhone = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfWORK_PHONE, modContact.strsREALTOR_SEARCH, null)].Value);
                string vntCellPhone = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfCELL_PHONE, modContact.strsREALTOR_SEARCH, null)].Value);
                object vntCompanyId = rstContact.Fields[modContact.strfCOMPANY_ID].Value;
                object datSaleDateFrom = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_FROM, modContact.strsREALTOR_SEARCH, null)].Value) ? DBNull.Value /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_FROM, modContact.strsREALTOR_SEARCH, null)].Value;
                object datSaleDateTo = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_TO, modContact.strsREALTOR_SEARCH, null)].Value) ? DBNull.Value /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_TO, modContact.strsREALTOR_SEARCH, null)].Value;
                object datCloseDateFrom = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfCLOSE_DATE_FROM, modContact.strsREALTOR_SEARCH, null)].Value) ? DBNull.Value /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfCLOSE_DATE_FROM, modContact.strsREALTOR_SEARCH, null)].Value;
                object datCloseDateTo = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfCLOSE_DATE_TO, modContact.strsREALTOR_SEARCH, null)].Value) ? DBNull.Value /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfCLOSE_DATE_TO, modContact.strsREALTOR_SEARCH, null)].Value;

                if (vntFirstName == "")
                {
                    vntFirstName = null;
                }

                if (vntLastName == "")
                {
                    vntLastName = null;
                }

                if (vntEmail == "")
                {
                    vntEmail = null;
                }

                if (vntWorkPhone == "")
                {
                    vntWorkPhone = null;
                }

                if (vntCellPhone == "")
                {
                    vntCellPhone = null;
                }

                // If no Company fields are searched, set blnNoCompanyFilter to true
                if (Convert.IsDBNull(vntCompanyId))
                {
                    blnNoCompanyFilter = true;
                }

                // If none of SaleDateFrom, SaleDateTo, CloseDateFrom, CloseDateTo filter exists then set blnNoSaleCloseDateFilter
                // to true
                //if (datSaleDateFrom == null && datSaleDateTo == null && datCloseDateFrom == null && datCloseDateTo == null)
                //{
                //    blnNoSaleCloseDateFilter = true;
                //}
                if (Convert.IsDBNull(datSaleDateFrom) && Convert.IsDBNull(datSaleDateTo) && Convert.IsDBNull(datCloseDateFrom) && 
                    Convert.IsDBNull(datCloseDateTo))
                {
                    blnNoSaleCloseDateFilter = true;
                }

                if (blnNoCompanyFilter && blnNoSaleCloseDateFilter)
                {
                     rstresult = objDLFunctionLib.GetRecordset(modContact.strqREALTOR_SEARCH_ONLY, 5, vntFirstName, vntLastName,
                        vntEmail, vntWorkPhone, vntCellPhone, modContact.strfCONTACT_ID);
                }

                if (!blnNoCompanyFilter && !blnNoSaleCloseDateFilter)
                {
                     rstresult = objDLFunctionLib.GetRecordset(modContact.strqREALTOR_SEARCH_QUOTE_AND_COMPANY, 10, vntFirstName,
                        vntLastName, vntEmail, vntWorkPhone, vntCellPhone, datSaleDateFrom, datSaleDateTo, datCloseDateFrom,
                        datCloseDateTo, vntCompanyId, modContact.strfCONTACT_ID);
                }

                if (blnNoCompanyFilter && !blnNoSaleCloseDateFilter)
                {
                     rstresult = objDLFunctionLib.GetRecordset(modContact.strqREALTOR_SEARCH_QUOTE, 9, vntFirstName, vntLastName,
                        vntEmail, vntWorkPhone, vntCellPhone, datSaleDateFrom, datSaleDateTo, datCloseDateFrom, datCloseDateTo,
                        modContact.strfCONTACT_ID);
                }

                if (!blnNoCompanyFilter && blnNoSaleCloseDateFilter)
                {
                     rstresult = objDLFunctionLib.GetRecordset(modContact.strqREALTOR_SEARCH_COMPANY, 6, vntFirstName, vntLastName,
                        vntEmail, vntWorkPhone, vntCellPhone, vntCompanyId, modContact.strfCONTACT_ID);
                }

                return rstresult;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Produce a SQL expression (=) to compare strFieldName to the Id value, vntValue.
        /// </summary>
        /// <param name="strFieldName">Field to compare to</param>
        /// <param name="vntValue">has this value</param>
        /// <param name="vntblnAND">determines if need to prefix ' AND ' in front of expression.</param>
        /// <returns>SQL expression</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual string SQLEqualId(string strFieldName, object vntValue, ref bool vntblnAND, ref Command cmdCommand)
        {
            try
            {
                string strSQLEqual = String.Empty;

                if (!((Convert.IsDBNull(vntValue) || (vntValue == null))))
                {
                    if (vntblnAND)
                    {
                        strSQLEqual = " AND ";
                    }

                    strSQLEqual = strSQLEqual + " " + strFieldName + " = ? ";
                    cmdCommand.Parameters.Append(cmdCommand.CreateParameter("@" + strFieldName, DataTypeEnum.adBinary, ParameterDirectionEnum.adParamInput, 8, vntValue));
                    vntblnAND = true;
                }

                return strSQLEqual;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// If vntValue is not null or not empty, then produce a SQL expression (Like)
        /// to see if value of strFieldName starts with vntValue.
        /// </summary>
        /// <param name="strFieldName">Field to compare to</param>
        /// <param name="vntValue">has this value</param>
        /// <param name="vntblnAND">determines if need to prefix ' AND ' in front of expression.</param>
        /// <returns>SQL expression</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual string SQLStartsWith(string strFieldName, object vntValue, ref bool vntblnAND, ref Command cmdCommand)
        {
            try
            {
                string strSQLStartsWith = String.Empty;
                
                if (!(IsNullEmptyBlank(vntValue)))
                {
                    if (vntblnAND)
                    {
                        strSQLStartsWith = " AND ";
                    }

                    strSQLStartsWith = strSQLStartsWith + " " + strFieldName + " Like ? ";
                    cmdCommand.Parameters.Append(cmdCommand.CreateParameter("@" + strFieldName, DataTypeEnum.adVarChar, ParameterDirectionEnum.adParamInput, 50, TypeConvert.ToString(vntValue) + "%"));
                    vntblnAND = true;
                }

                return strSQLStartsWith;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }

        }

        /// <summary>
        /// If vntValue is not null or not empty, then produce a SQL expression (Like)
        /// to see if value of strFieldName contains vntValue.
        /// </summary>
        /// <param name="strFieldName">Field to compare to</param>
        /// <param name="vntValue">has this value</param>
        /// <param name="vntblnAND">determines if need to prefix ' AND ' in front of expression.</param>
        /// <returns>SQL expression</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual string SQLContains(string strFieldName, object vntValue, ref bool vntblnAND, ref Command cmdCommand)
        {
            try
            {
                string strSQLContains = String.Empty;
                if (!IsNullEmptyBlank(vntValue))
                {
                    if (vntblnAND)
                    {
                        strSQLContains = " AND ";
                    }
                    strSQLContains = strSQLContains + " " + strFieldName + " Like ? ";
                    cmdCommand.Parameters.Append(cmdCommand.CreateParameter("@" + strFieldName,DataTypeEnum.adVarChar,ParameterDirectionEnum.adParamInput, 50, "%" + TypeConvert.ToString(vntValue) + "%"));
                    vntblnAND = true;
                }
                return strSQLContains;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Produces a SQL expression to see if a value is within a range
        /// </summary>
        /// <param name="strFieldName">Field to compare to</param>
        /// <param name="vntStart">>= this value</param>
        /// <param name="vntEnd">less than= this value</param>
        /// <param name="vntblnAND">determines if need to prefix ' AND ' in front of expression.</param>
        /// <returns>SQL expression</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual string SQLRange(string strFieldName, object vntStart, object vntEnd, ref bool vntblnAND, ref Command cmdCommand)
        {
            try
            {
                string strSQLRange = String.Empty;
                bool blnInnerAND = false;

                if (!(IsNullEmptyBlank(vntStart)))
                {
                    if (vntblnAND)
                    {
                        strSQLRange = " AND ";
                    }

                    strSQLRange = strSQLRange + strFieldName + " >= ? ";
                    cmdCommand.Parameters.Append(cmdCommand.CreateParameter("@" + strFieldName, DataTypeEnum.adDate, ParameterDirectionEnum.adParamInput,8 ,TypeConvert.ToString(vntStart)));
                    vntblnAND = true;
                    blnInnerAND = true;
                }

                if (!(IsNullEmptyBlank(vntEnd)))
                {
                    if (blnInnerAND || (vntblnAND && IsNullEmptyBlank(vntStart)))
                    {
                        strSQLRange = strSQLRange + "AND ";
                    }

                    strSQLRange = strSQLRange + strFieldName + " <= ? ";
                    cmdCommand.Parameters.Append(cmdCommand.CreateParameter("@" + strFieldName, DataTypeEnum.adDate,ParameterDirectionEnum.adParamInput,50 , TypeConvert.ToString(vntEnd)));
                    vntblnAND = true;
                }

                return strSQLRange;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// If the variant is null, empty or blank string, then return true
        /// </summary>
        /// <param name="vntValue">variant to test</param>
        /// <returns>
        /// true if vntValue is null, empty or a blank string, else false</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual bool IsNullEmptyBlank(object vntValue)
        {
            try
            {
                bool bIsNullEmptyBlank = false;

                if (Convert.IsDBNull(vntValue))
                {
                    bIsNullEmptyBlank = true;
                }

                if (TypeConvert.ToString(vntValue).Trim().Length>0)
                {
                    bIsNullEmptyBlank = false;
                }
                else 
                {
                    bIsNullEmptyBlank = true; ;
                }

                return bIsNullEmptyBlank;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Called by the Contact Search business object. To search any combination of the
        /// fields based on the HB Contact Center Search form.
        /// </summary>
        /// <param name="strFormName">Contact search form name</param>
        /// <param name="rstContact">Recordset holds the contact search information</param>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual Recordset ContactSearch(string strFormName, Recordset rstContact)
        {
            try
            {
                object datSaleDateFrom = null;
                object datSaleDateTo = null;
                Command cmdCommand = new Command();

                DataAccess objDLFunctionLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                UIAccess objPLFunctionLib = (UIAccess)RSysSystem.ServerScripts[AppServerRuleData.UIAccessAppServerRuleName].CreateInstance();

                // Initializing parameter variables.
                // At the time of testing, passing ParameterList(x) to query didn't work, but
                // passing local variables worked.
                object blnInternetLead = rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfLEAD_TYPE,
                    modContact.strsCONTACT_SEARCH, null)].Value;
                object datLeadDateFrom = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfLEAD_DATE_FROM, modContact.strsDATES, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfLEAD_DATE_FROM, modContact.strsDATES, null)].Value;
                object datLeadDateTo = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfLEAD_DATE_TO, modContact.strsDATES, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfLEAD_DATE_TO, modContact.strsDATES, null)].Value;
                object datVisitDateFrom = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfVISIT_DATE_FROM, modContact.strsDATES, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfVISIT_DATE_FROM, modContact.strsDATES, null)].Value;
                object datVisitDateTo = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfVISIT_DATE_TO, modContact.strsDATES, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfVISIT_DATE_TO, modContact.strsDATES, null)].Value;
                object datInactiveDateFrom = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfINACTIVE_DATE_FROM, modContact.strsINACTIVE, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfINACTIVE_DATE_FROM, modContact.strsINACTIVE, null)].Value;
                object datInactiveDateTo = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfINACTIVE_DATE_TO, modContact.strsINACTIVE, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfINACTIVE_DATE_TO, modContact.strsINACTIVE, null)].Value;

                string vntFirstName = TypeConvert.ToString(rstContact.Fields[modContact.strfFIRST_NAME].Value);
                string vntLastName = TypeConvert.ToString(rstContact.Fields[modContact.strfLAST_NAME].Value);
                string vntWorkPhone = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfWORK_PHONE, modContact.strsCONTACT_SEARCH, null)].Value);
                string vntAddress1 = TypeConvert.ToString(rstContact.Fields[modContact.strfADDRESS_1].Value);
                string vntAddress2 = TypeConvert.ToString(rstContact.Fields[modContact.strfADDRESS_2].Value);
                string vntAddress3 = TypeConvert.ToString(rstContact.Fields[modContact.strfADDRESS_3].Value);
                string vntCity = TypeConvert.ToString(rstContact.Fields[modContact.strfCITY].Value);
                string vntCountyId = TypeConvert.ToString(rstContact.Fields[modContact.strfCOUNTY_ID].Value);
                string vntZip = TypeConvert.ToString(rstContact.Fields[modContact.strfZIP].Value);
                string vntLotNumber = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfLOT_NUMBER, modContact.strsCONTACT_SEARCH, null)].Value);
                string vntBULNumber = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfBUL_NUMBER, modContact.strsCONTACT_SEARCH, null)].Value);
                string vntServiceRequest = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfSERVICE_REQUEST, modContact.strsSERVICES_AND_WARRENTY, null)].Value);
                string vntServiceItem = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfSERVICE_ITEM, modContact.strsSERVICES_AND_WARRENTY, null)].Value);
                string vntWorkOrder = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfWORK_ORDER, modContact.strsSERVICES_AND_WARRENTY, null)].Value);

                object vntDivisionId = rstContact.Fields[modContact.strfDIVISION_ID].Value;
                object vntNeighborhoodId = rstContact.Fields[modContact.strfNEIGHBORHOOD_ID].Value;
                object vntType = TypeConvert.ToString(rstContact.Fields[modContact.strfCONTACT_PROFILE_NBHD_TYPE].Value);

                if (!(Convert.IsDBNull(blnInternetLead)) && TypeConvert.ToBoolean(blnInternetLead))
                {
                    vntType = modContact.strNP_TYPE_UA_MKT_LEAD;
                    object vntType2 = modContact.strNP_TYPE_MKT_LEAD;
                    object vntType3 = modContact.strNP_TYPE_UA_NBHD_LEAD;
                    object vntType4 = modContact.strNP_TYPE_NBHD_LEAD;
                }

                string vntHomePhone = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfHOME_PHONE, modContact.strsCONTACT_SEARCH, null)].Value);
                string vntCellPhone = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfCELL_PHONE, modContact.strsCONTACT_SEARCH, null)].Value);
                string vntEmail = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfEMAIL, modContact.strsCONTACT_SEARCH, null)].Value);
                string vntCoBuyerFirstName = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfCO_BUYER_FIRST_NAME, modContact.strsCONTACT_SEARCH, null)].Value);
                string vntCoBuyerLastName = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfCO_BUYER_LAST_NAME, modContact.strsCONTACT_SEARCH, null)].Value);
                string vntCoBuyerHomePHone = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfCO_BUYER_HOME_PHONE, modContact.strsCONTACT_SEARCH, null)].Value);
                string vntCoBuyerWorkPhone = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfCO_BUYER_WORK_PHONE, modContact.strsCONTACT_SEARCH, null)].Value);
                string vntCoBuyerCellPhone = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfCO_BUYER_CELL_PHONE, modContact.strsCONTACT_SEARCH, null)].Value);
                string vntCoBuyerEmail = TypeConvert.ToString(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName,
                    modContact.strfCO_BUYER_EMAIL, modContact.strsCONTACT_SEARCH, null)].Value);

                object vntLeadSourceId = rstContact.Fields[modContact.strfLEAD_SOURCE_ID].Value;
                object vntSecLeadSourceId = rstContact.Fields[modContact.strfCS_SECONDARY_LEAD_SOURCE_ID].Value;

                object datQuoteDateFrom = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfQUOTE_DATE_FROM, modContact.strsDATES, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfQUOTE_DATE_FROM, modContact.strsDATES, null)].Value;
                object datQuoteDateTo = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfQUOTE_DATE_TO, modContact.strsDATES, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfQUOTE_DATE_TO, modContact.strsDATES, null)].Value;
                object datCloseDateFrom = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfCLOSE_DATE_FROM, modContact.strsCLOSED, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfCLOSE_DATE_FROM, modContact.strsCLOSED, null)].Value;
                object datCloseDateTo = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfCLOSE_DATE_TO, modContact.strsCLOSED, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfCLOSE_DATE_TO, modContact.strsCLOSED, null)].Value;
                object datCancelDateFrom = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfCANCEL_DATE_FROM, modContact.strsCANCELLED, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfCANCEL_DATE_FROM, modContact.strsCANCELLED, null)].Value;
                object datCancelDateTo = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfCANCEL_DATE_TO, modContact.strsCANCELLED, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfCANCEL_DATE_TO, modContact.strsCANCELLED, null)].Value;

                switch (TypeConvert.ToString(vntType))
                {
                    case modContact.strNP_TYPE_BUYER:
                        datSaleDateFrom = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_FROM, modContact.strsBUYERS, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_FROM, modContact.strsBUYERS, null)].Value;
                        datSaleDateTo = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_TO, modContact.strsBUYERS, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_TO, modContact.strsBUYERS, null)].Value;
                        break;
                    case modContact.strNP_TYPE_CANCELLED:
                        datSaleDateFrom = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_FROM, modContact.strsCANCELLED, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_FROM, modContact.strsCANCELLED, null)].Value;
                        datSaleDateTo = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_TO, modContact.strsCANCELLED, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_TO, modContact.strsCANCELLED, null)].Value;
                        break;
                    case modContact.strNP_TYPE_CLOSED:
                        datSaleDateFrom = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_FROM, modContact.strsCLOSED, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_FROM, modContact.strsCLOSED, null)].Value;
                        datSaleDateTo = Convert.IsDBNull(rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_TO, modContact.strsCLOSED, null)].Value) ? "" /* EMPTY */ : rstContact.Fields[objPLFunctionLib.GetDisconnectedFieldName(strFormName, modContact.strfSALE_DATE_TO, modContact.strsCLOSED, null)].Value;
                        break;
                    default:
                        datSaleDateFrom = string.Empty;
                        datSaleDateTo = string.Empty;
                        break;
                }

                object vntInactiveReasonId = rstContact.Fields[modContact.strfCS_INACTIVE_REASON].Value;
                object vntCancelReasonId = rstContact.Fields[modContact.strfCS_CANCEL_REASON_ID].Value;
                object vntDNCStatus = rstContact.Fields[modContact.strfDNC_STATUS].Value;
                object vntPriorityCodeId = rstContact.Fields[modContact.strfCS_PRIORITY_CODE_ID].Value;

                // Initialization
                bool blnLeadContact = false;
                bool blnContProfNbhd = false;
                bool blnSerRequest = false;
                bool blnSerItem = false;
                bool blnWorkOrder = false;
                bool blnLot = false;
                bool blnTrafficSource = false;
                bool blnCoBuyer = false;
                bool blnQuote = false;

                // Searching on Co-Buyer table?
                if (!(IsNullEmptyBlank(vntCoBuyerFirstName)) || !(IsNullEmptyBlank(vntCoBuyerLastName)) || !(IsNullEmptyBlank(vntCoBuyerWorkPhone))
                    || !(IsNullEmptyBlank(vntCoBuyerHomePHone)) || !(IsNullEmptyBlank(vntCoBuyerCellPhone)) || !(IsNullEmptyBlank(vntCoBuyerEmail)))
                {
                    blnCoBuyer = true;
                }

                // Searching on Lead and Contact tables?
                if (!(IsNullEmptyBlank(vntFirstName)) || !(IsNullEmptyBlank(vntLastName)) || !(IsNullEmptyBlank(vntAddress1))
                    || !(IsNullEmptyBlank(vntCity)) || !(IsNullEmptyBlank(vntZip)) || !(IsNullEmptyBlank(vntCellPhone))
                    || !(IsNullEmptyBlank(vntHomePhone)) || !(IsNullEmptyBlank(vntEmail)) || !(IsNullEmptyBlank(vntWorkPhone)))
                {
                    blnLeadContact = true;
                }

                // Searching on Contact Profile Neighborhood table?
                if (!(IsNullEmptyBlank(vntType)) || TypeConvert.ToBoolean(blnInternetLead) || !(IsNullEmptyBlank(datVisitDateFrom))
                    || !(IsNullEmptyBlank(datVisitDateTo)) || !(IsNullEmptyBlank(datQuoteDateFrom)) || !(IsNullEmptyBlank(datQuoteDateTo))
                    || !(IsNullEmptyBlank(datLeadDateFrom)) || !(IsNullEmptyBlank(datLeadDateTo)) || !(IsNullEmptyBlank(datSaleDateFrom))
                    || !(IsNullEmptyBlank(datSaleDateTo)) || !(IsNullEmptyBlank(datCloseDateFrom)) || !(IsNullEmptyBlank(datCloseDateTo))
                    || !(IsNullEmptyBlank(datCancelDateFrom)) || !(IsNullEmptyBlank(datCancelDateTo)) || !(IsNullEmptyBlank(datInactiveDateFrom))
                    || !(IsNullEmptyBlank(datInactiveDateTo)) || !(IsNullEmptyBlank(vntInactiveReasonId)) || !(IsNullEmptyBlank(vntPriorityCodeId))
                    || !(IsNullEmptyBlank(vntDivisionId)) || !(IsNullEmptyBlank(vntNeighborhoodId)) || !(IsNullEmptyBlank(vntLeadSourceId))
                    || !(IsNullEmptyBlank(vntSecLeadSourceId)))
                {
                    blnContProfNbhd = true;
                }

                // Search on Service and Warrenty related tables?
                if (!(IsNullEmptyBlank(vntServiceRequest)))
                {
                    blnSerRequest = true;
                }

                if (!(IsNullEmptyBlank(vntServiceItem)))
                {
                    blnSerItem = true;
                }

                if (!(IsNullEmptyBlank(vntWorkOrder)))
                {
                    blnWorkOrder = true;
                }

                if (!(IsNullEmptyBlank(vntLotNumber)) || !(IsNullEmptyBlank(vntBULNumber)))
                {
                    blnLot = true;
                }

                // Search on Traffic Source table?
                if (!(IsNullEmptyBlank(vntSecLeadSourceId)))
                {
                    blnTrafficSource = true;
                }

                if (!(IsNullEmptyBlank(vntCancelReasonId)))
                {
                    blnQuote = true;
                }

                bool blnAddAnd = false;

                // This query is made up of 2 main select statements unioned togather for performance. First one is
                // the Contact's Contact Profile Neighborhoods and second is for the
                // Lead's Contact Profile Neighborhoods.
                string strSQL = "Select TOP 1000 CPN0.Contact_Profile_Nbhd_Id , CPN0.Rn_Descriptor, CPN0.Rn_Create_Date, CPN0.Rn_Edit_Date, CPN0.Rn_Create_User, CPN0.Rn_Edit_User, CPN0.Contact_Id, CPN0.Lead_Id, CPN0.LeadContact_Descriptor ";
                strSQL = strSQL + " From Contact_Profile_Neighborhood CPN0";

                if (blnTrafficSource)
                {
                    strSQL = strSQL + " join Traffic_Source on CPN0.Contact_Profile_Nbhd_Id = Traffic_Source.Contact_Profile_NBHD_Id ";
                }

                strSQL = strSQL + " where ";

                // Contact Profile Neighborhood expressions
                if (blnContProfNbhd)
                {
                    if (TypeConvert.ToBoolean(blnInternetLead))
                    {
                        if (blnAddAnd)
                        {
                            strSQL = strSQL + " AND ";
                        }

                        strSQL = strSQL + " Type In ('" + modContact.strNP_TYPE_UA_MKT_LEAD + "', '"; strSQL = strSQL + modContact.strNP_TYPE_MKT_LEAD + "', '";
                        strSQL = strSQL + modContact.strNP_TYPE_UA_NBHD_LEAD + "', '";
                        strSQL = strSQL + modContact.strNP_TYPE_NBHD_LEAD + "') ";
                        blnAddAnd = true;
                    }
                    else
                    {
                        strSQL = strSQL + SQLStartsWith(modContact.strfTYPE, vntType, ref blnAddAnd, ref cmdCommand);
                    }
                    strSQL = strSQL + SQLRange("CPN0." + modContact.strfLEAD_DATE, datLeadDateFrom, datLeadDateTo, ref blnAddAnd, ref cmdCommand);
                    strSQL = strSQL + SQLRange("CPN0." + modContact.strfFIRST_VISIT_DATE, datVisitDateFrom, datVisitDateTo, ref blnAddAnd, ref cmdCommand);
                    strSQL = strSQL + SQLRange("CPN0.Quote_Date", datQuoteDateFrom, datQuoteDateTo, ref blnAddAnd, ref cmdCommand);
                    strSQL = strSQL + SQLRange("CPN0.Sale_Date", datSaleDateFrom, datSaleDateTo, ref blnAddAnd, ref cmdCommand);
                    strSQL = strSQL + SQLRange("CPN0.Close_Date", datCloseDateFrom, datCloseDateTo, ref blnAddAnd, ref cmdCommand);
                    strSQL = strSQL + SQLRange("CPN0.Cancel_Date", datCancelDateFrom, datCancelDateTo, ref blnAddAnd, ref cmdCommand);
                    strSQL = strSQL + SQLRange("CPN0.Inactive_Date", datInactiveDateFrom, datInactiveDateTo, ref blnAddAnd, ref cmdCommand);
                    strSQL = strSQL + SQLEqualId("CPN0.Inactive_Reason_Id", vntInactiveReasonId, ref blnAddAnd, ref cmdCommand);
                    // strSQL = strSQL & SQLEqual("CPN0.Cancel_Reason_Id", vntCancelReasonId, blnAddAnd)
                    strSQL = strSQL + SQLEqualId("CPN0.Priority_Code_Id", vntPriorityCodeId, ref blnAddAnd, ref cmdCommand);
                    strSQL = strSQL + SQLEqualId("CPN0.Division_Id", vntDivisionId, ref blnAddAnd, ref cmdCommand);
                    strSQL = strSQL + SQLEqualId("CPN0.Neighborhood_Id", vntNeighborhoodId, ref blnAddAnd, ref cmdCommand);
                    strSQL = strSQL + SQLEqualId("CPN0.Marketing_Project_Id", vntLeadSourceId, ref blnAddAnd, ref cmdCommand);

                    // Traffic Source expression
                    if (blnTrafficSource)
                    {
                        strSQL = strSQL + SQLEqualId("Traffic_Source.Marketing_Project_Id", vntSecLeadSourceId, ref blnAddAnd, ref cmdCommand);
                    }
                }

                // Lead, Contact or child records?
                if (blnLeadContact || blnCoBuyer || blnSerRequest || blnSerItem || blnWorkOrder || blnLot || blnQuote)
                {
                    if (blnAddAnd)
                    {
                        strSQL = strSQL + " AND ";
                    }

                    strSQL = strSQL + "CPN0.Contact_Profile_Nbhd_Id In ( ";
                    // SELECT for Contact and child records
                    strSQL = strSQL + "Select CPN.Contact_Profile_Nbhd_Id ";
                    strSQL = strSQL + "FROM Contact_Profile_Neighborhood CPN ";
                    strSQL = strSQL + " join Contact on CPN.Contact_Id = Contact.Contact_id ";

                    if (blnCoBuyer)
                    {
                        strSQL = strSQL + " join Contact_Cobuyer on CPN.Contact_Id = Contact_CoBuyer.Contact_id ";
                        strSQL = strSQL + " join Contact Cobuyer on Contact_CoBuyer.Co_Buyer_Contact_Id = CoBuyer.Contact_id ";
                    }

                    if (blnSerRequest || blnSerItem || blnWorkOrder)
                    {
                        strSQL = strSQL + " join Support_Incident SI on Contact.Contact_Id = SI.Contact_Id  ";
                    }

                    if (blnSerItem)
                    {
                        strSQL = strSQL + " join Support_Step SS on SI.Support_Incident_Id = SS.Support_Incident_Id  ";
                    }

                    if (blnWorkOrder)
                    {
                        strSQL = strSQL + " join Work_Order WO on SI.Support_Incident_Id = WO.Service_Request_Id  ";
                    }

                    if (blnQuote)
                    {
                        strSQL = strSQL + " join Opportunity Opp on Contact.Contact_Id = Opp.Contact_Id ";
                    }

                    // Build WHERE clause
                    strSQL = strSQL + "WHERE ";
                    blnAddAnd = false;

                    // Lead and Contact expressions
                    if (blnLeadContact)
                    {
                        strSQL = strSQL + SQLStartsWith("Contact." + modContact.strfFIRST_NAME, vntFirstName, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Contact." + modContact.strfLAST_NAME, vntLastName, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Contact." + modContact.strfADDRESS_1, vntAddress1, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Contact." + modContact.strfCITY, vntCity, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Contact." + modContact.strfZIP, vntZip, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Contact." + modContact.strfEMAIL, vntEmail, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLContains("Contact." + modContact.strfWORK_PHONE, vntWorkPhone, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLContains("Contact." + modContact.strfCELL, vntCellPhone, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLContains("Contact." + modContact.strfPHONE, vntHomePhone, ref blnAddAnd, ref cmdCommand);

                    }

                    // Co Buyer expressions
                    if (blnCoBuyer)
                    {
                        strSQL = strSQL + SQLStartsWith("CoBuyer." + modContact.strfFIRST_NAME, vntCoBuyerFirstName, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("CoBuyer." + modContact.strfLAST_NAME, vntCoBuyerLastName, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("CoBuyer." + modContact.strfEMAIL, vntCoBuyerEmail, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLContains("CoBuyer." + modContact.strfWORK_PHONE, vntCoBuyerWorkPhone, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLContains("CoBuyer." + modContact.strfPHONE, vntCoBuyerHomePHone, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLContains("CoBuyer." + modContact.strfCELL, vntCoBuyerCellPhone, ref blnAddAnd, ref cmdCommand);
                    }

                    // Service Requests / Service  / Work Orders expressions
                    if (blnSerRequest)
                    {
                        strSQL = strSQL + SQLStartsWith("SI.Request_Number", vntServiceRequest, ref blnAddAnd, ref cmdCommand);
                    }

                    if (blnSerItem)
                    {
                        strSQL = strSQL + SQLStartsWith("SS.Service_Item_Number", vntServiceItem, ref blnAddAnd, ref cmdCommand);
                    }

                    if (blnWorkOrder)
                    {
                        strSQL = strSQL + SQLStartsWith("WO.Work_Order_Number", vntWorkOrder, ref blnAddAnd, ref cmdCommand);
                    }

                    // Quotes expression
                    if (blnQuote)
                    {
                        strSQL = strSQL + SQLEqualId("Opp.Cancel_Reason_Id", vntCancelReasonId, ref blnAddAnd, ref cmdCommand);
                    }

                    // Lots expressions
                    // Using UNION for performance
                    if (blnLot)
                    {
                        if (blnAddAnd)
                        {
                            strSQL = strSQL + " AND ";
                        }

                        strSQL = strSQL + " CPN.Contact_Profile_NBHD_ID IN ("; strSQL = strSQL + " (SELECT CPN3.Contact_Profile_NBHD_ID ";
                        strSQL = strSQL + " FROM Contact_Profile_Neighborhood CPN3 JOIN Product Lot on CPN3.Contact_Id = Lot.Owner_id ";
                        strSQL = strSQL + " WHERE ";
                        blnAddAnd = false;
                        strSQL = strSQL + SQLStartsWith("Lot.Lot_Number", vntLotNumber, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Lot.Business_Unit_Lot_Number", vntBULNumber, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + " )";
                        strSQL = strSQL + " UNION ";
                        strSQL = strSQL + " (SELECT CPN4.Contact_Profile_NBHD_ID "; strSQL = strSQL + " FROM Contact_Profile_Neighborhood CPN4 join Lot__Contact LotContact on CPN4.Contact_Id = LotContact.Contact_Id  ";
                        strSQL = strSQL + " WHERE ";
                        blnAddAnd = false;
                        strSQL = strSQL + SQLStartsWith("LotContact.Lot_Number", vntLotNumber, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("LotContact.Business_Unit_Lot_Number", vntBULNumber, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + " )  )";

                    }

                    // SELECT for Leads and Lead's Contact Profile Neighborhoods
                    if (blnLeadContact || blnCoBuyer)
                    {
                        strSQL = strSQL + " UNION Select CPN2.Contact_Profile_Nbhd_Id ";
                        strSQL = strSQL + "FROM Contact_Profile_Neighborhood CPN2 ";
                        strSQL = strSQL + " join Lead_ on CPN2.Lead_Id = Lead_.Lead__Id  ";
                        strSQL = strSQL + "WHERE ";

                        blnAddAnd = false;
                        strSQL = strSQL + SQLStartsWith("Lead_." + modContact.strfFIRST_NAME, vntFirstName, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Lead_." + modContact.strfLAST_NAME, vntLastName, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Lead_." + modContact.strfADDRESS_1, vntAddress1, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Lead_." + modContact.strfCITY, vntCity, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Lead_." + modContact.strfZIP, vntZip, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Lead_." + modContact.strfEMAIL, vntEmail, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLContains("Lead_." + modContact.strfWORK_PHONE, vntWorkPhone, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLContains("Lead_." + modContact.strfCELL, vntCellPhone, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLContains("Lead_." + modContact.strfPHONE, vntHomePhone, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Lead_." + modContact.strfCO_BUYER_FIRST_NAME, vntCoBuyerFirstName, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Lead_." + modContact.strfCO_BUYER_LAST_NAME, vntCoBuyerLastName, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLStartsWith("Lead_." + modContact.strfCO_BUYER_EMAIL, vntCoBuyerEmail, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLContains("Lead_." + modContact.strfCO_BUYER_WORK_PHONE, vntCoBuyerWorkPhone, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLContains("Lead_." + modContact.strfCO_BUYER_PHONE, vntCoBuyerHomePHone, ref blnAddAnd, ref cmdCommand);
                        strSQL = strSQL + SQLContains("Lead_." + modContact.strfCO_BUYER_CELL, vntCoBuyerCellPhone, ref blnAddAnd, ref cmdCommand);
                    }

                    strSQL = strSQL + "  )";
                }
                // If blnLeadContact

                // Get user specific security
                string strUserWhere = RSysSystem.GetFilterWhereClause(RSysSystem.Tables[modContact.strtCONTACT_PROFILE_NBHD].TableId,
                    null);

                if ((strUserWhere.Trim()).Length > 0)
                {
                    strUserWhere = strUserWhere.Replace("Contact_Profile_Neighborhood", "CPN0");
                    strSQL = strSQL + " AND (" + strUserWhere + ")";
                }

                Connection objConn = new Connection();
                
                try
                {
                    objConn.Open(RSysSystem.EnterpriseString, "", "", -1);
                }
                catch
                {
                    objConn = null;
                }
                
                Recordset rstresult = new Recordset();
                rstresult.CursorLocation = (CursorLocationEnum)CursorLocationEnum.adUseClient;
                objConn.CommandTimeout = 600;
                cmdCommand.ActiveConnection = objConn;
                cmdCommand.CommandText = strSQL;
                rstresult.Open(cmdCommand, Type.Missing, (CursorTypeEnum)CursorTypeEnum.adOpenStatic, (LockTypeEnum)LockTypeEnum.adLockReadOnly, -1);
                rstresult.ActiveConnection = null;

                return rstresult;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Called by the tree control on the Contacts web tab to link a new contact to the co-buyer
        /// </summary>
        /// <returns>
        /// -</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        public virtual void LinkContactCoBuyer(object vntParentId, object vntCoBuyerId, string strRelationship)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                if ((strRelationship == null))
                {
                    strRelationship = "";
                }

                if ((vntParentId is Array) && (vntCoBuyerId is Array))
                {
                    Recordset rstContactCoBuyer = objLib.GetNewRecordset(modContact.strtCONTACT_COBUYER, modContact.strfCOBUYER_CONTACT_ID,
                        modContact.strfCONTACT_ID);
                    rstContactCoBuyer.AddNew(modContact.strfCONTACT_ID, DBNull.Value);
                    rstContactCoBuyer.Fields[modContact.strfCONTACT_ID].Value = vntParentId;
                    rstContactCoBuyer.Fields[modContact.strfCOBUYER_CONTACT_ID].Value = vntCoBuyerId;
                    objLib.SaveRecordset(modContact.strtCONTACT_COBUYER, rstContactCoBuyer);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Called by the tree control, gets all contacts of type Buyer and Prospect
        /// </summary>
        // None
        /// <returns>
        /// Recordset of contacts of type prospects and customers</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        public virtual Recordset GetContacts(object vntContactId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                return objLib.GetRecordset(modContact.strqALL_CONTACTS_OF_TYPE_CUST, 3, vntContactId, vntContactId,
                    vntContactId, modContact.strfCONTACT_ID);
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Called by the tree control, deletes a co-buyer from the tree.
        /// </summary>
        // None
        /// <returns>
        ///</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        public virtual void DeleteCoBuyer(object vntParentId, object vntCoBuyerId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstContactCoBuyer = objLib.GetRecordset(modContact.strqCOBUYER_FOR_CONT_COBUYER, 2, vntParentId, vntCoBuyerId,
                    modContact.strfCONTACT_COBUYER_ID);

                if (!(rstContactCoBuyer.EOF) && !(rstContactCoBuyer.BOF))
                {
                    rstContactCoBuyer.MoveFirst();
                    objLib.DeleteRecord(rstContactCoBuyer.Fields[modContact.strfCONTACT_COBUYER_ID].Value, modContact.strtCONTACT_COBUYER);
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Returns true if :
        ///     - at least one cobuyer
        ///     - has Account Manager
        ///     - there is a realtor associated
        /// </summary>
        /// <returns>
        /// Recordset of contacts of type prospects and customers</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        public virtual bool HasEmailRecipients(object vntContactId)
        {
            try
            {
                bool bHasEmailRecipients = false;

                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstContact = objLib.GetRecordset(vntContactId, modContact.strtCONTACT, modContact.strfACCOUNT_MANAGER_ID,
                    modContact.strfREALTOR_ID);

                if (rstContact.RecordCount > 0)
                {
                    if (Convert.IsDBNull(rstContact.Fields[modContact.strfACCOUNT_MANAGER_ID].Value) && Convert.IsDBNull(rstContact.Fields[modContact.strfREALTOR_ID].Value))
                    {
                        Recordset rstCoBuyers = objLib.GetRecordset(modContact.strqCO_BUYERS_FOR_CONTACT, 1, vntContactId, modContact.strfCONTACT_ID);
                        
                        if (rstCoBuyers.RecordCount > 0)
                        {
                            bHasEmailRecipients = true;
                        }
                    }
                    else
                    {
                        bHasEmailRecipients = true;
                    }
                }

                return bHasEmailRecipients;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Called by the tree control, checks before overwriting the cobuyer's address with the buyer's addr
        /// if they are the same. If the Co-buyer address is null then the the address is copied over.
        /// </summary>
        /// <param name="vntContactBuyerId">Buyer ID</param>
        /// <param name="vntContactCoBuyerId">Co Buyer Id</param>
        /// <param name="rstContact"></param>
        /// <param name="blnFromTree"></param>
        /// <returns>
        /// True, if its the same, false if different</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        public virtual bool IsBuyerCoBuyerAddressSame(object vntContactBuyerId, object vntContactCoBuyerId, ref Recordset
            rstContact, bool blnFromTree)
        {
            try
            {

                bool bIsBuyerCoBuyerAddressSame = true;
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                Recordset rstContactBuyer = objLib.GetRecordset(vntContactBuyerId, modContact.strtCONTACT, modContact.strfADDRESS_1,
                    modContact.strfADDRESS_2, modContact.strfADDRESS_3, modContact.strfCITY, modContact.strfSTATE, modContact.strfZIP,
                    modContact.strfAREA_CODE, modContact.strfCOUNTY_ID, modContact.strfCONTACT_ID, modContact.strfCOUNTRY, modContact.strfFAX);
                if (!(rstContactBuyer.EOF) && !(rstContactBuyer.BOF))
                {
                    Recordset rstContactCoBuyer = objLib.GetRecordset(vntContactCoBuyerId, modContact.strtCONTACT, modContact.strfADDRESS_1,
                        modContact.strfADDRESS_2, modContact.strfADDRESS_3, modContact.strfCITY, modContact.strfSTATE,
                        modContact.strfCOUNTY_ID, modContact.strfZIP, modContact.strfCOUNTRY, modContact.strfFAX);

                    if (!(rstContactCoBuyer.EOF) && !(rstContactCoBuyer.BOF))
                    {
                        // if cobuyer is all null, go ahead and do a straight copy
                        if ((System.DBNull.Value.Equals(rstContactCoBuyer.Fields[modContact.strfADDRESS_1].Value) ==
                            true ) && System.DBNull.Value.Equals(rstContactCoBuyer.Fields[modContact.strfADDRESS_2].Value)
                            == true && System.DBNull.Value.Equals(rstContactCoBuyer.Fields[modContact.strfADDRESS_3].Value)
                            == true && System.DBNull.Value.Equals(rstContactCoBuyer.Fields[modContact.strfCITY].Value)
                            == true && System.DBNull.Value.Equals(rstContactCoBuyer.Fields[modContact.strfSTATE].Value)
                            == true && System.DBNull.Value.Equals(rstContactCoBuyer.Fields[modContact.strfCOUNTY_ID].Value)
                            == true && System.DBNull.Value.Equals(rstContactCoBuyer.Fields[modContact.strfCOUNTRY].Value)
                            == true && System.DBNull.Value.Equals(rstContactCoBuyer.Fields[modContact.strfZIP].Value)
                            == true && System.DBNull.Value.Equals(rstContactCoBuyer.Fields[modContact.strfFAX].Value)
                            == true)
                        {
                            bIsBuyerCoBuyerAddressSame = true;
                            // no need to do anything else, therefore sent this to client
                            rstContact = rstContactBuyer;
                            if (blnFromTree)
                            {
                                CopyBuyerAddressToCoBuyer(vntContactBuyerId, vntContactCoBuyerId);
                            }
                        }
                        else
                        {
                            // compare the two addresses
                            if (rstContactBuyer.Fields[modContact.strfADDRESS_1].Value.Equals(rstContactCoBuyer.Fields[modContact.strfADDRESS_1].Value)
                                == false || rstContactBuyer.Fields[modContact.strfADDRESS_2].Value.Equals(rstContactCoBuyer.Fields[modContact.strfADDRESS_2].Value)
                                == false || rstContactBuyer.Fields[modContact.strfADDRESS_3].Value.Equals(rstContactCoBuyer.Fields[modContact.strfADDRESS_3].Value)
                                == false || rstContactBuyer.Fields[modContact.strfCITY].Value.Equals(rstContactCoBuyer.Fields[modContact.strfCITY].Value)
                                == false || rstContactBuyer.Fields[modContact.strfSTATE].Value.Equals(rstContactCoBuyer.Fields[modContact.strfSTATE].Value)
                                == false || RSysSystem.IdToString(rstContactBuyer.Fields[modContact.strfCOUNTY_ID].Value).Equals(RSysSystem.IdToString(rstContactCoBuyer.Fields[modContact.strfCOUNTY_ID].Value))
                                == false || rstContactBuyer.Fields[modContact.strfCOUNTRY].Value.Equals(rstContactCoBuyer.Fields[modContact.strfCOUNTRY].Value)
                                == false || rstContactBuyer.Fields[modContact.strfZIP].Value.Equals(rstContactCoBuyer.Fields[modContact.strfZIP].Value)
                                == false || rstContactBuyer.Fields[modContact.strfFAX].Value.Equals(rstContactCoBuyer.Fields[modContact.strfFAX].Value)
                                == false)
                            {
                                bIsBuyerCoBuyerAddressSame = false;
                                rstContact = rstContactBuyer;
                            }
                            else
                            {
                                // reset the id if from the tree
                                if (blnFromTree)
                                {
                                    CopyBuyerAddressToCoBuyer(vntContactBuyerId, vntContactCoBuyerId);
                                }

                            }
                        }
                    }
                }

                return bIsBuyerCoBuyerAddressSame;
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Copies the address from the Buyer into the Co-Buyer record. Sets the Flag on the Contact and link table
        /// </summary>
        /// <param name="vntContactId">Contact Id</param>
        /// <param name="vntCoBuyerId">Contact Co-buyer Id</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// 5.9.0.0     8/16/2007   BC          Modified the code to copy fax data.
        /// </history>
        public virtual void CopyBuyerAddressToCoBuyer(object vntContactId, object vntCoBuyerId)
        {
            try
            {
                DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

                if ((vntContactId is Array))
                {
                    Recordset rstContact = objLib.GetRecordset(vntContactId, modContact.strtCONTACT, modContact.strfADDRESS_1,
                        modContact.strfADDRESS_2, modContact.strfADDRESS_3, modContact.strfCITY, modContact.strfSTATE,
                        modContact.strfCOUNTY_ID, modContact.strfZIP, modContact.strfAREA_CODE, modContact.strfCOUNTRY, modContact.strfFAX);

                    if (!(rstContact.EOF) && !(rstContact.BOF))
                    {
                        if ((vntCoBuyerId is Array))
                        {
                            Recordset rstContactCoBuyer = objLib.GetRecordset(vntCoBuyerId, modContact.strtCONTACT, modContact.strfADDRESS_1,
                                modContact.strfADDRESS_2, modContact.strfADDRESS_3, modContact.strfCITY, modContact.strfSTATE,
                                modContact.strfCOUNTY_ID, modContact.strfZIP, modContact.strfHAS_SAME_ADDR_ID, modContact.strfCOUNTRY, modContact.strfFAX);
                            rstContactCoBuyer.Fields[modContact.strfADDRESS_1].Value = rstContact.Fields[modContact.strfADDRESS_1].Value;
                            rstContactCoBuyer.Fields[modContact.strfADDRESS_2].Value = rstContact.Fields[modContact.strfADDRESS_2].Value;
                            rstContactCoBuyer.Fields[modContact.strfADDRESS_3].Value = rstContact.Fields[modContact.strfADDRESS_3].Value;
                            rstContactCoBuyer.Fields[modContact.strfCITY].Value = rstContact.Fields[modContact.strfCITY].Value;
                            rstContactCoBuyer.Fields[modContact.strfSTATE].Value = rstContact.Fields[modContact.strfSTATE].Value;
                            rstContactCoBuyer.Fields[modContact.strfCOUNTY_ID].Value = rstContact.Fields[modContact.strfCOUNTY_ID].Value;
                            rstContactCoBuyer.Fields[modContact.strfCOUNTRY].Value = rstContact.Fields[modContact.strfCOUNTRY].Value;
                            rstContactCoBuyer.Fields[modContact.strfZIP].Value = rstContact.Fields[modContact.strfZIP].Value;
                            rstContactCoBuyer.Fields[modContact.strfFAX].Value = rstContact.Fields[modContact.strfFAX].Value;
                            rstContactCoBuyer.Fields[modContact.strfHAS_SAME_ADDR_ID].Value = vntContactId;
                            objLib.SaveRecordset(modContact.strtCONTACT, rstContactCoBuyer);
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// Helper sub to copy field value from one recordset to another
        /// </summary>
        /// <param name="rstSource">source recordset</param>
        /// <param name="rstTarget">target recordset</param>
        /// <param name="strFieldName">name of field to be copied</param>
        /// <returns>
        /// None</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// 3.8.0.0     5/5/2006    svadivu     Converted to .Net C# code.
        /// </history>
        protected virtual void CopyFieldValue(Recordset rstSource, Recordset rstTarget, String strFieldName)
        {
            if (Convert.IsDBNull(rstSource.Fields[strFieldName].Value) == false)
            {
                rstTarget.Fields[strFieldName].Value = rstSource.Fields[strFieldName].Value;
            }
        }
        /// <summary>
        /// To check whether the contactId is already in rn_contact_sync table
        /// </summary>
        /// <param name="ObjContactId">ContactId</param>
        /// <returns>
        /// Boolean</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        /// HB9.0      31/1/2007    AR           Osm Requirement 
        /// </history>

        protected virtual Boolean GetOSMStatus(object ObjContactId)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstActContStep = objLib.GetRecordset(modContact.strfCONTACT_SYNC_RECORD, 
                    2, ObjContactId, RSysSystem.CurrentUserId(), modContact.strfCONTACT_ID, 
                    modContact.strfUSER_ID);

                if (rstActContStep.RecordCount > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

        /// <summary>
        /// To add the contactId into rn_contact_sync Table
        /// </summary>
        /// <param name="ObjContactId">ContactId</param>
        /// <returns>
        /// nothing</returns>
        /// <history>
        /// Revision#   Date        Author      Description
        ///  HB9.0      31/1/2007   AR          Osm Requirement 
        /// </history>
        protected virtual void AddtoOSM(object ObjContactId)
        {
            try
            {
                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                object strFieldNames = (Object)new string[] { modContact.strfCONTACT_ID, modContact.strfUSER_ID };
                Recordset rstContactDup = dataAccess.GetNewRecordset(modContact.strtRN_CONTACT_SYNC, strFieldNames);
                rstContactDup.AddNew(Type.Missing, Type.Missing);
                rstContactDup.Fields[modContact.strfCONTACT_ID].Value = ObjContactId;
                rstContactDup.Fields[modContact.strfUSER_ID].Value = RSysSystem.CurrentUserId();
                dataAccess.SaveRecordset(modContact.strtRN_CONTACT_SYNC, rstContactDup);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }

        }

        /// <summary>
        /// To add the multiple contactId's into rn_contact_sync Table
        /// </summary>
        /// <param name="strAllContactIds">String containing ContactId's</param>
        /// <returns>
        /// String Array of length two with first having Contact Id's inserted for synchronization
        /// and second having Contact Id's already present in rn_contact_sync table
        /// </returns>
        /// <history>
        /// Revision#   Date        Author      Description
        ///  HB9.0      31/1/2007   AR          Osm Requirement 
        /// </history>
        protected virtual string[] AddMultipletoOSM(string strAllContactIds)
        {
            try
            {
                DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                string[] strArr = strAllContactIds.Split(',');
                string[] strReturn = new string[2];
                foreach (string strVal in strArr)
                {
                    object objContactId = RSysSystem.StringToId(strVal);
                    Recordset rstContact = objLib.GetRecordset(objContactId, modContact.strtCONTACT, modContact.strfFIRST_NAME, modContact.strfLAST_NAME);
                    if (!GetOSMStatus(objContactId))
                    {
                        AddtoOSM(objContactId);
                        strReturn[0] += rstContact.Fields[modContact.strfFIRST_NAME].Value + " " + rstContact.Fields[modContact.strfLAST_NAME].Value + ",";
                    }
                    else
                    {
                        strReturn[1] += rstContact.Fields[modContact.strfFIRST_NAME].Value + " " + rstContact.Fields[modContact.strfLAST_NAME].Value + ",";
                    }
                }
                return (strReturn);
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
          
        }
    }
}


