namespace GradingSystem.Services.Submissions.Api.Data;

public class SubmissionEntry
{
    public int Id { get; set; }
    public int SubmissionBatchId { get; set; }
    public SubmissionBatch SubmissionBatch { get; set; } = null!;

    public string StudentCode { get; set; } = null!;
    public string? Metadata { get; set; }
    public string? TextHash { get; set; }
    public decimal? TemporaryScore { get; set; }
    public DateTime ExtractedAt { get; set; }

    public ICollection<SubmissionViolation> Violations { get; set; } = new List<SubmissionViolation>();
    public ICollection<SubmissionAsset> Assets { get; set; } = new List<SubmissionAsset>();
}

