namespace Modules.AI.Core.Entities;

public class AiChatMessage
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string UserMessage { get; set; } = string.Empty;
    public string AiResponse { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
