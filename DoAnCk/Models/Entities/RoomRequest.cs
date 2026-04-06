using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCk.Models.Entities
{
    public class RoomRequest
    {
        [Key]
        public int Id { get; set; }

        // Liên kết đến phòng
        [Required]
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }

        // Người gửi yêu cầu (Khách thuê)
        [Required]
        public int SenderId { get; set; }

        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lời nhắn giới thiệu.")]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        // Ghi chú riêng của Chủ trọ (Dùng để phản hồi hoặc note nội bộ)
        // Đây là trường bạn đang thiếu gây ra lỗi trong Controller
        [StringLength(500)]
        public string? Note { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public RequestStatus Status { get; set; } = RequestStatus.Pending;
    }

    public enum RequestStatus
    {
        [Display(Name = "Đang chờ")]
        Pending = 0,

        [Display(Name = "Đã chấp nhận")]
        Accepted = 1,

        [Display(Name = "Đã từ chối")]
        Rejected = 2
    }
}