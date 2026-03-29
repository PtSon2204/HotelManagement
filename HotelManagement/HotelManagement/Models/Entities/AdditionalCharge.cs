namespace HotelManagement.Models.Entities;

public class AdditionalCharge
{
    public int AdditionalChargeId { get; set; }

    public int BookingId { get; set; }

    public string Description { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }
}
