using Microsoft.AspNetCore.Mvc;
using Testrepository.Server.Exceptions;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Services.UserService;
using Testrepository.Server.Services.UserService.Implementations;

namespace Testrepository.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
    
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
    
        [HttpGet("{sub}")]
        public async Task<IActionResult> GetUserById(string sub) 
        {
            try
            {
                var user = await _userService.GetUserBySubAsync(sub);
                return Ok(user);
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
            
        [HttpPost("handshake")]
        public async Task<IActionResult> Handshake([FromBody] string userIdString)
        {
            try
            {
                var userDto = await _userService.Handshake(userIdString);
                return Ok(userDto);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }

        }

        [HttpPost("set-current-project")]
        public async Task<IActionResult> SetCurrentProject(string sub, int currentProjectId)
        {
            try
            {
                await _userService.UpdateCurrentProject(sub, currentProjectId);
                return Ok();
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
    }
}