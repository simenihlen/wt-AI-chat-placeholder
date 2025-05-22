using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;
using Testrepository.Server.Services.EmbeddingService;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Models.DTO;
using Testrepository.Server.Exceptions;

namespace Testrepository.Server.Services.StoryService.Implementations
{
    public class StoryServiceImpl : IStoryService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly DatabaseContext _context;
        private readonly HttpClient _httpClient;

        public StoryServiceImpl(IEmbeddingService embeddingService, DatabaseContext context, IConfiguration configuration)
        {
            _embeddingService = embeddingService;
            _context = context;
            _httpClient = new HttpClient();

            var apiKey = configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception("OpenAI API key is not configured.");
            }

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        /// <summary>
        /// **Processes the received stories: Summarizes them immediately, then stores them.**
        /// </summary>
        /*
        public async Task<List<StoryDTO>> ProcessStoriesForSessionAsync(int projectId, List<StoryDTO> receivedStories)
        {
            Console.WriteLine($"[DEBUG] Processing {receivedStories.Count} stories for session {projectId}...");

            var summarizedStories = new List<StoryDTO>();

            foreach (var story in receivedStories)
            {
                Console.WriteLine($"[INFO] Summarizing story: {story.Title}");
                string summarizedText = await SummarizeStoryText(story.SummaryText);

                var summarizedStory = new StoryDTO
                {
                    Title = story.Title,
                    SummaryText = summarizedText,
                    CreatedAt = DateTime.UtcNow,
                    SessionId = projectId
                };

                summarizedStories.Add(summarizedStory);
            }

            // ✅ Store summarized stories in the database after OpenAI request
            foreach (var story in summarizedStories)
            {
                await StoreStorySummaryEmbedding(projectId, story.SummaryText);
            }

            return summarizedStories;
        }
        */
        /// <summary>
        /// Summarizes a given story text using OpenAI.
        /// </summary>
        public async Task<string> SummarizeStoryText(string text)
        {
            Console.WriteLine("[INFO] Sending story text for summarization.");
            var requestPayload = new
            {
                model = "gpt-4o-mini",
                messages = new List<object>
            {
                new { role = "system", content = "Summarize the following story text concisely." },
                new { role = "user", content = text }
            },
                max_tokens = 300,
                temperature = 0.7
            };

            var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR] OpenAI Story Summarization Error: {errorContent}");
                throw new Exception($"OpenAI Story Summarization Error: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            string summary = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            Console.WriteLine($"[INFO] Generated summary: {summary}");
            return summary;
        }

        /// <summary>
        /// Stores the summarized story in the database.
        /// </summary>
        /// 
        /**
        public async Task StoreStorySummaryEmbedding(int sessionId, string summaryText)
        {
            Console.WriteLine("[INFO] Generating embedding for the summarized story.");
            var embedding = await _embeddingService.GetEmbeddingAsync(summaryText);

            var story = new Story
            {
                title = "Auto-Summary",
                descrEmbedding = embedding,
                created_at = DateTime.UtcNow,
                project_id = sessionId

            };

            _context.Stories.Add(story);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[INFO] Stored summarized story in database for session {sessionId}.");
        }
        */
public async Task<StoryDTO> StoryHandshake(StoryDTO storyDTO)
{
    var userId = storyDTO.sub;

    var user = await _context.UserEntities.FirstOrDefaultAsync(u => u.sub == userId);
    if (user == null || user.currentProjectId == null)
        throw new Exception("User not found or currentProjectId is not set.");

    int currentProjectId = user.currentProjectId.Value;

    

    // Check if story already exists
    var existingStory = await _context.Stories
        .Include(s => s.backgroundInfo)
        .FirstOrDefaultAsync(s => s.id == storyDTO.id);

    
    if (existingStory != null)
    {
        // Ensure the story is linked to the current project
        var alreadyLinked = await _context.ProjectStories
            .AnyAsync(ps => ps.StoryId == existingStory.id && ps.ProjectId == currentProjectId);
        

        if (!alreadyLinked)
        {
            Console.WriteLine($"Linking existing story {existingStory.id} to project {currentProjectId}");
            _context.ProjectStories.Add(new ProjectStories
            {
                ProjectId = currentProjectId,
                StoryId = existingStory.id
            });

            await _context.SaveChangesAsync();
        }

        return new StoryDTO
        {
            id = existingStory.id,
            Title = existingStory.title,
            Description = existingStory.storySummary,
            BackgroundInfo = existingStory.backgroundInfo.Select(b => b.Text).ToList(),
            ProjectId = currentProjectId
        };
    }

    // Generate embeddings
    string descriptionSummary = await SummarizeStoryText(storyDTO.Description);
    var descriptionEmbedding = await _embeddingService.GetEmbeddingAsync(descriptionSummary);
    var descEmbedding = descriptionEmbedding;
    var backgroundEmbeddings = new List<double[]>();

    foreach (var text in storyDTO.BackgroundInfo)
    {
        var embedding = await _embeddingService.GetEmbeddingAsync(text);
        backgroundEmbeddings.Add(embedding);
    }

    var flattenedEmbeddings = backgroundEmbeddings.SelectMany(e => e).ToArray();

    var story = new Story
    {
        id = storyDTO.id,
        title = storyDTO.Title,
        descrEmbedding = descEmbedding,
        backgrndEmbedding = flattenedEmbeddings,
        storySummary = descriptionSummary
    };

    try
    {
        _context.Stories.Add(story);
        await _context.SaveChangesAsync();

        // Link the new story to the current project
        var projectStory = new ProjectStories
        {
            ProjectId = currentProjectId,
            StoryId = story.id
        };
        _context.ProjectStories.Add(projectStory);

        // Add background info
        foreach (var text in storyDTO.BackgroundInfo)
        {
            var backgroundSummary = await SummarizeStoryText(text);
            var embeddedSummary = await _embeddingService.GetEmbeddingAsync(backgroundSummary);
            
            backgroundEmbeddings.Add(embeddedSummary);
            
            var backgroundInfo = new BackgroundInfoEntity
            {
                StoryId = story.id,
                Text = backgroundSummary
            };
            _context.BackgroundInfos.Add(backgroundInfo);
        }

        await _context.SaveChangesAsync();
    }
    catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
    {
        Console.WriteLine($"[WARN] Duplicate insert for story ID {storyDTO.id}. Fetching instead.");
        return await Verify(storyDTO.id); // fallback in race condition
    }

    return new StoryDTO
    {
        id = story.id,
        Title = story.title,
        Description = story.storySummary,
        BackgroundInfo = storyDTO.BackgroundInfo,
        ProjectId = currentProjectId
    };
}



        public async Task<StoryDTO> Verify(string storyId)
        {
            var story = await _context.Stories.FindAsync(storyId);
            if (story == null)
            {
                throw new NoSuchStoryException("Story not found");
            }

            var storyDTO = new StoryDTO
            {
                id = story.id,
                Title = story.title,
                BackgroundInfo = story.backgroundInfo.Select(b => b.Text).ToList(),
                Description = story.storySummary,
                ProjectId = _context.ProjectStories
                    .Where(ps => ps.StoryId == story.id)
                    .Select(ps => ps.ProjectId)
                    .FirstOrDefault()

            };
            return storyDTO;
        }
        
        public async Task<List<Story>> GetSimilarStoriesAsync(double[] queryEmbedding, string userSub, int projectId, int limit = 5)
        {
            var queryEmbeddingText = "ARRAY[" + string.Join(",", queryEmbedding) + "]::vector"; 
            
            Console.WriteLine($"[DEBUG] Querying similar stories for user: {userSub}, project: {projectId}");


            var stories = await _context.Stories
                .FromSqlRaw($@"
        SELECT s.*
        FROM testschema.stories s
        JOIN testschema.project_stories ps ON s.id = ps.story_id
        JOIN testschema.projects p ON ps.project_id = p.id
        WHERE ps.project_id = @projectId
          AND p.sub = @userSub
        ORDER BY (s.description_embedding::vector <=> {queryEmbeddingText}) ASC
        LIMIT {limit}",
                    new NpgsqlParameter("projectId", projectId),
                    new NpgsqlParameter("userSub", userSub))
                .ToListAsync();


            Console.WriteLine($"[DEBUG] Retrieved {stories.Count} similar stories:");
            foreach (var story in stories)
            {
                Console.WriteLine($"[DEBUG] - {story.title} (Project ID: {story.projectStories.Select(ps => ps.ProjectId).FirstOrDefault()}, Story ID: {story.id})");
            }

            return stories;
        }

        public async Task<List<CurrentProjectStoryDTO>> GetStoriesForCurrentProject(int projectId)
        {   
            var currentProject = await _context.ProjectEntities
                .Include(p => p.projectStories)
                .FirstOrDefaultAsync(p => p.id == projectId);

            if (currentProject == null)
            {
                return new List<CurrentProjectStoryDTO>();
            }

            var stories = await _context.ProjectStories
                .Where(ps => ps.ProjectId == projectId)
                .Include(ps => ps.Story)
                .Select(ps => new CurrentProjectStoryDTO
                {
                    sub = ps.Story.id,
                    title = ps.Story.title
                })
                .ToListAsync();

            return stories;
        }

        public async Task DeleteStoryAsync(string storyId)
        {
            var story = await _context.Stories
                .Include(s => s.backgroundInfo)
                .Include(s => s.projectStories)
                .FirstOrDefaultAsync(s => s.id == storyId);

            if (story == null)
                throw new NoSuchStoryException($"Story with ID {storyId} not found.");

         
            _context.BackgroundInfos.RemoveRange(story.backgroundInfo);
            _context.ProjectStories.RemoveRange(story.projectStories);

            _context.Stories.Remove(story);
            await _context.SaveChangesAsync();
        }


    }
}