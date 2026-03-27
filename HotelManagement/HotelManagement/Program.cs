using HotelManagement.Context;
using HotelManagement.Filters;
using HotelManagement.Repositories;
using HotelManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using HotelManagement.Hubs;
using Microsoft.AspNetCore.SignalR;
using HotelManagement.Helpers;
using HotelManagement.Models.Entities;

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
            builder.Services.AddScoped<CustomerRepository>();
            builder.Services.AddScoped<RoomRepository>();
            builder.Services.AddScoped<ServiceRepository>();
            builder.Services.AddScoped<InvoiceRepository>();
            builder.Services.AddScoped<RoomTypeRepository>();
            builder.Services.AddScoped<StaffRepository>();
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<FeedbackRepository>();
            builder.Services.AddScoped<RoomBookingRepository>();

            // 6. Services
            builder.Services.AddScoped<BookingServiceHanlde>();
            builder.Services.AddScoped<CustomerService>();
            builder.Services.AddScoped<RoomService>();
            builder.Services.AddScoped<HotelServiceService>();
            builder.Services.AddScoped<InvoiceService>();
            builder.Services.AddScoped<RoomTypeService>();
            builder.Services.AddScoped<StaffService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<FeedbackService>();

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

                // Tạo bảng Messages nếu chưa có
                context.Database.ExecuteSqlRaw(@"
                    IF OBJECT_ID(N'dbo.Messages', N'U') IS NULL
                    BEGIN
                        CREATE TABLE [dbo].[Messages](
                            [MessageId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [SenderName] [nvarchar](100) NOT NULL,
                            [Content] [nvarchar](1000) NOT NULL,
                            [SentAt] [datetime] NOT NULL CONSTRAINT [DF_Messages_SentAt] DEFAULT (GETDATE())
                        );
                    END");

                // Seed Roles và Users nếu chưa có
                if (!context.Roles.Any())
                {
                    var adminRole = new Role { RoleName = "Admin" };
                    var staffRole = new Role { RoleName = "Staff" };
                    var customerRole = new Role { RoleName = "Customer" };
                    context.Roles.AddRange(adminRole, staffRole, customerRole);
                    context.SaveChanges();
                }

                if (!context.Users.Any())
                {
                    var adminRole = context.Roles.First(r => r.RoleName == "Admin");
                    var staffRole = context.Roles.First(r => r.RoleName == "Staff");
                    var customerRole = context.Roles.First(r => r.RoleName == "Customer");

                    var users = new List<User>
                    {
                        new User { Username = "admin", PasswordHash = Hash("123"), RoleId = adminRole.RoleId, FullName = "Administrator" },
                        new User { Username = "staff1", PasswordHash = Hash("123"), RoleId = staffRole.RoleId, FullName = "Staff 1" },
                        new User { Username = "staff2", PasswordHash = Hash("123"), RoleId = staffRole.RoleId, FullName = "Staff 2" },
                        new User { Username = "cus1", PasswordHash = Hash("123"), RoleId = customerRole.RoleId, FullName = "Customer 1" },
                        new User { Username = "cus2", PasswordHash = Hash("123"), RoleId = customerRole.RoleId, FullName = "Customer 2" },
                        new User { Username = "cus3", PasswordHash = Hash("123"), RoleId = customerRole.RoleId, FullName = "Customer 3" },
                    };

                    context.Users.AddRange(users);
                    context.SaveChanges();
                }
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

        public static string Hash(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}