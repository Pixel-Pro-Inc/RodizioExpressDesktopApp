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
        public List<List<OrderItem>> Search(string query, List<List<OrderItem>> orders)
        {
            List<List<OrderItem>> list = new List<List<OrderItem>>();

            foreach (var item in orders)
            {
                string orderNumber = item[0].OrderNumber.ToString();

                string x = orderNumber;
                string n = x.Substring(x.IndexOf('_') + 1, 4);

                if (item[0].PhoneNumber == query || n == query)
                {
                    List<OrderItem> orderItems = new List<OrderItem>();

                    orderItems.Add(item[0]);

                    list.Add(orderItems);
                }
            }

            return list;
        }
    }
}