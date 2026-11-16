
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

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    User user = new User
                    {
                        Username = username,
                        Password = HashPassword(model.Password),
                        UserRole = "U"
                    };

                    Customer customer = new Customer
                    {
                        Username = username,
                        CustomerName = model.CustomerName.Trim(),
                        CustomerPhone = model.CustomerPhone.Trim(),
                        CustomerEmail = string.IsNullOrWhiteSpace(model.CustomerEmail)
                            ? null : model.CustomerEmail.Trim(),
                        CustomerAddress = string.IsNullOrWhiteSpace(model.CustomerAddress)
                            ? null : model.CustomerAddress.Trim(),
                        User = user
                    };

                    db.User.Add(user);
                    db.Customer.Add(customer);

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] =
                        "Đăng ký thành công! Vui lòng đăng nhập.";

                    return RedirectToAction("Login");
                }
                catch (Exception)
                {
                    transaction.Rollback();

                    ModelState.AddModelError(
                        "",
                        "Đăng ký không thành công. Vui lòng kiểm tra lại thông tin."
                    );

                    return View(model);
                }
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.ShowAdminSetup = Request.IsLocal && !db.User.Any(u => u.UserRole.Trim() == "A");
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (model == null)
                model = new LoginViewModel();

            if (!ModelState.IsValid)
            {
                ViewBag.ShowAdminSetup = Request.IsLocal && !db.User.Any(u => u.UserRole.Trim() == "A");
                return View(model);
            }

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
                ViewBag.ShowAdminSetup = Request.IsLocal && !db.User.Any(u => u.UserRole.Trim() == "A");

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

        [HttpGet]
        public new ActionResult Profile()
        {
            var username = Session["Username"] as string;
            if (String.IsNullOrEmpty(username)) return RedirectToAction("Login");
            var customer = db.Customer.FirstOrDefault(c => c.Username == username);
            return View(customer == null
                ? new CustomerProfileViewModel { CustomerName = username }
                : new CustomerProfileViewModel { CustomerName = customer.CustomerName, CustomerPhone = customer.CustomerPhone, CustomerEmail = customer.CustomerEmail, CustomerAddress = customer.CustomerAddress });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public new ActionResult Profile(CustomerProfileViewModel model)
        {
            var username = Session["Username"] as string;
            if (String.IsNullOrEmpty(username)) return RedirectToAction("Login");
            if (!ModelState.IsValid) return View(model);
            var customer = db.Customer.FirstOrDefault(c => c.Username == username);
            if (customer == null) { customer = new Customer { Username = username }; db.Customer.Add(customer); }
            customer.CustomerName = model.CustomerName.Trim();
            customer.CustomerPhone = model.CustomerPhone.Trim();
            customer.CustomerEmail = String.IsNullOrWhiteSpace(model.CustomerEmail) ? null : model.CustomerEmail.Trim();
            customer.CustomerAddress = String.IsNullOrWhiteSpace(model.CustomerAddress) ? null : model.CustomerAddress.Trim();
            db.SaveChanges();
            TempData["Success"] = "Đã lưu hồ sơ khách hàng.";
            return RedirectToAction("Profile");
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
