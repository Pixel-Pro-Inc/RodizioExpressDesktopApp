using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    public class Stock: restaurantEntity
    {
        double carryOver { get; set; }
        double inStore { get; set; }

    }
}
