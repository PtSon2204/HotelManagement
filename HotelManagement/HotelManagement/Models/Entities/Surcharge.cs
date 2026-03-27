using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.Entities
{
    public class Surcharge
    {
        [Key]
        public int SurchargeId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }

        [Required]
        [StringLength(255)]
        public string Reason { get; set; } // Ví dụ: "Làm hỏng điều khiển TV", "Check-out muộn 2h"

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
