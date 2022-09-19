namespace RodizioSmartRestuarant.Core.Entities
{
    public class restaurantEntity:BaseEntity
    {
        public string Restaurant { get; set; }
        public string Address { get; set; }
        public string Manager { get; set; }
        public string RestaurantPhoneNumber { get; set; }
    }
}
