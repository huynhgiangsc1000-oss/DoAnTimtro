using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoAnCk.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // Action hiển thị danh sách
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var viewModel = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                viewModel.Add(new UserListViewModel
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            return View(viewModel);
        }

        // Action Khóa/Mở khóa tài khoản
        [HttpPost]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd == null || user.LockoutEnd < DateTime.Now)
            {
                // Khóa tài khoản
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            else
            {
                // Mở khóa tài khoản
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            return RedirectToAction(nameof(Index));
        }
    }

    // Định nghĩa ViewModel ngay tại đây để tránh lỗi Namespace không tìm thấy
    public class UserListViewModel
    {
        public User User { get; set; } = null!; // null! để báo trình biên dịch không cần lo về null
        public List<string> Roles { get; set; } = new List<string>();
    }
}