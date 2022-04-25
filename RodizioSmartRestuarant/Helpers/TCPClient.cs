using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Extensions;
using SimpleTcp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RodizioSmartRestuarant.Helpers
{
    public static class TCPClient
    {
        public static SimpleTcpClient client = null;

        public static bool CreateClient()
        {
            string baseIP = LocalIP.GetBaseIP();

            if(LocalIP.GetStoredTCPServerIpPort() != "")
            {
                client = new SimpleTcpClient(LocalIP.GetStoredTCPServerIpPort());
                client.Events.DataReceived += Events_DataReceived;
                client.Events.Disconnected += Events_Disconnected;

                try
                {
                    client.ConnectWithRetries(200);
                    DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
                    dispatcherTimer.Start();
                    return true;
                }
                catch
                {
                    ;
                }
            }
            

            for (int i = 1; i < 255; i++)
            {
                client = new SimpleTcpClient(baseIP + i + ":2000");
                client.Events.DataReceived += Events_DataReceived;
                client.Events.Disconnected += Events_Disconnected;
                try
                {
                    client.ConnectWithRetries(200);
                }
                catch
                {
                    continue;
                }

                DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
                dispatcherTimer.Start();

                LocalIP.SetStoredTCPServerIpPort(baseIP + i + ":2000");
                return true;
            }

            return false;
        }

        private static void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Action();
        }

        private static void Events_Disconnected(object sender, ConnectionEventArgs e)
        {
            Disconnect_Received();
        }

        private static void Reconnect()
        {
            WindowManager.Instance.CloseAllAndOpen(new ReconnectingPage());
        }

        public static bool ConnectToServer()
        {
            try
            {
                client.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        static List<object> awaitresponse = null;
        public async static Task<List<object>> SendRequest(object data, string fPath, RequestObject.requestMethod requestMethod)
        {
            if (client != null)
                if (client.IsConnected)
                {
                    RequestObject requestObject = new RequestObject()
                    {
                        data = data,
                        fullPath = fPath,
                        requestType = requestMethod,
                    };

                    byte[] request = requestObject.ToByteArray<RequestObject>("!MOBILE");

                    string requestString = "{" + Convert.ToBase64String(request) + "}";

                    client.Send(requestString);

                    if (requestMethod != RequestObject.requestMethod.Get)
                        return new List<object>();

                    //await response
                    awaitresponse = null; // Set the state as undetermined

                    while (awaitresponse == null)
                    {
                        await Task.Delay(25);
                    }

                    return awaitresponse;
                }

            return new List<object>();
        }
        public static bool receivingPacket = false;
        static List<string> receivedPacketsBase64 = new List<string>();
        private async static void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            var response = Encoding.UTF8.GetString(e.Data);

            //Update UI after network change
            if (response.Contains("REFRESH"))
            {
                while (true)
                {
                    if (!receivingPacket)
                    {
                        Refresh_Action();
                        return;
                    }

                    await Task.Delay(25);
                }                
            }

            //Sometimes it sends multiple at once
            var singlePackets = splitPackets(Encoding.UTF8.GetString(e.Data));

            foreach (var packet in singlePackets)
            {
                if (receivedPacketsBase64.Contains(packet))
                    continue;

                receivedPacketsBase64.Add(packet);
            }
        }

        static List<string> splitPackets(string input)
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
        static void ProcessPackets()
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

                DataReceived(_receivedData);//There is a data limit for every packet once exceeded is sent in another packet
            }
            //Full Packet
            if (!receivingPacket && _receivedData[_receivedData.Length - 1] == '}')
            {
                receivingPacket = true;

                //Remove Packet Header and Footer
                _receivedData = _receivedData.Remove(0, 1);
                _receivedData = _receivedData.Remove(_receivedData.Length - 1, 1);

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
        }
        private static void CompletePacketReception()
        {
            awaitresponse = (Convert.FromBase64String(receivedData)).FromByteArray<List<object>>();
            receivedData = "";

            receivingPacket = false;
        }
        private static void Action()
        {
            ProcessPackets();
            Disconnect_Action();
        }
        private static void Disconnect_Received()
        {
            startCounting_2 = true;
            elapsedTime_2 = 0;
        }
        private static void Disconnect_Action()
        {
            if (startCounting_2)
                elapsedTime_2 += 50;

            if (elapsedTime_2 > 1000)
            {
                startCounting_2 = false;
                elapsedTime_2 = 0;

                Reconnect();
            }
        }
        private static float elapsedTime_2 = 0;
        private static bool startCounting_2 = false;       

        private static void Refresh_Action()
        {
            WindowManager.Instance.UpdateAllOrderViews_Offline();
        }
        private static string receivedData = "";
        private static void DataReceived(string data)
        {
            receivedData += data;
        }
    }
}
