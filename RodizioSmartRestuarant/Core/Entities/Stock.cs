namespace RodizioSmartRestuarant.Core.Entities
{
    public class Stock: restaurantEntity
    {
        // This what the staff will call float, I assume
        double carryOver { get; set; }
        double inStore { get; set; }

    }
}
