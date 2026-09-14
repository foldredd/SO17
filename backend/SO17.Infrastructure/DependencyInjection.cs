using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SO17.Application.Abstractions;
using SO17.Application.Accounts;
using SO17.Application.Customers;
using SO17.Application.Items;
using SO17.Application.Orders;
using SO17.Infrastructure.Identity;
using SO17.Infrastructure.Persistence;

namespace SO17.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Configure ConnectionStrings__DefaultConnection or use .NET User Secrets.");
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));
        services.Configure<RegistrationOptions>(configuration.GetSection("Registration"));
        services.AddIdentityCore<AppUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Password.RequiredUniqueChars = 4;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        }).AddEntityFrameworkStores<AppDbContext>().AddSignInManager();
        services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }
}
