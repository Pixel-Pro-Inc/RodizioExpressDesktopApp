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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Windows.Threading;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant.Data
{
    public class FirebaseDataContext //Detect Changes In Local Storage Needs to call Updates As Well
    {
        public static FirebaseDataContext Instance { get; set; }

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
            Instance = this;

            client = new FireSharp.FirebaseClient(config);
            branchId = "/" + BranchSettings.Instance.branchId;

            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            dispatcherTimer.Start();

            GetDataChanging("Order/" + BranchSettings.Instance.branchId);
        }

        float elapsedTime = 0;
        bool startCounting = false;
        UIChangeSource source;
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (startCounting)
                elapsedTime++;

            if(elapsedTime > 5)
            {
                startCounting = false;
                elapsedTime = 0;
                WindowManager.Instance.UpdateAllOrderViews(source);
            }
            //
        }

        public async Task StoreData(string fullPath, object data)
        {
            if (connectionChecker.CheckConnection())
            {
                await SetLastActive();

                //await SyncData();

                client = new FireSharp.FirebaseClient(config);

                var response = await client.SetAsync(fullPath, data);//Add Id of data             

                //await UpdateOfflineData();

                return;
            }
            
            new OfflineDataContext().StoreData(fullPath, data);
        }

        public async Task<List<object>> GetData(string fullPath)
        {
            if (connectionChecker.CheckConnection())
            {
                await SetLastActive();

                //await SyncData();

                List<object> objects = new List<object>();

                client = new FireSharp.FirebaseClient(config);

                FirebaseResponse response = await client.GetAsync(fullPath);

                dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

                if(data != null)
                {
                    if (data.GetType() != typeof(JArray))
                    {
                        var result = response.ResultAs<Dictionary<string, object>>();
                        foreach (var item in result)
                        {
                            objects.Add(item.Value);
                        }
                    }
                    else
                    {
                        List<object> output = JsonConvert.DeserializeObject<List<object>>(((JArray)data).ToString());
                        return output;
                    }
                }                

                //await UpdateOfflineData();

                return objects;
            }
            
            return new OfflineDataContext().GetData(fullPath); 
        }

        public async void GetDataChanging(string fullPath)
        {
            EventStreamResponse response = await client.OnAsync(fullPath,
                (sender, args, context) => {
                    source = UIChangeSource.Addition;
                    DataReceived();
                },
                (sender, args, context) => {
                    source = UIChangeSource.Edit;
                    DataReceived();
                },
                (sender, args, context) => {
                    source = UIChangeSource.Deletion;
                    DataReceived();
                });
        }

        void DataReceived()
        {
            startCounting = true;
            elapsedTime = 0;
        }

        int count = 0;
        public int count1 = 0;
        void StopListening()
        {
            if(count1 == 1)
            {
                if(elapsedTime >= 20)
                {
                    if (count == 0)
                    {
                        count = 1;
                        WindowManager.Instance.UpdateAllOrderViews(source);
                        Instance = new FirebaseDataContext();
                    }
                }                
            }

            elapsedTime = 0;
        }
        public async Task EditData(string fullPath, object data)
        {
            if (connectionChecker.CheckConnection())
            {
                await SetLastActive();

                //await SyncData();

                client = new FireSharp.FirebaseClient(config);

                var response = await client.UpdateAsync(fullPath, data);

                //await UpdateOfflineData();

                return;
            }

            new OfflineDataContext().EditData(fullPath, (OrderItem)data);
        }

        #region Secondary Methods
        public async void StoreData1(string fullPath, object data)
        {
            if (connectionChecker.CheckConnection())
            {

                client = new FireSharp.FirebaseClient(config);

                var response = await client.SetAsync(fullPath, data);

                return;
            }

            new OfflineDataContext().StoreData(fullPath, data);
        }

        public async Task<List<object>> GetData1(string fullPath)
        {
            if (connectionChecker.CheckConnection())
            {
                List<object> objects = new List<object>();

                client = new FireSharp.FirebaseClient(config);

                FirebaseResponse response = await client.GetAsync(fullPath);

                var result = response.ResultAs<Dictionary<string, object>>();
                foreach (var item in result)
                {
                    objects.Add(item.Value);
                }

                return objects;
            }

            return new OfflineDataContext().GetData(fullPath);
        }

        public async void EditData1(string fullPath, object data)
        {
            if (connectionChecker.CheckConnection())
            {

                client = new FireSharp.FirebaseClient(config);

                var response = await client.UpdateAsync(fullPath, data);

                return;
            }

            new OfflineDataContext().EditData(fullPath, (OrderItem)data);
        }
        #endregion

        async Task UpdateOfflineData()
        {
            //Clear hdd data
            new SerializedObjectManager().DeleteAllData();
            //Store new data
            #region Retrieve data
            List<OrderItem> onlineOrders = new List<OrderItem>();

            List<object> list = await GetData1("Order" + branchId);

            for (int i = 0; i < list.Count; i++)
            {
                onlineOrders.Add((OrderItem)list[i]);
            }

            List<MenuItem> onlineMenu = new List<MenuItem>();

            list.Clear();
            list = await GetData1("Menu" + branchId);

            for (int i = 0; i < list.Count; i++)
            {
                onlineMenu.Add((MenuItem)list[i]);
            }

            List<AppUser> onlineUsers = new List<AppUser>();

            list.Clear();
            list = await GetData1("Account" + branchId);

            for (int i = 0; i < list.Count; i++)
            {
                onlineUsers.Add((AppUser)list[i]);
            }

            Branch onlineBranch = new Branch();

            list.Clear();
            list = await GetData1("Branch" + branchId);

            for (int i = 0; i < list.Count; i++)
            {
                if (((Branch)list[i]).Id == BranchSettings.Instance.branchId)
                    onlineBranch = ((Branch)list[i]);
            }
            #endregion

            new SerializedObjectManager().SaveData(onlineOrders, "Order");
            new SerializedObjectManager().SaveData(onlineMenu, "Menu");
            new SerializedObjectManager().SaveData(onlineUsers, "Account");
            new SerializedObjectManager().SaveData(onlineBranch, "Branch");
        }

        async Task SyncData()
        {
            #region Retrieve data
            List<List<OrderItem>> offlineOrders = new SerializedObjectManager().RetrieveData("Order") == null? new List<List<OrderItem>>() : (List<List<OrderItem>>)new SerializedObjectManager().RetrieveData("Order");

            List<List<OrderItem>> onlineOrders = new List<List<OrderItem>>();

            List<object> list = await GetData1("Order" + branchId);

            foreach (var item in list)
            {
                List<OrderItem> data = JsonConvert.DeserializeObject<List<OrderItem>>(((JArray)item).ToString());

                onlineOrders.Add(data);
            }
            #endregion

            //Update Database of changes made while offline
            #region Sync
            foreach (var item in onlineOrders)//List<List<OrderItem>>
            {
                List<string> orderNums = new List<string>();
                foreach (var num in offlineOrders)
                {
                    foreach (var i in num)
                    {
                        orderNums.Add(i.OrderNumber);
                    }                    
                }

                foreach (var order in item)//List<OrderItem>
                {
                    if (orderNums.Contains(order.OrderNumber))
                    {
                        foreach (var x in offlineOrders)
                        {
                            foreach (var x2 in x)
                            {
                                if(x2.OrderNumber == order.OrderNumber)
                                {
                                    OrderItem orderItem = x2;

                                    //OrderItem
                                    order.Fufilled = orderItem.Fufilled;
                                    order.Purchased = orderItem.Purchased;
                                    order.Preparable = orderItem.Preparable;
                                    order.Collected = orderItem.Collected;
                                    order.WaitingForPayment = orderItem.WaitingForPayment;
                                }
                            }
                        }
                    }
                    else
                    {
                        onlineOrders.Add(item);
                    }
                }               
            }

            EditData1("Order" + branchId + "/" + onlineOrders[0][0].OrderNumber, onlineOrders);
            #endregion
        }

        async Task SetLastActive()
        {
            var list = await GetData1("Branch");

            Branch branch = null;

            foreach (var item in list)
            {
                var b = JsonConvert.DeserializeObject<Branch>(((JObject)item).ToString());

                if(b.Id == BranchSettings.Instance.branchId)
                {
                    branch = b;
                }
            }

            branch.LastActive = DateTime.UtcNow;

            BranchSettings.Instance.branch = branch;

            await client.UpdateAsync("Branch" + branchId, branch);
        }
    }
}