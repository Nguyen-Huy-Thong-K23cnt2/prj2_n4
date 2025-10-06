using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VanPhongPhamDKT.Models;

namespace VanPhongPhamDKT.Areas.admins.Controllers
{
    [Area("admins")]
    public class DonHangController : Controller
    {
        private readonly VanPhongPhamContext _context;

        public DonHangController(VanPhongPhamContext context)
        {
            _context = context;
        }

        // ====== INDEX ======
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var donHangs = await _context.DonHangs
                .AsNoTracking()
                .Include(d => d.MaKhNavigation)
                .ToListAsync();

            return View(donHangs);
        }

        // ====== DETAILS ======
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var donHang = await _context.DonHangs
                .AsNoTracking()
                .Include(d => d.MaKhNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSpNavigation)
                .FirstOrDefaultAsync(d => d.MaDh == id);

            if (donHang == null) return NotFound();
            return View(donHang);
        }

        // ====== CREATE ======
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await BindKhachHangSelectListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonHang model)
        {
            var kh = await _context.KhachHangs.FindAsync(model.MaKh);
            if (kh == null)
                ModelState.AddModelError(nameof(model.MaKh), "Mã khách hàng không tồn tại.");

            if (model.TongTien <= 0)
                ModelState.AddModelError(nameof(model.TongTien), "Tổng tiền phải > 0.");

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                System.Diagnostics.Debug.WriteLine("MODELSTATE ERRORS (CREATE): " + errors);

                ViewBag.ServerErrors = errors;
                ViewBag.DbName = _context.Database.GetDbConnection().Database;
                ViewBag.DbSource = _context.Database.GetDbConnection().DataSource;

                await BindKhachHangSelectListAsync(model.MaKh);
                return View(model);
            }

            try
            {
                model.NgayDat = model.NgayDat ?? DateTime.Now;
                model.TrangThai = string.IsNullOrWhiteSpace(model.TrangThai) ? "Mới" : model.TrangThai;

                _context.DonHangs.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã lưu đơn hàng.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                var msg = ex.GetBaseException().Message;
                ModelState.AddModelError(string.Empty, "Không lưu được đơn hàng: " + msg);

                ViewBag.DbName = _context.Database.GetDbConnection().Database;
                ViewBag.DbSource = _context.Database.GetDbConnection().DataSource;

                await BindKhachHangSelectListAsync(model.MaKh);
                return View(model);
            }
        }

        // ====== EDIT ======
        // GET: admins/DonHang/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound();

            await BindKhachHangSelectListAsync(donHang.MaKh);
            return View(donHang);
        }

        /// <summary>
        /// CÁCH 2: Không dựa vào binder cho các trường nhạy cảm.
        /// Đọc thẳng Request.Form, tự parse/validate, gán vào entity đang tracking và lưu.
        /// (ĐỔI TÊN method -> EditPost, map lại tên action "Edit" để tránh trùng chữ ký)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(int id)
        {
            // 1) Lấy entity đang tracking
            var donHangDb = await _context.DonHangs.FirstOrDefaultAsync(d => d.MaDh == id);
            if (donHangDb == null) return NotFound();

            // 2) Lấy dữ liệu từ form
            var form = Request.Form;

            // MaKh
            if (!int.TryParse(form["MaKh"], out var maKh))
                ModelState.AddModelError("MaKh", "Vui lòng chọn khách hàng hợp lệ.");

            // NgayDat
            DateTime? ngayDat = null;
            var ngayDatStr = form["NgayDat"].ToString();
            if (!string.IsNullOrWhiteSpace(ngayDatStr))
            {
                if (DateTime.TryParse(ngayDatStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt) ||
                    DateTime.TryParse(ngayDatStr, CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.AssumeLocal, out dt) ||
                    DateTime.TryParse(ngayDatStr, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dt))
                {
                    ngayDat = dt;
                }
                else
                {
                    ModelState.AddModelError("NgayDat", "Định dạng ngày không hợp lệ.");
                }
            }

            // TongTien
            decimal tongTien = 0;
            var tongTienStr = form["TongTien"].ToString().Trim();
            if (!string.IsNullOrEmpty(tongTienStr))
            {
                tongTienStr = tongTienStr.Replace(" ", "");
                if (!decimal.TryParse(tongTienStr, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out tongTien))
                {
                    var inv = tongTienStr.Replace(".", "").Replace(",", ".");
                    decimal.TryParse(inv, NumberStyles.Number, CultureInfo.InvariantCulture, out tongTien);
                }
            }
            if (tongTien <= 0)
                ModelState.AddModelError("TongTien", "Tổng tiền phải > 0.");

            // TrangThai
            var trangThai = form["TrangThai"].ToString()?.Trim();

            // 3) Kiểm tra KH tồn tại
            var kh = await _context.KhachHangs.FindAsync(maKh);
            if (kh == null)
                ModelState.AddModelError("MaKh", "Mã khách hàng không tồn tại.");

            // 4) Trả lại view nếu lỗi
            if (!ModelState.IsValid)
            {
                await BindKhachHangSelectListAsync(maKh);
                if (maKh > 0) donHangDb.MaKh = maKh;
                if (ngayDat.HasValue) donHangDb.NgayDat = ngayDat;
                if (tongTien > 0) donHangDb.TongTien = tongTien;
                if (!string.IsNullOrWhiteSpace(trangThai)) donHangDb.TrangThai = trangThai;

                return View(donHangDb);
            }

            // 5) Gán & lưu
            donHangDb.MaKh = maKh;
            if (ngayDat.HasValue) donHangDb.NgayDat = ngayDat;
            donHangDb.TongTien = tongTien;
            donHangDb.TrangThai = trangThai;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật đơn hàng.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "Không cập nhật được: " + ex.GetBaseException().Message);
                await BindKhachHangSelectListAsync(donHangDb.MaKh);
                return View(donHangDb);
            }
        }

        // ====== DELETE ======
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var donHang = await _context.DonHangs
                .AsNoTracking()
                .Include(d => d.MaKhNavigation)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSpNavigation)
                .FirstOrDefaultAsync(d => d.MaDh == id);

            if (donHang == null) return NotFound();
            return View(donHang);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var donHang = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .FirstOrDefaultAsync(d => d.MaDh == id);

                if (donHang == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction(nameof(Index));
                }

                if (donHang.ChiTietDonHangs is { Count: > 0 })
                {
                    _context.ChiTietDonHangs.RemoveRange(donHang.ChiTietDonHangs);
                    await _context.SaveChangesAsync();
                }

                _context.DonHangs.Remove(donHang);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                TempData["Success"] = "Đã xóa đơn hàng.";
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Không thể xoá đơn hàng: " + ex.GetBaseException().Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ====== Helper ======
        private async Task BindKhachHangSelectListAsync(int? selectedId = null)
        {
            var khachHangs = await _context.KhachHangs
                .AsNoTracking()
                .OrderBy(k => k.HoTen)
                .Select(k => new
                {
                    k.MaKh,
                    Ten = string.IsNullOrEmpty(k.HoTen) ? k.Email : k.HoTen
                })
                .ToListAsync();

            ViewBag.MaKh = new SelectList(khachHangs, "MaKh", "Ten", selectedId);
        }
    }
}
