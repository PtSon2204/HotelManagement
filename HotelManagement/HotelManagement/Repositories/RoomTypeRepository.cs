using HotelManagement.Context;
using HotelManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repositories
{
    // RoomType không còn là entity riêng - class này không còn dùng DbSet RoomTypes
    // Giữ lại class để tránh DI errors; các method stub trả về empty
    public class RoomTypeRepository
    {
        public RoomTypeRepository(ApplicationDbContext context) { }
    }
}
