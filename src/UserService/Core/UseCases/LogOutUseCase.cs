//using Microsoft.AspNetCore.Identity;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace UserService.Application.UseCases
//{
//    public class LogOutUseCase
//    {
//        private readonly SignInManager<IdentityUser> _signInManager;
//        public LogOutUseCase(SignInManager<IdentityUser> signInManager)
//        {
//            _signInManager = signInManager;
//        }
//        public async Task Execute()
//        {
//            await _signInManager.SignOutAsync();
//        }
//    }
//}
