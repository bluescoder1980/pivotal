//
// $Workfile: BuyerWebService.cs$
// $Revision: 9$
// $Author: tlyne$
// $Date: Thursday, April 05, 2007 12:58:13 PM$
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
        
        //private System.Threading.SendOrPostCallback CreateBuyerOperationCompleted;
        
        //private System.Threading.SendOrPostCallback CreateBuyer1OperationCompleted;
        
        //private System.Threading.SendOrPostCallback UpdateBuyerOperationCompleted;
        
        //private System.Threading.SendOrPostCallback UpdateBuyerStatusOperationCompleted;
        
        //private System.Threading.SendOrPostCallback AssignNewHometoBuyerOperationCompleted;
        
            /// <remarks/>
            internal BuyerWebService(CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionIntegration envisionIntegration)
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
        //public event CreateBuyerCompletedEventHandler CreateBuyerCompleted;
        
        ///// <remarks/>
        //public event CreateBuyer1CompletedEventHandler CreateBuyer1Completed;
        
        ///// <remarks/>
        //public event UpdateBuyerCompletedEventHandler UpdateBuyerCompleted;
        
        ///// <remarks/>
        //public event UpdateBuyerStatusCompletedEventHandler UpdateBuyerStatusCompleted;
        
        ///// <remarks/>
        //public event AssignNewHometoBuyerCompletedEventHandler AssignNewHometoBuyerCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateBuyer", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode CreateBuyer(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument, string HomeNumber, bool AutoActivateBuyer) {
            object[] results = this.Invoke("CreateBuyer", new object[] {
                        LocationReference,
                        BuyerDocument,
                        HomeNumber,
                        AutoActivateBuyer});
                return (System.Xml.XmlNode)results[0];
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginCreateBuyer(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument, string HomeNumber, bool AutoActivateBuyer, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("CreateBuyer", new object[] {
        //                LocationReference,
        //                BuyerDocument,
        //                HomeNumber,
        //                AutoActivateBuyer}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndCreateBuyer(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void CreateBuyerAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument, string HomeNumber, bool AutoActivateBuyer) {
        //    this.CreateBuyerAsync(LocationReference, BuyerDocument, HomeNumber, AutoActivateBuyer, null);
        //}
        
        ///// <remarks/>
        //public void CreateBuyerAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument, string HomeNumber, bool AutoActivateBuyer, object userState) {
        //    if ((this.CreateBuyerOperationCompleted == null)) {
        //        this.CreateBuyerOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateBuyerOperationCompleted);
        //    }
        //    this.InvokeAsync("CreateBuyer", new object[] {
        //                LocationReference,
        //                BuyerDocument,
        //                HomeNumber,
        //                AutoActivateBuyer}, this.CreateBuyerOperationCompleted, userState);
        //}
        
        //private void OnCreateBuyerOperationCompleted(object arg) {
        //    if ((this.CreateBuyerCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.CreateBuyerCompleted(this, new CreateBuyerCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="CreateBuyer1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateBuyer2", RequestElementName="CreateBuyer2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateBuyer2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateBuyer2Result")]
        public System.Xml.XmlNode CreateBuyer(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument, string HomeNumber, bool AutoActivateBuyer, string WelcomeEmailCopyAddress) {
            object[] results = this.Invoke("CreateBuyer1", new object[] {
                        LocationReference,
                        BuyerDocument,
                        HomeNumber,
                        AutoActivateBuyer,
                        WelcomeEmailCopyAddress});
                return (System.Xml.XmlNode)results[0];
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginCreateBuyer1(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument, string HomeNumber, bool AutoActivateBuyer, string WelcomeEmailCopyAddress, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("CreateBuyer1", new object[] {
        //                LocationReference,
        //                BuyerDocument,
        //                HomeNumber,
        //                AutoActivateBuyer,
        //                WelcomeEmailCopyAddress}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndCreateBuyer1(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void CreateBuyer1Async(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument, string HomeNumber, bool AutoActivateBuyer, string WelcomeEmailCopyAddress) {
        //    this.CreateBuyer1Async(LocationReference, BuyerDocument, HomeNumber, AutoActivateBuyer, WelcomeEmailCopyAddress, null);
        //}
        
        ///// <remarks/>
        //public void CreateBuyer1Async(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument, string HomeNumber, bool AutoActivateBuyer, string WelcomeEmailCopyAddress, object userState) {
        //    if ((this.CreateBuyer1OperationCompleted == null)) {
        //        this.CreateBuyer1OperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateBuyer1OperationCompleted);
        //    }
        //    this.InvokeAsync("CreateBuyer1", new object[] {
        //                LocationReference,
        //                BuyerDocument,
        //                HomeNumber,
        //                AutoActivateBuyer,
        //                WelcomeEmailCopyAddress}, this.CreateBuyer1OperationCompleted, userState);
        //}
        
        //private void OnCreateBuyer1OperationCompleted(object arg) {
        //    if ((this.CreateBuyer1Completed != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.CreateBuyer1Completed(this, new CreateBuyer1CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateBuyer", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode UpdateBuyer(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument) {
            object[] results = this.Invoke("UpdateBuyer", new object[] {
                        LocationReference,
                        BuyerDocument});
                return (System.Xml.XmlNode)results[0];
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginUpdateBuyer(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("UpdateBuyer", new object[] {
        //                LocationReference,
        //                BuyerDocument}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndUpdateBuyer(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void UpdateBuyerAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument) {
        //    this.UpdateBuyerAsync(LocationReference, BuyerDocument, null);
        //}
        
        ///// <remarks/>
        //public void UpdateBuyerAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode BuyerDocument, object userState) {
        //    if ((this.UpdateBuyerOperationCompleted == null)) {
        //        this.UpdateBuyerOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUpdateBuyerOperationCompleted);
        //    }
        //    this.InvokeAsync("UpdateBuyer", new object[] {
        //                LocationReference,
        //                BuyerDocument}, this.UpdateBuyerOperationCompleted, userState);
        //}
        
        //private void OnUpdateBuyerOperationCompleted(object arg) {
        //    if ((this.UpdateBuyerCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.UpdateBuyerCompleted(this, new UpdateBuyerCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateBuyerStatus", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode UpdateBuyerStatus(System.Xml.XmlNode LocationReference, string BuyerNumber, string BuyerStatus) {
            object[] results = this.Invoke("UpdateBuyerStatus", new object[] {
                        LocationReference,
                        BuyerNumber,
                        BuyerStatus});
                return (System.Xml.XmlNode)results[0];
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginUpdateBuyerStatus(System.Xml.XmlNode LocationReference, string BuyerNumber, string BuyerStatus, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("UpdateBuyerStatus", new object[] {
        //                LocationReference,
        //                BuyerNumber,
        //                BuyerStatus}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndUpdateBuyerStatus(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void UpdateBuyerStatusAsync(System.Xml.XmlNode LocationReference, string BuyerNumber, string BuyerStatus) {
        //    this.UpdateBuyerStatusAsync(LocationReference, BuyerNumber, BuyerStatus, null);
        //}
        
        ///// <remarks/>
        //public void UpdateBuyerStatusAsync(System.Xml.XmlNode LocationReference, string BuyerNumber, string BuyerStatus, object userState) {
        //    if ((this.UpdateBuyerStatusOperationCompleted == null)) {
        //        this.UpdateBuyerStatusOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUpdateBuyerStatusOperationCompleted);
        //    }
        //    this.InvokeAsync("UpdateBuyerStatus", new object[] {
        //                LocationReference,
        //                BuyerNumber,
        //                BuyerStatus}, this.UpdateBuyerStatusOperationCompleted, userState);
        //}
        
        //private void OnUpdateBuyerStatusOperationCompleted(object arg) {
        //    if ((this.UpdateBuyerStatusCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.UpdateBuyerStatusCompleted(this, new UpdateBuyerStatusCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/AssignNewHometoBuyer", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode AssignNewHometoBuyer(System.Xml.XmlNode LocationReference, string BuyerNumber, string HomeNumber, bool TransferoldSelectionsToWishlist) {
            object[] results = this.Invoke("AssignNewHometoBuyer", new object[] {
                        LocationReference,
                        BuyerNumber,
                        HomeNumber,
                        TransferoldSelectionsToWishlist});
                return (System.Xml.XmlNode)results[0];
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginAssignNewHometoBuyer(System.Xml.XmlNode LocationReference, string BuyerNumber, string HomeNumber, bool TransferoldSelectionsToWishlist, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("AssignNewHometoBuyer", new object[] {
        //                LocationReference,
        //                BuyerNumber,
        //                HomeNumber,
        //                TransferoldSelectionsToWishlist}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndAssignNewHometoBuyer(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void AssignNewHometoBuyerAsync(System.Xml.XmlNode LocationReference, string BuyerNumber, string HomeNumber, bool TransferoldSelectionsToWishlist) {
        //    this.AssignNewHometoBuyerAsync(LocationReference, BuyerNumber, HomeNumber, TransferoldSelectionsToWishlist, null);
        //}
        
        ///// <remarks/>
        //public void AssignNewHometoBuyerAsync(System.Xml.XmlNode LocationReference, string BuyerNumber, string HomeNumber, bool TransferoldSelectionsToWishlist, object userState) {
        //    if ((this.AssignNewHometoBuyerOperationCompleted == null)) {
        //        this.AssignNewHometoBuyerOperationCompleted = new System.Threading.SendOrPostCallback(this.OnAssignNewHometoBuyerOperationCompleted);
        //    }
        //    this.InvokeAsync("AssignNewHometoBuyer", new object[] {
        //                LocationReference,
        //                BuyerNumber,
        //                HomeNumber,
        //                TransferoldSelectionsToWishlist}, this.AssignNewHometoBuyerOperationCompleted, userState);
        //}
        
            //private void OnAssignNewHometoBuyerOperationCompleted(object arg)
            //{
            //    if ((this.AssignNewHometoBuyerCompleted != null))
            //    {
            //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            //        this.AssignNewHometoBuyerCompleted(this, new AssignNewHometoBuyerCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
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
    //public delegate void CreateBuyerCompletedEventHandler(object sender, CreateBuyerCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class CreateBuyerCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal CreateBuyerCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void CreateBuyer1CompletedEventHandler(object sender, CreateBuyer1CompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class CreateBuyer1CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal CreateBuyer1CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void UpdateBuyerCompletedEventHandler(object sender, UpdateBuyerCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class UpdateBuyerCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal UpdateBuyerCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void UpdateBuyerStatusCompletedEventHandler(object sender, UpdateBuyerStatusCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class UpdateBuyerStatusCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal UpdateBuyerStatusCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void AssignNewHometoBuyerCompletedEventHandler(object sender, AssignNewHometoBuyerCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class AssignNewHometoBuyerCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal AssignNewHometoBuyerCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
