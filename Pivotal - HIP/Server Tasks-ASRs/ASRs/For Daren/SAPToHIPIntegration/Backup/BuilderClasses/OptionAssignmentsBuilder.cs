//
// $Workfile: OptionAssignmentsBuilder.cs$
// $Revision: 59$
// $Author: RYong$
// $Date: Monday, August 27, 2007 5:28:09 PM$
//
// Copyright © Pivotal Corporation
//


using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
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
    class OptionAssignmentsBuilder:BuilderBase
    {

        /// <summary>
        /// Process product/option assignments for a given plan assignment. 
        /// </summary>         
        /// <param name="currentContextPlanAssignmentId">Plan Id of the option assignments to be synchronized.</param>
        /// <param name="currentContextReleaseRst">context current Release recordset.  Do not change cursor position!</param>
        /// <param name="optionCreationLevel">Option creation level.</param>         
        /// <param name="objLib">Passing in an existing reference of DataAccess to reduce overhead.</param>
        /// <param name="m_rdaSystem">Passing in an existing reference of IRSystem7 to reduce overhead.</param>
        /// <param name="corporateLocationNumber">The HomeBuilder's corporate location number</param>
        /// <param name="transportType">Transport type: web service or Ftp.</param>
        public OptionAssignmentsBuilder(object currentContextPlanAssignmentId,
            Recordset20 currentContextReleaseRst, LocationReferenceType optionCreationLevel,
            DataAccess objLib, IRSystem7 m_rdaSystem, string corporateLocationNumber, EnvisionIntegration.TransportType transportType)
        {
            const string OptionsSort = "Division_Product_Id, WC_Level DESC";
            const string OptionAssignmentBuilderClass = "OptionAssignmentsBuilder class. ";

            Recordset20 optionAssignmentRst;
            Recordset20 planRst;
            Recordset20 dpLocationAssignmentRst;
            Recordset20 envSyncLocRst;
            ArrayList optionAssignments = new ArrayList();
            OptionAssignmentType iOptionAssignment, iRoomAssignment;
            object regionId;
            object divisionId;
            object neighborhoodId;
            object planId;
            object previousOptionId = null;
            object constructionStageId;
            object currentContextReleaseId = currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value;
            string locationNumber, locationLevel;

            try
            {
                divisionId = currentContextReleaseRst.Fields[NBHDPhaseData.DivisionIdField].Value;
                regionId = currentContextReleaseRst.Fields[NBHDPhaseData.RegionIdField].Value;
                neighborhoodId = currentContextReleaseRst.Fields[NBHDPhaseData.NeighborhoodIdField].Value;

                //Location number is supposed to come directly from the Option(s), but to improve export performance,
                //this value is taken from the Plan to save a database read inside a loop.
                switch (optionCreationLevel)
                {
                    case LocationReferenceType.Corporate:
                        locationLevel = EnvisionIntegration.LocationLevel.CodeCorporation;
                        locationNumber = corporateLocationNumber;
                        break;
                    case LocationReferenceType.Region:
                        locationLevel = EnvisionIntegration.LocationLevel.CodeRegion;
                        locationNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(regionId));
                        break;
                    case LocationReferenceType.Division:
                        locationLevel = EnvisionIntegration.LocationLevel.CodeDivision;
                        locationNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(divisionId));
                        break;
                    default:
                        return;
                }



                // Generate Option Assignment objects to send to Envision.
                optionAssignmentRst = objLib.GetRecordset(NBHDPProductData.OptionAssignmentsToSynchronizeQuery, 10,
                    currentContextPlanAssignmentId, regionId, divisionId, neighborhoodId,
                    currentContextReleaseId, divisionId, currentContextPlanAssignmentId, 
                    currentContextReleaseId, divisionId, currentContextReleaseId,
                    NBHDPProductData.NBHDPProductIdField, NBHDPProductData.PlanIdField,
                    NBHDPProductData.DivisionProductIdField, NBHDPProductData.RnUpdateField,
                    NBHDPProductData.WCLevelField, NBHDPProductData.CurrentPriceField,
                    NBHDPProductData.InactiveField, NBHDPProductData.PostCuttOffPriceField,
                    NBHDPProductData.LocationIdField, NBHDPProductData.OptionAvailableToField,
                    NBHDPProductData.ConstructionStageIdField, NBHDPProductData.ProductNameField);


                //Scroll through the changed option assignments
                if (optionAssignmentRst.RecordCount > 0)
                {
                    optionAssignmentRst.Sort = OptionsSort;  //Sort the assignments by products and by precedence rules.
                    optionAssignmentRst.MoveFirst();
                    StringBuilder sb = new StringBuilder();

                    planRst = objLib.GetRecordset(DivisionProductData.DivisionProductOfAssignmentQuery, 1, currentContextPlanAssignmentId, DivisionProductData.DivisionProductIdField, DivisionProductData.ProductNameField);
                    planRst.MoveFirst();
                    planId = planRst.Fields[DivisionProductData.DivisionProductIdField].Value;

                    while (!optionAssignmentRst.EOF)
                    {
                        //No duplicates allowed.  Keep skipping to the next option assignment until one with a different option.
                        if (m_rdaSystem.EqualIds(previousOptionId, optionAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value))
                        {
                            optionAssignmentRst.MoveNext();
                            continue;
                        }

                        previousOptionId = optionAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value;

                        iOptionAssignment = new OptionAssignmentType();
                        iOptionAssignment.OptionNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(optionAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value));
                        iOptionAssignment.LocationNumber = locationNumber;
                        iOptionAssignment.LocationLevel = locationLevel;
                        iOptionAssignment.Price = TypeConvert.ToDecimal(optionAssignmentRst.Fields[NBHDPProductData.CurrentPriceField].Value);
                        iOptionAssignment.PostCutoffPrice = TypeConvert.ToDecimal(optionAssignmentRst.Fields[NBHDPProductData.PostCuttOffPriceField].Value);
                        iOptionAssignment.Deactivate = TypeConvert.ToBoolean(optionAssignmentRst.Fields[NBHDPProductData.InactiveField].Value);

                        //Location_Id field is set if the option assignment is assigned to one specific location.
                        if (transportType == EnvisionIntegration.TransportType.WebService)
                        {
                            // If option assignment is inactive, do not include the room number, because Envision would treat that as only deactivating the room assignment.
                            if (!iOptionAssignment.Deactivate && !Convert.IsDBNull(optionAssignmentRst.Fields[NBHDPProductData.LocationIdField].Value))
                            {
                                iOptionAssignment.RoomNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(optionAssignmentRst.Fields[NBHDPProductData.LocationIdField].Value));
                                iOptionAssignment.RoomId = optionAssignmentRst.Fields[NBHDPProductData.LocationIdField].Value;
                                iOptionAssignment.RnUpdateRoom = (byte[])optionAssignmentRst.Fields[NBHDPProductData.RnUpdateField].Value;
                            }
                        }

                        constructionStageId = optionAssignmentRst.Fields[NBHDPProductData.ConstructionStageIdField].Value;
                        if (!Convert.IsDBNull(constructionStageId))
                            iOptionAssignment.CutoffStageNumber = CompactPivotalId(m_rdaSystem.IdToString(constructionStageId));
                        iOptionAssignment.RnUpdateOption = (byte[])optionAssignmentRst.Fields[NBHDPProductData.RnUpdateField].Value;
                        iOptionAssignment.OptionId = (byte[])optionAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value;
                        if (iOptionAssignment.PostCutoffPrice > 0.00M) iOptionAssignment.PostCutoffPriceSpecified = true;

                        optionAssignments.Add(iOptionAssignment);

                        // In Web Service mode, room assignment nodes are created below option assignments.
                        if (transportType == EnvisionIntegration.TransportType.WebService)
                        {
                            if (sb.Length > 0)
                                sb.Append(", ");
                            sb.Append(TypeConvert.ToString(optionAssignmentRst.Fields[NBHDPProductData.ProductNameField].Value));
                            sb.Append(System.String.Format("[{0}]", iOptionAssignment.OptionNumber));

                            // Create additional nodes for room assignments if Option_Available_To is "All Locations".
                            if (String.Equals(optionAssignmentRst.Fields[NBHDPProductData.OptionAvailableToField].Value, NBHDPProductData.OptionAvailableToAllLocations))
                            {
                                // Look for changes to existing valid location assignments.
                                dpLocationAssignmentRst = objLib.GetRecordset(DivisionProductLocationsData.DPLocationAssignmentsToSynchronizePerOptionAssignmentQuery, 3,
                                        planId,
                                        optionAssignmentRst.Fields[NBHDPProductData.NBHDPProductIdField].Value, currentContextReleaseId,
                                        DivisionProductLocationsData.RnUpdateField, DivisionProductLocationsData.LocationIdField, DivisionProductLocationsData.InactiveField);
                                if (dpLocationAssignmentRst.RecordCount > 0)
                                {
                                    dpLocationAssignmentRst.Sort = DivisionProductLocationsData.InactiveField;
                                    dpLocationAssignmentRst.MoveFirst();
                                    while (!dpLocationAssignmentRst.EOF)
                                    {
                                        iRoomAssignment = new OptionAssignmentType();
                                        iRoomAssignment.OptionNumber = iOptionAssignment.OptionNumber;
                                        iRoomAssignment.OptionId = iOptionAssignment.OptionId;
                                        iRoomAssignment.LocationNumber = iOptionAssignment.LocationNumber;
                                        iRoomAssignment.LocationLevel = iOptionAssignment.LocationLevel;
                                        iRoomAssignment.Price = iOptionAssignment.Price;
                                        iRoomAssignment.RoomNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(dpLocationAssignmentRst.Fields[DivisionProductLocationsData.LocationIdField].Value));
                                        iRoomAssignment.RoomId = dpLocationAssignmentRst.Fields[DivisionProductLocationsData.LocationIdField].Value;
                                        if (iOptionAssignment.Deactivate)
                                            iRoomAssignment.Deactivate = true;
                                        else
                                            iRoomAssignment.Deactivate = TypeConvert.ToBoolean(dpLocationAssignmentRst.Fields[DivisionProductLocationsData.InactiveField].Value);
                                        iRoomAssignment.SoftDeactivate = iOptionAssignment.Deactivate;
                                        iRoomAssignment.RnUpdateOption = iOptionAssignment.RnUpdateOption;
                                        iRoomAssignment.RnUpdateRoom = (byte[])dpLocationAssignmentRst.Fields[DivisionProductLocationsData.RnUpdateField].Value;
                                        iRoomAssignment.OptionId = iOptionAssignment.OptionId;

                                        optionAssignments.Add(iRoomAssignment);
                                        dpLocationAssignmentRst.MoveNext();
                                    }
                                }

                                // Look for previously assigned rooms that are no longer valid for the current option.  
                                // Inactivate them in Envision.
                                envSyncLocRst = objLib.GetRecordset(EnvSyncData.QueryPreviousLocationAssignmentsToDeactivateAllLocations, 4,
                                        currentContextReleaseId, planId, optionAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value,
                                        divisionId,
                                        EnvSyncData.LocationIdField);
                                if (envSyncLocRst.RecordCount > 0)
                                {
                                    envSyncLocRst.MoveFirst();
                                    while (!envSyncLocRst.EOF)
                                    {
                                        iRoomAssignment = new OptionAssignmentType();
                                        iRoomAssignment.OptionNumber = iOptionAssignment.OptionNumber;
                                        iRoomAssignment.OptionId = iOptionAssignment.OptionId;
                                        iRoomAssignment.LocationNumber = iOptionAssignment.LocationNumber;
                                        iRoomAssignment.LocationLevel = iOptionAssignment.LocationLevel;
                                        iRoomAssignment.Price = iOptionAssignment.Price;
                                        iRoomAssignment.RoomId = (byte[])envSyncLocRst.Fields[EnvSyncData.LocationIdField].Value;
                                        iRoomAssignment.RoomNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(iRoomAssignment.RoomId));
                                        iRoomAssignment.Deactivate = true;
                                        iRoomAssignment.SoftDeactivate = iOptionAssignment.Deactivate;
                                        iRoomAssignment.RnUpdateOption = iOptionAssignment.RnUpdateOption;
                                        iRoomAssignment.RnUpdateRoom = (byte[])m_rdaSystem.StringToId("0x0000000000000000");

                                        optionAssignments.Add(iRoomAssignment);
                                        envSyncLocRst.MoveNext();
                                    }
                                }

                            }

                            else
                            {
                                // Look for previously assigned rooms that are no longer valid for the current option where
                                // Option Available To = "Specific Location" or "Whole House".  Inactivate them in Envision.
                                object optionLocationId = optionAssignmentRst.Fields[NBHDPProductData.LocationIdField].Value;
                                envSyncLocRst = objLib.GetRecordset(EnvSyncData.QueryPreviousLocationAssignmentsToDeactivateSpecificOrWholeHouse, 4,
                                        currentContextReleaseId, planId, optionAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value,
                                        optionLocationId,
                                        EnvSyncData.LocationIdField);
                                if (envSyncLocRst.RecordCount > 0)
                                {
                                    envSyncLocRst.MoveFirst();
                                    while (!envSyncLocRst.EOF)
                                    {
                                        iRoomAssignment = new OptionAssignmentType();
                                        iRoomAssignment.OptionNumber = iOptionAssignment.OptionNumber;
                                        iRoomAssignment.OptionId = iOptionAssignment.OptionId;
                                        iRoomAssignment.LocationNumber = iOptionAssignment.LocationNumber;
                                        iRoomAssignment.LocationLevel = iOptionAssignment.LocationLevel;
                                        iRoomAssignment.Price = iOptionAssignment.Price;
                                        iRoomAssignment.RoomId = envSyncLocRst.Fields[EnvSyncData.LocationIdField].Value;
                                        iRoomAssignment.RoomNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(iRoomAssignment.RoomId));
                                        iRoomAssignment.Deactivate = true;
                                        iRoomAssignment.SoftDeactivate = iOptionAssignment.Deactivate;
                                        iRoomAssignment.RnUpdateOption = iOptionAssignment.RnUpdateOption;
                                        iRoomAssignment.RnUpdateRoom = (byte[])m_rdaSystem.StringToId("0x0000000000000000");

                                        optionAssignments.Add(iRoomAssignment);
                                        envSyncLocRst.MoveNext();
                                    }
                                }
                            }
                        }  // Transport = Web Service.
                    
                        

                        optionAssignmentRst.MoveNext();
                    }

                    string planName = TypeConvert.ToString(planRst.Fields[DivisionProductData.ProductNameField].Value);
                    string planIntegrationKey = BuilderBase.GetIntegrationKey(LocationReferenceType.Plan, currentContextPlanAssignmentId, currentContextReleaseId, m_rdaSystem);

                    //Log the option assignments.
                    if (transportType == EnvisionIntegration.TransportType.WebService)
                        comments = System.String.Format("Assigning these options to plan {0}[{1}] of release {2}:  {3}", planName, planIntegrationKey, currentContextReleaseRst.Fields[NBHDPhaseData.PhaseNameField].Value, sb.ToString());
                }


                //This section finds orphan records for option assignment using the Env_Sync records.
                Recordset20 outdatedEnvSyncRst = objLib.GetRecordset(EnvSyncData.QueryPreviousOptionAssignmentsToDelete, 7,
                    currentContextPlanAssignmentId, currentContextReleaseId,
                    regionId, divisionId, neighborhoodId, currentContextReleaseId, divisionId,
                    EnvSyncData.DivisionProductOptionIdField, EnvSyncData.LocationIdField, EnvSyncData.SyncTypeField);

                if (outdatedEnvSyncRst.RecordCount > 0)
                {
                    outdatedEnvSyncRst.MoveFirst();
                    while (!outdatedEnvSyncRst.EOF)
                    {
                        iOptionAssignment = new OptionAssignmentType();
                        iOptionAssignment.OptionNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(outdatedEnvSyncRst.Fields[EnvSyncData.DivisionProductOptionIdField].Value));
                        if (!Convert.IsDBNull(outdatedEnvSyncRst.Fields[EnvSyncData.DivisionProductOptionIdField].Value))
                            iOptionAssignment.OptionId = (byte[])outdatedEnvSyncRst.Fields[EnvSyncData.DivisionProductOptionIdField].Value;
                        iOptionAssignment.LocationNumber = locationNumber;
                        iOptionAssignment.LocationLevel = locationLevel;
                        iOptionAssignment.Price = 0.00M;
                        iOptionAssignment.Deactivate = true;
                        iOptionAssignment.SoftDeactivate = true;
                        iOptionAssignment.RnUpdateOption = (byte[])m_rdaSystem.StringToId("0x0000000000000000");

                        optionAssignments.Add(iOptionAssignment);
                        outdatedEnvSyncRst.MoveNext();
                    }
                }


                xsdObject = (OptionAssignmentType[])optionAssignments.ToArray(typeof(OptionAssignmentType));
            }
            catch (Exception ex)
            {
                throw new PivotalApplicationException(OptionAssignmentBuilderClass, ex);
            }
        }

        /// <summary>
        /// Given the release, plan and location id, return a list of location assignments to synchronize to Envision.
        /// This method is called in Ftp mode, when the integration is trying to generate room assignment nodes under the
        /// plan inventory / room Xml node.  It looks for changes for 2 types of option assignments: "Location Specific" and 
        /// "All Location".  For Location Specific, the room id is specified right at the option assignment record.  For All Location, 
        /// every location assigned to the plan is assigned to the option assignment.
        /// </summary> 
        /// <param name="locationId">The current location id.</param>
        /// <param name="currentContextPlanAssignmentId">Plan Id of the option assignments to be synchronized.</param>
        /// <param name="currentContextReleaseRst">context current Release recordset.  Do not change cursor position!</param>
        /// <param name="optionCreationLevel">Option creation level.</param>         
        /// <param name="objLib">Passing in an existing reference of DataAccess to reduce overhead.</param>
        /// <param name="m_rdaSystem">Passing in an existing reference of IRSystem7 to reduce overhead.</param>
        /// <param name="corporateLocationNumber">The HomeBuilder's corporate location number</param>
        public OptionAssignmentsBuilder( object locationId, object currentContextPlanAssignmentId,
            Recordset20 currentContextReleaseRst, LocationReferenceType optionCreationLevel,
            DataAccess objLib, IRSystem7 m_rdaSystem, string corporateLocationNumber)
        {
            const string OptionsSort = "Division_Product_Id, WC_Level DESC";
            const string OptionAssignmentBuilderClass = "OptionAssignmentsBuilder class. ";

            Recordset20 optionAssignmentRst;
            Recordset20 planRst;
            Recordset20 dpLocationAssignmentRst;
            ArrayList roomAssignments = new ArrayList();
            OptionAssignmentType iRoomAssignment;      
      
            object regionId;
            object divisionId;
            object neighborhoodId;
            object planId;
            object previousOptionId = null;
            object currentContextReleaseId = currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value;
            string locationNumber, locationLevel;

            try
            {
                divisionId = currentContextReleaseRst.Fields[NBHDPhaseData.DivisionIdField].Value;
                regionId = currentContextReleaseRst.Fields[NBHDPhaseData.RegionIdField].Value;
                neighborhoodId = currentContextReleaseRst.Fields[NBHDPhaseData.NeighborhoodIdField].Value;

                //Location number is supposed to come directly from the Option(s), but to improve export performance,
                //this value is taken from the Plan to save a database read inside a loop.
                switch (optionCreationLevel)
                {
                    case LocationReferenceType.Corporate:
                        locationLevel = EnvisionIntegration.LocationLevel.CodeCorporation;
                        locationNumber = corporateLocationNumber;
                        break;
                    case LocationReferenceType.Region:
                        locationLevel = EnvisionIntegration.LocationLevel.CodeRegion;
                        locationNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(regionId));
                        break;
                    case LocationReferenceType.Division:
                        locationLevel = EnvisionIntegration.LocationLevel.CodeDivision;
                        locationNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(divisionId));
                        break;
                    default:
                        return;
                }


                // Given the release, plan inventory and location id
                optionAssignmentRst = objLib.GetRecordset(NBHDPProductData.LocationOptionAssignmentsToSynchronizeQuery, 9,
                    currentContextReleaseId, currentContextPlanAssignmentId, 
                    regionId, divisionId, neighborhoodId, currentContextReleaseId,
                    currentContextReleaseId, currentContextPlanAssignmentId, locationId,
                    NBHDPProductData.NBHDPProductIdField, NBHDPProductData.PlanIdField,
                   NBHDPProductData.DivisionProductIdField, NBHDPProductData.RnUpdateField,
                    NBHDPProductData.WCLevelField, NBHDPProductData.CurrentPriceField,
                    NBHDPProductData.InactiveField, NBHDPProductData.PostCuttOffPriceField,
                    NBHDPProductData.LocationIdField, NBHDPProductData.OptionAvailableToField);


                //Scroll through the changed option assignments
                if (optionAssignmentRst.RecordCount > 0)
                {
                    optionAssignmentRst.Sort = OptionsSort;
                    optionAssignmentRst.MoveFirst();

                    planRst = objLib.GetRecordset(DivisionProductData.DivisionProductOfAssignmentQuery, 1, currentContextPlanAssignmentId, DivisionProductData.DivisionProductIdField, DivisionProductData.ProductNameField);
                    planRst.MoveFirst();
                    planId = planRst.Fields[DivisionProductData.DivisionProductIdField].Value;

                    while (!optionAssignmentRst.EOF)
                    {
                        //No duplicates allowed.  If another option assignment has the same option, skip to the next option assignment.
                        //This shouldn't be needed but here as a fail safe.
                        if (m_rdaSystem.EqualIds(previousOptionId, optionAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value))
                        {
                            optionAssignmentRst.MoveNext();
                            continue;
                        }
                        previousOptionId = optionAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value;

                        
                        // If option assignment is "Specific Location", then include room assignment update based on the option assignment.
                        if ((String)optionAssignmentRst.Fields[NBHDPProductData.OptionAvailableToField].Value == NBHDPProductData.OptionAvailableToSpecificLocation)
                        {
                            iRoomAssignment = new OptionAssignmentType();
                            iRoomAssignment.OptionNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(optionAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value));
                            iRoomAssignment.LocationNumber = locationNumber;
                            iRoomAssignment.LocationLevel = locationLevel;
                            iRoomAssignment.Price = TypeConvert.ToDecimal(optionAssignmentRst.Fields[NBHDPProductData.CurrentPriceField].Value);
                            iRoomAssignment.Deactivate = TypeConvert.ToBoolean(optionAssignmentRst.Fields[NBHDPProductData.InactiveField].Value);
                            iRoomAssignment.RnUpdateOption = (byte[])optionAssignmentRst.Fields[NBHDPProductData.RnUpdateField].Value;
                            iRoomAssignment.RnUpdateRoom = (byte[])optionAssignmentRst.Fields[NBHDPProductData.RnUpdateField].Value;
                            iRoomAssignment.OptionId = (byte[])optionAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value;
                            iRoomAssignment.PostCutoffPriceSpecified = false;

                            roomAssignments.Add(iRoomAssignment);
                        }
                        // If option assignment is "All Locations", then include room assignment update based on division_product_locations.
                        else if ((String)optionAssignmentRst.Fields[NBHDPProductData.OptionAvailableToField].Value == NBHDPProductData.OptionAvailableToAllLocations)
                        {
                            dpLocationAssignmentRst = objLib.GetRecordset(DivisionProductLocationsData.DPLocationAssignmentsToSynchronizePerLocation, 4,
                                planId, locationId, currentContextReleaseId, optionAssignmentRst.Fields[NBHDPProductData.NBHDPProductIdField].Value,
                                DivisionProductLocationsData.InactiveField, DivisionProductLocationsData.RnUpdateField);
                            if (dpLocationAssignmentRst.RecordCount > 0)
                            {
                                dpLocationAssignmentRst.MoveFirst();

                                iRoomAssignment = new OptionAssignmentType();
                                iRoomAssignment.OptionNumber = BuilderBase.CompactPivotalId(m_rdaSystem.IdToString(optionAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value));
                                iRoomAssignment.LocationNumber = locationNumber;
                                iRoomAssignment.LocationLevel = locationLevel;
                                iRoomAssignment.Price = TypeConvert.ToDecimal(optionAssignmentRst.Fields[NBHDPProductData.CurrentPriceField].Value);
                                iRoomAssignment.Deactivate = TypeConvert.ToBoolean(dpLocationAssignmentRst.Fields[DivisionProductLocationsData.InactiveField].Value);
                                iRoomAssignment.RnUpdateOption = (byte[])optionAssignmentRst.Fields[NBHDPProductData.RnUpdateField].Value;
                                iRoomAssignment.RnUpdateRoom = (byte[])dpLocationAssignmentRst.Fields[DivisionProductLocationsData.RnUpdateField].Value;
                                iRoomAssignment.OptionId = (byte[])optionAssignmentRst.Fields[NBHDPProductData.DivisionProductIdField].Value;
                                iRoomAssignment.PostCutoffPriceSpecified = false;
                                roomAssignments.Add(iRoomAssignment);
                            }

                        }

                        optionAssignmentRst.MoveNext();
                    }
                }

                xsdObject = (OptionAssignmentType[])roomAssignments.ToArray(typeof(OptionAssignmentType));  
            }
            catch (Exception ex)
            {
                throw new PivotalApplicationException(OptionAssignmentBuilderClass, ex);
            }

        }




        /// <summary>
        /// Returns the Xml of the option assignments.  Overriding this so that the generated XMLNode uses the 
        /// "OptionAssignments" element tag instead of "Assignments" which is used by Ftp.
        /// </summary>
        /// <returns>Xml of the option assignments</returns>
        override internal XmlNode ToXML()
        {
            const string OptionAssignmentsBuilderToXML = "OptionAssignmentsBuilder.ToXml()";

            try
            {
                MemoryStream memoryStream = new MemoryStream();
                StreamWriter streamWriter = new StreamWriter(memoryStream);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(OptionAssignments));
                OptionAssignments optionAssignments = new OptionAssignments();
                optionAssignments.Assignment = (OptionAssignmentType[])xsdObject;
                xmlSerializer.Serialize(streamWriter, optionAssignments);
                streamWriter.Close();

                UTF8Encoding encoding = new UTF8Encoding();
                string xmlString = encoding.GetString(memoryStream.GetBuffer());
                xmlString = xmlString.Trim(new char[] { '\0' });
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);
                if (!String.IsNullOrEmpty(comments))
                    xmlDoc.DocumentElement.AppendChild(xmlDoc.CreateComment(comments));
                return (XmlNode)xmlDoc.DocumentElement;
            }
            catch (Exception ex)
            {
                throw new PivotalApplicationException(OptionAssignmentsBuilderToXML, ex);
            }
        }
    }


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class OptionAssignments
    {
        private OptionAssignmentType[] assignmentField;


        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Assignment")]
        public OptionAssignmentType[] Assignment
        {
            get
            {
                return this.assignmentField;
            }
            set
            {
                this.assignmentField = value;
            }
        }
    }
}
