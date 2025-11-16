using GradingSystem.Services.Users.Api.Data;
using GradingSystem.Shared;
using GradingSystem.Shared.Contracts;
using Wolverine;

namespace GradingSystem.Services.Users.Api.Services;

public class UserService(UsersDbContext dbContext, IMessageBus messageBus) : IUserService
{
    public async Task<Result<int>> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        await messageBus.PublishAsync(new UserCreated(user.Id, user.Name, user.Email, user.Role.ToString()));
        return user.Id;
    }
    public async Task<Result<UserResponse>> GetUserByIdAsync(int id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return Result.Failure<UserResponse>(Error.NotFound("USER40401", "User not found."));
        }

        var response = new UserResponse(user.Id, user.Name, user.Email, user.Role.ToString());
        return response;
    }
}
public sealed record CreateUserRequest(string Name, string Email, string Password, UserRole Role);
public sealed record UserResponse(int Id, string Name, string Email, string Role);