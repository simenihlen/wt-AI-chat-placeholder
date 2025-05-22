using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Testrepository.Server.Exceptions;
using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Persistence.Internal.GeneratedArtifacts;
using Testrepository.Server.Services.SessionService;
using Testrepository.Server.Services.SessionService.Implementations;

namespace Testrepository.Server.Services.UserService.Implementations;

public class UserServiceImpl : IUserService
{
    private readonly DatabaseContext _context;
    private readonly ISessionService _sessionService;

    public UserServiceImpl(DatabaseContext context, ISessionService sessionService)
    {
        _context = context;
        
    }

   public async Task<HandshakeDTO> Handshake(string sub)
{
    // Get the user
    var user = await _context.UserEntities
        .AsSplitQuery()
        .FirstOrDefaultAsync(u => u.sub == sub);

    if (user == null)
    {
        user = new UserEntity
        {
            sub = sub,
            projects = new List<ProjectEntity>()
        };

        _context.UserEntities.Add(user);
        await _context.SaveChangesAsync();

        var newProject = new ProjectEntity
        {
            title = "New Project",
            sub = sub,
            user = user,
            sessions = new List<SessionEntity>(),
            projectStories = new List<ProjectStories>()
        };

        user.CurrentProject = newProject;
        user.DefaultProject = newProject;
        user.projects.Add(newProject);

        _context.ProjectEntities.Add(newProject);
        _context.UserEntities.Update(user);
        await _context.SaveChangesAsync();

        var currentSession = new SessionEntity
        {
            user_id = sub,
            project_id = newProject.id,
            ProjectEntity = newProject
        };

        newProject.sessions.Add(currentSession);
        newProject.currentSession = currentSession;

        _context.Sessions.Add(currentSession);
        _context.ProjectEntities.Update(newProject);
        await _context.SaveChangesAsync();
    }

    // Load all projects with projection
    var projects = await _context.ProjectEntities
        .AsSplitQuery()
        .Where(p => p.sub == sub)
        .Include(p => p.sessions)
        .ThenInclude(s => s.Messages)
        .Include(p => p.projectStories)
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
                    ProjectId = p.id
                }).ToList(),

        })
        .ToListAsync();

    var defaultProjectId = _context.UserEntities
        .Where(u => u.sub == sub)
        .Select(u => u.defaultProjectId)
        .FirstOrDefault();
    
    var currentProjectId = _context.UserEntities
        .Where(u => u.sub == sub)
        .Select(u => u.currentProjectId)
        .FirstOrDefault();
    
    var defaultProject = projects.FirstOrDefault(p => p.id == defaultProjectId);
    var currentProject = projects.FirstOrDefault(p => p.id == currentProjectId);

    var handshake = new HandshakeDTO
    {
        sub = user.sub,
        defaultProject = defaultProject,
        currentProject = currentProject,
        allProjects = projects
    };

    return handshake;
}

    // Renamed method to GetUserBySubAsync and updated parameter type to string
    public async Task<UserEntity> GetUserBySubAsync(string sub)
    {
        var user = await _context.UserEntities
            .Include(u => u.projects)
            .FirstOrDefaultAsync(u => u.sub == sub);
        if (user == null)
            throw new NoSuchUserException(sub);
        return user;
    }

    public async Task UpdateCurrentProject(string sub, int currentProjectId)
    {
        var user = await _context.UserEntities
            .FirstOrDefaultAsync(u => u.sub == sub);

        if (user == null)
            throw new Exception($"User with sub '{sub}' not found.");
        
        user.currentProjectId = currentProjectId;
        await _context.SaveChangesAsync();
        
    }
}