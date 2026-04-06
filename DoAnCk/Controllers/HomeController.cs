using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCk.Data;
using DoAnCk.Models;
using DoAnCk.Models.Entities; // Quan trọng để nhận diện đúng lớp Room

namespace DoAnCk.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

       public async Task<IActionResult> Index()
{
    // Cần .Include(r => r.Images) để dữ liệu ảnh được truyền ra View
    var rooms = await _context.Rooms
        .Include(r => r.Images)
        .OrderByDescending(r => r.CreatedDate)
        .ToListAsync();

    return View(rooms);
}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Ở trang chi tiết cũng cần lấy ảnh và thông tin người đăng, danh mục
            var room = await _context.Rooms
                .Include(r => r.Images)
                .Include(r => r.User)
                .Include(r => r.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null) return NotFound();

            return View(room);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}