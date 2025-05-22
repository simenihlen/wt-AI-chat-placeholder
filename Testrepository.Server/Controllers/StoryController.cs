using Microsoft.AspNetCore.Mvc;
using Testrepository.Server.Exceptions;
using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Services.StoryService;
using Testrepository.Server.Services.UserService;
using Testrepository.Server.Services.UserService.Implementations;

namespace Testrepository.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StoryController : ControllerBase
    {
        private readonly IStoryService _storyService;
        public StoryController(IStoryService storyService)
        {
            this._storyService = storyService;
        }
        [HttpPost("storyHandshake")]
        public async Task<IActionResult> StoryHandshake([FromBody] StoryDTO storyDTO)
        {
            try
            {
                StoryDTO processed = await _storyService.StoryHandshake(storyDTO);
                return Ok(processed);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
        [HttpGet("verifyStory/{storyId}")]
        public async Task<IActionResult> Verify(string storyId)
        {
            try
            {
                StoryDTO story = await _storyService.Verify(storyId);
                return Ok(story);
            }
            catch (NoSuchStoryException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("currentProject/{projectId}")]
        public async Task<IActionResult> GetStoriesForCurrentProject(int projectId)
        {
            try
            {
                var stories = await _storyService.GetStoriesForCurrentProject(projectId);
                return Ok(stories);
            }
            catch (NoSuchStoryException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
        
        [HttpDelete("deleteStory/{storyId}")]
        public async Task<IActionResult> DeleteStory(string storyId)
        {
            try
            {
                await _storyService.DeleteStoryAsync(storyId);
                return NoContent(); 
            }
            catch (NoSuchStoryException e)
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