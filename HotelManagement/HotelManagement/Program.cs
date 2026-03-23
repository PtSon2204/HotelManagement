using HotelManagement.Context;
using HotelManagement.Filters;
using HotelManagement.Repositories;
using HotelManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Session;
using HotelManagement.Models.Entities;

namespace HotelManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews(options =>
            {
                options.Conventions.Add(new AdminAreaConvention());
            });
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));
            builder.Services.AddScoped<BookingServiceHanlde>();
            builder.Services.AddScoped<BookingRepository>();
            builder.Services.AddScoped<CustomerRepository>();
            builder.Services.AddScoped<CustomerService>();
            builder.Services.AddScoped<RoomService>();
            builder.Services.AddScoped<RoomRepository>();
            builder.Services.AddScoped<ServiceRepository>();
            builder.Services.AddScoped<HotelServiceService>();
            builder.Services.AddScoped<InvoiceRepository>();
            builder.Services.AddScoped<InvoiceService>();

            builder.Services.AddSession();
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "RequestVerificationToken";
            });

            builder.Services.AddScoped<RoomRepository>();
            builder.Services.AddScoped<RoomService>();
            builder.Services.AddScoped<RoomTypeService>();
            builder.Services.AddScoped<StaffRepository>();
            builder.Services.AddScoped<StaffService>();
            builder.Services.AddScoped<ServiceRepository>();
            builder.Services.AddScoped<HotelServiceService>();
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<UserService>();

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (!context.Users.Any())
                {
                    // ====== STAFF ======
                    var staff1 = new Staff { FullName = "Staff 1" };
                    var staff2 = new Staff { FullName = "Staff 2" };

                    context.Staffs.AddRange(staff1, staff2);
                    context.SaveChanges();

                    // ====== CUSTOMER ======
                    var c1 = new Customer { FullName = "Customer 1" };
                    var c2 = new Customer { FullName = "Customer 2" };
                    var c3 = new Customer { FullName = "Customer 3" };

                    context.Customers.AddRange(c1, c2, c3);
                    context.SaveChanges();

                    // ====== USER ======
                    var users = new List<User>
        {
            new User {
                Username = "admin",
                PasswordHash = Hash("123"),
                Role = "Admin"
            },

            new User {
                Username = "staff1",
                PasswordHash = Hash("123"),
                Role = "Staff",
                StaffId = staff1.StaffId
            },
            new User {
                Username = "staff2",
                PasswordHash = Hash("123"),
                Role = "Staff",
                StaffId = staff2.StaffId
            },

            new User {
                Username = "cus1",
                PasswordHash = Hash("123"),
                Role = "Customer",
                CustomerId = c1.CustomerId
            },
            new User {
                Username = "cus2",
                PasswordHash = Hash("123"),
                Role = "Customer",
                CustomerId = c2.CustomerId
            },
            new User {
                Username = "cus3",
                PasswordHash = Hash("123"),
                Role = "Customer",
                CustomerId = c3.CustomerId
            }
        };

                    context.Users.AddRange(users);
                    context.SaveChanges();
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();       // must be before UseRouting so session is available everywhere

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
               name: "areaDefault",
               pattern: "{area:exists}/{controller=Rooms}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        static string Hash(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}