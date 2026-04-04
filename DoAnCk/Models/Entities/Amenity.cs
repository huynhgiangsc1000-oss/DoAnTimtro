using System.ComponentModel.DataAnnotations;

namespace DoAnCk.Models.Entities
{
    public class Amenity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } // Wifi, Máy giặt, Điều hòa...
        public string? IconClass { get; set; } // FontAwesome class (ví dụ: fa-wifi)
    }
}