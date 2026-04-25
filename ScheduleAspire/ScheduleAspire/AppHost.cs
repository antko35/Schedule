var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithDataVolume("keycloak");

var scheduleService = builder.AddProject<Projects.>()

builder.Build().Run();