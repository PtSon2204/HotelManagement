using System;
using HotelManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Context;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext() { }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // --- CÁC DBSET MỚI CHUẨN HÓA ---
    public virtual DbSet<Booking> Bookings { get; set; }
    public virtual DbSet<BookingService> BookingServices { get; set; }
    public virtual DbSet<Equipment> Equipments { get; set; }
    public virtual DbSet<Feedback> Feedbacks { get; set; }
    public virtual DbSet<Image> Images { get; set; }
    public virtual DbSet<Invoice> Invoices { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Room> Rooms { get; set; }
    public virtual DbSet<RoomBooking> RoomBookings { get; set; }
    public virtual DbSet<RoomEquipment> RoomEquipments { get; set; }
    public virtual DbSet<Service> Services { get; set; }
    public virtual DbSet<Surcharge> Surcharges { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. CẤU HÌNH BOOKING (Tránh vòng lặp Cascade khi xóa User)
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(e => e.ExpectedCheckIn).HasColumnType("datetime");
            entity.Property(e => e.ExpectedCheckOut).HasColumnType("datetime");
            entity.Property(e => e.ActualCheckIn).HasColumnType("datetime");
            entity.Property(e => e.ActualCheckOut).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.Deposit).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Chờ xác nhận");

            // Ràng buộc Khách hàng
            entity.HasOne(d => d.Customer)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict) // Không cho xóa User nếu đang có Booking
                .HasConstraintName("FK_Bookings_CustomerUser");

            // Ràng buộc Nhân viên
            entity.HasOne(d => d.Staff)
                .WithMany()
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Bookings_StaffUser");
        });

        // 2. CẤU HÌNH INVOICE VÀ SURCHARGE (Nối vào Booking)
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Chưa thanh toán");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Surcharge>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            // Xóa Equipment thì cập nhật Surcharge EquipmentId thành null thay vì xóa Surcharge
            entity.HasOne(s => s.Equipment)
                .WithMany()
                .HasForeignKey(s => s.EquipmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 3. CẤU HÌNH ROOMEQUIPMENT (Khóa chính kép)
        modelBuilder.Entity<RoomEquipment>(entity =>
        {
            entity.HasKey(re => new { re.RoomId, re.EquipmentId });
        });

        // 4. CẤU HÌNH ROOM
        modelBuilder.Entity<Room>(entity =>
        {
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Trống");
        });

        // 5. CẤU HÌNH EQUIPMENT VÀ SERVICE
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.Property(e => e.CompensationPrice).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        // 6. CẤU HÌNH FEEDBACK
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.Property(e => e.FeedbackDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.HasOne(f => f.User)
                  .WithMany()
                  .HasForeignKey(f => f.UserId)
                  .OnDelete(DeleteBehavior.Restrict); // Giữ lại review ngay cả khi xóa User (nếu muốn)
        });

        // 8. CẤU HÌNH USER
        modelBuilder.Entity<User>(entity =>
        {

            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}