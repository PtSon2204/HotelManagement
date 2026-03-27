using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.Entities;

public class Feedback
{
    [Key]
    public int FeedbackId { get; set; }

    // Khóa ngoại nối thẳng vào Room
    [Required]
    public int RoomId { get; set; }
    [ForeignKey("RoomId")]
    public Room Room { get; set; }

    [Required]
    public int UserId { get; set; } 
    [ForeignKey("UserId")]
    public User User { get; set; }

    public int Rating { get; set; } // 1 - 5 sao
    public string Comment { get; set; }
    public DateTime FeedbackDate { get; set; }
}
