using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Helpers;
using RodizioSmartRestuarant.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant.Configuration
{
    public class BranchSettings
    {
        public static BranchSettings Instance { get; set; }
        IDataService _dataService { get; set; }
        public string branchId { get; set; }
        public string printerName { get; set; }
        public Branch branch { get; set; }
        public BranchSettings()
        {
            Instance = this;

            Init();
        }
        public void Init()
        {
            //Retrieve Data
            List<object> data = (List<object>)(new SerializedObjectManager().RetrieveData(Directories.BranchId));
            List<object> data1 = (List<object>)(new SerializedObjectManager().RetrieveData(Directories.PrinterName));
            
            if (data != null)
            {
                string bId = (string)data[0];
                string pName = (string)data1[0];

                //Check if empty
                if (bId != null)
                {
                    branchId = "rd" + bId;
                    printerName = pName;

                    _dataService.SetBranchId();
                }
            }
        }
    }
}
