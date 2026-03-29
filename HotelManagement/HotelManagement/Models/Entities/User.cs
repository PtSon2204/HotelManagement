using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.Entities;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    public int RoleId { get; set; }

    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; }

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; }

    [Required]
    [StringLength(100)]
    public string? FullName { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(20)]
    public string? IDCard { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? Nationality { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Image { get; set; }

    public virtual ICollection<GuestProfile> GuestProfiles { get; set; } = new List<GuestProfile>();
    public virtual ICollection<AccountActivation> AccountActivations { get; set; } = new List<AccountActivation>();
}
