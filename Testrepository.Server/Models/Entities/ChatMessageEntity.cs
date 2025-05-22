using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Testrepository.Server.Models.Entities;

[Table("chat_messages")]
public class ChatMessageEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    
    [Column("user_id")]
    public string? user_id { get; set; }
    
    [Column("text")]
    public string text { get; set; }
    
    [Column("bot")]
    public bool bot { get; set; }
    
    [Column(TypeName = "timestamp")]
    public DateTime timestamp { get; set; } = DateTime.UtcNow;
    
    [Column("session_id")]
    public int session_id { get; set; }
    
    [ForeignKey("session_id")]
    public SessionEntity session { get; set; }
    
    [ForeignKey("user_id")]
    public UserEntity? User { get; set; }
    
    
    [Column("chatmessageembeddingid")]
    public int? ChatMessageEmbeddingId { get; set; }
    
    public virtual ChatMessageEmbeddingEntity? ChatMessageEmbedding { get; set; }
    
}