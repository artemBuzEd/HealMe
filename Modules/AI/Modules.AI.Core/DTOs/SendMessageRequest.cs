using System.ComponentModel.DataAnnotations;

namespace Modules.AI.Core.DTOs;

public class SendMessageRequest
{
    public string? SessionId { get; set; }
    
    [Required]
    public string Message { get; set; } = string.Empty;
}
