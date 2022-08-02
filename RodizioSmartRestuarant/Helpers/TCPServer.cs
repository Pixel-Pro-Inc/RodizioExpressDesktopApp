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
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Interfaces;

namespace RodizioSmartRestuarant.Helpers
{
    public class TCPServer         
    {
        public static TCPServer Instance { get; set; }
        //This is the server method that will give us all our server properties?
        public SimpleTcpServer server = null;

        IDataService _dataService;
        IOfflineDataService _offlineDataService;

        public List<string> networkIps = new List<string>();
        public string lastRequestSource;
        public bool localDataInUse = false;


        // TRACK: I need definitions to what this is
        public List<IDictionary<string, byte[]>> requestPool = new List<IDictionary<string, byte[]>>();

        public string CreateServer()
        {
            Instance = this;

            string ip = LocalIP.GetLocalIPv4();
            server = new SimpleTcpServer(ip + ":2000");

            server.Events.DataReceived += Events_DataReceived;
            server.Events.ClientConnected += Events_ClientConnected;

            // TRACK: Abel will figure this out later
            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            dispatcherTimer.Start();

            StartServer();

            return ip;
        }

        // I'm assuming this method was extracted so that you can have more things happen later on
        public void StartServer()
        {
            server.Start();
        }

        #region Server Events
        private void Events_ClientConnected(object sender, ConnectionEventArgs e)
        {
            if (!networkIps.Contains(e.IpPort))
                networkIps.Add(e.IpPort);
        }

        int numRetries = 1000;
        int delayMiliSeconds = 100;
        bool receivingPacket = false;
        string lastIpPort = "";
        private async void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            // TRACK: Tell me why we have two variables (response and recieved data, when we could have just had one?
            var response = Encoding.UTF8.GetString(e.Data);

            //Introduced retries to reduce crashes
            // REFACTOR: Here we need to consider how to rethink the retries logic, there is probably a better way of approaching this
            // but like don't ask me cause what in the 
            string receivedData = response;

            for (int i = 0; i < numRetries; i++)
            {
                // TRACK: @Yewo I don't understand what this does
                if (!receivingPacket || receivedData[0] != '[')
                {
                    receivingPacket = true;

                    if (receivedData[0] == '[')
                        receivedData = receivedData.Remove(0, 1);

                    lastIpPort = e.IpPort;
                    DataReceived(receivedData);//There is a data limit for every packet once exceeded is sent in another packet
                    break;
                }

                await Task.Delay(delayMiliSeconds);
            }
        }

        // REFACTOR: This amougst others are have the same name and similar purpose, we have to consider having overloads of a single method (that takes advantage of 'base' syntax)
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DataReceived_Action();
            TryProcessRequest();
        }

        #endregion

        #region Data movement processing
        private string receivedData = "";
        bool startCounting = false;
        int elapsedTime = 0;
        private void DataReceived(string data)
        {
            startCounting = true;
            elapsedTime = 0;

            receivedData += data;
        }

        private void DataReceived_Action()
        {
            if (startCounting)
                elapsedTime += 50;

            if (elapsedTime > 200)
            {
                startCounting = false;
                elapsedTime = 0;

                Dictionary<string, byte[]> keyValuePairs = new Dictionary<string, byte[]>();
                try
                {
                    keyValuePairs.Add(lastIpPort, Convert.FromBase64String(receivedData));
                }
                catch
                {
                    //Incorrect String Format
                    receivedData = "";
                    receivingPacket = false;
                    return;
                }

                // REFACTOR: What happens if the keyValuePairs returns null, we saw similar problems with the OrderItem problem
                requestPool.Add(keyValuePairs);

                receivedData = "";
                receivingPacket = false;
            }
        }


        public void SendData(string ipPort, List<object> data, string fullPath)
        {
            //Convert JObject and JArray to serializable types
            int count = 0;

            List<object> converted = new List<object>();

            foreach (var item in data)
            {
                if (item.GetType() == typeof(JObject))
                {
                    if (fullPath == "Account")
                    {
                        var obj = JsonConvert.DeserializeObject<AppUser>(((JObject)item).ToString());
                        converted.Add(obj.AsDictionary());
                        continue;
                    }

                    var o = JsonConvert.DeserializeObject<MenuItem>(((JObject)item).ToString());
                    converted.Add(o.AsDictionary());
                }

                if (item.GetType() == typeof(JArray))
                {
                    var oj = JsonConvert.DeserializeObject<Order>(((JArray)item).ToString());

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

            if (server.IsListening)
                if (data != null)
                {
                    Byte[] response = data.ToByteArray<List<object>>(lastRequestSource);

                    string data_Base64 = "";

                    if (lastRequestSource == "!MOBILE")
                        data_Base64 = "[" + Convert.ToBase64String(response);

                    if (lastRequestSource == "MOBILE")
                        data_Base64 = Convert.ToBase64String(response);

                    server.Send(ipPort, data_Base64);
                }
        }
        #endregion

        private void TryProcessRequest()
        {
            if(requestPool.Count > 0)
            {
                var e = requestPool[0];

                //if (localDataInUse)
                   //return;

                foreach (var keyValuePair in e)
                {
                    ProcessResponse(keyValuePair.Value.FromByteArray<RequestObject>(), keyValuePair.Key);
                }
            }
        }
        public async void ProcessResponse(RequestObject request, string ipPort)
        {
            // TRACK: Do you think its over kill to have a check on the request.requestType if null?
            switch (request.requestType)
            {
                case RequestObject.requestMethod.Get:
                    List<object> result =(List<object>) await _dataService.GetData(request.fullPath);
                    SendData(ipPort, result, request.fullPath);
                    break;
                case RequestObject.requestMethod.Store:
                    if(lastRequestSource == "MOBILE")
                    {
                        // TRACK: Handles data from mobile.
                        // REFACTOR: I believe you want work done here Yewo, but I'm not sure how to begin
                        var obj = request.data;

                        request.data = JsonConvert.DeserializeObject<OrderItem>(obj.ToString());
                    }
                    await _dataService.StoreData(request.fullPath, request.data);
                    break;
                case RequestObject.requestMethod.Update:
                    await _dataService.StoreData(request.fullPath, request.data);
                    break;
                case RequestObject.requestMethod.Delete:
                    _offlineDataService.OfflineDeleteOrder((Order)request.data);
                    break;
                case RequestObject.requestMethod.UpdateLocalDataRequest:
                    // UPDATE: So I removed the offline thing here. I figured it was necessary but dataservice job is to feed the most relevant information be offline or online
                    List<object> result_1 = (List<object>)await _dataService.GetData(request.fullPath);
                    SendData(ipPort, result_1, request.fullPath);
                    break;
            }

            requestPool.RemoveAt(0);
        }
        public void UpdateAllNetworkDevicesUI()
        {
            //while (localDataInUse)
            //{
                //await Task.Delay(25);
            //}

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
