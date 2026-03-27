using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.Entities;

public class User
{
    [Key]
    public int UserId { get; set; }

    // Khóa ngoại liên kết với bảng Role
    [Required]
    public int RoleId { get; set; }
    [ForeignKey("RoleId")]
    public Role Role { get; set; }
    public string Username { get; set; }    
    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; } // Dùng dấu ? để cho phép NULL

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
}
