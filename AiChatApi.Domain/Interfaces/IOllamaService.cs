using AiChatApi.Domain.Entities;

namespace AiChatApi.Domain.Interfaces;

public interface IOllamaService
{
    Task<string> GenerateResponseAsync(string prompt, string model = "llama3.2");
    Task<string> GenerateResponseAsync(IEnumerable<ChatMessage> conversationHistory, string model = "llama3.2");
    Task StreamResponseAsync(IEnumerable<ChatMessage> conversationHistory, Stream outputStream, string model = "llama3.2");
}
