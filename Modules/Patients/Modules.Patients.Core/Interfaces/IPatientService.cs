using Modules.Identity.Core.Entities.Enums;
using Modules.Patients.Core.DTOs;

namespace Modules.Patients.Core.Interfaces;

public interface IPatientService
{
    Task<PatientProfileDto?> GetProfileAsync(string userId);
    Task<PatientProfileDto> UpdateProfileAsync(string userId, UpdatePatientProfileRequest request);
    Task<PatientProfileDto> CreateProfileAsync(string userId, string firstName, string lastName, string email, Gender gender);
}
