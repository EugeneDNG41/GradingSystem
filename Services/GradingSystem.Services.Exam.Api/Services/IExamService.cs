using GradingSystem.Shared;
using GradingSystem.Services.Exams.Api.Models;

namespace GradingSystem.Services.Exams.Api.Services
{
    public interface IExamService
    {
        Task<Result<int>> CreateExamAsync(CreateExamRequest request);
        Task<Result<ExamDetailResponse>> GetExamByIdAsync(int id);
        Task<Result<List<ExamListItem>>> GetExamListAsync(int? semesterId);
    }
}
