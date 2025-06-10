var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Anekdotify_Api>("apiservice");

builder.AddProject<Projects.Anekdotify_Frontend>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
