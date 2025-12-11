using System.ComponentModel.DataAnnotations;
using Modules.Identity.Core.Entities.Enums;

namespace Modules.Patients.Core.DTOs;

public class UpdatePatientProfileRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public Gender Gender { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
}
