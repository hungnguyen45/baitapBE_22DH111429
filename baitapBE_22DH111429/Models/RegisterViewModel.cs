
using System.ComponentModel.DataAnnotations;

namespace baitapBE_22DH111429.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(255, ErrorMessage = "Tên đăng nhập không được quá 255 ký tự")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(255, ErrorMessage = "Họ tên không được quá 255 ký tự")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được quá 15 ký tự")]
        public string CustomerPhone { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email không được quá 255 ký tự")]
        public string CustomerEmail { get; set; }

        [StringLength(500, ErrorMessage = "Địa chỉ không được quá 500 ký tự")]
        public string CustomerAddress { get; set; }
    }
}
