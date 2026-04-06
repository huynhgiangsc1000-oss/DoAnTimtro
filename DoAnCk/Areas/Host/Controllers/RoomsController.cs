using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAnCk.Data;
using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Authorization;
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

        // Helper lấy UserId của người đang đăng nhập
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim) : 0;
        }

        // GET: Host/Rooms
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            var rooms = await _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.RoomImages) // Thêm cái này để hiện ảnh thumbnail ở Index
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return View(rooms);
        }

        // GET: Host/Rooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.User)
                .Include(r => r.RoomImages)
                .Include(r => r.RoomAmenities).ThenInclude(ra => ra.Amenity)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Kiểm tra: Nếu phòng không tồn tại hoặc không phải của chủ này
            if (room == null || room.UserId != GetCurrentUserId()) return NotFound();

            return View(room);
        }

        // GET: Host/Rooms/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Host/Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room, List<IFormFile> images)
        {
            room.UserId = GetCurrentUserId();
            room.CreatedDate = DateTime.Now;
            room.IsApproved = false;

            ModelState.Remove("User");
            ModelState.Remove("Category");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                _context.Add(room);
                await _context.SaveChangesAsync();

                if (images != null && images.Count > 0)
                {
                    await SaveImages(room.Id, images);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", room.CategoryId);
            return View(room);
        }

        // GET: Host/Rooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.RoomImages)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null || room.UserId != GetCurrentUserId()) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", room.CategoryId);
            return View(room);
        }

        // POST: Host/Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room, List<IFormFile> newImages)
        {
            if (id != room.Id) return NotFound();

            // Bảo mật: Đảm bảo người dùng không đổi UserId qua Tool F12
            if (room.UserId != GetCurrentUserId()) return Forbid();

            ModelState.Remove("User");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();

                    if (newImages != null && newImages.Count > 0)
                    {
                        await SaveImages(room.Id, newImages);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Rooms.Any(e => e.Id == room.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", room.CategoryId);
            return View(room);
        }

        // GET: Host/Rooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null || room.UserId != GetCurrentUserId()) return NotFound();

            return View(room);
        }

        // POST: Host/Rooms/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomImages)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room != null && room.UserId == GetCurrentUserId())
            {
                // Xóa file ảnh vật lý trên ổ cứng trước khi xóa bản ghi
                foreach (var img in room.RoomImages)
                {
                    var oldPath = Path.Combine(_hostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Private Method để tái sử dụng việc lưu ảnh
        private async Task SaveImages(int roomId, List<IFormFile> images)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string uploadDir = Path.Combine(wwwRootPath, "images", "rooms");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            foreach (var file in images)
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
                    IsMain = false // Cường có thể tùy chỉnh logic ảnh chính ở đây
                });
            }
            await _context.SaveChangesAsync();
        }
    }
}