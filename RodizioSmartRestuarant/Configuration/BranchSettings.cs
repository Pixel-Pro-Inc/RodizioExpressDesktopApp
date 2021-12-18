using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Configuration
{
    public class BranchSettings
    {
        public static BranchSettings Instance { get; set; }
        public string branchId { get; set; }
        public string printerName { get; set; }
        public Branch branch { get; set; }
        public BranchSettings()
        {
            branchId = "rd29502";

            Instance = this;
        }
    }
}
