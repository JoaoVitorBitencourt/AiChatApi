using AiChatApi.Domain.UseCases;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using AiChatApi.WebApi.Model;

namespace AiChatApi.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatUseCases _chatUseCases;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ChatUseCases chatUseCases, ILogger<ChatController> logger)
    {
        _chatUseCases = chatUseCases;
        _logger = logger;
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            var session = await _chatUseCases.CreateNewSessionAsync(request.Title);
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat session");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        try
        {
            var sessions = await _chatUseCases.GetAllSessionsAsync();
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat sessions");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("sessions/{sessionId:guid}")]
    public async Task<IActionResult> GetSession(Guid sessionId)
    {
        try
        {
            var session = await _chatUseCases.GetSessionByIdAsync(sessionId);
            if (session == null)
                return NotFound();

            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("sessions/{sessionId:guid}/messages")]
    public async Task<IActionResult> GetSessionMessages(Guid sessionId)
    {
        try
        {
            var messages = await _chatUseCases.GetSessionMessagesAsync(sessionId);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages for session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("sessions/{sessionId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid sessionId, [FromBody] SendMessageRequest request)
    {
        try
        {
            var message = await _chatUseCases.SendMessageAsync(sessionId, request.Content);
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("sessions/{sessionId:guid}/stream")]
    public async Task StreamMessage(Guid sessionId, [FromBody] SendMessageRequest request)
    {
        try
        {
            Response.Headers.Append("Content-Type", "text/plain; charset=utf-8");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");
            
            // Stream AI response
            await _chatUseCases.StreamMessageAsync(sessionId, request.Content, Response.Body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming message to session {SessionId}", sessionId);
            if (!Response.HasStarted)
            {
                Response.StatusCode = 500;
            }
        }
    }

    [HttpPost("sessions/{sessionId:guid}/files")]
    public async Task<IActionResult> SendFileMessage(Guid sessionId, [FromForm] SendFileMessageRequest request)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest("No file provided");
            }

            // Validate file type (you can extend this for more file types)
            var allowedExtensions = new[] { ".pdf", ".txt", ".docx", ".doc" };
            var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest($"File type {fileExtension} is not supported. Allowed types: {string.Join(", ", allowedExtensions)}");
            }

            // Validate file size (e.g., max 10MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (request.File.Length > maxFileSize)
            {
                return BadRequest($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB");
            }

            var message = await _chatUseCases.SendFileMessageAsync(sessionId, request.Message, request.File);
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending file message to session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("sessions/{sessionId:guid}/files/stream")]
    public async Task StreamFileMessage(Guid sessionId, [FromForm] SendFileMessageRequest request)
    {
        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("No file provided");
                return;
            }

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".txt", ".docx", ".doc" };
            var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                Response.StatusCode = 400;
                await Response.WriteAsync($"File type {fileExtension} is not supported. Allowed types: {string.Join(", ", allowedExtensions)}");
                return;
            }

            // Validate file size
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (request.File.Length > maxFileSize)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB");
                return;
            }

            Response.Headers.Append("Content-Type", "text/plain; charset=utf-8");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");
            
            // Stream AI response with file content
            await _chatUseCases.StreamFileMessageAsync(sessionId, request.Message, request.File, Response.Body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming file message to session {SessionId}", sessionId);
            if (!Response.HasStarted)
            {
                Response.StatusCode = 500;
            }
        }
    }

    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<IActionResult> DeleteSession(Guid sessionId)
    {
        try
        {
            await _chatUseCases.DeleteSessionAsync(sessionId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
            return StatusCode(500, "Internal server error");
        }
    }
}