using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Data
{
    public class OfflineDataContext
    {
        //Data Stored Order, Menu, User
        public void StoreData(string path, object data)
        {
            new SerializedObjectManager().SaveData(data, path); 
        }
        public List<object> GetData(string path)
        {
            return (List<object>)new SerializedObjectManager().RetrieveData(path) != null? (List<object>)new SerializedObjectManager().RetrieveData(path): new List<object>();
        }
        public void EditData(string path, OrderItem data)
        {
            new SerializedObjectManager().EditData(data, path);
        }
    }
}
