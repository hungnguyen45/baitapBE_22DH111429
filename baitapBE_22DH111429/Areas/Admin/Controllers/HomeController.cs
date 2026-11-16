using System.Linq;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;
namespace baitapBE_22DH111429.Areas.Admin.Controllers
{
    public class HomeController : AdminControllerBase
    {
        private readonly MystoreEntities2 db = new MystoreEntities2();
        public ActionResult Index()
        {
            ViewBag.ProductCount = db.Product.Count();
            ViewBag.CategoryCount = db.Category.Count();
            ViewBag.CustomerCount = db.Customer.Count();
            ViewBag.OrderCount = db.Order.Count();
            ViewBag.LowStockCount = db.Product.Count(p => p.Quantity == 0);
            return View();
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
