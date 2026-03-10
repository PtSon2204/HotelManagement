using System;
using System.Collections.Generic;

namespace HotelManagement.Models.Entities;

public partial class Booking
{
    public int BookingId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }

    public decimal? Deposit { get; set; }

    public int NumOfPeople { get; set; }

    public string? Status { get; set; }

    public int? StaffId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();

    public virtual ICollection<RoomBooking> RoomBookings { get; set; } = new List<RoomBooking>();

    public virtual Staff? Staff { get; set; }
}
