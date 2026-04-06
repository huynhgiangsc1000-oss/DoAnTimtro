using DoAnCk.Data;
using DoAnCk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DoAnCk.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // 1. Thêm dòng này
        private readonly ApplicationDbContext _context;

        // 2. Cập nhật Constructor để nhận ApplicationDbContext
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // 3. Gán giá trị vào biến _context
        }

        public async Task<IActionResult> Index()
        {
            // Bây giờ _context đã tồn tại và có thể sử dụng
            var rooms = await _context.Rooms
                .Include(r => r.Images)
                .OrderByDescending(r => r.CreatedDate)
                .Take(6)
                .ToListAsync();

            return View(rooms);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
