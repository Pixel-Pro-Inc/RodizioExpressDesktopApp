using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Data;
using static RodizioSmartRestuarant.Entities.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RodizioSmartRestuarant.Interfaces
{
    public interface IDataService:IBaseService
    {
        /// <summary>
        /// Takes an <paramref name="Aggregate"/> and stores the data in the locally and updates the changes using <see cref="OfflineDataContext.LocalDataChange()"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Aggregate"></param>
        /// <param name="directory"></param>
        void UpdateLocalStorage<T>(BaseAggregates<T> Aggregate, Directories directory);

        /// <summary>
        /// This is to get data whether online or offline
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        Task<List<object>> GetData_Online(string fullPath);
    }
}
