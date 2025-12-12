using Microsoft.EntityFrameworkCore;
using Modules.Appointments.Core.DTOs;
using Modules.Appointments.Core.Entities;
using Modules.Appointments.Core.Interfaces;
using Modules.Appointments.Infrastructure.Persistence;
using Modules.Doctors.Core.Interfaces;
using Modules.Patients.Core.Interfaces;

namespace Modules.Appointments.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly AppointmentsDbContext _dbContext;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;

    public AppointmentService(AppointmentsDbContext dbContext, IPatientService patientService, IDoctorService doctorService)
    {
        _dbContext = dbContext;
        _patientService = patientService;
        _doctorService = doctorService;
    }

    public async Task<AppointmentDto> BookAppointmentAsync(string patientUserId, BookAppointmentRequest request)
    {
        var patient = await _patientService.GetProfileAsync(patientUserId);
        if (patient == null) throw new Exception("Patient profile not found");

        // Validate doctor exists
        var doctor = await _doctorService.GetDoctorByIdAsync(request.DoctorId);
        if (doctor == null) throw new Exception("Doctor not found");

        if (request.StartTime >= request.EndTime)
            throw new ArgumentException("Start time must be before end time");

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            PatientUserId = patientUserId,
            DoctorId = request.DoctorId,
            DoctorUserId = doctor.UserId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.AddAsync(appointment);
        await _dbContext.SaveChangesAsync();

        return MapToDto(appointment);
    }

    public async Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(string patientUserId)
    {
        var patient = await _patientService.GetProfileAsync(patientUserId);
        if (patient == null) throw new Exception("Patient profile not found");

        var appointments = await _dbContext.Set<Appointment>()
            .Where(x => x.PatientId == patient.Id)
            .OrderByDescending(x => x.StartTime)
            .ToListAsync();

        return appointments.Select(MapToDto);
    }

    public async Task<IEnumerable<AppointmentDto>> GetDoctorAppointmentsAsync(string doctorUserId)
    {
        var doctor = await _doctorService.GetProfileAsync(doctorUserId);
        if (doctor == null) throw new Exception("Doctor profile not found");

        var appointments = await _dbContext.Set<Appointment>()
            .Where(x => x.DoctorId == doctor.Id)
            .OrderByDescending(x => x.StartTime)
            .ToListAsync();

        return appointments.Select(MapToDto);
    }

    public async Task<AppointmentDto> ConfirmAppointmentAsync(string doctorUserId, Guid appointmentId)
    {
        var doctor = await _doctorService.GetProfileAsync(doctorUserId);
        if (doctor == null) throw new Exception("Doctor profile not found");

        var appointment = await _dbContext.Set<Appointment>()
            .FirstOrDefaultAsync(x => x.Id == appointmentId);

        if (appointment == null) throw new Exception("Appointment not found");
        if (appointment.DoctorId != doctor.Id) throw new Exception("Appointment does not belong to this doctor");

        if (appointment.Status != AppointmentStatus.Pending)
            throw new Exception("Only pending appointments can be confirmed");

        appointment.Status = AppointmentStatus.Confirmed;
        await _dbContext.SaveChangesAsync();

        return MapToDto(appointment);
    }

    public async Task CancelAppointmentAsync(string userId, Guid appointmentId)
    {
        var appointment = await _dbContext.Set<Appointment>()
            .FirstOrDefaultAsync(x => x.Id == appointmentId);

        if (appointment == null) throw new Exception("Appointment not found");

        // Check if user is the patient or the doctor
        var patient = await _patientService.GetProfileAsync(userId);
        var doctor = await _doctorService.GetProfileAsync(userId);

        bool isPatient = patient != null && appointment.PatientId == patient.Id;
        bool isDoctor = doctor != null && appointment.DoctorId == doctor.Id;

        if (!isPatient && !isDoctor)
            throw new Exception("You are not authorized to cancel this appointment");

        // 4-hour cancellation rule
        if (appointment.StartTime <= DateTime.UtcNow.AddHours(4))
        {
            throw new Exception("Cannot cancel appointment less than 4 hours before start time");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<AppointmentDto> GetAppointmentByIdAsync(Guid appointmentId)
    {
        var appointment = await _dbContext.Set<Appointment>()
            .FirstOrDefaultAsync(x => x.Id == appointmentId);
        
        if(appointment == null) throw new Exception("Appointment not found");
        
        return MapToDto(appointment);
    }

    public async Task<AppointmentAuthDto> GetAppointmentAuthDetailsAsync(Guid appointmentId)
    {
        var appointment = await _dbContext.Set<Appointment>()
            .Select(x => new AppointmentAuthDto
            {
                Id = x.Id,
                PatientUserId = x.PatientUserId,
                DoctorUserId = x.DoctorUserId
            })
            .FirstOrDefaultAsync(x => x.Id == appointmentId);

        if (appointment == null) throw new Exception("Appointment not found");

        return appointment;
    }

    private static AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            CreatedAt = appointment.CreatedAt
        };
    }
}
