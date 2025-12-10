using System.ComponentModel.DataAnnotations;

namespace Modules.Doctors.Core.DTOs;

public class UpdateDoctorProfileRequest
{
    [Required]
    public string SpecializationId { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public double ConsultationFee { get; set; }

    [Required]
    public string MedicalInstitutionLicense { get; set; } = string.Empty;

    public string? Biography { get; set; }
}
