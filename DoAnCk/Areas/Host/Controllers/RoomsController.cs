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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim) : 0;
        }

        // --- DANH SÁCH & CHI TIẾT ---

        public async Task<IActionResult> Index(int? status)
        {
            int userId = GetCurrentUserId();
            var query = _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.RoomImages)
                .Where(r => r.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
                ViewBag.CurrentStatus = status.Value;
            }

            var rooms = await query.OrderByDescending(r => r.CreatedDate).ToListAsync();
            return View(rooms);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.User)
                .Include(r => r.RoomImages)
                .Include(r => r.RoomAmenities).ThenInclude(ra => ra.Amenity)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null || room.UserId != GetCurrentUserId()) return NotFound();

            return View(room);
        }

        // --- THÊM MỚI (CREATE) ---

        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room, List<IFormFile> images)
        {
            // Loại bỏ kiểm tra xác thực cho các object liên quan để tránh lỗi IsValid = false
            ModelState.Remove("User");
            ModelState.Remove("Category");
            ModelState.Remove("RoomImages");
            ModelState.Remove("RoomAmenities");

            if (ModelState.IsValid)
            {
                room.UserId = GetCurrentUserId();
                room.CreatedDate = DateTime.Now;
                room.Status = 0;

                _context.Add(room);
                await _context.SaveChangesAsync();

                if (images != null && images.Count > 0)
                {
                    await SaveImages(room.Id, images);
                }

                TempData["Success"] = "Đã thêm phòng thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", room.CategoryId);
            return View(room);
        }

        // --- CHỈNH SỬA (EDIT) ---

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.RoomImages)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null || room.UserId != GetCurrentUserId()) return NotFound();

            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", room.CategoryId);
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room, List<IFormFile> newImages)
        {
            if (id != room.Id) return NotFound();

            // Lấy dữ liệu gốc từ DB để so sánh và bảo mật (dùng AsNoTracking để tránh xung đột bản ghi)
            var existingRoom = await _context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (existingRoom == null || existingRoom.UserId != GetCurrentUserId()) return Forbid();

            // QUAN TRỌNG: Loại bỏ các lỗi xác thực không cần thiết gửi từ Form
            ModelState.Remove("User");
            ModelState.Remove("Category");
            ModelState.Remove("RoomImages");
            ModelState.Remove("RoomAmenities");
            ModelState.Remove("newImages");

            if (ModelState.IsValid)
            {
                try
                {
                    // Đảm bảo các thông tin hệ thống không bị ghi đè bởi dữ liệu rác từ View
                    room.UserId = existingRoom.UserId;
                    room.CreatedDate = existingRoom.CreatedDate;
                    room.IsApproved = existingRoom.IsApproved; // Giữ nguyên trạng thái duyệt

                    _context.Update(room);
                    await _context.SaveChangesAsync();

                    // Lưu thêm ảnh mới nếu có
                    if (newImages != null && newImages.Count > 0)
                    {
                        await SaveImages(room.Id, newImages);
                    }

                    TempData["Success"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id)) return NotFound();
                    else throw;
                }
            }

            // Nếu có lỗi xác thực, quay lại trang Edit và nạp lại DropdownList
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", room.CategoryId);
            return View(room);
        }

        // --- QUẢN LÝ YÊU CẦU THUÊ ---

        public async Task<IActionResult> Requests()
        {
            int userId = GetCurrentUserId();

            var requests = await _context.RoomRequests
                .Include(r => r.Room)
                .Where(r => r.Room != null && r.Room.UserId == userId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            var request = await _context.RoomRequests
                .Include(r => r.Room)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request != null && request.Room != null && request.Room.UserId == GetCurrentUserId())
            {
                request.Status = (RequestStatus)1;
                request.Room.Status = 2; // Đánh dấu phòng đã cho thuê

                await _context.SaveChangesAsync();
                TempData["Success"] = "Xác nhận cho thuê thành công!";
            }
            return RedirectToAction(nameof(Requests));
        }

        // --- TRẠNG THÁI & XÓA ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, int status)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null && room.UserId == GetCurrentUserId())
            {
                room.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomImages)
                .Include(r => r.RoomRequests)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room != null && room.UserId == GetCurrentUserId())
            {
                // Xóa file ảnh vật lý trên server
                if (room.RoomImages != null)
                {
                    foreach (var img in room.RoomImages)
                    {
                        var oldPath = Path.Combine(_hostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }
                }

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa phòng thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- HELPER METHODS ---

        private async Task SaveImages(int roomId, List<IFormFile> images)
        {
            // Đường dẫn: wwwroot/images/rooms/
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

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}