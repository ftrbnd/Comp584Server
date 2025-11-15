using Comp584Server.Data.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using WorldModel;

namespace Comp584Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(UserManager<WorldModelUser> userManager, JwtHandler jwtHandler) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            WorldModelUser? user = await userManager.FindByNameAsync(request.Username);
            if (user is null)
            {
                return Unauthorized("Invalid username");
            }

            bool passwordIsValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordIsValid)
            {
                return Unauthorized("Invalid password");
            }

            JwtSecurityToken token = await jwtHandler.GenerateTokenAsync(user);
            string serializedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new LoginResponse
            {
                Token = serializedToken,
                Success = true,
                Message = "Login successful"
            });
        }
    }
}
