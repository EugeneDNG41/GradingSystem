using GradingSystem.Services.Users.Api;
using GradingSystem.Services.Users.Api.Data;
using GradingSystem.Services.Users.Api.Extensions;
using GradingSystem.Services.Users.Api.Services;
using JasperFx.Resources;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSwaggerDocumentation().AddEndpoints(Assembly.GetExecutingAssembly());

var usersDbConnectionString = builder.Configuration.GetConnectionString("users-db");
builder.Services.AddDbContext<UsersDbContext>(options => options.UseNpgsql(usersDbConnectionString));

var rabbitmqEndpoint = builder.Configuration.GetConnectionString("rabbitmq");
if (rabbitmqEndpoint != null && usersDbConnectionString != null)
{
    builder.Host.UseWolverine(opts =>
    {
        //opts.PublishAllMessages().ToRabbitExchange("grading-system", exchange =>
        //{
        //    exchange.ExchangeType = ExchangeType.Direct;
        //});
        
        opts.UseRabbitMq(new Uri(rabbitmqEndpoint)).AutoProvision().EnableWolverineControlQueues();
        opts.ListenToRabbitQueue("users-service");
        opts.UseEntityFrameworkCoreTransactions();
        opts.PersistMessagesWithPostgresql(usersDbConnectionString);
    }).UseResourceSetupOnStartup();
}
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddAuthentication(builder.Configuration);

var app = builder.Build();
app.ApplyMigrations();
app.UseExceptionHandler();
app.MapDefaultEndpoints();
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
