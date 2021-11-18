using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    public class AppUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public bool Developer { get; set; }
        public int Phone { get; set; }
        public string Address { get; set; }
        public bool Admin { get; set; }
        public string branchId { get; set; }
        public string Restuarant { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}