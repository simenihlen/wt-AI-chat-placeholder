using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Testrepository.Server.Models.Entities;

[Table("sessions")]
public class SessionEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int session_id { get; set; }

    [Column("created_at")]
    public DateTime created_at { get; set; } = DateTime.UtcNow;

    [Column("user_id")]
    public string user_id { get; set; }

    [Column("title")]
    public string? title { get; set; }

    public ICollection<ChatMessageEntity> Messages { get; set; } = new List<ChatMessageEntity>();

    public int project_id { get; set; }
    public ProjectEntity ProjectEntity { get; set; }

    public List<Story> Stories { get; set; } = new List<Story>();
}