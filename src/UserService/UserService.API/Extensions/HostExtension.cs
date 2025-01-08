namespace UserService.API.Extensions
{
    public static class HostExtension
    {
        public static void AddHost(this WebApplicationBuilder builder)
        {
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8080, opt =>
                {
                    var certPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
                    var certPassword = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");

                    if (!string.IsNullOrEmpty(certPath) && !string.IsNullOrEmpty(certPassword))
                    {
                        opt.UseHttps(certPath, certPassword);
                    }
                });
            });
        }
    }
}
