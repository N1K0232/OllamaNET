using OllamaNET.Models;

namespace OllamaNET;

public interface IOllamaClient
{
    Task<OllamaChatResponse> AskAsync(string message, IEnumerable<Stream>? imagesStream = null, string? model = null, OllamaChatOptions? chatOptions = null, bool addToConversationHistory = true, CancellationToken cancellationToken = default)
        => AskAsync(Guid.CreateVersion7(), message, imagesStream, model, chatOptions, addToConversationHistory, cancellationToken);

    Task<OllamaChatResponse> AskAsync(Guid conversationId, string message, IEnumerable<Stream>? imagesStream = null, string? model = null, OllamaChatOptions? chatOptions = null, bool addToConversationHistory = true, CancellationToken cancellationToken = default);

    IAsyncEnumerable<OllamaChatResponse> AskStreamingAsync(string message, IEnumerable<Stream>? imagesStreams = null, string? model = null, OllamaChatOptions? chatOptions = null, bool addToConversationHistory = true, CancellationToken cancellationToken = default)
        => AskStreamingAsync(Guid.CreateVersion7(), message, imagesStreams, model, chatOptions, addToConversationHistory, cancellationToken);

    IAsyncEnumerable<OllamaChatResponse> AskStreamingAsync(Guid conversationId, string message, IEnumerable<Stream>? imagesStreams = null, string? model = null, OllamaChatOptions? chatOptions = null, bool addToConversationHistory = true, CancellationToken cancellationToken = default);

    Task<Guid> SetupAsync(string message, CancellationToken cancellationToken = default)
        => SetupAsync(Guid.CreateVersion7(), message, cancellationToken);

    Task<Guid> SetupAsync(Guid conversationId, string message, CancellationToken cancellationToken = default);

    Task<IEnumerable<OllamaChatMessage>> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task<bool> ConversationExistsAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task DeleteConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
        => DeleteConversationAsync(conversationId, false, cancellationToken);

    Task DeleteConversationAsync(Guid conversationId, bool preserveSetup = false, CancellationToken cancellationToken = default);

    Task AddInteractionAsync(Guid conversationId, string question, string answer, CancellationToken cancellationToken = default);

    Task<OllamaEmbeddingResponse> CreateEmbeddingAsync(string content, string? model = null, CancellationToken cancellationToken = default);
}