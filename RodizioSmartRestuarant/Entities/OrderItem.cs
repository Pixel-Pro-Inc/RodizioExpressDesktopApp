using RodizioSmartRestuarant.Data;
using RodizioSmartRestuarant.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    public class OrderItem
    {
        public OrderItem()
        {
            this.employee = LocalStorage.Instance.user; //so that who ever is logged in is instantly set to be the one serving the customer
        }
        public int Id { get; set; }
        public string Name { get; set; }
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
        public string PhoneNumber { get; set; }
        //A string in the form of Date_4 digit number e.g 08-11-2021_4927
        public string OrderNumber { get; set; }
        public bool Collected { get;  set; }

        public AppUser employee { get; set; }
       

    }
}
