using UserService.Domain.Models;

namespace UserService.API.Extensions;

public static class IdentityEndpoints
{
    public static void ConfigureEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        /*var identityGroup = routeBuilder
            .MapGroup("userService");

        identityGroup.MapIdentityApi<User>();

        identityGroup.MapGet("/register", context =>
        {
            
        } );*/
    }
}