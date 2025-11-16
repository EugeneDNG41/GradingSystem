namespace GradingSystem.Services.Users.Api.Data;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public UserRole Role { get; set; }

}
public enum UserRole
{
    Admin,
    Manager,
    Moderator,
    Examiner
}
