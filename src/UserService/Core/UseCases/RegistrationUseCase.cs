using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Domain.Models;

namespace UserService.Application.UseCases
{
    public class RegistrationUseCase
    {
        private readonly UserManager<IdentityUser> _userManager;
        public RegistrationUseCase(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IdentityResult> Execute(CustomRegisterRequest register)
        {
            if(register.Password != register.Password)
            {
                throw new Exception("passwords don’t match");
            }

            var user = new IdentityUser { 
                UserName = register.Username,
                Email = register.Email,
            };
            //TODO проверка есть ли уже такая почта
            // TODO exception handling middle
            var result = await _userManager.CreateAsync(user, register.Password);
            if (result.Succeeded) {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return result;
        }
    }
}
