using GradingSystem.Shared;
using GradingSystem.Services.Exams.Api.Models;

namespace GradingSystem.Services.Exams.Api.Services
{
    public interface IRubricService
    {
        Task<Result<int>> CreateRubricAsync(CreateRubricRequest request);
    }
}
