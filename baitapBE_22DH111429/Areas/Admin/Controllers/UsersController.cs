using System;
using System.Linq;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;
namespace baitapBE_22DH111429.Areas.Admin.Controllers
{
    public class UsersController : AdminControllerBase
    {
        private readonly MystoreEntities2 db = new MystoreEntities2();
        public ActionResult Index() => View(db.User.Where(u => u.UserRole.Trim() == "A").OrderBy(u => u.Username).ToList());
        public ActionResult Create() => View(new AdminUserEditViewModel { IsNew = true });
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(AdminUserEditViewModel model)
        {
            model.IsNew = true;
            if (String.IsNullOrWhiteSpace(model.Password)) ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu.");
            if (!ModelState.IsValid) return View(model);
            var username=model.Username.Trim(); if(db.User.Any(u=>u.Username==username)){ModelState.AddModelError("Username","Tên đăng nhập đã tồn tại.");return View(model);}
            db.User.Add(new User { Username=username, Password=PasswordUtility.Hash(model.Password), UserRole="A" }); db.SaveChanges(); TempData["Success"]="Đã tạo tài khoản quản trị."; return RedirectToAction("Index");
        }
        public ActionResult Edit(string id)
        {
            var user=db.User.FirstOrDefault(u=>u.Username==id && u.UserRole.Trim()=="A"); if(user==null)return HttpNotFound();
            return View(new AdminUserEditViewModel { Username=user.Username, IsNew=false });
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(AdminUserEditViewModel model)
        {
            model.IsNew=false;
            if(!ModelState.IsValid)return View(model);
            var user=db.User.FirstOrDefault(u=>u.Username==model.Username && u.UserRole.Trim()=="A"); if(user==null)return HttpNotFound();
            if(!String.IsNullOrWhiteSpace(model.Password))user.Password=PasswordUtility.Hash(model.Password);
            db.SaveChanges();TempData["Success"]="Đã cập nhật tài khoản quản trị.";return RedirectToAction("Index");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            if(id==(Session["Username"] as string)){TempData["Error"]="Không thể xóa tài khoản đang đăng nhập.";return RedirectToAction("Index");}
            var user=db.User.FirstOrDefault(u=>u.Username==id && u.UserRole.Trim()=="A");if(user==null)return HttpNotFound();
            if(db.Customer.Any(c=>c.Username==id)){TempData["Error"]="Tài khoản đang liên kết với hồ sơ khách hàng nên không thể xóa.";return RedirectToAction("Index");}
            db.User.Remove(user);db.SaveChanges();TempData["Success"]="Đã xóa tài khoản quản trị.";return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing){if(disposing)db.Dispose();base.Dispose(disposing);}
    }
}
