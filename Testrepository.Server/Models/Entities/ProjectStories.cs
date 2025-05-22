using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Testrepository.Server.Models.Entities
{
    [Table("project_stories", Schema = "testschema")]
    public class ProjectStories
    {
        [Key] 
        public int Id { get; set; }

        [Column("project_id")]
        public int ProjectId { get; set; }
        public ProjectEntity Project { get; set; }

        [Column("story_id")]
        public string StoryId { get; set; }
        public Story Story { get; set; }
    }
}