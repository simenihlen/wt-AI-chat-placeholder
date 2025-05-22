using LibGit2Sharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Testrepository.Server.Exceptions;
using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;

namespace Testrepository.Server.Services.ProjectService.Implementations
{
    public class ProjectServiceImpl : IProjectService
    {
        private readonly DatabaseContext _databaseContext;
        public ProjectServiceImpl(DatabaseContext databaseContext)
        {
            this._databaseContext = databaseContext;
        }
        public async Task<ProjectDTO> CreateProject(CreateProjectDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.sub))
                throw new ArgumentException("User ID (sub) cannot be empty.");
            if (string.IsNullOrWhiteSpace(request.title))
                throw new ArgumentException("Title cannot be empty.");
            var user = await _databaseContext.UserEntities.FindAsync(request.sub);
            if (user == null)
                throw new NoSuchUserException(request.sub);
            ProjectEntity projectEntity = new ProjectEntity
            {
                title = request.title, // Set title from frontend
                sub = request.sub,
                sessions = new List<SessionEntity>(), // Empty list
                projectStories = new List<ProjectStories>() // Empty list
            };
            _databaseContext.ProjectEntities.Add(projectEntity);
            await _databaseContext.SaveChangesAsync();
            return new ProjectDTO
            {
                id = projectEntity.id,
                sub = projectEntity.sub,
                title = projectEntity.title,
                sessions = new List<SessionDTO>(), // Empty list
                stories = new List<StoryDTO>(),        // Empty list
                storyIds = new List<string>()
                
            };
        }
       
         public async Task<List<ProjectDTO>> GetProjects(string sub)
        {
            var user = await _databaseContext.UserEntities.FindAsync(sub);
            if (user == null)
                throw new NoSuchUserException(sub);

            return await _databaseContext.ProjectEntities
                .AsSplitQuery()
                .Where(p => p.sub == sub)
                .Include(p => p.sessions)
                .ThenInclude(s => s.Messages) 
                .Include(p => p.projectStories)
                    .ThenInclude(ps => ps.Story)
                .Select(p => new ProjectDTO
                {
                    id = p.id,
                    title = p.title,
                    sub = p.sub,
                    currentSessionId = p.CurrentSessionId,
                    sessions = p.sessions.Select(s => new SessionDTO
                    {
                        id = s.session_id,
                        title = s.title,
                        created_at = s.created_at,

                        Messages = s.Messages.Select(m => new ChatMessageDTO
                        {
                            
                            text = m.text,
                            timestamp = m.timestamp,
                            sender = m.bot ? "bot" : m.user_id ?? "No one",
                            session_id = m.session_id
                        }).ToList()
                    }).ToList(),

                    stories = p.projectStories
                        .Where(ps => ps.Story != null)
                        .Select(ps => new StoryDTO 
                    { 
                        id = ps.Story.id, 
                        Title = ps.Story.title,
                    }).ToList(),
                    storyIds = p.projectStories.Select(ps => ps.Story.id).ToList()
                })
                .ToListAsync(); 
        }


        public async Task<ProjectDTO> GetProjectByIdAsync(int projectId)
        {
            var project = await _databaseContext.ProjectEntities
                .AsSplitQuery()
                .Include(p => p.sessions)
                .ThenInclude(s => s.Messages) 
                .Include(p => p.projectStories)
                .ThenInclude(ps => ps.Story)
                .FirstOrDefaultAsync(p => p.id == projectId);

            if (project == null)
            {
                throw new NotFoundException("Project not found");
            }

            return new ProjectDTO
            {
                id = project.id,
                sub = project.sub,
                title = project.title,
                currentSessionId = project.CurrentSessionId,
                sessions = project.sessions.Select(s => new SessionDTO
                {
                    id = s.session_id,
                    title = s.title,
                    created_at = s.created_at,
                    Messages = s.Messages.Select(m => new ChatMessageDTO
                    {
                        text = m.text,
                        timestamp = m.timestamp,
                        sender = m.bot ? "bot" : m.user_id ?? "No one",
                        session_id = m.session_id
                    }).ToList()
                }).ToList(),

                
                stories = project.projectStories
                    .Where(ps => ps.Story != null)
                    .Select(ps => new StoryDTO 
                { 
                    id = ps.Story.id, 
                    Title = ps.Story.title, 
                }).ToList(),
                storyIds = project.projectStories.Select(ps => ps.Story.id).ToList()
            };
        }

        
        public async Task<ProjectDTO> UpdateProject(ProjectDTO projdto)
        {
            ProjectEntity? projectEntity = await _databaseContext.ProjectEntities.FindAsync(projdto.id);
            if (projectEntity == null)
                throw new NoSuchProjectException(projdto.id ?? 0);
            if (string.IsNullOrWhiteSpace(projdto.title))
                throw new NoTitleException(projdto.title);
            projectEntity.title = projdto.title;
            _databaseContext.ProjectEntities.Update(projectEntity);
            await _databaseContext.SaveChangesAsync();
            return projdto;
        }
        public async Task<ProjectDTO> DeleteProject(int projectId)
        {
            var toBeDeleted = await _databaseContext.ProjectEntities.FindAsync(projectId);
            if (toBeDeleted == null)
            {
                throw new NoSuchProjectException(projectId);
            }
            
            var affectedUsers = await _databaseContext.UserEntities
                .Where(u => u.currentProjectId == projectId)
                .ToListAsync();

           
            foreach (var user in affectedUsers)
            {
                user.currentProjectId = null;
            }
            
            await _databaseContext.SaveChangesAsync();
            _databaseContext.ProjectEntities.Remove(toBeDeleted);
            await _databaseContext.SaveChangesAsync();

            return new ProjectDTO
            {
                id = toBeDeleted.id,
                title = toBeDeleted.title,
                sub = toBeDeleted.sub
            };
        }

    }
}