using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Shared;

namespace GradingSystem.Services.Submissions.Api.Services;

public interface ISubmissionUploadService
{
    Task<Result<SubmissionUploadResponse>> UploadAsync(
        UploadSubmissionRequest request,
        int userId,
        CancellationToken cancellationToken = default);
}


