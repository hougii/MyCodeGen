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
    public class FormGenerator
    {
        /// <summary>
        /// (TW)產生View的程式碼
        /// (EN)generate View Data 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="contentRootPath"></param>
        /// <returns></returns>
        public static dynamic GenerateForm(TableInfo table, List<ColumnInfo> columns, string contentRootPath)
        {

            var result = "";
            var liquidPath = contentRootPath + "\\template\\HtmlForm\\HtmlForm.liquid";

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
            string path = @"" + contentRootPath + "\\template\\HtmlForm\\Form.txt";

            //Form Name
            string frmName = "name='frm" + tableName.ToString() + "' novalidate";

            //Form Fields
            foreach (var item in columns)
            {
                //parameter
                builderPrm.AppendLine();
                builderPrm.Append(" <div class='form-group'>");
                builderPrm.AppendLine();
                if (item.ColumnName.Contains("email") || item.ColumnName.Contains("Email"))
                {
                    builderPrm.Append("  <label for='" + item.ColumnName + "' class='control-label'>" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(item.ColumnName)) + "</label>");
                    builderPrm.AppendLine();
                    builderPrm.Append("  <input type='email' class='form-control' ng-model='vmfrm." + item.ColumnName + "' name='" + item.ColumnName + "' required />");
                }
                else if (item.ColumnName.Contains("password") || item.ColumnName.Contains("Password"))
                {
                    builderPrm.Append("  <label for='" + item.ColumnName + "' class='control-label'>" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(item.ColumnName)) + "</label>");
                    builderPrm.AppendLine();
                    builderPrm.Append("  <input type='password' class='form-control' ng-model='vmfrm." + item.ColumnName + "' name='" + item.ColumnName + "' required />");
                }
                else
                {
                    builderPrm.Append("  <label for='" + item.ColumnName + "' class='control-label'>" + textInfo.ToTitleCase(Conversion.RemoveSpecialCharacters(item.ColumnName)) + "</label>");
                    builderPrm.AppendLine();
                    builderPrm.Append("  <input type='text' class='form-control' ng-model='vmfrm." + item.ColumnName + "' name='" + item.ColumnName + "' required />");
                }
                builderPrm.AppendLine();
                builderPrm.Append(" </div>");
            }
            queryPrm = builderPrm.AppendLine().ToString();

            //Form Submit
            builderSub.Append(" <div class='form-group'>");
            builderSub.AppendLine();
            builderSub.Append("  <input type='submit' name='reset' value='Reset' ng-click='Reset()' />");
            builderSub.AppendLine();
            builderSub.Append("  <input type='submit' name='update' value='Update' ng-click='Update()' />");
            builderSub.AppendLine();
            builderSub.Append("  <input type='submit' name='submit' value='Save' ng-click='Save()' />");
            builderSub.AppendLine();
            builderSub.Append(" </div>");

            submitPrm = builderSub.AppendLine().ToString();

            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                fileContent = sr.ReadToEnd().Replace("#frmName", frmName.ToString()).Replace("#frmGroup", queryPrm.ToString()).Replace("#frmSubmit", submitPrm.ToString());
            }

            return fileContent.ToString();
#endif
        }
    }
}
