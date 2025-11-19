using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Shared;

namespace GradingSystem.Services.Submissions.Api.Services;
public interface IGradeEntryService
{
    Task<Result<int>> CreateAsync(CreateGradeEntryRequest request);
    Task<Result<List<GradeEntryResponse>>> GetBySubmissionAsync(int submissionEntryId);
}
