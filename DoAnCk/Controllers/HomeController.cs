using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCk.Data;
using DoAnCk.Models;
using DoAnCk.Models.Entities;

namespace DoAnCk.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Hiển thị danh sách phòng trọ mới nhất tại trang chủ
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy danh sách phòng, Include bảng Images để hiển thị ảnh đại diện
                // Sử dụng .AsNoTracking() để tăng tốc độ load trang cho dữ liệu chỉ đọc
                var rooms = await _context.Rooms
                    .Include(r => r.RoomImages)
                    .Include(r => r.Category) // Lấy thông tin danh mục nếu cần hiển thị
                    .OrderByDescending(r => r.CreatedDate)
                    .Take(12) // Giới hạn hiển thị 12 phòng mới nhất
                    .AsNoTracking()
                    .ToListAsync();

                return View(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách phòng trọ tại trang chủ.");
                return View(new List<Room>());
            }
        }

        // Xem chi tiết một phòng trọ
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.RoomImages)
                .Include(r => r.User)
                .Include(r => r.Category)
                // SỬA DÒNG NÀY: Thay Amenities bằng RoomAmenities
                .Include(r => r.RoomAmenities)
                    .ThenInclude(ra => ra.Amenity) // Lấy thêm thông tin chi tiết của tiện ích
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }
    }
}