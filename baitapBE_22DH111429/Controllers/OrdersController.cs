using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;

namespace baitapBE_22DH111429.Controllers
{
    public class OrdersController : Controller
    {
        private readonly MystoreEntities2 db = new MystoreEntities2();
        private static readonly string[] Statuses = { "Chờ thanh toán", "Đang xử lý", "Đang vận chuyển", "Đã giao", "Đã hủy" };
        private string Username { get { return Session["Username"] as string; } }

        public ActionResult MyOrder(string search, string status = "Tất cả", string expand = null)
        {
            if (String.IsNullOrEmpty(Username)) return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyOrder") });
            var customer = db.Customer.FirstOrDefault(c => c.Username == Username);
            if (customer == null)
            {
                var emptyGroups = (status == "Tất cả" ? Statuses : Statuses.Where(s => s == status).ToArray())
                    .Select(s => new CustomerOrderGroup { Status = s, Orders = new List<Order>(), HasMore = false, Expanded = false }).ToList();
                return View(new CustomerOrdersViewModel { Search = search, ActiveStatus = status, Groups = emptyGroups, HasAnyOrders = false });
            }
            bool hasAnyOrders = db.Order.Any(o => o.CustomerID == customer.CustomerID);
            var orders = db.Order.Include("OrderDetail.Product").Where(o => o.CustomerID == customer.CustomerID);
            if (!String.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim(); int orderId;
                if (Int32.TryParse(term.TrimStart('#'), out orderId)) orders = orders.Where(o => o.OrderID == orderId || o.OrderDetail.Any(d => d.Product.ProductName.Contains(term)));
                else orders = orders.Where(o => o.OrderDetail.Any(d => d.Product.ProductName.Contains(term)));
            }
            var all = orders.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID).ToList();
            var groups = new List<CustomerOrderGroup>();
            var chosen = status == "Tất cả" ? Statuses : Statuses.Where(s => s == status).ToArray();
            foreach (var s in chosen)
            {
                var bucket = all.Where(o => o.OrderStatus == s).ToList();
                bool expanded = expand == s;
                groups.Add(new CustomerOrderGroup { Status = s, Orders = bucket.Take(expanded ? 50 : 2).ToList(), HasMore = !expanded && bucket.Count > 2, Expanded = expanded });
            }
            return View(new CustomerOrdersViewModel { Search = search, ActiveStatus = status, Groups = groups, HasAnyOrders = hasAnyOrders });
        }

        public ActionResult Details(int id)
        {
            if (String.IsNullOrEmpty(Username)) return RedirectToAction("Login", "Account");
            var customer = db.Customer.FirstOrDefault(c => c.Username == Username);
            if (customer == null) return HttpNotFound();
            var order = db.Order.Include("Customer").Include("OrderDetail.Product.Category")
                .FirstOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order == null) return HttpNotFound();
            return View(order);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult BuyAgain(int id)
        {
            if (String.IsNullOrEmpty(Username)) return RedirectToAction("Login", "Account");
            var customer = db.Customer.FirstOrDefault(c => c.Username == Username);
            var order = customer == null ? null : db.Order.Include("OrderDetail.Product").FirstOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order == null) return HttpNotFound();
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
            foreach (var line in order.OrderDetail.GroupBy(d => d.ProductID).Select(g => g.First()))
            {
                var p = line.Product;
                if (p == null || p.Quantity < 1) continue;
                var item = cart.FirstOrDefault(c => c.ProductID == p.ProductID);
                if (item == null) cart.Add(new CartItem { ProductID = p.ProductID, ProductName = p.ProductName, ProductPrice = p.ProductPrice, ProductImage = p.ProductImage, Quantity = 1 });
                else if (item.Quantity < p.Quantity) item.Quantity++;
            }
            Session["Cart"] = cart;
            TempData["Success"] = "Đã thêm các sản phẩm còn hàng vào giỏ.";
            return RedirectToAction("Index", "Cart");
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
