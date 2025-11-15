using Evently.Common.Infrastructure.Authentication;
using GradingSystem.Services.Users.Api;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddDbContext<UsersDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("users-db")));
var rabbitmqEndpoint = builder.Configuration.GetConnectionString("rabbitmq");
if (rabbitmqEndpoint != null)
{
    builder.Host.UseWolverine(opts =>
    {
        opts.PublishAllMessages().ToRabbitExchange("grading-system", exchange =>
        {
            exchange.ExchangeType = ExchangeType.Direct;
        });
        
        opts.UseRabbitMq(new Uri(rabbitmqEndpoint)).AutoProvision().EnableWolverineControlQueues();
        opts.ListenToRabbitQueue("users-service");
        opts.UseEntityFrameworkCoreTransactions();
        opts.PersistMessagesWithPostgresql("users-db");
    });
}
builder.Services.AddAuthentication(builder.Configuration);

var app = builder.Build();
app.MapDefaultEndpoints();
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
