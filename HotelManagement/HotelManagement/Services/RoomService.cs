namespace HotelManagement.Services
{
    public class RoomService
    {
        private readonly RoomRepository _roomRepository;

        public RoomService(RoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<int> CountRooms() => await _roomRepository.CountRoom();

        public async Task<PagedResult<RoomViewModel>> GetAllRoomsAsync(string? search, int page, int pageSize)
        {
            var result = await _roomRepository.GetAllRooms(search, page, pageSize);

            return new PagedResult<RoomViewModel>
            {
                Items = result.Items.Select(x => new RoomViewModel
                {
                    RoomId = x.RoomId,
                    RoomTypeId = x.RoomTypeId,
                    RoomNumber = x.RoomNumber,
                    Price = x.Price,
                    Status = x.Status,
                    RoomTypeName = x.RoomType.Name,
                    Capacity = x.RoomType.Capacity,

                    ImageUrls = x.Images.Select(i => i.Url).ToList()
                }).ToList(),

                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }

        public async Task<RoomViewModel> GetRoomById(int id)
        {
            var room = await _roomRepository.GetRoomByIdAsync(id);

            return new RoomViewModel
            {
                RoomId = room.RoomId,
                Price = room.Price,
                RoomNumber = room.RoomNumber,
                Status = room.Status,
                RoomTypeId = room.RoomTypeId,
                RoomTypeName = room.RoomType?.Name,
                Capacity = room.RoomType?.Capacity,
                Description = room.RoomType?.Description,

                ImageUrls = room.Images.Select(i => i.Url).ToList()
            };
        }

        //hàm này sửa
        public async Task<List<RoomViewModel>> GetAllAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();

            return rooms.Select(r => new RoomViewModel
            {
                RoomId = r.RoomId,
                RoomTypeId = r.RoomTypeId,
                RoomNumber = r.RoomNumber,
                Price = r.Price,
                Status = r.Status,
                RoomTypeName = r.RoomType?.Name,
                Capacity = r.RoomType?.Capacity,
                Description = r.RoomType?.Description,

                ImageUrls = r.Images.Select(i => i.Url).ToList(), // <-- Thêm dấu phẩy ở đây

                Images = r.Images.Select(i => new RoomImageItem
                {
                    ImageId = i.ImageId,
                    Url = i.Url
                }).ToList()
            }).ToList();
        }

        public async Task<RoomViewModel?> GetByIdAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null) return null;

            var roomTypes = await _roomRepository.GetRoomTypesAsync();

            return new RoomViewModel
            {
                RoomId = room.RoomId,
                RoomTypeId = room.RoomTypeId,
                RoomNumber = room.RoomNumber,
                Price = room.Price,
                Status = room.Status,
                RoomTypeName = room.RoomType?.Name,
                Capacity = room.RoomType?.Capacity,
                Description = room.RoomType?.Description,

                ImageUrls = room.Images.Select(i => i.Url).ToList(),

                Images = room.Images.Select(i => new RoomImageItem
                {
                    ImageId = i.ImageId,
                    Url = i.Url
                }).ToList(),
                RoomTypes = roomTypes.Select(rt => new RoomTypeItem
                {
                    RoomTypeId = rt.RoomTypeId,
                    Name = rt.Name,
                }).ToList()
            };
        }

        //hàm này sửa
        public async Task<RoomViewModel> CreateAsync(RoomViewModel model)
        {
            var room = new Room
            {
                RoomTypeId = model.RoomTypeId,
                RoomNumber = model.RoomNumber,
                Price = model.Price,
                Status = model.Status ?? "Available",

                // Lấy danh sách ảnh từ ImageUrls (nếu có)
                Images = model.ImageUrls?.Select(url => new Image
                {
                    Url = url
                }).ToList() ?? new List<Image>()
            };

            var created = await _roomRepository.CreateAsync(room);

            return new RoomViewModel
            {
                RoomId = created.RoomId,
                RoomTypeId = created.RoomTypeId,
                RoomNumber = created.RoomNumber,
                Price = created.Price,
                Status = created.Status,

                ImageUrls = created.Images?.Select(i => i.Url).ToList() ?? new List<string>(),
                Images = created.Images?.Select(i => new RoomImageItem
                {
                    ImageId = i.ImageId,
                    Url = i.Url
                }).ToList() ?? new List<RoomImageItem>()
            };
        }

        public async Task AddImagesAsync(int roomId, IEnumerable<string> urls)
        {
            await _roomRepository.AddImagesAsync(roomId, urls);
        }

        public async Task<List<Image>> GetImagesByRoomIdAsync(int roomId)
        {
            return await _roomRepository.GetImagesByRoomIdAsync(roomId);
        }

        public async Task DeleteImagesAsync(int roomId, IEnumerable<int> imageIds)
        {
            await _roomRepository.DeleteImagesAsync(roomId, imageIds);
        }

        public async Task UpdateAsync(RoomViewModel model)
        {
            var room = await _roomRepository.GetByIdAsync(model.RoomId);
            if (room == null) return;

            room.RoomTypeId = model.RoomTypeId;
            room.RoomNumber = model.RoomNumber;
            room.Price = model.Price;
            room.Status = model.Status;

            // 1. Xử lý xóa ảnh (Dùng list DeleteImageIds của người thứ 2)
            if (model.DeleteImageIds != null && model.DeleteImageIds.Any())
            {
                // Gọi hàm xóa ảnh trong DB (bạn đã có sẵn hàm này trong service)
                await DeleteImagesAsync(room.RoomId, model.DeleteImageIds);
            }

            // 2. Xử lý thêm ảnh mới (Dùng list ImageUrls của người thứ 1)
            // Chỉ thêm những ảnh mới được truyền vào, KHÔNG Clear() ảnh cũ nữa
            if (model.ImageUrls != null && model.ImageUrls.Any())
            {
                var newImages = model.ImageUrls.Select(url => new Image
                {
                    Url = url,
                    RoomId = room.RoomId
                }).ToList();

                foreach (var img in newImages)
                {
                    room.Images.Add(img);
                }
            }

            await _roomRepository.UpdateAsync(room);
        }

        public async Task DeleteAsync(int id)
        {
            await _roomRepository.DeleteAsync(id);
        }

        public async Task<List<RoomTypeItem>> GetRoomTypesAsync()
        {
            var roomTypes = await _roomRepository.GetRoomTypesAsync();

            return roomTypes.Select(rt => new RoomTypeItem
            {
                RoomTypeId = rt.RoomTypeId,
                Name = rt.Name,
            }).ToList();
        }
    }
}
