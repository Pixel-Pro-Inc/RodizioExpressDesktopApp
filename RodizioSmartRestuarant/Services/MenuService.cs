using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Extensions;
using RodizioSmartRestuarant.Helpers;
using RodizioSmartRestuarant.Interfaces;
using RodizioSmartRestuarant.Interfaces.BaseInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RodizioSmartRestuarant.Entities.Enums;

namespace RodizioSmartRestuarant.Services
{
    public class MenuService : _BaseService, IMenuService
    {
        private readonly IFirebaseServices _firebaseServices;
        private readonly IDataService _dataService;

        public MenuService(IFirebaseServices firebaseServices, IDataService dataService)
        {
            _firebaseServices = firebaseServices;
            _dataService = dataService;
        }

        public async Task<Menu> GetOnlineMenu(string branchId)
        {
            Menu menu=(Menu)await _firebaseServices.GetData<MenuItem>("Menu/" + branchId);
            _dataService.UpdateLocalStorage(menu, Directories.Menu);
            return menu;
        }
        public Menu SearchForQueryString(string query, Menu menu)
        {
            Menu list = new Menu();

            foreach (var menuitem in menu)
            {
                if (menuitem.Name.ToLower().Contains(query.ToLower()) || menuitem.Category.ToLower().Contains(query.ToLower()))
                {
                    list.Add(menuitem);
                }
            }

            return list;
        }


    }
}
