using DoAnCk.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DoAnCk.Controllers
{
    [AllowAnonymous] // Cho phép khách chưa đăng nhập truy cập
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action dùng chung cho Khách và Customer
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

            // Logic: Chỉ cho xem nếu phòng đã được duyệt (Status = 1)
            // Hoặc nếu là chủ phòng/Admin thì vẫn cho xem (tùy bạn cấu hình)
            return View(room);
        }
    }
}