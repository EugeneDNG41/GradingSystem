using GradingSystem.Services.Users.Api.Data;

namespace GradingSystem.Services.Users.Api.Services
{
    public interface IJwtTokenGenerator
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
