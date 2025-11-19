using GradingSystem.Services.Users.Api.Data;
using GradingSystem.Services.Users.Api.Models;
using GradingSystem.Shared;
using GradingSystem.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace GradingSystem.Services.Users.Api.Services;

public class UserService
    : IUserService
{
    
    private readonly UsersDbContext _dbContext;
    private readonly IMessageBus _messageBus;
    private readonly IJwtTokenGenerator _jwt;
    public UserService(UsersDbContext dbContext, IMessageBus messageBus, IJwtTokenGenerator jwt)
    {
        _dbContext = dbContext;
        _messageBus = messageBus;
        _jwt = jwt;
    }
    public async Task<Result<int>> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        await _messageBus.PublishAsync(new UserCreated(user.Id, user.Name, user.Email, user.Role.ToString()));
        return user.Id;
    }
    public async Task<Result<UserResponse>> GetUserByIdAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return Result.Failure<UserResponse>(Error.NotFound("USER40401", "User not found."));
        }

        var response = new UserResponse(user.Id, user.Name, user.Email, user.Role.ToString());
        return response;
    }
    public async Task<Result<List<UserListItem>>> GetUsersAsync()
    {
        var users = await _dbContext.Users
            .OrderBy(u => u.Name)
            .ToListAsync();

        var result = users.Select(u => new UserListItem(
            u.Id,
            u.Name,
            u.Email,
            u.Role.ToString()
        )).ToList();

        return result;
    }
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("AUTH40101", "Invalid email or password.")
            );
        }

        string token = _jwt.GenerateAccessToken(user);

        var response = new LoginResponse
        {
            AccessToken = token
        };

        return response;
    }

}
public sealed record CreateUserRequest(string Name, string Email, string Password, UserRole Role);
public sealed record UserResponse(int Id, string Name, string Email, string Role);