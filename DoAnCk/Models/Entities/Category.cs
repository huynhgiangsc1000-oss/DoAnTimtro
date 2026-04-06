using DoAnCk.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace DoAnCk.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Thêm hoặc sửa lại tên thuộc tính này cho đúng
        public string? Description { get; set; }

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}