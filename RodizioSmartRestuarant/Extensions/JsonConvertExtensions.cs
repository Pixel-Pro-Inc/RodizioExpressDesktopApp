using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodizioSmartRestuarant.Exceptions;
using System;
using System.Collections.Generic;

namespace RodizioSmartRestuarant.Extensions
{
    //NOTE: I'm under the impression that extension methods, don't need to be added in AddApplicationServices, because them being static means they are assesable
    /// <summary>
    /// This is an extention class for turning responses from the database to the object types required
    /// </summary>
    public static class JsonConvertExtensions
    {
        /// <summary>
        /// This takes a response and changes its to the desired types. It is used when we 'expect' to have an aggregate returned to us
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns> A list of type <typeparamref name="T"/></returns>
        public static List<T> FromJsonToObjectArray<T>(this List<object> source)
       where T : class, new()
        {
            List<T> results = new List<T>();
            try
            {
                for (int i = 0; i < source.Count; i++)
                {
                    T item = JsonConvert.DeserializeObject<T>(((JArray)source[i]).ToString());
                    // This adds the deserialized list in the format into the type we are returning
                    results.Add(item);
                }

            }
            catch (JsonSerializationException jsEx)
            {
                throw new FailedToConvertFromJson($" The Extension failed to convert {results[results.Count]} to {typeof(T)}", jsEx);
            }
            catch (ArgumentOutOfRangeException argEx)
            {
                throw new FailedToConvertFromJson($" The Extension failed to convert {results[results.Count]} to {typeof(T)}", argEx);
            }
            catch (InvalidCastException inEx)
            {
                throw new FailedToConvertFromJson($" The Extension failed to convert {results[results.Count]} to {typeof(T)}" + "It might be cause it is expecting a JObject but we are " +
                    "trying to cast it to a JArray, You should try using FromJsonToObject instead", inEx);
            }

            return results;
        }

        /// <summary>
        /// This takes in the response when it is 'expected' to be a List of objects/Entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns> A list of type <typeparamref name="T"/></returns>
        public static List<T> FromJsonToObject<T>(this List<object> source)
      where T : class, new()
        {
            List<T> result = new List<T>();
            try
            {
                for (int i = 0; i < source.Count; i++)
                {
                    result.Add(JsonConvert.DeserializeObject<T>(((JObject)source[i]).ToString()));
                }

            }
            catch (JsonSerializationException jsEx)
            {
                throw new FailedToConvertFromJson($" The Extension failed to convert {result} to {typeof(T)}", jsEx);
            }
            catch (InvalidCastException inEx)
            {
                throw new FailedToConvertFromJson($" The Extension failed to convert {result} to {typeof(T)}", inEx);
            }
            catch (Exception ex)
            {
                throw new FailedToConvertFromJson($" The Extension failed to convert {result} to {typeof(T)}. This is most probably cause you gave it an aggregate instead of " +
                    $"and entity. ", ex);
            }

            return result;
        }

    }
}
