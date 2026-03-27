using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Models.Entities
{
    public class Equipment
    {
        [Key]
        public int EquipmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string? EquipmentName { get; set; }

        // Giá tham khảo để đền bù nếu làm hỏng toàn bộ
        public decimal? CompensationPrice { get; set; }

        public ICollection<RoomEquipment> RoomEquipments { get; set; }
    }

    public class RoomEquipment
    {
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        public int? EquipmentId { get; set; }
        [ForeignKey("EquipmentId")]
        public Equipment? Equipment { get; set; }

        public int? Quantity { get; set; } 
    }
}
