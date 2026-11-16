using System.ComponentModel.DataAnnotations;
namespace baitapBE_22DH111429.Models
{
    public class ProductEditViewModel
    {
        public int ProductID { get; set; }
        [Required, Range(1, int.MaxValue)] public int CategoryID { get; set; }
        [Required, StringLength(200)] public string ProductName { get; set; }
        [Required, StringLength(2000)] public string ProductDecription { get; set; }
        [Required, Range(typeof(decimal), "0.01", "9999999999")] public decimal ProductPrice { get; set; }
        [Range(0, int.MaxValue)] public int Quantity { get; set; }
        [Range(0, int.MaxValue)] public int SoldQuantity { get; set; }
        public string ProductImage { get; set; }
    }
    public class CategoryEditViewModel
    {
        public int CategoryID { get; set; }
        [Required, StringLength(100)] public string CategoryName { get; set; }
    }
    public class AdminUserEditViewModel
    {
        [Required, StringLength(255)] public string Username { get; set; }
        [StringLength(255, MinimumLength = 6)] public string Password { get; set; }
        [Compare("Password")] public string ConfirmPassword { get; set; }
        public bool IsNew { get; set; }
    }
    public class CustomerProfileViewModel
    {
        [Required, StringLength(255)] public string CustomerName { get; set; }
        [Required, StringLength(15)] public string CustomerPhone { get; set; }
        [EmailAddress, StringLength(255)] public string CustomerEmail { get; set; }
        [StringLength(500)] public string CustomerAddress { get; set; }
    }
}
