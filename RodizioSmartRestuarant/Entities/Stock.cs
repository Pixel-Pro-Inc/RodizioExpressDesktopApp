using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    public class Stock: restaurantEntity
    {
        // This what the staff will call float, I assume
        double carryOver { get; set; }
        double inStore { get; set; }

    }
}
