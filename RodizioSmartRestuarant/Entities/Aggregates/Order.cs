using RodizioSmartRestuarant.AppLication.Exceptions;
using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RodizioSmartRestuarant.Core.Entities.Aggregates
{
    /// <summary>
    /// This is an aggregate of OrderItems, ( a list of OrderItems). It inherits from <see cref="BaseAggregates{T}"/>
    /// <para> We needed this for a long while but finally was pushed to make it when Abel had to convert object to json in
    /// the extention but the List of List of orderitem  wasn't really working nice with single items like a list of appusers</para>
    /// </summary>
    public class Order : BaseAggregates<OrderItem>
    {
        /// <summary>
        /// We were forced by the Newton.json converter to add this constructor back. If you are using this constructor yourself, shame on you. Stop that
        /// </summary>
        public Order() { }
        /// <summary>
        /// This constructor exists for when the <paramref name="orderItems"/> already exist but you want to set the <see cref="Order"/>. 
        /// Remember that most of the properties of <see cref="Order"/> are set when it initated. So this is a way to give it values and set it when its initiated,
        /// even if the <see cref="OrderItem"/>s were set before the <see cref="Order"/>. It does this buy using <see cref="List.Add(OrderItem)"/>
        /// </summary>
        /// <param name="orderItems"></param>
        public Order(List<OrderItem> orderItems)
        {
            //This is to populate the order durning intiantation.
            foreach (OrderItem item in orderItems)
            {
                Add(item);
            }
        }

        /// <summary>
        /// This is the message that the <see cref="NullReferenceException()"/> will have if there are no <see cref="OrderItem"/>s in the aggregate
        /// </summary>
        readonly string NullAggMessage = "There are no OrderItems in this Order";
        /// <summary>
        /// Get's properties of each <see cref="OrderItem"/> in the <see cref="Order"/> to make list to return
        /// </summary>
        /// <returns>
        ///  This gives a list of all the <see cref="OrderItem"/>s in the <see cref="Order"/> with the id and price included
        /// </returns>
        public override string ToString()
        {
            string orderItems = "";
            foreach (var orderItem in this)
            {
                orderItems = orderItem.Name + " IdentityFied with " + orderItem.Index.ToString() + "\n" + " Costing:" + Price.ToString() + "\n\n";
            }
            return orderItems;
        }


        /// <summary>
        /// This is the ordernumber gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        ///
        public string OrderNumber
        {
            get
            {
                NullAggregateGuard(NullAggMessage);
                return this.First().OrderNumber;
            }
        }

        /// <summary>
        /// This is the User as a string (Their name) gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public string User
        {
            get
            {
                NullAggregateGuard(NullAggMessage);
                return this.First().User;
            }
        }

        /// <summary>
        /// This is the Reference (Where the <see cref="Order"/> came from) gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public string Reference
        {
            get
            {
                NullAggregateGuard(NullAggMessage);
                return this.First().Reference;
            }
        }

        /// <summary>
        /// This is the <see cref="DateTime"/> gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public DateTime? OrderDateTime
        {
            get
            {
                NullAggregateGuard(NullAggMessage);
                return this.First().OrderDateTime;
            }
        }

        /// <summary>
        /// This is the Price gotten from the adding all the prices of the <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> It the edge case possiblity that you change the <see cref="Payments"/> or the Order, it will be updated internally to match the sum</para>
        /// </summary>
        public float? Price
        {
            get
            {
                NullAggregateGuard(NullAggMessage);
                float p = 0f;
                foreach (OrderItem item in this)
                {
                    p += float.Parse(item.Price);
                }
                return p;
            }
        }

        /// <summary>
        /// This is the id gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public string ID
        {
            get
            {
                NullAggregateGuard(NullAggMessage);
                return this.First().ID;
            }
        }

        /// <summary>
        /// This is the <see cref="PrepTime"/> gotten from the aggregate of.the <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public int? PrepTime
        {
            get
            {
                NullAggregateGuard(NullAggMessage);
                return this.Sum(x => x.PrepTime);
            }
        }

        /// <summary>
        /// This is whether or not the order has been purchased gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public bool? Purchased
        {
            get
            {
                NullAggregateGuard(NullAggMessage);
                return this.First().Purchased;
            }
        }

        /// <summary>
        ///  Summary of payments made to satisfy this order.
        /// </summary>
        public string PaymentMethod
        {
            get
            {
                NullAggregateGuard(NullAggMessage);

                return GetOrderPaymentSummary(this.First().OrderPayments);
            }
        }

        private string GetOrderPaymentSummary(Payments payments)
        {
            if (payments == null)
                return "Awaiting Payment";

            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

            var dictionary = payments.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(payments, null)
            );

            string paymentsSummary = "";

            foreach (var keyValuePair in dictionary)
            {
                if (keyValuePair.Value == null)
                    continue;

                if ((float)keyValuePair.Value == 0)
                    continue;

                if (!string.IsNullOrEmpty(paymentsSummary))
                {
                    paymentsSummary += $", {keyValuePair.Key}";
                    continue;
                }

                paymentsSummary += keyValuePair.Key;
            }

            return paymentsSummary;
        }

        /// <summary>
        /// This is whether or not the order is collected, gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public bool? Collected
        {
            get
            {
                NullAggregateGuard(NullAggMessage);
                return this.First().Collected;
            }
        }

        /// <summary>
        /// This is the phone number used to make the order, gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public string PhoneNumber
        {
            get
            {
                NullAggregateGuard(NullAggMessage);
                return this.First().PhoneNumber == null ? "" : this.First().PhoneNumber;
            }
        }

    }
}
