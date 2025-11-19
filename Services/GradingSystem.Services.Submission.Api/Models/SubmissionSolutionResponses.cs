namespace GradingSystem.Services.Submissions.Api.Models;

public sealed class SubmissionSolutionFilesResponse
{
    public int SubmissionEntryId { get; set; }
    public IReadOnlyList<SolutionFileDescriptor> SolutionFiles { get; set; } = Array.Empty<SolutionFileDescriptor>();
    public IReadOnlyList<SubmissionAttachmentResponse> Attachments { get; set; } = Array.Empty<SubmissionAttachmentResponse>();
}

public sealed class SolutionFileDescriptor
{
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public bool IsDirectory { get; set; }
}

public sealed class SubmissionAttachmentResponse
{
    public int AssetId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public long Size { get; set; }
}

public sealed record FileDownloadResult(Stream Content, string FileName, string ContentType);

