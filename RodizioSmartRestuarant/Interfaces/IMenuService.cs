using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Interfaces
{
    /// <summary>
    /// This is to abstract all the Menu logic into a unified scope. A good use is <see cref=""/> which I created cause we had duplicate code in <see cref="FirebaseDataContext"/>
    /// </summary>
    interface IMenuService :IBaseService
    {
        /// <summary>
        /// This gets the menu from the database. It also updates the local storage of the menu
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<Menu> GetOnlineMenu(string path);

    }
}
