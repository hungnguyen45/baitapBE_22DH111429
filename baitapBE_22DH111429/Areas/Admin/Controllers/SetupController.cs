using System;
using System.Linq;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;
namespace baitapBE_22DH111429.Areas.Admin.Controllers
{
    public class SetupController : Controller
    {
        private readonly MystoreEntities2 db=new MystoreEntities2();
        private bool Available { get { return Request.IsLocal && !db.User.Any(u=>u.UserRole.Trim()=="A"); } }
        public ActionResult Index(){if(!Available)return HttpNotFound();return View(new AdminUserEditViewModel{IsNew=true});}
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Index(AdminUserEditViewModel model)
        {
            if(!Available)return HttpNotFound();model.IsNew=true;
            if(String.IsNullOrWhiteSpace(model.Password))ModelState.AddModelError("Password","Vui lòng nhập mật khẩu.");
            if(!ModelState.IsValid)return View(model);
            var username=model.Username.Trim();if(db.User.Any(u=>u.Username==username)){ModelState.AddModelError("Username","Tên đăng nhập đã tồn tại.");return View(model);}
            db.User.Add(new User{Username=username,Password=PasswordUtility.Hash(model.Password),UserRole="A"});db.SaveChanges();TempData["Success"]="Đã tạo tài khoản quản trị đầu tiên. Hãy đăng nhập bằng tài khoản này.";return RedirectToAction("Login","Account",new{area=""});
        }
        protected override void Dispose(bool disposing){if(disposing)db.Dispose();base.Dispose(disposing);}
    }
}
