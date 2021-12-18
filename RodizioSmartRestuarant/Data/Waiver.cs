using RodizioSmartRestuarant.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Data
{
    public static class Waiver
    {

        static Dictionary<string, float> promosheet = new Dictionary<string, float>(); //this is a key for the promo code and value of the percent in the range 1-0

        public static float GetpromoPercent(string promo) => promosheet[promo];
        public static void Setpromocode(string name, float percent, AppUser creator)
        {
            if (creator.Admin)
            {
                promosheet.Add(name, percent);
            }
        }

    }
}
