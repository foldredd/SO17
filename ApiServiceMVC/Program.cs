using ApiServiceMVC.JWT;
using ApiServiceMVC.Models;
using ApiServiceMVC.Web.Api.Infrastructure.Loggers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ApiServiceMVC.DatabaseEnty;
using ApiServiceMVC.DatabaseEnty.Service;
using Microsoft.AspNetCore.Mvc;

namespace ApiServiceMVC {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();
            builder.Services.AddScoped<Database>();
            builder.Services.AddScoped<DatabaseService>();
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("http://localhost:4200")
                            .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JWTConfig.ISSUER,
            ValidateAudience = true,
            ValidAudience = JWTConfig.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = JWTConfig.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true
        };
    });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
         
            app.UseMiddleware<LoggerMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Auth}/{action=Index}/{id?}");
                app.MapControllerRoute(
                    name: "registration",
                    pattern: "registration",
                    defaults : new {controller = "Auth", action = "Registration" }
                    );
                app.MapControllerRoute(
                    name: "login",
                    pattern:"login",
                    defaults: new {controller = "Auth", action = "Login"}
                    );
                app.MapControllerRoute(
                    name: "profile",
                    pattern: "profile/{login?}",
                    defaults: new { controller = "Profile", action = "Profile" }
                    );

                app.MapControllerRoute(
                    name: "createitem",
                    pattern: "/createitem",
                    defaults: new {controller ="Item",action="CreateItem"}
                    );
                app.MapControllerRoute(
                    name: "getcustomer",
                    pattern: "/customers",
                    defaults: new { controller = "Customer",action= "GetCustomer" }
                    );
                app.MapControllerRoute(
                    name: "newcustomer",
                    pattern: "/newcustomer",
                    defaults: new { controller = "Customer", action = "NewCustomer" }
                    );
                app.MapControllerRoute(
                    name:"addorder",
                    pattern: "/addorder",
                    defaults: new {controller ="Order", action="AddOrder"}
                    );
                app.MapControllerRoute(
                    name: "orders",
                    pattern:"orders",
                    defaults: new {controller="Order", action="GetOrders"}
                    );
            });

           

            app.Run();
        }

       
    }

}