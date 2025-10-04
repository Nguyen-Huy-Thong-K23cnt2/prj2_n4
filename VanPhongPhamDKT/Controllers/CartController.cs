using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using VanPhongPhamDKT.Models;
using VanPhongPhamDKT.Helpers; // SessionExtensions

namespace VanPhongPhamDKT.Controllers
{
    [Authorize] // ✅ yêu cầu đăng nhập cho mọi action (trừ action gắn [AllowAnonymous])
    public class CartController : Controller
    {
        private readonly VanPhongPhamContext _context;
        private const string CARTKEY = "GioHang";

        public CartController(VanPhongPhamContext context)
        {
            _context = context;
        }

        // ---------------------------
        // Xem giỏ hàng
        // ---------------------------
        [AllowAnonymous] // ✅ cho phép ai cũng xem; nếu muốn bắt đăng nhập thì bỏ dòng này
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CARTKEY) ?? new List<CartItem>();
            return View(cart);
        }

        // ---------------------------
        // Thêm sản phẩm vào giỏ (yêu cầu login do [Authorize] ở class)
        // ---------------------------
        public async Task<IActionResult> AddToCart(int maSp, int soLuong = 1)
        {
            var sp = await _context.SanPhams.FindAsync(maSp);
            if (sp == null) return NotFound();

            var cart = HttpContext.Session.GetObject<List<CartItem>>(CARTKEY) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.MaSp == maSp);

            if (item != null)
            {
                item.SoLuong += soLuong;
            }
            else
            {
                cart.Add(new CartItem
                {
                    MaSp = sp.MaSp,
                    TenSp = sp.TenSp,
                    Gia = sp.Gia,
                    SoLuong = soLuong
                });
            }

            HttpContext.Session.SetObject(CARTKEY, cart);
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------
        // Xóa sản phẩm khỏi giỏ (yêu cầu login)
        // ---------------------------
        public IActionResult Remove(int maSp)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CARTKEY) ?? new List<CartItem>();
            cart.RemoveAll(c => c.MaSp == maSp);
            HttpContext.Session.SetObject(CARTKEY, cart);
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------
        // Đặt hàng (ghi vào DB) – yêu cầu login, tự lấy MaKh từ session/đăng nhập
        // ---------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatHang()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CARTKEY);
            if (cart == null || cart.Count == 0)
            {
                TempData["Err"] = "Giỏ hàng trống!";
                return RedirectToAction(nameof(Index));
            }

            // Lấy email từ session (bạn đã set khi đăng nhập)
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                // chưa login -> về trang login, kèm returnUrl quay lại giỏ
                return RedirectToAction("DangNhap", "Auth", new { returnUrl = Url.Action(nameof(Index), "Cart") });
            }

            // Tìm khách hàng theo email
            var kh = await _context.KhachHangs.AsNoTracking().FirstOrDefaultAsync(k => k.Email == email);
            if (kh == null)
            {
                TempData["Err"] = "Tài khoản khách hàng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var donHang = new DonHang
                {
                    MaKh = kh.MaKh,
                    NgayDat = DateTime.Now,
                    TrangThai = "Chờ xử lý",
                    TongTien = 0
                };

                _context.DonHangs.Add(donHang);
                await _context.SaveChangesAsync(); // có MaDh

                decimal tong = 0;

                foreach (var item in cart)
                {
                    // Lấy lại giá từ DB để chống sửa giá
                    var sp = await _context.SanPhams.AsNoTracking().FirstOrDefaultAsync(s => s.MaSp == item.MaSp);
                    if (sp == null)
                    {
                        await tx.RollbackAsync();
                        TempData["Err"] = $"Sản phẩm {item.MaSp} không tồn tại.";
                        return RedirectToAction(nameof(Index));
                    }

                    var giaBan = sp.Gia;
                    tong += giaBan * item.SoLuong;

                    _context.ChiTietDonHangs.Add(new ChiTietDonHang
                    {
                        MaDh = donHang.MaDh,
                        MaSp = item.MaSp,
                        SoLuong = item.SoLuong,
                        Gia = giaBan
                    });
                }

                donHang.TongTien = tong;
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                // Xoá giỏ
                HttpContext.Session.Remove(CARTKEY);

                TempData["Ok"] = "Đặt hàng thành công!";
                return RedirectToAction(nameof(ThongBaoDatHangThanhCong));
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public IActionResult ThongBaoDatHangThanhCong() => View();
    }
}
