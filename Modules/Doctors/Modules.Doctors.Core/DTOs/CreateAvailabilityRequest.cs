using System.ComponentModel.DataAnnotations;

namespace Modules.Doctors.Core.DTOs;

public class CreateAvailabilityRequest
{
    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }
}
