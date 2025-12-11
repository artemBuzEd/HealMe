using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Doctors.Core.DTOs;
using Modules.Doctors.Core.Interfaces;
using Modules.Doctors.Core.Validation;
using Modules.Doctors.Infrastructure.Persistence;
using Modules.Doctors.Infrastructure.Services;

namespace Modules.Doctors.Infrastructure;

public static class DoctorModuleExtensions
{
    public static IServiceCollection AddDoctorModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DoctorsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Modules.Doctors.Infrastructure")));

        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IValidator<CreateAvailabilityRequest>, CreateAvailabilityRequestValidator>();
        services.AddScoped<IValidator<UpdateDoctorProfileRequest>, UpdateDoctorProfileRequestValidator>();
        
        
        
        return services;
    }
}
