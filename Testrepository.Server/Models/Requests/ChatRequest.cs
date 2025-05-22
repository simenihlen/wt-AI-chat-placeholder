using Testrepository.Server.Models.DTO;

namespace Testrepository.Server.Models.Requests
{
    public class ChatRequest
    {
        public int SessionId { get; set; }
        public string UserId { get; set; }
        public string Prompt { get; set; }

        public List<StoryDTO> Stories { get; set; } = new List<StoryDTO>();
        
        public bool includeOnlySelected { get; set; }

    }
}