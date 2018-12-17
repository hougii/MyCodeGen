using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using CodeGen.Web.Models;
using CodeGen.Web.Utility;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CodeGen.Web.Controllers
{
    [EnableCors("AllowCors"), Produces("application/json"), Route("api/Codegen")]
    public class CodegenController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly string _conString = "";//"server=DESKTOP-80DEJMQ; uid=sa; pwd=sa@12345;";

        //定義程式的產生(Key,LiquidPath)
        Dictionary<string, string> _genDefineDic = new Dictionary<string, string> {
                    {"SP","\\template\\StoredProcedure\\SP.liquid" },
                    {"DbModel","\\template\\Model\\Model.liquid" },
                    {"Interface","\\template\\Interface\\Interface.liquid" },
                    {"Service","\\template\\Service\\Service.liquid" },
                    {"Test","\\template\\Test\\Test.liquid" },
                    {"Controller","\\template\\Controller\\Controller.liquid" },
                    {"APIGet","\\template\\WebAPI\\APIController.liquid" },
                    {"TS","\\template\\TS\\TS.liquid" },
                    {"View","\\template\\HtmlForm\\HtmlForm.liquid" },
                    {"Markdown","\\template\\Markdown\\Markdown.liquid" }
                };

        public CodegenController(
            IHostingEnvironment hostingEnvironment,
            IConfiguration config)
        {

            _hostingEnvironment = hostingEnvironment;
            _conString = ConfigurationExtensions.GetConnectionString(config, "Default");
        }

        #region ++++++ Database +++++++
        // api/Codegen/GetDatabaseList
        [HttpGet, Route("GetDatabaseList"), Produces("application/json")]
        public List<vmDatabase> GetDatabaseList()
        {
            List<vmDatabase> data = new List<vmDatabase>();
            using (SqlConnection con = new SqlConnection(_conString))
            {
                int count = 0; con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') ORDER BY create_date", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            count++;
                            data.Add(new vmDatabase()
                            {
                                DatabaseId = count,
                                DatabaseName = dr[0].ToString()
                            });
                        }
                    }
                }
            }
            return data.ToList();
        }

        // api/Codegen/GetDatabaseTableList
        [HttpPost, Route("GetDatabaseTableList"), Produces("application/json")]
        public List<TableInfo> GetDatabaseTableList([FromBody]vmParam model)
        {
            var dbName = model.DatabaseName;
            List<TableInfo> data = getAllTableFromDb(dbName);

            return data.ToList();
        }

       
        /// <summary>
        /// 取得Database下指定Table的欄位資訊
        /// </summary>
        /// <remarks>這是取得Table下的Column清單，並會Keep至Web端</remarks>
        /// <param name="model"></param>
        /// <returns></returns>
        // api/Codegen/GetDatabaseTableColumnList
        [HttpPost, Route("GetDatabaseTableColumnList"), Produces("application/json")]
        public List<ColumnInfo> GetDatabaseTableColumnList([FromBody]vmParam model)
        {
            var dbName = model.DatabaseName;
            var tableName = model.TableName;
            return getColumnsFromTable( dbName, tableName);
        }

        [HttpPost, Route("GetMapColumns"), Produces("application/json")]
        public IActionResult GetMapColumns([FromBody]vmParam model) {
            var result = "";
            string webRootPath = _hostingEnvironment.WebRootPath; //From web
            var filePath = webRootPath + "\\template\\ColumnMap\\colmap.json";
            if (!System.IO.File.Exists(filePath)) {
                using (StreamWriter sw = System.IO.File.CreateText(filePath)) {
                    sw.Write("[]");
                }
            }
            var content = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
            result = content;
            return Json(result);
        }

        [HttpPost, Route("SaveMapColumns"), Produces("application/json")]
        public void SaveMapColumns([FromBody]object data)
        {
            string webRootPath = _hostingEnvironment.WebRootPath; //From web
            var filePath = webRootPath + "\\template\\ColumnMap\\colmap.json";
            System.IO.File.WriteAllText(filePath, string.Empty);
            var content = JsonConvert.SerializeObject(data);   
            System.IO.File.WriteAllText(filePath, content);

        }
        #endregion



        #region +++++ CodeGeneration +++++
        /// <summary>
        /// (TW)執行產生Code的觸發點
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        // api/Codegen/GenerateCode
        [HttpPost, Route("GenerateCode"), Produces("application/json")]
        public IActionResult GenerateCode([FromBody]object data)
        {   
            Dictionary<string, string> resultCollectionDic = new Dictionary<string, string>();
            try
            {
                string webRootPath = _hostingEnvironment.WebRootPath; //From wwwroot
                string contentRootPath = _hostingEnvironment.ContentRootPath; //From Others

                var postData = JsonConvert.DeserializeObject<dynamic>(data.ToString());
                var tableJson = postData.table;
                var columnsJson = postData.columns;
                

                //(TW)取得Table資訊
                var tableInfo = JsonConvert.DeserializeObject<TableInfo>(tableJson.ToString());
                //(TW)頁面上勾選的Table Columns資訊反序列化 (包括從DB取得的資訊內容)
                List<ColumnInfo> columnInfos = JsonConvert.DeserializeObject<List<ColumnInfo>>(columnsJson.ToString());

                var tableInfoForLiquid = new TableInfoForLiquid(tableInfo);
                var columnInfosForLiquid = columnInfos.Select(m => new ColumnInfoForLiquid(m)).ToList();
                var otherInfoForLiquid = getOtherInfoForLiquid(tableInfoForLiquid, columnInfosForLiquid);

                
                foreach (var item in _genDefineDic) {
                    var result = new CustomGenerator(item.Value).Generate(tableInfoForLiquid, columnInfosForLiquid, otherInfoForLiquid, webRootPath);
                    resultCollectionDic.Add(item.Key, result);
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return Json(resultCollectionDic);
        }

        [HttpPost, Route("GenerateAllTable"), Produces("application/json")]
        public IActionResult GenerateAllTable([FromBody]object data)
        {
            Dictionary<string, string> resultCollectionDic = new Dictionary<string, string>();
            var builder = new StringBuilder("");
            var fileContentMarkdown = "";

            var markdownHeader = @"
# 資料庫說明文件

# 目錄

[TOC]

# 修訂歷程

| Date       | Author | Version | Change Reference         |
| ---------- | ------ | ------- | ------------------------ |
|  |  |     |  |
# 資料表設計

";
            builder.AppendLine(markdownHeader);
            try
            {
                string webRootPath = _hostingEnvironment.WebRootPath; //From wwwroot
                string contentRootPath = _hostingEnvironment.ContentRootPath; //From Others

                var postData = JsonConvert.DeserializeObject<dynamic>(data.ToString());
                var databaseJson = postData.database;
                bool enableMap = (bool)postData.enableMap;
                string dbName = databaseJson.DatabaseName;

                List<TableInfo> tableInfo = getAllTableFromDb(dbName);
                //Markdown
                var markdown = _genDefineDic.Where(m => m.Key == "Markdown").FirstOrDefault();

                foreach (TableInfo table in tableInfo) {
                    var tableName = table.TableName;
                    List<ColumnInfo> columnInfos = getColumnsFromTable(dbName, tableName);

                    var tableInfoForLiquid = new TableInfoForLiquid(table);
                    var columnInfosForLiquid = columnInfos.Select(m => new ColumnInfoForLiquid(m)).ToList();
                    var otherInfoForLiquid = getOtherInfoForLiquid(tableInfoForLiquid, columnInfosForLiquid);

                   fileContentMarkdown = new CustomGenerator(markdown.Value).Generate(tableInfoForLiquid, columnInfosForLiquid, otherInfoForLiquid, webRootPath);
                    builder.AppendLine(fileContentMarkdown);
                }

                resultCollectionDic.Add(markdown.Key, builder.ToString());
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return Json(resultCollectionDic);
        }

        /// <summary>
        /// Get All Tables From Database
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private List<TableInfo> getAllTableFromDb(string dbName)
        {
            List<TableInfo> data = new List<TableInfo>();
            string conString_ = _conString + " Database=" + dbName + ";";
            using (SqlConnection con = new SqlConnection(conString_))
            {
                int count = 0;
                con.Open();
                var sql = @"
            select st.TABLE_SCHEMA as TableSchema,st.TABLE_NAME as TableName,ep.value as [Description] 
                from INFORMATION_SCHEMA.TABLES st
                OUTER APPLY fn_listextendedproperty(default,
                                    'SCHEMA', TABLE_SCHEMA,
                                    'TABLE', TABLE_NAME, null, null) ep
            where st.TABLE_TYPE='BASE TABLE'";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ++count;
                            data.Add(new TableInfo
                            {
                                TableId = count,
                                TableSchema = Convert.ToString(reader["TableSchema"]),
                                TableName = Convert.ToString(reader["TableName"]),
                                TableDescription = Convert.ToString(reader["Description"])
                            });
                        }
                    }
                }
            }
            return data;
        }
        /// <summary>
        /// (EN)Get Columns From Table
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private List<ColumnInfo> getColumnsFromTable(string dbName, string tableName)
        {
            List<ColumnInfo> data = new List<ColumnInfo>();
            string conString_ = _conString + " Database=" + dbName + ";";
            using (SqlConnection con = new SqlConnection(conString_))
            {
                int count = 0;
                con.Open();
                //20181108-howard fix:改寫取得Column的資訊語法
                //          --加上column 描述語法
                //var sql = @"SELECT COLUMN_NAME, DATA_TYPE, ISNULL(CHARACTER_MAXIMUM_LENGTH,0), IS_NULLABLE, TABLE_SCHEMA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName ORDER BY ORDINAL_POSITION";
                var sql = @" select 
	        st.name [Table],
	        sc.name [Column],
	        sep.value as [Description],
		info.DATA_TYPE as [ColumnType],
		ISNULL(info.CHARACTER_MAXIMUM_LENGTH,0) as [Length], 
		info.IS_NULLABLE as [Nullable], --get[YES/NO]word.
		info.TABLE_SCHEMA as [Schema]
    from sys.tables st  --此資料庫下的Table
    inner join sys.columns sc on st.object_id = sc.object_id
    left join sys.extended_properties sep on st.object_id = sep.major_id
                                         and sc.column_id = sep.minor_id
                                         and sep.name = 'MS_Description'
   left join INFORMATION_SCHEMA.COLUMNS info on st.name= info.TABLE_NAME
					 and sc.name = info.COLUMN_NAME
    where st.name = @TableName
";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    //20181108-howard fix:改寫改用parameter的方式處理
                    cmd.Parameters.AddWithValue("TableName", tableName);
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            count++;
                            data.Add(new ColumnInfo()
                            {
                                ColumnId = count,
                                ColumnName = dr["Column"].ToString(),
                                DataType = dr["ColumnType"].ToString(),
                                MaxLength = Convert.ToInt32(dr["Length"].ToString()),
                                IsNullable = (dr["Nullable"].ToString() == "YES") ? true : false,
                                TableName = tableName,
                                TableSchema = dr["Schema"].ToString(),
                                ColumnDescription = (dr["Description"].ToString()??"").Replace("\\n", "").Replace("\\r", "")
                            });
                        }
                    }
                }
            }
            return data.ToList();
        }

        

        /// <summary>
        /// (TW)取得其它資訊內容
        /// </summary>
        /// <param name="tableInfoForLiquid"></param>
        /// <param name="columnsForLiquid"></param>
        /// <returns></returns>
        private OtherInfoForLiquid getOtherInfoForLiquid(TableInfoForLiquid tableInfoForLiquid, List<ColumnInfoForLiquid> columnsForLiquid)
        {
            var result = new OtherInfoForLiquid();
            var identityColumn = columnsForLiquid.FirstOrDefault();//暫以第一筆表示
            if (identityColumn != null)
            {
                result.IndentityColumn = identityColumn.MapColumnName;
                result.IndentityModelType = identityColumn.ModelType;
                result.IndentityColumnDescription = identityColumn.ColumnDescription;
            }
            return result;
        }

#endregion
    }
}
