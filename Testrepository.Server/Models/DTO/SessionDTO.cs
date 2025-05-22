using Testrepository.Server.Models.Entities;

namespace Testrepository.Server.Models.DTO;

public class SessionDTO
{
    public int id { get; set; }
    public DateTime created_at { get; set; }
    public string? user_id { get; set; }
    
    public string? title { get; set; }
    public List<ChatMessageDTO>? Messages { get; set; }
    public SessionDTO()
    {
        this.id = 0;
        this.created_at = DateTime.Now;
        this.user_id = "";
        this.title = "";
        this.Messages = new List<ChatMessageDTO>();
    }
    public SessionDTO(SessionEntity? currentSession)
    {
        this.id = currentSession?.session_id ?? 0;
        this.created_at = currentSession?.created_at ?? DateTime.MinValue;
        this.user_id = currentSession?.user_id;
        this.title = currentSession?.title;
        this.Messages = currentSession?.Messages
            ?.Select(m => new ChatMessageDTO(m))
            .ToList();
    }
}