using DoAnCk.Data;
using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnCk.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class RoomRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public RoomRequestsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Customer/RoomRequests
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var myRequests = await _context.RoomRequests
                .Include(r => r.Room)
                .Include(r => r.Sender)
                .Where(r => r.SenderId == currentUser.Id)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(myRequests);
        }

        // GET: Customer/RoomRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var roomRequest = await _context.RoomRequests
                .Include(r => r.Room)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(r => r.Id == id && r.SenderId == currentUser.Id);

            if (roomRequest == null) return NotFound();

            return View(roomRequest);
        }

        // GET: Customer/RoomRequests/Create
        public async Task<IActionResult> Create(int? roomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (roomId.HasValue)
            {
                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId.Value);
                if (room == null) return NotFound();

                ViewBag.RoomTitle = room.Title;
                ViewBag.RoomIdLocked = true;

                return View(new RoomRequest
                {
                    RoomId = roomId.Value,
                    RequestDate = DateTime.Now
                });
            }

            var rooms = await _context.Rooms.ToListAsync();
            ViewBag.RoomId = new SelectList(rooms, "Id", "Title");
            ViewBag.RoomIdLocked = false;

            return View(new RoomRequest
            {
                RequestDate = DateTime.Now
            });
        }

        // POST: Customer/RoomRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,Message")] RoomRequest roomRequest)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomRequest.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("RoomId", "Vui lòng chọn phòng hợp lệ.");
            }

            var hasPendingOrAccepted = await _context.RoomRequests.AnyAsync(r =>
                r.RoomId == roomRequest.RoomId &&
                r.SenderId == currentUser.Id &&
                (r.Status == RequestStatus.Pending || r.Status == RequestStatus.Accepted));

            if (hasPendingOrAccepted)
            {
                ModelState.AddModelError("", "Bạn đã gửi yêu cầu cho phòng này rồi.");
            }

            if (string.IsNullOrWhiteSpace(roomRequest.Message))
            {
                ModelState.AddModelError("Message", "Vui lòng nhập nội dung yêu cầu.");
            }

            if (!ModelState.IsValid)
            {
                var rooms = await _context.Rooms.ToListAsync();
                ViewBag.RoomId = new SelectList(rooms, "Id", "Title", roomRequest.RoomId);
                ViewBag.RoomTitle = room?.Title;
                ViewBag.RoomIdLocked = room != null;

                return View(roomRequest);
            }

            roomRequest.SenderId = currentUser.Id;
            roomRequest.RequestDate = DateTime.Now;
            roomRequest.Status = RequestStatus.Pending;

            _context.RoomRequests.Add(roomRequest);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã gửi yêu cầu thuê thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Customer/RoomRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var roomRequest = await _context.RoomRequests
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id && r.SenderId == currentUser.Id);

            if (roomRequest == null) return NotFound();

            if (roomRequest.Status != RequestStatus.Pending)
            {
                TempData["Error"] = "Chỉ có thể sửa yêu cầu đang chờ xử lý.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.RoomTitle = roomRequest.Room?.Title;
            return View(roomRequest);
        }

        // POST: Customer/RoomRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomId,Message")] RoomRequest roomRequest)
        {
            if (id != roomRequest.Id) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var oldRequest = await _context.RoomRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.SenderId == currentUser.Id);

            if (oldRequest == null) return NotFound();

            if (oldRequest.Status != RequestStatus.Pending)
            {
                TempData["Error"] = "Chỉ có thể sửa yêu cầu đang chờ xử lý.";
                return RedirectToAction(nameof(Index));
            }

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomRequest.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("RoomId", "Phòng không tồn tại.");
            }

            if (string.IsNullOrWhiteSpace(roomRequest.Message))
            {
                ModelState.AddModelError("Message", "Vui lòng nhập nội dung yêu cầu.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.RoomTitle = room?.Title;
                return View(roomRequest);
            }

            roomRequest.SenderId = currentUser.Id;
            roomRequest.RequestDate = oldRequest.RequestDate;
            roomRequest.Status = oldRequest.Status;

            _context.Update(roomRequest);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật yêu cầu.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Customer/RoomRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var roomRequest = await _context.RoomRequests
                .Include(r => r.Room)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(r => r.Id == id && r.SenderId == currentUser.Id);

            if (roomRequest == null) return NotFound();

            return View(roomRequest);
        }

        // POST: Customer/RoomRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var roomRequest = await _context.RoomRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.SenderId == currentUser.Id);

            if (roomRequest == null) return NotFound();

            if (roomRequest.Status != RequestStatus.Pending)
            {
                TempData["Error"] = "Chỉ có thể xóa yêu cầu đang chờ xử lý.";
                return RedirectToAction(nameof(Index));
            }

            _context.RoomRequests.Remove(roomRequest);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa yêu cầu.";
            return RedirectToAction(nameof(Index));
        }
    }
}