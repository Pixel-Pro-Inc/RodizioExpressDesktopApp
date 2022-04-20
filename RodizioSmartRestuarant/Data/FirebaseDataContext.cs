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
using RodizioSmartRestuarant.Extensions;

namespace RodizioSmartRestuarant.Data
{
    public class FirebaseDataContext:OfflineDataHelpers
    {
        public static FirebaseDataContext Instance { get; set; }

        bool startedSyncing = false;

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "Bjpp5DtGhoP1IllH6CbcD47SNMTgPU2S91EqWNwl",
            BasePath = "https://rodizoapp-default-rtdb.firebaseio.com/"
        };

        IFirebaseClient client;

        public ConnectionChecker connectionChecker = new ConnectionChecker();

        string branchId = "";

        public FirebaseDataContext()
        {
            Instance = this;

            StartFunction();
        }

        async void StartFunction()
        {
            client = new FireSharp.FirebaseClient(config);
            branchId = "/" + BranchSettings.Instance.branchId;

            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            dispatcherTimer.Start();            

            var response = await GetData_Online("Branch");

            for (int i = 0; i < response.Count; i++)
            {
                var item = response[i];
                if (item.GetType() == typeof(JObject))
                {
                    Branch branch = JsonConvert.DeserializeObject<Branch>(((JObject)item).ToString());
                    if(branch.Id == BranchSettings.Instance.branchId)
                        BranchSettings.Instance.branch = branch;
                }
                if (item.GetType() == typeof(Branch))//Local Storage
                {
                    if(((Branch)item).Id == BranchSettings.Instance.branchId)
                        BranchSettings.Instance.branch = (Branch)item;
                }
            }

            await SetLastActive();
        }

        public void SetBranchId()
        {
            branchId = "/" + BranchSettings.Instance.branchId;
        }

        public async Task UpdateLocalMenu() 
        {
            if (branchId == "/")
                return;

            //Delete Local Menu
            new SerializedObjectManager().DeleteMenu();

            List<MenuItem> onlineMenu = new List<MenuItem>();

            var list = await GetData1("Menu" + branchId);

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];

                if (item != null)
                {
                    MenuItem menuItem = JsonConvert.DeserializeObject<MenuItem>(((JObject)item).ToString());

                    onlineMenu.Add(menuItem);
                }
            }

            List<IDictionary<string, object>> values = new List<IDictionary<string, object>>();

            values.Clear();

            foreach (var item in onlineMenu)
            {
                values.Add(item.AsDictionary());
            }

            new SerializedObjectManager().SaveData(values, Directories.Menu);

            OfflineDataContext.LocalDataChange();
        }

        float elapsedTime = 0;
        bool startCounting = false;

        float elapsedTimeLastActive = 0;
        private async void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (startCounting)
                elapsedTime++;

            if(elapsedTime > 5)
            {
                startCounting = false;
                elapsedTime = 0;

                if (LocalStorage.Instance.networkIdentity.isServer && !startedSyncing)
                    WindowManager.Instance.UpdateAllOrderViews();
            }

            elapsedTimeLastActive++;

            if(elapsedTimeLastActive >= 1800)
            {
                await SetLastActive();                
            }

            if (DateTime.Now.Hour != 23)
                return;

            if (DateTime.Now.Minute < 45)
                return;

            if (!startedSyncing)
                WindowManager.Instance.CloseAllAndOpen(new SyncOrdersToDB());

            startedSyncing = true;
        }

        #region Primarily Run Offline
        public async Task StoreData(string fullPath, object data)
        {
            await OfflineStoreData(fullPath, data);
        }

        public async Task<List<object>> GetData(string fullPath)
        {
            return await OfflineGetData(fullPath);
        }
        #endregion

        public async Task StoreData_Online(string fullPath, object data)
        {
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();

                client = new FireSharp.FirebaseClient(config);

                var response = await client.SetAsync(fullPath, data);//Add Id of data  

                if (fullPath.ToLower().Contains("menu"))
                    await UpdateLocalMenu();

                return;
            }

            await OfflineStoreData(fullPath, data);
        }

        public async Task<bool> StoreData_Online_EndOfDaySync(string fullPath, object data)
        {
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();

                client = new FireSharp.FirebaseClient(config);

                var response = await client.SetAsync(fullPath, data);//Add Id of data  

                if (fullPath.ToLower().Contains("menu"))
                    await UpdateLocalMenu();

                return true;
            }

            return false;//Failure to submit data
        }

        public async Task<List<object>> GetData_Online(string fullPath)
        {
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();

                List<object> objects = new List<object>();

                client = new FireSharp.FirebaseClient(config);

                FirebaseResponse response = await client.GetAsync(fullPath);

                dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

                if (fullPath.ToLower().Contains("menu"))
                    await UpdateLocalMenu();

                if (data != null)
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

                return objects;
            }

            return await OfflineGetData(fullPath);
        }        

        public async Task EditData_Online(string fullPath, object data)
        {
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();

                client = new FireSharp.FirebaseClient(config);

                var response = await client.UpdateAsync(fullPath, data);

                if (fullPath.ToLower().Contains("menu"))
                    await UpdateLocalMenu();

                return;
            }
        }

        public async Task DeleteData(string fullPath)
        {
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();

                client = new FireSharp.FirebaseClient(config);

                var response = await client.DeleteAsync(fullPath);

                return;
            }
        }

        public async Task CompleteOrder(string fullPath)
        {
            //Moves order to completed directory
            if (branchId != "/")
            {
                string destination = "CompletedOrders" + branchId + "/" + fullPath.Substring(14, 15);
                await StoreData_Online(destination, await GetData(fullPath));

                await DeleteData(fullPath);
            }
        }

        public async Task CancelOrder(List<OrderItem> orderItems)
        {
            //Mark for deletion when back online
            foreach (var item in orderItems)
            {
                item.MarkedForDeletion = true;
                string branchId = BranchSettings.Instance.branchId;
                string fullPath = "Order/" + branchId + "/" + item.OrderNumber + "/" + item.Id.ToString();

                await StoreData(fullPath, item);//Remove from order view on all network devices
            }
        }

        public async Task CancelOrder_Offline(string fullPath)
        {
            //Moves order to completed directory
            if (branchId != "/")
            {
                string destination = "CancelledOrders" + branchId + "/" + fullPath.Substring(14, 15);
                var data = await GetData_Online(fullPath);

                List<object> result = new List<object>();

                for (int i = 0; i < data.Count; i++)
                {
                    OrderItem item = JsonConvert.DeserializeObject<OrderItem>(((JObject)data[i]).ToString());
                    item.User = LocalStorage.Instance.user.FullName();

                    result.Add(JToken.FromObject(item));
                }

                await StoreData_Online(destination, result);

                await DeleteData(fullPath);
            }
        }

        public async void GetDataChanging(string fullPath)
        {
            EventStreamResponse response = await client.OnAsync(fullPath,
                    (sender, args, context) => {
                        DataReceived();
                    },
                    (sender, args, context) => {
                        ;//DataReceived();
                    },
                    (sender, args, context) => {
                        ;//DataReceived();
                    });
        }

        void DataReceived()
        {
            startCounting = true;
            elapsedTime = 0;
        }

        #region Secondary Methods
        public async Task<List<object>> GetData1(string fullPath)
        {
            List<object> objects = new List<object>();

            client = new FireSharp.FirebaseClient(config);

            FirebaseResponse response = await client.GetAsync(fullPath);

            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            if (data != null)
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

            return objects;
        }
        #endregion

        async Task UpdateOfflineData()
        {
            if(branchId != "/" && !syncing)
            {
                //Clear hdd data
                new SerializedObjectManager().DeleteAllData();
                //Store new data
                #region Retrieve data
                List<List<OrderItem>> onlineOrders = new List<List<OrderItem>>();

                List<object> list = await GetData1("Order" + branchId);

                foreach (var item in list)
                {
                    List<OrderItem> data = JsonConvert.DeserializeObject<List<OrderItem>>(((JArray)item).ToString());

                    if (!data[0].Collected)
                        onlineOrders.Add(data); 
                }

                List<MenuItem> onlineMenu = new List<MenuItem>();

                list.Clear();
                list = await GetData1("Menu" + branchId);

                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];

                    if (item != null)
                    {
                        MenuItem menuItem = JsonConvert.DeserializeObject<MenuItem>(((JObject)item).ToString());

                        onlineMenu.Add(menuItem);
                    }
                }

                List<AppUser> onlineUsers = new List<AppUser>();

                list.Clear();
                list = await GetData1("Account");

                foreach (var item in list)
                {
                    var u = JsonConvert.DeserializeObject<AppUser>(((JObject)item).ToString());

                    onlineUsers.Add(u);
                }

                Branch onlineBranch = new Branch();

                list.Clear();
                list = await GetData1("Branch");

                foreach (var item in list)
                {
                    var b = JsonConvert.DeserializeObject<Branch>(((JObject)item).ToString());

                    if (b.Id == BranchSettings.Instance.branchId)
                        onlineBranch = b;
                }
                #endregion

                List<List<IDictionary<string, object>>> holder = new List<List<IDictionary<string, object>>>();

                List<IDictionary<string, object>> values = new List<IDictionary<string, object>>();

                foreach (var item in onlineOrders)
                {
                    holder.Add(new List<IDictionary<string, object>>());

                    foreach (var keyValuePair in item)
                    {
                        holder[holder.Count - 1].Add(keyValuePair.AsDictionary());
                    }
                }

                new SerializedObjectManager().SaveData(holder, Directories.Order);

                values.Clear();

                foreach (var item in onlineMenu)
                {
                    values.Add(item.AsDictionary());
                }

                new SerializedObjectManager().SaveData(values, Directories.Menu);

                values.Clear();

                foreach (var item in onlineUsers)
                {
                    values.Add(item.AsDictionary());
                }

                new SerializedObjectManager().SaveData(values, Directories.Account);

                new SerializedObjectManager().SaveData(onlineBranch.AsDictionary(), Directories.Branch);
            }
        }

        public void StoreUserDataLocally(List<AppUser> users)
        {
            List<IDictionary<string, object>> values = new List<IDictionary<string, object>>();

            foreach (var item in users)
            {
                values.Add(item.AsDictionary());
            }

            new SerializedObjectManager().SaveOverwriteData(values, Directories.Account);
        }

        public bool connected = false;
        bool lastStatus = false;

        public void ToggleConnectionStatus(bool status)
        {
            connected = status;

            //if current is online and last was offline => BackOnline

            if (connected && !lastStatus)
            {
                lastStatus = status;
                BackOnline();
            }

            lastStatus = status;
        }

        public async Task StoreDataBaseLocally_InitialStartUp()
        {
            await UpdateOfflineData();
        }

        async void BackOnline()
        {
            if(connectionChecker.notifCount != 0)
            {
                new Notification("Connectivity", "You're back online. We're syncing the changes you made while you were offline.");

                //Display syncing
                SyncData();
                //Hide syncing

                connectionChecker.notifCount = 0;
            }

            if (LocalStorage.Instance.networkIdentity.isServer)
                GetDataChanging("Order/" + BranchSettings.Instance.branchId);
        }
        bool syncing = false;
        void SyncData() //Apply offline changes to db
        {
            if(branchId != "/" && LocalStorage.Instance.networkIdentity.isServer)
            {
                syncing = true;

                //Update with online changes
                if (WindowManager.Instance != null)
                    WindowManager.Instance.UpdateAllOrderViews();

                syncing = false;
            }
        }

        //Including Completed
        public async Task SyncDataEndOfDay(List<List<OrderItem>> orders)
        {
            //Add new offline orders to database
            foreach (var order in orders)
            {
                for (int i = 0; i < order.Count; i++)
                {
                    order[i].Id = i;

                    if (order[i].Collected)
                    {
                        if (!(await StoreData_Online_EndOfDaySync("CompletedOrders" + branchId + "/" + order[i].OrderNumber + "/" + i, order[i])))
                            return;

                        continue;
                    }

                    if (!order[i].MarkedForDeletion)
                    {
                        if (!(await StoreData_Online_EndOfDaySync("Order" + branchId + "/" + order[i].OrderNumber + "/" + i, order[i])))
                            return;

                        continue;
                    }

                    if (!(await StoreData_Online_EndOfDaySync("CancelledOrders" + branchId + "/" + order[i].OrderNumber + "/" + i, order[i])))
                        return;
                }
            }

            //Delete Data
            //Restore Data

            await UpdateOfflineData();
        }

        async Task SetLastActive()
        {
            if (await connectionChecker.CheckConnection())
            {
                var list = await GetData1("Branch");

                Branch branch = null;

                foreach (var item in list)
                {
                    var b = JsonConvert.DeserializeObject<Branch>(((JObject)item).ToString());

                    if (b.Id == BranchSettings.Instance.branchId)
                    {
                        branch = b;
                    }
                }

                if (branchId != "/")
                {
                    branch.LastActive = DateTime.UtcNow;

                    BranchSettings.Instance.branch = branch;

                    await client.UpdateAsync("Branch" + branchId, branch);

                    elapsedTimeLastActive = 0;
                }
            }                        
        }

        public async Task<object> GetOfflineOrdersCompletedInclusive()
        {
            object offlineData = null;

            if (await OfflineDataContext.GetData(Directories.Order) is List<List<IDictionary<string, object>>>)
                offlineData = (List<List<IDictionary<string, object>>>)await OfflineDataContext.GetData(Directories.Order);


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

            return offlineOrders;
        }

        public void ResetLocalData(List<List<OrderItem>> orders)
        {
            //Clear hdd data
            new SerializedObjectManager().DeleteData();

            //Store data
            List<List<IDictionary<string, object>>> holder = new List<List<IDictionary<string, object>>>();

            foreach (var item in orders)
            {
                holder.Add(new List<IDictionary<string, object>>());

                foreach (var keyValuePair in item)
                {
                    holder[holder.Count - 1].Add(keyValuePair.AsDictionary());
                }
            }

            new SerializedObjectManager().SaveData(holder, Directories.Order);

            //We update only the network devices since this one has already all the uptodate view data
            OfflineDataContext.UpdateNetworkDevices();

            return;
        } 

        bool OrderItemChanged(List<OrderItem> itemsNew, List<OrderItem> itemsOld)
        {
            string newItem = itemsNew[0].OrderNumber;
            string oldItem = itemsOld[0].OrderNumber;

            if (itemsNew.Count == itemsOld.Count)
                for (int i = 0; i < itemsNew.Count; i++)
                {
                    if (itemsNew[i].Fufilled != itemsOld[i].Fufilled 
                        || itemsNew[i].Purchased != itemsOld[i].Purchased
                        || itemsNew[i].Collected != itemsOld[i].Collected)
                        return true;
                }

            return false;
        }
    }
}