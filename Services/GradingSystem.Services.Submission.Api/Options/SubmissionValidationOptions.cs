namespace GradingSystem.Services.Submissions.Api.Options;

public sealed class SubmissionValidationOptions
{
    public string StudentFolderPattern { get; set; } = "^[A-Za-z0-9]{5,15}$";
    public int MinReportFiles { get; set; } = 1;
    public int MaxReportFiles { get; set; } = 1;
    public int MinSourceFiles { get; set; } = 1;
    public int MaxSourceFiles { get; set; } = 500;
    public bool FlagExtraFilesAsViolations { get; set; } = true;
    public string[] AllowedStudentExtraExtensions { get; set; } = [".dat"];
}

