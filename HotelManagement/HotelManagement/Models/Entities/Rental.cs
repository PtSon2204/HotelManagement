using System;
using System.Collections.Generic;

namespace HotelManagement.Models.Entities;

public partial class Rental
{
    public int RentalId { get; set; }

    public int? BookingId { get; set; }

    public DateTime CheckInActual { get; set; }

    public DateTime? CheckOutActual { get; set; }

    public int? StaffId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Staff? Staff { get; set; }
}
