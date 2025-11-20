using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Services.Submissions.Api.Services.BlobStorage;
using GradingSystem.Shared;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Submissions.Api.Services;

public interface ISubmissionAssetService
{
    Task<Result> PersistAssetsAsync(
        SubmissionBatch batch,
        IReadOnlyCollection<int> submissionEntryIds,
        IReadOnlyCollection<ExtractedFile> extractedFiles,
        CancellationToken cancellationToken = default);
}

internal sealed class SubmissionAssetService(
    SubmissionsDbContext dbContext,
    IBlobService blobService,
    ILogger<SubmissionAssetService> logger) : ISubmissionAssetService
{
    private readonly SubmissionsDbContext _dbContext = dbContext;
    private readonly IBlobService _blobService = blobService;
    private readonly ILogger<SubmissionAssetService> _logger = logger;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public async Task<Result> PersistAssetsAsync(
        SubmissionBatch batch,
        IReadOnlyCollection<int> submissionEntryIds,
        IReadOnlyCollection<ExtractedFile> extractedFiles,
        CancellationToken cancellationToken = default)
    {
        if (submissionEntryIds.Count == 0)
        {
            return Result.Success();
        }

        try
        {
            var entries = await _dbContext.SubmissionEntries
                .Include(e => e.Assets)
                .Where(e => submissionEntryIds.Contains(e.Id))
                .ToListAsync(cancellationToken);

            if (entries.Count == 0)
            {
                return Result.Failure(Error.NotFound(
                    "SubmissionAssets.EntriesMissing",
                    "Không tìm thấy submission entry cần xử lý asset."));
            }

            var extractedLookup = extractedFiles
                .Where(f => !string.IsNullOrWhiteSpace(f.RelativePath))
                .GroupBy(f => NormalizeRelativePath(f.RelativePath))
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var entryUpdates = new List<EntryAssetUpdate>();

            foreach (var entry in entries)
            {
                var metadata = SubmissionEntryMetadataSerializer.Deserialize(entry.Metadata);
                if (metadata is null)
                {
                    continue;
                }

                metadata.AttachmentAssetIds ??= new List<int>();

                var updateContext = new EntryAssetUpdate(entry, metadata);

                if (metadata.SolutionAssetId is null)
                {
                    var solutionMeta = metadata.Files.FirstOrDefault(f => f.Category == SubmissionEntryFileCategory.SolutionPackage);
                    if (solutionMeta is not null &&
                        TryGetExtractedFile(solutionMeta.RelativePath, extractedLookup, out var extractedSolution))
                    {
                        var asset = await CreateAssetAsync(
                            entry,
                            extractedSolution,
                            "application/zip",
                            cancellationToken);
                        updateContext.PendingSolutionAsset = asset;
                    }
                }

                foreach (var attachmentMeta in metadata.Files.Where(f => f.Category == SubmissionEntryFileCategory.Attachment))
                {
                    if (metadata.AttachmentAssetIds.ContainsAsset(entry.Assets, attachmentMeta.RelativePath))
                    {
                        continue;
                    }

                    if (!TryGetExtractedFile(attachmentMeta.RelativePath, extractedLookup, out var extractedAttachment))
                    {
                        continue;
                    }

                    var asset = await CreateAssetAsync(
                        entry,
                        extractedAttachment,
                        DetermineContentType(extractedAttachment.FileExtension),
                        cancellationToken);
                    updateContext.PendingAttachmentAssets.Add(asset);
                }

                if (updateContext.HasPendingAssets)
                {
                    entryUpdates.Add(updateContext);
                }
            }

            if (entryUpdates.Count == 0)
            {
                return Result.Success();
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var update in entryUpdates)
            {
                var metadata = update.Metadata;
                var entry = update.Entry;

                if (update.PendingSolutionAsset is not null)
                {
                    metadata.SolutionAssetId = update.PendingSolutionAsset.Id;
                }

                foreach (var attachmentAsset in update.PendingAttachmentAssets)
                {
                    metadata.AttachmentAssetIds.Add(attachmentAsset.Id);
                }

                entry.Metadata = SubmissionEntryMetadataSerializer.Serialize(metadata);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể lưu submission assets cho batch {BatchId}", batch.Id);
            return Result.Failure(Error.Failure(
                "SubmissionAssets.PersistFailed",
                "Không thể lưu asset cho bài nộp."));
        }
    }

    private async Task<SubmissionAsset> CreateAssetAsync(
        SubmissionEntry entry,
        ExtractedFile file,
        string contentType,
        CancellationToken cancellationToken)
    {
        var blobName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        await using var stream = File.OpenRead(file.OriginalPath);
        await _blobService.UploadAsync(stream, blobName, contentType, cancellationToken);

        var asset = new SubmissionAsset
        {
            SubmissionEntryId = entry.Id,
            FileName = file.FileName,
            MimeType = contentType,
            BlobName = blobName,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.SubmissionAssets.Add(asset);
        entry.Assets.Add(asset);
        return asset;
    }

    private static bool TryGetExtractedFile(
        string? relativePath,
        IReadOnlyDictionary<string, ExtractedFile> lookup,
        out ExtractedFile extractedFile)
    {
        extractedFile = default!;
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        return lookup.TryGetValue(NormalizeRelativePath(relativePath), out extractedFile);
    }

    private string DetermineContentType(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return "application/octet-stream";
        }

        if (_contentTypeProvider.TryGetContentType($"file{extension}", out var contentType))
        {
            return contentType;
        }

        return "application/octet-stream";
    }

    private static string NormalizeRelativePath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        return relativePath.Replace('\\', '/');
    }

    private sealed class EntryAssetUpdate
    {
        public EntryAssetUpdate(SubmissionEntry entry, SubmissionEntryMetadata metadata)
        {
            Entry = entry;
            Metadata = metadata;
        }

        public SubmissionEntry Entry { get; }
        public SubmissionEntryMetadata Metadata { get; }
        public SubmissionAsset? PendingSolutionAsset { get; set; }
        public List<SubmissionAsset> PendingAttachmentAssets { get; } = new();
        public bool HasPendingAssets => PendingSolutionAsset is not null || PendingAttachmentAssets.Count > 0;
    }
}

internal static class SubmissionMetadataExtensions
{
    public static bool ContainsAsset(this List<int>? attachmentAssetIds, ICollection<SubmissionAsset> existingAssets, string? relativePath)
    {
        if (attachmentAssetIds is null || attachmentAssetIds.Count == 0 || existingAssets.Count == 0 || string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        var normalized = relativePath.Replace('\\', '/');
        return existingAssets
            .Any(asset => attachmentAssetIds.Contains(asset.Id) &&
                          normalized.EndsWith(asset.FileName, StringComparison.OrdinalIgnoreCase));
    }
}

