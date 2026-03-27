using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.Entities
{
    public class Surcharge
    {
        [Key]
        public int SurchargeId { get; set; }

        [Required]
        public int BookingId { get; set; } // Sửa RentalId thành BookingId
        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; } // Trỏ về Booking

        public int? EquipmentId { get; set; }
        [ForeignKey("EquipmentId")]
        public virtual Equipment? Equipment { get; set; }

        [Required]
        [StringLength(255)]
        public string Reason { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
