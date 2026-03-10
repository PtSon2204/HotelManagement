using System;
using System.Collections.Generic;

namespace HotelManagement.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? CustomerId { get; set; }

    public int? StaffId { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Staff? Staff { get; set; }
}
