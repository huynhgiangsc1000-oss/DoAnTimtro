using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace DoAnCk.Models.Entities
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; }
        public string? Bio { get; set; } // Giới thiệu bản thân để tìm người ở ghép
        public string? AvatarUrl { get; set; }

        // Quan hệ
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}