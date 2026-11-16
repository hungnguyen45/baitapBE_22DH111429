using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using baitapBE_22DH111429.Models;
namespace baitapBE_22DH111429.Areas.Admin.Controllers
{
    public class ProductsController : AdminControllerBase
    {
        private readonly MystoreEntities2 db = new MystoreEntities2();
        private void LoadCategories(int? selected = null) { ViewBag.Categories = new SelectList(db.Category.OrderBy(c => c.CategoryName).ToList(), "CategoryID", "CategoryName", selected); }
        private string SaveImage(HttpPostedFileBase image)
        {
            if (image == null || image.ContentLength == 0) return null;
            if (image.ContentLength > 5 * 1024 * 1024) throw new InvalidOperationException("Ảnh không được lớn hơn 5 MB.");
            var ext = Path.GetExtension(Path.GetFileName(image.FileName)).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(ext)) throw new InvalidOperationException("Chỉ chấp nhận ảnh JPG, PNG, GIF hoặc WEBP.");
            var folder = Server.MapPath("~/Content/Images/Products"); Directory.CreateDirectory(folder);
            var filename = Guid.NewGuid().ToString("N") + ext; image.SaveAs(Path.Combine(folder, filename));
            return "~/Content/Images/Products/" + filename;
        }
        public ActionResult Index(string search, int? categoryId, int page = 1)
        {
            var q = db.Product.Include("Category").AsQueryable();
            if (!String.IsNullOrWhiteSpace(search)) q = q.Where(p => p.ProductName.Contains(search.Trim()));
            if (categoryId.HasValue) q = q.Where(p => p.CategoryID == categoryId.Value);
            const int size = 15; var count = q.Count(); page = Math.Max(1, page);
            ViewBag.Search = search; ViewBag.CategoryId = categoryId; ViewBag.Categories = new SelectList(db.Category.OrderBy(c => c.CategoryName), "CategoryID", "CategoryName", categoryId); ViewBag.Page = page; ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)size));
            return View(q.OrderBy(p => p.ProductName).Skip((page - 1) * size).Take(size).ToList());
        }
        public ActionResult Create() { LoadCategories(); return View(new ProductEditViewModel()); }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(ProductEditViewModel model, HttpPostedFileBase image)
        {
            if (!ModelState.IsValid) { LoadCategories(model.CategoryID); return View(model); }
            try
            {
                if (!db.Category.Any(c => c.CategoryID == model.CategoryID)) { ModelState.AddModelError("CategoryID", "Danh mục không tồn tại."); LoadCategories(); return View(model); }
                var product = new Product { CategoryID = model.CategoryID, ProductName = model.ProductName.Trim(), ProductDecription = model.ProductDecription.Trim(), ProductPrice = model.ProductPrice, Quantity = model.Quantity, SoldQuantity = model.SoldQuantity, ProductImage = SaveImage(image) };
                db.Product.Add(product); db.SaveChanges(); TempData["Success"] = "Đã thêm sản phẩm."; return RedirectToAction("Index");
            }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); LoadCategories(model.CategoryID); return View(model); }
        }
        public ActionResult Edit(int id)
        {
            var p = db.Product.Find(id); if (p == null) return HttpNotFound(); LoadCategories(p.CategoryID);
            return View(new ProductEditViewModel { ProductID=p.ProductID, CategoryID=p.CategoryID, ProductName=p.ProductName, ProductDecription=p.ProductDecription, ProductPrice=p.ProductPrice, Quantity=p.Quantity, SoldQuantity=p.SoldQuantity, ProductImage=p.ProductImage });
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(ProductEditViewModel model, HttpPostedFileBase image)
        {
            if (!ModelState.IsValid) { LoadCategories(model.CategoryID); return View(model); }
            var p = db.Product.Find(model.ProductID); if (p == null) return HttpNotFound();
            try
            {
                var savedImage = SaveImage(image);
                p.CategoryID=model.CategoryID; p.ProductName=model.ProductName.Trim(); p.ProductDecription=model.ProductDecription.Trim(); p.ProductPrice=model.ProductPrice; p.Quantity=model.Quantity; p.SoldQuantity=model.SoldQuantity;
                if (savedImage != null) p.ProductImage=savedImage;
                db.SaveChanges(); TempData["Success"]="Đã cập nhật sản phẩm."; return RedirectToAction("Index");
            }
            catch (Exception ex) { ModelState.AddModelError("", ex.Message); LoadCategories(model.CategoryID); return View(model); }
        }
        public ActionResult Details(int id) { var p=db.Product.Include("Category").FirstOrDefault(x=>x.ProductID==id); return p==null?(ActionResult)HttpNotFound():View(p); }
        public ActionResult Delete(int id) { var p=db.Product.Include("Category").FirstOrDefault(x=>x.ProductID==id); if(p==null)return HttpNotFound(); ViewBag.HasOrders=db.OrderDetail.Any(d=>d.ProductID==id); return View(p); }
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var p=db.Product.Find(id); if(p==null)return HttpNotFound();
            if(db.OrderDetail.Any(d=>d.ProductID==id)){TempData["Error"]="Không thể xóa sản phẩm đã có trong đơn hàng.";return RedirectToAction("Index");}
            db.Product.Remove(p);db.SaveChanges();TempData["Success"]="Đã xóa sản phẩm.";return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing){if(disposing)db.Dispose();base.Dispose(disposing);}
    }
}
