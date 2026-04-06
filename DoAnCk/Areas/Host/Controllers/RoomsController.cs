using DoAnCk.Data;
using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoAnCk.Areas.Host.Controllers
{
    [Area("Host")]
    [Authorize(Roles = "Host")]
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RoomsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        public async Task<IActionResult> Index(int? status)
        {
            string userId = GetCurrentUserId();
            var query = _context.Rooms
                .Include(r => r.Category)
                // SỬA: Phải khớp với tên trong file Room.cs (Images)
                .Include(r => r.RoomImages)
                .Where(r => r.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
                ViewBag.CurrentStatus = status.Value;
            }

            return View(await query.OrderByDescending(r => r.CreatedDate).ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room, List<IFormFile> images)
        {
            // Xóa UserId khỏi ModelState vì chúng ta gán thủ công ở dưới
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                room.UserId = GetCurrentUserId();
                room.CreatedDate = DateTime.Now;

                _context.Add(room);
                await _context.SaveChangesAsync();

                if (images != null && images.Count > 0)
                {
                    await SaveImages(room.Id, images);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", room.CategoryId);
            return View(room);
        }

        // ============================================================
        // PHẦN BỔ SUNG: Hàm SaveImages để giải quyết lỗi "does not exist"
        // ============================================================
        private async Task SaveImages(int roomId, List<IFormFile> images)
        {
            string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "images", "rooms");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            foreach (var file in images)
            {
                if (file.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string path = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    _context.RoomImages.Add(new RoomImage
                    {
                        RoomId = roomId,
                        ImageUrl = "/images/rooms/" + fileName,
                        IsMain = false
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        // Các hàm Edit, Details, Delete khác...
    }
}