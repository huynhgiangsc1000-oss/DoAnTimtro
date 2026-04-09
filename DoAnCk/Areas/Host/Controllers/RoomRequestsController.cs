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

        public async Task<IActionResult> Details(int id)
        {
            string userId = GetCurrentUserId();

            var request = await _context.RoomRequests
                .Include(r => r.Room)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(r => r.Id == id && r.Room != null && r.Room.UserId == userId);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int requestId, RequestStatus status, string? note)
        {
            string userId = GetCurrentUserId();

            var request = await _context.RoomRequests
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Room != null && r.Room.UserId == userId);

            if (request == null)
            {
                return NotFound();
            }

            request.Status = status;

            if (!string.IsNullOrWhiteSpace(note))
            {
                request.Note = note;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật yêu cầu thành công.";

            return RedirectToAction(nameof(Index));
        }
    }
}