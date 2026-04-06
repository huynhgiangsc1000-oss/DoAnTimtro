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
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public FavoritesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Danh sách phòng đã lưu của user hiện tại
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var favorites = await _context.Favorites
                .Include(f => f.Room)
                    .ThenInclude(r => r.Images)
                .Include(f => f.User)
                .Where(f => f.UserId == currentUser.Id)
                .OrderByDescending(f => f.Id)
                .ToListAsync();

            return View(favorites);
        }

        // Xem chi tiết một mục yêu thích
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var favorite = await _context.Favorites
                .Include(f => f.Room)
                    .ThenInclude(r => r.Images)
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == currentUser.Id);

            if (favorite == null) return NotFound();

            return View(favorite);
        }

        // Trang xác nhận bỏ yêu thích
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var favorite = await _context.Favorites
                .Include(f => f.Room)
                    .ThenInclude(r => r.Images)
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == currentUser.Id);

            if (favorite == null) return NotFound();

            return View(favorite);
        }

        // Xóa favorite theo id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == currentUser.Id);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // AJAX: kiểm tra phòng này đã nằm trong favorite chưa
        [HttpGet]
        public async Task<IActionResult> IsFavorite(int roomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, isFavorite = false });
            }

            var isFavorite = await _context.Favorites
                .AnyAsync(f => f.UserId == currentUser.Id && f.RoomId == roomId);

            return Json(new { success = true, isFavorite });
        }

        // AJAX: thêm/bỏ yêu thích bằng nút tim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int roomId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn cần đăng nhập để dùng chức năng này."
                });
            }

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId);
            if (room == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Phòng không tồn tại."
                });
            }

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == currentUser.Id && f.RoomId == roomId);

            if (favorite == null)
            {
                favorite = new Favorite
                {
                    UserId = currentUser.Id,
                    RoomId = roomId
                };

                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    isFavorite = true,
                    message = "Đã thêm vào danh sách yêu thích."
                });
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isFavorite = false,
                message = "Đã bỏ khỏi danh sách yêu thích."
            });
        }
    }
}