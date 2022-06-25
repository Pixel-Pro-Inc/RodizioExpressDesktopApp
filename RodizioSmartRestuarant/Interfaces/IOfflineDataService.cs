using RodizioSmartRestuarant.Entities;
using RodizioSmartRestuarant.Entities.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Interfaces
{
    /// <summary>
    /// This service handles all things offline. It primarily talks to <see cref="IDataService"/> and I don't expect it to be used anywhere else
    /// </summary>
    interface IOfflineDataService:IBaseService
    {
        /// <summary>
        /// This will take the <typeparamref name="Entity"/> type you are looking for, and give you a <see cref="List{Entity}"/>s
        /// from the local storage 
        /// <para> It accomplishes doing this by using this <see cref="GetDirectory(string)"/> and passing in that <see cref="Directories"/> into
        /// <see cref="GetEntities{Entity}(Directories)"/>. If the <see cref="List{Entity}"/> is null it will throw <see cref="FailedToConvertFromSerialized"/></para>
        /// </summary>
        /// <typeparam name="Entity"></typeparam>
        /// <param name="fullPath"></param>
        /// <returns> <see cref="List{Entity}"/></returns>
        Task<List<Entity>> GetOfflineData<Entity>(string fullPath) where Entity : BaseEntity, new();
        /// <summary>
        /// This will take the <typeparamref name="Aggregate"/> type you are looking for and the <typeparamref name="Entity"/> for type checking, and give you a list of <typeparamref name="Aggregate"/> 
        /// from the local storage 
        /// <para> It accomplishes doing this by using this <see cref="GetDirectory(string)"/> and passing in that <see cref="Directories"/> into
        /// <see cref="GetAggregates{Aggregate, Entity}(Directories)"/>. If the <see cref="List{Aggregates}"/> is null it will throw <see cref="FailedToConvertFromSerialized"/></para>
        /// </summary>
        /// <typeparam name="Aggregate"></typeparam>
        /// <typeparam name="Entity"></typeparam>
        /// <param name="fullPath"></param>
        /// <returns><see cref="List{Aggregate}"/></returns>
        Task<List<Aggregate>> GetOfflineDataArray<Aggregate, Entity>(string fullPath) where Aggregate : BaseAggregates<Entity>, new();
        Task OfflineStoreData(string fullPath, object data);

    }
}
