using DoAnCk.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace DoAnCk.Models.Entities
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } // Ví dụ: Ở ghép, Phòng trọ độc lập, Căn hộ mini

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}