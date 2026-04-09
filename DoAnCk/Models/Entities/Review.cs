using System.ComponentModel.DataAnnotations;

namespace DoAnCk.Models.Entities
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số sao.")]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá.")]
        [StringLength(1000, ErrorMessage = "Nội dung đánh giá không được vượt quá 1000 ký tự.")]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; }

        public virtual User User { get; set; }

        [Required]
        public int RoomId { get; set; }

        public virtual Room Room { get; set; }
    }
}