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

    public class ModelGenerator : BaseGenerator
    {
        public override string LiquidPath => "\\template\\Model\\Model.liquid";
    }
}
