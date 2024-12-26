using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using UserService.API.Controllers;
using UserService.Application.JWTService;
using UserService.Application.UseCases;
using UserService.Domain.Models;
using UserService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => {
        builder.AllowAnyOrigin();
        builder.AllowAnyMethod();
        builder.AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
       // Role = new ClaimsIdentity(),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
})
.AddCookie(options =>
{
    options.LoginPath = "/login"; // Путь для перенаправления при неаутентифицированном доступе
    options.Cookie.Name = "authCookie"; // Имя куки
    options.Cookie.HttpOnly = true; // Защита от доступа через JavaScript
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Использование только по HTTPS
}); ;


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("postgresConnection");
builder.Services.AddEntityFrameworkNpgsql().AddDbContext<UserDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>();

builder.Services.AddAuthorization();


builder.Services.AddScoped<RegistrationUseCase>();
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<LogOutUseCase>();
builder.Services.AddScoped<JWTGenerator>();
var app = builder.Build();

app.MapIdentityApi<IdentityUser>();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });

    var application = app.Services.CreateScope().ServiceProvider.GetRequiredService<UserDbContext>();

    //var pendingMigrations = await application.Database.GetPendingMigrationsAsync();
    //if (pendingMigrations != null)
    //    await application.Database.MigrateAsync();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] { "Admin","User"};

    foreach (var role in roles)
    {
        if(!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

    app.Run();
