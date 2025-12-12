namespace Modules.Appointments.Core.DTOs;

public class AppointmentAuthDto
{
    public Guid Id { get; set; }
    public string PatientUserId { get; set; } = string.Empty;
    public string DoctorUserId { get; set; } = string.Empty;
}
