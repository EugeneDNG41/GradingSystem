var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithHostPort(5432)
    .WithBindMount("postgres-data", "/var/lib/postgresql/data").WithPgAdmin(pgAdmin =>
    {
        pgAdmin.WithHostPort(5050);
    })
    .WithLifetime(ContainerLifetime.Persistent);
var userDb = postgres.AddDatabase("users-db");
var examDb = postgres.AddDatabase("exams-db");
var submissionDb = postgres.AddDatabase("submissions-db");

var rabbitMq = builder.AddRabbitMQ("rabbitmq");

var blobs = builder.AddAzureStorage("storage").RunAsEmulator(azurite => azurite.WithDataVolume()).AddBlobs("blobs");

var examApi = builder.AddProject<Projects.GradingSystem_Services_Exams_Api>("gradingsystem-exams-api")
    .WithReference(examDb)
    .WaitFor(examDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq);
var submissionApi = builder.AddProject<Projects.GradingSystem_Services_Submissions_Api>("gradingsystem-submissions-api")
    .WithReference(submissionDb)
    .WaitFor(submissionDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq);
var userApi = builder.AddProject<Projects.GradingSystem_Services_Users_Api>("gradingsystem-users-api")
    .WithReference(userDb)
    .WaitFor(userDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq);

var gateway = builder.AddProject<Projects.GradingSystem_Gateway_Api>("gradingsystem-gateway-api")
    .WithReference(userApi)
    .WithReference(examApi)
    .WithReference(submissionApi);

builder.Build().Run();
