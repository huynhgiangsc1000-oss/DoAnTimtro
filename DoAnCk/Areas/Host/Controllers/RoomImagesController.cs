using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCk.Data;
using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DoAnCk.Areas.Host.Controllers
{
    [Area("Host")]
    [Authorize(Roles = "Host")]
    public class RoomImagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RoomImagesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // SỬA: Trả về string
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        public async Task<IActionResult> Index(int roomId)
        {
            // So sánh UserId (string) == GetCurrentUserId() (string)
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == roomId && r.UserId == GetCurrentUserId());

            if (room == null) return NotFound();

            ViewBag.RoomId = roomId;
            ViewBag.RoomTitle = room.Title;

            var images = await _context.RoomImages
                .Where(i => i.RoomId == roomId)
                .ToListAsync();

            return View(images);
        }

        // ... (Các hàm khác tương tự)
    }
}