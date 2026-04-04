using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCk.Models.Entities
{
    public class RoomRequest
    {
        [Key]
        public int Id { get; set; }

        public int RoomId { get; set; }
        public virtual Room Room { get; set; }

        public int SenderId { get; set; } // Người gửi lời mời ở ghép
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }

        public string Message { get; set; } // Lời nhắn giới thiệu bản thân
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
    }

    public enum RequestStatus { Pending, Accepted, Rejected }
}
