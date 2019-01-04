using DotLiquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGen.Web.Models
{
    /// <summary>
    /// (TW)為了達成於Liquid下可以直接使用的Model，而不動到原Model架構
    /// </summary>
    public class ColumnInfoForLiquid : ColumnInfo, ILiquidizable
    {
        /// <summary>
        /// (TW)建構子
        /// </summary>
        /// <param name="table"></param>
        public ColumnInfoForLiquid(ColumnInfo column)
        {
            base.ColumnId = column.ColumnId;
            base.ColumnName = column.ColumnName;
            base.MapColumnName = column.MapColumnName;
            base.ColumnDescription = column.ColumnDescription;
            base.DataType = column.DataType;
            base.IsNullable = column.IsNullable;
            base.MaxLength = column.MaxLength;
            ModelType = convertDataTypeToModelType(base.DataType, base.IsNullable);
        }

        /// <summary>
        /// (TW)轉換後的物件型別
        /// </summary>
        public string ModelType { get; set; }


        /// <summary>
        /// (TW)實作於Liquid下可用的參數
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            //定義Liquid使用的參數值
            return new
            {
                ColumnId,
                ColumnName,
                MapColumnName,
                ColumnDescription,
                DataType,
                ModelType,
                MaxLength,
            };
        }


        /// <summary>
        /// (TW)將資料庫的型別轉換為物件型別
        /// </summary>
        /// <remarks>(TW)因僅於Column時會應用到，暫先此Function之放於此Model之下</remarks>
        /// <param name="dataType">(TW)資料庫型別</param>
        /// <returns></returns>
        private string convertDataTypeToModelType(string dataType, bool isNullable)
        {
            var result = "";
            //(TW)轉換為小寫
            dataType = dataType.ToLower();
            switch (dataType)
            {
                case "bit":
                    result = (isNullable) ? "bool?" : "bool";
                    break;
                case "bigint":
                    result = (isNullable) ? "long?" : "long";
                    break;
                case string s when (s.Contains("int")):
                    result = (isNullable) ? "int?":"int";
                    break;
                case "decimal":
                case "money":
                case "float":
                    result = (isNullable) ? "decimal?":"decimal";
                    break;

                case string s when (s.Contains("date")):
                    result = (isNullable) ? "DateTime?":"DateTime";
                    break;
                default:
                    result = "string";
                    break;
            }
            return result;
        }
    }
}
