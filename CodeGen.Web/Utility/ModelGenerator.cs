using CodeGen.Web.Models;
using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGen.Web.Utility
{
    public class ModelGenerator
    {

        /// <summary>
        /// (TW)產生DbModel的程式碼
        /// (EN)generate Database Class Model 
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="contentRootPath"></param>
        /// <returns></returns>
        public static dynamic GenerateModel(TableInfo table, List<ColumnInfo> columns, string contentRootPath)
        {
            var result = "";
            var liquidPath = contentRootPath + "\\template\\Model\\Model.liquid";

            //(TW)使用Liquid的框架產生程式碼
            //    傳入參數：table , columns
            var templateContent = File.ReadAllText(liquidPath, Encoding.UTF8);
            Template template = Template.Parse(templateContent);
            var tableForLiquid = new TableInfoForLiquid(table);
            var columnsForLiquid = columns.Select(m => new ColumnInfoForLiquid(m));
            result = template.Render(Hash.FromAnonymousObject(new { table = tableForLiquid, columns = columnsForLiquid }));

            return result;
        }
    }
}
