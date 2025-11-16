using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GradingSystem.Services.Users.Api.Extensions;
using GradingSystem.Services.Users.Api.Services;
using Microsoft.AspNetCore.Routing;

namespace GradingSystem.Services.Users.Api.Endpoints;

internal interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
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