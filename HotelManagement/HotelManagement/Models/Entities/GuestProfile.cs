using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Models.Entities
{
    public class GuestProfile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [StringLength(100)]
        public string Label { get; set; } = "Khách"; // VD: "Bản thân", "Bố", "Sếp"

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = null!;

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? IdCard { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        [StringLength(50)]
        public string? Nationality { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }
    }
}
