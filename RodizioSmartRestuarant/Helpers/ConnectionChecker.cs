using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Helpers
{
    public class ConnectionChecker
    {
        Ping ping = new Ping();
        PingReply result;
        public bool CheckConnection()
        {
            result = ping.Send(IPAddress.Parse("8.8.8.8"));

            if (result.Status == IPStatus.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
