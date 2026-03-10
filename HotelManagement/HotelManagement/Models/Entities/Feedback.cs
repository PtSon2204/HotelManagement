using System;
using System.Collections.Generic;

namespace HotelManagement.Models.Entities;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int? CustomerId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? FeedbackDate { get; set; }

    public virtual Customer? Customer { get; set; }
}
