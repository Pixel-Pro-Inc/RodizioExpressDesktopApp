using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Helpers
{
    public static class Formatting
    {
        public static string FormatAmountString(float amount)=> String.Format("{0:n}", amount); // format 1,000,000.00
        public static string FormatListToString(List<string> strings)
        {
            string data = "";
            if (strings == null)
                return data;

            foreach (var item in strings)
            {
                if (strings.IndexOf(item) == 0)
                    data += item;

                if (strings.IndexOf(item) != 0)
                    data += ", " + item;
            }

            return data;
        }
        public static List<List<OrderItem>> ChronologicalOrderList(List<List<OrderItem>> orderItems)
        {
            List<List<OrderItem>> temp = new List<List<OrderItem>>();
            List<DateTime> usedTimes = new List<DateTime>();
            DateTime lastTime = new DateTime();

            for (int i = 0; i < orderItems.Count; i++)
            {
                lastTime = new DateTime();
                foreach (var item in orderItems)
                {
                    if (item[0].OrderDateTime > lastTime && !usedTimes.Contains(item[0].OrderDateTime))
                    {
                        lastTime = item[0].OrderDateTime;
                    }
                }

                usedTimes.Add(lastTime);

                foreach (var order in orderItems)
                {
                    if (order[0].OrderDateTime == lastTime)
                        if (!temp.Contains(order))
                            temp.Add(order);
                }
            }

            return temp.ToArray().Reverse().ToList();
        }
    }
}
