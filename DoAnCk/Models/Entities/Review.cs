using DoAnCk.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace DoAnCk.Models.Entities
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        public int Rating { get; set; } // 1-5 sao
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int RoomId { get; set; }
        public virtual Room Room { get; set; }
    }
}