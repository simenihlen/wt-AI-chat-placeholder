using Testrepository.Server.Models.Entities;

namespace Testrepository.Server.Models.DTO
{
    public class HandshakeDTO
    {
        public string sub { get; set; }
        public ProjectDTO currentProject { get; set; }
        
        public ProjectDTO defaultProject { get; set; }
        
        public List<ProjectDTO> allProjects { get; set;  }

        public HandshakeDTO()
        {
            sub = string.Empty;
            currentProject = new ProjectDTO();  
        }

        public HandshakeDTO(UserEntity userEntity) {
            sub = userEntity.sub;
            currentProject = new ProjectDTO(userEntity.CurrentProject);
            defaultProject = new ProjectDTO(userEntity.DefaultProject);
            allProjects = userEntity.projects.Select(p => new ProjectDTO(p)).ToList();
        }
    }
}