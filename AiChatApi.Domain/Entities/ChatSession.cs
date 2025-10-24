namespace AiChatApi.Domain.Entities;

public class ChatSession
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ChatMessage> Messages { get; set; } = [];
}
