using System.Collections.Generic;
using System.Threading.Tasks;

using RodizioSmartRestuarant.Interfaces;
using RodizioSmartRestuarant.Services;
using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Entities.Aggregates;
using RodizioSmartRestuarant.Extensions;

namespace API.Services
{
    public class FirebaseServices : _BaseService, IFirebaseServices
    {

        public readonly FirebaseDataContext _firebaseDataContext;
        public FirebaseServices()
        {
            _firebaseDataContext = new FirebaseDataContext();
        }

        public async void StoreData(string path, object thing)=> await _firebaseDataContext.StoreData(path, thing);
        public async void DeleteData(string fullpath) => await _firebaseDataContext.DeleteData(fullpath);
        public async Task<List<T>> GetData<T>(string path) where T : class, new()
        {
            List<T> objects = new List<T>();

            var response = await _firebaseDataContext.GetData(path);
            objects = response.FromJsonToObject<T>();

            return objects;
        }
        public async Task<List<Aggregate>> GetDataArray<Aggregate, Entity>(string path) where Aggregate : BaseAggregates<Entity>, new()
        {
            List<Aggregate> objects = new List<Aggregate>();

            var response = await _firebaseDataContext.GetData(path);
            // NOTE: Here you might get errors cause at some point it was refusing to take the correct overload
            objects = response.FromJsonToObjectArray<Aggregate>();

            return objects;
        }
        public void OnDataChanging(string fullPath) => _firebaseDataContext.OnDataChanging(fullPath);

    }
}
