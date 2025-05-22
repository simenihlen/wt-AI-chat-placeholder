using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Requests;

namespace Testrepository.Server.Services.ChatService;

public interface IChatService
{
    Task<string> GetResponseAsync(ChatRequest request, List<StoryDTO> stories);
    Task<string> DefaultOpenAIAsync(ChatRequest request);
}
