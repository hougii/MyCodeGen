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
    public class APIGenerator
    {
        /// <summary>
        /// (TW)產生API的程式碼
        /// (EN)generate API source code
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="contentRootPath"></param>
        /// <returns></returns>
        public static dynamic GenerateAPIGet(TableInfo table, List<ColumnInfo> columns, string contentRootPath)
        {
            var result = "";
            var liquidPath = contentRootPath + "\\template\\WebAPI\\APIController.liquid";

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

            string tableName = table.TableName;
            string tableDescription = table.TableDescription;
            string path = @"" + contentRootPath + "\\template\\WebAPI\\APIController.txt";

            //Api Controller
            string routePrefix = "api/" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString()));
            string apiController = textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString())) + "Controller";
            string collectionName = "List<" + tableName.ToString() + ">";
            string listObj = tableName.ToString() + "s";
            string getDbMethod = "_ctx." + tableName.ToString() + ".ToListAsync()";
            string entity = tableName.ToString();
            string urlApiGet = "api/" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString())) + "/GetAll";
            string urlApiGetByID = "api/" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString())) + "/GetByID/5";
            string urlApiPost = "api/" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString())) + "/Save";
            string urlApiPut = "api/" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString())) + "/UpdateByID/5";
            string urlApiDeleteByID = "api/" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(tableName.ToString())) + "/DeleteByID/5";

            //Enity Fields
            foreach (var item in columns)
            {
                //parameter
                builderPrm.AppendLine();
                builderPrm.Append("                        entityUpdate." + item.ColumnName + " = model." + item.ColumnName + ";");
            }
            submitPrm = builderPrm.AppendLine().ToString();

            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                fileContent = sr.ReadToEnd()
                    .Replace("#RoutePrefix", routePrefix.ToString())
                    .Replace("#APiController", apiController.ToString())
                    .Replace("#Collection", collectionName.ToString())
                    .Replace("#ListObj", listObj.ToString())
                    .Replace("#DbMethod", getDbMethod.ToString())
                    .Replace("#Entity", entity.ToString())
                    .Replace("#UrlApiGet", urlApiGet.ToString())
                    .Replace("#UrlGetByID", urlApiGetByID.ToString())
                    .Replace("#UrlPostByID", urlApiPost.ToString())
                    .Replace("#UrlApiPut", urlApiPut.ToString())
                    .Replace("#ColUpdate", submitPrm.ToString())
                    .Replace("#UrlDeleteByID", urlApiDeleteByID.ToString());
            }

            return fileContent.ToString();
#endif
        }
    }
}
