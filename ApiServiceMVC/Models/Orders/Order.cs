using ApiServiceMVC.Models.Orders.BaseClass;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiServiceMVC.Models.Orders {
    public class Order  {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string OrderNumber { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
