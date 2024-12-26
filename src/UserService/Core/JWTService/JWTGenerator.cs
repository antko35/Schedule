using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Application.JWTService
{
    public class JWTGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        public JWTGenerator(IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<string> GenerateToken(IdentityUser user)
        {
            var roles1 = await _userManager.GetRolesAsync(user);
            var roles = roles1.First().ToString();

            //TODO если не одна роль передавать список
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("Role", roles)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
