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
        IOfflineDataService _offlineDataServices;
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

            List<Branch> branches = await _firebaseServices.GetData<Branch>("Branch");

            foreach (var branch in branches)
            {
                if (branch.Id == BranchSettings.Instance.branchId)
                    BranchSettings.Instance.branch = branch;
            }
            SetBranchId();

            await SetLastActive();
        }
        public void SetBranchId() => branchId = "/" + BranchSettings.Instance.branchId;

        public async Task CancelOrder(Order orderItems)
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
        public async Task CancelOrder_Offline(string orderInvoice)
        {
            //Moves order to Cancelled directory // UPDATE: I changed the comment where the word said completed to Cancelled directory
            if (branchId != "/")
            {
                string destination = "CancelledOrders" + branchId + "/" + orderInvoice.Substring(14, 15);
                List<Order> Orders = await _firebaseServices.GetDataArray<Order,OrderItem>(orderInvoice);

                foreach (var order in Orders)
                {
                    order[0].User = LocalStorage.Instance.user.FullName();
                }

                await StoreData_Online(destination, Orders);

                await DeleteData(orderInvoice);
            }
        }
        bool OrderItemChanged(Order itemsNew, Order itemsOld)
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

            // REFACTOR: Here is a good place caching can come in handy
            //Collects new data
            #region Retrieve data
            List<Order> onlineOrders = await _firebaseServices.GetDataArray<Order, OrderItem>("Order" + branchId);
            List<Order> uncollectedOrders = new List<Order>();

            foreach (var order in onlineOrders)
            {
                if (!order[0].Collected)
                    uncollectedOrders.Add(order);
            }

            Menu onlineMenu = (Menu)await _firebaseServices.GetData<MenuItem>("Menu" + branchId);

            List<AppUser> onlineUsers = await _firebaseServices.GetData<AppUser>("Account");

            List<Branch> onlineBranches = await _firebaseServices.GetData<Branch>("Branch");
            Branch OurBranch = new Branch();
            foreach (var b in onlineBranches)
            {
                if (b.Id == BranchSettings.Instance.branchId)
                    OurBranch = b;
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

            new SerializedObjectManager().SaveData(onlineBranches.AsDictionary(), Directories.Branch);

        }

        public async Task<List<T>> GetData<T>(string fullPath) where T: BaseEntity, new()
        {
            // If connection to online data or the online data itself doesn't come this just releases the offlineObjects
            if (!await connectionChecker.CheckConnection()) return await _offlineDataServices.GetOfflineData<T>(fullPath);

            await SetLastActive();

            return await _firebaseServices.GetData<T>(fullPath);

        }

        public async Task StoreDataOffline(string fullPath, object data) => await _offlineDataServices.OfflineStoreData(fullPath, data);
       

        public async Task<object> GetOfflineOrdersCompletedInclusive()
        {
            object offlineData = null;

            // REFACTOR: have type checking in the dataservice
            if (await OfflineDataContext.GetData(Directories.Order) is List<List<IDictionary<string, object>>>)
                offlineData = (List<List<IDictionary<string, object>>>)await OfflineDataContext.GetData(Directories.Order);


            offlineData = offlineData == null ? new List<List<IDictionary<string, object>>>() : offlineData;

            List<Order> offlineOrders = new List<Order>();

            foreach (var item in (List<List<IDictionary<string, object>>>)offlineData)
            {
                offlineOrders.Add(new Order());

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
        // REFACTOR: This method is too similar to the one that has line 231 StoreData_Online(), consider using base method and overrides or simply extracting the logic
        public async Task EditData_Online(string fullPath, object data)
        {
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();

                _firebaseServices.StoreData(fullPath, data);

                // FIXME: The menu service needs to reference this, not firebase
                if (fullPath.ToLower().Contains("menu"))
                    //await UpdateLocalStorage();

                    return;
            }
        }

        public async void ResetLocalData(List<Order> orders)
        {
            //Makes sure only the server makes the syncing changes
            if (TCPServer.Instance == null)
                return;

            List<Order> orderItems = new List<Order>();

            //Offline include completed orders
            orderItems = (List<Order>)(await GetOfflineOrdersCompletedInclusive());

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
            if (!await connectionChecker.CheckConnection()) await _offlineDataServices.OfflineStoreData(fullPath, data);
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
        public async Task SyncDataEndOfDay(List<Order> orders)
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
                OnDataChanging("Order/" + BranchSettings.Instance.branchId);
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

        public void DataReceived()
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
                await StoreData_Online(destination, _offlineDataServices.GetOfflineData<T>(fullPath));

                await DeleteData(fullPath);
            }
        }


    }
}
