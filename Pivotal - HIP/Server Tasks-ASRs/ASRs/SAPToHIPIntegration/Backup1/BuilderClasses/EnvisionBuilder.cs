//
// $Workfile: EnvisionBuilder.cs$
// $Revision: 2$
// $Author: RYong$
// $Date: Wednesday, December 19, 2007 3:41:53 PM$
//
// Copyright © Pivotal Corporation
//


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;

using Pivotal.Interop.RDALib;
using Pivotal.Interop.ADODBLib;

using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;



namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.BuilderClasses
{
    /// <summary>
    /// Class for populating the Buyer and Home Xml object.
    /// </summary>
    public class EnvisionBuilder
    {
        // refrence to the Envision Asr
        private EnvisionIntegration m_envisionIntegration;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envisionIntegration">Reference to the Envision Integration ASR</param>
        public EnvisionBuilder(EnvisionIntegration envisionIntegration)
        {
            this.m_envisionIntegration = envisionIntegration;
        }

        /// <summary>
        /// Serializes an instance to an xml string
        /// </summary>
        /// <param name="o">Instance to serialize</param>
        /// <returns>Xml string</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string SerializeToXmlString(object o)
        {
            string returnString = string.Empty;
            using (StringWriter stringWriter = new StringWriter(CultureInfo.CurrentCulture))
            {
                // create the serializer
                XmlSerializer xmlSerializer = new XmlSerializer(o.GetType());

                // Serialize
                xmlSerializer.Serialize(stringWriter, o);
                stringWriter.Flush();
                returnString = stringWriter.ToString();
            }

            return returnString;
        }


        /// <summary>
        /// Returns the short version of a Pivotal Id
        /// </summary>
        /// <param name="pivotalId">Pivotal Id Array</param>
        /// <returns>Id string</returns>
        protected virtual string CompactPivotalId(object pivotalId)
        {
            string idString = this.m_envisionIntegration.PivotalSystem.IdToString(pivotalId);
            return string.Format(CultureInfo.CurrentCulture, "{0:X}", Convert.ToInt64(idString, 16));
        }

        /// <summary>
        /// This function generates the Home number for Envision by concatenating the contract Id and the homesite Id.
        /// </summary>
        /// <param name="opportunityId">Contract Id</param>
        /// <param name="productId">Homesite Id</param>
        /// <returns>Envision Home number.</returns>
        public virtual string GenerateHomeNumber(object opportunityId, object productId)
        {
            //2008-01-09 AB need to handle inventory quotes so will have to look first if inventory quote exists
            object vntInvID = this.m_envisionIntegration.PivotalSystem.Tables[OpportunityData.TableName].Fields["MI_Originating_Inv_Quote"].FindValue(
                        this.m_envisionIntegration.PivotalSystem.Tables[OpportunityData.TableName].Fields[OpportunityData.OpportunityIdField],
                        opportunityId);
            //2008-06-03 AB removed job number from unique ID to be able to support transfers
            //string strJobNumber = (string)this.m_envisionIntegration.PivotalSystem.Tables[ProductData.TableName].Fields[ProductData.JobNumberField].FindValue(
                        //this.m_envisionIntegration.PivotalSystem.Tables[ProductData.TableName].Fields[ProductData.ProductIdField],
                        //productId);

            if (vntInvID != DBNull.Value && vntInvID != null)
            {
                //return strJobNumber + " - " + CompactPivotalId(vntInvID);
                return CompactPivotalId(vntInvID);
            }
            else
            {
                //return strJobNumber + " - " + CompactPivotalId(opportunityId);
                return CompactPivotalId(opportunityId);
            }
            //return string.Format(CultureInfo.CurrentCulture, "{0}:{1}", CompactPivotalId(opportunityId), CompactPivotalId(productId));  //Contact.Contact_Id (formatting?)

        }

        /// <summary>
        /// This function generates the Buyer Number for Envision by concatenating the contract Id and the Contact Id.
        /// </summary>
        /// <param name="opportunityId">Contract Id</param>
        /// <param name="contactId">Contact Id</param>
        /// <returns>Buyer Number.</returns>
        public virtual string GenerateBuyerNumber(object opportunityId, object contactId)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}:{1}", CompactPivotalId(opportunityId), CompactPivotalId(contactId));  //Contact.Contact_Id (formatting?)
        }

        #region Buyer
        /// <summary>
        /// Generates the FinancialInfo entity from the Contract (Opportunity)
        /// </summary>
        /// <param name="opportunityId">The Pivotal Oppportunity record id from which to generate the FinancialInfo</param>
        /// <param name="loanProfileIds">Returns an array for Loan Profile Id's that were used to generate the FinancialInfo</param>
        /// <param name="loanId">Returns the Loan Id used to generate the FinancialInfo</param>
        /// <returns></returns>
        protected virtual EnvisionXsdGenerated.BuyerTypeFinancialInfo GetBuyerFinancialInfo(object opportunityId, out byte[][] loanProfileIds, out byte[] loanId)
        {
            EnvisionXsdGenerated.BuyerTypeFinancialInfo info = null;
            List<byte[]> loanProfileIdList = new List<byte[]>();
            loanId = new byte[] { };

            // get the Loan Profile records for the Contract
            Recordset loanProfileRecords = this.m_envisionIntegration.PivotalDataAccess.GetRecordset(LoanProfileData.QueryLoanProfilesForQuote, 1, new object[] { opportunityId, LoanProfileData.SelectedField, LoanProfileData.Loan1IdField, LoanProfileData.Loan1AmtField, LoanProfileData.Loan1IntField, LoanProfileData.DownPmtField });
            try
            {
                if (loanProfileRecords.RecordCount > 0)
                {
                    int selectedLoans = 0;
                    loanProfileRecords.MoveFirst();
                    while (!loanProfileRecords.EOF)
                    {
                        // first - added the Loan Profile Id to the 'used Loan Profiles' list so that if there
                        // is a failure the LoanProfiles involved and be identified
                        // note - all Loan Profile Ids must be in this list, including unselected ones.
                        loanProfileIdList.Add((byte[])loanProfileRecords.Fields[LoanProfileData.LoanProfileIdField].Value);

                        // only use the Loan Profile if it is selected.  There should only be one selected
                        // at any time.
                        object tmp = loanProfileRecords.Fields[LoanProfileData.SelectedField].Value;
                        if (tmp == DBNull.Value)
                        {
                            throw new PivotalApplicationException((string)this.m_envisionIntegration.LangDictionary.GetText("ExceptionUnexpectedDatabaseNull"));
                        }
                        else
                        {
                            if ((bool)tmp)
                            {
                                // double check that only one Loan Profile is selected.  If 2 or more are
                                // selected throw error.
                                selectedLoans++;
                                if (selectedLoans > 1) throw new PivotalApplicationException(((string)this.m_envisionIntegration.LangDictionary.GetText("ExceptionUnexpectedNumberOfLoansSelected")));

                                // populate FinancialInfo entity handling Nulls
                                info = new CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated.BuyerTypeFinancialInfo();
                                tmp = loanProfileRecords.Fields[LoanProfileData.Loan1AmtField].Value; // Loan_Profile.Loan1_Amt
                                if (tmp != DBNull.Value)
                                {
                                    info.MortgagePreQualAmountSpecified = true;
                                    info.MortgagePreQualAmount = new decimal((double)tmp);
                                }

                                tmp = loanProfileRecords.Fields[LoanProfileData.Loan1IntField].Value; // Load_Profile.Loan1_Int
                                if (tmp != DBNull.Value)
                                {
                                    info.InterestRateSpecified = true;
                                    info.InterestRate = new decimal((double)tmp / 100);  // InterestRate is expressed as a decimal, not a percentage
                                }

                                tmp = loanProfileRecords.Fields[LoanProfileData.DownPmtField].Value; // Loan_Profile.Down_Pmt
                                if (tmp != DBNull.Value)
                                {
                                    info.DownPaymentSpecified = true;
                                    info.DownPayment = new decimal((double)tmp);
                                }

                                tmp = loanProfileRecords.Fields[LoanProfileData.Loan1IdField].Value;
                                if (tmp != DBNull.Value)
                                {
                                    loanId = (byte[])tmp;
                                    tmp = this.m_envisionIntegration.PivotalSystem.Tables[LoanData.TableName].Fields[LoanData.TermField].Index(loanId);
                                    if (tmp != DBNull.Value)
                                    {
                                        info.LoanTerm = tmp.ToString();
                                    }
                                }
                            }
                        }

                        loanProfileRecords.MoveNext();
                    }
                }
            }
            finally
            {
                // create an array from the LoanProfileIdList
                loanProfileIds = loanProfileIdList.ToArray();

                loanProfileRecords.Close();
            }


            return info;
        }


        /// <summary>
        /// Generates an Envision CoBuyer array given a Pivotal Contact Id
        /// </summary>
        /// <param name="contactId">Pivotal Contact Id from which to generate the CoBuyer Array</param>
        /// <param name="coBuyerContactIds">Returns an array of CoBuyerContactIds that were used to generate the Envision CoBuyer array.</param>
        /// <returns>Envision CoBuyer array</returns>
        protected virtual EnvisionXsdGenerated.BuyerTypeCobuyer[] GetCoBuyers(object contactId, out byte[][] coBuyerContactIds)
        {
            List<EnvisionXsdGenerated.BuyerTypeCobuyer> coBuyerList = new List<CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated.BuyerTypeCobuyer>();
            List<byte[]> coBuyerContactIdList = new List<byte[]>();

            // Get the co-buyer recordset
            Recordset contactRecords = this.m_envisionIntegration.PivotalDataAccess.GetRecordset(ContactData.QueryCoBuyerContactsForContact, 1, new object[] { contactId, ContactData.ContactIdField, ContactData.FirstNameField, ContactData.LastNameField, ContactData.TitleField });
            try
            {

                if (contactRecords.RecordCount > 0)
                {
                    contactRecords.MoveFirst();
                    while (!contactRecords.EOF)
                    {
                        // first, add the contact id to the used list.
                        coBuyerContactIdList.Add((byte[])contactRecords.Fields[ContactData.ContactIdField].Value);

                        // populate fields
                        EnvisionXsdGenerated.BuyerTypeCobuyer coBuyer = new EnvisionXsdGenerated.BuyerTypeCobuyer();
                        object tmp = contactRecords.Fields[ContactData.TitleField].Value;
                        if (tmp != DBNull.Value) coBuyer.Title = (string)tmp;

                        // required
                        coBuyer.FirstName = (string)contactRecords.Fields[ContactData.FirstNameField].Value;

                        coBuyer.MiddleName = string.Empty;

                        // required
                        coBuyer.LastName = (string)contactRecords.Fields[ContactData.LastNameField].Value;

                        coBuyerList.Add(coBuyer);
                        contactRecords.MoveNext();
                    }
                }
            }
            finally
            {
                // create an array of Pivotal Ids from the Id List
                coBuyerContactIds = coBuyerContactIdList.ToArray();

                contactRecords.Close();
            }

            return coBuyerList.ToArray();
        }


        /// <summary>
        /// Generates an Envision Buyer entity given a Pivotal Contract and Contact Ids 
        /// </summary>
        /// <param name="opportunityId">Pivotal Contract Id</param>
        /// <param name="contactId">Pivotal Contact Id</param>
        /// <param name="coBuyerContactIds">Returns an array of Contact Ids that represent the CoBuyers used to construct the Envision Buyer</param>
        /// <param name="loanProfileIds">Returns an array of Loan Profile Ids use to contruct the Envision Buyer</param>
        /// <param name="loanId">Returns the Loan Id used to contruct the Envision Buyer</param>
        /// <returns>Envision Buyer Entity</returns>
        public virtual EnvisionXsdGenerated.Buyer GetBuyer(object opportunityId, object contactId, out byte[][] coBuyerContactIds, out byte[][] loanProfileIds, out byte[] loanId)
        {
            EnvisionXsdGenerated.Buyer buyer;
            // queries the home buyer record.
            Recordset records = this.m_envisionIntegration.PivotalDataAccess.GetRecordset(contactId, ContactData.TableName, new string[] { });

            try
            {
                // there should only be one record in recordset
                if (records.RecordCount == 1)
                {
                    buyer = new EnvisionXsdGenerated.Buyer();

                    //required
                    buyer.BuyerNumber = GenerateBuyerNumber(opportunityId, contactId);

                    object tmp;
                    tmp = records.Fields[ContactData.TitleField].Value;  //Contact.Title
                    if (tmp != DBNull.Value) buyer.Title = (string)tmp;

                    // required
                    buyer.FirstName = (string)records.Fields[ContactData.FirstNameField].Value; //Contact.First_Name

                    // required
                    buyer.LastName = (string)records.Fields[ContactData.LastNameField].Value;
                    if (records.Fields[ContactData.SuffixField].Value != DBNull.Value)
                    {
                        buyer.LastName = buyer.LastName + " " + (string)records.Fields[ContactData.SuffixField].Value;
                    }

                    buyer.MiddleName = string.Empty; //leave empty

                    tmp = records.Fields[ContactData.EmailField].Value;
                    if (tmp != DBNull.Value)
                        buyer.Email = (string)tmp;

                    // validate and set user id
                    tmp = this.m_envisionIntegration.PivotalDataAccess.SqlIndex(OpportunityData.TableName, OpportunityData.EnvEDCUsernameField, opportunityId);
                    if (tmp == DBNull.Value)
                        throw new PivotalApplicationException((string)this.m_envisionIntegration.LangDictionary.GetText("ExceptionEdcUserIdIsNull"));
                    else
                    {
                        string str = (string)tmp;
                        if (string.IsNullOrEmpty(str))
                            throw new PivotalApplicationException((string)this.m_envisionIntegration.LangDictionary.GetText("ExceptionEdcUserIdIsNull"));
                        else
                            buyer.UserName = str;
                    }

                    //validate and set password
                    tmp = this.m_envisionIntegration.PivotalDataAccess.SqlIndex(OpportunityData.TableName, OpportunityData.EnvEDCPasswordField, opportunityId);
                    if (tmp == DBNull.Value)
                        throw new PivotalApplicationException((string)this.m_envisionIntegration.LangDictionary.GetText("ExceptionEdcPasswordIsNull"));
                    else
                    {
                        string str = (string)tmp;
                        if (string.IsNullOrEmpty(str))
                            throw new PivotalApplicationException((string)this.m_envisionIntegration.LangDictionary.GetText("ExceptionEdcPasswordIsNull"));
                        else
                            buyer.Password = str;
                    }

                    EnvisionXsdGenerated.BuyerTypeAddressesAddress address = new EnvisionXsdGenerated.BuyerTypeAddressesAddress();
                    
                    //07-01-08 AB Custom code to allow for country code lookups
                    if (records.Fields[ContactData.CountryField].Value != null && records.Fields[ContactData.CountryField].Value != DBNull.Value)
                    {

                       MI_Envision_Utility util = new CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.MI_Envision_Utility();

                        string countryCode = util.GetTranslation("Pivotal", "E1", records.Fields[ContactData.CountryField].Value.ToString(), "CountryCode", this.m_envisionIntegration.PivotalSystem);
                        if (countryCode != "")
                        {
                            tmp = countryCode;
                        }
                        else
                        {
                            tmp = records.Fields[ContactData.CountryField].Value;
                        }
                        //tmp = records.Fields[ContactData.CountryField].Value;
                        if (tmp != DBNull.Value) address.Country = (string)tmp;
                    }
                    
                    
                    tmp = records.Fields[ContactData.StateField].Value;
                    if (tmp != DBNull.Value) address.State = (string)tmp;

                    tmp = records.Fields[ContactData.CityField].Value;
                    if (tmp != DBNull.Value) address.City = (string)tmp;

                    tmp = records.Fields[ContactData.ZipField].Value;
                    if (tmp != DBNull.Value) address.Zip = (string)tmp;


                    List<string> addressList = new List<string>();

                    //address 1
                    tmp = records.Fields[ContactData.Address1Field].Value;
                    if (tmp != DBNull.Value)
                        if ((string)tmp != string.Empty)
                            addressList.Add((string)tmp);

                    //address 2
                    tmp = records.Fields[ContactData.Address2Field].Value;
                    if (tmp != DBNull.Value)
                        if ((string)tmp != string.Empty)
                            addressList.Add((string)tmp);

                    //address 3
                    tmp = records.Fields[ContactData.Address3Field].Value;
                    if (tmp != DBNull.Value)
                        if ((string)tmp != string.Empty)
                            addressList.Add((string)tmp);

                    // place all into address 1
                    if (addressList.Count > 0)
                        address.StreetAddress1 = string.Join(", ", addressList.ToArray());


                    buyer.Addresses = new EnvisionXsdGenerated.BuyerTypeAddresses();
                    buyer.Addresses.Address = address;

                    buyer.Cobuyers = GetCoBuyers(contactId, out coBuyerContactIds);

                    buyer.FinancialInfo = GetBuyerFinancialInfo(opportunityId, out loanProfileIds, out loanId);

                    string preferredContact = "(none)";
                    tmp = records.Fields[ContactData.PreferredContactField].Value;
                    if (tmp != DBNull.Value) preferredContact = (string)tmp;

                    List<EnvisionXsdGenerated.PhoneType> list = new List<EnvisionXsdGenerated.PhoneType>();
                    // the order of the below specifies the default primary/secondary phone number rank
                    this.AddPhoneNumber(list, records.Fields[ContactData.CellField].Value, string.Equals("Cell Phone", preferredContact));
                    this.AddPhoneNumber(list, records.Fields[ContactData.PhoneField].Value, string.Equals("Home Phone", preferredContact));
                    this.AddPhoneNumber(list, records.Fields[ContactData.WorkPhoneField].Value, string.Equals("Work Phone", preferredContact));

                    // set primary secondary flags based on order
                    if (list.Count > 0) list[0].Type = CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated.PhoneTypeType.Primary;
                    if (list.Count > 1) list[1].Type = CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated.PhoneTypeType.Secondary;

                    //remove third+ number(s) if exists
                    if (list.Count > 2) list.RemoveRange(2, list.Count - 2);

                    buyer.Phones = list.ToArray();
                }
                else
                {
                    throw new PivotalApplicationException((string)this.m_envisionIntegration.LangDictionary.GetText("ExceptionUnexpectedNofRecords"));
                }
            }
            finally
            {
                records.Close();
            }

            return buyer;
        }

        /// <summary>
        /// Adds an Envision PhoneType entity to the list
        /// </summary>
        /// <param name="list">List of PhoneTypes</param>
        /// <param name="value">Phone number to add</param>
        /// <param name="preferredContact">Whether the phone number is preferred/primary or not</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        protected virtual void AddPhoneNumber(List<EnvisionXsdGenerated.PhoneType> list, object value, bool preferredContact)
        {
            // only add if phone # is in string form
            if (value is string)
            {
                string phoneNumber = value as string;
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    EnvisionXsdGenerated.PhoneType phoneType = new EnvisionXsdGenerated.PhoneType();
                    phoneType.Number = phoneNumber;

                    // set phone number type
                    if (preferredContact)
                        list.Insert(0, phoneType);   //primary, add to start of list
                    else
                        list.Add(phoneType);  //secondary add to bottom of list
                }
            }
        }
        #endregion

        #region Home
        /// <summary>
        /// This function populates the Home xml for sending in the Envision Home web service.
        /// </summary>
        /// <param name="opportunityId">Contract Id</param>
        /// <param name="productId">Homesite Id</param>
        /// <param name="nhtNumber">Builder NHT Number</param>
        /// <returns>Home Xml object.</returns>
        public virtual EnvisionXsdGenerated.Home GetHome(object opportunityId, object productId, string nhtNumber)
        {
            EnvisionXsdGenerated.Home home = null;

            SetHomeAttributes(out home, opportunityId, productId);
            //AAB 2010-05-17 Update to include new custom option fields
            //Get the standard options and selected options (from Pivotal Quote only), excluding package components.
            Recordset optionsInQuote = this.m_envisionIntegration.PivotalDataAccess.GetRecordset(OpportunityProductData.StandardAndSelectedOptionsForOpportunityQueryName,
                1, opportunityId,
                OpportunityProductData.OpportunityProductIdField,
                OpportunityProductData.NBHDPProductIdField,
                OpportunityProductData.DivisionProductIdField,
                OpportunityProductData.QuantityField,
                OpportunityProductData.ExtendedPriceField,
                OpportunityProductData.LocationIdField,
                OpportunityProductData.RnCreateDateField,
                OpportunityProductData.CustomerInstructionsField,
                OpportunityProductData.OptionNotesField,
                OpportunityProductData.ProductNameField,
                OpportunityProductData.ProductNumberField,
                OpportunityProductData.CategoryIdField,
                OpportunityProductData.SubCategoryIdField,
                "MI_Style_Notes", "MI_Color_Notes")
                ;

            //2007-12-18 AB Create util for later use
            MI_Envision_Utility util = new MI_Envision_Utility();

            List<EnvisionXsdGenerated.SelectedOptionType> selectedOptionList = new List<EnvisionXsdGenerated.SelectedOptionType>();
            if (optionsInQuote.RecordCount > 0)
            {
                optionsInQuote.MoveFirst();

                int count = 0;
                EnvisionXsdGenerated.SelectedOptionType selectedOption = null;

                while (!optionsInQuote.EOF)
                {
                    //bypass corrupt data
                    Recordset divisionProduct = null;
                    count = 0;
                    if (optionsInQuote.Fields[DivisionProductData.DivisionProductIdField].Value != DBNull.Value) //not custom option
                    {
                        try
                        {
                            divisionProduct = this.m_envisionIntegration.PivotalDataAccess.GetRecordset(
                                optionsInQuote.Fields[OpportunityProductData.DivisionProductIdField].Value,
                                DivisionProductData.TableName,
                                DivisionProductData.DivisionIdField,
                                DivisionProductData.RegionIdField,
                                DivisionProductData.SubCategoryIdField);
                            count = divisionProduct.RecordCount;
                        }
                        catch
                        {
                            count = 0;
                        }
                    }

                    Recordset rstOppLocs = m_envisionIntegration.PivotalDataAccess.GetRecordset(OppProductLocationData.QueryOppLocationsToSyncForOppProduct,
                        1, optionsInQuote.Fields[OpportunityProductData.OpportunityProductIdField].Value,
                        OppProductLocationData.LocationIdField,
                        OppProductLocationData.LocationQuantityField,
                        OppProductLocationData.NotesField,
                        OppProductLocationData.OppProductLocationIdField);

                    if (count > 0)
                    {
                        if (rstOppLocs.RecordCount == 1) //Whole House or Specific
                        {
                            selectedOption = new EnvisionXsdGenerated.SelectedOptionType();
                            SetSelectedOptionLocationInfo(ref selectedOption, divisionProduct, nhtNumber);
                            //2007-12-18 AB START Option number and category are based on JDE specific values
                            //selectedOption.OptionNumber = CompactPivotalId(optionsInQuote.Fields[OpportunityProductData.DivisionProductIdField].Value);
                            //selectedOption.CategoryNumber = CompactPivotalId(divisionProduct.Fields[DivisionProductData.SubCategoryIdField].Value);
                            string[] productInfo = util.GetNbhdpProductById(optionsInQuote.Fields[OpportunityProductData.NBHDPProductIdField].Value, this.m_envisionIntegration.PivotalSystem);
                            selectedOption.OptionNumber = productInfo[0];

                            if (productInfo[2] == "")
                            {
                                selectedOption.CategoryNumber = productInfo[1];
                            }
                            else
                            {
                                selectedOption.CategoryNumber = productInfo[2];
                            }
                            //2007-12-18 AB END
                            selectedOption.Quantity = TypeConvert.ToString(optionsInQuote.Fields[OpportunityProductData.QuantityField].Value);
                            selectedOption.PreSelected = false; //current functional spec does not need to distinguish standard options from regular options.
                            selectedOption.ValidateProductLinks = false;
                            selectedOption.Price = TypeConvert.ToDecimal(optionsInQuote.Fields[OpportunityProductData.ExtendedPriceField].Value);
                            //2008-08-11 AB Added product validation field
                            selectedOption.ValidateAvailability = false;

                            rstOppLocs.MoveFirst();
                            if (rstOppLocs.Fields[OppProductLocationData.LocationIdField].Value != DBNull.Value) //not Whole House                               
                            {
                                selectedOption.RoomNumber = CompactPivotalId(rstOppLocs.Fields[OppProductLocationData.LocationIdField].Value);
                            }

                            selectedOption.TransactionDate = TypeConvert.ToDateTime(optionsInQuote.Fields[OpportunityProductData.RnCreateDateField].Value);

                            //AAB 2010-05-17 notes are from opp products
                            /*if (rstOppLocs.Fields[OppProductLocationData.NotesField].Value != DBNull.Value)
                            {
                                //add notes from Opp_Product_Location table to Envision's Other notes.
                                List<EnvisionXsdGenerated.NoteType> notesList = new List<EnvisionXsdGenerated.NoteType>();
                                EnvisionXsdGenerated.NoteType notes = new EnvisionXsdGenerated.NoteType();
                                notes.Type = EnvisionXsdGenerated.NoteTypeType.Other;
                                notes.Text = TypeConvert.ToString(rstOppLocs.Fields[OppProductLocationData.NotesField].Value);
                                notesList.Add(notes);
                                selectedOption.Notes = notesList.ToArray();
                            }*/

                            if (optionsInQuote.Fields["MI_Style_Notes"].Value != DBNull.Value || optionsInQuote.Fields["MI_Color_Notes"].Value != DBNull.Value || optionsInQuote.Fields["CustomerInstructions"].Value != DBNull.Value || optionsInQuote.Fields["OptionNotes"].Value != DBNull.Value) 
                            {
                                //add notes from Opp_Product_Location table to Envision's Other notes.
                                List<EnvisionXsdGenerated.NoteType> notesList = new List<EnvisionXsdGenerated.NoteType>();
                                if (optionsInQuote.Fields["MI_Style_Notes"].Value != DBNull.Value)
                                {
                                    EnvisionXsdGenerated.NoteType styleNotes = new EnvisionXsdGenerated.NoteType();
                                    styleNotes.Type = EnvisionXsdGenerated.NoteTypeType.Style;
                                    styleNotes.Text = TypeConvert.ToString(optionsInQuote.Fields["MI_Style_Notes"].Value);
                                    notesList.Add(styleNotes);
                                }
                                if (optionsInQuote.Fields["OptionNotes"].Value != DBNull.Value)
                                {
                                    EnvisionXsdGenerated.NoteType oNotes = new EnvisionXsdGenerated.NoteType();
                                    oNotes.Type = EnvisionXsdGenerated.NoteTypeType.Other;
                                    oNotes.Text = TypeConvert.ToString(optionsInQuote.Fields["OptionNotes"].Value);
                                    notesList.Add(oNotes);
                                }
                                if (optionsInQuote.Fields["MI_Color_Notes"].Value != DBNull.Value)
                                {
                                    EnvisionXsdGenerated.NoteType colorNotes = new EnvisionXsdGenerated.NoteType();
                                    colorNotes.Type = EnvisionXsdGenerated.NoteTypeType.Color;
                                    colorNotes.Text = TypeConvert.ToString(optionsInQuote.Fields["MI_Color_Notes"].Value);
                                    notesList.Add(colorNotes);
                                }
                                if (optionsInQuote.Fields["CustomerInstructions"].Value != DBNull.Value)
                                {
                                    EnvisionXsdGenerated.NoteType ciNotes = new EnvisionXsdGenerated.NoteType();
                                    ciNotes.Type = EnvisionXsdGenerated.NoteTypeType.Location;
                                    ciNotes.Text = TypeConvert.ToString(optionsInQuote.Fields["CustomerInstructions"].Value);
                                    notesList.Add(ciNotes);
                                }
                                selectedOption.Notes = notesList.ToArray();
                            }

                            selectedOptionList.Add(selectedOption);

                        }
                        else if (rstOppLocs.RecordCount > 1) //Multiple locations or All locations
                        {
                            rstOppLocs.MoveFirst();
                            while (!rstOppLocs.EOF)
                            {
                                selectedOption = new EnvisionXsdGenerated.SelectedOptionType();
                                SetSelectedOptionLocationInfo(ref selectedOption, divisionProduct, nhtNumber);
                                selectedOption.OptionNumber = CompactPivotalId(optionsInQuote.Fields[OpportunityProductData.DivisionProductIdField].Value);
                                selectedOption.CategoryNumber = CompactPivotalId(divisionProduct.Fields[DivisionProductData.SubCategoryIdField].Value);

                                selectedOption.Quantity = TypeConvert.ToString(rstOppLocs.Fields[OppProductLocationData.LocationQuantityField].Value);
                                selectedOption.PreSelected = false; //current functional spec does not need to distinguish standard options from regular options.
                                selectedOption.ValidateProductLinks = false;
                                selectedOption.Price =
                                    TypeConvert.ToDecimal(optionsInQuote.Fields[OpportunityProductData.ExtendedPriceField].Value) *
                                    TypeConvert.ToDecimal(selectedOption.Quantity) /
                                    TypeConvert.ToDecimal(optionsInQuote.Fields[OpportunityProductData.QuantityField].Value);

                                selectedOption.RoomNumber = CompactPivotalId(rstOppLocs.Fields[OppProductLocationData.LocationIdField].Value);
                                selectedOption.TransactionDate = TypeConvert.ToDateTime(optionsInQuote.Fields[OpportunityProductData.RnCreateDateField].Value);

                                //2010-01-25 AB Added mapping for new notes
                                /*if (rstOppLocs.Fields[OppProductLocationData.NotesField].Value != DBNull.Value || optionsInQuote.Fields[OpportunityProductData.OptionNotesField].Value != DBNull.Value || optionsInQuote.Fields[OpportunityProductData.CustomerInstructionsField].Value != DBNull.Value)
                                {
                                    List<EnvisionXsdGenerated.NoteType> notesList = new List<EnvisionXsdGenerated.NoteType>();
                                    if (rstOppLocs.Fields[OppProductLocationData.NotesField].Value != DBNull.Value)
                                    {
                                        //add notes from Opp_Product_Location table to Envision's Style notes.
                                        EnvisionXsdGenerated.NoteType notesStyle = new EnvisionXsdGenerated.NoteType();
                                        notesStyle.Type = EnvisionXsdGenerated.NoteTypeType.Style;
                                        notesStyle.Text = TypeConvert.ToString(rstOppLocs.Fields[OppProductLocationData.NotesField].Value);
                                        notesList.Add(notesStyle);
                                    }

                                    if (optionsInQuote.Fields[OpportunityProductData.OptionNotesField].Value != DBNull.Value)
                                    {
                                        //add notes from Opportunity_Product.Option_Notes table to Envision's Other notes.
                                        EnvisionXsdGenerated.NoteType notesOther = new EnvisionXsdGenerated.NoteType();
                                        notesOther.Type = EnvisionXsdGenerated.NoteTypeType.Other;
                                        notesOther.Text = TypeConvert.ToString(optionsInQuote.Fields[OpportunityProductData.OptionNotesField].Value);
                                        notesList.Add(notesOther);
                                    }

                                    if (optionsInQuote.Fields[OpportunityProductData.CustomerInstructionsField].Value != DBNull.Value)
                                    {
                                        //add notes from Opportunity_Product.Customer_Instructions table to Envision's Location notes.
                                        EnvisionXsdGenerated.NoteType notesLoc = new EnvisionXsdGenerated.NoteType();
                                        notesLoc.Type = EnvisionXsdGenerated.NoteTypeType.Location;
                                        notesLoc.Text = TypeConvert.ToString(optionsInQuote.Fields[OpportunityProductData.CustomerInstructionsField].Value);
                                        notesList.Add(notesLoc);
                                    }

                                    selectedOption.Notes = notesList.ToArray();
                                }*/
                                if (rstOppLocs.Fields[OppProductLocationData.NotesField].Value != DBNull.Value)
                                {
                                    List<EnvisionXsdGenerated.NoteType> notesList = new List<EnvisionXsdGenerated.NoteType>();
                                    //add notes from Opp_Product_Location table to Envision's Other notes.
                                    EnvisionXsdGenerated.NoteType notes = new EnvisionXsdGenerated.NoteType();
                                    notes.Type = EnvisionXsdGenerated.NoteTypeType.Other;
                                    notes.Text = TypeConvert.ToString(rstOppLocs.Fields[OppProductLocationData.NotesField].Value);
                                    notesList.Add(notes);
                                    

                                    selectedOption.Notes = notesList.ToArray();
                                }

                                selectedOptionList.Add(selectedOption);

                                rstOppLocs.MoveNext();
                            }
                        }
                    }
                    else if (optionsInQuote.Fields[OpportunityProductData.DivisionProductIdField].Value == DBNull.Value) //Custom option
                    {
                        if (rstOppLocs.RecordCount == 0) //Whole House
                        {
                            selectedOption = new EnvisionXsdGenerated.SelectedOptionType();
                            selectedOption.OptionType = CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated.SelectedOptionTypeOptionType.Custom;
                            selectedOption.OptionTypeSpecified = true;
                            selectedOption.OptionName = TypeConvert.ToString(optionsInQuote.Fields[OpportunityProductData.ProductNameField].Value);
                            selectedOption.OptionDescription = TypeConvert.ToString(optionsInQuote.Fields[OpportunityProductData.OptionNotesField].Value);
                            selectedOption.LocationNumber = string.Empty;
                            selectedOption.LocationLevel = string.Empty;
                            //AB 08-11-08 Added product availability validation
                            selectedOption.ValidateAvailability = false;
                            //if it is custom option use product_number as OptionNumber
                            //selectedOption.OptionNumber = CompactPivotalId(optionsInQuote.Fields[OpportunityProductData.OpportunityProductIdField].Value) + ":";
                            selectedOption.OptionNumber = TypeConvert.ToString(optionsInQuote.Fields[OpportunityProductData.ProductNumberField].Value);
                            //AB 07-28-08 must use category codes
                            //if (optionsInQuote.Fields[OpportunityProductData.CategoryIdField].Value != DBNull.Value)
                            //{
                            //    selectedOption.CategoryNumber = CompactPivotalId(optionsInQuote.Fields[OpportunityProductData.CategoryIdField].Value);
                            //}
                            //else
                            //{
                            //    selectedOption.CategoryNumber = string.Empty;
                            //}
                            if (optionsInQuote.Fields[OpportunityProductData.SubCategoryIdField].Value == DBNull.Value || optionsInQuote.Fields[OpportunityProductData.SubCategoryIdField].Value == null)
                            {
                                if (optionsInQuote.Fields[OpportunityProductData.CategoryIdField].Value != DBNull.Value)
                                {
                                   
                                    string catCode = (string)this.m_envisionIntegration.PivotalSystem.Tables["Configuration_Type"].Fields["Code_"].FindValue(
                                        this.m_envisionIntegration.PivotalSystem.Tables["Configuration_Type"].Fields["Configuration_Type_Id"],
                                        optionsInQuote.Fields[OpportunityProductData.CategoryIdField].Value);

                                    selectedOption.CategoryNumber = catCode;
                                }
                                else
                                {
                                    selectedOption.CategoryNumber = string.Empty;
                                }
                            }

                            else
                            {
                                string subCatCode = (string)this.m_envisionIntegration.PivotalSystem.Tables["Sub_Category"].Fields["MI_Code"].FindValue(
                                        this.m_envisionIntegration.PivotalSystem.Tables["Sub_Category"].Fields["Sub_Category_Id"],
                                        optionsInQuote.Fields[OpportunityProductData.SubCategoryIdField].Value);
                                selectedOption.CategoryNumber = subCatCode;
                            }
                            selectedOption.Quantity = TypeConvert.ToString(optionsInQuote.Fields[OpportunityProductData.QuantityField].Value);
                            selectedOption.PreSelected = false; //current functional spec does not need to distinguish standard options from regular options.
                            selectedOption.ValidateProductLinks = false;
                            selectedOption.Price = TypeConvert.ToDecimal(optionsInQuote.Fields[OpportunityProductData.ExtendedPriceField].Value);
                            selectedOption.TransactionDate = TypeConvert.ToDateTime(optionsInQuote.Fields[OpportunityProductData.RnCreateDateField].Value);


                            selectedOptionList.Add(selectedOption);
                        }
                        else if (rstOppLocs.RecordCount > 0)
                        {
                            //Log exception.  Custom options that originate from Pivotal side do not have locations (rooms).
                            throw new PivotalApplicationException((string)m_envisionIntegration.LangDictionary.GetText("ExceptionCustomOptionsNoLocation"));
                        }
                    }
                    else
                    {
                        throw new PivotalApplicationException((string)this.m_envisionIntegration.LangDictionary.GetText("ExceptionUnexpectedNofRecords"));
                    }


                    optionsInQuote.MoveNext();
                }
            }

            home.SelectedOptions = selectedOptionList.ToArray();

            return home;
        }

        /// <summary>
        /// Sets the home xml attributes according to the Homesite mapping.
        /// </summary>
        /// <param name="home">Home xml object.</param>
        /// <param name="opportunityId">Contract Id.</param>
        /// <param name="productId">Division Product Id.</param>
        protected virtual void SetHomeAttributes(out EnvisionXsdGenerated.Home home, object opportunityId, object productId)
        {
            Recordset product = this.m_envisionIntegration.PivotalDataAccess.GetRecordset(productId, ProductData.TableName);

            if (product.RecordCount <= 0)
            {
                throw new PivotalApplicationException((string)this.m_envisionIntegration.LangDictionary.GetText("ExceptionUnexpectedNofRecords"));
            }

            Recordset contract = this.m_envisionIntegration.PivotalDataAccess.GetRecordset(opportunityId, OpportunityData.TableName,
                OpportunityData.ElevationPremiumField,
                OpportunityData.FinancedOptionsField,
                OpportunityData.QuoteTotalField,
                "MI_Originating_Inv_Quote");
            if (contract.RecordCount <= 0)
            {
                throw new PivotalApplicationException((string)this.m_envisionIntegration.LangDictionary.GetText("ExceptionUnexpectedNofRecords"));
            }

            product.MoveFirst();
            home = new EnvisionXsdGenerated.Home();

            //2007-12-30 AB updated code to send either originating opportunity ID or current opportunity. 
            //Product_Id is not needed to make home number unique. START
            //Set the home attributes.
            //home.HomeNumber = GenerateBuyerNumber(opportunityId, productId);
            if (contract.Fields["MI_Originating_Inv_Quote"].Value == DBNull.Value)
            {
                //home.HomeNumber = CompactPivotalId(opportunityId);
                home.HomeNumber = GenerateHomeNumber(opportunityId, productId);
            }
            else
            {
                //home.HomeNumber = CompactPivotalId(contract.Fields["MI_Originating_Inv_Quote"].Value);
                home.HomeNumber = GenerateHomeNumber(opportunityId, productId);
            }
            
            string strJobNumber = (string)this.m_envisionIntegration.PivotalSystem.Tables[ProductData.TableName].Fields[ProductData.JobNumberField].FindValue(
            this.m_envisionIntegration.PivotalSystem.Tables[ProductData.TableName].Fields[ProductData.ProductIdField],
            productId);

            home.LotNumber = strJobNumber;
            
            //home.LotNumber = TypeConvert.ToString(product.Fields[ProductData.LotNumberField].Value);
            
            //END
            
            StringBuilder lotAddress = new StringBuilder();
            if (product.Fields[ProductData.Address1Field].Value != DBNull.Value)
            {
                lotAddress.Append((string)product.Fields[ProductData.Address1Field].Value);
                if (product.Fields[ProductData.Address2Field].Value != DBNull.Value)
                {
                    lotAddress.AppendLine();
                    lotAddress.Append((string)product.Fields[ProductData.Address2Field].Value);
                    if (product.Fields[ProductData.Address3Field].Value != DBNull.Value)
                    {
                        lotAddress.AppendLine();
                        lotAddress.Append((string)product.Fields[ProductData.Address3Field].Value);
                    }
                }
            }

            home.LotAddress = lotAddress.ToString();
            home.LotCity = TypeConvert.ToString(product.Fields[ProductData.CityField].Value);
            home.LotState = TypeConvert.ToString(product.Fields[ProductData.StateField].Value);
            home.LotZip = TypeConvert.ToString(product.Fields[ProductData.ZipField].Value);

            home.BasePrice = TypeConvert.ToDecimal(contract.Fields[OpportunityData.QuoteTotalField].Value)
                - TypeConvert.ToDecimal(contract.Fields[OpportunityData.FinancedOptionsField].Value)
                - TypeConvert.ToDecimal(contract.Fields[OpportunityData.ElevationPremiumField].Value); //might change.
            home.BasePriceSpecified = true;

            //2007-12-30 AB update code to allow spec flag to be set for specs START
            //home.IsSpec = false;
            if (product.Fields[ProductData.TypeField].Value.ToString() == "Inventory")
            {
                home.IsSpec = true;
            }
            else
            {
                home.IsSpec = false;
            }
            //AB END

            home.CalculateConstructionStage = false;
            if (product.Fields[ProductData.ConstructionStageIdField].Value != DBNull.Value)
            {
                //2008-01-09 AB Need to pass the stage code and not the Pivotal ID
                //home.CurrentConstructionStage = CompactPivotalId(product.Fields[ProductData.ConstructionStageIdField].Value);
                string conStageCode = this.m_envisionIntegration.PivotalSystem.Tables["Construction_Stage"].Fields["External_Source_Id"].FindValue(
                        this.m_envisionIntegration.PivotalSystem.Tables["Construction_Stage"].Fields["Construction_Stage_Id"],
                        product.Fields[ProductData.ConstructionStageIdField].Value).ToString();
                home.CurrentConstructionStage = conStageCode;

            }
            else
            {
                home.CurrentConstructionStage = string.Empty;
            }
        }

        /// <summary>
        /// Sets the selected option location level and number depending on the product creation level.
        /// </summary>
        /// <param name="selectedOption">The option on wich to set the location level and number.</param>
        /// <param name="divisionProduct">Division product of the selected option.</param>
        /// <param name="nhtNumber">NHTNumber for setting corporate location number.</param>
        protected virtual void SetSelectedOptionLocationInfo(ref EnvisionXsdGenerated.SelectedOptionType selectedOption, Recordset divisionProduct,
            string nhtNumber)
        {
            divisionProduct.MoveFirst();
            if (divisionProduct.Fields[DivisionProductData.DivisionIdField].Value != DBNull.Value)
            {
                //2007-12-18 AB The location number for divisions will be the area ID from JDE
                //selectedOption.LocationNumber = CompactPivotalId(divisionProduct.Fields[DivisionProductData.DivisionIdField].Value);
                MI_Envision_Utility util = new MI_Envision_Utility();
                string[] divisionInfo = util.GetDivisionDetail(divisionProduct.Fields[DivisionProductData.DivisionIdField].Value, this.m_envisionIntegration.PivotalSystem);
                selectedOption.LocationNumber = divisionInfo[1];

                selectedOption.LocationLevel = EnvisionIntegration.LocationLevel.CodeDivision.ToUpper(CultureInfo.CurrentCulture);
            }
            else if (divisionProduct.Fields[DivisionProductData.RegionIdField].Value != DBNull.Value)
            {
                selectedOption.LocationNumber = CompactPivotalId(divisionProduct.Fields[NBHDPProductData.RegionIdField].Value);
                selectedOption.LocationLevel = EnvisionIntegration.LocationLevel.CodeRegion.ToUpper(CultureInfo.CurrentCulture);
            }
            else
            {
                selectedOption.LocationNumber = nhtNumber;
                selectedOption.LocationLevel = EnvisionIntegration.LocationLevel.CodeCorporation.ToUpper(CultureInfo.CurrentCulture);
            }
        }

        #endregion
    }
}
