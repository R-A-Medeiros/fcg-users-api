using FIAP.Domain.Repositories;
using FIAP.Domain.Security.Cryptography;
using FIAP.Domain.Security.Tokens;
using FIAP.Domain.Services.LoggedUser;
using FIAP.Infrastructure.DataAccess.Repositories;
using FIAP.Infrastructure.Security.Tokens;
using FIAP.Infrastructure.Services.LoggedUser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.Infrastructure;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordEncripter, Infrastructure.Security.BCrypt>();
        services.AddScoped<ILoggedUser, LoggedUser>();

        AddToken(services, configuration);
        AddRepositories(services);
    }

    private static void AddToken(IServiceCollection services, IConfiguration configuration)
    {
        var expirationTimeMinutes = configuration.GetValue<uint>("Settings:Jwt:ExpiresMinutes");
        var signingKey = configuration.GetValue<string>("Settings:Jwt:SigningKey");
        var issuer = configuration.GetValue<string>("Settings:Jwt:Issuer");
        var audience = configuration.GetValue<string>("Settings:Jwt:Audience");

        services.AddScoped<IAccessTokenGenerator>(config =>
            new JwtTokenGenerator(expirationTimeMinutes, signingKey!, issuer, audience));
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserReadOnlyRepository, UserRepository>();
        services.AddScoped<IUserWriteOnlyRepository, UserRepository>();
    }
}
