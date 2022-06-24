using RodizioSmartRestuarant.Entities;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using RodizioSmartRestuarant.Configuration;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Windows.Threading;
using static RodizioSmartRestuarant.Entities.Enums;
using RodizioSmartRestuarant.Extensions;
using RodizioSmartRestuarant.Interfaces;

namespace RodizioSmartRestuarant.Data
{
    // REFACTOR: consider changing Offlinedatahelpers
    /// <summary>
    ///  
    /// </summary>
    public class FirebaseDataContext:OfflineDataService
    {
        public static FirebaseDataContext Instance { get; set; }

        
        string branchId = "";

        // REFACTOR: Use environment variables here
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = ConnectionStringManager.GetConnectionString("FirebaseAuth"),
            BasePath = ConnectionStringManager.GetConnectionString("FireBaseBasePath")
        };
        IFirebaseClient client;
        IDataService _dataService;

        

        public FirebaseDataContext()
        {
            client = new FireSharp.FirebaseClient(config);
            Instance = this;
        }

        

        #region Reform version


        public async Task DeleteData(string fullPath)=> await client.DeleteAsync(fullPath);
        /// <summary>
        /// This sets data in the database using the <see cref="IFirebaseClient.SetAsync{T}(string, T)"/> method
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task StoreDataOnline(string path, object data) => await client.SetAsync(path, data);

        // TODO: Have all references of this use JsonConvert
        public async Task<List<object>> GetData(string path)
        {
            List<object> objects = new List<object>();

            FirebaseResponse response = await client.GetAsync(path);

            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            if (data != null)
            {
                foreach (var item in data)
                {
                    object _object = new object();

                    if (item != null)
                    {
                        // REFACTOR: Try to see if this is the solution we need to make the overload in JsonConvertExtension unified
                        if (item.GetType() == typeof(JProperty))
                        {
                            _object = JsonConvert.DeserializeObject<object>(((JProperty)item).Value.ToString());
                        }
                        else
                        {
                            _object = JsonConvert.DeserializeObject<object>(((JObject)item).ToString());
                        }

                        objects.Add(_object);
                    }
                }
            }

            return objects;
        }
        /// <summary>
        /// This updates the data in the database using the <see cref=" IFirebaseClient.UpdateAsync{T}(string, T)"/> method
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task EditData(string path, object data) => await client.UpdateAsync(path, data);

        #endregion

        /// <summary>
        /// This is a method that checks if the data is that path has changed. Or At least I think thats what it does.
        /// <para> Its uses a <see cref="EventStreamResponse"/> taken from the client using <see cref="IFirebaseClient.OnAsync(string, FireSharp.EventStreaming.ValueAddedEventHandler, FireSharp.EventStreaming.ValueChangedEventHandler, FireSharp.EventStreaming.ValueRemovedEventHandler, object)"/> 
        ///. Its very weird, you would want to check out so just navigate to it</para>
        /// </summary>
        /// <param name="fullPath"></param>
        public async void GetDataChanging(string fullPath)
        {
            EventStreamResponse response = await client.OnAsync(fullPath,
                    (sender, args, context) => {
                        _dataService.DataReceived();
                    },
                    (sender, args, context) => {
                        ;//DataReceived();
                    },
                    (sender, args, context) => {
                        ;//DataReceived();
                    });
        }


    }
}