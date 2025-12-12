using Modules.Appointments.Core.DTOs;

namespace Modules.Appointments.Core.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentDto> BookAppointmentAsync(string patientUserId, BookAppointmentRequest request);
    Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(string patientUserId);
    Task<IEnumerable<AppointmentDto>> GetDoctorAppointmentsAsync(string doctorUserId);
    Task<AppointmentDto> ConfirmAppointmentAsync(string doctorUserId, Guid appointmentId);
    Task CancelAppointmentAsync(string userId, Guid appointmentId);
    Task<AppointmentDto> GetAppointmentByIdAsync(Guid appointmentId);
    Task<AppointmentAuthDto> GetAppointmentAuthDetailsAsync(Guid appointmentId);
}
