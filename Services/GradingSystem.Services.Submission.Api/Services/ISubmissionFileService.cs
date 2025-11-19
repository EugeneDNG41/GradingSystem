using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Shared;

namespace GradingSystem.Services.Submissions.Api.Services;

public interface ISubmissionFileService
{
    Task<Result<bool>> UploadFile(IFormFile file);
    Task<Result<UnpackResult>> UnpackAsync(
        SubmissionBatch submissionBatch,
        CancellationToken cancellationToken = default);
}
public class UnpackResult
{
    public bool IsSuccess { get; set; }
    public List<ExtractedFile> ExtractedFiles { get; set; } = new();
    public ValidationResult Validation { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class ExtractedFile
{
    public string OriginalPath { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string RenamedFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileExtension { get; set; } = string.Empty;
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int TotalFiles { get; set; }
    public long TotalSize { get; set; }
    public bool HasDatFile { get; set; }
}
