using ApiServiceMVC.Models.Orders.BaseClass;

namespace ApiServiceMVC.Models.Orders.CustomerWithItems {
    public class CustomerWithItems {
        
       public string OrderNumber { get; set; }
        public BaseCustomer Customer { get; set; }
        public BaseItem Item { get; set; }
    }
}
