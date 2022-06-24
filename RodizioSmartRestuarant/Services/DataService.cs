using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Extensions;
using RodizioSmartRestuarant.Helpers;
using RodizioSmartRestuarant.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant.Services
{
    /// <summary>
    /// This will coordinate all the offline and online data so its seemless and used by all the services and components that should access it
    /// <para> I don't really expect to fuse <see cref="IFirebaseServices"/> and <see cref="OfflineDataService"/> because them being by themselves makes sense</para>
    /// </summary>
    public class DataService:_BaseService, IDataService
    {
        public ConnectionChecker connectionChecker = new ConnectionChecker();
        IFirebaseServices _firebaseServices;
        public bool startedSyncing = false;
        // UPDATE: I changed the bool to be private, its not used any where else and is relativly important so I don't want to expose it all willinilly
        private bool syncing = false;

        string branchId;

        public DataService()
        {

            StartFunction();
        }
        async void StartFunction()
        {

            branchId = "/" + BranchSettings.Instance.branchId;

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            dispatcherTimer.Start();

            // REFACTOR: This should be in firebaseService
            var response = await GetData_Online("Branch");

            for (int i = 0; i < response.Count; i++)
            {
                var item = response[i];
                if (item.GetType() == typeof(JObject))
                {
                    Branch branch = JsonConvert.DeserializeObject<Branch>(((JObject)item).ToString());
                    if (branch.Id == BranchSettings.Instance.branchId)
                        BranchSettings.Instance.branch = branch;
                }
                if (item.GetType() == typeof(Branch))//Local Storage
                {
                    if (((Branch)item).Id == BranchSettings.Instance.branchId)
                        BranchSettings.Instance.branch = (Branch)item;
                }
            }

            await SetLastActive();
        }

        public void SetBranchId() => branchId = "/" + BranchSettings.Instance.branchId;

        public async Task CancelOrder(List<OrderItem> orderItems)
        {
            //Mark for deletion when back online
            foreach (var item in orderItems)
            {
                item.MarkedForDeletion = true;
                string branchId = BranchSettings.Instance.branchId;
                string fullPath = "Order/" + branchId + "/" + item.OrderNumber + "/" + item.Id.ToString();

                if (TCPServer.Instance != null)
                    await StoreDataOffline(fullPath, item);//Remove from order view on all network devices
            }

            if (TCPServer.Instance == null)
                await StoreDataOffline("Order/", orderItems);
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

        public void UpdateLocalStorage<T>(BaseAggregates<T> Aggregate, Directories directory)
        {
            if (BranchSettings.Instance.branchId == "/")
                return;

            //Delete Local Menu
            new SerializedObjectManager().DeleteMenu();

            List<IDictionary<string, object>> values = new List<IDictionary<string, object>>();

            // To make sure there is nothing. Just like setting a variable to null to throw empty if something went wrong
            // REFACTOR: Consider just having a GuardAgainst null or something. Unless that's not needed either.
            values.Clear();

            foreach (var item in Aggregate)
            {
                values.Add(item.AsDictionary());
            }

            new SerializedObjectManager().SaveData(values, directory);

            OfflineDataContext.LocalDataChange();
        }
        // REFACTOR: Please, for the love of God , Fix this
        // FIXME: Its just so all over th eplace
        async Task UpdateOfflineData()
        {
            // if its syncing then it shouldn't updateOfflineData, it also shouldn't update when the, Okay this one I'm not sure, Yewo's convoluted code
            if (branchId == "/" && syncing) return;
            //Clear hdd data
            new SerializedObjectManager().DeleteAllData();
            //Store new data
            #region Retrieve data
            List<List<OrderItem>> onlineOrders = new List<List<OrderItem>>();

            List<object> list = await GetData("Order" + branchId);

            foreach (var item in list)
            {
                List<OrderItem> data = JsonConvert.DeserializeObject<List<OrderItem>>(((JArray)item).ToString());

                if (!data[0].Collected)
                    onlineOrders.Add(data);
            }

            Entities.Aggregates.Menu onlineMenu = new Entities.Aggregates.Menu();

            list.Clear();
            list = await GetData("Menu" + branchId);

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
            list = await GetData("Account");

            foreach (var item in list)
            {
                var u = JsonConvert.DeserializeObject<AppUser>(((JObject)item).ToString());

                onlineUsers.Add(u);
            }

            Branch onlineBranch = new Branch();

            list.Clear();
            list = await GetData("Branch");

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

        public async Task<List<object>> GetData_Online(string fullPath)
        {
            // If connection to online data or the online data itself doesn't come this just releases the offlineObjects
            if (!await connectionChecker.CheckConnection()) return await OfflineGetData(fullPath);

            await SetLastActive();

            return await _firebaseServices.GetData(fullPath);

        }

        public async Task StoreDataOffline(string fullPath, object data) => await OfflineStoreData(fullPath, data);
        public async Task<List<object>> GetOfflineData(string fullPath) => await OfflineGetData(fullPath);

        public async Task<object> GetOfflineOrdersCompletedInclusive()
        {
            object offlineData = null;

            // REFACTOR: have type checking in the dataservice
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

        public async Task DeleteData(string fullPath)
        {
            // REFACTOR: Consider throwing a noNetwork error here
            if (!await connectionChecker.CheckConnection()) return;
            await SetLastActive();
            _firebaseServices.DeleteData(fullPath);
        }
        // TODO: Put in DataService
        // REFACTOR: This method is too similar to the one that has line 231 StoreData_Online(), consider using base method and overrides or simply extracting the logic
        public async Task EditData_Online(string fullPath, object data)
        {
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();

                _firebaseServices.StoreData(fullPath, data);

                // FIXME: It doesn't need to know what the data is we just need to pass in the aggregate version
                if (fullPath.ToLower().Contains("menu"))
                    //await UpdateLocalStorage();

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

        async Task SetLastActive()
        {
            if (await connectionChecker.CheckConnection())
            {
                var list = await _firebaseServices.GetData<Branch>("Branch");

                Branch branch = null;

                foreach (var b in list)
                {
                    if (b.Id == BranchSettings.Instance.branchId)
                    {
                        branch = b;
                    }
                }

                if (BranchSettings.Instance.branchId != "/")
                {
                    branch.LastActive = DateTime.UtcNow;

                    BranchSettings.Instance.branch = branch;

                    _firebaseServices.StoreData("Branch" + BranchSettings.Instance.branchId, branch);

                    elapsedTimeLastActive = 0;
                }
            }
        }
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
        public async Task StoreData_Online(string fullPath, object data)
        {
            if (!await connectionChecker.CheckConnection()) await OfflineStoreData(fullPath, data);
            await SetLastActive();

            _firebaseServices.StoreData(fullPath, data);

            // TODO: Similar work
            //if (fullPath.ToLower().Contains("menu"))
            //await UpdateLocalMenu();

        }
        public async Task<bool> StoreData_Online_EndOfDaySync(string fullPath, object data)
        {
            // If there is no connection there is no way it can sync so should return false
            if (!await connectionChecker.CheckConnection()) return false;
            await StoreData_Online(fullPath, data);
            return true;
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
        public async Task StoreDataBaseLocally_InitialStartUp() => await UpdateOfflineData();
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

        void DataReceived()
        {
            startCounting = true;
            elapsedTime = 0;
        }

        public async Task CompleteOrder(string fullPath)
        {
            //Moves order to completed directory
            if (branchId != "/")
            {
                string destination = "CompletedOrders" + branchId + "/" + fullPath.Substring(14, 15);
                await StoreData_Online(destination, await GetOfflineData(fullPath));

                await DeleteData(fullPath);
            }
        }


    }
}
