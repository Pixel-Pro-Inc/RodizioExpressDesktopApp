using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml.Serialization;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant.Helpers
{
    public class SerializedObjectManager
    {
        private const int NumberOfRetries = 6;
        private const int DelayOnRetry = 1000;

        public SerializedObjectManager()
        {
            if (TCPServer.Instance != null)
                TCPServer.Instance.localDataInUse = true;
        }
        ~SerializedObjectManager()
        {
            if (TCPServer.Instance != null)
                TCPServer.Instance.localDataInUse = false;
        }

        Directories[] paths = { Directories.Order, Directories.Menu, Directories.Account, Directories.Branch };//Exclusive of Printer, Settings So They Don't Get Deleted On UpdateOfflineData
        public string savePath(Directories dir) 
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string fullPath = Path.Combine(path + "\\RodizioExpressApplicationFiles\\", dir.ToString());

            return fullPath;
        }
        public void DeleteAllData()
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (File.Exists(savePath(paths[i]) + "/data.txt"))
                    File.Delete(savePath(paths[i]) + "/data.txt");
            }           
        }
        public void DeleteData()
        {
            if (File.Exists(savePath(Directories.Order) + "/data.txt"))
                File.Delete(savePath(Directories.Order) + "/data.txt");
        }
        public void DeleteMenu()
        {
            if (File.Exists(savePath(Directories.Menu) + "/data.txt"))
                File.Delete(savePath(Directories.Menu) + "/data.txt");
        }
        public void SaveData(object serializedData, Directories dir)
        {
            List<object> objects = RetrieveData(dir) != null? (List<object>)RetrieveData(dir): new List<object>();

            objects.Add(serializedData);

            var data = objects;

            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    // Do stuff with file
                    Directory.CreateDirectory(savePath(dir));

                    var binaryFormatter = new BinaryFormatter();
                    using (var fileStream = File.Create(savePath(dir) + "/data.txt"))
                    {
                        binaryFormatter.Serialize(fileStream, data);
                    }

                    File.SetAttributes(savePath(dir) + "/data.txt", FileAttributes.Normal);

                    break; // When done we can break loop
                }
                catch (IOException e) when (i <= NumberOfRetries)
                {
                    Thread.Sleep(DelayOnRetry);
                }
            }

            if (TCPServer.Instance != null)
                TCPServer.Instance.localDataInUse = false;
        }
        public void SaveOverwriteData(object serializedData, Directories dir)
        {
            List<object> objects = new List<object>();

            objects.Add(serializedData);

            var data = objects;

            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    // Do stuff with file
                    if (!File.Exists(savePath(dir)))
                        Directory.CreateDirectory(savePath(dir));

                    var binaryFormatter = new BinaryFormatter();
                    using (var fileStream = File.Create(savePath(dir) + "/data.txt"))
                    {
                        binaryFormatter.Serialize(fileStream, data);
                    }

                    File.SetAttributes(savePath(dir) + "/data.txt", FileAttributes.Normal);

                    break; // When done we can break loop
                }
                catch (IOException e) when (i <= NumberOfRetries)
                {
                    Thread.Sleep(DelayOnRetry);
                }
            }

            if (TCPServer.Instance != null)
                TCPServer.Instance.localDataInUse = false;
        }
        public async void DeleteOrder(List<OrderItem> serializedData, Directories dir)
        {
            //Retrieve locally stored data
            object offlineData = null;

            var input = await OfflineDataContext.GetData(Directories.Order);

            if (input is List<List<IDictionary<string, object>>>)
                offlineData = (List<List<IDictionary<string, object>>>)input;

            offlineData = offlineData == null ? new List<List<IDictionary<string, object>>>() : offlineData;

            //Covert IDictionary to OrderItem
            List<List<OrderItem>> offlineOrders = new List<List<OrderItem>>();

            foreach (var item in (List<List<IDictionary<string, object>>>)offlineData)
            {
                offlineOrders.Add(new List<OrderItem>());

                foreach (var itm in item)
                {
                    offlineOrders[offlineOrders.Count - 1].Add(itm.ToObject<OrderItem>());
                }
            }

            //Replace old data with new data
            int index = 0;
            foreach (var order in offlineOrders)
            {
                if (order[0].OrderNumber == serializedData[0].OrderNumber)
                    break;

                index++;
            }

            offlineOrders.RemoveAt(index);

            //Convert OrderItem to IDictionary for storage
            List<List<IDictionary<string, object>>> data = new List<List<IDictionary<string, object>>>();

            foreach (var item in offlineOrders)
            {
                data.Add(new List<IDictionary<string, object>>());

                foreach (var itm in item)
                {
                    data[data.Count - 1].Add(itm.AsDictionary());
                }
            }

            //Save with data overwrite
            SaveOverwriteData(data, Directories.Order);
        }
        public async void EditOrderData(OrderItem serializedData, Directories dir)
        {
            //Retrieve locally stored data
            object offlineData = null;

            var input = await OfflineDataContext.GetData(Directories.Order);

            if (input is List<List<IDictionary<string, object>>>)
                offlineData = (List<List<IDictionary<string, object>>>)input;

            offlineData = offlineData == null ? new List<List<IDictionary<string, object>>>() : offlineData;

            //Covert IDictionary to OrderItem
            List<List<OrderItem>> offlineOrders = new List<List<OrderItem>>();

            foreach (var item in (List<List<IDictionary<string, object>>>)offlineData)
            {
                offlineOrders.Add(new List<OrderItem>());

                foreach (var itm in item)
                {
                    offlineOrders[offlineOrders.Count - 1].Add(itm.ToObject<OrderItem>());
                }
            }

            //Replace old data with new data
            foreach (var order in offlineOrders)
            {
                if(order[0].OrderNumber == serializedData.OrderNumber)
                {
                    order[serializedData.Id] = serializedData;
                    break;
                }
            }

            //Convert OrderItem to IDictionary for storage
            List<List<IDictionary<string, object>>> data = new List<List<IDictionary<string, object>>>();

            foreach (var item in offlineOrders)
            {
                data.Add(new List<IDictionary<string, object>>());

                foreach (var itm in item)
                {
                    data[data.Count - 1].Add(itm.AsDictionary());
                }
            }

            //Save with data overwrite
            SaveOverwriteData(data, Directories.Order);
        }
        public string PrintReceiptPath(Directories dir)
        {
            if (File.Exists(savePath(dir) + "/receipt.pdf"))
                File.Delete(savePath(dir) + "/receipt.pdf");

            if (!File.Exists(savePath(dir)))
                Directory.CreateDirectory(savePath(dir));

            File.SetAttributes(savePath(dir), FileAttributes.Normal);

            if (TCPServer.Instance != null)
                TCPServer.Instance.localDataInUse = false;

            return savePath(dir);
        }
        public object RetrieveData(Directories dir)
        {
            object load = null;

            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    // Do stuff with file
                    if (File.Exists(savePath(dir) + "/data.txt"))
                    {
                        var binaryFormatter = new BinaryFormatter();
                        using (var fileStream = File.Open(savePath(dir) + "/data.txt", FileMode.Open))
                        {
                            load = (object)binaryFormatter.Deserialize(fileStream);

                            return load;
                        }
                    }
                    break; // When done we can break loop
                }
                catch (IOException e) when (i <= NumberOfRetries)
                {
                    Thread.Sleep(DelayOnRetry);
                }
            }

            if (TCPServer.Instance != null)
                TCPServer.Instance.localDataInUse = false;

            return load;
        }
    }
}