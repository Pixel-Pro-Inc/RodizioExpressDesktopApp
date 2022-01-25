using RodizioSmartRestuarant.Extensions;
using SimpleTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Helpers
{
    public static class TCPClient
    {
        public static SimpleTcpClient client = null;

        public static void CreateClient(string ipPort)
        {
            client = new SimpleTcpClient("127.0.0.1" + ":2000");
            client.Events.DataReceived += Events_DataReceived;
        }

        public static bool ConnectToServer()
        {
            try
            {
                client.Connect();
            }
            catch(Exception ex)
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

                client.Send(requestObject.ToByteArray<RequestObject>());

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

            Byte[] bytes = Convert.FromBase64String(response);

            List<object> result = bytes.FromByteArray<List<object>>();

            awaitresponse = result;
        }
    }
}
