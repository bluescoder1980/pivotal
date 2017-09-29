//
// $Workfile: EnvisionIntegration_Inv.cs$
// $Revision: 5$
// $Author: RYong$
// $Date: Tuesday, January 22, 2008 3:42:39 PM$
//
// Copyright © Pivotal Corporation
//

using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// The ASR Class for Envision Integration
    /// This partial class file contains logics for the inventory integration, which include 
    /// communities, releases, plans, products (options), option rules and rooms.  
    ///
    /// This is a unidirectional sync from Pivotal to Envision.  Records are sent via 
    /// Ftp or web services.  The System table determines the transport type.  The class uses 
    /// the Env_Sync table to track the changes to send to Envision.
    ///
    /// Envision doesn’t support wildcarding, therefore this class translates Pivotal’s 
    /// wildcarded plans and option assignments into many Envision plan and option assignment records.
    /// </summary>
    public partial class EnvisionIntegration : IRAppScript
    {
        public enum TransportType
        {
            Ftp,
            WebService
        }

        public enum ProductCreationLevel
        {
            Corporate,
            Region,
            Division
        }

        const string LocationNumberXml = "LocationNumber";
        const string LocationLevelXml = "LocationLevel";
        const string DateGeneratedXml = "DateGenerated";
        const string BuilderXml = "Builder";
        const string NHTNumberXml = "NHTNumber";
        const string OrganizationXml = "Organization";
        const string InventoryXml = "Inventory";
        const string IntegrationKeyXml = "IntegrationKey";
        const string DeactivateXml = "Deactivate";
        const string NameXml = "Name";
        const string OrganizationOptionsXml = "OrganizationOptions";
        


        private TransportType transportType;    //Stores the transport type value from the System table.
        private string ftpFile;                 //Stores the Ftp file & path. 
        private LocationReferenceType optionCreationLevel;  //Organization level where option definitions are created.
        private Envision.OptionsManager.OptionsManagerService optionsManagerService;  // Envision web service reference.
        private SyncProxy syncProxy;            //Sync class proxy to create and edit Env_Sync records.
        private Recordset20 syncReleaseRst;
        private Recordset20 syncCommunityRst;

        /// <summary>
        /// XmlWriter object to write to the Ftp file.
        /// </summary>
        protected XmlWriter ftpWriter;


        /// <summary>
        /// Update community delegate for both Ftp and web services transports. 
        /// If it is for Ftp, then the returned OrganizationTypeInventory instance is used to attach to the caller's division object.
        /// If it is for web services, the method sends the community to Envision along with the division location reference.
        /// </summary>
        /// <param name="communityId">Indicates the pivotal Id of this community.</param>
        /// <returns>Instance of OrganizationTypeInventory.  Used if the transport is ftp.</returns>
        public delegate OrganizationTypeInventory UpdateCommunity(object communityId);
        /// <summary>
        /// Update releases delegate for both Ftp and web services transports. 
        /// If it is for Ftp, then the returned OrganizationTypeInventory is used to attach to the caller's community object.
        /// If it is for web services, the method sends the release to Envision along with the community location reference.
        /// </summary>
        /// <param name="releaseId">Indicates the pivotal Id of this release.</param>
        /// <returns>An instance of OrganizationTypeInventory.  Used if the transport is ftp.</returns>
        public delegate InventoryType UpdateRelease(object releaseId);
        /// <summary>
        /// Update plan assignment delegate for both Ftp and web services transports. 
        /// If it is for Ftp, then the returned OrganizationTypeInventory is used to attach to the caller's release object.
        /// If it is for web services, the method sends the plan to Envision along with the release location reference.
        /// </summary>
        /// <param name="planId">Indicates the pivotal Id of this plan.</param>
        /// <param name="currentContextReleaseId">Current Release Id.</param>
        /// <returns>An instance of OrganizationTypeInventory.  Used if the transport is ftp.</returns>
        public delegate InventoryType UpdatePlan(object planId, object currentContextReleaseId);
        /// <summary>
        /// Update product/option delegate for both Ftp and web services transports. 
        /// If it is for Ftp, then the returned DesignOptionType[] is used to attach to the organization's OrganizationOptions member.
        /// If it is for web services, the method sends the option creations to Envision along with the location reference pointing to the organization.
        /// </summary>
        /// <param name="optionRst">Recordset of the option.</param>
        /// <returns>An array of instances of DesignOptionType.  Used if the transport is ftp.</returns>        
        public delegate DesignOptionType UpdateOption(Recordset optionRst);
        /// <summary>
        /// Update product/option assignments delegate for both Ftp and web services transports. 
        /// If it is for Ftp, then the returned OptionAssignmentType[] is used to attach to the caller's plan object.
        /// If it is for web services, the method sends the option assignments to Envision along with the plan assignment location reference.
        /// </summary>
        /// <param name="planId">Indicates the pivotal Id of this plan assignment.</param>
        /// <param name="currentContextReleaseRst">Recordset of the current release context for the plan assignment.</param>
        /// <returns>An array of instances of OptionAssignmentType.  Used if the transport is ftp.</returns>        
        public delegate OptionAssignmentType[] UpdateOptionAssignments(object planId, Recordset20 currentContextReleaseRst);
        /// <summary>
        /// Update intersection rules delegate for both Ftp and web services transports.
        /// If it is for Ftp, then the returned InventoryTypeIntersectionRule[] is used to attach to the caller's plan object.
        /// If it is for web services, the method sends the intersection rules to Envision along with the plan assignment location reference.
        /// </summary>
        /// <param name="hardRuleRst">Recordset of the current hard rule record.</param>
        /// <param name="planAssignmentRst">Indicates the recordset of this plan assignment.</param>
        /// <param name="currentContextReleaseRst">Recordset of the current release context for the plan assignment.</param>
        /// <param name="softDeactivate">Indicates to deactivate the rule if no longer applicable to the plan inventory.</param>
        /// <returns>An array of instances of InventoryTypeIntersectionRule.  Used if the transport is Ftp.</returns>
        public delegate InventoryTypeIntersectionRule UpdateIntersectionRule(Recordset20 hardRuleRst, Recordset20 planAssignmentRst, Recordset20 currentContextReleaseRst, bool softDeactivate);
        /// <summary>
        /// Update rooms delegate for both Ftp and web services transports. 
        /// If it is for Ftp, then the returned RoomType[] is used to attach to the caller's plan assignment object.
        /// If it is for web services, the method sends the rooms to Envision along with the plan assignment location reference.
        /// </summary>
        /// <param name="planAssignmentId">Indicates the pivotal Id of this room.</param>
        /// <param name="planId">Plan Division Product Id</param>
        /// <param name="currentContextReleaseRst">Recordset of the current Release.</param>
        /// <returns>An array of instances of RoomType.  Used if the transport is ftp.</returns>
        public delegate RoomType[] UpdateRooms(object planAssignmentId, object planId, Recordset20 currentContextReleaseRst);
        /// <summary>
        /// Update room product/option assignments delegate for both Ftp and web services transports. 
        /// If it is for Ftp, then the returned OptionAssignmentType[] is used to attach to the caller's plan object.
        /// If it is for web services, the method sends the option assignments to Envision along with the plan assignment location reference.
        /// </summary>
        /// <param name="locationId">Location Id of the room.</param>
        /// <param name="planAssignmentId">Neighborhood Product Id of the plan assignment.</param>
        /// <param name="currentContextReleaseRst">Indicates the current release context for the plan assignment.  Do not change cursor position!</param>
        /// <returns>An array of instances of OptionAssignmentType.  Used if the transport is ftp.</returns>        
        public delegate OptionAssignmentType[] UpdateRoomOptionAssignments(object locationId, object planAssignmentId, Recordset20 currentContextReleaseRst);



        /// <summary>
        /// Top level inventory update method to send inventories to Envision.  This method is called by Pivotal Script Service.
        /// </summary>        
        /// <param name="arrDivisionIds">Array of divisions to setup.  Null if this function is not called for the 
        /// <param name="sendFtpFile">Ftp file name</param>
        /// first division inventory setup.</param>
        /// <returns>Number of regions with inventory changes to send.</returns>        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)")]
        public virtual int SendInventoriesToEnvision(object[] arrDivisionIds, bool sendFtpFile)
        
   {
            const string SendInventoryOverlap = "Send Inventory Overlap";
            const string InventorySyncFinished = "Inventory Sync Finished";
            const string InventorySyncStarted = "Inventory Sync Started";
            string fileFullPath = string.Empty;

            OrganizationType organization;

            //throw exception is Send Inventories is already running
            if (sendInventoryIsRunning)
                throw new PivotalApplicationException((string)this.LangDictionary.GetText(SendInventoryOverlap));
            
            DateTime start = DateTime.Now;

            try
            {
                sendInventoryIsRunning = true;  //set running flag

                Builder builder;  //The ultimate Builder instance to serialize and Ftp'd to Envision.

                Log.WriteEvent((string)this.LangDictionary.GetText(InventorySyncStarted));
                  
                transportType = this.Config.EnvisionTransportType;


                ftpFile = string.Empty;


                //Initialize the options manager web serive reference once to reduce the overhead of multiple calls.
                if (transportType == TransportType.WebService)
                {

                    // Setup options manager web service.  Do this once so web services calls are made faster.
                    optionsManagerService = new Envision.OptionsManager.OptionsManagerService(this);
                    optionsManagerService.AuthHeaderValue = new Envision.OptionsManager.AuthHeader();
                    optionsManagerService.AuthHeaderValue.UserName = this.Config.EnvisionWebServiceUserName;
                    optionsManagerService.AuthHeaderValue.Password = this.Config.EnvisionWebServicePassword;
                    optionsManagerService.AuthHeaderValue.NHTBillingNumber = this.Config.EnvisionNHTNumber;
                    optionsManagerService.Url = this.Config.EnvisionOptionsManagerWebServiceUrl;
                    optionsManagerService.Timeout = this.Config.EnvisionWebServiceTimeout;

                }
                else  //Ftp transport.
                {
                    // Construct the Ftp file path.
                    StringBuilder sb = new StringBuilder();
                    sb.Append("EnvisionOptions_");
                    sb.Append(DateTime.Now.ToString("yyyyMMdd_HHmm"));
                    sb.Append(".xml");
                    ftpFile = sb.ToString();
                    fileFullPath = Config.FtpTempDirectory + ftpFile;

                    ftpWriter = XmlWriter.Create(fileFullPath);

                    ftpWriter.WriteStartElement(BuilderXml);
                    ftpWriter.WriteAttributeString("xmlns", "xsi", null, @"http://www.w3.org/2001/XMLSchema-instance");
                    ftpWriter.WriteAttributeString("xmlns", "xsd", null, @"http://www.w3.org/2001/XMLSchema");

                    ftpWriter.WriteAttributeString(NameXml, this.Config.EnvisionBuilderName);
                    ftpWriter.WriteAttributeString(DateGeneratedXml, start.ToString("yyy-MM-ddTHH:mm:ss"));
                    ftpWriter.WriteAttributeString(NHTNumberXml, this.Config.EnvisionNHTNumber);

                    ftpWriter.WriteStartElement(OrganizationXml);
                    ftpWriter.WriteAttributeString(NameXml, this.Config.EnvisionBuilderName);
                    ftpWriter.WriteAttributeString(LocationNumberXml, this.Config.EnvisionNHTNumber);
                    ftpWriter.WriteAttributeString(LocationLevelXml, LocationLevel.CodeCorporation);

                }


                // The Organization level where products/options can be created.  This is a read-only setting,
                // that's set once per homebuilder.  The value is on the System form.
                optionCreationLevel = (LocationReferenceType)Config.ProductCreationLevel;

                //Construct the top-level builder object and the corporation organization level object.
                builder = new Builder();
                builder.Name = this.Config.EnvisionBuilderName;
                builder.NHTNumber = this.Config.EnvisionNHTNumber;
                builder.DateGenerated = start;
                organization = new OrganizationType();
                organization.Name = this.Config.EnvisionBuilderName;  //Todo: Make builder name and corporate name the same?
                organization.LocationLevel = LocationLevel.CodeCorporation;
                organization.LocationNumber = this.Config.EnvisionNHTNumber;
                builder.Organization = organization;


                //Begin processing inventory records.
                if (optionCreationLevel == LocationReferenceType.Corporate)
                    organization.OrganizationOptions = ProcessOptions(null);

                //Pre-load all the neighborhood sync records upfront to reduce trips to database server.
                syncCommunityRst = this.PivotalDataAccess.GetRecordset(EnvSyncData.QuerySyncRecordsForAllNeighborhoods, 0,
                    EnvSyncData.NeighborhoodTextField, EnvSyncData.RnUpdateCopyField, EnvSyncData.SyncStateField);
                syncCommunityRst.Sort = EnvSyncData.NeighborhoodTextField;

                //Pre-load all the release sync records upfront to reduce trips to database server.
                syncReleaseRst = this.PivotalDataAccess.GetRecordset(EnvSyncData.QuerySyncRecordsForAllRelease, 0,
                    EnvSyncData.ReleaseTextField, EnvSyncData.RnUpdateCopyField, EnvSyncData.SyncStateField);
                syncReleaseRst.Sort = EnvSyncData.ReleaseTextField;

                organization.Organization = ProcessRegions(arrDivisionIds);

                // If some changes to update via Ftp
                if (transportType == TransportType.Ftp)
                {
                    ftpWriter.WriteEndElement(); //Corporation
                    ftpWriter.WriteEndElement(); //Builder
                    ftpWriter.WriteEndDocument();

                    ftpWriter.Flush();
                    ftpWriter.Close();

                    //Only drop the file off to Envision if there are real changes.
                    if ((organization.Organization != null && organization.Organization.GetLength(0) > 0) ||
                    (organization.OrganizationOptions != null && organization.OrganizationOptions.GetLength(0) > 0))
                    {                        

                        TransitionPointParameter transitParams = (TransitionPointParameter)m_rdaSystem.ServerScripts[AppServerRuleData.TransitionPointParameterAppServerRuleName].CreateInstance();
                        transitParams.Construct();
                        transitParams.SetUserDefinedParameter(1, fileFullPath);
                        object parameterList = transitParams.ParameterList;

                        RBaseSystemWriteInproc rdaBaseSystem = new RBaseSystemWriteInproc();
                        rdaBaseSystem.ExecuteServerScript(PivotalSystem.SystemName, PivotalSystem.UserProfile.UserName
                            , PivotalSystem.UserProfile.Password, PivotalSystem.UserProfile.LoginType, PivotalSystem.UserProfile.TimeZone
                            , "PAHB Envision Integration Transactional", Configuration.UpdateTheLastGeneratedFtpFileName, ref parameterList);

                        if (sendFtpFile)
                        {
                            FtpService ftpObject = new FtpService(this);
                            ftpObject.Send(Config.FtpTempDirectory + ftpFile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                throw;
            }
            finally
            {
                sendInventoryIsRunning = false;  // set flag to not running
                syncProxy.CleanUpEnvSyncTable();  //Delete orphan records
                if (optionsManagerService != null) optionsManagerService.Dispose();
                TimeSpan elaps = DateTime.Now.Subtract(start);
                Log.WritePerformance(System.String.Format("Total inventory changes processed in {0} seconds.", elaps.TotalSeconds));
                Log.WriteEvent((string)this.LangDictionary.GetText(InventorySyncFinished));
            }

            //If at least one region is synched, send the number of regions.
            if (organization.Organization != null && organization.Organization.GetLength(0) > 0)
                return organization.Organization.GetLength(0);
            else
                return 0;
            
        }




        #region Processing methods - High level business logics for selecting records to synchronize.

        /// <summary>
        /// Process regions that have at least 1 division set to sync with Envision.
        /// </summary>
        /// <param name="arrDivisionIds">Array of divisions to setup.  Null if this function is not called for the 
        /// first division inventory setup.</param>
        /// <returns>Array of region instances to attach to the caller's corporate instance.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual OrganizationType[] ProcessRegions(object[] arrDivisionIds)
        {
            Recordset regionRst;
            ArrayList regions = new ArrayList();
            OrganizationType iRegion;
            string prevRegionName = string.Empty;

            if (arrDivisionIds == null)
            {
                regionRst = this.PivotalDataAccess.GetRecordset(RegionData.QueryRegionsToSynchronize, 0, RegionData.RegionIdField, RegionData.RegionNameField);
            }
            else
            {
                regionRst = new Recordset();

                regionRst.Fields.Append(DivisionData.RegionIdField + "Text", DataTypeEnum.adVarChar, 255, FieldAttributeEnum.adFldUpdatable);
                regionRst.Fields.Append(RegionData.RegionNameField, DataTypeEnum.adVarChar, 255, FieldAttributeEnum.adFldUpdatable);
                regionRst.Fields.Append(DivisionData.DivisionIdField + "Text", DataTypeEnum.adVarChar, 255, FieldAttributeEnum.adFldUpdatable);
                regionRst.Fields.Append(DivisionData.RegionIdField, DataTypeEnum.adBinary, 8, FieldAttributeEnum.adFldUpdatable);
                regionRst.Fields.Append(DivisionData.DivisionIdField, DataTypeEnum.adBinary, 8, FieldAttributeEnum.adFldUpdatable);
                regionRst.Open(Type.Missing, Type.Missing, CursorTypeEnum.adOpenStatic, LockTypeEnum.adLockOptimistic,
                    (int)CommandTypeEnum.adCmdUnspecified);

                for (int i = 0; i < arrDivisionIds.GetLength(0); i++)
                {
                    regionRst.AddNew(Type.Missing, Type.Missing);
                    regionRst.Fields[DivisionData.DivisionIdField + "Text"].Value = m_rdaSystem.IdToString(arrDivisionIds[i]);
                    regionRst.Fields[DivisionData.DivisionIdField].Value = arrDivisionIds[i];

                    Recordset tempRegionRst = this.PivotalDataAccess.GetRecordset(arrDivisionIds[i], DivisionData.TableName, DivisionData.RegionIdField);
                    tempRegionRst.MoveFirst();
                    regionRst.Fields[DivisionData.RegionIdField].Value = tempRegionRst.Fields[DivisionData.RegionIdField].Value;
                    regionRst.Fields[DivisionData.RegionIdField + "Text"].Value = m_rdaSystem.IdToString(tempRegionRst.Fields[DivisionData.RegionIdField].Value);

                    Recordset tempRegionRst2 = this.PivotalDataAccess.GetRecordset(tempRegionRst.Fields[DivisionData.RegionIdField].Value,
                        RegionData.TableName, RegionData.RegionNameField);
                    tempRegionRst2.MoveFirst();
                    regionRst.Fields[RegionData.RegionNameField].Value = TypeConvert.ToString(tempRegionRst2.Fields[RegionData.RegionNameField].Value);
                    regionRst.Sort = RegionData.RegionNameField;
                }
            }
            if (regionRst.RecordCount > 0)
            {
                regionRst.MoveFirst();
                while (!regionRst.EOF)
                {
                    try
                    {

                        iRegion = new OrganizationType();
                        iRegion.Name = (string)regionRst.Fields[RegionData.RegionNameField].Value;
                        iRegion.LocationLevel = LocationLevel.CodeRegion;
                        iRegion.LocationNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(regionRst.Fields[RegionData.RegionIdField].Value));

                        if (transportType == EnvisionIntegration.TransportType.Ftp)
                        {
                            ftpWriter.WriteStartElement(OrganizationXml);
                            ftpWriter.WriteAttributeString(NameXml, iRegion.Name);
                            ftpWriter.WriteAttributeString(LocationLevelXml, iRegion.LocationLevel);
                            ftpWriter.WriteAttributeString(LocationNumberXml, iRegion.LocationNumber);
                        }

                        if (optionCreationLevel == LocationReferenceType.Region)
                            iRegion.OrganizationOptions = ProcessOptions(regionRst.Fields[RegionData.RegionIdField].Value);

                        if (arrDivisionIds == null)
                        {
                            iRegion.Organization = ProcessDivisions(regionRst.Fields[RegionData.RegionIdField].Value, null);
                            if ((iRegion.Organization != null && iRegion.Organization.GetLength(0) > 0) ||
                                (iRegion.OrganizationOptions != null && iRegion.OrganizationOptions.GetLength(0) > 0))
                            {
                                regions.Add(iRegion);
                            }
                        }
                        else
                        {
                            //Division setup being called.
                            prevRegionName = iRegion.Name;
                            object currentRegionId = regionRst.Fields[RegionData.RegionIdField].Value;
                            ArrayList arrDivsForRegion = new ArrayList();
                            while (!regionRst.EOF && TypeConvert.ToString(regionRst.Fields[RegionData.RegionNameField].Value) == prevRegionName)
                            {
                                arrDivsForRegion.Add(regionRst.Fields[DivisionData.DivisionIdField].Value);
                                prevRegionName = TypeConvert.ToString(regionRst.Fields[RegionData.RegionNameField].Value);
                                regionRst.MoveNext();
                            }

                            iRegion.Organization = ProcessDivisions(currentRegionId, arrDivsForRegion);
                            if ((iRegion.Organization != null && iRegion.Organization.GetLength(0) > 0) ||
                                (iRegion.OrganizationOptions != null && iRegion.OrganizationOptions.GetLength(0) > 0))
                            {
                                regions.Add(iRegion);
                            }
                            continue;
                        }

                    }
                    catch (PivotalApplicationException ex)
                    {
                        if (ex.Number == (int)ErrorNumber.ErrorWebMethodCall)
                            throw;  // Serious transport error.  Halt and bubble up.
                        else
                            Log.WriteException(ex);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteException(ex);
                        // Don't bubble up the exception.  Just move on to the next region without stalling.
                    }
                    finally
                    {
                        if (transportType == EnvisionIntegration.TransportType.Ftp)
                            ftpWriter.WriteEndElement(); // Region
                    }

                    regionRst.MoveNext();

                }
                regionRst.Close();
                return (OrganizationType[])regions.ToArray(typeof(OrganizationType));
            }


            return null;
        }


        /// <summary>
        /// Process active dividions that are set to sync with Envision.  
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="arrDivsForRegion">Divisions to be set up under the specified region.</param>
        /// <returns>Array of division instances to attach to the caller's region instance.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual OrganizationType[] ProcessDivisions(object regionId, ArrayList arrDivsForRegion)
        {
            Recordset divisionRst;
            OrganizationType iDivision;
            ArrayList divisions = new ArrayList();

            if (arrDivsForRegion == null)
            {
                divisionRst = this.PivotalDataAccess.GetRecordset(DivisionData.QueryDivisionsToSynchronizeForRegion, 1, regionId, DivisionData.DivisionIdField, DivisionData.NameField);
            }
            else
            {
                divisionRst = new Recordset();
                divisionRst.Fields.Append(DivisionData.DivisionIdField, DataTypeEnum.adBinary, 8, FieldAttributeEnum.adFldUpdatable);
                divisionRst.Fields.Append(DivisionData.NameField, DataTypeEnum.adVarChar, 255, FieldAttributeEnum.adFldUpdatable);
                divisionRst.Open(Type.Missing, Type.Missing, CursorTypeEnum.adOpenStatic, LockTypeEnum.adLockOptimistic,
                    (int)CommandTypeEnum.adCmdUnspecified);
                for (int i = 0; i < arrDivsForRegion.Count; i++)
                {
                    divisionRst.AddNew(Type.Missing, Type.Missing);
                    divisionRst.Fields[DivisionData.DivisionIdField].Value = arrDivsForRegion[i];
                    Recordset tempDiv = this.PivotalDataAccess.GetRecordset(arrDivsForRegion[i], DivisionData.TableName, DivisionData.NameField);
                    tempDiv.MoveFirst();
                    divisionRst.Fields[DivisionData.NameField].Value = TypeConvert.ToString(tempDiv.Fields[DivisionData.NameField].Value);
                }
            }

            if (divisionRst.RecordCount > 0)
            {
                divisionRst.MoveFirst();

                while (!divisionRst.EOF)
                {
                    try
                    {
                        iDivision = new OrganizationType();
                        iDivision.Name = (string)divisionRst.Fields[DivisionData.NameField].Value;
                        iDivision.LocationLevel = LocationLevel.CodeDivision;
                        iDivision.LocationNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(divisionRst.Fields[DivisionData.DivisionIdField].Value));

                        if (transportType == EnvisionIntegration.TransportType.Ftp)
                        {
                            ftpWriter.WriteStartElement(OrganizationXml);
                            ftpWriter.WriteAttributeString(NameXml, iDivision.Name);
                            ftpWriter.WriteAttributeString(LocationLevelXml, iDivision.LocationLevel);
                            ftpWriter.WriteAttributeString(LocationNumberXml, iDivision.LocationNumber);
                        }

                        //If administrator sets to create options at the Division level
                        if (optionCreationLevel == LocationReferenceType.Division)
                        {
                            iDivision.OrganizationOptions = ProcessOptions(divisionRst.Fields[DivisionData.DivisionIdField].Value);

                        }
                        iDivision.Inventory = ProcessCommunities(divisionRst.Fields[DivisionData.DivisionIdField].Value);

                        if ((iDivision.OrganizationOptions != null && iDivision.OrganizationOptions.GetLength(0) > 0) || (iDivision.Inventory != null && iDivision.Inventory.GetLength(0) > 0))
                        {
                            divisions.Add(iDivision);
                        }
                    }
                    catch (PivotalApplicationException ex)
                    {
                        if (ex.Number == (int)ErrorNumber.ErrorWebMethodCall)
                            throw;  // Serious transport error.  Halt and bubble up.
                        else
                            Log.WriteException(ex);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteException(ex);
                        // Don't bubble up the exception.  Just move on to the next division without stalling.
                    }
                    finally
                    {
                        if (transportType == EnvisionIntegration.TransportType.Ftp)
                        {
                            ftpWriter.WriteEndElement(); // Division
                        }
                    }

                    divisionRst.MoveNext();
                }

                divisionRst.Close();
                return (OrganizationType[])divisions.ToArray(typeof(OrganizationType));
            }
            return null;
        }


        /// <summary>
        /// Process communities that are open/closed/inactive, but exclude Market Level Neighborhood.
        /// </summary>
        /// <param name="divisionId">Parent division id of these communities.</param>
        /// <returns>Array of community instances to attach to the caller's division instance</returns>
        /// <remarks>5.9.0.0   RYong   Fix bugs with Xml tags and field mapping for community.</remarks>
        /// <remarks>5.9.0.2   RYong   Fix bugs 65087 - Skip child records if community is inactive or closed.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual OrganizationTypeInventory[] ProcessCommunities(object divisionId)
        {
            Recordset20 communityRst;
            ArrayList communities = new ArrayList();
            UpdateCommunity updateCommunity;  //Delegate instance for community update

            //Define transport delegate
            if (transportType == TransportType.WebService)
                updateCommunity = new UpdateCommunity(UpdateCommunityWs);
            else
                updateCommunity = new UpdateCommunity(UpdateCommunityFtp);

            //Load division's neighborhoods 
            communityRst = this.PivotalDataAccess.GetRecordset(NeighborhoodData.QueryCommunitiesToSynchronizeForDivision, 1, divisionId,
                NeighborhoodData.NameField, NeighborhoodData.StatusField,   
                NeighborhoodData.RnUpdateField, NeighborhoodData.RnDescriptorField, NeighborhoodData.InactiveField);

            if (communityRst.RecordCount > 0)
            {
                OrganizationTypeInventory iCommunity;
                bool communityChanges;
                InventoryType[] releases = null;
                communityRst.MoveFirst();
                while (!communityRst.EOF)
                {
                    try
                    {
                        if (transportType == EnvisionIntegration.TransportType.Ftp)
                        {
                            ftpWriter.WriteStartElement(InventoryXml);
                            ftpWriter.WriteAttributeString(NameXml, TypeConvert.ToString(communityRst.Fields[NeighborhoodData.NameField].Value));
                            ftpWriter.WriteAttributeString(LocationLevelXml, EnvisionIntegration.LocationLevel.CodeCommunity.ToUpper());
                            ftpWriter.WriteAttributeString(LocationNumberXml, BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(communityRst.Fields[NeighborhoodData.NeighborhoodIdField].Value)));
                            ftpWriter.WriteAttributeString(IntegrationKeyXml, BuilderBase.GetIntegrationKey(LocationReferenceType.Community, communityRst.Fields[NeighborhoodData.NeighborhoodIdField].Value, m_rdaSystem));
                            if (TypeConvert.ToBoolean(communityRst.Fields[NeighborhoodData.InactiveField].Value))
                            {
                                ftpWriter.WriteAttributeString(DeactivateXml, "1");
                            }
                        }

                        communityChanges = true;  //initialize to true

                        // Compare with sync records to see if there are changes
                        syncCommunityRst.Filter = System.String.Format("{0} = '{1}'", EnvSyncData.NeighborhoodTextField, string.Format("{0}", Convert.ToInt64(m_rdaSystem.IdToString(communityRst.Fields[NeighborhoodData.NeighborhoodIdField].Value), 16)));
                        if (syncCommunityRst.RecordCount > 0)
                        {
                            syncCommunityRst.MoveFirst();
                            if (TypeConvert.ToByte(syncCommunityRst.Fields[EnvSyncData.SyncStateField].Value) == (byte)1 && //Status Success
                                m_rdaSystem.EqualIds(syncCommunityRst.Fields[EnvSyncData.RnUpdateCopyField].Value, communityRst.Fields[NeighborhoodData.RnUpdateField].Value))
                            {
                                communityChanges = false;
                            }
                        }
                        else if (!communityRst.Fields[NeighborhoodData.StatusField].Value.Equals(NeighborhoodData.StatusOpen))
                        {
                            // If the community is not open and has no sync record, simply don't sync this community.
                            communityChanges = false;
                        }

                        //If there exists changes in the community record itself, update via Ftp or web service.
                        //If no changes, check secondary records.
                        if (communityChanges)
                        {
                            iCommunity = (OrganizationTypeInventory)updateCommunity(communityRst.Fields[NeighborhoodData.NeighborhoodIdField].Value);
                            if (iCommunity != null)
                            {
                                syncProxy.SetNeighborhoodState(communityRst.Fields[NeighborhoodData.NeighborhoodIdField].Value,
                                    (byte[])communityRst.Fields[NeighborhoodData.RnUpdateField].Value,
                                    ftpFile);
                                communities.Add(iCommunity);

                                //If community is open, then process release
                                if (communityRst.Fields[NeighborhoodData.StatusField].Value.Equals(NeighborhoodData.StatusOpen))
                                    iCommunity.Inventory = ProcessReleases(communityRst.Fields[NeighborhoodData.NeighborhoodIdField].Value);
                            }
                        }
                        else if (communityRst.Fields[NeighborhoodData.StatusField].Value.Equals(NeighborhoodData.StatusOpen))
                        { // Community has no changes.  Process releases and see if they have changes.
                            releases = ProcessReleases(communityRst.Fields[NeighborhoodData.NeighborhoodIdField].Value);

                            if (releases != null && releases.Length > 0) //Has release changes.
                            {
                                iCommunity = (OrganizationTypeInventory)updateCommunity(communityRst.Fields[NeighborhoodData.NeighborhoodIdField].Value);
                                iCommunity.Inventory = releases;
                                communities.Add(iCommunity);
                            }
                        }
                    }
                    catch (PivotalApplicationException ex)
                    {
                        if (ex.Number == (int)ErrorNumber.ErrorWebMethodCall)
                            throw;  // Serious transport error.  Halt and bubble up.
                        else
                            Log.WriteException(ex);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteException(ex);
                        // Don't bubble up the exception.  Move on to the next community.
                    }
                    finally
                    {
                        if (transportType == EnvisionIntegration.TransportType.Ftp)
                        {
                            ftpWriter.WriteEndElement();  //Community
                        }
                    }
                    communityRst.MoveNext();
                }
            }

            return (OrganizationTypeInventory[])communities.ToArray(typeof(OrganizationTypeInventory));
        }

        /// <summary>
        /// Process releases that are open/closed/inactive.
        /// </summary>
        /// <param name="communityId">The parent community id of the releases.</param>
        /// <returns>Array of release instances to be attached to the caller's community instance.</returns>
        /// <remarks>5.9.0.0   RYong   Fix bugs with Xml tags and field mapping for release.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual InventoryType[] ProcessReleases(object communityId)
        {
            Recordset20 releaseRst;
            ArrayList releases = new ArrayList();

            //Delegate instances for release update
            UpdateRelease updateRelease;
            if (transportType == TransportType.WebService) //Web Service Transport            
                updateRelease = new UpdateRelease(UpdateReleaseWs);
            else                     //Ftp transport
                updateRelease = new UpdateRelease(UpdateReleaseFtp);


            //Load the community's releases.  
            releaseRst = this.PivotalDataAccess.GetRecordset(NBHDPhaseData.QueryReleasesToSynchronizeForCommunity, 1, communityId,
                   NBHDPhaseData.RegionIdField, NBHDPhaseData.DivisionIdField, NBHDPhaseData.NeighborhoodIdField, NBHDPhaseData.StatusField,
                   NBHDPhaseData.RnUpdateField, NBHDPhaseData.RnDescriptorField, NBHDPhaseData.PhaseNameField, NBHDPhaseData.InactiveField);

            if (releaseRst.RecordCount > 0)
            {
                InventoryType iRelease;
                bool releaseChanges;
                InventoryType[] plans;

                releaseRst.MoveFirst();
                while (!releaseRst.EOF)
                {
                    try
                    {

                        if (transportType == EnvisionIntegration.TransportType.Ftp)
                        {
                            ftpWriter.WriteStartElement(InventoryXml);
                            ftpWriter.WriteAttributeString(NameXml, TypeConvert.ToString(releaseRst.Fields[NBHDPhaseData.PhaseNameField].Value));
                            ftpWriter.WriteAttributeString(LocationLevelXml, EnvisionIntegration.LocationLevel.CodeRelease.ToUpper());
                            ftpWriter.WriteAttributeString(LocationNumberXml, BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(releaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value)));
                            ftpWriter.WriteAttributeString(IntegrationKeyXml, BuilderBase.GetIntegrationKey(LocationReferenceType.Release, releaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value, m_rdaSystem));
                            if (TypeConvert.ToBoolean(releaseRst.Fields[NBHDPhaseData.InactiveField].Value))
                            {
                                ftpWriter.WriteAttributeString(DeactivateXml, "1");
                            }
                        }

                        releaseChanges = true;  //Initialize to true
                        syncReleaseRst.Filter = System.String.Format("{0} = '{1}'", EnvSyncData.ReleaseTextField, string.Format("{0}", Convert.ToInt64(m_rdaSystem.IdToString(releaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value), 16)));
                        if (syncReleaseRst.RecordCount > 0)
                        {
                            if (TypeConvert.ToByte(syncReleaseRst.Fields[EnvSyncData.SyncStateField].Value) == (byte)1 && //Status Success
                                m_rdaSystem.EqualIds(syncReleaseRst.Fields[EnvSyncData.RnUpdateCopyField].Value, releaseRst.Fields[NBHDPhaseData.RnUpdateField].Value))
                            {
                                releaseChanges = false;
                            }
                        }
                        else if (!releaseRst.Fields[NBHDPhaseData.StatusField].Value.Equals(NBHDPhaseData.StatusOpen))
                        {
                            // If the release is not open and has no sync record, simply don't sync this release.
                            releaseChanges = false;
                        }

                        //If there exists changes in the release record itself, update via Ftp or web service.
                        //If no changes, check secondary records.
                        if (releaseChanges)
                        {
                            iRelease = (InventoryType)updateRelease(releaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value);
                            if (iRelease != null)
                            {
                                syncProxy.SetReleaseState(releaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value,
                                    (byte[])releaseRst.Fields[NBHDPhaseData.RnUpdateField].Value,
                                    ftpFile);
                                releases.Add(iRelease);
                                //If release is open, then process child records.
                                if (releaseRst.Fields[NBHDPhaseData.StatusField].Value.Equals(NBHDPhaseData.StatusOpen))
                                    iRelease.Inventory = ProcessPlanAssignments(releaseRst);
                            }
                        }
                        else if (releaseRst.Fields[NBHDPhaseData.StatusField].Value.Equals(NBHDPhaseData.StatusOpen))
                        {   // No changes to the open release.  Process plan assignments and see if they have changes.
                            plans = ProcessPlanAssignments(releaseRst);
                            if (plans != null && plans.Length > 0) //Has secondary changes.
                            {
                                if (transportType == TransportType.Ftp)
                                {
                                    //iRelease = (OrganizationTypeInventory)updateRelease(releaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value);
                                    iRelease = (InventoryType)updateRelease(releaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value); //fix casting.
                                    iRelease.Inventory = plans;
                                    releases.Add(iRelease);
                                }
                                else
                                { //Web Service - Add a dummy release object to indicate changes.
                                    releases.Add(new InventoryType());
                                }
                            }
                        }
                    }
                    catch (PivotalApplicationException ex)
                    {
                        if (ex.Number == (int)ErrorNumber.ErrorWebMethodCall)
                            throw;  // Serious transport error.  Halt and bubble up.
                        else
                            Log.WriteException(ex);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteException(ex);
                        // Don't bubble up the exception.  Move on to the next release.
                    }
                    finally
                    {
                        if (transportType == EnvisionIntegration.TransportType.Ftp)
                        {
                            ftpWriter.WriteEndElement();  //Release
                            Log.WriteInformation(System.String.Format("Processed release {0}.", releaseRst.Fields[NBHDPhaseData.RnDescriptorField].Value));
                        }
                    }

                    releaseRst.MoveNext();
                }
            }
            return (InventoryType[])releases.ToArray(typeof(InventoryType));

        }

        /// <summary>
        /// Process plan assignments for a given release.  
        /// </summary>
        /// <param name="currentContextReleaseRst">Recordset of the current release being processed.  Do not move the cursor position!</param>
        /// <returns></returns>
        /// <remarks>5.9.2.0   RYong   Issue 65536-21364: Fixed bug with sending child records when inactivating a plan inventory.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected virtual InventoryType[] ProcessPlanAssignments(Recordset20 currentContextReleaseRst)
        {            
            Recordset20 planAssignmentRst;
            Recordset20 syncRst;
            ArrayList planAssignments = new ArrayList();
            OptionAssignmentType[] optionAssignments;
            RoomType[] rooms = null;
            InventoryTypeIntersectionRule[] hardRules = null;
            InventoryType iPlan;
            
            object previousPlanId = null;

            object regionId = currentContextReleaseRst.Fields[NBHDPhaseData.RegionIdField].Value;
            object divisionId = currentContextReleaseRst.Fields[NBHDPhaseData.DivisionIdField].Value;
            object neighborhoodId = currentContextReleaseRst.Fields[NBHDPhaseData.NeighborhoodIdField].Value;
            object releaseId = currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value;

            string PlansSort = "Division_Product_Id, WC_Level DESC";


            //Delegate instances for plan update
            UpdatePlan updatePlan;
            if (transportType == TransportType.WebService) //Web Service Transport            
                updatePlan = new UpdatePlan(UpdatePlanAssignmentWs);
            else                     //Ftp transport
                updatePlan = new UpdatePlan(UpdatePlanAssignmentFtp);

            //Retrieve plan assignment records that have changes either in themselves or child records.
            //For performance reason, this query accepts 41 parameters to reduce JOINs in the query.
            planAssignmentRst = this.PivotalDataAccess.GetRecordset(NBHDPProductData.PlanAssignmentsToSynchronizeQuery, 41, 
                  regionId, divisionId, neighborhoodId, releaseId, releaseId,
                  regionId, divisionId, neighborhoodId, releaseId, divisionId, releaseId, divisionId, releaseId, releaseId,
                  regionId, divisionId, neighborhoodId, releaseId, divisionId,
                  regionId, divisionId, neighborhoodId, releaseId, divisionId, releaseId,
                  regionId, divisionId, neighborhoodId, releaseId,
                  regionId, divisionId, neighborhoodId, releaseId, releaseId,
                  regionId, divisionId, neighborhoodId, releaseId, divisionId, divisionId, releaseId,
                  NBHDPProductData.WCLevelField,
                  NBHDPProductData.NBHDPhaseIdField,
                  NBHDPProductData.DivisionProductIdField,
                  NBHDPProductData.RnUpdateField,
                  NBHDPProductData.InactiveField,
                  NBHDPProductData.RnDescriptorField,
                  NBHDPProductData.ProductNameField);

            if (planAssignmentRst.RecordCount > 0)
            {
                //Load all the sync record for plan assignments under this release once at the beginning to avoid too many db calls.
                syncRst = this.PivotalDataAccess.GetRecordset(EnvSyncData.QuerySyncRecordForPlanAssignmentRelease, 1,
                    releaseId,
                    EnvSyncData.RnUpdateCopyField, EnvSyncData.SyncStateField, EnvSyncData.DivisionProductPlanTextField);
                syncRst.Sort = EnvSyncData.DivisionProductPlanTextField;

                planAssignmentRst.Sort = PlansSort;  //Group all the division products together to by pass duplicates.
                planAssignmentRst.MoveFirst();
                bool planAssignmentChanges;
                while (!planAssignmentRst.EOF)
                {
                    try
                    {
                        //No duplicates allowed.  If another plan assignment has the same plan, skip to the next plan assignment.
                        if (m_rdaSystem.EqualIds(previousPlanId, planAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value))
                        {
                            planAssignmentRst.MoveNext();
                            continue;
                        }

                        planAssignmentChanges = true;  //default to true
                        syncRst.Filter = System.String.Format("{0} = '{1}'", EnvSyncData.DivisionProductPlanTextField, string.Format("{0}", Convert.ToInt64(m_rdaSystem.IdToString(planAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value), 16)));
                        if (syncRst.RecordCount > 0)
                        {
                            //syncRst.MoveFirst();
                            if ((byte)syncRst.Fields[EnvSyncData.SyncStateField].Value == (byte)1 && //Status Success
                                m_rdaSystem.EqualIds(syncRst.Fields[EnvSyncData.RnUpdateCopyField].Value, planAssignmentRst.Fields[NBHDPhaseData.RnUpdateField].Value))
                            {
                                planAssignmentChanges = false;
                            }
                        }
                        else if ((bool)planAssignmentRst.Fields[NBHDPProductData.InactiveField].Value)
                        {
                            // If plan assignment is inactive and has no sync record, then ignore it.
                            planAssignmentChanges = false;

                            // Create a Sync record so this plan assignment doesn't get picked up again by the query.
                            syncProxy.SetPlanAssignmentState(planAssignmentRst.Fields[NBHDPProductData.NBHDPProductIdField].Value, releaseId,
                                (byte[])planAssignmentRst.Fields[NBHDPProductData.RnUpdateField].Value,
                                String.Empty);
                        }

                        previousPlanId = planAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value;

                        if (planAssignmentChanges)
                        {
                            iPlan = updatePlan(planAssignmentRst.Fields[NBHDPProductData.NBHDPProductIdField].Value, releaseId);
                            if (iPlan != null)
                            {
                                
                                //Save the sync record for this particular plan assignment in this release.
                                syncProxy.SetPlanAssignmentState(planAssignmentRst.Fields[NBHDPProductData.NBHDPProductIdField].Value, releaseId,
                                    (byte[])planAssignmentRst.Fields[NBHDPProductData.RnUpdateField].Value,
                                    ftpFile);

                                if (!(bool)planAssignmentRst.Fields[NBHDPProductData.InactiveField].Value)
                                {
                                    rooms = ProcessRooms(planAssignmentRst, currentContextReleaseRst);
                                    if (rooms != null) iPlan.Rooms = rooms; 
                                    
                                    optionAssignments = ProcessOptionAssignments(planAssignmentRst, currentContextReleaseRst);
                                    if (optionAssignments != null) iPlan.Assignments = optionAssignments;

                                    hardRules = ProcessIntersectionRules(planAssignmentRst, currentContextReleaseRst);
                                    if (hardRules != null) iPlan.IntersectionRules = hardRules;
                                }

                                if (transportType == EnvisionIntegration.TransportType.Ftp)
                                {
                                    XmlSerializer serializer = new XmlSerializer(typeof(InventoryType));
                                    StringWriter planXmlWriter = new StringWriter(new StringBuilder());
                                    serializer.Serialize(planXmlWriter, iPlan);
                                    string planXml = planXmlWriter.ToString().ToString().Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                                    planXml = planXml.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                                    planXml = planXml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "").Trim();

                                    ftpWriter.WriteRaw(planXml);
                                    ftpWriter.Flush();
                                }

                                // Update: To conserve memory, only the first plan is added to array.  The returned array
                                // notifies caller if at least one change is made.
                                if (planAssignments.Count == 0)
                                    planAssignments.Add(iPlan);
                            }
                        }
                        else if (!(bool)planAssignmentRst.Fields[NBHDPProductData.InactiveField].Value)
                        {  //No changes, but check child records.
                            rooms = ProcessRooms(planAssignmentRst, currentContextReleaseRst);
                            optionAssignments =  ProcessOptionAssignments(planAssignmentRst, currentContextReleaseRst);
                            hardRules = ProcessIntersectionRules(planAssignmentRst, currentContextReleaseRst);

                            // If transport is Ftp, attach option assignments, rooms and hard rules to the plan inventory.
                            if (transportType == TransportType.Ftp)
                            {

                                if (optionAssignments != null && optionAssignments.Length > 0 || rooms != null && rooms.Length > 0
                                    || hardRules != null && hardRules.Length > 0)
                                {
                                    //Has secondary changes.
                                    iPlan = (InventoryType)updatePlan(planAssignmentRst.Fields[NBHDPProductData.NBHDPProductIdField].Value, releaseId);
                                    if ((optionAssignments != null) && optionAssignments.Length > 0) //Has secondary changes.
                                    {
                                        iPlan.Assignments = optionAssignments;
                                    }
                                    if (rooms != null && rooms.Length > 0) //Has room secondary changes.
                                    {
                                        iPlan.Rooms = rooms;
                                    }
                                    if (hardRules != null && hardRules.Length > 0)  //Has hard rule changes.
                                    {
                                        iPlan.IntersectionRules = hardRules;
                                    }

                                    if (transportType == EnvisionIntegration.TransportType.Ftp)
                                    {
                                        XmlSerializer serializer = new XmlSerializer(typeof(InventoryType));
                                        StringWriter planXmlWriter = new StringWriter(new StringBuilder());
                                        serializer.Serialize(planXmlWriter, iPlan);
                                        string planXml = planXmlWriter.ToString().ToString().Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                                        planXml = planXml.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                                        planXml = planXml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "").Trim();

                                        ftpWriter.WriteRaw(planXml);
                                        ftpWriter.Flush();
                                    }

                                    // Update: To conserve memory, only the first plan is added to array.  The returned array
                                    // notifies caller if at least one change is made.
                                    if (planAssignments.Count == 0)
                                        planAssignments.Add(iPlan);
                                }
                            }
                            else //Transport = Web Service
                            {
                                if ( (rooms != null && rooms.Length > 0)
                                      || (optionAssignments != null && optionAssignments.Length > 0)
                                      || (hardRules != null && hardRules.Length > 0)  )
                                {   //Adding iPlan to plan array in order to notify calling functions that a change
                                    //has been made to a child record of this plan.
                                    InventoryBuilder inventoryBuilder = new InventoryBuilder(LocationReferenceType.Plan, planAssignmentRst.Fields[NBHDPProductData.NBHDPProductIdField].Value, 
                                        releaseId, this.PivotalDataAccess, m_rdaSystem, transportType);
                                    iPlan = (InventoryType)inventoryBuilder.ToObject();
                                    planAssignments.Add(iPlan);
                                }
                            }
                        }
                    }
                    catch (PivotalApplicationException ex)
                    {
                        if (ex.Number == (int)ErrorNumber.ErrorWebMethodCall)
                            throw;  // Serious transport error.  Halt and bubble up.
                        else
                            Log.WriteException(ex);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteException(ex);
                    }
                    planAssignmentRst.MoveNext();

                }
            }
            return (InventoryType[])planAssignments.ToArray(typeof(InventoryType));
        }


        /// <summary>
        /// Process option definitions for a particular organization.  An organization can be the corporate,
        /// a region or a division depending on where the option creation level is set to.
        /// </summary>
        /// <param name="organizationId">the region id or the division id if option creation level is set to region or division respectively.  
        /// Null if corporate.</param>
        /// <returns>Array of option instances to attach to the caller's organization instance.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Body"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual DesignOptionType[] ProcessOptions(object organizationId)
        {

            DesignOptionType iOption;
            Recordset optionRst;
            ArrayList options = new ArrayList();
            int optionCount = 0;

            //Delegate instances for option update
            UpdateOption updateOption;

            string InvalidProductCreationLevel = "Invalid Product Creation Level";

            if (transportType == TransportType.WebService)
                //Web Service Transport            
                updateOption = new UpdateOption(UpdateOptionWs);
            else
                //Ftp transport
                updateOption = new UpdateOption(UpdateOptionFtp);

            // Query options based on Option Creation Level which is set on the system form.
            switch (optionCreationLevel)
            {
                case LocationReferenceType.Corporate:
                    optionRst = this.PivotalDataAccess.GetRecordset(DivisionProductData.OptionsToSynchronizeForCorporateQuery, 0
                                        , DivisionProductData.ProductNameField, DivisionProductData.TypeField
                                        , DivisionProductData.InactiveField, DivisionProductData.DescriptionField
                                        , DivisionProductData.SubCategoryIdField, DivisionProductData.ConstructionStageIdField
                                        , DivisionProductData.DivisionIdField, DivisionProductData.RegionIdField
                                        , DivisionProductData.AvailableDateField, DivisionProductData.RemovalDateField
                                        , DivisionProductData.RecommendedPriceField , DivisionProductData.PostCuttOffPriceField
                                        , DivisionProductData.RnDescriptorField, DivisionProductData.RnUpdateField);
                    break;
                case LocationReferenceType.Region:
                    optionRst = this.PivotalDataAccess.GetRecordset(DivisionProductData.OptionsToSynchronizeForRegionQuery, 1, organizationId
                                        , DivisionProductData.ProductNameField, DivisionProductData.TypeField
                                        , DivisionProductData.InactiveField, DivisionProductData.DescriptionField
                                        , DivisionProductData.SubCategoryIdField, DivisionProductData.ConstructionStageIdField
                                        , DivisionProductData.DivisionIdField, DivisionProductData.RegionIdField
                                        , DivisionProductData.AvailableDateField, DivisionProductData.RemovalDateField
                                        , DivisionProductData.RecommendedPriceField , DivisionProductData.PostCuttOffPriceField
                                        , DivisionProductData.RnDescriptorField, DivisionProductData.RnUpdateField);
                    break;
                case LocationReferenceType.Division:
                    optionRst = this.PivotalDataAccess.GetRecordset(DivisionProductData.OptionsToSynchronizeForDivisionQuery, 1, organizationId
                                        , DivisionProductData.ProductNameField, DivisionProductData.TypeField
                                        , DivisionProductData.InactiveField, DivisionProductData.DescriptionField
                                        , DivisionProductData.SubCategoryIdField, DivisionProductData.ConstructionStageIdField
                                        , DivisionProductData.DivisionIdField, DivisionProductData.RegionIdField
                                        , DivisionProductData.AvailableDateField, DivisionProductData.RemovalDateField
                                        , DivisionProductData.RecommendedPriceField, DivisionProductData.PostCuttOffPriceField
                                        , DivisionProductData.RnDescriptorField, DivisionProductData.RnUpdateField);
                    break;
                default:
                    throw new PivotalApplicationException((string)this.LangDictionary.GetText(InvalidProductCreationLevel));
            }

            if (optionRst.RecordCount > 0)
            {
                optionRst.Sort = DivisionProductData.TypeField;
                optionRst.MoveFirst();

                if (transportType==EnvisionIntegration.TransportType.Ftp)
                    ftpWriter.WriteStartElement(OrganizationOptionsXml);
                
                while (!optionRst.EOF)
                {
                    try
                    {
                        // Make web service update call or save as an object depending on transport delegate.
                        iOption = updateOption(optionRst);
                        if (iOption == null)
                        {
                            // If update is not successful, move on to the next option without refreshing the sync record.
                            StringBuilder sb = new StringBuilder();
                            sb.Append("Failed to sync option definition:  ");
                            sb.Append((string)optionRst.Fields[DivisionProductData.RnDescriptorField].Value);
                            sb.Append(".  ");
                            throw new PivotalApplicationException(sb.ToString());
                        }

                        // If it's an option package, retrieve the components' RnUpdate values to update Env_Sync records.
                        object[,] packageComponentUpdate = null;
                        if (iOption.Package != null)
                        {
                            packageComponentUpdate = new object[iOption.Package.Length, 2];
                            for (int i = 0; i < iOption.Package.Length; i++)
                            {
                                packageComponentUpdate[i, 0] = iOption.Package[i].PackageComponentId;
                                packageComponentUpdate[i, 1] = iOption.Package[i].RnUpdate;
                            }
                        }


                        // If update is successful, refresh the sync record to show that changes have been sent.                       
                        syncProxy.SetOptionState(optionRst.Fields[DivisionProductData.DivisionProductIdField].Value,
                            (byte[])optionRst.Fields[DivisionProductData.RnUpdateField].Value,
                            packageComponentUpdate, iOption.OptionRuleUpdate, ftpFile);

                        // Update: To conserve memory, only the first option is added to array.  The returned array
                        // notifies caller if at least one change is made.
                        if (options.Count == 0)
                            options.Add(iOption);

                        // Flush the options to Ftp file on every 1000th record.                       
                        if (transportType == EnvisionIntegration.TransportType.Ftp && optionCount ++ >= 1000)
                        {
                                ftpWriter.Flush();
                                optionCount = 0;
                        }
                    }
                    catch (PivotalApplicationException ex)
                    {
                        if (ex.Number == (int)ErrorNumber.ErrorWebMethodCall)
                            throw;  // Serious transport error.  Halt and bubble up.
                        else
                            Log.WriteException(ex);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteException(ex);
                    }
                    optionRst.MoveNext();
                }

                if (transportType == EnvisionIntegration.TransportType.Ftp)
                    ftpWriter.WriteEndElement(); //OrganizationOptions

                return (DesignOptionType[])options.ToArray(typeof(DesignOptionType));
            }

            return null;
        }


        /// <summary>
        /// Processes the rooms for a given plan assignment.
        /// </summary>
        /// <param name="planAssignmentRst">Recordset of the current plan assignment.</param>
        /// <param name="currentContextReleaseRst">Current Context Release Id of the plan assignment. Do not change cursor position!</param>
        /// <returns>Array of RoomType instances.</returns>
        protected virtual RoomType[] ProcessRooms(Recordset20 planAssignmentRst, Recordset20 currentContextReleaseRst)
        {
            //Delegate instances for plan update
            UpdateRooms updateRooms;
            object planAssignmentId = planAssignmentRst.Fields[NBHDPProductData.NBHDPProductIdField].Value;
            object planId = planAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value;
            object currentContextReleaseId = currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value;

            if (transportType == TransportType.WebService) //Web Service Transport            
            {
                updateRooms = new UpdateRooms(UpdateRoomsWs);
            }
            else                     //Ftp transport
            {
                updateRooms = new UpdateRooms(UpdateRoomsFtp);
            }

            RoomType[] roomTypes = updateRooms(planAssignmentId, planId, currentContextReleaseRst);
            if (roomTypes != null)
            {
                for (int i = 0; i < roomTypes.Length; i++)
                {
                    syncProxy.SetLocationState(roomTypes[i].LocationId, planId, currentContextReleaseId, roomTypes[i].RnUpdateLocation, roomTypes[i].RnUpdateDPLocation, ftpFile);
                    //roomTypes[i].RoomAssignment
                }
            }                    
            
            return roomTypes;

        }

        /// <summary>
        /// Processes room option assignments for a given room (location in pivotal).
        /// </summary>
        /// <param name="locationId">Location Id of the room option assignments.</param>
        /// <param name="planAssignmentId">Pivotal Id of the room.</param>
        /// <param name="planId">Division_Product_Id of the plan assignment.</param>
        /// <param name="currentContextReleaseRst">If the option assignment is wildcarded, pass in the current release that's being processed.  Do not change cursor position!</param>
        /// <returns></returns>
        protected virtual OptionAssignmentType[] ProcessRoomOptionAssignments(object locationId, object planAssignmentId, object planId, Recordset20 currentContextReleaseRst)
        {
            OptionAssignmentType[] roomAssignments;
            //Delegate instances for option assignment updates
            UpdateRoomOptionAssignments UpdateRoomOptionAssignments;

            object currentContextReleaseId = currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value;

            UpdateRoomOptionAssignments = new UpdateRoomOptionAssignments(UpdateRoomOptionAssignmentsFtp);

            roomAssignments = UpdateRoomOptionAssignments(locationId, planAssignmentId, currentContextReleaseRst);

            if (roomAssignments != null && roomAssignments.Length > 0)
            {
                for (int i = 0; i < roomAssignments.Length; i++)
                {
                    syncProxy.SetLocationProductAssignmentState(roomAssignments[i].OptionId, locationId, planId, currentContextReleaseId, roomAssignments[i].RnUpdateRoom, roomAssignments[i].SoftDeactivate, ftpFile);
                }
            }


            return roomAssignments;

        }


        /// <summary>
        /// Processes option assignments for a given plan assignment.
        /// </summary>
        /// <param name="rstPlanAssignment">Recordset of the plan assignment.  Division_Product_Id field must be included.</param>
        /// <param name="currentContextReleaseRst">If the option assignment is wildcarded, pass in the current release that's being processed.  Do not change cursor position!</param>
        /// <returns></returns>
        protected virtual OptionAssignmentType[] ProcessOptionAssignments(Recordset20 rstPlanAssignment, Recordset20 currentContextReleaseRst)
        {
            //Assume that rstPlanAssignment is already pointing to the current plan assignment.
            object planAssignmentId = rstPlanAssignment.Fields[NBHDPProductData.NBHDPProductIdField].Value;

            //Delegate instances for option assignment updates
            UpdateOptionAssignments updateOptionAssignments;
            if (transportType == TransportType.WebService) //Web Service Transport            
                updateOptionAssignments = new UpdateOptionAssignments(UpdateOptionAssignmentsWs);
            else                     //Ftp transport
                updateOptionAssignments = new UpdateOptionAssignments(UpdateOptionAssignmentsFtp);

            OptionAssignmentType[] optionAssignments = updateOptionAssignments(planAssignmentId, currentContextReleaseRst);
            if (optionAssignments != null && optionAssignments.Length > 0)
            {
                object currentContextPlanId = rstPlanAssignment.Fields[NBHDPProductData.DivisionProductIdField].Value;
                for (int i = 0; i < optionAssignments.Length; i++)
                {
                    syncProxy.SetProductAssignmentState(optionAssignments[i].OptionId,
                        currentContextPlanId, currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value, optionAssignments[i].RnUpdateOption, ftpFile, optionAssignments[i].SoftDeactivate);
                    if (optionAssignments[i].RoomId != null && !Convert.IsDBNull(optionAssignments[i].RoomId))
                    {
                        syncProxy.SetLocationProductAssignmentState(optionAssignments[i].OptionId,
                            optionAssignments[i].RoomId, currentContextPlanId, currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value, optionAssignments[i].RnUpdateRoom, optionAssignments[i].Deactivate, ftpFile);
                    }
                }
            }
            return optionAssignments;
        }


        /// <summary>
        /// Processes intersection rules for a given plan assignment.
        /// </summary>
        /// <param name="planAssignmentRst">Recordset of the plan assignment.  Division_Product_Id field must be included.</param>
        /// <param name="currentContextReleaseRst">If the option assignment is wildcarded, pass in the current release that's being processed.  Do not change cursor position!</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Body")]
        protected virtual InventoryTypeIntersectionRule[] ProcessIntersectionRules(Recordset20 planAssignmentRst, Recordset20 currentContextReleaseRst)
        {
            ArrayList intersectionRules = new ArrayList();
            object[,] hardRules;
            UpdateIntersectionRule updateIntersectionRule;  //Delegate instances for intersection rule updates
            object planAssignmentId = planAssignmentRst.Fields[NBHDPProductData.NBHDPProductIdField].Value;   

            object currentContextReleaseId = currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value;

            Recordset20 hardRuleRst = this.PivotalDataAccess.GetRecordset(ProductOptionRuleData.HardRulesToSynchronizeQuery, 2, currentContextReleaseId, planAssignmentId, 
                ProductOptionRuleData.ParentProductIdField, ProductOptionRuleData.ChildProductIdField, ProductOptionRuleData.RnUpdateField, ProductOptionRuleData.RnDescriptorField,
                ProductOptionRuleData.InactiveField);

            Recordset20 softDeactivateHardRuleRst = this.PivotalDataAccess.GetRecordset(ProductOptionRuleData.PreviousHardRulesToSoftDeactivateQuery, 2, currentContextReleaseId, planAssignmentId,
                ProductOptionRuleData.ParentProductIdField, ProductOptionRuleData.ChildProductIdField, ProductOptionRuleData.RnUpdateField, ProductOptionRuleData.RnDescriptorField,
                ProductOptionRuleData.InactiveField);

            if (hardRuleRst.RecordCount> 0 || softDeactivateHardRuleRst.RecordCount > 0 )
            {
                if (transportType == TransportType.WebService)
                    //Web Service Transport            
                    updateIntersectionRule = new UpdateIntersectionRule(UpdateIntersectionRuleWs);
                else
                    //Ftp transport
                    updateIntersectionRule = new UpdateIntersectionRule(UpdateIntersectionRuleFtp);

                SynchronizeHardRules(hardRuleRst, planAssignmentRst, currentContextReleaseRst, updateIntersectionRule, ref intersectionRules, false);
                SynchronizeHardRules(softDeactivateHardRuleRst, planAssignmentRst, currentContextReleaseRst, updateIntersectionRule, ref intersectionRules, true);

                // If exists successful updates, refresh the hard rule sync records.
                if (intersectionRules.Count > 0)
                {
                    hardRules = new object[intersectionRules.Count, 3];
                    int i = 0;
                    const int RULE_ID = 0;
                    const int RN_UPDATE = 1;
                    const int SOFT_DEACTIVATE = 2;
                    foreach (InventoryTypeIntersectionRule rule in intersectionRules)
                    {
                        hardRules[i, RULE_ID] = rule.RuleId;
                        hardRules[i, RN_UPDATE] = rule.RnUpdate;
                        hardRules[i, SOFT_DEACTIVATE] = rule.SoftDeactivate;
                        i++;
                    }
                    syncProxy.SetHardRuleState(hardRules, planAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value, currentContextReleaseId, ftpFile);
                }
            }

            return (InventoryTypeIntersectionRule[])intersectionRules.ToArray(typeof(InventoryTypeIntersectionRule));

        }


        /// <summary>
        /// Helper function to update hard rules via Ftp or Web Service functions.  It's called by ProcessIntersectionRule methods
        /// for both regular updates and soft deactivations.  Soft deactivation is done when an option of the rule is deactivated.
        /// The integration needs to deactivate the rule at the assignment level in Envision, even though the rule is still 
        /// active in Pivotal at the creation level.
        /// </summary>
        /// <param name="hardRuleRst">The recordset of hard rules to synchronize.</param>
        /// <param name="planAssignmentRst">The recordset of the current plan inventory.</param>
        /// <param name="currentContextReleaseRst">The recordset of the current release.</param>
        /// <param name="updateIntersectionRule">The delegate to update Envision via Ftp or web service.</param>
        /// <param name="intersectionRules">Reference parameter.  Arraylist of InventoryTypeIntersectionRules.</param>
        /// <param name="softDeactivate">Designates to soft deactivate the current hard rule recordset.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual void SynchronizeHardRules(Recordset20 hardRuleRst, Recordset20 planAssignmentRst,
            Recordset20 currentContextReleaseRst, UpdateIntersectionRule updateIntersectionRule,
            ref ArrayList intersectionRules, bool softDeactivate)
        {
            InventoryTypeIntersectionRule iRule;
            if (hardRuleRst.RecordCount == 0) return;

            hardRuleRst.MoveFirst();
            while (!hardRuleRst.EOF)
            {
                try
                {
                    iRule = updateIntersectionRule(hardRuleRst, planAssignmentRst, currentContextReleaseRst, softDeactivate);
                    if (iRule == null)
                    {
                        // If update is not successful, move on to the next rule without refreshing the sync record.
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Failed to synchronize intersection rule definition:  ");
                        sb.Append(TypeConvert.ToString(hardRuleRst.Fields[ProductOptionRuleData.RnDescriptorField].Value));
                        sb.Append(".  ");
                        sb.Append(string.Format("Plan = {0}.  Release = {1}.", planAssignmentRst.Fields[NBHDPProductData.ProductNameField].Value, currentContextReleaseRst.Fields[NBHDPhaseData.RnDescriptorField].Value));
                        throw new PivotalApplicationException(sb.ToString());
                    }
                    intersectionRules.Add(iRule);
                }
                catch (PivotalApplicationException ex)
                {
                    if (ex.Number == (int)ErrorNumber.ErrorWebMethodCall)
                        throw;  // Serious transport error.  Halt and bubble up.
                    else
                        Log.WriteException(ex);
                }
                catch (Exception ex)
                {
                    //Logs the non-transport exception, and move on to the next hard rule.
                    Log.WriteException(ex);
                }
                hardRuleRst.MoveNext();
            }
        }



        #endregion


        #region Delegates - Low level methods for constructing Xml objects and making web service calls.

        /// <summary>
        /// Returns the DesignOptionType instance of the option for Xml serialization.
        /// </summary>
        /// <param name="optionRst">Recordset of the option.</param>
        /// <returns>DesignOptionType instance of the design option.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual DesignOptionType UpdateOptionFtp(Recordset optionRst)
        {
            try
            {
                OptionBuilder optionBuilder = new OptionBuilder(optionRst, this.PivotalDataAccess, m_rdaSystem, Config, transportType);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(optionBuilder.ToXML().OuterXml);
                string optionXml = doc.OuterXml.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                optionXml = optionXml.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                ftpWriter.WriteRaw(optionXml);

                return (DesignOptionType)optionBuilder.ToObject();
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                return null;
            }
        }

        /// <summary>
        /// Returns the OrganizationTypeInventory instance of the community for Xml serialization.
        /// </summary>
        /// <param name="communityId">Pivotal Id of the community.</param>
        /// <returns>OrganizationTypeInventory of the community.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual OrganizationTypeInventory UpdateCommunityFtp(object communityId)
        {

            try
            {
                InventoryBuilder inventoryBuilder = new InventoryBuilder(LocationReferenceType.Community, communityId, this.PivotalDataAccess, m_rdaSystem, transportType);
                return (OrganizationTypeInventory)inventoryBuilder.ToObject();
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                return null;
            }
            // set Sync record with communityEditDate

        }

        /// <summary>
        /// Returns the InventoryType instance of the release for Xml serialization.
        /// </summary>
        /// <param name="releaseId">Pivotal Id of the release.</param>
        /// <returns>InventoryType instance of the release.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual InventoryType UpdateReleaseFtp(object releaseId)
        {
            try
            {
                InventoryBuilder inventoryBuilder = new InventoryBuilder(LocationReferenceType.Release, releaseId, this.PivotalDataAccess, m_rdaSystem, transportType);
                return (InventoryType)inventoryBuilder.ToObject();
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                return null;
            }
        }


        /// <summary>
        /// Returns the InventoryType instance of the plan assignment for Xml serialization.
        /// </summary>
        /// <param name="planAssignmentId">Pivotal Id of the plan assignment.</param>
        /// <param name="currentContextReleaseId">If plan assignment is release wildcarded, pass in the current release that's being processed.</param>
        /// <returns>InventoryType instance of the plan assignment.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual InventoryType UpdatePlanAssignmentFtp(object planAssignmentId, object currentContextReleaseId)
        {
            try
            {
                InventoryBuilder inventoryBuilder = new InventoryBuilder(LocationReferenceType.Plan, planAssignmentId, currentContextReleaseId, this.PivotalDataAccess, m_rdaSystem, transportType);
                return (InventoryType)inventoryBuilder.ToObject();
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                return null;
            }
        }

        /// <summary>
        /// Returns an array of OptionAssignmentType instances of option assignments for Xml serialization.
        /// </summary>
        /// <param name="planAssignmentId">Pivotal Id of the option assignment's plan assignment.</param>
        /// <param name="currentContextReleaseRst">If option assignment is release wildcarded, pass in the current release that's being processed.  Do not change cursor position!</param>
        /// <returns>Array of OptionAssignmentTypes.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual OptionAssignmentType[] UpdateOptionAssignmentsFtp(object planAssignmentId, Recordset20 currentContextReleaseRst)
        {
            try
            {
                OptionAssignmentsBuilder optionAssignmentBuilder = new OptionAssignmentsBuilder(planAssignmentId, currentContextReleaseRst, optionCreationLevel, this.PivotalDataAccess, m_rdaSystem, Config.EnvisionNHTNumber,EnvisionIntegration.TransportType.Ftp);
                return (OptionAssignmentType[])optionAssignmentBuilder.ToObject();
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                return null;
            }
        }

        /// <summary>
        /// Returns an array of OptionAssignmentType instances of option assignments for Xml serialization.
        /// </summary>
        /// <param name="locationId">Location Id of the room option assignments.</param>
        /// <param name="planAssignmentId">Pivotal Id of the option assignment's plan assignment.</param>
        /// <param name="currentContextReleaseRst">If option assignment is release wildcarded, pass in the current release that's being processed. Do not change cursor position!</param>
        /// <returns>Array of OptionAssignmentTypes.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual OptionAssignmentType[] UpdateRoomOptionAssignmentsFtp(object locationId, object planAssignmentId, Recordset20 currentContextReleaseRst)
        {
            try
            {
                OptionAssignmentsBuilder optionAssignmentBuilder = new OptionAssignmentsBuilder(locationId, planAssignmentId, currentContextReleaseRst, optionCreationLevel, this.PivotalDataAccess, m_rdaSystem, Config.EnvisionNHTNumber);
                return (OptionAssignmentType[])optionAssignmentBuilder.ToObject();
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                return null;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hardRuleRst">Pivotal hard rule recordset.</param>
        /// <param name="planAssignmentRst">Current plan assignment Id.</param>
        /// <param name="currentContextReleaseRst">Current release recordset.</param>
        /// <param name="softDeactivate">Indicates to deactivate the rule if no longer applicable to the plan inventory.</param>
        /// /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual InventoryTypeIntersectionRule UpdateIntersectionRuleFtp(Recordset20 hardRuleRst, Recordset20 planAssignmentRst, Recordset20 currentContextReleaseRst, bool softDeactivate)
        {
            try
            {
                IntersectionRuleBuilder intersectionRulesBuilder = new IntersectionRuleBuilder(hardRuleRst, planAssignmentRst, currentContextReleaseRst, optionCreationLevel, Config.EnvisionNHTNumber, softDeactivate, m_rdaSystem, TransportType.Ftp);
                return (InventoryTypeIntersectionRule) intersectionRulesBuilder.ToObject();
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                return null;
            }
        }


        /// <summary>
        /// Returns an array of RoomType instances of rooms for XmlSerialization.  
        /// </summary>
        /// <param name="planAssignmentId">Rooms' plan assignment Id.</param>
        /// <param name="currentContextReleaseRst">Recordset of the current release.</param>
        /// <param name="currentContextPlanId">Plan assignment Id.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual RoomType[] UpdateRoomsFtp(object planAssignmentId, object currentContextPlanId, Recordset20 currentContextReleaseRst)
        {
            try
            {
                object currentContextReleaseId = currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value;
                RoomsBuilder roomsBuilder = new RoomsBuilder(planAssignmentId, currentContextReleaseId, this.PivotalDataAccess, m_rdaSystem, transportType);
                Rooms rooms = (Rooms)roomsBuilder.ToObject();

                //Unlike web service, for Ftp, we need to include the room option assignments under the Rooms objects.
                if (rooms != null && rooms.Room.Length > 0)
                {
                    OptionAssignmentType[] roomOptionAssignments = new OptionAssignmentType[0];

                    for (int i = 0; i < rooms.Room.Length; i++)
                    {
                        roomOptionAssignments = ProcessRoomOptionAssignments(rooms.Room[i].LocationId, planAssignmentId, currentContextPlanId, currentContextReleaseRst);
                        if (roomOptionAssignments != null && roomOptionAssignments.Length > 0)
                        {
                            rooms.Room[i].RoomAssignment = roomOptionAssignments;
                        }
                    }
                    return (RoomType[])rooms.Room;
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                return null;
            }
        }


        /// <summary>
        /// Calls the Options web service to synchronize an option.
        /// </summary>
        /// <param name="optionRst">Recordset of the option.</param>
        /// <returns>DesignOptionType instance of the option.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        protected virtual DesignOptionType UpdateOptionWs(Recordset optionRst)
        {
            const string InvalidProductCreationLevel = "Invalid Product Creation Level";
            XmlNode returnedWsXmlNode;
            object optionCreationOrganizationId;
            bool toCreateOption;

            object optionId = optionRst.Fields[DivisionProductData.DivisionProductIdField].Value;

            switch (optionCreationLevel)
            {
                case LocationReferenceType.Corporate:
                    optionCreationOrganizationId = Config.EnvisionNHTNumber;
                    break;
                case LocationReferenceType.Region:
                    optionCreationOrganizationId = optionRst.Fields[DivisionProductData.RegionIdField].Value;
                    break;
                case LocationReferenceType.Division:
                    optionCreationOrganizationId = optionRst.Fields[DivisionProductData.DivisionIdField].Value;
                    break;
                default:
                    throw new PivotalApplicationException((string)this.LangDictionary.GetText(InvalidProductCreationLevel));
            }


            LocationReferenceBuilder locationReferenceBuilder = new LocationReferenceBuilder(optionCreationLevel, optionCreationOrganizationId, Config, m_rdaSystem);
            XmlNode locationReferenceXml = locationReferenceBuilder.ToXML();

            OptionBuilder optionBuilder = new OptionBuilder(optionRst, this.PivotalDataAccess, m_rdaSystem, Config, transportType);
            DesignOptionType optionObject = (DesignOptionType)optionBuilder.ToObject();
            XmlNode optionXml = optionBuilder.ToXML();

            XmlValidation.Option(optionXml);

            Recordset20 syncRst = this.PivotalDataAccess.GetRecordset(EnvSyncData.QuerySyncRecordForOption, 1, optionId, EnvSyncData.RnUpdateCopyField);



            try
            {
                if (syncRst.RecordCount == 0)
                {
                    ///// Create Design Option /////
                    toCreateOption = true;
                    returnedWsXmlNode = optionsManagerService.CreateOption(locationReferenceXml, optionXml);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "120", "130", "2500", "2505", "2510", "2802", "2803", "2804", "2805", "2806", "2807", "2808", "2809",  "2811", "2812", "2901", "2905", "2909", "2910", "2912", "2913", "2914", "2917" }))
                    {
                        Log.WriteException(new PivotalApplicationException(System.String.Format("Failure of CreateOption. optionId:({0}), Response message:{1}", BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(optionId)), returnedWsXmlNode.OuterXml)));
                        return null;
                    }
                    // If Envision says the Option is already there, then send an EditOption command.
                    if (MessageIdsInReturnedXml(returnedWsXmlNode, new string[] { "2813" }))
                    {
                        toCreateOption = false;
                        returnedWsXmlNode = optionsManagerService.EditOption(locationReferenceXml, optionXml);
                        XmlValidation.Output(returnedWsXmlNode);
                        if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "120", "130", "2500", "2505", "2510", "2800", "2801", "2802", "2803", "2804", "2805", "2806", "2807", "2808", "2809", "2811", "2812", "2813", "2901", "2905", "2909", "2910", "2912", "2913", "2914", "2917" }))
                        {
                            Log.WriteException(new PivotalApplicationException(System.String.Format("Failure of EditOption. optionId:({0}), Response message:{1}", BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(optionId)), returnedWsXmlNode.OuterXml)));
                            return null;
                        }
                    }
                    // If Envision says rules are not valid because the child options haven't been synced yet,
                    // then sync the current option for now without sending the rules, but mark the Env_Sync
                    // record outdated so the rules are picked up again in the next round of sync.  This avoids
                    // circular dependencies.
                    if (MessageIdsInReturnedXml(returnedWsXmlNode, new string[] { "2810" }))
                    {
                        foreach (System.Xml.XmlNode node in optionXml.SelectNodes("//Rules"))
                        {
                            node.ParentNode.RemoveChild(node);
                        }
                        if (toCreateOption)
                            returnedWsXmlNode = optionsManagerService.CreateOption(locationReferenceXml, optionXml);
                        else
                            returnedWsXmlNode = optionsManagerService.EditOption(locationReferenceXml, optionXml);

                        // If this is an Elevation option, return null so that the Elevation is synchronize again later.  Note that
                        // this will generate an Event Viewer error.
                        if (TypeConvert.ToString(optionRst.Fields[DivisionProductData.TypeField].Value) == DivisionProductData.TypeFieldChoice.Elevation.ToString())
                            return null;

                        DesignOptionType optionNoRules = (DesignOptionType)optionBuilder.ToObject();
                        optionNoRules.Rules = null;
                        optionNoRules.OptionRuleUpdate = null;
                        return optionNoRules;
                    }
                }
                else if ((bool)(optionRst.Fields[DivisionProductData.InactiveField].Value))
                {
                    ///// Deactivate Design Option /////
                    returnedWsXmlNode = optionsManagerService.DeactivateOption(locationReferenceXml, optionObject.OptionNumber);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "2500", "2505", "2510", "2908", "2909", "2917" }))
                    {
                        Log.WriteException(new PivotalApplicationException(System.String.Format("Failure of DeactivateOption. optionId:({0}), Response message:{1}", BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(optionId)), returnedWsXmlNode.OuterXml)));
                        return null;
                    }
                }
                else
                {
                    ///// Edit Design Option /////
                    toCreateOption = false;
                    returnedWsXmlNode = optionsManagerService.EditOption(locationReferenceXml, optionXml);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "120", "130", "2500", "2505", "2510", "2800", "2801", "2802", "2803", "2804", "2805", "2806", "2807", "2808", "2809", "2811", "2812", "2813", "2905", "2909", "2910", "2912", "2913", "2914", "2917" }))
                    {
                        Log.WriteException(new PivotalApplicationException(System.String.Format("Failure of EditOption. optionId:({0}), Response message:{1}", BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(optionId)), returnedWsXmlNode.OuterXml)));
                        return null;
                    }
                    // If Envision says Option isn't there, then send a CreateOption command.
                    if (MessageIdsInReturnedXml(returnedWsXmlNode, new string[] { "2901" }))
                    {
                        toCreateOption = true;
                        returnedWsXmlNode = optionsManagerService.CreateOption(locationReferenceXml, optionXml);
                        XmlValidation.Output(returnedWsXmlNode);
                        if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "120", "130", "2500", "2505", "2510", "2802", "2803", "2804", "2805", "2806", "2807", "2808", "2809", "2811", "2812", "2901", "2905", "2909", "2910", "2912", "2913", "2914", "2917" }))
                        {
                            Log.WriteException(new PivotalApplicationException(System.String.Format("Failure of CreateOption. optionId:({0}), Response message:{1}", BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(optionId)), returnedWsXmlNode.OuterXml)));
                            return null;
                        }
                    }

                    // If Envision says rules are not valid because the child options haven't been synced yet,
                    // then sync the current option for now without sending the rules, but leave the Env_Sync
                    // record outdated so the rules are picked up again in the next round of sync.  This avoids
                    // circular dependencies.
                    if (MessageIdsInReturnedXml(returnedWsXmlNode, new string[] { "2810" }))
                    {
                        foreach (System.Xml.XmlNode node in optionXml.SelectNodes("//Rules"))
                        {
                            node.ParentNode.RemoveChild(node);
                        }
                        if (toCreateOption)
                            returnedWsXmlNode = optionsManagerService.CreateOption(locationReferenceXml, optionXml);
                        else
                            returnedWsXmlNode = optionsManagerService.EditOption(locationReferenceXml, optionXml);

                        DesignOptionType optionNoRules = (DesignOptionType)optionBuilder.ToObject();
                        optionNoRules.Rules = null;
                        return optionNoRules;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PivotalApplicationException(System.String.Format("Option web method call failed.  {0}", ex.Message), (int)ErrorNumber.ErrorWebMethodCall);
            }

            return (DesignOptionType)optionBuilder.ToObject();

        }

        /// <summary>
        /// Calls the Options Web Service's Inventory methods to update a community.
        /// </summary>
        /// <param name="communityId">Pivotal Id of the community</param>
        /// <returns>Inventory instance of the community.</returns>
        /// Revision  Date       Author  Description
        /// 5.9.0     9/13/2007  RYong   Issue 64787: unable to deactivate inventory.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        protected virtual OrganizationTypeInventory UpdateCommunityWs(object communityId)
        {
            const string CommunityAlreadyExists = "Community Already Exists";
            XmlNode returnedWsXmlNode;

            Recordset20 communityRst = this.PivotalDataAccess.GetRecordset(communityId, NeighborhoodData.TableName, NeighborhoodData.InactiveField, NeighborhoodData.DivisionIdField,
                 NeighborhoodData.RnUpdateField, NeighborhoodData.StatusField);
            communityRst.MoveFirst();

            LocationReferenceBuilder locationReferenceBuilder = new LocationReferenceBuilder(LocationReferenceType.Division, communityRst.Fields[NeighborhoodData.DivisionIdField].Value, Config, m_rdaSystem);
            XmlNode locationReferenceXml = locationReferenceBuilder.ToXML();

            InventoryBuilder inventoryBuilder = new InventoryBuilder(LocationReferenceType.Community, communityId, this.PivotalDataAccess, m_rdaSystem, transportType);
            OrganizationTypeInventory communityObject = (OrganizationTypeInventory)inventoryBuilder.ToObject();
            XmlNode communityXml = inventoryBuilder.ToXML();

            Recordset20 syncRst = this.PivotalDataAccess.GetRecordset(EnvSyncData.SyncForNeighborhoodQuery, 1, communityId, EnvSyncData.RnUpdateCopyField);

            XmlValidation.Inventory(communityXml);

            try
            {
                if (syncRst.RecordCount == 0)
                {
                    ///// Create Community ////
                    returnedWsXmlNode = optionsManagerService.CreateInventory(locationReferenceXml, communityXml);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "130", "2500", "2505", "2510", "2906", "2909", "2910", "2911", "2917" }))
                        return null;
                    //If Envision says community is already there, then send an EditInventory command.
                    if (MessageIdsInReturnedXml(returnedWsXmlNode, new string[] { "200" }))
                    {
                        Log.WriteInformation((string)this.LangDictionary.GetTextSub(CommunityAlreadyExists, new string[] { communityObject.Name }));
                            
                        returnedWsXmlNode = optionsManagerService.EditInventory(locationReferenceXml, communityXml);
                        if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "130", "2500", "2505", 
                            "2510",  "2909", "2910", "2917"}))
                            return null;
                    }
                }
                else if ((bool)(communityRst.Fields[NeighborhoodData.InactiveField].Value) ||
                    (string)communityRst.Fields[NeighborhoodData.StatusField].Value == "Closed")
                {
                    ///// Deactivate Community ////
                    // Bug in Envision's documentation: Replaced Location Name with Location Number as the second parameter.
                    returnedWsXmlNode = optionsManagerService.DeactivateInventory(locationReferenceXml, communityObject.LocationNumber, communityObject.LocationLevel);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "2505", "2510", "2907", "2909", "2917" }))
                        return null;
                }
                else
                {
                    ///// Edit Community /////
                    returnedWsXmlNode = optionsManagerService.EditInventory(locationReferenceXml, communityXml);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "130", "2500", "2505", 
                        "2510",  "2909", "2910", "2917"}))
                        return null;
                    //If Envision says the community is not there, then send a CreateInventory command.
                    if (MessageIdsInReturnedXml(returnedWsXmlNode, new string[] { "2904" }))
                    {
                        Log.WriteInformation("Could not edit community " + communityObject.Name + ".  The community does not exist in Envision.  Sending the create command instead.");
                        returnedWsXmlNode = optionsManagerService.CreateInventory(locationReferenceXml, communityXml);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new PivotalApplicationException(System.String.Format("Community web method call failed.  {0}", ex.Message), (int)ErrorNumber.ErrorWebMethodCall);
            }

            return communityObject;  //objects returned by web service call delegates are ignored.
        }


        /// <summary>
        /// Calls the Options Web Service's Inventory methods to update a release.
        /// </summary>
        /// <param name="releaseId">Pivotal Id of the release.</param>
        /// <returns>InventoryType instance of the release.</returns>
        /// Revision  Date       Author  Description
        /// 5.9.0     9/13/2007  RYong   Issue 64787: unable to deactivate inventory.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        protected virtual InventoryType UpdateReleaseWs(object releaseId)
        {
            const string ReleaseAlreadyInEnvision = "Release Already In Envision";
            XmlNode returnedWsXmlNode;

            Recordset20 releaseRst = this.PivotalDataAccess.GetRecordset(releaseId, NBHDPhaseData.TableName, NBHDPhaseData.InactiveField,
                NBHDPhaseData.NeighborhoodIdField, NBHDPhaseData.RnUpdateField, NBHDPhaseData.StatusField);
            releaseRst.MoveFirst();

            LocationReferenceBuilder locationReferenceBuilder = new LocationReferenceBuilder(LocationReferenceType.Community, releaseRst.Fields[NBHDPhaseData.NeighborhoodIdField].Value, this.Config, m_rdaSystem);
            XmlNode locationReferenceXml = locationReferenceBuilder.ToXML();

            InventoryBuilder inventoryBuilder = new InventoryBuilder(LocationReferenceType.Release, releaseId, this.PivotalDataAccess, m_rdaSystem, transportType);
            InventoryType releaseObject = (InventoryType)inventoryBuilder.ToObject();
            XmlNode releaseXml = inventoryBuilder.ToXML();

            XmlValidation.Inventory(releaseXml);

            Recordset20 syncRst = this.PivotalDataAccess.GetRecordset(EnvSyncData.QuerySyncRecordForRelease, 1, releaseId, EnvSyncData.RnUpdateCopyField);

            try
            {
                if (syncRst.RecordCount == 0)
                {
                    ///// Create Release /////
                    returnedWsXmlNode = optionsManagerService.CreateInventory(locationReferenceXml, releaseXml);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "130", "2500", "2505", "2510", "2906", "2909", "2910", "2911", "2917" }))
                        return null;
                    //If Envision says the release is already there, then send an EditInventory command.
                    if (MessageIdsInReturnedXml(returnedWsXmlNode, new string[] { "200" }))
                    {
                        Log.WriteInformation((string)this.LangDictionary.GetTextSub(ReleaseAlreadyInEnvision, new string[] { releaseObject.Name}));
                        returnedWsXmlNode = optionsManagerService.EditInventory(locationReferenceXml, releaseXml);
                        if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "130", "2500", "2505", 
                            "2510",  "2909", "2910", "2917"}))
                            return null;
                    }
                }
                else if ((bool)(releaseRst.Fields[NBHDPhaseData.InactiveField].Value) ||
                    (string)releaseRst.Fields[NBHDPhaseData.StatusField].Value == "Closed")
                {
                    ///// Deactivate Release /////
                    // Bug in Envision's documentation: Replaced Location Name with Location Number as the second parameter.
                    returnedWsXmlNode = optionsManagerService.DeactivateInventory(locationReferenceXml, releaseObject.LocationNumber, releaseObject.LocationLevel);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "2505", "2510", "2907", "2909", "2917" }))
                        return null;
                }
                else
                {
                    ///// Edit Release /////
                    returnedWsXmlNode = optionsManagerService.EditInventory(locationReferenceXml, releaseXml);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "130", "2500", "2505", 
                        "2510", "2909", "2910", "2917"}))
                        return null;
                    //If Envision says the release isn't there, then send a CreateInventory command.
                    if (MessageIdsInReturnedXml(returnedWsXmlNode, new string[] { "2904" }))
                    {
                        Log.WriteInformation("Could not edit release " + releaseObject.Name + ".  The release does not exist in Envision.  Sending the create command instead.");
                        returnedWsXmlNode = optionsManagerService.CreateInventory(locationReferenceXml, releaseXml);
                    }
                }

                // Todo: write shared function(s) to read messages in returned xml.
                // Todo: If web method update comes back with in line errors, return NULL so sync record isn't updated.
                //if (!InventoryReturnedXmlMessages(returnedWsXmlNode))
                //    return null;
            }
            catch (Exception ex)
            {
                throw new PivotalApplicationException(System.String.Format("Release web method call failed.  {0}", ex.Message), (int)ErrorNumber.ErrorWebMethodCall);
            }

            return releaseObject;  //objects returned by web service call delegates are ignored.
        }



        /// <summary>
        /// Calls the Options Web Service's Inventory methods to update a plan assignment.
        /// </summary>
        /// <param name="planAssignmentId">Pivotal Id of the plan assignment.</param>
        /// <param name="currentContextReleaseId">If plan assignment is release wildcarded, pass in the current release being processed.</param>
        /// <returns>InventoryType instance of the current plan assignment.</returns>
        /// Revision  Date       Author  Description
        /// 5.9.0     9/13/2007  RYong   Issue 64787: unable to deactivate inventory.
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId="System.String.Format(System.String,System.Object)")]
        protected virtual InventoryType UpdatePlanAssignmentWs(object planAssignmentId, object currentContextReleaseId)
        {
            const string PlanAlreadyInEnvision  = "Plan Already In Envision";

            XmlNode returnedWsXmlNode;

            Recordset20 planAssignmentRst = this.PivotalDataAccess.GetRecordset(planAssignmentId, NBHDPProductData.TableName,
                NBHDPProductData.InactiveField, NBHDPProductData.NBHDPProductIdField, NBHDPProductData.DivisionProductIdField);
            planAssignmentRst.MoveFirst();

            LocationReferenceBuilder locationReferenceBuilder = new LocationReferenceBuilder(LocationReferenceType.Release, currentContextReleaseId, this.Config, m_rdaSystem);
            XmlNode locationReferenceXml = locationReferenceBuilder.ToXML();

            InventoryBuilder inventoryBuilder = new InventoryBuilder(LocationReferenceType.Plan, planAssignmentId, currentContextReleaseId, this.PivotalDataAccess, m_rdaSystem, transportType);
            InventoryType planAssignmentObject = (InventoryType)inventoryBuilder.ToObject();
            XmlNode planXml = inventoryBuilder.ToXML();

            XmlValidation.Inventory(planXml);

            Recordset20 syncRst = this.PivotalDataAccess.GetRecordset(EnvSyncData.QuerySyncRecordForPlanAssignment, 2,
                        currentContextReleaseId,
                        planAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value);

            try
            {
                if (syncRst.RecordCount == 0)
                {
                    ///// Create Plan Assignment /////
                    returnedWsXmlNode = optionsManagerService.CreateInventory(locationReferenceXml, planXml);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "130", "2500", "2505", "2510", "2906", "2909", "2910", "2911", "2917" }))
                        return null;
                    //If the plan assignment to be created is already in Envision, then send an edit command instead.
                    if (MessageIdsInReturnedXml(returnedWsXmlNode, new string[] { "200" }))
                    {
                        string releaseName = (string)this.PivotalDataAccess.SqlIndex(NBHDPhaseData.TableName, NBHDPhaseData.PhaseNameField, currentContextReleaseId);
                        Log.WriteInformation((string)this.LangDictionary.GetTextSub(PlanAlreadyInEnvision, new string[] { planAssignmentObject.Name, releaseName }));
                        returnedWsXmlNode = optionsManagerService.EditInventory(locationReferenceXml, planXml);
                        if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "130", "2500", "2505", 
                            "2510",  "2909", "2910", "2917"}))
                            return null;
                    }
                }
                else if ((bool)(planAssignmentRst.Fields[NBHDPProductData.InactiveField].Value))
                {
                    ///// Deactivate Plan Assignment /////
                    // Bug in Envision's documentation: Replaced Location Name with Location Number as the second parameter.
                    returnedWsXmlNode = optionsManagerService.DeactivateInventory(locationReferenceXml, planAssignmentObject.LocationNumber, planAssignmentObject.LocationLevel);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "2505", "2510", "2907", "2909", "2917" }))
                        return null;
                }
                else
                {
                    ///// Edit Plan Assignment /////
                    returnedWsXmlNode = optionsManagerService.EditInventory(locationReferenceXml, planXml);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "90", "99", "110", "130", "2500", "2505", 
                        "2510",  "2909", "2910", "2917"}))
                        return null;
                    //If a record to be edited isn't in Envision, then create it.
                    if (MessageIdsInReturnedXml(returnedWsXmlNode, new string[] { "2904" }))
                    {
                        string releaseName = (string)this.PivotalDataAccess.SqlIndex(NBHDPhaseData.TableName, NBHDPhaseData.PhaseNameField, currentContextReleaseId);
                        Log.WriteInformation("Could not edit plan " + planAssignmentObject.Name + " of release " + releaseName + ".  The plan does not exist in Envision.  Sending the create command instead.");
                        returnedWsXmlNode = optionsManagerService.CreateInventory(locationReferenceXml, planXml);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PivotalApplicationException(System.String.Format("Plan Assignment web method call failed.  {0}", ex.Message), (int)ErrorNumber.ErrorWebMethodCall);
            }


            return planAssignmentObject;
        }



        /// <summary>
        /// Calls Options Web Service's option assignment web methods.
        /// </summary>
        /// <param name="planAssignmentId">Pivotal Id of the option assignment's plan assignment.</param>
        /// <param name="currentContextReleaseRst">The current release id being processed, if the option assignment is release wildcarded. Do not change cursor position!</param>
        /// <returns>Array of option assignments.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        protected virtual OptionAssignmentType[] UpdateOptionAssignmentsWs(object planAssignmentId, Recordset20 currentContextReleaseRst)
        {

            OptionAssignmentsBuilder optionAssignmentsBuilder = new OptionAssignmentsBuilder(planAssignmentId, currentContextReleaseRst, optionCreationLevel, this.PivotalDataAccess, m_rdaSystem, Config.EnvisionNHTNumber, EnvisionIntegration.TransportType.WebService);
            OptionAssignmentType[] optionAssignments = (OptionAssignmentType[])optionAssignmentsBuilder.ToObject();
            if (optionAssignments != null && optionAssignments.Length > 0)
            {
                object currentContextReleaseId = currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value;
                LocationReferenceBuilder locationReferenceBuilder = new LocationReferenceBuilder(LocationReferenceType.Plan, planAssignmentId, currentContextReleaseId, this.Config, m_rdaSystem);
                XmlNode locationReferenceXml = locationReferenceBuilder.ToXML();
                XmlNode optionAssignmentsXml = optionAssignmentsBuilder.ToXML();
                XmlValidation.OptionAssignments(optionAssignmentsXml);

                try
                {
                    XmlNode returnedWsXmlNode = optionsManagerService.CreateEditOptionAssignments(locationReferenceXml, optionAssignmentsXml);
                    XmlValidation.Output(returnedWsXmlNode);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "2500", "2505", "2510", "2900", "2901", "2902", "2909", "2910", "2917", "171" }))
                        return null;
                }
                catch (Exception ex)
                {
                    throw new PivotalApplicationException(System.String.Format("Option Assignments web method call failed.  {0}", ex.Message), (int)ErrorNumber.ErrorWebMethodCall);
                }

            }

            return optionAssignments;
        }

        /// <summary>
        /// Call IntersectionRuleBuilder and LocationReferenceBuilder to create the XML necessary to call
        /// the Intersection Rule web methods.
        /// Return the InventoryTypeIntersectionRule back to caller.
        /// </summary>
        /// <param name="hardRuleRst">Pivotal hard rule recordset.</param>
        /// <param name="planAssignmentRst">Current plan assignment Rst.</param>
        /// <param name="currentContextReleaseRst">Current release recordset.</param>
        /// <param name="softDeactivate">Indicates to deactivate the rule if no longer applicable to the plan inventory.</param>
        /// <returns></returns>
        protected virtual InventoryTypeIntersectionRule UpdateIntersectionRuleWs(Recordset20 hardRuleRst, Recordset20 planAssignmentRst, Recordset20 currentContextReleaseRst, bool softDeactivate)
        {
            const string IntersectionRuleAlreadyExists = "Intersection Rule Already Exists";

            object currentContextReleaseId = currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value;
            object hardRuleId = hardRuleRst.Fields[ProductOptionRuleData.ProductOptionRuleIdField].Value;
            object planAssignmentId = planAssignmentRst.Fields[NBHDPProductData.NBHDPProductIdField].Value;
            IntersectionRuleBuilder intersectionRuleBuilder = new IntersectionRuleBuilder(hardRuleRst, planAssignmentRst, currentContextReleaseRst, optionCreationLevel, Config.EnvisionNHTNumber,softDeactivate, m_rdaSystem, TransportType.WebService);
            LocationReferenceBuilder locationReferenceBuilder = new LocationReferenceBuilder(LocationReferenceType.Plan, planAssignmentId, currentContextReleaseId, this.Config, m_rdaSystem);
            InventoryTypeIntersectionRule intersectionRule = (InventoryTypeIntersectionRule)intersectionRuleBuilder.ToObject();
            XmlNode locationReferenceXml = locationReferenceBuilder.ToXML();
            XmlNode intersectionRuleXML = intersectionRuleBuilder.ToXML();

            Recordset20 syncRst = this.PivotalDataAccess.GetRecordset(EnvSyncData.QuerySyncRecordForHardRule, 3, hardRuleId,
                planAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value,  currentContextReleaseId, EnvSyncData.RnUpdateCopyField, EnvSyncData.SoftDeactivateField);
            
            try
            {
                if (syncRst.RecordCount == 0)  // Brand new record to Envision.
                {
                    //If the new hard rule is inactive, don't send to Envision.
                    if (!TypeConvert.ToBoolean(hardRuleRst.Fields[ProductOptionRuleData.InactiveField].Value))
                    {
                        XmlNode returnedWsXmlNode = optionsManagerService.CreateIntersectionRule(locationReferenceXml, intersectionRuleXML);
                        if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "2902", "2920", "2923", "2924", "2925", "2926" }))
                            return null;
                        //If Envision says the rule already exists, then send a EditIntersectionRule command.
                        if (MessageIdsInReturnedXml(returnedWsXmlNode, new string[] { "2921" }))
                        {
                            Log.WriteInformation((string)this.LangDictionary.GetTextSub(IntersectionRuleAlreadyExists, new string[] { intersectionRule.Name }));
                            returnedWsXmlNode = optionsManagerService.EditIntersectionRule(locationReferenceXml, intersectionRuleXML);
                            if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "2902", "2920", "2923", "2924", "2925", "2926", "505" }))
                                return null;
                        }
                    }
                }
                else // Already synchronized to Envision before.
                {
                    syncRst.MoveFirst();

                    // If the modified hard rule is inactive in both Pivotal and Envision, don't bother sending the update.
                    if (!TypeConvert.ToBoolean(syncRst.Fields[EnvSyncData.SoftDeactivateField].Value) ||
                            !TypeConvert.ToBoolean(hardRuleRst.Fields[ProductOptionRuleData.InactiveField].Value))
                    {
                        XmlNode returnedWsXmlNode = optionsManagerService.EditIntersectionRule(locationReferenceXml, intersectionRuleXML);
                        if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "2902", "2920", "2923", "2924", "2925", "2926", "505" }))
                            return null;
                        //If Envision says the rule isn't there, then send a CreateIntersectionRule command.
                        if (MessageIdsInReturnedXml(returnedWsXmlNode, new string[] { "2922" }))
                        {
                            Log.WriteInformation("Could not edit intersection rule " + intersectionRule.Name + ".  The rule does not exist in Envision.  Sending the create command instead.");
                            returnedWsXmlNode = optionsManagerService.CreateIntersectionRule(locationReferenceXml, intersectionRuleXML);
                            if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "2902", "2920", "2923", "2924", "2925", "2926", "505" }))
                                return null;
                        }
                    }

                }
                return intersectionRule;
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                return null;
            }
        }



        /// <summary>
        /// Calls the Options Web Service's Inventory methods to update rooms.
        /// </summary>
        /// <param name="planAssignmentId">Rooms's plan assignment id.</param>
        /// <param name="planId">Division Product Id of the plan.</param>
        /// <param name="currentContextReleaseRst">If plan assignment is released wildcarded, pass in the current release recordset.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        protected virtual RoomType[] UpdateRoomsWs(object planAssignmentId, object planId, Recordset20 currentContextReleaseRst)
        {

            object currentContextReleaseId = currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value;
            RoomsBuilder roomsBuilder = new RoomsBuilder(planAssignmentId, currentContextReleaseId, this.PivotalDataAccess, m_rdaSystem, transportType);
            Rooms roomsObject = (Rooms)roomsBuilder.ToObject();

            if (roomsObject != null && roomsObject.Room != null && roomsObject.Room.Length > 0)
            {
           
                LocationReferenceBuilder locationReferenceBuilder = new LocationReferenceBuilder(LocationReferenceType.Plan, planAssignmentId, currentContextReleaseId, this.Config, m_rdaSystem);
                XmlNode locationReferenceXml = locationReferenceBuilder.ToXML();
                XmlNode rooms = roomsBuilder.ToXML();

                try
                { 

                    XmlNode returnedWsXmlNode = optionsManagerService.CreateEditRooms(locationReferenceXml, rooms);
                    if (!SuccessfulReturnedXml(returnedWsXmlNode, new string[] { "110", "130", "150", "240", "2500", "2505", "2510", "2903", "2909", "2910", "2917" }))
                        return null;
                }
                catch (Exception ex)
                {
                    throw new PivotalApplicationException(System.String.Format("Rooms web method call failed.  {0}", ex.Message), (int)ErrorNumber.ErrorWebMethodCall);
                }

            }
            else
            {
                return null;
            }
            return roomsObject.Room;
        }

        #endregion



        /// <summary>
        /// Takes the returnedXML from Envision Web Method call to determine if the call was successful.  If successful,
        /// return true.  If failure, inspect the message Ids if they contain fatalMessages. If not, then return true, 
        /// else false.
        /// </summary>
        /// <param name="returnedXml">Xml returned from web method calls.</param>
        /// <param name="fatalMessages">An array of error message Ids that are truely fatal errors.  These message Ids
        /// correspond to the error codes found in the document "Envision_BuilderWebServices_ErrorCodes-v1_5_0.pdf"</param>
        /// <returns>true if returnedXml is considered successful for the operation, else false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.Parse(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "x"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected virtual bool SuccessfulReturnedXml(XmlNode returnedXml, string[] fatalMessages)
        {
            ArrayList messageIds = new ArrayList();

            if (returnedXml != null)
            {
                if (returnedXml.Attributes["Status"].Value == "Success")
                    return true;

                // Populate the ArrayList of message ids.
                foreach (XmlNode message in returnedXml.SelectNodes("//Message"))
                {
                    //Attempt to convert Message Id to a number.  If fails, return false. 
                    try
                    {
                        int x = Int32.Parse(message.Attributes.GetNamedItem("Id").Value);
                    }
                    catch 
                    {
                        return false;
                    }


                    if ((string)message.Attributes.GetNamedItem("Type").Value == "Error")
                        messageIds.Add(message.Attributes.GetNamedItem("Id").Value);
                };

                // If returnedXml has a fatal error, return false.
                for (int i = 0; i < fatalMessages.Length; i++)
                {
                    if (messageIds.Contains(fatalMessages[i]))
                    {
                        return false;
                    };
                }

                return true;
            }
            return false;

        }

        /// <summary>
        /// Test to see if an error code in testMessages is in the returnedXml.
        /// </summary>
        /// <param name="returnedXml">returnedXml from web method calls.</param>
        /// <param name="testMessages">An array of error codes.</param>
        /// <returns>True if at least one of the error code is in returnedXml.</returns>
        protected virtual bool MessageIdsInReturnedXml(XmlNode returnedXml, string[] testMessages)
        {
            ArrayList messageIds = new ArrayList();

            if (returnedXml != null)
            {
                // Populate the ArrayList of message ids.
                foreach (XmlNode message in returnedXml.SelectNodes("//Message"))
                {
                    messageIds.Add(message.Attributes.GetNamedItem("Id").Value);
                };

                for (int i = 0; i < testMessages.Length; i++)
                {
                    if (messageIds.Contains(testMessages[i]))
                    {
                        return true;
                    };
                }
                return false;

            }
            return false;
        }



    }

}
