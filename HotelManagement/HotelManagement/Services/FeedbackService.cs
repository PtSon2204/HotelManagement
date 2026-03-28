using HotelManagement.Models.Common;
using HotelManagement.Models.ViewModels;
using HotelManagement.Repositories;

namespace HotelManagement.Services
{
    public class FeedbackService
    {
        private readonly FeedbackRepository _repo;

        public FeedbackService(FeedbackRepository repo)
        {
            _repo = repo;
        }

        public int CountFeedback() => _repo.CountFeedback();

        public async Task<PagedResult<FeedbackViewModel>> GetAllFeedback(string? search, int page, int pageSize)
            => await _repo.GetAllFeedback(search, page, pageSize);

        public async Task<FeedbackViewModel?> GetFeedbackById(int id)
            => await _repo.GetFeedbackById(id);
    }
}
