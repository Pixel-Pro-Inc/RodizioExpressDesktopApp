using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Helpers
{
    public class SearchMenu
    {
        public List<MenuItem> Search(string query, List<MenuItem> menuItems)
        {
            List<MenuItem> list = new List<MenuItem>();

            foreach (var item in menuItems)
            {
                if (item.Name.ToLower().Contains(query.ToLower()) || item.Category.ToLower().Contains(query.ToLower()))
                {
                    list.Add(item);
                }
            }

            return list;
        }
    }
}
