var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithHostPort(5432)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var userDb = postgres.AddDatabase("users-db");
var examDb = postgres.AddDatabase("exams-db");
var submissionDb = postgres.AddDatabase("submissions-db");

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var blobs = builder.AddAzureStorage("storage").RunAsEmulator(azurite => azurite.WithDataVolume()).AddBlobs("blobs");

var examApi = builder.AddProject<Projects.GradingSystem_Services_Exams_Api>("gradingsystem-exam-api").WithReference(examDb);
var submissionApi = builder.AddProject<Projects.GradingSystem_Services_Submissions_Api>("gradingsystem-submission-api").WithReference(submissionDb);
var userApi = builder.AddProject<Projects.GradingSystem_Services_Users_Api>("gradingsystem-grading-api").WithReference(userDb);

var gateway = builder.AddProject<Projects.GradingSystem_Gateway_Api>("gradingsystem-gateway-api")
    .WithReference(userApi)
    .WithReference(examApi)
    .WithReference(submissionApi);

builder.Build().Run();
