using DoAnCk.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace DoAnCk.Models.Entities
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public int RoomId { get; set; }
        public virtual Room Room { get; set; }
    }
}