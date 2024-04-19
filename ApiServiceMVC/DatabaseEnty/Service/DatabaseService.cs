using ApiServiceMVC.Helpers;
using ApiServiceMVC.Models;
using ApiServiceMVC.Models.Orders;
using ApiServiceMVC.Models.Orders.BaseClass;
using ApiServiceMVC.Models.Orders.CustomerWithItems;
using ApiServiceMVC.Models.Orders.OrderWithItems;
using ApiServiceMVC.Models.Orders.OrderWithItemsServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace ApiServiceMVC.DatabaseEnty.Service {
    public class DatabaseService {


        public bool RegistrationUser(BaseUser data, AuthorizationData authorization) {
            User localUser = new User();
            using (Database db = new Database())
            {
                var userInDb = (from user in db.Users
                                where user.Email == data.Email
                                select user).FirstOrDefault();
                if (userInDb == null)
                {
                    var adminRole = db.Roles.FirstOrDefault(r => r.Role_Name == "admin");

                    var user = new User
                    {
                        Login = data.Login,
                        Phone = data.Phone,
                        Email = data.Email,
                        Name = data.Name,
                        SurName = data.SurName,
                        PasswordHash = authorization.PasswordHash,
                        PasswordSalt = authorization.Salt,
                        Role = adminRole

                    };
                    localUser = user;
                    db.Users.Add(user);
                    int result = db.SaveChanges();
                    if (result == 0)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }

        }
        public User AuthorizationUser(BaseUser client) {
            User user;
            using (Database db = new Database())
            {
                UserHelper userHelper = new UserHelper(db);
                Console.WriteLine(client.Login);
                user = userHelper.FoundUser(client.Login);
                return user;
            }
        }
        public User GetProfile(string login) {
            User user;
            using (Database db = new Database())
            {
                UserHelper userHelper = new UserHelper(db);
                user = userHelper.FoundUser(login);
            }
            return user;
        }
        private bool AddItem(Database db ,Item item) {
            
                var itemInDb = (from itemdb in db.Items
                                where itemdb.SKU == item.SKU
                                select item).FirstOrDefault();
                if (itemInDb == null) {
                    db.Items.Add(item);
                    int result = db.SaveChanges();
                    if (result == 0)
                    {
                        return false;
                    }
                    return true;
                }
            
            return false;
        }
        public  bool AddItem(Item item) {
            using(Database db = new Database())
            {
                var itemInDb = (from itemdb in db.Items
                                where itemdb.SKU == item.SKU
                                select item).FirstOrDefault();
                if (itemInDb == null)
                {
                    db.Items.Add(item);
                    int result = db.SaveChanges();
                    if (result == 0)
                    {
                        return false;
                    }
                    return true;
                }

                return false;
            }
        }

        // Get limited Customers after skip number customers
        public List<Customer> GetCustomerList(int limit,int skip,string search) {
            if (!search.IsNullOrEmpty()){
                Console.WriteLine("Not null");
                using (Database db = new Database())
                {
                    var customerInDb = (from customer in db.Customers
                                        where customer.Phone.Contains(search)
                                        orderby customer.Id
                                        select customer)
                            .Skip(skip)
                            .Take(limit)
                            .ToList();
                    return customerInDb;
                }
            }
            Console.WriteLine("Null");
            using (Database db = new Database())
            {
                var customerInDb = (from customer in db.Customers
                                    orderby customer.Id
                                    select customer)
                        .Skip(skip)
                        .Take(limit)
                        .ToList();

                return customerInDb;
            }
        }
        public async Task<bool> NewCustomerAsync(Customer customer) {
            try {
                using (Database db = new Database())
                {
                    bool customerExists = await db.Customers.AnyAsync(c => c.Phone == customer.Phone);
                    if (!customerExists) {
                        db.Customers.Add(customer);
                        await db.SaveChangesAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex) {
                // Обработка исключений
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return false;
        }
       

        public  async Task<bool>  AddOrder(OrderWithItems orderWithItems) {
            try
            {
               
                using (Database db = new Database())
                {
                    int orderId = await OrderWithItemService.CreateOrder(db,orderWithItems.PhoneCustomer);
                    
                      // add list items
                       foreach(var baseitem in orderWithItems.Items){
                        Item item = new Item(baseitem.Title, baseitem.SKU, baseitem.Quantity, baseitem.Price);
                        AddItem(db,item);
                    }
                    bool result = await AddOrderItem(db, orderWithItems, orderId);
                    Console.WriteLine($"addorderitem= {result}");
                        if (result){
                            return true;
                        }
                }
            }catch(Exception ex){
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return false;
        }

        private static async Task<bool> AddOrderItem(Database db, OrderWithItems orderWithItems, int customerId) {
            try
            {
                foreach (var baseitem in orderWithItems.Items)
                {
                    Item item = new Item(baseitem.Title, baseitem.SKU, baseitem.Quantity, baseitem.Price);
                    OrderItem orderItem = new OrderItem(); // Создание нового экземпляра OrderItem для каждого элемента
                    orderItem.OrderId = customerId;
                    orderItem.ItemId = await GetItemIdBySku(db, item.SKU);
                    db.OrderItems.Add(orderItem);
                }
                await db.SaveChangesAsync(); // Сохранение всех созданных OrderItem в базу данных одновременно
                return true;
            }
            catch (Exception ex){
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }

        private static async Task<int> GetItemIdBySku(Database db,string sku) {
            int itemId = (from Item item in db.Items
                          where item.SKU == sku
                          select item.Id).FirstOrDefault();
            return  itemId;
        }
        public async Task<IEnumerable<Item>> GetItems() {
            using (var db = new Database()){

                var items = (from Item item in db.Items
                             select new { Title = item.Title, SKU = item.SKU }
             ).ToList();

                List<Item> itemList = new List<Item>();
                foreach (var anonymousItem in items){
                    Item newItem = new Item
                    {
                        Title = anonymousItem.Title,
                        SKU = anonymousItem.SKU
                        // Здесь вы можете установить остальные свойства объекта Item, если это необходимо
                    };
                    itemList.Add(newItem);
                }
                return itemList;
            }
        }
        public async Task<List<CustomerWithItems>> GetOrders(int skip, int limit,string phoneSearch) {
            if (string.IsNullOrEmpty(phoneSearch)){
                using (var db = new Database()){
                    var customersWithItems = db.Customers
                        .SelectMany(c => c.Orders, (customer, order) => new { Customer = customer, Order = order })
                        .SelectMany(co => co.Order.OrderItems, (co, orderItem) => new CustomerWithItems {
                            Customer = co.Customer,
                            Item = orderItem.Item,
                            OrderNumber = co.Order.OrderNumber
                        })
                        .Skip(skip)
                        .Take(limit)
                        .ToList();
                    return customersWithItems;
                }
            }
            else{
                using (var db = new Database()){
                    var customersWithItems = db.Customers
                        .Where(c => c.Phone.Contains(phoneSearch))
                        .SelectMany(c => c.Orders, (customer, order) => new { Customer = customer, Order = order })
                        .SelectMany(co => co.Order.OrderItems, (co, orderItem) => new CustomerWithItems
                        {
                            Customer = co.Customer,
                            Item = orderItem.Item
                        })
                        .Skip(skip)
                        .Take(limit)
                        .ToList();
                    return customersWithItems;
                }
            }
        }
    }
}
