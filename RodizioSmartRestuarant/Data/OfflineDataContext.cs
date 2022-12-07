using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Helpers;
using System.Collections.Generic;
using static RodizioSmartRestuarant.Entities.Enums;
using RodizioSmartRestuarant.Extensions;
using System;
using System.Threading.Tasks;
using RodizioSmartRestuarant.Core.Entities.Aggregates;

namespace RodizioSmartRestuarant.Data
{
    public static class OfflineDataContext
    {
        //Data Stored of Order, Menu, User
        #region Download

        public async static Task<object> GetData(Directories path)
        {
            if (LocalStorage.Instance.networkIdentity.isServer)
            {
                object data = (List<object>)new SerializedObjectManager().RetrieveData(path) != null ? ((List<object>)new SerializedObjectManager().RetrieveData(path))[0] : null;

                if (data == null)
                    return new List<object>();

                return data;
            }

            while (TCPClient.processingRequest)
            {
                await Task.Delay(25);
            }

            var data_1 = await TCPClient.SendRequest(null, path.ToString(), RequestObject.requestMethod.Get);

            return data_1;
        }

        #endregion
        #region Update

        // REFACTOR: Similiar method to Storedata(), consider using base method, override or extract logic and inject functions into it
        public async static void EditOrderData(Directories path, OrderItem data)
        {
            if (LocalStorage.Instance.networkIdentity.isServer)
            {
                new SerializedObjectManager().EditOrderData(data, path);
                LocalDataChange();
                return;
            }

            while (TCPClient.processingRequest)
            {
                await Task.Delay(25);
            }

            await TCPClient.SendRequest(data, path.ToString(), RequestObject.requestMethod.Update);
        }

        // REFACTOR: Similiar method to Storedata(), consider using base method, override or extract logic and inject functions into it
        public async static void DeleteOrder(Directories path, Order data)
        {
            if (LocalStorage.Instance.networkIdentity.isServer)
            {
                new SerializedObjectManager().DeleteOrder(data, path);
                LocalDataChange();
                return;
            }

            while (TCPClient.processingRequest)
            {
                await Task.Delay(25);
            }

            await TCPClient.SendRequest(data, path.ToString(), RequestObject.requestMethod.Delete);
        }
        public static void LocalDataChange()
        {
            WindowManager.Instance.UpdateAllOrderViews_Offline();
            UpdateNetworkDevices();
        }
        public static void UpdateNetworkDevices()
        {
            if (TCPServer.Instance != null)
                TCPServer.Instance.UpdateAllNetworkDevicesUI();
        }

        #endregion
        #region Store

        public async static void StoreData(Directories path, object data)
        {
            if (LocalStorage.Instance.networkIdentity.isServer)
            {
                new SerializedObjectManager().SaveData(data, path);
                LocalDataChange();
                return;
            }

            while (TCPClient.processingRequest)
            {
                await Task.Delay(25);
            }

            await TCPClient.SendRequest(data, path.ToString(), RequestObject.requestMethod.Store);
        }
        // REFACTOR: Above too similiar, consider base method, override or extract method
        public async static void StoreDataOverwrite(Directories path, object data)
        {
            if (LocalStorage.Instance.networkIdentity.isServer)
            {
                new SerializedObjectManager().SaveOverwriteData(data, path);
                LocalDataChange();
                return;
            }

            while (TCPClient.processingRequest)
            {
                await Task.Delay(25);
            }

            await TCPClient.SendRequest(data, path.ToString(), RequestObject.requestMethod.Store);
        }

        #endregion

    }
}
