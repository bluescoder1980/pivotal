﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.832
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#pragma warning disable 1591

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=2.0.50727.42.
// 


namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Home
    {

        private HomeImage[] imagesField;

        private ConstructionStageType[] constructionStagesField;

        private SelectedOptionType[] selectedOptionsField;

        private string homeNumberField;

        private bool isSpecField;

        private string lotNumberField;

        private string lotCityField;

        private string lotStateField;

        private string lotZipField;

        private string currentConstructionStageField;

        private bool calculateConstructionStageField;

        private string lotAddressField;

        private decimal basePriceField;

        private bool basePriceFieldSpecified;

        private string floorPlanURLField;

        public Home()
        {
            this.isSpecField = false;
            this.calculateConstructionStageField = false;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Image", IsNullable = false)]
        public HomeImage[] Images
        {
            get
            {
                return this.imagesField;
            }
            set
            {
                this.imagesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("ConstructionStage", IsNullable = false)]
        public ConstructionStageType[] ConstructionStages
        {
            get
            {
                return this.constructionStagesField;
            }
            set
            {
                this.constructionStagesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("SelectedOption", IsNullable = false)]
        public SelectedOptionType[] SelectedOptions
        {
            get
            {
                return this.selectedOptionsField;
            }
            set
            {
                this.selectedOptionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string HomeNumber
        {
            get
            {
                return this.homeNumberField;
            }
            set
            {
                this.homeNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool IsSpec
        {
            get
            {
                return this.isSpecField;
            }
            set
            {
                this.isSpecField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LotNumber
        {
            get
            {
                return this.lotNumberField;
            }
            set
            {
                this.lotNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LotCity
        {
            get
            {
                return this.lotCityField;
            }
            set
            {
                this.lotCityField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LotState
        {
            get
            {
                return this.lotStateField;
            }
            set
            {
                this.lotStateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LotZip
        {
            get
            {
                return this.lotZipField;
            }
            set
            {
                this.lotZipField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CurrentConstructionStage
        {
            get
            {
                return this.currentConstructionStageField;
            }
            set
            {
                this.currentConstructionStageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool CalculateConstructionStage
        {
            get
            {
                return this.calculateConstructionStageField;
            }
            set
            {
                this.calculateConstructionStageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LotAddress
        {
            get
            {
                return this.lotAddressField;
            }
            set
            {
                this.lotAddressField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal BasePrice
        {
            get
            {
                return this.basePriceField;
            }
            set
            {
                this.basePriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BasePriceSpecified
        {
            get
            {
                return this.basePriceFieldSpecified;
            }
            set
            {
                this.basePriceFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FloorPlanURL
        {
            get
            {
                return this.floorPlanURLField;
            }
            set
            {
                this.floorPlanURLField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class HomeImage : ImageType
    {

        private HomeImageImageType imageType1Field;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("ImageType")]
        public HomeImageImageType ImageType1
        {
            get
            {
                return this.imageType1Field;
            }
            set
            {
                this.imageType1Field = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public enum HomeImageImageType
    {

        /// <remarks/>
        FloorPlan,

        /// <remarks/>
        Elevation,
    }

    /// <remarks/>
    //[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    //[System.SerializableAttribute()]
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class ImageType
    //{

    //    private string sequencePositionField;

    //    private string captionField;

    //    private string referenceTypeField;

    //    private string valueField;

    //    /// <remarks/>
    //    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
    //    public string SequencePosition
    //    {
    //        get
    //        {
    //            return this.sequencePositionField;
    //        }
    //        set
    //        {
    //            this.sequencePositionField = value;
    //        }
    //    }

    //    /// <remarks/>
    //    [System.Xml.Serialization.XmlAttributeAttribute()]
    //    public string Caption
    //    {
    //        get
    //        {
    //            return this.captionField;
    //        }
    //        set
    //        {
    //            this.captionField = value;
    //        }
    //    }

    //    /// <remarks/>
    //    [System.Xml.Serialization.XmlAttributeAttribute()]
    //    public string ReferenceType
    //    {
    //        get
    //        {
    //            return this.referenceTypeField;
    //        }
    //        set
    //        {
    //            this.referenceTypeField = value;
    //        }
    //    }

    //    /// <remarks/>
    //    [System.Xml.Serialization.XmlTextAttribute()]
    //    public string Value
    //    {
    //        get
    //        {
    //            return this.valueField;
    //        }
    //        set
    //        {
    //            this.valueField = value;
    //        }
    //    }
    //}

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class OptionType
    {

        private OptionTypeProduct productField;

        private string locationNumberField;

        private string locationLevelField;

        private string optionNumberField;

        /// <remarks/>
        public OptionTypeProduct Product
        {
            get
            {
                return this.productField;
            }
            set
            {
                this.productField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LocationNumber
        {
            get
            {
                return this.locationNumberField;
            }
            set
            {
                this.locationNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LocationLevel
        {
            get
            {
                return this.locationLevelField;
            }
            set
            {
                this.locationLevelField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OptionNumber
        {
            get
            {
                return this.optionNumberField;
            }
            set
            {
                this.optionNumberField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class OptionTypeProduct
    {

        private string gTINField;

        private string nHTManufacturerNumberField;

        private string productNumberField;

        private string dUNSNumberField;

        private string uCCCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string GTIN
        {
            get
            {
                return this.gTINField;
            }
            set
            {
                this.gTINField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NHTManufacturerNumber
        {
            get
            {
                return this.nHTManufacturerNumberField;
            }
            set
            {
                this.nHTManufacturerNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ProductNumber
        {
            get
            {
                return this.productNumberField;
            }
            set
            {
                this.productNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DUNSNumber
        {
            get
            {
                return this.dUNSNumberField;
            }
            set
            {
                this.dUNSNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UCCCode
        {
            get
            {
                return this.uCCCodeField;
            }
            set
            {
                this.uCCCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class NoteType
    {

        private NoteTypeType typeField;

        private string textField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public NoteTypeType Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public enum NoteTypeType
    {

        /// <remarks/>
        Color,

        /// <remarks/>
        Location,

        /// <remarks/>
        Style,

        /// <remarks/>
        Other,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SelectedOptionType
    {

        private SelectedOptionTypeProduct productField;

        private NoteType[] notesField;

        private OptionType[] optionField;

        private string locationNumberField;

        private string locationLevelField;

        private string optionNumberField;

        private decimal priceField;

        private bool displayPriceField;

        private string quantityField;

        private string roomNumberField;

        private System.DateTime transactionDateField;

        private bool preSelectedField;

        private bool envisionEditLockField;

        private SelectedOptionTypeOptionType optionTypeField;

        private bool optionTypeFieldSpecified;

        private string optionNameField;

        private string optionDescriptionField;

        private bool validateProductLinksField;

        private string categoryNumberField;

        //AB Quick Development Fix
        private bool validateAvailabilityField;

        public SelectedOptionType()
        {
            this.priceField = ((decimal)(0m));
            this.displayPriceField = true;
            this.quantityField = "1";
            this.preSelectedField = false;
            this.envisionEditLockField = false;
            this.validateProductLinksField = true;
        }

        /// <remarks/>
        public SelectedOptionTypeProduct Product
        {
            get
            {
                return this.productField;
            }
            set
            {
                this.productField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Note", IsNullable = false)]
        public NoteType[] Notes
        {
            get
            {
                return this.notesField;
            }
            set
            {
                this.notesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Option")]
        public OptionType[] Option
        {
            get
            {
                return this.optionField;
            }
            set
            {
                this.optionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LocationNumber
        {
            get
            {
                return this.locationNumberField;
            }
            set
            {
                this.locationNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LocationLevel
        {
            get
            {
                return this.locationLevelField;
            }
            set
            {
                this.locationLevelField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OptionNumber
        {
            get
            {
                return this.optionNumberField;
            }
            set
            {
                this.optionNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(typeof(decimal), "0")]
        public decimal Price
        {
            get
            {
                return this.priceField;
            }
            set
            {
                this.priceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool DisplayPrice
        {
            get
            {
                return this.displayPriceField;
            }
            set
            {
                this.displayPriceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        [System.ComponentModel.DefaultValueAttribute("1")]
        public string Quantity
        {
            get
            {
                return this.quantityField;
            }
            set
            {
                this.quantityField = value;
            }
        }

        /// <remarks/>
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

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime TransactionDate
        {
            get
            {
                return this.transactionDateField;
            }
            set
            {
                this.transactionDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool PreSelected
        {
            get
            {
                return this.preSelectedField;
            }
            set
            {
                this.preSelectedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool EnvisionEditLock
        {
            get
            {
                return this.envisionEditLockField;
            }
            set
            {
                this.envisionEditLockField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public SelectedOptionTypeOptionType OptionType
        {
            get
            {
                return this.optionTypeField;
            }
            set
            {
                this.optionTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OptionTypeSpecified
        {
            get
            {
                return this.optionTypeFieldSpecified;
            }
            set
            {
                this.optionTypeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OptionName
        {
            get
            {
                return this.optionNameField;
            }
            set
            {
                this.optionNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string OptionDescription
        {
            get
            {
                return this.optionDescriptionField;
            }
            set
            {
                this.optionDescriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool ValidateProductLinks
        {
            get
            {
                return this.validateProductLinksField;
            }
            set
            {
                this.validateProductLinksField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CategoryNumber
        {
            get
            {
                return this.categoryNumberField;
            }
            set
            {
                this.categoryNumberField = value;
            }
        }
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        //[System.ComponentModel.DefaultValueAttribute(true)]
        public bool ValidateAvailability
        {
            get
            {
                return this.validateAvailabilityField;
            }
            set
            {
                this.validateAvailabilityField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SelectedOptionTypeProduct
    {

        private string gTINField;

        private string nHTManufacturerNumberField;

        private string productNumberField;

        private string dUNSNumberField;

        private string uCCCodeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string GTIN
        {
            get
            {
                return this.gTINField;
            }
            set
            {
                this.gTINField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NHTManufacturerNumber
        {
            get
            {
                return this.nHTManufacturerNumberField;
            }
            set
            {
                this.nHTManufacturerNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ProductNumber
        {
            get
            {
                return this.productNumberField;
            }
            set
            {
                this.productNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DUNSNumber
        {
            get
            {
                return this.dUNSNumberField;
            }
            set
            {
                this.dUNSNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UCCCode
        {
            get
            {
                return this.uCCCodeField;
            }
            set
            {
                this.uCCCodeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public enum SelectedOptionTypeOptionType
    {

        /// <remarks/>
        Custom,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ConstructionStageType
    {

        private string constructionStageField;

        private System.DateTime dateField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ConstructionStage
        {
            get
            {
                return this.constructionStageField;
            }
            set
            {
                this.constructionStageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public System.DateTime Date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }
    }

}