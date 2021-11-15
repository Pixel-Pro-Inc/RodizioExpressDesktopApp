
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    public class Order
    {
        public List<Item> ItemTransactions { get; set; }
        public DateTime Date { get; set; }
        public Customer Customer { get; set; }
        public int Invoice { get; set; }

        public double GrossTotal { get; set; }
        public double Total { get; set; }

    }
}
