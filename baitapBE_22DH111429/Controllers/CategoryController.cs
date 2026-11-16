using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;
namespace baitapBE_22DH111429.Controllers
{
    public class CategoryController : Controller
    {
        private readonly MystoreEntities2 db = new MystoreEntities2();
        public ActionResult Index(int? categoryId, string search, decimal? minPrice, decimal? maxPrice, string sort = "name", int page = 1)
        {
            var query = db.Product.Include("Category").Where(p => p.Quantity > 0);
            if (categoryId.HasValue) query = query.Where(p => p.CategoryID == categoryId.Value);
            if (!String.IsNullOrWhiteSpace(search)) query = query.Where(p => p.ProductName.Contains(search.Trim()));
            if (minPrice.HasValue) query = query.Where(p => p.ProductPrice >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.ProductPrice <= maxPrice.Value);
            ViewBag.Categories = db.Category.OrderBy(c => c.CategoryName).ToList();
            ViewBag.CategoryId = categoryId; ViewBag.Search = search; ViewBag.MinPrice = minPrice; ViewBag.MaxPrice = maxPrice; ViewBag.Sort = sort;
            const int pageSize = 12; var total = query.Count(); page = Math.Max(1, page);
            ViewBag.Page = page; ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            switch (sort) { case "price-asc": query = query.OrderBy(p => p.ProductPrice); break; case "price-desc": query = query.OrderByDescending(p => p.ProductPrice); break; default: query = query.OrderBy(p => p.ProductName); break; }
            return View(query.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
