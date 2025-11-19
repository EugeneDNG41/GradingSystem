namespace GradingSystem.Shared.Contracts;

public sealed record SubmissionViolationsDetected(
    int SubmissionBatchId,
    int ExamId,
    int UploadedByUserId,
    DateTime ProcessedAt,
    IReadOnlyCollection<StudentViolationEnvelope> Students);

public sealed record StudentViolationEnvelope(
    int SubmissionEntryId,
    string StudentCode,
    IReadOnlyCollection<SubmissionViolationEnvelope> Violations,
    SubmissionEntryMetadataEnvelope Metadata);

public sealed record SubmissionViolationEnvelope(
    int SubmissionViolationId,
    string Type,
    int Severity,
    string Description);

public sealed record SubmissionEntryMetadataEnvelope(
    string StudentCode,
    int TotalFiles,
    long TotalSize,
    IReadOnlyCollection<SubmissionEntryFileMetadataEnvelope> Files,
    IReadOnlyCollection<string> MissingItems,
    IReadOnlyCollection<string> ExtraItems);

public sealed record SubmissionEntryFileMetadataEnvelope(
    string RelativePath,
    long Size,
    string Extension,
    string Category);

