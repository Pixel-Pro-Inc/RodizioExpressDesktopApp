using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    public class Enums
    {
        public enum UIChangeSource
        {
            Deletion,
            Edit,
            Addition,
            Search,
            StartUp
        }
        public enum Directories
        {
            Order, 
            Menu, 
            Account, 
            Branch,
            BranchId,
            PrinterName,
            Settings
        }
    }
}
