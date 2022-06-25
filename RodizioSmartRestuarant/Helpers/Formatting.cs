﻿using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
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
        public static List<Order> ChronologicalOrderList(List<Order> orderItems)
        {
            List<Order> temp = new List<Order>();
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
                temp.Add(orderItems.Where(o => o[0].OrderDateTime == lastTime).ToArray()[0]);
            }

            return temp.ToArray().Reverse().ToList();
        }
    }
}
