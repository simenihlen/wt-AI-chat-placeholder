using Testrepository.Server.Models.Entities;

namespace Testrepository.Server.Models.DTO;

public class ChatMessageDTO
{  

    public string sender { get; set; }
    public string text { get; set; }
    public DateTime timestamp { get; set; } = DateTime.UtcNow;
    public int session_id { get; set; }

    public ChatMessageDTO(ChatMessageEntity messageEntity)
    {
        sender = messageEntity.bot ? "bot" : messageEntity.user_id ?? "unknown"; 
        text = messageEntity.text;
        timestamp = messageEntity.timestamp;
        session_id = messageEntity.session_id;
    }
    public ChatMessageDTO()
    {
        sender = "unknown";
        text = string.Empty;
        timestamp = DateTime.UtcNow;
        session_id = 0;
    }
}