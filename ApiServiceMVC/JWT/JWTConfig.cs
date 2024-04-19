using System.Text;
using Microsoft.IdentityModel.Tokens;
namespace ApiServiceMVC.JWT {
    
    public class JWTConfig {
        public const string ISSUER = "ApiServiceMVC"; // token publisher
        public const string AUDIENCE = "ApiServiceMVC"; // token consumer
        const string KEY = "your_security_key_with_at_least_32_characters";   // KEY encrypt
        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
