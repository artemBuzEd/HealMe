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

        var reviews = await _dbContext.Set<DoctorReview>()
            .Where(x => x.DoctorId == profile.Id)
            .ToListAsync();

        return MapToDto(profile, reviews);
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

        var reviews = await _dbContext.Set<DoctorReview>()
            .Where(x => x.DoctorId == profile.Id)
            .ToListAsync();

        return MapToDto(profile, reviews);
    }

    public async Task<DoctorProfileDto> CreateProfileAsync(string userId, string firstName, string lastName)
    {
        var profile = await _dbContext.Set<DoctorProfile>()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile != null) return MapToDto(profile, new List<DoctorReview>());

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

        return MapToDto(profile, new List<DoctorReview>());
    }

    public async Task<IEnumerable<DoctorProfileDto>> GetAllDoctorsAsync()
    {
        var profiles = await _dbContext.Set<DoctorProfile>().ToListAsync();
        var profileIds = profiles.Select(p => p.Id).ToList();
        var reviews = await _dbContext.Set<DoctorReview>()
            .Where(r => profileIds.Contains(r.DoctorId))
            .ToListAsync();
        var reviewsByDoctor = reviews.GroupBy(r => r.DoctorId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return profiles.Select(p =>
            MapToDto(p, reviewsByDoctor.GetValueOrDefault(p.Id, new List<DoctorReview>())));
    }

    public async Task<DoctorProfileDto?> GetDoctorByIdAsync(Guid id)
    {
        var profile = await _dbContext.Set<DoctorProfile>().FindAsync(id);
        if (profile == null) return null;

        var reviews = await _dbContext.Set<DoctorReview>()
            .Where(x => x.DoctorId == id)
            .ToListAsync();

        return MapToDto(profile, reviews);
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
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return reviews.Select(MapReviewToDto);
    }

    public async Task<PaginatedResponse<DoctorReviewDto>> GetReviewsAsync(Guid doctorId, int page, int pageSize)
    {
        var query = _dbContext.Set<DoctorReview>()
            .Where(x => x.DoctorId == doctorId);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var reviews = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<DoctorReviewDto>
        {
            Items = reviews.Select(MapReviewToDto),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<DoctorReviewDto> CreateReviewAsync(Guid appointmentId, Guid doctorId, string patientId, string patientFirstName, string patientLastName, CreateReviewRequest request)
    {
        var existingReview = await _dbContext.Set<DoctorReview>()
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

        if (existingReview != null)
            throw new Exception("A review has already been submitted for this appointment");

        var review = new DoctorReview
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            AppointmentId = appointmentId,
            PatientId = patientId,
            PatientFirstName = patientFirstName,
            PatientLastName = patientLastName,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.AddAsync(review);
        await _dbContext.SaveChangesAsync();

        return MapReviewToDto(review);
    }

    public async Task<DoctorReviewDto> UpdateReviewAsync(Guid reviewId, string patientId, UpdateReviewRequest request)
    {
        var review = await _dbContext.Set<DoctorReview>()
            .FirstOrDefaultAsync(x => x.Id == reviewId);

        if (review == null)
            throw new Exception("Review not found");

        if (review.PatientId != patientId)
            throw new Exception("You are not authorized to edit this review");

        if (DateTime.UtcNow > review.CreatedAt.AddHours(24))
            throw new Exception("Reviews can only be edited within 24 hours of publication");

        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapReviewToDto(review);
    }

    public async Task<DoctorReviewDto?> GetReviewByAppointmentAsync(Guid appointmentId)
    {
        var review = await _dbContext.Set<DoctorReview>()
            .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

        return review == null ? null : MapReviewToDto(review);
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

    private static DoctorProfileDto MapToDto(DoctorProfile profile, List<DoctorReview> reviews)
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
            Biography = profile.Biography,
            AverageRating = reviews.Count > 0 ? Math.Round(reviews.Average(r => r.Rating), 2) : 0,
            ReviewCount = reviews.Count
        };
    }

    private static DoctorReviewDto MapReviewToDto(DoctorReview review)
    {
        return new DoctorReviewDto
        {
            Id = review.Id,
            DoctorId = review.DoctorId,
            AppointmentId = review.AppointmentId,
            PatientId = review.PatientId,
            PatientFirstName = review.PatientFirstName,
            PatientLastName = review.PatientLastName,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }
}
