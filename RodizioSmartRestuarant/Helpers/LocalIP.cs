using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant.Helpers
{
    public static class LocalIP
    {
        public static List<NetworkInterfaceObject> GetMachineIPv4s()
        {
            List<NetworkInterfaceObject> output = new List<NetworkInterfaceObject>();

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Read the IP configuration for each network 
                IPInterfaceProperties properties = item.GetIPProperties();

                if (! new ConnectionChecker().CheckLAN())
                    continue;

                if (item.OperationalStatus != OperationalStatus.Up)
                    continue;

                // Each network interface may have multiple IP addresses 
                foreach (IPAddressInformation ip in properties.UnicastAddresses)
                {
                    // We're only interested in IPv4 addresses for now 
                    if (ip.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    // Ignore loopback addresses (e.g., 127.0.0.1) 
                    if (IPAddress.IsLoopback(ip.Address))
                        continue;

                    output.Add(new NetworkInterfaceObject() { 
                        IPAddress = ip.Address.ToString(),
                        _type = item.NetworkInterfaceType
                    });
                }
            }

            return output;
        }
        public static string GetLocalIPv4()
        {
            List<NetworkInterfaceObject> networkInterfaceObjects = (GetMachineIPv4s().Where(n => n._type == GetPrefferedNetworkInterfaceType()).ToList());
            return networkInterfaceObjects.Count > 0 ? networkInterfaceObjects[0].IPAddress: "1.0.0.0";
        }

        public static NetworkInterfaceType GetPrefferedNetworkInterfaceType()
        {
            List<object> data = (List<object>)(new SerializedObjectManager().RetrieveData(Directories.NetworkInterface));
            NetworkInterfaceType networkInterfaceType = data == null ? NetworkInterfaceType.Wireless80211 : (NetworkInterfaceType)data[0];

            return networkInterfaceType;
        }

        public static string GetBaseIP() 
        {
            string ip = GetLocalIPv4();
            string baseIP = "";

            int count = 0;

            for (int i = 0; i < ip.Length; i++)
            {
                if(ip[i] == '.')
                    count++;

                baseIP += ip[i];

                if (count == 3)
                    break;
            }

            return baseIP;
        }

        private static List<Ping> pingers = new List<Ping>();
        private static int instances = 0;

        private static object @lock = new object();

        private static int timeOut = 250;

        private static int ttl = 5;

        public static List<string> ipsOnNetwork = new List<string>();

        public static List<string> ScanNetwork()
        {
            string baseIP = GetBaseIP();//"192.168.1."
            ipsOnNetwork.Clear();

            CreatePingers(255);

            PingOptions po = new PingOptions(ttl, true);
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] data = enc.GetBytes("abababababababababababababababab");

            int cnt = 1;

            foreach (Ping p in pingers)
            {
                lock (@lock)
                {
                    instances += 1;
                }

                p.SendAsync(string.Concat(baseIP, cnt.ToString()), timeOut, data, po);
                cnt += 1;
            }

            while (instances > 0)
            {
                Task.Delay(1000);
            }

            DestroyPingers();

            return ipsOnNetwork;
        }

        public static void Ping_completed(object s, PingCompletedEventArgs e)
        {
            lock (@lock)
            {
                instances -= 1;
            }

            if (e.Reply.Status == IPStatus.Success)
                ipsOnNetwork.Add(e.Reply.Address.ToString());
        }

        private static void CreatePingers(int cnt)
        {
            for (int i = 1; i <= cnt; i++)
            {
                Ping p = new Ping();
                p.PingCompleted += Ping_completed;
                pingers.Add(p);
            }
        }

        private static void DestroyPingers()
        {
            foreach (Ping p in pingers)
            {
                p.PingCompleted -= Ping_completed;
                p.Dispose();
            }

            pingers.Clear();

        }
    }
}
