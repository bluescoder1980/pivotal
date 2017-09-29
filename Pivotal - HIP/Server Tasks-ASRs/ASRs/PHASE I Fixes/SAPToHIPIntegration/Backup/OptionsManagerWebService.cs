//
// $Workfile: OptionsManagerWebService.cs$
// $Revision: 9$
// $Author: tlyne$
// $Date: Thursday, April 05, 2007 12:58:18 PM$
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
        
        //private System.Threading.SendOrPostCallback CreateInventoryOperationCompleted;
        
        //private System.Threading.SendOrPostCallback CreateInventory1OperationCompleted;
        
        //private System.Threading.SendOrPostCallback EditInventoryOperationCompleted;
        
        //private System.Threading.SendOrPostCallback EditInventory1OperationCompleted;
        
        //private System.Threading.SendOrPostCallback DeactivateInventoryOperationCompleted;
        
        //private System.Threading.SendOrPostCallback DeactivateInventory1OperationCompleted;
        
        //private System.Threading.SendOrPostCallback CreateEditRoomsOperationCompleted;
        
        //private System.Threading.SendOrPostCallback CreateEditRooms1OperationCompleted;
        
        //private System.Threading.SendOrPostCallback UpdateOptionCategoriesOperationCompleted;
        
        //private System.Threading.SendOrPostCallback CreateOptionOperationCompleted;
        
        //private System.Threading.SendOrPostCallback CreateOption1OperationCompleted;
        
        //private System.Threading.SendOrPostCallback EditOptionOperationCompleted;
        
        //private System.Threading.SendOrPostCallback EditOption1OperationCompleted;
        
        //private System.Threading.SendOrPostCallback DeactivateOptionOperationCompleted;
        
        //private System.Threading.SendOrPostCallback DeactivateOption1OperationCompleted;
        
        //private System.Threading.SendOrPostCallback CreateEditOptionAssignmentsOperationCompleted;
        
        //private System.Threading.SendOrPostCallback CreateEditOptionAssignments1OperationCompleted;
        
        //private System.Threading.SendOrPostCallback CreateIntersectionRuleOperationCompleted;
        
        //private System.Threading.SendOrPostCallback EditIntersectionRuleOperationCompleted;
        
            /// <remarks/>
            internal OptionsManagerService(CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionIntegration envisionIntegration)
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
        //public event CreateInventoryCompletedEventHandler CreateInventoryCompleted;
        
        ///// <remarks/>
        //public event CreateInventory1CompletedEventHandler CreateInventory1Completed;
        
        ///// <remarks/>
        //public event EditInventoryCompletedEventHandler EditInventoryCompleted;
        
        ///// <remarks/>
        //public event EditInventory1CompletedEventHandler EditInventory1Completed;
        
        ///// <remarks/>
        //public event DeactivateInventoryCompletedEventHandler DeactivateInventoryCompleted;
        
        ///// <remarks/>
        //public event DeactivateInventory1CompletedEventHandler DeactivateInventory1Completed;
        
        ///// <remarks/>
        //public event CreateEditRoomsCompletedEventHandler CreateEditRoomsCompleted;
        
        ///// <remarks/>
        //public event CreateEditRooms1CompletedEventHandler CreateEditRooms1Completed;
        
        ///// <remarks/>
        //public event UpdateOptionCategoriesCompletedEventHandler UpdateOptionCategoriesCompleted;
        
        ///// <remarks/>
        //public event CreateOptionCompletedEventHandler CreateOptionCompleted;
        
        ///// <remarks/>
        //public event CreateOption1CompletedEventHandler CreateOption1Completed;
        
        ///// <remarks/>
        //public event EditOptionCompletedEventHandler EditOptionCompleted;
        
        ///// <remarks/>
        //public event EditOption1CompletedEventHandler EditOption1Completed;
        
        ///// <remarks/>
        //public event DeactivateOptionCompletedEventHandler DeactivateOptionCompleted;
        
        ///// <remarks/>
        //public event DeactivateOption1CompletedEventHandler DeactivateOption1Completed;
        
        ///// <remarks/>
        //public event CreateEditOptionAssignmentsCompletedEventHandler CreateEditOptionAssignmentsCompleted;
        
        ///// <remarks/>
        //public event CreateEditOptionAssignments1CompletedEventHandler CreateEditOptionAssignments1Completed;
        
        ///// <remarks/>
        //public event CreateIntersectionRuleCompletedEventHandler CreateIntersectionRuleCompleted;
        
        ///// <remarks/>
        //public event EditIntersectionRuleCompletedEventHandler EditIntersectionRuleCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateInventory2", RequestElementName="CreateInventory2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateInventory2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateInventory2Result")]
        public System.Xml.XmlNode CreateInventory(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Inventory) {
            object[] results = this.Invoke("CreateInventory", new object[] {
                        LocationReference,
                        Inventory});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginCreateInventory(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Inventory, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("CreateInventory", new object[] {
        //                LocationReference,
        //                Inventory}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndCreateInventory(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void CreateInventoryAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Inventory) {
        //    this.CreateInventoryAsync(LocationReference, Inventory, null);
        //}
        
        ///// <remarks/>
        //public void CreateInventoryAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Inventory, object userState) {
        //    if ((this.CreateInventoryOperationCompleted == null)) {
        //        this.CreateInventoryOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateInventoryOperationCompleted);
        //    }
        //    this.InvokeAsync("CreateInventory", new object[] {
        //                LocationReference,
        //                Inventory}, this.CreateInventoryOperationCompleted, userState);
        //}
        
        //private void OnCreateInventoryOperationCompleted(object arg) {
        //    if ((this.CreateInventoryCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.CreateInventoryCompleted(this, new CreateInventoryCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="CreateInventory1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateInventory", RequestElementName="CreateInventory", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateInventoryResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateInventoryResult")]
        public System.Xml.XmlNode CreateInventory(string LocationReference, string Inventory) {
            object[] results = this.Invoke("CreateInventory1", new object[] {
                        LocationReference,
                        Inventory});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginCreateInventory1(string LocationReference, string Inventory, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("CreateInventory1", new object[] {
        //                LocationReference,
        //                Inventory}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndCreateInventory1(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void CreateInventory1Async(string LocationReference, string Inventory) {
        //    this.CreateInventory1Async(LocationReference, Inventory, null);
        //}
        
        ///// <remarks/>
        //public void CreateInventory1Async(string LocationReference, string Inventory, object userState) {
        //    if ((this.CreateInventory1OperationCompleted == null)) {
        //        this.CreateInventory1OperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateInventory1OperationCompleted);
        //    }
        //    this.InvokeAsync("CreateInventory1", new object[] {
        //                LocationReference,
        //                Inventory}, this.CreateInventory1OperationCompleted, userState);
        //}
        
        //private void OnCreateInventory1OperationCompleted(object arg) {
        //    if ((this.CreateInventory1Completed != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.CreateInventory1Completed(this, new CreateInventory1CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/EditInventory2", RequestElementName="EditInventory2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="EditInventory2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("EditInventory2Result")]
        public System.Xml.XmlNode EditInventory(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Inventory) {
            object[] results = this.Invoke("EditInventory", new object[] {
                        LocationReference,
                        Inventory});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginEditInventory(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Inventory, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("EditInventory", new object[] {
        //                LocationReference,
        //                Inventory}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndEditInventory(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void EditInventoryAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Inventory) {
        //    this.EditInventoryAsync(LocationReference, Inventory, null);
        //}
        
        ///// <remarks/>
        //public void EditInventoryAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Inventory, object userState) {
        //    if ((this.EditInventoryOperationCompleted == null)) {
        //        this.EditInventoryOperationCompleted = new System.Threading.SendOrPostCallback(this.OnEditInventoryOperationCompleted);
        //    }
        //    this.InvokeAsync("EditInventory", new object[] {
        //                LocationReference,
        //                Inventory}, this.EditInventoryOperationCompleted, userState);
        //}
        
        //private void OnEditInventoryOperationCompleted(object arg) {
        //    if ((this.EditInventoryCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.EditInventoryCompleted(this, new EditInventoryCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="EditInventory1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/EditInventory", RequestElementName="EditInventory", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="EditInventoryResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("EditInventoryResult")]
        public System.Xml.XmlNode EditInventory(string LocationReference, string Inventory) {
            object[] results = this.Invoke("EditInventory1", new object[] {
                        LocationReference,
                        Inventory});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginEditInventory1(string LocationReference, string Inventory, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("EditInventory1", new object[] {
        //                LocationReference,
        //                Inventory}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndEditInventory1(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void EditInventory1Async(string LocationReference, string Inventory) {
        //    this.EditInventory1Async(LocationReference, Inventory, null);
        //}
        
        ///// <remarks/>
        //public void EditInventory1Async(string LocationReference, string Inventory, object userState) {
        //    if ((this.EditInventory1OperationCompleted == null)) {
        //        this.EditInventory1OperationCompleted = new System.Threading.SendOrPostCallback(this.OnEditInventory1OperationCompleted);
        //    }
        //    this.InvokeAsync("EditInventory1", new object[] {
        //                LocationReference,
        //                Inventory}, this.EditInventory1OperationCompleted, userState);
        //}
        
        //private void OnEditInventory1OperationCompleted(object arg) {
        //    if ((this.EditInventory1Completed != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.EditInventory1Completed(this, new EditInventory1CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/DeactivateInventory2", RequestElementName="DeactivateInventory2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="DeactivateInventory2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("DeactivateInventory2Result")]
        public System.Xml.XmlNode DeactivateInventory(System.Xml.XmlNode LocationReference, string LocationName, string LocationLevel) {
            object[] results = this.Invoke("DeactivateInventory", new object[] {
                        LocationReference,
                        LocationName,
                        LocationLevel});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginDeactivateInventory(System.Xml.XmlNode LocationReference, string LocationName, string LocationLevel, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("DeactivateInventory", new object[] {
        //                LocationReference,
        //                LocationName,
        //                LocationLevel}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndDeactivateInventory(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void DeactivateInventoryAsync(System.Xml.XmlNode LocationReference, string LocationName, string LocationLevel) {
        //    this.DeactivateInventoryAsync(LocationReference, LocationName, LocationLevel, null);
        //}
        
        ///// <remarks/>
        //public void DeactivateInventoryAsync(System.Xml.XmlNode LocationReference, string LocationName, string LocationLevel, object userState) {
        //    if ((this.DeactivateInventoryOperationCompleted == null)) {
        //        this.DeactivateInventoryOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDeactivateInventoryOperationCompleted);
        //    }
        //    this.InvokeAsync("DeactivateInventory", new object[] {
        //                LocationReference,
        //                LocationName,
        //                LocationLevel}, this.DeactivateInventoryOperationCompleted, userState);
        //}
        
        //private void OnDeactivateInventoryOperationCompleted(object arg) {
        //    if ((this.DeactivateInventoryCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.DeactivateInventoryCompleted(this, new DeactivateInventoryCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="DeactivateInventory1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/DeactivateInventory", RequestElementName="DeactivateInventory", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="DeactivateInventoryResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("DeactivateInventoryResult")]
        public System.Xml.XmlNode DeactivateInventory(string LocationReference, string LocationName, string LocationLevel) {
            object[] results = this.Invoke("DeactivateInventory1", new object[] {
                        LocationReference,
                        LocationName,
                        LocationLevel});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginDeactivateInventory1(string LocationReference, string LocationName, string LocationLevel, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("DeactivateInventory1", new object[] {
        //                LocationReference,
        //                LocationName,
        //                LocationLevel}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndDeactivateInventory1(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void DeactivateInventory1Async(string LocationReference, string LocationName, string LocationLevel) {
        //    this.DeactivateInventory1Async(LocationReference, LocationName, LocationLevel, null);
        //}
        
        ///// <remarks/>
        //public void DeactivateInventory1Async(string LocationReference, string LocationName, string LocationLevel, object userState) {
        //    if ((this.DeactivateInventory1OperationCompleted == null)) {
        //        this.DeactivateInventory1OperationCompleted = new System.Threading.SendOrPostCallback(this.OnDeactivateInventory1OperationCompleted);
        //    }
        //    this.InvokeAsync("DeactivateInventory1", new object[] {
        //                LocationReference,
        //                LocationName,
        //                LocationLevel}, this.DeactivateInventory1OperationCompleted, userState);
        //}
        
        //private void OnDeactivateInventory1OperationCompleted(object arg) {
        //    if ((this.DeactivateInventory1Completed != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.DeactivateInventory1Completed(this, new DeactivateInventory1CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateEditRooms2", RequestElementName="CreateEditRooms2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateEditRooms2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateEditRooms2Result")]
        public System.Xml.XmlNode CreateEditRooms(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Rooms) {
            object[] results = this.Invoke("CreateEditRooms", new object[] {
                        LocationReference,
                        Rooms});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginCreateEditRooms(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Rooms, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("CreateEditRooms", new object[] {
        //                LocationReference,
        //                Rooms}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndCreateEditRooms(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void CreateEditRoomsAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Rooms) {
        //    this.CreateEditRoomsAsync(LocationReference, Rooms, null);
        //}
        
        ///// <remarks/>
        //public void CreateEditRoomsAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Rooms, object userState) {
        //    if ((this.CreateEditRoomsOperationCompleted == null)) {
        //        this.CreateEditRoomsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateEditRoomsOperationCompleted);
        //    }
        //    this.InvokeAsync("CreateEditRooms", new object[] {
        //                LocationReference,
        //                Rooms}, this.CreateEditRoomsOperationCompleted, userState);
        //}
        
        //private void OnCreateEditRoomsOperationCompleted(object arg) {
        //    if ((this.CreateEditRoomsCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.CreateEditRoomsCompleted(this, new CreateEditRoomsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="CreateEditRooms1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateEditRooms", RequestElementName="CreateEditRooms", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateEditRoomsResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateEditRoomsResult")]
        public System.Xml.XmlNode CreateEditRooms(string LocationReference, string Rooms) {
            object[] results = this.Invoke("CreateEditRooms1", new object[] {
                        LocationReference,
                        Rooms});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginCreateEditRooms1(string LocationReference, string Rooms, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("CreateEditRooms1", new object[] {
        //                LocationReference,
        //                Rooms}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndCreateEditRooms1(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void CreateEditRooms1Async(string LocationReference, string Rooms) {
        //    this.CreateEditRooms1Async(LocationReference, Rooms, null);
        //}
        
        ///// <remarks/>
        //public void CreateEditRooms1Async(string LocationReference, string Rooms, object userState) {
        //    if ((this.CreateEditRooms1OperationCompleted == null)) {
        //        this.CreateEditRooms1OperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateEditRooms1OperationCompleted);
        //    }
        //    this.InvokeAsync("CreateEditRooms1", new object[] {
        //                LocationReference,
        //                Rooms}, this.CreateEditRooms1OperationCompleted, userState);
        //}
        
        //private void OnCreateEditRooms1OperationCompleted(object arg) {
        //    if ((this.CreateEditRooms1Completed != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.CreateEditRooms1Completed(this, new CreateEditRooms1CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/UpdateOptionCategories", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode UpdateOptionCategories(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Categories) {
            object[] results = this.Invoke("UpdateOptionCategories", new object[] {
                        LocationReference,
                        Categories});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginUpdateOptionCategories(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Categories, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("UpdateOptionCategories", new object[] {
        //                LocationReference,
        //                Categories}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndUpdateOptionCategories(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void UpdateOptionCategoriesAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Categories) {
        //    this.UpdateOptionCategoriesAsync(LocationReference, Categories, null);
        //}
        
        ///// <remarks/>
        //public void UpdateOptionCategoriesAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Categories, object userState) {
        //    if ((this.UpdateOptionCategoriesOperationCompleted == null)) {
        //        this.UpdateOptionCategoriesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnUpdateOptionCategoriesOperationCompleted);
        //    }
        //    this.InvokeAsync("UpdateOptionCategories", new object[] {
        //                LocationReference,
        //                Categories}, this.UpdateOptionCategoriesOperationCompleted, userState);
        //}
        
        //private void OnUpdateOptionCategoriesOperationCompleted(object arg) {
        //    if ((this.UpdateOptionCategoriesCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.UpdateOptionCategoriesCompleted(this, new UpdateOptionCategoriesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateOption2", RequestElementName="CreateOption2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateOption2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateOption2Result")]
        public System.Xml.XmlNode CreateOption(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Option) {
            object[] results = this.Invoke("CreateOption", new object[] {
                        LocationReference,
                        Option});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginCreateOption(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Option, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("CreateOption", new object[] {
        //                LocationReference,
        //                Option}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndCreateOption(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void CreateOptionAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Option) {
        //    this.CreateOptionAsync(LocationReference, Option, null);
        //}
        
        ///// <remarks/>
        //public void CreateOptionAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Option, object userState) {
        //    if ((this.CreateOptionOperationCompleted == null)) {
        //        this.CreateOptionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateOptionOperationCompleted);
        //    }
        //    this.InvokeAsync("CreateOption", new object[] {
        //                LocationReference,
        //                Option}, this.CreateOptionOperationCompleted, userState);
        //}
        
        //private void OnCreateOptionOperationCompleted(object arg) {
        //    if ((this.CreateOptionCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.CreateOptionCompleted(this, new CreateOptionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="CreateOption1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateOption", RequestElementName="CreateOption", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateOptionResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateOptionResult")]
        public System.Xml.XmlNode CreateOption(string LocationReference, string Option) {
            object[] results = this.Invoke("CreateOption1", new object[] {
                        LocationReference,
                        Option});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginCreateOption1(string LocationReference, string Option, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("CreateOption1", new object[] {
        //                LocationReference,
        //                Option}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndCreateOption1(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void CreateOption1Async(string LocationReference, string Option) {
        //    this.CreateOption1Async(LocationReference, Option, null);
        //}
        
        ///// <remarks/>
        //public void CreateOption1Async(string LocationReference, string Option, object userState) {
        //    if ((this.CreateOption1OperationCompleted == null)) {
        //        this.CreateOption1OperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateOption1OperationCompleted);
        //    }
        //    this.InvokeAsync("CreateOption1", new object[] {
        //                LocationReference,
        //                Option}, this.CreateOption1OperationCompleted, userState);
        //}
        
        //private void OnCreateOption1OperationCompleted(object arg) {
        //    if ((this.CreateOption1Completed != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.CreateOption1Completed(this, new CreateOption1CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/EditOption2", RequestElementName="EditOption2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="EditOption2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("EditOption2Result")]
        public System.Xml.XmlNode EditOption(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Option) {
            object[] results = this.Invoke("EditOption", new object[] {
                        LocationReference,
                        Option});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginEditOption(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Option, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("EditOption", new object[] {
        //                LocationReference,
        //                Option}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndEditOption(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void EditOptionAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Option) {
        //    this.EditOptionAsync(LocationReference, Option, null);
        //}
        
        ///// <remarks/>
        //public void EditOptionAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode Option, object userState) {
        //    if ((this.EditOptionOperationCompleted == null)) {
        //        this.EditOptionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnEditOptionOperationCompleted);
        //    }
        //    this.InvokeAsync("EditOption", new object[] {
        //                LocationReference,
        //                Option}, this.EditOptionOperationCompleted, userState);
        //}
        
        //private void OnEditOptionOperationCompleted(object arg) {
        //    if ((this.EditOptionCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.EditOptionCompleted(this, new EditOptionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="EditOption1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/EditOption", RequestElementName="EditOption", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="EditOptionResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("EditOptionResult")]
        public System.Xml.XmlNode EditOption(string LocationReference, string Option) {
            object[] results = this.Invoke("EditOption1", new object[] {
                        LocationReference,
                        Option});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginEditOption1(string LocationReference, string Option, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("EditOption1", new object[] {
        //                LocationReference,
        //                Option}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndEditOption1(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void EditOption1Async(string LocationReference, string Option) {
        //    this.EditOption1Async(LocationReference, Option, null);
        //}
        
        ///// <remarks/>
        //public void EditOption1Async(string LocationReference, string Option, object userState) {
        //    if ((this.EditOption1OperationCompleted == null)) {
        //        this.EditOption1OperationCompleted = new System.Threading.SendOrPostCallback(this.OnEditOption1OperationCompleted);
        //    }
        //    this.InvokeAsync("EditOption1", new object[] {
        //                LocationReference,
        //                Option}, this.EditOption1OperationCompleted, userState);
        //}
        
        //private void OnEditOption1OperationCompleted(object arg) {
        //    if ((this.EditOption1Completed != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.EditOption1Completed(this, new EditOption1CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/DeactivateOption2", RequestElementName="DeactivateOption2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="DeactivateOption2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("DeactivateOption2Result")]
        public System.Xml.XmlNode DeactivateOption(System.Xml.XmlNode LocationReference, string OptionNumber) {
            object[] results = this.Invoke("DeactivateOption", new object[] {
                        LocationReference,
                        OptionNumber});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginDeactivateOption(System.Xml.XmlNode LocationReference, string OptionNumber, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("DeactivateOption", new object[] {
        //                LocationReference,
        //                OptionNumber}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndDeactivateOption(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void DeactivateOptionAsync(System.Xml.XmlNode LocationReference, string OptionNumber) {
        //    this.DeactivateOptionAsync(LocationReference, OptionNumber, null);
        //}
        
        ///// <remarks/>
        //public void DeactivateOptionAsync(System.Xml.XmlNode LocationReference, string OptionNumber, object userState) {
        //    if ((this.DeactivateOptionOperationCompleted == null)) {
        //        this.DeactivateOptionOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDeactivateOptionOperationCompleted);
        //    }
        //    this.InvokeAsync("DeactivateOption", new object[] {
        //                LocationReference,
        //                OptionNumber}, this.DeactivateOptionOperationCompleted, userState);
        //}
        
        //private void OnDeactivateOptionOperationCompleted(object arg) {
        //    if ((this.DeactivateOptionCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.DeactivateOptionCompleted(this, new DeactivateOptionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="DeactivateOption1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/DeactivateOption", RequestElementName="DeactivateOption", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="DeactivateOptionResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("DeactivateOptionResult")]
        public System.Xml.XmlNode DeactivateOption(string LocationReference, string OptionNumber) {
            object[] results = this.Invoke("DeactivateOption1", new object[] {
                        LocationReference,
                        OptionNumber});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginDeactivateOption1(string LocationReference, string OptionNumber, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("DeactivateOption1", new object[] {
        //                LocationReference,
        //                OptionNumber}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndDeactivateOption1(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void DeactivateOption1Async(string LocationReference, string OptionNumber) {
        //    this.DeactivateOption1Async(LocationReference, OptionNumber, null);
        //}
        
        ///// <remarks/>
        //public void DeactivateOption1Async(string LocationReference, string OptionNumber, object userState) {
        //    if ((this.DeactivateOption1OperationCompleted == null)) {
        //        this.DeactivateOption1OperationCompleted = new System.Threading.SendOrPostCallback(this.OnDeactivateOption1OperationCompleted);
        //    }
        //    this.InvokeAsync("DeactivateOption1", new object[] {
        //                LocationReference,
        //                OptionNumber}, this.DeactivateOption1OperationCompleted, userState);
        //}
        
        //private void OnDeactivateOption1OperationCompleted(object arg) {
        //    if ((this.DeactivateOption1Completed != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.DeactivateOption1Completed(this, new DeactivateOption1CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateEditOptionAssignments2", RequestElementName="CreateEditOptionAssignments2", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateEditOptionAssignments2Response", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateEditOptionAssignments2Result")]
        public System.Xml.XmlNode CreateEditOptionAssignments(System.Xml.XmlNode LocationReference, System.Xml.XmlNode OptionAssignments) {
            object[] results = this.Invoke("CreateEditOptionAssignments", new object[] {
                        LocationReference,
                        OptionAssignments});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginCreateEditOptionAssignments(System.Xml.XmlNode LocationReference, System.Xml.XmlNode OptionAssignments, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("CreateEditOptionAssignments", new object[] {
        //                LocationReference,
        //                OptionAssignments}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndCreateEditOptionAssignments(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void CreateEditOptionAssignmentsAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode OptionAssignments) {
        //    this.CreateEditOptionAssignmentsAsync(LocationReference, OptionAssignments, null);
        //}
        
        ///// <remarks/>
        //public void CreateEditOptionAssignmentsAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode OptionAssignments, object userState) {
        //    if ((this.CreateEditOptionAssignmentsOperationCompleted == null)) {
        //        this.CreateEditOptionAssignmentsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateEditOptionAssignmentsOperationCompleted);
        //    }
        //    this.InvokeAsync("CreateEditOptionAssignments", new object[] {
        //                LocationReference,
        //                OptionAssignments}, this.CreateEditOptionAssignmentsOperationCompleted, userState);
        //}
        
        //private void OnCreateEditOptionAssignmentsOperationCompleted(object arg) {
        //    if ((this.CreateEditOptionAssignmentsCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.CreateEditOptionAssignmentsCompleted(this, new CreateEditOptionAssignmentsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.WebMethodAttribute(MessageName="CreateEditOptionAssignments1")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateEditOptionAssignments", RequestElementName="CreateEditOptionAssignments", RequestNamespace="http://newhometechnologies.com/envision/", ResponseElementName="CreateEditOptionAssignmentsResponse", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("CreateEditOptionAssignmentsResult")]
        public System.Xml.XmlNode CreateEditOptionAssignments(string LocationReference, string OptionAssignments) {
            object[] results = this.Invoke("CreateEditOptionAssignments1", new object[] {
                        LocationReference,
                        OptionAssignments});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginCreateEditOptionAssignments1(string LocationReference, string OptionAssignments, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("CreateEditOptionAssignments1", new object[] {
        //                LocationReference,
        //                OptionAssignments}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndCreateEditOptionAssignments1(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void CreateEditOptionAssignments1Async(string LocationReference, string OptionAssignments) {
        //    this.CreateEditOptionAssignments1Async(LocationReference, OptionAssignments, null);
        //}
        
        ///// <remarks/>
        //public void CreateEditOptionAssignments1Async(string LocationReference, string OptionAssignments, object userState) {
        //    if ((this.CreateEditOptionAssignments1OperationCompleted == null)) {
        //        this.CreateEditOptionAssignments1OperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateEditOptionAssignments1OperationCompleted);
        //    }
        //    this.InvokeAsync("CreateEditOptionAssignments1", new object[] {
        //                LocationReference,
        //                OptionAssignments}, this.CreateEditOptionAssignments1OperationCompleted, userState);
        //}
        
        //private void OnCreateEditOptionAssignments1OperationCompleted(object arg) {
        //    if ((this.CreateEditOptionAssignments1Completed != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.CreateEditOptionAssignments1Completed(this, new CreateEditOptionAssignments1CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/CreateIntersectionRule", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode CreateIntersectionRule(System.Xml.XmlNode LocationReference, System.Xml.XmlNode IntersectionRule) {
            object[] results = this.Invoke("CreateIntersectionRule", new object[] {
                        LocationReference,
                        IntersectionRule});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginCreateIntersectionRule(System.Xml.XmlNode LocationReference, System.Xml.XmlNode IntersectionRule, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("CreateIntersectionRule", new object[] {
        //                LocationReference,
        //                IntersectionRule}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndCreateIntersectionRule(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void CreateIntersectionRuleAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode IntersectionRule) {
        //    this.CreateIntersectionRuleAsync(LocationReference, IntersectionRule, null);
        //}
        
        ///// <remarks/>
        //public void CreateIntersectionRuleAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode IntersectionRule, object userState) {
        //    if ((this.CreateIntersectionRuleOperationCompleted == null)) {
        //        this.CreateIntersectionRuleOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCreateIntersectionRuleOperationCompleted);
        //    }
        //    this.InvokeAsync("CreateIntersectionRule", new object[] {
        //                LocationReference,
        //                IntersectionRule}, this.CreateIntersectionRuleOperationCompleted, userState);
        //}
        
        //private void OnCreateIntersectionRuleOperationCompleted(object arg) {
        //    if ((this.CreateIntersectionRuleCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.CreateIntersectionRuleCompleted(this, new CreateIntersectionRuleCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        //    }
        //}
        
        /// <remarks/>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode"), System.Web.Services.Protocols.SoapHeaderAttribute("AuthHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://newhometechnologies.com/envision/EditIntersectionRule", RequestNamespace="http://newhometechnologies.com/envision/", ResponseNamespace="http://newhometechnologies.com/envision/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Xml.XmlNode EditIntersectionRule(System.Xml.XmlNode LocationReference, System.Xml.XmlNode IntersectionRule) {
            object[] results = this.Invoke("EditIntersectionRule", new object[] {
                        LocationReference,
                        IntersectionRule});
            return ((System.Xml.XmlNode)(results[0]));
        }
        
        ///// <remarks/>
        //public System.IAsyncResult BeginEditIntersectionRule(System.Xml.XmlNode LocationReference, System.Xml.XmlNode IntersectionRule, System.AsyncCallback callback, object asyncState) {
        //    return this.BeginInvoke("EditIntersectionRule", new object[] {
        //                LocationReference,
        //                IntersectionRule}, callback, asyncState);
        //}
        
        ///// <remarks/>
        //public System.Xml.XmlNode EndEditIntersectionRule(System.IAsyncResult asyncResult) {
        //    object[] results = this.EndInvoke(asyncResult);
        //    return ((System.Xml.XmlNode)(results[0]));
        //}
        
        ///// <remarks/>
        //public void EditIntersectionRuleAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode IntersectionRule) {
        //    this.EditIntersectionRuleAsync(LocationReference, IntersectionRule, null);
        //}
        
        ///// <remarks/>
        //public void EditIntersectionRuleAsync(System.Xml.XmlNode LocationReference, System.Xml.XmlNode IntersectionRule, object userState) {
        //    if ((this.EditIntersectionRuleOperationCompleted == null)) {
        //        this.EditIntersectionRuleOperationCompleted = new System.Threading.SendOrPostCallback(this.OnEditIntersectionRuleOperationCompleted);
        //    }
        //    this.InvokeAsync("EditIntersectionRule", new object[] {
        //                LocationReference,
        //                IntersectionRule}, this.EditIntersectionRuleOperationCompleted, userState);
        //}
        
        //private void OnEditIntersectionRuleOperationCompleted(object arg) {
        //    if ((this.EditIntersectionRuleCompleted != null)) {
        //        System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
        //        this.EditIntersectionRuleCompleted(this, new EditIntersectionRuleCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
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
    //public delegate void CreateInventoryCompletedEventHandler(object sender, CreateInventoryCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class CreateInventoryCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal CreateInventoryCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void CreateInventory1CompletedEventHandler(object sender, CreateInventory1CompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class CreateInventory1CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal CreateInventory1CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void EditInventoryCompletedEventHandler(object sender, EditInventoryCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class EditInventoryCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal EditInventoryCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void EditInventory1CompletedEventHandler(object sender, EditInventory1CompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class EditInventory1CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal EditInventory1CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void DeactivateInventoryCompletedEventHandler(object sender, DeactivateInventoryCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class DeactivateInventoryCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal DeactivateInventoryCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void DeactivateInventory1CompletedEventHandler(object sender, DeactivateInventory1CompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class DeactivateInventory1CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal DeactivateInventory1CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void CreateEditRoomsCompletedEventHandler(object sender, CreateEditRoomsCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class CreateEditRoomsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal CreateEditRoomsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void CreateEditRooms1CompletedEventHandler(object sender, CreateEditRooms1CompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class CreateEditRooms1CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal CreateEditRooms1CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void UpdateOptionCategoriesCompletedEventHandler(object sender, UpdateOptionCategoriesCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class UpdateOptionCategoriesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal UpdateOptionCategoriesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void CreateOptionCompletedEventHandler(object sender, CreateOptionCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class CreateOptionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal CreateOptionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void CreateOption1CompletedEventHandler(object sender, CreateOption1CompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class CreateOption1CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal CreateOption1CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void EditOptionCompletedEventHandler(object sender, EditOptionCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class EditOptionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal EditOptionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void EditOption1CompletedEventHandler(object sender, EditOption1CompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class EditOption1CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal EditOption1CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void DeactivateOptionCompletedEventHandler(object sender, DeactivateOptionCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class DeactivateOptionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal DeactivateOptionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void DeactivateOption1CompletedEventHandler(object sender, DeactivateOption1CompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class DeactivateOption1CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal DeactivateOption1CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void CreateEditOptionAssignmentsCompletedEventHandler(object sender, CreateEditOptionAssignmentsCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class CreateEditOptionAssignmentsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal CreateEditOptionAssignmentsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void CreateEditOptionAssignments1CompletedEventHandler(object sender, CreateEditOptionAssignments1CompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class CreateEditOptionAssignments1CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal CreateEditOptionAssignments1CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void CreateIntersectionRuleCompletedEventHandler(object sender, CreateIntersectionRuleCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class CreateIntersectionRuleCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal CreateIntersectionRuleCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    //public delegate void EditIntersectionRuleCompletedEventHandler(object sender, EditIntersectionRuleCompletedEventArgs e);

    ///// <remarks/>
    //[System.Diagnostics.DebuggerStepThroughAttribute()]
    //[System.ComponentModel.DesignerCategoryAttribute("code")]
    //public partial class EditIntersectionRuleCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
    //    private object[] results;
        
    //    internal EditIntersectionRuleCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
