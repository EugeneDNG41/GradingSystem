using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Users.Api;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
}
