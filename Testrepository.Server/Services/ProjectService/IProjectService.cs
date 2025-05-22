using Testrepository.Server.Models.DTO;

namespace Testrepository.Server.Services.ProjectService
{
    public interface IProjectService
    {
        public Task<ProjectDTO> CreateProject(CreateProjectDTO request);
        public Task<ProjectDTO> DeleteProject(int projectId);
        public Task<List<ProjectDTO>> GetProjects(string sub);
        public Task<ProjectDTO> GetProjectByIdAsync(int projectId);
        public Task<ProjectDTO> UpdateProject(ProjectDTO projdto);
    }
}