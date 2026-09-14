using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SO17.Application.Accounts;
using SO17.Application.Common;

namespace SO17.Infrastructure.Identity;

public sealed class AccountService(UserManager<AppUser> users, SignInManager<AppUser> signIn,
    IOptions<RegistrationOptions> registration) : IAccountService
{
    public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        Validation.Ensure(request);
        ct.ThrowIfCancellationRequested();
        var expected = registration.Value.InvitationCode;
        if (expected.Length < 24 || !CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(expected)),
                SHA256.HashData(Encoding.UTF8.GetBytes(request.InvitationCode))))
            throw new AppException(403, "Реєстрація недоступна або код запрошення неправильний.");
        var user = new AppUser
        {
            UserName = request.Login.Trim(),
            Email = request.Email.Trim(),
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            PhoneNumber = Validation.Phone(request.Phone),
            LockoutEnabled = true
        };
        try { Check(await users.CreateAsync(user, request.Password)); }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        { throw AppException.Conflict("Не вдалося зареєструвати акаунт. Перевірте логін та email."); }
        await signIn.SignInAsync(user, isPersistent: false);
        return Map(user);
    }

    public async Task<UserDto> LoginAsync(LoginRequest request)
    {
        Validation.Ensure(request);
        var result = await signIn.PasswordSignInAsync(request.Login.Trim(), request.Password,
            isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
            throw new AppException(401, "Не вдалося увійти. Перевірте дані або повторіть спробу пізніше.");
        return Map((await users.FindByNameAsync(request.Login.Trim()))!);
    }

    public Task LogoutAsync() => signIn.SignOutAsync();
    public async Task<UserDto> GetAsync(string userId) => Map(await FindAsync(userId));

    public async Task<UserDto> UpdateAsync(string userId, ProfileRequest request)
    {
        Validation.Ensure(request);
        var user = await FindAsync(userId);
        user.Name = request.Name.Trim();
        user.Surname = request.Surname.Trim();
        user.PhoneNumber = Validation.Phone(request.Phone);
        Check(await users.UpdateAsync(user));
        return Map(user);
    }

    public async Task ChangePasswordAsync(string userId, PasswordRequest request)
    {
        Validation.Ensure(request);
        var user = await FindAsync(userId);
        Check(await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword));
        await signIn.RefreshSignInAsync(user);
    }

    private async Task<AppUser> FindAsync(string id) => await users.FindByIdAsync(id) ?? throw new AppException(401, "Увійдіть до акаунта.");

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new AppException(400, "Не вдалося зберегти дані. Перевірте унікальність логіна й email, поточний пароль та вимоги до нового пароля.");
    }

    private static UserDto Map(AppUser user) => new(user.Id, user.UserName!, user.Email!, user.Name, user.Surname, user.PhoneNumber ?? "");
}
