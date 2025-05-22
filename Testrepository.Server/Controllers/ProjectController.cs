using Microsoft.AspNetCore.Mvc;
using Testrepository.Server.Exceptions;
using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Services.ProjectService;
using Testrepository.Server.Services.StoryService;
using Testrepository.Server.Services.UserService;
using Testrepository.Server.Services.UserService.Implementations;

namespace Testrepository.Server.Controllers
{
    [Route("project")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IStoryService _storyService;

        public ProjectController(IProjectService projectService, IStoryService storyService)
        {
            this._projectService = projectService;
            this._storyService = storyService;
        }
        [HttpPost("CreateProject")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDTO createProjectDto)
        {
            try
            {
                ProjectDTO createdProject = await _projectService.CreateProject(createProjectDto);
                return Ok(createdProject);
            }
            catch (NoSuchUserException e)
            {
                return NotFound(e.Message);
            }
            catch (NoTitleException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
        [HttpGet("GetProjectsByUserId/{sub}")]
        public async Task<IActionResult> GetProjectsByUserId(string sub)
        {
            try
            {
                List<ProjectDTO> projects = await _projectService.GetProjects(sub);
                return Ok(projects);
            }
            catch (NoSuchUserException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
        [HttpGet("GetProjectById/{projectId}")]
        public async Task<ActionResult<ProjectDTO>> GetProjectById(int projectId)
        {
            try
            {
                ProjectDTO project = await _projectService.GetProjectByIdAsync(projectId);
                return Ok(project);
            }
            catch (NoSuchProjectException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
        [HttpPut("UpdateProject")]
        public async Task<IActionResult> UpdateProject([FromBody] ProjectDTO projdto)
        {
            try
            {
                ProjectDTO updated = await _projectService.UpdateProject(projdto);
                return Ok(updated);
            }
            catch (NoSuchProjectException e)
            {
                return NotFound(e.Message);
            }
            catch (NoTitleException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpDelete("DeleteProject/{projectId}")]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            try
            {
                ProjectDTO pros = await _projectService.DeleteProject(projectId);
                return Ok(pros);
            }
            catch (NoSuchProjectException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}