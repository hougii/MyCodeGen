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
    /// <summary>
    /// 客製產生
    /// </summary>
    public class CustomGenerator : BaseGenerator
    {
        private string _liquidPath;
        public override string LiquidPath { get { return _liquidPath; } }

        public CustomGenerator(string liquidPath)
        {
            _liquidPath = liquidPath;
        }
        //
    }
}
