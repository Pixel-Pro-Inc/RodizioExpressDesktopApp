namespace RodizioSmartRestuarant.Core.Entities
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
