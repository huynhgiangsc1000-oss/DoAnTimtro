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
        public async Task<IActionResult> Index(string district, int? categoryId, string price)
        {
            // Lấy danh mục cho dropdown (như bước trước)
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            var query = _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.RoomImages)
                .Where(r => r.IsApproved == true)
                .AsQueryable();

            // 1. Lọc theo Quận
            if (!string.IsNullOrEmpty(district))
            {
                query = query.Where(r => r.Address.Contains(district));
            }

            // 2. Lọc theo Loại phòng
            if (categoryId.HasValue)
            {
                query = query.Where(r => r.CategoryId == categoryId.Value);
            }

            // 3. Lọc theo Mức giá (Logic mới)
            if (!string.IsNullOrEmpty(price))
            {
                switch (price)
                {
                    case "duoi-3":
                        query = query.Where(r => r.Price < 3000000);
                        break;
                    case "3-5":
                        query = query.Where(r => r.Price >= 3000000 && r.Price <= 5000000);
                        break;
                    case "5-7":
                        query = query.Where(r => r.Price >= 5000000 && r.Price <= 7000000);
                        break;
                    case "tren-7":
                        query = query.Where(r => r.Price > 7000000);
                        break;
                }
            }

            var rooms = await query.OrderByDescending(r => r.CreatedDate).ToListAsync();
            return View(rooms);
        }
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