using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Entities;

namespace Testrepository.Server.Services.UserService
{
    public interface IUserService
    {
        Task<HandshakeDTO> Handshake(string sub);
        Task<UserEntity> GetUserBySubAsync(string sub);
        
        Task UpdateCurrentProject(string sub, int currentProjectId);
    }
}
