﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CRM.Pivotal.IAC.SitewireTest.PDFGenService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PDFFault", Namespace="http://schemas.datacontract.org/2004/07/PDFGenService.DataContracts")]
    [System.SerializableAttribute()]
    public partial class PDFFault : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="SitewireTest.PDFGenService.IPDFGenService")]
    public interface IPDFGenService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPDFGenService/GetEBrochure", ReplyAction="http://tempuri.org/IPDFGenService/GetEBrochureResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(CRM.Pivotal.IAC.SitewireTest.PDFGenService.PDFFault), Action="http://tempuri.org/IPDFGenService/GetEBrochurePDFFaultFault", Name="PDFFault", Namespace="http://schemas.datacontract.org/2004/07/PDFGenService.DataContracts")]
        byte[] GetEBrochure(string eBrochureId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPDFGenService/GetEBrochureId", ReplyAction="http://tempuri.org/IPDFGenService/GetEBrochureIdResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(CRM.Pivotal.IAC.SitewireTest.PDFGenService.PDFFault), Action="http://tempuri.org/IPDFGenService/GetEBrochureIdPDFFaultFault", Name="PDFFault", Namespace="http://schemas.datacontract.org/2004/07/PDFGenService.DataContracts")]
        string GetEBrochureId(string xml);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IPDFGenServiceChannel : CRM.Pivotal.IAC.SitewireTest.PDFGenService.IPDFGenService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class PDFGenServiceClient : System.ServiceModel.ClientBase<CRM.Pivotal.IAC.SitewireTest.PDFGenService.IPDFGenService>, CRM.Pivotal.IAC.SitewireTest.PDFGenService.IPDFGenService {
        
        public PDFGenServiceClient() {
        }
        
        public PDFGenServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public PDFGenServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PDFGenServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public PDFGenServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public byte[] GetEBrochure(string eBrochureId) {
            return base.Channel.GetEBrochure(eBrochureId);
        }
        
        public string GetEBrochureId(string xml) {
            return base.Channel.GetEBrochureId(xml);
        }
    }
}
