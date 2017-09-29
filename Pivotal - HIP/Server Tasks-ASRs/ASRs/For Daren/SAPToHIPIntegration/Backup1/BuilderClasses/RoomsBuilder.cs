//
// $Workfile: RoomsBuilder.cs$
// $Revision: 2$
// $Author: RYong$
// $Date: Wednesday, December 19, 2007 3:41:53 PM$
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
using System.Text;


namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated
{
    public class RoomsBuilder : BuilderBase
    {
        /// <summary>
        /// Constructs array of rooms to a plan inventory.
        /// </summary>
        /// <param name="planAssignmentId">The plan in the current context.</param>
        /// <param name="currentContextReleaseId">The release in the current context.</param>
        /// <param name="objLib">Object library to query data.</param>
        /// <param name="mrsysSystem">RsysSystemt to convert data.</param>
        /// <param name="transportType">Transport type: Ftp or web service.</param>
        public RoomsBuilder(object planAssignmentId, object currentContextReleaseId, DataAccess objLib, IRSystem7 mrsysSystem, EnvisionIntegration.TransportType transportType)
        {
            List<RoomType> arrListRooms;

            Recordset20 roomRst;
            Recordset20 dpRoomRst;
            RoomType aRoom;

            try
            {
                if (transportType == EnvisionIntegration.TransportType.WebService)
                {
                    dpRoomRst = objLib.GetRecordset(DivisionProductLocationsData.DPLocationsToSynchronizeForWSQuery, 2, planAssignmentId, currentContextReleaseId, DivisionProductLocationsData.LocationIdField, DivisionProductLocationsData.RnUpdateField, DivisionProductLocationsData.InactiveField, DivisionProductLocationsData.DivisionProductIdField);
                }
                else
                {
                    dpRoomRst = objLib.GetRecordset(DivisionProductLocationsData.DPLocationsToSynchronizeForFTPQuery, 2, planAssignmentId, currentContextReleaseId, DivisionProductLocationsData.LocationIdField, DivisionProductLocationsData.RnUpdateField, DivisionProductLocationsData.InactiveField, DivisionProductLocationsData.DivisionProductIdField);
                }

                if (dpRoomRst.RecordCount > 0)
                {
                    arrListRooms = new List<RoomType>();
                    dpRoomRst.MoveFirst();
                    StringBuilder sb = new StringBuilder();
                    while (!dpRoomRst.EOF)
                    {
                        roomRst = objLib.GetRecordset(dpRoomRst.Fields[DivisionProductLocationsData.LocationIdField].Value, LocationData.TableName, LocationData.LocationIdField, LocationData.NameField, LocationData.LocationTypeField, LocationData.InactiveField, LocationData.RnUpdateField);
                        roomRst.MoveFirst();
                        aRoom = new RoomType();
                        aRoom.Name = (string)roomRst.Fields[LocationData.NameField].Value;
                        aRoom.RoomNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(roomRst.Fields[LocationData.LocationIdField].Value));
                        aRoom.RoomType1 = (RoomTypeRoomType)(byte)(roomRst.Fields[LocationData.LocationTypeField].Value);
                        aRoom.Deactivate = TypeConvert.ToBoolean(dpRoomRst.Fields[DivisionProductLocationsData.InactiveField].Value);
                        aRoom.LocationId = (byte[])roomRst.Fields[LocationData.LocationIdField].Value;
                        aRoom.RnUpdateLocation = (byte[])roomRst.Fields[LocationData.RnUpdateField].Value;
                        aRoom.RnUpdateDPLocation = (byte[])dpRoomRst.Fields[DivisionProductLocationsData.RnUpdateField].Value;
                        aRoom.DPLocationId = (byte[])dpRoomRst.Fields[DivisionProductLocationsData.DivisionProductIdField].Value;
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(aRoom.Name);
                        sb.Append(string.Format("[{0}]", aRoom.RoomNumber));

                        arrListRooms.Add(aRoom);
                        dpRoomRst.MoveNext();
                    }

                    Rooms rooms = new Rooms();
                    rooms.Room = arrListRooms.ToArray();

                    xsdObject = rooms;

                    if (transportType == EnvisionIntegration.TransportType.WebService)
                    {
                        string releaseName = (string)objLib.SqlIndex(NBHDPhaseData.TableName, NBHDPhaseData.PhaseNameField, currentContextReleaseId);
                        string planName = (string)objLib.SqlIndex(NBHDPProductData.TableName, NBHDPProductData.ProductNameField, planAssignmentId);
                        string planIntegrationKey = BuilderBase.GetIntegrationKey(LocationReferenceType.Plan, planAssignmentId, currentContextReleaseId, mrsysSystem);
                        comments = string.Format("Assigning these rooms to plan '{0}'[{1}] of {2}: {3}", planName, planIntegrationKey, releaseName, sb.ToString());
                    }
                }
                else
                {
                    xsdObject = null;
                }
            }
            catch (Exception ex) 
            {
                throw new PivotalApplicationException("RoomsBuilder class.  ", ex); 
            }

        }

    }
}
