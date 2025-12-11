using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Appointments.Core.Interfaces;
using Modules.Appointments.Infrastructure.Persistence;
using Modules.Appointments.Infrastructure.Services;

namespace Modules.Appointments.Infrastructure;

public static class AppointmentsModuleExtensions
{
    public static IServiceCollection AddAppointmentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppointmentsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Modules.Appointments.Infrastructure")));

        services.AddScoped<IAppointmentService, AppointmentService>();

        return services;
    }
}
