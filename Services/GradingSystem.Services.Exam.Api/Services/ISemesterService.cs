using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Shared;

namespace GradingSystem.Services.Exams.Api.Services
{

    public interface ISemesterService
    {
        Task<Result<int>> CreateSemesterAsync(CreateSemesterRequest request);
        Task<Result<SemesterResponse>> GetSemesterByIdAsync(int id);
        Task<Result<List<SemesterResponse>>> GetAllSemestersAsync();
        Task<Result<SemesterResponse>> UpdateSemesterAsync(int id, UpdateSemesterRequest request);
        Task<Result<bool>> DeleteSemesterAsync(int id);
        Task<Result<List<SemesterResponse>>> GetSemestersAsync();
        Task<Result<SemesterResponse>> GetSemesterByIdAsync(int id);
    }

}
