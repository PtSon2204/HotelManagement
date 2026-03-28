using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.Entities;
public class Booking
{
    [Key]
    public int BookingId { get; set; }

    [Required]
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    [Required]
    public int RoomId { get; set; }
    [ForeignKey("RoomId")]
    public virtual Room Room { get; set; }

    public DateTime ExpectedCheckIn { get; set; }
    public DateTime ExpectedCheckOut { get; set; }
    public DateTime? ActualCheckIn { get; set; }
    public DateTime? ActualCheckOut { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Deposit { get; set; }

    public int NumOfPeople { get; set; }

    [StringLength(50)]
    public string? Status { get; set; } = "Chờ xác nhận";

    // --- Thông tin người ở thực tế (liên kết với sổ khách hàng) ---
    public int? GuestProfileId { get; set; }
    [ForeignKey("GuestProfileId")]
    public virtual GuestProfile? GuestProfile { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;


    public virtual ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    public virtual Invoice? Invoice { get; set; }
}
