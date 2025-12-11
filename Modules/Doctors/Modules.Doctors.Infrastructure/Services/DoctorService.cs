using Microsoft.EntityFrameworkCore;
using Modules.Doctors.Core;
using Modules.Doctors.Core.DTOs;
using Modules.Doctors.Core.Entities;
using Modules.Doctors.Core.Interfaces;
using Modules.Doctors.Infrastructure.Persistence;

namespace Modules.Doctors.Infrastructure.Services;

public class DoctorService : IDoctorService
{
    private readonly DoctorsDbContext _dbContext;

    public DoctorService(DoctorsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DoctorProfileDto?> GetProfileAsync(string userId)
    {
        var profile = await _dbContext.Set<DoctorProfile>()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null) return null;

        return MapToDto(profile);
    }

    public async Task<DoctorProfileDto> UpdateProfileAsync(string userId, UpdateDoctorProfileRequest request)
    {
        var profile = await _dbContext.Set<DoctorProfile>()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null)
        {
            throw new NotFoundException(userId, "Doctor");
        }

        profile.SpecializationId = request.SpecializationId;
        profile.ConsultationFee = request.ConsultationFee;
        profile.MedicalInstitutionLicense = request.MedicalInstitutionLicense;
        profile.PhoneNumber = request.PhoneNumber;
        profile.Biography = request.Biography;

        await _dbContext.SaveChangesAsync();

        return MapToDto(profile);
    }

    public async Task<DoctorProfileDto> CreateProfileAsync(string userId, string firstName, string lastName)
    {
        var profile = await _dbContext.Set<DoctorProfile>()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile != null) return MapToDto(profile);

        profile = new DoctorProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            SpecializationId = string.Empty,
            ConsultationFee = 0,
            PhoneNumber = string.Empty,
            MedicalInstitutionLicense = string.Empty,
            Biography = string.Empty
        };

        await _dbContext.AddAsync(profile);
        await _dbContext.SaveChangesAsync();

        return MapToDto(profile);
    }

    public async Task<IEnumerable<DoctorProfileDto>> GetAllDoctorsAsync()
    {
        var profiles = await _dbContext.Set<DoctorProfile>().ToListAsync();
        return profiles.Select(MapToDto);
    }

    public async Task<DoctorProfileDto?> GetDoctorByIdAsync(Guid id)
    {
        var profile = await _dbContext.Set<DoctorProfile>().FindAsync(id);
        return profile == null ? null : MapToDto(profile);
    }

    public async Task<IEnumerable<DoctorAvailabilityDto>> GetAvailabilityAsync(Guid doctorId)
    {
        var availability = await _dbContext.Set<DoctorAvailability>()
            .Where(x => x.DoctorId == doctorId)
            .ToListAsync();

        return availability.Select(x => new DoctorAvailabilityDto
        {
            Id = x.Id,
            DoctorId = x.DoctorId,
            DayOfWeek = x.DayOfWeek,
            StartTime = x.StartTime,
            EndTime = x.EndTime
        });
    }

    public async Task<IEnumerable<DoctorReviewDto>> GetReviewsAsync(Guid doctorId)
    {
        var reviews = await _dbContext.Set<DoctorReview>()
            .Where(x => x.DoctorId == doctorId)
            .ToListAsync();

        return reviews.Select(x => new DoctorReviewDto
        {
            Id = x.Id,
            DoctorId = x.DoctorId,
            PatientId = x.PatientId,
            Rating = x.Rating,
            Comment = x.Comment,
            CreatedAt = x.CreatedAt
        });
    }

    public async Task<DoctorAvailabilityDto> AddAvailabilityAsync(string userId, CreateAvailabilityRequest request)
    {
        var profile = await _dbContext.Set<DoctorProfile>()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null)
        {
            throw new NotFoundException(userId, "Doctor");
        }

        if (request.StartTime >= request.EndTime)
        {
            throw new ArgumentException("Start time must be before end time");
        }

        var availability = new DoctorAvailability
        {
            Id = Guid.NewGuid(),
            DoctorId = profile.Id,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        await _dbContext.AddAsync(availability);
        await _dbContext.SaveChangesAsync();

        return new DoctorAvailabilityDto
        {
            Id = availability.Id,
            DoctorId = availability.DoctorId,
            DayOfWeek = availability.DayOfWeek,
            StartTime = availability.StartTime,
            EndTime = availability.EndTime
        };
    }

    public async Task<DoctorAvailabilityDto> UpdateAvailabilityAsync(string userId, Guid availabilityId, UpdateAvailabilityRequest request)
    {
        var profile = await _dbContext.Set<DoctorProfile>()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null)
        {
            throw new NotFoundException(userId, "Doctor");
        }

        var availability = await _dbContext.Set<DoctorAvailability>()
            .FirstOrDefaultAsync(x => x.Id == availabilityId && x.DoctorId == profile.Id);

        if (availability == null)
        {
            throw new Exception("Availability slot not found or does not belong to this doctor");
        }

        if (request.StartTime >= request.EndTime)
        {
            throw new ArgumentException("Start time must be before end time");
        }

        availability.StartTime = request.StartTime;
        availability.EndTime = request.EndTime;

        await _dbContext.SaveChangesAsync();

        return new DoctorAvailabilityDto
        {
            Id = availability.Id,
            DoctorId = availability.DoctorId,
            DayOfWeek = availability.DayOfWeek,
            StartTime = availability.StartTime,
            EndTime = availability.EndTime
        };
    }

    public async Task DeleteAvailabilityAsync(string userId, Guid availabilityId)
    {
        var profile = await _dbContext.Set<DoctorProfile>()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile == null)
        {
            throw new NotFoundException(userId, "Doctor");
        }

        var availability = await _dbContext.Set<DoctorAvailability>()
            .FirstOrDefaultAsync(x => x.Id == availabilityId && x.DoctorId == profile.Id);

        if (availability == null)
        {
            throw new Exception("Availability slot not found or does not belong to this doctor");
        }

        _dbContext.Remove(availability);
        await _dbContext.SaveChangesAsync();
    }

    private static DoctorProfileDto MapToDto(DoctorProfile profile)
    {
        return new DoctorProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            SpecializationId = profile.SpecializationId,
            ConsultationFee = profile.ConsultationFee,
            PhoneNumber = profile.PhoneNumber,
            MedicalInstitutionLicense = profile.MedicalInstitutionLicense,
            Biography = profile.Biography
        };
    }
}
