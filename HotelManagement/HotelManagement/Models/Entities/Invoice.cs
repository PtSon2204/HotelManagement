using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.Entities;

public partial class Invoice
{
    [Key]
    public int InvoiceId { get; set; }

    // --- Thay RentalId bằng BookingId ---
    public int? BookingId { get; set; }

    [ForeignKey("BookingId")]
    public virtual Booking? Booking { get; set; }

    // --- Các thông tin hóa đơn ---
    public decimal TotalAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }
}
