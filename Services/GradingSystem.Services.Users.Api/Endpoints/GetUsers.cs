using GradingSystem.Services.Users.Api.Extensions;
using GradingSystem.Services.Users.Api.Services;
using Microsoft.AspNetCore.Routing;

namespace GradingSystem.Services.Users.Api.Endpoints;

internal sealed class GetUsers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users", async (IUserService userService) =>
        {
            var result = await userService.GetUsersAsync();
            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags("users");
    }
}

