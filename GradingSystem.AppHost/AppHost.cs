var builder = DistributedApplication.CreateBuilder(args);



builder.AddProject<Projects.GradingSystem_Gateway_Api>("gradingsystem-gateway-api");
builder.AddProject<Projects.GradingSystem_Services_Exam_Api>("gradingsystem-exam-api");
builder.AddProject<Projects.GradingSystem_Services_Submission_Api>("gradingsystem-submission-api");
builder.AddProject<Projects.GradingSystem_Services_Grading_Api>("gradingsystem-grading-api");


builder.Build().Run();
