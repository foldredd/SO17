using ApiServiceMVC.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiServiceMVC.JWT {
    public class JWTService {

        public static string NewToken(string login) {
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, login) };
            // Create JWT-token
            var jwt = new JwtSecurityToken(
                    issuer: JWTConfig.ISSUER,
                    audience: JWTConfig.AUDIENCE,
            claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                    signingCredentials: new SigningCredentials(JWTConfig.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }
    }
}
