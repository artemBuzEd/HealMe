using System.ComponentModel.DataAnnotations;

namespace Modules.Doctors.Core.DTOs;

public class UpdateAvailabilityRequest
{
    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }
}
