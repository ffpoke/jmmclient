﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JMMClient.JMMServerStreaming {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="JMMServerStreaming.IJMMServerStreaming")]
    public interface IJMMServerStreaming {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJMMServerStreaming/Download", ReplyAction="http://tempuri.org/IJMMServerStreaming/DownloadResponse")]
        System.IO.Stream Download(string fileName);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IJMMServerStreamingChannel : JMMClient.JMMServerStreaming.IJMMServerStreaming, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class JMMServerStreamingClient : System.ServiceModel.ClientBase<JMMClient.JMMServerStreaming.IJMMServerStreaming>, JMMClient.JMMServerStreaming.IJMMServerStreaming {
        
        public JMMServerStreamingClient() {
        }
        
        public JMMServerStreamingClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public JMMServerStreamingClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public JMMServerStreamingClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public JMMServerStreamingClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.IO.Stream Download(string fileName) {
            return base.Channel.Download(fileName);
        }
    }
}
