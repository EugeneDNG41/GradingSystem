using GradingSystem.Shared;

namespace GradingSystem.Services.Submissions.Api.Services;

public interface ISubmissionIngestionService
{
    Task<Result<IngestionResult>> IngestAsync(Stream archiveStream, CancellationToken cancellationToken = default);
}

public sealed class IngestionResult
{
    public string ExtractionPath { get; init; } = string.Empty;
    public IReadOnlyList<ExtractedFile> ExtractedFiles { get; init; } = Array.Empty<ExtractedFile>();
}


