using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RodizioSmartRestuarant.Entities
{
[Serializable]
    public class MenuItem
    {
        /// <summary>
        /// Unique Identifier for each order in DB.
        /// </summary>
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string Weight { get; set; }
        public string ImgUrl { get; set; }
        public string PublicId { get; set; }
        public string prepTime { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public float MinimumPrice { get; set; }
        public float Rate { get; set; }
        public bool Availability { get; set; }
        //new Additions
        public List<string> Flavours { get; set; }
        public List<string> MeatTemperatures { get; set; }
        public List<string> Sauces { get; set; }
    }
}