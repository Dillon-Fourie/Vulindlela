
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Portal.Models.Auth;

namespace Vulindlela.Portal.Data;

public static class AuthSeeder
{
    public static async Task SeedAsync(
        AuthDbContext db,
        IPasswordHasher<ApplicationUser> passwordHasher)
    {
        var accounts = new[]
        {
            new
            {
                Username = "citizen1",
                Password = "Citizen1!Demo",
                Role = "Citizen",
                CitizenId = "TEST-CITIZEN-005"
            },
            new
            {
                Username = "citizen2",
                Password = "Citizen2!Demo",
                Role = "Citizen",
                CitizenId = "TEST-CITIZEN-006"
            },
            new
            {
                Username = "official1",
                Password = "Official1!Demo",
                Role = "Official",
                CitizenId = ""
            }
        };

        foreach (var account in accounts)
        {
            if (await db.Users.AnyAsync(u => u.Username == account.Username))
                continue;

            var user = new ApplicationUser
            {
                Username = account.Username,
                Role = account.Role,
                CitizenId = account.CitizenId
            };

            user.PasswordHash = passwordHasher.HashPassword(
                user,
                account.Password);

            db.Users.Add(user);
        }

        await db.SaveChangesAsync();
    }
}
