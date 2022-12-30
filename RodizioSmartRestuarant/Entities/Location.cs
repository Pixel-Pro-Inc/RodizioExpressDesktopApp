using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    [Serializable]
    public class Location
    {
        /// <summary>
        /// Index of location in list.
        /// </summary>
        public int Index { get; set; }
        public string AddressName { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string PhysicalAddress { get; set; }
        public int DefaultZoomLevel { get; set; }
        public string ExtraAddressInfo { get; set; }
    }
}
