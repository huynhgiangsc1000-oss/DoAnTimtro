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
        public async Task<IActionResult> Index(string district)
        {
            // Lấy tất cả phòng đã duyệt
            var query = _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.RoomImages)
                .Where(r => r.IsApproved == true)
                .AsQueryable();

            // Nếu người dùng chọn Quận, thực hiện lọc theo địa chỉ
            if (!string.IsNullOrEmpty(district))
            {
                query = query.Where(r => r.Address.Contains(district));
            }

            var rooms = await query.OrderByDescending(r => r.CreatedDate).ToListAsync();

            return View(rooms);
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