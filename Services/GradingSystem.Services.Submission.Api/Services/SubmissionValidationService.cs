using GradingSystem.Services.Submissions.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GradingSystem.Services.Submissions.Api.Services;

public sealed class SubmissionValidationService(
    SubmissionsDbContext dbContext,
    ILogger<SubmissionValidationService> logger) : ISubmissionValidationService
{
    private static readonly HashSet<string> RequiredDocExtensions = new(StringComparer.OrdinalIgnoreCase) { ".doc", ".docx", ".pdf" };
    private static readonly HashSet<string> RequiredSourceExtensions = new(StringComparer.OrdinalIgnoreCase) { ".zip", ".rar", ".csproj", ".sln", ".cs", ".java", ".cpp", ".py", ".js" };
    private static readonly HashSet<string> ForbiddenExtensions = new(StringComparer.OrdinalIgnoreCase) { ".exe", ".bat", ".cmd", ".msi", ".dll", ".scr" };
    private const long MaxTotalSizeBytes = 200 * 1024 * 1024; // 200MB

    private readonly SubmissionsDbContext _dbContext = dbContext;
    private readonly ILogger<SubmissionValidationService> _logger = logger;

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
            HasDatFile = filesList.Any(f => string.Equals(f.FileExtension, ".dat", StringComparison.OrdinalIgnoreCase)),
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
                StudentCode = DetermineStudentCode(f, rootFolder)
            })
            .ToList();

        var groupedByStudent = annotatedFiles
            .Where(x => !string.IsNullOrWhiteSpace(x.StudentCode))
            .GroupBy(x => x.StudentCode!, x => x.File)
            .ToDictionary(g => g.Key, g => g.ToList());

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
            var entry = new SubmissionEntry
            {
                SubmissionBatchId = batch.Id,
                StudentCode = studentCode,
                ExtractedAt = DateTime.UtcNow,
                Metadata = JsonSerializer.Serialize(new
                {
                    Files = studentFiles.Select(f => new
                    {
                        f.RelativePath,
                        f.FileSize,
                        f.FileExtension
                    })
                })
            };

            entries.Add(entry);

            ValidateStudentFolder(studentCode, studentFiles, entry, validation, violations);
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

    private static string? DetermineStudentCode(ExtractedFile file, string? rootFolder)
    {
        var relativePath = file.RelativePath ?? string.Empty;
        var parts = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Cần ít nhất root/root/mssv
        if (parts.Length < 3 || string.IsNullOrWhiteSpace(rootFolder))
            return null;

        // Kiểm tra bắt buộc phải có 2 root liên tiếp
        if (!parts[0].Equals(rootFolder, StringComparison.OrdinalIgnoreCase) ||
            !parts[1].Equals(rootFolder, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Mã số sinh viên là phần thứ 3
        return parts[2];
    }


    private static string? DetectRootFolder(IReadOnlyCollection<ExtractedFile> files)
    {
        return files
            .Select(f => f.RelativePath)
            .Where(path => !string.IsNullOrWhiteSpace(path) && path.Contains('/'))
            .Select(path => path!.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
            .Where(segment => !string.IsNullOrWhiteSpace(segment))
            .GroupBy(segment => segment!)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()
            ?.Key;
    }

    private static void ValidateStudentFolder(
        string studentCode,
        List<ExtractedFile> files,
        SubmissionEntry entry,
        ValidationResult validation,
        List<SubmissionViolation> violations)
    {
        bool hasDoc = files.Any(f => RequiredDocExtensions.Contains(f.FileExtension));
        bool hasSource = files.Any(f => RequiredSourceExtensions.Contains(f.FileExtension));
        var forbiddenFiles = files.Where(f => ForbiddenExtensions.Contains(f.FileExtension)).ToList();

        if (!hasDoc)
        {
            AddViolation(entry, validation, violations, ViolationType.MissingContent,
                $"Sinh viên {studentCode} thiếu file báo cáo (.doc, .docx, .pdf).");
        }

        if (!hasSource)
        {
            AddViolation(entry, validation, violations, ViolationType.MissingContent,
                $"Sinh viên {studentCode} thiếu mã nguồn.");
        }

        if (forbiddenFiles.Count > 0)
        {
            foreach (var forbidden in forbiddenFiles)
            {
                AddViolation(entry, validation, violations, ViolationType.UnauthorizedResources,
                    $"File '{forbidden.RelativePath}' có định dạng bị cấm ({forbidden.FileExtension}).");
            }
        }

        foreach (var extraFile in files.Where(f =>
                     !RequiredDocExtensions.Contains(f.FileExtension) &&
                     !RequiredSourceExtensions.Contains(f.FileExtension) &&
                     !ForbiddenExtensions.Contains(f.FileExtension)))
        {
            validation.Warnings.Add($"Sinh viên {studentCode} có file phụ '{extraFile.RelativePath}'.");
        }
    }

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


