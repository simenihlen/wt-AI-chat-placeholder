using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Testrepository.Server.Models.Entities;


public class ProjectEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    public string sub { get; set; }
    public UserEntity user { get; set; }  

    public int? CurrentSessionId { get; set; }  
    public SessionEntity currentSession { get; set; }  
    public ICollection<SessionEntity> sessions = new List<SessionEntity>();

    public ICollection<ProjectStories> projectStories { get; set; } = new List<ProjectStories>();

    public string title { get; set; }
}