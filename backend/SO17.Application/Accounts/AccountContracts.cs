using System.ComponentModel.DataAnnotations;

namespace SO17.Application.Accounts;

public sealed record UserDto(string Id, string Login, string Email, string Name, string Surname, string Phone);
public sealed class LoginRequest
{
    [Required, StringLength(80)] public string Login { get; init; } = "";
    [Required, StringLength(128)] public string Password { get; init; } = "";
}
public sealed class RegisterRequest
{
    [Required, StringLength(80, MinimumLength = 3), RegularExpression(@"[a-zA-Z0-9._\-]+")]
    public string Login { get; init; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; init; } = "";
    [Required, StringLength(128, MinimumLength = 12)] public string Password { get; init; } = "";
    [Required, StringLength(80)] public string Name { get; init; } = "";
    [Required, StringLength(80)] public string Surname { get; init; } = "";
    [Required, StringLength(30)] public string Phone { get; init; } = "";
    [Required, StringLength(256)] public string InvitationCode { get; init; } = "";
}
public sealed class ProfileRequest
{
    [Required, StringLength(80)] public string Name { get; init; } = "";
    [Required, StringLength(80)] public string Surname { get; init; } = "";
    [Required, StringLength(30)] public string Phone { get; init; } = "";
}
public sealed class PasswordRequest
{
    [Required, StringLength(128)] public string CurrentPassword { get; init; } = "";
    [Required, StringLength(128, MinimumLength = 12)] public string NewPassword { get; init; } = "";
}

public interface IAccountService
{
    Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<UserDto> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<UserDto> GetAsync(string userId);
    Task<UserDto> UpdateAsync(string userId, ProfileRequest request);
    Task ChangePasswordAsync(string userId, PasswordRequest request);
}
