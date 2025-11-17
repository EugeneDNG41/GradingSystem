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

var username = builder.AddParameter("username", secret: true);
var password = builder.AddParameter("password", secret: true);

var rabbitmq = builder.AddRabbitMQ("rabbitmq", username, password)
                      .WithManagementPlugin();

var blobs = builder.AddAzureStorage("storage").RunAsEmulator(azurite => azurite.WithDataVolume()).AddBlobs("blobs");

var examService = builder.AddProject<Projects.GradingSystem_Services_Exams_Api>("exams-service")
    .WithReference(examDb)
    .WaitFor(examDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);
var submissionService = builder.AddProject<Projects.GradingSystem_Services_Submissions_Api>("submissions-service")
    .WithReference(submissionDb)
    .WaitFor(submissionDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);
var userService = builder.AddProject<Projects.GradingSystem_Services_Users_Api>("users-service")
    .WithReference(userDb)
    .WaitFor(userDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

var gateway = builder.AddYarp("gateway")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute(examService);
        yarp.AddRoute(submissionService);
        yarp.AddRoute(userService);
    });

builder.Build().Run();
