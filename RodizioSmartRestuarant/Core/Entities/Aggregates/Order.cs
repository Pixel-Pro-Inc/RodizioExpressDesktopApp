using System;
using System.Linq;

namespace RodizioSmartRestuarant.Core.Entities.Aggregates
{
    /// <summary>
    /// This is an aggregate of OrderItems, ( a list of OrderItems). It inherits from <see cref="BaseAggregates{T}"/>
    /// <para> We needed this for a long while but finally was pushed to make it when Abel had to convert object to json in
    /// the extention but the List of List of orderitem  wasn't really working nice with single items like a list of appusers</para>
    /// </summary>
    [Serializable]
    public class Order : BaseAggregates<OrderItem>
    {

        public override string ToString()
        {
            string orderItems = null;
            foreach (var orderItem in this)
            {
                orderItems = orderItem.Name + " Identified with " + orderItem.Id.ToString() + "\n"+" Costing:"+Price.ToString();
            }
            return orderItems;
        }

        // REFACTOR: Slowly but surely we will phase away the orderItems ability to have these following properties unless EXPLLICITLY needed for critical case

        private string _orderNumber { get; set; }

        /// <summary>
        /// This is the ordernumber gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public string OrderNumber
        {
            get { return _orderNumber; }
            private set { }
        }

       // public AggregateProp<string> OrderNumber1 { get { return _orderNumber; } set { _orderNumber = this.First().OrderNumber; } } 
        private string _user
        {
            get { return _user == null ? throw new NullReferenceException("There are no OrderItems in this Order") : _user; }
            set
            {
                if (this.Any())
                {
                    _user = this.First().User;
                }
            }
        }
        /// <summary>
        /// This is the User as a string (Their name) gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public string User
        {
            get { return _user; }
            private set { }
        }

        private string _reference
        {
            get { return _reference == null ? throw new NullReferenceException("There are no OrderItems in this Order") : _reference; }
            set
            {
                if (this.Any())
                {
                    _reference = this.First().Reference;
                }
            }
        }
        /// <summary>
        /// This is the Reference (Where the <see cref="Order"/> came from) gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public string Reference
        {
            get { return _reference; }
            private set { }
        }

        private DateTime _orderDateTime
        {
            get { return _orderDateTime == null ? throw new NullReferenceException("There are no OrderItems in this Order") : _orderDateTime; }
            set
            {
                if (this.Any())
                {
                    _orderDateTime = this.First().OrderDateTime;
                }
            }
        }
        /// <summary>
        /// This is the <see cref="DateTime"/> gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public DateTime OrderDateTime
        {
            get { return _orderDateTime; }
            private set { }
        }

        private float? _price { get; set; }
        /// <summary>
        /// This is the Price gotten from the adding all the prices of the <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public float? Price
        {
            get { return _price; }
            // You need to use net. 6.0 . That way you can use the keyword init. You can only use it when you upgrade to vs 22
            private set
            {
                // This is to add the price of every orderItem in the Order using the Aggregate method
                _price=this.Aggregate(0f, (_price, place2) => _price + float.Parse(place2.Price));
            }
        }


        private int? _id
        {
            get { return _id==null ? throw new NullReferenceException("There are no OrderItems in this Order"):_id; }
            set
            {
                if (this.Any())
                {
                    _id = this.First().Id;
                }
            }
        }
        /// <summary>
        /// This is the id gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public int? Id
        {
            get { return _id; }
            private set { }
        }

        private int? _prepTime
        {
            get { return _prepTime == null ? throw new NullReferenceException("There are no OrderItems in this Order") : _prepTime; }
            set
            {
                if (this.Any())
                {
                    _prepTime = this.First().PrepTime;
                }
            }
        }
        /// <summary>
        /// This is the <see cref="PrepTime"/> gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public int? PrepTime
        {
            get { return _prepTime; }
            private set { }
        }

        private bool? _purchased
        {
            get { return _purchased == null ? throw new NullReferenceException("There are no OrderItems in this Order") : _purchased; }
            set
            {
                if (this.Any())
                {
                    _purchased = this.First().Purchased;
                }
            }
        }
        /// <summary>
        /// This is whether or not the order has been purchased gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public bool? Purchased
        {
            get { return _purchased; }
            private set { }
        }

        private string _paymentMethod { get; set; }
        /// <summary>
        /// This is the payment method gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public string PaymentMethod
        {
            get { return _paymentMethod == null ? throw new NullReferenceException("There are no OrderItems in this Order") : _paymentMethod; }
            // FIXME: This should be a private init not set, but we need VS 22 for that
            private set
            {
                if (this.Any())
                {
                    _paymentMethod = this.First().PaymentMethod;
                }
            }
        }

        private bool? _waitingForPayment
        {
            get { return _waitingForPayment == null ? throw new NullReferenceException("There are no OrderItems in this Order") : _waitingForPayment; }
            set
            {
                if (this.Any())
                {
                    _waitingForPayment = this.First().WaitingForPayment;
                }
            }
        }
        /// <summary>
        /// This is whether or not the order is still waiting for payment, gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public bool? WaitingForPayment
        {
            get { return _waitingForPayment; }
            private set { }
        }


        private bool? _collected
        {
            get { return _collected == null ? throw new NullReferenceException("There are no OrderItems in this Order") : _collected; }
            set
            {
                if (this.Any())
                {
                    _collected = this.First().Collected;
                }
            }
        }
        /// <summary>
        /// This is whether or not the order is collected, gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public bool? Collected
        {
            get { return _collected; }
            private set { }
        }

        private bool? _markedForDeletion
        {
            get { return _markedForDeletion == null ? throw new NullReferenceException("There are no OrderItems in this Order") : _markedForDeletion; }
            set
            {
                if (this.Any())
                {
                    _markedForDeletion = this.First().MarkedForDeletion;
                }
            }
        }
        /// <summary>
        /// This is whether or not the order is about to be deleted, gotten from the first element of <see cref="OrderItem"/>s it has. If it doesn't have an element it throws <see cref="NullReferenceException()"/>
        /// <para> I also made it Immutable/ReadOnly</para>
        /// </summary>
        public bool? MarkedForDeletion
        {
            get { return _markedForDeletion; }
            private set { }
        }


    }

}
