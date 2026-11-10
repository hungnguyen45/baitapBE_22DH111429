
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
using baitapBE_22DH111429.Models;

namespace baitapBE_22DH111429.Controllers
{
    public class AccountController : Controller
    {
        private MystoreEntities2 db = new MystoreEntities2();

        [HttpGet]
        public ActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (model == null)
                model = new RegisterViewModel();

            if (!ModelState.IsValid)
                return View(model);

            string username = model.Username.Trim();

            bool exists = db.User.Any(u => u.Username == username);

            if (exists)
            {
                ModelState.AddModelError(
                    "Username",
                    "Tên đăng nhập đã tồn tại."
                );

                return View(model);
            }

            User user = new User
            {
                Username = username,
                Password = HashPassword(model.Password),
                UserRole = "U"
            };

            db.User.Add(user);
            db.SaveChanges();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (model == null)
                model = new LoginViewModel();

            if (!ModelState.IsValid)
                return View(model);

            string username = model.Username.Trim();

            var user = db.User.FirstOrDefault(
                u => u.Username == username
            );

            if (user == null ||
                !VerifyPassword(model.Password, user.Password))
            {
                ModelState.AddModelError(
                    "",
                    "Tên đăng nhập hoặc mật khẩu không chính xác."
                );

                return View(model);
            }

            Session["Username"] = user.Username;
            Session["UserRole"] = user.UserRole.Trim();

            TempData["Success"] = "Đăng nhập thành công!";

            if (user.UserRole.Trim() == "A")
            {
                return RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "Admin" }
                );
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            const int iterations = 100000;

            byte[] salt = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(32);
            }

            return "PBKDF2$" + iterations + "$"
                + Convert.ToBase64String(salt) + "$"
                + Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(
            string password,
            string storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword))
                return false;

            string[] parts = storedPassword.Split('$');

            if (parts.Length != 4 || parts[0] != "PBKDF2")
                return false;

            int iterations;

            if (!int.TryParse(parts[1], out iterations) ||
                iterations < 10000 ||
                iterations > 1000000)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expectedHash = Convert.FromBase64String(parts[3]);

                byte[] actualHash;

                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256))
                {
                    actualHash = pbkdf2.GetBytes(expectedHash.Length);
                }

                if (actualHash.Length != expectedHash.Length)
                    return false;

                int difference = 0;

                for (int i = 0; i < actualHash.Length; i++)
                {
                    difference |= actualHash[i] ^ expectedHash[i];
                }

                return difference == 0;
            }
            catch
            {
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
