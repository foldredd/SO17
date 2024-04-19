using ApiServiceMVC.Helpers;

namespace ApiServiceMVC.Models.Orders.BaseClass
{
    public class BaseItem : IFieldsFull {
        public string Title { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }

        public decimal Price { get; set; }
        public BaseItem(string title, string SKU,int quantity, decimal price) {
            this.Title = title;
            this.SKU = SKU;
            this.Quantity = quantity;
            this.Price = price;
        }
        public BaseItem() { }
        public bool IsFull() {
            if (this == null) return false;
            if (string.IsNullOrWhiteSpace(Title)) return false;
            if (string.IsNullOrWhiteSpace(SKU)) return false;
            if (this.Quantity == null || this.Quantity == 0) return false;
            if (this.Price <= 0 || this.Price == null) return false;
            return true;
        }
    }
}
