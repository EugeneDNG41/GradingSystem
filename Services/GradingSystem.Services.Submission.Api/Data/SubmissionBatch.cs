namespace GradingSystem.Services.Submissions.Api.Data;

public class SubmissionBatch
{
    public int Id { get; set; }
    public int SubmissionFileId { get; set; }
    public SubmissionFile SubmissionFile { get; set; } = null!;

    public int UploadedByUserId { get; set; }
    public SubmissionBatchStatus Status { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Notes { get; set; }

    public ICollection<SubmissionEntry> Entries { get; set; } = new List<SubmissionEntry>();
}

public enum SubmissionBatchStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}

