using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Services.Submissions.Api.Services;
using GradingSystem.Shared;
using System.Security.Claims;

namespace GradingSystem.Services.Submissions.Api.Endpoints;

internal sealed class UpdateGradeEntry : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/grading/entries/{id:int}", async (
            int id,
            UpdateGradeEntryRequest request,
            ClaimsPrincipal user,
            IGradeEntryService service) =>
        {
            var userIdResult = GetUserId(user);
            if (userIdResult.IsFailure)
            {
                return CustomResults.Problem(Result.Failure<GradeEntryResponse>(userIdResult.Error));
            }

            var result = await service.UpdateAsync(id, request, userIdResult.Value);

            return result.Match(
                Results.Ok,
                CustomResults.Problem
            );
        })
        .RequireAuthorization()
        .WithTags("grading");
    }

    private static Result<int> GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst("id");
        if (claim is null)
        {
            return Result.Failure<int>(Error.Unauthorized(
                "Grading.Update.UserIdMissing",
                "User identifier is missing from the token."));
        }

        if (!int.TryParse(claim.Value, out var userId))
        {
            return Result.Failure<int>(Error.Unauthorized(
                "Grading.Update.UserIdInvalid",
                "User identifier is invalid."));
        }

        return Result.Success(userId);
    }
}

