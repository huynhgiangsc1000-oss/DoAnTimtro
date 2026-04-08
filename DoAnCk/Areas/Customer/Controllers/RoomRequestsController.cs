using DoAnCk.Data;
using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnCk.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")] // Chỉ Customer mới có quyền truy cập
    public class RoomRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public RoomRequestsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. Danh sách yêu cầu của CHÍNH khách hàng đó
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var requests = await _context.RoomRequests
                .Include(r => r.Room)
                .Where(r => r.SenderId == userId) // Lọc theo người dùng hiện tại
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        // 2. Gửi yêu cầu mới từ trang Chi tiết phòng (Action này thường gọi từ bên ngoài Area)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequest(int roomId, string message)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Vui lòng nhập nội dung lời nhắn.";
                return RedirectToAction("Details", "Rooms", new { area = "", id = roomId });
            }

            var request = new RoomRequest
            {
                RoomId = roomId,
                SenderId = userId,
                Message = message,
                RequestDate = DateTime.Now,
                Status = 0 // 0: Chờ phản hồi, 1: Đã chấp nhận, 2: Từ chối
            };

            _context.RoomRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yêu cầu thuê phòng đã được gửi thành công!";
            return RedirectToAction("Index");
        }

        // 3. Xem chi tiết yêu cầu và phản hồi từ chủ trọ
        public async Task<IActionResult> Details(int? id)
        {
            var userId = _userManager.GetUserId(User);
            if (id == null) return NotFound();

            var roomRequest = await _context.RoomRequests
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id && m.SenderId == userId);

            if (roomRequest == null) return NotFound();

            return View(roomRequest);
        }

        // 4. Hủy yêu cầu (Xóa)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var roomRequest = await _context.RoomRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.SenderId == userId);

            if (roomRequest != null)
            {
                _context.RoomRequests.Remove(roomRequest);
                await _context.SaveChangesAsync();
                TempData["Warning"] = "Đã hủy yêu cầu thuê phòng.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}