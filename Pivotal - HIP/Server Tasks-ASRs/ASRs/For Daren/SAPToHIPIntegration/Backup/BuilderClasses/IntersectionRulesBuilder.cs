
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


namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated
{
    /// <summary>
    /// This helper class builds the hard rules object.  This object can be serialized to the IntersectionRule Xml element 
    /// necessary to create/edit hard rules through Envision web methods.  Alternatively in Ftp mode, this object is attached
    /// to the plan inventory object for serialization.
    /// </summary>
    class IntersectionRuleBuilder : BuilderBase
    {
        /// <summary>
        /// IntersectionRuleBuilder constructor.  
        /// </summary>
        /// <param name="hardRuleRst">Hard rule recordset.  Do not change cursor.</param>
        /// <param name="planAssignmentRst">The current plan assignment recordset.</param>
        /// <param name="currentContextReleaseRst">The current release record set.</param>
        /// <param name="optionCreationLevel">Option creation level.</param>
        /// <param name="corporateLocationNumber">Builder's corporate location number.</param>
        /// <param name="softDeactivate">Indicates to deactivate the rule if no longer applicable to the plan inventory.  For instance, one of the product configuration is deactivated for the plan.</param>
        /// <param name="mrsysSystem">IRSystem.</param>
        /// <param name="transportType">Transport type.</param>
        public IntersectionRuleBuilder(Recordset20 hardRuleRst, Recordset20 planAssignmentRst, Recordset20 currentContextReleaseRst, LocationReferenceType optionCreationLevel, string corporateLocationNumber, bool softDeactivate, IRSystem7 mrsysSystem, EnvisionIntegration.TransportType transportType)
        {
            const string IntersectionRuleBuilderClass = "IntersectionRuleBuilder class. ";

            InventoryTypeIntersectionRule iRule;
            object planAssignmentId = planAssignmentRst.Fields[NBHDPProductData.NBHDPProductIdField].Value;

            try
            {
                object currentContextReleaseId = currentContextReleaseRst.Fields[NBHDPhaseData.NBHDPhaseIdField].Value;        
                object divisionId = currentContextReleaseRst.Fields[NBHDPhaseData.DivisionIdField].Value;
                object regionId = currentContextReleaseRst.Fields[NBHDPhaseData.RegionIdField].Value;
                string locationLevel;
                string locationNumber;
                ArrayList iHardRuleOptionArray;
                IntersectionOptionType iHardRuleOption;

                //Location number is supposed to come directly from the Options, but to improve export performance,
                //this value is taken from the Plan to save a database read inside a loop.
                switch (optionCreationLevel)
                {
                    case LocationReferenceType.Corporate:
                        locationLevel = EnvisionIntegration.LocationLevel.CodeCorporation;
                        locationNumber = corporateLocationNumber;
                        break;
                    case LocationReferenceType.Region:
                        locationLevel = EnvisionIntegration.LocationLevel.CodeRegion;
                        locationNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(regionId));
                        break;
                    case LocationReferenceType.Division:
                        locationLevel = EnvisionIntegration.LocationLevel.CodeDivision;
                        locationNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(divisionId));
                        break;
                    default:
                        return;
                }

                if (transportType == EnvisionIntegration.TransportType.Ftp)
                {
                    iRule = new InventoryTypeIntersectionRule();
                }
                else
                {
                    iRule = new IntersectionRule();
                }

                

                iRule.RuleId = hardRuleRst.Fields[ProductOptionRuleData.ProductOptionRuleIdField].Value;
                iRule.Name = TypeConvert.ToString(hardRuleRst.Fields[ProductOptionRuleData.RnDescriptorField].Value);
                iRule.SoftDeactivate = softDeactivate;
                if (softDeactivate)
                    iRule.RnUpdate = (byte[])mrsysSystem.StringToId("0x0000000000000000");
                else
                    iRule.RnUpdate = hardRuleRst.Fields[ProductOptionRuleData.RnUpdateField].Value;
                iRule.IntersectionRuleNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(hardRuleRst.Fields[ProductOptionRuleData.ProductOptionRuleIdField].Value));
                if (softDeactivate || TypeConvert.ToBoolean(hardRuleRst.Fields[ProductOptionRuleData.InactiveField].Value))
                {
                    iRule.Deactivate = "1";

                    // Exclude prerequisite and postrequiste options if Deactivate = "1".  Please refer to Envision case # 1166.
                    // If the prerequisite or postrequiste option is already deactivated, deactivating the intersection rule
                    // will fail.  The workaround is to exclude the prerequisite and postrequiste in the rule's Xml.
                    iRule.PrerequisiteOptions = (IntersectionOptionType[])new ArrayList().ToArray(typeof(IntersectionOptionType));
                    iRule.PostrequisiteOptions = (IntersectionOptionType[])new ArrayList().ToArray(typeof(IntersectionOptionType));
                }
                else
                {
                    iRule.Deactivate = "0";

                    // Add Prerequisite Option.
                    iHardRuleOption = new IntersectionOptionType();
                    iHardRuleOption.OptionNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(hardRuleRst.Fields[ProductOptionRuleData.ParentProductIdField].Value));
                    iHardRuleOption.LocationLevel = locationLevel;
                    iHardRuleOption.LocationNumber = locationNumber;
                    iHardRuleOption.RoomNumber = "";
                    iHardRuleOptionArray = new ArrayList();
                    iHardRuleOptionArray.Add(iHardRuleOption);
                    iRule.PrerequisiteOptions = (IntersectionOptionType[])iHardRuleOptionArray.ToArray(typeof(IntersectionOptionType));

                    // Add Postrequiste Option.
                    iHardRuleOption = new IntersectionOptionType();
                    iHardRuleOption.OptionNumber = BuilderBase.CompactPivotalId(mrsysSystem.IdToString(hardRuleRst.Fields[ProductOptionRuleData.ChildProductIdField].Value));
                    iHardRuleOption.LocationLevel = locationLevel;
                    iHardRuleOption.LocationNumber = locationNumber;
                    iHardRuleOption.RoomNumber = "";
                    iHardRuleOptionArray = new ArrayList();
                    iHardRuleOptionArray.Add(iHardRuleOption);
                    iRule.PostrequisiteOptions = (IntersectionOptionType[])iHardRuleOptionArray.ToArray(typeof(IntersectionOptionType));
                }

                if (transportType == EnvisionIntegration.TransportType.WebService)
                {
                    string planIntegrationKey = BuilderBase.GetIntegrationKey(LocationReferenceType.Plan, planAssignmentId, currentContextReleaseId, mrsysSystem);
                    comments = System.String.Format("Assigning intersection rule to plan {0}[{1}] of release {2}:  ({3})[{4}]", planAssignmentRst.Fields[NBHDPProductData.ProductNameField].Value, planIntegrationKey, currentContextReleaseRst.Fields[NBHDPhaseData.RnDescriptorField].Value, iRule.Name, iRule.IntersectionRuleNumber);
                }

                xsdObject = iRule;

            }
            catch (Exception ex)
            {
                throw new PivotalApplicationException(IntersectionRuleBuilderClass, ex);
            }
        }
    }
}
