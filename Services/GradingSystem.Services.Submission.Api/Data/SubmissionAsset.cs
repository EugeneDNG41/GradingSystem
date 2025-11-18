namespace GradingSystem.Services.Submissions.Api.Data;

public class SubmissionAsset
{
    public int Id { get; set; }
    public int SubmissionEntryId { get; set; }
    public SubmissionEntry SubmissionEntry { get; set; } = null!;

    public string FileName { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; }

    public ICollection<SubmissionViolation> LinkedViolations { get; set; } = new List<SubmissionViolation>();
}

