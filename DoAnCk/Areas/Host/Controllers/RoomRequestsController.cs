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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim) : 0;
        }

        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();

            var requests = await _context.RoomRequests
                .Include(r => r.Room)
                .Include(r => r.Sender)
                .Where(r => r.Room != null && r.Room.UserId == userId) // Kiểm tra null cho Room
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            ViewBag.PendingCount = requests.Count(r => r.Status == RequestStatus.Pending);
            return View(requests);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.RoomRequests
                .Include(r => r.Room)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Kiểm tra null an toàn (Fix lỗi Dereference)
            if (request == null || request.Room == null || request.Room.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, RequestStatus status)
        {
            var request = await _context.RoomRequests
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null || request.Room == null || request.Room.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            try
            {
                request.Status = status;

                // Sửa Approved thành Accepted để khớp với Enum
                if (status == RequestStatus.Accepted)
                {
                    // Logic: request.Room.IsAvailable = false;
                }

                _context.Update(request);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã {(status == RequestStatus.Accepted ? "chấp nhận" : "từ chối")} yêu cầu.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Status,Note")] RoomRequest roomRequest)
        {
            if (id != roomRequest.Id) return NotFound();

            var existingRequest = await _context.RoomRequests
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingRequest == null || existingRequest.Room == null || existingRequest.Room.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            try
            {
                existingRequest.Status = roomRequest.Status;
                existingRequest.Note = roomRequest.Note;

                _context.Update(existingRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.RoomRequests.Any(e => e.Id == roomRequest.Id)) return NotFound();
                else throw;
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _context.RoomRequests
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request != null && request.Room != null && request.Room.UserId == GetCurrentUserId())
            {
                _context.RoomRequests.Remove(request);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa yêu cầu.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}