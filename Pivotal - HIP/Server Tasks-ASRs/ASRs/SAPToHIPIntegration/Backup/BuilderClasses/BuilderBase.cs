//
// $Workfile: BuilderBase.cs$
// $Revision: 41$
// $Author: RYong$
// $Date: Sunday, August 26, 2007 5:51:17 PM$
//
// Copyright © Pivotal Corporation
//


using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated;
//using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated.WsRooms;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using Pivotal.Interop.RDALib;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;



namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    /// <summary>
    /// Base class for all the builder classes.  A builder class is responsible for creating instances
    /// of Xsd generated classes.  The Xsd classes are based off the Xml schemas provided by Envision
    /// to talk to them via Ftp and web services.
    /// </summary>
    public abstract class BuilderBase
    {
        /// <summary>
        /// Holds a reference to a Xsd generated class.
        /// </summary>
        protected object xsdObject;

        internal string comments = string.Empty;

        /// <summary>
        /// Serializes the xsdObject into a XmlNode so that it can be passed in to web method calls as parameters.
        /// </summary>
        /// <returns></returns>
        virtual internal XmlNode ToXML()
        {
            const string BUILDERBASE_TOXML = "BuilderBase.ToXml(). ";

            try
            {
                if (xsdObject == null) return null; 

                MemoryStream memoryStream = new MemoryStream();
                StreamWriter streamWriter = new StreamWriter(memoryStream);
                XmlSerializer xmlSerializer = new XmlSerializer(this.xsdObject.GetType());
                xmlSerializer.Serialize(streamWriter, xsdObject);
                streamWriter.Close();

                UTF8Encoding encoding = new UTF8Encoding();
                string xmlString = encoding.GetString(memoryStream.GetBuffer());
                xmlString = xmlString.Trim(new char[] { '\0' });
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);

                if (!string.IsNullOrEmpty(comments))
                    xmlDoc.DocumentElement.AppendChild(xmlDoc.CreateComment(comments));
                return (XmlNode)xmlDoc.DocumentElement;
            }
            catch (Exception ex)
            {
                throw new PivotalApplicationException(BUILDERBASE_TOXML, ex);
            }
        }


        ///// <summary>
        ///// Helper function to convert a Pivotal Id into a string.
        ///// </summary>
        ///// <param name="characters">A Pivotal Id</param>
        ///// <returns></returns>
        //private String UTF8ByteArrayToString(Byte[] characters)
        //{

        //    UTF8Encoding encoding = new UTF8Encoding();
        //    String constructedString = encoding.GetString(characters);
        //    return (constructedString);
        //}



        /// <summary>
        /// Returns the xsdObject which is an instance of a Xsd generated class.
        /// </summary>
        /// <returns>xsdObject.</returns>
        virtual internal object ToObject()
        {
            return xsdObject;
        }


        /// <summary>
        /// Takes a Pivotal Id and returns it back if it has a value.  If it's System.TypeCode.DBNull,
        /// returns System.DBNull.Value.
        /// </summary>
        /// <param name="pivotalId"></param>
        /// <returns></returns>
        public static object QuerySafeId(object pivotalId)
        {
            if (Convert.IsDBNull(pivotalId))
            {
                return System.DBNull.Value;
            }
            else
            {
                return pivotalId;
            }
        }


        /// <summary>
        /// Strips "X" and leading zeros off the string pivotalId. 
        /// </summary>
        /// <param name="pivotalId">String representation of a hex pivotal Id.</param>
        /// <returns>The hex pivotal Id string without the leading zeros.</returns>
        /// <example>
        /// string compactHexId = PivotalId2IntegrationKey(mrsysSystem.IdToString(myPivotalId));
        /// </example>
        public static string CompactPivotalId(string pivotalId)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:X}", Convert.ToUInt64(pivotalId, 16));
        }


        /// <summary>
        /// Strips "X" and leading zeros off the string pivotalId. 
        /// </summary>
        /// <param name="compactHexId">String representation of a compacted hex pivotal Id.</param>
        /// <returns>The hex pivotal Id string with the leading zeros.</returns>
        /// <example>
        /// string fullPivotalId = UncompactPivotalId(mrsysSystem.IdToString(myCompactPivotalId));
        /// </example>
        public static string UncompactPivotalId(string compactHexId)
        {
            string backToHexId = "0000000000000000" + compactHexId;
            backToHexId = "0x" + backToHexId.Substring(backToHexId.Length - 16);
            return backToHexId;
        }


        /// <summary>
        /// Please refer to the other overloaded GetIntegrationKey function.
        /// </summary>
        /// <param name="locationReference"></param>
        /// <param name="pivotalId"></param>
        /// <param name="mrsysSystem"></param>
        /// <returns></returns>
        public static string GetIntegrationKey(LocationReferenceType locationReference, object pivotalId, IRSystem7 mrsysSystem)
        {
            return GetIntegrationKey(locationReference, pivotalId, null, mrsysSystem);       
        }


        /// <summary>
        /// Returns the IntegrationKey for a given geographical location.  An IntegrationKey is made up of a prefix (reg, div, com, rel) and the primary key.  Plan assignment is an exception due to wildcarding.  Its IntegrationKey is made up of the current release and the plan Id seperated by a colon.
        /// </summary>
        /// <param name="locationReference">Indicates the level of the current geographical location.</param>
        /// <param name="pivotalId">The primary key of the current geographical location.</param>
        /// <param name="currentReleaseContextId">If geographical location is a plan assignment (wildcarding), specify the release used to uniquely identify this instance of plan assignment for Envision.  Envision doesn't allow wildcarded plan assignments.</param>
        /// <param name="mrsysSystem">For data access and built-in functions.</param>
        /// <returns>A string representating the IntegrationKey of the current geographical location.  For examples: "reg1234", "divAB3d", "com73D", "rel78C3", "78C3:343F".</returns>
        public static string GetIntegrationKey(LocationReferenceType locationReference, object pivotalId, object currentReleaseContextId, IRSystem7 mrsysSystem)
        {
            if (mrsysSystem == null)
                throw new ArgumentNullException("mrsysSystem");

            StringBuilder stringBuilder = new StringBuilder();
            switch (locationReference)
            {
                case LocationReferenceType.Corporate:
                    stringBuilder.Append(EnvisionIntegration.LocationLevel.CodeCorporation);
                    stringBuilder.Append((string)pivotalId);
                    return stringBuilder.ToString();
                case LocationReferenceType.Region:
                    stringBuilder.Append(EnvisionIntegration.LocationLevel.CodeRegion);
                    break;
                case LocationReferenceType.Division:
                    stringBuilder.Append(EnvisionIntegration.LocationLevel.CodeDivision);
                    break;
                case LocationReferenceType.Community:
                    stringBuilder.Append(EnvisionIntegration.LocationLevel.CodeCommunity);
                    break;
                case LocationReferenceType.Release:
                    stringBuilder.Append(EnvisionIntegration.LocationLevel.CodeRelease);
                    break;
                case LocationReferenceType.Plan:
                    // For plan, the pivotalId passed in is the NBHDP_Product_Id (plan assignment), but we want to 
                    // pass the Division_Product_Id (plan definition) to Envision.
                    // The integration key format is "<ReleaseId>:<DivisionProductId>"
                    {
                        DataAccess objLib = (DataAccess)mrsysSystem.ServerScripts[AppServerRuleData.DataAccessAppServerRuleName].CreateInstance();
                        if (currentReleaseContextId == null)
                        {
                            object releaseId = objLib.SqlIndex(NBHDPProductData.TableName, NBHDPProductData.NBHDPhaseIdField, pivotalId);
                            stringBuilder.Append(CompactPivotalId(mrsysSystem.IdToString(releaseId)));
                        }
                        else
                        {
                            stringBuilder.Append(CompactPivotalId(mrsysSystem.IdToString(currentReleaseContextId)));
                        }
                        stringBuilder.Append(":");
                        object planId = objLib.SqlIndex(NBHDPProductData.TableName, NBHDPProductData.DivisionProductIdField, pivotalId);
                        stringBuilder.Append(CompactPivotalId(mrsysSystem.IdToString(planId)));
                        return stringBuilder.ToString();
                    }

                default:
                    return "";
            }

            stringBuilder.Append(CompactPivotalId(mrsysSystem.IdToString(pivotalId)));
            return stringBuilder.ToString();
        }
    }
}
