using Microsoft.EntityFrameworkCore;
using Modules.AI.Core.Entities;

namespace Modules.AI.Infrastructure.Persistence;

public class AiDbContext : DbContext
{
    public AiDbContext(DbContextOptions<AiDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("ai");
        
        builder.Entity<AiChatSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Title).IsRequired();
        });

        builder.Entity<AiChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired();
            entity.Property(e => e.UserMessage).IsRequired();
            entity.Property(e => e.AiResponse).IsRequired();
        });
    }
}
