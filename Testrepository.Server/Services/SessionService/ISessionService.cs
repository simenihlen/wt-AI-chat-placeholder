using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Entities;

namespace Testrepository.Server.Services.SessionService
{
    public interface ISessionService
    {
        Task<SessionEntity> CreateSessionAsync(CreateSessionDTO request);
        Task<SessionDTO?> GetSessionByIdAsync(int sessionId);
        Task<List<SessionDTO>> GetSessionsByUserIdAsync(string userId);
        
        Task<List<ChatMessageDTO>> AddMessagesToSessionAsync(int sessionId, string userId, List<ChatMessageDTO> messages);
        Task<bool> RemoveSessionAsync(int sessionId, string userId);

    }
}
