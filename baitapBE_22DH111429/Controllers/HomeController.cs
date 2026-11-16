
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;

namespace baitapBE_22DH111429.Controllers
{
    public class HomeController : Controller
    {
        private MystoreEntities2 db = new MystoreEntities2();

        public ActionResult Index()
        {
            var products = db.Product.Where(p => p.Quantity > 0).OrderBy(p => p.ProductName).ToList();
            return View(products);
        }

        public ActionResult Details(int id)
        {
            var product = db.Product.Include("Category").FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        [ChildActionOnly]
        public ActionResult CategoryMenu()
        {
            var categories = db.Category.OrderBy(c => c.CategoryName).ThenBy(c => c.CategoryID)
                .ToList().GroupBy(c => c.CategoryName).Select(g => g.First()).ToList();
            return PartialView("~/Views/Shared/_CategoryMenu.cshtml", categories);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
