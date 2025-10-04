using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VanPhongPhamDKT.Models;

namespace VanPhongPhamDKT.Controllers
{
    public class AuthController : Controller
    {
        private readonly VanPhongPhamContext _context;
        public AuthController(VanPhongPhamContext context) => _context = context;

        // ========== ĐĂNG NHẬP ==========
        [HttpGet, AllowAnonymous]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(string email, string matKhau, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập email và mật khẩu.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var user = await _context.KhachHangs
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(x => x.Email == email && x.MatKhau == matKhau);

            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng!";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaKh.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, string.IsNullOrWhiteSpace(user.ChucVu) ? "khachhang" : user.ChucVu!)
            };

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            // Cookie auth props
            var authProps = new AuthenticationProperties
            {
                IsPersistent = false, // tránh auto-login quá lâu; cần thì đổi true
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            // Lưu email vào Session cho CartController
            HttpContext.Session.SetString("UserEmail", user.Email);

            // Điều hướng
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (!string.IsNullOrEmpty(user.ChucVu) && user.ChucVu.Trim().ToLower() == "admin")
                return RedirectToAction("Index", "Dashboard", new { area = "admins" });

            return RedirectToAction("Index", "Home");
        }

        // ========== ĐĂNG KÝ ==========
        [HttpGet, AllowAnonymous]
        public IActionResult DangKi() => View();

        [HttpPost, AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKi(KhachHang model)
        {
            if (!ModelState.IsValid) return View(model);

            var exists = await _context.KhachHangs.AnyAsync(x => x.Email == model.Email);
            if (exists)
            {
                ViewBag.Error = "Email đã được sử dụng.";
                return View(model);
            }

            // TODO: Hash mật khẩu nếu cần
            _context.KhachHangs.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(DangNhap));
        }

        // ========== ĐĂNG XUẤT ==========
        // GET: Trang xác nhận
        [HttpGet]
        public IActionResult DangXuat() => View();

        // POST: Thực thi đăng xuất (an toàn)
        [HttpPost, ActionName("DangXuat")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangXuat_Post()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("UserEmail");
            return RedirectToAction(nameof(DangNhap));
        }

        // ========== TRANG 403 ==========
        [HttpGet]
        public IActionResult KhongDuQuyen() => View();
    }
}
