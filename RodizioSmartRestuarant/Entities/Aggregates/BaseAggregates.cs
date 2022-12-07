using RodizioSmartRestuarant.AppLication.Interfaces.BaseInterfaces;
using System;
using System.Collections.Generic;

namespace RodizioSmartRestuarant.Core.Entities.Aggregates
{
    /// <summary>
    /// This is a class that will define what all aggregates should have, eg Order.
    /// All Aggregates have the ability of <see cref="List{T}"/> as it inherits from it
    /// </summary>
    public class BaseAggregates<T>: List<T>, IBaseAggregate
    {
        /// <summary>
        /// This is called in every aggregate property. If there is no <see cref="T"/> in the <see cref="BaseAggregates{T}"/> it throws a <see cref="NullReferenceException()"/>
        /// <para> It does this by asking if !this.Any() throw the exception</para>
        /// </summary>
        protected void NullAggregateGuard(string message)
        {
            //I'm assuming this is the same as Any() but since we working with an older version of C# this will have to do
            if (Exists(x=> x!=null)) throw new NullReferenceException(message);
        }

    }
}
