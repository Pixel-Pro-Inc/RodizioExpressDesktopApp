using RodizioSmartRestuarant.Core.Entities.Aggregates;
using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Helpers
{
    public class SearchOrders
    {
        public List<Order> Search(string query, List<Order> orders)
        {
            List<Order> list = new List<Order>();

            foreach (var item in orders)
            {
                string orderNumber = item[0].OrderNumber.ToString();

                string x = orderNumber;
                string n = x.Substring(x.IndexOf('_') + 1, 4);

                if (item[0].PhoneNumber == query || n == query)
                {
                    Order orderItems = new Order();

                    orderItems.Add(item[0]);

                    list.Add(orderItems);
                }
            }

            return list;
        }
    }
}