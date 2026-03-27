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
        {
            var result = await _repo.GetAllFeedbacks(search, page, pageSize);

            return new PagedResult<FeedbackViewModel>
            {
                Items = result.Items.Select(x => new FeedbackViewModel
                {
                    FeedbackId = x.FeedbackId,
                    Comment = x.Comment,
                    FullName = x.User?.FullName,
                    UserId = x.UserId,
                    FeedbackDate = x.FeedbackDate,
                    Rating = x.Rating,
                }).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }

        public async Task<FeedbackViewModel> GetFeedbackById(int id)
        {
            var x = await _repo.GetFeedbackById(id);
            if (x == null) throw new Exception("Feedback not found");

            return new FeedbackViewModel
            {
                FeedbackId = x.FeedbackId,
                Comment = x.Comment,
                FullName = x.User?.FullName,
                UserId = x.UserId,
                FeedbackDate = x.FeedbackDate,
                Rating = x.Rating
            };
        }
    }
}
