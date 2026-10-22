using FIAP.Application.AutoMapper;
using FIAP.Application.UseCases.Login.DoLogin;
using FIAP.Application.UseCases.User.ChangeRole;
using FIAP.Application.UseCases.User.Register;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.Application;

public static class DependencyInjectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
        AddAutoMapper(services);
        AddUseCases(services);
    }

    private static void AddAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(config =>
        {
            config.AddProfile<AutoMapping>();
        });
    }

    private static void AddUseCases(IServiceCollection services)
    {
        services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
        services.AddScoped<IDoLoginUseCase, DoLoginUseCase>();
        services.AddScoped<IChangeUserRoleUseCase, ChangeUserRoleUseCase>();
    }
}
