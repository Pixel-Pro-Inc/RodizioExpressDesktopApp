using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Helpers
{
    public class LocalStorage
    {
        public static LocalStorage Instance { get; set; }
        public AppUser user { get; set; }

        public LocalStorage()
        {
            Instance = this;
        }
    }
}
