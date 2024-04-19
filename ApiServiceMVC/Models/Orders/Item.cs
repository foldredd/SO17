using ApiServiceMVC.Models.Orders.BaseClass;

namespace ApiServiceMVC.Models.Orders
{
    public  class Item:BaseItem{
        public int Id { get; set; }

        public string Order_number { get; set; } = Guid.NewGuid().ToString();
        public ICollection<OrderItem> OrderItems { get; set; }
        public Item():base() { }
        public Item(string title, string SKU, int quantity, decimal price) :base(title, SKU, quantity, price) {}
    }

}
