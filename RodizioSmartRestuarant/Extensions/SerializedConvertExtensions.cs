using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Extensions
{
    /// <summary>
    /// This is an extention class for turning responses from the local file system to the object types required
    /// </summary>
    public static class SerializedConvertExtensions
    {
        /// <summary>
        /// This takes a response and changes its to the desired types. It is used when we 'expect' to have an aggregate returned to us
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns> A list of type <typeparamref name="T"/></returns>
        public static List<T> FromSerializedToObjectArray<T>(this object source)
       where T : class, new()
        {
            List<T> result = new List<T>();

            // @Abel: This needs to be done
            // TODO: Put the change we need here
            if (result.Count < 1) throw new NotImplementedException("The object hasn't been changed to the correct type yet. Put in the logic");

            return result;
        }

        /// <summary>
        /// This takes in the response when it is 'expected' to be returned as List of objects/Entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns> A list of type <typeparamref name="T"/></returns>
        public static List<T> FromSerializedToObject<T>(this object source)
      where T : class, new()
        {
            List<T> result = new List<T>();

            // @Abel: This needs to be done
            // TODO: Put the change we need here
            if (result.Count < 1) throw new NotImplementedException("The object hasn't been changed to the correct type yet. Put in the logic");

            return result;
        }
    }
}
