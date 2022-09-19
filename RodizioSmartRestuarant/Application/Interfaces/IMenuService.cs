using RodizioSmartRestuarant.Core.Entities.Aggregates;
using RodizioSmartRestuarant.Interfaces.BaseInterfaces;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Application.Interfaces
{
    /// <summary>
    /// This is to abstract all the Menu logic into a unified scope. A good use is <see cref=""/> which I created cause we had duplicate code in <see cref="FirebaseDataContext"/>
    /// </summary>
    public interface IMenuService :IBaseService
    {
        /// <summary>
        /// This gets the menu from the database. It also updates the local storage of the menu
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<Menu> GetOnlineMenu(string path);

        /// <summary>
        /// Checks if the Menu inputed has the query string as its category or name. Nothing actually advanced
        /// </summary>
        /// <param name="query"></param>
        /// <param name="menu"></param>
        /// <returns></returns>
        Menu SearchForQueryString(string query, Menu menu);

    }
}
