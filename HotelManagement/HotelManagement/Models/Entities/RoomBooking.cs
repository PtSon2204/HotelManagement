using System;
using System.Collections.Generic;

namespace HotelManagement.Models.Entities;

public partial class RoomBooking
{
    public int RoomBookingId { get; set; }

    public int? BookingId { get; set; }

    public int? RoomId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Room? Room { get; set; }
}
