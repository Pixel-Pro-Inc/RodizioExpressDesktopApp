using SimpleTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RodizioSmartRestuarant.Extensions;
using RodizioSmartRestuarant.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RodizioSmartRestuarant.Entities;
using System.Net.NetworkInformation;
using System.Windows.Threading;

namespace RodizioSmartRestuarant.Helpers
{
    public class TCPServer : OfflineDataHelpers        
    {
        public static TCPServer Instance { get; set; }
        public SimpleTcpServer server = null;
        public List<string> networkIps = new List<string>();
        public string lastRequestSource;
        public bool localDataInUse = false;
        public List<IDictionary<string, byte[]>> requestPool = new List<IDictionary<string, byte[]>>();

        public string CreateServer()
        {
            Instance = this;

            string ip = LocalIP.GetLocalIPv4();
            server = new SimpleTcpServer(ip + ":2000");

            server.Events.DataReceived += Events_DataReceived;
            server.Events.ClientConnected += Events_ClientConnected;

            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            dispatcherTimer.Start();

            StartServer();

            return ip;
        }       

        private void Events_ClientConnected(object sender, ConnectionEventArgs e)
        {
            if (!networkIps.Contains(e.IpPort))
                networkIps.Add(e.IpPort);
        }

        bool receivingPacket = false;
        string lastIpPort = "";
        List<string> receivedPacketsBase64 = new List<string>();
        List<string> ipPorts = new List<string>();
        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            //Sometimes it sends multiple at once
            var singlePackets = splitPackets(Encoding.UTF8.GetString(e.Data));

            foreach (var packet in singlePackets)
            {
                if (receivedPacketsBase64.Contains(packet))
                    continue;

                receivedPacketsBase64.Add(packet);
                ipPorts.Add(e.IpPort);
            }
        }
        List<string> splitPackets(string input)
        {
            List<string> output = new List<string>();

            var strings = input.Split('{', '}');

            foreach (var str in strings)
            {
                if (!string.IsNullOrEmpty(str))
                    output.Add("{" + str + "}");
            }

            return output;
        }
        void ProcessPackets()
        {
            if (receivedPacketsBase64.Count == 0)
                return;

            string _receivedData = receivedPacketsBase64[0];

            //Start of Packet
            if (!receivingPacket && _receivedData[_receivedData.Length - 1] != '}')
            {
                receivingPacket = true;

                //Remove Packet Header
                _receivedData = _receivedData.Remove(0, 1);

                lastIpPort = ipPorts[0];
                DataReceived(_receivedData);//There is a data limit for every packet once exceeded is sent in another packet
            }
            //Full Packet
            if (!receivingPacket && _receivedData[_receivedData.Length - 1] == '}')
            {
                receivingPacket = true;

                //Remove Packet Header and Footer
                _receivedData = _receivedData.Remove(0, 1);
                _receivedData = _receivedData.Remove(_receivedData.Length - 1, 1);

                lastIpPort = ipPorts[0];
                DataReceived(_receivedData);//There is a data limit for every packet once exceeded is sent in another packet
                CompletePacketReception();
            }
            //Middle of Packet
            else if (receivingPacket && _receivedData[0] != '{' && _receivedData[_receivedData.Length - 1] != '}')
            {
                DataReceived(_receivedData);
            }
            //End of Packet
            else if (receivingPacket && _receivedData[_receivedData.Length - 1] == '}')
            {
                _receivedData = _receivedData.Remove(_receivedData.Length - 1, 1);
                DataReceived(_receivedData);
                CompletePacketReception();
            }

            receivedPacketsBase64.RemoveAt(0);
            ipPorts.RemoveAt(0);
        }
        private string receivedData = "";
        private void DataReceived(string data)
        {
            receivedData += data;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            ProcessPackets();
        }

        private void CompletePacketReception()
        {
            Dictionary<string, byte[]> keyValuePairs = new Dictionary<string, byte[]>();
            keyValuePairs.Add(lastIpPort, Convert.FromBase64String(receivedData));

            requestPool.Add(keyValuePairs);

            receivedData = "";
            receivingPacket = false;

            TryProcessRequest();
        }

        private void TryProcessRequest()
        {
            if(requestPool.Count > 0)
            {
                var e = requestPool[0];

                if (localDataInUse)
                    return;

                foreach (var keyValuePair in e)
                {
                    ProcessResponse(keyValuePair.Value.FromByteArray<RequestObject>(), keyValuePair.Key);
                }
            }
        }

        public void StartServer()
        {
            server.Start();
        }

        public async void ProcessResponse(RequestObject request, string ipPort)
        {
            switch (request.requestType)
            {
                case RequestObject.requestMethod.Get:
                    var result = await OfflineGetData(request.fullPath);
                    SendData(ipPort, result, request.fullPath);
                    break;
                case RequestObject.requestMethod.Store:
                    if(lastRequestSource == "MOBILE")
                    {
                        //Handles data from mobile
                        var obj = request.data;

                        request.data = JsonConvert.DeserializeObject<OrderItem>(obj.ToString());
                    }
                    await OfflineStoreData(request.fullPath, request.data);
                    break;
                case RequestObject.requestMethod.Update:
                    await OfflineStoreData(request.fullPath, request.data);
                    break;
                case RequestObject.requestMethod.Delete:
                    OfflineDeleteOrder((List<OrderItem>)request.data);
                    break;
                case RequestObject.requestMethod.UpdateLocalDataRequest:
                    var result_1 = await OfflineGetData(request.fullPath);
                    SendData(ipPort, result_1, request.fullPath);
                    break;
            }

            requestPool.RemoveAt(0);
        }

        public void SendData(string ipPort, List<object> data, string fullPath)
        {
            //Convert JObject and JArray to serializable types
            int count = 0;

            List<object> converted = new List<object>();

            foreach (var item in data)
            {
                if(item.GetType() == typeof(JObject))
                {
                    if(fullPath == "Account")
                    {
                        var obj = JsonConvert.DeserializeObject<AppUser>(((JObject)item).ToString());
                        converted.Add(obj.AsDictionary());
                        continue;
                    }

                    var o = JsonConvert.DeserializeObject<MenuItem>(((JObject)item).ToString());
                    converted.Add(o.AsDictionary());
                }

                if(item.GetType() == typeof(JArray))
                {
                    var oj = JsonConvert.DeserializeObject<List<OrderItem>>(((JArray)item).ToString());

                    List<object> list = new List<object>();
                    for (int i = 0; i < oj.Count; i++)
                    {
                        list.Add(oj[i].AsDictionary());
                    }

                    converted.Add(list);
                }

                count++;
            }

            if (converted.Count == data.Count)
                data = converted;

            if(server.IsListening)
                if(data != null)
                {
                    Byte[] response = data.ToByteArray<List<object>>(lastRequestSource);

                    string data_Base64 = "";

                    if (lastRequestSource == "!MOBILE")
                        data_Base64 = "{" + Convert.ToBase64String(response) + "}";

                    if (lastRequestSource == "MOBILE")
                        data_Base64 = Convert.ToBase64String(response);

                    server.Send(ipPort, data_Base64);
                }
        }

        public async void UpdateAllNetworkDevicesUI()
        {
            while (localDataInUse)
            {
                await Task.Delay(25);
            }

            //Sends a specific byte array to trigger a UI refresh
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] data = enc.GetBytes("REFRESH");

            foreach (var ipPort in networkIps)
            {
                server.Send(ipPort, data);
            }            
        }
    }
}
