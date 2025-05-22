using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;
using Pgvector.EntityFrameworkCore;
using Testrepository.Server.Services.ChatService;
using Testrepository.Server.Services.EmbeddingService;
using Testrepository.Server.Models.Requests;
using Testrepository.Server.Exceptions;
using Testrepository.Server.Services.StoryService;
using Testrepository.Server.Models.DTO;

public class OpenAIChatServiceImpl : IChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly DatabaseContext _context;
    private readonly IEmbeddingService _embeddingService;
    private readonly IStoryService _storyService;


    public OpenAIChatServiceImpl(IConfiguration configuration, DatabaseContext context, IEmbeddingService embeddingService, IStoryService storyService) 
    {
        _apiKey = configuration["OpenAI:ApiKey"];
        _context = context;
        _embeddingService = embeddingService;
        _storyService = storyService;

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new Exception("OpenAI API key is not configured in appsettings.json.");
        }

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    
    public async Task<string> GetResponseAsync(ChatRequest request, List<StoryDTO> stories)
    {
        Console.WriteLine($"[DEBUG] Received {stories.Count} stories in GetResponseAsync");
        foreach (var story in stories)
        {
            Console.WriteLine($"[DEBUG] Story: ID={story.id}, Title={story.Title}, Description={story.Description}, Background count={story.BackgroundInfo?.Count ?? 0}");
        }

        if (string.IsNullOrEmpty(request.Prompt))
            throw new InvalidPromptException(request.Prompt);

        UserEntity? user = await _context.UserEntities.FirstOrDefaultAsync(u => u.sub == request.UserId);
        if (user == null)
            throw new NoSuchUserException(request.UserId);
        SessionEntity? session = await _context.Sessions.FirstOrDefaultAsync(s => s.session_id == request.SessionId);
        if (session == null)
            throw new NoSuchSessionException(request.SessionId);

        var embeddingVector = await GetEmbeddingForPrompt(request.Prompt);

      /* List<StoryDTO> summarizedStories = await _storyService.ProcessStoriesForSessionAsync(request.SessionId, stories); 
        Console.WriteLine($"[DEBUG] Summarized {summarizedStories.Count} stories. Sending to OpenAI..."); */

        // ✅ Await `PrepareChatMessages` to fetch messages correctly
        var messages = await PrepareChatMessages(request.SessionId, request.UserId, request.Prompt, request.Stories, request.includeOnlySelected);

        var serializedMessages = JsonSerializer.Serialize(messages, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine("[DEBUG] Final OpenAI messages payload:\n" + serializedMessages);

        string botReply = await CallChatGptApi(messages);

        await SaveChatMessages(request.SessionId, request.UserId, request.Prompt, botReply, embeddingVector);
        await UpdateSessionTitleIfFirstMessage(request.SessionId, request.Prompt);
        await UpdateSessionSummaryIfNeeded(request.SessionId);

        return botReply;
    }

    internal async Task<double[]> GetEmbeddingForPrompt(string prompt)
    {
        return await _embeddingService.GetEmbeddingAsync(prompt);
    }

    internal async Task<List<SessionSummaryEntity>> GetPastSummaries(string user_id,int projectId, double[] embeddingVector)
    {
        return await _embeddingService.GetSimilarSessionSummariesAsync(user_id, projectId,embeddingVector, limit: 1);
    }

 internal async Task<List<object>> PrepareChatMessages(int sessionId, string userId, string prompt, List<StoryDTO> attachedStories, bool includeOnlySelected)
{
    var messages = new List<object>();
    var localBulletSummary = new Dictionary<string, string>();

    var session = await _context.Sessions.FindAsync(sessionId)
        ?? throw new NoSuchSessionException(sessionId);

    int projectId = session.project_id;
    var queryEmbedding = await _embeddingService.GetEmbeddingAsync(prompt);

    
    var similarSummaries = (await GetPastSummaries(userId, projectId, queryEmbedding))
        .Where(s => s.session_id != sessionId)
        .ToList();

    var combinedSummariesText = string.Join("\n", similarSummaries.Select(s => s.summary_text));
    var bulletPointsSummary = await _embeddingService.GetSummaryFromOpenAI(combinedSummariesText);

    messages.Add(new
    {
        role = "system",
        content = string.IsNullOrWhiteSpace(combinedSummariesText)
            ? "You are a helpful assistant."
            : $"You are a helpful assistant. Here are key details from past sessions:\n{bulletPointsSummary}"
    });

    
    var similarStories = await _storyService.GetSimilarStoriesAsync(queryEmbedding, userId, projectId, 1);
    var userStories = attachedStories.GroupBy(s => s.id).Select(g => g.First()).ToList();

    var vectorOnlyStories = similarStories
        .Where(s => !userStories.Any(us => us.id == s.id))
        .Select(s => new StoryDTO
        {
            id = s.id,
            Title = s.title,
            Description = s.storySummary,
            BackgroundInfo = s.backgroundInfo?.Select(b => b.Text).ToList(),
            ProjectId = projectId
        })
        .ToList();

    var storyIds = userStories.Select(s => s.id).Union(vectorOnlyStories.Select(s => s.id)).ToList();
    var storyEntities = await _context.Stories
        .Where(s => storyIds.Contains(s.id))
        .ToDictionaryAsync(s => s.id);

    string Truncate(string input, int max) =>
        string.IsNullOrWhiteSpace(input) ? "" : input.Length > max ? input.Substring(0, max) + "..." : input;

    
    var storyContent = new StringBuilder();

    if (userStories.Any())
    {
        storyContent.AppendLine("The user has selected the following stories:");

        foreach (var dto in userStories)
        {
            if (!storyEntities.TryGetValue(dto.id, out var entity)) continue;

            string miniSummary = await GetBulletSummaryCached(entity, localBulletSummary);
            storyContent.AppendLine($"- {dto.Title}: {miniSummary}");

            var background = dto.BackgroundInfo != null && dto.BackgroundInfo.Any()
                ? string.Join("\n- ", dto.BackgroundInfo.Select(b => Truncate(b, 200)))
                : "No background information available.";

            storyContent.AppendLine("**Background Info**:\n" + background);
        }
    }

    if (!includeOnlySelected && vectorOnlyStories.Any())
    {
        storyContent.AppendLine("\nThe assistant recalled these relevant stories based on similarity:");

        foreach (var dto in vectorOnlyStories)
        {
            if (!storyEntities.TryGetValue(dto.id, out var entity)) continue;

            string miniSummary = await GetBulletSummaryCached(entity, localBulletSummary);
            storyContent.AppendLine($"\n---\n**Title**: {dto.Title}");
            storyContent.AppendLine($"**Summary**: {miniSummary}");

            var background = dto.BackgroundInfo != null && dto.BackgroundInfo.Any()
                ? string.Join("\n- ", dto.BackgroundInfo.Select(b => Truncate(b, 150)))
                : "No background information available.";

            storyContent.AppendLine("**Background Info**:\n" + background);
        }
    }

    if (storyContent.Length > 0)
    {
        messages.Add(new { role = "system", content = storyContent.ToString() });
    }

    
    var liveSummary = await GenerateLiveMessageSummaryAsync(sessionId);
    messages.Add(new
    {
        role = "system",
        content = $"Here's a quick summary of the recent conversation:\n{liveSummary}"
    });
    
    var allStoryTexts = userStories.Concat(vectorOnlyStories)
        .SelectMany(s => new[] { s.Title }.Concat(s.BackgroundInfo ?? new List<string>()))
        .Distinct()
        .ToList();

    if (allStoryTexts.Any())
    {
        var tags = string.Join(", ", allStoryTexts.Take(10));
        messages.Add(new { role = "system", content = $"Topic references: {tags}" });
    }
    
    messages.Add(new { role = "user", content = prompt });
    
    Console.WriteLine($"[DEBUG] includeOnlySelected: {includeOnlySelected}");
    Console.WriteLine($"[DEBUG] Included user-selected stories: {userStories.Count}");
    Console.WriteLine($"[DEBUG] Included vector stories: {(includeOnlySelected ? 0 : vectorOnlyStories.Count)}");

    var serializedMessages = JsonSerializer.Serialize(messages, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine("[DEBUG] Final OpenAI messages payload:\n" + serializedMessages);

    return messages;
}


 
    internal async Task<string?> CallChatGptApi(List<object> messages)
    {
        var requestPayload = new
        {
            model = "gpt-4o-mini",
            messages = messages,
            max_tokens = 500,
            temperature = 0.5
        };

        var jsonContent = JsonSerializer.Serialize(requestPayload, new JsonSerializerOptions { WriteIndented = true });

        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[ERROR] OpenAI API Error: {errorContent}");
            throw new Exception($"OpenAI Error: {errorContent}");
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }

    internal async Task SaveChatMessages(int sessionId, string userId, string userMessage, string botReply, double[] embeddingVector)
    {
        var userMsg = new ChatMessageEntity
        {
            user_id = userId,
            bot = false,
            session_id = sessionId,
            text = userMessage,
            timestamp = DateTime.UtcNow
        };

        var botMsg = new ChatMessageEntity
        {
            user_id = null,  
            bot = true,   
            session_id = sessionId,
            text = botReply,
            timestamp = DateTime.UtcNow
        };

        _context.ChatMessages.Add(userMsg);
        _context.ChatMessages.Add(botMsg);
        await _context.SaveChangesAsync();

        var userEmbedding = new ChatMessageEmbeddingEntity
        {
            ChatMessageId = userMsg.id,
            Embedding = embeddingVector
        };

        var botEmbedding = new ChatMessageEmbeddingEntity
        {
            ChatMessageId = botMsg.id,
            Embedding = await GetEmbeddingForPrompt(botReply)
        };

        _context.ChatMessageEmbeddings.Add(userEmbedding);
        _context.ChatMessageEmbeddings.Add(botEmbedding);
        await _context.SaveChangesAsync();

        userMsg.ChatMessageEmbeddingId = userEmbedding.Id;
        botMsg.ChatMessageEmbeddingId = botEmbedding.Id;
        await _context.SaveChangesAsync();
    }

    internal async Task UpdateSessionTitleIfFirstMessage(int sessionId, string firstMessage)
    {
        var session = await _context.Sessions.FindAsync(sessionId);

        if (session == null)
        {
            Console.WriteLine($"Session {sessionId} not found!");
            return;
        }

        var userMessageCount = await _context.ChatMessages
            .Where(m => m.session_id == sessionId && !m.bot)
            .CountAsync();

        if (userMessageCount == 1) // Only update for the first message
        {
            string generatedTitle = await GenerateSessionTitle(firstMessage);
            session.title = generatedTitle;
            await _context.SaveChangesAsync();
            Console.WriteLine($"Updated session {sessionId} title: {generatedTitle}");
        }

        _context.Entry(session).Reload();
    }

    internal async Task<string> GenerateSessionTitle(string text)
    {
        try
        {
            var requestPayload = new
            {
                model = "gpt-4o",
                messages = new List<object>
                {
                    new { role = "system", content = "Generate a short session title (5 words max) for this message:" },
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
                return "Untitled Session";
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            string generatedTitle = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()?.Trim();

            return string.IsNullOrEmpty(generatedTitle) ? "Untitled Session" : generatedTitle;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating title: {ex.Message}");
            return "Untitled Session";
        }
    }

    internal async Task UpdateSessionSummaryIfNeeded(int sessionId)
    {
        // Get the total number of characters in this session
        var totalCharacterCount = await _context.ChatMessages
            .Where(m => m.session_id == sessionId)
            .SumAsync(m => m.text.Length);

        // Fetch the last processed character count (how many characters were already summarized)
        var sessionSummary = await _context.SessionsSummaries
            .FirstOrDefaultAsync(s => s.session_id == sessionId);

        int lastSummarizedCount = sessionSummary?.last_character_count ?? 0;

        // Check how many new characters have been added since the last summary
        int newCharacterCount = totalCharacterCount - lastSummarizedCount;

        // Trigger summaries for every 5000 characters added
        while (newCharacterCount >= 5000)
        {
            Console.WriteLine($"[INFO] Session {sessionId} has reached {lastSummarizedCount + 5000} characters. Generating summary...");

            await _embeddingService.GenerateOrUpdateSessionSummaryAsync(sessionId);

            lastSummarizedCount += 5000;
            newCharacterCount -= 5000;

            // Update the last processed character count in the database
            await UpdateLastCharacterCount(sessionId, lastSummarizedCount);
        }
    }

    internal async Task UpdateLastCharacterCount(int sessionId, int lastCharacterCount)
    {
        var sessionSummary = await _context.SessionsSummaries
            .FirstOrDefaultAsync(s => s.session_id == sessionId);

        if (sessionSummary != null)
        {
            sessionSummary.last_character_count = lastCharacterCount;
            sessionSummary.summary_text += "\n[Summary Updated]";
        }
        else
        {
            Console.WriteLine($"[WARN] No summary found for session {sessionId}. Creating a new one...");
            _context.SessionsSummaries.Add(new SessionSummaryEntity
            {
                session_id = sessionId,
                last_character_count = lastCharacterCount,
                summary_text = "[Summary Created]"
            });
        }

        await _context.SaveChangesAsync();
    }

  public async Task<string> DefaultOpenAIAsync(ChatRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Prompt))
        throw new InvalidPromptException(request.Prompt);


    var session = await _context.Sessions.FirstOrDefaultAsync(s => s.session_id == request.SessionId);
    if (session == null)
        throw new NoSuchSessionException(request.SessionId);

    var user = await _context.UserEntities.FirstOrDefaultAsync(u => u.sub == request.UserId);
    if (user == null)
        throw new NoSuchUserException(request.UserId);

 
    var pastMessages = await _context.ChatMessages
        .Where(m => m.session_id == request.SessionId)
        .OrderByDescending(m => m.timestamp)
        .Take(10)
        .ToListAsync();

    pastMessages.Reverse(); 

    var messages = new List<object>
    {
        new { role = "system", content = "You are a helpful assistant." }
    };

    foreach (var msg in pastMessages)
    {
        messages.Add(new
        {
            role = msg.bot ? "assistant" : "user",
            content = msg.text
        });
    }

    messages.Add(new { role = "user", content = request.Prompt });


    var requestPayload = new
    {
        model = "gpt-4o",
        messages = messages,
        max_tokens = 500,
        temperature = 0.7,
        top_p = 0.95
    };

    var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine("[ERROR] OpenAI API Error:", error);
        throw new Exception($"OpenAI Error: {error}");
    }

    var responseContent = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
    var botReply = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

    
    var userMessage = new ChatMessageEntity
    {
        session_id = request.SessionId,
        user_id = request.UserId,
        text = request.Prompt,
        bot = false,
        timestamp = DateTime.UtcNow
    };

    var botMessage = new ChatMessageEntity
    {
        session_id = request.SessionId,
        user_id = null,
        text = botReply,
        bot = true,
        timestamp = DateTime.UtcNow
    };

    
    _context.ChatMessages.Add(userMessage);
    _context.ChatMessages.Add(botMessage);
    await _context.SaveChangesAsync();

    await UpdateSessionTitleIfFirstMessage(request.SessionId, request.Prompt);
    return botReply;
}
  
internal async Task<string> GenerateLiveMessageSummaryAsync(int sessionId)
{
    var recentMessages = await _context.ChatMessages
        .Where(m => m.session_id == sessionId)
        .OrderByDescending(m => m.timestamp)
        .Take(15)
        .ToListAsync();

    recentMessages.Reverse(); 

    if (!recentMessages.Any())
        return "[No messages to summarize]";

    var conversation = new StringBuilder();
    foreach (var msg in recentMessages)
    {
        string role = msg.bot ? "Assistant" : "User";
        conversation.AppendLine($"{role}: {msg.text}");
    }

    var prompt = new List<object>
    {
        new { role = "system", content = "Summarize the following conversation in a **very concise way**. Use short phrases or keywords, not full sentences. Maximum 3 short bullet points" },
        new { role = "user", content = conversation.ToString() }
    };

    var payload = new
    {
        model = "gpt-4o",
        messages = prompt,
        max_tokens = 180,
        temperature = 0.5
    };

    var json = JsonSerializer.Serialize(payload);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

    var responseBody = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine("[ERROR] Failed to summarize inline messages: " + responseBody);
        return "[Live summary failed]";
    }

    var result = JsonDocument.Parse(responseBody);
    return result.RootElement
        .GetProperty("choices")[0]
        .GetProperty("message")
        .GetProperty("content")
        .GetString()
        ?.Trim() ?? "[Empty summary]";
}

public async Task<string> SummarizeSummaryAsBullets(string fullSummary)
{
    var requestPayload = new
    {
        model = "gpt-4o",
        messages = new List<object>
        {
            new { role = "system", content = "Rewrite the following session summary as 3 concise bullet points (max 300 characters total):" },
            new { role = "user", content = fullSummary }
        },
        max_tokens = 150,
        temperature = 0.5
    };

    var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

    if (!response.IsSuccessStatusCode)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[ERROR] OpenAI Summary Bullet Compression Error: {errorContent}");
        return null!;
    }

    var responseContent = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
    var bulletSummary = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

    return bulletSummary?.Trim() ?? "";
}

private async Task<string> GetBulletSummaryCached(Story story, Dictionary<string, string> localCache)
{
    if (localCache.TryGetValue(story.id, out var cached))
    {
        return cached;
    }

    string sourceText = story.storySummary ?? story.storySummary ?? "";
    if (string.IsNullOrWhiteSpace(sourceText))
        return "[No summary available]";

    string summary = await SummarizeSummaryAsBullets(sourceText);
    localCache[story.id] = summary;

    return summary;
}


}
