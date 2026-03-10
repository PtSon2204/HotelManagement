using System;
using System.Collections.Generic;

namespace HotelManagement.Models.Entities;

public partial class Room
{
    public int RoomId { get; set; }

    public int? RoomTypeId { get; set; }

    public string RoomNumber { get; set; } = null!;

    public string? Image { get; set; }

    public decimal? Price { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<RoomBooking> RoomBookings { get; set; } = new List<RoomBooking>();

    public virtual RoomType? RoomType { get; set; }
}
