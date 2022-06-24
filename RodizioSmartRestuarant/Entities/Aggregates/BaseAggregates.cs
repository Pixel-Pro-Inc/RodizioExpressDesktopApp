using RodizioSmartRestuarant.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities.Aggregates
{
    /// <summary>
    /// This is a class that will define what all aggregates should have, eg Order, Menu
    /// </summary>
    public class BaseAggregates<T> : List<T>, IBaseEntity
    {
    }
}
