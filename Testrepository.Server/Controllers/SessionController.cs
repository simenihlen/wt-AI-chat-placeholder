using Microsoft.AspNetCore.Mvc;
using Testrepository.Server.Exceptions;
using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Entities;
using Testrepository.Server.Services.SessionService;
using Testrepository.Server.Services.SessionService.Implementations;

namespace Testrepository.Server.Controllers;

[ApiController]
[Route("sessions")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionDTO request)
    {
        
        Console.WriteLine($"Received Project ID: {request?.projectId}");
        if (request == null || request.projectId <= 0)
        {
            return BadRequest("Invalid Project ID");
        }
        
        try
        {
            var session = await _sessionService.CreateSessionAsync(request);
            return Ok(session);
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
    
    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetSessionById(int sessionId)
    {
        try
        {
            var session = await _sessionService.GetSessionByIdAsync(sessionId);
            return Ok(session);
        }
        catch (NoSuchSessionException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

   
   
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetSessionsByUserId(string userId)
    {
        try
        {
            var sessions = await _sessionService.GetSessionsByUserIdAsync(userId);
            return Ok(sessions);
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
    
  
    [HttpPut("{sessionId}")]
    public async Task<IActionResult> UpdateSession(int sessionId, [FromQuery] string userId, [FromBody] List<ChatMessageDTO> messages) 
    {
        try
        {
            var updatedMessages = await _sessionService.AddMessagesToSessionAsync(sessionId, userId, messages);
            return Ok(updatedMessages);
        }
        catch (NoSuchUserException e)
        {
            return NotFound(e.Message);
        }
        catch (NoSuchSessionException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("delete/{sessionId}/{userId}")]
    public async Task<IActionResult> RemoveSessionAsync(int sessionId, string userId)
    {
        try
        {
            Console.WriteLine($"Received DELETE request for sessionId: {sessionId}, userId: {userId}");
            var deletedSession = await _sessionService.RemoveSessionAsync(sessionId, userId);
            return Ok(deletedSession);
        }
        catch (NoSuchUserException e) {
            return NotFound(e.Message);
        }
        catch (NoSuchSessionException e) {
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}