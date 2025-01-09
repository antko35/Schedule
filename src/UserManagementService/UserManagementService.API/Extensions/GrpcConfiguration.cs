using Grpc.Net.Client;
using UserManagementService.API.GrpcClient.Services;

namespace UserManagementService.API.Extensions
{
    public static class GrpcConfiguration
    {
        public static void ConfigureGrpc(
            this WebApplicationBuilder builder)
        {
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
        }
    }
}
