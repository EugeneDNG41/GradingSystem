using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Shared;

namespace GradingSystem.Services.Submissions.Api.Services;

public interface ISubmissionBatchService
{
    Task<Result<int>> AssignExaminerAsync(AssignExaminerToBatchRequest request);
    Task<Result<List<SubmissionEntryResponse>>> GetEntriesByBatchIdAsync(int submissionBatchId);
    Task<Result<List<SubmissionBatchListItem>>> GetSubmissionBatchesAsync();
    Task<Result<SubmissionBatchListItem>> GetSubmissionBatchByIdAsync(int submissionBatchId);
    Task<Result<List<SubmissionBatchListItem>>> GetBatchesByExaminerIdAsync(int examinerId);
}

