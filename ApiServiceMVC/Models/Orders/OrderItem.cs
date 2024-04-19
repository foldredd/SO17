using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiServiceMVC.Models.Orders {
    public class OrderItem {
        [Key]
        public int Id { get; set; }

        // Внешний ключ для заказа
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        // Внешний ключ для товара
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public Item Item { get; set; }
    }
}
