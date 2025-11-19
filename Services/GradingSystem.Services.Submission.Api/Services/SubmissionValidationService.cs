using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Services.Submissions.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace GradingSystem.Services.Submissions.Api.Services;

public sealed class SubmissionValidationService(
    SubmissionsDbContext dbContext,
    ILogger<SubmissionValidationService> logger,
    IOptions<SubmissionValidationOptions> validationOptions) : ISubmissionValidationService
{
    private static readonly HashSet<string> RequiredSourceExtensions = new(StringComparer.OrdinalIgnoreCase) { ".zip", ".rar", ".csproj", ".sln", ".cs" };
    private static readonly HashSet<string> ForbiddenExtensions = new(StringComparer.OrdinalIgnoreCase) { ".exe", ".bat", ".cmd", ".msi", ".dll", ".scr" };
    private const long MaxTotalSizeBytes = 200 * 1024 * 1024; // 200MB
    private const string ForbiddenFileCode = "FORBIDDEN_FILE";
    private const string ExtraFileCode = "EXTRA_FILE";
    private const string InvalidFolderCode = "INVALID_FOLDER";

    private readonly SubmissionsDbContext _dbContext = dbContext;
    private readonly ILogger<SubmissionValidationService> _logger = logger;
    private readonly SubmissionValidationOptions _options = validationOptions.Value;
    private readonly Regex _studentFolderRegex = new(validationOptions.Value.StudentFolderPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly HashSet<string> _allowedStudentExtraExtensions =
        validationOptions.Value.AllowedStudentExtraExtensions?
            .Where(ext => !string.IsNullOrWhiteSpace(ext))
            .Select(NormalizeExtension)
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public async Task<ValidationOutcome> ValidateAsync(
        SubmissionBatch batch,
        IReadOnlyCollection<ExtractedFile> files,
        CancellationToken cancellationToken = default)
    {
        var filesList = files.ToList();
        var validation = new ValidationResult
        {
            TotalFiles = filesList.Count,
            TotalSize = filesList.Sum(f => f.FileSize),
            IsValid = true
        };

        if (validation.TotalSize > MaxTotalSizeBytes)
        {
            validation.Errors.Add($"Tổng dung lượng {validation.TotalSize / (1024 * 1024)}MB vượt giới hạn {(MaxTotalSizeBytes / (1024 * 1024))}MB.");
            validation.IsValid = false;
        }

        await RemoveExistingEntriesAsync(batch.Id, cancellationToken);

        var entries = new List<SubmissionEntry>();
        var violations = new List<SubmissionViolation>();
        var rootFolder = DetectRootFolder(filesList);

        var annotatedFiles = filesList
            .Select(f => new
            {
                File = f,
                StudentCode = DetermineStudentCode(f, rootFolder, _studentFolderRegex)
            })
            .ToList();

        var groupedByStudent = annotatedFiles
            .Where(x => !string.IsNullOrWhiteSpace(x.StudentCode))
            .GroupBy(x => x.StudentCode!, x => x.File)
            .ToDictionary(g => g.Key, g => g.ToList());

        validation.StudentCount = groupedByStudent.Count;

        var orphanFiles = annotatedFiles
            .Where(x => string.IsNullOrWhiteSpace(x.StudentCode))
            .Select(x => x.File)
            .ToList();

        if (groupedByStudent.Count == 0)
        {
            validation.Errors.Add("Không tìm thấy thư mục MSSV trong archive.");
            validation.IsValid = false;
        }

        foreach (var orphan in orphanFiles)
        {
            validation.Warnings.Add($"File '{orphan.RelativePath}' nằm ngoài thư mục sinh viên.");
        }

        foreach (var (studentCode, studentFiles) in groupedByStudent)
        {
            var metadata = BuildMetadata(studentCode, studentFiles);

            var entry = new SubmissionEntry
            {
                SubmissionBatchId = batch.Id,
                StudentCode = studentCode,
                ExtractedAt = DateTime.UtcNow,
                Metadata = SubmissionEntryMetadataSerializer.Serialize(metadata)
            };

            entries.Add(entry);

            ValidateStudentFolder(studentCode, studentFiles, entry, validation, violations, metadata);
        }

        await PersistAsync(entries, violations, cancellationToken);

        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed for submission batch {BatchId}. Errors: {Errors}", batch.Id, string.Join("; ", validation.Errors));
        }

        return new ValidationOutcome
        {
            Validation = validation,
            Entries = entries,
            Violations = violations
        };
    }

    private async Task RemoveExistingEntriesAsync(int batchId, CancellationToken cancellationToken)
    {
        var existingEntries = await _dbContext.SubmissionEntries
            .Where(e => e.SubmissionBatchId == batchId)
            .ToListAsync(cancellationToken);

        if (existingEntries.Count == 0)
        {
            return;
        }

        _dbContext.SubmissionEntries.RemoveRange(existingEntries);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task PersistAsync(
        List<SubmissionEntry> entries,
        List<SubmissionViolation> violations,
        CancellationToken cancellationToken)
    {
        if (entries.Count > 0)
        {
            await _dbContext.SubmissionEntries.AddRangeAsync(entries, cancellationToken);
        }

        if (violations.Count > 0)
        {
            await _dbContext.SubmissionViolations.AddRangeAsync(violations, cancellationToken);
        }

        if (entries.Count > 0 || violations.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static string? DetermineStudentCode(ExtractedFile file, string? rootFolder, Regex studentFolderRegex)
    {
        var relativePath = file.RelativePath ?? string.Empty;
        var parts = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return null;
        }

        var index = 0;

        if (!string.IsNullOrWhiteSpace(rootFolder))
        {
            while (index < parts.Length &&
                   parts[index].Equals(rootFolder, StringComparison.OrdinalIgnoreCase))
            {
                index++;
            }
        }

        if (parts.Length - index < 2)
        {
            return null;
        }

        string? fallback = null;
        for (var i = index; i < parts.Length - 1; i++)
        {
            var segment = parts[i];
            if (studentFolderRegex.IsMatch(segment))
            {
                return segment;
            }

            fallback = segment;
        }

        return fallback;
    }


    private static string? DetectRootFolder(IReadOnlyCollection<ExtractedFile> files)
    {
        var grouping = files
            .Select(f => f.RelativePath)
            .Where(path => !string.IsNullOrWhiteSpace(path) && path.Contains('/'))
            .Select(path => path!.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .GroupBy(segment => segment!)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (grouping is null)
        {
            return null;
        }

        return grouping.Count() > 1 ? grouping.Key : null;
    }

    private void ValidateStudentFolder(
        string studentCode,
        List<ExtractedFile> files,
        SubmissionEntry entry,
        ValidationResult validation,
        List<SubmissionViolation> violations,
        SubmissionEntryMetadata metadata)
    {
        var sourceFiles = files.Where(f => RequiredSourceExtensions.Contains(f.FileExtension)).ToList();
        var forbiddenFiles = files.Where(f => ForbiddenExtensions.Contains(f.FileExtension)).ToList();
        var extraFiles = files
            .Where(f =>
                !RequiredSourceExtensions.Contains(f.FileExtension) &&
                !ForbiddenExtensions.Contains(f.FileExtension) &&
                !IsAllowedStudentExtra(f))
            .ToList();

        if (!_studentFolderRegex.IsMatch(studentCode))
        {
            AddViolation(entry, validation, violations, ViolationType.InvalidFormat,
                BuildViolationMessage(InvalidFolderCode,
                    $"Thư mục '{studentCode}' không đúng định dạng MSSV (pattern {_options.StudentFolderPattern})."));
        }

        if (forbiddenFiles.Count > 0)
        {
            foreach (var forbidden in forbiddenFiles)
            {
                AddViolation(entry, validation, violations, ViolationType.UnauthorizedResources,
                    BuildViolationMessage(ForbiddenFileCode,
                        $"File '{forbidden.RelativePath}' có định dạng bị cấm ({forbidden.FileExtension})."));
            }
        }

        if (extraFiles.Count > 0)
        {
            metadata.ExtraItems.AddRange(extraFiles.Select(f => f.RelativePath ?? f.FileName));

            if (_options.FlagExtraFilesAsViolations)
            {
                foreach (var extraFile in extraFiles)
                {
                    AddViolation(entry, validation, violations, ViolationType.InvalidFormat,
                        BuildViolationMessage(ExtraFileCode,
                            $"File '{extraFile.RelativePath}' nằm ngoài danh mục được phép."));
                }
            }
            else
            {
                foreach (var extraFile in extraFiles)
                {
                    validation.Warnings.Add($"Sinh viên {studentCode} có file phụ '{extraFile.RelativePath}'.");
                }
            }
        }
    }

    private static SubmissionEntryMetadata BuildMetadata(string studentCode, List<ExtractedFile> files)
    {
        var metadata = new SubmissionEntryMetadata
        {
            StudentCode = studentCode,
            TotalFiles = files.Count,
            TotalSize = files.Sum(f => f.FileSize),
            Files = files.Select(f => new SubmissionEntryFileMetadata
            {
                RelativePath = string.IsNullOrWhiteSpace(f.RelativePath) ? f.FileName : f.RelativePath!,
                Size = f.FileSize,
                Extension = f.FileExtension,
                Category = CategorizeFile(f)
            }).ToList()
        };

        return metadata;
    }

    private static SubmissionEntryFileCategory CategorizeFile(ExtractedFile file)
    {
        if (RequiredSourceExtensions.Contains(file.FileExtension))
        {
            return SubmissionEntryFileCategory.Source;
        }

        if (ForbiddenExtensions.Contains(file.FileExtension))
        {
            return SubmissionEntryFileCategory.Forbidden;
        }

        return SubmissionEntryFileCategory.Extra;
    }

    private static string BuildViolationMessage(string code, string message) =>
        $"[{code}] {message}";

    private static string NormalizeExtension(string raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? string.Empty
            : raw.StartsWith('.') ? raw : $".{raw}";

    private bool IsAllowedStudentExtra(ExtractedFile file) =>
        _allowedStudentExtraExtensions.Contains(NormalizeExtension(file.FileExtension));

    private static void AddViolation(
        SubmissionEntry entry,
        ValidationResult validation,
        List<SubmissionViolation> violations,
        ViolationType violationType,
        string message)
    {
        validation.IsValid = false;
        validation.Errors.Add(message);

        violations.Add(new SubmissionViolation
        {
            SubmissionEntry = entry,
            Type = violationType,
            Severity = violationType == ViolationType.UnauthorizedResources ? 3 : 2,
            Description = message
        });
    }
}


