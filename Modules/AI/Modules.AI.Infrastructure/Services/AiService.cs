using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.AI.Core.DTOs;
using Modules.AI.Core.Entities;
using Modules.AI.Core.Interfaces;
using Modules.AI.Infrastructure.Persistence;

namespace Modules.AI.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly AiDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AiService(AiDbContext dbContext, HttpClient httpClient, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AiMessageDto> SendMessageAsync(string userId, SendMessageRequest request)
    {
        Guid sessionId;
        bool isNewSession = string.IsNullOrEmpty(request.SessionId);

        if (isNewSession)
        {
            sessionId = Guid.NewGuid();
        }
        else
        {
            if (!Guid.TryParse(request.SessionId, out sessionId))
            {
                throw new ArgumentException("Invalid SessionId format");
            }
        }

        // 1. Create session if new
        if (isNewSession)
        {
            var session = new AiChatSession
            {
                Id = sessionId,
                UserId = userId,
                Title = request.Message.Length > 50 ? request.Message.Substring(0, 47) + "..." : request.Message,
                CreatedAt = DateTime.UtcNow
            };
            await _dbContext.AddAsync(session);
        }
        else
        {
            // Verify session exists and belongs to user
            var session = await _dbContext.Set<AiChatSession>()
                .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);
            
            if (session == null) throw new Exception("Session not found");
        }

        // 2. Call External AI
        var aiUrl = _configuration["AiService:Url"];
        if (string.IsNullOrEmpty(aiUrl)) throw new Exception("AI Service URL not configured");

        var externalRequest = new
        {
            chat_id = sessionId.ToString(),
            message = request.Message
        };

        var response = await _httpClient.PostAsJsonAsync(aiUrl, externalRequest);
        response.EnsureSuccessStatusCode();

        var externalResponse = await response.Content.ReadFromJsonAsync<ExternalAiResponse>();
        if (externalResponse == null) throw new Exception("Invalid response from AI Service");

        // 3. Save Message
        var message = new AiChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            UserMessage = request.Message,
            AiResponse = externalResponse.ai_response,
            Timestamp = DateTime.UtcNow
        };

        await _dbContext.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        return new AiMessageDto
        {
            SessionId = sessionId,
            MessageId = message.Id,
            UserMessage = message.UserMessage,
            AiResponse = message.AiResponse,
            Timestamp = message.Timestamp
        };
    }

    public async Task<IEnumerable<AiSessionDto>> GetUserSessionsAsync(string userId)
    {
        var sessions = await _dbContext.Set<AiChatSession>()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return sessions.Select(x => new AiSessionDto
        {
            Id = x.Id,
            Title = x.Title,
            CreatedAt = x.CreatedAt
        });
    }

    public async Task<IEnumerable<AiMessageDto>> GetSessionMessagesAsync(Guid sessionId, string userId)
    {
        // Verify ownership
        var session = await _dbContext.Set<AiChatSession>()
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);
        
        if (session == null) throw new Exception("Session not found");

        var messages = await _dbContext.Set<AiChatMessage>()
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.Timestamp)
            .ToListAsync();

        return messages.Select(x => new AiMessageDto
        {
            SessionId = sessionId,
            MessageId = x.Id,
            UserMessage = x.UserMessage,
            AiResponse = x.AiResponse,
            Timestamp = x.Timestamp
        });
    }

    private class ExternalAiResponse
    {
        public string chat_id { get; set; } = string.Empty;
        public string user_message { get; set; } = string.Empty;
        public string ai_response { get; set; } = string.Empty;
        public DateTime timestamp { get; set; }
    }
}
