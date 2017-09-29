//
// $Workfile: ValidateXml.cs$
// $Revision: 3$
// $Author: tlyne$
// $Date: Thursday, January 24, 2008 11:19:09 AM$
//
// Copyright © Pivotal Corporation
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Pivotal.Interop.RDALib;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// Provides a mechanism for validate Xml agains a stored schema
    /// </summary>
    /// <remarks>validate can be turned on/off via settings</remarks>
    public class ValidateXml
    {
        // Language string for writing validation info messages to the log
        public const string LangStringValidateXml = "LogInfoValidateXml";

        /// <summary>
        /// Client script names that contain the xml schemas
        /// </summary>
        protected static class SchemaName
        {
            public const string BuyerSelection = "EnvisionBuyerSelections";
            public const string Buyer = "EnvisionDcmWsBuyer";
            public const string Home = "EnvisionDcmWsHome";
            public const string Inventory = "EnvisionOmWsInventory";
            public const string OptionAssignments = "EnvisionOmWsOptnAssignmnts";
            public const string Option = "EnvisionOmWsOption";
            public const string LocationReference = "EnvisionWsLocationReference";
            public const string Output = "EnvisionWsOutput";
            public const string FtpOption = "EnvisionOption";
        }

        /// <summary>
        /// Validates an xml document against an xml schema and throws an Exception if the xml does not match
        /// </summary>
        /// <param name="xmlDocument">Xml node/document to validate</param>
        /// <param name="xmlSchema">Xml schema to validate against</param>
        protected static void ValidateXmlAgainstSchema(XmlNode xmlDocument, System.Xml.Schema.XmlSchema xmlSchema)
        {
            //setup the xml reader
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(xmlSchema);
            settings.ValidationType = ValidationType.Schema;

            // parse the xml
            using (System.IO.StringReader stringReader = new System.IO.StringReader(xmlDocument.OuterXml))
            {
                using (XmlReader xmlReader = XmlReader.Create(stringReader, settings))
                {
                    while (xmlReader.Read()) ;
                    xmlReader.Close();
                }
            }
        }

        /// <summary>
        /// Pivotal System reference
        /// </summary>
        private IRSystem7 m_rdaSystem;

        /// <summary>
        /// Envision Integration Language dictionary reference
        /// </summary>
        private ILangDict m_langDictionary;

        /// <summary>
        /// Envision Integration Configuration reference
        /// </summary>
        private Configuration m_config;

        /// <summary>
        /// Dictionary of previously used schemas that are cached for re-use
        /// </summary>
        private Dictionary<string, XmlSchema> m_schemaCache = new Dictionary<string,XmlSchema>();

        /// <summary>
        /// Envision Integration Logging reference
        /// </summary>
        private Logging m_log;


        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="rdaSystem">Pivotal System Reference</param>
        /// <param name="config">Envision Configuration reference</param>
        /// <param name="log">Envision Logging reference</param>
        /// <param name="langDictionary">Pivotal Language Dictionary reference</param>
        public ValidateXml(IRSystem7 rdaSystem, Configuration config, Logging log, ILangDict langDictionary)
        {

            if (rdaSystem == null)
                throw new ArgumentNullException("rdaSystem");
            this.m_rdaSystem = rdaSystem;

            if (langDictionary == null)
                throw new ArgumentNullException("langDictionary");
            this.m_langDictionary = langDictionary;

            if (config == null)
                throw new ArgumentNullException("config");
            this.m_config = config;

            if (log == null)
                throw new ArgumentNullException("log");
            this.m_log = log;
        }

        /// <summary>
        /// Returns an Xml schema out of the Pivotal Client Scripts database
        /// </summary>
        /// <param name="clientScriptName">The name of the Client Script to retrieve</param>
        /// <returns>Xml Schema entity populated with the schema from the database</returns>
        /// <remarks>Note, that retrieved schemas are cached so that if the schema is needed again
        /// we don't have to go all the way back to the database</remarks>
        protected virtual System.Xml.Schema.XmlSchema LoadSchema(string clientScriptName)
        {
            // check if schema is in cache, if so return schema.
            if (this.m_schemaCache.ContainsKey(clientScriptName))
                return this.m_schemaCache[clientScriptName];

            // pull the schema text out of the database
            string schemaText = this.m_rdaSystem.ClientScripts[clientScriptName].Text;

            XmlSchema xmlSchema;
            using (System.IO.StringReader reader = new System.IO.StringReader(schemaText))
                xmlSchema = XmlSchema.Read(reader, null);

            this.m_schemaCache.Add(clientScriptName, xmlSchema);

            return xmlSchema;
        }

        /// <summary>
        /// Validates BuyerSelections Xml
        /// </summary>
        /// <param name="xml">Xml that defines Envision Buyer Selections</param>
        public virtual void BuyerSelections(XmlNode xml)
        {
            if (this.m_config.ValidateXml)
            {
                this.m_log.WriteInformation((string)this.m_langDictionary.GetTextSub(LangStringValidateXml, new string[] { SchemaName.BuyerSelection }));
                XmlSchema schema = LoadSchema(SchemaName.BuyerSelection);
                ValidateXmlAgainstSchema(xml, schema);
            }
        }

        /// <summary>
        /// Validates Buyer Xml
        /// </summary>
        /// <param name="xml">Xml that defines an Envision Buyer entity</param>
        public virtual void Buyer(XmlNode xml)
        {
            if (this.m_config.ValidateXml)
            {
                this.m_log.WriteInformation((string)this.m_langDictionary.GetTextSub(LangStringValidateXml, new string[] { SchemaName.Buyer }));
                XmlSchema schema = LoadSchema(SchemaName.Buyer);
                ValidateXmlAgainstSchema(xml, schema);
            }
        }

        /// <summary>
        /// Validates Home Xml
        /// </summary>
        /// <param name="xml">Xml the defines an Envision Home entity</param>
        public virtual void Home(XmlNode xml)
        {
            if (this.m_config.ValidateXml)
            {
                this.m_log.WriteInformation((string)this.m_langDictionary.GetTextSub(LangStringValidateXml, new string[] { SchemaName.Home }));
                XmlSchema schema = LoadSchema(SchemaName.Home);
                ValidateXmlAgainstSchema(xml, schema);
            }
        }


        /// <summary>
        /// Validates Inventory Xml
        /// </summary>
        /// <param name="xml">Xml that defines an Envision Inventory entity</param>
        public virtual void Inventory(XmlNode xml)
        {
            if (this.m_config.ValidateXml)
            {
                this.m_log.WriteInformation((string)this.m_langDictionary.GetTextSub(LangStringValidateXml, new string[] { SchemaName.Inventory }));
                XmlSchema schema = LoadSchema(SchemaName.Inventory);
                ValidateXmlAgainstSchema(xml, schema);
            }
        }

        /// <summary>
        /// Validates Option Assignments Xml
        /// </summary>
        /// <param name="xml">Xml that defines an Envision OptionAssignments entity</param>
        public virtual void OptionAssignments(XmlNode xml)
        {
            if (this.m_config.ValidateXml)
            {
                this.m_log.WriteInformation((string)this.m_langDictionary.GetTextSub(LangStringValidateXml, new string[] { SchemaName.OptionAssignments }));
                XmlSchema schema = LoadSchema(SchemaName.OptionAssignments);
                ValidateXmlAgainstSchema(xml, schema);
            }
        }

        /// <summary>
        /// Validates Option Xml
        /// </summary>
        /// <param name="xml">Xml that defines an Envision Option entity</param>
        public virtual void Option(XmlNode xml)
        {
            if (this.m_config.ValidateXml)
            {
                this.m_log.WriteInformation((string)this.m_langDictionary.GetTextSub(LangStringValidateXml, new string[] { SchemaName.Option }));
                XmlSchema schema = LoadSchema(SchemaName.Option);
                ValidateXmlAgainstSchema(xml, schema);
            }
        }


        /// <summary>
        /// Validates Location Reference Xml
        /// </summary>
        /// <param name="xml">Xml that defines a Location Reference entity</param>
        public virtual void LocationReference(XmlNode xml)
        {
            if (this.m_config.ValidateXml)
            {
                this.m_log.WriteInformation((string)this.m_langDictionary.GetTextSub(LangStringValidateXml, new string[] { SchemaName.LocationReference }));
                XmlSchema schema = LoadSchema(SchemaName.LocationReference);
                ValidateXmlAgainstSchema(xml, schema);
            }
        }

        /// <summary>
        /// Validates Output Xml
        /// </summary>
        /// <param name="xml">Xml that define an Envision Output entity</param>
        public virtual void Output(XmlNode xml)
        {
            if (this.m_config.ValidateXml)
            {
                this.m_log.WriteInformation((string)this.m_langDictionary.GetTextSub(LangStringValidateXml, new string[] { SchemaName.Output }));
                XmlSchema schema = LoadSchema(SchemaName.Output);
                ValidateXmlAgainstSchema(xml, schema);
            }
        }

        /// <summary>
        /// Validates FtpOption Xml
        /// </summary>
        /// <param name="fileName">Name of the Ftp Xml file.</param>
        public virtual void FtpOption(string fileName)
        {
            if (this.m_config.ValidateXml)
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(fileName);

                this.m_log.WriteInformation((string)this.m_langDictionary.GetTextSub(LangStringValidateXml, new string[] { SchemaName.FtpOption }));
                XmlSchema schema = LoadSchema(SchemaName.FtpOption);
                ValidateXmlAgainstSchema(xml, schema);
            }
        }
    }
}
