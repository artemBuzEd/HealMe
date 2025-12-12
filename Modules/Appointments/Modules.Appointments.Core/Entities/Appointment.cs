namespace Modules.Appointments.Core.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientUserId { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorUserId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
