﻿using CodeGen.Web.Models;
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
    public class ControllerGenerator : BaseGenerator
    {
        public override string LiquidPath => "\\template\\Controller\\Controller.liquid";

    }
}