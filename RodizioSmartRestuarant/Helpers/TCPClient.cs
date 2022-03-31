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
            List<string> networkIPs = LocalIP.ScanNetwork();

            foreach (var ip in networkIPs)
            {
                client = new SimpleTcpClient(ip + ":2000");
                client.Events.DataReceived += Events_DataReceived;
                client.Events.Disconnected += Events_Disconnected;

                if (ConnectToServer())
                {
                    DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
                    dispatcherTimer.Start();
                    return true;
                }
            }

            return false;
        }

        private static void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Action();
        }

        private static void Events_Disconnected(object sender, ConnectionEventArgs e)
        {
            StartUp.InitNetworking();
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
            if (client.IsConnected)
            {
                RequestObject requestObject = new RequestObject()
                {
                    data = data,
                    fullPath = fPath,
                    requestType = requestMethod,
                };

                client.Send(requestObject.ToByteArray<RequestObject>("!MOBILE"));

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
        private static void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            var response = Encoding.UTF8.GetString(e.Data);

            //Update UI after network change
            if (response == "REFRESH")
            {
                Refresh_UI();
                return;
            }

            Byte[] bytes = Convert.FromBase64String(response);
            DataReceived(Encoding.UTF8.GetString(bytes));//There is a data limit for every packet once exceeded is sent in another packet
        }
        private static void Action()
        {
            Refresh_Action();
            DataReceived_Action();
        }
        private static void DataReceived_Action()
        {
            if (startCounting_1)
                elapsedTime_1++;

            if (elapsedTime_1 > 1)
            {
                startCounting_1 = false;
                elapsedTime_1 = 0;

                awaitresponse = (Encoding.UTF8.GetBytes(receivedData)).FromByteArray<List<object>>();
                receivedData = "";
            }
        }
        private static float elapsedTime_1 = 0;
        private static bool startCounting_1 = false;

        private static void Refresh_Action()
        {
            if (startCounting)
                elapsedTime++;

            if (elapsedTime > 1)
            {
                startCounting = false;
                elapsedTime = 0;

                WindowManager.Instance.UpdateAllOrderViews();
            }
        }
        private static float elapsedTime = 0;
        private static bool startCounting = false;
        private static void Refresh_UI()
        {
            startCounting = true;
            elapsedTime = 0;
        }
        private static string receivedData = "";
        private static void DataReceived(string data)
        {
            startCounting_1 = true;
            elapsedTime_1 = 0;

            receivedData += data;
        }
    }
}
