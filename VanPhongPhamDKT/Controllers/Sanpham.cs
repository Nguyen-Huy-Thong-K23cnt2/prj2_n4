using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanPhongPhamDKT.Models;

namespace VanPhongPhamDKT.Controllers
{
    // Controller dành cho trang khách (không phải Area admins)
    public class Sanpham : Controller
    {
        private readonly VanPhongPhamContext _context;

        public Sanpham(VanPhongPhamContext context)
        {
            _context = context;
        }

        // ===================== INDEX (liệt kê sản phẩm) =====================
        // /Sanpham?categoryId=...&page=1&pageSize=12
        [HttpGet]
        public async Task<IActionResult> Index(int? categoryId, int page = 1, int pageSize = 12)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 12;

            var query = _context.SanPhams
                .AsNoTracking()
                .Include(x => x.MaDmNavigation)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.MaDm == categoryId.Value);
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(x => x.MaSp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Categories cho sidebar/filter (nếu view cần)
            ViewBag.Categories = await _context.DanhMucSanPhams
                .AsNoTracking()
                .OrderBy(x => x.TenDm)
                .ToListAsync();

            ViewBag.CategoryId = categoryId;
            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(data);
        }


        // ===================== SEARCH (tìm kiếm sản phẩm) =====================
        // /Sanpham/Search?q=but&page=1&pageSize=12
        [HttpGet]
        public async Task<IActionResult> Search(string q, int page = 1, int pageSize = 12)
        {
            q ??= string.Empty;
            q = q.Trim();
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 12;

            var query = _context.SanPhams
                .AsNoTracking()
                .Include(x => x.MaDmNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                // Tìm theo tên & mô tả (có thể bỏ MoTa nếu cột lớn)
                query = query.Where(x =>
                    EF.Functions.Like(x.TenSp, $"%{q}%") ||
                    (x.MoTa != null && EF.Functions.Like(x.MoTa, $"%{q}%"))
                );
            }

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(x => x.MaSp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = q;
            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(data); // View: Views/Sanpham/Search.cshtml
        }
    }
}
