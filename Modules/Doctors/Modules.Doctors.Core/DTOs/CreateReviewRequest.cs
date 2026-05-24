using System.ComponentModel.DataAnnotations;

namespace Modules.Doctors.Core.DTOs;

public class CreateReviewRequest
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    public string Comment { get; set; } = string.Empty;
}
