using AiChatApi.Domain.Entities;

namespace AiChatApi.Domain.Interfaces;

public interface IChatRepository
{
    Task<ChatSession?> GetSessionByIdAsync(Guid sessionId);
    Task<IEnumerable<ChatSession>> GetAllSessionsAsync();
    Task<ChatSession> CreateSessionAsync(ChatSession session);
    Task<ChatSession> UpdateSessionAsync(ChatSession session);
    Task DeleteSessionAsync(Guid sessionId);
    
    Task<ChatMessage> AddMessageAsync(ChatMessage message);
    Task<IEnumerable<ChatMessage>> GetMessagesBySessionIdAsync(Guid sessionId);
    Task<ChatMessage?> GetMessageByIdAsync(Guid messageId);
    Task DeleteMessageAsync(Guid messageId);
}
