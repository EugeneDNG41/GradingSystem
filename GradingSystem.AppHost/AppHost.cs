var builder = DistributedApplication.CreateBuilder(args);
var postgresUsername = builder.AddParameter("postgresusername", secret: true);
var postgresPassword = builder.AddParameter("postgrespassword", secret: true);
var postgres = builder.AddPostgres("postgres", postgresUsername, postgresPassword)
    .WithHostPort(5432)
    .WithDataVolume().WithPgAdmin(pgAdmin =>
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
var storage = builder.AddAzureStorage("storage")
                     .RunAsEmulator(az => az.WithDataVolume());
var blobContainer = storage.AddBlobs("blobs");

var examService = builder.AddProject<Projects.GradingSystem_Services_Exams_Api>("exams-service")
    .WithReference(examDb)
    .WaitFor(examDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);
var submissionService = builder.AddProject<Projects.GradingSystem_Services_Submissions_Api>("submissions-service")
    .WithReference(submissionDb)
    .WaitFor(submissionDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(blobContainer); 

var userService = builder.AddProject<Projects.GradingSystem_Services_Users_Api>("users-service")
    .WithReference(userDb)
    .WaitFor(userDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
      .WithReference(blobContainer);

var gateway = builder.AddYarp("gateway")
    .WithHostPort(8080)
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/exams/api/{**catch-all}", examService);
        yarp.AddRoute("/submissions/api/{**catch-all}", submissionService);
        yarp.AddRoute("/users/api/{**catch-all}", userService);
    });

builder.Build().Run();
