//
// $Workfile: OptionsManagerWebService.cs$
// $Revision: 2$
// $Author: RYong$
// $Date: Wednesday, December 19, 2007 11:57:49 AM$
//
// Copyright © Pivotal Corporation
//
#pragma warning disable 1591

namespace Envision.OptionsManager {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;



    ///<remarks>
    /// This class is the proxy class for the 1.8 version of the Envision Options Manager Web Service
    /// All asyncronous capabilities have been commented out.
    ///</remarks>
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="OptionsManagerServiceSoap", Namespace="http://newhometechnologies.com/envision/")]
        public partial class OptionsManagerService : CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionHttpClientProtocol
        {
        
        private AuthHeader authHeaderValueField;
                
        
            /// <remarks/>
            public OptionsManagerService(CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionIntegration envisionIntegration)
                : base(envisionIntegration)
            {
                this.RequestEncoding = System.Text.Encoding.UTF8;
            }

            public virtual AuthHeader AuthHeaderValue
            {
            get {
                return this.authHeaderValueField;
            }
            set {
                this.authHeaderValueField = value;
            }
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateInventory2", RequestElementName="CreateInventory2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateInventory2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateInventory2Result")]
        public virtual System.Xml.XmlNode CreateInventory(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Inventory)
            {
            object[] results = this.Invoke("CreateInventory", new object[] {
                        LocationReference,
                        Inventory});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="CreateInventory1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateInventory", RequestElementName="CreateInventory", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateInventoryResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateInventoryResult")]
        public virtual System.Xml.XmlNode CreateInventory(string LocationReference, string Inventory)
            {
            object[] results = this.Invoke("CreateInventory1", new object[] {
                        LocationReference,
                        Inventory});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/EditInventory2", RequestElementName="EditInventory2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="EditInventory2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("EditInventory2Result")]
        public virtual System.Xml.XmlNode EditInventory(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Inventory)
            {
            object[] results = this.Invoke("EditInventory", new object[] {
                        LocationReference,
                        Inventory});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="EditInventory1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/EditInventory", RequestElementName="EditInventory", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="EditInventoryResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("EditInventoryResult")]
        public virtual System.Xml.XmlNode EditInventory(string LocationReference, string Inventory)
            {
            object[] results = this.Invoke("EditInventory1", new object[] {
                        LocationReference,
                        Inventory});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/DeactivateInventory2", RequestElementName="DeactivateInventory2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="DeactivateInventory2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("DeactivateInventory2Result")]
        public virtual System.Xml.XmlNode DeactivateInventory(System.Xml.XmlNode LocationReference, string LocationName, string LocationLevel)
            {
            object[] results = this.Invoke("DeactivateInventory", new object[] {
                        LocationReference,
                        LocationName,
                        LocationLevel});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="DeactivateInventory1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/DeactivateInventory", RequestElementName="DeactivateInventory", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="DeactivateInventoryResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("DeactivateInventoryResult")]
        public virtual System.Xml.XmlNode DeactivateInventory(string LocationReference, string LocationName, string LocationLevel)
            {
            object[] results = this.Invoke("DeactivateInventory1", new object[] {
                        LocationReference,
                        LocationName,
                        LocationLevel});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateEditRooms2", RequestElementName="CreateEditRooms2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateEditRooms2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateEditRooms2Result")]
        public virtual System.Xml.XmlNode CreateEditRooms(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Rooms)
            {
            object[] results = this.Invoke("CreateEditRooms", new object[] {
                        LocationReference,
                        Rooms});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="CreateEditRooms1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateEditRooms", RequestElementName="CreateEditRooms", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateEditRoomsResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateEditRoomsResult")]
        public virtual System.Xml.XmlNode CreateEditRooms(string LocationReference, string Rooms)
            {
            object[] results = this.Invoke("CreateEditRooms1", new object[] {
                        LocationReference,
                        Rooms});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateOptionCategories", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode UpdateOptionCategories(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Categories)
            {
            object[] results = this.Invoke("UpdateOptionCategories", new object[] {
                        LocationReference,
                        Categories});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateOption2", RequestElementName="CreateOption2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateOption2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateOption2Result")]
        public virtual System.Xml.XmlNode CreateOption(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Option)
            {
            object[] results = this.Invoke("CreateOption", new object[] {
                        LocationReference,
                        Option});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="CreateOption1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateOption", RequestElementName="CreateOption", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateOptionResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateOptionResult")]
        public virtual System.Xml.XmlNode CreateOption(string LocationReference, string Option)
            {
            object[] results = this.Invoke("CreateOption1", new object[] {
                        LocationReference,
                        Option});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/EditOption2", RequestElementName="EditOption2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="EditOption2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("EditOption2Result")]
        public virtual System.Xml.XmlNode EditOption(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Option)
            {
            object[] results = this.Invoke("EditOption", new object[] {
                        LocationReference,
                        Option});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="EditOption1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/EditOption", RequestElementName="EditOption", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="EditOptionResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("EditOptionResult")]
        public virtual System.Xml.XmlNode EditOption(string LocationReference, string Option)
            {
            object[] results = this.Invoke("EditOption1", new object[] {
                        LocationReference,
                        Option});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/DeactivateOption2", RequestElementName="DeactivateOption2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="DeactivateOption2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("DeactivateOption2Result")]
        public virtual System.Xml.XmlNode DeactivateOption(System.Xml.XmlNode LocationReference, string OptionNumber)
            {
            object[] results = this.Invoke("DeactivateOption", new object[] {
                        LocationReference,
                        OptionNumber});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="DeactivateOption1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/DeactivateOption", RequestElementName="DeactivateOption", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="DeactivateOptionResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("DeactivateOptionResult")]
        public virtual System.Xml.XmlNode DeactivateOption(string LocationReference, string OptionNumber)
            {
            object[] results = this.Invoke("DeactivateOption1", new object[] {
                        LocationReference,
                        OptionNumber});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateEditOptionAssignments2", RequestElementName="CreateEditOptionAssignments2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateEditOptionAssignments2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateEditOptionAssignments2Result")]
        public virtual System.Xml.XmlNode CreateEditOptionAssignments(System.Xml.XmlNode LocationReference, System.Xml.XmlNode OptionAssignments)
            {
            object[] results = this.Invoke("CreateEditOptionAssignments", new object[] {
                        LocationReference,
                        OptionAssignments});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="CreateEditOptionAssignments1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateEditOptionAssignments", RequestElementName="CreateEditOptionAssignments", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateEditOptionAssignmentsResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateEditOptionAssignmentsResult")]
        public virtual System.Xml.XmlNode CreateEditOptionAssignments(string LocationReference, string OptionAssignments)
            {
            object[] results = this.Invoke("CreateEditOptionAssignments1", new object[] {
                        LocationReference,
                        OptionAssignments});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateIntersectionRule", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode CreateIntersectionRule(System.Xml.XmlNode LocationReference, System.Xml.XmlNode IntersectionRule)
            {
            object[] results = this.Invoke("CreateIntersectionRule", new object[] {
                        LocationReference,
                        IntersectionRule});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
       
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/EditIntersectionRule", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
            public virtual System.Xml.XmlNode EditIntersectionRule(System.Xml.XmlNode LocationReference, System.Xml.XmlNode IntersectionRule)
            {
            object[] results = this.Invoke("EditIntersectionRule", new object[] {
                        LocationReference,
                        IntersectionRule});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://newhometechnologies.com/envision/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://newhometechnologies.com/envision/", IsNullable=false)]
    public partial class AuthHeader : System.Web.Services.Protocols.SoapHeader {
        
        private string userNameField;
        
        private string passwordField;
        
        private string m_NHTBillingNumberField;
        
        /// <remarks/>
        public virtual string UserName
        {
            get {
                return this.userNameField;
            }
            set {
                this.userNameField = value;
            }
        }
        
        /// <remarks/>
        public virtual string Password {
            get {
                return this.passwordField;
            }
            set {
                this.passwordField = value;
            }
        }
        
        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId = "Member")]
        public virtual string NHTBillingNumber {
            get {
                return this.m_NHTBillingNumberField;
            }
            set {
                this.m_NHTBillingNumberField = value;
            }
        }
    }

  
}

#pragma warning restore 1591
