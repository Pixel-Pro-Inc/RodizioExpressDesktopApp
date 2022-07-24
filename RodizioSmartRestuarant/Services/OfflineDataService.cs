using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Exceptions;
using RodizioSmartRestuarant.Extensions;
using RodizioSmartRestuarant.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant.Helpers
{
    public class OfflineDataService:IOfflineDataService
    {
        #region Download

        // @Abel: So now you need to look at the previous version and create tests, then push those tests here.
        public async Task<List<Entity>> GetOfflineData<Entity>(string fullPath) where Entity : BaseEntity, new()
        {
            Directories currentDirectory = GetDirectory(fullPath);

            switch (currentDirectory)
            {

                case Directories.Account:
                    List<Entity> AppUsers = await GetEntities<Entity>(currentDirectory);
                    if (AppUsers == null)
                        throw new FailedToConvertFromSerialized($"Tried to get the {typeof(List<Entity>)} stored locally and came back with nothing.", new NullReferenceException());
                    return AppUsers;

                case Directories.Branch:

                    List<Entity> branchresult = await GetEntities<Entity>(currentDirectory);

                    if (branchresult.Count == 0)
                    {
                        // This is if the application is acting as The TCPServer
                        if (TCPServer.Instance != null)
                        {
                            MessageBoxResult messageBoxResult = MessageBox.Show("You have to have had internet at least once before using this application.", "Startup Failure", System.Windows.MessageBoxButton.OK);
                            if (messageBoxResult == MessageBoxResult.OK)
                            {
                                Application.Current.Shutdown();
                            }
                        }
                        else
                        {
                            MessageBoxResult messageBoxResult = MessageBox.Show("We were unable to connect to the local server. Please make sure its on and connected to the LAN before restarting this application again.", "Connection Failure", System.Windows.MessageBoxButton.OK);
                            if (messageBoxResult == MessageBoxResult.OK)
                            {
                                Application.Current.Shutdown();
                            }
                        }
                        // UPDATE: I didn't think it would be wise just to have it given an empty branch so I changed it so that it throws an exception
                        //branchresult.Add(new Entity());
                        throw new FailedToConvertFromSerialized($"Tried to get the {typeof(List<Entity>)} stored locally and came back with nothing.", new NullReferenceException());
                    }

                    return branchresult;
            }

            return null;
        }
        async Task<List<Entity>> GetEntities<Entity>(Directories currentDirectory) where Entity : BaseEntity, new()
        {
            object response = await OfflineDataContext.GetData(currentDirectory);
            List<Entity> entity = response.FromSerializedToObject<Entity>();
            return entity;
        }
        public async Task<List<Aggregate>> GetOfflineDataArray<Aggregate, Entity>(string fullPath) where Aggregate : BaseAggregates<Entity>, new()
        {
            Directories currentDirectory = GetDirectory(fullPath);
            List<Aggregate> aggregates = await GetAggregates<Aggregate, Entity>(currentDirectory);
            if (aggregates != null)
                return aggregates;
            // If nothing comes up
            throw new FailedToConvertFromSerialized($"Tried to get the list of {typeof(Entity)}s stored locally and came back with nothing.", new NullReferenceException());

        }
        async Task<List<Aggregate>> GetAggregates<Aggregate, Entity>(Directories currentDirectory) where Aggregate : BaseAggregates<Entity>, new()
        {
            object response = await OfflineDataContext.GetData(currentDirectory);
            List<Aggregate> aggregate = response.FromSerializedToObjectArray<Aggregate>();
            return aggregate;
        }

        #endregion
        #region Store

        public async Task OfflineStoreData(string fullPath, object data)
        {
            Directories currentDirectory = GetDirectory(fullPath);

            // REFACTOR: Too much different logic exists in this method. Please consider extracting logic. Really, what I'm saying is I understand what is going on here but,
            // @Yewo: I don't know what you are trying to accomplish here.
            // REFACTOR: // @Yewo: Why not just say if(currentDirectory is Directories.Order) ?
            switch (currentDirectory)
            {
                case Directories.Order:
                    var orderresult = CovertListDictionaryOrders(await OfflineDataContext.GetData(currentDirectory));

                    //if data originated from a TCP client it will be a Order
                    if (data is Order)
                    {
                        if (TCPServer.Instance == null)//Is Client
                        {
                            //Client Sends this to server
                            OfflineDataContext.StoreDataOverwrite(Directories.Order, (Order)data);
                            return;
                        }

                        if (TCPServer.Instance != null)
                        {
                            //First we convert to a List<IDictionary<string, object>>
                            List<IDictionary<string, object>> vals = new List<IDictionary<string, object>>();

                            foreach (var oItem in (Order)data)
                            {
                                IDictionary<string, object> itm = oItem.AsDictionary();

                                vals.Add(itm);
                            }

                            //Server Interprets Info
                            List<string> currentOrderNums = GetCurrentOrderNumbers(orderresult);

                            //If new order
                            object receivedOrderNumber = null;
                            vals[0].TryGetValue("OrderNumber", out receivedOrderNumber);

                            if (!currentOrderNums.Contains((string)receivedOrderNumber))
                            {
                                //New Order
                                orderresult.Add(vals);

                                //Store DATA
                                OfflineDataContext.StoreDataOverwrite(Directories.Order, orderresult);
                                return;
                            }

                            //If Editing existing order
                            //If Cancelling existing order
                            if (currentOrderNums.Contains((string)receivedOrderNumber))
                            {
                                //Editing Operation
                                //OfflineDataContext.StoreDataOverwrite(Directories.Order, orderresult);
                                foreach (var orderItem in (Order)data)
                                {
                                    //Directly Call SerializedObjectManager To Avoid Extra LocalDataChangeCall
                                    new SerializedObjectManager().EditOrderData(orderItem, Directories.Order);
                                }

                                LocalDataChange();
                                return;
                            }
                        }
                    }

                    if (orderresult.Count == 0)
                    {
                        List<IDictionary<string, object>> vals = new List<IDictionary<string, object>>();

                        IDictionary<string, object> itm = ((OrderItem)data).AsDictionary();

                        vals.Add(itm);

                        orderresult.Add(vals);

                        OfflineDataContext.StoreDataOverwrite(Directories.Order, orderresult);

                        LocalDataChange();

                        return;
                    }

                    if (data is List<List<IDictionary<string, object>>>)
                    {
                        if (((List<List<IDictionary<string, object>>>)data).Count != 0)
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
                            var list = orderresult[i]; //Order as dictionary

                            foreach (var itm in list)
                            {
                                if ((itm.ToObject<OrderItem>()).OrderNumber == ((OrderItem)data).OrderNumber)
                                {
                                    if ((itm.ToObject<OrderItem>()).Id == ((OrderItem)data).Id)
                                        oldOrderItem = itm.ToObject<OrderItem>();
                                }
                            }
                        }

                        //Specifically here for call in orders
                        if (!(((OrderItem)data).Fufilled != oldOrderItem.Fufilled
                            || ((OrderItem)data).Purchased != false
                            || ((OrderItem)data).Collected != oldOrderItem.Collected
                            || ((OrderItem)data).MarkedForDeletion != oldOrderItem.MarkedForDeletion))
                        {
                            //If there are no differences with a standard order item || old order item

                            int index = orderNumbers.IndexOf(((OrderItem)data).OrderNumber);

                            IDictionary<string, object> itm = ((OrderItem)data).AsDictionary();

                            orderresult[index].Add(itm);

                            OfflineDataContext.StoreDataOverwrite(Directories.Order, orderresult);


                            LocalDataChange();

                            return;
                        }

                        //This Block Causes Order 2593 To Duplicate Its Order Items Why?
                        if (!(((OrderItem)data).Fufilled != oldOrderItem.Fufilled
                            || ((OrderItem)data).Purchased != oldOrderItem.Purchased
                            || ((OrderItem)data).Collected != oldOrderItem.Collected
                            || ((OrderItem)data).MarkedForDeletion != oldOrderItem.MarkedForDeletion))
                        {
                            //If there are no differences with a standard order item || old order item

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
                case Directories.Account:

                    List<IDictionary<string, object>> Users = new List<IDictionary<string, object>>();

                    // if it not an appUser then it 
                    if (!(data is List<AppUser>))
                    {
                        throw new TypeLoadException("Something when wrong. The computer thinks that we want a list of AppUsers but can't identify the data as such");
                    }
                    foreach (var person in (List<AppUser>)data)
                    {
                        Users.Add(person.AsDictionary());
                    }

                    new SerializedObjectManager().SaveOverwriteData(Users, Directories.Account);

                    break;
            }
        }

        #endregion
        #region Update

        protected void OfflineDeleteOrder(Order order) => OfflineDataContext.DeleteOrder(Directories.Order, order);
        protected void LocalDataChange()
        {
            WindowManager.Instance.UpdateAllOrderViews_Offline();
            UpdateNetworkDevices();
        }
        protected void UpdateNetworkDevices()
        {
            if (TCPServer.Instance != null)
                TCPServer.Instance.UpdateAllNetworkDevicesUI();
        }

        #endregion

        protected Directories GetDirectory(string path)
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
        protected List<List<IDictionary<string, object>>> CovertListDictionaryOrders(object input)
        {
            Type type = input.GetType();

            if (input is List<List<IDictionary<string, object>>>)
                return (List<List<IDictionary<string, object>>>)input;

            if (input is List<object>)
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
        protected List<IDictionary<string, object>> CovertListDictionary(object input)
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
        protected List<string> GetCurrentOrderNumbers(List<List<IDictionary<string, object>>> orders)
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
        protected List<string> GetCurrentOrderNumbersModel(List<Order> orders)
        {
            List<string> orderNumbers = new List<string>();

            foreach (var item in orders)
            {
                if (!orderNumbers.Contains(item[0].OrderNumber))
                    orderNumbers.Add(item[0].OrderNumber);
            }

            return orderNumbers;
        }

        void ShowWarning(string msg)
        {
            string messageBoxText = msg;
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
        }
    }
}
