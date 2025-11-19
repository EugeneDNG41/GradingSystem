namespace GradingSystem.Services.Submissions.Api.Models
{
    public sealed record GradeEntryResponse(
        int Id,
        int SubmissionEntryId,
        int ExaminerId,
        decimal Score,
        string? Notes,
        DateTime GradedAt
    );

}
