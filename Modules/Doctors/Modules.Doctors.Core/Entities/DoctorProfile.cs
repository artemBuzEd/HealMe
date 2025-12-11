namespace Modules.Doctors.Core;

public class DoctorProfile
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string SpecializationId { get; set; }
    public double ConsultationFee { get; set; }
    
    public string PhoneNumber { get; set; }
    public string MedicalInstitutionLicense { get; set; }
    public string? Biography { get; set; }
}