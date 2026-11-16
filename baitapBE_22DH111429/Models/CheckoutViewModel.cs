using System.ComponentModel.DataAnnotations;
namespace baitapBE_22DH111429.Models
{
    public class CheckoutViewModel
    {
        [Required, StringLength(255)]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; }

        [Required, StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; }

        [EmailAddress, StringLength(255)]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; }

        [Required, StringLength(500)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string AddressDelivery { get; set; }
    }
}
