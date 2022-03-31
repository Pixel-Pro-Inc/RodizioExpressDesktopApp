using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    public class NetworkInterfaceObject
    {
        public string IPAddress { get; set; }
        public NetworkInterfaceType _type { get; set; }
    }
}
