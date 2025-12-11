using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.AI.Core.Interfaces;
using Modules.AI.Infrastructure.Persistence;
using Modules.AI.Infrastructure.Services;

namespace Modules.AI.Infrastructure;

public static class AiModuleExtensions
{
    public static IServiceCollection AddAiModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AiDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Modules.AI.Infrastructure")));

        services.AddHttpClient<IAiService, AiService>();

        return services;
    }
}
