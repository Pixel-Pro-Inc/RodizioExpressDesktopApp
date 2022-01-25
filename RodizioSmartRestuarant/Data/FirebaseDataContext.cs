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

            var response = await GetData("Branch");

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
                WindowManager.Instance.UpdateAllOrderViews();
            }
            //

            elapsedTimeLastActive++;

            if(elapsedTimeLastActive >= 1800)
            {
                await SetLastActive();
            }
        }

        public async Task StoreData(string fullPath, object data)
        {
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();

                client = new FireSharp.FirebaseClient(config);

                var response = await client.SetAsync(fullPath, data);//Add Id of data  

                //await //UpdateOfflineData();

                return;
            }

            Directories currentDirectory = GetDirectory(fullPath);

            switch (currentDirectory)
            {
                case Directories.Order:
                    var orderresult = CovertListDictionaryOrders(await OfflineDataContext.GetData(currentDirectory));

                    if(orderresult.Count == 0)
                    {
                        List<IDictionary<string, object>> vals = new List<IDictionary<string, object>>();

                        IDictionary<string, object> itm = ((OrderItem)data).AsDictionary();

                        vals.Add(itm);

                        orderresult.Add(vals);

                        OfflineDataContext.StoreDataOverwrite(Directories.Order, orderresult);

                        LocalDataChange();

                        return;
                    }

                    if(data is List<List<IDictionary<string, object>>>)
                    {
                        if(((List<List<IDictionary<string, object>>>)data).Count != 0)
                        {
                            OfflineDataContext.StoreDataOverwrite(Directories.Order, data);

                            LocalDataChange();

                            return;
                        }                        
                    }

                    List<string> orderNumbers = GetCurrentOrderNumbers(orderresult);

                    if (orderNumbers.Contains(((OrderItem)data).OrderNumber))
                    {
                        OrderItem oldOrderItem = new OrderItem();
                        for (int i = 0; i < orderresult.Count; i++)
                        {
                            var list = orderresult[i];

                            foreach (var itm in list)
                            {
                                if((itm.ToObject<OrderItem>()).OrderNumber == ((OrderItem)data).OrderNumber)
                                {
                                    oldOrderItem = itm.ToObject<OrderItem>();
                                }
                            }
                        }

                        if (!(((OrderItem)data).Fufilled != oldOrderItem.Fufilled || ((OrderItem)data).Purchased != oldOrderItem.Purchased))
                        {
                            int index = orderNumbers.IndexOf(((OrderItem)data).OrderNumber);

                            IDictionary<string, object> itm = ((OrderItem)data).AsDictionary();

                            orderresult[index].Add(itm);

                            OfflineDataContext.StoreDataOverwrite(Directories.Order, orderresult);

                            LocalDataChange();

                            return;
                        }

                        OfflineDataContext.EditOrderData(Directories.Order, (OrderItem)data);

                        LocalDataChange();

                        return;
                    }

                    List<IDictionary<string, object>> values = new List<IDictionary<string, object>>();

                    IDictionary<string, object> item = ((OrderItem)data).AsDictionary();

                    values.Add(item);

                    orderresult.Add(values);

                    OfflineDataContext.StoreDataOverwrite(Directories.Order, orderresult);

                    LocalDataChange();
                    break;
            }
        }

        public async Task<List<object>> GetData(string fullPath)
        {
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();

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

                return objects;
            }

            Directories currentDirectory = GetDirectory(fullPath);            

            switch (currentDirectory)
            {
                case Directories.Order:
                    var orderresult = CovertListDictionaryOrders(await OfflineDataContext.GetData(currentDirectory));

                    List<object> orders = new List<object>();

                    foreach (var item in orderresult)
                    {
                        orders.Add(new List<object>());

                        foreach (var obj in item)
                        {
                            JObject valuePairs = (JObject)JToken.FromObject(obj.ToObject<OrderItem>());

                            ((List<object>)orders[orders.Count - 1]).Add(valuePairs);
                        }

                        orders[orders.Count - 1] = (JArray)JToken.FromObject(orders[orders.Count - 1]);
                    }

                    return orders;

                case Directories.Menu:

                    var menuresult = CovertListDictionary(await OfflineDataContext.GetData(currentDirectory));

                    List<object> menu = new List<object>();

                    foreach (var item in menuresult)
                    {
                        JObject keyValuePairs = (JObject)JToken.FromObject(item.ToObject<MenuItem>());
                        menu.Add(keyValuePairs);
                    }

                    return menu;

                case Directories.Account:
                    var accountresult = CovertListDictionary(await OfflineDataContext.GetData(currentDirectory));

                    List<object> accounts = new List<object>();

                    foreach (var item in accountresult)
                    {
                        JObject keyValuePairs = (JObject)JToken.FromObject(item.ToObject<AppUser>());
                        accounts.Add(keyValuePairs);
                    }

                    return accounts;

                case Directories.Branch:
                    var branchresult = await OfflineDataContext.GetData(Directories.Branch);

                    List<object> result = new List<object>();

                    if (branchresult is IDictionary<string, object>)
                    {
                        result.Add(((IDictionary<string, object>)branchresult).ToObject<Branch>());
                        return result;
                    }

                    var branch= (Branch)(((List<object>)branchresult)[0]);                    

                    result.Add(branch);

                    return result;
            }            

            return null;
        }        

        public async Task EditData(string fullPath, object data)
        {
            if (await connectionChecker.CheckConnection())
            {
                await SetLastActive();

                client = new FireSharp.FirebaseClient(config);

                var response = await client.UpdateAsync(fullPath, data);

                //await //UpdateOfflineData();

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

                //await //UpdateOfflineData();

                return;
            }

            //new OfflineDataContext().EditData(fullPath, (OrderItem)data);
        }

        public async Task CompleteOrder(string fullPath)
        {
            //Moves order to completed directory
            if (branchId != "/")
            {
                string destination = "CompletedOrders" + branchId + "/" + fullPath.Substring(14, 15);
                await StoreData(destination, await GetData(fullPath));

                await DeleteData(fullPath);
            }
        }

        public async Task CancelOrder(string fullPath)
        {
            //Moves order to completed directory
            if (branchId != "/")
            {
                string destination = "CancelledOrders" + branchId + "/" + fullPath.Substring(14, 15);
                var data = await GetData(fullPath);

                List<object> result = new List<object>();

                for (int i = 0; i < data.Count; i++)
                {
                    OrderItem item = JsonConvert.DeserializeObject<OrderItem>(((JObject)data[i]).ToString());
                    item.User = LocalStorage.Instance.user.FullName();

                    result.Add(JToken.FromObject(item));
                }                

                await StoreData(destination, result);

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
                        DataReceived();
                    },
                    (sender, args, context) => {
                        DataReceived();
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

        void BackOnline()
        {
            //Display syncing
            //SyncData();
            //Hide syncing
            GetDataChanging("Order/" + BranchSettings.Instance.branchId);
            ////UpdateOfflineData();
        }
        bool syncing = false;
        async void SyncData() //Apply offline changes to db
        {
            if(branchId != "/" && LocalStorage.Instance.networkIdentity.isServer)
            {
                syncing = true;
                #region Retrieve data
                object offlineData = null;

                if (await OfflineDataContext.GetData(Directories.Order) is List<List<IDictionary<string, object>>>)
                    offlineData = (List<List<IDictionary<string, object>>>) await OfflineDataContext.GetData(Directories.Order);


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

                List<List<OrderItem>> onlineOrders = new List<List<OrderItem>>();

                List<object> list = await GetData1("Order" + branchId);

                foreach (var item in list)
                {
                    List<OrderItem> data = JsonConvert.DeserializeObject<List<OrderItem>>(((JArray)item).ToString());

                    onlineOrders.Add(data);
                }
                #endregion

                //Add new offline orders to database
                foreach (var order in offlineOrders)
                {
                    if (GetCurrentOrderNumbersModel(onlineOrders).Contains(order[0].OrderNumber))
                        continue;

                    for (int i = 0; i < order.Count; i++)
                    {
                        order[i].Id = i;

                        await StoreData("Order" + branchId + "/" + order[i].OrderNumber + "/" + i, order[i]);
                    }
                }

                //Update with offline changes
                foreach (var order in offlineOrders)
                {
                    var ord = onlineOrders.Where(o => o[0].OrderNumber == order[0].OrderNumber).ToList();

                    if(ord.Count != 0)
                        if (OrderItemChanged(order, ord[0]))
                        {
                            for (int i = 0; i < order.Count; i++)
                            {
                                await EditData("Order" + branchId + "/" + order[i].OrderNumber + "/" + i, order[i]);
                            }
                        }
                }

                //Update with online changes
                if (WindowManager.Instance != null)
                    WindowManager.Instance.UpdateAllOrderViews();

                syncing = false;
            }
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

        bool OrderItemChanged(List<OrderItem> itemsNew, List<OrderItem> itemsOld)
        {
            string newItem = itemsNew[0].OrderNumber;
            string oldItem = itemsOld[0].OrderNumber;

            if (itemsNew.Count == itemsOld.Count)
                for (int i = 0; i < itemsNew.Count; i++)
                {
                    if (itemsNew[i].Fufilled != itemsOld[i].Fufilled || itemsNew[i].Purchased != itemsOld[i].Purchased)
                        return true;
                }

            return false;
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

        List<List<IDictionary<string, object>>> CovertListDictionaryOrders(object input)
        {
            Type type = input.GetType();

             if (input is List<List<IDictionary<string, object>>>)
                return (List<List<IDictionary<string, object>>>)input;

            if(input is List<object>)
            {
                List<List<IDictionary<string, object>>> keyValuePairs = new List<List<IDictionary<string, object>>>();

                foreach (var item in (List<object>)input)
                {
                    keyValuePairs.Add(new List<IDictionary<string, object>>());

                    foreach (var itm in (List<object>)item)
                    {
                        keyValuePairs[keyValuePairs.Count - 1].Add((IDictionary<string, object>)itm);
                    }
                }

                return keyValuePairs;
            }

            return new List<List<IDictionary<string, object>>>();
        }
        List<IDictionary<string, object>> CovertListDictionary(object input)
        {
            if (input is List<IDictionary<string, object>>)
                return (List<IDictionary<string, object>>)input;

            List<IDictionary<string, object>> keyValuePairs = new List<IDictionary<string, object>>();

            foreach (var item in (List<object>)input)
            {
                keyValuePairs.Add((IDictionary<string, object>)item);
            }

            return keyValuePairs;
        }

        void LocalDataChange()
        {
            WindowManager.Instance.UpdateAllOrderViews();
        }

        List<string> GetCurrentOrderNumbers(List<List<IDictionary<string, object>>> orders)
        {
            List<string> orderNumbers = new List<string>();

            object value = new object();

            foreach (var item in orders)
            {
                item[0].TryGetValue("OrderNumber", out value);

                if (!orderNumbers.Contains(value.ToString()))
                    orderNumbers.Add(value.ToString());
            }

            return orderNumbers;
        }
        List<string> GetCurrentOrderNumbersModel(List<List<OrderItem>> orders)
        {
            List<string> orderNumbers = new List<string>();

            foreach (var item in orders)
            {
                if (!orderNumbers.Contains(item[0].OrderNumber))
                    orderNumbers.Add(item[0].OrderNumber);
            }

            return orderNumbers;
        }
    }
}