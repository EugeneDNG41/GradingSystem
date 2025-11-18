using GradingSystem.Shared;
using GradingSystem.Shared.Services.BlobStorage;
using System.IO.Compression;

namespace GradingSystem.Services.Submissions.Api.Services;

public class SubmissionFileService(ILogger<SubmissionFileService> logger) : ISubmissionFileService
{
    private readonly ILogger<SubmissionFileService> _logger = logger;

    public async Task<Result<UnpackResult>> UnpackAsync(string blobName, IBlobService blobService, CancellationToken cancellationToken = default)
    {
        var result = new UnpackResult();
        var tempZipPath = string.Empty;
        var extractPath = string.Empty;

        try
        {
            //Download the blob
            var zipStream = await blobService.DownloadAsync(blobName, cancellationToken);
            //Validate before unpacking
            //Save to temp file
            tempZipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
            var tempFileStream = File.Create(tempZipPath);

            await zipStream.CopyToAsync(tempFileStream, cancellationToken);

            //Extract ZIP file
            extractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(extractPath);

            _logger.LogInformation("Extracting ZIP file to: {ExtractPath}", extractPath);
            ZipFile.ExtractToDirectory(tempZipPath, extractPath);
            return new UnpackResult
            {
                IsSuccess = true,
                ExtractedFiles = Directory.GetFiles(extractPath).Select(f => new ExtractedFile
                {
                    OriginalPath = f,
                    FileName = Path.GetFileName(f),
                    RenamedFileName = $"{Guid.NewGuid()}{Path.GetExtension(f)}",
                    FileSize = new FileInfo(f).Length,
                    FileExtension = Path.GetExtension(f)
                }).ToList(),
                Validation = new ValidationResult
                {
                    IsValid = true

                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpacking submission file from blob {BlobName}", blobName);
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
        finally
        {
            // Clean up temporary files
            try
            {
                if (!string.IsNullOrEmpty(tempZipPath) && File.Exists(tempZipPath))
                {
                    File.Delete(tempZipPath);
                }
                if (!string.IsNullOrEmpty(extractPath) && Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, recursive: true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error cleaning up temporary files");
            }
        }
    }

    public async Task<Result<bool>> UploadFile(IFormFile file)
    {
        if (file.Length == 0)
        {
            return Result.Failure<bool>(Error.BadRequest("001", "Empty file"));
        }
        try
        {
            var uploadPath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
            return Result.Failure<bool>(Error.Failure("002", "File upload failed"));
        }
    }
}
