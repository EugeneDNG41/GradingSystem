using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Shared;

namespace GradingSystem.Services.Exams.Api.Services
{

    public interface ISemesterService
    {
        Task<Result<int>> CreateSemesterAsync(CreateSemesterRequest request);
    }

}
