using Microsoft.EntityFrameworkCore;
using Modules.Patients.Core.Entities;

namespace Modules.Patients.Infrastructure.Persistence;

public class PatientsDbContext : DbContext
{
    public PatientsDbContext(DbContextOptions<PatientsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("patients");
        
        builder.Entity<PatientProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
            entity.Property(e => e.Email).IsRequired();
        });
    }
}
