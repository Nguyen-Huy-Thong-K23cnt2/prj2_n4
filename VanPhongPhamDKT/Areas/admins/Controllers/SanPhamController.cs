using Microsoft.AspNetCore.Mvc;
using VanPhongPhamDKT.Models;
using System.Linq;

namespace VanPhongPhamDKT.Areas.admins.Controllers
{
    [Area("admins")]
    public class SanPhamController : Controller
    {
        private readonly VanPhongPhamContext _context;

        public SanPhamController(VanPhongPhamContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var list = _context.SanPhams.ToList();
            return View(list);
        }


        public IActionResult Details(int id)
        {
            var kh = _context.KhachHangs.FirstOrDefault(k => k.MaKh == id);
            if (kh == null) return NotFound();
            return View(kh);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SanPham sp)
        {
            if (ModelState.IsValid)
            {
                _context.SanPhams.Add(sp);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(sp);
        }

        public IActionResult Edit(int id)
        {
            var sp = _context.SanPhams.Find(id);
            if (sp == null) return NotFound();
            return View(sp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SanPham sp)
        {
            if (ModelState.IsValid)
            {
                _context.SanPhams.Update(sp);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(sp);
        }

        public IActionResult Delete(int id)
        {
            var sp = _context.SanPhams.Find(id);
            if (sp == null) return NotFound();
            return View(sp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var sp = _context.SanPhams.Find(id);
            if (sp != null)
            {
                _context.SanPhams.Remove(sp);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
