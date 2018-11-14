using Microsoft.AspNetCore.Hosting;
using CodeGen.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;

namespace CodeGen.Web.Utility
{
    public abstract class BaseGenerator
    {
        /// <summary>
        /// (TW)Liquid 檔案的相對位置
        /// </summary>
        public abstract string LiquidPath { get; }

        /// <summary>
        /// (TW)產生程式碼
        /// (EN)generate Source code
        /// </summary>
        /// <param name="tableInfoForLiquid"></param>
        /// <param name="columnsForLiquid"></param>
        /// <param name="contentRootPath"></param>
        /// <returns></returns>
        public dynamic Generate(TableInfoForLiquid tableInfoForLiquid, List<ColumnInfoForLiquid> columnsForLiquid, OtherInfoForLiquid otherInfoForLiquid, string contentRootPath)
        {

            var result = "";
            var liquidPath = contentRootPath +LiquidPath;

            //(TW)使用Liquid的框架產生程式碼，傳入參數：table , columns
            var templateContent = File.ReadAllText(liquidPath, Encoding.UTF8);
            Template template = Template.Parse(templateContent);
            result = template.Render(Hash.FromAnonymousObject(
                new { table = tableInfoForLiquid, columns = columnsForLiquid }));

            return result;
        }

    }
}
