using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Testrepository.Server.Models.Entities;



public class UserEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string sub { get; set; }
    
    public ICollection<ProjectEntity> projects { get; set; } = new List<ProjectEntity>();

    public int? currentProjectId { get; set; }
    public ProjectEntity? CurrentProject { get; set; }
    
    public int? defaultProjectId { get; set; }
    public ProjectEntity? DefaultProject { get; set; }
}