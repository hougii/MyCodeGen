using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGen.Web.Models
{
    /// <summary>
    /// 資料表的欄位資訊
    /// </summary>
    public class vmColumn
    {
        public int? ColumnId { get; set; }
        /// <summary>
        /// 欄位名稱
        /// </summary>
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string MaxLength { get; set; }
        public string IsNullable { get; set; }
        public string TableSchema { get; set; }
        public string Tablename { get; set; }
        /// <summary>
        /// 欄位備註
        /// </summary>
        public string ColumnDescription { get; set; }
    }
}
