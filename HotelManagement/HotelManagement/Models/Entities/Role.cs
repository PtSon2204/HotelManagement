using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Models.Entities
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Tên quyền không được để trống")]
        [StringLength(50)]
        public string RoleName { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
