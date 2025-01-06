using Grpc.Net.Client;
using UserManagementService.API.Extensions;
using UserManagementService.Application.GrpcClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback =
    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

builder.Services.AddSingleton<UserClient>(sp =>
{
    var channel = GrpcChannel.ForAddress(
        "https://user-service:8080",
        new GrpcChannelOptions { HttpHandler = handler });
    return new UserClient(new UserGrpcService.UserGrpcServiceClient(channel));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExeptionHadlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
