using Microsoft.AspNetCore.Identity;
using UserService.API.Extensions;
using UserService.Domain.Models;
using UserService.Infrastructure;
using UserService.Infrastructure.Extensions;
using UserService.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin();
        builder.AllowAnyMethod();
        builder.AllowAnyHeader();
    });
});

// builder.Services.AddAuthentication(options =>
// {
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// }).AddJwtBearer(options =>
// {
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//    };
// })
// .AddCookie(options =>
// {
//    options.LoginPath = "/login"; // Путь для перенаправления при неаутентифицированном доступе
//    options.Cookie.Name = "authCookie"; // Имя куки
//    options.Cookie.HttpOnly = true; // Защита от доступа через JavaScript
//    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Использование только по HTTPS
// });;
builder.AddPresentation();

// builder.Services.AddIdentityApiEndpoints<User>()
//    .AddRoles<IdentityRole>()
//    .AddEntityFrameworkStores<UserDbContext>();
builder.Services.AddInfrastructure(builder.Configuration);

// builder.Services.AddScoped<RegistrationUseCase>();
// builder.Services.AddScoped<LoginUseCase>();
// builder.Services.AddScoped<LogOutUseCase>();
// builder.Services.AddScoped<JWTGenerator>();
var app = builder.Build();

app.UseMiddleware<ExeptionHadlingMiddleware>();

var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<IUserSeeder>();
await seeder.Seed();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });

    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseCors();

app.MapGroup("userService").MapIdentityApi<User>();

app.UseAuthorization();

app.MapControllers();

app.Run();
