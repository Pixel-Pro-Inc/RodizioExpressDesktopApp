using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Helpers
{
    public class SerializedObjectManager
    {
        string[] paths = { "Order", "Menu", "Account", "Branch"};
        string savePath(string dir)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RodizioData/" + dir);
        }
        public void DeleteAllData()
        {
            for (int i = 0; i < paths.Length; i++)
            {
                File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RodizioData/" + paths[i]));
            }           
        }

        public void SaveData(object serializedData, string dir)
        {
            List<object> objects = RetrieveData(dir) != null? (List<object>)RetrieveData(dir): new List<object>();

            objects.Add(serializedData);

            var save = objects;

            var binaryFormatter = new BinaryFormatter();
            using (var fileStream = File.Create(savePath(dir)))
            {
                binaryFormatter.Serialize(fileStream, save);
            }
        }
        public void EditData(OrderItem serializedData, string dir)
        {
            List<OrderItem> objects = RetrieveData(dir) != null ? (List<OrderItem>)RetrieveData(dir) : new List<OrderItem>();

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].OrderNumber == serializedData.OrderNumber)
                    objects[i] = serializedData;
            }

            var save = objects;

            var binaryFormatter = new BinaryFormatter();
            using (var fileStream = File.Create(savePath(dir)))
            {
                binaryFormatter.Serialize(fileStream, save);
            }
        }
        public object RetrieveData(string dir)
        {
            object load = null;

            if (File.Exists(savePath(dir)))
            {
                var binaryFormatter = new BinaryFormatter();
                using (var fileStream = File.Open(savePath(dir), FileMode.Open))
                {
                    load = (object)binaryFormatter.Deserialize(fileStream);

                    return load;
                }
            }

            return load;
        }
    }
}
