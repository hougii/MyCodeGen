using DotLiquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGen.Web.Models
{
    /// <summary>
    /// (TW)Table資訊 For Liquid
    /// </summary>
    /// <remarks>(TW)為了達成於Liquid下可以直接使用的Model，而不動到原Model架構</remarks>
    public class TableInfoForLiquid : TableInfo, ILiquidizable
    {
        /// <summary>
        /// (TW)建構子
        /// </summary>
        /// <param name="table"></param>
        public TableInfoForLiquid(TableInfo table)
        {
            base.TableId = table.TableId;
            base.TableName = table.TableName;
            base.MapTableName = table.MapTableName;
            base.TableDescription = table.TableDescription;
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
                TableId,
                TableName,
                MapTableName,
                TableDescription
            };
        }
    }
}
