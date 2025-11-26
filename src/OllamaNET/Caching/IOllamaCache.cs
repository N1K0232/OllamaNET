using OllamaNET.Models;

namespace OllamaNET.Caching;

public interface IOllamaCache
{
    Task RemoveAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task<IEnumerable<OllamaChatMessage>?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task SetAsync(Guid conversationId, IEnumerable<OllamaChatMessage> messages, TimeSpan expiration, CancellationToken cancellationToken = default);
}