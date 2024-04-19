namespace ApiServiceMVC.Models.Orders.BaseClass {
    public class BaseCustomer {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public BaseCustomer(string name ,string surname, string phone) {
            this.Name = name;
            this.Surname = surname;
            this.Phone = phone;
        }
       public BaseCustomer() { }
    }
}
