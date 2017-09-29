using System;
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
	public class System : IRAppScript, IRFormScript
	{
		/// <summary>
		/// This module provides all the business rules for the System object.
		/// </summary>
		/// This object is used to tickle records that need to be redistributed
		/// to mobile users and satellites.
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
        private IRSystem7 mrsysSystem;

        protected IRSystem7 RSysSystem
        {
            get { return mrsysSystem; }
            set { mrsysSystem = value; }
        }

        private ILangDict mrldtLangDict;

        protected ILangDict RldtLangDict
        {
            get { return mrldtLangDict; }
            set { mrldtLangDict = value; }
        }
		
		/// <summary>
		/// Sets the current IRSystem7 reference
		/// </summary>
		/// <param name="pSystem">the System to set</param>
		/// <returns></returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual void SetSystem(RSystem pSystem)
		{
			try
			{
				RSysSystem = (IRSystem7) pSystem;
				RldtLangDict = RSysSystem.GetLDGroup("System");
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// Executes a given method
		/// </summary>
		/// <param name="MethodName">the name of the method to execute</param>
		/// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
		/// ParameterList - Return parameter info back to the client script for further processing
		/// <returns>None</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual void Execute(string MethodName, ref object ParameterList)
		{
			try
			{
				TransitionPointParameter objParam = (TransitionPointParameter) RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
				objParam.ParameterList = ParameterList;
				// Dump out the user defined parameters
				object[] parameterArray = objParam.GetUserDefinedParameterArray();

				switch (MethodName)
				{
					case modSystem.strmBATCH_UPDATE_QUOTE_EXPIRY:
						BatchUpdateQuoteExpiry();
						parameterArray = new object[] {DBNull.Value};
						break;
					case modSystem.strmBATCH_UPDATE_PRICING:
						BatchUpdatePricing();
						parameterArray = new object[] {DBNull.Value};
						break;
					case modSystem.strmBATCH_UPDATE_LOT_STATUS:
						BatchUpdateLotStatus();
						break;
					case modSystem.strmBATCH_UPDATE_NBHD_STATUS:
						BatchUpdateNBHDStatus();
						break;
					case modSystem.strmBATCH_UPDATE_RELEASE_STATUS:
						BatchUpdateReleaseStatus();
						break;
					case modSystem.strmBATCH_UPDATE_QUOTE_STATUS:
						BatchUpdateQuoteStatus();
						parameterArray = new object[] {DBNull.Value};
						break;
					case modSystem.strmBATCH_UPDATE_DNC:
						BatchUpdateDNC();
						parameterArray = new object[] {DBNull.Value};
						break;
					case modSystem.strmBATCH_UPDATE_MARKET_PROJECT_STATUS:
						BatchUpdateMarketProjectStatus();
						parameterArray = new object[] {DBNull.Value};
						break;
					default:
						string message = MethodName + " may not be the correct method name for object System because the system has not detected it.";
						throw new PivotalApplicationException(message, modSystem.glngERR_METHOD_NOT_DEFINED);
				}

				// Add the returned values into transit point parameter list
				ParameterList = objParam.SetUserDefinedParameterArray(parameterArray);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This subroutine tickles a Rn_Appointment record and relative tables' records
		/// to the mobile users and satellite servers managed by the DSM.
		/// Assumptions:
		/// </summary>
		/// <param name="vntRecordId">ID for the appointment to tickle</param>
		/// <param name="blnImmediate">True when batch tickle record is calling this procedure</param>
		/// <returns>None</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		public virtual void TickleRnAppointment(object vntRecordId, bool blnImmediate)
		{
			try
			{
				if (!blnImmediate)
				{

					// The blnImmediate flag is not set, Read system record
					Recordset rstSystem = this.GetSystemInfo();

					// Are Rn_Appts set to automatically resolve?
					if (TypeConvert.ToInt32(rstSystem.Fields[modSystem.strf_RN_APPTS_CASCADE_TICKLING].Value) == 1)
					{
						// Do nothing
					}
					else if (TypeConvert.ToInt32(rstSystem.Fields[modSystem.strf_RN_APPTS_CASCADE_TICKLING].Value) == 2)
					{
						// Rn_Appointment set to batch resolve
						// Put a Tickle Record notification in the Tickle Record table
						this.SaveSysTickling(vntRecordId, 2);
						return;
					}
					else
					{
						// else return
						return;
					}
				}

				// Resolve the Rn_Appointment record
				RSysSystem.DoTickleRecord(RSysSystem.Tables[modSystem.strt_RN_APPOINTMENTS].TableId, vntRecordId);

				// Tickle Record Literature_listing
				this.TickleTableRecord(modSystem.strt_LITERATURE_LISTING, modSystem.strq_LITERATURE_LISTING, vntRecordId);

				// Tickle Record Meeting_Contact_Attendee
				this.TickleTableRecord(modSystem.strt_MEETING_CONTACT_ATTENDEE, modSystem.strq_MEETING_CONTACT_ATTENDEES, vntRecordId);

				// Tickle Record Meeting_Staff_Attendee
				this.TickleTableRecord(modSystem.strt_MEETING_STAFF_ATTENDEE, modSystem.strq_MEETING_STAFF_ATTENDEES, vntRecordId);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This function gets the system table information from the database.
		/// Assumptions:
		/// </summary>
		/// <returns>Recordset containing system information</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		public virtual Recordset GetSystemInfo()
		{
			try
			{
				DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
				return objLib.GetRecordset(modSystem.strt_SYSTEM,
										modSystem.strf_ADMINISTRATOR, modSystem.strf_COMPANY_CASCADE_TICKLING,
										modSystem.strf_CONTACT_ACTVY_CASCADE_TICKLING, modSystem.strf_CONTACT_CASCADE_TICKLING,
										modSystem.strf_DEFAULT_ARCHIVE_ER_DAYS, modSystem.strf_DEFAULT_CURRENCY,
										modSystem.strf_DEFAULT_FORECAST_ER_DAYS, modSystem.strf_DEFAULT_MILESTONE_TEMPLATE,
										modSystem.strf_DEFAULT_PRIORITY, modSystem.strf_DEFAULT_WEB_MP_ID,
										modSystem.strf_DEFAULT_WEB_SALES_TEAM_ID, modSystem.strf_DEFAULT_WEB_SUPPORT_TEAM,
										modSystem.strf_EMAIL, modSystem.strf_EURO,
										modSystem.strf_MOBILE_ADMIN_EMAIL, modSystem.strf_OPPORTUNITY_CASCADE_TICKLING,
										modSystem.strf_RN_APPTS_CASCADE_TICKLING, modSystem.strf_SYSTEM_BOOLEAN,
										modSystem.strf_SYSTEM_ID);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This subroutine tickles an Opportunity record and relative tables' records
		/// to the mobile users and satellite servers managed by the DSM.
		/// Assumptions:
		/// </summary>
		/// <param name="vntRecordId">ID of the opportunity to tickle</param>
		/// <param name="blnImmediate">True when batch tickle record is calling this procedure</param>
		/// <returns>None</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		public virtual void TickleOpportunity(object vntRecordId, bool blnImmediate)
		{
			try
			{
				if (!blnImmediate)
				{
					// The blnImmediate flag is not set
					// Read the system record
					Recordset rstSystem = this.GetSystemInfo();

					// Are Opportunity set to automatically resolve?
					if (TypeConvert.ToInt32(rstSystem.Fields[modSystem.strf_OPPORTUNITY_CASCADE_TICKLING].Value) == 1)
					{
						// Do nothing
					}
					else if (TypeConvert.ToInt32(rstSystem.Fields[modSystem.strf_OPPORTUNITY_CASCADE_TICKLING].Value) == 2)
					{
						// Opportunity set to batch resolve
						// Put a Tickle Record notification in the Tickle Record table
						this.SaveSysTickling(vntRecordId, 3);
						return ;
					}
					else
					{
						return ;
					}
				}

				// Resolve the Opportunity record
				RSysSystem.DoTickleRecord(RSysSystem.Tables[modSystem.strt_OPPORTUNITY].TableId, vntRecordId);

				// Tickle Record Opportunity__Influencer
				this.TickleTableRecord(modSystem.strt_OPPORTUNITY__INFLUENCER, modSystem.strq_OP_INFLUENCERS_WITH_OPPORTUNITY_ID,vntRecordId);

				// Tickle Record Opportunity__Product
				this.TickleTableRecord(modSystem.strt_OPPORTUNITY__PRODUCT, modSystem.strq_OP_PRODUCT_WITH_OPPORTUNITY_ID, vntRecordId);

				// Tickle Record Opportunity_Team_Member
				this.TickleTableRecord(modSystem.strt_OPPORTUNITY_TEAM_MEMBER, modSystem.strq_OPPORTUNITY_TEAM_MEMBER_OF_OPPORTUNITY,vntRecordId);

				// Tickle Record Influence
				this.TickleTableRecord(modSystem.strt_INFLUENCER_INFLUENCE, modSystem.strq_CONTACT_INFLUENCED_FOR_OPPORTUNITY,vntRecordId);
				this.TickleTableRecord(modSystem.strt_INFLUENCER_INFLUENCE, modSystem.strq_CONTACT_INFLUENCING_FOR_OPPORTUNITY,vntRecordId);

				// Tickle Record Rn_Appointment
				this.CascadeTickleTableRecord(modSystem.strt_RN_APPOINTMENTS, modSystem.strq_ACTIVITIES_WITH_OPPORTUNITY,vntRecordId, 3, blnImmediate);

				// Tickle Record Contact_Activities
				this.CascadeTickleTableRecord(modSystem.strt_CONTACT_ACTIVITIES, modSystem.strq_CONTACT_ACTIVITIES_WITH_OPPORTUNITY,vntRecordId, 4, blnImmediate);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This subroutine tickles a Contact record and relative tables' records
		/// to the mobile users and satellite servers managed by the DSM.
		/// Assumptions:
		/// </summary>
		/// <param name="vntRecordId">ID of the Contact to tickle</param>
		/// <param name="blnImmediate">True when batch tickle record is calling this procedure</param>
		/// <returns>None</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		public virtual void TickleContact(object vntRecordId, bool blnImmediate)
		{
			try
			{
				if (!blnImmediate)
				{
					// The blnImmediate flag is not set
					// Read the system record
					Recordset rstSystem = this.GetSystemInfo();

					// Are Contact set to automatically resolve?
					if (TypeConvert.ToInt32(rstSystem.Fields["Contact_Cascade_Tickling"].Value) == 1)
					{
						// Do nothing
					}
					else if (TypeConvert.ToInt32(rstSystem.Fields["Contact_Cascade_Tickling"].Value) == 2)
					{
						// Contact set to batch resolve
						// Put a Tickle Record notification in the Tickle Record table
						this.SaveSysTickling(vntRecordId, 1);

						return ;
					}
					else
					{
						return ;
					}
				}

				// Resolve the Contact record
				RSysSystem.DoTickleRecord(RSysSystem.Tables[modSystem.strt_CONTACT].TableId, vntRecordId);

				// Tickle Record Alt_Address
				this.TickleTableRecord(modSystem.strt_ALT_ADDRESS, modSystem.strq_ALTERNATE_ADDRESSES_OF_CONTACT, vntRecordId);

				// Tickle Record Alt_Phone
				this.TickleTableRecord(modSystem.strt_ALT_PHONE, modSystem.strq_ALTERNATE_PHONE_OF_CONTACT, vntRecordId);

				// Tickle Record Contact_Team_Member
				this.TickleTableRecord(modSystem.strt_CONTACT_TEAM_MEMBER, modSystem.strq_CONTACT_TEAM_MEMBER_OF_CONTACT,vntRecordId);

				// Tickle Record Opportunity__Influencer
				this.TickleTableRecord(modSystem.strt_OPPORTUNITY__INFLUENCER, modSystem.strq_OP_INFLUENCER_WITH_CONTACT,vntRecordId);

				// Tickle Record Rn_Appointment
				this.CascadeTickleTableRecord(modSystem.strt_RN_APPOINTMENTS, modSystem.strq_APPOINTMENT_WITH_CONTACT,vntRecordId, 3, blnImmediate);

				// Tickle Record Contact_Activities
				this.CascadeTickleTableRecord(modSystem.strt_CONTACT_ACTIVITIES, modSystem.strq_CONTACT_ACTIVITIES_WITH_CONTACT,vntRecordId, 4, blnImmediate);

				// Tickle Web Details
				this.TickleTableRecord(modSystem.strt_CONTACT_WEB_DETAILS, modSystem.strq_CONTACT_WEB_DETAILS_WITH_CONTACT,vntRecordId);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This subroutine tickles a Company record and relative tables' records
		/// to the mobile users and satellite servers managed by the DSM.
		/// Assumptions:
		/// </summary>
		/// /// <param name="vntRecordId">ID of the company to tickle</param>
		/// <param name="blnImmediate">True when batch tickle record is calling this procedure</param>
		/// <returns>None</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		public virtual void TickleCompany(object vntRecordId, bool blnImmediate)
		{
			try
			{
				if (!blnImmediate)
				{
					// The blnImmediate flag is not set
					// Read the system record
					Recordset rstSystem = this.GetSystemInfo();

					// Are Rn_Appts set to automatically resolve?
					if (TypeConvert.ToInt32(rstSystem.Fields[modSystem.strf_COMPANY_CASCADE_TICKLING].Value) == 1)
					{
						// automatic tickling, do nothing here
					}
					else if (TypeConvert.ToInt32(rstSystem.Fields[modSystem.strf_COMPANY_CASCADE_TICKLING].Value) == 2)
					{
						// Company set to batch resolve
						// Put a Tickle Record notification in the Tickle Record table
						this.SaveSysTickling(vntRecordId, 0);
						return ;
					}
					else
					{
						// No tickling
						return ;
					}
				}

				// Resolve the Company record
				RSysSystem.DoTickleRecord(RSysSystem.Tables[modSystem.strt_COMPANY].TableId, vntRecordId);

				// Tickle Record Alt_Address
				this.TickleTableRecord(modSystem.strt_ALT_ADDRESS, modSystem.strq_ALTERNATE_ADDRESSES_OF_COMPANY, vntRecordId);

				// Tickle Record Alt_Phone
				this.TickleTableRecord(modSystem.strt_ALT_PHONE, modSystem.strq_ALTERNATE_PHONE_OF_COMPANY, vntRecordId);

				// Tickle Record Company_Team_Member
				this.TickleTableRecord(modSystem.strt_COMPANY_TEAM_MEMBER, modSystem.strq_COMPANY_TEAM_MEMBER_OF_COMPANY,vntRecordId);

				// Tickle Record Reseller__Customer
				this.TickleTableRecord(modSystem.strt_RESELLER__CUSTOMER, modSystem.strq_RESELLER_CUSTOMERS_FOR_COMPANY,vntRecordId);

				// Tickle Record Contact
				this.CascadeTickleTableRecord(modSystem.strt_CONTACT, modSystem.strq_CONTACTS_WITH_COMPANY, vntRecordId,1, blnImmediate);

				// Tickle Record Opportunity
				this.CascadeTickleTableRecord(modSystem.strt_OPPORTUNITY, modSystem.strq_OPPORTUNITIES_WITH_COMPANY,vntRecordId, 2, blnImmediate);

				// Tickle Record Rn_Appointment
				this.CascadeTickleTableRecord(modSystem.strt_RN_APPOINTMENTS, modSystem.strq_APPOINTMENTS_WITH_COMPANY,vntRecordId, 3, blnImmediate);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This subroutine tickles the contents of all territory dependent tables to all
		/// mobile users and satellite servers.
		/// Assumptions:
		/// </summary>
		/// <returns>None</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		public virtual void TickleTable()
		{
			try
			{
				// Tickle Table Company
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_COMPANY].TableId);

				// Tickle Table Contact
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_CONTACT].TableId);

				// Tickle Table Opportunity
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_OPPORTUNITY].TableId);

				// Tickle Table Rn_Appointment
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_RN_APPOINTMENTS].TableId);

				// Tickle Table Alt_Address
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_ALT_ADDRESS].TableId);

				// Tickle Table Alt_Phone
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_ALT_PHONE].TableId);

				// Tickle Table Reseller__Customer
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_RESELLER__CUSTOMER].TableId);

				// Tickle Table Company_Team_Member
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_COMPANY_TEAM_MEMBER].TableId);

				// Tickle Table Literature_Listing
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_LITERATURE_LISTING].TableId);

				// Tickle Table Meeting_Contact_Attendee
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_MEETING_CONTACT_ATTENDEE].TableId);

				// Tickle Table Meeting_Staff_Attendee
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_MEETING_STAFF_ATTENDEE].TableId);

				// Tickle Table Opportunity__Influencer
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_OPPORTUNITY__INFLUENCER].TableId);

				// Tickle Table Opportunity__Product
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_OPPORTUNITY__PRODUCT].TableId);

				// Tickle Table Opportunity_Team_Member
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_OPPORTUNITY_TEAM_MEMBER].TableId);

				// Tickle Table Influence
				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_INFLUENCER_INFLUENCE].TableId);

				RSysSystem.DoTickleTable(RSysSystem.Tables[modSystem.strt_LEAD_].TableId);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This subroutine purges all records from the Sys_Tickling table.
		/// Assumptions:
		/// </summary>
		/// <returns>None</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		public virtual void PurgeTickleRecord()
		{
			try
			{
				DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

				// build recordset from sys_tickling table
				Recordset rstRecordset = objLib.GetRecordset(modSystem.strq_SYS_TICKLING_TABLE_RECORDS, 0, "Sys_Tickling_Id");

				if (rstRecordset.RecordCount > 0)
				{
					rstRecordset.MoveFirst();
					while(!(rstRecordset.EOF))
					{
						object vntRecordId = rstRecordset.Fields[modSystem.strf_SYS_TICKLING_ID].Value;
						// Delete record from database
						objLib.DeleteRecord(vntRecordId, modSystem.strt_SYS_TICKLING);
						rstRecordset.MoveNext();
					}
				}
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This function tickles all records in the Sys_Tickling table.
		/// Assumptions:
		/// </summary>
		/// <returns>int - the number of records tickled</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		public virtual int BatchRecordTickle()
		{
			try
			{
				DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();

				Recordset rstRecordset = objLib.GetRecordset(modSystem.strq_SYS_TICKLING_TABLE_RECORDS, 0, "Sys_Tickling_Id");
				int lngResult = 0;
				
				if (rstRecordset.RecordCount > 0)
				{
					rstRecordset.MoveFirst();
					while(!(rstRecordset.EOF))
					{
						// Ignore duplicates and records with null record id
						object vntRecordId = rstRecordset.Fields[modSystem.strf_SYS_TICKLING_ID].Value;
						if (!(Convert.IsDBNull(vntRecordId)))
						{
							//switch (rstRecordset.Fields[modSystem.strf_TABLE_INDICATOR].Value)
							//{
							//    case 0:
							//        break;
							//    default:
							//        break;
							//}
							if (TypeConvert.ToInt32(rstRecordset.Fields[modSystem.strf_TABLE_INDICATOR].Value) == 0)
							{
								this.TickleCompany(vntRecordId, true);
							}
							else if (TypeConvert.ToInt32(rstRecordset.Fields[modSystem.strf_TABLE_INDICATOR].Value) == 1)
							{
								this.TickleContact(vntRecordId, true);
							}
							else if (TypeConvert.ToInt32(rstRecordset.Fields[modSystem.strf_TABLE_INDICATOR].Value) == 2)
							{
								this.TickleRnAppointment(vntRecordId, true);
							}
							else if (TypeConvert.ToInt32(rstRecordset.Fields[modSystem.strf_TABLE_INDICATOR].Value) == 3)
							{
								this.TickleOpportunity(vntRecordId, true);
							}
							else if (TypeConvert.ToInt32(rstRecordset.Fields[modSystem.strf_TABLE_INDICATOR].Value) == 4)
							{
								this.TickleContactActivities(vntRecordId, true);
							}
							else if (TypeConvert.ToInt32(rstRecordset.Fields[modSystem.strf_TABLE_INDICATOR].Value) == 5)
							{
								this.TickleLead(vntRecordId, true);
							}
							lngResult++;
							objLib.DeleteRecord(vntRecordId, modSystem.strt_SYS_TICKLING);
						}
						rstRecordset.MoveNext();
					}
				}

				return lngResult;

			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This subroutine tickles a Lead record and relative tables' records
		/// to the mobile users and satellite servers managed by the DSM.
		/// Assumptions:
		/// </summary>
		/// <param name="vntRecordId">ID of the Lead to tickle</param>
		/// <param name="blnImmediate">True when batch tickle record is calling this procedure</param>
		/// <returns>None</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		public virtual void TickleLead(object vntRecordId, bool blnImmediate)
		{
			try
			{
				if (!blnImmediate)
				{
					// The blnImmediate flag is not set
					// Read the system record
					Recordset rstSystem = this.GetSystemInfo();

					// Are Leads set to automatically resolve?
					if (TypeConvert.ToInt32(rstSystem.Fields[modSystem.strf_LEAD_CASCADE_TICKLING].Value) == 1)
					{
						// automatic tickling, do nothing here
					}
					else if (TypeConvert.ToInt32(rstSystem.Fields[modSystem.strf_LEAD_CASCADE_TICKLING].Value) == 2)
					{
						// Company set to batch resolve
						// Put a Tickle Record notification in the Tickle Record table
						this.SaveSysTickling(vntRecordId, 5);
						return ;
					}
					else
					{
						// No tickling
						return ;
					}
				}

				// Resolve the Lead record
				RSysSystem.DoTickleRecord(RSysSystem.Tables[modSystem.strt_LEAD_], vntRecordId);

				// Resolve the Lead_Product_Interest records
				this.TickleTableRecord(modSystem.strt_PRODUCT_INTEREST, modSystem.strq_PRODUCT_INTEREST_FOR_LEAD_ID,vntRecordId);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This subroutine tickles a Contact Activities record and relative tables' records
		/// to the mobile users and satellite servers managed by the DSM.
		/// Assumptions:
		/// </summary>
		/// <param name="vntRecordId">ID of the Contact Activity to tickle</param>
		/// <param name="blnImmediate">True when batch tickle record is calling this procedure</param>
		/// <returns>None</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		public virtual void TickleContactActivities(object vntRecordId, bool blnImmediate)
		{
			try
			{
				if (!blnImmediate)
				{
					// The blnImmediate flag is not set
					// Read the system record
					Recordset rstSystem = this.GetSystemInfo();

					// Are Contact Activity set to automatically resolve?
					if (TypeConvert.ToInt32(rstSystem.Fields[modSystem.strf_CONTACT_ACTVY_CASCADE_TICKLING].Value) == 1)
					{
						// automatic tickling, do nothing here
					}
					else if (TypeConvert.ToInt32(rstSystem.Fields[modSystem.strf_CONTACT_ACTVY_CASCADE_TICKLING].Value) == 2)
					{

						// Contact Activities set to batch resolve
						// Put a Tickle Record notification in the Tickle Record table
						this.SaveSysTickling(vntRecordId, 4);
						return ;
					}
					else
					{
						// No tickling
						return ;
					}
				}

				// Resolve the Contact Activities record
				RSysSystem.DoTickleRecord(RSysSystem.Tables[modSystem.strt_CONTACT_ACTIVITIES].TableId, vntRecordId);

				// Resolve the Literature_Listing records1
				this.TickleTableRecord(modSystem.strt_LITERATURE_LISTING, modSystem.strq_LITERATURE_LISTING_FOR_CONTACT_ACTIVITIES,vntRecordId);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This function gets the web tab urls defined in the System table
		/// </summary>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		protected virtual object GetAllWebTabURLs()
		{
			try
			{
				object[] vntURLs = new object[4];
				DataAccess objLib = (DataAccess) RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
				Recordset rstSystem = objLib.GetRecordset(modSystem.strt_SYSTEM, modSystem.strf_ADMIN_WEB_TAB_URL, modSystem.strf_ADMIN_MOBILE_WEB_TAB_UR, modSystem.strf_FINANCIAL_CALC_MOBILE_WEB_TAB, modSystem.strf_FINANCIAL_CALC_WEB_TAB_URL);
				if (rstSystem.RecordCount > 0)
				{
					vntURLs[0] = VntToStr(rstSystem.Fields[modSystem.strf_ADMIN_WEB_TAB_URL].Value);
					vntURLs[1] = VntToStr(rstSystem.Fields[modSystem.strf_ADMIN_MOBILE_WEB_TAB_UR].Value);
					vntURLs[2] = VntToStr(rstSystem.Fields[modSystem.strf_FINANCIAL_CALC_WEB_TAB_URL].Value);
					vntURLs[3] = VntToStr(rstSystem.Fields[modSystem.strf_FINANCIAL_CALC_MOBILE_WEB_TAB].Value);
				}

				return vntURLs;

			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This function gets the mobile information from the rsys_system_flags table.
		/// </summary>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
        protected virtual string GetMobileInfo()
		{
			const string strcMOBILE = "Mobile";

			try
			{
				return (RSysSystem.BMSystemFlagExists(strcMOBILE) ? RSysSystem.GetBMSystemFlag(strcMOBILE) : string .Empty);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This function saves new system form data to the database.
		/// Assumptions:
		/// </summary>
		/// <param name="Recordsets">Variant array of recordsets of System form data</param>
		/// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
		/// <returns>Record Id for System</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
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
		/// This subroutine deletes the record for system table.
		/// Assumptions:
		/// </summary>
		/// <param name="RecordId">Record Id for System</param>
		/// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
		/// <returns>None</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
		{
			try
			{
				// delete the form
				pForm.DoDeleteFormData(RecordId, ref ParameterList);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This subroutine executes a specified method.
		/// Assumptions:
		/// </summary>
		/// <param name="pForm">IRform object reference to the client IRForm object</param>
		/// <param name="MethodName">Method name to be executed</param>
		/// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
		/// <returns>None
		/// Implements Agent: None</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
        ///             7/20/2006   JHui        Merged 3.7 SP1 in.
		/// </history>
		public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
		{
			//const string strsCUSTOMER_PROFILE_ADMINISTRATOR = "Customer Profile Administrator";
			const string strsCUSTOMER_PROFILE_ADMINISTRATOR = "PAHB Customer Profile";

			try
			{
				switch (MethodName)
				{
					case modSystem.strmCALCULATE_CUSTOMER_PROFILES:
						// Calculate customer profiles
						IRFormScript objCustomerProfile = (IRFormScript) RSysSystem.ServerScripts[strsCUSTOMER_PROFILE_ADMINISTRATOR].CreateInstance();
						IRForm form = RSysSystem.Forms[RSysSystem.UserProfile.get_DefaultFormId(RSysSystem.Tables[modSystem.strt_COMPANY].TableId)];

						objCustomerProfile.Execute(form, modSystem.strmCALCULATE_CUSTOMER_PROFILES, ref ParameterList);
						//objCustomerProfile.UpdateLTVsProfiles();
						ParameterList = new object[] {string.Empty /* EMPTY */ };
						break;
                    case modSystem.strmGET_RECORDSET_BY_ID:
                        TransitionPointParameter objParams = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                        DataAccess objDataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                        objParams.ParameterList = ParameterList;
                        object[] arrUserParams;
                        arrUserParams = objParams.GetUserDefinedParameterArray();
                        object vntRecordId = arrUserParams[0];
                        string strTable = TypeConvert.ToString(arrUserParams[1]);
                        object arrFields = arrUserParams[2];
                        Recordset rstRecordset;
                        rstRecordset = objDataAccess.GetRecordset(vntRecordId, strTable, arrFields);
                        ParameterList = new object [] {rstRecordset};
                        break;
					case modSystem.strmGET_SYSTEM:
						// Get system information
						ParameterList = new object[] { string.Empty, this.IsSystemRecordExist() };
						//ParameterList[1] = this.IsSystemRecordExist();
						break;
					case modSystem.strmGET_MOBILE_INFO:
						ParameterList = new object[] {GetMobileInfo()};
						break;
					case modSystem.strmGET_ALL_WEB_TAB_URLS:
						ParameterList = GetAllWebTabURLs();
						break;
					case modSystem.strmBATCH_UPDATE_QUOTE_EXPIRY:
						BatchUpdateQuoteExpiry();
						break;
					case modSystem.strmBATCH_UPDATE_PRICING:
						BatchUpdatePricing();
						break;
					case modSystem.strmGET_LCS_DATE_TIME:
						ParameterList = GetLCSDateTime();
						break;
					case modSystem.strmBATCH_UPDATE_LOT_STATUS:
						BatchUpdateLotStatus();
						break;
					case modSystem.strmBATCH_UPDATE_RELEASE_STATUS:
						BatchUpdateReleaseStatus();
						break;
					case modSystem.strmBATCH_UPDATE_NBHD_STATUS:
						BatchUpdateNBHDStatus();
						break;
					case modSystem.strmBATCH_UPDATE_QUOTE_STATUS:
						BatchUpdateQuoteStatus();
						break;
					case modSystem.strmBATCH_UPDATE_DNC:
						BatchUpdateDNC();
						break;
					case modSystem.strmBATCH_UPDATE_MARKET_PROJECT_STATUS:
						BatchUpdateMarketProjectStatus();
						break;
					case modSystem.strmGET_SYSTEM_FIELDS:
						ParameterList = new object[] { GetSystemFields(ParameterList) };
						break;
                    case modSystem.strmCLEAR_CONSTRUCTION_STAGE:
                        ClearConstructionStages();
                        break;
					default:
						string message = MethodName + " may not be the correct method name for object System because the system has not detected it.";
						throw new PivotalApplicationException(message, modSystem.glngERR_METHOD_NOT_DEFINED);
				}
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This function gets system form data from the database.
		/// Assumptions:
		/// </summary>
		/// <param name="RecordId">Record Id for System</param>
		/// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
		/// <returns>Variant array of recordsets of system form data</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
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
		/// This function gets a new system form from the database.
		/// Assumptions:
		/// </summary>
		/// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
		/// <returns>Variant array of recordsets of system form data</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual object NewFormData(IRForm pForm, ref object ParameterList)
		{
			try
			{
				if (!this.IsSystemRecordExist())
				{
					return pForm.DoNewFormData(ref ParameterList);
				}
				else
				{
					return null;
				}
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This subroutine adds a new record to a given secondary.
		/// Assumptions:
		/// </summary>
		/// <param name="SecondaryName">Secondary Name</param>
		/// <param name="ParameterList"> Transit Point Parameters passed from client to the AppServer</param>
        /// <param name="recordset"> Variant array of recordsets of System form data</param>
		/// <returns>None</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset recordset)
		{
			try
			{
				pForm.DoNewSecondaryData(SecondaryName, ref ParameterList, recordset);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This subroutine saves system form data to the database.
		/// Assumptions:
		/// </summary>
		/// <param name="Recordsets">Variant array of recordsets of System form</param>
		/// <param name="ParameterList">Transit Point Parameters passed from client to the AppServer</param>
		/// <returns>None</returns>
		/// Implements Agent: None
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
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
		/// This subroutine cascade tickles Table Record.
		/// </summary>
		/// Assumptionss:
		/// <param name="strQuery">Query Name</param>
		/// <param name="vntID">Record Id</param>
		/// <param name="int">Indicate Which table</param>
		/// <param name="blnImmediate">True when batch tickle record is calling this procedure</param>
		/// <returns>None</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		protected virtual void CascadeTickleTableRecord(string strTableName, string strQuery, object vntID, int intItem, bool blnImmediate)
		{
			try
			{
				DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
				object vntField = strTableName + "_Id";
				Recordset rstRecordset = objLib.GetRecordset(strQuery, 1, vntID, vntField);

				if (rstRecordset.RecordCount > 0)
				{
					rstRecordset.MoveFirst();
					while (!(rstRecordset.EOF))
					{
						object vntRecordId = rstRecordset.Fields[vntField].Value;
						switch (intItem)
						{
							case 1:
								// Tickle contact Record
								this.TickleContact(vntRecordId, blnImmediate);
								break;
							case 2:
								// Tickle Rn_Appointment Record
								this.TickleRnAppointment(vntRecordId, blnImmediate);
								break;
							case 3:
								// Tickle Opportunity Record
								this.TickleOpportunity(vntRecordId, blnImmediate);
								break;
							case 4:
								// Tickle Contact Activities Record
								this.TickleContactActivities(vntRecordId, blnImmediate);
								break;
						}
						rstRecordset.MoveNext();
					}
				}
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This function checks if there is any record in system table.
		/// Assumptions:
		/// </summary>
		/// <returns>True - exist</returns>
		// False - empty
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		protected virtual bool IsSystemRecordExist()
		{
			DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
			Recordset rstRecordset = objLib.GetRecordset(modSystem.strt_SYSTEM, "SYSTEM_ID");

			return ((rstRecordset != null) && (rstRecordset.RecordCount > 0));
		}

		/// <summary>
		/// This subroutine saves record which will be tickled to the Sys_Tickling table.
		/// Assumptions:
		/// </summary>
		/// <param name="sngTableIndicator">Indicate which table this record belongs to</param>
		///		0 - Company
		///		1 - Contact
		///		2 - Rn_Appointment
		///		3 - Opportunity
		///		4 - Contact Activities
		///		5 - Lead
		/// <returns>None</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		protected virtual void SaveSysTickling(object vntRecordId, int intTableIndicator)
		{
			try
			{
				if (Convert.IsDBNull(vntRecordId))
				{
					return;
				}

				DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
				Recordset rstRecordset = objLib.GetRecordset(modSystem.strt_SYS_TICKLING, new object[] { modSystem.strf_RECORD_ID, modSystem.strf_TABLE_INDICATOR });

				rstRecordset.AddNew(modSystem.strf_RECORD_ID, DBNull.Value);
				rstRecordset.Fields[modSystem.strf_TABLE_INDICATOR].Value = intTableIndicator;
				rstRecordset.Fields[modSystem.strf_RECORD_ID].Value = vntRecordId;

				objLib.SaveRecordset(modSystem.strt_SYS_TICKLING, rstRecordset);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// This subroutine tickles specified table based on given query and record Id.
		/// Assumptionss:
		/// </summary>
		/// <param name="strQuery">Query Name</param>
		/// <param name="vntID">Record Id</param>
		/// <returns>None</returns>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		protected virtual void TickleTableRecord(string strTableName, string strQuery, object vntID)
		{
			try
			{
				DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
				object vntTable_Id = RSysSystem.Tables[strTableName].TableId;
				string strField = string.Empty;

				if (strTableName == "Meeting_Staff_Attendee")
				{
					strField = strTableName + "s_Id";
				}
				else if (strTableName == "Contact_Team_Member")
				{
					strField = "Member_Team_Member_Id";
				}
				else
				{
					strField = strTableName + "_Id";
				}

				if (strField.Length > 30)
				{
					strField = strField.Substring(0, 30);
				}

				Recordset rstRecordset = objLib.GetRecordset(strQuery, 1, vntID, strField);
				if (rstRecordset.RecordCount > 0)
				{
					rstRecordset.MoveFirst();
					while (!(rstRecordset.EOF))
					{
						object vntRecordId = rstRecordset.Fields[strField].Value;
						// Tickle Record
						RSysSystem.DoTickleRecord(vntTable_Id, vntRecordId);
						rstRecordset.MoveNext();
					}
				}
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		public virtual string VntToStr(object vntVar)
		{
			try
			{
				return (Convert.IsDBNull(vntVar) ? string.Empty : TypeConvert.ToString(vntVar).Trim() );
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}

		/// <summary>
		/// Calls the Opportunity dll to perform the Batch Update Quote Expiry function
		/// </summary>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual object BatchUpdateQuoteExpiry()
		{
			try
			{
				IRFormScript objOpportunity = (IRFormScript) RSysSystem.ServerScripts[modSystem.strsOPPORTUNITY].CreateInstance();
                object id = RSysSystem.UserProfile.get_DefaultFormId(modSystem.strt_OPPORTUNITY);

                if (RSysSystem.EqualIds(id, null))
                    throw new PivotalApplicationException((string)RldtLangDict.GetTextSub((object)modSystem.stre_ERROR_GETTING_DEFAULT_FORM, new string[] { RSysSystem.UserProfile.UserName, modSystem.strt_OPPORTUNITY }));

				IRForm form = RSysSystem.Forms[id];
				
                object noObject = null;
				objOpportunity.Execute(form, modSystem.strmBATCH_UPDATE_QUOTE_EXPIRY, ref noObject); 
			}
			catch(Exception exc)
			{
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
			return null;
		}

		/// <summary>
		/// Calls the Opportunity dll to perform the Batch Update Quote Expiry function
		/// </summary>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual object BatchUpdatePricing()
		{
			try
			{
				IRFormScript objNBHDProduct = (IRFormScript) RSysSystem.ServerScripts[modSystem.strsNEIGHBORHOOD_PRODUCT].CreateInstance();
                object id = RSysSystem.UserProfile.get_DefaultFormId(modSystem.strt_NEIGHBORHOOD_PRODUCT);

                if (RSysSystem.EqualIds(id, null))
                    throw new PivotalApplicationException((string)RldtLangDict.GetTextSub((object)modSystem.stre_ERROR_GETTING_DEFAULT_FORM, new string[] { RSysSystem.UserProfile.UserName, modSystem.strt_NEIGHBORHOOD_PRODUCT }));

				IRForm form = RSysSystem.Forms[id];
				// call batch routine
				object noObject = null;
				objNBHDProduct.Execute(form, modSystem.strmBATCH_UPDATE_PRICING, ref noObject);
			}
			catch(Exception exc)
			{
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
			return null;
		}

		/// <summary>
		/// Calls the DNC logic
		/// </summary>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual object BatchUpdateDNC()
		{
			try
			{
				IRAppScript objAppScript = (IRAppScript) RSysSystem.ServerScripts["PAHB DO Not Contact Script Service"].CreateInstance();

				// call batch routine
				object vntParams = null;
				objAppScript.Execute("nothing", ref vntParams);
			}
			catch(Exception exc)
			{
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
			return null;
		}

		// GetLCSDateTime
		// Purpose : This function returns the Lifecycle Server Date and Time
		// Inputs : None
		// Returns : Returns Lifecycle server Date and Time
		// History :
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
        protected virtual DateTime GetLCSDateTime()
		{
			return DateTime.Now;
		}

		/// <summary>
		/// Calls the Product dll to perform the Batch Update Lot Status function
		/// </summary>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual object BatchUpdateLotStatus()
		{
			try
			{
				IRFormScript objProduct = (IRFormScript) RSysSystem.ServerScripts[modSystem.strsPRODUCT].CreateInstance();
                object id = RSysSystem.UserProfile.get_DefaultFormId(modSystem.strt_PRODUCT);

                if (RSysSystem.EqualIds(id, null))
                    throw new PivotalApplicationException((string)RldtLangDict.GetTextSub((object)modSystem.stre_ERROR_GETTING_DEFAULT_FORM, new string[] { RSysSystem.UserProfile.UserName, modSystem.strt_PRODUCT}));

                IRForm form = RSysSystem.Forms[id];

				// call batch routine
				object noObject = null;
				objProduct.Execute(form, modSystem.strmBATCH_UPDATE_LOT_STATUS, ref noObject);
			}
			catch(Exception exc)
			{
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
			return null;
		}

		/// <summary>
		/// Calls the NBHDPhase dll to perform the Batch Update Release Status function
		/// </summary>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual object BatchUpdateReleaseStatus()
		{
			try
			{
				IRFormScript objNBHD = (IRFormScript)RSysSystem.ServerScripts[modSystem.strsNEIGHBORHOOD_PHASE].CreateInstance();
                object id = RSysSystem.UserProfile.get_DefaultFormId("NBHD_Phase");

                if (RSysSystem.EqualIds(id, null))
                    throw new PivotalApplicationException((string)RldtLangDict.GetTextSub((object)modSystem.stre_ERROR_GETTING_DEFAULT_FORM, new string[] { RSysSystem.UserProfile.UserName, "NBHD_Phase"}));

                IRForm form = RSysSystem.Forms[id];
				
				// call batch routine
				object noObject = null;
				objNBHD.Execute(form,modSystem.strmBATCH_UPDATE_RELEASE_STATUS, ref noObject);
			}
			catch(Exception exc)
			{
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
			return null;
		}

		/// <summary>
		/// Calls the Neighborhood dll to perform the Batch Update Neighborhood Status function
		/// </summary>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual object BatchUpdateNBHDStatus()
		{
			try
			{
				IRFormScript objNBHD = (IRFormScript) RSysSystem.ServerScripts[modSystem.strsNEIGHBORHOOD].CreateInstance();
                object id = RSysSystem.UserProfile.get_DefaultFormId(modSystem.strt_NEIGHBORHOOD);

                if (RSysSystem.EqualIds(id, null))
                    throw new PivotalApplicationException((string)RldtLangDict.GetTextSub((object)modSystem.stre_ERROR_GETTING_DEFAULT_FORM, new string[] { RSysSystem.UserProfile.UserName, modSystem.strt_NEIGHBORHOOD}));

                IRForm form = RSysSystem.Forms[id];

				object noObject = null;
				objNBHD.Execute(form, modSystem.strmUPDATE_NEIGHBORHOOD_STATUS, ref noObject);
			}
			catch(Exception exc)
			{
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
			return null;
		}

		/// <summary>
		/// Calls the Opportunity dll to perform the Batch Update Quoteod Status function
		/// </summary>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual object BatchUpdateQuoteStatus()
		{
			try
			{
				IRFormScript objQuote = (IRFormScript) RSysSystem.ServerScripts[modSystem.strsOPPORTUNITY].CreateInstance();

                object id = RSysSystem.UserProfile.get_DefaultFormId(modSystem.strt_OPPORTUNITY);

                if (RSysSystem.EqualIds(id, null))
                    throw new PivotalApplicationException((string)RldtLangDict.GetTextSub((object)modSystem.stre_ERROR_GETTING_DEFAULT_FORM, new string[] { RSysSystem.UserProfile.UserName, modSystem.strt_OPPORTUNITY}));

                IRForm form = RSysSystem.Forms[id];

				// call batch routine
				object noObject = null;
				objQuote.Execute(form, modSystem.strmBATCH_UPDATE_QUOTE_STATUS, ref noObject);
			}
			catch(Exception exc)
			{
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
			return null;
		}

		/// <summary>
		/// Calls the MarketingProject dll to perform the Batch Update Market Project Status function
		/// </summary>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual object BatchUpdateMarketProjectStatus()
		{
			try
			{
				IRFormScript objMarketProject = (IRFormScript) RSysSystem.ServerScripts[modSystem.strsMARKETING_PROJECT].CreateInstance();

                object id = RSysSystem.UserProfile.get_DefaultFormId(modSystem.strt_MARKETING_PROJECT);

                if (RSysSystem.EqualIds(id, null))
                    throw new PivotalApplicationException((string)RldtLangDict.GetTextSub((object)modSystem.stre_ERROR_GETTING_DEFAULT_FORM, new string[] { RSysSystem.UserProfile.UserName, modSystem.strt_MARKETING_PROJECT}));

                IRForm form = RSysSystem.Forms[id];
                
				// call batch routine
				object noObject = null;
				objMarketProject.Execute(form, modSystem.strmBATCH_UPDATE_MARKET_PROJECT_STATUS, ref noObject);
			}
			catch(Exception exc)
			{
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
			return null;
		}

		/// <summary>
        /// This function will delete all the records present in Construction_Stage table
        /// </summary>
        /// <history>
        /// Revision#	Date		Author		Description
        /// 5.9.0.0		21/02/2007	ML      	Initial version
        /// </history>
        public virtual void ClearConstructionStages()
        {
            try
            {
                DataAccess dataAccess = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                Recordset rstConstStages = dataAccess.GetRecordset(modSystem.strt_CONSTRUCTION_STAGE, modSystem.strf_CONSTRUCTION_STAGE_ID);
                if (rstConstStages.RecordCount > 0)
                {
                    rstConstStages.MoveFirst();
                    while (!rstConstStages.EOF)
                    {
                        dataAccess.DeleteRecord(rstConstStages.Fields[modSystem.strf_CONSTRUCTION_STAGE_ID].Value, modSystem.strt_CONSTRUCTION_STAGE);
                        rstConstStages.MoveNext();
                    }
                }
            }
            catch (Exception exc)
            {
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
            }
        }

		/// <summary>
		/// Returns the System field values which were required by the calling method
		/// </summary>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		6/28/2006	dschaffer	Converted to .Net C# code.
		/// </history>
        protected virtual object GetSystemFields(object ParameterList)
		{
			try
			{
				TransitionPointParameter objParam = (TransitionPointParameter)RSysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
				objParam.ParameterList = ParameterList;
				// Dump out the user defined parameters
				object[] parameterArray = objParam.GetUserDefinedParameterArray();

				if ((parameterArray == null) || (parameterArray.Length == 0))
					return null;

				string fieldName = string.Empty;
				if (parameterArray[0] is Array)
				{
					object[] temp = (object[])parameterArray[0];
					fieldName = TypeConvert.ToString(temp[0]);
				}
				else
				{
					fieldName = TypeConvert.ToString(parameterArray[0]);
				}
				if (fieldName.Length == 0)
					return null;

				DataAccess objLib = (DataAccess)RSysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
				Recordset rstSystem = objLib.GetRecordset(modSystem.strqFIND_SYSTEM_WIDE_PROPERTIES_RECORD, 0, modSystem.strf_SYSTEM_ID, fieldName);

				return (rstSystem.BOF || rstSystem.EOF) ? null : rstSystem;
			}
			catch(Exception exc)
			{
                throw new PivotalApplicationException(exc.Message, exc, RSysSystem);
			}
		}
	}
}
