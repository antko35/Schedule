var builder = DistributedApplication.CreateBuilder(args);

var rabbit = builder.AddRabbitMQ("rabbitmq"); // Создаем сервер

// 3. MongoDB для ScheduleService + MongoExpress
var scheduleMongo = builder.AddMongoDB("schedule-service-mongo")
    .WithDataVolume()
    .WithMongoExpress();
var scheduleDb = scheduleMongo.AddDatabase("scheduleDb");

// 4. MongoDB для UserManagement + MongoExpress
var managementMongo = builder.AddMongoDB("management-mongo")
    .WithDataVolume()
    .WithMongoExpress();
var managementDb = managementMongo.AddDatabase("userManagementDb");

// 5. Keycloak
var keycloak = builder.AddKeycloak("keycloak")
    .WithDataVolume()
    .WithRealmImport("keycloak")
    .WithExternalHttpEndpoints();

// --- СЕРВИСЫ ---

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

// builder.AddViteApp("vue-frontend", "../Front/electron-vue") // Путь к папке с package.json
//     .WithReference(userService)
//     .WithReference(scheduleService)
//     .WithReference(keycloak)
//     .WithHttpEndpoint(name:"ui", port: 5173, env: "PORT") // Стандартный порт Vite
//     .WithExternalHttpEndpoints()     // Разрешаем доступ извне (браузера)
//     .WithEnvironment("VITE_USER_API_URL", userService.GetEndpoint("https"))
//     .WithEnvironment("VITE_SCHEDULE_API_URL", scheduleService.GetEndpoint("https"))
//     .WithEnvironment("VITE_KEYCLOAK_URL", keycloak.GetEndpoint("http"));

// builder.AddViteApp("vue-frontend-1", "../Front/electron-vue") // Путь к папке с package.json
//     .WithReference(userService)      
//     .WithReference(scheduleService)  
//     .WithReference(keycloak)
//     .WithHttpEndpoint(name:"NewUi", port: 5173, env: "PORT") // Стандартный порт Vite
//     .WithExternalHttpEndpoints()     // Разрешаем доступ извне (браузера)
//     //.WithHttpsDeveloperCertificate()           // ← важно
//     .WithDeveloperCertificateTrust(true)
//     .WithEnvironment("VITE_USER_API_URL", userService.GetEndpoint("https"))
//     .WithEnvironment("VITE_SCHEDULE_API_URL", scheduleService.GetEndpoint("https"));

builder.AddViteApp("angular-frontend", "../FrontendAngular")
    .WithReference(userService)
    .WithReference(scheduleService)
    .WithReference(keycloak)
    .WithHttpEndpoint(name: "angular-ui", port: 4200)
    .WithExternalHttpEndpoints();

builder.Build().Run();