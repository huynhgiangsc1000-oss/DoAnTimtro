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
                .Where(r => r.UserId == userId) // So sánh string với string
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review review)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            if (ModelState.IsValid)
            {
                review.UserId = userId; // Gán string
                review.CreatedAt = DateTime.Now;
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(review);
        }

        // Các hàm Edit, Delete cũng sử dụng userId (string) tương tự
    }
}