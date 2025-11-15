namespace GradingSystem.Services.Users.Api.Models
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
