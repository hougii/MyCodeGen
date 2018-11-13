using CodeGen.Web.Models;
using DotLiquid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGen.Web.Utility
{
    public class NgGenerator
    {
        /// <summary>
        /// (TW)產生Angular的程式碼
        /// (EN)generate Angualar source code
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="contentRootPath"></param>
        /// <returns></returns>
        public static dynamic GenerateNgController(TableInfo table, List<ColumnInfo> columns, string contentRootPath)
        {
            var result = "";
            var liquidPath = contentRootPath + "\\template\\AngularJS\\ngController.liquid";

            //(TW)使用Liquid的框架產生程式碼，傳入參數：table , columns
            var templateContent = File.ReadAllText(liquidPath, Encoding.UTF8);
            Template template = Template.Parse(templateContent);
            var tableForLiquid = new TableInfoForLiquid(table);
            var columnsForLiquid = columns.Select(m => new ColumnInfoForLiquid(m));
            result = template.Render(Hash.FromAnonymousObject(
                new { table = tableForLiquid, columns = columnsForLiquid }));

            return result;
#if false //origin code
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            StringBuilder builderPrm = new StringBuilder();
            StringBuilder builderSub = new StringBuilder();
            builderPrm.Clear(); builderSub.Clear();
            string fileContent = string.Empty; string queryPrm = string.Empty; string submitPrm = string.Empty;

            string tableName =table.TableName;
            string tableDescription = table.TableDescription;
            string path = @"" + contentRootPath + "\\template\\AngularJS\\Controller.txt";

            //Controller Name
            string ctrlName = textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString())) + "Controller";
            string serviceInjected = "'$scope', '$http'"; string srvParam = "$scope, $http";
            string urlApiGet = "'/api/" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString())) + "/GetAll'";
            string url_GetByID = "'/api/" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString())) + "/GetByID/'+ parseInt(model.id)";
            string url_Post = "'/api/" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString())) + "/Save'";
            string url_Put = "'/api/" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString())) + "/UpdateByID/'+ parseInt(model.id)";
            string url_Delete = "'/api/" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString())) + "/DeleteByID/'+ parseInt(model.id)";

            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                fileContent = sr.ReadToEnd()
                    .Replace("#ControllerName", ctrlName.ToString())
                    .Replace("#ServiceInjected", serviceInjected.ToString())
                    .Replace("#SrvParam", srvParam.ToString())
                    .Replace("#UrlGet", urlApiGet.ToString())
                    .Replace("#Url_GetByID", url_GetByID.ToString())
                    .Replace("#Url_Post", url_Post.ToString())
                    .Replace("#Url_Put", url_Put.ToString())
                    .Replace("#Url_Delete", url_Delete.ToString());
            }

            return fileContent.ToString();
#endif
        }
    }
}
