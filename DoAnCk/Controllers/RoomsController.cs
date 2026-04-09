using DoAnCk.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DoAnCk.Controllers
{
    [AllowAnonymous]
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // TRANG HIỂN THỊ TẤT CẢ PHÒNG TRỌ
        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms
                .Include(r => r.RoomImages)
                .Include(r => r.Category)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(rooms);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.RoomImages)
                .Include(r => r.RoomAmenities).ThenInclude(ra => ra.Amenity)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null) return NotFound();

            return View(room);
        }

        public async Task<IActionResult> ByDistrict(string district)
        {
            if (string.IsNullOrWhiteSpace(district))
            {
                return RedirectToAction("Index", "Home");
            }

            var rooms = await _context.Rooms
                .Include(r => r.RoomImages)
                .Include(r => r.Category)
                .Where(r => r.District != null && r.District == district)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            ViewBag.District = district;
            return View(rooms);
        }
    }
}