using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VanPhongPhamDKT.Models;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// 1. Thêm dịch vụ MVC
// ----------------------------
builder.Services.AddControllersWithViews();

// ----------------------------
// 2. Cấu hình Session
// ----------------------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // thời gian sống của session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ----------------------------
// 3. Cấu hình Cookie Auth
// ----------------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/DangNhap";            // URL login
        options.AccessDeniedPath = "/Auth/KhongDuQuyen"; // URL khi bị chặn quyền
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ----------------------------
// 4. Kết nối DB
// ----------------------------
var connectionString = builder.Configuration.GetConnectionString("NhtDbConnect");
builder.Services.AddDbContext<VanPhongPhamContext>(x => x.UseSqlServer(connectionString));

var app = builder.Build();

// ----------------------------
// 5. Middleware
// ----------------------------
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚠️ Session phải đặt trước Authentication và Authorization
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ----------------------------
// 6. Định tuyến
// ----------------------------

// Route cho Areas (phần admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Route mặc định cho public
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
