var builder = DistributedApplication.CreateBuilder(args);

var weatherApi = builder.AddProject<Projects.ProjectAthena_MinimalApi>("weatherapi")
    .WithExternalHttpEndpoints();


builder.AddNpmApp("reactvite", "../AspireJavaScript.Vite")
    .WithReference(weatherApi)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "VITE_PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
