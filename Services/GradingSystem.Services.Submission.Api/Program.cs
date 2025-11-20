using GradingSystem.Services.Submissions.Api;
using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Options;
using GradingSystem.Services.Submissions.Api.Services;
using GradingSystem.Services.Submissions.Api.Services.BlobStorage;
using GradingSystem.Shared.Contracts;
using Microsoft.AspNetCore.Http.Features;
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.AddAzureBlobServiceClient("blobs");

builder.Services.AddBlobService();
builder.Services.Configure<SubmissionValidationOptions>(builder.Configuration.GetSection("SubmissionValidation"));

var submissionsDbConnectionString = builder.Configuration.GetConnectionString("submissions-db");
builder.Services.AddDbContext<SubmissionsDbContext>(options => options.UseNpgsql(submissionsDbConnectionString));

var rabbitmqEndpoint = builder.Configuration.GetConnectionString("rabbitmq");
if (rabbitmqEndpoint != null && submissionsDbConnectionString != null)
{
    builder.Host.UseWolverine(opts =>
    {
        //opts.PublishAllMessages().ToRabbitExchange("grading-system", exchange =>
        //{
        //    exchange.ExchangeType = ExchangeType.Direct;
        //});

        opts.PublishMessage<SubmissionViolationsDetected>().ToRabbitQueue("violations-service");

        opts.UseRabbitMq(new Uri(rabbitmqEndpoint)).AutoProvision().EnableWolverineControlQueues();
        opts.ListenToRabbitQueue("submissions-service");
        opts.UseEntityFrameworkCoreTransactions();
        opts.PersistMessagesWithPostgresql(submissionsDbConnectionString);
    });
}
builder.Services.AddScoped<ISubmissionIngestionService, SubmissionIngestionService>();
builder.Services.AddScoped<ISubmissionValidationService, SubmissionValidationService>();
builder.Services.AddScoped<ISubmissionAssetService, SubmissionAssetService>();
builder.Services.AddScoped<ISubmissionFileService, SubmissionFileService>();
builder.Services.AddScoped<ISubmissionUploadService, SubmissionUploadService>();
builder.Services.AddScoped<ISubmissionSolutionService, SubmissionSolutionService>();
builder.Services.AddAuthentication(builder.Configuration);
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 500;
});

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 1024L * 1024L * 500L;
});
builder.Services.AddScoped<IGradeEntryService, GradeEntryService>();
builder.Services.AddScoped<ISubmissionBatchService, SubmissionBatchService>();
builder.Services.AddScoped<IStudentSubmissionService, StudentSubmissionService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.ApplyMigrations();
app.UseExceptionHandler();
app.MapDefaultEndpoints();
// Configure the HTTP request pipeline.

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
