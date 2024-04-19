using ApiServiceMVC.DatabaseEnty;
using ApiServiceMVC.Models.Orders.OrderWithItemsServices;
using Microsoft.EntityFrameworkCore;
using System;

namespace ApiServiceMVC.Models.Orders.OrderWithItemsServices {
    public class OrderWithItemService {

        private static Customer GetCustomer(Database db, string Phone) {
            var getcustomer = (from Customer customer in db.Customers
                               where customer.Phone == Phone
                               select customer
                               ).FirstOrDefault();
            return getcustomer;
        }
        public static  string UUIDGenerator() {
            Guid myuuid = Guid.NewGuid();
            string myuuidAsString = myuuid.ToString();
            return myuuidAsString;
        }

        public static async Task<int> CreateOrder(Database db, string Phone){
            try{
                Customer customer = GetCustomer(db, Phone);
                if (customer != null){
                    Order order = new Order();
                    bool exists;
                    order.CustomerId = customer.Id;
                    string uuid;
                    do{
                        uuid = UUIDGenerator();
                        exists = await db.Orders.AnyAsync(o => o.OrderNumber == uuid);
                    } while (exists);
                    order.OrderNumber = uuid;
                    order.CreatedDate = DateTime.Now;
                    order.CustomerId = customer.Id;
                    db.Orders.Add(order);
                    await db.SaveChangesAsync();
                    return order.Id;
                }
            }catch (Exception ex)
            {
                return 0;
            }
            return 0;
        }

         
        
    }

    
}
