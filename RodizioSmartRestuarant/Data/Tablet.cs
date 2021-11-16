using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Data
{
    public static class Tablet
    {
        static Random r = new Random();
        private readonly static int TabletNumber = r.Next(0, 50);
        private static string _BranchIp;
        public static int GetTabletNumber() { return TabletNumber; }
    }
}
