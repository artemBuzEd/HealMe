using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Modules.AI.Core.DTOs;
using Modules.AI.Core.Entities;
using Modules.AI.Core.Interfaces;
using Modules.AI.Infrastructure.Persistence;
using Modules.Patients.Core.Interfaces;

namespace Modules.AI.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly AiDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IPatientService _patientService;

    public AiService(AiDbContext dbContext, HttpClient httpClient, IConfiguration configuration, IPatientService patientService)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _configuration = configuration;
        _patientService = patientService;
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

    public async Task<AnamnesisPdfData> GetAnamnesisPdfDataAsync(Guid sessionId, string userId)
    {
        var session = await _dbContext.Set<AiChatSession>()
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);

        if (session == null) throw new Exception("Session not found");

        // Fetch structured anamnesis from external AI service
        var aiUrl = _configuration["AiService:Url"];
        if (string.IsNullOrEmpty(aiUrl)) throw new Exception("AI Service URL not configured");

        var baseUrl = aiUrl.TrimEnd('/');
        var anamnesisUrl = $"{baseUrl}/{sessionId}/anamnesis";

        var response = await _httpClient.GetAsync(anamnesisUrl);
        response.EnsureSuccessStatusCode();

        var anamnesisResponse = await response.Content.ReadFromJsonAsync<ExternalAnamnesisResponse>();
        if (anamnesisResponse == null || string.IsNullOrEmpty(anamnesisResponse.anamnesis))
            throw new Exception("Failed to get anamnesis from AI Service");

        var patient = await _patientService.GetProfileAsync(userId);

        var pdfData = new AnamnesisPdfData
        {
            PatientFullName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "Unknown Patient",
            DateOfBirth = patient?.DateOfBirth,
            Gender = patient?.Gender.ToString() ?? "Not specified",
            PatientEmail = patient?.Email ?? string.Empty,
            PatientPhone = patient?.PhoneNumber ?? string.Empty,
            SessionTitle = session.Title,
            SessionDate = session.CreatedAt,
            RawAnamnesis = anamnesisResponse.anamnesis,
            Sections = ParseAnamnesisSections(anamnesisResponse.anamnesis)
        };

        return pdfData;
    }

    private static List<AnamnesisSection> ParseAnamnesisSections(string raw)
    {
        var sections = new List<AnamnesisSection>();
        AnamnesisSection? current = null;

        var lines = raw.Split('\n');
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Strip trailing markdown spaces (two trailing spaces = line break)
            line = line.TrimEnd();

            // Detect section header: **1. Title** or **N. Title**
            if (line.StartsWith("**") && line.Contains(".**"))
            {
                var clean = StripBold(line);
                // Remove leading number + dot: "1. Title" -> "Title"
                var dotIdx = clean.IndexOf('.');
                if (dotIdx >= 0 && dotIdx < 4)
                {
                    var title = clean.Substring(dotIdx + 1).Trim();
                    if (!string.IsNullOrEmpty(title))
                    {
                        current = new AnamnesisSection { Title = title };
                        sections.Add(current);
                        continue;
                    }
                }
            }

            // Ensure we have a section to add items to
            if (current == null)
            {
                current = new AnamnesisSection { Title = "Загальне" };
                sections.Add(current);
            }

            // Bullet item: "- text"
            if (line.StartsWith("-"))
            {
                var item = StripBold(line.Substring(1).Trim());
                if (!string.IsNullOrEmpty(item))
                    current.Items.Add(item);
            }
            else
            {
                // Plain body text — strip bold markers and add
                var item = StripBold(line);
                if (!string.IsNullOrEmpty(item))
                    current.Items.Add(item);
            }
        }

        return sections;
    }

    /// <summary>
    /// Remove all **bold** markers from text: "**word** other" → "word other"
    /// </summary>
    private static string StripBold(string text)
    {
        return text.Replace("**", "");
    }

    private class ExternalAnamnesisResponse
    {
        public string chat_id { get; set; } = string.Empty;
        public string anamnesis { get; set; } = string.Empty;
    }

    private class ExternalAiResponse
    {
        public string chat_id { get; set; } = string.Empty;
        public string user_message { get; set; } = string.Empty;
        public string ai_response { get; set; } = string.Empty;
        public DateTime timestamp { get; set; }
    }
}
