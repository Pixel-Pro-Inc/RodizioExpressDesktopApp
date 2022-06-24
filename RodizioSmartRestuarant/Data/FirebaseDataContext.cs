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

        public bool startedSyncing = false;
        string branchId = "";

        // REFACTOR: Use environment variables here
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = ConnectionStringManager.GetConnectionString("FirebaseAuth"),
            BasePath = ConnectionStringManager.GetConnectionString("FireBaseBasePath")
        };
        IFirebaseClient client;

        public ConnectionChecker connectionChecker = new ConnectionChecker();

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

        #region Download

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

        // REFACTOR: This is too similiar to previous block, consider override or using base method or extract logic entirely
        public async Task<List<object>> GetData_Online(string fullPath)
        {
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();

                List<object> objects = new List<object>();

                // @Yewo: Why do you have this line when in the method that this is called, its already defined there
                client = new FireSharp.FirebaseClient(config);

                FirebaseResponse response = await client.GetAsync(fullPath);

                //This is the data collectd from the data base
                dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

                // logic for if the data being searched for is Menu related
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
            // If connection to online data or the online data itself doesn't come this just releases the offlineObjects
            return await OfflineGetData(fullPath);
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

        #endregion
        #region Update

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

            // @Yewo: Why clear an empty list?
            values.Clear();

            foreach (var item in onlineMenu)
            {
                values.Add(item.AsDictionary());
            }

            new SerializedObjectManager().SaveData(values, Directories.Menu);

            OfflineDataContext.LocalDataChange();
        }
        async Task UpdateOfflineData()
        {
            if (branchId != "/" && !syncing)
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

        // REFACTOR: This method is too similar to the one that has line 231 StoreData_Online(), consider using base method and overrides or simply extracting the logic
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
        public async void ResetLocalData(List<List<OrderItem>> orders)
        {
            //Makes sure only the server makes the syncing changes
            if (TCPServer.Instance == null)
                return;

            List<List<OrderItem>> orderItems = new List<List<OrderItem>>();

            //Offline include completed orders
            orderItems = (List<List<OrderItem>>)(await GetOfflineOrdersCompletedInclusive());

            foreach (var item in orderItems)
            {
                //Add Back Completed and Deleted Orders To Local HDD So They Can Be Sent BACK Up At Sync Time
                // @Yewo: Why are Deleted orders sent up? or is that a feature we want to have, for analysis?
                if (item[0].Collected || item[0].MarkedForDeletion)
                    orders.Add(item);
            }

            //Clear hdd data
            new SerializedObjectManager().DeleteData();

            //Store data
            List<List<IDictionary<string, object>>> holder = new List<List<IDictionary<string, object>>>();

            // REFACTOR: Hopefully if we factor this list<list< OrderItems> to just Order<OrderItems> by having Order inhert from IEnumerable, we can end this kind of hacking
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

                if (TCPServer.Instance != null)
                    await StoreData(fullPath, item);//Remove from order view on all network devices
            }

            if (TCPServer.Instance == null)
                await StoreData("Order/", orderItems);
        }
        public async Task CancelOrder_Offline(string fullPath)
        {
            //Moves order to Cancelled directory // UPDATE: I changed the comment where the word said completed to Cancelled directory
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

        #endregion
        #region Upload

        // REFACTOR: This amougst others are have the same name and similar purpose, we have to consider having overloads of a single method (that takes advantage of 'base' syntax)
        // It also makes sense that this and others like it be put in a interface

        float elapsedTime = 0;
        bool startCounting = false;

        float elapsedTimeLastActive = 0;
        private async void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (startCounting)
                elapsedTime++;

            if (elapsedTime > 1)
            {
                startCounting = false;
                elapsedTime = 0;

                if (LocalStorage.Instance.networkIdentity.isServer && !startedSyncing)
                    WindowManager.Instance.UpdateAllOrderViews();
            }

            elapsedTimeLastActive++;

            if (elapsedTimeLastActive >= 1800)
            {
                await SetLastActive();
            }

            if (DateTime.Now.Hour != 2)
                return;

            if (DateTime.Now.Minute < 45)
                return;

            if (!startedSyncing)
                WindowManager.Instance.CloseAllAndOpen(new SyncOrdersToDB());

            startedSyncing = true;
        }

        #endregion
        #region Store

        public async Task StoreData_Online(string fullPath, object data)
        {
            if (await connectionChecker.CheckConnection())
            {
                // REFACTOR: Consider having this method called within CheckConnection
                await SetLastActive();
                // @Yewo: Seriously, why do we have client set twice.
                client = new FireSharp.FirebaseClient(config);

                // TODO: Add Id of data 
                var response = await client.SetAsync(fullPath, data);

                if (fullPath.ToLower().Contains("menu"))
                    await UpdateLocalMenu();

                return;
            }

            await OfflineStoreData(fullPath, data);
        }

        // REFACTOR: This method is too similar to the previous block. Consider using override and base keyword or at least extract the method
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
        public void StoreUserDataLocally(List<AppUser> users)
        {
            List<IDictionary<string, object>> values = new List<IDictionary<string, object>>();

            foreach (var item in users)
            {
                values.Add(item.AsDictionary());
            }

            new SerializedObjectManager().SaveOverwriteData(values, Directories.Account);
        }
        public async Task StoreDataBaseLocally_InitialStartUp()=>await UpdateOfflineData();

        #endregion
        #region Syncing and connectivity


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

        // UPDATE: I removed the async key word on this method
        void BackOnline()
        {
            if (connectionChecker.notifCount != 0)
            {
                new Notification("Connectivity", "You're back online. We're syncing the changes you made while you were offline.");

                // TODO: Show and hide the syncing
                //Display syncing
                SyncData();
                //Hide syncing

                connectionChecker.notifCount = 0;
            }

            if (LocalStorage.Instance.networkIdentity.isServer)
                GetDataChanging("Order/" + BranchSettings.Instance.branchId);
        }

        // UPDATE: I changed the bool to be private, its not used any where else and is relativly important so I don't want to expose it all willinilly
        private bool syncing = false;

        //Apply offline changes to db
        void SyncData()
        {
            if (branchId != "/" && LocalStorage.Instance.networkIdentity.isServer)
            {
                // REFACTOR: look down Yewo
                // @Yewo: Why do you flip the bool when there is noone listening to it?
                syncing = true;

                //Updates with online changes
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

            // TODO: Delete data and then restore data
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

        #endregion
        #region Primarily Run Offline
        public async Task StoreData(string fullPath, object data)=> await OfflineStoreData(fullPath, data);
        public async Task<List<object>> GetData(string fullPath)=> await OfflineGetData(fullPath);

        #endregion
        #region Secondary Methods

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
        public void SetBranchId() => branchId = "/" + BranchSettings.Instance.branchId;

        #endregion

    }
}