using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAnCk.Data;
using DoAnCk.Models.Entities;

namespace DoAnCk.Areas.Host.Controllers
{
    [Area("Host")]
    public class RoomRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Host/RoomRequests
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.RoomRequests.Include(r => r.Room).Include(r => r.Sender);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Host/RoomRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomRequest = await _context.RoomRequests
                .Include(r => r.Room)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomRequest == null)
            {
                return NotFound();
            }

            return View(roomRequest);
        }

        // GET: Host/RoomRequests/Create
        public IActionResult Create()
        {
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Title");
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Host/RoomRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoomId,SenderId,Message,RequestDate,Status")] RoomRequest roomRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(roomRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Title", roomRequest.RoomId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", roomRequest.SenderId);
            return View(roomRequest);
        }

        // GET: Host/RoomRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomRequest = await _context.RoomRequests.FindAsync(id);
            if (roomRequest == null)
            {
                return NotFound();
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Title", roomRequest.RoomId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", roomRequest.SenderId);
            return View(roomRequest);
        }

        // POST: Host/RoomRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomId,SenderId,Message,RequestDate,Status")] RoomRequest roomRequest)
        {
            if (id != roomRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(roomRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomRequestExists(roomRequest.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Title", roomRequest.RoomId);
            ViewData["SenderId"] = new SelectList(_context.Users, "Id", "Id", roomRequest.SenderId);
            return View(roomRequest);
        }

        // GET: Host/RoomRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var roomRequest = await _context.RoomRequests
                .Include(r => r.Room)
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (roomRequest == null)
            {
                return NotFound();
            }

            return View(roomRequest);
        }

        // POST: Host/RoomRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var roomRequest = await _context.RoomRequests.FindAsync(id);
            if (roomRequest != null)
            {
                _context.RoomRequests.Remove(roomRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomRequestExists(int id)
        {
            return _context.RoomRequests.Any(e => e.Id == id);
        }
    }
}
