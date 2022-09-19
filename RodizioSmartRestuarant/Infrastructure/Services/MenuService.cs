using RodizioSmartRestuarant.Application.Interfaces;
using RodizioSmartRestuarant.Core.Entities;
using RodizioSmartRestuarant.Core.Entities.Aggregates;
using System.Threading.Tasks;
using static RodizioSmartRestuarant.Core.Entities.Enums;

namespace RodizioSmartRestuarant.Infrastructure.Services
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
