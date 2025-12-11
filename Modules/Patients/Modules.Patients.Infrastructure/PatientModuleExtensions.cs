using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Patients.Core.Interfaces;
using Modules.Patients.Infrastructure.Persistence;
using Modules.Patients.Infrastructure.Services;

namespace Modules.Patients.Infrastructure;

public static class PatientModuleExtensions
{
    public static IServiceCollection AddPatientModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PatientsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Modules.Patients.Infrastructure")));

        services.AddScoped<IPatientService, PatientService>();

        return services;
    }
}
