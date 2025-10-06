using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VanPhongPhamDKT.Controllers
{
    public class Sanpham : Controller
    {
        // GET: Sanpham
        public ActionResult Index()
        {
            return View();
        }

        // GET: Sanpham/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Sanpham/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Sanpham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Sanpham/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Sanpham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Sanpham/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Sanpham/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
