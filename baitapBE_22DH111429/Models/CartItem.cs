
namespace baitapBE_22DH111429.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }

        public decimal Total
        {
            get { return ProductPrice * Quantity; }
        }
    }
}
