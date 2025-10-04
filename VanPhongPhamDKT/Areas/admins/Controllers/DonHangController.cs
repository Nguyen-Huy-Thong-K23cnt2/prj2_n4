using Microsoft.AspNetCore.Mvc;
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

        // GET: admins/DonHang
        public IActionResult Index()
        {
            var donHangs = _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .ToList();

            return View(donHangs);
        }

        // GET: admins/DonHang/Details/5
        public IActionResult Details(int id)
        {
            var donHang = _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .FirstOrDefault(d => d.MaDh == id);

            if (donHang == null) return NotFound();
            return View(donHang);
        }

        // GET: admins/DonHang/Create
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DonHang donHang)
        {
            if (donHang.TongTien <= 0)
                ModelState.AddModelError(nameof(donHang.TongTien), "Tổng tiền phải > 0.");

            var kh = _context.KhachHangs.Find(donHang.MaKh);
            if (kh == null)
                ModelState.AddModelError(nameof(donHang.MaKh), "Mã khách hàng không tồn tại.");

            if (!ModelState.IsValid)
            {
                // log ra Output window
                var errors = string.Join(" | ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                System.Diagnostics.Debug.WriteLine("MODELSTATE ERRORS: " + errors);

                // show ngay trên view
                ViewBag.ServerErrors = errors;
                ViewBag.DbName = _context.Database.GetDbConnection().Database;
                ViewBag.DbSource = _context.Database.GetDbConnection().DataSource;

                return View(donHang);
            }

            try
            {
                donHang.NgayDat = DateTime.Now;
                donHang.TrangThai = "Mới";
                _context.DonHangs.Add(donHang);
                _context.SaveChanges();
                TempData["Success"] = "Đã lưu đơn hàng.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                var msg = ex.GetBaseException().Message;
                ModelState.AddModelError(string.Empty, "Không lưu được đơn hàng: " + msg);

                ViewBag.DbName = _context.Database.GetDbConnection().Database;
                ViewBag.DbSource = _context.Database.GetDbConnection().DataSource;

                return View(donHang);
            }
        }

        // GET: admins/DonHang/Edit/5
        public IActionResult Edit(int id)
        {
            var donHang = _context.DonHangs.Find(id);
            if (donHang == null) return NotFound();
            return View(donHang);
        }

        // POST: admins/DonHang/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, DonHang donHang)
        {
            if (id != donHang.MaDh) return NotFound();

            // Kiểm tra MaKh khi sửa
            var kh = _context.KhachHangs.Find(donHang.MaKh);
            if (kh == null)
                ModelState.AddModelError(nameof(donHang.MaKh), "Mã khách hàng không tồn tại.");

            if (!ModelState.IsValid)
                return View(donHang);

            try
            {
                _context.Update(donHang);
                _context.SaveChanges();
                TempData["Success"] = "Đã cập nhật đơn hàng.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "Không cập nhật được: " + ex.GetBaseException().Message);
                return View(donHang);
            }
        }

        // GET: admins/DonHang/Delete/5
        public IActionResult Delete(int id)
        {
            var donHang = _context.DonHangs
                .Include(d => d.MaKhNavigation)
                .FirstOrDefault(d => d.MaDh == id);

            if (donHang == null) return NotFound();
            return View(donHang);
        }

        // POST: admins/DonHang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var donHang = _context.DonHangs.Find(id);
            if (donHang != null)
            {
                _context.DonHangs.Remove(donHang);
                _context.SaveChanges();
                TempData["Success"] = "Đã xóa đơn hàng.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
