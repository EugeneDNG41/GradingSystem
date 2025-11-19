namespace GradingSystem.Services.Users.Api.Models;

public record UserListItem(
    int Id,
    string Name,
    string Email,
    string Role
);

