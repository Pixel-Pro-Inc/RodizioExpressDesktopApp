using System;
using System.Collections.Generic;

namespace RodizioSmartRestuarant.Entities
{
    [Serializable]
    public class Branch
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string ImgUrl { get; set; }
        public string PublicId { get; set; }
        public DateTime LastActive { get; set; }
        public List<int> PhoneNumbers { get; set; }
        public List<DateTime> OpeningTimes { get; set; }
        public List<DateTime> ClosingTimes { get; set; }
    }
}
