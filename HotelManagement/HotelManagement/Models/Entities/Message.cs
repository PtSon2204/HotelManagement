using System;

namespace HotelManagement.Models.Entities;

public partial class Message
{
    public int MessageId { get; set; }

    public string SenderName { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime SentAt { get; set; }
}
