using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Helpers;
using System.Collections.Generic;
using static RodizioSmartRestuarant.Entities.Enums;
using RodizioSmartRestuarant.Extensions;
using System;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Data
{
    public static class OfflineDataContext
    {
        //Data Stored Order, Menu, User
        public async static void StoreData(Directories path, object data)
        {
            if (LocalStorage.Instance.networkIdentity.isServer)
            {
                new SerializedObjectManager().SaveData(data, path);
                return;
            }

            await TCPClient.SendRequest(data, path.ToString(), RequestObject.requestMethod.Store);
        }
        public async static void StoreDataOverwrite(Directories path, object data)
        {
            if (LocalStorage.Instance.networkIdentity.isServer)
            {
                new SerializedObjectManager().SaveOverwriteData(data, path);
                return;
            }

            await TCPClient.SendRequest(data, path.ToString(), RequestObject.requestMethod.Store);
        }
        public async static Task<object> GetData(Directories path)
        {
            if (LocalStorage.Instance.networkIdentity.isServer)
            {
                object data = (List<object>)new SerializedObjectManager().RetrieveData(path) != null ? ((List<object>)new SerializedObjectManager().RetrieveData(path))[0] : null;

                if (data == null)
                    return new List<object>();

                return data;
            }

            return await TCPClient.SendRequest(null, path.ToString(), RequestObject.requestMethod.Get);
        }
        public async static void EditOrderData(Directories path, OrderItem data)
        {
            if (LocalStorage.Instance.networkIdentity.isServer)
            {
                new SerializedObjectManager().EditOrderData(data, path);
                return;
            }

            await TCPClient.SendRequest(data, path.ToString(), RequestObject.requestMethod.Update);
        }
    }
}
