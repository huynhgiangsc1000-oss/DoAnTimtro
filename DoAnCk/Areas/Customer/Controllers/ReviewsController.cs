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
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Customer/Reviews
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var reviews = await _context.Reviews
                .Include(r => r.Room)
                .Include(r => r.User)
                .Where(r => r.UserId == currentUser.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        // GET: /Customer/Reviews/Create
        // GET: /Customer/Reviews/Create?roomId=1
        public async Task<IActionResult> Create(int? roomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (roomId.HasValue)
            {
                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId.Value);
                if (room == null) return NotFound();

                var canReview = await _context.RoomRequests.AnyAsync(r =>
                    r.RoomId == roomId.Value &&
                    r.SenderId == currentUser.Id &&
                    r.Status == RequestStatus.Accepted);

                if (!canReview)
                {
                    TempData["ReviewError"] = "Chỉ người đã được chấp nhận thuê/ở ghép mới có thể đánh giá.";
                    return RedirectToAction(nameof(Index));
                }

                var hasReviewed = await _context.Reviews.AnyAsync(r =>
                    r.RoomId == roomId.Value && r.UserId == currentUser.Id);

                if (hasReviewed)
                {
                    TempData["ReviewError"] = "Bạn đã đánh giá phòng này rồi.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.RoomTitle = room.Title;
                ViewBag.RoomIdLocked = true;

                return View(new Review
                {
                    RoomId = roomId.Value,
                    Rating = 5
                });
            }

            var acceptedRooms = await _context.RoomRequests
                .Where(r => r.SenderId == currentUser.Id && r.Status == RequestStatus.Accepted)
                .Select(r => r.Room)
                .Distinct()
                .ToListAsync();

            ViewData["RoomId"] = new SelectList(acceptedRooms, "Id", "Title");
            ViewBag.RoomIdLocked = false;

            return View(new Review
            {
                Rating = 5
            });
        }

        // POST: /Customer/Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,Rating,Comment")] Review review)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == review.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("RoomId", "Phòng không tồn tại.");
            }

            var canReview = await _context.RoomRequests.AnyAsync(r =>
                r.RoomId == review.RoomId &&
                r.SenderId == currentUser.Id &&
                r.Status == RequestStatus.Accepted);

            if (!canReview)
            {
                ModelState.AddModelError("", "Chỉ người đã được chấp nhận thuê/ở ghép mới có thể đánh giá.");
            }

            var hasReviewed = await _context.Reviews.AnyAsync(r =>
                r.RoomId == review.RoomId && r.UserId == currentUser.Id);

            if (hasReviewed)
            {
                ModelState.AddModelError("", "Bạn đã đánh giá phòng này rồi.");
            }

            if (review.Rating < 1 || review.Rating > 5)
            {
                ModelState.AddModelError("Rating", "Số sao phải từ 1 đến 5.");
            }

            if (string.IsNullOrWhiteSpace(review.Comment))
            {
                ModelState.AddModelError("Comment", "Vui lòng nhập nội dung đánh giá.");
            }

            if (!ModelState.IsValid)
            {
                var acceptedRooms = await _context.RoomRequests
                    .Where(r => r.SenderId == currentUser.Id && r.Status == RequestStatus.Accepted)
                    .Select(r => r.Room)
                    .Distinct()
                    .ToListAsync();

                ViewData["RoomId"] = new SelectList(acceptedRooms, "Id", "Title", review.RoomId);
                ViewBag.RoomTitle = room?.Title;
                ViewBag.RoomIdLocked = room != null;

                return View(review);
            }

            review.UserId = currentUser.Id;
            review.CreatedAt = DateTime.Now;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["ReviewSuccess"] = "Đã gửi đánh giá thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Customer/Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var review = await _context.Reviews
                .Include(r => r.Room)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (review == null) return NotFound();

            return View(review);
        }

        // GET: /Customer/Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var review = await _context.Reviews
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (review == null) return NotFound();

            ViewBag.RoomTitle = review.Room?.Title;
            return View(review);
        }

        // POST: /Customer/Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomId,Rating,Comment")] Review review)
        {
            if (id != review.Id) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var oldReview = await _context.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (oldReview == null) return NotFound();

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == review.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("RoomId", "Phòng không tồn tại.");
            }

            if (review.Rating < 1 || review.Rating > 5)
            {
                ModelState.AddModelError("Rating", "Số sao phải từ 1 đến 5.");
            }

            if (string.IsNullOrWhiteSpace(review.Comment))
            {
                ModelState.AddModelError("Comment", "Vui lòng nhập nội dung đánh giá.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.RoomTitle = room?.Title;
                return View(review);
            }

            review.UserId = currentUser.Id;
            review.CreatedAt = oldReview.CreatedAt;

            _context.Update(review);
            await _context.SaveChangesAsync();

            TempData["ReviewSuccess"] = "Đã cập nhật đánh giá.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Customer/Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var review = await _context.Reviews
                .Include(r => r.Room)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (review == null) return NotFound();

            return View(review);
        }

        // POST: /Customer/Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }

            TempData["ReviewSuccess"] = "Đã xóa đánh giá.";
            return RedirectToAction(nameof(Index));
        }
    }
}