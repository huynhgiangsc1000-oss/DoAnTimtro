using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCk.Models.Entities
{
    public class RoomRequest
    {
        [Key]
        public int Id { get; set; }

        // Khóa ngoại liên kết tới bảng Room (vẫn giữ int nếu RoomId là int)
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        // SỬA TẠI ĐÂY: Đổi sang string để khớp với IdentityUser mặc định (GUID)
        [Required]
        public string SenderId { get; set; }

        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lời nhắn giới thiệu.")]
        [StringLength(500)]
        public string Message { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public RequestStatus Status { get; set; } = RequestStatus.Pending;
    }

    public enum RequestStatus
    {
        Pending,   // Đang chờ
        Accepted,  // Đã chấp nhận
        Rejected   // Đã từ chối
    }
}