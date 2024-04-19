using ApiServiceMVC.Models.Orders;
using ApiServiceMVC.Models.Orders.BaseClass;
using ApiServiceMVC.Models.Orders.OrderWithItems;

namespace ApiServiceMVC.DatabaseEnty.Service {
    public static class ConvertService {

        public static List<BaseCustomer> CustomerToBaseCustomerList(List<Customer> customers) {
            List<BaseCustomer> baseCustomerList = customers.ConvertAll(customer => (BaseCustomer)customer);
            return baseCustomerList;
        }
       /* public static Item ConvertToItem(BaseItem baseItem) {
            Item item = new Item();
            item.Title = baseItem.Title;
            item.SKU = baseItem.SKU;
            item.Quantity = baseItem.Quantity;
             return item;
        }*/
    }
}
