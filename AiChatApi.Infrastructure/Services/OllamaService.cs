using AiChatApi.Domain.Entities;
using AiChatApi.Domain.Interfaces;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System.Text;
using OllamaSharp.Models;

namespace AiChatApi.Infrastructure.Services;

public class OllamaService : IOllamaService
{
    private readonly OllamaApiClient _ollamaClient;

    public OllamaService(string baseUrl = "http://localhost:11434")
    {
        _ollamaClient = new OllamaApiClient(baseUrl);
    }

    public async Task<string> GenerateResponseAsync(string prompt, string model = "llama3.2")
    {
        var chat = _ollamaClient.ChatAsync(new ChatRequest()
        {
            Model = model,
            Messages = new List<Message>
            {
                new()
                {
                    Content = prompt
                }
            }
        });

        var response = string.Empty;
        await foreach (var stream in chat)
        {
            response = stream?.Message.Content;
        }

        return response ?? string.Empty;
    }

    public async Task<string> GenerateResponseAsync(IEnumerable<ChatMessage> conversationHistory, string model = "llama3.2")
    {
        var messages = conversationHistory.Select(msg => new Message
        {
            Role = msg.Role,
            Content = msg.Content
        }).ToList();

        var chat = _ollamaClient.GenerateAsync(new GenerateRequest()
        {
            Model = model,
            Prompt = messages.Select(x => x.Content).Last() ?? ""
        });
        
        var builder = new StringBuilder();
        await foreach (var stream in chat)
        {
            builder.Append(stream?.Response ?? string.Empty);
        }
        var response = builder.ToString();

        return response;
    }

    public async Task StreamResponseAsync(IEnumerable<ChatMessage> conversationHistory, Stream outputStream, string model = "llama3.2")
    {
        var messages = conversationHistory.Select(msg => new Message
        {
            Role = msg.Role,
            Content = msg.Content
        }).ToList();

        var chat = _ollamaClient.ChatAsync(new ChatRequest()
        {
            Messages = messages,
            Model = model
        });

        await foreach (var stream in chat)
        {
            if (stream?.Message?.Content != null)
            {
                var content = stream.Message.Content;
                var bytes = Encoding.UTF8.GetBytes(content);
                await outputStream.WriteAsync(bytes, 0, bytes.Length);
                await outputStream.FlushAsync();
            }
        }
    }
}
