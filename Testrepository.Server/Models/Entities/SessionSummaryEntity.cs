using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Testrepository.Server.Models.Entities;

[Table("session_summaries", Schema = "testschema")]
public class SessionSummaryEntity
{
    [Key]
    public int id { get; set; }

    [Column("session_id")]
    public int session_id { get; set; } // Link to the session

    [Column("summary_text")]
    public string summary_text { get; set; } // The textual summary

    public int last_character_count { get; set; } = 0;

    [Column("embedding", TypeName = "float8[]")]
    public double[] embedding { get; set; } // Embedding of the summary

    public SessionEntity Session { get; set; }
}