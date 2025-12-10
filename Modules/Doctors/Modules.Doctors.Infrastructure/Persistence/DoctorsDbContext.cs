using Microsoft.EntityFrameworkCore;
using Modules.Doctors.Core;
using Modules.Doctors.Core.Entities;

namespace Modules.Doctors.Infrastructure.Persistence;

public class DoctorsDbContext : DbContext
{
    public DoctorsDbContext(DbContextOptions<DoctorsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("doctors");
        
        builder.Entity<DoctorProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            //entity.Property(e => e.MedicalInstitutionLicense).IsRequired();
            //entity.Property(e => e.SpecializationId).IsRequired();
        });

        builder.Entity<DoctorAvailability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DoctorId).IsRequired();
        });

        builder.Entity<DoctorReview>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DoctorId).IsRequired();
            entity.Property(e => e.PatientId).IsRequired();
        });
        
        // builder.Entity<Specialization>(entity =>
        // {
        //     entity.HasKey(e => e.Id);
        //     entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        // });
    }
}