using ApiServiceMVC.Models.Orders.BaseClass;
namespace ApiServiceMVC.Models.Orders
{
    public class Customer:BaseCustomer
    {
        public int Id { get; set; }
        
        public List<Order> Orders { get; set; }

        public Customer(string name,string surname, string phone):base(name,surname,phone) {}
        public Customer() {}

    }
}
