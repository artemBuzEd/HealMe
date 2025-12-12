using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Chat.Core.Interfaces;
using Modules.Chat.Infrastructure.Persistence;
using Modules.Chat.Infrastructure.Services;

namespace Modules.Chat.Infrastructure;

public static class ChatModuleExtensions
{
    public static IServiceCollection AddChatModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ChatDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Modules.Chat.Infrastructure")));

        services.AddScoped<IChatService, ChatService>();

        return services;
    }
}
