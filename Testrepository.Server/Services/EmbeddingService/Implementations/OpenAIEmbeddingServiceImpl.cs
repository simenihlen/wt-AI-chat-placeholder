using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenAI;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;
using System.Collections.Generic;
using Testrepository.Server.Services.EmbeddingService;
using Npgsql;


public class OpenAIEmbeddingServiceImpl : IEmbeddingService
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _embeddingModel;
    private readonly DatabaseContext _context;
    private readonly HttpClient _httpClient;

    public OpenAIEmbeddingServiceImpl(IConfiguration configuration, DatabaseContext context)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception("OpenAI API key is not configured.");
        }

        _embeddingModel = "text-embedding-ada-002"; 
        _openAIClient = new OpenAIClient(apiKey);
        _context = context;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    /// <summary>
    /// Generates an embedding vector for a given text input.
    /// </summary>
    public async Task<double[]> GetEmbeddingAsync(string text)
    {
      
        text = text.Trim().ToLower();             
        text = text.Replace("\n", " ");           
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " "); 
        text = new string(text.Where(c => !char.IsPunctuation(c)).ToArray()); 
        
        var embeddingClient = _openAIClient.GetEmbeddingClient(_embeddingModel);
        var response = await embeddingClient.GenerateEmbeddingAsync(text);

        return response.Value.ToFloats().ToArray().Select(x => (double)x).ToArray(); 
    }


    /// <summary>
    /// Finds the most similar past chat messages using cosine similarity.
    /// </summary>
    public async Task<List<ChatMessageEntity>> GetSimilarMessagesAsync(double[] queryEmbedding, int session_id, int limit = 5)
    {
        var queryEmbeddingText = "ARRAY[" + string.Join(",", queryEmbedding) + "]::vector";

        var messages = await _context.ChatMessages
            .FromSqlRaw($@"
            SELECT c.*
            FROM testschema.chat_messages c
            JOIN testschema.chat_message_embeddings e ON c.id = e.chatmessageid
            WHERE c.session_id = @session_id
            ORDER BY (e.embedding::vector <=> {queryEmbeddingText}) ASC
            LIMIT {limit}",
                new NpgsqlParameter("session_id", session_id))
            .ToListAsync();

        return messages;
    }

    
    public async Task GenerateSessionSummaryAsync(int sessionId)
    {
        var messages = await _context.ChatMessages
            .Where(m => m.session_id == sessionId)
            .OrderBy(m => m.timestamp)
            .Select(m => m.text)
            .ToListAsync();

        var sessionText = string.Join(" ", messages);

        // Send the session text to OpenAI for summarization
        var summary = await GetSummaryFromOpenAI(sessionText);

        var embedding = await GetEmbeddingAsync(summary); // Embed the summary

        if (!_context.SessionsSummaries.Any(s => s.session_id == sessionId))
        {
            var sessionSummary = new SessionSummaryEntity
            {
                session_id = sessionId,
                summary_text = summary,
                embedding = embedding
            };
        

        _context.SessionsSummaries.Add(sessionSummary);
        await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GetSummaryFromOpenAI(string text)
    {
        var requestPayload = new
        {
            model = "gpt-4o",
            messages = new List<object>
            {
                new { role = "system", content =  "Rewrite the following session summary as 3 concise bullet points (max 300 characters total):" },
                new { role = "user", content = text }
            },
            max_tokens = 200,
            temperature = 0.2
        };

        var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"OpenAI Summary Error: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
        return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }


    public async Task<List<SessionSummaryEntity>> GetSimilarSessionSummariesAsync(string userId, int projectId, double[] queryEmbedding, int limit = 5)
    {
        var queryEmbeddingText = "ARRAY[" + string.Join(",", queryEmbedding) + "]::vector";

        var summaries = await _context.SessionsSummaries
            .FromSqlRaw($@"
            SELECT ss.* 
            FROM testschema.session_summaries ss
            INNER JOIN testschema.sessions s ON ss.session_id = s.id
            WHERE s.user_id = @userId AND s.project_id = @projectId
            ORDER BY (ss.embedding::vector <=> {queryEmbeddingText}) ASC
            LIMIT {limit}",
                new NpgsqlParameter("userId", userId),
                new NpgsqlParameter("projectId", projectId))
            .ToListAsync();

        return summaries;
    }


    public async Task GenerateOrUpdateSessionSummaryAsync(int sessionId)
    {
        var messages = await _context.ChatMessages
            .Where(m => m.session_id == sessionId)
            .OrderBy(m => m.timestamp)
            .Select(m => m.text)
            .ToListAsync();

        // ✅ Limit text to last 50,000 characters to avoid large payloads
        var fullText = string.Join(" ", messages);
        var sessionText = fullText.Length > 5_000 ? fullText[^5_000..] : fullText;

        if (string.IsNullOrWhiteSpace(sessionText))
        {
            Console.WriteLine($"[WARNING] Session {sessionId} has no messages to summarize.");
            return;
        }

        // ✅ Generate summary using OpenAI
        var summary = await GetSummaryFromOpenAI(sessionText);

        if (string.IsNullOrWhiteSpace(summary))
        {
            Console.WriteLine($"[ERROR] OpenAI returned an empty summary for session {sessionId}.");
            return;
        }

        // ✅ Generate an embedding for the summary
        var embedding = await GetEmbeddingAsync(summary);

        // ✅ Check if a summary already exists for this session
        var existingSummary = await _context.SessionsSummaries
            .FirstOrDefaultAsync(s => s.session_id == sessionId);

        if (existingSummary != null)
        {
            existingSummary.summary_text = summary;
            existingSummary.embedding = embedding;
        }
        else
        {
            var sessionSummary = new SessionSummaryEntity
            {
                session_id = sessionId,
                summary_text = summary,
                embedding = embedding
            };
            _context.SessionsSummaries.Add(sessionSummary);
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"[INFO] Session {sessionId} summary updated successfully.");
    }




}
