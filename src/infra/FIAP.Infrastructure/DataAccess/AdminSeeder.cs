using FIAP.Domain.Entities;
using FIAP.Domain.Enums;
using FIAP.Domain.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FIAP.Infrastructure.DataAccess;

// Seeds an initial administrator so there is a way to bootstrap authorization.
// Idempotent: does nothing if an administrator already exists.
// Credentials come from configuration (Settings:Seed:Admin*), never hardcoded.
public static class AdminSeeder
{
    public static async Task SeedAdminAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var db = provider.GetRequiredService<UsersDbContext>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        var encripter = provider.GetRequiredService<IPasswordEncripter>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

        var email = configuration.GetValue<string>("Settings:Seed:AdminEmail");
        var password = configuration.GetValue<string>("Settings:Seed:AdminPassword");
        var name = configuration.GetValue<string>("Settings:Seed:AdminName") ?? "Administrator";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("Admin seed skipped: Settings:Seed:AdminEmail/AdminPassword not configured.");
            return;
        }

        var adminExists = await db.Users.AnyAsync(u => u.Role == Roles.ADMIN);
        if (adminExists)
        {
            logger.LogInformation("Admin seed skipped: an administrator already exists.");
            return;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var emailInUse = await db.Users.AnyAsync(u => u.Email == normalizedEmail);
        if (emailInUse)
        {
            logger.LogWarning("Admin seed skipped: email {Email} already in use by a non-admin user.", normalizedEmail);
            return;
        }

        db.Users.Add(new User
        {
            Name = name,
            Email = normalizedEmail,
            Password = encripter.Encrypt(password),
            UserIdentifier = Guid.NewGuid(),
            Role = Roles.ADMIN,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        logger.LogInformation("Admin seed: administrator {Email} created.", normalizedEmail);
    }
}
