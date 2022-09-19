using System.Net.NetworkInformation;

namespace RodizioSmartRestuarant.Core.Entities
{
    public class NetworkInterfaceObject
    {
        public string IPAddress { get; set; }
        public NetworkInterfaceType _type { get; set; }
    }
}
