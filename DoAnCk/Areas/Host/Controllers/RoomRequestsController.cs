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

        // Helper lấy ID người dùng hiện tại
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim) : 0;
        }

        // ==========================================
        // 1. DANH SÁCH YÊU CẦU (INDEX)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();

            var requests = await _context.RoomRequests
                .Include(r => r.Room)
                .Include(r => r.Sender)
                .Where(r => r.Room.UserId == userId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        // ==========================================
        // 2. TẠO MỚI (CREATE) - Thường gọi từ phía khách
        // ==========================================
        // Lưu ý: Nếu khách hàng vãng lai gửi yêu cầu, 
        // Action này có thể cần đặt ở một Controller công khai thay vì trong Area Host.
        [HttpGet]
        public IActionResult Create(int roomId)
        {
            var room = _context.Rooms.Find(roomId);
            if (room == null) return NotFound();

            ViewBag.RoomTitle = room.Title;
            return View(new RoomRequest { RoomId = roomId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomRequest request)
        {
            // Thiết lập mặc định cho yêu cầu mới
            request.RequestDate = DateTime.Now;
            request.Status = RequestStatus.Pending;
            request.SenderId = GetCurrentUserId();

            // Xóa bỏ kiểm tra validation cho các object liên kết để tránh lỗi ModelState
            ModelState.Remove("Room");
            ModelState.Remove("Sender");

            if (ModelState.IsValid)
            {
                _context.Add(request);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Rooms", new { id = request.RoomId, area = "" });
            }
            return View(request);
        }

        // ==========================================
        // 3. CHỈNH SỬA (EDIT) - Chủ trọ cập nhật ghi chú/trạng thái
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.RoomRequests
                .Include(r => r.Sender)
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null || request.Room.UserId != GetCurrentUserId())
            {
                return NotFound();
            }
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoomRequest roomRequest)
        {
            if (id != roomRequest.Id) return NotFound();

            // Kiểm tra lại quyền sở hữu phòng trước khi lưu
            var existingRequest = await _context.RoomRequests
                .Include(r => r.Room)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingRequest == null || existingRequest.Room.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            try
            {
                _context.Update(roomRequest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.RoomRequests.Any(e => e.Id == roomRequest.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 4. CHI TIẾT (DETAILS)
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.RoomRequests
                .Include(r => r.Room)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null || request.Room.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            return View(request);
        }

        // ==========================================
        // 5. CẬP NHẬT NHANH TRẠNG THÁI (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int requestId, RequestStatus status)
        {
            var request = await _context.RoomRequests
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request != null && request.Room.UserId == GetCurrentUserId())
            {
                request.Status = status;
                _context.Update(request);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 6. XÓA (DELETE)
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.RoomRequests
                .Include(r => r.Room)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null || request.Room.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _context.RoomRequests
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request != null && request.Room.UserId == GetCurrentUserId())
            {
                _context.RoomRequests.Remove(request);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}