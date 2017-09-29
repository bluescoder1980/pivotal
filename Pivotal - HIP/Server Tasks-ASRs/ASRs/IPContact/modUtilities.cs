using System;
using ADODB;
using RDALib;
using Microsoft.VisualBasic;
namespace PHbContact
{
    public class modUtilities
    {

        // ==========================================================================================
        // Project Name: CMSCompany, CMSProduct, CMSTerritory, ...
        // Module Name: modUtilities
        // Description:
        // Provide some utility function for all classes.
        // This module needs to reference to
        // Microsoft ActiveX Data Objects 2.1 Library
        // Pivotal RDA.dll
        // 
        // History:
        // Revision#    Date        Author  Description
        // ----------    ----        ------  -----------
        // 1.0           07/09/1999  DY      Initial version
        // 1.0.1         04/11/2000  DY      Update comments
        // ==========================================================================================
        private const string mstrEMPTY_STRING = "";
        private const int mlngERR_SHARED_START_NUMBER = 0x80040000 + 13400;
        private const int mlngERR_PARAMETER_EXPECTED = mlngERR_SHARED_START_NUMBER + 2;
        // -----------------------------------------------------------------------------------------------------------------
        // Name:    CopyFieldValue
        // Purpose: Helper sub to copy field value from one recordset to another
        // ------------------------------------------------------------------------------------------
        // Inputs:
        // rstSource - source recordset
        // rstTarget - target recordset
        // strFieldName - name of field to be copied
        // Outputs:
        // History:
        // Revision#    Date        Author  Description
        // ---------    ----        ------  -----------
        // 1.0          08/20/2004  RY      Initial version
        // ------------------------------------------------------------------------------------------
        public static void CopyFieldValue(ADODB.Recordset rstSource, ADODB.Recordset rstTarget, string strFieldName) 
        {

            // WARNING: On Error Resume Next is not supported
            if (!(Convert.IsDBNull(rstSource.Fields[strFieldName].Value)))
            {
                rstTarget.Fields[strFieldName].Value = rstSource.Fields[strFieldName].Value;
            }

        }

        // -----------------------------------------------------------------------------------------------------------------
        // Name:    IsRecordsetDirty
        // Purpose: Public function to check if a specified recordset is dirty, or changed.
        // ------------------------------------------------------------------------------------------
        // Inputs:
        // rstRecordset - Recordset to be checked.
        // Outputs:
        // Returns:
        // IsRecordsetDirty - True for dirty, False for not dirty
        // History:
        // Revision#    Date        Author  Description
        // ---------    ----        ------  -----------
        // 1.0          07/09/1999  DY      Initial version
        // ------------------------------------------------------------------------------------------
        public static bool IsRecordsetDirty(ADODB.Recordset rstRecordset) 
        {
            bool IsRecordsetDirty = false;

            // WARNING: On Error Resume Next is not supported

            // Set default value as not dirty
            IsRecordsetDirty = false;

            // Do not check the empty recordset
            if (rstRecordset.EOF)
            {
                return IsRecordsetDirty;
            }
            rstRecordset.MoveFirst();
            while(!(rstRecordset.EOF))
            {
                if (rstRecordset.Status == ADODB.RecordStatusEnum.adRecModified)
                {
                    IsRecordsetDirty = true;
                    return IsRecordsetDirty;
                }
                rstRecordset.MoveNext();
            } 

            return IsRecordsetDirty;
        }

        // -----------------------------------------------------------------------------------------------------------------
        // IsRecordDirty
        // Purpose:  Public function to check if a record in a specified recordset is dirty, or
        // changed.
        // ------------------------------------------------------------------------------------------
        // Called by:  Utility Function
        // Inputs:
        // rstRecordset - Recordset holds the record to be checked.
        // Outputs:
        // Returns:
        // IsRecordDirty - True for dirty, False for not dirty
        // History
        // Revision#    Date        Author  Description
        // ---------    ----        ------  -----------
        // 1.0          07/09/1999  DY      Initial version
        // ------------------------------------------------------------------------------------------
        public static bool IsRecordDirty(ADODB.Recordset rstRecordset) 
        {
            bool IsRecordDirty = false;
            // WARNING: On Error Resume Next is not supported

            // Set default value as not dirty
            IsRecordDirty = false;

            // Do not check the empty recordset
            if (rstRecordset.EOF)
            {
                return IsRecordDirty;
            }
            IsRecordDirty = (rstRecordset.Status == ADODB.RecordStatusEnum.adRecModified);

            return IsRecordDirty;
        }

        // -----------------------------------------------------------------------------------------------------------------
        // IsFieldDirty
        // Purpose:    Public function to check if a specified field changed
        // ------------------------------------------------------------------------------------------
        // Called by:  Utility function
        // Inputs:
        // fldField - Hold the reference for a specified field
        // Returns:
        // IsFieldDirty - True for changed, False for not changed
        // History:
        // Revision#    Date        Author  Description
        // ---------    ----        ------  -----------
        // 1.0          07/09/1999  DY      Initial version
        // 08/13/1999  DY      Null Check. Because VB can not compare two variables
        // when one or both are Null. In this case the compared
        // result is Null
        // ------------------------------------------------------------------------------------------
        public static bool IsFieldDirty(ADODB.Field fldField) 
        {
            bool IsFieldDirty = false;
            object vntValue = null;
            object vntOriginalValue = null;

            // If any error occurs, return the default value
            // WARNING: On Error GOTO IsFieldDirty_Err is not supported
            try 
            {

                IsFieldDirty = false;
                vntValue = fldField.Value;
                if (Convert.IsDBNull(fldField.Value) == true)
                {
                    vntValue = mstrEMPTY_STRING;
                }
                vntOriginalValue = fldField.OriginalValue;
                if (Convert.IsDBNull(fldField.OriginalValue) == true)
                {
                    vntOriginalValue = mstrEMPTY_STRING;
                }
                if (vntValue != vntOriginalValue)
                {
                    IsFieldDirty = true;
                }
                return IsFieldDirty;

                // WARNING: IsFieldDirty_Err: is not supported 
            }
            catch(Exception exc)
            {
            }
            return IsFieldDirty;
        }

        // -----------------------------------------------------------------------------------------------------------------
        // Name:     CompareValues
        // Purpose:  Compare two values to see if they are equal
        // ------------------------------------------------------------------------------------------
        // Inputs:
        // vntValue1 - One of the values to be compared
        // vntValue2 - One of the values to be compared
        // Outputs:
        // Returns:
        // CompareValues ; True - Equal, False - not equal
        // Revision#    Date        Author  Description
        // ---------    ----        ------  -----------
        // 1.0          08/13/1999  DY      Initial version
        // ------------------------------------------------------------------------------------------
        public static bool CompareValues(ref object vntValue1, ref object vntValue2) 
        {
            // WARNING: On Error Resume Next is not supported

            if (Convert.IsDBNull(vntValue1))
            {
                vntValue1 = "" /* EMPTY */ ;
            }
            if (Convert.IsDBNull(vntValue2))
            {
                vntValue2 = "" /* EMPTY */ ;
            }

            return (vntValue1 == vntValue2);
        }

        // -----------------------------------------------------------------------------------------------------------------
        // Name :    CompareValues
        // Purpose:  Compare two values to see if they are equal
        // if two Ids are null, return true
        // if only one Id is null, return false
        // if one or both Ids do not exist, return false
        // ------------------------------------------------------------------------------------------
        // Inputs:
        // vntValue1 - One of the values to be compared
        // vntValue2 - One of the values to be compared
        // Outputs:
        // Returns:
        // CompareValues ; True - Equal, False - not equal
        // History:
        // Revision#    Date        Author  Description
        // ---------    ----        ------  -----------
        // 1.0          08/13/1999  DY      Initial version
        // ------------------------------------------------------------------------------------------
        public static bool CompareIds(object vntId1, object vntId2, RDALib.RSystem objrSystem) 
        {
            bool CompareIds = false;
            // WARNING: On Error Resume Next is not supported

            if (Convert.IsDBNull(vntId1))
            {
                if (Convert.IsDBNull(vntId2))
                {
                    CompareIds = true;
                }
                else
                {
                    CompareIds = false;
                }
            }
            else
            {
                if (Convert.IsDBNull(vntId2))
                {
                    CompareIds = false;
                }
                else
                {
                    CompareIds = objrSystem.IdToString(vntId1) == objrSystem.IdToString(vntId2);
                    if (Information.Err().Number == 5)
                    {
                        CompareIds = false;
                    }
                }
            }

            return CompareIds;
        }

        // -----------------------------------------------------------------------------------------------------------------
        // Name:     Sum
        // Purpose:  Calculate the sum value of a specified filed in the specified recordset
        // Note: This function only sum the defined record (not Null)
        // ------------------------------------------------------------------------------------------
        // Inputs:
        // rstRecordset - Recordset to be used for the calculation
        // strField     - Field which value will be calculated
        // Outputs:
        // Returns:
        // Sum - Calculated value
        // History:
        // Revision#    Date        Author  Description
        // ---------    ----        ------  -----------
        // 1.0          07/27/1999  DY      Initial version
        // ------------------------------------------------------------------------------------------
        public static object Sum(ADODB.Recordset rstRecordset, string strField) 
        {
            object Sum = null;
            // WARNING: On Error GOTO Sum_Err is not supported
            try 
            {

                Sum = 0;
                if (!(rstRecordset.EOF))
                {
                    rstRecordset.MoveFirst();
                }
                while(!(rstRecordset.EOF))
                {
                    if (Convert.IsDBNull(rstRecordset.Fields[strField].Value) == false)
                    {
                        Sum = Convert.ToDouble(Sum) + Convert.ToDouble(rstRecordset.Fields[strField].Value);
                    }
                    rstRecordset.MoveNext();
                } 
                return Sum;

                // WARNING: Sum_Err: is not supported 
            }
            catch(Exception exc)
            {
                Information.Err().Raise(Information.Err().Number, null, Information.Err().Description, null, null);
            }
            return Sum;
        }

        // -----------------------------------------------------------------------------------------------------------------
        // Name:     GetArrayDim
        // Purpose:  Get the dimension of an given array
        // ------------------------------------------------------------------------------------------
        // Inputs:
        // vntArray - Array to be checked
        // Outputs:
        // Returns:
        // GetArrayDim - number of dimensions of the array
        // History:
        // Revision#    Date        Author  Description
        // ---------    ----        ------  -----------
        // 1.0          15/11/1999  CT      Initial version
        // ------------------------------------------------------------------------------------------
        public static short GetArrayDim(object vntArray) 
        {
            int N = 0;
            short k = 0;

            do
            {
                k = (short)(k + 1);
                // WARNING: On Error Resume Next is not supported
                N = Information.UBound(vntArray, k);
                if (Information.Err().Number == 9)
                {
                    k = (short)(k - 1);
                    return k;
                }
            } while (true);

            return k;
        }

        // -----------------------------------------------------------------------------------------------------------------
        // Name:     IsRecordsetEmpty
        // Purpose:  Determin if i a recordset is empty
        // ------------------------------------------------------------------------------------------
        // Inputs:
        // rstRecordset - Recordset
        // Outputs:
        // Returns:
        // IsRecordsetEmpty - True for empty,  False for not empty, with at least one record
        // History:
        // Revision#    Date        Author  Description
        // ---------    ----        ------  -----------
        // 1.0          11/26/1999  DY      Initial version
        // ------------------------------------------------------------------------------------------
        public static bool IsRecordsetEmpty(ADODB.Recordset rstRecordset) 
        {
            bool IsRecordsetEmpty = false;

            // WARNING: On Error GOTO IsRecordsetEmpty_Err is not supported
            try 
            {

                IsRecordsetEmpty = true;
                if (Convert.IsDBNull(rstRecordset.Fields) || rstRecordset == null)
                {
                    IsRecordsetEmpty = true;
                }
                else if( rstRecordset.RecordCount > 0)
                {
                    IsRecordsetEmpty = false;
                }
                else
                {
                    IsRecordsetEmpty = true;
                }
                return IsRecordsetEmpty;

                // WARNING: IsRecordsetEmpty_Err: is not supported 
            }
            catch(Exception exc)
            {
            }
            return IsRecordsetEmpty;
        }

        // ------------------------------------------------------------------------------------------
        // Name:     CheckParameterNum
        // Purpose:  This function check if any parameter is missing in the passed parameter array.
        // ------------------------------------------------------------------------------------------
        // Inputs:
        // intRequired   - Required parameter number
        // vntParameters - Parameter array only containing user defined parameters
        // Outputs:
        // Returns:
        // History:
        // Revision#     Date        Author  Description
        // ----------    ----        ------  -----------
        // 1.0           11/21/1999  DY      Initial version
        // 1.0.1         04/11/2000  DY      Update comments
        // 1.0.2         04/25/2000  DY      Move it from classes to utility for share use
        // ------------------------------------------------------------------------------------------
        public static void CheckParameterNum(short intRequired, ref object vntParameters) 
        {
            short intPassed = 0;

            if (Information.IsNothing(vntParameters))
            {
                intPassed = (short)0;
                // WARNING: Goto CheckParameterNum_Exit is not supported
            }
            if (Information.IsArray(vntParameters) == false)
            {
                vntParameters = new object[] {vntParameters};
            }
            intPassed = (short)(Information.UBound(vntParameters, 1) + 1);

            // WARNING: CheckParameterNum_Exit: is not supported 
            if (intPassed < intRequired)
            {
                Information.Err().Raise(mlngERR_PARAMETER_EXPECTED, null, Convert.ToString(intRequired) + " parameter(s) were expected.  " + "However, you only passed " + Convert.ToString(intPassed) + " parameter(s).", null, null);
            }
            return ;

            // WARNING: CheckParameterNum_Err: is not supported 
            Information.Err().Raise(Information.Err().Number, Information.Err().Source, Information.Err().Description, null, null);

        }

        // ------------------------------------------------------------
        // Purpose:  Return a trimmed string from a variant that may be Null
        // Inputs:   vntVal is the variant to convert
        // Passed:   None
        // Returns:  A valid string
        // Outputs:  None
        // Notes:    Useful in database operations or validating arguments
        // Usage:    strName = VntToStr(vntName)
        // History:
        // Revision#     Date        Author  Description
        // ----------    ----        ------  -----------
        // 1.0           11/07/2000  JC      Initial version
        // ------------------------------------------------------------
        public static string VntToStr(object vntVar) 
        {
            string VntToStr = String.Empty;

            // WARNING: On Error GOTO ErrTrap is not supported
            try 
            {
                // Initialize return value
                VntToStr = "";

                if (Convert.IsDBNull(vntVar))
                {
                    return VntToStr;
                }
                VntToStr = Convert.ToString(vntVar).Trim();
                return VntToStr;

                // WARNING: ErrTrap: is not supported 
            }
            catch(Exception exc)
            {
                Information.Err().Raise(Information.Err().Number, Information.Err().Source, Information.Err().Description, null, null);
            }
            return VntToStr;
        }

        // ------------------------------------------------------------
        // Purpose:  Return a long from a variant
        // Inputs:   vntVar is the variant to be converted
        // Passed:   None
        // Returns:  A valid long
        // Outputs:  None
        // Notes:    Returns zero if the variant is invalid
        // Usage:    lngRevenue = VntToLng(vntRevenue)
        // History:
        // Revision#     Date        Author  Description
        // ----------    ----        ------  -----------
        // 1.0           11/07/2000  JC      Initial version
        // ------------------------------------------------------------
        public static int VntToLng(object vntVar) 
        {
            int VntToLng = 0;

            // WARNING: On Error GOTO ErrTrap is not supported
            try 
            {

                // Initialize return value
                VntToLng = 0;

                // Create Long
                if (!(Convert.IsDBNull(vntVar)))
                {
                    if (Information.IsNumeric(vntVar))
                    {
                        VntToLng = Convert.ToInt32(vntVar);
                    }
                }
                return VntToLng;

                // WARNING: ErrTrap: is not supported 
            }
            catch(Exception exc)
            {
                Information.Err().Raise(Information.Err().Number, Information.Err().Source, Information.Err().Description, null, null);
            }
            return VntToLng;
        }

        // ------------------------------------------------------------
        // Name:     VntToBool
        // Purpose:  Return a boolean value form a variant that may be Null
        // Inputs:
        // vntVal  - The variant to convert
        // Returns:
        // VntToBol - A valid boolean value
        // Outputs:
        // None
        // Notes:    Useful in database operations or validating arguments
        // Usage:    blnChecked = VntToStr(vntChecked)
        // History:
        // Revision#     Date        Author  Description
        // ----------    ----        ------  -----------
        // 1.0           05/17/2001  DY      Initial version
        // ------------------------------------------------------------
        public static bool VntToBool(object vntVar) 
        {
            bool VntToBool = false;

            // WARNING: On Error GOTO VntToBool_Err is not supported
            try 
            {

                // Initialize return value
                VntToBool = false;

                if (Convert.IsDBNull(vntVar))
                {
                    return VntToBool;
                }
                VntToBool = Convert.ToBoolean(vntVar);
                return VntToBool;

                // WARNING: VntToBool_Err: is not supported 
            }
            catch(Exception exc)
            {
                Information.Err().Raise(Information.Err().Number, Information.Err().Source, Information.Err().Description, null, null);
            }
            return VntToBool;
        }

        // ------------------------------------------------------------
        // Name:     VntToDbl
        // Purpose:  Return a double value form a variant that may be Null
        // Inputs:
        // vntVal  - The variant to convert
        // Returns:
        // VntToBol - A valid double value
        // Outputs:
        // None
        // Notes:    Useful in database operations or validating arguments
        // Usage:    dblChecked = VntToDbl(vntChecked)
        // History:
        // Revision#     Date        Author  Description
        // ----------    ----        ------  -----------
        // 1.0           05/22/2001  JC      Initial version
        // ------------------------------------------------------------
        public static double VntToDbl(object vntVar) 
        {
            double VntToDbl = 0;

            // WARNING: On Error GOTO ErrTrap is not supported
            try 
            {

                // Initialize return value
                VntToDbl = 0D;

                if (Convert.IsDBNull(vntVar))
                {
                    return VntToDbl;
                }
                VntToDbl = Convert.ToDouble(vntVar);
                return VntToDbl;

                // WARNING: ErrTrap: is not supported 
            }
            catch(Exception exc)
            {
                Information.Err().Raise(Information.Err().Number, Information.Err().Source, Information.Err().Description, null, null);
            }
            return VntToDbl;
        }


    }

}
