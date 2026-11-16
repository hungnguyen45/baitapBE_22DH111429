using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;

namespace baitapBE_22DH111429.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly MystoreEntities2 db = new MystoreEntities2();
        public ActionResult Index()
        {
            var username = Session["Username"] as string;
            if (String.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index") });
            var customer = db.Customer.FirstOrDefault(c => c.Username == username);
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
            if (!cart.Any()) return RedirectToAction("Index", "Cart");
            return View(new CheckoutViewModel
            {
                CustomerName = customer == null ? username : customer.CustomerName,
                CustomerPhone = customer == null ? "" : customer.CustomerPhone,
                CustomerEmail = customer == null ? "" : customer.CustomerEmail,
                AddressDelivery = customer == null ? "" : customer.CustomerAddress
            });
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Index(CheckoutViewModel model)
        {
            var username = Session["Username"] as string;
            if (String.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");
            var customer = db.Customer.FirstOrDefault(c => c.Username == username);
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
            if (!cart.Any()) return RedirectToAction("Index", "Cart");
            if (!ModelState.IsValid) return View(model);
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    if (customer == null)
                    {
                        customer = new Customer
                        {
                            Username = username,
                            CustomerName = model.CustomerName.Trim(),
                            CustomerPhone = model.CustomerPhone.Trim(),
                            CustomerEmail = String.IsNullOrWhiteSpace(model.CustomerEmail) ? null : model.CustomerEmail.Trim(),
                            CustomerAddress = model.AddressDelivery.Trim()
                        };
                        db.Customer.Add(customer);
                        db.SaveChanges();
                    }
                    var order = new Order { CustomerID = customer.CustomerID, OrderDate = DateTime.Today, AddressDelivery = model.AddressDelivery.Trim(), PaymentStatus = "Chưa thanh toán", OrderStatus = "Chờ thanh toán", TotalAmount = 0m };
                    foreach (var item in cart)
                    {
                        var product = db.Product.Find(item.ProductID);
                        if (product == null || product.Quantity < item.Quantity)
                        {
                            ModelState.AddModelError("", "Tồn kho không đủ cho sản phẩm: " + item.ProductName);
                            tx.Rollback(); return View(model);
                        }
                        var qty = Math.Max(1, item.Quantity);
                        order.OrderDetail.Add(new OrderDetail { ProductID = product.ProductID, Quantity = qty, UnitPrice = product.ProductPrice });
                        order.TotalAmount += product.ProductPrice * qty;
                        product.Quantity -= qty; product.SoldQuantity += qty;
                    }
                    db.Order.Add(order); db.SaveChanges(); tx.Commit();
                    Session["Cart"] = new List<CartItem>();
                    TempData["Success"] = "Đặt hàng thành công. Mã đơn: #" + order.OrderID;
                    return RedirectToAction("MyOrder", "Orders");
                }
                catch
                {
                    tx.Rollback(); ModelState.AddModelError("", "Chưa thể tạo đơn hàng. Vui lòng thử lại."); return View(model);
                }
            }
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
