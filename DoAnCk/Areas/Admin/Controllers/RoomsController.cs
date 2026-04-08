using DoAnCk.Data;
using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnCk.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang quản lý chính của Admin
        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.User)
                .Include(r => r.RoomImages)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
            return View(rooms);
        }

        // --- BỔ SUNG: XEM CHI TIẾT PHÒNG (Dành cho Admin) ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.User)
                .Include(r => r.RoomImages)
                .Include(r => r.RoomAmenities).ThenInclude(ra => ra.Amenity)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // Chức năng Duyệt tin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            room.IsApproved = true;
            room.Status = 1; // Chuyển sang trạng thái 1: "Đang hiển thị"

            _context.Update(room);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã phê duyệt tin đăng!";
            return RedirectToAction(nameof(Index));
        }

        // Chức năng Từ chối/Gỡ tin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            room.IsApproved = false;
            room.Status = 0; // Quay về 0: "Chờ duyệt"

            _context.Update(room);
            await _context.SaveChangesAsync();

            TempData["Warning"] = "Đã gỡ/từ chối tin đăng!";
            return RedirectToAction(nameof(Index));
        }

        // Xóa vĩnh viễn (nếu tin vi phạm chính sách)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                // Xóa ảnh vật lý nếu cần thiết trước khi xóa record
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                TempData["Error"] = "Đã xóa vĩnh viễn tin đăng!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}