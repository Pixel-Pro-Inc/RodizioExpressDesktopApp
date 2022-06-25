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

        private string _orderNumber { get; set; }
        /// <summary>
        /// This is the ordernumber gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// </summary>
        public string OrderNumber 
        { 
            get{ return _orderNumber; }
            set
            {
                if (this.Any())
                {
                    _orderNumber=this.First().OrderNumber;
                }
                else
                {
                    new NullReferenceException("There are no OrderItems in this Order");
                }
            } 
        }

        private DateTime _orderDateTime { get; set; }
        /// <summary>
        /// This is the <see cref="DateTime"/> gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// </summary>
        public DateTime OrderDateTime 
        {
            get { return _orderDateTime; }
            set
            {
                if (this.Any())
                {
                    _orderDateTime = this.First().OrderDateTime;
                }
                else
                {
                    new NullReferenceException("There are no OrderItems in this Order");
                }
            }
        }

        private float _price { get; set; }
        /// <summary>
        /// This is the Price gotten from the adding all the prices of the <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// </summary>
        public float Price
        {
            get { return _price; }
            set
            {
                if (this.Any())
                {
                    foreach (OrderItem item in this)
                    {
                        _price = +float.Parse(item.Price);
                    }
                }
                else
                {
                    new NullReferenceException("There are no OrderItems in this Order");
                }
            }
        }

        public int _id
        {
            get { return _id; }
            set
            {
                if (this.Any())
                {
                    _id = this.First().Id;
                }
                else
                {
                    new NullReferenceException("There are no OrderItems in this Order");
                }
            }
        }
        /// <summary>
        /// This is the id gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// </summary>
        public int Id
        {
            get { return _id; }
            set
            {
                if (this.Any())
                {
                    _id = this.First().Id;
                }
                else
                {
                    new NullReferenceException("There are no OrderItems in this Order");
                }
            }
        }

        

    }

}
