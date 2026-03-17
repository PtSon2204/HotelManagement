using HotelManagement.Context;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Services
{
    public class RoomTypeService
    {
        private readonly ApplicationDbContext _context;

        public RoomTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoomTypeViewModel>> GetAllAsync()
        {
            var roomTypes = await _context.RoomTypes.ToListAsync();
            return roomTypes.Select(rt => new RoomTypeViewModel
            {
                RoomTypeId = rt.RoomTypeId,
                Name = rt.Name,
                Image = rt.Image,
                Price = rt.Price,
                Capacity = rt.Capacity,
                Description = rt.Description,
                IsActive = rt.IsActive
            }).ToList();
        }

        public async Task<RoomTypeViewModel?> GetByIdAsync(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null) return null;

            return new RoomTypeViewModel
            {
                RoomTypeId = roomType.RoomTypeId,
                Name = roomType.Name,
                Image = roomType.Image,
                Price = roomType.Price,
                Capacity = roomType.Capacity,
                Description = roomType.Description,
                IsActive = roomType.IsActive
            };
        }

        // model.Image already holds the path returned by the UploadImage endpoint (or null if no image)
        public async Task<RoomTypeViewModel> CreateAsync(RoomTypeViewModel model)
        {
            var roomType = new RoomType
            {
                Name = model.Name,
                Image = model.Image,
                Price = model.Price,
                Capacity = model.Capacity,
                Description = model.Description,
                IsActive = model.IsActive ?? true
            };

            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();

            return new RoomTypeViewModel
            {
                RoomTypeId = roomType.RoomTypeId,
                Name = roomType.Name,
                Image = roomType.Image,
                Price = roomType.Price,
                Capacity = roomType.Capacity,
                Description = roomType.Description,
                IsActive = roomType.IsActive
            };
        }

        // If model.Image is null/empty, preserve the existing image path already in the DB
        public async Task UpdateAsync(RoomTypeViewModel model)
        {
            var roomType = await _context.RoomTypes.FindAsync(model.RoomTypeId);
            if (roomType == null) return;

            roomType.Name = model.Name;
            if (!string.IsNullOrEmpty(model.Image))
                roomType.Image = model.Image;
            roomType.Price = model.Price;
            roomType.Capacity = model.Capacity;
            roomType.Description = model.Description;
            roomType.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType != null)
            {
                _context.RoomTypes.Remove(roomType);
                await _context.SaveChangesAsync();
            }
        }
    }
}
