using Microsoft.Extensions.Caching.Memory;
using OllamaNET.Models;

namespace OllamaNET.Caching;

internal class OllamaMemoryCache(IMemoryCache cache) : IOllamaCache
{
    public Task RemoveAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        cache.Remove(conversationId);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<OllamaChatMessage>?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = cache.Get<IEnumerable<OllamaChatMessage>>(conversationId);
        return Task.FromResult(conversation);
    }

    public Task<bool> ExistsAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var exists = cache.TryGetValue(conversationId, out _);
        return Task.FromResult(exists);
    }

    public Task SetAsync(Guid conversationId, IEnumerable<OllamaChatMessage> messages, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        cache.Set(conversationId, messages, expiration);
        return Task.CompletedTask;
    }
}