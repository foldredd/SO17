using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SO17.Application.Accounts;
namespace SO17.Api.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(IAccountService accounts, IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet("csrf"), AllowAnonymous]
    public IActionResult Csrf() => Ok(new { token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });

    [HttpPost("register"), AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<ActionResult<UserDto>> Register(RegisterRequest request, CancellationToken ct) =>
        Ok(await accounts.RegisterAsync(request, ct));

    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<ActionResult<UserDto>> Login(LoginRequest request) => Ok(await accounts.LoginAsync(request));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout() { await accounts.LogoutAsync(); return NoContent(); }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me() => Ok(await accounts.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!));

    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> Profile(ProfileRequest request) =>
        Ok(await accounts.UpdateAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, request));

    [HttpPut("password"), EnableRateLimiting("auth")]
    public async Task<IActionResult> Password(PasswordRequest request)
    {
        await accounts.ChangePasswordAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, request);
        return NoContent();
    }
}
