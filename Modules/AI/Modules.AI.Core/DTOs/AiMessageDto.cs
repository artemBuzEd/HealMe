namespace Modules.AI.Core.DTOs;

public class AiMessageDto
{
    public Guid SessionId { get; set; }
    
    public Guid MessageId {get;set;}
    public string UserMessage { get; set; } = string.Empty;
    public string AiResponse { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
