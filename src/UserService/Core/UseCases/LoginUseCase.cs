//using Microsoft.AspNetCore.Identity;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UserService.Application.DTOs;
//using UserService.Application.JWTService;

//namespace UserService.Application.UseCases
//{
//    public class LoginUseCase
//    {
//        private readonly SignInManager<IdentityUser> _signInManager;
//        private readonly UserManager<IdentityUser> _userManager;
//        private readonly JWTGenerator _jwtGenerator;
//        public LoginUseCase(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, JWTGenerator jWTGenerator)
//        {
//            _signInManager = signInManager;
//            _userManager = userManager;
//            _jwtGenerator = jWTGenerator;
//        }
//        public async Task<string> Execute(CustomLoginRequest request)
//        {

//            IdentityUser? user = await _userManager.FindByEmailAsync(request.Email);
//            if (user == null) {
//                throw new Exception("user doesnt exist");
//            }
//            var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe,false);
//            if (!result.Succeeded)
//            {
//                throw new Exception("Login faild");
//            }

//            //TODO если все плохо исключение
//            var token = await _jwtGenerator.GenerateToken(user);

//            return token;
//        }
//    }
//}
