using DoAnCk.Data;
using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var reviews = await _context.Reviews
                .Include(r => r.Room)
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? roomId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            if (roomId == null)
            {
                TempData["ReviewError"] = "Bạn cần chọn phòng để viết đánh giá.";
                return RedirectToAction(nameof(Index));
            }

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null) return NotFound();

            var existedReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.RoomId == roomId && r.UserId == userId);

            if (existedReview != null)
            {
                TempData["ReviewError"] = "Bạn đã đánh giá phòng này rồi.";
                return RedirectToAction(nameof(Index));
            }

            var model = new Review
            {
                RoomId = room.Id
            };

            ViewBag.RoomId = room.Id;
            ViewBag.RoomTitle = room.Title;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review review)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == review.RoomId);
            if (room == null)
            {
                TempData["ReviewError"] = "Phòng không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            var existedReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.RoomId == review.RoomId && r.UserId == userId);

            if (existedReview != null)
            {
                TempData["ReviewError"] = "Bạn đã đánh giá phòng này rồi.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                review.UserId = userId;
                review.CreatedAt = DateTime.Now;

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                TempData["ReviewSuccess"] = "Đánh giá đã được gửi thành công.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.RoomId = room.Id;
            ViewBag.RoomTitle = room.Title;
            TempData["ReviewError"] = "Vui lòng kiểm tra lại thông tin đánh giá.";

            return View(review);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var review = await _context.Reviews
                .Include(r => r.Room)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null) return NotFound();

            return View(review);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var review = await _context.Reviews
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null) return NotFound();

            ViewBag.RoomId = review.RoomId;
            ViewBag.RoomTitle = review.Room?.Title;

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Review review)
        {
            if (id != review.Id) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var reviewInDb = await _context.Reviews
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reviewInDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                reviewInDb.Rating = review.Rating;
                reviewInDb.Comment = review.Comment;

                await _context.SaveChangesAsync();

                TempData["ReviewSuccess"] = "Cập nhật đánh giá thành công.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.RoomId = reviewInDb.RoomId;
            ViewBag.RoomTitle = reviewInDb.Room?.Title;
            TempData["ReviewError"] = "Vui lòng kiểm tra lại nội dung chỉnh sửa.";

            return View(review);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var review = await _context.Reviews
                .Include(r => r.Room)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null) return NotFound();

            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null) return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            TempData["ReviewSuccess"] = "Đã xóa đánh giá.";
            return RedirectToAction(nameof(Index));
        }
    }
}