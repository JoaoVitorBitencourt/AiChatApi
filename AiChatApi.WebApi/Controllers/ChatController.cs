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