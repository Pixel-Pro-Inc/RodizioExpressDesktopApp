using System;
using System.Collections.Generic;

namespace RodizioSmartRestuarant.Entities
{
    [Serializable]
    public class Branch
    {
        public string BranchId { get; set; }
        public string Name { get; set; }
        public string ImgUrl { get; set; }
        public string PublicId { get; set; }
        public DateTime LastActive { get; set; }
        public List<int> PhoneNumbers { get; set; }
        // REFACTOR: Consider having a dictionary here so that we can remove the ClosingTime class
        public List<DateTime> OpeningTimes { get; set; }
        public List<DateTime> ClosingTimes { get; set; }
        //Localization and Personalization Info
        public Location Location { get; set; }
        public string Currency { get; set; }
        public int TimeZone { get; set; }
        //This is the minimum order amount on which we will deliver food
        public float MinimumDeliveryAmount { get; set; }
        //We have to set a minimum amount per branch
        //because banks charge a fee per transaction
        //and its smart to limit those to larger orders
        /// <summary>
        /// This applies for both card payment and online payment
        /// </summary>
        public float MinimumCardPaymentAmount { get; set; }
        /// <summary>
        /// The maximum distance an order for delivery can be placed in km
        /// </summary>
        public float DeliveryRadius { get; set; }
    }
}
