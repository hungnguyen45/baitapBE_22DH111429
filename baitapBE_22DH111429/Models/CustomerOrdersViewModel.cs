using System.Collections.Generic;
namespace baitapBE_22DH111429.Models
{
    public class CustomerOrderGroup
    {
        public string Status { get; set; }
        public List<Order> Orders { get; set; }
        public bool HasMore { get; set; }
        public bool Expanded { get; set; }
    }
    public class CustomerOrdersViewModel
    {
        public string Search { get; set; }
        public string ActiveStatus { get; set; }
        public bool HasAnyOrders { get; set; }
        public List<CustomerOrderGroup> Groups { get; set; }
    }
}
