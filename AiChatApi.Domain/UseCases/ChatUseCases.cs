using AiChatApi.Domain.Entities;
using AiChatApi.Domain.Interfaces;

namespace AiChatApi.Domain.UseCases;

public class ChatUseCases
{
    private readonly IChatRepository _chatRepository;
    private readonly IOllamaService _ollamaService;

    public ChatUseCases(IChatRepository chatRepository, IOllamaService ollamaService)
    {
        _chatRepository = chatRepository;
        _ollamaService = ollamaService;
    }

    public async Task<ChatSession> CreateNewSessionAsync(string title)
    {
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            Title = title,
            CreatedAt = DateTime.UtcNow
        };

        return await _chatRepository.CreateSessionAsync(session);
    }

    public async Task<ChatMessage> SendMessageAsync(Guid sessionId, string content, string role = "user")
    {
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            Content = content,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            ChatSessionId = sessionId
        };

        await _chatRepository.AddMessageAsync(message);

        // If it's a user message, generate AI response
        if (role != "user") return message;
        
        var conversationHistory = await _chatRepository.GetMessagesBySessionIdAsync(sessionId);
        var aiResponse = await _ollamaService.GenerateResponseAsync(conversationHistory, "assistant");
            
        var aiMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            Content = aiResponse,
            Role = "assistant",
            CreatedAt = DateTime.UtcNow,
            ChatSessionId = sessionId
        };

        await _chatRepository.AddMessageAsync(aiMessage);

        return message;
    }

    public async Task<IEnumerable<ChatMessage>> GetSessionMessagesAsync(Guid sessionId)
    {
        return await _chatRepository.GetMessagesBySessionIdAsync(sessionId);
    }

    public async Task<IEnumerable<ChatSession>> GetAllSessionsAsync()
    {
        return await _chatRepository.GetAllSessionsAsync();
    }

    public async Task<ChatSession?> GetSessionByIdAsync(Guid sessionId)
    {
        return await _chatRepository.GetSessionByIdAsync(sessionId);
    }

    public async Task DeleteSessionAsync(Guid sessionId)
    {
        await _chatRepository.DeleteSessionAsync(sessionId);
    }

    public async Task StreamMessageAsync(Guid sessionId, string content, Stream outputStream)
    {
        // Save user message
        var userMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            Content = content,
            Role = "user",
            CreatedAt = DateTime.UtcNow,
            ChatSessionId = sessionId
        };

        await _chatRepository.AddMessageAsync(userMessage);

        // Get conversation history
        var conversationHistory = await _chatRepository.GetMessagesBySessionIdAsync(sessionId);

        // Stream AI response
        await _ollamaService.StreamResponseAsync(conversationHistory, outputStream);
    }
}
