using ApiServiceMVC.Models.Orders.BaseClass;

namespace ApiServiceMVC.Models.Orders.OrderWithItems {
    public class OrderWithItems {
        public List<BaseItem> Items { get; set; }
        public  string PhoneCustomer { get; set; }
        public string OrderNumber { get; set; }

        public bool isFull() {
            if (this == null) return false;
          if(string.IsNullOrWhiteSpace(PhoneCustomer))return false;
          if(Items == null || Items.Count == 0) return false;
          return true;
        }
    }

   
}
