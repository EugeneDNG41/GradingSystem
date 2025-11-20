using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Services.Submissions.Api.Services.BlobStorage;
using GradingSystem.Shared;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Linq;

namespace GradingSystem.Services.Submissions.Api.Services;

public interface ISubmissionSolutionService
{
    Task<Result<SubmissionSolutionFilesResponse>> ListFilesAsync(
        int submissionEntryId,
        CancellationToken cancellationToken = default);

    Task<Result<FileDownloadResult>> DownloadSolutionFileAsync(
        int submissionEntryId,
        string filePath,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<SubmissionAttachmentResponse>>> ListAttachmentsAsync(
        int submissionEntryId,
        CancellationToken cancellationToken = default);

    Task<Result<FileDownloadResult>> DownloadAttachmentAsync(
        int submissionEntryId,
        int attachmentAssetId,
        CancellationToken cancellationToken = default);
}

internal sealed class SubmissionSolutionService(
    SubmissionsDbContext dbContext,
    IBlobService blobService,
    ILogger<SubmissionSolutionService> logger) : ISubmissionSolutionService
{
    private readonly SubmissionsDbContext _dbContext = dbContext;
    private readonly IBlobService _blobService = blobService;
    private readonly ILogger<SubmissionSolutionService> _logger = logger;

    public async Task<Result<SubmissionSolutionFilesResponse>> ListFilesAsync(
        int submissionEntryId,
        CancellationToken cancellationToken = default)
    {
        var entryResult = await LoadEntryWithAssetsAsync(submissionEntryId, cancellationToken);
        if (entryResult.IsFailure)
        {
            return Result.Failure<SubmissionSolutionFilesResponse>(entryResult.Error);
        }

        var (entry, metadata, solutionAsset) = entryResult.Value;
        try
        {
            await using var blobStream = await _blobService.DownloadAsync(solutionAsset.BlobName, cancellationToken);
            using var archive = new ZipArchive(blobStream, ZipArchiveMode.Read, leaveOpen: true);

            var files = BuildFileDescriptors(archive);
            var attachments = BuildAttachmentResponses(entry, metadata);

            return Result.Success(new SubmissionSolutionFilesResponse
            {
                SubmissionEntryId = entry.Id,
                SolutionFiles = files,
                Attachments = attachments
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể đọc solution.zip cho entry {EntryId}", entry.Id);
            return Result.Failure<SubmissionSolutionFilesResponse>(Error.Failure(
                "SubmissionSolution.ListFailed",
                "Không thể đọc nội dung solution.zip."));
        }
    }

    public async Task<Result<FileDownloadResult>> DownloadSolutionFileAsync(
        int submissionEntryId,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Result.Failure<FileDownloadResult>(Error.BadRequest(
                "SubmissionSolution.InvalidPath",
                "Đường dẫn file cần tải không hợp lệ."));
        }

        var entryResult = await LoadEntryWithAssetsAsync(submissionEntryId, cancellationToken);
        if (entryResult.IsFailure)
        {
            return Result.Failure<FileDownloadResult>(entryResult.Error);
        }

        var (entry, _, solutionAsset) = entryResult.Value;
        var normalizedPath = NormalizeZipPath(filePath);

        try
        {
            await using var blobStream = await _blobService.DownloadAsync(solutionAsset.BlobName, cancellationToken);
            using var archive = new ZipArchive(blobStream, ZipArchiveMode.Read, leaveOpen: true);

            var targetEntry = archive.Entries.FirstOrDefault(e =>
                !string.IsNullOrWhiteSpace(e.FullName) &&
                NormalizeZipPath(e.FullName).Equals(normalizedPath, StringComparison.OrdinalIgnoreCase));

            if (targetEntry is null)
            {
                return Result.Failure<FileDownloadResult>(Error.NotFound(
                    "SubmissionSolution.FileNotFound",
                    "Không tìm thấy file trong solution.zip."));
            }

            var memoryStream = new MemoryStream();
            await using var entryStream = targetEntry.Open();
            await entryStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            return Result.Success(new FileDownloadResult(
                memoryStream,
                Path.GetFileName(targetEntry.FullName),
                "application/octet-stream"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải file {Path} từ solution entry {EntryId}", filePath, entry.Id);
            return Result.Failure<FileDownloadResult>(Error.Failure(
                "SubmissionSolution.DownloadFailed",
                "Không thể tải file mong muốn."));
        }
    }

    public async Task<Result<IReadOnlyList<SubmissionAttachmentResponse>>> ListAttachmentsAsync(
        int submissionEntryId,
        CancellationToken cancellationToken = default)
    {
        var entryResult = await LoadEntryWithAssetsAsync(submissionEntryId, cancellationToken);
        if (entryResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<SubmissionAttachmentResponse>>(entryResult.Error);
        }

        var (entry, metadata, _) = entryResult.Value;
        var attachments = BuildAttachmentResponses(entry, metadata);
        return Result.Success((IReadOnlyList<SubmissionAttachmentResponse>)attachments);
    }

    public async Task<Result<FileDownloadResult>> DownloadAttachmentAsync(
        int submissionEntryId,
        int attachmentAssetId,
        CancellationToken cancellationToken = default)
    {
        var entryResult = await LoadEntryWithAssetsAsync(submissionEntryId, cancellationToken);
        if (entryResult.IsFailure)
        {
            return Result.Failure<FileDownloadResult>(entryResult.Error);
        }

        var (entry, metadata, _) = entryResult.Value;
        if (metadata.AttachmentAssetIds is null || !metadata.AttachmentAssetIds.Contains(attachmentAssetId))
        {
            return Result.Failure<FileDownloadResult>(Error.NotFound(
                "SubmissionSolution.AttachmentMissing",
                "Không tìm thấy file đính kèm yêu cầu."));
        }

        var asset = entry.Assets.FirstOrDefault(a => a.Id == attachmentAssetId);
        if (asset is null)
        {
            return Result.Failure<FileDownloadResult>(Error.NotFound(
                "SubmissionSolution.AttachmentBlobMissing",
                "Không tìm thấy dữ liệu file đính kèm."));
        }

        try
        {
            var stream = await _blobService.DownloadAsync(asset.BlobName, cancellationToken);
            return Result.Success(new FileDownloadResult(
                stream,
                asset.FileName,
                asset.MimeType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải attachment {AttachmentId} của entry {EntryId}", attachmentAssetId, entry.Id);
            return Result.Failure<FileDownloadResult>(Error.Failure(
                "SubmissionSolution.AttachmentDownloadFailed",
                "Không thể tải file đính kèm."));
        }
    }

    private async Task<Result<(SubmissionEntry Entry, SubmissionEntryMetadata Metadata, SubmissionAsset SolutionAsset)>> LoadEntryWithAssetsAsync(
        int submissionEntryId,
        CancellationToken cancellationToken)
    {
        var entry = await _dbContext.SubmissionEntries
            .Include(e => e.Assets)
            .FirstOrDefaultAsync(e => e.Id == submissionEntryId, cancellationToken);

        if (entry is null)  
        {
            return Result.Failure<(SubmissionEntry, SubmissionEntryMetadata, SubmissionAsset)>(Error.NotFound(
                "SubmissionSolution.EntryMissing",
                "Không tìm thấy submission entry."));
        }

        var metadata = SubmissionEntryMetadataSerializer.Deserialize(entry.Metadata);
        if (metadata is null || metadata.SolutionAssetId is null)
        {
            return Result.Failure<(SubmissionEntry, SubmissionEntryMetadata, SubmissionAsset)>(Error.NotFound(
                "SubmissionSolution.MetadataMissing",
                "Entry chưa có dữ liệu solution."));
        }

        var solutionAsset = entry.Assets.FirstOrDefault(a => a.Id == metadata.SolutionAssetId);
        if (solutionAsset is null)
        {
            return Result.Failure<(SubmissionEntry, SubmissionEntryMetadata, SubmissionAsset)>(Error.NotFound(
                "SubmissionSolution.AssetMissing",
                "Không tìm thấy solution asset."));
        }

        metadata.AttachmentAssetIds ??= new List<int>();

        return Result.Success((entry, metadata, solutionAsset));
    }

    private static List<SolutionFileDescriptor> BuildFileDescriptors(ZipArchive archive)
    {
        var files = new List<SolutionFileDescriptor>();
        var directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in archive.Entries)
        {
            var normalized = NormalizeZipPath(entry.FullName);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                continue;
            }

            if (normalized.EndsWith('/'))
            {
                directories.Add(normalized.TrimEnd('/'));
                continue;
            }

            files.Add(new SolutionFileDescriptor
            {
                Path = normalized,
                Size = entry.Length,
                IsDirectory = false
            });

            AddParentDirectories(normalized, directories);
        }

        files.AddRange(directories.Select(dir => new SolutionFileDescriptor
        {
            Path = dir,
            Size = 0,
            IsDirectory = true
        }));

        return files
            .DistinctBy(f => f.Path, StringComparer.OrdinalIgnoreCase)
            .OrderBy(f => f.Path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void AddParentDirectories(string path, HashSet<string> directories)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length <= 1)
        {
            return;
        }

        var current = string.Empty;
        for (var i = 0; i < segments.Length - 1; i++)
        {
            current = string.IsNullOrEmpty(current) ? segments[i] : $"{current}/{segments[i]}";
            directories.Add(current);
        }
    }

    private static List<SubmissionAttachmentResponse> BuildAttachmentResponses(
        SubmissionEntry entry,
        SubmissionEntryMetadata metadata)
    {
        if (metadata.AttachmentAssetIds is null || metadata.AttachmentAssetIds.Count == 0)
        {
            return new List<SubmissionAttachmentResponse>();
        }

        var attachmentFiles = metadata.Files
            .Where(f => f.Category == SubmissionEntryFileCategory.Attachment)
            .ToList();

        var attachments = new List<SubmissionAttachmentResponse>();
        foreach (var assetId in metadata.AttachmentAssetIds)
        {
            var asset = entry.Assets.FirstOrDefault(a => a.Id == assetId);
            if (asset is null)
            {
                continue;
            }

            var fileMeta = attachmentFiles.FirstOrDefault(f =>
                !string.IsNullOrWhiteSpace(f.RelativePath) &&
                f.RelativePath.EndsWith(asset.FileName, StringComparison.OrdinalIgnoreCase));

            attachments.Add(new SubmissionAttachmentResponse
            {
                AssetId = asset.Id,
                FileName = asset.FileName,
                RelativePath = fileMeta?.RelativePath ?? asset.FileName,
                Size = fileMeta?.Size ?? 0
            });
        }

        return attachments;
    }

    private static string NormalizeZipPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return path.Replace('\\', '/');
    }
}

