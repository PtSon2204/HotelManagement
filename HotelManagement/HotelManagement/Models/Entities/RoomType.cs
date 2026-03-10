using System;
using System.Collections.Generic;

namespace HotelManagement.Models.Entities;

public partial class RoomType
{
    public int RoomTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Image { get; set; }

    public decimal Price { get; set; }

    public int Capacity { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
