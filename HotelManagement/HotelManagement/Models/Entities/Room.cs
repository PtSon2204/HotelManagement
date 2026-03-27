using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.Entities;

public class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required]
    [StringLength(20)]
    public string RoomNumber { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; }

    // --- Các cột được gộp từ bảng RoomTypes cũ ---
    [StringLength(100)]
    public string RoomTypeName { get; set; } // Ví dụ: VIP, Standard...
    public int Capacity { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }

    // --- Navigation Properties ---
    public ICollection<Feedback> Feedbacks { get; set; }
    public ICollection<RoomEquipment> RoomEquipments { get; set; }
    public ICollection<RoomBooking> RoomBookings { get; set; }
    public virtual ICollection<Image> Images { get; set; } = new List<Image>();
}
