using HotelManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Context;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public virtual DbSet<Booking> Bookings { get; set; }
    public virtual DbSet<BookingService> BookingServices { get; set; }
    public virtual DbSet<Feedback> Feedbacks { get; set; }
    public virtual DbSet<Image> Images { get; set; }
    public virtual DbSet<Invoice> Invoices { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Room> Rooms { get; set; }
    public virtual DbSet<Service> Services { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<GuestProfile> GuestProfiles { get; set; }
    public virtual DbSet<AccountActivation> AccountActivations { get; set; }
    public virtual DbSet<AdditionalCharge> AdditionalCharges { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cấu hình quan hệ 1-1 giữa Booking và Invoice
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Invoice)
            .WithOne(i => i.Booking)
            .HasForeignKey<Invoice>(i => i.BookingId);

        // Chặn Cascade Delete gây lỗi vòng lặp
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Feedback>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Chặn cấu hình Delete để lịch sử Bookings không bao giờ mất thông tin khi GuestProfile bị xóa
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.GuestProfile)
            .WithMany()
            .HasForeignKey(b => b.GuestProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cấu hình Role - User
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccountActivation>()
            .HasOne(a => a.User)
            .WithMany(u => u.AccountActivations)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Định dạng tiền tệ mặc định
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18, 2)");
        }

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
