//
// $Workfile: EnvisionHttpClientProtocol.cs$
// $Revision: 1$
// $Author: tlyne$
// $Date: Wednesday, March 07, 2007 4:39:50 PM$
//
// Copyright © Pivotal Corporation
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// An HttpClientProtocol class that logs web service request and response SOAP envelopes and records performance.
    /// </summary>
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public class EnvisionHttpClientProtocol : System.Web.Services.Protocols.SoapHttpClientProtocol
    {
        // Reference to main Asr in order to reuse services
        private EnvisionIntegration m_envisionIntegration;

        // string builders for holding the request and response
        private StringBuilder m_soapRequest;
        private StringBuilder m_soapResponse;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="envisionIntegration">Reference to the an Envision Integration instance</param>
        public EnvisionHttpClientProtocol(EnvisionIntegration envisionIntegration)
        {
            this.m_envisionIntegration = envisionIntegration;
        }

        /// <summary>
        /// Web Method invoke.
        /// </summary>
        /// <param name="methodName">The Method Name</param>
        /// <param name="parameters">Web Method parameters</param>
        /// <returns>Web Method return parameters</returns>
        protected new object[] Invoke(string methodName, object[] parameters)
        {

            string comments = string.Empty;
            
            // Move the Xml comment from the request into a string variable.
            // Envision cannot handle Xml comments.
            if (parameters.Length > 1)
            {
                if (parameters[1] is System.Xml.XmlNode)
                {
                    System.Xml.XmlNode operationNode = (System.Xml.XmlNode)parameters[1];
                    foreach (System.Xml.XmlNode node in operationNode.SelectNodes("//comment()"))
                    {
                        if (node.NodeType == System.Xml.XmlNodeType.Comment)
                        {
                            comments = node.InnerText;
                            node.ParentNode.RemoveChild(node);
                        }
                    }
                }
            }

            // invoke the web service, noting the elaps time.
            DateTime elapseStart = DateTime.Now;
            object[] returnParams = base.Invoke(methodName, parameters);
            double totalSeconds = DateTime.Now.Subtract(elapseStart).TotalSeconds;
            
            // build the request string into an Xml doc.
            System.Xml.XmlDocument requestDoc = new System.Xml.XmlDocument();
            requestDoc.LoadXml(this.m_soapRequest.ToString());

            // log request the Xml
            // If comments exist, display it on the log.
            if (string.IsNullOrEmpty(comments))
                this.m_envisionIntegration.Log.WriteXml((string)this.m_envisionIntegration.LangDictionary.GetTextSub("LogXmlSoapRequest", new string[] { methodName, this.Url }), requestDoc);
            else
                this.m_envisionIntegration.Log.WriteXml(comments, requestDoc);

            // build the response string into an Xml doc.
            System.Xml.XmlDocument responseDoc = new System.Xml.XmlDocument();
            responseDoc.LoadXml(this.m_soapResponse.ToString());

            // log the response Xml
            this.m_envisionIntegration.Log.WriteXml((string)this.m_envisionIntegration.LangDictionary.GetTextSub("LogXmlSoapResponse", new string[] { methodName, this.Url }), responseDoc);

            // log the performance
            this.m_envisionIntegration.Log.WritePerformance((string)this.m_envisionIntegration.LangDictionary.GetTextSub("LogPerformanceWebServiceElapse", new string[] { methodName, totalSeconds.ToString(CultureInfo.CurrentCulture) }));

            // return the return parameters
            return returnParams;
        }



        /// <summary>
        /// Substitutes a custom XmlWriter that will write out the SOAP Envelope to the m_soapRequest variable.
        /// </summary>
        /// <param name="message">The data in the SOAP request.</param>
        /// <param name="bufferSize">Size of the data</param>
        /// <returns>Custom XmlWriter</returns>
        protected override System.Xml.XmlWriter GetWriterForMessage(System.Web.Services.Protocols.SoapClientMessage message, int bufferSize)
        {
            // re-initialize m_soapRequest to insure it is empty
            this.m_soapRequest = new StringBuilder(bufferSize);
            return System.Xml.XmlWriter.Create(new EnvisionLoggingWriter(message.Stream, this.m_soapRequest, bufferSize));
        }

        /// <summary>
        /// Substitutes a custom XmlReader that will write out the SOAP Envelope to the m_soapResponse variable.
        /// </summary>
        /// <param name="message">The data in the SOAP response</param>
        /// <param name="bufferSize">Size of the data</param>
        /// <returns>Custom XmlReader</returns>
        protected override System.Xml.XmlReader GetReaderForMessage(System.Web.Services.Protocols.SoapClientMessage message, int bufferSize)
        {
            return System.Xml.XmlReader.Create(new EnvisionLoggingReader(message.Stream, out this.m_soapResponse, bufferSize));
        }

    }
}
