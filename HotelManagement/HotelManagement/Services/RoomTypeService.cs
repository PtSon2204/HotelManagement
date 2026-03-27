using HotelManagement.Context;
using HotelManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    // RoomType không còn là bảng riêng - service này giờ chỉ là stub
    public class RoomTypeService
    {
        public RoomTypeService(ApplicationDbContext context) { }

        public Task<List<RoomTypeViewModel>> GetAllAsync() =>
            Task.FromResult(new List<RoomTypeViewModel>());

        public Task<RoomTypeViewModel?> GetByIdAsync(int id) =>
            Task.FromResult<RoomTypeViewModel?>(null);

        public Task<RoomTypeViewModel> CreateAsync(RoomTypeViewModel model) =>
            Task.FromResult(model);

        public Task UpdateAsync(RoomTypeViewModel model) => Task.CompletedTask;

        public Task DeleteAsync(int id) => Task.CompletedTask;
    }

    public class RoomTypeViewModel
    {
        public int RoomTypeId { get; set; }
        public string Name { get; set; } = null!;
        public string? Image { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
