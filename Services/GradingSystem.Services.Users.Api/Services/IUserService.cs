using GradingSystem.Services.Users.Api.Models;
using GradingSystem.Shared;

namespace GradingSystem.Services.Users.Api.Services
{
    public interface IUserService
    {
        Task<Result<int>> CreateUserAsync(CreateUserRequest request);
        Task<Result<UserResponse>> GetUserByIdAsync(int id);
        Task<Result<List<UserListItem>>> GetUsersAsync();
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    }
}