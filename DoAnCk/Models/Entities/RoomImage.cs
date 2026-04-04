using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCk.Models.Entities
{
    public class RoomImage
    {
        [Key]
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; } // Ảnh đại diện cho tin đăng

        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }
    }
}
