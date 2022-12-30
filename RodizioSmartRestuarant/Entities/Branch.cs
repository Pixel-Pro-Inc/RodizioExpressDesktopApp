using System;
using System.Collections.Generic;

namespace RodizioSmartRestuarant.Entities
{
    [Serializable]
    public class Branch
    {
        public string BranchId { get; set; }
        public string Name { get; set; }
        public string ImgUrl { get; set; }
        public string PublicId { get; set; }
        public DateTime LastActive { get; set; }
        public List<int> PhoneNumbers { get; set; }
        // REFACTOR: Consider having a dictionary here so that we can remove the ClosingTime class
        public List<DateTime> OpeningTimes { get; set; }
        public List<DateTime> ClosingTimes { get; set; }
        //Localization Info
        public Location Location { get; set; }
        public string Currency { get; set; }
        public int TimeZone { get; set; }
    }
}
