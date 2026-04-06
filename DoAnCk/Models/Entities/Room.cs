using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCk.Models.Entities
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public double Area { get; set; }

        public string Address { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // --- BỔ SUNG: TRẠNG THÁI CHI TIẾT ---
        // Thay vì chỉ dùng IsApproved, dùng Status để quản lý: 0: Chờ duyệt, 1: Đang hiển thị, 2: Đã cho thuê
        public int Status { get; set; } = 0;

        public bool IsApproved { get; set; } = false;

        // --- BỔ SUNG: THỐNG KÊ TƯƠNG TÁC ---
        // Lưu trữ số lượt xem để chủ trọ theo dõi hiệu quả bài đăng
        public int ViewCount { get; set; } = 0;

        // --- BỔ SUNG: THÔNG TIN LIÊN HỆ NHANH ---
        // Hỗ trợ tính năng nút gọi và Zalo trên giao diện
        public string? PhoneNumber { get; set; }
        public string? ZaloNumber { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;

        public virtual ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();

        public virtual ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        // --- BỔ SUNG: QUAN HỆ VỚI YÊU CẦU THUÊ ---
        // Để đếm số thông báo trên Navbar chính xác
        public virtual ICollection<RoomRequest> RoomRequests { get; set; } = new List<RoomRequest>();
    }
}