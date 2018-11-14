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
    public class SpGenerator : BaseGenerator
    {
        public override string LiquidPath => "\\template\\StoredProcedure\\SP.liquid";

    }
}
