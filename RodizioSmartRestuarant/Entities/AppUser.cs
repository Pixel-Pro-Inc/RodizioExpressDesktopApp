using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    public class AppUser:restaurantEntity
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public bool Developer { get; set; }
        public int Phone { get; set; }
        public bool Admin { get; set; }
        public List<string> branchId { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        // Restaurant and address are inherted from restaurantEntity
    }
}