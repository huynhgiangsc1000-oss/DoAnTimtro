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

        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // DANH SÁCH PHÒNG
        public async Task<IActionResult> Index(int? status)
        {
            string userId = GetCurrentUserId();
            var query = _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.RoomImages)
                .Where(r => r.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
                ViewBag.CurrentStatus = status.Value;
            }

            return View(await query.OrderByDescending(r => r.CreatedDate).ToListAsync());
        }

        // CHI TIẾT PHÒNG
        // Lưu ý: Đây là Controller phục vụ việc xem phòng của khách (thường nằm ở root hoặc Customer Area)
        public async Task<IActionResult> Details(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.User) // QUAN TRỌNG: Để lấy FullName, PhoneNumber, Address của chủ trọ
                .Include(r => r.Category)
                .Include(r => r.RoomImages)
                .Include(r => r.RoomAmenities).ThenInclude(ra => ra.Amenity)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }
        public async Task<IActionResult> Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Amenities = await _context.Amenities.ToListAsync();
            return View();
        }

        // TẠO MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room, List<IFormFile> images, int[] selectedAmenities)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Category");
            ModelState.Remove("RoomAmenities");

            if (ModelState.IsValid)
            {
                room.UserId = GetCurrentUserId();
                room.CreatedDate = DateTime.Now;
                room.IsApproved = false;
                room.Status = 0;

                _context.Add(room);
                await _context.SaveChangesAsync();

                if (selectedAmenities != null)
                {
                    foreach (var amenityId in selectedAmenities)
                    {
                        _context.RoomAmenities.Add(new RoomAmenity { RoomId = room.Id, AmenityId = amenityId });
                    }
                }

                if (images != null && images.Count > 0)
                {
                    await SaveImages(room.Id, images);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đăng tin thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", room.CategoryId);
            ViewBag.Amenities = await _context.Amenities.ToListAsync();
            return View(room);
        }

        // CHỈNH SỬA (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.RoomAmenities)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == GetCurrentUserId());

            if (room == null) return NotFound();

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", room.CategoryId);
            ViewBag.Amenities = await _context.Amenities.ToListAsync();
            ViewBag.SelectedAmenities = room.RoomAmenities.Select(ra => ra.AmenityId).ToList();

            return View(room);
        }

        // CHỈNH SỬA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room, int[] selectedAmenities)
        {
            if (id != room.Id) return NotFound();

            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Category");
            ModelState.Remove("RoomAmenities");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRoom = await _context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
                    room.UserId = GetCurrentUserId();
                    room.CreatedDate = existingRoom.CreatedDate;
                    room.UpdatedDate = DateTime.Now;
                    room.IsApproved = existingRoom.IsApproved;
                    room.Status = existingRoom.Status;

                    _context.Update(room);

                    // Cập nhật Amenities
                    var currentAmenities = _context.RoomAmenities.Where(ra => ra.RoomId == id);
                    _context.RoomAmenities.RemoveRange(currentAmenities);
                    if (selectedAmenities != null)
                    {
                        foreach (var amenityId in selectedAmenities)
                        {
                            _context.RoomAmenities.Add(new RoomAmenity { RoomId = id, AmenityId = amenityId });
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Rooms.Any(e => e.Id == room.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // XÓA PHÒNG (POST/AJAX)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomImages)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == GetCurrentUserId());

            if (room == null) return Json(new { success = false, message = "Không tìm thấy phòng!" });

            // Xóa ảnh vật lý
            foreach (var img in room.RoomImages)
            {
                var path = Path.Combine(_hostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa thành công!" });
        }

        private async Task SaveImages(int roomId, List<IFormFile> images)
        {
            string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "images", "rooms");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            bool isFirst = !_context.RoomImages.Any(i => i.RoomId == roomId);
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
                    _context.RoomImages.Add(new RoomImage { RoomId = roomId, ImageUrl = "/images/rooms/" + fileName, IsMain = isFirst });
                    isFirst = false;
                }
            }
        }
    }
}