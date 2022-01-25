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
        public string ipAddress { get; set; }
        public string deviceType { get; set; }
        public string serverAddress { get; private set; } = "127.0.0.1";
        public bool isServer { get; set; }

        public NetworkIdentity(string _deviceType, bool _isServer)
        {
            ipAddress = GetIpAddress();

            deviceType = _deviceType;
        }

        string GetIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return null;
        }
    }
}
