using GradingSystem.Services.Users.Api.Extensions;
using GradingSystem.Services.Users.Api.Models;
using GradingSystem.Services.Users.Api.Services;
using GradingSystem.Shared;
using Microsoft.AspNetCore.Routing;

namespace GradingSystem.Services.Users.Api.Endpoints;

internal sealed class LoginUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/login", async (LoginRequest request, IUserService userService) =>
        {
            var result = await userService.LoginAsync(request);

            return result.Match(
                Results.Ok,               
                CustomResults.Problem     
            );
        })
        .WithTags("users");
    }
}
