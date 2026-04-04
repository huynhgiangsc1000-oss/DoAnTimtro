using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoAnCk.Data
{
    // Sử dụng Role (class tự tạo) thay vì IdentityRole để đồng bộ hệ thống
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<RoomAmenity> RoomAmenities { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<RoomRequest> RoomRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình cho RoomRequest (Yêu cầu ở ghép) - FIX LỖI HIỆN TẠI
            modelBuilder.Entity<RoomRequest>()
                .HasOne(rr => rr.Room)
                .WithMany() // Một phòng có thể có nhiều yêu cầu
                .HasForeignKey(rr => rr.RoomId)
                .OnDelete(DeleteBehavior.Restrict); // Tắt xóa tự động khi xóa Room

            modelBuilder.Entity<RoomRequest>()
                .HasOne(rr => rr.Sender)
                .WithMany() // Một người dùng có thể gửi nhiều yêu cầu
                .HasForeignKey(rr => rr.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Tắt xóa tự động khi xóa User (Sender)

            // 2. Cấu hình cho Favorites (Để tránh lỗi tương tự ở bảng này)
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Room)
                .WithMany()
                .HasForeignKey(f => f.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Cấu hình cho Review (Đánh giá)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Cấu hình khóa chính cho RoomAmenity (Giữ nguyên cái cũ của bạn)
            modelBuilder.Entity<RoomAmenity>()
                .HasKey(ra => new { ra.RoomId, ra.AmenityId });
        }
    }
}