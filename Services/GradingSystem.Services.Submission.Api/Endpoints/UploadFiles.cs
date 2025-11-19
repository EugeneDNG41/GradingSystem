using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Services.Submissions.Api.Services;
using GradingSystem.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GradingSystem.Services.Submissions.Api.Endpoints;

internal sealed class UploadFiles : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/submissions/upload", async (
            [FromForm] UploadSubmissionRequest request,
            ClaimsPrincipal user,
            ISubmissionUploadService submissionUploadService,
            CancellationToken cancellationToken) =>
        {
            var userIdResult = GetUserId(user);
            if (userIdResult.IsFailure)
            {
                return CustomResults.Problem(Result.Failure(userIdResult.Error));
            }

            var uploadResult = await submissionUploadService.UploadAsync(
            request,
            userIdResult.Value,
            cancellationToken);

            return uploadResult.Match(
                response => Results.Created($"/submissions/batches/{response.Id}", response),
                error => CustomResults.Problem(error));
        })
        .Accepts<UploadSubmissionRequest>("multipart/form-data")
        .DisableAntiforgery()
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithTags("submissions");
    }

    private static Result<int> GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst("id");
        if (claim is null)
        {
            return Result.Failure<int>(Error.Unauthorized(
                "Submissions.Upload.UserIdMissing",
                "User identifier is missing from the token."));
        }

        if (!int.TryParse(claim.Value, out var userId))
        {
            return Result.Failure<int>(Error.Unauthorized(
                "Submissions.Upload.UserIdInvalid",
                "User identifier is invalid."));
        }

        return Result.Success(userId);
    }
}
