using Testrepository.Server.Models.Entities;

namespace Testrepository.Server.Services.EmbeddingService
{
    public interface IEmbeddingService
    {
        Task<double[]> GetEmbeddingAsync(string text);
        Task<List<ChatMessageEntity>> GetSimilarMessagesAsync(double[] queryEmbedding, int session_id, int limit = 5);
        public Task<string> GetSummaryFromOpenAI(string text);
        Task GenerateSessionSummaryAsync(int sessionId);
        Task<List<SessionSummaryEntity>> GetSimilarSessionSummariesAsync(string user_id, int projectId, double[] queryEmbedding, int limit = 5);
        Task GenerateOrUpdateSessionSummaryAsync(int sessionId);

    }
}
