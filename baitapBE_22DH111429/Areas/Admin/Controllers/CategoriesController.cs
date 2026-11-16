using System.Linq;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;
namespace baitapBE_22DH111429.Areas.Admin.Controllers
{
    public class CategoriesController : AdminControllerBase
    {
        private readonly MystoreEntities2 db = new MystoreEntities2();
        public ActionResult Index() => View(db.Category.OrderBy(c => c.CategoryName).ToList());
        public ActionResult Create() => View(new CategoryEditViewModel());
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(CategoryEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var name = model.CategoryName.Trim();
            if (db.Category.Any(c => c.CategoryName == name)) { ModelState.AddModelError("CategoryName", "Tên danh mục đã tồn tại."); return View(model); }
            db.Category.Add(new Category { CategoryName = name }); db.SaveChanges(); TempData["Success"] = "Đã thêm danh mục."; return RedirectToAction("Index");
        }
        public ActionResult Edit(int id)
        {
            var item = db.Category.Find(id); if (item == null) return HttpNotFound();
            return View(new CategoryEditViewModel { CategoryID = item.CategoryID, CategoryName = item.CategoryName });
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(CategoryEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var item = db.Category.Find(model.CategoryID); if (item == null) return HttpNotFound();
            var name = model.CategoryName.Trim();
            if (db.Category.Any(c => c.CategoryName == name && c.CategoryID != item.CategoryID)) { ModelState.AddModelError("CategoryName", "Tên danh mục đã tồn tại."); return View(model); }
            item.CategoryName = name; db.SaveChanges(); TempData["Success"] = "Đã cập nhật danh mục."; return RedirectToAction("Index");
        }
        public ActionResult Delete(int id)
        {
            var item = db.Category.Find(id); if (item == null) return HttpNotFound();
            ViewBag.ProductCount = db.Product.Count(p => p.CategoryID == id); return View(item);
        }
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var item = db.Category.Find(id); if (item == null) return HttpNotFound();
            if (db.Product.Any(p => p.CategoryID == id)) { TempData["Error"] = "Không thể xóa danh mục đang có sản phẩm."; return RedirectToAction("Index"); }
            db.Category.Remove(item); db.SaveChanges(); TempData["Success"] = "Đã xóa danh mục."; return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
