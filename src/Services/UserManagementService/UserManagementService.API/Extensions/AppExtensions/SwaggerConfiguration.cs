namespace UserManagementService.API.Extensions.AppExtensions
{
    public static class SwaggerConfiguration
    {
        public static void ConfigureSwagger(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = string.Empty;
                    options.OAuthClientId("schedule-service");
                    options.OAuthUsePkce();
                });
            }
        }
    }
}
