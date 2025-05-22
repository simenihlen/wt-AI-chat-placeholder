using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;
using Pgvector.EntityFrameworkCore;

using Testrepository.Server.Models.Entities;

[Table("chat_message_embeddings", Schema = "testschema")]
public class ChatMessageEmbeddingEntity
{
    [Key]
    public int Id { get; set; }
    
    [Column("chatmessageid")]
    public int ChatMessageId { get; set; }

    [Column("embedding", TypeName = "float8[]")] // Store as PostgreSQL float array
    public double[] Embedding { get; set; } = Array.Empty<double>();
    
    [ForeignKey(nameof(ChatMessageId))]
    public virtual ChatMessageEntity ChatMessage { get; set; }
}