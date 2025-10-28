using AiChatApi.Domain.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AiChatApi.Infrastructure.Services;

public class PdfService : IPdfService
{
    public async Task<IDictionary<int, string>> ExtractPagesAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pdfStream);
        if (!pdfStream.CanRead) throw new ArgumentException("Stream must be readable", nameof(pdfStream));

        // Ensure stream position at 0
        if (pdfStream.CanSeek)
        {
            pdfStream.Seek(0, SeekOrigin.Begin);
        }

        // PdfPig is synchronous; wrap in Task.Run to avoid blocking
        var result = await Task.Run(IDictionary<int, string> () =>
        {
            var pages = new Dictionary<int, string>();
            using var document = PdfDocument.Open(pdfStream);
            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var text = page.Text ?? string.Empty;
                pages[page.Number] = text;
            }
            return pages;
        }, cancellationToken);

        return result;
    }

    public async Task<Stream> CreatePdfAsync(IDictionary<int, string> pagesByNumber, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pagesByNumber);

        // QuestPDF document builder
        var memoryStream = new MemoryStream();

        var orderedPages = pagesByNumber
            .OrderBy(kv => kv.Key)
            .ToList();

        var doc = Document.Create(container =>
        {
            foreach (var (pageNumber, text) in orderedPages)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Text($"Page {pageNumber}").SemiBold().FontSize(14);
                        col.Item().Text(text ?? string.Empty);
                    });
                });
            }
        });

        await Task.Run(() => doc.GeneratePdf(memoryStream), cancellationToken);
        if (memoryStream.CanSeek) memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}


