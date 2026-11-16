using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;

namespace baitapBE_22DH111429.Areas.Admin.Controllers
{
    public class OrdersController : AdminControllerBase
    {
        private readonly MystoreEntities2 db = new MystoreEntities2();
        private static readonly string[] Statuses = { "Chờ thanh toán", "Đang xử lý", "Đang vận chuyển", "Đã giao", "Đã hủy" };

        private ActionResult RequireAdmin()
        {
            if (Session["UserRole"] == null || Session["UserRole"].ToString() != "A")
                return RedirectToAction("Login", "Account", new { area = "", returnUrl = "/Admin/Orders" });
            return null;
        }

        public ActionResult Index(string customerName, string paymentStatus, DateTime? fromDate, DateTime? toDate, string sort = "date", string direction = "desc", int page = 1)
        {
            var denied = RequireAdmin(); if (denied != null) return denied;
            var query = db.Order.Include("Customer").AsQueryable();
            if (!String.IsNullOrWhiteSpace(customerName)) query = query.Where(o => o.Customer.CustomerName.Contains(customerName));
            if (!String.IsNullOrWhiteSpace(paymentStatus)) query = query.Where(o => o.PaymentStatus == paymentStatus);
            if (fromDate.HasValue) query = query.Where(o => o.OrderDate >= fromDate.Value);
            if (toDate.HasValue) { var end = toDate.Value.Date.AddDays(1); query = query.Where(o => o.OrderDate < end); }
            const int pageSize = 15;
            var total = query.Count(); page = Math.Max(1, page);
            ViewBag.CustomerName = customerName; ViewBag.PaymentStatus = paymentStatus;
            ViewBag.FromDate = fromDate.HasValue ? fromDate.Value.ToString("yyyy-MM-dd") : "";
            ViewBag.ToDate = toDate.HasValue ? toDate.Value.ToString("yyyy-MM-dd") : "";
            ViewBag.Sort = sort; ViewBag.Direction = direction;
            ViewBag.Page = page; ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            ViewBag.PaymentStatuses = new[] { "Chưa thanh toán", "Đã thanh toán" };
            bool ascending = direction == "asc";
            switch (sort)
            {
                case "customer": query = ascending ? query.OrderBy(o => o.Customer.CustomerName) : query.OrderByDescending(o => o.Customer.CustomerName); break;
                case "total": query = ascending ? query.OrderBy(o => o.TotalAmount) : query.OrderByDescending(o => o.TotalAmount); break;
                case "status": query = ascending ? query.OrderBy(o => o.OrderStatus) : query.OrderByDescending(o => o.OrderStatus); break;
                default: query = ascending ? query.OrderBy(o => o.OrderDate).ThenBy(o => o.OrderID) : query.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID); break;
            }
            return View(query.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        public ActionResult Details(int id)
        {
            var denied = RequireAdmin(); if (denied != null) return denied;
            var order = db.Order.Include("Customer").Include("OrderDetail.Product").FirstOrDefault(o => o.OrderID == id);
            if (order == null) return HttpNotFound();
            ViewBag.Statuses = Statuses;
            return View("DetailedOrder", order);
        }

        [HttpGet]
        public ActionResult UpdateStatus(int id)
        {
            var denied = RequireAdmin(); if (denied != null) return denied;
            var order = db.Order.Include("Customer").FirstOrDefault(o => o.OrderID == id);
            if (order == null) return HttpNotFound();
            ViewBag.Statuses = Statuses;
            return View(order);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string orderStatus, string paymentStatus)
        {
            var denied = RequireAdmin(); if (denied != null) return denied;
            if (!Statuses.Contains(orderStatus)) return new HttpStatusCodeResult(400, "Trạng thái đơn không hợp lệ.");
            var order = db.Order.Include("OrderDetail.Product").FirstOrDefault(o => o.OrderID == id);
            if (order == null) return HttpNotFound();
            bool wasCancelled = order.OrderStatus == "Đã hủy";
            bool nowCancelled = orderStatus == "Đã hủy";
            if (!wasCancelled && nowCancelled)
                foreach (var line in order.OrderDetail) { line.Product.Quantity += line.Quantity; line.Product.SoldQuantity = Math.Max(0, line.Product.SoldQuantity - line.Quantity); }
            if (wasCancelled && !nowCancelled)
            {
                foreach (var line in order.OrderDetail)
                {
                    if (line.Product.Quantity < line.Quantity) { TempData["Error"] = "Tồn kho không đủ để mở lại đơn."; return RedirectToAction("Details", new { id }); }
                }
                foreach (var line in order.OrderDetail) { line.Product.Quantity -= line.Quantity; line.Product.SoldQuantity += line.Quantity; }
            }
            order.OrderStatus = orderStatus;
            if (paymentStatus == "Chưa thanh toán" || paymentStatus == "Đã thanh toán") order.PaymentStatus = paymentStatus;
            db.SaveChanges(); TempData["Success"] = "Đã cập nhật đơn hàng.";
            return RedirectToAction("Details", new { id });
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
