namespace Modules.Doctors.Core.Entities;

public class DoctorReview
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
