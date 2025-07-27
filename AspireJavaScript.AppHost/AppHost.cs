using k8s.Models;

var builder = DistributedApplication.CreateBuilder(args);
var username = builder.AddParameter("postgres-username", "postgres");
var password = builder.AddParameter("postgres-password", "mypassword123");
var postgres = builder.AddPostgres("postgres", username, password)
    .WithDataVolume(isReadOnly: false)
    .WithPgAdmin();
var ProjectAthenaDB = postgres.AddDatabase("ProjectAthenaDB");
var ngrokAuthToken = builder.AddParameter("ngrok-auth-token", "2zgYIX4zF8cSEjmsq70MuxIzj_6FWxijKnH7ZeGL5rUK6t", secret: true);
var dbService = builder.AddProject<Projects.ProjectAthena_DbWorkerService>("ProjectAthena-DbWorkerService").WithReference(ProjectAthenaDB).WaitFor(ProjectAthenaDB);

var projectAthenaApi = builder.AddProject<Projects.ProjectAthena_MinimalApi>("ProjectAthenaApi")
    .WithExternalHttpEndpoints()
    .WaitForCompletion(dbService)
    .WithReference(ProjectAthenaDB);




var reactClient = builder.AddNpmApp("reactvite", "../AspireJavaScript.Vite")
    .WithReference(projectAthenaApi)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "VITE_PORT")
    .WithExternalHttpEndpoints()
    .WaitFor(projectAthenaApi)
    .PublishAsDockerFile();

builder.Build().Run();
