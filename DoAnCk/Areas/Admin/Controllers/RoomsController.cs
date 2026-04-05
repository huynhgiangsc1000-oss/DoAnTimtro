using DoAnCk.Data;
using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnCk.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Rooms
        public async Task<IActionResult> Index()
        {
            // Sử dụng .Include(r => r.Images) theo đúng Model Room của bạn
            var rooms = _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.User)
                .Include(r => r.Images)
                .OrderByDescending(r => r.CreatedDate);

            return View(await rooms.ToListAsync());
        }

        // POST: Admin/Rooms/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            room.IsApproved = true;
            _context.Update(room);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Duyệt bài đăng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Rooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.User)
                .Include(r => r.Images) // Đổi thành .Images
                .Include(r => r.RoomAmenities) // Load thêm tiện ích để xem chi tiết
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null) return NotFound();

            return View(room);
        }

        // GET: Admin/Rooms/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Admin/Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Price,Area,Address,Latitude,Longitude,CreatedDate,IsApproved,UserId,CategoryId")] Room room)
        {
            if (ModelState.IsValid)
            {
                room.CreatedDate = DateTime.Now;
                _context.Add(room);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", room.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", room.UserId);
            return View(room);
        }

        // GET: Admin/Rooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", room.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", room.UserId);
            return View(room);
        }

        // POST: Admin/Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,Area,Address,Latitude,Longitude,CreatedDate,IsApproved,UserId,CategoryId")] Room room)
        {
            if (id != room.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", room.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", room.UserId);
            return View(room);
        }

        // GET: Admin/Rooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null) return NotFound();

            return View(room);
        }

        // POST: Admin/Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}