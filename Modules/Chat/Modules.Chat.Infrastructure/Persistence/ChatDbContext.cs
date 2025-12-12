using Microsoft.EntityFrameworkCore;
using Modules.Chat.Core.Entities;

namespace Modules.Chat.Infrastructure.Persistence;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("chat");
        
        builder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AppointmentId).IsRequired();
            entity.Property(e => e.SenderId).IsRequired();
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
        });
    }
}
