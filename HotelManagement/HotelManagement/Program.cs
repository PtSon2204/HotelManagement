using HotelManagement.Context;
using HotelManagement.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using HotelManagement.Hubs;
using Microsoft.AspNetCore.SignalR;
using HotelManagement.Helpers;
using HotelManagement.Models.Entities;
using HotelManagement.Repositories;
using HotelManagement.Services;

namespace HotelManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/LoginRegister";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            // 2. SignalR
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

            // 3. MVC
            builder.Services.AddControllersWithViews(options =>
            {
                options.Conventions.Add(new AdminAreaConvention());
            });

            // 4. DB
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

            // 5. Repositories
            builder.Services.AddScoped<BookingRepository>();
            builder.Services.AddScoped<RoomRepository>();
            builder.Services.AddScoped<ServiceRepository>();
            builder.Services.AddScoped<InvoiceRepository>();
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<FeedbackRepository>();
            builder.Services.AddScoped<SurchargeRepository>();

            // 6. Services
            builder.Services.AddScoped<BookingServiceHandle>();
            builder.Services.AddScoped<RoomService>();
            builder.Services.AddScoped<ServiceHotelService>();
            builder.Services.AddScoped<InvoiceService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<FeedbackService>();
            builder.Services.AddScoped<SurchargeService>();

            builder.Services.AddSession();

            var app = builder.Build();

            // ✅ SEED DATA
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                SeedData(context);
            }

            // Pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<ChatHub>("/chatHub");

            app.MapControllerRoute(
                name: "areaDefault",
                pattern: "{area:exists}/{controller=Rooms}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        // ================= SEED METHOD =================
        private static void SeedData(ApplicationDbContext context)
        {
            // 1. Seed Roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "Staff" },
                    new Role { RoleName = "Customer" }
                );
                context.SaveChanges();
            }

            // 2. Seed Users
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
                var staffRole = context.Roles.FirstOrDefault(r => r.RoleName == "Staff");
                var customerRole = context.Roles.FirstOrDefault(r => r.RoleName == "Customer");

                if (adminRole == null || staffRole == null || customerRole == null)
                    return; // tránh crash

                var users = new List<User>
                {
                    new User {
                        Username = "admin",
                        PasswordHash = Hash("Admin@123"),
                        RoleId = adminRole.RoleId,
                        FullName = "Nguyễn Quản Trị",
                        Gender = "Nam",
                        DateOfBirth = new DateTime(1985, 5, 20),
                        IDCard = "001085000123",
                        Address = "Hà Nội",
                        Nationality = "Việt Nam",
                        Email = "admin@hotel.com",
                        Phone = "0901234567",
                        Image = "admin.jpg"
                    },

                    new User {
                        Username = "staff1",
                        PasswordHash = Hash("123"),
                        RoleId = staffRole.RoleId,
                        FullName = "Staff Demo"
                    },

                    new User {
                        Username = "cus1",
                        PasswordHash = Hash("123"),
                        RoleId = customerRole.RoleId,
                        FullName = "Customer Demo"
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }
        }

        // ================= HASH =================
        public static string Hash(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}