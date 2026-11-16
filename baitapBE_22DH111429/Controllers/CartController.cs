
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;

namespace baitapBE_22DH111429.Controllers
{
    public class CartController : Controller
    {
        private MystoreEntities2 db = new MystoreEntities2();

        // Lấy giỏ hàng từ Session
        private List<CartItem> GetCart()
        {
            var cart = Session["Cart"] as List<CartItem>;

            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }

            return cart;
        }

        // Trang giỏ hàng
        public ActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int id, int quantity = 1)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            var product = db.Product.Find(id);

            if (product == null)
            {
                return HttpNotFound();
            }
            if (product.Quantity < 1)
            {
                TempData["Error"] = "Sản phẩm hiện đã hết hàng.";
                return RedirectToAction("Index", "Home");
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item == null)
            {
                item = new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ProductPrice = product.ProductPrice,
                    ProductImage = product.ProductImage,
                    Quantity = Math.Min(product.Quantity, quantity)
                };
                cart.Add(item);
            }
            else
            {
                item.Quantity = Math.Min(product.Quantity, item.Quantity + quantity);
            }
            Session["Cart"] = cart;

            return RedirectToAction("Index");
        }

        // Cập nhật số lượng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    var product = db.Product.Find(id);
                    item.Quantity = product == null ? 0 : Math.Min(product.Quantity, quantity);
                    if (item.Quantity == 0) cart.Remove(item);
                }
            }

            Session["Cart"] = cart;
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(p => p.ProductID == id);

            if (item != null)
            {
                cart.Remove(item);
            }

            Session["Cart"] = cart;
            return RedirectToAction("Index");
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
