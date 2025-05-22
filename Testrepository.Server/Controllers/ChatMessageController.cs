using Microsoft.AspNetCore.Mvc;
using Testrepository.Server.Exceptions;
using Testrepository.Server.Models;
using Testrepository.Server.Models.DTO;
using Testrepository.Server.Models.Requests;
using Testrepository.Server.Services.ChatService;
using Testrepository.Server.Services.SessionService;
using Testrepository.Server.Services.SessionService.Implementations;

[ApiController]
[Route("api/openai-chat")] // Base route
public class ChatMessageController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ISessionService _sessionService;

    public ChatMessageController(IChatService chatService, ISessionService sessionService)
    {
        _chatService = chatService;
        _sessionService = sessionService;
    }
    


    /// <summary>
    /// Sends a prompt to the OpenAI Chat API with session tracking.
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var response = await _chatService.GetResponseAsync(request, request.Stories ?? new List<StoryDTO>());
            return Ok(new { Response = response });
        }
        catch (NoSuchUserException ex)
        {
            return NotFound(ex.Message);
        }
        catch (NoSuchSessionException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidPromptException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] OpenAI API failed: {ex.Message}");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("chat/default")]
    public async Task<IActionResult> ChatDefault([FromBody] ChatRequest request)
    {
        try
        {
            var response = await _chatService.DefaultOpenAIAsync(request);
            return Ok(new { response });
        }
        catch (NoSuchUserException ex)
        {
            return NotFound(ex.Message);
        }
        catch (NoSuchSessionException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidPromptException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] OpenAI API failed: {ex.Message}");
            return StatusCode(500, ex.Message);
        }

    }
}




