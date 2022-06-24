using RodizioSmartRestuarant.Entities.Aggregates;

namespace RodizioSmartRestuarant.Helpers
{
    public class SearchMenu
    {
        public Menu Search(string query, Menu menuItems)
        {
            Menu list = new Menu();

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
