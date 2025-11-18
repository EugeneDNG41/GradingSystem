namespace GradingSystem.Services.Submissions.Api.Data;

public class SubmissionViolation
{
    public int Id { get; set; }
    public int SubmissionEntryId { get; set; }
    public SubmissionEntry SubmissionEntry { get; set; } = null!;

    public ViolationType Type { get; set; }
    public int Severity { get; set; }
    public string Description { get; set; } = null!;
    public string? AssetLink { get; set; }

    public int? SubmissionAssetId { get; set; }
    public SubmissionAsset? SubmissionAsset { get; set; }
}

public enum ViolationType
{
    Plagiarism = 0,
    MissingContent = 1,
    InvalidFormat = 2,
    UnauthorizedResources = 3,
    Other = 4
}

