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
        public string? Title { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public double Area { get; set; } // Diện tích m2

        public string Address { get; set; }

        // Tọa độ Google Maps
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false; // Trạng thái duyệt tin

        // Foreign Keys
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        // Navigation Properties
        public virtual ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();
        public virtual ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}