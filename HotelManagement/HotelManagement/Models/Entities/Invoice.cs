using System;
using System.Collections.Generic;

namespace HotelManagement.Models.Entities;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int? RentalId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Status { get; set; }

    public virtual Rental? Rental { get; set; }
}
