var builder = DistributedApplication.CreateBuilder(args);

var rabbit = builder.AddRabbitMQ("rabbitmq");

var scheduleMongo = builder.AddMongoDB("schedule-service-mongo")
    .WithDataVolume()
    .WithMongoExpress();
var scheduleDb = scheduleMongo.AddDatabase("scheduleDb");

var managementMongo = builder.AddMongoDB("management-mongo")
    .WithDataVolume()
    .WithMongoExpress();
var managementDb = managementMongo.AddDatabase("userManagementDb");

var keycloak = builder.AddKeycloak("keycloak")
    .WithDataVolume()
    .WithRealmImport("keycloak")
    .WithExternalHttpEndpoints();

var scheduleService = builder.AddProject<Projects.ScheduleService_API>("schedule-service")
    .WithReference(scheduleDb)
    .WithReference(keycloak)
    .WithReference(rabbit)
    .WaitFor(rabbit)
    .WaitFor(scheduleDb)
    .WaitFor(keycloak);

var userService = builder.AddProject<Projects.UserManagementService_API>("user-management-service")
    .WithReference(managementDb)
    .WithReference(keycloak)
    .WithReference(rabbit)
    .WaitFor(rabbit)
    .WaitFor(managementDb)
    .WaitFor(keycloak);

builder.AddViteApp("angular-frontend", "../../apps/web")
    .WithReference(userService)
    .WithReference(scheduleService)
    .WithReference(keycloak)
    .WithHttpEndpoint(name: "angular-ui", port: 4200)
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
