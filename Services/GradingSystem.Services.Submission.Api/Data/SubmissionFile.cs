namespace GradingSystem.Services.Submissions.Api.Data;

public class SubmissionFile
{
    public int Id { get; set; }
    public string BlobName { get; set; } = null!;
    public DateTime UploadDate { get; set; }
}
