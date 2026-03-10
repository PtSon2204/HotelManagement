using System;
using System.Collections.Generic;

namespace HotelManagement.Models.Entities;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Gender { get; set; }

    public string? Idcard { get; set; }

    public string? Address { get; set; }

    public string? Nationality { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
