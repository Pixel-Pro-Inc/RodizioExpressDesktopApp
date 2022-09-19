using RodizioSmartRestuarant.Application.Exceptions;
using RodizioSmartRestuarant.Core.Entities;
using RodizioSmartRestuarant.Core.Entities.Aggregates;
using System;
using System.Collections.Generic;

namespace RodizioSmartRestuarant.Application.Extensions
{
    /// <summary>
    /// This is an extention class for turning responses from the local file system to the object types required
    /// </summary>
    public static class SerializedConvertExtensions
    {
        /// <summary>
        /// This takes a response and changes its to the desired types. It is used when we 'expect' to have an aggregate returned to us.
        /// <para> We honestly expect this to get in an object thats really a List of a List of <see cref="IDictionary{TKey, TValue} "/></para>
        /// </summary>
        /// <typeparam name="Aggregate"></typeparam>
        /// <param name="source"></param>
        /// <returns> A list of type <typeparamref name="Aggregate"/></returns>
        public static List<Aggregate> FromSerializedToObjectArray<Aggregate, Entity>(this object source)
       where Aggregate : BaseAggregates<Entity>, new() where Entity : BaseEntity, new ()
        {
            List<Aggregate> result = new List<Aggregate>();
            List<List<IDictionary<string, object>>> input;
            try
            {
                input = (List<List<IDictionary<string, object>>>)source;
            }
            catch (InvalidCastException e)
            {
                throw new FailedToConvertFromSerialized(" Yeah this isn't the object that you are able to go from Serialized to a data type", e);
            }
            

            foreach (var aggregate in input)
            {
                result.Add(new Aggregate());

                foreach (var item in aggregate)
                {
                    result[result.Count - 1].Add(item.ToObject<Entity>());
                }
            }

            return result;
        }

        /// <summary>
        /// This takes in the response when it is 'expected' to be returned as List of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns> A list of type <typeparamref name="T"/></returns>
        public static List<T> FromSerializedToObject<T>(this object source)
      where T : class, new()
        {
            List<T> result = new List<T>();

            try
            {
                result = (List<T>)source;
            }
            catch (InvalidCastException e)
            {
                throw new FailedToConvertFromSerialized(" Yeah this isn't the object that you are able to go from Serialized to a data type", e);
            }

            return result;
        }

    }
}
