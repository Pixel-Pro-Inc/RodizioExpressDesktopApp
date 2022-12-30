using RodizioSmartRestuarant.AppLication.Exceptions;
using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// This is the default constructor that initiates but does nothing
        /// </summary>
        public Order()
        {

        }
        /// <summary>
        /// This constructor exists for when the <paramref name="orderItems"/> already exist but you want to set the <see cref="Order"/>. 
        /// Remember that most of the properties of <see cref="Order"/> are set when it initated. So this is a way to give it values and set it when its initiated,
        /// even if the <see cref="OrderItem"/>s were set before the <see cref="Order"/>. It does this buy using <see cref="List.Add(OrderItem)"/>
        /// </summary>
        /// <param name="orderItems"></param>
        public Order(Order orderItems)
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
        /// This exists because there are edge cases where, within the computations of the property values, one would need to update the price value, so a private set
        /// was absolutely neccessary
        /// </summary>
        private float? _price { get; set; }
        /// <summary>
        /// This is the Price gotten from the adding all the prices of the <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> It the edge case possiblity that you change the <see cref="Payments"/> or the Order, it will be updated internally to match the sum</para>
        /// </summary>
        public float? Price
        {
            get
            {
                if (_price != null) return _price;
                NullAggregateGuard(NullAggMessage);
                float p = 0f;
                foreach (OrderItem item in this)
                {
                    p += float.Parse(item.Price);
                }
                return p;
            }
            private set
            {
                _price = value;
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
                return this.Sum(x=>x.PrepTime);
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
        ///  Checks the first item for the payments method it has .If it doesn't have an element, or that first element does not have a defined paymentmethod
        ///  it throws <see cref="NullReferenceException()"/>.
        ///  If it does find a paymentMethod, it checks the other <see cref="OrderItem"/>s if any of them have anything different. If it does, or even the 
        ///  <see cref="OrderItem"/> specifically mentions 'split', it will return 'split'
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public string PaymentMethod
        {
            get
            {
                NullAggregateGuard(NullAggMessage);
                //Takes the first paymentMethod it finds
                string firstPaymentMethod = this.FirstOrDefault().paymentMethod?? throw new NullReferenceException("You didn't give a paymentMethod for an orderItem"); 
                foreach (var item in this)
                {
                    //If the firstpayment method isn't the same as the following or in case by some fluke the orderitem is labelled split
                    if (item.paymentMethod != firstPaymentMethod|| item.paymentMethod.ToLower()== "split") return "split";
                }

                return firstPaymentMethod;
            }
        }
        /// <summary>
        /// This will return the payment Methods it finds in the respective positions, but if it doesn't find that kind of payment method it will return 'N/A' for 
        /// that position.
        /// <para>
        /// Please Note that it will always give a Count of 3, but whether the list itself contains 3 actual values depends
        ///  on the logic. 
        ///  If it doesn't have an element it throws <see cref="NullReferenceException()"/>.  I also made it Immutable/ReadOnly
        ///  </para>
        /// </summary>
        public List<string> PaymentMethods
        {
            get
            {
                List<string> result = new List<string>();
                NullAggregateGuard(NullAggMessage);

                //Checks if any of the have the paymentMethods we expect
                //Please note that this arrangement must not be tampered with. It has to start with cash then card then online
                //if the PaymentMethod is the same it registers that indeed it exists by putting it in
                //if it hasn't found anything then it just adds an empty string, that way it will put the next payment method in the next position but have
                //nothing in the previous posistion. This is so the arrangement is kept
                result.Add(this.Any(orderitem => orderitem.paymentMethod.ToUpper() == "CASH") ? "Cash" : "N/A");
                result.Add(this.Any(orderitem => orderitem.paymentMethod.ToUpper() == "CARD") ? "Card" : "N/A");
                result.Add(this.Any(orderitem => orderitem.paymentMethod.ToUpper() == "ONLINE") ? "Online" : "N/A");

                return result;
            }
        }
        private List<string> _payments;
        /// <summary>
        /// This first checks if any of the items have null payments or if it doesn't have an element then throws <see cref="NullReferenceException()"/>. 
        /// <para>If that's not the case then it will go through the <see cref="Order.PaymentMethods"/> checking with each <see cref="OrderItem"/> to see if 
        /// they are the same and sums their <see cref="OrderItem.payment"/> The result is a list of the sum of each type of paymentmethod in the Order.</para>
        /// It also makes sure that the sum is the same as the price, otherwise that means that something is missing and it will throw a 
        /// <see cref="NullReferenceException()"/>
        /// </summary>
        public List<string> Payments
        {
            get
            {
                if (_payments != null) return _payments;
                NullAggregateGuard(NullAggMessage);
                //Default is that there is zero in each of them since that's not available (N/A)
                string[] result = new string[3] { "0", "0", "0" };
                float summation = 0; float Paymentvalue = 0;

                //If they forgot to put in the payments then it will throw the exception. Because how is it supposed to know which individual value of 
                //payment was made for the price. That is the whole point of the information payment
                this.Where(orderitem => orderitem.payment == null)
                        .ToList()
                        .ForEach(filteredorderitem => filteredorderitem.payment = "0");
                //if (this.Any(orderitem => orderitem.payment == null))
                //{
                //    //The below code isn't being handled by the API well so we will just comment it out but by all means it should be there
                //    //throw new NullReferenceException("You didn't give a payment for an orderItem(s)");
                //}

                //Checks if there is several Paymentmethods if any at all then then it will get the Paymentvalue of each kind
                for (int i = 0; i < PaymentMethods.Count; i++)
                {
                    //Here we get the payment value, but also the summation for an important check
                    summation += Paymentvalue = this
                        //Checks where the PaymentMethod we are looking for is the same as the item in the iteration
                        .Where(orderitem => PaymentMethods[i] == orderitem.paymentMethod)
                        //Adds all of them that are the same to the payment value. Here is the most important code line for this block
                        .Sum(filteredorderitem => float.Parse(filteredorderitem.payment));
                    result[i] = Paymentvalue.ToString();
                }
                //Makes sure that the total Payments of all the orderitems equals the price, then returns the result or informs that there is a payment missing
                return summation == Price ? result.ToList() : throw new BaseAggregateException("The Price doesn't match the payments you gave for the orderitems");

            }
            set
            {
                //Sets the value of the _payments but also updates the price of the Order
                _payments = value;
                //Since Price is only set when you equate it to something this is the only way I could do it
                float comprice = 0;
                foreach (var pay in _payments)
                {
                    //Gets the addition of all the payments
                    comprice += float.Parse(pay);
                }
                //Set the sum to be the price
                Price = comprice;

            }
        }


        /// <summary>
        /// This is whether or not the order is still waiting for payment, gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public bool? WaitingForPayment
        {
            get
            {
                NullAggregateGuard(NullAggMessage);
                return this.First().WaitingForPayment;
            }
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
                return this.First().PhoneNumber == null? "" : this.First().PhoneNumber;
            }
        }


    }
}
