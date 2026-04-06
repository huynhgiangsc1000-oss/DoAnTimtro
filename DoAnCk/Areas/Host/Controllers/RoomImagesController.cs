using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCk.Data;
using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DoAnCk.Areas.Host.Controllers
{
    [Area("Host")]
    [Authorize(Roles = "Host")] // Đảm bảo chỉ Host mới truy cập được
    public class RoomImagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RoomImagesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // Helper lấy ID người dùng hiện tại
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim) : 0;
        }

        // GET: Host/RoomImages?roomId=5
        public async Task<IActionResult> Index(int roomId)
        {
            // Kiểm tra quyền sở hữu phòng trước khi cho phép xem ảnh
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId && r.UserId == GetCurrentUserId());
            if (room == null) return NotFound();

            ViewBag.RoomId = roomId;
            ViewBag.RoomTitle = room.Title;

            var images = await _context.RoomImages
                .Where(i => i.RoomId == roomId)
                .ToListAsync();
            return View(images);
        }

        // GET: Host/RoomImages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var roomImage = await _context.RoomImages
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (roomImage == null || roomImage.Room.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            return View(roomImage);
        }

        // GET: Host/RoomImages/Create?roomId=5
        public IActionResult Create(int roomId)
        {
            // Kiểm tra phòng có tồn tại và thuộc chủ này không
            var roomExists = _context.Rooms.Any(r => r.Id == roomId && r.UserId == GetCurrentUserId());
            if (!roomExists) return NotFound();

            return View(new RoomImage { RoomId = roomId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int roomId, IFormFile imageFile)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId && r.UserId == GetCurrentUserId());
            if (room == null) return NotFound();

            if (imageFile != null && imageFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images/rooms");

                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                var roomImage = new RoomImage
                {
                    RoomId = roomId,
                    ImageUrl = "/images/rooms/" + fileName
                };

                _context.Add(roomImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { roomId = roomId });
            }
            return View();
        }

        // GET: Host/RoomImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var roomImage = await _context.RoomImages
                .Include(r => r.Room)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (roomImage == null || roomImage.Room.UserId != GetCurrentUserId())
            {
                return NotFound();
            }
            return View(roomImage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile? newImageFile, RoomImage roomImage)
        {
            if (id != roomImage.Id) return NotFound();

            var existingImage = await _context.RoomImages
                .Include(i => i.Room)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (existingImage == null || existingImage.Room.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            if (newImageFile != null && newImageFile.Length > 0)
            {
                // Xóa ảnh cũ vật lý
                var oldPath = Path.Combine(_hostEnvironment.WebRootPath, existingImage.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);

                // Lưu ảnh mới
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(newImageFile.FileName);
                string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images/rooms");
                string filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await newImageFile.CopyToAsync(stream);
                }

                existingImage.ImageUrl = "/images/rooms/" + fileName;
                _context.Update(existingImage);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { roomId = existingImage.RoomId });
        }

        // GET: Host/RoomImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var roomImage = await _context.RoomImages
                .Include(r => r.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (roomImage == null || roomImage.Room.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            return View(roomImage);
        }

        // POST: Host/RoomImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var image = await _context.RoomImages
                .Include(i => i.Room)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (image != null && image.Room.UserId == GetCurrentUserId())
            {
                var filePath = Path.Combine(_hostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                _context.RoomImages.Remove(image);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { roomId = image.RoomId });
            }
            return RedirectToAction("Index", "Rooms");
        }
    }
}