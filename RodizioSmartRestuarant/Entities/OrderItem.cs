using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    [Serializable]
    public class OrderItem
    {
        /// <summary>
        /// Unique Identifier for each order in DB.
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Index of order item in order list
        /// </summary>
        public int Index { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; } = "";
        public string Description { get; set; }
        public string Reference { get; set; }
        public string Price { get; set; } = "0";
        //To allow customers to use multiple payment methods 
        public bool SplitPayment { get; set; } = false;
        //In the Kernel it doesn't have a default because we want it to throw a NullException. But I think we can start to do away with it
        public string paymentMethod { get; set; } = "N/A";
        public string payment { get; set; } = "0";
        //End
        public string Weight { get; set; } = "0";
        public bool Fufilled { get; set; }
        public bool Purchased { get; set; } = true;//Defaulted to true because offline orders have to be paid for before they are made
        public string PaymentMethod { get; set; } 
        public bool Preparable { get; set; }
        public bool WaitingForPayment { get; set; }
        public int Quantity { get; set; }
        public string PhoneNumber { get; set; }
        //A string in the form of Date_4 digit number e.g 08-11-2021_4927
        public string OrderNumber { get; set; }
        public bool Collected { get; set; } = false;
        public bool MarkedForDeletion { get; set; } = false;

        /// <summary>
        /// This is the time the Order was complete. So we can find out how long orders take to be prepared
        /// </summary>
        public DateTime OrderCompletionTime { get; set; }
        public DateTime OrderDateTime { get; set; }
        public string User { get; set; }
        private int _preptime { get; set; } = 0;
        /// <summary>
        /// The prepTime is now the difference between when the order was made and when it was marked as complete. When it is marked as complete is up to the POS
        /// </summary>
        public int PrepTime
        {
            get
            {
                //if the actual preptime assessed when order is paid is more than the value the menuitem gives then it will return that value
                return _preptime<(OrderCompletionTime - OrderDateTime).Minutes? (OrderCompletionTime - OrderDateTime).Minutes : _preptime;
            }
            set
            {
                _preptime = value;
            }
        }
        public List<string> Chefs { get; set; }
        //New Additions
        public string Flavour { get; set; }
        public string MeatTemperature { get; set; }
        public List<string> Sauces { get; set; }

        //Apparently these are already defined somewhere except I can't find where so i just commented them out
        //New Additions
        //public string SubCategory { get; set; }
        ////To allow customers to use multiple payment methods 
        //public bool SplitPayment { get; set; } = false;
        //public List<string> paymentMethod { get; set; } = new List<string>();
        //public List<string> payment { get; set; } = new List<string>();
    }
}
