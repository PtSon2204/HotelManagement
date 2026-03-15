using HotelManagement.Context;
using HotelManagement.Filters;
using HotelManagement.Repositories;
using HotelManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Session;

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

            builder.Services.AddSession();

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
    }
}
