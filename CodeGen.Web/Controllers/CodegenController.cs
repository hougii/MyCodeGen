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
            List<TableInfo> data = new List<TableInfo>();
            string conString_ = _conString + " Database=" + model.DatabaseName + ";";
            using (SqlConnection con = new SqlConnection(conString_))
            {
                int count = 0; con.Open();
                DataTable schema = con.GetSchema("Tables");
                foreach (DataRow row in schema.Rows)
                {
                    count++;
                    data.Add(new TableInfo()
                    {
                        TableId = count,
                        TableName = row[2].ToString()
                    });
                }
            }

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
            List<ColumnInfo> data = new List<ColumnInfo>();
            string conString_ = _conString + " Database=" + model.DatabaseName + ";";
            using (SqlConnection con = new SqlConnection(conString_))
            {
                int count = 0; con.Open();
                //20181108-howard fix:改寫取得Column的資訊語法
                //          --加上column 描述語法
                //var sql = @"SELECT COLUMN_NAME, DATA_TYPE, ISNULL(CHARACTER_MAXIMUM_LENGTH,0), IS_NULLABLE, TABLE_SCHEMA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName ORDER BY ORDINAL_POSITION";
                var sql = @" select 
	        st.name [Table],
	        sc.name [Column],
	        sep.value as [Description],
		info.DATA_TYPE as [ColumnType],
		ISNULL(info.CHARACTER_MAXIMUM_LENGTH,0) as [Length], 
		info.IS_NULLABLE as [Nullable],
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
                    cmd.Parameters.AddWithValue("TableName", model.TableName);
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
                                MaxLength = dr["Length"].ToString(),
                                IsNullable = dr["Nullable"].ToString(),
                                TableName = model.TableName.ToString(),
                                TableSchema = dr["Schema"].ToString(),
                                ColumnDescription = dr["Description"].ToString()
                            });
                        }
                    }
                }
            }
            return data.ToList();
        }
        #endregion



        #region +++++ CodeGeneration +++++
        /// <summary>
        /// 執行產生Code的觸發點
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        // api/Codegen/GenerateCode
        [HttpPost, Route("GenerateCode"), Produces("application/json")]
        public IActionResult GenerateCode([FromBody]object[] data)
        {
            List<string> spCollection = new List<string>();
            try
            {
                string webRootPath = _hostingEnvironment.WebRootPath; //From wwwroot
                string contentRootPath = _hostingEnvironment.ContentRootPath; //From Others

                //TODO:
                var dbTable = new TableInfo();
                //頁面上勾選的Table Columns資訊反序列化 (包括從DB取得的資訊內容)
                var dbColumns = JsonConvert.DeserializeObject<List<ColumnInfo>>(data[0].ToString());

                string fileContentSet = string.Empty; string fileContentGet = string.Empty;
                string fileContentPut = string.Empty; string fileContentDelete = string.Empty;
                string fileContentVm = string.Empty; string fileContentView = string.Empty;
                string fileContentNg = string.Empty; string fileContentAPIGet = string.Empty;
                string fileContentAPIGetById = string.Empty;

                //SP
                fileContentSet = SpGenerator.GenerateSetSP(dbColumns, webRootPath);
                fileContentGet = SpGenerator.GenerateGetSP(dbColumns, webRootPath);
                fileContentPut = SpGenerator.GeneratePutSP(dbColumns, webRootPath);
                fileContentDelete = SpGenerator.GenerateDeleteSP(dbColumns, webRootPath);
                spCollection.Add(fileContentSet);
                spCollection.Add(fileContentGet);
                spCollection.Add(fileContentPut);
                spCollection.Add(fileContentDelete);

                //VM
                fileContentVm = ModelGenerator.GenerateModel(dbTable,dbColumns, webRootPath);
                spCollection.Add(fileContentVm);

                //VU
                fileContentView = ViewGenerator.GenerateForm(dbColumns, webRootPath);
                spCollection.Add(fileContentView);

                //NG
                fileContentNg = NgGenerator.GenerateNgController(dbColumns, webRootPath);
                spCollection.Add(fileContentNg);

                //API
                fileContentAPIGet = APIGenerator.GenerateAPIGet(dbColumns, webRootPath);
                spCollection.Add(fileContentAPIGet);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return Json(new
            {
                spCollection
            });
        }

        #endregion
    }
}
