using ApiServiceMVC.DatabaseEnty;
using ApiServiceMVC.Helpers;
using ApiServiceMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ApiServiceMVC.DatabaseEnty.Service;

namespace ApiServiceMVC.Controllers {
    public class ProfileController : Controller {
        private readonly ILogger<ProfileController> _logger;
        private DatabaseService databaseService;
       
        public ProfileController(ILogger<ProfileController> logger,DatabaseService databaseService) {
            _logger = logger;
          this.databaseService = databaseService;
        }

        [HttpGet]
        [Route("profile/{login}")]
        [Authorize]
        public IActionResult Profile( string login) {
            _logger.LogInformation($"Profile?login={login}");
            var emailClaim = User.FindFirstValue(ClaimTypes.Email);
            if(emailClaim != null) {
                if(emailClaim== login)
                {
                    User user=databaseService.GetProfile(login);
                   
                    
                    var response = new
                    {
                        name = user.Name,
                        surname = user.SurName,
                        email = user.Email,
                        phone = user.Phone,
                    };
                    _logger.LogTrace("Good Profile ");
                    return Json(response);
                }
            }
            _logger.LogError("Error Profile");
            return BadRequest();
        }
    }
}
