using HotelManagement.Context;
using HotelManagement.Filters;
//using HotelManagement.Repositories;
//using HotelManagement.Services;
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

            // 1. Cookie Authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/LoginRegister";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            // 2. SignalR
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

            // 3. MVC & Filters
            builder.Services.AddControllersWithViews(options =>
            {
                options.Conventions.Add(new AdminAreaConvention());
            });

            // 4. Database
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
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "RequestVerificationToken";
            });

            var app = builder.Build();

            // 7. Seed Data
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                //  THÊM ĐOẠN NÀY (seed Roles trước)
                if (!context.Roles.Any())
                {
                    context.Roles.AddRange(
                        new Role { RoleName = "Admin" },
                        new Role { RoleName = "Staff" },
                        new Role { RoleName = "Customer" }
                    );
                    context.SaveChanges();
                }

                if (!context.Users.Any())
                {
                    var adminRole = context.Roles.First(r => r.RoleName == "Admin");
                    var staffRole = context.Roles.First(r => r.RoleName == "Staff");
                    var customerRole = context.Roles.First(r => r.RoleName == "Customer");

                    var users = new List<User>
    {
        // 1 Tài khoản Admin
        new User {
            Username = "admin",
            PasswordHash = Hash("Admin@123"),
            RoleId = adminRole.RoleId,
            FullName = "Nguyễn Quản Trị",
            Gender = "Nam",
            DateOfBirth = new DateTime(1985, 5, 20),
            IDCard = "001085000123",
            Address = "123 Đường Láng, Đống Đa, Hà Nội",
            Nationality = "Việt Nam",
            Email = "admin@hotel.com",
            Phone = "0901234567",
            Image = "admin.jpg"
        },

        // 2 Tài khoản Staff
        new User {
            Username = "staff_lan",
            PasswordHash = Hash("Staff@123"),
            RoleId = staffRole.RoleId,
            FullName = "Mai Thị Lan",
            Gender = "Nữ",
            DateOfBirth = new DateTime(1998, 3, 15),
            IDCard = "001098000456",
            Address = "45 Cầu Giấy, Hà Nội",
            Nationality = "Việt Nam",
            Email = "lanmt@hotel.com",
            Phone = "0912345678",
            Image = "staff_lan.jpg"
        },
        new User {
            Username = "staff_hung",
            PasswordHash = Hash("Staff@123"),
            RoleId = staffRole.RoleId,
            FullName = "Trần Văn Hùng",
            Gender = "Nam",
            DateOfBirth = new DateTime(1995, 10, 10),
            IDCard = "001095000789",
            Address = "12 Trần Duy Hưng, Hà Nội",
            Nationality = "Việt Nam",
            Email = "hungtv@hotel.com",
            Phone = "0922345678",
            Image = "staff_hung.jpg"
        },

        // 3 Tài khoản Customer
        new User {
            Username = "cus_minh",
            PasswordHash = Hash("Customer@123"),
            RoleId = customerRole.RoleId,
            FullName = "Lê Quang Minh",
            Gender = "Nam",
            DateOfBirth = new DateTime(1990, 1, 1),
            IDCard = "040090000111",
            Address = "Phường Bến Nghé, Quận 1, TP.HCM",
            Nationality = "Việt Nam",
            Email = "minhlq@gmail.com",
            Phone = "0933111222",
            Image = "cus_minh.jpg"
        },
        new User {
            Username = "cus_elena",
            PasswordHash = Hash("Customer@123"),
            RoleId = customerRole.RoleId,
            FullName = "Elena Watson",
            Gender = "Nữ",
            DateOfBirth = new DateTime(1992, 12, 25),
            IDCard = "A12345678",
            Address = "London, UK",
            Nationality = "Anh",
            Email = "elena.w@yahoo.com",
            Phone = "044207123456",
            Image = "cus_elena.jpg"
        },
        new User {
            Username = "cus_binh",
            PasswordHash = Hash("Customer@123"),
            RoleId = customerRole.RoleId,
            FullName = "Phạm Thanh Bình",
            Gender = "Nữ",
            DateOfBirth = new DateTime(2000, 8, 20),
            IDCard = "030200000555",
            Address = "Ngô Quyền, Hải Phòng",
            Nationality = "Việt Nam",
            Email = "binhpt@hotmail.com",
            Phone = "0944555666",
            Image = "cus_binh.jpg"
        }
    };

                    context.Users.AddRange(users);
                    context.SaveChanges();
                }

                // 8. HTTP Pipeline
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
        }

        public static string Hash(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}