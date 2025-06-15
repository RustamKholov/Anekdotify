var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache", 6379);

var apiService = builder.AddProject<Projects.Anekdotify_Api>("apiservice")
    .WithReference(cache);



builder.AddProject<Projects.Anekdotify_Frontend>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
