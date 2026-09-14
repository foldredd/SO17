using Microsoft.AspNetCore.Identity;
namespace SO17.Infrastructure.Identity;

public sealed class AppUser : IdentityUser
{
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
}
