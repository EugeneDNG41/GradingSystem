using System.Text.Json;

namespace GradingSystem.Services.Submissions.Api.Services;

internal sealed class SubmissionEntryMetadata
{
    public string StudentCode { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public long TotalSize { get; set; }
    public List<SubmissionEntryFileMetadata> Files { get; set; } = new();
    public List<string> MissingItems { get; set; } = new();
    public List<string> ExtraItems { get; set; } = new();
    public int? SolutionAssetId { get; set; }
    public List<int> AttachmentAssetIds { get; set; } = new();
}

internal sealed class SubmissionEntryFileMetadata
{
    public string RelativePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Extension { get; set; } = string.Empty;
    public SubmissionEntryFileCategory Category { get; set; }
}

internal enum SubmissionEntryFileCategory
{
    SolutionPackage,
    Attachment,
    Source,
    Forbidden,
    Extra
}

internal static class SubmissionEntryMetadataSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Serialize(SubmissionEntryMetadata metadata) =>
        JsonSerializer.Serialize(metadata, SerializerOptions);

    public static SubmissionEntryMetadata? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var metadata = JsonSerializer.Deserialize<SubmissionEntryMetadata>(json, SerializerOptions);
        if (metadata is null)
        {
            return null;
        }

        metadata.AttachmentAssetIds ??= new List<int>();
        metadata.MissingItems ??= new List<string>();
        metadata.ExtraItems ??= new List<string>();
        metadata.Files ??= new List<SubmissionEntryFileMetadata>();
        return metadata;
    }
}

