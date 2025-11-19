using GradingSystem.Shared;
using GradingSystem.Services.Exams.Api.Models;

namespace GradingSystem.Services.Exams.Api.Services
{
    public interface IRubricService
    {
        Task<Result<int>> CreateRubricAsync(CreateRubricRequest request);
        Task<Result> UpdateRubricAsync(int id, UpdateRubricRequest request);
        Task<Result> DeleteRubricAsync(int id);
        Task<Result<List<RubricResponse>>> GetRubricsAsync(int? examId);
    }
}
