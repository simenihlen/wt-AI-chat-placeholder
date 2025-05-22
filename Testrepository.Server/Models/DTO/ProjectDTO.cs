using System.Transactions;
using Testrepository.Server.Models.Entities;

namespace Testrepository.Server.Models.DTO
{
    public class ProjectDTO
    {
        public int? id { get; set; }
        public string sub { get; set; }
        public string title { get; set; }
      
   
        public int? currentSessionId { get; set; }
      
      
        public List<SessionDTO> sessions { get; set; } = new List<SessionDTO>();
        public List<int> sessionIds { get; set; } = new List<int>();
        public List<StoryDTO> stories { get; set; } = new List<StoryDTO>();
        public List<string> storyIds { get; set; } = new List<string>();
      
        public ProjectDTO()
        {
            sub = "";
            title = "";
        }
        public ProjectDTO(ProjectEntity currentProject)
        {
            id = currentProject.id;
            sub = currentProject.sub;
            title = currentProject.title;
            sessionIds = currentProject.sessions.Select(s => s.session_id).ToList();
            currentSessionId = currentProject.CurrentSessionId;
            storyIds = currentProject.projectStories.Select(ps => ps.StoryId).ToList();
        }
    }
}