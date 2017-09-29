//
// $Workfile: BuyerWebService.cs$
// $Revision: 2$
// $Author: RYong$
// $Date: Wednesday, December 19, 2007 11:57:50 AM$
//
// Copyright © Pivotal Corporation
//
#pragma warning disable 1591


namespace Envision.DesignCenterManager.Buyer {
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;



    ///<remarks>
    /// This class is the proxy class for the 1.8 version of the Envision Buyer Web Service
    /// All asyncronous capabilities have been commented out.
    ///</remarks>
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="BuyerWebServiceSoap", Namespace="http://newhometechnologies.com/envision/")]
        public partial class BuyerWebService : CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionHttpClientProtocol 
        {

        private AuthHeader authHeaderValueField;
        
        
            /// <remarks/>
        public  BuyerWebService(CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionIntegration envisionIntegration)
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
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateBuyer", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode CreateBuyer(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument, string HomeNumber, bool AutoActivateBuyer)
            {
            object[] results = this.Invoke("CreateBuyer", new object[] {
                        LocationReference,
                        BuyerDocument,
                        HomeNumber,
                        AutoActivateBuyer});
                return (System.Xml.XmlNode)results[0];
        }
        
       
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="CreateBuyer1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateBuyer2", RequestElementName="CreateBuyer2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateBuyer2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateBuyer2Result")]
        public virtual System.Xml.XmlNode CreateBuyer(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument, string HomeNumber, bool AutoActivateBuyer, string WelcomeEmailCopyAddress)
            {
            object[] results = this.Invoke("CreateBuyer1", new object[] {
                        LocationReference,
                        BuyerDocument,
                        HomeNumber,
                        AutoActivateBuyer,
                        WelcomeEmailCopyAddress});
                return (System.Xml.XmlNode)results[0];
        }
        
        
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateBuyer", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode UpdateBuyer(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument)
            {
            object[] results = this.Invoke("UpdateBuyer", new object[] {
                        LocationReference,
                        BuyerDocument});
                return (System.Xml.XmlNode)results[0];
        }
        
        
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateBuyerStatus", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode UpdateBuyerStatus(System.Xml.XmlNode LocationReference, string BuyerNumber, string BuyerStatus)
            {
            object[] results = this.Invoke("UpdateBuyerStatus", new object[] {
                        LocationReference,
                        BuyerNumber,
                        BuyerStatus});
                return (System.Xml.XmlNode)results[0];
        }
        
        
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/AssignNewHometoBuyer", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode AssignNewHometoBuyer(System.Xml.XmlNode LocationReference, string BuyerNumber, string HomeNumber, bool TransferoldSelectionsToWishlist)
            {
            object[] results = this.Invoke("AssignNewHometoBuyer", new object[] {
                        LocationReference,
                        BuyerNumber,
                        HomeNumber,
                        TransferoldSelectionsToWishlist});
                return (System.Xml.XmlNode)results[0];
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
        public virtual string Password
        {
            get {
                return this.passwordField;
            }
            set {
                this.passwordField = value;
            }
        }
        
        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId = "Member")]
        public virtual string NHTBillingNumber
        {
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
