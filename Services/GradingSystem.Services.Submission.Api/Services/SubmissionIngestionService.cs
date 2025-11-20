using GradingSystem.Shared;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System.Text;

namespace GradingSystem.Services.Submissions.Api.Services;

public sealed class SubmissionIngestionService(ILogger<SubmissionIngestionService> logger) : ISubmissionIngestionService
{
    private readonly ILogger<SubmissionIngestionService> _logger = logger;

    public async Task<Result<IngestionResult>> IngestAsync(Stream archiveStream, CancellationToken cancellationToken = default)
    {
        if (archiveStream is null)
        {
            return Result.Failure<IngestionResult>(Error.BadRequest(
                "Submissions.Ingest.EmptyStream",
                "Archive stream is required."));
        }

        var extractionRoot = Path.Combine(Path.GetTempPath(), $"submission_{Guid.NewGuid():N}");
        Directory.CreateDirectory(extractionRoot);

        try
        {
            using var archive = RarArchive.Open(archiveStream);
            var extractedFiles = new List<ExtractedFile>();

            foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var relativePath = SanitizeRelativePath(entry.Key);
                var destinationPath = Path.Combine(extractionRoot, relativePath);
                var destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrWhiteSpace(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                await using var entryStream = entry.OpenEntryStream();
                await using var outputStream = File.Create(destinationPath);
                await entryStream.CopyToAsync(outputStream, cancellationToken);

                var fileInfo = new FileInfo(destinationPath);
                extractedFiles.Add(new ExtractedFile
                {
                    OriginalPath = destinationPath,
                    RelativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/'),
                    FileName = fileInfo.Name,
                    RenamedFileName = $"{Guid.NewGuid():N}{fileInfo.Extension}",
                    FileSize = fileInfo.Length,
                    FileExtension = fileInfo.Extension
                });
            }

            return Result.Success(new IngestionResult
            {
                ExtractionPath = extractionRoot,
                ExtractedFiles = extractedFiles
            });
        }
        catch (ExtractionException ex)
        {
            _logger.LogError(ex, "RAR extraction failed.");
            return Result.Failure<IngestionResult>(Error.BadRequest(
                "Submissions.Ingest.InvalidArchive",
                "Archive is corrupted or in an unsupported format."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while ingesting archive.");
            return Result.Failure<IngestionResult>(Error.Failure(
                "Submissions.Ingest.Unexpected",
                "Failed to process archive."));
        }
    }

    private static string SanitizeRelativePath(string? entryKey)
    {
        if (string.IsNullOrWhiteSpace(entryKey))
        {
            return "unknown";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var parts = entryKey.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        var sanitizedSegments = parts
            .Select(segment =>
            {
                var cleaned = new StringBuilder();
                foreach (var ch in segment)
                {
                    cleaned.Append(invalidChars.Contains(ch) ? '_' : ch);
                }
                return cleaned.ToString();
            })
            .Where(segment => segment != "." && segment != "..");

        var sanitizedPath = Path.Combine(sanitizedSegments.ToArray());
        return string.IsNullOrWhiteSpace(sanitizedPath) ? "unknown" : sanitizedPath;
    }
}



