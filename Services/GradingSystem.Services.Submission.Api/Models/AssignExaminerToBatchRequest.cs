namespace GradingSystem.Services.Submissions.Api.Models;

public sealed record AssignExaminerToBatchRequest(
    int SubmissionBatchId,
    int ExaminerId
);

