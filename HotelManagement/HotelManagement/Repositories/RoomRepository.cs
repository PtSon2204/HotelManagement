using HotelManagement.Context;
using HotelManagement.Models.Common;
using HotelManagement.Models.Entities;
using HotelManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repositories
{
    public class RoomRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountRooms()
        {
            return await _context.Rooms.CountAsync(r => r.IsActive);
        }

        public async Task<PagedResult<RoomViewModel>> GetAllRoomsAsync(string? search, int page, int pageSize)
        {
            var query = _context.Rooms
                .Include(r => r.Images)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(r =>
                    r.RoomNumber.ToLower().Contains(s) ||
                    r.RoomTypeName.ToLower().Contains(s) ||
                    r.Status.ToLower().Contains(s));
            }

            int total = await query.CountAsync();

            var items = await query
                .OrderBy(r => r.Status == "Available" || r.Status == "Tr?ng" ? 1 :
                              r.Status == "Occupied" ? 2 :
                              r.Status == "Maintenance" ? 3 : 4)
                .ThenBy(r => r.RoomNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RoomViewModel
                {
                    RoomId = r.RoomId,
                    RoomNumber = r.RoomNumber,
                    Price = r.Price,
                    Status = r.Status,
                    RoomTypeName = r.RoomTypeName,
                    Capacity = r.Capacity,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    Images = r.Images
                        .Select(i => new RoomImageItem { ImageId = i.ImageId, Url = i.Url })
                        .ToList()
                })
                .ToListAsync();

            return new PagedResult<RoomViewModel>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Room?> GetRoomById(int id)
        {
            return await _context.Rooms
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.RoomId == id);
        }

        public async Task<PagedResult<Room>> GetAllRooms(string? search, DateTime? checkIn, DateTime? checkOut, int page, int pageSize)
        {
            var safePage = Math.Max(page, 1);
            var safePageSize = Math.Max(pageSize, 1);
            var query = BuildRoomQuery();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(r =>
                    r.RoomNumber.Contains(keyword) ||
                    r.RoomTypeName.Contains(keyword) ||
                    r.Status.Contains(keyword));
            }

            if (checkIn.HasValue && checkOut.HasValue && checkOut > checkIn)
            {
                var requestedCheckIn = checkIn.Value;
                var requestedCheckOut = checkOut.Value;

                query = query.Where(r =>
                    r.IsActive &&
                    (r.Status == "Available" || r.Status == "Tr?ng") &&
                    !_context.Bookings.Any(b =>
                        b.RoomId == r.RoomId &&
                        b.Status != BookingStatus.Cancelled.ToString() &&
                        requestedCheckIn < b.ExpectedCheckOut &&
                        requestedCheckOut > b.ExpectedCheckIn));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(r => r.Status == "Available" || r.Status == "Tr?ng" ? 1 :
                              r.Status == "Occupied" ? 2 :
                              r.Status == "Maintenance" ? 3 : 4)
                .ThenBy(r => r.RoomNumber)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync();

            return new PagedResult<Room>
            {
                Items = items,
                TotalCount = totalCount,
                Page = safePage,
                PageSize = safePageSize
            };
        }

        public async Task<Room> GetRoomByIdAsync(int id)
        {
            var room = await GetByIdAsync(id);
            if (room == null)
            {
                throw new InvalidOperationException($"Room with id {id} was not found.");
            }

            return room;
        }

        public Task<List<Room>> GetAllAsync()
        {
            return BuildRoomQuery()
                .OrderBy(r => r.RoomId)
                .ToListAsync();
        }

        public Task<Room?> GetByIdAsync(int id)
        {
            return BuildRoomQuery()
                .FirstOrDefaultAsync(r => r.RoomId == id);
        }

        public async Task<List<RoomTypeItem>> GetRoomTypesAsync()
        {
            var names = await _context.Rooms
                .Where(r => !string.IsNullOrWhiteSpace(r.RoomTypeName))
                .Select(r => r.RoomTypeName)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();

            return names
                .Select((name, index) => new RoomTypeItem
                {
                    RoomTypeId = index + 1,
                    Name = name
                })
                .ToList();
        }

        public async Task<Room> CreateAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task AddImagesAsync(int roomId, IEnumerable<string> urls)
        {
            var images = urls
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => new Image
                {
                    RoomId = roomId,
                    Url = url.Trim()
                })
                .ToList();

            if (images.Count == 0)
            {
                return;
            }

            _context.Images.AddRange(images);
            await _context.SaveChangesAsync();
        }

        public Task<List<Image>> GetImagesByRoomIdAsync(int roomId)
        {
            return _context.Images
                .Where(i => i.RoomId == roomId)
                .OrderBy(i => i.ImageId)
                .ToListAsync();
        }

        public async Task DeleteImagesAsync(int roomId, IEnumerable<int> imageIds)
        {
            var ids = imageIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return;
            }

            var images = await _context.Images
                .Where(i => i.RoomId == roomId && ids.Contains(i.ImageId))
                .ToListAsync();

            if (images.Count == 0)
            {
                return;
            }

            _context.Images.RemoveRange(images);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Room room)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == id);
            if (room == null)
            {
                return;
            }

            var images = await _context.Images.Where(i => i.RoomId == id).ToListAsync();
            if (images.Count > 0)
            {
                _context.Images.RemoveRange(images);
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Room> BuildRoomQuery()
        {
            return _context.Rooms
                .Include(r => r.Images)
                .Include(r => r.Feedbacks)
                .ThenInclude(f => f.User);
        }
    }
}
