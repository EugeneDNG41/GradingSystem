using GradingSystem.Services.Users.Api.Extensions;
using GradingSystem.Services.Users.Api.Services;
using Microsoft.AspNetCore.Routing;

namespace GradingSystem.Services.Users.Api.Endpoints;

internal sealed class GetUserById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:int}", async (int id, IUserService userService) =>
        {
            var result = await userService.GetUserByIdAsync(id);
            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags("users");
    }
}

