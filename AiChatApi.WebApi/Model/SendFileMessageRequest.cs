namespace AiChatApi.WebApi.Model;

public class SendFileMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}
