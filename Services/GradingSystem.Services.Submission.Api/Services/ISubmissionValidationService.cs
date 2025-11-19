using GradingSystem.Services.Submissions.Api.Data;

namespace GradingSystem.Services.Submissions.Api.Services;

public interface ISubmissionValidationService
{
    Task<ValidationOutcome> ValidateAsync(
        SubmissionBatch batch,
        IReadOnlyCollection<ExtractedFile> files,
        CancellationToken cancellationToken = default);
}

public sealed class ValidationOutcome
{
    public ValidationResult Validation { get; init; } = new();
    public IReadOnlyList<SubmissionEntry> Entries { get; init; } = Array.Empty<SubmissionEntry>();
    public IReadOnlyList<SubmissionViolation> Violations { get; init; } = Array.Empty<SubmissionViolation>();
}


