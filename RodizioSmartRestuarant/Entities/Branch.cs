using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
    public class Branch:restaurantEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ImgUrl { get; set; }
        public string PublicId { get; set; }
        public DateTime LastActive { get; set; }
    }
}
