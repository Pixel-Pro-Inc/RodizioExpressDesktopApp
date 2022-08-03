﻿using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Helpers;
using RodizioSmartRestuarant.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant.Services
{
    public class OrderService: _BaseService, IOrderService
    {
        IDataService _dataService;
        IOfflineDataService _offlineService;

        public OrderService( IDataService dataService, IOfflineDataService offlineDataService)
        {
            _dataService = dataService;
            _offlineService = offlineDataService;
        }

        public async Task MarkOrderForDeletion(Order orderItems)
        {
            //Mark for deletion when back online
            foreach (var item in orderItems)
            {
                item.MarkedForDeletion = true;
                string branchId = BranchSettings.Instance.branchId;
                string fullPath = "Order/" + branchId + "/" + item.OrderNumber + "/" + item.Id.ToString();

                if (TCPServer.Instance != null)
                    await _dataService.StoreData(fullPath, item);//Remove from order view on all network devices
            }

            if (TCPServer.Instance == null)
                await _dataService.StoreData("Order/", orderItems);
        }
        public async Task CancelOrder(string orderInvoice)
        {
            //Moves order to Cancelled directory // UPDATE: I changed the comment where the word said completed to Cancelled directory
            if (BranchSettings.Instance.branchId != "/")
            {
                string destination = "CancelledOrders" + BranchSettings.Instance.branchId + "/" + orderInvoice.Substring(14, 15);
                List<Order> Orders = await _dataService.GetDataArray<Order, OrderItem>(orderInvoice);

                foreach (var order in Orders)
                {
                    order[0].User = LocalStorage.Instance.user.FullName();
                }

                await _dataService.StoreData(destination, Orders);

                await _dataService.DeleteData(orderInvoice);
            }
        }

        public List<Order> SearchForQueryString(string querystring, List<Order> orders)
        {
            List<Order> list = new List<Order>();

            foreach (var item in orders)
            {
                string orderNumber = item[0].OrderNumber.ToString();

                string x = orderNumber;
                string n = x.Substring(x.IndexOf('_') + 1, 4);

                if (item[0].PhoneNumber == querystring || n == querystring)
                {
                    Order orderItems = new Order();

                    orderItems.Add(item[0]);

                    list.Add(orderItems);
                }
            }

            return list;
        }
        public async Task<object> GetOfflineOrdersCompletedInclusive()=> await _offlineService.GetOfflineDataArray<Order, OrderItem>("Order");

        public async Task CompleteOrder(string fullPath)
        {
            if (BranchSettings.Instance.branchId != "/")
            {
                string destination = "CompletedOrders" + BranchSettings.Instance.branchId + "/" + fullPath.Substring(14, 15);
                await _dataService.StoreData(destination, _dataService.GetDataArray<Order, OrderItem>(fullPath));

                await _dataService.DeleteData(fullPath);
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
        // TODO: New order bug
        /*
         Say you want to remove an order placed, there is no functionality to remove it, you just have to start the order all afresh

         */

    }
}
