using System.Text.Json.Serialization;

namespace Testrepository.Server.Models.DTO
{
    public class StoryDTO
    {
        public string id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        
        public string sub { get; set; }

        [JsonPropertyName("background")] public List<string> BackgroundInfo { get; set; } = new();
    }
}
