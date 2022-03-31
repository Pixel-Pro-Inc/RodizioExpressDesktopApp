using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    public class NetworkIdentity
    {
        public string deviceType { get; set; }
        public bool isServer { get; set; }
        public string serverIP { get; set; }

        public NetworkIdentity(string _deviceType, bool _isServer)
        {
            isServer = _isServer;

            deviceType = _deviceType;
        }
    }
}
