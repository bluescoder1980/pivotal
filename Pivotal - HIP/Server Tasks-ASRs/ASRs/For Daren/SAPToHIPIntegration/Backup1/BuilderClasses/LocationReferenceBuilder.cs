//
// $Workfile: LocationReferenceBuilder.cs$
// $Revision: 1$
// $Author: RYong$
// $Date: Monday, August 27, 2007 5:28:09 PM$
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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{

   
    /// <summary>
    /// Defines 6 location levels
    /// </summary>
    public enum LocationReferenceType
    {
        /// <summary>
        /// Corporation level
        /// </summary>
        Corporate,

        /// <summary>
        /// Region level
        /// </summary>
        Region,

        /// <summary>
        /// Division level
        /// </summary>
        Division,

        /// <summary>
        /// Community level
        /// </summary>
        Community,

        /// <summary>
        /// Release level
        /// </summary>
        Release,

        /// <summary>
        /// Plan level
        /// </summary>
        Plan
    }

    /// <summary>
    /// Builds Location Reference which is used by web service to point to the location 
    /// where the web service operation should happen.
    /// </summary>
    class LocationReferenceBuilder:BuilderBase
    { 
        /// <summary>
        /// Constructs the location reference objects.
        /// </summary>
        /// <param name="locationReference">Specifies the location level: Corporation, Region, Division, Community, Release or Plan.</param>
        /// <param name="inventoryId">The pivotal id of the current organization or inventory object.</param>
        /// <param name="currentContextReleaseId">The current release id being processed for a release wildcarded plan assignment.</param>
        /// <param name="configuration">Configration object to retrieve header information.</param>
        /// <param name="mrsysSystem">IRSystem object to access some needed functions.</param>
        public LocationReferenceBuilder(LocationReferenceType locationReference, object inventoryId, object currentContextReleaseId, Configuration configuration, IRSystem7 mrsysSystem)
        {
            xsdObject = new Builder();
            Builder builder = (Builder) xsdObject;
            if (inventoryId != null)
            {
                builder.IntegrationKey = GetIntegrationKey(locationReference, inventoryId, currentContextReleaseId, mrsysSystem);
            }
            else
            {
                builder.IntegrationKey = GetIntegrationKey(locationReference, inventoryId, mrsysSystem);
            }
            builder.Name = configuration.EnvisionBuilderName;
            builder.NHTNumber = configuration.EnvisionNHTNumber;
        }

        /// <summary>
        /// Constructs the location reference objects.
        /// </summary>       
        /// <param name="locationReference">Specifies the location level: Corporation, Region, Division, Community, Release or Plan.</param>
        /// <param name="inventoryId">The pivotal id of the current organization or inventory object.</param>       
        /// <param name="configuration">Configration object to retrieve header information.</param>
        /// <param name="mrsysSystem">IRSystem object to access some needed functions.</param>
        public LocationReferenceBuilder(LocationReferenceType locationReference, object inventoryId, Configuration configuration, IRSystem7 mrsysSystem)
            :this(locationReference, inventoryId, null, configuration, mrsysSystem)
        {
        }

        
        /// <summary>
        /// Generate the Xml for the current location reference.  Overriding to include DataGenerated.
        /// </summary>
        /// <returns>Xml of the location reference.</returns>
        public override XmlNode ToXML()
        {
            XmlNode locationRefXmlNode = base.ToXML();
            XmlAttribute dateGeneratedAttribute = locationRefXmlNode.Attributes["DateGenerated"];

            locationRefXmlNode.Attributes.Remove(dateGeneratedAttribute);

            return locationRefXmlNode;
        }
    }
}
