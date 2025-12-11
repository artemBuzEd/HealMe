using Modules.AI.Core.DTOs;

namespace Modules.AI.Core.Interfaces;

public interface IAiService
{
    Task<AiMessageDto> SendMessageAsync(string userId, SendMessageRequest request);
    Task<IEnumerable<AiSessionDto>> GetUserSessionsAsync(string userId);
    Task<IEnumerable<AiMessageDto>> GetSessionMessagesAsync(Guid sessionId, string userId);
}
