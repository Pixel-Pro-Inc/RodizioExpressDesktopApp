using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    [Serializable]
    public class Payments
    {
        public float Cash { get; set; }
        public float Card { get; set; }
        public float Online { get; set; }
        public float EFT { get; set; }
        public float Cheque { get; set; }
        public float MobileMoneyWallet { get; set; }
        public float EWallet { get; set; }
    }
}
