namespace Modules.Doctors.Core.DTOs;

public class DoctorProfileDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string SpecializationId { get; set; } = string.Empty;
    public double ConsultationFee { get; set; }
    public string MedicalInstitutionLicense { get; set; } = string.Empty;
    public string? Biography { get; set; }
}
