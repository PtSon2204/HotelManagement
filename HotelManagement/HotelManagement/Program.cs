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

            // 1. Thêm Cookie Authentication (Bắt buộc cho SignalR nhận diện User)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            // 2. Thêm SignalR và Provider nhận diện ID
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

            // Add services to the container.
            builder.Services.AddControllersWithViews(options =>
            {
                options.Conventions.Add(new AdminAreaConvention());
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

            // Đăng ký các Repository và Service
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
            builder.Services.AddScoped<RoomTypeService>();
            builder.Services.AddScoped<StaffRepository>();
            builder.Services.AddScoped<StaffService>();
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<UserService>();

            builder.Services.AddSession();
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "RequestVerificationToken";
            });

            var app = builder.Build();

            // Seed Data (Giữ nguyên logic của bạn)
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'dbo.Messages', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Messages](
        [MessageId] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [SenderName] [nvarchar](100) NOT NULL,
        [Content] [nvarchar](1000) NOT NULL,
        [SentAt] [datetime] NOT NULL CONSTRAINT [DF_Messages_SentAt] DEFAULT (GETDATE())
    );
END
");
                if (!context.Users.Any())
                {
                    var staff1 = new Staff { FullName = "Staff 1" };
                    var staff2 = new Staff { FullName = "Staff 2" };
                    context.Staffs.AddRange(staff1, staff2);
                    context.SaveChanges();

                    var c1 = new Customer { FullName = "Customer 1" };
                    var c2 = new Customer { FullName = "Customer 2" };
                    var c3 = new Customer { FullName = "Customer 3" };
                    context.Customers.AddRange(c1, c2, c3);
                    context.SaveChanges();

                    var users = new List<User>
                    {
                        new User { Username = "admin", PasswordHash = Hash("123"), Role = "Admin" },
                        new User { Username = "staff1", PasswordHash = Hash("123"), Role = "Staff", StaffId = staff1.StaffId },
                        new User { Username = "staff2", PasswordHash = Hash("123"), Role = "Staff", StaffId = staff2.StaffId },
                        new User { Username = "cus1", PasswordHash = Hash("123"), Role = "Customer", CustomerId = c1.CustomerId }
                    };
                    context.Users.AddRange(users);
                    context.SaveChanges();
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();

            // QUAN TRỌNG: Authentication phải đứng trước Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map SignalR Hub
            app.MapHub<ChatHub>("/chatHub");

            app.MapControllerRoute(
               name: "areaDefault",
               pattern: "{area:exists}/{controller=Rooms}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        // Sửa lỗi 'not all code paths return a value'
        public static string Hash(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}