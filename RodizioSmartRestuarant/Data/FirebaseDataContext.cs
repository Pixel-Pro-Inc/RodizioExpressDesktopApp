using RodizioSmartRestuarant.Entities;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Data
{
    public class FirebaseDataContext
    {
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "KIxlMLOIsiqVrQmM0V7pppI1Ao67UPZv5jOdU0QJ",
            BasePath = "https://rodizoapp-default-rtdb.firebaseio.com/"
        };

        IFirebaseClient client;

        ConnectionChecker connectionChecker = new ConnectionChecker();

        string branchId = "";

        public FirebaseDataContext()
        {
            client = new FireSharp.FirebaseClient(config);
            branchId = "/" + BranchSettings.Instance.branchId;
        }

        public async void StoreData(string path, object data, string id)
        {
            if (connectionChecker.CheckConnection())
            {
                SetLastActive();

                await SyncData();

                client = new FireSharp.FirebaseClient(config);

                var response = await client.SetAsync(path + branchId + id, data);//Add Id of data             

                await UpdateOfflineData();

                return;
            }
            
            new OfflineDataContext().StoreData(path, data);
        }

        public async Task<List<object>> GetData(string path)
        {
            if (connectionChecker.CheckConnection())
            {
                SetLastActive();

                await SyncData();

                List<object> objects = new List<object>();

                client = new FireSharp.FirebaseClient(config);

                FirebaseResponse response = await client.GetAsync(path + branchId);

                var result = response.ResultAs<Dictionary<string, object>>();
                foreach (var item in result)
                {
                    objects.Add(item.Value);
                }

                await UpdateOfflineData();

                return objects;
            }
            
            return new OfflineDataContext().GetData(path); ;
        }

        public async void EditData(string path, object data, string id)
        {
            if (connectionChecker.CheckConnection())
            {
                SetLastActive();

                await SyncData();

                client = new FireSharp.FirebaseClient(config);

                var response = await client.UpdateAsync(path + branchId + id, data);

                await UpdateOfflineData();

                return;
            }

            new OfflineDataContext().EditData(path, (OrderItem)data);
        }

        async Task UpdateOfflineData()
        {
            //Clear hdd data
            new SerializedObjectManager().DeleteAllData();
            //Store new data
            #region Retrieve data
            List<OrderItem> onlineOrders = new List<OrderItem>();

            List<object> list = await GetData("Order" + branchId);

            for (int i = 0; i < list.Count; i++)
            {
                onlineOrders.Add((OrderItem)list[i]);
            }

            List<MenuItem> onlineMenu = new List<MenuItem>();

            list.Clear();
            list = await GetData("Menu" + branchId);

            for (int i = 0; i < list.Count; i++)
            {
                onlineMenu.Add((MenuItem)list[i]);
            }

            List<AppUser> onlineUsers = new List<AppUser>();

            list.Clear();
            list = await GetData("Account" + branchId);

            for (int i = 0; i < list.Count; i++)
            {
                onlineUsers.Add((AppUser)list[i]);
            }
            #endregion

            new SerializedObjectManager().SaveData(onlineOrders, "Order");
            new SerializedObjectManager().SaveData(onlineMenu, "Menu");
            new SerializedObjectManager().SaveData(onlineUsers, "Account");
        }

        async Task SyncData()
        {
            #region Retrieve data
            List<OrderItem> offlineOrders = (List<OrderItem>)new SerializedObjectManager().RetrieveData("Order");

            List<OrderItem> onlineOrders = new List<OrderItem>();

            List<object> list = await GetData("Order" + branchId);

            for (int i = 0; i < list.Count; i++)
            {
                onlineOrders.Add((OrderItem)list[i]);
            }
            #endregion

            //Update Database of changes made while offline
            #region Sync
            foreach (var item in onlineOrders)
            {
                List<string> orderNums = new List<string>();
                foreach (var num in offlineOrders)
                {
                    orderNums.Add(num.OrderNumber);
                }

                if (orderNums.Contains(item.OrderNumber))
                {
                    OrderItem orderItem = offlineOrders[orderNums.IndexOf(item.OrderNumber)];

                    item.Fufilled = orderItem.Fufilled;
                    item.Purchased = orderItem.Purchased; 
                    item.Preparable = orderItem.Preparable;
                    item.Collected = orderItem.Collected;
                    item.WaitingForPayment = orderItem.WaitingForPayment;
                }
            }

            EditData("Order" + branchId, onlineOrders, "");
            #endregion
        }

        void SetLastActive()
        {
            EditData("Branch/" + branchId + "/LastActive", DateTime.UtcNow, "");
        }
    }
}