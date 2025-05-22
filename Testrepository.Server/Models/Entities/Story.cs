using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Testrepository.Server.Models.Entities
{
    public class Story
    {
        [Key]
        public string id { get; set; }

        [Column("title")]
        public string title { get; set; }
        
        public string storySummary { get; set; }

        [Column("embedding", TypeName = "float8[]")]
        public double[] descrEmbedding { get; set; }
        public double[] backgrndEmbedding { get; set; }
        
        public ICollection<ProjectStories> projectStories { get; set; } = new List<ProjectStories>();
        
        public List<BackgroundInfoEntity> backgroundInfo { get; set; } = new List<BackgroundInfoEntity>();

    }
}