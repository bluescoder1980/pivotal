//
// $Workfile: HomeWebService.cs$
// $Revision: 10$
// $Author: tlyne$
// $Date: Thursday, April 05, 2007 12:58:13 PM$
//
// Copyright © Pivotal Corporation
//

#pragma warning disable 1591
namespace Envision.DesignCenterManager.Home {
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
    [System.Web.Services.WebServiceBindingAttribute(Name="HomeWebServiceSoap", Namespace="http://newhometechnologies.com/envision/")]
        public partial class HomeWebService : CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionHttpClientProtocol
        {
        
        private AuthHeader authHeaderValueField;
        
        //private System.Threading.SendOrPostCallback CreateHomeOperationCompleted;
        
        //private System.Threading.SendOrPostCallback UpdateHomeOperationCompleted;
        
        //private System.Threading.SendOrPostCallback UpdateHomeStatusOperationCompleted;
        
        //private System.Threading.SendOrPostCallback UpdateConstructionStageOperationCompleted;
        
        //private System.Threading.SendOrPostCallback RefreshPricesOperationCompleted;
        
        //private System.Threading.SendOrPostCallback UpdateSelectionStatusOperationCompleted;
        
        //private System.Threading.SendOrPostCallback ChangeHomePlanOperationCompleted;
        
            /// <remarks/>
            public HomeWebService(CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionIntegration envisionIntegration)
                : base(envisionIntegration) 
            {
                this.RequestEncoding = System.Text.Encoding.UTF8;
            }
        
        public AuthHeader AuthHeaderValue {
            get {
                return this.authHeaderValueField;
            }
            set {
                this.authHeaderValueField = value;
            }
        }
        
        ///// <remarks/>
        //public event CreateHomeCompletedEventHandler CreateHomeCompleted;
        
        ///// <remarks/>
        //public event UpdateHomeCompletedEventHandler UpdateHomeCompleted;
        
        ///// <remarks/>
        //public event UpdateHomeStatusCompletedEventHandler UpdateHomeStatusCompleted;
        
        ///// <remarks/>
        //public event UpdateConstructionStageCompletedEventHandler UpdateConstructionStageCompleted;
        
        ///// <remarks/>
        //public event RefreshPricesCompletedEventHandler RefreshPricesCompleted;
        
        ///// <remarks/>
        //public event UpdateSelectionStatusCompletedEventHandler UpdateSelectionStatusCompleted;
        
        ///// <remarks/>
        //public event ChangeHomePlanCompletedEventHandler ChangeHomePlanCompleted;
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateHome", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode CreateHome(System.Xml.XmlNode LocationReference, System.Xml.XmlNode HomeDocument) {
            object[] results = this.Invoke("CreateHome", new object[] {
                        LocationReference,
                        HomeDocument});
                return (System.Xml.XmlNode)results[0];
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginCreateHome(System.Xml.XmlNode LocationReference, System.Xml.XmlNode HomeDocument, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("CreateHome", new object[] {
        //                LocationReference,
        //                HomeDocument}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndCreateHome(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void CreateHomeAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode HomeDocument) {
        //    this.CreateHomeAsync(LocationReference, HomeDocument, null);
        //}
        
        ///// <remarks/>
        //public void CreateHomeAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode HomeDocument, object userState) {
        //    if ((this.CreateHomeOperationCompleted == null)) {
        //        this.CreateHomeOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateHomeOperationCompleted);
        //    }
        //    this.InvokeAsync("CreateHome", new object[] {
        //                LocationReference,
        //                HomeDocument}, this.CreateHomeOperationCompleted, userState);
        //}
        
        //private void OnCreateHomeOperationCompleted(object arg) {
        //    if ((this.CreateHomeCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.CreateHomeCompleted(this, new CreateHomeCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateHome", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
            public System.Xml.XmlNode UpdateHome(System.Xml.XmlNode LocationReference, System.Xml.XmlNode HomeDocument)
            {
            object[] results = this.Invoke("UpdateHome", new object[] {
                        LocationReference,
                        HomeDocument});
                return (System.Xml.XmlNode)results[0];
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginUpdateHome(System.Xml.XmlNode LocationReference, System.Xml.XmlNode HomeDocument, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("UpdateHome", new object[] {
        //                LocationReference,
        //                HomeDocument}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndUpdateHome(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void UpdateHomeAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode HomeDocument) {
        //    this.UpdateHomeAsync(LocationReference, HomeDocument, null);
        //}
        
        ///// <remarks/>
        //public void UpdateHomeAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode HomeDocument, object userState) {
        //    if ((this.UpdateHomeOperationCompleted == null)) {
        //        this.UpdateHomeOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUpdateHomeOperationCompleted);
        //    }
        //    this.InvokeAsync("UpdateHome", new object[] {
        //                LocationReference,
        //                HomeDocument}, this.UpdateHomeOperationCompleted, userState);
        //}
        
        //private void OnUpdateHomeOperationCompleted(object arg) {
        //    if ((this.UpdateHomeCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.UpdateHomeCompleted(this, new UpdateHomeCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateHomeStatus", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
            public System.Xml.XmlNode UpdateHomeStatus(System.Xml.XmlNode LocationReference, string HomeNumber, string HomeStatus, bool DeleteEnvisionSelectedOptions, bool DeleteExternalSelectedOptions)
            {
            object[] results = this.Invoke("UpdateHomeStatus", new object[] {
                        LocationReference,
                        HomeNumber,
                        HomeStatus,
                        DeleteEnvisionSelectedOptions,
                        DeleteExternalSelectedOptions});
                return (System.Xml.XmlNode)results[0];
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginUpdateHomeStatus(System.Xml.XmlNode LocationReference, string HomeNumber, string HomeStatus, bool DeleteEnvisionSelectedOptions, bool DeleteExternalSelectedOptions, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("UpdateHomeStatus", new object[] {
        //                LocationReference,
        //                HomeNumber,
        //                HomeStatus,
        //                DeleteEnvisionSelectedOptions,
        //                DeleteExternalSelectedOptions}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndUpdateHomeStatus(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void UpdateHomeStatusAsync(System.Xml.XmlNode LocationReference, string HomeNumber, string HomeStatus, bool DeleteEnvisionSelectedOptions, bool DeleteExternalSelectedOptions) {
        //    this.UpdateHomeStatusAsync(LocationReference, HomeNumber, HomeStatus, DeleteEnvisionSelectedOptions, DeleteExternalSelectedOptions, null);
        //}
        
        ///// <remarks/>
        //public void UpdateHomeStatusAsync(System.Xml.XmlNode LocationReference, string HomeNumber, string HomeStatus, bool DeleteEnvisionSelectedOptions, bool DeleteExternalSelectedOptions, object userState) {
        //    if ((this.UpdateHomeStatusOperationCompleted == null)) {
        //        this.UpdateHomeStatusOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUpdateHomeStatusOperationCompleted);
        //    }
        //    this.InvokeAsync("UpdateHomeStatus", new object[] {
        //                LocationReference,
        //                HomeNumber,
        //                HomeStatus,
        //                DeleteEnvisionSelectedOptions,
        //                DeleteExternalSelectedOptions}, this.UpdateHomeStatusOperationCompleted, userState);
        //}
        
        //private void OnUpdateHomeStatusOperationCompleted(object arg) {
        //    if ((this.UpdateHomeStatusCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.UpdateHomeStatusCompleted(this, new UpdateHomeStatusCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
            [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateConstructionStage", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
            public System.Xml.XmlNode UpdateConstructionStage(System.Xml.XmlNode LocationReference, string HomeNumber, string ConstructionStage)
            {
            object[] results = this.Invoke("UpdateConstructionStage", new object[] {
                        LocationReference,
                        HomeNumber,
                        ConstructionStage});
                return (System.Xml.XmlNode)results[0];
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginUpdateConstructionStage(System.Xml.XmlNode LocationReference, string HomeNumber, string ConstructionStage, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("UpdateConstructionStage", new object[] {
        //                LocationReference,
        //                HomeNumber,
        //                ConstructionStage}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndUpdateConstructionStage(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void UpdateConstructionStageAsync(System.Xml.XmlNode LocationReference, string HomeNumber, string ConstructionStage) {
        //    this.UpdateConstructionStageAsync(LocationReference, HomeNumber, ConstructionStage, null);
        //}
        
        ///// <remarks/>
        //public void UpdateConstructionStageAsync(System.Xml.XmlNode LocationReference, string HomeNumber, string ConstructionStage, object userState) {
        //    if ((this.UpdateConstructionStageOperationCompleted == null)) {
        //        this.UpdateConstructionStageOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUpdateConstructionStageOperationCompleted);
        //    }
        //    this.InvokeAsync("UpdateConstructionStage", new object[] {
        //                LocationReference,
        //                HomeNumber,
        //                ConstructionStage}, this.UpdateConstructionStageOperationCompleted, userState);
        //}
        
        //private void OnUpdateConstructionStageOperationCompleted(object arg) {
        //    if ((this.UpdateConstructionStageCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.UpdateConstructionStageCompleted(this, new UpdateConstructionStageCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/RefreshPrices", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
            public System.Xml.XmlNode RefreshPrices(System.Xml.XmlNode LocationReference, string HomeNumber, bool EnvisionSelectedOptions, bool ExternalSelectedOptions, bool PreSelectedOptions)
            {
            object[] results = this.Invoke("RefreshPrices", new object[] {
                        LocationReference,
                        HomeNumber,
                        EnvisionSelectedOptions,
                        ExternalSelectedOptions,
                        PreSelectedOptions});
                return (System.Xml.XmlNode)results[0];
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginRefreshPrices(System.Xml.XmlNode LocationReference, string HomeNumber, bool EnvisionSelectedOptions, bool ExternalSelectedOptions, bool PreSelectedOptions, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("RefreshPrices", new object[] {
        //                LocationReference,
        //                HomeNumber,
        //                EnvisionSelectedOptions,
        //                ExternalSelectedOptions,
        //                PreSelectedOptions}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndRefreshPrices(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void RefreshPricesAsync(System.Xml.XmlNode LocationReference, string HomeNumber, bool EnvisionSelectedOptions, bool ExternalSelectedOptions, bool PreSelectedOptions) {
        //    this.RefreshPricesAsync(LocationReference, HomeNumber, EnvisionSelectedOptions, ExternalSelectedOptions, PreSelectedOptions, null);
        //}
        
        ///// <remarks/>
        //public void RefreshPricesAsync(System.Xml.XmlNode LocationReference, string HomeNumber, bool EnvisionSelectedOptions, bool ExternalSelectedOptions, bool PreSelectedOptions, object userState) {
        //    if ((this.RefreshPricesOperationCompleted == null)) {
        //        this.RefreshPricesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnRefreshPricesOperationCompleted);
        //    }
        //    this.InvokeAsync("RefreshPrices", new object[] {
        //                LocationReference,
        //                HomeNumber,
        //                EnvisionSelectedOptions,
        //                ExternalSelectedOptions,
        //                PreSelectedOptions}, this.RefreshPricesOperationCompleted, userState);
        //}
        
        //private void OnRefreshPricesOperationCompleted(object arg) {
        //    if ((this.RefreshPricesCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.RefreshPricesCompleted(this, new RefreshPricesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "0#"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateSelectionStatus", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
            public System.Xml.XmlNode UpdateSelectionStatus(int TransactionID, string Status)
            {
            object[] results = this.Invoke("UpdateSelectionStatus", new object[] {
                        TransactionID,
                        Status});
                return (System.Xml.XmlNode)results[0];
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginUpdateSelectionStatus(int TransactionID, string Status, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("UpdateSelectionStatus", new object[] {
        //                TransactionID,
        //                Status}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndUpdateSelectionStatus(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void UpdateSelectionStatusAsync(int TransactionID, string Status) {
        //    this.UpdateSelectionStatusAsync(TransactionID, Status, null);
        //}
        
        ///// <remarks/>
        //public void UpdateSelectionStatusAsync(int TransactionID, string Status, object userState) {
        //    if ((this.UpdateSelectionStatusOperationCompleted == null)) {
        //        this.UpdateSelectionStatusOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUpdateSelectionStatusOperationCompleted);
        //    }
        //    this.InvokeAsync("UpdateSelectionStatus", new object[] {
        //                TransactionID,
        //                Status}, this.UpdateSelectionStatusOperationCompleted, userState);
        //}
        
        //private void OnUpdateSelectionStatusOperationCompleted(object arg) {
        //    if ((this.UpdateSelectionStatusCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.UpdateSelectionStatusCompleted(this, new UpdateSelectionStatusCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/ChangeHomePlan", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
            public System.Xml.XmlNode ChangeHomePlan(System.Xml.XmlNode LocationReference, string homeNumber, System.Xml.XmlNode newLocationReference)
            {
            object[] results = this.Invoke("ChangeHomePlan", new object[] {
                        LocationReference,
                        homeNumber,
                        newLocationReference});
                return (System.Xml.XmlNode)results[0];
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginChangeHomePlan(System.Xml.XmlNode LocationReference, string homeNumber, System.Xml.XmlNode newLocationReference, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("ChangeHomePlan", new object[] {
        //                LocationReference,
        //                homeNumber,
        //                newLocationReference}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndChangeHomePlan(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void ChangeHomePlanAsync(System.Xml.XmlNode LocationReference, string homeNumber, System.Xml.XmlNode newLocationReference) {
        //    this.ChangeHomePlanAsync(LocationReference, homeNumber, newLocationReference, null);
        //}
        
        ///// <remarks/>
        //public void ChangeHomePlanAsync(System.Xml.XmlNode LocationReference, string homeNumber, System.Xml.XmlNode newLocationReference, object userState) {
        //    if ((this.ChangeHomePlanOperationCompleted == null)) {
        //        this.ChangeHomePlanOperationCompleted = new System.Threading.SendOrPostCallback(this.OnChangeHomePlanOperationCompleted);
        //    }
        //    this.InvokeAsync("ChangeHomePlan", new object[] {
        //                LocationReference,
        //                homeNumber,
        //                newLocationReference}, this.ChangeHomePlanOperationCompleted, userState);
        //}
        
        //private void OnChangeHomePlanOperationCompleted(object arg) {
        //    if ((this.ChangeHomePlanCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.ChangeHomePlanCompleted(this, new ChangeHomePlanCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        ///// <remarks/>
        //public new void CancelAsync(object userState) {
        //    base.CancelAsync(userState);
        //}
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
        public string UserName {
            get {
                return this.userNameField;
            }
            set {
                this.userNameField = value;
            }
        }
        
        /// <remarks/>
        public string Password {
            get {
                return this.passwordField;
            }
            set {
                this.passwordField = value;
            }
        }
        
        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased", MessageId = "Member")]
        public string NHTBillingNumber {
            get {
                return this.m_NHTBillingNumberField;
            }
            set {
                this.m_NHTBillingNumberField = value;
            }
        }
    }

    ///// <remarks/>
    //public delegate void CreateHomeCompletedEventHandler(object sender, CreateHomeCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class CreateHomeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal CreateHomeCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
    //            base(exception, cancelled, userState) {
    //        this.results = results;
    //    }
        
    //    /// <remarks/>
    //    public System.Xml.XmlNode Result {
    //        get {
    //            this.RaiseExceptionIfNecessary();
    //            return ((System.Xml.XmlNode)(this.results[0]));
    //        }
    //    }
    //}

    ///// <remarks/>
    //public delegate void UpdateHomeCompletedEventHandler(object sender, UpdateHomeCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class UpdateHomeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal UpdateHomeCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
    //            base(exception, cancelled, userState) {
    //        this.results = results;
    //    }
        
    //    /// <remarks/>
    //    public System.Xml.XmlNode Result {
    //        get {
    //            this.RaiseExceptionIfNecessary();
    //            return ((System.Xml.XmlNode)(this.results[0]));
    //        }
    //    }
    //}

    ///// <remarks/>
    //public delegate void UpdateHomeStatusCompletedEventHandler(object sender, UpdateHomeStatusCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class UpdateHomeStatusCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal UpdateHomeStatusCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
    //            base(exception, cancelled, userState) {
    //        this.results = results;
    //    }
        
    //    /// <remarks/>
    //    public System.Xml.XmlNode Result {
    //        get {
    //            this.RaiseExceptionIfNecessary();
    //            return ((System.Xml.XmlNode)(this.results[0]));
    //        }
    //    }
    //}

    ///// <remarks/>
    //public delegate void UpdateConstructionStageCompletedEventHandler(object sender, UpdateConstructionStageCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class UpdateConstructionStageCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal UpdateConstructionStageCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
    //            base(exception, cancelled, userState) {
    //        this.results = results;
    //    }
        
    //    /// <remarks/>
    //    public System.Xml.XmlNode Result {
    //        get {
    //            this.RaiseExceptionIfNecessary();
    //            return ((System.Xml.XmlNode)(this.results[0]));
    //        }
    //    }
    //}

    ///// <remarks/>
    //public delegate void RefreshPricesCompletedEventHandler(object sender, RefreshPricesCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class RefreshPricesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal RefreshPricesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
    //            base(exception, cancelled, userState) {
    //        this.results = results;
    //    }
        
    //    /// <remarks/>
    //    public System.Xml.XmlNode Result {
    //        get {
    //            this.RaiseExceptionIfNecessary();
    //            return ((System.Xml.XmlNode)(this.results[0]));
    //        }
    //    }
    //}

    ///// <remarks/>
    //public delegate void UpdateSelectionStatusCompletedEventHandler(object sender, UpdateSelectionStatusCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class UpdateSelectionStatusCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal UpdateSelectionStatusCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
    //            base(exception, cancelled, userState) {
    //        this.results = results;
    //    }
        
    //    /// <remarks/>
    //    public System.Xml.XmlNode Result {
    //        get {
    //            this.RaiseExceptionIfNecessary();
    //            return ((System.Xml.XmlNode)(this.results[0]));
    //        }
    //    }
    //}

    ///// <remarks/>
    //public delegate void ChangeHomePlanCompletedEventHandler(object sender, ChangeHomePlanCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class ChangeHomePlanCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal ChangeHomePlanCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
    //            base(exception, cancelled, userState) {
    //        this.results = results;
    //    }
        
    //    /// <remarks/>
    //    public System.Xml.XmlNode Result {
    //        get {
    //            this.RaiseExceptionIfNecessary();
    //            return ((System.Xml.XmlNode)(this.results[0]));
    //        }
    //    }
    //}
}

#pragma warning restore 1591
