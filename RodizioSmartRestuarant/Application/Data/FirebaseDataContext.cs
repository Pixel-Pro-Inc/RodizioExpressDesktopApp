using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RodizioSmartRestuarant.Application.Interfaces;
using RodizioSmartRestuarant.Infrastructure.Helpers;

namespace RodizioSmartRestuarant.Application.Data
{
    /// <summary>
    ///  
    /// </summary>
    public class FirebaseDataContext
    {
        public static FirebaseDataContext Instance { get; set; }
        string branchId = "";

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = ConnectionStringManager.GetConnectionString("RodizioSmartRestuarant.Properties.Settings.FirebaseAuth"),
            BasePath = ConnectionStringManager.GetConnectionString("RodizioSmartRestuarant.Properties.Settings.FireBaseBasePath")
        };
        IFirebaseClient client;
        IDataService _dataService;

        public FirebaseDataContext()
        {
            client = new FireSharp.FirebaseClient(config);
            Instance = this;
        }

        public async Task DeleteData(string fullPath) => await client.DeleteAsync(fullPath);
        /// <summary>
        /// This sets data in the database using the <see cref="IFirebaseClient.SetAsync{T}(string, T)"/> method
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task StoreData(string path, object data) => await client.SetAsync(path, data);
        /// <summary>
        /// Gets data from database by using <see cref="IFirebaseClient.GetAsync(string)"/> and deserializes them using <see cref="JsonConvert.DeserializeObject{T}(string)"/>
        /// </summary>
        /// <param name="path"></param>
        /// <returns> <see cref="List{Object}"/></returns>
        public async Task<List<object>> GetData(string path)
        {
            List<object> objects = new List<object>();

            // Collect data from database
            FirebaseResponse response = await client.GetAsync(path);

            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            // This is cause if it is null we just give it back the null
            if (data == null) return objects;
            // This works well when it is a list of objects to parse, but as a single object it fights
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

            return objects;
        }
        /// <summary>
        /// This updates the data in the database using the <see cref=" IFirebaseClient.UpdateAsync{T}(string, T)"/> method
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task EditData(string path, object data) => await client.UpdateAsync(path, data);

        /// <summary>
        /// This is a method that checks if the data is that path has changed. Or At least I think thats what it does.
        /// <para> Its uses a <see cref="EventStreamResponse"/> taken from the client using <see cref="IFirebaseClient.OnAsync(string, FireSharp.EventStreaming.ValueAddedEventHandler, FireSharp.EventStreaming.ValueChangedEventHandler, FireSharp.EventStreaming.ValueRemovedEventHandler, object)"/> 
        ///. Its very weird, you would want to check out so just navigate to it</para>
        /// </summary>
        /// <param name="fullPath"></param>
        public async void OnDataChanging(string fullPath)
        {
            // TODO: Have this have tests
            EventStreamResponse response = await client.OnAsync(fullPath,
                    (sender, args, context) => {
                        _dataService.DataReceived();
                    });
        }
        // @Yewo: I think you need to consider making a test for this one, or at least explain to me what it hopes to accomplish cause yeah, I don't fully understand its behavious

    }
}