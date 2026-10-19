
using System.Linq;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;

namespace baitapBE_22DH111429.Controllers
{
    public class HomeController : Controller
    {
        private MystoreEntities2 db = new MystoreEntities2();

        public ActionResult Index()
        {
            var products = db.Product.ToList();
            return View(products);
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
