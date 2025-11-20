namespace GradingSystem.Services.Submissions.Api.Models;

public sealed record SubmissionEntryResponse(
    int Id,
    int SubmissionBatchId,
    string StudentCode,
    string? TextHash,
    decimal? TemporaryScore,
    DateTime ExtractedAt
);

