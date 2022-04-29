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
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public string Price { get; set; }
        public string Weight { get; set; }
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
        public DateTime OrderDateTime { get; set; }
        public string User { get; set; }
        public int PrepTime { get; set; }
        public List<string> Chefs { get; set; }
        //New Additions
        public string Flavour { get; set; }
        public string MeatTemperature { get; set; }
        public List<string> Sauces { get; set; }
    }
}

namespace RdKitchenApp.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public string Price { get; set; }
        public string Weight { get; set; }
        public bool Fufilled { get; set; }
        public bool Purchased { get; set; }
        public string PaymentMethod { get; set; }
        public bool Preparable { get; set; }
        public bool WaitingForPayment { get; set; }
        public int Quantity { get; set; }
        public string OrderNumber { get; set; }
        public bool Collected { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime OrderDateTime { get; set; }
        public string User { get; set; }
        public int PrepTime { get; set; }
    }
}
