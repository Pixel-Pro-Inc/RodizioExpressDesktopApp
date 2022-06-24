using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities.Aggregates
{
    /// <summary>
    /// This is an aggregate of OrderItems, ( a list of OrderItems). It inherits from <see cref="BaseAggregates{T}"/>
    /// <para> We needed this for a long while but finally was pushed to make it when Abel had to convert object to json in
    /// the extention but the List of List of orderitem  wasn't really working nice with single items like a list of appusers</para>
    /// </summary>
    public class Order : BaseAggregates<OrderItem>
    {
        // Refactor this so that when we need the orderItems details we can get it easier, cause as it stands its a waste
        public override string ToString()
        {
            string orderItems = null;
            foreach (var orderItem in this)
            {
                orderItems = orderItem.Name + " " + orderItem.Id.ToString() + "\n";
            }
            return orderItems;
        }
    }

}
