using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGen.Web.Models
{
    /// <summary>
    /// 資料表的欄位資訊
    /// </summary>
    public class ColumnInfo
    {
        public int? ColumnId { get; set; }
        /// <summary>
        /// (TW)欄位名稱
        /// (EN)Column Name
        /// </summary>
        public string ColumnName { get; set; }
        public string MapColumnName { get; set; }
        public string DataType { get; set; }
        public int MaxLength { get; set; }
        public bool IsNullable { get; set; }
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        /// <summary>
        /// (TW)欄位備註
        /// (EN)Column Description
        /// </summary>
        public string ColumnDescription { get; set; }
    }
}
