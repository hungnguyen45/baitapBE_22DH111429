using System.Linq;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;
namespace baitapBE_22DH111429.Areas.Admin.Controllers
{
    public class WarehouseController : AdminControllerBase
    {
        private readonly MystoreEntities2 db=new MystoreEntities2();
        public ActionResult Index()
        {
            ViewBag.InStock=db.Product.Where(p=>p.Quantity>0).OrderBy(p=>p.ProductName).ToList();
            ViewBag.NeedRestock=db.Product.Where(p=>p.Quantity==0).OrderBy(p=>p.ProductName).ToList();
            return View();
        }
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Update(int id,int quantity)
        {
            if(quantity<0){TempData["Error"]="Số lượng không thể âm.";return RedirectToAction("Index");}
            var p=db.Product.Find(id);if(p==null)return HttpNotFound();p.Quantity=quantity;db.SaveChanges();TempData["Success"]="Đã cập nhật tồn kho.";return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing){if(disposing)db.Dispose();base.Dispose(disposing);}
    }
}
