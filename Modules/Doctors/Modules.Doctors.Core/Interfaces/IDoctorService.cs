using Modules.Doctors.Core.DTOs;

namespace Modules.Doctors.Core.Interfaces;

public interface IDoctorService
{
    Task<DoctorProfileDto?> GetProfileAsync(string userId);
    Task<DoctorProfileDto> UpdateProfileAsync(string userId, UpdateDoctorProfileRequest request);
    Task<DoctorProfileDto> CreateProfileAsync(string userId);
    Task<IEnumerable<DoctorProfileDto>> GetAllDoctorsAsync();
    Task<DoctorProfileDto?> GetDoctorByIdAsync(Guid id);
    Task<IEnumerable<DoctorAvailabilityDto>> GetAvailabilityAsync(Guid doctorId);
    Task<IEnumerable<DoctorReviewDto>> GetReviewsAsync(Guid doctorId);
    Task<DoctorAvailabilityDto> AddAvailabilityAsync(string userId, CreateAvailabilityRequest request);
}
