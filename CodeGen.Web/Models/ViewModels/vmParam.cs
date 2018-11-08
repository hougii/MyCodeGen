using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGen.Web.Models
{
    public class vmParam
    {
        /// <summary>
        /// 資料庫
        /// </summary>
        public int DatabaseId { get; set; }
        /// <summary>
        /// 資料庫名稱
        /// </summary>
        public string DatabaseName { get; set; }
        /// <summary>
        /// Table Id
        /// </summary>
        public int TableId { get; set; }
        /// <summary>
        /// Table名稱
        /// </summary>
        public string TableName { get; set; }
    }
}
