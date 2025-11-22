using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Core.DTOs;

namespace Modules.Identity.Infrastructure;

public static class IdentityModuleExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IValidator<ForgotPasswordRequest>, ForgotPasswordRequestValidation>();
        services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordRequestValidation>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidation>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidation>();
        
        return services;
    }
}