using System.Runtime.CompilerServices;
using OllamaNET.Models;

namespace OllamaNET;

public interface IOllamaClient
{
    Task<OllamaChatResponse> AskAsync(string message, string? model = null, OllamaChatOptions? chatOptions = null, bool addToConversationHistory = true, CancellationToken cancellationToken = default)
        => AskAsync(Guid.CreateVersion7(), message, model, chatOptions, addToConversationHistory, cancellationToken);

    Task<OllamaChatResponse> AskAsync(Guid conversationId, string message, string? model = null, OllamaChatOptions? chatOptions = null, bool addToConversationHistory = true, CancellationToken cancellationToken = default);

    IAsyncEnumerable<OllamaChatResponse> AskStreamingAsync(string message, string? model = null, OllamaChatOptions? chatOptions = null, bool addToConversationHistory = true, CancellationToken cancellationToken = default)
        => AskStreamingAsync(Guid.CreateVersion7(), message, model, chatOptions, addToConversationHistory, cancellationToken);

    IAsyncEnumerable<OllamaChatResponse> AskStreamingAsync(Guid conversationId, string message, string? model = null, OllamaChatOptions? chatOptions = null, bool addToConversationHistory = true, CancellationToken cancellationToken = default);

    Task<Guid> SetupAsync(string message, CancellationToken cancellationToken = default)
        => SetupAsync(Guid.CreateVersion7(), message, cancellationToken);

    Task<Guid> SetupAsync(Guid conversationId, string message, CancellationToken cancellationToken = default);

    Task<IEnumerable<OllamaChatMessage>> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task<bool> ConversationExistsAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task DeleteConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
        => DeleteConversationAsync(conversationId, false, cancellationToken);

    Task DeleteConversationAsync(Guid conversationId, bool preserveSetup = false, CancellationToken cancellationToken = default);
}