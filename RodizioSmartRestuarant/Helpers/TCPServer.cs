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

namespace RodizioSmartRestuarant.Helpers
{
    public static class TCPServer
    {
        public static SimpleTcpServer server = null;

        public static void CreateServer(string ip)
        {
            server = new SimpleTcpServer(ip + ":2000");

            server.Events.DataReceived += Events_DataReceived;

            StartServer();
        }

        private static void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            ProcessResponse(e.Data.FromByteArray<RequestObject>(), e.IpPort);
        }

        public static void StartServer()
        {
            server.Start();
        }

        public async static void ProcessResponse(RequestObject request, string ipPort)
        {
            switch (request.requestType)
            {
                case RequestObject.requestMethod.Get:
                    var result = await FirebaseDataContext.Instance.GetData(request.fullPath);
                    SendData(ipPort, result, request.fullPath);
                    break;
                case RequestObject.requestMethod.Store:
                    await FirebaseDataContext.Instance.StoreData(request.fullPath, request.data);
                    break;
                case RequestObject.requestMethod.Update:
                    await FirebaseDataContext.Instance.StoreData(request.fullPath, request.data);
                    break;
            }
        }

        public static void SendData(string ipPort, List<object> data, string fullPath)
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
                    Byte[] response = data.ToByteArray<List<object>>();

                    string data_Base64 = Convert.ToBase64String(response);

                    server.Send(ipPort, data_Base64);
                }
        }
    }
}
