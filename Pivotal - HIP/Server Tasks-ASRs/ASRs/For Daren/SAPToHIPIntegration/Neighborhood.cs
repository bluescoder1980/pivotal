using System;
using System.Collections.Generic;
using System.Text;
//Add Pivotal namespaces
using Pivotal.Interop.ADODBLib;
using Pivotal.Interop.RDALib;
using Pivotal.Interop.COMAdminLib;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Utility;
using CdcSoftware.Pivotal.Applications.Foundation.Server.BusinessRule;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Choice;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Form;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.LDGroup;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Query;
using CdcSoftware.Pivotal.Applications.Foundation.Server.Data.Element.Table;

namespace CRM.Pivotal.IP.SAPToHIPIntegration
{

    /// <summary>
    /// MI Homes - Communities will by synchronized from JDE to Pivotal via Fusion. 
    /// The community data will be entered into JDE and sent to Pivotal 
    /// on completion. It is understood that if required fields are not set 
    /// in JDE the Pivotal record will not be able to be updated in Pivotal. 
    /// If the community received is currently not in Pivotal, it will be created. 
    /// If it already exists, it will be updated. Only communities in “live” divisions 
    /// will be sent to Pivotal.  Fusion will coordinate what release/communities 
    /// are sent to Pivotal and when they are ready for synchronization. 
    /// </summary>
    /// 
    /// <Author>A.Maldonado</Author>
    /// <CreateDate>10/17/2007</CreateDate>
    /// <Revision>1.0</Revision>
    class Neighborhood : IRFormScript
    {

        #region Class Variables

        IRSystem7 mrsysSystem;

        #endregion

        #region IRFormScript Members


        /// <summary>
        /// For inserts need to make sure integration logic is performed on record before
        /// being written to Pivotal DB.
        /// </summary>
        /// <param name="pForm">HBIntNeighborhood</param>
        /// <param name="Recordsets"></param>
        /// <param name="ParameterList"></param>
        /// <returns></returns>
        public object AddFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {

            try
            {
                //Get incoming recordset
                object[] recordsetArray = (object[])Recordsets;
                Recordset rstNeighborhood = (Recordset)recordsetArray[0];
                                
                //SetFields 
                IntegrationUtility util = new IntegrationUtility();

                //Build Object Array with values to use for Lookup
                object[] arrLookUpFieldVals = new object[] 
                { 
                    new object[] {true, pForm.Segments[1].SegmentName, IntegrationConstants.gstrfCOMMUNITY_COUNTY},
                    new object[] {false, pForm.Segments[0].SegmentName, IntegrationConstants.gstrfSTATE}
                };

               //Build Object Array with values to set on integration form
                object[] arrSetFieldVals = new object[] 
                { 
                    IntegrationConstants.gstrfCOUNTY_ID 
                };

               
                //This method will use object arrays passed in to dynamically perform lookups and set fields 
                //- 1) Pivotal RSystem object
                //- 2) HBIntNeighborhood Form Object
                //- 3) Incoming recordset
                //- 4) Empty object array (place holder for lookup values)
                //- 5) Object Array with Look Up Field values (defined above)
                //- 6) Object Array with field values to set (defined above)
                //- 7) Name of table to do look ups on
                //- 8) Name of Query to use to do lookups on
                //- 9) Table fields ???

                util.SetValuesByLookUp(mrsysSystem, pForm, rstNeighborhood, null, arrLookUpFieldVals, arrSetFieldVals,
                    IntegrationConstants.gstrtCOUNTY,
                    IntegrationConstants.gstrqCOUNTY_BY_NAME_STATE,
                    new object[] { IntegrationConstants.gstrfCOUNTY_ID });

                //Set Division
                util.SetDivision(mrsysSystem, pForm, rstNeighborhood);

                //Set Neighborhood Managers
                //util.SetNeighborhoodManagers(mrsysSystem, rstNeighborhood);

                //2007.11.19 - AAM MI Specific logic
                //Set Open/Closed Date based on Status
                if (rstNeighborhood.Fields[IntegrationConstants.strfSTATUS].Value.ToString() == "Open")
                {
                    //Set Open_Date = Today
                    rstNeighborhood.Fields[IntegrationConstants.strfOPEN_DATE].Value = DateTime.Now;
                    //Clear out Closed_Date
                    rstNeighborhood.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value = DBNull.Value;
                }
                else if (rstNeighborhood.Fields[IntegrationConstants.strfSTATUS].Value.ToString() == "Closed")
                {
                    //Set Closed Date
                    rstNeighborhood.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value = DateTime.Now;
                }
                else
                {
                    //If neither open nor closed null out open/closed dates
                    rstNeighborhood.Fields[IntegrationConstants.strfOPEN_DATE].Value = DBNull.Value;
                    rstNeighborhood.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value = DBNull.Value;
                }


            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }

            return pForm.DoAddFormData(Recordsets, ref ParameterList);
        }

        public void DeleteFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            pForm.DoDeleteFormData(RecordId, ref ParameterList);
        }

        public void Execute(IRForm pForm, string MethodName, ref object ParameterList)
        { }

        public object LoadFormData(IRForm pForm, object RecordId, ref object ParameterList)
        {
            return pForm.DoLoadFormData(RecordId, ref ParameterList);
        }

        public object NewFormData(IRForm pForm, ref object ParameterList)
        {
            return pForm.DoNewFormData(ref ParameterList);
        }

        public void NewSecondaryData(IRForm pForm, object SecondaryName, ref object ParameterList, ref Recordset Recordset)
        {
            pForm.DoNewSecondaryData(SecondaryName, ref ParameterList, Recordset);
        }

        /// <summary>
        /// For update need to make sure integration logic is performed on record before
        /// being written to Pivotal DB.
        /// </summary>
        /// <param name="pForm">HBIntCompany</param>
        /// <param name="Recordsets">Incoming recordset from Pervasive work</param>
        /// <param name="ParameterList"></param>
        public void SaveFormData(IRForm pForm, object Recordsets, ref object ParameterList)
        {
            try
            {
                //Get incoming recordset
                object[] recordsetArray = (object[])Recordsets;
                Recordset rstNeighborhood = (Recordset)recordsetArray[0];

                //Utility class 
                IntegrationUtility util = new IntegrationUtility();

                //Build Object Array with values to use for Lookup
                object[] arrLookUpFieldVals = new object[] 
                { 
                    new object[] {true, pForm.Segments[1].SegmentName, IntegrationConstants.gstrfCOMMUNITY_COUNTY},
                    new object[] {false, pForm.Segments[0].SegmentName, IntegrationConstants.gstrfSTATE}
                };

                //Build Object Array with fields to be set on Integration Form
                object[] arrSetFieldVals = new object[] 
                { 
                    IntegrationConstants.gstrfCOUNTY_ID 
                };

                //This method will use object arrays passed in to dynamically perform lookups and set fields 
                //- 1) Pivotal RSystem object
                //- 2) HBIntCompany Form Object
                //- 3) Incoming recordset
                //- 4) Empty object array (place holder for lookup values)
                //- 5) Object Array with Look Up Field values (defined above)
                //- 6) Object Array with field values to set (defined above)
                //- 7) Name of table to do look ups on
                //- 8) Name of Query to use to do lookups on
                //- 9) Table fields ???

                util.SetValuesByLookUp(mrsysSystem, pForm, rstNeighborhood, null, arrLookUpFieldVals, arrSetFieldVals,
                    IntegrationConstants.gstrtCOUNTY,
                    IntegrationConstants.gstrqCOUNTY_BY_NAME_STATE,
                    new object[] { IntegrationConstants.gstrfCOUNTY_ID });

                //Set Division
                util.SetDivision(mrsysSystem, pForm, rstNeighborhood);

                //Set Neighborhood Managers
                //util.SetNeighborhoodManagers(mrsysSystem, rstNeighborhood);

                //2007.11.19 - AAM MI Specific logic
                //Set Open/Closed Date based on Status
                if (rstNeighborhood.Fields[IntegrationConstants.strfSTATUS].Value.ToString() == "Open")
                {
                    //Only update Release Date if not already set.
                    if (DBNull.Value == rstNeighborhood.Fields[IntegrationConstants.strfOPEN_DATE].Value)
                    { 
                        //Set Open_Date = Today
                        rstNeighborhood.Fields[IntegrationConstants.strfOPEN_DATE].Value = DateTime.Now;
                    }
                    //Clear out Closed_Date
                    rstNeighborhood.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value = DBNull.Value;
                }
                else if (rstNeighborhood.Fields[IntegrationConstants.strfSTATUS].Value.ToString() == "Closed")
                {
                    //Only update Closed Date if not already set
                    if (DBNull.Value == rstNeighborhood.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value)
                    {
                        rstNeighborhood.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value = DateTime.Now;
                    }
                }
                else
                {
                    //If neither open nor closed null out open/closed dates
                    rstNeighborhood.Fields[IntegrationConstants.strfOPEN_DATE].Value = DBNull.Value;
                    rstNeighborhood.Fields[IntegrationConstants.strfCOMM_CLOSE_DATE].Value = DBNull.Value;
                }
   
            }
            catch (Exception e)
            {
                throw new PivotalApplicationException(e.Message, e, mrsysSystem);
            }

            //Call DoSaveForm Data
            pForm.DoSaveFormData(Recordsets, ref ParameterList);
        }

        public void SetSystem(RSystem pSystem)
        {

            mrsysSystem = (IRSystem7)pSystem;
        }

        #endregion

    }
}
