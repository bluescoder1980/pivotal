using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Pivotal.Interop.ADODBLib;

namespace CRM.Pivotal
{
    class Utils
    {
        static DataTypeEnum TranslateType(Type columnType)
        {
            switch (columnType.UnderlyingSystemType.ToString())
            {
                case "System.Boolean":
                    return DataTypeEnum.adBoolean;

                case "System.Byte":
                    return DataTypeEnum.adUnsignedTinyInt;

                case "System.Char":
                    return DataTypeEnum.adChar;

                case "System.DateTime":
                    return DataTypeEnum.adDate;

                case "System.Decimal":
                    return DataTypeEnum.adCurrency;

                case "System.Double":
                    return DataTypeEnum.adDouble;

                case "System.Int16":
                    return DataTypeEnum.adSmallInt;

                case "System.Int32":
                    return DataTypeEnum.adInteger;

                case "System.Int64":
                    return DataTypeEnum.adBigInt;

                case "System.SByte":
                    return DataTypeEnum.adTinyInt;

                case "System.Single":
                    return DataTypeEnum.adSingle;

                case "System.UInt16":
                    return DataTypeEnum.adUnsignedSmallInt;

                case "System.UInt32":
                    return DataTypeEnum.adUnsignedInt;

                case "System.UInt64":
                    return DataTypeEnum.adUnsignedBigInt;

                case "System.Byte[]":
                    return DataTypeEnum.adBinary;

                default:
                    return DataTypeEnum.adVarChar;
            }
        }

        static public Recordset ConvertToRecordset(DataTable inTable)
        {
            //rstRecordset = CRM.Pivotal.Utils.ConvertToRecordset(oDataset.Tables[0])
            Recordset result = new Recordset();
            result.CursorLocation = CursorLocationEnum.adUseClient;

            Fields resultFields = result.Fields;
            System.Data.DataColumnCollection inColumns = inTable.Columns;

            foreach (DataColumn inColumn in inColumns)
            {
                resultFields.Append(inColumn.ColumnName
                    , TranslateType(inColumn.DataType)
                    , inColumn.MaxLength
                    , inColumn.AllowDBNull ? FieldAttributeEnum.adFldIsNullable :
                                             FieldAttributeEnum.adFldUnspecified);
            }

            result.Open(System.Reflection.Missing.Value
                    , System.Reflection.Missing.Value
                    , CursorTypeEnum.adOpenStatic
                    , LockTypeEnum.adLockOptimistic, 0);

            foreach (DataRow dr in inTable.Rows)
            {
                result.AddNew(System.Reflection.Missing.Value,
                              System.Reflection.Missing.Value);

                for (int columnIndex = 0; columnIndex < inColumns.Count; columnIndex++)
                {
                    resultFields[columnIndex].Value = dr[columnIndex];
                }
            }

            return result;
        }
    }
}
