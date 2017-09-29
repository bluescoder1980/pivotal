using System;
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
	public class Tree : IRFormScript
	{
		/// <summary>
		/// </summary>
		/// This class represents the Tree object
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		private IRSystem7 mrsysSystem;
		private ILangDict mrldtLangDict;
		private const int intNUM_RECORDS = 5;

		// Aggregation types
		private const int intNONE = 0;
		private const int intALPHABET = 1;
		private const int intDAY = 2;
		private const int intWEEK = 3;
		private const int intMONTH = 4;
		private const int intYEAR = 5;
		private const int intUSE_DESCRIPTOR = 6;

		/// <summary>
		/// </summary>
		/// <param name="pForm">The IRForm object reference to the client IRForm object</param>
		/// <param name="Recordsets">Hold the reference for the current primary recordset and its all</param>
		// secondaries in the specified form
		/// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
		/// <returns>
		/// IRFormScript_AddFormData - Return information to IRSystem</returns>
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
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="pForm">The IRform object reference to the client IRForm object</param>
		/// <param name="RecordId">The business object record Id</param>
		/// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
		/// <returns>
		/// N/A
		/// Implements Agent:</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		 public virtual void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
		{
			try
			{
				pForm.DoDeleteFormData(RecordId, ref ParameterList);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
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
		/// ParameterList - Return executed result
		/// Implements Agent:</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		 public virtual void Execute(IRForm pForm, string MethodName, ref object ParameterList)
		{
			try
			{
				TransitionPointParameter objParam = (TransitionPointParameter)mrsysSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
				objParam.ParameterList = ParameterList;
				// Dump out the user defined parameters
				object[] parameterArray = objParam.GetUserDefinedParameterArray();

				switch (MethodName)
				{
					case "GetXMLForList":
						ParameterList = new object[] {
									GetXMLForList(
											TypeConvert.ToString(parameterArray[0]), 
											TypeConvert.ToString(parameterArray[1]),
											TypeConvert.ToString(parameterArray[2]))
									};
						break;
					default:
						string message = MethodName + " may not be the correct method name for object GenericCode because the system has not detected it.";
						throw new PivotalApplicationException(message, Convert.ToInt32(modSystem.glngERR_METHOD_NOT_DEFINED));
				}

				// Add the returned values into transit point parameter list
				ParameterList = objParam.SetUserDefinedParameterArray(parameterArray);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="pForm">The IRform object reference to the client IRForm object</param>
		/// <param name="RecordId">The Generic Code Id</param>
		/// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
		/// <returns>
		/// IRFormScript_LoadFormData  - The form data
		///</returns>
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
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
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
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		public virtual object NewFormData(IRForm pForm, ref object ParameterList)
		{
			try
			{
				return pForm.DoNewFormData(ref ParameterList);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
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
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This function updates the GenericCode plan
		/// Called by:
		/// </summary>
		/// <param name="pForm">The IRForm object reference to the client IRForm object</param>
		/// <param name="Recordsets">Hold the reference for the current primary recordset and its all</param>
		// secondaries in the specified form
		/// <param name="ParameterList">The Parameters passed from Client to Middle tier for Business rule</param>
		/// <returns></returns>
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
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// Public method to get current IRSystem7 reference
		/// </summary>
		/// <param name="pSystem">Hold the current System Instance Reference</param>
		/// <returns></returns>
		// N/A
		public virtual void SetSystem(RSystem pSystem)
		{
			try
			{
				mrsysSystem = (IRSystem7) pSystem;
				mrldtLangDict = mrsysSystem.GetLDGroup("System");
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is used to get the XML for a List
		/// Called by:
		/// </summary>
		// strListName = the name of the list we are basing the tree xml on
		// strCSName = client script name
		/// <returns>
		/// string - this is the string containing the xml for the tree object
		///</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private string GetXMLForList(string strListName, string strCSName, string strParameters)
		{
			try
			{
				XmlDocument objXML = null;
				Recordset rstList = null;
				Recordset rstListLevels = null;

				if (strListName.Length > 0)
				{
					// get the list recordset
					rstList = GetListRecordset(strListName);
					if (rstList.RecordCount == 0)
					{
						return string.Empty;
					}
					object vntList_Id = rstList.Fields[modSystem.strfLISTS_ID].Value;

					// get the list level recordset
					rstListLevels = GetListLevelRecordset(vntList_Id);
				}

				// build the xml from the Client Script
				if (strCSName.Length > 0)
				{
					// TODO (NETCOOLE) ISSUE: Method or data member not found: 'ClientScripts'
					string strClientScript = TypeConvert.ToString(mrsysSystem.ClientScripts[strCSName].Text);
					if (strClientScript.Length > 0)
					{
						try
						{
							objXML = new XmlDocument();
							objXML.LoadXml(strClientScript);
						}
						catch (XmlException xexc)
						{
							string errMessage = "The following errors were generated while loading the XML definition: " + "\r\n" + xexc.Message + "\r\n" + "On line: " + xexc.LineNumber;
							throw new PivotalApplicationException(errMessage, xexc, mrsysSystem);
						}
					}
				}

				// build the xml from the list and list level recordsets
				return GetTreeXML(rstList, rstListLevels, objXML, strParameters);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is used to get the List Level recordset
		/// Called by:
		/// </summary>
		// vntList_Id = the list id
		/// <returns>
		/// a recordset containing the list levels for the list</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private Recordset GetListLevelRecordset(object vntList_Id)
		{
			try
			{
				Recordset rstListLevels = new Recordset();
				Connection objConnBM = new Connection();
				string strSchema = String.Empty;
				
				// get oracle info
				if (mrsysSystem.ServerBrand == CRServerBrand.SQL_BRAND_ORACLE)
				{
					strSchema = mrsysSystem.UserSchema + ".";
				}

				// Set up BM Connection Object
				objConnBM.CursorLocation = (CursorLocationEnum)Convert.ToInt32(CursorLocationEnum.adUseClient);
				// TODO (NETCOOLE) ISSUE: Method or data member not found: 'BusinessString'
				objConnBM.Open(TypeConvert.ToString(mrsysSystem.BusinessString), "", "", -1);

				// Get the list levels from the BM
				string strSQL = "Select * from " + strSchema + "List_Levels where List_Id=" + mrsysSystem.IdToString(vntList_Id) + " order by level_number ASC";
				rstListLevels.Open(
									strSQL, 
									objConnBM, 
									CursorTypeEnum.adOpenForwardOnly, 
									(LockTypeEnum)(-1),
									-1);

				return rstListLevels;

			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is used to get the list recordset
		/// Called by:
		/// </summary>
		// strListName = the list name (note: based on the rules in the customization system, the name has to be unique)
		/// <returns>
		/// a recordset containing the list</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private Recordset GetListRecordset(string strListName)
		{
			try
			{
				string strSchema = String.Empty;

				// get oracle info
				if (mrsysSystem.ServerBrand == CRServerBrand.SQL_BRAND_ORACLE)
				{
					strSchema = mrsysSystem.UserSchema + ".";
				}

				// Set up BM Connection Object
				Connection objConnBM = new Connection();
				objConnBM.CursorLocation = CursorLocationEnum.adUseClient;
				objConnBM.Open(TypeConvert.ToString(mrsysSystem.BusinessString), "", "", -1);

				// Get the list from the BM
				string strSQL = "Select * from " + strSchema + "Lists where List_Name='" + strListName + "'";
				
				Recordset rstList = new Recordset();
				rstList.Open(strSQL, objConnBM, CursorTypeEnum.adOpenForwardOnly, (LockTypeEnum)(-1), -1);

				return rstList;

			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is used to build the rdaTree xml for the list and list levels recordsets
		/// structure of the xml:
		/// <Relationships>
		/// <nodes rootNode='Relationships'>
		/// <node id='0x0000000000000001' name='Household'></node>
		/// <node id='0x0000000000000009' name='Internal Prospect'>
		/// <node id='0x00000000000001FA;;0x0000000000000231' name='Global One Financial'/></node>
		/// </nodes>
		/// <menus>
		/// <menu level='0'>
		/// <item name='Add New Relationship' event='add'/>
		/// </menu>
		/// <menu level='1'>
		/// <item name='Delete Relationships' event='delete'/>
		/// </menu>
		/// <menu level='2'>
		/// <item name='Edit Relationship' event='edit'/>
		/// <item name='Delete Relationship' event='delete'/>
		/// </menu>
		/// </menus>
		/// </Relationships>
		/// Called by:
		/// </summary>
		/// <param name="rstList">the list recorset</param>
		/// <param name="rstListLevel">the list level recordset</param>
		/// <returns>
		/// a string containing the tree xml</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private string GetTreeXML(Recordset rstList, Recordset rstListLevel, XmlDocument objXML, string strParameters)
		{
			try
			{
				string strXML = String.Empty;
				XmlNodeList objQueries = null;

				// get the where clause
				if (objXML != null)
				{
					objQueries = objXML.SelectNodes("//query");
				}

				if (rstList != null)
				{
					string strTableName = TypeConvert.ToString(mrsysSystem.Tables[rstList.Fields[modSystem.strfTABLE_ID].Value].TableName);
					strXML += "<" + strTableName + ">";
					strXML += "<nodes rootNode='" + strTableName + "'>";
					strXML += GetListLevelXML(rstListLevel, 1, DBNull.Value, "", "", "");
					strXML += "</nodes>";
					strXML += "<menus>";
					strXML += GetMenuXML(rstListLevel);
					strXML += "</menus>";
					strXML += "</" + strTableName + ">";
				}

				if (objQueries != null)
				{
					if (objQueries.Count > 0)
					{
						strXML += GetQueryXML(objXML, strParameters);
					}
				}

				return strXML;
			}
			catch (Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is used to get all of the fields and all of the records from a table
		/// Called by:
		/// </summary>
		/// <param name="strTableName">the name of the table to retrieve the records from</param>
		/// <returns>
		/// a recordset containing the information requested</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private Recordset GetFullRecordset(string strTableName)
		{
			try
			{
				string strSchema = String.Empty;

				DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
				Connection objConn = CreateED_ADOConnection();

				// get oracle info
				if (mrsysSystem.ServerBrand == CRServerBrand.SQL_BRAND_ORACLE)
				{
					// TODO (NETCOOLE) ISSUE: Method or data member not found: 'UserSchema'
					strSchema = mrsysSystem.UserSchema + ".";
				}

				return GetRecordsetByCustomQuery("select * from " + strSchema + strTableName, objConn);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is used to get all of the fields and all of the records from a table
		/// Called by:
		/// </summary>
		/// <param name="strTableName">the name of the table to retrieve the records from</param>
		/// <param name="strLinkField">the link field</param>
		/// <param name="vntLinkRecordId">the link record id</param>
		/// <returns>
		/// a recordset containing the information requested</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private Recordset GetFullLinkedRecordset(string strTableName, string strLinkField, object vntLinkRecordId)
		{
			try
			{
				DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
				Connection objConn = CreateED_ADOConnection();
				string strSchema = String.Empty;

				// get oracle info
				if (mrsysSystem.ServerBrand == CRServerBrand.SQL_BRAND_ORACLE)
				{
					strSchema = mrsysSystem.UserSchema + ".";
				}

				string strSQL = "select * from " + strSchema + strTableName + " where " + strLinkField + "='" + mrsysSystem.IdToString(vntLinkRecordId) + "'";
				return GetRecordsetByCustomQuery("select * from " + strTableName, objConn);
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}
		
		/// <summary>
		/// This Recursive Function is used to get the xml for the list levels
		/// Called by:
		/// </summary>
		/// <param name="rstListLevel">the list level recordset</param>
		/// <param name="intLevelNumber">the list level number</param>
		/// <param name="vntRecordId">the record id</param>
		/// <param name="strWhereValue">the where clause</param>
		/// <param name="strQueryName">query name</param>
		/// <param name="strParameters">extra parameters</param>
		/// <returns>
		/// the string containing the list level xml</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private string GetListLevelXML(
								Recordset rstListLevel, 
								int intLevelNumber, 
								object vntRecordId, 
								string strWhereValue,
								string strQueryName, 
								string strParameters)
		{
			try
			{
				DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
				Connection objConnED = CreateED_ADOConnection();
				Recordset rstAggregated = null;
				Recordset rstRoot = null;

				string strSchema = string.Empty;
				string returnValue = string.Empty;
				string strWhereClause = string.Empty;
				string strFieldName = string.Empty;
				string strLinkField = string.Empty;
				string strFieldDescription = string.Empty;
				string strXML = string.Empty;
				int i = 0;
				object vntNewRecordId = DBNull.Value;

				// get oracle info
				if (mrsysSystem.ServerBrand == CRServerBrand.SQL_BRAND_ORACLE)
				{
					strSchema = mrsysSystem.UserSchema + ".";
				}

				// get the full recordset for this table
				rstListLevel.Filter = "Level_Number=" + intLevelNumber;
				if (rstListLevel.RecordCount == 0)
				{
					return string.Empty;
				}
				else
				{
					rstListLevel.MoveFirst();
				}
				int intAggregation = TypeConvert.ToInt32(rstListLevel.Fields[modSystem.strfAGGREGATIONS].Value);
				string strDescriptorFormula = TypeConvert.ToString(rstListLevel.Fields[modSystem.strfDESCRIPTOR_FORMULA].Value);
				
				if (strDescriptorFormula.Contains("."))
				{
					// return everything to the right of the '.'
					strFieldName = strDescriptorFormula.Substring(0,strDescriptorFormula.IndexOf('.')-1);
				}
				else
				{
					strFieldName = strDescriptorFormula;
				}
				object vntForeignKeyId = rstListLevel.Fields[modSystem.strfFOREIGN_KEY_ID].Value;
				string strTableName = TypeConvert.ToString(mrsysSystem.Tables[rstListLevel.Fields[modSystem.strfTABLE_ID].Value].TableName);

				// get the aggregated recordset
				if (strQueryName.Length > 0)
				{
					// Set rstAggregated = objLib.GetRecordset(strQueryName, strTableName, 0, strFieldName)
					rstAggregated = GetCustomRecordset(strQueryName, strTableName, strParameters, strFieldName);
				}
				else
				{
					rstAggregated = GetRecordsetByCustomQuery(GetAggregatedSQL(intAggregation, strSchema + strTableName,
						strDescriptorFormula, strWhereValue), objConnED);
				}
				
				if (Convert.IsDBNull(rstListLevel.Fields[modSystem.strfFOREIGN_KEY_ID].Value))
				{
					strLinkField = TypeConvert.ToString(mrsysSystem.Tables[strTableName].PrimaryKeyField.FieldName);
				}
				else
				{
					strLinkField = TypeConvert.ToString(mrsysSystem.Tables[strTableName].Fields[vntForeignKeyId].FieldName);
				}

				rstListLevel.Filter = "";

				// recursive exit
				if (intLevelNumber == rstListLevel.RecordCount)
				{
					if (strQueryName.Length == 0)
					{
						strWhereClause = GetAggregatedWhereClause(intAggregation, TypeConvert.ToString(rstAggregated.Fields["fieldDescription"].Value), strDescriptorFormula);
					}

					if (strWhereClause.Length > 0 && strWhereValue.Length > 0)
					{
						strWhereClause = strWhereValue + " AND " + strWhereClause;
					}
					else
					{
						strWhereClause = strWhereValue;
					}

					if (strQueryName.Length == 0)
					{
						rstRoot = GetRecordsetByCustomQuery(GetNextLevelSql(intAggregation, strSchema + strTableName,
										strDescriptorFormula, strWhereClause), objConnED);
					}
					else
					{
						rstRoot = rstAggregated;
					}

					string strPKName = TypeConvert.ToString(mrsysSystem.Tables[strTableName].PrimaryKeyField.FieldName);
					while(!rstRoot.EOF && !rstRoot.BOF)
					{
						if (strQueryName.Length == 0)
						{
							strFieldDescription = XMLEncode(TypeConvert.ToString(rstRoot.Fields["fieldDescription"].Value));
						}
						else
						{
							strFieldDescription = XMLEncode(TypeConvert.ToString(rstRoot.Fields[strFieldName].Value));
						}
						vntNewRecordId = rstRoot.Fields[strPKName].Value;
						strXML = strXML + "<node id='" + mrsysSystem.IdToString(vntNewRecordId) + ";" + strTableName + "' name='" + XMLEncode(strFieldDescription) + "'></node>";
						rstRoot.MoveNext();
					}
					returnValue = strXML;
					return returnValue;
				}

				i = 1;
				while(((!rstAggregated.EOF && !rstAggregated.BOF) && (intLevelNumber == 1 && i <= intNUM_RECORDS)) ||
					(!rstAggregated.EOF && !rstAggregated.BOF && intLevelNumber > 1))
				{
					if (strWhereValue != "")
					{
						strWhereClause = strWhereValue + " AND " + GetAggregatedWhereClause(intAggregation, TypeConvert.ToString(rstAggregated.Fields["fieldDescription"].Value), strDescriptorFormula);
					}
					else
					{
						strWhereClause = strWhereValue + GetAggregatedWhereClause(intAggregation, TypeConvert.ToString(rstAggregated.Fields["fieldDescription"].Value), strDescriptorFormula);
					}

					strXML = strXML + "<node id=';;;;" + strTableName + "' name='" + XMLEncode(GetDescriptor(TypeConvert.ToString(rstAggregated.Fields["fieldDescription"].Value), intAggregation)) + "'>";
					strXML = strXML + GetListLevelXML(rstListLevel, (intLevelNumber + 1), vntNewRecordId, strWhereClause,
						String.Empty, String.Empty);
					strXML = strXML + "</node>";
					rstAggregated.MoveNext();
				}

				returnValue = strXML;

				return returnValue;

			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is used to return the aggregation sql
		/// Called by:
		/// </summary>
		/// <param name="intAggregation">the aggregation value</param>
		/// <param name="strTableName">the table name</param>
		/// <param name="strDescriptor">the descriptor function</param>
		/// <param name="strWhereValue">the where clause</param>
		/// <returns>
		/// string containing the sql</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private string GetAggregatedSQL(int intAggregation, string strTableName, string strDescriptor, string strWhereValue)
		{
			try
			{
				string strSQL = String.Empty;

				switch (intAggregation)
				{
					case intALPHABET:
						strSQL = "select distinct UPPER(LEFT(" + strDescriptor + ",1)) as fieldDescription  from " + strTableName;
						break;
					case intDAY:
						strSQL = "SELECT DISTINCT LTRIM(RTRIM(STR({fn YEAR(" + strDescriptor + ")}))) + '/' + LTRIM(RTRIM(STR({fn MONTH(" + strDescriptor + ")}))) + '/' + LTRIM(RTRIM(STR({fn DAYOFMONTH(" + strDescriptor + ")}))) as fieldDescription FROM  " + strTableName + " ORDER BY fieldDescription";
						break;
					case intWEEK:
						strSQL = "SELECT DISTINCT LTRIM(RTRIM(STR({fn YEAR(" + strDescriptor + ")}))) + '_' + LTRIM(RTRIM(STR({fn WEEK(" + strDescriptor + ")}))) as fieldDescription FROM  " + strTableName + " ORDER BY fieldDescription";
						break;
					case intMONTH:
						strSQL = "SELECT DISTINCT LTRIM(RTRIM(STR({fn YEAR(" + strDescriptor + ")}))) + '_' + LTRIM(RTRIM(STR({fn MONTH(" + strDescriptor + ")}))) as fieldDescription FROM " + strTableName + " ORDER BY fieldDescription";
						break;
					case intYEAR:
						strSQL = "SELECT DISTINCT {fn YEAR(" + strDescriptor + ")} as fieldDescription FROM  " + strTableName + "  ORDER BY fieldDescription";
						break;
					case intNONE:
						strSQL = "select distinct Rn_Descriptor as fieldDescription from " + strTableName;
						break;
					case intUSE_DESCRIPTOR:
						strSQL = "select distinct " + strDescriptor + " as fieldDescription from " + strTableName;
						break;
				}

				if (strWhereValue.Length > 0)
				{
					strSQL += " where " + strWhereValue;
				}

				return strSQL;

			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is used to get the next level sql
		/// Called by:
		/// </summary>
		/// <param name="intAggregation">the aggregation value</param>
		/// <param name="strTableName">the table name</param>
		/// <param name="strDescriptor">the descriptor function</param>
		/// <param name="strWhereValue">the where clause</param>
		/// <returns>
		/// string containing the sql</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private string GetNextLevelSql(int intAggregation, string strTableName, string strDescriptor, string strWhereValue)
		{
			try
			{
				string strPKName = TypeConvert.ToString(mrsysSystem.Tables[strTableName].PrimaryKeyField.FieldName);
				string strSQL = "select distinct " + strDescriptor + " as fieldDescription, " + strPKName + " from " + strTableName;

				if (strWhereValue.Length > 0)
				{
					strSQL += " where " + strWhereValue;
				}

				return strSQL;

			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is used to get the where clause for the aggregated value
		/// Called by:
		/// </summary>
		/// <param name="intAggregation">the aggregation value</param>
		/// <param name="strValue">the value to build the where clause from</param>
		/// <param name="strField">the field which to build the where clause from</param>
		/// <returns>
		/// a string containing the where clause - minus the word WHERE</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private string GetAggregatedWhereClause(int intAggregation, string strValue, string strField)
		{
			try
			{
				string strResult = String.Empty;
				DateTime date = Convert.ToDateTime(strValue);
				
				switch (intAggregation)
				{
					case intALPHABET:
						strResult = " LEFT(" + strField + ", 1) = '" + strValue + "'";
						break;
					case intDAY:
						strResult = " " + "(({fn YEAR(" + strField + ")} = " + date.Year + " AND {fn MONTH(" + strField + ")} = " + date.Month + " AND {fn DAYOFMONTH(" + strField + ")} = " + date.Day + "))";
						break;
					case intWEEK:
						strResult = " " + "(({fn YEAR(" + strField + ")} = " + strValue.Substring(0, 4) + " AND {fn WEEK(" + strField + ")} = " + strValue.Substring(strValue.IndexOf('_') + 1) + "))";
						break;
					case intMONTH:
						strResult = " " + "(({fn YEAR(" + strField + ")} = " + strValue.Substring(0, 4) + " AND {fn MONTH(" + strField + ")} = " + strValue.Substring(strValue.IndexOf('_') + 1) + "))";
						break;
					case intYEAR:
						strResult = " " + "{fn YEAR(" + strField + ")} = " + strValue;
						break;
					case intNONE:
						break;
					case intUSE_DESCRIPTOR:
						strResult = " " + strField + " = '" + strValue + "'";
						break;
				}

				return strResult;

			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is used to replace all of the special chars: ' " < > & with xml encoded strings
		/// Called by:
		/// </summary>
		/// <param name="strXML">the xml string to be encoded</param>
		/// <returns>
		/// a encoded xml string</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private string XMLEncode(string strXML)
		{
			string strTempXML = strXML;

			if (strTempXML.Length > 0)
			{
				strTempXML = strTempXML.Replace("&", "&amp;");
				strTempXML = strTempXML.Replace("'", "&apos;");
				strTempXML = strTempXML.Replace("\"", "&quot;");
				strTempXML = strTempXML.Replace("<", "&lt;");
				strTempXML = strTempXML.Replace(">", "&gt;");
			}

			return strTempXML;
		}

		/// <summary>
		/// This Function is used to get the descriptor value from the recordset
		/// Called by:
		/// </summary>
		// rstRecordset = the recordset
		// intAggregation = the aggregation value
		/// <returns>
		/// string containing the descriptor</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private string GetDescriptor(string strFieldValue, int intAggregation)
		{
			try
			{
				string strResult = String.Empty;
				switch (intAggregation)
				{
					case intALPHABET:
						strResult = strFieldValue.Trim();
						break;
					case intDAY:
						strResult = string.Format("dddd, mmmm dd, yyyy", strFieldValue);
						break;
					case intWEEK:
						//strValue.Substring(strValue.IndexOf('_') + 1)
						//strResult = "Week " & Right$(vntFieldValue, Len(vntFieldValue) - InStr(1, vntFieldValue, "_")) & ", " & Left$(vntFieldValue, 4)
						strResult = "Week " + strFieldValue.Substring(strFieldValue.IndexOf('_') + 1) + ", " + strFieldValue.Substring(0, 4);
						break;
					case intMONTH:
						//strResult = Right$(vntFieldValue, Len(vntFieldValue) - InStr(1, vntFieldValue, "_")) & "/15/" & Left$(vntFieldValue, 4)
						//strResult = Format(strResult, "mmmm") & ", " & Left$(vntFieldValue, 4)
						strResult = strFieldValue.Substring(strFieldValue.IndexOf('_') + 1) + "/15/" + strFieldValue.Substring(0, 4);
						strResult = string.Format("mmmm", strResult) + ", " + strFieldValue.Substring(0, 4);
						break;
					case intYEAR:
						strResult = strFieldValue.Trim();
						break;
					case intNONE:
						break;
					case intUSE_DESCRIPTOR:
						strResult = strFieldValue.Trim();
						break;
				}
				
				return strResult;

			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is used to get the Menu XML
		/// Called by:
		/// </summary>
		/// <param name="rstListLevel">the list level recordset</param>
		/// <returns>
		///</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private string GetMenuXML(Recordset rstListLevel)
		{
			try
			{
				string strXML = string.Empty;

				if ((rstListLevel != null) && (rstListLevel.RecordCount > 0))
				{
					rstListLevel.MoveFirst();

					int i = 1;

					while (!rstListLevel.BOF && !rstListLevel.EOF)
					{
						string strTableName = mrsysSystem.Tables[rstListLevel.Fields[modSystem.strfTABLE_ID].Value].TableName;
						strXML += "<menu level='" + i + "'>";
						if (i == rstListLevel.RecordCount)
						{
							strXML += "  <item name='Edit' event='edit'/>";
							strXML += "  <item name='Delete' event='delete'/>";
						}
						else if (i == (rstListLevel.RecordCount - 1))
						{
							strXML += "  <item name='Add New' event='add'/>";
						}
						strXML += "</menu>";

						i++;
						rstListLevel.MoveNext();
					}
				}
				
				return strXML;

			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is used to get a recordset based on a query name, table name and a list of
		/// parameters in string format - id;id;id
		/// Called by:
		/// </summary>
		// strQueryName = the name of the query
		// strTableName = the name of the table
		// strParameters = a string containing the id parameters - (e.g. id;id;id 0x000000000000001;0x0000000000000002;0x0000000000000003)
		// strFields = a string containing the field parameters - (e.g. field;field;field;...)
		/// <returns>
		/// a recordset</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private Recordset GetCustomRecordset(string strQueryName, string strTableName, string strParameters, string strFields)
		{
			try
			{
				DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
				string[] arrParameters = strParameters.Split(';');
				string[] arrNewParameters = new string[arrParameters.Length - 1];
				//string[] arrFields = strFields.Split(';');

				//// Create Dataset object
				//rdstDataset = mrsysSystem.CreateDataset;
				//// Assign the table to be searched
				//rdstDataset.TableName = strTableName;
				//// Get query object
				//if (strQueryName != "")
				//{
				//    rdstDataset.Query = mrsysSystem.Queries.Item(strQueryName);
				//}
				//// Pass parameters
				for (int i = 0; i <= arrParameters.Length - 1; i++)
				{
					arrNewParameters[i] = InttoIdHexStr(TypeConvert.ToInt32(arrParameters[i]));
					//rdstDataset.SetParameter(i + 1, mrsysSystem.StringToId(InttoIdHexStr(arrParameters[i])));
				}
				//// Append fields
				//for(int i = 0;i <= arrFields.GetUpperBound(0); i++)
				//{
				//    rdstDataset.Fields.Append(arrFields[i]);
				//}
				//rdstDataset.Fields.Append(mrsysSystem.Tables(strTableName).PrimaryKeyField.FieldName);
				//rdstDataset.Fields.Append(strfRN_DESCRIPTOR);

				// Actually execute the query
				//return rdstDataset.BuildRecordset;

				return objLib.GetRecordset(strQueryName, arrParameters.Length, arrNewParameters);

			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// Private function to convert an integer to its binary hex representation
		/// </summary>
		// strInt = the id value
		/// <returns>
		/// string containing the id</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private string InttoIdHexStr(int intValue)
		{
			return "0x" + (intValue.ToString("X").PadLeft(16, '0'));

			//string strHexValue = String.Empty;
			//string strBinaryId = "0x0000000000000000";
			//string strFirstPart = String.Empty;

			//// TODO (PIV) The following code may contain the type from Microsoft.VisualBasic namespace, please convert
			//// them by using .Net Framework.
			
			//strHexValue = Conversion.Hex(strInt);
			//// TODO (PIV) The following code may contain the type from Microsoft.VisualBasic namespace, please convert
			//// them by using .Net Framework.
			//strFirstPart = strBinaryId.Substring(0, Strings.Len(strBinaryId) - Strings.Len(strHexValue));
			//return strFirstPart + strHexValue;
		}

		/// <summary>
		/// This Function is used to get the xml for a list of queries
		/// Called by:
		/// </summary>
		/// <param name="objXML">the xml document</param>
		/// <returns>
		/// a string containing the node xml for the list of queries</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private string GetQueryXML(XmlDocument objXML, string strParameters)
		{
			try
			{
				string strXML = String.Empty;
				XmlNodeList objQueries = null;
				XmlNode objMenu = null;
				string strDisplayName = String.Empty;
				string strTableName = String.Empty;
				string strQueryName = String.Empty;
				string strDisplayField = String.Empty;
				string strTitle = String.Empty;
				string strFormName = String.Empty;
				string strGroupByField = String.Empty;
				string strFields = String.Empty;
				string strID = String.Empty;
				string strPKFieldName = String.Empty;
				string strMenu = String.Empty;
				int intNumParams = 0;
				string strTempString = String.Empty;
				int intIndexParams = 0;
				string[] arrParams = null;
				string strTempParams = String.Empty;

				DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
				Connection objConnED = CreateED_ADOConnection();
				int intQueryNumber = 0;

				// get the queries node
				try
				{
					objQueries = objXML.SelectNodes("//query");
				}
				catch
				{
					throw new PivotalApplicationException("Missing queries node in XML definition", 1);
				}

				// get the parameters
				arrParams = strParameters.Split(new char[] {';' });
				strXML = "<List>";
				strTitle = string.Empty;
				try
				{
					strTitle = objXML.DocumentElement.SelectSingleNode("./title").InnerText;
				}
				catch
				{
					strTitle = "Missing Name";
				}
				strXML += "<nodes rootNode='" + strTitle + "'>";

				// loop through the nodes and build the xml for each query
				foreach(XmlNode objQuery in objQueries)
				{
					// get the required variables and do some error handling
					try
					{
						strQueryName = objQuery.SelectSingleNode("./name").InnerText;
					}
					catch
					{
						throw new PivotalApplicationException("Missing name node for query in XML definition.", 1);
					}
					try
					{
						strTableName = objQuery.SelectSingleNode("./tableName").InnerText;
					}
					catch
					{
						throw new PivotalApplicationException("Missing tableName node in XML definition.", 1);
					}
					try
					{
						strDisplayName = objQuery.SelectSingleNode("./displayName").InnerText;
					}
					catch
					{
						strDisplayName = strQueryName;
					}
					try
					{
						strDisplayField = objQuery.SelectSingleNode("./displayField").InnerText;
					}
					catch
					{
						strDisplayField = modSystem.strfRN_DESCRIPTOR;
					}
					try
					{
						intNumParams = TypeConvert.ToInt32(objQuery.SelectSingleNode("./numParams").InnerText);
					}
					catch
					{
						throw new PivotalApplicationException("Missing numParams node in XML def", 1);
					}
					try
					{
						strGroupByField = objQuery.SelectSingleNode("./groupByField/name").InnerText;
					}
					catch
					{
						// do nothing on error
					}
					if (mrsysSystem.ServerBrand == CRServerBrand.SQL_BRAND_ORACLE)
					{
						strTempString = mrsysSystem.UserSchema + "." + strTableName;
					}
					else
					{
						strTempString = strTableName;
					}

					Recordset rstAggregation = GetRecordsetByCustomQuery("select distinct " + strGroupByField + " from " + strTempString, objConnED);
					try
					{
						strFormName = objQuery.SelectSingleNode("./formName").InnerText;
					}
					catch
					{
					}
					try
					{
						objMenu = objQuery.SelectSingleNode("./menus");
						if (objMenu != null)
							strMenu += BuildMenuItem(objMenu, 1, intQueryNumber, strTableName);
					}
					catch
					{
					}

					// get the parameters
					for(int i = 0;i <= intNumParams - 1; i++)
					{
						strTempParams = arrParams[i];
					}
					intIndexParams = intNumParams;

					// get the recordset
					if (strGroupByField.Length > 0)
					{
						strFields = strDisplayField + ";" + strGroupByField;
					}
					else
					{
						strFields = strDisplayField;
					}
					Recordset rstRecordset = GetCustomRecordset(strQueryName, strTableName, strTempParams, strFields);

					strXML += "<node id=';;;;" + strTableName + "' name='" + strDisplayName + "'>";
					// build the xml
					if (strGroupByField.Length > 0)
					{
						// build the menu for this level
						if (objMenu != null)
						{
							strMenu += BuildMenuItem(objMenu, 2, intQueryNumber, strTableName);
						}
						if (rstAggregation.RecordCount > 0)
						{
							rstAggregation.MoveFirst();
							while(!rstAggregation.BOF && !rstAggregation.EOF)
							{
								rstRecordset.Filter = " " + strGroupByField + "='" + rstAggregation.Fields[strGroupByField].Value + "'";
								
								if (rstRecordset.RecordCount > 0)
								{
									strXML += "<node id='' name='" + rstRecordset.Fields[strGroupByField].Value + "'>";
									rstRecordset.MoveFirst();

									// build the menu for this level
									if (objMenu != null)
									{
										strMenu += BuildMenuItem(objMenu, 3, intQueryNumber, strTableName);
									}
									strPKFieldName = mrsysSystem.Tables[strTableName].PrimaryKeyField.FieldName;

									while(!rstRecordset.EOF && !rstRecordset.BOF)
									{
										if (strFormName.Length > 0)
										{
											strID = mrsysSystem.IdToString(rstRecordset.Fields[strPKFieldName].Value) + ";" + strTableName + ";" + strFormName;
										}
										else
										{
											strID = mrsysSystem.IdToString(rstRecordset.Fields[strPKFieldName].Value) + ";" + strTableName + ";";
										}
										strXML += "<node id='" + strID + "' name='" + XMLEncode(TypeConvert.ToString(rstRecordset.Fields[strDisplayField].Value)) + "'/>";
										rstRecordset.MoveNext();
									}
									strXML += "</node>";
								}
								rstAggregation.MoveNext();
							}
						}
					}
					else
					{
						if (rstRecordset.RecordCount > 0)
						{
							rstRecordset.MoveFirst();

							// build the menu for this level
							if (objMenu != null)
							{
								strMenu += BuildMenuItem(objMenu, 2, intQueryNumber, strTableName);
							}
							strPKFieldName = TypeConvert.ToString(mrsysSystem.Tables[strTableName].PrimaryKeyField.FieldName);

							while(!rstRecordset.EOF && !rstRecordset.BOF)
							{
								if (strFormName.Length > 0)
								{
									strID = mrsysSystem.IdToString(rstRecordset.Fields[strPKFieldName].Value) + ";" + strTableName + ";" + strFormName;
								}
								else
								{
									strID = mrsysSystem.IdToString(rstRecordset.Fields[strPKFieldName].Value) + ";" + strTableName + ";";
								}
								strXML += "<node id='" + strID + "' name='" + XMLEncode(TypeConvert.ToString(rstRecordset.Fields[strDisplayField].Value)) + "'/>";
								rstRecordset.MoveNext();
							}
						}
					}
					strXML += "</node>";
					intQueryNumber++;
				}
				strXML += "</nodes>";

				strXML += "<menus>" + strMenu + "</menus>";
				strXML += "</List>";

				return strXML;
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This Function is designed to return the string contiaing the menu options for this level
		/// Called by:
		/// </summary>
		// intLevel = the level we are on
		// intQueryNumber = the number of the query we are on - start with a 0
		// strTableName = the name of the table
		/// <returns>a string containing the menu xml
		///</returns>
		/// <history>
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		/// </history>
		private string BuildMenuItem(XmlNode objMenu, int intLevel, int intQueryNumber, string strTableName)
		{
			try
			{
				XmlNodeList objMenus = objMenu.SelectNodes("./menu[@level='" + intLevel + "']/item");
				string strMenu = string.Empty;
				if (objMenus.Count > 0)
				{
					strMenu = "<menu level='" + intLevel + "' nodeNumber='" + intQueryNumber + "'>";
				}

				foreach(XmlNode objNode in objMenus)
				{
					string strEvent = objNode.Attributes.GetNamedItem("event").InnerText;
					string strName = objNode.Attributes.GetNamedItem("name").InnerText;

					// make sure the user has access to the events
					EnumTablePermissions enuTblPerm = mrsysSystem.UserProfile.get_TablePermissions(strTableName);
					if (strEvent.ToUpper() == "ADD")
					{
						if (enuTblPerm >= EnumTablePermissions.rtpNew)
						{
							strMenu += "<item name='" + strName + "' event='" + strEvent + "'/>";
						}
					}
					else if (strEvent.ToUpper() == "DELETE")
					{
						if (enuTblPerm >= EnumTablePermissions.rtpDelete)
						{
							strMenu += "<item name='" + strName + "' event='" + strEvent + "'/>";
						}
					}
					else if (strEvent.ToUpper() == "EDIT")
					{
						object formId = mrsysSystem.UserProfile.get_DefaultFormId(mrsysSystem.Tables[strTableName].TableId);
						if (formId is Array)
						{
							strMenu += "<item name='" + strName + "' event='" + strEvent + "'/>";
						}
					}
					else
					{
						strMenu += "<item name='" + strName + "' event='" + strEvent + "'/>";
					}
				}
				if (strMenu.Length > 0)
				{
					strMenu += "</menu>";
				}

				return strMenu;

			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}

		/// <summary>
		/// This function is used to create an ADO connection to the ED database of the system
		/// </summary>
		/// NOTE: THIS CONNECTION WILL ONLY BE USED FOR READING DATA!
		/// <param name="objConnectionED">ADO connection to be instantiated</param>
		/// <returns></returns>
		// bStatus - true if successful
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		protected virtual Connection CreateED_ADOConnection()
		{
			Connection objConnectionED = null;
			string strED_Name = mrsysSystem.EnterpriseString;

			if (strED_Name.Length > 0)
			{
				try
				{
					objConnectionED = new Connection();
					objConnectionED.Open(strED_Name, "", "", -1);

					return objConnectionED;
				}
				catch
				{
					// do nothing as we'll return null at the bottom	
				}
				//if (Information.Err().Number != 0 && Information.Err().Number == -2147467259)
				//{
				//	objConnectionED = null;
				//}
			}
			return null;
		}

		/// <summary>
		/// This function is used to get a recordset using an SQL statement
		/// </summary>
		/// NOTE: THIS CONNECTION WILL ONLY BE USED FOR READING DATA AND NEVER FOR WRITING!
		/// <param name="strSQL">SQL Statement string</param>
		/// <param name="objConnectionED">ADO connection to be instantiated</param>
		/// <returns></returns>
		// An ADO RecordSet
		/// Revision#	Date		Author		Description
		/// 3.8.0.0		5/2/2006	dschaffer	Converted to .Net C# code.
		public virtual Recordset GetRecordsetByCustomQuery(string strSQL, Connection objConnectionED)
		{
			try
			{
				Recordset rstRecordset = new Recordset();

				rstRecordset.Open(
							strSQL, 
							objConnectionED, 
							CursorTypeEnum.adOpenKeyset, 
							LockTypeEnum.adLockReadOnly,
							-1);

				return rstRecordset;
			}
			catch(Exception exc)
			{
				throw new PivotalApplicationException(exc.Message, exc, mrsysSystem);
			}
		}
	}
}
