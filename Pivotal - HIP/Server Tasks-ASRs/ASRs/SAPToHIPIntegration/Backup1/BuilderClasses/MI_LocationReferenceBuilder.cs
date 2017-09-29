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
    public enum MI_LocationReferenceType
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
        Plan,

        /// <summary>
        /// ElevationLevel level
        /// </summary>
        Elevation
    }
   
    /// <summary>
    /// Defines 6 location levels
    /// </summary>
    /// <summary>
    /// Builds Location Reference which is used by web service to point to the location 
    /// where the web service operation should happen.
    /// </summary>
    class MI_LocationReferenceBuilder:BuilderBase
    { 
        /// <summary>
        /// Constructs the location reference objects.
        /// </summary>
        /// <param name="locationReference">Specifies the location level: Corporation, Region, Division, Community, Release or Plan.</param>
        /// <param name="inventoryId">The pivotal id of the current organization or inventory object.</param>
        /// <param name="currentContextReleaseId">The current release id being processed for a release wildcarded plan assignment.</param>
        /// <param name="configuration">Configration object to retrieve header information.</param>
        /// <param name="mrsysSystem">IRSystem object to access some needed functions.</param>
        public MI_LocationReferenceBuilder(MI_LocationReferenceType locationReference, object inventoryId, object currentContextReleaseId, Configuration configuration, IRSystem7 mrsysSystem)
        {
            xsdObject = new Builder();
            Builder builder = (Builder) xsdObject;
            if (inventoryId != null)
            {
                //Integration keys will not be sent with the buyer for defining the division or the home for defining the plan
                //builder.IntegrationKey = GetIntegrationKey(locationReference, inventoryId, currentContextReleaseId, mrsysSystem);
            }
            else
            {
                //builder.IntegrationKey = GetIntegrationKey(locationReference, inventoryId, mrsysSystem);
            }
            builder.Name = configuration.EnvisionBuilderName;
            builder.NHTNumber = configuration.EnvisionNHTNumber;

            //go to the division level
            //create organization element
            OrganizationType corpOrg = new OrganizationType();
            corpOrg.Name = configuration.EnvisionBuilderName;
            corpOrg.LocationNumber = "100";
            corpOrg.LocationLevel = EnvisionIntegration.LocationLevel.CodeCorporation.ToUpper();

            OrganizationType divOrg = new OrganizationType();
            //retrieve division specifics
            MI_Envision_Utility util = new MI_Envision_Utility();

            if (locationReference.Equals(MI_LocationReferenceType.Division))
            {
                string[] divisionInfo = util.GetDivisionDetail(inventoryId, mrsysSystem);
                divOrg.Name = divisionInfo[0];
                divOrg.LocationNumber = divisionInfo[1];
                divOrg.LocationLevel = EnvisionIntegration.LocationLevel.CodeDivision.ToUpper();

                OrganizationType[] divOrgArray = { divOrg };
                corpOrg.Organization = divOrgArray;
            }
                     
                     
            //check to see how deep to build XML
            if (locationReference.Equals (MI_LocationReferenceType.Elevation))
            {
                //This is a plan so dig down to the elevation level
                //retrieve the phase information
                object[] phaseInfo = util.GetPhaseDetail(currentContextReleaseId, mrsysSystem);
                
                //populate division information
                string[] divisionInfo = util.GetDivisionDetail(phaseInfo[3], mrsysSystem);
                divOrg.Name = divisionInfo[0];
                divOrg.LocationNumber = divisionInfo[1];
                divOrg.LocationLevel = EnvisionIntegration.LocationLevel.CodeDivision.ToUpper();

                OrganizationType[] divOrgArray = { divOrg };

                //retrieve the community information
                string [] communityInfo = util.GetCommunityDetail(phaseInfo[2],mrsysSystem);
                OrganizationTypeInventory communityType = new OrganizationTypeInventory();
                communityType.Name = communityInfo[0];
                communityType.LocationNumber = communityInfo[1];
                communityType.LocationLevel = EnvisionIntegration.LocationLevel.CodeCommunity.ToUpper();
                                
                //set the phase information
                InventoryType phaseType = new InventoryType();
                phaseType.Name = (string)phaseInfo[0];
                phaseType.LocationNumber = (string)phaseInfo[1];
                //2008-01-09 AB MI specific
                //phaseType.LocationLevel = EnvisionIntegration.LocationLevel.CodeRelease.ToUpper();
                phaseType.LocationLevel = "PHA";

                //get plan and elevation info
                string[] planInfo = util.GetPlanDetail(inventoryId, mrsysSystem);
                InventoryType planType = new InventoryType();
                planType.Name = planInfo[0];
                planType.LocationNumber = planInfo[1];
                //2008-01-09 AB MI specific
                //planType.LocationLevel = EnvisionIntegration.LocationLevel.CodePlan.ToUpper();
                planType.LocationLevel = "PLAN";
                

                InventoryType elevType = new InventoryType();
                elevType.Name = planInfo[1] + " - " + planInfo[2];
                elevType.LocationNumber = planInfo[2];
                elevType.LocationLevel = "ELEV";
                
                InventoryType[] elevArray = { elevType };
                planType.Inventory = elevArray;
                
                InventoryType[] planArray = { planType };
                phaseType.Inventory = planArray;

                InventoryType[] phaseArray = { phaseType };
                communityType.Inventory = phaseArray;

                OrganizationTypeInventory[] communityArray = { communityType };
                divOrg.Inventory = communityArray;

                corpOrg.Organization = divOrgArray;
            }
            builder.Organization = corpOrg;
        }

        /// <summary>
        /// Constructs the location reference objects.
        /// </summary>       
        /// <param name="locationReference">Specifies the location level: Corporation, Region, Division, Community, Release or Plan.</param>
        /// <param name="inventoryId">The pivotal id of the current organization or inventory object.</param>       
        /// <param name="configuration">Configration object to retrieve header information.</param>
        /// <param name="mrsysSystem">IRSystem object to access some needed functions.</param>
        public MI_LocationReferenceBuilder(MI_LocationReferenceType locationReference, object inventoryId, Configuration configuration, IRSystem7 mrsysSystem)
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
