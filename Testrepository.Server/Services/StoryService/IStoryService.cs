using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Entities;

namespace Testrepository.Server.Services.StoryService
{
    public interface IStoryService
    {
        /*Task<List<StoryDTO>> ProcessStoriesForSessionAsync(int sessionId, List<StoryDTO> receivedStories); */
        public Task<StoryDTO> StoryHandshake(StoryDTO storyDTO);
        public Task<StoryDTO> Verify(string storyId);
        public Task<string> SummarizeStoryText(string text);
        public Task<List<Story>> GetSimilarStoriesAsync(double[] queryEmbedding, string userId, int projectId,
            int limit);

        public Task<List<CurrentProjectStoryDTO>> GetStoriesForCurrentProject(int projectId);
        
        public Task DeleteStoryAsync(string storyId);

    }
}
