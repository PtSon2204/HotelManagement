using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Models.Entities;

public class AccountActivation
{
    [Key]
    public int AccountActivationId { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(6)]
    public string OtpCode { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? VerifiedAt { get; set; }
}
