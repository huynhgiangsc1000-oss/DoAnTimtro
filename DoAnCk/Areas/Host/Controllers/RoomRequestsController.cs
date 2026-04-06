using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCk.Data;
using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DoAnCk.Areas.Host.Controllers
{
    [Area("Host")]
    [Authorize(Roles = "Host")]
    public class RoomRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // SỬA: Trả về string thay vì int
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        public async Task<IActionResult> Index()
        {
            string userId = GetCurrentUserId();

            var requests = await _context.RoomRequests
                .Include(r => r.Room)
                .Include(r => r.Sender)
                .Where(r => r.Room != null && r.Room.UserId == userId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            ViewBag.PendingCount = requests.Count(r => r.Status == RequestStatus.Pending);
            return View(requests);
        }

        // Các hàm Details, Edit, Delete... trong file này cũng cần đảm bảo 
        // việc so sánh existingRequest.Room.UserId == GetCurrentUserId() 
        // giờ là so sánh string với string.

        // ... (Giữ nguyên logic xử lý, chỉ thay đổi kiểu dữ liệu so sánh)
    }
}