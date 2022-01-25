using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant.Helpers
{
    public class SerializedObjectManager
    {
        Directories[] paths = { Directories.Order, Directories.Menu, Directories.Account, Directories.Branch };
        string savePath(Directories dir) { return Path.Combine(dir.ToString()); }
        public void DeleteAllData()
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (File.Exists(savePath(paths[i]) + "/data.txt"))
                    File.Delete(savePath(paths[i]) + "/data.txt");
            }           
        }
        public void SaveData(object serializedData, Directories dir)
        {
            List<object> objects = RetrieveData(dir) != null? (List<object>)RetrieveData(dir): new List<object>();

            objects.Add(serializedData);

            var data = objects;

            Directory.CreateDirectory(savePath(dir));            

            var binaryFormatter = new BinaryFormatter();
            using (var fileStream = File.Create(savePath(dir) + "/data.txt"))
            {
                binaryFormatter.Serialize(fileStream, data);
            }

            File.SetAttributes(savePath(dir) + "/data.txt", FileAttributes.Normal);
        }
        public void SaveOverwriteData(object serializedData, Directories dir)
        {
            List<object> objects = new List<object>();

            objects.Add(serializedData);

            var data = objects;

            if (!File.Exists(savePath(dir)))
                Directory.CreateDirectory(savePath(dir));

            var binaryFormatter = new BinaryFormatter();
            using (var fileStream = File.Create(savePath(dir) + "/data.txt"))
            {
                binaryFormatter.Serialize(fileStream, data);
            }

            File.SetAttributes(savePath(dir) + "/data.txt", FileAttributes.Normal);
        }
        public async void EditOrderData(OrderItem serializedData, Directories dir)
        {
            object offlineData = null;

            var input = await OfflineDataContext.GetData(Directories.Order);

            if (input is List<List<IDictionary<string, object>>>)
                offlineData = (List<List<IDictionary<string, object>>>)input;

            offlineData = offlineData == null ? new List<List<IDictionary<string, object>>>() : offlineData;

            List<List<OrderItem>> offlineOrders = new List<List<OrderItem>>();

            foreach (var item in (List<List<IDictionary<string, object>>>)offlineData)
            {
                offlineOrders.Add(new List<OrderItem>());

                foreach (var itm in item)
                {
                    offlineOrders[offlineOrders.Count - 1].Add(itm.ToObject<OrderItem>());
                }
            }

            foreach (var order in offlineOrders)
            {
                if(order[0].OrderNumber == serializedData.OrderNumber)
                {
                    order[serializedData.Id] = serializedData;
                    break;
                }
            }

            List<List<IDictionary<string, object>>> data = new List<List<IDictionary<string, object>>>();

            foreach (var item in offlineOrders)
            {
                data.Add(new List<IDictionary<string, object>>());

                foreach (var itm in item)
                {
                    data[data.Count - 1].Add(itm.AsDictionary());
                }
            }

            SaveOverwriteData(data, Directories.Order);
        }
        public object RetrieveData(Directories dir)
        {
            object load = null;

            if (File.Exists(savePath(dir) + "/data.txt"))
            {
                var binaryFormatter = new BinaryFormatter();
                using (var fileStream = File.Open(savePath(dir) + "/data.txt", FileMode.Open))
                {
                    load = (object)binaryFormatter.Deserialize(fileStream);

                    return load;
                }
            }

            return load;
        }
    }
}