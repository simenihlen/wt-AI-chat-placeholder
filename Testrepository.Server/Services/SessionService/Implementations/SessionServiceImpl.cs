using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Testrepository.Server.Exceptions;
using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;

namespace Testrepository.Server.Services.SessionService.Implementations;

public class SessionServiceImpl : ISessionService
{
    private readonly DatabaseContext _context;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public SessionServiceImpl(IConfiguration configuration, DatabaseContext context)
    {
        _context = context;
        _apiKey = configuration["OpenAI:ApiKey"];
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    // ✅ Create a session for a specific user
    public async Task<SessionEntity> CreateSessionAsync(CreateSessionDTO request)
    {
        var project = await _context.ProjectEntities.FindAsync(request.projectId);
        if (project == null)
        {
            throw new NoSuchProjectException(request.projectId);
        }
        var session = new SessionEntity
        {
            user_id = project.sub,
            created_at = DateTime.UtcNow,
            project_id = request.projectId,
            title = "New Session",
        };
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<SessionDTO?> GetSessionByIdAsync(int sessionId)
    {
        var session = await _context.Sessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.session_id == sessionId);

        if (session == null) 
            throw new NoSuchSessionException(sessionId);

        return new SessionDTO(session);
    }

    // ✅ Get sessions only for a specific user
    public async Task<List<SessionDTO>> GetSessionsByUserIdAsync(string userId)
    {
        var user = await _context.UserEntities.FindAsync(userId);
        if (user == null)
        {
            throw new NoSuchUserException(userId);
        }
        var sessions = await _context.Sessions
            .Where(s => s.user_id == userId)
            .Include(s => s.Messages)
            .OrderByDescending(s => s.created_at)
            .AsNoTracking()
            .ToListAsync();

        return sessions.Select(session => new SessionDTO(session)).ToList();
    }

    // ✅ Add messages to a session, separating user messages from bot messages
    public async Task<List<ChatMessageDTO>> AddMessagesToSessionAsync(int sessionId, string userId, List<ChatMessageDTO> messages)
    {
        var session = await _context.Sessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.session_id == sessionId);

        if (session == null)
        {
            throw new NoSuchSessionException(sessionId);
        }

        // Convert DTOs to Entities
        var messageEntities = messages.Select(msg => new ChatMessageEntity
        {
            user_id = msg.sender == "bot" ? null : userId, // ✅ Assign user_id for users, NULL for bots
            bot = msg.sender == "bot", // ✅ Mark bot messages
            text = msg.text,
            timestamp = DateTime.UtcNow,
            session_id = sessionId
        }).ToList();

        _context.ChatMessages.AddRange(messageEntities);
        await _context.SaveChangesAsync();

        // ✅ Generate session title if this is the first message
        if (session.Messages.Count == 0 && messageEntities.Count > 0)
        {
            string firstMessage = messageEntities[0].text;
            session.title = await GenerateSessionTitle(firstMessage);
            Console.WriteLine($"Session {session.session_id}:  {session.title}");
            await _context.SaveChangesAsync();
        }

        // Convert back to DTOs before returning
        return messageEntities.Select(m => new ChatMessageDTO(m)).ToList();
    }

    private async Task<string> GenerateSessionTitle(string text)
    {
        var requestPayload = new
        {
            model = "gpt-4o",
            messages = new List<object>
            {
                new { role = "system", content = "Extract a short session title from the following user message:" },
                new { role = "user", content = text }
            },
            max_tokens = 10,
            temperature = 0.7
        };

        var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"OpenAI Error: {errorContent}");
            return "New Chat";
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()?.Trim() ?? "New Chat";
    }

    // ✅ Remove session, ensuring only the session owner can delete it
    public async Task<bool> RemoveSessionAsync(int sessionId, string userId)
    {
        var user = await _context.UserEntities.FindAsync(userId);
        if (user == null)
        {
            throw new NoSuchUserException(userId);
        }
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.session_id == sessionId);

        if (session == null)
        {
            throw new NoSuchSessionException(sessionId);
        }
        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
        return true; // Indicate success
    }
}
