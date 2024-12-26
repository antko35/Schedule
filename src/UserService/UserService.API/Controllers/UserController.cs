using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.UseCases;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly RegistrationUseCase _registrationUseCase;
        private readonly LoginUseCase _loginUseCase;
        private readonly LogOutUseCase _logoutUseCase;
        public UserController(RegistrationUseCase registrationUseCase, LoginUseCase loginUseCase, LogOutUseCase logOutUseCase)
        {
           _registrationUseCase = registrationUseCase;
            _loginUseCase = loginUseCase;
            _logoutUseCase = logOutUseCase;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Registration([FromBody] CustomRegisterRequest request)
        {
            var result = await _registrationUseCase.Execute(request);

            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(result);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] CustomLoginRequest request)
        {
            var result = await _loginUseCase.Execute(request);
            //if (!result.Succeeded) return Unauthorized("Invalid login attempt");
            HttpContext.Response.Cookies.Append("authToken", result,
       new CookieOptions
       {
           HttpOnly = true,
           Secure = true,
           SameSite = SameSiteMode.None,
           Expires = DateTimeOffset.UtcNow.AddHours(1) // Время жизни куки
       });
            return Ok(new { Message = "Login successful" });
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> LogOut()
        {
            await _logoutUseCase.Execute();
            return Ok();
        }
    }
}
