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
        Task OfflineStoreData(string fullPath, object data);
        Task OfflineGetData(string fullPath);

    }
}
