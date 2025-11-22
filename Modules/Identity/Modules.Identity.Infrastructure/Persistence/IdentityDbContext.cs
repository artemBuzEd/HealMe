using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Core.Entities;

namespace Modules.Identity.Infrastructure.Persistence;

public class IdentityDbContext : IdentityDbContext<User, Role, string>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.HasDefaultSchema("identity");

        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
        });

        builder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
        });
    }
}
