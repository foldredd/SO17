using ApiServiceMVC.JWT;
using ApiServiceMVC.Models;
using ApiServiceMVC.Web.Api.Infrastructure.EncryptAndDecrypt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApiServiceMVC.DatabaseEnty;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ApiServiceMVC.Helpers;
using ApiServiceMVC.Helpers;
using ApiServiceMVC.DatabaseEnty.Service;

namespace ApiServiceMVC.Controllers
{
    public class AuthController : Controller {

        private readonly ILogger<AuthController> _logger;
        private DatabaseService databaseService;
        public AuthController(ILogger<AuthController> logger, Database database,DatabaseService databaseService) {
            _logger = logger;
           this.databaseService = databaseService;
            database.Database.EnsureCreated();
        }
        public IActionResult Index() {
            Console.WriteLine("Hello World!");
            return Json("server is up and running");
        }
        [HttpPost("/registration")]
        public IActionResult Registration([FromBody]  BaseUser data) {
            if (!data.IsFull())
            {
                _logger.LogError("User was`t full fields");
                return BadRequest("User was`t full fields");
               
            }
            AuthorizationData authorizationData;
           
            authorizationData = Encrypt.EncryptPassword(data.Password);

            if (databaseService.RegistrationUser(data,authorizationData) == true)
            {
                _logger.LogInformation("User add");
                return Json(authorizationData);
            }
           
            return BadRequest();
        }

        [HttpPost("/login")]
        public IActionResult Login([FromBody] BaseUser client) {
            User user=databaseService.AuthorizationUser(client);
 
               if(user==null){
                _logger.LogError("User not found");  
                return BadRequest();
               }


            string passwordHash=Encoding.UTF8.GetString(user.PasswordHash);
            string salt = Encoding.UTF8.GetString(user.PasswordSalt);
             bool verification = Decryption.VerifyPassword(salt+client.Password, passwordHash);
          

            if (verification == true)
                {
                    _logger.LogInformation("Verification result: {0}", verification);
                string encodedJwt = JWTService.NewToken(user.Login);
                    var response = new
                    {
                         email = user.Email,
                        access_token = encodedJwt
                    };
                    return Json(response);
                }
               
                    _logger.LogError($"Verification result {verification}");
                    return Json(false);
               
            
        }

      
    }
}
