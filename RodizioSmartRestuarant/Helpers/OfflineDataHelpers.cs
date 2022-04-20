﻿using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant.Helpers
{
    public class OfflineDataHelpers
    {
        protected async Task<List<object>> OfflineGetData(string fullPath)
        {
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

                    if(((List<object>)branchresult).Count == 0)
                    {
                        //Error Message
                        MessageBoxResult messageBoxResult = MessageBox.Show("You have to have had internet at least once before using this application.", "Startup Failure", System.Windows.MessageBoxButton.OK);
                        if (messageBoxResult == MessageBoxResult.OK)
                        {
                            Application.Current.Shutdown();
                        }

                        result.Add(new Branch());

                        return result;
                    }

                    var branch = (Branch)(((List<object>)branchresult)[0]);

                    result.Add(branch);

                    return result;
            }

            return null;
        }
        protected async Task OfflineStoreData(string fullPath, object data)
        {
            Directories currentDirectory = GetDirectory(fullPath);

            switch (currentDirectory)
            {
                case Directories.Order:
                    var orderresult = CovertListDictionaryOrders(await OfflineDataContext.GetData(currentDirectory));

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
                            var list = orderresult[i]; //List<OrderItem> as dictionary

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
                            || ((OrderItem)data).Collected != oldOrderItem.Collected))
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
                            || ((OrderItem)data).Collected != oldOrderItem.Collected))
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
            }
        }
        protected void OfflineDeleteOrder(List<OrderItem> order)
        {
            OfflineDataContext.DeleteOrder(Directories.Order, order);
        }
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
        protected List<string> GetCurrentOrderNumbersModel(List<List<OrderItem>> orders)
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