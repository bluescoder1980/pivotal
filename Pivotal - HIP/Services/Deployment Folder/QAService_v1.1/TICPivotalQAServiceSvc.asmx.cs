﻿

/****************************************************************************
 CodeBehind File for Web Services generated.
 Generated by Pivotal Web Services Generator r5.9.
 Do not modify  the contents of this file with the code editor.
 Copyright (c) 2006, Pivotal Corporation
 ***************************************************************************/
 
using System;
using System.Web.Services;
using System.Collections;
using System.ComponentModel;
using System.Web.Services.Protocols;
using System.Globalization;
using System.Resources;
using Pivotal.Interop.RDALib;
using System.Collections.Generic;
using TICPivotalQADataObjects;


[System.Web.Services.WebService(Namespace="http://tempuri.org/TICPivotalQAService")] 
public class TICPivotalQAServiceSvc : System.Web.Services.WebService
{
    // Access to resource file object.Use this object to access resource strings.
    private GenericData genData;
    

    private const string ACTIVE_FORM_NAME = "HB Quick Contact";
    private const string TABLE_NAME = "Contact";

    private class HB_Quick_ContactActiveForm 
    { 


        public string TIC_Numeric_Contact_Id; 
        public string M1_Contact_Id; 
        public string Type; 
        public string First_Name; 
        public string Last_Name; 
        public string Middle_Initial; 
        public string Preferred_Contact; 
        public string Title; 
        public string Suffix; 
        public string SSN; 
        public string Gender; 
        public string Marital_Status; 
        public string TIC_VIP; 
        public string TIC_VIP_Date; 
        public string Company_Id; 
        public string Has_Same_Address_Id; 
        public string Address_1; 
        public string Address_2; 
        public string Zip; 
        public string Address_3; 
        public string City; 
        public string State_; 
        public string Area_Code; 
        public string County_Id; 
        public string Country; 
        public string Phone; 
        public string Cell; 
        public string Fax; 
        public string Email; 
        public string TIC_Invalid_Email; 
        public string M1_Unsubscribe; 
        public string Walk_In_Date; 
        public string First_Contact_Date; 
        public string Next_Follow_Up_Date; 
        public string Close_Date; 
        public string Company_Name; 
        public string TIC_Work_Zip; 
        public string Work_Phone; 
        public string Extension; 
        public string Lead_Source_Type; 
        public string Lead_Source_Id; 
        public string Lead_Date; 
        public string Realtor_Company_Id; 
        public string Realtor_Id; 
        public string Referred_By_Contact_Id; 
        public string Account_Manager_Id; 
        public string Comments; 
        public string Household_Size; 
        public string Education; 
        public string Single_or_Dual_Income; 
        public string Combined_Income_Range; 
        public string Age_Range_Of_Buyers; 
        public string Number_Of_Children; 
        public string Age_Range_Of_Children; 
        public string Ethnic_Background; 
        public string TIC_Household_Config; 
        public string Time_Searching; 
        public string Resale; 
        public string Other_Neighborhoods; 
        public string Other_Builders; 
        public string TIC_First_Home; 
        public string Home_Type; 
        public string Minimum_Bedrooms; 
        public string Minimum_Bathrooms; 
        public string Minimum_Garage; 
        public string Number_Living_Areas; 
        public string Preferred_Area; 
        public string Desired_Move_In_Date; 
        public string TIC_Move_Timing; 
        public string Desired_Monthly_Payment; 
        public string TIC_Preferred_Price_Range_From; 
        public string TIC_Preferred_Price_Range_To; 
        public string TIC_Square_Footage_From; 
        public string TIC_Square_Footage_To; 
        public string Ownership; 
        public string For_Sale; 
        public string Transferring_To_Area; 
        public string Current_Monthly_Payment; 
        public string Current_Square_Footage; 
        public string Reasons_For_Moving; 
        public string Homes_Owned; 
        public string Commute; 
        public string TIC_Important_Factor_1; 
        public string TIC_If_Other_Factor_1; 
        public string TIC_Important_Factor_2; 
        public string TIC_If_Other_Factor_2; 
        public string TIC_Important_Factor_3; 
        public string TIC_If_Other_Factor_3; 
        public string Cell_NDNC; 
        public string Cell_CDNC; 
        public string Phone_NDNC; 
        public string Phone_CDNC; 
        public string Work_Phone_NDNC; 
        public string Work_Phone_CDNC; 
        public string Fax_NDNC; 
        public string Fax_CDNC; 
        public string Email_CDNC; 
        public string DNC_Status;
    }

    #region Enumerations
    public enum ActionForInspection
    {
        Save,
        CreateNew,
        Delete,
        Approve
    }
    #endregion      
    
    #region  Web Services Designer Generated Code

    public TICPivotalQAServiceSvc () 
	{
		genData = new GenericData();
		//This call is required by the Web Services Designer.
		InitializeComponent();

		//Add your own initialization code after the InitializeComponent() call
	}

	//Required by the Web Services Designer
	private IContainer components;

	//NOTE: The following procedure is required by the Web Services Designer
	//It can be modified using the Web Services Designer.  
	//Do not modify it using the code editor.
	[System.Diagnostics.DebuggerStepThrough()]
	private void InitializeComponent()
	{
		components = new System.ComponentModel.Container();
	}

	protected override void Dispose(bool disposing)
	{
		//CODEGEN: This procedure is required by the Web Services Designer
		//Do not modify it using the code editor.
		if (disposing)
		{
			if (components != null)
				components.Dispose();
		}
		base.Dispose(disposing);
	}

	#endregion
    
    #region Pivotal-Generated

    // WEBMETHODS
	//[WebMethod(Description="Adds a new record to Pivotal.Returns record id.Ensure that the mandatory fields are passed.")]
	private string Insert(string System,HB_Quick_ContactActiveForm HB_Quick_ContactData,string[] CommandParameters)
	{
       
		ExceptionHandler exHandler = null;
		string errMsg = string.Empty;
		if(System.Equals("") )
		{
			exHandler = new ExceptionHandler();
			errMsg = genData.GetString("MSG_EMPTY_SYSTEM_NAME");
			throw exHandler.RaiseException("",errMsg,"",Enumerations.FaultCode.Client);
		}
		if(HB_Quick_ContactData == null)
		{
			exHandler = new ExceptionHandler();
			errMsg = "HB_Quick_ContactData"  + genData.GetString("MSG_NULL");
			throw exHandler.RaiseException("",errMsg,"",Enumerations.FaultCode.Client);
		}
		// Default return
		string tempInsert = "";
		PBSComms pbsComms = new PBSComms();

		try
		{
			// Construct PBS XML
			pbsComms.AddRequestHeader(System);
			pbsComms.AddRequestCommandStart(PBSComms.CommandType.Insert, ref CommandParameters, ACTIVE_FORM_NAME);
			CreateActiveFormFieldXML(ref pbsComms,ref HB_Quick_ContactData);
			pbsComms.AddRequestCommandEnd(PBSComms.CommandType.Insert);

			// Execute PBS command and branch for success
			if (pbsComms.DoPBSRequest())
			{
				// Success - return record id
				tempInsert = pbsComms.GetResponseNewRecordId();
			}
			else
				RaisePBSError(pbsComms); // Request failed - Raise the PBS error 

		}
		catch (SoapException ex)
		{
			throw ex;
		}
		return tempInsert;
	}
		
	
	//[WebMethod(Description="Updates an existing record in Pivotal.Ensure that the mandatory fields are passed.")]
    private void Update(string System, string RecordId, HB_Quick_ContactActiveForm HB_Quick_ContactData, string[] CommandParameters)
	{
		ExceptionHandler exHandler = null;
		string errMsg = string.Empty;
		if(System.Equals("") )
		{
			exHandler = new ExceptionHandler();
			errMsg = genData.GetString("MSG_EMPTY_SYSTEM_NAME");
			throw exHandler.RaiseException("",errMsg,"",Enumerations.FaultCode.Client);
		}
		if(RecordId.Equals("") )
		{
			exHandler = new ExceptionHandler();
			errMsg = genData.GetString("MSG_EMPTY_RECORDID");
			throw exHandler.RaiseException("",errMsg,"",Enumerations.FaultCode.Client);
		}
		if(HB_Quick_ContactData == null)
		{
			exHandler = new ExceptionHandler();
			errMsg = "HB_Quick_ContactData"  + genData.GetString("MSG_NULL");
			throw exHandler.RaiseException("",errMsg,"",Enumerations.FaultCode.Client);
		}
		PBSComms pbsComms = new PBSComms();

		try
		{
			// Construct PBS XML
			pbsComms.AddRequestHeader(System);
			pbsComms.AddRequestCommandStart(PBSComms.CommandType.Update, ref CommandParameters, ACTIVE_FORM_NAME);
			pbsComms.AddRequestRecordSource(RecordId);
			CreateActiveFormFieldXML(ref pbsComms, ref HB_Quick_ContactData);
			pbsComms.AddRequestCommandEnd(PBSComms.CommandType.Update);

			// Execute PBS command and branch for success
			if (! (pbsComms.DoPBSRequest()))
			{
				// Request failed - Raise the PBS error 
				RaisePBSError(pbsComms);
			}

		}
		catch (SoapException ex)
		{
			throw ex; 
		}
	}
	
	
		
	//[WebMethod(Description="Returns data for the business object based on the record id.")]
    private HB_Quick_ContactActiveForm LoadById(string System, string RecordId)
		{
	HB_Quick_ContactActiveForm tempGetRecordById = null;

		ExceptionHandler exHandler = null;
		string errMsg = string.Empty;
		if(System.Equals("") )
		{
			exHandler = new ExceptionHandler();
			errMsg = genData.GetString("MSG_EMPTY_SYSTEM_NAME");
			throw exHandler.RaiseException("",errMsg,"",Enumerations.FaultCode.Client);
		}
		if(RecordId.Equals("") )
		{
			exHandler = new ExceptionHandler();
			errMsg = genData.GetString("MSG_EMPTY_RECORDID");
			throw exHandler.RaiseException("",errMsg,"",Enumerations.FaultCode.Client);
		}
		PBSComms pbsComms = new PBSComms();
		
		HB_Quick_ContactActiveForm Data = new HB_Quick_ContactActiveForm();
		string [] CommandParameters = {};
		object obj = Data;
		try
		{
			// Construct PBS XML
			pbsComms.AddRequestHeader(System);
			pbsComms.AddRequestCommandStart(PBSComms.CommandType.getFormData, ref CommandParameters, ACTIVE_FORM_NAME);
			pbsComms.AddRequestRecordId(RecordId);
			pbsComms.AddRequestCommandEnd(PBSComms.CommandType.getFormData);

			// Execute PBS command and branch for success
			if (pbsComms.DoPBSRequest())
			{
				// Success - fill in the data class result object
				pbsComms.FillDataClass(ref obj);
				Data = ( HB_Quick_ContactActiveForm )obj;
				// Set result
				tempGetRecordById = Data;
			}
			else
				RaisePBSError(pbsComms); // Raise the PBS error 
		}
		catch (SoapException ex)
		{
			throw ex; 
		}
		
		return tempGetRecordById;
	}
	
	
	
	
	//[WebMethod(Description="Returns rows of data for a business object based on the following condition:[Field name] [condition] [field value].The following conditional operators are supported:=,>,>=,<,<=,!=.For eg. Give me records which satisfy the following condition for Company:First_Name = Bill.Last_Name=Gates.")]
    private HB_Quick_ContactActiveForm[] LoadByKeyValuePairs(string System, string[] keys, PBSComms.ConditionType[] Operator, string[] values)
	{
		ExceptionHandler exHandler = null;
		string errMsg = string.Empty;
		if(System.Equals("") )
		{
			exHandler = new ExceptionHandler();
			errMsg = genData.GetString("MSG_EMPTY_SYSTEM_NAME");
			throw exHandler.RaiseException("",errMsg,"",Enumerations.FaultCode.Client);
		}
		if(keys == null)
		{
			exHandler = new ExceptionHandler();
			errMsg = genData.GetString("MSG_NULL_KEYLIST");
			throw exHandler.RaiseException("",errMsg,"",Enumerations.FaultCode.Client);
		}
		if(Operator == null)
		{
			exHandler = new ExceptionHandler();
			errMsg = genData.GetString("MSG_NULL_OPERATORSLIST");
			throw exHandler.RaiseException("",errMsg,"",Enumerations.FaultCode.Client);
		}
		if(values == null)
		{
			exHandler = new ExceptionHandler();
			errMsg = genData.GetString("MSG_NULL_VALUESLIST");
			throw exHandler.RaiseException("",errMsg,"",Enumerations.FaultCode.Client);
		}

		PBSComms pbsComms = new PBSComms();
		int recordCount = 0;
		string [] CommandParameters = {};
		ArrayList resultRows=new ArrayList();
		 HB_Quick_ContactActiveForm[] results=null;
		try
		{
			// Construct PBS XML
			pbsComms.AddRequestHeader(System);
			pbsComms.AddRequestCommandStart(PBSComms.CommandType.getSearchData, ref CommandParameters, TABLE_NAME);
			pbsComms.AddCondition( keys,  Operator, values);
			pbsComms.AddRequestCommandEnd(PBSComms.CommandType.getSearchData);

			// Execute PBS command and branch for success
			if (pbsComms.DoPBSRequest())
			{
				recordCount = pbsComms.GetSearchRecordCount();
				if (recordCount <= 0)
				{
					// No record found - return blank data
					return results;
				}
				else
				{
					// Multiple Records found.
					ArrayList recordIdList;
					recordIdList = pbsComms.GetSearchRecordIds();
					for(int i=0;i<(recordIdList.Count);i++)
					{
						//Get Record Id for all records returned from PBS
						string recordId=recordIdList[i].ToString();
						//Get tha data for each record
						 HB_Quick_ContactActiveForm resultRecord = LoadById(System,recordId);
						if(resultRecord!=null)
						{
							resultRows.Add(resultRecord);
						}

					}
					results = ( HB_Quick_ContactActiveForm[])resultRows.ToArray(typeof( HB_Quick_ContactActiveForm));
				}
			}
			else
				RaisePBSError(pbsComms);

		}
		catch (SoapException ex)
		{
			throw ex; 
		}
		
		return results;
	}
	
	
	
	
	private void CreateActiveFormFieldXML(ref PBSComms PBSComms, ref HB_Quick_ContactActiveForm Data)
	{
		try
		{
	
			
            //PBSComms.AddSegmentUpdateStart("Contact Information");
            //PBSComms.AddFieldUpdate("TIC_Numeric_Contact_Id", Data.TIC_Numeric_Contact_Id);
            //PBSComms.AddFieldUpdate("M1_Contact_Id", Data.M1_Contact_Id);
            //PBSComms.AddFieldUpdate("Type", Data.Type);
            //PBSComms.AddFieldUpdate("First_Name", Data.First_Name);
            //PBSComms.AddFieldUpdate("Last_Name", Data.Last_Name);
            //PBSComms.AddFieldUpdate("Middle_Initial", Data.Middle_Initial);
            //PBSComms.AddFieldUpdate("Preferred_Contact", Data.Preferred_Contact);
            //PBSComms.AddFieldUpdate("Title", Data.Title);
            //PBSComms.AddFieldUpdate("Suffix", Data.Suffix);
            //PBSComms.AddFieldUpdate("SSN", Data.SSN);
            //PBSComms.AddFieldUpdate("Gender", Data.Gender);
            //PBSComms.AddFieldUpdate("Marital_Status", Data.Marital_Status);
            //PBSComms.AddFieldUpdate("TIC_VIP", Data.TIC_VIP);
            //PBSComms.AddFieldUpdate("TIC_VIP_Date", Data.TIC_VIP_Date);
            //PBSComms.AddFieldUpdate("Company_Id", Data.Company_Id);
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Address Information");
            //PBSComms.AddFieldUpdate("Has_Same_Address_Id", Data.Has_Same_Address_Id);
            //PBSComms.AddFieldUpdate("Address_1", Data.Address_1);
            //PBSComms.AddFieldUpdate("Address_2", Data.Address_2);
            //PBSComms.AddFieldUpdate("Zip", Data.Zip);
            //PBSComms.AddFieldUpdate("Address_3", Data.Address_3);
            //PBSComms.AddFieldUpdate("City", Data.City);
            //PBSComms.AddFieldUpdate("State_", Data.State_);
            //PBSComms.AddFieldUpdate("Area_Code", Data.Area_Code);
            //PBSComms.AddFieldUpdate("County_Id", Data.County_Id);
            //PBSComms.AddFieldUpdate("Country", Data.Country);
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Communication");
            //PBSComms.AddFieldUpdate("Phone", Data.Phone);
            //PBSComms.AddFieldUpdate("Cell", Data.Cell);
            //PBSComms.AddFieldUpdate("Fax", Data.Fax);
            //PBSComms.AddFieldUpdate("Email", Data.Email);
            //PBSComms.AddFieldUpdate("TIC_Invalid_Email", Data.TIC_Invalid_Email);
            //PBSComms.AddFieldUpdate("M1_Unsubscribe", Data.M1_Unsubscribe);
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Dates");
            //PBSComms.AddFieldUpdate("Walk_In_Date", Data.Walk_In_Date);
            //PBSComms.AddFieldUpdate("First_Contact_Date", Data.First_Contact_Date);
            //PBSComms.AddFieldUpdate("Next_Follow_Up_Date", Data.Next_Follow_Up_Date);
            //PBSComms.AddFieldUpdate("Close_Date", Data.Close_Date);
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Work Information");
            //PBSComms.AddFieldUpdate("Company_Name", Data.Company_Name);
            //PBSComms.AddFieldUpdate("TIC_Work_Zip", Data.TIC_Work_Zip);
            //PBSComms.AddFieldUpdate("Work_Phone", Data.Work_Phone);
            //PBSComms.AddFieldUpdate("Extension", Data.Extension);
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Other Information");
            //PBSComms.AddFieldUpdate("Lead_Source_Type", Data.Lead_Source_Type);
            //PBSComms.AddFieldUpdate("Lead_Source_Id", Data.Lead_Source_Id);
            //PBSComms.AddFieldUpdate("Lead_Date", Data.Lead_Date);
            //PBSComms.AddFieldUpdate("Realtor_Company_Id", Data.Realtor_Company_Id);
            //PBSComms.AddFieldUpdate("Realtor_Id", Data.Realtor_Id);
            //PBSComms.AddFieldUpdate("Referred_By_Contact_Id", Data.Referred_By_Contact_Id);
            //PBSComms.AddFieldUpdate("Account_Manager_Id", Data.Account_Manager_Id);
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Comments");
            //PBSComms.AddFieldUpdate("Comments", Data.Comments);
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Co-Buyers");
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Household / Rating Information");
            //PBSComms.AddFieldUpdate("Household_Size", Data.Household_Size);
            //PBSComms.AddFieldUpdate("Education", Data.Education);
            //PBSComms.AddFieldUpdate("Single_or_Dual_Income", Data.Single_or_Dual_Income);
            //PBSComms.AddFieldUpdate("Combined_Income_Range", Data.Combined_Income_Range);
            //PBSComms.AddFieldUpdate("Age_Range_Of_Buyers", Data.Age_Range_Of_Buyers);
            //PBSComms.AddFieldUpdate("Number_Of_Children", Data.Number_Of_Children);
            //PBSComms.AddFieldUpdate("Age_Range_Of_Children", Data.Age_Range_Of_Children);
            //PBSComms.AddFieldUpdate("Ethnic_Background", Data.Ethnic_Background);
            //PBSComms.AddFieldUpdate("TIC_Household_Config", Data.TIC_Household_Config);
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Visit Information");
            //PBSComms.AddFieldUpdate("Time_Searching", Data.Time_Searching);
            //PBSComms.AddFieldUpdate("Resale", Data.Resale);
            //PBSComms.AddFieldUpdate("Other_Neighborhoods", Data.Other_Neighborhoods);
            //PBSComms.AddFieldUpdate("Other_Builders", Data.Other_Builders);
            //PBSComms.AddFieldUpdate("TIC_First_Home", Data.TIC_First_Home);
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Desired Home Information");
            //PBSComms.AddFieldUpdate("Home_Type", Data.Home_Type);
            //PBSComms.AddFieldUpdate("Minimum_Bedrooms", Data.Minimum_Bedrooms);
            //PBSComms.AddFieldUpdate("Minimum_Bathrooms", Data.Minimum_Bathrooms);
            //PBSComms.AddFieldUpdate("Minimum_Garage", Data.Minimum_Garage);
            //PBSComms.AddFieldUpdate("Number_Living_Areas", Data.Number_Living_Areas);
            //PBSComms.AddFieldUpdate("Preferred_Area", Data.Preferred_Area);
            //PBSComms.AddFieldUpdate("Desired_Move_In_Date", Data.Desired_Move_In_Date);
            //PBSComms.AddFieldUpdate("TIC_Move_Timing", Data.TIC_Move_Timing);
            //PBSComms.AddFieldUpdate("Desired_Monthly_Payment", Data.Desired_Monthly_Payment);
            //PBSComms.AddFieldUpdate("TIC_Preferred_Price_Range_From", Data.TIC_Preferred_Price_Range_From);
            //PBSComms.AddFieldUpdate("TIC_Preferred_Price_Range_To", Data.TIC_Preferred_Price_Range_To);
            //PBSComms.AddFieldUpdate("TIC_Square_Footage_From", Data.TIC_Square_Footage_From);
            //PBSComms.AddFieldUpdate("TIC_Square_Footage_To", Data.TIC_Square_Footage_To);
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Current Home Information");
            //PBSComms.AddFieldUpdate("Ownership", Data.Ownership);
            //PBSComms.AddFieldUpdate("For_Sale", Data.For_Sale);
            //PBSComms.AddFieldUpdate("Transferring_To_Area", Data.Transferring_To_Area);
            //PBSComms.AddFieldUpdate("Current_Monthly_Payment", Data.Current_Monthly_Payment);
            //PBSComms.AddFieldUpdate("Current_Square_Footage", Data.Current_Square_Footage);
            //PBSComms.AddFieldUpdate("Reasons_For_Moving", Data.Reasons_For_Moving);
            //PBSComms.AddFieldUpdate("Homes_Owned", Data.Homes_Owned);
            //PBSComms.AddFieldUpdate("Commute", Data.Commute);
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Important Factors");
            //PBSComms.AddFieldUpdate("TIC_Important_Factor_1", Data.TIC_Important_Factor_1);
            //PBSComms.AddFieldUpdate("TIC_If_Other_Factor_1", Data.TIC_If_Other_Factor_1);
            //PBSComms.AddFieldUpdate("TIC_Important_Factor_2", Data.TIC_Important_Factor_2);
            //PBSComms.AddFieldUpdate("TIC_If_Other_Factor_2", Data.TIC_If_Other_Factor_2);
            //PBSComms.AddFieldUpdate("TIC_Important_Factor_3", Data.TIC_Important_Factor_3);
            //PBSComms.AddFieldUpdate("TIC_If_Other_Factor_3", Data.TIC_If_Other_Factor_3);
            //PBSComms.AddSegmentUpdateEnd();
			
            //PBSComms.AddSegmentUpdateStart("Contact DNC");
            //PBSComms.AddFieldUpdate("Cell", Data.Cell);
            //PBSComms.AddFieldUpdate("Cell_NDNC", Data.Cell_NDNC);
            //PBSComms.AddFieldUpdate("Cell_CDNC", Data.Cell_CDNC);
            //PBSComms.AddFieldUpdate("Phone", Data.Phone);
            //PBSComms.AddFieldUpdate("Phone_NDNC", Data.Phone_NDNC);
            //PBSComms.AddFieldUpdate("Phone_CDNC", Data.Phone_CDNC);
            //PBSComms.AddFieldUpdate("Work_Phone", Data.Work_Phone);
            //PBSComms.AddFieldUpdate("Work_Phone_NDNC", Data.Work_Phone_NDNC);
            //PBSComms.AddFieldUpdate("Work_Phone_CDNC", Data.Work_Phone_CDNC);
            //PBSComms.AddFieldUpdate("Fax", Data.Fax);
            //PBSComms.AddFieldUpdate("Fax_NDNC", Data.Fax_NDNC);
            //PBSComms.AddFieldUpdate("Fax_CDNC", Data.Fax_CDNC);
            //PBSComms.AddFieldUpdate("Email", Data.Email);
            //PBSComms.AddFieldUpdate("Email_CDNC", Data.Email_CDNC);
            //PBSComms.AddFieldUpdate("DNC_Status", Data.DNC_Status);
            //PBSComms.AddSegmentUpdateEnd();
			
	
		}
	catch (Exception ex)
	{
		ExceptionHandler exHandler = new ExceptionHandler();
		throw exHandler.RaiseException("",ex.Message,"",Enumerations.FaultCode.Client);
	}
}



	private void RaisePBSError(PBSComms PBSComms)
	{
		try
		{
			int errorNumber = PBSComms.GetResponseErrorNumber();
			string errorText = PBSComms.GetResponseErrorText();
			string errorDetails = PBSComms.GetResponseErrorDetails();

			ExceptionHandler exHandler = new ExceptionHandler();
			throw exHandler.RaiseException("",errorText,"",Enumerations.FaultCode.Server);
			
		}
		catch (SoapException ex)
		{
			throw ex; 
		}
    }

    #endregion
    
    #region Custom Web Methods

    /// <summary>
    /// This method will authenticate the user logging in from the QA Website against the Pivotal
    /// Contact Web Details record.  Password should be digested prior to authenticating
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="passwordHash"></param>
    /// <returns></returns>
    [WebMethod(Description="Called from QA Website to authenticate user against the Contact's Web Detail record.")]
    public InspectorObj AuthenticateUserLogin(string userName, string passwordHash)
    { 
        //TO-DO: Need to write code to authenticate the user name and password and return the necessary
        //data object to the web app

        //Test against hard-coded user login info to authenticate for prototype
        if (userName != "amaldonado@irvinecompany.com")
        {
            string strUserNotFound = "Invalid username : " + userName;
            ExceptionHandler exHandler = new ExceptionHandler();
            throw exHandler.RaiseException("", strUserNotFound, "", Enumerations.FaultCode.Server);
        }

        if (passwordHash != "b081dbe85e1ec3ffc3d4e7d0227400cd")
        {
            string strInvalidPassword = "Invalid password.";
            ExceptionHandler exHandler = new ExceptionHandler();
            throw exHandler.RaiseException("", strInvalidPassword, "", Enumerations.FaultCode.Server);
        }

        //Mock-Up Hello World Stub
        InspectorObj inspector = new InspectorObj();
        inspector.contactHexId = "0000000000000158";
        inspector.Company_Name = "Broadcom";
        inspector.email = "amaldonado@irvinecompany.com";
        inspector.First_Name = "Abram";
        inspector.Last_Name = "Maldonado";
        inspector.Title = "Integration Architect";
        inspector.Type = "Inspector";

        InspectorObj[] impersonUsers = null;
        List<InspectorObj> impUsers = new List<InspectorObj>();
        
        //Add some fake users
        InspectorObj u1 = new InspectorObj();
        u1.contactHexId = "0000000000000210";
        u1.Company_Name= "Broadcom";
        u1.email = "testadmin1@irvinecompany.com";
        u1.First_Name = "Test";
        u1.Last_Name = "Admin 1";
        u1.Title = "";
        u1.Type = "Administrator";
        impUsers.Add(u1);
        
        InspectorObj u2 = new InspectorObj();
        u2.contactHexId = "0000000000000229";
        u2.Company_Name = "Broadcom";
        u2.email = "testadmin2@irvinecompany.com";
        u2.First_Name = "Test";
        u2.Last_Name = "Admin 2";
        u2.Title = "";
        u2.Type = "Administrator";
        impUsers.Add(u2);

        impersonUsers = impUsers.ToArray();
        inspector.impersUsers = impersonUsers;

        return inspector;        
    
    }

    /// <summary>
    /// This method will need to be called from the QA Website in order to digest the password
    /// so that it can be matched against the Password stored in the Contact Web Details record.
    /// </summary>
    /// <param name="plainTextPassword"></param>
    /// <returns></returns>
    [WebMethod(Description="Called by QA Website to get encrypted password for user authentication.")]
    public string GetPivotalMD5MessageDigest(string plainTextPassword)
    {
        TICPivotalQAUtility.MD5Helper md5 = new TICPivotalQAUtility.MD5Helper();
        return md5.GetMessageDigest(plainTextPassword);
    }

    /// <summary>
    /// This method will be called by the QA Website to get all inspections associated with the
    /// users Company.
    /// </summary>
    /// <param name="userLogin"></param>
    /// <returns></returns>
    [WebMethod(Description="Called by QA Website to get a list of scheduled inspections by user's company.")]
    public ScheduledInspectionWrapper GetScheduledInspectionsForUser(string userLogin)
    {
        //For now fake some data to go to the web
        //Project Filter
        ProjectFilter proj1 = new ProjectFilter();
        proj1.projectId = "000000000000000B";
        proj1.projectName = "Capistrano";
        ProjectFilter proj2 = new ProjectFilter();
        proj2.projectId = "000000000000000C";
        proj2.projectName = "Los Altos";
        List<ProjectFilter> projList = new List<ProjectFilter>();
        projList.Add(proj1);
        projList.Add(proj2);
        ProjectFilter[] projFiltArr = projList.ToArray();

        //Phase Filter
        PhaseFilter phase1 = new PhaseFilter();
        phase1.projectId = "000000000000000B";
        phase1.PhaseName = "1";
        PhaseFilter phase2 = new PhaseFilter();
        phase2.projectId = "000000000000000B";
        phase2.PhaseName = "2";
        PhaseFilter phase3 = new PhaseFilter();
        phase3.projectId = "000000000000000C";
        phase3.PhaseName = "1";
        PhaseFilter phase4 = new PhaseFilter();
        phase4.projectId = "000000000000000C";
        phase4.PhaseName = "2";
        List<PhaseFilter> phaseList = new List<PhaseFilter>();
        phaseList.Add(phase1);
        phaseList.Add(phase2);
        phaseList.Add(phase3);
        phaseList.Add(phase4);
        PhaseFilter[] phaseFiltArr = phaseList.ToArray();

        InspectionTypeFilter type1 = new InspectionTypeFilter();
        type1.projectId = "000000000000000B";
        type1.InspectionType = "Roofing";
        InspectionTypeFilter type2 = new InspectionTypeFilter();
        type2.projectId = "000000000000000C";
        type2.InspectionType = "Roofing";
        List<InspectionTypeFilter> typeList = new List<InspectionTypeFilter>();
        typeList.Add(type1);
        typeList.Add(type2);
        InspectionTypeFilter[] typeFiltArr = typeList.ToArray();

        FilterWrapper filtWrap = new FilterWrapper();
        filtWrap.projFilter = projFiltArr;
        filtWrap.phaseFilter = phaseFiltArr;
        filtWrap.inspectionTypeFilter = typeFiltArr;


        //Scheduled Inspection
        ScheduledInspection sInsp1 = new ScheduledInspection();
        sInsp1.inspectionType = "Roofing";
        sInsp1.scheduledInspectionId = "0000000000000AD5";
        sInsp1.projectName = "Capistrano";
        sInsp1.baseLineDate = "01/14/2011";
        sInsp1.phaseName = "1";
        sInsp1.projectedDate = "01/20/2011";
        sInsp1.scheduledDate = "01/23/2011";

        //Scheduled Inspection
        ScheduledInspection sInsp2 = new ScheduledInspection();
        sInsp2.inspectionType = "Test - Electric";
        sInsp2.scheduledInspectionId = "0000000000000AD6";
        sInsp2.projectName = "Capistrano";
        sInsp2.baseLineDate = "01/14/2011";
        sInsp2.phaseName = "2";
        sInsp2.projectedDate = "01/20/2011";
        sInsp2.scheduledDate = "01/23/2011";

        List<ScheduledInspection> schList = new List<ScheduledInspection>();
        schList.Add(sInsp1);
        schList.Add(sInsp2);
        ScheduledInspection[] schArr = schList.ToArray();

        ScheduledInspectionWrapper wrap = new ScheduledInspectionWrapper();
        wrap.scheduledInspections = schArr;
        wrap.filterWrapper = filtWrap;
               
        return wrap;
    }

    /// <summary>
    /// This method will be used by the QA Website to get a list of inspections to be 
    /// retreived by the userlogin passed in.
    /// </summary>
    /// <param name="userLogin"></param>
    /// <returns></returns>
    [WebMethod(Description="Called by QA Website to get a list of actual inspections assigned to the userlogin.")]
    public InspectionWrapper GetInspectionsForUser(string userLogin)
    {
        return null;
    }

    /// <summary>
    /// This method will be used by the QA Website to start a new inspection for 
    /// the selected scheduled inspections the user selections.  This method will not
    /// create a new record in Pivotal, a seperate method will need to be called to commite
    /// to the Pivotal database.
    /// </summary>
    /// <param name="scheduledInspectionIds"></param>
    /// <returns></returns>
    [WebMethod(Description="Called by QA Website to start a new inspection.")]
    public InspectionTemplate CreateNewInspection(string[] scheduledInspectionIds)
    {
        return null;
    }


    /// <summary>
    /// This method will be used to interact with the Pivotal database for the
    /// save, create, delete or approve of the Inspection records.
    /// </summary>
    /// <param name="inspection"></param>
    /// <param name="action"></param>
    [WebMethod(Description="Called by QA Website to interact with Pivotal DB.")]
    public void ApplyInspection(Inspection inspection, ActionForInspection action)
    { }
    

    #endregion


    #region Test Web Methods
    [WebMethod]
    public InspectorObj TestDAL()
    {
        TICPivotalQAController.QAController control = new TICPivotalQAController.QAController();
        InspectorObj insp = new InspectorObj();
        insp.First_Name = control.TestDataAccessLayer("test");
        return insp;

    }
    
    #endregion

}
	
	