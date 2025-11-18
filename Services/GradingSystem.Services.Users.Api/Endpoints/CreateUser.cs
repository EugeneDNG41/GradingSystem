using GradingSystem.Services.Users.Api.Extensions;
using GradingSystem.Services.Users.Api.Services;

namespace GradingSystem.Services.Users.Api.Endpoints;

internal sealed class CreateUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users", async (CreateUserRequest request, IUserService userService) =>
        {
            var result = await userService.CreateUserAsync(request);
            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags("users");
    }
}