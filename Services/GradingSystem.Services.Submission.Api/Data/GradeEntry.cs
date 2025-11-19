using GradingSystem.Services.Submissions.Api.Data;

public class GradeEntry
{
    public int Id { get; set; }

    public int SubmissionEntryId { get; set; }
    public SubmissionEntry SubmissionEntry { get; set; } = null!;

    public int ExaminerId { get; set; }
    public decimal Score { get; set; }
    public string? Notes { get; set; }

    public DateTime GradedAt { get; set; }
}
