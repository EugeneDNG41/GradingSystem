using GradingSystem.Services.Submissions.Api.Services.BlobStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GradingSystem.Services.Submissions.Api.Endpoints;

internal sealed class BlobTestEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/blob-test")
                        .WithTags("BlobStorage Test");

        group.MapPost("/upload", async (
     IFormFile file,
     IBlobService blobService) =>
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("File is required");

            var blobName = $"{Guid.NewGuid()}-{file.FileName}";

            using var stream = file.OpenReadStream();

            await blobService.UploadAsync(stream, blobName, file.ContentType);

            return Results.Ok(new { blobName });
        })
         .Accepts<IFormFile>("multipart/form-data")
         .DisableAntiforgery()
         .WithTags("BlobStorage Test");


        group.MapGet("/download/{blobName}", async (
            string blobName,
            IBlobService blobService) =>
        {
            try
            {
                var stream = await blobService.DownloadAsync(blobName);

                return Results.File(stream,
                    contentType: "application/octet-stream",
                    fileDownloadName: blobName);
            }
            catch (FileNotFoundException)
            {
                return Results.NotFound(new { error = "Blob not found" });
            }
        });
    }
}
