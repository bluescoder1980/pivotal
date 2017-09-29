using Pivotal.Interop.ADODBLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

/// <summary>
/// This file contains extended classes and partial classes to the XSD generated classes.  
/// The XSD generated classes are generally left alone so that they can be regenerated
/// in the future without overwritting the extended features.
/// </summary>
namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated
{

    /// <summary>
    /// Extended the Builder class to inclue IntegrationKey so that the Builder class can also be 
    /// used as LocationReference for web method calls.
    /// </summary>
    public partial class Builder
    {       
        private string integrationKey;

        /// <summary>
        /// IntegrationKey is a unique key find the organization or inventory record in Pivotal.
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()] 
        public string IntegrationKey
        {
            get
            {
                return this.integrationKey;
            }
            set
            {
                this.integrationKey = value;
            }
        }
    }


    ///// <summary>
    ///// Extended the InventoryType to serialize to "Inventory".  This matches the Envision schema.
    ///// </summary>
    [XmlRoot(ElementName = "Inventory")]
    public partial class InventoryType
    {
    }

    ///// <summary>
    ///// Extended the OrganizationTypeInventory to serialize to "Inventory".  This matches the Envision schema.
    ///// </summary>
    [XmlRoot(ElementName = "Inventory")]
    public partial class OrganizationTypeInventory : InventoryType
    {
    }

    
    ///// <summary>
    ///// Extended the OrganizationTypeInventory to the new name "Inventory", so that the
    ///// class can be serialized correctly for web method calls.
    ///// </summary>
    //[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    //[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    //public partial class Inventory : OrganizationTypeInventory
    //{
    //}


    /// <summary>
    /// Extended the DesignOptionType to the new name "Option", so that the
    /// class can be serialized correctly for web method calls.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Option : DesignOptionType
    {
    }


    /// <summary>
    /// Added "OptionRuleUpdate" to capture rule ids and rn_update values.
    /// </summary>
    public partial class DesignOptionType
    {
        /// <summary>
        /// Array of option rule ids and RnUpdate values 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public object[,] OptionRuleUpdate;

    }



    /// <summary>
    /// Extended the InventoryTypeIntersectionRule to the new name "IntersectionRule", so that the
    /// class can be serialized correctly for web method calls.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class IntersectionRule : InventoryTypeIntersectionRule
    {
    }


    /// <summary>
    /// Added "serialization ignored" fields to aid tracking of prerequisite and postrequisite options.
    /// </summary>
    public partial class IntersectionOptionType
    {

        /// <summary>
        /// Prerequisite Option Id
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public object PrerequisiteOptionId;

        /// <summary>
        /// Postrequisite Option Id
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public object PostrequisiteOptionId;

    }


    /// <summary>
    /// Added "serialization ignored" fields to aid tracking of a rule.
    /// </summary>
    public partial class InventoryTypeIntersectionRule
    {
        /// <summary>
        /// Product Option Rule Id of this intersection rule.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public object RuleId;

        /// <summary>
        /// Rn_Update value of the Product Option Rule record.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public object RnUpdate;

        /// <summary>
        /// Name of the intersection rule.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public string Name;

        /// <summary>
        /// SoftDeactivate set to true of one of the intersection options isn't assigned to the plan inventory.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public bool SoftDeactivate;
    }

    /// <summary>
    /// Added "serialization ignored" fields to aid tracking of changed option assignments.
    /// </summary>
    public partial class OptionAssignmentType
    {
        private string roomNumberField;

        /// <summary>
        /// Rn_Update value of the option assignment.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public byte[] RnUpdateOption;

        /// <summary>
        /// Rn_Update value of the Division_Product_Locations record.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public byte[] RnUpdateRoom;

        /// <summary>
        /// Division Product Id of the option.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public byte[] OptionId;

        /// <summary>
        /// SoftDeactivate sets to true if the option assignment is no longer applicable to this plan inventory.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public Boolean SoftDeactivate;

        /// <summary>
        /// Division Product Locations Id of the room.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public object RoomId;


        /// <summary>
        /// Compact room number of the room.
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RoomNumber
        {
            get
            {
                return this.roomNumberField;
            }
            set
            {
                this.roomNumberField = value;
            }
        }
    }

    public partial class DesignOptionTypeIncludedOption
    {
        /// <summary>
        /// Rn_Update value of the option.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public object RnUpdate;

        /// <summary>
        /// Neighborhood Product id of the package.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public object PackageComponentId;
    }

    public partial class DesignOptionTypeRule
    {
        /// <summary>
        /// Rn_Update value of the Product Option Rule.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public object RnUpdate;

        /// <summary>
        /// Product Option Rule Id of the rule.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public object OptionRuleId;

        /// <summary>
        /// Inactive value of the Product Option Rule.  
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public bool Inactive;

    }

    /// <summary>
    /// Added "serialization ignored" fields to aid tracking of changed rooms.
    /// </summary>
    public partial class RoomType
    {
        /// <summary>
        /// Rn_Update of the Location record.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public byte[] RnUpdateLocation;

        /// <summary>
        /// Rn_Update of the Division Product Locations record.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public byte[] RnUpdateDPLocation;

        /// <summary>
        /// Location Id of the room.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public byte[] LocationId;

        /// <summary>
        /// Division Product Locations Id of an instance of the room assigned to the Plan Division Product.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public byte[] DPLocationId;
    }


    /// <summary>
    /// List of rooms.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class Rooms
    {

        private RoomType[] roomField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Room")]
        public RoomType[] Room
        {
            get
            {
                return this.roomField;
            }
            set
            {
                this.roomField = value;
            }
        }
    }

}
