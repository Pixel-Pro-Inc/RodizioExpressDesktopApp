using System;
using System.Collections.Generic;

namespace RodizioSmartRestuarant.Entities
{
    [Serializable]
    public class Customer
    {
        public string PhoneNumber { get; set; }
        public byte[] Cards { get; set; }
        public List<Location> Locations { get; set; } = new List<Location>();
    }
}
