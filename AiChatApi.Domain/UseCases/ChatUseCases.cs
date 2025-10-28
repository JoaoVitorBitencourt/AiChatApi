using AiChatApi.Domain.Entities;
using AiChatApi.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text;
using UglyToad.PdfPig;

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
        var aiResponse = await _ollamaService.GenerateResponseAsync(conversationHistory);
            
        var aiMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            Content = aiResponse,
            Role = "assistant",
            CreatedAt = DateTime.UtcNow,
            ChatSessionId = sessionId
        };

        await _chatRepository.AddMessageAsync(aiMessage);

        return aiMessage;
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

    public async Task<ChatMessage> SendFileMessageAsync(Guid sessionId, string message, IFormFile file)
    {
        // Extract text content from file
        var fileContent = await ExtractTextFromFileAsync(file);
        
        // Combine user message with file content
        var combinedContent = $"{message}\n\nFile: {file.FileName}\nContent:\n{fileContent}";

        // Save user message with file content
        var userMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            Content = combinedContent,
            Role = "user",
            CreatedAt = DateTime.UtcNow,
            ChatSessionId = sessionId
        };

        await _chatRepository.AddMessageAsync(userMessage);

        // Get conversation history
        var conversationHistory = await _chatRepository.GetMessagesBySessionIdAsync(sessionId);

        // Generate AI response
        var aiResponse = await _ollamaService.GenerateResponseAsync(conversationHistory);
            
        var aiMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            Content = aiResponse,
            Role = "assistant",
            CreatedAt = DateTime.UtcNow,
            ChatSessionId = sessionId
        };

        await _chatRepository.AddMessageAsync(aiMessage);

        return aiMessage;
    }

    public async Task StreamFileMessageAsync(Guid sessionId, string message, IFormFile file, Stream outputStream)
    {
        // Extract text content from file
        var fileContent = await ExtractTextFromFileAsync(file);
        
        // Combine user message with file content
        var combinedContent = $"{message}\n\nFile: {file.FileName}\nContent:\n{fileContent}";

        // Save user message with file content
        var userMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            Content = combinedContent,
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

    private async Task<string> ExtractTextFromFileAsync(IFormFile file)
    {
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        await using var stream = file.OpenReadStream();
        
        return fileExtension switch
        {
            ".txt" => await ExtractTextFromTxtAsync(stream),
            ".pdf" => await ExtractTextFromPdfAsync(stream),
            ".docx" => await ExtractTextFromDocxAsync(stream),
            ".doc" => await ExtractTextFromDocAsync(stream),
            _ => throw new NotSupportedException($"File type {fileExtension} is not supported for text extraction")
        };
    }

    private async Task<string> ExtractTextFromTxtAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private async Task<string> ExtractTextFromPdfAsync(Stream stream)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            try
            {
                using var document = PdfDocument.Open(stream);
                foreach (var page in document.GetPages())
                {
                    if (!string.IsNullOrWhiteSpace(page.Text))
                    {
                        sb.AppendLine(page.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[PDF extraction failed: {ex.Message}]");
            }

            return sb.ToString();
        });
    }

    private async Task<string> ExtractTextFromDocxAsync(Stream stream)
    {
        // For DOCX extraction, you would typically use DocumentFormat.OpenXml
        // This is a placeholder implementation - you'll need to add the appropriate NuGet package
        // For now, return a placeholder message
        return "[DOCX content extraction not implemented - requires OpenXML library]";
    }

    private async Task<string> ExtractTextFromDocAsync(Stream stream)
    {
        // For DOC extraction, you would typically use a library like Aspose.Words or similar
        // This is a placeholder implementation - you'll need to add the appropriate NuGet package
        // For now, return a placeholder message
        return "[DOC content extraction not implemented - requires DOC library]";
    }
}
