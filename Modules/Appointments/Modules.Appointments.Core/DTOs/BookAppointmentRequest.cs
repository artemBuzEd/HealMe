using System.ComponentModel.DataAnnotations;

namespace Modules.Appointments.Core.DTOs;

public class BookAppointmentRequest
{
    [Required]
    public Guid DoctorId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }
}
