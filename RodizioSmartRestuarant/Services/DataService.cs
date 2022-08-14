using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Exceptions;
using RodizioSmartRestuarant.Extensions;
using RodizioSmartRestuarant.Helpers;
using RodizioSmartRestuarant.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
        IOrderService _orderService;
        public bool startedSyncing { get; set; } = false;
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

        public async Task<object> GetData(string fullPath)
        {
            Directories dir = GetDirectory(fullPath);
            switch (dir)
            {
                case Directories.Order:
                    return await GetDataArray<Order, OrderItem>(fullPath);
                case Directories.Menu:
                    return await GetDataArray<Menu, MenuItem>(fullPath);
                case Directories.Account:
                    return await GetEntity<AppUser>(fullPath);
                case Directories.Branch: 
                    return await GetEntity<Branch>(fullPath);
                case Directories.BranchId:
                    List<string> branchids = new List<string>();
                    List<Branch> branches= await GetEntity<Branch>(fullPath);
                    foreach (var branch in branches)
                    {
                        branchids.Add(branch.Id);
                    }
                    return branchids;
                case Directories.PrinterName:
                    throw new NotImplementedException("This type of data isn't called from this method or at all. You might be using the method wrong, consider using something in the TCPServer");   
                case Directories.Settings:
                    throw new NotImplementedException("This type of data isn't called from this method or at all. You might be using the method wrong, consider using something in the TCPServer");
                case Directories.NetworkInterface:
                    throw new NotImplementedException("This type of data isn't called from this method or at all. You might be using the method wrong, consider using something in the TCPServer");
                case Directories.Print:
                    throw new NotImplementedException("This type of data isn't called from this method or at all. You might be using the method wrong, consider using something in the TCPServer");
                case Directories.TCPServer:
                    throw new NotImplementedException("This type of data isn't called from this method or at all. You might be using the method wrong, consider using something in the TCPServer");
                case Directories.TCPServerIP:
                    throw new NotImplementedException("This type of data isn't called from this method or at all. You might be using the method wrong, consider using something in the TCPServer");
                case Directories.Error:
                    throw new NotImplementedException("This type of data isn't called from this method or at all. You might be using the method wrong, consider using something in the TCPServer");
                case Directories.CalledOutOrders:
                    throw new NotImplementedException("This type of data isn't called from this method or at all. You might be using the method wrong, consider using something in the TCPServer");
                default:
                    break;
            }
            throw new InCorrectDirectory("It either failed to get the correct directory from correct string provided or given the wrong directory. See exception for more info");

        }
        public async Task<List<Entity>> GetEntity<Entity>(string fullPath) where Entity : BaseEntity, new()
        {
            // If connection to online data or the online data itself doesn't come this just releases the offlineObjects
            // hence if false, it will fire offline
            if (!await connectionChecker.CheckConnection()) return await _offlineDataServices.GetOfflineData<Entity>(fullPath);

            await SetLastActive();

            return await _firebaseServices.GetData<Entity>(fullPath);

        }
        public async Task<List<Aggregate>> GetDataArray<Aggregate, Entity>(string path) where Aggregate : BaseAggregates<Entity>, new() where Entity : BaseEntity, new()
        {
            // If connection to online data or the online data itself doesn't come this just releases the offlineObjects
            // hence if false, it will fire offline
            if (!await connectionChecker.CheckConnection()) return await _offlineDataServices.GetOfflineDataArray<Aggregate, Entity>(path);

            await SetLastActive();

            return await _firebaseServices.GetDataArray<Aggregate, Entity>(path);

        }

        public void UpdateLocalStorage<Entity>(BaseAggregates<Entity> Aggregate, Directories directory)
        {
            if (BranchSettings.Instance.branchId == "/")
                return;

            //Delete Local Menu
            // @Yewo: I have no Idea why you do this, like why doe
            new SerializedObjectManager().DeleteData(Directories.Menu);

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
        public async Task UpdateOfflineData()
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
            #region Saves data

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


            #endregion


        }

        
        public async Task DeleteData(string fullPath)
        {

            _offlineDataServices.OfflineDeleteData(fullPath);
            // if there is connection
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();
                _firebaseServices.DeleteData(fullPath);
            }
           
        }
        [Obsolete]
        public async Task EditData_Online(string fullPath, object data)
        {
            // REFACTOR: Consider using Windows Message method
            if (!await connectionChecker.CheckConnection()) throw new NoNetworkException(" You can't edit online without network");

            await SetLastActive();

            _firebaseServices.StoreData(fullPath, data);

            if (fullPath.ToLower().Contains("menu"))
                UpdateLocalStorage((Menu)data, Directories.Menu);
        }

        public async void ResetLocalData(List<Order> orders)
        {
            //Makes sure only the server makes the syncing changes
            if (TCPServer.Instance == null)
                return;

            List<Order> orderItems = new List<Order>();

            //Offline include completed orders
            orderItems = (List<Order>)(await _orderService.GetOfflineOrdersCompletedInclusive());

            foreach (var item in orderItems)
            {
                //Add Back Completed and Deleted Orders To Local HDD So They Can Be Sent BACK Up At Sync Time
                // @Yewo: Why are Deleted orders sent up? or is that a feature we want to have, for analysis?
                if (item[0].Collected || item[0].MarkedForDeletion)
                    orders.Add(item);
            }

            //Clear hdd data of orders
            new SerializedObjectManager().DeleteData(Directories.Order);

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
            _offlineDataServices.UpdateNetworkDevices();

            return;
        }

        async Task SetLastActive()
        {
            // REFACTOR: Consider using Windows Message method
            if (!await connectionChecker.CheckConnection()) throw new NoNetworkException(" You can't edit online without network");

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
        public async Task StoreData(string fullPath, object data)
        {
            await _offlineDataServices.OfflineStoreData(fullPath, data);
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();
                _firebaseServices.StoreData(fullPath, data);
            }
            else
            {
                throw new NoNetworkException();
            }

            if (fullPath.ToLower().Contains("menu"))
                UpdateLocalStorage((Menu)data, Directories.Menu);

        }
        public async Task<bool> StoreData_Online_EndOfDaySync(string fullPath, object data)
        {
            // If there is no connection there is no way it can sync so should return false
            if (!await connectionChecker.CheckConnection()) return false;
            await StoreData(fullPath, data);
            return true;
        }

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

        Directories GetDirectory(string path)
        {
            string query = "";
            foreach (char c in path)
            {
                if (c != '/')
                    query += c;

                if (c == '/')
                    break;
            }

            var array = (Directories[])Directories.GetValues(typeof(Directories));

            var result = array.Where(d => d.ToString().ToLower() == query.ToLower());

            return result.ToList()[0];
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
                _firebaseServices.OnDataChanging("Order/" + BranchSettings.Instance.branchId);
        }
        public bool connected = false;
        bool lastStatus = false;
        public void ToggleConnectionStatus(bool status)
        {
            connected = status;

            //if current is online and last was offline => BackOnline

            if (connected && !lastStatus)
            {
                // This line is useless since you do this already 5 lines down
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
                WindowManager.Instance.CloseAllAndOpen(new SyncOrdersToDB(this));

            startedSyncing = true;
        }

        public void DataReceived()
        {
            startCounting = true;
            elapsedTime = 0;
        }



    }
}
