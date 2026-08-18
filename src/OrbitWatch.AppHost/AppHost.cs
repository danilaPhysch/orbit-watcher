var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.OrbitWatcher>("api");

builder.AddProject<Projects.OrbitWatcher_Client>("client")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
