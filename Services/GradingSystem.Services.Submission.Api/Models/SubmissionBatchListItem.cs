namespace GradingSystem.Services.Submissions.Api.Models;

public sealed record SubmissionBatchListItem(
    int Id,
    string? Notes,
    string? Title,
    string? ExaminerName,
    int? ExamId
);

