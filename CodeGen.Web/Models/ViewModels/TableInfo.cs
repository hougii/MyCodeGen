﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGen.Web.Models
{
    /// <summary>
    /// Table Infomation
    /// </summary>
    public class TableInfo
    {
        /// <summary>
        /// Table no
        /// </summary>
        public int TableId { get; set; }
        /// <summary>
        /// Table Schema
        /// </summary>
        public string TableSchema { get; set; }
        /// <summary>
        /// Table Name
        /// </summary>
        public string TableName { get; set; }

        public string MapTableName { get; set; }
        /// <summary>
        /// Table Description
        /// </summary>
        public string TableDescription { get; set; }
    }
}
