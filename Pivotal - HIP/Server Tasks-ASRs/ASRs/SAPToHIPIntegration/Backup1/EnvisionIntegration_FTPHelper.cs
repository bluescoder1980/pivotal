//
// $Workfile: EnvisionIntegration_FTPHelper.cs$
// $Revision: 2$
// $Author: RYong$
// $Date: Wednesday, December 19, 2007 11:57:50 AM$
//
// Copyright © Pivotal Corporation
//


using CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server.EnvisionXsdGenerated;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace CdcSoftware.Pivotal.Applications.HomeBuilders.EF.Server
{
    public partial class EnvisionIntegration: IRAppScript
    {
        /// <summary>
        /// After the division setup xml file is processed successfully, this function can be called to
        /// activate the divisions for synchronization.
        /// </summary>
        /// <param name="rstNewDivisions">Recordset of division Ids</param>
        protected virtual void AddDivisionsToIntegration(Recordset rstNewDivisions)
        {
            if (rstNewDivisions == null) return;
            if (rstNewDivisions.RecordCount <= 0) return;

            rstNewDivisions.MoveFirst();
            while (!rstNewDivisions.EOF)
            {
                Recordset rstDiv = PivotalDataAccess.GetRecordset(rstNewDivisions.Fields[DivisionData.DivisionIdField].Value,
                    DivisionData.TableName, DivisionData.EnvEnvisionActivatedField, DivisionData.EnvSetupBeingProcessedField);
                if (rstDiv.RecordCount > 0)
                {
                    rstDiv.MoveFirst();
                    rstDiv.Fields[DivisionData.EnvEnvisionActivatedField].Value = false;
                    rstDiv.Fields[DivisionData.EnvSetupBeingProcessedField].Value = true;

                    PivotalDataAccess.SaveRecordset(DivisionData.TableName, rstDiv);
                }
                rstNewDivisions.MoveNext();
            }
        }


        /// <summary>
        /// Sent all pending divisions that were sent to Envision via FTP to integrated/activated.
        /// </summary>
        protected virtual void SetPendingDivisionsToActivatedFTP()
        {
            string fileName = "";
            Recordset recordset = PivotalDataAccess.GetRecordset(SystemData.TableName, SystemData.EnvLastGeneratedFtpFileField);
            try
            {
                if (TypeConvert.ToString(recordset.Fields[SystemData.EnvLastGeneratedFtpFileField].Value) != string.Empty)
                    fileName = new FileInfo(TypeConvert.ToString(recordset.Fields[SystemData.EnvLastGeneratedFtpFileField].Value)).Name;
                else
                    return; //do nothing.  Admin not supposed to click on "Confirm pending changes" when there's not 
                            //ftp file generated.

            }
            finally
            {
                recordset.Close();
            }

            Sync.SetCurrentFTPSuccessState(fileName);

            Recordset divisionRst = PivotalDataAccess.GetRecordset("Env: Divisions with setup being processed", 0
                , new string[] { DivisionData.DivisionIdField
                               , DivisionData.EnvEnvisionActivatedField
                               , DivisionData.EnvSetupBeingProcessedField });
            try
            {

                if (divisionRst.RecordCount > 0)
                {
                    divisionRst.MoveFirst();
                    for (int i = 0; i < divisionRst.RecordCount; i++)
                    {
                        divisionRst.Fields[DivisionData.EnvSetupBeingProcessedField].Value = false;
                        divisionRst.Fields[DivisionData.EnvEnvisionActivatedField].Value = true;
                        divisionRst.MoveNext();
                    }
                }

                PivotalDataAccess.SaveRecordset(DivisionData.TableName, divisionRst);
            }
            finally
            {
                divisionRst.Close();
            }


            recordset = PivotalDataAccess.GetRecordset(SystemData.TableName, SystemData.EnvLastFtpChangesConfirmedField);
            try
            {
                recordset.Fields[SystemData.EnvLastFtpChangesConfirmedField].Value = DateTime.Now;
                PivotalDataAccess.SaveRecordset(SystemData.TableName, recordset);
            }
            finally
            {
                recordset.Close();
            }
        }

        /// <summary>
        /// Sent all pending divisions that were sent to Envision via Web Service to integrated/activated.
        /// </summary>
        protected virtual void SetPendingDivisionsToActivatedWS()
        {
            // note - FTP and WS currently use the same mechanism.
            SetPendingDivisionsToActivatedFTP();
        }

        /// <summary>
        /// This function sets the "Envision activated" and "setup being processed" flags on all division
        /// records passed in
        /// </summary>
        protected virtual void RemoveDivisionsFromIntegration(Recordset rstSelectedDivisions)
        {
            if (rstSelectedDivisions == null) return;
            if (rstSelectedDivisions.RecordCount <= 0) return;

            rstSelectedDivisions.MoveFirst();
            while (!rstSelectedDivisions.EOF)
            {
                Recordset rstDiv = PivotalDataAccess.GetRecordset(rstSelectedDivisions.Fields[DivisionData.DivisionIdField].Value,
                    DivisionData.TableName, DivisionData.EnvEnvisionActivatedField, DivisionData.EnvSetupBeingProcessedField);
                if (rstDiv.RecordCount > 0)
                {
                    rstDiv.MoveFirst();
                    rstDiv.Fields[DivisionData.EnvEnvisionActivatedField].Value = false;
                    rstDiv.Fields[DivisionData.EnvSetupBeingProcessedField].Value = false;

                    PivotalDataAccess.SaveRecordset(DivisionData.TableName, rstDiv);
                }
                rstSelectedDivisions.MoveNext();
            }
        }

        /// <summary>
        /// This functions sets the SetupBeingProcessed flag for the specified divisions.
        /// </summary>
        /// <param name="arrDivisionIds">Array of Division Ids</param>
        protected virtual void SetDivisionSetupBeingProcessed(object[] arrDivisionIds)
        {
            for (int i = 0; i < arrDivisionIds.GetLength(0); i++)
            {
                Recordset divisionRst = PivotalDataAccess.GetRecordset(arrDivisionIds[i], DivisionData.TableName, DivisionData.EnvSetupBeingProcessedField);
                divisionRst.MoveFirst();
                divisionRst.Fields[DivisionData.EnvSetupBeingProcessedField].Value = true;
                divisionRst.Fields[DivisionData.EnvEnvisionActivatedField].Value = false;
                PivotalDataAccess.SaveRecordset(DivisionData.TableName, divisionRst);
            }
        }

        /// <summary>
        /// Get list of generated file names for specified age.
        /// </summary>
        /// <param name="ageInDays">Age in days.</param>
        /// <returns>Recordset of filenames with column name "Filename".</returns>
        protected virtual object GetGeneratedFileList(int ageInDays)
        {
            ArrayList fileList = new ArrayList();

            DirectoryInfo dir = new DirectoryInfo(@Config.FtpTempDirectory);
            //DirectoryInfo dir = new DirectoryInfo(@"c:\\EnvisionXmlFeedFiles");            
            foreach (FileInfo f in dir.GetFiles("*.xml"))
            {
                TimeSpan ts = DateTime.Now - f.CreationTime;
                if (ts.TotalDays <= ageInDays)
                {
                    fileList.Add(f.Name);                    
                }
            }

            fileList.Sort();
            fileList.Reverse();
            return (object[])fileList.ToArray();
        }



        protected virtual void SendLatestFtpFile()
        {
            new FtpService(this).Send(Config.EnvisionLatestFtpFile);
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="neighborhoodProdId"></param>
        /// <param name="fromOptionAvailableTo"></param>
        /// <param name="toOptionAvailableTo"></param>
        /// <param name="warningMessage"></param>
        /// <returns></returns>
        protected virtual bool OptionAvailableToChangeAllowed(object neighborhoodProdId, string fromOptionAvailableTo, string toOptionAvailableTo, out string warningMessage)
        {
            warningMessage = "";
            Recordset rstOpps = null;

            string strSpecific = TypeConvert.ToString(PivotalSystem.GetLDGroup("").GetText("CHC: S-0-2409"));
            string strAllLocations = TypeConvert.ToString(PivotalSystem.GetLDGroup("").GetText("CHC: S-0-2408"));
            if ((fromOptionAvailableTo == strSpecific && toOptionAvailableTo == strAllLocations) ||
                fromOptionAvailableTo == string.Empty)
            {
                return true; //only allow change from Specific to All Locations with no validation.
            }

            rstOpps = PivotalDataAccess.GetRecordset(OpportunityData.QueryOpportunitiesWithLinkedNeighborhoodProduct,
                1, neighborhoodProdId, OpportunityData.OpportunityIdField);

            if (rstOpps.RecordCount > 0)
            {                
                warningMessage = TypeConvert.ToString(LangDictionary.GetTextSub("MessageOptAvailChangeNotAllowed", new object[] {
                    fromOptionAvailableTo, toOptionAvailableTo}));
                return false;
            }

            return true;
        }
    }
}
