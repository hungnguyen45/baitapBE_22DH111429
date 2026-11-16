using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;
namespace baitapBE_22DH111429.Areas.Admin.Controllers
{
    public class CustomersController : AdminControllerBase
    {
        private readonly MystoreEntities2 db=new MystoreEntities2();
        public ActionResult Index(string search)
        {
            var q=db.Customer.AsQueryable();if(!string.IsNullOrWhiteSpace(search))q=q.Where(c=>c.CustomerName.Contains(search)||c.CustomerPhone.Contains(search));
            ViewBag.Search=search;return View(q.OrderBy(c=>c.CustomerName).ToList());
        }
        public ActionResult Details(int id)
        {
            var c=db.Customer.Include("Order.OrderDetail.Product").FirstOrDefault(x=>x.CustomerID==id);if(c==null)return HttpNotFound();return View(c);
        }
        protected override void Dispose(bool disposing){if(disposing)db.Dispose();base.Dispose(disposing);}
    }
}
