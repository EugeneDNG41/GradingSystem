using GradingSystem.Shared;

namespace GradingSystem.Services.Users.Api.Services
{
    public interface IUserService
    {
        Task<Result<int>> CreateUserAsync(CreateUserRequest request);
        Task<Result<UserResponse>> GetUserByIdAsync(int id);
    }
}