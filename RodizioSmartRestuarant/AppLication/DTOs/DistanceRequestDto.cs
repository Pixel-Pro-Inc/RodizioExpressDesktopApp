using RodizioSmartRestuarant.Entities;
using System.ComponentModel.DataAnnotations;

namespace RodizioSmartRestuarant.AppLication.DTOs
{
    public class DistanceRequestDto
    {
        [Required]
        public Location FirstLocation { get; set; }
        [Required]
        public Location SecondLocation { get; set; }
    }
}
