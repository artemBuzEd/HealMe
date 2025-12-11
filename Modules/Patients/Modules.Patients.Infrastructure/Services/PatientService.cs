using Microsoft.EntityFrameworkCore;
using Modules.Identity.Core.Entities.Enums;
using Modules.Patients.Core.DTOs;
using Modules.Patients.Core.Entities;
using Modules.Patients.Core.Interfaces;
using Modules.Patients.Infrastructure.Persistence;

namespace Modules.Patients.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly PatientsDbContext _dbContext;

    public PatientService(PatientsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PatientProfileDto?> GetProfileAsync(string userId)
    {
        var profile = await _dbContext.Set<PatientProfile>()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null) return null;

        return MapToDto(profile);
    }

    public async Task<PatientProfileDto> UpdateProfileAsync(string userId, UpdatePatientProfileRequest request)
    {
        var profile = await _dbContext.Set<PatientProfile>()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null)
        {
            throw new Exception("Patient profile not found");
        }

        profile.FirstName = request.FirstName;
        profile.LastName = request.LastName;
        profile.Gender = request.Gender;
        profile.DateOfBirth = request.DateOfBirth;
        profile.PhoneNumber = request.PhoneNumber;

        await _dbContext.SaveChangesAsync();

        return MapToDto(profile);
    }

    public async Task<PatientProfileDto> CreateProfileAsync(string userId, string firstName, string lastName, string email, Gender gender)
    {
        var profile = await _dbContext.Set<PatientProfile>()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile != null) return MapToDto(profile);

        profile = new PatientProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Gender = gender,
            DateOfBirth = DateTime.MinValue, // Default value, user should update it
            PhoneNumber = string.Empty
        };

        await _dbContext.AddAsync(profile);
        await _dbContext.SaveChangesAsync();

        return MapToDto(profile);
    }

    private static PatientProfileDto MapToDto(PatientProfile profile)
    {
        return new PatientProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Gender = profile.Gender,
            DateOfBirth = profile.DateOfBirth,
            PhoneNumber = profile.PhoneNumber,
            Email = profile.Email
        };
    }
}
