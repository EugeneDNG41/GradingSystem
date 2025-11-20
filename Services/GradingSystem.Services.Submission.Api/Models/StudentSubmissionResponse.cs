namespace GradingSystem.Services.Submissions.Api.Models;

public sealed record StudentSubmissionResponse(
    int Id,
    int SubmissionBatchId,
    string StudentCode,
    string? Metadata,
    decimal? TemporaryScore,
    DateTime ExtractedAt,
    int ExamId,
    DateTime UploadedAt,
    List<StudentAssetResponse> Assets,
    List<StudentViolationResponse> Violations,
    List<GradeEntryResponse> GradeEntries
);

public sealed record StudentAssetResponse(
    int Id,
    string FileName,
    string MimeType,
    string BlobName,
    DateTime CreatedAt
);

public sealed record StudentViolationResponse(
    int Id,
    string Type,
    int Severity,
    string Description,
    string? AssetLink,
    int? SubmissionAssetId
);

