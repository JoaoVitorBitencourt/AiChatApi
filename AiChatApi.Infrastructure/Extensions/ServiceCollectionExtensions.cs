using AiChatApi.Domain.Interfaces;
using AiChatApi.Infrastructure.Data;
using AiChatApi.Infrastructure.Repositories;
using AiChatApi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiChatApi.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ChatDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Add repositories
        services.AddScoped<IChatRepository, ChatRepository>();

        // Add Ollama service
        var ollamaBaseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        services.AddSingleton<IOllamaService>(provider => new OllamaService(ollamaBaseUrl));

        // Add PDF service
        services.AddSingleton<IPdfService, PdfService>();

        return services;
    }
}
