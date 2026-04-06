using DoAnCk.Data;
using DoAnCk.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. Cấu hình Identity (CHỈ DÙNG MỘT CÁCH NÀY)
// Sử dụng User và Role tùy chỉnh của bạn
builder.Services.AddIdentity<User, Role>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Tắt xác nhận Email để dễ test
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI() // Quan trọng: Để sử dụng các trang Login/Register mặc định
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 3. Cấu hình Cookie cho phân quyền Areas
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";
});

var app = builder.Build();

// 4. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // Xác thực danh tính
app.UseAuthorization();  // Phân quyền

// 5. Định nghĩa Route
// Lưu ý: Route Area phải nằm TRÊN Route Default
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// 6. Seed Data: Tự động tạo Role khi ứng dụng chạy lần đầu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        string[] roles = { "Admin", "Host", "Member" };

        // Tạo một Task chạy ngầm để xử lý các hàm Async trong Program.cs
        Task.Run(async () => {
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper() // Thêm dòng này để Identity hoạt động chuẩn
                    });
                }
            }
        }).GetAwaiter().GetResult(); // Chờ tác vụ hoàn thành trước khi chạy tiếp App
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Có lỗi xảy ra khi khởi tạo Role.");
    }
}

app.Run();