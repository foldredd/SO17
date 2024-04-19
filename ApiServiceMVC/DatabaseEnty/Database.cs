using ApiServiceMVC.Models;
using ApiServiceMVC.Models.Orders;
using Microsoft.EntityFrameworkCore;
namespace ApiServiceMVC.DatabaseEnty {
    public class Database : DbContext {

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<Order> Orders { get; set; }=null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public Database() {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlServer("Server=localhost;Database=test;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}
