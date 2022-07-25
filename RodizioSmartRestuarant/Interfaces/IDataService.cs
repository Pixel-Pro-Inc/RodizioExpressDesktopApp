using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Data;
using static RodizioSmartRestuarant.Entities.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using RodizioSmartRestuarant.Entities;

namespace RodizioSmartRestuarant.Interfaces
{
    /// <summary>
    /// This will coordinate all the offline and online data so its seemless and used by all the services and components that should access it. Its job is basically to feed what ever is asking of it the most relevant information available
    /// <para> I don't really expect to fuse <see cref="IFirebaseServices"/> and <see cref="IOfflineDataService"/> because them being by themselves makes sense</para>
    /// </summary>
    public interface IDataService:IBaseService
    {
        bool startedSyncing { get; set; }

        /// <summary>
        /// Takes an <paramref name="Aggregate"/> and stores the data locally and updates the changes using <see cref="OfflineDataContext.LocalDataChange()"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Aggregate"></param>
        /// <param name="directory"></param>
        void UpdateLocalStorage<T>(BaseAggregates<T> Aggregate, Directories directory);

        Task UpdateOfflineData();
        void ResetLocalData(List<Order> orders);

        /// <summary>
        /// This adds '/' and the static branch setting instance branch id. Note that there hasn't been a check if there is already a branch id set
        /// </summary>
        void SetBranchId();

        Task StoreData(string fullPath, object data);

        /// <summary>
        /// This is to get data whether online or offline. It gets only single types, not Aggregate types
        /// </summary>
        /// <typeparam name="Entity"></typeparam>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        Task<List<Entity>> GetEntity<Entity>(string fullPath) where Entity : BaseEntity, new();
        /// <summary>
        /// This is to get data whether online or offline.It gets only Aggregate types, not single types
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<List<Aggregate>> GetDataArray<Aggregate, Entity>(string path) where Aggregate : BaseAggregates<Entity>, new();

        /// <summary>
        /// This is a method that will be used when ever you need to get anything. Just pop in what ever the directory type you want as a string into <paramref name="fullPath"/> for example "Account" to get appusers. or "Branch" for branches. You would 
        /// typically simply put in the full path and the directory type is already included in what you are looking for
        /// <para> Note that it doesn't work for everything, eg  <see cref="NetworkIdentity"/>s are still not BaseEntities. so you might have trouble getting that info just as yet.</para>
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        Task<object> GetData(string fullPath);
        /// <summary>
        /// Removes the data from the database if there is connection and locally in the other case
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        Task DeleteData(string fullPath);

        Task<object> GetOfflineOrdersCompletedInclusive();

        /// <summary>
        /// This really shouldn't be here. This method is patch work cause we haven't masters events just yet.
        /// <para> But from what I understand, this is to change data so that the tick counter can start afresh to check for updates</para>
        /// </summary>
        void DataReceived();

        /// <summary>
        /// This basically gives the 'connected' bool the status you input and works accordingly if reconnected after offline status
        /// </summary>
        /// <param name="status"></param>
        void ToggleConnectionStatus(bool status);
        Task SyncDataEndOfDay(List<Order> orders);

        // TODO: Make a OrderService
        Task CancelOrder(Order orderItems);
    }
}
