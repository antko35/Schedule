using Microsoft.AspNetCore.Mvc;
using UserManagementService.Application.GrpcClient;

namespace UserManagementService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
    };

    private readonly ILogger<WeatherForecastController> _logger;
    public readonly UserClient _userClient;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, UserClient userClient)
    {
        _logger = logger;
        _userClient = userClient;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpPost]
    [Route("AddClaim")]
    public async Task AddClaim(AddClaimRequest request)
    {
        await _userClient.AddClaimAsync(request);
    }

    [HttpPost]
    [Route("DeleteClaim")]
    public async Task DeleteClaim(AddClaimRequest request)
    {
        await _userClient.DeleteClaimAsync(request);
    }
}
