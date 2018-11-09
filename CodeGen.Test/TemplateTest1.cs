using System;
using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeGen.Test
{

    /// <summary>
    /// Table Infomation
    /// </summary>
    public class TableInfo:ILiquidizable
    {
        /// <summary>
        /// Table no
        /// </summary>
        public int TableId { get; set; }
        /// <summary>
        /// Table Name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Table Description
        /// </summary>
        public string TableDescription { get; set; }

        /// <summary>
        /// (TW)為了使此類別直接轉為Liquid使用物件
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            return new { TableId , TableName, TableDescription };
        }
    }
    [TestClass]
    public class TemplateTest1
    {
        /// <summary>
        /// (TW)實驗性質的測試方法
        /// </summary>
        [TestMethod]
        public void TestLiquidSample()
        {
   
            
            //        var liquidTemplate = @"
    ///// <summary>
    ///// {{table.TableDescription}}
    ///// </summary>
    //public class {{table.TableName}} ";

            var liquidTemplate = "{{table.TableDescription}} {{table.TableName}} ";


            var table = new TableInfo { TableName = "XXXX", TableDescription = "AAAAAA" };
            //Template.RegisterSafeType(typeof(TableInfo), new[] { "TableName", "TableDescription" }, m => m);
            Template template = Template.Parse(liquidTemplate);


            //var output = template.Render(Hash.FromAnonymousObject(new { table = new TableInfo() {  TableName = "Bar" } }));
            //var output = template.Render(Hash.FromAnonymousObject(table));

            //Liquid :when model not implement 'ILiquidizable'
            //Liquid syntax error: Object 'CodeGen.Test.TableInfo' is invalid because it is neither a built-in type nor implements ILiquidizable
            //var output = template.Render(Hash.FromAnonymousObject(new { table = table }));


            //success:because use the Anonymous Object ,
            //var output = template.Render(Hash.FromAnonymousObject(new { table = new { TableName = "XXXX", TableDescription = "AAAAAA" } }));

            ////got Missing property. Did you mean 'table_description'?
            //var output = template.Render(Hash.FromAnonymousObject(new { table = table }));

        }

        /// <summary>
        /// (TW)使用Model較可行的方式處理
        /// </summary>
        [TestMethod]
        public void TestLiquidWithModel() {
            var liquidTemplate = "{{table.TableDescription}} {{table.TableName}} ";
            var table = new TableInfo { TableName = "XXXX", TableDescription = "AAAAAA" };
            Template template = Template.Parse(liquidTemplate);

            //Success: can get data by implement ILiquidizable
            var output = template.Render(Hash.FromAnonymousObject(new { table = table }));

            //Fail: will get Empty data , because final not table.TableName format.
            //var output = template.Render(Hash.FromAnonymousObject(table));
        }

    }
}
