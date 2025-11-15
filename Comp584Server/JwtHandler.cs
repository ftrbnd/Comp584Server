using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WorldModel;

namespace Comp584Server
{
    public class JwtHandler(UserManager<WorldModelUser> userManager, IConfiguration configuration)
    {
        public async Task<JwtSecurityToken> GenerateTokenAsync(WorldModelUser user)
        {
            JwtSecurityToken token = new
            (
                issuer: configuration["JwtSettings:Issuer"]!,
                audience: configuration["JwtSettings:Audience"]!,
                expires: DateTime.Now.AddMinutes(Convert.ToInt16(configuration["JwtSettings:ExpiryMinutes"]!)),
                signingCredentials: GetSigningCredentials(),
                claims: await GetClaimsAsync(user)
            );

            return token;
        }

        private SigningCredentials GetSigningCredentials()
        {
            byte[] key = Convert.FromBase64String(configuration["JwtSettings:SecretKey"]!);
            SymmetricSecurityKey secret = new(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaimsAsync(WorldModelUser user)
        {
            List<Claim> claims = [new Claim(ClaimTypes.Name, user.UserName!)];
            foreach (var role in await userManager.GetRolesAsync(user))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
    }
}
