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
            base.ColumnDescription = column.ColumnDescription;
            base.DataType = column.DataType;
            base.MaxLength = column.MaxLength;
        }

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
                ColumnDescription,
                DataType,
                MaxLength,
            };
        }
    }
}
