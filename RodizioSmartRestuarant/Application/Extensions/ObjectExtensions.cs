using Newtonsoft.Json;
using RodizioSmartRestuarant.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace RodizioSmartRestuarant.Application.Extensions
{
    public static class ObjectExtensions
    {
        // TRACK: I am assuming that this is so you can change atype from what it is to what you want it to be?
        public static T ToObject<T>(this IDictionary<string, object> source)
        where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in source)
            {
                if (item.Value == null)
                    continue;

                someObjectType
                         .GetProperty(item.Key)
                         .SetValue(someObject, item.Value, null);
            }

            return someObject;
        }

        /// <summary>
        /// This gives <paramref name="source"/> a key and to itself so it can be inputed in a <see cref="IDictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
              return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }

        public static byte[] ToByteArray<T>(this T obj, string destination)
        {
            //We only use JSON objects if the data is going to an android so that we dont run into issues with namespaces

            if(destination.ToUpper() == "MOBILE")
            {
                if (obj == null)
                    return null;

                var data = JsonConvert.SerializeObject(obj);//JsonSerializer.SerializeToUtf8Bytes(obj);
                
                return System.Text.Encoding.UTF8.GetBytes(data);
            }

            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T FromByteArray<T>(this byte[] data) where T : class, new()
        {
            if (data == null)
                return default(T);

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                var obj = bf.Deserialize(ms);

                if (obj is string)
                {
                    if (TCPServer.Instance != null)
                        TCPServer.Instance.lastRequestSource = "MOBILE";

                    return JsonConvert.DeserializeObject<T>(obj.ToString());
                }

                //Test
                if (TCPServer.Instance != null)
                    TCPServer.Instance.lastRequestSource = "!MOBILE";

                return (T)obj;
            }
        }
    }
}
