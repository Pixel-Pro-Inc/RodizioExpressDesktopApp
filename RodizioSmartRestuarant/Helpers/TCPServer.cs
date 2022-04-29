﻿using SimpleTcp;
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

        int numRetries = 1000;
        int delayMiliSeconds = 100;
        bool receivingPacket = false;
        string lastIpPort = "";
        private async void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            var response = Encoding.UTF8.GetString(e.Data);

            //Introduced retries to reduce crashes
            string receivedData = response;

            for (int i = 0; i < numRetries; i++)
            {
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
        private string receivedData = "";
        bool startCounting = false;
        int elapsedTime = 0;
        private void DataReceived(string data)
        {
            startCounting = true;
            elapsedTime = 0;

            receivedData += data;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DataReceived_Action();
            TryProcessRequest();
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
                keyValuePairs.Add(lastIpPort, Convert.FromBase64String(receivedData));

                requestPool.Add(keyValuePairs);

                receivedData = "";
                receivingPacket = false;
            }
        }

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
                        data_Base64 = "[" + Convert.ToBase64String(response);

                    if (lastRequestSource == "MOBILE")
                        data_Base64 = Convert.ToBase64String(response);

                    server.Send(ipPort, data_Base64);
                }
        }

        public async void UpdateAllNetworkDevicesUI()
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
