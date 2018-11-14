using DotLiquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGen.Web.Models
{
    /// <summary>
    /// (TW)其它資訊
    /// </summary>
    public class OtherInfoForLiquid : ILiquidizable
    {
        /// <summary>
        /// (TW)建構子
        /// </summary>
        public OtherInfoForLiquid()
        {
        }

        /// <summary>
        /// (TW)資料表下的Key值欄位
        /// </summary>
        public string IndentityColumn { get; set; }

        /// <summary>
        /// (TW)資料表下的Key值欄位類別
        /// </summary>
        public string IndentityModelType { get; set; }

        /// <summary>
        /// (TW)資料表下的Key值欄位描述
        /// </summary>
        public string IndentityColumnDescription { get; set; }



        /// <summary>
        /// (TW)實作於Liquid下可用的參數
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            //定義Liquid使用的參數值
            return new
            {
                IndentityColumn,
                IndentityModelType,
                IndentityColumnDescription
            };
        }
    }
}
