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
    public class SpGenerator
    {
        /// <summary>
        /// (TW)產生SP的Create的程式碼
        /// (EN)generate SP Create source code
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="contentRootPath"></param>
        /// <returns></returns>
        public static dynamic GenerateSP(TableInfo table, List<ColumnInfo> columns, string contentRootPath)
        {

            var result = "";
            var liquidPath = contentRootPath + "\\template\\StoredProcedure\\SP.liquid";

            //(TW)使用Liquid的框架產生程式碼，傳入參數：table , columns
            var templateContent = File.ReadAllText(liquidPath, Encoding.UTF8);
            Template template = Template.Parse(templateContent);
            var tableForLiquid = new TableInfoForLiquid(table);
            var columnsForLiquid = columns.Select(m => new ColumnInfoForLiquid(m));
            result = template.Render(Hash.FromAnonymousObject(
                new { table = tableForLiquid, columns = columnsForLiquid }));

            return result;
        }

#if false //origin code
        /// <summary>
        /// (TW)產生SP的Create的程式碼
        /// (EN)generate SP Create source code
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="contentRootPath"></param>
        /// <returns></returns>
        public static dynamic GenerateSetSP(TableInfo table, List<ColumnInfo> columns, string contentRootPath)
        {
            StringBuilder builderPrm = new StringBuilder();
            StringBuilder builderBody = new StringBuilder();
            builderPrm.Clear(); builderBody.Clear();

            string path = @"" + contentRootPath + "\\template\\StoredProcedure\\InsertSP.txt";
            string fileContent = string.Empty; string fileld = string.Empty; string fileldPrm = string.Empty; string queryPrm = string.Empty;
            string tableName = table.TableName;
            string tableSchema = table.TableSchema;
            string tableDescription = table.TableDescription;

            string spName = ("[" + tableSchema + "].[Set_" + tableName + "]").ToString();
            foreach (var item in columns)
            {
                fileld = fileld + item.ColumnName + ",";
                fileldPrm = fileldPrm + "@" + item.ColumnName + ",";

                //parameter
                builderPrm.AppendLine();
                if ((item.DataType.ToString() == "nvarchar") || (item.DataType.ToString() == "varchar"))
                    builderPrm.Append("  @" + item.ColumnName + " " + item.DataType + "(" + item.MaxLength + "),");
                else
                    builderPrm.Append("  @" + item.ColumnName + " " + item.DataType + ",");
            }

            queryPrm = builderPrm.Remove((builderPrm.Length - 1), 1).AppendLine().ToString();
            //queryPrm = builderPrm.ToString().TrimEnd(',');

            //Body
            builderBody.Append("INSERT INTO [" + tableSchema + "].[" + tableName + "](");
            builderBody.Append(fileld.TrimEnd(',') + ") ");
            //builderBody.AppendLine();
            builderBody.Append("VALUES (" + fileldPrm.TrimEnd(',') + ")");

            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                fileContent = sr.ReadToEnd().Replace("#Name", spName.ToString()).Replace("#Param", queryPrm.ToString()).Replace("#Body", builderBody.ToString());
            }

            return fileContent.ToString();
        }

        /// <summary>
        /// (TW)產生SP的Read的程式碼
        /// (EN)generate SP Read source code
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="contentRootPath"></param>
        /// <returns></returns>
        public static dynamic GenerateGetSP(TableInfo table, List<ColumnInfo> columns, string contentRootPath)
        {
            StringBuilder builderPrm = new StringBuilder();
            StringBuilder builderBody = new StringBuilder();
            builderPrm.Clear(); builderBody.Clear();

            string path = @"" + contentRootPath + "\\template\\StoredProcedure\\ReadSP.txt";
            string fileContent = string.Empty; string fileld = string.Empty; string fileldPrm = string.Empty; string queryPrm = string.Empty;
            string tableName = table.TableName;
            string tableSchema = table.TableSchema;
            string tableDescription = table.TableDescription;

            string spName = ("[" + tableSchema + "].[Get_" + tableName + "]").ToString();
            foreach (var item in columns)
            {
                fileld = fileld + item.ColumnName + ",";
                fileldPrm = fileldPrm + "@" + item.ColumnName + ",";
            }


            //Body
            builderBody.Append("SELECT " + fileldPrm.TrimEnd(',') + " FROM [" + tableSchema + "].[" + tableName + "]");

            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                fileContent = sr.ReadToEnd().Replace("#Name", spName.ToString()).Replace("#Body", builderBody.ToString()).Replace("#OrdPrm", fileldPrm.TrimEnd(',').ToString());
            }

            return fileContent.ToString();
        }

        /// <summary>
        /// (TW)產生SP的Update的程式碼
        /// (EN)generate SP Update source code
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="contentRootPath"></param>
        /// <returns></returns>
        public static dynamic GeneratePutSP(TableInfo table, List<ColumnInfo> columns, string contentRootPath)
        {
            StringBuilder builderPrm = new StringBuilder();
            StringBuilder builderBody = new StringBuilder();
            builderPrm.Clear(); builderBody.Clear();

            string path = @"" + contentRootPath + "\\template\\StoredProcedure\\UpdateSP.txt";
            string fileContent = string.Empty; string fileld = string.Empty; string fileldPrm = string.Empty; string queryPrm = string.Empty;
            string tableName = table.TableName;
            string tableSchema = table.TableSchema;
            string tableDescription = table.TableDescription;

            string spName = ("[" + tableSchema + "].[Get_" + tableName + "]").ToString();
            foreach (var item in columns)
            {
                fileld = fileld + item.ColumnName + ",";
                fileldPrm = fileldPrm + item.ColumnName + " = @" + item.ColumnName + ",";

                //parameter
                builderPrm.AppendLine();
                if ((item.DataType.ToString() == "nvarchar") || (item.DataType.ToString() == "varchar"))
                    builderPrm.Append("  @" + item.ColumnName + " " + item.DataType + "(" + item.MaxLength + "),");
                else
                    builderPrm.Append("  @" + item.ColumnName + " " + item.DataType + ",");
            }

            queryPrm = builderPrm.Remove((builderPrm.Length - 1), 1).AppendLine().ToString();

            //Body
            builderBody.Append("UPDATE [" + tableSchema + "].[" + tableName + "] SET " + fileldPrm.TrimEnd(',') + " WHERE [CONDITIONS]");

            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                fileContent = sr.ReadToEnd().Replace("#Name", spName.ToString()).Replace("#Param", queryPrm.ToString()).Replace("#Body", builderBody.ToString()).Replace("#OrdPrm", fileldPrm.ToString());
            }

            return fileContent.ToString();
        }

        /// <summary>
        /// (TW)產生SP的Delete的程式碼
        /// (EN)generate SP Delete source code
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <param name="contentRootPath"></param>
        /// <returns></returns>
        public static dynamic GenerateDeleteSP(TableInfo table, List<ColumnInfo> columns, string contentRootPath)
        {
            StringBuilder builderPrm = new StringBuilder();
            StringBuilder builderBody = new StringBuilder();
            builderPrm.Clear(); builderBody.Clear();

            string path = @"" + contentRootPath + "\\template\\StoredProcedure\\DeleteSP.txt";
            string fileContent = string.Empty; string fileld = string.Empty; string fileldPrm = string.Empty; string queryPrm = string.Empty;
            string tableName = table.TableName;
            string tableSchema = table.TableSchema;
            string tableDescription = table.TableDescription;

            string spName = ("[" + tableSchema + "].[Delete_" + tableName + "]").ToString();
            foreach (var item in columns)
            {
                fileld = fileld + item.ColumnName + ",";
                fileldPrm = fileldPrm + "@" + item.ColumnName + ",";

                //parameter
                builderPrm.AppendLine();
                if ((item.DataType.ToString() == "nvarchar") || (item.DataType.ToString() == "varchar"))
                    builderPrm.Append("  @" + item.ColumnName + " " + item.DataType + "(" + item.MaxLength + "),");
                else
                    builderPrm.Append("  @" + item.ColumnName + " " + item.DataType + ",");
            }

            queryPrm = builderPrm.Remove((builderPrm.Length - 1), 1).AppendLine().ToString();

            //Body
            builderBody.Append("DELETE FROM [" + tableSchema + "].[" + tableName + "] WHERE [CONDITIONS]");

            using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
            {
                fileContent = sr.ReadToEnd().Replace("#Name", spName.ToString()).Replace("#Param", queryPrm.ToString()).Replace("#Body", builderBody.ToString()).Replace("#OrdPrm", fileldPrm.ToString());
            }

            return fileContent.ToString();
        }
#endif
    }
}
