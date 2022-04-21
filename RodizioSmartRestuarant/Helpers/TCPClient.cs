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
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
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

        static int numRetries = 10;
        static int delaySeconds = 2;

        static bool processingRequest;
        private async static void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            var response = Encoding.UTF8.GetString(e.Data);

            //Update UI after network change
            if (response.Contains("REFRESH"))
            {
                for (int i = 0; i < numRetries; i++)
                {
                    if (!processingRequest)
                    {
                        Refresh_UI();
                        return;
                    }

                    await Task.Delay(delaySeconds * 1000);
                }
            }

            //var x = e.Data.FromByteArray<List<object>>();

            //Introduced retries to reduce crashes
            string receivedData = response;

            for (int i = 0; i < numRetries; i++)
            {
                if (!processingRequest || receivedData[0] != '[')
                {
                    if (receivedData[0] == '[')
                        receivedData = receivedData.Remove(0, 1);

                    DataReceived(receivedData);//There is a data limit for every packet once exceeded is sent in another packet
                    processingRequest = true;
                    break;
                }

                await Task.Delay(delaySeconds * 1000);
            }            
        }
        private static void Action()
        {
            Refresh_Action();
            DataReceived_Action();
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
                elapsedTime_2++;

            if (elapsedTime_2 > 1)
            {
                startCounting_2 = false;
                elapsedTime_2 = 0;

                Reconnect();
            }
        }
        private static float elapsedTime_2 = 0;
        private static bool startCounting_2 = false;

        private static void DataReceived_Action()
        {
            if (startCounting_1)
                elapsedTime_1++;

            if (elapsedTime_1 > 1)
            {
                startCounting_1 = false;
                elapsedTime_1 = 0;

                awaitresponse = (Convert.FromBase64String(receivedData)).FromByteArray<List<object>>();
                receivedData = "";
                processingRequest = false;
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

                WindowManager.Instance.UpdateAllOrderViews_Offline();
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
