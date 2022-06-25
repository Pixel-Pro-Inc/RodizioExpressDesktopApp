using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Data;
using static RodizioSmartRestuarant.Entities.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using RodizioSmartRestuarant.Entities;

namespace RodizioSmartRestuarant.Interfaces
{
    /// <summary>
    /// This will coordinate all the offline and online data so its seemless and used by all the services and components that should access it
    /// <para> I don't really expect to fuse <see cref="IFirebaseServices"/> and <see cref="OfflineDataService"/> because them being by themselves makes sense</para>
    /// </summary>
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
        Task<List<T>> GetData<T>(string fullPath) where T : BaseEntity, new();

        /// <summary>
        /// This really shouldn't be here. This method is patch work cause we haven't masters events just yet.
        /// <para> But from what I understand, this is to change data so that the tick counter can start afresh to check for updates</para>
        /// </summary>
        void DataReceived();
    }
}
