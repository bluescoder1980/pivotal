//
// $Workfile: HomeWebService.cs$
// $Revision: 2$
// $Author: RYong$
// $Date: Wednesday, December 19, 2007 11:57:49 AM$
//
// Copyright © Pivotal Corporation
//

#pragma warning disable 1591
namespace Envision.DesignCenterManager.Home
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;

    ///<remarks>
    /// This class is the proxy class for the 1.8 version of the Envision Home Web Service
    /// All asyncronous capabilities have been commented out.
    ///</remarks>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "HomeWebServiceSoap", Namespace = "http://newhometechnologies.com/envision/")]
    public partial class HomeWebService : CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionHttpClientProtocol
    {

        private AuthHeader authHeaderValueField;



        /// <remarks/>
        public HomeWebService(CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionIntegration envisionIntegration)
            : base(envisionIntegration)
        {
            this.RequestEncoding = System.Text.Encoding.UTF8;
        }

        public virtual AuthHeader AuthHeaderValue
        {
            get
            {
                return this.authHeaderValueField;
            }
            set
            {
                this.authHeaderValueField = value;
            }
        }


        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateHome", RequestNamespace = "http://newhometechnologies.com/envision/", ResponseNamespace = "http://newhometechnologies.com/envision/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode CreateHome(System.Xml.XmlNode LocationReference, System.Xml.XmlNode HomeDocument)
        {
            object[] results = this.Invoke("CreateHome", new object[] {
                        LocationReference,
                        HomeDocument});
            return (System.Xml.XmlNode)results[0];
        }


        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateHome", RequestNamespace = "http://newhometechnologies.com/envision/", ResponseNamespace = "http://newhometechnologies.com/envision/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode UpdateHome(System.Xml.XmlNode LocationReference, System.Xml.XmlNode HomeDocument)
        {
            object[] results = this.Invoke("UpdateHome", new object[] {
                        LocationReference,
                        HomeDocument});
            return (System.Xml.XmlNode)results[0];
        }



        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateHomeStatus", RequestNamespace = "http://newhometechnologies.com/envision/", ResponseNamespace = "http://newhometechnologies.com/envision/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode UpdateHomeStatus(System.Xml.XmlNode LocationReference, string HomeNumber, string HomeStatus, bool DeleteEnvisionSelectedOptions, bool DeleteExternalSelectedOptions)
        {
            object[] results = this.Invoke("UpdateHomeStatus", new object[] {
                        LocationReference,
                        HomeNumber,
                        HomeStatus,
                        DeleteEnvisionSelectedOptions,
                        DeleteExternalSelectedOptions});
            return (System.Xml.XmlNode)results[0];
        }



        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateConstructionStage", RequestNamespace = "http://newhometechnologies.com/envision/", ResponseNamespace = "http://newhometechnologies.com/envision/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode UpdateConstructionStage(System.Xml.XmlNode LocationReference, string HomeNumber, string ConstructionStage)
        {
            object[] results = this.Invoke("UpdateConstructionStage", new object[] {
                        LocationReference,
                        HomeNumber,
                        ConstructionStage});
            return (System.Xml.XmlNode)results[0];
        }



        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/RefreshPrices", RequestNamespace = "http://newhometechnologies.com/envision/", ResponseNamespace = "http://newhometechnologies.com/envision/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode RefreshPrices(System.Xml.XmlNode LocationReference, string HomeNumber, bool EnvisionSelectedOptions, bool ExternalSelectedOptions, bool PreSelectedOptions)
        {
            object[] results = this.Invoke("RefreshPrices", new object[] {
                        LocationReference,
                        HomeNumber,
                        EnvisionSelectedOptions,
                        ExternalSelectedOptions,
                        PreSelectedOptions});
            return (System.Xml.XmlNode)results[0];
        }



        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "0#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateSelectionStatus", RequestNamespace = "http://newhometechnologies.com/envision/", ResponseNamespace = "http://newhometechnologies.com/envision/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode UpdateSelectionStatus(int TransactionID, string Status)
        {
            object[] results = this.Invoke("UpdateSelectionStatus", new object[] {
                        TransactionID,
                        Status});
            return (System.Xml.XmlNode)results[0];
        }



        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/ChangeHomePlan", RequestNamespace = "http://newhometechnologies.com/envision/", ResponseNamespace = "http://newhometechnologies.com/envision/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode ChangeHomePlan(System.Xml.XmlNode LocationReference, string homeNumber, System.Xml.XmlNode newLocationReference)
        {
            object[] results = this.Invoke("ChangeHomePlan", new object[] {
                        LocationReference,
                        homeNumber,
                        newLocationReference});
            return (System.Xml.XmlNode)results[0];
        }

        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/DeleteSelection", RequestNamespace = "http://newhometechnologies.com/envision/", ResponseNamespace = "http://newhometechnologies.com/envision/", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public virtual System.Xml.XmlNode DeleteSelection(System.Xml.XmlNode LocationReference, string HomeNumber, string RoomNumber, string LocationNumber, string LocationLevel, string OptionNumber)
        {
            object[] results = this.Invoke("DeleteSelection", new object[] {
                        LocationReference,
                        HomeNumber,
                        RoomNumber,
                        LocationNumber,
                        LocationLevel,
                        OptionNumber});
            return (System.Xml.XmlNode)results[0];
        }

    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://newhometechnologies.com/envision/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://newhometechnologies.com/envision/", IsNullable = false)]
    public partial class AuthHeader : System.Web.Services.Protocols.SoapHeader
    {

        private string userNameField;

        private string passwordField;

        private string m_NHTBillingNumberField;

        /// <remarks/>
        public virtual string UserName
        {
            get
            {
                return this.userNameField;
            }
            set
            {
                this.userNameField = value;
            }
        }

        /// <remarks/>
        public virtual string Password
        {
            get
            {
                return this.passwordField;
            }
            set
            {
                this.passwordField = value;
            }
        }

        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId = "Member")]
        public virtual string NHTBillingNumber
        {
            get
            {
                return this.m_NHTBillingNumberField;
            }
            set
            {
                this.m_NHTBillingNumberField = value;
            }
        }
    }


}

#pragma warning restore 1591
