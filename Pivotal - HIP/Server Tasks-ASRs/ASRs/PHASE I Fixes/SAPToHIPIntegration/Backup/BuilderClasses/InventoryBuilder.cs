//
// $Workfile: InventoryBuilder.cs$
// $Revision: 26$
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    class InventoryBuilder:BuilderBase
    {
        /// <summary>
        /// Constructs an Inventory object.  An inventory can be either Neighborhood, Release or Plan Assignment.
        /// </summary>
        /// <param name="locationReference">Specifies the inventory level of this inventory.</param>
        /// <param name="inventoryId">Pivotal Id of this inventory.</param>
        /// <param name="objLib">Object library for querying.</param>
        /// <param name="mrsysSystem">IRSystem instance to use some of its methods.</param>
        /// <param name="transportType">Transport mode</param>
        public InventoryBuilder(LocationReferenceType locationReference, object inventoryId, DataAccess objLib, IRSystem7 mrsysSystem, EnvisionIntegration.TransportType transportType)
            :
            this(locationReference, inventoryId, null, objLib, mrsysSystem, transportType)
        {
        }

        /// <summary>
        /// Constructs an Inventory object.  An inventory can be either Neighborhood, Release or Plan Assignment.
        /// </summary>
        /// <param name="locationReference">Specifies the inventory level of this inventory.</param>
        /// <param name="inventoryId">Pivotal Id of this inventory.</param>
        /// <param name="currentContextReleaseId">If the inventory is a plan assignment and it's release wildcarded, currentContextReleaseId is the current release being processed.</param>
        /// <param name="objLib">Object library for querying.</param>
        /// <param name="mrsysSystem">IRSystem instance to use some of its methods.</param>
        /// <param name="transportType">Indicates the transport type which can be web service or Ftp.</param>
        public InventoryBuilder(LocationReferenceType locationReference, object inventoryId, object currentContextReleaseId, DataAccess objLib, IRSystem7 mrsysSystem, EnvisionIntegration.TransportType transportType)
        {
            const string InventoryBuilderClass = "InventoryBuilder class. ";
            Recordset20 inventoryRst;
            //OrganizationTypeInventory inventory;
            InventoryType inventory;
            string inventoryNameDbFieldName;
            StringBuilder sb = new StringBuilder();

            try
            {
                if (transportType == EnvisionIntegration.TransportType.WebService)
                {
                    inventory = (InventoryType) new OrganizationTypeInventory();
                }
                else  //Ftp Transport
                {
                    if (locationReference == LocationReferenceType.Community)
                    {   // Community inventory
                        inventory =  (InventoryType) new OrganizationTypeInventory();
                    }
                    else
                    {   //Release or Plan inventory
                        inventory =  new InventoryType();            
                    }
                }

                switch (locationReference)
                {
                    case (LocationReferenceType.Community):
                        inventoryRst = objLib.GetRecordset(inventoryId, NeighborhoodData.TableName, NeighborhoodData.NeighborhoodIdField, NeighborhoodData.NameField, NeighborhoodData.InactiveField);
                        inventoryNameDbFieldName = NeighborhoodData.NameField;
                        inventory.LocationLevel = EnvisionIntegration.LocationLevel.CodeCommunity.ToUpper();
                        inventory.LocationNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(inventoryId));                        
                        break;
                    case (LocationReferenceType.Release):
                        inventoryRst = objLib.GetRecordset(inventoryId, NBHDPhaseData.TableName, NBHDPhaseData.NBHDPhaseIdField, NBHDPhaseData.PhaseNameField, NBHDPhaseData.InactiveField);
                        inventoryNameDbFieldName = NBHDPhaseData.PhaseNameField;
                        inventory.LocationLevel = EnvisionIntegration.LocationLevel.CodeRelease.ToUpper();
                        inventory.LocationNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(inventoryId));
                        break;
                    case (LocationReferenceType.Plan):
                        inventoryRst = objLib.GetRecordset(inventoryId, NBHDPProductData.TableName, NBHDPProductData.NBHDPProductIdField, NBHDPProductData.ProductNameField, NBHDPProductData.InactiveField);
                        inventoryNameDbFieldName = NBHDPProductData.ProductNameField;
                        inventory.LocationLevel = EnvisionIntegration.LocationLevel.CodePlan.ToUpper();
                        inventory.LocationNumber = BuilderBase.GetIntegrationKey(locationReference, inventoryId, currentContextReleaseId, mrsysSystem);
                        break;
                    default:
                        return;
                }

                if (inventoryRst.RecordCount > 0)
                {
                    inventoryRst.MoveFirst();
                    inventory.Name = TypeConvert.ToString(inventoryRst.Fields[inventoryNameDbFieldName].Value);
                    inventory.IntegrationKey = BuilderBase.GetIntegrationKey(locationReference, inventoryId, currentContextReleaseId, mrsysSystem);
                    inventory.Deactivate = TypeConvert.ToBoolean(inventoryRst.Fields[NeighborhoodData.InactiveField].Value);
                    xsdObject = inventory;
                    string releaseDescriptor;
                    if (locationReference == LocationReferenceType.Plan && !Convert.IsDBNull(currentContextReleaseId))
                        releaseDescriptor = string.Format(" of release '{0}'",objLib.SqlIndex(NBHDPhaseData.TableName, NBHDPhaseData.PhaseNameField, currentContextReleaseId));
                    else
                        releaseDescriptor = String.Empty;

                    if (transportType == EnvisionIntegration.TransportType.WebService)
                    {
                        sb.Append(System.String.Format("Synchronizing {0}: '{1}'[{2}]{3}.", locationReference.ToString(), inventory.Name, inventory.IntegrationKey, releaseDescriptor));
                        comments = sb.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PivotalApplicationException(InventoryBuilderClass, ex);
            }

        }



  

    }
}
