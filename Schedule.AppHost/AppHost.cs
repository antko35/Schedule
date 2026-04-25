var builder = DistributedApplication.CreateBuilder(args);


var rabbit = builder.AddRabbitMQ("rabbitmq"); // Создаем сервер

// 3. MongoDB для ScheduleService + MongoExpress
var scheduleMongo = builder.AddMongoDB("schedule-service-mongo")
    .WithMongoExpress();
var scheduleDb = scheduleMongo.AddDatabase("scheduleDb");

// 4. MongoDB для UserManagement + MongoExpress
var managementMongo = builder.AddMongoDB("management-mongo")
    .WithMongoExpress();
var managementDb = managementMongo.AddDatabase("userManagementDb");

// 5. Keycloak
var keycloak = builder.AddKeycloak("keycloak")
    .WithDataVolume();

// --- СЕРВИСЫ ---

var scheduleService = builder.AddProject<Projects.ScheduleService_API>("schedule-service")
    .WithReference(scheduleDb)
    .WithReference(keycloak)
    .WithReference(rabbit)
    .WaitFor(rabbit)
    .WaitFor(scheduleDb);

var userService = builder.AddProject<Projects.UserManagementService_API>("user-management-service")
    .WithReference(managementDb)
    .WithReference(keycloak)
    .WithReference(rabbit)
    .WaitFor(rabbit)
    .WaitFor(managementDb);

builder.AddViteApp("vue-frontend", "../../front/electron-vue") // Путь к папке с package.json
    .WithReference(userService)      
    .WithReference(scheduleService)  
    .WithReference(keycloak)
    .WithHttpEndpoint(name:"ui", port: 5173, env: "PORT") // Стандартный порт Vite
    .WithExternalHttpEndpoints()     // Разрешаем доступ извне (браузера)
    .WithEnvironment("VITE_USER_API_URL", userService.GetEndpoint("http"))
    .WithEnvironment("VITE_SCHEDULE_API_URL", scheduleService.GetEndpoint("http"));

builder.Build().Run();