using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Models.Entities;

public class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required]
    [StringLength(20)]
    public string RoomNumber { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; }

    [Required]
    [StringLength(100)]
    public string RoomTypeName { get; set; }

    public int Capacity { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Image> Images { get; set; } = new List<Image>();
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
