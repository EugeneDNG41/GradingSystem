using GradingSystem.Services.Exams.Api;
using GradingSystem.Services.Exams.Api.Data;
using GradingSystem.Services.Exams.Api.Extensions;
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

var examsDbConnectionString = builder.Configuration.GetConnectionString("exams-db");
builder.Services.AddDbContext<ExamsDbContext>(options => options.UseNpgsql(examsDbConnectionString));

var rabbitmqEndpoint = builder.Configuration.GetConnectionString("rabbitmq");
if (rabbitmqEndpoint != null && examsDbConnectionString != null)
{
    builder.Host.UseWolverine(opts =>
    {
        //opts.PublishAllMessages().ToRabbitExchange("grading-system", exchange =>
        //{
        //    exchange.ExchangeType = ExchangeType.Direct;
        //});

        opts.UseRabbitMq(new Uri(rabbitmqEndpoint)).AutoProvision().EnableWolverineControlQueues();
        opts.ListenToRabbitQueue("exams-service");
        opts.UseEntityFrameworkCoreTransactions();
        opts.PersistMessagesWithPostgresql(examsDbConnectionString);
    });
}
builder.Services.AddAuthentication(builder.Configuration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
//app.ApplyMigrations();
app.UseExceptionHandler();
app.MapDefaultEndpoints();
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
