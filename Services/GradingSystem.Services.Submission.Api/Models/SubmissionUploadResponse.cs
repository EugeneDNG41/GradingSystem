using GradingSystem.Services.Submissions.Api.Data;

namespace GradingSystem.Services.Submissions.Api.Models;

public sealed class SubmissionUploadResponse
{
    public int Id { get; init; }
    public SubmissionBatchStatus Status { get; init; }
    public DateTime UploadedAt { get; init; }
    public int ExamId { get; init; }
    public int UploadedByUserId { get; init; }
    public string? Notes { get; init; }
    public SubmissionUploadFileResponse File { get; init; } = null!;
}

public sealed class SubmissionUploadFileResponse
{
    public int Id { get; init; }
    public string BlobName { get; init; } = string.Empty;
    public DateTime UploadDate { get; init; }
}


