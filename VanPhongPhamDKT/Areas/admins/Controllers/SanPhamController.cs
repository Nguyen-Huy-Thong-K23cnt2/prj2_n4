using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VanPhongPhamDKT.Models;

namespace VanPhongPhamDKT.Areas.admins.Controllers
{
    [Area("admins")]
    public class SanPhamController : Controller
    {
        private readonly VanPhongPhamContext _context;
        private readonly IWebHostEnvironment _env;

        public SanPhamController(VanPhongPhamContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // -------------------- Helpers --------------------
        private async Task BindDanhMucAsync(int? selectedId = null)
        {
            var dms = await _context.DanhMucSanPhams
                .AsNoTracking()
                .OrderBy(x => x.TenDm)
                .ToListAsync();

            ViewBag.MaDm = new SelectList(dms, "MaDm", "TenDm", selectedId);
        }

        private static readonly string[] _imgExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private async Task<string?> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            const long maxSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxSize) return null;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_imgExt.Contains(ext)) return null;

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsDir);

            var fileName = Path.GetRandomFileName() + ext;
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            return $"/uploads/products/{fileName}";
        }

        private void DeleteImageIfLocal(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return;

            var fullPath = Path.Combine(
                _env.WebRootPath,
                path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(fullPath))
            {
                try { System.IO.File.Delete(fullPath); } catch { /* ignore */ }
            }
        }

        // -------------------- INDEX --------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _context.SanPhams
                .AsNoTracking()
                .Include(x => x.MaDmNavigation)
                .ToListAsync();

            return View(list);
        }

        // -------------------- DETAILS --------------------
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var sp = await _context.SanPhams
                .AsNoTracking()
                .Include(x => x.MaDmNavigation)
                .FirstOrDefaultAsync(x => x.MaSp == id);

            if (sp == null) return NotFound();
            return View(sp);
        }

        // -------------------- CREATE --------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await BindDanhMucAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham sp, IFormFile? HinhAnhFile)
        {
            // Bỏ validation cho navigation (tránh lỗi nếu EF sinh Required)
            ModelState.Remove(nameof(sp.MaDmNavigation));
            // Nếu Model có [Required] HinhAnh mà bạn chưa upload
            ModelState.Remove(nameof(sp.HinhAnh));

            if (!ModelState.IsValid)
            {
                await BindDanhMucAsync(sp.MaDm);
                return View(sp);
            }

            if (HinhAnhFile != null)
            {
                var saved = await SaveImageAsync(HinhAnhFile);
                if (saved == null)
                {
                    ViewBag.FileError = "Ảnh không hợp lệ (jpg, jpeg, png, gif, webp; tối đa 5MB).";
                    await BindDanhMucAsync(sp.MaDm);
                    return View(sp);
                }
                sp.HinhAnh = saved;
            }

            _context.SanPhams.Add(sp);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã thêm sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        // -------------------- EDIT --------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null) return NotFound();

            await BindDanhMucAsync(sp.MaDm);
            return View(sp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile? HinhAnhFile)
        {
            var spDb = await _context.SanPhams.FirstOrDefaultAsync(x => x.MaSp == id);
            if (spDb == null) return NotFound();

            // Chỉ cập nhật các field cho phép (tránh overposting)
            if (await TryUpdateModelAsync(spDb, "",
                s => s.TenSp, s => s.Gia, s => s.SoLuong, s => s.MoTa, s => s.MaDm))
            {
                ModelState.Remove("MaDmNavigation");

                if (!ModelState.IsValid)
                {
                    await BindDanhMucAsync(spDb.MaDm);
                    return View(spDb);
                }

                // Nếu có ảnh mới -> lưu & xóa ảnh cũ (local)
                if (HinhAnhFile != null && HinhAnhFile.Length > 0)
                {
                    var saved = await SaveImageAsync(HinhAnhFile);
                    if (saved == null)
                    {
                        ViewBag.FileError = "Ảnh không hợp lệ (jpg, jpeg, png, gif, webp; tối đa 5MB).";
                        await BindDanhMucAsync(spDb.MaDm);
                        return View(spDb);
                    }

                    DeleteImageIfLocal(spDb.HinhAnh);
                    spDb.HinhAnh = saved;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            await BindDanhMucAsync(spDb.MaDm);
            return View(spDb);
        }

        // -------------------- DELETE --------------------
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var sp = await _context.SanPhams
                .AsNoTracking()
                .Include(x => x.MaDmNavigation)
                .FirstOrDefaultAsync(x => x.MaSp == id);

            if (sp == null) return NotFound();
            return View(sp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp != null)
            {
                DeleteImageIfLocal(sp.HinhAnh); // xoá file local nếu có
                _context.SanPhams.Remove(sp);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xoá sản phẩm.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
