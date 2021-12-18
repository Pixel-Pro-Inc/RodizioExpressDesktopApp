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
        public bool Preparable { get; set; }
        public bool WaitingForPayment { get; set; }
        public int Quantity { get; set; }
        public int PhoneNumber { get; set; }
        public string OrderNumber { get; set; }

        public List<MenuItem> ItemTransactions { get; set; }
        public DateTime Date { get; set; }
        public AppUser Customer { get; set; } // I am not sure any more how the customer is going to be involved, Like do they ever log on, where is this info set
        public AppUser employee { get; set; }
        public int Invoice { get; set; }

        public double GrossTotal { get; set; }
        public double Total { get; set; }

        public void ApplyDiscount(string promoCode) => Total *= 1+Waiver.GetpromoPercent(promoCode);
        public void ApplyDiscount(double amount)=> Total -= LocalStorage.Instance.user == employee ? amount : 0;
        public void RemoveDiscount(string promoCode) => Total /= 1 + Waiver.GetpromoPercent(promoCode);
        public void RemoveDiscount(double amount) => Total += Int32.Parse(Price) * Quantity >= Total + amount ? amount : 0;

    }
}
