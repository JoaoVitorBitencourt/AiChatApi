namespace AiChatApi.Domain.Interfaces;

public interface IPdfService
{
    Task<IDictionary<int, string>> ExtractPagesAsync(Stream pdfStream, CancellationToken cancellationToken = default);
    Task<Stream> CreatePdfAsync(IDictionary<int, string> pagesByNumber, CancellationToken cancellationToken = default);
}


