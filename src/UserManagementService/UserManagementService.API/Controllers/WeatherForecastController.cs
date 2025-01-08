using Microsoft.AspNetCore.Mvc;
using UserManagementService.API.GrpcClient.Services;

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

    [HttpGet]
    [Route("GetClaims/{email}")]
    public async Task<IActionResult> GetClaims([FromRoute] string email)
    {
        var response = await _userClient.GetUserClaimsAsync(email);
        return Ok(response);
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
