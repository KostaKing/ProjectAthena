using k8s.Models;

var builder = DistributedApplication.CreateBuilder(args);
var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var postgresVolume = postgres.WithDataVolume(isReadOnly: false);
var ProjectAthenaDB = postgres.AddDatabase("ProjectAthena");
var ngrokAuthToken = builder.AddParameter("ngrok-auth-token", "2zg42YIX4zF8cSEjmsq70MuxIzj_6FKWxijKnH7ZeGL5rUK6t", secret: true);
var dbService = builder.AddProject<Projects.ProjectAthena_DbWorkerService>("ProjectAthena-DbWorkerService").WithReference(ProjectAthenaDB).WaitFor(ProjectAthenaDB);


var weatherApi = builder.AddProject<Projects.ProjectAthena_MinimalApi>("weatherapi")
    .WithExternalHttpEndpoints()
    .WaitFor(dbService)
    .WithReference(ProjectAthenaDB);




var reactClient = builder.AddNpmApp("reactvite", "../AspireJavaScript.Vite")
    .WithReference(weatherApi)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "VITE_PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();



//builder.AddNgrok("ngrok")
//    .WithAuthToken(ngrokAuthToken)
//    // Add tunnel for API service (will get a random ngrok URL)
//    .WithTunnelEndpoint(weatherApi, "https", "smartfixpro.ngrok-free.app")
//    // Add tunnel for Client service (will get a separate random ngrok URL)  
//    .WithTunnelEndpoint(reactClient, "https", "smartfixproclient.ngrok-free.app");
builder.Build().Run();
